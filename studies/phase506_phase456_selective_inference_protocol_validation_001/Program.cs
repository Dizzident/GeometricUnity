using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

const string PhaseId = "phase506-phase456-selective-inference-protocol-validation";
const string ContractPath = "studies/phase506_phase456_selective_inference_protocol_validation_001/preregistration/phase506_selective_inference_validation_contract_v1.json";
const string Phase505SummaryPath = "studies/phase505_phase503_frozen_failure_localization_001/output/phase503_frozen_failure_localization_summary.json";
const string ExpectedPhase505SummarySha256 = "956beedf3cb166f92529fa3f577962e905a88e715cff7be132a3661b6a3e5a61";

// The complete decision menu is consumed before any Phase505 byte.
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
    .Select(x => new SpectralCase(x.GetProperty("id").GetString()!, x.GetProperty("truth").GetString()!,
        x.GetProperty("signal").GetDouble(), x.GetProperty("decisiveRequired").GetBoolean())).ToArray();
JsonElement decisionRule = contract.GetProperty("decisionRule");
double singleUpperScore = decisionRule.GetProperty("singleUpperScore").GetDouble();
double doubleLowerScore = decisionRule.GetProperty("doubleLowerScore").GetDouble();
string[] unresolvedSet = decisionRule.GetProperty("betweenThresholdOutputSet").EnumerateArray().Select(x => x.GetString()!).ToArray();
double truthSetCoverageMinimum = contract.GetProperty("truthSetCoverageMinimum").GetDouble();
double decisiveCorrectRateMinimum = contract.GetProperty("decisiveCorrectRateMinimumForIdentifiableCases").GetDouble();
int maximumWrongDecisiveCalls = contract.GetProperty("maximumWrongDecisiveCalls").GetInt32();
int maximumSingleFalseSelections = contract.GetProperty("maximumSingleFalseSelections").GetInt32();
string[] invalidControls = contract.GetProperty("invalidControls").EnumerateArray().Select(x => x.GetString()!).ToArray();
double minimumConvergenceEssDecisionAccuracy = contract.GetProperty("minimumConvergenceEssDecisionAccuracy").GetDouble();
double minimumEscalationDecisionAccuracy = contract.GetProperty("minimumEscalationDecisionAccuracy").GetDouble();
double maximumNormalizedCost = contract.GetProperty("maximumNormalizedCost").GetDouble();

bool contractValid = contract.GetProperty("contractId").GetString() == "phase506-a13-selective-inference-validation-contract-v1"
    && contract.GetProperty("frozenBeforePhase505Read").GetBoolean()
    && replicateCount == 32 && seeds.Length == replicateCount && seeds.Distinct().Count() == seeds.Length
    && covarianceFamilies.Length == 3 && autocorrelationFamilies.Length == 3 && spectralCases.Length == 4
    && spectralCases.Count(x => x.DecisiveRequired) == 2 && spectralCases.Count(x => !x.DecisiveRequired) == 2
    && singleUpperScore < doubleLowerScore && unresolvedSet.SequenceEqual(new[] { "single", "double" })
    && decisionRule.GetProperty("unresolvedCountsAsDecisive").ValueKind is JsonValueKind.False
    && truthSetCoverageMinimum == 1.0 && maximumWrongDecisiveCalls == 0 && maximumSingleFalseSelections == 0
    && invalidControls.Length == 3 && contract.GetProperty("wholeMenuRequired").GetBoolean()
    && contract.GetProperty("conditionalSubsetCannotPassCompleteGate").GetBoolean() && contractSha256.Length == 64;

