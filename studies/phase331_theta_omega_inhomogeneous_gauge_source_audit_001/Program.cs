using System.Text.Json;

const string DefaultOutputDir = "studies/phase331_theta_omega_inhomogeneous_gauge_source_audit_001/output";
const string Phase201Path = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase255Path = "studies/phase255_observed_field_extraction_no_go_audit_001/output/observed_field_extraction_no_go_audit_summary.json";
const string Phase256Path = "studies/phase256_observed_field_extraction_intake_contract_001/output/observed_field_extraction_intake_contract_summary.json";
const string Phase267Path = "studies/phase267_completion_revision_direct_bridge_source_audit_001/output/completion_revision_direct_bridge_source_audit_summary.json";
const string Phase313Path = "studies/phase313_official_draft_electroweak_projection_map_audit_001/output/official_draft_electroweak_projection_map_audit_summary.json";
const string Phase315Path = "studies/phase315_ucsd_dark_geometric_energy_source_audit_001/output/ucsd_dark_geometric_energy_source_audit_summary.json";
const string Phase317Path = "studies/phase317_electroweak_mass_matrix_bridge_source_audit_001/output/electroweak_mass_matrix_bridge_source_audit_summary.json";
const string Phase323Path = "studies/phase323_coupled_yang_mills_higgs_mass_extraction_audit_001/output/coupled_yang_mills_higgs_mass_extraction_audit_summary.json";
const string Phase330Path = "studies/phase330_weyl_geometric_mass_generation_source_audit_001/output/weyl_geometric_mass_generation_source_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE331_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase255 = JsonDocument.Parse(File.ReadAllText(Phase255Path));
using var phase256 = JsonDocument.Parse(File.ReadAllText(Phase256Path));
using var phase267 = JsonDocument.Parse(File.ReadAllText(Phase267Path));
using var phase313 = JsonDocument.Parse(File.ReadAllText(Phase313Path));
using var phase315 = JsonDocument.Parse(File.ReadAllText(Phase315Path));
using var phase317 = JsonDocument.Parse(File.ReadAllText(Phase317Path));
using var phase323 = JsonDocument.Parse(File.ReadAllText(Phase323Path));
using var phase330 = JsonDocument.Parse(File.ReadAllText(Phase330Path));

const string officialDraftUrl = "https://geometricunity.nyc3.digitaloceanspaces.com/Geometric_Unity-Draft-April-1st-2021.pdf";
const string officialDraftDownloadPageUrl = "https://geometricunity.org/pull-that-up-jamie/";
const string officialOxfordLectureUrl = "https://geometricunity.org/2013-oxford-lecture/";
const string officialUcsdDarkGeometricEnergyUrl = "https://theportal.group/from-dark-to-geometric-energy-a-sector-of-geometric-unity/";

const bool officialDraftPrimarySourceReviewed = true;
const bool officialDraftDownloadPageReviewed = true;
const bool officialOxfordLectureReviewed = true;
const bool officialUcsdDarkGeometricEnergyAbstractReviewed = true;
const bool thetaOmegaInhomogeneousGaugeRouteGuNative = true;
const bool thetaOmegaInhomogeneousGaugeRouteTargetIndependentAsGeometry = true;
const bool thetaOmegaRouteMentionsDiracSpinorBundle = true;
const bool thetaOmegaRouteMentionsFourteenDimensionalLorentzianMetricSpace = true;
const bool thetaOmegaRouteMentionsSupersymmetricEinsteinDirac = true;
const bool thetaOmegaRouteLocatesWeakIsospinAndHypercharge = true;
const bool thetaOmegaRouteLocatesHiggsAndPotential = true;
const bool thetaOmegaRouteDescribesBosonicVariablesNativeToY = true;
const bool thetaOmegaRouteGivesResearchLeadForSourceLaw = true;

const bool thetaOmegaRouteProvidesDirectTargetIndependentWzBridgeSourceLaw = false;
const bool thetaOmegaRouteProvidesGuLocalWzTheorem = false;
const bool thetaOmegaRouteProvidesSeparateWzSourceRows = false;
const bool thetaOmegaRouteProvidesWzRawAmplitudeGates = false;
const bool thetaOmegaRouteProvidesWzCommonBridgeGate = false;
const bool thetaOmegaRouteProvidesTargetIndependentVevSource = false;
const bool thetaOmegaRouteProvidesWeakMixingAngleSource = false;
const bool thetaOmegaRouteProvidesGaugeCouplingNormalization = false;
const bool thetaOmegaRouteProvidesObservedPhotonWzProjectionRows = false;
const bool thetaOmegaRouteProvidesNeutralMassMatrixDiagonalization = false;
const bool thetaOmegaRouteProvidesObservedFieldExtraction = false;
const bool thetaOmegaRouteProvidesHiggsScalarSourceOperator = false;
const bool thetaOmegaRouteProvidesHiggsQuarticOrExcitationSource = false;
const bool thetaOmegaRouteProvidesHiggsMassiveScalarProfile = false;
const bool thetaOmegaRouteProvidesGeVUnitNormalization = false;
const bool thetaOmegaRoutePromotesWzMasses = false;
const bool thetaOmegaRoutePromotesHiggsMass = false;
const bool thetaOmegaRouteCompletesBosonPredictions = false;
const bool canFillPhase201WzContract = false;
const bool canFillPhase201HiggsContract = false;
const bool canFillPhase256ObservedFieldExtractionContract = false;

