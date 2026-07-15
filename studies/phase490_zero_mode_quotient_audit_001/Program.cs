using System.Diagnostics;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

var stopwatch = Stopwatch.StartNew();
const string outputDir = "studies/phase490_zero_mode_quotient_audit_001/output";
const string unique = "unique-quotient-derived";
const string family = "quotient-family-derived";
const string underdetermined = "quotient-underdetermined";
const string invalid = "invalid-committed-inputs";
const string decisionContract = "phase490-a7-v1|invalid-if-required-input-missing-malformed-or-committed-spectrum-check-red|unique-only-if-one-compatible-generator-image-projector-determines-all-removals|family-only-if-compatible-generator-image-evidence-determines-more-than-one-admissible-quotient|otherwise-valid-inputs-underdetermined|spectral-nullity-never-implies-gauge-status";
const double tolerance = 2e-12;

var primaryPaths = new[]
{
    "studies/phase428_fermion_loop_block_selection_no_go_probe_001/Program.cs",
    "studies/phase428_fermion_loop_block_selection_no_go_probe_001/output/fermion_loop_block_selection_no_go_probe_summary.json",
    "studies/phase430_net_one_loop_direction_selection_probe_001/Program.cs",
    "studies/phase430_net_one_loop_direction_selection_probe_001/output/net_one_loop_direction_selection_probe_summary.json",
    "studies/phase455_exact_fermionic_backreaction_probe_001/Program.cs",
    "studies/phase455_exact_fermionic_backreaction_probe_001/output/exact_fermionic_backreaction_probe_summary.json",
};
var supportingPaths = new[]
{
    "src/Gu.Phase3.GaugeReduction/GaugeActionLinearization.cs",
    "src/Gu.Phase3.GaugeReduction/GaugeProjector.cs",
    "src/Gu.Phase3.Spectra/LinearizedOperatorBundle.cs",
    "src/Gu.Phase3.Spectra/PhysicalModeFormulation.cs",
    "src/Gu.Phase4.Dirac/DiracGaugeReductionProjector.cs",
    "studies/phase97_identity_fermion_lift_quotient_derivation_001/output/identity_fermion_lift_connection_quotient_derivation.json",
};
var inputEvidence = primaryPaths.Select(path => Evidence(path, "primary-spectrum-and-source"))
    .Concat(supportingPaths.Select(path => Evidence(path, "supporting-generator-projector-domain")))
    .ToArray();
bool allFilesPresent = inputEvidence.All(item => item.Present);

bool summariesValid = false;
string summaryValidationDetail;
try
{
    using var p428 = JsonDocument.Parse(File.ReadAllText(primaryPaths[1]));
    using var p430 = JsonDocument.Parse(File.ReadAllText(primaryPaths[3]));
    using var p455 = JsonDocument.Parse(File.ReadAllText(primaryPaths[5]));
    summariesValid = Bool(p428.RootElement, "fermionLoopBlockSelectionNoGoProbePassed")
        && Bool(p428.RootElement, "closedFormSpectrumVerified")
        && Bool(p428.RootElement, "analysisInternallyConsistent")
        && String(p428.RootElement, "terminalStatus") == "fermion-loop-cannot-select-doublet-on-rank-one-rays-mechanism-class-closed"
        && Bool(p430.RootElement, "netOneLoopDirectionSelectionProbePassed")
        && Bool(p430.RootElement, "closedFormSpectrumVerified")
        && Bool(p430.RootElement, "analysisInternallyConsistent")
        && String(p430.RootElement, "terminalStatus") == "net-one-loop-direction-selective-hypercharge-axis-preferred-no-scale-law"
        && Bool(p455.RootElement, "exactFermionicBackreactionProbePassed")
        && !Bool(p455.RootElement, "anyBatteryRed")
        && Bool(p455.RootElement, "conventionFragile")
        && String(p455.RootElement, "terminalStatus") == "exact-fermionic-backreaction-probe-convention-fragile";
    summaryValidationDetail = summariesValid ? "all three committed summary terminals and spectrum batteries match" : "one or more committed summary terminals or spectrum batteries drifted";
}
catch (Exception exception) when (exception is IOException or JsonException or InvalidOperationException)
{
    summariesValid = false;
    summaryValidationDetail = $"summary parse failed: {exception.GetType().Name}";
}

