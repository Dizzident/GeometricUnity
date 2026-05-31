using System.Text.Json;
using Gu.Core;
using Gu.Geometry;
using Gu.Phase4.Dirac;
using Gu.Phase4.Fermions;
using Gu.Phase4.Spin;

const string DefaultOutputDir = "studies/phase374_shared_weighted_fermion_spectral_solver_repair_audit_001/output";
const string Phase12Root = "studies/phase12_joined_calculation_001/output/background_family";
const string FermionDir = $"{Phase12Root}/fermions";
const string Phase373SummaryPath = "studies/phase373_mass_psi_stiffness_operator_convention_repair_audit_001/output/mass_psi_stiffness_operator_convention_repair_audit_summary.json";
const string SpinorRepresentationPath = $"{FermionDir}/spinor_representation.json";
const int ExpectedBackgroundCount = 2;
const int ExpectedModeCountPerBackground = 12;
const int ExpectedSyntheticModeCount = 4;
const double ResidualTolerance = 1e-6;
const double NormalizationTolerance = 1e-8;
const double OrthonormalityTolerance = 1e-8;
const double IdentityRegressionTolerance = 1e-8;

var options = JsonOptions();
var outputDir = Environment.GetEnvironmentVariable("PHASE374_OUTPUT_DIR") ?? DefaultOutputDir;
var backgroundOutputDir = Path.Combine(outputDir, "backgrounds");
Directory.CreateDirectory(backgroundOutputDir);

using var phase373 = JsonDocument.Parse(File.ReadAllText(Phase373SummaryPath));
bool phase373ConventionCandidatePresent =
    JsonBool(phase373.RootElement, "massPsiStiffnessOperatorConventionRepairAuditPassed") &&
    JsonBool(phase373.RootElement, "meshVolumeMassPsiMaterialized") &&
    JsonBool(phase373.RootElement, "stiffnessMatrixConventionCandidateMaterialized") &&
    JsonBool(phase373.RootElement, "weightedOperatorConventionCandidateMaterialized") &&
    JsonBool(phase373.RootElement, "symmetricRepresentativeConventionCandidateMaterialized");
bool phase373SyntheticBReplayQualityPassed =
    JsonBool(phase373.RootElement, "matchingWeightedModeReplayQualityPassed");

var spinorSpec = JsonSerializer.Deserialize<SpinorRepresentationSpec>(
    File.ReadAllText(SpinorRepresentationPath),
    options) ?? throw new InvalidDataException($"Failed to deserialize {SpinorRepresentationPath}.");
var mesh = ToyGeometryFactory.CreateStructuredFiberBundle2D(rows: 2, cols: 2).AmbientMesh;
int dofsPerVertex = spinorSpec.SpinorComponents * 3;
double[] meshWeights = MassPsiWeightsBuilder.BuildFromMesh(mesh, dofsPerVertex);
double[] identityWeights = MassPsiWeightsBuilder.BuildIdentity(mesh.VertexCount, dofsPerVertex);
bool meshVolumeMassPsiMaterialized =
    meshWeights.Length > 0 &&
    meshWeights.All(weight => double.IsFinite(weight) && weight > 0.0);
bool meshVolumeMassPsiNonuniform = meshWeights.Distinct().Skip(1).Any();

var provenance = new ProvenanceMeta
{
    CreatedAt = DateTimeOffset.UtcNow,
    CodeRevision = "phase374-shared-weighted-fermion-spectral-solver-repair-audit",
    Branch = new() { BranchId = "phase374-shared-solver-repair-audit", SchemaVersion = "1.0" },
    Backend = "cpu-reference",
};
var solver = new FermionSpectralSolver(new CpuDiracOperatorAssembler());
var backgroundAudits = Directory
    .GetFiles(FermionDir, "dirac_bundle_*.json")
    .Where(path => !path.EndsWith(".matrix.json", StringComparison.Ordinal))
    .OrderBy(path => path, StringComparer.Ordinal)
    .Select(BuildBackgroundAudit)
    .ToArray();
