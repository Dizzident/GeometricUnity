using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

// Phase428: fermion-loop block-selection no-go probe (the first experiment of
// the 2026-07-01 beyond-the-literature directive).
//
// Phases 405/410/418 proved the bare bosonic control-branch objective cannot
// select a doublet VEV and explicitly named fermionic backreaction as the
// remaining internal mechanism class. This phase decides that class for the
// one-loop fermion determinant along the constant rank-1 block rays of the
// Phase418 menu (Gell-Mann axes: triplet T={1,2,3}, doublet D={4,5,6,7},
// singlet S={8}).
//
// The decision is theorem-grade and machine-verified:
// (1) CLASS FUNCTION: on constant rank-1 rays omega = t*u the Dirac spectrum
//     depends only on the eigenvalue multiset of the gauge generator U in the
//     fermion representation (closed form below), i.e. on the adjoint ORBIT
//     of u - because a constant gauge rotation commutes with the free Dirac
//     operator and conjugation preserves spectra.
// (2) ONE ORBIT FOR T AND D: lambda_1..lambda_7 are SU(3)-conjugate (explicit
//     conjugator recorded: the color-swap permutation g maps lambda_4 to
//     lambda_1 exactly), so the fermion-loop potential is EXACTLY DEGENERATE
//     between triplet and doublet directions, in every representation.
//     Only the lambda_8 orbit (the singlet axis) differs.
// (3) NO STABILIZER: along every ray the one-loop functional falls like
//     -N log t at large t (N = coupled-mode count, verified exactly), so the
//     fermion loop supplies no positive quartic stabilizer in any direction.
//
// Conclusion: an su(3)-invariant fermionic sector CANNOT produce the
// direction-dependent block mass law or the stabilizer Phase418 had to
// import; doublet selection via fermionic backreaction requires
// su(3)-breaking fermionic structure that no reviewed source defines. The
// mechanism class named by Phases 405/410/418 is CLOSED on the control
// branch.
//
// Fail-closed: the fermion representation (fundamental 3 and adjoint 8 are
// both probed), the 4x4 periodic lattice, the 4-dimensional Euclidean
// spinors, and the naive central-difference Dirac operator are recorded
// workbench conventions, not source-defined physics; no targets are
// consulted; no Phase201 or Phase256 field can be filled.

const string DefaultOutputDir = "studies/phase428_fermion_loop_block_selection_no_go_probe_001/output";
const string Phase405SummaryPath = "studies/phase405_vacuum_manifold_doublet_vev_orbit_scan_001/output/vacuum_manifold_doublet_vev_orbit_scan_summary.json";
const string Phase410SummaryPath = "studies/phase410_curvature_coupled_vev_selection_probe_001/output/curvature_coupled_vev_selection_probe_summary.json";
const string Phase418SummaryPath = "studies/phase418_direction_dependent_curvature_vev_coupling_scan_001/output/direction_dependent_curvature_vev_coupling_scan_summary.json";
const string ApplicationSubjectKind = "fermion-loop-block-selection-no-go-probe";

