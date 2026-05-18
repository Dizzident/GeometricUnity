using System.Text.Json;
using System.Text.Json.Serialization;
using Gu.Core;
using Gu.Geometry;
using Gu.Phase4.Fermions;
using Gu.Phase4.Spin;
using Gu.Phase5.Reporting;

const string DefaultOutputDir = "studies/phase298_production_analytic_wz_source_row_replay_attempt_001/output";
const string RunRoot = "studies/phase12_joined_calculation_001/output/background_family";
const string SpinorRepresentationPath = "studies/phase12_joined_calculation_001/output/background_family/fermions/spinor_representation.json";
const string BosonModeVectorDir = "studies/phase12_joined_calculation_001/output/background_family/spectra/modes";
const string PromotedFermionModeDir = "studies/phase91_branch_stability_evidence_promotion_001/output";
const string Phase190Path = "studies/phase190_wz_direct_target_independent_geometric_bridge_source_law_001/output/wz_direct_target_independent_geometric_bridge_source_law_summary.json";
const string Phase191Path = "studies/phase191_wz_direct_bridge_prediction_decision_001/output/wz_direct_bridge_prediction_decision_summary.json";
const string Phase201Path = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase280Path = "studies/phase280_direct_bridge_analytic_variation_upgrade_audit_001/output/direct_bridge_analytic_variation_upgrade_audit_summary.json";
const string Phase297Path = "studies/phase297_wz_direct_bridge_source_contract_application_audit_001/output/wz_direct_bridge_source_contract_application_audit_summary.json";
const double StabilitySpreadToleranceFallback = 0.05;

var outputDir = Environment.GetEnvironmentVariable("PHASE298_OUTPUT_DIR") ?? DefaultOutputDir;
var packageOutputDir = Path.Combine(outputDir, "full_replay_packages");
Directory.CreateDirectory(outputDir);
Directory.CreateDirectory(packageOutputDir);

var jsonOptions = JsonOptions();
using var phase190 = JsonDocument.Parse(File.ReadAllText(Phase190Path));
using var phase191 = JsonDocument.Parse(File.ReadAllText(Phase191Path));
using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase280 = JsonDocument.Parse(File.ReadAllText(Phase280Path));
using var phase297 = JsonDocument.Parse(File.ReadAllText(Phase297Path));

var p190SiblingStability = phase190.RootElement.GetProperty("siblingStability");
var p190BestCandidate = p190SiblingStability.GetProperty("bestCandidate");
var p191Gates = phase191.RootElement.GetProperty("gates");
var candidateId = RequiredString(p190BestCandidate, "candidateId");
var p190TargetIndependent = JsonBool(phase190.RootElement, "targetObservablesUsed") is false;
var p190TheoremClaimed = JsonBool(phase190.RootElement, "theoremClaimed") is true;
var p190StableCandidateCount = JsonInt(p190SiblingStability, "stableCandidateCount") ?? -1;
var p190FiniteStabilityPassed = JsonBool(p190BestCandidate, "stabilityPassed") is true;
var stabilitySpreadTolerance = JsonDouble(p190SiblingStability, "stabilitySpreadTolerance") ?? StabilitySpreadToleranceFallback;
var targetRaw = RequiredDouble(p191Gates, "targetRaw");
var rawGateRatio = RequiredDouble(p191Gates, "rawGateRatio");
var p191RawGatePassed = JsonBool(p191Gates, "rawGatePassed") is true;
var theoremClaimed = p190TheoremClaimed || JsonBool(p191Gates, "theoremClaimed") is true;
var wZParticleSplitPresent = JsonBool(p191Gates, "wZParticleSplitPresent") is true;
var p201WzPromotable = phase201.RootElement.TryGetProperty("wzValidation", out var p201WzValidation)
    && JsonBool(p201WzValidation, "promotable") is true;
