using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using Gu.Core;
using Gu.Geometry;
using Gu.Phase4.Couplings;
using Gu.Phase4.Fermions;
using Gu.Phase4.Spin;

const string DefaultOutputDir = "studies/phase371_discrete_connection_dirac_first_variation_coverage_audit_001/output";
const string Phase12Root = "studies/phase12_joined_calculation_001/output/background_family";
const string VariationDir = $"{Phase12Root}/fermions/couplings/variations";
const string ModeDir = $"{Phase12Root}/spectra/modes";
const string FermionDir = $"{Phase12Root}/fermions";
const string SpinorRepresentationPath = $"{Phase12Root}/fermions/spinor_representation.json";
const string Phase120Path = "studies/phase120_analytic_variation_measure_consistency_001/output/analytic_variation_measure_consistency_summary.json";
const string Phase273Path = "studies/phase273_boson_fermion_coupling_proxy_source_audit_001/output/boson_fermion_coupling_proxy_source_audit_summary.json";
const string Phase370Path = "studies/phase370_completion_fermionic_yukawa_higgs_mixed_linearization_source_audit_001/output/completion_fermionic_yukawa_higgs_mixed_linearization_source_audit_summary.json";

const int ExpectedVariationCount = 24;
const int ExpectedBackgroundCount = 2;
const int ExpectedModesPerBackground = 12;
const int ExpectedFermionModesPerBackground = 12;
const double ParityTolerance = 1e-8;
const double HermiticityTolerance = 1e-10;

