using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

// Phase417: vector-spinor 144 decomposition probe.
//
// Phase416 sharpened the unobserved-phase residual to the source-pinned
// Z_{1/2} carrier: the vector-spinor 144 remainder in 10 x 16 = 16 + 144.
// This phase materializes that representation directly, not as a promotion
// route. It builds the gamma-trace map 10 x 16+ -> 16-, takes its 144-complex-
// dimensional kernel, then decomposes the kernel under the same SM and welded
// spin actions used by Phases407/411.
//
// Fail-closed: representation bookkeeping only. The current source does not
// define an even-composite map, bosonic projection, action, VEV selection,
// observed photon/W/Z/H rows, weak-angle lineage, pole extraction, or
// GeV/unit normalization, so no contract field can be filled.

const string DefaultOutputDir = "studies/phase417_vector_spinor_144_decomposition_probe_001/output";
const string Phase411SummaryPath = "studies/phase411_quartic_dirac_squared_spinor_composite_probe_001/output/quartic_dirac_squared_spinor_composite_probe_summary.json";
const string Phase416SummaryPath = "studies/phase416_unobserved_phase_carrier_census_001/output/unobserved_phase_carrier_census_summary.json";
const string ApplicationSubjectKind = "vector-spinor-144-decomposition-probe";

var outputDir = Environment.GetEnvironmentVariable("PHASE417_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase411 = JsonDocument.Parse(File.ReadAllText(Phase411SummaryPath));
using var phase416 = JsonDocument.Parse(File.ReadAllText(Phase416SummaryPath));

bool phase411PrecursorPassed =
    JsonBool(phase411.RootElement, "quarticDiracSquaredSpinorCompositeProbePassed") is true &&
    JsonBool(phase411.RootElement, "spinorBilinearSpinZeroDoubletAbsent") is true;

bool phase416PrecursorPassed =
    JsonBool(phase416.RootElement, "unobservedPhaseCarrierCensusPassed") is true &&
    JsonBool(phase416.RootElement, "vectorSpinor144RemainsConcreteNextCarrier") is true &&
    JsonBool(phase416.RootElement, "unobservedPhaseStillRequiresBosonicMap") is true;

// ---------------------------------------------------------------------------
// Cl(10), chiral 16+ / 16-, Sym^2 weld, and the SM chain.
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

// Sym^2 weld pi: so(4) -> so(10), same orthonormal basis convention as
// Phases408/411.
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

// ---------------------------------------------------------------------------
// Direct gamma-trace split 10 x 16+ -> 16-.
// ---------------------------------------------------------------------------

const int VectorDim = 10;
const int SpinorDim = 16;
const int VectorSpinorDim = VectorDim * SpinorDim;

var gammaTrace = new Complex[SpinorDim, VectorSpinorDim];
for (int i = 0; i < VectorDim; i++)
{
    var gammaMinusPlus = Restrict(gammas[i], minusAxes, plusAxes);
    for (int r = 0; r < SpinorDim; r++)
        for (int c = 0; c < SpinorDim; c++)
            gammaTrace[r, i * SpinorDim + c] = gammaMinusPlus[r, c];
}

var gammaTraceReal = RealifyRectangular(gammaTrace);
var gammaGram = Gram(gammaTraceReal);
var (gammaGramEigenvalues, gammaGramVectors) = Jacobi(gammaGram);
var kernelBasis = new List<double[]>();
int gammaTraceRankReal = 0;
for (int e = 0; e < gammaGramEigenvalues.Length; e++)
{
    if (gammaGramEigenvalues[e] > 1e-8)
    {
        gammaTraceRankReal++;
        continue;
    }
    var v = new double[2 * VectorSpinorDim];
    for (int r = 0; r < v.Length; r++)
        v[r] = gammaGramVectors[r, e];
    kernelBasis.Add(v);
}

int vectorSpinor144KernelRealDimension = kernelBasis.Count;
int gammaTraceRankComplex = gammaTraceRankReal / 2;
int vectorSpinor144KernelComplexDimension = vectorSpinor144KernelRealDimension / 2;
double gammaTraceKernelMaxResidual = MaxMapResidual(gammaTraceReal, kernelBasis);
bool vectorSpinor144DimensionCheck =
    gammaTraceRankReal == 32 &&
    gammaTraceRankComplex == 16 &&
    vectorSpinor144KernelRealDimension == 288 &&
    vectorSpinor144KernelComplexDimension == 144 &&
    gammaTraceKernelMaxResidual <= 1e-8;

Complex[,] VectorSpinorOp(double[,] vectorOp)
{
    var spinOp = Sigma16Plus(vectorOp);
    var result = new Complex[VectorSpinorDim, VectorSpinorDim];
    for (int a = 0; a < VectorDim; a++)
        for (int b = 0; b < VectorDim; b++)
            if (Math.Abs(vectorOp[a, b]) > 1e-15)
                for (int m = 0; m < SpinorDim; m++)
                    result[a * SpinorDim + m, b * SpinorDim + m] += vectorOp[a, b];
    for (int a = 0; a < VectorDim; a++)
        for (int m = 0; m < SpinorDim; m++)
            for (int n = 0; n < SpinorDim; n++)
                if (spinOp[m, n].Magnitude > 1e-15)
                    result[a * SpinorDim + m, a * SpinorDim + n] += spinOp[m, n];
    return result;
}

var weldedVectorSpinorOps = piGenerators.Select(VectorSpinorOp).Select(Realify).ToArray();
var su2LVectorSpinorOps = su2LGen.Select(VectorSpinorOp).Select(Realify).ToArray();
var hyperchargeVectorSpinorOp = Realify(VectorSpinorOp(hypercharge));
var colorVectorSpinorOps = colorGenerators.Select(VectorSpinorOp).Select(Realify).ToArray();

double kernelInvarianceMaxResidual = 0.0;
foreach (var op in weldedVectorSpinorOps.Concat(su2LVectorSpinorOps).Concat(new[] { hyperchargeVectorSpinorOp }).Concat(colorVectorSpinorOps))
    kernelInvarianceMaxResidual = Math.Max(kernelInvarianceMaxResidual, MaxMapResidual(gammaTraceReal, ApplyToBasis(op, kernelBasis)));

bool vectorSpinor144InvariantUnderProbedGenerators = kernelInvarianceMaxResidual <= 1e-7;

var welded144Ops = weldedVectorSpinorOps.Select(op => RestrictReal(op, kernelBasis)).ToArray();
var su2L144Ops = su2LVectorSpinorOps.Select(op => RestrictReal(op, kernelBasis)).ToArray();
var hypercharge144Op = RestrictReal(hyperchargeVectorSpinorOp, kernelBasis);
var color144Ops = colorVectorSpinorOps.Select(op => RestrictReal(op, kernelBasis)).ToArray();

// ---------------------------------------------------------------------------
// Welded and SM decompositions of the 144 kernel.
// ---------------------------------------------------------------------------

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
    var aCoeffsForKappa = BuildSu2Coefficients(so4Pairs).A;
    var aGens = aCoeffsForKappa.Select(c => CombineComplex(vectorGens, c)).ToArray();
    var cA = CasimirRealComplex(aGens);
    kappaCalibration = cA[0, 0] / 0.75;
}

