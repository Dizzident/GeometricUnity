using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Gu.Math;

// Phase403: adjoint doublet-substructure branching probe.
//
// Phase402 sharpened the scalar-sector gap to three named sub-gaps, headed
// by: exhibit a DOUBLET-EQUIVALENT substructure inside the pulled-back
// (Lie-algebra-valued) connection component - because an adjoint-triplet
// VEV cannot produce the SM neutral sector. This phase machine-checks the
// only GU-native mechanism by which an adjoint-valued scalar CAN contain a
// doublet: branching of a LARGER algebra's adjoint under an embedded
// su(2) x u(1) subalgebra (the gauge-Higgs-unification mechanism - the
// off-diagonal blocks of a larger adjoint transform as doublets of the
// subgroup). Three fail-closed results:
//
//   1. BRANCHING (computed from the repo's own structure constants, no
//      external input): the su(2) adjoint contains NO spin-1/2 block
//      (j-Casimir spectrum pure j=1) - so the existing su(2)-only control
//      branch CANNOT realize the doublet route, which machine-explains the
//      Phase397 photon/Z underdetermination at the algebra level. The
//      su(3) adjoint branches as 3_0 + 1_0 + 2_{+Y0} + 2_{-Y0} under the
//      canonical su(2) x u(1) subalgebra (generators 1-3 and lambda_8):
//      doublet blocks EXIST in larger adjoints, with the hypercharge
//      magnitude Y0 fixed by the embedding.
//
//   2. CUSTODIAL PATTERN: a VEV in the su(3) adjoint's doublet block
//      produces the SM-shaped pattern - one massless neutral, one massive
//      neutral from T3-Y mixing, a degenerate charged pair, and the
//      custodial identity m_charged^2 = m_neutral^2 cos^2(theta_emb)
//      EXACTLY, where theta_emb is read off the same mass matrix.
//
//   3. EMBEDDING-DERIVED COUPLING RATIO: because every gauge field shares
//      the single larger-algebra coupling, the neutral mixing ratio
//      tan^2(theta_emb) = |[Y-gen, phi0]|^2 / |[T3, phi0]|^2 is DERIVED
//      from the embedding (for the su(3) toy it computes to a fixed
//      rational value) - a theorem-level, target-independent MECHANISM for
//      the hypercharge/coupling-ratio lineage row. The number is recorded
//      blind; no comparison against measured electroweak values is made
//      anywhere in this phase, and su(3) is a study toy, NOT GU's algebra:
//      the GU-specific embedding chain (spin(6,4)/su(3,2) -> SM) remains
//      the open derivation.
//
// Fail-closed: no contract field is filled; no scales exist here; nothing
// is promoted.

const string DefaultOutputDir = "studies/phase403_adjoint_doublet_substructure_branching_probe_001/output";
const string Phase397SummaryPath = "studies/phase397_parametrized_u1_extension_neutral_mixing_underdetermination_probe_001/output/parametrized_u1_extension_neutral_mixing_underdetermination_probe_summary.json";
const string Phase402SummaryPath = "studies/phase402_gu_draft_scalar_route_dictionary_audit_001/output/gu_draft_scalar_route_dictionary_audit_summary.json";

const double SpectralTolerance = 1e-12;
const double CustodialTolerance = 1e-12;
// Arbitrary non-physical VEV magnitude (the pattern is scale-invariant).
const double StudyVev = 2.0;

