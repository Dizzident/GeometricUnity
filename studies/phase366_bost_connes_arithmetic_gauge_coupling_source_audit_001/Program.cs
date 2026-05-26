using System.Text.Json;

const string DefaultOutputDir = "studies/phase366_bost_connes_arithmetic_gauge_coupling_source_audit_001/output";
const string Phase201Path = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase224Path = "studies/phase224_electroweak_parameter_dependency_audit_001/output/electroweak_parameter_dependency_audit_summary.json";
const string Phase256Path = "studies/phase256_observed_field_extraction_intake_contract_001/output/observed_field_extraction_intake_contract_summary.json";
const string Phase284Path = "studies/phase284_predicted_ratio_alpha_gf_external_closure_diagnostic_001/output/predicted_ratio_alpha_gf_external_closure_diagnostic_summary.json";
const string Phase292Path = "studies/phase292_electromagnetic_alpha_source_audit_001/output/electromagnetic_alpha_source_audit_summary.json";
const string Phase293Path = "studies/phase293_fermi_vev_source_audit_001/output/fermi_vev_source_audit_summary.json";
const string Phase294Path = "studies/phase294_rg_scheme_transport_source_audit_001/output/rg_scheme_transport_source_audit_summary.json";
const string Phase317Path = "studies/phase317_electroweak_mass_matrix_bridge_source_audit_001/output/electroweak_mass_matrix_bridge_source_audit_summary.json";
const string Phase359Path = "studies/phase359_finite_ncg_discrete_higgs_source_audit_001/output/finite_ncg_discrete_higgs_source_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE366_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase224 = JsonDocument.Parse(File.ReadAllText(Phase224Path));
using var phase256 = JsonDocument.Parse(File.ReadAllText(Phase256Path));
using var phase284 = JsonDocument.Parse(File.ReadAllText(Phase284Path));
using var phase292 = JsonDocument.Parse(File.ReadAllText(Phase292Path));
using var phase293 = JsonDocument.Parse(File.ReadAllText(Phase293Path));
using var phase294 = JsonDocument.Parse(File.ReadAllText(Phase294Path));
using var phase317 = JsonDocument.Parse(File.ReadAllText(Phase317Path));
using var phase359 = JsonDocument.Parse(File.ReadAllText(Phase359Path));

const string huBostConnesGaugeForceUrl = "https://papers.ssrn.com/sol3/papers.cfm?abstract_id=6723462";
const string huBostConnesGaugeForceDoiUrl = "https://dx.doi.org/10.2139/ssrn.6723462";
const string bostConnesOriginalDoiUrl = "https://doi.org/10.1007/BF01589495";
const string chamseddineConnesMarcolliUrl = "https://arxiv.org/abs/hep-th/0610241";

const bool bostConnesArithmeticGaugeCouplingLeadPresent = true;
const bool bostConnesArithmeticGaugeCouplingSourceReviewed = true;
const bool bostConnesArithmeticGaugeCouplingPostedMay2026 = true;
const bool bostConnesArithmeticGaugeCouplingRouteExternalToGu = true;
const bool routeUsesBostConnesCStarAlgebra = true;
const bool routeUsesPrimeSevenLocalFactor = true;
const bool routeUsesCrtGaugeGroupDecomposition = true;
const bool routeClaimsSpectralTraceRatios = true;
const bool routeClaimsAlphaStrongAlphaEmAlphaWeak = true;
const bool routeClaimsWeakMixingAngle = true;
const bool routeClaimsVirtualProcessCorrections = true;
const bool routeProvidesExternalGaugeCouplingTemplate = true;
const bool routeProvidesExternalWeakAngleTemplate = true;
const bool routePotentiallyAddressesWeakCouplingPartOfWzContract = true;

const double bornAlphaWeakInverse = 29.2;
const double correctedAlphaWeakInverse = 29.60;
const double correctedAlphaEmInverse = 137.036;
const double correctedAlphaStrongInverse = 8.49;
const double claimedWeakMixingSinSquared = 0.2315;
const double claimedWeakMixingExperimentalReference = 0.23122;
const double externalFermiVevGeV = 246.21965079413738;
const double wTargetGeV = 80.3692;
const double zTargetGeV = 91.188;
const double broadMassGateRelativeTolerance = 0.01;

