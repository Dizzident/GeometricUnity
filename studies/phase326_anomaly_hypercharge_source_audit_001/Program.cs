using System.Text.Json;

const string DefaultOutputDir = "studies/phase326_anomaly_hypercharge_source_audit_001/output";
const string Phase201Path = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase224Path = "studies/phase224_electroweak_parameter_dependency_audit_001/output/electroweak_parameter_dependency_audit_summary.json";
const string Phase233Path = "studies/phase233_external_cox_gu_papers_iii_iv_source_intake_audit_001/output/external_cox_gu_papers_iii_iv_source_intake_audit_summary.json";
const string Phase240Path = "studies/phase240_cox_iii_axial_contact_rg_boson_parameter_audit_001/output/cox_iii_axial_contact_rg_boson_parameter_audit_summary.json";
const string Phase244Path = "studies/phase244_electroweak_identifiability_rank_audit_001/output/electroweak_identifiability_rank_audit_summary.json";
const string Phase245Path = "studies/phase245_rank_deficit_minimal_unlock_contract_001/output/rank_deficit_minimal_unlock_contract_summary.json";
const string Phase256Path = "studies/phase256_observed_field_extraction_intake_contract_001/output/observed_field_extraction_intake_contract_summary.json";
const string Phase295Path = "studies/phase295_observed_field_extraction_contract_candidate_scan_001/output/observed_field_extraction_contract_candidate_scan_summary.json";
const string Phase296Path = "studies/phase296_source_lineage_contract_field_candidate_scan_001/output/source_lineage_contract_field_candidate_scan_summary.json";
const string Phase313Path = "studies/phase313_official_draft_electroweak_projection_map_audit_001/output/official_draft_electroweak_projection_map_audit_summary.json";
const string Phase321Path = "studies/phase321_neutral_electroweak_mixing_source_audit_001/output/neutral_electroweak_mixing_source_audit_summary.json";
const string Phase323Path = "studies/phase323_coupled_yang_mills_higgs_mass_extraction_audit_001/output/coupled_yang_mills_higgs_mass_extraction_audit_summary.json";
const string Phase324Path = "studies/phase324_custodial_rho_parameter_source_audit_001/output/custodial_rho_parameter_source_audit_summary.json";
const string Phase325Path = "studies/phase325_electroweak_unitarity_scattering_source_audit_001/output/electroweak_unitarity_scattering_source_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE326_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase224 = JsonDocument.Parse(File.ReadAllText(Phase224Path));
using var phase233 = JsonDocument.Parse(File.ReadAllText(Phase233Path));
using var phase240 = JsonDocument.Parse(File.ReadAllText(Phase240Path));
using var phase244 = JsonDocument.Parse(File.ReadAllText(Phase244Path));
using var phase245 = JsonDocument.Parse(File.ReadAllText(Phase245Path));
using var phase256 = JsonDocument.Parse(File.ReadAllText(Phase256Path));
using var phase295 = JsonDocument.Parse(File.ReadAllText(Phase295Path));
using var phase296 = JsonDocument.Parse(File.ReadAllText(Phase296Path));
using var phase313 = JsonDocument.Parse(File.ReadAllText(Phase313Path));
using var phase321 = JsonDocument.Parse(File.ReadAllText(Phase321Path));
using var phase323 = JsonDocument.Parse(File.ReadAllText(Phase323Path));
using var phase324 = JsonDocument.Parse(File.ReadAllText(Phase324Path));
using var phase325 = JsonDocument.Parse(File.ReadAllText(Phase325Path));

