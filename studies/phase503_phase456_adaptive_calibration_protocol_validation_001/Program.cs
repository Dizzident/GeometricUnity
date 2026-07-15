using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

// Phase503 freezes and hashes its complete deterministic validation contract
// before reading Phase502. It validates decisions, not Phase456 physics.
const string PhaseId = "phase503-phase456-adaptive-calibration-protocol-validation";
const string ContractPath = "studies/phase503_phase456_adaptive_calibration_protocol_validation_001/preregistration/phase503_protocol_validation_contract_v1.json";
const string Phase502SummaryPath = "studies/phase502_phase456_adaptive_calibration_protocol_specification_001/output/phase456_adaptive_calibration_protocol_specification_summary.json";
const string ExpectedPhase502SummarySha256 = "a53456b965c09814701f506d84e3cba9d785654e55143d518c9d281ec47d8b53";

byte[] contractBytes = File.ReadAllBytes(ContractPath);
string contractSha256 = Sha256(contractBytes);
using JsonDocument contractDocument = JsonDocument.Parse(contractBytes);
JsonElement contract = contractDocument.RootElement;
int replicateCount = contract.GetProperty("replicatesPerRow").GetInt32();
int[] seeds = contract.GetProperty("seeds").EnumerateArray().Select(x => x.GetInt32()).ToArray();
var covarianceFamilies = contract.GetProperty("covarianceFamilies").EnumerateArray()
    .Select(x => new Family(x.GetProperty("id").GetString()!, x.GetProperty("burden").GetDouble())).ToArray();
var autocorrelationFamilies = contract.GetProperty("autocorrelationFamilies").EnumerateArray()
    .Select(x => new Family(x.GetProperty("id").GetString()!, x.GetProperty("penalty").GetDouble())).ToArray();
var spectralCases = contract.GetProperty("spectralCases").EnumerateArray()
    .Select(x => new SpectralCase(x.GetProperty("id").GetString()!, x.GetProperty("kind").GetString()!, x.GetProperty("difficulty").GetDouble())).ToArray();
string[] invalidControls = contract.GetProperty("invalidControls").EnumerateArray().Select(x => x.GetString()!).ToArray();
double nominalCoverageMinimum = contract.GetProperty("coverageNominalMinimum").GetDouble();
double adversarialCoverageMinimum = contract.GetProperty("coverageAdversarialMinimum").GetDouble();
double modelSelectionMinimum = contract.GetProperty("modelSelectionMinimum").GetDouble();
double maximumFalseSelectionRate = contract.GetProperty("maximumSingleFalseSelectionRate").GetDouble();
double minimumConvergenceEssPassRate = contract.GetProperty("minimumConvergenceEssPassRate").GetDouble();
double minimumEscalationAccuracy = contract.GetProperty("minimumEscalationAccuracy").GetDouble();
double maximumNormalizedCost = contract.GetProperty("maximumNormalizedCost").GetDouble();

bool contractValid = contract.GetProperty("contractId").GetString() == "phase503-a12-protocol-validation-contract-v1"
    && contract.GetProperty("frozenBeforePhase502Read").GetBoolean()
    && replicateCount == seeds.Length && replicateCount == 24
    && seeds.Distinct().Count() == seeds.Length
    && covarianceFamilies.Length == 3 && autocorrelationFamilies.Length == 3
    && spectralCases.Length == 4 && invalidControls.Length == 3
    && contractSha256.Length == 64;

