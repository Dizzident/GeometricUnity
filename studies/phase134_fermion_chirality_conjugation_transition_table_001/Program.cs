using System.Text.Json;

const string DefaultOutputDir = "studies/phase134_fermion_chirality_conjugation_transition_table_001/output";
const string Phase133Path = "studies/phase133_fermion_identity_feature_extractor_001/output/fermion_identity_feature_extractor.json";
const string Phase12ChiralityPath = "studies/phase12_joined_calculation_001/output/background_family/fermions/chirality_analysis_bg-phase12-bg-a-20260315212202.json";
const string Phase12ConjugationPath = "studies/phase12_joined_calculation_001/output/background_family/fermions/conjugation_pairs_bg-phase12-bg-a-20260315212202.json";
const string SpinorRepresentationPath = "studies/phase12_joined_calculation_001/output/background_family/fermions/spinor_representation.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE134_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase133 = JsonDocument.Parse(File.ReadAllText(Phase133Path));
using var chirality = JsonDocument.Parse(File.ReadAllText(Phase12ChiralityPath));
using var conjugation = JsonDocument.Parse(File.ReadAllText(Phase12ConjugationPath));
using var spinor = JsonDocument.Parse(File.ReadAllText(SpinorRepresentationPath));

var featureRows = phase133.RootElement.GetProperty("featureRecords").EnumerateArray().ToList();
var chiralityByMode = chirality.RootElement.EnumerateArray()
    .ToDictionary(r => RequiredString(r, "modeId"), r => r.Clone(), StringComparer.Ordinal);
var conjugationPairs = conjugation.RootElement.ValueKind == JsonValueKind.Array
    ? conjugation.RootElement.EnumerateArray().Select(e => e.Clone()).ToList()
    : [];

bool spinorHasChirality = JsonBool(spinor.RootElement.GetProperty("chiralityConvention"), "hasChirality") is true;
string chiralityPhaseFactor = JsonString(spinor.RootElement.GetProperty("chiralityConvention"), "phaseFactor") ?? "unknown";
bool chargeConjugationMatrixAvailable = JsonString(spinor.RootElement.GetProperty("conjugationConvention"), "chargeConjugationMatrixId") is not null;

var rowChirality = featureRows.Select(row =>
{
    string sourceModeId = RequiredString(row, "sourceCanonicalFermionModeId");
    chiralityByMode.TryGetValue(sourceModeId, out var c);
    return new RowChiralityRecord(
        FamilyId: RequiredString(row, "familyId"),
        CandidateId: RequiredString(row, "candidateId"),
        SourceCanonicalFermionModeId: sourceModeId,
        ModeIndex: RequiredInt(row, "modeIndex"),
        ChiralityStatus: c.ValueKind == JsonValueKind.Undefined ? "missing" : JsonString(c, "chiralityStatus") ?? "missing",
        ChiralityTag: c.ValueKind == JsonValueKind.Undefined ? "missing" : JsonString(c, "chiralityTag") ?? "missing",
        LeftFraction: c.ValueKind == JsonValueKind.Undefined ? null : JsonDouble(c, "leftFraction"),
        RightFraction: c.ValueKind == JsonValueKind.Undefined ? null : JsonDouble(c, "rightFraction"),
        DiagnosticNotes: c.ValueKind == JsonValueKind.Undefined ? [] : StringArray(c, "diagnosticNotes"));
}).ToList();

var transitions = (
    from fromRow in rowChirality
    from toRow in rowChirality
    select BuildTransition(fromRow, toRow, conjugationPairs)).ToList();

bool allRowsHaveChirality = rowChirality.All(r => r.ChiralityStatus != "missing");
bool anyNontrivialChirality = rowChirality.Any(r => r.ChiralityStatus is not "trivial" and not "missing");
bool anyConjugationPair = conjugationPairs.Count > 0;
bool anyPromotableTransition = transitions.Any(t => t.PromotableTransition);
bool transitionTablePromotable = spinorHasChirality && anyNontrivialChirality && anyConjugationPair && anyPromotableTransition;
string terminalStatus = transitionTablePromotable
    ? "fermion-chirality-conjugation-transition-table-ready"
    : "fermion-chirality-conjugation-transition-table-blocked";

var blockers = new List<string>();
if (!spinorHasChirality)
    blockers.Add("Phase12 spinor representation has no full Y-chirality operator");
if (string.Equals(chiralityPhaseFactor, "trivial", StringComparison.Ordinal))
    blockers.Add("Phase12 chirality convention is trivial");
if (!allRowsHaveChirality)
    blockers.Add("not every P133 feature row has a matching Phase12 chirality record");
if (!anyNontrivialChirality)
    blockers.Add("all matched repaired rows have trivial chirality");
if (!anyConjugationPair)
    blockers.Add("no Phase12 conjugation pairs exist for the repaired background");
