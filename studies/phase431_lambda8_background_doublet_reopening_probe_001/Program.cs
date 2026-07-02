using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

// Phase431: lambda_8-background doublet-reopening probe (stage two of the
// Coleman-Weinberg program that began with Phase428).
//
// Phase428 proved the one-loop fermion determinant on constant rank-1 rays
// omega = t*u is an adjoint-orbit class function, so the triplet axes
// T={lambda_1,2,3} and the doublet axes D={lambda_4..7} are EXACTLY
// degenerate for any su(3)-INVARIANT fermion sector - doublet selection
// therefore requires su(3)-breaking fermionic structure. This phase supplies
// exactly one candidate breaking: a constant lambda_8 BACKGROUND
// omega_bg = t8 * (lambda_8/2). Because [lambda_8, T-axes] = 0 while
// [lambda_8, D-axes] != 0, the background leaves the triplet directions
// unbroken and breaks the doublet directions, so the class-function argument
// no longer forces T/D degeneracy.
//
// QUESTION: does a lambda_8 background make the one-loop landscape acquire a
// genuine triplet-vs-doublet distinction, and does it induce a doublet-block
// mass law (the structure Phase418 had to import by hand)?
//
// Findings (machine-verified below):
// (1) The Dirac operator on the background-plus-probe configuration
//     block-diagonalizes over lattice momentum into 4*dimG-dim blocks whose
//     spectrum is the exact closed form
//        lambda^2 = (s1 + m_c)^2 + (s2 + m_c)^2
//     with m_c the eigenvalues of the SINGLE Hermitian gauge matrix
//        M(t8,t,u) = t8*(lambda_8/2) + t*(lambda_u/2)   in the fermion rep.
//     (The same M couples on both lattice directions, so A = s1 I + M and
//     B = s2 I + M commute and the block factorizes exactly.) Verified
//     against a full 192-dim dense Hermitian Dirac solve at a noncommuting
//     background point (residual reported).
// (2) At t8 = 0 the T and D one-loop potentials are exactly degenerate
//     (Phase428 consistency battery, residual <= 1e-9). At t8 > 0 they split
//     by O(10-100) - the background REOPENS the T/D distinction.
// (3) BLOCK-DEPENDENT MASS LAW: at t8 > 0 the small-t quadratic coefficient
//     of the fermion one-loop potential is POSITIVE on the triplet probe and
//     NEGATIVE on the doublet probe (opposite signs, both finite and
//     h-stable in the fundamental rep) - the background induces a genuine,
//     direction-dependent quadratic mass law that distinguishes the doublet
//     block. In the gauge/boson sector the doublet direction is GAPPED
//     (analytic finite quadratic) while the triplet direction retains
//     probe-Higgsed massless-to-massive gauge modes (non-analytic log
//     runaway) - a second, qualitative block distinction.
//
// Fail-closed: t8 is a RECORDED CANDIDATE BACKGROUND PARAMETER, not a
// dynamically derived quantity (deriving it is the Phase430 chain's job); the
// 4x4 lattice, 4-dim spinors, naive central-difference Dirac, and
// fundamental/adjoint fermion contents are recorded workbench conventions,
// not source-defined physics; no target values are consulted; no scale law is
// produced; no Phase201 or Phase256 field can be filled.

const string DefaultOutputDir = "studies/phase431_lambda8_background_doublet_reopening_probe_001/output";
const string Phase428SummaryPath = "studies/phase428_fermion_loop_block_selection_no_go_probe_001/output/fermion_loop_block_selection_no_go_probe_summary.json";
const string Phase418SummaryPath = "studies/phase418_direction_dependent_curvature_vev_coupling_scan_001/output/direction_dependent_curvature_vev_coupling_scan_summary.json";
const string ApplicationSubjectKind = "lambda8-background-doublet-reopening-probe";