var phase201AllRequiredLineagesPromotable = JsonBool(phase201.RootElement, "allRequiredLineagesPromotable") is true;
var phase201AnySourceLineagePromotable = JsonBool(phase201.RootElement, "anySourceLineagePromotable") is true;
var existingEvidenceFound = JsonBool(phase213.RootElement, "existingEvidenceFound") is true;
var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;

var observedFieldExtractionNoGoPassed = JsonBool(phase255.RootElement, "observedFieldExtractionNoGoPassed") is true;
var observedFieldExtractionBridgePromotable = JsonBool(phase255.RootElement, "observedFieldExtractionBridgePromotable") is true;
var newObservedFieldExtractionArtifactRequired = JsonBool(phase255.RootElement, "newObservedFieldExtractionArtifactRequired") is true;
var observedFieldExtractionRequiredFieldCount = JsonInt(phase256.RootElement, "requiredFieldCount") ?? -1;
var observedFieldExtractionFilledRequiredFieldCount = JsonInt(phase256.RootElement, "filledRequiredFieldCount") ?? -1;
var observedFieldExtractionContractPromotable = JsonBool(phase256.RootElement, "observedFieldExtractionContractPromotable") is true;

var completionRevisionDirectBridgeSourceAuditPassed = JsonBool(phase267.RootElement, "completionRevisionDirectBridgeSourceAuditPassed") is true;
var latestCompletionProvidesDirectWzTheorem = JsonBool(phase267.RootElement, "latestCompletionProvidesDirectWzTheorem") is true;
var latestCompletionProvidesObservedFieldExtractionTheorem = JsonBool(phase267.RootElement, "latestCompletionProvidesObservedFieldExtractionTheorem") is true;
var latestCompletionProvidesQuantitativeMassScaleSource = JsonBool(phase267.RootElement, "latestCompletionProvidesQuantitativeMassScaleSource") is true;
var latestCompletionProvidesHiggsScalarSource = JsonBool(phase267.RootElement, "latestCompletionProvidesHiggsScalarSource") is true;
var latestCompletionCompletesBosonPredictions = JsonBool(phase267.RootElement, "latestCompletionCompletesBosonPredictions") is true;

var officialDraftElectroweakProjectionMapAuditPassed = JsonBool(phase313.RootElement, "officialDraftElectroweakProjectionMapAuditPassed") is true;
var officialGuParameterLocationLeadPresent = JsonBool(phase313.RootElement, "officialGuParameterLocationLeadPresent") is true;
var officialDraftProvidesWeakIsospinLocation = JsonBool(phase313.RootElement, "officialDraftProvidesWeakIsospinLocation") is true;
var officialDraftProvidesWeakHyperchargeLocation = JsonBool(phase313.RootElement, "officialDraftProvidesWeakHyperchargeLocation") is true;
var officialDraftProvidesPhotonZWeinbergRotation = JsonBool(phase313.RootElement, "officialDraftProvidesPhotonZWeinbergRotation") is true;
var officialDraftProvidesWeakMixingAngleSource = JsonBool(phase313.RootElement, "officialDraftProvidesWeakMixingAngleSource") is true;
var officialDraftProvidesNeutralMassMatrixDiagonalization = JsonBool(phase313.RootElement, "officialDraftProvidesNeutralMassMatrixDiagonalization") is true;
var officialDraftProvidesWChargedProjectionRows = JsonBool(phase313.RootElement, "officialDraftProvidesWChargedProjectionRows") is true;
var officialDraftProvidesZSourceRowProjection = JsonBool(phase313.RootElement, "officialDraftProvidesZSourceRowProjection") is true;
var officialDraftProjectionMapCompletesObservedFieldExtraction = JsonBool(phase313.RootElement, "officialDraftProjectionMapCompletesObservedFieldExtraction") is true;
var officialDraftProjectionMapPromotesWzMasses = JsonBool(phase313.RootElement, "officialDraftProjectionMapPromotesWzMasses") is true;

var ucsdDarkGeometricEnergySourceAuditPassed = JsonBool(phase315.RootElement, "ucsdDarkGeometricEnergySourceAuditPassed") is true;
var ucsdDarkGeometricEnergyMentionsThetaOmega = JsonBool(phase315.RootElement, "ucsdDarkGeometricEnergyMentionsThetaOmega") is true;
var ucsdDarkGeometricEnergyMentionsInhomogeneousGaugeGroup = JsonBool(phase315.RootElement, "ucsdDarkGeometricEnergyMentionsInhomogeneousGaugeGroup") is true;
var ucsdDarkGeometricEnergyMentionsFourteenDimensionalMetrics = JsonBool(phase315.RootElement, "ucsdDarkGeometricEnergyMentionsFourteenDimensionalMetrics") is true;
var ucsdDarkGeometricEnergyMentionsSupersymmetricEinsteinDirac = JsonBool(phase315.RootElement, "ucsdDarkGeometricEnergyMentionsSupersymmetricEinsteinDirac") is true;
var ucsdDarkGeometricEnergyPromotesWzMasses = JsonBool(phase315.RootElement, "ucsdDarkGeometricEnergyPromotesWzMasses") is true;
var ucsdDarkGeometricEnergyPromotesHiggsMass = JsonBool(phase315.RootElement, "ucsdDarkGeometricEnergyPromotesHiggsMass") is true;
var ucsdDarkGeometricEnergyCompletesBosonPredictions = JsonBool(phase315.RootElement, "ucsdDarkGeometricEnergyCompletesBosonPredictions") is true;