var outputDir = Environment.GetEnvironmentVariable("PHASE428_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase405 = JsonDocument.Parse(File.ReadAllText(Phase405SummaryPath));
using var phase410 = JsonDocument.Parse(File.ReadAllText(Phase410SummaryPath));
using var phase418 = JsonDocument.Parse(File.ReadAllText(Phase418SummaryPath));

bool phase405PrecursorPassed =
    JsonBool(phase405.RootElement, "vacuumManifoldDoubletVevOrbitScanPassed") is true &&
    JsonBool(phase405.RootElement, "noSelectionMechanismAtConstantRank1Level") is true;
bool phase410PrecursorPassed =
    JsonBool(phase410.RootElement, "curvatureCoupledVevSelectionProbePassed") is true &&
    JsonBool(phase410.RootElement, "curvatureCouplingFailsToSelectDoublet") is true;
bool phase418PrecursorPassed =
    JsonBool(phase418.RootElement, "directionDependentCurvatureVevCouplingScanPassed") is true &&
    JsonBool(phase418.RootElement, "directionDependentCouplingSourceLawStillMissing") is true;

// ---------------------------------------------------------------------------
// su(3): Gell-Mann fundamental and adjoint representations, with exactness
// batteries.
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
        // f_abc = -2 i tr([T_a,T_b] T_c)
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

// ---------------------------------------------------------------------------
// Explicit conjugacy witness: the color-swap permutation g (colors 2 <-> 3)
// maps lambda_4 to lambda_1 exactly, so the T and D axes lie on ONE adjoint
// orbit and every conjugation-invariant functional is T/D-degenerate.
// ---------------------------------------------------------------------------

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

// ---------------------------------------------------------------------------
// Generator eigenvalue multisets per axis and representation (the ray
// spectrum's only representation input), via realified Jacobi.
// ---------------------------------------------------------------------------

double[] GeneratorEigenvalues(Complex[,] hermitian)
{
    var (values, _) = Jacobi(Realify(hermitian));
    return values.OrderBy(v => v).Where((_, i) => i % 2 == 0).ToArray(); // realified doubling: keep one copy
}

int axisCount = 8;
var eigFund = new double[axisCount][];
var eigAdjoint = new double[axisCount][];
for (int a = 0; a < axisCount; a++)
{
    eigFund[a] = GeneratorEigenvalues(genFund[a]);
    eigAdjoint[a] = GeneratorEigenvalues(genAdjoint[a]);
}

double OrbitDistance(double[] x, double[] y)
{
    var xs = x.OrderBy(v => v).ToArray();
    var ys = y.OrderBy(v => v).ToArray();
    double max = 0.0;
    for (int i = 0; i < xs.Length; i++)
        max = Math.Max(max, Math.Abs(xs[i] - ys[i]));
    return max;
}

// T and D axes (1..7 in 1-based Gell-Mann labels; indices 0..6 here) must all
// share axis 0's eigenvalue multiset; the S axis (index 7) must differ.
double tdOrbitDegeneracyResidual = 0.0;
for (int a = 1; a < 7; a++)
{
    tdOrbitDegeneracyResidual = Math.Max(tdOrbitDegeneracyResidual, OrbitDistance(eigFund[a], eigFund[0]));
    tdOrbitDegeneracyResidual = Math.Max(tdOrbitDegeneracyResidual, OrbitDistance(eigAdjoint[a], eigAdjoint[0]));
}
double singletOrbitSeparation = Math.Min(OrbitDistance(eigFund[7], eigFund[0]), OrbitDistance(eigAdjoint[7], eigAdjoint[0]));
bool tripletDoubletOnSingleAdjointOrbit = tdOrbitDegeneracyResidual <= 1e-13;
bool singletAxisOnDistinctOrbit = singletOrbitSeparation >= 0.05;

// ---------------------------------------------------------------------------
// Closed-form ray spectrum on the 4x4 periodic lattice with 4-dim Euclidean
// spinors and the naive central-difference Dirac operator:
// D(t*u) block-diagonalizes over momentum (s1, s2) and gauge eigenvalue u_c:
//   lambda^2 = (s1 + t*u_c)^2 + (s2 + t*u_c)^2, multiplicity 4 per (k, c),
// with s_mu = sin(2 pi n_mu / 4) in {0, +1, 0, -1}. Verified below against a
// direct dense Hermitian solve of the full fundamental-representation
// operator at sample points.
// ---------------------------------------------------------------------------

const int LatticeSize = 4;
var sineValues = Enumerable.Range(0, LatticeSize).Select(n => Math.Sin(2.0 * Math.PI * n / LatticeSize)).ToArray();
var momenta = new List<(double S1, double S2)>();
for (int n1 = 0; n1 < LatticeSize; n1++)
    for (int n2 = 0; n2 < LatticeSize; n2++)
        momenta.Add((sineValues[n1], sineValues[n2]));

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

// direct dense cross-check (fundamental rep, full 192-dim complex operator)
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
    var gamma = new Complex[2][,];
    gamma[0] = new Complex[4, 4];
    gamma[1] = new Complex[4, 4];
    // gamma_1 = sigma_x (x) I2, gamma_2 = sigma_y (x) I2 (Hermitian, anticommuting)
    gamma[0][0, 2] = 1; gamma[0][1, 3] = 1; gamma[0][2, 0] = 1; gamma[0][3, 1] = 1;
    gamma[1][0, 2] = -Complex.ImaginaryOne; gamma[1][1, 3] = -Complex.ImaginaryOne;
    gamma[1][2, 0] = Complex.ImaginaryOne; gamma[1][3, 1] = Complex.ImaginaryOne;

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
                        {
                            if (Math.Abs(hop[mu][v, vp]) > 1e-15)
                                for (int g = 0; g < dimG; g++)
                                    dirac[(s * vertices + v) * dimG + g, (sp * vertices + vp) * dimG + g] +=
                                        gamma[mu][s, sp] * Complex.ImaginaryOne * hop[mu][v, vp];
                            // constant gauge term: diagonal in vertices
                        }
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
// One-loop functional along the rays: relative regularized
// W(t) = -(1/2) sum log(lambda^2) over nonzero modes, minus W(0);
// exact T/D degeneracy; large-t log slopes; kinetic coefficients.
// Zero-crossing t values (where a mode hits lambda = 0 along the ray) are
// enumerated exactly from the closed form and recorded.
// ---------------------------------------------------------------------------

const double KernelTolerance = 1e-18;
double RelativePotential(double t, double[] generatorEigenvalues, double reference)
{
    double sum = 0.0;
    foreach (double lambdaSquared in RaySpectrumSquared(t, generatorEigenvalues))
        if (lambdaSquared > KernelTolerance)
            sum += Math.Log(lambdaSquared);
    return -0.5 * sum - reference;
}

double ReferencePotential(double[] generatorEigenvalues)
{
    double sum = 0.0;
    foreach (double lambdaSquared in RaySpectrumSquared(0.0, generatorEigenvalues))
        if (lambdaSquared > KernelTolerance)
            sum += Math.Log(lambdaSquared);
    return -0.5 * sum;
}

int CoupledModeCount(double[] generatorEigenvalues) =>
    4 * momenta.Count * generatorEigenvalues.Count(u => Math.Abs(u) > 1e-12);

List<double> ZeroCrossingTs(double[] generatorEigenvalues)
{
    // lambda = 0 requires s1 + t u = 0 AND s2 + t u = 0 with u != 0 -> s1 = s2, t = -s1/u
    var crossings = new SortedSet<double>();
    foreach (var (s1, s2) in momenta)
        if (Math.Abs(s1 - s2) < 1e-15)
            foreach (double uc in generatorEigenvalues)
                if (Math.Abs(uc) > 1e-12)
                {
                    double t = -s1 / uc;
                    if (t > 1e-12)
                        crossings.Add(Math.Round(t, 10));
                }
    return crossings.ToList();
}

var blockAxes = new (string Block, int Axis)[]
{
    ("T", 0), ("T", 1), ("T", 2), ("D", 3), ("D", 4), ("D", 5), ("D", 6), ("S", 7),
};
var representations = new (string Name, double[][] Eigenvalues)[]
{
    ("fundamental-3", eigFund),
    ("adjoint-8", eigAdjoint),
};

var rayReports = new List<RayReport>();
double maxTdPotentialDegeneracyResidual = 0.0;
double minTripletSingletSeparation = double.MaxValue;
bool anyPositiveQuarticStabilizer = false;
foreach (var (repName, eig) in representations)
{
    var perAxisSmallT = new Dictionary<int, double[]>();
    foreach (var (block, axis) in blockAxes)
    {
        var u = eig[axis];
        double reference = ReferencePotential(u);
        // kinetic coefficients from a small-t window clear of crossings
        var crossings = ZeroCrossingTs(u);
        double window = crossings.Count > 0 ? Math.Min(0.25, 0.5 * crossings[0]) : 0.25;
        var ts = Enumerable.Range(1, 12).Select(i => window * i / 12.0).ToArray();
        var values = ts.Select(t => RelativePotential(t, u, reference)).ToArray();
        perAxisSmallT[axis] = values;
        // least squares V = A t^2 + B t^4
        double s22 = 0, s24 = 0, s44 = 0, b2 = 0, b4 = 0;
        for (int i = 0; i < ts.Length; i++)
        {
            double t2 = ts[i] * ts[i];
            double t4 = t2 * t2;
            s22 += t2 * t2; s24 += t2 * t4; s44 += t4 * t4;
            b2 += t2 * values[i]; b4 += t4 * values[i];
        }
        double det = s22 * s44 - s24 * s24;
        double aCoefficient = (b2 * s44 - b4 * s24) / det;
        double bCoefficient = (b4 * s22 - b2 * s24) / det;
        anyPositiveQuarticStabilizer |= bCoefficient > 1e-9 && aCoefficient < 0;
        // large-t log slope between crossunder-free samples
        double w1 = RelativePotential(30.0, u, reference);
        double w2 = RelativePotential(60.0, u, reference);
        double logSlope = (w2 - w1) / Math.Log(2.0);
        int coupled = CoupledModeCount(u);
        rayReports.Add(new RayReport(repName, block, axis + 1, aCoefficient, bCoefficient, logSlope, -coupled, crossings.Count, crossings.Count > 0 ? crossings[0] : 0.0));
    }
    // exact T/D degeneracy of the potential values across all seven axes
    for (int axis = 1; axis < 7; axis++)
        for (int i = 0; i < perAxisSmallT[axis].Length; i++)
            maxTdPotentialDegeneracyResidual = Math.Max(maxTdPotentialDegeneracyResidual,
                Math.Abs(perAxisSmallT[axis][i] - perAxisSmallT[0][i]));
    // T vs S separation (relative potential differs measurably at matched t)
    double separation = Math.Abs(perAxisSmallT[7][^1] - perAxisSmallT[0][^1]);
    minTripletSingletSeparation = Math.Min(minTripletSingletSeparation, separation);
}
bool tripletDoubletFermionLoopExactlyDegenerate = maxTdPotentialDegeneracyResidual <= 1e-9;
bool singletAxisPotentialDistinct = minTripletSingletSeparation >= 1e-3;
bool largeTSlopesMatchCoupledModeCounts = rayReports.All(r => Math.Abs(r.LargeTLogSlope - r.ExpectedLogSlope) <= 0.05);
bool fermionLoopProvidesPositiveQuarticStabilizer = anyPositiveQuarticStabilizer;
bool fermionLoopClassFunctionOnRankOneRays =
    closedFormSpectrumVerified && tripletDoubletOnSingleAdjointOrbit && tripletDoubletFermionLoopExactlyDegenerate;
bool doubletSelectedByFermionLoop = false; // exact degeneracy forbids any T/D ordering
bool su3BreakingFermionicStructureRequired = true; // the only escape from the class-function argument
bool fermionLoopBlockSelectionMechanismClosed =
    fermionLoopClassFunctionOnRankOneRays &&
    conjugacyWitnessExact &&
    !fermionLoopProvidesPositiveQuarticStabilizer &&
    !doubletSelectedByFermionLoop;

// ---------------------------------------------------------------------------
// Fail-closed boundary.
// ---------------------------------------------------------------------------

const bool targetBlindConstruction = true;
const bool physicalTargetsConsultedForConstruction = false;
const bool workbenchConventionsAreSourceDefined = false; // lattice, spinors, reps: recorded conventions only
const bool sourceDefinesSu3BreakingFermionicStructure = false;
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
    "gell-mann orbits; color-swap conjugator; closed-form 4x4 naive-Dirac ray spectrum; relative one-loop functional; no target values")))).ToLowerInvariant();

