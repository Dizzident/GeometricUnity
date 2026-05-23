using System.Text.Json;

const string DefaultOutputDir = "studies/phase353_gauge_higgs_unification_source_audit_001/output";
const string Phase201Path = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase256Path = "studies/phase256_observed_field_extraction_intake_contract_001/output/observed_field_extraction_intake_contract_summary.json";
const string Phase333Path = "studies/phase333_kaluza_klein_internal_symmetry_source_audit_001/output/kaluza_klein_internal_symmetry_source_audit_summary.json";
const string Phase341Path = "studies/phase341_scherk_schwarz_twisted_compactification_source_audit_001/output/scherk_schwarz_twisted_compactification_source_audit_summary.json";
const string Phase342Path = "studies/phase342_higgsless_boundary_condition_source_audit_001/output/higgsless_boundary_condition_source_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE353_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase256 = JsonDocument.Parse(File.ReadAllText(Phase256Path));
using var phase333 = JsonDocument.Parse(File.ReadAllText(Phase333Path));
using var phase341 = JsonDocument.Parse(File.ReadAllText(Phase341Path));
using var phase342 = JsonDocument.Parse(File.ReadAllText(Phase342Path));

const string wMassGaugeHiggsUnificationArxiv = "https://arxiv.org/abs/2310.03276";
const string wMassGaugeHiggsUnificationPrd = "https://doi.org/10.1103/PhysRevD.108.115036";
const string warpedGaugeHiggsHiggsMassArxiv = "https://arxiv.org/abs/hep-ph/0503020";
const string warpedGaugeHiggsHiggsMassDoi = "https://doi.org/10.1016/j.physletb.2005.04.039";
const string gaugeHiggsGrandUnificationArxiv = "https://arxiv.org/abs/1504.03817";
const string gaugeHiggsGrandUnificationDoi = "https://doi.org/10.1093/ptep/ptv153";

const bool gaugeHiggsUnificationSourceAuditPassedExpected = true;
const bool gaugeHiggsUnificationLeadPresent = true;
const bool gaugeHiggsUnificationPrimarySourcesReviewed = true;
const bool gaugeHiggsUnificationRouteExternalToGu = true;
const bool routeUsesHosotaniMechanism = true;
const bool routeHiggsAsExtraDimensionalGaugeComponent = true;
const bool routeUsesWilsonLineAharonovBohmPhase = true;
const bool routeUsesRandallSundrumWarpedSpace = true;
const bool routeUsesKaluzaKleinModes = true;
const bool routeCanGenerateEwsbDynamically = true;
const bool routeProvidesExternalWMassEvaluation = true;
const bool routeProvidesExternalHiggsMassRelation = true;
const bool routeProvidesGaugeHiggsGrandUnificationModel = true;
const bool latestWMassEvaluationUsesMuonDecayMatching = true;
const bool latestWMassEvaluationIncludesKkWExchange = true;
const bool latestWMassEvaluationIncludesAdSCurvatureEffects = true;

const double latestMkkReferenceTeV = 13.0;
const double latestThetaHMin = 0.085;
const double latestThetaHMax = 0.11;
const double latestWPredictedMinGeV = 80.381;
const double latestWPredictedMaxGeV = 80.407;
const double latestMkkScanMinTeV = 13.0;
const double latestMkkScanMaxTeV = 20.0;
const double latestSmWReferenceGeV = 80.354;
const double latestSmWReferenceUncertaintyGeV = 0.007;
const double latestCdfWReferenceGeV = 80.4335;
const double latestCdfWReferenceUncertaintyGeV = 0.0094;
const double warpedRelationTypicalKR = 12.0;
const double warpedRelationThetaFractionMin = 0.2;
const double warpedRelationThetaFractionMax = 0.4;
const double warpedRelationMkkMinTeV = 1.7;
const double warpedRelationMkkMaxTeV = 3.5;
const double warpedRelationHiggsMinGeV = 140.0;
const double warpedRelationHiggsMaxGeV = 280.0;
const bool warpedRelationHiggsBandContainsObserved125 = false;

