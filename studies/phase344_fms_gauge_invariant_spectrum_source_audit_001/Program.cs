using System.Text.Json;

const string DefaultOutputDir = "studies/phase344_fms_gauge_invariant_spectrum_source_audit_001/output";
const string Phase201Path = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase256Path = "studies/phase256_observed_field_extraction_intake_contract_001/output/observed_field_extraction_intake_contract_summary.json";
const string Phase257Path = "studies/phase257_observation_pipeline_physical_boson_capability_audit_001/output/observation_pipeline_physical_boson_capability_audit_summary.json";
const string Phase295Path = "studies/phase295_observed_field_extraction_contract_candidate_scan_001/output/observed_field_extraction_contract_candidate_scan_summary.json";
const string Phase323Path = "studies/phase323_coupled_yang_mills_higgs_mass_extraction_audit_001/output/coupled_yang_mills_higgs_mass_extraction_audit_summary.json";
const string Phase342Path = "studies/phase342_higgsless_boundary_condition_source_audit_001/output/higgsless_boundary_condition_source_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE344_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase256 = JsonDocument.Parse(File.ReadAllText(Phase256Path));
using var phase257 = JsonDocument.Parse(File.ReadAllText(Phase257Path));
using var phase295 = JsonDocument.Parse(File.ReadAllText(Phase295Path));
using var phase323 = JsonDocument.Parse(File.ReadAllText(Phase323Path));
using var phase342 = JsonDocument.Parse(File.ReadAllText(Phase342Path));

const string originalFmsDoi = "https://doi.org/10.1016/0550-3213(81)90448-X";
const string fmsLegacyReviewUrl = "https://arxiv.org/abs/2305.01960";
const string observableBehSpectrumUrl = "https://arxiv.org/abs/1709.07477";
const string suNGaugeHiggsSpectrumUrl = "https://arxiv.org/abs/1710.01941";
const string twoHiggsDoubletFmsUrl = "https://arxiv.org/abs/1601.02006";
const string fmsQuantumGravityUrl = "https://arxiv.org/abs/1908.02140";

const bool fmsGaugeInvariantSpectrumSourceAuditPassedExpected = true;
const bool fmsLeadPresent = true;
const bool fmsPrimarySourcesReviewed = true;
const bool fmsRouteExternalToGu = true;
const bool originalFmsGaugeInvariantOperatorLeadPresent = true;
const bool physicalSpectrumMadeOfGaugeInvariantStates = true;
const bool standardModelFmsMapsCompositeStatesToElementaryWzh = true;
const bool fmsUsesBehExpansionAroundHiggsField = true;
const bool fmsProvidesObservedFieldExtractionTemplate = true;
const bool fmsExtensionCanChangeBsmSpectrum = true;
const bool fmsQuantumGravityDiffeomorphismInvariantExtensionLeadPresent = true;
const bool routeOverlapsObservedFieldExtraction = true;
const bool routeOverlapsCoupledYangMillsHiggsMassExtraction = true;
const bool routeOverlapsHiggslessBoundaryAsContrast = true;

const bool fmsRouteRequiresSmHiggsDoubletAndVev = true;
const bool fmsRouteRequiresGuLocalGaugeInvariantCompositeOperators = true;
const bool fmsRouteRequiresGuObservedVacuumAndExpansion = true;
const bool fmsRouteRequiresCorrelationFunctionPoleExtraction = true;
const bool fmsRouteRequiresPhotonWzHiggsProjectionRows = true;
const bool fmsRouteRequiresTargetIndependentMassScaleAndCouplings = true;
const bool fmsRouteRequiresGuScalarSourceOperator = true;
const bool fmsRouteRequiresGeVUnitNormalization = true;

