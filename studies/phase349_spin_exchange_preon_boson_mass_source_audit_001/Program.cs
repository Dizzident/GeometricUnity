using System.Text.Json;

const string DefaultOutputDir = "studies/phase349_spin_exchange_preon_boson_mass_source_audit_001/output";
const string Phase201Path = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase256Path = "studies/phase256_observed_field_extraction_intake_contract_001/output/observed_field_extraction_intake_contract_summary.json";
const string Phase317Path = "studies/phase317_electroweak_mass_matrix_bridge_source_audit_001/output/electroweak_mass_matrix_bridge_source_audit_summary.json";
const string Phase322Path = "studies/phase322_higgs_upsilon_scalar_source_boundary_audit_001/output/higgs_upsilon_scalar_source_boundary_audit_summary.json";
const string Phase346Path = "studies/phase346_nielsen_pole_mass_gauge_independence_source_audit_001/output/nielsen_pole_mass_gauge_independence_source_audit_summary.json";
const string Phase347Path = "studies/phase347_dispersive_electroweak_scale_mass_source_audit_001/output/dispersive_electroweak_scale_mass_source_audit_summary.json";
const string Phase348Path = "studies/phase348_right_handed_weak_coupling_source_audit_001/output/right_handed_weak_coupling_source_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE349_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase256 = JsonDocument.Parse(File.ReadAllText(Phase256Path));
using var phase317 = JsonDocument.Parse(File.ReadAllText(Phase317Path));
using var phase322 = JsonDocument.Parse(File.ReadAllText(Phase322Path));
using var phase346 = JsonDocument.Parse(File.ReadAllText(Phase346Path));
using var phase347 = JsonDocument.Parse(File.ReadAllText(Phase347Path));
using var phase348 = JsonDocument.Parse(File.ReadAllText(Phase348Path));

const string spinExchangePreonArxiv = "https://arxiv.org/abs/2410.13902";
const string spinExchangePreonDoi = "https://doi.org/10.48550/arXiv.2410.13902";
const string spinExchangePreonTitle = "Mass Prediction of the Weak and Higgs Bosons Using the Massless Spin-Exchange Preons Model: An Alternative to the Higgs Mechanism in Yang-Mills Theory";

const bool spinExchangePreonBosonMassSourceAuditPassedExpected = true;
const bool spinExchangePreonMassLeadPresent = true;
const bool spinExchangePreonPrimarySourceReviewed = true;
const bool spinExchangePreonRouteExternalToGu = true;
const bool preonRouteUsesMasslessPreonPairs = true;
const bool preonRouteUsesChiralPreonPairInternalDynamics = true;
const bool preonRouteUsesGellMannGeneratorStructure = true;
const bool preonRouteModelsPhotonAsIsospinSinglet = true;
const bool preonRouteModelsWeakBosonsAsTriplet = true;
const bool preonRouteClaimsNoAdjustableParameters = true;
const bool preonRouteClaimsWeakBosonMassRatio = true;
const bool preonRouteClaimsWeakMixingAngle = true;
const bool preonRouteClaimsDecayWidthRatio = true;
const bool preonRouteClaimsHiggsBosonMassRatio = true;
const bool preonRouteIntroducesCurlOperatorChirality = true;
const bool preonRouteUsesMobiusToriTopology = true;
const double reportedWeakBosonMassRatio = 0.87;
const double reportedWeakBosonMassRatioExperimentalComparison = 0.88;
const double reportedWeinbergAngleDegrees = 30.0;
const double reportedWeinbergAngleExperimentalComparisonDegrees = 29.0;
const double reportedDecayWidthRatio = 0.87;
const double reportedDecayWidthRatioExperimentalComparison = 0.84;
const bool higgsBosonMassRatioNumericValueRecoverableFromArxivHtml = false;
const string higgsBosonMassRatioArxivHtmlNote = "The arXiv abstract records a Higgs-boson ratio comparison, but the HTML text elides the predicted MathJax expression; this audit records the claim only and does not use a recovered numeric value.";

