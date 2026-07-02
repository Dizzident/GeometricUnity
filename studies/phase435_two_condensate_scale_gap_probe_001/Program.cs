using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

// Phase435: two-condensate landscape / scale-gap probe (the self-consistent
// background iteration named after the Phase430-434 team wave).
//
// Phase430 showed the net one-loop landscape dynamically prefers the
// hypercharge axis but has no finite minimum on rays; Phase431 showed a
// candidate lambda_8 background induces the doublet-block mass law. The
// remaining dynamical question is whether the TWO-CONDENSATE landscape
// (lambda_8 amplitude a on the x-links, doublet lambda_4 amplitude b on the
// y-links) - where the tree bosonic quartic is NONZERO
// (||[u8,u4]||^2 = 3/4) - has a finite self-consistent point.
//
// Answer (machine-verified below): NO on this workbench. Interior stationary
// structure exists, but it is not global: for the derived matter content the
// pure-lambda_8 axis log-runaway undercuts every interior value, and for the
// fundamental content the sampled landscape is nonnegative (the origin
// undercuts the interior structure - no condensation onset). A finite
// self-consistent scale therefore requires log-saturation structure (higher
// order, compactness, or UV data) that no reviewed source defines. This
// phase converts the scale gap from a narrative into recorded quantities.
//
// Fail-closed: workbench conventions as in Phases 428/430/431 (4x4 lattice,
// 4-spinors, naive Dirac, combined-direction adjoint-mass bosonic model,
// tree quartic weight kappa_B = 1); interior stationary data are recorded
// blind as candidate-only structure; no contract field can be filled.

const string DefaultOutputDir = "studies/phase435_two_condensate_scale_gap_probe_001/output";
const string Phase430SummaryPath = "studies/phase430_net_one_loop_direction_selection_probe_001/output/net_one_loop_direction_selection_probe_summary.json";
const string Phase431SummaryPath = "studies/phase431_lambda8_background_doublet_reopening_probe_001/output/lambda8_background_doublet_reopening_probe_summary.json";
const string ApplicationSubjectKind = "two-condensate-scale-gap-probe";

var outputDir = Environment.GetEnvironmentVariable("PHASE435_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase430 = JsonDocument.Parse(File.ReadAllText(Phase430SummaryPath));
using var phase431 = JsonDocument.Parse(File.ReadAllText(Phase431SummaryPath));

bool phase430PrecursorPassed =
    JsonBool(phase430.RootElement, "netOneLoopDirectionSelectionProbePassed") is true &&
    JsonBool(phase430.RootElement, "noFiniteMinimumOnRays") is true &&
    JsonBool(phase430.RootElement, "derivedContentSelectsHyperchargeAxis") is true;
bool phase431PrecursorPassed =
    JsonBool(phase431.RootElement, "lambda8BackgroundDoubletReopeningProbePassed") is true &&
    JsonBool(phase431.RootElement, "backgroundInducesBlockDependentMassLaw") is true;

// ---------------------------------------------------------------------------
// su(3): Gell-Mann fundamental generators and adjoint action.
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

// structure constants and adjoint generators
var fabc = new double[8, 8, 8];
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
    }

Complex[,] AdjointOf(int axis)
{
    var m = new Complex[8, 8];
    for (int b = 0; b < 8; b++)
        for (int c = 0; c < 8; c++)
            m[b, c] = -Complex.ImaginaryOne * fabc[axis, b, c];
    return m;
}
var ad8 = AdjointOf(7);
var ad4 = AdjointOf(3);

// tree quartic coefficient: ||[u8, u4]||^2 with u = lambda/2, Frobenius x2
double bracketNormSquared;
{
    var comm = new Complex[3, 3];
    for (int r = 0; r < 3; r++)
        for (int c = 0; c < 3; c++)
        {
            Complex sum = Complex.Zero;
            for (int k = 0; k < 3; k++)
                sum += genFund[7][r, k] * genFund[3][k, c] - genFund[3][r, k] * genFund[7][k, c];
            comm[r, c] = sum;
        }
    double norm = 0.0;
    for (int r = 0; r < 3; r++)
        for (int c = 0; c < 3; c++)
            norm += comm[r, c].Real * comm[r, c].Real + comm[r, c].Imaginary * comm[r, c].Imaginary;
    bracketNormSquared = 2.0 * norm;
}
bool bracketNormIsThreeQuarters = Math.Abs(bracketNormSquared - 0.75) <= 1e-12;