var syntheticLayout = FermionFieldLayoutFactory.BuildStandardLayout(
    "layout-phase374-synthetic-nonzero",
    spinorSpec,
    gaugeDimension: 1,
    provenance,
    insertedAssumptionIds: ["phase374-discrete-solver-diagnostic-only"]);
var syntheticNonzeroBenchmark = BuildSyntheticNonzeroBenchmark(syntheticLayout);

int backgroundCount = backgroundAudits.Length;
int weightedModeCount = backgroundAudits.Sum(row => row.WeightedSolve.ModeCount);
int phase12SelectedWeightedNonzeroModeCount = backgroundAudits.Sum(row => row.WeightedSolve.NonzeroModeCount);
bool phase12SelectedWeightedModesAreKernelOnly = phase12SelectedWeightedNonzeroModeCount == 0;
int weightedGeneralizedResidualPassedCount = backgroundAudits.Sum(row => row.WeightedSolve.GeneralizedResidualPassedCount);
int weightedMNormalizationPassedCount = backgroundAudits.Sum(row => row.WeightedSolve.NormalizationPassedCount);
int weightedMOrthonormalityPassedBackgroundCount = backgroundAudits.Count(row => row.WeightedSolve.OrthonormalityPassed);
int identityRegressionPassedCount = backgroundAudits.Count(row => row.IdentityRegression.Passed);
double maxWeightedGeneralizedRelativeResidual = backgroundAudits.Max(row => row.WeightedSolve.MaxGeneralizedRelativeResidual);
double maxWeightedMNormResidual = backgroundAudits.Max(row => row.WeightedSolve.MaxNormalizationResidual);
double maxWeightedMOrthonormalityResidual = backgroundAudits.Max(row => row.WeightedSolve.MaxOrthonormalityResidual);
double maxIdentityRegressionEigenvalueResidual = backgroundAudits.Max(row => row.IdentityRegression.MaxEigenvalueScaleAwareResidual);

const bool routeProvidesPhysicalMassPsiCompatibleBranch = false;
const bool routeProvidesCanonicalPhysicalMassPsi = false;
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

bool sharedWeightedFermionSpectralSolverRepairAuditPassed =
    phase373ConventionCandidatePresent &&
    phase373SyntheticBReplayQualityPassed &&
    meshVolumeMassPsiMaterialized &&
    meshVolumeMassPsiNonuniform &&
    backgroundCount == ExpectedBackgroundCount &&
    weightedModeCount == ExpectedBackgroundCount * ExpectedModeCountPerBackground &&
    weightedGeneralizedResidualPassedCount == weightedModeCount &&
    weightedMNormalizationPassedCount == weightedModeCount &&
    weightedMOrthonormalityPassedBackgroundCount == ExpectedBackgroundCount &&
    identityRegressionPassedCount == ExpectedBackgroundCount &&
    syntheticNonzeroBenchmark.Passed &&
    !routeProvidesPhysicalMassPsiCompatibleBranch &&
    !routeProvidesCanonicalPhysicalMassPsi &&
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

string terminalStatus = sharedWeightedFermionSpectralSolverRepairAuditPassed
    ? "shared-weighted-fermion-spectral-solver-repair-validated-discrete-only"
    : "shared-weighted-fermion-spectral-solver-repair-audit-blocked";
string decision = sharedWeightedFermionSpectralSolverRepairAuditPassed
    ? "The repaired shared solver passes the Phase12 mesh-weighted stiffness-matrix kernel benchmark, identity-weight regression, and an independent nonuniform-M_psi nonzero-spectrum benchmark. Treat this as a discrete solver repair only; no physical GU fermionic branch or boson source law is supplied."
    : "Do not promote the shared solver repair until Phase373 synthetic-B replay, mesh-weighted generalized residuals, M_psi orthonormality, identity-weight regression, and the independent nonzero-spectrum benchmark all pass.";