var (aCoefficients, bCoefficients) = BuildSu2Coefficients(so4Pairs);
var welded144Content = WeldedContent(welded144Ops, kappaCalibration, aCoefficients, bCoefficients, out bool welded144ContentRecovered);

var gamma4 = BuildGamma4(paulis);
var (twoLGens, twoRGens, weylHalvesAreTwoDimensional, sigma4HomResidual) = BuildSpacetimeWeylGenerators(gamma4, so4Pairs);
var content2L = WeldedContent(twoLGens.Select(Realify).ToArray(), kappaCalibration, aCoefficients, bCoefficients, out bool ok2L);
var content2R = WeldedContent(twoRGens.Select(Realify).ToArray(), kappaCalibration, aCoefficients, bCoefficients, out bool ok2R);
var label2L = content2L.Count == 1 ? (content2L[0].J1, content2L[0].J2) : (-1.0, -1.0);
var label2R = content2R.Count == 1 ? (content2R[0].J1, content2R[0].J2) : (-1.0, -1.0);

var zCarrierLeftContent = TensorContent(label2L, welded144Content);
var zCarrierRightContent = TensorContent(label2R, welded144Content);
int linearWeldedScalarCountLeft = zCarrierLeftContent.Where(t => IsZero(t.J1) && IsZero(t.J2)).Sum(t => t.Multiplicity);
int linearWeldedScalarCountRight = zCarrierRightContent.Where(t => IsZero(t.J1) && IsZero(t.J2)).Sum(t => t.Multiplicity);
int linearWeldedScalarCountTotal = linearWeldedScalarCountLeft + linearWeldedScalarCountRight;
bool vectorSpinor144LinearCarrierHasNoWeldedScalar = linearWeldedScalarCountTotal == 0;

