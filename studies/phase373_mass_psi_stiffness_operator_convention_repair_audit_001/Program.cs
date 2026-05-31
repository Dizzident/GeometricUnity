using System.Text.Json;
using System.Text.Json.Serialization;
using Gu.Core;
using Gu.Geometry;
using Gu.Phase4.Couplings;
using Gu.Phase4.Dirac;
using Gu.Phase4.Fermions;
using Gu.Phase4.Spin;

const string DefaultOutputDir = "studies/phase373_mass_psi_stiffness_operator_convention_repair_audit_001/output";
const string Phase12Root = "studies/phase12_joined_calculation_001/output/background_family";
const string FermionDir = $"{Phase12Root}/fermions";
const string VariationDir = $"{FermionDir}/couplings/variations";
const string ModeDir = $"{Phase12Root}/spectra/modes";
const string SpinorRepresentationPath = $"{FermionDir}/spinor_representation.json";
const int ExpectedBackgroundCount = 2;
const int ExpectedVariationCount = 24;
const int ExpectedDirectionsPerVariation = 12;
const int ExpectedDirectionalCheckCount = ExpectedVariationCount * ExpectedDirectionsPerVariation;
const double AlgebraTolerance = 1e-11;
const double MatrixParityTolerance = 1e-8;
const double DirectionalParityTolerance = 1e-8;
const double CentralDerivativeTolerance = 1e-9;
const double ModeReplayResidualTolerance = 1e-6;
const double ModeReplayOrthonormalityTolerance = 1e-8;
double[] epsilonLadder = [1e-2, 1e-3, 1e-4, 1e-5, 1e-6];

var options = JsonOptions();
var outputDir = Environment.GetEnvironmentVariable("PHASE373_OUTPUT_DIR") ?? DefaultOutputDir;
var backgroundOutputDir = Path.Combine(outputDir, "backgrounds");
var variationOutputDir = Path.Combine(outputDir, "variations");
Directory.CreateDirectory(backgroundOutputDir);
Directory.CreateDirectory(variationOutputDir);

using var spinorDoc = JsonDocument.Parse(File.ReadAllText(SpinorRepresentationPath));
var spinorSpec = spinorDoc.RootElement.Deserialize<SpinorRepresentationSpec>(options)
    ?? throw new InvalidDataException($"Failed to deserialize {SpinorRepresentationPath}.");
var provenance = new ProvenanceMeta
{
    CreatedAt = DateTimeOffset.UtcNow,
    CodeRevision = "phase373-mass-psi-stiffness-operator-convention-repair-audit",
    Branch = new() { BranchId = "phase373-branch-local-convention-candidate", SchemaVersion = "1.0" },
    Backend = "cpu-reference",
};
var gammas = new GammaMatrixBuilder().Build(
    spinorSpec.CliffordSignature,
    spinorSpec.GammaConvention,
    provenance);
var mesh = ToyGeometryFactory.CreateStructuredFiberBundle2D(rows: 2, cols: 2).AmbientMesh;
int dimG = 3;
int spinorDim = spinorSpec.SpinorComponents;
int dofsPerVertex = spinorDim * dimG;
double[] massPsiWeights = MassPsiWeightsBuilder.BuildFromMesh(mesh, dofsPerVertex);
double[] edgeLengths = Enumerable.Range(0, mesh.EdgeCount).Select(edge => ComputeEdgeLength(mesh, edge)).ToArray();
double[][] edgeDirections = Enumerable.Range(0, mesh.EdgeCount).Select(edge => ComputeEdgeDirection(mesh, edge)).ToArray();
int[][] cellsPerEdge = Enumerable.Range(0, mesh.EdgeCount).Select(edge => new[] { mesh.Edges[edge][0], mesh.Edges[edge][1] }).ToArray();
var fermionModesByBackground = LoadFermionModesByBackground(FermionDir);

var backgroundAudits = fermionModesByBackground
    .OrderBy(pair => pair.Key, StringComparer.Ordinal)
    .Select(pair => BuildBackgroundAudit(pair.Key, pair.Value))
    .ToArray();
var variationAudits = Directory
    .GetFiles(VariationDir, "variation-*.json")
    .Where(path => !path.EndsWith(".matrix.json", StringComparison.Ordinal))
    .OrderBy(path => path, StringComparer.Ordinal)
    .Select(BuildVariationAudit)
    .ToArray();

int transformedBaseBackgroundCount = backgroundAudits.Length;
int transformedVariationCount = variationAudits.Length;
int transformedVariationIdentityPassedCount = variationAudits.Count(row => row.TransformedVariationIdentityPassed);
int transformedDirectionalCheckCount = variationAudits.Sum(row => row.DirectionalCheckCount);
int transformedDirectionalIdentityPassedCount = variationAudits.Sum(row => row.DirectionalIdentityPassedCount);
int transformedAnalyticPersistedParityPassedCount = variationAudits.Count(row => row.AnalyticPersistedParityPassed);
bool meshVolumeMassPsiMaterialized =
    massPsiWeights.Length > 0 &&
    massPsiWeights.All(weight => double.IsFinite(weight) && weight > 0.0);
bool stiffnessMatrixConventionCandidateMaterialized =
    backgroundAudits.All(row => row.BaseConvention.KIsEuclideanHermitian) &&
    variationAudits.All(row =>
        row.PersistedConvention.KIsEuclideanHermitian &&
        row.AnalyticConvention.KIsEuclideanHermitian);
bool weightedOperatorConventionCandidateMaterialized =
    backgroundAudits.All(row => row.BaseConvention.AIsMSelfAdjoint) &&
    variationAudits.All(row =>
        row.PersistedConvention.AIsMSelfAdjoint &&
        row.AnalyticConvention.AIsMSelfAdjoint);
bool symmetricRepresentativeConventionCandidateMaterialized =
    backgroundAudits.All(row => row.BaseConvention.BIsEuclideanHermitian && row.BaseConvention.BSimilarityIdentityPassed) &&
    variationAudits.All(row =>
        row.PersistedConvention.BIsEuclideanHermitian &&
        row.PersistedConvention.BSimilarityIdentityPassed &&
        row.AnalyticConvention.BIsEuclideanHermitian &&
        row.AnalyticConvention.BSimilarityIdentityPassed);
bool phase372MeshVolumeWeightObstructionPresent =
    backgroundAudits.All(row => !row.BaseConvention.RawKMisreadAsOperatorIsMSelfAdjoint) &&
    variationAudits.Any(row =>
        !row.PersistedConvention.RawKMisreadAsOperatorIsMSelfAdjoint ||
        !row.AnalyticConvention.RawKMisreadAsOperatorIsMSelfAdjoint);
bool matchingWeightedModeReplayMaterialized =
    backgroundAudits.Length == ExpectedBackgroundCount &&
    backgroundAudits.All(row => row.WeightedModeReplay.Materialized);
bool matchingWeightedModeReplayQualityPassed =
    matchingWeightedModeReplayMaterialized &&
    backgroundAudits.All(row => row.WeightedModeReplay.QualityPassed);
string? matchingWeightedModeReplaySolverQualityObstruction = matchingWeightedModeReplayQualityPassed
    ? null
    : "The existing FermionSpectralSolver synthetic-B diagnostic replay yields weak residuals or M-orthonormality. The algebraic K/A/B convention checks remain independently valid; shared solver repair is outside this branch-local study.";