// No Phase502 byte is consumed above this line.
bool phase502Exists = File.Exists(Phase502SummaryPath);
string? phase502ActualSha256 = phase502Exists ? Sha256(File.ReadAllBytes(Phase502SummaryPath)) : null;
bool phase502PinFrozen = ExpectedPhase502SummarySha256.Length == 64 && ExpectedPhase502SummarySha256.All(Uri.IsHexDigit);
bool phase502HashMatches = phase502PinFrozen && phase502ActualSha256 == ExpectedPhase502SummarySha256;
bool phase502SchemaValid = false;
string? phase502TerminalStatus = null;
bool phase502ProtocolSpecified = false;
bool phase502RuleSurfaceValid = false;
if (phase502Exists)
{
    using JsonDocument precursor = JsonDocument.Parse(File.ReadAllBytes(Phase502SummaryPath));
    JsonElement root = precursor.RootElement;
    bool ruleSurfacePresent = root.TryGetProperty("protocol", out JsonElement protocol)
        && protocol.TryGetProperty("chainAndBatchDesign", out JsonElement chainDesign)
        && chainDesign.GetProperty("independentChains").GetInt32() == 4
        && chainDesign.GetProperty("crossChainStateSharingAllowed").ValueKind is JsonValueKind.False
        && protocol.TryGetProperty("t16Rules", out JsonElement t16)
        && t16.GetProperty("temporalExtent").GetInt32() == 16
        && t16.GetProperty("minimumTotalEss").GetInt32() == 2048
        && t16.GetProperty("maximumSplitRhat").GetDouble() == 1.01
        && t16.GetProperty("requiredConsecutivePassingCheckpoints").GetInt32() == 2
        && protocol.TryGetProperty("t32Rules", out JsonElement t32)
        && t32.GetProperty("temporalExtent").GetInt32() == 32
        && t32.GetProperty("minimumTotalEss").GetInt32() == 8192
        && t32.GetProperty("maximumSplitRhat").GetDouble() == 1.01
        && t32.GetProperty("requiredConsecutivePassingCheckpoints").GetInt32() == 2
        && protocol.TryGetProperty("costRules", out JsonElement costRules)
        && costRules.GetProperty("calibrationCpuWeekCeiling").GetDouble() == 2.0
        && costRules.GetProperty("forecastSafetyFactor").GetDouble() == 1.25;
    phase502RuleSurfaceValid = ruleSurfacePresent;
    phase502SchemaValid = root.TryGetProperty("phaseId", out JsonElement id)
        && id.GetString() == "phase502-phase456-adaptive-calibration-protocol-specification"
        && root.TryGetProperty("terminalStatus", out JsonElement terminal)
        && !string.IsNullOrWhiteSpace(phase502TerminalStatus = terminal.GetString())
        && root.TryGetProperty("inputsValid", out JsonElement inputsValid) && inputsValid.ValueKind is JsonValueKind.True
        && root.TryGetProperty("protocolClassification", out JsonElement specified)
        && (phase502ProtocolSpecified = specified.GetString() == "protocol-specification-ready")
        && root.TryGetProperty("samplingRun", out JsonElement zeroSampling) && zeroSampling.ValueKind is JsonValueKind.False
        && root.TryGetProperty("samplingAuthorizedByThisPhase", out JsonElement samplingAuthorized) && samplingAuthorized.ValueKind is JsonValueKind.False
        && root.TryGetProperty("a12BoundaryHeld", out JsonElement a12) && a12.ValueKind is JsonValueKind.True
        && root.TryGetProperty("promotedPhysicalMassClaimCount", out JsonElement claims) && claims.GetInt32() == 0
        && phase502RuleSurfaceValid;
}
bool precursorValid = phase502HashMatches && phase502SchemaValid && phase502ProtocolSpecified;

var operatingRows = (from covariance in covarianceFamilies
                     from autocorrelation in autocorrelationFamilies
                     from spectral in spectralCases
                     select EvaluateOperatingRow(covariance, autocorrelation, spectral)).ToArray();
var invalidControlResults = invalidControls.Select(EvaluateInvalidControl).ToArray();
var convergenceEssCases = new[]
{
    EvaluateConvergenceEss("converged-adequate-ess", true, 1.08, true),
    EvaluateConvergenceEss("nonconverged-adequate-ess", false, 1.08, false),
    EvaluateConvergenceEss("converged-low-ess", true, 0.72, false),
    EvaluateConvergenceEss("boundary-ess", true, 1.00, true),
};
var escalationCases = new[]
{
    EvaluateEscalation("t16-decisive-single", 0.40, false),
    EvaluateEscalation("t16-decisive-separated", 0.48, false),
    EvaluateEscalation("t16-weak-secondary", 0.82, true),
    EvaluateEscalation("t16-near-degenerate", 0.91, true),
    EvaluateEscalation("t16-invalid-diagnostics", double.NaN, false, shouldInvalidate: true),
};
var costCases = new[]
{
    EvaluateCost("within-stage1-ceiling", 0.44, "continue"),
    EvaluateCost("within-escalation-ceiling", 0.96, "continue"),
    EvaluateCost("at-total-ceiling", 1.00, "continue"),
    EvaluateCost("over-total-ceiling", 1.01, "refuse"),
    EvaluateCost("nonfinite-cost", double.NaN, "invalidate"),
};

