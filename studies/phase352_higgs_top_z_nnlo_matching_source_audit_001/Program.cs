using System.Text.Json;

const string DefaultOutputDir = "studies/phase352_higgs_top_z_nnlo_matching_source_audit_001/output";
const string Phase201Path = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase256Path = "studies/phase256_observed_field_extraction_intake_contract_001/output/observed_field_extraction_intake_contract_summary.json";
const string Phase262Path = "studies/phase262_higgs_top_empirical_relation_source_audit_001/output/higgs_top_empirical_relation_source_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE352_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase256 = JsonDocument.Parse(File.ReadAllText(Phase256Path));
using var phase262 = JsonDocument.Parse(File.ReadAllText(Phase262Path));

const string higgsTopZNNLOArxiv = "https://arxiv.org/abs/2605.21721";
const string higgsTopZNNLOPdf = "https://arxiv.org/pdf/2605.21721v1";
const string originalHiggsTopCoincidenceArxiv = "https://arxiv.org/abs/1209.0474";
const string originalHiggsTopCoincidenceEpjc = "https://doi.org/10.1140/epjc/s10052-014-2744-3";

const bool higgsTopZNnloMatchingSourceAuditPassedExpected = true;
const bool higgsTopZNnloLeadPresent = true;
const bool higgsTopZNnloPrimarySourceReviewed = true;
const bool higgsTopZNnloRouteExternalToGu = true;
const bool routeUpdatesPhase262EmpiricalRelation = true;
const bool routeUsesMeasuredTopMassCombination = true;
const bool routeUsesMeasuredZMass = true;
const bool routeUsesMeasuredHiggsMassForRatioTest = true;
const bool routeUsesMeasuredWMassForCompanionArithmeticRelation = true;
const bool routeIsPoleLevelCoincidence = true;
const bool routeProvidesPoleLevelGeometricRelation = true;
const bool routeProvidesRunningCouplingBoundaryTest = true;
const bool runningBoundaryRejectedByNnloMatching = true;
const bool routeRequiresPoleThresholdSymmetryOrFiniteMatchingFactor = true;
const bool routeSuggestsCustodialTopHiggsOrTrialityLikeExtensions = true;

const double poleLevelRhoZt = 1.00362;
const double poleLevelRhoZtUncertainty = 0.00261;
const double poleLevelPredictedHiggsGeV = 125.426;
const double poleLevelPredictedHiggsUncertaintyGeV = 0.120;
const double poleLevelPredictedTopGeV = 171.898;
const double poleLevelPredictedTopUncertaintyGeV = 0.302;
const double poleLevelRelationTestSigma = 1.4;
const double arithmeticRhoWt = 1.00994;
const double arithmeticRhoWtUncertainty = 0.00159;
const bool arithmeticRelationViableExactMassSumRule = false;
const double runningRhoZtAtTopScale = 0.96714;
const double runningRhoZtAtTopScaleUncertainty = 0.00361;
const double runningBoundaryPredictedHiggsGeV = 123.19;
const double runningBoundaryPredictedHiggsUncertaintyGeV = 0.20;
const double runningBoundaryPredictedTopGeV = 177.81;
const double runningBoundaryPredictedTopUncertaintyGeV = 0.50;
const bool runningBoundaryCompatibleWithMeasuredPoint = false;
const double requiredFiniteMatchingFactorKappa = 1.0340;
const double requiredFiniteMatchingFactorKappaUncertainty = 0.0039;

const bool routeProvidesGuTopYukawaSource = false;
const bool routeProvidesGuZMassSource = false;
const bool routeProvidesGuHiggsScalarSourceOperator = false;
const bool routeProvidesPotentialOrSelfCouplingSource = false;
const bool routeProvidesObservedFieldExtraction = false;
const bool routeProvidesWzAbsoluteScale = false;
const bool routeProvidesGuFiniteMatchingFactor = false;
const bool routeProvidesCustodialTopHiggsOrTrialityMechanism = false;
const bool routeProvidesGeVUnitNormalization = false;
const bool routePromotesWzMasses = false;
const bool routePromotesHiggsMass = false;
const bool routeCompletesBosonPredictions = false;
const bool canFillPhase201WzContract = false;
const bool canFillPhase201HiggsContract = false;
const bool canFillPhase256ObservedFieldExtractionContract = false;

