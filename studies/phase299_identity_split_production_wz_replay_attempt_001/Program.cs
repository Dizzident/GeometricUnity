using System.Text.Json;
using System.Text.Json.Serialization;
using Gu.Core;
using Gu.Geometry;
using Gu.Phase4.Fermions;
using Gu.Phase4.Spin;
using Gu.Phase5.Reporting;

const string DefaultOutputDir = "studies/phase299_identity_split_production_wz_replay_attempt_001/output";
const string SpinorRepresentationPath = "studies/phase12_joined_calculation_001/output/background_family/fermions/spinor_representation.json";
const string BosonRegistryPath = "studies/phase12_joined_calculation_001/output/background_family/bosons/registry.json";
const string BosonModeVectorDir = "studies/phase12_joined_calculation_001/output/background_family/spectra/modes";
const string PromotedFermionModeDir = "studies/phase91_branch_stability_evidence_promotion_001/output";
const string Phase27IdentityReadinessPath = "studies/phase27_charge_sector_convention_001/identity_rule_readiness_after_charge_sectors.json";
const string Phase28PromotionResultPath = "studies/phase28_wz_physical_prediction_promotion_001/promotion_result.json";
const string Phase190Path = "studies/phase190_wz_direct_target_independent_geometric_bridge_source_law_001/output/wz_direct_target_independent_geometric_bridge_source_law_summary.json";
const string Phase191Path = "studies/phase191_wz_direct_bridge_prediction_decision_001/output/wz_direct_bridge_prediction_decision_summary.json";
const string Phase201Path = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase251Path = "studies/phase251_upstream_wz_identity_rule_source_chain_audit_001/output/upstream_wz_identity_rule_source_chain_audit_summary.json";
const string Phase297Path = "studies/phase297_wz_direct_bridge_source_contract_application_audit_001/output/wz_direct_bridge_source_contract_application_audit_summary.json";
const string Phase298Path = "studies/phase298_production_analytic_wz_source_row_replay_attempt_001/output/production_analytic_wz_source_row_replay_attempt_summary.json";
const double StabilitySpreadToleranceFallback = 0.05;

var outputDir = Environment.GetEnvironmentVariable("PHASE299_OUTPUT_DIR") ?? DefaultOutputDir;
var packageOutputDir = Path.Combine(outputDir, "full_replay_packages");
Directory.CreateDirectory(outputDir);
Directory.CreateDirectory(packageOutputDir);

var jsonOptions = JsonOptions();
using var spinorDoc = JsonDocument.Parse(File.ReadAllText(SpinorRepresentationPath));
using var bosonRegistry = JsonDocument.Parse(File.ReadAllText(BosonRegistryPath));
using var phase27 = JsonDocument.Parse(File.ReadAllText(Phase27IdentityReadinessPath));
using var phase28 = JsonDocument.Parse(File.ReadAllText(Phase28PromotionResultPath));
using var phase190 = JsonDocument.Parse(File.ReadAllText(Phase190Path));
using var phase191 = JsonDocument.Parse(File.ReadAllText(Phase191Path));
using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase251 = JsonDocument.Parse(File.ReadAllText(Phase251Path));
using var phase297 = JsonDocument.Parse(File.ReadAllText(Phase297Path));
using var phase298 = JsonDocument.Parse(File.ReadAllText(Phase298Path));

var p190SiblingStability = phase190.RootElement.GetProperty("siblingStability");
var p190BestCandidate = p190SiblingStability.GetProperty("bestCandidate");
var p191Gates = phase191.RootElement.GetProperty("gates");
var p190BackgroundRecords = p190BestCandidate.GetProperty("backgroundRecords")
    .EnumerateArray()
    .Select(row => new P190BackgroundRecord(
        RequiredString(row, "backgroundId"),
        RequiredString(row, "fromFermionModeId"),
        RequiredString(row, "toFermionModeId")))
    .ToArray();
var backgroundIds = p190BackgroundRecords.Select(row => row.BackgroundId).ToArray();