if (!chargeConjugationMatrixAvailable)
    blockers.Add("spinor representation does not provide a charge-conjugation matrix id");

var result = new
{
    phaseId = "phase134-fermion-chirality-conjugation-transition-table",
    terminalStatus,
    transitionTableMaterialized = true,
    transitionTablePromotable,
    spinorHasChirality,
    chiralityPhaseFactor,
    chargeConjugationMatrixAvailable,
    anyNontrivialChirality,
    anyConjugationPair,
    anyPromotableTransition,
    rowChirality,
    transitions,
    phase133Gate = new
    {
        terminalStatus = JsonString(phase133.RootElement, "terminalStatus"),
        featureExtractorPromotable = JsonBool(phase133.RootElement, "featureExtractorPromotable"),
    },
    blockers,
    closureRequirements = new[]
    {
        "provide a nontrivial chirality operator or transition observable for the repaired fermion rows",
        "materialize conjugation-pair evidence or an equivalent charged-current transition pairing",
        "rerun sector-label feature extraction only after a nontrivial transition table is available",
    },
    sourceEvidence = new
    {
        phase133Path = Phase133Path,
        phase12ChiralityPath = Phase12ChiralityPath,
        phase12ConjugationPath = Phase12ConjugationPath,
        spinorRepresentationPath = SpinorRepresentationPath,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "fermion_chirality_conjugation_transition_table.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(Path.Combine(outputDir, "fermion_chirality_conjugation_transition_table_summary.json"), JsonSerializer.Serialize(new
{
    result.phaseId,
    result.terminalStatus,
    result.transitionTableMaterialized,
    result.transitionTablePromotable,
    result.spinorHasChirality,
    result.anyNontrivialChirality,
    result.anyConjugationPair,
    transitionCount = transitions.Count,
    promotableTransitionCount = transitions.Count(t => t.PromotableTransition),
    result.rowChirality,
    result.blockers,
    result.closureRequirements,
}, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"transitionCount={transitions.Count}");
Console.WriteLine($"promotableTransitionCount={transitions.Count(t => t.PromotableTransition)}");
Console.WriteLine($"transitionTablePromotable={transitionTablePromotable}");

static TransitionRecord BuildTransition(RowChiralityRecord fromRow, RowChiralityRecord toRow, IReadOnlyList<JsonElement> conjugationPairs)
{
    bool hasConjugationPair = conjugationPairs.Any(pair =>
        (JsonString(pair, "modeId") == fromRow.SourceCanonicalFermionModeId && JsonString(pair, "conjugateModeId") == toRow.SourceCanonicalFermionModeId) ||
        (JsonString(pair, "modeId") == toRow.SourceCanonicalFermionModeId && JsonString(pair, "conjugateModeId") == fromRow.SourceCanonicalFermionModeId));
    bool nontrivialChirality = fromRow.ChiralityStatus != "trivial" && toRow.ChiralityStatus != "trivial";
    bool promotable = hasConjugationPair && nontrivialChirality;
    return new TransitionRecord(
        FromFamilyId: fromRow.FamilyId,
        ToFamilyId: toRow.FamilyId,
        FromModeIndex: fromRow.ModeIndex,
        ToModeIndex: toRow.ModeIndex,
        FromChiralityStatus: fromRow.ChiralityStatus,
        ToChiralityStatus: toRow.ChiralityStatus,
        HasConjugationPair: hasConjugationPair,
        TransitionKind: promotable ? "charged-current-candidate" : "unassigned",
        PromotableTransition: promotable,
        Blocker: promotable ? null : "transition lacks nontrivial chirality and conjugation-pair evidence");
}

static string RequiredString(JsonElement element, string propertyName) =>
    JsonString(element, propertyName) ?? throw new InvalidDataException($"Missing string property '{propertyName}'.");
static int RequiredInt(JsonElement element, string propertyName) =>
    JsonInt(element, propertyName) ?? throw new InvalidDataException($"Missing integer property '{propertyName}'.");
static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;
static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;
static double? JsonDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var value) ? value : null;
static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;
static IReadOnlyList<string> StringArray(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Array
        ? property.EnumerateArray().Where(item => item.ValueKind == JsonValueKind.String).Select(item => item.GetString()!).ToArray()
        : [];

sealed record RowChiralityRecord(
    string FamilyId,
    string CandidateId,
    string SourceCanonicalFermionModeId,
    int ModeIndex,
    string ChiralityStatus,
    string ChiralityTag,
    double? LeftFraction,
    double? RightFraction,
    IReadOnlyList<string> DiagnosticNotes);

sealed record TransitionRecord(
    string FromFamilyId,
    string ToFamilyId,
    int FromModeIndex,
    int ToModeIndex,
    string FromChiralityStatus,
    string ToChiralityStatus,
    bool HasConjugationPair,
    string TransitionKind,
    bool PromotableTransition,
    string? Blocker);
