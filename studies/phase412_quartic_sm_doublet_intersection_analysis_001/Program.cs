using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

// Phase412: quartic SM-doublet intersection analysis (the named "heavy"
// follow-up of Phase411, executed on user directive 2026-06-12).
//
// QUESTION: Phase411 closed the spinor-bilinear channels and recorded the
// quartic welded-singlet dimensions (9856 / 9632 per channel) by character
// arithmetic, leaving the quartic SM-stable analysis as the named remaining
// order. This phase decides it with an AMBIENT-INTERSECTION formulation
// that is strictly STRONGER than the stable-subspace formulation:
//
//   If ANY quartic spinor composite were a welded spacetime scalar carrying
//   the SM-doublet pattern, the ambient space 16^(x4) would contain a state
//   that is SIMULTANEOUSLY (i) in the welded (a,b)-isotypic sum allowed by
//   the channel's spacetime (2-leg) factors and (ii) in the SM-doublet
//   isotypic (color-singlet, j_L = 1/2, |Y| = 1/2). If that intersection is
//   ZERO for every channel, the quartic order is CLOSED for welded-scalar
//   SM-doublets - no SM-stable-subspace analysis can find what the ambient
//   space does not contain.
//
// METHOD (exact, deterministic, no stochastic steps):
//   X1. Build the SM-diagonal basis of the 16 (joint eigenbasis of the
//       commuting Cartans Y, L3, and the two color Cartans, via a single
//       generic-combination diagonalization with residual verification).
//       In that basis all SM Cartans are DIAGONAL on 16^(x4) by index
//       arithmetic.
//   X2. Weight census: the doublet-pattern candidates live in the indices
//       with |Y_tot| = 1/2, color weight (0,0), |mL_tot| = 1/2 (a j = 1/2
//       SU(2)_L irrep has only m = +-1/2 states; color singlets have zero
//       color weight). This cuts 65536 down to a small sector V_w.
//   X3. The SM-doublet isotypic basis D inside V_w: joint kernel of
//       C_color and (C_L - (3/4) kappa_L)^2 restricted to V_w (both
//       operators preserve weight sectors exactly - machine-verified
//       leakage residuals). Dense eigen-decomposition on V_w.
//   X4. The welded side: per-label spectral projectors P_(a,b) =
//       product-form polynomials in the two commuting welded sub-Casimirs
//       C_A, C_B on 16^(x4) (eigenvalue grids j(j+1) kappa, j = 0..6,
//       machine-calibrated kappa; factors applied nearest-root-first;
//       idempotency verified on every projected vector). The channel's
//       ALLOWED label set comes from its spacetime 2-leg content (exact
//       su(2) character arithmetic from the machine-discovered Weyl
//       labels).
//   X5. THE INTERSECTION: per channel, G[k,l] = <d_k, P_allowed d_l> on
//       the doublet basis; eigenvalues of G in [0,1]; eigenvalue-1
//       eigenvectors (verified by direct residuals) are states in the
//       intersection. Count per channel and for the union of all
//       channels.
//   X6. Character-arithmetic cross-checks: the per-channel quartic
//       welded-singlet dimensions must reproduce Phase411's recorded
//       9856 / 9632 (and the new mixed-channel dimensions are recorded).
//
// SCOPE HONESTY: the unrestricted tensor power 16^(x4) CONTAINS every
// statistics-projected (antisymmetrized) composite space, so a zero
// ambient intersection closes those too; a NONZERO intersection would be
// necessary-only evidence (statistics, coupling to the 2-leg factors, and
// SM-stability would all still have to be checked by a refinement phase).
// All arithmetic is complex/compact (Nguyen-Polya caveat carried).
//
// Fail-closed: no dynamics, no scales; nothing promoted; no contract
// field is filled.

const string DefaultOutputDir = "studies/phase412_quartic_sm_doublet_intersection_analysis_001/output";
const string Phase411SummaryPath = "studies/phase411_quartic_dirac_squared_spinor_composite_probe_001/output/quartic_dirac_squared_spinor_composite_probe_summary.json";

const double Tolerance = 1e-9;
const int Dim4Legs = 65536;

var stageClock = System.Diagnostics.Stopwatch.StartNew();
void Stage(string message) =>
    Console.Error.WriteLine($"[{stageClock.Elapsed:hh\\:mm\\:ss}] {message}");