const bool fmsRouteProvidesGuObservedFieldExtractionTheorem = false;
const bool fmsRouteProvidesGuLocalWzTheorem = false;
const bool fmsRouteProvidesSeparateWzSourceRows = false;
const bool fmsRouteProvidesTargetIndependentVevOrMassScale = false;
const bool fmsRouteProvidesGuWeakMixingAngleSource = false;
const bool fmsRouteProvidesGuGaugeCouplingNormalization = false;
const bool fmsRouteProvidesGuHiggsScalarSourceOperator = false;
const bool fmsRouteProvidesObservedHiggsMassFromGu = false;
const bool fmsRouteProvidesGeVUnitNormalization = false;
const bool fmsRoutePromotesObservedFieldExtraction = false;
const bool fmsRoutePromotesWzMasses = false;
const bool fmsRoutePromotesHiggsMass = false;
const bool fmsRouteCompletesBosonPredictions = false;
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
var currentImplementationCanFillObservedFieldExtractionContract = JsonBool(phase257.RootElement, "currentImplementationCanFillObservedFieldExtractionContract") is true;
var directObservationPipelineBosonCapable = JsonBool(phase257.RootElement, "directObservationPipelineBosonCapable") is true;
var phase3ObservationPipelineBosonCapable = JsonBool(phase257.RootElement, "phase3ObservationPipelineBosonCapable") is true;
var spectrumPhysicalBosonMassMatrixCapable = JsonBool(phase257.RootElement, "spectrumPhysicalBosonMassMatrixCapable") is true;
var observedFieldExtractionContractCandidateScanPassed = JsonBool(phase295.RootElement, "observedFieldExtractionContractCandidateScanPassed") is true;
var anyObservedFieldExtractionCandidateFillsContract = JsonBool(phase295.RootElement, "anyObservedFieldExtractionCandidateFillsContract") is true;
var intakeReadyObservedFieldExtractionCandidateCount = JsonInt(phase295.RootElement, "intakeReadyObservedFieldExtractionCandidateCount") ?? -1;
var coupledYangMillsHiggsMassExtractionAuditPassed = JsonBool(phase323.RootElement, "coupledYangMillsHiggsMassExtractionAuditPassed") is true;
var coupledYangMillsHiggsRouteCompletesBosonPredictions = JsonBool(phase323.RootElement, "coupledYangMillsHiggsRouteCompletesBosonPredictions") is true;
var higgslessBoundaryConditionSourceAuditPassed = JsonBool(phase342.RootElement, "higgslessBoundaryConditionSourceAuditPassed") is true;
var higgslessRouteCompletesBosonPredictions = JsonBool(phase342.RootElement, "higgslessRouteCompletesBosonPredictions") is true;

var sourceRows = new[]
{
    new SourceRow(
        "doi-10.1016-0550-3213-81-90448-x-fms-original",
        originalFmsDoi,
        "Original FMS gauge-invariant Higgs phenomenon",
        "Formulates the Higgs phenomenon without treating local gauge symmetry breaking as an observable order parameter.",
        "Foundational observed-spectrum lead; not a GU-local mass-source theorem."),
    new SourceRow(
        "arxiv-2305.01960-fms-underestimated-legacy",
        fmsLegacyReviewUrl,
        "FMS mechanism review",
        "Reviews how gauge-invariant composite operators can map to gauge-fixed elementary Higgs, W, Z, and fermion descriptions in the Standard Model.",
        "Strong observed-field extraction template; still requires GU-local operators, vacuum, and source scale."),
    new SourceRow(
        "arxiv-1709.07477-observable-spectrum-beh-effect",
        observableBehSpectrumUrl,
        "Observable spectrum of theories with a Brout-Englert-Higgs effect",
        "Explains that physical spectra are gauge-invariant and that FMS can map Standard Model observable states to W, Z, and Higgs elementary fields.",
        "Directly relevant to observed W/Z/H identity, but not a GU source-lineage row."),
    new SourceRow(
        "arxiv-1710.01941-su-n-gauge-higgs-spectrum",
        suNGaugeHiggsSpectrumUrl,
        "SU(N) gauge-Higgs spectrum emergence",
        "Studies how particle spectra emerge in SU(N) gauge theories with a fundamental Higgs and where FMS mappings can change beyond the Standard Model.",
        "Useful boundary for BSM caution; not a target-independent GU prediction."),
    new SourceRow(
        "arxiv-1601.02006-two-higgs-doublet-fms",
        twoHiggsDoubletFmsUrl,
        "Gauge-invariant spectrum in the two-Higgs-doublet model",
        "Applies FMS reasoning to 2HDM spectra and checks how physical gauge-invariant states relate to elementary fields.",
        "Shows the method is model-dependent; GU still needs its own scalar/operator source."),
    new SourceRow(
        "arxiv-1908.02140-fms-and-quantum-gravity",
        fmsQuantumGravityUrl,
        "FMS mechanism and quantum gravity",
        "Extends the gauge-invariant operator idea to Yang-Mills-Higgs theory coupled to gravity and diffeomorphism-invariant physical objects.",
        "Closest conceptual bridge to GU geometry, but still no GU W/Z/H mass-source rows.")
};