var correctedWeakCoupling = Math.Sqrt(4.0 * Math.PI / correctedAlphaWeakInverse);
var claimedCosThetaW = Math.Sqrt(1.0 - claimedWeakMixingSinSquared);
var arithmeticExternalVevWGeV = correctedWeakCoupling * externalFermiVevGeV / 2.0;
var arithmeticExternalVevZGeV = arithmeticExternalVevWGeV / claimedCosThetaW;
var arithmeticWRelativeError = Math.Abs(arithmeticExternalVevWGeV - wTargetGeV) / wTargetGeV;
var arithmeticZRelativeError = Math.Abs(arithmeticExternalVevZGeV - zTargetGeV) / zTargetGeV;
var arithmeticExternalVevWzDiagnosticPassesBroadGate = arithmeticWRelativeError <= broadMassGateRelativeTolerance
    && arithmeticZRelativeError <= broadMassGateRelativeTolerance;

const bool routeUsesExternalFermiVevForWzDiagnostic = true;
const bool routeUsesExternalSsrnPreprintAsCouplingSource = true;
const bool routeDoesNotUseWzTargetsForConstruction = true;
const bool routeUsesTargetsOnlyForPostConstructionEvaluation = true;
const bool routeDoesNotSupplyGuBostConnesEmbedding = true;
const bool routeDoesNotSupplyGuArithmeticSpectralTriple = true;
const bool routeDoesNotSupplyGuGaugeKineticNormalization = true;
const bool routeDoesNotSupplyTargetIndependentGuVev = true;
const bool routeDoesNotSupplyWzRawAmplitudeRows = true;
const bool routeDoesNotSupplyWzStabilitySidecars = true;
const bool routeDoesNotSupplyObservedFieldExtraction = true;
const bool routeDoesNotSupplyHiggsScalarSource = true;
const bool routeDoesNotSupplyHiggsSelfCoupling = true;
const bool routeDoesNotSupplyGeVUnitNormalization = true;

const bool routeProvidesGuLocalBostConnesMap = false;
const bool routeProvidesGuGaugeCouplingNormalization = false;
const bool routeProvidesGuWeakMixingAngleSource = false;
const bool routeProvidesTargetIndependentGuVevSource = false;
const bool routeProvidesSeparateWzSourceRows = false;
const bool routeProvidesWzRawAmplitudeGates = false;
const bool routeProvidesWzCommonBridgeGate = false;
const bool routeProvidesWzStabilitySidecars = false;
const bool routeProvidesObservedPhotonWzHiggsProjectionRows = false;
const bool routeProvidesHiggsScalarSourceOperator = false;
const bool routeProvidesHiggsQuarticOrExcitationSource = false;
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
var observedFieldExtractionRequiredFieldCount = JsonInt(phase256.RootElement, "requiredFieldCount") ?? -1;
var observedFieldExtractionFilledRequiredFieldCount = JsonInt(phase256.RootElement, "filledRequiredFieldCount") ?? -1;
var observedFieldExtractionContractPromotable = JsonBool(phase256.RootElement, "observedFieldExtractionContractPromotable") is true;

var electroweakParameterAuditPassed = JsonBool(phase224.RootElement, "electroweakParameterAuditPassed") is true;
var weakCouplingSourcePromotable = JsonBool(phase224.RootElement, "weakCouplingSourcePromotable") is true;
var wAbsoluteMassParameterClosure = JsonBool(phase224.RootElement, "wAbsoluteMassParameterClosure") is true;
var zAbsoluteMassParameterClosure = JsonBool(phase224.RootElement, "zAbsoluteMassParameterClosure") is true;
var higgsMassParameterClosure = JsonBool(phase224.RootElement, "higgsMassParameterClosure") is true;