const bool routeRequiresExternalPreonModel = true;
const bool routeRequiresCompositeBindingDynamics = true;
const bool routeRequiresMappingPreonStatesToObservedBosons = true;
const bool routeRequiresTargetIndependentAbsoluteMassScaleOrInput = true;
const bool routeRequiresGuLocalPreonOrSpinExchangeSource = true;
const bool routeRequiresGuChiralPreonPairOperator = true;
const bool routeRequiresGuGellMannGeneratorEmbedding = true;
const bool routeRequiresGuWeakMixingAngleSource = true;
const bool routeRequiresGuGaugeCouplingNormalization = true;
const bool routeRequiresGuObservedFieldExtraction = true;
const bool routeRequiresGuIndependentWzSourceRows = true;
const bool routeRequiresGuHiggsScalarSourceOperator = true;
const bool routeRequiresGeVUnitNormalization = true;

const bool routeProvidesGuLocalPreonOrSpinExchangeSource = false;
const bool routeProvidesGuChiralPreonPairOperator = false;
const bool routeProvidesGuLocalWzTheorem = false;
const bool routeProvidesSeparateObservedWzRows = false;
const bool routeProvidesTargetIndependentVevOrMassScale = false;
const bool routeProvidesGuWeakMixingAngleSource = false;
const bool routeProvidesGuGaugeCouplingNormalization = false;
const bool routeProvidesObservedPhotonWzHiggsProjectionRows = false;
const bool routeProvidesGuObservedFieldExtractionContract = false;
const bool routeProvidesGuHiggsScalarSourceOperator = false;
const bool routeProvidesObservedHiggsMassFromGu = false;
const bool routeProvidesGeVUnitNormalization = false;
const bool routePromotesObservedFieldExtraction = false;
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
var smMassMatrixPromotesWzMasses = JsonBool(phase317.RootElement, "smMassMatrixPromotesWzMasses") is true;
var smMassMatrixPromotesHiggsMass = JsonBool(phase317.RootElement, "smMassMatrixPromotesHiggsMass") is true;
var higgsUpsilonRouteCompletesBosonPredictions = JsonBool(phase322.RootElement, "higgsUpsilonRouteCompletesBosonPredictions") is true;
var nielsenRouteCompletesBosonPredictions = JsonBool(phase346.RootElement, "nielsenRouteCompletesBosonPredictions") is true;
var dispersiveRouteCompletesBosonPredictions = JsonBool(phase347.RootElement, "dispersiveRouteCompletesBosonPredictions") is true;
var rightHandedWeakCouplingRouteCompletesBosonPredictions = JsonBool(phase348.RootElement, "routeCompletesBosonPredictions") is true;

var sourceRows = new[]
{
    new SourceRow(
        "arxiv-2410.13902-spin-exchange-preon-boson-mass",
        spinExchangePreonArxiv,
        spinExchangePreonTitle,
        "Uses massless chiral preon pairs, Gell-Mann generator structure, a curl-operator chirality mechanism, and Mobius-tori topology to describe photon, Z, W, and Higgs-sector mass ratios outside the Standard Model Higgs mechanism.",
        "Direct W/Z/H numerical-ratio lead, but it is an external preon/composite model and supplies no GU-local source rows, observed-field extraction, absolute scale, Higgs scalar-source lineage, or GeV unit normalization.")
};

