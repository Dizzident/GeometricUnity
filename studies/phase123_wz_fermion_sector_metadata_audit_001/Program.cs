using System.Text.Json;

const string DefaultOutputDir = "studies/phase123_wz_fermion_sector_metadata_audit_001/output";
const string Phase122Path = "studies/phase122_corrected_operator_selection_rule_sweep_001/output/corrected_operator_selection_rule_sweep.json";
const string Phase95ModesPath = "studies/phase95_target_blind_refinement_mode_matching_001/output/phase94_l0_2x2_refinement_matched_fermion_modes.json";
const string Phase12ChiralityPath = "studies/phase12_joined_calculation_001/output/background_family/fermions/chirality_analysis_bg-phase12-bg-a-20260315212202.json";
const string Phase12ConjugationPath = "studies/phase12_joined_calculation_001/output/background_family/fermions/conjugation_pairs_bg-phase12-bg-a-20260315212202.json";
const string Phase12FamiliesPath = "studies/phase12_joined_calculation_001/output/background_family/fermions/fermion_families.json";
const string Phase12ClusterReportPath = "studies/phase12_joined_calculation_001/output/background_family/fermions/family_cluster_report.json";
const string Phase27BosonChargeSectorsPath = "studies/phase27_charge_sector_convention_001/mode_families_with_charge_sectors.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE123_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase122 = JsonDocument.Parse(File.ReadAllText(Phase122Path));
using var phase95 = JsonDocument.Parse(File.ReadAllText(Phase95ModesPath));
using var phase12Chirality = JsonDocument.Parse(File.ReadAllText(Phase12ChiralityPath));
using var phase12Conjugation = JsonDocument.Parse(File.ReadAllText(Phase12ConjugationPath));
using var phase12Families = JsonDocument.Parse(File.ReadAllText(Phase12FamiliesPath));
using var phase12Clusters = JsonDocument.Parse(File.ReadAllText(Phase12ClusterReportPath));
using var phase27 = JsonDocument.Parse(File.ReadAllText(Phase27BosonChargeSectorsPath));

var modes = phase95.RootElement.GetProperty("modes").EnumerateArray().Select(ModeMetadata.FromJson).ToList();
var qualityModes = modes.Where(m => m.QualityPassed).ToList();
var missingFields = new[]
{
    "chargeSector",
    "familyId",
    "weakIsospin",
    "weakSector",
    "quantumNumbers",
    "sourceModeId",
    "sourceFermionModeId",
}.Where(field => modes.All(m => !m.PresentFields.Contains(field, StringComparer.Ordinal))).ToList();

var phase12ChiralityTags = phase12Chirality.RootElement
    .EnumerateArray()
    .GroupBy(e => JsonString(e, "chiralityTag") ?? "missing")
    .ToDictionary(g => g.Key, g => g.Count(), StringComparer.Ordinal);
var phase12ChiralityNotes = phase12Chirality.RootElement
    .EnumerateArray()
    .SelectMany(e => StringArray(e, "diagnosticNotes"))
    .Distinct(StringComparer.Ordinal)
    .ToList();
var phase12ConjugationPairCount = phase12Conjugation.RootElement.ValueKind == JsonValueKind.Array
    ? phase12Conjugation.RootElement.GetArrayLength()
    : 0;

var phase12FermionFamilies = phase12Families.RootElement.GetProperty("families").EnumerateArray().Select(f => new
{
    familyId = JsonString(f, "familyId"),
    memberModeIds = StringArray(f, "memberModeIds"),
    dominantChiralityProfile = JsonString(f, "dominantChiralityProfile"),
    hasConjugationPair = JsonBool(f, "hasConjugationPair"),
    refinementPersistenceScore = JsonDouble(f, "refinementPersistenceScore"),
}).ToList();
var phase12ClusterSummary = phase12Clusters.RootElement.GetProperty("summary").Clone();