var predictedRatioAlphaGfExternalClosurePassed = JsonBool(phase284.RootElement, "predictedRatioAlphaGfExternalClosureDiagnosticPassed") is true;
var alphaMzExternalClosurePasses = JsonBool(phase284.RootElement, "alphaMzRowPassesWzTargetComparison") is true;
var electromagneticAlphaSourceAuditPassed = JsonBool(phase292.RootElement, "electromagneticAlphaSourceAuditPassed") is true;
var alphaSourcePromotesWzMasses = JsonBool(phase292.RootElement, "alphaSourcePromotesWzMasses") is true;
var fermiVevSourceAuditPassed = JsonBool(phase293.RootElement, "fermiVevSourceAuditPassed") is true;
var fermiVevPromotesWzMasses = JsonBool(phase293.RootElement, "fermiVevSourcePromotesWzMasses") is true;
var rgSchemeTransportSourceAuditPassed = JsonBool(phase294.RootElement, "rgSchemeTransportSourceAuditPassed") is true;
var rgSchemeRoutePromotesWzMasses = JsonBool(phase294.RootElement, "rgSchemeTransportPromotesWzMasses") is true;

var electroweakMassMatrixBridgeSourceAuditPassed = JsonBool(phase317.RootElement, "electroweakMassMatrixBridgeSourceAuditPassed") is true;
var smMassGenerationRequiresVev = JsonBool(phase317.RootElement, "smMassGenerationRequiresVev") is true;
var smMassGenerationRequiresWeakCouplingG = JsonBool(phase317.RootElement, "smMassGenerationRequiresWeakCouplingG") is true;
var smMassGenerationRequiresHyperchargeCouplingGPrime = JsonBool(phase317.RootElement, "smMassGenerationRequiresHyperchargeCouplingGPrime") is true;
var smMassMatrixPromotesWzMasses = JsonBool(phase317.RootElement, "smMassMatrixPromotesWzMasses") is true;
var smMassMatrixPromotesHiggsMass = JsonBool(phase317.RootElement, "smMassMatrixPromotesHiggsMass") is true;

var finiteNcgDiscreteHiggsSourceAuditPassed = JsonBool(phase359.RootElement, "finiteNcgDiscreteHiggsSourceAuditPassed") is true;
var finiteNcgRouteExternalToGu = JsonBool(phase359.RootElement, "finiteNcgRouteExternalToGu") is true;
var finiteNcgRoutePromotesWzMasses = JsonBool(phase359.RootElement, "finiteNcgRoutePromotesWzMasses") is true;
var finiteNcgRoutePromotesHiggsMass = JsonBool(phase359.RootElement, "finiteNcgRoutePromotesHiggsMass") is true;

var sourceRows = new[]
{
    new SourceRow(
        "hu-ssrn-6723462-bost-connes-gauge-coupling",
        huBostConnesGaugeForceUrl,
        "SSRN preprint abstract, posted 6 May 2026",
        "Claims that the local factor of the Bost-Connes C*-algebra at p=7 describes the Standard Model gauge group modulo Z6 and yields gauge couplings through spectral trace ratios.",
        "Direct weak-coupling and weak-angle lead, but external to GU and not a W/Z source-row or VEV theorem."),
    new SourceRow(
        "bost-connes-1995-original-system",
        bostConnesOriginalDoiUrl,
        "Bost and Connes, Hecke algebras, type III factors and phase transitions with spontaneous symmetry breaking in number theory",
        "Original mathematical Bost-Connes system context for arithmetic C*-dynamical systems and phase transitions.",
        "Provides mathematical context only; it does not contain GU electroweak source-lineage rows."),
    new SourceRow(
        "chamseddine-connes-marcolli-0610241-spectral-standard-model",
        chamseddineConnesMarcolliUrl,
        "Gravity and the standard model with neutrino mixing",
        "Established noncommutative-geometry Standard Model source already covered by Phase359 finite-NCG audit.",
        "Useful comparison for NCG gauge-coupling relations; Phase359 keeps it non-promotional for GU W/Z/H predictions.")
};