var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;
var p280CanRepairWithAnalyticVariation = JsonBool(phase280.RootElement, "canRepairDirectBridgeWithAnalyticVariation") is true;
var p280AnalyticRawGatePassed = JsonBool(phase280.RootElement, "analyticRawGatePassed") is true;
var p280BranchLocalAnalyticStabilityPassed = JsonBool(phase280.RootElement, "branchLocalAnalyticStabilityPassed") is true;
var p297Passed = JsonBool(phase297.RootElement, "wzDirectBridgeSourceContractApplicationAuditPassed") is true;
var p297CanFillWzSourceContractNow = JsonBool(phase297.RootElement, "canFillWzSourceContractNow") is true;
var p297AcceptedContractFieldCount = JsonInt(phase297.RootElement, "acceptedContractFieldCount") ?? -1;
var p297BlockedContractFieldCount = JsonInt(phase297.RootElement, "blockedContractFieldCount") ?? -1;

var provenance = new ProvenanceMeta
{
    CreatedAt = DateTimeOffset.UtcNow,
    CodeRevision = "phase298-production-analytic-wz-source-row-replay-attempt",
    Branch = new()
    {
        BranchId = "phase298-production-analytic-wz-source-row-replay-attempt",
        SchemaVersion = "1.0.0",
    },
    Backend = "cpu-reference",
    Notes = "Production analytic replay over the P190 best W/Z-like direct-bridge candidate.",
};

var spinorSpec = LoadJson<SpinorRepresentationSpec>(SpinorRepresentationPath);
var gammas = new GammaMatrixBuilder().Build(spinorSpec.CliffordSignature, spinorSpec.GammaConvention, provenance);
var mesh = ToyGeometryFactory.CreateStructuredFiberBundle2D(rows: 2, cols: 2).AmbientMesh;
var geometry = BuildGeometry(mesh, spinorSpec.SpinorComponents);
int spinorDim = spinorSpec.SpinorComponents;
int dimG = 3;

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

var replayRows = p190BackgroundRecords.Select(BuildReplayRow).ToArray();
var productionReplayBuiltCount = replayRows.Count(row => row.ProductionReplayBuilt);
var materializationAuditPassedCount = replayRows.Count(row => row.MaterializationAuditPassed);
var evidenceValidatedCount = replayRows.Count(row => row.RawMatrixElementEvidenceValidated);
var allProductionReplaysBuilt = replayRows.Length > 0 && replayRows.All(row => row.ProductionReplayBuilt);
var allMaterializationAuditsPassed = replayRows.Length > 0 && replayRows.All(row => row.MaterializationAuditPassed);
var allRawMatrixElementEvidenceValidated = replayRows.Length > 0 && replayRows.All(row => row.RawMatrixElementEvidenceValidated);
var finiteRawValues = replayRows.Select(row => row.RawMatrixElementMagnitude).Where(value => value.HasValue).Select(value => value!.Value).ToArray();
var meanRawMagnitude = finiteRawValues.Length == 0 ? double.NaN : finiteRawValues.Average();
var bestRawToTargetRatio = double.IsFinite(meanRawMagnitude) ? meanRawMagnitude / targetRaw : double.NaN;
var rawGatePassed = double.IsFinite(bestRawToTargetRatio) && bestRawToTargetRatio >= rawGateRatio;
var allRawRowGatesPassed = replayRows.Length > 0 && replayRows.All(row => row.RawGatePassed);
var branchLocalAnalyticRelativeSpread = RelativeSpread(finiteRawValues);
var branchLocalAnalyticStabilityPassed =
    double.IsFinite(branchLocalAnalyticRelativeSpread) && branchLocalAnalyticRelativeSpread <= stabilitySpreadTolerance;
var targetObservablesUsedForConstruction = false;
var targetValuesUsedOnlyForPostReplayEvaluation = true;
var sourceRowsPromotable =
    allProductionReplaysBuilt
    && allMaterializationAuditsPassed
    && allRawMatrixElementEvidenceValidated
    && rawGatePassed
    && branchLocalAnalyticStabilityPassed
    && theoremClaimed
    && wZParticleSplitPresent;
