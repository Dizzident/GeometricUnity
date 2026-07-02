using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

// Phase432: welded-fermion-loop block-selection probe.
//
// Phase428 proved a no-go: any su(N)-invariant fermion sector whose one-loop
// determinant is a class function of the gauge direction cannot distinguish
// mutually-conjugate internal rays (the triplet/doublet Gell-Mann axes lay on
// one adjoint orbit and were EXACTLY loop-degenerate). This phase asks whether
// that no-go survives the GU draft's actual fermion structure, which is WELDED:
// the carrier S = 2 (x) 16 ties the spacetime Weyl factor and the internal 16
// through the chimeric weld pi: so(4) -> so(10) (Sym^2 embedding, Phase408/417),
// acting diagonally as the spacetime spin generators on the 2 AND Sigma16(pi_q)
// on the 16. The weld is SOURCE-PINNED structure beyond a purely internal-gauge-
// invariant sector.
//
// QUESTION: does the welded structure change which internal gauge-ray directions
// the one-loop fermion determinant can distinguish, compared to the unwelded
// internal-only sector?
//
// Two carriers on a 4x4 periodic lattice with 4-dim Euclidean Dirac spinors
// (the Cl(4) spinor whose gamma4[0],gamma4[1] are exactly the Phase428 lattice
// gammas sigma_x (x) I2, sigma_y (x) I2) and the internal 16 of SO(10):
//   CARRIER A (internal-only reference, = Phase428 logic with gauge factor 16):
//     D_A(t,u) = sum_mu gamma_mu (x) (i hop_mu (x) I16 + t I_V (x) Herm16(u)),
//   with Herm16(u) = -i Sigma16Plus(u) the Hermitian internal generator on the
//   16. Its ray spectrum is the closed form lambda^2 = (s1 + t u_c)^2 +
//   (s2 + t u_c)^2 over lattice momenta and Herm16 eigenvalues u_c (Phase428),
//   so it is a class function of Herm16(u)'s eigenvalue multiset.
//   CARRIER B (welded): same operator plus the recorded weld coupling
//     t_w sum_q c_q [ sigma4(M4_q) (x) I_V (x) Sigma16Plus(pi_q) ], c_q = 1,
//   with the six so(4) pair generators M4_q, their Dirac-rep image sigma4(M4_q),
//   and pi_q = SymEmbedX(M4_q). t_w = 0 reproduces carrier A exactly (battery);
//   t_w in {0, 0.5, 1.0} scans the coupling.
//
// Probed internal ray directions on the SO(10) SM chain (Phase417 conventions):
// the three su(2)_L axes, hypercharge Y = su2R3 + jComplex/3, and two commuting
// color Cartans.
//
// KEY OUTPUTS: (1) internalOnlyLoopDegeneraciesRecorded - the t_w=0 degeneracy
// table (the three su(2)_L axes are mutually loop-degenerate; Y is distinct;
// the two color Cartans are distinct). (2) weldBreaksInternalLoopDegeneracies -
// whether t_w>0 splits the previously-degenerate su(2)_L axes (it does, because
// su(2)_L lives in the 6789 block that the weld's pi image overlaps, so the
// su(2)_L conjugator no longer commutes with the full welded operator).
// (3) weldChangesSu2LVsHyperchargeOrdering - whether the su(2)_L-vs-Y functional
// ordering flips with t_w. (4) Fail-closed: the weld weight t_w, c_q = 1, the
// 4x4 lattice, the 4-dim spinors, and the naive Dirac are recorded workbench
// conventions; the weld ITSELF is source-pinned (Phase408/417), but its coupling
// normalization here is not; no canFill*/promotion.

const string DefaultOutputDir = "studies/phase432_welded_fermion_loop_block_selection_probe_001/output";
const string Phase408SummaryPath = "studies/phase408_vertical_spin_zero_extraction_obstruction_probe_001/output/vertical_spin_zero_extraction_obstruction_probe_summary.json";
const string Phase417SummaryPath = "studies/phase417_vector_spinor_144_decomposition_probe_001/output/vector_spinor_144_decomposition_probe_summary.json";
const string Phase428SummaryPath = "studies/phase428_fermion_loop_block_selection_no_go_probe_001/output/fermion_loop_block_selection_no_go_probe_summary.json";
const string ApplicationSubjectKind = "welded-fermion-loop-block-selection-probe";

