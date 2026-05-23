using System.Text.Json;

const string DefaultOutputDir = "studies/phase356_eguchi_hanson_substandard_higgs_source_audit_001/output";
const string Phase201Path = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase256Path = "studies/phase256_observed_field_extraction_intake_contract_001/output/observed_field_extraction_intake_contract_summary.json";
const string Phase334Path = "studies/phase334_su21_superconnection_source_audit_001/output/su21_superconnection_source_audit_summary.json";
const string Phase337Path = "studies/phase337_octonion_clifford_internal_space_source_audit_001/output/octonion_clifford_internal_space_source_audit_summary.json";
const string Phase351Path = "studies/phase351_weak_hypercharge_superselection_source_audit_001/output/weak_hypercharge_superselection_source_audit_summary.json";
const string Phase355Path = "studies/phase355_dirac_lichnerowicz_yang_mills_higgs_source_audit_001/output/dirac_lichnerowicz_yang_mills_higgs_source_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE356_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase256 = JsonDocument.Parse(File.ReadAllText(Phase256Path));
using var phase334 = JsonDocument.Parse(File.ReadAllText(Phase334Path));
using var phase337 = JsonDocument.Parse(File.ReadAllText(Phase337Path));
using var phase351 = JsonDocument.Parse(File.ReadAllText(Phase351Path));
using var phase355 = JsonDocument.Parse(File.ReadAllText(Phase355Path));

const string substandardHiggsArxiv = "https://arxiv.org/abs/hep-th/0702177";
const string substandardHiggsDoi = "https://doi.org/10.48550/arXiv.hep-th/0702177";

const bool eguchiHansonSubstandardHiggsSourceAuditPassedExpected = true;
const bool eguchiHansonSubstandardLeadPresent = true;
const bool eguchiHansonSubstandardPrimarySourceReviewed = true;
const bool eguchiHansonSubstandardRouteExternalToGu = true;
const bool routeUsesEguchiHansonMetric = true;
const bool routeProvidesGeometricAlgebraicU2Interpretation = true;
const bool routeScopeLeptonsElectroweakBosonsAndHiggs = true;
const bool routeExcludesChromodynamicsOfQuarks = true;
const bool routeProvidesHiggsFromWAndWeakAngleFormula = true;
const bool routeUsesObservedWMassInput = true;
const bool routeUsesObservedWeinbergAngleInput = true;
const bool routeDoesNotPredictWzAbsoluteMasses = true;
const bool routeDoesNotPredictWeakMixingAngle = true;
const bool routeDoesNotProvideObservedHiggsExtraction = true;
const bool routeHiggsPredictionConflictsWithObserved125 = true;

const int substandardHiggsLatestArxivVersion = 1;
const int substandardHiggsLatestRevisionYear = 2007;
const double substandardPredictedHiggsMassGeV = 115.3;
const double observedHiggsReferenceGeV = 125.2;
const double absoluteHiggsShortfallGeV = observedHiggsReferenceGeV - substandardPredictedHiggsMassGeV;
const double relativeHiggsShortfall = absoluteHiggsShortfallGeV / observedHiggsReferenceGeV;

const bool routeRequiresExternalSubstandardModel = true;
const bool routeRequiresExternalU2GeometricInterpretation = true;
const bool routeRequiresObservedWMass = true;
const bool routeRequiresObservedWeakMixingAngle = true;
const bool routeRequiresElectroweakInputNormalization = true;
const bool routeRequiresObservedMassComparison = true;
const bool routeRequiresExtensionBeyondLeptonicElectroweakSector = true;

const bool routeRequiresGuLocalEguchiHansonMap = true;
const bool routeRequiresGuU2ToObservedElectroweakEmbedding = true;
const bool routeRequiresGuWzSourceRows = true;
const bool routeRequiresGuWeakMixingAngleSource = true;
const bool routeRequiresGuObservedFieldExtraction = true;
const bool routeRequiresGuHiggsScalarSourceOperator = true;
const bool routeRequiresGuHiggsSelfCouplingSource = true;
const bool routeRequiresTargetIndependentVevOrMassScale = true;
const bool routeRequiresGeVUnitNormalization = true;

