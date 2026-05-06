using System.Text.Json;

const string DefaultOutputDir = "studies/phase126_fermion_sector_identity_source_audit_001/output";
const string Phase125EnrichedModesPath = "studies/phase125_source_join_family_metadata_materialization_001/output/phase95_l0_source_family_enriched_fermion_modes.json";
const string Phase125Path = "studies/phase125_source_join_family_metadata_materialization_001/output/source_join_family_metadata_materialization.json";
const string Phase12FamiliesPath = "studies/phase12_joined_calculation_001/output/background_family/fermions/fermion_families.json";
const string Phase12ClusterReportPath = "studies/phase12_joined_calculation_001/output/background_family/fermions/family_cluster_report.json";
const string Phase12ChiralityPath = "studies/phase12_joined_calculation_001/output/background_family/fermions/chirality_analysis_bg-phase12-bg-a-20260315212202.json";
const string Phase12ConjugationPath = "studies/phase12_joined_calculation_001/output/background_family/fermions/conjugation_pairs_bg-phase12-bg-a-20260315212202.json";
const string Phase12RegistryPath = "studies/phase12_joined_calculation_001/output/background_family/particle_registry/unified_particle_registry.json";
const string Phase27BosonChargePath = "studies/phase27_charge_sector_convention_001/mode_families_with_charge_sectors.json";
const string Phase5RepresentationContentPath = "studies/phase5_su2_branch_refinement_env_validation/config/representation_content.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE126_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase125 = JsonDocument.Parse(File.ReadAllText(Phase125Path));
using var enriched = JsonDocument.Parse(File.ReadAllText(Phase125EnrichedModesPath));
using var families = JsonDocument.Parse(File.ReadAllText(Phase12FamiliesPath));
using var clusters = JsonDocument.Parse(File.ReadAllText(Phase12ClusterReportPath));
using var chirality = JsonDocument.Parse(File.ReadAllText(Phase12ChiralityPath));
using var conjugation = JsonDocument.Parse(File.ReadAllText(Phase12ConjugationPath));
using var registry = JsonDocument.Parse(File.ReadAllText(Phase12RegistryPath));
using var bosonCharge = JsonDocument.Parse(File.ReadAllText(Phase27BosonChargePath));

var qualityModes = enriched.RootElement.GetProperty("modes")
    .EnumerateArray()
    .Where(IsQualityMode)
    .Select(mode => new QualityModeSectorRecord(
        ModeId: RequiredString(mode, "modeId"),
        ModeIndex: RequiredInt(mode, "modeIndex"),
        SourceFermionModeId: JsonString(mode, "sourceFermionModeId"),
        SourceCanonicalFermionModeId: JsonString(mode, "sourceCanonicalFermionModeId"),
        FamilyId: JsonString(mode, "familyId"),
        HasChargeSector: mode.TryGetProperty("chargeSector", out _),
        HasWeakSector: mode.TryGetProperty("weakSector", out _) || mode.TryGetProperty("weakIsospin", out _),
        HasQuantumNumbers: mode.TryGetProperty("quantumNumbers", out _),
        ChiralitySignConvention: mode.TryGetProperty("chiralityDecomposition", out var c) ? JsonString(c, "signConvention") : null,
        ConjugationType: mode.TryGetProperty("conjugationPairing", out var cp) ? JsonString(cp, "conjugationType") : null))
    .ToList();

var qualityFamilyIds = qualityModes.Select(m => m.FamilyId).Where(id => id is not null).Distinct(StringComparer.Ordinal).ToHashSet(StringComparer.Ordinal);
var familyRecords = families.RootElement.GetProperty("families")
    .EnumerateArray()
    .Where(f => qualityFamilyIds.Contains(RequiredString(f, "familyId")))
    .Select(f => new SourceAuditRecord(
        SourceId: RequiredString(f, "familyId"),
        SourceType: "phase12-fermion-family",
        HasChargeSector: f.TryGetProperty("chargeSector", out _),
        HasWeakSector: f.TryGetProperty("weakSector", out _) || f.TryGetProperty("weakIsospin", out _),
        HasQuantumNumbers: f.TryGetProperty("quantumNumbers", out _),
        ChiralityStatus: JsonString(f, "dominantChiralityProfile"),
        HasConjugationPair: JsonBool(f, "hasConjugationPair") is true,
        Notes: StringArray(f, "ambiguityNotes")))
    .ToList();

