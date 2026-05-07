using System.Text.Json;

const string DefaultOutputDir = "studies/phase138_repaired_row_coupling_transition_graph_001/output";
const string Phase131Path = "studies/phase131_sector_label_candidate_coverage_repair_001/output/sector_label_candidate_coverage_repair.json";
const string Phase137Path = "studies/phase137_base_chirality_route_audit_001/output/base_chirality_route_audit.json";
const string CouplingAtlasPath = "studies/phase12_joined_calculation_001/output/background_family/fermions/couplings/coupling_atlas_bg-phase12-bg-a-20260315212202.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE138_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase131 = JsonDocument.Parse(File.ReadAllText(Phase131Path));
using var phase137 = JsonDocument.Parse(File.ReadAllText(Phase137Path));
using var couplingAtlas = JsonDocument.Parse(File.ReadAllText(CouplingAtlasPath));

var labelRows = phase131.RootElement.GetProperty("labelRecords").EnumerateArray()
    .Select(row => new LabelRow(
        FamilyId: RequiredString(row, "familyId"),
        CandidateId: RequiredString(row, "candidateId"),
        SourceCanonicalFermionModeId: RequiredString(row, "sourceCanonicalFermionModeId"),
        ChargeSector: JsonString(row, "chargeSector"),
        WeakSector: JsonString(row, "weakSector"),
        HasQuantumNumbers: row.TryGetProperty("quantumNumbers", out var qn) && qn.ValueKind is not JsonValueKind.Null and not JsonValueKind.Undefined))
    .ToArray();
var modeIds = labelRows.Select(r => r.SourceCanonicalFermionModeId).ToHashSet(StringComparer.Ordinal);

var couplings = couplingAtlas.RootElement.GetProperty("couplings").EnumerateArray()
    .Where(c => modeIds.Contains(RequiredString(c, "fermionModeIdI"))
        && modeIds.Contains(RequiredString(c, "fermionModeIdJ")))
    .Select(c => new CouplingRecord(
        BosonModeId: RequiredString(c, "bosonModeId"),
        FermionModeIdI: RequiredString(c, "fermionModeIdI"),
        FermionModeIdJ: RequiredString(c, "fermionModeIdJ"),
        Real: RequiredDouble(c, "couplingProxyReal"),
        Imaginary: RequiredDouble(c, "couplingProxyImaginary"),
        Magnitude: RequiredDouble(c, "couplingProxyMagnitude"),
        VariationEvidenceId: JsonString(c, "variationEvidenceId")))
    .ToArray();

var familyByMode = labelRows.ToDictionary(r => r.SourceCanonicalFermionModeId, r => r.FamilyId, StringComparer.Ordinal);
var transitionRecords = couplings
    .GroupBy(c => c.BosonModeId, StringComparer.Ordinal)
    .Select(group => BuildTransitionRecord(group.Key, group.ToArray(), familyByMode))
    .OrderByDescending(r => r.MaxOffDiagonalMagnitude)
    .ThenBy(r => r.BosonModeId, StringComparer.Ordinal)
    .ToArray();

bool graphMaterialized = labelRows.Length > 1 && transitionRecords.Length > 0;
bool allRowsHaveLabels = labelRows.All(r => r.ChargeSector is not null && (r.WeakSector is not null || r.HasQuantumNumbers));
bool anyDominantOffDiagonalTransition = transitionRecords.Any(r => r.OffDiagonalDominanceRatio > 1.0);
bool anyDirectionalTransition = transitionRecords.Any(r => r.DirectionalityRatio > 1e-9);
bool transitionGraphPromotable = graphMaterialized
    && allRowsHaveLabels
    && anyDominantOffDiagonalTransition
    && anyDirectionalTransition;
string terminalStatus = transitionGraphPromotable
    ? "fermion-coupling-transition-graph-sector-rule-ready"
    : "fermion-coupling-transition-graph-diagnostic-only";

var blockers = new List<string>();
if (!graphMaterialized)
    blockers.Add("repaired-row coupling transition graph could not be materialized");
if (!allRowsHaveLabels)
    blockers.Add("repaired rows still lack explicit chargeSector and weakSector/quantumNumbers labels");
if (!anyDominantOffDiagonalTransition)
    blockers.Add("no boson candidate has off-diagonal coupling larger than both repaired-row diagonal couplings");
if (!anyDirectionalTransition)
    blockers.Add("coupling transitions are symmetric in repaired-row order and do not define charged-current direction");
blockers.Add("coupling graph alone does not identify which transition is W-like or Z-like without a sector-label rule");

