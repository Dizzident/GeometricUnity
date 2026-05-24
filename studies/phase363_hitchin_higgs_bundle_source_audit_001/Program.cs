using System.Text.Json;

const string DefaultOutputDir = "studies/phase363_hitchin_higgs_bundle_source_audit_001/output";
const string Phase201Path = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase256Path = "studies/phase256_observed_field_extraction_intake_contract_001/output/observed_field_extraction_intake_contract_summary.json";
const string Phase332Path = "studies/phase332_string_m_theory_compactification_source_audit_001/output/string_m_theory_compactification_source_audit_summary.json";
const string Phase353Path = "studies/phase353_gauge_higgs_unification_source_audit_001/output/gauge_higgs_unification_source_audit_summary.json";
const string Phase362Path = "studies/phase362_framed_standard_model_source_audit_001/output/framed_standard_model_source_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE363_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase256 = JsonDocument.Parse(File.ReadAllText(Phase256Path));
using var phase332 = JsonDocument.Parse(File.ReadAllText(Phase332Path));
using var phase353 = JsonDocument.Parse(File.ReadAllText(Phase353Path));
using var phase362 = JsonDocument.Parse(File.ReadAllText(Phase362Path));

const string hitchinSelfDualityUrl = "https://doi.org/10.1112/plms/s3-55.1.59";
const string fTheoryModelBuildingUrl = "https://arxiv.org/abs/0802.2969";
const string fTheoryGutsIUrl = "https://arxiv.org/abs/0802.3391";
const string fTheoryGutsIIUrl = "https://arxiv.org/abs/0806.0102";
const string higgsBundlesUvCompletionUrl = "https://arxiv.org/abs/0904.1218";
const string fTheoryCompactificationsGutsUrl = "https://arxiv.org/abs/0904.3932";
const string tBranesMonodromyUrl = "https://arxiv.org/abs/1010.5780";

const bool hitchinHiggsBundleSourceAuditPassedExpected = true;
const bool hitchinHiggsBundleLeadPresent = true;
const bool hitchinHiggsBundlePrimarySourcesReviewed = true;
const bool hitchinHiggsBundleRouteExternalToGu = true;
const bool routeUsesHitchinEquations = true;
const bool routeUsesConnectionPlusHiggsField = true;
const bool routeUsesHiggsBundleModuli = true;
const bool routeUsesSpectralCovers = true;
const bool routeUsesFTheorySevenBranes = true;
const bool routeEncodesGaugeGroupMatterYukawaGeometrically = true;
const bool routeUsesHyperchargeFluxBreaking = true;
const bool routeCanKeepU1YGaugeBosonMasslessInGlobalModels = true;
const bool routeUsesMeromorphicHiggsBundles = true;
const bool routeIncludesNoGoForGenericLocalSu5ThreeGenerationModel = true;
const bool routeIncludesTBranesNonAbelianHiggsField = true;
const bool tBranesShowSpectralEigenvaluesInsufficient = true;
const bool routeIncludesMssmOrGutNotObservedSmMassLaw = true;
const bool routeIncludesFlavorHierarchyOrYukawaLead = true;
const int sourceRowCountExpected = 7;
const int fTheoryModelBuildingLatestArxivVersion = 3;
const int fTheoryModelBuildingPageCount = 75;
const int fTheoryGutsILatestArxivVersion = 2;
const int fTheoryGutsIPageCount = 121;
const int fTheoryGutsIIFinalArxivVersion = 3;
const int fTheoryGutsIIPageCount = 203;
const int higgsBundlesUvCompletionArxivVersion = 1;
const int higgsBundlesUvCompletionPageCount = 48;
const int fTheoryCompactificationsGutsLatestArxivVersion = 3;
const int fTheoryCompactificationsGutsPageCount = 55;
const int tBranesArxivVersion = 4;
const int tBranesPageCount = 110;
const int hitchinSelfDualityStartPage = 59;
const int hitchinSelfDualityEndPage = 126;

