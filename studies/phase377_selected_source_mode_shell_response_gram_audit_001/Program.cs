using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

const string DefaultOutputDir = "studies/phase377_selected_source_mode_shell_response_gram_audit_001/output";
const string Phase376FullPath = "studies/phase376_persisted_nonzero_shell_reciprocal_replay_audit_001/output/persisted_nonzero_shell_reciprocal_replay_audit.json";
const string Phase376SummaryPath = "studies/phase376_persisted_nonzero_shell_reciprocal_replay_audit_001/output/persisted_nonzero_shell_reciprocal_replay_audit_summary.json";
const string ModeDir = "studies/phase12_joined_calculation_001/output/background_family/spectra/modes";
const int ExpectedBackgroundCount = 2;
const int ExpectedVariationCount = 24;
const int ExpectedSelectedSourceModeCountPerBackground = 12;
const int ExpectedShellDimension = 4;
const int ExpectedConnectionVectorLength = 156;
const int ExpectedObservedResponseRank = 3;
const double MatrixTolerance = 1e-8;
const double SymmetryTolerance = 1e-12;
const double SourceOrthonormalityTolerance = 1e-12;
const double NonzeroResponseTolerance = 1e-12;

var outputDir = Environment.GetEnvironmentVariable("PHASE377_OUTPUT_DIR") ?? DefaultOutputDir;
var backgroundOutputDir = Path.Combine(outputDir, "backgrounds");
Directory.CreateDirectory(backgroundOutputDir);

using var phase376Full = JsonDocument.Parse(File.ReadAllText(Phase376FullPath));
using var phase376Summary = JsonDocument.Parse(File.ReadAllText(Phase376SummaryPath));
var p376 = phase376Summary.RootElement;
bool phase376DiscreteShellReplayPresent =
    JsonBool(p376, "persistedNonzeroShellReciprocalReplayAuditPassed") &&
    JsonInt(p376, "backgroundCount") == ExpectedBackgroundCount &&
    JsonInt(p376, "variationCount") == ExpectedVariationCount &&
    JsonInt(p376, "variationPassedCount") == ExpectedVariationCount;
bool phase376NonpromotionalBoundaryVerified =
    FalseFlag(p376, "routeProvidesPhysicalGuBranch") &&
    FalseFlag(p376, "routeProvidesCanonicalPhysicalMassPsi") &&
    FalseFlag(p376, "routeProvidesCompletedFermionicAction") &&
    FalseFlag(p376, "routeProvidesCompletedMixedLinearizationBlocks") &&
    FalseFlag(p376, "routeProvidesDirectTargetIndependentWzBridgeSourceLaw") &&
    FalseFlag(p376, "routeProvidesHiggsScalarSourceOperator") &&
    FalseFlag(p376, "routeProvidesGeVUnitNormalization") &&
    FalseFlag(p376, "routeCompletesBosonPredictions");

var variationRows = phase376Full.RootElement
    .GetProperty("variationAudits")
    .EnumerateArray()
    .Select(ParseVariation)
    .ToArray();
var backgroundAudits = variationRows
    .GroupBy(row => row.FermionBackgroundId, StringComparer.Ordinal)
    .OrderBy(group => group.Key, StringComparer.Ordinal)
    .Select(group => BuildBackgroundAudit(group.Key, group.OrderBy(row => row.VariationId, StringComparer.Ordinal).ToArray()))
    .ToArray();

int backgroundCount = backgroundAudits.Length;
int selectedSourceModeCount = backgroundAudits.Sum(row => row.SelectedSourceModeCount);
int variationCount = variationRows.Length;
int backgroundPassedCount = backgroundAudits.Count(row => row.BackgroundPassed);
int sourceModeOrthonormalityPassedCount = backgroundAudits.Count(row => row.SourceModeOrthonormalityPassed);
int persistedResponseGramPsdPassedCount = backgroundAudits.Count(row => row.PersistedResponseGram.PsdPassed);
int analyticResponseGramPsdPassedCount = backgroundAudits.Count(row => row.AnalyticResponseGram.PsdPassed);
int persistedResponseNonzeroPassedCount = backgroundAudits.Count(row => row.PersistedResponseGram.NonzeroResponsePassed);
int analyticResponseNonzeroPassedCount = backgroundAudits.Count(row => row.AnalyticResponseGram.NonzeroResponsePassed);
int persistedDftShellBasisInvariancePassedCount = backgroundAudits.Count(row => row.PersistedResponseGram.DftShellBasisInvariancePassed);
int analyticDftShellBasisInvariancePassedCount = backgroundAudits.Count(row => row.AnalyticResponseGram.DftShellBasisInvariancePassed);
int persistedDiagonalBlockNormIdentityPassedCount = backgroundAudits.Count(row => row.PersistedResponseGram.DiagonalBlockNormIdentityPassed);
int analyticDiagonalBlockNormIdentityPassedCount = backgroundAudits.Count(row => row.AnalyticResponseGram.DiagonalBlockNormIdentityPassed);
int persistedAnalyticResponseGramParityPassedCount = backgroundAudits.Count(row => row.PersistedAnalyticResponseGramParityPassed);
int observedResponseRankThreeCount = backgroundAudits.Count(row => row.PersistedResponseGram.PositiveResponseRank == ExpectedObservedResponseRank);
bool stableObservedResponseRankAcrossBackgrounds =
    backgroundAudits.Select(row => row.PersistedResponseGram.PositiveResponseRank).Distinct().Count() == 1;
