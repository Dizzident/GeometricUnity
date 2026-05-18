using System.Text.Json;

const string DefaultOutputDir = "studies/phase307_target_independent_decoupled_wz_row_selection_law_audit_001/output";
const string BosonRegistryPath = "studies/phase12_joined_calculation_001/output/background_family/bosons/registry.json";
const string VariationMatrixDir = "studies/phase12_joined_calculation_001/output/background_family/fermions/couplings/variations";
const string Phase27FamiliesPath = "studies/phase27_charge_sector_convention_001/mode_families_with_charge_sectors.json";
const string Phase91Dir = "studies/phase91_branch_stability_evidence_promotion_001/output";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase299Path = "studies/phase299_identity_split_production_wz_replay_attempt_001/output/identity_split_production_wz_replay_attempt_summary.json";
const string Phase302Path = "studies/phase302_identity_split_particle_normalization_audit_001/output/identity_split_particle_normalization_audit_summary.json";
const string Phase305Path = "studies/phase305_phase27_charged_ladder_operator_wz_source_audit_001/output/phase27_charged_ladder_operator_wz_source_audit_summary.json";
const string Phase306Path = "studies/phase306_decoupled_charged_ladder_wz_row_source_audit_001/output/decoupled_charged_ladder_wz_row_source_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE307_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var bosonRegistry = JsonDocument.Parse(File.ReadAllText(BosonRegistryPath));
using var phase27Families = JsonDocument.Parse(File.ReadAllText(Phase27FamiliesPath));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase299 = JsonDocument.Parse(File.ReadAllText(Phase299Path));
using var phase302 = JsonDocument.Parse(File.ReadAllText(Phase302Path));
using var phase305 = JsonDocument.Parse(File.ReadAllText(Phase305Path));
using var phase306 = JsonDocument.Parse(File.ReadAllText(Phase306Path));

var backgrounds = phase299.RootElement.GetProperty("rows")
    .EnumerateArray()
    .Select(row => RequiredString(row, "backgroundId"))
    .Distinct(StringComparer.Ordinal)
    .Order(StringComparer.Ordinal)
    .ToArray();
double targetRaw = RequiredDouble(phase299.RootElement, "targetRaw");
double rawGateRatio = RequiredDouble(phase299.RootElement, "rawGateRatio");
double stabilitySpreadTolerance = RequiredDouble(phase299.RootElement, "stabilitySpreadTolerance");
double commonScaleSpreadTolerance = 0.05;
double p302WTotalScale = RequiredDouble(phase302.RootElement.GetProperty("bestSourceInvariantRawCommonCandidate"), "wTotalScale");
double p302ZTotalScale = RequiredDouble(phase302.RootElement.GetProperty("bestSourceInvariantRawCommonCandidate"), "zTotalScale");
int wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
int higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;
bool p305Passed = JsonBool(phase305.RootElement, "phase27ChargedLadderOperatorWzSourceAuditPassed") is true;
bool p305CanFillContract = JsonBool(phase305.RootElement, "canFillPhase201WzContract") is true;
bool p306Passed = JsonBool(phase306.RootElement, "decoupledChargedLadderWzRowSourceAuditPassed") is true;
bool p306CanFillContract = JsonBool(phase306.RootElement, "canFillPhase201WzContract") is true;

var candidates = bosonRegistry.RootElement.GetProperty("candidates")
    .EnumerateArray()
    .Select(candidate => new CandidateRecord(
        RequiredString(candidate, "candidateId"),
        candidate.GetProperty("contributingModeIds")
            .EnumerateArray()
            .Select(item => item.GetString() ?? "")
            .Where(item => item.Length > 0)
            .ToArray()))
    .OrderBy(candidate => CandidateOrdinal(candidate.CandidateId))
    .ThenBy(candidate => candidate.CandidateId, StringComparer.Ordinal)
    .ToArray();
var candidateMetadata = LoadCandidateMetadata();
var chargedAxis0CandidateIds = candidateMetadata.Values
    .Where(candidate => string.Equals(candidate.ChargeSector, "charged", StringComparison.Ordinal) && candidate.DominantBasisIndex == 0)
    .Select(candidate => candidate.CandidateId)
    .OrderBy(CandidateOrdinal)
    .ThenBy(id => id, StringComparer.Ordinal)
    .ToArray();
var chargedAxis1CandidateIds = candidateMetadata.Values
    .Where(candidate => string.Equals(candidate.ChargeSector, "charged", StringComparison.Ordinal) && candidate.DominantBasisIndex == 1)
    .Select(candidate => candidate.CandidateId)
    .OrderBy(CandidateOrdinal)
    .ThenBy(id => id, StringComparer.Ordinal)
    .ToArray();
var neutralAxisCandidateIds = candidateMetadata.Values
    .Where(candidate => string.Equals(candidate.ChargeSector, "neutral", StringComparison.Ordinal) && candidate.DominantBasisIndex == 2)
    .Select(candidate => candidate.CandidateId)
    .OrderBy(CandidateOrdinal)
    .ThenBy(id => id, StringComparer.Ordinal)
    .ToArray();

var sourceDefinitions = BuildSourceDefinitions();
var backgroundModes = backgrounds.ToDictionary(
    backgroundId => backgroundId,
    backgroundId => LoadModes(Path.Combine(Phase91Dir, backgroundId, "branch_stability_promoted_fermion_modes.json")),
    StringComparer.Ordinal);
var pairKeys = backgroundModes[backgrounds[0]]
    .SelectMany(
        from => backgroundModes[backgrounds[0]].Where(to => to.ModeIndex != from.ModeIndex),
        (from, to) => new PairKey(from.ModeIndex, to.ModeIndex))
    .OrderBy(pair => pair.FromModeIndex)
    .ThenBy(pair => pair.ToModeIndex)
    .ToArray();
var matrices = LoadMatrices(backgrounds, candidates);

var assessments = pairKeys
    .SelectMany(pair => sourceDefinitions.Select(definition => AssessDefinition(definition, pair)))
    .OrderBy(assessment => assessment.MaxParticleRelativeSpread)
    .ThenByDescending(assessment => assessment.MinP302ScaledRowRawToTargetRatio)
    .ThenBy(assessment => assessment.P302ScaledCommonMeanRelativeSpread)
    .ThenBy(assessment => assessment.DefinitionId, StringComparer.Ordinal)
    .ThenBy(assessment => assessment.Pair.FromModeIndex)
    .ThenBy(assessment => assessment.Pair.ToModeIndex)
    .ToArray();

var stableAssessments = assessments.Where(assessment => assessment.AllParticlesStabilityPassed).ToArray();
var allRowsRawPassingAssessments = assessments.Where(assessment => assessment.AllRowsRawGatePassed).ToArray();
var p302ScaledAllRowsRawPassingAssessments = assessments.Where(assessment => assessment.P302ScaledAllRowsRawGatePassed).ToArray();
var stableRawCommonAssessments = assessments.Where(assessment => assessment.StableRawCommonPassed).ToArray();
var stableP302ScaledAssessments = assessments.Where(assessment => assessment.P302ScaledStableRawCommonPassed).ToArray();
var stableBlockedAssessments = stableAssessments
    .Where(assessment => !assessment.StableRawCommonPassed && !assessment.P302ScaledStableRawCommonPassed)
    .OrderByDescending(assessment => assessment.MinP302ScaledRowRawToTargetRatio)
    .ThenBy(assessment => assessment.P302ScaledCommonMeanRelativeSpread)
    .ToArray();
var bestAssessment = assessments.FirstOrDefault();
var bestP302ScaledRawAssessment = p302ScaledAllRowsRawPassingAssessments
    .OrderBy(assessment => assessment.MaxParticleRelativeSpread)
    .ThenBy(assessment => assessment.P302ScaledCommonMeanRelativeSpread)
    .ThenByDescending(assessment => assessment.MinP302ScaledRowRawToTargetRatio)
    .FirstOrDefault();
var closestScaledCommonAssessment = p302ScaledAllRowsRawPassingAssessments
    .OrderBy(assessment => assessment.P302ScaledCommonMeanRelativeSpread)
    .ThenBy(assessment => assessment.MaxParticleRelativeSpread)
    .ThenByDescending(assessment => assessment.MinP302ScaledRowRawToTargetRatio)
    .FirstOrDefault();