var predictionContractImpact = new
{
    canFillPhase201WzContract,
    canFillPhase201HiggsContract,
    canFillPhase256ObservedFieldExtractionContract,
    phase201FieldsDefensiblyFilled = Array.Empty<string>(),
};
var result = new
{
    phaseId = "phase374-shared-weighted-fermion-spectral-solver-repair-audit",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    sharedWeightedFermionSpectralSolverRepairAuditPassed,
    phase373ConventionCandidatePresent,
    phase373SyntheticBReplayQualityPassed,
    meshVolumeMassPsiMaterialized,
    meshVolumeMassPsiNonuniform,
    backgroundCount,
    weightedModeCount,
    phase12SelectedWeightedNonzeroModeCount,
    phase12SelectedWeightedModesAreKernelOnly,
    weightedGeneralizedResidualPassedCount,
    weightedMNormalizationPassedCount,
    weightedMOrthonormalityPassedBackgroundCount,
    identityRegressionPassedCount,
    maxWeightedGeneralizedRelativeResidual,
    maxWeightedMNormResidual,
    maxWeightedMOrthonormalityResidual,
    maxIdentityRegressionEigenvalueResidual,
    syntheticNonzeroWeightedBenchmarkPassed = syntheticNonzeroBenchmark.Passed,
    syntheticWeightedModeCount = syntheticNonzeroBenchmark.WeightedSolve.ModeCount,
    syntheticWeightedNonzeroModeCount = syntheticNonzeroBenchmark.WeightedSolve.NonzeroModeCount,
    syntheticWeightedGeneralizedResidualPassedCount = syntheticNonzeroBenchmark.WeightedSolve.GeneralizedResidualPassedCount,
    syntheticWeightedMNormalizationPassedCount = syntheticNonzeroBenchmark.WeightedSolve.NormalizationPassedCount,
    syntheticWeightedMOrthonormalityPassed = syntheticNonzeroBenchmark.WeightedSolve.OrthonormalityPassed,
    syntheticIdentityRegressionPassed = syntheticNonzeroBenchmark.IdentityRegression.Passed,
    maxSyntheticWeightedGeneralizedRelativeResidual = syntheticNonzeroBenchmark.WeightedSolve.MaxGeneralizedRelativeResidual,
    maxSyntheticWeightedMOrthonormalityResidual = syntheticNonzeroBenchmark.WeightedSolve.MaxOrthonormalityResidual,
    tolerances = new
    {
        residualTolerance = ResidualTolerance,
        normalizationTolerance = NormalizationTolerance,
        orthonormalityTolerance = OrthonormalityTolerance,
        identityRegressionTolerance = IdentityRegressionTolerance,
    },
    meshVolumeMassPsi = new
    {
        builder = "MassPsiWeightsBuilder.BuildFromMesh(mesh, spinorComponents * dimG)",
        realWeightCount = meshWeights.Length,
        minimumWeight = meshWeights.Min(),
        maximumWeight = meshWeights.Max(),
        distinctWeights = meshWeights.Distinct().Order().ToArray(),
    },
    backgroundAudits,
    syntheticNonzeroBenchmark,
    routeProvidesPhysicalMassPsiCompatibleBranch,
    routeProvidesCanonicalPhysicalMassPsi,
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
    decision,
};
Directory.CreateDirectory(outputDir);
string fullPath = Path.Combine(outputDir, "shared_weighted_fermion_spectral_solver_repair_audit.json");
string summaryPath = Path.Combine(outputDir, "shared_weighted_fermion_spectral_solver_repair_audit_summary.json");
File.WriteAllText(fullPath, JsonSerializer.Serialize(result, options));
File.WriteAllText(summaryPath, JsonSerializer.Serialize(new
{
    result.phaseId,
    result.generatedAt,
    terminalStatus,
    sharedWeightedFermionSpectralSolverRepairAuditPassed,
    phase373ConventionCandidatePresent,
    phase373SyntheticBReplayQualityPassed,
    meshVolumeMassPsiMaterialized,
    meshVolumeMassPsiNonuniform,
    backgroundCount,
    weightedModeCount,
    phase12SelectedWeightedNonzeroModeCount,
    phase12SelectedWeightedModesAreKernelOnly,
    weightedGeneralizedResidualPassedCount,
    weightedMNormalizationPassedCount,
    weightedMOrthonormalityPassedBackgroundCount,
    identityRegressionPassedCount,
    maxWeightedGeneralizedRelativeResidual,
    maxWeightedMNormResidual,
    maxWeightedMOrthonormalityResidual,
    maxIdentityRegressionEigenvalueResidual,
    syntheticNonzeroWeightedBenchmarkPassed = syntheticNonzeroBenchmark.Passed,
    syntheticWeightedModeCount = syntheticNonzeroBenchmark.WeightedSolve.ModeCount,
    syntheticWeightedNonzeroModeCount = syntheticNonzeroBenchmark.WeightedSolve.NonzeroModeCount,
    syntheticWeightedGeneralizedResidualPassedCount = syntheticNonzeroBenchmark.WeightedSolve.GeneralizedResidualPassedCount,
    syntheticWeightedMNormalizationPassedCount = syntheticNonzeroBenchmark.WeightedSolve.NormalizationPassedCount,
    syntheticWeightedMOrthonormalityPassed = syntheticNonzeroBenchmark.WeightedSolve.OrthonormalityPassed,
    syntheticIdentityRegressionPassed = syntheticNonzeroBenchmark.IdentityRegression.Passed,
    maxSyntheticWeightedGeneralizedRelativeResidual = syntheticNonzeroBenchmark.WeightedSolve.MaxGeneralizedRelativeResidual,
    maxSyntheticWeightedMOrthonormalityResidual = syntheticNonzeroBenchmark.WeightedSolve.MaxOrthonormalityResidual,
    routeProvidesPhysicalMassPsiCompatibleBranch,
    routeProvidesCanonicalPhysicalMassPsi,
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
    decision,
}, options));
Console.WriteLine(terminalStatus);
Console.WriteLine($"sharedWeightedFermionSpectralSolverRepairAuditPassed={sharedWeightedFermionSpectralSolverRepairAuditPassed}");
Console.WriteLine($"phase373SyntheticBReplayQualityPassed={phase373SyntheticBReplayQualityPassed}");
Console.WriteLine($"backgroundCount={backgroundCount}");
Console.WriteLine($"weightedGeneralizedResidualPassedCount={weightedGeneralizedResidualPassedCount}/{weightedModeCount}");
Console.WriteLine($"phase12SelectedWeightedNonzeroModeCount={phase12SelectedWeightedNonzeroModeCount}/{weightedModeCount}");
Console.WriteLine($"weightedMNormalizationPassedCount={weightedMNormalizationPassedCount}/{weightedModeCount}");
Console.WriteLine($"weightedMOrthonormalityPassedBackgroundCount={weightedMOrthonormalityPassedBackgroundCount}/{backgroundCount}");
Console.WriteLine($"identityRegressionPassedCount={identityRegressionPassedCount}/{backgroundCount}");
Console.WriteLine($"maxWeightedGeneralizedRelativeResidual={maxWeightedGeneralizedRelativeResidual:R}");
Console.WriteLine($"maxWeightedMOrthonormalityResidual={maxWeightedMOrthonormalityResidual:R}");
Console.WriteLine($"syntheticNonzeroWeightedBenchmarkPassed={syntheticNonzeroBenchmark.Passed}");
Console.WriteLine($"syntheticWeightedNonzeroModeCount={syntheticNonzeroBenchmark.WeightedSolve.NonzeroModeCount}/{syntheticNonzeroBenchmark.WeightedSolve.ModeCount}");
Console.WriteLine($"maxSyntheticWeightedGeneralizedRelativeResidual={syntheticNonzeroBenchmark.WeightedSolve.MaxGeneralizedRelativeResidual:R}");
Console.WriteLine($"summaryPath={summaryPath}");