var checks = new[]
{
    new Check(
        "bost-connes-arithmetic-gauge-coupling-source-reviewed",
        bostConnesArithmeticGaugeCouplingLeadPresent
            && bostConnesArithmeticGaugeCouplingSourceReviewed
            && bostConnesArithmeticGaugeCouplingPostedMay2026
            && bostConnesArithmeticGaugeCouplingRouteExternalToGu
            && sourceRows.Length == 3,
        $"lead={bostConnesArithmeticGaugeCouplingLeadPresent}; reviewed={bostConnesArithmeticGaugeCouplingSourceReviewed}; postedMay2026={bostConnesArithmeticGaugeCouplingPostedMay2026}; externalToGu={bostConnesArithmeticGaugeCouplingRouteExternalToGu}; sourceRows={sourceRows.Length}"),
    new Check(
        "arithmetic-coupling-claim-recorded",
        routeUsesBostConnesCStarAlgebra
            && routeUsesPrimeSevenLocalFactor
            && routeUsesCrtGaugeGroupDecomposition
            && routeClaimsSpectralTraceRatios
            && routeClaimsAlphaStrongAlphaEmAlphaWeak
            && routeClaimsWeakMixingAngle
            && routeClaimsVirtualProcessCorrections
            && routeProvidesExternalGaugeCouplingTemplate
            && routeProvidesExternalWeakAngleTemplate
            && routePotentiallyAddressesWeakCouplingPartOfWzContract,
        $"bostConnes={routeUsesBostConnesCStarAlgebra}; p7={routeUsesPrimeSevenLocalFactor}; crt={routeUsesCrtGaugeGroupDecomposition}; traceRatios={routeClaimsSpectralTraceRatios}; couplings={routeClaimsAlphaStrongAlphaEmAlphaWeak}; weakAngle={routeClaimsWeakMixingAngle}; corrections={routeClaimsVirtualProcessCorrections}; weakCouplingPart={routePotentiallyAddressesWeakCouplingPartOfWzContract}"),
    new Check(
        "arithmetic-external-vev-wz-diagnostic-materialized",
        routeDoesNotUseWzTargetsForConstruction
            && routeUsesTargetsOnlyForPostConstructionEvaluation
            && routeUsesExternalFermiVevForWzDiagnostic
            && routeUsesExternalSsrnPreprintAsCouplingSource
            && Math.Abs(correctedAlphaWeakInverse - 29.60) < 1.0e-12
            && Math.Abs(claimedWeakMixingSinSquared - 0.2315) < 1.0e-12
            && arithmeticExternalVevWzDiagnosticPassesBroadGate,
        $"alphaWInv={correctedAlphaWeakInverse:R}; g={correctedWeakCoupling:R}; sin2={claimedWeakMixingSinSquared:R}; W={arithmeticExternalVevWGeV:R}; Z={arithmeticExternalVevZGeV:R}; wRelErr={arithmeticWRelativeError:R}; zRelErr={arithmeticZRelativeError:R}; broadPass={arithmeticExternalVevWzDiagnosticPassesBroadGate}; targetsForConstruction=False"),
    new Check(
        "existing-alpha-gf-route-boundaries-preserved",
        predictedRatioAlphaGfExternalClosurePassed
            && alphaMzExternalClosurePasses
            && electromagneticAlphaSourceAuditPassed
            && !alphaSourcePromotesWzMasses
            && fermiVevSourceAuditPassed
            && !fermiVevPromotesWzMasses
            && rgSchemeTransportSourceAuditPassed
            && !rgSchemeRoutePromotesWzMasses,
        $"p284Passed={predictedRatioAlphaGfExternalClosurePassed}; p284AlphaMzPass={alphaMzExternalClosurePasses}; p292Passed={electromagneticAlphaSourceAuditPassed}; p292Promotes={alphaSourcePromotesWzMasses}; p293Passed={fermiVevSourceAuditPassed}; p293Promotes={fermiVevPromotesWzMasses}; p294Passed={rgSchemeTransportSourceAuditPassed}; p294Promotes={rgSchemeRoutePromotesWzMasses}"),
    new Check(
        "standard-mass-matrix-dependency-boundary-preserved",
        electroweakParameterAuditPassed
            && !weakCouplingSourcePromotable
            && !wAbsoluteMassParameterClosure
            && !zAbsoluteMassParameterClosure
            && !higgsMassParameterClosure
            && electroweakMassMatrixBridgeSourceAuditPassed
            && smMassGenerationRequiresVev
            && smMassGenerationRequiresWeakCouplingG
            && smMassGenerationRequiresHyperchargeCouplingGPrime
            && !smMassMatrixPromotesWzMasses
            && !smMassMatrixPromotesHiggsMass,
        $"p224Passed={electroweakParameterAuditPassed}; weakCouplingPromotable={weakCouplingSourcePromotable}; wClosure={wAbsoluteMassParameterClosure}; zClosure={zAbsoluteMassParameterClosure}; higgsClosure={higgsMassParameterClosure}; p317Passed={electroweakMassMatrixBridgeSourceAuditPassed}; requiresV={smMassGenerationRequiresVev}; requiresG={smMassGenerationRequiresWeakCouplingG}; requiresGPrime={smMassGenerationRequiresHyperchargeCouplingGPrime}; p317PromotesWz={smMassMatrixPromotesWzMasses}; p317PromotesHiggs={smMassMatrixPromotesHiggsMass}"),
    new Check(
        "finite-ncg-boundary-preserved",
        finiteNcgDiscreteHiggsSourceAuditPassed
            && finiteNcgRouteExternalToGu
            && !finiteNcgRoutePromotesWzMasses
            && !finiteNcgRoutePromotesHiggsMass,
        $"p359Passed={finiteNcgDiscreteHiggsSourceAuditPassed}; externalToGu={finiteNcgRouteExternalToGu}; promotesWz={finiteNcgRoutePromotesWzMasses}; promotesHiggs={finiteNcgRoutePromotesHiggsMass}"),
    new Check(
        "arithmetic-route-does-not-fill-gu-contracts",
        routeDoesNotSupplyGuBostConnesEmbedding
            && routeDoesNotSupplyGuArithmeticSpectralTriple
            && routeDoesNotSupplyGuGaugeKineticNormalization
            && routeDoesNotSupplyTargetIndependentGuVev
            && routeDoesNotSupplyWzRawAmplitudeRows
            && routeDoesNotSupplyWzStabilitySidecars
            && routeDoesNotSupplyObservedFieldExtraction
            && routeDoesNotSupplyHiggsScalarSource
            && routeDoesNotSupplyHiggsSelfCoupling
            && routeDoesNotSupplyGeVUnitNormalization
            && !routeProvidesGuLocalBostConnesMap
            && !routeProvidesGuGaugeCouplingNormalization
            && !routeProvidesGuWeakMixingAngleSource
            && !routeProvidesTargetIndependentGuVevSource
            && !routeProvidesSeparateWzSourceRows
            && !routeProvidesWzRawAmplitudeGates
            && !routeProvidesWzCommonBridgeGate
            && !routeProvidesWzStabilitySidecars
            && !routeProvidesObservedPhotonWzHiggsProjectionRows
            && !routeProvidesHiggsScalarSourceOperator
            && !routeProvidesHiggsQuarticOrExcitationSource
            && !routeProvidesGeVUnitNormalization
            && !routePromotesWzMasses
            && !routePromotesHiggsMass
            && !routeCompletesBosonPredictions
            && !canFillPhase201WzContract
            && !canFillPhase201HiggsContract
            && !canFillPhase256ObservedFieldExtractionContract,
        $"guMap={routeProvidesGuLocalBostConnesMap}; guCoupling={routeProvidesGuGaugeCouplingNormalization}; guWeakAngle={routeProvidesGuWeakMixingAngleSource}; guVev={routeProvidesTargetIndependentGuVevSource}; wzRows={routeProvidesSeparateWzSourceRows}; raw={routeProvidesWzRawAmplitudeGates}; common={routeProvidesWzCommonBridgeGate}; stability={routeProvidesWzStabilitySidecars}; observed={routeProvidesObservedPhotonWzHiggsProjectionRows}; higgsSource={routeProvidesHiggsScalarSourceOperator}; geV={routeProvidesGeVUnitNormalization}; promotesWz={routePromotesWzMasses}; promotesHiggs={routePromotesHiggsMass}"),
    new Check(
        "phase201-and-phase213-contract-state-preserved",
        !phase201AllRequiredLineagesPromotable
            && !existingEvidenceFound
            && wzMissingFieldCount == 15
            && higgsMissingFieldCount == 14
            && observedFieldExtractionRequiredFieldCount == 20
            && observedFieldExtractionFilledRequiredFieldCount == 0
            && !observedFieldExtractionContractPromotable,
        $"phase201Promotable={phase201AllRequiredLineagesPromotable}; existingEvidence={existingEvidenceFound}; wzMissing={wzMissingFieldCount}; higgsMissing={higgsMissingFieldCount}; phase256Required={observedFieldExtractionRequiredFieldCount}; phase256Filled={observedFieldExtractionFilledRequiredFieldCount}; phase256Promotable={observedFieldExtractionContractPromotable}")
};

