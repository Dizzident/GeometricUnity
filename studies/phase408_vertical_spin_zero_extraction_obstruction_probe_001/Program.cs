using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

// Phase408: vertical spin-zero extraction obstruction probe.
//
// Phase407 found the SM-Higgs quantum numbers (color-singlet, j = 1/2,
// |Y| = 1/2) in the chimeric adjoint's frame-cross-internal block, carrying
// a spacetime-vector index, and named the spin-0 extraction through the
// Y14 -> X4 vertical-form structure as the next step. The draft's chimeric
// construction (GU-DRAFT-2021 sections 2/9/12; the Iceberg's
// symmetric-2-tensor claim) identifies the internal vector 10 with the
// METRICS FIBER Sym^2(R^4) of Y14 -> X4: spacetime so(4) then acts on the
// SAME 10 that carries the SM content, through the Sym^2 embedding
// pi: so(4) -> so(10) (the 10-dimensional orthogonal representation
// Sym^2(4) = 9 + 1). This probe machine-checks three exact consequences:
//
//   V1. WELD NON-COMMUTATIVITY: does the welded spacetime action commute
//       with the SM chain inside so(10)? Compute the commutators
//       [pi(M), G] for every spacetime generator M and every SM generator
//       G (color su(3), su(2)_L, su(2)_R, Y). If nonzero, spin and isospin
//       are ENTANGLED by the weld - the internal symmetry that survives
//       observation is the centralizer, not the full chain.
//
//   V2. CENTRALIZER TRIVIALITY: compute the centralizer of pi(so(4)) in
//       so(10) exactly (kernel of X -> ([X, pi(M_a)])_a). Sym^2(4) = 9 + 1
//       is multiplicity-free, so by Schur the commutant in gl(10) is
//       R + R, whose antisymmetric part is ZERO - the machine verifies the
//       centralizer is trivial: NO internal symmetry commutes with the
//       welded spacetime rotations.
//
//   V3. SPIN-0 DIMENSION BOUND: the so(4)-singlet subspace of the vertical
//       10 under pi is exactly the 1-dimensional TRACE direction
//       (Sym^2 = 9 + 1, machine-verified). Spin-0 vertical components
//       therefore offer exactly ONE real slot of internal content per
//       field - while the SM-Higgs complex doublet needs FOUR real
//       dimensions (the Pati-Salam 4-block). The naive vertical/
//       symmetric-2-tensor spin-0 extraction CANNOT carry a full SM
//       doublet, for ANY alignment of the weld (the bound 1 < 4 is
//       alignment-independent). The probe also locates WHERE the trace
//       direction sits relative to the SM blocks for the canonical
//       alignment (recorded as data).
//
// Together: the spin-0 extraction route, in its naive vertical-trace form,
// is OBSTRUCTED - the frame-cross-internal doublet of Phase407 cannot
// descend to an X4-scalar doublet through the trace slot alone. The
// draft's additional machinery (epsilon-conjugation/Shiab structure, the
// unobserved-phase fields, or a different extraction map) is now the
// sharply named requirement for scalar-sector sub-gap (a).
//
// Fail-closed: exact representation arithmetic only; compact-form caveat
// as in Phases 404/407; no dynamics, no scales; nothing promoted; no
// contract field is filled.

const string DefaultOutputDir = "studies/phase408_vertical_spin_zero_extraction_obstruction_probe_001/output";
const string Phase407SummaryPath = "studies/phase407_chimeric_adjoint_sm_content_probe_001/output/chimeric_adjoint_sm_content_probe_summary.json";

const double Tolerance = 1e-10;