var checks = new[]
{
    new Check(
        "fms-primary-sources-reviewed",
        fmsLeadPresent && fmsPrimarySourcesReviewed && fmsRouteExternalToGu && sourceRows.Length == 6,
        $"lead={fmsLeadPresent}; reviewed={fmsPrimarySourcesReviewed}; externalToGu={fmsRouteExternalToGu}; sourceRows={sourceRows.Length}"),
    new Check(
        "gauge-invariant-observed-spectrum-lead-captured",
        originalFmsGaugeInvariantOperatorLeadPresent
            && physicalSpectrumMadeOfGaugeInvariantStates
            && standardModelFmsMapsCompositeStatesToElementaryWzh
            && fmsUsesBehExpansionAroundHiggsField
            && fmsProvidesObservedFieldExtractionTemplate
            && fmsExtensionCanChangeBsmSpectrum
            && fmsQuantumGravityDiffeomorphismInvariantExtensionLeadPresent,
        $"original={originalFmsGaugeInvariantOperatorLeadPresent}; physicalSpectrumGaugeInvariant={physicalSpectrumMadeOfGaugeInvariantStates}; smMap={standardModelFmsMapsCompositeStatesToElementaryWzh}; behExpansion={fmsUsesBehExpansionAroundHiggsField}; template={fmsProvidesObservedFieldExtractionTemplate}; bsmCaution={fmsExtensionCanChangeBsmSpectrum}; quantumGravity={fmsQuantumGravityDiffeomorphismInvariantExtensionLeadPresent}"),
    new Check(
        "current-observed-field-blockers-preserved",
        routeOverlapsObservedFieldExtraction
            && observedFieldExtractionRequiredFieldCount == 20
            && observedFieldExtractionFilledRequiredFieldCount == 0
            && !observedFieldExtractionContractPromotable
            && !currentImplementationCanFillObservedFieldExtractionContract
            && !directObservationPipelineBosonCapable
            && !phase3ObservationPipelineBosonCapable
            && !spectrumPhysicalBosonMassMatrixCapable
            && observedFieldExtractionContractCandidateScanPassed
            && !anyObservedFieldExtractionCandidateFillsContract
            && intakeReadyObservedFieldExtractionCandidateCount == 0,
        $"phase256Required={observedFieldExtractionRequiredFieldCount}; phase256Filled={observedFieldExtractionFilledRequiredFieldCount}; phase256Promotable={observedFieldExtractionContractPromotable}; phase257CanFill={currentImplementationCanFillObservedFieldExtractionContract}; direct={directObservationPipelineBosonCapable}; phase3={phase3ObservationPipelineBosonCapable}; massMatrix={spectrumPhysicalBosonMassMatrixCapable}; phase295={observedFieldExtractionContractCandidateScanPassed}; intakeReady={intakeReadyObservedFieldExtractionCandidateCount}"),
    new Check(
        "related-electroweak-routes-preserved",
        routeOverlapsCoupledYangMillsHiggsMassExtraction
            && coupledYangMillsHiggsMassExtractionAuditPassed
            && !coupledYangMillsHiggsRouteCompletesBosonPredictions
            && routeOverlapsHiggslessBoundaryAsContrast
            && higgslessBoundaryConditionSourceAuditPassed
            && !higgslessRouteCompletesBosonPredictions,
        $"p323={coupledYangMillsHiggsMassExtractionAuditPassed}; p323Completes={coupledYangMillsHiggsRouteCompletesBosonPredictions}; p342={higgslessBoundaryConditionSourceAuditPassed}; p342Completes={higgslessRouteCompletesBosonPredictions}"),
    new Check(
        "fms-route-requires-missing-gu-source-data",
        fmsRouteRequiresSmHiggsDoubletAndVev
            && fmsRouteRequiresGuLocalGaugeInvariantCompositeOperators
            && fmsRouteRequiresGuObservedVacuumAndExpansion
            && fmsRouteRequiresCorrelationFunctionPoleExtraction
            && fmsRouteRequiresPhotonWzHiggsProjectionRows
            && fmsRouteRequiresTargetIndependentMassScaleAndCouplings
            && fmsRouteRequiresGuScalarSourceOperator
            && fmsRouteRequiresGeVUnitNormalization,
        $"smHiggsVev={fmsRouteRequiresSmHiggsDoubletAndVev}; guOperators={fmsRouteRequiresGuLocalGaugeInvariantCompositeOperators}; guVacuum={fmsRouteRequiresGuObservedVacuumAndExpansion}; poles={fmsRouteRequiresCorrelationFunctionPoleExtraction}; projection={fmsRouteRequiresPhotonWzHiggsProjectionRows}; massScale={fmsRouteRequiresTargetIndependentMassScaleAndCouplings}; scalarSource={fmsRouteRequiresGuScalarSourceOperator}; gev={fmsRouteRequiresGeVUnitNormalization}"),
    new Check(
        "fms-route-does-not-fill-gu-contracts",
        !fmsRouteProvidesGuObservedFieldExtractionTheorem
            && !fmsRouteProvidesGuLocalWzTheorem
            && !fmsRouteProvidesSeparateWzSourceRows
            && !fmsRouteProvidesTargetIndependentVevOrMassScale
            && !fmsRouteProvidesGuWeakMixingAngleSource
            && !fmsRouteProvidesGuGaugeCouplingNormalization
            && !fmsRouteProvidesGuHiggsScalarSourceOperator
            && !fmsRouteProvidesObservedHiggsMassFromGu
            && !fmsRouteProvidesGeVUnitNormalization
            && !fmsRoutePromotesObservedFieldExtraction
            && !fmsRoutePromotesWzMasses
            && !fmsRoutePromotesHiggsMass
            && !fmsRouteCompletesBosonPredictions,
        $"observedTheorem={fmsRouteProvidesGuObservedFieldExtractionTheorem}; guWzTheorem={fmsRouteProvidesGuLocalWzTheorem}; sourceRows={fmsRouteProvidesSeparateWzSourceRows}; massScale={fmsRouteProvidesTargetIndependentVevOrMassScale}; weakAngle={fmsRouteProvidesGuWeakMixingAngleSource}; gaugeNorm={fmsRouteProvidesGuGaugeCouplingNormalization}; scalarSource={fmsRouteProvidesGuHiggsScalarSourceOperator}; observedHiggs={fmsRouteProvidesObservedHiggsMassFromGu}; gev={fmsRouteProvidesGeVUnitNormalization}; promotesObserved={fmsRoutePromotesObservedFieldExtraction}; promotesWz={fmsRoutePromotesWzMasses}; promotesHiggs={fmsRoutePromotesHiggsMass}"),
    new Check(
        "phase201-phase256-contract-state-unchanged",
        !phase201AllRequiredLineagesPromotable
            && !existingEvidenceFound
            && wzMissingFieldCount == 15
            && higgsMissingFieldCount == 14
            && !canFillPhase201WzContract
            && !canFillPhase201HiggsContract
            && !canFillPhase256ObservedFieldExtractionContract,
        $"phase201Promotable={phase201AllRequiredLineagesPromotable}; existingEvidence={existingEvidenceFound}; wzMissing={wzMissingFieldCount}; higgsMissing={higgsMissingFieldCount}; canFillWz={canFillPhase201WzContract}; canFillHiggs={canFillPhase201HiggsContract}; canFillObserved={canFillPhase256ObservedFieldExtractionContract}")
};

