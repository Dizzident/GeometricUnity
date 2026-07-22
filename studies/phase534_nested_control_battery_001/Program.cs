using System.Security.Cryptography;
using System.Text.Json;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.ReferenceCpu;

const string PhaseDir = "studies/phase534_nested_control_battery_001";
const string ContractPath = PhaseDir + "/preregistration/phase534_nested_control_contract_v1.json";
const string Phase533ContractPath = "studies/phase533_nested_validation_contract_001/preregistration/phase533_nested_validation_contract_v1.json";
const string OutputPath = PhaseDir + "/output/nested_control_battery.json";
const string SummaryPath = PhaseDir + "/output/nested_control_battery_summary.json";

byte[] contractBytes = File.ReadAllBytes(ContractPath);
using JsonDocument contractDocument = JsonDocument.Parse(contractBytes);
JsonElement contract = contractDocument.RootElement;
var bindings = contract.GetProperty("expectedInputs").EnumerateArray().Select(x => Bind(
    x.GetProperty("id").GetString()!, x.GetProperty("path").GetString()!, x.GetProperty("sha256").GetString()!)).ToArray();
using JsonDocument phase533Document = JsonDocument.Parse(File.ReadAllText(Phase533ContractPath));
JsonElement phase533 = phase533Document.RootElement;
JsonElement controls = phase533.GetProperty("controlConfiguration");
var seedTables = phase533.GetProperty("seedTables").EnumerateArray().Select(t => new SeedTable(
    t.GetProperty("id").GetString()!,
    t.GetProperty("seeds").EnumerateArray().Select(x => x.GetInt32()).ToArray(),
    t.GetProperty("initialScales").EnumerateArray().Select(x => x.GetDouble()).ToArray())).ToArray();
JsonElement reduction = contract.GetProperty("registeredReduction");
JsonElement quadratureSpec = contract.GetProperty("quadrature");

bool contractValid = contract.GetProperty("schemaVersion").GetInt32() == 1
    && contract.GetProperty("contractId").GetString() == "phase534-a22-nested-control-battery-v1"
    && contract.GetProperty("frozenBeforeControlExecution").GetBoolean()
    && bindings.Length == 3 && bindings.All(x => x.HashMatches)
    && seedTables.Length == 2 && seedTables.All(x => x.Seeds.Length == 4 && x.InitialScales.Length == 4)
    && reduction.GetProperty("member").GetString() == "sd2-id0/c0.5"
    && reduction.GetProperty("extent").GetInt32() == 3
    && reduction.GetProperty("thetaRule").GetString() == "theta-identically-zero"
    && !contract.GetProperty("claimBoundary").GetProperty("physicalClaimAllowed").GetBoolean()
    && contract.GetProperty("claimBoundary").GetProperty("promotedPhysicalMassClaimCount").GetInt32() == 0;

int gaussianDimension = controls.GetProperty("gaussianDimension").GetInt32();
int gaussianDraws = controls.GetProperty("gaussianDrawsPerSeed").GetInt32();
int freeWarmup = controls.GetProperty("freeWarmupPerSeed").GetInt32();
int freeRetained = controls.GetProperty("freeRetainedPerSeed").GetInt32();
int reducedWarmup = controls.GetProperty("reducedWarmupPerSeed").GetInt32();
int reducedRetained = controls.GetProperty("reducedRetainedPerSeed").GetInt32();
int leapfrogSteps = controls.GetProperty("leapfrogSteps").GetInt32();
double acceptanceMin = controls.GetProperty("targetAcceptanceMinimum").GetDouble();
double acceptanceMax = controls.GetProperty("targetAcceptanceMaximum").GetDouble();
double rhatMaximum = controls.GetProperty("rhatMaximum").GetDouble();
double essMinimum = controls.GetProperty("bulkEssMinimum").GetDouble();
double seMultiplier = controls.GetProperty("simultaneousStandardErrorMultiplier").GetDouble();