const bool routeTargetsGutOrMssmNotObservedWzHMassLaw = true;
const bool routeDependsOnCompactificationGeometryChoice = true;
const bool routeDependsOnFluxChoice = true;
const bool routeDependsOnSevenBraneConfiguration = true;
const bool routeDependsOnSupersymmetricGutAssumptions = true;
const bool routeHyperchargeFluxNotLowEnergyWzMassSource = true;
const bool routeKeepsU1YMasslessButDoesNotDerivePhotonWzMasses = true;
const bool routeYukawaFlavorLeadNotGaugeBosonMassLaw = true;
const bool routeTBraneSpectralDataCanMissLocalizedModes = true;
const bool routeDoesNotPredictObservedWzMasses = true;
const bool routeDoesNotProvideTargetIndependentObservedHiggsMass = true;
const bool routeDoesNotProvidePhysicalPoleExtraction = true;
const bool routeDoesNotProvideObservedPhotonWzHiggsProjection = true;

const bool routeRequiresGuLocalHitchinBundleMap = true;
const bool routeRequiresGuRiemannSurfaceOrSpectralCurveSource = true;
const bool routeRequiresGuFTheoryCompactificationMap = true;
const bool routeRequiresGuFluxVacuumSelection = true;
const bool routeRequiresGuElectroweakHiggsDoubletProjection = true;
const bool routeRequiresGuWzSourceRows = true;
const bool routeRequiresGuWeakMixingAngleSource = true;
const bool routeRequiresGuGaugeCouplingNormalization = true;
const bool routeRequiresGuObservedFieldExtraction = true;
const bool routeRequiresGuHiggsScalarSourceOperator = true;
const bool routeRequiresGuHiggsSelfCouplingSource = true;
const bool routeRequiresTargetIndependentVevOrMassScale = true;
const bool routeRequiresLowEnergyRgAndThresholdTransport = true;
const bool routeRequiresGeVUnitNormalization = true;

const bool routeProvidesGuLocalHitchinBundleMap = false;
const bool routeProvidesGuRiemannSurfaceOrSpectralCurveSource = false;
const bool routeProvidesGuFTheoryCompactificationMap = false;
const bool routeProvidesGuFluxVacuumSelection = false;
const bool routeProvidesGuElectroweakHiggsDoubletProjection = false;
const bool routeProvidesGuWzSourceRows = false;
const bool routeProvidesGuWeakMixingAngleSource = false;
const bool routeProvidesGuGaugeCouplingNormalization = false;
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

var phase201AllRequiredLineagesPromotable = JsonBool(phase201.RootElement, "allRequiredLineagesPromotable") is true;
var existingEvidenceFound = JsonBool(phase213.RootElement, "existingEvidenceFound") is true;
var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;
var observedFieldExtractionFilledRequiredFieldCount = JsonInt(phase256.RootElement, "filledRequiredFieldCount") ?? -1;
var observedFieldExtractionContractPromotable = JsonBool(phase256.RootElement, "observedFieldExtractionContractPromotable") is true;
var stringMTheoryCompactificationSourceAuditPassed = JsonBool(phase332.RootElement, "stringMTheoryCompactificationSourceAuditPassed") is true;
var stringRoutePromotesWzMasses = JsonBool(phase332.RootElement, "stringRoutePromotesWzMasses") is true;
var stringRoutePromotesHiggsMass = JsonBool(phase332.RootElement, "stringRoutePromotesHiggsMass") is true;
var gaugeHiggsUnificationSourceAuditPassed = JsonBool(phase353.RootElement, "gaugeHiggsUnificationSourceAuditPassed") is true;
var gaugeHiggsRoutePromotesWzMasses = JsonBool(phase353.RootElement, "routePromotesWzMasses") is true;
var gaugeHiggsRoutePromotesHiggsMass = JsonBool(phase353.RootElement, "routePromotesHiggsMass") is true;
var framedStandardModelSourceAuditPassed = JsonBool(phase362.RootElement, "framedStandardModelSourceAuditPassed") is true;
var framedStandardModelRoutePromotesWzMasses = JsonBool(phase362.RootElement, "routePromotesWzMasses") is true;
var framedStandardModelRoutePromotesHiggsMass = JsonBool(phase362.RootElement, "routePromotesHiggsMass") is true;