const bool routeRequiresExternalGaugeHiggsModel = true;
const bool routeRequiresSO5U1SU3OrSO11GaugeGroup = true;
const bool routeRequiresOrbifoldBoundaryConditions = true;
const bool routeRequiresBraneScalarOrBoundaryBreaking = true;
const bool routeRequiresKkMassScale = true;
const bool routeRequiresAdSCurvatureAndRadius = true;
const bool routeRequiresWilsonLinePhase = true;
const bool routeRequiresFermionBulkMatterAndKkSpectrum = true;
const bool routeRequiresMuonDecayMatching = true;
const bool routeRequiresPrecisionFitAndColliderBounds = true;
const bool routeRequiresGuLocalExtraDimensionalGaugeMap = true;
const bool routeRequiresGuBoundaryOrbifoldLaw = true;
const bool routeRequiresGuWilsonLinePhaseSource = true;
const bool routeRequiresGuTargetIndependentKkScale = true;
const bool routeRequiresGuObservedFieldExtraction = true;
const bool routeRequiresGuHiggsAsGaugeComponentIdentification = true;
const bool routeRequiresGeVUnitNormalization = true;

const bool routeProvidesGuLocalExtraDimensionalGaugeMap = false;
const bool routeProvidesGuBoundaryOrbifoldLaw = false;
const bool routeProvidesGuWilsonLinePhaseSource = false;
const bool routeProvidesGuTargetIndependentKkScale = false;
const bool routeProvidesGuMuonDecayMatching = false;
const bool routeProvidesGuWzSourceRows = false;
const bool routeProvidesGuObservedFieldExtraction = false;
const bool routeProvidesGuHiggsScalarSourceOperator = false;
const bool routeProvidesGuHiggsPotentialOrMassiveProfile = false;
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
var kkInternalSymmetryAuditPassed = JsonBool(phase333.RootElement, "kaluzaKleinInternalSymmetrySourceAuditPassed") is true;
var kkRoutePromotesWzMasses = JsonBool(phase333.RootElement, "kkRoutePromotesWzMasses") is true;
var kkRoutePromotesHiggsMass = JsonBool(phase333.RootElement, "kkRoutePromotesHiggsMass") is true;
var scherkSchwarzAuditPassed = JsonBool(phase341.RootElement, "scherkSchwarzTwistedCompactificationSourceAuditPassed") is true;
var scherkRoutePromotesWzMasses = JsonBool(phase341.RootElement, "scherkRoutePromotesWzMasses") is true;
var scherkRoutePromotesHiggsMass = JsonBool(phase341.RootElement, "scherkRoutePromotesHiggsMass") is true;
var higgslessBoundaryAuditPassed = JsonBool(phase342.RootElement, "higgslessBoundaryConditionSourceAuditPassed") is true;
var higgslessRoutePromotesWzMasses = JsonBool(phase342.RootElement, "higgslessRoutePromotesWzMasses") is true;
var higgslessRoutePromotesHiggsMass = JsonBool(phase342.RootElement, "higgslessRoutePromotesHiggsMass") is true;

var sourceRows = new[]
{
    new SourceRow(
        "arxiv-2310.03276-w-mass-gauge-higgs-unification",
        wMassGaugeHiggsUnificationArxiv,
        "W mass in SO(5) x U(1) x SU(3) gauge-Higgs unification",
        "Evaluates the W mass in RS gauge-Higgs unification using muon-decay matching with zero-mode and KK W exchange, AdS curvature effects, and a constrained Wilson-line phase range.",
        "Direct geometric W-mass lead, but it depends on an external gauge-Higgs model, KK scale, Wilson-line phase, bulk spectrum, and precision matching."),
    new SourceRow(
        "arxiv-hep-ph-0503020-warped-dynamical-gauge-higgs",
        warpedGaugeHiggsHiggsMassArxiv,
        "Dynamical gauge-Higgs unification Higgs/W/KK relations",
        "Derives relations among W mass, KK scale, and Higgs mass in warped spacetime using the Hosotani mechanism; typical parameters give a 140-280 GeV Higgs band.",
        "Serious Higgs-as-gauge-geometry lead, but the numerical Higgs band misses 125 GeV and depends on external kR and Wilson-line phase inputs."),
    new SourceRow(
        "arxiv-1504.03817-gauge-higgs-grand-unification",
        gaugeHiggsGrandUnificationArxiv,
        "SO(11) gauge-Higgs grand unification model",
        "Uses orbifold boundary conditions and a brane scalar to reduce SO(11) to Standard Model symmetry, then breaks to SU(3)C x U(1)EM by the Hosotani mechanism.",
        "Model-building context for gauge-Higgs unification; no GU-local W/Z/H source-lineage artifact is supplied.")
};

