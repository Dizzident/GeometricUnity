using System.Text.Json;

const string DefaultOutputDir = "studies/phase332_string_m_theory_compactification_source_audit_001/output";
const string Phase201Path = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase224Path = "studies/phase224_electroweak_parameter_dependency_audit_001/output/electroweak_parameter_dependency_audit_summary.json";
const string Phase236Path = "studies/phase236_low_energy_rg_transport_source_audit_001/output/low_energy_rg_transport_source_audit_summary.json";
const string Phase256Path = "studies/phase256_observed_field_extraction_intake_contract_001/output/observed_field_extraction_intake_contract_summary.json";
const string Phase265Path = "studies/phase265_gauge_higgs_boundary_source_audit_001/output/gauge_higgs_boundary_source_audit_summary.json";
const string Phase268Path = "studies/phase268_spectral_action_boson_source_audit_001/output/spectral_action_boson_source_audit_summary.json";
const string Phase317Path = "studies/phase317_electroweak_mass_matrix_bridge_source_audit_001/output/electroweak_mass_matrix_bridge_source_audit_summary.json";
const string Phase323Path = "studies/phase323_coupled_yang_mills_higgs_mass_extraction_audit_001/output/coupled_yang_mills_higgs_mass_extraction_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE332_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase224 = JsonDocument.Parse(File.ReadAllText(Phase224Path));
using var phase236 = JsonDocument.Parse(File.ReadAllText(Phase236Path));
using var phase256 = JsonDocument.Parse(File.ReadAllText(Phase256Path));
using var phase265 = JsonDocument.Parse(File.ReadAllText(Phase265Path));
using var phase268 = JsonDocument.Parse(File.ReadAllText(Phase268Path));
using var phase317 = JsonDocument.Parse(File.ReadAllText(Phase317Path));
using var phase323 = JsonDocument.Parse(File.ReadAllText(Phase323Path));

const string stringMTheoryHiggsPredictionUrl = "https://arxiv.org/abs/1112.1059";
const string stringMTheoryHiggsReviewUrl = "https://arxiv.org/abs/1211.2231";
const string stringTheoreticPerspectiveUrl = "https://arxiv.org/abs/1304.2767";
const string stringHiggsCalculationUrl = "https://arxiv.org/abs/2106.04622";
const string heteroticStandardModelUrl = "https://doi.org/10.1016/j.physletb.2005.12.014";

const bool stringCompactificationLeadPresent = true;
const bool stringMTheoryPrimarySourcesReviewed = true;
const bool stringCompactificationRouteExternalToGu = true;
const bool stringRouteGeometricCompactificationBased = true;
const bool stringRouteIncludesG2HolonomyMTheoryLead = true;
const bool stringRouteIncludesCalabiYauHeteroticLead = true;
const bool stringRouteIncludesFTheoryOrBraneHiggsShiftSymmetryLead = true;
const bool stringRouteIncludesFirstPrinciplesClosedStringHiggsMassFramework = true;
const bool stringRouteClaimsHiggsMassRangeNearObserved = true;
const bool stringRouteClaimsNoFreeParametersInSpecificMTheoryReview = true;
const bool stringRouteCanEngineerMssmSpectrumAndHiggsPairs = true;
const bool stringRouteRequiresSpecificCompactificationChoice = true;
const bool stringRouteRequiresModuliStabilization = true;
const bool stringRouteRequiresSupersymmetryBreakingScenario = true;
const bool stringRouteRequiresTanBetaOrHiggsSectorBoundary = true;
const bool stringRouteRequiresRgTransport = true;
const bool stringRouteRequiresYukawaTopAndGaugeInputs = true;
const bool stringRouteOftenPredictsHiggsNotWzAbsoluteRows = true;