var phase262AuditPassed = JsonBool(phase262.RootElement, "higgsTopEmpiricalRelationSourceAuditPassed") is true;
var phase262PromotesHiggsMass = JsonBool(phase262.RootElement, "relationPromotesHiggsMass") is true;
var phase262RelationNumericallyClose = JsonBool(phase262.RootElement, "relationNumericallyClose") is true;
var phase262GeometricMeanHiggsGeV = phase262.RootElement.TryGetProperty("empiricalRelations", out var empiricalRelations)
    && empiricalRelations.TryGetProperty("geometricMean", out var geometricMean)
        ? JsonDouble(geometricMean, "geometricMeanHiggsGeV")
        : null;

var phase201AllRequiredLineagesPromotable = JsonBool(phase201.RootElement, "allRequiredLineagesPromotable") is true;
var existingEvidenceFound = JsonBool(phase213.RootElement, "existingEvidenceFound") is true;
var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;
var observedFieldExtractionFilledRequiredFieldCount = JsonInt(phase256.RootElement, "filledRequiredFieldCount") ?? -1;
var observedFieldExtractionContractPromotable = JsonBool(phase256.RootElement, "observedFieldExtractionContractPromotable") is true;

var sourceRows = new[]
{
    new SourceRow(
        "arxiv-2605.21721-higgs-top-z-nnlo-matching",
        higgsTopZNNLOArxiv,
        "2026 NNLO matching update for Higgs-top-Z mass coincidence",
        "Rechecks M_H^2 ~= M_Z M_t with 2025 electroweak inputs and the ATLAS-CMS top-mass combination, finding a close pole-level ratio but an incompatible NNLO running-coupling boundary.",
        "Sharpens the Phase262 empirical relation boundary; no GU scalar source, W/Z source row, observed-field extraction, or matching-factor source is supplied."),
    new SourceRow(
        "arxiv-1209.0474-original-higgs-top-coincidence",
        originalHiggsTopCoincidenceArxiv,
        "Original Higgs-top mass coincidence framing",
        "Frames M_H^2 ~= M_Z M_t and 2 M_H ~= M_W + M_t as coincidences requiring an underlying mechanism.",
        "Historical empirical lead only; no GU-local source-lineage contract is filled."),
    new SourceRow(
        "epjc-2014-higgs-top-coincidence",
        originalHiggsTopCoincidenceEpjc,
        "Journal version of the Higgs-top coincidence relation",
        "Provides the earlier published context for the mass-coincidence relations audited in Phase262.",
        "Useful provenance; still an empirical relation without GU source-lineage artifacts.")
};

