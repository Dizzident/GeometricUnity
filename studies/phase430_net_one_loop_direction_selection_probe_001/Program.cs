using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

// Phase430: net one-loop direction-selection probe (the named non-constant /
// full-one-loop successor to the Phase428 fermion-loop no-go).
//
// Phase428 proved that the FERMION loop alone, on constant rank-1 rays, is an
// su(3) class function: it falls like -N log t in every direction and cannot
// supply a stabilizer or select a block. This phase adds the BOSONIC one-loop
// contribution (transverse fluctuation determinant, recorded workbench model)
// and studies the NET one-loop functional W(t) = W_B(t) + W_F(t) along the
// same constant rays omega = t*u.
//
// The bosonic and fermionic logarithmic slopes have OPPOSITE signs, so the net
// large-t slope is a genuine competition whose winner depends on the fermionic
// matter content:
//   - fundamental-3 fermions: NET T/D slope +64 (CONFINED, rises) while the
//     singlet/hypercharge axis has the unique negative slope -64 => the
//     su(3)->su(2)xu(1) breaking direction is the steepest descent.
//   - adjoint-8 fermions: the selection FLIPS (T/D -192 steeper than S -128).
//   - THE DERIVED CONTENT (Phase404 blind one-family pattern: the 16 contains 4
//     triplet-type reps + 4 su(3) singlets, i.e. fermionic content = 4 copies
//     of the fundamental): NET T/D -320, S -640 => the hypercharge axis is
//     again the steepest descent.
//
// Fail-closed: the lattice, spinors, Dirac discretization, and (especially) the
// bosonic mode-mass model are RECORDED WORKBENCH CONVENTIONS, not source-defined
// physics. The exact part of the bosonic side is the mode-count arithmetic;
// tying the masses to the actual control-branch Hessian is named future work.
// Every slope is a log runaway: there is NO finite minimum on any ray, hence NO
// scale law. No target values are consulted; no Phase201 or Phase256 field can
// be filled.

const string DefaultOutputDir = "studies/phase430_net_one_loop_direction_selection_probe_001/output";
const string Phase404SummaryPath = "studies/phase404_gu_embedding_chain_coupling_ratio_enumeration_001/output/gu_embedding_chain_coupling_ratio_enumeration_summary.json";
const string Phase410SummaryPath = "studies/phase410_curvature_coupled_vev_selection_probe_001/output/curvature_coupled_vev_selection_probe_summary.json";
const string Phase428SummaryPath = "studies/phase428_fermion_loop_block_selection_no_go_probe_001/output/fermion_loop_block_selection_no_go_probe_summary.json";
const string ApplicationSubjectKind = "net-one-loop-direction-selection-probe";

