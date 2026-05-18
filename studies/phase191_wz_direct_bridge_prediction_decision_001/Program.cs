using System.Text.Json;

const string DefaultOutputDir = "studies/phase191_wz_direct_bridge_prediction_decision_001/output";
const string Phase190Path = "studies/phase190_wz_direct_target_independent_geometric_bridge_source_law_001/output/wz_direct_target_independent_geometric_bridge_source_law_summary.json";
const string Phase110Path = "studies/phase110_wz_absolute_repair_execution_contract_001/output/wz_absolute_repair_execution_contract.json";
const string Phase151Path = "studies/phase151_validated_boson_prediction_generator_001/output/validated_boson_predictions.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE191_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase190 = JsonDocument.Parse(File.ReadAllText(Phase190Path));
using var phase110 = JsonDocument.Parse(File.ReadAllText(Phase110Path));
using var phase151 = JsonDocument.Parse(File.ReadAllText(Phase151Path));

var siblingStability = phase190.RootElement.GetProperty("siblingStability");
var bestCandidate = siblingStability.GetProperty("bestCandidate");
double targetRaw = phase110.RootElement.GetProperty("repairTarget").GetProperty("targetImpliedRawMatrixElementMagnitude").GetDouble();
double rawGateRatio = 0.95;
double sigmaThreshold = 5.0;

bool candidateLawConstructed = JsonBool(phase190.RootElement, "candidateLawConstructed") is true;
bool theoremClaimed = JsonBool(phase190.RootElement, "theoremClaimed") is true;
bool targetObservablesUsed = JsonBool(phase190.RootElement, "targetObservablesUsed") is true;
int stableCandidateCount = JsonInt(siblingStability, "stableCandidateCount") ?? 0;
bool stableCandidatePresent = stableCandidateCount > 0;
double bestMeanMagnitude = RequiredDouble(bestCandidate, "meanMagnitude");
double bestRawToTargetRatio = bestMeanMagnitude / targetRaw;
bool rawGatePassed = bestRawToTargetRatio >= rawGateRatio;
bool wZParticleSplitPresent = false;
bool targetComparisonRunnable = theoremClaimed && rawGatePassed && wZParticleSplitPresent;

var existingWzRows = phase151.RootElement.GetProperty("failedAttempts")
    .EnumerateArray()
    .Where(row => RequiredString(row, "particleId") is "w-boson" or "z-boson")
    .Select(row => new ExistingComparisonRow(
        RequiredString(row, "particleId"),
        RequiredString(row, "observableId"),
        JsonDouble(row, "predictedValue"),
        RequiredDouble(row, "targetValue"),
        RequiredDouble(row, "pullOrSigmaResidual"),
        JsonBool(row, "passed") is true,
        JsonBool(row, "promotionAllowed") is true))
    .ToArray();

bool existingWzPredictionPromoted = existingWzRows.Length == 2 && existingWzRows.All(row => row.Passed && row.PromotionAllowed);
bool canCompleteSuccessfulPrediction = targetComparisonRunnable && existingWzPredictionPromoted;
string terminalStatus = canCompleteSuccessfulPrediction
    ? "wz-direct-bridge-prediction-successful"
    : stableCandidatePresent
        ? "wz-direct-bridge-prediction-blocked-stable-candidate-insufficient"
        : "wz-direct-bridge-prediction-blocked-no-stable-candidate";