// Independent direct draws: this code shares neither an HMC trajectory nor an
// action implementation with the controls below.
var gaussianRows = new List<object>();
double gaussianWorstZ = 0.0;
foreach (SeedTable table in seedTables)
{
    foreach (int seed in table.Seeds)
    {
        var rng = new Random(seed + 700_000);
        var sums = new double[gaussianDimension];
        var sums2 = new double[gaussianDimension];
        for (int n = 0; n < gaussianDraws; n++)
            for (int d = 0; d < gaussianDimension; d++)
            {
                double x = Gaussian(rng);
                sums[d] += x; sums2[d] += x * x;
            }
        for (int d = 0; d < gaussianDimension; d++)
        {
            double mean = sums[d] / gaussianDraws;
            double second = sums2[d] / gaussianDraws;
            double meanZ = System.Math.Abs(mean) * System.Math.Sqrt(gaussianDraws);
            double secondZ = System.Math.Abs(second - 1.0) / System.Math.Sqrt(2.0 / gaussianDraws);
            gaussianWorstZ = System.Math.Max(gaussianWorstZ, System.Math.Max(meanZ, secondZ));
        }
        gaussianRows.Add(new { table = table.Id, seed, sampleCount = gaussianDraws });
    }
}
bool analysisControlPassed = gaussianWorstZ <= seMultiplier;

var freeTables = new List<SamplerTableResult>();
foreach (SeedTable table in seedTables)
{
    var chains = new List<ChainResult>();
    for (int c = 0; c < table.Seeds.Length; c++)
        chains.Add(RunVectorHmc(table.Seeds[c], gaussianDimension, table.InitialScales[c],
            freeWarmup, freeRetained, leapfrogSteps));
    freeTables.Add(SummarizeTable(table.Id, chains));
}
bool freeSamplerPassed = freeTables.All(x => x.AcceptanceRate >= acceptanceMin
    && x.AcceptanceRate <= acceptanceMax && x.NonFiniteCount == 0 && x.DivergenceCount == 0
    && x.ExpMinusDeltaHError <= System.Math.Max(0.01, 3.0 * x.ExpMinusDeltaHSe)
    && x.Rhat <= rhatMaximum && x.Ess >= essMinimum
    && System.Math.Abs(x.Mean) <= seMultiplier * x.MeanSe
    && System.Math.Abs(x.SecondMoment - 1.0) <= seMultiplier * x.SecondMomentSe);

// Reconstruct the exact-theta-zero registered SD2 action on the Phase527
// witness ray. The known exact cubic coefficient is an independent guard.
var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
int extent = reduction.GetProperty("extent").GetInt32();
var mesh = SimplicialMeshGenerator.CreateUniform4DPeriodic(extent, latticeCanonical: true);
var member = new EinsteinianShiabFamilyMember
{
    Phi1 = InvariantElementSpec.Sd2, Phi2 = InvariantElementSpec.Id0,
    EinsteinCoefficient = 0.5, EpsilonMode = "independent-theta",
};
var op = new EinsteinianShiabOperator(mesh, algebra, member, latticePeriod: extent);
var mass = new CpuMassMatrix(mesh, algebra);
var ray = new double[mesh.EdgeCount * algebra.Dimension];
int[] witnessEdges = reduction.GetProperty("witnessEdges").EnumerateArray().Select(x => x.GetInt32()).ToArray();
int[] witnessComponents = reduction.GetProperty("witnessComponents").EnumerateArray().Select(x => x.GetInt32()).ToArray();
int[] witnessValues = reduction.GetProperty("witnessValues").EnumerateArray().Select(x => x.GetInt32()).ToArray();
for (int i = 0; i < witnessEdges.Length; i++) ray[witnessEdges[i] * algebra.Dimension + witnessComponents[i]] = witnessValues[i];
var thetaZero = new double[mesh.VertexCount * algebra.Dimension];
double Action(double a)
{
    var omega = new double[ray.Length];
    for (int i = 0; i < ray.Length; i++) omega[i] = a * ray[i];
    return op.ComputeJointGradient(omega, thetaZero, mass).Objective;
}
double s1p = Action(1), s1m = Action(-1), s2p = Action(2), s2m = Action(-2);
double even1 = 0.5 * (s1p + s1m), even2 = 0.5 * (s2p + s2m);
double c4 = (even2 - 4.0 * even1) / 12.0;
double c2 = even1 - c4;
double c3 = 0.5 * (s1p - s1m);
double expectedC3 = reduction.GetProperty("expectedCubicCoefficient").GetDouble();
double coefficientTolerance = reduction.GetProperty("coefficientTolerance").GetDouble();
double replayTolerance = reduction.GetProperty("polynomialReplayRelativeTolerance").GetDouble();
double Potential(double x) => c2 * x * x + c3 * x * x * x + c4 * x * x * x * x;
double Gradient(double x) => 2 * c2 * x + 3 * c3 * x * x + 4 * c4 * x * x * x;
double replayWorst = new[] { -1.5, -0.5, 0.5, 1.5 }.Max(x =>
    System.Math.Abs(Action(x) - Potential(x)) / System.Math.Max(1.0, System.Math.Abs(Action(x))));
