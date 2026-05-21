using System.Text.Json;

const string DefaultOutputDir = "studies/phase327_oblique_precision_electroweak_source_audit_001/output";
const string Phase201Path = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase224Path = "studies/phase224_electroweak_parameter_dependency_audit_001/output/electroweak_parameter_dependency_audit_summary.json";
const string Phase245Path = "studies/phase245_rank_deficit_minimal_unlock_contract_001/output/rank_deficit_minimal_unlock_contract_summary.json";
const string Phase256Path = "studies/phase256_observed_field_extraction_intake_contract_001/output/observed_field_extraction_intake_contract_summary.json";
const string Phase261Path = "studies/phase261_electroweak_scheme_radiative_source_audit_001/output/electroweak_scheme_radiative_source_audit_summary.json";
const string Phase279Path = "studies/phase279_technicolor_walking_electroweak_scale_source_audit_001/output/technicolor_walking_electroweak_scale_source_audit_summary.json";
const string Phase295Path = "studies/phase295_observed_field_extraction_contract_candidate_scan_001/output/observed_field_extraction_contract_candidate_scan_summary.json";
const string Phase296Path = "studies/phase296_source_lineage_contract_field_candidate_scan_001/output/source_lineage_contract_field_candidate_scan_summary.json";
const string Phase313Path = "studies/phase313_official_draft_electroweak_projection_map_audit_001/output/official_draft_electroweak_projection_map_audit_summary.json";
const string Phase321Path = "studies/phase321_neutral_electroweak_mixing_source_audit_001/output/neutral_electroweak_mixing_source_audit_summary.json";
const string Phase324Path = "studies/phase324_custodial_rho_parameter_source_audit_001/output/custodial_rho_parameter_source_audit_summary.json";
const string Phase326Path = "studies/phase326_anomaly_hypercharge_source_audit_001/output/anomaly_hypercharge_source_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE327_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase224 = JsonDocument.Parse(File.ReadAllText(Phase224Path));
using var phase245 = JsonDocument.Parse(File.ReadAllText(Phase245Path));
using var phase256 = JsonDocument.Parse(File.ReadAllText(Phase256Path));
using var phase261 = JsonDocument.Parse(File.ReadAllText(Phase261Path));
using var phase279 = JsonDocument.Parse(File.ReadAllText(Phase279Path));
using var phase295 = JsonDocument.Parse(File.ReadAllText(Phase295Path));
using var phase296 = JsonDocument.Parse(File.ReadAllText(Phase296Path));
using var phase313 = JsonDocument.Parse(File.ReadAllText(Phase313Path));
using var phase321 = JsonDocument.Parse(File.ReadAllText(Phase321Path));
using var phase324 = JsonDocument.Parse(File.ReadAllText(Phase324Path));
using var phase326 = JsonDocument.Parse(File.ReadAllText(Phase326Path));

const string peskinTakeuchiOstiUrl = "https://www.osti.gov/biblio/7235268";
const string pdg2025ElectroweakReviewUrl = "https://pdg.lbl.gov/2025/reviews/rpp2025-rev-standard-model.pdf";
const bool peskinTakeuchiObliqueReferencePresent = true;
const bool pdgElectroweakPrecisionReviewPresent = true;
const bool obliqueParametersSummarizeVacuumPolarizationCorrections = true;
const bool obliqueParametersConstrainNewPhysics = true;
const bool obliqueFitUsesPrecisionWzData = true;
const bool obliqueRouteProvidesFitConstraint = true;
const bool obliqueRouteProvidesLoopCorrectionParameterization = true;
const bool obliqueRouteProvidesExactTreeLevelMassSource = false;
const bool obliqueRouteProvidesTargetIndependentVevSource = false;
const bool obliqueRouteProvidesWeakMixingAngleSource = false;
const bool obliqueRouteProvidesGaugeCouplingNormalization = false;
const bool obliqueRouteProvidesAbsoluteWzScale = false;
const bool obliqueRouteProvidesObservedFieldExtraction = false;
const bool obliqueRouteProvidesPhotonWzProjectionRows = false;
const bool obliqueRouteProvidesNeutralMassMatrixDiagonalization = false;
const bool obliqueRouteProvidesHiggsScalarSelfCouplingSource = false;
const bool obliqueRoutePromotesWzMasses = false;
const bool obliqueRoutePromotesHiggsMass = false;
const bool obliqueRouteCompletesBosonPredictions = false;
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