var startTime = DateTimeOffset.UtcNow;
var outputDir = Environment.GetEnvironmentVariable("PHASE431_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase428 = JsonDocument.Parse(File.ReadAllText(Phase428SummaryPath));
using var phase418 = JsonDocument.Parse(File.ReadAllText(Phase418SummaryPath));

bool phase428PrecursorPassed =
    JsonBool(phase428.RootElement, "fermionLoopBlockSelectionNoGoProbePassed") is true &&
    JsonBool(phase428.RootElement, "fermionLoopBlockSelectionMechanismClosed") is true &&
    JsonBool(phase428.RootElement, "su3BreakingFermionicStructureRequired") is true;
bool phase418PrecursorPassed =
    JsonBool(phase418.RootElement, "directionDependentCurvatureVevCouplingScanPassed") is true;

// ---------------------------------------------------------------------------
// su(3): Gell-Mann fundamental and adjoint representations with exactness
// batteries (same conventions as Phase428).
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
        for (int c3 = 0; c3 < 8; c3++)
        {
            Complex trace = Complex.Zero;
            for (int r = 0; r < 3; r++)
                for (int c = 0; c < 3; c++)
                    trace += comm[r, c] * genFund[c3][c, r];
            fabc[a, b, c3] = (-2.0 * Complex.ImaginaryOne * trace).Real;
        }
        var reconstructed = new Complex[3, 3];
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

// ---------------------------------------------------------------------------
// The physical crux of the phase: [lambda_8, T-axes] = 0 (background leaves
// the triplet unbroken) while [lambda_8, D-axes] != 0 (background breaks the
// doublet). These commutator norms are what make T and D inequivalent in the
// lambda_8 background even though they are adjoint-conjugate at t8 = 0.
// ---------------------------------------------------------------------------

double CommutatorNorm(Complex[,] x, Complex[,] y)
{
    int n = x.GetLength(0);
    double max = 0.0;
    for (int r = 0; r < n; r++)
        for (int c = 0; c < n; c++)
        {
            Complex sum = Complex.Zero;
            for (int k = 0; k < n; k++)
                sum += x[r, k] * y[k, c] - y[r, k] * x[k, c];
            max = Math.Max(max, sum.Magnitude);
        }
    return max;
}

double maxTripletBackgroundCommutator = 0.0;   // must be ~0
double minDoubletBackgroundCommutator = double.MaxValue; // must be > 0
for (int axis = 0; axis < 3; axis++) // lambda_1,2,3
    maxTripletBackgroundCommutator = Math.Max(maxTripletBackgroundCommutator, CommutatorNorm(genFund[7], genFund[axis]));
for (int axis = 3; axis < 7; axis++) // lambda_4,5,6,7
    minDoubletBackgroundCommutator = Math.Min(minDoubletBackgroundCommutator, CommutatorNorm(genFund[7], genFund[axis]));
bool backgroundCommutesWithTriplet = maxTripletBackgroundCommutator <= 1e-13;
bool backgroundBreaksDoublet = minDoubletBackgroundCommutator >= 0.05;

// ---------------------------------------------------------------------------
// Workbench: 4x4 periodic lattice, 4-dim Euclidean spinors, naive central-
// difference Hermitian Dirac. Block spectrum on the background-plus-probe
// configuration omega = t8*(lambda_8/2) + t*(lambda_u/2):
//     lambda^2 = (s1 + m_c)^2 + (s2 + m_c)^2, multiplicity 4 per (k, m_c),
// with s_mu = sin(2 pi n_mu / 4) and m_c the eigenvalues of the fermion-rep
// gauge matrix M(t8,t,u).
// ---------------------------------------------------------------------------

const int LatticeSize = 4;
var sineValues = Enumerable.Range(0, LatticeSize).Select(n => Math.Sin(2.0 * Math.PI * n / LatticeSize)).ToArray();
var momenta = new List<(double S1, double S2)>();
for (int n1 = 0; n1 < LatticeSize; n1++)
    for (int n2 = 0; n2 < LatticeSize; n2++)
        momenta.Add((sineValues[n1], sineValues[n2]));

// eigenvalues of the fermion-rep gauge matrix M = t8*U8 + t*U_u
double[] GaugeMatrixEigenvalues(double t8, double t, int axis, Complex[][,] gen)
{
    int n = gen[axis].GetLength(0);
    var m = new Complex[n, n];
    for (int r = 0; r < n; r++)
        for (int c = 0; c < n; c++)
            m[r, c] = t8 * gen[7][r, c] + t * gen[axis][r, c];
    var (values, _) = Jacobi(Realify(m));
    return values.OrderBy(v => v).Where((_, i) => i % 2 == 0).ToArray(); // realified doubling
}

const double KernelTolerance = 1e-12;

double FermionPotential(double t8, double t, int axis, Complex[][,] gen)
{
    var mc = GaugeMatrixEigenvalues(t8, t, axis, gen);
    double sum = 0.0;
    foreach (var (s1, s2) in momenta)
        foreach (double m in mc)
        {
            double a1 = s1 + m, a2 = s2 + m;
            double lambdaSquared = a1 * a1 + a2 * a2;
            if (lambdaSquared > KernelTolerance)
                sum += 4.0 * Math.Log(lambdaSquared);
        }
    return -0.5 * sum;
}

// bosonic structural model: gauge-fluctuation mass^2 = eigenvalues of
// -(ad(t8*u8 + t*u))^2 = squares of the adjoint-charge eigenvalues; lattice
// dispersion eps_k^2 = sin^2 k1 + sin^2 k2; 2 polarizations.
double[] BosonMassSquared(double t8, double t, int axis)
{
    var m = new Complex[8, 8];
    for (int r = 0; r < 8; r++)
        for (int c = 0; c < 8; c++)
            m[r, c] = t8 * genAdjoint[7][r, c] + t * genAdjoint[axis][r, c];
    var (values, _) = Jacobi(Realify(m));
    var rho = values.OrderBy(v => v).Where((_, i) => i % 2 == 0).ToArray();
    return rho.Select(v => v * v).ToArray();
}

double BosonPotential(double t8, double t, int axis)
{
    var m2 = BosonMassSquared(t8, t, axis);
    double sum = 0.0;
    foreach (var (s1, s2) in momenta)
    {
        double eps2 = s1 * s1 + s2 * s2;
        foreach (double mm in m2)
        {
            double val = eps2 + mm;
            if (val > KernelTolerance)
                sum += 2.0 * Math.Log(val);
        }
    }
    return 0.5 * sum;
}

// relative-to-t=0 potentials at fixed t8
double FermionRel(double t8, double t, int axis, Complex[][,] gen) =>
    FermionPotential(t8, t, axis, gen) - FermionPotential(t8, 0.0, axis, gen);
double BosonRel(double t8, double t, int axis) =>
    BosonPotential(t8, t, axis) - BosonPotential(t8, 0.0, axis);
double NetRel(double t8, double t, int axis, Complex[][,] gen) =>
    BosonRel(t8, t, axis) + FermionRel(t8, t, axis, gen);

// ---------------------------------------------------------------------------
// Dense cross-check: the closed block spectrum vs a full 192-dim Hermitian
// Dirac solve at a NONCOMMUTING background point (t8=1, t=0.7, u=lambda_4).
// ---------------------------------------------------------------------------

double closedFormCrossCheckResidual;
{
    const double t8Sample = 1.0, tSample = 0.7;
    const int axisSample = 3; // lambda_4 (doublet, noncommuting with lambda_8)
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
    var gamma = new Complex[2][,];
    gamma[0] = new Complex[4, 4];
    gamma[1] = new Complex[4, 4];
    gamma[0][0, 2] = 1; gamma[0][1, 3] = 1; gamma[0][2, 0] = 1; gamma[0][3, 1] = 1;
    gamma[1][0, 2] = -Complex.ImaginaryOne; gamma[1][1, 3] = -Complex.ImaginaryOne;
    gamma[1][2, 0] = Complex.ImaginaryOne; gamma[1][3, 1] = Complex.ImaginaryOne;

    var mMatrix = new Complex[dimG, dimG];
    for (int r = 0; r < dimG; r++)
        for (int c = 0; c < dimG; c++)
            mMatrix[r, c] = t8Sample * genFund[7][r, c] + tSample * genFund[axisSample][r, c];

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
                            if (mMatrix[g, gp] != Complex.Zero)
                                dirac[(s * vertices + v) * dimG + g, (sp * vertices + v) * dimG + gp] +=
                                    gamma[mu][s, sp] * mMatrix[g, gp];
            }
    var (denseValues, _) = Jacobi(Realify(dirac));
    var denseSquared = denseValues.Select(v => v * v).OrderBy(v => v).Where((_, i) => i % 2 == 0).OrderBy(v => v).ToArray();

    var mc = GaugeMatrixEigenvalues(t8Sample, tSample, axisSample, genFund);
    var closed = new List<double>();
    foreach (var (s1, s2) in momenta)
        foreach (double m in mc)
        {
            double a1 = s1 + m, a2 = s2 + m;
            double l2 = a1 * a1 + a2 * a2;
            for (int k = 0; k < 4; k++)
                closed.Add(l2);
        }
    var closedSorted = closed.OrderBy(v => v).ToArray();
    double res = 0.0;
    for (int i = 0; i < closedSorted.Length; i++)
        res = Math.Max(res, Math.Abs(closedSorted[i] - denseSquared[i]));
    closedFormCrossCheckResidual = res;
}
bool closedFormBlockSpectrumVerified = closedFormCrossCheckResidual <= 1e-9;