const string bouchiatIliopoulosMeyerOstiUrl = "https://www.osti.gov/biblio/4663309";
const string wittenSu2AnomalyPrincetonUrl = "https://collaborate.princeton.edu/en/publications/an-su2-anomaly/";
const string alvarezGraciaBondiaMartinNcgUrl = "https://arxiv.org/abs/hep-th/9506115";
const bool bouchiatIliopoulosMeyerAnomalyFreeReferencePresent = true;
const bool wittenGlobalSu2AnomalyReferencePresent = true;
const bool ncgAnomalyHyperchargeReferencePresent = true;
const bool anomalyCancellationConstrainsFermionQuantumNumbers = true;
const bool globalSu2AnomalyConstrainsDoubletParity = true;
const bool anomalyCancellationAlmostDeterminesHyperchargesUnderAssumptions = true;
const bool anomalyRouteProvidesQuantumConsistencyConditions = true;
const bool anomalyRouteProvidesRepresentationConstraint = true;
const bool anomalyRouteProvidesLowEnergyHyperchargeSource = false;
const bool anomalyRouteProvidesWeakMixingAngleSource = false;
const bool anomalyRouteProvidesGaugeCouplingNormalization = false;
const bool anomalyRouteProvidesTargetIndependentVevSource = false;
const bool anomalyRouteProvidesAbsoluteWzScale = false;
const bool anomalyRouteProvidesObservedFieldExtraction = false;
const bool anomalyRouteProvidesPhotonWzProjectionRows = false;
const bool anomalyRouteProvidesNeutralMassMatrixDiagonalization = false;
const bool anomalyRouteProvidesHiggsScalarSelfCouplingSource = false;
const bool anomalyRoutePromotesWzMasses = false;
const bool anomalyRoutePromotesHiggsMass = false;
const bool anomalyRouteCompletesBosonPredictions = false;
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

var coxPapersIIIIVSourceIntakeAuditPassed = JsonBool(phase233.RootElement, "externalCoxPapersIIIIVSourceIntakeAuditPassed") is true;
var externalCoxPapersIIIIVPromotableForBosonMasses = JsonBool(phase233.RootElement, "externalCoxPapersIIIIVPromotableForBosonMasses") is true;
var paperIIICohomologyAndAnomalyLeadPresent = phase233.RootElement.TryGetProperty("claimedResearchLeads", out var phase233Leads)
    && JsonBool(phase233Leads, "paperIIICohomologyAndAnomalyLeadPresent") is true;

var coxIiiAxialContactRgBosonParameterAuditPassed = JsonBool(phase240.RootElement, "coxIiiAxialContactRgBosonParameterAuditPassed") is true;
var coxIiiBrstQuantizationLeadPresent = JsonBool(phase240.RootElement, "coxIiiBrstQuantizationLeadPresent") is true;
var coxIiiControlsElectroweakCouplingRunning = JsonBool(phase240.RootElement, "coxIiiControlsElectroweakCouplingRunning") is true;
var coxIiiControlsHiggsQuarticRunning = JsonBool(phase240.RootElement, "coxIiiControlsHiggsQuarticRunning") is true;
var coxIiiAxialContactRgPromotableForBosonMasses = JsonBool(phase240.RootElement, "coxIiiAxialContactRgPromotableForBosonMasses") is true;

var electroweakIdentifiabilityRankAuditPassed = JsonBool(phase244.RootElement, "electroweakIdentifiabilityRankAuditPassed") is true;
var promotedWzRatioPresent = JsonBool(phase244.RootElement, "promotedWzRatioPresent") is true;
var anyPromotedAbsoluteMassPresent = JsonBool(phase244.RootElement, "anyPromotedAbsoluteMassPresent") is true;
var currentPromotedConstraintRank = JsonInt(phase244.RootElement, "currentPromotedConstraintRank") ?? -1;
var remainingNullity = JsonInt(phase244.RootElement, "remainingNullity") ?? -1;
var minimumAdditionalIndependentSourceConstraints = JsonInt(phase244.RootElement, "minimumAdditionalIndependentSourceConstraints") ?? -1;

var rankDeficitMinimalUnlockContractPassed = JsonBool(phase245.RootElement, "rankDeficitMinimalUnlockContractPassed") is true;
var unlockContractFilled = JsonBool(phase245.RootElement, "unlockContractFilled") is true;
var newSourceEvidenceStillRequired = JsonBool(phase245.RootElement, "newSourceEvidenceStillRequired") is true;
var unlockRowCount = JsonArray(phase245.RootElement, "unlockRows").Count;