var electroweakMassMatrixBridgeSourceAuditPassed = JsonBool(phase317.RootElement, "electroweakMassMatrixBridgeSourceAuditPassed") is true;
var smMassGenerationRequiresVev = JsonBool(phase317.RootElement, "smMassGenerationRequiresVev") is true;
var smMassGenerationRequiresWeakCouplingG = JsonBool(phase317.RootElement, "smMassGenerationRequiresWeakCouplingG") is true;
var smMassGenerationRequiresHyperchargeCouplingGPrime = JsonBool(phase317.RootElement, "smMassGenerationRequiresHyperchargeCouplingGPrime") is true;
var smDefinesPhotonZWeinbergRotation = JsonBool(phase317.RootElement, "smDefinesPhotonZWeinbergRotation") is true;
var smTreeLevelMwDependsOnGAndV = JsonBool(phase317.RootElement, "smTreeLevelMwDependsOnGAndV") is true;
var smTreeLevelMzDependsOnGAndGPrimeAndV = JsonBool(phase317.RootElement, "smTreeLevelMzDependsOnGAndGPrimeAndV") is true;
var smTreeLevelHiggsMassDependsOnPotentialParameter = JsonBool(phase317.RootElement, "smTreeLevelHiggsMassDependsOnPotentialParameter") is true;
var smMassMatrixPromotesWzMasses = JsonBool(phase317.RootElement, "smMassMatrixPromotesWzMasses") is true;
var smMassMatrixPromotesHiggsMass = JsonBool(phase317.RootElement, "smMassMatrixPromotesHiggsMass") is true;

var coupledYangMillsHiggsMassExtractionAuditPassed = JsonBool(phase323.RootElement, "coupledYangMillsHiggsMassExtractionAuditPassed") is true;
var officialDraftAppendixLocatesWeakIsospin = JsonBool(phase323.RootElement, "officialDraftAppendixLocatesWeakIsospin") is true;
var officialDraftAppendixLocatesWeakHypercharge = JsonBool(phase323.RootElement, "officialDraftAppendixLocatesWeakHypercharge") is true;
var officialDraftAppendixLocatesHiggsField = JsonBool(phase323.RootElement, "officialDraftAppendixLocatesHiggsField") is true;
var officialDraftAppendixMapsHiggsPotentialToUpsilonNorm = JsonBool(phase323.RootElement, "officialDraftAppendixMapsHiggsPotentialToUpsilonNorm") is true;
var officialPublicSourcesProvideTargetIndependentVevSource = JsonBool(phase323.RootElement, "officialPublicSourcesProvideTargetIndependentVevSource") is true;
var officialPublicSourcesProvideGaugeCouplingNormalization = JsonBool(phase323.RootElement, "officialPublicSourcesProvideGaugeCouplingNormalization") is true;
var officialPublicSourcesProvideHyperchargeCouplingOrWeakAngle = JsonBool(phase323.RootElement, "officialPublicSourcesProvideHyperchargeCouplingOrWeakAngle") is true;
var officialPublicSourcesProvideGaugeFixedQuadraticExpansion = JsonBool(phase323.RootElement, "officialPublicSourcesProvideGaugeFixedQuadraticExpansion") is true;
var officialPublicSourcesProvidePhotonWzHiggsProjectionRows = JsonBool(phase323.RootElement, "officialPublicSourcesProvidePhotonWzHiggsProjectionRows") is true;
var officialPublicSourcesProvideHiggsScalarSelfCouplingSource = JsonBool(phase323.RootElement, "officialPublicSourcesProvideHiggsScalarSelfCouplingSource") is true;
var officialPublicSourcesProvideCompleteMassEigenstateExtraction = JsonBool(phase323.RootElement, "officialPublicSourcesProvideCompleteMassEigenstateExtraction") is true;
var coupledYangMillsHiggsRouteCompletesBosonPredictions = JsonBool(phase323.RootElement, "coupledYangMillsHiggsRouteCompletesBosonPredictions") is true;

var weylGeometricMassGenerationSourceAuditPassed = JsonBool(phase330.RootElement, "weylGeometricMassGenerationSourceAuditPassed") is true;
var weylRoutePromotesWzMasses = JsonBool(phase330.RootElement, "weylRoutePromotesWzMasses") is true;
var weylRoutePromotesHiggsMass = JsonBool(phase330.RootElement, "weylRoutePromotesHiggsMass") is true;
var weylRouteCompletesBosonPredictions = JsonBool(phase330.RootElement, "weylRouteCompletesBosonPredictions") is true;