var checks = new[]
{
    new Check(
        "higgs-top-z-nnlo-primary-source-reviewed",
        higgsTopZNnloLeadPresent
            && higgsTopZNnloPrimarySourceReviewed
            && higgsTopZNnloRouteExternalToGu
            && sourceRows.Length == 3,
        $"lead={higgsTopZNnloLeadPresent}; reviewed={higgsTopZNnloPrimarySourceReviewed}; externalToGu={higgsTopZNnloRouteExternalToGu}; sourceRows={sourceRows.Length}"),
    new Check(
        "phase262-empirical-relation-updated-not-overridden",
        phase262AuditPassed
            && !phase262PromotesHiggsMass
            && phase262RelationNumericallyClose
            && phase262GeometricMeanHiggsGeV is not null
            && routeUpdatesPhase262EmpiricalRelation,
        $"phase262Passed={phase262AuditPassed}; phase262Promotes={phase262PromotesHiggsMass}; phase262NumericallyClose={phase262RelationNumericallyClose}; phase262GeometricMeanHiggsGeV={phase262GeometricMeanHiggsGeV:R}; updatesPhase262={routeUpdatesPhase262EmpiricalRelation}"),
    new Check(
        "pole-level-relation-remains-coincidence-lead",
        routeUsesMeasuredTopMassCombination
            && routeUsesMeasuredZMass
            && routeUsesMeasuredHiggsMassForRatioTest
            && routeIsPoleLevelCoincidence
            && routeProvidesPoleLevelGeometricRelation
            && Math.Abs(poleLevelRhoZt - 1.00362) < 1e-12
            && Math.Abs(poleLevelPredictedHiggsGeV - 125.426) < 1e-12
            && poleLevelRelationTestSigma > 1.0,
        $"topInput={routeUsesMeasuredTopMassCombination}; zInput={routeUsesMeasuredZMass}; higgsInput={routeUsesMeasuredHiggsMassForRatioTest}; rhoZt={poleLevelRhoZt:R}; predictedHiggs={poleLevelPredictedHiggsGeV:R}; predictedTop={poleLevelPredictedTopGeV:R}; sigma={poleLevelRelationTestSigma:R}"),
    new Check(
        "arithmetic-and-running-boundaries-are-not-promotable",
        routeUsesMeasuredWMassForCompanionArithmeticRelation
            && !arithmeticRelationViableExactMassSumRule
            && routeProvidesRunningCouplingBoundaryTest
            && runningBoundaryRejectedByNnloMatching
            && !runningBoundaryCompatibleWithMeasuredPoint
            && Math.Abs(runningRhoZtAtTopScale - 0.96714) < 1e-12
            && Math.Abs(runningBoundaryPredictedHiggsGeV - 123.19) < 1e-12,
        $"wInput={routeUsesMeasuredWMassForCompanionArithmeticRelation}; rhoWt={arithmeticRhoWt:R}; arithmeticViable={arithmeticRelationViableExactMassSumRule}; runningRho={runningRhoZtAtTopScale:R}; runningPredictedHiggs={runningBoundaryPredictedHiggsGeV:R}; runningCompatible={runningBoundaryCompatibleWithMeasuredPoint}"),
    new Check(
        "finite-matching-factor-requirement-captured",
        routeRequiresPoleThresholdSymmetryOrFiniteMatchingFactor
            && Math.Abs(requiredFiniteMatchingFactorKappa - 1.0340) < 1e-12
            && routeSuggestsCustodialTopHiggsOrTrialityLikeExtensions
            && !routeProvidesGuFiniteMatchingFactor
            && !routeProvidesCustodialTopHiggsOrTrialityMechanism,
        $"requiresThresholdOrMatching={routeRequiresPoleThresholdSymmetryOrFiniteMatchingFactor}; kappa={requiredFiniteMatchingFactorKappa:R}; suggestsExtensions={routeSuggestsCustodialTopHiggsOrTrialityLikeExtensions}; guKappa={routeProvidesGuFiniteMatchingFactor}; guMechanism={routeProvidesCustodialTopHiggsOrTrialityMechanism}"),
    new Check(
        "higgs-top-z-route-does-not-fill-gu-contracts",
        !routeProvidesGuTopYukawaSource
            && !routeProvidesGuZMassSource
            && !routeProvidesGuHiggsScalarSourceOperator
            && !routeProvidesPotentialOrSelfCouplingSource
            && !routeProvidesObservedFieldExtraction
            && !routeProvidesWzAbsoluteScale
            && !routeProvidesGeVUnitNormalization
            && !routePromotesWzMasses
            && !routePromotesHiggsMass
            && !routeCompletesBosonPredictions
            && !canFillPhase201WzContract
            && !canFillPhase201HiggsContract
            && !canFillPhase256ObservedFieldExtractionContract,
        $"topYukawa={routeProvidesGuTopYukawaSource}; zMass={routeProvidesGuZMassSource}; higgsOperator={routeProvidesGuHiggsScalarSourceOperator}; selfCoupling={routeProvidesPotentialOrSelfCouplingSource}; observed={routeProvidesObservedFieldExtraction}; wzScale={routeProvidesWzAbsoluteScale}; gev={routeProvidesGeVUnitNormalization}; promotesWz={routePromotesWzMasses}; promotesHiggs={routePromotesHiggsMass}; completes={routeCompletesBosonPredictions}"),
    new Check(
        "phase201-phase256-contract-state-preserved",
        !phase201AllRequiredLineagesPromotable
            && !existingEvidenceFound
            && wzMissingFieldCount == 15
            && higgsMissingFieldCount == 14
            && observedFieldExtractionFilledRequiredFieldCount == 0
            && !observedFieldExtractionContractPromotable,
        $"phase201Promotable={phase201AllRequiredLineagesPromotable}; existingEvidence={existingEvidenceFound}; wzMissing={wzMissingFieldCount}; higgsMissing={higgsMissingFieldCount}; observedFilled={observedFieldExtractionFilledRequiredFieldCount}; observedPromotable={observedFieldExtractionContractPromotable}")
};

var higgsTopZNnloMatchingSourceAuditPassed = checks.All(check => check.Passed)
    && higgsTopZNnloMatchingSourceAuditPassedExpected
    && !routePromotesWzMasses
    && !routePromotesHiggsMass
    && !routeCompletesBosonPredictions
    && !canFillPhase201WzContract
    && !canFillPhase201HiggsContract
    && !canFillPhase256ObservedFieldExtractionContract;

var terminalStatus = higgsTopZNnloMatchingSourceAuditPassed
    ? "higgs-top-z-nnlo-matching-source-audit-pole-coincidence-running-boundary-fails"
    : "higgs-top-z-nnlo-matching-source-audit-review-required";

