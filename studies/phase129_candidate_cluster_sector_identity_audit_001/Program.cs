using System.Text.Json;

const string DefaultOutputDir = "studies/phase129_candidate_cluster_sector_identity_audit_001/output";
const string Phase125EnrichedModesPath = "studies/phase125_source_join_family_metadata_materialization_001/output/phase95_l0_source_family_enriched_fermion_modes.json";
const string Phase128Path = "studies/phase128_fermion_su2_generator_sector_observable_001/output/fermion_su2_generator_sector_observable.json";
const string Phase12ClusterReportPath = "studies/phase12_joined_calculation_001/output/background_family/fermions/family_cluster_report.json";
const string Phase4RegistryPath = "studies/phase4_fermion_family_atlas_001/output/unified_particle_registry.json";
const string Phase5ObservationChainPath = "studies/phase5_su2_branch_refinement_env_validation/config/observation_chain.json";
const string Phase5RepresentationContentPath = "studies/phase5_su2_branch_refinement_env_validation/config/representation_content.json";
const string Phase5CouplingConsistencyPath = "studies/phase5_su2_branch_refinement_env_validation/config/coupling_consistency.json";
const string Phase5CandidateProvenanceLinksPath = "studies/phase5_su2_branch_refinement_env_validation/config/candidate_provenance_links.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE129_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var enriched = JsonDocument.Parse(File.ReadAllText(Phase125EnrichedModesPath));
using var phase128 = JsonDocument.Parse(File.ReadAllText(Phase128Path));
using var clusters = JsonDocument.Parse(File.ReadAllText(Phase12ClusterReportPath));
using var registry = JsonDocument.Parse(File.ReadAllText(Phase4RegistryPath));
using var observationChain = JsonDocument.Parse(File.ReadAllText(Phase5ObservationChainPath));
using var representationContent = JsonDocument.Parse(File.ReadAllText(Phase5RepresentationContentPath));
using var couplingConsistency = JsonDocument.Parse(File.ReadAllText(Phase5CouplingConsistencyPath));
using var provenanceLinks = JsonDocument.Parse(File.ReadAllText(Phase5CandidateProvenanceLinksPath));

var qualityModes = enriched.RootElement.GetProperty("modes")
    .EnumerateArray()
    .Where(IsQualityMode)
    .Select(mode => new
    {
        modeId = RequiredString(mode, "modeId"),
        modeIndex = RequiredInt(mode, "modeIndex"),
        familyId = JsonString(mode, "familyId"),
        sourceCanonicalFermionModeId = JsonString(mode, "sourceCanonicalFermionModeId"),
    })
    .ToList();
var qualityFamilyIds = qualityModes.Select(m => m.familyId).Where(id => id is not null).Distinct(StringComparer.Ordinal).ToHashSet(StringComparer.Ordinal);

var matchedClusters = clusters.RootElement.GetProperty("clusters")
    .EnumerateArray()
    .Where(cluster => cluster.GetProperty("memberFamilyIds").EnumerateArray().Any(id => qualityFamilyIds.Contains(id.GetString())))
    .Select(cluster => new
    {
        clusterId = RequiredString(cluster, "clusterId"),
        memberFamilyIds = StringArray(cluster, "memberFamilyIds"),
        matchedQualityFamilyIds = StringArray(cluster, "memberFamilyIds").Where(qualityFamilyIds.Contains).ToArray(),
        dominantChirality = JsonString(cluster, "dominantChirality"),
        hasConjugatePair = JsonBool(cluster, "hasConjugatePair"),
        clusteringMethod = JsonString(cluster, "clusteringMethod"),
        hasChargeSector = HasAny(cluster, "chargeSector"),
        hasWeakSector = HasAny(cluster, "weakSector", "weakIsospin"),
        hasQuantumNumbers = HasAny(cluster, "quantumNumbers"),
        clusteringNotes = StringArray(cluster, "clusteringNotes"),
    })
    .ToList();
var matchedClusterIds = matchedClusters.Select(c => c.clusterId).ToHashSet(StringComparer.Ordinal);

var candidateLinks = observationChain.RootElement
    .EnumerateArray()
    .Where(link => JsonString(link, "primarySourceId") is { } primarySourceId && matchedClusterIds.Contains(primarySourceId))
    .Select(link => new
    {
        candidateId = RequiredString(link, "candidateId"),
        primarySourceId = JsonString(link, "primarySourceId"),
        observableId = JsonString(link, "observableId"),
        completenessStatus = JsonString(link, "completenessStatus"),
        passed = JsonBool(link, "passed"),
        hasChargeSector = HasAny(link, "chargeSector"),
        hasWeakSector = HasAny(link, "weakSector", "weakIsospin"),
        hasQuantumNumbers = HasAny(link, "quantumNumbers"),
        notes = JsonString(link, "notes"),
    })
    .ToList();