const bool routeProvidesPhysicalMassPsiCompatibleBranch = false;
const bool routeProvidesSharedSolverRepair = false;
const bool routeProvidesCompletedFermionicAction = false;
const bool routeProvidesFixedFermionicOperatorBranch = false;
const bool routeProvidesExplicitYukawaFunctional = false;
const bool routeProvidesSolvedYukawaCouplingMap = false;
const bool routeProvidesCoupledResidual = false;
const bool routeProvidesCompletedMixedLinearizationBlocks = false;
const bool routeProvidesMixedLinearizationGaugeCompatibilityIdentities = false;
const bool routeProvidesDirectTargetIndependentWzBridgeSourceLaw = false;
const bool routeProvidesHiggsScalarSourceOperator = false;
const bool routeProvidesScalarProjectionTheorem = false;
const bool routeProvidesGeVUnitNormalization = false;
const bool routePromotesWzMasses = false;
const bool routePromotesHiggsMass = false;
const bool routeCompletesBosonPredictions = false;
const bool canFillPhase201WzContract = false;
const bool canFillPhase201HiggsContract = false;
const bool canFillPhase256ObservedFieldExtractionContract = false;

bool massPsiStiffnessOperatorConventionRepairAuditPassed =
    transformedBaseBackgroundCount == ExpectedBackgroundCount &&
    transformedVariationCount == ExpectedVariationCount &&
    transformedVariationIdentityPassedCount == ExpectedVariationCount &&
    transformedDirectionalCheckCount == ExpectedDirectionalCheckCount &&
    transformedDirectionalIdentityPassedCount == ExpectedDirectionalCheckCount &&
    transformedAnalyticPersistedParityPassedCount == ExpectedVariationCount &&
    phase372MeshVolumeWeightObstructionPresent &&
    meshVolumeMassPsiMaterialized &&
    stiffnessMatrixConventionCandidateMaterialized &&
    weightedOperatorConventionCandidateMaterialized &&
    symmetricRepresentativeConventionCandidateMaterialized &&
    matchingWeightedModeReplayMaterialized &&
    !routeProvidesPhysicalMassPsiCompatibleBranch &&
    !routeProvidesSharedSolverRepair &&
    !routeProvidesCompletedFermionicAction &&
    !routeProvidesFixedFermionicOperatorBranch &&
    !routeProvidesExplicitYukawaFunctional &&
    !routeProvidesSolvedYukawaCouplingMap &&
    !routeProvidesCoupledResidual &&
    !routeProvidesCompletedMixedLinearizationBlocks &&
    !routeProvidesMixedLinearizationGaugeCompatibilityIdentities &&
    !routeProvidesDirectTargetIndependentWzBridgeSourceLaw &&
    !routeProvidesHiggsScalarSourceOperator &&
    !routeProvidesScalarProjectionTheorem &&
    !routeProvidesGeVUnitNormalization &&
    !routePromotesWzMasses &&
    !routePromotesHiggsMass &&
    !routeCompletesBosonPredictions &&
    !canFillPhase201WzContract &&
    !canFillPhase201HiggsContract &&
    !canFillPhase256ObservedFieldExtractionContract;

string terminalStatus = massPsiStiffnessOperatorConventionRepairAuditPassed
    ? "branch-local-mass-psi-stiffness-operator-convention-candidate-materialized"
    : "branch-local-mass-psi-stiffness-operator-convention-repair-audit-blocked";
string decision = massPsiStiffnessOperatorConventionRepairAuditPassed
    ? "Treating the persisted Euclidean-Hermitian assembled Dirac matrices as stiffness/action matrices K repairs the Phase372 mesh-volume M_psi algebra locally: A=M_psi^-1 K is M-self-adjoint and B=M_psi^-1/2 K M_psi^-1/2 is Euclidean Hermitian across both backgrounds and all 24 variations. This is a branch-local convention candidate only. The matching synthetic-B mode replay is diagnostic and does not prove a physical GU/local branch contract or a shared solver repair."
    : "Do not promote the branch-local convention candidate until the K/A/B algebra, transformed variation parity, and all 288 transformed directional checks pass.";

var predictionContractImpact = new
{
    canFillPhase201WzContract,
    canFillPhase201HiggsContract,
    canFillPhase256ObservedFieldExtractionContract,
    phase201FieldsDefensiblyFilled = Array.Empty<string>(),
};
var result = new
{
    phaseId = "phase373-mass-psi-stiffness-operator-convention-repair-audit",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    massPsiStiffnessOperatorConventionRepairAuditPassed,
    phase372MeshVolumeWeightObstructionPresent,
    meshVolumeMassPsiMaterialized,
    stiffnessMatrixConventionCandidateMaterialized,
    weightedOperatorConventionCandidateMaterialized,
    symmetricRepresentativeConventionCandidateMaterialized,
    transformedBaseBackgroundCount,
    transformedVariationCount,
    transformedVariationIdentityPassedCount,
    transformedDirectionalCheckCount,
    transformedDirectionalIdentityPassedCount,
    transformedAnalyticPersistedParityPassedCount,
    matchingWeightedModeReplayMaterialized,
    matchingWeightedModeReplayQualityPassed,
    matchingWeightedModeReplaySolverQualityObstruction,
    implementedObjectClassification = "branch-local stiffness/operator convention candidate only",
    conventionDefinitions = new
    {
        stiffnessActionMatrix = "K = persisted Euclidean-Hermitian assembled Dirac matrix",
        weightedOperator = "A = M_psi^-1 K",
        symmetricRepresentative = "B = M_psi^-1/2 K M_psi^-1/2",
        similarityIdentity = "B = M_psi^1/2 A M_psi^-1/2",
        weightedPairingIdentity = "<chi, A psi>_M = chi^dagger K psi",
        matchingModeReplay = "solve B u = lambda u with MassPsiWeights=null, then psi=M_psi^-1/2 u and M-normalize",
    },
    tolerances = new
    {
        algebraTolerance = AlgebraTolerance,
        matrixParityTolerance = MatrixParityTolerance,
        directionalParityTolerance = DirectionalParityTolerance,
        centralDerivativeTolerance = CentralDerivativeTolerance,
        modeReplayResidualTolerance = ModeReplayResidualTolerance,
        modeReplayOrthonormalityTolerance = ModeReplayOrthonormalityTolerance,
        epsilonLadder,
    },
    meshVolumeMassPsi = new
    {
        builder = "MassPsiWeightsBuilder.BuildFromMesh(mesh, spinorDim * dimG)",
        realWeightCount = massPsiWeights.Length,
        minimumWeight = massPsiWeights.Min(),
        maximumWeight = massPsiWeights.Max(),
        distinctWeights = massPsiWeights.Distinct().Order().ToArray(),
    },
    routeProvidesPhysicalMassPsiCompatibleBranch,
    routeProvidesSharedSolverRepair,
    routeProvidesCompletedFermionicAction,
    routeProvidesFixedFermionicOperatorBranch,
    routeProvidesExplicitYukawaFunctional,
    routeProvidesSolvedYukawaCouplingMap,
    routeProvidesCoupledResidual,
    routeProvidesCompletedMixedLinearizationBlocks,
    routeProvidesMixedLinearizationGaugeCompatibilityIdentities,
    routeProvidesDirectTargetIndependentWzBridgeSourceLaw,
    routeProvidesHiggsScalarSourceOperator,
    routeProvidesScalarProjectionTheorem,
    routeProvidesGeVUnitNormalization,
    routePromotesWzMasses,
    routePromotesHiggsMass,
    routeCompletesBosonPredictions,
    canFillPhase201WzContract,
    canFillPhase201HiggsContract,
    canFillPhase256ObservedFieldExtractionContract,
    predictionContractImpact,
    explicitCandidateOnlyNonclaims = new[]
    {
        "branch-local convention candidate only",
        "no GU/local physical M_psi branch contract proven",
        "no shared solver repair",
        "no completed fermionic action",
        "no fixed fermionic operator branch",
        "no explicit Yukawa functional or solved coupling map",
        "no coupled residual or completed mixed linearization blocks",
        "no gauge compatibility identities",
        "no target-independent W/Z bridge source law",
        "no Higgs scalar source operator or scalar projection theorem",
        "no GeV unit normalization",
        "no promoted boson predictions",
        "no Phase201 or Phase256 contract fill",
    },
    backgroundAudits,
    variationAudits,
    sourceEvidence = new
    {
        phase12Root = Phase12Root,
        fermionDir = FermionDir,
        variationDir = VariationDir,
        modeDir = ModeDir,
        spinorRepresentationPath = SpinorRepresentationPath,
    },
    decision,
};

