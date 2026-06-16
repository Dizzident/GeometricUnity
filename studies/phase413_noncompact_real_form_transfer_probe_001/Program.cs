using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

// Phase413: noncompact real-form transfer probe.
//
// THE NAMED LOOPHOLE: every composite-extraction no-go in Phases
// 408/409/411/412 was proved on the COMPACT form (so(4) welded into
// so(10)), while the draft's structures are noncompact (Lorentzian
// spacetime so(1,3); internal Spin(6,4)). DEEP-RESEARCH-20260612 named a
// noncompact-only invariant - one with no compact analogue - as the
// single most plausible evasion. This probe machine-decides whether that
// loophole exists for the welded chain.
//
// THE TRANSFER PRINCIPLE (machine-verified, not assumed): every no-go in
// the series is a COMPLEX-LINEAR KERNEL DIMENSION of a family of
// operators built functorially from the weld. Complex kernel dimensions
// are properties of the COMPLEXIFIED representation: if the noncompact
// weld complexifies to the SAME so(10, C) embedding as the compact weld
// (up to an explicit change of basis), every count transfers verbatim -
// no noncompact-only invariant can exist. The probe verifies exactly
// this, plus direct recomputations as fail-closed spot-checks:
//
//   N1. THE LORENTZIAN WELD: pi_eta: so(1,3) -> gl(10) on the metrics
//       fiber Sym^2(R^{1,3}) with eta = diag(-1,1,1,1) (the draft's
//       chimeric identification in physical signature). Machine-verify
//       it is a Lie homomorphism, that it preserves the induced trace
//       form B(S,T) = Tr(eta S eta T), and compute B's SIGNATURE on the
//       10 (recorded; the (7,3)/(3,7)-type outcome connects to the
//       Phase406 Cl(7,3) signature axis).
//
//   N2. COMPLEXIFIED COINCIDENCE (the keystone): the complex change of
//       basis T4 = diag(i,1,1,1) carries eta to delta; machine-verify
//       T10 pi_eta(X) T10^{-1} = pi_compact(T4 X T4^{-1}) exactly for
//       all six generators, where T10 is the induced map on Sym^2 and
//       pi_compact is extended C-linearly. Same complexified weld =>
//       identical complex invariant theory on every functorial carrier.
//
//   N3. DIRECT SPOT-CHECKS (fail-closed): recompute on the NONCOMPACT
//       form, over C, the two headline counts: (a) the linear singlet
//       content of the welded frame-cross block 4 x 10 (compact answer:
//       0); (b) the complete bilinear spin-0 dimension of (4x10)^(x2)
//       (compact answer: 7). Both must transfer exactly.
//
//   N4. THE RESIDUAL CAVEAT (named, not closed): real forms differ in
//       REAL structure (Majorana conditions, real-vs-complex-vs-
//       quaternionic type of irreps, symmetric-vs-antisymmetric type of
//       invariant forms). These can relabel REAL-dimension bookkeeping
//       but CANNOT create complex-invariant directions where the
//       complexification has none - so no SM-doublet extraction can
//       appear in any real form of this chain. The unitary
//       (infinite-dimensional) representation category of the noncompact
//       groups is outside this probe's scope and is recorded as such.
//
// CONSEQUENCE IF PASSED: the Phase408-412 no-gos are REAL-FORM
// INDEPENDENT for the welded chain; the noncompact-evasion route is
// closed at the finite-dimensional level. The remaining named routes
// reduce to: the draft's unobserved-phase fields, or a new
// primary-source specification.
//
// Fail-closed: exact representation arithmetic only; no dynamics, no
// scales; nothing promoted; no contract field is filled.

const string DefaultOutputDir = "studies/phase413_noncompact_real_form_transfer_probe_001/output";
const string Phase409SummaryPath = "studies/phase409_invariant_pairing_menu_spin_zero_extraction_probe_001/output/invariant_pairing_menu_spin_zero_extraction_probe_summary.json";
const string Phase412SummaryPath = "studies/phase412_quartic_sm_doublet_intersection_analysis_001/output/quartic_sm_doublet_intersection_analysis_summary.json";

const double Tolerance = 1e-10;