const bool routeProvidesGuLocalEguchiHansonMap = false;
const bool routeProvidesGuU2ToObservedElectroweakEmbedding = false;
const bool routeProvidesGuWzSourceRows = false;
const bool routeProvidesGuWeakMixingAngleSource = false;
const bool routeProvidesGuObservedFieldExtraction = false;
const bool routeProvidesGuHiggsScalarSourceOperator = false;
const bool routeProvidesGuHiggsSelfCouplingSource = false;
const bool routeProvidesTargetIndependentVevOrMassScale = false;
const bool routeProvidesGeVUnitNormalization = false;
const bool routePromotesWzMasses = false;
const bool routePromotesHiggsMass = false;
const bool routeCompletesBosonPredictions = false;
const bool canFillPhase201WzContract = false;
const bool canFillPhase201HiggsContract = false;
const bool canFillPhase256ObservedFieldExtractionContract = false;

var phase201AllRequiredLineagesPromotable = JsonBool(phase201.RootElement, "allRequiredLineagesPromotable") is true;
var existingEvidenceFound = JsonBool(phase213.RootElement, "existingEvidenceFound") is true;
var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;
var observedFieldExtractionFilledRequiredFieldCount = JsonInt(phase256.RootElement, "filledRequiredFieldCount") ?? -1;
var observedFieldExtractionContractPromotable = JsonBool(phase256.RootElement, "observedFieldExtractionContractPromotable") is true;
var su21RoutePromotesWzMasses = JsonBool(phase334.RootElement, "su21RoutePromotesWzMasses") is true;
var su21RoutePromotesHiggsMass = JsonBool(phase334.RootElement, "su21RoutePromotesHiggsMass") is true;
var octonionRoutePromotesWzMasses = JsonBool(phase337.RootElement, "octonionRoutePromotesWzMasses") is true;
var octonionRoutePromotesHiggsMass = JsonBool(phase337.RootElement, "octonionRoutePromotesHiggsMass") is true;
var weakHyperchargeRoutePromotesWzMasses = JsonBool(phase351.RootElement, "routePromotesWzMasses") is true;
var weakHyperchargeRoutePromotesHiggsMass = JsonBool(phase351.RootElement, "routePromotesHiggsMass") is true;
var diracLichnerowiczRoutePromotesWzMasses = JsonBool(phase355.RootElement, "routePromotesWzMasses") is true;
var diracLichnerowiczRoutePromotesHiggsMass = JsonBool(phase355.RootElement, "routePromotesHiggsMass") is true;

var sourceRows = new[]
{
    new SourceRow(
        "arxiv-hep-th-0702177-eguchi-hanson-substandard-higgs",
        substandardHiggsArxiv,
        "Eguchi-Hanson U(2) substandard Higgs relation",
        "Interprets a leptonic electroweak/Higgs U(2) sector geometrically through the Eguchi-Hanson metric and reports m(H)=m(W)/sqrt(sin(thetaW))=115.3 GeV.",
        "Direct geometric Higgs/W-angle relation lead, but it imports the W mass and weak angle, misses the observed 125 GeV Higgs scale, and supplies no GU-local W/Z/H source rows.")
};

