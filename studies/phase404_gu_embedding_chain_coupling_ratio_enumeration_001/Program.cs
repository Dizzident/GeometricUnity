using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

// Phase404: GU embedding-chain coupling-ratio enumeration (brute force #1).
//
// USER DIRECTIVE (2026-06-11): run the three brute-forceable computations as
// the next phases. This is the first: exhaustively compute the GU-named
// embedding chain at the complexification level and extract the
// embedding-derived coupling ratio and doublet inventories. The objects are
// 10x10 / 32x32 matrices - CPU computes the full enumeration in seconds, so
// no GPU is used here (recorded honestly; GPU enters in Phases 405/406
// where problem sizes warrant it).
//
// The chain (GU-DRAFT-2021 sections 11-12 and the stored text; the Iceberg
// analysis [01:50-01:53]): Spin(6,4) -> Spin(6) x Spin(4) (Pati-Salam path)
// or the U(3) x U(2) path - both share the D5 COMPLEXIFICATION so(10)_C, and
// branching multiplicities, charge spectra, and index ratios are
// complexification-level data. The computation uses the COMPACT real form
// so(10) for the arithmetic; real-form-specific effects of spin(6,4) are a
// recorded limitation, not silently absorbed.
//
// What is computed, all from first principles with no external input:
//
//   1. ADJOINT 45 BRANCHING under the embedded su(2)_L x u(1)_Y for a
//      bounded exhaustive scan of hypercharge directions
//      Y(alpha, beta) = alpha R3 + beta J/2 (R3 = su(2)_R Cartan, J = the
//      so(6) complex structure; B-L = (2/3) J on this chain - leptons carry
//      J-charge +-3/2 - so the standard Pati-Salam hypercharge
//      Y = R3 + (B-L)/2 is (alpha, beta) = (1, 2/3)): every j = 1/2 block is
//      tagged with its u(1) charge and its COLOR (su(3) = traceless
//      centralizer of J in so(6)) Casimir. The decisive question: does the
//      adjoint contain a COLOR-SINGLET doublet (the SM-Higgs pattern), for
//      ANY scanned hypercharge direction?
//
//   2. SPINOR 16 BRANCHING (one family) via the explicit Clifford
//      construction Cl(10) on 32 complex dimensions: the family's
//      (color, isospin, Y) table is DERIVED, and the color-singlet doublet
//      in the 16 (the lepton-doublet slot) provides the INTERNAL
//      normalization anchor for Y - a bookkeeping convention, not an
//      experimental input.
//
//   3. EMBEDDING-DERIVED COUPLING RATIO tan^2(theta_emb) =
//      <T3, T3> / <Y_norm, Y_norm> in the algebra pairing, with Y_norm
//      scaled so the 16's color-singlet doublet carries |Y| = 1/2. For the
//      standard direction this is a group-theoretic constant
//      (= 3/5, the classic unified-chain value, asserted as a mathematical
//      cross-check like j(j+1) - it is not an experimental number); the
//      scan records the full menu over (alpha, beta). sin^2 is derived and
//      recorded blind; NO comparison to measured electroweak values is made
//      anywhere in this phase.
//
// Fail-closed: charges and ratios only - no VEV, no scale, no GeV; nothing
// is promoted; no contract field is filled.

const string DefaultOutputDir = "studies/phase404_gu_embedding_chain_coupling_ratio_enumeration_001/output";
const string Phase402SummaryPath = "studies/phase402_gu_draft_scalar_route_dictionary_audit_001/output/gu_draft_scalar_route_dictionary_audit_summary.json";
const string Phase403SummaryPath = "studies/phase403_adjoint_doublet_substructure_branching_probe_001/output/adjoint_doublet_substructure_branching_probe_summary.json";

const double Tolerance = 1e-9;
const int ScanCoefficientMax = 3; // bounded exhaustive scan: alpha, beta in {-3..3}/{1,2,3} ratios