// ---------------------------------------------------------------------------
// Grids and probe menu.
// ---------------------------------------------------------------------------

var t8Grid = new[] { 0.0, 0.5, 1.0, 2.0 };
var tDegeneracyGrid = Enumerable.Range(1, 15).Select(i => 1.5 * i / 15.0).ToArray(); // (0,1.5], step 0.1
var probeAxes = new (string Block, int Axis)[]
{
    ("T", 0), // lambda_1
    ("T", 2), // lambda_3 (second triplet representative)
    ("D", 3), // lambda_4
    ("D", 4), // lambda_5
};
var fermionContents = new (string Name, Complex[][,] Gen)[]
{
    ("fundamental-3", genFund),
    ("adjoint-8", genAdjoint),
};

// central-difference quadratic coefficient A of W ~ A t^2 (+ ...):
//   A = 0.5 * W''(0) with W''(0) ~ (W(h) + W(-h)) / h^2 since W(0) = 0.
// analyticAtOrigin: A is stable as h -> 0 (a genuine finite mass); a
// probe-Higgsed massless mode makes W ~ log t and A diverges as 1/h^2.
const double HessianH = 1e-3, HessianHFine = 1e-4;
(double A, bool Analytic) QuadraticCoefficient(Func<double, double> relPotentialAtT, double t8)
{
    double Coef(double h) => 0.5 * (relPotentialAtT(h) + relPotentialAtT(-h)) / (h * h);
    double coarse = Coef(HessianH);
    double fine = Coef(HessianHFine);
    bool analytic = Math.Abs(fine) <= 10.0 * Math.Abs(coarse) + 10.0;
    return (coarse, analytic);
}