var checks = new[]
{
    new Check(
        "spin-exchange-preon-primary-source-reviewed",
        spinExchangePreonMassLeadPresent
            && spinExchangePreonPrimarySourceReviewed
            && spinExchangePreonRouteExternalToGu
            && sourceRows.Length == 1,
        $"lead={spinExchangePreonMassLeadPresent}; reviewed={spinExchangePreonPrimarySourceReviewed}; externalToGu={spinExchangePreonRouteExternalToGu}; sourceRows={sourceRows.Length}"),
    new Check(
        "preon-model-ratio-claims-captured",
        preonRouteUsesMasslessPreonPairs
            && preonRouteUsesChiralPreonPairInternalDynamics
            && preonRouteUsesGellMannGeneratorStructure
            && preonRouteModelsPhotonAsIsospinSinglet
            && preonRouteModelsWeakBosonsAsTriplet
            && preonRouteClaimsNoAdjustableParameters
            && preonRouteClaimsWeakBosonMassRatio
            && preonRouteClaimsWeakMixingAngle
            && preonRouteClaimsDecayWidthRatio
            && preonRouteClaimsHiggsBosonMassRatio
            && preonRouteIntroducesCurlOperatorChirality
            && preonRouteUsesMobiusToriTopology
            && reportedWeakBosonMassRatio == 0.87
            && reportedWeakBosonMassRatioExperimentalComparison == 0.88
            && reportedWeinbergAngleDegrees == 30.0
            && reportedWeinbergAngleExperimentalComparisonDegrees == 29.0
            && reportedDecayWidthRatio == 0.87
            && reportedDecayWidthRatioExperimentalComparison == 0.84
            && !higgsBosonMassRatioNumericValueRecoverableFromArxivHtml,
        $"massRatio={reportedWeakBosonMassRatio}; massRatioExp={reportedWeakBosonMassRatioExperimentalComparison}; thetaW={reportedWeinbergAngleDegrees}; thetaWExp={reportedWeinbergAngleExperimentalComparisonDegrees}; widthRatio={reportedDecayWidthRatio}; widthRatioExp={reportedDecayWidthRatioExperimentalComparison}; higgsRatioClaim={preonRouteClaimsHiggsBosonMassRatio}; higgsRatioNumericFromHtml={higgsBosonMassRatioNumericValueRecoverableFromArxivHtml}"),
    new Check(
        "current-source-lineage-blockers-preserved",
        !phase201AllRequiredLineagesPromotable
            && !existingEvidenceFound
            && wzMissingFieldCount == 15
            && higgsMissingFieldCount == 14,
        $"phase201Promotable={phase201AllRequiredLineagesPromotable}; existingEvidence={existingEvidenceFound}; wzMissing={wzMissingFieldCount}; higgsMissing={higgsMissingFieldCount}"),
    new Check(
        "observed-field-contract-and-adjacent-routes-remain-nonpromotional",
        observedFieldExtractionFilledRequiredFieldCount == 0
            && !observedFieldExtractionContractPromotable
            && !smMassMatrixPromotesWzMasses
            && !smMassMatrixPromotesHiggsMass
            && !higgsUpsilonRouteCompletesBosonPredictions
            && !nielsenRouteCompletesBosonPredictions
            && !dispersiveRouteCompletesBosonPredictions
            && !rightHandedWeakCouplingRouteCompletesBosonPredictions,
        $"observedFilled={observedFieldExtractionFilledRequiredFieldCount}; observedPromotable={observedFieldExtractionContractPromotable}; smWz={smMassMatrixPromotesWzMasses}; smHiggs={smMassMatrixPromotesHiggsMass}; higgsUpsilonCompletes={higgsUpsilonRouteCompletesBosonPredictions}; nielsenCompletes={nielsenRouteCompletesBosonPredictions}; dispersiveCompletes={dispersiveRouteCompletesBosonPredictions}; rightHandedCompletes={rightHandedWeakCouplingRouteCompletesBosonPredictions}"),
    new Check(
        "preon-route-requires-missing-gu-source-data",
        routeRequiresExternalPreonModel
            && routeRequiresCompositeBindingDynamics
            && routeRequiresMappingPreonStatesToObservedBosons
            && routeRequiresTargetIndependentAbsoluteMassScaleOrInput
            && routeRequiresGuLocalPreonOrSpinExchangeSource
            && routeRequiresGuChiralPreonPairOperator
            && routeRequiresGuGellMannGeneratorEmbedding
            && routeRequiresGuWeakMixingAngleSource
            && routeRequiresGuGaugeCouplingNormalization
            && routeRequiresGuObservedFieldExtraction
            && routeRequiresGuIndependentWzSourceRows
            && routeRequiresGuHiggsScalarSourceOperator
            && routeRequiresGeVUnitNormalization,
        $"externalPreon={routeRequiresExternalPreonModel}; binding={routeRequiresCompositeBindingDynamics}; stateMap={routeRequiresMappingPreonStatesToObservedBosons}; scale={routeRequiresTargetIndependentAbsoluteMassScaleOrInput}; guPreon={routeRequiresGuLocalPreonOrSpinExchangeSource}; guOperator={routeRequiresGuChiralPreonPairOperator}; guGenerators={routeRequiresGuGellMannGeneratorEmbedding}; weakMixing={routeRequiresGuWeakMixingAngleSource}; coupling={routeRequiresGuGaugeCouplingNormalization}; observed={routeRequiresGuObservedFieldExtraction}; wzRows={routeRequiresGuIndependentWzSourceRows}; scalar={routeRequiresGuHiggsScalarSourceOperator}; gev={routeRequiresGeVUnitNormalization}"),
    new Check(
        "preon-route-does-not-fill-gu-contracts",
        !routeProvidesGuLocalPreonOrSpinExchangeSource
            && !routeProvidesGuChiralPreonPairOperator
            && !routeProvidesGuLocalWzTheorem
            && !routeProvidesSeparateObservedWzRows
            && !routeProvidesTargetIndependentVevOrMassScale
            && !routeProvidesGuWeakMixingAngleSource
            && !routeProvidesGuGaugeCouplingNormalization
            && !routeProvidesObservedPhotonWzHiggsProjectionRows
            && !routeProvidesGuObservedFieldExtractionContract
            && !routeProvidesGuHiggsScalarSourceOperator
            && !routeProvidesObservedHiggsMassFromGu
            && !routeProvidesGeVUnitNormalization
            && !routePromotesObservedFieldExtraction
            && !routePromotesWzMasses
            && !routePromotesHiggsMass
            && !routeCompletesBosonPredictions
            && !canFillPhase201WzContract
            && !canFillPhase201HiggsContract
            && !canFillPhase256ObservedFieldExtractionContract,
        $"guPreon={routeProvidesGuLocalPreonOrSpinExchangeSource}; guOperator={routeProvidesGuChiralPreonPairOperator}; guWzTheorem={routeProvidesGuLocalWzTheorem}; observedWzRows={routeProvidesSeparateObservedWzRows}; scale={routeProvidesTargetIndependentVevOrMassScale}; weakMixing={routeProvidesGuWeakMixingAngleSource}; coupling={routeProvidesGuGaugeCouplingNormalization}; observedRows={routeProvidesObservedPhotonWzHiggsProjectionRows}; observedContract={routeProvidesGuObservedFieldExtractionContract}; scalarSource={routeProvidesGuHiggsScalarSourceOperator}; higgsMass={routeProvidesObservedHiggsMassFromGu}; gev={routeProvidesGeVUnitNormalization}; promotesWz={routePromotesWzMasses}; promotesHiggs={routePromotesHiggsMass}; completes={routeCompletesBosonPredictions}")
};

