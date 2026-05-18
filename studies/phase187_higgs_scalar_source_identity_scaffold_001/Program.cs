using System.Text.Json;

const string DefaultOutputDir = "studies/phase187_higgs_scalar_source_identity_scaffold_001/output";
const string RegistryPath = "studies/phase12_joined_calculation_001/output/background_family/bosons/registry.json";
const string Phase51Path = "studies/phase51_broad_boson_prediction_readiness_001/broad_boson_prediction_readiness.json";
const string Phase70Path = "studies/phase70_scalar_sector_bridge_evidence_001/scalar_sector_bridge_evidence.json";
const string Phase106Path = "studies/phase106_candidate3_observable_identity_derivation_001/output/candidate3_observable_identity_derivation.json";
const string Phase173Path = "studies/phase173_next_prediction_route_selection_001/output/next_prediction_route_selection.json";
const string Phase184Path = "studies/phase184_massive_boson_prediction_closure_001/output/massive_boson_prediction_closure.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE187_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var registry = JsonDocument.Parse(File.ReadAllText(RegistryPath));
using var phase51 = JsonDocument.Parse(File.ReadAllText(Phase51Path));
using var phase70 = JsonDocument.Parse(File.ReadAllText(Phase70Path));
using var phase106 = JsonDocument.Parse(File.ReadAllText(Phase106Path));
using var phase173 = JsonDocument.Parse(File.ReadAllText(Phase173Path));
using var phase184 = JsonDocument.Parse(File.ReadAllText(Phase184Path));

var candidates = registry.RootElement.GetProperty("candidates").EnumerateArray()
    .Select(candidate => new ScalarCandidateAudit(
        RequiredString(candidate, "candidateId"),
        JsonString(candidate, "claimClass") ?? "unknown",
        JsonDouble(candidate, "branchStabilityScore") ?? 0.0,
        candidate.TryGetProperty("polarizationEnvelope", out var polarizationEnvelope) && polarizationEnvelope.ValueKind != JsonValueKind.Null,
        candidate.TryGetProperty("symmetryEnvelope", out var symmetryEnvelope) && symmetryEnvelope.ValueKind != JsonValueKind.Null,
        candidate.TryGetProperty("interactionProxyEnvelope", out var interactionProxyEnvelope) && interactionProxyEnvelope.ValueKind != JsonValueKind.Null,
        NumberArray(candidate, "massLikeEnvelope"),
        NumberArray(candidate, "gaugeLeakEnvelope"),
        StringArray(candidate, "contributingModeIds")))
    .OrderBy(candidate => candidate.CandidateId, StringComparer.Ordinal)
    .ToArray();

var higgsReadiness = phase51.RootElement.GetProperty("records").EnumerateArray()
    .Single(record => RequiredString(record, "particleId") == "higgs");
var higgsRoute = phase173.RootElement.GetProperty("routeAssessments").EnumerateArray()
    .Single(route => RequiredString(route, "routeId") == "higgs-scalar-mass");
var p184HiggsRow = phase184.RootElement.GetProperty("rows").EnumerateArray()
    .Single(row => RequiredString(row, "particleId") == "higgs");
bool scalarBridgeDerived = string.Equals(JsonString(phase70.RootElement, "terminalStatus"), "scalar-sector-bridge-evidence-derived", StringComparison.Ordinal);
bool candidate3RejectedForHiggs = phase106.RootElement.GetProperty("identityCandidates").EnumerateArray()
    .Any(candidate => RequiredString(candidate, "physicalObservableId") == "higgs-mass"
        && RequiredString(candidate, "status") == "rejected");
bool anyScalarFeatureEnvelopePresent = candidates.Any(candidate =>
    candidate.PolarizationEnvelopePresent || candidate.SymmetryEnvelopePresent || candidate.InteractionProxyEnvelopePresent);
bool anyBranchStableCandidate = candidates.Any(candidate =>
    candidate.BranchStabilityScore >= 0.5 && !string.Equals(candidate.ClaimClass, "C0_NumericalMode", StringComparison.Ordinal));
bool anyMassiveScalarCandidate = candidates.Any(candidate =>
    candidate.MassLikeEnvelope.Any(value => Math.Abs(value) >= 0.01 && Math.Abs(value) <= 100.0)
    && candidate.GaugeLeakEnvelope.All(value => Math.Abs(value) <= 0.10));
bool routeSourcePresent = JsonBool(higgsRoute, "sourceEvidencePresent") is true;
bool routeIdentityPresent = JsonBool(higgsRoute, "identityEvidencePresent") is true;
bool routeStabilityPresent = JsonBool(higgsRoute, "stabilityEvidencePresent") is true;
bool higgsPredictionAttemptAllowed = JsonBool(higgsRoute, "predictionAttemptAllowed") is true;