int observedStableResponseRank = stableObservedResponseRankAcrossBackgrounds
    ? backgroundAudits[0].PersistedResponseGram.PositiveResponseRank
    : -1;
double maxSourceModeGramDiagonalResidual = backgroundAudits.Max(row => row.MaxSourceModeGramDiagonalResidual);
double maxSourceModeGramOffDiagonalMagnitude = backgroundAudits.Max(row => row.MaxSourceModeGramOffDiagonalMagnitude);
double maxPersistedAnalyticResponseGramRelativeResidual = backgroundAudits.Max(row => row.PersistedAnalyticResponseGramRelativeResidual);
double maxPersistedResponseGramSymmetryRelativeResidual = backgroundAudits.Max(row => row.PersistedResponseGram.SymmetryRelativeResidual);
double maxAnalyticResponseGramSymmetryRelativeResidual = backgroundAudits.Max(row => row.AnalyticResponseGram.SymmetryRelativeResidual);
double maxPersistedDftShellBasisGramRelativeResidual = backgroundAudits.Max(row => row.PersistedResponseGram.DftShellBasisGramRelativeResidual);
double maxAnalyticDftShellBasisGramRelativeResidual = backgroundAudits.Max(row => row.AnalyticResponseGram.DftShellBasisGramRelativeResidual);
double minPersistedResponseGramEigenvalue = backgroundAudits.Min(row => row.PersistedResponseGram.MinimumEigenvalue);
double minAnalyticResponseGramEigenvalue = backgroundAudits.Min(row => row.AnalyticResponseGram.MinimumEigenvalue);
double minPersistedResponseTrace = backgroundAudits.Min(row => row.PersistedResponseGram.Trace);
double minAnalyticResponseTrace = backgroundAudits.Min(row => row.AnalyticResponseGram.Trace);

const bool targetBlindConstruction = true;
const bool physicalTargetsConsultedForConstruction = false;
const bool selectedSourceModeSubspaceOnly = true;
const bool studyDefinedHilbertSchmidtPullbackMetric = true;
const bool routeProvidesDiscreteShellResponseGramPrecursor = true;
const bool routeProvidesFullConnectionCarrierResponseOperator = false;
const bool routeProvidesPhysicalEffectiveActionHessian = false;
const bool routeProvidesPhysicalGuBranch = false;
const bool routeProvidesPhysicalMassPsiCompatibleBranch = false;
const bool routeProvidesCanonicalPhysicalMassPsi = false;
const bool routeProvidesFixedGuBranch = false;
const bool routeProvidesFixedFermionicOperatorBranch = false;
const bool routeProvidesCompletedFermionicAction = false;
const bool routeProvidesExplicitYukawaFunctional = false;
const bool routeProvidesSolvedYukawaCouplingMap = false;
const bool routeProvidesCoupledResidual = false;
const bool routeProvidesCompletedMixedLinearizationBlocks = false;
const bool routeProvidesCorrectedGaugeIdentities = false;
const bool routeProvidesMixedLinearizationGaugeCompatibilityIdentities = false;
const bool routeProvidesDirectWzBridgeLaw = false;
const bool routeProvidesDirectTargetIndependentWzBridgeSourceLaw = false;
const bool routeProvidesHiggsRow = false;
const bool routeProvidesHiggsScalarSourceOperator = false;
const bool routeProvidesScalarProjectionTheorem = false;
const bool routeProvidesGeVNormalization = false;
const bool routeProvidesGeVUnitNormalization = false;
const bool routeProvidesPredictions = false;
const bool routePromotesWzMasses = false;
const bool routePromotesHiggsMass = false;
const bool routeCompletesBosonPredictions = false;
const bool canFillPhase201WzContract = false;
const bool canFillPhase201HiggsContract = false;
const bool canFillPhase256ObservedFieldExtractionContract = false;

string targetBlindConstructionHash = HashText(string.Join(
    "\n",
    "phase377-selected-source-mode-shell-response-gram-v1",
    $"phase376TargetBlindConstructionHash={JsonString(p376, "targetBlindConstructionHash")}",
    "projectedBlock=G_a=Psi_shell^dagger deltaK[b_a] Psi_shell",
    "responseGram=Q_ab=Re Tr(G_a^dagger G_b)",
    "sourceSubspace=phase376 persisted selected connection-1form source modes only",
    "physicalTargetsConsultedForConstruction=false"));