var outputDir = Environment.GetEnvironmentVariable("PHASE413_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var p409 = JsonDocument.Parse(File.ReadAllText(Phase409SummaryPath));
using var p412 = JsonDocument.Parse(File.ReadAllText(Phase412SummaryPath));
bool phase409PrecursorPassed =
    JsonBool(p409.RootElement, "invariantPairingMenuSpinZeroExtractionProbePassed") is true &&
    JsonInt(p409.RootElement, "bilinearSpinZeroDimension") == 7 &&
    JsonInt(p409.RootElement, "linearSingletDimension") == 0;
bool phase412PrecursorPassed =
    JsonBool(p412.RootElement, "quarticSmDoubletIntersectionAnalysisPassed") is true &&
    JsonBool(p412.RootElement, "quarticWeldedScalarSmDoubletAbsentAllChannels") is true;

// ---------------------------------------------------------------------------
// Bases and embeddings. eta = diag(-1,1,1,1); so(eta) basis
// (M^eta_ab)_{ij} = delta_ai eta_bj - delta_bi eta_aj; compact so(4) basis
// as in Phase408/409. Sym^2 orthonormal-against-delta basis B_I (the SAME
// coordinate basis is used for both forms; only the FORMS differ).
// ---------------------------------------------------------------------------

double[] eta = [-1.0, 1.0, 1.0, 1.0];

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

var so4Pairs = new List<(int I, int J)>();
for (int i = 0; i < 4; i++)
    for (int j = i + 1; j < 4; j++)
        so4Pairs.Add((i, j));

double[,] CompactGenerator(int g)
{
    var (i, j) = so4Pairs[g];
    var m = new double[4, 4];
    m[i, j] = 1.0;
    m[j, i] = -1.0;
    return m;
}

double[,] LorentzGenerator(int g)
{
    var (a, b) = so4Pairs[g];
    // (M_ab)_{ij} = delta_ai eta_bj - delta_bi eta_aj with eta diagonal
    var result = new double[4, 4];
    for (int i = 0; i < 4; i++)
        for (int j = 0; j < 4; j++)
            result[i, j] = (i == a && j == b ? eta[b] : 0.0) - (i == b && j == a ? eta[a] : 0.0);
    return result;
}

// Sym^2 action S -> X S + S X^T expressed in the coordinate basis B_I via
// the DELTA trace pairing (a pure coordinate expansion; valid for any X
// because {B_I} is delta-orthonormal).
double[,] SymAction(double[,] x)
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
                    sum += x[r, k] * symMats[col][k, c] + symMats[col][r, k] * x[c, k];
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

// NOTE: the compact Phase408 weld used S -> X S - S X (antisymmetric X:
// -SX = SX^T), i.e. the SAME formula S -> X S + S X^T. So SymAction is the
// uniform functorial definition for both forms.
var piCompact = Enumerable.Range(0, 6).Select(g => SymAction(CompactGenerator(g))).ToArray();
var piLorentz = Enumerable.Range(0, 6).Select(g => SymAction(LorentzGenerator(g))).ToArray();

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

// N1a: homomorphism check for pi_eta: express [X_g, X_h] in the Lorentz
// generator basis (6x6 linear solve) and compare embeddings.
double lorentzHomResidual = 0.0;
{
    // build the 6x6 Gram of the Lorentz generators under Frobenius pairing
    var gens = Enumerable.Range(0, 6).Select(LorentzGenerator).ToArray();
    var gram = new double[6, 6];
    for (int x = 0; x < 6; x++)
        for (int y = 0; y < 6; y++)
        {
            double sum = 0.0;
            for (int r = 0; r < 4; r++)
                for (int c = 0; c < 4; c++)
                    sum += gens[x][r, c] * gens[y][r, c];
            gram[x, y] = sum;
        }
    double[] Solve(double[,] aIn, double[] bIn)
    {
        int n = bIn.Length;
        var a = (double[,])aIn.Clone();
        var b = (double[])bIn.Clone();
        for (int k = 0; k < n; k++)
        {
            int p = k;
            for (int i = k + 1; i < n; i++)
                if (System.Math.Abs(a[i, k]) > System.Math.Abs(a[p, k]))
                    p = i;
            if (p != k)
            {
                for (int j = 0; j < n; j++)
                    (a[k, j], a[p, j]) = (a[p, j], a[k, j]);
                (b[k], b[p]) = (b[p], b[k]);
            }
            for (int i = k + 1; i < n; i++)
            {
                double f = a[i, k] / a[k, k];
                for (int j = k; j < n; j++)
                    a[i, j] -= f * a[k, j];
                b[i] -= f * b[k];
            }
        }
        var x = new double[n];
        for (int i = n - 1; i >= 0; i--)
        {
            double sum = b[i];
            for (int j = i + 1; j < n; j++)
                sum -= a[i, j] * x[j];
            x[i] = sum / a[i, i];
        }
        return x;
    }
    for (int x = 0; x < 6; x++)
        for (int y = x + 1; y < 6; y++)
        {
            var comm = MatComm(gens[x], gens[y]);
            var rhs = new double[6];
            for (int g = 0; g < 6; g++)
            {
                double sum = 0.0;
                for (int r = 0; r < 4; r++)
                    for (int c = 0; c < 4; c++)
                        sum += gens[g][r, c] * comm[r, c];
                rhs[g] = sum;
            }
            var coefficients = Solve(gram, rhs);
            // residual of the expansion itself
            var rebuilt = new double[4, 4];
            for (int g = 0; g < 6; g++)
                for (int r = 0; r < 4; r++)
                    for (int c = 0; c < 4; c++)
                        rebuilt[r, c] += coefficients[g] * gens[g][r, c];
            for (int r = 0; r < 4; r++)
                for (int c = 0; c < 4; c++)
                    lorentzHomResidual = System.Math.Max(lorentzHomResidual, System.Math.Abs(rebuilt[r, c] - comm[r, c]));
            // embedded comparison
            var lhs = MatComm(piLorentz[x], piLorentz[y]);
            var expected = new double[symDim, symDim];
            for (int g = 0; g < 6; g++)
                for (int r = 0; r < symDim; r++)
                    for (int c = 0; c < symDim; c++)
                        expected[r, c] += coefficients[g] * piLorentz[g][r, c];
            for (int r = 0; r < symDim; r++)
                for (int c = 0; c < symDim; c++)
                    lorentzHomResidual = System.Math.Max(lorentzHomResidual, System.Math.Abs(lhs[r, c] - expected[r, c]));
        }
}
bool lorentzWeldIsHomomorphism = lorentzHomResidual <= Tolerance;

// N1b: induced bilinear form B(S,T) = Tr(eta S eta T) on Sym^2; its
// signature and pi_eta-invariance.
var inducedForm = new double[symDim, symDim];
for (int x = 0; x < symDim; x++)
    for (int y = 0; y < symDim; y++)
    {
        double sum = 0.0;
        for (int r = 0; r < 4; r++)
            for (int c = 0; c < 4; c++)
                sum += eta[r] * symMats[x][r, c] * eta[c] * symMats[y][c, r];
        inducedForm[x, y] = sum;
    }
var (formEv, _) = Jacobi(inducedForm);
int inducedPlus = formEv.Count(v => v > 1e-9);
int inducedMinus = formEv.Count(v => v < -1e-9);
double invarianceResidual = 0.0;
foreach (var rho in piLorentz)
{
    // rho^T B + B rho = 0
    for (int r = 0; r < symDim; r++)
        for (int c = 0; c < symDim; c++)
        {
            double sum = 0.0;
            for (int k = 0; k < symDim; k++)
                sum += rho[k, r] * inducedForm[k, c] + inducedForm[r, k] * rho[k, c];
            invarianceResidual = System.Math.Max(invarianceResidual, System.Math.Abs(sum));
        }
}
bool weldPreservesInducedForm = invarianceResidual <= Tolerance;

// ---------------------------------------------------------------------------
// N2: complexified coincidence. T4 = diag(i,1,1,1) carries eta to delta:
// T4^T eta-form -> delta. For each Lorentz generator X: X' = T4 X T4^{-1}
// must be complex-antisymmetric, and the induced map on Sym^2,
// T10: S -> T4 S T4^T, must intertwine: T10 pi_eta(X) T10^{-1} =
// pi_compact_C(X') where pi_compact is extended C-linearly via the
// coefficients of X' in the compact basis.
// ---------------------------------------------------------------------------

Complex[,] T10;
{
    var t4 = new Complex[4, 4];
    t4[0, 0] = Complex.ImaginaryOne;
    t4[1, 1] = Complex.One;
    t4[2, 2] = Complex.One;
    t4[3, 3] = Complex.One;
    T10 = new Complex[symDim, symDim];
    for (int col = 0; col < symDim; col++)
    {
        // T4 B_col T4^T
        var acted = new Complex[4, 4];
        for (int r = 0; r < 4; r++)
            for (int c = 0; c < 4; c++)
                acted[r, c] = t4[r, r] * symMats[col][r, c] * t4[c, c];
        for (int row = 0; row < symDim; row++)
        {
            Complex trace = Complex.Zero;
            for (int r = 0; r < 4; r++)
                for (int c = 0; c < 4; c++)
                    trace += symMats[row][r, c] * acted[c, r];
            T10[row, col] = trace;
        }
    }
}

double complexCoincidenceResidual = 0.0;
double antisymmetryResidualOfConjugated = 0.0;
for (int g = 0; g < 6; g++)
{
    var x = LorentzGenerator(g);
    // X' = T4 X T4^{-1}: entries x'[r,c] = t4[r] x[r,c] / t4[c]
    var xPrime = new Complex[4, 4];
    Complex[] t4d = [Complex.ImaginaryOne, Complex.One, Complex.One, Complex.One];
    for (int r = 0; r < 4; r++)
        for (int c = 0; c < 4; c++)
            xPrime[r, c] = t4d[r] * x[r, c] / t4d[c];
    for (int r = 0; r < 4; r++)
        for (int c = 0; c < 4; c++)
            antisymmetryResidualOfConjugated = System.Math.Max(antisymmetryResidualOfConjugated, (xPrime[r, c] + xPrime[c, r]).Magnitude);
    // coefficients of X' in the compact basis M_ij (i<j): c_ij = x'[i,j]
    var piCx = new Complex[symDim, symDim];
    for (int h = 0; h < 6; h++)
    {
        var (i, j) = so4Pairs[h];
        Complex coefficient = xPrime[i, j];
        if (coefficient.Magnitude < 1e-15)
            continue;
        for (int r = 0; r < symDim; r++)
            for (int c = 0; c < symDim; c++)
                piCx[r, c] += coefficient * piCompact[h][r, c];
    }
    // T10 pi_eta(X) T10^{-1} == piCx  <=>  T10 pi_eta(X) == piCx T10
    for (int r = 0; r < symDim; r++)
        for (int c = 0; c < symDim; c++)
        {
            Complex lhs = Complex.Zero, rhs = Complex.Zero;
            for (int k = 0; k < symDim; k++)
            {
                lhs += T10[r, k] * piLorentz[g][k, c];
                rhs += piCx[r, k] * T10[k, c];
            }
            complexCoincidenceResidual = System.Math.Max(complexCoincidenceResidual, (lhs - rhs).Magnitude);
        }
}
bool complexifiedWeldsCoincide = complexCoincidenceResidual <= 1e-9 && antisymmetryResidualOfConjugated <= 1e-12;

// ---------------------------------------------------------------------------
// N3: direct noncompact recomputation of the headline counts.
// V = 4 x 10 with D_eta(X) = X (x) I + I (x) pi_eta(X) (real 40x40).
// (a) linear singlet content: kernel of the stacked {D_eta(X_g)} (Gram).
// (b) bilinear spin-0: kernel of T -> D_eta T + T D_eta^T stacked (Gram on
//     the 1600-dim space).
// ---------------------------------------------------------------------------

const int VDim = 40;
double[,] WeldOnV(int g)
{
    var x = LorentzGenerator(g);
    var d = new double[VDim, VDim];
    for (int a = 0; a < 4; a++)
        for (int ap = 0; ap < 4; ap++)
            if (System.Math.Abs(x[a, ap]) > 1e-15)
                for (int m = 0; m < 10; m++)
                    d[a * 10 + m, ap * 10 + m] += x[a, ap];
    for (int m = 0; m < 10; m++)
        for (int mp = 0; mp < 10; mp++)
            if (System.Math.Abs(piLorentz[g][m, mp]) > 1e-15)
                for (int a = 0; a < 4; a++)
                    d[a * 10 + m, a * 10 + mp] += piLorentz[g][m, mp];
    return d;
}
var weldV = Enumerable.Range(0, 6).Select(WeldOnV).ToArray();

// (a) linear kernel
int noncompactLinearSingletDimension;
{
    var gram = new double[VDim, VDim];
    foreach (var d in weldV)
        for (int a = 0; a < VDim; a++)
            for (int b = 0; b < VDim; b++)
            {
                double sum = 0.0;
                for (int r = 0; r < VDim; r++)
                    sum += d[r, a] * d[r, b];
                gram[a, b] += sum;
            }
    var (ev, _) = Jacobi(gram);
    double scale = ev.Max();
    noncompactLinearSingletDimension = ev.Count(v => System.Math.Abs(v) <= scale * 1e-12);
}

// (b) bilinear spin-0 kernel on V (x) V (1600-dim Gram)
int noncompactBilinearSpinZeroDimension;
{
    int n2 = VDim * VDim;
    var gram = new double[n2, n2];
    foreach (var d in weldV)
    {
        // action A(T) = d T + T d^T as a matrix on vec(T): A = d (x) I + I (x) d
        // accumulate Gram += A^T A without materializing A fully:
        // A[(r,c),(k,l)] = d[r,k] delta_{c,l} + delta_{r,k} d[c,l]
        // Build A as dense (1600x1600) per generator (manageable: 2.56M doubles)
        var aMat = new double[n2, n2];
        for (int r = 0; r < VDim; r++)
            for (int c = 0; c < VDim; c++)
            {
                int row = r * VDim + c;
                for (int k = 0; k < VDim; k++)
                {
                    if (System.Math.Abs(d[r, k]) > 1e-15)
                        aMat[row, k * VDim + c] += d[r, k];
                    if (System.Math.Abs(d[c, k]) > 1e-15)
                        aMat[row, r * VDim + k] += d[c, k];
                }
            }
        Parallel.For(0, n2, a =>
        {
            for (int b = 0; b < n2; b++)
            {
                double sum = 0.0;
                for (int r = 0; r < n2; r++)
                    sum += aMat[r, a] * aMat[r, b];
                gram[a, b] += sum;
            }
        });
    }
    var (ev, _) = Jacobi(gram);
    double scale = ev.Max();
    noncompactBilinearSpinZeroDimension = ev.Count(v => System.Math.Abs(v) <= scale * 1e-12);
}

bool linearCountTransfers = noncompactLinearSingletDimension == 0;
bool bilinearCountTransfers = noncompactBilinearSpinZeroDimension == 7;

bool probeInternallyConsistent =
    phase409PrecursorPassed &&
    phase412PrecursorPassed &&
    lorentzWeldIsHomomorphism &&
    weldPreservesInducedForm &&
    complexifiedWeldsCoincide &&
    linearCountTransfers &&
    bilinearCountTransfers;

bool realFormTransferEstablished =
    complexifiedWeldsCoincide && linearCountTransfers && bilinearCountTransfers;

// ---------------------------------------------------------------------------
// Fail-closed boundary.
// ---------------------------------------------------------------------------

const bool unitaryRepresentationCategoryProbed = false; // named out of scope
const bool realStructureBookkeepingResolved = false;    // named residual caveat
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
const string ApplicationSubjectKind = "noncompact-real-form-transfer-probe";
const bool physicalTargetsConsultedForConstruction = false;

string targetBlindConstructionHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join(
    "|",
    ApplicationSubjectKind,
    "Lorentzian Sym^2 weld; induced-form signature; T4 complexified coincidence; direct noncompact linear+bilinear kernel recomputation")))).ToLowerInvariant();