var observedFieldExtractionRequiredFieldCount = JsonInt(phase256.RootElement, "requiredFieldCount") ?? -1;
var observedFieldExtractionFilledRequiredFieldCount = JsonInt(phase256.RootElement, "filledRequiredFieldCount") ?? -1;
var observedFieldExtractionContractPromotable = JsonBool(phase256.RootElement, "observedFieldExtractionContractPromotable") is true;
var phase295IntakeReadyObservedFieldExtractionCandidateCount = JsonInt(phase295.RootElement, "intakeReadyObservedFieldExtractionCandidateCount") ?? -1;
var phase295AnyObservedFieldExtractionCandidateFillsContract = JsonBool(phase295.RootElement, "anyObservedFieldExtractionCandidateFillsContract") is true;
var phase296IntakeReadySourceLineageFieldCandidateCount = JsonInt(phase296.RootElement, "intakeReadySourceLineageFieldCandidateCount") ?? -1;
var phase296AnySourceLineageCandidateFillsContract = JsonBool(phase296.RootElement, "anySourceLineageCandidateFillsContract") is true;

var officialDraftElectroweakProjectionMapAuditPassed = JsonBool(phase313.RootElement, "officialDraftElectroweakProjectionMapAuditPassed") is true;
var officialDraftProvidesWeakIsospinLocation = JsonBool(phase313.RootElement, "officialDraftProvidesWeakIsospinLocation") is true;
var officialDraftProvidesWeakHyperchargeLocation = JsonBool(phase313.RootElement, "officialDraftProvidesWeakHyperchargeLocation") is true;
var officialDraftProvidesPhotonZWeinbergRotation = JsonBool(phase313.RootElement, "officialDraftProvidesPhotonZWeinbergRotation") is true;
var officialDraftProvidesWeakMixingAngleSource = JsonBool(phase313.RootElement, "officialDraftProvidesWeakMixingAngleSource") is true;
var officialDraftProvidesNeutralMassMatrixDiagonalization = JsonBool(phase313.RootElement, "officialDraftProvidesNeutralMassMatrixDiagonalization") is true;
var officialDraftProvidesWChargedProjectionRows = JsonBool(phase313.RootElement, "officialDraftProvidesWChargedProjectionRows") is true;
var officialDraftProvidesZSourceRowProjection = JsonBool(phase313.RootElement, "officialDraftProvidesZSourceRowProjection") is true;
var officialDraftProvidesObservedElectroweakGaugeEmbedding = JsonBool(phase313.RootElement, "officialDraftProvidesObservedElectroweakGaugeEmbedding") is true;

var neutralElectroweakMixingSourceAuditPassed = JsonBool(phase321.RootElement, "neutralElectroweakMixingSourceAuditPassed") is true;
var lowEnergyHyperchargeSourcePresent = JsonBool(phase321.RootElement, "lowEnergyHyperchargeSourcePresent") is true;
var neutralMixingRouteProvidesTargetIndependentWeakMixingAngle = JsonBool(phase321.RootElement, "neutralMixingRouteProvidesTargetIndependentWeakMixingAngle") is true;
var neutralMixingRouteCompletesBosonPredictions = JsonBool(phase321.RootElement, "neutralMixingRouteCompletesBosonPredictions") is true;

var coupledYangMillsHiggsMassExtractionAuditPassed = JsonBool(phase323.RootElement, "coupledYangMillsHiggsMassExtractionAuditPassed") is true;
var officialPublicSourcesProvideTargetIndependentVevSource = JsonBool(phase323.RootElement, "officialPublicSourcesProvideTargetIndependentVevSource") is true;
var officialPublicSourcesProvideGaugeFixedQuadraticExpansion = JsonBool(phase323.RootElement, "officialPublicSourcesProvideGaugeFixedQuadraticExpansion") is true;
var officialPublicSourcesProvidePhotonWzHiggsProjectionRows = JsonBool(phase323.RootElement, "officialPublicSourcesProvidePhotonWzHiggsProjectionRows") is true;
var coupledYangMillsHiggsRouteCompletesBosonPredictions = JsonBool(phase323.RootElement, "coupledYangMillsHiggsRouteCompletesBosonPredictions") is true;