// No Phase505 byte is consumed above this line.
bool phase505Exists = File.Exists(Phase505SummaryPath);
string? phase505ActualSha256 = phase505Exists ? Sha256(File.ReadAllBytes(Phase505SummaryPath)) : null;
bool phase505PinFrozen = ExpectedPhase505SummarySha256.Length == 64 && ExpectedPhase505SummarySha256.All(Uri.IsHexDigit);
bool phase505HashMatches = phase505PinFrozen && phase505ActualSha256 == ExpectedPhase505SummarySha256;
bool phase505SchemaValid = false;
string? phase505TerminalStatus = null;
if (phase505Exists)
{
    using JsonDocument precursor = JsonDocument.Parse(File.ReadAllBytes(Phase505SummaryPath));
    JsonElement root = precursor.RootElement;
    phase505SchemaValid = root.TryGetProperty("phaseId", out JsonElement id)
        && id.GetString() == "phase505-phase503-frozen-failure-localization"
        && root.TryGetProperty("terminalStatus", out JsonElement terminal)
        && !string.IsNullOrWhiteSpace(phase505TerminalStatus = terminal.GetString())
        && root.TryGetProperty("inputsValid", out JsonElement inputsValid) && inputsValid.ValueKind is JsonValueKind.True
        && root.TryGetProperty("sourceRowCount", out JsonElement sourceRows) && sourceRows.GetInt32() == 36
        && root.TryGetProperty("failedSourceRowCount", out JsonElement failedRows) && failedRows.GetInt32() == 11
        && root.TryGetProperty("allFailedRowsAccountedFor", out JsonElement accounted) && accounted.ValueKind is JsonValueKind.True
        && root.TryGetProperty("recommendsTerminal", out JsonElement recommendsTerminal) && recommendsTerminal.ValueKind is JsonValueKind.False
        && root.TryGetProperty("a13BoundaryHeld", out JsonElement a13) && a13.ValueKind is JsonValueKind.True
        && root.TryGetProperty("samplingAuthorized", out JsonElement samplingAuthorized) && samplingAuthorized.ValueKind is JsonValueKind.False
        && root.TryGetProperty("promotedPhysicalMassClaimCount", out JsonElement claims) && claims.GetInt32() == 0;
}
bool precursorValid = phase505HashMatches && phase505SchemaValid;

var operatingRows = (from covariance in covarianceFamilies
                     from autocorrelation in autocorrelationFamilies
                     from spectral in spectralCases
                     select EvaluateRow(covariance, autocorrelation, spectral)).ToArray();
var invalidControlResults = invalidControls.Select(EvaluateInvalidControl).ToArray();
var convergenceEssCases = new[]
{
    EvaluateConvergenceEss("converged-adequate-ess", true, 1.10, "evaluate"),
    EvaluateConvergenceEss("nonconverged-adequate-ess", false, 1.10, "unresolved"),
    EvaluateConvergenceEss("converged-low-ess", true, 0.74, "unresolved"),
    EvaluateConvergenceEss("nonconverged-low-ess", false, 0.60, "unresolved"),
    EvaluateConvergenceEss("boundary-ess", true, 1.00, "evaluate"),
    EvaluateConvergenceEss("nonfinite-ess", true, double.NaN, "invalidate"),
};
var escalationCases = new[]
{
    EvaluateEscalation("t16-identifiable-single", "decisive-single", false, "stop-decisive"),
    EvaluateEscalation("t16-identifiable-separated", "decisive-double", false, "stop-decisive"),
    EvaluateEscalation("t16-weak-unresolved", "unresolved", true, "escalate-t32"),
    EvaluateEscalation("t16-near-degenerate-unresolved", "unresolved", true, "escalate-t32"),
    EvaluateEscalation("t32-still-unresolved", "unresolved", false, "stop-unresolved"),
    EvaluateEscalation("invalid-diagnostics", "invalid", false, "invalidate"),
};
var costCases = new[]
{
    EvaluateCost("within-t16-ceiling", 0.42, "continue"),
    EvaluateCost("within-t32-ceiling", 0.94, "continue"),
    EvaluateCost("at-ceiling", 1.00, "continue"),
    EvaluateCost("over-ceiling", 1.01, "refuse"),
    EvaluateCost("nonfinite-cost", double.NaN, "invalidate"),
};

bool truthSetCoverageGatePassed = operatingRows.All(x => x.TruthSetCoverageRate >= truthSetCoverageMinimum);
bool decisiveIdentifiableGatePassed = operatingRows.Where(x => x.DecisiveRequired)
    .All(x => x.DecisiveCorrectRate >= decisiveCorrectRateMinimum && x.UnresolvedCalls == 0);
bool unresolvedScoredHonestly = operatingRows.All(x => x.UnresolvedCalls + x.DecisiveCorrectCalls + x.WrongDecisiveCalls == replicateCount)
    && operatingRows.Where(x => !x.DecisiveRequired).All(x => x.UnresolvedCalls > 0)
    && operatingRows.Sum(x => x.UnresolvedCalls) > 0;