var readinessChecks = new[]
{
    new ReadinessCheck("scalar-vev-bridge-present", scalarBridgeDerived, "P70 provides electroweak VEV/order-parameter bridge evidence, not Higgs excitation identity."),
    new ReadinessCheck("scalar-feature-envelopes-present", anyScalarFeatureEnvelopePresent, "Phase12 registry lacks polarization/symmetry/interaction envelopes for Higgs identity features."),
    new ReadinessCheck("branch-stable-scalar-candidate-present", anyBranchStableCandidate, "Registry candidates are branch-fragile C0 numerical modes."),
    new ReadinessCheck("massive-scalar-candidate-present", anyMassiveScalarCandidate, "No current candidate has a target-independent massive-scalar envelope in the internal profile range."),
    new ReadinessCheck("candidate3-higgs-identity-accepted", !candidate3RejectedForHiggs, "Candidate-3 Higgs identity was explicitly rejected as internal-only."),
    new ReadinessCheck("higgs-source-evidence-present", routeSourcePresent, "P173 Higgs route has no source evidence."),
    new ReadinessCheck("higgs-identity-evidence-present", routeIdentityPresent, "P173 Higgs route has no identity evidence."),
    new ReadinessCheck("higgs-stability-evidence-present", routeStabilityPresent, "P173 Higgs route has no stability evidence."),
    new ReadinessCheck("higgs-prediction-attempt-allowed", higgsPredictionAttemptAllowed, "P173 Higgs route remains closed."),
};

bool scaffoldMaterialized = true;
bool higgsSourceIdentityValidated = routeSourcePresent
    && routeIdentityPresent
    && routeStabilityPresent
    && anyScalarFeatureEnvelopePresent
    && anyBranchStableCandidate
    && anyMassiveScalarCandidate
    && !candidate3RejectedForHiggs;
string terminalStatus = higgsSourceIdentityValidated
    ? "higgs-scalar-source-identity-scaffold-validated"
    : "higgs-scalar-source-identity-scaffold-materialized-blocked";

var result = new
{
    phaseId = "phase187-higgs-scalar-source-identity-scaffold",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    scaffoldMaterialized,
    higgsSourceIdentityValidated,
    predictionAttemptAllowed = higgsSourceIdentityValidated && higgsPredictionAttemptAllowed,
    scalarInternalProfile = new
    {
        profileId = "internal-massive-scalar",
        expectedMassRange = new[] { 0.01, 100.0 },
        expectedMultiplicity = 1,
        expectedPolarizationType = "scalar",
        expectedSymmetryGroup = "trivial",
        maxGaugeLeakForCompatibility = 0.10,
    },
    readinessChecks,
    candidateAudit = candidates,
    higgsReadiness = new
    {
        readinessStatus = JsonString(higgsReadiness, "readinessStatus"),
        claimGateStatus = JsonString(higgsReadiness, "claimGateStatus"),
        closureRequirements = StringArray(higgsReadiness, "closureRequirements"),
    },
    routeAudit = new
    {
        diagnosticObservablePresent = JsonBool(higgsRoute, "diagnosticObservablePresent"),
        targetContractPresent = JsonBool(higgsRoute, "targetContractPresent"),
        identityEvidencePresent = JsonBool(higgsRoute, "identityEvidencePresent"),
        sourceEvidencePresent = JsonBool(higgsRoute, "sourceEvidencePresent"),
        stabilityEvidencePresent = JsonBool(higgsRoute, "stabilityEvidencePresent"),
        predictionAttemptAllowed = JsonBool(higgsRoute, "predictionAttemptAllowed"),
        promotionAllowed = JsonBool(higgsRoute, "promotionAllowed"),
    },
    p184HiggsRow = p184HiggsRow.Clone(),
    decision = higgsSourceIdentityValidated
        ? "Higgs scalar source/identity sidecar is validated; rerun route gates before any mass comparison."
        : "Do not predict Higgs mass. The scaffold is now materialized, but source/operator, identity features, and stability sidecars remain absent.",
    nextRequiredArtifact = "A solved scalar-sector source/operator with target-independent Higgs identity features and branch/refinement/environment/representation/coupling stability sidecars.",
    sourceEvidence = new
    {
        registryPath = RegistryPath,
        phase51Path = Phase51Path,
        phase70Path = Phase70Path,
        phase106Path = Phase106Path,
        phase173Path = Phase173Path,
        phase184Path = Phase184Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "higgs_scalar_source_identity_scaffold.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "higgs_scalar_source_identity_scaffold_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.scaffoldMaterialized,
        result.higgsSourceIdentityValidated,
        result.predictionAttemptAllowed,
        result.readinessChecks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"scaffoldMaterialized={scaffoldMaterialized}");
Console.WriteLine($"higgsSourceIdentityValidated={higgsSourceIdentityValidated}");
Console.WriteLine($"predictionAttemptAllowed={result.predictionAttemptAllowed}");

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

sealed record ScalarCandidateAudit(
    string CandidateId,
    string ClaimClass,
    double BranchStabilityScore,
    bool PolarizationEnvelopePresent,
    bool SymmetryEnvelopePresent,
    bool InteractionProxyEnvelopePresent,
    IReadOnlyList<double> MassLikeEnvelope,
    IReadOnlyList<double> GaugeLeakEnvelope,
    IReadOnlyList<string> ContributingModeIds);

sealed record ReadinessCheck(string CheckId, bool Passed, string Detail);
