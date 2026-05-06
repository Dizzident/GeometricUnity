using System.Text.Json;
using System.Text.Json.Nodes;

const string DefaultOutputDir = "studies/phase125_source_join_family_metadata_materialization_001/output";
const string Phase124EnrichedModesPath = "studies/phase124_phase95_source_join_metadata_materialization_001/output/phase95_l0_source_join_enriched_fermion_modes.json";
const string Phase124SummaryPath = "studies/phase124_phase95_source_join_metadata_materialization_001/output/phase95_source_join_metadata_materialization.json";
const string Phase12FamiliesPath = "studies/phase12_joined_calculation_001/output/background_family/fermions/fermion_families.json";
const string Phase12ClusterReportPath = "studies/phase12_joined_calculation_001/output/background_family/fermions/family_cluster_report.json";
const string Phase12ChiralityPath = "studies/phase12_joined_calculation_001/output/background_family/fermions/chirality_analysis_bg-phase12-bg-a-20260315212202.json";
const string Phase12ConjugationPath = "studies/phase12_joined_calculation_001/output/background_family/fermions/conjugation_pairs_bg-phase12-bg-a-20260315212202.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE125_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase124 = JsonDocument.Parse(File.ReadAllText(Phase124SummaryPath));
using var familiesDoc = JsonDocument.Parse(File.ReadAllText(Phase12FamiliesPath));
using var clustersDoc = JsonDocument.Parse(File.ReadAllText(Phase12ClusterReportPath));
using var chiralityDoc = JsonDocument.Parse(File.ReadAllText(Phase12ChiralityPath));
using var conjugationDoc = JsonDocument.Parse(File.ReadAllText(Phase12ConjugationPath));

var enrichedBundle = JsonNode.Parse(File.ReadAllText(Phase124EnrichedModesPath))!.AsObject();
var familyByCanonicalModeId = BuildFamilyIndex(familiesDoc.RootElement);
var familyClusterByFamilyId = BuildClusterIndex(clustersDoc.RootElement);
var chiralityByCanonicalModeId = chiralityDoc.RootElement.EnumerateArray()
    .ToDictionary(e => RequiredString(e, "modeId"), e => e.Clone(), StringComparer.Ordinal);
int conjugationPairCount = conjugationDoc.RootElement.ValueKind == JsonValueKind.Array
    ? conjugationDoc.RootElement.GetArrayLength()
    : 0;

var modeRecords = new List<ModeFamilyRecord>();
int qualityModeCount = 0;
int qualityFamilyMappedCount = 0;
var modes = enrichedBundle["modes"]?.AsArray() ?? throw new InvalidDataException("Phase124 bundle has no modes array.");
foreach (var node in modes)
{
    var mode = node?.AsObject() ?? throw new InvalidDataException("Mode entry is not an object.");
    string modeId = RequiredNodeString(mode, "modeId");
    int modeIndex = RequiredNodeInt(mode, "modeIndex");
    bool qualityPassed = IsQualityMode(mode);
    if (qualityPassed)
        qualityModeCount++;

    string? sourceFermionModeId = NodeString(mode, "sourceFermionModeId");
    int? sourceFermionModeIndex = NodeInt(mode, "sourceFermionModeIndex");
    string? sourceBackgroundId = NodeString(mode, "sourceFermionBackgroundId");
    string? canonicalSourceModeId = sourceBackgroundId is not null && sourceFermionModeIndex is not null
        ? $"mode-{sourceBackgroundId}-{sourceFermionModeIndex.Value:000}"
        : null;

    FamilyRecord? family = canonicalSourceModeId is not null && familyByCanonicalModeId.TryGetValue(canonicalSourceModeId, out var f)
        ? f
        : null;
    JsonElement? chirality = canonicalSourceModeId is not null && chiralityByCanonicalModeId.TryGetValue(canonicalSourceModeId, out var c)
        ? c
        : null;
    string? clusterId = family is not null && familyClusterByFamilyId.TryGetValue(family.FamilyId, out var cluster)
        ? cluster.ClusterId
        : null;

    if (family is not null)
    {
        mode["familyId"] = family.FamilyId;
        mode["sourceCanonicalFermionModeId"] = canonicalSourceModeId;
        mode["sourceFamilyJoin"] = JsonSerializer.SerializeToNode(new
        {
            family.FamilyId,
            canonicalSourceModeId,
            sourceFermionModeId,
            sourceFermionModeIndex,
            sourceBackgroundId,
            family.DominantChiralityProfile,
            family.HasConjugationPair,
            family.RefinementPersistenceScore,
            clusterId,
            joinBasis = "source background plus mode index mapped to Phase12 fermion family memberModeIds",
            externalTargetsUsed = false,
        });

        if (qualityPassed)
            qualityFamilyMappedCount++;
    }

    modeRecords.Add(new ModeFamilyRecord(
        ModeId: modeId,
        ModeIndex: modeIndex,
        QualityPassed: qualityPassed,
        SourceFermionModeId: sourceFermionModeId,
        CanonicalSourceModeId: canonicalSourceModeId,
        FamilyMapped: family is not null,
        FamilyId: family?.FamilyId,
        ClusterId: clusterId,
        DominantChiralityProfile: family?.DominantChiralityProfile,
        HasConjugationPair: family?.HasConjugationPair,
        ChiralityTag: chirality is not null ? JsonString(chirality.Value, "chiralityTag") : null,
        ChiralityStatus: chirality is not null ? JsonString(chirality.Value, "chiralityStatus") : null));
}

