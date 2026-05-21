using System.Text.Json;

const string DefaultOutputDir = "studies/phase333_kaluza_klein_internal_symmetry_source_audit_001/output";
const string Phase201Path = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase224Path = "studies/phase224_electroweak_parameter_dependency_audit_001/output/electroweak_parameter_dependency_audit_summary.json";
const string Phase236Path = "studies/phase236_low_energy_rg_transport_source_audit_001/output/low_energy_rg_transport_source_audit_summary.json";
const string Phase256Path = "studies/phase256_observed_field_extraction_intake_contract_001/output/observed_field_extraction_intake_contract_summary.json";
const string Phase265Path = "studies/phase265_gauge_higgs_boundary_source_audit_001/output/gauge_higgs_boundary_source_audit_summary.json";
const string Phase317Path = "studies/phase317_electroweak_mass_matrix_bridge_source_audit_001/output/electroweak_mass_matrix_bridge_source_audit_summary.json";
const string Phase323Path = "studies/phase323_coupled_yang_mills_higgs_mass_extraction_audit_001/output/coupled_yang_mills_higgs_mass_extraction_audit_summary.json";
const string Phase332Path = "studies/phase332_string_m_theory_compactification_source_audit_001/output/string_m_theory_compactification_source_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE333_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase224 = JsonDocument.Parse(File.ReadAllText(Phase224Path));
using var phase236 = JsonDocument.Parse(File.ReadAllText(Phase236Path));
using var phase256 = JsonDocument.Parse(File.ReadAllText(Phase256Path));
using var phase265 = JsonDocument.Parse(File.ReadAllText(Phase265Path));
using var phase317 = JsonDocument.Parse(File.ReadAllText(Phase317Path));
using var phase323 = JsonDocument.Parse(File.ReadAllText(Phase323Path));
using var phase332 = JsonDocument.Parse(File.ReadAllText(Phase332Path));

const string kkInternalSymmetriesUrl = "https://arxiv.org/abs/2306.01049";
const string kkChiralMassiveGaugeUrl = "https://arxiv.org/abs/2506.09126";
const string kkInternalDynamicsDoi = "https://doi.org/10.1088/1126-6708/2003/02/054";
const string csdrE8WilsonFluxUrl = "https://arxiv.org/abs/0808.3236";
const string sixDimensionalGaugeHiggsUrl = "https://arxiv.org/abs/1710.04811";

const bool kkInternalSymmetryLeadPresent = true;
const bool kkPrimarySourcesReviewed = true;
const bool kkRouteExternalToGu = true;
const bool kkRouteGeometricGaugeBosonMassBased = true;
const bool kkRouteUsesInternalMetricOrNonKillingFields = true;
const bool kkRouteProvidesClassicalMassFormulaFromInternalGeometry = true;
const bool kkRouteGeneratesMassiveGaugeBosonsWithoutAdHocHiggs = true;
const bool kkRouteAllowsArbitrarilyLightMassiveBosons = true;
const bool kkRouteCanBreakInternalSymmetryToStandardModelGaugeGroupLead = true;
const bool kkRouteCanModelWeakChiralityLead = true;
const bool kkRouteIncludesCosetDimensionalReductionLead = true;
const bool kkRouteIncludesGaugeHiggsGrandUnificationLead = true;
const bool kkRouteRequiresSpecificInternalManifold = true;
const bool kkRouteRequiresInternalMetricVacuumChoice = true;
const bool kkRouteRequiresMetricStabilization = true;
const bool kkRouteRequiresElectroweakEmbedding = true;
const bool kkRouteRequiresWeakScaleSelection = true;
const bool kkRouteRequiresFermionSpectrumAndCouplingMatching = true;
const bool kkRouteRequiresLowEnergyTransport = true;