var outputDir = Environment.GetEnvironmentVariable("PHASE403_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

// ---------------------------------------------------------------------------
// Precursors.
// ---------------------------------------------------------------------------

using var phase397Doc = JsonDocument.Parse(File.ReadAllText(Phase397SummaryPath));
bool phase397PrecursorPassed =
    JsonBool(phase397Doc.RootElement, "parametrizedU1ExtensionNeutralMixingUnderdeterminationProbePassed") is true &&
    JsonBool(phase397Doc.RootElement, "photonZSeparationUnderdetermined") is true;
using var phase402Doc = JsonDocument.Parse(File.ReadAllText(Phase402SummaryPath));
bool phase402PrecursorPassed =
    JsonBool(phase402Doc.RootElement, "guDraftScalarRouteDictionaryAuditPassed") is true &&
    JsonBool(phase402Doc.RootElement, "routeRequiresDoubletEquivalentSubstructure") is true;

// ---------------------------------------------------------------------------
// Branching analysis from the repo's own structure constants.
// ---------------------------------------------------------------------------

var su2 = LieAlgebraFactory.CreateSu2WithTracePairing();
var su3 = LieAlgebraFactory.CreateSu3();

// su(2) adjoint under itself: j-Casimir spectrum.
var su2Branching = ComputeBranching(su2, su2Indices: [0, 1, 2], hyperchargeIndex: null);
int su2DoubletBlockCount = su2Branching.Blocks.Count(block => block.IsDoublet);
bool su2AdjointHasNoDoubletBlock = su2DoubletBlockCount == 0;
bool su2AdjointIsPureTriplet =
    su2Branching.Blocks.Count == 1 &&
    su2Branching.Blocks[0].Dimension == 3 &&
    System.Math.Abs(su2Branching.Blocks[0].JValue - 1.0) <= SpectralTolerance;

// su(3) adjoint under the canonical su(2) x u(1): generators 1-3 (lambda_1..3)
// span the su(2); lambda_8 (index 7) generates the u(1).
var su3Branching = ComputeBranching(su3, su2Indices: [0, 1, 2], hyperchargeIndex: 7);
var su3DoubletBlocks = su3Branching.Blocks.Where(block => block.IsDoublet).ToArray();
int su3DoubletBlockCount = su3DoubletBlocks.Length;
// In the REAL adjoint basis the conjugate pair 2_{+Y0} + 2_{-Y0} appears as
// a single 4-real-dimensional j = 1/2 sector with uniform |Y| (Y^2 cannot
// separate the signs); that real form IS the complex doublet plus conjugate.
bool su3AdjointContainsConjugateDoubletPair =
    su3DoubletBlockCount == 1 &&
    su3DoubletBlocks[0].Dimension == 4 &&
    su3DoubletBlocks[0].AbsHypercharge > SpectralTolerance;
bool su3BranchingDimensionsConsistent = su3Branching.Blocks.Sum(block => block.Dimension) == 8;
double su3DoubletAbsHypercharge = su3DoubletBlockCount > 0 ? su3DoubletBlocks[0].AbsHypercharge : 0.0;

// ---------------------------------------------------------------------------
// Custodial pattern for a VEV in the su(3) doublet block.
// ---------------------------------------------------------------------------
// phi0 = v * T_4 (Gell-Mann index 4 = basis index 3) lies in the doublet
// block. Unbroken-candidate gauge directions: T1, T2, T3, T8.
// M^2_{AB} = <[T_A, phi0], [T_B, phi0]> (plain coefficient pairing; the
// structural pattern is pairing-scale-invariant).

var phi0 = new double[8];
phi0[3] = StudyVev;
int[] gaugeDirections = [0, 1, 2, 7];
var massMatrix = new double[4, 4];
for (int a = 0; a < 4; a++)
{
    var ea = BasisVector(8, gaugeDirections[a]);
    var bracketA = su3.Bracket(ea, phi0);
    for (int b = a; b < 4; b++)
    {
        var eb = BasisVector(8, gaugeDirections[b]);
        var bracketB = su3.Bracket(eb, phi0);
        double entry = Dot(bracketA, bracketB);
        massMatrix[a, b] = entry;
        massMatrix[b, a] = entry;
    }
}

var (massEigenvalues, _) = JacobiSymmetric(massMatrix);
var orderedMass = massEigenvalues.OrderBy(value => value).ToArray();
double massScale = orderedMass[^1];
double zeroTolerance = System.Math.Max(1e-14, SpectralTolerance * System.Math.Max(massScale, 1.0));
int masslessCount = orderedMass.Count(value => System.Math.Abs(value) <= zeroTolerance);
bool chargedPairDegenerate =
    System.Math.Abs(massMatrix[0, 0] - massMatrix[1, 1]) <= zeroTolerance &&
    System.Math.Abs(massMatrix[0, 2]) <= zeroTolerance && System.Math.Abs(massMatrix[0, 3]) <= zeroTolerance &&
    System.Math.Abs(massMatrix[1, 2]) <= zeroTolerance && System.Math.Abs(massMatrix[1, 3]) <= zeroTolerance;
bool neutralMixingPresent = System.Math.Abs(massMatrix[2, 3]) > zeroTolerance;

// Embedding-derived neutral mixing: tan^2(theta_emb) read off the same data.
double embeddingTanSquared = massMatrix[3, 3] / System.Math.Max(massMatrix[2, 2], 1e-300);
double embeddingCosSquared = massMatrix[2, 2] / (massMatrix[2, 2] + massMatrix[3, 3]);
double chargedMassSquared = massMatrix[0, 0];
double neutralMassiveMassSquared = massMatrix[2, 2] + massMatrix[3, 3];
double custodialIdentityResidual =
    System.Math.Abs(chargedMassSquared - neutralMassiveMassSquared * embeddingCosSquared)
    / System.Math.Max(chargedMassSquared, 1e-300);
bool custodialPatternProducedByAdjointDoubletBlock =
    masslessCount == 1 &&
    chargedPairDegenerate &&
    neutralMixingPresent &&
    custodialIdentityResidual <= CustodialTolerance;

bool doubletSubstructureExistsInLargerAdjoint =
    su3AdjointContainsConjugateDoubletPair && custodialPatternProducedByAdjointDoubletBlock;
bool couplingRatioLineageMechanismIdentified =
    doubletSubstructureExistsInLargerAdjoint && embeddingTanSquared > SpectralTolerance;
bool toyBranchPhotonZUnderdeterminationExplained =
    su2AdjointHasNoDoubletBlock && phase397PrecursorPassed;

bool probeInternallyConsistent =
    su2AdjointIsPureTriplet &&
    su2AdjointHasNoDoubletBlock &&
    su3BranchingDimensionsConsistent &&
    su3AdjointContainsConjugateDoubletPair &&
    custodialPatternProducedByAdjointDoubletBlock;

// ---------------------------------------------------------------------------
// Fail-closed boundary.
// ---------------------------------------------------------------------------

const bool su3IsGuAlgebra = false;
const bool physicalEmbeddingChainDerived = false;
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
const string ApplicationSubjectKind = "adjoint-doublet-substructure-branching-probe";
const bool physicalTargetsConsultedForConstruction = false;

string targetBlindConstructionHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join(
    "|",
    ApplicationSubjectKind,
    "su2/su3 adjoint branching from repo structure constants; custodial pattern from M2_AB = <[T_A,phi0],[T_B,phi0]>; embedding-derived tan^2",
    StudyVev.ToString("R"))))).ToLowerInvariant();