var colorCasimir144 = CasimirRealMatrices(color144Ops);
var su2LCasimir144 = CasimirRealMatrices(su2L144Ops);
var ySquared144 = CasimirRealMatrices(new[] { hypercharge144Op });
var smBlocks = SmBlockCensus(colorCasimir144, su2LCasimir144, ySquared144);

var su2LOn16 = su2LGen.Select(Sigma16Plus).ToArray();
var hyperchargeOn16 = Sigma16Plus(hypercharge);
var calSu2L = CasimirRealComplex(su2LOn16);
var (calEvL, _) = Jacobi(calSu2L);
double jHalfValue = calEvL.Max();
var calY = CasimirRealComplex(new[] { hyperchargeOn16 });
var (calEvY, _) = Jacobi(calY);
// |Y| = 1/2 in this chain's physical normalization: the 16 carries the
// lepton-doublet weights y = +/-1/2, so the SM-Higgs-pattern Y^2 value is
// exactly 1/4. The pre-2026-07-01 heuristic here ("smallest Y^2 above 0.05")
// selected the |Y| = 1/3 family value 1/9 instead, undercounting the
// SM-Higgs-pattern block; see the 2026-07-01 journal defect entry.
double yHalfValue = calEvY.OrderBy(v => Math.Abs(v - 0.25)).First();
bool yHalfCalibrationExact = Math.Abs(yHalfValue - 0.25) <= 1e-9;

int internalSmHiggsPatternRealDimension = smBlocks
    .Where(b => Math.Abs(b.ColorCasimir) <= 1e-6 &&
        Math.Abs(b.Su2LCasimir - jHalfValue) <= 1e-6 &&
        Math.Abs(b.YSquared - yHalfValue) <= 1e-6)
    .Sum(b => b.Dimension);
int internalSmHiggsPatternComplexDimension = internalSmHiggsPatternRealDimension / 2;

bool smBlockDimensionAccounting = smBlocks.Sum(b => b.Dimension) == vectorSpinor144KernelRealDimension;
bool weldedDimensionAccounting = welded144Content.Sum(t => (int)Math.Round((2 * t.J1 + 1) * (2 * t.J2 + 1)) * t.Multiplicity) == vectorSpinor144KernelRealDimension;
bool zCarrierDimensionAccounting =
    zCarrierLeftContent.Sum(t => (int)Math.Round((2 * t.J1 + 1) * (2 * t.J2 + 1)) * t.Multiplicity) == vectorSpinor144KernelRealDimension * 2 &&
    zCarrierRightContent.Sum(t => (int)Math.Round((2 * t.J1 + 1) * (2 * t.J2 + 1)) * t.Multiplicity) == vectorSpinor144KernelRealDimension * 2;

const bool targetBlindConstruction = true;
const bool physicalTargetsConsultedForConstruction = false;
const bool sourceDefinesVectorSpinor144BosonicProjectionMap = false;
const bool sourceDefinesVectorSpinor144CarrierAction = false;
const bool sourceDefinesVectorSpinor144VevSelection = false;
const bool sourceDefinesVectorSpinor144ObservedProjectionRows = false;
const bool sourceDefinesVectorSpinor144WeakAngleScalePoleOrGevLineage = false;
const int sourceDefinedEvenCompositeOrBosonicProjectionMapCount = 0;
const int testedSourceDefinedEvenCompositeOrBosonicProjectionMapCount = 0;
bool vectorSpinor144StillRequiresBosonicProjectionMap =
    !sourceDefinesVectorSpinor144BosonicProjectionMap &&
    sourceDefinedEvenCompositeOrBosonicProjectionMapCount == 0;

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
    "gamma-trace kernel 10x16+->16-; SM and welded generator restriction; source-defined map count only")))).ToLowerInvariant();