// count of gauge modes that leave zero (get Higgsed by the probe) at the
// origin - the source of the triplet non-analyticity.
int ProbeMasslessLift(double t8, int axis)
{
    var at0 = BosonMassSquared(t8, 0.0, axis).OrderBy(v => v).ToArray();
    var atT = BosonMassSquared(t8, HessianH, axis).OrderBy(v => v).ToArray();
    int count = 0;
    for (int i = 0; i < at0.Length; i++)
        if (at0[i] < 1e-10 && atT[i] > 1e-10)
            count++;
    return count;
}

// ---------------------------------------------------------------------------
// (1) Degeneracy breaking by the background: max_t |W(T) - W(D)| per t8, for
// the fermion sector and the net sector, in both fermion contents.
// ---------------------------------------------------------------------------

var degeneracyRows = new List<DegeneracyRow>();
double maxT8ZeroDegeneracyResidual = 0.0;
double minBrokenSeparationAtNonzeroT8 = double.MaxValue;
foreach (var (repName, gen) in fermionContents)
    foreach (var (sector, relPotential) in new (string, Func<double, double, int, double>)[]
    {
        ("fermion", (t8, t, axis) => FermionRel(t8, t, axis, gen)),
        ("net", (t8, t, axis) => NetRel(t8, t, axis, gen)),
    })
    {
        foreach (double t8 in t8Grid)
        {
            double maxDiff = 0.0;
            foreach (double t in tDegeneracyGrid)
                maxDiff = Math.Max(maxDiff, Math.Abs(relPotential(t8, t, 0) - relPotential(t8, t, 3)));
            degeneracyRows.Add(new DegeneracyRow(repName, sector, t8, maxDiff));
            if (t8 == 0.0)
                maxT8ZeroDegeneracyResidual = Math.Max(maxT8ZeroDegeneracyResidual, maxDiff);
            else
                minBrokenSeparationAtNonzeroT8 = Math.Min(minBrokenSeparationAtNonzeroT8, maxDiff);
        }
    }
