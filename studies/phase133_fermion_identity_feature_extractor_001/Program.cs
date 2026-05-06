using System.Text.Json;

const string DefaultOutputDir = "studies/phase133_fermion_identity_feature_extractor_001/output";
const string Phase131Path = "studies/phase131_sector_label_candidate_coverage_repair_001/output/sector_label_candidate_coverage_repair.json";
const string Phase132Path = "studies/phase132_fermion_sector_label_derivation_source_gate_001/output/fermion_sector_label_derivation_source_gate.json";
const string Phase127Path = "studies/phase127_fermion_gauge_basis_content_observable_001/output/fermion_gauge_basis_content_observable.json";
const string Phase128Path = "studies/phase128_fermion_su2_generator_sector_observable_001/output/fermion_su2_generator_sector_observable.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE133_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase131 = JsonDocument.Parse(File.ReadAllText(Phase131Path));
using var phase132 = JsonDocument.Parse(File.ReadAllText(Phase132Path));
using var phase127 = JsonDocument.Parse(File.ReadAllText(Phase127Path));
using var phase128 = JsonDocument.Parse(File.ReadAllText(Phase128Path));

var labelRows = phase131.RootElement.GetProperty("labelRecords").EnumerateArray().ToList();
var gaugeRecords = phase127.RootElement.GetProperty("modeGaugeBasisRecords")
    .EnumerateArray()
    .ToDictionary(r => RequiredString(r, "familyId"), r => r.Clone(), StringComparer.Ordinal);
var generatorRecords = phase128.RootElement.GetProperty("modeGeneratorSectorRecords")
    .EnumerateArray()
    .ToDictionary(r => RequiredString(r, "familyId"), r => r.Clone(), StringComparer.Ordinal);

var featureRecords = labelRows
    .Select(row => BuildFeatureRecord(row, gaugeRecords, generatorRecords))
    .ToList();

bool allRowsHaveFeatureRecords = featureRecords.Count == labelRows.Count;
bool allRowsHaveDiagnosticInputs = featureRecords.All(r => r.HasGaugeBasisDiagnostic && r.HasGeneratorSectorDiagnostic);
bool allRowsHavePromotableSectorObservable = featureRecords.All(r => r.PromotableGaugeBasisSector || r.PromotableT3Sector);
bool allRowsHavePhysicalLabels = featureRecords.All(r => r.ChargeSector is not null && (r.WeakSector is not null || r.QuantumNumbers is not null));
bool featureExtractorPromotable = allRowsHaveFeatureRecords
    && allRowsHaveDiagnosticInputs
    && allRowsHavePromotableSectorObservable
    && allRowsHavePhysicalLabels;
string terminalStatus = featureExtractorPromotable
    ? "fermion-identity-feature-extractor-labels-ready"
    : "fermion-identity-feature-extractor-materialized-labels-blocked";

var blockers = new List<string>();
if (!allRowsHaveFeatureRecords)
    blockers.Add("not every P131 coverage-repaired row produced a fermion identity feature record");
if (!allRowsHaveDiagnosticInputs)
    blockers.Add("not every fermion identity feature record has both gauge-basis and generator-sector diagnostics");
if (!allRowsHavePromotableSectorObservable)
    blockers.Add("P127/P128 diagnostics remain mixed and do not provide promotable charge or weak-sector labels");
if (!allRowsHavePhysicalLabels)
    blockers.Add("fermion identity feature records do not contain explicit chargeSector and weak-sector/quantum-number labels");
if (JsonBool(phase132.RootElement, "derivationSourcePromotable") is not true)
    blockers.Add("Phase132 found no matching derivation source for existing charge-sector assignments");

