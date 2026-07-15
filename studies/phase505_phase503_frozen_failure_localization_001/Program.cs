using System.Security.Cryptography;
using System.Text.Json;

const string PhaseId = "phase505-phase503-frozen-failure-localization";
const string PhaseDir = "studies/phase505_phase503_frozen_failure_localization_001";
const string ContractPath = PhaseDir + "/preregistration/phase505_failure_localization_contract_v1.json";
const string Phase503Path = "studies/phase503_phase456_adaptive_calibration_protocol_validation_001/output/phase456_adaptive_calibration_protocol_validation_summary.json";
const string ExpectedPhase503Sha256 = "9f5492b11f71cb2a62305837e68d0de6402f4d167f2199d33be31c62119e1d29";

// The complete classification contract is consumed before any Phase503 byte.
byte[] contractBytes = File.ReadAllBytes(ContractPath);
string contractSha256 = Sha(contractBytes);
using JsonDocument contractDocument = JsonDocument.Parse(contractBytes);
JsonElement contract = contractDocument.RootElement;
string[] taxonomy = Strings(contract, "localizationRulesInPrecedenceOrder");
bool contractValid = S(contract, "contractId") == "phase505-a13-phase503-failure-localization-v1"
    && B(contract, "frozenBeforePhase503Consumption")
    && S(contract, "expectedPhase503SummarySha256") == ExpectedPhase503Sha256
    && S(contract, "requiredPhase503Verdict") == "assumption-conditional-protocol"
    && I(contract, "requiredOperatingRowCount") == 36
    && I(contract, "requiredFailedOperatingRowCount") == 11
    && I(contract, "replicateCount") == 24
    && taxonomy.SequenceEqual(new[]
    {
        "false-decisive-single-to-pair-call", "unresolved-pair-spectral-ambiguity",
        "coverage-deficiency", "unclassified-failed-row",
    })
    && B(contract, "phase503NegativeResultMustRemain")
    && !B(contract, "thresholdTuningAllowed") && !B(contract, "terminalRecommendationAllowed")
    && !B(contract, "samplingOrReprocessingAllowed") && !B(contract, "phase481PackCreationAllowed")
    && !B(contract, "productionAuthorizationAllowed") && !B(contract, "physicalClaimAllowed");

byte[] phase503Bytes = File.ReadAllBytes(Phase503Path);
string phase503Sha256 = Sha(phase503Bytes);
using JsonDocument phase503Document = JsonDocument.Parse(phase503Bytes);
JsonElement source = phase503Document.RootElement;
JsonElement config = source.GetProperty("frozenConfiguration");
var covarianceValues = ReadFamilies(config.GetProperty("covarianceFamilies"));
var autocorrelationValues = ReadFamilies(config.GetProperty("autocorrelationFamilies"));
var spectralCases = config.GetProperty("spectralCases").EnumerateArray().ToDictionary(
    x => S(x, "id"), x => new Spectral(S(x, "kind"), D(x, "difficulty")));
double nominalCoverageMinimum = D(config, "nominalCoverageMinimum");
double adversarialCoverageMinimum = D(config, "adversarialCoverageMinimum");
double modelSelectionMinimum = D(config, "modelSelectionMinimum");
double maximumFalseSelectionRate = D(config, "maximumFalseSelectionRate");

var sourceRows = source.GetProperty("operatingRows").EnumerateArray().Select(ReadRow).ToArray();
var failedSourceRows = sourceRows.Where(x => !x.CoveragePass || !x.ModelSelectionPass).ToArray();
bool precursorValid = phase503Sha256 == ExpectedPhase503Sha256
    && S(source, "phaseId") == "phase503-phase456-adaptive-calibration-protocol-validation"
    && S(source, "verdictKind") == "assumption-conditional-protocol"
    && B(source, "inputsValid") && !B(source, "wholeFrozenMenuPassed")
    && !B(source, "coverageGatePassed") && !B(source, "modelSelectionGatePassed")
    && !B(source, "samplingAuthorized") && !B(source, "productionAuthorized")
    && I(source, "promotedPhysicalMassClaimCount") == 0
    && sourceRows.Length == 36 && failedSourceRows.Length == 11
    && sourceRows.All(x => x.Replicates == 24);