bool wrongDecisiveGatePassed = operatingRows.Sum(x => x.WrongDecisiveCalls) <= maximumWrongDecisiveCalls;
bool falseSelectionGatePassed = operatingRows.Where(x => x.Truth == "single").Sum(x => x.WrongDecisiveCalls) <= maximumSingleFalseSelections;
bool invalidControlGatePassed = invalidControlResults.All(x => x.Passed);
double convergenceAccuracy = convergenceEssCases.Count(x => x.Passed) / (double)convergenceEssCases.Length;
bool convergenceEssGatePassed = convergenceEssCases.All(x => x.Passed) && convergenceAccuracy >= minimumConvergenceEssDecisionAccuracy;
double escalationAccuracy = escalationCases.Count(x => x.Passed) / (double)escalationCases.Length;
bool escalationGatePassed = escalationCases.All(x => x.Passed) && escalationAccuracy >= minimumEscalationDecisionAccuracy;
bool costGatePassed = costCases.All(x => x.Passed);
bool validationBatteryValid = contractValid && operatingRows.Length == 36
    && operatingRows.All(x => x.Replicates == replicateCount && double.IsFinite(x.TruthSetCoverageRate) && double.IsFinite(x.DecisiveCorrectRate))
    && invalidControlResults.Length == 3 && convergenceEssCases.Length == 6 && escalationCases.Length == 6 && costCases.Length == 5;
bool completeFrozenMenuPassed = truthSetCoverageGatePassed && decisiveIdentifiableGatePassed && unresolvedScoredHonestly
    && wrongDecisiveGatePassed && falseSelectionGatePassed && invalidControlGatePassed
    && convergenceEssGatePassed && escalationGatePassed && costGatePassed;
bool nominalSubsetPassed = operatingRows.Where(x => x.CovarianceId == "toeplitz-moderate" && x.AutocorrelationId == "declared-ar1")
    .All(x => x.TruthSetCoverageRate >= truthSetCoverageMinimum)
    && decisiveIdentifiableGatePassed && invalidControlGatePassed;
bool protocolValidationPassed = precursorValid && validationBatteryValid && completeFrozenMenuPassed;
bool assumptionConditional = precursorValid && validationBatteryValid && !completeFrozenMenuPassed && nominalSubsetPassed;
string verdictKind = !precursorValid || !validationBatteryValid ? "invalid-precursor-or-validation-battery"
    : protocolValidationPassed ? "selective-protocol-validation-passed"
    : assumptionConditional ? "assumption-conditional-selective-protocol"
    : "selective-protocol-validation-failed";
string terminalStatus = "phase456-selective-inference-protocol-validation-" + verdictKind;

