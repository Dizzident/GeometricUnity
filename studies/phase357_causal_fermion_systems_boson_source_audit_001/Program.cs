using System.Text.Json;

const string DefaultOutputDir = "studies/phase357_causal_fermion_systems_boson_source_audit_001/output";
const string Phase201Path = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase256Path = "studies/phase256_observed_field_extraction_intake_contract_001/output/observed_field_extraction_intake_contract_summary.json";
const string Phase317Path = "studies/phase317_electroweak_mass_matrix_bridge_source_audit_001/output/electroweak_mass_matrix_bridge_source_audit_summary.json";
const string Phase321Path = "studies/phase321_neutral_electroweak_mixing_source_audit_001/output/neutral_electroweak_mixing_source_audit_summary.json";
const string Phase322Path = "studies/phase322_higgs_upsilon_scalar_source_boundary_audit_001/output/higgs_upsilon_scalar_source_boundary_audit_summary.json";
const string Phase323Path = "studies/phase323_coupled_yang_mills_higgs_mass_extraction_audit_001/output/coupled_yang_mills_higgs_mass_extraction_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE357_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase256 = JsonDocument.Parse(File.ReadAllText(Phase256Path));
using var phase317 = JsonDocument.Parse(File.ReadAllText(Phase317Path));
using var phase321 = JsonDocument.Parse(File.ReadAllText(Phase321Path));
using var phase322 = JsonDocument.Parse(File.ReadAllText(Phase322Path));
using var phase323 = JsonDocument.Parse(File.ReadAllText(Phase323Path));

const string causalFermionMonographArxiv = "https://arxiv.org/abs/1605.04742";
const string causalFermionMonographDoi = "https://doi.org/10.48550/arXiv.1605.04742";
const string causalFermionSpringerDoi = "https://doi.org/10.1007/978-3-319-42067-7";
const string causalFermionStandardModelPage = "https://causal-fermion-system.com/theory/physics/sm-and-gr/";
const string causalFermionResearchPage = "https://causal-fermion-system.com/research/";
const string causalFermionHiggsFuturePage = "https://causal-fermion-system.com/research/research_projects2/";

const bool causalFermionSystemsBosonSourceAuditPassedExpected = true;
const bool causalFermionSystemsLeadPresent = true;
const bool causalFermionSystemsPrimarySourcesReviewed = true;
const bool causalFermionSystemsRouteExternalToGu = true;
const bool routeUsesCausalActionPrinciple = true;
const bool routeEncodesSpaceTimeAsOperators = true;
const bool routeDerivesStandardModelGaugeFieldsInContinuumLimit = true;
const bool routeDerivesGravityInContinuumLimit = true;
const bool routeDerivesMassiveLeftHandedBosonicPotentials = true;
const bool routeProvidesElectroweakAndStrongEquationsAfterSymmetryBreaking = true;
const bool routeIdentifiesHiggsScalarDegrees = true;
const bool routeHiggsDynamicsNotWorkedOut = true;
const bool routeHiggsContinuumLimitTaskNotStarted = true;
const bool routeBosonicMassesRegularizationDependent = true;
const bool routeCouplingsRegularizationDependent = true;
const bool routeRegularizationParametersCurrentlyEmpirical = true;
const bool routeRequiresFermionMassInputs = true;
const bool routeDoesNotPredictObservedWzMasses = true;
const bool routeDoesNotPredictObservedHiggsMass = true;
const bool routeDoesNotProvidePhysicalPoleExtraction = true;
const bool routeDoesNotProvideObservedPhotonWzHiggsProjection = true;

const bool routeRequiresExternalCausalFermionSystemVacuumChoice = true;
const bool routeRequiresExternalSectorAndGenerationInputs = true;
const bool routeRequiresExternalDiracSeaMassInputs = true;
const bool routeRequiresUnknownRegularizationParameters = true;
const bool routeRequiresHiggsDynamicsCompletion = true;
const bool routeRequiresContinuumLimitOneLightConeOrderLower = true;
const bool routeRequiresObservedMassComparison = true;

const bool routeRequiresGuLocalCausalActionMap = true;
const bool routeRequiresGuOperatorSpaceTimeEmbedding = true;
const bool routeRequiresGuWzSourceRows = true;
const bool routeRequiresGuWeakMixingAngleSource = true;
const bool routeRequiresGuObservedFieldExtraction = true;
const bool routeRequiresGuHiggsScalarSourceOperator = true;
const bool routeRequiresGuHiggsSelfCouplingSource = true;
const bool routeRequiresTargetIndependentVevOrMassScale = true;
const bool routeRequiresGeVUnitNormalization = true;