string p428Source = allFilesPresent ? File.ReadAllText(primaryPaths[0]) : "";
string p430Source = allFilesPresent ? File.ReadAllText(primaryPaths[2]) : "";
string p455Source = allFilesPresent ? File.ReadAllText(primaryPaths[4]) : "";
bool sourceFormulaPinsValid = p428Source.Contains("double lambdaSquared = a1 * a1 + a2 * a2;", StringComparison.Ordinal)
    && p430Source.Contains("double arg = eps2 + t * t * m2;", StringComparison.Ordinal)
    && p455Source.Contains("Q3[] FundEig(string axis)", StringComparison.Ordinal)
    && p455Source.Contains("Q3[] AdjEig(string axis)", StringComparison.Ordinal)
    && p455Source.Contains("convention != \"Za\"", StringComparison.Ordinal)
    && p455Source.Contains("lambda^2 <= 1e-18", StringComparison.Ordinal);

Complex[][,] basis = GellMannHalfBasis();
var axisDefinitions = new[]
{
    new AxisDefinition("T", basis[0]),
    new AxisDefinition("D", basis[3]),
    new AxisDefinition("S", basis[7]),
};
var axes = new List<AxisAudit>();
double maximumSpectrumResidual = 0.0;
foreach (var definition in axisDefinitions)
{
    double[] fundamental = JacobiEigen(RealPart(definition.Matrix)).OrderBy(x => x).ToArray();
    double[] adjointSquared = JacobiEigen(AdjointSquared(definition.Matrix, basis)).OrderBy(x => x).ToArray();
    double[] expectedFundamental = definition.Id == "S"
        ? new[] { -1.0 / Math.Sqrt(3.0), 1.0 / (2.0 * Math.Sqrt(3.0)), 1.0 / (2.0 * Math.Sqrt(3.0)) }
        : new[] { -0.5, 0.0, 0.5 };
    double[] expectedAdjointSquared = definition.Id == "S"
        ? new[] { 0.0, 0.0, 0.0, 0.0, 0.75, 0.75, 0.75, 0.75 }
        : new[] { 0.0, 0.0, 0.25, 0.25, 0.25, 0.25, 1.0, 1.0 };
    double fundamentalResidual = MaxResidual(fundamental, expectedFundamental);
    double adjointResidual = MaxResidual(adjointSquared, expectedAdjointSquared);
    maximumSpectrumResidual = Math.Max(maximumSpectrumResidual, Math.Max(fundamentalResidual, adjointResidual));
    int fundamentalNullity = fundamental.Count(value => Math.Abs(value) <= tolerance);
    int adjointNullity = adjointSquared.Count(value => Math.Abs(value) <= tolerance);
    axes.Add(new AxisAudit(
        definition.Id,
        fundamental,
        adjointSquared,
        fundamentalResidual,
        adjointResidual,
        fundamentalNullity,
        adjointNullity,
        4 * fundamentalNullity,
        4 * adjointNullity,
        2 * adjointNullity));
}
bool spectraReconstructed = maximumSpectrumResidual <= tolerance;

const int latticeSize = 4;
var vanishingSineMomenta = new List<int[]>();
for (int n1 = 0; n1 < latticeSize; n1++)
for (int n2 = 0; n2 < latticeSize; n2++)
{
    double s1 = Math.Sin(2.0 * Math.PI * n1 / latticeSize);
    double s2 = Math.Sin(2.0 * Math.PI * n2 / latticeSize);
    if (Math.Abs(s1) <= tolerance && Math.Abs(s2) <= tolerance) vanishingSineMomenta.Add(new[] { n1, n2 });
}
int geometricZeroMomentumCount = vanishingSineMomenta.Count(momentum => momentum[0] == 0 && momentum[1] == 0);
int latticeDoublerVanishingSineCount = vanishingSineMomenta.Count - geometricZeroMomentumCount;