BackgroundAudit BuildBackgroundAudit(string metadataPath)
{
    using var metadataDoc = JsonDocument.Parse(File.ReadAllText(metadataPath));
    var metadata = metadataDoc.RootElement;
    string backgroundId = RequiredString(metadata, "fermionBackgroundId");
    string layoutPath = Path.Combine(FermionDir, $"layout_{backgroundId}.json");
    var layout = JsonSerializer.Deserialize<FermionFieldLayout>(File.ReadAllText(layoutPath), options)
        ?? throw new InvalidDataException($"Failed to deserialize {layoutPath}.");
    string matrixPath = Path.Combine(FermionDir, RequiredString(metadata, "explicitMatrixRef"));
    double[] k = JsonSerializer.Deserialize<double[]>(File.ReadAllText(matrixPath))
        ?? throw new InvalidDataException($"Failed to deserialize {matrixPath}.");
    var bundle = BuildBundle(backgroundId, layout.LayoutId, metadata, k, provenance);
    var weighted = solver.Solve(bundle, layout, BuildConfig(meshWeights), provenance);
    var identity = solver.Solve(bundle, layout, BuildConfig(identityWeights), provenance);
    var unweighted = solver.Solve(bundle, layout, BuildConfig(null), provenance);
    var weightedAudit = EvaluateSolve(weighted, k, meshWeights);
    var identityAudit = EvaluateSolve(identity, k, identityWeights);
    var unweightedAudit = EvaluateSolve(unweighted, k, null);
    var identityRegression = BuildIdentityRegression(identityAudit, unweightedAudit);
    string sidecarPath = Path.Combine(backgroundOutputDir, $"{backgroundId}-shared-weighted-solver-repair.json");
    var audit = new BackgroundAudit
    {
        FermionBackgroundId = backgroundId,
        BaseDiracMetadataPath = metadataPath,
        BaseDiracMatrixPath = matrixPath,
        WeightedSolve = weightedAudit,
        IdentityWeightedSolve = identityAudit,
        UnweightedSolve = unweightedAudit,
        IdentityRegression = identityRegression,
        SidecarPath = sidecarPath,
    };
    File.WriteAllText(sidecarPath, JsonSerializer.Serialize(audit, options));
    return audit;
}