bool adjointDoubletSubstructureBranchingProbePassed =
    phase397PrecursorPassed &&
    phase402PrecursorPassed &&
    probeInternallyConsistent &&
    !su3IsGuAlgebra &&
    !physicalEmbeddingChainDerived &&
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

string terminalStatus = adjointDoubletSubstructureBranchingProbePassed
    ? "adjoint-doublet-substructure-exists-in-larger-adjoint-embedding-derived-ratio-mechanism-identified"
    : "adjoint-doublet-substructure-branching-probe-blocked";
string decision = adjointDoubletSubstructureBranchingProbePassed
    ? "The Phase402 doublet-equivalent sub-gap is now machine-characterized at the algebra level, from the repo's own structure constants. (1) The su(2) adjoint is a pure j = 1 triplet with NO doublet block: the existing su(2)-only control branch CANNOT realize the doublet route, which explains the Phase397 photon/Z underdetermination at the algebra level - the toy algebra is too small. (2) The su(3) adjoint branches as 3_0 + 1_0 + 2_{+Y0} + 2_{-Y0} under the canonical su(2) x u(1) subalgebra: doublet blocks DO exist inside larger adjoints (the gauge-Higgs-unification mechanism), so the GU route (Lie-algebra-valued scalar from a large algebra) is NOT structurally excluded - it requires the larger-algebra branching. (3) A VEV in the doublet block produces the SM-shaped custodial pattern EXACTLY (one massless neutral, T3-Y mixing, degenerate charged pair, custodial identity at machine precision), with the neutral mixing ratio tan^2(theta_emb) DERIVED from the embedding rather than input - a theorem-level, target-independent MECHANISM for the hypercharge/coupling-ratio lineage row. The derived toy value is recorded blind; su(3) is a study toy, NOT GU's algebra; the GU-specific embedding chain (spin(6,4)/su(3,2) -> SM with its derived ratio), the vacuum-manifold selection of the doublet-block VEV, and the entire quantitative chain remain the open gaps. Nothing is promoted; no contract field is filled."
    : "Do not use the branching characterization until the precursors and the branching/custodial battery pass.";