var clusterRecords = clusters.RootElement.GetProperty("clusters")
    .EnumerateArray()
    .Where(c => c.GetProperty("memberFamilyIds").EnumerateArray().Any(id => qualityFamilyIds.Contains(id.GetString())))
    .Select(c => new SourceAuditRecord(
        SourceId: RequiredString(c, "clusterId"),
        SourceType: "phase12-fermion-cluster",
        HasChargeSector: c.TryGetProperty("chargeSector", out _),
        HasWeakSector: c.TryGetProperty("weakSector", out _) || c.TryGetProperty("weakIsospin", out _),
        HasQuantumNumbers: c.TryGetProperty("quantumNumbers", out _),
        ChiralityStatus: JsonString(c, "dominantChirality"),
        HasConjugationPair: JsonBool(c, "hasConjugatePair") is true,
        Notes: StringArray(c, "clusteringNotes")))
    .ToList();

var registryEntries = registry.RootElement.TryGetProperty("particles", out var particles)
    ? particles
    : registry.RootElement.GetProperty("candidates");
var registryRecords = registryEntries
    .EnumerateArray()
    .Where(p => string.Equals(JsonString(p, "particleType"), "Fermion", StringComparison.Ordinal))
    .Select(p => new SourceAuditRecord(
        SourceId: RequiredString(p, "particleId"),
        SourceType: "phase12-unified-fermion-registry",
        HasChargeSector: p.TryGetProperty("chargeSector", out _),
        HasWeakSector: p.TryGetProperty("weakSector", out _) || p.TryGetProperty("weakIsospin", out _),
        HasQuantumNumbers: p.TryGetProperty("quantumNumbers", out _),
        ChiralityStatus: JsonString(p, "chirality"),
        HasConjugationPair: false,
        Notes: StringArray(p, "ambiguityNotes")))
    .ToList();

var chiralityTags = chirality.RootElement.EnumerateArray()
    .GroupBy(e => JsonString(e, "chiralityTag") ?? "missing")
    .ToDictionary(g => g.Key, g => g.Count(), StringComparer.Ordinal);
int conjugationPairCount = conjugation.RootElement.ValueKind == JsonValueKind.Array
    ? conjugation.RootElement.GetArrayLength()
    : 0;
var bosonChargeSectorCounts = bosonCharge.RootElement.GetProperty("families")
    .EnumerateArray()
    .Select(f => f.GetProperty("identityFeatures"))
    .GroupBy(f => JsonString(f, "chargeSector") ?? "missing")
    .ToDictionary(g => g.Key, g => g.Count(), StringComparer.Ordinal);

bool representationContentExists = File.Exists(Phase5RepresentationContentPath);
bool anyFermionChargeSource = qualityModes.Any(m => m.HasChargeSector)
    || familyRecords.Any(r => r.HasChargeSector)
    || clusterRecords.Any(r => r.HasChargeSector)
    || registryRecords.Any(r => r.HasChargeSector);
bool anyWeakSectorSource = qualityModes.Any(m => m.HasWeakSector || m.HasQuantumNumbers)
    || familyRecords.Any(r => r.HasWeakSector || r.HasQuantumNumbers)
    || clusterRecords.Any(r => r.HasWeakSector || r.HasQuantumNumbers)
    || registryRecords.Any(r => r.HasWeakSector || r.HasQuantumNumbers);
bool chiralityPromotable = chiralityTags.Any(kvp => !string.Equals(kvp.Key, "trivial", StringComparison.Ordinal));
bool conjugationPromotable = conjugationPairCount > 0
    || familyRecords.Any(r => r.HasConjugationPair)
    || clusterRecords.Any(r => r.HasConjugationPair);
bool fermionSectorIdentityPromotable = anyFermionChargeSource && anyWeakSectorSource;
string terminalStatus = fermionSectorIdentityPromotable
    ? "fermion-sector-identity-source-ready"
    : "fermion-sector-identity-source-blocked";

var blockers = new List<string>();
if (!anyFermionChargeSource)
    blockers.Add("no target-blind fermion chargeSector source exists for the quality repaired families");
if (!anyWeakSectorSource)
    blockers.Add("no target-blind weak-sector or quantum-number source exists for the quality repaired families");
if (!chiralityPromotable)
    blockers.Add("chirality evidence is trivial and cannot define W charged-current transitions");
if (!conjugationPromotable)
    blockers.Add("conjugation-pair evidence is absent for the repaired families");
blockers.Add("boson charge-sector metadata cannot be transferred to fermions without an explicit fermion sector observable");

