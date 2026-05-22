using System.Text.Json;

const string DefaultOutputDir = "studies/phase346_nielsen_pole_mass_gauge_independence_source_audit_001/output";
const string Phase201Path = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase256Path = "studies/phase256_observed_field_extraction_intake_contract_001/output/observed_field_extraction_intake_contract_summary.json";
const string Phase260Path = "studies/phase260_mass_definition_convention_sensitivity_audit_001/output/mass_definition_convention_sensitivity_audit_summary.json";
const string Phase295Path = "studies/phase295_observed_field_extraction_contract_candidate_scan_001/output/observed_field_extraction_contract_candidate_scan_summary.json";
const string Phase344Path = "studies/phase344_fms_gauge_invariant_spectrum_source_audit_001/output/fms_gauge_invariant_spectrum_source_audit_summary.json";
const string Phase345Path = "studies/phase345_fradkin_shenker_complementarity_source_audit_001/output/fradkin_shenker_complementarity_source_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE346_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase256 = JsonDocument.Parse(File.ReadAllText(Phase256Path));
using var phase260 = JsonDocument.Parse(File.ReadAllText(Phase260Path));
using var phase295 = JsonDocument.Parse(File.ReadAllText(Phase295Path));
using var phase344 = JsonDocument.Parse(File.ReadAllText(Phase344Path));
using var phase345 = JsonDocument.Parse(File.ReadAllText(Phase345Path));

const string nielsenSmMassDefinitionDoi = "https://doi.org/10.1103/PhysRevD.62.076002";
const string unstableParticleNielsenDoi = "https://doi.org/10.1103/PhysRevD.65.085001";
const string unstableParticleNielsenArxiv = "https://arxiv.org/abs/hep-ph/0109228";
const string complexMassSchemeDoi = "https://doi.org/10.1140/epjc/s10052-015-3579-2";
const string pinchTechniqueDoi = "https://doi.org/10.1103/PhysRevLett.75.3060";
const string algebraicRenormalizationDoi = "https://doi.org/10.1006/aphy.2001.6117";

const bool nielsenPoleMassGaugeIndependenceSourceAuditPassedExpected = true;
const bool nielsenPoleMassLeadPresent = true;
const bool nielsenPrimarySourcesReviewed = true;
const bool nielsenRouteExternalToGu = true;
const bool nielsenIdentitiesControlGaugeParameterDependence = true;
const bool complexPoleGaugeIndependentForSmPhysicalFields = true;
const bool mixingAndCpViolationCoveredBySmNielsenIdentity = true;
const bool poleResiduesAndPartialWidthsGaugeIndependentLead = true;
const bool complexMassSchemeUsesGaugeIndependentPoleRenormalization = true;
const bool pinchTechniqueResonantAmplitudeGaugeIndependentLead = true;
const bool routeConstrainsPhysicalMassConvention = true;
const bool routeSupportsGaugeInvariantObservedPoleExtractionBoundary = true;

const bool nielsenRouteRequiresGuBrstOrSlavnovTaylorControl = true;
const bool nielsenRouteRequiresGuTwoPointFunctionsOrCorrelationMatrix = true;
const bool nielsenRouteRequiresGuObservedFieldOperators = true;
const bool nielsenRouteRequiresGuPoleEquationAndResidueExtraction = true;
const bool nielsenRouteRequiresPhotonWzHiggsProjectionRows = true;
const bool nielsenRouteRequiresTargetIndependentMassScaleAndCouplings = true;
const bool nielsenRouteRequiresGuHiggsScalarSourceOperator = true;
const bool nielsenRouteRequiresGeVUnitNormalization = true;

