using System.Text.Json;

const string DefaultOutputDir = "studies/phase330_weyl_geometric_mass_generation_source_audit_001/output";
const string Phase201Path = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase224Path = "studies/phase224_electroweak_parameter_dependency_audit_001/output/electroweak_parameter_dependency_audit_summary.json";
const string Phase256Path = "studies/phase256_observed_field_extraction_intake_contract_001/output/observed_field_extraction_intake_contract_summary.json";
const string Phase265Path = "studies/phase265_gauge_higgs_boundary_source_audit_001/output/gauge_higgs_boundary_source_audit_summary.json";
const string Phase268Path = "studies/phase268_spectral_action_boson_source_audit_001/output/spectral_action_boson_source_audit_summary.json";
const string Phase317Path = "studies/phase317_electroweak_mass_matrix_bridge_source_audit_001/output/electroweak_mass_matrix_bridge_source_audit_summary.json";
const string Phase323Path = "studies/phase323_coupled_yang_mills_higgs_mass_extraction_audit_001/output/coupled_yang_mills_higgs_mass_extraction_audit_summary.json";
const string Phase324Path = "studies/phase324_custodial_rho_parameter_source_audit_001/output/custodial_rho_parameter_source_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE330_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase224 = JsonDocument.Parse(File.ReadAllText(Phase224Path));
using var phase256 = JsonDocument.Parse(File.ReadAllText(Phase256Path));
using var phase265 = JsonDocument.Parse(File.ReadAllText(Phase265Path));
using var phase268 = JsonDocument.Parse(File.ReadAllText(Phase268Path));
using var phase317 = JsonDocument.Parse(File.ReadAllText(Phase317Path));
using var phase323 = JsonDocument.Parse(File.ReadAllText(Phase323Path));
using var phase324 = JsonDocument.Parse(File.ReadAllText(Phase324Path));

const string arxivWeylGeometricMassGenerationUrl = "https://arxiv.org/abs/2605.02955";
const string arxivWeylGeometricMassGenerationPdfUrl = "https://arxiv.org/pdf/2605.02955";

const bool arxivWeylGeometricMassGenerationLeadPresent = true;
const bool arxivWeylGeometricMassGenerationSubmittedMay2026 = true;
const bool arxivWeylGeometricMassGenerationPrimarySourceReviewed = true;
const bool weylRouteExternalToGu = true;
const bool weylRouteUsesStandardModelGaugeGroup = true;
const bool weylRouteConstructsWeylSu2U1InvariantTheory = true;
const bool weylRouteUsesStueckelbergMechanism = true;
const bool weylRouteProducesEinsteinHilbertAndProcaActions = true;
const bool weylRouteProducesHiggsPotential = true;
const bool weylRouteReproducesStandardModelMassGeneration = true;
const bool weylRouteIncludesWzMassTerms = true;
const bool weylRouteIncludesHiggsMassFormula = true;
const bool weylRouteIncludesWeylGaugeBosonMassFormula = true;
const bool weylRouteAddsHiggsWeylCouplings = true;
const bool weylRouteComparesToObservedHiggsMass = true;
const bool weylRouteComparesToObservedHiggsVev = true;
const bool weylRouteUsesPlanckScaleInput = true;
const bool weylRouteLeavesElectroweakCouplingsAsModelInputs = true;

const bool weylRouteProvidesGuLocalWzTheorem = false;
const bool weylRouteProvidesSeparateWzSourceRows = false;
const bool weylRouteProvidesWzRawAmplitudeGates = false;
const bool weylRouteProvidesWzCommonBridgeGate = false;
const bool weylRouteProvidesTargetIndependentGuVevSource = false;
const bool weylRouteProvidesWeakMixingAngleSource = false;
const bool weylRouteProvidesGuGaugeCouplingNormalization = false;
const bool weylRouteProvidesObservedPhotonWzProjectionRows = false;
const bool weylRouteProvidesNeutralMassMatrixDiagonalization = false;
const bool weylRouteProvidesGuObservedFieldExtraction = false;
const bool weylRouteProvidesHiggsScalarSourceOperator = false;
const bool weylRouteProvidesHiggsQuarticOrExcitationSource = false;
const bool weylRouteProvidesTargetIndependentHiggsMass = false;
const bool weylRouteProvidesGeVUnitNormalization = false;
const bool weylRoutePromotesWzMasses = false;
const bool weylRoutePromotesHiggsMass = false;
const bool weylRouteCompletesBosonPredictions = false;
const bool canFillPhase201WzContract = false;
const bool canFillPhase201HiggsContract = false;
const bool canFillPhase256ObservedFieldExtractionContract = false;