bool noncompactRealFormTransferProbePassed =
    probeInternallyConsistent &&
    !unitaryRepresentationCategoryProbed &&
    !realStructureBookkeepingResolved &&
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

string terminalStatus = noncompactRealFormTransferProbePassed
    ? (realFormTransferEstablished
        ? "noncompact-real-form-transfer-established-no-gos-are-signature-independent"
        : "noncompact-real-form-transfer-probe-found-discrepancy")
    : "noncompact-real-form-transfer-probe-blocked";

string decision = noncompactRealFormTransferProbePassed
    ? (realFormTransferEstablished
        ? "The most plausible loophole is CLOSED at the finite-dimensional level: the Phase408-412 composite no-gos are REAL-FORM INDEPENDENT. (N1) The Lorentzian chimeric weld pi_eta: so(1,3) -> gl(10) on Sym^2(R^{1,3}) is an exact Lie homomorphism preserving the induced trace form, whose signature is machine-computed and recorded (connecting to the Phase406 Cl(7,3) signature axis). (N2) THE KEYSTONE: the complexified Lorentzian weld coincides exactly with the complexified compact weld under the explicit change of basis T4 = diag(i,1,1,1) - so every complex-linear kernel dimension (which is what every Phase408-412 no-go is) transfers verbatim to the noncompact form; no noncompact-only invariant can exist on this chain's finite-dimensional carriers. (N3) Direct fail-closed recomputation on the noncompact form confirms the transfer: the welded frame-cross block has ZERO linear singlet content and EXACTLY 7 bilinear spin-0 dimensions, matching the compact counts. NAMED RESIDUALS: real-structure (Majorana-type) bookkeeping can relabel real dimensions but cannot create complex invariants (recorded, not resolved); the unitary infinite-dimensional representation category is out of scope. The remaining named routes reduce to: the draft's unobserved-phase fields, or a new primary-source specification. Nothing is promoted; no contract field is filled."
        : "A real-form DISCREPANCY was detected - the recorded residuals identify where; the no-go transfer fails and the noncompact route must be characterized by a refinement phase before any conclusion. Nothing is promoted; no contract field is filled.")
    : "Do not use the transfer claim until the precursors and the coincidence/recomputation battery pass.";

