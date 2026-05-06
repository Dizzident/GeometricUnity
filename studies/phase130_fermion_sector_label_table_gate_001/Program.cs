using System.Text.Json;

const string DefaultOutputDir = "studies/phase130_fermion_sector_label_table_gate_001/output";
const string Phase129Path = "studies/phase129_candidate_cluster_sector_identity_audit_001/output/candidate_cluster_sector_identity_audit.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE130_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase129 = JsonDocument.Parse(File.ReadAllText(Phase129Path));
var root = phase129.RootElement;

var qualityModes = root.GetProperty("qualityModes").EnumerateArray().ToList();
var matchedClusters = root.GetProperty("matchedClusters").EnumerateArray().ToList();
var candidateLinks = root.GetProperty("candidateLinks").EnumerateArray().ToList();
var registryCandidates = root.GetProperty("registryCandidates").EnumerateArray().ToList();
var representationRecords = root.GetProperty("representationRecords").EnumerateArray().ToList();

var familyToCluster = matchedClusters
    .SelectMany(cluster => StringArray(cluster, "matchedQualityFamilyIds").Select(familyId => new
        FamilyClusterJoin(
            FamilyId: familyId,
            ClusterId: RequiredString(cluster, "clusterId"),
            ClusterMemberFamilyIds: StringArray(cluster, "memberFamilyIds"),
            DominantChirality: JsonString(cluster, "dominantChirality"),
            HasConjugatePair: JsonBool(cluster, "hasConjugatePair"))))
    .ToDictionary(x => x.FamilyId, x => x, StringComparer.Ordinal);
var clusterToCandidate = candidateLinks
    .Where(link => JsonString(link, "primarySourceId") is not null)
    .ToDictionary(link => JsonString(link, "primarySourceId")!, link => link, StringComparer.Ordinal);
var candidateToRegistry = registryCandidates
    .Where(candidate => JsonString(candidate, "candidateId") is not null)
    .ToDictionary(candidate => JsonString(candidate, "candidateId")!, candidate => candidate, StringComparer.Ordinal);
var candidateToRepresentation = representationRecords
    .Where(record => JsonString(record, "candidateId") is not null)
    .ToDictionary(record => JsonString(record, "candidateId")!, record => record, StringComparer.Ordinal);

var labelRecords = qualityModes
    .Select(mode => BuildLabelRecord(mode, familyToCluster, clusterToCandidate, candidateToRegistry, candidateToRepresentation))
    .ToList();

bool allQualityFamiliesHaveRows = labelRecords.Count == qualityModes.Count;
bool allRowsHaveCandidateCoverage = labelRecords.All(r => r.CandidateCoversFamily);
bool allRowsHaveRepresentationCompleteness = labelRecords.All(r => r.RepresentationContentComplete);
bool allRowsHavePhysicalLabels = labelRecords.All(r => r.HasPhysicalSectorLabels);
bool sectorLabelTablePromotable = allQualityFamiliesHaveRows
    && allRowsHaveCandidateCoverage
    && allRowsHaveRepresentationCompleteness
    && allRowsHavePhysicalLabels;
string terminalStatus = sectorLabelTablePromotable
    ? "fermion-sector-label-table-ready"
    : "fermion-sector-label-table-incomplete-blocked";

var blockers = new List<string>();
if (!allQualityFamiliesHaveRows)
    blockers.Add("not every quality repaired family has a sector-label table row");
if (!allRowsHaveCandidateCoverage)
    blockers.Add("matched registry candidate does not cover every repaired quality family");
if (!allRowsHaveRepresentationCompleteness)
    blockers.Add("not every candidate row has complete representation-content evidence");
if (!allRowsHavePhysicalLabels)
    blockers.Add("sector-label table rows are missing explicit chargeSector and weak-sector/quantum-number labels");
if (JsonBool(root, "sectorIdentityPromotable") is not true)
    blockers.Add("Phase129 found no promotable candidate/cluster sector identity source");

var result = new
{
    phaseId = "phase130-fermion-sector-label-table-gate",
    terminalStatus,
    sectorLabelTableMaterialized = true,
    sectorLabelTablePromotable,
    allQualityFamiliesHaveRows,
    allRowsHaveCandidateCoverage,
    allRowsHaveRepresentationCompleteness,
    allRowsHavePhysicalLabels,
    qualityModeCount = qualityModes.Count,
    labelRecords,
    labelSchema = new
    {
        requiredFields = new[]
        {
            "familyId",
            "candidateId",
            "chargeSector",
            "weakSector or quantumNumbers",
            "derivationSource",
            "targetBlind=true",
        },
        allowedLabelStatus = new[] { "assigned", "unassigned" },
        promotionRule = "all quality repaired families must have candidate coverage, complete representation-content evidence, and explicit target-blind physical sector labels",
    },
    phase129Gate = new
    {
        terminalStatus = JsonString(root, "terminalStatus"),
        sectorIdentityPromotable = JsonBool(root, "sectorIdentityPromotable"),
        representationCompletenessPromotable = JsonBool(root, "representationCompletenessPromotable"),
    },
    blockers,
    closureRequirements = new[]
    {
        "repair candidate coverage so every repaired quality family is represented by the matched candidate or by a derived successor candidate",
        "populate explicit target-blind chargeSector and weak-sector/quantum-number labels for every quality repaired family",
        "attach a derivationSource for each assigned sector label before rerunning the corrected W/Z transition sweep",
    },
    sourceEvidence = new
    {
        phase129Path = Phase129Path,
    },
};

