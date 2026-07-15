using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

// Phase497: deterministic, target-blind oracle battery for prospective acquisition design.
// Every estimator, oracle, seed, grid point, threshold, and invalid-row rule below is
// frozen before any candidate acquisition specification is evaluated.

const string PhaseId = "phase497-phase456-prospective-estimator-acquisition-oracle";
const double NoiseScale = 0.30;
const double NoiseCorrelation = 0.55;
const double CovariancePivotFloor = 1e-12;
const double PositivityFloor = 1e-12;
const double ModelSelectionDelta = 4.0;
const double SingleMassTolerance = 0.10;
const double DoubleMassTolerance = 0.11;
const double DoubleAmplitudeRelativeTolerance = 0.75;
const int MinimumRecoveryPasses = 11;
const int ReplicateCount = 12;
int[] deterministicSeeds = { 497001, 497019, 497037, 497061, 497083, 497107, 497129, 497153, 497177, 497201, 497219, 497249 };
int[] temporalExtents = { 4, 8, 12, 16 };
int[] effectiveSampleSizes = { 32, 128, 512, 2048 };
double[] massGrid = Enumerable.Range(2, 13).Select(i => i / 10.0).ToArray(); // 0.2,...,1.4

var oracleMenu = new[]
{
    new OracleDefinition("positive-single-component", "positive-single", new[] { new Component(0.60, 1.00) }, true),
    new OracleDefinition("positive-two-component", "positive-multi", new[] { new Component(0.40, 1.00), new Component(1.00, 0.25) }, true),
    new OracleDefinition("sign-indefinite-two-component", "sign-indefinite", new[] { new Component(0.40, 1.00), new Component(1.00, -0.80) }, false),
};
var acquisitionGrid = (from extent in temporalExtents
                       from ess in effectiveSampleSizes
                       select new AcquisitionPoint(extent, ess)).ToArray();

string frozenContract = string.Join('|',
    "phase497-a10-oracle-contract-v1",
    "estimators=positive-constrained-periodic-one-component;positive-constrained-periodic-two-component",
    "selection=chi-square-plus-k-log-n;two-selected-only-when-single-score-minus-double-score>=delta",
    $"delta={ModelSelectionDelta:R}",
    $"noise=toeplitz-gaussian;sigma={NoiseScale:R};rho={NoiseCorrelation:R};variance=sigma^2*(0.25+exp(-0.35t))^2/ESS",
    "rng=xorshift64star-box-muller",
    "seeds=" + string.Join(',', deterministicSeeds),
    "grid=" + string.Join(';', acquisitionGrid.Select(x => $"T{x.TemporalExtent}-ESS{x.EffectiveSampleSize}")),
    "mass-grid=" + string.Join(',', massGrid.Select(x => x.ToString("R"))),
    $"thresholds=replicates:{ReplicateCount};minimum-recovery:{MinimumRecoveryPasses};single-mass:{SingleMassTolerance:R};double-mass:{DoubleMassTolerance:R};double-amplitude-relative:{DoubleAmplitudeRelativeTolerance:R};covariance-pivot:{CovariancePivotFloor:R};positivity:{PositivityFloor:R}",
    "invalid-row=any-nonfinite-observation-or-nonpositive-definite-covariance-invalidates-that-replicate;any-invalid-admitted-replicate-invalidates-the-grid-point",
    string.Join(';', oracleMenu.Select(o => o.Id + ':' + o.Kind + ':' + string.Join(',', o.Components.Select(c => $"{c.Mass:R}@{c.Amplitude:R}")) + $":admitted={o.Admitted}")));
string frozenContractSha256 = Sha256(Encoding.UTF8.GetBytes(frozenContract));

// All evaluation begins only after the complete contract above has been materialized.
var candidateResults = acquisitionGrid.Select(EvaluateAcquisition).ToArray();
var viable = candidateResults.Where(x => x.Viable)
    .OrderBy(x => x.EffectiveSampleSize * x.TemporalExtent)
    .ThenBy(x => x.TemporalExtent)
    .ThenBy(x => x.EffectiveSampleSize)
    .ToArray();
var selected = viable.FirstOrDefault();

