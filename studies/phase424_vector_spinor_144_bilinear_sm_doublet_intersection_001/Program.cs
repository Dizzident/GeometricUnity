using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

// Phase424: vector-spinor 144 bilinear SM-doublet intersection analysis.
//
// Phase422 established the same-chirality Majorana-like welded-scalar CAPACITY
// of the chiral vector-spinor carriers (Z_L x Z_L = 264, Z_R x Z_R = 264,
// Z_L x Z_R = 0) and explicitly deferred the direct SM-stable analysis of the
// 528-dimensional sector. This phase decides that deferred question with the
// ambient-intersection method of Phase412 (strictly stronger than a
// stable-subspace census): inside each same-chirality bilinear, compute the
// SM-doublet isotypic of the doublet-candidate weight sector and intersect it
// with the welded-spin (0,0) isotypic via exact spectral projectors.
//
// The same pipeline is also run on the observed 2 x 16 chiral carriers as a
// regression re-check of Phase411's Majorana-channel SM-doublet census, which
// (like Phase417's internal SM-Higgs-pattern census) carried the pre-2026-07-01
// |Y| = 1/2 calibration defect: the "smallest Y^2 above 0.05" heuristic
// selected the |Y| = 1/3 family value 1/9 instead of the exact lepton-doublet
// value 1/4. Both upstream phases were fixed and re-run on 2026-07-01; this
// phase asserts the corrected calibration and confirms the corrected verdicts
// with the stronger ambient method.
//
// Fail-closed: representation arithmetic only. No source defines a bosonic
// projection map, action, VEV selection, observed photon/W/Z/H rows,
// weak-angle lineage, pole extraction, or GeV normalization, so no Phase201
// or Phase256 contract field can be filled regardless of the outcome.

const string DefaultOutputDir = "studies/phase424_vector_spinor_144_bilinear_sm_doublet_intersection_001/output";
const string Phase417SummaryPath = "studies/phase417_vector_spinor_144_decomposition_probe_001/output/vector_spinor_144_decomposition_probe_summary.json";
const string Phase422SummaryPath = "studies/phase422_vector_spinor_144_bilinear_scalar_capacity_audit_001/output/vector_spinor_144_bilinear_scalar_capacity_audit_summary.json";
const string Phase411SummaryPath = "studies/phase411_quartic_dirac_squared_spinor_composite_probe_001/output/quartic_dirac_squared_spinor_composite_probe_summary.json";
const string ApplicationSubjectKind = "vector-spinor-144-bilinear-sm-doublet-intersection-analysis";