var zeroModeSectors = axes.SelectMany(axis => new[]
{
    new
    {
        axis = axis.Id,
        sector = "fundamental-fermion",
        stateDimensionPerMomentum = 12,
        exactKernelDimensionPerMomentumAtGenericNonzeroT = axis.FundamentalFermionKernelDimension,
        exactKernelDimensionAcrossAllVanishingSineMomenta = axis.FundamentalFermionKernelDimension * vanishingSineMomenta.Count,
        generatorImageClassifiedDimension = 0,
        classification = axis.FundamentalFermionKernelDimension == 0 ? "no-exact-kernel" : "exact-but-unclassified",
    },
    new
    {
        axis = axis.Id,
        sector = "adjoint-fermion",
        stateDimensionPerMomentum = 32,
        exactKernelDimensionPerMomentumAtGenericNonzeroT = axis.AdjointFermionKernelDimension,
        exactKernelDimensionAcrossAllVanishingSineMomenta = axis.AdjointFermionKernelDimension * vanishingSineMomenta.Count,
        generatorImageClassifiedDimension = 0,
        classification = "exact-but-unclassified",
    },
    new
    {
        axis = axis.Id,
        sector = "bosonic-adjoint-two-polarization-workbench",
        stateDimensionPerMomentum = 16,
        exactKernelDimensionPerMomentumAtGenericNonzeroT = axis.BosonKernelDimension,
        exactKernelDimensionAcrossAllVanishingSineMomenta = axis.BosonKernelDimension * vanishingSineMomenta.Count,
        generatorImageClassifiedDimension = 0,
        classification = "exact-but-unclassified",
    },
}).ToArray();

var generatorProjectorEvidence = new[]
{
    new
    {
        id = "phase3-connection-generator-image",
        evidencePath = supportingPaths[0],
        domain = "vertex-valued gauge parameters",
        codomain = "edge-major connection-one-form coefficients",
        compatibleWithPhase455DeterminantStateSpace = false,
        classificationAuthority = false,
        reason = "an explicit map exists, but no committed intertwiner identifies its image with the audited fundamental/adjoint determinant mode bases",
    },
    new
    {
        id = "phase3-connection-projector",
        evidencePath = supportingPaths[1],
        domain = "edge-major connection-one-form coefficients",
        codomain = "edge-major connection-one-form coefficients",
        compatibleWithPhase455DeterminantStateSpace = false,
        classificationAuthority = false,
        reason = "the projector is explicit in connection space only; Phase455's ray determinant does not consume it",
    },
    new
    {
        id = "phase4-fermion-projector-consumer",
        evidencePath = supportingPaths[4],
        domain = "supplied persisted fermion projector matrix",
        codomain = "projected explicit Dirac matrix",
        compatibleWithPhase455DeterminantStateSpace = false,
        classificationAuthority = false,
        reason = "the code validates and applies a supplied projector but does not derive one from a symmetry generator image",
    },
    new
    {
        id = "phase97-domain-separation",
        evidencePath = supportingPaths[5],
        domain = "connection quotient with identity fermion lift",
        codomain = "persisted fermion coefficients",
        compatibleWithPhase455DeterminantStateSpace = false,
        classificationAuthority = false,
        reason = "the committed artifact explicitly does not claim a nontrivial gauge quotient on fermion states",
    },
    new
    {
        id = "phase3-quotient-aware-scaffold",
        evidencePath = supportingPaths[2],
        domain = "Phase3 spectral bundle",
        codomain = "quotient-aware generalized eigenproblem",
        compatibleWithPhase455DeterminantStateSpace = false,
        classificationAuthority = false,
        reason = "the quotient-aware formulation throws NotSupportedException and is not an implemented quotient",
    },
};
int explicitGeneratorImageEvidenceCount = generatorProjectorEvidence.Count(row => row.id.Contains("generator-image", StringComparison.Ordinal));
int explicitProjectorOrConsumerEvidenceCount = generatorProjectorEvidence.Count(row => row.id.Contains("projector", StringComparison.Ordinal));
int compatibleGeneratorImageMapCount = generatorProjectorEvidence.Count(row => row.compatibleWithPhase455DeterminantStateSpace && row.id.Contains("generator-image", StringComparison.Ordinal));
int compatibleK0ProjectorCount = generatorProjectorEvidence.Count(row => row.compatibleWithPhase455DeterminantStateSpace && row.id.Contains("projector", StringComparison.Ordinal));
int generatorImageClassifiedDirectionCount = zeroModeSectors.Sum(row => row.generatorImageClassifiedDimension);
int exactButUnclassifiedZeroModeDirectionCountAtGeometricK0 = zeroModeSectors.Sum(row => row.exactKernelDimensionPerMomentumAtGenericNonzeroT);