// ---------------------------------------------------------------------------
// One-loop functionals on the 4x4 lattice (momentum-block form).
// Fermionic: per momentum (s1, s2), block = gamma1 (x) (s1 I + a U8)
// + gamma2 (x) (s2 I + b U4) on the 4 (x) 3 factor -> 12-dim Hermitian.
// Bosonic (recorded workbench model, Phase430 convention): masses
// m_i^2(a, b) = eigenvalues of (-(a ad8 + b ad4)^2), dispersion
// eps_k^2 = s1^2 + s2^2, 2 polarizations.
// ---------------------------------------------------------------------------

var sines = Enumerable.Range(0, 4).Select(n => Math.Sin(2.0 * Math.PI * n / 4.0)).ToArray();
var momenta = new List<(double S1, double S2)>();
for (int n1 = 0; n1 < 4; n1++)
    for (int n2 = 0; n2 < 4; n2++)
        momenta.Add((sines[n1], sines[n2]));
// IR convention (recorded): the zero-dispersion doubler sector (s1 = s2 = 0)
// makes the relative potential discontinuous at the origin (its modes sit at
// exactly zero and acquire log-divergent contributions for any nonzero
// amplitude). It is excluded from BOTH determinants so that V is continuous
// with V(0,0) = 0 exactly.
var infraredMomenta = momenta.Where(k => k.S1 * k.S1 + k.S2 * k.S2 > 1e-12).ToList();
int excludedZeroDispersionMomentumCount = momenta.Count - infraredMomenta.Count;

double FermionFunctional(double a, double b)
{
    // -1/2 sum log lambda^2 over nonzero modes, IR-excluded momenta
    double total = 0.0;
    foreach (var (s1, s2) in infraredMomenta)
    {
        // 12-dim block: gamma1 = sx (x) I2 on spinor factor, gauge 3-dim
        var block = new Complex[12, 12];
        // spinor basis: 4 = 2x2; gamma1 = sx (x) I2, gamma2 = sy (x) I2
        for (int sp = 0; sp < 2; sp++)   // the I2 factor index
            for (int g = 0; g < 3; g++)
                for (int gp = 0; gp < 3; gp++)
                {
                    Complex m1 = (g == gp ? s1 : 0.0) + a * genFund[7][g, gp];
                    Complex m2 = (g == gp ? s2 : 0.0) + b * genFund[3][g, gp];
                    // gamma1: |0><1| + |1><0| on the sx factor
                    block[(0 * 2 + sp) * 3 + g, (1 * 2 + sp) * 3 + gp] += m1;
                    block[(1 * 2 + sp) * 3 + g, (0 * 2 + sp) * 3 + gp] += m1;
                    // gamma2: -i|0><1| + i|1><0|
                    block[(0 * 2 + sp) * 3 + g, (1 * 2 + sp) * 3 + gp] += -Complex.ImaginaryOne * m2;
                    block[(1 * 2 + sp) * 3 + g, (0 * 2 + sp) * 3 + gp] += Complex.ImaginaryOne * m2;
                }
        var (values, _) = Jacobi(Realify(block));
        // realified doubling: every eigenvalue appears twice; use half the sum
        double sum = 0.0;
        foreach (double v in values)
        {
            double lambdaSquared = v * v;
            if (lambdaSquared > 1e-18)
                sum += Math.Log(lambdaSquared);
        }
        total += -0.5 * (sum / 2.0);
    }
    return total;
}