const bool nielsenRouteProvidesGuLocalWzTheorem = false;
const bool nielsenRouteProvidesSeparateWzSourceRows = false;
const bool nielsenRouteProvidesTargetIndependentVevOrMassScale = false;
const bool nielsenRouteProvidesGuWeakMixingAngleSource = false;
const bool nielsenRouteProvidesGuGaugeCouplingNormalization = false;
const bool nielsenRouteProvidesObservedPhotonWzHiggsProjectionRows = false;
const bool nielsenRouteProvidesGuObservedFieldExtractionContract = false;
const bool nielsenRouteProvidesGuHiggsScalarSourceOperator = false;
const bool nielsenRouteProvidesObservedHiggsMassFromGu = false;
const bool nielsenRouteProvidesGeVUnitNormalization = false;
const bool nielsenRoutePromotesObservedFieldExtraction = false;
const bool nielsenRoutePromotesWzMasses = false;
const bool nielsenRoutePromotesHiggsMass = false;
const bool nielsenRouteCompletesBosonPredictions = false;
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
var massDefinitionConventionSensitivityAuditPassed = JsonBool(phase260.RootElement, "massDefinitionConventionSensitivityAuditPassed") is true;
var conventionShiftPromotesBosonMasses = JsonBool(phase260.RootElement, "conventionShiftPromotesBosonMasses") is true;
var failedComparisonsPersistUnderPoleConvention = JsonBool(phase260.RootElement, "failedComparisonsPersistUnderPoleConvention") is true;
var observedFieldExtractionContractCandidateScanPassed = JsonBool(phase295.RootElement, "observedFieldExtractionContractCandidateScanPassed") is true;
var anyObservedFieldExtractionCandidateFillsContract = JsonBool(phase295.RootElement, "anyObservedFieldExtractionCandidateFillsContract") is true;
var intakeReadyObservedFieldExtractionCandidateCount = JsonInt(phase295.RootElement, "intakeReadyObservedFieldExtractionCandidateCount") ?? -1;
var fmsGaugeInvariantSpectrumSourceAuditPassed = JsonBool(phase344.RootElement, "fmsGaugeInvariantSpectrumSourceAuditPassed") is true;
var fmsProvidesObservedFieldExtractionTemplate = JsonBool(phase344.RootElement, "fmsProvidesObservedFieldExtractionTemplate") is true;
var fmsRoutePromotesObservedFieldExtraction = JsonBool(phase344.RootElement, "fmsRoutePromotesObservedFieldExtraction") is true;
var fradkinShenkerComplementaritySourceAuditPassed = JsonBool(phase345.RootElement, "fradkinShenkerComplementaritySourceAuditPassed") is true;
var complementarityRoutePromotesObservedFieldExtraction = JsonBool(phase345.RootElement, "complementarityRoutePromotesObservedFieldExtraction") is true;

var sourceRows = new[]
{
    new SourceRow(
        "doi-10.1103-physrevd-62-076002-sm-nielsen-mass",
        nielsenSmMassDefinitionDoi,
        "Nielsen identities of the Standard Model and mass definition",
        "Shows that Nielsen identities control gauge-parameter dependence and that complex-pole masses of physical Standard Model fields are gauge independent to all orders, including mixing.",
        "Defines a physical pole-mass boundary; does not derive GU W/Z/H source rows."),
    new SourceRow(
        "doi-10.1103-physrevd-65-085001-widths-nielsen-identities",
        unstableParticleNielsenDoi,
        "Widths and partial widths from Nielsen identities",
        "Uses Nielsen identities to prove gauge independence of pole residues and associated partial-width definitions to all orders.",
        "Useful for resonance extraction consistency; not a mass-source law."),
    new SourceRow(
        "arxiv-hep-ph-0109228-widths-nielsen-identities",
        unstableParticleNielsenArxiv,
        "ArXiv record for Nielsen-identity width analysis",
        "Open source record for the unstable-particle Nielsen identity analysis and Z-gamma sector discussion.",
        "Reference access for the same gauge-independence boundary."),
    new SourceRow(
        "doi-10.1140-epjc-s10052-015-3579-2-complex-mass-scheme",
        complexMassSchemeDoi,
        "Complex-mass scheme and unitarity",
        "Reviews how complex-pole renormalization supports gauge-invariant treatment of unstable particles in perturbative QFT.",
        "Mass-convention and finite-width boundary; still imports Lagrangian inputs."),
    new SourceRow(
        "doi-10.1103-physrevlett-75-3060-pinch-technique",
        pinchTechniqueDoi,
        "Gauge-invariant resonant amplitudes via pinch technique",
        "Builds gauge-independent resonant transition amplitudes for Standard Model processes involving W/Z interactions.",
        "Amplitude extraction lead; no GU-local W/Z/H source scale."),
    new SourceRow(
        "doi-10.1006-aphy-2001-6117-algebraic-renormalization",
        algebraicRenormalizationDoi,
        "Practical algebraic renormalization",
        "Reviews Slavnov-Taylor and Ward-Takahashi identity control for Standard Model perturbative calculations.",
        "BRST/identity control template; no GU pole equation or mass values.")
};