bool vectorSpinor144DecompositionProbePassed =
    phase411PrecursorPassed &&
    phase416PrecursorPassed &&
    chiralHalvesAreSixteenDimensional &&
    vectorSpinor144DimensionCheck &&
    vectorSpinor144InvariantUnderProbedGenerators &&
    yHalfCalibrationExact &&
    colorAlgebraDimensionIsEight &&
    welded144ContentRecovered &&
    ok2L &&
    ok2R &&
    weylHalvesAreTwoDimensional &&
    sigma4HomResidual <= 1e-10 &&
    smBlockDimensionAccounting &&
    weldedDimensionAccounting &&
    zCarrierDimensionAccounting &&
    targetBlindConstruction &&
    !physicalTargetsConsultedForConstruction &&
    !sourceDefinesVectorSpinor144BosonicProjectionMap &&
    !sourceDefinesVectorSpinor144CarrierAction &&
    !sourceDefinesVectorSpinor144VevSelection &&
    !sourceDefinesVectorSpinor144ObservedProjectionRows &&
    !sourceDefinesVectorSpinor144WeakAngleScalePoleOrGevLineage &&
    vectorSpinor144StillRequiresBosonicProjectionMap &&
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

string terminalStatus = vectorSpinor144DecompositionProbePassed
    ? (vectorSpinor144LinearCarrierHasNoWeldedScalar
        ? "vector-spinor-144-decomposed-linear-carrier-no-welded-scalar-map-still-missing"
        : "vector-spinor-144-decomposed-linear-welded-scalar-candidate-needs-source-map")
    : "vector-spinor-144-decomposition-probe-blocked";

string decision = vectorSpinor144DecompositionProbePassed
    ? (vectorSpinor144LinearCarrierHasNoWeldedScalar
        ? "The source-pinned vector-spinor 144 has been materialized as the exact gamma-trace kernel in 10 x 16 = 16 + 144 and decomposed under the same SM and welded-spin actions used by the prior phases. The actual chiral 2 x 144 Z carrier contains no linear welded spin-zero component, so it cannot by itself supply a linear scalar VEV carrier. The internal 144 does contain the recorded SM block content, including any color-singlet j_L=1/2 |Y|=1/2 dimensions listed in the artifact, but that is not enough: the source still does not define a bosonic projection, even-composite map, carrier action, VEV selection, observed photon/W/Z/H rows, weak-angle lineage, pole extraction, or GeV normalization. No Phase201 or Phase256 field is filled."
        : "The vector-spinor 144 decomposition found a linear welded-scalar representation slot in the chiral carrier. This is candidate-only representation data: without a source-defined bosonic projection/action/observed-field map, it cannot fill Phase201 or Phase256. The next phase would have to characterize the slot and source lineage before any promotion.")
    : "Do not use the vector-spinor 144 decomposition until the precursor and dimension/invariance batteries pass.";

