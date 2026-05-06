using System.Text.Json;

const string DefaultOutputDir = "studies/phase131_sector_label_candidate_coverage_repair_001/output";
const string Phase130Path = "studies/phase130_fermion_sector_label_table_gate_001/output/fermion_sector_label_table_gate.json";
const string SuccessorCandidateId = "fermion-registry-phase4-toy-v1-0001-p131-quality-coverage";

var outputDir = Environment.GetEnvironmentVariable("PHASE131_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase130 = JsonDocument.Parse(File.ReadAllText(Phase130Path));
var root = phase130.RootElement;
var priorRows = root.GetProperty("labelRecords").EnumerateArray().ToList();
var qualityFamilyIds = priorRows
    .Select(row => RequiredString(row, "familyId"))
    .Distinct(StringComparer.Ordinal)
    .Order(StringComparer.Ordinal)
    .ToArray();
var sourceCandidateIds = priorRows
    .Select(row => JsonString(row, "candidateId"))
    .Where(id => id is not null)
    .Distinct(StringComparer.Ordinal)
    .ToArray();
var sourceRepresentationRecordIds = priorRows
    .Select(row => JsonString(row, "representationRecordId"))
    .Where(id => id is not null)
    .Distinct(StringComparer.Ordinal)
    .ToArray();
var clusterIds = priorRows
    .Select(row => JsonString(row, "clusterId"))
    .Where(id => id is not null)
    .Distinct(StringComparer.Ordinal)
    .ToArray();

var repairedRows = priorRows
    .Select(row => RepairCoverage(row, qualityFamilyIds))
    .ToList();

bool allRowsHaveCandidateCoverage = repairedRows.All(row => row.CandidateCoversFamily);
bool allRowsHaveRepresentationCompleteness = repairedRows.All(row => row.RepresentationContentComplete);
bool allRowsHavePhysicalLabels = repairedRows.All(row => row.HasPhysicalSectorLabels);
bool sectorLabelTablePromotable = allRowsHaveCandidateCoverage && allRowsHaveRepresentationCompleteness && allRowsHavePhysicalLabels;
string terminalStatus = sectorLabelTablePromotable
    ? "sector-label-candidate-coverage-repaired-label-table-ready"
    : "sector-label-candidate-coverage-repaired-labels-blocked";

var blockers = new List<string>();
if (!allRowsHaveCandidateCoverage)
    blockers.Add("candidate coverage remains incomplete after successor-candidate repair");
if (!allRowsHaveRepresentationCompleteness)
    blockers.Add("representation-content evidence is incomplete for at least one repaired row");
if (!allRowsHavePhysicalLabels)
    blockers.Add("coverage-repaired sector-label table still lacks explicit chargeSector and weak-sector/quantum-number labels");
if (JsonBool(root, "allRowsHavePhysicalLabels") is not true)
    blockers.Add("Phase130 label rows were unassigned; P131 intentionally does not invent physical labels");

var successorCandidate = new
{
    candidateId = SuccessorCandidateId,
    candidateKind = "target-blind-coverage-successor",
    sourceCandidateIds,
    sourceRepresentationRecordIds,
    clusterIds,
    contributingSourceIds = qualityFamilyIds,
    coverageRepair = new
    {
        repairedMissingFamilies = priorRows
            .Where(row => JsonBool(row, "candidateCoversFamily") is not true)
            .Select(row => RequiredString(row, "familyId"))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray(),
        coverageDerivedFrom = "Phase130 quality repaired family set and matched cluster membership",
        physicalSectorLabelsAssigned = false,
    },
};

var result = new
{
    phaseId = "phase131-sector-label-candidate-coverage-repair",
    terminalStatus,
    candidateCoverageRepairMaterialized = true,
    sectorLabelTablePromotable,
    allRowsHaveCandidateCoverage,
    allRowsHaveRepresentationCompleteness,
    allRowsHavePhysicalLabels,
    successorCandidate,
    labelRecords = repairedRows,
    phase130Gate = new
    {
        terminalStatus = JsonString(root, "terminalStatus"),
        sectorLabelTablePromotable = JsonBool(root, "sectorLabelTablePromotable"),
        allRowsHaveCandidateCoverage = JsonBool(root, "allRowsHaveCandidateCoverage"),
        allRowsHavePhysicalLabels = JsonBool(root, "allRowsHavePhysicalLabels"),
    },
    blockers,
    closureRequirements = new[]
    {
        "derive explicit target-blind chargeSector labels for every coverage-repaired row",
        "derive explicit target-blind weakSector or quantumNumbers labels for every coverage-repaired row",
        "attach a derivationSource to each physical label before rerunning the corrected W/Z transition sweep",
    },
    sourceEvidence = new
    {
        phase130Path = Phase130Path,
    },
};

var options = new JsonSerializerOptions
{
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
};
File.WriteAllText(
    Path.Combine(outputDir, "sector_label_candidate_coverage_repair.json"),
    JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "sector_label_candidate_coverage_repair_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.candidateCoverageRepairMaterialized,
        result.sectorLabelTablePromotable,
        result.allRowsHaveCandidateCoverage,
        result.allRowsHaveRepresentationCompleteness,
        result.allRowsHavePhysicalLabels,
        result.successorCandidate,
        labelRecords = repairedRows.Select(row => new
        {
            row.ModeIndex,
            row.FamilyId,
            row.CandidateId,
            row.LabelStatus,
            row.CandidateCoversFamily,
            row.MissingRequiredLabels,
            row.RowBlockers,
        }),
        result.blockers,
        result.closureRequirements,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"allRowsHaveCandidateCoverage={allRowsHaveCandidateCoverage}");
Console.WriteLine($"allRowsHavePhysicalLabels={allRowsHavePhysicalLabels}");
Console.WriteLine($"sectorLabelTablePromotable={sectorLabelTablePromotable}");

static CoverageRepairedSectorLabelRecord RepairCoverage(JsonElement row, IReadOnlyList<string> qualityFamilyIds)
{
    var rowBlockers = new List<string>();
    if (JsonBool(row, "hasPhysicalSectorLabels") is not true)
        rowBlockers.Add("physical sector labels are unassigned");

    return new CoverageRepairedSectorLabelRecord(
        ModeId: RequiredString(row, "modeId"),
        ModeIndex: RequiredInt(row, "modeIndex"),
        FamilyId: RequiredString(row, "familyId"),
        SourceCanonicalFermionModeId: JsonString(row, "sourceCanonicalFermionModeId"),
        ClusterId: JsonString(row, "clusterId"),
        CandidateId: SuccessorCandidateId,
        SourceCandidateId: JsonString(row, "candidateId"),
        LabelStatus: "unassigned",
        TargetBlind: true,
        ChargeSector: null,
        WeakSector: null,
        QuantumNumbers: null,
        DerivationSource: null,
        HasPhysicalSectorLabels: false,
        MissingRequiredLabels: new[] { "chargeSector", "weakSector or quantumNumbers" },
        CandidateCoversFamily: true,
        RepresentationContentComplete: JsonBool(row, "representationContentComplete") is true,
        ClusterMemberFamilyIds: StringArray(row, "clusterMemberFamilyIds"),
        RegistryContributingSourceIds: qualityFamilyIds,
        SourceRegistryContributingSourceIds: StringArray(row, "registryContributingSourceIds"),
        RepresentationRecordId: JsonString(row, "representationRecordId"),
        RepresentationStructuralMismatchScore: JsonDouble(row, "representationStructuralMismatchScore"),
        RowBlockers: rowBlockers);
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

static IReadOnlyList<string> StringArray(JsonElement element, string propertyName)
{
    if (!element.TryGetProperty(propertyName, out var property) || property.ValueKind != JsonValueKind.Array)
        return [];

    return property.EnumerateArray()
        .Where(item => item.ValueKind == JsonValueKind.String)
        .Select(item => item.GetString()!)
        .ToArray();
}

sealed record CoverageRepairedSectorLabelRecord(
    string ModeId,
    int ModeIndex,
    string FamilyId,
    string? SourceCanonicalFermionModeId,
    string? ClusterId,
    string CandidateId,
    string? SourceCandidateId,
    string LabelStatus,
    bool TargetBlind,
    string? ChargeSector,
    string? WeakSector,
    object? QuantumNumbers,
    string? DerivationSource,
    bool HasPhysicalSectorLabels,
    IReadOnlyList<string> MissingRequiredLabels,
    bool CandidateCoversFamily,
    bool RepresentationContentComplete,
    IReadOnlyList<string> ClusterMemberFamilyIds,
    IReadOnlyList<string> RegistryContributingSourceIds,
    IReadOnlyList<string> SourceRegistryContributingSourceIds,
    string? RepresentationRecordId,
    double? RepresentationStructuralMismatchScore,
    IReadOnlyList<string> RowBlockers);
