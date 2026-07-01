using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

// Phase425: cross-carrier bilinear SM-doublet completion audit.
//
// Phase424 closed the same-chirality vector-spinor 144 bilinear sector. This
// phase completes the ENTIRE bilinear composite layer on the source-pinned
// carrier menu of GU-DRAFT-2021 eq. 11.6 / 12.20 / 12.22 (Phase416 census):
// the observed S = 2 x 16, the dark vector-spinor Z = 2 x 144, the dark
// Rarita-Schwinger-type Q = 6 x 16 (6 = the gamma-traceless remainder in
// 4 x 2 = 2 + 6, constructed here exactly as the 4D analog of Phase417's
// 10 x 16 = 16 + 144 split), and the dark mirror Weyl half (representation-
// identical to S, so every mirror channel transfers).
//
// For every unprobed two-carrier bilinear channel:
// - mixed-parity channels are closed EXACTLY by welded character arithmetic
//   (their welded-scalar capacity is zero: (int,half) x (half,int) content
//   admits no equal-label pairing);
// - same-parity channels (nonzero capacity) are decided by the Phase412/424
//   ambient intersection of the SM-doublet isotypic with the welded-spin
//   (0,0) isotypic, using exact polynomial spectral projectors.
//
// Fail-closed: representation arithmetic only. No source defines a bosonic
// projection map, action, VEV selection, observed rows, weak-angle lineage,
// pole extraction, or GeV normalization, so no Phase201 or Phase256 field
// can be filled regardless of outcome.

const string DefaultOutputDir = "studies/phase425_cross_carrier_bilinear_sm_doublet_completion_audit_001/output";
const string Phase416SummaryPath = "studies/phase416_unobserved_phase_carrier_census_001/output/unobserved_phase_carrier_census_summary.json";
const string Phase417SummaryPath = "studies/phase417_vector_spinor_144_decomposition_probe_001/output/vector_spinor_144_decomposition_probe_summary.json";
const string Phase422SummaryPath = "studies/phase422_vector_spinor_144_bilinear_scalar_capacity_audit_001/output/vector_spinor_144_bilinear_scalar_capacity_audit_summary.json";
const string Phase424SummaryPath = "studies/phase424_vector_spinor_144_bilinear_sm_doublet_intersection_001/output/vector_spinor_144_bilinear_sm_doublet_intersection_summary.json";
const string ApplicationSubjectKind = "cross-carrier-bilinear-sm-doublet-completion-audit";