string fullPath = Path.Combine(outputDir, "mass_psi_stiffness_operator_convention_repair_audit.json");
string summaryPath = Path.Combine(outputDir, "mass_psi_stiffness_operator_convention_repair_audit_summary.json");
File.WriteAllText(fullPath, JsonSerializer.Serialize(result, options));
File.WriteAllText(summaryPath, JsonSerializer.Serialize(new
{
    result.phaseId,
    result.generatedAt,
    terminalStatus,
    massPsiStiffnessOperatorConventionRepairAuditPassed,
    phase372MeshVolumeWeightObstructionPresent,
    meshVolumeMassPsiMaterialized,
    stiffnessMatrixConventionCandidateMaterialized,
    weightedOperatorConventionCandidateMaterialized,
    symmetricRepresentativeConventionCandidateMaterialized,
    transformedBaseBackgroundCount,
    transformedVariationCount,
    transformedVariationIdentityPassedCount,
    transformedDirectionalCheckCount,
    transformedDirectionalIdentityPassedCount,
    transformedAnalyticPersistedParityPassedCount,
    matchingWeightedModeReplayMaterialized,
    matchingWeightedModeReplayQualityPassed,
    matchingWeightedModeReplaySolverQualityObstruction,
    result.implementedObjectClassification,
    result.meshVolumeMassPsi,
    maxBaseRawKMAdjointRelativeResidual = backgroundAudits.Max(row => row.BaseConvention.RawKMisreadAsOperatorMAdjointRelativeResidual),
    maxBaseAWeightedAdjointRelativeResidual = backgroundAudits.Max(row => row.BaseConvention.AMAdjointRelativeResidual),
    maxBaseBHermiticityRelativeResidual = backgroundAudits.Max(row => row.BaseConvention.BEuclideanHermiticityRelativeResidual),
    maxVariationAnalyticPersistedRelativeResidual = variationAudits.Max(row => row.AnalyticPersistedRelativeResidual),
    maxDirectionalPairingIdentityScaleAwareResidual = variationAudits.Max(row => row.MaxDirectionalPairingIdentityScaleAwareResidual),
    maxDirectionalCentralDerivativeScaleAwareResidual = variationAudits.Max(row => row.MaxDirectionalCentralDerivativeScaleAwareResidual),
    maxModeReplayBRelativeResidual = backgroundAudits.Max(row => row.WeightedModeReplay.MaxBRelativeResidual),
    maxModeReplayGeneralizedRelativeResidual = backgroundAudits.Max(row => row.WeightedModeReplay.MaxGeneralizedRelativeResidual),
    maxModeReplayMOrthonormalityResidual = backgroundAudits.Max(row => row.WeightedModeReplay.MaxMOrthonormalityResidual),
    routeProvidesPhysicalMassPsiCompatibleBranch,
    routeProvidesSharedSolverRepair,
    routeProvidesCompletedFermionicAction,
    routeProvidesFixedFermionicOperatorBranch,
    routeProvidesExplicitYukawaFunctional,
    routeProvidesSolvedYukawaCouplingMap,
    routeProvidesCoupledResidual,
    routeProvidesCompletedMixedLinearizationBlocks,
    routeProvidesMixedLinearizationGaugeCompatibilityIdentities,
    routeProvidesDirectTargetIndependentWzBridgeSourceLaw,
    routeProvidesHiggsScalarSourceOperator,
    routeProvidesScalarProjectionTheorem,
    routeProvidesGeVUnitNormalization,
    routePromotesWzMasses,
    routePromotesHiggsMass,
    routeCompletesBosonPredictions,
    canFillPhase201WzContract,
    canFillPhase201HiggsContract,
    canFillPhase256ObservedFieldExtractionContract,
    predictionContractImpact,
    result.explicitCandidateOnlyNonclaims,
    decision,
}, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"massPsiStiffnessOperatorConventionRepairAuditPassed={massPsiStiffnessOperatorConventionRepairAuditPassed}");
Console.WriteLine($"phase372MeshVolumeWeightObstructionPresent={phase372MeshVolumeWeightObstructionPresent}");
Console.WriteLine($"transformedBaseBackgroundCount={transformedBaseBackgroundCount}");
Console.WriteLine($"transformedVariationIdentityPassedCount={transformedVariationIdentityPassedCount}/{transformedVariationCount}");
Console.WriteLine($"transformedDirectionalIdentityPassedCount={transformedDirectionalIdentityPassedCount}/{transformedDirectionalCheckCount}");
Console.WriteLine($"transformedAnalyticPersistedParityPassedCount={transformedAnalyticPersistedParityPassedCount}/{transformedVariationCount}");
Console.WriteLine($"matchingWeightedModeReplayQualityPassed={matchingWeightedModeReplayQualityPassed}");
Console.WriteLine($"matchingWeightedModeReplaySolverQualityObstruction={matchingWeightedModeReplaySolverQualityObstruction}");
Console.WriteLine($"summaryPath={summaryPath}");

BackgroundAudit BuildBackgroundAudit(string backgroundId, IReadOnlyList<FermionModeSnapshot> modes)
{
    string metadataPath = Path.Combine(FermionDir, $"dirac_bundle_{backgroundId}.json");
    using var metadataDoc = JsonDocument.Parse(File.ReadAllText(metadataPath));
    var metadata = metadataDoc.RootElement;
    int matrixSize = SquareMatrixSize(metadata);
    string matrixPath = Path.Combine(FermionDir, RequiredString(metadata, "explicitMatrixRef"));
    var k = LoadFlatInterleavedMatrix(matrixPath, matrixSize);
    var convention = BuildConventionAudit(k);
    var replay = RunMatchingWeightedModeReplay(backgroundId, metadata, k, convention.B);
    string sidecarPath = Path.Combine(backgroundOutputDir, $"{backgroundId}-stiffness-operator-convention.json");
    var audit = new BackgroundAudit
    {
        FermionBackgroundId = backgroundId,
        BaseDiracMetadataPath = metadataPath,
        BaseDiracMatrixPath = matrixPath,
        PersistedSourceModeId = modes.OrderBy(mode => mode.ModeIndex).First().ModeId,
        PersistedDirectionCount = modes.Count,
        BaseConvention = convention.Record,
        WeightedModeReplay = replay,
        SidecarPath = sidecarPath,
    };
    File.WriteAllText(sidecarPath, JsonSerializer.Serialize(audit, options));
    return audit;
}

VariationAudit BuildVariationAudit(string metadataPath)
{
    using var metadataDoc = JsonDocument.Parse(File.ReadAllText(metadataPath));
    var metadata = metadataDoc.RootElement;
    string variationId = RequiredString(metadata, "variationId");
    string backgroundId = RequiredString(metadata, "fermionBackgroundId");
    string bosonModeId = RequiredString(metadata, "bosonModeId");
    string matrixPath = Path.Combine(VariationDir, RequiredString(metadata, "matrixArtifactRef"));
    string modePath = Path.Combine(ModeDir, $"{bosonModeId}.json");
    double[] modeVector = LoadModeVector(modePath);
    var persistedK = LoadNestedMatrix(matrixPath);
    var (analyticRe, analyticIm) = DiracVariationComputer.ComputeAnalytical(
        modeVector,
        gammas,
        mesh.VertexCount,
        spinorDim,
        dimG,
        edgeLengths,
        cellsPerEdge,
        edgeDirections);
    var analyticK = new ComplexMatrix(analyticRe, analyticIm);
    var persistedConvention = BuildConventionAudit(persistedK);
    var analyticConvention = BuildConventionAudit(analyticK);
    double analyticPersistedRelativeResidual = MatrixRelativeResidual(persistedK, analyticK);
    bool analyticPersistedParityPassed = analyticPersistedRelativeResidual <= MatrixParityTolerance;
    if (!fermionModesByBackground.TryGetValue(backgroundId, out var modes))
        throw new InvalidDataException($"Missing persisted modes for {backgroundId}.");
    var source = modes.OrderBy(mode => mode.ModeIndex).First();
    var directions = modes
        .OrderBy(mode => mode.ModeIndex)
        .Select(mode => BuildDirectionalAudit(source, mode, persistedConvention, analyticConvention))
        .ToArray();
    int directionalIdentityPassedCount = directions.Count(row => row.TransformedDirectionalIdentityPassed);
    bool transformedVariationIdentityPassed =
        persistedConvention.Record.AllTransformedIdentitiesPassed &&
        analyticConvention.Record.AllTransformedIdentitiesPassed &&
        analyticPersistedParityPassed &&
        directions.Length == ExpectedDirectionsPerVariation &&
        directionalIdentityPassedCount == ExpectedDirectionsPerVariation;
    string sidecarPath = Path.Combine(variationOutputDir, $"{variationId}-stiffness-operator-convention.json");
    var audit = new VariationAudit
    {
        VariationId = variationId,
        FermionBackgroundId = backgroundId,
        BosonModeId = bosonModeId,
        PersistedVariationMetadataPath = metadataPath,
        PersistedVariationMatrixPath = matrixPath,
        AnalyticVariationSource = "Gu.Phase4.Couplings.DiracVariationComputer.ComputeAnalytical",
        PersistedConvention = persistedConvention.Record,
        AnalyticConvention = analyticConvention.Record,
        AnalyticPersistedRelativeResidual = analyticPersistedRelativeResidual,
        AnalyticPersistedParityPassed = analyticPersistedParityPassed,
        DirectionalCheckCount = directions.Length,
        DirectionalIdentityPassedCount = directionalIdentityPassedCount,
        MaxDirectionalPairingIdentityScaleAwareResidual = directions.Max(row => row.MaxPairingIdentityScaleAwareResidual),
        MaxDirectionalCentralDerivativeScaleAwareResidual = directions.Max(row => row.MaxCentralDerivativeScaleAwareResidual),
        TransformedVariationIdentityPassed = transformedVariationIdentityPassed,
        Directions = directions,
        SidecarPath = sidecarPath,
    };
    File.WriteAllText(sidecarPath, JsonSerializer.Serialize(audit, options));
    return audit;
}

ConventionAudit BuildConventionAudit(ComplexMatrix k)
{
    var a = k.ScaleRows(massPsiWeights, inverseSquareRoot: false);
    var b = k.ScaleSymmetric(massPsiWeights);
    var similarB = a.ScaleSimilarity(massPsiWeights);
    double kHermiticity = EuclideanHermiticityResidual(k);
    double rawKMAdjoint = MAdjointResidual(k, massPsiWeights);
    double aMAdjoint = MAdjointResidual(a, massPsiWeights);
    double bHermiticity = EuclideanHermiticityResidual(b);
    double bSimilarity = MatrixRelativeResidual(b, similarB);
    var record = new ConventionAuditRecord
    {
        KIsEuclideanHermitian = kHermiticity <= AlgebraTolerance,
        KEuclideanHermiticityRelativeResidual = kHermiticity,
        RawKMisreadAsOperatorIsMSelfAdjoint = rawKMAdjoint <= AlgebraTolerance,
        RawKMisreadAsOperatorMAdjointRelativeResidual = rawKMAdjoint,
        AIsMSelfAdjoint = aMAdjoint <= AlgebraTolerance,
        AMAdjointRelativeResidual = aMAdjoint,
        BIsEuclideanHermitian = bHermiticity <= AlgebraTolerance,
        BEuclideanHermiticityRelativeResidual = bHermiticity,
        BSimilarityIdentityPassed = bSimilarity <= AlgebraTolerance,
        BSimilarityIdentityRelativeResidual = bSimilarity,
        AllTransformedIdentitiesPassed =
            kHermiticity <= AlgebraTolerance &&
            aMAdjoint <= AlgebraTolerance &&
            bHermiticity <= AlgebraTolerance &&
            bSimilarity <= AlgebraTolerance,
    };
    return new ConventionAudit(k, a, b, record);
}

DirectionalAudit BuildDirectionalAudit(
    FermionModeSnapshot source,
    FermionModeSnapshot direction,
    ConventionAudit persisted,
    ConventionAudit analytic)
{
    var persistedDiagnostic = BuildDirectionalMatrixDiagnostic(persisted.K, persisted.A, source.EigenvectorCoefficients, direction.EigenvectorCoefficients);
    var analyticDiagnostic = BuildDirectionalMatrixDiagnostic(analytic.K, analytic.A, source.EigenvectorCoefficients, direction.EigenvectorCoefficients);
    double pairingParity = ComplexScaleAwareResidual(
        persistedDiagnostic.WeightedOperatorPairing,
        analyticDiagnostic.WeightedOperatorPairing);
    double derivativeParity = ScaleAwareResidual(
        persistedDiagnostic.ReciprocalRealFormDirectionalDerivative,
        analyticDiagnostic.ReciprocalRealFormDirectionalDerivative);
    bool analyticPersistedDirectionalParityPassed =
        pairingParity <= DirectionalParityTolerance &&
        derivativeParity <= DirectionalParityTolerance;
    return new()
    {
        ChiModeId = direction.ModeId,
        ChiModeIndex = direction.ModeIndex,
        PsiSourceModeId = source.ModeId,
        PsiSourceModeIndex = source.ModeIndex,
        Persisted = persistedDiagnostic,
        Analytic = analyticDiagnostic,
        AnalyticPersistedPairingScaleAwareResidual = pairingParity,
        AnalyticPersistedDerivativeScaleAwareResidual = derivativeParity,
        AnalyticPersistedDirectionalParityPassed = analyticPersistedDirectionalParityPassed,
        MaxPairingIdentityScaleAwareResidual = Math.Max(
            persistedDiagnostic.WeightedOperatorToStiffnessPairingScaleAwareResidual,
            analyticDiagnostic.WeightedOperatorToStiffnessPairingScaleAwareResidual),
        MaxCentralDerivativeScaleAwareResidual = Math.Max(
            persistedDiagnostic.MaxCentralDerivativeScaleAwareResidual,
            analyticDiagnostic.MaxCentralDerivativeScaleAwareResidual),
        TransformedDirectionalIdentityPassed =
            persistedDiagnostic.AllDirectionalIdentitiesPassed &&
            analyticDiagnostic.AllDirectionalIdentitiesPassed &&
            analyticPersistedDirectionalParityPassed,
    };
}

DirectionalMatrixDiagnostic BuildDirectionalMatrixDiagnostic(
    ComplexMatrix k,
    ComplexMatrix a,
    double[] psi,
    double[] chi)
{
    var aPsi = a.Apply(psi);
    var aChi = a.Apply(chi);
    var weightedPairing = ComplexInnerProduct(chi, aPsi, massPsiWeights);
    var stiffnessPairing = ComplexInnerProduct(chi, k.Apply(psi), null);
    var reciprocalPartner = ComplexInnerProduct(psi, aChi, massPsiWeights);
    double reciprocalDerivative = weightedPairing.Real + reciprocalPartner.Real;
    double realFormShortcut = 2.0 * weightedPairing.Real;
    double pairingResidual = ComplexScaleAwareResidual(weightedPairing, stiffnessPairing);
    double reciprocalResidual = ScaleAwareResidual(realFormShortcut, reciprocalDerivative);
    var ladder = epsilonLadder.Select(epsilon =>
    {
        double plus = WeightedActionValue(a, AddScaled(psi, chi, epsilon), massPsiWeights);
        double minus = WeightedActionValue(a, AddScaled(psi, chi, -epsilon), massPsiWeights);
        double derivative = (plus - minus) / (2.0 * epsilon);
        double residual = ScaleAwareResidual(reciprocalDerivative, derivative);
        return new CentralDerivativeRow
        {
            Epsilon = epsilon,
            ActionAtPsiPlusEpsilonChi = plus,
            ActionAtPsiMinusEpsilonChi = minus,
            CentralDerivative = derivative,
            ExpectedReciprocalDirectionalDerivative = reciprocalDerivative,
            ScaleAwareResidual = residual,
            Passed = residual <= CentralDerivativeTolerance,
        };
    }).ToArray();
    double maxCentralResidual = ladder.Max(row => row.ScaleAwareResidual);
    return new()
    {
        WeightedOperatorPairing = weightedPairing,
        StiffnessPairing = stiffnessPairing,
        WeightedOperatorToStiffnessPairingScaleAwareResidual = pairingResidual,
        WeightedOperatorToStiffnessPairingIdentityPassed = pairingResidual <= AlgebraTolerance,
        ReciprocalPartnerPairing = reciprocalPartner,
        ReciprocalRealFormDirectionalDerivative = reciprocalDerivative,
        HermitianRealFormShortcutDirectionalDerivative = realFormShortcut,
        ReciprocalRealFormScaleAwareResidual = reciprocalResidual,
        ReciprocalRealFormIdentityPassed = reciprocalResidual <= AlgebraTolerance,
        CentralDerivativeLadder = ladder,
        MaxCentralDerivativeScaleAwareResidual = maxCentralResidual,
        CentralDerivativeIdentityPassed = ladder.All(row => row.Passed),
        AllDirectionalIdentitiesPassed =
            pairingResidual <= AlgebraTolerance &&
            reciprocalResidual <= AlgebraTolerance &&
            ladder.All(row => row.Passed),
    };
}

WeightedModeReplay RunMatchingWeightedModeReplay(
    string backgroundId,
    JsonElement metadata,
    ComplexMatrix k,
    ComplexMatrix b)
{
    string layoutPath = Path.Combine(FermionDir, $"layout_{backgroundId}.json");
    var layout = JsonSerializer.Deserialize<FermionFieldLayout>(File.ReadAllText(layoutPath), options)
        ?? throw new InvalidDataException($"Failed to deserialize {layoutPath}.");
    var bundle = new DiracOperatorBundle
    {
        OperatorId = $"phase373-synthetic-b-{backgroundId}",
        FermionBackgroundId = backgroundId,
        LayoutId = layout.LayoutId,
        SpinConnectionId = $"phase373-synthetic-b-{backgroundId}",
        MatrixShape = [b.Size, b.Size],
        HasExplicitMatrix = true,
        ExplicitMatrix = b.ToFlatInterleaved(),
        ExplicitMatrixRef = null,
        IsHermitian = EuclideanHermiticityResidual(b) <= AlgebraTolerance,
        HermiticityResidual = EuclideanHermiticityResidual(b),
        HermiticityTolerance = AlgebraTolerance,
        MassBranchTermIncluded = metadata.GetProperty("massBranchTermIncluded").GetBoolean(),
        CorrectionTermIncluded = metadata.GetProperty("correctionTermIncluded").GetBoolean(),
        GaugeReductionApplied = metadata.GetProperty("gaugeReductionApplied").GetBoolean(),
        CellCount = metadata.GetProperty("cellCount").GetInt32(),
        DofsPerCell = metadata.GetProperty("dofsPerCell").GetInt32(),
        DiagnosticNotes = ["Phase373 synthetic explicit B bundle for branch-local weighted-mode replay."],
        Provenance = provenance,
    };
    var solver = new FermionSpectralSolver(new CpuDiracOperatorAssembler());
    var solve = solver.Solve(
        bundle,
        layout,
        new()
        {
            TargetRegion = "lowest-magnitude",
            ModeCount = ExpectedDirectionsPerVariation,
            GaugeReduction = false,
            NullspaceDeflation = false,
            ConvergenceTolerance = 1e-10,
            MaxIterations = 1000,
            Seed = 42,
            MassPsiWeights = null,
        },
        provenance);
    var rows = solve.Modes.Select(mode =>
    {
        double[] u = NormalizeEuclidean(mode.EigenvectorCoefficients
            ?? throw new InvalidDataException($"Solver mode {mode.ModeId} has no eigenvector coefficients."));
        double[] psi = NormalizeM(ScaleVectorByInverseSqrtWeights(u, massPsiWeights), massPsiWeights);
        double lambda = mode.EigenvalueRe;
        var bu = b.Apply(u);
        var kPsi = k.Apply(psi);
        var lambdaU = Scale(u, lambda);
        var lambdaMPsi = Scale(ApplyWeights(psi, massPsiWeights), lambda);
        return new ModeReplayRow
        {
            ModeId = mode.ModeId,
            ModeIndex = mode.ModeIndex,
            Eigenvalue = lambda,
            SolverReportedResidualNorm = mode.ResidualNorm,
            BAbsoluteResidual = EuclideanNorm(Subtract(bu, lambdaU)),
            BRelativeResidual = RelativeVectorResidual(bu, lambdaU),
            GeneralizedKPsiEqualsLambdaMPsiAbsoluteResidual = EuclideanNorm(Subtract(kPsi, lambdaMPsi)),
            GeneralizedKPsiEqualsLambdaMPsiRelativeResidual = RelativeVectorResidual(kPsi, lambdaMPsi),
            PsiMNorm = Math.Sqrt(ComplexInnerProduct(psi, psi, massPsiWeights).Real),
            U = u,
            Psi = psi,
        };
    }).ToArray();
    double maxOrthResidual = 0.0;
    for (int left = 0; left < rows.Length; left++)
        for (int right = 0; right < rows.Length; right++)
        {
            var value = ComplexInnerProduct(rows[left].Psi, rows[right].Psi, massPsiWeights);
            double expected = left == right ? 1.0 : 0.0;
            maxOrthResidual = Math.Max(maxOrthResidual, Math.Sqrt((value.Real - expected) * (value.Real - expected) + value.Imaginary * value.Imaginary));
        }
    double maxBResidual = rows.Max(row => row.BRelativeResidual);
    double maxGeneralizedResidual = rows.Max(row => row.GeneralizedKPsiEqualsLambdaMPsiRelativeResidual);
    bool qualityPassed =
        maxBResidual <= ModeReplayResidualTolerance &&
        maxGeneralizedResidual <= ModeReplayResidualTolerance &&
        maxOrthResidual <= ModeReplayOrthonormalityTolerance;
    return new()
    {
        Materialized = true,
        SyntheticBundleConvention = "explicit B bundle with FermionSpectralConfig.MassPsiWeights=null",
        BackTransformConvention = "psi=M_psi^-1/2 u followed by M-normalization",
        SolverName = solve.Diagnostics.SolverName,
        SolverConverged = solve.Diagnostics.Converged,
        SolverIterations = solve.Diagnostics.Iterations,
        ModeCount = rows.Length,
        MaxBRelativeResidual = maxBResidual,
        MaxGeneralizedRelativeResidual = maxGeneralizedResidual,
        MaxMOrthonormalityResidual = maxOrthResidual,
        QualityPassed = qualityPassed,
        SolverQualityObstruction = qualityPassed
            ? null
            : "Synthetic-B replay does not satisfy strict residual or M-orthonormality tolerance with the existing shared solver.",
        Modes = rows,
    };
}

static JsonSerializerOptions JsonOptions() => new()
{
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true,
};

static int SquareMatrixSize(JsonElement metadata)
{
    int[] shape = metadata.GetProperty("matrixShape").EnumerateArray().Select(value => value.GetInt32()).ToArray();
    if (shape.Length != 2 || shape[0] != shape[1])
        throw new InvalidDataException("Expected a square matrix shape.");
    return shape[0];
}

static string RequiredString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
        ? value.GetString() ?? throw new InvalidDataException($"{propertyName} must not be null.")
        : throw new InvalidDataException($"{propertyName} must be a string.");

static IReadOnlyDictionary<string, IReadOnlyList<FermionModeSnapshot>> LoadFermionModesByBackground(string fermionDir)
{
    var result = new Dictionary<string, IReadOnlyList<FermionModeSnapshot>>(StringComparer.Ordinal);
    foreach (string path in Directory.GetFiles(fermionDir, "fermion_modes_*.json").OrderBy(path => path, StringComparer.Ordinal))
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        string backgroundId = RequiredString(doc.RootElement, "fermionBackgroundId");
        result.Add(backgroundId, doc.RootElement.GetProperty("modes").EnumerateArray().Select(mode => new FermionModeSnapshot
        {
            ModeId = RequiredString(mode, "modeId"),
            ModeIndex = mode.GetProperty("modeIndex").GetInt32(),
            EigenvectorCoefficients = mode.GetProperty("eigenvectorCoefficients").EnumerateArray().Select(value => value.GetDouble()).ToArray(),
        }).ToArray());
    }
    return result;
}