var candidateIds = candidateLinks.Select(c => c.candidateId).ToHashSet(StringComparer.Ordinal);

var registryEntries = registry.RootElement.TryGetProperty("particles", out var particles)
    ? particles
    : registry.RootElement.GetProperty("candidates");
var registryCandidates = registryEntries
    .EnumerateArray()
    .Where(candidate => JsonString(candidate, "particleId") is { } particleId && candidateIds.Contains(particleId))
    .Select(candidate => new
    {
        candidateId = RequiredString(candidate, "particleId"),
        particleType = JsonString(candidate, "particleType"),
        primarySourceId = JsonString(candidate, "primarySourceId"),
        contributingSourceIds = StringArray(candidate, "contributingSourceIds"),
        chirality = JsonString(candidate, "chirality"),
        observationConfidence = JsonDouble(candidate, "observationConfidence"),
        claimClass = JsonString(candidate, "claimClass"),
        hasChargeSector = HasAny(candidate, "chargeSector"),
        hasWeakSector = HasAny(candidate, "weakSector", "weakIsospin"),
        hasQuantumNumbers = HasAny(candidate, "quantumNumbers"),
        ambiguityNotes = StringArray(candidate, "ambiguityNotes"),
    })
    .ToList();

var representationRecords = representationContent.RootElement
    .EnumerateArray()
    .Where(record => JsonString(record, "candidateId") is { } candidateId && candidateIds.Contains(candidateId))
    .Select(record => new
    {
        recordId = RequiredString(record, "recordId"),
        candidateId = JsonString(record, "candidateId"),
        expectedModeCount = JsonInt(record, "expectedModeCount"),
        observedModeCount = JsonInt(record, "observedModeCount"),
        missingRequiredCount = JsonInt(record, "missingRequiredCount"),
        structuralMismatchScore = JsonDouble(record, "structuralMismatchScore"),
        consistent = JsonBool(record, "consistent"),
        hasChargeSector = HasAny(record, "chargeSector"),
        hasWeakSector = HasAny(record, "weakSector", "weakIsospin"),
        hasQuantumNumbers = HasAny(record, "quantumNumbers"),
        inconsistencyDescription = JsonString(record, "inconsistencyDescription"),
    })
    .ToList();

var couplingRecords = couplingConsistency.RootElement
    .EnumerateArray()
    .Where(record => JsonString(record, "candidateId") is { } candidateId && candidateIds.Contains(candidateId))
    .Select(record => new
    {
        recordId = RequiredString(record, "recordId"),
        candidateId = JsonString(record, "candidateId"),
        couplingType = JsonString(record, "couplingType"),
        relativeSpread = JsonDouble(record, "relativeSpread"),
        consistent = JsonBool(record, "consistent"),
        hasChargeSector = HasAny(record, "chargeSector"),
        hasWeakSector = HasAny(record, "weakSector", "weakIsospin"),
        hasQuantumNumbers = HasAny(record, "quantumNumbers"),
        notes = JsonString(record, "notes"),
    })
    .ToList();

var provenanceRecords = provenanceLinks.RootElement
    .EnumerateArray()
    .Where(record => JsonString(record, "candidateId") is { } candidateId && candidateIds.Contains(candidateId))
    .Select(record => new
    {
        candidateId = JsonString(record, "candidateId"),
        branchVariantIds = StringArray(record, "branchVariantIds"),
        backgroundIds = StringArray(record, "backgroundIds"),
        hasChargeSector = HasAny(record, "chargeSector"),
        hasWeakSector = HasAny(record, "weakSector", "weakIsospin"),
        hasQuantumNumbers = HasAny(record, "quantumNumbers"),
        notes = JsonString(record, "notes"),
    })
    .ToList();

bool clusterIdentityPromotable = matchedClusters.Any(c => c.hasChargeSector || c.hasWeakSector || c.hasQuantumNumbers);
bool candidateIdentityPromotable = candidateLinks.Any(c => c.hasChargeSector || c.hasWeakSector || c.hasQuantumNumbers)
    || registryCandidates.Any(c => c.hasChargeSector || c.hasWeakSector || c.hasQuantumNumbers)
    || representationRecords.Any(c => c.hasChargeSector || c.hasWeakSector || c.hasQuantumNumbers)
    || couplingRecords.Any(c => c.hasChargeSector || c.hasWeakSector || c.hasQuantumNumbers)
    || provenanceRecords.Any(c => c.hasChargeSector || c.hasWeakSector || c.hasQuantumNumbers);