var candidateTreatments = new[]
{
    new
    {
        id = "Za",
        treatment = "remove every vanishing-sine momentum block from both sectors",
        removesGeometricK0 = true,
        removesThreeAdditionalLatticeDoublerBlocks = true,
        removesNonzeroEigenvalueModesAtGenericNonzeroT = true,
        derivedFromCompatibleGeneratorImage = false,
        kind = "regulator-or-scheme-choice",
    },
    new
    {
        id = "Zb",
        treatment = "retain all momentum blocks and apply a relative soft floor in value evaluation",
        removesGeometricK0 = false,
        removesThreeAdditionalLatticeDoublerBlocks = false,
        removesNonzeroEigenvalueModesAtGenericNonzeroT = false,
        derivedFromCompatibleGeneratorImage = false,
        kind = "regulator-choice",
    },
    new
    {
        id = "Zc",
        treatment = "retain all momentum blocks and omit eigenvalues below the committed exact-zero tolerance",
        removesGeometricK0 = false,
        removesThreeAdditionalLatticeDoublerBlocks = false,
        removesNonzeroEigenvalueModesAtGenericNonzeroT = false,
        derivedFromCompatibleGeneratorImage = false,
        kind = "algebraic-prime-prescription-with-unclassified-kernel",
    },
};

bool inputsValid = allFilesPresent && summariesValid && sourceFormulaPinsValid && spectraReconstructed
    && vanishingSineMomenta.Count == 4 && geometricZeroMomentumCount == 1;
string quotientClassification = !inputsValid
    ? invalid
    : compatibleGeneratorImageMapCount > 0 && compatibleK0ProjectorCount > 0 && candidateTreatments.Count(row => row.derivedFromCompatibleGeneratorImage) == 1
        ? unique
        : compatibleGeneratorImageMapCount > 0 && candidateTreatments.Count(row => row.derivedFromCompatibleGeneratorImage) > 1
            ? family
            : underdetermined;
string terminalStatus = $"zero-mode-quotient-audit-{quotientClassification}";