const bool stringRouteProvidesGuLocalWzTheorem = false;
const bool stringRouteProvidesSeparateWzSourceRows = false;
const bool stringRouteProvidesWzRawAmplitudeGates = false;
const bool stringRouteProvidesWzCommonBridgeGate = false;
const bool stringRouteProvidesTargetIndependentGuVevSource = false;
const bool stringRouteProvidesWeakMixingAngleSource = false;
const bool stringRouteProvidesGuGaugeCouplingNormalization = false;
const bool stringRouteProvidesObservedPhotonWzProjectionRows = false;
const bool stringRouteProvidesNeutralMassMatrixDiagonalization = false;
const bool stringRouteProvidesGuObservedFieldExtraction = false;
const bool stringRouteProvidesHiggsScalarSourceOperator = false;
const bool stringRouteProvidesHiggsQuarticOrExcitationSource = false;
const bool stringRouteProvidesTargetIndependentGuHiggsMass = false;
const bool stringRouteProvidesGeVUnitNormalization = false;
const bool stringRoutePromotesWzMasses = false;
const bool stringRoutePromotesHiggsMass = false;
const bool stringRouteCompletesBosonPredictions = false;
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
var lowEnergyRgTransportSourcePromotable = JsonBool(phase236.RootElement, "lowEnergyRgTransportSourcePromotable") is true;

var gaugeHiggsBoundarySourceAuditPassed = JsonBool(phase265.RootElement, "gaugeHiggsBoundarySourceAuditPassed") is true;
var gaugeHiggsBoundaryPromotesHiggsMass = JsonBool(phase265.RootElement, "gaugeHiggsBoundaryPromotesHiggsMass") is true;
var spectralActionBosonSourceAuditPassed = JsonBool(phase268.RootElement, "spectralActionBosonSourceAuditPassed") is true;
var spectralActionPromotesHiggsMass = JsonBool(phase268.RootElement, "spectralActionPromotesHiggsMass") is true;

var electroweakMassMatrixBridgeSourceAuditPassed = JsonBool(phase317.RootElement, "electroweakMassMatrixBridgeSourceAuditPassed") is true;
var smMassGenerationRequiresVev = JsonBool(phase317.RootElement, "smMassGenerationRequiresVev") is true;
var smMassGenerationRequiresWeakCouplingG = JsonBool(phase317.RootElement, "smMassGenerationRequiresWeakCouplingG") is true;
var smMassGenerationRequiresHyperchargeCouplingGPrime = JsonBool(phase317.RootElement, "smMassGenerationRequiresHyperchargeCouplingGPrime") is true;
var smTreeLevelMwDependsOnGAndV = JsonBool(phase317.RootElement, "smTreeLevelMwDependsOnGAndV") is true;
var smTreeLevelMzDependsOnGAndGPrimeAndV = JsonBool(phase317.RootElement, "smTreeLevelMzDependsOnGAndGPrimeAndV") is true;
var smTreeLevelHiggsMassDependsOnPotentialParameter = JsonBool(phase317.RootElement, "smTreeLevelHiggsMassDependsOnPotentialParameter") is true;
var smMassMatrixPromotesWzMasses = JsonBool(phase317.RootElement, "smMassMatrixPromotesWzMasses") is true;
var smMassMatrixPromotesHiggsMass = JsonBool(phase317.RootElement, "smMassMatrixPromotesHiggsMass") is true;

var coupledYangMillsHiggsMassExtractionAuditPassed = JsonBool(phase323.RootElement, "coupledYangMillsHiggsMassExtractionAuditPassed") is true;
var officialPublicSourcesProvideTargetIndependentVevSource = JsonBool(phase323.RootElement, "officialPublicSourcesProvideTargetIndependentVevSource") is true;
var officialPublicSourcesProvideGaugeCouplingNormalization = JsonBool(phase323.RootElement, "officialPublicSourcesProvideGaugeCouplingNormalization") is true;
var officialPublicSourcesProvidePhotonWzHiggsProjectionRows = JsonBool(phase323.RootElement, "officialPublicSourcesProvidePhotonWzHiggsProjectionRows") is true;
var officialPublicSourcesProvideHiggsScalarSelfCouplingSource = JsonBool(phase323.RootElement, "officialPublicSourcesProvideHiggsScalarSelfCouplingSource") is true;
var coupledYangMillsHiggsRouteCompletesBosonPredictions = JsonBool(phase323.RootElement, "coupledYangMillsHiggsRouteCompletesBosonPredictions") is true;