var phase201AllRequiredLineagesPromotable = JsonBool(phase201.RootElement, "allRequiredLineagesPromotable") is true;
var existingEvidenceFound = JsonBool(phase213.RootElement, "existingEvidenceFound") is true;
var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;
var observedFieldExtractionRequiredFieldCount = JsonInt(phase256.RootElement, "requiredFieldCount") ?? -1;
var observedFieldExtractionFilledRequiredFieldCount = JsonInt(phase256.RootElement, "filledRequiredFieldCount") ?? -1;
var observedFieldExtractionContractPromotable = JsonBool(phase256.RootElement, "observedFieldExtractionContractPromotable") is true;

var electroweakParameterAuditPassed = JsonBool(phase224.RootElement, "electroweakParameterAuditPassed") is true;
var wAbsoluteMassParameterClosure = JsonBool(phase224.RootElement, "wAbsoluteMassParameterClosure") is true;
var zAbsoluteMassParameterClosure = JsonBool(phase224.RootElement, "zAbsoluteMassParameterClosure") is true;
var higgsMassParameterClosure = JsonBool(phase224.RootElement, "higgsMassParameterClosure") is true;
var weakCouplingSourcePromotable = JsonBool(phase224.RootElement, "weakCouplingSourcePromotable") is true;

var gaugeHiggsBoundarySourceAuditPassed = JsonBool(phase265.RootElement, "gaugeHiggsBoundarySourceAuditPassed") is true;
var gaugeHiggsBoundaryPromotesHiggsMass = JsonBool(phase265.RootElement, "gaugeHiggsBoundaryPromotesHiggsMass") is true;
var gaugeHiggsBoundaryCompletesBosonPredictions = JsonBool(phase265.RootElement, "gaugeHiggsBoundaryCompletesBosonPredictions") is true;

var spectralActionBosonSourceAuditPassed = JsonBool(phase268.RootElement, "spectralActionBosonSourceAuditPassed") is true;
var spectralActionPromotesWzMasses = JsonBool(phase268.RootElement, "spectralActionPromotesWzMasses") is true;
var spectralActionPromotesHiggsMass = JsonBool(phase268.RootElement, "spectralActionPromotesHiggsMass") is true;

var electroweakMassMatrixBridgeSourceAuditPassed = JsonBool(phase317.RootElement, "electroweakMassMatrixBridgeSourceAuditPassed") is true;
var smMassGenerationRequiresVev = JsonBool(phase317.RootElement, "smMassGenerationRequiresVev") is true;
var smMassGenerationRequiresWeakCouplingG = JsonBool(phase317.RootElement, "smMassGenerationRequiresWeakCouplingG") is true;
var smMassGenerationRequiresHyperchargeCouplingGPrime = JsonBool(phase317.RootElement, "smMassGenerationRequiresHyperchargeCouplingGPrime") is true;
var smTreeLevelMwDependsOnGAndV = JsonBool(phase317.RootElement, "smTreeLevelMwDependsOnGAndV") is true;
var smTreeLevelMzDependsOnGAndGPrimeAndV = JsonBool(phase317.RootElement, "smTreeLevelMzDependsOnGAndGPrimeAndV") is true;
var smTreeLevelHiggsMassDependsOnPotentialParameter = JsonBool(phase317.RootElement, "smTreeLevelHiggsMassDependsOnPotentialParameter") is true;
var smMassMatrixProvidesExternalDependencyMap = JsonBool(phase317.RootElement, "smMassMatrixProvidesExternalDependencyMap") is true;
var smMassMatrixPromotesWzMasses = JsonBool(phase317.RootElement, "smMassMatrixPromotesWzMasses") is true;
var smMassMatrixPromotesHiggsMass = JsonBool(phase317.RootElement, "smMassMatrixPromotesHiggsMass") is true;