var custodialRhoParameterSourceAuditPassed = JsonBool(phase324.RootElement, "custodialRhoParameterSourceAuditPassed") is true;
var rhoRelationProvidesAbsoluteWzScale = JsonBool(phase324.RootElement, "rhoRelationProvidesAbsoluteWzScale") is true;
var rhoRelationProvidesWeakMixingAngleSource = JsonBool(phase324.RootElement, "rhoRelationProvidesWeakMixingAngleSource") is true;
var custodialRouteCompletesBosonPredictions = JsonBool(phase324.RootElement, "custodialRouteCompletesBosonPredictions") is true;

var electroweakUnitarityScatteringSourceAuditPassed = JsonBool(phase325.RootElement, "electroweakUnitarityScatteringSourceAuditPassed") is true;
var unitarityRouteProvidesExactHiggsMass = JsonBool(phase325.RootElement, "unitarityRouteProvidesExactHiggsMass") is true;
var unitarityRouteProvidesAbsoluteWzScale = JsonBool(phase325.RootElement, "unitarityRouteProvidesAbsoluteWzScale") is true;
var unitarityRouteCompletesBosonPredictions = JsonBool(phase325.RootElement, "unitarityRouteCompletesBosonPredictions") is true;

var anomalyRequirements = new[]
{
    new Requirement("perturbative-gauge-anomaly-cancellation", "Perturbative anomaly cancellation is a real consistency condition for chiral electroweak gauge theories.", bouchiatIliopoulosMeyerAnomalyFreeReferencePresent && anomalyRouteProvidesQuantumConsistencyConditions),
    new Requirement("global-su2-anomaly-doublet-parity", "Witten's SU(2) global anomaly restricts the parity of left-handed SU(2) doublets.", wittenGlobalSu2AnomalyReferencePresent && globalSu2AnomalyConstrainsDoubletParity),
    new Requirement("hypercharge-quantization-lead", "Anomaly cancellation can constrain or almost determine hypercharges under model assumptions.", ncgAnomalyHyperchargeReferencePresent && anomalyCancellationAlmostDeterminesHyperchargesUnderAssumptions),
    new Requirement("local-gu-anomaly-closure-lead", "Local Cox/GU side evidence records anomaly-closure leads, but not boson mass source rows.", paperIIICohomologyAndAnomalyLeadPresent && coxIiiBrstQuantizationLeadPresent),
    new Requirement("low-energy-hypercharge-source", "A GU-local low-energy hypercharge normalization source.", anomalyRouteProvidesLowEnergyHyperchargeSource),
    new Requirement("weak-mixing-angle-source", "A target-independent weak-mixing angle or g/g-prime source.", anomalyRouteProvidesWeakMixingAngleSource),
    new Requirement("gauge-coupling-normalization", "A target-independent electroweak gauge-coupling normalization.", anomalyRouteProvidesGaugeCouplingNormalization),
    new Requirement("target-independent-vev-source", "A target-independent electroweak VEV or vacuum-selection source.", anomalyRouteProvidesTargetIndependentVevSource),
    new Requirement("observed-field-extraction", "Observed photon/W/Z/H projection and field-extraction rows.", anomalyRouteProvidesObservedFieldExtraction && anomalyRouteProvidesPhotonWzProjectionRows),
    new Requirement("higgs-scalar-self-coupling-source", "A solved Higgs scalar self-coupling/source lineage.", anomalyRouteProvidesHiggsScalarSelfCouplingSource),
};