var fmsGaugeInvariantSpectrumSourceAuditPassed = checks.All(check => check.Passed)
    && fmsGaugeInvariantSpectrumSourceAuditPassedExpected
    && !fmsRouteProvidesGuObservedFieldExtractionTheorem
    && !fmsRouteProvidesGuLocalWzTheorem
    && !fmsRouteProvidesSeparateWzSourceRows
    && !fmsRoutePromotesObservedFieldExtraction
    && !fmsRoutePromotesWzMasses
    && !fmsRoutePromotesHiggsMass
    && !fmsRouteCompletesBosonPredictions
    && !canFillPhase201WzContract
    && !canFillPhase201HiggsContract
    && !canFillPhase256ObservedFieldExtractionContract;

var terminalStatus = fmsGaugeInvariantSpectrumSourceAuditPassed
    ? "fms-gauge-invariant-spectrum-source-audit-observed-map-template-not-gu-source"
    : "fms-gauge-invariant-spectrum-source-audit-review-required";

var result = new
{
    phaseId = "phase344-fms-gauge-invariant-spectrum-source-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    fmsGaugeInvariantSpectrumSourceAuditPassed,
    fmsLeadPresent,
    fmsPrimarySourcesReviewed,
    fmsRouteExternalToGu,
    originalFmsGaugeInvariantOperatorLeadPresent,
    physicalSpectrumMadeOfGaugeInvariantStates,
    standardModelFmsMapsCompositeStatesToElementaryWzh,
    fmsUsesBehExpansionAroundHiggsField,
    fmsProvidesObservedFieldExtractionTemplate,
    fmsExtensionCanChangeBsmSpectrum,
    fmsQuantumGravityDiffeomorphismInvariantExtensionLeadPresent,
    routeOverlapsObservedFieldExtraction,
    routeOverlapsCoupledYangMillsHiggsMassExtraction,
    routeOverlapsHiggslessBoundaryAsContrast,
    fmsRouteRequiresSmHiggsDoubletAndVev,
    fmsRouteRequiresGuLocalGaugeInvariantCompositeOperators,
    fmsRouteRequiresGuObservedVacuumAndExpansion,
    fmsRouteRequiresCorrelationFunctionPoleExtraction,
    fmsRouteRequiresPhotonWzHiggsProjectionRows,
    fmsRouteRequiresTargetIndependentMassScaleAndCouplings,
    fmsRouteRequiresGuScalarSourceOperator,
    fmsRouteRequiresGeVUnitNormalization,
    fmsRouteProvidesGuObservedFieldExtractionTheorem,
    fmsRouteProvidesGuLocalWzTheorem,
    fmsRouteProvidesSeparateWzSourceRows,
    fmsRouteProvidesTargetIndependentVevOrMassScale,
    fmsRouteProvidesGuWeakMixingAngleSource,
    fmsRouteProvidesGuGaugeCouplingNormalization,
    fmsRouteProvidesGuHiggsScalarSourceOperator,
    fmsRouteProvidesObservedHiggsMassFromGu,
    fmsRouteProvidesGeVUnitNormalization,
    fmsRoutePromotesObservedFieldExtraction,
    fmsRoutePromotesWzMasses,
    fmsRoutePromotesHiggsMass,
    fmsRouteCompletesBosonPredictions,
    canFillPhase201WzContract,
    canFillPhase201HiggsContract,
    canFillPhase256ObservedFieldExtractionContract,
    sourceRows,
    checks,
    contractImpact = new
    {
        canFillPhase201WzContract,
        canFillPhase201HiggsContract,
        canFillPhase256ObservedFieldExtractionContract,
        wzMissingFieldCount,
        higgsMissingFieldCount,
        observedFieldExtractionFilledRequiredFieldCount
    },
    relatedObservedFieldEvidence = new
    {
        observedFieldExtractionRequiredFieldCount,
        observedFieldExtractionFilledRequiredFieldCount,
        observedFieldExtractionContractPromotable,
        currentImplementationCanFillObservedFieldExtractionContract,
        directObservationPipelineBosonCapable,
        phase3ObservationPipelineBosonCapable,
        spectrumPhysicalBosonMassMatrixCapable,
        observedFieldExtractionContractCandidateScanPassed,
        anyObservedFieldExtractionCandidateFillsContract,
        intakeReadyObservedFieldExtractionCandidateCount,
        coupledYangMillsHiggsMassExtractionAuditPassed,
        coupledYangMillsHiggsRouteCompletesBosonPredictions,
        higgslessBoundaryConditionSourceAuditPassed,
        higgslessRouteCompletesBosonPredictions
    },
    decision = "Do not promote W/Z or Higgs physical masses from the FMS gauge-invariant spectrum mechanism. FMS is a strong external template for observed-field extraction because it maps gauge-invariant composite electroweak states to the familiar elementary W/Z/H fields in the Standard Model regime, but the current repository still lacks GU-local composite operators, observed vacuum/expansion data, correlation-function pole extraction, source-lineage mass scales, Higgs scalar-source lineage, and GeV normalization.",
    nextRequiredArtifact = new[]
    {
        "A GU-local FMS-like theorem defining gauge-invariant photon/W/Z/H composite operators from native fields.",
        "A source-derived observed-sector vacuum and expansion rule, not a gauge-fixed Standard Model import.",
        "Correlation-function or spectral-pole extraction rows for W, Z, and Higgs with stability sidecars.",
        "Target-independent scale, coupling, weak-angle, scalar-source, and GeV-normalization lineage validated through Phase201/Phase256."
    }
};