bool selectedSourceModeShellResponseGramAuditPassed =
    phase376DiscreteShellReplayPresent &&
    phase376NonpromotionalBoundaryVerified &&
    targetBlindConstruction &&
    !physicalTargetsConsultedForConstruction &&
    selectedSourceModeSubspaceOnly &&
    studyDefinedHilbertSchmidtPullbackMetric &&
    routeProvidesDiscreteShellResponseGramPrecursor &&
    backgroundCount == ExpectedBackgroundCount &&
    selectedSourceModeCount == ExpectedBackgroundCount * ExpectedSelectedSourceModeCountPerBackground &&
    variationCount == ExpectedVariationCount &&
    backgroundPassedCount == ExpectedBackgroundCount &&
    sourceModeOrthonormalityPassedCount == ExpectedBackgroundCount &&
    persistedResponseGramPsdPassedCount == ExpectedBackgroundCount &&
    analyticResponseGramPsdPassedCount == ExpectedBackgroundCount &&
    persistedResponseNonzeroPassedCount == ExpectedBackgroundCount &&
    analyticResponseNonzeroPassedCount == ExpectedBackgroundCount &&
    persistedDftShellBasisInvariancePassedCount == ExpectedBackgroundCount &&
    analyticDftShellBasisInvariancePassedCount == ExpectedBackgroundCount &&
    persistedDiagonalBlockNormIdentityPassedCount == ExpectedBackgroundCount &&
    analyticDiagonalBlockNormIdentityPassedCount == ExpectedBackgroundCount &&
    persistedAnalyticResponseGramParityPassedCount == ExpectedBackgroundCount &&
    observedResponseRankThreeCount == ExpectedBackgroundCount &&
    stableObservedResponseRankAcrossBackgrounds &&
    observedStableResponseRank == ExpectedObservedResponseRank &&
    !routeProvidesFullConnectionCarrierResponseOperator &&
    !routeProvidesPhysicalEffectiveActionHessian &&
    !routeProvidesPhysicalGuBranch &&
    !routeProvidesPhysicalMassPsiCompatibleBranch &&
    !routeProvidesCanonicalPhysicalMassPsi &&
    !routeProvidesFixedGuBranch &&
    !routeProvidesFixedFermionicOperatorBranch &&
    !routeProvidesCompletedFermionicAction &&
    !routeProvidesExplicitYukawaFunctional &&
    !routeProvidesSolvedYukawaCouplingMap &&
    !routeProvidesCoupledResidual &&
    !routeProvidesCompletedMixedLinearizationBlocks &&
    !routeProvidesCorrectedGaugeIdentities &&
    !routeProvidesMixedLinearizationGaugeCompatibilityIdentities &&
    !routeProvidesDirectWzBridgeLaw &&
    !routeProvidesDirectTargetIndependentWzBridgeSourceLaw &&
    !routeProvidesHiggsRow &&
    !routeProvidesHiggsScalarSourceOperator &&
    !routeProvidesScalarProjectionTheorem &&
    !routeProvidesGeVNormalization &&
    !routeProvidesGeVUnitNormalization &&
    !routeProvidesPredictions &&
    !routePromotesWzMasses &&
    !routePromotesHiggsMass &&
    !routeCompletesBosonPredictions &&
    !canFillPhase201WzContract &&
    !canFillPhase201HiggsContract &&
    !canFillPhase256ObservedFieldExtractionContract;

string terminalStatus = selectedSourceModeShellResponseGramAuditPassed
    ? "selected-source-mode-shell-response-gram-validated-discrete-only"
    : "selected-source-mode-shell-response-gram-audit-blocked";
string decision = selectedSourceModeShellResponseGramAuditPassed
    ? "Both persisted backgrounds expose a nonzero positive-semidefinite target-blind Hilbert-Schmidt pullback metric on the 12 selected connection-1form source modes. The response image has stable rank 3 and nullity 9. This is a bounded discrete shell-response precursor only: the selected source-mode subspace, action, fermion determinant, gauge reduction, physical branch, observed electroweak projection, Higgs row, and GeV normalization remain missing."
    : "Do not retain the selected-source-mode shell-response Gram precursor until Phase376 inheritance, source-mode orthonormality, positive-semidefinite Gram checks, analytic parity, shell-basis invariance, and explicit physical nonclaims all pass.";