double BosonFunctional(double a, double b)
{
    var combined = new Complex[8, 8];
    for (int r = 0; r < 8; r++)
        for (int c = 0; c < 8; c++)
            combined[r, c] = a * ad8[r, c] + b * ad4[r, c];
    // masses^2 = eigenvalues of A^dag A (A Hermitian -> A^2)
    var squared = new Complex[8, 8];
    for (int r = 0; r < 8; r++)
        for (int c = 0; c < 8; c++)
        {
            Complex sum = Complex.Zero;
            for (int k = 0; k < 8; k++)
                sum += combined[r, k] * combined[k, c];
            squared[r, c] = sum;
        }
    var (m2Values, _) = Jacobi(Realify(squared));
    var masses = m2Values.Where((_, i) => i % 2 == 0).OrderBy(v => v).ToArray(); // undouble
    double total = 0.0;
    foreach (var (s1, s2) in infraredMomenta)
    {
        double eps2 = s1 * s1 + s2 * s2;
        foreach (double m2 in masses)
        {
            double value = eps2 + Math.Max(m2, 0.0);
            if (value > 1e-18)
                total += 2.0 * 0.5 * Math.Log(value); // 2 polarizations
        }
    }
    return total;
}

const double KappaB = 1.0;
double fermionReference = FermionFunctional(0.0, 0.0);
double bosonReference = BosonFunctional(0.0, 0.0);

double Potential(double a, double b, int fermionCopies) =>
    KappaB * (a * b) * (a * b) * bracketNormSquared
    + (BosonFunctional(a, b) - bosonReference)
    + fermionCopies * (FermionFunctional(a, b) - fermionReference);