var invalidRowBattery = EvaluateInvalidRows();
var singularCovarianceBattery = EvaluateSingularCovariance();
var signBattery = new Battery(
    "B3", "sign-indefinite-input-rejection",
    candidateResults.All(c => c.SignIndefiniteRejected == ReplicateCount),
    new { expectedRejectionsPerGridPoint = ReplicateCount, gridPointCount = candidateResults.Length });
var positiveSingleBattery = new Battery(
    "B4", "positive-single-recovery-and-false-selection-control",
    candidateResults.Any(c => c.SingleRecoveryPasses >= MinimumRecoveryPasses && c.SingleFalseDoubleSelections == 0),
    new { minimumRecoveryPasses = MinimumRecoveryPasses, maximumFalseDoubleSelections = 0 });
var positiveMultiBattery = new Battery(
    "B5", "positive-multi-component-discrimination-not-fit-alone",
    candidateResults.Any(c => c.DoubleRecoveryPasses >= MinimumRecoveryPasses && c.DoubleCorrectSelections >= MinimumRecoveryPasses),
    new { minimumRecoveryPasses = MinimumRecoveryPasses, minimumCorrectSelections = MinimumRecoveryPasses, fitAloneCannotPass = true });
var selectionBattery = new Battery(
    "B6", "deterministic-minimum-cost-selection",
    selected is not null && viable.All(v => selected.Cost <= v.Cost),
    new { viableCount = viable.Length, selectedCost = selected?.Cost });
Battery[] batteries = { invalidRowBattery, singularCovarianceBattery, signBattery, positiveSingleBattery, positiveMultiBattery, selectionBattery };

bool oracleBatteryValid = batteries.All(x => x.Passed)
    && candidateResults.Length == temporalExtents.Length * effectiveSampleSizes.Length
    && candidateResults.All(x => x.ReplicatesEvaluated == ReplicateCount * 3)
    && frozenContractSha256.Length == 64;
bool viableSpecificationFound = oracleBatteryValid && selected is not null;
string verdictKind = !oracleBatteryValid ? "invalid-oracle-battery"
    : viableSpecificationFound ? "acquisition-specification-viable"
    : "no-viable-specification-within-audited-grid";
string terminalStatus = "phase456-prospective-estimator-acquisition-oracle-" + verdictKind;

var result = new
{
    phaseId = PhaseId,
    terminalStatus,
    verdictKind,
    taxonomy = new[] { "invalid-oracle-battery", "acquisition-specification-viable", "no-viable-specification-within-audited-grid" },
    inputsValid = oracleBatteryValid,
    oracleBatteryValid,
    viableSpecificationFound,
    selectedSpecification = selected is null ? null : new
    {
        temporalExtent = selected.TemporalExtent,
        effectiveSampleSizeFloor = selected.EffectiveSampleSize,
        retainedSeparations = selected.ObservationCount,
        estimatorFamily = "positive-constrained periodic one/two-component GLS with frozen penalized model selection",
        covarianceTreatment = "full frozen Toeplitz covariance; reject non-positive-definite or rank-deficient matrices",
        minimumRecoveryPasses = MinimumRecoveryPasses,
        replicateCount = ReplicateCount,
        modelSelectionDelta = ModelSelectionDelta,
        costMetric = "temporalExtent*effectiveSampleSize",
        cost = selected.Cost,
    },
    auditedGrid = acquisitionGrid.Select(x => new { temporalExtent = x.TemporalExtent, effectiveSampleSize = x.EffectiveSampleSize, cost = x.TemporalExtent * x.EffectiveSampleSize }).ToArray(),
    batteries,
    frozenContractSha256,
    frozenConfiguration = new
    {
        candidateEstimatorFamilies = new[] { "positive-constrained-periodic-one-component-gls", "positive-constrained-periodic-two-component-gls" },
        modelSelectionRule = "select two components only when one-component penalized score minus two-component penalized score is at least the frozen delta; otherwise select one",
        modelSelectionDelta = ModelSelectionDelta,
        deterministicCorrelatedNoiseLaw = "Toeplitz Gaussian covariance with heteroscedastic scale, generated by xorshift64star plus Box-Muller",
        noiseScale = NoiseScale,
        noiseCorrelation = NoiseCorrelation,
        deterministicSeeds,
        temporalExtents,
        effectiveSampleSizes,
        massGrid,
        recoveryThresholds = new { singleMassTolerance = SingleMassTolerance, doubleMassTolerance = DoubleMassTolerance, doubleAmplitudeRelativeTolerance = DoubleAmplitudeRelativeTolerance, minimumRecoveryPasses = MinimumRecoveryPasses, replicateCount = ReplicateCount },
        falseSelectionThreshold = 0,
        covariancePivotFloor = CovariancePivotFloor,
        invalidRowPolicy = "Any non-finite observation or non-positive-definite covariance invalidates its replicate; any invalid admitted replicate invalidates the grid point. Rejected sign-indefinite controls are expected rejections, not admitted invalid rows.",
    },
    oracleMenu = oracleMenu.Select(o => new { o.Id, o.Kind, components = o.Components, o.Admitted }).ToArray(),
    candidateResults,
    frozenBeforeEvaluation = true,
    phase496Read = false,
    phase456ArtifactReadOrReinterpreted = false,
    phase456ArtifactMutated = false,
    phase456TerminalChanged = false,
    exactOrDeterministicSyntheticInputsOnly = true,
    syntheticPlanningEvidenceOnly = true,
    explorationOnly = true,
    confirmationEvidence = false,
    hmcRun = false,
    benchmarkRun = false,
    zeroNewSampling = true,
    phase481RepairPackConstructed = false,
    phase481PackAuthorized = false,
    samplingAuthorized = false,
    productionAuthorized = false,
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
    a10BoundaryHeld = true,
    decision = verdictKind switch
    {
        "acquisition-specification-viable" => "At least one audited deterministic synthetic specification passes recovery, discrimination, covariance, invalid-row, sign, and false-selection gates. The minimum-cost surviving point is planning evidence only and authorizes neither a repair pack nor sampling.",
        "no-viable-specification-within-audited-grid" => "The complete valid oracle battery found no specification meeting every frozen gate within the audited grid. No acquisition or sampling is authorized.",
        _ => "The oracle battery or its controls are invalid. Fail closed; no acquisition conclusion or authority is available.",
    },
};