const double DegeneracyBreakThreshold = 1.0;
bool tdDegenerateAtZeroBackground = maxT8ZeroDegeneracyResidual <= 1e-9;
bool tdDegeneracyBrokenByBackground = tdDegenerateAtZeroBackground && minBrokenSeparationAtNonzeroT8 >= DegeneracyBreakThreshold;

// ---------------------------------------------------------------------------
// (2) Block-dependent mass law: quadratic coefficient A per (content, axis,
// t8) for the fermion, boson, and net sectors.
// ---------------------------------------------------------------------------

var massLawRows = new List<MassLawRow>();
foreach (var (repName, gen) in fermionContents)
    foreach (var (block, axis) in probeAxes)
        foreach (double t8 in t8Grid)
        {
            var (aF, analyticF) = QuadraticCoefficient(t => FermionRel(t8, t, axis, gen), t8);
            var (aB, analyticB) = QuadraticCoefficient(t => BosonRel(t8, t, axis), t8);
            var (aN, analyticN) = QuadraticCoefficient(t => NetRel(t8, t, axis, gen), t8);
            int lift = ProbeMasslessLift(t8, axis);
            massLawRows.Add(new MassLawRow(repName, block, axis + 1, t8,
                aF, analyticF, aB, analyticB, aN, analyticN, lift));
        }

double GetA(string rep, int gellMannAxis, double t8, Func<MassLawRow, double> sel) =>
    sel(massLawRows.First(r => r.Representation == rep && r.GellMannAxis == gellMannAxis && r.T8 == t8));
bool GetAnalytic(string rep, int gellMannAxis, double t8, Func<MassLawRow, bool> sel) =>
    sel(massLawRows.First(r => r.Representation == rep && r.GellMannAxis == gellMannAxis && r.T8 == t8));

// Fundamental fermion sector: both T (lambda_1) and D (lambda_4) are analytic
// and finite; the mass law is block-dependent iff they differ substantially
// and carry OPPOSITE signs at every t8 > 0, and are degenerate at t8 = 0.
const double MassLawBlockThreshold = 1.0;
bool fermionMassLawDegenerateAtZeroBackground =
    Math.Abs(GetA("fundamental-3", 1, 0.0, r => r.FermionA) - GetA("fundamental-3", 4, 0.0, r => r.FermionA)) <= 1e-6;
bool fermionMassLawBlockDependent = true;
bool fermionTripletCurvaturePositiveDoubletNegative = true;
foreach (double t8 in new[] { 0.5, 1.0, 2.0 })
{
    double aT = GetA("fundamental-3", 1, t8, r => r.FermionA);
    double aD = GetA("fundamental-3", 4, t8, r => r.FermionA);
    bool analyticT = GetAnalytic("fundamental-3", 1, t8, r => r.FermionAnalytic);
    bool analyticD = GetAnalytic("fundamental-3", 4, t8, r => r.FermionAnalytic);
    fermionMassLawBlockDependent &= analyticT && analyticD && Math.Abs(aT - aD) >= MassLawBlockThreshold;
    fermionTripletCurvaturePositiveDoubletNegative &= aT > 0.0 && aD < 0.0;
}
bool backgroundInducesBlockDependentMassLaw =
    fermionMassLawDegenerateAtZeroBackground &&
    fermionMassLawBlockDependent &&
    fermionTripletCurvaturePositiveDoubletNegative;

// Gauge/boson sector: the doublet direction is gapped (analytic finite
// quadratic, no probe-Higgsed lift) while the triplet direction is not
// (non-analytic, lift > 0) at every t8 > 0.
bool doubletDirectionGaugeGappedTripletNot = true;
foreach (double t8 in new[] { 0.5, 1.0, 2.0 })
{
    bool dAnalytic = GetAnalytic("fundamental-3", 4, t8, r => r.BosonAnalytic);
    bool tAnalytic = GetAnalytic("fundamental-3", 1, t8, r => r.BosonAnalytic);
    int tLift = (int)GetA("fundamental-3", 1, t8, r => r.ProbeMasslessLift);
    int dLift = (int)GetA("fundamental-3", 4, t8, r => r.ProbeMasslessLift);
    doubletDirectionGaugeGappedTripletNot &= dAnalytic && !tAnalytic && tLift > 0 && dLift == 0;
}