var checks = new[]
{
    new DecisionCheck("p190-candidate-law-constructed", candidateLawConstructed, "P190 emitted a direct branch-local law candidate."),
    new DecisionCheck("p190-target-independent", !targetObservablesUsed, "P190 did not use W/Z target values during construction."),
    new DecisionCheck("p190-stable-candidate-present", stableCandidatePresent, "At least one P190 candidate is stable across sibling backgrounds."),
    new DecisionCheck("p190-theorem-or-derivation-promoted", theoremClaimed, "The manuscript still does not provide a unique derivation-backed W/Z bridge-source theorem."),
    new DecisionCheck("raw-amplitude-gate", rawGatePassed, $"Best P190 mean raw magnitude / target-implied raw magnitude is {bestRawToTargetRatio:R}; required >= {rawGateRatio:R}."),
    new DecisionCheck("wz-particle-specific-source-split", wZParticleSplitPresent, "P190 evaluates the P172 W/Z-like fermion pair but does not derive separate W and Z source rows."),
    new DecisionCheck("target-comparison-runnable", targetComparisonRunnable, "A physical W/Z target comparison requires a promoted derivation, raw gate pass, and particle-specific W/Z rows."),
    new DecisionCheck("existing-wz-comparison-promoted", existingWzPredictionPromoted, "Existing W/Z absolute-mass rows remain failed attempts, not validated predictions."),
};

var result = new
{
    phaseId = "phase191-wz-direct-bridge-prediction-decision",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    canCompleteSuccessfulPrediction,
    predictionCompleted = false,
    targetValuesUsedForConstruction = false,
    targetValuesUsedForValidation = true,
    p190BestCandidate = new
    {
        candidateId = RequiredString(bestCandidate, "candidateId"),
        meanMagnitude = bestMeanMagnitude,
        relativeSpread = RequiredDouble(bestCandidate, "relativeSpread"),
        complexAlignment = RequiredDouble(bestCandidate, "complexAlignment"),
    },
    gates = new
    {
        targetRaw,
        rawGateRatio,
        bestRawToTargetRatio,
        rawGatePassed,
        theoremClaimed,
        stableCandidateCount,
        wZParticleSplitPresent,
        sigmaThreshold,
        targetComparisonRunnable,
        existingWzPredictionPromoted,
    },
    existingWzRows,
    checks,
    decision = canCompleteSuccessfulPrediction
        ? "A successful W/Z absolute-mass prediction can be completed."
        : stableCandidatePresent
            ? "Do not complete or promote a W/Z absolute-mass prediction from P190. It is a stable target-independent branch-local source candidate, but it is not derivation-promoted, does not clear the raw-amplitude gate, and does not derive separate W/Z prediction rows."
            : "Do not complete or promote a W/Z absolute-mass prediction from P190. After branch-local finite-difference replay, no candidate passes sibling stability; the best candidate also is not derivation-promoted, does not clear the raw-amplitude gate, and does not derive separate W/Z prediction rows.",
    nextRequiredArtifact = "A derivation-backed direct W/Z bridge-source theorem or branch-local proof discharging the mixed-linearization obligation, with particle-specific W/Z source rows that clear raw-amplitude, common-scale, and target-comparison gates without target-fitted construction.",
    sourceEvidence = new
    {
        phase190Path = Phase190Path,
        phase110Path = Phase110Path,
        phase151Path = Phase151Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "wz_direct_bridge_prediction_decision.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "wz_direct_bridge_prediction_decision_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.canCompleteSuccessfulPrediction,
        result.predictionCompleted,
        result.p190BestCandidate,
        result.gates,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"canCompleteSuccessfulPrediction={canCompleteSuccessfulPrediction}");
Console.WriteLine($"bestRawToTargetRatio={bestRawToTargetRatio:R}");
Console.WriteLine($"rawGatePassed={rawGatePassed}");
Console.WriteLine($"theoremClaimed={theoremClaimed}");
Console.WriteLine($"wZParticleSplitPresent={wZParticleSplitPresent}");

static string RequiredString(JsonElement element, string propertyName) =>
    JsonString(element, propertyName) ?? throw new InvalidDataException($"Missing string property '{propertyName}'.");
static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;
static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;
static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;
static double RequiredDouble(JsonElement element, string propertyName) =>
    JsonDouble(element, propertyName) ?? throw new InvalidDataException($"Missing numeric property '{propertyName}'.");
static double? JsonDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var value) ? value : null;

sealed record DecisionCheck(string CheckId, bool Passed, string Detail);
sealed record ExistingComparisonRow(
    string ParticleId,
    string ObservableId,
    double? PredictedValue,
    double TargetValue,
    double PullOrSigmaResidual,
    bool Passed,
    bool PromotionAllowed);