// dense cross-check battery: block fermionic functional vs a direct dense
// 192-dim construction at one nontrivial point
double denseCrossCheckResidual;
{
    double aS = 0.9, bS = 0.6;
    int vertices = 16, dimG = 3, n = 4 * vertices * dimG;
    var hop = new double[2][,];
    for (int mu = 0; mu < 2; mu++)
    {
        hop[mu] = new double[vertices, vertices];
        for (int x = 0; x < 4; x++)
            for (int y = 0; y < 4; y++)
            {
                int i = x + 4 * y;
                int j = mu == 0 ? ((x + 1) % 4) + 4 * y : x + 4 * ((y + 1) % 4);
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
    var gaugeTerm = new[] { new Complex[3, 3], new Complex[3, 3] };
    for (int r = 0; r < 3; r++)
        for (int c = 0; c < 3; c++)
        {
            gaugeTerm[0][r, c] = aS * genFund[7][r, c];
            gaugeTerm[1][r, c] = bS * genFund[3][r, c];
        }
    var dirac = new Complex[n, n];
    for (int mu = 0; mu < 2; mu++)
        for (int s = 0; s < 4; s++)
            for (int sp = 0; sp < 4; sp++)
            {
                if (gamma[mu][s, sp] == Complex.Zero)
                    continue;
                for (int v = 0; v < vertices; v++)
                {
                    for (int vp = 0; vp < vertices; vp++)
                        if (Math.Abs(hop[mu][v, vp]) > 1e-15)
                            for (int g = 0; g < dimG; g++)
                                dirac[(s * vertices + v) * dimG + g, (sp * vertices + vp) * dimG + g] +=
                                    gamma[mu][s, sp] * Complex.ImaginaryOne * hop[mu][v, vp];
                    for (int g = 0; g < dimG; g++)
                        for (int gp = 0; gp < dimG; gp++)
                            if (gaugeTerm[mu][g, gp] != Complex.Zero)
                                dirac[(s * vertices + v) * dimG + g, (sp * vertices + v) * dimG + gp] +=
                                    gamma[mu][s, sp] * gaugeTerm[mu][g, gp];
                }
            }
    var (denseValues, _) = Jacobi(Realify(dirac));
    double denseSum = 0.0;
    foreach (double v in denseValues)
    {
        double lambdaSquared = v * v;
        if (lambdaSquared > 1e-18)
            denseSum += Math.Log(lambdaSquared);
    }
    double denseFunctional = -0.5 * (denseSum / 2.0);
    // dense operator contains ALL momenta; rebuild the all-momentum block sum
    double allMomentumBlockSum = 0.0;
    foreach (var (s1, s2) in momenta)
    {
        var block = new Complex[12, 12];
        for (int sp = 0; sp < 2; sp++)
            for (int g = 0; g < 3; g++)
                for (int gp = 0; gp < 3; gp++)
                {
                    Complex m1 = (g == gp ? s1 : 0.0) + aS * genFund[7][g, gp];
                    Complex m2 = (g == gp ? s2 : 0.0) + bS * genFund[3][g, gp];
                    block[(0 * 2 + sp) * 3 + g, (1 * 2 + sp) * 3 + gp] += m1;
                    block[(1 * 2 + sp) * 3 + g, (0 * 2 + sp) * 3 + gp] += m1;
                    block[(0 * 2 + sp) * 3 + g, (1 * 2 + sp) * 3 + gp] += -Complex.ImaginaryOne * m2;
                    block[(1 * 2 + sp) * 3 + g, (0 * 2 + sp) * 3 + gp] += Complex.ImaginaryOne * m2;
                }
        var (bv, _) = Jacobi(Realify(block));
        double bs = 0.0;
        foreach (double v in bv)
        {
            double l2 = v * v;
            if (l2 > 1e-18)
                bs += Math.Log(l2);
        }
        allMomentumBlockSum += -0.5 * (bs / 2.0);
    }
    denseCrossCheckResidual = Math.Abs(denseFunctional - allMomentumBlockSum);
}
bool blockFunctionalVerified = denseCrossCheckResidual <= 1e-8;

// ---------------------------------------------------------------------------
// Landscape analysis per content: coarse grid, interior local minima,
// verified-descent refinement (backtracking line search), Hessian
// classification, near-origin scan, and the axis-runaway battery.
// ---------------------------------------------------------------------------

var contentReports = new List<ContentReport>();
foreach (var (contentName, copies) in new[] { ("fundamental", 1), ("derived-4x", 4) })
{
    int gridN = 24;
    var grid = Enumerable.Range(0, gridN).Select(i => 0.05 + (4.0 - 0.05) * i / (gridN - 1)).ToArray();
    var values = new double[gridN, gridN];
    for (int i = 0; i < gridN; i++)
        for (int j = 0; j < gridN; j++)
            values[i, j] = Potential(grid[i], grid[j], copies);

    // interior local minima on the grid
    var interiorMinima = new List<(double A, double B, double V)>();
    for (int i = 1; i < gridN - 1; i++)
        for (int j = 1; j < gridN - 1; j++)
        {
            bool isMin = true;
            for (int di = -1; di <= 1 && isMin; di++)
                for (int dj = -1; dj <= 1 && isMin; dj++)
                    if ((di != 0 || dj != 0) && values[i + di, j + dj] < values[i, j])
                        isMin = false;
            if (isMin)
                interiorMinima.Add((grid[i], grid[j], values[i, j]));
        }
    var best = interiorMinima.OrderBy(m => m.V).FirstOrDefault();
    bool interiorStationaryStructureExists = interiorMinima.Count > 0;

    // verified-descent refinement (gradient descent with backtracking:
    // every accepted step strictly decreases the potential)
    double aStar = best.A, bStar = best.B, vStar = best.V;
    double gradientNorm = double.NaN;
    if (interiorStationaryStructureExists)
    {
        double h = 1e-3;
        for (int iter = 0; iter < 200; iter++)
        {
            double ga = (Potential(aStar + h, bStar, copies) - Potential(aStar - h, bStar, copies)) / (2 * h);
            double gb = (Potential(aStar, bStar + h, copies) - Potential(aStar, bStar - h, copies)) / (2 * h);
            gradientNorm = Math.Sqrt(ga * ga + gb * gb);
            if (gradientNorm < 1e-4)
                break;
            double step = 0.05;
            bool moved = false;
            while (step > 1e-7)
            {
                double aNew = Math.Max(aStar - step * ga / Math.Max(gradientNorm, 1e-12), 0.01);
                double bNew = Math.Max(bStar - step * gb / Math.Max(gradientNorm, 1e-12), 0.01);
                double vNew = Potential(aNew, bNew, copies);
                if (vNew < vStar - 1e-12)
                {
                    aStar = aNew; bStar = bNew; vStar = vNew;
                    moved = true;
                    break;
                }
                step /= 2.0;
            }
            if (!moved)
                break;
        }
    }
    // 2x2 Hessian classification at the refined point
    double hessDet = double.NaN, hessTrace = double.NaN;
    bool refinedPointIsLocalMinimum = false;
    if (interiorStationaryStructureExists)
    {
        double h2 = 5e-3;
        double vaa = (Potential(aStar + h2, bStar, copies) - 2 * vStar + Potential(aStar - h2, bStar, copies)) / (h2 * h2);
        double vbb = (Potential(aStar, bStar + h2, copies) - 2 * vStar + Potential(aStar, bStar - h2, copies)) / (h2 * h2);
        double vab = (Potential(aStar + h2, bStar + h2, copies) - Potential(aStar + h2, bStar - h2, copies)
                    - Potential(aStar - h2, bStar + h2, copies) + Potential(aStar - h2, bStar - h2, copies)) / (4 * h2 * h2);
        hessDet = vaa * vbb - vab * vab;
        hessTrace = vaa + vbb;
        refinedPointIsLocalMinimum = hessDet > 0 && hessTrace > 0;
    }

    // near-origin scan: any negative value? (condensation onset test)
    double nearOriginMin = double.MaxValue;
    foreach (double a in new[] { 0.002, 0.005, 0.01, 0.02, 0.05, 0.1, 0.2 })
        foreach (double b in new[] { 0.002, 0.005, 0.01, 0.02, 0.05, 0.1, 0.2 })
            nearOriginMin = Math.Min(nearOriginMin, Potential(a, b, copies));
    bool condensationOnsetNearOrigin = nearOriginMin < -1e-6;

    // axis-runaway battery: derived content must be undercut along pure
    // lambda_8 at large amplitude; fundamental must stay above the origin
    double axisValueA20 = Potential(20.0, 0.01, copies);
    double axisValueB20 = Potential(0.01, 20.0, copies);
    bool axisUndercutsInterior = interiorStationaryStructureExists &&
        Math.Min(axisValueA20, axisValueB20) < vStar;
    bool landscapeNonNegativeOnSample = nearOriginMin >= -1e-6 && vStar >= -1e-6 &&
        axisValueA20 >= -1e-6 && axisValueB20 >= -1e-6;
    bool globalFiniteMinimumExists = !axisUndercutsInterior && condensationOnsetNearOrigin;

    contentReports.Add(new ContentReport(
        contentName, copies, interiorMinima.Count, interiorStationaryStructureExists,
        aStar, bStar, vStar, interiorStationaryStructureExists ? bStar / aStar : double.NaN,
        gradientNorm, hessDet, hessTrace, refinedPointIsLocalMinimum,
        nearOriginMin, condensationOnsetNearOrigin,
        axisValueA20, axisValueB20, axisUndercutsInterior,
        landscapeNonNegativeOnSample, globalFiniteMinimumExists));
}

var fundamentalReport = contentReports[0];
var derivedReport = contentReports[1];
bool derivedAxisRunawayUndercutsInterior = derivedReport.AxisUndercutsInterior;
bool fundamentalShowsNoCondensationOnset = !fundamentalReport.CondensationOnsetNearOrigin;
bool anyContentHasGlobalFiniteMinimum = contentReports.Any(r => r.GlobalFiniteMinimumExists);
bool finiteSelfConsistentScaleExists = anyContentHasGlobalFiniteMinimum;
bool scaleRequiresLogSaturationBeyondWorkbench = !finiteSelfConsistentScaleExists;
const bool logSaturationStructureSourceDefined = false;

// ---------------------------------------------------------------------------
// Fail-closed boundary.
// ---------------------------------------------------------------------------

const bool targetBlindConstruction = true;
const bool physicalTargetsConsultedForConstruction = false;
const bool workbenchConventionsAreSourceDefined = false;
const bool interiorStationaryDataIsCandidateOnly = true;
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
    "two-condensate (lambda8, lambda4) landscape; tree quartic + net one-loop; verified-descent stationarity; no target values")))).ToLowerInvariant();

