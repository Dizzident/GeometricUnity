using System.Text.Json;

const string DefaultOutputDir = "studies/phase189_higgs_scalar_source_operator_census_001/output";
const string RegistryPath = "studies/phase12_joined_calculation_001/output/background_family/bosons/registry.json";
const string Phase51Path = "studies/phase51_broad_boson_prediction_readiness_001/broad_boson_prediction_readiness.json";
const string Phase70Path = "studies/phase70_scalar_sector_bridge_evidence_001/scalar_sector_bridge_evidence.json";
const string Phase106Path = "studies/phase106_candidate3_observable_identity_derivation_001/output/candidate3_observable_identity_derivation.json";
const string Phase187Path = "studies/phase187_higgs_scalar_source_identity_scaffold_001/output/higgs_scalar_source_identity_scaffold.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE189_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var registry = JsonDocument.Parse(File.ReadAllText(RegistryPath));
using var phase51 = JsonDocument.Parse(File.ReadAllText(Phase51Path));
using var phase70 = JsonDocument.Parse(File.ReadAllText(Phase70Path));
using var phase106 = JsonDocument.Parse(File.ReadAllText(Phase106Path));
using var phase187 = JsonDocument.Parse(File.ReadAllText(Phase187Path));

var candidates = registry.RootElement.GetProperty("candidates").EnumerateArray()
    .Select(candidate => new ScalarOperatorCandidate(
        RequiredString(candidate, "candidateId"),
        JsonString(candidate, "claimClass") ?? "unknown",
        JsonDouble(candidate, "branchStabilityScore") ?? 0.0,
        JsonDouble(candidate, "refinementStabilityScore") ?? 0.0,
        candidate.TryGetProperty("polarizationEnvelope", out var polarizationEnvelope) && polarizationEnvelope.ValueKind != JsonValueKind.Null,
        candidate.TryGetProperty("symmetryEnvelope", out var symmetryEnvelope) && symmetryEnvelope.ValueKind != JsonValueKind.Null,
        candidate.TryGetProperty("interactionProxyEnvelope", out var interactionProxyEnvelope) && interactionProxyEnvelope.ValueKind != JsonValueKind.Null,
        NumberArray(candidate, "massLikeEnvelope"),
        NumberArray(candidate, "gaugeLeakEnvelope")))
    .OrderBy(candidate => candidate.CandidateId, StringComparer.Ordinal)
    .ToArray();

bool scalarVevBridgePresent = string.Equals(JsonString(phase70.RootElement, "terminalStatus"), "scalar-sector-bridge-evidence-derived", StringComparison.Ordinal);
bool candidate3RejectedForHiggs = phase106.RootElement.GetProperty("identityCandidates").EnumerateArray()
    .Any(candidate => RequiredString(candidate, "physicalObservableId") == "higgs-mass"
        && RequiredString(candidate, "status") == "rejected");
bool p187ScaffoldMaterialized = JsonBool(phase187.RootElement, "scaffoldMaterialized") is true;
bool p187Validated = JsonBool(phase187.RootElement, "higgsSourceIdentityValidated") is true;

int scalarFeatureEnvelopeCount = candidates.Count(candidate =>
    candidate.PolarizationEnvelopePresent || candidate.SymmetryEnvelopePresent || candidate.InteractionProxyEnvelopePresent);
int branchStableNonC0Count = candidates.Count(candidate =>
    candidate.BranchStabilityScore >= 0.5 && !string.Equals(candidate.ClaimClass, "C0_NumericalMode", StringComparison.Ordinal));
int massiveScalarProfileCount = candidates.Count(candidate =>
    candidate.MassLikeEnvelope.Any(value => Math.Abs(value) >= 0.01 && Math.Abs(value) <= 100.0)
    && candidate.GaugeLeakEnvelope.All(value => Math.Abs(value) <= 0.10));
double maxAbsMassLike = candidates.SelectMany(candidate => candidate.MassLikeEnvelope).Select(Math.Abs).DefaultIfEmpty(0.0).Max();
double maxAbsGaugeLeak = candidates.SelectMany(candidate => candidate.GaugeLeakEnvelope).Select(Math.Abs).DefaultIfEmpty(0.0).Max();

bool sourceOperatorSolved = false;
bool identitySidecarPresent = scalarFeatureEnvelopeCount > 0 && !candidate3RejectedForHiggs;
bool stabilitySidecarsPresent = branchStableNonC0Count > 0;
bool targetIndependentConstruction = true;
bool censusPromotable = sourceOperatorSolved
    && identitySidecarPresent
    && stabilitySidecarsPresent
    && massiveScalarProfileCount > 0
    && targetIndependentConstruction;

var higgsReadiness = phase51.RootElement.GetProperty("records").EnumerateArray()
    .Single(record => RequiredString(record, "particleId") == "higgs");