var canEmitWzSourceRows = sourceRowsPromotable;
var canFillPhase201WzContract = canEmitWzSourceRows && p201WzPromotable;
var phase201TemplateMutated = false;
var fieldsAppliedToPhase201TemplateCount = 0;
var productionInputGapClosedForP190BestCandidate =
    allProductionReplaysBuilt && allMaterializationAuditsPassed && allRawMatrixElementEvidenceValidated;
var productionReplayDoesNotFixSourceContract =
    productionInputGapClosedForP190BestCandidate
    && !rawGatePassed
    && !theoremClaimed
    && !wZParticleSplitPresent
    && !canFillPhase201WzContract
    && wzMissingFieldCount > 0;
var productionAnalyticWzSourceRowReplayAttemptPassed =
    p190TargetIndependent
    && !p190FiniteStabilityPassed
    && !p191RawGatePassed
    && productionReplayDoesNotFixSourceContract
    && p280CanRepairWithAnalyticVariation is false
    && p280AnalyticRawGatePassed is false
    && p280BranchLocalAnalyticStabilityPassed is false
    && p297Passed
    && !p297CanFillWzSourceContractNow
    && p297AcceptedContractFieldCount == 0
    && p297BlockedContractFieldCount > 0
    && !phase201TemplateMutated
    && fieldsAppliedToPhase201TemplateCount == 0;

var terminalStatus = !allProductionReplaysBuilt
    ? "production-analytic-wz-source-row-replay-blocked-production-inputs-missing"
    : productionAnalyticWzSourceRowReplayAttemptPassed
        ? "production-analytic-wz-source-row-replay-built-raw-gate-and-contract-blocked"
        : sourceRowsPromotable
            ? "production-analytic-wz-source-row-replay-review-source-rows-promotable"
            : "production-analytic-wz-source-row-replay-review-required";

var checks = new[]
{
    new Check(
        "production-source-backed-replays-built",
        allProductionReplaysBuilt,
        $"productionReplayBuiltCount={productionReplayBuiltCount}; expectedCount={p190BackgroundRecords.Length}"),
    new Check(
        "production-input-materialization-audits-pass",
        allMaterializationAuditsPassed,
        $"materializationAuditPassedCount={materializationAuditPassedCount}; expectedCount={p190BackgroundRecords.Length}; acceptedSourceKind={ProductionAnalyticReplayInputMaterializationAuditor.AcceptedBosonModeSourceKind}"),
    new Check(
        "raw-matrix-element-evidence-validated",
        allRawMatrixElementEvidenceValidated,
        $"evidenceValidatedCount={evidenceValidatedCount}; expectedCount={p190BackgroundRecords.Length}; targetObservablesUsedForConstruction={targetObservablesUsedForConstruction}"),
    new Check(
        "production-replay-still-fails-raw-gate",
        !rawGatePassed && !allRawRowGatesPassed,
        $"meanRawMagnitude={meanRawMagnitude:R}; targetRaw={targetRaw:R}; bestRawToTargetRatio={bestRawToTargetRatio:R}; rawGateRatio={rawGateRatio:R}; p191RawGatePassed={p191RawGatePassed}"),
    new Check(
        "production-replay-still-lacks-promoted-theorem-and-wz-split",
        !theoremClaimed && !wZParticleSplitPresent,
        $"theoremClaimed={theoremClaimed}; wZParticleSplitPresent={wZParticleSplitPresent}"),
    new Check(
        "phase201-contract-not-filled-or-mutated",
        !canFillPhase201WzContract && !phase201TemplateMutated && fieldsAppliedToPhase201TemplateCount == 0 && wzMissingFieldCount > 0,
        $"canFillPhase201WzContract={canFillPhase201WzContract}; phase201TemplateMutated={phase201TemplateMutated}; wzMissingFieldCount={wzMissingFieldCount}; p297CanFillWzSourceContractNow={p297CanFillWzSourceContractNow}"),
};