var rankDeficitMinimalUnlockContractPassed = JsonBool(phase245.RootElement, "rankDeficitMinimalUnlockContractPassed") is true;
var unlockContractFilled = JsonBool(phase245.RootElement, "unlockContractFilled") is true;
var newSourceEvidenceStillRequired = JsonBool(phase245.RootElement, "newSourceEvidenceStillRequired") is true;
var unlockRowCount = JsonArray(phase245.RootElement, "unlockRows").Count;

var observedFieldExtractionRequiredFieldCount = JsonInt(phase256.RootElement, "requiredFieldCount") ?? -1;
var observedFieldExtractionFilledRequiredFieldCount = JsonInt(phase256.RootElement, "filledRequiredFieldCount") ?? -1;
var observedFieldExtractionContractPromotable = JsonBool(phase256.RootElement, "observedFieldExtractionContractPromotable") is true;

var electroweakSchemeRadiativeSourceAuditPassed = JsonBool(phase261.RootElement, "electroweakSchemeRadiativeSourceAuditPassed") is true;
var schemeChoicePromotesBosonMasses = JsonBool(phase261.RootElement, "schemeChoicePromotesBosonMasses") is true;
var anySchemeNearTargetWeakCoupling = JsonBool(phase261.RootElement, "anySchemeNearTargetWeakCoupling") is true;
var schemeInputsAreExternalElectroweakInputs = JsonBool(phase261.RootElement, "schemeInputsAreExternalElectroweakInputs") is true;
var schemeChoiceProvidesGuSourceLineage = JsonBool(phase261.RootElement, "schemeChoiceProvidesGuSourceLineage") is true;
var schemeChoiceProvidesObservedFieldExtraction = JsonBool(phase261.RootElement, "schemeChoiceProvidesObservedFieldExtraction") is true;

var technicolorWalkingElectroweakScaleSourceAuditPassed = JsonBool(phase279.RootElement, "technicolorWalkingElectroweakScaleSourceAuditPassed") is true;
var technicolorPromotesWzMasses = JsonBool(phase279.RootElement, "technicolorPromotesWzMasses") is true;
var technicolorPromotesHiggsMass = JsonBool(phase279.RootElement, "technicolorPromotesHiggsMass") is true;
var technicolorRequiresPrecisionElectroweakConstraintSource = phase279.RootElement.TryGetProperty("technicolorBoundary", out var p279Boundary)
    && JsonBool(p279Boundary, "technicolorRequiresPrecisionElectroweakConstraintSource") is true;
var technicolorRequiresWzMassMatrixSource = phase279.RootElement.TryGetProperty("technicolorBoundary", out p279Boundary)
    && JsonBool(p279Boundary, "technicolorRequiresWzMassMatrixSource") is true;
var technicolorRequiresObservedFieldExtraction = phase279.RootElement.TryGetProperty("technicolorBoundary", out p279Boundary)
    && JsonBool(p279Boundary, "technicolorRequiresObservedFieldExtraction") is true;
var technicolorExternalToGu = phase279.RootElement.TryGetProperty("technicolorBoundary", out p279Boundary)
    && JsonBool(p279Boundary, "technicolorExternalToGu") is true;