var checks = new[]
{
    new Check(
        "anomaly-and-hypercharge-references-recorded",
        bouchiatIliopoulosMeyerAnomalyFreeReferencePresent
            && wittenGlobalSu2AnomalyReferencePresent
            && ncgAnomalyHyperchargeReferencePresent
            && anomalyCancellationConstrainsFermionQuantumNumbers
            && globalSu2AnomalyConstrainsDoubletParity
            && anomalyCancellationAlmostDeterminesHyperchargesUnderAssumptions,
        $"bim={bouchiatIliopoulosMeyerAnomalyFreeReferencePresent}; witten={wittenGlobalSu2AnomalyReferencePresent}; ncg={ncgAnomalyHyperchargeReferencePresent}; constrainsQuantumNumbers={anomalyCancellationConstrainsFermionQuantumNumbers}; doubletParity={globalSu2AnomalyConstrainsDoubletParity}; hyperchargeLead={anomalyCancellationAlmostDeterminesHyperchargesUnderAssumptions}"),
    new Check(
        "local-gu-anomaly-leads-remain-nonpromotional",
        coxPapersIIIIVSourceIntakeAuditPassed
            && paperIIICohomologyAndAnomalyLeadPresent
            && !externalCoxPapersIIIIVPromotableForBosonMasses
            && coxIiiAxialContactRgBosonParameterAuditPassed
            && coxIiiBrstQuantizationLeadPresent
            && !coxIiiControlsElectroweakCouplingRunning
            && !coxIiiControlsHiggsQuarticRunning
            && !coxIiiAxialContactRgPromotableForBosonMasses,
        $"p233Passed={coxPapersIIIIVSourceIntakeAuditPassed}; anomalyLead={paperIIICohomologyAndAnomalyLeadPresent}; p233Promotes={externalCoxPapersIIIIVPromotableForBosonMasses}; p240Passed={coxIiiAxialContactRgBosonParameterAuditPassed}; brst={coxIiiBrstQuantizationLeadPresent}; ewRunning={coxIiiControlsElectroweakCouplingRunning}; higgsRunning={coxIiiControlsHiggsQuarticRunning}; p240Promotes={coxIiiAxialContactRgPromotableForBosonMasses}"),
    new Check(
        "anomaly-route-does-not-supply-electroweak-parameter-closure",
        electroweakParameterAuditPassed
            && !wAbsoluteMassParameterClosure
            && !zAbsoluteMassParameterClosure
            && !higgsMassParameterClosure
            && !weakCouplingSourcePromotable
            && !canPromoteExternalElectroweakBridge
            && electroweakIdentifiabilityRankAuditPassed
            && promotedWzRatioPresent
            && currentPromotedConstraintRank == 1
            && remainingNullity == 2
            && minimumAdditionalIndependentSourceConstraints == 2
            && !anyPromotedAbsoluteMassPresent
            && rankDeficitMinimalUnlockContractPassed
            && unlockRowCount == 2
            && !unlockContractFilled
            && newSourceEvidenceStillRequired,
        $"p224Passed={electroweakParameterAuditPassed}; wClosure={wAbsoluteMassParameterClosure}; zClosure={zAbsoluteMassParameterClosure}; higgsClosure={higgsMassParameterClosure}; weakCoupling={weakCouplingSourcePromotable}; rank={currentPromotedConstraintRank}; nullity={remainingNullity}; anyAbsolute={anyPromotedAbsoluteMassPresent}; unlockFilled={unlockContractFilled}; newSourceRequired={newSourceEvidenceStillRequired}"),
    new Check(
        "anomaly-route-does-not-fill-neutral-mixing-or-observed-map",
        !anomalyRouteProvidesLowEnergyHyperchargeSource
            && !anomalyRouteProvidesWeakMixingAngleSource
            && !anomalyRouteProvidesGaugeCouplingNormalization
            && officialDraftElectroweakProjectionMapAuditPassed
            && officialDraftProvidesWeakIsospinLocation
            && officialDraftProvidesWeakHyperchargeLocation
            && !officialDraftProvidesPhotonZWeinbergRotation
            && !officialDraftProvidesWeakMixingAngleSource
            && !officialDraftProvidesNeutralMassMatrixDiagonalization
            && !officialDraftProvidesWChargedProjectionRows
            && !officialDraftProvidesZSourceRowProjection
            && !officialDraftProvidesObservedElectroweakGaugeEmbedding
            && neutralElectroweakMixingSourceAuditPassed
            && !lowEnergyHyperchargeSourcePresent
            && !neutralMixingRouteProvidesTargetIndependentWeakMixingAngle
            && !neutralMixingRouteCompletesBosonPredictions,
        $"anomalyHypercharge={anomalyRouteProvidesLowEnergyHyperchargeSource}; anomalyWeakAngle={anomalyRouteProvidesWeakMixingAngleSource}; p313Passed={officialDraftElectroweakProjectionMapAuditPassed}; p313PhotonZ={officialDraftProvidesPhotonZWeinbergRotation}; p313WeakAngle={officialDraftProvidesWeakMixingAngleSource}; p313Observed={officialDraftProvidesObservedElectroweakGaugeEmbedding}; p321Passed={neutralElectroweakMixingSourceAuditPassed}; p321Hypercharge={lowEnergyHyperchargeSourcePresent}; p321WeakAngle={neutralMixingRouteProvidesTargetIndependentWeakMixingAngle}"),
    new Check(
        "mass-extraction-custodial-and-unitarity-gates-remain-blocked",
        coupledYangMillsHiggsMassExtractionAuditPassed
            && !officialPublicSourcesProvideTargetIndependentVevSource
            && !officialPublicSourcesProvideGaugeFixedQuadraticExpansion
            && !officialPublicSourcesProvidePhotonWzHiggsProjectionRows
            && !coupledYangMillsHiggsRouteCompletesBosonPredictions
            && custodialRhoParameterSourceAuditPassed
            && !rhoRelationProvidesAbsoluteWzScale
            && !rhoRelationProvidesWeakMixingAngleSource
            && !custodialRouteCompletesBosonPredictions
            && electroweakUnitarityScatteringSourceAuditPassed
            && !unitarityRouteProvidesExactHiggsMass
            && !unitarityRouteProvidesAbsoluteWzScale
            && !unitarityRouteCompletesBosonPredictions,
        $"p323Passed={coupledYangMillsHiggsMassExtractionAuditPassed}; p323Vev={officialPublicSourcesProvideTargetIndependentVevSource}; p323Rows={officialPublicSourcesProvidePhotonWzHiggsProjectionRows}; p324Passed={custodialRhoParameterSourceAuditPassed}; rhoAbsolute={rhoRelationProvidesAbsoluteWzScale}; p325Passed={electroweakUnitarityScatteringSourceAuditPassed}; unitarityExactHiggs={unitarityRouteProvidesExactHiggsMass}; unitarityAbsoluteWz={unitarityRouteProvidesAbsoluteWzScale}"),
    new Check(
        "anomaly-route-does-not-fill-source-lineage-or-observed-extraction-contracts",
        !anomalyRoutePromotesWzMasses
            && !anomalyRoutePromotesHiggsMass
            && !anomalyRouteCompletesBosonPredictions
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
        $"routePromotesWz={anomalyRoutePromotesWzMasses}; routePromotesHiggs={anomalyRoutePromotesHiggsMass}; routeCompletes={anomalyRouteCompletesBosonPredictions}; phase201AllLineages={phase201AllRequiredLineagesPromotable}; existingEvidence={existingEvidenceFound}; wzMissing={wzMissingFieldCount}; higgsMissing={higgsMissingFieldCount}; phase256Filled={observedFieldExtractionFilledRequiredFieldCount}; p295Ready={phase295IntakeReadyObservedFieldExtractionCandidateCount}; p296Ready={phase296IntakeReadySourceLineageFieldCandidateCount}"),
};