// ---------------------------------------------------------------------------
// (3) Ordering: sign of W(D) - W(T) at representative finite t (recorded, not
// used to claim selection - the ordering is t8- and t-dependent).
// ---------------------------------------------------------------------------

var orderingRows = new List<OrderingRow>();
foreach (var (repName, gen) in fermionContents)
    foreach (double t8 in new[] { 0.5, 1.0, 2.0 })
        foreach (double t in new[] { 0.75, 1.5 })
            foreach (var (sector, relPotential) in new (string, Func<double, double, int, double>)[]
            {
                ("fermion", (a, b, axis) => FermionRel(a, b, axis, gen)),
                ("net", (a, b, axis) => NetRel(a, b, axis, gen)),
            })
            {
                double dW = relPotential(t8, t, 3) - relPotential(t8, t, 0);
                orderingRows.Add(new OrderingRow(repName, sector, t8, t, dW,
                    dW < 0 ? "doublet-lower" : "doublet-higher"));
            }

// ---------------------------------------------------------------------------
// Consistency and fail-closed boundary.
// ---------------------------------------------------------------------------

bool analysisInternallyConsistent =
    traceNormalizationExact &&
    structureConstantsExact &&
    adjointRepresentationHermitian &&
    backgroundCommutesWithTriplet &&
    backgroundBreaksDoublet &&
    closedFormBlockSpectrumVerified &&
    tdDegenerateAtZeroBackground &&
    fermionMassLawDegenerateAtZeroBackground;

const bool targetBlindConstruction = true;
const bool physicalTargetsConsultedForConstruction = false;
const bool workbenchConventionsAreSourceDefined = false;
const bool backgroundParameterT8IsRecordedCandidateOnly = true;   // not dynamically derived
const bool backgroundParameterT8DynamicallyDerived = false;       // that is the Phase430 chain's job
const bool sourceDefinesSu3BreakingFermionicStructure = false;
const bool sourceDefinesBackgroundScaleOrOccupation = false;
const bool scaleLawProduced = false;
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
    "lambda_8 background t8*(lambda_8/2); commutator [lambda_8,T]=0 [lambda_8,D]!=0; closed-form 4x4 naive-Dirac background+probe block spectrum; relative one-loop fermion/boson/net potentials; candidate background parameter only; no target values")))).ToLowerInvariant();

bool lambda8BackgroundDoubletReopeningProbePassed =
    phase428PrecursorPassed &&
    phase418PrecursorPassed &&
    analysisInternallyConsistent &&
    closedFormBlockSpectrumVerified &&
    tdDegeneracyBrokenByBackground &&
    backgroundInducesBlockDependentMassLaw &&
    doubletDirectionGaugeGappedTripletNot &&
    targetBlindConstruction &&
    !physicalTargetsConsultedForConstruction &&
    !workbenchConventionsAreSourceDefined &&
    backgroundParameterT8IsRecordedCandidateOnly &&
    !backgroundParameterT8DynamicallyDerived &&
    !sourceDefinesSu3BreakingFermionicStructure &&
    !sourceDefinesBackgroundScaleOrOccupation &&
    !scaleLawProduced &&
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

string terminalStatus = lambda8BackgroundDoubletReopeningProbePassed
    ? "lambda8-background-reopens-td-distinction-and-induces-doublet-block-mass-law-candidate-only"
    : "lambda8-background-doublet-reopening-probe-blocked";

string decision = lambda8BackgroundDoubletReopeningProbePassed
    ? "A constant lambda_8 background omega_bg = t8*(lambda_8/2) reopens the triplet-vs-doublet distinction that Phase428 proved is exactly degenerate for any su(3)-invariant fermion sector. The mechanism is transparent: [lambda_8, T-axes] = 0 (the background leaves the triplet unbroken) while [lambda_8, D-axes] != 0 (it breaks the doublet). On the recorded 4x4 naive-Dirac workbench the one-loop potentials, exactly degenerate at t8 = 0 (residual <= 1e-9), split by O(10-100) once t8 > 0. The background induces a genuine BLOCK-DEPENDENT MASS LAW: the small-t quadratic coefficient of the fermion one-loop potential is POSITIVE on the triplet probe and NEGATIVE on the doublet probe (opposite signs, both finite and h-stable in the fundamental rep), and in the gauge/boson sector the doublet direction is gapped (analytic finite quadratic) while the triplet retains probe-Higgsed massless-to-massive modes (non-analytic log runaway). This is exactly the kind of direction-dependent quadratic structure Phase418 had to import by hand - but t8 is a RECORDED CANDIDATE BACKGROUND PARAMETER, not a dynamically derived scale (deriving it is the Phase430 chain's job). No scale law is produced, no target value is consulted, and no Phase201 or Phase256 field is filled."
    : "Do not use the reopening verdicts until the precursor and consistency batteries pass.";

