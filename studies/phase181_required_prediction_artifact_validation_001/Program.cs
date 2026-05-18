using System.Text.Json;

const string DefaultOutputDir = "studies/phase181_required_prediction_artifact_validation_001/output";
const string Phase149Path = "studies/phase149_known_boson_predictability_contracts_001/output/known_boson_predictability_contracts.json";
const string Phase173Path = "studies/phase173_next_prediction_route_selection_001/output/next_prediction_route_selection.json";
const string Phase182Path = "studies/phase182_wz_operator_algebra_bridge_source_001/output/wz_operator_algebra_bridge_source.json";
const string Phase175Path = "studies/phase175_massless_gauge_identity_split_feasibility_001/output/massless_gauge_identity_split_feasibility.json";
const string Phase176Path = "studies/phase176_massless_cartan_line_stability_001/output/massless_cartan_line_stability.json";
const string Phase177Path = "studies/phase177_massless_benchmark_contracts_001/output/massless_benchmark_contracts.json";
const string Phase178Path = "studies/phase178_protected_massless_subspace_transport_001/output/protected_massless_subspace_transport.json";
const string Phase179Path = "studies/phase179_gauge_frame_aligned_subspace_transport_001/output/gauge_frame_aligned_subspace_transport.json";
const string Phase180Path = "studies/phase180_prediction_blocker_root_cause_procedure_001/output/prediction_blocker_root_cause_procedure.json";
const string Phase183Path = "studies/phase183_massless_sector_invariant_prediction_001/output/massless_sector_invariant_prediction.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE181_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase149 = JsonDocument.Parse(File.ReadAllText(Phase149Path));
using var phase173 = JsonDocument.Parse(File.ReadAllText(Phase173Path));
using var phase182 = File.Exists(Phase182Path) ? JsonDocument.Parse(File.ReadAllText(Phase182Path)) : null;
using var phase175 = JsonDocument.Parse(File.ReadAllText(Phase175Path));
using var phase176 = JsonDocument.Parse(File.ReadAllText(Phase176Path));
using var phase177 = JsonDocument.Parse(File.ReadAllText(Phase177Path));
using var phase178 = JsonDocument.Parse(File.ReadAllText(Phase178Path));
using var phase179 = JsonDocument.Parse(File.ReadAllText(Phase179Path));
using var phase180 = JsonDocument.Parse(File.ReadAllText(Phase180Path));
using var phase183 = File.Exists(Phase183Path) ? JsonDocument.Parse(File.ReadAllText(Phase183Path)) : null;

var routeAssessments = phase173.RootElement.GetProperty("routeAssessments").EnumerateArray()
    .ToDictionary(route => RequiredString(route, "routeId"), route => route.Clone(), StringComparer.Ordinal);

var contractRows = phase149.RootElement.GetProperty("contracts").EnumerateArray()
    .ToDictionary(contract => RequiredString(contract, "particleId"), contract => contract.Clone(), StringComparer.Ordinal);

var artifactChecks = new[]
{
    new ArtifactCheck(
        ArtifactClass: "wz-bridge-revision",
        RequiredForParticles: new[] { "w-boson", "z-boson" },
        Present: JsonBool(routeAssessments["wz-absolute-mass"], "sourceEvidencePresent") is true
            || (phase182 is not null && JsonBool(phase182.RootElement, "bridgeRevisionArtifactValidated") is true),
        Stable: JsonBool(routeAssessments["wz-absolute-mass"], "stabilityEvidencePresent") is true
            || (phase182 is not null && (JsonInt(phase182.RootElement, "stableRawGatePassingCandidateCount") ?? 0) > 0),
        Validated: JsonBool(routeAssessments["wz-absolute-mass"], "predictionAttemptAllowed") is true
            || JsonBool(routeAssessments["wz-absolute-mass"], "promotionAllowed") is true
            || (phase182 is not null && JsonBool(phase182.RootElement, "bridgeRevisionArtifactValidated") is true),
        Evidence: phase182 is null ? "P173 W/Z route source/stability gates" : "P173 W/Z route source/stability gates plus P182 operator-algebra construction gate"),
    new ArtifactCheck(
        ArtifactClass: "higgs-scalar-source-and-identity",
        RequiredForParticles: new[] { "higgs" },
        Present: JsonBool(routeAssessments["higgs-scalar-mass"], "diagnosticObservablePresent") is true
            && JsonBool(routeAssessments["higgs-scalar-mass"], "identityEvidencePresent") is true,
        Stable: JsonBool(routeAssessments["higgs-scalar-mass"], "stabilityEvidencePresent") is true,
        Validated: JsonBool(routeAssessments["higgs-scalar-mass"], "predictionAttemptAllowed") is true
            || JsonBool(routeAssessments["higgs-scalar-mass"], "promotionAllowed") is true,
        Evidence: "P173 Higgs route diagnostic/identity/stability gates"),
    new ArtifactCheck(
        ArtifactClass: "photon-u1-identity-with-stable-transport-or-masslessness-invariant",
        RequiredForParticles: new[] { "photon" },
        Present: IdentitySplitBool(phase175.RootElement, "u1IdentityCandidatePresent")
            || JsonBool(phase176.RootElement, "u1CartanLineCandidatePresent") is true
            || (phase183 is not null && JsonBool(phase183.RootElement, "knownMasslessPredictionAllowed") is true),
        Stable: JsonBool(phase178.RootElement, "transportStable") is true
            || JsonBool(phase179.RootElement, "alignedTransportStable") is true
            || (phase183 is not null && JsonBool(phase183.RootElement, "sectorInvariantValidated") is true),
        Validated: JsonBool(routeAssessments["massless-gauge-photon-gluon"], "predictionAttemptAllowed") is true
            || JsonBool(routeAssessments["massless-gauge-photon-gluon"], "promotionAllowed") is true
            || (phase183 is not null && JsonBool(phase183.RootElement, "knownMasslessPredictionAllowed") is true),
        Evidence: "P175/P176 identity checks plus P178/P179 transport checks, or P183 protected-sector zero masslessness invariant"),
    new ArtifactCheck(
        ArtifactClass: "gluon-color-octet-identity-with-stable-transport-or-masslessness-invariant",
        RequiredForParticles: new[] { "gluon" },
        Present: IdentitySplitBool(phase175.RootElement, "colorOctetIdentityCandidatePresent")
            || (phase183 is not null && JsonBool(phase183.RootElement, "knownMasslessPredictionAllowed") is true),
        Stable: JsonBool(phase178.RootElement, "transportStable") is true
            || JsonBool(phase179.RootElement, "alignedTransportStable") is true
            || (phase183 is not null && JsonBool(phase183.RootElement, "sectorInvariantValidated") is true),
        Validated: JsonBool(routeAssessments["massless-gauge-photon-gluon"], "predictionAttemptAllowed") is true
            || JsonBool(routeAssessments["massless-gauge-photon-gluon"], "promotionAllowed") is true
            || (phase183 is not null && JsonBool(phase183.RootElement, "knownMasslessPredictionAllowed") is true),
        Evidence: "P175 color-octet check plus P178/P179 transport checks, or P183 protected-sector zero masslessness invariant"),
};