var outputDir = Environment.GetEnvironmentVariable("PHASE424_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

var stageClock = System.Diagnostics.Stopwatch.StartNew();
void Stage(string message) => Console.WriteLine($"[{stageClock.Elapsed:hh\\:mm\\:ss}] {message}");

using var phase417 = JsonDocument.Parse(File.ReadAllText(Phase417SummaryPath));
using var phase422 = JsonDocument.Parse(File.ReadAllText(Phase422SummaryPath));
using var phase411 = JsonDocument.Parse(File.ReadAllText(Phase411SummaryPath));

bool phase417PrecursorPassed =
    JsonBool(phase417.RootElement, "vectorSpinor144DecompositionProbePassed") is true &&
    JsonBool(phase417.RootElement, "yHalfCalibrationExact") is true &&
    JsonBool(phase417.RootElement, "vectorSpinor144LinearCarrierHasNoWeldedScalar") is true &&
    JsonInt(phase417.RootElement, "internalSmHiggsPatternComplexDimension") == 6 &&
    JsonBool(phase417.RootElement, "canFillPhase201WzContract") is false;

bool phase422PrecursorPassed =
    JsonBool(phase422.RootElement, "vectorSpinor144BilinearScalarCapacityAuditPassed") is true &&
    JsonBool(phase422.RootElement, "mixedChiralityDiracLikeScalarChannelClosed") is true &&
    JsonInt(phase422.RootElement, "sameChiralityScalarCapacity") == 528 &&
    JsonBool(phase422.RootElement, "directSmStableAnalysisDeferredDueToLargeSector") is true &&
    JsonBool(phase422.RootElement, "canFillPhase201WzContract") is false;

bool phase411PrecursorPassed =
    JsonBool(phase411.RootElement, "quarticDiracSquaredSpinorCompositeProbePassed") is true &&
    JsonBool(phase411.RootElement, "majoranaYHalfCalibrationExact") is true &&
    JsonInt(phase411.RootElement, "majoranaSpinZeroSmDoubletCount") == 0;

// ---------------------------------------------------------------------------
// Cl(10), chiral 16+/16-, Sym^2 weld, SM chain (Phase417 conventions).
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

// commuting color Cartan pair (exact, on the 10)
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

// su2L Casimir normalization and the exact |Y| = 1/2 calibration on the 16.
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
// Gamma-trace kernel projector and the SM-diagonal complex 144 basis.
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
{
    // A A^dag must equal 10 I exactly (the gammas are Hermitian involutions),
    // so P = I - A^dag A / 10 is the exact kernel projector.
    for (int r = 0; r < SpinorDim; r++)
        for (int c = 0; c < SpinorDim; c++)
        {
            Complex sum = Complex.Zero;
            for (int k = 0; k < VectorSpinorDim; k++)
                sum += gammaTrace[r, k] * Complex.Conjugate(gammaTrace[c, k]);
            sum -= r == c ? 10.0 : 0.0;
            gammaTraceFrameResidual = Math.Max(gammaTraceFrameResidual, sum.Magnitude);
        }
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

// SM-diagonal kernel basis: eigenbasis of P H P + shift (I - P) with the
// generic incommensurate Hermitian Cartan combination H.
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
        {
            ph[r, c] += Shift * ((r == c ? Complex.One : Complex.Zero) - kernelProjector[r, c]);
            // enforce exact Hermiticity before realified Jacobi
        }
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
int kernelComplexDimension = kernel144.Count;
bool kernelDimensionIs144 = kernelComplexDimension == 144;

double kernelMembershipResidual = 0.0;
foreach (var z in kernel144)
    for (int r = 0; r < SpinorDim; r++)
    {
        Complex sum = Complex.Zero;
        for (int c = 0; c < VectorSpinorDim; c++)
            sum += gammaTrace[r, c] * z[c];
        kernelMembershipResidual = Math.Max(kernelMembershipResidual, sum.Magnitude);
    }
bool kernelMembershipExact = kernelMembershipResidual <= 1e-8;

// Cartan weights of every kernel basis vector, residual-verified.
double maxCartanResidual = 0.0;
double[] KernelWeights(Complex[,] cartanOp)
{
    var weights = new double[kernel144.Count];
    for (int k = 0; k < kernel144.Count; k++)
    {
        var image = ApplyC(cartanOp, kernel144[k]);
        Complex ip = Complex.Zero;
        for (int r = 0; r < VectorSpinorDim; r++)
            ip += Complex.Conjugate(kernel144[k][r]) * image[r];
        double w = ip.Imaginary; // Op z = i w z for anti-Hermitian Op
        double res = 0.0;
        for (int r = 0; r < VectorSpinorDim; r++)
        {
            var d = image[r] - Complex.ImaginaryOne * w * kernel144[k][r];
            res += d.Real * d.Real + d.Imaginary * d.Imaginary;
        }
        maxCartanResidual = Math.Max(maxCartanResidual, Math.Sqrt(res));
        weights[k] = Math.Round(w, 9);
    }
    return weights;
}

var yWeights144 = KernelWeights(hypercharge160);
var mLWeights144 = KernelWeights(su2L160[2]);
var c1Weights144 = KernelWeights(color160[colorCartan1]);
var c2Weights144 = KernelWeights(color160[colorCartan2]);
bool smDiagonalKernelBasisExact = maxCartanResidual <= 1e-8;
Stage($"kernel basis built: {kernelComplexDimension} complex vectors, cartan residual {maxCartanResidual:E2}");

// restrict operators to the kernel in the SM-diagonal basis
Complex[,] RestrictToKernel(Complex[,] op160)
{
    int n = kernel144.Count;
    var images = new Complex[n][];
    for (int c = 0; c < n; c++)
        images[c] = ApplyC(op160, kernel144[c]);
    var result = new Complex[n, n];
    for (int r = 0; r < n; r++)
        for (int c = 0; c < n; c++)
        {
            Complex ip = Complex.Zero;
            for (int k = 0; k < VectorSpinorDim; k++)
                ip += Complex.Conjugate(kernel144[r][k]) * images[c][k];
            result[r, c] = ip;
        }
    return result;
}

var su2L144 = su2L160.Select(RestrictToKernel).ToArray();
var color144 = color160.Select(RestrictToKernel).ToArray();
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
    aWeld144[p] = RestrictToKernel(aOp);
    bWeld144[p] = RestrictToKernel(bOp);
}
Stage("kernel-restricted operators built");

// SM-diagonal basis and weights on the plain 16 (for the Phase411 re-check)
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

double maxCartanResidual16 = 0.0;
double[] Weights16(Complex[,] cartanOp)
{
    var weights = new double[basis16.Count];
    for (int k = 0; k < basis16.Count; k++)
    {
        var image = ApplyC(cartanOp, basis16[k]);
        Complex ip = Complex.Zero;
        for (int r = 0; r < 16; r++)
            ip += Complex.Conjugate(basis16[k][r]) * image[r];
        double w = ip.Imaginary;
        double res = 0.0;
        for (int r = 0; r < 16; r++)
        {
            var d = image[r] - Complex.ImaginaryOne * w * basis16[k][r];
            res += d.Real * d.Real + d.Imaginary * d.Imaginary;
        }
        maxCartanResidual16 = Math.Max(maxCartanResidual16, Math.Sqrt(res));
        weights[k] = Math.Round(w, 9);
    }
    return weights;
}

Complex[,] RestrictTo16Basis(Complex[,] op16)
{
    var result = new Complex[16, 16];
    for (int c = 0; c < 16; c++)
    {
        var image = ApplyC(op16, basis16[c]);
        for (int r = 0; r < 16; r++)
        {
            Complex ip = Complex.Zero;
            for (int k = 0; k < 16; k++)
                ip += Complex.Conjugate(basis16[r][k]) * image[k];
            result[r, c] = ip;
        }
    }
    return result;
}

var yWeights16 = Weights16(Sigma16Plus(hypercharge));
var mLWeights16 = Weights16(Sigma16Plus(su2LGen[2]));
var c1Weights16 = Weights16(Sigma16Plus(colorGenerators[colorCartan1]));
var c2Weights16 = Weights16(Sigma16Plus(colorGenerators[colorCartan2]));
bool smDiagonal16BasisExact = maxCartanResidual16 <= 1e-8;

var su2L16Diag = su2LGen.Select(g => RestrictTo16Basis(Sigma16Plus(g))).ToArray();
var color16Diag = colorGenerators.Select(g => RestrictTo16Basis(Sigma16Plus(g))).ToArray();
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
    aWeld16[p] = RestrictTo16Basis(Sigma16Plus(aOp));
    bWeld16[p] = RestrictTo16Basis(Sigma16Plus(bOp));
}