var result = new
{
    phaseId = "phase298-production-analytic-wz-source-row-replay-attempt",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    productionAnalyticWzSourceRowReplayAttemptPassed,
    candidateId,
    applicationAttempted = true,
    targetObservablesUsedForConstruction,
    targetValuesUsedOnlyForPostReplayEvaluation,
    productionInputGapClosedForP190BestCandidate,
    productionReplayBuiltCount,
    materializationAuditPassedCount,
    evidenceValidatedCount,
    expectedReplayCount = p190BackgroundRecords.Length,
    allProductionReplaysBuilt,
    allMaterializationAuditsPassed,
    allRawMatrixElementEvidenceValidated,
    targetRaw,
    rawGateRatio,
    meanRawMagnitude,
    bestRawToTargetRatio,
    rawGatePassed,
    allRawRowGatesPassed,
    stabilitySpreadTolerance,
    p190StableCandidateCount,
    p190FiniteStabilityPassed,
    branchLocalAnalyticRelativeSpread,
    branchLocalAnalyticStabilityPassed,
    theoremClaimed,
    wZParticleSplitPresent,
    sourceRowsPromotable,
    canEmitWzSourceRows,
    canFillPhase201WzContract,
    phase201TemplateMutated,
    fieldsAppliedToPhase201TemplateCount,
    p201WzPromotable,
    wzMissingFieldCount,
    higgsMissingFieldCount,
    inheritedBlockers = new
    {
        phase280 = new
        {
            p280CanRepairWithAnalyticVariation,
            p280AnalyticRawGatePassed,
            p280BranchLocalAnalyticStabilityPassed,
        },
        phase297 = new
        {
            p297Passed,
            p297CanFillWzSourceContractNow,
            p297AcceptedContractFieldCount,
            p297BlockedContractFieldCount,
        },
    },
    replayRows,
    checks,
    decision = productionAnalyticWzSourceRowReplayAttemptPassed
        ? "The Phase83 production replay path can now materialize analytic W/Z-like matrix-element evidence for the P190 best candidate, but it still cannot emit W/Z source rows. The replay remains far below the raw target, lacks a derivation-backed theorem, lacks a particle-specific W/Z split, and cannot fill the Phase201 source-lineage contract."
        : "Review the production analytic W/Z source-row replay before relying on this route.",
    nextRequiredArtifact = new[]
    {
        "A derivation-backed direct W/Z bridge-source theorem that turns the W/Z-like matrix element into particle-specific W and Z source rows.",
        "A source-derived raw-amplitude or normalization law that clears the raw and common W/Z bridge gates without target fitting.",
        "A source-lineage application artifact that fills all Phase201 W/Z contract fields without mutating the intake template prematurely.",
    },
    sourceEvidence = new
    {
        runRoot = RunRoot,
        spinorRepresentationPath = SpinorRepresentationPath,
        bosonModeVectorDir = BosonModeVectorDir,
        promotedFermionModeDir = PromotedFermionModeDir,
        phase190Path = Phase190Path,
        phase191Path = Phase191Path,
        phase201Path = Phase201Path,
        phase213Path = Phase213Path,
        phase280Path = Phase280Path,
        phase297Path = Phase297Path,
    },
};

File.WriteAllText(
    Path.Combine(outputDir, "production_analytic_wz_source_row_replay_attempt.json"),
    JsonSerializer.Serialize(result, jsonOptions));