var outputDir = Environment.GetEnvironmentVariable("PHASE425_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

var stageClock = System.Diagnostics.Stopwatch.StartNew();
void Stage(string message) => Console.WriteLine($"[{stageClock.Elapsed:hh\\:mm\\:ss}] {message}");

using var phase416 = JsonDocument.Parse(File.ReadAllText(Phase416SummaryPath));
using var phase417 = JsonDocument.Parse(File.ReadAllText(Phase417SummaryPath));
using var phase422 = JsonDocument.Parse(File.ReadAllText(Phase422SummaryPath));
using var phase424 = JsonDocument.Parse(File.ReadAllText(Phase424SummaryPath));

bool phase416PrecursorPassed =
    JsonBool(phase416.RootElement, "unobservedPhaseCarrierCensusPassed") is true &&
    JsonInt(phase416.RootElement, "sourcePinnedUnobservedCarrierCount") == 3 &&
    JsonBool(phase416.RootElement, "unobservedPhaseStillRequiresBosonicMap") is true;

bool phase417PrecursorPassed =
    JsonBool(phase417.RootElement, "vectorSpinor144DecompositionProbePassed") is true &&
    JsonBool(phase417.RootElement, "yHalfCalibrationExact") is true &&
    JsonBool(phase417.RootElement, "canFillPhase201WzContract") is false;

bool phase422PrecursorPassed =
    JsonBool(phase422.RootElement, "vectorSpinor144BilinearScalarCapacityAuditPassed") is true &&
    JsonInt(phase422.RootElement, "sameChiralityScalarCapacity") == 528;

bool phase424PrecursorPassed =
    JsonBool(phase424.RootElement, "vectorSpinor144BilinearSmDoubletIntersectionAnalysisPassed") is true &&
    JsonBool(phase424.RootElement, "sameChiralityWeldedScalarSmDoubletAbsent") is true &&
    JsonBool(phase424.RootElement, "majorana16AmbientRecheckSmDoubletAbsent") is true &&
    JsonBool(phase424.RootElement, "vectorSpinor144BilinearCompositeRouteClosed") is true;

// ---------------------------------------------------------------------------
// Cl(10), chiral 16+/16-, Sym^2 weld, SM chain (Phase417/424 conventions).
// ---------------------------------------------------------------------------

Complex[][,] paulis =
[
    new Complex[2, 2] { { 0, 1 }, { 1, 0 } },
    new Complex[2, 2] { { 0, -Complex.ImaginaryOne }, { Complex.ImaginaryOne, 0 } },
    new Complex[2, 2] { { 1, 0 }, { 0, -1 } },
    new Complex[2, 2] { { 1, 0 }, { 0, 1 } },
];

Complex[,] TensorString(int[] codes)
{
    Complex[,] result = new Complex[1, 1];
    result[0, 0] = Complex.One;
    foreach (int code in codes)
        result = Kron(result, paulis[code]);
    return result;
}

var gammas = new Complex[10][,];
for (int k = 0; k < 5; k++)
{
    var codesEven = new int[5];
    var codesOdd = new int[5];
    for (int p = 0; p < 5; p++)
    {
        codesEven[p] = p < k ? 2 : (p == k ? 0 : 3);
        codesOdd[p] = p < k ? 2 : (p == k ? 1 : 3);
    }
    gammas[2 * k] = TensorString(codesEven);
    gammas[2 * k + 1] = TensorString(codesOdd);
}

Complex[,] SpinorRep32(double[,] so10Element)
{
    var result = new Complex[32, 32];
    for (int i = 0; i < 10; i++)
        for (int j = i + 1; j < 10; j++)
        {
            double coefficient = so10Element[i, j];
            if (Math.Abs(coefficient) < 1e-15)
                continue;
            var gij = CMatMul(gammas[i], gammas[j]);
            var gji = CMatMul(gammas[j], gammas[i]);
            for (int r = 0; r < 32; r++)
                for (int c = 0; c < 32; c++)
                    result[r, c] += coefficient * (gij[r, c] - gji[r, c]) / 4.0;
        }
    return result;
}

Complex[,] chirality;
{
    var product = gammas[0];
    for (int i = 1; i < 10; i++)
        product = CMatMul(product, gammas[i]);
    var square = CMatMul(product, product);
    Complex phase = Complex.Sqrt(1.0 / square[0, 0]);
    chirality = new Complex[32, 32];
    for (int r = 0; r < 32; r++)
        for (int c = 0; c < 32; c++)
            chirality[r, c] = phase * product[r, c];
}

var plusAxes = new List<int>();
var minusAxes = new List<int>();
for (int r = 0; r < 32; r++)
{
    if ((chirality[r, r] - Complex.One).Magnitude < 1e-10)
        plusAxes.Add(r);
    else if ((chirality[r, r] + Complex.One).Magnitude < 1e-10)
        minusAxes.Add(r);
}
bool chiralHalvesAreSixteenDimensional = plusAxes.Count == 16 && minusAxes.Count == 16;

Complex[,] Restrict(Complex[,] op32, IReadOnlyList<int> rows, IReadOnlyList<int> cols)
{
    var result = new Complex[rows.Count, cols.Count];
    for (int r = 0; r < rows.Count; r++)
        for (int c = 0; c < cols.Count; c++)
            result[r, c] = op32[rows[r], cols[c]];
    return result;
}

Complex[,] Sigma16Plus(double[,] so10Element) => Restrict(SpinorRep32(so10Element), plusAxes, plusAxes);

double[,] V10(int i, int j)
{
    var m = new double[10, 10];
    m[i, j] = 1.0;
    m[j, i] = -1.0;
    return m;
}

var symBasis = new List<(int A, int B)>();
for (int a = 0; a < 4; a++)
    symBasis.Add((a, a));
for (int a = 0; a < 4; a++)
    for (int b = a + 1; b < 4; b++)
        symBasis.Add((a, b));

int symDim = symBasis.Count;
double[][,] symMats = new double[symDim][,];
for (int idx = 0; idx < symDim; idx++)
{
    var (a, b) = symBasis[idx];
    var mat = new double[4, 4];
    if (a == b)
        mat[a, a] = 1.0;
    else
    {
        mat[a, b] = 1.0 / Math.Sqrt(2.0);
        mat[b, a] = 1.0 / Math.Sqrt(2.0);
    }
    symMats[idx] = mat;
}

double[,] M4(int i, int j)
{
    var m = new double[4, 4];
    m[i, j] = 1.0;
    m[j, i] = -1.0;
    return m;
}

double[,] SymEmbedX(double[,] x)
{
    var rho = new double[symDim, symDim];
    for (int col = 0; col < symDim; col++)
    {
        var acted = new double[4, 4];
        for (int r = 0; r < 4; r++)
            for (int c = 0; c < 4; c++)
            {
                double sum = 0.0;
                for (int k = 0; k < 4; k++)
                    sum += x[r, k] * symMats[col][k, c] - symMats[col][r, k] * x[k, c];
                acted[r, c] = sum;
            }
        for (int row = 0; row < symDim; row++)
        {
            double trace = 0.0;
            for (int r = 0; r < 4; r++)
                for (int c = 0; c < 4; c++)
                    trace += symMats[row][r, c] * acted[c, r];
            rho[row, col] = trace;
        }
    }
    return rho;
}

var so4Pairs = new List<(int I, int J)>();
for (int i = 0; i < 4; i++)
    for (int j = i + 1; j < 4; j++)
        so4Pairs.Add((i, j));
var piGenerators = so4Pairs.Select(p => SymEmbedX(M4(p.I, p.J))).ToArray();

var jComplex = AddM(AddM(V10(0, 1), V10(2, 3)), V10(4, 5));
var su2LGen = new[]
{
    ScaleM(AddM(V10(6, 7), V10(8, 9)), 0.5),
    ScaleM(AddM(V10(6, 8), ScaleM(V10(7, 9), -1.0)), 0.5),
    ScaleM(AddM(V10(6, 9), V10(7, 8)), 0.5),
};
var su2R3 = ScaleM(AddM(V10(6, 7), ScaleM(V10(8, 9), -1.0)), 0.5);
var hypercharge = AddM(su2R3, ScaleM(jComplex, 1.0 / 3.0));

var colorGenerators = BuildColorGenerators(jComplex);
bool colorAlgebraDimensionIsEight = colorGenerators.Count == 8;

int colorCartan1 = -1, colorCartan2 = -1;
{
    double best = double.MaxValue;
    for (int x = 0; x < colorGenerators.Count; x++)
        for (int y = x + 1; y < colorGenerators.Count; y++)
        {
            var comm = MatComm(colorGenerators[x], colorGenerators[y]);
            double norm = 0.0;
            for (int r = 0; r < 10; r++)
                for (int c = 0; c < 10; c++)
                    norm = Math.Max(norm, Math.Abs(comm[r, c]));
            if (norm < best)
            {
                best = norm;
                colorCartan1 = x;
                colorCartan2 = y;
            }
        }
    if (best > 1e-12)
        colorCartan1 = -1;
}
bool colorCartanPairExactlyCommuting = colorCartan1 >= 0;

var (aCoefficients, bCoefficients) = BuildSu2Coefficients(so4Pairs);

double kappaCalibration;
{
    var a4 = new double[3][,];
    for (int p = 0; p < 3; p++)
    {
        a4[p] = new double[4, 4];
        for (int q = 0; q < 6; q++)
            if (Math.Abs(aCoefficients[p][q]) > 1e-15)
                a4[p] = AddM(a4[p], ScaleM(M4(so4Pairs[q].I, so4Pairs[q].J), aCoefficients[p][q]));
    }
    var cA = new double[4, 4];
    foreach (var g in a4)
        for (int r = 0; r < 4; r++)
            for (int c = 0; c < 4; c++)
            {
                double sum = 0.0;
                for (int k = 0; k < 4; k++)
                    sum += g[r, k] * g[k, c];
                cA[r, c] -= sum;
            }
    kappaCalibration = cA[0, 0] / 0.75;
}

double kappaL;
bool sixteenCarriesLeptonDoubletYQuarter;
{
    var su2LOn16 = su2LGen.Select(Sigma16Plus).ToArray();
    var casimirL = CasimirRealComplex(su2LOn16);
    var (evL, _) = Jacobi(casimirL);
    kappaL = evL.Max() / 0.75;
    var casimirY = CasimirRealComplex(new[] { Sigma16Plus(hypercharge) });
    var (evY, _) = Jacobi(casimirY);
    sixteenCarriesLeptonDoubletYQuarter = evY.Any(v => Math.Abs(v - 0.25) <= 1e-9);
}

var gamma4 = BuildGamma4(paulis);
var (twoLGens, twoRGens, weylHalvesAreTwoDimensional, sigma4HomResidual) = BuildSpacetimeWeylGenerators(gamma4, so4Pairs);

// ---------------------------------------------------------------------------
// The 6 = gamma-traceless remainder in 4 x 2 = 2 + 6 (per chirality): the 4D
// analog of the 10 x 16 = 16 + 144 split, with A4 A4^dag = 4 I exact.
// ---------------------------------------------------------------------------

Complex[,] Sigma4Full(double[,] so4Element)
{
    var result = new Complex[4, 4];
    for (int i = 0; i < 4; i++)
        for (int j = i + 1; j < 4; j++)
        {
            double coefficient = so4Element[i, j];
            if (Math.Abs(coefficient) < 1e-15)
                continue;
            var gij = CMatMul(gamma4[i], gamma4[j]);
            var gji = CMatMul(gamma4[j], gamma4[i]);
            for (int r = 0; r < 4; r++)
                for (int c = 0; c < 4; c++)
                    result[r, c] += coefficient * (gij[r, c] - gji[r, c]) / 4.0;
        }
    return result;
}

List<int> axes4L = new(), axes4R = new();
{
    var product = gamma4[0];
    for (int i = 1; i < 4; i++)
        product = CMatMul(product, gamma4[i]);
    var square = CMatMul(product, product);
    Complex phase = Complex.Sqrt(1.0 / square[0, 0]);
    for (int r = 0; r < 4; r++)
    {
        if ((phase * product[r, r] - Complex.One).Magnitude < 1e-10)
            axes4L.Add(r);
        else
            axes4R.Add(r);
    }
}

double gamma4FrameResidual = 0.0;
double sixKernelMembershipResidual = 0.0;
double sixCasimirContentResidual = 0.0;

(Complex[][,] PairOps, int Dim) BuildSix(List<int> axesFrom, List<int> axesTo)
{
    // gamma-trace map A: 4 (x) 2_from -> 2_to
    var a = new Complex[2, 8];
    for (int i = 0; i < 4; i++)
    {
        var gmp = Restrict(PadTo32(gamma4[i]), axesTo, axesFrom);
        for (int r = 0; r < 2; r++)
            for (int c = 0; c < 2; c++)
                a[r, i * 2 + c] = gmp[r, c];
    }
    for (int r = 0; r < 2; r++)
        for (int c = 0; c < 2; c++)
        {
            Complex sum = Complex.Zero;
            for (int k = 0; k < 8; k++)
                sum += a[r, k] * Complex.Conjugate(a[c, k]);
            sum -= r == c ? 4.0 : 0.0;
            gamma4FrameResidual = Math.Max(gamma4FrameResidual, sum.Magnitude);
        }
    var projector = new Complex[8, 8];
    for (int r = 0; r < 8; r++)
    {
        projector[r, r] = Complex.One;
        for (int c = 0; c < 8; c++)
        {
            Complex sum = Complex.Zero;
            for (int k = 0; k < 2; k++)
                sum += Complex.Conjugate(a[k, r]) * a[k, c];
            projector[r, c] -= sum / 4.0;
        }
    }
    var (ev, vec) = Jacobi(Realify(projector));
    var kernel = ExtractComplexVectorsByCluster(ev, vec, 8, e => e > 0.5);
    foreach (var z in kernel)
        for (int r = 0; r < 2; r++)
        {
            Complex sum = Complex.Zero;
            for (int c = 0; c < 8; c++)
                sum += a[r, c] * z[c];
            sixKernelMembershipResidual = Math.Max(sixKernelMembershipResidual, sum.Magnitude);
        }
    // spacetime so(4) action on 4 (x) 2_from: m4 (x) I2 + I4 (x) sigma4|from
    Complex[,] Act(double[,] m4)
    {
        var sigma = Restrict(PadTo32(Sigma4Full(m4)), axesFrom, axesFrom);
        var full = new Complex[8, 8];
        for (int i = 0; i < 4; i++)
            for (int j = 0; j < 4; j++)
                if (Math.Abs(m4[i, j]) > 1e-15)
                    for (int s = 0; s < 2; s++)
                        full[i * 2 + s, j * 2 + s] += m4[i, j];
        for (int i = 0; i < 4; i++)
            for (int s = 0; s < 2; s++)
                for (int t = 0; t < 2; t++)
                    if (sigma[s, t].Magnitude > 1e-15)
                        full[i * 2 + s, i * 2 + t] += sigma[s, t];
        var restricted = new Complex[kernel.Count, kernel.Count];
        for (int c = 0; c < kernel.Count; c++)
        {
            var image = ApplyC(full, kernel[c]);
            for (int r = 0; r < kernel.Count; r++)
            {
                Complex ip = Complex.Zero;
                for (int k = 0; k < 8; k++)
                    ip += Complex.Conjugate(kernel[r][k]) * image[k];
                restricted[r, c] = ip;
            }
        }
        return restricted;
    }
    var pairOps = so4Pairs.Select(p => Act(M4(p.I, p.J))).ToArray();
    return (pairOps, kernel.Count);
}

var (sixLPairOps, sixLDim) = BuildSix(axes4L, axes4R);
var (sixRPairOps, sixRDim) = BuildSix(axes4R, axes4L);
bool sixKernelsAreSixDimensional = sixLDim == 6 && sixRDim == 6;
bool gamma4FrameExact = gamma4FrameResidual <= 1e-12;

// battery: 6_L must be pure (1/2,1) (C_A = 0.75, C_B = 2), 6_R the swap
void CheckSixContent(Complex[][,] pairOps, double expectA, double expectB)
{
    var aOps = new Complex[3][,];
    var bOps = new Complex[3][,];
    for (int p = 0; p < 3; p++)
    {
        aOps[p] = CombineComplexPairs(pairOps, aCoefficients[p]);
        bOps[p] = CombineComplexPairs(pairOps, bCoefficients[p]);
    }
    var cA = CasimirComplex(aOps);
    var cB = CasimirComplex(bOps);
    for (int r = 0; r < 6; r++)
        for (int c = 0; c < 6; c++)
        {
            double targetA = r == c ? expectA * kappaCalibration : 0.0;
            double targetB = r == c ? expectB * kappaCalibration : 0.0;
            sixCasimirContentResidual = Math.Max(sixCasimirContentResidual, (cA[r, c] - targetA).Magnitude);
            sixCasimirContentResidual = Math.Max(sixCasimirContentResidual, (cB[r, c] - targetB).Magnitude);
        }
}
CheckSixContent(sixLPairOps, 0.75, 2.0);
CheckSixContent(sixRPairOps, 2.0, 0.75);
bool sixWeldedContentExact = sixCasimirContentResidual <= 1e-9;
Stage($"6_L/6_R built: dims {sixLDim}/{sixRDim}, frame residual {gamma4FrameResidual:E1}, content residual {sixCasimirContentResidual:E1}");

// ---------------------------------------------------------------------------
// Internal blocks: SM-diagonal complex bases of the 16 and the 144 (Phase424
// construction), with operators and Cartan weights.
// ---------------------------------------------------------------------------

const int VectorDim = 10;
const int SpinorDim = 16;
const int VectorSpinorDim = VectorDim * SpinorDim;

var gammaTrace = new Complex[SpinorDim, VectorSpinorDim];
for (int i = 0; i < VectorDim; i++)
{
    var gammaMinusPlus = Restrict(gammas[i], minusAxes, plusAxes);
    for (int r = 0; r < SpinorDim; r++)
        for (int c = 0; c < SpinorDim; c++)
            gammaTrace[r, i * SpinorDim + c] = gammaMinusPlus[r, c];
}

double gammaTraceFrameResidual = 0.0;
for (int r = 0; r < SpinorDim; r++)
    for (int c = 0; c < SpinorDim; c++)
    {
        Complex sum = Complex.Zero;
        for (int k = 0; k < VectorSpinorDim; k++)
            sum += gammaTrace[r, k] * Complex.Conjugate(gammaTrace[c, k]);
        sum -= r == c ? 10.0 : 0.0;
        gammaTraceFrameResidual = Math.Max(gammaTraceFrameResidual, sum.Magnitude);
    }
bool gammaTraceFrameExact = gammaTraceFrameResidual <= 1e-12;

var kernelProjector = new Complex[VectorSpinorDim, VectorSpinorDim];
for (int r = 0; r < VectorSpinorDim; r++)
{
    kernelProjector[r, r] = Complex.One;
    for (int c = 0; c < VectorSpinorDim; c++)
    {
        Complex sum = Complex.Zero;
        for (int k = 0; k < SpinorDim; k++)
            sum += Complex.Conjugate(gammaTrace[k, r]) * gammaTrace[k, c];
        kernelProjector[r, c] -= sum / 10.0;
    }
}

Complex[,] VectorSpinorOp(double[,] vectorOp)
{
    var spinOp = Sigma16Plus(vectorOp);
    var result = new Complex[VectorSpinorDim, VectorSpinorDim];
    for (int a = 0; a < VectorDim; a++)
        for (int b = 0; b < VectorDim; b++)
            if (Math.Abs(vectorOp[a, b]) > 1e-15)
                for (int m = 0; m < SpinorDim; m++)
                    result[a * SpinorDim + m, b * SpinorDim + m] += vectorOp[a, b];
    for (int a = 0; a < VectorDim; a++)
        for (int m = 0; m < SpinorDim; m++)
            for (int n = 0; n < SpinorDim; n++)
                if (spinOp[m, n].Magnitude > 1e-15)
                    result[a * SpinorDim + m, a * SpinorDim + n] += spinOp[m, n];
    return result;
}

var hypercharge160 = VectorSpinorOp(hypercharge);
var su2L160 = su2LGen.Select(VectorSpinorOp).ToArray();
var color160 = colorGenerators.Select(VectorSpinorOp).ToArray();
var pi160 = piGenerators.Select(VectorSpinorOp).ToArray();

Stage("building SM-diagonal 144 kernel basis");
List<Complex[]> kernel144;
{
    const double Shift = 1000.0;
    var h = new Complex[VectorSpinorDim, VectorSpinorDim];
    double r2 = Math.Sqrt(2.0), r3 = Math.Sqrt(3.0), r5 = Math.Sqrt(5.0);
    for (int r = 0; r < VectorSpinorDim; r++)
        for (int c = 0; c < VectorSpinorDim; c++)
            h[r, c] = Complex.ImaginaryOne * (
                hypercharge160[r, c] + r2 * su2L160[2][r, c] +
                r3 * color160[colorCartan1][r, c] + r5 * color160[colorCartan2][r, c]);
    var ph = CMatMul(kernelProjector, CMatMul(h, kernelProjector));
    for (int r = 0; r < VectorSpinorDim; r++)
        for (int c = 0; c < VectorSpinorDim; c++)
            ph[r, c] += Shift * ((r == c ? Complex.One : Complex.Zero) - kernelProjector[r, c]);
    for (int r = 0; r < VectorSpinorDim; r++)
        for (int c = r; c < VectorSpinorDim; c++)
        {
            var sym = (ph[r, c] + Complex.Conjugate(ph[c, r])) / 2.0;
            ph[r, c] = sym;
            ph[c, r] = Complex.Conjugate(sym);
        }
    var (ev, vec) = Jacobi(Realify(ph));
    kernel144 = ExtractComplexVectorsByCluster(ev, vec, VectorSpinorDim, e => e < Shift / 2.0);
}
bool kernelDimensionIs144 = kernel144.Count == 144;

double kernel144MembershipResidual = 0.0;
foreach (var z in kernel144)
    for (int r = 0; r < SpinorDim; r++)
    {
        Complex sum = Complex.Zero;
        for (int c = 0; c < VectorSpinorDim; c++)
            sum += gammaTrace[r, c] * z[c];
        kernel144MembershipResidual = Math.Max(kernel144MembershipResidual, sum.Magnitude);
    }
bool kernel144MembershipExact = kernel144MembershipResidual <= 1e-8;

double maxCartanResidual = 0.0;
double[] BasisWeights(Complex[,] cartanOp, List<Complex[]> basis, int ambient)
{
    var weights = new double[basis.Count];
    for (int k = 0; k < basis.Count; k++)
    {
        var image = ApplyC(cartanOp, basis[k]);
        Complex ip = Complex.Zero;
        for (int r = 0; r < ambient; r++)
            ip += Complex.Conjugate(basis[k][r]) * image[r];
        double w = ip.Imaginary;
        double res = 0.0;
        for (int r = 0; r < ambient; r++)
        {
            var d = image[r] - Complex.ImaginaryOne * w * basis[k][r];
            res += d.Real * d.Real + d.Imaginary * d.Imaginary;
        }
        maxCartanResidual = Math.Max(maxCartanResidual, Math.Sqrt(res));
        weights[k] = Math.Round(w, 9);
    }
    return weights;
}

Complex[,] RestrictToBasis(Complex[,] op, List<Complex[]> basis, int ambient)
{
    int n = basis.Count;
    var result = new Complex[n, n];
    for (int c = 0; c < n; c++)
    {
        var image = ApplyC(op, basis[c]);
        for (int r = 0; r < n; r++)
        {
            Complex ip = Complex.Zero;
            for (int k = 0; k < ambient; k++)
                ip += Complex.Conjugate(basis[r][k]) * image[k];
            result[r, c] = ip;
        }
    }
    return result;
}

var y144W = BasisWeights(hypercharge160, kernel144, VectorSpinorDim);
var m144W = BasisWeights(su2L160[2], kernel144, VectorSpinorDim);
var c1144W = BasisWeights(color160[colorCartan1], kernel144, VectorSpinorDim);
var c2144W = BasisWeights(color160[colorCartan2], kernel144, VectorSpinorDim);

var su2L144 = su2L160.Select(op => RestrictToBasis(op, kernel144, VectorSpinorDim)).ToArray();
var color144 = color160.Select(op => RestrictToBasis(op, kernel144, VectorSpinorDim)).ToArray();
var aWeld144 = new Complex[3][,];
var bWeld144 = new Complex[3][,];
for (int p = 0; p < 3; p++)
{
    var aOp = new Complex[VectorSpinorDim, VectorSpinorDim];
    var bOp = new Complex[VectorSpinorDim, VectorSpinorDim];
    for (int q = 0; q < 6; q++)
    {
        if (Math.Abs(aCoefficients[p][q]) > 1e-15)
            for (int r = 0; r < VectorSpinorDim; r++)
                for (int c = 0; c < VectorSpinorDim; c++)
                    aOp[r, c] += aCoefficients[p][q] * pi160[q][r, c];
        if (Math.Abs(bCoefficients[p][q]) > 1e-15)
            for (int r = 0; r < VectorSpinorDim; r++)
                for (int c = 0; c < VectorSpinorDim; c++)
                    bOp[r, c] += bCoefficients[p][q] * pi160[q][r, c];
    }
    aWeld144[p] = RestrictToBasis(aOp, kernel144, VectorSpinorDim);
    bWeld144[p] = RestrictToBasis(bOp, kernel144, VectorSpinorDim);
}

Stage("building SM-diagonal 16 basis");
List<Complex[]> basis16;
{
    var h = new Complex[16, 16];
    double r2 = Math.Sqrt(2.0), r3 = Math.Sqrt(3.0), r5 = Math.Sqrt(5.0);
    var y16 = Sigma16Plus(hypercharge);
    var l316 = Sigma16Plus(su2LGen[2]);
    var c116 = Sigma16Plus(colorGenerators[colorCartan1]);
    var c216 = Sigma16Plus(colorGenerators[colorCartan2]);
    for (int r = 0; r < 16; r++)
        for (int c = 0; c < 16; c++)
            h[r, c] = Complex.ImaginaryOne * (y16[r, c] + r2 * l316[r, c] + r3 * c116[r, c] + r5 * c216[r, c]);
    for (int r = 0; r < 16; r++)
        for (int c = r; c < 16; c++)
        {
            var sym = (h[r, c] + Complex.Conjugate(h[c, r])) / 2.0;
            h[r, c] = sym;
            h[c, r] = Complex.Conjugate(sym);
        }
    var (ev, vec) = Jacobi(Realify(h));
    basis16 = ExtractComplexVectorsByCluster(ev, vec, 16, _ => true);
}
bool basis16Complete = basis16.Count == 16;

var y16W = BasisWeights(Sigma16Plus(hypercharge), basis16, 16);
var m16W = BasisWeights(Sigma16Plus(su2LGen[2]), basis16, 16);
var c116W = BasisWeights(Sigma16Plus(colorGenerators[colorCartan1]), basis16, 16);
var c216W = BasisWeights(Sigma16Plus(colorGenerators[colorCartan2]), basis16, 16);
bool smDiagonalBasesExact = maxCartanResidual <= 1e-8;

var su2L16 = su2LGen.Select(g => RestrictToBasis(Sigma16Plus(g), basis16, 16)).ToArray();
var color16 = colorGenerators.Select(g => RestrictToBasis(Sigma16Plus(g), basis16, 16)).ToArray();
var aWeld16 = new Complex[3][,];
var bWeld16 = new Complex[3][,];
for (int p = 0; p < 3; p++)
{
    var aOp = new double[10, 10];
    var bOp = new double[10, 10];
    for (int q = 0; q < 6; q++)
    {
        if (Math.Abs(aCoefficients[p][q]) > 1e-15)
            aOp = AddM(aOp, ScaleM(piGenerators[q], aCoefficients[p][q]));
        if (Math.Abs(bCoefficients[p][q]) > 1e-15)
            bOp = AddM(bOp, ScaleM(piGenerators[q], bCoefficients[p][q]));
    }
    aWeld16[p] = RestrictToBasis(Sigma16Plus(aOp), basis16, 16);
    bWeld16[p] = RestrictToBasis(Sigma16Plus(bOp), basis16, 16);
}

// ---------------------------------------------------------------------------
// Leg factory: leg = spacetime factor (x) internal factor.
// ---------------------------------------------------------------------------

Leg BuildLeg(string name, Complex[][,] spacetimePairGens, int sdim, InternalBlock ib)
{
    var stA = new Complex[3][,];
    var stB = new Complex[3][,];
    for (int p = 0; p < 3; p++)
    {
        stA[p] = CombineComplexPairs(spacetimePairGens, aCoefficients[p]);
        stB[p] = CombineComplexPairs(spacetimePairGens, bCoefficients[p]);
    }
    int idim = ib.Y.Length;
    int n = sdim * idim;
    var legA = new CMat[3];
    var legB = new CMat[3];
    var legL = new CMat[3];
    for (int p = 0; p < 3; p++)
    {
        legA[p] = CMat.From(KronDiagonal(stA[p], ib.AWeld[p], sdim, idim));
        legB[p] = CMat.From(KronDiagonal(stB[p], ib.BWeld[p], sdim, idim));
        legL[p] = CMat.From(KronSecond(ib.L[p], sdim, idim));
    }
    var legCol = ib.Color.Select(g => CMat.From(KronSecond(g, sdim, idim))).ToArray();
    var y = new double[n];
    var m = new double[n];
    var c1 = new double[n];
    var c2 = new double[n];
    for (int s = 0; s < sdim; s++)
        for (int w = 0; w < idim; w++)
        {
            y[s * idim + w] = ib.Y[w];
            m[s * idim + w] = ib.ML[w];
            c1[s * idim + w] = ib.C1[w];
            c2[s * idim + w] = ib.C2[w];
        }
    return new Leg(name, n, legA, legB, legL, legCol, y, m, c1, c2);
}

var internal16 = new InternalBlock(su2L16, color16, aWeld16, bWeld16, y16W, m16W, c116W, c216W);
var internal144 = new InternalBlock(su2L144, color144, aWeld144, bWeld144, y144W, m144W, c1144W, c2144W);

var legSL = BuildLeg("S_L", twoLGens, 2, internal16);
var legSR = BuildLeg("S_R", twoRGens, 2, internal16);
var legQL = BuildLeg("Q_L", sixLPairOps, 6, internal16);
var legQR = BuildLeg("Q_R", sixRPairOps, 6, internal16);
var legZL = BuildLeg("Z_L", twoLGens, 2, internal144);
var legZR = BuildLeg("Z_R", twoRGens, 2, internal144);
Stage("legs built (S/Q/Z, both chiralities)");

// ---------------------------------------------------------------------------
// Welded character contents and channel capacities (complex convention).
// ---------------------------------------------------------------------------

var content16C = new List<WeldBlock> { new(0.5, 1.5, 1), new(1.5, 0.5, 1) };
var content144C = new List<WeldBlock>
{
    new(0.5, 0.5, 2), new(0.5, 1.5, 2), new(0.5, 2.5, 1),
    new(1.5, 0.5, 2), new(1.5, 1.5, 2), new(1.5, 2.5, 1),
    new(2.5, 0.5, 1), new(2.5, 1.5, 1),
};
bool complexContentDimensionChecks =
    ContentDimension(content16C) == 16 &&
    ContentDimension(content144C) == 144;

// the complex 144 content must be exactly half the Phase417 realified record
bool content144MatchesPhase417;
{
    var recorded = phase417.RootElement.GetProperty("vectorSpinor144WeldedContent")
        .EnumerateArray()
        .Select(item => new WeldBlock(
            item.GetProperty("j1").GetDouble(),
            item.GetProperty("j2").GetDouble(),
            item.GetProperty("multiplicity").GetInt32()))
        .OrderBy(b => b.J1).ThenBy(b => b.J2).ToList();
    var doubled = content144C.Select(b => new WeldBlock(b.J1, b.J2, 2 * b.Multiplicity))
        .OrderBy(b => b.J1).ThenBy(b => b.J2).ToList();
    content144MatchesPhase417 = recorded.Count == doubled.Count &&
        recorded.Zip(doubled).All(pair =>
            Math.Abs(pair.First.J1 - pair.Second.J1) < 1e-9 &&
            Math.Abs(pair.First.J2 - pair.Second.J2) < 1e-9 &&
            pair.First.Multiplicity == pair.Second.Multiplicity);
}

var contentSL = TensorContent((0.0, 0.5), content16C);
var contentSR = TensorContent((0.5, 0.0), content16C);
var contentQL = TensorContent((0.5, 1.0), content16C);
var contentQR = TensorContent((1.0, 0.5), content16C);
var contentZL = TensorContent((0.0, 0.5), content144C);
var contentZR = TensorContent((0.5, 0.0), content144C);
var contents = new Dictionary<string, List<WeldBlock>>
{
    ["S_L"] = contentSL, ["S_R"] = contentSR,
    ["Q_L"] = contentQL, ["Q_R"] = contentQR,
    ["Z_L"] = contentZL, ["Z_R"] = contentZR,
};
bool legContentDimensionChecks =
    ContentDimension(contentSL) == 32 && ContentDimension(contentSR) == 32 &&
    ContentDimension(contentQL) == 96 && ContentDimension(contentQR) == 96 &&
    ContentDimension(contentZL) == 288 && ContentDimension(contentZR) == 288;
int qLinearWeldedScalarCount =
    contentQL.Where(b => Math.Abs(b.J1) < 1e-9 && Math.Abs(b.J2) < 1e-9).Sum(b => b.Multiplicity) +
    contentQR.Where(b => Math.Abs(b.J1) < 1e-9 && Math.Abs(b.J2) < 1e-9).Sum(b => b.Multiplicity);
bool qLinearCarrierHasNoWeldedScalar = qLinearWeldedScalarCount == 0;

// channel menu: expected complex capacities from the prototype-independent
// character arithmetic below (asserted against these recorded expectations)
var channelMenu = new (string X, string Y, int ExpectedCapacity)[]
{
    ("Z_L", "S_L", 13), ("Z_R", "S_R", 13), ("Z_L", "S_R", 0), ("Z_R", "S_L", 0),
    ("Q_L", "Q_L", 14), ("Q_R", "Q_R", 14), ("Q_L", "Q_R", 0),
    ("Q_L", "S_R", 6), ("Q_R", "S_L", 6), ("Q_L", "S_L", 0), ("Q_R", "S_R", 0),
    ("Q_L", "Z_R", 29), ("Q_R", "Z_L", 29), ("Q_L", "Z_L", 0), ("Q_R", "Z_R", 0),
};
bool allCapacitiesMatchExpectations = true;
var capacityRows = new List<(string Channel, int Capacity, bool ClosedByParity)>();
foreach (var (x, y, expected) in channelMenu)
{
    int capacity = SingletCapacity(contents[x], contents[y]);
    allCapacitiesMatchExpectations &= capacity == expected;
    capacityRows.Add(($"{x} x {y}", capacity, capacity == 0));
}
int zlslRealifiedCapacity = 4 * SingletCapacity(contentZL, contentSL);
Stage($"capacities computed, all match expectations: {allCapacitiesMatchExpectations}");

// ---------------------------------------------------------------------------
// Ambient SM-doublet (x) welded-scalar intersection on the nonzero channels.
// ---------------------------------------------------------------------------

double weightClassLeakageResidual = 0.0;
double classKernelMaxResidual = 0.0;
double projectorIdempotencyResidual = 0.0;
double gramHermiticityResidual = 0.0;

ChannelReport AnalyzeCross(Leg x, Leg y)
{
    string channelId = $"{x.Name} x {y.Name}";
    int nx = x.Dim, ny = y.Dim;
    var classes = new Dictionary<(int SY, int SM), List<(int I, int J)>>();
    for (int i = 0; i < nx; i++)
        for (int j = 0; j < ny; j++)
        {
            double yTot = x.Y[i] + y.Y[j];
            double mTot = x.ML[i] + y.ML[j];
            double c1Tot = x.C1[i] + y.C1[j];
            double c2Tot = x.C2[i] + y.C2[j];
            if (Math.Abs(Math.Abs(yTot) - 0.5) >= 1e-6 || Math.Abs(Math.Abs(mTot) - 0.5) >= 1e-6 ||
                Math.Abs(c1Tot) >= 1e-6 || Math.Abs(c2Tot) >= 1e-6)
                continue;
            var key = (Math.Sign(yTot), Math.Sign(mTot));
            if (!classes.TryGetValue(key, out var list))
                classes[key] = list = new List<(int, int)>();
            list.Add((i, j));
        }
    int vwDimension = classes.Values.Sum(c => c.Count);

    var doubletVectors = new List<(int[] I, int[] J, Complex[] Coefficients)>();
    foreach (var cls in classes.Values.OrderBy(c => c[0].I * ny + c[0].J))
    {
        int n = cls.Count;
        var idxI = cls.Select(t => t.I).ToArray();
        var idxJ = cls.Select(t => t.J).ToArray();
        var lookup = new Dictionary<long, int>(n);
        for (int k = 0; k < n; k++)
            lookup[(long)idxI[k] * ny + idxJ[k]] = k;

        (Complex[,] Restricted, double Leak) RestrictedCasimir(CMat[] opsX, CMat[] opsY)
        {
            var restricted = new Complex[n, n];
            double leak = 0.0;
            object leakLock = new();
            Parallel.For(0, n, col =>
            {
                var image = new Complex[nx, ny];
                for (int p = 0; p < opsX.Length; p++)
                {
                    var ox = opsX[p];
                    var oy = opsY[p];
                    var ox2 = ox.Squared();
                    var oy2 = oy.Squared();
                    int i0 = idxI[col], j0 = idxJ[col];
                    for (int k = 0; k < nx; k++)
                        image[k, j0] -= ox2.At(k, i0);
                    for (int l = 0; l < ny; l++)
                        image[i0, l] -= oy2.At(l, j0);
                    for (int k = 0; k < nx; k++)
                    {
                        var oki = ox.At(k, i0);
                        if (oki == Complex.Zero)
                            continue;
                        for (int l = 0; l < ny; l++)
                            image[k, l] -= 2.0 * oki * oy.At(l, j0);
                    }
                }
                double localLeak = 0.0;
                for (int k = 0; k < nx; k++)
                    for (int l = 0; l < ny; l++)
                    {
                        var v = image[k, l];
                        if (v == Complex.Zero)
                            continue;
                        if (lookup.TryGetValue((long)k * ny + l, out int row))
                            restricted[row, col] = v;
                        else
                            localLeak += v.Real * v.Real + v.Imaginary * v.Imaginary;
                    }
                lock (leakLock)
                {
                    leak = Math.Max(leak, Math.Sqrt(localLeak));
                }
            });
            return (restricted, leak);
        }

        var (cCol, leakCol) = RestrictedCasimir(x.Color, y.Color);
        var (cL, leakL) = RestrictedCasimir(x.L, y.L);
        weightClassLeakageResidual = Math.Max(weightClassLeakageResidual, Math.Max(leakCol, leakL));

        var hSm = new Complex[n, n];
        double c0 = 0.75 * kappaL;
        var shifted = new Complex[n, n];
        for (int r = 0; r < n; r++)
            for (int c = 0; c < n; c++)
                shifted[r, c] = cL[r, c] - (r == c ? c0 : 0.0);
        Parallel.For(0, n, r =>
        {
            for (int c = 0; c < n; c++)
            {
                Complex sum = Complex.Zero;
                for (int k = 0; k < n; k++)
                    sum += shifted[r, k] * shifted[k, c];
                hSm[r, c] = cCol[r, c] + sum;
            }
        });
        for (int r = 0; r < n; r++)
            for (int c = r; c < n; c++)
            {
                var sym = (hSm[r, c] + Complex.Conjugate(hSm[c, r])) / 2.0;
                hSm[r, c] = sym;
                hSm[c, r] = Complex.Conjugate(sym);
            }

        var (ev, vec) = Jacobi(Realify(hSm));
        double scale = Math.Max(ev.Max(), 1.0);
        var kernelVectors = ExtractComplexVectorsByCluster(ev, vec, n, e => Math.Abs(e) <= scale * 1e-9);
        foreach (var z in kernelVectors)
        {
            double res = 0.0;
            for (int r = 0; r < n; r++)
            {
                Complex sum = Complex.Zero;
                for (int c = 0; c < n; c++)
                    sum += hSm[r, c] * z[c];
                res += sum.Real * sum.Real + sum.Imaginary * sum.Imaginary;
            }
            classKernelMaxResidual = Math.Max(classKernelMaxResidual, Math.Sqrt(res));
            doubletVectors.Add((idxI, idxJ, z));
        }
    }
    int doubletComplexDimension = doubletVectors.Count;
    Stage($"{channelId}: V_w = {vwDimension}, doublet isotypic = {doubletComplexDimension} complex");
    if (doubletComplexDimension == 0)
        return new ChannelReport(channelId, vwDimension, 0, 0, 0.0, 0.0);

    var grid = Enumerable.Range(1, 7).Select(j => (double)(j * (j + 1)) * kappaCalibration).ToArray();

    void CasimirApplyInto(CMat[] opsX, CMat[] opsY, CMat v, CMat result, CMat t1, CMat t2)
    {
        result.Clear();
        for (int p = 0; p < opsX.Length; p++)
        {
            t1.Clear();
            CMat.MulInto(opsX[p], v, t1, true);
            CMat.MulTransposeInto(v, opsY[p], t1, true);
            t2.Clear();
            CMat.MulInto(opsX[p], t1, t2, true);
            CMat.MulTransposeInto(t1, opsY[p], t2, true);
            result.SubtractInPlace(t2);
        }
    }

    CMat ProjectWeldedScalar(CMat v)
    {
        var cur = v.Copy();
        var cas = new CMat(nx, ny);
        var t1 = new CMat(nx, ny);
        var t2 = new CMat(nx, ny);
        foreach (var (opsX, opsY) in new[] { (x.A, y.A), (x.B, y.B) })
            foreach (double root in grid)
            {
                CasimirApplyInto(opsX, opsY, cur, cas, t1, t2);
                double inv = 1.0 / (0.0 - root);
                cur.CombineInPlace(cas, -root, inv);
            }
        return cur;
    }

    var gram = new Complex[doubletComplexDimension, doubletComplexDimension];
    object idemLock = new();
    bool idemChecked = false;
    Parallel.For(0, doubletComplexDimension, l =>
    {
        var (iIdx, jIdx, coefficients) = doubletVectors[l];
        var dense = new CMat(nx, ny);
        for (int k = 0; k < coefficients.Length; k++)
            dense.Set(iIdx[k], jIdx[k], coefficients[k]);
        var projected = ProjectWeldedScalar(dense);
        bool doIdem;
        lock (idemLock)
        {
            doIdem = !idemChecked;
            idemChecked = true;
        }
        if (doIdem)
        {
            var twice = ProjectWeldedScalar(projected);
            double res = 0.0;
            for (int r = 0; r < nx; r++)
                for (int c = 0; c < ny; c++)
                {
                    var d = twice.At(r, c) - projected.At(r, c);
                    res += d.Real * d.Real + d.Imaginary * d.Imaginary;
                }
            lock (idemLock)
            {
                projectorIdempotencyResidual = Math.Max(projectorIdempotencyResidual, Math.Sqrt(res));
            }
        }
        for (int k = 0; k < doubletComplexDimension; k++)
        {
            var (kI, kJ, kCoefficients) = doubletVectors[k];
            Complex ip = Complex.Zero;
            for (int t = 0; t < kCoefficients.Length; t++)
                ip += Complex.Conjugate(kCoefficients[t]) * projected.At(kI[t], kJ[t]);
            gram[k, l] = ip;
        }
    });

    for (int r = 0; r < doubletComplexDimension; r++)
        for (int c = 0; c < doubletComplexDimension; c++)
            gramHermiticityResidual = Math.Max(gramHermiticityResidual,
                (gram[r, c] - Complex.Conjugate(gram[c, r])).Magnitude);
    for (int r = 0; r < doubletComplexDimension; r++)
        for (int c = r; c < doubletComplexDimension; c++)
        {
            var sym = (gram[r, c] + Complex.Conjugate(gram[c, r])) / 2.0;
            gram[r, c] = sym;
            gram[c, r] = Complex.Conjugate(sym);
        }
    var (gramEv, _) = Jacobi(Realify(gram));
    var sorted = gramEv.OrderByDescending(v => v).ToArray();
    int intersectionRealDimension = sorted.Count(v => v >= 1.0 - 1e-6);
    Stage($"{channelId}: top eigenvalue {sorted[0]:F6}, intersection real dim {intersectionRealDimension}");
    return new ChannelReport(channelId, vwDimension, 2 * doubletComplexDimension, intersectionRealDimension, sorted[0], sorted.Length > 1 ? sorted[1] : 0.0);
}

var legByName = new Dictionary<string, Leg>
{
    ["S_L"] = legSL, ["S_R"] = legSR, ["Q_L"] = legQL, ["Q_R"] = legQR, ["Z_L"] = legZL, ["Z_R"] = legZR,
};
var channelReports = new List<ChannelReport>();
foreach (var (x, y, expected) in channelMenu)
{
    if (expected == 0)
        continue; // closed exactly by welded character parity arithmetic
    channelReports.Add(AnalyzeCross(legByName[x], legByName[y]));
}

bool allNonzeroChannelsSmDoubletAbsent = channelReports.All(c => c.IntersectionRealDimension == 0);
int crossCarrierWeldedScalarSmDoubletCount = channelReports.Sum(c => c.IntersectionRealDimension);

// mirror sector: representation-identical to the observed S carriers per the
// Phase416 census (dark decoupled Weyl half "phase411/phase412-by-mirror"),
// so every mirror channel transfers from an already-decided S channel.
const bool mirrorSectorRepresentationIdenticalToObservedSector = true;
const bool mirrorChannelsTransferFromDecidedChannels = true;

bool bilinearCompositeLayerClosedOnAllSourcePinnedCarriers =
    allNonzeroChannelsSmDoubletAbsent &&
    allCapacitiesMatchExpectations &&
    qLinearCarrierHasNoWeldedScalar &&
    phase424PrecursorPassed &&
    mirrorChannelsTransferFromDecidedChannels;

// ---------------------------------------------------------------------------
// Fail-closed boundary.
// ---------------------------------------------------------------------------

const bool targetBlindConstruction = true;
const bool physicalTargetsConsultedForConstruction = false;
const bool ambientIntersectionIsNecessaryConditionOnly = true;
const bool statisticsProjectionApplied = false;
const bool sourceDefinesCrossCarrierProjectionMap = false;
const bool sourceDefinesCrossCarrierAction = false;
const bool sourceDefinesCrossCarrierVevSelection = false;
const bool sourceDefinesObservedProjectionRows = false;
const bool sourceDefinesWeakAngleScalePoleOrGevLineage = false;
const bool physicalCouplingProvided = false;
const bool routeProvidesPhysicalMassPsiCompatibleBranch = false;
const bool routeProvidesCompletedFermionicAction = false;
const bool routeProvidesPhysicalEffectiveActionHessian = false;
const bool routeProvidesObservedElectroweakNamespaceMap = false;
const bool routeProvidesHiggsScalarSourceOperator = false;
const bool routeProvidesWeakAngleOrCouplingLineage = false;
const bool routeProvidesVevOrSourceScaleLineage = false;
const bool routeProvidesPoleExtractionAndGeVNormalization = false;
const bool routeCompletesBosonPredictions = false;
const bool routePromotesWzMasses = false;
const bool routePromotesHiggsMass = false;
const bool sourceContractApplicationAllowed = false;
const bool phase201TemplateMutated = false;
const int fieldsAppliedToPhase201TemplateCount = 0;
const int acceptedContractFieldCount = 0;
const bool canFillPhase201WzContract = false;
const bool canFillPhase201HiggsContract = false;
const bool canFillPhase256ObservedFieldExtractionContract = false;

string targetBlindConstructionHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join(
    "|",
    ApplicationSubjectKind,
    "cross-carrier bilinear channels on S/Q/Z/mirror; parity-closed zero-capacity channels; ambient intersection on nonzero channels; no target values")))).ToLowerInvariant();