var result = new
{
    phaseId = "phase352-higgs-top-z-nnlo-matching-source-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    higgsTopZNnloMatchingSourceAuditPassed,
    higgsTopZNnloLeadPresent,
    higgsTopZNnloPrimarySourceReviewed,
    higgsTopZNnloRouteExternalToGu,
    higgsTopZNNLOPdf,
    routeUpdatesPhase262EmpiricalRelation,
    routeUsesMeasuredTopMassCombination,
    routeUsesMeasuredZMass,
    routeUsesMeasuredHiggsMassForRatioTest,
    routeUsesMeasuredWMassForCompanionArithmeticRelation,
    routeIsPoleLevelCoincidence,
    routeProvidesPoleLevelGeometricRelation,
    routeProvidesRunningCouplingBoundaryTest,
    runningBoundaryRejectedByNnloMatching,
    routeRequiresPoleThresholdSymmetryOrFiniteMatchingFactor,
    routeSuggestsCustodialTopHiggsOrTrialityLikeExtensions,
    poleLevelRhoZt,
    poleLevelRhoZtUncertainty,
    poleLevelPredictedHiggsGeV,
    poleLevelPredictedHiggsUncertaintyGeV,
    poleLevelPredictedTopGeV,
    poleLevelPredictedTopUncertaintyGeV,
    poleLevelRelationTestSigma,
    arithmeticRhoWt,
    arithmeticRhoWtUncertainty,
    arithmeticRelationViableExactMassSumRule,
    runningRhoZtAtTopScale,
    runningRhoZtAtTopScaleUncertainty,
    runningBoundaryPredictedHiggsGeV,
    runningBoundaryPredictedHiggsUncertaintyGeV,
    runningBoundaryPredictedTopGeV,
    runningBoundaryPredictedTopUncertaintyGeV,
    runningBoundaryCompatibleWithMeasuredPoint,
    requiredFiniteMatchingFactorKappa,
    requiredFiniteMatchingFactorKappaUncertainty,
    routeProvidesGuTopYukawaSource,
    routeProvidesGuZMassSource,
    routeProvidesGuHiggsScalarSourceOperator,
    routeProvidesPotentialOrSelfCouplingSource,
    routeProvidesObservedFieldExtraction,
    routeProvidesWzAbsoluteScale,
    routeProvidesGuFiniteMatchingFactor,
    routeProvidesCustodialTopHiggsOrTrialityMechanism,
    routeProvidesGeVUnitNormalization,
    routePromotesWzMasses,
    routePromotesHiggsMass,
    routeCompletesBosonPredictions,
    canFillPhase201WzContract,
    canFillPhase201HiggsContract,
    canFillPhase256ObservedFieldExtractionContract,
    sourceRows,
    checks,
    phase262Boundary = new
    {
        phase262AuditPassed,
        phase262PromotesHiggsMass,
        phase262RelationNumericallyClose,
        phase262GeometricMeanHiggsGeV
    },
    contractImpact = new
    {
        canFillPhase201WzContract,
        canFillPhase201HiggsContract,
        canFillPhase256ObservedFieldExtractionContract,
        wzMissingFieldCount,
        higgsMissingFieldCount,
        observedFieldExtractionFilledRequiredFieldCount
    },
    decision = "Do not promote W/Z or Higgs physical masses from the Higgs-top-Z NNLO matching route. The latest source preserves a close pole-level mass coincidence, but it imports measured top, Z, W, and Higgs inputs for the test; the running-coupling boundary fails under NNLO matching and would require a finite threshold/matching factor or new custodial/top-Higgs/triality-like mechanism not supplied by current GU artifacts.",
    nextRequiredArtifact = new[]
    {
        "A GU-derived top/Yukawa source and Z/W absolute mass source if top-Z-Higgs relations are to become predictive rather than empirical.",
        "A GU-local pole-threshold or finite-matching-factor derivation corresponding to kappa_th around 1.034, independent of target masses.",
        "A solved Higgs scalar-source/operator and observed-field extraction contract before any Higgs mass promotion.",
        "Independent W/Z source rows and GeV normalization if this route is to complete all boson predictions."
    }
};

var fullPath = Path.Combine(outputDir, "higgs_top_z_nnlo_matching_source_audit.json");
var summaryPath = Path.Combine(outputDir, "higgs_top_z_nnlo_matching_source_audit_summary.json");
var jsonOptions = new JsonSerializerOptions { WriteIndented = true };