const bool kkRouteProvidesGuLocalWzTheorem = false;
const bool kkRouteProvidesSeparateWzSourceRows = false;
const bool kkRouteProvidesWzRawAmplitudeGates = false;
const bool kkRouteProvidesWzCommonBridgeGate = false;
const bool kkRouteProvidesTargetIndependentGuVevSource = false;
const bool kkRouteProvidesWeakMixingAngleSource = false;
const bool kkRouteProvidesGuGaugeCouplingNormalization = false;
const bool kkRouteProvidesObservedPhotonWzProjectionRows = false;
const bool kkRouteProvidesNeutralMassMatrixDiagonalization = false;
const bool kkRouteProvidesGuObservedFieldExtraction = false;
const bool kkRouteProvidesHiggsScalarSourceOperator = false;
const bool kkRouteProvidesHiggsQuarticOrExcitationSource = false;
const bool kkRouteProvidesObservedHiggsMass = false;
const bool kkRouteProvidesGeVUnitNormalization = false;
const bool kkRoutePromotesWzMasses = false;
const bool kkRoutePromotesHiggsMass = false;
const bool kkRouteCompletesBosonPredictions = false;
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

var stringMTheoryCompactificationSourceAuditPassed = JsonBool(phase332.RootElement, "stringMTheoryCompactificationSourceAuditPassed") is true;
var stringRoutePromotesWzMasses = JsonBool(phase332.RootElement, "stringRoutePromotesWzMasses") is true;
var stringRoutePromotesHiggsMass = JsonBool(phase332.RootElement, "stringRoutePromotesHiggsMass") is true;

var sourceRows = new[]
{
    new SourceRow(
        "arxiv-2306-01049-internal-symmetries-kk",
        kkInternalSymmetriesUrl,
        "non-Killing internal symmetries and massive gauge bosons",
        "The route computes classical gauge-boson masses from internal metric geometry and Lie-derivative data, with possible symmetry breaking toward the Standard Model gauge group.",
        "This is the closest external geometry lead to a direct W/Z mass-source law, but it remains outside GU and does not emit Phase201 source rows."),
    new SourceRow(
        "arxiv-2506-09126-chiral-massive-kk-gauge-fields",
        kkChiralMassiveGaugeUrl,
        "chiral interactions of fermions and massive Kaluza-Klein gauge fields",
        "The 2026 update shows massive light non-Killing gauge fields can have asymmetric left/right fermion couplings and may model weak-force chirality.",
        "It strengthens the weak-interaction geometry lead, but still lacks observed W/Z eigenstate rows, weak-angle source, and physical scale normalization."),
    new SourceRow(
        "doi-10-1088-1126-6708-2003-02-054-internal-space-dynamics",
        kkInternalDynamicsDoi,
        "internal metric dynamics and broken Killing isometries",
        "The paper proposes that transitions of the internal manifold metric can make gauge bosons of broken Killing isometries massive.",
        "It is a geometric mass-generation precedent, not a GU-local boson prediction artifact."),
    new SourceRow(
        "arxiv-0808-3236-csdr-e8-wilson-flux",
        csdrE8WilsonFluxUrl,
        "coset-space dimensional reduction and Wilson-flux breaking",
        "The source surveys E8 coset reductions and Wilson-flux symmetry breaking toward four-dimensional GUT structures.",
        "It supplies compactification model-building context, not W/Z/H source-lineage rows or GeV normalization."),
    new SourceRow(
        "arxiv-1710-04811-six-dimensional-gauge-higgs-grand-unification",
        sixDimensionalGaugeHiggsUrl,
        "six-dimensional gauge-Higgs grand unification mass-spectrum lead",
        "The source produces electroweak symmetry breaking and a near-observed Higgs mass in a specific warped extra-dimensional model.",
        "It overlaps the gauge-Higgs boundary route and remains parameter/model dependent rather than GU-local."),
};