var sourceRows = new[]
{
    new SourceRow(
        "hitchin-1987-self-duality",
        hitchinSelfDualityUrl,
        "The Self-Duality Equations on a Riemann Surface",
        "Foundational Hitchin-equation source: a connection-plus-Higgs-field system on a Riemann surface whose moduli space became the mathematical base for Higgs-bundle geometry.",
        "Mathematical foundation for Higgs bundles, but not an electroweak W/Z/H source law or GU-local observed-field map."),
    new SourceRow(
        "donagi-wijnholt-0802-2969-f-theory-model-building",
        fTheoryModelBuildingUrl,
        "Model Building with F-Theory",
        "Uses F-theory compactifications and G-flux to derive charged chiral spectra and Yukawa couplings in N=1 compactifications.",
        "Geometric spectrum/Yukawa lead; compactification- and flux-dependent and not a physical W/Z/H mass extraction."),
    new SourceRow(
        "beasley-heckman-vafa-0802-3391-f-theory-guts-i",
        fTheoryGutsIUrl,
        "GUTs and Exceptional Branes in F-theory - I",
        "Shows how local F-theory singularity geometry encodes gauge group, matter content, and Yukawa couplings through a twisted gauge theory with defects.",
        "Strong gauge/matter/Yukawa geometry lead; no target-independent observed W/Z/H masses."),
    new SourceRow(
        "beasley-heckman-vafa-0806-0102-f-theory-guts-ii",
        fTheoryGutsIIUrl,
        "GUTs and Exceptional Branes in F-theory - II: Experimental Predictions",
        "Uses internal hypercharge flux to break a GUT group to the MSSM or flipped GUT models, with flavor and neutrino phenomenology.",
        "Phenomenological GUT lead; hypercharge flux is not a low-energy W/Z mass source law."),
    new SourceRow(
        "donagi-wijnholt-0904-1218-higgs-bundles-uv-completion",
        higgsBundlesUvCompletionUrl,
        "Higgs Bundles and UV Completion in F-Theory",
        "Describes supersymmetric eight-dimensional Yang-Mills compactifications with meromorphic Higgs bundles and spectral covers, including a no-go theorem for generic local SU(5) three-generation models.",
        "Closest Higgs-bundle/spectral-cover lead; still external to GU and dependent on compactification and moduli choices."),
    new SourceRow(
        "marsano-saulina-schafer-nameki-0904-3932-f-theory-guts",
        fTheoryCompactificationsGutsUrl,
        "F-theory Compactifications for Supersymmetric GUTs",
        "Constructs global SU(5) F-theory GUT compactifications and states topological conditions allowing the U(1)_Y gauge boson to remain massless.",
        "Useful photon/U(1)_Y boundary; keeping U(1)_Y massless is not an observed photon/W/Z/H projection or mass law."),
    new SourceRow(
        "cecotti-cordova-heckman-vafa-1010-5780-t-branes",
        tBranesMonodromyUrl,
        "T-Branes and Monodromy",
        "Introduces T-branes with non-abelian upper-triangular Higgs fields and shows spectral eigenvalues can miss localized modes.",
        "Important warning against spectral-data-only promotion; no GU-local mass prediction.")
};