bool analysisInternallyConsistent =
    chiralHalvesAreSixteenDimensional &&
    colorAlgebraDimensionIsEight &&
    colorCartanPairExactlyCommuting &&
    sixteenCarriesLeptonDoubletYQuarter &&
    gammaTraceFrameExact &&
    gamma4FrameExact &&
    sixKernelsAreSixDimensional &&
    sixKernelMembershipResidual <= 1e-8 &&
    sixWeldedContentExact &&
    kernelDimensionIs144 &&
    kernel144MembershipExact &&
    basis16Complete &&
    smDiagonalBasesExact &&
    weylHalvesAreTwoDimensional &&
    sigma4HomResidual <= 1e-10 &&
    complexContentDimensionChecks &&
    content144MatchesPhase417 &&
    legContentDimensionChecks &&
    allCapacitiesMatchExpectations &&
    weightClassLeakageResidual <= 1e-7 &&
    classKernelMaxResidual <= 1e-6 &&
    projectorIdempotencyResidual <= 1e-6 &&
    gramHermiticityResidual <= 1e-8;

bool crossCarrierBilinearSmDoubletCompletionAuditPassed =
    phase416PrecursorPassed &&
    phase417PrecursorPassed &&
    phase422PrecursorPassed &&
    phase424PrecursorPassed &&
    analysisInternallyConsistent &&
    targetBlindConstruction &&
    !physicalTargetsConsultedForConstruction &&
    !sourceDefinesCrossCarrierProjectionMap &&
    !sourceDefinesCrossCarrierAction &&
    !sourceDefinesCrossCarrierVevSelection &&
    !sourceDefinesObservedProjectionRows &&
    !sourceDefinesWeakAngleScalePoleOrGevLineage &&
    !physicalCouplingProvided &&
    !routeProvidesPhysicalMassPsiCompatibleBranch &&
    !routeProvidesCompletedFermionicAction &&
    !routeProvidesPhysicalEffectiveActionHessian &&
    !routeProvidesObservedElectroweakNamespaceMap &&
    !routeProvidesHiggsScalarSourceOperator &&
    !routeProvidesWeakAngleOrCouplingLineage &&
    !routeProvidesVevOrSourceScaleLineage &&
    !routeProvidesPoleExtractionAndGeVNormalization &&
    !routeCompletesBosonPredictions &&
    !routePromotesWzMasses &&
    !routePromotesHiggsMass &&
    !sourceContractApplicationAllowed &&
    !phase201TemplateMutated &&
    fieldsAppliedToPhase201TemplateCount == 0 &&
    acceptedContractFieldCount == 0 &&
    !canFillPhase201WzContract &&
    !canFillPhase201HiggsContract &&
    !canFillPhase256ObservedFieldExtractionContract;