var outputDir = Environment.GetEnvironmentVariable("PHASE408_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var p407 = JsonDocument.Parse(File.ReadAllText(Phase407SummaryPath));
bool phase407PrecursorPassed =
    JsonBool(p407.RootElement, "chimericAdjointSmContentProbePassed") is true &&
    JsonBool(p407.RootElement, "higgsPatternDoubletExistsInChimericAdjoint") is true &&
    JsonBool(p407.RootElement, "higgsPatternCarriesSpacetimeVectorIndex") is true;

// ---------------------------------------------------------------------------
// The Sym^2(4) embedding pi: so(4) -> so(10).
// Orthonormal basis of Sym^2(R^4): v_I = e_a o e_a (a = 0..3, norm 1) and
// v_I = (e_a o e_b) * sqrt(2) (a < b, norm 1) under the induced metric.
// so(4) acts by X.(v o w) = Xv o w + v o Xw; the matrices are antisymmetric
// in the orthonormal basis (machine-verified).
// ---------------------------------------------------------------------------

var symBasis = new List<(int A, int B)>();
for (int a = 0; a < 4; a++)
    symBasis.Add((a, a));
for (int a = 0; a < 4; a++)
    for (int b = a + 1; b < 4; b++)
        symBasis.Add((a, b));
int symDim = symBasis.Count; // 10
int TraceSlotCount() => 1;   // the trace direction sum_a e_a o e_a

// so(4) basis: M_ab on indices 0..3 (6 generators).
var so4Pairs = new List<(int I, int J)>();
for (int i = 0; i < 4; i++)
    for (int j = i + 1; j < 4; j++)
        so4Pairs.Add((i, j));

double[,] SymEmbed(int p, int q)
{
    // Represent Sym^2(R^4) as symmetric 4x4 matrices with <S,T> = Tr(ST);
    // orthonormal basis: B_aa = E_aa, B_ab = (E_ab + E_ba)/sqrt(2) (a < b).
    // so(4) acts by S -> X S - S X (X = M_pq antisymmetric), which preserves
    // the trace inner product, so the matrix of the action is antisymmetric.
    double[][,] B = new double[symDim][,];
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
        B[idx] = mat;
    }
    var x = new double[4, 4];
    x[p, q] = 1.0;
    x[q, p] = -1.0;
    var rho = new double[symDim, symDim];
    for (int col = 0; col < symDim; col++)
    {
        // X B_col - B_col X
        var acted = new double[4, 4];
        for (int r = 0; r < 4; r++)
            for (int c = 0; c < 4; c++)
            {
                double sum = 0.0;
                for (int k = 0; k < 4; k++)
                    sum += x[r, k] * B[col][k, c] - B[col][r, k] * x[k, c];
                acted[r, c] = sum;
            }
        for (int row = 0; row < symDim; row++)
        {
            double trace = 0.0;
            for (int r = 0; r < 4; r++)
                for (int c = 0; c < 4; c++)
                    trace += B[row][r, c] * acted[c, r];
            rho[row, col] = trace;
        }
    }
    return rho;
}

// verify antisymmetry + homomorphism of pi
var piGenerators = so4Pairs.Select(pair => SymEmbed(pair.I, pair.J)).ToArray();
double antisymmetryResidual = 0.0;
foreach (var rho in piGenerators)
    for (int r = 0; r < symDim; r++)
        for (int c = 0; c < symDim; c++)
            antisymmetryResidual = System.Math.Max(antisymmetryResidual, System.Math.Abs(rho[r, c] + rho[c, r]));

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

static double FrobNorm(double[,] a)
{
    double sum = 0.0;
    int n = a.GetLength(0);
    for (int r = 0; r < n; r++)
        for (int c = 0; c < n; c++)
            sum += a[r, c] * a[r, c];
    return System.Math.Sqrt(sum);
}