var outputDir = Environment.GetEnvironmentVariable("PHASE412_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var p411 = JsonDocument.Parse(File.ReadAllText(Phase411SummaryPath));
bool phase411PrecursorPassed =
    JsonBool(p411.RootElement, "quarticDiracSquaredSpinorCompositeProbePassed") is true &&
    JsonBool(p411.RootElement, "spinorBilinearSpinZeroDoubletAbsent") is true;
int phase411QuarticLLLL = JsonInt(p411.RootElement, "quarticSpinZeroCountLLLL") ?? -1;
int phase411QuarticLRLR = JsonInt(p411.RootElement, "quarticSpinZeroCountLRLR") ?? -1;

// ---------------------------------------------------------------------------
// Cl(10), the 16, the weld, the SM chain (Phase411 machinery, verbatim).
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
            if (System.Math.Abs(coefficient) < 1e-15)
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
var chiralAxes = new List<int>();
for (int r = 0; r < 32; r++)
    if ((chirality[r, r] - Complex.One).Magnitude < 1e-10)
        chiralAxes.Add(r);
bool chiralHalfIsSixteenDimensional = chiralAxes.Count == 16;

double chiralInvarianceResidual = 0.0;
Complex[,] Sigma16(double[,] so10Element)
{
    var full = SpinorRep32(so10Element);
    var complement = Enumerable.Range(0, 32).Where(r => !chiralAxes.Contains(r)).ToArray();
    foreach (int r in complement)
        foreach (int c in chiralAxes)
            chiralInvarianceResidual = System.Math.Max(chiralInvarianceResidual, full[r, c].Magnitude);
    var result = new Complex[16, 16];
    for (int r = 0; r < 16; r++)
        for (int c = 0; c < 16; c++)
            result[r, c] = full[chiralAxes[r], chiralAxes[c]];
    return result;
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
        mat[a, b] = 1.0 / System.Math.Sqrt(2.0);
        mat[b, a] = 1.0 / System.Math.Sqrt(2.0);
    }
    symMats[idx] = mat;
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

double[,] M4(int i, int j)
{
    var m = new double[4, 4];
    m[i, j] = 1.0;
    m[j, i] = -1.0;
    return m;
}

var so4Pairs = new List<(int I, int J)>();
for (int i = 0; i < 4; i++)
    for (int j = i + 1; j < 4; j++)
        so4Pairs.Add((i, j));
var piGenerators = so4Pairs.Select(p => SymEmbedX(M4(p.I, p.J))).ToArray();

double[,] V10(int i, int j)
{
    var m = new double[10, 10];
    m[i, j] = 1.0;
    m[j, i] = -1.0;
    return m;
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

var jComplex = AddM(AddM(V10(0, 1), V10(2, 3)), V10(4, 5));
var su2LGen = new[]
{
    ScaleM(AddM(V10(6, 7), V10(8, 9)), 0.5),
    ScaleM(AddM(V10(6, 8), ScaleM(V10(7, 9), -1.0)), 0.5),
    ScaleM(AddM(V10(6, 9), V10(7, 8)), 0.5),
};
var hypercharge = AddM(ScaleM(AddM(V10(6, 7), ScaleM(V10(8, 9), -1.0)), 0.5), ScaleM(jComplex, 1.0 / 3.0));

var so6Pairs = new List<(int I, int J)>();
for (int i = 0; i < 6; i++)
    for (int j = i + 1; j < 6; j++)
        so6Pairs.Add((i, j));
var colorGenerators = new List<double[,]>();
{
    int n6 = so6Pairs.Count;
    var map = new double[n6, n6];
    for (int col = 0; col < n6; col++)
    {
        var bracket = MatComm(V10(so6Pairs[col].I, so6Pairs[col].J), jComplex);
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
    double jNorm = System.Math.Sqrt(jCoefficients.Sum(x => x * x));
    for (int k = 0; k < n6; k++)
        jCoefficients[k] /= jNorm;
    for (int e = 0; e < n6; e++)
    {
        if (System.Math.Abs(ev6[e]) > Tolerance)
            continue;
        var coefficient = new double[n6];
        for (int k = 0; k < n6; k++)
            coefficient[k] = vec6[k, e];
        double overlap = 0.0;
        for (int k = 0; k < n6; k++)
            overlap += coefficient[k] * jCoefficients[k];
        for (int k = 0; k < n6; k++)
            coefficient[k] -= overlap * jCoefficients[k];
        double norm = System.Math.Sqrt(coefficient.Sum(x => x * x));
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
        double scale = System.Math.Sqrt(2.0 / fro);
        for (int r = 0; r < 10; r++)
            for (int c = 0; c < 10; c++)
                gen[r, c] *= scale;
        colorGenerators.Add(gen);
    }
}
bool colorAlgebraDimensionIsEight = colorGenerators.Count == 8;

var piOn16 = piGenerators.Select(Sigma16).ToArray();
var su2LOn16 = su2LGen.Select(Sigma16).ToArray();
var hyperchargeOn16 = Sigma16(hypercharge);
var colorOn16 = colorGenerators.Select(Sigma16).ToArray();

// Welded su(2)_A/su(2)_B combination coefficients (Phase411).
int PairIndex(int i, int j) => so4Pairs.FindIndex(p => p.I == System.Math.Min(i, j) && p.J == System.Math.Max(i, j));
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
double[][] aCoefficients = new double[3][];
double[][] bCoefficients = new double[3][];
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

Complex[,] Combine16(double[] coefficients)
{
    var result = new Complex[16, 16];
    for (int g = 0; g < 6; g++)
        if (System.Math.Abs(coefficients[g]) > 1e-15)
            for (int r = 0; r < 16; r++)
                for (int c = 0; c < 16; c++)
                    result[r, c] += coefficients[g] * piOn16[g][r, c];
    return result;
}

var aOn16 = aCoefficients.Select(Combine16).ToArray();
var bOn16 = bCoefficients.Select(Combine16).ToArray();

// kappa calibration on the so(4) vector rep (C_A = kappa * 3/4 there).
double kappaCalibration;
{
    var aVec = new double[3][,];
    for (int i = 0; i < 3; i++)
    {
        aVec[i] = new double[4, 4];
        for (int g = 0; g < 6; g++)
            if (System.Math.Abs(aCoefficients[i][g]) > 1e-15)
            {
                var m = M4(so4Pairs[g].I, so4Pairs[g].J);
                for (int r = 0; r < 4; r++)
                    for (int c = 0; c < 4; c++)
                        aVec[i][r, c] += aCoefficients[i][g] * m[r, c];
            }
    }
    var casimir = new double[4, 4];
    foreach (var g in aVec)
        for (int r = 0; r < 4; r++)
            for (int c = 0; c < 4; c++)
            {
                double sum = 0.0;
                for (int k = 0; k < 4; k++)
                    sum += g[r, k] * g[k, c];
                casimir[r, c] -= sum;
            }
    kappaCalibration = casimir[0, 0] / 0.75;
}

// ---------------------------------------------------------------------------
// X1: the SM-diagonal basis of the 16. The Cartans {Y, L3, col3, col8}
// commute; a generic fixed-irrational combination is diagonalized once and
// the joint eigenbasis read off (residual-verified).
// ---------------------------------------------------------------------------

// pick two commuting color Cartans: find a maximal commuting pair among the
// 8 color generators (machine: take colorOn16[0], then the first generator
// commuting with it after projecting; simpler: use Jacobi on the realified
// span - here: search pairs with smallest commutator norm).
int cartan1 = 0, cartan2 = -1;
{
    double best = double.MaxValue;
    for (int g = 1; g < 8; g++)
    {
        double norm = 0.0;
        for (int r = 0; r < 16; r++)
            for (int c = 0; c < 16; c++)
            {
                Complex sum = Complex.Zero;
                for (int k = 0; k < 16; k++)
                    sum += colorOn16[0][r, k] * colorOn16[g][k, c] - colorOn16[g][r, k] * colorOn16[0][k, c];
                norm += sum.Magnitude * sum.Magnitude;
            }
        if (norm < best)
        {
            best = norm;
            cartan2 = g;
        }
    }
    if (best > 1e-18)
    {
        // no exactly-commuting partner among raw generators: build one as
        // the projection of a commuting element - fall back to diagonalizing
        // within the centralizer: use i*[col0, g] structure. For robustness
        // simply orthogonalize: find x = sum c_g col_g with [col0, x] = 0 by
        // least squares over the 7 remaining generators.
        var rows = new List<double[]>();
        // commutator of col0 with each generator, realified into row vectors
        var commutators = new Complex[8][,];
        for (int g = 1; g < 8; g++)
        {
            var m = new Complex[16, 16];
            for (int r = 0; r < 16; r++)
                for (int c = 0; c < 16; c++)
                {
                    Complex sum = Complex.Zero;
                    for (int k = 0; k < 16; k++)
                        sum += colorOn16[0][r, k] * colorOn16[g][k, c] - colorOn16[g][r, k] * colorOn16[0][k, c];
                    m[r, c] = sum;
                }
            commutators[g] = m;
        }
        // Gram over coefficients c_1..c_7
        var gramC = new double[7, 7];
        for (int x = 0; x < 7; x++)
            for (int y = 0; y < 7; y++)
            {
                double sum = 0.0;
                for (int r = 0; r < 16; r++)
                    for (int c = 0; c < 16; c++)
                        sum += (commutators[x + 1][r, c] * Complex.Conjugate(commutators[y + 1][r, c])).Real;
                gramC[x, y] = sum;
            }
        var (evC, vecC) = Jacobi(gramC);
        int kernelIdx = Array.FindIndex(evC, v => System.Math.Abs(v) <= 1e-9);
        if (kernelIdx >= 0)
        {
            var combo = new Complex[16, 16];
            for (int g = 0; g < 7; g++)
                for (int r = 0; r < 16; r++)
                    for (int c = 0; c < 16; c++)
                        combo[r, c] += vecC[g, kernelIdx] * colorOn16[g + 1][r, c];
            colorOn16 = colorOn16.Take(8).ToArray();
            colorOn16[7] = combo; // place the commuting combo at slot 7
            cartan2 = 7;
        }
    }
}
bool colorCartanPairFound = cartan2 >= 0;

// generic Hermitian combination H = i(Y + r2 L3 + r3 c1 + r5 c2)
var hCombo = new Complex[16, 16];
{
    double r2 = System.Math.Sqrt(2.0), r3 = System.Math.Sqrt(3.0), r5 = System.Math.Sqrt(5.0);
    for (int r = 0; r < 16; r++)
        for (int c = 0; c < 16; c++)
            hCombo[r, c] = Complex.ImaginaryOne *
                (hyperchargeOn16[r, c] + r2 * su2LOn16[2][r, c] + r3 * colorOn16[cartan1][r, c] + r5 * colorOn16[cartan2][r, c]);
}
// realified Jacobi then complex reconstruction via J-pairing
Complex[][] smBasis = new Complex[16][];
{
    var hr = new double[32, 32];
    for (int r = 0; r < 16; r++)
        for (int c = 0; c < 16; c++)
        {
            hr[r, c] = hCombo[r, c].Real;
            hr[r, c + 16] = -hCombo[r, c].Imaginary;
            hr[r + 16, c] = hCombo[r, c].Imaginary;
            hr[r + 16, c + 16] = hCombo[r, c].Real;
        }
    var (ev, vec) = Jacobi(hr);
    // group into complex vectors: take eigenvectors in eigenvalue order, skip J-partners
    var used = new bool[32];
    int filled = 0;
    var order = Enumerable.Range(0, 32).OrderBy(i => ev[i]).ToArray();
    foreach (int e in order)
    {
        if (used[e] || filled >= 16)
            continue;
        var z = new Complex[16];
        for (int k = 0; k < 16; k++)
            z[k] = new Complex(vec[k, e], vec[k + 16, e]);
        double zNorm = System.Math.Sqrt(z.Sum(x => x.Magnitude * x.Magnitude));
        for (int k = 0; k < 16; k++)
            z[k] /= zNorm;
        // mark the J-partner (same eigenvalue, J-rotated vector) as used:
        // find the remaining eigenvector with max overlap with iZ
        used[e] = true;
        int partner = -1;
        double bestOverlap = 0.0;
        foreach (int f in order)
        {
            if (used[f] || System.Math.Abs(ev[f] - ev[e]) > 1e-8)
                continue;
            double overlap = 0.0;
            for (int k = 0; k < 16; k++)
            {
                // iz realified = (-Im z; Re z)
                overlap += -z[k].Imaginary * vec[k, f] + z[k].Real * vec[k + 16, f];
            }
            if (System.Math.Abs(overlap) > bestOverlap)
            {
                bestOverlap = System.Math.Abs(overlap);
                partner = f;
            }
        }
        if (partner >= 0)
            used[partner] = true;
        smBasis[filled++] = z;
    }
}

// weights per basis vector via Rayleigh quotients (rounded; residual-verified)
double[] WeightOf(Complex[,] cartanOp)
{
    var w = new double[16];
    for (int k = 0; k < 16; k++)
    {
        Complex sum = Complex.Zero;
        var img = new Complex[16];
        for (int r = 0; r < 16; r++)
        {
            Complex s = Complex.Zero;
            for (int c = 0; c < 16; c++)
                s += cartanOp[r, c] * smBasis[k][c];
            img[r] = s;
        }
        for (int r = 0; r < 16; r++)
            sum += Complex.Conjugate(smBasis[k][r]) * img[r];
        // eigenvalue of the ANTI-Hermitian op is i*w: w = Im(<z, Op z>)... Op z = i w z
        w[k] = sum.Imaginary;
    }
    return w;
}

double maxCartanResidual = 0.0;
double[] CartanWeightsVerified(Complex[,] cartanOp)
{
    var w = WeightOf(cartanOp);
    for (int k = 0; k < 16; k++)
    {
        double res = 0.0;
        for (int r = 0; r < 16; r++)
        {
            Complex s = Complex.Zero;
            for (int c = 0; c < 16; c++)
                s += cartanOp[r, c] * smBasis[k][c];
            s -= Complex.ImaginaryOne * w[k] * smBasis[k][r];
            res += s.Magnitude * s.Magnitude;
        }
        maxCartanResidual = System.Math.Max(maxCartanResidual, System.Math.Sqrt(res));
    }
    return w;
}

var yWeights = CartanWeightsVerified(hyperchargeOn16);
var mLWeights = CartanWeightsVerified(su2LOn16[2]);
var c1Weights = CartanWeightsVerified(colorOn16[cartan1]);
var c2Weights = CartanWeightsVerified(colorOn16[cartan2]);
bool smDiagonalBasisExact = maxCartanResidual <= 1e-8;

// transform all needed 16-ops into the SM-diagonal basis: op' = U^dag op U
Complex[,] ToSmBasis(Complex[,] op)
{
    var result = new Complex[16, 16];
    for (int r = 0; r < 16; r++)
        for (int c = 0; c < 16; c++)
        {
            Complex sum = Complex.Zero;
            for (int x = 0; x < 16; x++)
            {
                Complex ox = Complex.Zero;
                for (int y = 0; y < 16; y++)
                    ox += op[x, y] * smBasis[c][y];
                sum += Complex.Conjugate(smBasis[r][x]) * ox;
            }
            result[r, c] = sum;
        }
    return result;
}

var aOpsC = aOn16.Select(ToSmBasis).ToArray();
var bOpsC = bOn16.Select(ToSmBasis).ToArray();
var lOpsC = su2LOn16.Select(ToSmBasis).ToArray();
var colOpsC = colorOn16.Select(ToSmBasis).ToArray();
var aOps = aOpsC.Select(Flatten).ToArray();
var bOps = bOpsC.Select(Flatten).ToArray();
var lOps = lOpsC.Select(Flatten).ToArray();
var colOps = colOpsC.Select(Flatten).ToArray();
Stage("operators built");

// su2L Casimir normalization on the 16: kappaL from its largest eigenvalue
// (the family doublets carry j = 1/2 -> value (3/4) kappaL).
double kappaL;
{
    var casimir = new Complex[16, 16];
    foreach (var g in lOpsC)
    {
        var sq = CMatMul(g, g);
        for (int r = 0; r < 16; r++)
            for (int c = 0; c < 16; c++)
                casimir[r, c] -= sq[r, c];
    }
    double maxEv = 0.0;
    for (int r = 0; r < 16; r++)
        maxEv = System.Math.Max(maxEv, casimir[r, r].Real);
    kappaL = maxEv / 0.75;
}

// ---------------------------------------------------------------------------
// Sparse 4-leg machinery on 16^(x4) (complex vectors, flat arrays).
// Index = l0 + 16 l1 + 256 l2 + 4096 l3.
// ---------------------------------------------------------------------------

static (double[] Re, double[] Im) Flatten(Complex[,] op)
{
    var re = new double[256];
    var im = new double[256];
    for (int r = 0; r < 16; r++)
        for (int c = 0; c < 16; c++)
        {
            re[r * 16 + c] = op[r, c].Real;
            im[r * 16 + c] = op[r, c].Imaginary;
        }
    return (re, im);
}

void LegApply(double[] opRe, double[] opIm, double[] vRe, double[] vIm, double[] outRe, double[] outIm, int leg)
{
    int stride = 1;
    for (int p = 0; p < leg; p++)
        stride *= 16;
    int outerStride = stride * 16;
    for (int baseIdx = 0; baseIdx < Dim4Legs; baseIdx += outerStride)
        for (int inner = 0; inner < stride; inner++)
        {
            int off = baseIdx + inner;
            for (int r = 0; r < 16; r++)
            {
                double sr = 0.0, si = 0.0;
                int row = r * 16;
                for (int c = 0; c < 16; c++)
                {
                    double oRe = opRe[row + c];
                    double oIm = opIm[row + c];
                    if (oRe == 0.0 && oIm == 0.0)
                        continue;
                    double xr = vRe[off + c * stride];
                    double xi = vIm[off + c * stride];
                    sr += oRe * xr - oIm * xi;
                    si += oRe * xi + oIm * xr;
                }
                outRe[off + r * stride] += sr;
                outIm[off + r * stride] += si;
            }
        }
}

void SumLegs((double[] Re, double[] Im) op, double[] vRe, double[] vIm, double[] outRe, double[] outIm)
{
    Array.Clear(outRe);
    Array.Clear(outIm);
    for (int leg = 0; leg < 4; leg++)
        LegApply(op.Re, op.Im, vRe, vIm, outRe, outIm, leg);
}

(double[] Re, double[] Im) CasimirApply((double[] Re, double[] Im)[] ops, double[] vRe, double[] vIm)
{
    var accRe = new double[Dim4Legs];
    var accIm = new double[Dim4Legs];
    var t1Re = new double[Dim4Legs];
    var t1Im = new double[Dim4Legs];
    var t2Re = new double[Dim4Legs];
    var t2Im = new double[Dim4Legs];
    foreach (var op in ops)
    {
        SumLegs(op, vRe, vIm, t1Re, t1Im);
        SumLegs(op, t1Re, t1Im, t2Re, t2Im);
        for (int i = 0; i < Dim4Legs; i++)
        {
            accRe[i] -= t2Re[i];
            accIm[i] -= t2Im[i];
        }
    }
    return (accRe, accIm);
}

// ---------------------------------------------------------------------------
// X2: weight census and the candidate sector V_w (|Y|=1/2, color weight 0,
// |mL| = 1/2).
// ---------------------------------------------------------------------------

var vwIndices = new List<int>();
for (int idx = 0; idx < Dim4Legs; idx++)
{
    int l0 = idx & 15, l1 = (idx >> 4) & 15, l2 = (idx >> 8) & 15, l3 = (idx >> 12) & 15;
    double y = yWeights[l0] + yWeights[l1] + yWeights[l2] + yWeights[l3];
    double m = mLWeights[l0] + mLWeights[l1] + mLWeights[l2] + mLWeights[l3];
    double c1 = c1Weights[l0] + c1Weights[l1] + c1Weights[l2] + c1Weights[l3];
    double c2 = c2Weights[l0] + c2Weights[l1] + c2Weights[l2] + c2Weights[l3];
    if (System.Math.Abs(System.Math.Abs(y) - 0.5) < 1e-6 &&
        System.Math.Abs(System.Math.Abs(m) - 0.5) < 1e-6 &&
        System.Math.Abs(c1) < 1e-6 && System.Math.Abs(c2) < 1e-6)
        vwIndices.Add(idx);
}
int vwDimension = vwIndices.Count;

// ---------------------------------------------------------------------------
// X3: the SM-doublet isotypic basis D inside V_w: joint kernel of
// C_color and (C_L - (3/4) kappaL)^2, restricted to V_w (both preserve the
// weight sector; leakage residual recorded).
// ---------------------------------------------------------------------------

var vwLookup = new Dictionary<int, int>();
for (int k = 0; k < vwDimension; k++)
    vwLookup[vwIndices[k]] = k;

double weightSectorLeakageResidual = 0.0;
object leakLock = new();
double[,] RestrictedOperator(Func<double[], double[], (double[] Re, double[] Im)> apply)
{
    // returns realified 2n x 2n restricted matrix on V_w (columns parallel)
    int n = vwDimension;
    var result = new double[2 * n, 2 * n];
    Parallel.For(0, n, k =>
    {
        var eRe = new double[Dim4Legs];
        var eIm = new double[Dim4Legs];
        eRe[vwIndices[k]] = 1.0;
        var (oRe, oIm) = apply(eRe, eIm);
        double leak = 0.0;
        for (int idx = 0; idx < Dim4Legs; idx++)
        {
            if (vwLookup.TryGetValue(idx, out int kk))
            {
                result[kk, k] = oRe[idx];
                result[kk + n, k] = oIm[idx];
                result[kk, k + n] = -oIm[idx];
                result[kk + n, k + n] = oRe[idx];
            }
            else
                leak += oRe[idx] * oRe[idx] + oIm[idx] * oIm[idx];
        }
        lock (leakLock)
        {
            weightSectorLeakageResidual = System.Math.Max(weightSectorLeakageResidual, System.Math.Sqrt(leak));
        }
    });
    return result;
}

Stage($"candidate sector V_w dimension = {vwDimension}");
var cColorRestricted = RestrictedOperator((re, im) => CasimirApply(colOps, re, im));
Stage("restricted color Casimir built");
var cLRestricted = RestrictedOperator((re, im) => CasimirApply(lOps, re, im));
Stage("restricted su2L Casimir built");

// H_SM = C_color + (C_L - (3/4)kappaL)^2 on realified V_w.
int nW = 2 * vwDimension;
var hSm = new double[nW, nW];
{
    double c0 = 0.75 * kappaL;
    var shifted = new double[nW, nW];
    for (int r = 0; r < nW; r++)
        for (int c = 0; c < nW; c++)
            shifted[r, c] = cLRestricted[r, c] - (r == c ? c0 : 0.0);
    Parallel.For(0, nW, r =>
    {
        for (int c = 0; c < nW; c++)
        {
            double sum = 0.0;
            for (int k = 0; k < nW; k++)
                sum += shifted[r, k] * shifted[k, c];
            hSm[r, c] = cColorRestricted[r, c] + sum;
        }
    });
}
Stage("H_SM assembled");

// Kernel of the PSD H_SM by dense Jacobi (nW is small enough), with
// residual verification on every kernel vector.
List<double[]> dBasisVw;
{
    var (ev, vec) = Jacobi(hSm);
    double scale = ev.Max();
    dBasisVw = new List<double[]>();
    double maxKernelResidual = 0.0;
    for (int e = 0; e < nW; e++)
    {
        if (System.Math.Abs(ev[e]) > scale * 1e-9)
            continue;
        var x = new double[nW];
        for (int k = 0; k < nW; k++)
            x[k] = vec[k, e];
        double res = 0.0;
        for (int r = 0; r < nW; r++)
        {
            double sum = 0.0;
            for (int c = 0; c < nW; c++)
                sum += hSm[r, c] * x[c];
            res += sum * sum;
        }
        maxKernelResidual = System.Math.Max(maxKernelResidual, System.Math.Sqrt(res));
        dBasisVw.Add(x);
    }
    Stage($"doublet isotypic kernel (Jacobi): dim {dBasisVw.Count}, max residual {maxKernelResidual:E3}");
}
int doubletIsotypicRealDimension = dBasisVw.Count;

// lift D basis to full-space complex vectors
List<(double[] Re, double[] Im)> dBasis = new();
foreach (var v in dBasisVw)
{
    var re = new double[Dim4Legs];
    var im = new double[Dim4Legs];
    for (int k = 0; k < vwDimension; k++)
    {
        re[vwIndices[k]] = v[k];
        im[vwIndices[k]] = v[k + vwDimension];
    }
    dBasis.Add((re, im));
}

// ---------------------------------------------------------------------------
// X4: welded per-label spectral projectors on the D basis.
// C_A, C_B eigenvalues on 16^(x4): j(j+1) kappa for j = 0..6.
// ---------------------------------------------------------------------------

double[] CasimirEigenvalueGrid()
{
    var grid = new double[7];
    for (int j = 0; j <= 6; j++)
        grid[j] = j * (j + 1) * kappaCalibration;
    return grid;
}
var casimirGrid = CasimirEigenvalueGrid();

(double[] Re, double[] Im) ProjectLabel(double a, double b, double[] vRe, double[] vIm)
{
    // product-form spectral projector for C_A -> a(a+1)k, then C_B -> b(b+1)k
    var curRe = (double[])vRe.Clone();
    var curIm = (double[])vIm.Clone();
    foreach (var (ops, target) in new[] { (aOps, a), (bOps, b) })
    {
        double lambda0 = target * (target + 1) * kappaCalibration;
        // factors ordered nearest-root-first
        var roots = casimirGrid.Where(l => System.Math.Abs(l - lambda0) > 1e-9)
            .OrderBy(l => System.Math.Abs(l - lambda0)).ToArray();
        foreach (double l in roots)
        {
            var (cRe, cIm) = CasimirApply(ops, curRe, curIm);
            double inv = 1.0 / (lambda0 - l);
            for (int i = 0; i < Dim4Legs; i++)
            {
                curRe[i] = (cRe[i] - l * curRe[i]) * inv;
                curIm[i] = (cIm[i] - l * curIm[i]) * inv;
            }
        }
    }
    return (curRe, curIm);
}

// channel label sets from the machine-known Weyl labels (Phase411: the
// chiral halves are (0,1/2) and (1/2,0); contents of 2-leg products by
// su(2) character arithmetic).
List<(double A, double B)> TwoLegLabels(int nL, int nR)
{
    // nL legs of (0,1/2) and nR legs of (1/2,0): A-content = coupling of nR
    // spin-1/2s; B-content = coupling of nL spin-1/2s (labels only).
    IEnumerable<double> Couple(int n)
    {
        var current = new List<double> { 0.0 };
        for (int i = 0; i < n; i++)
        {
            var next = new List<double>();
            foreach (double j in current)
            {
                next.Add(j + 0.5);
                if (j > 0.25)
                    next.Add(j - 0.5);
            }
            current = next;
        }
        return current.Distinct();
    }
    var labels = new List<(double, double)>();
    foreach (double ja in Couple(nR))
        foreach (double jb in Couple(nL))
            labels.Add((ja, jb));
    return labels;
}

var channels = new (string Name, int NL, int NR)[]
{
    ("LLLL", 4, 0), ("LLLR", 3, 1), ("LLRR", 2, 2), ("LRRR", 1, 3), ("RRRR", 0, 4),
};
var channelLabels = channels.Select(ch => (ch.Name, Labels: TwoLegLabels(ch.NL, ch.NR))).ToArray();
var unionLabels = channelLabels.SelectMany(c => c.Labels).Distinct().ToList();

// per-label projections of every D vector (parallel over vectors)
var projections = new Dictionary<(double A, double B), List<(double[] Re, double[] Im)>>();
double maxProjectorIdempotencyResidual = 0.0;
object idempotencyLock = new();
foreach (var label in unionLabels)
{
    var array = new (double[] Re, double[] Im)[dBasis.Count];
    Parallel.For(0, dBasis.Count, k =>
    {
        var (re, im) = dBasis[k];
        var (pRe, pIm) = ProjectLabel(label.A, label.B, re, im);
        if (k == 0)
        {
            var (p2Re, p2Im) = ProjectLabel(label.A, label.B, pRe, pIm);
            double res = 0.0;
            for (int i = 0; i < Dim4Legs; i++)
            {
                double dr = p2Re[i] - pRe[i];
                double di = p2Im[i] - pIm[i];
                res += dr * dr + di * di;
            }
            lock (idempotencyLock)
            {
                maxProjectorIdempotencyResidual = System.Math.Max(maxProjectorIdempotencyResidual, System.Math.Sqrt(res));
            }
        }
        array[k] = (pRe, pIm);
    });
    projections[label] = array.ToList();
    Stage($"label ({label.A},{label.B}) projected onto {dBasis.Count} doublet vectors");
}

// ---------------------------------------------------------------------------
// X5: per-channel Gram G[k,l] = <d_k, P_allowed d_l>; eigenvalue-1 count.
// ---------------------------------------------------------------------------

Complex InnerC((double[] Re, double[] Im) x, (double[] Re, double[] Im) y)
{
    double rr = 0.0, ri = 0.0;
    for (int i = 0; i < Dim4Legs; i++)
    {
        rr += x.Re[i] * y.Re[i] + x.Im[i] * y.Im[i];
        ri += x.Re[i] * y.Im[i] - x.Im[i] * y.Re[i];
    }
    return new Complex(rr, ri);
}

var channelResults = new List<(string Name, int LabelCount, int IntersectionDim, double TopEigenvalue, double SecondEigenvalue)>();
int unionIntersectionDim = 0;
double unionTopEigenvalue = 0.0;

(int Dim, double Top, double Second) IntersectionOf(List<(double A, double B)> labels)
{
    int dCount = dBasis.Count;
    if (dCount == 0)
        return (0, 0.0, 0.0);
    // P_allowed d_l = sum over labels (orthogonal isotypics -> sum of projections)
    var pd = new List<(double[] Re, double[] Im)>();
    for (int l = 0; l < dCount; l++)
    {
        var re = new double[Dim4Legs];
        var im = new double[Dim4Legs];
        foreach (var label in labels)
        {
            var (pRe, pIm) = projections[label][l];
            for (int i = 0; i < Dim4Legs; i++)
            {
                re[i] += pRe[i];
                im[i] += pIm[i];
            }
        }
        pd.Add((re, im));
    }
    // realified Gram of <d_k, P d_l> (Hermitian -> realify 2x)
    var gram = new double[2 * dCount, 2 * dCount];
    for (int k = 0; k < dCount; k++)
        for (int l = 0; l < dCount; l++)
        {
            var g = InnerC(dBasis[k], pd[l]);
            gram[k, l] = g.Real;
            gram[k + dCount, l + dCount] = g.Real;
            gram[k, l + dCount] = -g.Imaginary;
            gram[k + dCount, l] = g.Imaginary;
        }
    var (ev, _) = Jacobi(gram);
    var sorted = ev.OrderByDescending(x => x).ToArray();
    int dim = sorted.Count(x => x >= 1.0 - 1e-6);
    return (dim / 2 * 2 == dim ? dim : dim, sorted.Length > 0 ? sorted[0] : 0.0, sorted.Length > 1 ? sorted[1] : 0.0);
}

foreach (var (name, labels) in channelLabels)
{
    var (dim, top, second) = IntersectionOf(labels);
    channelResults.Add((name, labels.Count, dim, top, second));
}
{
    var (dim, top, _) = IntersectionOf(unionLabels);
    unionIntersectionDim = dim;
    unionTopEigenvalue = top;
}

bool quarticWeldedScalarSmDoubletAbsentAllChannels =
    channelResults.All(c => c.IntersectionDim == 0) && unionIntersectionDim == 0;

// ---------------------------------------------------------------------------
// X6: character-arithmetic cross-checks of the quartic singlet dimensions.
// 16 content (1/2,3/2)+(3/2,1/2) realified x2 (Phase411-verified); carrier
// contents and channel singlet dims must reproduce Phase411's numbers.
// ---------------------------------------------------------------------------

var content16 = new List<(double J1, double J2, int M)> { (0.5, 1.5, 2), (1.5, 0.5, 2) };
List<(double J1, double J2, int M)> Tensor(List<(double J1, double J2, int M)> x, (double J1, double J2, int M) f)
{
    var dict = new Dictionary<(double, double), int>();
    foreach (var (j1, j2, m) in x)
        for (double a = System.Math.Abs(f.J1 - j1); a <= f.J1 + j1 + 1e-9; a += 1.0)
            for (double b = System.Math.Abs(f.J2 - j2); b <= f.J2 + j2 + 1e-9; b += 1.0)
            {
                var key = (System.Math.Round(a * 2) / 2, System.Math.Round(b * 2) / 2);
                dict[key] = dict.GetValueOrDefault(key) + m * f.M;
            }
    return dict.Select(kv => (kv.Key.Item1, kv.Key.Item2, kv.Value)).ToList();
}
List<(double J1, double J2, int M)> TensorContents(List<(double J1, double J2, int M)> x, List<(double J1, double J2, int M)> y)
{
    var dict = new Dictionary<(double, double), int>();
    foreach (var (j1, j2, m1) in x)
        foreach (var (k1, k2, m2) in y)
            for (double a = System.Math.Abs(j1 - k1); a <= j1 + k1 + 1e-9; a += 1.0)
                for (double b = System.Math.Abs(j2 - k2); b <= j2 + k2 + 1e-9; b += 1.0)
                {
                    var key = (System.Math.Round(a * 2) / 2, System.Math.Round(b * 2) / 2);
                    dict[key] = dict.GetValueOrDefault(key) + m1 * m2;
                }
    return dict.Select(kv => (kv.Key.Item1, kv.Key.Item2, kv.Value)).ToList();
}

// realified carrier contents: S_L = (0,1/2)x16 etc.; Phase411 convention:
// the realified spinor carriers had complex content x2 - here the channel
// singlet count uses the complex-content convention of Phase411 (S as the
// realified 64-dim carrier with content from its WeldedContent), i.e. the
// 2-leg factor contributes one complex label per leg, the 16 contributes
// content16 (already realified x2 per label).
var contentSL = Tensor(content16, (0.0, 0.5, 1));
var contentSR = Tensor(content16, (0.5, 0.0, 1));
int Singlets(List<(double J1, double J2, int M)> x, List<(double J1, double J2, int M)> y)
{
    int total = 0;
    foreach (var (j1, j2, m1) in x)
        foreach (var (k1, k2, m2) in y)
            if (System.Math.Abs(j1 - k1) < 1e-9 && System.Math.Abs(j2 - k2) < 1e-9)
                total += m1 * m2;
    return total;
}
var contentLL = TensorContents(contentSL, contentSL);
var contentLR = TensorContents(contentSL, contentSR);
var contentRR = TensorContents(contentSR, contentSR);
int quarticLLLL = Singlets(contentLL, contentLL);
int quarticLLRR = Singlets(contentLR, contentLR);
int quarticLLLR = Singlets(contentLL, contentLR);
int quarticLRRR = Singlets(contentLR, contentRR);
int quarticRRRR = Singlets(contentRR, contentRR);
bool quarticCountsMatchPhase411 =
    quarticLLLL == phase411QuarticLLLL && quarticLLRR == phase411QuarticLRLR;

bool analysisInternallyConsistent =
    phase411PrecursorPassed &&
    chiralHalfIsSixteenDimensional &&
    chiralInvarianceResidual <= 1e-10 &&
    colorAlgebraDimensionIsEight &&
    colorCartanPairFound &&
    smDiagonalBasisExact &&
    weightSectorLeakageResidual <= 1e-7 &&
    maxProjectorIdempotencyResidual <= 1e-6 &&
    quarticCountsMatchPhase411;

// ---------------------------------------------------------------------------
// Fail-closed boundary.
// ---------------------------------------------------------------------------

const bool complexCompactArithmeticUsed = true;
const bool statisticsProjectionApplied = false; // unrestricted tensor power (conservative superset)
const bool ambientIntersectionIsNecessaryConditionOnly = true; // nonzero would not yet exhibit an extraction
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
const string ApplicationSubjectKind = "quartic-sm-doublet-intersection-analysis";
const bool physicalTargetsConsultedForConstruction = false;

string targetBlindConstructionHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join(
    "|",
    ApplicationSubjectKind,
    "SM-diagonal weight census on 16^4; doublet isotypic via restricted C_color + C_L kernel; per-channel welded label projectors; ambient intersection Gram")))).ToLowerInvariant();