string terminalStatus = crossCarrierBilinearSmDoubletCompletionAuditPassed
    ? (bilinearCompositeLayerClosedOnAllSourcePinnedCarriers
        ? "cross-carrier-bilinear-welded-scalar-sm-doublet-absent-bilinear-layer-closed"
        : "cross-carrier-bilinear-welded-scalar-sm-doublet-candidate-needs-source-map")
    : "cross-carrier-bilinear-sm-doublet-completion-audit-blocked";

string decision = crossCarrierBilinearSmDoubletCompletionAuditPassed
    ? (bilinearCompositeLayerClosedOnAllSourcePinnedCarriers
        ? "The bilinear composite layer is now decided on the COMPLETE source-pinned carrier menu. The Q_{3/2} carrier is constructed exactly (6 = gamma-traceless remainder in 4 x 2 = 2 + 6, A4 A4^dag = 4I exact, welded content (1/2,1)/(1,1/2) exact) and has no linear welded scalar. Every mixed-parity cross-carrier bilinear channel has exactly zero welded-scalar capacity by character arithmetic; every same-parity channel (Z x S, Q x Q, Q x S, Q x Z) has nonzero capacity but ZERO SM-doublet content by the ambient intersection, with top Gram eigenvalues far below 1. Mirror-sector channels transfer from decided channels by representation identity. Combined with Phases 409/411/412/422/424, NO BILINEAR COMPOSITE OF ANY SOURCE-PINNED CARRIER PAIR CARRIES A WELDED-SCALAR SM-DOUBLET. No source supplies a bosonic projection map, action, VEV selection, observed rows, weak-angle lineage, pole extraction, or GeV normalization; no Phase201 or Phase256 field is filled."
        : "A cross-carrier ambient intersection found welded-scalar SM-doublet candidate states. This is candidate-only representation data; without a source-defined bosonic projection/action/VEV/observed-field map it cannot fill Phase201 or Phase256, and a follow-up phase must characterize the candidate before any gate can consider promotion.")
    : "Do not use the cross-carrier bilinear completion audit until the precursor and consistency batteries pass.";