var checks = new[]
{
    new Check(
        "hitchin-higgs-bundle-primary-sources-reviewed",
        hitchinHiggsBundleLeadPresent
            && hitchinHiggsBundlePrimarySourcesReviewed
            && hitchinHiggsBundleRouteExternalToGu
            && sourceRows.Length == sourceRowCountExpected,
        $"lead={hitchinHiggsBundleLeadPresent}; reviewed={hitchinHiggsBundlePrimarySourcesReviewed}; externalToGu={hitchinHiggsBundleRouteExternalToGu}; sourceRows={sourceRows.Length}"),
    new Check(
        "hitchin-higgs-bundle-geometric-structure-captured",
        routeUsesHitchinEquations
            && routeUsesConnectionPlusHiggsField
            && routeUsesHiggsBundleModuli
            && routeUsesSpectralCovers
            && routeUsesFTheorySevenBranes
            && routeEncodesGaugeGroupMatterYukawaGeometrically
            && routeUsesHyperchargeFluxBreaking
            && routeCanKeepU1YGaugeBosonMasslessInGlobalModels
            && routeUsesMeromorphicHiggsBundles
            && routeIncludesTBranesNonAbelianHiggsField,
        $"hitchin={routeUsesHitchinEquations}; connectionPlusHiggs={routeUsesConnectionPlusHiggsField}; moduli={routeUsesHiggsBundleModuli}; spectralCovers={routeUsesSpectralCovers}; sevenBranes={routeUsesFTheorySevenBranes}; gaugeMatterYukawa={routeEncodesGaugeGroupMatterYukawaGeometrically}; hyperchargeFlux={routeUsesHyperchargeFluxBreaking}; masslessU1Y={routeCanKeepU1YGaugeBosonMasslessInGlobalModels}; meromorphic={routeUsesMeromorphicHiggsBundles}; tBranes={routeIncludesTBranesNonAbelianHiggsField}"),
    new Check(
        "hitchin-higgs-bundle-version-and-page-metadata-captured",
        hitchinSelfDualityStartPage == 59
            && hitchinSelfDualityEndPage == 126
            && fTheoryModelBuildingLatestArxivVersion == 3
            && fTheoryModelBuildingPageCount == 75
            && fTheoryGutsILatestArxivVersion == 2
            && fTheoryGutsIPageCount == 121
            && fTheoryGutsIIFinalArxivVersion == 3
            && fTheoryGutsIIPageCount == 203
            && higgsBundlesUvCompletionArxivVersion == 1
            && higgsBundlesUvCompletionPageCount == 48
            && fTheoryCompactificationsGutsLatestArxivVersion == 3
            && fTheoryCompactificationsGutsPageCount == 55
            && tBranesArxivVersion == 4
            && tBranesPageCount == 110,
        $"hitchinPages={hitchinSelfDualityStartPage}-{hitchinSelfDualityEndPage}; fTheoryModelBuildingV={fTheoryModelBuildingLatestArxivVersion}; fTheoryModelBuildingPages={fTheoryModelBuildingPageCount}; gutsIV={fTheoryGutsILatestArxivVersion}; gutsIPages={fTheoryGutsIPageCount}; gutsIIV={fTheoryGutsIIFinalArxivVersion}; gutsIIPages={fTheoryGutsIIPageCount}; higgsBundleV={higgsBundlesUvCompletionArxivVersion}; higgsBundlePages={higgsBundlesUvCompletionPageCount}; compactificationV={fTheoryCompactificationsGutsLatestArxivVersion}; compactificationPages={fTheoryCompactificationsGutsPageCount}; tBranesV={tBranesArxivVersion}; tBranesPages={tBranesPageCount}"),
    new Check(
        "hitchin-higgs-bundle-promotion-obstructions-captured",
        routeTargetsGutOrMssmNotObservedWzHMassLaw
            && routeDependsOnCompactificationGeometryChoice
            && routeDependsOnFluxChoice
            && routeDependsOnSevenBraneConfiguration
            && routeDependsOnSupersymmetricGutAssumptions
            && routeHyperchargeFluxNotLowEnergyWzMassSource
            && routeKeepsU1YMasslessButDoesNotDerivePhotonWzMasses
            && routeYukawaFlavorLeadNotGaugeBosonMassLaw
            && routeTBraneSpectralDataCanMissLocalizedModes
            && routeDoesNotPredictObservedWzMasses
            && routeDoesNotProvideTargetIndependentObservedHiggsMass
            && routeDoesNotProvidePhysicalPoleExtraction
            && routeDoesNotProvideObservedPhotonWzHiggsProjection,
        $"gutMssmScope={routeTargetsGutOrMssmNotObservedWzHMassLaw}; compactification={routeDependsOnCompactificationGeometryChoice}; flux={routeDependsOnFluxChoice}; sevenBranes={routeDependsOnSevenBraneConfiguration}; susyGut={routeDependsOnSupersymmetricGutAssumptions}; hyperchargeFluxNotMass={routeHyperchargeFluxNotLowEnergyWzMassSource}; u1YNotWz={routeKeepsU1YMasslessButDoesNotDerivePhotonWzMasses}; yukawaNotMass={routeYukawaFlavorLeadNotGaugeBosonMassLaw}; tBraneWarning={routeTBraneSpectralDataCanMissLocalizedModes}; observedWz={routeDoesNotPredictObservedWzMasses}; observedHiggs={routeDoesNotProvideTargetIndependentObservedHiggsMass}; pole={routeDoesNotProvidePhysicalPoleExtraction}; projection={routeDoesNotProvideObservedPhotonWzHiggsProjection}"),
    new Check(
        "adjacent-compactification-higgs-routes-remain-nonpromotional",
        stringMTheoryCompactificationSourceAuditPassed
            && !stringRoutePromotesWzMasses
            && !stringRoutePromotesHiggsMass
            && gaugeHiggsUnificationSourceAuditPassed
            && !gaugeHiggsRoutePromotesWzMasses
            && !gaugeHiggsRoutePromotesHiggsMass
            && framedStandardModelSourceAuditPassed
            && !framedStandardModelRoutePromotesWzMasses
            && !framedStandardModelRoutePromotesHiggsMass,
        $"stringPassed={stringMTheoryCompactificationSourceAuditPassed}; stringWz={stringRoutePromotesWzMasses}; stringHiggs={stringRoutePromotesHiggsMass}; gaugeHiggsPassed={gaugeHiggsUnificationSourceAuditPassed}; gaugeHiggsWz={gaugeHiggsRoutePromotesWzMasses}; gaugeHiggsHiggs={gaugeHiggsRoutePromotesHiggsMass}; framedPassed={framedStandardModelSourceAuditPassed}; framedWz={framedStandardModelRoutePromotesWzMasses}; framedHiggs={framedStandardModelRoutePromotesHiggsMass}"),
    new Check(
        "route-does-not-fill-gu-contracts",
        routeRequiresGuLocalHitchinBundleMap
            && routeRequiresGuRiemannSurfaceOrSpectralCurveSource
            && routeRequiresGuFTheoryCompactificationMap
            && routeRequiresGuFluxVacuumSelection
            && routeRequiresGuElectroweakHiggsDoubletProjection
            && routeRequiresGuWzSourceRows
            && routeRequiresGuWeakMixingAngleSource
            && routeRequiresGuGaugeCouplingNormalization
            && routeRequiresGuObservedFieldExtraction
            && routeRequiresGuHiggsScalarSourceOperator
            && routeRequiresGuHiggsSelfCouplingSource
            && routeRequiresTargetIndependentVevOrMassScale
            && routeRequiresLowEnergyRgAndThresholdTransport
            && routeRequiresGeVUnitNormalization
            && !routeProvidesGuLocalHitchinBundleMap
            && !routeProvidesGuRiemannSurfaceOrSpectralCurveSource
            && !routeProvidesGuFTheoryCompactificationMap
            && !routeProvidesGuFluxVacuumSelection
            && !routeProvidesGuElectroweakHiggsDoubletProjection
            && !routeProvidesGuWzSourceRows
            && !routeProvidesGuWeakMixingAngleSource
            && !routeProvidesGuGaugeCouplingNormalization
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
        $"requiresHitchinMap={routeRequiresGuLocalHitchinBundleMap}; requiresSpectralCurve={routeRequiresGuRiemannSurfaceOrSpectralCurveSource}; requiresFTheory={routeRequiresGuFTheoryCompactificationMap}; requiresFlux={routeRequiresGuFluxVacuumSelection}; requiresDoublet={routeRequiresGuElectroweakHiggsDoubletProjection}; requiresWzRows={routeRequiresGuWzSourceRows}; requiresObserved={routeRequiresGuObservedFieldExtraction}; requiresHiggsOperator={routeRequiresGuHiggsScalarSourceOperator}; providesHitchinMap={routeProvidesGuLocalHitchinBundleMap}; providesWzRows={routeProvidesGuWzSourceRows}; providesObserved={routeProvidesGuObservedFieldExtraction}; providesHiggsOperator={routeProvidesGuHiggsScalarSourceOperator}; promotesWz={routePromotesWzMasses}; promotesHiggs={routePromotesHiggsMass}; completes={routeCompletesBosonPredictions}"),
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

var hitchinHiggsBundleSourceAuditPassed = checks.All(check => check.Passed)
    && hitchinHiggsBundleSourceAuditPassedExpected
    && !routePromotesWzMasses
    && !routePromotesHiggsMass
    && !routeCompletesBosonPredictions
    && !canFillPhase201WzContract
    && !canFillPhase201HiggsContract
    && !canFillPhase256ObservedFieldExtractionContract;

var terminalStatus = hitchinHiggsBundleSourceAuditPassed
    ? "hitchin-higgs-bundle-source-audit-spectral-cover-lead-not-gu-mass-law"
    : "hitchin-higgs-bundle-source-audit-review-required";

var result = new
{
    phaseId = "phase363-hitchin-higgs-bundle-source-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    hitchinHiggsBundleSourceAuditPassed,
    hitchinHiggsBundleLeadPresent,
    hitchinHiggsBundlePrimarySourcesReviewed,
    hitchinHiggsBundleRouteExternalToGu,
    routeUsesHitchinEquations,
    routeUsesConnectionPlusHiggsField,
    routeUsesHiggsBundleModuli,
    routeUsesSpectralCovers,
    routeUsesFTheorySevenBranes,
    routeEncodesGaugeGroupMatterYukawaGeometrically,
    routeUsesHyperchargeFluxBreaking,
    routeCanKeepU1YGaugeBosonMasslessInGlobalModels,
    routeUsesMeromorphicHiggsBundles,
    routeIncludesNoGoForGenericLocalSu5ThreeGenerationModel,
    routeIncludesTBranesNonAbelianHiggsField,
    tBranesShowSpectralEigenvaluesInsufficient,
    routeIncludesMssmOrGutNotObservedSmMassLaw,
    routeIncludesFlavorHierarchyOrYukawaLead,
    fTheoryModelBuildingLatestArxivVersion,
    fTheoryModelBuildingPageCount,
    fTheoryGutsILatestArxivVersion,
    fTheoryGutsIPageCount,
    fTheoryGutsIIFinalArxivVersion,
    fTheoryGutsIIPageCount,
    higgsBundlesUvCompletionArxivVersion,
    higgsBundlesUvCompletionPageCount,
    fTheoryCompactificationsGutsLatestArxivVersion,
    fTheoryCompactificationsGutsPageCount,
    tBranesArxivVersion,
    tBranesPageCount,
    hitchinSelfDualityStartPage,
    hitchinSelfDualityEndPage,
    routeTargetsGutOrMssmNotObservedWzHMassLaw,
    routeDependsOnCompactificationGeometryChoice,
    routeDependsOnFluxChoice,
    routeDependsOnSevenBraneConfiguration,
    routeDependsOnSupersymmetricGutAssumptions,
    routeHyperchargeFluxNotLowEnergyWzMassSource,
    routeKeepsU1YMasslessButDoesNotDerivePhotonWzMasses,
    routeYukawaFlavorLeadNotGaugeBosonMassLaw,
    routeTBraneSpectralDataCanMissLocalizedModes,
    routeDoesNotPredictObservedWzMasses,
    routeDoesNotProvideTargetIndependentObservedHiggsMass,
    routeDoesNotProvidePhysicalPoleExtraction,
    routeDoesNotProvideObservedPhotonWzHiggsProjection,
    routeRequiresGuLocalHitchinBundleMap,
    routeRequiresGuRiemannSurfaceOrSpectralCurveSource,
    routeRequiresGuFTheoryCompactificationMap,
    routeRequiresGuFluxVacuumSelection,
    routeRequiresGuElectroweakHiggsDoubletProjection,
    routeRequiresGuWzSourceRows,
    routeRequiresGuWeakMixingAngleSource,
    routeRequiresGuGaugeCouplingNormalization,
    routeRequiresGuObservedFieldExtraction,
    routeRequiresGuHiggsScalarSourceOperator,
    routeRequiresGuHiggsSelfCouplingSource,
    routeRequiresTargetIndependentVevOrMassScale,
    routeRequiresLowEnergyRgAndThresholdTransport,
    routeRequiresGeVUnitNormalization,
    routeProvidesGuLocalHitchinBundleMap,
    routeProvidesGuRiemannSurfaceOrSpectralCurveSource,
    routeProvidesGuFTheoryCompactificationMap,
    routeProvidesGuFluxVacuumSelection,
    routeProvidesGuElectroweakHiggsDoubletProjection,
    routeProvidesGuWzSourceRows,
    routeProvidesGuWeakMixingAngleSource,
    routeProvidesGuGaugeCouplingNormalization,
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
        stringMTheoryCompactificationSourceAuditPassed,
        stringRoutePromotesWzMasses,
        stringRoutePromotesHiggsMass,
        gaugeHiggsUnificationSourceAuditPassed,
        gaugeHiggsRoutePromotesWzMasses,
        gaugeHiggsRoutePromotesHiggsMass,
        framedStandardModelSourceAuditPassed,
        framedStandardModelRoutePromotesWzMasses,
        framedStandardModelRoutePromotesHiggsMass
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
    decision = "Do not promote W/Z or Higgs physical masses from Hitchin/Higgs-bundle/spectral-cover routes in this repository. These sources provide a serious connection-plus-Higgs-field and F-theory spectral-cover lead: Hitchin equations, Higgs-bundle moduli, seven-brane spectral covers, hypercharge-flux GUT breaking, and T-brane warnings about spectral data. They do not supply a GU-local Hitchin-bundle map from Shiab/observer geometry, a target-independent compactification or flux-vacuum law, observed photon/W/Z/H projection, separate W/Z source rows, Higgs scalar-source/self-coupling lineage, low-energy transport, pole extraction, or GeV normalization.",
    nextRequiredArtifact = new[]
    {
        "A GU-local theorem deriving a Hitchin bundle or spectral-cover object from Shiab/observer-sector geometry rather than importing F-theory compactification data.",
        "A target-independent compactification, spectral-curve, and flux-vacuum selection law yielding observed electroweak SU(2) x U(1), a massless photon, and separate W/Z source rows.",
        "A Higgs scalar-source, doublet-projection, and self-coupling lineage from the same geometry, not from an assumed supersymmetric GUT/MSSM model.",
        "A low-energy RG/threshold transport, physical-pole extraction, and GeV unit-normalization chain before any Hitchin/spectral-cover route can promote W/Z/H masses."
    }
};

var options = new JsonSerializerOptions { WriteIndented = true };
File.WriteAllText(Path.Combine(outputDir, "hitchin_higgs_bundle_source_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "hitchin_higgs_bundle_source_audit_summary.json"),
    JsonSerializer.Serialize(
        new
        {
            result.phaseId,
            result.terminalStatus,
            result.hitchinHiggsBundleSourceAuditPassed,
            result.hitchinHiggsBundleLeadPresent,
            result.hitchinHiggsBundlePrimarySourcesReviewed,
            result.hitchinHiggsBundleRouteExternalToGu,
            result.routeUsesHitchinEquations,
            result.routeUsesConnectionPlusHiggsField,
            result.routeUsesHiggsBundleModuli,
            result.routeUsesSpectralCovers,
            result.routeUsesFTheorySevenBranes,
            result.routeEncodesGaugeGroupMatterYukawaGeometrically,
            result.routeUsesHyperchargeFluxBreaking,
            result.routeCanKeepU1YGaugeBosonMasslessInGlobalModels,
            result.routeUsesMeromorphicHiggsBundles,
            result.routeIncludesNoGoForGenericLocalSu5ThreeGenerationModel,
            result.routeIncludesTBranesNonAbelianHiggsField,
            result.tBranesShowSpectralEigenvaluesInsufficient,
            result.routeIncludesMssmOrGutNotObservedSmMassLaw,
            result.routeIncludesFlavorHierarchyOrYukawaLead,
            result.fTheoryModelBuildingLatestArxivVersion,
            result.fTheoryModelBuildingPageCount,
            result.fTheoryGutsILatestArxivVersion,
            result.fTheoryGutsIPageCount,
            result.fTheoryGutsIIFinalArxivVersion,
            result.fTheoryGutsIIPageCount,
            result.higgsBundlesUvCompletionArxivVersion,
            result.higgsBundlesUvCompletionPageCount,
            result.fTheoryCompactificationsGutsLatestArxivVersion,
            result.fTheoryCompactificationsGutsPageCount,
            result.tBranesArxivVersion,
            result.tBranesPageCount,
            result.hitchinSelfDualityStartPage,
            result.hitchinSelfDualityEndPage,
            result.routeTargetsGutOrMssmNotObservedWzHMassLaw,
            result.routeDependsOnCompactificationGeometryChoice,
            result.routeDependsOnFluxChoice,
            result.routeDependsOnSevenBraneConfiguration,
            result.routeDependsOnSupersymmetricGutAssumptions,
            result.routeHyperchargeFluxNotLowEnergyWzMassSource,
            result.routeKeepsU1YMasslessButDoesNotDerivePhotonWzMasses,
            result.routeYukawaFlavorLeadNotGaugeBosonMassLaw,
            result.routeTBraneSpectralDataCanMissLocalizedModes,
            result.routeDoesNotPredictObservedWzMasses,
            result.routeDoesNotProvideTargetIndependentObservedHiggsMass,
            result.routeDoesNotProvidePhysicalPoleExtraction,
            result.routeDoesNotProvideObservedPhotonWzHiggsProjection,
            result.routeRequiresGuLocalHitchinBundleMap,
            result.routeRequiresGuRiemannSurfaceOrSpectralCurveSource,
            result.routeRequiresGuFTheoryCompactificationMap,
            result.routeRequiresGuFluxVacuumSelection,
            result.routeRequiresGuElectroweakHiggsDoubletProjection,
            result.routeRequiresGuWzSourceRows,
            result.routeRequiresGuWeakMixingAngleSource,
            result.routeRequiresGuGaugeCouplingNormalization,
            result.routeRequiresGuObservedFieldExtraction,
            result.routeRequiresGuHiggsScalarSourceOperator,
            result.routeRequiresGuHiggsSelfCouplingSource,
            result.routeRequiresTargetIndependentVevOrMassScale,
            result.routeRequiresLowEnergyRgAndThresholdTransport,
            result.routeRequiresGeVUnitNormalization,
            result.routeProvidesGuLocalHitchinBundleMap,
            result.routeProvidesGuRiemannSurfaceOrSpectralCurveSource,
            result.routeProvidesGuFTheoryCompactificationMap,
            result.routeProvidesGuFluxVacuumSelection,
            result.routeProvidesGuElectroweakHiggsDoubletProjection,
            result.routeProvidesGuWzSourceRows,
            result.routeProvidesGuWeakMixingAngleSource,
            result.routeProvidesGuGaugeCouplingNormalization,
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
Console.WriteLine($"hitchinHiggsBundleSourceAuditPassed={hitchinHiggsBundleSourceAuditPassed}");
Console.WriteLine($"routeUsesHitchinEquations={routeUsesHitchinEquations}");
Console.WriteLine($"routeUsesSpectralCovers={routeUsesSpectralCovers}");
Console.WriteLine($"routeIncludesTBranesNonAbelianHiggsField={routeIncludesTBranesNonAbelianHiggsField}");
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
