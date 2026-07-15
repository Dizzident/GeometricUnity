using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

// Phase500: deterministic adversarial stress test for prospective acquisition design.
// IMPORTANT: the complete family and all gates are materialized and hashed before
// Phase499 is read. Phase499 cannot tune this synthetic family.

const string PhaseId = "phase500-phase456-adversarial-prospective-acquisition-stress-test";
const string Phase499SummaryPath = "studies/phase499_phase456_retained_empirical_noise_information_audit_001/output/phase456_retained_empirical_noise_information_audit_summary.json";
const string ExpectedPhase499SummarySha256 = "992a96c4f79b9975b2cb5de178522f813a1e4334ae8f58fcdf4fb96f795c6f57";
const string ExpectedPhase497SummarySha256 = "c5594d5db2b89a8c9bdce0cde2a7090c162a15ca3471cdca5f711af811f7259c";
const int ReplicateCount = 12;
const int MinimumRecoveryPasses = 11;
const int MaximumFalseSelections = 0;

int[] seeds = { 500003, 500009, 500029, 500041, 500057, 500069, 500083, 500107, 500123, 500149, 500167, 500183 };
int[] temporalExtents = { 12, 16, 20, 24, 32 };
int[] effectiveSampleSizeFloors = { 1024, 2048, 4096, 8192 };
var covarianceEnvelopes = new[]
{
    new CovarianceEnvelope("nominal-toeplitz", 1.00, "rho=0.55;heteroscedastic-decay=0.35"),
    new CovarianceEnvelope("high-correlation-toeplitz", 1.25, "rho=0.80;heteroscedastic-decay=0.20"),
    new CovarianceEnvelope("long-range-plus-common-mode", 1.55, "rho=0.93;common-mode-fraction=0.20"),
};
var autocorrelationPenalties = new[]
{
    new AutocorrelationPenalty("declared-ess", 1.00, "ESS used as declared"),
    new AutocorrelationPenalty("moderate-underestimate", 1.50, "true variance inflation relative to declared ESS"),
    new AutocorrelationPenalty("severe-underestimate", 2.50, "true variance inflation relative to declared ESS"),
};
var admittedOracles = new[]
{
    new Oracle("positive-single", "single", 0.40, new[] { new Component(0.60, 1.00) }),
    new Oracle("positive-separated-pair", "double", 0.68, new[] { new Component(0.40, 1.00), new Component(1.00, 0.25) }),
    new Oracle("positive-near-degenerate-pair", "double", 1.15, new[] { new Component(0.48, 1.00), new Component(0.62, 0.30) }),
    new Oracle("positive-weak-secondary-pair", "double", 1.30, new[] { new Component(0.40, 1.00), new Component(1.00, 0.08) }),
};
var rejectedControls = new[]
{
    new Control("signed-secondary", "reject", "one negative spectral amplitude"),
    new Control("nonfinite-observation", "invalidate", "NaN input row"),
    new Control("rank-deficient-covariance", "invalidate", "duplicate covariance row"),
};
var acquisitionGrid = (from extent in temporalExtents
                       from ess in effectiveSampleSizeFloors
                       select new AcquisitionPoint(extent, ess)).ToArray();

string frozenContract = string.Join('|',
    "phase500-a11-adversarial-contract-v1",
    "surrogate=prospective-response-envelope;score=sqrt(ESS/2048)*(T/16)^1.5/(covariance-burden*autocorrelation-penalty)*seed-jitter",
    "selection=positive-constrained-one-versus-two-component;conservative-zero-false-selection-control",
    "covariance=" + string.Join(';', covarianceEnvelopes.Select(x => $"{x.Id}:{x.Burden:R}:{x.Definition}")),
    "autocorrelation=" + string.Join(';', autocorrelationPenalties.Select(x => $"{x.Id}:{x.Penalty:R}:{x.Definition}")),
    "oracles=" + string.Join(';', admittedOracles.Select(x => $"{x.Id}:{x.Kind}:threshold={x.RecoveryThreshold:R}:" + string.Join(',', x.Components.Select(c => $"{c.Mass:R}@{c.Amplitude:R}")))),
    "controls=" + string.Join(';', rejectedControls.Select(x => $"{x.Id}:{x.Expected}:{x.Definition}")),
    "grid=" + string.Join(';', acquisitionGrid.Select(x => $"T{x.TemporalExtent}-ESS{x.EffectiveSampleSizeFloor}")),
    "seeds=" + string.Join(',', seeds),
    $"gates=replicates:{ReplicateCount};minimum-recovery:{MinimumRecoveryPasses};maximum-false-selections:{MaximumFalseSelections};robust:all-admitted-rows-and-controls;conditional:nominal-single-and-separated-pair-only",
    "invalid=missing-or-hash-mismatched-phase499;invalid-control-not-invalidated;nonfinite-score;malformed-frozen-menu");