File.WriteAllText(
    Path.Combine(outputDir, "production_analytic_wz_source_row_replay_attempt_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.productionAnalyticWzSourceRowReplayAttemptPassed,
        result.candidateId,
        result.applicationAttempted,
        result.targetObservablesUsedForConstruction,
        result.targetValuesUsedOnlyForPostReplayEvaluation,
        result.productionInputGapClosedForP190BestCandidate,
        result.productionReplayBuiltCount,
        result.materializationAuditPassedCount,
        result.evidenceValidatedCount,
        result.expectedReplayCount,
        result.allProductionReplaysBuilt,
        result.allMaterializationAuditsPassed,
        result.allRawMatrixElementEvidenceValidated,
        result.targetRaw,
        result.rawGateRatio,
        result.meanRawMagnitude,
        result.bestRawToTargetRatio,
        result.rawGatePassed,
        result.allRawRowGatesPassed,
        result.stabilitySpreadTolerance,
        result.p190StableCandidateCount,
        result.p190FiniteStabilityPassed,
        result.branchLocalAnalyticRelativeSpread,
        result.branchLocalAnalyticStabilityPassed,
        result.theoremClaimed,
        result.wZParticleSplitPresent,
        result.sourceRowsPromotable,
        result.canEmitWzSourceRows,
        result.canFillPhase201WzContract,
        result.phase201TemplateMutated,
        result.fieldsAppliedToPhase201TemplateCount,
        result.p201WzPromotable,
        result.wzMissingFieldCount,
        result.higgsMissingFieldCount,
        result.inheritedBlockers,
        rows = replayRows.Select(row => new
        {
            row.BackgroundId,
            row.EtaModeId,
            row.FromFermionModeId,
            row.ToFermionModeId,
            row.ReplayTerminalStatus,
            row.ProductionReplayBuilt,
            row.MaterializationStatus,
            row.MaterializationAuditPassed,
            row.RawMatrixElementEvidenceStatus,
            row.RawMatrixElementEvidenceValidated,
            row.RawMatrixElementMagnitude,
            row.RawToTargetRatio,
            row.RawGatePassed,
            row.P190Magnitude,
            row.MagnitudeRelativeDeltaFromP190,
            row.VariationEvidenceId,
            row.FullReplayPackagePath,
        }),
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, jsonOptions));

Console.WriteLine(terminalStatus);
Console.WriteLine($"productionAnalyticWzSourceRowReplayAttemptPassed={productionAnalyticWzSourceRowReplayAttemptPassed}");
Console.WriteLine($"productionInputGapClosedForP190BestCandidate={productionInputGapClosedForP190BestCandidate}");
Console.WriteLine($"productionReplayBuiltCount={productionReplayBuiltCount}");
Console.WriteLine($"materializationAuditPassedCount={materializationAuditPassedCount}");
Console.WriteLine($"evidenceValidatedCount={evidenceValidatedCount}");
Console.WriteLine($"meanRawMagnitude={meanRawMagnitude:R}");
Console.WriteLine($"bestRawToTargetRatio={bestRawToTargetRatio:R}");
Console.WriteLine($"rawGatePassed={rawGatePassed}");
Console.WriteLine($"branchLocalAnalyticRelativeSpread={branchLocalAnalyticRelativeSpread:R}");
Console.WriteLine($"branchLocalAnalyticStabilityPassed={branchLocalAnalyticStabilityPassed}");
Console.WriteLine($"theoremClaimed={theoremClaimed}");
Console.WriteLine($"wZParticleSplitPresent={wZParticleSplitPresent}");
Console.WriteLine($"canEmitWzSourceRows={canEmitWzSourceRows}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");