var checks = new[]
{
    new Check(
        "nielsen-primary-sources-reviewed",
        nielsenPoleMassLeadPresent && nielsenPrimarySourcesReviewed && nielsenRouteExternalToGu && sourceRows.Length == 6,
        $"lead={nielsenPoleMassLeadPresent}; reviewed={nielsenPrimarySourcesReviewed}; externalToGu={nielsenRouteExternalToGu}; sourceRows={sourceRows.Length}"),
    new Check(
        "gauge-independent-pole-mass-boundary-captured",
        nielsenIdentitiesControlGaugeParameterDependence
            && complexPoleGaugeIndependentForSmPhysicalFields
            && mixingAndCpViolationCoveredBySmNielsenIdentity
            && poleResiduesAndPartialWidthsGaugeIndependentLead
            && complexMassSchemeUsesGaugeIndependentPoleRenormalization
            && pinchTechniqueResonantAmplitudeGaugeIndependentLead
            && routeConstrainsPhysicalMassConvention
            && routeSupportsGaugeInvariantObservedPoleExtractionBoundary,
        $"nielsenIdentity={nielsenIdentitiesControlGaugeParameterDependence}; complexPole={complexPoleGaugeIndependentForSmPhysicalFields}; mixing={mixingAndCpViolationCoveredBySmNielsenIdentity}; residues={poleResiduesAndPartialWidthsGaugeIndependentLead}; complexMassScheme={complexMassSchemeUsesGaugeIndependentPoleRenormalization}; pinchTechnique={pinchTechniqueResonantAmplitudeGaugeIndependentLead}; convention={routeConstrainsPhysicalMassConvention}; poleExtraction={routeSupportsGaugeInvariantObservedPoleExtractionBoundary}"),
    new Check(
        "mass-definition-and-observed-field-blockers-preserved",
        massDefinitionConventionSensitivityAuditPassed
            && !conventionShiftPromotesBosonMasses
            && failedComparisonsPersistUnderPoleConvention
            && observedFieldExtractionRequiredFieldCount == 20
            && observedFieldExtractionFilledRequiredFieldCount == 0
            && !observedFieldExtractionContractPromotable
            && observedFieldExtractionContractCandidateScanPassed
            && !anyObservedFieldExtractionCandidateFillsContract
            && intakeReadyObservedFieldExtractionCandidateCount == 0,
        $"p260={massDefinitionConventionSensitivityAuditPassed}; conventionPromotes={conventionShiftPromotesBosonMasses}; poleFailurePersists={failedComparisonsPersistUnderPoleConvention}; observedRequired={observedFieldExtractionRequiredFieldCount}; observedFilled={observedFieldExtractionFilledRequiredFieldCount}; observedPromotable={observedFieldExtractionContractPromotable}; p295={observedFieldExtractionContractCandidateScanPassed}; observedCandidates={intakeReadyObservedFieldExtractionCandidateCount}"),
    new Check(
        "fms-and-complementarity-boundaries-preserved",
        fmsGaugeInvariantSpectrumSourceAuditPassed
            && fmsProvidesObservedFieldExtractionTemplate
            && !fmsRoutePromotesObservedFieldExtraction
            && fradkinShenkerComplementaritySourceAuditPassed
            && !complementarityRoutePromotesObservedFieldExtraction,
        $"p344={fmsGaugeInvariantSpectrumSourceAuditPassed}; fmsTemplate={fmsProvidesObservedFieldExtractionTemplate}; fmsPromotesObserved={fmsRoutePromotesObservedFieldExtraction}; p345={fradkinShenkerComplementaritySourceAuditPassed}; complementarityPromotesObserved={complementarityRoutePromotesObservedFieldExtraction}"),
    new Check(
        "current-source-lineage-blockers-preserved",
        !phase201AllRequiredLineagesPromotable
            && !existingEvidenceFound
            && wzMissingFieldCount == 15
            && higgsMissingFieldCount == 14,
        $"phase201Promotable={phase201AllRequiredLineagesPromotable}; existingEvidence={existingEvidenceFound}; wzMissing={wzMissingFieldCount}; higgsMissing={higgsMissingFieldCount}"),
    new Check(
        "nielsen-route-requires-missing-gu-source-data",
        nielsenRouteRequiresGuBrstOrSlavnovTaylorControl
            && nielsenRouteRequiresGuTwoPointFunctionsOrCorrelationMatrix
            && nielsenRouteRequiresGuObservedFieldOperators
            && nielsenRouteRequiresGuPoleEquationAndResidueExtraction
            && nielsenRouteRequiresPhotonWzHiggsProjectionRows
            && nielsenRouteRequiresTargetIndependentMassScaleAndCouplings
            && nielsenRouteRequiresGuHiggsScalarSourceOperator
            && nielsenRouteRequiresGeVUnitNormalization,
        $"brst={nielsenRouteRequiresGuBrstOrSlavnovTaylorControl}; twoPoint={nielsenRouteRequiresGuTwoPointFunctionsOrCorrelationMatrix}; operators={nielsenRouteRequiresGuObservedFieldOperators}; poleEquation={nielsenRouteRequiresGuPoleEquationAndResidueExtraction}; projection={nielsenRouteRequiresPhotonWzHiggsProjectionRows}; massScale={nielsenRouteRequiresTargetIndependentMassScaleAndCouplings}; scalarSource={nielsenRouteRequiresGuHiggsScalarSourceOperator}; gev={nielsenRouteRequiresGeVUnitNormalization}"),
    new Check(
        "nielsen-route-does-not-fill-gu-contracts",
        !nielsenRouteProvidesGuLocalWzTheorem
            && !nielsenRouteProvidesSeparateWzSourceRows
            && !nielsenRouteProvidesTargetIndependentVevOrMassScale
            && !nielsenRouteProvidesGuWeakMixingAngleSource
            && !nielsenRouteProvidesGuGaugeCouplingNormalization
            && !nielsenRouteProvidesObservedPhotonWzHiggsProjectionRows
            && !nielsenRouteProvidesGuObservedFieldExtractionContract
            && !nielsenRouteProvidesGuHiggsScalarSourceOperator
            && !nielsenRouteProvidesObservedHiggsMassFromGu
            && !nielsenRouteProvidesGeVUnitNormalization
            && !nielsenRoutePromotesObservedFieldExtraction
            && !nielsenRoutePromotesWzMasses
            && !nielsenRoutePromotesHiggsMass
            && !nielsenRouteCompletesBosonPredictions
            && !canFillPhase201WzContract
            && !canFillPhase201HiggsContract
            && !canFillPhase256ObservedFieldExtractionContract,
        $"guWzTheorem={nielsenRouteProvidesGuLocalWzTheorem}; sourceRows={nielsenRouteProvidesSeparateWzSourceRows}; massScale={nielsenRouteProvidesTargetIndependentVevOrMassScale}; weakAngle={nielsenRouteProvidesGuWeakMixingAngleSource}; gaugeNorm={nielsenRouteProvidesGuGaugeCouplingNormalization}; projection={nielsenRouteProvidesObservedPhotonWzHiggsProjectionRows}; observedContract={nielsenRouteProvidesGuObservedFieldExtractionContract}; scalarSource={nielsenRouteProvidesGuHiggsScalarSourceOperator}; observedHiggs={nielsenRouteProvidesObservedHiggsMassFromGu}; gev={nielsenRouteProvidesGeVUnitNormalization}; promotesObserved={nielsenRoutePromotesObservedFieldExtraction}; promotesWz={nielsenRoutePromotesWzMasses}; promotesHiggs={nielsenRoutePromotesHiggsMass}; completes={nielsenRouteCompletesBosonPredictions}")
};