var runtimeSeconds = (DateTimeOffset.UtcNow - startTime).TotalSeconds;

var result = new
{
    phaseId = "phase431-lambda8-background-doublet-reopening-probe",
    generatedAt = DateTimeOffset.UtcNow,
    runtimeSeconds,
    terminalStatus,
    lambda8BackgroundDoubletReopeningProbePassed,
    phase428PrecursorPassed,
    phase418PrecursorPassed,
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
        fermionContentsProbed = new[] { "fundamental-3", "adjoint-8" },
        backgroundDirection = "lambda_8/2 (singlet axis)",
        probeMenu = "T={lambda_1,lambda_3}, D={lambda_4,lambda_5} representatives",
        t8Grid,
        tGridMax = 1.5,
        workbenchConventionsAreSourceDefined,
    },
    traceNormalizationResidual,
    traceNormalizationExact,
    structureConstantResidual,
    structureConstantsExact,
    adjointHermiticityResidual,
    adjointRepresentationHermitian,
    maxTripletBackgroundCommutator,
    backgroundCommutesWithTriplet,
    minDoubletBackgroundCommutator,
    backgroundBreaksDoublet,
    closedFormCrossCheckResidual,
    closedFormBlockSpectrumVerified,
    maxT8ZeroDegeneracyResidual,
    tdDegenerateAtZeroBackground,
    minBrokenSeparationAtNonzeroT8,
    degeneracyBreakThreshold = DegeneracyBreakThreshold,
    tdDegeneracyBrokenByBackground,
    degeneracyRows = degeneracyRows.Select(r => new
    {
        representation = r.Representation,
        sector = r.Sector,
        t8 = r.T8,
        maxTripletDoubletPotentialDifference = r.MaxTdDifference,
    }).ToArray(),
    massLawBlockThreshold = MassLawBlockThreshold,
    fermionMassLawDegenerateAtZeroBackground,
    fermionMassLawBlockDependent,
    fermionTripletCurvaturePositiveDoubletNegative,
    backgroundInducesBlockDependentMassLaw,
    doubletDirectionGaugeGappedTripletNot,
    massLawRows = massLawRows.Select(r => new
    {
        representation = r.Representation,
        block = r.Block,
        gellMannAxis = r.GellMannAxis,
        t8 = r.T8,
        fermionQuadraticCoefficient = r.FermionA,
        fermionAnalyticAtOrigin = r.FermionAnalytic,
        bosonQuadraticCoefficient = r.BosonA,
        bosonAnalyticAtOrigin = r.BosonAnalytic,
        netQuadraticCoefficient = r.NetA,
        netAnalyticAtOrigin = r.NetAnalytic,
        probeMasslessLift = (int)r.ProbeMasslessLift,
    }).ToArray(),
    orderingRows = orderingRows.Select(r => new
    {
        representation = r.Representation,
        sector = r.Sector,
        t8 = r.T8,
        t = r.T,
        doubletMinusTripletPotential = r.DoubletMinusTriplet,
        ordering = r.Ordering,
    }).ToArray(),
    analysisInternallyConsistent,
    backgroundParameterT8IsRecordedCandidateOnly,
    backgroundParameterT8DynamicallyDerived,
    sourceDefinesSu3BreakingFermionicStructure,
    sourceDefinesBackgroundScaleOrOccupation,
    scaleLawProduced,
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
        phase428SummaryPath = Phase428SummaryPath,
        phase418SummaryPath = Phase418SummaryPath,
    },
    explicitCandidateOnlyNonclaims = new[]
    {
        "t8 is a RECORDED CANDIDATE BACKGROUND PARAMETER swept over a grid, not a dynamically derived scale; deriving it is the Phase430 chain's job. No dimensionful anchor and no scale law appear in this phase.",
        "The workbench (4x4 lattice, 4-spinors, naive central-difference Dirac, fundamental/adjoint fermions) and the bosonic structural model are recorded conventions, not source-defined physics; the commutator mechanism [lambda_8,T]=0 vs [lambda_8,D]!=0 that drives the reopening is representation-independent.",
        "The lambda_8 background is one candidate su(3)-breaking structure; no reviewed source defines it, an occupation/chemical potential, or a regularization scheme.",
        "The triplet-vs-doublet ordering of the one-loop potential is t8- and t-dependent (recorded, not used to claim dynamical selection); the phase demonstrates a block-dependent mass law, not a completed vacuum.",
        "The gauge/boson non-analyticity on the triplet direction is the phase428 massless-mode log runaway of the probe itself; the doublet gapping is the new background effect.",
        "No Phase201 or Phase256 fill; nothing is promoted.",
    },
    decision,
};