var sourceRows = new[]
{
    new SourceRow(
        "official-gu-draft-download-page",
        officialDraftDownloadPageUrl,
        "official Geometric Unity source list",
        "The official site identifies the April 1, 2021 working draft as the latest public manuscript source.",
        "This makes the draft relevant source material, but does not by itself fill any W/Z/H prediction row."),
    new SourceRow(
        "official-gu-draft-field-location",
        officialDraftUrl,
        "appendix electroweak/Higgs notation and field-equation locations",
        "The draft locates weak isospin, weak hypercharge, Higgs-field, Higgs-potential, Yang-Mills, and Higgs-equation notation in the GU framework.",
        "Field-location notation is useful source context, but Phase313 and Phase323 show it stops before observed photon/W/Z/H projection and mass-eigenstate extraction."),
    new SourceRow(
        "official-gu-draft-energy-scale-limitation",
        officialDraftUrl,
        "main text discussion of thresholds and QFT sharpening",
        "The draft frames energy-threshold sharpening and detailed dynamical prediction as requiring further quantum-field-theoretic work.",
        "That is incompatible with promoting absolute W/Z/H masses from the draft alone."),
    new SourceRow(
        "official-oxford-lecture-higgs-context",
        officialOxfordLectureUrl,
        "2013 Oxford lecture transcript",
        "The official lecture records the Standard Model SU(3) x SU(2) x U(1) breaking context and describes the Higgs mechanism as an as-if mass/Yukawa patch.",
        "It motivates the geometric problem but does not supply a direct target-independent W/Z bridge-source law."),
    new SourceRow(
        "portal-ucsd-theta-omega-abstract",
        officialUcsdDarkGeometricEnergyUrl,
        "April 2025 UCSD seminar abstract",
        "The official Portal page records theta_omega, an inhomogeneous gauge group over the Dirac spinor bundle, 14-dimensional Lorentzian-metric geometry, and a supersymmetric Einstein-Dirac extension.",
        "This is the strongest public GU-native lead for the requested geometry, but Phase315 records it as abstract-level structural evidence with no transcript-level W/Z/H source rows."),
};