var identityRulesReady = JsonString(phase27.RootElement, "terminalStatus") == "identity-rule-ready";
var wIdentityRule = RequiredIdentityRule("w-boson");
var zIdentityRule = RequiredIdentityRule("z-boson");
var wCandidateId = ToRegistryCandidateId(wIdentityRule.SourceId);
var zCandidateId = ToRegistryCandidateId(zIdentityRule.SourceId);
var p190BestCandidateId = RequiredString(p190BestCandidate, "candidateId");
var p190TargetIndependent = JsonBool(phase190.RootElement, "targetObservablesUsed") is false;
var p190TheoremClaimed = JsonBool(phase190.RootElement, "theoremClaimed") is true;
var p190StableCandidateCount = JsonInt(p190SiblingStability, "stableCandidateCount") ?? -1;
var stabilitySpreadTolerance = JsonDouble(p190SiblingStability, "stabilitySpreadTolerance") ?? StabilitySpreadToleranceFallback;
var targetRaw = RequiredDouble(p191Gates, "targetRaw");
var rawGateRatio = RequiredDouble(p191Gates, "rawGateRatio");
var p191RawGatePassed = JsonBool(p191Gates, "rawGatePassed") is true;
var p191WzParticleSplitPresent = JsonBool(p191Gates, "wZParticleSplitPresent") is true;
var phase28RatioOnly = phase28.RootElement.TryGetProperty("physicalObservableMappings", out var physicalMappings)
    && physicalMappings.TryGetProperty("mappings", out var mappings)
    && mappings.EnumerateArray().All(row => JsonString(row, "physicalObservableType") == "mass-ratio");
var p201WzPromotable = phase201.RootElement.TryGetProperty("wzValidation", out var p201WzValidation)
    && JsonBool(p201WzValidation, "promotable") is true;
var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;
var p251InternalIdentityNotAbsoluteSource = JsonBool(phase251.RootElement, "upstreamWzIdentityRuleSourceChainAuditPassed") is true
    && JsonBool(phase251.RootElement, "upstreamFillsWzAbsoluteSourceContract") is false;
var p297CanFillWzSourceContractNow = JsonBool(phase297.RootElement, "canFillWzSourceContractNow") is true;
var p298ProductionInputGapClosed = JsonBool(phase298.RootElement, "productionInputGapClosedForP190BestCandidate") is true;
var p298CanEmitWzSourceRows = JsonBool(phase298.RootElement, "canEmitWzSourceRows") is true;

var provenance = new ProvenanceMeta
{
    CreatedAt = DateTimeOffset.UtcNow,
    CodeRevision = "phase299-identity-split-production-wz-replay-attempt",
    Branch = new()
    {
        BranchId = "phase299-identity-split-production-wz-replay-attempt",
        SchemaVersion = "1.0.0",
    },
    Backend = "cpu-reference",
    Notes = "Production analytic replay over Phase27 identity-selected W and Z candidates using the P190 promoted fermion transition.",
};

var spinorSpec = spinorDoc.RootElement.Deserialize<SpinorRepresentationSpec>(jsonOptions)
    ?? throw new InvalidDataException($"Failed to deserialize {SpinorRepresentationPath}");
var gammas = new GammaMatrixBuilder().Build(spinorSpec.CliffordSignature, spinorSpec.GammaConvention, provenance);
var mesh = ToyGeometryFactory.CreateStructuredFiberBundle2D(rows: 2, cols: 2).AmbientMesh;
var geometry = BuildGeometry(mesh, spinorSpec.SpinorComponents);
int spinorDim = spinorSpec.SpinorComponents;
int dimG = 3;

var identitySelections = new[]
{
    new IdentitySelection("w-boson", wIdentityRule.SourceId, wIdentityRule.SourceObservableId, wIdentityRule.DerivationId, wCandidateId),
    new IdentitySelection("z-boson", zIdentityRule.SourceId, zIdentityRule.SourceObservableId, zIdentityRule.DerivationId, zCandidateId),
};
var replayRows = identitySelections.SelectMany(selection => BuildReplayRows(selection)).ToArray();
var productionReplayBuiltCount = replayRows.Count(row => row.ProductionReplayBuilt);
var materializationAuditPassedCount = replayRows.Count(row => row.MaterializationAuditPassed);
var evidenceValidatedCount = replayRows.Count(row => row.RawMatrixElementEvidenceValidated);
var expectedReplayCount = identitySelections.Length * p190BackgroundRecords.Length;
var allProductionReplaysBuilt = replayRows.Length == expectedReplayCount && replayRows.All(row => row.ProductionReplayBuilt);
var allMaterializationAuditsPassed = replayRows.Length == expectedReplayCount && replayRows.All(row => row.MaterializationAuditPassed);
var allRawMatrixElementEvidenceValidated = replayRows.Length == expectedReplayCount && replayRows.All(row => row.RawMatrixElementEvidenceValidated);
var productionInputGapClosedForIdentitySplitCandidates =
    allProductionReplaysBuilt && allMaterializationAuditsPassed && allRawMatrixElementEvidenceValidated;
