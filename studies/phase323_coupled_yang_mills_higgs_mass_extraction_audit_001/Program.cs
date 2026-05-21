using System.Text.Json;

const string DefaultOutputDir = "studies/phase323_coupled_yang_mills_higgs_mass_extraction_audit_001/output";
const string Phase201Path = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase224Path = "studies/phase224_electroweak_parameter_dependency_audit_001/output/electroweak_parameter_dependency_audit_summary.json";
const string Phase228Path = "studies/phase228_boson_mass_matrix_extraction_obstruction_audit_001/output/boson_mass_matrix_extraction_obstruction_audit_summary.json";
const string Phase229Path = "studies/phase229_electroweak_vev_source_lineage_obstruction_audit_001/output/electroweak_vev_source_lineage_obstruction_audit_summary.json";
const string Phase256Path = "studies/phase256_observed_field_extraction_intake_contract_001/output/observed_field_extraction_intake_contract_summary.json";
const string Phase295Path = "studies/phase295_observed_field_extraction_contract_candidate_scan_001/output/observed_field_extraction_contract_candidate_scan_summary.json";
const string Phase296Path = "studies/phase296_source_lineage_contract_field_candidate_scan_001/output/source_lineage_contract_field_candidate_scan_summary.json";
const string Phase313Path = "studies/phase313_official_draft_electroweak_projection_map_audit_001/output/official_draft_electroweak_projection_map_audit_summary.json";
const string Phase317Path = "studies/phase317_electroweak_mass_matrix_bridge_source_audit_001/output/electroweak_mass_matrix_bridge_source_audit_summary.json";
const string Phase321Path = "studies/phase321_neutral_electroweak_mixing_source_audit_001/output/neutral_electroweak_mixing_source_audit_summary.json";
const string Phase322Path = "studies/phase322_higgs_upsilon_scalar_source_boundary_audit_001/output/higgs_upsilon_scalar_source_boundary_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE323_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase224 = JsonDocument.Parse(File.ReadAllText(Phase224Path));
using var phase228 = JsonDocument.Parse(File.ReadAllText(Phase228Path));
using var phase229 = JsonDocument.Parse(File.ReadAllText(Phase229Path));
using var phase256 = JsonDocument.Parse(File.ReadAllText(Phase256Path));
using var phase295 = JsonDocument.Parse(File.ReadAllText(Phase295Path));
using var phase296 = JsonDocument.Parse(File.ReadAllText(Phase296Path));
using var phase313 = JsonDocument.Parse(File.ReadAllText(Phase313Path));
using var phase317 = JsonDocument.Parse(File.ReadAllText(Phase317Path));
using var phase321 = JsonDocument.Parse(File.ReadAllText(Phase321Path));
using var phase322 = JsonDocument.Parse(File.ReadAllText(Phase322Path));

const string officialGuSiteUrl = "https://geometricunity.org/";
const string officialGuDraftUrl = "https://geometricunity.nyc3.digitaloceanspaces.com/Geometric_Unity-Draft-April-1st-2021.pdf";
const string officialGuLectureTranscriptUrl = "https://geometricunity.org/2013-oxford-lecture/";
const bool officialGuSiteLatestDraftReferencePresent = true;
const bool officialDraftAppendixLocatesWeakIsospin = true;
const bool officialDraftAppendixLocatesWeakHypercharge = true;
const bool officialDraftAppendixLocatesHiggsField = true;
const bool officialDraftAppendixMapsHiggsPotentialToUpsilonNorm = true;
const bool officialDraftAppendixMapsYangMillsAndHiggsEquationsToDStarUpsilon = true;
const bool officialLectureRecordsSu3Su2U1BreakingNarrative = true;
const bool officialLectureRecordsHiggsAsAsIfMassYukawaPatch = true;
const bool officialPublicSourcesProvideElectroweakVacuumSelectionRule = false;
const bool officialPublicSourcesProvideTargetIndependentVevSource = false;
const bool officialPublicSourcesProvideGaugeCouplingNormalization = false;
const bool officialPublicSourcesProvideHyperchargeCouplingOrWeakAngle = false;
const bool officialPublicSourcesProvideGaugeFixedQuadraticExpansion = false;
const bool officialPublicSourcesProvidePhotonWzHiggsProjectionRows = false;
const bool officialPublicSourcesProvideGeVUnitNormalization = false;
const bool officialPublicSourcesProvideHiggsScalarSelfCouplingSource = false;
const bool officialPublicSourcesProvideCompleteMassEigenstateExtraction = false;
const bool coupledYangMillsHiggsRoutePromotesWzMasses = false;
const bool coupledYangMillsHiggsRoutePromotesHiggsMass = false;
const bool coupledYangMillsHiggsRouteCompletesBosonPredictions = false;
const bool canFillPhase201WzContract = false;
const bool canFillPhase201HiggsContract = false;
const bool canFillPhase256ObservedFieldExtractionContract = false;