SyntheticBenchmarkAudit BuildSyntheticNonzeroBenchmark(FermionFieldLayout layout)
{
    double[] k = InterleaveRealMatrix(new double[,]
    {
        { 6.0, 1.0, 0.5, 0.0 },
        { 1.0, 4.0, -0.25, 0.75 },
        { 0.5, -0.25, 3.0, 1.25 },
        { 0.0, 0.75, 1.25, 2.0 },
    });
    double[] weights = [1.0, 1.0, 2.0, 2.0, 4.0, 4.0, 0.5, 0.5];
    double[] identity = Enumerable.Repeat(1.0, weights.Length).ToArray();
    var bundle = new DiracOperatorBundle
    {
        OperatorId = "phase374-synthetic-nonzero-stiffness-k",
        FermionBackgroundId = "phase374-synthetic-nonzero",
        LayoutId = layout.LayoutId,
        SpinConnectionId = "phase374-synthetic-nonzero",
        MatrixShape = [4, 4],
        HasExplicitMatrix = true,
        ExplicitMatrix = k,
        IsHermitian = true,
        HermiticityResidual = 0.0,
        HermiticityTolerance = 1e-12,
        MassBranchTermIncluded = false,
        CorrectionTermIncluded = false,
        GaugeReductionApplied = false,
        CellCount = 1,
        DofsPerCell = layout.PrimalDofsPerCell,
        DiagnosticNotes = ["Independent nonuniform-M_psi nonzero-spectrum shared-solver benchmark."],
        Provenance = provenance,
    };
    var weighted = EvaluateSolve(solver.Solve(bundle, layout, BuildConfig(weights, ExpectedSyntheticModeCount), provenance), k, weights);
    var identityWeighted = EvaluateSolve(solver.Solve(bundle, layout, BuildConfig(identity, ExpectedSyntheticModeCount), provenance), k, identity);
    var unweighted = EvaluateSolve(solver.Solve(bundle, layout, BuildConfig(null, ExpectedSyntheticModeCount), provenance), k, null);
    var identityRegression = BuildIdentityRegression(identityWeighted, unweighted);
    return new()
    {
        BenchmarkId = "phase374-synthetic-nonuniform-mpsi-nonzero-spectrum",
        StiffnessMatrixK = k,
        MassPsiWeights = weights,
        WeightedSolve = weighted,
        IdentityWeightedSolve = identityWeighted,
        UnweightedSolve = unweighted,
        IdentityRegression = identityRegression,
        Passed =
            weighted.ModeCount == ExpectedSyntheticModeCount &&
            weighted.NonzeroModeCount == ExpectedSyntheticModeCount &&
            weighted.GeneralizedResidualPassedCount == ExpectedSyntheticModeCount &&
            weighted.NormalizationPassedCount == ExpectedSyntheticModeCount &&
            weighted.OrthonormalityPassed &&
            identityRegression.Passed,
    };
}