var wSummary = BuildParticleSummary("w-boson");
var zSummary = BuildParticleSummary("z-boson");
var identitySplitRawGatePassed = wSummary.RawGatePassed && zSummary.RawGatePassed;
var identitySplitStabilityPassed = wSummary.StabilityPassed && zSummary.StabilityPassed;
var identitySplitRatio = double.IsFinite(wSummary.MeanRawMagnitude) && double.IsFinite(zSummary.MeanRawMagnitude) && zSummary.MeanRawMagnitude != 0.0
    ? wSummary.MeanRawMagnitude / zSummary.MeanRawMagnitude
    : double.NaN;
var identitySplitCandidateMatchesP190BestZ = zCandidateId == p190BestCandidateId;
var identitySplitCandidateMatchesP190BestW = wCandidateId == p190BestCandidateId;
var internalIdentitySplitPresent = identityRulesReady && wCandidateId != zCandidateId;
var productionAnalyticReplayRowsByParticleBuilt = productionInputGapClosedForIdentitySplitCandidates && internalIdentitySplitPresent;
var theoremClaimed = p190TheoremClaimed;
var contractGradeParticleSpecificSourceRowsPresent = false;
var wZParticleSplitPromotable =
    productionAnalyticReplayRowsByParticleBuilt
    && identitySplitRawGatePassed
    && identitySplitStabilityPassed
    && theoremClaimed
    && contractGradeParticleSpecificSourceRowsPresent;
var sourceRowsPromotable = wZParticleSplitPromotable;
var canEmitWzSourceRows = sourceRowsPromotable;
var canFillPhase201WzContract = canEmitWzSourceRows && p201WzPromotable;
var phase201TemplateMutated = false;
var fieldsAppliedToPhase201TemplateCount = 0;
var targetObservablesUsedForConstruction = false;
var targetValuesUsedOnlyForPostReplayEvaluation = true;
var identitySplitProductionWzReplayAttemptPassed =
    identityRulesReady
    && p190TargetIndependent
    && phase28RatioOnly
    && p251InternalIdentityNotAbsoluteSource
    && productionInputGapClosedForIdentitySplitCandidates
    && internalIdentitySplitPresent
    && productionAnalyticReplayRowsByParticleBuilt
    && !identitySplitRawGatePassed
    && !theoremClaimed
    && !contractGradeParticleSpecificSourceRowsPresent
    && !sourceRowsPromotable
    && !canFillPhase201WzContract
    && !phase201TemplateMutated
    && fieldsAppliedToPhase201TemplateCount == 0
    && !p297CanFillWzSourceContractNow
    && p298ProductionInputGapClosed
    && !p298CanEmitWzSourceRows
    && wzMissingFieldCount > 0;

var terminalStatus = !productionInputGapClosedForIdentitySplitCandidates
    ? "identity-split-production-wz-replay-blocked-production-inputs-missing"
    : identitySplitProductionWzReplayAttemptPassed
        ? "identity-split-production-wz-replay-built-raw-gate-and-contract-blocked"
        : sourceRowsPromotable
            ? "identity-split-production-wz-replay-review-source-rows-promotable"
            : "identity-split-production-wz-replay-review-required";

var checks = new[]
{
    new Check(
        "phase27-identity-split-present",
        identityRulesReady && internalIdentitySplitPresent,
        $"identityRulesReady={identityRulesReady}; wCandidateId={wCandidateId}; zCandidateId={zCandidateId}; phase28RatioOnly={phase28RatioOnly}"),
    new Check(
        "identity-selected-production-replays-built",
        productionInputGapClosedForIdentitySplitCandidates,
        $"productionReplayBuiltCount={productionReplayBuiltCount}; materializationAuditPassedCount={materializationAuditPassedCount}; evidenceValidatedCount={evidenceValidatedCount}; expectedReplayCount={expectedReplayCount}"),
    new Check(
        "identity-split-rows-are-target-independent-construction",
        targetObservablesUsedForConstruction is false && targetValuesUsedOnlyForPostReplayEvaluation,
        $"targetObservablesUsedForConstruction={targetObservablesUsedForConstruction}; targetValuesUsedOnlyForPostReplayEvaluation={targetValuesUsedOnlyForPostReplayEvaluation}"),
    new Check(
        "identity-split-still-fails-raw-gate",
        !identitySplitRawGatePassed && !wSummary.RawGatePassed && !zSummary.RawGatePassed,
        $"wRawToTargetRatio={wSummary.RawToTargetRatio:R}; zRawToTargetRatio={zSummary.RawToTargetRatio:R}; rawGateRatio={rawGateRatio:R}; p191RawGatePassed={p191RawGatePassed}"),
    new Check(
        "identity-split-is-not-contract-grade-source-row-split",
        !theoremClaimed && !contractGradeParticleSpecificSourceRowsPresent && !sourceRowsPromotable,
        $"theoremClaimed={theoremClaimed}; contractGradeParticleSpecificSourceRowsPresent={contractGradeParticleSpecificSourceRowsPresent}; sourceRowsPromotable={sourceRowsPromotable}; p191WzParticleSplitPresent={p191WzParticleSplitPresent}"),
    new Check(
        "phase201-contract-not-filled-or-mutated",
        !canFillPhase201WzContract && !phase201TemplateMutated && fieldsAppliedToPhase201TemplateCount == 0 && wzMissingFieldCount > 0,
        $"canFillPhase201WzContract={canFillPhase201WzContract}; phase201TemplateMutated={phase201TemplateMutated}; wzMissingFieldCount={wzMissingFieldCount}"),
};