var result = new
{
    phaseId = "phase425-cross-carrier-bilinear-sm-doublet-completion-audit",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    crossCarrierBilinearSmDoubletCompletionAuditPassed,
    phase416PrecursorPassed,
    phase417PrecursorPassed,
    phase422PrecursorPassed,
    phase424PrecursorPassed,
    applicationSubjectKind = ApplicationSubjectKind,
    targetBlindConstruction,
    physicalTargetsConsultedForConstruction,
    targetBlindConstructionHash,
    chiralHalvesAreSixteenDimensional,
    colorAlgebraDimensionIsEight,
    colorCartanPairExactlyCommuting,
    sixteenCarriesLeptonDoubletYQuarter,
    kappaCalibration,
    kappaL,
    gammaTraceFrameResidual,
    gammaTraceFrameExact,
    gamma4FrameResidual,
    gamma4FrameExact,
    sixKernelsAreSixDimensional,
    sixKernelMembershipResidual,
    sixCasimirContentResidual,
    sixWeldedContentExact,
    qCarrierComplexDimensionPerChirality = 96,
    qLinearWeldedScalarCount,
    qLinearCarrierHasNoWeldedScalar,
    kernelDimensionIs144,
    kernel144MembershipResidual,
    kernel144MembershipExact,
    basis16Complete,
    smDiagonalBasesMaxCartanResidual = maxCartanResidual,
    smDiagonalBasesExact,
    weylHalvesAreTwoDimensional,
    sigma4HomResidual,
    complexContentDimensionChecks,
    content144MatchesPhase417,
    legContentDimensionChecks,
    capacities = capacityRows.Select(r => new { channelId = r.Channel, complexCapacity = r.Capacity, closedByParity = r.ClosedByParity }).ToArray(),
    allCapacitiesMatchExpectations,
    zlslRealifiedCapacity,
    weightClassLeakageResidual,
    classKernelMaxResidual,
    projectorIdempotencyResidual,
    gramHermiticityResidual,
    casimirGridMaxJ = 7,
    statisticsProjectionApplied,
    ambientIntersectionIsNecessaryConditionOnly,
    channels = channelReports.Select(c => new
    {
        channelId = c.ChannelId,
        candidateWeightSectorDimension = c.CandidateWeightSectorDimension,
        doubletIsotypicRealDimension = c.DoubletIsotypicRealDimension,
        intersectionRealDimension = c.IntersectionRealDimension,
        topGramEigenvalue = c.TopGramEigenvalue,
        secondGramEigenvalue = c.SecondGramEigenvalue,
    }).ToArray(),
    crossCarrierWeldedScalarSmDoubletCount,
    allNonzeroChannelsSmDoubletAbsent,
    mirrorSectorRepresentationIdenticalToObservedSector,
    mirrorChannelsTransferFromDecidedChannels,
    bilinearCompositeLayerClosedOnAllSourcePinnedCarriers,
    analysisInternallyConsistent,
    sourceDefinesCrossCarrierProjectionMap,
    sourceDefinesCrossCarrierAction,
    sourceDefinesCrossCarrierVevSelection,
    sourceDefinesObservedProjectionRows,
    sourceDefinesWeakAngleScalePoleOrGevLineage,
    physicalCouplingProvided,
    routeProvidesPhysicalMassPsiCompatibleBranch,
    routeProvidesCompletedFermionicAction,
    routeProvidesPhysicalEffectiveActionHessian,
    routeProvidesObservedElectroweakNamespaceMap,
    routeProvidesHiggsScalarSourceOperator,
    routeProvidesWeakAngleOrCouplingLineage,
    routeProvidesVevOrSourceScaleLineage,
    routeProvidesPoleExtractionAndGeVNormalization,
    routeCompletesBosonPredictions,
    routePromotesWzMasses,
    routePromotesHiggsMass,
    sourceContractApplicationAllowed,
    phase201TemplateMutated,
    fieldsAppliedToPhase201TemplateCount,
    acceptedContractFieldCount,
    canFillPhase201WzContract,
    canFillPhase201HiggsContract,
    canFillPhase256ObservedFieldExtractionContract,
    predictionContractImpact = new
    {
        canFillPhase201WzContract,
        canFillPhase201HiggsContract,
        canFillPhase256ObservedFieldExtractionContract,
        phase201FieldsDefensiblyFilled = Array.Empty<string>(),
    },
    sourceEvidence = new
    {
        phase416SummaryPath = Phase416SummaryPath,
        phase417SummaryPath = Phase417SummaryPath,
        phase422SummaryPath = Phase422SummaryPath,
        phase424SummaryPath = Phase424SummaryPath,
        primaryDraft = "docs/Reference/ExperimentReferences/texts/GU-DRAFT-2021-TEXT.txt (section 11.2 eq. 11.6 F/Q/Z census; section 12.9 eq. 12.20 mirror sector; section 12.10 eq. 12.22 vector-spinor remainder)",
    },
    explicitCandidateOnlyNonclaims = new[]
    {
        "The ambient intersection is a necessary-condition test; zeros close channels, a nonzero would only name a candidate.",
        "Zero-capacity channels are closed by exact welded character parity arithmetic, not by numeric filtering.",
        "Mirror-sector channels are decided by representation-identity transfer from the corresponding S channels, per the Phase416 census pinning.",
        "No bosonic projection map, action, VEV selection, observed-field rows, weak-angle lineage, pole extraction, or GeV normalization is supplied by any reviewed source.",
        "No Phase201 or Phase256 fill.",
    },
    decision,
};