var phase295IntakeReadyObservedFieldExtractionCandidateCount = JsonInt(phase295.RootElement, "intakeReadyObservedFieldExtractionCandidateCount") ?? -1;
var phase295AnyObservedFieldExtractionCandidateFillsContract = JsonBool(phase295.RootElement, "anyObservedFieldExtractionCandidateFillsContract") is true;
var phase296IntakeReadySourceLineageFieldCandidateCount = JsonInt(phase296.RootElement, "intakeReadySourceLineageFieldCandidateCount") ?? -1;
var phase296AnySourceLineageCandidateFillsContract = JsonBool(phase296.RootElement, "anySourceLineageCandidateFillsContract") is true;

var officialDraftElectroweakProjectionMapAuditPassed = JsonBool(phase313.RootElement, "officialDraftElectroweakProjectionMapAuditPassed") is true;
var officialDraftProvidesWeakIsospinLocation = JsonBool(phase313.RootElement, "officialDraftProvidesWeakIsospinLocation") is true;
var officialDraftProvidesWeakHyperchargeLocation = JsonBool(phase313.RootElement, "officialDraftProvidesWeakHyperchargeLocation") is true;
var officialDraftProvidesPhotonZWeinbergRotation = JsonBool(phase313.RootElement, "officialDraftProvidesPhotonZWeinbergRotation") is true;
var officialDraftProvidesWeakMixingAngleSource = JsonBool(phase313.RootElement, "officialDraftProvidesWeakMixingAngleSource") is true;
var officialDraftProvidesObservedElectroweakGaugeEmbedding = JsonBool(phase313.RootElement, "officialDraftProvidesObservedElectroweakGaugeEmbedding") is true;

var neutralElectroweakMixingSourceAuditPassed = JsonBool(phase321.RootElement, "neutralElectroweakMixingSourceAuditPassed") is true;
var lowEnergyHyperchargeSourcePresent = JsonBool(phase321.RootElement, "lowEnergyHyperchargeSourcePresent") is true;
var neutralMixingRouteProvidesTargetIndependentWeakMixingAngle = JsonBool(phase321.RootElement, "neutralMixingRouteProvidesTargetIndependentWeakMixingAngle") is true;
var neutralMixingRouteCompletesBosonPredictions = JsonBool(phase321.RootElement, "neutralMixingRouteCompletesBosonPredictions") is true;

var custodialRhoParameterSourceAuditPassed = JsonBool(phase324.RootElement, "custodialRhoParameterSourceAuditPassed") is true;
var rhoRelationProvidesAbsoluteWzScale = JsonBool(phase324.RootElement, "rhoRelationProvidesAbsoluteWzScale") is true;
var rhoRelationProvidesWeakMixingAngleSource = JsonBool(phase324.RootElement, "rhoRelationProvidesWeakMixingAngleSource") is true;
var custodialRouteCompletesBosonPredictions = JsonBool(phase324.RootElement, "custodialRouteCompletesBosonPredictions") is true;

var anomalyHyperchargeSourceAuditPassed = JsonBool(phase326.RootElement, "anomalyHyperchargeSourceAuditPassed") is true;
var anomalyRouteProvidesWeakMixingAngleSource = JsonBool(phase326.RootElement, "anomalyRouteProvidesWeakMixingAngleSource") is true;
var anomalyRouteProvidesAbsoluteWzScale = JsonBool(phase326.RootElement, "anomalyRouteProvidesAbsoluteWzScale") is true;
var anomalyRouteCompletesBosonPredictions = JsonBool(phase326.RootElement, "anomalyRouteCompletesBosonPredictions") is true;