var phase201AllRequiredLineagesPromotable = JsonBool(phase201.RootElement, "allRequiredLineagesPromotable") is true;
var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;
var existingEvidenceFound = JsonBool(phase213.RootElement, "existingEvidenceFound") is true;

var phase224Closure = phase224.RootElement.GetProperty("closure");
var electroweakParameterAuditPassed = JsonBool(phase224.RootElement, "electroweakParameterAuditPassed") is true;
var wAbsoluteMassParameterClosure = JsonBool(phase224Closure, "wAbsoluteMassParameterClosure") is true;
var zAbsoluteMassParameterClosure = JsonBool(phase224Closure, "zAbsoluteMassParameterClosure") is true;
var higgsMassParameterClosure = JsonBool(phase224Closure, "higgsMassParameterClosure") is true;
var weakCouplingSourcePromotable = JsonBool(phase224Closure, "weakCouplingSourcePromotable") is true;
var canPromoteExternalElectroweakBridge = JsonBool(phase224Closure, "canPromoteExternalElectroweakBridge") is true;

var bosonMassMatrixExtractionObstructionCertified = JsonBool(phase228.RootElement, "bosonMassMatrixExtractionObstructionCertified") is true;
var bosonMassMatrixExtractionPromotable = JsonBool(phase228.RootElement, "bosonMassMatrixExtractionPromotable") is true;
var massMatrixUnfilledRequirementCount = JsonArray(phase228.RootElement, "extractionRequirements").Count(row => JsonBool(row, "filled") is false);

var electroweakVevSourceLineageObstructionCertified = JsonBool(phase229.RootElement, "electroweakVevSourceLineageObstructionCertified") is true;
var targetIndependentGuVevSourcePromotable = JsonBool(phase229.RootElement, "targetIndependentGuVevSourcePromotable") is true;
var vevSourceUnfilledRequirementCount = JsonArray(phase229.RootElement, "guVevSourceRequirements").Count(row => JsonBool(row, "filled") is false);

var observedFieldExtractionFilledRequiredFieldCount = JsonInt(phase256.RootElement, "filledRequiredFieldCount") ?? -1;
var observedFieldExtractionRequiredFieldCount = JsonInt(phase256.RootElement, "requiredFieldCount") ?? -1;
var observedFieldExtractionContractPromotable = JsonBool(phase256.RootElement, "observedFieldExtractionContractPromotable") is true;
var phase295IntakeReadyObservedFieldExtractionCandidateCount = JsonInt(phase295.RootElement, "intakeReadyObservedFieldExtractionCandidateCount") ?? -1;
var phase295AnyObservedFieldExtractionCandidateFillsContract = JsonBool(phase295.RootElement, "anyObservedFieldExtractionCandidateFillsContract") is true;
var phase296IntakeReadySourceLineageFieldCandidateCount = JsonInt(phase296.RootElement, "intakeReadySourceLineageFieldCandidateCount") ?? -1;
var phase296AnySourceLineageCandidateFillsContract = JsonBool(phase296.RootElement, "anySourceLineageCandidateFillsContract") is true;

var officialDraftElectroweakProjectionMapAuditPassed = JsonBool(phase313.RootElement, "officialDraftElectroweakProjectionMapAuditPassed") is true;
var officialDraftProvidesWeakIsospinLocation = JsonBool(phase313.RootElement, "officialDraftProvidesWeakIsospinLocation") is true;
var officialDraftProvidesWeakHyperchargeLocation = JsonBool(phase313.RootElement, "officialDraftProvidesWeakHyperchargeLocation") is true;
var officialDraftProvidesPhotonZWeinbergRotation = JsonBool(phase313.RootElement, "officialDraftProvidesPhotonZWeinbergRotation") is true;
var officialDraftProvidesElectromagneticUnbrokenGenerator = JsonBool(phase313.RootElement, "officialDraftProvidesElectromagneticUnbrokenGenerator") is true;
var officialDraftProvidesWeakMixingAngleSource = JsonBool(phase313.RootElement, "officialDraftProvidesWeakMixingAngleSource") is true;
var officialDraftProvidesNeutralMassMatrixDiagonalization = JsonBool(phase313.RootElement, "officialDraftProvidesNeutralMassMatrixDiagonalization") is true;
var officialDraftProvidesPhotonMasslessProjectionRow = JsonBool(phase313.RootElement, "officialDraftProvidesPhotonMasslessProjectionRow") is true;
var officialDraftProvidesWChargedProjectionRows = JsonBool(phase313.RootElement, "officialDraftProvidesWChargedProjectionRows") is true;
var officialDraftProvidesZSourceRowProjection = JsonBool(phase313.RootElement, "officialDraftProvidesZSourceRowProjection") is true;
var officialDraftProvidesObservedElectroweakGaugeEmbedding = JsonBool(phase313.RootElement, "officialDraftProvidesObservedElectroweakGaugeEmbedding") is true;