static double[] LoadModeVector(string path)
{
    using var doc = JsonDocument.Parse(File.ReadAllText(path));
    return doc.RootElement.GetProperty("modeVector").EnumerateArray().Select(value => value.GetDouble()).ToArray();
}

static ComplexMatrix LoadFlatInterleavedMatrix(string path, int size)
{
    double[] values = JsonSerializer.Deserialize<double[]>(File.ReadAllText(path))
        ?? throw new InvalidDataException($"Failed to deserialize {path}.");
    if (values.Length != 2 * size * size)
        throw new InvalidDataException($"Expected {2 * size * size} matrix values in {path}, found {values.Length}.");
    return ComplexMatrix.FromFlatInterleaved(values, size);
}

static ComplexMatrix LoadNestedMatrix(string path)
{
    using var doc = JsonDocument.Parse(File.ReadAllText(path));
    return new(LoadNestedArray(doc.RootElement.GetProperty("real")), LoadNestedArray(doc.RootElement.GetProperty("imag")));
}

static double[,] LoadNestedArray(JsonElement element)
{
    var rows = element.EnumerateArray().ToArray();
    int cols = rows[0].GetArrayLength();
    var result = new double[rows.Length, cols];
    for (int row = 0; row < rows.Length; row++)
    {
        var values = rows[row].EnumerateArray().ToArray();
        if (values.Length != cols)
            throw new InvalidDataException("Matrix rows must have equal lengths.");
        for (int col = 0; col < cols; col++)
            result[row, col] = values[col].GetDouble();
    }
    return result;
}