static DiracOperatorBundle BuildBundle(
    string backgroundId,
    string layoutId,
    JsonElement metadata,
    double[] explicitMatrix,
    ProvenanceMeta provenance) => new()
{
    OperatorId = $"phase374-stiffness-k-{backgroundId}",
    FermionBackgroundId = backgroundId,
    LayoutId = layoutId,
    SpinConnectionId = $"phase374-stiffness-k-{backgroundId}",
    MatrixShape = metadata.GetProperty("matrixShape").EnumerateArray().Select(value => value.GetInt32()).ToArray(),
    HasExplicitMatrix = true,
    ExplicitMatrix = explicitMatrix,
    IsHermitian = metadata.GetProperty("isHermitian").GetBoolean(),
    HermiticityResidual = metadata.GetProperty("hermiticityResidual").GetDouble(),
    HermiticityTolerance = metadata.GetProperty("hermiticityTolerance").GetDouble(),
    MassBranchTermIncluded = metadata.GetProperty("massBranchTermIncluded").GetBoolean(),
    CorrectionTermIncluded = metadata.GetProperty("correctionTermIncluded").GetBoolean(),
    GaugeReductionApplied = metadata.GetProperty("gaugeReductionApplied").GetBoolean(),
    CellCount = metadata.GetProperty("cellCount").GetInt32(),
    DofsPerCell = metadata.GetProperty("dofsPerCell").GetInt32(),
    DiagnosticNotes = ["Phase374 shared weighted solver stiffness-matrix K benchmark."],
    Provenance = provenance,
};

static FermionSpectralConfig BuildConfig(double[]? weights, int modeCount = ExpectedModeCountPerBackground) => new()
{
    TargetRegion = "lowest-magnitude",
    ModeCount = modeCount,
    GaugeReduction = false,
    NullspaceDeflation = false,
    ConvergenceTolerance = 1e-10,
    MaxIterations = 1000,
    Seed = 42,
    MassPsiWeights = weights,
};