var electroweakMassMatrixBridgeSourceAuditPassed = JsonBool(phase317.RootElement, "electroweakMassMatrixBridgeSourceAuditPassed") is true;
var smMassMatrixProvidesExternalDependencyMap = JsonBool(phase317.RootElement, "smMassMatrixProvidesExternalDependencyMap") is true;
var smMassGenerationRequiresHiggsDoublet = JsonBool(phase317.RootElement, "smMassGenerationRequiresHiggsDoublet") is true;
var smMassGenerationRequiresVev = JsonBool(phase317.RootElement, "smMassGenerationRequiresVev") is true;
var smMassGenerationRequiresWeakCouplingG = JsonBool(phase317.RootElement, "smMassGenerationRequiresWeakCouplingG") is true;
var smMassGenerationRequiresHyperchargeCouplingGPrime = JsonBool(phase317.RootElement, "smMassGenerationRequiresHyperchargeCouplingGPrime") is true;
var smDefinesPhotonZWeinbergRotation = JsonBool(phase317.RootElement, "smDefinesPhotonZWeinbergRotation") is true;
var smDefinesChargedWCombination = JsonBool(phase317.RootElement, "smDefinesChargedWCombination") is true;
var smTreeLevelMwDependsOnGAndV = JsonBool(phase317.RootElement, "smTreeLevelMwDependsOnGAndV") is true;
var smTreeLevelMzDependsOnGAndGPrimeAndV = JsonBool(phase317.RootElement, "smTreeLevelMzDependsOnGAndGPrimeAndV") is true;
var smTreeLevelHiggsMassDependsOnPotentialParameter = JsonBool(phase317.RootElement, "smTreeLevelHiggsMassDependsOnPotentialParameter") is true;
var smMassMatrixPromotesWzMasses = JsonBool(phase317.RootElement, "smMassMatrixPromotesWzMasses") is true;
var smMassMatrixPromotesHiggsMass = JsonBool(phase317.RootElement, "smMassMatrixPromotesHiggsMass") is true;

var neutralElectroweakMixingSourceAuditPassed = JsonBool(phase321.RootElement, "neutralElectroweakMixingSourceAuditPassed") is true;
var lowEnergyHyperchargeSourcePresent = JsonBool(phase321.RootElement, "lowEnergyHyperchargeSourcePresent") is true;
var neutralMixingRouteCompletesBosonPredictions = JsonBool(phase321.RootElement, "neutralMixingRouteCompletesBosonPredictions") is true;

var higgsUpsilonScalarSourceBoundaryAuditPassed = JsonBool(phase322.RootElement, "higgsUpsilonScalarSourceBoundaryAuditPassed") is true;
var officialGuSourcesProvideFixedScalarSourceOperator = JsonBool(phase322.RootElement, "officialGuSourcesProvideFixedScalarSourceOperator") is true;
var officialGuSourcesProvideUpsilonComponentExtractionTheorem = JsonBool(phase322.RootElement, "officialGuSourcesProvideUpsilonComponentExtractionTheorem") is true;
var officialGuSourcesProvideMassiveScalarProfile = JsonBool(phase322.RootElement, "officialGuSourcesProvideMassiveScalarProfile") is true;
var officialGuSourcesProvideQuarticSelfCouplingValue = JsonBool(phase322.RootElement, "officialGuSourcesProvideQuarticSelfCouplingValue") is true;
var higgsUpsilonRouteCompletesBosonPredictions = JsonBool(phase322.RootElement, "higgsUpsilonRouteCompletesBosonPredictions") is true;