static double EuclideanHermiticityResidual(ComplexMatrix matrix) =>
    AdjointResidual(matrix, null);

static double MAdjointResidual(ComplexMatrix matrix, double[] weights) =>
    AdjointResidual(matrix, weights);

static double AdjointResidual(ComplexMatrix matrix, double[]? weights)
{
    double residual2 = 0.0;
    double norm2 = 0.0;
    for (int row = 0; row < matrix.Size; row++)
        for (int col = 0; col < matrix.Size; col++)
        {
            double rowWeight = weights is null ? 1.0 : weights[2 * row];
            double colWeight = weights is null ? 1.0 : weights[2 * col];
            double leftRe = rowWeight * matrix.Re[row, col];
            double leftIm = rowWeight * matrix.Im[row, col];
            double rightRe = colWeight * matrix.Re[col, row];
            double rightIm = -colWeight * matrix.Im[col, row];
            residual2 += Square(leftRe - rightRe) + Square(leftIm - rightIm);
            norm2 += Square(leftRe) + Square(leftIm);
        }
    return Math.Sqrt(residual2 / Math.Max(norm2, 1e-300));
}

static double MatrixRelativeResidual(ComplexMatrix left, ComplexMatrix right)
{
    if (left.Size != right.Size)
        throw new InvalidDataException("Matrix sizes must match.");
    double residual2 = 0.0;
    double norm2 = 0.0;
    for (int row = 0; row < left.Size; row++)
        for (int col = 0; col < left.Size; col++)
        {
            residual2 += Square(left.Re[row, col] - right.Re[row, col]) + Square(left.Im[row, col] - right.Im[row, col]);
            norm2 += Square(left.Re[row, col]) + Square(left.Im[row, col]);
        }
    return Math.Sqrt(residual2 / Math.Max(norm2, 1e-300));
}