var checks = new[]
{
    new Check(
        "theta-omega-official-source-lead-captured",
        officialDraftPrimarySourceReviewed
            && officialDraftDownloadPageReviewed
            && officialOxfordLectureReviewed
            && officialUcsdDarkGeometricEnergyAbstractReviewed
            && ucsdDarkGeometricEnergySourceAuditPassed
            && ucsdDarkGeometricEnergyMentionsThetaOmega
            && ucsdDarkGeometricEnergyMentionsInhomogeneousGaugeGroup
            && ucsdDarkGeometricEnergyMentionsFourteenDimensionalMetrics
            && ucsdDarkGeometricEnergyMentionsSupersymmetricEinsteinDirac
            && sourceRows.Length == 5,
        $"draftReviewed={officialDraftPrimarySourceReviewed}; ucsdPassed={ucsdDarkGeometricEnergySourceAuditPassed}; thetaOmega={ucsdDarkGeometricEnergyMentionsThetaOmega}; inhomogeneousGauge={ucsdDarkGeometricEnergyMentionsInhomogeneousGaugeGroup}; fourteenDimensionalMetrics={ucsdDarkGeometricEnergyMentionsFourteenDimensionalMetrics}; supersymmetricEinsteinDirac={ucsdDarkGeometricEnergyMentionsSupersymmetricEinsteinDirac}; sourceRows={sourceRows.Length}"),
    new Check(
        "official-draft-geometry-is-location-not-observed-electroweak-map",
        officialDraftElectroweakProjectionMapAuditPassed
            && officialGuParameterLocationLeadPresent
            && officialDraftProvidesWeakIsospinLocation
            && officialDraftProvidesWeakHyperchargeLocation
            && coupledYangMillsHiggsMassExtractionAuditPassed
            && officialDraftAppendixLocatesWeakIsospin
            && officialDraftAppendixLocatesWeakHypercharge
            && officialDraftAppendixLocatesHiggsField
            && officialDraftAppendixMapsHiggsPotentialToUpsilonNorm
            && !officialDraftProvidesPhotonZWeinbergRotation
            && !officialDraftProvidesWeakMixingAngleSource
            && !officialDraftProvidesNeutralMassMatrixDiagonalization
            && !officialDraftProvidesWChargedProjectionRows
            && !officialDraftProvidesZSourceRowProjection
            && !officialDraftProjectionMapCompletesObservedFieldExtraction
            && !officialDraftProjectionMapPromotesWzMasses,
        $"p313Passed={officialDraftElectroweakProjectionMapAuditPassed}; weakIsospinLocation={officialDraftProvidesWeakIsospinLocation}; weakHyperchargeLocation={officialDraftProvidesWeakHyperchargeLocation}; higgsLocation={officialDraftAppendixLocatesHiggsField}; photonZRotation={officialDraftProvidesPhotonZWeinbergRotation}; weakAngle={officialDraftProvidesWeakMixingAngleSource}; neutralMatrix={officialDraftProvidesNeutralMassMatrixDiagonalization}; wRows={officialDraftProvidesWChargedProjectionRows}; zRows={officialDraftProvidesZSourceRowProjection}"),
    new Check(
        "theta-omega-route-does-not-supply-direct-wz-bridge-source-law",
        thetaOmegaInhomogeneousGaugeRouteGuNative
            && thetaOmegaInhomogeneousGaugeRouteTargetIndependentAsGeometry
            && thetaOmegaRouteGivesResearchLeadForSourceLaw
            && thetaOmegaRouteLocatesWeakIsospinAndHypercharge
            && !thetaOmegaRouteProvidesDirectTargetIndependentWzBridgeSourceLaw
            && !thetaOmegaRouteProvidesGuLocalWzTheorem
            && !thetaOmegaRouteProvidesSeparateWzSourceRows
            && !thetaOmegaRouteProvidesWzRawAmplitudeGates
            && !thetaOmegaRouteProvidesWzCommonBridgeGate
            && !thetaOmegaRouteProvidesTargetIndependentVevSource
            && !thetaOmegaRouteProvidesWeakMixingAngleSource
            && !thetaOmegaRouteProvidesGaugeCouplingNormalization
            && !thetaOmegaRouteProvidesObservedPhotonWzProjectionRows
            && !thetaOmegaRouteProvidesNeutralMassMatrixDiagonalization
            && !thetaOmegaRoutePromotesWzMasses
            && !canFillPhase201WzContract,
        $"guNative={thetaOmegaInhomogeneousGaugeRouteGuNative}; geometricTargetIndependent={thetaOmegaInhomogeneousGaugeRouteTargetIndependentAsGeometry}; directBridgeLaw={thetaOmegaRouteProvidesDirectTargetIndependentWzBridgeSourceLaw}; guWzTheorem={thetaOmegaRouteProvidesGuLocalWzTheorem}; separateRows={thetaOmegaRouteProvidesSeparateWzSourceRows}; vev={thetaOmegaRouteProvidesTargetIndependentVevSource}; weakAngle={thetaOmegaRouteProvidesWeakMixingAngleSource}; couplingNorm={thetaOmegaRouteProvidesGaugeCouplingNormalization}; observedRows={thetaOmegaRouteProvidesObservedPhotonWzProjectionRows}"),
    new Check(
        "standard-electroweak-dependencies-remain-unsourced-by-gu",
        electroweakMassMatrixBridgeSourceAuditPassed
            && smMassGenerationRequiresVev
            && smMassGenerationRequiresWeakCouplingG
            && smMassGenerationRequiresHyperchargeCouplingGPrime
            && smDefinesPhotonZWeinbergRotation
            && smTreeLevelMwDependsOnGAndV
            && smTreeLevelMzDependsOnGAndGPrimeAndV
            && smTreeLevelHiggsMassDependsOnPotentialParameter
            && !smMassMatrixPromotesWzMasses
            && !smMassMatrixPromotesHiggsMass
            && !officialPublicSourcesProvideTargetIndependentVevSource
            && !officialPublicSourcesProvideGaugeCouplingNormalization
            && !officialPublicSourcesProvideHyperchargeCouplingOrWeakAngle
            && !officialPublicSourcesProvideGaugeFixedQuadraticExpansion,
        $"p317Passed={electroweakMassMatrixBridgeSourceAuditPassed}; requiresV={smMassGenerationRequiresVev}; requiresG={smMassGenerationRequiresWeakCouplingG}; requiresGPrime={smMassGenerationRequiresHyperchargeCouplingGPrime}; mwGv={smTreeLevelMwDependsOnGAndV}; mzGGPrimeV={smTreeLevelMzDependsOnGAndGPrimeAndV}; guVev={officialPublicSourcesProvideTargetIndependentVevSource}; guCouplingNorm={officialPublicSourcesProvideGaugeCouplingNormalization}; guWeakAngle={officialPublicSourcesProvideHyperchargeCouplingOrWeakAngle}; guQuadraticExpansion={officialPublicSourcesProvideGaugeFixedQuadraticExpansion}"),
    new Check(
        "theta-omega-route-does-not-supply-higgs-source-lineage",
        thetaOmegaRouteLocatesHiggsAndPotential
            && officialDraftAppendixLocatesHiggsField
            && officialDraftAppendixMapsHiggsPotentialToUpsilonNorm
            && !thetaOmegaRouteProvidesHiggsScalarSourceOperator
            && !thetaOmegaRouteProvidesHiggsQuarticOrExcitationSource
            && !thetaOmegaRouteProvidesHiggsMassiveScalarProfile
            && !officialPublicSourcesProvideHiggsScalarSelfCouplingSource
            && !thetaOmegaRoutePromotesHiggsMass
            && !canFillPhase201HiggsContract,
        $"locatesHiggs={thetaOmegaRouteLocatesHiggsAndPotential}; draftHiggsField={officialDraftAppendixLocatesHiggsField}; draftHiggsPotential={officialDraftAppendixMapsHiggsPotentialToUpsilonNorm}; higgsOperator={thetaOmegaRouteProvidesHiggsScalarSourceOperator}; quarticOrExcitation={thetaOmegaRouteProvidesHiggsQuarticOrExcitationSource}; massiveProfile={thetaOmegaRouteProvidesHiggsMassiveScalarProfile}; publicSelfCoupling={officialPublicSourcesProvideHiggsScalarSelfCouplingSource}"),
    new Check(
        "observed-field-extraction-and-completion-blockers-remain-binding",
        observedFieldExtractionNoGoPassed
            && !observedFieldExtractionBridgePromotable
            && newObservedFieldExtractionArtifactRequired
            && observedFieldExtractionRequiredFieldCount == 20
            && observedFieldExtractionFilledRequiredFieldCount == 0
            && !observedFieldExtractionContractPromotable
            && completionRevisionDirectBridgeSourceAuditPassed
            && !latestCompletionProvidesDirectWzTheorem
            && !latestCompletionProvidesObservedFieldExtractionTheorem
            && !latestCompletionProvidesQuantitativeMassScaleSource
            && !latestCompletionProvidesHiggsScalarSource
            && !latestCompletionCompletesBosonPredictions
            && !thetaOmegaRouteProvidesObservedFieldExtraction
            && !canFillPhase256ObservedFieldExtractionContract,
        $"p255Passed={observedFieldExtractionNoGoPassed}; p255Promotable={observedFieldExtractionBridgePromotable}; p256Required={observedFieldExtractionRequiredFieldCount}; p256Filled={observedFieldExtractionFilledRequiredFieldCount}; p267Passed={completionRevisionDirectBridgeSourceAuditPassed}; p267DirectWz={latestCompletionProvidesDirectWzTheorem}; p267Extraction={latestCompletionProvidesObservedFieldExtractionTheorem}; p267Scale={latestCompletionProvidesQuantitativeMassScaleSource}; p267Higgs={latestCompletionProvidesHiggsScalarSource}"),
    new Check(
        "source-lineage-contracts-remain-unfilled-after-theta-omega-audit",
        !phase201AnySourceLineagePromotable
            && !phase201AllRequiredLineagesPromotable
            && !existingEvidenceFound
            && wzMissingFieldCount == 15
            && higgsMissingFieldCount == 14
            && !officialPublicSourcesProvidePhotonWzHiggsProjectionRows
            && !officialPublicSourcesProvideCompleteMassEigenstateExtraction
            && weylGeometricMassGenerationSourceAuditPassed
            && !weylRoutePromotesWzMasses
            && !weylRoutePromotesHiggsMass
            && !weylRouteCompletesBosonPredictions
            && !ucsdDarkGeometricEnergyPromotesWzMasses
            && !ucsdDarkGeometricEnergyPromotesHiggsMass
            && !ucsdDarkGeometricEnergyCompletesBosonPredictions
            && !thetaOmegaRouteProvidesGeVUnitNormalization
            && !thetaOmegaRouteCompletesBosonPredictions,
        $"phase201Any={phase201AnySourceLineagePromotable}; phase201All={phase201AllRequiredLineagesPromotable}; existingEvidence={existingEvidenceFound}; wzMissing={wzMissingFieldCount}; higgsMissing={higgsMissingFieldCount}; publicProjectionRows={officialPublicSourcesProvidePhotonWzHiggsProjectionRows}; publicMassExtraction={officialPublicSourcesProvideCompleteMassEigenstateExtraction}; weylCompletes={weylRouteCompletesBosonPredictions}; ucsdCompletes={ucsdDarkGeometricEnergyCompletesBosonPredictions}; thetaOmegaCompletes={thetaOmegaRouteCompletesBosonPredictions}"),
};