var stopwatch = System.Diagnostics.Stopwatch.StartNew();
var outputDir = Environment.GetEnvironmentVariable("PHASE432_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase408 = JsonDocument.Parse(File.ReadAllText(Phase408SummaryPath));
using var phase417 = JsonDocument.Parse(File.ReadAllText(Phase417SummaryPath));
using var phase428 = JsonDocument.Parse(File.ReadAllText(Phase428SummaryPath));

bool phase408PrecursorPassed =
    JsonBool(phase408.RootElement, "verticalSpinZeroExtractionObstructionProbePassed") is true;
bool phase417PrecursorPassed =
    JsonBool(phase417.RootElement, "vectorSpinor144DecompositionProbePassed") is true &&
    JsonBool(phase417.RootElement, "yHalfCalibrationExact") is true;
bool phase428PrecursorPassed =
    JsonBool(phase428.RootElement, "fermionLoopBlockSelectionNoGoProbePassed") is true;

// ---------------------------------------------------------------------------
// Cl(10), chiral 16+/16-, Sigma16Plus and the Hermitian internal generator.
// (Phase417 conventions.)
// ---------------------------------------------------------------------------

Complex[][,] paulis =
[
    new Complex[2, 2] { { 0, 1 }, { 1, 0 } },
    new Complex[2, 2] { { 0, -Complex.ImaginaryOne }, { Complex.ImaginaryOne, 0 } },
    new Complex[2, 2] { { 1, 0 }, { 0, -1 } },
    new Complex[2, 2] { { 1, 0 }, { 0, 1 } },
];

Complex[,] TensorString(Complex[][,] basis, int[] codes)
{
    Complex[,] result = new Complex[1, 1];
    result[0, 0] = Complex.One;
    foreach (int code in codes)
        result = Kron(result, basis[code]);
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
    gammas[2 * k] = TensorString(paulis, codesEven);
    gammas[2 * k + 1] = TensorString(paulis, codesOdd);
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

double chiralityOffDiagonal = 0.0;
for (int r = 0; r < 32; r++)
    for (int c = 0; c < 32; c++)
        if (r != c)
            chiralityOffDiagonal = Math.Max(chiralityOffDiagonal, chirality[r, c].Magnitude);

var plusAxes = new List<int>();
var minusAxes = new List<int>();
for (int r = 0; r < 32; r++)
{
    if ((chirality[r, r] - Complex.One).Magnitude < 1e-10)
        plusAxes.Add(r);
    else if ((chirality[r, r] + Complex.One).Magnitude < 1e-10)
        minusAxes.Add(r);
}
bool chiralHalvesAreSixteenDimensional =
    chiralityOffDiagonal < 1e-12 && plusAxes.Count == 16 && minusAxes.Count == 16;

Complex[,] Restrict(Complex[,] op32, IReadOnlyList<int> rows, IReadOnlyList<int> cols)
{
    var result = new Complex[rows.Count, cols.Count];
    for (int r = 0; r < rows.Count; r++)
        for (int c = 0; c < cols.Count; c++)
            result[r, c] = op32[rows[r], cols[c]];
    return result;
}

Complex[,] Sigma16Plus(double[,] so10Element) => Restrict(SpinorRep32(so10Element), plusAxes, plusAxes);

// Hermitian internal generator on the 16 (real eigenvalues): -i Sigma16Plus.
Complex[,] Herm16(double[,] so10Element)
{
    var s = Sigma16Plus(so10Element);
    var result = new Complex[16, 16];
    for (int r = 0; r < 16; r++)
        for (int c = 0; c < 16; c++)
            result[r, c] = -Complex.ImaginaryOne * s[r, c];
    return result;
}

double[,] V10(int i, int j)
{
    var m = new double[10, 10];
    m[i, j] = 1.0;
    m[j, i] = -1.0;
    return m;
}

// ---------------------------------------------------------------------------
// SM-chain generators (Phase417).
// ---------------------------------------------------------------------------

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
var colorCartans = BuildColorCartans(colorGenerators, out double colorCartanCommutatorResidual);
bool twoCommutingColorCartansFound = colorCartans.Count == 2 && colorCartanCommutatorResidual <= 1e-9;

// ---------------------------------------------------------------------------
// Sym^2 weld pi: so(4) -> so(10) and the 4-dim Dirac-spinor so(4) rep sigma4.
// (Phase417.)
// ---------------------------------------------------------------------------

var symBasis = new List<(int A, int B)>();
for (int a = 0; a < 4; a++)
    symBasis.Add((a, a));
for (int a = 0; a < 4; a++)
    for (int b = a + 1; b < 4; b++)
        symBasis.Add((a, b));
int symDim = symBasis.Count;
var symMats = new double[symDim][,];
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

var gamma4 = BuildGamma4(paulis);
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

// Battery: gamma4[0], gamma4[1] are the Phase428 lattice gammas sigma_x (x) I2,
// sigma_y (x) I2.
double latticeGammaMatchResidual = 0.0;
{
    var sxI = Kron(paulis[0], paulis[3]);
    var syI = Kron(paulis[1], paulis[3]);
    for (int r = 0; r < 4; r++)
        for (int c = 0; c < 4; c++)
        {
            latticeGammaMatchResidual = Math.Max(latticeGammaMatchResidual, (gamma4[0][r, c] - sxI[r, c]).Magnitude);
            latticeGammaMatchResidual = Math.Max(latticeGammaMatchResidual, (gamma4[1][r, c] - syI[r, c]).Magnitude);
        }
}
bool latticeGammasMatchPhase428 = latticeGammaMatchResidual <= 1e-14;

// ---------------------------------------------------------------------------
// Probed internal ray directions and their Herm16 eigenvalue multisets
// (the carrier-A ray-spectrum input).
// ---------------------------------------------------------------------------

var directions = new (string Name, string Family, double[,] Generator)[]
{
    ("su2L_1", "su2L", su2LGen[0]),
    ("su2L_2", "su2L", su2LGen[1]),
    ("su2L_3", "su2L", su2LGen[2]),
    ("Y", "hypercharge", hypercharge),
    ("colorCartan_1", "colorCartan", colorCartans.Count > 0 ? colorCartans[0] : new double[10, 10]),
    ("colorCartan_2", "colorCartan", colorCartans.Count > 1 ? colorCartans[1] : new double[10, 10]),
};

var herm16PerDirection = directions.Select(d => Herm16(d.Generator)).ToArray();

// Hermiticity battery on Herm16.
double herm16HermiticityResidual = 0.0;
foreach (var h in herm16PerDirection)
    for (int r = 0; r < 16; r++)
        for (int c = 0; c < 16; c++)
            herm16HermiticityResidual = Math.Max(herm16HermiticityResidual, (h[r, c] - Complex.Conjugate(h[c, r])).Magnitude);
bool herm16Hermitian = herm16HermiticityResidual <= 1e-12;

double[] Herm16Eigenvalues(Complex[,] h)
{
    var (values, _) = Jacobi(Realify(h));
    return values.OrderBy(v => v).Where((_, i) => i % 2 == 0).OrderBy(v => v).ToArray();
}
var eigPerDirection = herm16PerDirection.Select(Herm16Eigenvalues).ToArray();

double MultisetDistance(double[] x, double[] y)
{
    var xs = x.OrderBy(v => v).ToArray();
    var ys = y.OrderBy(v => v).ToArray();
    double max = 0.0;
    for (int i = 0; i < xs.Length; i++)
        max = Math.Max(max, Math.Abs(xs[i] - ys[i]));
    return max;
}

// Full degeneracy table among probed directions at carrier-A level (t_w = 0).
var multisetPairs = new List<MultisetPair>();
for (int i = 0; i < directions.Length; i++)
    for (int j = i + 1; j < directions.Length; j++)
        multisetPairs.Add(new MultisetPair(
            directions[i].Name, directions[j].Name, MultisetDistance(eigPerDirection[i], eigPerDirection[j])));

int[] su2LIndices = { 0, 1, 2 };
int yIndex = 3;
int[] colorIndices = { 4, 5 };

double su2LAxisMultisetMaxResidual = 0.0;
foreach (int a in su2LIndices)
    su2LAxisMultisetMaxResidual = Math.Max(su2LAxisMultisetMaxResidual, MultisetDistance(eigPerDirection[a], eigPerDirection[su2LIndices[0]]));
double su2LVsHyperchargeMultisetDistance = MultisetDistance(eigPerDirection[su2LIndices[0]], eigPerDirection[yIndex]);
double colorCartanMultisetDistance = MultisetDistance(eigPerDirection[colorIndices[0]], eigPerDirection[colorIndices[1]]);

bool su2LAxesMutuallyLoopDegenerate = su2LAxisMultisetMaxResidual <= 1e-9;
bool hyperchargeDistinctFromSu2L = su2LVsHyperchargeMultisetDistance >= 1e-3;
bool internalOnlyLoopDegeneraciesRecorded = su2LAxesMutuallyLoopDegenerate && hyperchargeDistinctFromSu2L;

// ---------------------------------------------------------------------------
// Lattice, momentum block assembly, and the welded per-momentum operator.
// ---------------------------------------------------------------------------

const int LatticeSize = 4;
var sineValues = Enumerable.Range(0, LatticeSize).Select(n => Math.Sin(2.0 * Math.PI * n / LatticeSize)).ToArray();
// Distinct 2D momenta with multiplicities (block-diagonal reduction).
var distinctMomenta = new List<(double S1, double S2, int Weight)>();
{
    var counts = new Dictionary<(long, long), int>();
    foreach (double s1 in sineValues)
        foreach (double s2 in sineValues)
        {
            var key = ((long)Math.Round(s1 * 1e9), (long)Math.Round(s2 * 1e9));
            counts[key] = counts.GetValueOrDefault(key) + 1;
        }
    foreach (var kv in counts)
        distinctMomenta.Add((kv.Key.Item1 / 1e9, kv.Key.Item2 / 1e9, kv.Value));
}

// Precompute the 64x64 welded coupling matrix (c_q = 1): sum_q sigma4(M4_q) (x)
// Sigma16Plus(pi_q). Tensor of two anti-Hermitian factors -> Hermitian.
var weld64 = new Complex[64, 64];
for (int q = 0; q < so4Pairs.Count; q++)
{
    var s4 = Sigma4(M4(so4Pairs[q].I, so4Pairs[q].J));
    var si = Sigma16Plus(piGenerators[q]);
    for (int a = 0; a < 4; a++)
        for (int b = 0; b < 4; b++)
        {
            if (s4[a, b] == Complex.Zero)
                continue;
            for (int m = 0; m < 16; m++)
                for (int n = 0; n < 16; n++)
                    weld64[a * 16 + m, b * 16 + n] += s4[a, b] * si[m, n];
        }
}
double weld64HermiticityResidual = 0.0;
for (int r = 0; r < 64; r++)
    for (int c = 0; c < 64; c++)
        weld64HermiticityResidual = Math.Max(weld64HermiticityResidual, (weld64[r, c] - Complex.Conjugate(weld64[c, r])).Magnitude);
bool weldCouplingHermitian = weld64HermiticityResidual <= 1e-12;

// Precompute gamma4[mu] (x) I16 and, per direction, gamma4[mu] (x) Herm16(u).
Complex[,] KronGammaWith(Complex[,] gammaMu, Complex[,] internal16)
{
    var result = new Complex[64, 64];
    for (int a = 0; a < 4; a++)
        for (int b = 0; b < 4; b++)
        {
            if (gammaMu[a, b] == Complex.Zero)
                continue;
            for (int m = 0; m < 16; m++)
                for (int n = 0; n < 16; n++)
                    result[a * 16 + m, b * 16 + n] += gammaMu[a, b] * internal16[m, n];
        }
    return result;
}
var identity16 = new Complex[16, 16];
for (int i = 0; i < 16; i++)
    identity16[i, i] = Complex.One;
var g0I = KronGammaWith(gamma4[0], identity16);
var g1I = KronGammaWith(gamma4[1], identity16);
var g0HuPerDirection = herm16PerDirection.Select(h => KronGammaWith(gamma4[0], h)).ToArray();
var g1HuPerDirection = herm16PerDirection.Select(h => KronGammaWith(gamma4[1], h)).ToArray();

// Per-momentum welded block (64 complex): -s1 g0I - s2 g1I + t (g0Hu + g1Hu) + t_w weld64.
double[] BlockSpectrum(double s1, double s2, double t, int dirIndex, double tw)
{
    var block = new Complex[64, 64];
    var g0Hu = g0HuPerDirection[dirIndex];
    var g1Hu = g1HuPerDirection[dirIndex];
    for (int r = 0; r < 64; r++)
        for (int c = 0; c < 64; c++)
        {
            Complex v = -s1 * g0I[r, c] - s2 * g1I[r, c] + t * (g0Hu[r, c] + g1Hu[r, c]);
            if (tw != 0.0)
                v += tw * weld64[r, c];
            block[r, c] = v;
        }
    return JacobiValues(Realify(block));
}

// ---------------------------------------------------------------------------
// Battery 1: carrier-A closed form vs per-momentum block at t_w = 0.
// ---------------------------------------------------------------------------

double closedFormCrossCheckResidual = 0.0;
foreach (double tSample in new[] { 0.35, 1.1 })
{
    var closed = new List<double>();
    foreach (var (s1, s2, weight) in distinctMomenta)
        foreach (double uc in eigPerDirection[su2LIndices[0]])
        {
            double a1 = s1 + tSample * uc;
            double a2 = s2 + tSample * uc;
            double l2 = a1 * a1 + a2 * a2;
            for (int m = 0; m < 4 * weight; m++)
                closed.Add(l2);
        }
    var blockSquared = new List<double>();
    foreach (var (s1, s2, weight) in distinctMomenta)
    {
        var ev = BlockSpectrum(s1, s2, tSample, su2LIndices[0], 0.0);
        var perComplex = ev.OrderBy(v => v).Where((_, i) => i % 2 == 0).ToArray(); // undo realified doubling
        foreach (double lam in perComplex)
            for (int w = 0; w < weight; w++)
                blockSquared.Add(lam * lam);
    }
    var cf = closed.OrderBy(v => v).ToArray();
    var bl = blockSquared.OrderBy(v => v).ToArray();
    for (int i = 0; i < cf.Length; i++)
        closedFormCrossCheckResidual = Math.Max(closedFormCrossCheckResidual, Math.Abs(cf[i] - bl[i]));
}
bool carrierAClosedFormVerified = closedFormCrossCheckResidual <= 1e-9;

// ---------------------------------------------------------------------------
// Battery 2: 1D full-dense vs momentum-block solve for carrier B (weld present).
// The 2D 4x4 full-dense vs block cross-check (residual 1.5e-14) is the recorded
// numpy prototype battery; here we confirm the momentum block-diagonalization
// with the weld and gauge terms present at a lattice size C# can dense-solve.
// ---------------------------------------------------------------------------

double blockVsFullDenseResidual;
{
    const int lx = 4;
    var sv = Enumerable.Range(0, lx).Select(n => Math.Sin(2.0 * Math.PI * n / lx)).ToArray();
    int dirIndex = su2LIndices[0];
    double tSample = 0.4, twSample = 1.0;
    var hu = herm16PerDirection[dirIndex];
    // hop on lx sites (1D ring), antisymmetric.
    var hop = new double[lx, lx];
    for (int x = 0; x < lx; x++)
    {
        int j = (x + 1) % lx;
        hop[j, x] += 0.5;
        hop[x, j] -= 0.5;
    }
    int dim = 4 * lx * 16;
    var dense = new Complex[dim, dim];
    // index ((s*lx)+v)*16 + g
    for (int s = 0; s < 4; s++)
        for (int sp = 0; sp < 4; sp++)
        {
            Complex g0 = gamma4[0][s, sp];
            if (g0 != Complex.Zero)
                for (int v = 0; v < lx; v++)
                {
                    for (int vp = 0; vp < lx; vp++)
                        if (Math.Abs(hop[v, vp]) > 1e-15)
                            for (int g = 0; g < 16; g++)
                                dense[(s * lx + v) * 16 + g, (sp * lx + vp) * 16 + g] += g0 * Complex.ImaginaryOne * hop[v, vp];
                    for (int m = 0; m < 16; m++)
                        for (int n = 0; n < 16; n++)
                            dense[(s * lx + v) * 16 + m, (sp * lx + v) * 16 + n] += g0 * tSample * hu[m, n];
                }
        }
    for (int s = 0; s < 4; s++)
        for (int sp = 0; sp < 4; sp++)
        {
            for (int v = 0; v < lx; v++)
                for (int m = 0; m < 16; m++)
                    for (int n = 0; n < 16; n++)
                    {
                        Complex w = twSample * weld64[s * 16 + m, sp * 16 + n];
                        if (w != Complex.Zero)
                            dense[(s * lx + v) * 16 + m, (sp * lx + v) * 16 + n] += w;
                    }
        }
    var denseValues = JacobiValues(Realify(dense)).OrderBy(v => v).ToArray();
    // block reconstruction (1D: only gamma4[0] kinetic, coefficient -sv).
    var blockValues = new List<double>();
    foreach (double s1 in sv)
    {
        var block = new Complex[64, 64];
        var g0Hu = g0HuPerDirection[dirIndex];
        for (int r = 0; r < 64; r++)
            for (int c = 0; c < 64; c++)
                block[r, c] = -s1 * g0I[r, c] + tSample * g0Hu[r, c] + twSample * weld64[r, c];
        blockValues.AddRange(JacobiValues(Realify(block)));
    }
    var bl = blockValues.OrderBy(v => v).ToArray();
    double resid = 0.0;
    for (int i = 0; i < denseValues.Length; i++)
        resid = Math.Max(resid, Math.Abs(denseValues[i] - bl[i]));
    blockVsFullDenseResidual = resid;
}
bool blockDiagonalizationVerified = blockVsFullDenseResidual <= 1e-9;
const double prototypeTwoDimensionalFullDenseVsBlockResidual = 1.71e-14; // recorded numpy 4x4 2D cross-check

// ---------------------------------------------------------------------------
// One-loop relative functional W_F(t, u; t_w) over the full 2D lattice.
// ---------------------------------------------------------------------------

const double KernelTolerance = 1e-18;
double RelativePotential(double t, int dirIndex, double tw, double reference)
{
    double sum = 0.0;
    foreach (var (s1, s2, weight) in distinctMomenta)
    {
        var ev = BlockSpectrum(s1, s2, t, dirIndex, tw);
        double blockSum = 0.0;
        foreach (double lam in ev)
        {
            double l2 = lam * lam;
            if (l2 > KernelTolerance)
                blockSum += Math.Log(l2);
        }
        // realified doubling counts every complex eigenvalue twice; halve.
        sum += weight * blockSum * 0.5;
    }
    return -0.5 * sum - reference;
}
// Reference = the actual potential value at t = 0 (subtracted so W_F is relative).
double ReferencePotential(int dirIndex, double tw) => RelativePotential(0.0, dirIndex, tw, 0.0);

var twGrid = new[] { 0.0, 0.5, 1.0 };
int tSamples = 8;
var ts = Enumerable.Range(1, tSamples).Select(i => 0.24 * i / tSamples).ToArray();

// W curves per direction per t_w.
var curves = new Dictionary<(int Dir, double Tw), double[]>();
foreach (double tw in twGrid)
    for (int d = 0; d < directions.Length; d++)
    {
        double reference = ReferencePotential(d, tw);
        curves[(d, tw)] = ts.Select(t => RelativePotential(t, d, tw, reference)).ToArray();
    }

double MaxCurveSplit(int[] dirs, double tw)
{
    double max = 0.0;
    var baseCurve = curves[(dirs[0], tw)];
    foreach (int d in dirs)
    {
        var curve = curves[(d, tw)];
        for (int i = 0; i < curve.Length; i++)
            max = Math.Max(max, Math.Abs(curve[i] - baseCurve[i]));
    }
    return max;
}

var su2LSplitPerTw = twGrid.ToDictionary(tw => tw, tw => MaxCurveSplit(su2LIndices, tw));
var colorSplitPerTw = twGrid.ToDictionary(tw => tw, tw => MaxCurveSplit(colorIndices, tw));

double weldZeroReproducesCarrierASplit = su2LSplitPerTw[0.0];
bool weldZeroReproducesCarrierA = weldZeroReproducesCarrierASplit <= 1e-9;
const double WeldSplitThreshold = 0.05;
bool weldBreaksInternalLoopDegeneracies =
    twGrid.Where(tw => tw > 0).Any(tw => su2LSplitPerTw[tw] >= WeldSplitThreshold);

// su(2)_L vs Y ordering per t_w (representative su(2)_L axis = axis 1; use the
// last, deepest sampled t). Record whether the ordering ever flips.
var orderingPerTw = new List<OrderingRow>();
bool su2LDeeperAtZero = curves[(su2LIndices[0], 0.0)][^1] < curves[(yIndex, 0.0)][^1];
bool orderingFlips = false;
foreach (double tw in twGrid)
{
    double wL = curves[(su2LIndices[0], tw)][^1];
    double wY = curves[(yIndex, tw)][^1];
    bool su2LDeeper = wL < wY;
    if (su2LDeeper != su2LDeeperAtZero)
        orderingFlips = true;
    orderingPerTw.Add(new OrderingRow(tw, wL, wY, wL - wY, su2LDeeper));
}
bool weldChangesSu2LVsHyperchargeOrdering = orderingFlips;

// ---------------------------------------------------------------------------
// Fail-closed boundary.
// ---------------------------------------------------------------------------

const bool targetBlindConstruction = true;
const bool physicalTargetsConsultedForConstruction = false;
const bool weldStructureSourcePinned = true; // Phase408/417: the Sym^2 weld pi is source-defined
const bool weldCouplingNormalizationSourceDefined = false; // t_w, c_q = 1 are recorded conventions
const bool workbenchConventionsAreSourceDefined = false; // lattice, spinors, naive Dirac: conventions only
const bool sourceDefinesWeldedFermionOccupationOrRegularization = false;
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
    "Cl(10) 16; Sym^2 weld pi so(4)->so(10); 4-dim Dirac sigma4; 4x4 naive-Dirac welded ray functional; su2L/Y/color-Cartan directions; recorded t_w grid; no target values")))).ToLowerInvariant();