static SolveAudit EvaluateSolve(FermionSpectralResult solve, double[] k, double[]? weights)
{
    var modes = solve.Modes.Select(mode =>
    {
        double[] psi = mode.EigenvectorCoefficients
            ?? throw new InvalidDataException($"Mode {mode.ModeId} has no eigenvector coefficients.");
        double[] kPsi = ApplyComplexMatrix(k, psi);
        double[] mPsi = weights is null ? psi.ToArray() : ApplyWeights(psi, weights);
        double[] lambdaMPsi = Scale(mPsi, mode.EigenvalueRe);
        double generalizedResidual = RelativeVectorResidual(kPsi, lambdaMPsi);
        double norm = Math.Sqrt(ComplexInnerProductReal(psi, psi, weights));
        return new ModeAudit
        {
            ModeId = mode.ModeId,
            ModeIndex = mode.ModeIndex,
            Eigenvalue = mode.EigenvalueRe,
            SolverReportedResidualNorm = mode.ResidualNorm,
            GeneralizedRelativeResidual = generalizedResidual,
            GeneralizedResidualPassed = generalizedResidual <= ResidualTolerance,
            Norm = norm,
            NormalizationResidual = Math.Abs(norm - 1.0),
            NormalizationPassed = Math.Abs(norm - 1.0) <= NormalizationTolerance,
            Coefficients = psi,
        };
    }).ToArray();
    double maxOrthResidual = 0.0;
    for (int left = 0; left < modes.Length; left++)
        for (int right = 0; right < modes.Length; right++)
        {
            double expected = left == right ? 1.0 : 0.0;
            maxOrthResidual = Math.Max(
                maxOrthResidual,
                Math.Abs(ComplexInnerProductReal(modes[left].Coefficients, modes[right].Coefficients, weights) - expected));
        }
    return new()
    {
        SolverName = solve.Diagnostics.SolverName,
        SolverNotes = solve.Diagnostics.Notes,
        ModeCount = modes.Length,
        NonzeroModeCount = modes.Count(mode => Math.Abs(mode.Eigenvalue) > 1e-12),
        GeneralizedResidualPassedCount = modes.Count(mode => mode.GeneralizedResidualPassed),
        NormalizationPassedCount = modes.Count(mode => mode.NormalizationPassed),
        MaxGeneralizedRelativeResidual = modes.Max(mode => mode.GeneralizedRelativeResidual),
        MaxNormalizationResidual = modes.Max(mode => mode.NormalizationResidual),
        MaxOrthonormalityResidual = maxOrthResidual,
        OrthonormalityPassed = maxOrthResidual <= OrthonormalityTolerance,
        Modes = modes,
    };
}

static IdentityRegression BuildIdentityRegression(SolveAudit identity, SolveAudit unweighted)
{
    if (identity.ModeCount != unweighted.ModeCount)
        throw new InvalidDataException("Identity-weight and unweighted mode counts must match.");
    double maxEigenvalueResidual = identity.Modes
        .Zip(unweighted.Modes, (left, right) => ScaleAwareResidual(left.Eigenvalue, right.Eigenvalue))
        .DefaultIfEmpty()
        .Max();
    return new()
    {
        MaxEigenvalueScaleAwareResidual = maxEigenvalueResidual,
        Passed =
            maxEigenvalueResidual <= IdentityRegressionTolerance &&
            identity.GeneralizedResidualPassedCount == identity.ModeCount &&
            unweighted.GeneralizedResidualPassedCount == unweighted.ModeCount &&
            identity.OrthonormalityPassed &&
            unweighted.OrthonormalityPassed,
    };
}

static double[] ApplyComplexMatrix(double[] matrix, double[] vector)
{
    int size = vector.Length / 2;
    if (matrix.Length != 2 * size * size)
        throw new InvalidDataException("Matrix and vector dimensions must match.");
    var result = new double[vector.Length];
    for (int row = 0; row < size; row++)
        for (int col = 0; col < size; col++)
        {
            int matrixIndex = 2 * (row * size + col);
            double re = matrix[matrixIndex];
            double im = matrix[matrixIndex + 1];
            result[2 * row] += re * vector[2 * col] - im * vector[2 * col + 1];
            result[2 * row + 1] += re * vector[2 * col + 1] + im * vector[2 * col];
        }
    return result;
}

static double[] InterleaveRealMatrix(double[,] matrix)
{
    int rowCount = matrix.GetLength(0);
    int columnCount = matrix.GetLength(1);
    if (rowCount != columnCount)
        throw new InvalidDataException("Synthetic stiffness matrix must be square.");
    var interleaved = new double[2 * rowCount * columnCount];
    for (int row = 0; row < rowCount; row++)
        for (int column = 0; column < columnCount; column++)
            interleaved[2 * (row * columnCount + column)] = matrix[row, column];
    return interleaved;
}