var bosonChargeFamilies = phase27.RootElement.GetProperty("families").EnumerateArray().Select(f =>
{
    var identity = f.GetProperty("identityFeatures");
    return new
    {
        familyId = JsonString(f, "familyId"),
        sourceCandidateId = JsonString(f, "sourceCandidateId"),
        electroweakMultipletId = JsonString(identity, "electroweakMultipletId"),
        algebraBasisSector = JsonString(identity, "algebraBasisSector"),
        chargeSector = JsonString(identity, "chargeSector"),
        dominantBasisIndex = JsonInt(identity, "dominantBasisIndex"),
    };
}).ToList();
var wzBosonSources = bosonChargeFamilies
    .Where(f => string.Equals(f.sourceCandidateId, "phase12-candidate-0", StringComparison.Ordinal)
        || string.Equals(f.sourceCandidateId, "phase12-candidate-2", StringComparison.Ordinal))
    .ToList();

bool hasFermionChargeMetadata = modes.Any(m => m.PresentFields.Contains("chargeSector", StringComparer.Ordinal));
bool hasFermionFamilyMetadata = modes.Any(m => m.PresentFields.Contains("familyId", StringComparer.Ordinal));
bool hasFermionQuantumMetadata = modes.Any(m => m.PresentFields.Contains("quantumNumbers", StringComparer.Ordinal)
    || m.PresentFields.Contains("weakIsospin", StringComparer.Ordinal)
    || m.PresentFields.Contains("weakSector", StringComparer.Ordinal));
bool hasSourceJoinKey = modes.Any(m => m.PresentFields.Contains("sourceModeId", StringComparer.Ordinal)
    || m.PresentFields.Contains("sourceFermionModeId", StringComparer.Ordinal));
bool phase12ChiralityPromotable = phase12ChiralityTags.Any(kvp => !string.Equals(kvp.Key, "trivial", StringComparison.Ordinal));
bool phase95ChiralityPromotable = modes.Any(m => !string.Equals(m.ChiralitySignConvention, "not-evaluated", StringComparison.Ordinal));
bool phase95ConjugationPromotable = modes.Any(m => m.ConjugationHasPair);
bool phase12ConjugationPromotable = phase12ConjugationPairCount > 0;
bool bosonChargeMetadataAvailable = wzBosonSources.Count == 2
    && wzBosonSources.Any(f => string.Equals(f.chargeSector, "charged", StringComparison.Ordinal))
    && wzBosonSources.Any(f => string.Equals(f.chargeSector, "neutral", StringComparison.Ordinal));

bool fermionSectorMetadataAvailable = hasFermionChargeMetadata
    && hasFermionFamilyMetadata
    && hasFermionQuantumMetadata
    && (hasSourceJoinKey || phase95ConjugationPromotable || phase12ConjugationPromotable || phase95ChiralityPromotable || phase12ChiralityPromotable);
bool fermionSectorRulePromotable = bosonChargeMetadataAvailable && fermionSectorMetadataAvailable;

string terminalStatus = fermionSectorRulePromotable
    ? "wz-fermion-sector-rule-prerequisites-ready"
    : "wz-fermion-sector-rule-prerequisites-blocked";

var blockers = new List<string>();
if (!bosonChargeMetadataAvailable)
    blockers.Add("W/Z boson charge-sector metadata is incomplete");
if (!hasFermionChargeMetadata)
    blockers.Add("Phase95 repaired fermion modes have no chargeSector field");
if (!hasFermionFamilyMetadata)
    blockers.Add("Phase95 repaired fermion modes have no familyId field");
if (!hasFermionQuantumMetadata)
    blockers.Add("Phase95 repaired fermion modes have no weak-sector or quantum-number field");
if (!hasSourceJoinKey)
    blockers.Add("Phase95 repaired fermion modes have no source fermion mode join key");
if (!phase95ChiralityPromotable)
    blockers.Add("Phase95 repaired fermion chirality is not evaluated");
if (!phase95ConjugationPromotable)
    blockers.Add("Phase95 repaired fermion conjugation pairing is not evaluated");
if (!phase12ChiralityPromotable)
    blockers.Add("Phase12 chirality analysis is trivial and cannot select charged-current fermion transitions");
if (!phase12ConjugationPromotable)
    blockers.Add("Phase12 conjugation-pair artifact contains no pairs");