var localizations = failedSourceRows.Select(row =>
{
    double burden = covarianceValues[row.CovarianceId] * autocorrelationValues[row.AutocorrelationId] * spectralCases[row.SpectralCaseId].Difficulty;
    double expectedCoverageProbability = Math.Clamp(0.995 - 0.030 * burden, 0.0, 1.0);
    double expectedCorrectSelectionProbability = Math.Clamp(1.075 - 0.095 * burden, 0.0, 1.0);
    double expectedFalseSelectionProbability = spectralCases[row.SpectralCaseId].Kind == "single"
        ? 1.0 - expectedCorrectSelectionProbability : 0.0;
    bool coverageExpectedToFail = !row.CoveragePass && expectedCoverageProbability < row.RequiredCoverage;
    bool selectionExpectedToFail = !row.ModelSelectionPass &&
        (expectedCorrectSelectionProbability < modelSelectionMinimum ||
         (spectralCases[row.SpectralCaseId].Kind == "single" && expectedFalseSelectionProbability > maximumFalseSelectionRate));
    string primary = !row.ModelSelectionPass && spectralCases[row.SpectralCaseId].Kind == "single"
        ? "false-decisive-single-to-pair-call"
        : !row.ModelSelectionPass && spectralCases[row.SpectralCaseId].Kind == "double"
            ? "unresolved-pair-spectral-ambiguity"
            : !row.CoveragePass ? "coverage-deficiency" : "unclassified-failed-row";
    string coverageScale = !row.CoveragePass
        ? coverageExpectedToFail ? "structural-expected-probability-failure" : "finite-24-replicate-marginal-miss"
        : "not-applicable";
    string selectionScale = !row.ModelSelectionPass
        ? selectionExpectedToFail ? "structural-expected-probability-failure" : "finite-24-replicate-marginal-miss"
        : "not-applicable";
    return new
    {
        rowId = $"{row.CovarianceId}|{row.AutocorrelationId}|{row.SpectralCaseId}",
        row.CovarianceId,
        row.AutocorrelationId,
        row.SpectralCaseId,
        spectralKind = spectralCases[row.SpectralCaseId].Kind,
        row.Replicates,
        primaryLocalization = primary,
        failedGates = new[] { !row.CoveragePass ? "coverage" : null, !row.ModelSelectionPass ? "model-selection" : null }.Where(x => x is not null).ToArray(),
        observed = new { row.Covered, row.SelectedCorrectly, row.FalseSelections, row.CoverageRate, row.CorrectSelectionRate, row.FalseSelectionRate },
        frozenThresholds = new { row.RequiredCoverage, modelSelectionMinimum, maximumFalseSelectionRate },
        exactExpectedProbabilities = new { burden, expectedCoverageProbability, expectedCorrectSelectionProbability, expectedFalseSelectionProbability },
        failureScale = new { coverage = coverageScale, modelSelection = selectionScale },
        phase503GateFailurePreserved = true,
        countedAsSuccessfulDecisiveSelection = false,
    };
}).ToArray();

int finiteCoverageMissCount = localizations.Count(x => x.failureScale.coverage == "finite-24-replicate-marginal-miss");
int structuralCoverageFailureCount = localizations.Count(x => x.failureScale.coverage == "structural-expected-probability-failure");
int falseDecisiveSingleCount = localizations.Count(x => x.primaryLocalization == "false-decisive-single-to-pair-call");
int unresolvedPairAmbiguityCount = localizations.Count(x => x.primaryLocalization == "unresolved-pair-spectral-ambiguity");
int coverageOnlyCount = localizations.Count(x => x.primaryLocalization == "coverage-deficiency");
int structuralSelectionFailureCount = localizations.Count(x => x.failureScale.modelSelection == "structural-expected-probability-failure");
int finiteSelectionMissCount = localizations.Count(x => x.failureScale.modelSelection == "finite-24-replicate-marginal-miss");
bool allFailedRowsAccountedFor = localizations.Length == 11
    && localizations.Select(x => x.rowId).Distinct().Count() == 11
    && localizations.All(x => taxonomy.Contains(x.primaryLocalization))
    && localizations.All(x => x.failedGates.Length > 0);
bool localizationValid = contractValid && precursorValid && allFailedRowsAccountedFor;
string classification = localizationValid ? "complete-negative-preserved" : "invalid-precursor-or-localization";
string terminalStatus = "phase503-frozen-failure-localization-" + classification;