var result = new
{
    phaseId = "phase133-fermion-identity-feature-extractor",
    terminalStatus,
    fermionIdentityFeaturesMaterialized = true,
    featureExtractorPromotable,
    allRowsHaveFeatureRecords,
    allRowsHaveDiagnosticInputs,
    allRowsHavePromotableSectorObservable,
    allRowsHavePhysicalLabels,
    featureRecords,
    featureSchema = new
    {
        electroweakMultipletId = "su2-adjoint-triplet:fermion-diagnostic-basis",
        diagnosticInputs = new[]
        {
            "P127 gauge-basis energy fractions",
            "P128 SU(2) generator moments and T3 eigenbasis fractions",
        },
        promotionRule = "physical labels may be assigned only when a diagnostic observable is promotable or a separate nontrivial chirality/conjugation transition table exists",
        externalTargetValuesUsed = false,
    },
    phase132Gate = new
    {
        terminalStatus = JsonString(phase132.RootElement, "terminalStatus"),
        derivationSourcePromotable = JsonBool(phase132.RootElement, "derivationSourcePromotable"),
    },
    blockers,
    closureRequirements = new[]
    {
        "derive a promotable fermion sector observable or nontrivial chirality/conjugation transition table for the P131 rows",
        "assign chargeSector and weakSector/quantumNumbers only from that target-blind derivation",
        "rerun the derivation-source gate and sector-label table gate after labels are assigned",
    },
    sourceEvidence = new
    {
        phase131Path = Phase131Path,
        phase132Path = Phase132Path,
        phase127Path = Phase127Path,
        phase128Path = Phase128Path,
    },
};

var options = new JsonSerializerOptions
{
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
};
File.WriteAllText(
    Path.Combine(outputDir, "fermion_identity_feature_extractor.json"),
    JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "fermion_identity_feature_extractor_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.fermionIdentityFeaturesMaterialized,
        result.featureExtractorPromotable,
        result.allRowsHaveFeatureRecords,
        result.allRowsHaveDiagnosticInputs,
        result.allRowsHavePromotableSectorObservable,
        result.allRowsHavePhysicalLabels,
        featureRecords = featureRecords.Select(r => new
        {
            r.ModeIndex,
            r.FamilyId,
            r.CandidateId,
            r.FeatureStatus,
            r.DiagnosticDominantGaugeBasisIndex,
            r.DiagnosticDominantGaugeBasisFraction,
            r.GeneratorPolarizationMagnitude,
            r.DominantT3Fraction,
            r.ChargeSector,
            r.WeakSector,
            r.Blockers,
        }),
        result.blockers,
        result.closureRequirements,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"featureRecordCount={featureRecords.Count}");
Console.WriteLine($"allRowsHaveDiagnosticInputs={allRowsHaveDiagnosticInputs}");
Console.WriteLine($"featureExtractorPromotable={featureExtractorPromotable}");

static FermionIdentityFeatureRecord BuildFeatureRecord(
    JsonElement row,
    IReadOnlyDictionary<string, JsonElement> gaugeRecords,
    IReadOnlyDictionary<string, JsonElement> generatorRecords)
{
    string familyId = RequiredString(row, "familyId");
    gaugeRecords.TryGetValue(familyId, out var gauge);
    generatorRecords.TryGetValue(familyId, out var generator);

    bool hasGauge = gauge.ValueKind != JsonValueKind.Undefined;
    bool hasGenerator = generator.ValueKind != JsonValueKind.Undefined;
    bool promotableGauge = hasGauge && JsonBool(gauge, "promotableGaugeBasisSector") is true;
    bool promotableT3 = hasGenerator && JsonBool(generator, "promotableT3Sector") is true;
    var blockers = new List<string>();
    if (!hasGauge)
        blockers.Add("missing P127 gauge-basis diagnostic");
    if (!hasGenerator)
        blockers.Add("missing P128 generator-sector diagnostic");
    if (!promotableGauge && !promotableT3)
        blockers.Add("available target-blind sector diagnostics are mixed and non-promotable");
    blockers.Add("physical charge/weak-sector labels are unassigned");

    return new FermionIdentityFeatureRecord(
        FeatureId: $"phase133-feature:{familyId}",
        FamilyId: familyId,
        CandidateId: RequiredString(row, "candidateId"),
        SourceCanonicalFermionModeId: JsonString(row, "sourceCanonicalFermionModeId"),
        ModeId: RequiredString(row, "modeId"),
        ModeIndex: RequiredInt(row, "modeIndex"),
        ElectroweakMultipletId: "su2-adjoint-triplet:fermion-diagnostic-basis",
        HasGaugeBasisDiagnostic: hasGauge,
        DiagnosticDominantGaugeBasisIndex: hasGauge ? JsonInt(gauge, "dominantGaugeBasisIndex") : null,
        DiagnosticDominantGaugeBasisFraction: hasGauge ? JsonDouble(gauge, "dominantGaugeBasisFraction") : null,
        GaugeBasisDominanceGap: hasGauge ? JsonDouble(gauge, "dominanceGap") : null,
        GaugeBasisEntropyNormalized: hasGauge ? JsonDouble(gauge, "gaugeBasisEntropyNormalized") : null,
        PromotableGaugeBasisSector: promotableGauge,
        HasGeneratorSectorDiagnostic: hasGenerator,
        GeneratorExpectation: hasGenerator ? DoubleArray(generator, "generatorExpectation") : [],
        GeneratorPolarizationMagnitude: hasGenerator ? JsonDouble(generator, "generatorPolarizationMagnitude") : null,
        T3EigenbasisFractions: hasGenerator ? DoubleArray(generator, "t3EigenbasisFractions") : [],
        DominantT3Eigenvalue: hasGenerator ? JsonInt(generator, "dominantT3Eigenvalue") : null,
        DominantT3Fraction: hasGenerator ? JsonDouble(generator, "dominantT3Fraction") : null,
        T3DominanceGap: hasGenerator ? JsonDouble(generator, "t3DominanceGap") : null,
        PromotableT3Sector: promotableT3,
        ChargeSector: null,
        WeakSector: null,
        QuantumNumbers: null,
        DerivationId: "phase133-diagnostic-feature-extractor:no-physical-label-assignment",
        ExternalTargetValuesUsed: false,
        FeatureStatus: "partial",
        Blockers: blockers);
}

