using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

// Phase411: quartic / Dirac-squared spinor-sector composite probe.
//
// CONVERGING MOTIVATION: (1) Phase409 proved the frame-cross block cannot
// carry a spin-0 SM doublet at any order up to bilinear and that quartic is
// the lowest open composite order; (2) Phase410 closed the uniform bosonic
// realization of the curvature-coaxing VEV claim and pointed at the
// Dirac-sector realization ("a VEV in a Dirac-like operator" coupling "two
// chiral halves"); (3) the primary-heuristic statement (relayed via
// DEEP-RESEARCH-20260612, dating plausible-unverified) that the "quartic
// Higgs piece" arises from "Dirac squaring of a quadratic". All three point
// at SPINOR-SECTOR composites: can a spinor bilinear (the quadratic) carry
// a welded-spin-0 SM doublet, so that its Dirac square supplies the quartic
// Higgs invariant?
//
// THE CARRIER: the chimeric spinor factorizes as S(4) (x) S(10) (draft
// section 9; S_g(C) = S(V) (x) S(H*)). The spacetime Weyl halves are
// 2_L = (1/2,0) and 2_R = (0,1/2); the internal factor is the so(10)
// chiral spinor 16 (Phase404 Clifford construction). Under the chimeric
// weld pi: so(4) -> so(10) (Phase408 Sym^2 embedding), spacetime rotations
// act on BOTH factors: D(M) = sigma(M) (x) I + I (x) Sigma(pi(M)) where
// Sigma is the so(10) spinor representation. The SM chain acts on the 16
// factor only.
//
// BATTERIES (exact complex arithmetic, realified for kernel computations):
//
//   W1. The welded branching of the internal 16 under pi(so(4)): joint
//       sub-Casimir spectra recover the (j1, j2) irrep content exactly.
//
//   W2. The welded content of the chiral carrier spaces S_L = 2_L (x) 16
//       and S_R = 2_R (x) 16, by direct Casimir spectra AND by character
//       arithmetic from W1 (must agree).
//
//   W3. THE BILINEAR CHANNEL THEOREM: the welded-singlet (spin-0) content
//       of the three spinor-bilinear channels S_L (x) S_R (the Dirac
//       mass/Yukawa channel - exactly where the SM Higgs couples),
//       S_L (x) S_L and S_R (x) S_R (Majorana-type channels), by character
//       arithmetic AND by direct blockwise kernel construction (must
//       agree). A channel with zero welded-singlet content cannot carry
//       ANY welded-scalar composite, SM-doublet or otherwise.
//
//   W4. For each channel with nonzero spin-0 content: construct the exact
//       spin-0 basis, find the largest SM-stable subspace (iterated
//       stabilization under su(2)_L, su(2)_R Cartan, hypercharge, full
//       color su(3), all in the spinor representation), decompose under
//       machine-calibrated SM Casimirs, and count SM-doublet-pattern
//       states (color-singlet, j_L = 1/2, |Y| = 1/2).
//
//   W5. QUARTIC COUNT: the welded-singlet dimension of the Dirac-squared
//       (bilinear x bilinear) composites by character arithmetic over the
//       machine-derived channel contents; the quartic SM-stable analysis
//       is bounded out as the named remaining order if W4 yields nothing.
//
// NAMED CAVEATS: complex/compact arithmetic stands in for the noncompact
// draft structures (the Nguyen-Polya complexification objection - that the
// original Shiab presentation requires a complexification step - is
// carried as a named caveat on ANY construction found here); composite
// (bilinear-built) objects are NOT the draft's fundamental scalar; parity
// (Pin-group) classification of spinor composites is NOT attempted (the
// orientation-reversal lift to spinors has sign subtleties this probe does
// not adjudicate).
//
// Fail-closed: exact representation arithmetic only; no dynamics, no
// scales; nothing promoted; no contract field is filled.

const string DefaultOutputDir = "studies/phase411_quartic_dirac_squared_spinor_composite_probe_001/output";
const string Phase409SummaryPath = "studies/phase409_invariant_pairing_menu_spin_zero_extraction_probe_001/output/invariant_pairing_menu_spin_zero_extraction_probe_summary.json";
const string Phase410SummaryPath = "studies/phase410_curvature_coupled_vev_selection_probe_001/output/curvature_coupled_vev_selection_probe_summary.json";

const double Tolerance = 1e-9;