var anomalyHyperchargeSourceAuditPassed = checks.All(check => check.Passed)
    && anomalyRequirements.Count(requirement => requirement.Filled) == 4
    && anomalyRouteProvidesQuantumConsistencyConditions
    && anomalyRouteProvidesRepresentationConstraint
    && !anomalyRouteProvidesLowEnergyHyperchargeSource
    && !anomalyRouteProvidesWeakMixingAngleSource
    && !anomalyRouteProvidesGaugeCouplingNormalization
    && !anomalyRouteProvidesTargetIndependentVevSource
    && !anomalyRouteProvidesAbsoluteWzScale
    && !anomalyRouteProvidesObservedFieldExtraction
    && !anomalyRouteProvidesPhotonWzProjectionRows
    && !anomalyRouteProvidesNeutralMassMatrixDiagonalization
    && !anomalyRouteProvidesHiggsScalarSelfCouplingSource
    && !anomalyRoutePromotesWzMasses
    && !anomalyRoutePromotesHiggsMass
    && !anomalyRouteCompletesBosonPredictions
    && !canFillPhase201WzContract
    && !canFillPhase201HiggsContract
    && !canFillPhase256ObservedFieldExtractionContract;

var terminalStatus = anomalyHyperchargeSourceAuditPassed
    ? "anomaly-hypercharge-source-audit-consistency-not-mass-source"
    : "anomaly-hypercharge-source-audit-review-required";