bool polynomialReplayPassed = c2 > 0 && c4 > 0
    && System.Math.Abs(c3 - expectedC3) <= coefficientTolerance
    && replayWorst <= replayTolerance;

int coarsePanels = quadratureSpec.GetProperty("coarsePanels").GetInt32();
int finePanels = quadratureSpec.GetProperty("finePanels").GetInt32();
double halfWidth = quadratureSpec.GetProperty("initialHalfWidth").GetDouble();
double maxHalfWidth = quadratureSpec.GetProperty("maximumHalfWidth").GetDouble();
QuadratureResult fine = default, coarse = default;
while (true)
{
    coarse = IntegrateMoments(Potential, halfWidth, coarsePanels);
    fine = IntegrateMoments(Potential, halfWidth, finePanels);
    double edge = System.Math.Max(System.Math.Exp(-Potential(-halfWidth)), System.Math.Exp(-Potential(halfWidth))) / fine.Z;
    if (edge <= quadratureSpec.GetProperty("boundaryDensityTolerance").GetDouble() || halfWidth >= maxHalfWidth) break;
    halfWidth *= 2;
}
double quadratureDifference = MaxRelativeMomentDifference(coarse, fine);
bool quadraturePassed = double.IsFinite(fine.Z) && fine.Z > 0
    && halfWidth <= maxHalfWidth
    && quadratureDifference <= quadratureSpec.GetProperty("nestedAgreementTolerance").GetDouble()
    && System.Math.Max(System.Math.Exp(-Potential(-halfWidth)), System.Math.Exp(-Potential(halfWidth))) / fine.Z
        <= quadratureSpec.GetProperty("boundaryDensityTolerance").GetDouble();

var reducedTables = new List<SamplerTableResult>();
foreach (SeedTable table in seedTables)
{
    var chains = new List<ChainResult>();
    for (int c = 0; c < table.Seeds.Length; c++)
        chains.Add(RunScalarHmc(table.Seeds[c] + 900_000, table.InitialScales[c],
            reducedWarmup, reducedRetained, leapfrogSteps, Potential, Gradient));
    reducedTables.Add(SummarizeTable(table.Id, chains));
}
bool reducedSamplerDiagnosticsPassed = reducedTables.All(x => x.AcceptanceRate >= acceptanceMin
    && x.AcceptanceRate <= acceptanceMax && x.NonFiniteCount == 0 && x.DivergenceCount == 0
    && x.ExpMinusDeltaHError <= System.Math.Max(0.01, 3.0 * x.ExpMinusDeltaHSe)
    && x.Rhat <= rhatMaximum && x.Ess >= essMinimum);
bool reducedMomentAgreement = reducedTables.All(x =>
    System.Math.Abs(x.Mean - fine.M1) <= seMultiplier * x.MeanSe + quadratureDifference
    && System.Math.Abs(x.SecondMoment - fine.M2) <= seMultiplier * x.SecondMomentSe + quadratureDifference);
bool schwingerDysonPassed = reducedTables.All(x =>
{
    double residual = x.XGradientMean - 1.0;
    return System.Math.Abs(residual) <= seMultiplier * x.XGradientSe + quadratureDifference;
});
bool reducedInteractingPassed = polynomialReplayPassed && quadraturePassed
    && reducedSamplerDiagnosticsPassed && reducedMomentAgreement && schwingerDysonPassed;