var nielsenPoleMassGaugeIndependenceSourceAuditPassed = checks.All(check => check.Passed)
    && nielsenPoleMassGaugeIndependenceSourceAuditPassedExpected
    && !nielsenRoutePromotesObservedFieldExtraction
    && !nielsenRoutePromotesWzMasses
    && !nielsenRoutePromotesHiggsMass
    && !nielsenRouteCompletesBosonPredictions
    && !canFillPhase201WzContract
    && !canFillPhase201HiggsContract
    && !canFillPhase256ObservedFieldExtractionContract;

var terminalStatus = nielsenPoleMassGaugeIndependenceSourceAuditPassed
    ? "nielsen-pole-mass-gauge-independence-source-audit-pole-boundary-not-gu-source"
    : "nielsen-pole-mass-gauge-independence-source-audit-review-required";

var result = new
{
    phaseId = "phase346-nielsen-pole-mass-gauge-independence-source-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    nielsenPoleMassGaugeIndependenceSourceAuditPassed,
    nielsenPoleMassLeadPresent,
    nielsenPrimarySourcesReviewed,
    nielsenRouteExternalToGu,
    nielsenIdentitiesControlGaugeParameterDependence,
    complexPoleGaugeIndependentForSmPhysicalFields,
    mixingAndCpViolationCoveredBySmNielsenIdentity,
    poleResiduesAndPartialWidthsGaugeIndependentLead,
    complexMassSchemeUsesGaugeIndependentPoleRenormalization,
    pinchTechniqueResonantAmplitudeGaugeIndependentLead,
    routeConstrainsPhysicalMassConvention,
    routeSupportsGaugeInvariantObservedPoleExtractionBoundary,
    nielsenRouteRequiresGuBrstOrSlavnovTaylorControl,
    nielsenRouteRequiresGuTwoPointFunctionsOrCorrelationMatrix,
    nielsenRouteRequiresGuObservedFieldOperators,
    nielsenRouteRequiresGuPoleEquationAndResidueExtraction,
    nielsenRouteRequiresPhotonWzHiggsProjectionRows,
    nielsenRouteRequiresTargetIndependentMassScaleAndCouplings,
    nielsenRouteRequiresGuHiggsScalarSourceOperator,
    nielsenRouteRequiresGeVUnitNormalization,
    nielsenRouteProvidesGuLocalWzTheorem,
    nielsenRouteProvidesSeparateWzSourceRows,
    nielsenRouteProvidesTargetIndependentVevOrMassScale,
    nielsenRouteProvidesGuWeakMixingAngleSource,
    nielsenRouteProvidesGuGaugeCouplingNormalization,
    nielsenRouteProvidesObservedPhotonWzHiggsProjectionRows,
    nielsenRouteProvidesGuObservedFieldExtractionContract,
    nielsenRouteProvidesGuHiggsScalarSourceOperator,
    nielsenRouteProvidesObservedHiggsMassFromGu,
    nielsenRouteProvidesGeVUnitNormalization,
    nielsenRoutePromotesObservedFieldExtraction,
    nielsenRoutePromotesWzMasses,
    nielsenRoutePromotesHiggsMass,
    nielsenRouteCompletesBosonPredictions,
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
    relatedPhysicalMassEvidence = new
    {
        massDefinitionConventionSensitivityAuditPassed,
        conventionShiftPromotesBosonMasses,
        failedComparisonsPersistUnderPoleConvention,
        observedFieldExtractionRequiredFieldCount,
        observedFieldExtractionFilledRequiredFieldCount,
        observedFieldExtractionContractPromotable,
        observedFieldExtractionContractCandidateScanPassed,
        anyObservedFieldExtractionCandidateFillsContract,
        intakeReadyObservedFieldExtractionCandidateCount,
        fmsGaugeInvariantSpectrumSourceAuditPassed,
        fmsProvidesObservedFieldExtractionTemplate,
        fmsRoutePromotesObservedFieldExtraction,
        fradkinShenkerComplementaritySourceAuditPassed,
        complementarityRoutePromotesObservedFieldExtraction
    },
    decision = "Do not promote W/Z or Higgs physical masses from Nielsen identities, complex-pole gauge independence, the complex-mass scheme, or pinch-technique resonance extraction. These sources define a rigorous gauge-independence boundary for physical pole masses and amplitudes, but the current repository still lacks GU-local BRST/Slavnov-Taylor control, observed W/Z/H operators, pole equations, target-independent scale/coupling lineage, Higgs scalar-source lineage, and GeV normalization.",
    nextRequiredArtifact = new[]
    {
        "A GU-local BRST/Slavnov-Taylor or equivalent identity proving gauge-parameter control for the observed sector.",
        "Gauge-invariant photon/W/Z/H two-point functions or correlation matrices with pole and residue extraction.",
        "Target-independent W/Z scale, weak-mixing, gauge-coupling, scalar-source, and GeV-unit lineage feeding those pole equations.",
        "A proof that the extracted GU poles correspond to observed physical W, Z, and Higgs states before target comparison."
    }
};