var result = new
{
    phaseId = "phase326-anomaly-hypercharge-source-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    anomalyHyperchargeSourceAuditPassed,
    bouchiatIliopoulosMeyerAnomalyFreeReferencePresent,
    wittenGlobalSu2AnomalyReferencePresent,
    ncgAnomalyHyperchargeReferencePresent,
    anomalyCancellationConstrainsFermionQuantumNumbers,
    globalSu2AnomalyConstrainsDoubletParity,
    anomalyCancellationAlmostDeterminesHyperchargesUnderAssumptions,
    anomalyRouteProvidesQuantumConsistencyConditions,
    anomalyRouteProvidesRepresentationConstraint,
    anomalyRouteProvidesLowEnergyHyperchargeSource,
    anomalyRouteProvidesWeakMixingAngleSource,
    anomalyRouteProvidesGaugeCouplingNormalization,
    anomalyRouteProvidesTargetIndependentVevSource,
    anomalyRouteProvidesAbsoluteWzScale,
    anomalyRouteProvidesObservedFieldExtraction,
    anomalyRouteProvidesPhotonWzProjectionRows,
    anomalyRouteProvidesNeutralMassMatrixDiagonalization,
    anomalyRouteProvidesHiggsScalarSelfCouplingSource,
    anomalyRoutePromotesWzMasses,
    anomalyRoutePromotesHiggsMass,
    anomalyRouteCompletesBosonPredictions,
    canFillPhase201WzContract,
    canFillPhase201HiggsContract,
    canFillPhase256ObservedFieldExtractionContract,
    externalSources = new[]
    {
        new ExternalSource(
            "bouchiat-iliopoulos-meyer-1972",
            "Bouchiat, Iliopoulos, and Meyer, Anomaly-free version of Weinberg's model",
            bouchiatIliopoulosMeyerOstiUrl,
            "Records the anomaly-free electroweak model route as a quantum-consistency constraint, not as a W/Z/H mass source."),
        new ExternalSource(
            "witten-1982-su2-anomaly",
            "Witten, An SU(2) anomaly",
            wittenSu2AnomalyPrincetonUrl,
            "Restricts SU(2) fermion quantum numbers, including odd left-handed doublet inconsistency; it does not set electroweak mass scales."),
        new ExternalSource(
            "alvarez-gracia-bondia-martin-1995-ncg",
            "Alvarez, Gracia-Bondia, and Martin, Anomaly Cancellation and gauge group of the standard model in NCG",
            alvarezGraciaBondiaMartinNcgUrl,
            "Records that anomaly cancellation almost determines Standard Model hypercharges and is related to the gauge group in NCG; it does not derive low-energy W/Z/H mass rows."),
    },
    anomalyRequirements,
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
        phase233 = new
        {
            path = Phase233Path,
            coxPapersIIIIVSourceIntakeAuditPassed,
            paperIIICohomologyAndAnomalyLeadPresent,
            externalCoxPapersIIIIVPromotableForBosonMasses,
        },
        phase240 = new
        {
            path = Phase240Path,
            coxIiiAxialContactRgBosonParameterAuditPassed,
            coxIiiBrstQuantizationLeadPresent,
            coxIiiControlsElectroweakCouplingRunning,
            coxIiiControlsHiggsQuarticRunning,
            coxIiiAxialContactRgPromotableForBosonMasses,
        },
        phase244 = new
        {
            path = Phase244Path,
            electroweakIdentifiabilityRankAuditPassed,
            promotedWzRatioPresent,
            anyPromotedAbsoluteMassPresent,
            currentPromotedConstraintRank,
            remainingNullity,
            minimumAdditionalIndependentSourceConstraints,
        },
        phase245 = new
        {
            path = Phase245Path,
            rankDeficitMinimalUnlockContractPassed,
            unlockContractFilled,
            newSourceEvidenceStillRequired,
            unlockRowCount,
        },
        phase313 = new
        {
            path = Phase313Path,
            officialDraftElectroweakProjectionMapAuditPassed,
            officialDraftProvidesWeakIsospinLocation,
            officialDraftProvidesWeakHyperchargeLocation,
            officialDraftProvidesPhotonZWeinbergRotation,
            officialDraftProvidesWeakMixingAngleSource,
            officialDraftProvidesNeutralMassMatrixDiagonalization,
            officialDraftProvidesWChargedProjectionRows,
            officialDraftProvidesZSourceRowProjection,
            officialDraftProvidesObservedElectroweakGaugeEmbedding,
        },
        phase321 = new
        {
            path = Phase321Path,
            neutralElectroweakMixingSourceAuditPassed,
            lowEnergyHyperchargeSourcePresent,
            neutralMixingRouteProvidesTargetIndependentWeakMixingAngle,
            neutralMixingRouteCompletesBosonPredictions,
        },
        phase323 = new
        {
            path = Phase323Path,
            coupledYangMillsHiggsMassExtractionAuditPassed,
            officialPublicSourcesProvideTargetIndependentVevSource,
            officialPublicSourcesProvideGaugeFixedQuadraticExpansion,
            officialPublicSourcesProvidePhotonWzHiggsProjectionRows,
            coupledYangMillsHiggsRouteCompletesBosonPredictions,
        },
        phase324 = new
        {
            path = Phase324Path,
            custodialRhoParameterSourceAuditPassed,
            rhoRelationProvidesAbsoluteWzScale,
            rhoRelationProvidesWeakMixingAngleSource,
            custodialRouteCompletesBosonPredictions,
        },
        phase325 = new
        {
            path = Phase325Path,
            electroweakUnitarityScatteringSourceAuditPassed,
            unitarityRouteProvidesExactHiggsMass,
            unitarityRouteProvidesAbsoluteWzScale,
            unitarityRouteCompletesBosonPredictions,
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
    decision = anomalyHyperchargeSourceAuditPassed
        ? "Do not promote W/Z or Higgs absolute masses from anomaly cancellation, hypercharge quantization, or local anomaly-closure leads. They are necessary quantum-consistency and representation constraints, but they do not derive the low-energy weak-mixing angle, gauge-coupling normalization, electroweak VEV, W/Z absolute scale, observed photon/W/Z/H rows, or GU Higgs scalar-source/self-coupling lineage."
        : "Review the anomaly/hypercharge source audit before using anomaly cancellation as boson prediction evidence.",
    nextRequiredArtifact = new[]
    {
        "A GU-local electroweak anomaly-to-observed-field theorem that yields physical photon/W/Z/H rows, not only consistency of representations.",
        "A target-independent low-energy weak-mixing angle or gauge-coupling ratio source.",
        "A target-independent electroweak VEV and W/Z mass-matrix source.",
        "A solved Higgs scalar-source/self-coupling lineage satisfying Phase201.",
    },
    sourceEvidence = new
    {
        phase201Path = Phase201Path,
        phase213Path = Phase213Path,
        phase224Path = Phase224Path,
        phase233Path = Phase233Path,
        phase240Path = Phase240Path,
        phase244Path = Phase244Path,
        phase245Path = Phase245Path,
        phase256Path = Phase256Path,
        phase295Path = Phase295Path,
        phase296Path = Phase296Path,
        phase313Path = Phase313Path,
        phase321Path = Phase321Path,
        phase323Path = Phase323Path,
        phase324Path = Phase324Path,
        phase325Path = Phase325Path,
        bouchiatIliopoulosMeyerOstiUrl,
        wittenSu2AnomalyPrincetonUrl,
        alvarezGraciaBondiaMartinNcgUrl,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true };
File.WriteAllText(
    Path.Combine(outputDir, "anomaly_hypercharge_source_audit.json"),
    JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "anomaly_hypercharge_source_audit_summary.json"),
    JsonSerializer.Serialize(result, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"anomalyHyperchargeSourceAuditPassed={anomalyHyperchargeSourceAuditPassed}");
Console.WriteLine($"anomalyCancellationConstrainsFermionQuantumNumbers={anomalyCancellationConstrainsFermionQuantumNumbers}");
Console.WriteLine($"anomalyRouteProvidesWeakMixingAngleSource={anomalyRouteProvidesWeakMixingAngleSource}");
Console.WriteLine($"anomalyRouteProvidesAbsoluteWzScale={anomalyRouteProvidesAbsoluteWzScale}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");

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