string verdict = !contractValid ? "invalid-or-drifted-input"
    : !analysisControlPassed ? "analysis-control-failed"
    : !freeSamplerPassed ? "free-sampler-control-failed"
    : !reducedInteractingPassed ? "reduced-interacting-control-failed"
    : "nested-controls-closed";
bool phase534Passed = verdict == "nested-controls-closed";
var result = new
{
    schemaVersion = 1, phase = 534, phaseId = "phase534-nested-control-battery",
    contractSha256 = Sha(contractBytes), contractValid, bindings,
    analysisControl = new { passed = analysisControlPassed, dimension = gaussianDimension, gaussianDraws, gaussianWorstZ, rows = gaussianRows },
    freeSamplerControl = new { passed = freeSamplerPassed, tables = freeTables },
    reducedInteractingControl = new
    {
        passed = reducedInteractingPassed, member = "sd2-id0/c0.5", extent,
        thetaRule = "theta-identically-zero", witnessEdges, witnessComponents, witnessValues,
        polynomial = new { c2, c3, c4, expectedC3, replayWorst, polynomialReplayPassed },
        quadrature = new { passed = quadraturePassed, halfWidth, coarsePanels, finePanels, quadratureDifference, fine },
        samplerDiagnosticsPassed = reducedSamplerDiagnosticsPassed, reducedMomentAgreement,
        schwingerDysonPassed, tables = reducedTables
    },
    verdictKind = verdict, terminalStatus = "nested-control-battery-" + verdict, phase534Passed,
    completeLatticeSd2Validated = false, dynamicalThetaValidated = false, productionValidated = false,
    phase481PackCreated = false, phase458G3Satisfied = false, phase458G4Satisfied = false,
    phase458G5Satisfied = false, o4Discharged = false, physicalClaimAllowed = false,
    promotedPhysicalMassClaimCount = 0,
    decision = phase534Passed
        ? "The independent Gaussian, free-sampler, and reduced interacting controls closed under both frozen seed tables. This does not validate the complete lattice action or dynamical theta."
        : "The earliest failing nested control is preserved. No downstream pilot may treat the control stack as validated."
};
Directory.CreateDirectory(Path.GetDirectoryName(OutputPath)!);
var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
byte[] json = JsonSerializer.SerializeToUtf8Bytes(result, options);
File.WriteAllBytes(OutputPath, json); File.WriteAllBytes(SummaryPath, json);
Console.WriteLine($"Phase534 verdict: {verdict}");
Console.WriteLine($"registered-ray polynomial: {c2:R} x^2 + {c3:R} x^3 + {c4:R} x^4");

static ChainResult RunVectorHmc(int seed, int dimension, double initialScale, int warmup, int retained, int nLeap)
{
    var rng = new Random(seed);
    var x = Enumerable.Repeat(initialScale, dimension).ToArray();
    double eps = 0.25;
    var samples = new double[retained];
    var second = new double[retained];
    var xgrad = new double[retained];
    var deltaH = new List<double>();
    int accepted = 0, total = 0, nonfinite = 0, divergences = 0, windowAccepted = 0, windowTotal = 0;
    for (int iter = 0; iter < warmup + retained; iter++)
    {
        var p = Enumerable.Range(0, dimension).Select(_ => Gaussian(rng)).ToArray();
        var oldX = (double[])x.Clone(); var oldP = (double[])p.Clone();
        for (int d = 0; d < dimension; d++) p[d] -= 0.5 * eps * x[d];
        for (int l = 0; l < nLeap; l++)
        {
            for (int d = 0; d < dimension; d++) x[d] += eps * p[d];
            if (l != nLeap - 1) for (int d = 0; d < dimension; d++) p[d] -= eps * x[d];
        }
        for (int d = 0; d < dimension; d++) p[d] -= 0.5 * eps * x[d];
        double dh = 0.5 * (x.Sum(v => v * v) + p.Sum(v => v * v) - oldX.Sum(v => v * v) - oldP.Sum(v => v * v));
        bool finite = double.IsFinite(dh) && x.All(double.IsFinite);
        bool accept = finite && (dh <= 0 || rng.NextDouble() < System.Math.Exp(-dh));
        if (!accept) x = oldX;
        if (!finite) nonfinite++; if (finite && System.Math.Abs(dh) > 100) divergences++;
        if (iter < warmup)
        {
            windowTotal++; if (accept) windowAccepted++;
            if ((iter + 1) % 32 == 0)
            {
                double rate = (double)windowAccepted / windowTotal;
                if (rate > 0.90) eps *= 1.2; else if (rate < 0.65) eps *= 0.8;
                windowAccepted = 0; windowTotal = 0;
            }
        }
        else
        {
            total++; if (accept) accepted++; deltaH.Add(dh);
            int i = iter - warmup; samples[i] = x[0]; second[i] = x[0] * x[0]; xgrad[i] = x.Sum(v => v * v) / dimension;
        }
    }
    return new(samples, second, xgrad, accepted, total, nonfinite, divergences, deltaH.ToArray(), eps);
}