var sourceRows = new[]
{
    new SourceRow(
        "arxiv-1112-1059-string-m-higgs-range",
        stringMTheoryHiggsPredictionUrl,
        "compactified string/M-theory Higgs mass prediction",
        "The paper reports a Standard-Model-like Higgs mass range from compactified string/M-theory under MSSM low-energy content and tan beta assumptions.",
        "This is a serious external Higgs-mass lead, but it is not a GU-local source lineage and does not supply W/Z source rows."),
    new SourceRow(
        "arxiv-1211-2231-g2-m-theory-review",
        stringMTheoryHiggsReviewUrl,
        "G2-holonomy M-theory compactification review",
        "The review summarizes moduli stabilization, gauge coupling unification, radiative electroweak symmetry breaking, and a 126 +/- 2 GeV Higgs claim in a specific compactified M-theory program.",
        "The claim is conditional on the compactified M-theory scenario, not a derivation from GU artifacts."),
    new SourceRow(
        "arxiv-1304-2767-string-higgs-perspective",
        stringTheoreticPerspectiveUrl,
        "string-theoretic Higgs quartic boundary and shift-symmetry analysis",
        "The paper identifies stringy geometries where lambda near zero can arise and states that geometric problems remain to determine precise generic values.",
        "This is a high-scale boundary-condition lead, not a complete W/Z/H prediction source for this repository."),
    new SourceRow(
        "arxiv-2106-04622-closed-string-higgs-mass-framework",
        stringHiggsCalculationUrl,
        "first-principles closed-string one-loop Higgs mass framework",
        "The paper builds a framework for one-loop scalar Higgs masses in perturbative closed string theory and relates Higgs mass to one-loop cosmological constant structure.",
        "It is a general string framework and does not provide the GU-local observed-field and source-row contracts."),
    new SourceRow(
        "doi-10-1016-j-physletb-2005-12-014-heterotic-standard-model",
        heteroticStandardModelUrl,
        "heterotic Calabi-Yau Standard Model construction",
        "The paper constructs heterotic vacua with MSSM-like spectrum and Higgs-pair content using Calabi-Yau geometry, stable bundles, and Wilson lines.",
        "Spectrum engineering is not a W/Z/H absolute mass source law."),
};