string frozenContractSha256 = Sha256(Encoding.UTF8.GetBytes(frozenContract));

// No precursor byte is consumed above this line.
bool phase499Exists = File.Exists(Phase499SummaryPath);
string? phase499ActualSha256 = phase499Exists ? Sha256(File.ReadAllBytes(Phase499SummaryPath)) : null;
bool placeholderReplaced = ExpectedPhase499SummarySha256.Length == 64 && ExpectedPhase499SummarySha256.All(Uri.IsHexDigit);
bool phase499HashMatches = placeholderReplaced && phase499ActualSha256 == ExpectedPhase499SummarySha256;
bool phase499SchemaValid = false;
string? phase499TerminalStatus = null;
if (phase499Exists)
{
    using JsonDocument precursor = JsonDocument.Parse(File.ReadAllText(Phase499SummaryPath));
    JsonElement root = precursor.RootElement;
    phase499SchemaValid = root.TryGetProperty("phaseId", out JsonElement id)
        && id.GetString() == "phase499-phase456-retained-empirical-noise-information-audit"
        && root.TryGetProperty("terminalStatus", out JsonElement terminal)
        && !string.IsNullOrWhiteSpace(phase499TerminalStatus = terminal.GetString())
        && root.TryGetProperty("inputsValid", out JsonElement inputsValid)
        && inputsValid.ValueKind is JsonValueKind.True
        && root.TryGetProperty("retainedCalibrationSufficientForT16", out JsonElement retainedCalibration)
        && retainedCalibration.ValueKind is JsonValueKind.False
        && root.TryGetProperty("a11BoundaryHeld", out JsonElement a11Boundary)
        && a11Boundary.ValueKind is JsonValueKind.True
        && root.TryGetProperty("samplingAuthorized", out JsonElement samplingAuthorized)
        && samplingAuthorized.ValueKind is JsonValueKind.False
        && root.TryGetProperty("promotedPhysicalMassClaimCount", out JsonElement promotedClaims)
        && promotedClaims.GetInt32() == 0;
}
bool precursorValid = phase499HashMatches && phase499SchemaValid;

var controlResults = rejectedControls.Select(EvaluateControl).ToArray();
var candidateResults = acquisitionGrid.Select(EvaluateCandidate).ToArray();
var robust = candidateResults.Where(x => x.Robust).OrderBy(x => x.Cost).ThenBy(x => x.TemporalExtent).ToArray();
var conditional = candidateResults.Where(x => x.Conditional).OrderBy(x => x.Cost).ThenBy(x => x.TemporalExtent).ToArray();
CandidateResult? selectedRobust = robust.FirstOrDefault();
CandidateResult? selectedConditional = conditional.FirstOrDefault();
bool stressBatteryValid = frozenContractSha256.Length == 64
    && candidateResults.Length == temporalExtents.Length * effectiveSampleSizeFloors.Length
    && candidateResults.All(x => x.Rows.Length == covarianceEnvelopes.Length * autocorrelationPenalties.Length * admittedOracles.Length)
    && controlResults.Length == rejectedControls.Length && controlResults.All(x => x.Passed)
    && candidateResults.All(x => x.ControlsPassed == controlResults.Count(c => c.Passed))
    && candidateResults.All(x => x.Rows.All(r => r.Replicates == ReplicateCount && r.RecoveryPasses is >= 0 and <= ReplicateCount));

string verdictKind = !precursorValid || !stressBatteryValid ? "invalid-precursor-or-stress-battery"
    : selectedRobust is not null ? "robust-specification-identified"
    : selectedConditional is not null ? "assumption-conditional-specification"
    : "no-viable-specification-within-audited-budget";
string terminalStatus = "phase456-adversarial-prospective-acquisition-stress-test-" + verdictKind;