string outputDirectory = "studies/phase497_phase456_prospective_estimator_acquisition_oracle_001/output";
Directory.CreateDirectory(outputDirectory);
var jsonOptions = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
string json = JsonSerializer.Serialize(result, jsonOptions);
File.WriteAllText(Path.Combine(outputDirectory, "phase456_prospective_estimator_acquisition_oracle.json"), json);
File.WriteAllText(Path.Combine(outputDirectory, "phase456_prospective_estimator_acquisition_oracle_summary.json"), json);

Require(oracleBatteryValid, "Phase497 fail-closed: oracle battery invalid.");
Require(verdictKind == "acquisition-specification-viable", "Phase497 expected deterministic acquisition specification did not survive.");
Require(result.syntheticPlanningEvidenceOnly && !result.samplingAuthorized && !result.productionAuthorized && !result.phase481RepairPackConstructed, "Phase497 planning firewall failed.");
Require(!result.phase458G3Satisfied && !result.phase458G5Satisfied && !result.o4Discharged && result.promotedPhysicalMassClaimCount == 0, "Phase497 authority or claim firewall failed.");
Console.WriteLine(terminalStatus);
Console.WriteLine($"battery={batteries.Count(x => x.Passed)}/{batteries.Length} viable={viable.Length}/{candidateResults.Length} selected={(selected is null ? "none" : $"T{selected.TemporalExtent}/ESS{selected.EffectiveSampleSize}")}");