var coupledYangMillsHiggsMassExtractionAuditPassed = JsonBool(phase323.RootElement, "coupledYangMillsHiggsMassExtractionAuditPassed") is true;
var officialPublicSourcesProvideTargetIndependentVevSource = JsonBool(phase323.RootElement, "officialPublicSourcesProvideTargetIndependentVevSource") is true;
var officialPublicSourcesProvideGaugeCouplingNormalization = JsonBool(phase323.RootElement, "officialPublicSourcesProvideGaugeCouplingNormalization") is true;
var officialPublicSourcesProvidePhotonWzHiggsProjectionRows = JsonBool(phase323.RootElement, "officialPublicSourcesProvidePhotonWzHiggsProjectionRows") is true;
var officialPublicSourcesProvideHiggsScalarSelfCouplingSource = JsonBool(phase323.RootElement, "officialPublicSourcesProvideHiggsScalarSelfCouplingSource") is true;
var coupledYangMillsHiggsRouteCompletesBosonPredictions = JsonBool(phase323.RootElement, "coupledYangMillsHiggsRouteCompletesBosonPredictions") is true;

var custodialRhoParameterSourceAuditPassed = JsonBool(phase324.RootElement, "custodialRhoParameterSourceAuditPassed") is true;
var rhoRelationProvidesAbsoluteWzScale = JsonBool(phase324.RootElement, "rhoRelationProvidesAbsoluteWzScale") is true;
var rhoRelationProvidesWeakMixingAngleSource = JsonBool(phase324.RootElement, "rhoRelationProvidesWeakMixingAngleSource") is true;
var rhoRelationProvidesTargetIndependentVevSource = JsonBool(phase324.RootElement, "rhoRelationProvidesTargetIndependentVevSource") is true;

var sourceRows = new[]
{
    new SourceRow(
        "arxiv-2605-02955-abstract",
        arxivWeylGeometricMassGenerationUrl,
        "abstract",
        "The paper constructs a Weyl x SU(2)_L x U(1)_Y invariant model and states that Weyl symmetry breaking generates an Einstein-Hilbert sector, a Weyl gauge-field mass, and the Higgs potential while reproducing Standard Model mass generation.",
        "This is a real external geometric mass-generation lead, but it is not GU source-lineage evidence."),
    new SourceRow(
        "arxiv-2605-02955-lagrangian",
        arxivWeylGeometricMassGenerationPdfUrl,
        "Section 2, equations 2.1-2.4",
        "The model starts from a Weyl-invariant electroweak action with a geometric term built from the Weyl scalar curvature and Higgs norm.",
        "It supplies an external model Lagrangian, not a GU-local derivation or observed-field extraction theorem."),
    new SourceRow(
        "arxiv-2605-02955-broken-mass-terms",
        arxivWeylGeometricMassGenerationPdfUrl,
        "Section 3, equation 3.20",
        "After symmetry breaking the action contains W, Z, Higgs, Yukawa, Weyl-gauge-boson, and Higgs-Weyl interaction terms.",
        "The W/Z terms still use the Standard Model g, g-prime, and v dependency shape already classified by Phase317."),
    new SourceRow(
        "arxiv-2605-02955-parameter-comparison",
        arxivWeylGeometricMassGenerationPdfUrl,
        "Section 4, equations 4.1-4.2",
        "The paper fixes parameters by comparing its Higgs and vacuum-expectation formulas with observed Standard Model quantities.",
        "That makes the known Higgs/VEV use calibrational for this repository, not a target-independent GU prediction source."),
};