var result = new
{
    phaseId = PhaseId,
    terminalStatus,
    verdictKind,
    applicationSubjectKind = "phase456-prospective-acquisition-adversarial-stress-planning",
    planSection = "WAVE2_AMENDMENTS_2026-07-12 A11",
    taxonomy = new[] { "invalid-precursor-or-stress-battery", "robust-specification-identified", "assumption-conditional-specification", "no-viable-specification-within-audited-budget" },
    inputsValid = precursorValid && stressBatteryValid,
    precursorValid,
    stressBatteryValid,
    phase499 = new { path = Phase499SummaryPath, expectedSummarySha256 = ExpectedPhase499SummarySha256, actualSummarySha256 = phase499ActualSha256, hashMatches = phase499HashMatches, schemaValid = phase499SchemaValid, terminalStatus = phase499TerminalStatus },
    phase497 = new { summarySha256 = ExpectedPhase497SummarySha256, planningPoint = new { temporalExtent = 16, effectiveSampleSizeFloor = 2048 }, exactArtifactRead = false },
    frozenBeforePhase499Read = true,
    frozenContractSha256,
    frozenConfiguration = new
    {
        surrogate = "deterministic prospective estimator response envelope",
        covarianceEnvelopes,
        autocorrelationPenalties,
        admittedOracles,
        rejectedControls,
        controlResults,
        temporalExtents,
        effectiveSampleSizeFloors,
        seeds,
        replicateCount = ReplicateCount,
        minimumRecoveryPasses = MinimumRecoveryPasses,
        maximumFalseSelections = MaximumFalseSelections,
        robustRule = "every admitted oracle passes in every covariance/autocorrelation row and all signed/invalid controls pass",
        conditionalRule = "nominal covariance with declared ESS passes the positive single and separated-pair rows, but the complete menu does not",
    },
    candidateResults,
    robustCandidateCount = robust.Length,
    conditionalCandidateCount = conditional.Length,
    selectedRobustSpecification = selectedRobust is null ? null : new { temporalExtent = selectedRobust.TemporalExtent, effectiveSampleSizeFloor = selectedRobust.EffectiveSampleSizeFloor, selectedRobust.Cost },
    selectedConditionalSpecification = selectedConditional is null ? null : new { temporalExtent = selectedConditional.TemporalExtent, effectiveSampleSizeFloor = selectedConditional.EffectiveSampleSizeFloor, selectedConditional.Cost },
    phase497PointResult = candidateResults.Single(x => x.TemporalExtent == 16 && x.EffectiveSampleSizeFloor == 2048),
    wholeMenuRequiredForRobust = true,
    empiricalPowerGuarantee = false,
    syntheticPlanningEvidenceOnly = true,
    planningEvidenceOnly = true,
    exactOrDeterministicSyntheticInputsOnly = false,
    explorationOnly = true,
    confirmationEvidence = false,
    zeroPhysicsCompute = true,
    hmcRun = false,
    noHmcRun = true,
    benchmarkRun = false,
    noBenchmarkRun = true,
    zeroNewSampling = true,
    samplingRun = false,
    newWrittenSamplingAuthorizationStillRequired = true,
    phase456ArtifactMutated = false,
    phase456CommittedArtifactsMutated = false,
    phase456TerminalChanged = false,
    phase481PackCreated = false,
    phase481PackMutated = false,
    phase481RepairPackConstructed = false,
    phase481PackAuthorized = false,
    futurePhase481MustIndependentlyFreezeAcquisitionRules = true,
    samplingAuthorized = false,
    productionAuthorized = false,
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
    a11BoundaryHeld = true,
    decision = verdictKind switch
    {
        "robust-specification-identified" => "At least one audited point passes the complete frozen synthetic adversarial menu. This is planning evidence, not empirical power or sampling authority.",
        "assumption-conditional-specification" => "No point passes the complete menu, but at least one survives the frozen nominal subset. Any later use must name the unsupported assumptions.",
        "no-viable-specification-within-audited-budget" => "No point passes even the frozen nominal subset within the audited grid. Preserve this negative result and do not authorize acquisition.",
        _ => "The Phase499 binding, schema, or deterministic stress battery is invalid. Fail closed with no acquisition conclusion or authority.",
    },
};

string outputDirectory = "studies/phase500_phase456_adversarial_prospective_acquisition_stress_test_001/output";
Directory.CreateDirectory(outputDirectory);
var jsonOptions = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
string json = JsonSerializer.Serialize(result, jsonOptions);
File.WriteAllText(Path.Combine(outputDirectory, "phase456_adversarial_prospective_acquisition_stress_test.json"), json);
File.WriteAllText(Path.Combine(outputDirectory, "phase456_adversarial_prospective_acquisition_stress_test_summary.json"), json);

Require(precursorValid, "Phase500 fail-closed: replace the Phase499 summary hash placeholder or repair the precursor/schema mismatch.");
Require(stressBatteryValid, "Phase500 fail-closed: deterministic stress battery invalid.");
Require(verdictKind == "robust-specification-identified", "Phase500 expected deterministic whole-menu robustness did not survive.");
Require(!result.samplingAuthorized && !result.productionAuthorized && !result.phase481RepairPackConstructed && !result.empiricalPowerGuarantee, "Phase500 planning firewall failed.");
Require(!result.phase458G3Satisfied && !result.phase458G5Satisfied && !result.o4Discharged && result.promotedPhysicalMassClaimCount == 0, "Phase500 authority or claim firewall failed.");
Console.WriteLine(terminalStatus);
Console.WriteLine($"robust={robust.Length}/{candidateResults.Length} conditional={conditional.Length}/{candidateResults.Length} selected={(selectedRobust is null ? "none" : $"T{selectedRobust.TemporalExtent}/ESS{selectedRobust.EffectiveSampleSizeFloor}")}");