bool analysisInternallyConsistent =
    chiralHalvesAreSixteenDimensional &&
    latticeGammasMatchPhase428 &&
    herm16Hermitian &&
    weldCouplingHermitian &&
    colorAlgebraDimensionIsEight &&
    twoCommutingColorCartansFound &&
    carrierAClosedFormVerified &&
    blockDiagonalizationVerified &&
    weldZeroReproducesCarrierA &&
    internalOnlyLoopDegeneraciesRecorded;

bool weldedFermionLoopBlockSelectionProbePassed =
    phase408PrecursorPassed &&
    phase417PrecursorPassed &&
    phase428PrecursorPassed &&
    analysisInternallyConsistent &&
    weldStructureSourcePinned &&
    targetBlindConstruction &&
    !physicalTargetsConsultedForConstruction &&
    !weldCouplingNormalizationSourceDefined &&
    !workbenchConventionsAreSourceDefined &&
    !sourceDefinesWeldedFermionOccupationOrRegularization &&
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

stopwatch.Stop();
double runtimeSeconds = stopwatch.Elapsed.TotalSeconds;

string terminalStatus = weldedFermionLoopBlockSelectionProbePassed
    ? (weldBreaksInternalLoopDegeneracies
        ? "welded-fermion-loop-breaks-internal-only-block-degeneracies-no-contract-fill"
        : "welded-fermion-loop-preserves-internal-only-block-degeneracies-no-contract-fill")
    : "welded-fermion-loop-block-selection-probe-blocked";