var result = new
{
    phaseId = "phase490-zero-mode-quotient-audit",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    verdictKind = quotientClassification,
    quotientClassification,
    allowedQuotientClassifications = new[] { unique, family, underdetermined, invalid },
    decisionContract,
    decisionContractSha256 = Sha256(Encoding.UTF8.GetBytes(decisionContract)),
    applicationSubjectKind = "zero-mode-quotient-audit",
    planSection = "WAVE2_AMENDMENTS_2026-07-12 A7; PHASE455_CONVENTION_CLOSURE_PLAN_2026-07-15 Phase490",
    lane = "exploration",
    explorationOnly = true,
    inputsValid,
    inputValidation = new { allFilesPresent, summariesValid, summaryValidationDetail, sourceFormulaPinsValid, spectraReconstructed, maximumSpectrumResidual, tolerance },
    inputEvidenceCount = inputEvidence.Length,
    inputEvidence,
    reconstruction = new
    {
        independentlyConstructedGellMannHalfBasis = true,
        independentlyConstructedAdjointCommutatorGramOperator = true,
        axes,
        latticeSize,
        vanishingSineMomentumCount = vanishingSineMomenta.Count,
        vanishingSineMomenta,
        geometricZeroMomentumCount,
        latticeDoublerVanishingSineCount,
        genericBackgroundParameterCondition = "t is nonzero and lies below the committed first crossing",
    },
    zeroModeSectorCount = zeroModeSectors.Length,
    zeroModeSectors,
    generatorProjectorEvidenceCount = generatorProjectorEvidence.Length,
    generatorProjectorEvidence,
    explicitGeneratorImageEvidenceCount,
    explicitProjectorOrConsumerEvidenceCount,
    compatibleGeneratorImageMapCount,
    compatibleK0ProjectorCount,
    generatorImageClassifiedDirectionCount,
    exactButUnclassifiedZeroModeDirectionCountAtGeometricK0,
    directionCountAggregation = "sum over the nine audited axis-sector rows; T, D, and S are alternative ray axes, not simultaneous subspaces",
    regulatorChoiceCount = candidateTreatments.Length,
    candidateTreatments,
    uniqueAdmissibleQuotient = quotientClassification == unique,
    uniqueQuotientDerived = quotientClassification == unique,
    quotientFamilyDerived = quotientClassification == family,
    quotientUnderdetermined = quotientClassification == underdetermined,
    invalidCommittedInputs = quotientClassification == invalid,
    underdeterminationIsValidGreenScientificTerminal = inputsValid && quotientClassification == underdetermined,
    forceUniquenessAttempted = false,
    physicalOrGaugeStatusInferredFromNames = false,
    phase428ArtifactMutated = false,
    phase430ArtifactMutated = false,
    phase455ArtifactMutated = false,
    humanRulingAuthored = false,
    o4Discharged = false,
    satisfiesPhase458G3 = false,
    satisfiesPhase458G5 = false,
    phase458G3Satisfied = false,
    phase458G5Satisfied = false,
    phase458EvaluationAuthorized = false,
    binderLaunchAuthorized = false,
    samplingAuthorized = false,
    productionAuthorized = false,
    sourceContractApplicationAllowed = false,
    canFillPhase201WzContract = false,
    canFillPhase201HiggsContract = false,
    canFillPhase256ObservedFieldExtractionContract = false,
    routePromotesWzMasses = false,
    routePromotesHiggsMass = false,
    routeCompletesBosonPredictions = false,
    targetBlindConstruction = true,
    physicalTargetsConsultedForConstruction = false,
    noGevPromotion = true,
    promotedPhysicalMassClaimCount = 0,
    decision = quotientClassification switch
    {
        underdetermined => "The committed spectra determine exact k=0 and lattice-doubler kernels, but no committed generator image and compatible projector classify those determinant directions as gauge volume. Za, Zb, and Zc remain regulator or algebraic prescriptions rather than a derived unique quotient.",
        unique => "Exactly one quotient is derived from a compatible explicit generator image and projector.",
        family => "A non-unique quotient family is derived from compatible explicit generator-image evidence.",
        _ => "One or more required committed inputs is missing, malformed, drifted, or fails the independent spectrum reconstruction; no quotient result is available.",
    },
    limitations = new[]
    {
        "The audit classifies only the committed reduced ray determinant state spaces at generic nonzero t.",
        "The four vanishing-sine momentum tuples include three naive-lattice doubler tuples and must not all be called geometric k=0.",
        "Connection-space gauge projectors are not transferred to fermion or bosonic determinant modes without a committed intertwiner.",
        "Exact spectral nullity does not by itself determine physical, gauge-volume, collective-coordinate, or regulator status.",
    },
    runtimeSeconds = stopwatch.Elapsed.TotalSeconds,
};

Directory.CreateDirectory(outputDir);
var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
string json = JsonSerializer.Serialize(result, options);
File.WriteAllText(Path.Combine(outputDir, "zero_mode_quotient_audit.json"), json);
File.WriteAllText(Path.Combine(outputDir, "zero_mode_quotient_audit_summary.json"), json);