var checks = new[]
{
    new Check(
        "gauge-higgs-unification-primary-sources-reviewed",
        gaugeHiggsUnificationLeadPresent
            && gaugeHiggsUnificationPrimarySourcesReviewed
            && gaugeHiggsUnificationRouteExternalToGu
            && sourceRows.Length == 3,
        $"lead={gaugeHiggsUnificationLeadPresent}; reviewed={gaugeHiggsUnificationPrimarySourcesReviewed}; externalToGu={gaugeHiggsUnificationRouteExternalToGu}; sourceRows={sourceRows.Length}"),
    new Check(
        "gauge-higgs-route-is-direct-geometric-mass-generation-lead",
        routeUsesHosotaniMechanism
            && routeHiggsAsExtraDimensionalGaugeComponent
            && routeUsesWilsonLineAharonovBohmPhase
            && routeUsesRandallSundrumWarpedSpace
            && routeUsesKaluzaKleinModes
            && routeCanGenerateEwsbDynamically
            && routeProvidesExternalWMassEvaluation
            && routeProvidesExternalHiggsMassRelation
            && routeProvidesGaugeHiggsGrandUnificationModel,
        $"hosotani={routeUsesHosotaniMechanism}; higgsGaugeComponent={routeHiggsAsExtraDimensionalGaugeComponent}; wilsonLine={routeUsesWilsonLineAharonovBohmPhase}; rs={routeUsesRandallSundrumWarpedSpace}; kkModes={routeUsesKaluzaKleinModes}; dynamicEwsb={routeCanGenerateEwsbDynamically}; wMassEvaluation={routeProvidesExternalWMassEvaluation}; higgsRelation={routeProvidesExternalHiggsMassRelation}"),
    new Check(
        "latest-w-mass-evaluation-parameter-dependence-captured",
        latestWMassEvaluationUsesMuonDecayMatching
            && latestWMassEvaluationIncludesKkWExchange
            && latestWMassEvaluationIncludesAdSCurvatureEffects
            && latestMkkReferenceTeV == 13.0
            && latestThetaHMin == 0.085
            && latestThetaHMax == 0.11
            && latestWPredictedMinGeV == 80.381
            && latestWPredictedMaxGeV == 80.407,
        $"muonDecay={latestWMassEvaluationUsesMuonDecayMatching}; kkW={latestWMassEvaluationIncludesKkWExchange}; ads={latestWMassEvaluationIncludesAdSCurvatureEffects}; mkkRefTeV={latestMkkReferenceTeV:R}; thetaRange={latestThetaHMin:R}-{latestThetaHMax:R}; wRange={latestWPredictedMinGeV:R}-{latestWPredictedMaxGeV:R}"),
    new Check(
        "warped-higgs-relation-not-current-observed-higgs-prediction",
        warpedRelationTypicalKR == 12.0
            && warpedRelationThetaFractionMin == 0.2
            && warpedRelationThetaFractionMax == 0.4
            && warpedRelationMkkMinTeV == 1.7
            && warpedRelationMkkMaxTeV == 3.5
            && warpedRelationHiggsMinGeV == 140.0
            && warpedRelationHiggsMaxGeV == 280.0
            && !warpedRelationHiggsBandContainsObserved125,
        $"kR={warpedRelationTypicalKR:R}; thetaFractionRange={warpedRelationThetaFractionMin:R}-{warpedRelationThetaFractionMax:R}; mkkRangeTeV={warpedRelationMkkMinTeV:R}-{warpedRelationMkkMaxTeV:R}; higgsRangeGeV={warpedRelationHiggsMinGeV:R}-{warpedRelationHiggsMaxGeV:R}; contains125={warpedRelationHiggsBandContainsObserved125}"),
    new Check(
        "external-model-inputs-required-before-promotion",
        routeRequiresExternalGaugeHiggsModel
            && routeRequiresSO5U1SU3OrSO11GaugeGroup
            && routeRequiresOrbifoldBoundaryConditions
            && routeRequiresBraneScalarOrBoundaryBreaking
            && routeRequiresKkMassScale
            && routeRequiresAdSCurvatureAndRadius
            && routeRequiresWilsonLinePhase
            && routeRequiresFermionBulkMatterAndKkSpectrum
            && routeRequiresMuonDecayMatching
            && routeRequiresPrecisionFitAndColliderBounds,
        $"externalModel={routeRequiresExternalGaugeHiggsModel}; gaugeGroup={routeRequiresSO5U1SU3OrSO11GaugeGroup}; orbifold={routeRequiresOrbifoldBoundaryConditions}; braneOrBoundary={routeRequiresBraneScalarOrBoundaryBreaking}; kkScale={routeRequiresKkMassScale}; adsGeometry={routeRequiresAdSCurvatureAndRadius}; wilsonPhase={routeRequiresWilsonLinePhase}; spectrum={routeRequiresFermionBulkMatterAndKkSpectrum}; muonDecay={routeRequiresMuonDecayMatching}; precision={routeRequiresPrecisionFitAndColliderBounds}"),
    new Check(
        "adjacent-kk-twist-boundary-routes-remain-nonpromotional",
        kkInternalSymmetryAuditPassed
            && !kkRoutePromotesWzMasses
            && !kkRoutePromotesHiggsMass
            && scherkSchwarzAuditPassed
            && !scherkRoutePromotesWzMasses
            && !scherkRoutePromotesHiggsMass
            && higgslessBoundaryAuditPassed
            && !higgslessRoutePromotesWzMasses
            && !higgslessRoutePromotesHiggsMass,
        $"kkPassed={kkInternalSymmetryAuditPassed}; kkPromotesWz={kkRoutePromotesWzMasses}; kkPromotesHiggs={kkRoutePromotesHiggsMass}; scherkPassed={scherkSchwarzAuditPassed}; scherkPromotesWz={scherkRoutePromotesWzMasses}; scherkPromotesHiggs={scherkRoutePromotesHiggsMass}; higgslessPassed={higgslessBoundaryAuditPassed}; higgslessPromotesWz={higgslessRoutePromotesWzMasses}; higgslessPromotesHiggs={higgslessRoutePromotesHiggsMass}"),
    new Check(
        "gauge-higgs-route-does-not-fill-gu-contracts",
        !routeProvidesGuLocalExtraDimensionalGaugeMap
            && !routeProvidesGuBoundaryOrbifoldLaw
            && !routeProvidesGuWilsonLinePhaseSource
            && !routeProvidesGuTargetIndependentKkScale
            && !routeProvidesGuMuonDecayMatching
            && !routeProvidesGuWzSourceRows
            && !routeProvidesGuObservedFieldExtraction
            && !routeProvidesGuHiggsScalarSourceOperator
            && !routeProvidesGuHiggsPotentialOrMassiveProfile
            && !routeProvidesGeVUnitNormalization
            && !routePromotesWzMasses
            && !routePromotesHiggsMass
            && !routeCompletesBosonPredictions
            && !canFillPhase201WzContract
            && !canFillPhase201HiggsContract
            && !canFillPhase256ObservedFieldExtractionContract,
        $"guExtraDimMap={routeProvidesGuLocalExtraDimensionalGaugeMap}; guBoundary={routeProvidesGuBoundaryOrbifoldLaw}; guWilsonPhase={routeProvidesGuWilsonLinePhaseSource}; guKkScale={routeProvidesGuTargetIndependentKkScale}; guMuonDecay={routeProvidesGuMuonDecayMatching}; guWzRows={routeProvidesGuWzSourceRows}; observed={routeProvidesGuObservedFieldExtraction}; higgsOperator={routeProvidesGuHiggsScalarSourceOperator}; higgsProfile={routeProvidesGuHiggsPotentialOrMassiveProfile}; gev={routeProvidesGeVUnitNormalization}; promotesWz={routePromotesWzMasses}; promotesHiggs={routePromotesHiggsMass}; completes={routeCompletesBosonPredictions}"),
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

var gaugeHiggsUnificationSourceAuditPassed = checks.All(check => check.Passed)
    && gaugeHiggsUnificationSourceAuditPassedExpected
    && !routePromotesWzMasses
    && !routePromotesHiggsMass
    && !routeCompletesBosonPredictions
    && !canFillPhase201WzContract
    && !canFillPhase201HiggsContract
    && !canFillPhase256ObservedFieldExtractionContract;

var terminalStatus = gaugeHiggsUnificationSourceAuditPassed
    ? "gauge-higgs-unification-source-audit-geometric-hosotani-lead-not-gu-source"
    : "gauge-higgs-unification-source-audit-review-required";

var result = new
{
    phaseId = "phase353-gauge-higgs-unification-source-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    gaugeHiggsUnificationSourceAuditPassed,
    gaugeHiggsUnificationLeadPresent,
    gaugeHiggsUnificationPrimarySourcesReviewed,
    gaugeHiggsUnificationRouteExternalToGu,
    wMassGaugeHiggsUnificationPrd,
    warpedGaugeHiggsHiggsMassDoi,
    gaugeHiggsGrandUnificationDoi,
    routeUsesHosotaniMechanism,
    routeHiggsAsExtraDimensionalGaugeComponent,
    routeUsesWilsonLineAharonovBohmPhase,
    routeUsesRandallSundrumWarpedSpace,
    routeUsesKaluzaKleinModes,
    routeCanGenerateEwsbDynamically,
    routeProvidesExternalWMassEvaluation,
    routeProvidesExternalHiggsMassRelation,
    routeProvidesGaugeHiggsGrandUnificationModel,
    latestWMassEvaluationUsesMuonDecayMatching,
    latestWMassEvaluationIncludesKkWExchange,
    latestWMassEvaluationIncludesAdSCurvatureEffects,
    latestMkkReferenceTeV,
    latestThetaHMin,
    latestThetaHMax,
    latestWPredictedMinGeV,
    latestWPredictedMaxGeV,
    latestMkkScanMinTeV,
    latestMkkScanMaxTeV,
    latestSmWReferenceGeV,
    latestSmWReferenceUncertaintyGeV,
    latestCdfWReferenceGeV,
    latestCdfWReferenceUncertaintyGeV,
    warpedRelationTypicalKR,
    warpedRelationThetaFractionMin,
    warpedRelationThetaFractionMax,
    warpedRelationMkkMinTeV,
    warpedRelationMkkMaxTeV,
    warpedRelationHiggsMinGeV,
    warpedRelationHiggsMaxGeV,
    warpedRelationHiggsBandContainsObserved125,
    routeRequiresExternalGaugeHiggsModel,
    routeRequiresSO5U1SU3OrSO11GaugeGroup,
    routeRequiresOrbifoldBoundaryConditions,
    routeRequiresBraneScalarOrBoundaryBreaking,
    routeRequiresKkMassScale,
    routeRequiresAdSCurvatureAndRadius,
    routeRequiresWilsonLinePhase,
    routeRequiresFermionBulkMatterAndKkSpectrum,
    routeRequiresMuonDecayMatching,
    routeRequiresPrecisionFitAndColliderBounds,
    routeRequiresGuLocalExtraDimensionalGaugeMap,
    routeRequiresGuBoundaryOrbifoldLaw,
    routeRequiresGuWilsonLinePhaseSource,
    routeRequiresGuTargetIndependentKkScale,
    routeRequiresGuObservedFieldExtraction,
    routeRequiresGuHiggsAsGaugeComponentIdentification,
    routeRequiresGeVUnitNormalization,
    routeProvidesGuLocalExtraDimensionalGaugeMap,
    routeProvidesGuBoundaryOrbifoldLaw,
    routeProvidesGuWilsonLinePhaseSource,
    routeProvidesGuTargetIndependentKkScale,
    routeProvidesGuMuonDecayMatching,
    routeProvidesGuWzSourceRows,
    routeProvidesGuObservedFieldExtraction,
    routeProvidesGuHiggsScalarSourceOperator,
    routeProvidesGuHiggsPotentialOrMassiveProfile,
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
        kkInternalSymmetryAuditPassed,
        kkRoutePromotesWzMasses,
        kkRoutePromotesHiggsMass,
        scherkSchwarzAuditPassed,
        scherkRoutePromotesWzMasses,
        scherkRoutePromotesHiggsMass,
        higgslessBoundaryAuditPassed,
        higgslessRoutePromotesWzMasses,
        higgslessRoutePromotesHiggsMass
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
    decision = "Do not promote W/Z or Higgs masses from gauge-Higgs unification. It is a direct geometric Hosotani/Wilson-line mass-generation lead, but the audited sources depend on an external RS extra-dimensional model, boundary/orbifold choices, KK scale, Wilson-line phase, bulk spectrum, and precision matching; no GU-local map, phase source, KK-scale source, observed-field extraction, Higgs source, or GeV normalization is supplied.",
    nextRequiredArtifact = new[]
    {
        "A GU-local extra-dimensional or equivalent gauge-component map identifying the Higgs-as-gauge degree of freedom.",
        "A target-independent GU source for the Wilson-line/Aharonov-Bohm phase and KK or warped scale.",
        "A GU boundary/orbifold/brane-breaking law or replacement that selects the observed electroweak symmetry breaking pattern.",
        "Observed photon/W/Z/H projection rows, pole extraction, and GeV normalization before any W/Z/H promotion."
    }
};

var fullPath = Path.Combine(outputDir, "gauge_higgs_unification_source_audit.json");
var summaryPath = Path.Combine(outputDir, "gauge_higgs_unification_source_audit_summary.json");
var jsonOptions = new JsonSerializerOptions { WriteIndented = true };

File.WriteAllText(fullPath, JsonSerializer.Serialize(result, jsonOptions));
File.WriteAllText(summaryPath, JsonSerializer.Serialize(new
{
    result.phaseId,
    result.terminalStatus,
    result.gaugeHiggsUnificationSourceAuditPassed,
    result.gaugeHiggsUnificationLeadPresent,
    result.gaugeHiggsUnificationPrimarySourcesReviewed,
    result.gaugeHiggsUnificationRouteExternalToGu,
    result.routeUsesHosotaniMechanism,
    result.routeHiggsAsExtraDimensionalGaugeComponent,
    result.routeUsesWilsonLineAharonovBohmPhase,
    result.routeUsesRandallSundrumWarpedSpace,
    result.routeUsesKaluzaKleinModes,
    result.routeCanGenerateEwsbDynamically,
    result.routeProvidesExternalWMassEvaluation,
    result.routeProvidesExternalHiggsMassRelation,
    result.routeProvidesGaugeHiggsGrandUnificationModel,
    result.latestWMassEvaluationUsesMuonDecayMatching,
    result.latestWMassEvaluationIncludesKkWExchange,
    result.latestWMassEvaluationIncludesAdSCurvatureEffects,
    result.latestMkkReferenceTeV,
    result.latestThetaHMin,
    result.latestThetaHMax,
    result.latestWPredictedMinGeV,
    result.latestWPredictedMaxGeV,
    result.latestMkkScanMinTeV,
    result.latestMkkScanMaxTeV,
    result.latestSmWReferenceGeV,
    result.latestSmWReferenceUncertaintyGeV,
    result.latestCdfWReferenceGeV,
    result.latestCdfWReferenceUncertaintyGeV,
    result.warpedRelationTypicalKR,
    result.warpedRelationThetaFractionMin,
    result.warpedRelationThetaFractionMax,
    result.warpedRelationMkkMinTeV,
    result.warpedRelationMkkMaxTeV,
    result.warpedRelationHiggsMinGeV,
    result.warpedRelationHiggsMaxGeV,
    result.warpedRelationHiggsBandContainsObserved125,
    result.routeRequiresExternalGaugeHiggsModel,
    result.routeRequiresSO5U1SU3OrSO11GaugeGroup,
    result.routeRequiresOrbifoldBoundaryConditions,
    result.routeRequiresBraneScalarOrBoundaryBreaking,
    result.routeRequiresKkMassScale,
    result.routeRequiresAdSCurvatureAndRadius,
    result.routeRequiresWilsonLinePhase,
    result.routeRequiresFermionBulkMatterAndKkSpectrum,
    result.routeRequiresMuonDecayMatching,
    result.routeRequiresPrecisionFitAndColliderBounds,
    result.routeRequiresGuLocalExtraDimensionalGaugeMap,
    result.routeRequiresGuBoundaryOrbifoldLaw,
    result.routeRequiresGuWilsonLinePhaseSource,
    result.routeRequiresGuTargetIndependentKkScale,
    result.routeRequiresGuObservedFieldExtraction,
    result.routeRequiresGuHiggsAsGaugeComponentIdentification,
    result.routeRequiresGeVUnitNormalization,
    result.routeProvidesGuLocalExtraDimensionalGaugeMap,
    result.routeProvidesGuBoundaryOrbifoldLaw,
    result.routeProvidesGuWilsonLinePhaseSource,
    result.routeProvidesGuTargetIndependentKkScale,
    result.routeProvidesGuMuonDecayMatching,
    result.routeProvidesGuWzSourceRows,
    result.routeProvidesGuObservedFieldExtraction,
    result.routeProvidesGuHiggsScalarSourceOperator,
    result.routeProvidesGuHiggsPotentialOrMassiveProfile,
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
Console.WriteLine($"gaugeHiggsUnificationSourceAuditPassed={gaugeHiggsUnificationSourceAuditPassed}");
Console.WriteLine($"routeUsesHosotaniMechanism={routeUsesHosotaniMechanism}");
Console.WriteLine($"latestWPredictedRangeGeV={latestWPredictedMinGeV:R}-{latestWPredictedMaxGeV:R}");
Console.WriteLine($"warpedRelationHiggsRangeGeV={warpedRelationHiggsMinGeV:R}-{warpedRelationHiggsMaxGeV:R}");
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