ReplayRow BuildReplayRow(P190BackgroundRecord row)
{
    var bosonModePath = Path.Combine(BosonModeVectorDir, row.EtaModeId + ".json");
    var fermionModesPath = PromotedFermionModePath(row.BackgroundId);
    var fermionModes = LoadJson<PromotedFermionModeBundle>(fermionModesPath).Modes;
    var modeI = fermionModes.Single(mode => string.Equals(mode.ModeId, row.FromFermionModeId, StringComparison.Ordinal));
    var modeJ = fermionModes.Single(mode => string.Equals(mode.ModeId, row.ToFermionModeId, StringComparison.Ordinal));
    var packageId = $"phase298-production-analytic-wz-replay-{Sanitize(row.BackgroundId)}-{Sanitize(row.EtaModeId)}";
    var provenanceId = $"phase298:{row.BackgroundId}:{row.EtaModeId}:{row.FromFermionModeId}->{row.ToFermionModeId}";
    var replay = SourceBackedAnalyticReplayPackageRunner.Run(
        packageId,
        bosonModePath,
        File.ReadAllText(bosonModePath),
        gammas,
        mesh.VertexCount,
        spinorDim,
        dimG,
        geometry.EdgeLengths,
        geometry.CellsPerEdge,
        geometry.EdgeDirections,
        modeI,
        modeJ,
        provenanceId,
        provenance);

    var fullPackage = replay.FullReplayPackage;
    string? packagePath = null;
    if (fullPackage is not null)
    {
        packagePath = Path.Combine(packageOutputDir, packageId + ".json");
        File.WriteAllText(packagePath, JsonSerializer.Serialize(fullPackage, jsonOptions));
    }

    var coupling = fullPackage?.CouplingRecord;
    var evidenceValidation = fullPackage?.EvidenceBuild.EvidenceValidation;
    var materializationAudit = fullPackage?.MaterializationAudit;
    var rawMagnitude = evidenceValidation?.AcceptedRawMatrixElementMagnitude ?? coupling?.CouplingProxyMagnitude;
    var rawToTargetRatio = rawMagnitude.HasValue ? rawMagnitude.Value / targetRaw : (double?)null;
    var rawGatePassedForRow = rawToTargetRatio.HasValue && rawToTargetRatio.Value >= rawGateRatio;
    var productionReplayBuilt =
        string.Equals(replay.TerminalStatus, "source-backed-analytic-replay-package-built", StringComparison.Ordinal)
        && replay.ClosureRequirements.Count == 0
        && fullPackage is not null
        && fullPackage.ClosureRequirements.Count == 0;
    var materializationAuditPassed =
        string.Equals(materializationAudit?.TerminalStatus, "production-analytic-replay-inputs-materialized", StringComparison.Ordinal);
    var evidenceValidated =
        string.Equals(evidenceValidation?.TerminalStatus, "raw-weak-coupling-matrix-element-evidence-validated", StringComparison.Ordinal);
    var magnitudeRelativeDeltaFromP190 = rawMagnitude.HasValue
        ? Math.Abs(rawMagnitude.Value - row.P190Magnitude) / Math.Max(Math.Abs(row.P190Magnitude), 1e-300)
        : (double?)null;

    return new ReplayRow(
        row.BackgroundId,
        row.EtaModeId,
        row.FromFermionModeId,
        row.ToFermionModeId,
        bosonModePath,
        fermionModesPath,
        packagePath,
        replay.TerminalStatus,
        productionReplayBuilt,
        replay.ClosureRequirements,
        materializationAudit?.TerminalStatus,
        materializationAuditPassed,
        materializationAudit?.ClosureRequirements ?? [],
        fullPackage?.EvidenceBuild.TerminalStatus,
        evidenceValidation?.TerminalStatus,
        evidenceValidated,
        evidenceValidation?.ClosureRequirements ?? [],
        coupling?.CouplingId,
        coupling?.CouplingProxyReal,
        coupling?.CouplingProxyImaginary,
        coupling?.CouplingProxyMagnitude,
        evidenceValidation?.Evidence.EvidenceId,
        coupling?.VariationEvidenceId,
        rawMagnitude,
        rawToTargetRatio,
        rawGatePassedForRow,
        row.P190MatrixElementReal,
        row.P190MatrixElementImaginary,
        row.P190Magnitude,
        coupling?.CouplingProxyReal - row.P190MatrixElementReal,
        coupling?.CouplingProxyImaginary - row.P190MatrixElementImaginary,
        magnitudeRelativeDeltaFromP190);
}