var fullPath = Path.Combine(outputDir, "fms_gauge_invariant_spectrum_source_audit.json");
var summaryPath = Path.Combine(outputDir, "fms_gauge_invariant_spectrum_source_audit_summary.json");
var jsonOptions = new JsonSerializerOptions { WriteIndented = true };

File.WriteAllText(fullPath, JsonSerializer.Serialize(result, jsonOptions));
File.WriteAllText(summaryPath, JsonSerializer.Serialize(new
{
    result.phaseId,
    result.terminalStatus,
    result.fmsGaugeInvariantSpectrumSourceAuditPassed,
    result.fmsLeadPresent,
    result.fmsPrimarySourcesReviewed,
    result.fmsRouteExternalToGu,
    result.originalFmsGaugeInvariantOperatorLeadPresent,
    result.physicalSpectrumMadeOfGaugeInvariantStates,
    result.standardModelFmsMapsCompositeStatesToElementaryWzh,
    result.fmsUsesBehExpansionAroundHiggsField,
    result.fmsProvidesObservedFieldExtractionTemplate,
    result.fmsExtensionCanChangeBsmSpectrum,
    result.fmsQuantumGravityDiffeomorphismInvariantExtensionLeadPresent,
    result.routeOverlapsObservedFieldExtraction,
    result.fmsRouteRequiresSmHiggsDoubletAndVev,
    result.fmsRouteRequiresGuLocalGaugeInvariantCompositeOperators,
    result.fmsRouteRequiresGuObservedVacuumAndExpansion,
    result.fmsRouteRequiresCorrelationFunctionPoleExtraction,
    result.fmsRouteRequiresPhotonWzHiggsProjectionRows,
    result.fmsRouteRequiresTargetIndependentMassScaleAndCouplings,
    result.fmsRouteRequiresGuScalarSourceOperator,
    result.fmsRouteRequiresGeVUnitNormalization,
    result.fmsRouteProvidesGuObservedFieldExtractionTheorem,
    result.fmsRouteProvidesGuLocalWzTheorem,
    result.fmsRouteProvidesSeparateWzSourceRows,
    result.fmsRouteProvidesTargetIndependentVevOrMassScale,
    result.fmsRouteProvidesGuWeakMixingAngleSource,
    result.fmsRouteProvidesGuGaugeCouplingNormalization,
    result.fmsRouteProvidesGuHiggsScalarSourceOperator,
    result.fmsRouteProvidesObservedHiggsMassFromGu,
    result.fmsRouteProvidesGeVUnitNormalization,
    result.fmsRoutePromotesObservedFieldExtraction,
    result.fmsRoutePromotesWzMasses,
    result.fmsRoutePromotesHiggsMass,
    result.fmsRouteCompletesBosonPredictions,
    result.canFillPhase201WzContract,
    result.canFillPhase201HiggsContract,
    result.canFillPhase256ObservedFieldExtractionContract,
    sourceRowCount = sourceRows.Length,
    result.contractImpact,
    result.relatedObservedFieldEvidence,
    result.decision,
    result.nextRequiredArtifact
}, jsonOptions));