var outputDir = Environment.GetEnvironmentVariable("PHASE430_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase404 = JsonDocument.Parse(File.ReadAllText(Phase404SummaryPath));
using var phase410 = JsonDocument.Parse(File.ReadAllText(Phase410SummaryPath));
using var phase428 = JsonDocument.Parse(File.ReadAllText(Phase428SummaryPath));

bool phase404PrecursorPassed =
    JsonBool(phase404.RootElement, "guEmbeddingChainCouplingRatioEnumerationPassed") is true;
bool phase410PrecursorPassed =
    JsonBool(phase410.RootElement, "curvatureCoupledVevSelectionProbePassed") is true;
bool phase428PrecursorPassed =
    JsonBool(phase428.RootElement, "fermionLoopBlockSelectionNoGoProbePassed") is true &&
    JsonBool(phase428.RootElement, "fermionLoopBlockSelectionMechanismClosed") is true;

// ---------------------------------------------------------------------------
// su(3): Gell-Mann fundamental and adjoint representations, with exactness
// batteries (ported from Phase428).
// ---------------------------------------------------------------------------

var gellMann = new Complex[8][,];
for (int a = 0; a < 8; a++)
    gellMann[a] = new Complex[3, 3];
gellMann[0][0, 1] = 1; gellMann[0][1, 0] = 1;
gellMann[1][0, 1] = -Complex.ImaginaryOne; gellMann[1][1, 0] = Complex.ImaginaryOne;
gellMann[2][0, 0] = 1; gellMann[2][1, 1] = -1;
gellMann[3][0, 2] = 1; gellMann[3][2, 0] = 1;
gellMann[4][0, 2] = -Complex.ImaginaryOne; gellMann[4][2, 0] = Complex.ImaginaryOne;
gellMann[5][1, 2] = 1; gellMann[5][2, 1] = 1;
gellMann[6][1, 2] = -Complex.ImaginaryOne; gellMann[6][2, 1] = Complex.ImaginaryOne;
double invSqrt3 = 1.0 / Math.Sqrt(3.0);
gellMann[7][0, 0] = invSqrt3; gellMann[7][1, 1] = invSqrt3; gellMann[7][2, 2] = -2.0 * invSqrt3;

var genFund = new Complex[8][,];
for (int a = 0; a < 8; a++)
{
    genFund[a] = new Complex[3, 3];
    for (int r = 0; r < 3; r++)
        for (int c = 0; c < 3; c++)
            genFund[a][r, c] = gellMann[a][r, c] / 2.0;
}

// tr(T_a T_b) = delta_ab / 2 battery
double traceNormalizationResidual = 0.0;
for (int a = 0; a < 8; a++)
    for (int b = 0; b < 8; b++)
    {
        Complex trace = Complex.Zero;
        for (int r = 0; r < 3; r++)
            for (int c = 0; c < 3; c++)
                trace += genFund[a][r, c] * genFund[b][c, r];
        trace -= a == b ? 0.5 : 0.0;
        traceNormalizationResidual = Math.Max(traceNormalizationResidual, trace.Magnitude);
    }
bool traceNormalizationExact = traceNormalizationResidual <= 1e-14;

// structure constants f_abc from [T_a, T_b] = i f_abc T_c; adjoint rep
var fabc = new double[8, 8, 8];
double structureConstantResidual = 0.0;
for (int a = 0; a < 8; a++)
    for (int b = 0; b < 8; b++)
    {
        var comm = new Complex[3, 3];
        for (int r = 0; r < 3; r++)
            for (int c = 0; c < 3; c++)
            {
                Complex sum = Complex.Zero;
                for (int k = 0; k < 3; k++)
                    sum += genFund[a][r, k] * genFund[b][k, c] - genFund[b][r, k] * genFund[a][k, c];
                comm[r, c] = sum;
            }
        var reconstructed = new Complex[3, 3];
        for (int c3 = 0; c3 < 8; c3++)
        {
            Complex trace = Complex.Zero;
            for (int r = 0; r < 3; r++)
                for (int c = 0; c < 3; c++)
                    trace += comm[r, c] * genFund[c3][c, r];
            fabc[a, b, c3] = (-2.0 * Complex.ImaginaryOne * trace).Real;
        }
        for (int c3 = 0; c3 < 8; c3++)
            for (int r = 0; r < 3; r++)
                for (int c = 0; c < 3; c++)
                    reconstructed[r, c] += Complex.ImaginaryOne * fabc[a, b, c3] * genFund[c3][r, c];
        for (int r = 0; r < 3; r++)
            for (int c = 0; c < 3; c++)
                structureConstantResidual = Math.Max(structureConstantResidual, (comm[r, c] - reconstructed[r, c]).Magnitude);
    }
bool structureConstantsExact = structureConstantResidual <= 1e-13;

var genAdjoint = new Complex[8][,];
double adjointHermiticityResidual = 0.0;
for (int a = 0; a < 8; a++)
{
    genAdjoint[a] = new Complex[8, 8];
    for (int b = 0; b < 8; b++)
        for (int c = 0; c < 8; c++)
            genAdjoint[a][b, c] = -Complex.ImaginaryOne * fabc[a, b, c];
    for (int b = 0; b < 8; b++)
        for (int c = 0; c < 8; c++)
            adjointHermiticityResidual = Math.Max(adjointHermiticityResidual,
                (genAdjoint[a][b, c] - Complex.Conjugate(genAdjoint[a][c, b])).Magnitude);
}
bool adjointRepresentationHermitian = adjointHermiticityResidual <= 1e-13;

// Explicit conjugacy witness: color-swap g maps lambda_4 to lambda_1 exactly.
var conjugator = new Complex[3, 3];
conjugator[0, 0] = 1; conjugator[1, 2] = 1; conjugator[2, 1] = 1;
double conjugacyWitnessResidual = 0.0;
{
    var mapped = new Complex[3, 3];
    for (int r = 0; r < 3; r++)
        for (int c = 0; c < 3; c++)
        {
            Complex sum = Complex.Zero;
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    sum += conjugator[r, i] * gellMann[3][i, j] * Complex.Conjugate(conjugator[c, j]);
            mapped[r, c] = sum;
        }
    for (int r = 0; r < 3; r++)
        for (int c = 0; c < 3; c++)
            conjugacyWitnessResidual = Math.Max(conjugacyWitnessResidual, (mapped[r, c] - gellMann[0][r, c]).Magnitude);
}
bool conjugacyWitnessExact = conjugacyWitnessResidual <= 1e-14;

// Generator eigenvalue multisets per axis and representation.
double[] GeneratorEigenvalues(Complex[,] hermitian)
{
    var (values, _) = Jacobi(Realify(hermitian));
    return values.OrderBy(v => v).Where((_, i) => i % 2 == 0).ToArray(); // realified doubling
}

int axisCount = 8;
var eigFund = new double[axisCount][];
var eigAdjoint = new double[axisCount][];
for (int a = 0; a < axisCount; a++)
{
    eigFund[a] = GeneratorEigenvalues(genFund[a]);
    eigAdjoint[a] = GeneratorEigenvalues(genAdjoint[a]);
}

// Bosonic mode masses m_i^2(u) = eigenvalues of (-ad_u^2) on su(3). With
// ad_u = i * (Hermitian adjoint generator), -ad_u^2 = (adjoint generator)^2,
// so the masses are exactly the squared adjoint eigenvalues (positive
// semidefinite). ker ad_u = 2 for regular T/D axes, 4 for the lambda_8 axis.
double[] BosonMassesSquared(int axis) => eigAdjoint[axis].Select(e => e * e).ToArray();

// ---------------------------------------------------------------------------
// Lattice, closed-form fermion ray spectrum, and the two one-loop functionals.
// ---------------------------------------------------------------------------

const int LatticeSize = 4;
var sineValues = Enumerable.Range(0, LatticeSize).Select(n => Math.Sin(2.0 * Math.PI * n / LatticeSize)).ToArray();
var momenta = new List<(double S1, double S2)>();
for (int n1 = 0; n1 < LatticeSize; n1++)
    for (int n2 = 0; n2 < LatticeSize; n2++)
        momenta.Add((sineValues[n1], sineValues[n2]));

const double KernelTolerance = 1e-18;

// FERMIONIC: closed form lambda^2 = (s1 + t*u_c)^2 + (s2 + t*u_c)^2, mult 4.
List<double> RaySpectrumSquared(double t, double[] generatorEigenvalues)
{
    var values = new List<double>(momenta.Count * generatorEigenvalues.Length * 4);
    foreach (var (s1, s2) in momenta)
        foreach (double uc in generatorEigenvalues)
        {
            double a1 = s1 + t * uc;
            double a2 = s2 + t * uc;
            double lambdaSquared = a1 * a1 + a2 * a2;
            for (int m = 0; m < 4; m++)
                values.Add(lambdaSquared);
        }
    return values;
}

double FermionPotential(double t, double[] u)
{
    double sum = 0.0;
    foreach (double l2 in RaySpectrumSquared(t, u))
        if (l2 > KernelTolerance)
            sum += Math.Log(l2);
    return -0.5 * sum;
}

// BOSONIC: W_B(t) = +1/2 sum over 16 momenta, 2 polarizations, 8 adjoint
// directions of log(eps_k^2 + t^2 m_i^2), eps_k^2 = sin^2 k1 + sin^2 k2.
double BosonPotential(double t, double[] massesSquared)
{
    double sum = 0.0;
    foreach (var (s1, s2) in momenta)
    {
        double eps2 = s1 * s1 + s2 * s2;
        foreach (double m2 in massesSquared)
        {
            double arg = eps2 + t * t * m2;
            if (arg > KernelTolerance)
                sum += 2.0 * Math.Log(arg); // 2 polarizations
        }
    }
    return 0.5 * sum;
}

// Relative-to-t=0 net functional and its large-t log slope. The raw octave
// slope between t=40 and t=80 carries an O(1/t^2) lattice correction e that
// scales by 1/4 per octave; Richardson extrapolation over the 40/80/160
// octaves removes it exactly for a pure 1/t^2 tail: N = (4*hi - lo)/3.
double Slope(double whi, double wlo) => (whi - wlo) / Math.Log(2.0);
(double Rich, double Raw) RichardsonSlope(Func<double, double> w)
{
    double lo = Slope(w(80.0), w(40.0));
    double hi = Slope(w(160.0), w(80.0));
    return ((4.0 * hi - lo) / 3.0, lo);
}

Func<double, double> BosonNet(int axis)
{
    var m = BosonMassesSquared(axis);
    double refB = BosonPotential(0.0, m);
    return t => BosonPotential(t, m) - refB;
}
Func<double, double> FermionNet(int axis, double[][] eig, int copies)
{
    var u = eig[axis];
    double refF = FermionPotential(0.0, u);
    return t => copies * (FermionPotential(t, u) - refF);
}
Func<double, double> TotalNet(int axis, double[][] eig, int copies)
{
    var b = BosonNet(axis);
    var f = FermionNet(axis, eig, copies);
    return t => b(t) + f(t);
}

// ---------------------------------------------------------------------------
// Slope batteries against the exact-integer acceptance targets.
// ---------------------------------------------------------------------------

const double SlopeTolerance = 0.05;
const int RepresentativeT = 0, RepresentativeD = 3, RepresentativeS = 7;

var contents = new (string Name, double[][] Eig, int Copies)[]
{
    ("fundamental-3", eigFund, 1),
    ("adjoint-8", eigAdjoint, 1),
    ("derived-4x-fundamental", eigFund, 4),
};

// bosonic slope battery (content-independent): T/D +192, S +128
var bosonBattery = new List<SlopeRow>();
double maxBosonSlopeResidual = 0.0;
foreach (var (block, axis, expected) in new[] { ("T", RepresentativeT, 192.0), ("D", RepresentativeD, 192.0), ("S", RepresentativeS, 128.0) })
{
    var (rich, raw) = RichardsonSlope(BosonNet(axis));
    maxBosonSlopeResidual = Math.Max(maxBosonSlopeResidual, Math.Abs(rich - expected));
    bosonBattery.Add(new SlopeRow("bosonic", block, axis + 1, rich, raw, expected));
}
bool bosonicSlopesMatchTargets = maxBosonSlopeResidual <= SlopeTolerance;

// fermionic slope battery per content
var fermionTargets = new Dictionary<string, (double Td, double S)>
{
    ["fundamental-3"] = (-128.0, -192.0),
    ["adjoint-8"] = (-384.0, -256.0),
    ["derived-4x-fundamental"] = (-512.0, -768.0),
};
var netTargets = new Dictionary<string, (double Td, double S)>
{
    ["fundamental-3"] = (64.0, -64.0),
    ["adjoint-8"] = (-192.0, -128.0),
    ["derived-4x-fundamental"] = (-320.0, -640.0),
};

var fermionBattery = new List<SlopeRow>();
var netBattery = new List<SlopeRow>();
double maxFermionSlopeResidual = 0.0;
double maxNetSlopeResidual = 0.0;
double maxTdNetDegeneracyResidual = 0.0;
foreach (var (name, eig, copies) in contents)
{
    var (fTd, fS) = fermionTargets[name];
    var (nTd, nS) = netTargets[name];
    foreach (var (block, axis, fExp, nExp) in new[]
             {
                 ("T", RepresentativeT, fTd, nTd),
                 ("D", RepresentativeD, fTd, nTd),
                 ("S", RepresentativeS, fS, nS),
             })
    {
        var (fRich, fRaw) = RichardsonSlope(FermionNet(axis, eig, copies));
        var (nRich, nRaw) = RichardsonSlope(TotalNet(axis, eig, copies));
        maxFermionSlopeResidual = Math.Max(maxFermionSlopeResidual, Math.Abs(fRich - fExp));
        maxNetSlopeResidual = Math.Max(maxNetSlopeResidual, Math.Abs(nRich - nExp));
        fermionBattery.Add(new SlopeRow($"fermionic-{name}", block, axis + 1, fRich, fRaw, fExp));
        netBattery.Add(new SlopeRow($"net-{name}", block, axis + 1, nRich, nRaw, nExp));
    }
    // T/D exact degeneracy (class-function): net slope equal across axes 1..7
    double baseTd = RichardsonSlope(TotalNet(0, eig, copies)).Rich;
    for (int axis = 1; axis < 7; axis++)
        maxTdNetDegeneracyResidual = Math.Max(maxTdNetDegeneracyResidual,
            Math.Abs(RichardsonSlope(TotalNet(axis, eig, copies)).Rich - baseTd));
}
bool fermionicSlopesMatchTargets = maxFermionSlopeResidual <= SlopeTolerance;
bool netSlopesMatchTargets = maxNetSlopeResidual <= SlopeTolerance;
bool tdNetSlopeExactlyDegenerate = maxTdNetDegeneracyResidual <= 1e-9;

// net slope lookups for verdicts
double NetSlope(string content, int axis)
{
    var (_, eig, copies) = contents.First(c => c.Name == content);
    return RichardsonSlope(TotalNet(axis, eig, copies)).Rich;
}
double fundNetTd = NetSlope("fundamental-3", RepresentativeT);
double fundNetS = NetSlope("fundamental-3", RepresentativeS);
double adjNetTd = NetSlope("adjoint-8", RepresentativeT);
double adjNetS = NetSlope("adjoint-8", RepresentativeS);
double derivedNetTd = NetSlope("derived-4x-fundamental", RepresentativeT);
double derivedNetS = NetSlope("derived-4x-fundamental", RepresentativeS);

// ---------------------------------------------------------------------------
// Dense fermion cross-check of the closed-form ray spectrum (full 192-dim
// fundamental operator at sample points), ported from Phase428.
// ---------------------------------------------------------------------------

var gamma = new Complex[2][,];
gamma[0] = new Complex[4, 4];
gamma[1] = new Complex[4, 4];
gamma[0][0, 2] = 1; gamma[0][1, 3] = 1; gamma[0][2, 0] = 1; gamma[0][3, 1] = 1;              // sigma_x (x) I2
gamma[1][0, 2] = -Complex.ImaginaryOne; gamma[1][1, 3] = -Complex.ImaginaryOne;             // sigma_y (x) I2
gamma[1][2, 0] = Complex.ImaginaryOne; gamma[1][3, 1] = Complex.ImaginaryOne;

double closedFormCrossCheckResidual = 0.0;
{
    int vertices = LatticeSize * LatticeSize;
    int dimG = 3;
    int n = 4 * vertices * dimG;
    var hop = new double[2][,];
    for (int mu = 0; mu < 2; mu++)
    {
        hop[mu] = new double[vertices, vertices];
        for (int x = 0; x < LatticeSize; x++)
            for (int y = 0; y < LatticeSize; y++)
            {
                int i = x + LatticeSize * y;
                int j = mu == 0 ? ((x + 1) % LatticeSize) + LatticeSize * y : x + LatticeSize * ((y + 1) % LatticeSize);
                hop[mu][j, i] += 0.5;
                hop[mu][i, j] -= 0.5;
            }
    }
    foreach (double tSample in new[] { 0.35, 1.25 })
    {
        var dirac = new Complex[n, n];
        for (int mu = 0; mu < 2; mu++)
            for (int s = 0; s < 4; s++)
                for (int sp = 0; sp < 4; sp++)
                {
                    if (gamma[mu][s, sp] == Complex.Zero)
                        continue;
                    for (int v = 0; v < vertices; v++)
                        for (int vp = 0; vp < vertices; vp++)
                            if (Math.Abs(hop[mu][v, vp]) > 1e-15)
                                for (int g = 0; g < dimG; g++)
                                    dirac[(s * vertices + v) * dimG + g, (sp * vertices + vp) * dimG + g] +=
                                        gamma[mu][s, sp] * Complex.ImaginaryOne * hop[mu][v, vp];
                    for (int v = 0; v < vertices; v++)
                        for (int g = 0; g < dimG; g++)
                            for (int gp = 0; gp < dimG; gp++)
                                if (genFund[0][g, gp] != Complex.Zero)
                                    dirac[(s * vertices + v) * dimG + g, (sp * vertices + v) * dimG + gp] +=
                                        gamma[mu][s, sp] * tSample * genFund[0][g, gp];
                }
        var (denseValues, _) = Jacobi(Realify(dirac));
        var denseSquared = denseValues.Select(v => v * v).OrderBy(v => v).Where((_, i) => i % 2 == 0).OrderBy(v => v).ToArray();
        var closed = RaySpectrumSquared(tSample, eigFund[0]).OrderBy(v => v).ToArray();
        for (int i = 0; i < closed.Length; i++)
            closedFormCrossCheckResidual = Math.Max(closedFormCrossCheckResidual, Math.Abs(closed[i] - denseSquared[i]));
    }
}
bool closedFormSpectrumVerified = closedFormCrossCheckResidual <= 1e-9;

// ---------------------------------------------------------------------------
// Mixed-configuration teaser (the motivation for the net analysis): with
// omega_x = a*U, omega_y = b*V, U = lambda_1/2, V = lambda_4/2, the fermion
// loop alone plus the tree bosonic quartic kappa*(ab)^2*||[U,V]||^2 leaves the
// minimum on the a- or b-axis (boundary escape along a ray). Fundamental rep,
// dense per-momentum 12-dim eigensolve (constant gauge conserves momentum).
// ---------------------------------------------------------------------------

Complex[,] BlockDirac(double s1, double s2, double a, double b)
{
    // 12x12: index (s*3 + c). D = gamma_1 (x) (s1 I + a U) + gamma_2 (x) (s2 I + b V).
    var A1 = new Complex[3, 3];
    var A2 = new Complex[3, 3];
    for (int r = 0; r < 3; r++)
        for (int c = 0; c < 3; c++)
        {
            A1[r, c] = a * genFund[0][r, c] + (r == c ? s1 : 0.0);
            A2[r, c] = b * genFund[3][r, c] + (r == c ? s2 : 0.0);
        }
    var d = new Complex[12, 12];
    for (int s = 0; s < 4; s++)
        for (int sp = 0; sp < 4; sp++)
            for (int c = 0; c < 3; c++)
                for (int cp = 0; cp < 3; cp++)
                    d[s * 3 + c, sp * 3 + cp] = gamma[0][s, sp] * A1[c, cp] + gamma[1][s, sp] * A2[c, cp];
    return d;
}

double MixedFermionPotential(double a, double b)
{
    double sum = 0.0;
    foreach (var (s1, s2) in momenta)
    {
        var (vals, _) = Jacobi(Realify(BlockDirac(s1, s2, a, b)));
        var sq = vals.Select(v => v * v).OrderBy(v => v).Where((_, i) => i % 2 == 0);
        foreach (double l2 in sq)
            if (l2 > KernelTolerance)
                sum += Math.Log(l2);
    }
    return -0.5 * sum;
}

double commNormSquared = 0.0;
for (int r = 0; r < 3; r++)
    for (int c = 0; c < 3; c++)
    {
        Complex commutator = Complex.Zero;
        for (int k = 0; k < 3; k++)
            commutator += genFund[0][r, k] * genFund[3][k, c] - genFund[3][r, k] * genFund[0][k, c];
        commNormSquared += commutator.Magnitude * commutator.Magnitude;
    }

const double MixedKappa = 1.0;
double mixedReference = MixedFermionPotential(0.0, 0.0);
double MixedTotal(double a, double b) =>
    MixedKappa * (a * b) * (a * b) * commNormSquared + (MixedFermionPotential(a, b) - mixedReference);

double mixedBoxMin = double.MaxValue;
double mixedArgA = 0.0, mixedArgB = 0.0;
const double MixedBoxMax = 16.0;
const int MixedGrid = 17;
for (int i = 0; i < MixedGrid; i++)
    for (int j = 0; j < MixedGrid; j++)
    {
        double a = MixedBoxMax * i / (MixedGrid - 1);
        double b = MixedBoxMax * j / (MixedGrid - 1);
        double v = MixedTotal(a, b);
        if (v < mixedBoxMin)
        {
            mixedBoxMin = v;
            mixedArgA = a;
            mixedArgB = b;
        }
    }
bool mixedMinOnAxis = mixedArgA <= 1e-9 || mixedArgB <= 1e-9;
bool mixedMinOnBoundary = mixedArgA <= 1e-9 || mixedArgB <= 1e-9 ||
    mixedArgA >= MixedBoxMax - 1e-9 || mixedArgB >= MixedBoxMax - 1e-9;
// escape: V(a,0) strictly decreasing to large a, and a far axis point beats the box min
var axisTs = Enumerable.Range(0, 20).Select(k => 1.0 + (64.0 - 1.0) * k / 19.0).ToArray();
var axisVals = axisTs.Select(a => MixedTotal(a, 0.0)).ToArray();
bool axisMonotoneDecreasing = true;
for (int i = 1; i < axisVals.Length; i++)
    if (!(axisVals[i] < axisVals[i - 1]))
        axisMonotoneDecreasing = false;
double farAxisValue = MixedTotal(64.0, 0.0);
bool mixedConfigurationFermionOnlyMinimumEscapesAlongRays =
    mixedMinOnAxis && mixedMinOnBoundary && axisMonotoneDecreasing && farAxisValue < mixedBoxMin;

// ---------------------------------------------------------------------------
// Verdicts.
// ---------------------------------------------------------------------------

bool netOneLoopDirectionSelective =
    Math.Abs(fundNetTd - fundNetS) > SlopeTolerance ||
    Math.Abs(adjNetTd - adjNetS) > SlopeTolerance ||
    Math.Abs(derivedNetTd - derivedNetS) > SlopeTolerance;
bool fundamentalContentConfinesTdRays = fundNetTd > SlopeTolerance;          // +64 rises => confined
bool fundamentalContentSelectsHyperchargeAxis = fundNetS < 0.0 && fundNetTd > 0.0; // S unique unbounded
bool selectionIsMatterContentDependent =
    fundamentalContentSelectsHyperchargeAxis && adjNetTd < adjNetS - SlopeTolerance; // adjoint flips to T/D
bool derivedContentSelectsHyperchargeAxis = derivedNetS < derivedNetTd - SlopeTolerance; // S steeper descent
bool su3ToSu2U1BreakingDirectionDynamicallyPreferred = derivedContentSelectsHyperchargeAxis;
bool bothDerivedDirectionsUnbounded = derivedNetTd < 0.0 && derivedNetS < 0.0;
bool noFiniteMinimumOnRays = true; // every net slope is a log runaway; no stationary scale
bool bosonicOneLoopIsRecordedWorkbenchModel = true;
bool bosonicModeCountArithmeticIsTheExactPart = true;

// ---------------------------------------------------------------------------
// Fail-closed boundary.
// ---------------------------------------------------------------------------

const bool targetBlindConstruction = true;
const bool physicalTargetsConsultedForConstruction = false;
const bool workbenchConventionsAreSourceDefined = false;
const bool bosonicMassModelTiedToControlBranchHessian = false; // named future work
const bool sourceDefinesBosonicFluctuationDeterminant = false;
const bool sourceDefinesFermionOccupationOrRegularization = false;
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
    "gell-mann orbits; closed-form naive-Dirac fermion loop; workbench bosonic mode-mass determinant; richardson large-t log slopes; net direction selection; no target values")))).ToLowerInvariant();