const bool routeProvidesGuLocalCausalActionMap = false;
const bool routeProvidesGuOperatorSpaceTimeEmbedding = false;
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

const int latestMonographArxivVersion = 3;
const int latestMonographRevisionYear = 2018;
const int monographPageCount = 457;
const int standardModelGenerationCondition = 3;
const int sourceRowCountExpected = 4;

var phase201AllRequiredLineagesPromotable = JsonBool(phase201.RootElement, "allRequiredLineagesPromotable") is true;
var existingEvidenceFound = JsonBool(phase213.RootElement, "existingEvidenceFound") is true;
var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;
var observedFieldExtractionFilledRequiredFieldCount = JsonInt(phase256.RootElement, "filledRequiredFieldCount") ?? -1;
var observedFieldExtractionContractPromotable = JsonBool(phase256.RootElement, "observedFieldExtractionContractPromotable") is true;
var electroweakMassMatrixBridgePromotesWz = JsonBool(phase317.RootElement, "smMassMatrixPromotesWzMasses") is true;
var electroweakMassMatrixBridgePromotesHiggs = JsonBool(phase317.RootElement, "smMassMatrixPromotesHiggsMass") is true;
var neutralMixingCanFillWz = JsonBool(phase321.RootElement, "canFillPhase201WzContract") is true;
var higgsUpsilonCanFillHiggs = JsonBool(phase322.RootElement, "canFillPhase201HiggsContract") is true;
var coupledYangMillsHiggsCanFillWz = JsonBool(phase323.RootElement, "canFillPhase201WzContract") is true;
var coupledYangMillsHiggsCanFillHiggs = JsonBool(phase323.RootElement, "canFillPhase201HiggsContract") is true;

var sourceRows = new[]
{
    new SourceRow(
        "arxiv-1605-04742-causal-fermion-continuum-limit",
        causalFermionMonographArxiv,
        "The Continuum Limit of Causal Fermion Systems",
        "Monograph source for causal action, continuum limit, standard-model interactions, bosonic masses and couplings as regularization-dependent quantities.",
        "Strong geometric/variational source lead, but it leaves regularization parameters empirical and Higgs dynamics incomplete."),
    new SourceRow(
        "cfs-standard-model-and-general-relativity",
        causalFermionStandardModelPage,
        "CFS Standard Model and General Relativity overview",
        "Official CFS page summarizing Standard Model plus gravity results, massive left-handed bosonic potentials, and scalar degrees identifiable with the Higgs field.",
        "States that the corresponding Higgs field equations have not yet been worked out."),
    new SourceRow(
        "cfs-research-standard-model-beyond",
        causalFermionResearchPage,
        "CFS research page on Standard Model and beyond",
        "Official research page recording that gauge fields of the Standard Model have been derived but the Higgs particle remains the missing piece.",
        "Classifies the Higgs description as future work, not current prediction evidence."),
    new SourceRow(
        "cfs-future-perspectives-higgs-particle",
        causalFermionHiggsFuturePage,
        "CFS future perspectives: description of the Higgs particle",
        "Official future-project note saying Higgs scalar degrees are known in multi-sector models but their dynamics has not been worked out and the lower light-cone continuum-limit analysis has not started.",
        "Directly blocks Higgs source-lineage promotion and confirms the work is not complete.")
};