var result = new
{
    phaseId = "phase123-wz-fermion-sector-metadata-audit",
    terminalStatus,
    fermionSectorRulePromotable,
    bosonChargeMetadataAvailable,
    fermionSectorMetadataAvailable,
    phase122Gate = new
    {
        terminalStatus = JsonString(phase122.RootElement, "terminalStatus"),
        transitionCount = JsonInt(phase122.RootElement, "transitionCount"),
        qualityTransitionCount = JsonInt(phase122.RootElement, "qualityTransitionCount"),
        projectionCandidatePresent = phase122.RootElement.TryGetProperty("projectionCandidate", out var candidate)
            && candidate.ValueKind != JsonValueKind.Null,
        selectedPair03 = phase122.RootElement.GetProperty("selectedPair03").Clone(),
        strongest = phase122.RootElement.GetProperty("strongest").Clone(),
        strongestOffDiagonal = phase122.RootElement.GetProperty("strongestOffDiagonal").Clone(),
    },
    phase95ModeAudit = new
    {
        modeCount = modes.Count,
        qualityModeCount = qualityModes.Count,
        qualityModeIndices = qualityModes.Select(m => m.ModeIndex).ToList(),
        missingFields,
        hasFermionChargeMetadata,
        hasFermionFamilyMetadata,
        hasFermionQuantumMetadata,
        hasSourceJoinKey,
        phase95ChiralityPromotable,
        phase95ConjugationPromotable,
        modes,
    },
    phase12FermionAudit = new
    {
        chiralityTags = phase12ChiralityTags,
        chiralityPromotable = phase12ChiralityPromotable,
        chiralityNotes = phase12ChiralityNotes,
        conjugationPairCount = phase12ConjugationPairCount,
        conjugationPromotable = phase12ConjugationPromotable,
        familyCount = phase12FermionFamilies.Count,
        familiesWithConjugationPair = phase12FermionFamilies.Count(f => f.hasConjugationPair is true),
        familiesWithRefinementPersistence = phase12FermionFamilies.Count(f => (f.refinementPersistenceScore ?? 0.0) > 0.0),
        clusterSummary = phase12ClusterSummary,
    },
    phase27BosonAudit = new
    {
        bosonFamilyCount = bosonChargeFamilies.Count,
        wzBosonSources,
        chargedFamilyCount = bosonChargeFamilies.Count(f => string.Equals(f.chargeSector, "charged", StringComparison.Ordinal)),
        neutralFamilyCount = bosonChargeFamilies.Count(f => string.Equals(f.chargeSector, "neutral", StringComparison.Ordinal)),
    },
    diagnosis = fermionSectorRulePromotable
        ? new[]
        {
            "target-blind fermion sector metadata is available for a W/Z transition-selection rule",
        }
        : new[]
        {
            "the corrected W/Z boson source metadata is available, but no target-blind fermion charge/family/weak-sector rule can be derived from the repaired Phase95 modes",
            "Phase122's corrected-operator sweep exhausted all quality transitions without a projection candidate",
            "Phase95 mode vectors cannot be joined to Phase12 fermion family metadata by a persisted source-mode key",
            "Phase12 chirality and conjugation artifacts are not promotable as W charged-current selection rules",
        },
    blockers,
    closureRequirements = fermionSectorRulePromotable
        ? new[]
        {
            "materialize the derived transition rule and rerun Phase122 under the rule",
            "rerun Phase115 and Phase116 only after the rule-gated corrected transition produces a projection candidate",
        }
        : new[]
        {
            "materialize target-blind fermion charge/family/weak-sector metadata on repaired exact modes",
            "persist source fermion mode join keys from Phase12/Phase91/Phase94 into Phase95 repaired modes",
            "rerun the corrected-operator transition sweep with the derived W/Z transition rule before any boson prediction rerun",
            "if fermion-sector metadata cannot be derived, open a W/Z bridge-construction audit instead of applying scalar normalization",
        },
    sourceEvidence = new
    {
        phase122Path = Phase122Path,
        phase95ModesPath = Phase95ModesPath,
        phase12ChiralityPath = Phase12ChiralityPath,
        phase12ConjugationPath = Phase12ConjugationPath,
        phase12FamiliesPath = Phase12FamiliesPath,
        phase12ClusterReportPath = Phase12ClusterReportPath,
        phase27BosonChargeSectorsPath = Phase27BosonChargeSectorsPath,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true };
File.WriteAllText(
    Path.Combine(outputDir, "wz_fermion_sector_metadata_audit.json"),
    JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "wz_fermion_sector_metadata_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.fermionSectorRulePromotable,
        result.bosonChargeMetadataAvailable,
        result.fermionSectorMetadataAvailable,
        result.phase95ModeAudit.modeCount,
        result.phase95ModeAudit.qualityModeCount,
        result.phase95ModeAudit.missingFields,
        result.blockers,
        result.closureRequirements,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"fermionSectorRulePromotable={fermionSectorRulePromotable}");
Console.WriteLine($"bosonChargeMetadataAvailable={bosonChargeMetadataAvailable}");
Console.WriteLine($"fermionSectorMetadataAvailable={fermionSectorMetadataAvailable}");
Console.WriteLine($"missingFields={string.Join(",", missingFields)}");

static string? JsonString(JsonElement element, string propertyName)
{
    return element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
        ? property.GetString()
        : null;
}

static int? JsonInt(JsonElement element, string propertyName)
{
    return element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number
        ? property.GetInt32()
        : null;
}

static double? JsonDouble(JsonElement element, string propertyName)
{
    return element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number
        ? property.GetDouble()
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
    return element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Array
        ? property.EnumerateArray()
            .Where(item => item.ValueKind == JsonValueKind.String)
            .Select(item => item.GetString() ?? string.Empty)
            .Where(value => value.Length > 0)
            .ToList()
        : Array.Empty<string>();
}

sealed record ModeMetadata(
    int ModeIndex,
    string ModeId,
    bool QualityPassed,
    IReadOnlyList<string> PresentFields,
    double? BranchStabilityScore,
    double? RefinementStabilityScore,
    double? ResidualNorm,
    bool? GaugeReductionApplied,
    string? ChiralitySignConvention,
    double? ChiralityLeftFraction,
    double? ChiralityRightFraction,
    double? ChiralityMixedFraction,
    bool ConjugationHasPair,
    string? ConjugationType)
{
    public static ModeMetadata FromJson(JsonElement mode)
    {
        var fields = mode.EnumerateObject().Select(p => p.Name).ToList();
        var chirality = mode.TryGetProperty("chiralityDecomposition", out var c) ? c : default;
        var conjugation = mode.TryGetProperty("conjugationPairing", out var cp) ? cp : default;
        var branch = JsonDouble(mode, "branchStabilityScore");
        var refinement = JsonDouble(mode, "refinementStabilityScore");
        var residual = JsonDouble(mode, "residualNorm");
        var gauge = JsonBool(mode, "gaugeReductionApplied");
        bool qualityPassed = (branch ?? 0.0) >= 0.5
            && (refinement ?? 0.0) >= 0.5
            && (residual ?? double.PositiveInfinity) <= 1e-6
            && gauge is true;

        return new ModeMetadata(
            ModeIndex: JsonInt(mode, "modeIndex") ?? -1,
            ModeId: JsonString(mode, "modeId") ?? "missing-mode-id",
            QualityPassed: qualityPassed,
            PresentFields: fields,
            BranchStabilityScore: branch,
            RefinementStabilityScore: refinement,
            ResidualNorm: residual,
            GaugeReductionApplied: gauge,
            ChiralitySignConvention: chirality.ValueKind == JsonValueKind.Object ? JsonString(chirality, "signConvention") : null,
            ChiralityLeftFraction: chirality.ValueKind == JsonValueKind.Object ? JsonDouble(chirality, "leftFraction") : null,
            ChiralityRightFraction: chirality.ValueKind == JsonValueKind.Object ? JsonDouble(chirality, "rightFraction") : null,
            ChiralityMixedFraction: chirality.ValueKind == JsonValueKind.Object ? JsonDouble(chirality, "mixedFraction") : null,
            ConjugationHasPair: conjugation.ValueKind == JsonValueKind.Object && JsonBool(conjugation, "hasPair") is true,
            ConjugationType: conjugation.ValueKind == JsonValueKind.Object ? JsonString(conjugation, "conjugationType") : null);
    }

    private static string? JsonString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;
    }

    private static int? JsonInt(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number
            ? property.GetInt32()
            : null;
    }

    private static double? JsonDouble(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number
            ? property.GetDouble()
            : null;
    }

    private static bool? JsonBool(JsonElement element, string propertyName)
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
}