var spinExchangePreonBosonMassSourceAuditPassed = checks.All(check => check.Passed)
    && spinExchangePreonBosonMassSourceAuditPassedExpected
    && !routePromotesObservedFieldExtraction
    && !routePromotesWzMasses
    && !routePromotesHiggsMass
    && !routeCompletesBosonPredictions
    && !canFillPhase201WzContract
    && !canFillPhase201HiggsContract
    && !canFillPhase256ObservedFieldExtractionContract;

var terminalStatus = spinExchangePreonBosonMassSourceAuditPassed
    ? "spin-exchange-preon-source-audit-external-ratio-model-not-gu-source"
    : "spin-exchange-preon-source-audit-review-required";

var result = new
{
    phaseId = "phase349-spin-exchange-preon-boson-mass-source-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    spinExchangePreonDoi,
    spinExchangePreonBosonMassSourceAuditPassed,
    spinExchangePreonMassLeadPresent,
    spinExchangePreonPrimarySourceReviewed,
    spinExchangePreonRouteExternalToGu,
    preonRouteUsesMasslessPreonPairs,
    preonRouteUsesChiralPreonPairInternalDynamics,
    preonRouteUsesGellMannGeneratorStructure,
    preonRouteModelsPhotonAsIsospinSinglet,
    preonRouteModelsWeakBosonsAsTriplet,
    preonRouteClaimsNoAdjustableParameters,
    preonRouteClaimsWeakBosonMassRatio,
    preonRouteClaimsWeakMixingAngle,
    preonRouteClaimsDecayWidthRatio,
    preonRouteClaimsHiggsBosonMassRatio,
    preonRouteIntroducesCurlOperatorChirality,
    preonRouteUsesMobiusToriTopology,
    reportedWeakBosonMassRatio,
    reportedWeakBosonMassRatioExperimentalComparison,
    reportedWeinbergAngleDegrees,
    reportedWeinbergAngleExperimentalComparisonDegrees,
    reportedDecayWidthRatio,
    reportedDecayWidthRatioExperimentalComparison,
    higgsBosonMassRatioNumericValueRecoverableFromArxivHtml,
    higgsBosonMassRatioArxivHtmlNote,
    routeRequiresExternalPreonModel,
    routeRequiresCompositeBindingDynamics,
    routeRequiresMappingPreonStatesToObservedBosons,
    routeRequiresTargetIndependentAbsoluteMassScaleOrInput,
    routeRequiresGuLocalPreonOrSpinExchangeSource,
    routeRequiresGuChiralPreonPairOperator,
    routeRequiresGuGellMannGeneratorEmbedding,
    routeRequiresGuWeakMixingAngleSource,
    routeRequiresGuGaugeCouplingNormalization,
    routeRequiresGuObservedFieldExtraction,
    routeRequiresGuIndependentWzSourceRows,
    routeRequiresGuHiggsScalarSourceOperator,
    routeRequiresGeVUnitNormalization,
    routeProvidesGuLocalPreonOrSpinExchangeSource,
    routeProvidesGuChiralPreonPairOperator,
    routeProvidesGuLocalWzTheorem,
    routeProvidesSeparateObservedWzRows,
    routeProvidesTargetIndependentVevOrMassScale,
    routeProvidesGuWeakMixingAngleSource,
    routeProvidesGuGaugeCouplingNormalization,
    routeProvidesObservedPhotonWzHiggsProjectionRows,
    routeProvidesGuObservedFieldExtractionContract,
    routeProvidesGuHiggsScalarSourceOperator,
    routeProvidesObservedHiggsMassFromGu,
    routeProvidesGeVUnitNormalization,
    routePromotesObservedFieldExtraction,
    routePromotesWzMasses,
    routePromotesHiggsMass,
    routeCompletesBosonPredictions,
    canFillPhase201WzContract,
    canFillPhase201HiggsContract,
    canFillPhase256ObservedFieldExtractionContract,
    sourceRows,
    checks,
    contractImpact = new
    {
        canFillPhase201WzContract,
        canFillPhase201HiggsContract,
        canFillPhase256ObservedFieldExtractionContract,
        wzMissingFieldCount,
        higgsMissingFieldCount,
        observedFieldExtractionFilledRequiredFieldCount
    },
    relatedBlockingEvidence = new
    {
        smMassMatrixPromotesWzMasses,
        smMassMatrixPromotesHiggsMass,
        higgsUpsilonRouteCompletesBosonPredictions,
        nielsenRouteCompletesBosonPredictions,
        dispersiveRouteCompletesBosonPredictions,
        rightHandedWeakCouplingRouteCompletesBosonPredictions
    },
    decision = "Do not promote W/Z or Higgs physical masses from the spin-exchange preon route. It is a direct external numerical-ratio lead for weak and Higgs bosons, but it imports an external preon/composite model and does not supply GU-local preon operators, Gell-Mann embedding, observed photon/W/Z/H projection rows, independent W/Z source rows, a Higgs scalar-source operator, an absolute GeV scale, or unit normalization.",
    nextRequiredArtifact = new[]
    {
        "A GU-native source for the chiral preon-pair or spin-exchange operator, or a proof that the preon degrees of freedom are GU fields.",
        "A GU embedding of the generator structure and curl/chirality operator that derives the observed photon, W, Z, and Higgs state map.",
        "Target-independent W and Z source rows with absolute mass-scale lineage, not only dimensionless ratios against external comparisons.",
        "A GU Higgs scalar-source/operator and a pole-mass extraction route for the observed Higgs state.",
        "GeV unit normalization and observed-field extraction before any physical W/Z/H mass comparison."
    }
};