var options = new JsonSerializerOptions
{
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
};
File.WriteAllText(
    Path.Combine(outputDir, "fermion_sector_label_table_gate.json"),
    JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "fermion_sector_label_table_gate_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.sectorLabelTableMaterialized,
        result.sectorLabelTablePromotable,
        result.allRowsHaveCandidateCoverage,
        result.allRowsHaveRepresentationCompleteness,
        result.allRowsHavePhysicalLabels,
        result.qualityModeCount,
        labelRecords = labelRecords.Select(r => new
        {
            r.ModeIndex,
            r.FamilyId,
            r.ClusterId,
            r.CandidateId,
            r.LabelStatus,
            r.CandidateCoversFamily,
            r.RepresentationContentComplete,
            r.MissingRequiredLabels,
            r.RowBlockers,
        }),
        result.blockers,
        result.closureRequirements,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"qualityModeCount={qualityModes.Count}");
Console.WriteLine($"allRowsHaveCandidateCoverage={allRowsHaveCandidateCoverage}");
Console.WriteLine($"sectorLabelTablePromotable={sectorLabelTablePromotable}");

static SectorLabelRecord BuildLabelRecord(
    JsonElement mode,
    IReadOnlyDictionary<string, FamilyClusterJoin> familyToCluster,
    IReadOnlyDictionary<string, JsonElement> clusterToCandidate,
    IReadOnlyDictionary<string, JsonElement> candidateToRegistry,
    IReadOnlyDictionary<string, JsonElement> candidateToRepresentation)
{
    string familyId = RequiredString(mode, "familyId");
    var rowBlockers = new List<string>();
    string? clusterId = null;
    string? candidateId = null;
    IReadOnlyList<string> clusterMemberFamilyIds = [];
    IReadOnlyList<string> registryContributingSourceIds = [];
    bool candidateCoversFamily = false;
    bool representationContentComplete = false;
    string? representationRecordId = null;
    double? representationStructuralMismatchScore = null;

    if (familyToCluster.TryGetValue(familyId, out var cluster))
    {
        clusterId = cluster.ClusterId;
        clusterMemberFamilyIds = cluster.ClusterMemberFamilyIds;
        if (clusterToCandidate.TryGetValue(clusterId, out var candidateLink))
            candidateId = JsonString(candidateLink, "candidateId");
    }
    else
    {
        rowBlockers.Add("family does not map to a matched cluster");
    }

    if (candidateId is not null && candidateToRegistry.TryGetValue(candidateId, out var registryCandidate))
    {
        registryContributingSourceIds = StringArray(registryCandidate, "contributingSourceIds");
        candidateCoversFamily = registryContributingSourceIds.Contains(familyId, StringComparer.Ordinal);
        if (!candidateCoversFamily)
            rowBlockers.Add("matched registry candidate does not list this repaired family as a contributing source");
    }
    else
    {
        rowBlockers.Add("family does not map to a registry candidate");
    }

    if (candidateId is not null && candidateToRepresentation.TryGetValue(candidateId, out var representationRecord))
    {
        representationRecordId = JsonString(representationRecord, "recordId");
        representationStructuralMismatchScore = JsonDouble(representationRecord, "structuralMismatchScore");
        representationContentComplete = JsonBool(representationRecord, "consistent") is true
            && JsonInt(representationRecord, "missingRequiredCount") == 0;
        if (!representationContentComplete)
            rowBlockers.Add("candidate representation-content evidence is incomplete");
    }
    else
    {
        rowBlockers.Add("family candidate does not map to representation-content evidence");
    }

    var missingRequiredLabels = new List<string> { "chargeSector", "weakSector or quantumNumbers" };
    rowBlockers.Add("physical sector labels are unassigned");

    return new SectorLabelRecord(
        ModeId: RequiredString(mode, "modeId"),
        ModeIndex: RequiredInt(mode, "modeIndex"),
        FamilyId: familyId,
        SourceCanonicalFermionModeId: JsonString(mode, "sourceCanonicalFermionModeId"),
        ClusterId: clusterId,
        CandidateId: candidateId,
        LabelStatus: "unassigned",
        TargetBlind: true,
        ChargeSector: null,
        WeakSector: null,
        QuantumNumbers: null,
        DerivationSource: null,
        HasPhysicalSectorLabels: false,
        MissingRequiredLabels: missingRequiredLabels,
        CandidateCoversFamily: candidateCoversFamily,
        RepresentationContentComplete: representationContentComplete,
        ClusterMemberFamilyIds: clusterMemberFamilyIds,
        RegistryContributingSourceIds: registryContributingSourceIds,
        RepresentationRecordId: representationRecordId,
        RepresentationStructuralMismatchScore: representationStructuralMismatchScore,
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

sealed record SectorLabelRecord(
    string ModeId,
    int ModeIndex,
    string FamilyId,
    string? SourceCanonicalFermionModeId,
    string? ClusterId,
    string? CandidateId,
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
    string? RepresentationRecordId,
    double? RepresentationStructuralMismatchScore,
    IReadOnlyList<string> RowBlockers);

sealed record FamilyClusterJoin(
    string FamilyId,
    string ClusterId,
    IReadOnlyList<string> ClusterMemberFamilyIds,
    string? DominantChirality,
    bool? HasConjugatePair);