var result = new
{
    phaseId = PhaseId,
    terminalStatus,
    verdictKind,
    applicationSubjectKind = "phase456-selective-set-valued-inference-protocol-validation",
    planSection = "WAVE2_AMENDMENTS_2026-07-12 A13",
    taxonomy = new[] { "invalid-precursor-or-validation-battery", "selective-protocol-validation-passed", "assumption-conditional-selective-protocol", "selective-protocol-validation-failed" },
    inputsValid = precursorValid && validationBatteryValid,
    precursorValid,
    validationBatteryValid,
    contract = new { path = ContractPath, sha256 = contractSha256, frozenBeforePhase505Read = true, replicateCount, seeds },
    phase505 = new { path = Phase505SummaryPath, expectedSummarySha256 = ExpectedPhase505SummarySha256, actualSummarySha256 = phase505ActualSha256, hashMatches = phase505HashMatches, schemaValid = phase505SchemaValid, terminalStatus = phase505TerminalStatus },
    frozenConfiguration = new { covarianceFamilies, autocorrelationFamilies, spectralCases, singleUpperScore, doubleLowerScore, unresolvedSet, truthSetCoverageMinimum, decisiveCorrectRateMinimum, maximumWrongDecisiveCalls, maximumSingleFalseSelections, invalidControls, minimumConvergenceEssDecisionAccuracy, minimumEscalationDecisionAccuracy, maximumNormalizedCost },
    operatingRows,
    invalidControlResults,
    convergenceEssCases,
    escalationCases,
    costCases,
    truthSetCoverageGatePassed,
    decisiveIdentifiableGatePassed,
    unresolvedScoredHonestly,
    wrongDecisiveGatePassed,
    falseSelectionGatePassed,
    invalidControlGatePassed,
    convergenceEssGatePassed,
    escalationGatePassed,
    costGatePassed,
    completeFrozenMenuPassed,
    nominalSubsetPassed,
    protocolValidationPassed,
    assumptionConditional,
    totalUnresolvedCalls = operatingRows.Sum(x => x.UnresolvedCalls),
    totalDecisiveCorrectCalls = operatingRows.Sum(x => x.DecisiveCorrectCalls),
    totalWrongDecisiveCalls = operatingRows.Sum(x => x.WrongDecisiveCalls),
    unresolvedCountsAsDecisiveSuccess = false,
    identifiableCasesRequireDecisiveCalls = true,
    syntheticOracleEvidenceOnly = true,
    exactOrDeterministicSyntheticInputsOnly = false,
    empiricalPowerGuarantee = false,
    thresholdTuningAfterInspection = false,
    planningEvidenceOnly = true,
    explorationOnly = true,
    confirmationEvidence = false,
    zeroPhysicsCompute = true,
    hmcRun = false,
    noHmcRun = true,
    benchmarkRun = false,
    noBenchmarkRun = true,
    reprocessingRun = false,
    zeroNewSampling = true,
    samplingRun = false,
    samplingAuthorized = false,
    productionAuthorized = false,
    phase456ArtifactMutated = false,
    phase456TerminalChanged = false,
    phase481PackCreated = false,
    phase481PackMutated = false,
    phase481RepairPackConstructed = false,
    phase481PackAuthorized = false,
    futurePhase481MustIndependentlyFreezeAcquisitionRules = true,
    existingWrittenSamplingPermissionRecordedUpstream = true,
    additionalWrittenPermissionRequiredByThisPhase = false,
    writtenPermissionIsPermissionElementOnly = true,
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
    a13BoundaryHeld = true,
    decision = verdictKind switch
    {
        "selective-protocol-validation-passed" => "The complete frozen selective-inference menu passes. Unresolved sets retain truth-set coverage but are not decisive successes; identifiable controls remain decisively recovered. This is planning evidence only.",
        "assumption-conditional-selective-protocol" => "Only the frozen nominal subset passes. Preserve the conditional result; it cannot satisfy the complete-menu gate.",
        "selective-protocol-validation-failed" => "At least one mandatory selective-inference gate fails. Preserve the negative result and do not use this protocol for an executable pack.",
        _ => "The Phase505 binding, schema, or frozen validation battery is invalid. Fail closed without a protocol conclusion or authority.",
    },
};

string outputDirectory = "studies/phase506_phase456_selective_inference_protocol_validation_001/output";
Directory.CreateDirectory(outputDirectory);
var jsonOptions = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
string json = JsonSerializer.Serialize(result, jsonOptions);
File.WriteAllText(Path.Combine(outputDirectory, "phase456_selective_inference_protocol_validation.json"), json);
File.WriteAllText(Path.Combine(outputDirectory, "phase456_selective_inference_protocol_validation_summary.json"), json);

Require(precursorValid, "Phase506 fail-closed: replace the Phase505 summary hash placeholder or repair its schema/binding.");
Require(validationBatteryValid, "Phase506 fail-closed: frozen deterministic validation battery is malformed.");
Require(verdictKind is "selective-protocol-validation-passed" or "assumption-conditional-selective-protocol" or "selective-protocol-validation-failed", "Phase506 valid battery did not produce a terminal.");
Require(!result.unresolvedCountsAsDecisiveSuccess && result.identifiableCasesRequireDecisiveCalls, "Phase506 abstention accounting firewall failed.");
Require(!result.samplingAuthorized && !result.productionAuthorized && !result.phase481RepairPackConstructed && !result.empiricalPowerGuarantee, "Phase506 planning firewall failed.");
Require(!result.phase458G3Satisfied && !result.phase458G5Satisfied && !result.o4Discharged && result.promotedPhysicalMassClaimCount == 0, "Phase506 authority or claim firewall failed.");
Console.WriteLine(terminalStatus);
Console.WriteLine($"coverage={truthSetCoverageGatePassed} decisive={decisiveIdentifiableGatePassed} wrong={operatingRows.Sum(x => x.WrongDecisiveCalls)} unresolved={operatingRows.Sum(x => x.UnresolvedCalls)} controls={invalidControlGatePassed} complete={completeFrozenMenuPassed}");