static ChainResult RunScalarHmc(int seed, double initialScale, int warmup, int retained, int nLeap,
    Func<double, double> potential, Func<double, double> gradient)
{
    var rng = new Random(seed); double x = initialScale, eps = 0.12;
    var samples = new double[retained]; var second = new double[retained]; var xgrad = new double[retained];
    var deltaH = new List<double>(); int accepted = 0, total = 0, nonfinite = 0, divergences = 0, wa = 0, wt = 0;
    for (int iter = 0; iter < warmup + retained; iter++)
    {
        double oldX = x, p = Gaussian(rng), oldP = p;
        p -= 0.5 * eps * gradient(x);
        for (int l = 0; l < nLeap; l++)
        {
            x += eps * p;
            if (l != nLeap - 1) p -= eps * gradient(x);
        }
        p -= 0.5 * eps * gradient(x);
        double dh = potential(x) + 0.5 * p * p - potential(oldX) - 0.5 * oldP * oldP;
        bool finite = double.IsFinite(dh) && double.IsFinite(x);
        bool accept = finite && (dh <= 0 || rng.NextDouble() < System.Math.Exp(-dh));
        if (!accept) x = oldX;
        if (!finite) nonfinite++; if (finite && System.Math.Abs(dh) > 100) divergences++;
        if (iter < warmup)
        {
            wt++; if (accept) wa++;
            if ((iter + 1) % 50 == 0)
            {
                double rate = (double)wa / wt;
                if (rate > 0.90) eps *= 1.15; else if (rate < 0.65) eps *= 0.85;
                wa = 0; wt = 0;
            }
        }
        else
        {
            total++; if (accept) accepted++; deltaH.Add(dh);
            int i = iter - warmup; samples[i] = x; second[i] = x * x; xgrad[i] = x * gradient(x);
        }
    }
    return new(samples, second, xgrad, accepted, total, nonfinite, divergences, deltaH.ToArray(), eps);
}

static SamplerTableResult SummarizeTable(string id, List<ChainResult> chains)
{
    double[] all = chains.SelectMany(x => x.Samples).ToArray();
    double[] all2 = chains.SelectMany(x => x.Second).ToArray();
    double[] allXg = chains.SelectMany(x => x.XGradient).ToArray();
    double ess = EffectiveSampleSize(chains.Select(x => x.Samples).ToArray());
    double ess2 = EffectiveSampleSize(chains.Select(x => x.Second).ToArray());
    double essXg = EffectiveSampleSize(chains.Select(x => x.XGradient).ToArray());
    double[] expDh = chains.SelectMany(x => x.DeltaH).Where(double.IsFinite).Select(x => System.Math.Exp(-x)).ToArray();
    double expMean = Mean(expDh), expSe = StdDev(expDh) / System.Math.Sqrt(System.Math.Max(1, expDh.Length));
    return new(id, chains.Sum(x => x.Accepted) / (double)chains.Sum(x => x.Total),
        chains.Sum(x => x.NonFinite), chains.Sum(x => x.Divergences), expMean,
        System.Math.Abs(expMean - 1.0), expSe, Rhat(chains.Select(x => x.Samples).ToArray()), ess,
        Mean(all), StdDev(all) / System.Math.Sqrt(System.Math.Max(1.0, ess)),
        Mean(all2), StdDev(all2) / System.Math.Sqrt(System.Math.Max(1.0, ess2)),
        Mean(allXg), StdDev(allXg) / System.Math.Sqrt(System.Math.Max(1.0, essXg)),
        chains.Select(x => x.StepSize).ToArray());
}