var outputDir = Environment.GetEnvironmentVariable("PHASE411_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var p409 = JsonDocument.Parse(File.ReadAllText(Phase409SummaryPath));
using var p410 = JsonDocument.Parse(File.ReadAllText(Phase410SummaryPath));
bool phase409PrecursorPassed =
    JsonBool(p409.RootElement, "invariantPairingMenuSpinZeroExtractionProbePassed") is true &&
    JsonBool(p409.RootElement, "obstructionMenuCompleteThroughBilinearOrder") is true;
bool phase410PrecursorPassed =
    JsonBool(p410.RootElement, "curvatureCoupledVevSelectionProbePassed") is true &&
    JsonBool(p410.RootElement, "curvatureCouplingFailsToSelectDoublet") is true;

// ---------------------------------------------------------------------------
// Cl(10) on 32 complex dimensions (Phase404 construction).
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
    int k2 = a.GetLength(1);
    var result = new Complex[n, m];
    for (int r = 0; r < n; r++)
        for (int c = 0; c < m; c++)
        {
            Complex sum = Complex.Zero;
            for (int k = 0; k < k2; k++)
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

// Chirality and an orthonormal basis of the +1 chiral 16.
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
// The tensor-string chirality is diagonal in this construction; collect the
// +1 eigen-axes as the 16 basis (verified diagonal).
double chiralityOffDiagonal = 0.0;
for (int r = 0; r < 32; r++)
    for (int c = 0; c < 32; c++)
        if (r != c)
            chiralityOffDiagonal = System.Math.Max(chiralityOffDiagonal, chirality[r, c].Magnitude);
var chiralAxes = new List<int>();
for (int r = 0; r < 32; r++)
    if ((chirality[r, r] - Complex.One).Magnitude < 1e-10)
        chiralAxes.Add(r);
bool chiralHalfIsSixteenDimensional = chiralityOffDiagonal < 1e-12 && chiralAxes.Count == 16;

Complex[,] RestrictTo16(Complex[,] op32)
{
    var result = new Complex[16, 16];
    for (int r = 0; r < 16; r++)
        for (int c = 0; c < 16; c++)
            result[r, c] = op32[chiralAxes[r], chiralAxes[c]];
    return result;
}

// Verify the 16 is invariant (off-block residual) for a sample generator.
double chiralInvarianceResidual = 0.0;

// ---------------------------------------------------------------------------
// so(10) elements: the weld pi (Phase408 Sym^2 embedding) and the SM chain.
// ---------------------------------------------------------------------------

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
var su2R3 = ScaleM(AddM(V10(6, 7), ScaleM(V10(8, 9), -1.0)), 0.5);
var hypercharge = AddM(su2R3, ScaleM(jComplex, 1.0 / 3.0));

// Full color su(3): traceless centralizer of J in so(6) (Phase409 method).
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

// Spinor (16x16 complex) representations.
Complex[,] Sigma16(double[,] so10Element)
{
    var full = SpinorRep32(so10Element);
    // off-block residual against the chiral split (sampled into the global)
    double offBlock = 0.0;
    var complement = Enumerable.Range(0, 32).Where(r => !chiralAxes.Contains(r)).ToArray();
    foreach (int r in complement)
        foreach (int c in chiralAxes)
            offBlock = System.Math.Max(offBlock, full[r, c].Magnitude);
    chiralInvarianceResidual = System.Math.Max(chiralInvarianceResidual, offBlock);
    return RestrictTo16(full);
}

var piOn16 = piGenerators.Select(Sigma16).ToArray();
var su2LOn16 = su2LGen.Select(Sigma16).ToArray();
var su2R3On16 = Sigma16(su2R3);
var hyperchargeOn16 = Sigma16(hypercharge);
var colorOn16 = colorGenerators.Select(Sigma16).ToArray();

// ---------------------------------------------------------------------------
// Welded chiral carriers S_L = 2_L (x) 16, S_R = 2_R (x) 16 (32 complex).
// Spacetime so(4) = su(2)_A + su(2)_B with A_i = (J_i + K_i)/2,
// B_i = (J_i - K_i)/2; on 2_L the A's act as -i sigma_i / 2 and the B's act
// as zero (and vice versa on 2_R).
// ---------------------------------------------------------------------------

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

// Spacetime Weyl halves built from Cl(4) exactly like the Cl(10)
// construction (homomorphism guaranteed by the Clifford relations):
// gamma4_{2k} = Z^k X I^(1-k), gamma4_{2k+1} = Z^k Y I^(1-k) on 2 qubits;
// sigma4(M) = sum c_ij [g_i, g_j]/4; chirality4 = phase * g0 g1 g2 g3
// (diagonal in this basis); the +1 half is CALLED 2_L and the -1 half 2_R
// (which su(2) factor each carries is machine-discovered, not assumed).
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
    gamma4[2 * k] = TensorString(codesEven);
    gamma4[2 * k + 1] = TensorString(codesOdd);
}

Complex[,] Sigma4(double[,] so4Element)
{
    var result = new Complex[4, 4];
    for (int i = 0; i < 4; i++)
        for (int j = i + 1; j < 4; j++)
        {
            double coefficient = so4Element[i, j];
            if (System.Math.Abs(coefficient) < 1e-15)
                continue;
            var gij = CMatMul(gamma4[i], gamma4[j]);
            var gji = CMatMul(gamma4[j], gamma4[i]);
            for (int r = 0; r < 4; r++)
                for (int c = 0; c < 4; c++)
                    result[r, c] += coefficient * (gij[r, c] - gji[r, c]) / 4.0;
        }
    return result;
}

Complex[,] chirality4;
{
    var product = gamma4[0];
    for (int i = 1; i < 4; i++)
        product = CMatMul(product, gamma4[i]);
    var square = CMatMul(product, product);
    Complex phase = Complex.Sqrt(1.0 / square[0, 0]);
    chirality4 = new Complex[4, 4];
    for (int r = 0; r < 4; r++)
        for (int c = 0; c < 4; c++)
            chirality4[r, c] = phase * product[r, c];
}
double chirality4OffDiagonal = 0.0;
for (int r = 0; r < 4; r++)
    for (int c = 0; c < 4; c++)
        if (r != c)
            chirality4OffDiagonal = System.Math.Max(chirality4OffDiagonal, chirality4[r, c].Magnitude);
var axesL = new List<int>();
var axesR = new List<int>();
for (int r = 0; r < 4; r++)
{
    if ((chirality4[r, r] - Complex.One).Magnitude < 1e-10)
        axesL.Add(r);
    else
        axesR.Add(r);
}
bool weylHalvesAreTwoDimensional = chirality4OffDiagonal < 1e-12 && axesL.Count == 2 && axesR.Count == 2;

// Verify sigma4 is an so(4) homomorphism (sampled over all generator pairs).
double sigma4HomResidual = 0.0;
for (int x = 0; x < 6; x++)
    for (int y = x + 1; y < 6; y++)
    {
        var mx = M4(so4Pairs[x].I, so4Pairs[x].J);
        var my = M4(so4Pairs[y].I, so4Pairs[y].J);
        var lhs = Sigma4(MatComm(mx, my));
        var sx = Sigma4(mx);
        var sy = Sigma4(my);
        var rhs = CMatMul(sx, sy);
        var rhs2 = CMatMul(sy, sx);
        for (int r = 0; r < 4; r++)
            for (int c = 0; c < 4; c++)
                sigma4HomResidual = System.Math.Max(sigma4HomResidual, (lhs[r, c] - (rhs[r, c] - rhs2[r, c])).Magnitude);
    }

Complex[,] SpacetimeOn2(int g, bool left)
{
    var full = Sigma4(M4(so4Pairs[g].I, so4Pairs[g].J));
    var axes = left ? axesL : axesR;
    var result = new Complex[2, 2];
    for (int r = 0; r < 2; r++)
        for (int c = 0; c < 2; c++)
            result[r, c] = full[axes[r], axes[c]];
    return result;
}

Complex[,] WeldOnCarrier(int g, bool left)
{
    // D(M_g) = sigma(M_g) (x) I_16 + I_2 (x) Sigma(pi(M_g)) on 2 (x) 16.
    var sigma = SpacetimeOn2(g, left);
    var result = new Complex[32, 32];
    for (int a = 0; a < 2; a++)
        for (int b = 0; b < 2; b++)
            if (sigma[a, b].Magnitude > 1e-15)
                for (int m = 0; m < 16; m++)
                    result[a * 16 + m, b * 16 + m] += sigma[a, b];
    for (int m = 0; m < 16; m++)
        for (int mp = 0; mp < 16; mp++)
            if (piOn16[g][m, mp].Magnitude > 1e-15)
                for (int a = 0; a < 2; a++)
                    result[a * 16 + m, a * 16 + mp] += piOn16[g][m, mp];
    return result;
}

var weldOnL = Enumerable.Range(0, 6).Select(g => WeldOnCarrier(g, true)).ToArray();
var weldOnR = Enumerable.Range(0, 6).Select(g => WeldOnCarrier(g, false)).ToArray();

// ---------------------------------------------------------------------------
// Realification helpers and spectral machinery.
// ---------------------------------------------------------------------------

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

double[,] CasimirReal(IEnumerable<Complex[,]> generators)
{
    double[,]? casimir = null;
    foreach (var g in generators)
    {
        var gr = Realify(g);
        int n = gr.GetLength(0);
        casimir ??= new double[n, n];
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
            {
                double sum = 0.0;
                for (int k = 0; k < n; k++)
                    sum += gr[i, k] * gr[k, j];
                casimir[i, j] -= sum;
            }
    }
    return casimir!;
}

Complex[,] CombineComplex(Complex[][,] generators, double[] coefficients)
{
    int n = generators[0].GetLength(0);
    var result = new Complex[n, n];
    for (int g = 0; g < generators.Length; g++)
        if (System.Math.Abs(coefficients[g]) > 1e-15)
            for (int r = 0; r < n; r++)
                for (int c = 0; c < n; c++)
                    result[r, c] += coefficients[g] * generators[g][r, c];
    return result;
}

// su(2)_A / su(2)_B generator coefficient vectors in the so(4) basis:
// A_i = (J_i + K_i)/2 -> coefficients on M_g.
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

// (j1, j2) content of a welded representation given its 6 generator
// matrices (complex): joint sub-Casimir spectra, kappa-calibrated on 2_L.
List<(double J1, double J2, int Multiplicity)> WeldedContent(Complex[][,] generators, double kappa, out bool ok)
{
    var aGens = aCoefficients.Select(c => CombineComplex(generators, c)).ToArray();
    var bGens = bCoefficients.Select(c => CombineComplex(generators, c)).ToArray();
    var cA = CasimirReal(aGens);
    var cB = CasimirReal(bGens);
    int n = cA.GetLength(0);
    var (evA, vecA) = Jacobi(cA);
    ok = true;
    var content = new List<(double, double, int)>();
    var groups = new Dictionary<long, List<int>>();
    for (int e = 0; e < n; e++)
    {
        long key = (long)System.Math.Round(evA[e] / kappa * 4.0);
        if (!groups.TryGetValue(key, out var list))
            groups[key] = list = new List<int>();
        list.Add(e);
    }
    foreach (var (aKey, indices) in groups.OrderBy(kv => kv.Key))
    {
        double j1 = (-1.0 + System.Math.Sqrt(1.0 + aKey)) / 2.0;
        var basis = new List<double[]>();
        foreach (int e in indices)
        {
            var v = new double[n];
            for (int k = 0; k < n; k++)
                v[k] = vecA[k, e];
            basis.Add(v);
        }
        var cbR = RestrictReal(cB, basis);
        var (cbEv, _) = Jacobi(cbR);
        var bGroups = new Dictionary<long, int>();
        foreach (double ev in cbEv)
        {
            long key = (long)System.Math.Round(ev / kappa * 4.0);
            bGroups[key] = bGroups.GetValueOrDefault(key) + 1;
        }
        foreach (var (bKey, count) in bGroups.OrderBy(kv => kv.Key))
        {
            double j2 = (-1.0 + System.Math.Sqrt(1.0 + bKey)) / 2.0;
            int irrepRealDim = (int)System.Math.Round((2 * j1 + 1) * (2 * j2 + 1));
            if (count % irrepRealDim != 0)
            {
                ok = false;
                continue;
            }
            content.Add((j1, j2, count / irrepRealDim));
        }
    }
    return content;
}

static double[,] RestrictReal(double[,] op, List<double[]> basis)
{
    int s = basis.Count;
    int n = op.GetLength(0);
    var result = new double[s, s];
    for (int b = 0; b < s; b++)
    {
        var image = new double[n];
        for (int r = 0; r < n; r++)
        {
            double sum = 0.0;
            for (int c = 0; c < n; c++)
                sum += op[r, c] * basis[b][c];
            image[r] = sum;
        }
        for (int a = 0; a < s; a++)
        {
            double ip = 0.0;
            for (int r = 0; r < n; r++)
                ip += image[r] * basis[a][r];
            result[a, b] = ip;
        }
    }
    return result;
}

// kappa calibration on the so(4) VECTOR rep 4 = (1/2,1/2): C_A = kappa*3/4
// (independent of any spinor convention).
double kappaCalibration;
{
    var vectorGens = so4Pairs.Select(p =>
    {
        var m = M4(p.I, p.J);
        var c = new Complex[4, 4];
        for (int r = 0; r < 4; r++)
            for (int q = 0; q < 4; q++)
                c[r, q] = m[r, q];
        return c;
    }).ToArray();
    var aGens = aCoefficients.Select(c => CombineComplex(vectorGens, c)).ToArray();
    var cA = CasimirReal(aGens);
    kappaCalibration = cA[0, 0] / 0.75;
}

// W1: welded content of the internal 16 (pi action alone) and the labels of
// the bare Weyl halves (machine-discovered, not assumed).
var content16 = WeldedContent(piOn16, kappaCalibration, out bool ok16);
var twoLGens = Enumerable.Range(0, 6).Select(g => SpacetimeOn2(g, true)).ToArray();
var twoRGens = Enumerable.Range(0, 6).Select(g => SpacetimeOn2(g, false)).ToArray();
var content2L = WeldedContent(twoLGens, kappaCalibration, out bool ok2L);
var content2R = WeldedContent(twoRGens, kappaCalibration, out bool ok2R);
bool weylLabelsRecovered = ok2L && ok2R && content2L.Count == 1 && content2R.Count == 1;
var label2L = content2L.Count == 1 ? (content2L[0].J1, content2L[0].J2) : (-1.0, -1.0);
var label2R = content2R.Count == 1 ? (content2R[0].J1, content2R[0].J2) : (-1.0, -1.0);

// W2: welded content of S_L and S_R.
var contentL = WeldedContent(weldOnL, kappaCalibration, out bool okL);
var contentR = WeldedContent(weldOnR, kappaCalibration, out bool okR);
bool irrepContentsRecovered = ok16 && okL && okR && weylLabelsRecovered;

// Character cross-check: contentL must equal (1/2,0) (x) content16 etc.
List<(double J1, double J2, int M)> TensorContent(
    (double J1, double J2) factor, List<(double J1, double J2, int Multiplicity)> baseContent)
{
    var dict = new Dictionary<(double, double), int>();
    foreach (var (j1, j2, m) in baseContent)
        for (double x = System.Math.Abs(factor.J1 - j1); x <= factor.J1 + j1 + 1e-9; x += 1.0)
            for (double y = System.Math.Abs(factor.J2 - j2); y <= factor.J2 + j2 + 1e-9; y += 1.0)
            {
                var key = (System.Math.Round(x * 2) / 2, System.Math.Round(y * 2) / 2);
                dict[key] = dict.GetValueOrDefault(key) + m;
            }
    return dict.Select(kv => (kv.Key.Item1, kv.Key.Item2, kv.Value)).OrderBy(t => t.Item1).ThenBy(t => t.Item2).ToList();
}

bool ContentsEqual(List<(double J1, double J2, int M)> a, List<(double J1, double J2, int Multiplicity)> b)
{
    var db = b.GroupBy(t => (t.J1, t.J2)).ToDictionary(g => g.Key, g => g.Sum(t => t.Multiplicity));
    var da = a.GroupBy(t => (t.J1, t.J2)).ToDictionary(g => g.Key, g => g.Sum(t => t.M));
    return da.Count == db.Count && da.All(kv => db.TryGetValue(kv.Key, out int m) && m == kv.Value);
}

bool contentLMatchesCharacter = ContentsEqual(TensorContent(label2L, content16), contentL);
bool contentRMatchesCharacter = ContentsEqual(TensorContent(label2R, content16), contentR);

// ---------------------------------------------------------------------------
// W3: spin-0 content of the bilinear channels by character arithmetic.
// Multiplicity of (0,0) in X (x) Y = sum over matching (j1,j2) pairs of
// m_X * m_Y (each matching pair contributes one singlet in the
// complexified tensor product; the real-dimension bookkeeping is recorded
// honestly: counts here are per the realified spectra, i.e. REAL irrep
// blocks as recovered by the sub-Casimirs).
// ---------------------------------------------------------------------------

int SingletCount(List<(double J1, double J2, int Multiplicity)> x, List<(double J1, double J2, int Multiplicity)> y)
{
    int total = 0;
    foreach (var (j1, j2, mx) in x)
        foreach (var (k1, k2, my) in y)
            if (System.Math.Abs(j1 - k1) < 1e-9 && System.Math.Abs(j2 - k2) < 1e-9)
                total += mx * my;
    return total;
}

int spinZeroLR = SingletCount(contentL, contentR);
int spinZeroLL = SingletCount(contentL, contentL);
int spinZeroRR = SingletCount(contentR, contentR);
bool leftRightBilinearChannelHasNoWeldedScalar = spinZeroLR == 0;

// Direct verification for the LR channel: the welded Casimir kernel of the
// realified S_L (x) S_R must have the same dimension as the character count
// implies. (Block pairs over the joint Casimir eigenspaces of each side.)
// For economy the direct check is done at the level of the A-Casimir +
// B-Casimir kernel on a randomly-projected subspace ... instead, exact and
// simple: the total welded Casimir C = C_A + C_B on S_L (x) S_R has kernel
// exactly the spin-0 sector. Realified dimension 4096 is too large for a
// dense Jacobi here, so the direct check is performed CHANNEL-BLOCKWISE on
// the smaller S_L (x) S_R sub-blocks: for each pair of A-Casimir eigenspace
// blocks with matching content the kernel is computed exactly.
// Implemented as: realified joint blocks of each carrier from WeldedContent
// reconstruction are not retained; the independent check instead verifies
// the MAJORANA channel (smaller content) by direct kernel and the LR
// channel by the exact integer character argument plus the verified W1/W2
// contents. Recorded honestly in the output.

// Direct kernel check on S_L (x) S_L spin-0 sector dimension:
int spinZeroLLDirect;
{
    // Build realified welded action on S_L (x) S_L via matrices T (64x64
    // real, viewing T as real matrix on realified S_L): D2 T = D T + T D^T.
    var dReal = weldOnL.Select(Realify).ToArray();
    int n = dReal[0].GetLength(0); // 64
    // stack kernels blockwise via the total Casimir on the realified tensor
    // square: C2(T) = sum_g D_g (D_g T + T D_g^T) + (D_g T + T D_g^T) D_g^T
    // Kernel found by Gram of the 6 maps T -> D_g T + T D_g^T over the
    // n^2-dim space, using the Casimir eigenspace blocks of S_L to keep
    // blocks small.
    var cL = CasimirReal(weldOnL);
    var (evL, vecL) = Jacobi(cL);
    var blocks = new List<List<double[]>>();
    var blockKeys = new List<long>();
    for (int e = 0; e < n; e++)
    {
        long key = (long)System.Math.Round(evL[e] / kappaCalibration * 4.0);
        var v = new double[n];
        for (int k = 0; k < n; k++)
            v[k] = vecL[k, e];
        int idx = blockKeys.IndexOf(key);
        if (idx < 0)
        {
            blockKeys.Add(key);
            blocks.Add(new List<double[]> { v });
        }
        else
            blocks[idx].Add(v);
    }
    int kernelTotal = 0;
    for (int ba = 0; ba < blocks.Count; ba++)
        for (int bb = 0; bb < blocks.Count; bb++)
        {
            int da = blocks[ba].Count, db = blocks[bb].Count;
            int dt = da * db;
            var gram = new double[dt, dt];
            for (int g = 0; g < 6; g++)
            {
                var ra = RestrictReal(dReal[g], blocks[ba]);
                var rb = RestrictReal(dReal[g], blocks[bb]);
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
            var (ev, _) = Jacobi(gram);
            kernelTotal += ev.Count(v => System.Math.Abs(v) <= Tolerance);
        }
    spinZeroLLDirect = kernelTotal;
}
bool majoranaChannelDirectCheckMatches = spinZeroLLDirect == spinZeroLL;

// ---------------------------------------------------------------------------
// W5: quartic (Dirac-squared) spin-0 count by character arithmetic:
// singlets in (S_L (x) S_R) (x) (S_L (x) S_R) and (S_L (x) S_L)^2 etc.
// computed from the channel contents.
// ---------------------------------------------------------------------------

List<(double J1, double J2, int Multiplicity)> ProductContent(
    List<(double J1, double J2, int Multiplicity)> x, List<(double J1, double J2, int Multiplicity)> y)
{
    var dict = new Dictionary<(double, double), int>();
    foreach (var (j1, j2, mx) in x)
        foreach (var (k1, k2, my) in y)
            for (double a = System.Math.Abs(j1 - k1); a <= j1 + k1 + 1e-9; a += 1.0)
                for (double b = System.Math.Abs(j2 - k2); b <= j2 + k2 + 1e-9; b += 1.0)
                {
                    var key = (System.Math.Round(a * 2) / 2, System.Math.Round(b * 2) / 2);
                    dict[key] = dict.GetValueOrDefault(key) + mx * my;
                }
    return dict.Select(kv => (kv.Key.Item1, kv.Key.Item2, kv.Value)).ToList();
}

var contentLR = ProductContent(contentL, contentR);
int quarticSpinZeroLRLR = SingletCount(contentLR, contentLR);
var contentLL = ProductContent(contentL, contentL);
int quarticSpinZeroLLLL = SingletCount(contentLL, contentLL);

bool probeInternallyConsistent =
    phase409PrecursorPassed &&
    phase410PrecursorPassed &&
    chiralHalfIsSixteenDimensional &&
    chiralInvarianceResidual <= 1e-10 &&
    weylHalvesAreTwoDimensional &&
    sigma4HomResidual <= 1e-10 &&
    colorAlgebraDimensionIsEight &&
    irrepContentsRecovered &&
    contentLMatchesCharacter &&
    contentRMatchesCharacter &&
    majoranaChannelDirectCheckMatches;

// The headline verdicts are DATA (the gate is the battery, not the answer).
bool diracMassChannelClosedForWeldedScalars = leftRightBilinearChannelHasNoWeldedScalar;

// ---------------------------------------------------------------------------
// W4: SM-doublet analysis of the Majorana-channel spin-0 sector is bounded
// out in this phase if the channel content makes it moot, and otherwise is
// the named next computation. Honest bound: the SM-doublet pattern carries
// |Y| = 1/2 with HALF-INTEGER su(2)_L isospin; a Majorana bilinear of two
// 16s carries the SUM of two spinor weights. Whether the doublet pattern
// appears is recorded here from the integer character data of the
// SM-decomposition only if the spin-0 sector is nonzero; the full SM-stable
// subspace construction on the spinor bilinears is deferred to the next
// phase with the spin-0 basis persisted... For fail-closed honesty this
// phase records the spin-0 dimensions and the channel theorem; the
// SM-doublet count WITHIN the Majorana spin-0 sector is computed directly
// below when the sector is small enough (<= 40 real dims), else named open.
// ---------------------------------------------------------------------------

int majoranaSpinZeroSmDoubletCount = -1; // -1 = not computed
bool majoranaSmAnalysisComputed = false;
if (spinZeroLLDirect > 0 && spinZeroLLDirect <= 40)
{
    // Reconstruct the spin-0 basis of S_L (x) S_L (realified) and decompose
    // under the SM chain (acting on the 16 factor of each leg).
    var dReal = weldOnL.Select(Realify).ToArray();
    int n = dReal[0].GetLength(0);
    var cL = CasimirReal(weldOnL);
    var (evL, vecL) = Jacobi(cL);
    var blocks = new List<List<double[]>>();
    var blockKeys = new List<long>();
    for (int e = 0; e < n; e++)
    {
        long key = (long)System.Math.Round(evL[e] / kappaCalibration * 4.0);
        var v = new double[n];
        for (int k = 0; k < n; k++)
            v[k] = vecL[k, e];
        int idx = blockKeys.IndexOf(key);
        if (idx < 0)
        {
            blockKeys.Add(key);
            blocks.Add(new List<double[]> { v });
        }
        else
            blocks[idx].Add(v);
    }
    var spinZeroBasis = new List<double[,]>();
    for (int ba = 0; ba < blocks.Count; ba++)
        for (int bb = 0; bb < blocks.Count; bb++)
        {
            int da = blocks[ba].Count, db = blocks[bb].Count;
            int dt = da * db;
            var gram = new double[dt, dt];
            for (int g = 0; g < 6; g++)
            {
                var ra = RestrictReal(dReal[g], blocks[ba]);
                var rb = RestrictReal(dReal[g], blocks[bb]);
                var act = new double[dt, dt];
                for (int i = 0; i < da; i++)
                    for (int ip = 0; ip < da; ip++)
                        if (System.Math.Abs(ra[i, ip]) > 1e-15)
                            for (int j2 = 0; j2 < db; j2++)
                                act[i * db + j2, ip * db + j2] += ra[i, ip];
                for (int j2 = 0; j2 < db; j2++)
                    for (int jp = 0; jp < db; jp++)
                        if (System.Math.Abs(rb[j2, jp]) > 1e-15)
                            for (int i = 0; i < da; i++)
                                act[i * db + j2, i * db + jp] += rb[j2, jp];
                for (int a2 = 0; a2 < dt; a2++)
                    for (int b2 = 0; b2 < dt; b2++)
                    {
                        double sum = 0.0;
                        for (int r = 0; r < dt; r++)
                            sum += act[r, a2] * act[r, b2];
                        gram[a2, b2] += sum;
                    }
            }
            var (ev, vec) = Jacobi(gram);
            for (int e = 0; e < dt; e++)
            {
                if (System.Math.Abs(ev[e]) > Tolerance)
                    continue;
                var t = new double[n, n];
                for (int i = 0; i < da; i++)
                    for (int j2 = 0; j2 < db; j2++)
                    {
                        double coefficient = vec[i * db + j2, e];
                        if (System.Math.Abs(coefficient) < 1e-15)
                            continue;
                        for (int r = 0; r < n; r++)
                            for (int c = 0; c < n; c++)
                                t[r, c] += coefficient * blocks[ba][i][r] * blocks[bb][j2][c];
                    }
                spinZeroBasis.Add(t);
            }
        }
    // orthonormalize
    var ortho = new List<double[,]>();
    foreach (var t in spinZeroBasis)
    {
        var v = (double[,])t.Clone();
        foreach (var u in ortho)
        {
            double ip = WInner(v, u);
            for (int r = 0; r < n; r++)
                for (int c = 0; c < n; c++)
                    v[r, c] -= ip * u[r, c];
        }
        double norm = System.Math.Sqrt(WInner(v, v));
        if (norm < 1e-9)
            continue;
        for (int r = 0; r < n; r++)
            for (int c = 0; c < n; c++)
                v[r, c] /= norm;
        ortho.Add(v);
    }
    // SM generators on S_L = 2 (x) 16: I_2 (x) Sigma(G); realified.
    double[,] SmOnCarrierReal(Complex[,] g16)
    {
        var op = new Complex[32, 32];
        for (int m = 0; m < 16; m++)
            for (int mp = 0; mp < 16; mp++)
                if (g16[m, mp].Magnitude > 1e-15)
                    for (int a2 = 0; a2 < 2; a2++)
                        op[a2 * 16 + m, a2 * 16 + mp] = g16[m, mp];
        return Realify(op);
    }
    var smReal = new List<double[,]>();
    foreach (var g in su2LOn16)
        smReal.Add(SmOnCarrierReal(g));
    smReal.Add(SmOnCarrierReal(su2R3On16));
    smReal.Add(SmOnCarrierReal(hyperchargeOn16));
    foreach (var g in colorOn16)
        smReal.Add(SmOnCarrierReal(g));

    // iterated SM stabilization within the spin-0 sector
    var uBasis = new List<double[,]>(ortho);
    for (int iter = 0; iter < 40 && uBasis.Count > 0; iter++)
    {
        int s = uBasis.Count;
        var gramU = new double[s, s];
        foreach (var a2 in smReal)
        {
            var residuals = new List<double[,]>();
            foreach (var w in uBasis)
            {
                var image = WAction(a2, w);
                foreach (var u in uBasis)
                {
                    double ip = WInner(image, u);
                    for (int r = 0; r < n; r++)
                        for (int c = 0; c < n; c++)
                            image[r, c] -= ip * u[r, c];
                }
                residuals.Add(image);
            }
            for (int x = 0; x < s; x++)
                for (int y = 0; y < s; y++)
                    gramU[x, y] += WInner(residuals[x], residuals[y]);
        }
        var (evU, vecU) = Jacobi(gramU);
        var newBasis = new List<double[,]>();
        for (int e = 0; e < s; e++)
        {
            if (System.Math.Abs(evU[e]) > Tolerance)
                continue;
            var t = new double[n, n];
            for (int k = 0; k < s; k++)
            {
                double coefficient = vecU[k, e];
                if (System.Math.Abs(coefficient) < 1e-15)
                    continue;
                for (int r = 0; r < n; r++)
                    for (int c = 0; c < n; c++)
                        t[r, c] += coefficient * uBasis[k][r, c];
            }
            newBasis.Add(t);
        }
        var ortho2 = new List<double[,]>();
        foreach (var t in newBasis)
        {
            var v = (double[,])t.Clone();
            foreach (var u in ortho2)
            {
                double ip = WInner(v, u);
                for (int r = 0; r < n; r++)
                    for (int c = 0; c < n; c++)
                        v[r, c] -= ip * u[r, c];
            }
            double norm = System.Math.Sqrt(WInner(v, v));
            if (norm < 1e-9)
                continue;
            for (int r = 0; r < n; r++)
                for (int c = 0; c < n; c++)
                    v[r, c] /= norm;
            ortho2.Add(v);
        }
        bool stable = ortho2.Count == uBasis.Count;
        uBasis = ortho2;
        if (stable)
            break;
    }
    // SM Casimir decomposition on the stable subspace
    double[] SpectrumOnU(IEnumerable<double[,]> gens)
    {
        int s = uBasis.Count;
        if (s == 0)
            return Array.Empty<double>();
        var casimir = new double[s, s];
        foreach (var a2 in gens)
            for (int k = 0; k < s; k++)
            {
                var once = WAction(a2, uBasis[k]);
                var twice = WAction(a2, once);
                for (int l = 0; l < s; l++)
                    casimir[l, k] -= WInner(twice, uBasis[l]);
            }
        var sym = new double[s, s];
        for (int x = 0; x < s; x++)
            for (int y = 0; y < s; y++)
                sym[x, y] = 0.5 * (casimir[x, y] + casimir[y, x]);
        var (ev, _) = Jacobi(sym);
        return ev;
    }
    var su2LSpec = SpectrumOnU(su2LOn16.Select(SmOnCarrierReal));
    var ySpec = SpectrumOnU(new[] { SmOnCarrierReal(hyperchargeOn16) });
    var colorSpec = SpectrumOnU(colorOn16.Select(SmOnCarrierReal));
    // calibration on the 16 itself (realified): doublet j_L = 1/2 value and
    // |Y| = 1/2 value from the known family content.
    var calSu2L = CasimirReal(su2LOn16);
    var (calEvL, _) = Jacobi(calSu2L);
    double jHalfValue = calEvL.Max();
    var calY = CasimirReal(new[] { hyperchargeOn16 });
    var (calEvY, _) = Jacobi(calY);
    double yHalfValue = calEvY.Where(v => v > 0.05).DefaultIfEmpty(0.0).Min();
    int count = 0;
    for (int k = 0; k < uBasis.Count; k++)
    {
        bool jHalf = System.Math.Abs(su2LSpec[k] - jHalfValue) <= 1e-6;
        bool yHalf = System.Math.Abs(ySpec[k] - yHalfValue) <= 1e-6;
        bool colorSinglet = System.Math.Abs(colorSpec[k]) <= 1e-6;
        if (jHalf && yHalf && colorSinglet)
            count++;
    }
    majoranaSpinZeroSmDoubletCount = count;
    majoranaSmAnalysisComputed = true;
}

bool spinorBilinearSpinZeroDoubletAbsent =
    leftRightBilinearChannelHasNoWeldedScalar &&
    (!majoranaSmAnalysisComputed || majoranaSpinZeroSmDoubletCount == 0) &&
    (spinZeroLLDirect == 0 || majoranaSmAnalysisComputed);

// ---------------------------------------------------------------------------
// Fail-closed boundary.
// ---------------------------------------------------------------------------

const bool complexCompactArithmeticUsed = true; // Nguyen-Polya caveat carried
const bool pinParityClassificationAttempted = false;
const bool quarticSmStableAnalysisComputed = false; // named remaining order
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
const string ApplicationSubjectKind = "quartic-dirac-squared-spinor-composite-probe";
const bool physicalTargetsConsultedForConstruction = false;

string targetBlindConstructionHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join(
    "|",
    ApplicationSubjectKind,
    "welded chiral carriers 2_LR x 16 via Cl(10) + Sym^2 weld; sub-Casimir branching; bilinear channel singlet theorem; Majorana-channel SM analysis; quartic character counts")))).ToLowerInvariant();

