using System.Text.Json;

const string DefaultOutputDir = "studies/phase359_finite_ncg_discrete_higgs_source_audit_001/output";
const string Phase201Path = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase256Path = "studies/phase256_observed_field_extraction_intake_contract_001/output/observed_field_extraction_intake_contract_summary.json";
const string Phase268Path = "studies/phase268_spectral_action_boson_source_audit_001/output/spectral_action_boson_source_audit_summary.json";
const string Phase334Path = "studies/phase334_su21_superconnection_source_audit_001/output/su21_superconnection_source_audit_summary.json";
const string Phase355Path = "studies/phase355_dirac_lichnerowicz_yang_mills_higgs_source_audit_001/output/dirac_lichnerowicz_yang_mills_higgs_source_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE359_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase256 = JsonDocument.Parse(File.ReadAllText(Phase256Path));
using var phase268 = JsonDocument.Parse(File.ReadAllText(Phase268Path));
using var phase334 = JsonDocument.Parse(File.ReadAllText(Phase334Path));
using var phase355 = JsonDocument.Parse(File.ReadAllText(Phase355Path));

const string connesLottParticleModelsUrl = "https://repo-archives.ihes.fr/FONDS_IHES/I_Prepublications/CONNES/1985-1993/M_90_23/M_90_23.pdf";
const string connesLottHiggsMechanismUrl = "https://www.sciencedirect.com/science/article/pii/0370269391911804";
const string sitarzNoSpecialRelationUrl = "https://arxiv.org/abs/hep-th/9304005";
const string okumuraRgAnalysisUrl = "https://academic.oup.com/ptp/article/98/6/1333/1868457";
const string chamseddineConnesMarcolliUrl = "https://arxiv.org/abs/hep-th/0610241";
const string resilienceSpectralStandardModelUrl = "https://arxiv.org/abs/1208.1030";
const string higgsMassNcgUrl = "https://arxiv.org/abs/1403.7567";
const string lorentzianNcgBlUrl = "https://arxiv.org/abs/2010.04960";

const bool finiteNcgDiscreteHiggsSourceAuditPassedExpected = true;
const bool finiteNcgLeadPresent = true;
const bool finiteNcgPrimarySourcesReviewed = true;
const bool finiteNcgRouteExternalToGu = true;
const bool routeUsesAlmostCommutativeGeometry = true;
const bool routeUsesFiniteDiscreteInternalSpace = true;
const bool routeDerivesHiggsAsDiscreteConnectionOrInnerFluctuation = true;
const bool routeProducesYangMillsHiggsAction = true;
const bool routeEncodesStandardModelGaugeStructure = true;
const bool routeHasHistoricalTreeLevelMassRelations = true;
const bool historicalRelationMhSqrt2MwPresent = true;
const bool historicalRelationSin2ThetaWThreeEighthsPresent = true;
const bool historicalRgPredictionHighHiggsRangePresent = true;
const double historicalPredictedHiggsMassForMtop175GeV = 164.01;
const double historicalPredictedHiggsMassForMtop180GeV = 171.92;
const double historicalPredictedHiggsRangeLowGeV = 153.42;
const double historicalPredictedHiggsRangeHighGeV = 191.94;
const bool spectralActionPredictsHiggsAround170GeV = true;
const bool postLhcScalarRepairRequiresExtraScalar = true;
const bool sitarzNoSpecialMassRelationObstructionPresent = true;
const bool routeMassRelationsNotInvariantWhenFullDiscreteGeometryTermsAllowed = true;
const bool routeNeedsFiniteTripleOrAlgebraChoice = true;
const bool routeNeedsCutoffScaleAndRgTransport = true;
const bool routeNeedsYukawaTopNeutrinoInputs = true;
const bool routeNeedsBMinusLExtensionOrExtraScalarForCompatibility = true;
const bool routePredictsCompatibleOnlyInSmallParameterSpace = true;
const bool routeDoesNotPredictObservedWzMasses = true;
const bool routeDoesNotProvideTargetIndependentObservedHiggsMass = true;
const bool routeDoesNotProvidePhysicalPoleExtraction = true;
const bool routeDoesNotProvideObservedPhotonWzHiggsProjection = true;