var predictionContractImpact = new
{
    canFillPhase201WzContract,
    canFillPhase201HiggsContract,
    canFillPhase256ObservedFieldExtractionContract,
    phase201FieldsDefensiblyFilled = Array.Empty<string>(),
};
var primarySourceBoundary = new
{
    guDraft = "https://geometricunity.nyc3.digitaloceanspaces.com/Geometric_Unity-Draft-April-1st-2021.pdf",
    degenerateHellmannFeynman = "https://doi.org/10.1103/PhysRevB.68.033105",
    diracDeterminantEffectiveAction = "https://arxiv.org/abs/math-ph/0104011",
    interpretation = "The GU draft motivates a mixed deformation program but does not stabilize the required physical coupled blocks. Alon-Cederbaum supports diagonalizing a derivative operator within an exactly degenerate subspace; Phase377 uses an absolute-value spectral shell and does not claim an energy-slope theorem. Langmann treats a physical fermion-induced gauge action as a regularized Dirac-determinant effective action; Phase377 does not construct that action.",
};
var result = new
{
    phaseId = "phase377-selected-source-mode-shell-response-gram-audit",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    selectedSourceModeShellResponseGramAuditPassed,
    phase376DiscreteShellReplayPresent,
    phase376NonpromotionalBoundaryVerified,
    implementedObjectClassification = "bounded discrete-only selected-source-mode shell-response Hilbert-Schmidt pullback Gram audit",
    physicalInterpretationBoundary = "study-defined discrete response metric only",
    targetBlindConstruction,
    physicalTargetsConsultedForConstruction,
    targetBlindConstructionHash,
    selectedSourceModeSubspaceOnly,
    studyDefinedHilbertSchmidtPullbackMetric,
    routeProvidesDiscreteShellResponseGramPrecursor,
    routeProvidesFullConnectionCarrierResponseOperator,
    routeProvidesPhysicalEffectiveActionHessian,
    conventionDefinitions = new
    {
        projectedBlock = "G_a = Psi_shell^dagger deltaK[b_a] Psi_shell",
        selectedSourceModeResponseGram = "Q_ab = Re Tr(G_a^dagger G_b)",
        responseQuadraticForm = "c^T Q c = ||sum_a c_a G_a||_F^2 >= 0",
        shellBasisTransformation = "G_a -> U^dagger G_a U leaves Q unchanged",
        selectedSourceModeSubspace = "the 12 persisted Phase12 connection-1form source modes per background used by Phase376",
    },
    primarySourceBoundary,
    backgroundCount,
    selectedSourceModeCount,
    variationCount,
    backgroundPassedCount,
    sourceModeOrthonormalityPassedCount,
    persistedResponseGramPsdPassedCount,
    analyticResponseGramPsdPassedCount,
    persistedResponseNonzeroPassedCount,
    analyticResponseNonzeroPassedCount,
    persistedDftShellBasisInvariancePassedCount,
    analyticDftShellBasisInvariancePassedCount,
    persistedDiagonalBlockNormIdentityPassedCount,
    analyticDiagonalBlockNormIdentityPassedCount,
    persistedAnalyticResponseGramParityPassedCount,
    observedResponseRankThreeCount,
    stableObservedResponseRankAcrossBackgrounds,
    observedStableResponseRank,
    observedStableResponseNullity = ExpectedSelectedSourceModeCountPerBackground - observedStableResponseRank,
    maxSourceModeGramDiagonalResidual,
    maxSourceModeGramOffDiagonalMagnitude,
    maxPersistedAnalyticResponseGramRelativeResidual,
    maxPersistedResponseGramSymmetryRelativeResidual,
    maxAnalyticResponseGramSymmetryRelativeResidual,
    maxPersistedDftShellBasisGramRelativeResidual,
    maxAnalyticDftShellBasisGramRelativeResidual,
    minPersistedResponseGramEigenvalue,
    minAnalyticResponseGramEigenvalue,
    minPersistedResponseTrace,
    minAnalyticResponseTrace,
    tolerances = new
    {
        matrixTolerance = MatrixTolerance,
        symmetryTolerance = SymmetryTolerance,
        sourceOrthonormalityTolerance = SourceOrthonormalityTolerance,
        nonzeroResponseTolerance = NonzeroResponseTolerance,
        positiveRankToleranceFormula = "max(1e-14, 1e-10 * max(abs(responseGramEigenvalue)))",
    },
    routeProvidesPhysicalGuBranch,
    routeProvidesPhysicalMassPsiCompatibleBranch,
    routeProvidesCanonicalPhysicalMassPsi,
    routeProvidesFixedGuBranch,
    routeProvidesFixedFermionicOperatorBranch,
    routeProvidesCompletedFermionicAction,
    routeProvidesExplicitYukawaFunctional,
    routeProvidesSolvedYukawaCouplingMap,
    routeProvidesCoupledResidual,
    routeProvidesCompletedMixedLinearizationBlocks,
    routeProvidesCorrectedGaugeIdentities,
    routeProvidesMixedLinearizationGaugeCompatibilityIdentities,
    routeProvidesDirectWzBridgeLaw,
    routeProvidesDirectTargetIndependentWzBridgeSourceLaw,
    routeProvidesHiggsRow,
    routeProvidesHiggsScalarSourceOperator,
    routeProvidesScalarProjectionTheorem,
    routeProvidesGeVNormalization,
    routeProvidesGeVUnitNormalization,
    routeProvidesPredictions,
    routePromotesWzMasses,
    routePromotesHiggsMass,
    routeCompletesBosonPredictions,
    canFillPhase201WzContract,
    canFillPhase201HiggsContract,
    canFillPhase256ObservedFieldExtractionContract,
    predictionContractImpact,
    explicitDiscreteOnlyNonclaims = new[]
    {
        "selected source-mode subspace only",
        "study-defined Hilbert-Schmidt pullback metric only",
        "not a full connection-carrier response operator",
        "not a GU action Hessian",
        "not a regularized fermion-determinant effective-action Hessian",
        "not a gauge-reduced operator",
        "no physical GU branch",
        "no canonical physical M_psi",
        "no direct W/Z bridge law",
        "no Higgs row",
        "no GeV normalization",
        "no predictions",
        "no contract fills",
    },
    backgroundAudits,
    sourceEvidence = new
    {
        phase376FullPath = Phase376FullPath,
        phase376SummaryPath = Phase376SummaryPath,
        modeDir = ModeDir,
    },
    decision,
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
string fullPath = Path.Combine(outputDir, "selected_source_mode_shell_response_gram_audit.json");
string summaryPath = Path.Combine(outputDir, "selected_source_mode_shell_response_gram_audit_summary.json");
File.WriteAllText(fullPath, JsonSerializer.Serialize(result, options));
File.WriteAllText(summaryPath, JsonSerializer.Serialize(new
{
    result.phaseId,
    result.generatedAt,
    terminalStatus,
    selectedSourceModeShellResponseGramAuditPassed,
    phase376DiscreteShellReplayPresent,
    phase376NonpromotionalBoundaryVerified,
    result.implementedObjectClassification,
    result.physicalInterpretationBoundary,
    targetBlindConstruction,
    physicalTargetsConsultedForConstruction,
    targetBlindConstructionHash,
    selectedSourceModeSubspaceOnly,
    studyDefinedHilbertSchmidtPullbackMetric,
    routeProvidesDiscreteShellResponseGramPrecursor,
    routeProvidesFullConnectionCarrierResponseOperator,
    routeProvidesPhysicalEffectiveActionHessian,
    backgroundCount,
    selectedSourceModeCount,
    variationCount,
    backgroundPassedCount,
    sourceModeOrthonormalityPassedCount,
    persistedResponseGramPsdPassedCount,
    analyticResponseGramPsdPassedCount,
    persistedResponseNonzeroPassedCount,
    analyticResponseNonzeroPassedCount,
    persistedDftShellBasisInvariancePassedCount,
    analyticDftShellBasisInvariancePassedCount,
    persistedDiagonalBlockNormIdentityPassedCount,
    analyticDiagonalBlockNormIdentityPassedCount,
    persistedAnalyticResponseGramParityPassedCount,
    observedResponseRankThreeCount,
    stableObservedResponseRankAcrossBackgrounds,
    observedStableResponseRank,
    observedStableResponseNullity = ExpectedSelectedSourceModeCountPerBackground - observedStableResponseRank,
    maxSourceModeGramDiagonalResidual,
    maxSourceModeGramOffDiagonalMagnitude,
    maxPersistedAnalyticResponseGramRelativeResidual,
    maxPersistedResponseGramSymmetryRelativeResidual,
    maxAnalyticResponseGramSymmetryRelativeResidual,
    maxPersistedDftShellBasisGramRelativeResidual,
    maxAnalyticDftShellBasisGramRelativeResidual,
    minPersistedResponseGramEigenvalue,
    minAnalyticResponseGramEigenvalue,
    minPersistedResponseTrace,
    minAnalyticResponseTrace,
    routeProvidesPhysicalGuBranch,
    routeProvidesPhysicalMassPsiCompatibleBranch,
    routeProvidesCanonicalPhysicalMassPsi,
    routeProvidesFixedGuBranch,
    routeProvidesFixedFermionicOperatorBranch,
    routeProvidesCompletedFermionicAction,
    routeProvidesExplicitYukawaFunctional,
    routeProvidesSolvedYukawaCouplingMap,
    routeProvidesCoupledResidual,
    routeProvidesCompletedMixedLinearizationBlocks,
    routeProvidesCorrectedGaugeIdentities,
    routeProvidesMixedLinearizationGaugeCompatibilityIdentities,
    routeProvidesDirectWzBridgeLaw,
    routeProvidesDirectTargetIndependentWzBridgeSourceLaw,
    routeProvidesHiggsRow,
    routeProvidesHiggsScalarSourceOperator,
    routeProvidesScalarProjectionTheorem,
    routeProvidesGeVNormalization,
    routeProvidesGeVUnitNormalization,
    routeProvidesPredictions,
    routePromotesWzMasses,
    routePromotesHiggsMass,
    routeCompletesBosonPredictions,
    canFillPhase201WzContract,
    canFillPhase201HiggsContract,
    canFillPhase256ObservedFieldExtractionContract,
    predictionContractImpact,
    result.explicitDiscreteOnlyNonclaims,
    decision,
}, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"selectedSourceModeShellResponseGramAuditPassed={selectedSourceModeShellResponseGramAuditPassed}");
Console.WriteLine($"backgroundPassedCount={backgroundPassedCount}/{backgroundCount}");
Console.WriteLine($"selectedSourceModeCount={selectedSourceModeCount}/{ExpectedBackgroundCount * ExpectedSelectedSourceModeCountPerBackground}");
Console.WriteLine($"persistedAnalyticResponseGramParityPassedCount={persistedAnalyticResponseGramParityPassedCount}/{backgroundCount}");
Console.WriteLine($"observedStableResponseRank={observedStableResponseRank}");
Console.WriteLine($"observedStableResponseNullity={ExpectedSelectedSourceModeCountPerBackground - observedStableResponseRank}");
Console.WriteLine($"maxPersistedAnalyticResponseGramRelativeResidual={maxPersistedAnalyticResponseGramRelativeResidual:R}");
Console.WriteLine($"minPersistedResponseGramEigenvalue={minPersistedResponseGramEigenvalue:R}");
Console.WriteLine($"summaryPath={summaryPath}");

BackgroundAudit BuildBackgroundAudit(string backgroundId, SourceVariation[] rows)
{
    var modeVectors = rows.Select(row => LoadModeVector(row.BosonModeId)).ToArray();
    var sourceModeGram = RealGram(modeVectors);
    double maxDiagonalResidual = Enumerable.Range(0, rows.Length)
        .Max(index => Math.Abs(sourceModeGram[index][index] - 1.0));
    double maxOffDiagonalMagnitude = Enumerable.Range(0, rows.Length)
        .SelectMany(left => Enumerable.Range(0, rows.Length)
            .Where(right => right != left)
            .Select(right => Math.Abs(sourceModeGram[left][right])))
        .Max();
    bool sourceModeOrthonormalityPassed =
        maxDiagonalResidual <= SourceOrthonormalityTolerance &&
        maxOffDiagonalMagnitude <= SourceOrthonormalityTolerance;
    var persisted = BuildResponseGramAudit(rows.Select(row => row.PersistedBlock).ToArray(), rows.Select(row => row.PersistedFrobeniusNormSquared).ToArray());
    var analytic = BuildResponseGramAudit(rows.Select(row => row.AnalyticBlock).ToArray(), rows.Select(row => row.AnalyticFrobeniusNormSquared).ToArray());
    double parityResidual = MatrixRelativeResidual(persisted.ResponseGram, analytic.ResponseGram);
    bool parityPassed = parityResidual <= MatrixTolerance;
    var audit = new BackgroundAudit
    {
        FermionBackgroundId = backgroundId,
        SelectedSourceModeCount = rows.Length,
        ExpectedSelectedSourceModeCount = ExpectedSelectedSourceModeCountPerBackground,
        VariationIds = rows.Select(row => row.VariationId).ToArray(),
        BosonModeIds = rows.Select(row => row.BosonModeId).ToArray(),
        ConnectionVectorLength = modeVectors[0].Length,
        ExpectedConnectionVectorLength = ExpectedConnectionVectorLength,
        SourceModeGram = sourceModeGram,
        MaxSourceModeGramDiagonalResidual = maxDiagonalResidual,
        MaxSourceModeGramOffDiagonalMagnitude = maxOffDiagonalMagnitude,
        SourceModeOrthonormalityPassed = sourceModeOrthonormalityPassed,
        PersistedResponseGram = persisted,
        AnalyticResponseGram = analytic,
        PersistedAnalyticResponseGramRelativeResidual = parityResidual,
        PersistedAnalyticResponseGramParityPassed = parityPassed,
        BackgroundPassed =
            rows.Length == ExpectedSelectedSourceModeCountPerBackground &&
            modeVectors.All(vector => vector.Length == ExpectedConnectionVectorLength) &&
            rows.All(row =>
                IsExpectedShellBlock(row.PersistedBlock) &&
                IsExpectedShellBlock(row.AnalyticBlock)) &&
            sourceModeOrthonormalityPassed &&
            persisted.PsdPassed &&
            analytic.PsdPassed &&
            persisted.NonzeroResponsePassed &&
            analytic.NonzeroResponsePassed &&
            persisted.DftShellBasisInvariancePassed &&
            analytic.DftShellBasisInvariancePassed &&
            persisted.DiagonalBlockNormIdentityPassed &&
            analytic.DiagonalBlockNormIdentityPassed &&
            parityPassed &&
            persisted.PositiveResponseRank == ExpectedObservedResponseRank &&
            analytic.PositiveResponseRank == ExpectedObservedResponseRank,
    };
    File.WriteAllText(
        Path.Combine(backgroundOutputDir, $"{backgroundId}-shell-response-gram.json"),
        JsonSerializer.Serialize(audit, new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
    return audit;
}

ResponseGramAudit BuildResponseGramAudit(Complex[][][] blocks, double[] expectedDiagonal)
{
    double[][] responseGram = ComplexBlockGram(blocks);
    double symmetryResidual = SymmetryRelativeResidual(responseGram);
    double diagonalResidual = Enumerable.Range(0, responseGram.Length)
        .Max(index => ScaleAwareResidual(responseGram[index][index], expectedDiagonal[index]));
    var transformed = blocks.Select(DftShellBasisTransform).ToArray();
    double dftResidual = MatrixRelativeResidual(responseGram, ComplexBlockGram(transformed));
    double[] eigenvalues = SymmetricEigenvalues(responseGram);
    double maxMagnitude = eigenvalues.Max(value => Math.Abs(value));
    double rankTolerance = Math.Max(1e-14, 1e-10 * maxMagnitude);
    int positiveRank = eigenvalues.Count(value => value > rankTolerance);
    double trace = Enumerable.Range(0, responseGram.Length).Sum(index => responseGram[index][index]);
    return new()
    {
        Construction = "Q_ab = Re Tr(G_a^dagger G_b)",
        ResponseGram = responseGram,
        SymmetryRelativeResidual = symmetryResidual,
        SymmetryPassed = symmetryResidual <= SymmetryTolerance,
        DiagonalBlockNormIdentityRelativeResidual = diagonalResidual,
        DiagonalBlockNormIdentityPassed = diagonalResidual <= MatrixTolerance,
        DftShellBasisGramRelativeResidual = dftResidual,
        DftShellBasisInvariancePassed = dftResidual <= MatrixTolerance,
        EigenvaluesAscending = eigenvalues,
        PositiveRankTolerance = rankTolerance,
        MinimumEigenvalue = eigenvalues[0],
        MaximumEigenvalue = eigenvalues[^1],
        PositiveResponseRank = positiveRank,
        ResponseNullity = responseGram.Length - positiveRank,
        Trace = trace,
        PsdPassed = eigenvalues[0] >= -rankTolerance,
        NonzeroResponsePassed = trace > NonzeroResponseTolerance,
    };
}

SourceVariation ParseVariation(JsonElement row) => new()
{
    VariationId = JsonString(row, "variationId"),
    FermionBackgroundId = JsonString(row, "fermionBackgroundId"),
    BosonModeId = JsonString(row, "bosonModeId"),
    PersistedBlock = ParseBlock(row.GetProperty("persistedBlock").GetProperty("stiffnessProjectedBlock")),
    AnalyticBlock = ParseBlock(row.GetProperty("analyticBlock").GetProperty("stiffnessProjectedBlock")),
    PersistedFrobeniusNormSquared = JsonDouble(row.GetProperty("persistedBlock").GetProperty("metrics"), "frobeniusNormSquared"),
    AnalyticFrobeniusNormSquared = JsonDouble(row.GetProperty("analyticBlock").GetProperty("metrics"), "frobeniusNormSquared"),
};

static Complex[][] ParseBlock(JsonElement element) => element
    .EnumerateArray()
    .Select(row => row.EnumerateArray().Select(value =>
        new Complex(JsonDouble(value, "real"), JsonDouble(value, "imaginary"))).ToArray())
    .ToArray();

static double[] LoadModeVector(string bosonModeId)
{
    using var doc = JsonDocument.Parse(File.ReadAllText(Path.Combine(ModeDir, $"{bosonModeId}.json")));
    return doc.RootElement.GetProperty("modeVector").EnumerateArray().Select(value => value.GetDouble()).ToArray();
}

static bool IsExpectedShellBlock(Complex[][] block) =>
    block.Length == ExpectedShellDimension &&
    block.All(row => row.Length == ExpectedShellDimension);

static double[][] RealGram(double[][] vectors) => vectors
    .Select(left => vectors.Select(right => left.Zip(right, (l, r) => l * r).Sum()).ToArray())
    .ToArray();

static double[][] ComplexBlockGram(Complex[][][] blocks) => blocks
    .Select(left => blocks.Select(right =>
    {
        Complex value = Complex.Zero;
        for (int row = 0; row < left.Length; row++)
            for (int col = 0; col < left[row].Length; col++)
                value += Complex.Conjugate(left[row][col]) * right[row][col];
        return value.Real;
    }).ToArray())
    .ToArray();

static Complex[][] DftShellBasisTransform(Complex[][] block)
{
    int n = block.Length;
    var result = Enumerable.Range(0, n).Select(_ => new Complex[n]).ToArray();
    for (int row = 0; row < n; row++)
        for (int col = 0; col < n; col++)
            for (int left = 0; left < n; left++)
                for (int right = 0; right < n; right++)
                {
                    Complex uLeft = Complex.FromPolarCoordinates(1.0 / Math.Sqrt(n), 2.0 * Math.PI * left * row / n);
                    Complex uRight = Complex.FromPolarCoordinates(1.0 / Math.Sqrt(n), 2.0 * Math.PI * right * col / n);
                    result[row][col] += Complex.Conjugate(uLeft) * block[left][right] * uRight;
                }
    return result;
}

static double[] SymmetricEigenvalues(double[][] input)
{
    int n = input.Length;
    var matrix = input.Select(row => row.ToArray()).ToArray();
    for (int iteration = 0; iteration < 100 * n * n; iteration++)
    {
        int pivotRow = 0;
        int pivotCol = 1;
        double maxOffDiagonal = 0.0;
        for (int row = 0; row < n; row++)
            for (int col = row + 1; col < n; col++)
                if (Math.Abs(matrix[row][col]) > maxOffDiagonal)
                {
                    maxOffDiagonal = Math.Abs(matrix[row][col]);
                    pivotRow = row;
                    pivotCol = col;
                }
        if (maxOffDiagonal <= 1e-16)
            break;
        double angle = 0.5 * Math.Atan2(
            2.0 * matrix[pivotRow][pivotCol],
            matrix[pivotCol][pivotCol] - matrix[pivotRow][pivotRow]);
        double cosine = Math.Cos(angle);
        double sine = Math.Sin(angle);
        for (int index = 0; index < n; index++)
            if (index != pivotRow && index != pivotCol)
            {
                double left = matrix[index][pivotRow];
                double right = matrix[index][pivotCol];
                matrix[index][pivotRow] = matrix[pivotRow][index] = cosine * left - sine * right;
                matrix[index][pivotCol] = matrix[pivotCol][index] = sine * left + cosine * right;
            }
        double diagonalLeft = matrix[pivotRow][pivotRow];
        double diagonalRight = matrix[pivotCol][pivotCol];
        double offDiagonal = matrix[pivotRow][pivotCol];
        matrix[pivotRow][pivotRow] =
            cosine * cosine * diagonalLeft -
            2.0 * sine * cosine * offDiagonal +
            sine * sine * diagonalRight;
        matrix[pivotCol][pivotCol] =
            sine * sine * diagonalLeft +
            2.0 * sine * cosine * offDiagonal +
            cosine * cosine * diagonalRight;
        matrix[pivotRow][pivotCol] = matrix[pivotCol][pivotRow] = 0.0;
    }
    return Enumerable.Range(0, n).Select(index => matrix[index][index]).Order().ToArray();
}

static double SymmetryRelativeResidual(double[][] matrix)
{
    double residual2 = 0.0;
    double norm2 = 0.0;
    for (int row = 0; row < matrix.Length; row++)
        for (int col = 0; col < matrix[row].Length; col++)
        {
            residual2 += Square(matrix[row][col] - matrix[col][row]);
            norm2 += Square(matrix[row][col]);
        }
    return Math.Sqrt(residual2 / Math.Max(norm2, 1e-300));
}

static double MatrixRelativeResidual(double[][] left, double[][] right)
{
    double residual2 = 0.0;
    double norm2 = 0.0;
    for (int row = 0; row < left.Length; row++)
        for (int col = 0; col < left[row].Length; col++)
        {
            residual2 += Square(left[row][col] - right[row][col]);
            norm2 += Square(left[row][col]);
        }
    return Math.Sqrt(residual2 / Math.Max(norm2, 1e-300));
}

static double ScaleAwareResidual(double left, double right) =>
    Math.Abs(left - right) / Math.Max(1.0, Math.Max(Math.Abs(left), Math.Abs(right)));

static double Square(double value) => value * value;

static string HashText(string text) =>
    Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();

static bool JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) &&
    (value.ValueKind == JsonValueKind.True || value.ValueKind == JsonValueKind.False) &&
    value.GetBoolean();

static bool FalseFlag(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) &&
    (value.ValueKind == JsonValueKind.True || value.ValueKind == JsonValueKind.False) &&
    !value.GetBoolean();

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.TryGetInt32(out int result) ? result : null;

static string JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
        ? value.GetString() ?? throw new InvalidDataException($"{propertyName} must not be null.")
        : throw new InvalidDataException($"{propertyName} must be a string.");

static double JsonDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Number
        ? value.GetDouble()
        : throw new InvalidDataException($"{propertyName} must be a number.");

public sealed class SourceVariation
{
    public required string VariationId { get; init; }
    public required string FermionBackgroundId { get; init; }
    public required string BosonModeId { get; init; }
    public required Complex[][] PersistedBlock { get; init; }
    public required Complex[][] AnalyticBlock { get; init; }
    public required double PersistedFrobeniusNormSquared { get; init; }
    public required double AnalyticFrobeniusNormSquared { get; init; }
}

public sealed class ResponseGramAudit
{
    public required string Construction { get; init; }
    public required double[][] ResponseGram { get; init; }
    public required double SymmetryRelativeResidual { get; init; }
    public required bool SymmetryPassed { get; init; }
    public required double DiagonalBlockNormIdentityRelativeResidual { get; init; }
    public required bool DiagonalBlockNormIdentityPassed { get; init; }
    public required double DftShellBasisGramRelativeResidual { get; init; }
    public required bool DftShellBasisInvariancePassed { get; init; }
    public required double[] EigenvaluesAscending { get; init; }
    public required double PositiveRankTolerance { get; init; }
    public required double MinimumEigenvalue { get; init; }
    public required double MaximumEigenvalue { get; init; }
    public required int PositiveResponseRank { get; init; }
    public required int ResponseNullity { get; init; }
    public required double Trace { get; init; }
    public required bool PsdPassed { get; init; }
    public required bool NonzeroResponsePassed { get; init; }
}

public sealed class BackgroundAudit
{
    public required string FermionBackgroundId { get; init; }
    public required int SelectedSourceModeCount { get; init; }
    public required int ExpectedSelectedSourceModeCount { get; init; }
    public required string[] VariationIds { get; init; }
    public required string[] BosonModeIds { get; init; }
    public required int ConnectionVectorLength { get; init; }
    public required int ExpectedConnectionVectorLength { get; init; }
    public required double[][] SourceModeGram { get; init; }
    public required double MaxSourceModeGramDiagonalResidual { get; init; }
    public required double MaxSourceModeGramOffDiagonalMagnitude { get; init; }
    public required bool SourceModeOrthonormalityPassed { get; init; }
    public required ResponseGramAudit PersistedResponseGram { get; init; }
    public required ResponseGramAudit AnalyticResponseGram { get; init; }
    public required double PersistedAnalyticResponseGramRelativeResidual { get; init; }
    public required bool PersistedAnalyticResponseGramParityPassed { get; init; }
    public required bool BackgroundPassed { get; init; }
}
