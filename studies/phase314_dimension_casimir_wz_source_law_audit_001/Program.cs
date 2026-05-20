using System.Text.Json;

const string DefaultOutputDir = "studies/phase314_dimension_casimir_wz_source_law_audit_001/output";
const string Phase63Path = "studies/phase63_su2_generator_normalization_001/su2_generator_normalization.json";
const string Phase64Path = "studies/phase64_non_proxy_fermion_current_matrix_element_001/non_proxy_fermion_current_matrix_element.json";
const string Phase82Path = "studies/phase82_boson_perturbation_vector_materializer_001/boson_perturbation_vector_materializer.json";
const string Phase84Path = "studies/phase84_first_boson_prediction_attempt_001/output/first_boson_prediction_attempt.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase225Path = "studies/phase225_su2_normalization_representation_compatibility_audit_001/output/su2_normalization_representation_compatibility_audit_summary.json";
const string Phase249Path = "studies/phase249_invariant_origin_search_for_near_miss_constants_001/output/invariant_origin_search_for_near_miss_constants_summary.json";
const string Phase302Path = "studies/phase302_identity_split_particle_normalization_audit_001/output/identity_split_particle_normalization_audit_summary.json";
const string Phase307Path = "studies/phase307_target_independent_decoupled_wz_row_selection_law_audit_001/output/target_independent_decoupled_wz_row_selection_law_audit_summary.json";
const string Phase308Path = "studies/phase308_phase302_scale_transfer_to_decoupled_charged_ladder_audit_001/output/phase302_scale_transfer_to_decoupled_charged_ladder_audit_summary.json";
const string Phase309Path = "studies/phase309_source_mode_vector_length_measure_normalization_audit_001/output/source_mode_vector_length_measure_normalization_audit_summary.json";
const string Phase310Path = "studies/phase310_completion_variational_branch_to_wz_normalization_audit_001/output/completion_variational_branch_to_wz_normalization_audit_summary.json";
const string Phase313Path = "studies/phase313_official_draft_electroweak_projection_map_audit_001/output/official_draft_electroweak_projection_map_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE314_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase63 = JsonDocument.Parse(File.ReadAllText(Phase63Path));
using var phase64 = JsonDocument.Parse(File.ReadAllText(Phase64Path));
using var phase82 = JsonDocument.Parse(File.ReadAllText(Phase82Path));
using var phase84 = JsonDocument.Parse(File.ReadAllText(Phase84Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase225 = JsonDocument.Parse(File.ReadAllText(Phase225Path));
using var phase249 = JsonDocument.Parse(File.ReadAllText(Phase249Path));
using var phase302 = JsonDocument.Parse(File.ReadAllText(Phase302Path));
using var phase307 = JsonDocument.Parse(File.ReadAllText(Phase307Path));
using var phase308 = JsonDocument.Parse(File.ReadAllText(Phase308Path));
using var phase309 = JsonDocument.Parse(File.ReadAllText(Phase309Path));
using var phase310 = JsonDocument.Parse(File.ReadAllText(Phase310Path));
using var phase313 = JsonDocument.Parse(File.ReadAllText(Phase313Path));

var phase63TraceHalfConventionDerived = JsonString(phase63.RootElement, "terminalStatus") == "su2-generator-normalization-derived"
    && JsonString(phase63.RootElement, "normalizationConventionId") == "physical-weak-coupling-normalization:su2-canonical-trace-half-v1"
    && JsonDouble(phase63.RootElement, "physicalTraceMetricDiagonal") == 0.5;
var phase64FermionCurrentDerived = JsonString(phase64.RootElement, "terminalStatus") == "non-proxy-fermion-current-matrix-element-derived"
    && JsonString(phase64.RootElement, "sourceKind") == "non-proxy-fermion-current-matrix-element"
    && JsonString(phase64.RootElement, "normalizationConventionId") == "physical-weak-coupling-normalization:su2-canonical-trace-half-v1";

var phase82VectorLength = phase82.RootElement.TryGetProperty("sourceArtifact", out var phase82SourceArtifact)
    ? JsonInt(phase82SourceArtifact, "vectorLength")
    : null;
var phase82NormalizationConvention = phase82.RootElement.TryGetProperty("sourceArtifact", out phase82SourceArtifact)
    ? JsonString(phase82SourceArtifact, "normalizationConvention")
    : null;
var phase84EdgeCount = phase84.RootElement.TryGetProperty("geometry", out var phase84Geometry)
    ? JsonInt(phase84Geometry, "edgeCount")
    : null;
var phase84ExpectedBosonVectorLength = phase84.RootElement.TryGetProperty("geometry", out phase84Geometry)
    ? JsonInt(phase84Geometry, "expectedBosonVectorLength")
    : null;
var phase84MeshSource = phase84.RootElement.TryGetProperty("geometry", out phase84Geometry)
    ? JsonString(phase84Geometry, "meshSource")
    : null;
const int phase84DimG = 3;
var phase12DiscreteVectorLengthExplained = phase82VectorLength == 156
    && phase84EdgeCount == 52
    && phase84ExpectedBosonVectorLength == phase84EdgeCount * phase84DimG
    && phase84ExpectedBosonVectorLength == phase82VectorLength
    && phase82NormalizationConvention == "unit-M-norm";

var so13AdjointDimension = 13 * 12 / 2;
var twiceSo13AdjointDimension = 2 * so13AdjointDimension;
var twiceSo13AdjointDimensionMatchesPhase12VectorLength = twiceSo13AdjointDimension == phase82VectorLength;
var spin13OrSo13DimensionSourceEvidencePresent = false;
var spin13OrSo13DimensionIsPhase12VectorSource = false;

var p225ObstructionCertified = JsonBool(phase225.RootElement, "representationNormalizationObstructionCertified") is true;
var fundamentalCasimir = phase249.RootElement.TryGetProperty("localInvariantInputs", out var localInvariantInputs)
    ? JsonDouble(localInvariantInputs, "fundamentalCasimir")
    : null;
var adjointCasimir = phase249.RootElement.TryGetProperty("localInvariantInputs", out localInvariantInputs)
    ? JsonDouble(localInvariantInputs, "adjointCasimir")
    : null;
var su2AdjointOverFundamentalCasimirRatio = adjointCasimir / fundamentalCasimir;
var casimirEightThirdsArithmeticMatches = Close(su2AdjointOverFundamentalCasimirRatio, 8.0 / 3.0);
var wzInvariantFormulaCandidateFound = JsonBool(phase249.RootElement, "wzInvariantFormulaCandidateFound") is true;
var wzInvariantFormulaSourceBackedForBosonApplication = JsonBool(phase249.RootElement, "wzInvariantFormulaSourceBacked") is true;
var casimirRatioSourceBackedAsLocalInvariant = wzInvariantFormulaCandidateFound && casimirEightThirdsArithmeticMatches;
var casimirRatioSourceBackedForBosonApplication = wzInvariantFormulaSourceBackedForBosonApplication;

var p302BestCandidate = phase302.RootElement.GetProperty("bestSourceInvariantRawCommonCandidate");
var p302CandidateId = JsonString(p302BestCandidate, "candidateId");
var p302CommonScaleId = JsonString(p302BestCandidate, "commonScaleId");
var p302ParticleLawId = JsonString(p302BestCandidate, "particleLawId");
var p302CommonScaleValue = JsonDouble(p302BestCandidate, "commonScaleValue");
var p302WParticleMultiplier = JsonDouble(p302BestCandidate, "wParticleMultiplier");
var p302ZParticleMultiplier = JsonDouble(p302BestCandidate, "zParticleMultiplier");
var p302WTotalScale = JsonDouble(p302BestCandidate, "wTotalScale");
var p302ZTotalScale = JsonDouble(p302BestCandidate, "zTotalScale");
var p302RawAndCommonGatesPassed = JsonBool(p302BestCandidate, "rawAndCommonGatesPassed") is true;
var p302StableRawCommonGatesPassed = JsonBool(p302BestCandidate, "stableRawCommonGatesPassed") is true;
var p302CommonScaleApplicationTheoremPresent = JsonBool(p302BestCandidate, "commonScaleApplicationTheoremPresent") is true;
var p302ParticleLawApplicationTheoremPresent = JsonBool(p302BestCandidate, "particleLawApplicationTheoremPresent") is true;
var p302PromotionEligible = JsonBool(p302BestCandidate, "promotionEligible") is true;

var phase307SelectionLawAuditPassed = JsonBool(phase307.RootElement, "targetIndependentDecoupledWzRowSelectionLawAuditPassed") is true;
var phase307P302ScaledStableCommonSelectionLawCount = JsonInt(phase307.RootElement, "p302ScaledStableCommonSelectionLawCount") ?? -1;
var phase307CanFillPhase201WzContract = JsonBool(phase307.RootElement, "canFillPhase201WzContract") is true;
var phase308ScaleTransferAllowed = JsonBool(phase308.RootElement, "scaleTransferAllowed") is true;
var phase308CanFillPhase201WzContract = JsonBool(phase308.RootElement, "canFillPhase201WzContract") is true;
var phase309HiddenMeasureConversionPresent = JsonBool(phase309.RootElement, "hiddenMeasureConversionPresent") is true;
var phase309VectorLengthScalePromotable = JsonBool(phase309.RootElement, "sourceModeVectorLengthScalePromotable") is true;
var phase310ProvidesVectorLengthTheorem = JsonBool(phase310.RootElement, "completionDraftProvidesVectorLengthNormalizationTheorem") is true;
var phase310ProvidesCasimirTheorem = JsonBool(phase310.RootElement, "completionDraftProvidesCasimirApplicationTheorem") is true;
var phase310ProvidesChargedLadderTransferTheorem = JsonBool(phase310.RootElement, "completionDraftProvidesChargedLadderTransferTheorem") is true;
var phase310CanPromotePhase302Lead = JsonBool(phase310.RootElement, "completionDraftCanPromotePhase302Lead") is true;
var phase313ProvidesWRows = JsonBool(phase313.RootElement, "officialDraftProvidesWChargedProjectionRows") is true;
var phase313ProvidesZProjection = JsonBool(phase313.RootElement, "officialDraftProvidesZSourceRowProjection") is true;
var phase313ProvidesObservedElectroweakGaugeEmbedding = JsonBool(phase313.RootElement, "officialDraftProvidesObservedElectroweakGaugeEmbedding") is true;

var su2CasimirRatioWouldApplyToTripletSymmetrically = true;
var wOnlyCasimirMultiplierJustified = false;
var zUnitMultiplierJustified = false;
var neutralMixingProjectionPresent = phase313ProvidesZProjection || phase313ProvidesObservedElectroweakGaugeEmbedding;
var dimensionCasimirSourceLawPromotesWzMasses = false;
var dimensionCasimirSourceLawPromotesHiggsMass = false;
var canFillPhase201WzContract = false;
var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;

var checks = new[]
{
    new Check(
        "phase302-dimension-casimir-lead-materialized",
        p302CandidateId == "source-mode-vector-length::adjoint-casimir-over-fundamental-casimir"
            && p302CommonScaleId == "source-mode-vector-length"
            && p302ParticleLawId == "adjoint-casimir-over-fundamental-casimir"
            && p302CommonScaleValue == 156.0
            && Close(p302WParticleMultiplier, 8.0 / 3.0)
            && p302ZParticleMultiplier == 1.0
            && p302WTotalScale == 416.0
            && p302ZTotalScale == 156.0
            && p302RawAndCommonGatesPassed
            && !p302PromotionEligible,
        $"candidateId={p302CandidateId}; commonScale={p302CommonScaleValue}; wMultiplier={p302WParticleMultiplier}; zMultiplier={p302ZParticleMultiplier}; rawAndCommon={p302RawAndCommonGatesPassed}; promotionEligible={p302PromotionEligible}"),
    new Check(
        "phase12-vector-length-is-discrete-coordinate-count",
        phase12DiscreteVectorLengthExplained
            && !phase309HiddenMeasureConversionPresent
            && !phase309VectorLengthScalePromotable,
        $"phase82VectorLength={phase82VectorLength}; normalization={phase82NormalizationConvention}; phase84EdgeCount={phase84EdgeCount}; dimG={phase84DimG}; expectedBosonVectorLength={phase84ExpectedBosonVectorLength}; hiddenMeasureConversion={phase309HiddenMeasureConversionPresent}; vectorLengthScalePromotable={phase309VectorLengthScalePromotable}"),
    new Check(
        "spin13-so13-dimension-coincidence-is-not-source-law",
        twiceSo13AdjointDimensionMatchesPhase12VectorLength
            && !spin13OrSo13DimensionSourceEvidencePresent
            && !spin13OrSo13DimensionIsPhase12VectorSource
            && phase12DiscreteVectorLengthExplained,
        $"so13AdjointDimension={so13AdjointDimension}; twiceSo13AdjointDimension={twiceSo13AdjointDimension}; matchesPhase12VectorLength={twiceSo13AdjointDimensionMatchesPhase12VectorLength}; spin13OrSo13DimensionSourceEvidencePresent={spin13OrSo13DimensionSourceEvidencePresent}; phase84MeshSource={phase84MeshSource}"),
    new Check(
        "su2-casimir-arithmetic-is-not-application-theorem",
        phase63TraceHalfConventionDerived
            && phase64FermionCurrentDerived
            && p225ObstructionCertified
            && casimirRatioSourceBackedAsLocalInvariant
            && !casimirRatioSourceBackedForBosonApplication
            && !p302ParticleLawApplicationTheoremPresent,
        $"phase63TraceHalf={phase63TraceHalfConventionDerived}; phase64FermionCurrent={phase64FermionCurrentDerived}; p225Obstruction={p225ObstructionCertified}; adjointOverFundamental={su2AdjointOverFundamentalCasimirRatio}; casimirSourceBackedForBosonApplication={casimirRatioSourceBackedForBosonApplication}; p302ParticleLawApplicationTheoremPresent={p302ParticleLawApplicationTheoremPresent}"),
    new Check(
        "w-only-casimir-and-z-unit-split-not-justified",
        su2CasimirRatioWouldApplyToTripletSymmetrically
            && !wOnlyCasimirMultiplierJustified
            && !zUnitMultiplierJustified
            && !neutralMixingProjectionPresent
            && !phase313ProvidesWRows
            && !phase313ProvidesZProjection,
        $"su2TripletSymmetric={su2CasimirRatioWouldApplyToTripletSymmetrically}; wOnlyCasimirMultiplierJustified={wOnlyCasimirMultiplierJustified}; zUnitMultiplierJustified={zUnitMultiplierJustified}; neutralMixingProjectionPresent={neutralMixingProjectionPresent}; phase313WRows={phase313ProvidesWRows}; phase313ZProjection={phase313ProvidesZProjection}"),
    new Check(
        "charged-ladder-transfer-remains-nonpromotional",
        phase307SelectionLawAuditPassed
            && phase307P302ScaledStableCommonSelectionLawCount >= 1
            && !phase307CanFillPhase201WzContract
            && !phase308ScaleTransferAllowed
            && !phase308CanFillPhase201WzContract
            && !phase310ProvidesVectorLengthTheorem
            && !phase310ProvidesCasimirTheorem
            && !phase310ProvidesChargedLadderTransferTheorem
            && !phase310CanPromotePhase302Lead,
        $"phase307P302ScaledStableCommonSelectionLawCount={phase307P302ScaledStableCommonSelectionLawCount}; phase307CanFill={phase307CanFillPhase201WzContract}; phase308ScaleTransferAllowed={phase308ScaleTransferAllowed}; phase310VectorLengthTheorem={phase310ProvidesVectorLengthTheorem}; phase310CasimirTheorem={phase310ProvidesCasimirTheorem}; phase310ChargedLadderTransferTheorem={phase310ProvidesChargedLadderTransferTheorem}"),
    new Check(
        "source-contract-remains-unfilled",
        !dimensionCasimirSourceLawPromotesWzMasses
            && !dimensionCasimirSourceLawPromotesHiggsMass
            && !canFillPhase201WzContract
            && wzMissingFieldCount == 15
            && higgsMissingFieldCount == 14
            && !p302CommonScaleApplicationTheoremPresent
            && !p302StableRawCommonGatesPassed,
        $"dimensionCasimirSourceLawPromotesWzMasses={dimensionCasimirSourceLawPromotesWzMasses}; canFillPhase201WzContract={canFillPhase201WzContract}; wzMissingFieldCount={wzMissingFieldCount}; higgsMissingFieldCount={higgsMissingFieldCount}; p302CommonScaleApplicationTheoremPresent={p302CommonScaleApplicationTheoremPresent}; p302StableRawCommonGatesPassed={p302StableRawCommonGatesPassed}"),
};

var dimensionCasimirWzSourceLawAuditPassed = checks.All(check => check.Passed)
    && !dimensionCasimirSourceLawPromotesWzMasses
    && !canFillPhase201WzContract;
var terminalStatus = dimensionCasimirWzSourceLawAuditPassed
    ? "dimension-casimir-wz-source-law-audit-coordinate-count-and-casimir-arithmetic-not-source-law"
    : "dimension-casimir-wz-source-law-audit-review-required";

var result = new
{
    phaseId = "phase314-dimension-casimir-wz-source-law-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    dimensionCasimirWzSourceLawAuditPassed,
    phase12DiscreteVectorLengthExplained,
    phase82VectorLength,
    phase82NormalizationConvention,
    phase84EdgeCount,
    phase84DimG,
    phase84ExpectedBosonVectorLength,
    phase84MeshSource,
    so13AdjointDimension,
    twiceSo13AdjointDimension,
    twiceSo13AdjointDimensionMatchesPhase12VectorLength,
    spin13OrSo13DimensionSourceEvidencePresent,
    spin13OrSo13DimensionIsPhase12VectorSource,
    fundamentalCasimir,
    adjointCasimir,
    su2AdjointOverFundamentalCasimirRatio,
    casimirEightThirdsArithmeticMatches,
    casimirRatioSourceBackedAsLocalInvariant,
    casimirRatioSourceBackedForBosonApplication,
    phase63TraceHalfConventionDerived,
    phase64FermionCurrentDerived,
    p225ObstructionCertified,
    p302CandidateId,
    p302CommonScaleId,
    p302ParticleLawId,
    p302CommonScaleValue,
    p302WParticleMultiplier,
    p302ZParticleMultiplier,
    p302WTotalScale,
    p302ZTotalScale,
    p302RawAndCommonGatesPassed,
    p302StableRawCommonGatesPassed,
    p302CommonScaleApplicationTheoremPresent,
    p302ParticleLawApplicationTheoremPresent,
    p302PromotionEligible,
    su2CasimirRatioWouldApplyToTripletSymmetrically,
    wOnlyCasimirMultiplierJustified,
    zUnitMultiplierJustified,
    neutralMixingProjectionPresent,
    phase307P302ScaledStableCommonSelectionLawCount,
    phase307CanFillPhase201WzContract,
    phase308ScaleTransferAllowed,
    phase308CanFillPhase201WzContract,
    phase309HiddenMeasureConversionPresent,
    phase309VectorLengthScalePromotable,
    phase310ProvidesVectorLengthTheorem,
    phase310ProvidesCasimirTheorem,
    phase310ProvidesChargedLadderTransferTheorem,
    phase310CanPromotePhase302Lead,
    dimensionCasimirSourceLawPromotesWzMasses,
    dimensionCasimirSourceLawPromotesHiggsMass,
    canFillPhase201WzContract,
    wzMissingFieldCount,
    higgsMissingFieldCount,
    checks,
    decision = dimensionCasimirWzSourceLawAuditPassed
        ? "Do not promote the Phase302/307 W/Z near-pass by interpreting 156 as a Spin(13)/SO(13) dimension law or by treating the SU(2) adjoint/fundamental Casimir ratio as a W-only source theorem. The current evidence identifies 156 as a Phase12 discrete connection-vector coordinate count, while 8/3 is valid SU(2) arithmetic without a physical W-only application theorem, Z projection, or charged-ladder transfer theorem."
        : "Review the dimension/Casimir W/Z source-law audit before relying on this boundary.",
    nextRequiredArtifact = new[]
    {
        "A source-side theorem deriving the physical W/Z normalization from the GU operator, not from Phase12 coordinate count or post-candidate arithmetic.",
        "A W/Z particle-law theorem explaining why an adjoint/fundamental Casimir ratio applies to W rows while Z rows use multiplier one, including neutral mixing/projection.",
        "A charged-ladder transfer theorem with branch-stable W and Z source rows before target comparison.",
        "Phase201/P209 W/Z source-lineage rows with derivation, raw, common-bridge, target-comparison, and stability fields filled.",
    },
    sourceEvidence = new
    {
        phase63Path = Phase63Path,
        phase64Path = Phase64Path,
        phase82Path = Phase82Path,
        phase84Path = Phase84Path,
        phase213Path = Phase213Path,
        phase225Path = Phase225Path,
        phase249Path = Phase249Path,
        phase302Path = Phase302Path,
        phase307Path = Phase307Path,
        phase308Path = Phase308Path,
        phase309Path = Phase309Path,
        phase310Path = Phase310Path,
        phase313Path = Phase313Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "dimension_casimir_wz_source_law_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "dimension_casimir_wz_source_law_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.dimensionCasimirWzSourceLawAuditPassed,
        result.phase12DiscreteVectorLengthExplained,
        result.phase82VectorLength,
        result.phase84EdgeCount,
        result.phase84DimG,
        result.phase84ExpectedBosonVectorLength,
        result.so13AdjointDimension,
        result.twiceSo13AdjointDimension,
        result.twiceSo13AdjointDimensionMatchesPhase12VectorLength,
        result.spin13OrSo13DimensionSourceEvidencePresent,
        result.spin13OrSo13DimensionIsPhase12VectorSource,
        result.su2AdjointOverFundamentalCasimirRatio,
        result.casimirEightThirdsArithmeticMatches,
        result.casimirRatioSourceBackedAsLocalInvariant,
        result.casimirRatioSourceBackedForBosonApplication,
        result.phase63TraceHalfConventionDerived,
        result.phase64FermionCurrentDerived,
        result.p225ObstructionCertified,
        result.p302CandidateId,
        result.p302CommonScaleValue,
        result.p302WParticleMultiplier,
        result.p302ZParticleMultiplier,
        result.p302RawAndCommonGatesPassed,
        result.p302StableRawCommonGatesPassed,
        result.p302CommonScaleApplicationTheoremPresent,
        result.p302ParticleLawApplicationTheoremPresent,
        result.p302PromotionEligible,
        result.su2CasimirRatioWouldApplyToTripletSymmetrically,
        result.wOnlyCasimirMultiplierJustified,
        result.zUnitMultiplierJustified,
        result.neutralMixingProjectionPresent,
        result.phase307P302ScaledStableCommonSelectionLawCount,
        result.phase307CanFillPhase201WzContract,
        result.phase308ScaleTransferAllowed,
        result.phase310ProvidesVectorLengthTheorem,
        result.phase310ProvidesCasimirTheorem,
        result.phase310ProvidesChargedLadderTransferTheorem,
        result.dimensionCasimirSourceLawPromotesWzMasses,
        result.dimensionCasimirSourceLawPromotesHiggsMass,
        result.canFillPhase201WzContract,
        result.wzMissingFieldCount,
        result.higgsMissingFieldCount,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"dimensionCasimirWzSourceLawAuditPassed={dimensionCasimirWzSourceLawAuditPassed}");
Console.WriteLine($"phase12DiscreteVectorLengthExplained={phase12DiscreteVectorLengthExplained}");
Console.WriteLine($"casimirEightThirdsArithmeticMatches={casimirEightThirdsArithmeticMatches}");
Console.WriteLine($"casimirRatioSourceBackedForBosonApplication={casimirRatioSourceBackedForBosonApplication}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

static double? JsonDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var value) ? value : null;

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static bool Close(double? left, double right, double tolerance = 1.0e-12) =>
    left is not null && Math.Abs(left.Value - right) <= tolerance;

sealed record Check(string CheckId, bool Passed, string Detail);