var checks = new[]
{
    new Check(
        "string-compactification-primary-sources-reviewed",
        stringCompactificationLeadPresent
            && stringMTheoryPrimarySourcesReviewed
            && stringCompactificationRouteExternalToGu
            && stringRouteGeometricCompactificationBased
            && sourceRows.Length == 5,
        $"lead={stringCompactificationLeadPresent}; primaryReviewed={stringMTheoryPrimarySourcesReviewed}; externalToGu={stringCompactificationRouteExternalToGu}; geometricCompactification={stringRouteGeometricCompactificationBased}; sourceRows={sourceRows.Length}"),
    new Check(
        "string-route-higgs-and-spectrum-leads-recorded",
        stringRouteIncludesG2HolonomyMTheoryLead
            && stringRouteIncludesCalabiYauHeteroticLead
            && stringRouteIncludesFTheoryOrBraneHiggsShiftSymmetryLead
            && stringRouteIncludesFirstPrinciplesClosedStringHiggsMassFramework
            && stringRouteClaimsHiggsMassRangeNearObserved
            && stringRouteCanEngineerMssmSpectrumAndHiggsPairs,
        $"g2MTheory={stringRouteIncludesG2HolonomyMTheoryLead}; calabiYauHeterotic={stringRouteIncludesCalabiYauHeteroticLead}; fTheoryOrBraneShiftSymmetry={stringRouteIncludesFTheoryOrBraneHiggsShiftSymmetryLead}; closedStringFramework={stringRouteIncludesFirstPrinciplesClosedStringHiggsMassFramework}; higgsRangeNearObserved={stringRouteClaimsHiggsMassRangeNearObserved}; mssmSpectrum={stringRouteCanEngineerMssmSpectrumAndHiggsPairs}"),
    new Check(
        "string-route-remains-conditional-external-boundary",
        stringRouteRequiresSpecificCompactificationChoice
            && stringRouteRequiresModuliStabilization
            && stringRouteRequiresSupersymmetryBreakingScenario
            && stringRouteRequiresTanBetaOrHiggsSectorBoundary
            && stringRouteRequiresRgTransport
            && stringRouteRequiresYukawaTopAndGaugeInputs
            && stringRouteOftenPredictsHiggsNotWzAbsoluteRows,
        $"specificCompactification={stringRouteRequiresSpecificCompactificationChoice}; moduliStabilization={stringRouteRequiresModuliStabilization}; susyBreaking={stringRouteRequiresSupersymmetryBreakingScenario}; tanBetaOrBoundary={stringRouteRequiresTanBetaOrHiggsSectorBoundary}; rgTransport={stringRouteRequiresRgTransport}; yukawaTopGaugeInputs={stringRouteRequiresYukawaTopAndGaugeInputs}; oftenHiggsNotWz={stringRouteOftenPredictsHiggsNotWzAbsoluteRows}"),
    new Check(
        "standard-electroweak-dependencies-remain-unsourced",
        electroweakMassMatrixBridgeSourceAuditPassed
            && smMassGenerationRequiresVev
            && smMassGenerationRequiresWeakCouplingG
            && smMassGenerationRequiresHyperchargeCouplingGPrime
            && smTreeLevelMwDependsOnGAndV
            && smTreeLevelMzDependsOnGAndGPrimeAndV
            && smTreeLevelHiggsMassDependsOnPotentialParameter
            && !smMassMatrixPromotesWzMasses
            && !smMassMatrixPromotesHiggsMass
            && electroweakParameterAuditPassed
            && !wAbsoluteMassParameterClosure
            && !zAbsoluteMassParameterClosure
            && !higgsMassParameterClosure
            && !weakCouplingSourcePromotable
            && !lowEnergyRgTransportSourcePromotable,
        $"p317={electroweakMassMatrixBridgeSourceAuditPassed}; requiresV={smMassGenerationRequiresVev}; requiresG={smMassGenerationRequiresWeakCouplingG}; requiresGPrime={smMassGenerationRequiresHyperchargeCouplingGPrime}; mwGv={smTreeLevelMwDependsOnGAndV}; mzGGPrimeV={smTreeLevelMzDependsOnGAndGPrimeAndV}; higgsPotentialParameter={smTreeLevelHiggsMassDependsOnPotentialParameter}; p224={electroweakParameterAuditPassed}; wClosure={wAbsoluteMassParameterClosure}; zClosure={zAbsoluteMassParameterClosure}; higgsClosure={higgsMassParameterClosure}; weakCoupling={weakCouplingSourcePromotable}; rg={lowEnergyRgTransportSourcePromotable}"),
    new Check(
        "string-route-does-not-supply-gu-wz-source-lineage",
        !stringRouteProvidesGuLocalWzTheorem
            && !stringRouteProvidesSeparateWzSourceRows
            && !stringRouteProvidesWzRawAmplitudeGates
            && !stringRouteProvidesWzCommonBridgeGate
            && !stringRouteProvidesTargetIndependentGuVevSource
            && !stringRouteProvidesWeakMixingAngleSource
            && !stringRouteProvidesGuGaugeCouplingNormalization
            && !stringRouteProvidesObservedPhotonWzProjectionRows
            && !stringRouteProvidesNeutralMassMatrixDiagonalization
            && !stringRoutePromotesWzMasses
            && !canFillPhase201WzContract,
        $"guWzTheorem={stringRouteProvidesGuLocalWzTheorem}; separateRows={stringRouteProvidesSeparateWzSourceRows}; rawGates={stringRouteProvidesWzRawAmplitudeGates}; commonGate={stringRouteProvidesWzCommonBridgeGate}; vev={stringRouteProvidesTargetIndependentGuVevSource}; weakAngle={stringRouteProvidesWeakMixingAngleSource}; couplingNorm={stringRouteProvidesGuGaugeCouplingNormalization}; observedRows={stringRouteProvidesObservedPhotonWzProjectionRows}; neutralMatrix={stringRouteProvidesNeutralMassMatrixDiagonalization}; promotesWz={stringRoutePromotesWzMasses}"),
    new Check(
        "string-route-does-not-supply-gu-higgs-source-lineage",
        gaugeHiggsBoundarySourceAuditPassed
            && !gaugeHiggsBoundaryPromotesHiggsMass
            && spectralActionBosonSourceAuditPassed
            && !spectralActionPromotesHiggsMass
            && coupledYangMillsHiggsMassExtractionAuditPassed
            && !officialPublicSourcesProvideTargetIndependentVevSource
            && !officialPublicSourcesProvideGaugeCouplingNormalization
            && !officialPublicSourcesProvidePhotonWzHiggsProjectionRows
            && !officialPublicSourcesProvideHiggsScalarSelfCouplingSource
            && !coupledYangMillsHiggsRouteCompletesBosonPredictions
            && !stringRouteProvidesGuObservedFieldExtraction
            && !stringRouteProvidesHiggsScalarSourceOperator
            && !stringRouteProvidesHiggsQuarticOrExcitationSource
            && !stringRouteProvidesTargetIndependentGuHiggsMass
            && !stringRoutePromotesHiggsMass
            && !canFillPhase201HiggsContract,
        $"p265={gaugeHiggsBoundarySourceAuditPassed}; gaugeHiggsPromotes={gaugeHiggsBoundaryPromotesHiggsMass}; p268={spectralActionBosonSourceAuditPassed}; spectralPromotes={spectralActionPromotesHiggsMass}; p323={coupledYangMillsHiggsMassExtractionAuditPassed}; p323Vev={officialPublicSourcesProvideTargetIndependentVevSource}; p323Coupling={officialPublicSourcesProvideGaugeCouplingNormalization}; p323Projection={officialPublicSourcesProvidePhotonWzHiggsProjectionRows}; p323SelfCoupling={officialPublicSourcesProvideHiggsScalarSelfCouplingSource}; observedExtraction={stringRouteProvidesGuObservedFieldExtraction}; higgsOperator={stringRouteProvidesHiggsScalarSourceOperator}; higgsQuartic={stringRouteProvidesHiggsQuarticOrExcitationSource}; higgsMass={stringRouteProvidesTargetIndependentGuHiggsMass}; promotesHiggs={stringRoutePromotesHiggsMass}"),
    new Check(
        "source-lineage-contracts-remain-unfilled-after-string-audit",
        !phase201AllRequiredLineagesPromotable
            && !existingEvidenceFound
            && wzMissingFieldCount == 15
            && higgsMissingFieldCount == 14
            && observedFieldExtractionRequiredFieldCount == 20
            && observedFieldExtractionFilledRequiredFieldCount == 0
            && !observedFieldExtractionContractPromotable
            && !stringRouteProvidesGeVUnitNormalization
            && !stringRouteCompletesBosonPredictions
            && !canFillPhase201WzContract
            && !canFillPhase201HiggsContract
            && !canFillPhase256ObservedFieldExtractionContract,
        $"phase201Promotable={phase201AllRequiredLineagesPromotable}; existingEvidence={existingEvidenceFound}; wzMissing={wzMissingFieldCount}; higgsMissing={higgsMissingFieldCount}; phase256Required={observedFieldExtractionRequiredFieldCount}; phase256Filled={observedFieldExtractionFilledRequiredFieldCount}; observedPromotable={observedFieldExtractionContractPromotable}; geV={stringRouteProvidesGeVUnitNormalization}; completes={stringRouteCompletesBosonPredictions}; canFillWz={canFillPhase201WzContract}; canFillHiggs={canFillPhase201HiggsContract}; canFillObserved={canFillPhase256ObservedFieldExtractionContract}"),
};