static double ComplexInnerProductReal(double[] left, double[] right, double[]? weights)
{
    double result = 0.0;
    for (int index = 0; index < left.Length; index += 2)
    {
        double weight = weights is null ? 1.0 : weights[index];
        result += weight * (left[index] * right[index] + left[index + 1] * right[index + 1]);
    }
    return result;
}

static double[] ApplyWeights(double[] vector, double[] weights) =>
    vector.Zip(weights, (value, weight) => value * weight).ToArray();

static double[] Scale(double[] vector, double scale) =>
    vector.Select(value => scale * value).ToArray();

static double RelativeVectorResidual(double[] left, double[] right)
{
    double residual = Math.Sqrt(left.Zip(right, (l, r) => Square(l - r)).Sum());
    double leftNorm = Math.Sqrt(left.Sum(Square));
    double rightNorm = Math.Sqrt(right.Sum(Square));
    return residual / Math.Max(1e-300, Math.Max(leftNorm, rightNorm));
}

static double ScaleAwareResidual(double left, double right) =>
    Math.Abs(left - right) / Math.Max(1.0, Math.Max(Math.Abs(left), Math.Abs(right)));

static double Square(double value) => value * value;

static bool JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) &&
    (property.ValueKind == JsonValueKind.True || property.ValueKind == JsonValueKind.False) &&
    property.GetBoolean();

static string RequiredString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
        ? value.GetString() ?? throw new InvalidDataException($"{propertyName} must not be null.")
        : throw new InvalidDataException($"{propertyName} must be a string.");

static JsonSerializerOptions JsonOptions() => new()
{
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true,
};

public sealed class BackgroundAudit
{
    public required string FermionBackgroundId { get; init; }
    public required string BaseDiracMetadataPath { get; init; }
    public required string BaseDiracMatrixPath { get; init; }
    public required SolveAudit WeightedSolve { get; init; }
    public required SolveAudit IdentityWeightedSolve { get; init; }
    public required SolveAudit UnweightedSolve { get; init; }
    public required IdentityRegression IdentityRegression { get; init; }
    public required string SidecarPath { get; init; }
}

public sealed class SolveAudit
{
    public required string SolverName { get; init; }
    public required IReadOnlyList<string> SolverNotes { get; init; }
    public required int ModeCount { get; init; }
    public required int NonzeroModeCount { get; init; }
    public required int GeneralizedResidualPassedCount { get; init; }
    public required int NormalizationPassedCount { get; init; }
    public required double MaxGeneralizedRelativeResidual { get; init; }
    public required double MaxNormalizationResidual { get; init; }
    public required double MaxOrthonormalityResidual { get; init; }
    public required bool OrthonormalityPassed { get; init; }
    public required IReadOnlyList<ModeAudit> Modes { get; init; }
}

public sealed class SyntheticBenchmarkAudit
{
    public required string BenchmarkId { get; init; }
    public required double[] StiffnessMatrixK { get; init; }
    public required double[] MassPsiWeights { get; init; }
    public required SolveAudit WeightedSolve { get; init; }
    public required SolveAudit IdentityWeightedSolve { get; init; }
    public required SolveAudit UnweightedSolve { get; init; }
    public required IdentityRegression IdentityRegression { get; init; }
    public required bool Passed { get; init; }
}

public sealed class ModeAudit
{
    public required string ModeId { get; init; }
    public required int ModeIndex { get; init; }
    public required double Eigenvalue { get; init; }
    public required double SolverReportedResidualNorm { get; init; }
    public required double GeneralizedRelativeResidual { get; init; }
    public required bool GeneralizedResidualPassed { get; init; }
    public required double Norm { get; init; }
    public required double NormalizationResidual { get; init; }
    public required bool NormalizationPassed { get; init; }
    public required double[] Coefficients { get; init; }
}

public sealed class IdentityRegression
{
    public required double MaxEigenvalueScaleAwareResidual { get; init; }
    public required bool Passed { get; init; }
}