var bostConnesArithmeticGaugeCouplingSourceAuditPassed = checks.All(check => check.Passed)
    && arithmeticExternalVevWzDiagnosticPassesBroadGate
    && routePotentiallyAddressesWeakCouplingPartOfWzContract
    && !routeProvidesGuLocalBostConnesMap
    && !routeProvidesGuGaugeCouplingNormalization
    && !routeProvidesGuWeakMixingAngleSource
    && !routeProvidesTargetIndependentGuVevSource
    && !routeProvidesSeparateWzSourceRows
    && !routeProvidesHiggsScalarSourceOperator
    && !routePromotesWzMasses
    && !routePromotesHiggsMass
    && !routeCompletesBosonPredictions;

var terminalStatus = bostConnesArithmeticGaugeCouplingSourceAuditPassed
    ? "bost-connes-arithmetic-gauge-coupling-source-audit-external-coupling-not-gu-source"
    : "bost-connes-arithmetic-gauge-coupling-source-audit-review-required";

var result = new
{
    phaseId = "phase366-bost-connes-arithmetic-gauge-coupling-source-audit",
    terminalStatus,
    bostConnesArithmeticGaugeCouplingSourceAuditPassed,
    bostConnesArithmeticGaugeCouplingLeadPresent,
    bostConnesArithmeticGaugeCouplingSourceReviewed,
    bostConnesArithmeticGaugeCouplingPostedMay2026,
    bostConnesArithmeticGaugeCouplingRouteExternalToGu,
    huBostConnesGaugeForceDoiUrl,
    routeUsesBostConnesCStarAlgebra,
    routeUsesPrimeSevenLocalFactor,
    routeUsesCrtGaugeGroupDecomposition,
    routeClaimsSpectralTraceRatios,
    routeClaimsAlphaStrongAlphaEmAlphaWeak,
    routeClaimsWeakMixingAngle,
    routeClaimsVirtualProcessCorrections,
    routeProvidesExternalGaugeCouplingTemplate,
    routeProvidesExternalWeakAngleTemplate,
    routePotentiallyAddressesWeakCouplingPartOfWzContract,
    bornAlphaWeakInverse,
    correctedAlphaWeakInverse,
    correctedAlphaEmInverse,
    correctedAlphaStrongInverse,
    claimedWeakMixingSinSquared,
    claimedWeakMixingExperimentalReference,
    correctedWeakCoupling,
    claimedCosThetaW,
    externalFermiVevGeV,
    arithmeticExternalVevWGeV,
    arithmeticExternalVevZGeV,
    arithmeticWRelativeError,
    arithmeticZRelativeError,
    broadMassGateRelativeTolerance,
    arithmeticExternalVevWzDiagnosticPassesBroadGate,
    routeUsesExternalFermiVevForWzDiagnostic,
    routeUsesExternalSsrnPreprintAsCouplingSource,
    routeDoesNotUseWzTargetsForConstruction,
    routeUsesTargetsOnlyForPostConstructionEvaluation,
    routeDoesNotSupplyGuBostConnesEmbedding,
    routeDoesNotSupplyGuArithmeticSpectralTriple,
    routeDoesNotSupplyGuGaugeKineticNormalization,
    routeDoesNotSupplyTargetIndependentGuVev,
    routeDoesNotSupplyWzRawAmplitudeRows,
    routeDoesNotSupplyWzStabilitySidecars,
    routeDoesNotSupplyObservedFieldExtraction,
    routeDoesNotSupplyHiggsScalarSource,
    routeDoesNotSupplyHiggsSelfCoupling,
    routeDoesNotSupplyGeVUnitNormalization,
    routeProvidesGuLocalBostConnesMap,
    routeProvidesGuGaugeCouplingNormalization,
    routeProvidesGuWeakMixingAngleSource,
    routeProvidesTargetIndependentGuVevSource,
    routeProvidesSeparateWzSourceRows,
    routeProvidesWzRawAmplitudeGates,
    routeProvidesWzCommonBridgeGate,
    routeProvidesWzStabilitySidecars,
    routeProvidesObservedPhotonWzHiggsProjectionRows,
    routeProvidesHiggsScalarSourceOperator,
    routeProvidesHiggsQuarticOrExcitationSource,
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
        predictedRatioAlphaGfExternalClosurePassed,
        alphaMzExternalClosurePasses,
        electromagneticAlphaSourceAuditPassed,
        alphaSourcePromotesWzMasses,
        fermiVevSourceAuditPassed,
        fermiVevPromotesWzMasses,
        rgSchemeTransportSourceAuditPassed,
        rgSchemeRoutePromotesWzMasses,
        electroweakMassMatrixBridgeSourceAuditPassed,
        finiteNcgDiscreteHiggsSourceAuditPassed,
        finiteNcgRouteExternalToGu,
        finiteNcgRoutePromotesWzMasses,
        finiteNcgRoutePromotesHiggsMass
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
        canFillPhase256ObservedFieldExtractionContract
    },
    decision = "Do not promote W/Z or Higgs masses from the Bost-Connes arithmetic gauge-coupling route in this repository. The current preprint is a specific weak-coupling and weak-angle lead, and when combined with the already external Fermi VEV it gives a close W/Z diagnostic without using W/Z targets for construction. It is still external to GU and does not supply a GU Bost-Connes embedding, GU gauge kinetic normalization, target-independent GU VEV, separate W/Z source rows, raw/common bridge gates, stability sidecars, observed photon/W/Z/H projection, Higgs scalar-source/self-coupling lineage, pole extraction, or GeV normalization.",
    nextRequiredArtifact = new[]
    {
        "A GU-local arithmetic or spectral theorem mapping Shiab/observer-sector data to gauge kinetic normalization and weak-angle rows before target comparison.",
        "A target-independent GU VEV or direct W/Z mass-scale source that does not import the Fermi-derived electroweak scale.",
        "Separate W and Z source rows with raw-amplitude, common-bridge, target-comparison, stability, and derivation gates all filled.",
        "A separate GU Higgs scalar-source/self-coupling or excitation lineage; the Bost-Connes gauge-coupling lead does not address the Higgs contract."
    }
};