var coupledExtractionRequirements = new[]
{
    new Requirement("official-gu-coupled-field-location", "Official public GU sources locate weak isospin, weak hypercharge, the Higgs field, Higgs potential, and Yang-Mills/Higgs second-order equations.", officialDraftAppendixLocatesWeakIsospin && officialDraftAppendixLocatesWeakHypercharge && officialDraftAppendixLocatesHiggsField && officialDraftAppendixMapsHiggsPotentialToUpsilonNorm && officialDraftAppendixMapsYangMillsAndHiggsEquationsToDStarUpsilon),
    new Requirement("standard-model-dependency-shape", "External Standard Model Higgs mechanism supplies the dependency shape for W/Z/H masses after importing a Higgs doublet, VEV, gauge couplings, and scalar-potential parameter.", electroweakMassMatrixBridgeSourceAuditPassed && smMassMatrixProvidesExternalDependencyMap),
    new Requirement("gu-electroweak-vacuum-source", "A target-independent GU vacuum/VEV source and selection rule.", officialPublicSourcesProvideTargetIndependentVevSource && targetIndependentGuVevSourcePromotable),
    new Requirement("gu-gauge-coupling-normalization", "GU-local g, g-prime, or weak-angle source and low-energy transport.", officialPublicSourcesProvideGaugeCouplingNormalization && officialPublicSourcesProvideHyperchargeCouplingOrWeakAngle && !lowEnergyHyperchargeSourcePresent),
    new Requirement("gu-quadratic-mass-expansion", "A gauge-fixed quadratic expansion of the coupled GU Yang-Mills-Higgs/Upsilon system around the physical vacuum.", officialPublicSourcesProvideGaugeFixedQuadraticExpansion),
    new Requirement("gu-mass-eigenstate-projection", "Photon/W/Z/H eigenstate projection rows with observed-field extraction provenance.", officialPublicSourcesProvidePhotonWzHiggsProjectionRows),
    new Requirement("gu-higgs-scalar-self-coupling-source", "A solved Higgs scalar source/operator and self-coupling or excitation relation.", officialPublicSourcesProvideHiggsScalarSelfCouplingSource && officialGuSourcesProvideQuarticSelfCouplingValue),
    new Requirement("phase201-phase256-contract-fill", "Phase201 W/Z and Higgs source-lineage contracts plus Phase256 observed-field extraction contract filled.", canFillPhase201WzContract && canFillPhase201HiggsContract && canFillPhase256ObservedFieldExtractionContract),
};