var obliqueRequirements = new[]
{
    new Requirement("peskin-takeuchi-stu-parameters", "S/T/U parameterize electroweak vacuum-polarization corrections in precision fits.", peskinTakeuchiObliqueReferencePresent && obliqueParametersSummarizeVacuumPolarizationCorrections),
    new Requirement("pdg-precision-electroweak-fit", "PDG electroweak precision review records S/T constraints and W/Z precision data.", pdgElectroweakPrecisionReviewPresent && obliqueFitUsesPrecisionWzData),
    new Requirement("new-physics-fit-constraint", "Oblique parameters constrain new-physics sectors, including technicolor-like routes.", obliqueParametersConstrainNewPhysics && technicolorRequiresPrecisionElectroweakConstraintSource),
    new Requirement("exact-mass-source-row", "A target-independent exact W/Z/H mass source row, not a fit parameter.", obliqueRouteProvidesExactTreeLevelMassSource),
    new Requirement("weak-mixing-angle-source", "A target-independent low-energy weak-mixing angle or g/g-prime source.", obliqueRouteProvidesWeakMixingAngleSource),
    new Requirement("target-independent-vev-source", "A target-independent electroweak VEV or vacuum-selection source.", obliqueRouteProvidesTargetIndependentVevSource),
    new Requirement("observed-field-extraction", "Observed photon/W/Z/H projection and field-extraction rows.", obliqueRouteProvidesObservedFieldExtraction && obliqueRouteProvidesPhotonWzProjectionRows),
    new Requirement("higgs-scalar-self-coupling-source", "A solved Higgs scalar self-coupling/source lineage.", obliqueRouteProvidesHiggsScalarSelfCouplingSource),
};