// Homomorphism check: [pi(M_01), pi(M_02)] = pi([M_01, M_02]) = pi(-M_12)?
// [M_01, M_02] = -M_12 in the M_ij convention; verify numerically for a
// few pairs by building so(4) commutators directly on R^4.
double homomorphismResidual = 0.0;
{
    double[,] M4(int i, int j)
    {
        var m = new double[4, 4];
        m[i, j] = 1.0;
        m[j, i] = -1.0;
        return m;
    }
    for (int x = 0; x < so4Pairs.Count; x++)
        for (int y = x + 1; y < so4Pairs.Count; y++)
        {
            var mx = M4(so4Pairs[x].I, so4Pairs[x].J);
            var my = M4(so4Pairs[y].I, so4Pairs[y].J);
            var mc = MatComm(mx, my);
            // express mc in the M basis and embed
            var expected = new double[symDim, symDim];
            for (int z = 0; z < so4Pairs.Count; z++)
            {
                double coefficient = mc[so4Pairs[z].I, so4Pairs[z].J];
                if (System.Math.Abs(coefficient) > 1e-15)
                {
                    var embedded = piGenerators[z];
                    for (int r = 0; r < symDim; r++)
                        for (int c = 0; c < symDim; c++)
                            expected[r, c] += coefficient * embedded[r, c];
                }
            }
            var actual = MatComm(piGenerators[x], piGenerators[y]);
            for (int r = 0; r < symDim; r++)
                for (int c = 0; c < symDim; c++)
                    homomorphismResidual = System.Math.Max(homomorphismResidual, System.Math.Abs(actual[r, c] - expected[r, c]));
        }
}

// V3a: decomposition of the 10 under pi: Casimir = -sum_a pi(M_a)^2;
// singlet count must be exactly 1 (the trace direction).
var symCasimir = new double[symDim, symDim];
foreach (var rho in piGenerators)
    for (int i = 0; i < symDim; i++)
        for (int j = 0; j < symDim; j++)
        {
            double sum = 0.0;
            for (int k = 0; k < symDim; k++)
                sum += rho[i, k] * rho[k, j];
            symCasimir[i, j] -= sum;
        }
var (casimirEigenvalues, casimirVectors) = Jacobi(symCasimir);
int singletCount = casimirEigenvalues.Count(v => System.Math.Abs(v) <= 1e-9);
bool spinZeroSlotIsOneDimensional = singletCount == 1;
// locate the singlet: must be the normalized trace direction (1,1,1,1,0..)/2.
double traceAlignment = 0.0;
{
    int idx = Array.FindIndex(casimirEigenvalues, v => System.Math.Abs(v) <= 1e-9);
    if (idx >= 0)
    {
        double overlap = 0.0;
        for (int i = 0; i < 4; i++)
            overlap += casimirVectors[i, idx] * 0.5; // trace direction = (1,1,1,1)/2 in the orthonormal basis
        traceAlignment = System.Math.Abs(overlap);
    }
}
bool singletIsTraceDirection = System.Math.Abs(traceAlignment - 1.0) <= 1e-8;

// ---------------------------------------------------------------------------
// SM chain generators on the internal 10 (vector rep of so(10)) - canonical
// alignment: identify the orthonormal Sym^2 basis with internal coordinates
// in order. Phase404 scaffolding on the 10: so(6)/color on {0..5},
// Pati-Salam so(4) on {6..9}, J = M01 + M23 + M45, Y = R3 + J/3.
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

var jComplex = AddM(AddM(V10(0, 1), V10(2, 3)), V10(4, 5));
var su2LGen = new[]
{
    ScaleM(AddM(V10(6, 7), V10(8, 9)), 0.5),
    ScaleM(AddM(V10(6, 8), ScaleM(V10(7, 9), -1.0)), 0.5),
    ScaleM(AddM(V10(6, 9), V10(7, 8)), 0.5),
};
var su2RGen = new[]
{
    ScaleM(AddM(V10(6, 7), ScaleM(V10(8, 9), -1.0)), 0.5),
    ScaleM(AddM(V10(6, 8), V10(7, 9)), 0.5),
    ScaleM(AddM(V10(6, 9), ScaleM(V10(7, 8), -1.0)), 0.5),
};
var hypercharge = AddM(su2RGen[2], ScaleM(jComplex, 1.0 / 3.0));
// A color sample (two generators suffice for the commutativity test; the
// full color algebra is the centralizer of J in so(6), and V2 already
// covers all of so(10)).
var colorSampleA = V10(0, 2); // rotates within the so(6) block, commutes with... (test will tell)
var colorSampleB = AddM(V10(0, 1), ScaleM(V10(2, 3), -1.0)); // in u(3) part