bool analysisInternallyConsistent =
    traceNormalizationExact &&
    structureConstantsExact &&
    adjointRepresentationHermitian &&
    conjugacyWitnessExact &&
    tripletDoubletOnSingleAdjointOrbit &&
    singletAxisOnDistinctOrbit &&
    closedFormSpectrumVerified &&
    tripletDoubletFermionLoopExactlyDegenerate &&
    singletAxisPotentialDistinct &&
    largeTSlopesMatchCoupledModeCounts;

bool fermionLoopBlockSelectionNoGoProbePassed =
    phase405PrecursorPassed &&
    phase410PrecursorPassed &&
    phase418PrecursorPassed &&
    analysisInternallyConsistent &&
    fermionLoopClassFunctionOnRankOneRays &&
    !fermionLoopProvidesPositiveQuarticStabilizer &&
    !doubletSelectedByFermionLoop &&
    fermionLoopBlockSelectionMechanismClosed &&
    targetBlindConstruction &&
    !physicalTargetsConsultedForConstruction &&
    !workbenchConventionsAreSourceDefined &&
    !sourceDefinesSu3BreakingFermionicStructure &&
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

string terminalStatus = fermionLoopBlockSelectionNoGoProbePassed
    ? "fermion-loop-cannot-select-doublet-on-rank-one-rays-mechanism-class-closed"
    : "fermion-loop-block-selection-no-go-probe-blocked";