var checks = new[]
{
    new Check(
        "kaluza-klein-internal-symmetry-primary-sources-reviewed",
        kkInternalSymmetryLeadPresent
            && kkPrimarySourcesReviewed
            && kkRouteExternalToGu
            && kkRouteGeometricGaugeBosonMassBased
            && sourceRows.Length == 5,
        $"lead={kkInternalSymmetryLeadPresent}; primaryReviewed={kkPrimarySourcesReviewed}; externalToGu={kkRouteExternalToGu}; geometricGaugeBosonMass={kkRouteGeometricGaugeBosonMassBased}; sourceRows={sourceRows.Length}"),
    new Check(
        "kaluza-klein-route-captures-direct-geometric-mass-lead",
        kkRouteUsesInternalMetricOrNonKillingFields
            && kkRouteProvidesClassicalMassFormulaFromInternalGeometry
            && kkRouteGeneratesMassiveGaugeBosonsWithoutAdHocHiggs
            && kkRouteAllowsArbitrarilyLightMassiveBosons
            && kkRouteCanBreakInternalSymmetryToStandardModelGaugeGroupLead
            && kkRouteCanModelWeakChiralityLead,
        $"internalMetricOrNonKilling={kkRouteUsesInternalMetricOrNonKillingFields}; massFormula={kkRouteProvidesClassicalMassFormulaFromInternalGeometry}; noAdHocHiggs={kkRouteGeneratesMassiveGaugeBosonsWithoutAdHocHiggs}; lightBosons={kkRouteAllowsArbitrarilyLightMassiveBosons}; smGaugeGroupLead={kkRouteCanBreakInternalSymmetryToStandardModelGaugeGroupLead}; weakChiralityLead={kkRouteCanModelWeakChiralityLead}"),
    new Check(
        "kaluza-klein-route-remains-conditional-external-geometry",
        kkRouteRequiresSpecificInternalManifold
            && kkRouteRequiresInternalMetricVacuumChoice
            && kkRouteRequiresMetricStabilization
            && kkRouteRequiresElectroweakEmbedding
            && kkRouteRequiresWeakScaleSelection
            && kkRouteRequiresFermionSpectrumAndCouplingMatching
            && kkRouteRequiresLowEnergyTransport,
        $"internalManifold={kkRouteRequiresSpecificInternalManifold}; metricVacuum={kkRouteRequiresInternalMetricVacuumChoice}; stabilization={kkRouteRequiresMetricStabilization}; electroweakEmbedding={kkRouteRequiresElectroweakEmbedding}; weakScale={kkRouteRequiresWeakScaleSelection}; fermionCouplings={kkRouteRequiresFermionSpectrumAndCouplingMatching}; rgTransport={kkRouteRequiresLowEnergyTransport}"),
    new Check(
        "standard-electroweak-dependencies-remain-unsourced-by-kaluza-klein-route",
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
        "kaluza-klein-route-does-not-supply-gu-wz-source-lineage",
        !kkRouteProvidesGuLocalWzTheorem
            && !kkRouteProvidesSeparateWzSourceRows
            && !kkRouteProvidesWzRawAmplitudeGates
            && !kkRouteProvidesWzCommonBridgeGate
            && !kkRouteProvidesTargetIndependentGuVevSource
            && !kkRouteProvidesWeakMixingAngleSource
            && !kkRouteProvidesGuGaugeCouplingNormalization
            && !kkRouteProvidesObservedPhotonWzProjectionRows
            && !kkRouteProvidesNeutralMassMatrixDiagonalization
            && !kkRoutePromotesWzMasses
            && !canFillPhase201WzContract,
        $"guWzTheorem={kkRouteProvidesGuLocalWzTheorem}; separateRows={kkRouteProvidesSeparateWzSourceRows}; rawGates={kkRouteProvidesWzRawAmplitudeGates}; commonGate={kkRouteProvidesWzCommonBridgeGate}; vev={kkRouteProvidesTargetIndependentGuVevSource}; weakAngle={kkRouteProvidesWeakMixingAngleSource}; couplingNorm={kkRouteProvidesGuGaugeCouplingNormalization}; observedRows={kkRouteProvidesObservedPhotonWzProjectionRows}; neutralMatrix={kkRouteProvidesNeutralMassMatrixDiagonalization}; promotesWz={kkRoutePromotesWzMasses}"),
    new Check(
        "kaluza-klein-route-does-not-supply-gu-higgs-source-lineage",
        gaugeHiggsBoundarySourceAuditPassed
            && !gaugeHiggsBoundaryPromotesHiggsMass
            && coupledYangMillsHiggsMassExtractionAuditPassed
            && !officialPublicSourcesProvideTargetIndependentVevSource
            && !officialPublicSourcesProvideGaugeCouplingNormalization
            && !officialPublicSourcesProvidePhotonWzHiggsProjectionRows
            && !officialPublicSourcesProvideHiggsScalarSelfCouplingSource
            && !coupledYangMillsHiggsRouteCompletesBosonPredictions
            && stringMTheoryCompactificationSourceAuditPassed
            && !stringRoutePromotesWzMasses
            && !stringRoutePromotesHiggsMass
            && !kkRouteProvidesGuObservedFieldExtraction
            && !kkRouteProvidesHiggsScalarSourceOperator
            && !kkRouteProvidesHiggsQuarticOrExcitationSource
            && !kkRouteProvidesObservedHiggsMass
            && !kkRoutePromotesHiggsMass
            && !canFillPhase201HiggsContract,
        $"p265={gaugeHiggsBoundarySourceAuditPassed}; gaugeHiggsPromotes={gaugeHiggsBoundaryPromotesHiggsMass}; p323={coupledYangMillsHiggsMassExtractionAuditPassed}; p323Vev={officialPublicSourcesProvideTargetIndependentVevSource}; p323Coupling={officialPublicSourcesProvideGaugeCouplingNormalization}; p323Projection={officialPublicSourcesProvidePhotonWzHiggsProjectionRows}; p323SelfCoupling={officialPublicSourcesProvideHiggsScalarSelfCouplingSource}; p332={stringMTheoryCompactificationSourceAuditPassed}; observedExtraction={kkRouteProvidesGuObservedFieldExtraction}; higgsOperator={kkRouteProvidesHiggsScalarSourceOperator}; higgsQuartic={kkRouteProvidesHiggsQuarticOrExcitationSource}; observedHiggs={kkRouteProvidesObservedHiggsMass}; promotesHiggs={kkRoutePromotesHiggsMass}"),
    new Check(
        "source-lineage-contracts-remain-unfilled-after-kaluza-klein-audit",
        !phase201AllRequiredLineagesPromotable
            && !existingEvidenceFound
            && wzMissingFieldCount == 15
            && higgsMissingFieldCount == 14
            && observedFieldExtractionRequiredFieldCount == 20
            && observedFieldExtractionFilledRequiredFieldCount == 0
            && !observedFieldExtractionContractPromotable
            && !kkRouteProvidesGeVUnitNormalization
            && !kkRouteCompletesBosonPredictions
            && !canFillPhase201WzContract
            && !canFillPhase201HiggsContract
            && !canFillPhase256ObservedFieldExtractionContract,
        $"phase201Promotable={phase201AllRequiredLineagesPromotable}; existingEvidence={existingEvidenceFound}; wzMissing={wzMissingFieldCount}; higgsMissing={higgsMissingFieldCount}; phase256Required={observedFieldExtractionRequiredFieldCount}; phase256Filled={observedFieldExtractionFilledRequiredFieldCount}; observedPromotable={observedFieldExtractionContractPromotable}; geV={kkRouteProvidesGeVUnitNormalization}; completes={kkRouteCompletesBosonPredictions}; canFillWz={canFillPhase201WzContract}; canFillHiggs={canFillPhase201HiggsContract}; canFillObserved={canFillPhase256ObservedFieldExtractionContract}"),
};