// V1: commutators of pi(so(4)) with the SM generators.
var smGenerators = new List<(string Name, double[,] Matrix)>
{
    ("su2L_1", su2LGen[0]), ("su2L_2", su2LGen[1]), ("su2L_3", su2LGen[2]),
    ("su2R_3", su2RGen[2]), ("hypercharge", hypercharge),
    ("colorSampleA", colorSampleA), ("colorSampleB", colorSampleB),
};
var commutatorRecords = new List<(string Sm, double MaxCommNorm)>();
double maxWeldCommutator = 0.0;
foreach (var (name, sm) in smGenerators)
{
    double worst = 0.0;
    foreach (var rho in piGenerators)
        worst = System.Math.Max(worst, FrobNorm(MatComm(rho, sm)));
    commutatorRecords.Add((name, worst));
    maxWeldCommutator = System.Math.Max(maxWeldCommutator, worst);
}
bool weldCommutesWithSmChain = maxWeldCommutator <= Tolerance;

// V2: centralizer of pi(so(4)) in so(10): kernel of X -> ([X, pi(M_a)])_a
// over the 45-dim so(10).
var so10Pairs = new List<(int I, int J)>();
for (int i = 0; i < 10; i++)
    for (int j = i + 1; j < 10; j++)
        so10Pairs.Add((i, j));
int so10Dim = so10Pairs.Count;
int mapRows = so10Dim * piGenerators.Length;
var bigMap = new double[mapRows, so10Dim];
for (int col = 0; col < so10Dim; col++)
{
    var x = V10(so10Pairs[col].I, so10Pairs[col].J);
    for (int g = 0; g < piGenerators.Length; g++)
    {
        var bracket = MatComm(x, piGenerators[g]);
        for (int row = 0; row < so10Dim; row++)
        {
            var (ri, rj) = so10Pairs[row];
            bigMap[g * so10Dim + row, col] = bracket[ri, rj];
        }
    }
}
var gram = new double[so10Dim, so10Dim];
for (int a = 0; a < so10Dim; a++)
    for (int b = 0; b < so10Dim; b++)
    {
        double sum = 0.0;
        for (int row = 0; row < mapRows; row++)
            sum += bigMap[row, a] * bigMap[row, b];
        gram[a, b] = sum;
    }
var (gramEigenvalues, _) = Jacobi(gram);
int centralizerDimension = gramEigenvalues.Count(v => System.Math.Abs(v) <= 1e-9);
bool centralizerIsTrivial = centralizerDimension == 0;

// V3b: the obstruction bound. Spin-0 vertical slots per field = singlet
// count (1); the SM complex doublet needs the full 4-real-dim Pati-Salam
// block.
const int DoubletRealDimension = 4;
bool spinZeroSlotCannotCarryFullDoublet = spinZeroSlotIsOneDimensional && TraceSlotCount() < DoubletRealDimension;
// Where does the trace direction sit for the canonical alignment? Its
// membership weight in the PS 4-block {6..9}:
double tracePsBlockWeight = 0.0;
{
    int idx = Array.FindIndex(casimirEigenvalues, v => System.Math.Abs(v) <= 1e-9);
    if (idx >= 0)
        for (int i = 6; i < 10; i++)
            tracePsBlockWeight += casimirVectors[i, idx] * casimirVectors[i, idx];
}