string decision = fermionLoopBlockSelectionNoGoProbePassed
    ? "The fermionic-backreaction mechanism class named by Phases 405/410/418 is decided at one-loop order on constant rank-1 rays: the fermion determinant is an adjoint-orbit class function; the Gell-Mann axes lambda_1..lambda_7 lie on ONE orbit (explicit color-swap conjugator, exact), so the fermion-loop potential is EXACTLY degenerate between the triplet and doublet blocks in both the fundamental and adjoint representations - the loop can only distinguish the lambda_8 orbit type. Along every ray the functional falls like -N log t with N matching the coupled-mode count exactly, so no positive quartic stabilizer is generated in any direction. An su(3)-invariant fermionic sector therefore CANNOT produce the direction-dependent block mass law or the stabilizer Phase418 had to import; doublet selection via fermionic backreaction requires su(3)-breaking fermionic structure that no reviewed source defines. No Phase201 or Phase256 field is filled; nothing is promoted."
    : "Do not use the fermion-loop no-go verdicts until the precursor and consistency batteries pass.";

var result = new
{
    phaseId = "phase428-fermion-loop-block-selection-no-go-probe",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    fermionLoopBlockSelectionNoGoProbePassed,
    phase405PrecursorPassed,
    phase410PrecursorPassed,
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
        representationsProbed = new[] { "fundamental-3", "adjoint-8" },
        blockMenu = "Phase418 Gell-Mann axes: T={1,2,3}, D={4,5,6,7}, S={8}",
        workbenchConventionsAreSourceDefined,
    },
    traceNormalizationResidual,
    traceNormalizationExact,
    structureConstantResidual,
    structureConstantsExact,
    adjointHermiticityResidual,
    adjointRepresentationHermitian,
    conjugacyWitnessResidual,
    conjugacyWitnessExact,
    tdOrbitDegeneracyResidual,
    tripletDoubletOnSingleAdjointOrbit,
    singletOrbitSeparation,
    singletAxisOnDistinctOrbit,
    closedFormCrossCheckResidual,
    closedFormSpectrumVerified,
    maxTdPotentialDegeneracyResidual,
    tripletDoubletFermionLoopExactlyDegenerate,
    minTripletSingletSeparation,
    singletAxisPotentialDistinct,
    largeTSlopesMatchCoupledModeCounts,
    rays = rayReports.Select(r => new
    {
        representation = r.Representation,
        block = r.Block,
        gellMannAxis = r.GellMannAxis,
        kineticQuadraticCoefficient = r.KineticQuadraticCoefficient,
        kineticQuarticCoefficient = r.KineticQuarticCoefficient,
        largeTLogSlope = r.LargeTLogSlope,
        expectedLogSlope = r.ExpectedLogSlope,
        zeroCrossingCount = r.ZeroCrossingCount,
        firstZeroCrossingT = r.FirstZeroCrossingT,
    }).ToArray(),
    fermionLoopClassFunctionOnRankOneRays,
    fermionLoopProvidesPositiveQuarticStabilizer,
    doubletSelectedByFermionLoop,
    su3BreakingFermionicStructureRequired,
    sourceDefinesSu3BreakingFermionicStructure,
    sourceDefinesFermionOccupationOrRegularization,
    fermionLoopBlockSelectionMechanismClosed,
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
        phase405SummaryPath = Phase405SummaryPath,
        phase410SummaryPath = Phase410SummaryPath,
        phase418SummaryPath = Phase418SummaryPath,
    },
    explicitCandidateOnlyNonclaims = new[]
    {
        "The workbench (4x4 lattice, 4-spinors, naive central-difference Dirac, fundamental/adjoint fermions) is a recorded convention, not source-defined physics; the class-function argument itself is representation- and discretization-independent for any su(3)-invariant fermion sector on constant rank-1 rays.",
        "The no-go is scoped to one-loop determinants along constant rank-1 rays; mixed/non-constant configurations and multi-loop effects are outside this probe.",
        "The only escape - su(3)-breaking fermionic structure (background masses, chemical potentials, non-invariant occupations) - is not defined by any reviewed source and would be candidate-only if invented.",
        "No Phase201 or Phase256 fill.",
    },
    decision,
};