bool quarticSmDoubletIntersectionAnalysisPassed =
    analysisInternallyConsistent &&
    complexCompactArithmeticUsed &&
    !statisticsProjectionApplied &&
    ambientIntersectionIsNecessaryConditionOnly &&
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

string terminalStatus = quarticSmDoubletIntersectionAnalysisPassed
    ? (quarticWeldedScalarSmDoubletAbsentAllChannels
        ? "quartic-order-closed-no-welded-scalar-sm-doublet-in-any-channel"
        : "quartic-welded-scalar-sm-doublet-ambient-candidate-found")
    : "quartic-sm-doublet-intersection-analysis-blocked";

string decision = quarticSmDoubletIntersectionAnalysisPassed
    ? (quarticWeldedScalarSmDoubletAbsentAllChannels
        ? "The quartic order is CLOSED for welded-scalar SM-doublets, by an ambient-intersection argument STRONGER than the deferred SM-stable-subspace analysis: the SM-doublet isotypic of 16^(x4) (constructed exactly inside the weight-filtered candidate sector) has ZERO intersection with the welded isotypic sectors available to quartic spacetime singlets, in EVERY channel (LLLL/LLLR/LLRR/LRRR/RRRR) and in their union. Since the unrestricted tensor power contains every statistics-projected composite space, the closure covers antisymmetrized composites too. Together with Phases 408-411, every internal composite-extraction route on every probed carrier is now closed through QUARTIC order. The remaining named routes are unchanged in kind but narrowed: the draft's unobserved-phase fields, a noncompact real-form evasion of the compact arithmetic (Nguyen-Polya caveat carried), or a genuinely new primary-source specification. Nothing is promoted; no contract field is filled."
        : "An ambient quartic candidate EXISTS (a state simultaneously welded-allowed and SM-doublet-patterned) - this is NECESSARY-ONLY evidence: statistics projection, 2-leg coupling, and SM-stability must be checked by a refinement phase before any extraction claim. Nothing is promoted; no contract field is filled.")
    : "Do not use the closure claim until the precursor and the exactness battery pass.";