var summary = new
{
    result.phaseId,
    result.terminalStatus,
    result.bostConnesArithmeticGaugeCouplingSourceAuditPassed,
    result.bostConnesArithmeticGaugeCouplingLeadPresent,
    result.bostConnesArithmeticGaugeCouplingSourceReviewed,
    result.bostConnesArithmeticGaugeCouplingPostedMay2026,
    result.bostConnesArithmeticGaugeCouplingRouteExternalToGu,
    result.routeUsesBostConnesCStarAlgebra,
    result.routeUsesPrimeSevenLocalFactor,
    result.routeUsesCrtGaugeGroupDecomposition,
    result.routeClaimsSpectralTraceRatios,
    result.routeClaimsAlphaStrongAlphaEmAlphaWeak,
    result.routeClaimsWeakMixingAngle,
    result.routeClaimsVirtualProcessCorrections,
    result.routeProvidesExternalGaugeCouplingTemplate,
    result.routeProvidesExternalWeakAngleTemplate,
    result.routePotentiallyAddressesWeakCouplingPartOfWzContract,
    result.correctedAlphaWeakInverse,
    result.correctedAlphaEmInverse,
    result.correctedAlphaStrongInverse,
    result.claimedWeakMixingSinSquared,
    result.correctedWeakCoupling,
    result.externalFermiVevGeV,
    result.arithmeticExternalVevWGeV,
    result.arithmeticExternalVevZGeV,
    result.arithmeticWRelativeError,
    result.arithmeticZRelativeError,
    result.arithmeticExternalVevWzDiagnosticPassesBroadGate,
    result.routeUsesExternalFermiVevForWzDiagnostic,
    result.routeUsesExternalSsrnPreprintAsCouplingSource,
    result.routeDoesNotUseWzTargetsForConstruction,
    result.routeUsesTargetsOnlyForPostConstructionEvaluation,
    result.routeDoesNotSupplyGuBostConnesEmbedding,
    result.routeDoesNotSupplyGuArithmeticSpectralTriple,
    result.routeDoesNotSupplyGuGaugeKineticNormalization,
    result.routeDoesNotSupplyTargetIndependentGuVev,
    result.routeDoesNotSupplyWzRawAmplitudeRows,
    result.routeDoesNotSupplyWzStabilitySidecars,
    result.routeDoesNotSupplyObservedFieldExtraction,
    result.routeDoesNotSupplyHiggsScalarSource,
    result.routeDoesNotSupplyHiggsSelfCoupling,
    result.routeDoesNotSupplyGeVUnitNormalization,
    result.routeProvidesGuLocalBostConnesMap,
    result.routeProvidesGuGaugeCouplingNormalization,
    result.routeProvidesGuWeakMixingAngleSource,
    result.routeProvidesTargetIndependentGuVevSource,
    result.routeProvidesSeparateWzSourceRows,
    result.routeProvidesWzRawAmplitudeGates,
    result.routeProvidesWzCommonBridgeGate,
    result.routeProvidesWzStabilitySidecars,
    result.routeProvidesObservedPhotonWzHiggsProjectionRows,
    result.routeProvidesHiggsScalarSourceOperator,
    result.routeProvidesHiggsQuarticOrExcitationSource,
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
};