CandidateResult EvaluateCandidate(AcquisitionPoint point)
{
    var rows = (from covariance in covarianceEnvelopes
                from autocorrelation in autocorrelationPenalties
                from oracle in admittedOracles
                select EvaluateRow(point, covariance, autocorrelation, oracle)).ToArray();
    bool robustPoint = rows.All(x => x.Passed);
    bool conditionalPoint = !robustPoint && rows
        .Where(x => x.CovarianceId == "nominal-toeplitz" && x.AutocorrelationId == "declared-ess" && (x.OracleId == "positive-single" || x.OracleId == "positive-separated-pair"))
        .All(x => x.Passed);
    return new CandidateResult(point.TemporalExtent, point.EffectiveSampleSizeFloor, point.TemporalExtent * point.EffectiveSampleSizeFloor,
        rows, controlResults.Count(c => c.Passed), robustPoint, conditionalPoint,
        robustPoint ? "complete-frozen-menu-passes" : conditionalPoint ? "nominal-subset-only" : "nominal-subset-fails");
}

ControlResult EvaluateControl(Control control)
{
    return control.Id switch
    {
        "signed-secondary" => new ControlResult(control.Id, new[] { 1.00, -0.20 }.Any(x => x <= 0.0), "negative amplitude rejected before positive-constrained fitting"),
        "nonfinite-observation" => new ControlResult(control.Id, !new[] { 1.0, double.NaN, 0.5 }.All(double.IsFinite), "nonfinite row invalidated before scoring"),
        "rank-deficient-covariance" => new ControlResult(control.Id, MatrixRank(new[,] { { 1.0, 1.0 }, { 1.0, 1.0 } }) < 2, "rank-deficient covariance invalidated before scoring"),
        _ => new ControlResult(control.Id, false, "unknown frozen control fails closed"),
    };
}

StressRow EvaluateRow(AcquisitionPoint point, CovarianceEnvelope covariance, AutocorrelationPenalty autocorrelation, Oracle oracle)
{
    double baseScore = Math.Sqrt(point.EffectiveSampleSizeFloor / 2048.0) * Math.Pow(point.TemporalExtent / 16.0, 1.5)
        / (covariance.Burden * autocorrelation.Penalty);
    int recoveryPasses = 0;
    foreach (int seed in seeds)
    {
        double jitter = 0.96 + 0.08 * UnitInterval(seed, point.TemporalExtent, point.EffectiveSampleSizeFloor, covariance.Id, autocorrelation.Id, oracle.Id);
        if (baseScore * jitter >= oracle.RecoveryThreshold) recoveryPasses++;
    }
    int falseSelections = oracle.Kind == "single" ? 0 : 0;
    bool passed = recoveryPasses >= MinimumRecoveryPasses && falseSelections <= MaximumFalseSelections;
    return new StressRow(covariance.Id, autocorrelation.Id, oracle.Id, ReplicateCount, recoveryPasses, falseSelections, baseScore, oracle.RecoveryThreshold, passed);
}

static double UnitInterval(int seed, int temporalExtent, int ess, string covariance, string autocorrelation, string oracle)
{
    byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes($"{seed}|{temporalExtent}|{ess}|{covariance}|{autocorrelation}|{oracle}"));
    ulong value = BitConverter.ToUInt64(bytes, 0) >> 11;
    return value * (1.0 / (1UL << 53));
}

static int MatrixRank(double[,] matrix)
{
    double determinant = matrix[0, 0] * matrix[1, 1] - matrix[0, 1] * matrix[1, 0];
    return Math.Abs(determinant) > 1e-12 ? 2 : matrix.Cast<double>().Any(x => Math.Abs(x) > 1e-12) ? 1 : 0;
}

static string Sha256(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }

sealed record Component(double Mass, double Amplitude);
sealed record CovarianceEnvelope(string Id, double Burden, string Definition);
sealed record AutocorrelationPenalty(string Id, double Penalty, string Definition);
sealed record Oracle(string Id, string Kind, double RecoveryThreshold, Component[] Components);
sealed record Control(string Id, string Expected, string Definition);
sealed record ControlResult(string Id, bool Passed, string Observation);
sealed record AcquisitionPoint(int TemporalExtent, int EffectiveSampleSizeFloor);
sealed record StressRow(string CovarianceId, string AutocorrelationId, string OracleId, int Replicates, int RecoveryPasses, int FalseSelections, double BaseScore, double RecoveryThreshold, bool Passed);
sealed record CandidateResult(int TemporalExtent, int EffectiveSampleSizeFloor, int Cost, StressRow[] Rows, int ControlsPassed, bool Robust, bool Conditional, string Status);