static (double Real, double Imaginary) ComplexInnerProduct(double[] left, double[] right, double[]? weights)
{
    if (left.Length != right.Length || left.Length % 2 != 0)
        throw new InvalidDataException("Complex-interleaved vector lengths must match.");
    if (weights is not null && weights.Length != left.Length)
        throw new InvalidDataException("Weight and vector lengths must match.");
    double real = 0.0;
    double imaginary = 0.0;
    for (int index = 0; index < left.Length; index += 2)
    {
        double weight = weights is null ? 1.0 : weights[index];
        real += weight * (left[index] * right[index] + left[index + 1] * right[index + 1]);
        imaginary += weight * (left[index] * right[index + 1] - left[index + 1] * right[index]);
    }
    return (real, imaginary);
}

static double WeightedActionValue(ComplexMatrix a, double[] psi, double[] weights) =>
    ComplexInnerProduct(psi, a.Apply(psi), weights).Real;

static double[] AddScaled(double[] vector, double[] direction, double scale) =>
    vector.Zip(direction, (value, delta) => value + scale * delta).ToArray();

static double[] Scale(double[] vector, double scale) =>
    vector.Select(value => scale * value).ToArray();

static double[] Subtract(double[] left, double[] right) =>
    left.Zip(right, (l, r) => l - r).ToArray();