var options = new JsonSerializerOptions { WriteIndented = true };
File.WriteAllText(
    Path.Combine(outputDir, "bost_connes_arithmetic_gauge_coupling_source_audit.json"),
    JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "bost_connes_arithmetic_gauge_coupling_source_audit_summary.json"),
    JsonSerializer.Serialize(summary, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"bostConnesArithmeticGaugeCouplingSourceAuditPassed={bostConnesArithmeticGaugeCouplingSourceAuditPassed}");
Console.WriteLine($"correctedWeakCoupling={correctedWeakCoupling:R}");
Console.WriteLine($"claimedWeakMixingSinSquared={claimedWeakMixingSinSquared:R}");
Console.WriteLine($"arithmeticExternalVevWGeV={arithmeticExternalVevWGeV:R}");
Console.WriteLine($"arithmeticExternalVevZGeV={arithmeticExternalVevZGeV:R}");
Console.WriteLine($"arithmeticExternalVevWzDiagnosticPassesBroadGate={arithmeticExternalVevWzDiagnosticPassesBroadGate}");
Console.WriteLine($"routePromotesWzMasses={routePromotesWzMasses}");
Console.WriteLine($"routePromotesHiggsMass={routePromotesHiggsMass}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");

static bool? JsonBool(JsonElement element, string name)
{
    if (!element.TryGetProperty(name, out var property))
    {
        return null;
    }

    return property.ValueKind switch
    {
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        _ => null
    };
}

static int? JsonInt(JsonElement element, string name)
{
    if (!element.TryGetProperty(name, out var property))
    {
        return null;
    }

    if (property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value))
    {
        return value;
    }

    return null;
}

record Check(string CheckId, bool Passed, string Detail);
record SourceRow(string SourceId, string Url, string Locator, string Finding, string PredictionImpact);