var result = new
{
    phaseId = "phase126-fermion-sector-identity-source-audit",
    terminalStatus,
    fermionSectorIdentityPromotable,
    phase125Gate = new
    {
        terminalStatus = JsonString(phase125.RootElement, "terminalStatus"),
        familyMetadataMaterialized = JsonBool(phase125.RootElement, "familyMetadataMaterialized"),
    },
    qualityModeCount = qualityModes.Count,
    qualityFamilyIds,
    qualityModes,
    sourceAudits = new
    {
        phase125EnrichedModes = new
        {
            hasFermionChargeSource = qualityModes.Any(m => m.HasChargeSector),
            hasWeakSectorSource = qualityModes.Any(m => m.HasWeakSector || m.HasQuantumNumbers),
        },
        phase12Families = familyRecords,
        phase12Clusters = clusterRecords,
        phase12UnifiedFermionRegistry = registryRecords,
        phase12Chirality = new
        {
            chiralityTags,
            chiralityPromotable,
        },
        phase12Conjugation = new
        {
            conjugationPairCount,
            conjugationPromotable,
        },
        phase27BosonCharge = new
        {
            bosonChargeSectorCounts,
            transferableToFermions = false,
            transferBlocker = "Phase27 charge sectors label boson source families only.",
        },
        representationContent = new
        {
            path = Phase5RepresentationContentPath,
            exists = representationContentExists,
            promotableForFermionSectorIdentity = false,
            blocker = representationContentExists
                ? "representation-content sidecar is Phase5 falsification evidence, not a fermion charge/weak-sector assignment table"
                : "representation-content sidecar is not present at the checked path",
        },
    },
    blockers,
    closureRequirements = new[]
    {
        "implement a target-blind fermion sector identity observable that emits chargeSector and weak-sector/quantum-number labels for repaired families",
        "or derive a nontrivial chirality/conjugation sector table that can define W charged-current and Z neutral-current transition rules",
        "rerun the corrected-operator transition sweep only after that sector table exists",
    },
    sourceEvidence = new
    {
        phase125EnrichedModesPath = Phase125EnrichedModesPath,
        phase125Path = Phase125Path,
        phase12FamiliesPath = Phase12FamiliesPath,
        phase12ClusterReportPath = Phase12ClusterReportPath,
        phase12ChiralityPath = Phase12ChiralityPath,
        phase12ConjugationPath = Phase12ConjugationPath,
        phase12RegistryPath = Phase12RegistryPath,
        phase27BosonChargePath = Phase27BosonChargePath,
        phase5RepresentationContentPath = Phase5RepresentationContentPath,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true };
File.WriteAllText(
    Path.Combine(outputDir, "fermion_sector_identity_source_audit.json"),
    JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "fermion_sector_identity_source_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.fermionSectorIdentityPromotable,
        result.qualityModeCount,
        result.qualityFamilyIds,
        anyFermionChargeSource,
        anyWeakSectorSource,
        chiralityPromotable,
        conjugationPromotable,
        result.blockers,
        result.closureRequirements,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"fermionSectorIdentityPromotable={fermionSectorIdentityPromotable}");
Console.WriteLine($"anyFermionChargeSource={anyFermionChargeSource}");
Console.WriteLine($"anyWeakSectorSource={anyWeakSectorSource}");

static bool IsQualityMode(JsonElement mode)
{
    return (JsonDouble(mode, "branchStabilityScore") ?? 0.0) >= 0.5
        && (JsonDouble(mode, "refinementStabilityScore") ?? 0.0) >= 0.5
        && (JsonDouble(mode, "residualNorm") ?? double.PositiveInfinity) <= 1e-6
        && JsonBool(mode, "gaugeReductionApplied") is true;
}

static string RequiredString(JsonElement element, string propertyName)
{
    return JsonString(element, propertyName)
        ?? throw new InvalidDataException($"Missing string property '{propertyName}'.");
}

static int RequiredInt(JsonElement element, string propertyName)
{
    return element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number
        ? property.GetInt32()
        : throw new InvalidDataException($"Missing integer property '{propertyName}'.");
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

static IReadOnlyList<string> StringArray(JsonElement element, string propertyName)
{
    return element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Array
        ? property.EnumerateArray()
            .Where(item => item.ValueKind == JsonValueKind.String)
            .Select(item => item.GetString() ?? string.Empty)
            .Where(value => value.Length > 0)
            .ToList()
        : Array.Empty<string>();
}

sealed record QualityModeSectorRecord(
    string ModeId,
    int ModeIndex,
    string? SourceFermionModeId,
    string? SourceCanonicalFermionModeId,
    string? FamilyId,
    bool HasChargeSector,
    bool HasWeakSector,
    bool HasQuantumNumbers,
    string? ChiralitySignConvention,
    string? ConjugationType);

sealed record SourceAuditRecord(
    string SourceId,
    string SourceType,
    bool HasChargeSector,
    bool HasWeakSector,
    bool HasQuantumNumbers,
    string? ChiralityStatus,
    bool HasConjugationPair,
    IReadOnlyList<string> Notes);