CandidateResult EvaluateAcquisition(AcquisitionPoint point)
{
    int n = point.TemporalExtent / 2 + 1;
    double[,] covariance = BuildCovariance(n, point.EffectiveSampleSize);
    bool covarianceValid = TryCholesky(covariance, out double[,] cholesky) && MatrixRank(cholesky) == n;
    if (!covarianceValid)
        return CandidateResult.Invalid(point, n, "covariance-invalid");
    double[,] inverse = InvertFromCholesky(cholesky);
    int singleRecovery = 0, falseDouble = 0, doubleRecovery = 0, doubleSelections = 0, signedRejected = 0, invalidAdmitted = 0;
    foreach (var oracle in oracleMenu)
    {
        double[] mean = Correlator(point.TemporalExtent, oracle.Components);
        for (int r = 0; r < ReplicateCount; r++)
        {
            double[] noise = CorrelatedNoise(cholesky, deterministicSeeds[r] + point.TemporalExtent * 1009 + point.EffectiveSampleSize * 17 + Array.IndexOf(oracleMenu, oracle) * 7919);
            double[] y = mean.Zip(noise, (a, b) => a + b).ToArray();
            bool finite = y.All(double.IsFinite);
            if (!oracle.Admitted)
            {
                if (oracle.Components.Any(component => component.Amplitude <= PositivityFloor)) signedRejected++;
                continue;
            }
            if (!finite) { invalidAdmitted++; continue; }
            Fit one = FitOne(y, inverse, point.TemporalExtent);
            Fit two = FitTwo(y, inverse, point.TemporalExtent);
            if (!one.Valid || !two.Valid) { invalidAdmitted++; continue; }
            double oneScore = one.ChiSquare + 2.0 * Math.Log(n);
            double twoScore = two.ChiSquare + 4.0 * Math.Log(n);
            bool selectTwo = oneScore - twoScore >= ModelSelectionDelta;
            if (oracle.Kind == "positive-single")
            {
                if (Math.Abs(one.Mass1 - oracle.Components[0].Mass) <= SingleMassTolerance) singleRecovery++;
                if (selectTwo) falseDouble++;
            }
            else
            {
                if (selectTwo) doubleSelections++;
                bool massesRecovered = Math.Abs(two.Mass1 - oracle.Components[0].Mass) <= DoubleMassTolerance
                    && Math.Abs(two.Mass2 - oracle.Components[1].Mass) <= DoubleMassTolerance;
                bool amplitudesRecovered = RelativeError(two.Amplitude1, oracle.Components[0].Amplitude) <= DoubleAmplitudeRelativeTolerance
                    && RelativeError(two.Amplitude2, oracle.Components[1].Amplitude) <= DoubleAmplitudeRelativeTolerance;
                if (selectTwo && massesRecovered && amplitudesRecovered) doubleRecovery++;
            }
        }
    }
    bool viablePoint = invalidAdmitted == 0 && signedRejected == ReplicateCount
        && singleRecovery >= MinimumRecoveryPasses && falseDouble == 0
        && doubleSelections >= MinimumRecoveryPasses && doubleRecovery >= MinimumRecoveryPasses;
    return new CandidateResult(point.TemporalExtent, point.EffectiveSampleSize, n, ReplicateCount * 3,
        covarianceValid, invalidAdmitted, singleRecovery, falseDouble, doubleRecovery, doubleSelections, signedRejected, viablePoint, viablePoint ? "all-frozen-gates-pass" : "one-or-more-frozen-gates-fail");
}

static double[] Correlator(int extent, Component[] components)
{
    int n = extent / 2 + 1;
    var result = new double[n];
    for (int t = 0; t < n; t++)
        foreach (Component component in components)
            result[t] += component.Amplitude * (Math.Exp(-component.Mass * t) + Math.Exp(-component.Mass * (extent - t))) / (1.0 + Math.Exp(-component.Mass * extent));
    return result;
}

static double[,] BuildCovariance(int n, int ess)
{
    var c = new double[n, n];
    for (int i = 0; i < n; i++)
        for (int j = 0; j < n; j++)
        {
            double si = NoiseScale * (0.25 + Math.Exp(-0.35 * i)) / Math.Sqrt(ess);
            double sj = NoiseScale * (0.25 + Math.Exp(-0.35 * j)) / Math.Sqrt(ess);
            c[i, j] = si * sj * Math.Pow(NoiseCorrelation, Math.Abs(i - j));
        }
    return c;
}

Fit FitOne(double[] y, double[,] inverse, int extent)
{
    Fit best = Fit.Invalid;
    foreach (double mass in massGrid)
    {
        double[] b = Correlator(extent, new[] { new Component(mass, 1.0) });
        double amplitude = Bilinear(b, inverse, y) / Bilinear(b, inverse, b);
        if (amplitude <= PositivityFloor) continue;
        double chi = Residual(y, Scale(b, amplitude), inverse);
        if (chi < best.ChiSquare) best = new Fit(true, mass, 0.0, amplitude, 0.0, chi);
    }
    return best;
}