Console.WriteLine(terminalStatus);
Console.WriteLine($"fmsGaugeInvariantSpectrumSourceAuditPassed={fmsGaugeInvariantSpectrumSourceAuditPassed}");
Console.WriteLine($"standardModelFmsMapsCompositeStatesToElementaryWzh={standardModelFmsMapsCompositeStatesToElementaryWzh}");
Console.WriteLine($"fmsProvidesObservedFieldExtractionTemplate={fmsProvidesObservedFieldExtractionTemplate}");
Console.WriteLine($"fmsRoutePromotesObservedFieldExtraction={fmsRoutePromotesObservedFieldExtraction}");
Console.WriteLine($"fmsRoutePromotesWzMasses={fmsRoutePromotesWzMasses}");
Console.WriteLine($"fmsRoutePromotesHiggsMass={fmsRoutePromotesHiggsMass}");
Console.WriteLine($"canFillPhase256ObservedFieldExtractionContract={canFillPhase256ObservedFieldExtractionContract}");

static bool? JsonBool(JsonElement element, string propertyName)
{
    if (!element.TryGetProperty(propertyName, out var value))
    {
        return null;
    }

    return value.ValueKind switch
    {
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        _ => null
    };
}

static int? JsonInt(JsonElement element, string propertyName)
{
    if (!element.TryGetProperty(propertyName, out var value) || value.ValueKind != JsonValueKind.Number)
    {
        return null;
    }

    return value.TryGetInt32(out var parsed) ? parsed : null;
}

public sealed record SourceRow(string SourceId, string Url, string FindingKind, string Summary, string PredictionImpact);

public sealed record Check(string CheckId, bool Passed, string Detail);