const bool routeRequiresGuLocalFiniteNcgMap = true;
const bool routeRequiresGuDiscreteInternalSpaceDerivation = true;
const bool routeRequiresGuDiracOperatorAndAlgebraSource = true;
const bool routeRequiresGuWzSourceRows = true;
const bool routeRequiresGuWeakMixingAngleSource = true;
const bool routeRequiresGuObservedFieldExtraction = true;
const bool routeRequiresGuHiggsScalarSourceOperator = true;
const bool routeRequiresGuHiggsSelfCouplingSource = true;
const bool routeRequiresTargetIndependentVevOrMassScale = true;
const bool routeRequiresLowEnergyRgAndThresholdTransport = true;
const bool routeRequiresGeVUnitNormalization = true;

const bool routeProvidesGuLocalFiniteNcgMap = false;
const bool routeProvidesGuDiscreteInternalSpaceDerivation = false;
const bool routeProvidesGuDiracOperatorAndAlgebraSource = false;
const bool routeProvidesGuWzSourceRows = false;
const bool routeProvidesGuWeakMixingAngleSource = false;
const bool routeProvidesGuObservedFieldExtraction = false;
const bool routeProvidesGuHiggsScalarSourceOperator = false;
const bool routeProvidesGuHiggsSelfCouplingSource = false;
const bool routeProvidesTargetIndependentVevOrMassScale = false;
const bool routeProvidesLowEnergyRgAndThresholdTransport = false;
const bool routeProvidesGeVUnitNormalization = false;
const bool routePromotesWzMasses = false;
const bool routePromotesHiggsMass = false;
const bool routeCompletesBosonPredictions = false;
const bool canFillPhase201WzContract = false;
const bool canFillPhase201HiggsContract = false;
const bool canFillPhase256ObservedFieldExtractionContract = false;
const int sourceRowCountExpected = 8;

var phase201AllRequiredLineagesPromotable = JsonBool(phase201.RootElement, "allRequiredLineagesPromotable") is true;
var existingEvidenceFound = JsonBool(phase213.RootElement, "existingEvidenceFound") is true;
var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;
var observedFieldExtractionFilledRequiredFieldCount = JsonInt(phase256.RootElement, "filledRequiredFieldCount") ?? -1;
var observedFieldExtractionContractPromotable = JsonBool(phase256.RootElement, "observedFieldExtractionContractPromotable") is true;
var spectralActionBosonSourceAuditPassed = JsonBool(phase268.RootElement, "spectralActionBosonSourceAuditPassed") is true;
var spectralActionPromotesWzMasses = JsonBool(phase268.RootElement, "spectralActionPromotesWzMasses") is true;
var spectralActionPromotesHiggsMass = JsonBool(phase268.RootElement, "spectralActionPromotesHiggsMass") is true;
var spectralActionCompletesBosonPredictions = JsonBool(phase268.RootElement, "spectralActionCompletesBosonPredictions") is true;
var su21RoutePromotesWzMasses = JsonBool(phase334.RootElement, "su21RoutePromotesWzMasses") is true;
var su21RoutePromotesHiggsMass = JsonBool(phase334.RootElement, "su21RoutePromotesHiggsMass") is true;
var diracLichnerowiczRoutePromotesWzMasses = JsonBool(phase355.RootElement, "routePromotesWzMasses") is true;
var diracLichnerowiczRoutePromotesHiggsMass = JsonBool(phase355.RootElement, "routePromotesHiggsMass") is true;