var result = new
{
    phaseId = "phase299-identity-split-production-wz-replay-attempt",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    identitySplitProductionWzReplayAttemptPassed,
    applicationAttempted = true,
    targetObservablesUsedForConstruction,
    targetValuesUsedOnlyForPostReplayEvaluation,
    identityRulesReady,
    phase28RatioOnly,
    p251InternalIdentityNotAbsoluteSource,
    identitySelections,
    p190BestCandidateId,
    p190StableCandidateCount,
    identitySplitCandidateMatchesP190BestW,
    identitySplitCandidateMatchesP190BestZ,
    internalIdentitySplitPresent,
    productionAnalyticReplayRowsByParticleBuilt,
    productionInputGapClosedForIdentitySplitCandidates,
    expectedReplayCount,
    productionReplayBuiltCount,
    materializationAuditPassedCount,
    evidenceValidatedCount,
    allProductionReplaysBuilt,
    allMaterializationAuditsPassed,
    allRawMatrixElementEvidenceValidated,
    targetRaw,
    rawGateRatio,
    wSummary,
    zSummary,
    identitySplitRatio,
    identitySplitRawGatePassed,
    stabilitySpreadTolerance,
    identitySplitStabilityPassed,
    theoremClaimed,
    contractGradeParticleSpecificSourceRowsPresent,
    wZParticleSplitPromotable,
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
        phase297 = new
        {
            p297CanFillWzSourceContractNow,
        },
        phase298 = new
        {
            p298ProductionInputGapClosed,
            p298CanEmitWzSourceRows,
        },
    },
    replayRows,
    checks,
    decision = identitySplitProductionWzReplayAttemptPassed
        ? "The Phase27 identity-selected W and Z candidates can be replayed as separate source-backed analytic rows, but the transfer does not complete the W/Z source contract. The rows remain far below the raw gate, the identity split is ratio/internal-mode evidence rather than a direct bridge theorem, and no contract-grade W/Z source rows can be emitted."
        : "Review the identity-split production W/Z replay attempt before relying on this route.",
    nextRequiredArtifact = new[]
    {
        "A derivation-backed theorem that transfers Phase27 internal W/Z identity labels into direct bridge source rows.",
        "Source-derived raw-amplitude closure for both identity-selected W and Z rows.",
        "Phase201/P209 source-lineage rows with sourceLineageId, theoremOrDerivationId, sourceRowId, raw/common/target gates, and stability sidecars.",
    },
    sourceEvidence = new
    {
        spinorRepresentationPath = SpinorRepresentationPath,
        bosonRegistryPath = BosonRegistryPath,
        bosonModeVectorDir = BosonModeVectorDir,
        promotedFermionModeDir = PromotedFermionModeDir,
        phase27IdentityReadinessPath = Phase27IdentityReadinessPath,
        phase28PromotionResultPath = Phase28PromotionResultPath,
        phase190Path = Phase190Path,
        phase191Path = Phase191Path,
        phase201Path = Phase201Path,
        phase213Path = Phase213Path,
        phase251Path = Phase251Path,
        phase297Path = Phase297Path,
        phase298Path = Phase298Path,
    },
};

File.WriteAllText(
    Path.Combine(outputDir, "identity_split_production_wz_replay_attempt.json"),
    JsonSerializer.Serialize(result, jsonOptions));