bool representationCompletenessPromotable = representationRecords.Any(r => r.consistent is true && r.missingRequiredCount == 0);
bool sectorIdentityPromotable = clusterIdentityPromotable || candidateIdentityPromotable;
string terminalStatus = sectorIdentityPromotable
    ? "candidate-cluster-sector-identity-source-ready"
    : "candidate-cluster-sector-identity-audit-blocked";

var blockers = new List<string>();
if (matchedClusters.Count == 0)
    blockers.Add("quality repaired families do not map to any Phase12 cluster");
if (candidateLinks.Count == 0)
    blockers.Add("matched Phase12 clusters do not map to a Phase5 observation-chain candidate");
if (representationCompletenessPromotable)
    blockers.Add("matched representation-content record is complete, but it is only a multiplicity/falsification check and carries no chargeSector, weak-sector, or quantum-number labels");
if (!sectorIdentityPromotable)
    blockers.Add("candidate, cluster, representation-content, coupling-consistency, and provenance records contain no target-blind fermion sector labels");
if (JsonBool(phase128.RootElement, "wzTransitionRulePromotable") is not true)
    blockers.Add("Phase128 found no promotable SU(2) generator-sector transition rule");

var result = new
{
    phaseId = "phase129-candidate-cluster-sector-identity-audit",
    terminalStatus,
    candidateClusterAuditMaterialized = true,
    sectorIdentityPromotable,
    clusterIdentityPromotable,
    candidateIdentityPromotable,
    representationCompletenessPromotable,
    qualityModeCount = qualityModes.Count,
    qualityFamilyIds,
    qualityModes,
    matchedClusters,
    candidateLinks,
    registryCandidates,
    representationRecords,
    couplingRecords,
    provenanceRecords,
    phase128Gate = new
    {
        terminalStatus = JsonString(phase128.RootElement, "terminalStatus"),
        wzTransitionRulePromotable = JsonBool(phase128.RootElement, "wzTransitionRulePromotable"),
    },
    diagnosis = sectorIdentityPromotable
        ? "Candidate/cluster records provide a target-blind sector identity source."
        : "The repaired families map to a stable candidate/cluster, but available candidate-level evidence does not encode fermion charge or weak-sector identity.",
    blockers,
    closureRequirements = new[]
    {
        "add or derive explicit target-blind chargeSector and weak-sector/quantum-number labels for the matched fermion candidate or its member families",
        "do not treat representation-content completeness, coupling consistency, or branch provenance as physical sector identity without an explicit derivation",
        "rerun the corrected W/Z transition sweep only after candidate/family sector labels or a nontrivial transition table are materialized",
    },
    sourceEvidence = new
    {
        phase125EnrichedModesPath = Phase125EnrichedModesPath,
        phase128Path = Phase128Path,
        phase12ClusterReportPath = Phase12ClusterReportPath,
        phase4RegistryPath = Phase4RegistryPath,
        phase5ObservationChainPath = Phase5ObservationChainPath,
        phase5RepresentationContentPath = Phase5RepresentationContentPath,
        phase5CouplingConsistencyPath = Phase5CouplingConsistencyPath,
        phase5CandidateProvenanceLinksPath = Phase5CandidateProvenanceLinksPath,
    },
};

var options = new JsonSerializerOptions
{
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
};
File.WriteAllText(
    Path.Combine(outputDir, "candidate_cluster_sector_identity_audit.json"),
    JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "candidate_cluster_sector_identity_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.candidateClusterAuditMaterialized,
        result.sectorIdentityPromotable,
        result.representationCompletenessPromotable,
        result.qualityModeCount,
        result.qualityFamilyIds,
        matchedClusterIds = matchedClusters.Select(c => c.clusterId),
        candidateIds,
        result.blockers,
        result.closureRequirements,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"qualityModeCount={qualityModes.Count}");
Console.WriteLine($"matchedClusterCount={matchedClusters.Count}");
Console.WriteLine($"sectorIdentityPromotable={sectorIdentityPromotable}");

static bool IsQualityMode(JsonElement mode)
{
    return (JsonDouble(mode, "branchStabilityScore") ?? 0.0) >= 0.5
        && (JsonDouble(mode, "refinementStabilityScore") ?? 0.0) >= 0.5
        && (JsonDouble(mode, "residualNorm") ?? double.PositiveInfinity) <= 1e-6
        && JsonBool(mode, "gaugeReductionApplied") is true;
}

static bool HasAny(JsonElement element, params string[] propertyNames) =>
    propertyNames.Any(name => element.TryGetProperty(name, out _));

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
