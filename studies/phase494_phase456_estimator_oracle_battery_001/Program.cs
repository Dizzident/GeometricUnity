using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

// Phase494: A8 deterministic estimator/oracle battery.
// Everything through the oracle evaluation and decision contract is frozen before Phase493 is read.

const string Phase493SummaryPath = "studies/phase493_phase456_stored_artifact_failure_decomposition_001/output/phase456_stored_artifact_failure_decomposition_summary.json";
const string ExpectedPhase493SummarySha256 = "dd2d00e7bb1a5dc3869a0b8362d8f2a855cd6b2c80cee62818ad4eb0671b5d85";
const int Extent = 4;
const double OriginalMassTolerance = 1e-10;
const double FullCorrelatorReducedChiSquareCeiling = 3.0;
const double ExactAlternativeResidualCeiling = 1e-8;
const double PositivityTolerance = 1e-12;
const double OriginalRatioDomainFloor = 1.0;
const double SingleMassGridMin = 0.05;
const double SingleMassGridMax = 2.50;
const double SingleMassGridStep = 0.001;
const int ExpectedFamilyRows = 5;
double[] AlternativeMassGrid = Enumerable.Range(1, 10).Select(i => 0.2 * i).ToArray();
double[,] exactGaussianCovariance = BuildCovariance(1e-4, 0.30);

var oracleCases = new[]
{
    new OracleCase("positive-single-pole", Correlator((0.8, 1.0)), "positive-single-pole", 0.8),
    new OracleCase("positive-two-pole", Correlator((0.4, 1.0), (1.2, 0.35)), "positive-mixture", null),
    new OracleCase("signed-two-pole", Correlator((0.4, 1.0), (1.2, -0.6)), "sign-indefinite-mixture", null),
    new OracleCase("negative-scale-single-pole", Correlator((0.8, -1.0)), "negative-scale", 0.8),
    new OracleCase("massless-boundary", new[] { 2.0, 2.0, 2.0, 2.0 }, "massless-boundary", 0.0),
};

string contractMaterial = string.Join('|',
    Extent, OriginalMassTolerance, FullCorrelatorReducedChiSquareCeiling,
    ExactAlternativeResidualCeiling, PositivityTolerance, OriginalRatioDomainFloor,
    SingleMassGridMin, SingleMassGridMax, SingleMassGridStep, ExpectedFamilyRows,
    string.Join(';', AlternativeMassGrid.Select(x => x.ToString("R"))),
    string.Join(';', oracleCases.Select(x => x.Id + ":" + x.Kind + ":" + string.Join(',', x.C.Select(v => v.ToString("R"))))),
    "original:t1-over-t2-cosh-bisection-ratio>1",
    "alternative:covariance-aware-single-cosh-and-fixed-grid-two-pole-gls",
    "invalid:any-invalid-row-withholds-family-threshold-and-invalidates-all-five" );
string decisionContractSha256 = Sha256(Encoding.UTF8.GetBytes(contractMaterial));

// Oracle work occurs before any Phase493 byte or terminal is consumed.
double[,] covarianceInverse = Invert(exactGaussianCovariance);
var caseResults = oracleCases.Select(x => EvaluateCase(x, covarianceInverse)).ToArray();
var batteries = new List<BatteryResult>();

var positiveSingle = caseResults.Single(x => x.Id == "positive-single-pole");
batteries.Add(new BatteryResult("B1", "original-cosh-domain-and-exact-single-pole-recovery",
    positiveSingle.OriginalMass is double om && Math.Abs(om - 0.8) <= OriginalMassTolerance,
    new { positiveSingle.OriginalRatio, positiveSingle.OriginalMass, expectedMass = 0.8, tolerance = OriginalMassTolerance }));