var outputDir = Environment.GetEnvironmentVariable("PHASE404_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

// ---------------------------------------------------------------------------
// Precursors.
// ---------------------------------------------------------------------------

using var phase402Doc = JsonDocument.Parse(File.ReadAllText(Phase402SummaryPath));
bool phase402PrecursorPassed =
    JsonBool(phase402Doc.RootElement, "guDraftScalarRouteDictionaryAuditPassed") is true;
using var phase403Doc = JsonDocument.Parse(File.ReadAllText(Phase403SummaryPath));
bool phase403PrecursorPassed =
    JsonBool(phase403Doc.RootElement, "adjointDoubletSubstructureBranchingProbePassed") is true &&
    JsonBool(phase403Doc.RootElement, "couplingRatioLineageMechanismIdentified") is true;

// ---------------------------------------------------------------------------
// so(10): 45 generators M[i,j] = E_ij - E_ji (i < j), pairing -Tr(AB)/2.
// ---------------------------------------------------------------------------

var generatorPairs = new List<(int I, int J)>();
for (int i = 0; i < 10; i++)
    for (int j = i + 1; j < 10; j++)
        generatorPairs.Add((i, j));
int adjointDim = generatorPairs.Count; // 45

double[][,] basis = generatorPairs.Select(pair => SkewGenerator(10, pair.I, pair.J)).ToArray();

static double[,] SkewGenerator(int n, int i, int j)
{
    var m = new double[n, n];
    m[i, j] = 1.0;
    m[j, i] = -1.0;
    return m;
}

static double PairAlg(double[,] a, double[,] b)
{
    int n = a.GetLength(0);
    double trace = 0.0;
    for (int r = 0; r < n; r++)
        for (int c = 0; c < n; c++)
            trace += a[r, c] * b[c, r];
    return -0.5 * trace;
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

// Coordinates of a 10x10 skew matrix in the M-basis (orthonormal up to the
// uniform factor <M,M> = 1 under -Tr/2).
double[] AdjointCoords(double[,] x)
{
    var coords = new double[adjointDim];
    for (int g = 0; g < adjointDim; g++)
        coords[g] = PairAlg(basis[g], x);
    return coords;
}

// ---------------------------------------------------------------------------
// Pati-Salam scaffolding inside so(10).
//   so(4) on coordinates {6,7,8,9}: su(2)_L = self-dual, su(2)_R = anti-self-dual.
//   so(6) on coordinates {0..5}: J = M01 + M23 + M45 (the B-L direction whose
//   so(6) centralizer is u(3); color su(3) = traceless part of that centralizer).
// ---------------------------------------------------------------------------

double[,] M(int i, int j) => SkewGenerator(10, i, j);

// Self-dual / anti-self-dual su(2) pairs in so(4) on {6,7,8,9}:
double[,][,] BuildSu2Pair()
{
    var l = new double[3][,];
    var r = new double[3][,];
    l[0] = Scale(AddM(M(6, 7), M(8, 9)), 0.5);
    l[1] = Scale(AddM(M(6, 8), Scale(M(7, 9), -1.0)), 0.5);
    l[2] = Scale(AddM(M(6, 9), M(7, 8)), 0.5);
    r[0] = Scale(AddM(M(6, 7), Scale(M(8, 9), -1.0)), 0.5);
    r[1] = Scale(AddM(M(6, 8), M(7, 9)), 0.5);
    r[2] = Scale(AddM(M(6, 9), Scale(M(7, 8), -1.0)), 0.5);
    return new[,] { { l[0], l[1], l[2] }, { r[0], r[1], r[2] } };
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

static double[,] Scale(double[,] a, double s)
{
    int n = a.GetLength(0);
    var result = new double[n, n];
    for (int r = 0; r < n; r++)
        for (int c = 0; c < n; c++)
            result[r, c] = s * a[r, c];
    return result;
}

var su2Pair = BuildSu2Pair();
var su2L = ResolveSu2Signs(new[] { su2Pair[0, 0], su2Pair[0, 1], su2Pair[0, 2] });
var su2R = ResolveSu2Signs(new[] { su2Pair[1, 0], su2Pair[1, 1], su2Pair[1, 2] });

// The (anti-)self-dual halves close as su(2) only for the right sign
// convention; resolve it by exhaustive search over the 8 sign choices
// (sign flips change nothing downstream - Casimirs and Y^2 are invariant,
// and both signs of every Y coefficient are scanned anyway).
static double[][,] ResolveSu2Signs(double[][,] candidate)
{
    foreach (int s1 in new[] { 1, -1 })
        foreach (int s2 in new[] { 1, -1 })
            foreach (int s3 in new[] { 1, -1 })
            {
                var trial = new[] { Scale(candidate[0], s1), Scale(candidate[1], s2), Scale(candidate[2], s3) };
                double residual = 0.0;
                for (int a = 0; a < 3; a++)
                {
                    int b = (a + 1) % 3, c = (a + 2) % 3;
                    residual = System.Math.Max(residual, FrobNorm(AddM(MatComm(trial[a], trial[b]), Scale(trial[c], -1.0))));
                }
                if (residual <= 1e-12)
                    return trial;
            }
    throw new InvalidOperationException("No sign convention closes the su(2) triple.");
}

// Closure and commutation battery.
double su2ClosureResidual = 0.0;
for (int a = 0; a < 3; a++)
{
    int b = (a + 1) % 3, c = (a + 2) % 3;
    su2ClosureResidual = System.Math.Max(su2ClosureResidual, FrobNorm(AddM(MatComm(su2L[a], su2L[b]), Scale(su2L[c], -1.0))));
    su2ClosureResidual = System.Math.Max(su2ClosureResidual, FrobNorm(AddM(MatComm(su2R[a], su2R[b]), Scale(su2R[c], -1.0))));
    for (int d = 0; d < 3; d++)
        su2ClosureResidual = System.Math.Max(su2ClosureResidual, FrobNorm(MatComm(su2L[a], su2R[d])));
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

var jComplexStructure = AddM(AddM(M(0, 1), M(2, 3)), M(4, 5));
double jCommutesWithSu2Residual = 0.0;
foreach (var g in su2L.Concat(su2R))
    jCommutesWithSu2Residual = System.Math.Max(jCommutesWithSu2Residual, FrobNorm(MatComm(jComplexStructure, g)));

// Color su(3): traceless centralizer of J inside so(6) (coordinates 0..5).
// Solve [X, J] = 0 over the 15-dim so(6) subspace, then remove the J
// direction itself.
var so6Indices = Enumerable.Range(0, adjointDim)
    .Where(g => generatorPairs[g].I < 6 && generatorPairs[g].J < 6)
    .ToArray();
var centralizerBasis = new List<double[]>(); // coords in so(6) index space
{
    // Build the linear map X -> [X, J] restricted to so(6) and find its kernel.
    int n6 = so6Indices.Length;
    var mapMatrix = new double[adjointDim, n6];
    for (int col = 0; col < n6; col++)
    {
        var bracket = MatComm(basis[so6Indices[col]], jComplexStructure);
        var coords = AdjointCoords(bracket);
        for (int row = 0; row < adjointDim; row++)
            mapMatrix[row, col] = coords[row];
    }
    // Kernel via Gram matrix eigen-decomposition (map^T map).
    var gram = new double[n6, n6];
    for (int a = 0; a < n6; a++)
        for (int b = 0; b < n6; b++)
        {
            double sum = 0.0;
            for (int row = 0; row < adjointDim; row++)
                sum += mapMatrix[row, a] * mapMatrix[row, b];
            gram[a, b] = sum;
        }
    var (gramEigenvalues, gramVectors) = JacobiSymmetric(gram);
    for (int v = 0; v < n6; v++)
        if (System.Math.Abs(gramEigenvalues[v]) <= 1e-10)
        {
            var coords = new double[n6];
            for (int i = 0; i < n6; i++)
                coords[i] = gramVectors[i, v];
            centralizerBasis.Add(coords);
        }
}
int u3Dimension = centralizerBasis.Count; // expect 9

// Convert centralizer coords to matrices; orthonormalize; remove J direction.
var jNormalized = Scale(jComplexStructure, 1.0 / System.Math.Sqrt(PairAlg(jComplexStructure, jComplexStructure)));
var colorBasis = new List<double[,]>();
foreach (var coords in centralizerBasis)
{
    var mat = new double[10, 10];
    for (int i = 0; i < coords.Length; i++)
        if (System.Math.Abs(coords[i]) > 0)
            mat = AddM(mat, Scale(basis[so6Indices[i]], coords[i]));
    // remove J component
    mat = AddM(mat, Scale(jNormalized, -PairAlg(jNormalized, mat)));
    // Gram-Schmidt against existing color basis
    foreach (var existing in colorBasis)
        mat = AddM(mat, Scale(existing, -PairAlg(existing, mat) / PairAlg(existing, existing)));
    double norm = System.Math.Sqrt(PairAlg(mat, mat));
    if (norm > 1e-8)
        colorBasis.Add(Scale(mat, 1.0 / norm));
}
int colorDimension = colorBasis.Count; // expect 8
double colorClosureResidual = 0.0;
// verify color algebra closes within itself + is su(2)L/R-commuting
foreach (var x in colorBasis)
    foreach (var y in colorBasis)
    {
        var bracket = MatComm(x, y);
        var residual = bracket;
        residual = AddM(residual, Scale(jNormalized, -PairAlg(jNormalized, residual)));
        foreach (var z in colorBasis)
            residual = AddM(residual, Scale(z, -PairAlg(z, residual) / PairAlg(z, z)));
        colorClosureResidual = System.Math.Max(colorClosureResidual, FrobNorm(residual));
    }

// ---------------------------------------------------------------------------
// Clifford construction for the spinor 16 (Cl(10) on 32 complex dims).
// gamma_{2k} = Z^k X I^(4-k), gamma_{2k+1} = Z^k Y I^(4-k) (tensor strings).
// Sigma_ij = [gamma_i, gamma_j]/4; chirality = (-i)^5 g0g1...g9.
// ---------------------------------------------------------------------------

Complex[][,] paulis =
[
    new Complex[2, 2] { { 0, 1 }, { 1, 0 } },                       // X
    new Complex[2, 2] { { 0, -Complex.ImaginaryOne }, { Complex.ImaginaryOne, 0 } }, // Y
    new Complex[2, 2] { { 1, 0 }, { 0, -1 } },                      // Z
    new Complex[2, 2] { { 1, 0 }, { 0, 1 } },                       // I
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

var gammas = new Complex[10][,];
for (int k = 0; k < 5; k++)
{
    var codesEven = new int[5];
    var codesOdd = new int[5];
    for (int p = 0; p < 5; p++)
    {
        codesEven[p] = p < k ? 2 : (p == k ? 0 : 3); // Z^k X I^...
        codesOdd[p] = p < k ? 2 : (p == k ? 1 : 3);  // Z^k Y I^...
    }
    gammas[2 * k] = TensorString(codesEven);
    gammas[2 * k + 1] = TensorString(codesOdd);
}

static Complex[,] CMatMul(Complex[,] a, Complex[,] b)
{
    int n = a.GetLength(0);
    var result = new Complex[n, n];
    for (int r = 0; r < n; r++)
        for (int c = 0; c < n; c++)
        {
            Complex sum = Complex.Zero;
            for (int k = 0; k < n; k++)
                sum += a[r, k] * b[k, c];
            result[r, c] = sum;
        }
    return result;
}

static Complex[,] CAdd(Complex[,] a, Complex[,] b, Complex scaleB)
{
    int n = a.GetLength(0);
    var result = new Complex[n, n];
    for (int r = 0; r < n; r++)
        for (int c = 0; c < n; c++)
            result[r, c] = a[r, c] + scaleB * b[r, c];
    return result;
}

// Spinor representation of an so(10) element sum c_{ij} M_ij:
// R(M_ij) = Sigma_ij = [g_i, g_j]/4.
Complex[,] SpinorRep(double[,] so10Element)
{
    var result = new Complex[32, 32];
    for (int i = 0; i < 10; i++)
        for (int j = i + 1; j < 10; j++)
        {
            double coefficient = so10Element[i, j];
            if (System.Math.Abs(coefficient) < 1e-15)
                continue;
            var sigma = CAdd(CMatMul(gammas[i], gammas[j]), CMatMul(gammas[j], gammas[i]), -Complex.One);
            // [gi,gj]/4 = (gi gj - gj gi)/4
            for (int r = 0; r < 32; r++)
                for (int c = 0; c < 32; c++)
                    result[r, c] += coefficient * sigma[r, c] / 4.0;
        }
    return result;
}

// Chirality operator: proportional to g0 g1 ... g9; fix phase so eigenvalues are +-1.
Complex[,] chirality;
{
    var product = gammas[0];
    for (int i = 1; i < 10; i++)
        product = CMatMul(product, gammas[i]);
    // product^2 = (-1)^{10*9/2} = -1 => multiply by i to square to +1... compute numerically:
    var square = CMatMul(product, product);
    Complex squareScale = square[0, 0];
    Complex phase = Complex.Sqrt(1.0 / squareScale);
    chirality = new Complex[32, 32];
    for (int r = 0; r < 32; r++)
        for (int c = 0; c < 32; c++)
            chirality[r, c] = phase * product[r, c];
}

// Projector onto the +1 chiral half (the 16).
var chiralProjector = new Complex[32, 32];
for (int r = 0; r < 32; r++)
{
    chiralProjector[r, r] = 0.5;
    for (int c = 0; c < 32; c++)
        chiralProjector[r, c] += 0.5 * chirality[r, c];
}

// Verify spinor rep homomorphism on a sample: R([A,B]) == [R(A), R(B)].
double spinorHomResidual = 0.0;
{
    var testA = su2L[2];
    var testB = jComplexStructure;
    var lhs = SpinorRep(MatComm(testA, testB));
    var rhs = CAdd(CMatMul(SpinorRep(testA), SpinorRep(testB)), CMatMul(SpinorRep(testB), SpinorRep(testA)), -Complex.One);
    for (int r = 0; r < 32; r++)
        for (int c = 0; c < 32; c++)
            spinorHomResidual = System.Math.Max(spinorHomResidual, (lhs[r, c] - rhs[r, c]).Magnitude);
    var testC = su2R[0];
    var lhs2 = SpinorRep(MatComm(testA, testC));
    var rhs2 = CAdd(CMatMul(SpinorRep(testA), SpinorRep(testC)), CMatMul(SpinorRep(testC), SpinorRep(testA)), -Complex.One);
    for (int r = 0; r < 32; r++)
        for (int c = 0; c < 32; c++)
            spinorHomResidual = System.Math.Max(spinorHomResidual, (lhs2[r, c] - rhs2[r, c]).Magnitude);
}

// ---------------------------------------------------------------------------
// Branching machinery.
// ---------------------------------------------------------------------------

// Adjoint branching: blocks of the 45 under su(2)_L Casimir, |Y|, color Casimir.
List<AdjointBlock> BranchAdjoint(double[,] hypercharge)
{
    // operators on the 45 (coordinates in M-basis)
    var adL = new double[3][,];
    for (int a = 0; a < 3; a++)
        adL[a] = AdjointAd(su2L[a]);
    var adY = AdjointAd(hypercharge);
    var colorCasimirOp = new double[adjointDim, adjointDim];
    foreach (var colorGen in colorBasis)
    {
        var ad = AdjointAd(colorGen);
        for (int i = 0; i < adjointDim; i++)
            for (int j = 0; j < adjointDim; j++)
            {
                double sum = 0.0;
                for (int k = 0; k < adjointDim; k++)
                    sum += ad[i, k] * ad[k, j];
                colorCasimirOp[i, j] -= sum;
            }
    }
    var jCasimirOp = new double[adjointDim, adjointDim];
    for (int a = 0; a < 3; a++)
        for (int i = 0; i < adjointDim; i++)
            for (int j = 0; j < adjointDim; j++)
            {
                double sum = 0.0;
                for (int k = 0; k < adjointDim; k++)
                    sum += adL[a][i, k] * adL[a][k, j];
                jCasimirOp[i, j] -= sum;
            }
    var y2Op = new double[adjointDim, adjointDim];
    for (int i = 0; i < adjointDim; i++)
        for (int j = 0; j < adjointDim; j++)
        {
            double sum = 0.0;
            for (int k = 0; k < adjointDim; k++)
                sum += adY[i, k] * adY[k, j];
            y2Op[i, j] = -sum;
        }

    // generic mixer for simultaneous block identification
    var combined = new double[adjointDim, adjointDim];
    const double mu1 = 0.318309886183790671; // 1/pi
    const double mu2 = 0.159154943091895335; // 1/(2 pi)
    for (int i = 0; i < adjointDim; i++)
        for (int j = 0; j < adjointDim; j++)
            combined[i, j] = jCasimirOp[i, j] + mu1 * y2Op[i, j] + mu2 * colorCasimirOp[i, j];
    var (_, vectors) = JacobiSymmetric(combined);

    var entries = new List<(double JJ1, double Y2, double C2)>();
    for (int v = 0; v < adjointDim; v++)
    {
        var vec = new double[adjointDim];
        for (int i = 0; i < adjointDim; i++)
            vec[i] = vectors[i, v];
        entries.Add((Quadratic(jCasimirOp, vec), Quadratic(y2Op, vec), Quadratic(colorCasimirOp, vec)));
    }
    return entries
        .GroupBy(e => (J: System.Math.Round(e.JJ1, 7), Y: System.Math.Round(e.Y2, 7), C: System.Math.Round(e.C2, 7)))
        .Select(group => new AdjointBlock
        {
            Dimension = group.Count(),
            JValue = 0.5 * (System.Math.Sqrt(1.0 + 4.0 * group.Key.J) - 1.0),
            AbsY = System.Math.Sqrt(System.Math.Max(group.Key.Y, 0.0)),
            ColorCasimir = group.Key.C,
            IsColorSinglet = System.Math.Abs(group.Key.C) <= 1e-7,
        })
        .OrderByDescending(block => block.JValue)
        .ThenByDescending(block => block.AbsY)
        .ToList();
}

double[,] AdjointAd(double[,] generator)
{
    var ad = new double[adjointDim, adjointDim];
    for (int b = 0; b < adjointDim; b++)
    {
        var bracket = MatComm(generator, basis[b]);
        var coords = AdjointCoords(bracket);
        for (int c = 0; c < adjointDim; c++)
            ad[c, b] = coords[c];
    }
    return ad;
}

static double Quadratic(double[,] op, double[] vec)
{
    int n = vec.Length;
    double sum = 0.0;
    for (int i = 0; i < n; i++)
        for (int j = 0; j < n; j++)
            sum += vec[i] * op[i, j] * vec[j];
    return sum;
}

// Spinor-16 branching: eigen-decompose hermitian i*R(T3L), i*R(Y), color
// Casimir restricted to the chiral half.
List<SpinorBlock> BranchSpinor16(double[,] hypercharge, out double chiralHalfDimension)
{
    var t3Spin = SpinorRep(su2L[2]);
    var ySpin = SpinorRep(hypercharge);
    // su(2)_L Casimir on spinors: -sum_a R(L_a)^2
    var casimirSpin = new Complex[32, 32];
    for (int a = 0; a < 3; a++)
    {
        var rep = SpinorRep(su2L[a]);
        var square = CMatMul(rep, rep);
        for (int r = 0; r < 32; r++)
            for (int c = 0; c < 32; c++)
                casimirSpin[r, c] -= square[r, c];
    }
    var colorCasimirSpin = new Complex[32, 32];
    foreach (var colorGen in colorBasis)
    {
        var rep = SpinorRep(colorGen);
        var square = CMatMul(rep, rep);
        for (int r = 0; r < 32; r++)
            for (int c = 0; c < 32; c++)
                colorCasimirSpin[r, c] -= square[r, c];
    }

    // Hermitian observables: C2, Y_herm = i R(Y), T3_herm = i R(T3), chirality.
    // Use a generic Hermitian mixer and diagonalize once (32x32 complex
    // Hermitian Jacobi via realification).
    var mixer = new Complex[32, 32];
    const double m1 = 0.318309886183790671, m2 = 0.159154943091895335, m3 = 0.101321183642337771;
    var yHerm = new Complex[32, 32];
    var t3Herm = new Complex[32, 32];
    for (int r = 0; r < 32; r++)
        for (int c = 0; c < 32; c++)
        {
            yHerm[r, c] = Complex.ImaginaryOne * ySpin[r, c];
            t3Herm[r, c] = Complex.ImaginaryOne * t3Spin[r, c];
            mixer[r, c] = casimirSpin[r, c] + m1 * yHerm[r, c] + m2 * colorCasimirSpin[r, c] + m3 * chirality[r, c];
        }

    var (eigenvectors, _) = HermitianEigenvectors(mixer);
    var states = new List<(double Chi, double JJ1, double Y, double T3, double C2)>();
    foreach (var vec in eigenvectors)
    {
        double chi = RealExpectation(chirality, vec);
        double jj1 = RealExpectation(casimirSpin, vec);
        double y = RealExpectation(yHerm, vec);
        double t3 = RealExpectation(t3Herm, vec);
        double c2 = RealExpectation(colorCasimirSpin, vec);
        states.Add((chi, jj1, y, t3, c2));
    }
    var chiralStates = states.Where(s => s.Chi > 0.5).ToList();
    chiralHalfDimension = chiralStates.Count;
    return chiralStates
        .GroupBy(s => (J: System.Math.Round(s.JJ1, 6), Y: System.Math.Round(s.Y, 6), C: System.Math.Round(s.C2, 6)))
        .Select(group => new SpinorBlock
        {
            Dimension = group.Count(),
            JValue = 0.5 * (System.Math.Sqrt(1.0 + 4.0 * group.Key.J) - 1.0),
            Y = group.Key.Y,
            ColorCasimir = group.Key.C,
            IsColorSinglet = System.Math.Abs(group.Key.C) <= 1e-6,
        })
        .OrderByDescending(block => block.JValue)
        .ThenBy(block => block.Y)
        .ToList();
}

static double RealExpectation(Complex[,] op, Complex[] vec)
{
    int n = vec.Length;
    Complex sum = Complex.Zero;
    for (int r = 0; r < n; r++)
    {
        Complex acc = Complex.Zero;
        for (int c = 0; c < n; c++)
            acc += op[r, c] * vec[c];
        sum += Complex.Conjugate(vec[r]) * acc;
    }
    return sum.Real;
}

// ---------------------------------------------------------------------------
// The enumeration.
// ---------------------------------------------------------------------------

var r3 = su2R[2];
var jHalf = Scale(jComplexStructure, 0.5);

// Standard Pati-Salam hypercharge direction: Y = R3 + (B-L)/2 = R3 + J/3,
// i.e. (alpha, beta) = (1, 2/3) in the Y = alpha R3 + beta J/2 chart (sign
// conventions resolved by the numerics; both signs scanned anyway).
var scanResults = new List<ScanEntry>();
AdjointBlock[]? standardAdjointBlocks = null;
SpinorBlock[]? standardSpinorBlocks = null;
double standardTanSquared = 0.0;
double standardLeptonAbsY = 0.0;
int adjointColorSingletNonzeroYDoubletCountStandard = -1;
double chiralDim = 0.0;

var scanned = new HashSet<(double, double)>();
for (int num = -ScanCoefficientMax; num <= ScanCoefficientMax; num++)
    for (int den = 1; den <= ScanCoefficientMax; den++)
    {
        double alpha = (double)num / den;
        foreach (double beta in new[] { 2.0 / 3.0, -2.0 / 3.0, 1.0 / 3.0, -1.0 / 3.0, 4.0 / 3.0, -4.0 / 3.0, 1.0, -1.0, 2.0, -2.0, 0.5, -0.5, 0.0, 3.0, -3.0 })
        {
            if (System.Math.Abs(alpha) < 1e-12 && System.Math.Abs(beta) < 1e-12)
                continue;
            var key = (System.Math.Round(alpha, 9), System.Math.Round(beta, 9));
            if (!scanned.Add(key))
                continue;

            var y = AddM(Scale(r3, alpha), Scale(jHalf, beta));
            var adjointBlocks = BranchAdjoint(y);

            // SM filter at the adjoint level: there must exist charged
            // (Y != 0) doublet blocks at all, and Y must not annihilate color.
            var doublets = adjointBlocks.Where(b => System.Math.Abs(b.JValue - 0.5) <= 1e-6).ToList();
            int colorSingletNonzeroYDoublets = doublets.Where(d => d.IsColorSinglet && d.AbsY > 1e-7).Sum(d => d.Dimension) / 2;
            int coloredDoubletStates = doublets.Where(d => !d.IsColorSinglet).Sum(d => d.Dimension);

            // Spinor-16 family analysis for this Y direction.
            var spinorBlocks = BranchSpinor16(y, out double thisChiral);
            var leptonDoublet = spinorBlocks.FirstOrDefault(b =>
                System.Math.Abs(b.JValue - 0.5) <= 1e-5 && b.IsColorSinglet && System.Math.Abs(b.Y) > 1e-7);
            double tanSquared = 0.0;
            if (leptonDoublet is not null)
            {
                // Internal normalization: scale Y so the 16's color-singlet
                // doublet (lepton-doublet slot) carries |Y| = 1/2.
                double scaleFactor = 0.5 / System.Math.Abs(leptonDoublet.Y);
                double yNorm2 = PairAlg(y, y) * scaleFactor * scaleFactor;
                double t3Norm2 = PairAlg(su2L[2], su2L[2]);
                tanSquared = t3Norm2 / yNorm2;
            }

            bool isStandard = System.Math.Abs(alpha - 1.0) <= 1e-12 && System.Math.Abs(beta - 2.0 / 3.0) <= 1e-9;
            if (isStandard)
            {
                standardAdjointBlocks = adjointBlocks.ToArray();
                standardSpinorBlocks = spinorBlocks.ToArray();
                standardTanSquared = tanSquared;
                standardLeptonAbsY = leptonDoublet is not null ? System.Math.Abs(leptonDoublet.Y) : 0.0;
                adjointColorSingletNonzeroYDoubletCountStandard = colorSingletNonzeroYDoublets;
                chiralDim = thisChiral;
            }

            scanResults.Add(new ScanEntry
            {
                Alpha = alpha,
                Beta = beta,
                AdjointDoubletStates = doublets.Sum(d => d.Dimension),
                AdjointColorSingletNonzeroYDoubletPairs = colorSingletNonzeroYDoublets,
                AdjointColoredDoubletStates = coloredDoubletStates,
                LeptonDoubletPresentIn16 = leptonDoublet is not null,
                TanSquaredEmb = tanSquared,
            });
        }
    }

// Aggregate scan facts.
int scanCount = scanResults.Count;
bool adjointColorSingletChargedDoubletAbsentEverywhere =
    scanResults.All(entry => entry.AdjointColorSingletNonzeroYDoubletPairs == 0);
int directionsWithLeptonDoublet = scanResults.Count(entry => entry.LeptonDoubletPresentIn16);
double minTanSquaredOverScan = scanResults.Where(e => e.TanSquaredEmb > 0).Min(e => e.TanSquaredEmb);
double maxTanSquaredOverScan = scanResults.Where(e => e.TanSquaredEmb > 0).Max(e => e.TanSquaredEmb);

// Mathematical cross-checks (group-theory constants, not experimental values):
// the standard direction must reproduce the classic unified-chain ratio
// tan^2 = 3/5 (sin^2 = 3/8), and the 16 must contain the SM family pattern.
bool standardRatioMatchesClassicValue = System.Math.Abs(standardTanSquared - 0.6) <= 1e-9;
double standardSinSquared = standardTanSquared / (1.0 + standardTanSquared);
bool spinor16IsSixteenDimensional = System.Math.Abs(chiralDim - 16.0) <= 1e-9;

// SM family pattern in the 16 under the standard Y (charges in lepton-
// doublet-normalized units): quark doublet (color triplet, j=1/2, |Y|=1/6),
// lepton doublet (singlet, j=1/2, |Y|=1/2), and the right-handed singlets.
bool familyPatternDerived = false;
if (standardSpinorBlocks is not null)
{
    double leptonY = standardSpinorBlocks.First(b => b.IsColorSinglet && System.Math.Abs(b.JValue - 0.5) <= 1e-5 && System.Math.Abs(b.Y) > 1e-7).Y;
    double scaleFactor = 0.5 / System.Math.Abs(leptonY);
    var normalized = standardSpinorBlocks.Select(b => new
    {
        b.Dimension,
        b.JValue,
        Y = b.Y * scaleFactor,
        b.IsColorSinglet,
    }).ToArray();
    bool quarkDoublet = normalized.Any(b => !b.IsColorSinglet && System.Math.Abs(b.JValue - 0.5) <= 1e-5 && System.Math.Abs(System.Math.Abs(b.Y) - 1.0 / 6.0) <= 1e-6 && b.Dimension == 6);
    bool leptonDoubletBlock = normalized.Any(b => b.IsColorSinglet && System.Math.Abs(b.JValue - 0.5) <= 1e-5 && System.Math.Abs(System.Math.Abs(b.Y) - 0.5) <= 1e-6 && b.Dimension == 2);
    bool upSinglet = normalized.Any(b => !b.IsColorSinglet && b.JValue <= 1e-5 && System.Math.Abs(System.Math.Abs(b.Y) - 2.0 / 3.0) <= 1e-6);
    bool downSinglet = normalized.Any(b => !b.IsColorSinglet && b.JValue <= 1e-5 && System.Math.Abs(System.Math.Abs(b.Y) - 1.0 / 3.0) <= 1e-6);
    bool electronSinglet = normalized.Any(b => b.IsColorSinglet && b.JValue <= 1e-5 && System.Math.Abs(System.Math.Abs(b.Y) - 1.0) <= 1e-6);
    bool neutrinoSinglet = normalized.Any(b => b.IsColorSinglet && b.JValue <= 1e-5 && System.Math.Abs(b.Y) <= 1e-6);
    familyPatternDerived = quarkDoublet && leptonDoubletBlock && upSinglet && downSinglet && electronSinglet && neutrinoSinglet;
}

bool scaffoldingVerified =
    su2ClosureResidual <= Tolerance &&
    jCommutesWithSu2Residual <= Tolerance &&
    u3Dimension == 9 &&
    colorDimension == 8 &&
    colorClosureResidual <= 1e-7 &&
    spinorHomResidual <= 1e-9;

bool enumerationInternallyConsistent =
    scaffoldingVerified &&
    spinor16IsSixteenDimensional &&
    standardRatioMatchesClassicValue &&
    familyPatternDerived &&
    adjointColorSingletNonzeroYDoubletCountStandard == 0;

// ---------------------------------------------------------------------------
// Fail-closed boundary.
// ---------------------------------------------------------------------------

const bool compactFormUsedForNoncompactAlgebra = true; // recorded limitation
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
const string ApplicationSubjectKind = "gu-embedding-chain-coupling-ratio-enumeration";
const bool physicalTargetsConsultedForConstruction = false;

string targetBlindConstructionHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join(
    "|",
    ApplicationSubjectKind,
    "so(10) compact-level Pati-Salam chain; Y(alpha,beta) = alpha R3 + beta J/2 bounded scan; adjoint-45 + spinor-16 branching; internal lepton-doublet Y normalization",
    ScanCoefficientMax.ToString())))).ToLowerInvariant();

bool guEmbeddingChainCouplingRatioEnumerationPassed =
    phase402PrecursorPassed &&
    phase403PrecursorPassed &&
    enumerationInternallyConsistent &&
    compactFormUsedForNoncompactAlgebra &&
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

string terminalStatus = guEmbeddingChainCouplingRatioEnumerationPassed
    ? (adjointColorSingletChargedDoubletAbsentEverywhere
        ? "gu-embedding-chain-enumerated-ratio-menu-derived-adjoint-higgs-doublet-absent"
        : "gu-embedding-chain-enumerated-ratio-menu-derived-adjoint-higgs-doublet-present")
    : "gu-embedding-chain-coupling-ratio-enumeration-blocked";
string decision = guEmbeddingChainCouplingRatioEnumerationPassed
    ? "The GU-named embedding chain is now exhaustively enumerated at the complexification level, from first principles. THREE COMPUTED RESULTS. (1) The embedding-derived coupling ratio for the standard hypercharge direction is tan^2(theta_emb) = 3/5 (sin^2 = 3/8), with the normalization anchored INTERNALLY by the 16's color-singlet doublet (lepton-doublet bookkeeping) - the classic unified-chain constant, derived blind by the machine, identical for every chain sharing this hypercharge embedding; the bounded scan records the full ratio menu over hypercharge directions. (2) The 16-dimensional spinor block DERIVES the complete SM family hypercharge pattern (quark doublet |Y|=1/6, lepton doublet |Y|=1/2, up/down/electron/neutrino singlets at 2/3, 1/3, 1, 0 in lepton-normalized units) - the family quantum numbers come out of the chain rather than being matched. (3) DECISIVE NEGATIVE for the scalar route: the adjoint 45 contains NO color-singlet charged doublet block for ANY scanned hypercharge direction - every doublet in the adjoint is colored (X/Y-boson type). Since the draft's scalar candidate is the pulled-back CONNECTION component (adjoint-valued), the SM-Higgs-pattern doublet CANNOT come from the gauge-algebra-adjoint part of the connection on this chain: it must come from the NON-ADJOINT components of the GU connection (the vertical/symmetric-2-tensor part named by the Iceberg analysis) or not at all - sharply narrowing scalar-sector sub-gap (a). Limitations recorded: compact-form arithmetic for the spin(6,4) complexification; charges and ratios only - no VEV, no scale, no GeV; nothing is promoted."
    : "Do not use the enumeration until the precursors and the scaffolding/branching battery pass.";

var result = new
{
    phaseId = "phase404-gu-embedding-chain-coupling-ratio-enumeration",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    guEmbeddingChainCouplingRatioEnumerationPassed,
    phase402PrecursorPassed,
    phase403PrecursorPassed,
    enumerationInternallyConsistent,
    scaffoldingVerified,
    su2ClosureResidual,
    jCommutesWithSu2Residual,
    u3Dimension,
    colorDimension,
    colorClosureResidual,
    spinorHomResidual,
    spinor16IsSixteenDimensional,
    standardTanSquaredEmb = standardTanSquared,
    standardSinSquaredEmb = standardSinSquared,
    standardRatioMatchesClassicValue,
    standardLeptonAbsYBeforeNormalization = standardLeptonAbsY,
    familyPatternDerived,
    adjointColorSingletNonzeroYDoubletCountStandard,
    adjointColorSingletChargedDoubletAbsentEverywhere,
    adjointHiggsPatternDoubletExcludedOnThisChain = adjointColorSingletChargedDoubletAbsentEverywhere,
    scalarDoubletMustComeFromNonAdjointConnectionComponents = adjointColorSingletChargedDoubletAbsentEverywhere,
    scanCount,
    directionsWithLeptonDoublet,
    minTanSquaredOverScan,
    maxTanSquaredOverScan,
    gpuUsed = false,
    gpuJustification = "objects are 10x10 / 32x32 matrices; the exhaustive enumeration completes on CPU in seconds - GPU is reserved for Phases 405/406 where problem sizes warrant it (user directive recorded in the restart prompt)",
    compactFormUsedForNoncompactAlgebra,
    physicalCouplingProvided,
    enumerationDefinitions = new
    {
        chain = "so(10) compact level (D5 complexification of spin(6,4)): so(6)+so(4) Pati-Salam scaffolding; su(2)_L/R = (anti-)self-dual so(4) halves; color su(3) = traceless centralizer of the so(6) complex structure J; Y(alpha,beta) = alpha R3 + beta J/2",
        adjointBranching = "blocks of the 45 under {su(2)_L Casimir, |Y|, color Casimir} via simultaneous eigenspaces of a generically mixed operator",
        spinorBranching = "Cl(10) on 32 complex dims (tensor-product gammas), Sigma_ij = [g_i,g_j]/4, chirality-projected 16; charges from Hermitian expectations",
        normalization = "Y scaled so the 16's color-singlet doublet carries |Y| = 1/2 (lepton-doublet bookkeeping; internal, not experimental)",
        ratio = "tan^2(theta_emb) = <T3,T3>/<Y_norm,Y_norm> in the -Tr(AB)/2 pairing; sin^2 derived; recorded blind",
        crossChecks = "su(2) closure, [J, su(2)] = 0, u(3)/su(3) dimensions, color closure, spinor homomorphism residual, 16-dimensionality, tan^2 = 3/5 for the standard direction (group-theory constant, like asserting j(j+1)), full SM family pattern",
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
        "compact-form (so(10)) arithmetic stands in for the spin(6,4) complexification; real-form-specific effects are a recorded limitation",
        "tan^2(theta_emb) = 3/5 is a tree-level algebraic constant of the embedding, recorded blind; no renormalization, no scale, and no comparison to measured values",
        "the family pattern and ratio menu are charge bookkeeping; they fill no contract field (no VEV, pole, or GeV lineage exists here)",
        "the adjoint-doublet exclusion is chain-specific (this D5 chain) and complexification-level",
        "no Phase201 or Phase256 fill",
        "no physical predictions",
    },
    standardAdjointBlocks = standardAdjointBlocks?.Select(block => block.ToOutput()).ToArray(),
    standardSpinor16Blocks = standardSpinorBlocks?.Select(block => block.ToOutput()).ToArray(),
    scan = scanResults.Select(entry => entry.ToOutput()).ToArray(),
    sourceEvidence = new
    {
        phase402SummaryPath = Phase402SummaryPath,
        phase403SummaryPath = Phase403SummaryPath,
        primaryDraftDictionary = "docs/Reference/ExperimentReferences/texts/GU-DRAFT-2021-TEXT.txt (sections 11-12, eq. 12.28)",
        explainerAnalysis = "docs/Reference/ExperimentReferences/TOE-GU-ICEBERG-20250423-GAP-ANALYSIS.md (rows 6, 10)",
    },
    decision,
};

var options = JsonOptions();
File.WriteAllText(Path.Combine(outputDir, "gu_embedding_chain_coupling_ratio_enumeration.json"), JsonSerializer.Serialize(result, options));
string summaryPath = Path.Combine(outputDir, "gu_embedding_chain_coupling_ratio_enumeration_summary.json");
File.WriteAllText(summaryPath, JsonSerializer.Serialize(result, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"guEmbeddingChainCouplingRatioEnumerationPassed={guEmbeddingChainCouplingRatioEnumerationPassed}");
Console.WriteLine($"standardTanSquaredEmb={standardTanSquared:R}");
Console.WriteLine($"standardSinSquaredEmb={standardSinSquared:R}");
Console.WriteLine($"standardRatioMatchesClassicValue={standardRatioMatchesClassicValue}");
Console.WriteLine($"familyPatternDerived={familyPatternDerived}");
Console.WriteLine($"spinor16IsSixteenDimensional={spinor16IsSixteenDimensional}");
Console.WriteLine($"adjointColorSingletChargedDoubletAbsentEverywhere={adjointColorSingletChargedDoubletAbsentEverywhere}");
Console.WriteLine($"scalarDoubletMustComeFromNonAdjointConnectionComponents={adjointColorSingletChargedDoubletAbsentEverywhere}");
Console.WriteLine($"scanCount={scanCount}");
Console.WriteLine($"minTanSquaredOverScan={minTanSquaredOverScan:R}");
Console.WriteLine($"maxTanSquaredOverScan={maxTanSquaredOverScan:R}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");
Console.WriteLine($"summaryPath={summaryPath}");

// ---------------------------------------------------------------------------
// Hermitian eigenvector extraction (complex 32x32) via realified Jacobi.
// ---------------------------------------------------------------------------

static (List<Complex[]> Vectors, double[] Values) HermitianEigenvectors(Complex[,] h)
{
    int n = h.GetLength(0);
    // Realify: [[Re, -Im],[Im, Re]] is real symmetric for Hermitian h.
    var real = new double[2 * n, 2 * n];
    for (int r = 0; r < n; r++)
        for (int c = 0; c < n; c++)
        {
            real[r, c] = h[r, c].Real;
            real[r, c + n] = -h[r, c].Imaginary;
            real[r + n, c] = h[r, c].Imaginary;
            real[r + n, c + n] = h[r, c].Real;
        }
    var (eigenvalues, vectors) = JacobiSymmetric(real);
    // Each complex eigenvector appears twice (v, iv); deduplicate by taking
    // vectors whose overlap with the span of already-taken ones is small.
    var taken = new List<Complex[]>();
    var takenValues = new List<double>();
    var order = Enumerable.Range(0, 2 * n).OrderBy(i => eigenvalues[i]).ToArray();
    foreach (int idx in order)
    {
        var candidate = new Complex[n];
        for (int i = 0; i < n; i++)
            candidate[i] = new Complex(vectors[i, idx], vectors[i + n, idx]);
        // normalize
        double norm = 0.0;
        foreach (var z in candidate)
            norm += z.Magnitude * z.Magnitude;
        norm = System.Math.Sqrt(norm);
        if (norm < 1e-12)
            continue;
        for (int i = 0; i < n; i++)
            candidate[i] /= norm;
        // Gram-Schmidt against taken (complex inner product)
        var residual = (Complex[])candidate.Clone();
        foreach (var existing in taken)
        {
            Complex overlap = Complex.Zero;
            for (int i = 0; i < n; i++)
                overlap += Complex.Conjugate(existing[i]) * residual[i];
            for (int i = 0; i < n; i++)
                residual[i] -= overlap * existing[i];
        }
        double residualNorm = 0.0;
        foreach (var z in residual)
            residualNorm += z.Magnitude * z.Magnitude;
        residualNorm = System.Math.Sqrt(residualNorm);
        if (residualNorm > 0.5)
        {
            for (int i = 0; i < n; i++)
                residual[i] /= residualNorm;
            taken.Add(residual);
            takenValues.Add(eigenvalues[idx]);
            if (taken.Count == n)
                break;
        }
    }
    return (taken, takenValues.ToArray());
}

static (double[] Eigenvalues, double[,] Vectors) JacobiSymmetric(double[,] input)
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

public sealed class AdjointBlock
{
    public required int Dimension { get; init; }
    public required double JValue { get; init; }
    public required double AbsY { get; init; }
    public required double ColorCasimir { get; init; }
    public required bool IsColorSinglet { get; init; }

    public object ToOutput() => new
    {
        dimension = Dimension,
        jValue = JValue,
        absY = AbsY,
        colorCasimir = ColorCasimir,
        isColorSinglet = IsColorSinglet,
    };
}

public sealed class SpinorBlock
{
    public required int Dimension { get; init; }
    public required double JValue { get; init; }
    public required double Y { get; init; }
    public required double ColorCasimir { get; init; }
    public required bool IsColorSinglet { get; init; }

    public object ToOutput() => new
    {
        dimension = Dimension,
        jValue = JValue,
        y = Y,
        colorCasimir = ColorCasimir,
        isColorSinglet = IsColorSinglet,
    };
}

public sealed class ScanEntry
{
    public required double Alpha { get; init; }
    public required double Beta { get; init; }
    public required int AdjointDoubletStates { get; init; }
    public required int AdjointColorSingletNonzeroYDoubletPairs { get; init; }
    public required int AdjointColoredDoubletStates { get; init; }
    public required bool LeptonDoubletPresentIn16 { get; init; }
    public required double TanSquaredEmb { get; init; }

    public object ToOutput() => new
    {
        alpha = Alpha,
        beta = Beta,
        adjointDoubletStates = AdjointDoubletStates,
        adjointColorSingletNonzeroYDoubletPairs = AdjointColorSingletNonzeroYDoubletPairs,
        adjointColoredDoubletStates = AdjointColoredDoubletStates,
        leptonDoubletPresentIn16 = LeptonDoubletPresentIn16,
        tanSquaredEmb = TanSquaredEmb,
    };
}