var result = new
{
    phaseId = "phase417-vector-spinor-144-decomposition-probe",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    vectorSpinor144DecompositionProbePassed,
    phase411PrecursorPassed,
    phase416PrecursorPassed,
    applicationSubjectKind = ApplicationSubjectKind,
    targetBlindConstruction,
    physicalTargetsConsultedForConstruction,
    targetBlindConstructionHash,
    chiralHalvesAreSixteenDimensional,
    gammaTraceRankReal,
    gammaTraceRankComplex,
    vectorSpinor144KernelRealDimension,
    vectorSpinor144KernelComplexDimension,
    vectorSpinor144DimensionCheck,
    gammaTraceKernelMaxResidual,
    kernelInvarianceMaxResidual,
    vectorSpinor144InvariantUnderProbedGenerators,
    colorAlgebraDimensionIsEight,
    welded144ContentRecovered,
    weldedDimensionAccounting,
    smBlockDimensionAccounting,
    zCarrierDimensionAccounting,
    weylHalvesAreTwoDimensional,
    sigma4HomResidual,
    weylLabel2L = new { j1 = label2L.Item1, j2 = label2L.Item2 },
    weylLabel2R = new { j1 = label2R.Item1, j2 = label2R.Item2 },
    vectorSpinor144WeldedContent = welded144Content.Select(t => new { j1 = t.J1, j2 = t.J2, multiplicity = t.Multiplicity }).ToArray(),
    vectorSpinor144SmBlocks = smBlocks.Select(b => new
    {
        dimension = b.Dimension,
        colorCasimir = b.ColorCasimir,
        su2LCasimir = b.Su2LCasimir,
        ySquared = b.YSquared,
        jL = CasimirToJ(b.Su2LCasimir, jHalfValue / 0.75),
        absYRelativeToHalf = yHalfValue > 0 ? Math.Sqrt(Math.Max(b.YSquared, 0.0) / yHalfValue) * 0.5 : 0.0,
    }).ToArray(),
    yHalfValueSquared = yHalfValue,
    yHalfCalibrationExact,
    internalSmHiggsPatternRealDimension,
    internalSmHiggsPatternComplexDimension,
    zCarrierLeftWeldedContent = zCarrierLeftContent.Select(t => new { j1 = t.J1, j2 = t.J2, multiplicity = t.Multiplicity }).ToArray(),
    zCarrierRightWeldedContent = zCarrierRightContent.Select(t => new { j1 = t.J1, j2 = t.J2, multiplicity = t.Multiplicity }).ToArray(),
    linearWeldedScalarCountLeft,
    linearWeldedScalarCountRight,
    linearWeldedScalarCountTotal,
    vectorSpinor144LinearCarrierHasNoWeldedScalar,
    sourceDefinesVectorSpinor144BosonicProjectionMap,
    sourceDefinesVectorSpinor144CarrierAction,
    sourceDefinesVectorSpinor144VevSelection,
    sourceDefinesVectorSpinor144ObservedProjectionRows,
    sourceDefinesVectorSpinor144WeakAngleScalePoleOrGevLineage,
    sourceDefinedEvenCompositeOrBosonicProjectionMapCount,
    testedSourceDefinedEvenCompositeOrBosonicProjectionMapCount,
    vectorSpinor144StillRequiresBosonicProjectionMap,
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
        phase411SummaryPath = Phase411SummaryPath,
        phase416SummaryPath = Phase416SummaryPath,
        primaryDraft = "docs/Reference/ExperimentReferences/texts/GU-DRAFT-2021-TEXT.txt (section 11.2 eq. 11.6; section 12.22; 10 x 16 = 16 + 144 vector-spinor remainder)",
    },
    explicitCandidateOnlyNonclaims = new[]
    {
        "The vector-spinor 144 is a source-pinned fermionic carrier, not a source-defined bosonic scalar VEV carrier.",
        "This phase decomposes the gamma-trace kernel and chiral 2 x 144 carrier; it does not invent arbitrary non-source-defined composite maps.",
        "Internal SM doublet-pattern content, if present, is representation data only and is not an observed Higgs projection row.",
        "No carrier action, VEV selection, weak-angle lineage, pole extraction, or GeV normalization is supplied.",
        "No Phase201 or Phase256 fill.",
    },
    decision,
};