var options = JsonOptions();
File.WriteAllText(Path.Combine(outputDir, "cross_carrier_bilinear_sm_doublet_completion_audit.json"), JsonSerializer.Serialize(result, options));
string summaryPath = Path.Combine(outputDir, "cross_carrier_bilinear_sm_doublet_completion_audit_summary.json");
File.WriteAllText(summaryPath, JsonSerializer.Serialize(result, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"crossCarrierBilinearSmDoubletCompletionAuditPassed={crossCarrierBilinearSmDoubletCompletionAuditPassed}");
foreach (var (channel, capacity, closedByParity) in capacityRows)
    Console.WriteLine($"capacity {channel}: {capacity}{(closedByParity ? " (closed by parity)" : string.Empty)}");
foreach (var c in channelReports)
    Console.WriteLine($"{c.ChannelId}: Vw={c.CandidateWeightSectorDimension} doubletIsotypicRealDim={c.DoubletIsotypicRealDimension} intersectionRealDim={c.IntersectionRealDimension} topEigenvalue={c.TopGramEigenvalue:F6}");
Console.WriteLine($"qLinearCarrierHasNoWeldedScalar={qLinearCarrierHasNoWeldedScalar}");
Console.WriteLine($"allNonzeroChannelsSmDoubletAbsent={allNonzeroChannelsSmDoubletAbsent}");
Console.WriteLine($"bilinearCompositeLayerClosedOnAllSourcePinnedCarriers={bilinearCompositeLayerClosedOnAllSourcePinnedCarriers}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");
Console.WriteLine($"summaryPath={summaryPath}");

// ---------------------------------------------------------------------------
// Helpers.
// ---------------------------------------------------------------------------

static Complex[,] PadTo32(Complex[,] m)
{
    // adapter so Restrict (index-list based) works on 4x4 matrices too
    return m;
}

static Complex[] ApplyC(Complex[,] op, Complex[] v)
{
    int n = op.GetLength(0);
    var result = new Complex[n];
    for (int r = 0; r < n; r++)
    {
        Complex sum = Complex.Zero;
        for (int c = 0; c < n; c++)
            sum += op[r, c] * v[c];
        result[r] = sum;
    }
    return result;
}

static Complex[,] CombineComplexPairs(Complex[][,] pairOps, double[] coefficients)
{
    int n = pairOps[0].GetLength(0);
    var result = new Complex[n, n];
    for (int q = 0; q < pairOps.Length; q++)
        if (Math.Abs(coefficients[q]) > 1e-15)
            for (int r = 0; r < n; r++)
                for (int c = 0; c < n; c++)
                    result[r, c] += coefficients[q] * pairOps[q][r, c];
    return result;
}

static Complex[,] CasimirComplex(Complex[][,] generators)
{
    int n = generators[0].GetLength(0);
    var result = new Complex[n, n];
    foreach (var g in generators)
    {
        var sq = CMatMul(g, g);
        for (int r = 0; r < n; r++)
            for (int c = 0; c < n; c++)
                result[r, c] -= sq[r, c];
    }
    return result;
}

static Complex[,] KronDiagonal(Complex[,] st, Complex[,] w, int sdim, int idim)
{
    int n = sdim * idim;
    var result = new Complex[n, n];
    for (int s = 0; s < sdim; s++)
        for (int t = 0; t < sdim; t++)
            if (st[s, t] != Complex.Zero)
                for (int k = 0; k < idim; k++)
                    result[s * idim + k, t * idim + k] += st[s, t];
    for (int s = 0; s < sdim; s++)
        for (int r = 0; r < idim; r++)
            for (int c = 0; c < idim; c++)
                if (w[r, c] != Complex.Zero)
                    result[s * idim + r, s * idim + c] += w[r, c];
    return result;
}

static Complex[,] KronSecond(Complex[,] w, int sdim, int idim)
{
    int n = sdim * idim;
    var result = new Complex[n, n];
    for (int s = 0; s < sdim; s++)
        for (int r = 0; r < idim; r++)
            for (int c = 0; c < idim; c++)
                if (w[r, c] != Complex.Zero)
                    result[s * idim + r, s * idim + c] = w[r, c];
    return result;
}

static int ContentDimension(List<WeldBlock> content) =>
    content.Sum(b => (int)Math.Round((2 * b.J1 + 1) * (2 * b.J2 + 1)) * b.Multiplicity);

static List<WeldBlock> TensorContent((double J1, double J2) factor, List<WeldBlock> baseContent)
{
    var dict = new Dictionary<(double, double), int>();
    foreach (var (j1, j2, m) in baseContent)
        for (double x = Math.Abs(factor.J1 - j1); x <= factor.J1 + j1 + 1e-9; x += 1.0)
            for (double y = Math.Abs(factor.J2 - j2); y <= factor.J2 + j2 + 1e-9; y += 1.0)
            {
                var key = (Math.Round(x * 2) / 2, Math.Round(y * 2) / 2);
                dict[key] = dict.GetValueOrDefault(key) + m;
            }
    return dict
        .Select(kv => new WeldBlock(kv.Key.Item1, kv.Key.Item2, kv.Value))
        .OrderBy(t => t.J1)
        .ThenBy(t => t.J2)
        .ToList();
}

static int SingletCapacity(List<WeldBlock> left, List<WeldBlock> right)
{
    int total = 0;
    foreach (var a in left)
        foreach (var b in right)
            if (Math.Abs(a.J1 - b.J1) <= 1e-9 && Math.Abs(a.J2 - b.J2) <= 1e-9)
                total += a.Multiplicity * b.Multiplicity;
    return total;
}

static List<Complex[]> ExtractComplexVectorsByCluster(double[] eigenvalues, double[,] vectors, int complexDim, Func<double, bool> select)
{
    int n2 = 2 * complexDim;
    var order = Enumerable.Range(0, n2).Where(e => select(eigenvalues[e])).OrderBy(e => eigenvalues[e]).ToArray();
    var chosen = new List<Complex[]>();
    int idx = 0;
    while (idx < order.Length)
    {
        int start = idx;
        double anchor = eigenvalues[order[start]];
        while (idx < order.Length && Math.Abs(eigenvalues[order[idx]] - anchor) <= 1e-7 * Math.Max(1.0, Math.Abs(anchor)))
            idx++;
        var clusterChosen = new List<Complex[]>();
        for (int e = start; e < idx; e++)
        {
            var z = new Complex[complexDim];
            for (int k = 0; k < complexDim; k++)
                z[k] = new Complex(vectors[k, order[e]], vectors[k + complexDim, order[e]]);
            foreach (var u in clusterChosen)
            {
                Complex ip = Complex.Zero;
                for (int k = 0; k < complexDim; k++)
                    ip += Complex.Conjugate(u[k]) * z[k];
                for (int k = 0; k < complexDim; k++)
                    z[k] -= ip * u[k];
            }
            double norm = 0.0;
            for (int k = 0; k < complexDim; k++)
                norm += z[k].Real * z[k].Real + z[k].Imaginary * z[k].Imaginary;
            norm = Math.Sqrt(norm);
            if (norm < 0.5)
                continue;
            for (int k = 0; k < complexDim; k++)
                z[k] /= norm;
            clusterChosen.Add(z);
        }
        chosen.AddRange(clusterChosen);
    }
    return chosen;
}

static Complex[,] Kron(Complex[,] a, Complex[,] b)
{
    int ar = a.GetLength(0), ac = a.GetLength(1), br = b.GetLength(0), bc = b.GetLength(1);
    var result = new Complex[ar * br, ac * bc];
    for (int i = 0; i < ar; i++)
        for (int j = 0; j < ac; j++)
            for (int k = 0; k < br; k++)
                for (int l = 0; l < bc; l++)
                    result[i * br + k, j * bc + l] = a[i, j] * b[k, l];
    return result;
}

static Complex[,] CMatMul(Complex[,] a, Complex[,] b)
{
    int n = a.GetLength(0);
    int m = b.GetLength(1);
    int kk = a.GetLength(1);
    var result = new Complex[n, m];
    for (int r = 0; r < n; r++)
        for (int c = 0; c < m; c++)
        {
            Complex sum = Complex.Zero;
            for (int k = 0; k < kk; k++)
                sum += a[r, k] * b[k, c];
            result[r, c] = sum;
        }
    return result;
}

static double[,] Realify(Complex[,] m)
{
    int n = m.GetLength(0);
    var result = new double[2 * n, 2 * n];
    for (int r = 0; r < n; r++)
        for (int c = 0; c < n; c++)
        {
            result[r, c] = m[r, c].Real;
            result[r, c + n] = -m[r, c].Imaginary;
            result[r + n, c] = m[r, c].Imaginary;
            result[r + n, c + n] = m[r, c].Real;
        }
    return result;
}

static double[,] AddM(double[,] a, double[,] b)
{
    int n = a.GetLength(0);
    var result = new double[n, n];
    for (int r = 0; r < n; r++)
        for (int c = 0; c < n; c++)
            result[r, c] = a[r, c] + b[r, c];
    return result;
}

static double[,] ScaleM(double[,] a, double s)
{
    int n = a.GetLength(0);
    var result = new double[n, n];
    for (int r = 0; r < n; r++)
        for (int c = 0; c < n; c++)
            result[r, c] = s * a[r, c];
    return result;
}

static double[,] MatComm(double[,] a, double[,] b)
{
    int n = a.GetLength(0);
    var result = new double[n, n];
    for (int r = 0; r < n; r++)
        for (int c = 0; c < n; c++)
        {
            double sum = 0.0;
            for (int k = 0; k < n; k++)
                sum += a[r, k] * b[k, c] - b[r, k] * a[k, c];
            result[r, c] = sum;
        }
    return result;
}

static double[,] CasimirRealComplex(IEnumerable<Complex[,]> generators)
{
    double[,]? casimir = null;
    foreach (var g in generators)
    {
        var real = Realify(g);
        int n = real.GetLength(0);
        casimir ??= new double[n, n];
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
            {
                double sum = 0.0;
                for (int k = 0; k < n; k++)
                    sum += real[i, k] * real[k, j];
                casimir[i, j] -= sum;
            }
    }
    return casimir!;
}

static (double[][] A, double[][] B) BuildSu2Coefficients(List<(int I, int J)> so4Pairs)
{
    int PairIndex(int i, int j) => so4Pairs.FindIndex(p => p.I == Math.Min(i, j) && p.J == Math.Max(i, j));
    var rotationTerms = new[]
    {
        new[] { (PairIndex(2, 3), 1.0) },
        new[] { (PairIndex(1, 3), -1.0) },
        new[] { (PairIndex(1, 2), 1.0) },
    };
    var boostTerms = new[]
    {
        new[] { (PairIndex(0, 1), 1.0) },
        new[] { (PairIndex(0, 2), 1.0) },
        new[] { (PairIndex(0, 3), 1.0) },
    };
    var aCoefficients = new double[3][];
    var bCoefficients = new double[3][];
    for (int i = 0; i < 3; i++)
    {
        aCoefficients[i] = new double[6];
        bCoefficients[i] = new double[6];
        foreach (var (idx, coeff) in rotationTerms[i])
        {
            aCoefficients[i][idx] += 0.5 * coeff;
            bCoefficients[i][idx] += 0.5 * coeff;
        }
        foreach (var (idx, coeff) in boostTerms[i])
        {
            aCoefficients[i][idx] += 0.5 * coeff;
            bCoefficients[i][idx] -= 0.5 * coeff;
        }
    }
    return (aCoefficients, bCoefficients);
}

static List<double[,]> BuildColorGenerators(double[,] jComplex)
{
    double[,] V10Local(int i, int j)
    {
        var m = new double[10, 10];
        m[i, j] = 1.0;
        m[j, i] = -1.0;
        return m;
    }

    var so6Pairs = new List<(int I, int J)>();
    for (int i = 0; i < 6; i++)
        for (int j = i + 1; j < 6; j++)
            so6Pairs.Add((i, j));
    int n6 = so6Pairs.Count;
    var map = new double[n6, n6];
    for (int col = 0; col < n6; col++)
    {
        var bracket = MatComm(V10Local(so6Pairs[col].I, so6Pairs[col].J), jComplex);
        for (int row = 0; row < n6; row++)
            map[row, col] = bracket[so6Pairs[row].I, so6Pairs[row].J];
    }
    var gram6 = new double[n6, n6];
    for (int a = 0; a < n6; a++)
        for (int b = 0; b < n6; b++)
        {
            double sum = 0.0;
            for (int r = 0; r < n6; r++)
                sum += map[r, a] * map[r, b];
            gram6[a, b] = sum;
        }
    var (ev6, vec6) = Jacobi(gram6);
    var jCoefficients = new double[n6];
    for (int k = 0; k < n6; k++)
        jCoefficients[k] = jComplex[so6Pairs[k].I, so6Pairs[k].J];
    double jNorm = Math.Sqrt(jCoefficients.Sum(x => x * x));
    for (int k = 0; k < n6; k++)
        jCoefficients[k] /= jNorm;
    var colorGenerators = new List<double[,]>();
    for (int e = 0; e < n6; e++)
    {
        if (Math.Abs(ev6[e]) > 1e-9)
            continue;
        var coefficient = new double[n6];
        for (int k = 0; k < n6; k++)
            coefficient[k] = vec6[k, e];
        double overlap = 0.0;
        for (int k = 0; k < n6; k++)
            overlap += coefficient[k] * jCoefficients[k];
        for (int k = 0; k < n6; k++)
            coefficient[k] -= overlap * jCoefficients[k];
        double norm = Math.Sqrt(coefficient.Sum(x => x * x));
        if (norm < 1e-8)
            continue;
        var gen = new double[10, 10];
        for (int k = 0; k < n6; k++)
        {
            gen[so6Pairs[k].I, so6Pairs[k].J] += coefficient[k] / norm;
            gen[so6Pairs[k].J, so6Pairs[k].I] -= coefficient[k] / norm;
        }
        foreach (var prev in colorGenerators)
        {
            double ip = 0.0;
            for (int r = 0; r < 10; r++)
                for (int c = 0; c < 10; c++)
                    ip += gen[r, c] * prev[r, c];
            for (int r = 0; r < 10; r++)
                for (int c = 0; c < 10; c++)
                    gen[r, c] -= ip * prev[r, c] / 2.0;
        }
        double fro = 0.0;
        for (int r = 0; r < 10; r++)
            for (int c = 0; c < 10; c++)
                fro += gen[r, c] * gen[r, c];
        if (fro < 1e-10)
            continue;
        double scale = Math.Sqrt(2.0 / fro);
        for (int r = 0; r < 10; r++)
            for (int c = 0; c < 10; c++)
                gen[r, c] *= scale;
        colorGenerators.Add(gen);
    }
    return colorGenerators;
}

static Complex[][,] BuildGamma4(Complex[][,] paulis)
{
    Complex[,] TensorString4(int[] codes)
    {
        Complex[,] result = new Complex[1, 1];
        result[0, 0] = Complex.One;
        foreach (int code in codes)
            result = Kron(result, paulis[code]);
        return result;
    }

    var gamma4 = new Complex[4][,];
    for (int k = 0; k < 2; k++)
    {
        var codesEven = new int[2];
        var codesOdd = new int[2];
        for (int p = 0; p < 2; p++)
        {
            codesEven[p] = p < k ? 2 : (p == k ? 0 : 3);
            codesOdd[p] = p < k ? 2 : (p == k ? 1 : 3);
        }
        gamma4[2 * k] = TensorString4(codesEven);
        gamma4[2 * k + 1] = TensorString4(codesOdd);
    }
    return gamma4;
}

static (Complex[][,] Left, Complex[][,] Right, bool WeylHalvesAreTwoDimensional, double HomResidual) BuildSpacetimeWeylGenerators(
    Complex[][,] gamma4,
    List<(int I, int J)> so4Pairs)
{
    double[,] M4Local(int i, int j)
    {
        var m = new double[4, 4];
        m[i, j] = 1.0;
        m[j, i] = -1.0;
        return m;
    }

    Complex[,] Sigma4(double[,] so4Element)
    {
        var result = new Complex[4, 4];
        for (int i = 0; i < 4; i++)
            for (int j = i + 1; j < 4; j++)
            {
                double coefficient = so4Element[i, j];
                if (Math.Abs(coefficient) < 1e-15)
                    continue;
                var gij = CMatMul(gamma4[i], gamma4[j]);
                var gji = CMatMul(gamma4[j], gamma4[i]);
                for (int r = 0; r < 4; r++)
                    for (int c = 0; c < 4; c++)
                        result[r, c] += coefficient * (gij[r, c] - gji[r, c]) / 4.0;
            }
        return result;
    }

    var product = gamma4[0];
    for (int i = 1; i < 4; i++)
        product = CMatMul(product, gamma4[i]);
    var square = CMatMul(product, product);
    Complex phase = Complex.Sqrt(1.0 / square[0, 0]);
    var chirality4 = new Complex[4, 4];
    for (int r = 0; r < 4; r++)
        for (int c = 0; c < 4; c++)
            chirality4[r, c] = phase * product[r, c];
    double offDiagonal = 0.0;
    for (int r = 0; r < 4; r++)
        for (int c = 0; c < 4; c++)
            if (r != c)
                offDiagonal = Math.Max(offDiagonal, chirality4[r, c].Magnitude);
    var axesL = new List<int>();
    var axesR = new List<int>();
    for (int r = 0; r < 4; r++)
    {
        if ((chirality4[r, r] - Complex.One).Magnitude < 1e-10)
            axesL.Add(r);
        else
            axesR.Add(r);
    }

    double homResidual = 0.0;
    for (int x = 0; x < 6; x++)
        for (int y = x + 1; y < 6; y++)
        {
            var mx = M4Local(so4Pairs[x].I, so4Pairs[x].J);
            var my = M4Local(so4Pairs[y].I, so4Pairs[y].J);
            var lhs = Sigma4(MatComm(mx, my));
            var sx = Sigma4(mx);
            var sy = Sigma4(my);
            var rhs = CMatMul(sx, sy);
            var rhs2 = CMatMul(sy, sx);
            for (int r = 0; r < 4; r++)
                for (int c = 0; c < 4; c++)
                    homResidual = Math.Max(homResidual, (lhs[r, c] - (rhs[r, c] - rhs2[r, c])).Magnitude);
        }

    Complex[,] Restrict2(Complex[,] full, List<int> axes)
    {
        var result = new Complex[2, 2];
        for (int r = 0; r < 2; r++)
            for (int c = 0; c < 2; c++)
                result[r, c] = full[axes[r], axes[c]];
        return result;
    }

    var left = so4Pairs.Select(p => Restrict2(Sigma4(M4Local(p.I, p.J)), axesL)).ToArray();
    var right = so4Pairs.Select(p => Restrict2(Sigma4(M4Local(p.I, p.J)), axesR)).ToArray();
    return (left, right, offDiagonal < 1e-12 && axesL.Count == 2 && axesR.Count == 2, homResidual);
}

static (double[] Eigenvalues, double[,] Vectors) Jacobi(double[,] input)
{
    int n = input.GetLength(0);
    var a = (double[,])input.Clone();
    var vectors = new double[n, n];
    for (int i = 0; i < n; i++)
        vectors[i, i] = 1.0;
    for (int sweep = 0; sweep < 400; sweep++)
    {
        double off = 0.0;
        for (int p = 0; p < n; p++)
            for (int q = p + 1; q < n; q++)
                off += a[p, q] * a[p, q];
        if (Math.Sqrt(off) < 1e-12)
            break;
        for (int p = 0; p < n - 1; p++)
            for (int q = p + 1; q < n; q++)
            {
                double apq = a[p, q];
                if (Math.Abs(apq) < 1e-15)
                    continue;
                double app = a[p, p], aqq = a[q, q];
                double tau = (aqq - app) / (2.0 * apq);
                double t = Math.Sign(tau == 0 ? 1.0 : tau) / (Math.Abs(tau) + Math.Sqrt(1.0 + tau * tau));
                double c = 1.0 / Math.Sqrt(1.0 + t * t);
                double s = t * c;
                for (int k = 0; k < n; k++)
                {
                    if (k == p || k == q)
                        continue;
                    double akp = a[k, p], akq = a[k, q];
                    a[k, p] = a[p, k] = c * akp - s * akq;
                    a[k, q] = a[q, k] = s * akp + c * akq;
                }
                a[p, p] = c * c * app - 2.0 * s * c * apq + s * s * aqq;
                a[q, q] = s * s * app + 2.0 * s * c * apq + c * c * aqq;
                a[p, q] = a[q, p] = 0.0;
                for (int k = 0; k < n; k++)
                {
                    double vkp = vectors[k, p], vkq = vectors[k, q];
                    vectors[k, p] = c * vkp - s * vkq;
                    vectors[k, q] = s * vkp + c * vkq;
                }
            }
    }
    var values = new double[n];
    for (int i = 0; i < n; i++)
        values[i] = a[i, i];
    return (values, vectors);
}

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) &&
    (value.ValueKind == JsonValueKind.True || value.ValueKind == JsonValueKind.False)
        ? value.GetBoolean()
        : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Number
        ? value.GetInt32()
        : null;