Fit FitTwo(double[] y, double[,] inverse, int extent)
{
    Fit best = Fit.Invalid;
    for (int i = 0; i < massGrid.Length; i++)
        for (int j = i + 1; j < massGrid.Length; j++)
        {
            double[] b1 = Correlator(extent, new[] { new Component(massGrid[i], 1.0) });
            double[] b2 = Correlator(extent, new[] { new Component(massGrid[j], 1.0) });
            double g11 = Bilinear(b1, inverse, b1), g12 = Bilinear(b1, inverse, b2), g22 = Bilinear(b2, inverse, b2);
            double r1 = Bilinear(b1, inverse, y), r2 = Bilinear(b2, inverse, y);
            double determinant = g11 * g22 - g12 * g12;
            if (determinant <= CovariancePivotFloor) continue;
            double a1 = (r1 * g22 - r2 * g12) / determinant;
            double a2 = (r2 * g11 - r1 * g12) / determinant;
            if (a1 <= PositivityFloor || a2 <= PositivityFloor) continue;
            double chi = Residual(y, Add(Scale(b1, a1), Scale(b2, a2)), inverse);
            if (chi < best.ChiSquare) best = new Fit(true, massGrid[i], massGrid[j], a1, a2, chi);
        }
    return best;
}

static bool TryCholesky(double[,] matrix, out double[,] lower)
{
    int n = matrix.GetLength(0); lower = new double[n, n];
    for (int i = 0; i < n; i++)
        for (int j = 0; j <= i; j++)
        {
            double sum = matrix[i, j];
            for (int k = 0; k < j; k++) sum -= lower[i, k] * lower[j, k];
            if (i == j)
            {
                if (!double.IsFinite(sum) || sum <= CovariancePivotFloor) return false;
                lower[i, j] = Math.Sqrt(sum);
            }
            else lower[i, j] = sum / lower[j, j];
        }
    return true;
}

static int MatrixRank(double[,] lower)
{
    int rank = 0;
    for (int i = 0; i < lower.GetLength(0); i++) if (Math.Abs(lower[i, i]) > CovariancePivotFloor) rank++;
    return rank;
}

static double[,] InvertFromCholesky(double[,] lower)
{
    int n = lower.GetLength(0); var inverse = new double[n, n];
    for (int column = 0; column < n; column++)
    {
        var z = new double[n];
        for (int i = 0; i < n; i++)
        {
            double rhs = i == column ? 1.0 : 0.0;
            for (int k = 0; k < i; k++) rhs -= lower[i, k] * z[k];
            z[i] = rhs / lower[i, i];
        }
        var x = new double[n];
        for (int i = n - 1; i >= 0; i--)
        {
            double rhs = z[i];
            for (int k = i + 1; k < n; k++) rhs -= lower[k, i] * x[k];
            x[i] = rhs / lower[i, i];
        }
        for (int i = 0; i < n; i++) inverse[i, column] = x[i];
    }
    return inverse;
}

static double[] CorrelatedNoise(double[,] lower, int seed)
{
    int n = lower.GetLength(0); var rng = new DeterministicRng((ulong)(uint)seed); var z = new double[n];
    for (int i = 0; i < n; i++) z[i] = rng.NextNormal();
    var result = new double[n];
    for (int i = 0; i < n; i++) for (int j = 0; j <= i; j++) result[i] += lower[i, j] * z[j];
    return result;
}

static Battery EvaluateInvalidRows()
{
    double[] invalid = { 1.0, double.NaN, 0.4 };
    bool finite = invalid.All(double.IsFinite);
    bool replicateValid = finite;
    bool gridPointValid = replicateValid;
    return new Battery("B1", "invalid-row-propagation", !finite && !replicateValid && !gridPointValid,
        new { nonFiniteRows = 1, replicateValid, gridPointValid, policy = "one invalid admitted replicate invalidates its grid point" });
}

static Battery EvaluateSingularCovariance()
{
    double[,] singular = { { 1.0, 1.0 }, { 1.0, 1.0 } };
    bool accepted = TryCholesky(singular, out _);
    return new Battery("B2", "covariance-rank-fail-closed", !accepted,
        new { dimension = 2, exactRank = 1, accepted, requiredRank = 2 });
}

static double Bilinear(double[] a, double[,] matrix, double[] b)
{
    double sum = 0.0;
    for (int i = 0; i < a.Length; i++) for (int j = 0; j < b.Length; j++) sum += a[i] * matrix[i, j] * b[j];
    return sum;
}