var options = JsonOptions();
File.WriteAllText(Path.Combine(outputDir, "vector_spinor_144_decomposition_probe.json"), JsonSerializer.Serialize(result, options));
string summaryPath = Path.Combine(outputDir, "vector_spinor_144_decomposition_probe_summary.json");
File.WriteAllText(summaryPath, JsonSerializer.Serialize(result, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"vectorSpinor144DecompositionProbePassed={vectorSpinor144DecompositionProbePassed}");
Console.WriteLine($"gammaTraceRankComplex={gammaTraceRankComplex} vectorSpinor144KernelComplexDimension={vectorSpinor144KernelComplexDimension}");
Console.WriteLine($"vectorSpinor144DimensionCheck={vectorSpinor144DimensionCheck} gammaTraceKernelMaxResidual={gammaTraceKernelMaxResidual:E3}");
Console.WriteLine($"vectorSpinor144InvariantUnderProbedGenerators={vectorSpinor144InvariantUnderProbedGenerators} kernelInvarianceMaxResidual={kernelInvarianceMaxResidual:E3}");
Console.WriteLine($"welded144Content=[{string.Join(",", welded144Content.Select(t => $"({t.J1},{t.J2})x{t.Multiplicity}"))}]");
Console.WriteLine($"yHalfCalibrationExact={yHalfCalibrationExact} yHalfValueSquared={yHalfValue}");
Console.WriteLine($"internalSmHiggsPatternComplexDimension={internalSmHiggsPatternComplexDimension}");
Console.WriteLine($"linearWeldedScalarCountTotal={linearWeldedScalarCountTotal}");
Console.WriteLine($"sourceDefinedEvenCompositeOrBosonicProjectionMapCount={sourceDefinedEvenCompositeOrBosonicProjectionMapCount}");
Console.WriteLine($"vectorSpinor144StillRequiresBosonicProjectionMap={vectorSpinor144StillRequiresBosonicProjectionMap}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");
Console.WriteLine($"summaryPath={summaryPath}");

// ---------------------------------------------------------------------------
// Helpers.
// ---------------------------------------------------------------------------

static bool IsZero(double x) => Math.Abs(x) <= 1e-9;

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

static double[,] RealifyRectangular(Complex[,] m)
{
    int rows = m.GetLength(0), cols = m.GetLength(1);
    var result = new double[2 * rows, 2 * cols];
    for (int r = 0; r < rows; r++)
        for (int c = 0; c < cols; c++)
        {
            result[r, c] = m[r, c].Real;
            result[r, c + cols] = -m[r, c].Imaginary;
            result[r + rows, c] = m[r, c].Imaginary;
            result[r + rows, c + cols] = m[r, c].Real;
        }
    return result;
}

static double[,] Gram(double[,] map)
{
    int rows = map.GetLength(0), cols = map.GetLength(1);
    var result = new double[cols, cols];
    for (int a = 0; a < cols; a++)
        for (int b = a; b < cols; b++)
        {
            double sum = 0.0;
            for (int r = 0; r < rows; r++)
                sum += map[r, a] * map[r, b];
            result[a, b] = result[b, a] = sum;
        }
    return result;
}

static double MaxMapResidual(double[,] map, IEnumerable<double[]> basis)
{
    double max = 0.0;
    int rows = map.GetLength(0), cols = map.GetLength(1);
    foreach (var v in basis)
        for (int r = 0; r < rows; r++)
        {
            double sum = 0.0;
            for (int c = 0; c < cols; c++)
                sum += map[r, c] * v[c];
            max = Math.Max(max, Math.Abs(sum));
        }
    return max;
}

static List<double[]> ApplyToBasis(double[,] op, List<double[]> basis)
{
    int n = op.GetLength(0);
    var result = new List<double[]>(basis.Count);
    foreach (var v in basis)
    {
        var image = new double[n];
        for (int r = 0; r < n; r++)
        {
            double sum = 0.0;
            for (int c = 0; c < n; c++)
                sum += op[r, c] * v[c];
            image[r] = sum;
        }
        result.Add(image);
    }
    return result;
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

static double[,] CasimirRealComplex(IEnumerable<Complex[,]> generators)
{
    return CasimirRealMatrices(generators.Select(Realify).ToArray());
}

static double[,] CasimirRealMatrices(IEnumerable<double[,]> generators)
{
    double[,]? casimir = null;
    foreach (var g in generators)
    {
        int n = g.GetLength(0);
        casimir ??= new double[n, n];
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
            {
                double sum = 0.0;
                for (int k = 0; k < n; k++)
                    sum += g[i, k] * g[k, j];
                casimir[i, j] -= sum;
            }
    }
    return casimir!;
}

static (double[][] A, double[][] B) BuildSu2Coefficients(List<(int I, int J)> so4Pairs)
{
    int PairIndex(int i, int j) => so4Pairs.FindIndex(p => p.I == Math.Min(i, j) && p.J == Math.Max(i, j));
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
    var aCoefficients = new double[3][];
    var bCoefficients = new double[3][];
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
    return (aCoefficients, bCoefficients);
}

static Complex[,] CombineComplex(Complex[][,] generators, double[] coefficients)
{
    int n = generators[0].GetLength(0);
    var result = new Complex[n, n];
    for (int g = 0; g < generators.Length; g++)
        if (Math.Abs(coefficients[g]) > 1e-15)
            for (int r = 0; r < n; r++)
                for (int c = 0; c < n; c++)
                    result[r, c] += coefficients[g] * generators[g][r, c];
    return result;
}

static double[,] CombineReal(double[][,] generators, double[] coefficients)
{
    int n = generators[0].GetLength(0);
    var result = new double[n, n];
    for (int g = 0; g < generators.Length; g++)
        if (Math.Abs(coefficients[g]) > 1e-15)
            for (int r = 0; r < n; r++)
                for (int c = 0; c < n; c++)
                    result[r, c] += coefficients[g] * generators[g][r, c];
    return result;
}

static List<WeldBlock> WeldedContent(double[][,] generators, double kappa, double[][] aCoefficients, double[][] bCoefficients, out bool ok)
{
    var aGens = aCoefficients.Select(c => CombineReal(generators, c)).ToArray();
    var bGens = bCoefficients.Select(c => CombineReal(generators, c)).ToArray();
    var cA = CasimirRealMatrices(aGens);
    var cB = CasimirRealMatrices(bGens);
    int n = cA.GetLength(0);
    var (evA, vecA) = Jacobi(cA);
    ok = true;
    var content = new List<WeldBlock>();
    var groups = new Dictionary<long, List<int>>();
    for (int e = 0; e < n; e++)
    {
        long key = (long)Math.Round(evA[e] / kappa * 4.0);
        if (!groups.TryGetValue(key, out var list))
            groups[key] = list = new List<int>();
        list.Add(e);
    }
    foreach (var (aKey, indices) in groups.OrderBy(kv => kv.Key))
    {
        double j1 = (-1.0 + Math.Sqrt(Math.Max(0.0, 1.0 + aKey))) / 2.0;
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
            long key = (long)Math.Round(ev / kappa * 4.0);
            bGroups[key] = bGroups.GetValueOrDefault(key) + 1;
        }
        foreach (var (bKey, count) in bGroups.OrderBy(kv => kv.Key))
        {
            double j2 = (-1.0 + Math.Sqrt(Math.Max(0.0, 1.0 + bKey))) / 2.0;
            int irrepRealDim = (int)Math.Round((2 * j1 + 1) * (2 * j2 + 1));
            if (irrepRealDim <= 0 || count % irrepRealDim != 0)
            {
                ok = false;
                continue;
            }
            content.Add(new WeldBlock(j1, j2, count / irrepRealDim));
        }
    }
    return content;
}

static List<WeldBlock> TensorContent((double J1, double J2) factor, List<WeldBlock> baseContent)
{
    var dict = new Dictionary<(double, double), int>();
    foreach (var (j1, j2, m) in baseContent)
        for (double x = Math.Abs(factor.J1 - j1); x <= factor.J1 + j1 + 1e-9; x += 1.0)
            for (double y = Math.Abs(factor.J2 - j2); y <= factor.J2 + j2 + 1e-9; y += 1.0)
            {
                var key = (Math.Round(x * 2) / 2, Math.Round(y * 2) / 2);
                dict[key] = dict.GetValueOrDefault(key) + m;
            }
    return dict
        .Select(kv => new WeldBlock(kv.Key.Item1, kv.Key.Item2, kv.Value))
        .OrderBy(t => t.J1)
        .ThenBy(t => t.J2)
        .ToList();
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

static (Complex[][,] Left, Complex[][,] Right, bool WeylHalvesAreTwoDimensional, double HomResidual) BuildSpacetimeWeylGenerators(
    Complex[][,] gamma4,
    List<(int I, int J)> so4Pairs)
{
    double[,] M4Local(int i, int j)
    {
        var m = new double[4, 4];
        m[i, j] = 1.0;
        m[j, i] = -1.0;
        return m;
    }

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

    var product = gamma4[0];
    for (int i = 1; i < 4; i++)
        product = CMatMul(product, gamma4[i]);
    var square = CMatMul(product, product);
    Complex phase = Complex.Sqrt(1.0 / square[0, 0]);
    var chirality4 = new Complex[4, 4];
    for (int r = 0; r < 4; r++)
        for (int c = 0; c < 4; c++)
            chirality4[r, c] = phase * product[r, c];
    double offDiagonal = 0.0;
    for (int r = 0; r < 4; r++)
        for (int c = 0; c < 4; c++)
            if (r != c)
                offDiagonal = Math.Max(offDiagonal, chirality4[r, c].Magnitude);
    var axesL = new List<int>();
    var axesR = new List<int>();
    for (int r = 0; r < 4; r++)
    {
        if ((chirality4[r, r] - Complex.One).Magnitude < 1e-10)
            axesL.Add(r);
        else
            axesR.Add(r);
    }

    double homResidual = 0.0;
    for (int x = 0; x < 6; x++)
        for (int y = x + 1; y < 6; y++)
        {
            var mx = M4Local(so4Pairs[x].I, so4Pairs[x].J);
            var my = M4Local(so4Pairs[y].I, so4Pairs[y].J);
            var lhs = Sigma4(MatComm(mx, my));
            var sx = Sigma4(mx);
            var sy = Sigma4(my);
            var rhs = CMatMul(sx, sy);
            var rhs2 = CMatMul(sy, sx);
            for (int r = 0; r < 4; r++)
                for (int c = 0; c < 4; c++)
                    homResidual = Math.Max(homResidual, (lhs[r, c] - (rhs[r, c] - rhs2[r, c])).Magnitude);
        }

    Complex[,] Restrict2(Complex[,] full, List<int> axes)
    {
        var result = new Complex[2, 2];
        for (int r = 0; r < 2; r++)
            for (int c = 0; c < 2; c++)
                result[r, c] = full[axes[r], axes[c]];
        return result;
    }

    var left = so4Pairs.Select(p => Restrict2(Sigma4(M4Local(p.I, p.J)), axesL)).ToArray();
    var right = so4Pairs.Select(p => Restrict2(Sigma4(M4Local(p.I, p.J)), axesR)).ToArray();
    return (left, right, offDiagonal < 1e-12 && axesL.Count == 2 && axesR.Count == 2, homResidual);
}

static List<SmBlock> SmBlockCensus(double[,] colorCasimir, double[,] su2LCasimir, double[,] ySquared)
{
    int n = colorCasimir.GetLength(0);
    var combined = new double[n, n];
    const double m1 = 0.318309886183790671;
    const double m2 = 0.159154943091895335;
    for (int i = 0; i < n; i++)
        for (int j = 0; j < n; j++)
            combined[i, j] = su2LCasimir[i, j] + m1 * ySquared[i, j] + m2 * colorCasimir[i, j];
    var (_, vectors) = Jacobi(combined);
    double Quad(double[,] op, int col)
    {
        double sum = 0.0;
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
                sum += vectors[i, col] * op[i, j] * vectors[j, col];
        return sum;
    }
    return Enumerable.Range(0, n)
        .Select(e => (
            C: Math.Round(Quad(colorCasimir, e), 7),
            L: Math.Round(Quad(su2LCasimir, e), 7),
            Y: Math.Round(Quad(ySquared, e), 7)))
        .GroupBy(e => e)
        .Select(g => new SmBlock(g.Count(), g.Key.C, g.Key.L, g.Key.Y))
        .OrderBy(b => b.ColorCasimir)
        .ThenBy(b => b.Su2LCasimir)
        .ThenBy(b => b.YSquared)
        .ToList();
}

static double CasimirToJ(double value, double kappa)
{
    if (kappa <= 0)
        return 0.0;
    return (-1.0 + Math.Sqrt(Math.Max(0.0, 1.0 + 4.0 * value / kappa))) / 2.0;
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

static bool? JsonBool(JsonElement element, string propertyName)
{
    return element.TryGetProperty(propertyName, out var property) && property.ValueKind is JsonValueKind.True or JsonValueKind.False
        ? property.GetBoolean()
        : null;
}

static JsonSerializerOptions JsonOptions() => new()
{
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
};

public sealed record WeldBlock(double J1, double J2, int Multiplicity);
public sealed record SmBlock(int Dimension, double ColorCasimir, double Su2LCasimir, double YSquared);