var fullPath = Path.Combine(outputDir, "nielsen_pole_mass_gauge_independence_source_audit.json");
var summaryPath = Path.Combine(outputDir, "nielsen_pole_mass_gauge_independence_source_audit_summary.json");
var jsonOptions = new JsonSerializerOptions { WriteIndented = true };

File.WriteAllText(fullPath, JsonSerializer.Serialize(result, jsonOptions));
File.WriteAllText(summaryPath, JsonSerializer.Serialize(new
{
    result.phaseId,
    result.terminalStatus,
    result.nielsenPoleMassGaugeIndependenceSourceAuditPassed,
    result.nielsenPoleMassLeadPresent,
    result.nielsenPrimarySourcesReviewed,
    result.nielsenRouteExternalToGu,
    result.nielsenIdentitiesControlGaugeParameterDependence,
    result.complexPoleGaugeIndependentForSmPhysicalFields,
    result.mixingAndCpViolationCoveredBySmNielsenIdentity,
    result.poleResiduesAndPartialWidthsGaugeIndependentLead,
    result.complexMassSchemeUsesGaugeIndependentPoleRenormalization,
    result.pinchTechniqueResonantAmplitudeGaugeIndependentLead,
    result.routeConstrainsPhysicalMassConvention,
    result.routeSupportsGaugeInvariantObservedPoleExtractionBoundary,
    result.nielsenRouteRequiresGuBrstOrSlavnovTaylorControl,
    result.nielsenRouteRequiresGuTwoPointFunctionsOrCorrelationMatrix,
    result.nielsenRouteRequiresGuObservedFieldOperators,
    result.nielsenRouteRequiresGuPoleEquationAndResidueExtraction,
    result.nielsenRouteRequiresPhotonWzHiggsProjectionRows,
    result.nielsenRouteRequiresTargetIndependentMassScaleAndCouplings,
    result.nielsenRouteRequiresGuHiggsScalarSourceOperator,
    result.nielsenRouteRequiresGeVUnitNormalization,
    result.nielsenRouteProvidesGuLocalWzTheorem,
    result.nielsenRouteProvidesSeparateWzSourceRows,
    result.nielsenRouteProvidesTargetIndependentVevOrMassScale,
    result.nielsenRouteProvidesGuWeakMixingAngleSource,
    result.nielsenRouteProvidesGuGaugeCouplingNormalization,
    result.nielsenRouteProvidesObservedPhotonWzHiggsProjectionRows,
    result.nielsenRouteProvidesGuObservedFieldExtractionContract,
    result.nielsenRouteProvidesGuHiggsScalarSourceOperator,
    result.nielsenRouteProvidesObservedHiggsMassFromGu,
    result.nielsenRouteProvidesGeVUnitNormalization,
    result.nielsenRoutePromotesObservedFieldExtraction,
    result.nielsenRoutePromotesWzMasses,
    result.nielsenRoutePromotesHiggsMass,
    result.nielsenRouteCompletesBosonPredictions,
    result.canFillPhase201WzContract,
    result.canFillPhase201HiggsContract,
    result.canFillPhase256ObservedFieldExtractionContract,
    sourceRowCount = sourceRows.Length,
    result.contractImpact,
    result.relatedPhysicalMassEvidence,
    result.decision,
    result.nextRequiredArtifact
}, jsonOptions));

Console.WriteLine(terminalStatus);
Console.WriteLine($"nielsenPoleMassGaugeIndependenceSourceAuditPassed={nielsenPoleMassGaugeIndependenceSourceAuditPassed}");
Console.WriteLine($"complexPoleGaugeIndependentForSmPhysicalFields={complexPoleGaugeIndependentForSmPhysicalFields}");
Console.WriteLine($"routeSupportsGaugeInvariantObservedPoleExtractionBoundary={routeSupportsGaugeInvariantObservedPoleExtractionBoundary}");
Console.WriteLine($"nielsenRoutePromotesObservedFieldExtraction={nielsenRoutePromotesObservedFieldExtraction}");
Console.WriteLine($"nielsenRoutePromotesWzMasses={nielsenRoutePromotesWzMasses}");
Console.WriteLine($"nielsenRoutePromotesHiggsMass={nielsenRoutePromotesHiggsMass}");
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