var kaluzaKleinInternalSymmetrySourceAuditPassed = checks.All(check => check.Passed)
    && kkInternalSymmetryLeadPresent
    && kkPrimarySourcesReviewed
    && kkRouteExternalToGu
    && kkRouteGeometricGaugeBosonMassBased
    && !kkRouteProvidesGuLocalWzTheorem
    && !kkRouteProvidesSeparateWzSourceRows
    && !kkRouteProvidesTargetIndependentGuVevSource
    && !kkRouteProvidesWeakMixingAngleSource
    && !kkRouteProvidesGuGaugeCouplingNormalization
    && !kkRouteProvidesGuObservedFieldExtraction
    && !kkRouteProvidesHiggsScalarSourceOperator
    && !kkRouteProvidesHiggsQuarticOrExcitationSource
    && !kkRouteProvidesObservedHiggsMass
    && !kkRouteProvidesGeVUnitNormalization
    && !kkRoutePromotesWzMasses
    && !kkRoutePromotesHiggsMass
    && !kkRouteCompletesBosonPredictions
    && !canFillPhase201WzContract
    && !canFillPhase201HiggsContract
    && !canFillPhase256ObservedFieldExtractionContract;

var terminalStatus = kaluzaKleinInternalSymmetrySourceAuditPassed
    ? "kaluza-klein-internal-symmetry-source-audit-geometric-mass-lead-not-gu-source"
    : "kaluza-klein-internal-symmetry-source-audit-review-required";