var stringMTheoryCompactificationSourceAuditPassed = checks.All(check => check.Passed)
    && stringCompactificationLeadPresent
    && stringMTheoryPrimarySourcesReviewed
    && stringCompactificationRouteExternalToGu
    && stringRouteGeometricCompactificationBased
    && !stringRouteProvidesGuLocalWzTheorem
    && !stringRouteProvidesSeparateWzSourceRows
    && !stringRouteProvidesTargetIndependentGuVevSource
    && !stringRouteProvidesWeakMixingAngleSource
    && !stringRouteProvidesGuGaugeCouplingNormalization
    && !stringRouteProvidesGuObservedFieldExtraction
    && !stringRouteProvidesHiggsScalarSourceOperator
    && !stringRouteProvidesHiggsQuarticOrExcitationSource
    && !stringRouteProvidesTargetIndependentGuHiggsMass
    && !stringRouteProvidesGeVUnitNormalization
    && !stringRoutePromotesWzMasses
    && !stringRoutePromotesHiggsMass
    && !stringRouteCompletesBosonPredictions
    && !canFillPhase201WzContract
    && !canFillPhase201HiggsContract
    && !canFillPhase256ObservedFieldExtractionContract;

var terminalStatus = stringMTheoryCompactificationSourceAuditPassed
    ? "string-m-theory-compactification-source-audit-external-boundary-not-gu-source"
    : "string-m-theory-compactification-source-audit-review-required";