var result = new
{
    phaseId = "phase138-repaired-row-coupling-transition-graph",
    terminalStatus,
    couplingTransitionGraphMaterialized = graphMaterialized,
    transitionGraphPromotable,
    allRowsHaveLabels,
    anyDominantOffDiagonalTransition,
    anyDirectionalTransition,
    repairedRows = labelRows,
    transitionRecords,
    strongestOffDiagonalTransition = transitionRecords.FirstOrDefault(),
    phase137Gate = new
    {
        terminalStatus = JsonString(phase137.RootElement, "terminalStatus"),
        baseChiralityRoutePromotable = JsonBool(phase137.RootElement, "baseChiralityRoutePromotable"),
    },
    blockers,
    closureRequirements = new[]
    {
        "derive explicit fermion chargeSector and weakSector/quantumNumbers labels before promoting the transition graph",
        "derive a direction or conjugation rule for charged-current transitions if W-like transitions are required",
        "rerun the corrected W/Z sweep only after transition graph evidence is combined with a sector-label rule",
    },
    sourceEvidence = new
    {
        phase131Path = Phase131Path,
        phase137Path = Phase137Path,
        couplingAtlasPath = CouplingAtlasPath,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(
    Path.Combine(outputDir, "repaired_row_coupling_transition_graph.json"),
    JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "repaired_row_coupling_transition_graph_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.couplingTransitionGraphMaterialized,
        result.transitionGraphPromotable,
        result.allRowsHaveLabels,
        result.anyDominantOffDiagonalTransition,
        result.anyDirectionalTransition,
        result.strongestOffDiagonalTransition,
        result.blockers,
        result.closureRequirements,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"transitionRecordCount={transitionRecords.Length}");
Console.WriteLine($"strongestBosonModeId={transitionRecords.FirstOrDefault()?.BosonModeId}");
Console.WriteLine($"transitionGraphPromotable={transitionGraphPromotable}");

static TransitionRecord BuildTransitionRecord(
    string bosonModeId,
    IReadOnlyList<CouplingRecord> couplings,
    IReadOnlyDictionary<string, string> familyByMode)
{
    var diagonal = couplings.Where(c => c.FermionModeIdI == c.FermionModeIdJ).ToArray();
    var offDiagonal = couplings.Where(c => c.FermionModeIdI != c.FermionModeIdJ).ToArray();
    double maxDiagonal = diagonal.Length == 0 ? 0.0 : diagonal.Max(c => c.Magnitude);
    double maxOffDiagonal = offDiagonal.Length == 0 ? 0.0 : offDiagonal.Max(c => c.Magnitude);
    double dominanceRatio = maxDiagonal > 0.0 ? maxOffDiagonal / maxDiagonal : 0.0;
    double directionalityRatio = 0.0;
    if (offDiagonal.Length == 2)
    {
        double a = offDiagonal[0].Magnitude;
        double b = offDiagonal[1].Magnitude;
        double denom = Math.Max(Math.Max(a, b), 1e-30);
        directionalityRatio = Math.Abs(a - b) / denom;
    }

    var strongest = offDiagonal.OrderByDescending(c => c.Magnitude).FirstOrDefault();
    return new TransitionRecord(
        BosonModeId: bosonModeId,
        DiagonalMagnitudeByFamily: diagonal
            .OrderBy(c => familyByMode[c.FermionModeIdI], StringComparer.Ordinal)
            .Select(c => new FamilyMagnitude(familyByMode[c.FermionModeIdI], c.Magnitude))
            .ToArray(),
        OffDiagonalMagnitudeByOrderedFamilyPair: offDiagonal
            .OrderBy(c => familyByMode[c.FermionModeIdI], StringComparer.Ordinal)
            .ThenBy(c => familyByMode[c.FermionModeIdJ], StringComparer.Ordinal)
            .Select(c => new OrderedFamilyMagnitude(familyByMode[c.FermionModeIdI], familyByMode[c.FermionModeIdJ], c.Magnitude))
            .ToArray(),
        MaxDiagonalMagnitude: maxDiagonal,
        MaxOffDiagonalMagnitude: maxOffDiagonal,
        OffDiagonalDominanceRatio: dominanceRatio,
        DirectionalityRatio: directionalityRatio,
        StrongestTransitionFromFamilyId: strongest is null ? null : familyByMode[strongest.FermionModeIdI],
        StrongestTransitionToFamilyId: strongest is null ? null : familyByMode[strongest.FermionModeIdJ],
        VariationEvidenceId: couplings.Select(c => c.VariationEvidenceId).FirstOrDefault(id => id is not null));
}

static string RequiredString(JsonElement element, string propertyName) =>
    JsonString(element, propertyName) ?? throw new InvalidDataException($"Missing string property '{propertyName}'.");
static double RequiredDouble(JsonElement element, string propertyName) =>
    JsonDouble(element, propertyName) ?? throw new InvalidDataException($"Missing numeric property '{propertyName}'.");
static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;
static double? JsonDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var value) ? value : null;
static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

sealed record LabelRow(
    string FamilyId,
    string CandidateId,
    string SourceCanonicalFermionModeId,
    string? ChargeSector,
    string? WeakSector,
    bool HasQuantumNumbers);

sealed record CouplingRecord(
    string BosonModeId,
    string FermionModeIdI,
    string FermionModeIdJ,
    double Real,
    double Imaginary,
    double Magnitude,
    string? VariationEvidenceId);

sealed record FamilyMagnitude(string FamilyId, double Magnitude);

sealed record OrderedFamilyMagnitude(string FromFamilyId, string ToFamilyId, double Magnitude);

sealed record TransitionRecord(
    string BosonModeId,
    IReadOnlyList<FamilyMagnitude> DiagonalMagnitudeByFamily,
    IReadOnlyList<OrderedFamilyMagnitude> OffDiagonalMagnitudeByOrderedFamilyPair,
    double MaxDiagonalMagnitude,
    double MaxOffDiagonalMagnitude,
    double OffDiagonalDominanceRatio,
    double DirectionalityRatio,
    string? StrongestTransitionFromFamilyId,
    string? StrongestTransitionToFamilyId,
    string? VariationEvidenceId);