var positiveMixture = caseResults.Single(x => x.Id == "positive-two-pole");
batteries.Add(new BatteryResult("B2", "T4-ratio-identifiability-versus-spectral-content",
    positiveMixture.OriginalMass is not null && double.IsFinite(positiveMixture.SinglePoleReducedChiSquare) &&
    positiveMixture.PositiveTwoPoleReducedChiSquare <= ExactAlternativeResidualCeiling,
    new { positiveMixture.OriginalMass, positiveMixture.SinglePoleReducedChiSquare, positiveMixture.PositiveTwoPoleReducedChiSquare,
        singlePoleRejectedAtFrozenCovariance = positiveMixture.SinglePoleReducedChiSquare > FullCorrelatorReducedChiSquareCeiling,
        interpretation = "the t=1 ratio returns one effective mass but cannot identify the positive two-pole content; the full-correlator residual reports whether the frozen covariance resolves the mismatch" }));

batteries.Add(new BatteryResult("B3", "covariance-aware-full-correlator-single-pole-control",
    positiveSingle.SinglePoleReducedChiSquare <= ExactAlternativeResidualCeiling,
    new { positiveSingle.SinglePoleMass, positiveSingle.SinglePoleAmplitude, positiveSingle.SinglePoleReducedChiSquare,
        ceiling = ExactAlternativeResidualCeiling }));

batteries.Add(new BatteryResult("B4", "target-blind-positive-two-pole-alternative",
    positiveMixture.PositiveTwoPoleFitFeasible && positiveMixture.PositiveTwoPoleReducedChiSquare <= ExactAlternativeResidualCeiling,
    new { positiveMixture.PositiveTwoPoleMass1, positiveMixture.PositiveTwoPoleMass2,
        positiveMixture.PositiveTwoPoleAmplitude1, positiveMixture.PositiveTwoPoleAmplitude2,
        positiveMixture.PositiveTwoPoleReducedChiSquare, massGrid = AlternativeMassGrid }));

var signed = caseResults.Single(x => x.Id == "signed-two-pole");
var negative = caseResults.Single(x => x.Id == "negative-scale-single-pole");
var massless = caseResults.Single(x => x.Id == "massless-boundary");
batteries.Add(new BatteryResult("B5", "sign-and-positivity-assumption-tripwires",
    signed.OriginalMass is null && signed.SignedTwoPoleFitFeasible && !signed.PositiveTwoPoleFitFeasible &&
    negative.OriginalMass is not null && !negative.PositiveTwoPoleFitFeasible && massless.OriginalMass is null,
    new
    {
        signedRatio = signed.OriginalRatio,
        signedOriginalMass = signed.OriginalMass,
        signed.SignedTwoPoleFitFeasible,
        signedPositiveTwoPoleFitFeasible = signed.PositiveTwoPoleFitFeasible,
        negativeScaleOriginalMass = negative.OriginalMass,
        negativePositiveTwoPoleFitFeasible = negative.PositiveTwoPoleFitFeasible,
        masslessRatio = massless.OriginalRatio,
        masslessOriginalMass = massless.OriginalMass,
        interpretation = "the original ratio rejects ratio<=1 but accepts an overall-negative correlator; positivity must therefore be an explicit channel precondition",
    }));

var invalidPropagation = EvaluateInvalidRowPropagation();
batteries.Add(new BatteryResult("B6", "invalid-row-family-firewall-propagation", invalidPropagation.Passed, invalidPropagation));

bool originalEstimatorIdentifiableOnSinglePoleDomain = batteries.Single(x => x.Id == "B1").Passed;
bool originalEstimatorStructurallyIdentifiable = false; // T=4 ratio alone cannot identify spectral content or enforce positivity.
bool alternativeEstimatorFeasible = batteries.Single(x => x.Id == "B3").Passed && batteries.Single(x => x.Id == "B4").Passed;
bool channelNonidentifiabilityObserved = batteries.Single(x => x.Id == "B5").Passed;
bool covarianceAwareResidualsPassed = batteries.Single(x => x.Id == "B2").Passed && batteries.Single(x => x.Id == "B3").Passed;
bool covarianceAwareFullCorrelatorDiscriminatesMixture = positiveMixture.SinglePoleReducedChiSquare > FullCorrelatorReducedChiSquareCeiling;
bool invalidRowPropagationPassed = batteries.Single(x => x.Id == "B6").Passed;
bool oracleBatteriesPassed = batteries.All(x => x.Passed);
bool statisticsOrPowerLimitationSupported = false; // exact analytic means and exact covariance, no finite-chain inference.
bool positiveOracleChannelValid = positiveSingle.PositivitySatisfied && positiveMixture.PositivitySatisfied;
bool allOracleChannelsValid = caseResults.All(x => x.PositivitySatisfied);