static GeometryReplayInputs BuildGeometry(SimplicialMesh mesh, int spinorDim)
{
    var edgeLengths = new double[mesh.EdgeCount];
    var edgeDirections = new double[mesh.EdgeCount][];
    var cellsPerEdge = new int[mesh.EdgeCount][];
    for (int edge = 0; edge < mesh.EdgeCount; edge++)
    {
        edgeLengths[edge] = ComputeEdgeLength(mesh, edge);
        edgeDirections[edge] = ComputeEdgeDirection(mesh, edge);
        cellsPerEdge[edge] = [mesh.Edges[edge][0], mesh.Edges[edge][1]];
    }

    return new GeometryReplayInputs(
        edgeLengths,
        cellsPerEdge,
        edgeDirections,
        mesh.VertexCount,
        mesh.EdgeCount,
        mesh.EmbeddingDimension,
        mesh.EdgeCount * 3,
        mesh.VertexCount * spinorDim * 3 * 2);
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

static double RelativeSpread(IReadOnlyCollection<double> values)
{
    if (values.Count == 0)
    {
        return double.NaN;
    }

    var mean = values.Average();
    return (values.Max() - values.Min()) / Math.Max(Math.Abs(mean), 1e-300);
}

T LoadJson<T>(string path) =>
    JsonSerializer.Deserialize<T>(File.ReadAllText(path), jsonOptions)
    ?? throw new InvalidDataException($"Failed to deserialize {path}");

static JsonSerializerOptions JsonOptions() => new()
{
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
};

static string PromotedFermionModePath(string backgroundId) =>
    Path.Combine(PromotedFermionModeDir, backgroundId, "branch_stability_promoted_fermion_modes.json");

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

static string Sanitize(string value)
{
    var chars = value.Select(ch => char.IsLetterOrDigit(ch) || ch is '-' or '_' ? ch : '-').ToArray();
    return new string(chars);
}

sealed class PromotedFermionModeBundle
{
    [JsonPropertyName("modes")]
    public required List<FermionModeRecord> Modes { get; init; }
}

sealed record GeometryReplayInputs(
    double[] EdgeLengths,
    int[][] CellsPerEdge,
    double[][] EdgeDirections,
    int VertexCount,
    int EdgeCount,
    int EmbeddingDimension,
    int ExpectedBosonVectorLength,
    int ExpectedFermionEigenvectorLength);

sealed record P190BackgroundRecord(
    string BackgroundId,
    string EtaModeId,
    string FromFermionModeId,
    string ToFermionModeId,
    double P190MatrixElementReal,
    double P190MatrixElementImaginary,
    double P190Magnitude);

sealed record ReplayRow(
    string BackgroundId,
    string EtaModeId,
    string FromFermionModeId,
    string ToFermionModeId,
    string BosonModeSourcePath,
    string FermionModeSourcePath,
    string? FullReplayPackagePath,
    string ReplayTerminalStatus,
    bool ProductionReplayBuilt,
    IReadOnlyList<string> ReplayClosureRequirements,
    string? MaterializationStatus,
    bool MaterializationAuditPassed,
    IReadOnlyList<string> MaterializationClosureRequirements,
    string? EvidenceBuildStatus,
    string? RawMatrixElementEvidenceStatus,
    bool RawMatrixElementEvidenceValidated,
    IReadOnlyList<string> RawMatrixElementEvidenceClosureRequirements,
    string? CouplingId,
    double? CouplingReal,
    double? CouplingImaginary,
    double? CouplingMagnitude,
    string? RawMatrixElementEvidenceId,
    string? VariationEvidenceId,
    double? RawMatrixElementMagnitude,
    double? RawToTargetRatio,
    bool RawGatePassed,
    double P190MatrixElementReal,
    double P190MatrixElementImaginary,
    double P190Magnitude,
    double? RealDeltaFromP190,
    double? ImaginaryDeltaFromP190,
    double? MagnitudeRelativeDeltaFromP190);

sealed record Check(string CheckId, bool Passed, string Detail);