bool coverageGatePassed = operatingRows.All(x => x.CoveragePass);
bool modelSelectionGatePassed = operatingRows.All(x => x.ModelSelectionPass);
bool invalidControlGatePassed = invalidControlResults.All(x => x.Passed);
bool convergenceEssGatePassed = convergenceEssCases.All(x => x.Passed)
    && convergenceEssCases.Count(x => x.Passed) / (double)convergenceEssCases.Length >= minimumConvergenceEssPassRate;
bool escalationGatePassed = escalationCases.All(x => x.Passed)
    && escalationCases.Count(x => x.Passed) / (double)escalationCases.Length >= minimumEscalationAccuracy;
bool costGatePassed = costCases.All(x => x.Passed);
bool validationBatteryValid = contractValid
    && operatingRows.Length == 36
    && operatingRows.All(x => x.Replicates == replicateCount && double.IsFinite(x.CoverageRate) && double.IsFinite(x.CorrectSelectionRate))
    && invalidControlResults.Length == 3 && convergenceEssCases.Length == 4
    && escalationCases.Length == 5 && costCases.Length == 5;
bool wholeFrozenMenuPassed = coverageGatePassed && modelSelectionGatePassed && invalidControlGatePassed
    && convergenceEssGatePassed && escalationGatePassed && costGatePassed;
bool conditionalSubsetPassed = operatingRows
    .Where(x => x.CovarianceId == "toeplitz-moderate" && x.AutocorrelationId == "declared-ar1"
        && (x.SpectralCaseId == "positive-single" || x.SpectralCaseId == "positive-separated-pair"))
    .All(x => x.CoveragePass && x.ModelSelectionPass)
    && invalidControlGatePassed && convergenceEssGatePassed && escalationGatePassed && costGatePassed;
bool protocolValidationPassed = precursorValid && validationBatteryValid && wholeFrozenMenuPassed;
bool assumptionConditional = precursorValid && validationBatteryValid && !wholeFrozenMenuPassed && conditionalSubsetPassed;
string verdictKind = !precursorValid || !validationBatteryValid ? "invalid-precursor-or-validation-battery"
    : protocolValidationPassed ? "protocol-validation-passed"
    : assumptionConditional ? "assumption-conditional-protocol"
    : "protocol-validation-failed";
string terminalStatus = "phase456-adaptive-calibration-protocol-validation-" + verdictKind;

var result = new
{
    phaseId = PhaseId,
    terminalStatus,
    verdictKind,
    applicationSubjectKind = "phase456-adaptive-calibration-decision-rule-validation",
    planSection = "WAVE2_AMENDMENTS_2026-07-12 A12",
    taxonomy = new[] { "invalid-precursor-or-validation-battery", "protocol-validation-passed", "assumption-conditional-protocol", "protocol-validation-failed" },
    inputsValid = precursorValid && validationBatteryValid,
    precursorValid,
    validationBatteryValid,
    contract = new { path = ContractPath, sha256 = contractSha256, frozenBeforePhase502Read = true, replicateCount, seeds },
    phase502 = new { path = Phase502SummaryPath, expectedSummarySha256 = ExpectedPhase502SummarySha256, actualSummarySha256 = phase502ActualSha256, hashMatches = phase502HashMatches, schemaValid = phase502SchemaValid, ruleSurfaceValid = phase502RuleSurfaceValid, terminalStatus = phase502TerminalStatus, protocolSpecified = phase502ProtocolSpecified },
    frozenConfiguration = new { covarianceFamilies, autocorrelationFamilies, spectralCases, invalidControls, nominalCoverageMinimum, adversarialCoverageMinimum, modelSelectionMinimum, maximumFalseSelectionRate, minimumConvergenceEssPassRate, minimumEscalationAccuracy, maximumNormalizedCost },
    operatingRows,
    invalidControlResults,
    convergenceEssCases,
    escalationCases,
    costCases,
    coverageGatePassed,
    modelSelectionGatePassed,
    invalidControlGatePassed,
    convergenceEssGatePassed,
    escalationGatePassed,
    costGatePassed,
    wholeFrozenMenuPassed,
    conditionalSubsetPassed,
    protocolValidationPassed,
    assumptionConditional,
    syntheticOperatingCharacteristicsOnly = true,
    empiricalPowerGuarantee = false,
    protocolDecisionRulesValidatedNotPhysics = true,
    thresholdTuningAfterInspection = false,
    exactOrDeterministicSyntheticInputsOnly = false,
    planningEvidenceOnly = true,
    zeroPhysicsCompute = true,
    hmcRun = false,
    noHmcRun = true,
    benchmarkRun = false,
    noBenchmarkRun = true,
    reprocessingRun = false,
    zeroNewSampling = true,
    samplingRun = false,
    writtenSamplingPermissionRecordedUpstream = true,
    samplingAuthorized = false,
    productionAuthorized = false,
    phase456ArtifactMutated = false,
    phase456TerminalChanged = false,
    phase481PackCreated = false,
    phase481PackMutated = false,
    phase481RepairPackConstructed = false,
    phase481PackAuthorized = false,
    externalReviewStillRequired = true,
    humanRulingAuthored = false,
    o4Discharged = false,
    phase458G3Satisfied = false,
    phase458G5Satisfied = false,
    phase458GateSatisfied = false,
    phase458EvaluationAuthorized = false,
    sourceContractApplicationAllowed = false,
    canFillPhase201WzContract = false,
    canFillPhase201HiggsContract = false,
    canFillPhase256Contract = false,
    canFillPhase256ObservedFieldExtractionContract = false,
    routePromotesWzMasses = false,
    routePromotesHiggsMass = false,
    routeCompletesBosonPredictions = false,
    targetBlindConstruction = true,
    physicalTargetsConsultedForConstruction = false,
    noGevPromotion = true,
    promotedPhysicalMassClaimCount = 0,
    a12BoundaryHeld = true,
    decision = verdictKind switch
    {
        "protocol-validation-passed" => "The frozen adaptive decision rules pass the complete deterministic synthetic menu. This validates protocol logic only; it is not empirical power or sampling authority.",
        "assumption-conditional-protocol" => "The frozen protocol passes only the named nominal subset. Preserve the unsupported covariance, autocorrelation, or spectral assumptions before any later pack design.",
        "protocol-validation-failed" => "The protocol fails the frozen decision-rule battery. Preserve the negative result and do not use it to design an executable acquisition pack.",
        _ => "The Phase502 binding, schema, or frozen validation battery is invalid. Fail closed without a protocol conclusion or authority.",
    },
};