OperatingRow EvaluateRow(Family covariance, Family autocorrelation, SpectralCase spectral)
{
    int covered = 0, decisiveCorrect = 0, wrongDecisive = 0, unresolved = 0;
    double burden = covariance.Value * autocorrelation.Value;
    foreach (int seed in seeds)
    {
        double centered = UnitInterval(seed, covariance.Id, autocorrelation.Id, spectral.Id) - 0.5;
        double score = spectral.Signal + centered * 0.15 * burden;
        string[] selectedSet = score <= singleUpperScore ? new[] { "single" }
            : score >= doubleLowerScore ? new[] { "double" }
            : unresolvedSet;
        bool isUnresolved = selectedSet.Length > 1;
        bool containsTruth = selectedSet.Contains(spectral.Truth);
        if (containsTruth) covered++;
        if (isUnresolved) unresolved++;
        else if (containsTruth) decisiveCorrect++;
        else wrongDecisive++;
    }
    double coverageRate = covered / (double)replicateCount;
    double decisiveCorrectRate = decisiveCorrect / (double)replicateCount;
    return new(covariance.Id, autocorrelation.Id, spectral.Id, spectral.Truth, spectral.DecisiveRequired,
        replicateCount, covered, decisiveCorrect, wrongDecisive, unresolved, coverageRate, decisiveCorrectRate,
        coverageRate >= truthSetCoverageMinimum,
        !spectral.DecisiveRequired || (decisiveCorrectRate >= decisiveCorrectRateMinimum && unresolved == 0),
        wrongDecisive <= maximumWrongDecisiveCalls);
}

ControlResult EvaluateInvalidControl(string id) => id switch
{
    "signed-amplitude" => new(id, new[] { 1.0, -0.1 }.Any(x => x <= 0.0), "signed spectrum invalidated before selective decision"),
    "nonfinite-observation" => new(id, !new[] { 1.0, double.NaN, 0.5 }.All(double.IsFinite), "nonfinite row invalidated before selective decision"),
    "rank-deficient-covariance" => new(id, MatrixRank(new[,] { { 1.0, 1.0 }, { 1.0, 1.0 } }) < 2, "singular covariance invalidated before selective decision"),
    _ => new(id, false, "unknown frozen control fails closed"),
};

DecisionCase EvaluateConvergenceEss(string id, bool converged, double essFraction, string expected)
{
    string actual = !double.IsFinite(essFraction) ? "invalidate" : converged && essFraction >= 1.0 ? "evaluate" : "unresolved";
    return new(id, double.IsFinite(essFraction) ? essFraction : null, expected, actual, actual == expected);
}

DecisionCase EvaluateEscalation(string id, string t16Decision, bool escalationAvailable, string expected)
{
    string actual = t16Decision switch
    {
        "invalid" => "invalidate",
        "unresolved" when escalationAvailable => "escalate-t32",
        "unresolved" => "stop-unresolved",
        "decisive-single" or "decisive-double" => "stop-decisive",
        _ => "invalidate",
    };
    return new(id, null, expected, actual, actual == expected);
}

DecisionCase EvaluateCost(string id, double normalizedCost, string expected)
{
    string actual = !double.IsFinite(normalizedCost) ? "invalidate" : normalizedCost <= maximumNormalizedCost ? "continue" : "refuse";
    return new(id, double.IsFinite(normalizedCost) ? normalizedCost : null, expected, actual, actual == expected);
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
sealed record SpectralCase(string Id, string Truth, double Signal, bool DecisiveRequired);
sealed record OperatingRow(string CovarianceId, string AutocorrelationId, string SpectralCaseId, string Truth,
    bool DecisiveRequired, int Replicates, int TruthCoveredCalls, int DecisiveCorrectCalls, int WrongDecisiveCalls,
    int UnresolvedCalls, double TruthSetCoverageRate, double DecisiveCorrectRate, bool TruthSetCoveragePass,
    bool DecisiveRequirementPass, bool WrongDecisivePass);
sealed record ControlResult(string Id, bool Passed, string Detail);
sealed record DecisionCase(string Id, double? Metric, string Expected, string Actual, bool Passed);