var outputDir = Environment.GetEnvironmentVariable("PHASE371_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);
var responseOutputDir = Path.Combine(outputDir, "responses");
Directory.CreateDirectory(responseOutputDir);

using var phase120 = JsonDocument.Parse(File.ReadAllText(Phase120Path));
using var phase273 = JsonDocument.Parse(File.ReadAllText(Phase273Path));
using var phase370 = JsonDocument.Parse(File.ReadAllText(Phase370Path));
using var spinorDoc = JsonDocument.Parse(File.ReadAllText(SpinorRepresentationPath));

var spinorSpec = spinorDoc.RootElement.Deserialize<SpinorRepresentationSpec>(JsonOptions())
    ?? throw new InvalidDataException($"Failed to deserialize {SpinorRepresentationPath}");
var gammas = new GammaMatrixBuilder().Build(
    spinorSpec.CliffordSignature,
    spinorSpec.GammaConvention,
    new()
    {
        CreatedAt = DateTimeOffset.UtcNow,
        CodeRevision = "phase371-discrete-connection-dirac-first-variation-coverage-audit",
        Branch = new() { BranchId = "phase371-discrete-connection-dirac-first-variation-coverage-audit", SchemaVersion = "1.0" },
        Backend = "cpu-reference",
    });

var mesh = ToyGeometryFactory.CreateStructuredFiberBundle2D(rows: 2, cols: 2).AmbientMesh;
int dimG = 3;
int spinorDim = spinorSpec.SpinorComponents;
var edgeLengths = new double[mesh.EdgeCount];
var edgeDirections = new double[mesh.EdgeCount][];
var cellsPerEdge = new int[mesh.EdgeCount][];
for (int edge = 0; edge < mesh.EdgeCount; edge++)
{
    edgeLengths[edge] = ComputeEdgeLength(mesh, edge);
    edgeDirections[edge] = ComputeEdgeDirection(mesh, edge);
    cellsPerEdge[edge] = [mesh.Edges[edge][0], mesh.Edges[edge][1]];
}

var fermionModesByBackground = LoadFermionModesByBackground(FermionDir);
var variationMetadataPaths = Directory
    .GetFiles(VariationDir, "variation-*.json")
    .Where(path => !path.EndsWith(".matrix.json", StringComparison.Ordinal))
    .OrderBy(path => path, StringComparer.Ordinal)
    .ToArray();
var records = variationMetadataPaths.Select(BuildRecord).ToArray();

var backgroundGroups = records
    .GroupBy(record => record.FermionBackgroundId, StringComparer.Ordinal)
    .OrderBy(group => group.Key, StringComparer.Ordinal)
    .Select(group => new
    {
        fermionBackgroundId = group.Key,
        variationCount = group.Count(),
        parityPassedCount = group.Count(record => record.AnalyticParityPassed),
        hermiticityPassedCount = group.Count(record => record.HermiticityPassed),
        maxUnitScaleRelativeResidual = group.Max(record => record.UnitScaleRelativeResidual),
        maxBestFitRelativeResidual = group.Max(record => record.BestFitRelativeResidual),
        maxAnalyticHermiticityRelativeResidual = group.Max(record => record.AnalyticHermiticityRelativeResidual),
        maxFiniteDifferenceHermiticityRelativeResidual = group.Max(record => record.FiniteDifferenceHermiticityRelativeResidual),
    })
    .ToArray();

int variationCount = records.Length;
int backgroundCount = backgroundGroups.Length;
int analyticParityPassedCount = records.Count(record => record.AnalyticParityPassed);
int hermiticityPassedCount = records.Count(record => record.HermiticityPassed);
int responseArtifactCount = records.Count(record => File.Exists(record.ResponseArtifactPath));
int responseParityPassedCount = records.Count(record => record.ResponseParityPassed);
int nonzeroVariationCount = records.Count(record =>
    record.FiniteDifferenceFrobeniusNorm > 0.0 &&
    record.AnalyticFrobeniusNorm > 0.0);
bool expectedCoveragePresent =
    variationCount == ExpectedVariationCount &&
    backgroundCount == ExpectedBackgroundCount &&
    backgroundGroups.All(group => group.variationCount == ExpectedModesPerBackground);
bool matrixShapesConsistent = records.All(record =>
    record.MatrixShape.Count == 2 &&
    record.MatrixShape[0] == record.MatrixShape[1] &&
    record.MatrixShape[0] > 0);
double maxUnitScaleRelativeResidual = records.Max(record => record.UnitScaleRelativeResidual);
double maxBestFitRelativeResidual = records.Max(record => record.BestFitRelativeResidual);
double maxAnalyticHermiticityRelativeResidual = records.Max(record => record.AnalyticHermiticityRelativeResidual);
double maxFiniteDifferenceHermiticityRelativeResidual = records.Max(record => record.FiniteDifferenceHermiticityRelativeResidual);
double maxResponseRelativeResidual = records.Max(record => record.ResponseRelativeResidual);
double maxSelectedProjectionRelativeResidual = records.Max(record => record.MaxSelectedProjectionRelativeResidual);
double bestFitScaleMean = records.Average(record => record.BestFitFiniteDifferenceToAnalyticScale);
double bestFitScaleRelativeRange = RelativeRange(records.Select(record => record.BestFitFiniteDifferenceToAnalyticScale));

bool phase120SelectedModeParityPresent = JsonBool(phase120.RootElement, "promotableAmplitudeMeasureFound") is true;
bool phase273FiniteDifferenceProxyBoundaryPresent =
    JsonBool(phase273.RootElement, "phase12CouplingAtlasesPresent") is true &&
    JsonBool(phase273.RootElement, "phase12FiniteDifferenceOnly") is true &&
    JsonInt(phase273.RootElement, "phase12VariationBundleCount") == ExpectedVariationCount;
bool phase370Vo7BoundaryPresent =
    JsonBool(phase370.RootElement, "completionFermionicYukawaHiggsMixedLinearizationSourceAuditPassed") is true &&
    JsonBool(phase370.RootElement, "completionRecordsVo7CoupledMixedLinearizationObligation") is true &&
    JsonBool(phase370.RootElement, "routeProvidesCompletedMixedLinearizationBlocks") is false;

bool discreteConnectionToDiracFirstVariationCoverageMaterialized =
    expectedCoveragePresent &&
    matrixShapesConsistent &&
    nonzeroVariationCount == ExpectedVariationCount &&
    analyticParityPassedCount == ExpectedVariationCount &&
    hermiticityPassedCount == ExpectedVariationCount &&
    responseArtifactCount == ExpectedVariationCount &&
    responseParityPassedCount == ExpectedVariationCount;

const bool routeProvidesCompletedFermionicAction = false;
const bool routeProvidesFixedFermionicOperatorBranch = false;
const bool routeProvidesExplicitYukawaFunctional = false;
const bool routeProvidesSolvedYukawaCouplingMap = false;
const bool routeProvidesCoupledResidual = false;
const bool routeProvidesCompletedMixedLinearizationBlocks = false;
const bool routeProvidesMixedLinearizationGaugeCompatibilityIdentities = false;
const bool routeProvidesDirectTargetIndependentWzBridgeSourceLaw = false;
const bool routeProvidesSeparateWzSourceRows = false;
const bool routeProvidesTargetIndependentGuVevSource = false;
const bool routeProvidesObservedPhotonWzHiggsProjectionRows = false;
const bool routeProvidesGuObservedFieldExtraction = false;
const bool routeProvidesHiggsScalarSourceOperator = false;
const bool routeProvidesScalarProjectionTheorem = false;
const bool routeProvidesScalarNormalizationSource = false;
const bool routeProvidesPoleMassExtraction = false;
const bool routeProvidesGeVUnitNormalization = false;
const bool routePromotesWzMasses = false;
const bool routePromotesHiggsMass = false;
const bool routeCompletesBosonPredictions = false;
const bool canFillPhase201WzContract = false;
const bool canFillPhase201HiggsContract = false;
const bool canFillPhase256ObservedFieldExtractionContract = false;

bool discreteConnectionDiracFirstVariationCoverageAuditPassed =
    phase120SelectedModeParityPresent &&
    phase273FiniteDifferenceProxyBoundaryPresent &&
    phase370Vo7BoundaryPresent &&
    discreteConnectionToDiracFirstVariationCoverageMaterialized &&
    !routeProvidesCompletedFermionicAction &&
    !routeProvidesFixedFermionicOperatorBranch &&
    !routeProvidesExplicitYukawaFunctional &&
    !routeProvidesSolvedYukawaCouplingMap &&
    !routeProvidesCoupledResidual &&
    !routeProvidesCompletedMixedLinearizationBlocks &&
    !routeProvidesMixedLinearizationGaugeCompatibilityIdentities &&
    !routeProvidesDirectTargetIndependentWzBridgeSourceLaw &&
    !routeProvidesSeparateWzSourceRows &&
    !routeProvidesTargetIndependentGuVevSource &&
    !routeProvidesObservedPhotonWzHiggsProjectionRows &&
    !routeProvidesGuObservedFieldExtraction &&
    !routeProvidesHiggsScalarSourceOperator &&
    !routeProvidesScalarProjectionTheorem &&
    !routeProvidesScalarNormalizationSource &&
    !routeProvidesPoleMassExtraction &&
    !routeProvidesGeVUnitNormalization &&
    !routePromotesWzMasses &&
    !routePromotesHiggsMass &&
    !routeCompletesBosonPredictions &&
    !canFillPhase201WzContract &&
    !canFillPhase201HiggsContract &&
    !canFillPhase256ObservedFieldExtractionContract;

var terminalStatus = discreteConnectionDiracFirstVariationCoverageAuditPassed
    ? "discrete-connection-dirac-first-variation-coverage-audit-analytic-parity-only-not-coupled-hessian"
    : "discrete-connection-dirac-first-variation-coverage-audit-blocked";
var decision = discreteConnectionDiracFirstVariationCoverageAuditPassed
    ? "The existing discrete connection-to-Dirac first variation is analytically replayable and Hermitian across all 24 persisted Phase12 branch-local mode directions. This is one implemented VO-7 building block, not a completed boson-fermion mixed Hessian: the repository still lacks a fixed GU fermionic branch, explicit Yukawa functional, coupled residual, the remaining mixed blocks, gauge-compatibility identities, scalar projection, observed-field extraction, pole extraction, and GeV normalization. Do not promote W/Z or Higgs masses."
    : "Do not treat the discrete connection-to-Dirac first variation as covered until every persisted Phase12 variation passes analytic parity and Hermiticity checks.";

var result = new
{
    phaseId = "phase371-discrete-connection-dirac-first-variation-coverage-audit",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    discreteConnectionDiracFirstVariationCoverageAuditPassed,
    phase120SelectedModeParityPresent,
    phase273FiniteDifferenceProxyBoundaryPresent,
    phase370Vo7BoundaryPresent,
    discreteConnectionToDiracFirstVariationCoverageMaterialized,
    discreteConnectionToDiracFirstVariationIsVo7BuildingBlock = true,
    discreteConnectionToDiracFirstVariationCompletesVo7 = false,
    expectedCoveragePresent,
    matrixShapesConsistent,
    variationCount,
    backgroundCount,
    expectedVariationCount = ExpectedVariationCount,
    expectedBackgroundCount = ExpectedBackgroundCount,
    expectedModesPerBackground = ExpectedModesPerBackground,
    analyticParityPassedCount,
    hermiticityPassedCount,
    responseArtifactCount,
    responseParityPassedCount,
    nonzeroVariationCount,
    parityTolerance = ParityTolerance,
    hermiticityTolerance = HermiticityTolerance,
    maxUnitScaleRelativeResidual,
    maxBestFitRelativeResidual,
    bestFitScaleMean,
    bestFitScaleRelativeRange,
    maxAnalyticHermiticityRelativeResidual,
    maxFiniteDifferenceHermiticityRelativeResidual,
    maxResponseRelativeResidual,
    maxSelectedProjectionRelativeResidual,
    responseSelection = new
    {
        targetBlind = true,
        sourceModeSelectionRule = "select persisted branch-local fermion mode with minimum modeIndex; no physical boson target or observed mass consulted",
        responseDefinition = "delta_D[b_k] phi_j",
        projectionDefinition = "<phi_i, delta_D[b_k] phi_j>",
        normalizationConvention = "persisted branch-local fermion eigenvector coefficients with raw boson variation vector",
        responseArtifactDirectory = responseOutputDir,
    },
    routeProvidesCompletedFermionicAction,
    routeProvidesFixedFermionicOperatorBranch,
    routeProvidesExplicitYukawaFunctional,
    routeProvidesSolvedYukawaCouplingMap,
    routeProvidesCoupledResidual,
    routeProvidesCompletedMixedLinearizationBlocks,
    routeProvidesMixedLinearizationGaugeCompatibilityIdentities,
    routeProvidesDirectTargetIndependentWzBridgeSourceLaw,
    routeProvidesSeparateWzSourceRows,
    routeProvidesTargetIndependentGuVevSource,
    routeProvidesObservedPhotonWzHiggsProjectionRows,
    routeProvidesGuObservedFieldExtraction,
    routeProvidesHiggsScalarSourceOperator,
    routeProvidesScalarProjectionTheorem,
    routeProvidesScalarNormalizationSource,
    routeProvidesPoleMassExtraction,
    routeProvidesGeVUnitNormalization,
    routePromotesWzMasses,
    routePromotesHiggsMass,
    routeCompletesBosonPredictions,
    canFillPhase201WzContract,
    canFillPhase201HiggsContract,
    canFillPhase256ObservedFieldExtractionContract,
    backgroundGroups,
    records,
    missingVo7Artifacts = new[]
    {
        "fixed GU fermionic operator branch",
        "explicit GU Yukawa-like functional and solved coupling map",
        "coupled boson-fermion residual",
        "remaining mixed-linearization blocks",
        "mixed-block gauge-compatibility identities",
        "scalar projection theorem and normalization source",
        "epsilon-ladder finite-difference convergence evidence beyond the persisted Phase12 epsilon=1e-5 sidecars",
    },
    predictionContractImpact = new
    {
        canFillPhase201WzContract,
        canFillPhase201HiggsContract,
        canFillPhase256ObservedFieldExtractionContract,
        phase201FieldsDefensiblyFilled = Array.Empty<string>(),
    },
    sourceEvidence = new
    {
        phase12Root = Phase12Root,
        variationDir = VariationDir,
        modeDir = ModeDir,
        fermionDir = FermionDir,
        spinorRepresentationPath = SpinorRepresentationPath,
        phase120Path = Phase120Path,
        phase273Path = Phase273Path,
        phase370Path = Phase370Path,
    },
    decision,
};

var options = JsonOptions();
var resultPath = Path.Combine(outputDir, "discrete_connection_dirac_first_variation_coverage_audit.json");
File.WriteAllText(resultPath, JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "discrete_connection_dirac_first_variation_coverage_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.generatedAt,
        terminalStatus,
        discreteConnectionDiracFirstVariationCoverageAuditPassed,
        phase120SelectedModeParityPresent,
        phase273FiniteDifferenceProxyBoundaryPresent,
        phase370Vo7BoundaryPresent,
        discreteConnectionToDiracFirstVariationCoverageMaterialized,
        discreteConnectionToDiracFirstVariationIsVo7BuildingBlock = true,
        discreteConnectionToDiracFirstVariationCompletesVo7 = false,
        expectedCoveragePresent,
        matrixShapesConsistent,
        variationCount,
        backgroundCount,
        analyticParityPassedCount,
        hermiticityPassedCount,
        responseArtifactCount,
        responseParityPassedCount,
        nonzeroVariationCount,
        maxUnitScaleRelativeResidual,
        maxBestFitRelativeResidual,
        bestFitScaleMean,
        bestFitScaleRelativeRange,
        maxAnalyticHermiticityRelativeResidual,
        maxFiniteDifferenceHermiticityRelativeResidual,
        maxResponseRelativeResidual,
        maxSelectedProjectionRelativeResidual,
        routeProvidesCompletedFermionicAction,
        routeProvidesExplicitYukawaFunctional,
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
        result.missingVo7Artifacts,
        result.predictionContractImpact,
        result.decision,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"discreteConnectionDiracFirstVariationCoverageAuditPassed={discreteConnectionDiracFirstVariationCoverageAuditPassed}");
Console.WriteLine($"variationCount={variationCount}");
Console.WriteLine($"analyticParityPassedCount={analyticParityPassedCount}");
Console.WriteLine($"hermiticityPassedCount={hermiticityPassedCount}");
Console.WriteLine($"responseArtifactCount={responseArtifactCount}");
Console.WriteLine($"responseParityPassedCount={responseParityPassedCount}");
Console.WriteLine($"maxUnitScaleRelativeResidual={maxUnitScaleRelativeResidual:R}");
Console.WriteLine($"maxBestFitRelativeResidual={maxBestFitRelativeResidual:R}");
Console.WriteLine($"maxResponseRelativeResidual={maxResponseRelativeResidual:R}");
Console.WriteLine($"maxSelectedProjectionRelativeResidual={maxSelectedProjectionRelativeResidual:R}");
Console.WriteLine($"bestFitScaleRelativeRange={bestFitScaleRelativeRange:R}");
Console.WriteLine($"resultPath={resultPath}");

VariationCoverageRecord BuildRecord(string metadataPath)
{
    using var metadataDoc = JsonDocument.Parse(File.ReadAllText(metadataPath));
    var metadata = metadataDoc.RootElement;
    string variationId = RequiredString(metadata, "variationId");
    string bosonModeId = RequiredString(metadata, "bosonModeId");
    string fermionBackgroundId = RequiredString(metadata, "fermionBackgroundId");
    string normalizationConvention = RequiredString(metadata, "normalizationConvention");
    string variationMethod = RequiredString(metadata, "variationMethod");
    string matrixArtifactRef = RequiredString(metadata, "matrixArtifactRef");
    double finiteDifferenceEpsilon = RequiredDouble(metadata, "finiteDifferenceEpsilon");
    string modePath = Path.Combine(ModeDir, $"{bosonModeId}.json");
    string finiteDifferenceMatrixPath = Path.Combine(VariationDir, matrixArtifactRef);
    var modeVector = LoadModeVector(modePath);

    using var matrixDoc = JsonDocument.Parse(File.ReadAllText(finiteDifferenceMatrixPath));
    var finiteRe = LoadMatrix(matrixDoc.RootElement.GetProperty("real"));
    var finiteIm = LoadMatrix(matrixDoc.RootElement.GetProperty("imag"));
    ValidateSameShape(finiteRe, finiteIm, finiteDifferenceMatrixPath);

    var (analyticRe, analyticIm) = DiracVariationComputer.ComputeAnalytical(
        modeVector,
        gammas,
        mesh.VertexCount,
        spinorDim,
        dimG,
        edgeLengths,
        cellsPerEdge,
        edgeDirections);
    ValidateSameShape(finiteRe, analyticRe, finiteDifferenceMatrixPath);
    ValidateSameShape(finiteIm, analyticIm, finiteDifferenceMatrixPath);

    double scale = BestFitScale(finiteRe, finiteIm, analyticRe, analyticIm);
    double bestFitRelativeResidual = RelativeResidual(finiteRe, finiteIm, analyticRe, analyticIm, scale);
    double unitScaleRelativeResidual = RelativeResidual(finiteRe, finiteIm, analyticRe, analyticIm, 1.0);
    double analyticHermiticityRelativeResidual = HermiticityRelativeResidual(analyticRe, analyticIm);
    double finiteDifferenceHermiticityRelativeResidual = HermiticityRelativeResidual(finiteRe, finiteIm);
    bool analyticParityPassed =
        variationMethod == "finite-difference" &&
        unitScaleRelativeResidual <= ParityTolerance &&
        bestFitRelativeResidual <= ParityTolerance;
    bool hermiticityPassed =
        analyticHermiticityRelativeResidual <= HermiticityTolerance &&
        finiteDifferenceHermiticityRelativeResidual <= HermiticityTolerance;
    var response = BuildResponseDiagnostic(
        variationId,
        fermionBackgroundId,
        normalizationConvention,
        finiteDifferenceEpsilon,
        finiteRe,
        finiteIm,
        analyticRe,
        analyticIm);

    return new VariationCoverageRecord
    {
        VariationId = variationId,
        BosonModeId = bosonModeId,
        FermionBackgroundId = fermionBackgroundId,
        NormalizationConvention = normalizationConvention,
        VariationMethod = variationMethod,
        FiniteDifferenceEpsilon = finiteDifferenceEpsilon,
        MetadataPath = metadataPath,
        ModePath = modePath,
        FiniteDifferenceMatrixPath = finiteDifferenceMatrixPath,
        ModeVectorLength = modeVector.Length,
        ModeVectorNorm = Math.Sqrt(modeVector.Sum(value => value * value)),
        MatrixShape = [finiteRe.GetLength(0), finiteRe.GetLength(1)],
        FiniteDifferenceFrobeniusNorm = FrobeniusNorm(finiteRe, finiteIm),
        AnalyticFrobeniusNorm = FrobeniusNorm(analyticRe, analyticIm),
        BestFitFiniteDifferenceToAnalyticScale = scale,
        BestFitRelativeResidual = bestFitRelativeResidual,
        UnitScaleRelativeResidual = unitScaleRelativeResidual,
        AnalyticHermiticityRelativeResidual = analyticHermiticityRelativeResidual,
        FiniteDifferenceHermiticityRelativeResidual = finiteDifferenceHermiticityRelativeResidual,
        AnalyticParityPassed = analyticParityPassed,
        HermiticityPassed = hermiticityPassed,
        ResponseSourceFermionModeId = response.SourceFermionModeId,
        ResponseArtifactPath = response.ResponseArtifactPath,
        FiniteDifferenceResponseSha256 = response.FiniteDifferenceResponseSha256,
        AnalyticResponseSha256 = response.AnalyticResponseSha256,
        FiniteDifferenceResponseNorm = response.FiniteDifferenceResponseNorm,
        AnalyticResponseNorm = response.AnalyticResponseNorm,
        ResponseRelativeResidual = response.ResponseRelativeResidual,
        SelectedProjectionCount = response.SelectedProjectionCount,
        MaxSelectedProjectionRelativeResidual = response.MaxSelectedProjectionRelativeResidual,
        ResponseParityPassed = response.ResponseParityPassed,
    };
}

ResponseDiagnostic BuildResponseDiagnostic(
    string variationId,
    string fermionBackgroundId,
    string normalizationConvention,
    double finiteDifferenceEpsilon,
    double[,] finiteRe,
    double[,] finiteIm,
    double[,] analyticRe,
    double[,] analyticIm)
{
    if (!fermionModesByBackground.TryGetValue(fermionBackgroundId, out var fermionModes))
        throw new InvalidDataException($"Missing fermion modes for background {fermionBackgroundId}.");
    if (fermionModes.Count != ExpectedFermionModesPerBackground)
        throw new InvalidDataException($"Expected {ExpectedFermionModesPerBackground} fermion modes for {fermionBackgroundId}, found {fermionModes.Count}.");

    var sourceMode = fermionModes.OrderBy(mode => mode.ModeIndex).First();
    var finiteResponse = ApplyComplexMatrix(finiteRe, finiteIm, sourceMode.EigenvectorCoefficients);
    var analyticResponse = ApplyComplexMatrix(analyticRe, analyticIm, sourceMode.EigenvectorCoefficients);
    double responseRelativeResidual = InterleavedRelativeResidual(finiteResponse, analyticResponse);
    var projections = fermionModes
        .OrderBy(mode => mode.ModeIndex)
        .Select(mode =>
        {
            var finiteProjection = ComplexInnerProduct(mode.EigenvectorCoefficients, finiteResponse);
            var analyticProjection = ComplexInnerProduct(mode.EigenvectorCoefficients, analyticResponse);
            return new ProjectionDiagnostic
            {
                FermionModeIdI = mode.ModeId,
                FermionModeIdJ = sourceMode.ModeId,
                FiniteDifferenceProjectionReal = finiteProjection.Real,
                FiniteDifferenceProjectionImaginary = finiteProjection.Imaginary,
                AnalyticProjectionReal = analyticProjection.Real,
                AnalyticProjectionImaginary = analyticProjection.Imaginary,
                RelativeResidual = ComplexRelativeResidual(finiteProjection, analyticProjection),
            };
        })
        .ToArray();
    double maxSelectedProjectionRelativeResidual = projections.Max(projection => projection.RelativeResidual);
    bool responseParityPassed =
        responseRelativeResidual <= ParityTolerance &&
        maxSelectedProjectionRelativeResidual <= ParityTolerance;
    string responseArtifactPath = Path.Combine(responseOutputDir, $"{variationId}-response-mode-min-index.json");

    File.WriteAllText(
        responseArtifactPath,
        JsonSerializer.Serialize(new
        {
            variationId,
            fermionBackgroundId,
            sourceFermionModeId = sourceMode.ModeId,
            sourceFermionModeIndex = sourceMode.ModeIndex,
            targetBlindSelection = true,
            sourceModeSelectionRule = "select persisted branch-local fermion mode with minimum modeIndex; no physical boson target or observed mass consulted",
            responseDefinition = "delta_D[b_k] phi_j",
            projectionDefinition = "<phi_i, delta_D[b_k] phi_j>",
            normalizationConvention,
            fermionModeCoefficientConvention = "persisted complex interleaved eigenvector coefficients",
            finiteDifferenceEpsilon,
            epsilonLadderRecomputed = false,
            finiteDifferenceResponseInterleaved = finiteResponse,
            analyticResponseInterleaved = analyticResponse,
            finiteDifferenceResponseSha256 = Sha256(finiteResponse),
            analyticResponseSha256 = Sha256(analyticResponse),
            finiteDifferenceResponseNorm = VectorNorm(finiteResponse),
            analyticResponseNorm = VectorNorm(analyticResponse),
            responseRelativeResidual,
            maxSelectedProjectionRelativeResidual,
            responseParityPassed,
            projections,
        }, JsonOptions()));

    return new ResponseDiagnostic
    {
        SourceFermionModeId = sourceMode.ModeId,
        ResponseArtifactPath = responseArtifactPath,
        FiniteDifferenceResponseSha256 = Sha256(finiteResponse),
        AnalyticResponseSha256 = Sha256(analyticResponse),
        FiniteDifferenceResponseNorm = VectorNorm(finiteResponse),
        AnalyticResponseNorm = VectorNorm(analyticResponse),
        ResponseRelativeResidual = responseRelativeResidual,
        SelectedProjectionCount = projections.Length,
        MaxSelectedProjectionRelativeResidual = maxSelectedProjectionRelativeResidual,
        ResponseParityPassed = responseParityPassed,
    };
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
    element.TryGetProperty(propertyName, out var value) &&
    value.ValueKind == JsonValueKind.Number &&
    value.TryGetInt32(out var result)
        ? result
        : null;

static string RequiredString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
        ? value.GetString() ?? throw new InvalidDataException($"{propertyName} must not be null.")
        : throw new InvalidDataException($"{propertyName} must be a string.");

static double RequiredDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Number
        ? value.GetDouble()
        : throw new InvalidDataException($"{propertyName} must be a number.");

static IReadOnlyDictionary<string, IReadOnlyList<FermionModeSnapshot>> LoadFermionModesByBackground(string fermionDir)
{
    var result = new Dictionary<string, IReadOnlyList<FermionModeSnapshot>>(StringComparer.Ordinal);
    foreach (string path in Directory.GetFiles(fermionDir, "fermion_modes_*.json").OrderBy(path => path, StringComparer.Ordinal))
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        string backgroundId = RequiredString(doc.RootElement, "fermionBackgroundId");
        var modes = doc.RootElement
            .GetProperty("modes")
            .EnumerateArray()
            .Select(mode => new FermionModeSnapshot
            {
                ModeId = RequiredString(mode, "modeId"),
                ModeIndex = mode.GetProperty("modeIndex").GetInt32(),
                EigenvectorCoefficients = mode
                    .GetProperty("eigenvectorCoefficients")
                    .EnumerateArray()
                    .Select(value => value.GetDouble())
                    .ToArray(),
            })
            .ToArray();
        result.Add(backgroundId, modes);
    }

    return result;
}

static double[] LoadModeVector(string path)
{
    using var doc = JsonDocument.Parse(File.ReadAllText(path));
    return doc.RootElement.GetProperty("modeVector").EnumerateArray().Select(value => value.GetDouble()).ToArray();
}

static double[,] LoadMatrix(JsonElement array)
{
    var rows = array.EnumerateArray().ToArray();
    if (rows.Length == 0)
        throw new InvalidDataException("Matrix must contain at least one row.");

    int rowCount = rows.Length;
    int colCount = rows[0].GetArrayLength();
    var matrix = new double[rowCount, colCount];
    for (int row = 0; row < rowCount; row++)
    {
        var cols = rows[row].EnumerateArray().ToArray();
        if (cols.Length != colCount)
            throw new InvalidDataException("Matrix rows must have consistent lengths.");
        for (int col = 0; col < colCount; col++)
            matrix[row, col] = cols[col].GetDouble();
    }

    return matrix;
}

static void ValidateSameShape(double[,] left, double[,] right, string path)
{
    if (left.GetLength(0) != right.GetLength(0) || left.GetLength(1) != right.GetLength(1))
        throw new InvalidDataException($"Matrix shape mismatch while processing {path}.");
}

static double BestFitScale(double[,] targetRe, double[,] targetIm, double[,] sourceRe, double[,] sourceIm)
{
    double dot = 0.0;
    double norm2 = 0.0;
    int rows = targetRe.GetLength(0);
    int cols = targetRe.GetLength(1);
    for (int row = 0; row < rows; row++)
        for (int col = 0; col < cols; col++)
        {
            dot += targetRe[row, col] * sourceRe[row, col] + targetIm[row, col] * sourceIm[row, col];
            norm2 += sourceRe[row, col] * sourceRe[row, col] + sourceIm[row, col] * sourceIm[row, col];
        }

    return norm2 > 0.0 ? dot / norm2 : double.NaN;
}

static double RelativeResidual(double[,] targetRe, double[,] targetIm, double[,] sourceRe, double[,] sourceIm, double scale)
{
    double residual2 = 0.0;
    double target2 = 0.0;
    int rows = targetRe.GetLength(0);
    int cols = targetRe.GetLength(1);
    for (int row = 0; row < rows; row++)
        for (int col = 0; col < cols; col++)
        {
            double dRe = targetRe[row, col] - scale * sourceRe[row, col];
            double dIm = targetIm[row, col] - scale * sourceIm[row, col];
            residual2 += dRe * dRe + dIm * dIm;
            target2 += targetRe[row, col] * targetRe[row, col] + targetIm[row, col] * targetIm[row, col];
        }

    return target2 > 0.0 ? Math.Sqrt(residual2 / target2) : double.NaN;
}

static double FrobeniusNorm(double[,] re, double[,] im)
{
    double sum = 0.0;
    int rows = re.GetLength(0);
    int cols = re.GetLength(1);
    for (int row = 0; row < rows; row++)
        for (int col = 0; col < cols; col++)
            sum += re[row, col] * re[row, col] + im[row, col] * im[row, col];

    return Math.Sqrt(sum);
}

static double[] ApplyComplexMatrix(double[,] re, double[,] im, double[] vector)
{
    int rows = re.GetLength(0);
    int cols = re.GetLength(1);
    if (vector.Length != 2 * cols)
        throw new InvalidDataException($"Expected complex interleaved vector length {2 * cols}, found {vector.Length}.");

    var result = new double[2 * rows];
    for (int row = 0; row < rows; row++)
        for (int col = 0; col < cols; col++)
        {
            double aRe = re[row, col];
            double aIm = im[row, col];
            double bRe = vector[2 * col];
            double bIm = vector[2 * col + 1];
            result[2 * row] += aRe * bRe - aIm * bIm;
            result[2 * row + 1] += aRe * bIm + aIm * bRe;
        }

    return result;
}

static (double Real, double Imaginary) ComplexInnerProduct(double[] left, double[] right)
{
    if (left.Length != right.Length || left.Length % 2 != 0)
        throw new InvalidDataException("Complex interleaved vectors must have equal even lengths.");

    double real = 0.0;
    double imaginary = 0.0;
    for (int index = 0; index < left.Length; index += 2)
    {
        double leftRe = left[index];
        double leftIm = left[index + 1];
        double rightRe = right[index];
        double rightIm = right[index + 1];
        real += leftRe * rightRe + leftIm * rightIm;
        imaginary += leftRe * rightIm - leftIm * rightRe;
    }

    return (real, imaginary);
}

static double InterleavedRelativeResidual(double[] target, double[] source)
{
    if (target.Length != source.Length)
        throw new InvalidDataException("Vectors must have equal lengths.");

    double residual2 = 0.0;
    double target2 = 0.0;
    for (int index = 0; index < target.Length; index++)
    {
        double difference = target[index] - source[index];
        residual2 += difference * difference;
        target2 += target[index] * target[index];
    }

    return target2 > 0.0 ? Math.Sqrt(residual2 / target2) : double.NaN;
}

static double ComplexRelativeResidual(
    (double Real, double Imaginary) target,
    (double Real, double Imaginary) source)
{
    double differenceReal = target.Real - source.Real;
    double differenceImaginary = target.Imaginary - source.Imaginary;
    double target2 = target.Real * target.Real + target.Imaginary * target.Imaginary;
    double residual2 = differenceReal * differenceReal + differenceImaginary * differenceImaginary;
    return target2 > 0.0 ? Math.Sqrt(residual2 / target2) : Math.Sqrt(residual2);
}

static double VectorNorm(double[] vector) => Math.Sqrt(vector.Sum(value => value * value));

static string Sha256(double[] vector)
{
    var bytes = new byte[vector.Length * sizeof(double)];
    Buffer.BlockCopy(vector, 0, bytes, 0, bytes.Length);
    return Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
}

static double HermiticityRelativeResidual(double[,] re, double[,] im)
{
    if (re.GetLength(0) != re.GetLength(1))
        return double.PositiveInfinity;

    double residual2 = 0.0;
    double norm2 = 0.0;
    int size = re.GetLength(0);
    for (int row = 0; row < size; row++)
        for (int col = 0; col < size; col++)
        {
            double dRe = re[row, col] - re[col, row];
            double dIm = im[row, col] + im[col, row];
            residual2 += dRe * dRe + dIm * dIm;
            norm2 += re[row, col] * re[row, col] + im[row, col] * im[row, col];
        }

    return norm2 > 0.0 ? Math.Sqrt(residual2 / norm2) : double.NaN;
}

static double RelativeRange(IEnumerable<double> values)
{
    var array = values.ToArray();
    double mean = array.Average();
    return Math.Abs(mean) > 0.0 ? (array.Max() - array.Min()) / Math.Abs(mean) : double.NaN;
}

static double ComputeEdgeLength(SimplicialMesh mesh, int edgeIdx)
{
    int v0 = mesh.Edges[edgeIdx][0];
    int v1 = mesh.Edges[edgeIdx][1];
    var coords0 = mesh.GetVertexCoordinates(v0);
    var coords1 = mesh.GetVertexCoordinates(v1);
    double norm = 0.0;
    for (int k = 0; k < coords0.Length; k++)
    {
        double d = coords1[k] - coords0[k];
        norm += d * d;
    }

    return Math.Sqrt(norm);
}

static double[] ComputeEdgeDirection(SimplicialMesh mesh, int edgeIdx)
{
    int v0 = mesh.Edges[edgeIdx][0];
    int v1 = mesh.Edges[edgeIdx][1];
    int dim = mesh.EmbeddingDimension;
    var coords0 = mesh.GetVertexCoordinates(v0);
    var coords1 = mesh.GetVertexCoordinates(v1);
    var direction = new double[dim];
    double norm = 0.0;
    for (int k = 0; k < dim; k++)
    {
        direction[k] = coords1[k] - coords0[k];
        norm += direction[k] * direction[k];
    }

    norm = Math.Sqrt(norm);
    if (norm > 1e-14)
        for (int k = 0; k < dim; k++)
            direction[k] /= norm;

    return direction;
}

public sealed class VariationCoverageRecord
{
    [JsonPropertyName("variationId")]
    public required string VariationId { get; init; }

    [JsonPropertyName("bosonModeId")]
    public required string BosonModeId { get; init; }

    [JsonPropertyName("fermionBackgroundId")]
    public required string FermionBackgroundId { get; init; }

    [JsonPropertyName("normalizationConvention")]
    public required string NormalizationConvention { get; init; }

    [JsonPropertyName("variationMethod")]
    public required string VariationMethod { get; init; }

    [JsonPropertyName("finiteDifferenceEpsilon")]
    public required double FiniteDifferenceEpsilon { get; init; }

    [JsonPropertyName("metadataPath")]
    public required string MetadataPath { get; init; }

    [JsonPropertyName("modePath")]
    public required string ModePath { get; init; }

    [JsonPropertyName("finiteDifferenceMatrixPath")]
    public required string FiniteDifferenceMatrixPath { get; init; }

    [JsonPropertyName("modeVectorLength")]
    public required int ModeVectorLength { get; init; }

    [JsonPropertyName("modeVectorNorm")]
    public required double ModeVectorNorm { get; init; }

    [JsonPropertyName("matrixShape")]
    public required IReadOnlyList<int> MatrixShape { get; init; }

    [JsonPropertyName("finiteDifferenceFrobeniusNorm")]
    public required double FiniteDifferenceFrobeniusNorm { get; init; }

    [JsonPropertyName("analyticFrobeniusNorm")]
    public required double AnalyticFrobeniusNorm { get; init; }

    [JsonPropertyName("bestFitFiniteDifferenceToAnalyticScale")]
    public required double BestFitFiniteDifferenceToAnalyticScale { get; init; }

    [JsonPropertyName("bestFitRelativeResidual")]
    public required double BestFitRelativeResidual { get; init; }

    [JsonPropertyName("unitScaleRelativeResidual")]
    public required double UnitScaleRelativeResidual { get; init; }

    [JsonPropertyName("analyticHermiticityRelativeResidual")]
    public required double AnalyticHermiticityRelativeResidual { get; init; }

    [JsonPropertyName("finiteDifferenceHermiticityRelativeResidual")]
    public required double FiniteDifferenceHermiticityRelativeResidual { get; init; }

    [JsonPropertyName("analyticParityPassed")]
    public required bool AnalyticParityPassed { get; init; }

    [JsonPropertyName("hermiticityPassed")]
    public required bool HermiticityPassed { get; init; }

    [JsonPropertyName("responseSourceFermionModeId")]
    public required string ResponseSourceFermionModeId { get; init; }

    [JsonPropertyName("responseArtifactPath")]
    public required string ResponseArtifactPath { get; init; }

    [JsonPropertyName("finiteDifferenceResponseSha256")]
    public required string FiniteDifferenceResponseSha256 { get; init; }

    [JsonPropertyName("analyticResponseSha256")]
    public required string AnalyticResponseSha256 { get; init; }

    [JsonPropertyName("finiteDifferenceResponseNorm")]
    public required double FiniteDifferenceResponseNorm { get; init; }

    [JsonPropertyName("analyticResponseNorm")]
    public required double AnalyticResponseNorm { get; init; }

    [JsonPropertyName("responseRelativeResidual")]
    public required double ResponseRelativeResidual { get; init; }

    [JsonPropertyName("selectedProjectionCount")]
    public required int SelectedProjectionCount { get; init; }

    [JsonPropertyName("maxSelectedProjectionRelativeResidual")]
    public required double MaxSelectedProjectionRelativeResidual { get; init; }

    [JsonPropertyName("responseParityPassed")]
    public required bool ResponseParityPassed { get; init; }
}

public sealed class FermionModeSnapshot
{
    public required string ModeId { get; init; }

    public required int ModeIndex { get; init; }

    public required double[] EigenvectorCoefficients { get; init; }
}

public sealed class ProjectionDiagnostic
{
    [JsonPropertyName("fermionModeIdI")]
    public required string FermionModeIdI { get; init; }

    [JsonPropertyName("fermionModeIdJ")]
    public required string FermionModeIdJ { get; init; }

    [JsonPropertyName("finiteDifferenceProjectionReal")]
    public required double FiniteDifferenceProjectionReal { get; init; }

    [JsonPropertyName("finiteDifferenceProjectionImaginary")]
    public required double FiniteDifferenceProjectionImaginary { get; init; }

    [JsonPropertyName("analyticProjectionReal")]
    public required double AnalyticProjectionReal { get; init; }

    [JsonPropertyName("analyticProjectionImaginary")]
    public required double AnalyticProjectionImaginary { get; init; }

    [JsonPropertyName("relativeResidual")]
    public required double RelativeResidual { get; init; }
}

public sealed class ResponseDiagnostic
{
    public required string SourceFermionModeId { get; init; }

    public required string ResponseArtifactPath { get; init; }

    public required string FiniteDifferenceResponseSha256 { get; init; }

    public required string AnalyticResponseSha256 { get; init; }

    public required double FiniteDifferenceResponseNorm { get; init; }

    public required double AnalyticResponseNorm { get; init; }

    public required double ResponseRelativeResidual { get; init; }

    public required int SelectedProjectionCount { get; init; }

    public required double MaxSelectedProjectionRelativeResidual { get; init; }

    public required bool ResponseParityPassed { get; init; }
}