var checks = new[]
{
    new Check(
        "oblique-precision-references-recorded",
        peskinTakeuchiObliqueReferencePresent
            && pdgElectroweakPrecisionReviewPresent
            && obliqueParametersSummarizeVacuumPolarizationCorrections
            && obliqueParametersConstrainNewPhysics
            && obliqueFitUsesPrecisionWzData
            && obliqueRouteProvidesFitConstraint
            && obliqueRouteProvidesLoopCorrectionParameterization,
        $"peskinTakeuchi={peskinTakeuchiObliqueReferencePresent}; pdg={pdgElectroweakPrecisionReviewPresent}; vacuumPolarization={obliqueParametersSummarizeVacuumPolarizationCorrections}; constrainsNewPhysics={obliqueParametersConstrainNewPhysics}; fitUsesWz={obliqueFitUsesPrecisionWzData}"),
    new Check(
        "radiative-scheme-route-remains-external-input",
        electroweakSchemeRadiativeSourceAuditPassed
            && anySchemeNearTargetWeakCoupling
            && schemeInputsAreExternalElectroweakInputs
            && !schemeChoicePromotesBosonMasses
            && !schemeChoiceProvidesGuSourceLineage
            && !schemeChoiceProvidesObservedFieldExtraction,
        $"p261Passed={electroweakSchemeRadiativeSourceAuditPassed}; nearTarget={anySchemeNearTargetWeakCoupling}; externalInputs={schemeInputsAreExternalElectroweakInputs}; promotes={schemeChoicePromotesBosonMasses}; sourceLineage={schemeChoiceProvidesGuSourceLineage}; observedExtraction={schemeChoiceProvidesObservedFieldExtraction}"),
    new Check(
        "precision-constraint-is-not-technicolor-or-bsm-source",
        technicolorWalkingElectroweakScaleSourceAuditPassed
            && technicolorRequiresPrecisionElectroweakConstraintSource
            && technicolorRequiresWzMassMatrixSource
            && technicolorRequiresObservedFieldExtraction
            && technicolorExternalToGu
            && !technicolorPromotesWzMasses
            && !technicolorPromotesHiggsMass,
        $"p279Passed={technicolorWalkingElectroweakScaleSourceAuditPassed}; precisionConstraintRequired={technicolorRequiresPrecisionElectroweakConstraintSource}; massMatrixRequired={technicolorRequiresWzMassMatrixSource}; observedExtractionRequired={technicolorRequiresObservedFieldExtraction}; externalToGu={technicolorExternalToGu}; promotesWz={technicolorPromotesWzMasses}; promotesHiggs={technicolorPromotesHiggsMass}"),
    new Check(
        "oblique-route-does-not-close-electroweak-parameter-contract",
        electroweakParameterAuditPassed
            && !wAbsoluteMassParameterClosure
            && !zAbsoluteMassParameterClosure
            && !higgsMassParameterClosure
            && !weakCouplingSourcePromotable
            && !canPromoteExternalElectroweakBridge
            && rankDeficitMinimalUnlockContractPassed
            && unlockRowCount == 2
            && !unlockContractFilled
            && newSourceEvidenceStillRequired
            && !obliqueRouteProvidesTargetIndependentVevSource
            && !obliqueRouteProvidesWeakMixingAngleSource
            && !obliqueRouteProvidesGaugeCouplingNormalization
            && !obliqueRouteProvidesAbsoluteWzScale,
        $"p224Passed={electroweakParameterAuditPassed}; wClosure={wAbsoluteMassParameterClosure}; zClosure={zAbsoluteMassParameterClosure}; higgsClosure={higgsMassParameterClosure}; weakCoupling={weakCouplingSourcePromotable}; unlockFilled={unlockContractFilled}; obliqueVev={obliqueRouteProvidesTargetIndependentVevSource}; obliqueWeakAngle={obliqueRouteProvidesWeakMixingAngleSource}; obliqueAbsoluteWz={obliqueRouteProvidesAbsoluteWzScale}"),
    new Check(
        "projection-mixing-custodial-and-anomaly-gates-remain-blocked",
        officialDraftElectroweakProjectionMapAuditPassed
            && officialDraftProvidesWeakIsospinLocation
            && officialDraftProvidesWeakHyperchargeLocation
            && !officialDraftProvidesPhotonZWeinbergRotation
            && !officialDraftProvidesWeakMixingAngleSource
            && !officialDraftProvidesObservedElectroweakGaugeEmbedding
            && neutralElectroweakMixingSourceAuditPassed
            && !lowEnergyHyperchargeSourcePresent
            && !neutralMixingRouteProvidesTargetIndependentWeakMixingAngle
            && !neutralMixingRouteCompletesBosonPredictions
            && custodialRhoParameterSourceAuditPassed
            && !rhoRelationProvidesAbsoluteWzScale
            && !rhoRelationProvidesWeakMixingAngleSource
            && !custodialRouteCompletesBosonPredictions
            && anomalyHyperchargeSourceAuditPassed
            && !anomalyRouteProvidesWeakMixingAngleSource
            && !anomalyRouteProvidesAbsoluteWzScale
            && !anomalyRouteCompletesBosonPredictions,
        $"p313Passed={officialDraftElectroweakProjectionMapAuditPassed}; p313PhotonZ={officialDraftProvidesPhotonZWeinbergRotation}; p321Passed={neutralElectroweakMixingSourceAuditPassed}; p321Hypercharge={lowEnergyHyperchargeSourcePresent}; p324Passed={custodialRhoParameterSourceAuditPassed}; rhoAbsolute={rhoRelationProvidesAbsoluteWzScale}; p326Passed={anomalyHyperchargeSourceAuditPassed}; anomalyWeakAngle={anomalyRouteProvidesWeakMixingAngleSource}; anomalyAbsolute={anomalyRouteProvidesAbsoluteWzScale}"),
    new Check(
        "oblique-route-does-not-fill-source-lineage-or-observed-extraction-contracts",
        !obliqueRoutePromotesWzMasses
            && !obliqueRoutePromotesHiggsMass
            && !obliqueRouteCompletesBosonPredictions
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
        $"routePromotesWz={obliqueRoutePromotesWzMasses}; routePromotesHiggs={obliqueRoutePromotesHiggsMass}; routeCompletes={obliqueRouteCompletesBosonPredictions}; phase201AllLineages={phase201AllRequiredLineagesPromotable}; existingEvidence={existingEvidenceFound}; wzMissing={wzMissingFieldCount}; higgsMissing={higgsMissingFieldCount}; phase256Filled={observedFieldExtractionFilledRequiredFieldCount}; p295Ready={phase295IntakeReadyObservedFieldExtractionCandidateCount}; p296Ready={phase296IntakeReadySourceLineageFieldCandidateCount}"),
};