// ---------------------------------------------------------------------------
// Channel analysis: for each chiral carrier 2 (x) W, decide whether the
// bilinear carries a welded-scalar SM-doublet via the ambient intersection.
// ---------------------------------------------------------------------------

var channelReports = new List<ChannelReport>();
double weightClassLeakageResidual = 0.0;
double classKernelMaxResidual = 0.0;
double projectorIdempotencyResidual = 0.0;
double gramHermiticityResidual = 0.0;

ChannelReport AnalyzeChannel(
    string channelId,
    Complex[][,] twoGens,
    Complex[][,] wA, Complex[][,] wB, Complex[][,] wL, Complex[][,] wCol,
    double[] yW, double[] mW, double[] c1W, double[] c2W)
{
    int wdim = yW.Length;
    int legDim = 2 * wdim;
    var a2 = new Complex[3][,];
    var b2 = new Complex[3][,];
    for (int p = 0; p < 3; p++)
    {
        a2[p] = new Complex[2, 2];
        b2[p] = new Complex[2, 2];
        for (int q = 0; q < 6; q++)
        {
            if (Math.Abs(aCoefficients[p][q]) > 1e-15)
                for (int r = 0; r < 2; r++)
                    for (int c = 0; c < 2; c++)
                        a2[p][r, c] += aCoefficients[p][q] * twoGens[q][r, c];
            if (Math.Abs(bCoefficients[p][q]) > 1e-15)
                for (int r = 0; r < 2; r++)
                    for (int c = 0; c < 2; c++)
                        b2[p][r, c] += bCoefficients[p][q] * twoGens[q][r, c];
        }
    }

    var legA = new CMat[3];
    var legB = new CMat[3];
    var legL = new CMat[3];
    var legCol = new CMat[wCol.Length];
    for (int p = 0; p < 3; p++)
    {
        legA[p] = CMat.From(KronDiagonal(a2[p], wA[p], wdim));
        legB[p] = CMat.From(KronDiagonal(b2[p], wB[p], wdim));
        legL[p] = CMat.From(KronSecond(wL[p], wdim));
    }
    for (int g = 0; g < wCol.Length; g++)
        legCol[g] = CMat.From(KronSecond(wCol[g], wdim));

    // leg SM weights (SM acts trivially on the 2 factor): t = s * wdim + w
    var yLeg = new double[legDim];
    var mLeg = new double[legDim];
    var c1Leg = new double[legDim];
    var c2Leg = new double[legDim];
    for (int s = 0; s < 2; s++)
        for (int w = 0; w < wdim; w++)
        {
            yLeg[s * wdim + w] = yW[w];
            mLeg[s * wdim + w] = mW[w];
            c1Leg[s * wdim + w] = c1W[w];
            c2Leg[s * wdim + w] = c2W[w];
        }

    // doublet-candidate weight census, split into exact (sign Y, sign mL) classes
    var classes = new Dictionary<(int SY, int SM), List<(int I, int J)>>();
    for (int i = 0; i < legDim; i++)
        for (int j = 0; j < legDim; j++)
        {
            double y = yLeg[i] + yLeg[j];
            double m = mLeg[i] + mLeg[j];
            double c1 = c1Leg[i] + c1Leg[j];
            double c2 = c2Leg[i] + c2Leg[j];
            if (Math.Abs(Math.Abs(y) - 0.5) >= 1e-6 || Math.Abs(Math.Abs(m) - 0.5) >= 1e-6 ||
                Math.Abs(c1) >= 1e-6 || Math.Abs(c2) >= 1e-6)
                continue;
            var key = (Math.Sign(y), Math.Sign(m));
            if (!classes.TryGetValue(key, out var list))
                classes[key] = list = new List<(int, int)>();
            list.Add((i, j));
        }
    int vwDimension = classes.Values.Sum(c => c.Count);

    // per-class SM-doublet isotypic (kernel of C_color + (C_L - 0.75 kappaL)^2)
    var doubletVectors = new List<(int[] I, int[] J, Complex[] Coefficients)>();
    foreach (var cls in classes.Values.OrderBy(c => c[0].I * legDim + c[0].J))
    {
        int n = cls.Count;
        var idxI = cls.Select(t => t.I).ToArray();
        var idxJ = cls.Select(t => t.J).ToArray();
        var lookup = new Dictionary<long, int>(n);
        for (int k = 0; k < n; k++)
            lookup[(long)idxI[k] * legDim + idxJ[k]] = k;

        (Complex[,] Restricted, double Leak) RestrictedCasimir(CMat[] ops)
        {
            var restricted = new Complex[n, n];
            double leak = 0.0;
            object leakLock = new();
            Parallel.For(0, n, col =>
            {
                // dense image of C e_(i,j): M[k,l] = -sum_p (O2[k,i] d(l,j)
                // + d(k,i) O2[l,j] + 2 O[k,i] O[l,j])
                var image = new Complex[legDim, legDim];
                foreach (var op in ops)
                {
                    var o2 = op.Squared();
                    int i0 = idxI[col], j0 = idxJ[col];
                    for (int k = 0; k < legDim; k++)
                    {
                        image[k, j0] -= o2.At(k, i0);
                        image[i0, k] -= o2.At(k, j0);
                    }
                    for (int k = 0; k < legDim; k++)
                    {
                        var oki = op.At(k, i0);
                        if (oki == Complex.Zero)
                            continue;
                        for (int l = 0; l < legDim; l++)
                            image[k, l] -= 2.0 * oki * op.At(l, j0);
                    }
                }
                double localLeak = 0.0;
                for (int k = 0; k < legDim; k++)
                    for (int l = 0; l < legDim; l++)
                    {
                        var v = image[k, l];
                        if (v == Complex.Zero)
                            continue;
                        if (lookup.TryGetValue((long)k * legDim + l, out int row))
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

        var (cCol, leakCol) = RestrictedCasimir(legCol);
        var (cL, leakL) = RestrictedCasimir(legL);
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
            // residual verification against the complex hSm
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

    // welded-scalar spectral projector: polynomial filter to (0,0) on the
    // integer Casimir grid j = 0..6 (a superset of the channel's labels).
    var grid = Enumerable.Range(1, 6).Select(j => (double)(j * (j + 1)) * kappaCalibration).ToArray();

    void CasimirApplyInto(CMat[] ops, CMat v, CMat result, CMat t1, CMat t2)
    {
        result.Clear();
        foreach (var op in ops)
        {
            t1.Clear();
            CMat.MulInto(op, v, t1, false);
            CMat.MulTransposeInto(v, op, t1, true);
            t2.Clear();
            CMat.MulInto(op, t1, t2, false);
            CMat.MulTransposeInto(t1, op, t2, true);
            result.SubtractInPlace(t2);
        }
    }

    CMat ProjectWeldedScalar(CMat v)
    {
        var cur = v.Copy();
        var cas = new CMat(legDim);
        var t1 = new CMat(legDim);
        var t2 = new CMat(legDim);
        foreach (var ops in new[] { legA, legB })
            foreach (double root in grid)
            {
                CasimirApplyInto(ops, cur, cas, t1, t2);
                double inv = 1.0 / (0.0 - root);
                cur.CombineInPlace(cas, -root, inv);
            }
        return cur;
    }

    // Gram G[k,l] = <d_k, P00 d_l> over the pooled doublet vectors; each d_k
    // is sparse on its weight class, so columns of P00 d_l are extracted by
    // sparse dots and the dense projected matrix is discarded per column.
    var gram = new Complex[doubletComplexDimension, doubletComplexDimension];
    object idemLock = new();
    bool idemChecked = false;
    Parallel.For(0, doubletComplexDimension, l =>
    {
        var (iIdx, jIdx, coefficients) = doubletVectors[l];
        var dense = new CMat(legDim);
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
            for (int r = 0; r < legDim; r++)
                for (int c = 0; c < legDim; c++)
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
    Stage($"{channelId}: projections and Gram complete");

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
    var sorted = gramEv.OrderByDescending(x => x).ToArray();
    int intersectionRealDimension = sorted.Count(x => x >= 1.0 - 1e-6);
    Stage($"{channelId}: top eigenvalue {sorted[0]:F6}, intersection real dim {intersectionRealDimension}");
    return new ChannelReport(
        channelId,
        vwDimension,
        2 * doubletComplexDimension,
        intersectionRealDimension,
        sorted[0],
        sorted.Length > 1 ? sorted[1] : 0.0);
}

Stage("analyzing Z_L x Z_L");
channelReports.Add(AnalyzeChannel("Z_L x Z_L", twoLGens, aWeld144, bWeld144, su2L144, color144, yWeights144, mLWeights144, c1Weights144, c2Weights144));
Stage("analyzing Z_R x Z_R");
channelReports.Add(AnalyzeChannel("Z_R x Z_R", twoRGens, aWeld144, bWeld144, su2L144, color144, yWeights144, mLWeights144, c1Weights144, c2Weights144));
Stage("analyzing S_L x S_L (Phase411 ambient re-check)");
channelReports.Add(AnalyzeChannel("S_L x S_L", twoLGens, aWeld16, bWeld16, su2L16Diag, color16Diag, yWeights16, mLWeights16, c1Weights16, c2Weights16));
Stage("analyzing S_R x S_R (Phase411 ambient re-check)");
channelReports.Add(AnalyzeChannel("S_R x S_R", twoRGens, aWeld16, bWeld16, su2L16Diag, color16Diag, yWeights16, mLWeights16, c1Weights16, c2Weights16));

var llReport = channelReports[0];
var rrReport = channelReports[1];
var majoranaLLReport = channelReports[2];
var majoranaRRReport = channelReports[3];

bool sameChiralityWeldedScalarSmDoubletAbsent =
    llReport.IntersectionRealDimension == 0 && rrReport.IntersectionRealDimension == 0;
bool majorana16AmbientRecheckSmDoubletAbsent =
    majoranaLLReport.IntersectionRealDimension == 0 && majoranaRRReport.IntersectionRealDimension == 0;
bool vectorSpinor144BilinearCompositeRouteClosed =
    sameChiralityWeldedScalarSmDoubletAbsent &&
    JsonBool(phase422.RootElement, "mixedChiralityDiracLikeScalarChannelClosed") is true;

// ---------------------------------------------------------------------------
// Character-arithmetic cross-checks against the Phase417/422 records.
// ---------------------------------------------------------------------------

var leftContent = ReadWeldBlocks(phase417.RootElement.GetProperty("zCarrierLeftWeldedContent"));
var rightContent = ReadWeldBlocks(phase417.RootElement.GetProperty("zCarrierRightWeldedContent"));
int llCapacity = SingletCapacity(leftContent, leftContent);
int rrCapacity = SingletCapacity(rightContent, rightContent);
int lrCapacity = SingletCapacity(leftContent, rightContent);
bool characterCapacitiesMatchPhase422 =
    llCapacity == 264 && rrCapacity == 264 && lrCapacity == 0 &&
    JsonInt(phase422.RootElement, "sameChiralityScalarCapacity") == llCapacity + rrCapacity &&
    JsonInt(phase422.RootElement, "mixedChiralityScalarCapacity") == lrCapacity;

// ---------------------------------------------------------------------------
// Fail-closed boundary.
// ---------------------------------------------------------------------------

const bool targetBlindConstruction = true;
const bool physicalTargetsConsultedForConstruction = false;
const bool ambientIntersectionIsNecessaryConditionOnly = true;
const bool statisticsProjectionApplied = false; // full tensor square (conservative superset of Sym^2 and Lambda^2)
const bool sourceDefinesVectorSpinor144BilinearProjectionMap = false;
const bool sourceDefinesVectorSpinor144BilinearAction = false;
const bool sourceDefinesVectorSpinor144BilinearVevSelection = false;
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
    "ambient intersection of SM-doublet isotypic and welded (0,0) isotypic on Z x Z and S x S same-chirality bilinears; no target values")))).ToLowerInvariant();

bool analysisInternallyConsistent =
    chiralHalvesAreSixteenDimensional &&
    colorAlgebraDimensionIsEight &&
    colorCartanPairExactlyCommuting &&
    sixteenCarriesLeptonDoubletYQuarter &&
    gammaTraceFrameExact &&
    kernelDimensionIs144 &&
    kernelMembershipExact &&
    smDiagonalKernelBasisExact &&
    basis16Complete &&
    smDiagonal16BasisExact &&
    weylHalvesAreTwoDimensional &&
    sigma4HomResidual <= 1e-10 &&
    weightClassLeakageResidual <= 1e-7 &&
    classKernelMaxResidual <= 1e-6 &&
    projectorIdempotencyResidual <= 1e-6 &&
    gramHermiticityResidual <= 1e-8 &&
    characterCapacitiesMatchPhase422;

bool vectorSpinor144BilinearSmDoubletIntersectionAnalysisPassed =
    phase417PrecursorPassed &&
    phase422PrecursorPassed &&
    phase411PrecursorPassed &&
    analysisInternallyConsistent &&
    targetBlindConstruction &&
    !physicalTargetsConsultedForConstruction &&
    !sourceDefinesVectorSpinor144BilinearProjectionMap &&
    !sourceDefinesVectorSpinor144BilinearAction &&
    !sourceDefinesVectorSpinor144BilinearVevSelection &&
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

string terminalStatus = vectorSpinor144BilinearSmDoubletIntersectionAnalysisPassed
    ? (sameChiralityWeldedScalarSmDoubletAbsent
        ? "vector-spinor-144-bilinear-welded-scalar-sm-doublet-absent-branch-closed"
        : "vector-spinor-144-bilinear-welded-scalar-sm-doublet-candidate-needs-source-map")
    : "vector-spinor-144-bilinear-sm-doublet-intersection-analysis-blocked";

string decision = vectorSpinor144BilinearSmDoubletIntersectionAnalysisPassed
    ? (sameChiralityWeldedScalarSmDoubletAbsent
        ? "The deferred Phase422 question is decided: the 528-dimensional same-chirality Majorana-like welded-scalar capacity of the vector-spinor Z carriers contains NO SM-doublet state. The ambient intersection of the SM-doublet isotypic with the welded-spin (0,0) isotypic is zero in both Z_L x Z_L and Z_R x Z_R (top Gram eigenvalues well below 1), covering all statistics projections. Together with Phase422's mixed-chirality closure and Phase417's linear closure, the vector-spinor 144 bilinear composite route is CLOSED. The same ambient method confirms the corrected Phase411 verdict on the observed 2 x 16 Majorana channels (zero in both), discharging the 2026-07-01 |Y|=1/2 calibration defect with a stronger check. No bosonic projection map, action, VEV selection, observed rows, weak-angle lineage, pole extraction, or GeV normalization exists in any reviewed source; no Phase201 or Phase256 field is filled."
        : "The ambient intersection found welded-scalar SM-doublet candidate states in the same-chirality vector-spinor bilinear sector. This is candidate-only representation data: without a source-defined bosonic projection/action/VEV/observed-field map it cannot fill Phase201 or Phase256, and a follow-up phase must characterize the candidate and its source lineage before any gate can consider promotion.")
    : "Do not use the vector-spinor bilinear SM-doublet intersection analysis until the precursor and consistency batteries pass.";

var result = new
{
    phaseId = "phase424-vector-spinor-144-bilinear-sm-doublet-intersection-analysis",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    vectorSpinor144BilinearSmDoubletIntersectionAnalysisPassed,
    phase417PrecursorPassed,
    phase422PrecursorPassed,
    phase411PrecursorPassed,
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
    kernelComplexDimension,
    kernelDimensionIs144,
    kernelMembershipResidual,
    kernelMembershipExact,
    smDiagonalKernelBasisMaxCartanResidual = maxCartanResidual,
    smDiagonalKernelBasisExact,
    basis16Complete,
    smDiagonal16BasisMaxCartanResidual = maxCartanResidual16,
    smDiagonal16BasisExact,
    weylHalvesAreTwoDimensional,
    sigma4HomResidual,
    weightClassLeakageResidual,
    classKernelMaxResidual,
    projectorIdempotencyResidual,
    gramHermiticityResidual,
    casimirGridMaxJ = 6,
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
    llIntersectionRealDimension = llReport.IntersectionRealDimension,
    rrIntersectionRealDimension = rrReport.IntersectionRealDimension,
    sameChiralityWeldedScalarSmDoubletCount = llReport.IntersectionRealDimension + rrReport.IntersectionRealDimension,
    sameChiralityWeldedScalarSmDoubletAbsent,
    majorana16LLIntersectionRealDimension = majoranaLLReport.IntersectionRealDimension,
    majorana16RRIntersectionRealDimension = majoranaRRReport.IntersectionRealDimension,
    majorana16AmbientRecheckSmDoubletAbsent,
    vectorSpinor144BilinearCompositeRouteClosed,
    characterCapacityLL = llCapacity,
    characterCapacityRR = rrCapacity,
    characterCapacityLR = lrCapacity,
    characterCapacitiesMatchPhase422,
    analysisInternallyConsistent,
    sourceDefinesVectorSpinor144BilinearProjectionMap,
    sourceDefinesVectorSpinor144BilinearAction,
    sourceDefinesVectorSpinor144BilinearVevSelection,
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
        phase417SummaryPath = Phase417SummaryPath,
        phase422SummaryPath = Phase422SummaryPath,
        phase411SummaryPath = Phase411SummaryPath,
        primaryDraft = "docs/Reference/ExperimentReferences/texts/GU-DRAFT-2021-TEXT.txt (section 11.2 eq. 11.6; section 12.22; Z_{1/2} vector-spinor 144 remainder)",
    },
    explicitCandidateOnlyNonclaims = new[]
    {
        "The ambient intersection is a necessary-condition test; a zero closes the channel, a nonzero would only name a candidate.",
        "The analysis covers the full tensor square, a conservative superset of every statistics projection.",
        "No bosonic projection map, action, VEV selection, observed-field rows, weak-angle lineage, pole extraction, or GeV normalization is supplied by any reviewed source.",
        "The 2026-07-01 |Y|=1/2 calibration defect fix changes recorded representation counts upstream (Phase411/417), not any promotion decision; this phase asserts and re-checks the corrected values.",
        "No Phase201 or Phase256 fill.",
    },
    decision,
};

var options = JsonOptions();
File.WriteAllText(Path.Combine(outputDir, "vector_spinor_144_bilinear_sm_doublet_intersection.json"), JsonSerializer.Serialize(result, options));
string summaryPath = Path.Combine(outputDir, "vector_spinor_144_bilinear_sm_doublet_intersection_summary.json");
File.WriteAllText(summaryPath, JsonSerializer.Serialize(result, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"vectorSpinor144BilinearSmDoubletIntersectionAnalysisPassed={vectorSpinor144BilinearSmDoubletIntersectionAnalysisPassed}");
foreach (var c in channelReports)
    Console.WriteLine($"{c.ChannelId}: Vw={c.CandidateWeightSectorDimension} doubletIsotypicRealDim={c.DoubletIsotypicRealDimension} intersectionRealDim={c.IntersectionRealDimension} topEigenvalue={c.TopGramEigenvalue:F6}");
Console.WriteLine($"sameChiralityWeldedScalarSmDoubletAbsent={sameChiralityWeldedScalarSmDoubletAbsent}");
Console.WriteLine($"majorana16AmbientRecheckSmDoubletAbsent={majorana16AmbientRecheckSmDoubletAbsent}");
Console.WriteLine($"vectorSpinor144BilinearCompositeRouteClosed={vectorSpinor144BilinearCompositeRouteClosed}");
Console.WriteLine($"characterCapacitiesMatchPhase422={characterCapacitiesMatchPhase422} (LL={llCapacity} RR={rrCapacity} LR={lrCapacity})");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");
Console.WriteLine($"summaryPath={summaryPath}");

// ---------------------------------------------------------------------------
// Helpers.
// ---------------------------------------------------------------------------

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

static Complex[,] KronDiagonal(Complex[,] two, Complex[,] w, int wdim)
{
    // diagonal action on 2 (x) W: two (x) I_W + I_2 (x) w
    int n = 2 * wdim;
    var result = new Complex[n, n];
    for (int s = 0; s < 2; s++)
        for (int t = 0; t < 2; t++)
            if (two[s, t] != Complex.Zero)
                for (int k = 0; k < wdim; k++)
                    result[s * wdim + k, t * wdim + k] += two[s, t];
    for (int s = 0; s < 2; s++)
        for (int r = 0; r < wdim; r++)
            for (int c = 0; c < wdim; c++)
                if (w[r, c] != Complex.Zero)
                    result[s * wdim + r, s * wdim + c] += w[r, c];
    return result;
}

static Complex[,] KronSecond(Complex[,] w, int wdim)
{
    // I_2 (x) w on 2 (x) W
    int n = 2 * wdim;
    var result = new Complex[n, n];
    for (int s = 0; s < 2; s++)
        for (int r = 0; r < wdim; r++)
            for (int c = 0; c < wdim; c++)
                if (w[r, c] != Complex.Zero)
                    result[s * wdim + r, s * wdim + c] = w[r, c];
    return result;
}

static List<Complex[]> ExtractComplexVectorsByCluster(double[] eigenvalues, double[,] vectors, int complexDim, Func<double, bool> select)
{
    // Realified Hermitian eigenvectors come in J-pairs. Group the selected
    // eigenvectors into degenerate clusters (J-closed) and Gram-Schmidt a
    // complex basis inside each cluster: project out the complex span of the
    // vectors already chosen (which removes both z and iz components), keep
    // the vector when a new complex direction remains.
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

static IReadOnlyList<WeldBlock> ReadWeldBlocks(JsonElement array) =>
    array.EnumerateArray()
        .Select(item => new WeldBlock(
            item.GetProperty("j1").GetDouble(),
            item.GetProperty("j2").GetDouble(),
            item.GetProperty("multiplicity").GetInt32()))
        .ToArray();

static int SingletCapacity(IReadOnlyList<WeldBlock> left, IReadOnlyList<WeldBlock> right)
{
    int total = 0;
    foreach (var a in left)
        foreach (var b in right)
            if (Math.Abs(a.J1 - b.J1) <= 1e-9 && Math.Abs(a.J2 - b.J2) <= 1e-9)
                total += a.Multiplicity * b.Multiplicity;
    return total;
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

// Dense complex matrix in flat Re/Im arrays for the hot bilinear filter path.
public sealed class CMat
{
    public readonly int N;
    public readonly double[] Re;
    public readonly double[] Im;
    private CMat? squared;

    public CMat(int n)
    {
        N = n;
        Re = new double[n * n];
        Im = new double[n * n];
    }

    public static CMat From(Complex[,] m)
    {
        int n = m.GetLength(0);
        var result = new CMat(n);
        for (int r = 0; r < n; r++)
            for (int c = 0; c < n; c++)
            {
                result.Re[r * n + c] = m[r, c].Real;
                result.Im[r * n + c] = m[r, c].Imaginary;
            }
        return result;
    }

    public Complex At(int r, int c) => new(Re[r * N + c], Im[r * N + c]);

    public void Set(int r, int c, Complex value)
    {
        Re[r * N + c] = value.Real;
        Im[r * N + c] = value.Imaginary;
    }

    public CMat Copy()
    {
        var result = new CMat(N);
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
        // this = (cas + selfCoefficient * this) * scale
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
                    var computed = new CMat(N);
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
        // result (+)= a * b, ikj loop with row caching
        int n = a.N;
        if (!accumulate)
            result.Clear();
        for (int i = 0; i < n; i++)
        {
            int rowI = i * n;
            for (int k = 0; k < n; k++)
            {
                double are = a.Re[rowI + k];
                double aim = a.Im[rowI + k];
                if (are == 0.0 && aim == 0.0)
                    continue;
                int rowK = k * n;
                for (int j = 0; j < n; j++)
                {
                    double bre = b.Re[rowK + j];
                    double bim = b.Im[rowK + j];
                    result.Re[rowI + j] += are * bre - aim * bim;
                    result.Im[rowI + j] += are * bim + aim * bre;
                }
            }
        }
    }

    public static void MulTransposeInto(CMat a, CMat b, CMat result, bool accumulate)
    {
        // result (+)= a * b^T (plain transpose, no conjugation)
        int n = a.N;
        if (!accumulate)
            result.Clear();
        for (int i = 0; i < n; i++)
        {
            int rowI = i * n;
            for (int j = 0; j < n; j++)
            {
                int rowJ = j * n;
                double sumRe = 0.0, sumIm = 0.0;
                for (int k = 0; k < n; k++)
                {
                    double are = a.Re[rowI + k];
                    double aim = a.Im[rowI + k];
                    double bre = b.Re[rowJ + k];
                    double bim = b.Im[rowJ + k];
                    sumRe += are * bre - aim * bim;
                    sumIm += are * bim + aim * bre;
                }
                result.Re[rowI + j] += sumRe;
                result.Im[rowI + j] += sumIm;
            }
        }
    }
}