var result = new
{
    phaseId = "phase403-adjoint-doublet-substructure-branching-probe",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    adjointDoubletSubstructureBranchingProbePassed,
    phase397PrecursorPassed,
    phase402PrecursorPassed,
    probeInternallyConsistent,
    su2AdjointIsPureTriplet,
    su2AdjointHasNoDoubletBlock,
    su2DoubletBlockCount,
    su3BranchingDimensionsConsistent,
    su3AdjointContainsConjugateDoubletPair,
    su3DoubletBlockCount,
    su3DoubletAbsHypercharge,
    custodialPatternProducedByAdjointDoubletBlock,
    custodialIdentityResidual,
    masslessCount,
    chargedPairDegenerate,
    neutralMixingPresent,
    embeddingTanSquared,
    embeddingCosSquared,
    doubletSubstructureExistsInLargerAdjoint,
    couplingRatioLineageMechanismIdentified,
    toyBranchPhotonZUnderdeterminationExplained,
    su3IsGuAlgebra,
    physicalEmbeddingChainDerived,
    physicalCouplingProvided,
    probeDefinitions = new
    {
        branching = "ad-matrices built from LieAlgebraFactory structure constants; j-Casimir = -sum_a (ad T_a)^2 over the embedded su(2) (generators lambda_1..3), hypercharge magnitude from -(ad lambda_8)^2; blocks identified by simultaneous eigenspaces",
        custodial = "M^2_{AB} = <[T_A, phi0],[T_B, phi0]> over (T1,T2,T3,lambda_8) with phi0 = v lambda_4 in the doublet block; pattern checks are structural (massless count, charged degeneracy, neutral mixing, custodial identity) with the mixing angle READ OFF the same matrix",
        targetBlindness = "no measured electroweak value appears anywhere; the embedding-derived tan^2(theta_emb) is recorded as a structural output without comparison",
        su2Branching = su2Branching.Describe(),
        su3Branching = su3Branching.Describe(),
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
        "su(3) is a minimal study toy for the branching mechanism, NOT GU's algebra; the GU-specific embedding chain remains underived",
        "the embedding-derived ratio is recorded blind; no comparison against measured electroweak values is made",
        "no vacuum-manifold mechanism selects the doublet-block VEV here; that remains a named gap",
        "no scales, poles, or GeV lineage exist in this probe",
        "no Phase201 or Phase256 fill",
        "no physical predictions",
    },
    su2Blocks = su2Branching.Blocks.Select(block => block.ToOutput()).ToArray(),
    su3Blocks = su3Branching.Blocks.Select(block => block.ToOutput()).ToArray(),
    massMatrix = new
    {
        basis = new[] { "T1", "T2", "T3", "lambda8" },
        entries = Flatten(massMatrix),
        eigenvalues = orderedMass,
        chargedMassSquared,
        neutralMassiveMassSquared,
    },
    sourceEvidence = new
    {
        phase397SummaryPath = Phase397SummaryPath,
        phase402SummaryPath = Phase402SummaryPath,
        structureConstantSource = "src/Gu.Math/LieAlgebraFactory.cs (CreateSu2WithTracePairing, CreateSu3)",
        primaryDraftDictionary = "docs/Reference/ExperimentReferences/texts/GU-DRAFT-2021-TEXT.txt (eq. 12.28)",
    },
    decision,
};