bool familyMetadataMaterialized = qualityModeCount > 0 && qualityFamilyMappedCount == qualityModeCount;
bool chiralityPromotable = modeRecords
    .Where(r => r.QualityPassed)
    .Any(r => !string.Equals(r.ChiralityTag, "trivial", StringComparison.Ordinal)
        && !string.IsNullOrWhiteSpace(r.ChiralityTag));
bool conjugationPromotable = modeRecords.Where(r => r.QualityPassed).Any(r => r.HasConjugationPair is true)
    || conjugationPairCount > 0;
bool sectorRulePromotable = false;
string terminalStatus = familyMetadataMaterialized
    ? "source-family-metadata-materialized-sector-labels-blocked"
    : "source-family-metadata-materialization-blocked";

var blockers = new List<string>();
if (!familyMetadataMaterialized)
    blockers.Add("familyId could not be materialized for all quality source-joined modes");
blockers.Add("fermion chargeSector remains absent");
blockers.Add("weak-sector or quantum-number fields remain absent");
if (!chiralityPromotable)
    blockers.Add("chirality remains trivial or not promotable for W charged-current selection");
if (!conjugationPromotable)
    blockers.Add("conjugation pairing remains absent for source family selection");

var enrichedPath = Path.Combine(outputDir, "phase95_l0_source_family_enriched_fermion_modes.json");
var result = new
{
    phaseId = "phase125-source-join-family-metadata-materialization",
    terminalStatus,
    familyMetadataMaterialized,
    sectorRulePromotable,
    enrichedModeBundlePath = enrichedPath,
    phase124Gate = new
    {
        terminalStatus = JsonString(phase124.RootElement, "terminalStatus"),
        sourceJoinMetadataMaterialized = JsonBool(phase124.RootElement, "sourceJoinMetadataMaterialized"),
    },
    familyJoinAudit = new
    {
        modeCount = modes.Count,
        qualityModeCount,
        qualityFamilyMappedCount,
        familyCount = familyByCanonicalModeId.Values.Select(f => f.FamilyId).Distinct(StringComparer.Ordinal).Count(),
        clusterCount = familyClusterByFamilyId.Values.Select(c => c.ClusterId).Distinct(StringComparer.Ordinal).Count(),
        chiralityPromotable,
        conjugationPromotable,
        conjugationPairCount,
        qualityModeRecords = modeRecords.Where(r => r.QualityPassed).ToList(),
    },
    diagnosis = familyMetadataMaterialized
        ? new[]
        {
            "target-blind Phase12 family IDs are now materialized for all quality source-joined repaired modes",
            "the remaining W/Z transition-rule blocker is charge/weak-sector identity, not source provenance or family join",
        }
        : new[]
        {
            "the source-joined repaired modes could not be mapped to Phase12 family records",
        },
    blockers,
    closureRequirements = new[]
    {
        "derive or materialize target-blind fermion chargeSector and weak-sector/quantum-number labels",
        "evaluate a nontrivial chirality or conjugation/sector identity observable for the repaired fermion families",
        "rerun the corrected-operator transition sweep only after those labels define a W/Z transition rule",
    },
    sourceEvidence = new
    {
        phase124EnrichedModesPath = Phase124EnrichedModesPath,
        phase124SummaryPath = Phase124SummaryPath,
        phase12FamiliesPath = Phase12FamiliesPath,
        phase12ClusterReportPath = Phase12ClusterReportPath,
        phase12ChiralityPath = Phase12ChiralityPath,
        phase12ConjugationPath = Phase12ConjugationPath,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true };
File.WriteAllText(enrichedPath, enrichedBundle.ToJsonString(options));
File.WriteAllText(
    Path.Combine(outputDir, "source_join_family_metadata_materialization.json"),
    JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "source_join_family_metadata_materialization_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.familyMetadataMaterialized,
        result.sectorRulePromotable,
        result.enrichedModeBundlePath,
        result.familyJoinAudit.qualityModeCount,
        result.familyJoinAudit.qualityFamilyMappedCount,
        result.blockers,
        result.closureRequirements,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"familyMetadataMaterialized={familyMetadataMaterialized}");
Console.WriteLine($"qualityFamilyMappedCount={qualityFamilyMappedCount}/{qualityModeCount}");
Console.WriteLine($"enrichedModeBundlePath={enrichedPath}");

static Dictionary<string, FamilyRecord> BuildFamilyIndex(JsonElement root)
{
    var index = new Dictionary<string, FamilyRecord>(StringComparer.Ordinal);
    foreach (var family in root.GetProperty("families").EnumerateArray())
    {
        var record = new FamilyRecord(
            FamilyId: RequiredString(family, "familyId"),
            DominantChiralityProfile: JsonString(family, "dominantChiralityProfile"),
            HasConjugationPair: JsonBool(family, "hasConjugationPair") ?? false,
            RefinementPersistenceScore: JsonDouble(family, "refinementPersistenceScore"));
        foreach (var modeId in family.GetProperty("memberModeIds").EnumerateArray().Select(e => e.GetString()).Where(s => s is not null))
            index[modeId!] = record;
    }

    return index;
}

static Dictionary<string, ClusterRecord> BuildClusterIndex(JsonElement root)
{
    var index = new Dictionary<string, ClusterRecord>(StringComparer.Ordinal);
    foreach (var cluster in root.GetProperty("clusters").EnumerateArray())
    {
        var record = new ClusterRecord(
            ClusterId: RequiredString(cluster, "clusterId"),
            DominantChirality: JsonString(cluster, "dominantChirality"),
            HasConjugatePair: JsonBool(cluster, "hasConjugatePair") ?? false);
        foreach (var familyId in cluster.GetProperty("memberFamilyIds").EnumerateArray().Select(e => e.GetString()).Where(s => s is not null))
            index[familyId!] = record;
    }

    return index;
}

static bool IsQualityMode(JsonObject mode)
{
    return (NodeDouble(mode, "branchStabilityScore") ?? 0.0) >= 0.5
        && (NodeDouble(mode, "refinementStabilityScore") ?? 0.0) >= 0.5
        && (NodeDouble(mode, "residualNorm") ?? double.PositiveInfinity) <= 1e-6
        && NodeBool(mode, "gaugeReductionApplied") is true;
}

static string RequiredString(JsonElement element, string propertyName)
{
    return JsonString(element, propertyName)
        ?? throw new InvalidDataException($"Missing string property '{propertyName}'.");
}

static string RequiredNodeString(JsonObject element, string propertyName)
{
    return element[propertyName]?.GetValue<string>()
        ?? throw new InvalidDataException($"Missing string property '{propertyName}'.");
}

static int RequiredNodeInt(JsonObject element, string propertyName)
{
    return element[propertyName]?.GetValue<int>()
        ?? throw new InvalidDataException($"Missing integer property '{propertyName}'.");
}

static string? JsonString(JsonElement element, string propertyName)
{
    return element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
        ? property.GetString()
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

static double? JsonDouble(JsonElement element, string propertyName)
{
    return element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number
        ? property.GetDouble()
        : null;
}

static string? NodeString(JsonObject element, string propertyName)
{
    return element[propertyName]?.GetValueKind() == JsonValueKind.String
        ? element[propertyName]!.GetValue<string>()
        : null;
}

static int? NodeInt(JsonObject element, string propertyName)
{
    return element[propertyName]?.GetValueKind() == JsonValueKind.Number
        ? element[propertyName]!.GetValue<int>()
        : null;
}

static bool? NodeBool(JsonObject element, string propertyName)
{
    return element[propertyName]?.GetValueKind() switch
    {
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        _ => null,
    };
}

static double? NodeDouble(JsonObject element, string propertyName)
{
    return element[propertyName]?.GetValueKind() == JsonValueKind.Number
        ? element[propertyName]!.GetValue<double>()
        : null;
}

sealed record FamilyRecord(
    string FamilyId,
    string? DominantChiralityProfile,
    bool HasConjugationPair,
    double? RefinementPersistenceScore);

sealed record ClusterRecord(
    string ClusterId,
    string? DominantChirality,
    bool HasConjugatePair);

sealed record ModeFamilyRecord(
    string ModeId,
    int ModeIndex,
    bool QualityPassed,
    string? SourceFermionModeId,
    string? CanonicalSourceModeId,
    bool FamilyMapped,
    string? FamilyId,
    string? ClusterId,
    string? DominantChiralityProfile,
    bool? HasConjugationPair,
    string? ChiralityTag,
    string? ChiralityStatus);