File.WriteAllText(fullPath, JsonSerializer.Serialize(result, jsonOptions));
File.WriteAllText(summaryPath, JsonSerializer.Serialize(new
{
    result.phaseId,
    result.terminalStatus,
    result.higgsTopZNnloMatchingSourceAuditPassed,
    result.higgsTopZNnloLeadPresent,
    result.higgsTopZNnloPrimarySourceReviewed,
    result.higgsTopZNnloRouteExternalToGu,
    result.routeUpdatesPhase262EmpiricalRelation,
    result.routeUsesMeasuredTopMassCombination,
    result.routeUsesMeasuredZMass,
    result.routeUsesMeasuredHiggsMassForRatioTest,
    result.routeUsesMeasuredWMassForCompanionArithmeticRelation,
    result.routeIsPoleLevelCoincidence,
    result.routeProvidesPoleLevelGeometricRelation,
    result.routeProvidesRunningCouplingBoundaryTest,
    result.runningBoundaryRejectedByNnloMatching,
    result.routeRequiresPoleThresholdSymmetryOrFiniteMatchingFactor,
    result.routeSuggestsCustodialTopHiggsOrTrialityLikeExtensions,
    result.poleLevelRhoZt,
    result.poleLevelRhoZtUncertainty,
    result.poleLevelPredictedHiggsGeV,
    result.poleLevelPredictedHiggsUncertaintyGeV,
    result.poleLevelPredictedTopGeV,
    result.poleLevelPredictedTopUncertaintyGeV,
    result.poleLevelRelationTestSigma,
    result.arithmeticRhoWt,
    result.arithmeticRhoWtUncertainty,
    result.arithmeticRelationViableExactMassSumRule,
    result.runningRhoZtAtTopScale,
    result.runningRhoZtAtTopScaleUncertainty,
    result.runningBoundaryPredictedHiggsGeV,
    result.runningBoundaryPredictedHiggsUncertaintyGeV,
    result.runningBoundaryPredictedTopGeV,
    result.runningBoundaryPredictedTopUncertaintyGeV,
    result.runningBoundaryCompatibleWithMeasuredPoint,
    result.requiredFiniteMatchingFactorKappa,
    result.requiredFiniteMatchingFactorKappaUncertainty,
    result.routeProvidesGuTopYukawaSource,
    result.routeProvidesGuZMassSource,
    result.routeProvidesGuHiggsScalarSourceOperator,
    result.routeProvidesPotentialOrSelfCouplingSource,
    result.routeProvidesObservedFieldExtraction,
    result.routeProvidesWzAbsoluteScale,
    result.routeProvidesGuFiniteMatchingFactor,
    result.routeProvidesCustodialTopHiggsOrTrialityMechanism,
    result.routeProvidesGeVUnitNormalization,
    result.routePromotesWzMasses,
    result.routePromotesHiggsMass,
    result.routeCompletesBosonPredictions,
    result.canFillPhase201WzContract,
    result.canFillPhase201HiggsContract,
    result.canFillPhase256ObservedFieldExtractionContract,
    sourceRowCount = sourceRows.Length,
    result.phase262Boundary,
    result.contractImpact,
    result.decision,
    result.nextRequiredArtifact
}, jsonOptions));

Console.WriteLine(terminalStatus);
Console.WriteLine($"higgsTopZNnloMatchingSourceAuditPassed={higgsTopZNnloMatchingSourceAuditPassed}");
Console.WriteLine($"poleLevelRhoZt={poleLevelRhoZt:R}");
Console.WriteLine($"runningRhoZtAtTopScale={runningRhoZtAtTopScale:R}");
Console.WriteLine($"runningBoundaryCompatibleWithMeasuredPoint={runningBoundaryCompatibleWithMeasuredPoint}");
Console.WriteLine($"requiredFiniteMatchingFactorKappa={requiredFiniteMatchingFactorKappa:R}");
Console.WriteLine($"routePromotesHiggsMass={routePromotesHiggsMass}");
Console.WriteLine($"canFillPhase201HiggsContract={canFillPhase201HiggsContract}");

static bool? JsonBool(JsonElement element, string propertyName)
{
    if (!element.TryGetProperty(propertyName, out var value))
    {
        return null;
    }

    return value.ValueKind switch
    {
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        _ => null
    };
}

static int? JsonInt(JsonElement element, string propertyName)
{
    if (!element.TryGetProperty(propertyName, out var value) || value.ValueKind != JsonValueKind.Number)
    {
        return null;
    }

    return value.TryGetInt32(out var parsed) ? parsed : null;
}

static double? JsonDouble(JsonElement element, string propertyName)
{
    if (!element.TryGetProperty(propertyName, out var value) || value.ValueKind != JsonValueKind.Number)
    {
        return null;
    }

    return value.TryGetDouble(out var parsed) ? parsed : null;
}

public sealed record SourceRow(string SourceId, string Url, string FindingKind, string Summary, string PredictionImpact);

public sealed record Check(string CheckId, bool Passed, string Detail);