var checks = new[]
{
    new Check(
        "official-gu-coupled-locations-and-motivation-recorded",
        officialGuSiteLatestDraftReferencePresent
            && officialDraftAppendixLocatesWeakIsospin
            && officialDraftAppendixLocatesWeakHypercharge
            && officialDraftAppendixLocatesHiggsField
            && officialDraftAppendixMapsHiggsPotentialToUpsilonNorm
            && officialDraftAppendixMapsYangMillsAndHiggsEquationsToDStarUpsilon
            && officialLectureRecordsSu3Su2U1BreakingNarrative
            && officialLectureRecordsHiggsAsAsIfMassYukawaPatch
            && officialDraftElectroweakProjectionMapAuditPassed
            && officialDraftProvidesWeakIsospinLocation
            && officialDraftProvidesWeakHyperchargeLocation
            && higgsUpsilonScalarSourceBoundaryAuditPassed,
        $"siteLatestDraft={officialGuSiteLatestDraftReferencePresent}; weakIsospinLocation={officialDraftProvidesWeakIsospinLocation}; weakHyperchargeLocation={officialDraftProvidesWeakHyperchargeLocation}; higgsFieldLocation={officialDraftAppendixLocatesHiggsField}; higgsPotentialUpsilon={officialDraftAppendixMapsHiggsPotentialToUpsilonNorm}; ymHiggsDStarUpsilon={officialDraftAppendixMapsYangMillsAndHiggsEquationsToDStarUpsilon}; lectureBreakingNarrative={officialLectureRecordsSu3Su2U1BreakingNarrative}; lectureHiggsPatch={officialLectureRecordsHiggsAsAsIfMassYukawaPatch}"),
    new Check(
        "standard-higgs-mechanism-dependency-shape-recorded-but-external",
        electroweakMassMatrixBridgeSourceAuditPassed
            && smMassGenerationRequiresHiggsDoublet
            && smMassGenerationRequiresVev
            && smMassGenerationRequiresWeakCouplingG
            && smMassGenerationRequiresHyperchargeCouplingGPrime
            && smDefinesPhotonZWeinbergRotation
            && smDefinesChargedWCombination
            && smTreeLevelMwDependsOnGAndV
            && smTreeLevelMzDependsOnGAndGPrimeAndV
            && smTreeLevelHiggsMassDependsOnPotentialParameter
            && smMassMatrixProvidesExternalDependencyMap
            && !smMassMatrixPromotesWzMasses
            && !smMassMatrixPromotesHiggsMass,
        $"p317Passed={electroweakMassMatrixBridgeSourceAuditPassed}; higgsDoublet={smMassGenerationRequiresHiggsDoublet}; vev={smMassGenerationRequiresVev}; g={smMassGenerationRequiresWeakCouplingG}; gPrime={smMassGenerationRequiresHyperchargeCouplingGPrime}; photonZ={smDefinesPhotonZWeinbergRotation}; chargedW={smDefinesChargedWCombination}; externalDependencyMap={smMassMatrixProvidesExternalDependencyMap}; smPromotesWz={smMassMatrixPromotesWzMasses}; smPromotesHiggs={smMassMatrixPromotesHiggsMass}"),
    new Check(
        "official-public-gu-does-not-supply-coupled-extraction-operators",
        !officialPublicSourcesProvideElectroweakVacuumSelectionRule
            && !officialPublicSourcesProvideTargetIndependentVevSource
            && !officialPublicSourcesProvideGaugeCouplingNormalization
            && !officialPublicSourcesProvideHyperchargeCouplingOrWeakAngle
            && !officialPublicSourcesProvideGaugeFixedQuadraticExpansion
            && !officialPublicSourcesProvidePhotonWzHiggsProjectionRows
            && !officialPublicSourcesProvideGeVUnitNormalization
            && !officialPublicSourcesProvideHiggsScalarSelfCouplingSource
            && !officialPublicSourcesProvideCompleteMassEigenstateExtraction,
        $"vacuumSelection={officialPublicSourcesProvideElectroweakVacuumSelectionRule}; vevSource={officialPublicSourcesProvideTargetIndependentVevSource}; gaugeCoupling={officialPublicSourcesProvideGaugeCouplingNormalization}; hyperchargeOrWeakAngle={officialPublicSourcesProvideHyperchargeCouplingOrWeakAngle}; quadraticExpansion={officialPublicSourcesProvideGaugeFixedQuadraticExpansion}; projectionRows={officialPublicSourcesProvidePhotonWzHiggsProjectionRows}; geVUnits={officialPublicSourcesProvideGeVUnitNormalization}; higgsSelfCoupling={officialPublicSourcesProvideHiggsScalarSelfCouplingSource}; completeExtraction={officialPublicSourcesProvideCompleteMassEigenstateExtraction}"),
    new Check(
        "mass-parameter-vacuum-and-projection-gates-remain-blocked",
        electroweakParameterAuditPassed
            && !wAbsoluteMassParameterClosure
            && !zAbsoluteMassParameterClosure
            && !higgsMassParameterClosure
            && !weakCouplingSourcePromotable
            && !canPromoteExternalElectroweakBridge
            && electroweakVevSourceLineageObstructionCertified
            && !targetIndependentGuVevSourcePromotable
            && vevSourceUnfilledRequirementCount == 5
            && officialDraftElectroweakProjectionMapAuditPassed
            && !officialDraftProvidesPhotonZWeinbergRotation
            && !officialDraftProvidesElectromagneticUnbrokenGenerator
            && !officialDraftProvidesWeakMixingAngleSource
            && !officialDraftProvidesNeutralMassMatrixDiagonalization
            && !officialDraftProvidesPhotonMasslessProjectionRow
            && !officialDraftProvidesWChargedProjectionRows
            && !officialDraftProvidesZSourceRowProjection
            && !officialDraftProvidesObservedElectroweakGaugeEmbedding,
        $"p224Passed={electroweakParameterAuditPassed}; wClosure={wAbsoluteMassParameterClosure}; zClosure={zAbsoluteMassParameterClosure}; higgsClosure={higgsMassParameterClosure}; weakCouplingSource={weakCouplingSourcePromotable}; externalBridge={canPromoteExternalElectroweakBridge}; vevPromotable={targetIndependentGuVevSourcePromotable}; vevMissing={vevSourceUnfilledRequirementCount}; photonZ={officialDraftProvidesPhotonZWeinbergRotation}; weakAngle={officialDraftProvidesWeakMixingAngleSource}; neutralMatrix={officialDraftProvidesNeutralMassMatrixDiagonalization}; observedEmbedding={officialDraftProvidesObservedElectroweakGaugeEmbedding}"),
    new Check(
        "mass-matrix-and-higgs-scalar-source-gates-remain-blocked",
        bosonMassMatrixExtractionObstructionCertified
            && !bosonMassMatrixExtractionPromotable
            && massMatrixUnfilledRequirementCount == 8
            && neutralElectroweakMixingSourceAuditPassed
            && !lowEnergyHyperchargeSourcePresent
            && !neutralMixingRouteCompletesBosonPredictions
            && higgsUpsilonScalarSourceBoundaryAuditPassed
            && !officialGuSourcesProvideFixedScalarSourceOperator
            && !officialGuSourcesProvideUpsilonComponentExtractionTheorem
            && !officialGuSourcesProvideMassiveScalarProfile
            && !officialGuSourcesProvideQuarticSelfCouplingValue
            && !higgsUpsilonRouteCompletesBosonPredictions,
        $"p228Certified={bosonMassMatrixExtractionObstructionCertified}; p228Promotable={bosonMassMatrixExtractionPromotable}; p228Missing={massMatrixUnfilledRequirementCount}; p321Passed={neutralElectroweakMixingSourceAuditPassed}; lowEnergyHypercharge={lowEnergyHyperchargeSourcePresent}; neutralCompletes={neutralMixingRouteCompletesBosonPredictions}; p322Passed={higgsUpsilonScalarSourceBoundaryAuditPassed}; scalarOperator={officialGuSourcesProvideFixedScalarSourceOperator}; upsilonExtraction={officialGuSourcesProvideUpsilonComponentExtractionTheorem}; massiveProfile={officialGuSourcesProvideMassiveScalarProfile}; quartic={officialGuSourcesProvideQuarticSelfCouplingValue}; higgsCompletes={higgsUpsilonRouteCompletesBosonPredictions}"),
    new Check(
        "coupled-route-does-not-fill-source-lineage-or-observed-extraction-contracts",
        !coupledYangMillsHiggsRoutePromotesWzMasses
            && !coupledYangMillsHiggsRoutePromotesHiggsMass
            && !coupledYangMillsHiggsRouteCompletesBosonPredictions
            && !phase201AllRequiredLineagesPromotable
            && !existingEvidenceFound
            && wzMissingFieldCount == 15
            && higgsMissingFieldCount == 14
            && observedFieldExtractionRequiredFieldCount == 20
            && observedFieldExtractionFilledRequiredFieldCount == 0
            && !observedFieldExtractionContractPromotable
            && phase295IntakeReadyObservedFieldExtractionCandidateCount == 0
            && !phase295AnyObservedFieldExtractionCandidateFillsContract
            && phase296IntakeReadySourceLineageFieldCandidateCount == 0
            && !phase296AnySourceLineageCandidateFillsContract
            && !canFillPhase201WzContract
            && !canFillPhase201HiggsContract
            && !canFillPhase256ObservedFieldExtractionContract,
        $"routePromotesWz={coupledYangMillsHiggsRoutePromotesWzMasses}; routePromotesHiggs={coupledYangMillsHiggsRoutePromotesHiggsMass}; routeCompletes={coupledYangMillsHiggsRouteCompletesBosonPredictions}; phase201AllLineages={phase201AllRequiredLineagesPromotable}; existingEvidence={existingEvidenceFound}; wzMissing={wzMissingFieldCount}; higgsMissing={higgsMissingFieldCount}; phase256Filled={observedFieldExtractionFilledRequiredFieldCount}; p295IntakeReady={phase295IntakeReadyObservedFieldExtractionCandidateCount}; p296IntakeReady={phase296IntakeReadySourceLineageFieldCandidateCount}"),
};