// Only now consume Phase493's identity/schema, after estimators and thresholds are frozen and evaluated.
var precursor = ValidatePhase493(Phase493SummaryPath);
bool inputsValid = precursor.Valid && oracleBatteriesPassed;
string oracleClassification = !inputsValid ? "invalid-oracle-battery"
    : originalEstimatorIdentifiableOnSinglePoleDomain && alternativeEstimatorFeasible && channelNonidentifiabilityObserved
        ? "mixed-estimator-outcome"
        : channelNonidentifiabilityObserved && !alternativeEstimatorFeasible
            ? "channel-nonidentifiable"
            : alternativeEstimatorFeasible && !originalEstimatorStructurallyIdentifiable
                ? "alternative-estimator-feasible"
                : "original-estimator-identifiable";
string terminalStatus = "phase456-estimator-oracle-battery-" + oracleClassification;

var result = new
{
    phaseId = "phase494-phase456-estimator-oracle-battery",
    terminalStatus,
    verdictKind = oracleClassification,
    inputsValid,
    oracleClassification,
    taxonomy = new[]
    {
        "original-estimator-identifiable",
        "alternative-estimator-feasible",
        "channel-nonidentifiable",
        "mixed-estimator-outcome",
        "invalid-oracle-battery",
    },
    decisionContractSha256,
    frozenBeforePhase493Consumption = true,
    frozenConfiguration = new
    {
        extent = Extent,
        originalEstimator = "Phase456 t=1 ratio C(1)/C(2), accepted only when finite and >1, solved against cosh(m) at T=4",
        originalMassTolerance = OriginalMassTolerance,
        fullCorrelatorReducedChiSquareCeiling = FullCorrelatorReducedChiSquareCeiling,
        exactAlternativeResidualCeiling = ExactAlternativeResidualCeiling,
        positivityTolerance = PositivityTolerance,
        covariance = ToRows(exactGaussianCovariance),
        singleMassGrid = new { min = SingleMassGridMin, max = SingleMassGridMax, step = SingleMassGridStep },
        alternativeMassGrid = AlternativeMassGrid,
        invalidRowRule = "one invalid full estimate or aligned replicate withholds the family threshold; a missing threshold makes every family row invalid",
    },
    phase493Binding = precursor,
    batteries,
    oracleCases = caseResults,
    invalidRowPropagation = invalidPropagation,
    originalEstimatorIdentifiableOnSinglePoleDomain,
    originalEstimatorStructurallyIdentifiable,
    t4OriginalRatioUnderdeterminesSpectralContent = true,
    alternativeEstimatorFeasible,
    covarianceAwareResidualsPassed,
    covarianceAwareFullCorrelatorDiscriminatesMixture,
    channelNonidentifiabilityObserved,
    positiveOracleChannelValid,
    allOracleChannelsValid,
    statisticsOrPowerLimitationSupported,
    invalidRowPropagationPassed,
    exactGaussianSyntheticInputsOnly = true,
    hmcRun = false,
    performanceBenchmarkRun = false,
    phase456ArtifactReadOrReinterpreted = false,
    phase456ArtifactMutated = false,
    phase456TerminalChanged = false,
    explorationOnly = true,
    confirmationEvidence = false,
    targetBlindConstruction = true,
    physicalTargetsConsultedForConstruction = false,
    humanRulingAuthored = false,
    o4Discharged = false,
    phase458EvaluationAuthorized = false,
    phase458G3Satisfied = false,
    phase458G5Satisfied = false,
    productionAuthorized = false,
    samplingAuthorized = false,
    phase481RepairPackConstructed = false,
    sourceContractApplicationAllowed = false,
    phase201TemplateMutated = false,
    fieldsAppliedToPhase201TemplateCount = 0,
    acceptedContractFieldCount = 0,
    noGevPromotion = true,
    promotedPhysicalMassClaimCount = 0,
    decision = oracleClassification switch
    {
        "mixed-estimator-outcome" => "The original estimator is exact on its positive single-pole domain, a frozen covariance-aware alternative resolves the positive mixture oracle, and sign-indefinite or negative-scale channels remain nonidentifiable without an explicit positivity premise. This exploration does not reinterpret Phase456 or authorize repair or sampling.",
        "alternative-estimator-feasible" => "A frozen covariance-aware target-blind alternative passes the oracle while the original estimator is not structurally sufficient. This exploration does not reinterpret Phase456 or authorize repair or sampling.",
        "channel-nonidentifiable" => "The oracle exposes unresolved channel sign or positivity assumptions that prevent identification. This exploration does not reinterpret Phase456 or authorize repair or sampling.",
        "original-estimator-identifiable" => "The original estimator passes every frozen applicable oracle. This exploration does not reinterpret Phase456 or authorize repair or sampling.",
        _ => "The precursor binding or an oracle battery is invalid. Fail closed; do not reinterpret Phase456 or authorize repair or sampling.",
    },
};