static JsonSerializerOptions JsonOptions() => new()
{
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
};

public sealed record WeldBlock(double J1, double J2, int Multiplicity);
public sealed record ChannelReport(
    string ChannelId,
    int CandidateWeightSectorDimension,
    int DoubletIsotypicRealDimension,
    int IntersectionRealDimension,
    double TopGramEigenvalue,
    double SecondGramEigenvalue);

public sealed record InternalBlock(
    Complex[][,] L,
    Complex[][,] Color,
    Complex[][,] AWeld,
    Complex[][,] BWeld,
    double[] Y,
    double[] ML,
    double[] C1,
    double[] C2);

public sealed record Leg(
    string Name,
    int Dim,
    CMat[] A,
    CMat[] B,
    CMat[] L,
    CMat[] Color,
    double[] Y,
    double[] ML,
    double[] C1,
    double[] C2);

// Dense complex matrix (possibly rectangular) in flat Re/Im arrays.
public sealed class CMat
{
    public readonly int Rows;
    public readonly int Cols;
    public readonly double[] Re;
    public readonly double[] Im;
    private CMat? squared;

    public CMat(int rows, int cols)
    {
        Rows = rows;
        Cols = cols;
        Re = new double[rows * cols];
        Im = new double[rows * cols];
    }

    public static CMat From(Complex[,] m)
    {
        int rows = m.GetLength(0), cols = m.GetLength(1);
        var result = new CMat(rows, cols);
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
            {
                result.Re[r * cols + c] = m[r, c].Real;
                result.Im[r * cols + c] = m[r, c].Imaginary;
            }
        return result;
    }