var thetaOmegaInhomogeneousGaugeSourceAuditPassed = checks.All(check => check.Passed)
    && thetaOmegaInhomogeneousGaugeRouteGuNative
    && thetaOmegaRouteGivesResearchLeadForSourceLaw
    && !thetaOmegaRouteProvidesDirectTargetIndependentWzBridgeSourceLaw
    && !thetaOmegaRouteProvidesGuLocalWzTheorem
    && !thetaOmegaRouteProvidesSeparateWzSourceRows
    && !thetaOmegaRouteProvidesWzRawAmplitudeGates
    && !thetaOmegaRouteProvidesWzCommonBridgeGate
    && !thetaOmegaRouteProvidesTargetIndependentVevSource
    && !thetaOmegaRouteProvidesWeakMixingAngleSource
    && !thetaOmegaRouteProvidesGaugeCouplingNormalization
    && !thetaOmegaRouteProvidesObservedPhotonWzProjectionRows
    && !thetaOmegaRouteProvidesNeutralMassMatrixDiagonalization
    && !thetaOmegaRouteProvidesObservedFieldExtraction
    && !thetaOmegaRouteProvidesHiggsScalarSourceOperator
    && !thetaOmegaRouteProvidesHiggsQuarticOrExcitationSource
    && !thetaOmegaRouteProvidesHiggsMassiveScalarProfile
    && !thetaOmegaRouteProvidesGeVUnitNormalization
    && !thetaOmegaRoutePromotesWzMasses
    && !thetaOmegaRoutePromotesHiggsMass
    && !thetaOmegaRouteCompletesBosonPredictions
    && !canFillPhase201WzContract
    && !canFillPhase201HiggsContract
    && !canFillPhase256ObservedFieldExtractionContract;

var terminalStatus = thetaOmegaInhomogeneousGaugeSourceAuditPassed
    ? "theta-omega-inhomogeneous-gauge-source-audit-structural-geometry-not-wzh-source"
    : "theta-omega-inhomogeneous-gauge-source-audit-review-required";