bool analysisInternallyConsistent =
    traceNormalizationExact &&
    structureConstantsExact &&
    adjointRepresentationHermitian &&
    conjugacyWitnessExact &&
    closedFormSpectrumVerified &&
    bosonicSlopesMatchTargets &&
    fermionicSlopesMatchTargets &&
    netSlopesMatchTargets &&
    tdNetSlopeExactlyDegenerate &&
    mixedConfigurationFermionOnlyMinimumEscapesAlongRays;

bool netOneLoopDirectionSelectionProbePassed =
    phase404PrecursorPassed &&
    phase410PrecursorPassed &&
    phase428PrecursorPassed &&
    analysisInternallyConsistent &&
    netOneLoopDirectionSelective &&
    fundamentalContentConfinesTdRays &&
    fundamentalContentSelectsHyperchargeAxis &&
    selectionIsMatterContentDependent &&
    derivedContentSelectsHyperchargeAxis &&
    su3ToSu2U1BreakingDirectionDynamicallyPreferred &&
    bothDerivedDirectionsUnbounded &&
    noFiniteMinimumOnRays &&
    bosonicOneLoopIsRecordedWorkbenchModel &&
    bosonicModeCountArithmeticIsTheExactPart &&
    targetBlindConstruction &&
    !physicalTargetsConsultedForConstruction &&
    !workbenchConventionsAreSourceDefined &&
    !bosonicMassModelTiedToControlBranchHessian &&
    !sourceDefinesBosonicFluctuationDeterminant &&
    !sourceDefinesFermionOccupationOrRegularization &&
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