static QuadratureResult IntegrateMoments(Func<double, double> potential, double halfWidth, int panels)
{
    double h = 2 * halfWidth / panels;
    var sums = new double[5];
    for (int i = 0; i <= panels; i++)
    {
        double x = -halfWidth + i * h;
        double w = i == 0 || i == panels ? 1 : i % 2 == 0 ? 2 : 4;
        double density = System.Math.Exp(-potential(x));
        double power = 1;
        for (int k = 0; k < sums.Length; k++) { sums[k] += w * density * power; power *= x; }
    }
    for (int k = 0; k < sums.Length; k++) sums[k] *= h / 3.0;
    return new(sums[0], sums[1] / sums[0], sums[2] / sums[0], sums[3] / sums[0], sums[4] / sums[0]);
}

static double MaxRelativeMomentDifference(QuadratureResult a, QuadratureResult b) =>
    new[] { Rel(a.M1, b.M1), Rel(a.M2, b.M2), Rel(a.M3, b.M3), Rel(a.M4, b.M4) }.Max();
static double Rel(double a, double b) => System.Math.Abs(a - b) / System.Math.Max(1.0, System.Math.Max(System.Math.Abs(a), System.Math.Abs(b)));
static double Mean(double[] x) => x.Length == 0 ? double.NaN : x.Average();
static double StdDev(double[] x)
{
    if (x.Length < 2) return double.NaN;
    double m = Mean(x); return System.Math.Sqrt(x.Sum(v => (v - m) * (v - m)) / (x.Length - 1));
}
static double Rhat(double[][] chains)
{
    int m = chains.Length, n = chains.Min(x => x.Length);
    double[] means = chains.Select(x => x.Take(n).Average()).ToArray();
    double w = chains.Select(x => StdDev(x.Take(n).ToArray())).Select(x => x * x).Average();
    double b = n * means.Sum(x => (x - means.Average()) * (x - means.Average())) / (m - 1);
    return System.Math.Sqrt((((n - 1.0) / n) * w + b / n) / w);
}
static double EffectiveSampleSize(double[][] chains)
{
    int m = chains.Length, n = chains.Min(x => x.Length);
    double tau = 1.0;
    for (int lag = 1; lag < System.Math.Min(n / 2, 1000); lag++)
    {
        double rho = chains.Select(c => AutoCorrelation(c, lag)).Average();
        if (rho <= 0) break;
        tau += 2 * rho;
    }
    return m * n / tau;
}
static double AutoCorrelation(double[] x, int lag)
{
    double mean = x.Average(), den = x.Sum(v => (v - mean) * (v - mean));
    if (den == 0) return 0;
    double num = 0; for (int i = 0; i < x.Length - lag; i++) num += (x[i] - mean) * (x[i + lag] - mean);
    return num / den;
}
static double Gaussian(Random rng)
{
    double u1 = 1.0 - rng.NextDouble(), u2 = rng.NextDouble();
    return System.Math.Sqrt(-2.0 * System.Math.Log(u1)) * System.Math.Cos(2.0 * System.Math.PI * u2);
}
static Binding Bind(string id, string path, string expected)
{
    string actual = Sha(File.ReadAllBytes(path)); return new(id, path, expected, actual, actual == expected);
}
static string Sha(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

public sealed record Binding(string Id, string Path, string ExpectedSha256, string ActualSha256, bool HashMatches);
public sealed record SeedTable(string Id, int[] Seeds, double[] InitialScales);
public sealed record ChainResult(double[] Samples, double[] Second, double[] XGradient, int Accepted, int Total,
    int NonFinite, int Divergences, double[] DeltaH, double StepSize);
public sealed record SamplerTableResult(string Id, double AcceptanceRate, int NonFiniteCount, int DivergenceCount,
    double ExpMinusDeltaHMean, double ExpMinusDeltaHError, double ExpMinusDeltaHSe, double Rhat, double Ess,
    double Mean, double MeanSe, double SecondMoment, double SecondMomentSe, double XGradientMean, double XGradientSe,
    double[] StepSizes);
public readonly record struct QuadratureResult(double Z, double M1, double M2, double M3, double M4);