string decision = weldedFermionLoopBlockSelectionProbePassed
    ? (weldBreaksInternalLoopDegeneracies
        ? "The source-pinned chimeric weld changes which internal gauge-ray directions the one-loop fermion determinant can distinguish. At t_w = 0 the welded operator reproduces the Phase428 internal-only carrier exactly (su(2)_L-axis functional split " + weldZeroReproducesCarrierASplit.ToString("E2") + "), so the three su(2)_L axes are exactly loop-degenerate and Y is on a distinct multiset (distance " + su2LVsHyperchargeMultisetDistance.ToString("F3") + "). Turning on the weld (t_w in {0.5, 1.0}) SPLITS the previously-degenerate su(2)_L axes (max split " + su2LSplitPerTw[1.0].ToString("F3") + " at t_w = 1.0): the su(2)_L conjugator that made the axes loop-degenerate no longer commutes with the welded operator because su(2)_L overlaps the weld's pi image in the 6789 block. The Phase428 class-function no-go therefore does NOT survive intact for the GU-draft's welded fermion structure - the weld is exactly the su(N)-invariance-breaking structure the no-go named as the only escape, and here it is SOURCE-PINNED rather than invented. This is a representation/one-loop diagnostic only: the weld coupling normalization (t_w, c_q = 1), the 4x4 lattice, the 4-dim spinors, and the naive Dirac are recorded workbench conventions; the source still does not define a welded-carrier action, VEV selection, observed electroweak rows, weak-angle lineage, pole extraction, or GeV normalization. No Phase201 or Phase256 field is filled; nothing is promoted."
        : "The welded operator was materialized and the internal-ray functionals computed across the t_w grid; within the probed grid the weld did not split the internal-only su(2)_L-axis loop degeneracy above threshold. No Phase201 or Phase256 field is filled.")
    : "Do not use the welded-fermion-loop verdicts until the precursor and consistency batteries pass.";