var result = new
{
    phaseId = "phase331-theta-omega-inhomogeneous-gauge-source-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    thetaOmegaInhomogeneousGaugeSourceAuditPassed,
    officialDraftPrimarySourceReviewed,
    officialDraftDownloadPageReviewed,
    officialOxfordLectureReviewed,
    officialUcsdDarkGeometricEnergyAbstractReviewed,
    thetaOmegaInhomogeneousGaugeRouteGuNative,
    thetaOmegaInhomogeneousGaugeRouteTargetIndependentAsGeometry,
    thetaOmegaRouteMentionsDiracSpinorBundle,
    thetaOmegaRouteMentionsFourteenDimensionalLorentzianMetricSpace,
    thetaOmegaRouteMentionsSupersymmetricEinsteinDirac,
    thetaOmegaRouteLocatesWeakIsospinAndHypercharge,
    thetaOmegaRouteLocatesHiggsAndPotential,
    thetaOmegaRouteDescribesBosonicVariablesNativeToY,
    thetaOmegaRouteGivesResearchLeadForSourceLaw,
    thetaOmegaRouteProvidesDirectTargetIndependentWzBridgeSourceLaw,
    thetaOmegaRouteProvidesGuLocalWzTheorem,
    thetaOmegaRouteProvidesSeparateWzSourceRows,
    thetaOmegaRouteProvidesWzRawAmplitudeGates,
    thetaOmegaRouteProvidesWzCommonBridgeGate,
    thetaOmegaRouteProvidesTargetIndependentVevSource,
    thetaOmegaRouteProvidesWeakMixingAngleSource,
    thetaOmegaRouteProvidesGaugeCouplingNormalization,
    thetaOmegaRouteProvidesObservedPhotonWzProjectionRows,
    thetaOmegaRouteProvidesNeutralMassMatrixDiagonalization,
    thetaOmegaRouteProvidesObservedFieldExtraction,
    thetaOmegaRouteProvidesHiggsScalarSourceOperator,
    thetaOmegaRouteProvidesHiggsQuarticOrExcitationSource,
    thetaOmegaRouteProvidesHiggsMassiveScalarProfile,
    thetaOmegaRouteProvidesGeVUnitNormalization,
    thetaOmegaRoutePromotesWzMasses,
    thetaOmegaRoutePromotesHiggsMass,
    thetaOmegaRouteCompletesBosonPredictions,
    canFillPhase201WzContract,
    canFillPhase201HiggsContract,
    canFillPhase256ObservedFieldExtractionContract,
    sourceRows,
    checks,
    upstreamBoundaryEvidence = new
    {
        phase313 = new
        {
            officialDraftElectroweakProjectionMapAuditPassed,
            officialGuParameterLocationLeadPresent,
            officialDraftProvidesWeakIsospinLocation,
            officialDraftProvidesWeakHyperchargeLocation,
            officialDraftProvidesPhotonZWeinbergRotation,
            officialDraftProvidesWeakMixingAngleSource,
            officialDraftProvidesNeutralMassMatrixDiagonalization,
            officialDraftProvidesWChargedProjectionRows,
            officialDraftProvidesZSourceRowProjection,
            officialDraftProjectionMapCompletesObservedFieldExtraction,
            officialDraftProjectionMapPromotesWzMasses,
        },
        phase315 = new
        {
            ucsdDarkGeometricEnergySourceAuditPassed,
            ucsdDarkGeometricEnergyMentionsThetaOmega,
            ucsdDarkGeometricEnergyMentionsInhomogeneousGaugeGroup,
            ucsdDarkGeometricEnergyMentionsFourteenDimensionalMetrics,
            ucsdDarkGeometricEnergyMentionsSupersymmetricEinsteinDirac,
            ucsdDarkGeometricEnergyPromotesWzMasses,
            ucsdDarkGeometricEnergyPromotesHiggsMass,
            ucsdDarkGeometricEnergyCompletesBosonPredictions,
        },
        phase317 = new
        {
            electroweakMassMatrixBridgeSourceAuditPassed,
            smMassGenerationRequiresVev,
            smMassGenerationRequiresWeakCouplingG,
            smMassGenerationRequiresHyperchargeCouplingGPrime,
            smDefinesPhotonZWeinbergRotation,
            smTreeLevelMwDependsOnGAndV,
            smTreeLevelMzDependsOnGAndGPrimeAndV,
            smTreeLevelHiggsMassDependsOnPotentialParameter,
            smMassMatrixPromotesWzMasses,
            smMassMatrixPromotesHiggsMass,
        },
        phase323 = new
        {
            coupledYangMillsHiggsMassExtractionAuditPassed,
            officialDraftAppendixLocatesWeakIsospin,
            officialDraftAppendixLocatesWeakHypercharge,
            officialDraftAppendixLocatesHiggsField,
            officialDraftAppendixMapsHiggsPotentialToUpsilonNorm,
            officialPublicSourcesProvideTargetIndependentVevSource,
            officialPublicSourcesProvideGaugeCouplingNormalization,
            officialPublicSourcesProvideHyperchargeCouplingOrWeakAngle,
            officialPublicSourcesProvideGaugeFixedQuadraticExpansion,
            officialPublicSourcesProvidePhotonWzHiggsProjectionRows,
            officialPublicSourcesProvideHiggsScalarSelfCouplingSource,
            officialPublicSourcesProvideCompleteMassEigenstateExtraction,
            coupledYangMillsHiggsRouteCompletesBosonPredictions,
        },
    },
    contractImpact = new
    {
        phase201AnySourceLineagePromotable,
        phase201AllRequiredLineagesPromotable,
        existingEvidenceFound,
        wzMissingFieldCount,
        higgsMissingFieldCount,
        observedFieldExtractionRequiredFieldCount,
        observedFieldExtractionFilledRequiredFieldCount,
        observedFieldExtractionContractPromotable,
        canFillPhase201WzContract,
        canFillPhase201HiggsContract,
        canFillPhase256ObservedFieldExtractionContract,
    },
    decision = thetaOmegaInhomogeneousGaugeSourceAuditPassed
        ? "Do not promote W/Z or Higgs physical masses from the theta_omega / inhomogeneous-gauge route. The route is GU-native and structurally relevant, but the public draft/lecture/abstract evidence currently supplies field locations and geometric research direction, not a direct target-independent W/Z bridge-source law, separate W/Z rows, electroweak VEV, weak-angle/coupling normalization, photon/W/Z/H observed-field extraction, Higgs scalar-source/self-coupling lineage, or GeV normalization."
        : "Review the theta_omega / inhomogeneous-gauge route before relying on this boundary.",
    nextRequiredArtifact = new[]
    {
        "A GU-local theta_omega or inhomogeneous-gauge theorem deriving the observed SU(2)_L x U(1)_Y embedding, weak angle, target-independent VEV, and separate W/Z source rows.",
        "A source-derived observed-field extraction theorem mapping GU-native Y variables to photon, W, Z, and Higgs rows with branch, normalization, and stability sidecars.",
        "A solved Higgs scalar-source/operator and self-coupling or excitation relation with GeV normalization before target comparison.",
    },
    sourceEvidence = new
    {
        officialDraftUrl,
        officialDraftDownloadPageUrl,
        officialOxfordLectureUrl,
        officialUcsdDarkGeometricEnergyUrl,
        phase201Path = Phase201Path,
        phase213Path = Phase213Path,
        phase255Path = Phase255Path,
        phase256Path = Phase256Path,
        phase267Path = Phase267Path,
        phase313Path = Phase313Path,
        phase315Path = Phase315Path,
        phase317Path = Phase317Path,
        phase323Path = Phase323Path,
        phase330Path = Phase330Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "theta_omega_inhomogeneous_gauge_source_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "theta_omega_inhomogeneous_gauge_source_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.thetaOmegaInhomogeneousGaugeSourceAuditPassed,
        result.officialDraftPrimarySourceReviewed,
        result.officialDraftDownloadPageReviewed,
        result.officialOxfordLectureReviewed,
        result.officialUcsdDarkGeometricEnergyAbstractReviewed,
        result.thetaOmegaInhomogeneousGaugeRouteGuNative,
        result.thetaOmegaInhomogeneousGaugeRouteTargetIndependentAsGeometry,
        result.thetaOmegaRouteMentionsDiracSpinorBundle,
        result.thetaOmegaRouteMentionsFourteenDimensionalLorentzianMetricSpace,
        result.thetaOmegaRouteMentionsSupersymmetricEinsteinDirac,
        result.thetaOmegaRouteLocatesWeakIsospinAndHypercharge,
        result.thetaOmegaRouteLocatesHiggsAndPotential,
        result.thetaOmegaRouteDescribesBosonicVariablesNativeToY,
        result.thetaOmegaRouteGivesResearchLeadForSourceLaw,
        result.thetaOmegaRouteProvidesDirectTargetIndependentWzBridgeSourceLaw,
        result.thetaOmegaRouteProvidesGuLocalWzTheorem,
        result.thetaOmegaRouteProvidesSeparateWzSourceRows,
        result.thetaOmegaRouteProvidesWzRawAmplitudeGates,
        result.thetaOmegaRouteProvidesWzCommonBridgeGate,
        result.thetaOmegaRouteProvidesTargetIndependentVevSource,
        result.thetaOmegaRouteProvidesWeakMixingAngleSource,
        result.thetaOmegaRouteProvidesGaugeCouplingNormalization,
        result.thetaOmegaRouteProvidesObservedPhotonWzProjectionRows,
        result.thetaOmegaRouteProvidesNeutralMassMatrixDiagonalization,
        result.thetaOmegaRouteProvidesObservedFieldExtraction,
        result.thetaOmegaRouteProvidesHiggsScalarSourceOperator,
        result.thetaOmegaRouteProvidesHiggsQuarticOrExcitationSource,
        result.thetaOmegaRouteProvidesHiggsMassiveScalarProfile,
        result.thetaOmegaRouteProvidesGeVUnitNormalization,
        result.thetaOmegaRoutePromotesWzMasses,
        result.thetaOmegaRoutePromotesHiggsMass,
        result.thetaOmegaRouteCompletesBosonPredictions,
        result.canFillPhase201WzContract,
        result.canFillPhase201HiggsContract,
        result.canFillPhase256ObservedFieldExtractionContract,
        result.sourceRows,
        result.checks,
        result.contractImpact,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"thetaOmegaInhomogeneousGaugeSourceAuditPassed={thetaOmegaInhomogeneousGaugeSourceAuditPassed}");
Console.WriteLine($"thetaOmegaRouteGuNative={thetaOmegaInhomogeneousGaugeRouteGuNative}");
Console.WriteLine($"thetaOmegaRouteProvidesDirectTargetIndependentWzBridgeSourceLaw={thetaOmegaRouteProvidesDirectTargetIndependentWzBridgeSourceLaw}");
Console.WriteLine($"thetaOmegaRoutePromotesWzMasses={thetaOmegaRoutePromotesWzMasses}");
Console.WriteLine($"thetaOmegaRoutePromotesHiggsMass={thetaOmegaRoutePromotesHiggsMass}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

sealed record SourceRow(string SourceId, string Url, string Locator, string Finding, string PredictionImpact);
sealed record Check(string CheckId, bool Passed, string Detail);