var obliquePrecisionElectroweakSourceAuditPassed = checks.All(check => check.Passed)
    && obliqueRequirements.Count(requirement => requirement.Filled) == 3
    && obliqueRouteProvidesFitConstraint
    && obliqueRouteProvidesLoopCorrectionParameterization
    && !obliqueRouteProvidesExactTreeLevelMassSource
    && !obliqueRouteProvidesTargetIndependentVevSource
    && !obliqueRouteProvidesWeakMixingAngleSource
    && !obliqueRouteProvidesGaugeCouplingNormalization
    && !obliqueRouteProvidesAbsoluteWzScale
    && !obliqueRouteProvidesObservedFieldExtraction
    && !obliqueRouteProvidesPhotonWzProjectionRows
    && !obliqueRouteProvidesNeutralMassMatrixDiagonalization
    && !obliqueRouteProvidesHiggsScalarSelfCouplingSource
    && !obliqueRoutePromotesWzMasses
    && !obliqueRoutePromotesHiggsMass
    && !obliqueRouteCompletesBosonPredictions
    && !canFillPhase201WzContract
    && !canFillPhase201HiggsContract
    && !canFillPhase256ObservedFieldExtractionContract;

var terminalStatus = obliquePrecisionElectroweakSourceAuditPassed
    ? "oblique-precision-electroweak-source-audit-fit-constraint-not-mass-source"
    : "oblique-precision-electroweak-source-audit-review-required";