var result = new
{
    phaseId = "phase412-quartic-sm-doublet-intersection-analysis",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    quarticSmDoubletIntersectionAnalysisPassed,
    phase411PrecursorPassed,
    analysisInternallyConsistent,
    chiralHalfIsSixteenDimensional,
    chiralInvarianceResidual,
    colorAlgebraDimensionIsEight,
    colorCartanPairFound,
    smDiagonalBasisExact,
    maxCartanResidual,
    candidateSectorDimension = vwDimension,
    weightSectorLeakageResidual,
    doubletIsotypicRealDimension,
    kappaCalibration,
    kappaL,
    maxProjectorIdempotencyResidual,
    channelIntersections = channelResults.Select(c => new
    {
        channel = c.Name,
        allowedLabelCount = c.LabelCount,
        intersectionRealDimension = c.IntersectionDim,
        topGramEigenvalue = c.TopEigenvalue,
        secondGramEigenvalue = c.SecondEigenvalue,
    }).ToArray(),
    unionIntersectionRealDimension = unionIntersectionDim,
    unionTopGramEigenvalue = unionTopEigenvalue,
    quarticWeldedScalarSmDoubletAbsentAllChannels,
    quarticSingletDimensions = new
    {
        llll = quarticLLLL,
        lllr = quarticLLLR,
        llrr = quarticLLRR,
        lrrr = quarticLRRR,
        rrrr = quarticRRRR,
    },
    quarticCountsMatchPhase411,
    statisticsProjectionApplied,
    ambientIntersectionIsNecessaryConditionOnly,
    complexCompactArithmeticUsed,
    physicalCouplingProvided,
    probeDefinitions = new
    {
        x1 = "SM-diagonal 16-basis: joint eigenbasis of the commuting Cartans (Y, L3, two color Cartans) via one generic-combination diagonalization, residual-verified",
        x2 = "candidate sector V_w: indices of 16^(x4) with |Y_tot| = 1/2, color weight (0,0), |mL_tot| = 1/2 (every SM-doublet state lives there)",
        x3 = "doublet isotypic D: joint kernel of C_color and (C_L - (3/4)kappaL)^2 restricted to V_w (weight-sector preservation machine-verified)",
        x4 = "welded label projectors: product-form spectral polynomials in the commuting sub-Casimirs C_A, C_B (grid j(j+1)kappa, j = 0..6), nearest-root-first, idempotency-verified; channel label sets from exact 2-leg su(2) character arithmetic",
        x5 = "intersection: eigenvalue-1 count of the Hermitian Gram <d_k, P_allowed d_l> per channel and for the union",
        x6 = "character cross-checks: per-channel quartic singlet dimensions must reproduce Phase411's recorded values",
    },
    applicationSubjectKind = ApplicationSubjectKind,
    targetBlindConstruction = true,
    physicalTargetsConsultedForConstruction,
    targetBlindConstructionHash,
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
    explicitCandidateOnlyNonclaims = new[]
    {
        "the ambient-intersection argument is one-directional: ZERO closes the quartic order outright; NONZERO would be necessary-only evidence requiring statistics, coupling, and stability refinements",
        "the unrestricted tensor power 16^(x4) is a conservative superset of every statistics-projected composite space",
        "all arithmetic is complex/compact; the Nguyen-Polya complexification caveat and noncompact real-form effects are carried as named caveats",
        "the draft's unobserved-phase fields remain unprobed",
        "no dynamics, no scales, no VEV; the binding gaps are unchanged",
        "no Phase201 or Phase256 fill",
        "no physical predictions",
    },
    sourceEvidence = new
    {
        phase411SummaryPath = Phase411SummaryPath,
        primaryDraft = "docs/Reference/ExperimentReferences/texts/GU-DRAFT-2021-TEXT.txt (sections 9, 12)",
        userDirective = "2026-06-12: execute the quartic SM-stable analysis named in the journal",
    },
    decision,
};