var result = new
{
    phaseId = "phase413-noncompact-real-form-transfer-probe",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    noncompactRealFormTransferProbePassed,
    phase409PrecursorPassed,
    phase412PrecursorPassed,
    probeInternallyConsistent,
    lorentzWeldIsHomomorphism,
    lorentzHomResidual,
    inducedFormSignature = new { plus = inducedPlus, minus = inducedMinus },
    weldPreservesInducedForm,
    invarianceResidual,
    complexifiedWeldsCoincide,
    complexCoincidenceResidual,
    antisymmetryResidualOfConjugated,
    noncompactLinearSingletDimension,
    linearCountTransfers,
    compactLinearSingletDimension = 0,
    noncompactBilinearSpinZeroDimension,
    bilinearCountTransfers,
    compactBilinearSpinZeroDimension = 7,
    realFormTransferEstablished,
    unitaryRepresentationCategoryProbed,
    realStructureBookkeepingResolved,
    physicalCouplingProvided,
    probeDefinitions = new
    {
        n1 = "pi_eta: so(1,3) -> gl(10) via S -> X S + S X^T on Sym^2(R^{1,3}); homomorphism via exact basis expansion; induced form B(S,T) = Tr(eta S eta T) with machine signature and invariance check",
        n2 = "T4 = diag(i,1,1,1) carries eta to delta; verify T10 pi_eta(X) T10^{-1} = pi_compact(T4 X T4^{-1}) (C-linear extension) for all six generators",
        n3 = "direct noncompact kernels: stacked-generator Gram on V = 4 x 10 (linear) and on V (x) V (bilinear); counts must equal the compact 0 and 7",
        n4 = "named residuals: real/Majorana structure bookkeeping (cannot create complex invariants); unitary representation category out of scope",
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
        "the transfer covers FINITE-DIMENSIONAL functorial carriers of the welded chain; the unitary (infinite-dimensional) representation category of the noncompact groups is out of scope",
        "real/Majorana structure bookkeeping is a named residual: it can change real-type labels and real dimension conventions but cannot create complex-invariant directions",
        "the signature eta = diag(-1,1,1,1) realizes the physical Lorentzian slice; other signatures complexify to the same so(10, C) embedding by the same argument",
        "no dynamics, no scales, no VEV; the binding gaps are unchanged",
        "no Phase201 or Phase256 fill",
        "no physical predictions",
    },
    sourceEvidence = new
    {
        phase409SummaryPath = Phase409SummaryPath,
        phase412SummaryPath = Phase412SummaryPath,
        loopholeContext = "docs/Reference/ExperimentReferences/DEEP-RESEARCH-20260612.md (noncompact-only invariant named as the most plausible evasion)",
        primaryDraft = "docs/Reference/ExperimentReferences/texts/GU-DRAFT-2021-TEXT.txt (sections 2.3, 9; Spin(6,4) chain)",
    },
    decision,
};