var checks = new[]
{
    new Check(
        "eguchi-hanson-substandard-primary-source-reviewed",
        eguchiHansonSubstandardLeadPresent
            && eguchiHansonSubstandardPrimarySourceReviewed
            && eguchiHansonSubstandardRouteExternalToGu
            && sourceRows.Length == 1,
        $"lead={eguchiHansonSubstandardLeadPresent}; reviewed={eguchiHansonSubstandardPrimarySourceReviewed}; externalToGu={eguchiHansonSubstandardRouteExternalToGu}; sourceRows={sourceRows.Length}"),
    new Check(
        "eguchi-hanson-u2-geometric-route-captured",
        routeUsesEguchiHansonMetric
            && routeProvidesGeometricAlgebraicU2Interpretation
            && routeScopeLeptonsElectroweakBosonsAndHiggs
            && routeExcludesChromodynamicsOfQuarks
            && routeProvidesHiggsFromWAndWeakAngleFormula,
        $"eguchiHanson={routeUsesEguchiHansonMetric}; u2={routeProvidesGeometricAlgebraicU2Interpretation}; scope={routeScopeLeptonsElectroweakBosonsAndHiggs}; excludesQcd={routeExcludesChromodynamicsOfQuarks}; higgsFormula={routeProvidesHiggsFromWAndWeakAngleFormula}"),
    new Check(
        "external-input-and-observed-conflict-boundary-captured",
        routeUsesObservedWMassInput
            && routeUsesObservedWeinbergAngleInput
            && routeDoesNotPredictWzAbsoluteMasses
            && routeDoesNotPredictWeakMixingAngle
            && routeDoesNotProvideObservedHiggsExtraction
            && routeHiggsPredictionConflictsWithObserved125
            && substandardPredictedHiggsMassGeV == 115.3
            && absoluteHiggsShortfallGeV > 9.0,
        $"wInput={routeUsesObservedWMassInput}; weakAngleInput={routeUsesObservedWeinbergAngleInput}; wzPrediction={routeDoesNotPredictWzAbsoluteMasses}; weakAngleSource={routeDoesNotPredictWeakMixingAngle}; observedExtraction={routeDoesNotProvideObservedHiggsExtraction}; conflicts125={routeHiggsPredictionConflictsWithObserved125}; predictedHiggsGeV={substandardPredictedHiggsMassGeV}; shortfallGeV={absoluteHiggsShortfallGeV}"),
    new Check(
        "adjacent-algebraic-geometric-routes-remain-nonpromotional",
        !su21RoutePromotesWzMasses
            && !su21RoutePromotesHiggsMass
            && !octonionRoutePromotesWzMasses
            && !octonionRoutePromotesHiggsMass
            && !weakHyperchargeRoutePromotesWzMasses
            && !weakHyperchargeRoutePromotesHiggsMass
            && !diracLichnerowiczRoutePromotesWzMasses
            && !diracLichnerowiczRoutePromotesHiggsMass,
        $"su21PromotesWz={su21RoutePromotesWzMasses}; su21PromotesHiggs={su21RoutePromotesHiggsMass}; octonionPromotesWz={octonionRoutePromotesWzMasses}; octonionPromotesHiggs={octonionRoutePromotesHiggsMass}; weakHyperchargePromotesWz={weakHyperchargeRoutePromotesWzMasses}; weakHyperchargePromotesHiggs={weakHyperchargeRoutePromotesHiggsMass}; diracPromotesWz={diracLichnerowiczRoutePromotesWzMasses}; diracPromotesHiggs={diracLichnerowiczRoutePromotesHiggsMass}"),
    new Check(
        "external-inputs-required-before-promotion",
        routeRequiresExternalSubstandardModel
            && routeRequiresExternalU2GeometricInterpretation
            && routeRequiresObservedWMass
            && routeRequiresObservedWeakMixingAngle
            && routeRequiresElectroweakInputNormalization
            && routeRequiresObservedMassComparison
            && routeRequiresExtensionBeyondLeptonicElectroweakSector,
        $"externalModel={routeRequiresExternalSubstandardModel}; u2Geometry={routeRequiresExternalU2GeometricInterpretation}; wMass={routeRequiresObservedWMass}; weakAngle={routeRequiresObservedWeakMixingAngle}; normalization={routeRequiresElectroweakInputNormalization}; observedComparison={routeRequiresObservedMassComparison}; qcdExtension={routeRequiresExtensionBeyondLeptonicElectroweakSector}"),
    new Check(
        "route-does-not-fill-gu-contracts",
        !routeProvidesGuLocalEguchiHansonMap
            && !routeProvidesGuU2ToObservedElectroweakEmbedding
            && !routeProvidesGuWzSourceRows
            && !routeProvidesGuWeakMixingAngleSource
            && !routeProvidesGuObservedFieldExtraction
            && !routeProvidesGuHiggsScalarSourceOperator
            && !routeProvidesGuHiggsSelfCouplingSource
            && !routeProvidesTargetIndependentVevOrMassScale
            && !routeProvidesGeVUnitNormalization
            && !routePromotesWzMasses
            && !routePromotesHiggsMass
            && !routeCompletesBosonPredictions
            && !canFillPhase201WzContract
            && !canFillPhase201HiggsContract
            && !canFillPhase256ObservedFieldExtractionContract,
        $"guEguchiHansonMap={routeProvidesGuLocalEguchiHansonMap}; guU2Embedding={routeProvidesGuU2ToObservedElectroweakEmbedding}; guWzRows={routeProvidesGuWzSourceRows}; weakAngle={routeProvidesGuWeakMixingAngleSource}; observed={routeProvidesGuObservedFieldExtraction}; higgsOperator={routeProvidesGuHiggsScalarSourceOperator}; selfCoupling={routeProvidesGuHiggsSelfCouplingSource}; scale={routeProvidesTargetIndependentVevOrMassScale}; gev={routeProvidesGeVUnitNormalization}; promotesWz={routePromotesWzMasses}; promotesHiggs={routePromotesHiggsMass}; completes={routeCompletesBosonPredictions}"),
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

var eguchiHansonSubstandardHiggsSourceAuditPassed = checks.All(check => check.Passed)
    && eguchiHansonSubstandardHiggsSourceAuditPassedExpected
    && !routePromotesWzMasses
    && !routePromotesHiggsMass
    && !routeCompletesBosonPredictions
    && !canFillPhase201WzContract
    && !canFillPhase201HiggsContract
    && !canFillPhase256ObservedFieldExtractionContract;

var terminalStatus = eguchiHansonSubstandardHiggsSourceAuditPassed
    ? "eguchi-hanson-substandard-higgs-source-audit-imported-w-angle-relation-not-gu-source"
    : "eguchi-hanson-substandard-higgs-source-audit-review-required";

var result = new
{
    phaseId = "phase356-eguchi-hanson-substandard-higgs-source-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    eguchiHansonSubstandardHiggsSourceAuditPassed,
    eguchiHansonSubstandardLeadPresent,
    eguchiHansonSubstandardPrimarySourceReviewed,
    eguchiHansonSubstandardRouteExternalToGu,
    substandardHiggsDoi,
    routeUsesEguchiHansonMetric,
    routeProvidesGeometricAlgebraicU2Interpretation,
    routeScopeLeptonsElectroweakBosonsAndHiggs,
    routeExcludesChromodynamicsOfQuarks,
    routeProvidesHiggsFromWAndWeakAngleFormula,
    routeUsesObservedWMassInput,
    routeUsesObservedWeinbergAngleInput,
    routeDoesNotPredictWzAbsoluteMasses,
    routeDoesNotPredictWeakMixingAngle,
    routeDoesNotProvideObservedHiggsExtraction,
    routeHiggsPredictionConflictsWithObserved125,
    substandardHiggsLatestArxivVersion,
    substandardHiggsLatestRevisionYear,
    substandardPredictedHiggsMassGeV,
    observedHiggsReferenceGeV,
    absoluteHiggsShortfallGeV,
    relativeHiggsShortfall,
    routeRequiresExternalSubstandardModel,
    routeRequiresExternalU2GeometricInterpretation,
    routeRequiresObservedWMass,
    routeRequiresObservedWeakMixingAngle,
    routeRequiresElectroweakInputNormalization,
    routeRequiresObservedMassComparison,
    routeRequiresExtensionBeyondLeptonicElectroweakSector,
    routeRequiresGuLocalEguchiHansonMap,
    routeRequiresGuU2ToObservedElectroweakEmbedding,
    routeRequiresGuWzSourceRows,
    routeRequiresGuWeakMixingAngleSource,
    routeRequiresGuObservedFieldExtraction,
    routeRequiresGuHiggsScalarSourceOperator,
    routeRequiresGuHiggsSelfCouplingSource,
    routeRequiresTargetIndependentVevOrMassScale,
    routeRequiresGeVUnitNormalization,
    routeProvidesGuLocalEguchiHansonMap,
    routeProvidesGuU2ToObservedElectroweakEmbedding,
    routeProvidesGuWzSourceRows,
    routeProvidesGuWeakMixingAngleSource,
    routeProvidesGuObservedFieldExtraction,
    routeProvidesGuHiggsScalarSourceOperator,
    routeProvidesGuHiggsSelfCouplingSource,
    routeProvidesTargetIndependentVevOrMassScale,
    routeProvidesGeVUnitNormalization,
    routePromotesWzMasses,
    routePromotesHiggsMass,
    routeCompletesBosonPredictions,
    canFillPhase201WzContract,
    canFillPhase201HiggsContract,
    canFillPhase256ObservedFieldExtractionContract,
    sourceRows,
    checks,
    adjacentRouteBoundary = new
    {
        su21RoutePromotesWzMasses,
        su21RoutePromotesHiggsMass,
        octonionRoutePromotesWzMasses,
        octonionRoutePromotesHiggsMass,
        weakHyperchargeRoutePromotesWzMasses,
        weakHyperchargeRoutePromotesHiggsMass,
        diracLichnerowiczRoutePromotesWzMasses,
        diracLichnerowiczRoutePromotesHiggsMass
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
    decision = "Do not promote W/Z or Higgs physical masses from the Eguchi-Hanson Substandard Theory route. The source is a direct geometric/algebraic U(2) Higgs relation lead, but its Higgs formula imports the W mass and Weinberg angle, does not predict W/Z absolute masses or the weak angle, conflicts with the observed 125 GeV Higgs mass, excludes QCD/quark-sector completion, and supplies no GU-local Eguchi-Hanson map, observed-field extraction, W/Z source rows, Higgs scalar-source/self-coupling lineage, target-independent scale, or GeV normalization.",
    nextRequiredArtifact = new[]
    {
        "A GU-local map from Shiab/observer-sector geometry to any Eguchi-Hanson or U(2) electroweak structure before using this route as more than an analogy.",
        "Independent GU W/Z rows and a weak-angle source; the Higgs formula cannot import m(W) and thetaW.",
        "A corrected target-independent Higgs scalar-source/self-coupling or excitation source compatible with the observed 125 GeV Higgs scale.",
        "Observed photon/W/Z/H extraction and GeV unit normalization before any physical mass promotion."
    }
};

var fullPath = Path.Combine(outputDir, "eguchi_hanson_substandard_higgs_source_audit.json");
var summaryPath = Path.Combine(outputDir, "eguchi_hanson_substandard_higgs_source_audit_summary.json");
var jsonOptions = new JsonSerializerOptions { WriteIndented = true };

File.WriteAllText(fullPath, JsonSerializer.Serialize(result, jsonOptions));
File.WriteAllText(summaryPath, JsonSerializer.Serialize(new
{
    result.phaseId,
    result.terminalStatus,
    result.eguchiHansonSubstandardHiggsSourceAuditPassed,
    result.eguchiHansonSubstandardLeadPresent,
    result.eguchiHansonSubstandardPrimarySourceReviewed,
    result.eguchiHansonSubstandardRouteExternalToGu,
    result.routeUsesEguchiHansonMetric,
    result.routeProvidesGeometricAlgebraicU2Interpretation,
    result.routeScopeLeptonsElectroweakBosonsAndHiggs,
    result.routeExcludesChromodynamicsOfQuarks,
    result.routeProvidesHiggsFromWAndWeakAngleFormula,
    result.routeUsesObservedWMassInput,
    result.routeUsesObservedWeinbergAngleInput,
    result.routeDoesNotPredictWzAbsoluteMasses,
    result.routeDoesNotPredictWeakMixingAngle,
    result.routeDoesNotProvideObservedHiggsExtraction,
    result.routeHiggsPredictionConflictsWithObserved125,
    result.substandardHiggsLatestArxivVersion,
    result.substandardHiggsLatestRevisionYear,
    result.substandardPredictedHiggsMassGeV,
    result.observedHiggsReferenceGeV,
    result.absoluteHiggsShortfallGeV,
    result.relativeHiggsShortfall,
    result.routeRequiresExternalSubstandardModel,
    result.routeRequiresExternalU2GeometricInterpretation,
    result.routeRequiresObservedWMass,
    result.routeRequiresObservedWeakMixingAngle,
    result.routeRequiresElectroweakInputNormalization,
    result.routeRequiresObservedMassComparison,
    result.routeRequiresExtensionBeyondLeptonicElectroweakSector,
    result.routeRequiresGuLocalEguchiHansonMap,
    result.routeRequiresGuU2ToObservedElectroweakEmbedding,
    result.routeRequiresGuWzSourceRows,
    result.routeRequiresGuWeakMixingAngleSource,
    result.routeRequiresGuObservedFieldExtraction,
    result.routeRequiresGuHiggsScalarSourceOperator,
    result.routeRequiresGuHiggsSelfCouplingSource,
    result.routeRequiresTargetIndependentVevOrMassScale,
    result.routeRequiresGeVUnitNormalization,
    result.routeProvidesGuLocalEguchiHansonMap,
    result.routeProvidesGuU2ToObservedElectroweakEmbedding,
    result.routeProvidesGuWzSourceRows,
    result.routeProvidesGuWeakMixingAngleSource,
    result.routeProvidesGuObservedFieldExtraction,
    result.routeProvidesGuHiggsScalarSourceOperator,
    result.routeProvidesGuHiggsSelfCouplingSource,
    result.routeProvidesTargetIndependentVevOrMassScale,
    result.routeProvidesGeVUnitNormalization,
    result.routePromotesWzMasses,
    result.routePromotesHiggsMass,
    result.routeCompletesBosonPredictions,
    result.canFillPhase201WzContract,
    result.canFillPhase201HiggsContract,
    result.canFillPhase256ObservedFieldExtractionContract,
    sourceRowCount = sourceRows.Length,
    result.adjacentRouteBoundary,
    result.contractImpact,
    result.decision,
    result.nextRequiredArtifact
}, jsonOptions));

Console.WriteLine(terminalStatus);
Console.WriteLine($"eguchiHansonSubstandardHiggsSourceAuditPassed={eguchiHansonSubstandardHiggsSourceAuditPassed}");
Console.WriteLine($"routeUsesEguchiHansonMetric={routeUsesEguchiHansonMetric}");
Console.WriteLine($"routeProvidesHiggsFromWAndWeakAngleFormula={routeProvidesHiggsFromWAndWeakAngleFormula}");
Console.WriteLine($"substandardPredictedHiggsMassGeV={substandardPredictedHiggsMassGeV}");
Console.WriteLine($"routeHiggsPredictionConflictsWithObserved125={routeHiggsPredictionConflictsWithObserved125}");
Console.WriteLine($"routePromotesWzMasses={routePromotesWzMasses}");
Console.WriteLine($"routePromotesHiggsMass={routePromotesHiggsMass}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");

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

public sealed record SourceRow(string SourceId, string Url, string FindingKind, string Summary, string PredictionImpact);

public sealed record Check(string CheckId, bool Passed, string Detail);