static double[] ApplyWeights(double[] vector, double[] weights) =>
    vector.Zip(weights, (value, weight) => value * weight).ToArray();

static double[] ScaleVectorByInverseSqrtWeights(double[] vector, double[] weights) =>
    vector.Zip(weights, (value, weight) => value / Math.Sqrt(weight)).ToArray();

static double[] NormalizeEuclidean(double[] vector)
{
    double norm = EuclideanNorm(vector);
    return norm > 1e-300 ? Scale(vector, 1.0 / norm) : vector.ToArray();
}

static double[] NormalizeM(double[] vector, double[] weights)
{
    double norm = Math.Sqrt(ComplexInnerProduct(vector, vector, weights).Real);
    return norm > 1e-300 ? Scale(vector, 1.0 / norm) : vector.ToArray();
}

static double RelativeVectorResidual(double[] left, double[] right) =>
    EuclideanNorm(Subtract(left, right)) / Math.Max(1e-300, Math.Max(EuclideanNorm(left), EuclideanNorm(right)));

static double EuclideanNorm(double[] vector) =>
    Math.Sqrt(vector.Sum(Square));

static double ScaleAwareResidual(double left, double right) =>
    Math.Abs(left - right) / Math.Max(1.0, Math.Max(Math.Abs(left), Math.Abs(right)));

static double ComplexScaleAwareResidual((double Real, double Imaginary) left, (double Real, double Imaginary) right) =>
    Math.Sqrt(Square(left.Real - right.Real) + Square(left.Imaginary - right.Imaginary)) /
    Math.Max(1.0, Math.Max(Math.Sqrt(Square(left.Real) + Square(left.Imaginary)), Math.Sqrt(Square(right.Real) + Square(right.Imaginary))));

static double Square(double value) => value * value;

static double ComputeEdgeLength(SimplicialMesh mesh, int edge)
{
    var left = mesh.GetVertexCoordinates(mesh.Edges[edge][0]);
    var right = mesh.GetVertexCoordinates(mesh.Edges[edge][1]);
    double sum = 0.0;
    for (int index = 0; index < left.Length; index++)
        sum += Square(right[index] - left[index]);
    return Math.Sqrt(sum);
}

static double[] ComputeEdgeDirection(SimplicialMesh mesh, int edge)
{
    var left = mesh.GetVertexCoordinates(mesh.Edges[edge][0]);
    var right = mesh.GetVertexCoordinates(mesh.Edges[edge][1]);
    var result = new double[left.Length];
    for (int index = 0; index < left.Length; index++)
        result[index] = right[index] - left[index];
    double norm = Math.Sqrt(result.Sum(Square));
    return norm > 1e-14 ? result.Select(value => value / norm).ToArray() : result;
}

public sealed class ComplexMatrix
{
    public ComplexMatrix(double[,] re, double[,] im)
    {
        if (re.GetLength(0) != re.GetLength(1) ||
            re.GetLength(0) != im.GetLength(0) ||
            re.GetLength(1) != im.GetLength(1))
            throw new InvalidDataException("Complex matrix must be square with matching real and imaginary shapes.");
        Re = re;
        Im = im;
    }

    [JsonIgnore]
    public double[,] Re { get; }
    [JsonIgnore]
    public double[,] Im { get; }
    public int Size => Re.GetLength(0);

    public static ComplexMatrix FromFlatInterleaved(double[] values, int size)
    {
        var re = new double[size, size];
        var im = new double[size, size];
        for (int row = 0; row < size; row++)
            for (int col = 0; col < size; col++)
            {
                int index = 2 * (row * size + col);
                re[row, col] = values[index];
                im[row, col] = values[index + 1];
            }
        return new(re, im);
    }

    public double[] ToFlatInterleaved()
    {
        var result = new double[2 * Size * Size];
        for (int row = 0; row < Size; row++)
            for (int col = 0; col < Size; col++)
            {
                int index = 2 * (row * Size + col);
                result[index] = Re[row, col];
                result[index + 1] = Im[row, col];
            }
        return result;
    }

    public double[] Apply(double[] vector)
    {
        if (vector.Length != 2 * Size)
            throw new InvalidDataException("Matrix and vector sizes must match.");
        var result = new double[vector.Length];
        for (int row = 0; row < Size; row++)
            for (int col = 0; col < Size; col++)
            {
                result[2 * row] += Re[row, col] * vector[2 * col] - Im[row, col] * vector[2 * col + 1];
                result[2 * row + 1] += Re[row, col] * vector[2 * col + 1] + Im[row, col] * vector[2 * col];
            }
        return result;
    }

    public ComplexMatrix ScaleRows(double[] weights, bool inverseSquareRoot)
    {
        var re = new double[Size, Size];
        var im = new double[Size, Size];
        for (int row = 0; row < Size; row++)
            for (int col = 0; col < Size; col++)
            {
                double divisor = inverseSquareRoot ? Math.Sqrt(weights[2 * row]) : weights[2 * row];
                re[row, col] = Re[row, col] / divisor;
                im[row, col] = Im[row, col] / divisor;
            }
        return new(re, im);
    }