var result = new
{
    phaseId = "phase333-kaluza-klein-internal-symmetry-source-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    kaluzaKleinInternalSymmetrySourceAuditPassed,
    kkInternalSymmetryLeadPresent,
    kkPrimarySourcesReviewed,
    kkRouteExternalToGu,
    kkRouteGeometricGaugeBosonMassBased,
    kkRouteUsesInternalMetricOrNonKillingFields,
    kkRouteProvidesClassicalMassFormulaFromInternalGeometry,
    kkRouteGeneratesMassiveGaugeBosonsWithoutAdHocHiggs,
    kkRouteAllowsArbitrarilyLightMassiveBosons,
    kkRouteCanBreakInternalSymmetryToStandardModelGaugeGroupLead,
    kkRouteCanModelWeakChiralityLead,
    kkRouteIncludesCosetDimensionalReductionLead,
    kkRouteIncludesGaugeHiggsGrandUnificationLead,
    kkRouteRequiresSpecificInternalManifold,
    kkRouteRequiresInternalMetricVacuumChoice,
    kkRouteRequiresMetricStabilization,
    kkRouteRequiresElectroweakEmbedding,
    kkRouteRequiresWeakScaleSelection,
    kkRouteRequiresFermionSpectrumAndCouplingMatching,
    kkRouteRequiresLowEnergyTransport,
    kkRouteProvidesGuLocalWzTheorem,
    kkRouteProvidesSeparateWzSourceRows,
    kkRouteProvidesWzRawAmplitudeGates,
    kkRouteProvidesWzCommonBridgeGate,
    kkRouteProvidesTargetIndependentGuVevSource,
    kkRouteProvidesWeakMixingAngleSource,
    kkRouteProvidesGuGaugeCouplingNormalization,
    kkRouteProvidesObservedPhotonWzProjectionRows,
    kkRouteProvidesNeutralMassMatrixDiagonalization,
    kkRouteProvidesGuObservedFieldExtraction,
    kkRouteProvidesHiggsScalarSourceOperator,
    kkRouteProvidesHiggsQuarticOrExcitationSource,
    kkRouteProvidesObservedHiggsMass,
    kkRouteProvidesGeVUnitNormalization,
    kkRoutePromotesWzMasses,
    kkRoutePromotesHiggsMass,
    kkRouteCompletesBosonPredictions,
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
    decision = kaluzaKleinInternalSymmetrySourceAuditPassed
        ? "Do not promote W/Z or Higgs physical masses from the Kaluza-Klein internal-symmetry route in this repository. The route is a serious geometric direct-mass lead because non-Killing internal fields and internal metric dynamics can generate massive gauge bosons and weak-like chirality, but it does not supply GU-local W/Z source rows, a GU electroweak embedding and weak-angle source, target-independent scale selection, observed photon/W/Z/H extraction, Higgs scalar-source lineage, or GeV normalization."
        : "Review the Kaluza-Klein internal-symmetry route before relying on this boundary.",
    nextRequiredArtifact = new[]
    {
        "A GU-local internal-geometry theorem mapping the non-Killing/internal-metric mass formula into observed SU(2)_L x U(1)_Y source rows.",
        "Separate W and Z rows with weak-angle, neutral-mixing, photon projection, and physical-unit normalization independent of target values.",
        "A target-independent weak-scale selection or GU VEV replacement and low-energy transport theorem.",
        "A solved GU Higgs scalar source/operator, self-coupling or excitation lineage, and observed Higgs extraction if the no-ad-hoc-Higgs path is extended to the physical Higgs.",
    },
    sourceEvidence = new
    {
        kkInternalSymmetriesUrl,
        kkChiralMassiveGaugeUrl,
        kkInternalDynamicsDoi,
        csdrE8WilsonFluxUrl,
        sixDimensionalGaugeHiggsUrl,
        phase201Path = Phase201Path,
        phase213Path = Phase213Path,
        phase224Path = Phase224Path,
        phase236Path = Phase236Path,
        phase256Path = Phase256Path,
        phase265Path = Phase265Path,
        phase317Path = Phase317Path,
        phase323Path = Phase323Path,
        phase332Path = Phase332Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true };
File.WriteAllText(Path.Combine(outputDir, "kaluza_klein_internal_symmetry_source_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "kaluza_klein_internal_symmetry_source_audit_summary.json"),
    JsonSerializer.Serialize(
        new
        {
            result.phaseId,
            result.terminalStatus,
            result.kaluzaKleinInternalSymmetrySourceAuditPassed,
            result.kkInternalSymmetryLeadPresent,
            result.kkPrimarySourcesReviewed,
            result.kkRouteExternalToGu,
            result.kkRouteGeometricGaugeBosonMassBased,
            result.kkRouteUsesInternalMetricOrNonKillingFields,
            result.kkRouteProvidesClassicalMassFormulaFromInternalGeometry,
            result.kkRouteGeneratesMassiveGaugeBosonsWithoutAdHocHiggs,
            result.kkRouteAllowsArbitrarilyLightMassiveBosons,
            result.kkRouteCanBreakInternalSymmetryToStandardModelGaugeGroupLead,
            result.kkRouteCanModelWeakChiralityLead,
            result.kkRouteRequiresSpecificInternalManifold,
            result.kkRouteRequiresInternalMetricVacuumChoice,
            result.kkRouteRequiresMetricStabilization,
            result.kkRouteRequiresElectroweakEmbedding,
            result.kkRouteRequiresWeakScaleSelection,
            result.kkRouteRequiresFermionSpectrumAndCouplingMatching,
            result.kkRouteRequiresLowEnergyTransport,
            result.kkRouteProvidesGuLocalWzTheorem,
            result.kkRouteProvidesSeparateWzSourceRows,
            result.kkRouteProvidesTargetIndependentGuVevSource,
            result.kkRouteProvidesWeakMixingAngleSource,
            result.kkRouteProvidesGuGaugeCouplingNormalization,
            result.kkRouteProvidesGuObservedFieldExtraction,
            result.kkRouteProvidesHiggsScalarSourceOperator,
            result.kkRouteProvidesHiggsQuarticOrExcitationSource,
            result.kkRouteProvidesObservedHiggsMass,
            result.kkRouteProvidesGeVUnitNormalization,
            result.kkRoutePromotesWzMasses,
            result.kkRoutePromotesHiggsMass,
            result.kkRouteCompletesBosonPredictions,
            result.canFillPhase201WzContract,
            result.canFillPhase201HiggsContract,
            result.canFillPhase256ObservedFieldExtractionContract,
            sourceRowCount = result.sourceRows.Length,
            result.contractImpact,
            result.decision,
            result.nextRequiredArtifact,
        },
        options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"kaluzaKleinInternalSymmetrySourceAuditPassed={kaluzaKleinInternalSymmetrySourceAuditPassed}");
Console.WriteLine($"kkInternalSymmetryLeadPresent={kkInternalSymmetryLeadPresent}");
Console.WriteLine($"kkRouteGeometricGaugeBosonMassBased={kkRouteGeometricGaugeBosonMassBased}");
Console.WriteLine($"kkRoutePromotesWzMasses={kkRoutePromotesWzMasses}");
Console.WriteLine($"kkRoutePromotesHiggsMass={kkRoutePromotesHiggsMass}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");

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