var checks = new[]
{
    new Check(
        "weyl-geometric-mass-generation-source-reviewed",
        arxivWeylGeometricMassGenerationLeadPresent
            && arxivWeylGeometricMassGenerationSubmittedMay2026
            && arxivWeylGeometricMassGenerationPrimarySourceReviewed
            && weylRouteExternalToGu
            && weylRouteUsesStandardModelGaugeGroup
            && sourceRows.Length == 4,
        $"leadPresent={arxivWeylGeometricMassGenerationLeadPresent}; submittedMay2026={arxivWeylGeometricMassGenerationSubmittedMay2026}; primaryReviewed={arxivWeylGeometricMassGenerationPrimarySourceReviewed}; externalToGu={weylRouteExternalToGu}; smGaugeGroup={weylRouteUsesStandardModelGaugeGroup}; sourceRowCount={sourceRows.Length}"),
    new Check(
        "external-weyl-model-mass-generation-structure-recorded",
        weylRouteConstructsWeylSu2U1InvariantTheory
            && weylRouteUsesStueckelbergMechanism
            && weylRouteProducesEinsteinHilbertAndProcaActions
            && weylRouteProducesHiggsPotential
            && weylRouteReproducesStandardModelMassGeneration
            && weylRouteIncludesWzMassTerms
            && weylRouteIncludesHiggsMassFormula
            && weylRouteIncludesWeylGaugeBosonMassFormula
            && weylRouteAddsHiggsWeylCouplings,
        $"weylSu2U1={weylRouteConstructsWeylSu2U1InvariantTheory}; stueckelberg={weylRouteUsesStueckelbergMechanism}; einsteinHilbert={weylRouteProducesEinsteinHilbertAndProcaActions}; higgsPotential={weylRouteProducesHiggsPotential}; smMassGeneration={weylRouteReproducesStandardModelMassGeneration}; wzTerms={weylRouteIncludesWzMassTerms}; higgsFormula={weylRouteIncludesHiggsMassFormula}; omegaFormula={weylRouteIncludesWeylGaugeBosonMassFormula}; higgsWeylCouplings={weylRouteAddsHiggsWeylCouplings}"),
    new Check(
        "weyl-model-uses-observed-inputs-and-standard-parameter-shape",
        electroweakMassMatrixBridgeSourceAuditPassed
            && smMassGenerationRequiresVev
            && smMassGenerationRequiresWeakCouplingG
            && smMassGenerationRequiresHyperchargeCouplingGPrime
            && smTreeLevelMwDependsOnGAndV
            && smTreeLevelMzDependsOnGAndGPrimeAndV
            && smTreeLevelHiggsMassDependsOnPotentialParameter
            && smMassMatrixProvidesExternalDependencyMap
            && !smMassMatrixPromotesWzMasses
            && !smMassMatrixPromotesHiggsMass
            && weylRouteComparesToObservedHiggsMass
            && weylRouteComparesToObservedHiggsVev
            && weylRouteUsesPlanckScaleInput
            && weylRouteLeavesElectroweakCouplingsAsModelInputs,
        $"p317Passed={electroweakMassMatrixBridgeSourceAuditPassed}; requiresV={smMassGenerationRequiresVev}; requiresG={smMassGenerationRequiresWeakCouplingG}; requiresGPrime={smMassGenerationRequiresHyperchargeCouplingGPrime}; mwGv={smTreeLevelMwDependsOnGAndV}; mzGGPrimeV={smTreeLevelMzDependsOnGAndGPrimeAndV}; higgsPotentialParameter={smTreeLevelHiggsMassDependsOnPotentialParameter}; externalMap={smMassMatrixProvidesExternalDependencyMap}; comparesMh={weylRouteComparesToObservedHiggsMass}; comparesV={weylRouteComparesToObservedHiggsVev}; leavesElectroweakCouplings={weylRouteLeavesElectroweakCouplingsAsModelInputs}"),
    new Check(
        "weyl-route-does-not-supply-gu-wz-source-lineage",
        electroweakParameterAuditPassed
            && !wAbsoluteMassParameterClosure
            && !zAbsoluteMassParameterClosure
            && !weakCouplingSourcePromotable
            && custodialRhoParameterSourceAuditPassed
            && !rhoRelationProvidesAbsoluteWzScale
            && !rhoRelationProvidesWeakMixingAngleSource
            && !rhoRelationProvidesTargetIndependentVevSource
            && !weylRouteProvidesGuLocalWzTheorem
            && !weylRouteProvidesSeparateWzSourceRows
            && !weylRouteProvidesWzRawAmplitudeGates
            && !weylRouteProvidesWzCommonBridgeGate
            && !weylRouteProvidesTargetIndependentGuVevSource
            && !weylRouteProvidesWeakMixingAngleSource
            && !weylRouteProvidesGuGaugeCouplingNormalization
            && !weylRouteProvidesObservedPhotonWzProjectionRows
            && !weylRouteProvidesNeutralMassMatrixDiagonalization,
        $"p224Passed={electroweakParameterAuditPassed}; wClosure={wAbsoluteMassParameterClosure}; zClosure={zAbsoluteMassParameterClosure}; weakCoupling={weakCouplingSourcePromotable}; p324Passed={custodialRhoParameterSourceAuditPassed}; rhoAbsolute={rhoRelationProvidesAbsoluteWzScale}; rhoWeakAngle={rhoRelationProvidesWeakMixingAngleSource}; guWzTheorem={weylRouteProvidesGuLocalWzTheorem}; separateRows={weylRouteProvidesSeparateWzSourceRows}; vev={weylRouteProvidesTargetIndependentGuVevSource}; weakAngle={weylRouteProvidesWeakMixingAngleSource}; couplingNorm={weylRouteProvidesGuGaugeCouplingNormalization}; observedRows={weylRouteProvidesObservedPhotonWzProjectionRows}"),
    new Check(
        "weyl-route-does-not-supply-gu-higgs-source-lineage",
        gaugeHiggsBoundarySourceAuditPassed
            && !gaugeHiggsBoundaryPromotesHiggsMass
            && !gaugeHiggsBoundaryCompletesBosonPredictions
            && spectralActionBosonSourceAuditPassed
            && !spectralActionPromotesWzMasses
            && !spectralActionPromotesHiggsMass
            && coupledYangMillsHiggsMassExtractionAuditPassed
            && !officialPublicSourcesProvideTargetIndependentVevSource
            && !officialPublicSourcesProvideGaugeCouplingNormalization
            && !officialPublicSourcesProvidePhotonWzHiggsProjectionRows
            && !officialPublicSourcesProvideHiggsScalarSelfCouplingSource
            && !coupledYangMillsHiggsRouteCompletesBosonPredictions
            && !higgsMassParameterClosure
            && !weylRouteProvidesHiggsScalarSourceOperator
            && !weylRouteProvidesHiggsQuarticOrExcitationSource
            && !weylRouteProvidesTargetIndependentHiggsMass,
        $"p265Passed={gaugeHiggsBoundarySourceAuditPassed}; gaugeHiggsPromotes={gaugeHiggsBoundaryPromotesHiggsMass}; p268Passed={spectralActionBosonSourceAuditPassed}; spectralPromotesWz={spectralActionPromotesWzMasses}; spectralPromotesHiggs={spectralActionPromotesHiggsMass}; p323Passed={coupledYangMillsHiggsMassExtractionAuditPassed}; p323Vev={officialPublicSourcesProvideTargetIndependentVevSource}; p323ProjectionRows={officialPublicSourcesProvidePhotonWzHiggsProjectionRows}; p323SelfCoupling={officialPublicSourcesProvideHiggsScalarSelfCouplingSource}; higgsClosure={higgsMassParameterClosure}; weylHiggsOperator={weylRouteProvidesHiggsScalarSourceOperator}; weylHiggsSelfCoupling={weylRouteProvidesHiggsQuarticOrExcitationSource}"),
    new Check(
        "source-lineage-contracts-remain-unfilled",
        !phase201AllRequiredLineagesPromotable
            && !existingEvidenceFound
            && wzMissingFieldCount == 15
            && higgsMissingFieldCount == 14
            && observedFieldExtractionRequiredFieldCount == 20
            && observedFieldExtractionFilledRequiredFieldCount == 0
            && !observedFieldExtractionContractPromotable
            && !weylRouteProvidesGuObservedFieldExtraction
            && !weylRouteProvidesGeVUnitNormalization
            && !weylRoutePromotesWzMasses
            && !weylRoutePromotesHiggsMass
            && !weylRouteCompletesBosonPredictions
            && !canFillPhase201WzContract
            && !canFillPhase201HiggsContract
            && !canFillPhase256ObservedFieldExtractionContract,
        $"phase201Promotable={phase201AllRequiredLineagesPromotable}; existingEvidenceFound={existingEvidenceFound}; wzMissing={wzMissingFieldCount}; higgsMissing={higgsMissingFieldCount}; phase256Required={observedFieldExtractionRequiredFieldCount}; phase256Filled={observedFieldExtractionFilledRequiredFieldCount}; observedPromotable={observedFieldExtractionContractPromotable}; guObservedExtraction={weylRouteProvidesGuObservedFieldExtraction}; geV={weylRouteProvidesGeVUnitNormalization}; promotesWz={weylRoutePromotesWzMasses}; promotesHiggs={weylRoutePromotesHiggsMass}; completes={weylRouteCompletesBosonPredictions}"),
};