bool benchmarkContractsPresent = JsonBool(phase177.RootElement, "allBenchmarkContractsPresent") is true;
bool p180AllowsAttempt = JsonBool(phase180.RootElement, "procedureAllowsPredictionAttempt") is true;
bool anyValidatedArtifact = artifactChecks.Any(check => check.Validated);
bool rerunBosonPredictionJustified = p180AllowsAttempt && anyValidatedArtifact;
string terminalStatus = rerunBosonPredictionJustified
    ? "required-prediction-artifact-validated-rerun-justified"
    : "required-prediction-artifact-validation-failed-no-rerun";

var result = new
{
    phaseId = "phase181-required-prediction-artifact-validation",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    benchmarkContractsPresent,
    p180AllowsAttempt,
    anyValidatedArtifact,
    rerunBosonPredictionJustified,
    artifactChecks,
    contractReadiness = new
    {
        phase149Status = JsonString(phase149.RootElement, "terminalStatus"),
        allContractsReady = JsonBool(phase149.RootElement, "allContractsReady"),
        readyContractCount = JsonInt(phase149.RootElement, "readyContractCount"),
        openContractCount = JsonInt(phase149.RootElement, "openContractCount"),
        wContractReady = ContractReady(contractRows, "w-boson"),
        zContractReady = ContractReady(contractRows, "z-boson"),
        higgsContractReady = ContractReady(contractRows, "higgs"),
        photonContractReady = ContractReady(contractRows, "photon"),
        gluonContractReady = ContractReady(contractRows, "gluon"),
    },
    decision = rerunBosonPredictionJustified
        ? "validated artifact exists; rerun ./scripts/generate_validated_boson_predictions.sh"
        : "no required artifact class validated; do not rerun a boson prediction attempt because it would repeat the same fail-closed result",
    nextRequiredInput = rerunBosonPredictionJustified
        ? "The rerun gate is open for the protected-sector masslessness invariant; rerun the generator and keep stronger photon/gluon identity claims out of scope."
        : "Provide or derive one target-independent source/identity artifact that makes at least one P173 route predictionAttemptAllowed=true.",
    sourceEvidence = new
    {
        phase149Path = Phase149Path,
        phase173Path = Phase173Path,
        phase182Path = File.Exists(Phase182Path) ? Phase182Path : null,
        phase175Path = Phase175Path,
        phase176Path = Phase176Path,
        phase177Path = Phase177Path,
        phase178Path = Phase178Path,
        phase179Path = Phase179Path,
        phase180Path = Phase180Path,
        phase183Path = File.Exists(Phase183Path) ? Phase183Path : null,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "required_prediction_artifact_validation.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "required_prediction_artifact_validation_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.benchmarkContractsPresent,
        result.p180AllowsAttempt,
        result.anyValidatedArtifact,
        result.rerunBosonPredictionJustified,
        result.artifactChecks,
        result.contractReadiness,
        result.decision,
        result.nextRequiredInput,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"anyValidatedArtifact={anyValidatedArtifact}");
Console.WriteLine($"rerunBosonPredictionJustified={rerunBosonPredictionJustified}");

static bool IdentitySplitBool(JsonElement root, string propertyName) =>
    root.TryGetProperty("identitySplitAudit", out var audit) && JsonBool(audit, propertyName) is true;
static bool ContractReady(IReadOnlyDictionary<string, JsonElement> contracts, string particleId) =>
    contracts.TryGetValue(particleId, out var contract) && JsonBool(contract, "predictabilityReady") is true;
static string RequiredString(JsonElement element, string propertyName) =>
    JsonString(element, propertyName) ?? throw new InvalidDataException($"Missing string property '{propertyName}'.");
static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;
static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;
static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

sealed record ArtifactCheck(
    string ArtifactClass,
    IReadOnlyList<string> RequiredForParticles,
    bool Present,
    bool Stable,
    bool Validated,
    string Evidence);