var coupledYangMillsHiggsMassExtractionAuditPassed = checks.All(check => check.Passed)
    && coupledExtractionRequirements.Count(requirement => requirement.Filled) == 2
    && !coupledYangMillsHiggsRoutePromotesWzMasses
    && !coupledYangMillsHiggsRoutePromotesHiggsMass
    && !coupledYangMillsHiggsRouteCompletesBosonPredictions
    && !canFillPhase201WzContract
    && !canFillPhase201HiggsContract
    && !canFillPhase256ObservedFieldExtractionContract;
var terminalStatus = coupledYangMillsHiggsMassExtractionAuditPassed
    ? "coupled-yang-mills-higgs-mass-extraction-audit-location-only-not-source"
    : "coupled-yang-mills-higgs-mass-extraction-audit-review-required";

var result = new
{
    phaseId = "phase323-coupled-yang-mills-higgs-mass-extraction-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    coupledYangMillsHiggsMassExtractionAuditPassed,
    officialGuSiteLatestDraftReferencePresent,
    officialDraftAppendixLocatesWeakIsospin,
    officialDraftAppendixLocatesWeakHypercharge,
    officialDraftAppendixLocatesHiggsField,
    officialDraftAppendixMapsHiggsPotentialToUpsilonNorm,
    officialDraftAppendixMapsYangMillsAndHiggsEquationsToDStarUpsilon,
    officialLectureRecordsSu3Su2U1BreakingNarrative,
    officialLectureRecordsHiggsAsAsIfMassYukawaPatch,
    officialPublicSourcesProvideElectroweakVacuumSelectionRule,
    officialPublicSourcesProvideTargetIndependentVevSource,
    officialPublicSourcesProvideGaugeCouplingNormalization,
    officialPublicSourcesProvideHyperchargeCouplingOrWeakAngle,
    officialPublicSourcesProvideGaugeFixedQuadraticExpansion,
    officialPublicSourcesProvidePhotonWzHiggsProjectionRows,
    officialPublicSourcesProvideGeVUnitNormalization,
    officialPublicSourcesProvideHiggsScalarSelfCouplingSource,
    officialPublicSourcesProvideCompleteMassEigenstateExtraction,
    coupledYangMillsHiggsRoutePromotesWzMasses,
    coupledYangMillsHiggsRoutePromotesHiggsMass,
    coupledYangMillsHiggsRouteCompletesBosonPredictions,
    canFillPhase201WzContract,
    canFillPhase201HiggsContract,
    canFillPhase256ObservedFieldExtractionContract,
    externalSources = new[]
    {
        new ExternalSource(
            "geometric-unity-official-site-current-draft",
            "Geometric Unity official site",
            officialGuSiteUrl,
            "The official site still identifies the April 1, 2021 manuscript as the latest draft source."),
        new ExternalSource(
            "geometric-unity-draft-appendix-locations",
            "Geometric Unity Author's Working Draft v1.0 appendix",
            officialGuDraftUrl,
            "The appendix locates weak isospin, weak hypercharge, the Higgs field, the Higgs potential, and Yang-Mills/Higgs equations in GU notation, but it does not provide physical W/Z/H source rows or mass-eigenstate extraction."),
        new ExternalSource(
            "geometric-unity-2013-oxford-lecture",
            "2013 Oxford lecture transcript",
            officialGuLectureTranscriptUrl,
            "The lecture states the standard SU(3)xSU(2)xU(1) breaking context and frames Higgs mass generation as a patch for chiral lightness, but it does not supply the missing GU mass-extraction theorem."),
    },
    coupledExtractionRequirements,
    inheritedEvidence = new
    {
        phase224 = new
        {
            path = Phase224Path,
            electroweakParameterAuditPassed,
            wAbsoluteMassParameterClosure,
            zAbsoluteMassParameterClosure,
            higgsMassParameterClosure,
            weakCouplingSourcePromotable,
            canPromoteExternalElectroweakBridge,
        },
        phase228 = new
        {
            path = Phase228Path,
            bosonMassMatrixExtractionObstructionCertified,
            bosonMassMatrixExtractionPromotable,
            massMatrixUnfilledRequirementCount,
        },
        phase229 = new
        {
            path = Phase229Path,
            electroweakVevSourceLineageObstructionCertified,
            targetIndependentGuVevSourcePromotable,
            vevSourceUnfilledRequirementCount,
        },
        phase313 = new
        {
            path = Phase313Path,
            officialDraftElectroweakProjectionMapAuditPassed,
            officialDraftProvidesWeakIsospinLocation,
            officialDraftProvidesWeakHyperchargeLocation,
            officialDraftProvidesPhotonZWeinbergRotation,
            officialDraftProvidesElectromagneticUnbrokenGenerator,
            officialDraftProvidesWeakMixingAngleSource,
            officialDraftProvidesNeutralMassMatrixDiagonalization,
            officialDraftProvidesPhotonMasslessProjectionRow,
            officialDraftProvidesWChargedProjectionRows,
            officialDraftProvidesZSourceRowProjection,
            officialDraftProvidesObservedElectroweakGaugeEmbedding,
        },
        phase317 = new
        {
            path = Phase317Path,
            electroweakMassMatrixBridgeSourceAuditPassed,
            smMassGenerationRequiresHiggsDoublet,
            smMassGenerationRequiresVev,
            smMassGenerationRequiresWeakCouplingG,
            smMassGenerationRequiresHyperchargeCouplingGPrime,
            smDefinesPhotonZWeinbergRotation,
            smDefinesChargedWCombination,
            smTreeLevelMwDependsOnGAndV,
            smTreeLevelMzDependsOnGAndGPrimeAndV,
            smTreeLevelHiggsMassDependsOnPotentialParameter,
            smMassMatrixProvidesExternalDependencyMap,
            smMassMatrixPromotesWzMasses,
            smMassMatrixPromotesHiggsMass,
        },
        phase321 = new
        {
            path = Phase321Path,
            neutralElectroweakMixingSourceAuditPassed,
            lowEnergyHyperchargeSourcePresent,
            neutralMixingRouteCompletesBosonPredictions,
        },
        phase322 = new
        {
            path = Phase322Path,
            higgsUpsilonScalarSourceBoundaryAuditPassed,
            officialGuSourcesProvideFixedScalarSourceOperator,
            officialGuSourcesProvideUpsilonComponentExtractionTheorem,
            officialGuSourcesProvideMassiveScalarProfile,
            officialGuSourcesProvideQuarticSelfCouplingValue,
            higgsUpsilonRouteCompletesBosonPredictions,
        },
        contracts = new
        {
            phase201Path = Phase201Path,
            phase213Path = Phase213Path,
            phase256Path = Phase256Path,
            phase295Path = Phase295Path,
            phase296Path = Phase296Path,
            phase201AllRequiredLineagesPromotable,
            existingEvidenceFound,
            wzMissingFieldCount,
            higgsMissingFieldCount,
            observedFieldExtractionRequiredFieldCount,
            observedFieldExtractionFilledRequiredFieldCount,
            observedFieldExtractionContractPromotable,
            phase295IntakeReadyObservedFieldExtractionCandidateCount,
            phase295AnyObservedFieldExtractionCandidateFillsContract,
            phase296IntakeReadySourceLineageFieldCandidateCount,
            phase296AnySourceLineageCandidateFillsContract,
        },
    },
    contractImpact = new
    {
        canFillPhase201WzContract,
        canFillPhase201HiggsContract,
        canFillPhase256ObservedFieldExtractionContract,
        wzMissingFieldCount,
        higgsMissingFieldCount,
        observedFieldExtractionFilledRequiredFieldCount,
        observedFieldExtractionContractPromotable,
    },
    checks,
    decision = coupledYangMillsHiggsMassExtractionAuditPassed
        ? "Do not promote W/Z/H masses from the coupled official GU Yang-Mills-Higgs/Upsilon placement. The public sources provide location and motivation evidence, and the Standard Model supplies an external dependency shape, but the route still lacks a GU-derived electroweak vacuum/VEV, gauge-coupling and weak-angle normalization, gauge-fixed quadratic expansion, photon/W/Z/H projection rows, GeV unit normalization, and solved Higgs scalar self-coupling/source lineage."
        : "Review the coupled Yang-Mills-Higgs mass-extraction audit before using it as boson prediction evidence.",
    nextRequiredArtifact = new[]
    {
        "A target-independent GU electroweak vacuum/VEV source and selection rule.",
        "GU-local low-energy g, g-prime or weak-angle normalization and transport.",
        "A gauge-fixed quadratic expansion of the coupled GU Yang-Mills-Higgs/Upsilon system.",
        "Observed photon, W, Z, and Higgs mass-eigenstate projection rows with provenance.",
        "A solved Higgs scalar source/operator and self-coupling or scalar-excitation relation.",
        "Filled Phase201 and Phase256 contract rows that pass target-blindness, stability, replay, and target-comparison gates.",
    },
    sourceEvidence = new
    {
        phase201Path = Phase201Path,
        phase213Path = Phase213Path,
        phase224Path = Phase224Path,
        phase228Path = Phase228Path,
        phase229Path = Phase229Path,
        phase256Path = Phase256Path,
        phase295Path = Phase295Path,
        phase296Path = Phase296Path,
        phase313Path = Phase313Path,
        phase317Path = Phase317Path,
        phase321Path = Phase321Path,
        phase322Path = Phase322Path,
        officialGuSiteUrl,
        officialGuDraftUrl,
        officialGuLectureTranscriptUrl,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true };
File.WriteAllText(
    Path.Combine(outputDir, "coupled_yang_mills_higgs_mass_extraction_audit.json"),
    JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "coupled_yang_mills_higgs_mass_extraction_audit_summary.json"),
    JsonSerializer.Serialize(result, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"coupledYangMillsHiggsMassExtractionAuditPassed={coupledYangMillsHiggsMassExtractionAuditPassed}");
Console.WriteLine($"officialPublicSourcesProvideGaugeFixedQuadraticExpansion={officialPublicSourcesProvideGaugeFixedQuadraticExpansion}");
Console.WriteLine($"officialPublicSourcesProvidePhotonWzHiggsProjectionRows={officialPublicSourcesProvidePhotonWzHiggsProjectionRows}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");
Console.WriteLine($"canFillPhase201HiggsContract={canFillPhase201HiggsContract}");

static IReadOnlyList<JsonElement> JsonArray(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Array
        ? value.EnumerateArray().ToArray()
        : Array.Empty<JsonElement>();

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var i)
        ? i
        : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind is JsonValueKind.True or JsonValueKind.False
        ? value.GetBoolean()
        : null;

public sealed record Requirement(string RequirementId, string Detail, bool Filled);

public sealed record Check(string CheckId, bool Passed, string Detail);

public sealed record ExternalSource(string SourceId, string Title, string Url, string Finding);