var result = new
{
    phaseId = "phase327-oblique-precision-electroweak-source-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    obliquePrecisionElectroweakSourceAuditPassed,
    peskinTakeuchiObliqueReferencePresent,
    pdgElectroweakPrecisionReviewPresent,
    obliqueParametersSummarizeVacuumPolarizationCorrections,
    obliqueParametersConstrainNewPhysics,
    obliqueFitUsesPrecisionWzData,
    obliqueRouteProvidesFitConstraint,
    obliqueRouteProvidesLoopCorrectionParameterization,
    obliqueRouteProvidesExactTreeLevelMassSource,
    obliqueRouteProvidesTargetIndependentVevSource,
    obliqueRouteProvidesWeakMixingAngleSource,
    obliqueRouteProvidesGaugeCouplingNormalization,
    obliqueRouteProvidesAbsoluteWzScale,
    obliqueRouteProvidesObservedFieldExtraction,
    obliqueRouteProvidesPhotonWzProjectionRows,
    obliqueRouteProvidesNeutralMassMatrixDiagonalization,
    obliqueRouteProvidesHiggsScalarSelfCouplingSource,
    obliqueRoutePromotesWzMasses,
    obliqueRoutePromotesHiggsMass,
    obliqueRouteCompletesBosonPredictions,
    canFillPhase201WzContract,
    canFillPhase201HiggsContract,
    canFillPhase256ObservedFieldExtractionContract,
    externalSources = new[]
    {
        new ExternalSource(
            "peskin-takeuchi-1992-oblique-corrections",
            "Peskin and Takeuchi, Estimation of oblique electroweak corrections",
            peskinTakeuchiOstiUrl,
            "S/T/U summarize vacuum-polarization contributions to precision experiments and constrain new physics, not GU-local exact mass rows."),
        new ExternalSource(
            "pdg-2025-electroweak-precision",
            "Particle Data Group, Electroweak Model and Constraints on New Physics, 2025 update",
            pdg2025ElectroweakReviewUrl,
            "Records precision W/Z/H inputs, radiative corrections, and S/T constraints as electroweak fit machinery rather than source-lineage artifacts."),
    },
    obliqueRequirements,
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
        phase245 = new
        {
            path = Phase245Path,
            rankDeficitMinimalUnlockContractPassed,
            unlockContractFilled,
            newSourceEvidenceStillRequired,
            unlockRowCount,
        },
        phase261 = new
        {
            path = Phase261Path,
            electroweakSchemeRadiativeSourceAuditPassed,
            schemeChoicePromotesBosonMasses,
            anySchemeNearTargetWeakCoupling,
            schemeInputsAreExternalElectroweakInputs,
            schemeChoiceProvidesGuSourceLineage,
            schemeChoiceProvidesObservedFieldExtraction,
        },
        phase279 = new
        {
            path = Phase279Path,
            technicolorWalkingElectroweakScaleSourceAuditPassed,
            technicolorRequiresPrecisionElectroweakConstraintSource,
            technicolorRequiresWzMassMatrixSource,
            technicolorRequiresObservedFieldExtraction,
            technicolorExternalToGu,
            technicolorPromotesWzMasses,
            technicolorPromotesHiggsMass,
        },
        phase313 = new
        {
            path = Phase313Path,
            officialDraftElectroweakProjectionMapAuditPassed,
            officialDraftProvidesWeakIsospinLocation,
            officialDraftProvidesWeakHyperchargeLocation,
            officialDraftProvidesPhotonZWeinbergRotation,
            officialDraftProvidesWeakMixingAngleSource,
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
        phase324 = new
        {
            path = Phase324Path,
            custodialRhoParameterSourceAuditPassed,
            rhoRelationProvidesAbsoluteWzScale,
            rhoRelationProvidesWeakMixingAngleSource,
            custodialRouteCompletesBosonPredictions,
        },
        phase326 = new
        {
            path = Phase326Path,
            anomalyHyperchargeSourceAuditPassed,
            anomalyRouteProvidesWeakMixingAngleSource,
            anomalyRouteProvidesAbsoluteWzScale,
            anomalyRouteCompletesBosonPredictions,
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
    decision = obliquePrecisionElectroweakSourceAuditPassed
        ? "Do not promote W/Z or Higgs absolute masses from precision-electroweak oblique parameters. S/T/U are real vacuum-polarization fit constraints and useful BSM consistency diagnostics, but they do not derive a GU-local electroweak VEV, weak-mixing angle, gauge-coupling normalization, W/Z absolute scale, observed photon/W/Z/H rows, or Higgs scalar-source/self-coupling lineage."
        : "Review the oblique precision electroweak source audit before using S/T/U as boson prediction evidence.",
    nextRequiredArtifact = new[]
    {
        "A GU-local electroweak vacuum and low-energy coupling source, not a fitted oblique correction parameter.",
        "Observed photon/W/Z/H projection rows satisfying Phase256.",
        "A W/Z mass-matrix source theorem that produces absolute source rows before precision-fit comparison.",
        "A solved Higgs scalar-source/self-coupling lineage satisfying Phase201.",
    },
    sourceEvidence = new
    {
        phase201Path = Phase201Path,
        phase213Path = Phase213Path,
        phase224Path = Phase224Path,
        phase245Path = Phase245Path,
        phase256Path = Phase256Path,
        phase261Path = Phase261Path,
        phase279Path = Phase279Path,
        phase295Path = Phase295Path,
        phase296Path = Phase296Path,
        phase313Path = Phase313Path,
        phase321Path = Phase321Path,
        phase324Path = Phase324Path,
        phase326Path = Phase326Path,
        peskinTakeuchiOstiUrl,
        pdg2025ElectroweakReviewUrl,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true };
File.WriteAllText(
    Path.Combine(outputDir, "oblique_precision_electroweak_source_audit.json"),
    JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "oblique_precision_electroweak_source_audit_summary.json"),
    JsonSerializer.Serialize(result, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"obliquePrecisionElectroweakSourceAuditPassed={obliquePrecisionElectroweakSourceAuditPassed}");
Console.WriteLine($"obliqueParametersSummarizeVacuumPolarizationCorrections={obliqueParametersSummarizeVacuumPolarizationCorrections}");
Console.WriteLine($"obliqueRouteProvidesWeakMixingAngleSource={obliqueRouteProvidesWeakMixingAngleSource}");
Console.WriteLine($"obliqueRouteProvidesAbsoluteWzScale={obliqueRouteProvidesAbsoluteWzScale}");
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