var checks = new[]
{
    new CensusCheck("p187-scaffold-materialized", p187ScaffoldMaterialized, "P187 materialized the Higgs source/identity scaffold."),
    new CensusCheck("scalar-vev-bridge-present", scalarVevBridgePresent, "P70 supplies an electroweak VEV/order-parameter bridge, not a Higgs excitation source."),
    new CensusCheck("source-operator-solved", sourceOperatorSolved, "Missing solved scalar-sector source/operator for Higgs excitation."),
    new CensusCheck("scalar-identity-feature-envelopes-present", scalarFeatureEnvelopeCount > 0, "No current registry candidate has polarization, symmetry, or interaction identity envelopes."),
    new CensusCheck("candidate3-higgs-identity-accepted", !candidate3RejectedForHiggs, "Candidate-3 remains rejected as internal-only Higgs mass identity evidence."),
    new CensusCheck("branch-stable-non-c0-scalar-candidate-present", branchStableNonC0Count > 0, "No branch-stable non-C0 scalar candidate exists."),
    new CensusCheck("massive-scalar-profile-present", massiveScalarProfileCount > 0, "No target-independent massive scalar profile exists in the current registry."),
    new CensusCheck("target-independent-construction", targetIndependentConstruction, "This census excludes the physical Higgs mass target from construction."),
};

string terminalStatus = censusPromotable
    ? "higgs-scalar-source-operator-census-promotable"
    : "higgs-scalar-source-operator-census-blocked-no-solved-scalar-source";

var result = new
{
    phaseId = "phase189-higgs-scalar-source-operator-census",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    censusPromotable,
    predictionAttemptAllowed = censusPromotable && p187Validated,
    checks,
    candidateSummary = new
    {
        candidateCount = candidates.Length,
        scalarFeatureEnvelopeCount,
        branchStableNonC0Count,
        massiveScalarProfileCount,
        maxAbsMassLike,
        maxAbsGaugeLeak,
    },
    higgsReadiness = new
    {
        readinessStatus = JsonString(higgsReadiness, "readinessStatus"),
        claimGateStatus = JsonString(higgsReadiness, "claimGateStatus"),
        closureRequirements = StringArray(higgsReadiness, "closureRequirements"),
    },
    topCandidates = candidates
        .OrderByDescending(candidate => candidate.BranchStabilityScore)
        .ThenByDescending(candidate => candidate.RefinementStabilityScore)
        .Take(12)
        .ToArray(),
    decision = censusPromotable
        ? "A target-independent Higgs scalar source/operator census is promotable; rerun P187 and route gates."
        : "Do not predict Higgs mass. The missing artifact is a solved scalar source/operator with identity features, massive profile, and stability sidecars.",
    nextRequiredArtifact = "Solved scalar-sector source/operator with target-independent Higgs identity envelopes, a massive scalar profile, and branch/refinement/environment/representation/coupling stability sidecars.",
    sourceEvidence = new
    {
        registryPath = RegistryPath,
        phase51Path = Phase51Path,
        phase70Path = Phase70Path,
        phase106Path = Phase106Path,
        phase187Path = Phase187Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "higgs_scalar_source_operator_census.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "higgs_scalar_source_operator_census_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.censusPromotable,
        result.predictionAttemptAllowed,
        result.checks,
        result.candidateSummary,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"censusPromotable={censusPromotable}");
Console.WriteLine($"scalarFeatureEnvelopeCount={scalarFeatureEnvelopeCount}");
Console.WriteLine($"massiveScalarProfileCount={massiveScalarProfileCount}");

static string RequiredString(JsonElement element, string propertyName) =>
    JsonString(element, propertyName) ?? throw new InvalidDataException($"Missing string property '{propertyName}'.");
static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;
static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;
static double? JsonDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var value) ? value : null;
static IReadOnlyList<double> NumberArray(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Array
        ? property.EnumerateArray().Where(item => item.ValueKind == JsonValueKind.Number).Select(item => item.GetDouble()).ToArray()
        : Array.Empty<double>();
static IReadOnlyList<string> StringArray(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Array
        ? property.EnumerateArray().Where(item => item.ValueKind == JsonValueKind.String).Select(item => item.GetString() ?? "").ToArray()
        : Array.Empty<string>();

sealed record ScalarOperatorCandidate(
    string CandidateId,
    string ClaimClass,
    double BranchStabilityScore,
    double RefinementStabilityScore,
    bool PolarizationEnvelopePresent,
    bool SymmetryEnvelopePresent,
    bool InteractionProxyEnvelopePresent,
    IReadOnlyList<double> MassLikeEnvelope,
    IReadOnlyList<double> GaugeLeakEnvelope);

sealed record CensusCheck(string CheckId, bool Passed, string Detail);