File.WriteAllText(
    Path.Combine(outputDir, "identity_split_production_wz_replay_attempt_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.identitySplitProductionWzReplayAttemptPassed,
        result.applicationAttempted,
        result.targetObservablesUsedForConstruction,
        result.targetValuesUsedOnlyForPostReplayEvaluation,
        result.identityRulesReady,
        result.phase28RatioOnly,
        result.p251InternalIdentityNotAbsoluteSource,
        result.identitySelections,
        result.p190BestCandidateId,
        result.p190StableCandidateCount,
        result.identitySplitCandidateMatchesP190BestW,
        result.identitySplitCandidateMatchesP190BestZ,
        result.internalIdentitySplitPresent,
        result.productionAnalyticReplayRowsByParticleBuilt,
        result.productionInputGapClosedForIdentitySplitCandidates,
        result.expectedReplayCount,
        result.productionReplayBuiltCount,
        result.materializationAuditPassedCount,
        result.evidenceValidatedCount,
        result.allProductionReplaysBuilt,
        result.allMaterializationAuditsPassed,
        result.allRawMatrixElementEvidenceValidated,
        result.targetRaw,
        result.rawGateRatio,
        result.wSummary,
        result.zSummary,
        result.identitySplitRatio,
        result.identitySplitRawGatePassed,
        result.stabilitySpreadTolerance,
        result.identitySplitStabilityPassed,
        result.theoremClaimed,
        result.contractGradeParticleSpecificSourceRowsPresent,
        result.wZParticleSplitPromotable,
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
            row.ParticleId,
            row.CandidateId,
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
            row.VariationEvidenceId,
            row.FullReplayPackagePath,
        }),
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, jsonOptions));

Console.WriteLine(terminalStatus);
Console.WriteLine($"identitySplitProductionWzReplayAttemptPassed={identitySplitProductionWzReplayAttemptPassed}");
Console.WriteLine($"wCandidateId={wCandidateId}");
Console.WriteLine($"zCandidateId={zCandidateId}");
Console.WriteLine($"productionInputGapClosedForIdentitySplitCandidates={productionInputGapClosedForIdentitySplitCandidates}");
Console.WriteLine($"productionReplayBuiltCount={productionReplayBuiltCount}");
Console.WriteLine($"materializationAuditPassedCount={materializationAuditPassedCount}");
Console.WriteLine($"evidenceValidatedCount={evidenceValidatedCount}");
Console.WriteLine($"wMeanRawMagnitude={wSummary.MeanRawMagnitude:R}");
Console.WriteLine($"zMeanRawMagnitude={zSummary.MeanRawMagnitude:R}");
Console.WriteLine($"wRawToTargetRatio={wSummary.RawToTargetRatio:R}");
Console.WriteLine($"zRawToTargetRatio={zSummary.RawToTargetRatio:R}");
Console.WriteLine($"identitySplitRawGatePassed={identitySplitRawGatePassed}");
Console.WriteLine($"wStabilityPassed={wSummary.StabilityPassed}");
Console.WriteLine($"zStabilityPassed={zSummary.StabilityPassed}");
Console.WriteLine($"theoremClaimed={theoremClaimed}");
Console.WriteLine($"contractGradeParticleSpecificSourceRowsPresent={contractGradeParticleSpecificSourceRowsPresent}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");

IEnumerable<ReplayRow> BuildReplayRows(IdentitySelection selection)
{
    var registryCandidate = bosonRegistry.RootElement.GetProperty("candidates")
        .EnumerateArray()
        .Single(candidate => RequiredString(candidate, "candidateId") == selection.CandidateId);
    var contributingModeIds = registryCandidate.GetProperty("contributingModeIds")
        .EnumerateArray()
        .Select(mode => mode.GetString() ?? "")
        .Where(mode => mode.Length > 0)
        .ToArray();

    foreach (var p190Row in p190BackgroundRecords)
    {
        var etaModeId = contributingModeIds.Single(modeId => modeId.Contains(p190Row.BackgroundId, StringComparison.Ordinal));
        yield return BuildReplayRow(selection, p190Row, etaModeId);
    }
}

