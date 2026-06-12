using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

// Phase409: invariant-pairing-menu spin-zero extraction probe.
//
// Phase408 obstructed the NAIVE vertical-trace spin-0 extraction: the
// so(4)-singlet slot of the welded vertical 10 is 1-dimensional, too small
// for the 4-real-dimensional SM doublet. The named open route was the
// draft's epsilon-conjugation/Shiab machinery. This probe extends the
// obstruction analysis from the single trace pairing to the COMPLETE
// machine-enumerated invariant-pairing menu on the chain, including the
// parity-odd (Levi-Civita epsilon-built) sector, at linear and bilinear
// order in the frame-cross content:
//
//   V = the frame-cross block 4 (x) 10 with the WELD-TWISTED so(4) action
//       D(M) = M (x) I + I (x) pi(M) (pi = the Phase408 Sym^2 weld), and
//       the SM chain acting on the internal factor only, S(G) = I (x) G.
//       Phase407 located the SM-Higgs quantum numbers in exactly this
//       block; Phase408 proved spin and isospin are entangled there.
//
//   M1. PAIRING MENU: the exact dimension of the so(4)-invariant bilinear
//       pairings on each fiber pair (4x4, 10x10, 4x10), each invariant
//       classified by PARITY under the orientation-reversing reflection
//       R = diag(1,1,1,-1) (parity-odd = built with the Levi-Civita
//       epsilon; parity-even = metric-built). This is the repository's
//       own, source-independent answer to "how many invariant fiber
//       pairings exist" - external pairing-count claims are NOT used.
//
//   M2. LINEAR ORDER: the so(4)-singlet content of V itself. If zero, NO
//       invariant linear extraction (epsilon-built or not) can produce
//       ANY spin-0 scalar from the frame-cross block - strengthening
//       Phase408 V3 from the trace slot to every linear map.
//
//   M3. BILINEAR ORDER: the complete spin-0 subspace S of V (x) V,
//       constructed exactly (blockwise Schur kernels over the Casimir
//       eigenspaces of V, ALL ordered block pairs enumerated, every
//       constructed invariant re-verified by direct annihilation under
//       all six welded generators). Then the largest SM-stable subspace
//       U inside S (iterated stabilization under the full SM chain:
//       su(2)_L, su(2)_R Cartan, hypercharge, and the FULL color su(3)
//       built as the traceless centralizer of J in so(6)), decomposed
//       under the SM Casimirs. THE QUESTION: does any spin-0 bilinear
//       composite carry the SM-doublet quantum numbers (color-singlet,
//       j_L = 1/2, |Y| = 1/2)? Each spin-0 invariant is also classified
//       by parity, so an epsilon-sector doublet would be seen as such.
//
//   M4. TRILINEAR BOUND: the spin-0 dimension of V (x) V (x) V counted
//       exactly by su(2) x su(2) character arithmetic (integer triangle
//       rules over the machine-derived irrep content of V). The SM
//       decomposition at trilinear order is NOT computed here and is
//       recorded as the named remaining order (trilinear composites are
//       the lowest order at which half-integer SM isospin can in
//       principle ride on a spin-0 composite if the bilinear order
//       carries none).
//
// Fail-closed: exact representation arithmetic only; compact-form caveat
// as in Phases 404/407/408; composite (pairing-built) objects are NOT the
// draft's fundamental scalar - this probe characterizes the extraction
// menu, it does not construct a Higgs; no dynamics, no scales; nothing
// promoted; no contract field is filled.

const string DefaultOutputDir = "studies/phase409_invariant_pairing_menu_spin_zero_extraction_probe_001/output";
const string Phase408SummaryPath = "studies/phase408_vertical_spin_zero_extraction_obstruction_probe_001/output/vertical_spin_zero_extraction_obstruction_probe_summary.json";

const double Tolerance = 1e-10;
const double KernelEigenTolerance = 1e-9;