var result = new
{
    phaseId = "phase332-string-m-theory-compactification-source-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    stringMTheoryCompactificationSourceAuditPassed,
    stringCompactificationLeadPresent,
    stringMTheoryPrimarySourcesReviewed,
    stringCompactificationRouteExternalToGu,
    stringRouteGeometricCompactificationBased,
    stringRouteIncludesG2HolonomyMTheoryLead,
    stringRouteIncludesCalabiYauHeteroticLead,
    stringRouteIncludesFTheoryOrBraneHiggsShiftSymmetryLead,
    stringRouteIncludesFirstPrinciplesClosedStringHiggsMassFramework,
    stringRouteClaimsHiggsMassRangeNearObserved,
    stringRouteClaimsNoFreeParametersInSpecificMTheoryReview,
    stringRouteCanEngineerMssmSpectrumAndHiggsPairs,
    stringRouteRequiresSpecificCompactificationChoice,
    stringRouteRequiresModuliStabilization,
    stringRouteRequiresSupersymmetryBreakingScenario,
    stringRouteRequiresTanBetaOrHiggsSectorBoundary,
    stringRouteRequiresRgTransport,
    stringRouteRequiresYukawaTopAndGaugeInputs,
    stringRouteOftenPredictsHiggsNotWzAbsoluteRows,
    stringRouteProvidesGuLocalWzTheorem,
    stringRouteProvidesSeparateWzSourceRows,
    stringRouteProvidesWzRawAmplitudeGates,
    stringRouteProvidesWzCommonBridgeGate,
    stringRouteProvidesTargetIndependentGuVevSource,
    stringRouteProvidesWeakMixingAngleSource,
    stringRouteProvidesGuGaugeCouplingNormalization,
    stringRouteProvidesObservedPhotonWzProjectionRows,
    stringRouteProvidesNeutralMassMatrixDiagonalization,
    stringRouteProvidesGuObservedFieldExtraction,
    stringRouteProvidesHiggsScalarSourceOperator,
    stringRouteProvidesHiggsQuarticOrExcitationSource,
    stringRouteProvidesTargetIndependentGuHiggsMass,
    stringRouteProvidesGeVUnitNormalization,
    stringRoutePromotesWzMasses,
    stringRoutePromotesHiggsMass,
    stringRouteCompletesBosonPredictions,
    canFillPhase201WzContract,
    canFillPhase201HiggsContract,
    canFillPhase256ObservedFieldExtractionContract,
    sourceRows,
    checks,
    currentBlockerEvidence = new
    {
        phase201AllRequiredLineagesPromotable,
        existingEvidenceFound,
        wzMissingFieldCount,
        higgsMissingFieldCount,
        observedFieldExtractionRequiredFieldCount,
        observedFieldExtractionFilledRequiredFieldCount,
        observedFieldExtractionContractPromotable,
        electroweakParameterAuditPassed,
        wAbsoluteMassParameterClosure,
        zAbsoluteMassParameterClosure,
        higgsMassParameterClosure,
        weakCouplingSourcePromotable,
        lowEnergyRgTransportSourcePromotable,
    },
    contractImpact = new
    {
        canFillPhase201WzContract,
        canFillPhase201HiggsContract,
        canFillPhase256ObservedFieldExtractionContract,
        wzMissingFieldCount,
        higgsMissingFieldCount,
        observedFieldExtractionFilledRequiredFieldCount,
    },
    decision = stringMTheoryCompactificationSourceAuditPassed
        ? "Do not promote W/Z or Higgs physical masses from string/M-theory compactification literature in this repository. The route is a serious external geometric high-scale boundary and spectrum-engineering lead, including conditional Higgs-mass claims, but it does not supply GU-local W/Z source rows, target-independent GU VEV or weak-angle lineage, GU gauge-coupling normalization, observed photon/W/Z/H extraction, a GU Higgs scalar-source/self-coupling lineage, or GeV unit normalization."
        : "Review the string/M-theory compactification route before relying on this boundary.",
    nextRequiredArtifact = new[]
    {
        "A GU-local compactification or equivalent source theorem deriving observed SU(2)_L x U(1)_Y embedding and separate W/Z source rows.",
        "A target-independent GU VEV, weak-mixing angle, gauge-coupling normalization, and observed-field extraction map.",
        "A solved GU Higgs scalar source/operator with self-coupling or excitation lineage and stability sidecars.",
        "A physical-unit normalization and low-energy transport theorem that does not import target values.",
    },
    sourceEvidence = new
    {
        stringMTheoryHiggsPredictionUrl,
        stringMTheoryHiggsReviewUrl,
        stringTheoreticPerspectiveUrl,
        stringHiggsCalculationUrl,
        heteroticStandardModelUrl,
        phase201Path = Phase201Path,
        phase213Path = Phase213Path,
        phase224Path = Phase224Path,
        phase236Path = Phase236Path,
        phase256Path = Phase256Path,
        phase265Path = Phase265Path,
        phase268Path = Phase268Path,
        phase317Path = Phase317Path,
        phase323Path = Phase323Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true };
File.WriteAllText(Path.Combine(outputDir, "string_m_theory_compactification_source_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "string_m_theory_compactification_source_audit_summary.json"),
    JsonSerializer.Serialize(
        new
        {
            result.phaseId,
            result.terminalStatus,
            result.stringMTheoryCompactificationSourceAuditPassed,
            result.stringCompactificationLeadPresent,
            result.stringMTheoryPrimarySourcesReviewed,
            result.stringCompactificationRouteExternalToGu,
            result.stringRouteGeometricCompactificationBased,
            result.stringRouteIncludesG2HolonomyMTheoryLead,
            result.stringRouteIncludesCalabiYauHeteroticLead,
            result.stringRouteIncludesFTheoryOrBraneHiggsShiftSymmetryLead,
            result.stringRouteIncludesFirstPrinciplesClosedStringHiggsMassFramework,
            result.stringRouteClaimsHiggsMassRangeNearObserved,
            result.stringRouteClaimsNoFreeParametersInSpecificMTheoryReview,
            result.stringRouteRequiresSpecificCompactificationChoice,
            result.stringRouteRequiresModuliStabilization,
            result.stringRouteRequiresSupersymmetryBreakingScenario,
            result.stringRouteRequiresTanBetaOrHiggsSectorBoundary,
            result.stringRouteRequiresRgTransport,
            result.stringRouteRequiresYukawaTopAndGaugeInputs,
            result.stringRouteProvidesGuLocalWzTheorem,
            result.stringRouteProvidesSeparateWzSourceRows,
            result.stringRouteProvidesTargetIndependentGuVevSource,
            result.stringRouteProvidesWeakMixingAngleSource,
            result.stringRouteProvidesGuGaugeCouplingNormalization,
            result.stringRouteProvidesGuObservedFieldExtraction,
            result.stringRouteProvidesHiggsScalarSourceOperator,
            result.stringRouteProvidesHiggsQuarticOrExcitationSource,
            result.stringRouteProvidesTargetIndependentGuHiggsMass,
            result.stringRouteProvidesGeVUnitNormalization,
            result.stringRoutePromotesWzMasses,
            result.stringRoutePromotesHiggsMass,
            result.stringRouteCompletesBosonPredictions,
            result.canFillPhase201WzContract,
            result.canFillPhase201HiggsContract,
            result.canFillPhase256ObservedFieldExtractionContract,
            sourceRowCount = result.sourceRows.Length,
            result.contractImpact,
            result.decision,
            result.nextRequiredArtifact,
        },
        options));

Console.WriteLine($"stringMTheoryCompactificationSourceAuditPassed={stringMTheoryCompactificationSourceAuditPassed}");
Console.WriteLine($"stringCompactificationLeadPresent={stringCompactificationLeadPresent}");
Console.WriteLine($"stringRoutePromotesWzMasses={stringRoutePromotesWzMasses}");
Console.WriteLine($"stringRoutePromotesHiggsMass={stringRoutePromotesHiggsMass}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");
Console.WriteLine($"canFillPhase201HiggsContract={canFillPhase201HiggsContract}");

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind is JsonValueKind.True or JsonValueKind.False
        ? value.GetBoolean()
        : null;

static int? JsonInt(JsonElement element, string propertyName)
{
    if (!element.TryGetProperty(propertyName, out var value))
    {
        return null;
    }

    return value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var intValue)
        ? intValue
        : null;
}

sealed record SourceRow(string SourceId, string Url, string Scope, string Finding, string PredictionImpact);
sealed record Check(string CheckId, bool Passed, string Detail);