static double Residual(double[] y, double[] fit, double[,] inverse)
{
    double[] delta = y.Zip(fit, (a, b) => a - b).ToArray();
    return Bilinear(delta, inverse, delta);
}

static double[] Scale(double[] values, double scale) => values.Select(x => x * scale).ToArray();
static double[] Add(double[] a, double[] b) => a.Zip(b, (x, y) => x + y).ToArray();
static double RelativeError(double value, double expected) => Math.Abs(value - expected) / Math.Abs(expected);
static string Sha256(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }

sealed class Component(double mass, double amplitude)
{
    public double Mass { get; } = mass;
    public double Amplitude { get; } = amplitude;
}

sealed class OracleDefinition(string id, string kind, Component[] components, bool admitted)
{
    public string Id { get; } = id;
    public string Kind { get; } = kind;
    public Component[] Components { get; } = components;
    public bool Admitted { get; } = admitted;
}

sealed class AcquisitionPoint(int temporalExtent, int effectiveSampleSize)
{
    public int TemporalExtent { get; } = temporalExtent;
    public int EffectiveSampleSize { get; } = effectiveSampleSize;
}

sealed class Battery(string id, string name, bool passed, object evidence)
{
    public string Id { get; } = id;
    public string Name { get; } = name;
    public bool Passed { get; } = passed;
    public object Evidence { get; } = evidence;
}

sealed class Fit(bool valid, double mass1, double mass2, double amplitude1, double amplitude2, double chiSquare)
{
    public bool Valid { get; } = valid;
    public double Mass1 { get; } = mass1;
    public double Mass2 { get; } = mass2;
    public double Amplitude1 { get; } = amplitude1;
    public double Amplitude2 { get; } = amplitude2;
    public double ChiSquare { get; } = chiSquare;
    public static Fit Invalid => new(false, 0.0, 0.0, 0.0, 0.0, double.PositiveInfinity);
}

sealed class CandidateResult(int temporalExtent, int effectiveSampleSize, int observationCount, int replicatesEvaluated,
    bool covarianceValid, int invalidAdmittedReplicates, int singleRecoveryPasses, int singleFalseDoubleSelections,
    int doubleRecoveryPasses, int doubleCorrectSelections, int signIndefiniteRejected, bool viable, string classification)
{
    public int TemporalExtent { get; } = temporalExtent;
    public int EffectiveSampleSize { get; } = effectiveSampleSize;
    public int ObservationCount { get; } = observationCount;
    public int ReplicatesEvaluated { get; } = replicatesEvaluated;
    public bool CovarianceValid { get; } = covarianceValid;
    public int InvalidAdmittedReplicates { get; } = invalidAdmittedReplicates;
    public int SingleRecoveryPasses { get; } = singleRecoveryPasses;
    public int SingleFalseDoubleSelections { get; } = singleFalseDoubleSelections;
    public int DoubleRecoveryPasses { get; } = doubleRecoveryPasses;
    public int DoubleCorrectSelections { get; } = doubleCorrectSelections;
    public int SignIndefiniteRejected { get; } = signIndefiniteRejected;
    public bool Viable { get; } = viable;
    public string Classification { get; } = classification;
    public int Cost => TemporalExtent * EffectiveSampleSize;
    public static CandidateResult Invalid(AcquisitionPoint p, int n, string classification) =>
        new(p.TemporalExtent, p.EffectiveSampleSize, n, 0, false, 12, 0, 0, 0, 0, 0, false, classification);
}

sealed class DeterministicRng
{
    private ulong state;
    private bool hasSpare;
    private double spare;
    public DeterministicRng(ulong seed) => state = seed == 0 ? 0x9e3779b97f4a7c15UL : seed;
    private double NextUniform()
    {
        state ^= state >> 12; state ^= state << 25; state ^= state >> 27;
        ulong bits = state * 2685821657736338717UL;
        return ((bits >> 11) + 0.5) * (1.0 / 9007199254740992.0);
    }
    public double NextNormal()
    {
        if (hasSpare) { hasSpare = false; return spare; }
        double radius = Math.Sqrt(-2.0 * Math.Log(NextUniform()));
        double angle = 2.0 * Math.PI * NextUniform();
        spare = radius * Math.Sin(angle); hasSpare = true;
        return radius * Math.Cos(angle);
    }
}