bool potentialContinuousAtOrigin = Math.Abs(Potential(1e-4, 1e-4, 1)) < 1.0 && Math.Abs(Potential(1e-4, 1e-4, 4)) < 1.0;
bool analysisInternallyConsistent =
    bracketNormIsThreeQuarters &&
    blockFunctionalVerified &&
    potentialContinuousAtOrigin;

bool twoCondensateScaleGapProbePassed =
    phase430PrecursorPassed &&
    phase431PrecursorPassed &&
    analysisInternallyConsistent &&
    derivedAxisRunawayUndercutsInterior &&
    !anyContentHasGlobalFiniteMinimum &&
    scaleRequiresLogSaturationBeyondWorkbench &&
    !logSaturationStructureSourceDefined &&
    targetBlindConstruction &&
    !physicalTargetsConsultedForConstruction &&
    !workbenchConventionsAreSourceDefined &&
    interiorStationaryDataIsCandidateOnly &&
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

string terminalStatus = twoCondensateScaleGapProbePassed
    ? "two-condensate-landscape-characterized-finite-scale-requires-saturation-beyond-workbench"
    : "two-condensate-scale-gap-probe-blocked";

string decision = twoCondensateScaleGapProbePassed
    ? "The self-consistent background question is decided on the workbench: the two-condensate (lambda_8, lambda_4) landscape has interior stationary structure but NO finite global minimum. For the derived matter content the pure-lambda_8 log-runaway undercuts every interior value (the doublet admixture cannot stop it because the tree quartic vanishes as the doublet amplitude goes to zero); for the fundamental content no condensation onset exists near the origin on the sampled region. A finite self-consistent scale therefore requires log-saturation structure - higher-order terms, compact field space, or ultraviolet data - that no reviewed source defines. The interior stationary points and their dimensionless amplitude ratios are recorded blind as candidate-only structure. No Phase201 or Phase256 field is filled; nothing is promoted."
    : "Do not use the two-condensate landscape verdicts until the precursor and consistency batteries pass.";