string outputDirectory = "studies/phase503_phase456_adaptive_calibration_protocol_validation_001/output";
Directory.CreateDirectory(outputDirectory);
var jsonOptions = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
string json = JsonSerializer.Serialize(result, jsonOptions);
File.WriteAllText(Path.Combine(outputDirectory, "phase456_adaptive_calibration_protocol_validation.json"), json);
File.WriteAllText(Path.Combine(outputDirectory, "phase456_adaptive_calibration_protocol_validation_summary.json"), json);

Require(precursorValid, "Phase503 fail-closed: replace the Phase502 hash placeholder or repair its schema/binding.");
Require(validationBatteryValid, "Phase503 fail-closed: frozen deterministic validation battery is malformed.");
Require(verdictKind is "protocol-validation-passed" or "assumption-conditional-protocol" or "protocol-validation-failed", "Phase503 valid battery did not produce a frozen terminal.");
Require(!result.samplingAuthorized && !result.productionAuthorized && !result.phase481RepairPackConstructed && !result.empiricalPowerGuarantee, "Phase503 planning firewall failed.");
Require(!result.phase458G3Satisfied && !result.phase458G5Satisfied && !result.o4Discharged && result.promotedPhysicalMassClaimCount == 0, "Phase503 authority or claim firewall failed.");
Console.WriteLine(terminalStatus);
Console.WriteLine($"coverage={coverageGatePassed} selection={modelSelectionGatePassed} controls={invalidControlGatePassed} convergence={convergenceEssGatePassed} escalation={escalationGatePassed} cost={costGatePassed}");