var weylGeometricMassGenerationSourceAuditPassed = checks.All(check => check.Passed)
    && weylRouteExternalToGu
    && weylRouteConstructsWeylSu2U1InvariantTheory
    && weylRouteReproducesStandardModelMassGeneration
    && weylRouteComparesToObservedHiggsMass
    && weylRouteComparesToObservedHiggsVev
    && weylRouteLeavesElectroweakCouplingsAsModelInputs
    && !weylRouteProvidesGuLocalWzTheorem
    && !weylRouteProvidesSeparateWzSourceRows
    && !weylRouteProvidesWzRawAmplitudeGates
    && !weylRouteProvidesWzCommonBridgeGate
    && !weylRouteProvidesTargetIndependentGuVevSource
    && !weylRouteProvidesWeakMixingAngleSource
    && !weylRouteProvidesGuGaugeCouplingNormalization
    && !weylRouteProvidesObservedPhotonWzProjectionRows
    && !weylRouteProvidesNeutralMassMatrixDiagonalization
    && !weylRouteProvidesGuObservedFieldExtraction
    && !weylRouteProvidesHiggsScalarSourceOperator
    && !weylRouteProvidesHiggsQuarticOrExcitationSource
    && !weylRouteProvidesTargetIndependentHiggsMass
    && !weylRouteProvidesGeVUnitNormalization
    && !weylRoutePromotesWzMasses
    && !weylRoutePromotesHiggsMass
    && !weylRouteCompletesBosonPredictions
    && !canFillPhase201WzContract
    && !canFillPhase201HiggsContract
    && !canFillPhase256ObservedFieldExtractionContract;