var result = new
{
    phaseId = "phase435-two-condensate-scale-gap-probe",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    twoCondensateScaleGapProbePassed,
    phase430PrecursorPassed,
    phase431PrecursorPassed,
    applicationSubjectKind = ApplicationSubjectKind,
    targetBlindConstruction,
    physicalTargetsConsultedForConstruction,
    targetBlindConstructionHash,
    workbenchConventions = new
    {
        latticeSize = 4,
        spinorDimension = 4,
        diracDiscretization = "naive-central-difference-hermitian",
        configuration = "lambda8 amplitude a on x-links, lambda4 amplitude b on y-links",
        treeQuarticWeightKappaB = KappaB,
        bosonicModel = "combined-direction adjoint masses (Phase430 convention)",
        infraredConvention = "zero-dispersion doubler momentum sector excluded from both determinants (V continuous, V(0,0)=0)",
        workbenchConventionsAreSourceDefined,
    },
    bracketNormSquared,
    bracketNormIsThreeQuarters,
    denseCrossCheckResidual,
    blockFunctionalVerified,
    excludedZeroDispersionMomentumCount,
    potentialContinuousAtOrigin,
    contents = contentReports.Select(r => new
    {
        content = r.Content,
        fermionCopies = r.Copies,
        interiorGridMinimaCount = r.InteriorMinimaCount,
        interiorStationaryStructureExists = r.InteriorStationaryStructureExists,
        refinedA = Finite(r.AStar),
        refinedB = Finite(r.BStar),
        refinedValue = Finite(r.VStar),
        amplitudeRatioBOverA = Finite(r.RatioBOverA),
        refinedGradientNorm = Finite(r.GradientNorm),
        hessianDeterminant = Finite(r.HessianDeterminant),
        hessianTrace = Finite(r.HessianTrace),
        refinedPointIsLocalMinimum = r.RefinedPointIsLocalMinimum,
        nearOriginMinimum = r.NearOriginMin,
        condensationOnsetNearOrigin = r.CondensationOnsetNearOrigin,
        axisValueA20 = r.AxisValueA20,
        axisValueB20 = r.AxisValueB20,
        axisUndercutsInterior = r.AxisUndercutsInterior,
        landscapeNonNegativeOnSample = r.LandscapeNonNegativeOnSample,
        globalFiniteMinimumExists = r.GlobalFiniteMinimumExists,
    }).ToArray(),
    derivedAxisRunawayUndercutsInterior,
    fundamentalShowsNoCondensationOnset,
    anyContentHasGlobalFiniteMinimum,
    finiteSelfConsistentScaleExists,
    scaleRequiresLogSaturationBeyondWorkbench,
    logSaturationStructureSourceDefined,
    interiorStationaryDataIsCandidateOnly,
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
        phase430SummaryPath = Phase430SummaryPath,
        phase431SummaryPath = Phase431SummaryPath,
    },
    explicitCandidateOnlyNonclaims = new[]
    {
        "The interior stationary points and amplitude ratios are workbench structure data, not condensate predictions.",
        "The workbench (lattice, spinors, bosonic mass model, kappa_B) is a recorded convention; the scale-gap verdict is about THIS workbench and the absence of source-defined saturation structure.",
        "No log-saturation structure (higher-order terms, compactness, UV data) is defined by any reviewed source; inventing one would be candidate-only.",
        "No Phase201 or Phase256 fill.",
    },
    decision,
};