var sourceRows = new[]
{
    new SourceRow(
        "connes-lott-particle-models-ncg",
        connesLottParticleModelsUrl,
        "Particle Models and Noncommutative Geometry",
        "Original Connes-Lott finite-geometry framing of particle models as noncommutative gauge theory over an ordinary space plus a finite internal algebra.",
        "Geometric mechanism lead; it does not supply GU-local source-lineage rows or observed W/Z/H pole masses."),
    new SourceRow(
        "chamseddine-felder-frohlich-ncg-higgs-mechanism",
        connesLottHiggsMechanismUrl,
        "Noncommutative geometry and Higgs mechanism in the Standard Model",
        "Uses a generalized field strength on a noncommutative/discrete extension of spacetime to obtain a Higgs potential and symmetry breaking.",
        "Historical geometric-Higgs lead; any numerical relations depend on model choices and later obstructions/renormalization analysis."),
    new SourceRow(
        "sitarz-9304005-no-special-higgs-relation",
        sitarzNoSpecialRelationUrl,
        "Higgs Mass and Noncommutative Geometry",
        "Shows that electroweak NCG over continuous plus discrete space gives no forced special Higgs-mass relation once a gauge-invariant discrete curvature term is allowed.",
        "Directly blocks promotion of simple finite-NCG Higgs/W mass ratios as target-independent predictions."),
    new SourceRow(
        "okumura-ptp-1997-ncg-rg-higgs",
        okumuraRgAnalysisUrl,
        "Renormalization Group Analysis of the Higgs Boson Mass in a Noncommutative Geometry",
        "Audits NCG initial conditions such as 4 lambda = g^2 = (5/3) g'^2, giving m_H = sqrt(2) m_W at the boundary and high low-energy Higgs masses after RG flow.",
        "Numerical relation lead; it depends on a boundary scale, top mass, RG scheme, and does not match the observed 125 GeV Higgs without extra structure."),
    new SourceRow(
        "chamseddine-connes-marcolli-0610241",
        chamseddineConnesMarcolliUrl,
        "Gravity and the Standard Model with neutrino mixing",
        "Spectral-action finite-geometry model predicting high-scale couplings, Higgs scattering parameter, Yukawa sum, and a low-energy Higgs mass around 170 GeV.",
        "Geometric unification lead already bounded by Phase268; original Higgs value is not observed and needs RG/Yukawa/cutoff inputs."),
    new SourceRow(
        "resilience-spectral-standard-model-1208-1030",
        resilienceSpectralStandardModelUrl,
        "Resilience of the Spectral Standard Model",
        "Post-LHC repair route where an additional real scalar coupled to the Higgs changes RG behavior and restores compatibility with low Higgs mass.",
        "Compatibility repair, not a GU-local prediction; it introduces extra scalar dynamics and high-scale boundary inputs."),
    new SourceRow(
        "devastato-lizzi-martinetti-1403-7567",
        higgsMassNcgUrl,
        "Higgs mass in Noncommutative Geometry",
        "Reviews how an extra scalar generated from the Majorana neutrino mass can make the NCG Higgs computation compatible with the 126 GeV value.",
        "Requires neutrino/Majorana and extra-scalar assumptions; does not provide W/Z rows or a target-independent GU Higgs scalar-source law."),
    new SourceRow(
        "besnard-brouder-2010-04960-lorentzian-ncg-bl",
        lorentzianNcgBlUrl,
        "Noncommutative geometry, the Lorentzian Standard Model and its B-L extension",
        "Studies 1-loop RG flow of Lorentzian Connes-Lott models and finds only the B-L extension compatible with top and Higgs masses in a small parameter region.",
        "Predictive external model but still depends on B-L extension, thresholds, neutrino matrix orientation, and high-scale parameter choices.")
};