var particleRows = assessments
    .SelectMany(assessment => new[]
    {
        BuildParticleRowCandidate(assessment, "w-boson"),
        BuildParticleRowCandidate(assessment, "z-boson"),
    })
    .ToArray();
var wParticleRows = particleRows.Where(row => row.ParticleId == "w-boson").ToArray();
var zParticleRows = particleRows.Where(row => row.ParticleId == "z-boson").ToArray();
var wStableRows = wParticleRows.Where(row => row.StabilityPassed).ToArray();
var zStableRows = zParticleRows.Where(row => row.StabilityPassed).ToArray();
var wStableRawRows = wStableRows.Where(row => row.RawGatePassed).ToArray();
var zStableRawRows = zStableRows.Where(row => row.RawGatePassed).ToArray();
var wStableP302ScaledRawRows = wStableRows.Where(row => row.P302ScaledRawGatePassed).ToArray();
var zStableP302ScaledRawRows = zStableRows.Where(row => row.P302ScaledRawGatePassed).ToArray();
var stableDecoupledAssessments = wStableRows
    .SelectMany(wRow => zStableRows.Select(zRow => BuildDecoupledAssessment(wRow, zRow)))
    .ToArray();
var decoupledRawAssessments = stableDecoupledAssessments
    .Where(assessment => assessment.BothParticlesRawGatePassed)
    .ToArray();
var decoupledP302ScaledAssessments = stableDecoupledAssessments
    .Where(assessment => assessment.BothParticlesP302ScaledRawGatePassed)
    .ToArray();
var decoupledRawCommonPassingAssessments = decoupledRawAssessments
    .Where(assessment => assessment.RawStableCommonPassed)
    .ToArray();
var decoupledP302ScaledCommonPassingAssessments = decoupledP302ScaledAssessments
    .Where(assessment => assessment.P302ScaledStableCommonPassed)
    .OrderByDescending(assessment => assessment.MinP302ScaledRowRawToTargetRatio)
    .ThenBy(assessment => assessment.P302ScaledCommonMeanRelativeSpread)
    .ToArray();
var bestDecoupledP302ScaledCommonAssessment = decoupledP302ScaledAssessments
    .OrderBy(assessment => assessment.P302ScaledCommonMeanRelativeSpread)
    .ThenByDescending(assessment => assessment.MinP302ScaledRowRawToTargetRatio)
    .FirstOrDefault();
var targetIndependentSelectionLawAssessments = BuildTargetIndependentSelectionLawAssessments(stableDecoupledAssessments).ToArray();
var selectionLawsUsingP302Scale = targetIndependentSelectionLawAssessments
    .Where(assessment => assessment.UsesP302ScaleForSelection)
    .ToArray();
var selectionLawsSelectingRawStableCommon = targetIndependentSelectionLawAssessments
    .Where(assessment => assessment.SelectedRawStableCommonPassed)
    .ToArray();
var selectionLawsSelectingP302ScaledStableCommon = targetIndependentSelectionLawAssessments
    .Where(assessment => assessment.SelectedP302ScaledStableCommonPassed)
    .ToArray();
var selectionLawsSelectingP302ScaledNearPassWithoutRaw = targetIndependentSelectionLawAssessments
    .Where(assessment => !assessment.SelectedRawStableCommonPassed && assessment.SelectedP302ScaledStableCommonPassed)
    .ToArray();
var selectionLawsThatCanFillPhase201WzContract = targetIndependentSelectionLawAssessments
    .Where(assessment => assessment.CanFillPhase201WzContract)
    .ToArray();
var bestTargetIndependentSelectionLaw = targetIndependentSelectionLawAssessments
    .OrderBy(assessment => assessment.SelectedAssessment.P302ScaledCommonMeanRelativeSpread)
    .ThenByDescending(assessment => assessment.SelectedAssessment.MinP302ScaledRowRawToTargetRatio)
    .ThenBy(assessment => assessment.LawId, StringComparer.Ordinal)
    .FirstOrDefault();
var bestP302ScaledStableCommonSelectionLaw = selectionLawsSelectingP302ScaledStableCommon
    .OrderBy(assessment => assessment.SelectedAssessment.P302ScaledCommonMeanRelativeSpread)
    .ThenByDescending(assessment => assessment.SelectedAssessment.MinP302ScaledRowRawToTargetRatio)
    .ThenBy(assessment => assessment.LawId, StringComparer.Ordinal)
    .FirstOrDefault();
bool targetObservablesUsedForConstruction = false;
bool targetValuesUsedOnlyForPostCandidateEvaluation = true;
bool theoremClaimed = false;
bool sourceRowsPromotable = false;
bool canFillPhase201WzContract = false;

var checks = new[]
{
    new Check(
        "phase27-axis-metadata-available",
        chargedAxis0CandidateIds.Length > 0 && chargedAxis1CandidateIds.Length > 0 && neutralAxisCandidateIds.Length > 0,
        $"chargedAxis0CandidateCount={chargedAxis0CandidateIds.Length}; chargedAxis1CandidateCount={chargedAxis1CandidateIds.Length}; neutralAxisCandidateCount={neutralAxisCandidateIds.Length}"),
    new Check(
        "upstream-phase305-charged-ladder-blocker-preserved",
        p305Passed && !p305CanFillContract,
        $"p305Passed={p305Passed}; p305CanFillContract={p305CanFillContract}"),
    new Check(
        "upstream-phase306-decoupled-row-blocker-preserved",
        p306Passed && !p306CanFillContract,
        $"p306Passed={p306Passed}; p306CanFillContract={p306CanFillContract}"),
    new Check(
        "target-independent-selection-laws-materialized",
        !targetObservablesUsedForConstruction
            && targetValuesUsedOnlyForPostCandidateEvaluation
            && targetIndependentSelectionLawAssessments.Length >= 6
            && targetIndependentSelectionLawAssessments.All(assessment => !assessment.UsesTargetsForSelection),
        $"targetObservablesUsedForConstruction={targetObservablesUsedForConstruction}; targetValuesUsedOnlyForPostCandidateEvaluation={targetValuesUsedOnlyForPostCandidateEvaluation}; selectionLawCount={targetIndependentSelectionLawAssessments.Length}; selectionLawUsesTargetCount={targetIndependentSelectionLawAssessments.Count(assessment => assessment.UsesTargetsForSelection)}"),
    new Check(
        "no-charged-ladder-operator-clears-current-raw-stability-common-gates",
        stableRawCommonAssessments.Length == 0,
        $"stableRawCommonAssessmentCount={stableRawCommonAssessments.Length}; allRowsRawPassingAssessmentCount={allRowsRawPassingAssessments.Length}; stableAssessmentCount={stableAssessments.Length}; bestAssessment={bestAssessment?.AssessmentId}; bestMaxParticleRelativeSpread={bestAssessment?.MaxParticleRelativeSpread:R}"),
    new Check(
        "same-definition-phase305-blocker-preserved",
        stableP302ScaledAssessments.Length == 0,
        $"p302ScaledStableRawCommonAssessmentCount={stableP302ScaledAssessments.Length}; p302ScaledAllRowsRawPassingAssessmentCount={p302ScaledAllRowsRawPassingAssessments.Length}; bestP302ScaledRawAssessment={bestP302ScaledRawAssessment?.AssessmentId}; bestP302ScaledRawAssessmentMaxSpread={bestP302ScaledRawAssessment?.MaxParticleRelativeSpread:R}"),
    new Check(
        "decoupled-unscaled-raw-common-route-still-blocked",
        decoupledRawCommonPassingAssessments.Length == 0,
        $"wStableRawRowCount={wStableRawRows.Length}; zStableRawRowCount={zStableRawRows.Length}; decoupledRawCommonPassingAssessmentCount={decoupledRawCommonPassingAssessments.Length}"),
    new Check(
        "decoupled-phase302-scaled-numerical-near-pass-materialized",
        decoupledP302ScaledCommonPassingAssessments.Length > 0,
        $"wStableP302ScaledRawRowCount={wStableP302ScaledRawRows.Length}; zStableP302ScaledRawRowCount={zStableP302ScaledRawRows.Length}; decoupledP302ScaledCommonPassingAssessmentCount={decoupledP302ScaledCommonPassingAssessments.Length}; bestDecoupledP302ScaledCommonAssessment={bestDecoupledP302ScaledCommonAssessment?.AssessmentId}; bestP302ScaledCommonMeanRelativeSpread={bestDecoupledP302ScaledCommonAssessment?.P302ScaledCommonMeanRelativeSpread:R}; bestMinP302ScaledRowRawToTargetRatio={bestDecoupledP302ScaledCommonAssessment?.MinP302ScaledRowRawToTargetRatio:R}"),
    new Check(
        "selection-laws-find-no-unscaled-raw-common-source",
        selectionLawsSelectingRawStableCommon.Length == 0,
        $"selectionLawCount={targetIndependentSelectionLawAssessments.Length}; rawStableCommonSelectionLawCount={selectionLawsSelectingRawStableCommon.Length}; bestSelectionLaw={bestTargetIndependentSelectionLaw?.LawId}; bestSelectionAssessment={bestTargetIndependentSelectionLaw?.SelectedAssessment.AssessmentId}"),
    new Check(
        "selection-laws-can-find-phase302-scaled-near-pass-only",
        selectionLawsSelectingP302ScaledStableCommon.Length > 0 && selectionLawsSelectingP302ScaledNearPassWithoutRaw.Length > 0,
        $"p302ScaleUsingSelectionLawCount={selectionLawsUsingP302Scale.Length}; p302ScaledStableCommonSelectionLawCount={selectionLawsSelectingP302ScaledStableCommon.Length}; p302ScaledNearPassWithoutRawSelectionLawCount={selectionLawsSelectingP302ScaledNearPassWithoutRaw.Length}; bestP302ScaledStableCommonSelectionLaw={bestP302ScaledStableCommonSelectionLaw?.LawId}; bestP302ScaledStableCommonSelectionAssessment={bestP302ScaledStableCommonSelectionLaw?.SelectedAssessment.AssessmentId}; bestP302ScaledStableCommonSelectionSpread={bestP302ScaledStableCommonSelectionLaw?.SelectedAssessment.P302ScaledCommonMeanRelativeSpread:R}; bestP302ScaledStableCommonSelectionMinRaw={bestP302ScaledStableCommonSelectionLaw?.SelectedAssessment.MinP302ScaledRowRawToTargetRatio:R}"),
    new Check(
        "source-contract-remains-blocked",
        !theoremClaimed
            && !sourceRowsPromotable
            && !canFillPhase201WzContract
            && selectionLawsThatCanFillPhase201WzContract.Length == 0
            && wzMissingFieldCount > 0
            && higgsMissingFieldCount > 0,
        $"theoremClaimed={theoremClaimed}; sourceRowsPromotable={sourceRowsPromotable}; canFillPhase201WzContract={canFillPhase201WzContract}; selectionLawCanFillPhase201WzContractCount={selectionLawsThatCanFillPhase201WzContract.Length}; wzMissingFieldCount={wzMissingFieldCount}; higgsMissingFieldCount={higgsMissingFieldCount}"),
};