Require(result.allowedQuotientClassifications.Contains(quotientClassification, StringComparer.Ordinal), "taxonomy drifted");
Require(inputsValid, terminalStatus);
Require(quotientClassification == underdetermined && result.underdeterminationIsValidGreenScientificTerminal, "expected valid underdetermination terminal drifted");
Require(!result.humanRulingAuthored && !result.o4Discharged && !result.phase458EvaluationAuthorized && !result.productionAuthorized, "authority firewall failed");
Require(!result.sourceContractApplicationAllowed && result.noGevPromotion && result.promotedPhysicalMassClaimCount == 0, "claim firewall failed");
Console.WriteLine(terminalStatus);

static InputEvidence Evidence(string path, string role)
{
    bool present = File.Exists(path);
    byte[] bytes = present ? File.ReadAllBytes(path) : Array.Empty<byte>();
    return new InputEvidence(path, role, present, bytes.LongLength, present ? Sha256(bytes) : "missing");
}

static string Sha256(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

static bool Bool(JsonElement root, string name) => root.GetProperty(name).GetBoolean();

static string String(JsonElement root, string name) => root.GetProperty(name).GetString() ?? "";

static Complex[][,] GellMannHalfBasis()
{
    var matrices = Enumerable.Range(0, 8).Select(_ => new Complex[3, 3]).ToArray();
    SetSymmetric(matrices[0], 0, 1, Complex.One / 2.0);
    matrices[1][0, 1] = -Complex.ImaginaryOne / 2.0; matrices[1][1, 0] = Complex.ImaginaryOne / 2.0;
    matrices[2][0, 0] = 0.5; matrices[2][1, 1] = -0.5;
    SetSymmetric(matrices[3], 0, 2, Complex.One / 2.0);
    matrices[4][0, 2] = -Complex.ImaginaryOne / 2.0; matrices[4][2, 0] = Complex.ImaginaryOne / 2.0;
    SetSymmetric(matrices[5], 1, 2, Complex.One / 2.0);
    matrices[6][1, 2] = -Complex.ImaginaryOne / 2.0; matrices[6][2, 1] = Complex.ImaginaryOne / 2.0;
    double scale = 1.0 / (2.0 * Math.Sqrt(3.0));
    matrices[7][0, 0] = scale; matrices[7][1, 1] = scale; matrices[7][2, 2] = -2.0 * scale;
    return matrices;
}

static void SetSymmetric(Complex[,] matrix, int i, int j, Complex value)
{
    matrix[i, j] = value;
    matrix[j, i] = Complex.Conjugate(value);
}

static double[,] RealPart(Complex[,] matrix)
{
    int n = matrix.GetLength(0);
    var result = new double[n, n];
    for (int i = 0; i < n; i++)
    for (int j = 0; j < n; j++)
    {
        Require(Math.Abs(matrix[i, j].Imaginary) <= 1e-15, "selected ray matrix unexpectedly complex");
        result[i, j] = matrix[i, j].Real;
    }
    return result;
}

static double[,] AdjointSquared(Complex[,] axis, Complex[][,] basis)
{
    var commutators = basis.Select(generator => Commutator(axis, generator)).ToArray();
    var result = new double[basis.Length, basis.Length];
    for (int a = 0; a < basis.Length; a++)
    for (int b = 0; b < basis.Length; b++)
    {
        Complex inner = Complex.Zero;
        for (int i = 0; i < 3; i++)
        for (int j = 0; j < 3; j++) inner += Complex.Conjugate(commutators[a][i, j]) * commutators[b][i, j];
        Require(Math.Abs(inner.Imaginary) <= 1e-13, "adjoint Gram matrix is not real");
        result[a, b] = 2.0 * inner.Real;
    }
    return result;
}

static Complex[,] Commutator(Complex[,] a, Complex[,] b)
{
    var result = new Complex[3, 3];
    for (int i = 0; i < 3; i++)
    for (int j = 0; j < 3; j++)
    for (int k = 0; k < 3; k++) result[i, j] += a[i, k] * b[k, j] - b[i, k] * a[k, j];
    return result;
}

static double[] JacobiEigen(double[,] input)
{
    int n = input.GetLength(0);
    var matrix = (double[,])input.Clone();
    for (int iteration = 0; iteration < 100 * n * n; iteration++)
    {
        int p = 0, q = 1;
        double maximum = 0.0;
        for (int i = 0; i < n; i++)
        for (int j = i + 1; j < n; j++)
            if (Math.Abs(matrix[i, j]) > maximum) (maximum, p, q) = (Math.Abs(matrix[i, j]), i, j);
        if (maximum <= 1e-15) break;
        double angle = 0.5 * Math.Atan2(2.0 * matrix[p, q], matrix[q, q] - matrix[p, p]);
        double cosine = Math.Cos(angle), sine = Math.Sin(angle);
        double app = matrix[p, p], aqq = matrix[q, q], apq = matrix[p, q];
        for (int k = 0; k < n; k++)
        {
            if (k == p || k == q) continue;
            double mkp = matrix[k, p], mkq = matrix[k, q];
            matrix[k, p] = matrix[p, k] = cosine * mkp - sine * mkq;
            matrix[k, q] = matrix[q, k] = sine * mkp + cosine * mkq;
        }
        matrix[p, p] = cosine * cosine * app - 2.0 * sine * cosine * apq + sine * sine * aqq;
        matrix[q, q] = sine * sine * app + 2.0 * sine * cosine * apq + cosine * cosine * aqq;
        matrix[p, q] = matrix[q, p] = 0.0;
    }
    return Enumerable.Range(0, n).Select(i => Math.Abs(matrix[i, i]) <= 1e-14 ? 0.0 : matrix[i, i]).ToArray();
}

static double MaxResidual(double[] actual, double[] expected)
{
    Require(actual.Length == expected.Length, "spectrum length mismatch");
    return actual.Zip(expected, (a, e) => Math.Abs(a - e)).Max();
}

static void Require(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}

sealed class InputEvidence
{
    public InputEvidence(string path, string role, bool present, long byteCount, string sha256)
        => (Path, Role, Present, ByteCount, Sha256) = (path, role, present, byteCount, sha256);
    public string Path { get; }
    public string Role { get; }
    public bool Present { get; }
    public long ByteCount { get; }
    public string Sha256 { get; }
}

sealed class AxisDefinition
{
    public AxisDefinition(string id, Complex[,] matrix) => (Id, Matrix) = (id, matrix);
    public string Id { get; }
    public Complex[,] Matrix { get; }
}

sealed class AxisAudit
{
    public AxisAudit(string id, double[] fundamentalEigenvalues, double[] adjointEigenvaluesSquared,
        double fundamentalSpectrumResidual, double adjointSpectrumResidual, int fundamentalNullity,
        int adjointNullity, int fundamentalFermionKernelDimension, int adjointFermionKernelDimension,
        int bosonKernelDimension)
    {
        Id = id;
        FundamentalEigenvalues = fundamentalEigenvalues;
        AdjointEigenvaluesSquared = adjointEigenvaluesSquared;
        FundamentalSpectrumResidual = fundamentalSpectrumResidual;
        AdjointSpectrumResidual = adjointSpectrumResidual;
        FundamentalNullity = fundamentalNullity;
        AdjointNullity = adjointNullity;
        FundamentalFermionKernelDimension = fundamentalFermionKernelDimension;
        AdjointFermionKernelDimension = adjointFermionKernelDimension;
        BosonKernelDimension = bosonKernelDimension;
    }
    public string Id { get; }
    public double[] FundamentalEigenvalues { get; }
    public double[] AdjointEigenvaluesSquared { get; }
    public double FundamentalSpectrumResidual { get; }
    public double AdjointSpectrumResidual { get; }
    public int FundamentalNullity { get; }
    public int AdjointNullity { get; }
    public int FundamentalFermionKernelDimension { get; }
    public int AdjointFermionKernelDimension { get; }
    public int BosonKernelDimension { get; }
}