var options = JsonOptions();
File.WriteAllText(Path.Combine(outputDir, "fermion_loop_block_selection_no_go_probe.json"), JsonSerializer.Serialize(result, options));
string summaryPath = Path.Combine(outputDir, "fermion_loop_block_selection_no_go_probe_summary.json");
File.WriteAllText(summaryPath, JsonSerializer.Serialize(result, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"fermionLoopBlockSelectionNoGoProbePassed={fermionLoopBlockSelectionNoGoProbePassed}");
Console.WriteLine($"conjugacyWitnessExact={conjugacyWitnessExact} tdOrbitDegeneracyResidual={tdOrbitDegeneracyResidual:E2}");
Console.WriteLine($"closedFormSpectrumVerified={closedFormSpectrumVerified} (residual {closedFormCrossCheckResidual:E2})");
Console.WriteLine($"tripletDoubletFermionLoopExactlyDegenerate={tripletDoubletFermionLoopExactlyDegenerate} (residual {maxTdPotentialDegeneracyResidual:E2})");
Console.WriteLine($"singletAxisPotentialDistinct={singletAxisPotentialDistinct} largeTSlopesMatchCoupledModeCounts={largeTSlopesMatchCoupledModeCounts}");
foreach (var r in rayReports.Where(r => r.GellMannAxis is 1 or 4 or 8))
    Console.WriteLine($"  {r.Representation} {r.Block}(axis {r.GellMannAxis}): A={r.KineticQuadraticCoefficient:F3} B={r.KineticQuarticCoefficient:F3} logSlope={r.LargeTLogSlope:F1} (expected {r.ExpectedLogSlope})");
Console.WriteLine($"fermionLoopProvidesPositiveQuarticStabilizer={fermionLoopProvidesPositiveQuarticStabilizer}");
Console.WriteLine($"doubletSelectedByFermionLoop={doubletSelectedByFermionLoop}");
Console.WriteLine($"fermionLoopBlockSelectionMechanismClosed={fermionLoopBlockSelectionMechanismClosed}");
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

public sealed record RayReport(
    string Representation,
    string Block,
    int GellMannAxis,
    double KineticQuadraticCoefficient,
    double KineticQuarticCoefficient,
    double LargeTLogSlope,
    double ExpectedLogSlope,
    int ZeroCrossingCount,
    double FirstZeroCrossingT);