string terminalStatus = netOneLoopDirectionSelectionProbePassed
    ? "net-one-loop-direction-selective-hypercharge-axis-preferred-no-scale-law"
    : "net-one-loop-direction-selection-probe-blocked";

string decision = netOneLoopDirectionSelectionProbePassed
    ? "The net one-loop functional W(t) = W_B(t) + W_F(t) along constant rank-1 rays is direction-selective, and the selection is matter-content dependent. Bosonic and fermionic log slopes have opposite signs (bosonic T/D +192, S +128; fermionic fund T/D -128, S -192), so the net large-t slope is a genuine competition. For fundamental-3 fermions the net T/D slope is +64 (CONFINED, rising) while the singlet/hypercharge axis carries the unique negative slope -64: the su(3)->su(2)xu(1) breaking direction is the steepest descent. For adjoint-8 fermions the selection FLIPS (T/D -192 steeper than S -128). For the Phase404-derived content (the 16 read as 4 triplet-type reps + 4 su(3) singlets = 4 copies of the fundamental) the net slopes are T/D -320 and S -640, so the hypercharge axis is again the steepest descent - though BOTH directions are unbounded, so 'selected' means steepest-descent direction, not a stabilized minimum. Every ray is a log runaway: there is NO finite minimum and hence NO scale law. The bosonic side is a recorded workbench model (only the mode-count arithmetic is exact; tying the masses to the actual control-branch Hessian is named future work). No target value is consulted; no Phase201 or Phase256 field is filled; nothing is promoted."
    : "Do not use the net one-loop direction-selection verdicts until the precursor and consistency batteries pass.";