var fullPath = Path.Combine(outputDir, "spin_exchange_preon_boson_mass_source_audit.json");
var summaryPath = Path.Combine(outputDir, "spin_exchange_preon_boson_mass_source_audit_summary.json");
var jsonOptions = new JsonSerializerOptions { WriteIndented = true };

File.WriteAllText(fullPath, JsonSerializer.Serialize(result, jsonOptions));
File.WriteAllText(summaryPath, JsonSerializer.Serialize(new
{
    result.phaseId,
    result.terminalStatus,
    result.spinExchangePreonDoi,
    result.spinExchangePreonBosonMassSourceAuditPassed,
    result.spinExchangePreonMassLeadPresent,
    result.spinExchangePreonPrimarySourceReviewed,
    result.spinExchangePreonRouteExternalToGu,
    result.preonRouteUsesMasslessPreonPairs,
    result.preonRouteUsesChiralPreonPairInternalDynamics,
    result.preonRouteUsesGellMannGeneratorStructure,
    result.preonRouteModelsPhotonAsIsospinSinglet,
    result.preonRouteModelsWeakBosonsAsTriplet,
    result.preonRouteClaimsNoAdjustableParameters,
    result.preonRouteClaimsWeakBosonMassRatio,
    result.preonRouteClaimsWeakMixingAngle,
    result.preonRouteClaimsDecayWidthRatio,
    result.preonRouteClaimsHiggsBosonMassRatio,
    result.preonRouteIntroducesCurlOperatorChirality,
    result.preonRouteUsesMobiusToriTopology,
    result.reportedWeakBosonMassRatio,
    result.reportedWeakBosonMassRatioExperimentalComparison,
    result.reportedWeinbergAngleDegrees,
    result.reportedWeinbergAngleExperimentalComparisonDegrees,
    result.reportedDecayWidthRatio,
    result.reportedDecayWidthRatioExperimentalComparison,
    result.higgsBosonMassRatioNumericValueRecoverableFromArxivHtml,
    result.higgsBosonMassRatioArxivHtmlNote,
    result.routeRequiresExternalPreonModel,
    result.routeRequiresCompositeBindingDynamics,
    result.routeRequiresMappingPreonStatesToObservedBosons,
    result.routeRequiresTargetIndependentAbsoluteMassScaleOrInput,
    result.routeRequiresGuLocalPreonOrSpinExchangeSource,
    result.routeRequiresGuChiralPreonPairOperator,
    result.routeRequiresGuGellMannGeneratorEmbedding,
    result.routeRequiresGuWeakMixingAngleSource,
    result.routeRequiresGuGaugeCouplingNormalization,
    result.routeRequiresGuObservedFieldExtraction,
    result.routeRequiresGuIndependentWzSourceRows,
    result.routeRequiresGuHiggsScalarSourceOperator,
    result.routeRequiresGeVUnitNormalization,
    result.routeProvidesGuLocalPreonOrSpinExchangeSource,
    result.routeProvidesGuChiralPreonPairOperator,
    result.routeProvidesGuLocalWzTheorem,
    result.routeProvidesSeparateObservedWzRows,
    result.routeProvidesTargetIndependentVevOrMassScale,
    result.routeProvidesGuWeakMixingAngleSource,
    result.routeProvidesGuGaugeCouplingNormalization,
    result.routeProvidesObservedPhotonWzHiggsProjectionRows,
    result.routeProvidesGuObservedFieldExtractionContract,
    result.routeProvidesGuHiggsScalarSourceOperator,
    result.routeProvidesObservedHiggsMassFromGu,
    result.routeProvidesGeVUnitNormalization,
    result.routePromotesObservedFieldExtraction,
    result.routePromotesWzMasses,
    result.routePromotesHiggsMass,
    result.routeCompletesBosonPredictions,
    result.canFillPhase201WzContract,
    result.canFillPhase201HiggsContract,
    result.canFillPhase256ObservedFieldExtractionContract,
    sourceRowCount = sourceRows.Length,
    result.contractImpact,
    result.relatedBlockingEvidence,
    result.decision,
    result.nextRequiredArtifact
}, jsonOptions));

Console.WriteLine(terminalStatus);
Console.WriteLine($"spinExchangePreonBosonMassSourceAuditPassed={spinExchangePreonBosonMassSourceAuditPassed}");
Console.WriteLine($"reportedWeakBosonMassRatio={reportedWeakBosonMassRatio}");
Console.WriteLine($"reportedWeinbergAngleDegrees={reportedWeinbergAngleDegrees}");
Console.WriteLine($"routePromotesWzMasses={routePromotesWzMasses}");
Console.WriteLine($"routePromotesHiggsMass={routePromotesHiggsMass}");
Console.WriteLine($"canFillPhase256ObservedFieldExtractionContract={canFillPhase256ObservedFieldExtractionContract}");

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