var options = JsonOptions();
File.WriteAllText(Path.Combine(outputDir, "adjoint_doublet_substructure_branching_probe.json"), JsonSerializer.Serialize(result, options));
string summaryPath = Path.Combine(outputDir, "adjoint_doublet_substructure_branching_probe_summary.json");
File.WriteAllText(summaryPath, JsonSerializer.Serialize(result, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"adjointDoubletSubstructureBranchingProbePassed={adjointDoubletSubstructureBranchingProbePassed}");
Console.WriteLine($"su2AdjointHasNoDoubletBlock={su2AdjointHasNoDoubletBlock}");
Console.WriteLine($"su3AdjointContainsConjugateDoubletPair={su3AdjointContainsConjugateDoubletPair}");
Console.WriteLine($"su3DoubletAbsHypercharge={su3DoubletAbsHypercharge:R}");
Console.WriteLine($"custodialPatternProducedByAdjointDoubletBlock={custodialPatternProducedByAdjointDoubletBlock}");
Console.WriteLine($"custodialIdentityResidual={custodialIdentityResidual:R}");
Console.WriteLine($"embeddingTanSquared={embeddingTanSquared:R}");
Console.WriteLine($"couplingRatioLineageMechanismIdentified={couplingRatioLineageMechanismIdentified}");
Console.WriteLine($"toyBranchPhotonZUnderdeterminationExplained={toyBranchPhotonZUnderdeterminationExplained}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");
Console.WriteLine($"summaryPath={summaryPath}");

// ---------------------------------------------------------------------------
// Branching machinery.
// ---------------------------------------------------------------------------

static BranchingResult ComputeBranching(LieAlgebra algebra, int[] su2Indices, int? hyperchargeIndex)
{
    int dim = algebra.Dimension;

    // j-Casimir: C2 = -sum_a (ad T_a)^2 over the embedded su(2).
    var casimir = new double[dim, dim];
    foreach (int a in su2Indices)
    {
        var ad = AdMatrix(algebra, a);
        for (int i = 0; i < dim; i++)
            for (int j = 0; j < dim; j++)
            {
                double sum = 0.0;
                for (int k = 0; k < dim; k++)
                    sum += ad[i, k] * ad[k, j];
                casimir[i, j] -= sum;
            }
    }

    // Hypercharge-squared operator: Y2 = -(ad T_Y)^2.
    var hypercharge2 = new double[dim, dim];
    if (hyperchargeIndex is int yIndex)
    {
        var adY = AdMatrix(algebra, yIndex);
        for (int i = 0; i < dim; i++)
            for (int j = 0; j < dim; j++)
            {
                double sum = 0.0;
                for (int k = 0; k < dim; k++)
                    sum += adY[i, k] * adY[k, j];
                hypercharge2[i, j] = -sum;
            }
    }

    // Simultaneous diagnosis: diagonalize C2 + mu * Y2 (generic mu separates
    // joint eigenspaces), then read both eigenvalues per eigenvector.
    const double mu = 0.318309886183790671; // 1/pi: generic irrational mixer
    var combined = new double[dim, dim];
    for (int i = 0; i < dim; i++)
        for (int j = 0; j < dim; j++)
            combined[i, j] = casimir[i, j] + mu * hypercharge2[i, j];
    var (eigenvalues, vectors) = JacobiSymmetric(combined);

    // Group eigenvectors by (j, |Y|) read from Rayleigh quotients.
    var assignments = new List<(double JJ1, double Y2)>();
    for (int v = 0; v < dim; v++)
    {
        var vector = new double[dim];
        for (int i = 0; i < dim; i++)
            vector[i] = vectors[i, v];
        double jj1 = Quadratic(casimir, vector);
        double y2 = Quadratic(hypercharge2, vector);
        assignments.Add((jj1, y2));
    }

    var blocks = assignments
        .GroupBy(pair => (JJ1: System.Math.Round(pair.JJ1, 9), Y2: System.Math.Round(pair.Y2, 9)))
        .Select(group =>
        {
            double jj1 = group.Key.JJ1;
            double j = 0.5 * (System.Math.Sqrt(1.0 + 4.0 * jj1) - 1.0);
            return new BranchingBlock
            {
                Dimension = group.Count(),
                JCasimir = jj1,
                JValue = j,
                AbsHypercharge = System.Math.Sqrt(System.Math.Max(group.Key.Y2, 0.0)),
                IsDoublet = System.Math.Abs(jj1 - 0.75) <= 1e-9,
            };
        })
        .OrderByDescending(block => block.JCasimir)
        .ThenByDescending(block => block.AbsHypercharge)
        .ToList();

    return new BranchingResult { Blocks = blocks };
}

static double[,] AdMatrix(LieAlgebra algebra, int generatorIndex)
{
    int dim = algebra.Dimension;
    var matrix = new double[dim, dim];
    var generator = BasisVector(dim, generatorIndex);
    for (int b = 0; b < dim; b++)
    {
        var basis = BasisVector(dim, b);
        var bracket = algebra.Bracket(generator, basis);
        for (int c = 0; c < dim; c++)
            matrix[c, b] = bracket[c];
    }
    return matrix;
}

static double Quadratic(double[,] matrix, double[] vector)
{
    int n = vector.Length;
    double sum = 0.0;
    for (int i = 0; i < n; i++)
        for (int j = 0; j < n; j++)
            sum += vector[i] * matrix[i, j] * vector[j];
    return sum;
}

static double[] BasisVector(int dim, int index)
{
    var vector = new double[dim];
    vector[index] = 1.0;
    return vector;
}

static double Dot(double[] left, double[] right)
{
    double sum = 0.0;
    for (int i = 0; i < left.Length; i++)
        sum += left[i] * right[i];
    return sum;
}

static double[] Flatten(double[,] matrix)
{
    int rows = matrix.GetLength(0);
    int cols = matrix.GetLength(1);
    var flat = new double[rows * cols];
    for (int i = 0; i < rows; i++)
        for (int j = 0; j < cols; j++)
            flat[i * cols + j] = matrix[i, j];
    return flat;
}

static (double[] Eigenvalues, double[,] Vectors) JacobiSymmetric(double[,] input)
{
    int n = input.GetLength(0);
    var a = (double[,])input.Clone();
    var vectors = new double[n, n];
    for (int i = 0; i < n; i++)
        vectors[i, i] = 1.0;
    for (int sweep = 0; sweep < 200; sweep++)
    {
        double off = 0.0;
        for (int p = 0; p < n; p++)
            for (int q = p + 1; q < n; q++)
                off += a[p, q] * a[p, q];
        if (System.Math.Sqrt(off) < 1e-15)
            break;
        for (int p = 0; p < n - 1; p++)
            for (int q = p + 1; q < n; q++)
            {
                if (System.Math.Abs(a[p, q]) < 1e-18)
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

public sealed class BranchingBlock
{
    public required int Dimension { get; init; }
    public required double JCasimir { get; init; }
    public required double JValue { get; init; }
    public required double AbsHypercharge { get; init; }
    public required bool IsDoublet { get; init; }

    public object ToOutput() => new
    {
        dimension = Dimension,
        jCasimir = JCasimir,
        jValue = JValue,
        absHypercharge = AbsHypercharge,
        isDoublet = IsDoublet,
    };
}

public sealed class BranchingResult
{
    public required IReadOnlyList<BranchingBlock> Blocks { get; init; }

    public string Describe() => string.Join("; ", Blocks.Select(block =>
        $"dim={block.Dimension} j={block.JValue:F3} |Y|={block.AbsHypercharge:F6}"));
}