static string RequiredString(JsonElement element, string propertyName)
{
    return JsonString(element, propertyName)
        ?? throw new InvalidDataException($"Missing string property '{propertyName}'.");
}

static int RequiredInt(JsonElement element, string propertyName)
{
    return JsonInt(element, propertyName)
        ?? throw new InvalidDataException($"Missing integer property '{propertyName}'.");
}

static string? JsonString(JsonElement element, string propertyName)
{
    return element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
        ? property.GetString()
        : null;
}

static double? JsonDouble(JsonElement element, string propertyName)
{
    return element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var value)
        ? value
        : null;
}

static int? JsonInt(JsonElement element, string propertyName)
{
    return element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value)
        ? value
        : null;
}

static bool? JsonBool(JsonElement element, string propertyName)
{
    return element.TryGetProperty(propertyName, out var property)
        ? property.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            _ => null,
        }
        : null;
}

static IReadOnlyList<double> DoubleArray(JsonElement element, string propertyName)
{
    if (!element.TryGetProperty(propertyName, out var property) || property.ValueKind != JsonValueKind.Array)
        return [];

    return property.EnumerateArray()
        .Where(item => item.ValueKind == JsonValueKind.Number)
        .Select(item => item.GetDouble())
        .ToArray();
}

sealed record FermionIdentityFeatureRecord(
    string FeatureId,
    string FamilyId,
    string CandidateId,
    string? SourceCanonicalFermionModeId,
    string ModeId,
    int ModeIndex,
    string ElectroweakMultipletId,
    bool HasGaugeBasisDiagnostic,
    int? DiagnosticDominantGaugeBasisIndex,
    double? DiagnosticDominantGaugeBasisFraction,
    double? GaugeBasisDominanceGap,
    double? GaugeBasisEntropyNormalized,
    bool PromotableGaugeBasisSector,
    bool HasGeneratorSectorDiagnostic,
    IReadOnlyList<double> GeneratorExpectation,
    double? GeneratorPolarizationMagnitude,
    IReadOnlyList<double> T3EigenbasisFractions,
    int? DominantT3Eigenvalue,
    double? DominantT3Fraction,
    double? T3DominanceGap,
    bool PromotableT3Sector,
    string? ChargeSector,
    string? WeakSector,
    object? QuantumNumbers,
    string DerivationId,
    bool ExternalTargetValuesUsed,
    string FeatureStatus,
    IReadOnlyList<string> Blockers);