var result = new
{
    phaseId = PhaseId,
    terminalStatus,
    verdictKind = classification,
    localizationClassification = classification,
    inputsValid = localizationValid,
    applicationSubjectKind = "phase503-frozen-operating-row-failure-localization",
    planSection = "WAVE2_AMENDMENTS_2026-07-12 A13",
    contract = new { path = ContractPath, byteSha256 = contractSha256, valid = contractValid, frozenBeforePhase503Consumption = true, taxonomy },
    phase503 = new
    {
        path = Phase503Path,
        expectedSummarySha256 = ExpectedPhase503Sha256,
        actualSummarySha256 = phase503Sha256,
        exactHashMatch = phase503Sha256 == ExpectedPhase503Sha256,
        precursorValid,
        originalVerdictKind = S(source, "verdictKind"),
        originalTerminalStatus = S(source, "terminalStatus"),
        originalNegativeResultPreserved = true,
    },
    sourceRowCount = sourceRows.Length,
    failedSourceRowCount = failedSourceRows.Length,
    localizedRowCount = localizations.Length,
    allFailedRowsAccountedFor,
    gridStep = 1.0 / 24.0,
    counts = new
    {
        finiteCoverageMissCount,
        structuralCoverageFailureCount,
        falseDecisiveSingleCount,
        unresolvedPairAmbiguityCount,
        coverageOnlyCount,
        structuralSelectionFailureCount,
        finiteSelectionMissCount,
    },
    localizations,
    findings = new
    {
        allObservedCoverageFailuresRemainGateFailures = true,
        noStructuralExpectedProbabilityCoverageFailureObserved = structuralCoverageFailureCount == 0,
        falseDecisiveSingleCallsObserved = falseDecisiveSingleCount > 0,
        structuralPairAmbiguityObserved = localizations.Any(x => x.primaryLocalization == "unresolved-pair-spectral-ambiguity" && x.failureScale.modelSelection == "structural-expected-probability-failure"),
        finiteReplicateLabelDoesNotExcuseObservedFailure = true,
    },
    deterministicReadOnlyLocalization = true,
    thresholdTuningAfterInspection = false,
    recommendsTerminal = false,
    phase503VerdictChanged = false,
    phase503ArtifactMutated = false,
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
    phase481PackCreated = false,
    phase481PackMutated = false,
    phase481RepairPackConstructed = false,
    phase458G3Satisfied = false,
    phase458G5Satisfied = false,
    phase458EvaluationAuthorized = false,
    o4Discharged = false,
    humanRulingAuthored = false,
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
    decision = localizationValid
        ? "All eleven Phase503 failed rows are localized without altering the frozen negative result. The diagnosis recommends no repair terminal and grants no authority."
        : "The frozen contract, exact Phase503 binding, or eleven-row accounting failed. No localization conclusion is available.",
};

string outputDirectory = Path.Combine(PhaseDir, "output");
Directory.CreateDirectory(outputDirectory);
var jsonOptions = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
string json = JsonSerializer.Serialize(result, jsonOptions);
File.WriteAllText(Path.Combine(outputDirectory, "phase503_frozen_failure_localization.json"), json);
File.WriteAllText(Path.Combine(outputDirectory, "phase503_frozen_failure_localization_summary.json"), json);

Require(localizationValid, terminalStatus);
Require(localizations.Length == 11 && finiteCoverageMissCount + structuralCoverageFailureCount == failedSourceRows.Count(x => !x.CoveragePass), "coverage accounting failed");
Require(falseDecisiveSingleCount == failedSourceRows.Count(x => !x.ModelSelectionPass && spectralCases[x.SpectralCaseId].Kind == "single"), "single-call accounting failed");
Require(unresolvedPairAmbiguityCount == failedSourceRows.Count(x => !x.ModelSelectionPass && spectralCases[x.SpectralCaseId].Kind == "double"), "pair ambiguity accounting failed");
Require(!result.recommendsTerminal && !result.phase503VerdictChanged && !result.samplingAuthorized && !result.productionAuthorized, "negative-result or authority firewall failed");
Require(!result.sourceContractApplicationAllowed && !result.phase458G3Satisfied && result.promotedPhysicalMassClaimCount == 0, "claim firewall failed");
Console.WriteLine(terminalStatus);
Console.WriteLine($"rows={localizations.Length} finiteCoverage={finiteCoverageMissCount} structuralCoverage={structuralCoverageFailureCount} falseSingle={falseDecisiveSingleCount} pairAmbiguity={unresolvedPairAmbiguityCount}");

static Dictionary<string, double> ReadFamilies(JsonElement array) => array.EnumerateArray().ToDictionary(x => S(x, "id"), x => D(x, "value"));
static OperatingRow ReadRow(JsonElement x) => new(
    S(x, "covarianceId"), S(x, "autocorrelationId"), S(x, "spectralCaseId"), I(x, "replicates"),
    I(x, "covered"), I(x, "selectedCorrectly"), I(x, "falseSelections"), D(x, "coverageRate"),
    D(x, "correctSelectionRate"), D(x, "falseSelectionRate"), D(x, "requiredCoverage"),
    B(x, "coveragePass"), B(x, "modelSelectionPass"));
static string Sha(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
static string S(JsonElement x, string name) => x.GetProperty(name).GetString() ?? "";
static int I(JsonElement x, string name) => x.GetProperty(name).GetInt32();
static double D(JsonElement x, string name) => x.GetProperty(name).GetDouble();
static bool B(JsonElement x, string name) => x.GetProperty(name).ValueKind == JsonValueKind.True;
static string[] Strings(JsonElement x, string name) => x.GetProperty(name).EnumerateArray().Select(v => v.GetString() ?? "").ToArray();
static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }

sealed record Spectral(string Kind, double Difficulty);
sealed record OperatingRow(string CovarianceId, string AutocorrelationId, string SpectralCaseId, int Replicates,
    int Covered, int SelectedCorrectly, int FalseSelections, double CoverageRate, double CorrectSelectionRate,
    double FalseSelectionRate, double RequiredCoverage, bool CoveragePass, bool ModelSelectionPass);