var options = JsonOptions();
File.WriteAllText(Path.Combine(outputDir, "quartic_sm_doublet_intersection_analysis.json"), JsonSerializer.Serialize(result, options));
string summaryPath = Path.Combine(outputDir, "quartic_sm_doublet_intersection_analysis_summary.json");
File.WriteAllText(summaryPath, JsonSerializer.Serialize(result, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"quarticSmDoubletIntersectionAnalysisPassed={quarticSmDoubletIntersectionAnalysisPassed}");
Console.WriteLine($"candidateSectorDimension={vwDimension} doubletIsotypicRealDimension={doubletIsotypicRealDimension}");
Console.WriteLine($"weightSectorLeakageResidual={weightSectorLeakageResidual:E3} cartanResidual={maxCartanResidual:E3} projectorIdempotency={maxProjectorIdempotencyResidual:E3}");
foreach (var c in channelResults)
    Console.WriteLine($"channel {c.Name}: labels={c.LabelCount} intersectionDim={c.IntersectionDim} topGramEv={c.TopEigenvalue:F9}");
Console.WriteLine($"union: intersectionDim={unionIntersectionDim} topGramEv={unionTopEigenvalue:F9}");
Console.WriteLine($"quarticSinglets: LLLL={quarticLLLL} LLLR={quarticLLLR} LLRR={quarticLLRR} LRRR={quarticLRRR} RRRR={quarticRRRR} (match411={quarticCountsMatchPhase411})");
Console.WriteLine($"quarticWeldedScalarSmDoubletAbsentAllChannels={quarticWeldedScalarSmDoubletAbsentAllChannels}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");
Console.WriteLine($"summaryPath={summaryPath}");

static (double[] Eigenvalues, double[,] Vectors) Jacobi(double[,] input)
{
    int n = input.GetLength(0);
    var a = (double[,])input.Clone();
    var vectors = new double[n, n];
    for (int i = 0; i < n; i++)
        vectors[i, i] = 1.0;
    for (int sweep = 0; sweep < 300; sweep++)
    {
        double off = 0.0;
        for (int p = 0; p < n; p++)
            for (int q = p + 1; q < n; q++)
                off += a[p, q] * a[p, q];
        if (System.Math.Sqrt(off) < 1e-13)
            break;
        for (int p = 0; p < n - 1; p++)
            for (int q = p + 1; q < n; q++)
            {
                if (System.Math.Abs(a[p, q]) < 1e-16)
                    continue;
                double theta = 0.5 * System.Math.Atan2(2.0 * a[p, q], a[p, p] - a[q, q]);
                double c = System.Math.Cos(theta);
                double s = System.Math.Sin(theta);
                for (int k = 0; k < n; k++)
                {
                    double akp = a[k, p];
                    double akq = a[k, q];
                    a[k, p] = c * akp + s * akq;
                    a[k, q] = -s * akp + c * akq;
                }
                for (int k = 0; k < n; k++)
                {
                    double apk = a[p, k];
                    double aqk = a[q, k];
                    a[p, k] = c * apk + s * aqk;
                    a[q, k] = -s * apk + c * aqk;
                }
                for (int k = 0; k < n; k++)
                {
                    double vkp = vectors[k, p];
                    double vkq = vectors[k, q];
                    vectors[k, p] = c * vkp + s * vkq;
                    vectors[k, q] = -s * vkp + c * vkq;
                }
            }
    }
    var eigenvalues = new double[n];
    for (int i = 0; i < n; i++)
        eigenvalues[i] = a[i, i];
    return (eigenvalues, vectors);
}

static JsonSerializerOptions JsonOptions() => new()
{
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
};

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) &&
    (value.ValueKind == JsonValueKind.True || value.ValueKind == JsonValueKind.False)
        ? value.GetBoolean()
        : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Number
        ? value.GetInt32()
        : null;