var outputDir = Environment.GetEnvironmentVariable("PHASE409_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var p408 = JsonDocument.Parse(File.ReadAllText(Phase408SummaryPath));
bool phase408PrecursorPassed =
    JsonBool(p408.RootElement, "verticalSpinZeroExtractionObstructionProbePassed") is true &&
    JsonBool(p408.RootElement, "spinZeroSlotCannotCarryFullDoublet") is true &&
    JsonBool(p408.RootElement, "weldEntanglesSpinAndIsospin") is true;

// ---------------------------------------------------------------------------
// The weld pi: so(4) -> so(10) on Sym^2(R^4) (Phase408 construction, exact).
// ---------------------------------------------------------------------------

var symBasis = new List<(int A, int B)>();
for (int a = 0; a < 4; a++)
    symBasis.Add((a, a));
for (int a = 0; a < 4; a++)
    for (int b = a + 1; b < 4; b++)
        symBasis.Add((a, b));
int symDim = symBasis.Count; // 10

var so4Pairs = new List<(int I, int J)>();
for (int i = 0; i < 4; i++)
    for (int j = i + 1; j < 4; j++)
        so4Pairs.Add((i, j));

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
    // rho(X): S -> X S - S X in the orthonormal trace basis.
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

double[,] SymGroupEmbed(double[,] g)
{
    // Group-level action on Sym^2: S -> g S g^T in the orthonormal basis.
    var rho = new double[symDim, symDim];
    for (int col = 0; col < symDim; col++)
    {
        var acted = new double[4, 4];
        for (int r = 0; r < 4; r++)
            for (int c = 0; c < 4; c++)
            {
                double sum = 0.0;
                for (int k = 0; k < 4; k++)
                    for (int l = 0; l < 4; l++)
                        sum += g[r, k] * symMats[col][k, l] * g[c, l];
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

var so4Vector = so4Pairs.Select(p => M4(p.I, p.J)).ToArray();          // on the 4
var piGenerators = so4Vector.Select(SymEmbedX).ToArray();              // on the 10

// ---------------------------------------------------------------------------
// V = 4 (x) 10 (40-dim). Index v = a*10 + m. Welded spin action
// D(M) = M (x) I + I (x) pi(M); SM action S(G) = I (x) G.
// ---------------------------------------------------------------------------

const int VDim = 40;

double[,] WeldAction(int g)
{
    var d = new double[VDim, VDim];
    for (int a = 0; a < 4; a++)
        for (int ap = 0; ap < 4; ap++)
            if (System.Math.Abs(so4Vector[g][a, ap]) > 1e-15)
                for (int m = 0; m < 10; m++)
                    d[a * 10 + m, ap * 10 + m] += so4Vector[g][a, ap];
    for (int m = 0; m < 10; m++)
        for (int mp = 0; mp < 10; mp++)
            if (System.Math.Abs(piGenerators[g][m, mp]) > 1e-15)
                for (int a = 0; a < 4; a++)
                    d[a * 10 + m, a * 10 + mp] += piGenerators[g][m, mp];
    return d;
}

var weldGenerators = Enumerable.Range(0, 6).Select(WeldAction).ToArray();

double[,] SmActionOnV(double[,] g10)
{
    var s = new double[VDim, VDim];
    for (int m = 0; m < 10; m++)
        for (int mp = 0; mp < 10; mp++)
            if (System.Math.Abs(g10[m, mp]) > 1e-15)
                for (int a = 0; a < 4; a++)
                    s[a * 10 + m, a * 10 + mp] = g10[m, mp];
    return s;
}

// ---------------------------------------------------------------------------
// SM chain on the internal 10 (Phase404/408 scaffolding) + FULL color su(3)
// as the traceless centralizer of J inside so(6).
// ---------------------------------------------------------------------------

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
var su2R3 = ScaleM(AddM(V10(6, 7), ScaleM(V10(8, 9), -1.0)), 0.5);
var hypercharge = AddM(su2R3, ScaleM(jComplex, 1.0 / 3.0));

// Color su(3): centralizer of J in so(6) (kernel of X -> [X, J]), minus the
// J direction itself.
var so6Pairs = new List<(int I, int J)>();
for (int i = 0; i < 6; i++)
    for (int j = i + 1; j < 6; j++)
        so6Pairs.Add((i, j));
var colorGenerators = new List<double[,]>();
{
    int n6 = so6Pairs.Count; // 15
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
    // J's coefficient vector in the so(6) pair basis:
    var jCoefficients = new double[n6];
    for (int k = 0; k < n6; k++)
        jCoefficients[k] = jComplex[so6Pairs[k].I, so6Pairs[k].J];
    double jNorm = System.Math.Sqrt(jCoefficients.Sum(x => x * x));
    for (int k = 0; k < n6; k++)
        jCoefficients[k] /= jNorm;
    for (int e = 0; e < n6; e++)
    {
        if (System.Math.Abs(ev6[e]) > KernelEigenTolerance)
            continue;
        var coefficient = new double[n6];
        for (int k = 0; k < n6; k++)
            coefficient[k] = vec6[k, e];
        // remove the J component
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
        // Gram-Schmidt against already accepted color generators
        foreach (var prev in colorGenerators)
        {
            double ip = 0.0;
            for (int r = 0; r < 10; r++)
                for (int c = 0; c < 10; c++)
                    ip += gen[r, c] * prev[r, c];
            for (int r = 0; r < 10; r++)
                for (int c = 0; c < 10; c++)
                    gen[r, c] -= ip * prev[r, c] / 2.0; // prev normalized to Frob^2 = 2
        }
        double fro = 0.0;
        for (int r = 0; r < 10; r++)
            for (int c = 0; c < 10; c++)
                fro += gen[r, c] * gen[r, c];
        if (fro < 1e-10)
            continue;
        double scale = System.Math.Sqrt(2.0 / fro); // normalize Frob^2 = 2 like V10(i,j)
        for (int r = 0; r < 10; r++)
            for (int c = 0; c < 10; c++)
                gen[r, c] *= scale;
        colorGenerators.Add(gen);
    }
}
int colorGeneratorCount = colorGenerators.Count; // expect 8
bool colorAlgebraDimensionIsEight = colorGeneratorCount == 8;

var smChain = new List<(string Name, double[,] G10)>
{
    ("su2L_1", su2LGen[0]), ("su2L_2", su2LGen[1]), ("su2L_3", su2LGen[2]),
    ("su2R_3", su2R3), ("hypercharge", hypercharge),
};
for (int c = 0; c < colorGenerators.Count; c++)
    smChain.Add(($"color_{c + 1}", colorGenerators[c]));
var smOnV = smChain.Select(g => (g.Name, Matrix: SmActionOnV(g.G10))).ToArray();

// ---------------------------------------------------------------------------
// M1: the invariant pairing menu on the fibers, with parity classification.
// Invariant bilinears on A (x) B = kernel of the stacked generator action
// rhoA(M) (x) I + I (x) rhoB(M) on A (x) B (a bilinear form is an invariant
// VECTOR of the tensor square because the reps are orthogonal, hence
// self-dual). Parity: eigenvalue of the reflection R = diag(1,1,1,-1).
// ---------------------------------------------------------------------------

var reflection4 = new double[4, 4];
reflection4[0, 0] = 1.0;
reflection4[1, 1] = 1.0;
reflection4[2, 2] = 1.0;
reflection4[3, 3] = -1.0;
var reflection10 = SymGroupEmbed(reflection4);

(int Dim, int EvenCount, int OddCount) PairingMenu(double[][,] rhoA, double[][,] rhoB, double[,] parityA, double[,] parityB)
{
    int da = rhoA[0].GetLength(0);
    int db = rhoB[0].GetLength(0);
    int dt = da * db;
    var gram = new double[dt, dt];
    for (int g = 0; g < 6; g++)
    {
        var act = new double[dt, dt];
        for (int i = 0; i < da; i++)
            for (int ip = 0; ip < da; ip++)
                if (System.Math.Abs(rhoA[g][i, ip]) > 1e-15)
                    for (int j = 0; j < db; j++)
                        act[i * db + j, ip * db + j] += rhoA[g][i, ip];
        for (int j = 0; j < db; j++)
            for (int jp = 0; jp < db; jp++)
                if (System.Math.Abs(rhoB[g][j, jp]) > 1e-15)
                    for (int i = 0; i < da; i++)
                        act[i * db + j, i * db + jp] += rhoB[g][j, jp];
        for (int a = 0; a < dt; a++)
            for (int b = 0; b < dt; b++)
            {
                double sum = 0.0;
                for (int r = 0; r < dt; r++)
                    sum += act[r, a] * act[r, b];
                gram[a, b] += sum;
            }
    }
    var (ev, vec) = Jacobi(gram);
    var kernelVectors = new List<double[]>();
    for (int e = 0; e < dt; e++)
        if (System.Math.Abs(ev[e]) <= KernelEigenTolerance)
        {
            var v = new double[dt];
            for (int k = 0; k < dt; k++)
                v[k] = vec[k, e];
            kernelVectors.Add(v);
        }
    // parity operator on the tensor product
    int even = 0, odd = 0;
    if (kernelVectors.Count > 0)
    {
        var parityOp = new double[dt, dt];
        for (int i = 0; i < da; i++)
            for (int ip = 0; ip < da; ip++)
                if (System.Math.Abs(parityA[i, ip]) > 1e-15)
                    for (int j = 0; j < db; j++)
                        for (int jp = 0; jp < db; jp++)
                            if (System.Math.Abs(parityB[j, jp]) > 1e-15)
                                parityOp[i * db + j, ip * db + jp] = parityA[i, ip] * parityB[j, jp];
        // restrict to the kernel (parity preserves it) and diagonalize
        int s = kernelVectors.Count;
        var restricted = new double[s, s];
        for (int a = 0; a < s; a++)
        {
            var image = new double[dt];
            for (int r = 0; r < dt; r++)
            {
                double sum = 0.0;
                for (int c = 0; c < dt; c++)
                    sum += parityOp[r, c] * kernelVectors[a][c];
                image[r] = sum;
            }
            for (int b = 0; b < s; b++)
            {
                double ip2 = 0.0;
                for (int r = 0; r < dt; r++)
                    ip2 += image[r] * kernelVectors[b][r];
                restricted[b, a] = ip2;
            }
        }
        var sym = new double[s, s];
        for (int a = 0; a < s; a++)
            for (int b = 0; b < s; b++)
                sym[a, b] = 0.5 * (restricted[a, b] + restricted[b, a]);
        var (pev, _) = Jacobi(sym);
        even = pev.Count(v => v > 0.0);
        odd = pev.Count(v => v < 0.0);
    }
    return (kernelVectors.Count, even, odd);
}

var menu44 = PairingMenu(so4Vector, so4Vector, reflection4, reflection4);
var menu1010 = PairingMenu(piGenerators, piGenerators, reflection10, reflection10);
var menu410 = PairingMenu(so4Vector, piGenerators, reflection4, reflection10);

// ---------------------------------------------------------------------------
// M2: linear order - the so(4)-singlet content of V itself via the welded
// Casimir on V (40 x 40).
// ---------------------------------------------------------------------------

var vCasimir = new double[VDim, VDim];
foreach (var d in weldGenerators)
    for (int i = 0; i < VDim; i++)
        for (int j = 0; j < VDim; j++)
        {
            double sum = 0.0;
            for (int k = 0; k < VDim; k++)
                sum += d[i, k] * d[k, j];
            vCasimir[i, j] -= sum;
        }
var (vCasimirEv, vCasimirVec) = Jacobi(vCasimir);
int linearSingletDimension = vCasimirEv.Count(v => System.Math.Abs(v) <= KernelEigenTolerance);
bool linearSpinZeroContentIsZero = linearSingletDimension == 0;

// Casimir eigenspace blocks of V (for the blockwise bilinear construction).
var blockEigenvalues = new List<double>();
var blockBases = new List<List<double[]>>();
{
    var sortedIdx = Enumerable.Range(0, VDim).OrderBy(i => vCasimirEv[i]).ToArray();
    foreach (int e in sortedIdx)
    {
        var v = new double[VDim];
        for (int k = 0; k < VDim; k++)
            v[k] = vCasimirVec[k, e];
        int found = -1;
        for (int b = 0; b < blockEigenvalues.Count; b++)
            if (System.Math.Abs(blockEigenvalues[b] - vCasimirEv[e]) <= 1e-7)
            {
                found = b;
                break;
            }
        if (found < 0)
        {
            blockEigenvalues.Add(vCasimirEv[e]);
            blockBases.Add(new List<double[]> { v });
        }
        else
            blockBases[found].Add(v);
    }
}
int blockCount = blockEigenvalues.Count;
var blockDims = blockBases.Select(b => b.Count).ToArray();

// ---------------------------------------------------------------------------
// M3: bilinear order. Spin-0 subspace S of W = V (x) V, blockwise.
// For each ORDERED block pair (X, Y): restrict each welded generator to the
// blocks, build the tensor-sum action on X (x) Y, take the exact kernel.
// (Schur: only equal-Casimir pairs can contribute - the machine checks ALL
// pairs anyway and records the cross-pair kernels.)
// Spin-0 vectors are stored as 40 x 40 coefficient matrices T (the W vector
// is T[v1, v2] on basis e_{v1} (x) e_{v2}).
// ---------------------------------------------------------------------------

double[,] RestrictTo(double[,] op, List<double[]> basis)
{
    int s = basis.Count;
    var result = new double[s, s];
    for (int b = 0; b < s; b++)
    {
        var image = new double[VDim];
        for (int r = 0; r < VDim; r++)
        {
            double sum = 0.0;
            for (int c = 0; c < VDim; c++)
                sum += op[r, c] * basis[b][c];
            image[r] = sum;
        }
        for (int a = 0; a < s; a++)
        {
            double ip = 0.0;
            for (int r = 0; r < VDim; r++)
                ip += image[r] * basis[a][r];
            result[a, b] = ip;
        }
    }
    return result;
}

var spinZeroBasis = new List<double[,]>(); // each a 40x40 matrix in W
var crossBlockKernelDims = new List<(int BlockA, int BlockB, int KernelDim)>();
for (int ba = 0; ba < blockCount; ba++)
    for (int bb = 0; bb < blockCount; bb++)
    {
        int da = blockDims[ba], db = blockDims[bb];
        int dt = da * db;
        var gram = new double[dt, dt];
        for (int g = 0; g < 6; g++)
        {
            var ra = RestrictTo(weldGenerators[g], blockBases[ba]);
            var rb = RestrictTo(weldGenerators[g], blockBases[bb]);
            var act = new double[dt, dt];
            for (int i = 0; i < da; i++)
                for (int ip = 0; ip < da; ip++)
                    if (System.Math.Abs(ra[i, ip]) > 1e-15)
                        for (int j = 0; j < db; j++)
                            act[i * db + j, ip * db + j] += ra[i, ip];
            for (int j = 0; j < db; j++)
                for (int jp = 0; jp < db; jp++)
                    if (System.Math.Abs(rb[j, jp]) > 1e-15)
                        for (int i = 0; i < da; i++)
                            act[i * db + j, i * db + jp] += rb[j, jp];
            for (int a = 0; a < dt; a++)
                for (int b = 0; b < dt; b++)
                {
                    double sum = 0.0;
                    for (int r = 0; r < dt; r++)
                        sum += act[r, a] * act[r, b];
                    gram[a, b] += sum;
                }
        }
        var (ev, vec) = Jacobi(gram);
        int kernelDim = 0;
        for (int e = 0; e < dt; e++)
        {
            if (System.Math.Abs(ev[e]) > KernelEigenTolerance)
                continue;
            kernelDim++;
            // lift to W: T = sum_{ij} coeff[i*db+j] u_i v_j^T
            var t = new double[VDim, VDim];
            for (int i = 0; i < da; i++)
                for (int j = 0; j < db; j++)
                {
                    double coefficient = vec[i * db + j, e];
                    if (System.Math.Abs(coefficient) < 1e-15)
                        continue;
                    for (int r = 0; r < VDim; r++)
                        for (int c = 0; c < VDim; c++)
                            t[r, c] += coefficient * blockBases[ba][i][r] * blockBases[bb][j][c];
                }
            spinZeroBasis.Add(t);
        }
        if (ba != bb && kernelDim > 0)
            crossBlockKernelDims.Add((ba, bb, kernelDim));
    }
int bilinearSpinZeroDimension = spinZeroBasis.Count;
bool crossBlocksContributeNothing = crossBlockKernelDims.Count == 0;

// Direct re-verification: every constructed invariant is annihilated by all
// six welded generators acting on W (D2 T = D T + T D^T).
static double[,] WAction(double[,] d, double[,] t)
{
    int n = t.GetLength(0);
    var result = new double[n, n];
    for (int r = 0; r < n; r++)
        for (int c = 0; c < n; c++)
        {
            double sum = 0.0;
            for (int k = 0; k < n; k++)
                sum += d[r, k] * t[k, c] + t[r, k] * d[c, k];
            result[r, c] = sum;
        }
    return result;
}

static double WNorm(double[,] t)
{
    double sum = 0.0;
    int n = t.GetLength(0);
    for (int r = 0; r < n; r++)
        for (int c = 0; c < n; c++)
            sum += t[r, c] * t[r, c];
    return System.Math.Sqrt(sum);
}

static double WInner(double[,] a, double[,] b)
{
    double sum = 0.0;
    int n = a.GetLength(0);
    for (int r = 0; r < n; r++)
        for (int c = 0; c < n; c++)
            sum += a[r, c] * b[r, c];
    return sum;
}

double maxInvariantAnnihilationResidual = 0.0;
foreach (var t in spinZeroBasis)
    foreach (var d in weldGenerators)
        maxInvariantAnnihilationResidual = System.Math.Max(maxInvariantAnnihilationResidual, WNorm(WAction(d, t)));
bool allInvariantsExactlyAnnihilated = maxInvariantAnnihilationResidual <= Tolerance;

// Orthonormalize S (Gram-Schmidt in the W inner product).
var sOrtho = new List<double[,]>();
foreach (var t in spinZeroBasis)
{
    var v = (double[,])t.Clone();
    foreach (var u in sOrtho)
    {
        double ip = WInner(v, u);
        for (int r = 0; r < VDim; r++)
            for (int c = 0; c < VDim; c++)
                v[r, c] -= ip * u[r, c];
    }
    double norm = WNorm(v);
    if (norm < 1e-9)
        continue;
    for (int r = 0; r < VDim; r++)
        for (int c = 0; c < VDim; c++)
            v[r, c] /= norm;
    sOrtho.Add(v);
}
bool spinZeroBasisIndependent = sOrtho.Count == bilinearSpinZeroDimension;

// Parity classification of S: rho_W(R) T = P T P^T with P = R (x) Sym^2(R).
var parityOnV = new double[VDim, VDim];
for (int a = 0; a < 4; a++)
    for (int ap = 0; ap < 4; ap++)
        if (System.Math.Abs(reflection4[a, ap]) > 1e-15)
            for (int m = 0; m < 10; m++)
                for (int mp = 0; mp < 10; mp++)
                    if (System.Math.Abs(reflection10[m, mp]) > 1e-15)
                        parityOnV[a * 10 + m, ap * 10 + mp] = reflection4[a, ap] * reflection10[m, mp];

double[,] ParityOnW(double[,] t)
{
    int n = t.GetLength(0);
    var temp = new double[n, n];
    for (int r = 0; r < n; r++)
        for (int c = 0; c < n; c++)
        {
            double sum = 0.0;
            for (int k = 0; k < n; k++)
                sum += parityOnV[r, k] * t[k, c];
            temp[r, c] = sum;
        }
    var result = new double[n, n];
    for (int r = 0; r < n; r++)
        for (int c = 0; c < n; c++)
        {
            double sum = 0.0;
            for (int k = 0; k < n; k++)
                sum += temp[r, k] * parityOnV[c, k];
            result[r, c] = sum;
        }
    return result;
}

int parityEvenInvariantCount = 0, parityOddInvariantCount = 0;
double[]? parityEigenvalues = null;
{
    int s = sOrtho.Count;
    var restricted = new double[s, s];
    for (int b = 0; b < s; b++)
    {
        var image = ParityOnW(sOrtho[b]);
        for (int a = 0; a < s; a++)
            restricted[a, b] = WInner(image, sOrtho[a]);
    }
    var sym = new double[s, s];
    for (int a = 0; a < s; a++)
        for (int b = 0; b < s; b++)
            sym[a, b] = 0.5 * (restricted[a, b] + restricted[b, a]);
    var (pev, _) = Jacobi(sym);
    parityEigenvalues = pev;
    parityEvenInvariantCount = pev.Count(v => v > 0.0);
    parityOddInvariantCount = pev.Count(v => v < 0.0);
}

// ---------------------------------------------------------------------------
// M3b: the largest SM-stable subspace U of S, then the SM decomposition.
// S2(G) T = A T + T A^T with A = S(G) on V. Iterate: keep the subspace of
// the CURRENT U whose SM images stay inside U, until stable.
// ---------------------------------------------------------------------------

var uBasis = new List<double[,]>(sOrtho);
int stabilizationIterations = 0;
while (true)
{
    stabilizationIterations++;
    int s = uBasis.Count;
    if (s == 0)
        break;
    // residual rows: for each generator and basis vector, the component of
    // the image orthogonal to span(U)
    var residuals = new List<double[,]>();
    foreach (var (_, a) in smOnV)
        foreach (var w in uBasis)
        {
            var image = WAction(a, w);
            foreach (var u in uBasis)
            {
                double ip = WInner(image, u);
                for (int r = 0; r < VDim; r++)
                    for (int c = 0; c < VDim; c++)
                        image[r, c] -= ip * u[r, c];
            }
            residuals.Add(image);
        }
    // constraint Gram on coefficients x: sum_k x_k residual_{g,k} = 0.
    // Build G[a,b] = sum_g <residual_{g,a}, residual_{g,b}>.
    int gCount = smOnV.Length;
    var gramU = new double[s, s];
    for (int g = 0; g < gCount; g++)
        for (int a = 0; a < s; a++)
            for (int b = 0; b < s; b++)
                gramU[a, b] += WInner(residuals[g * s + a], residuals[g * s + b]);
    var (evU, vecU) = Jacobi(gramU);
    var newBasis = new List<double[,]>();
    for (int e = 0; e < s; e++)
    {
        if (System.Math.Abs(evU[e]) > KernelEigenTolerance)
            continue;
        var t = new double[VDim, VDim];
        for (int k = 0; k < s; k++)
        {
            double coefficient = vecU[k, e];
            if (System.Math.Abs(coefficient) < 1e-15)
                continue;
            for (int r = 0; r < VDim; r++)
                for (int c = 0; c < VDim; c++)
                    t[r, c] += coefficient * uBasis[k][r, c];
        }
        newBasis.Add(t);
    }
    // re-orthonormalize
    var ortho = new List<double[,]>();
    foreach (var t in newBasis)
    {
        var v = (double[,])t.Clone();
        foreach (var u in ortho)
        {
            double ip = WInner(v, u);
            for (int r = 0; r < VDim; r++)
                for (int c = 0; c < VDim; c++)
                    v[r, c] -= ip * u[r, c];
        }
        double norm = WNorm(v);
        if (norm < 1e-9)
            continue;
        for (int r = 0; r < VDim; r++)
            for (int c = 0; c < VDim; c++)
                v[r, c] /= norm;
        ortho.Add(v);
    }
    if (ortho.Count == uBasis.Count)
    {
        uBasis = ortho;
        break;
    }
    uBasis = ortho;
    if (stabilizationIterations > 40)
        break;
}
int smStableSubspaceDimension = uBasis.Count;

// SM decomposition of U: su(2)_L Casimir, hypercharge squared, color Casimir.
double[] OperatorSpectrumOnU(IEnumerable<double[,]> generators10)
{
    int s = uBasis.Count;
    if (s == 0)
        return Array.Empty<double>();
    var casimir = new double[s, s];
    foreach (var g10 in generators10)
    {
        var a = SmActionOnV(g10);
        // C += -(S2)^2 restricted to U
        for (int k = 0; k < s; k++)
        {
            var once = WAction(a, uBasis[k]);
            var twice = WAction(a, once);
            for (int l = 0; l < s; l++)
                casimir[l, k] -= WInner(twice, uBasis[l]);
        }
    }
    var sym = new double[s, s];
    for (int a2 = 0; a2 < s; a2++)
        for (int b2 = 0; b2 < s; b2++)
            sym[a2, b2] = 0.5 * (casimir[a2, b2] + casimir[b2, a2]);
    var (ev, _) = Jacobi(sym);
    return ev;
}

var su2LCasimirSpectrum = OperatorSpectrumOnU(su2LGen);
var hyperchargeSquaredSpectrum = OperatorSpectrumOnU(new[] { hypercharge });
var colorCasimirSpectrum = OperatorSpectrumOnU(colorGenerators);

// Calibration: the same Casimirs on the bare internal 10 (known content:
// PS 4-block carries j_L = 1/2 -> the j = 1/2 eigenvalue is read off the
// machine, not assumed).
double[] CalibrationSpectrum(IEnumerable<double[,]> generators10)
{
    var casimir = new double[10, 10];
    foreach (var g in generators10)
        for (int i = 0; i < 10; i++)
            for (int j = 0; j < 10; j++)
            {
                double sum = 0.0;
                for (int k = 0; k < 10; k++)
                    sum += g[i, k] * g[k, j];
                casimir[i, j] -= sum;
            }
    var (ev, _) = Jacobi(casimir);
    return ev;
}
var su2LCalibration = CalibrationSpectrum(su2LGen);
double doubletCasimirValue = su2LCalibration.Max(); // the PS block's j_L = 1/2 value
var hyperchargeCalibration = CalibrationSpectrum(new[] { hypercharge });
double doubletHyperchargeSquared = hyperchargeCalibration.Where(v => v > 0.1).DefaultIfEmpty(0.0).Min();
// (on the 10 the PS block carries |Y| = 1/2 -> smallest positive Y^2 value)

int doubletPatternStateCount = 0;
if (smStableSubspaceDimension > 0)
{
    for (int k = 0; k < smStableSubspaceDimension; k++)
    {
        bool jHalf = System.Math.Abs(su2LCasimirSpectrum[k] - doubletCasimirValue) <= 1e-6;
        bool yHalf = System.Math.Abs(hyperchargeSquaredSpectrum[k] - doubletHyperchargeSquared) <= 1e-6;
        bool colorSinglet = System.Math.Abs(colorCasimirSpectrum[k]) <= 1e-6;
        if (jHalf && yHalf && colorSinglet)
            doubletPatternStateCount++;
    }
}
bool bilinearSpinZeroDoubletAbsent = doubletPatternStateCount == 0;

// ---------------------------------------------------------------------------
// M4: irrep labels of V recovered EXACTLY from the two commuting su(2)
// sub-Casimirs of the welded so(4) = su(2)_A + su(2)_B
// (A_i = (J_i + K_i)/2, B_i = (J_i - K_i)/2 with J = rotations, K = boosts).
// Joint (C_A, C_B) eigenspace dims / (2j1+1)(2j2+1) give the multiplicities;
// the normalization kappa is machine-calibrated on the bare vector 4.
// ---------------------------------------------------------------------------

int PairIndex(int i, int j) => so4Pairs.FindIndex(p => p.I == System.Math.Min(i, j) && p.J == System.Math.Max(i, j));

// J_1 = M_23, J_2 = M_31 = -M_13, J_3 = M_12; K_i = M_0i.
var rotationCoefficients = new[]
{
    new[] { (PairIndex(2, 3), 1.0) },
    new[] { (PairIndex(1, 3), -1.0) },
    new[] { (PairIndex(1, 2), 1.0) },
};
var boostCoefficients = new[]
{
    new[] { (PairIndex(0, 1), 1.0) },
    new[] { (PairIndex(0, 2), 1.0) },
    new[] { (PairIndex(0, 3), 1.0) },
};

double[,] CombineWeld(IEnumerable<(int Index, double Coefficient)> terms, double scale)
{
    var result = new double[VDim, VDim];
    foreach (var (index, coefficient) in terms)
        for (int r = 0; r < VDim; r++)
            for (int c = 0; c < VDim; c++)
                result[r, c] += scale * coefficient * weldGenerators[index][r, c];
    return result;
}

double[,] Combine4(IEnumerable<(int Index, double Coefficient)> terms, double scale)
{
    var result = new double[4, 4];
    foreach (var (index, coefficient) in terms)
        for (int r = 0; r < 4; r++)
            for (int c = 0; c < 4; c++)
                result[r, c] += scale * coefficient * so4Vector[index][r, c];
    return result;
}

var aGeneratorsWeld = new double[3][,];
var bGeneratorsWeld = new double[3][,];
var aGenerators4 = new double[3][,];
for (int i = 0; i < 3; i++)
{
    var combined = rotationCoefficients[i].Concat(boostCoefficients[i]).ToArray();
    var diff = rotationCoefficients[i].Concat(boostCoefficients[i].Select(t => (t.Item1, -t.Item2))).ToArray();
    aGeneratorsWeld[i] = CombineWeld(combined, 0.5);
    bGeneratorsWeld[i] = CombineWeld(diff, 0.5);
    aGenerators4[i] = Combine4(combined, 0.5);
}

double[,] CasimirOf(double[][,] generators, int dim)
{
    var casimir = new double[dim, dim];
    foreach (var g in generators)
        for (int i = 0; i < dim; i++)
            for (int j = 0; j < dim; j++)
            {
                double sum = 0.0;
                for (int k = 0; k < dim; k++)
                    sum += g[i, k] * g[k, j];
                casimir[i, j] -= sum;
            }
    return casimir;
}

// kappa calibration: C_A on the bare 4 must be kappa * (1/2)(3/2) * I.
double kappaCalibration;
{
    var c4 = CasimirOf(aGenerators4, 4);
    kappaCalibration = c4[0, 0] / 0.75;
}

var cA = CasimirOf(aGeneratorsWeld, VDim);
var cB = CasimirOf(bGeneratorsWeld, VDim);
var (cAEv, cAVec) = Jacobi(cA);
// diagonalize C_B inside each C_A eigenspace (they commute)
var vIrreps = new List<(double J1, double J2)>();
bool irrepLabelsRecovered = true;
{
    var aGroups = new Dictionary<long, List<int>>();
    for (int e = 0; e < VDim; e++)
    {
        long key = (long)System.Math.Round(cAEv[e] / kappaCalibration * 4.0); // 4*j(j+1) integer
        if (!aGroups.TryGetValue(key, out var list))
            aGroups[key] = list = new List<int>();
        list.Add(e);
    }
    foreach (var (aKey, indices) in aGroups.OrderBy(kv => kv.Key))
    {
        double j1 = (-1.0 + System.Math.Sqrt(1.0 + aKey)) / 2.0; // from 4j(j+1) = aKey
        // restrict C_B to this eigenspace
        int s = indices.Count;
        var basis = new List<double[]>();
        foreach (int e in indices)
        {
            var v = new double[VDim];
            for (int k = 0; k < VDim; k++)
                v[k] = cAVec[k, e];
            basis.Add(v);
        }
        var cbR = RestrictTo(cB, basis);
        var (cbEv, _) = Jacobi(cbR);
        var bGroups = new Dictionary<long, int>();
        foreach (double ev in cbEv)
        {
            long key = (long)System.Math.Round(ev / kappaCalibration * 4.0);
            bGroups[key] = bGroups.GetValueOrDefault(key) + 1;
        }
        foreach (var (bKey, count) in bGroups.OrderBy(kv => kv.Key))
        {
            double j2 = (-1.0 + System.Math.Sqrt(1.0 + bKey)) / 2.0;
            int irrepDim = (int)System.Math.Round((2 * j1 + 1) * (2 * j2 + 1));
            if (count % irrepDim != 0)
            {
                irrepLabelsRecovered = false;
                continue;
            }
            for (int m = 0; m < count / irrepDim; m++)
                vIrreps.Add((j1, j2));
        }
    }
    if (System.Math.Abs(vIrreps.Sum(l => (2 * l.J1 + 1) * (2 * l.J2 + 1)) - VDim) > 1e-9)
        irrepLabelsRecovered = false;
}
bool allVIrrepsHalfIntegerPairs = vIrreps.Count > 0 && vIrreps.All(l =>
    System.Math.Abs(l.J1 - System.Math.Floor(l.J1) - 0.5) <= 1e-9 &&
    System.Math.Abs(l.J2 - System.Math.Floor(l.J2) - 0.5) <= 1e-9);

static int TriangleSingletCount(double a, double b, double c) =>
    (c >= System.Math.Abs(a - b) - 1e-9 && c <= a + b + 1e-9 &&
     System.Math.Abs((a + b + c) - System.Math.Round(a + b + c)) <= 1e-9)
        ? 1
        : 0;

// bilinear character check: singlets in V (x) V = sum over pairs of
// [j1 triangle with k1 to 0] = delta(j1,k1) delta(j2,k2)
int bilinearCharacterCount = 0;
foreach (var x in vIrreps)
    foreach (var y in vIrreps)
        if (System.Math.Abs(x.J1 - y.J1) <= 1e-9 && System.Math.Abs(x.J2 - y.J2) <= 1e-9)
            bilinearCharacterCount++;
bool bilinearCharacterCountMatches = irrepLabelsRecovered && bilinearCharacterCount == bilinearSpinZeroDimension;

int trilinearSpinZeroDimension = 0;
foreach (var x in vIrreps)
    foreach (var y in vIrreps)
        foreach (var z in vIrreps)
            trilinearSpinZeroDimension += TriangleSingletCount(x.J1, y.J1, z.J1) * TriangleSingletCount(x.J2, y.J2, z.J2);

// ---------------------------------------------------------------------------
// Verdicts.
// ---------------------------------------------------------------------------

bool probeInternallyConsistent =
    phase408PrecursorPassed &&
    colorAlgebraDimensionIsEight &&
    linearSpinZeroContentIsZero &&
    crossBlocksContributeNothing &&
    allInvariantsExactlyAnnihilated &&
    spinZeroBasisIndependent &&
    parityEvenInvariantCount + parityOddInvariantCount == bilinearSpinZeroDimension &&
    irrepLabelsRecovered &&
    bilinearCharacterCountMatches &&
    (!allVIrrepsHalfIntegerPairs || trilinearSpinZeroDimension == 0);

bool obstructionMenuCompleteThroughBilinearOrder =
    linearSpinZeroContentIsZero && bilinearSpinZeroDoubletAbsent;

// ---------------------------------------------------------------------------
// Fail-closed boundary.
// ---------------------------------------------------------------------------

const bool compactFormUsedForNoncompactAlgebra = true;
const bool trilinearSmDecompositionEvaluated = false; // named remaining order
const bool draftEpsilonConjugationSpecificationUsed = false; // none exists in the primary
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
const string ApplicationSubjectKind = "invariant-pairing-menu-spin-zero-extraction-probe";
const bool physicalTargetsConsultedForConstruction = false;

string targetBlindConstructionHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join(
    "|",
    ApplicationSubjectKind,
    "weld-twisted V=4x10; fiber pairing menu with parity; linear singlet content; blockwise bilinear spin-0 basis; SM-stable subspace + Casimir decomposition; trilinear character count")))).ToLowerInvariant();

bool invariantPairingMenuSpinZeroExtractionProbePassed =
    probeInternallyConsistent &&
    compactFormUsedForNoncompactAlgebra &&
    !trilinearSmDecompositionEvaluated &&
    !draftEpsilonConjugationSpecificationUsed &&
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

string terminalStatus = invariantPairingMenuSpinZeroExtractionProbePassed
    ? (obstructionMenuCompleteThroughBilinearOrder
        ? "invariant-pairing-menu-enumerated-spin-zero-doublet-absent-through-bilinear-order"
        : "invariant-pairing-menu-enumerated-spin-zero-doublet-route-found")
    : "invariant-pairing-menu-spin-zero-extraction-probe-blocked";

string decision = invariantPairingMenuSpinZeroExtractionProbePassed
    ? (obstructionMenuCompleteThroughBilinearOrder
        ? "The Phase408 obstruction is extended from the single trace pairing to the COMPLETE invariant-pairing menu through bilinear order. (M1) The fiber pairing menu is machine-enumerated with parity labels - the epsilon-built (parity-odd) sector is now explicitly included, not just named. (M2) The frame-cross block 4x10 has ZERO so(4)-singlet content under the welded action: NO invariant linear extraction of any parity can produce a spin-0 scalar from it. (M3) The complete bilinear spin-0 sector is constructed exactly and its largest SM-stable subspace decomposed under the full SM chain (including full color su(3)): NO state carries the SM-doublet pattern (color-singlet, j_L = 1/2, |Y| = 1/2) in EITHER parity sector - the epsilon route at bilinear order is closed along with the metric route. (M4) STRONGER THAN PLANNED: every welded irrep of the frame-cross block is a half-integer x half-integer pair, so ALL ODD-ORDER composites have zero spin-0 content (the trilinear count is exactly 0 by character arithmetic, machine-cross-checked) - the next open order is QUARTIC, whose SM decomposition is the named remaining order. The extraction question now stands at: quartic-or-higher even composites, the draft's unspecified epsilon-conjugation/Shiab machinery acting on content BEYOND the frame-cross block (e.g. spinorial or unobserved-phase fields), or a different welded carrier. Nothing is promoted; no contract field is filled."
        : "A spin-0 SM-doublet candidate EXISTS in the bilinear invariant sector - the parity/SM data identify where; this opens an internal extraction route that the next phase must characterize before any contract claim. Nothing is promoted; no contract field is filled.")
    : "Do not use the menu characterization until the precursor and the full enumeration battery pass.";

var result = new
{
    phaseId = "phase409-invariant-pairing-menu-spin-zero-extraction-probe",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    invariantPairingMenuSpinZeroExtractionProbePassed,
    phase408PrecursorPassed,
    probeInternallyConsistent,
    colorGeneratorCount,
    colorAlgebraDimensionIsEight,
    pairingMenu = new
    {
        fiber4x4 = new { dimension = menu44.Dim, parityEven = menu44.EvenCount, parityOdd = menu44.OddCount },
        fiber10x10 = new { dimension = menu1010.Dim, parityEven = menu1010.EvenCount, parityOdd = menu1010.OddCount },
        fiber4x10 = new { dimension = menu410.Dim, parityEven = menu410.EvenCount, parityOdd = menu410.OddCount },
        note = "machine-enumerated on this chain; external pairing-count claims were not used",
    },
    linearSingletDimension,
    linearSpinZeroContentIsZero,
    casimirBlockCount = blockCount,
    casimirBlockDimensions = blockDims,
    irrepLabelsRecovered,
    vIrrepContent = vIrreps.Select(l => new { j1 = l.J1, j2 = l.J2 }).ToArray(),
    allVIrrepsHalfIntegerPairs,
    oddOrderSpinZeroForbidden = allVIrrepsHalfIntegerPairs,
    bilinearSpinZeroDimension,
    bilinearCharacterCount,
    bilinearCharacterCountMatches,
    crossBlocksContributeNothing,
    crossBlockKernels = crossBlockKernelDims.Select(x => new { blockA = x.BlockA, blockB = x.BlockB, kernelDim = x.KernelDim }).ToArray(),
    maxInvariantAnnihilationResidual,
    allInvariantsExactlyAnnihilated,
    spinZeroBasisIndependent,
    parityEvenInvariantCount,
    parityOddInvariantCount,
    parityEigenvalues,
    smStableSubspaceDimension,
    stabilizationIterations,
    su2LCasimirSpectrumOnSmStableSubspace = su2LCasimirSpectrum,
    hyperchargeSquaredSpectrumOnSmStableSubspace = hyperchargeSquaredSpectrum,
    colorCasimirSpectrumOnSmStableSubspace = colorCasimirSpectrum,
    doubletCasimirCalibration = doubletCasimirValue,
    doubletHyperchargeSquaredCalibration = doubletHyperchargeSquared,
    doubletPatternStateCount,
    bilinearSpinZeroDoubletAbsent,
    obstructionMenuCompleteThroughBilinearOrder,
    trilinearSpinZeroDimension,
    trilinearSmDecompositionEvaluated,
    draftEpsilonConjugationSpecificationUsed,
    compactFormUsedForNoncompactAlgebra,
    physicalCouplingProvided,
    probeDefinitions = new
    {
        v = "the frame-cross block 4 (x) 10 with the weld-twisted so(4) action D(M) = M (x) I + I (x) pi(M) (pi = the Phase408 Sym^2 weld); the SM chain acts on the internal factor only",
        m1 = "exact kernel of the stacked tensor-sum so(4) action on each fiber pair, parity-split by the reflection R = diag(1,1,1,-1) (parity-odd = Levi-Civita epsilon-built)",
        m2 = "so(4)-singlet content of V from the welded Casimir kernel (linear extractions of every parity)",
        m3 = "complete spin-0 subspace of V (x) V from blockwise Schur kernels over ALL ordered Casimir-eigenspace pairs; every invariant re-verified by direct annihilation; largest SM-stable subspace by iterated stabilization under su(2)_L + su(2)_R Cartan + hypercharge + full color su(3); decomposition by machine-calibrated Casimir spectra",
        m4 = "trilinear singlet dimension by su(2) x su(2) triangle-rule character arithmetic over the machine-recovered irrep content of V",
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
        "pairing-built composites are NOT the draft's fundamental scalar; this probe characterizes the extraction menu, it does not construct a Higgs",
        "odd-order composites are CLOSED exactly (all welded irreps are half-integer pairs); the QUARTIC-and-higher even-order SM decomposition is NOT computed and is the named remaining order",
        "the draft's epsilon-conjugation/Shiab machinery may act on content beyond the frame-cross block (e.g. spinorial or unobserved-phase fields); that route remains open and unspecified quantitatively in the primary",
        "compact-form arithmetic (so(10)/so(4)) stands in for the noncompact draft structures; real-form effects recorded",
        "external pairing-count claims (e.g. relayed 'seven pairings' summaries) were NOT used; the menu here is machine-enumerated on this chain",
        "no dynamics, no scales, no VEV; the binding gaps are unchanged",
        "no Phase201 or Phase256 fill",
        "no physical predictions",
    },
    sourceEvidence = new
    {
        phase408SummaryPath = Phase408SummaryPath,
        primaryDraft = "docs/Reference/ExperimentReferences/texts/GU-DRAFT-2021-TEXT.txt (sections 2.3, 9, 12.6)",
        sweepContext = "docs/BOSON_PREDICTION_DIAGNOSIS_JOURNAL.md 2026-06-12 entries (unverified external pairing-count claims; this probe is source-independent)",
    },
    decision,
};

var options = JsonOptions();
File.WriteAllText(Path.Combine(outputDir, "invariant_pairing_menu_spin_zero_extraction_probe.json"), JsonSerializer.Serialize(result, options));
string summaryPath = Path.Combine(outputDir, "invariant_pairing_menu_spin_zero_extraction_probe_summary.json");
File.WriteAllText(summaryPath, JsonSerializer.Serialize(result, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"invariantPairingMenuSpinZeroExtractionProbePassed={invariantPairingMenuSpinZeroExtractionProbePassed}");
Console.WriteLine($"pairingMenu 4x4={menu44.Dim}(even {menu44.EvenCount}/odd {menu44.OddCount}) 10x10={menu1010.Dim}(even {menu1010.EvenCount}/odd {menu1010.OddCount}) 4x10={menu410.Dim}");
Console.WriteLine($"linearSingletDimension={linearSingletDimension} (linear extraction impossible={linearSpinZeroContentIsZero})");
Console.WriteLine($"casimirBlocks=[{string.Join(",", blockDims)}] irreps=[{string.Join(",", vIrreps.Select(l => $"({l.J1},{l.J2})"))}]");
Console.WriteLine($"bilinearSpinZeroDimension={bilinearSpinZeroDimension} characterCount={bilinearCharacterCount} match={bilinearCharacterCountMatches}");
Console.WriteLine($"parity even/odd={parityEvenInvariantCount}/{parityOddInvariantCount} annihilationResidual={maxInvariantAnnihilationResidual:E3}");
Console.WriteLine($"smStableSubspaceDimension={smStableSubspaceDimension} (iterations={stabilizationIterations})");
Console.WriteLine($"doubletPatternStateCount={doubletPatternStateCount} bilinearSpinZeroDoubletAbsent={bilinearSpinZeroDoubletAbsent}");
Console.WriteLine($"obstructionMenuCompleteThroughBilinearOrder={obstructionMenuCompleteThroughBilinearOrder}");
Console.WriteLine($"trilinearSpinZeroDimension={trilinearSpinZeroDimension} oddOrderSpinZeroForbidden={allVIrrepsHalfIntegerPairs} (quartic = named remaining order)");
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