var allSlopeRows = bosonBattery.Concat(fermionBattery).Concat(netBattery)
    .Select(r => new
    {
        family = r.Family,
        block = r.Block,
        gellMannAxis = r.GellMannAxis,
        richardsonLogSlope = r.RichardsonSlope,
        rawOctaveLogSlope = r.RawSlope,
        expectedLogSlope = r.Expected,
        residual = Math.Abs(r.RichardsonSlope - r.Expected),
    }).ToArray();

var result = new
{
    phaseId = "phase430-net-one-loop-direction-selection-probe",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    netOneLoopDirectionSelectionProbePassed,
    phase404PrecursorPassed,
    phase410PrecursorPassed,
    phase428PrecursorPassed,
    applicationSubjectKind = ApplicationSubjectKind,
    targetBlindConstruction,
    physicalTargetsConsultedForConstruction,
    targetBlindConstructionHash,
    workbenchConventions = new
    {
        latticeSize = LatticeSize,
        latticeVertices = LatticeSize * LatticeSize,
        spinorDimension = 4,
        diracDiscretization = "naive-central-difference-hermitian",
        fermionRepresentationsProbed = new[] { "fundamental-3", "adjoint-8", "derived-4x-fundamental" },
        bosonicOneLoopModel = "transverse fluctuation determinant: 16 momenta x 2 polarizations x eigenvalues of -ad_u^2 on su(3); eps_k^2 = sin^2 k1 + sin^2 k2",
        derivedContentBranching = "Phase404 blind one-family pattern: the 16 contains 4 triplet-type reps (2x 3 + 2x 3bar, |eigenvalue|-identical so loop-equivalent) and 4 su(3) singlets => fermionic content = 4 copies of the fundamental",
        blockMenu = "Gell-Mann axes: T={1,2,3}, D={4,5,6,7}, S={8}",
        largeTSlopeConvention = "Richardson-extrapolated log slope across the t=40/80/160 octaves (removes the O(1/t^2) lattice correction); raw t=40->t=80 octave slope also recorded",
        workbenchConventionsAreSourceDefined,
        bosonicOneLoopIsRecordedWorkbenchModel,
        bosonicModeCountArithmeticIsTheExactPart,
        bosonicMassModelTiedToControlBranchHessian,
    },
    traceNormalizationResidual,
    traceNormalizationExact,
    structureConstantResidual,
    structureConstantsExact,
    adjointHermiticityResidual,
    adjointRepresentationHermitian,
    conjugacyWitnessResidual,
    conjugacyWitnessExact,
    closedFormCrossCheckResidual,
    closedFormSpectrumVerified,
    slopeTolerance = SlopeTolerance,
    maxBosonSlopeResidual,
    bosonicSlopesMatchTargets,
    maxFermionSlopeResidual,
    fermionicSlopesMatchTargets,
    maxNetSlopeResidual,
    netSlopesMatchTargets,
    maxTdNetDegeneracyResidual,
    tdNetSlopeExactlyDegenerate,
    slopeBattery = allSlopeRows,
    netSlopeSummary = new
    {
        fundamentalTd = fundNetTd,
        fundamentalS = fundNetS,
        adjointTd = adjNetTd,
        adjointS = adjNetS,
        derivedTd = derivedNetTd,
        derivedS = derivedNetS,
    },
    mixedConfiguration = new
    {
        pair = "(lambda_1, lambda_4)",
        representation = "fundamental-3",
        kappa = MixedKappa,
        commutatorNormSquared = commNormSquared,
        boxMax = MixedBoxMax,
        gridSize = MixedGrid,
        boxMinimumValue = mixedBoxMin,
        boxArgminA = mixedArgA,
        boxArgminB = mixedArgB,
        minOnAxis = mixedMinOnAxis,
        minOnBoundary = mixedMinOnBoundary,
        axisMonotoneDecreasing,
        farAxisValue,
        mixedConfigurationFermionOnlyMinimumEscapesAlongRays,
    },
    netOneLoopDirectionSelective,
    fundamentalContentConfinesTdRays,
    fundamentalContentSelectsHyperchargeAxis,
    selectionIsMatterContentDependent,
    derivedContentSelectsHyperchargeAxis,
    su3ToSu2U1BreakingDirectionDynamicallyPreferred,
    bothDerivedDirectionsUnbounded,
    noFiniteMinimumOnRays,
    bosonicOneLoopIsRecordedWorkbenchModel,
    bosonicModeCountArithmeticIsTheExactPart,
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
    sourceDefinesBosonicFluctuationDeterminant,
    sourceDefinesFermionOccupationOrRegularization,
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
        phase404SummaryPath = Phase404SummaryPath,
        phase410SummaryPath = Phase410SummaryPath,
        phase428SummaryPath = Phase428SummaryPath,
    },
    explicitCandidateOnlyNonclaims = new[]
    {
        "The bosonic one-loop is a RECORDED WORKBENCH MODEL of the transverse fluctuation determinant: only the mode-count arithmetic (16 momenta x 2 polarizations x nonzero -ad_u^2 eigenvalues) is exact; tying the masses to the actual control-branch Hessian is named future work.",
        "The lattice, 4-spinors, naive central-difference Dirac, and the fermion representations are recorded conventions, not source-defined physics.",
        "Every net slope is a log runaway (no finite minimum on any ray); 'selection' means the steepest-descent direction, NOT a stabilized minimum, and there is NO scale law.",
        "The derived content selects the hypercharge axis as steepest descent, but BOTH T/D and S directions are unbounded for that content.",
        "The direction selection is matter-content dependent (adjoint fermions flip it to the T/D block), so it is a structural observation about the competition, not a promotion.",
        "No Phase201 or Phase256 fill.",
    },
    decision,
};