string outputDirectory = "studies/phase494_phase456_estimator_oracle_battery_001/output";
Directory.CreateDirectory(outputDirectory);
var jsonOptions = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
string json = JsonSerializer.Serialize(result, jsonOptions);
File.WriteAllText(Path.Combine(outputDirectory, "phase456_estimator_oracle_battery.json"), json);
File.WriteAllText(Path.Combine(outputDirectory, "phase456_estimator_oracle_battery_summary.json"), json);
Console.WriteLine(terminalStatus);
Console.WriteLine($"inputsValid={inputsValid} batteries={batteries.Count(x => x.Passed)}/{batteries.Count} precursor={precursor.FailureClassification}");
foreach (var battery in batteries) Console.WriteLine($"{battery.Id} {battery.Name}: {battery.Passed}");

if (!inputsValid) throw new InvalidOperationException("Phase494 fail-closed: precursor binding or oracle battery invalid.");

OracleResult EvaluateCase(OracleCase oracle, double[,] inverseCovariance)
{
    double ratio = oracle.C[1] / oracle.C[2];
    double? originalMass = SolveOriginal(ratio);
    FitResult single = FitSinglePole(oracle.C, inverseCovariance);
    FitResult positiveTwo = FitTwoPole(oracle.C, inverseCovariance, positive: true);
    FitResult signedTwo = FitTwoPole(oracle.C, inverseCovariance, positive: false);
    bool pointwisePositive = oracle.C.All(x => x > PositivityTolerance);
    bool reflectionSymmetric = Math.Abs(oracle.C[1] - oracle.C[3]) <= 1e-12;
    bool logConvex = oracle.C[1] * oracle.C[1] <= oracle.C[0] * oracle.C[2] + PositivityTolerance;
    bool positivity = pointwisePositive && reflectionSymmetric && logConvex;
    return new OracleResult(oracle.Id, oracle.Kind, oracle.C, ratio, originalMass,
        single.Mass1, single.Amplitude1, single.ReducedChiSquare,
        positiveTwo.Feasible && positiveTwo.ReducedChiSquare <= ExactAlternativeResidualCeiling,
        positiveTwo.Mass1, positiveTwo.Mass2, positiveTwo.Amplitude1, positiveTwo.Amplitude2, positiveTwo.ReducedChiSquare,
        signedTwo.Feasible && signedTwo.ReducedChiSquare <= ExactAlternativeResidualCeiling, signedTwo.ReducedChiSquare,
        pointwisePositive, reflectionSymmetric, logConvex, positivity);
}