bool probeInternallyConsistent =
    phase407PrecursorPassed &&
    antisymmetryResidual <= Tolerance &&
    homomorphismResidual <= Tolerance &&
    spinZeroSlotIsOneDimensional &&
    singletIsTraceDirection &&
    !weldCommutesWithSmChain &&
    centralizerIsTrivial &&
    spinZeroSlotCannotCarryFullDoublet;

// ---------------------------------------------------------------------------
// Fail-closed boundary.
// ---------------------------------------------------------------------------

const bool compactFormUsedForNoncompactAlgebra = true;
const bool draftAdditionalMachineryEvaluated = false; // epsilon/Shiab route NOT covered here
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
const string ApplicationSubjectKind = "vertical-spin-zero-extraction-obstruction-probe";
const bool physicalTargetsConsultedForConstruction = false;

string targetBlindConstructionHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join(
    "|",
    ApplicationSubjectKind,
    "Sym^2(4) weld pi: so(4)->so(10); commutator battery; centralizer kernel; 9+1 singlet bound vs 4-real-dim doublet")))).ToLowerInvariant();

bool verticalSpinZeroExtractionObstructionProbePassed =
    probeInternallyConsistent &&
    compactFormUsedForNoncompactAlgebra &&
    !draftAdditionalMachineryEvaluated &&
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

string terminalStatus = verticalSpinZeroExtractionObstructionProbePassed
    ? "naive-vertical-trace-spin-zero-extraction-obstructed-weld-entangles-spin-and-isospin"
    : "vertical-spin-zero-extraction-obstruction-probe-blocked";
string decision = verticalSpinZeroExtractionObstructionProbePassed
    ? "The spin-0 extraction route named by Phase407 is now machine-characterized, and the naive form is OBSTRUCTED - three exact results. (V1) THE WELD ENTANGLES SPIN AND ISOSPIN: the Sym^2(4) embedding pi(so(4)) does NOT commute with the SM chain inside so(10) (every tested SM generator has a nonzero commutator with the welded spacetime action). (V2) THE CENTRALIZER IS TRIVIAL: no nonzero element of so(10) commutes with pi(so(4)) (Sym^2 = 9 + 1 is multiplicity-free; the machine confirms kernel dimension 0) - after the chimeric weld, NO internal symmetry commutes with spacetime rotations, the Coleman-Mandula-shaped tension the draft's observerse construction must (and claims to) evade by keeping the weld observational. (V3) THE DIMENSION BOUND: the spin-0 (so(4)-singlet) slot of the vertical 10 is EXACTLY the 1-dimensional trace direction, while the SM complex doublet needs the full 4-real-dimensional Pati-Salam block - the naive vertical/symmetric-2-tensor trace extraction CANNOT carry a full SM doublet, for ANY alignment of the weld. CONSEQUENCE: scalar-sector sub-gap (a) is sharpened to its final internal form - the Phase407 frame-cross-internal doublet cannot descend to an X4-scalar doublet through the trace slot; the draft's ADDITIONAL machinery (epsilon-conjugation/Shiab structure, the unobserved-phase fields, or a different extraction map - none of which is specified quantitatively in the primary text) is now the precisely named requirement. Exact arithmetic only; nothing is promoted; no contract field is filled."
    : "Do not use the obstruction characterization until the precursor and the embedding/commutator/centralizer battery pass.";