bool quarticDiracSquaredSpinorCompositeProbePassed =
    probeInternallyConsistent &&
    complexCompactArithmeticUsed &&
    !pinParityClassificationAttempted &&
    !quarticSmStableAnalysisComputed &&
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

string terminalStatus = quarticDiracSquaredSpinorCompositeProbePassed
    ? (spinorBilinearSpinZeroDoubletAbsent
        ? "spinor-bilinear-channels-carry-no-welded-scalar-sm-doublet"
        : "spinor-bilinear-welded-scalar-sm-doublet-candidate-found")
    : "quartic-dirac-squared-spinor-composite-probe-blocked";

string decision = quarticDiracSquaredSpinorCompositeProbePassed
    ? (spinorBilinearSpinZeroDoubletAbsent
        ? "The Dirac-squared composite route is machine-characterized at its natural (bilinear) channel level. The welded branching of the internal 16 and the chiral carriers is exact, and the channel theorem follows: the DIRAC MASS / YUKAWA CHANNEL (S_L x S_R) - exactly where the SM Higgs couples - contains NO welded-scalar at all (its welded content pairs (integer, half-integer) against (half-integer, integer) types, so no singlet can form); whatever welded scalars exist live in the MAJORANA-type channels, and the SM-stable analysis of that sector finds NO SM-doublet pattern there either. CONSEQUENCE: a 'VEV in a Dirac-like operator coupling two chiral halves' cannot be a welded spacetime SCALAR on this chain's bilinear channels - the curvature-coaxing and Dirac-squared heuristics, in their composite readings, are now CLOSED through bilinear order alongside the Phase409 frame-cross closure. The quartic SM-stable analysis (the spin-0 counts are recorded by character arithmetic) and the draft's unobserved-phase fields remain the named open routes, along with a noncompact-real-form evasion (the Nguyen-Polya complexification caveat is carried: all arithmetic here is complex/compact). Nothing is promoted; no contract field is filled."
        : "A welded-scalar SM-doublet CANDIDATE exists in a spinor-bilinear channel - the recorded spectra identify where; the next phase must characterize whether it descends to a field-level extraction before any contract claim. Nothing is promoted; no contract field is filled.")
    : "Do not use the channel characterization until the precursors and the exactness battery pass.";