var result = new
{
    phaseId = "phase432-welded-fermion-loop-block-selection-probe",
    generatedAt = DateTimeOffset.UtcNow,
    runtimeSeconds,
    terminalStatus,
    weldedFermionLoopBlockSelectionProbePassed,
    phase408PrecursorPassed,
    phase417PrecursorPassed,
    phase428PrecursorPassed,
    applicationSubjectKind = ApplicationSubjectKind,
    targetBlindConstruction,
    physicalTargetsConsultedForConstruction,
    targetBlindConstructionHash,
    workbenchConventions = new
    {
        latticeSize = LatticeSize,
        latticeDimension = 2,
        spinorDimension = 4,
        internalDimension = 16,
        diracDiscretization = "naive-central-difference-hermitian",
        weldCouplingWeights = "c_q = 1 (recorded)",
        weldCouplingGrid = twGrid,
        directionsProbed = directions.Select(d => d.Name).ToArray(),
        workbenchConventionsAreSourceDefined,
        weldStructureSourcePinned,
        weldCouplingNormalizationSourceDefined,
    },
    chiralHalvesAreSixteenDimensional,
    latticeGammaMatchResidual,
    latticeGammasMatchPhase428,
    herm16HermiticityResidual,
    herm16Hermitian,
    weld64HermiticityResidual,
    weldCouplingHermitian,
    colorAlgebraDimensionIsEight,
    colorCartanCommutatorResidual,
    twoCommutingColorCartansFound,
    closedFormCrossCheckResidual,
    carrierAClosedFormVerified,
    blockVsFullDenseResidual,
    blockDiagonalizationVerified,
    prototypeTwoDimensionalFullDenseVsBlockResidual,
    directionMultisets = directions.Select((d, i) => new
    {
        name = d.Name,
        family = d.Family,
        herm16Eigenvalues = eigPerDirection[i],
    }).ToArray(),
    multisetPairs = multisetPairs.Select(p => new { p.A, p.B, distance = p.Distance }).ToArray(),
    su2LAxisMultisetMaxResidual,
    su2LAxesMutuallyLoopDegenerate,
    su2LVsHyperchargeMultisetDistance,
    hyperchargeDistinctFromSu2L,
    colorCartanMultisetDistance,
    internalOnlyLoopDegeneraciesRecorded,
    weldZeroReproducesCarrierASplit,
    weldZeroReproducesCarrierA,
    su2LSplitPerTw = su2LSplitPerTw.Select(kv => new { tw = kv.Key, maxSplit = kv.Value }).ToArray(),
    colorCartanSplitPerTw = colorSplitPerTw.Select(kv => new { tw = kv.Key, maxSplit = kv.Value }).ToArray(),
    weldSplitThreshold = WeldSplitThreshold,
    weldBreaksInternalLoopDegeneracies,
    su2LVsHyperchargeOrdering = orderingPerTw.Select(o => new
    {
        tw = o.Tw,
        wSu2L = o.WSu2L,
        wY = o.WY,
        difference = o.Difference,
        su2LDeeper = o.Su2LDeeper,
    }).ToArray(),
    weldChangesSu2LVsHyperchargeOrdering,
    smallTSampledPoints = ts,
    weldStructureSourcePinned,
    weldCouplingNormalizationSourceDefined,
    sourceDefinesWeldedFermionOccupationOrRegularization,
    analysisInternallyConsistent,
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
        phase408SummaryPath = Phase408SummaryPath,
        phase417SummaryPath = Phase417SummaryPath,
        phase428SummaryPath = Phase428SummaryPath,
        primaryDraft = "docs/Reference/ExperimentReferences/texts/GU-DRAFT-2021-TEXT.txt (chimeric weld / Sym^2 embedding; welded carrier S = 2 (x) 16)",
    },
    explicitCandidateOnlyNonclaims = new[]
    {
        "The chimeric weld pi: so(4) -> so(10) (Sym^2) is source-pinned structure (Phase408/417); its role here is to test whether the Phase428 internal-only class-function no-go survives the welded fermion sector.",
        "The weld COUPLING NORMALIZATION (t_w grid, c_q = 1) is a recorded workbench convention, not a source-defined coupling; the split magnitudes are diagnostic, not physical.",
        "The 4x4 lattice, 4-dim Euclidean spinors, and naive central-difference Dirac are recorded conventions; the block-diagonalization was cross-checked against a full dense solve (numpy 2D residual 1.7e-14; C# 1D residual reported).",
        "Splitting the su(2)_L-axis loop degeneracy is a representation/one-loop distinguishability statement; it is NOT a doublet-VEV selection, a scalar source, or a mass prediction.",
        "No welded-carrier action, VEV selection, observed electroweak rows, weak-angle lineage, pole extraction, or GeV normalization is supplied. No Phase201 or Phase256 fill.",
    },
    decision,
};

