using System.Text.Json;
using Gu.Core;
using Gu.Geometry;
using Gu.Phase4.Couplings;
using Gu.Phase4.Fermions;
using Gu.Phase4.Spin;

const string DefaultOutputDir = "studies/phase280_direct_bridge_analytic_variation_upgrade_audit_001/output";
const string SpinorRepresentationPath = "studies/phase12_joined_calculation_001/output/background_family/fermions/spinor_representation.json";
const string BosonRegistryPath = "studies/phase12_joined_calculation_001/output/background_family/bosons/registry.json";
const string BosonModeVectorDir = "studies/phase12_joined_calculation_001/output/background_family/spectra/modes";
const string VariationMatrixDir = "studies/phase12_joined_calculation_001/output/background_family/fermions/couplings/variations";
const string PromotedFermionModeDir = "studies/phase91_branch_stability_evidence_promotion_001/output";
const string Phase120Path = "studies/phase120_analytic_variation_measure_consistency_001/output/analytic_variation_measure_consistency_summary.json";
const string Phase190Path = "studies/phase190_wz_direct_target_independent_geometric_bridge_source_law_001/output/wz_direct_target_independent_geometric_bridge_source_law_summary.json";
const string Phase191Path = "studies/phase191_wz_direct_bridge_prediction_decision_001/output/wz_direct_bridge_prediction_decision_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase247Path = "studies/phase247_direct_bridge_repairability_audit_001/output/direct_bridge_repairability_audit_summary.json";
const string Phase273Path = "studies/phase273_boson_fermion_coupling_proxy_source_audit_001/output/boson_fermion_coupling_proxy_source_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE280_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var spinorDoc = JsonDocument.Parse(File.ReadAllText(SpinorRepresentationPath));
using var bosonRegistry = JsonDocument.Parse(File.ReadAllText(BosonRegistryPath));
using var phase120 = JsonDocument.Parse(File.ReadAllText(Phase120Path));
using var phase190 = JsonDocument.Parse(File.ReadAllText(Phase190Path));
using var phase191 = JsonDocument.Parse(File.ReadAllText(Phase191Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase247 = JsonDocument.Parse(File.ReadAllText(Phase247Path));
using var phase273 = JsonDocument.Parse(File.ReadAllText(Phase273Path));

var spinorSpec = spinorDoc.RootElement.Deserialize<SpinorRepresentationSpec>(JsonOptions())
    ?? throw new InvalidDataException($"Failed to deserialize {SpinorRepresentationPath}");
var gammas = new GammaMatrixBuilder().Build(
    spinorSpec.CliffordSignature,
    spinorSpec.GammaConvention,
    new()
    {
        CreatedAt = DateTimeOffset.UtcNow,
        CodeRevision = "phase280-direct-bridge-analytic-variation-upgrade-audit",
        Branch = new() { BranchId = "phase280-direct-bridge-analytic-variation-upgrade-audit", SchemaVersion = "1.0" },
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

var p190SiblingStability = phase190.RootElement.GetProperty("siblingStability");
var p190BestCandidate = p190SiblingStability.GetProperty("bestCandidate");
var p190BackgroundRecords = p190BestCandidate.GetProperty("backgroundRecords")
    .EnumerateArray()
    .Select(row => new P190BackgroundRecord(
        RequiredString(row, "backgroundId"),
        RequiredString(row, "etaModeId"),
        RequiredString(row, "fromFermionModeId"),
        RequiredString(row, "toFermionModeId"),
        RequiredDouble(row, "matrixElementReal"),
        RequiredDouble(row, "matrixElementImaginary"),
        RequiredDouble(row, "magnitude")))
    .ToArray();

var candidateId = RequiredString(p190BestCandidate, "candidateId");
var p190StabilitySpreadTolerance = RequiredDouble(p190SiblingStability, "stabilitySpreadTolerance");
var p190FiniteStabilityPassed = JsonBool(p190BestCandidate, "stabilityPassed") is true;
var registryCandidate = bosonRegistry.RootElement.GetProperty("candidates")
    .EnumerateArray()
    .Single(candidate => RequiredString(candidate, "candidateId") == candidateId);
var contributingModeIds = registryCandidate.GetProperty("contributingModeIds")
    .EnumerateArray()
    .Select(mode => mode.GetString() ?? "")
    .Where(mode => mode.Length > 0)
    .ToArray();
var representativeModeId = contributingModeIds.FirstOrDefault()
    ?? throw new InvalidDataException($"Candidate {candidateId} has no contributing mode ids.");
var representativeModeIsBranchLocalForAllBackgrounds =
    p190BackgroundRecords.All(record => string.Equals(record.EtaModeId, representativeModeId, StringComparison.Ordinal));
var representativeModeBackgroundMismatchCount =
    p190BackgroundRecords.Count(record => !string.Equals(record.EtaModeId, representativeModeId, StringComparison.Ordinal));
var targetRaw = RequiredDouble(phase191.RootElement.GetProperty("gates"), "targetRaw");
var rawGateRatio = RequiredDouble(phase191.RootElement.GetProperty("gates"), "rawGateRatio");
var theoremClaimed = JsonBool(phase191.RootElement.GetProperty("gates"), "theoremClaimed") is true;
var wZParticleSplitPresent = JsonBool(phase191.RootElement.GetProperty("gates"), "wZParticleSplitPresent") is true;
var p190StableCandidateCount = JsonInt(p190SiblingStability, "stableCandidateCount") ?? 0;
var p120AnalyticVariationPromotable = JsonBool(phase120.RootElement, "promotableAmplitudeMeasureFound") is true;
var p247DirectBridgeRepairabilityAuditPassed = JsonBool(phase247.RootElement, "directBridgeRepairabilityAuditPassed") is true;
var p247NewDirectBridgeTheoremStillRequired = JsonBool(phase247.RootElement, "newDirectBridgeTheoremStillRequired") is true;
var p273CouplingProxySourceAuditPassed = JsonBool(phase273.RootElement, "couplingProxySourceAuditPassed") is true;
var p273Phase12FiniteDifferenceOnly = JsonBool(phase273.RootElement, "phase12FiniteDifferenceOnly") is true;
var p273Phase81ProductionInputsMaterialized = JsonBool(phase273.RootElement, "phase81ProductionInputsMaterialized") is true;
var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;

var records = p190BackgroundRecords.Select(BuildRecord).ToArray();
var analyticMeanMagnitude = records.Average(record => record.BranchLocalAnalyticMagnitude);
var representativeAnalyticMeanMagnitude = records.Average(record => record.RepresentativeAnalyticMagnitude);
var finiteMeanMagnitude = records.Average(record => record.FiniteMagnitude);
var analyticRawToTargetRatio = analyticMeanMagnitude / targetRaw;
var representativeAnalyticRawToTargetRatio = representativeAnalyticMeanMagnitude / targetRaw;
var finiteRawToTargetRatio = finiteMeanMagnitude / targetRaw;
var analyticRawGatePassed = analyticRawToTargetRatio >= rawGateRatio;
var representativeAnalyticRawGatePassed = representativeAnalyticRawToTargetRatio >= rawGateRatio;
var finiteRawGatePassed = finiteRawToTargetRatio >= rawGateRatio;
var branchLocalAnalyticRelativeSpread = RelativeSpread(records.Select(record => record.BranchLocalAnalyticMagnitude));
var branchLocalAnalyticStabilityPassed = branchLocalAnalyticRelativeSpread <= p190StabilitySpreadTolerance;
var representativeAnalyticRelativeSpread = RelativeSpread(records.Select(record => record.RepresentativeAnalyticMagnitude));
var representativeAnalyticStabilityPassed = representativeAnalyticRelativeSpread <= p190StabilitySpreadTolerance;
var maxFiniteAnalyticResidual = records.Max(record => record.FiniteToBranchLocalAnalyticBestFitRelativeResidual);
var maxMagnitudeRelativeDelta = records.Max(record => record.BranchLocalMagnitudeRelativeDelta);
var maxFiniteRepresentativeAnalyticResidual =
    records.Max(record => record.FiniteToRepresentativeAnalyticBestFitRelativeResidual);
var maxRepresentativeMagnitudeRelativeDelta =
    records.Max(record => record.RepresentativeMagnitudeRelativeDelta);
var analyticVariationMatchesP190FiniteDifference = maxFiniteAnalyticResidual < 1e-9 && maxMagnitudeRelativeDelta < 1e-9;
var finiteVariationMatchesRegistryRepresentativeMode =
    maxFiniteRepresentativeAnalyticResidual < 1e-9 && maxRepresentativeMagnitudeRelativeDelta < 1e-9;
var p190FiniteVariationUsesRegistryRepresentativeMode =
    finiteVariationMatchesRegistryRepresentativeMode && !representativeModeIsBranchLocalForAllBackgrounds;
var branchLocalFiniteVariationReplayed =
    analyticVariationMatchesP190FiniteDifference
    && !finiteVariationMatchesRegistryRepresentativeMode
    && !p190FiniteVariationUsesRegistryRepresentativeMode;
var analyticVariationCreatesNewSourceRow = false;
var canRepairDirectBridgeWithAnalyticVariation =
    analyticRawGatePassed
    && branchLocalAnalyticStabilityPassed
    && theoremClaimed
    && wZParticleSplitPresent
    && analyticVariationCreatesNewSourceRow;

var directBridgeAnalyticVariationUpgradeAuditPassed =
    p120AnalyticVariationPromotable
    && p190StableCandidateCount == 0
    && !p190FiniteStabilityPassed
    && branchLocalFiniteVariationReplayed
    && !branchLocalAnalyticStabilityPassed
    && !analyticRawGatePassed
    && !representativeAnalyticRawGatePassed
    && !finiteRawGatePassed
    && !theoremClaimed
    && !wZParticleSplitPresent
    && !analyticVariationCreatesNewSourceRow
    && !canRepairDirectBridgeWithAnalyticVariation
    && p247DirectBridgeRepairabilityAuditPassed
    && p247NewDirectBridgeTheoremStillRequired
    && p273CouplingProxySourceAuditPassed
    && p273Phase12FiniteDifferenceOnly
    && !p273Phase81ProductionInputsMaterialized
    && wzMissingFieldCount > 0
    && higgsMissingFieldCount > 0;

var checks = new[]
{
    new Check(
        "analytic-variation-computed-for-p190-best-candidate",
        records.Length == 2 && records.All(record => record.BranchLocalAnalyticVariationComputed),
        $"recordCount={records.Length}; computedCount={records.Count(record => record.BranchLocalAnalyticVariationComputed)}; candidateId={candidateId}"),
    new Check(
        "p190-finite-variation-is-branch-local-after-extractor-repair",
        branchLocalFiniteVariationReplayed,
        $"representativeModeId={representativeModeId}; representativeModeBackgroundMismatchCount={representativeModeBackgroundMismatchCount}; maxFiniteBranchLocalAnalyticResidual={maxFiniteAnalyticResidual:R}; maxFiniteRepresentativeAnalyticResidual={maxFiniteRepresentativeAnalyticResidual:R}"),
    new Check(
        "branch-local-analytic-variation-does-not-repair-stability-or-raw-gate",
        p190StableCandidateCount == 0 && !p190FiniteStabilityPassed && !branchLocalAnalyticStabilityPassed && !analyticRawGatePassed && !finiteRawGatePassed,
        $"p190StableCandidateCount={p190StableCandidateCount}; p190FiniteStabilityPassed={p190FiniteStabilityPassed}; branchLocalAnalyticRelativeSpread={branchLocalAnalyticRelativeSpread:R}; p190StabilitySpreadTolerance={p190StabilitySpreadTolerance:R}; analyticRawToTargetRatio={analyticRawToTargetRatio:R}; finiteRawToTargetRatio={finiteRawToTargetRatio:R}; rawGateRatio={rawGateRatio:R}"),
    new Check(
        "analytic-variation-does-not-supply-theorem-or-particle-split",
        !theoremClaimed && !wZParticleSplitPresent && !analyticVariationCreatesNewSourceRow,
        $"theoremClaimed={theoremClaimed}; wZParticleSplitPresent={wZParticleSplitPresent}; analyticVariationCreatesNewSourceRow={analyticVariationCreatesNewSourceRow}"),
    new Check(
        "existing-repairability-and-coupling-proxy-blockers-preserved",
        p247DirectBridgeRepairabilityAuditPassed
            && p247NewDirectBridgeTheoremStillRequired
            && p273CouplingProxySourceAuditPassed
            && p273Phase12FiniteDifferenceOnly
            && !p273Phase81ProductionInputsMaterialized,
        $"p247DirectBridgeRepairabilityAuditPassed={p247DirectBridgeRepairabilityAuditPassed}; p247NewDirectBridgeTheoremStillRequired={p247NewDirectBridgeTheoremStillRequired}; p273Phase12FiniteDifferenceOnly={p273Phase12FiniteDifferenceOnly}; p273Phase81ProductionInputsMaterialized={p273Phase81ProductionInputsMaterialized}"),
    new Check(
        "source-lineage-blockers-still-active",
        wzMissingFieldCount > 0 && higgsMissingFieldCount > 0,
        $"wzMissingFieldCount={wzMissingFieldCount}; higgsMissingFieldCount={higgsMissingFieldCount}"),
};

var terminalStatus = directBridgeAnalyticVariationUpgradeAuditPassed
    ? "direct-bridge-analytic-variation-upgrade-audit-branch-local-no-repair"
    : "direct-bridge-analytic-variation-upgrade-audit-review-required";

var result = new
{
    phaseId = "phase280-direct-bridge-analytic-variation-upgrade-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    directBridgeAnalyticVariationUpgradeAuditPassed,
    candidateId,
    representativeModeId,
    contributingModeIds,
    representativeModeIsBranchLocalForAllBackgrounds,
    representativeModeBackgroundMismatchCount,
    p190StableCandidateCount,
    p190FiniteStabilityPassed,
    p190StabilitySpreadTolerance,
    p120AnalyticVariationPromotable,
    analyticVariationMatchesP190FiniteDifference,
    finiteVariationMatchesRegistryRepresentativeMode,
    p190FiniteVariationUsesRegistryRepresentativeMode,
    branchLocalFiniteVariationReplayed,
    analyticVariationCreatesNewSourceRow,
    canRepairDirectBridgeWithAnalyticVariation,
    targetRaw,
    rawGateRatio,
    analyticMeanMagnitude,
    representativeAnalyticMeanMagnitude,
    finiteMeanMagnitude,
    analyticRawToTargetRatio,
    representativeAnalyticRawToTargetRatio,
    finiteRawToTargetRatio,
    analyticRawGatePassed,
    representativeAnalyticRawGatePassed,
    finiteRawGatePassed,
    branchLocalAnalyticRelativeSpread,
    branchLocalAnalyticStabilityPassed,
    representativeAnalyticRelativeSpread,
    representativeAnalyticStabilityPassed,
    theoremClaimed,
    wZParticleSplitPresent,
    maxFiniteAnalyticResidual,
    maxMagnitudeRelativeDelta,
    maxFiniteRepresentativeAnalyticResidual,
    maxRepresentativeMagnitudeRelativeDelta,
    records,
    currentBlockerEvidence = new
    {
        phase247 = new
        {
            p247DirectBridgeRepairabilityAuditPassed,
            p247NewDirectBridgeTheoremStillRequired,
        },
        phase273 = new
        {
            p273CouplingProxySourceAuditPassed,
            p273Phase12FiniteDifferenceOnly,
            p273Phase81ProductionInputsMaterialized,
        },
        phase213 = new
        {
            wzMissingFieldCount,
            higgsMissingFieldCount,
        },
    },
    checks,
    decision = directBridgeAnalyticVariationUpgradeAuditPassed
        ? "Do not promote P190/P191 after repairing finite-difference extraction to branch-local perturbations. The persisted P190 finite matrix now matches branch-local analytic replay, but that replay removes the sibling-stability evidence, still fails the raw gate, and still lacks theorem promotion and W/Z particle split."
        : "Review direct bridge analytic-variation upgrade audit before relying on this route.",
    nextRequiredArtifact = new[]
    {
        "A theorem or branch-local proof that turns the direct bridge matrix element into separate W and Z source rows.",
        "A source-derived normalization or amplitude law that changes the raw scale without target fitting.",
        "A physical W/Z observed-field extraction artifact; analytic variation alone does not create the particle split.",
    },
    sourceEvidence = new
    {
        spinorRepresentationPath = SpinorRepresentationPath,
        bosonRegistryPath = BosonRegistryPath,
        bosonModeVectorDir = BosonModeVectorDir,
        variationMatrixDir = VariationMatrixDir,
        promotedFermionModeDir = PromotedFermionModeDir,
        phase120Path = Phase120Path,
        phase190Path = Phase190Path,
        phase191Path = Phase191Path,
        phase213Path = Phase213Path,
        phase247Path = Phase247Path,
        phase273Path = Phase273Path,
    },
};

var options = JsonOptions();
File.WriteAllText(Path.Combine(outputDir, "direct_bridge_analytic_variation_upgrade_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "direct_bridge_analytic_variation_upgrade_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.directBridgeAnalyticVariationUpgradeAuditPassed,
        result.candidateId,
        result.representativeModeId,
        result.representativeModeIsBranchLocalForAllBackgrounds,
        result.representativeModeBackgroundMismatchCount,
        result.p190StableCandidateCount,
        result.p190FiniteStabilityPassed,
        result.p190StabilitySpreadTolerance,
        result.p120AnalyticVariationPromotable,
        result.analyticVariationMatchesP190FiniteDifference,
        result.finiteVariationMatchesRegistryRepresentativeMode,
        result.p190FiniteVariationUsesRegistryRepresentativeMode,
        result.branchLocalFiniteVariationReplayed,
        result.analyticVariationCreatesNewSourceRow,
        result.canRepairDirectBridgeWithAnalyticVariation,
        result.analyticMeanMagnitude,
        result.representativeAnalyticMeanMagnitude,
        result.finiteMeanMagnitude,
        result.analyticRawToTargetRatio,
        result.representativeAnalyticRawToTargetRatio,
        result.finiteRawToTargetRatio,
        result.analyticRawGatePassed,
        result.representativeAnalyticRawGatePassed,
        result.finiteRawGatePassed,
        result.branchLocalAnalyticRelativeSpread,
        result.branchLocalAnalyticStabilityPassed,
        result.representativeAnalyticRelativeSpread,
        result.representativeAnalyticStabilityPassed,
        result.theoremClaimed,
        result.wZParticleSplitPresent,
        result.maxFiniteAnalyticResidual,
        result.maxMagnitudeRelativeDelta,
        result.maxFiniteRepresentativeAnalyticResidual,
        result.maxRepresentativeMagnitudeRelativeDelta,
        result.currentBlockerEvidence,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"directBridgeAnalyticVariationUpgradeAuditPassed={directBridgeAnalyticVariationUpgradeAuditPassed}");
Console.WriteLine($"candidateId={candidateId}");
Console.WriteLine($"representativeModeId={representativeModeId}");
Console.WriteLine($"analyticVariationMatchesP190FiniteDifference={analyticVariationMatchesP190FiniteDifference}");
Console.WriteLine($"finiteVariationMatchesRegistryRepresentativeMode={finiteVariationMatchesRegistryRepresentativeMode}");
Console.WriteLine($"p190FiniteVariationUsesRegistryRepresentativeMode={p190FiniteVariationUsesRegistryRepresentativeMode}");
Console.WriteLine($"branchLocalFiniteVariationReplayed={branchLocalFiniteVariationReplayed}");
Console.WriteLine($"branchLocalAnalyticRelativeSpread={branchLocalAnalyticRelativeSpread:R}");
Console.WriteLine($"analyticRawToTargetRatio={analyticRawToTargetRatio:R}");
Console.WriteLine($"analyticRawGatePassed={analyticRawGatePassed}");
Console.WriteLine($"theoremClaimed={theoremClaimed}");
Console.WriteLine($"wZParticleSplitPresent={wZParticleSplitPresent}");
Console.WriteLine($"canRepairDirectBridgeWithAnalyticVariation={canRepairDirectBridgeWithAnalyticVariation}");

AnalyticUpgradeRecord BuildRecord(P190BackgroundRecord row)
{
    var branchLocalModePath = Path.Combine(BosonModeVectorDir, row.EtaModeId + ".json");
    var representativeModePath = Path.Combine(BosonModeVectorDir, representativeModeId + ".json");
    var finiteMatrixPath = Path.Combine(VariationMatrixDir, $"variation-{row.BackgroundId}-{candidateId}.matrix.json");
    using var matrixDoc = JsonDocument.Parse(File.ReadAllText(finiteMatrixPath));
    var finiteRe = LoadMatrix(matrixDoc.RootElement.GetProperty("real"));
    var finiteIm = LoadMatrix(matrixDoc.RootElement.GetProperty("imag"));
    var branchLocalModeVector = LoadModeVector(branchLocalModePath);
    var representativeModeVector = LoadModeVector(representativeModePath);
    var (branchLocalAnalyticRe, branchLocalAnalyticIm) = DiracVariationComputer.ComputeAnalytical(
        branchLocalModeVector,
        gammas,
        mesh.VertexCount,
        spinorDim,
        dimG,
        edgeLengths,
        cellsPerEdge,
        edgeDirections);
    var (representativeAnalyticRe, representativeAnalyticIm) = DiracVariationComputer.ComputeAnalytical(
        representativeModeVector,
        gammas,
        mesh.VertexCount,
        spinorDim,
        dimG,
        edgeLengths,
        cellsPerEdge,
        edgeDirections);
    var fermionModes = LoadFermionModes(PromotedFermionModePath(row.BackgroundId));
    var from = fermionModes.Single(mode => mode.ModeId == row.FromFermionModeId);
    var to = fermionModes.Single(mode => mode.ModeId == row.ToFermionModeId);
    var finiteElement = MatrixElement(finiteRe, finiteIm, from.Coefficients, to.Coefficients);
    var branchLocalAnalyticElement = MatrixElement(branchLocalAnalyticRe, branchLocalAnalyticIm, from.Coefficients, to.Coefficients);
    var representativeAnalyticElement = MatrixElement(representativeAnalyticRe, representativeAnalyticIm, from.Coefficients, to.Coefficients);
    var finiteMagnitude = Magnitude(finiteElement.Real, finiteElement.Imaginary);
    var branchLocalAnalyticMagnitude = Magnitude(branchLocalAnalyticElement.Real, branchLocalAnalyticElement.Imaginary);
    var representativeAnalyticMagnitude = Magnitude(representativeAnalyticElement.Real, representativeAnalyticElement.Imaginary);
    var finiteToBranchLocalAnalyticScale = BestFitScale(finiteRe, finiteIm, branchLocalAnalyticRe, branchLocalAnalyticIm);
    var finiteToBranchLocalAnalyticResidual =
        RelativeResidual(finiteRe, finiteIm, branchLocalAnalyticRe, branchLocalAnalyticIm, finiteToBranchLocalAnalyticScale);
    var branchLocalUnitScaleResidual = RelativeResidual(finiteRe, finiteIm, branchLocalAnalyticRe, branchLocalAnalyticIm, 1.0);
    var branchLocalMagnitudeRelativeDelta =
        Math.Abs(branchLocalAnalyticMagnitude - finiteMagnitude) / Math.Max(Math.Abs(finiteMagnitude), 1e-300);
    var finiteToRepresentativeAnalyticScale = BestFitScale(finiteRe, finiteIm, representativeAnalyticRe, representativeAnalyticIm);
    var finiteToRepresentativeAnalyticResidual =
        RelativeResidual(finiteRe, finiteIm, representativeAnalyticRe, representativeAnalyticIm, finiteToRepresentativeAnalyticScale);
    var representativeUnitScaleResidual = RelativeResidual(finiteRe, finiteIm, representativeAnalyticRe, representativeAnalyticIm, 1.0);
    var representativeMagnitudeRelativeDelta =
        Math.Abs(representativeAnalyticMagnitude - finiteMagnitude) / Math.Max(Math.Abs(finiteMagnitude), 1e-300);

    return new AnalyticUpgradeRecord(
        row.BackgroundId,
        row.EtaModeId,
        representativeModeId,
        row.FromFermionModeId,
        row.ToFermionModeId,
        branchLocalModePath,
        representativeModePath,
        finiteMatrixPath,
        row.P190MatrixElementReal,
        row.P190MatrixElementImaginary,
        row.P190Magnitude,
        finiteElement.Real,
        finiteElement.Imaginary,
        finiteMagnitude,
        branchLocalAnalyticElement.Real,
        branchLocalAnalyticElement.Imaginary,
        branchLocalAnalyticMagnitude,
        representativeAnalyticElement.Real,
        representativeAnalyticElement.Imaginary,
        representativeAnalyticMagnitude,
        finiteToBranchLocalAnalyticScale,
        finiteToBranchLocalAnalyticResidual,
        branchLocalUnitScaleResidual,
        branchLocalMagnitudeRelativeDelta,
        finiteToRepresentativeAnalyticScale,
        finiteToRepresentativeAnalyticResidual,
        representativeUnitScaleResidual,
        representativeMagnitudeRelativeDelta,
        true);
}

static IReadOnlyList<FermionModeRecord> LoadFermionModes(string path)
{
    using var doc = JsonDocument.Parse(File.ReadAllText(path));
    return doc.RootElement.GetProperty("modes")
        .EnumerateArray()
        .Select(mode => new FermionModeRecord(
            RequiredString(mode, "modeId"),
            RequiredString(mode, "backgroundId"),
            JsonInt(mode, "modeIndex") ?? throw new InvalidDataException("Missing modeIndex"),
            mode.GetProperty("eigenvectorCoefficients").EnumerateArray().Select(value => value.GetDouble()).ToArray()))
        .OrderBy(mode => mode.ModeIndex)
        .ToArray();
}

static string PromotedFermionModePath(string backgroundId) =>
    Path.Combine(PromotedFermionModeDir, backgroundId, "branch_stability_promoted_fermion_modes.json");

static JsonSerializerOptions JsonOptions() => new()
{
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
};

static double[] LoadModeVector(string path)
{
    using var doc = JsonDocument.Parse(File.ReadAllText(path));
    return doc.RootElement.GetProperty("modeVector").EnumerateArray().Select(value => value.GetDouble()).ToArray();
}

static double[,] LoadMatrix(JsonElement rows)
{
    int rowCount = rows.GetArrayLength();
    int colCount = rows[0].GetArrayLength();
    var matrix = new double[rowCount, colCount];
    int row = 0;
    foreach (var rowElement in rows.EnumerateArray())
    {
        int col = 0;
        foreach (var value in rowElement.EnumerateArray())
        {
            matrix[row, col++] = value.GetDouble();
        }

        row++;
    }

    return matrix;
}

static (double Real, double Imaginary) MatrixElement(double[,] matrixRe, double[,] matrixIm, double[] phiI, double[] phiJ)
{
    int n = matrixRe.GetLength(0);
    var iNorm = Normalize(phiI);
    var jNorm = Normalize(phiJ);
    var deltaJ = new double[n * 2];
    for (int row = 0; row < n; row++)
    {
        double sumRe = 0.0;
        double sumIm = 0.0;
        for (int col = 0; col < n; col++)
        {
            double aRe = matrixRe[row, col];
            double aIm = matrixIm[row, col];
            double bRe = jNorm[col * 2];
            double bIm = jNorm[col * 2 + 1];
            sumRe += aRe * bRe - aIm * bIm;
            sumIm += aRe * bIm + aIm * bRe;
        }

        deltaJ[row * 2] = sumRe;
        deltaJ[row * 2 + 1] = sumIm;
    }

    double real = 0.0;
    double imaginary = 0.0;
    for (int k = 0; k < n; k++)
    {
        double iRe = iNorm[k * 2];
        double iIm = iNorm[k * 2 + 1];
        double dRe = deltaJ[k * 2];
        double dIm = deltaJ[k * 2 + 1];
        real += iRe * dRe + iIm * dIm;
        imaginary += iRe * dIm - iIm * dRe;
    }

    return (real, imaginary);
}

static double BestFitScale(double[,] targetRe, double[,] targetIm, double[,] sourceRe, double[,] sourceIm)
{
    double dot = 0.0;
    double norm2 = 0.0;
    int rows = targetRe.GetLength(0);
    int cols = targetRe.GetLength(1);
    for (int r = 0; r < rows; r++)
        for (int c = 0; c < cols; c++)
        {
            dot += targetRe[r, c] * sourceRe[r, c] + targetIm[r, c] * sourceIm[r, c];
            norm2 += sourceRe[r, c] * sourceRe[r, c] + sourceIm[r, c] * sourceIm[r, c];
        }

    return norm2 > 0.0 ? dot / norm2 : double.NaN;
}

static double RelativeResidual(double[,] targetRe, double[,] targetIm, double[,] sourceRe, double[,] sourceIm, double scale)
{
    double residual2 = 0.0;
    double target2 = 0.0;
    int rows = targetRe.GetLength(0);
    int cols = targetRe.GetLength(1);
    for (int r = 0; r < rows; r++)
        for (int c = 0; c < cols; c++)
        {
            double dRe = targetRe[r, c] - scale * sourceRe[r, c];
            double dIm = targetIm[r, c] - scale * sourceIm[r, c];
            residual2 += dRe * dRe + dIm * dIm;
            target2 += targetRe[r, c] * targetRe[r, c] + targetIm[r, c] * targetIm[r, c];
        }

    return target2 > 0.0 ? Math.Sqrt(residual2 / target2) : double.NaN;
}

static double[] Normalize(double[] vector)
{
    double norm = Norm(vector);
    return norm < 1e-30 ? vector : vector.Select(value => value / norm).ToArray();
}

static double Norm(IEnumerable<double> values) => Math.Sqrt(values.Sum(value => value * value));

static double Magnitude(double real, double imaginary) => Math.Sqrt(real * real + imaginary * imaginary);

static double RelativeSpread(IEnumerable<double> values)
{
    var array = values.ToArray();
    if (array.Length == 0)
    {
        return double.NaN;
    }

    var mean = array.Average();
    return (array.Max() - array.Min()) / Math.Max(Math.Abs(mean), 1e-300);
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
    var dir = new double[dim];
    double norm = 0.0;
    for (int k = 0; k < dim; k++)
    {
        dir[k] = coords1[k] - coords0[k];
        norm += dir[k] * dir[k];
    }

    norm = Math.Sqrt(norm);
    if (norm > 1e-14)
    {
        for (int k = 0; k < dim; k++)
        {
            dir[k] /= norm;
        }
    }

    return dir;
}

static string RequiredString(JsonElement element, string propertyName) =>
    JsonString(element, propertyName) ?? throw new InvalidDataException($"Missing string property '{propertyName}'.");

static double RequiredDouble(JsonElement element, string propertyName) =>
    JsonDouble(element, propertyName) ?? throw new InvalidDataException($"Missing numeric property '{propertyName}'.");

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

static double? JsonDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var value) ? value : null;

sealed record P190BackgroundRecord(
    string BackgroundId,
    string EtaModeId,
    string FromFermionModeId,
    string ToFermionModeId,
    double P190MatrixElementReal,
    double P190MatrixElementImaginary,
    double P190Magnitude);

sealed record FermionModeRecord(string ModeId, string BackgroundId, int ModeIndex, double[] Coefficients);

sealed record AnalyticUpgradeRecord(
    string BackgroundId,
    string EtaModeId,
    string RepresentativeModeId,
    string FromFermionModeId,
    string ToFermionModeId,
    string BranchLocalModePath,
    string RepresentativeModePath,
    string FiniteMatrixPath,
    double P190MatrixElementReal,
    double P190MatrixElementImaginary,
    double P190Magnitude,
    double ReplayedFiniteMatrixElementReal,
    double ReplayedFiniteMatrixElementImaginary,
    double FiniteMagnitude,
    double BranchLocalAnalyticMatrixElementReal,
    double BranchLocalAnalyticMatrixElementImaginary,
    double BranchLocalAnalyticMagnitude,
    double RepresentativeAnalyticMatrixElementReal,
    double RepresentativeAnalyticMatrixElementImaginary,
    double RepresentativeAnalyticMagnitude,
    double FiniteToBranchLocalAnalyticBestFitScale,
    double FiniteToBranchLocalAnalyticBestFitRelativeResidual,
    double BranchLocalUnitScaleRelativeResidual,
    double BranchLocalMagnitudeRelativeDelta,
    double FiniteToRepresentativeAnalyticBestFitScale,
    double FiniteToRepresentativeAnalyticBestFitRelativeResidual,
    double RepresentativeUnitScaleRelativeResidual,
    double RepresentativeMagnitudeRelativeDelta,
    bool BranchLocalAnalyticVariationComputed);

record Check(string CheckId, bool Passed, string Detail);