var result = new
{
    phaseId = "phase411-quartic-dirac-squared-spinor-composite-probe",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    quarticDiracSquaredSpinorCompositeProbePassed,
    phase409PrecursorPassed,
    phase410PrecursorPassed,
    probeInternallyConsistent,
    chiralHalfIsSixteenDimensional,
    chiralInvarianceResidual,
    weylHalvesAreTwoDimensional,
    sigma4HomResidual,
    weylLabel2L = new { j1 = label2L.Item1, j2 = label2L.Item2 },
    weylLabel2R = new { j1 = label2R.Item1, j2 = label2R.Item2 },
    colorAlgebraDimensionIsEight,
    irrepContentsRecovered,
    welded16Content = content16.Select(t => new { j1 = t.J1, j2 = t.J2, multiplicity = t.Multiplicity }).ToArray(),
    weldedLeftCarrierContent = contentL.Select(t => new { j1 = t.J1, j2 = t.J2, multiplicity = t.Multiplicity }).ToArray(),
    weldedRightCarrierContent = contentR.Select(t => new { j1 = t.J1, j2 = t.J2, multiplicity = t.Multiplicity }).ToArray(),
    contentLMatchesCharacter,
    contentRMatchesCharacter,
    spinZeroDiracMassChannel = spinZeroLR,
    spinZeroMajoranaLLChannel = spinZeroLL,
    spinZeroMajoranaRRChannel = spinZeroRR,
    spinZeroMajoranaLLDirectKernel = spinZeroLLDirect,
    majoranaChannelDirectCheckMatches,
    leftRightBilinearChannelHasNoWeldedScalar,
    diracMassChannelClosedForWeldedScalars,
    majoranaSmAnalysisComputed,
    majoranaSpinZeroSmDoubletCount,
    spinorBilinearSpinZeroDoubletAbsent,
    quarticSpinZeroCountLRLR = quarticSpinZeroLRLR,
    quarticSpinZeroCountLLLL = quarticSpinZeroLLLL,
    quarticSmStableAnalysisComputed,
    complexCompactArithmeticUsed,
    pinParityClassificationAttempted,
    physicalCouplingProvided,
    probeDefinitions = new
    {
        carrier = "chimeric chiral carriers S_L = 2_L (x) 16, S_R = 2_R (x) 16 with the welded action D(M) = sigma(M) (x) I + I (x) Sigma(pi(M)); Cl(10) tensor-string gammas (Phase404), Sym^2 weld (Phase408)",
        w1w2 = "welded (j1, j2) content via joint su(2)_A/su(2)_B sub-Casimir spectra on realified carriers, kappa-calibrated on the bare 2_L, cross-checked by character arithmetic",
        w3 = "spin-0 content of S_L x S_R, S_L x S_L, S_R x S_R by exact character pairing; Majorana channel verified by direct blockwise kernel",
        w4 = "largest SM-stable subspace of the Majorana spin-0 sector (su2L + su2R Cartan + Y + full color su(3) in the spinor rep), machine-calibrated Casimir decomposition, SM-doublet pattern count",
        w5 = "quartic spin-0 dimensions by character arithmetic over the machine-derived channel contents; quartic SM-stable analysis = named remaining order",
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
        "spinor-bilinear composites are NOT the draft's fundamental scalar; this probe characterizes the Dirac-squared composite reading of the heuristics",
        "all arithmetic is complex/compact (Cl(10), so(10)); the Nguyen-Polya complexification objection and noncompact real-form effects are carried as named caveats - a noncompact evasion remains conceivable and unprobed",
        "Pin-group parity classification of spinor composites is NOT attempted",
        "the quartic SM-stable analysis is NOT computed (spin-0 counts recorded by character arithmetic); it is the named remaining order",
        "the draft's unobserved-phase fields remain unprobed",
        "no dynamics, no scales, no VEV; the binding gaps are unchanged",
        "no Phase201 or Phase256 fill",
        "no physical predictions",
    },
    sourceEvidence = new
    {
        phase409SummaryPath = Phase409SummaryPath,
        phase410SummaryPath = Phase410SummaryPath,
        primaryDraft = "docs/Reference/ExperimentReferences/texts/GU-DRAFT-2021-TEXT.txt (sections 9, 12; S_g(C) = S(V) x S(H*))",
        heuristicContext = "docs/Reference/ExperimentReferences/DEEP-RESEARCH-20260612.md ('quartic Higgs piece' from 'Dirac squaring of a quadratic', dating plausible-unverified) and TOE-GU-40YEARS-20250602.md ('a VEV in a Dirac-like operator')",
    },
    decision,
};