static double? SolveOriginal(double ratio)
{
    if (!double.IsFinite(ratio) || ratio <= OriginalRatioDomainFloor) return null;
    return Math.Acosh(ratio); // at T=4,t=1 the committed cosh equation reduces exactly to ratio=cosh(m).
}

static FitResult FitSinglePole(double[] y, double[,] w)
{
    FitResult best = FitResult.Invalid;
    int count = (int)Math.Round((SingleMassGridMax - SingleMassGridMin) / SingleMassGridStep);
    for (int i = 0; i <= count; i++)
    {
        double mass = SingleMassGridMin + i * SingleMassGridStep;
        double[] b = Basis(mass);
        double denominator = Quad(b, w, b);
        double amplitude = Bilinear(b, w, y) / denominator;
        double chi = Residual(y, Scale(b, amplitude), w);
        if (amplitude > 0.0 && chi < best.ChiSquare) best = new FitResult(true, mass, null, amplitude, null, chi, chi / 2.0);
    }
    return best;
}

FitResult FitTwoPole(double[] y, double[,] w, bool positive)
{
    FitResult best = FitResult.Invalid;
    for (int i = 0; i < AlternativeMassGrid.Length; i++)
        for (int j = i + 1; j < AlternativeMassGrid.Length; j++)
        {
            double m1 = AlternativeMassGrid[i], m2 = AlternativeMassGrid[j];
            double[] b1 = Basis(m1), b2 = Basis(m2);
            double g11 = Quad(b1, w, b1), g12 = Bilinear(b1, w, b2), g22 = Quad(b2, w, b2);
            double r1 = Bilinear(b1, w, y), r2 = Bilinear(b2, w, y);
            double determinant = g11 * g22 - g12 * g12;
            if (determinant <= 1e-20) continue;
            double a1 = (r1 * g22 - r2 * g12) / determinant;
            double a2 = (r2 * g11 - r1 * g12) / determinant;
            if (positive && (a1 < -PositivityTolerance || a2 < -PositivityTolerance)) continue;
            double chi = Residual(y, Add(Scale(b1, a1), Scale(b2, a2)), w);
            if (chi < best.ChiSquare) best = new FitResult(true, m1, m2, a1, a2, chi, chi / 1.0);
        }
    return best;
}

static InvalidPropagation EvaluateInvalidRowPropagation()
{
    double?[] full = { 0.2, 0.1, null, 0.0, -0.1 };
    bool allFullFinite = full.All(x => x is double d && double.IsFinite(d));
    double? familyThreshold = allFullFinite ? 3.0 : null;
    string[] terminals = full.Select(x => x is not double || familyThreshold is null ? "invalid" : "free-compatible").ToArray();
    bool passed = !allFullFinite && familyThreshold is null && terminals.Length == ExpectedFamilyRows && terminals.All(x => x == "invalid");
    return new InvalidPropagation(passed, ExpectedFamilyRows, allFullFinite, familyThreshold, terminals,
        "one invalid row prevents calibration; absent threshold propagates invalid to every family row");
}

static PrecursorBinding ValidatePhase493(string path)
{
    if (!File.Exists(path)) return new PrecursorBinding(false, path, null, null, null, null, "missing-summary");
    byte[] bytes = File.ReadAllBytes(path);
    try
    {
        using var doc = JsonDocument.Parse(bytes);
        JsonElement root = doc.RootElement;
        string? phaseId = root.TryGetProperty("phaseId", out var p) ? p.GetString() : null;
        bool precursorInputsValid = root.TryGetProperty("inputsValid", out var iv) && iv.ValueKind == JsonValueKind.True;
        string? classification = root.TryGetProperty("failureClassification", out var fc) ? fc.GetString() : null;
        string? contractHash = root.TryGetProperty("decisionContractSha256", out var ch) ? ch.GetString() : null;
        string[] taxonomy = { "sampler-defect-supported", "estimator-domain-failure-supported", "statistics-limited-supported", "mixed-failure-supported", "failure-source-unresolved", "invalid-committed-artifact" };
        string byteHash = Sha256(bytes);
        bool valid = byteHash == ExpectedPhase493SummarySha256 &&
            phaseId == "phase493-phase456-stored-artifact-failure-decomposition" && precursorInputsValid &&
            classification is not null && taxonomy.Contains(classification) && contractHash is { Length: 64 } && contractHash.All(Uri.IsHexDigit);
        return new PrecursorBinding(valid, path, byteHash, phaseId, classification, contractHash, valid ? "exact-bytes-and-schema-bound" : "byte-schema-or-contract-drift");
    }
    catch (Exception ex)
    {
        return new PrecursorBinding(false, path, Sha256(bytes), null, null, null, "json-invalid:" + ex.GetType().Name);
    }
}