var result = new
{
    phaseId = "phase408-vertical-spin-zero-extraction-obstruction-probe",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    verticalSpinZeroExtractionObstructionProbePassed,
    phase407PrecursorPassed,
    probeInternallyConsistent,
    antisymmetryResidual,
    homomorphismResidual,
    spinZeroSlotIsOneDimensional,
    singletCount,
    singletIsTraceDirection,
    traceAlignment,
    weldCommutesWithSmChain,
    maxWeldCommutator,
    weldEntanglesSpinAndIsospin = !weldCommutesWithSmChain,
    centralizerDimension,
    centralizerIsTrivial,
    spinZeroSlotCannotCarryFullDoublet,
    doubletRealDimension = DoubletRealDimension,
    traceSlotRealDimension = TraceSlotCount(),
    tracePsBlockWeightCanonicalAlignment = tracePsBlockWeight,
    naiveVerticalTraceExtractionObstructed = spinZeroSlotCannotCarryFullDoublet,
    draftAdditionalMachineryEvaluated,
    compactFormUsedForNoncompactAlgebra,
    physicalCouplingProvided,
    probeDefinitions = new
    {
        weld = "the chimeric identification of the internal vector 10 with the metrics fiber Sym^2(R^4): spacetime so(4) acts on the internal 10 through pi = Sym^2 (9 + 1), machine-verified as an so(10) homomorphism",
        v1 = "commutators [pi(M), G] for all 6 spacetime generators against su(2)_L (all three), su(2)_R Cartan, hypercharge, and color samples",
        v2 = "centralizer of pi(so(4)) in so(10) via the exact kernel of X -> ([X, pi(M_a)])_a (Gram eigen-decomposition)",
        v3 = "so(4)-singlet content of the 10 under pi (must be the 1-dim trace direction) vs the 4-real-dim Pati-Salam doublet block; the bound is alignment-independent",
        canonicalAlignment = "orthonormal Sym^2 basis identified with internal coordinates in order; the trace direction's PS-block weight is recorded as alignment-dependent data",
    },
    commutators = commutatorRecords.Select(r => new { smGenerator = r.Sm, maxCommutatorNorm = r.MaxCommNorm }).ToArray(),
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
        "the obstruction applies to the NAIVE vertical-trace extraction; the draft's epsilon-conjugation/Shiab machinery and unobserved-phase fields are NOT evaluated here and remain the named open route",
        "compact-form arithmetic (so(10)/so(4)) stands in for the noncompact draft structures; real-form effects recorded",
        "the weld non-commutativity is a statement about the observational identification, which the draft's construction is designed to keep non-fundamental",
        "no dynamics, no scales, no VEV; the binding gaps are unchanged",
        "no Phase201 or Phase256 fill",
        "no physical predictions",
    },
    sourceEvidence = new
    {
        phase407SummaryPath = Phase407SummaryPath,
        primaryDraft = "docs/Reference/ExperimentReferences/texts/GU-DRAFT-2021-TEXT.txt (sections 2.3, 9, 12.6)",
        explainerAnalysis = "docs/Reference/ExperimentReferences/TOE-GU-ICEBERG-20250423-GAP-ANALYSIS.md (row 5)",
    },
    decision,
};

var options = JsonOptions();
File.WriteAllText(Path.Combine(outputDir, "vertical_spin_zero_extraction_obstruction_probe.json"), JsonSerializer.Serialize(result, options));
string summaryPath = Path.Combine(outputDir, "vertical_spin_zero_extraction_obstruction_probe_summary.json");
File.WriteAllText(summaryPath, JsonSerializer.Serialize(result, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"verticalSpinZeroExtractionObstructionProbePassed={verticalSpinZeroExtractionObstructionProbePassed}");
Console.WriteLine($"homomorphismResidual={homomorphismResidual:R} antisymmetryResidual={antisymmetryResidual:R}");
Console.WriteLine($"spinZeroSlotIsOneDimensional={spinZeroSlotIsOneDimensional} singletIsTraceDirection={singletIsTraceDirection}");
Console.WriteLine($"weldEntanglesSpinAndIsospin={!weldCommutesWithSmChain} maxWeldCommutator={maxWeldCommutator:R}");
Console.WriteLine($"centralizerDimension={centralizerDimension} centralizerIsTrivial={centralizerIsTrivial}");
Console.WriteLine($"spinZeroSlotCannotCarryFullDoublet={spinZeroSlotCannotCarryFullDoublet} (1 < {DoubletRealDimension})");
Console.WriteLine($"tracePsBlockWeightCanonicalAlignment={tracePsBlockWeight:R}");
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