var options = JsonOptions();
File.WriteAllText(Path.Combine(outputDir, "noncompact_real_form_transfer_probe.json"), JsonSerializer.Serialize(result, options));
string summaryPath = Path.Combine(outputDir, "noncompact_real_form_transfer_probe_summary.json");
File.WriteAllText(summaryPath, JsonSerializer.Serialize(result, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"noncompactRealFormTransferProbePassed={noncompactRealFormTransferProbePassed}");
Console.WriteLine($"lorentzWeldIsHomomorphism={lorentzWeldIsHomomorphism} (residual {lorentzHomResidual:E3})");
Console.WriteLine($"inducedFormSignature=({inducedPlus},{inducedMinus}) preserved={weldPreservesInducedForm} (residual {invarianceResidual:E3})");
Console.WriteLine($"complexifiedWeldsCoincide={complexifiedWeldsCoincide} (residual {complexCoincidenceResidual:E3})");
Console.WriteLine($"noncompactLinearSingletDimension={noncompactLinearSingletDimension} (compact 0, transfers={linearCountTransfers})");
Console.WriteLine($"noncompactBilinearSpinZeroDimension={noncompactBilinearSpinZeroDimension} (compact 7, transfers={bilinearCountTransfers})");
Console.WriteLine($"realFormTransferEstablished={realFormTransferEstablished}");
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