bool targetIndependentDecoupledWzRowSelectionLawAuditPassed =
    checks.All(check => check.Passed)
    && stableRawCommonAssessments.Length == 0
    && stableP302ScaledAssessments.Length == 0
    && decoupledRawCommonPassingAssessments.Length == 0
    && decoupledP302ScaledCommonPassingAssessments.Length > 0
    && selectionLawsSelectingRawStableCommon.Length == 0
    && selectionLawsSelectingP302ScaledStableCommon.Length > 0
    && selectionLawsThatCanFillPhase201WzContract.Length == 0
    && !sourceRowsPromotable
    && !canFillPhase201WzContract;

var terminalStatus = targetIndependentDecoupledWzRowSelectionLawAuditPassed
    ? "target-independent-decoupled-wz-row-selection-law-audit-scaled-selector-not-promotable"
    : "target-independent-decoupled-wz-row-selection-law-audit-review-required";

var result = new
{
    phaseId = "phase307-target-independent-decoupled-wz-row-selection-law-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    targetIndependentDecoupledWzRowSelectionLawAuditPassed,
    targetObservablesUsedForConstruction,
    targetValuesUsedOnlyForPostCandidateEvaluation,
    targetRaw,
    rawGateRatio,
    stabilitySpreadTolerance,
    commonScaleSpreadTolerance,
    p302WTotalScale,
    p302ZTotalScale,
    backgroundIds = backgrounds,
    pairCount = pairKeys.Length,
    definitionCount = sourceDefinitions.Length,
    assessmentCount = assessments.Length,
    chargedAxis0CandidateIds,
    chargedAxis1CandidateIds,
    neutralAxisCandidateIds,
    canonicalChargedOperator = "T+/-=(axis0 +/- i axis1)/sqrt(2), evaluated on Phase27 charged axes 0 and 1.",
    canonicalNeutralOperator = "axis2 neutral candidate, coherent neutral-axis sum, or neutral-axis root-sum-square depending on source definition.",
    allRowsRawPassingAssessmentCount = allRowsRawPassingAssessments.Length,
    p302ScaledAllRowsRawPassingAssessmentCount = p302ScaledAllRowsRawPassingAssessments.Length,
    stableAssessmentCount = stableAssessments.Length,
    stableRawCommonAssessmentCount = stableRawCommonAssessments.Length,
    p302ScaledStableRawCommonAssessmentCount = stableP302ScaledAssessments.Length,
    stableButRawOrCommonBlockedAssessmentCount = stableBlockedAssessments.Length,
    particleRowCount = particleRows.Length,
    wParticleRowCount = wParticleRows.Length,
    zParticleRowCount = zParticleRows.Length,
    wStableRowCount = wStableRows.Length,
    zStableRowCount = zStableRows.Length,
    wStableRawRowCount = wStableRawRows.Length,
    zStableRawRowCount = zStableRawRows.Length,
    wStableP302ScaledRawRowCount = wStableP302ScaledRawRows.Length,
    zStableP302ScaledRawRowCount = zStableP302ScaledRawRows.Length,
    stableDecoupledAssessmentCount = stableDecoupledAssessments.Length,
    decoupledRawAssessmentCount = decoupledRawAssessments.Length,
    decoupledP302ScaledAssessmentCount = decoupledP302ScaledAssessments.Length,
    decoupledRawCommonPassingAssessmentCount = decoupledRawCommonPassingAssessments.Length,
    decoupledP302ScaledCommonPassingAssessmentCount = decoupledP302ScaledCommonPassingAssessments.Length,
    numericalP302ScaledDecoupledNearPassPresent = decoupledP302ScaledCommonPassingAssessments.Length > 0,
    selectionLawCount = targetIndependentSelectionLawAssessments.Length,
    p302ScaleUsingSelectionLawCount = selectionLawsUsingP302Scale.Length,
    rawStableCommonSelectionLawCount = selectionLawsSelectingRawStableCommon.Length,
    p302ScaledStableCommonSelectionLawCount = selectionLawsSelectingP302ScaledStableCommon.Length,
    p302ScaledNearPassWithoutRawSelectionLawCount = selectionLawsSelectingP302ScaledNearPassWithoutRaw.Length,
    selectionLawCanFillPhase201WzContractCount = selectionLawsThatCanFillPhase201WzContract.Length,
    targetIndependentSelectionLawAssessments,
    bestTargetIndependentSelectionLaw,
    bestP302ScaledStableCommonSelectionLaw,
    theoremClaimed,
    sourceRowsPromotable,
    canFillPhase201WzContract,
    wzMissingFieldCount,
    higgsMissingFieldCount,
    bestAssessment,
    bestP302ScaledRawAssessment,
    closestScaledCommonAssessment,
    bestDecoupledP302ScaledCommonAssessment,
    topDecoupledP302ScaledCommonAssessments = decoupledP302ScaledCommonPassingAssessments.Take(24).ToArray(),
    topAssessments = assessments.Take(24).ToArray(),
    topP302ScaledRawAssessments = p302ScaledAllRowsRawPassingAssessments
        .OrderBy(assessment => assessment.MaxParticleRelativeSpread)
        .ThenBy(assessment => assessment.P302ScaledCommonMeanRelativeSpread)
        .Take(24)
        .ToArray(),
    topStableAssessments = stableBlockedAssessments.Take(12).ToArray(),
    sourceDefinitions,
    inheritedBlockers = new
    {
        phase305 = new
        {
            p305Passed,
            p305CanFillContract,
            p305StableRawCommonAssessmentCount = JsonInt(phase305.RootElement, "stableRawCommonAssessmentCount"),
            p305P302ScaledStableRawCommonAssessmentCount = JsonInt(phase305.RootElement, "p302ScaledStableRawCommonAssessmentCount"),
        },
        phase306 = new
        {
            p306Passed,
            p306CanFillContract,
            p306DecoupledRawCommonPassingAssessmentCount = JsonInt(phase306.RootElement, "decoupledRawCommonPassingAssessmentCount"),
            p306DecoupledP302ScaledCommonPassingAssessmentCount = JsonInt(phase306.RootElement, "decoupledP302ScaledCommonPassingAssessmentCount"),
        },
    },
    checks,
    decision = targetIndependentDecoupledWzRowSelectionLawAuditPassed
        ? "Do not promote the target-independent decoupled W/Z row selector. Some predeclared source-side selectors can pick a Phase302-scaled numerical near pass without target construction, but no selector clears the unscaled raw/common gate and no theorem promotes the decoupled row choice or Phase302 scale as a contract-grade W/Z source law."
        : "Review Phase307 target-independent decoupled W/Z row selection audit before relying on this route.",
    nextRequiredArtifact = new[]
    {
        "A theorem-backed W/Z source law deriving particle-specific W and Z transition/source-row selection before target comparison.",
        "A derivation that promotes the Phase302 W/Z scales or replaces them with a source-declared normalization that is not fitted to W/Z targets.",
        "A branch-stable source-row construction that clears raw and common W/Z gates without post-hoc target fitting.",
        "Phase201/P209 W/Z source rows with derivation ids, raw sidecars, common-bridge sidecars, target-comparison sidecars, and stability sidecars filled.",
    },
    sourceEvidence = new
    {
        bosonRegistryPath = BosonRegistryPath,
        variationMatrixDir = VariationMatrixDir,
        phase27FamiliesPath = Phase27FamiliesPath,
        phase91Dir = Phase91Dir,
        phase213Path = Phase213Path,
        phase299Path = Phase299Path,
        phase302Path = Phase302Path,
        phase305Path = Phase305Path,
        phase306Path = Phase306Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "target_independent_decoupled_wz_row_selection_law_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "target_independent_decoupled_wz_row_selection_law_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.targetIndependentDecoupledWzRowSelectionLawAuditPassed,
        result.targetObservablesUsedForConstruction,
        result.targetValuesUsedOnlyForPostCandidateEvaluation,
        result.targetRaw,
        result.rawGateRatio,
        result.stabilitySpreadTolerance,
        result.commonScaleSpreadTolerance,
        result.p302WTotalScale,
        result.p302ZTotalScale,
        result.backgroundIds,
        result.pairCount,
        result.definitionCount,
        result.assessmentCount,
        result.chargedAxis0CandidateIds,
        result.chargedAxis1CandidateIds,
        result.neutralAxisCandidateIds,
        result.canonicalChargedOperator,
        result.canonicalNeutralOperator,
        result.allRowsRawPassingAssessmentCount,
        result.p302ScaledAllRowsRawPassingAssessmentCount,
        result.stableAssessmentCount,
        result.stableRawCommonAssessmentCount,
        result.p302ScaledStableRawCommonAssessmentCount,
        result.stableButRawOrCommonBlockedAssessmentCount,
        result.particleRowCount,
        result.wParticleRowCount,
        result.zParticleRowCount,
        result.wStableRowCount,
        result.zStableRowCount,
        result.wStableRawRowCount,
        result.zStableRawRowCount,
        result.wStableP302ScaledRawRowCount,
        result.zStableP302ScaledRawRowCount,
        result.stableDecoupledAssessmentCount,
        result.decoupledRawAssessmentCount,
        result.decoupledP302ScaledAssessmentCount,
        result.decoupledRawCommonPassingAssessmentCount,
        result.decoupledP302ScaledCommonPassingAssessmentCount,
        result.numericalP302ScaledDecoupledNearPassPresent,
        result.selectionLawCount,
        result.p302ScaleUsingSelectionLawCount,
        result.rawStableCommonSelectionLawCount,
        result.p302ScaledStableCommonSelectionLawCount,
        result.p302ScaledNearPassWithoutRawSelectionLawCount,
        result.selectionLawCanFillPhase201WzContractCount,
        result.targetIndependentSelectionLawAssessments,
        result.bestTargetIndependentSelectionLaw,
        result.bestP302ScaledStableCommonSelectionLaw,
        result.theoremClaimed,
        result.sourceRowsPromotable,
        result.canFillPhase201WzContract,
        result.wzMissingFieldCount,
        result.higgsMissingFieldCount,
        result.bestAssessment,
        result.bestP302ScaledRawAssessment,
        result.closestScaledCommonAssessment,
        result.bestDecoupledP302ScaledCommonAssessment,
        result.inheritedBlockers,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"targetIndependentDecoupledWzRowSelectionLawAuditPassed={targetIndependentDecoupledWzRowSelectionLawAuditPassed}");
Console.WriteLine($"chargedAxis0CandidateCount={chargedAxis0CandidateIds.Length}");
Console.WriteLine($"chargedAxis1CandidateCount={chargedAxis1CandidateIds.Length}");
Console.WriteLine($"neutralAxisCandidateCount={neutralAxisCandidateIds.Length}");
Console.WriteLine($"pairCount={pairKeys.Length}");
Console.WriteLine($"definitionCount={sourceDefinitions.Length}");
Console.WriteLine($"assessmentCount={assessments.Length}");
Console.WriteLine($"allRowsRawPassingAssessmentCount={allRowsRawPassingAssessments.Length}");
Console.WriteLine($"p302ScaledAllRowsRawPassingAssessmentCount={p302ScaledAllRowsRawPassingAssessments.Length}");
Console.WriteLine($"stableAssessmentCount={stableAssessments.Length}");
Console.WriteLine($"stableRawCommonAssessmentCount={stableRawCommonAssessments.Length}");
Console.WriteLine($"p302ScaledStableRawCommonAssessmentCount={stableP302ScaledAssessments.Length}");
Console.WriteLine($"wStableP302ScaledRawRowCount={wStableP302ScaledRawRows.Length}");
Console.WriteLine($"zStableP302ScaledRawRowCount={zStableP302ScaledRawRows.Length}");
Console.WriteLine($"decoupledP302ScaledCommonPassingAssessmentCount={decoupledP302ScaledCommonPassingAssessments.Length}");
Console.WriteLine($"selectionLawCount={targetIndependentSelectionLawAssessments.Length}");
Console.WriteLine($"rawStableCommonSelectionLawCount={selectionLawsSelectingRawStableCommon.Length}");
Console.WriteLine($"p302ScaledStableCommonSelectionLawCount={selectionLawsSelectingP302ScaledStableCommon.Length}");
Console.WriteLine($"bestTargetIndependentSelectionLaw={bestTargetIndependentSelectionLaw?.LawId}");
Console.WriteLine($"bestTargetIndependentSelectionAssessment={bestTargetIndependentSelectionLaw?.SelectedAssessment.AssessmentId}");
Console.WriteLine($"bestP302ScaledStableCommonSelectionLaw={bestP302ScaledStableCommonSelectionLaw?.LawId}");
Console.WriteLine($"bestP302ScaledStableCommonSelectionAssessment={bestP302ScaledStableCommonSelectionLaw?.SelectedAssessment.AssessmentId}");
Console.WriteLine($"bestAssessment={bestAssessment?.AssessmentId}");
Console.WriteLine($"bestMaxParticleRelativeSpread={bestAssessment?.MaxParticleRelativeSpread:R}");
Console.WriteLine($"bestP302ScaledRawAssessment={bestP302ScaledRawAssessment?.AssessmentId}");
Console.WriteLine($"bestP302ScaledRawAssessmentMaxSpread={bestP302ScaledRawAssessment?.MaxParticleRelativeSpread:R}");
Console.WriteLine($"bestDecoupledP302ScaledCommonAssessment={bestDecoupledP302ScaledCommonAssessment?.AssessmentId}");
Console.WriteLine($"bestDecoupledP302ScaledCommonSpread={bestDecoupledP302ScaledCommonAssessment?.P302ScaledCommonMeanRelativeSpread:R}");
Console.WriteLine($"bestDecoupledMinP302ScaledRawToTargetRatio={bestDecoupledP302ScaledCommonAssessment?.MinP302ScaledRowRawToTargetRatio:R}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");

ParticleRowCandidate BuildParticleRowCandidate(SourceAssessment assessment, string particleId)
{
    var summary = assessment.ParticleSummaries.Single(item => item.ParticleId == particleId);
    var rows = assessment.Rows.Where(row => row.ParticleId == particleId).ToArray();
    return new ParticleRowCandidate(
        $"{particleId}:{assessment.AssessmentId}",
        particleId,
        assessment.AssessmentId,
        assessment.DefinitionId,
        assessment.Pair,
        rows.Length,
        rows.Min(row => row.AggregateNorm),
        rows.Min(row => row.AggregateNorm * row.P302Scale),
        rows.Average(row => row.AggregateNorm),
        rows.Average(row => row.AggregateNorm * row.P302Scale),
        rows.Min(row => row.RawToTargetRatio),
        rows.Min(row => row.P302ScaledRawToTargetRatio),
        summary.MeanRawToTargetRatio,
        summary.MeanP302ScaledRawToTargetRatio,
        summary.RelativeSpread,
        summary.StabilityPassed,
        summary.AllRowsRawGatePassed,
        summary.P302ScaledAllRowsRawGatePassed);
}

DecoupledWzRowAssessment BuildDecoupledAssessment(ParticleRowCandidate wRow, ParticleRowCandidate zRow)
{
    double requiredScaleRelativeSpread = RelativeSpread(new[]
    {
        wRow.MeanRawToTargetRatio > 0.0 ? 1.0 / wRow.MeanRawToTargetRatio : double.PositiveInfinity,
        zRow.MeanRawToTargetRatio > 0.0 ? 1.0 / zRow.MeanRawToTargetRatio : double.PositiveInfinity,
    });
    double p302ScaledCommonMeanRelativeSpread = RelativeSpread(new[]
    {
        wRow.MeanP302ScaledRawToTargetRatio,
        zRow.MeanP302ScaledRawToTargetRatio,
    });
    bool bothStable = wRow.StabilityPassed && zRow.StabilityPassed;
    bool bothRaw = wRow.RawGatePassed && zRow.RawGatePassed;
    bool bothP302ScaledRaw = wRow.P302ScaledRawGatePassed && zRow.P302ScaledRawGatePassed;
    bool rawCommon = bothStable && bothRaw && requiredScaleRelativeSpread <= commonScaleSpreadTolerance;
    bool p302ScaledCommon = bothStable && bothP302ScaledRaw && p302ScaledCommonMeanRelativeSpread <= commonScaleSpreadTolerance;
    return new DecoupledWzRowAssessment(
        $"decoupled:{wRow.RowId}|{zRow.RowId}",
        wRow,
        zRow,
        Math.Min(wRow.MinRowRawToTargetRatio, zRow.MinRowRawToTargetRatio),
        Math.Min(wRow.MinP302ScaledRowRawToTargetRatio, zRow.MinP302ScaledRowRawToTargetRatio),
        Math.Max(wRow.RelativeSpread, zRow.RelativeSpread),
        requiredScaleRelativeSpread,
        p302ScaledCommonMeanRelativeSpread,
        bothStable,
        bothRaw,
        bothP302ScaledRaw,
        rawCommon,
        p302ScaledCommon);
}

IReadOnlyList<RowSelectionLawAssessment> BuildTargetIndependentSelectionLawAssessments(IReadOnlyList<DecoupledWzRowAssessment> stablePairs)
{
    var laws = new List<RowSelectionLawAssessment>();
    if (stablePairs.Count == 0)
    {
        return laws;
    }

    var sameTransitionPairs = stablePairs
        .Where(assessment => assessment.WRow.Pair == assessment.ZRow.Pair)
        .ToArray();

    Add(
        "source-raw-common-spread-min",
        "Select the stable decoupled W/Z rows with the closest unscaled source magnitudes.",
        usesP302ScaleForSelection: false,
        "all-stable-decoupled-pairs",
        stablePairs
            .OrderBy(RawSourceCommonRelativeSpread)
            .ThenBy(assessment => assessment.MaxParticleRelativeSpread)
            .ThenByDescending(MinMeanAggregateNorm)
            .ThenBy(assessment => assessment.AssessmentId, StringComparer.Ordinal)
            .First());
    Add(
        "source-stability-then-raw-common-spread",
        "Select the stable decoupled W/Z rows with the smallest branch spread, then closest unscaled source magnitudes.",
        usesP302ScaleForSelection: false,
        "all-stable-decoupled-pairs",
        stablePairs
            .OrderBy(assessment => assessment.MaxParticleRelativeSpread)
            .ThenBy(RawSourceCommonRelativeSpread)
            .ThenByDescending(MinMeanAggregateNorm)
            .ThenBy(assessment => assessment.AssessmentId, StringComparer.Ordinal)
            .First());
    Add(
        "source-max-min-raw-magnitude",
        "Select the stable decoupled W/Z rows that maximize the weaker unscaled source magnitude.",
        usesP302ScaleForSelection: false,
        "all-stable-decoupled-pairs",
        stablePairs
            .OrderByDescending(MinMeanAggregateNorm)
            .ThenBy(RawSourceCommonRelativeSpread)
            .ThenBy(assessment => assessment.MaxParticleRelativeSpread)
            .ThenBy(assessment => assessment.AssessmentId, StringComparer.Ordinal)
            .First());
    Add(
        "p302-scaled-common-spread-min",
        "Select the stable decoupled W/Z rows with the closest Phase302-scaled source magnitudes.",
        usesP302ScaleForSelection: true,
        "all-stable-decoupled-pairs",
        stablePairs
            .OrderBy(P302ScaledSourceCommonRelativeSpread)
            .ThenByDescending(MinMeanP302ScaledAggregateNorm)
            .ThenBy(assessment => assessment.MaxParticleRelativeSpread)
            .ThenBy(assessment => assessment.AssessmentId, StringComparer.Ordinal)
            .First());
    Add(
        "p302-scaled-stability-then-common-spread",
        "Select the stable decoupled W/Z rows with the smallest branch spread, then closest Phase302-scaled source magnitudes.",
        usesP302ScaleForSelection: true,
        "all-stable-decoupled-pairs",
        stablePairs
            .OrderBy(assessment => assessment.MaxParticleRelativeSpread)
            .ThenBy(P302ScaledSourceCommonRelativeSpread)
            .ThenByDescending(MinMeanP302ScaledAggregateNorm)
            .ThenBy(assessment => assessment.AssessmentId, StringComparer.Ordinal)
            .First());
    Add(
        "p302-scaled-max-min-magnitude",
        "Select the stable decoupled W/Z rows that maximize the weaker Phase302-scaled source magnitude.",
        usesP302ScaleForSelection: true,
        "all-stable-decoupled-pairs",
        stablePairs
            .OrderByDescending(MinMeanP302ScaledAggregateNorm)
            .ThenBy(P302ScaledSourceCommonRelativeSpread)
            .ThenBy(assessment => assessment.MaxParticleRelativeSpread)
            .ThenBy(assessment => assessment.AssessmentId, StringComparer.Ordinal)
            .First());

    if (sameTransitionPairs.Length > 0)
    {
        Add(
            "same-transition-source-raw-common-spread-min",
            "Select same-transition stable W/Z rows with the closest unscaled source magnitudes.",
            usesP302ScaleForSelection: false,
            "same-transition-stable-decoupled-pairs",
            sameTransitionPairs
                .OrderBy(RawSourceCommonRelativeSpread)
                .ThenBy(assessment => assessment.MaxParticleRelativeSpread)
                .ThenByDescending(MinMeanAggregateNorm)
                .ThenBy(assessment => assessment.AssessmentId, StringComparer.Ordinal)
                .First());
        Add(
            "same-transition-p302-scaled-common-spread-min",
            "Select same-transition stable W/Z rows with the closest Phase302-scaled source magnitudes.",
            usesP302ScaleForSelection: true,
            "same-transition-stable-decoupled-pairs",
            sameTransitionPairs
                .OrderBy(P302ScaledSourceCommonRelativeSpread)
                .ThenByDescending(MinMeanP302ScaledAggregateNorm)
                .ThenBy(assessment => assessment.MaxParticleRelativeSpread)
                .ThenBy(assessment => assessment.AssessmentId, StringComparer.Ordinal)
                .First());
    }

    return laws;

    void Add(
        string lawId,
        string description,
        bool usesP302ScaleForSelection,
        string selectionPopulation,
        DecoupledWzRowAssessment selected)
    {
        laws.Add(new RowSelectionLawAssessment(
            lawId,
            description,
            usesP302ScaleForSelection,
            UsesTargetsForSelection: false,
            selectionPopulation,
            RawSourceCommonRelativeSpread(selected),
            P302ScaledSourceCommonRelativeSpread(selected),
            selected,
            selected.RawStableCommonPassed,
            selected.P302ScaledStableCommonPassed,
            CanFillPhase201WzContract: false));
    }
}

double RawSourceCommonRelativeSpread(DecoupledWzRowAssessment assessment) =>
    RelativeSpread(new[]
    {
        assessment.WRow.MeanAggregateNorm,
        assessment.ZRow.MeanAggregateNorm,
    });

double P302ScaledSourceCommonRelativeSpread(DecoupledWzRowAssessment assessment) =>
    RelativeSpread(new[]
    {
        assessment.WRow.MeanP302ScaledAggregateNorm,
        assessment.ZRow.MeanP302ScaledAggregateNorm,
    });

double MinMeanAggregateNorm(DecoupledWzRowAssessment assessment) =>
    Math.Min(assessment.WRow.MeanAggregateNorm, assessment.ZRow.MeanAggregateNorm);

double MinMeanP302ScaledAggregateNorm(DecoupledWzRowAssessment assessment) =>
    Math.Min(assessment.WRow.MeanP302ScaledAggregateNorm, assessment.ZRow.MeanP302ScaledAggregateNorm);

SourceAssessment AssessDefinition(SourceDefinition definition, PairKey pair)
{
    var rows = backgrounds
        .SelectMany(backgroundId => new[]
        {
            BuildSourceRow("w-boson", definition, backgroundId, pair),
            BuildSourceRow("z-boson", definition, backgroundId, pair),
        })
        .ToArray();
    var particleSummaries = rows
        .GroupBy(row => row.ParticleId, StringComparer.Ordinal)
        .OrderBy(group => group.Key, StringComparer.Ordinal)
        .Select(group =>
        {
            var groupRows = group.ToArray();
            var rawRatios = groupRows.Select(row => row.RawToTargetRatio).ToArray();
            var p302ScaledRatios = groupRows.Select(row => row.P302ScaledRawToTargetRatio).ToArray();
            double meanRawRatio = rawRatios.Average();
            double meanP302Scaled = p302ScaledRatios.Average();
            double relativeSpread = RelativeSpread(rawRatios);
            return new ParticleSourceSummary(
                group.Key,
                groupRows.Length,
                meanRawRatio,
                meanP302Scaled,
                relativeSpread,
                relativeSpread <= stabilitySpreadTolerance,
                rawRatios.All(ratio => ratio >= rawGateRatio),
                p302ScaledRatios.All(ratio => ratio >= rawGateRatio));
        })
        .ToArray();
    var requiredScales = particleSummaries.Select(summary => summary.MeanRawToTargetRatio > 0.0 ? 1.0 / summary.MeanRawToTargetRatio : double.PositiveInfinity).ToArray();
    double requiredScaleSpread = RelativeSpread(requiredScales);
    double commonMeanSpread = RelativeSpread(particleSummaries.Select(summary => summary.MeanP302ScaledRawToTargetRatio));
    bool commonRawPassed = requiredScaleSpread <= commonScaleSpreadTolerance;
    bool p302CommonPassed = commonMeanSpread <= commonScaleSpreadTolerance;
    bool allRowsRawGatePassed = rows.All(row => row.RawToTargetRatio >= rawGateRatio);
    bool p302ScaledAllRowsRawGatePassed = rows.All(row => row.P302ScaledRawToTargetRatio >= rawGateRatio);
    bool allParticlesStabilityPassed = particleSummaries.All(summary => summary.StabilityPassed);
    bool stableRawCommonPassed = allRowsRawGatePassed && commonRawPassed && allParticlesStabilityPassed;
    bool p302ScaledStableRawCommonPassed = p302ScaledAllRowsRawGatePassed && p302CommonPassed && allParticlesStabilityPassed;
    return new SourceAssessment(
        $"{definition.DefinitionId}:{pair.FromModeIndex}->{pair.ToModeIndex}",
        definition.DefinitionId,
        pair,
        rows,
        particleSummaries,
        rows.Min(row => row.RawToTargetRatio),
        rows.Min(row => row.P302ScaledRawToTargetRatio),
        particleSummaries.Max(summary => summary.RelativeSpread),
        requiredScaleSpread,
        commonMeanSpread,
        allRowsRawGatePassed,
        p302ScaledAllRowsRawGatePassed,
        allParticlesStabilityPassed,
        commonRawPassed,
        p302CommonPassed,
        stableRawCommonPassed,
        p302ScaledStableRawCommonPassed);
}

SourceRow BuildSourceRow(string particleId, SourceDefinition definition, string backgroundId, PairKey pair)
{
    var modes = backgroundModes[backgroundId];
    var from = modes.Single(mode => mode.ModeIndex == pair.FromModeIndex);
    var to = modes.Single(mode => mode.ModeIndex == pair.ToModeIndex);
    var components = definition.CandidateIds.Select(candidateId =>
    {
        var value = CandidateElement(candidateId, backgroundId, from, to);
        return new CandidateComponent(candidateId, value.Real, value.Imaginary, value.Magnitude);
    }).ToArray();

    ComplexValue operatorValue = particleId == "w-boson"
        ? ChargedOperatorValue(definition, backgroundId, from, to)
        : NeutralOperatorValue(definition, backgroundId, from, to);
    double aggregateNorm = operatorValue.Magnitude;
    double rawToTargetRatio = aggregateNorm / targetRaw;
    double p302Scale = particleId == "w-boson" ? p302WTotalScale : p302ZTotalScale;
    return new SourceRow(
        particleId,
        backgroundId,
        from.ModeId,
        to.ModeId,
        definition.DefinitionId,
        particleId == "w-boson" ? definition.WCandidateIds : definition.ZCandidateIds,
        aggregateNorm,
        operatorValue.Real,
        operatorValue.Imaginary,
        rawToTargetRatio,
        p302Scale,
        rawToTargetRatio * p302Scale,
        components.OrderByDescending(component => component.Magnitude).Take(8).ToArray());
}

ComplexValue ChargedOperatorValue(SourceDefinition definition, string backgroundId, ModeRecord from, ModeRecord to) =>
    definition.Kind switch
    {
        SourceDefinitionKind.ChargedCoherentNeutralCoherent or SourceDefinitionKind.ChargedCoherentNeutralRss => LadderCombination(
            SumCandidateElements(chargedAxis0CandidateIds, backgroundId, from, to),
            SumCandidateElements(chargedAxis1CandidateIds, backgroundId, from, to),
            definition.ChargedSign),
        SourceDefinitionKind.AxisRssNeutralRss => ComplexValue.RealOnly(Math.Sqrt(
            Math.Pow(SumCandidateElements(chargedAxis0CandidateIds, backgroundId, from, to).Magnitude, 2.0)
            + Math.Pow(SumCandidateElements(chargedAxis1CandidateIds, backgroundId, from, to).Magnitude, 2.0)) / Math.Sqrt(2.0)),
        SourceDefinitionKind.PairLadderSingletonNeutral => LadderCombination(
            CandidateElement(definition.Axis0CandidateId ?? throw new InvalidDataException("Missing axis0 candidate."), backgroundId, from, to),
            CandidateElement(definition.Axis1CandidateId ?? throw new InvalidDataException("Missing axis1 candidate."), backgroundId, from, to),
            definition.ChargedSign),
        _ => throw new InvalidDataException($"Unsupported source definition kind {definition.Kind}."),
    };

ComplexValue NeutralOperatorValue(SourceDefinition definition, string backgroundId, ModeRecord from, ModeRecord to) =>
    definition.Kind switch
    {
        SourceDefinitionKind.ChargedCoherentNeutralCoherent => SumCandidateElements(neutralAxisCandidateIds, backgroundId, from, to),
        SourceDefinitionKind.ChargedCoherentNeutralRss or SourceDefinitionKind.AxisRssNeutralRss => RssCandidateElements(neutralAxisCandidateIds, backgroundId, from, to),
        SourceDefinitionKind.PairLadderSingletonNeutral => CandidateElement(definition.NeutralCandidateId ?? throw new InvalidDataException("Missing neutral candidate."), backgroundId, from, to),
        _ => throw new InvalidDataException($"Unsupported source definition kind {definition.Kind}."),
    };

ComplexValue LadderCombination(ComplexValue axis0, ComplexValue axis1, int sign) =>
    (axis0 + axis1.MultiplyByI(sign)).Scale(1.0 / Math.Sqrt(2.0));

ComplexValue SumCandidateElements(IReadOnlyList<string> candidateIds, string backgroundId, ModeRecord from, ModeRecord to)
{
    var sum = ComplexValue.Zero;
    foreach (var candidateId in candidateIds)
        sum += CandidateElement(candidateId, backgroundId, from, to);
    return sum;
}

ComplexValue RssCandidateElements(IReadOnlyList<string> candidateIds, string backgroundId, ModeRecord from, ModeRecord to) =>
    ComplexValue.RealOnly(Math.Sqrt(candidateIds.Sum(candidateId => Math.Pow(CandidateElement(candidateId, backgroundId, from, to).Magnitude, 2.0))));

ComplexValue CandidateElement(string candidateId, string backgroundId, ModeRecord from, ModeRecord to)
{
    var matrix = matrices[(backgroundId, candidateId)];
    var value = MatrixElement(matrix.Real, matrix.Imaginary, from.Coefficients, to.Coefficients);
    return new ComplexValue(value.Real, value.Imaginary);
}

SourceDefinition[] BuildSourceDefinitions()
{
    var definitions = new List<SourceDefinition>
    {
        SourceDefinition.AllAxis(
            "charged-ladder-all-axis-neutral-coherent-plus",
            SourceDefinitionKind.ChargedCoherentNeutralCoherent,
            chargedAxis0CandidateIds,
            chargedAxis1CandidateIds,
            neutralAxisCandidateIds,
            +1,
            "Coherent charged-current ladder T+=(axis0+i axis1)/sqrt(2) from all Phase27 charged axes, with coherent neutral-axis sum."),
        SourceDefinition.AllAxis(
            "charged-ladder-all-axis-neutral-coherent-minus",
            SourceDefinitionKind.ChargedCoherentNeutralCoherent,
            chargedAxis0CandidateIds,
            chargedAxis1CandidateIds,
            neutralAxisCandidateIds,
            -1,
            "Coherent charged-current ladder T-=(axis0-i axis1)/sqrt(2) from all Phase27 charged axes, with coherent neutral-axis sum."),
        SourceDefinition.AllAxis(
            "charged-ladder-all-axis-neutral-rss-plus",
            SourceDefinitionKind.ChargedCoherentNeutralRss,
            chargedAxis0CandidateIds,
            chargedAxis1CandidateIds,
            neutralAxisCandidateIds,
            +1,
            "Coherent charged-current ladder T+=(axis0+i axis1)/sqrt(2) from all Phase27 charged axes, with neutral-axis root-sum-square."),
        SourceDefinition.AllAxis(
            "charged-ladder-all-axis-neutral-rss-minus",
            SourceDefinitionKind.ChargedCoherentNeutralRss,
            chargedAxis0CandidateIds,
            chargedAxis1CandidateIds,
            neutralAxisCandidateIds,
            -1,
            "Coherent charged-current ladder T-=(axis0-i axis1)/sqrt(2) from all Phase27 charged axes, with neutral-axis root-sum-square."),
        SourceDefinition.AllAxis(
            "charged-axis-rss-neutral-rss",
            SourceDefinitionKind.AxisRssNeutralRss,
            chargedAxis0CandidateIds,
            chargedAxis1CandidateIds,
            neutralAxisCandidateIds,
            0,
            "Root-sum-square over coherent axis-0 and axis-1 charged sums divided by sqrt(2), with neutral-axis root-sum-square."),
    };

    foreach (var axis0CandidateId in chargedAxis0CandidateIds)
    {
        foreach (var axis1CandidateId in chargedAxis1CandidateIds)
        {
            foreach (var neutralCandidateId in neutralAxisCandidateIds)
            {
                definitions.Add(SourceDefinition.Pair(
                    $"charged-ladder-pair-{axis0CandidateId}-plus-{axis1CandidateId}-z-{neutralCandidateId}",
                    axis0CandidateId,
                    axis1CandidateId,
                    neutralCandidateId,
                    +1,
                    "Single axis-0/axis-1 charged-current ladder T+ candidate pair with one neutral-axis Z candidate."));
                definitions.Add(SourceDefinition.Pair(
                    $"charged-ladder-pair-{axis0CandidateId}-minus-{axis1CandidateId}-z-{neutralCandidateId}",
                    axis0CandidateId,
                    axis1CandidateId,
                    neutralCandidateId,
                    -1,
                    "Single axis-0/axis-1 charged-current ladder T- candidate pair with one neutral-axis Z candidate."));
            }
        }
    }

    return definitions
        .OrderBy(definition => definition.Kind)
        .ThenBy(definition => definition.DefinitionId, StringComparer.Ordinal)
        .ToArray();
}

Dictionary<string, CandidateMetadata> LoadCandidateMetadata()
{
    var metadata = phase27Families.RootElement.GetProperty("families")
        .EnumerateArray()
        .Select(family =>
        {
            string candidateId = RequiredString(family, "sourceCandidateId").Replace("phase12-", "", StringComparison.Ordinal);
            var features = family.GetProperty("identityFeatures");
            return new CandidateMetadata(
                candidateId,
                RequiredString(features, "chargeSector"),
                RequiredString(features, "algebraBasisSector"),
                JsonInt(features, "dominantBasisIndex") ?? -1);
        })
        .ToDictionary(candidate => candidate.CandidateId, StringComparer.Ordinal);

    foreach (var candidate in candidates)
    {
        if (!metadata.ContainsKey(candidate.CandidateId))
            throw new InvalidDataException($"Phase27 metadata missing for {candidate.CandidateId}.");
    }

    return metadata;
}

static IReadOnlyDictionary<(string BackgroundId, string CandidateId), MatrixRecord> LoadMatrices(
    IReadOnlyList<string> backgrounds,
    IReadOnlyList<CandidateRecord> candidates)
{
    var matrices = new Dictionary<(string BackgroundId, string CandidateId), MatrixRecord>();
    foreach (var backgroundId in backgrounds)
    {
        foreach (var candidate in candidates)
        {
            var path = Path.Combine(VariationMatrixDir, $"variation-{backgroundId}-{candidate.CandidateId}.matrix.json");
            using var doc = JsonDocument.Parse(File.ReadAllText(path));
            matrices[(backgroundId, candidate.CandidateId)] = new MatrixRecord(
                backgroundId,
                candidate.CandidateId,
                LoadMatrix(doc.RootElement.GetProperty("real")),
                LoadMatrix(doc.RootElement.GetProperty("imag")));
        }
    }

    return matrices;
}

static IReadOnlyList<ModeRecord> LoadModes(string path)
{
    using var doc = JsonDocument.Parse(File.ReadAllText(path));
    return doc.RootElement.GetProperty("modes")
        .EnumerateArray()
        .Select(mode => new ModeRecord(
            RequiredString(mode, "modeId"),
            JsonInt(mode, "modeIndex") ?? throw new InvalidDataException("Missing modeIndex."),
            mode.GetProperty("eigenvectorCoefficients").EnumerateArray().Select(value => value.GetDouble()).ToArray()))
        .OrderBy(mode => mode.ModeIndex)
        .ToArray();
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
            matrix[row, col++] = value.GetDouble();
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

static double[] Normalize(double[] vector)
{
    double norm = Math.Sqrt(vector.Sum(value => value * value));
    return norm < 1e-30 ? vector : vector.Select(value => value / norm).ToArray();
}

static double RelativeSpread(IEnumerable<double> values)
{
    var array = values.Where(double.IsFinite).ToArray();
    if (array.Length == 0)
        return double.NaN;
    double mean = array.Average();
    return (array.Max() - array.Min()) / Math.Max(Math.Abs(mean), 1e-300);
}

static int CandidateOrdinal(string candidateId)
{
    const string prefix = "candidate-";
    return candidateId.StartsWith(prefix, StringComparison.Ordinal) && int.TryParse(candidateId[prefix.Length..], out int value)
        ? value
        : int.MaxValue;
}

static string RequiredString(JsonElement element, string propertyName) =>
    JsonString(element, propertyName) ?? throw new InvalidDataException($"Missing string property '{propertyName}'.");

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

static double RequiredDouble(JsonElement element, string propertyName) =>
    JsonDouble(element, propertyName) ?? throw new InvalidDataException($"Missing numeric property '{propertyName}'.");

static double? JsonDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var value) ? value : null;

readonly record struct ComplexValue(double Real, double Imaginary)
{
    public static ComplexValue Zero { get; } = new(0.0, 0.0);
    public double Magnitude => Math.Sqrt(Real * Real + Imaginary * Imaginary);
    public ComplexValue Scale(double scalar) => new(Real * scalar, Imaginary * scalar);
    public ComplexValue MultiplyByI(int sign) => new(-sign * Imaginary, sign * Real);
    public static ComplexValue RealOnly(double value) => new(value, 0.0);
    public static ComplexValue operator +(ComplexValue left, ComplexValue right) => new(left.Real + right.Real, left.Imaginary + right.Imaginary);
}

enum SourceDefinitionKind
{
    ChargedCoherentNeutralCoherent,
    ChargedCoherentNeutralRss,
    AxisRssNeutralRss,
    PairLadderSingletonNeutral,
}

sealed record SourceDefinition(
    string DefinitionId,
    SourceDefinitionKind Kind,
    IReadOnlyList<string> WCandidateIds,
    IReadOnlyList<string> ZCandidateIds,
    IReadOnlyList<string> CandidateIds,
    int ChargedSign,
    string? Axis0CandidateId,
    string? Axis1CandidateId,
    string? NeutralCandidateId,
    string Description)
{
    public static SourceDefinition AllAxis(
        string definitionId,
        SourceDefinitionKind kind,
        IReadOnlyList<string> axis0CandidateIds,
        IReadOnlyList<string> axis1CandidateIds,
        IReadOnlyList<string> neutralCandidateIds,
        int chargedSign,
        string description)
    {
        var wCandidateIds = axis0CandidateIds.Concat(axis1CandidateIds).ToArray();
        var zCandidateIds = neutralCandidateIds.ToArray();
        return new SourceDefinition(
            definitionId,
            kind,
            wCandidateIds,
            zCandidateIds,
            wCandidateIds.Concat(zCandidateIds).Distinct(StringComparer.Ordinal).ToArray(),
            chargedSign,
            null,
            null,
            null,
            description);
    }

    public static SourceDefinition Pair(
        string definitionId,
        string axis0CandidateId,
        string axis1CandidateId,
        string neutralCandidateId,
        int chargedSign,
        string description)
    {
        var wCandidateIds = new[] { axis0CandidateId, axis1CandidateId };
        var zCandidateIds = new[] { neutralCandidateId };
        return new SourceDefinition(
            definitionId,
            SourceDefinitionKind.PairLadderSingletonNeutral,
            wCandidateIds,
            zCandidateIds,
            wCandidateIds.Concat(zCandidateIds).ToArray(),
            chargedSign,
            axis0CandidateId,
            axis1CandidateId,
            neutralCandidateId,
            description);
    }
}

sealed record CandidateRecord(string CandidateId, IReadOnlyList<string> ContributingModeIds);
sealed record CandidateMetadata(string CandidateId, string ChargeSector, string AlgebraBasisSector, int DominantBasisIndex);
sealed record ModeRecord(string ModeId, int ModeIndex, double[] Coefficients);
sealed record MatrixRecord(string BackgroundId, string CandidateId, double[,] Real, double[,] Imaginary);
sealed record PairKey(int FromModeIndex, int ToModeIndex);
sealed record CandidateComponent(string CandidateId, double Real, double Imaginary, double Magnitude);
sealed record SourceRow(
    string ParticleId,
    string BackgroundId,
    string FromFermionModeId,
    string ToFermionModeId,
    string DefinitionId,
    IReadOnlyList<string> CandidateIds,
    double AggregateNorm,
    double OperatorReal,
    double OperatorImaginary,
    double RawToTargetRatio,
    double P302Scale,
    double P302ScaledRawToTargetRatio,
    IReadOnlyList<CandidateComponent> Components);
sealed record ParticleSourceSummary(
    string ParticleId,
    int RowCount,
    double MeanRawToTargetRatio,
    double MeanP302ScaledRawToTargetRatio,
    double RelativeSpread,
    bool StabilityPassed,
    bool AllRowsRawGatePassed,
    bool P302ScaledAllRowsRawGatePassed);
sealed record ParticleRowCandidate(
    string RowId,
    string ParticleId,
    string SourceAssessmentId,
    string DefinitionId,
    PairKey Pair,
    int RowCount,
    double MinRowAggregateNorm,
    double MinP302ScaledAggregateNorm,
    double MeanAggregateNorm,
    double MeanP302ScaledAggregateNorm,
    double MinRowRawToTargetRatio,
    double MinP302ScaledRowRawToTargetRatio,
    double MeanRawToTargetRatio,
    double MeanP302ScaledRawToTargetRatio,
    double RelativeSpread,
    bool StabilityPassed,
    bool RawGatePassed,
    bool P302ScaledRawGatePassed);
sealed record DecoupledWzRowAssessment(
    string AssessmentId,
    ParticleRowCandidate WRow,
    ParticleRowCandidate ZRow,
    double MinRowRawToTargetRatio,
    double MinP302ScaledRowRawToTargetRatio,
    double MaxParticleRelativeSpread,
    double RequiredScaleRelativeSpread,
    double P302ScaledCommonMeanRelativeSpread,
    bool BothParticlesStabilityPassed,
    bool BothParticlesRawGatePassed,
    bool BothParticlesP302ScaledRawGatePassed,
    bool RawStableCommonPassed,
    bool P302ScaledStableCommonPassed);
sealed record RowSelectionLawAssessment(
    string LawId,
    string Description,
    bool UsesP302ScaleForSelection,
    bool UsesTargetsForSelection,
    string SelectionPopulation,
    double SelectedRawSourceCommonRelativeSpread,
    double SelectedP302ScaledSourceCommonRelativeSpread,
    DecoupledWzRowAssessment SelectedAssessment,
    bool SelectedRawStableCommonPassed,
    bool SelectedP302ScaledStableCommonPassed,
    bool CanFillPhase201WzContract);
sealed record SourceAssessment(
    string AssessmentId,
    string DefinitionId,
    PairKey Pair,
    IReadOnlyList<SourceRow> Rows,
    IReadOnlyList<ParticleSourceSummary> ParticleSummaries,
    double MinRowRawToTargetRatio,
    double MinP302ScaledRowRawToTargetRatio,
    double MaxParticleRelativeSpread,
    double RequiredScaleRelativeSpread,
    double P302ScaledCommonMeanRelativeSpread,
    bool AllRowsRawGatePassed,
    bool P302ScaledAllRowsRawGatePassed,
    bool AllParticlesStabilityPassed,
    bool CommonRawGatePassed,
    bool P302ScaledCommonGatePassed,
    bool StableRawCommonPassed,
    bool P302ScaledStableRawCommonPassed);
sealed record Check(string Id, bool Passed, string Detail);