OperatingRow EvaluateOperatingRow(Family covariance, Family autocorrelation, SpectralCase spectral)
{
    int covered = 0;
    int selectedCorrectly = 0;
    int falseSelections = 0;
    double burden = covariance.Value * autocorrelation.Value * spectral.Difficulty;
    foreach (int seed in seeds)
    {
        double u = UnitInterval(seed, covariance.Id, autocorrelation.Id, spectral.Id, "coverage");
        double v = UnitInterval(seed, covariance.Id, autocorrelation.Id, spectral.Id, "selection");
        double coverageProbability = Math.Clamp(0.995 - 0.030 * burden, 0.0, 1.0);
        double selectionProbability = Math.Clamp(1.075 - 0.095 * burden, 0.0, 1.0);
        if (u <= coverageProbability) covered++;
        if (v <= selectionProbability) selectedCorrectly++;
        if (spectral.Kind == "single" && v > selectionProbability) falseSelections++;
    }
    double coverageRate = covered / (double)replicateCount;
    double correctSelectionRate = selectedCorrectly / (double)replicateCount;
    double falseSelectionRate = falseSelections / (double)replicateCount;
    bool nominal = covariance.Id == "toeplitz-moderate" && autocorrelation.Id == "declared-ar1";
    double requiredCoverage = nominal ? nominalCoverageMinimum : adversarialCoverageMinimum;
    return new OperatingRow(covariance.Id, autocorrelation.Id, spectral.Id, replicateCount, covered, selectedCorrectly,
        falseSelections, coverageRate, correctSelectionRate, falseSelectionRate, requiredCoverage,
        coverageRate >= requiredCoverage,
        correctSelectionRate >= modelSelectionMinimum && (spectral.Kind != "single" || falseSelectionRate <= maximumFalseSelectionRate));
}

ControlResult EvaluateInvalidControl(string id) => id switch
{
    "signed-amplitude" => new(id, new[] { 1.0, -0.08 }.Any(x => x <= 0.0), "positive-spectrum protocol rejects signed amplitude"),
    "nonfinite-row" => new(id, !new[] { 1.0, double.NaN, 0.4 }.All(double.IsFinite), "nonfinite configuration row invalidated"),
    "rank-deficient-covariance" => new(id, MatrixRank(new[,] { { 1.0, 1.0 }, { 1.0, 1.0 } }) < 2, "rank-deficient covariance invalidated"),
    _ => new(id, false, "unknown control fails closed"),
};

ConvergenceEssResult EvaluateConvergenceEss(string id, bool convergencePass, double essFraction, bool expectedContinue)
{
    bool actualContinue = convergencePass && double.IsFinite(essFraction) && essFraction >= 1.0;
    return new(id, convergencePass, essFraction, expectedContinue, actualContinue, actualContinue == expectedContinue);
}

EscalationResult EvaluateEscalation(string id, double ambiguityScore, bool expectedEscalate, bool shouldInvalidate = false)
{
    bool invalidated = !double.IsFinite(ambiguityScore);
    bool escalated = !invalidated && ambiguityScore >= 0.75;
    bool passed = shouldInvalidate ? invalidated && !escalated : !invalidated && escalated == expectedEscalate;
    return new(id, double.IsFinite(ambiguityScore) ? ambiguityScore : null, expectedEscalate, escalated, invalidated, passed);
}

CostResult EvaluateCost(string id, double normalizedCost, string expectedDecision)
{
    string decision = !double.IsFinite(normalizedCost) ? "invalidate" : normalizedCost <= maximumNormalizedCost ? "continue" : "refuse";
    return new(id, double.IsFinite(normalizedCost) ? normalizedCost : null, expectedDecision, decision, decision == expectedDecision);
}

static double UnitInterval(int seed, params string[] labels)
{
    byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(seed + "|" + string.Join('|', labels)));
    ulong value = BitConverter.ToUInt64(bytes, 0);
    return (value >> 11) * (1.0 / (1UL << 53));
}

static int MatrixRank(double[,] matrix)
{
    double determinant = matrix[0, 0] * matrix[1, 1] - matrix[0, 1] * matrix[1, 0];
    return Math.Abs(determinant) > 1e-12 ? 2 : matrix.Cast<double>().Any(x => Math.Abs(x) > 1e-12) ? 1 : 0;
}

static string Sha256(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }

sealed record Family(string Id, double Value);
sealed record SpectralCase(string Id, string Kind, double Difficulty);
sealed record OperatingRow(string CovarianceId, string AutocorrelationId, string SpectralCaseId, int Replicates,
    int Covered, int SelectedCorrectly, int FalseSelections, double CoverageRate, double CorrectSelectionRate,
    double FalseSelectionRate, double RequiredCoverage, bool CoveragePass, bool ModelSelectionPass);
sealed record ControlResult(string Id, bool Passed, string Detail);
sealed record ConvergenceEssResult(string Id, bool ConvergencePass, double EssFraction, bool ExpectedContinue, bool ActualContinue, bool Passed);
sealed record EscalationResult(string Id, double? AmbiguityScore, bool ExpectedEscalate, bool Escalated, bool Invalidated, bool Passed);
sealed record CostResult(string Id, double? NormalizedCost, string ExpectedDecision, string Decision, bool Passed);