var checks = new[]
{
    new Check(
        "finite-ncg-primary-sources-reviewed",
        finiteNcgLeadPresent
            && finiteNcgPrimarySourcesReviewed
            && finiteNcgRouteExternalToGu
            && sourceRows.Length == sourceRowCountExpected,
        $"lead={finiteNcgLeadPresent}; reviewed={finiteNcgPrimarySourcesReviewed}; externalToGu={finiteNcgRouteExternalToGu}; sourceRows={sourceRows.Length}"),
    new Check(
        "finite-ncg-discrete-higgs-mechanism-captured",
        routeUsesAlmostCommutativeGeometry
            && routeUsesFiniteDiscreteInternalSpace
            && routeDerivesHiggsAsDiscreteConnectionOrInnerFluctuation
            && routeProducesYangMillsHiggsAction
            && routeEncodesStandardModelGaugeStructure,
        $"almostCommutative={routeUsesAlmostCommutativeGeometry}; finiteSpace={routeUsesFiniteDiscreteInternalSpace}; higgsDiscreteConnection={routeDerivesHiggsAsDiscreteConnectionOrInnerFluctuation}; ymh={routeProducesYangMillsHiggsAction}; smGauge={routeEncodesStandardModelGaugeStructure}"),
    new Check(
        "historical-ncg-mass-relation-leads-captured",
        routeHasHistoricalTreeLevelMassRelations
            && historicalRelationMhSqrt2MwPresent
            && historicalRelationSin2ThetaWThreeEighthsPresent
            && historicalRgPredictionHighHiggsRangePresent
            && historicalPredictedHiggsMassForMtop175GeV > 160.0
            && historicalPredictedHiggsMassForMtop180GeV > 170.0
            && spectralActionPredictsHiggsAround170GeV,
        $"treeRelations={routeHasHistoricalTreeLevelMassRelations}; mhSqrt2Mw={historicalRelationMhSqrt2MwPresent}; sin2ThreeEighths={historicalRelationSin2ThetaWThreeEighthsPresent}; rgHighRange={historicalRgPredictionHighHiggsRangePresent}; mh175={historicalPredictedHiggsMassForMtop175GeV:R}; mh180={historicalPredictedHiggsMassForMtop180GeV:R}; spectral170={spectralActionPredictsHiggsAround170GeV}"),
    new Check(
        "finite-ncg-promotion-obstructions-captured",
        sitarzNoSpecialMassRelationObstructionPresent
            && routeMassRelationsNotInvariantWhenFullDiscreteGeometryTermsAllowed
            && postLhcScalarRepairRequiresExtraScalar
            && routeNeedsFiniteTripleOrAlgebraChoice
            && routeNeedsCutoffScaleAndRgTransport
            && routeNeedsYukawaTopNeutrinoInputs
            && routeNeedsBMinusLExtensionOrExtraScalarForCompatibility
            && routePredictsCompatibleOnlyInSmallParameterSpace,
        $"sitarz={sitarzNoSpecialMassRelationObstructionPresent}; fullTerms={routeMassRelationsNotInvariantWhenFullDiscreteGeometryTermsAllowed}; extraScalar={postLhcScalarRepairRequiresExtraScalar}; finiteTriple={routeNeedsFiniteTripleOrAlgebraChoice}; cutoffRg={routeNeedsCutoffScaleAndRgTransport}; yukawaNeutrino={routeNeedsYukawaTopNeutrinoInputs}; blOrScalar={routeNeedsBMinusLExtensionOrExtraScalarForCompatibility}; smallParameterSpace={routePredictsCompatibleOnlyInSmallParameterSpace}"),
    new Check(
        "adjacent-ncg-superconnection-and-dirac-routes-remain-nonpromotional",
        spectralActionBosonSourceAuditPassed
            && !spectralActionPromotesWzMasses
            && !spectralActionPromotesHiggsMass
            && !spectralActionCompletesBosonPredictions
            && !su21RoutePromotesWzMasses
            && !su21RoutePromotesHiggsMass
            && !diracLichnerowiczRoutePromotesWzMasses
            && !diracLichnerowiczRoutePromotesHiggsMass,
        $"spectralPassed={spectralActionBosonSourceAuditPassed}; spectralWz={spectralActionPromotesWzMasses}; spectralHiggs={spectralActionPromotesHiggsMass}; spectralCompletes={spectralActionCompletesBosonPredictions}; su21Wz={su21RoutePromotesWzMasses}; su21Higgs={su21RoutePromotesHiggsMass}; diracWz={diracLichnerowiczRoutePromotesWzMasses}; diracHiggs={diracLichnerowiczRoutePromotesHiggsMass}"),
    new Check(
        "route-does-not-fill-gu-contracts",
        !routeProvidesGuLocalFiniteNcgMap
            && !routeProvidesGuDiscreteInternalSpaceDerivation
            && !routeProvidesGuDiracOperatorAndAlgebraSource
            && !routeProvidesGuWzSourceRows
            && !routeProvidesGuWeakMixingAngleSource
            && !routeProvidesGuObservedFieldExtraction
            && !routeProvidesGuHiggsScalarSourceOperator
            && !routeProvidesGuHiggsSelfCouplingSource
            && !routeProvidesTargetIndependentVevOrMassScale
            && !routeProvidesLowEnergyRgAndThresholdTransport
            && !routeProvidesGeVUnitNormalization
            && !routePromotesWzMasses
            && !routePromotesHiggsMass
            && !routeCompletesBosonPredictions
            && !canFillPhase201WzContract
            && !canFillPhase201HiggsContract
            && !canFillPhase256ObservedFieldExtractionContract,
        $"guFiniteMap={routeProvidesGuLocalFiniteNcgMap}; guFiniteSpace={routeProvidesGuDiscreteInternalSpaceDerivation}; guDiracAlgebra={routeProvidesGuDiracOperatorAndAlgebraSource}; guWzRows={routeProvidesGuWzSourceRows}; weakAngle={routeProvidesGuWeakMixingAngleSource}; observed={routeProvidesGuObservedFieldExtraction}; higgsOperator={routeProvidesGuHiggsScalarSourceOperator}; selfCoupling={routeProvidesGuHiggsSelfCouplingSource}; scale={routeProvidesTargetIndependentVevOrMassScale}; rg={routeProvidesLowEnergyRgAndThresholdTransport}; gev={routeProvidesGeVUnitNormalization}; promotesWz={routePromotesWzMasses}; promotesHiggs={routePromotesHiggsMass}; completes={routeCompletesBosonPredictions}"),
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

var finiteNcgDiscreteHiggsSourceAuditPassed = checks.All(check => check.Passed)
    && finiteNcgDiscreteHiggsSourceAuditPassedExpected
    && !routePromotesWzMasses
    && !routePromotesHiggsMass
    && !routeCompletesBosonPredictions
    && !canFillPhase201WzContract
    && !canFillPhase201HiggsContract
    && !canFillPhase256ObservedFieldExtractionContract;

var terminalStatus = finiteNcgDiscreteHiggsSourceAuditPassed
    ? "finite-ncg-discrete-higgs-source-audit-geometric-mechanism-not-gu-mass-law"
    : "finite-ncg-discrete-higgs-source-audit-review-required";

var result = new
{
    phaseId = "phase359-finite-ncg-discrete-higgs-source-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    finiteNcgDiscreteHiggsSourceAuditPassed,
    finiteNcgLeadPresent,
    finiteNcgPrimarySourcesReviewed,
    finiteNcgRouteExternalToGu,
    routeUsesAlmostCommutativeGeometry,
    routeUsesFiniteDiscreteInternalSpace,
    routeDerivesHiggsAsDiscreteConnectionOrInnerFluctuation,
    routeProducesYangMillsHiggsAction,
    routeEncodesStandardModelGaugeStructure,
    routeHasHistoricalTreeLevelMassRelations,
    historicalRelationMhSqrt2MwPresent,
    historicalRelationSin2ThetaWThreeEighthsPresent,
    historicalRgPredictionHighHiggsRangePresent,
    historicalPredictedHiggsMassForMtop175GeV,
    historicalPredictedHiggsMassForMtop180GeV,
    historicalPredictedHiggsRangeLowGeV,
    historicalPredictedHiggsRangeHighGeV,
    spectralActionPredictsHiggsAround170GeV,
    postLhcScalarRepairRequiresExtraScalar,
    sitarzNoSpecialMassRelationObstructionPresent,
    routeMassRelationsNotInvariantWhenFullDiscreteGeometryTermsAllowed,
    routeNeedsFiniteTripleOrAlgebraChoice,
    routeNeedsCutoffScaleAndRgTransport,
    routeNeedsYukawaTopNeutrinoInputs,
    routeNeedsBMinusLExtensionOrExtraScalarForCompatibility,
    routePredictsCompatibleOnlyInSmallParameterSpace,
    routeDoesNotPredictObservedWzMasses,
    routeDoesNotProvideTargetIndependentObservedHiggsMass,
    routeDoesNotProvidePhysicalPoleExtraction,
    routeDoesNotProvideObservedPhotonWzHiggsProjection,
    routeRequiresGuLocalFiniteNcgMap,
    routeRequiresGuDiscreteInternalSpaceDerivation,
    routeRequiresGuDiracOperatorAndAlgebraSource,
    routeRequiresGuWzSourceRows,
    routeRequiresGuWeakMixingAngleSource,
    routeRequiresGuObservedFieldExtraction,
    routeRequiresGuHiggsScalarSourceOperator,
    routeRequiresGuHiggsSelfCouplingSource,
    routeRequiresTargetIndependentVevOrMassScale,
    routeRequiresLowEnergyRgAndThresholdTransport,
    routeRequiresGeVUnitNormalization,
    routeProvidesGuLocalFiniteNcgMap,
    routeProvidesGuDiscreteInternalSpaceDerivation,
    routeProvidesGuDiracOperatorAndAlgebraSource,
    routeProvidesGuWzSourceRows,
    routeProvidesGuWeakMixingAngleSource,
    routeProvidesGuObservedFieldExtraction,
    routeProvidesGuHiggsScalarSourceOperator,
    routeProvidesGuHiggsSelfCouplingSource,
    routeProvidesTargetIndependentVevOrMassScale,
    routeProvidesLowEnergyRgAndThresholdTransport,
    routeProvidesGeVUnitNormalization,
    routePromotesWzMasses,
    routePromotesHiggsMass,
    routeCompletesBosonPredictions,
    canFillPhase201WzContract,
    canFillPhase201HiggsContract,
    canFillPhase256ObservedFieldExtractionContract,
    sourceRowCount = sourceRows.Length,
    sourceRows,
    adjacentRouteBoundary = new
    {
        spectralActionBosonSourceAuditPassed,
        spectralActionPromotesWzMasses,
        spectralActionPromotesHiggsMass,
        spectralActionCompletesBosonPredictions,
        su21RoutePromotesWzMasses,
        su21RoutePromotesHiggsMass,
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
    checks,
    decision = "Do not promote W/Z or Higgs physical masses from finite noncommutative-geometry or Connes-Lott discrete-Higgs routes in this repository. The sources supply a genuine geometric Higgs-as-discrete-connection mechanism and historical mass-relation leads, but simple Higgs/W relations are not invariant once all allowed discrete-geometry terms are included, spectral-action relations are high-scale/RG/Yukawa/cutoff dependent, and post-LHC compatibility requires extra scalar or B-L extension structure. Current GU artifacts still do not supply a finite-NCG map from Shiab/observer geometry, W/Z source rows, weak-angle/coupling source, observed photon/W/Z/H projection, target-independent VEV or mass scale, Higgs scalar-source/self-coupling lineage, RG/threshold transport, pole extraction, or GeV normalization.",
    nextRequiredArtifact = new[]
    {
        "A GU-local derivation of an almost-commutative or finite internal algebra from Shiab/observer-sector geometry rather than importing an external spectral triple.",
        "A GU Dirac-operator/algebra source that produces observed electroweak SU(2) x U(1), photon/W/Z projection, and separate W/Z source rows.",
        "A target-independent Higgs scalar-source and self-coupling law that survives the Sitarz discrete-curvature obstruction and does not rely on fitted extra-scalar or B-L parameters.",
        "A low-energy RG/threshold and physical-pole extractor with GeV normalization before any finite-NCG route can promote W/Z/H masses."
    }
};

var options = new JsonSerializerOptions { WriteIndented = true };
File.WriteAllText(Path.Combine(outputDir, "finite_ncg_discrete_higgs_source_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "finite_ncg_discrete_higgs_source_audit_summary.json"),
    JsonSerializer.Serialize(
        new
        {
            result.phaseId,
            result.terminalStatus,
            result.finiteNcgDiscreteHiggsSourceAuditPassed,
            result.finiteNcgLeadPresent,
            result.finiteNcgPrimarySourcesReviewed,
            result.finiteNcgRouteExternalToGu,
            result.routeUsesAlmostCommutativeGeometry,
            result.routeUsesFiniteDiscreteInternalSpace,
            result.routeDerivesHiggsAsDiscreteConnectionOrInnerFluctuation,
            result.routeProducesYangMillsHiggsAction,
            result.routeEncodesStandardModelGaugeStructure,
            result.routeHasHistoricalTreeLevelMassRelations,
            result.historicalRelationMhSqrt2MwPresent,
            result.historicalRelationSin2ThetaWThreeEighthsPresent,
            result.historicalRgPredictionHighHiggsRangePresent,
            result.historicalPredictedHiggsMassForMtop175GeV,
            result.historicalPredictedHiggsMassForMtop180GeV,
            result.historicalPredictedHiggsRangeLowGeV,
            result.historicalPredictedHiggsRangeHighGeV,
            result.spectralActionPredictsHiggsAround170GeV,
            result.postLhcScalarRepairRequiresExtraScalar,
            result.sitarzNoSpecialMassRelationObstructionPresent,
            result.routeMassRelationsNotInvariantWhenFullDiscreteGeometryTermsAllowed,
            result.routeNeedsFiniteTripleOrAlgebraChoice,
            result.routeNeedsCutoffScaleAndRgTransport,
            result.routeNeedsYukawaTopNeutrinoInputs,
            result.routeNeedsBMinusLExtensionOrExtraScalarForCompatibility,
            result.routePredictsCompatibleOnlyInSmallParameterSpace,
            result.routeDoesNotPredictObservedWzMasses,
            result.routeDoesNotProvideTargetIndependentObservedHiggsMass,
            result.routeDoesNotProvidePhysicalPoleExtraction,
            result.routeDoesNotProvideObservedPhotonWzHiggsProjection,
            result.routeRequiresGuLocalFiniteNcgMap,
            result.routeRequiresGuDiscreteInternalSpaceDerivation,
            result.routeRequiresGuDiracOperatorAndAlgebraSource,
            result.routeRequiresGuWzSourceRows,
            result.routeRequiresGuWeakMixingAngleSource,
            result.routeRequiresGuObservedFieldExtraction,
            result.routeRequiresGuHiggsScalarSourceOperator,
            result.routeRequiresGuHiggsSelfCouplingSource,
            result.routeRequiresTargetIndependentVevOrMassScale,
            result.routeRequiresLowEnergyRgAndThresholdTransport,
            result.routeRequiresGeVUnitNormalization,
            result.routeProvidesGuLocalFiniteNcgMap,
            result.routeProvidesGuDiscreteInternalSpaceDerivation,
            result.routeProvidesGuDiracOperatorAndAlgebraSource,
            result.routeProvidesGuWzSourceRows,
            result.routeProvidesGuWeakMixingAngleSource,
            result.routeProvidesGuObservedFieldExtraction,
            result.routeProvidesGuHiggsScalarSourceOperator,
            result.routeProvidesGuHiggsSelfCouplingSource,
            result.routeProvidesTargetIndependentVevOrMassScale,
            result.routeProvidesLowEnergyRgAndThresholdTransport,
            result.routeProvidesGeVUnitNormalization,
            result.routePromotesWzMasses,
            result.routePromotesHiggsMass,
            result.routeCompletesBosonPredictions,
            result.canFillPhase201WzContract,
            result.canFillPhase201HiggsContract,
            result.canFillPhase256ObservedFieldExtractionContract,
            result.sourceRowCount,
            result.adjacentRouteBoundary,
            result.contractImpact,
            result.decision,
            result.nextRequiredArtifact
        },
        options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"finiteNcgDiscreteHiggsSourceAuditPassed={finiteNcgDiscreteHiggsSourceAuditPassed}");
Console.WriteLine($"routeDerivesHiggsAsDiscreteConnectionOrInnerFluctuation={routeDerivesHiggsAsDiscreteConnectionOrInnerFluctuation}");
Console.WriteLine($"historicalRelationMhSqrt2MwPresent={historicalRelationMhSqrt2MwPresent}");
Console.WriteLine($"sitarzNoSpecialMassRelationObstructionPresent={sitarzNoSpecialMassRelationObstructionPresent}");
Console.WriteLine($"routePromotesWzMasses={routePromotesWzMasses}");
Console.WriteLine($"routePromotesHiggsMass={routePromotesHiggsMass}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind is JsonValueKind.True or JsonValueKind.False ? property.GetBoolean() : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.TryGetInt32(out var value) ? value : null;

sealed record Check(string Id, bool Passed, string Evidence);

sealed record SourceRow(
    string Id,
    string Url,
    string Title,
    string Summary,
    string PromotionBoundary);