var options = JsonOptions();
File.WriteAllText(Path.Combine(outputDir, "two_condensate_scale_gap_probe.json"), JsonSerializer.Serialize(result, options));
string summaryPath = Path.Combine(outputDir, "two_condensate_scale_gap_probe_summary.json");
File.WriteAllText(summaryPath, JsonSerializer.Serialize(result, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"twoCondensateScaleGapProbePassed={twoCondensateScaleGapProbePassed}");
Console.WriteLine($"bracketNormIsThreeQuarters={bracketNormIsThreeQuarters} blockFunctionalVerified={blockFunctionalVerified} (residual {denseCrossCheckResidual:E2})");
foreach (var r in contentReports)
    Console.WriteLine($"  {r.Content}: interiorMinima={r.InteriorMinimaCount} refined=({r.AStar:F3},{r.BStar:F3}) V={r.VStar:F2} gradNorm={r.GradientNorm:F4} localMin={r.RefinedPointIsLocalMinimum} nearOriginMin={r.NearOriginMin:F4} axisA20={r.AxisValueA20:F1} axisUndercuts={r.AxisUndercutsInterior} globalMin={r.GlobalFiniteMinimumExists}");
Console.WriteLine($"derivedAxisRunawayUndercutsInterior={derivedAxisRunawayUndercutsInterior}");
Console.WriteLine($"fundamentalShowsNoCondensationOnset={fundamentalShowsNoCondensationOnset}");
Console.WriteLine($"finiteSelfConsistentScaleExists={finiteSelfConsistentScaleExists}");
Console.WriteLine($"scaleRequiresLogSaturationBeyondWorkbench={scaleRequiresLogSaturationBeyondWorkbench}");
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

static double? Finite(double value) => double.IsFinite(value) ? value : null;

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

public sealed record ContentReport(
    string Content,
    int Copies,
    int InteriorMinimaCount,
    bool InteriorStationaryStructureExists,
    double AStar,
    double BStar,
    double VStar,
    double RatioBOverA,
    double GradientNorm,
    double HessianDeterminant,
    double HessianTrace,
    bool RefinedPointIsLocalMinimum,
    double NearOriginMin,
    bool CondensationOnsetNearOrigin,
    double AxisValueA20,
    double AxisValueB20,
    bool AxisUndercutsInterior,
    bool LandscapeNonNegativeOnSample,
    bool GlobalFiniteMinimumExists);