var terminalStatus = weylGeometricMassGenerationSourceAuditPassed
    ? "weyl-geometric-mass-generation-source-audit-external-model-not-gu-source"
    : "weyl-geometric-mass-generation-source-audit-review-required";

var result = new
{
    phaseId = "phase330-weyl-geometric-mass-generation-source-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    weylGeometricMassGenerationSourceAuditPassed,
    arxivWeylGeometricMassGenerationLeadPresent,
    arxivWeylGeometricMassGenerationSubmittedMay2026,
    arxivWeylGeometricMassGenerationPrimarySourceReviewed,
    weylRouteExternalToGu,
    weylRouteUsesStandardModelGaugeGroup,
    weylRouteConstructsWeylSu2U1InvariantTheory,
    weylRouteUsesStueckelbergMechanism,
    weylRouteProducesEinsteinHilbertAndProcaActions,
    weylRouteProducesHiggsPotential,
    weylRouteReproducesStandardModelMassGeneration,
    weylRouteIncludesWzMassTerms,
    weylRouteIncludesHiggsMassFormula,
    weylRouteIncludesWeylGaugeBosonMassFormula,
    weylRouteAddsHiggsWeylCouplings,
    weylRouteComparesToObservedHiggsMass,
    weylRouteComparesToObservedHiggsVev,
    weylRouteUsesPlanckScaleInput,
    weylRouteLeavesElectroweakCouplingsAsModelInputs,
    weylRouteProvidesGuLocalWzTheorem,
    weylRouteProvidesSeparateWzSourceRows,
    weylRouteProvidesWzRawAmplitudeGates,
    weylRouteProvidesWzCommonBridgeGate,
    weylRouteProvidesTargetIndependentGuVevSource,
    weylRouteProvidesWeakMixingAngleSource,
    weylRouteProvidesGuGaugeCouplingNormalization,
    weylRouteProvidesObservedPhotonWzProjectionRows,
    weylRouteProvidesNeutralMassMatrixDiagonalization,
    weylRouteProvidesGuObservedFieldExtraction,
    weylRouteProvidesHiggsScalarSourceOperator,
    weylRouteProvidesHiggsQuarticOrExcitationSource,
    weylRouteProvidesTargetIndependentHiggsMass,
    weylRouteProvidesGeVUnitNormalization,
    weylRoutePromotesWzMasses,
    weylRoutePromotesHiggsMass,
    weylRouteCompletesBosonPredictions,
    canFillPhase201WzContract,
    canFillPhase201HiggsContract,
    canFillPhase256ObservedFieldExtractionContract,
    sourceRows,
    checks,
    upstreamBoundaries = new
    {
        phase224 = new
        {
            electroweakParameterAuditPassed,
            wAbsoluteMassParameterClosure,
            zAbsoluteMassParameterClosure,
            higgsMassParameterClosure,
            weakCouplingSourcePromotable,
        },
        phase265 = new
        {
            gaugeHiggsBoundarySourceAuditPassed,
            gaugeHiggsBoundaryPromotesHiggsMass,
            gaugeHiggsBoundaryCompletesBosonPredictions,
        },
        phase268 = new
        {
            spectralActionBosonSourceAuditPassed,
            spectralActionPromotesWzMasses,
            spectralActionPromotesHiggsMass,
        },
        phase317 = new
        {
            electroweakMassMatrixBridgeSourceAuditPassed,
            smMassGenerationRequiresVev,
            smMassGenerationRequiresWeakCouplingG,
            smMassGenerationRequiresHyperchargeCouplingGPrime,
            smTreeLevelMwDependsOnGAndV,
            smTreeLevelMzDependsOnGAndGPrimeAndV,
            smTreeLevelHiggsMassDependsOnPotentialParameter,
            smMassMatrixProvidesExternalDependencyMap,
            smMassMatrixPromotesWzMasses,
            smMassMatrixPromotesHiggsMass,
        },
        phase323 = new
        {
            coupledYangMillsHiggsMassExtractionAuditPassed,
            officialPublicSourcesProvideTargetIndependentVevSource,
            officialPublicSourcesProvideGaugeCouplingNormalization,
            officialPublicSourcesProvidePhotonWzHiggsProjectionRows,
            officialPublicSourcesProvideHiggsScalarSelfCouplingSource,
            coupledYangMillsHiggsRouteCompletesBosonPredictions,
        },
        phase324 = new
        {
            custodialRhoParameterSourceAuditPassed,
            rhoRelationProvidesAbsoluteWzScale,
            rhoRelationProvidesWeakMixingAngleSource,
            rhoRelationProvidesTargetIndependentVevSource,
        },
    },
    contractImpact = new
    {
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
    decision = weylGeometricMassGenerationSourceAuditPassed
        ? "Do not promote W/Z or Higgs masses from the external Weyl geometric mass-generation route. The paper is a relevant current geometric electroweak model and derives a Weyl-invariant mechanism that reproduces Standard Model mass generation, but it fixes key parameters by comparison with observed Higgs/VEV data and still relies on Standard Model electroweak couplings, external Planck-scale normalization, and external observed-field definitions. It does not supply GU-local W/Z source rows, raw/common bridge gates, weak-angle or coupling normalization, target-independent VEV, observed photon/W/Z extraction, Higgs scalar-source/self-coupling lineage, or GeV normalization."
        : "Review the Weyl geometric mass-generation route before relying on this boundary.",
    nextRequiredArtifact = new[]
    {
        "A GU-local theorem deriving the observed SU(2)_L x U(1)_Y embedding, weak angle, W/Z source rows, and target-independent VEV without importing observed electroweak targets.",
        "A GU-local Higgs scalar-source/operator and self-coupling or excitation relation that fixes the Higgs mass rather than fitting the model parameters to observed Higgs and VEV values.",
        "A filled Phase256 observed-field extraction artifact connecting GU fields to photon/W/Z/H rows with provenance and stability sidecars.",
    },
    sourceEvidence = new
    {
        arxivWeylGeometricMassGenerationUrl,
        arxivWeylGeometricMassGenerationPdfUrl,
        phase201Path = Phase201Path,
        phase213Path = Phase213Path,
        phase224Path = Phase224Path,
        phase256Path = Phase256Path,
        phase265Path = Phase265Path,
        phase268Path = Phase268Path,
        phase317Path = Phase317Path,
        phase323Path = Phase323Path,
        phase324Path = Phase324Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "weyl_geometric_mass_generation_source_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "weyl_geometric_mass_generation_source_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.weylGeometricMassGenerationSourceAuditPassed,
        result.arxivWeylGeometricMassGenerationLeadPresent,
        result.arxivWeylGeometricMassGenerationSubmittedMay2026,
        result.arxivWeylGeometricMassGenerationPrimarySourceReviewed,
        result.weylRouteExternalToGu,
        result.weylRouteUsesStandardModelGaugeGroup,
        result.weylRouteConstructsWeylSu2U1InvariantTheory,
        result.weylRouteUsesStueckelbergMechanism,
        result.weylRouteProducesEinsteinHilbertAndProcaActions,
        result.weylRouteProducesHiggsPotential,
        result.weylRouteReproducesStandardModelMassGeneration,
        result.weylRouteIncludesWzMassTerms,
        result.weylRouteIncludesHiggsMassFormula,
        result.weylRouteIncludesWeylGaugeBosonMassFormula,
        result.weylRouteAddsHiggsWeylCouplings,
        result.weylRouteComparesToObservedHiggsMass,
        result.weylRouteComparesToObservedHiggsVev,
        result.weylRouteUsesPlanckScaleInput,
        result.weylRouteLeavesElectroweakCouplingsAsModelInputs,
        result.weylRouteProvidesGuLocalWzTheorem,
        result.weylRouteProvidesSeparateWzSourceRows,
        result.weylRouteProvidesWzRawAmplitudeGates,
        result.weylRouteProvidesWzCommonBridgeGate,
        result.weylRouteProvidesTargetIndependentGuVevSource,
        result.weylRouteProvidesWeakMixingAngleSource,
        result.weylRouteProvidesGuGaugeCouplingNormalization,
        result.weylRouteProvidesObservedPhotonWzProjectionRows,
        result.weylRouteProvidesNeutralMassMatrixDiagonalization,
        result.weylRouteProvidesGuObservedFieldExtraction,
        result.weylRouteProvidesHiggsScalarSourceOperator,
        result.weylRouteProvidesHiggsQuarticOrExcitationSource,
        result.weylRouteProvidesTargetIndependentHiggsMass,
        result.weylRouteProvidesGeVUnitNormalization,
        result.weylRoutePromotesWzMasses,
        result.weylRoutePromotesHiggsMass,
        result.weylRouteCompletesBosonPredictions,
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
Console.WriteLine($"weylGeometricMassGenerationSourceAuditPassed={weylGeometricMassGenerationSourceAuditPassed}");
Console.WriteLine($"weylRouteExternalToGu={weylRouteExternalToGu}");
Console.WriteLine($"weylRoutePromotesWzMasses={weylRoutePromotesWzMasses}");
Console.WriteLine($"weylRoutePromotesHiggsMass={weylRoutePromotesHiggsMass}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

sealed record SourceRow(string SourceId, string Url, string Locator, string Finding, string PredictionImpact);
sealed record Check(string CheckId, bool Passed, string Detail);