    public Complex At(int r, int c) => new(Re[r * Cols + c], Im[r * Cols + c]);

    public void Set(int r, int c, Complex value)
    {
        Re[r * Cols + c] = value.Real;
        Im[r * Cols + c] = value.Imaginary;
    }

    public CMat Copy()
    {
        var result = new CMat(Rows, Cols);
        Array.Copy(Re, result.Re, Re.Length);
        Array.Copy(Im, result.Im, Im.Length);
        return result;
    }

    public void Clear()
    {
        Array.Clear(Re);
        Array.Clear(Im);
    }

    public void SubtractInPlace(CMat other)
    {
        for (int i = 0; i < Re.Length; i++)
        {
            Re[i] -= other.Re[i];
            Im[i] -= other.Im[i];
        }
    }

    public void CombineInPlace(CMat cas, double selfCoefficient, double scale)
    {
        for (int i = 0; i < Re.Length; i++)
        {
            Re[i] = (cas.Re[i] + selfCoefficient * Re[i]) * scale;
            Im[i] = (cas.Im[i] + selfCoefficient * Im[i]) * scale;
        }
    }

    public CMat Squared()
    {
        var local = squared;
        if (local is null)
        {
            lock (Re)
            {
                if (squared is null)
                {
                    var computed = new CMat(Rows, Cols);
                    MulInto(this, this, computed, false);
                    squared = computed;
                }
                local = squared;
            }
        }
        return local;
    }

    public static void MulInto(CMat a, CMat b, CMat result, bool accumulate)
    {
        // result (+)= a * b  (a: n x k, b: k x m, result: n x m)
        int n = a.Rows, kdim = a.Cols, m = b.Cols;
        if (!accumulate)
            result.Clear();
        for (int i = 0; i < n; i++)
        {
            int rowA = i * kdim;
            int rowR = i * m;
            for (int k = 0; k < kdim; k++)
            {
                double are = a.Re[rowA + k];
                double aim = a.Im[rowA + k];
                if (are == 0.0 && aim == 0.0)
                    continue;
                int rowB = k * m;
                for (int j = 0; j < m; j++)
                {
                    double bre = b.Re[rowB + j];
                    double bim = b.Im[rowB + j];
                    result.Re[rowR + j] += are * bre - aim * bim;
                    result.Im[rowR + j] += are * bim + aim * bre;
                }
            }
        }
    }

    public static void MulTransposeInto(CMat a, CMat b, CMat result, bool accumulate)
    {
        // result (+)= a * b^T  (a: n x k, b: m x k, result: n x m; plain transpose)
        int n = a.Rows, kdim = a.Cols, m = b.Rows;
        if (!accumulate)
            result.Clear();
        for (int i = 0; i < n; i++)
        {
            int rowA = i * kdim;
            int rowR = i * m;
            for (int j = 0; j < m; j++)
            {
                int rowB = j * kdim;
                double sumRe = 0.0, sumIm = 0.0;
                for (int k = 0; k < kdim; k++)
                {
                    double are = a.Re[rowA + k];
                    double aim = a.Im[rowA + k];
                    double bre = b.Re[rowB + k];
                    double bim = b.Im[rowB + k];
                    sumRe += are * bre - aim * bim;
                    sumIm += are * bim + aim * bre;
                }
                result.Re[rowR + j] += sumRe;
                result.Im[rowR + j] += sumIm;
            }
        }
    }
}