ReplayRow BuildReplayRow(IdentitySelection selection, P190BackgroundRecord p190Row, string etaModeId)
{
    var bosonModePath = Path.Combine(BosonModeVectorDir, etaModeId + ".json");
    var fermionModesPath = PromotedFermionModePath(p190Row.BackgroundId);
    var fermionModes = LoadJson<PromotedFermionModeBundle>(fermionModesPath).Modes;
    var modeI = fermionModes.Single(mode => string.Equals(mode.ModeId, p190Row.FromFermionModeId, StringComparison.Ordinal));
    var modeJ = fermionModes.Single(mode => string.Equals(mode.ModeId, p190Row.ToFermionModeId, StringComparison.Ordinal));
    var packageId = $"phase299-identity-split-{selection.ParticleId}-{Sanitize(p190Row.BackgroundId)}-{Sanitize(etaModeId)}";
    var provenanceId = $"phase299:{selection.ParticleId}:{p190Row.BackgroundId}:{etaModeId}:{p190Row.FromFermionModeId}->{p190Row.ToFermionModeId}";
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

    return new ReplayRow(
        selection.ParticleId,
        selection.CandidateId,
        selection.SourceId,
        selection.SourceObservableId,
        selection.DerivationId,
        p190Row.BackgroundId,
        etaModeId,
        p190Row.FromFermionModeId,
        p190Row.ToFermionModeId,
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
        rawGatePassedForRow);
}

ParticleReplaySummary BuildParticleSummary(string particleId)
{
    var rows = replayRows.Where(row => row.ParticleId == particleId).ToArray();
    var rawValues = rows.Select(row => row.RawMatrixElementMagnitude).Where(value => value.HasValue).Select(value => value!.Value).ToArray();
    var meanRawMagnitude = rawValues.Length == 0 ? double.NaN : rawValues.Average();
    var rawToTargetRatio = double.IsFinite(meanRawMagnitude) ? meanRawMagnitude / targetRaw : double.NaN;
    var relativeSpread = RelativeSpread(rawValues);
    return new ParticleReplaySummary(
        particleId,
        rows.FirstOrDefault()?.CandidateId ?? "",
        rows.Length,
        meanRawMagnitude,
        rawToTargetRatio,
        double.IsFinite(rawToTargetRatio) && rawToTargetRatio >= rawGateRatio,
        relativeSpread,
        double.IsFinite(relativeSpread) && relativeSpread <= stabilitySpreadTolerance,
        rows.Count(row => row.ProductionReplayBuilt),
        rows.Count(row => row.MaterializationAuditPassed),
        rows.Count(row => row.RawMatrixElementEvidenceValidated));
}

IdentityRule RequiredIdentityRule(string particleId)
{
    var rule = phase27.RootElement.GetProperty("derivedRules")
        .EnumerateArray()
        .Single(row => JsonString(row, "particleId") == particleId);
    return new IdentityRule(
        RequiredString(rule, "particleId"),
        RequiredString(rule, "sourceId"),
        RequiredString(rule, "sourceObservableId"),
        RequiredString(rule, "derivationId"));
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

    return new GeometryReplayInputs(edgeLengths, cellsPerEdge, edgeDirections);
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

static double RelativeSpread(IReadOnlyCollection<double> values)
{
    if (values.Count == 0)
    {
        return double.NaN;
    }

    var mean = values.Average();
    return (values.Max() - values.Min()) / Math.Max(Math.Abs(mean), 1e-300);
}

static string ToRegistryCandidateId(string sourceId)
{
    var index = sourceId.LastIndexOf("candidate-", StringComparison.Ordinal);
    if (index < 0)
    {
        throw new InvalidDataException($"Cannot extract registry candidate id from '{sourceId}'.");
    }

    return sourceId[index..];
}

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

sealed record IdentityRule(string ParticleId, string SourceId, string SourceObservableId, string DerivationId);

sealed record IdentitySelection(
    string ParticleId,
    string SourceId,
    string SourceObservableId,
    string DerivationId,
    string CandidateId);

sealed record GeometryReplayInputs(double[] EdgeLengths, int[][] CellsPerEdge, double[][] EdgeDirections);

sealed record P190BackgroundRecord(string BackgroundId, string FromFermionModeId, string ToFermionModeId);

sealed record ParticleReplaySummary(
    string ParticleId,
    string CandidateId,
    int RowCount,
    double MeanRawMagnitude,
    double RawToTargetRatio,
    bool RawGatePassed,
    double RelativeSpread,
    bool StabilityPassed,
    int ProductionReplayBuiltCount,
    int MaterializationAuditPassedCount,
    int EvidenceValidatedCount);

sealed record ReplayRow(
    string ParticleId,
    string CandidateId,
    string SourceId,
    string SourceObservableId,
    string DerivationId,
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
    bool RawGatePassed);

sealed record Check(string CheckId, bool Passed, string Detail);