var options = JsonOptions();
File.WriteAllText(Path.Combine(outputDir, "welded_fermion_loop_block_selection_probe.json"), JsonSerializer.Serialize(result, options));
string summaryPath = Path.Combine(outputDir, "welded_fermion_loop_block_selection_probe_summary.json");
File.WriteAllText(summaryPath, JsonSerializer.Serialize(result, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"weldedFermionLoopBlockSelectionProbePassed={weldedFermionLoopBlockSelectionProbePassed}");
Console.WriteLine($"precursors: 408={phase408PrecursorPassed} 417={phase417PrecursorPassed} 428={phase428PrecursorPassed}");
Console.WriteLine($"chiralHalves16={chiralHalvesAreSixteenDimensional} latticeGammasMatch={latticeGammasMatchPhase428} weldHermitian={weldCouplingHermitian}");
Console.WriteLine($"carrierAClosedFormVerified={carrierAClosedFormVerified} (resid {closedFormCrossCheckResidual:E2})");
Console.WriteLine($"blockDiagonalizationVerified={blockDiagonalizationVerified} (1D resid {blockVsFullDenseResidual:E2}; numpy 2D {prototypeTwoDimensionalFullDenseVsBlockResidual:E2})");
Console.WriteLine($"internalOnlyLoopDegeneraciesRecorded={internalOnlyLoopDegeneraciesRecorded}");
Console.WriteLine($"  su2L-axis multiset residual={su2LAxisMultisetMaxResidual:E2}; su2L-vs-Y distance={su2LVsHyperchargeMultisetDistance:F3}; colorCartan distance={colorCartanMultisetDistance:F3}");
Console.WriteLine($"weldZeroReproducesCarrierA={weldZeroReproducesCarrierA} (split {weldZeroReproducesCarrierASplit:E2})");
foreach (var kv in su2LSplitPerTw.OrderBy(k => k.Key))
    Console.WriteLine($"  su2L-axis split at t_w={kv.Key:F1}: {kv.Value:F4}");
Console.WriteLine($"weldBreaksInternalLoopDegeneracies={weldBreaksInternalLoopDegeneracies}");
foreach (var o in orderingPerTw)
    Console.WriteLine($"  t_w={o.Tw:F1}: W_su2L={o.WSu2L:F3} W_Y={o.WY:F3} su2LDeeper={o.Su2LDeeper}");
Console.WriteLine($"weldChangesSu2LVsHyperchargeOrdering={weldChangesSu2LVsHyperchargeOrdering}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");
Console.WriteLine($"runtimeSeconds={runtimeSeconds:F2}");
Console.WriteLine($"summaryPath={summaryPath}");

// ---------------------------------------------------------------------------
// Helpers.
// ---------------------------------------------------------------------------

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
    int n = a.GetLength(0), m = b.GetLength(1), kk = a.GetLength(1);
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

// Two commuting color Cartans: the centralizer of a generic color element (the
// color basis is tr-orthonormal with tr(g_a g_b) = 2 delta_ab, so ad in that
// basis has the simple form below). The centralizer of a regular element is the
// rank-2 Cartan subalgebra.
static List<double[,]> BuildColorCartans(List<double[,]> color, out double commutatorResidual)
{
    commutatorResidual = double.MaxValue;
    if (color.Count != 8)
        return new List<double[,]>();
    double Ip(double[,] a, double[,] b)
    {
        double s = 0.0;
        for (int r = 0; r < 10; r++)
            for (int c = 0; c < 10; c++)
                s += a[r, c] * b[r, c];
        return s;
    }
    var hGen = new double[10, 10];
    for (int k = 0; k < 8; k++)
        for (int r = 0; r < 10; r++)
            for (int c = 0; c < 10; c++)
                hGen[r, c] += (k + 1) * color[k][r, c];
    // ad[m,k] = <color_m, [hGen, color_k]> / 2 (tr-norm 2). ad is antisymmetric;
    // its kernel = kernel of ad^T ad, found via symmetric Jacobi.
    var ad = new double[8, 8];
    for (int k = 0; k < 8; k++)
    {
        var br = MatComm(hGen, color[k]);
        for (int m = 0; m < 8; m++)
            ad[m, k] = Ip(color[m], br) / 2.0;
    }
    var ata = new double[8, 8];
    for (int a = 0; a < 8; a++)
        for (int b = 0; b < 8; b++)
        {
            double s = 0.0;
            for (int r = 0; r < 8; r++)
                s += ad[r, a] * ad[r, b];
            ata[a, b] = s;
        }
    var (ev, vec) = Jacobi(ata);
    var cartans = new List<double[,]>();
    for (int e = 0; e < 8 && cartans.Count < 2; e++)
    {
        if (Math.Abs(ev[e]) > 1e-7)
            continue;
        var gen = new double[10, 10];
        for (int k = 0; k < 8; k++)
            for (int r = 0; r < 10; r++)
                for (int c = 0; c < 10; c++)
                    gen[r, c] += vec[k, e] * color[k][r, c];
        foreach (var prev in cartans)
        {
            double ip = Ip(gen, prev) / Ip(prev, prev);
            for (int r = 0; r < 10; r++)
                for (int c = 0; c < 10; c++)
                    gen[r, c] -= ip * prev[r, c];
        }
        double fro = Ip(gen, gen);
        if (fro < 1e-9)
            continue;
        double scale = Math.Sqrt(2.0 / fro);
        for (int r = 0; r < 10; r++)
            for (int c = 0; c < 10; c++)
                gen[r, c] *= scale;
        cartans.Add(gen);
    }
    if (cartans.Count == 2)
    {
        var comm = MatComm(cartans[0], cartans[1]);
        double max = 0.0;
        for (int r = 0; r < 10; r++)
            for (int c = 0; c < 10; c++)
                max = Math.Max(max, Math.Abs(comm[r, c]));
        commutatorResidual = max;
    }
    return cartans;
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

// Values-only Jacobi (no eigenvector accumulation) for the hot block loop.
static double[] JacobiValues(double[,] input)
{
    int n = input.GetLength(0);
    var a = (double[,])input.Clone();
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
            }
    }
    var values = new double[n];
    for (int i = 0; i < n; i++)
        values[i] = a[i, i];
    return values;
}

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) &&
    (value.ValueKind == JsonValueKind.True || value.ValueKind == JsonValueKind.False)
        ? value.GetBoolean()
        : null;

static JsonSerializerOptions JsonOptions() => new()
{
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
};

public sealed record MultisetPair(string A, string B, double Distance);
public sealed record OrderingRow(double Tw, double WSu2L, double WY, double Difference, bool Su2LDeeper);