var options = JsonOptions();
File.WriteAllText(Path.Combine(outputDir, "quartic_dirac_squared_spinor_composite_probe.json"), JsonSerializer.Serialize(result, options));
string summaryPath = Path.Combine(outputDir, "quartic_dirac_squared_spinor_composite_probe_summary.json");
File.WriteAllText(summaryPath, JsonSerializer.Serialize(result, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"quarticDiracSquaredSpinorCompositeProbePassed={quarticDiracSquaredSpinorCompositeProbePassed}");
Console.WriteLine($"welded16Content=[{string.Join(",", content16.Select(t => $"({t.J1},{t.J2})x{t.Multiplicity}"))}] (realified blocks)");
Console.WriteLine($"S_L content=[{string.Join(",", contentL.Select(t => $"({t.J1},{t.J2})x{t.Multiplicity}"))}] characterMatch={contentLMatchesCharacter}");
Console.WriteLine($"S_R content=[{string.Join(",", contentR.Select(t => $"({t.J1},{t.J2})x{t.Multiplicity}"))}] characterMatch={contentRMatchesCharacter}");
Console.WriteLine($"spinZero: DiracMass(LR)={spinZeroLR} MajoranaLL={spinZeroLL} (direct {spinZeroLLDirect}, match={majoranaChannelDirectCheckMatches}) MajoranaRR={spinZeroRR}");
Console.WriteLine($"leftRightBilinearChannelHasNoWeldedScalar={leftRightBilinearChannelHasNoWeldedScalar}");
Console.WriteLine($"majoranaSmAnalysisComputed={majoranaSmAnalysisComputed} majoranaSpinZeroSmDoubletCount={majoranaSpinZeroSmDoubletCount}");
Console.WriteLine($"spinorBilinearSpinZeroDoubletAbsent={spinorBilinearSpinZeroDoubletAbsent}");
Console.WriteLine($"quarticSpinZero: (LR)^2={quarticSpinZeroLRLR} (LL)^2={quarticSpinZeroLLLL} (SM-stable analysis = named remaining order)");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");
Console.WriteLine($"summaryPath={summaryPath}");

static double WInner(double[,] a, double[,] b)
{
    double sum = 0.0;
    int n = a.GetLength(0);
    for (int r = 0; r < n; r++)
        for (int c = 0; c < n; c++)
            sum += a[r, c] * b[r, c];
    return sum;
}

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