static double[] Correlator(params (double Mass, double Amplitude)[] poles)
{
    var c = new double[Extent];
    for (int t = 0; t < Extent; t++)
        foreach (var pole in poles) c[t] += pole.Amplitude * (Math.Exp(-pole.Mass * t) + Math.Exp(-pole.Mass * (Extent - t)));
    return c;
}

static double[] Basis(double mass) => Enumerable.Range(0, Extent)
    .Select(t => Math.Exp(-mass * t) + Math.Exp(-mass * (Extent - t))).ToArray();
static double[] Scale(double[] x, double a) => x.Select(v => a * v).ToArray();
static double[] Add(double[] x, double[] y) => x.Zip(y, (a, b) => a + b).ToArray();
static double Bilinear(double[] x, double[,] a, double[] y)
{
    double sum = 0.0;
    for (int i = 0; i < x.Length; i++) for (int j = 0; j < y.Length; j++) sum += x[i] * a[i, j] * y[j];
    return sum;
}
static double Quad(double[] x, double[,] a, double[] y) => Bilinear(x, a, y);
static double Residual(double[] y, double[] prediction, double[,] w)
{
    double[] d = y.Zip(prediction, (a, b) => a - b).ToArray();
    return Quad(d, w, d);
}
static double[,] BuildCovariance(double variance, double rho)
{
    var result = new double[Extent, Extent];
    for (int i = 0; i < Extent; i++) for (int j = 0; j < Extent; j++) result[i, j] = variance * Math.Pow(rho, Math.Abs(i - j));
    return result;
}
static double[,] Invert(double[,] input)
{
    int n = input.GetLength(0);
    var augmented = new double[n, 2 * n];
    for (int i = 0; i < n; i++) for (int j = 0; j < n; j++) { augmented[i, j] = input[i, j]; augmented[i, n + j] = i == j ? 1.0 : 0.0; }
    for (int column = 0; column < n; column++)
    {
        int pivot = Enumerable.Range(column, n - column).MaxBy(row => Math.Abs(augmented[row, column]));
        if (Math.Abs(augmented[pivot, column]) < 1e-20) throw new InvalidOperationException("Frozen covariance is singular.");
        for (int j = 0; j < 2 * n; j++) (augmented[column, j], augmented[pivot, j]) = (augmented[pivot, j], augmented[column, j]);
        double scale = augmented[column, column];
        for (int j = 0; j < 2 * n; j++) augmented[column, j] /= scale;
        for (int row = 0; row < n; row++) if (row != column)
        {
            double factor = augmented[row, column];
            for (int j = 0; j < 2 * n; j++) augmented[row, j] -= factor * augmented[column, j];
        }
    }
    var inverse = new double[n, n];
    for (int i = 0; i < n; i++) for (int j = 0; j < n; j++) inverse[i, j] = augmented[i, n + j];
    return inverse;
}
static double[][] ToRows(double[,] matrix) => Enumerable.Range(0, matrix.GetLength(0))
    .Select(i => Enumerable.Range(0, matrix.GetLength(1)).Select(j => matrix[i, j]).ToArray()).ToArray();