var checks = new[]
{
    new Check(
        "causal-fermion-primary-sources-reviewed",
        causalFermionSystemsLeadPresent
            && causalFermionSystemsPrimarySourcesReviewed
            && causalFermionSystemsRouteExternalToGu
            && sourceRows.Length == sourceRowCountExpected,
        $"lead={causalFermionSystemsLeadPresent}; reviewed={causalFermionSystemsPrimarySourcesReviewed}; externalToGu={causalFermionSystemsRouteExternalToGu}; sourceRows={sourceRows.Length}"),
    new Check(
        "cfs-geometric-standard-model-lead-captured",
        routeUsesCausalActionPrinciple
            && routeEncodesSpaceTimeAsOperators
            && routeDerivesStandardModelGaugeFieldsInContinuumLimit
            && routeDerivesGravityInContinuumLimit
            && routeDerivesMassiveLeftHandedBosonicPotentials
            && routeProvidesElectroweakAndStrongEquationsAfterSymmetryBreaking
            && latestMonographArxivVersion == 3
            && latestMonographRevisionYear == 2018
            && monographPageCount == 457
            && standardModelGenerationCondition == 3,
        $"causalAction={routeUsesCausalActionPrinciple}; operators={routeEncodesSpaceTimeAsOperators}; smGauge={routeDerivesStandardModelGaugeFieldsInContinuumLimit}; gravity={routeDerivesGravityInContinuumLimit}; massiveLeft={routeDerivesMassiveLeftHandedBosonicPotentials}; ewStrongAfterSsb={routeProvidesElectroweakAndStrongEquationsAfterSymmetryBreaking}; arxivV={latestMonographArxivVersion}; revisionYear={latestMonographRevisionYear}; pageCount={monographPageCount}; generations={standardModelGenerationCondition}"),
    new Check(
        "cfs-higgs-and-regularization-blockers-captured",
        routeIdentifiesHiggsScalarDegrees
            && routeHiggsDynamicsNotWorkedOut
            && routeHiggsContinuumLimitTaskNotStarted
            && routeBosonicMassesRegularizationDependent
            && routeCouplingsRegularizationDependent
            && routeRegularizationParametersCurrentlyEmpirical
            && routeRequiresFermionMassInputs
            && routeDoesNotPredictObservedWzMasses
            && routeDoesNotPredictObservedHiggsMass
            && routeDoesNotProvidePhysicalPoleExtraction
            && routeDoesNotProvideObservedPhotonWzHiggsProjection,
        $"higgsDegrees={routeIdentifiesHiggsScalarDegrees}; higgsDynamicsOpen={routeHiggsDynamicsNotWorkedOut}; lowerLightConeNotStarted={routeHiggsContinuumLimitTaskNotStarted}; bosonMassesRegularization={routeBosonicMassesRegularizationDependent}; couplingsRegularization={routeCouplingsRegularizationDependent}; empiricalRegularization={routeRegularizationParametersCurrentlyEmpirical}; fermionMassInputs={routeRequiresFermionMassInputs}; observedWz={routeDoesNotPredictObservedWzMasses}; observedHiggs={routeDoesNotPredictObservedHiggsMass}; poleExtraction={routeDoesNotProvidePhysicalPoleExtraction}; observedProjection={routeDoesNotProvideObservedPhotonWzHiggsProjection}"),
    new Check(
        "adjacent-electroweak-and-higgs-boundaries-remain-nonpromotional",
        !electroweakMassMatrixBridgePromotesWz
            && !electroweakMassMatrixBridgePromotesHiggs
            && !neutralMixingCanFillWz
            && !higgsUpsilonCanFillHiggs
            && !coupledYangMillsHiggsCanFillWz
            && !coupledYangMillsHiggsCanFillHiggs,
        $"massMatrixPromotesWz={electroweakMassMatrixBridgePromotesWz}; massMatrixPromotesHiggs={electroweakMassMatrixBridgePromotesHiggs}; neutralMixingCanFillWz={neutralMixingCanFillWz}; higgsUpsilonCanFillHiggs={higgsUpsilonCanFillHiggs}; coupledYmhCanFillWz={coupledYangMillsHiggsCanFillWz}; coupledYmhCanFillHiggs={coupledYangMillsHiggsCanFillHiggs}"),
    new Check(
        "external-inputs-and-open-work-required-before-promotion",
        routeRequiresExternalCausalFermionSystemVacuumChoice
            && routeRequiresExternalSectorAndGenerationInputs
            && routeRequiresExternalDiracSeaMassInputs
            && routeRequiresUnknownRegularizationParameters
            && routeRequiresHiggsDynamicsCompletion
            && routeRequiresContinuumLimitOneLightConeOrderLower
            && routeRequiresObservedMassComparison,
        $"vacuumChoice={routeRequiresExternalCausalFermionSystemVacuumChoice}; sectors={routeRequiresExternalSectorAndGenerationInputs}; diracSeaMasses={routeRequiresExternalDiracSeaMassInputs}; regularization={routeRequiresUnknownRegularizationParameters}; higgsDynamics={routeRequiresHiggsDynamicsCompletion}; lowerLightCone={routeRequiresContinuumLimitOneLightConeOrderLower}; observedComparison={routeRequiresObservedMassComparison}"),
    new Check(
        "route-does-not-fill-gu-contracts",
        !routeProvidesGuLocalCausalActionMap
            && !routeProvidesGuOperatorSpaceTimeEmbedding
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
        $"guCausalMap={routeProvidesGuLocalCausalActionMap}; guOperatorEmbedding={routeProvidesGuOperatorSpaceTimeEmbedding}; guWzRows={routeProvidesGuWzSourceRows}; weakAngle={routeProvidesGuWeakMixingAngleSource}; observed={routeProvidesGuObservedFieldExtraction}; higgsOperator={routeProvidesGuHiggsScalarSourceOperator}; selfCoupling={routeProvidesGuHiggsSelfCouplingSource}; scale={routeProvidesTargetIndependentVevOrMassScale}; gev={routeProvidesGeVUnitNormalization}; promotesWz={routePromotesWzMasses}; promotesHiggs={routePromotesHiggsMass}; completes={routeCompletesBosonPredictions}"),
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

var causalFermionSystemsBosonSourceAuditPassed = checks.All(check => check.Passed)
    && causalFermionSystemsBosonSourceAuditPassedExpected
    && !routePromotesWzMasses
    && !routePromotesHiggsMass
    && !routeCompletesBosonPredictions
    && !canFillPhase201WzContract
    && !canFillPhase201HiggsContract
    && !canFillPhase256ObservedFieldExtractionContract;

var terminalStatus = causalFermionSystemsBosonSourceAuditPassed
    ? "causal-fermion-systems-boson-source-audit-open-higgs-and-regularization-not-gu-source"
    : "causal-fermion-systems-boson-source-audit-review-required";

var result = new
{
    phaseId = "phase357-causal-fermion-systems-boson-source-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    causalFermionSystemsBosonSourceAuditPassed,
    causalFermionSystemsLeadPresent,
    causalFermionSystemsPrimarySourcesReviewed,
    causalFermionSystemsRouteExternalToGu,
    causalFermionMonographDoi,
    causalFermionSpringerDoi,
    routeUsesCausalActionPrinciple,
    routeEncodesSpaceTimeAsOperators,
    routeDerivesStandardModelGaugeFieldsInContinuumLimit,
    routeDerivesGravityInContinuumLimit,
    routeDerivesMassiveLeftHandedBosonicPotentials,
    routeProvidesElectroweakAndStrongEquationsAfterSymmetryBreaking,
    routeIdentifiesHiggsScalarDegrees,
    routeHiggsDynamicsNotWorkedOut,
    routeHiggsContinuumLimitTaskNotStarted,
    routeBosonicMassesRegularizationDependent,
    routeCouplingsRegularizationDependent,
    routeRegularizationParametersCurrentlyEmpirical,
    routeRequiresFermionMassInputs,
    routeDoesNotPredictObservedWzMasses,
    routeDoesNotPredictObservedHiggsMass,
    routeDoesNotProvidePhysicalPoleExtraction,
    routeDoesNotProvideObservedPhotonWzHiggsProjection,
    latestMonographArxivVersion,
    latestMonographRevisionYear,
    monographPageCount,
    standardModelGenerationCondition,
    routeRequiresExternalCausalFermionSystemVacuumChoice,
    routeRequiresExternalSectorAndGenerationInputs,
    routeRequiresExternalDiracSeaMassInputs,
    routeRequiresUnknownRegularizationParameters,
    routeRequiresHiggsDynamicsCompletion,
    routeRequiresContinuumLimitOneLightConeOrderLower,
    routeRequiresObservedMassComparison,
    routeRequiresGuLocalCausalActionMap,
    routeRequiresGuOperatorSpaceTimeEmbedding,
    routeRequiresGuWzSourceRows,
    routeRequiresGuWeakMixingAngleSource,
    routeRequiresGuObservedFieldExtraction,
    routeRequiresGuHiggsScalarSourceOperator,
    routeRequiresGuHiggsSelfCouplingSource,
    routeRequiresTargetIndependentVevOrMassScale,
    routeRequiresGeVUnitNormalization,
    routeProvidesGuLocalCausalActionMap,
    routeProvidesGuOperatorSpaceTimeEmbedding,
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
    sourceRowCount = sourceRows.Length,
    sourceRows,
    adjacentRouteBoundary = new
    {
        electroweakMassMatrixBridgePromotesWz,
        electroweakMassMatrixBridgePromotesHiggs,
        neutralMixingCanFillWz,
        higgsUpsilonCanFillHiggs,
        coupledYangMillsHiggsCanFillWz,
        coupledYangMillsHiggsCanFillHiggs
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
    decision = "Do not promote W/Z or Higgs physical masses from causal fermion systems in this repository. The route is a serious geometric variational Standard Model lead, but current primary sources leave Higgs dynamics unworked out, treat bosonic masses and couplings as functions of unknown regularization parameters, require fermion mass/vacuum/sector inputs, and supply no GU-local W/Z source rows, observed photon/W/Z/H projection, physical pole extraction, target-independent scale, Higgs scalar-source/self-coupling lineage, or GeV normalization.",
    nextRequiredArtifact = new[]
    {
        "A GU-local map from Shiab/observer-sector geometry to a causal-action or operator-spacetime structure before using this route as more than an analogy.",
        "A target-independent regularization law deriving W/Z masses, weak mixing, coupling normalization, and GeV scale without fitting empirical regularization parameters.",
        "A completed Higgs dynamics calculation one order lower on the light cone, including scalar source/operator, self-coupling or excitation relation, and stability sidecars.",
        "Observed photon/W/Z/H extraction and complex-pole or equivalent physical mass projection before any promotion."
    }
};

var options = new JsonSerializerOptions { WriteIndented = true };
File.WriteAllText(Path.Combine(outputDir, "causal_fermion_systems_boson_source_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "causal_fermion_systems_boson_source_audit_summary.json"),
    JsonSerializer.Serialize(
        new
        {
            result.phaseId,
            result.terminalStatus,
            result.causalFermionSystemsBosonSourceAuditPassed,
            result.causalFermionSystemsLeadPresent,
            result.causalFermionSystemsPrimarySourcesReviewed,
            result.causalFermionSystemsRouteExternalToGu,
            result.routeUsesCausalActionPrinciple,
            result.routeEncodesSpaceTimeAsOperators,
            result.routeDerivesStandardModelGaugeFieldsInContinuumLimit,
            result.routeDerivesGravityInContinuumLimit,
            result.routeDerivesMassiveLeftHandedBosonicPotentials,
            result.routeProvidesElectroweakAndStrongEquationsAfterSymmetryBreaking,
            result.routeIdentifiesHiggsScalarDegrees,
            result.routeHiggsDynamicsNotWorkedOut,
            result.routeHiggsContinuumLimitTaskNotStarted,
            result.routeBosonicMassesRegularizationDependent,
            result.routeCouplingsRegularizationDependent,
            result.routeRegularizationParametersCurrentlyEmpirical,
            result.routeRequiresFermionMassInputs,
            result.routeDoesNotPredictObservedWzMasses,
            result.routeDoesNotPredictObservedHiggsMass,
            result.routeDoesNotProvidePhysicalPoleExtraction,
            result.routeDoesNotProvideObservedPhotonWzHiggsProjection,
            result.latestMonographArxivVersion,
            result.latestMonographRevisionYear,
            result.monographPageCount,
            result.standardModelGenerationCondition,
            result.routeRequiresExternalCausalFermionSystemVacuumChoice,
            result.routeRequiresExternalSectorAndGenerationInputs,
            result.routeRequiresExternalDiracSeaMassInputs,
            result.routeRequiresUnknownRegularizationParameters,
            result.routeRequiresHiggsDynamicsCompletion,
            result.routeRequiresContinuumLimitOneLightConeOrderLower,
            result.routeRequiresObservedMassComparison,
            result.routeRequiresGuLocalCausalActionMap,
            result.routeRequiresGuOperatorSpaceTimeEmbedding,
            result.routeRequiresGuWzSourceRows,
            result.routeRequiresGuWeakMixingAngleSource,
            result.routeRequiresGuObservedFieldExtraction,
            result.routeRequiresGuHiggsScalarSourceOperator,
            result.routeRequiresGuHiggsSelfCouplingSource,
            result.routeRequiresTargetIndependentVevOrMassScale,
            result.routeRequiresGeVUnitNormalization,
            result.routeProvidesGuLocalCausalActionMap,
            result.routeProvidesGuOperatorSpaceTimeEmbedding,
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
            result.sourceRowCount,
            result.adjacentRouteBoundary,
            result.contractImpact,
            result.decision,
            result.nextRequiredArtifact
        },
        options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"causalFermionSystemsBosonSourceAuditPassed={causalFermionSystemsBosonSourceAuditPassed}");
Console.WriteLine($"routeDerivesStandardModelGaugeFieldsInContinuumLimit={routeDerivesStandardModelGaugeFieldsInContinuumLimit}");
Console.WriteLine($"routeDerivesMassiveLeftHandedBosonicPotentials={routeDerivesMassiveLeftHandedBosonicPotentials}");
Console.WriteLine($"routeHiggsDynamicsNotWorkedOut={routeHiggsDynamicsNotWorkedOut}");
Console.WriteLine($"routeRegularizationParametersCurrentlyEmpirical={routeRegularizationParametersCurrentlyEmpirical}");
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