var options = JsonOptions();
File.WriteAllText(Path.Combine(outputDir, "lambda8_background_doublet_reopening_probe.json"), JsonSerializer.Serialize(result, options));
string summaryPath = Path.Combine(outputDir, "lambda8_background_doublet_reopening_probe_summary.json");
File.WriteAllText(summaryPath, JsonSerializer.Serialize(result, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"lambda8BackgroundDoubletReopeningProbePassed={lambda8BackgroundDoubletReopeningProbePassed}");
Console.WriteLine($"phase428PrecursorPassed={phase428PrecursorPassed} phase418PrecursorPassed={phase418PrecursorPassed}");
Console.WriteLine($"backgroundCommutesWithTriplet={backgroundCommutesWithTriplet} ({maxTripletBackgroundCommutator:E2}) backgroundBreaksDoublet={backgroundBreaksDoublet} ({minDoubletBackgroundCommutator:F4})");
Console.WriteLine($"closedFormBlockSpectrumVerified={closedFormBlockSpectrumVerified} (residual {closedFormCrossCheckResidual:E2})");
Console.WriteLine($"tdDegenerateAtZeroBackground={tdDegenerateAtZeroBackground} ({maxT8ZeroDegeneracyResidual:E2}); minBrokenSeparation={minBrokenSeparationAtNonzeroT8:F3}");
Console.WriteLine($"tdDegeneracyBrokenByBackground={tdDegeneracyBrokenByBackground}");
foreach (var r in degeneracyRows.Where(r => r.Representation == "fundamental-3" && r.Sector == "net"))
    Console.WriteLine($"  net(fund) t8={r.T8}: max_t|W_T-W_D|={r.MaxTdDifference:F4}");
Console.WriteLine($"backgroundInducesBlockDependentMassLaw={backgroundInducesBlockDependentMassLaw}");
Console.WriteLine($"  (fermion fund) A_T1 / A_D4 per t8:");
foreach (double t8 in t8Grid)
    Console.WriteLine($"    t8={t8}: A_T1={GetA("fundamental-3", 1, t8, r => r.FermionA):F4} A_D4={GetA("fundamental-3", 4, t8, r => r.FermionA):F4}");
Console.WriteLine($"doubletDirectionGaugeGappedTripletNot={doubletDirectionGaugeGappedTripletNot}");
Console.WriteLine($"analysisInternallyConsistent={analysisInternallyConsistent}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract} canFillPhase201HiggsContract={canFillPhase201HiggsContract}");
Console.WriteLine($"runtimeSeconds={runtimeSeconds:F2}");
Console.WriteLine($"summaryPath={summaryPath}");

// ---------------------------------------------------------------------------
// Helpers (same realified Jacobi as Phase428).
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

public sealed record DegeneracyRow(string Representation, string Sector, double T8, double MaxTdDifference);

public sealed record MassLawRow(
    string Representation,
    string Block,
    int GellMannAxis,
    double T8,
    double FermionA,
    bool FermionAnalytic,
    double BosonA,
    bool BosonAnalytic,
    double NetA,
    bool NetAnalytic,
    double ProbeMasslessLift);

public sealed record OrderingRow(
    string Representation,
    string Sector,
    double T8,
    double T,
    double DoubletMinusTriplet,
    string Ordering);