    public ComplexMatrix ScaleSymmetric(double[] weights)
    {
        var re = new double[Size, Size];
        var im = new double[Size, Size];
        for (int row = 0; row < Size; row++)
            for (int col = 0; col < Size; col++)
            {
                double divisor = Math.Sqrt(weights[2 * row] * weights[2 * col]);
                re[row, col] = Re[row, col] / divisor;
                im[row, col] = Im[row, col] / divisor;
            }
        return new(re, im);
    }

    public ComplexMatrix ScaleSimilarity(double[] weights)
    {
        var re = new double[Size, Size];
        var im = new double[Size, Size];
        for (int row = 0; row < Size; row++)
            for (int col = 0; col < Size; col++)
            {
                double factor = Math.Sqrt(weights[2 * row] / weights[2 * col]);
                re[row, col] = factor * Re[row, col];
                im[row, col] = factor * Im[row, col];
            }
        return new(re, im);
    }
}

public sealed record ConventionAudit(ComplexMatrix K, ComplexMatrix A, ComplexMatrix B, ConventionAuditRecord Record);

public sealed class FermionModeSnapshot
{
    public required string ModeId { get; init; }
    public required int ModeIndex { get; init; }
    public required double[] EigenvectorCoefficients { get; init; }
}

public sealed class ConventionAuditRecord
{
    public required bool KIsEuclideanHermitian { get; init; }
    public required double KEuclideanHermiticityRelativeResidual { get; init; }
    public required bool RawKMisreadAsOperatorIsMSelfAdjoint { get; init; }
    public required double RawKMisreadAsOperatorMAdjointRelativeResidual { get; init; }
    public required bool AIsMSelfAdjoint { get; init; }
    public required double AMAdjointRelativeResidual { get; init; }
    public required bool BIsEuclideanHermitian { get; init; }
    public required double BEuclideanHermiticityRelativeResidual { get; init; }
    public required bool BSimilarityIdentityPassed { get; init; }
    public required double BSimilarityIdentityRelativeResidual { get; init; }
    public required bool AllTransformedIdentitiesPassed { get; init; }
}

public sealed class BackgroundAudit
{
    public required string FermionBackgroundId { get; init; }
    public required string BaseDiracMetadataPath { get; init; }
    public required string BaseDiracMatrixPath { get; init; }
    public required string PersistedSourceModeId { get; init; }
    public required int PersistedDirectionCount { get; init; }
    public required ConventionAuditRecord BaseConvention { get; init; }
    public required WeightedModeReplay WeightedModeReplay { get; init; }
    public required string SidecarPath { get; init; }
}

public sealed class VariationAudit
{
    public required string VariationId { get; init; }
    public required string FermionBackgroundId { get; init; }
    public required string BosonModeId { get; init; }
    public required string PersistedVariationMetadataPath { get; init; }
    public required string PersistedVariationMatrixPath { get; init; }
    public required string AnalyticVariationSource { get; init; }
    public required ConventionAuditRecord PersistedConvention { get; init; }
    public required ConventionAuditRecord AnalyticConvention { get; init; }
    public required double AnalyticPersistedRelativeResidual { get; init; }
    public required bool AnalyticPersistedParityPassed { get; init; }
    public required int DirectionalCheckCount { get; init; }
    public required int DirectionalIdentityPassedCount { get; init; }
    public required double MaxDirectionalPairingIdentityScaleAwareResidual { get; init; }
    public required double MaxDirectionalCentralDerivativeScaleAwareResidual { get; init; }
    public required bool TransformedVariationIdentityPassed { get; init; }
    public required IReadOnlyList<DirectionalAudit> Directions { get; init; }
    public required string SidecarPath { get; init; }
}

public sealed class DirectionalAudit
{
    public required string ChiModeId { get; init; }
    public required int ChiModeIndex { get; init; }
    public required string PsiSourceModeId { get; init; }
    public required int PsiSourceModeIndex { get; init; }
    public required DirectionalMatrixDiagnostic Persisted { get; init; }
    public required DirectionalMatrixDiagnostic Analytic { get; init; }
    public required double AnalyticPersistedPairingScaleAwareResidual { get; init; }
    public required double AnalyticPersistedDerivativeScaleAwareResidual { get; init; }
    public required bool AnalyticPersistedDirectionalParityPassed { get; init; }
    public required double MaxPairingIdentityScaleAwareResidual { get; init; }
    public required double MaxCentralDerivativeScaleAwareResidual { get; init; }
    public required bool TransformedDirectionalIdentityPassed { get; init; }
}

public sealed class DirectionalMatrixDiagnostic
{
    public required (double Real, double Imaginary) WeightedOperatorPairing { get; init; }
    public required (double Real, double Imaginary) StiffnessPairing { get; init; }
    public required double WeightedOperatorToStiffnessPairingScaleAwareResidual { get; init; }
    public required bool WeightedOperatorToStiffnessPairingIdentityPassed { get; init; }
    public required (double Real, double Imaginary) ReciprocalPartnerPairing { get; init; }
    public required double ReciprocalRealFormDirectionalDerivative { get; init; }
    public required double HermitianRealFormShortcutDirectionalDerivative { get; init; }
    public required double ReciprocalRealFormScaleAwareResidual { get; init; }
    public required bool ReciprocalRealFormIdentityPassed { get; init; }
    public required IReadOnlyList<CentralDerivativeRow> CentralDerivativeLadder { get; init; }
    public required double MaxCentralDerivativeScaleAwareResidual { get; init; }
    public required bool CentralDerivativeIdentityPassed { get; init; }
    public required bool AllDirectionalIdentitiesPassed { get; init; }
}

public sealed class CentralDerivativeRow
{
    public required double Epsilon { get; init; }
    public required double ActionAtPsiPlusEpsilonChi { get; init; }
    public required double ActionAtPsiMinusEpsilonChi { get; init; }
    public required double CentralDerivative { get; init; }
    public required double ExpectedReciprocalDirectionalDerivative { get; init; }
    public required double ScaleAwareResidual { get; init; }
    public required bool Passed { get; init; }
}

public sealed class WeightedModeReplay
{
    public required bool Materialized { get; init; }
    public required string SyntheticBundleConvention { get; init; }
    public required string BackTransformConvention { get; init; }
    public required string SolverName { get; init; }
    public required bool SolverConverged { get; init; }
    public required int SolverIterations { get; init; }
    public required int ModeCount { get; init; }
    public required double MaxBRelativeResidual { get; init; }
    public required double MaxGeneralizedRelativeResidual { get; init; }
    public required double MaxMOrthonormalityResidual { get; init; }
    public required bool QualityPassed { get; init; }
    public required string? SolverQualityObstruction { get; init; }
    public required IReadOnlyList<ModeReplayRow> Modes { get; init; }
}

public sealed class ModeReplayRow
{
    public required string ModeId { get; init; }
    public required int ModeIndex { get; init; }
    public required double Eigenvalue { get; init; }
    public required double SolverReportedResidualNorm { get; init; }
    public required double BAbsoluteResidual { get; init; }
    public required double BRelativeResidual { get; init; }
    public required double GeneralizedKPsiEqualsLambdaMPsiAbsoluteResidual { get; init; }
    public required double GeneralizedKPsiEqualsLambdaMPsiRelativeResidual { get; init; }
    public required double PsiMNorm { get; init; }
    public required double[] U { get; init; }
    public required double[] Psi { get; init; }
}