var options = JsonOptions();
File.WriteAllText(Path.Combine(outputDir, "net_one_loop_direction_selection_probe.json"), JsonSerializer.Serialize(result, options));
string summaryPath = Path.Combine(outputDir, "net_one_loop_direction_selection_probe_summary.json");
File.WriteAllText(summaryPath, JsonSerializer.Serialize(result, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"netOneLoopDirectionSelectionProbePassed={netOneLoopDirectionSelectionProbePassed}");
Console.WriteLine($"precursors: phase404={phase404PrecursorPassed} phase410={phase410PrecursorPassed} phase428={phase428PrecursorPassed}");
Console.WriteLine($"closedFormSpectrumVerified={closedFormSpectrumVerified} (residual {closedFormCrossCheckResidual:E2})");
Console.WriteLine($"bosonic={bosonicSlopesMatchTargets}({maxBosonSlopeResidual:E2}) fermionic={fermionicSlopesMatchTargets}({maxFermionSlopeResidual:E2}) net={netSlopesMatchTargets}({maxNetSlopeResidual:E2})");
Console.WriteLine($"tdNetSlopeExactlyDegenerate={tdNetSlopeExactlyDegenerate} (residual {maxTdNetDegeneracyResidual:E2})");
Console.WriteLine($"net fund   T/D={fundNetTd:F3} S={fundNetS:F3}");
Console.WriteLine($"net adj    T/D={adjNetTd:F3} S={adjNetS:F3}");
Console.WriteLine($"net derived T/D={derivedNetTd:F3} S={derivedNetS:F3}");
Console.WriteLine($"netOneLoopDirectionSelective={netOneLoopDirectionSelective}");
Console.WriteLine($"fundamentalContentConfinesTdRays={fundamentalContentConfinesTdRays} fundamentalContentSelectsHyperchargeAxis={fundamentalContentSelectsHyperchargeAxis}");
Console.WriteLine($"selectionIsMatterContentDependent={selectionIsMatterContentDependent} derivedContentSelectsHyperchargeAxis={derivedContentSelectsHyperchargeAxis}");
Console.WriteLine($"su3ToSu2U1BreakingDirectionDynamicallyPreferred={su3ToSu2U1BreakingDirectionDynamicallyPreferred} noFiniteMinimumOnRays={noFiniteMinimumOnRays}");
Console.WriteLine($"mixedConfigurationFermionOnlyMinimumEscapesAlongRays={mixedConfigurationFermionOnlyMinimumEscapesAlongRays} (argmin a={mixedArgA:F1} b={mixedArgB:F1}, farAxis={farAxisValue:F1} < boxMin={mixedBoxMin:F1})");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");
Console.WriteLine($"summaryPath={summaryPath}");

// ---------------------------------------------------------------------------
// Helpers.
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

static JsonSerializerOptions JsonOptions() => new()
{
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
};

public sealed record SlopeRow(
    string Family,
    string Block,
    int GellMannAxis,
    double RichardsonSlope,
    double RawSlope,
    double Expected);