static string Sha256(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

sealed class OracleCase(string id, double[] c, string kind, double? sourceMass)
{ public string Id { get; } = id; public double[] C { get; } = c; public string Kind { get; } = kind; public double? SourceMass { get; } = sourceMass; }
sealed class FitResult(bool feasible, double? mass1, double? mass2, double? amplitude1, double? amplitude2, double chiSquare, double reducedChiSquare)
{
    public static FitResult Invalid => new(false, null, null, null, null, double.MaxValue, double.MaxValue);
    public bool Feasible { get; } = feasible; public double? Mass1 { get; } = mass1; public double? Mass2 { get; } = mass2;
    public double? Amplitude1 { get; } = amplitude1; public double? Amplitude2 { get; } = amplitude2;
    public double ChiSquare { get; } = chiSquare; public double ReducedChiSquare { get; } = reducedChiSquare;
}
sealed class OracleResult(string id, string kind, double[] c, double originalRatio, double? originalMass,
    double? singlePoleMass, double? singlePoleAmplitude, double singlePoleReducedChiSquare,
    bool positiveTwoPoleFitFeasible, double? positiveTwoPoleMass1, double? positiveTwoPoleMass2,
    double? positiveTwoPoleAmplitude1, double? positiveTwoPoleAmplitude2, double positiveTwoPoleReducedChiSquare,
    bool signedTwoPoleFitFeasible, double signedTwoPoleReducedChiSquare,
    bool pointwisePositive, bool reflectionSymmetric, bool logConvex, bool positivitySatisfied)
{
    public string Id { get; } = id; public string Kind { get; } = kind; public double[] C { get; } = c;
    public double OriginalRatio { get; } = originalRatio; public double? OriginalMass { get; } = originalMass;
    public double? SinglePoleMass { get; } = singlePoleMass; public double? SinglePoleAmplitude { get; } = singlePoleAmplitude;
    public double SinglePoleReducedChiSquare { get; } = singlePoleReducedChiSquare;
    public bool PositiveTwoPoleFitFeasible { get; } = positiveTwoPoleFitFeasible;
    public double? PositiveTwoPoleMass1 { get; } = positiveTwoPoleMass1; public double? PositiveTwoPoleMass2 { get; } = positiveTwoPoleMass2;
    public double? PositiveTwoPoleAmplitude1 { get; } = positiveTwoPoleAmplitude1; public double? PositiveTwoPoleAmplitude2 { get; } = positiveTwoPoleAmplitude2;
    public double PositiveTwoPoleReducedChiSquare { get; } = positiveTwoPoleReducedChiSquare;
    public bool SignedTwoPoleFitFeasible { get; } = signedTwoPoleFitFeasible; public double SignedTwoPoleReducedChiSquare { get; } = signedTwoPoleReducedChiSquare;
    public bool PointwisePositive { get; } = pointwisePositive; public bool ReflectionSymmetric { get; } = reflectionSymmetric;
    public bool LogConvex { get; } = logConvex; public bool PositivitySatisfied { get; } = positivitySatisfied;
}
sealed class BatteryResult(string id, string name, bool passed, object metrics)
{ public string Id { get; } = id; public string Name { get; } = name; public bool Passed { get; } = passed; public object Metrics { get; } = metrics; }
sealed class InvalidPropagation(bool passed, int rowCount, bool allFullFinite, double? familyThreshold, string[] terminals, string rule)
{ public bool Passed { get; } = passed; public int RowCount { get; } = rowCount; public bool AllFullFinite { get; } = allFullFinite;
  public double? FamilyThreshold { get; } = familyThreshold; public string[] Terminals { get; } = terminals; public string Rule { get; } = rule; }
sealed class PrecursorBinding(bool valid, string path, string? byteSha256, string? phaseId, string? failureClassification, string? decisionContractSha256, string status)
{ public bool Valid { get; } = valid; public string Path { get; } = path; public string? ByteSha256 { get; } = byteSha256;
  public string? PhaseId { get; } = phaseId; public string? FailureClassification { get; } = failureClassification;
  public string? DecisionContractSha256 { get; } = decisionContractSha256; public string Status { get; } = status; }
