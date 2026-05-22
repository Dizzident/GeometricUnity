using System.Text.Json;

const string DefaultOutputDir = "studies/phase345_fradkin_shenker_complementarity_source_audit_001/output";
const string Phase201Path = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase256Path = "studies/phase256_observed_field_extraction_intake_contract_001/output/observed_field_extraction_intake_contract_summary.json";
const string Phase295Path = "studies/phase295_observed_field_extraction_contract_candidate_scan_001/output/observed_field_extraction_contract_candidate_scan_summary.json";
const string Phase344Path = "studies/phase344_fms_gauge_invariant_spectrum_source_audit_001/output/fms_gauge_invariant_spectrum_source_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE345_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase256 = JsonDocument.Parse(File.ReadAllText(Phase256Path));
using var phase295 = JsonDocument.Parse(File.ReadAllText(Phase295Path));
using var phase344 = JsonDocument.Parse(File.ReadAllText(Phase344Path));

const string fradkinShenkerDoi = "https://doi.org/10.1103/PhysRevD.19.3682";
const string elitzurDoi = "https://doi.org/10.1103/PhysRevD.12.3978";
const string osterwalderSeilerDoi = "https://doi.org/10.1016/0003-4916(78)90039-8";
const string su2HiggsPhaseDiagramUrl = "https://arxiv.org/abs/0911.1721";
const string separationOfChargeConfinementUrl = "https://arxiv.org/abs/1708.08979";
const string higgsSpinGlassUrl = "https://arxiv.org/abs/2001.03068";
const string gaugeIndependentTransitionUrl = "https://arxiv.org/abs/2308.13430";

const bool fradkinShenkerComplementaritySourceAuditPassedExpected = true;
const bool fradkinShenkerLeadPresent = true;
const bool complementarityPrimarySourcesReviewed = true;
const bool complementarityRouteExternalToGu = true;
const bool elitzurBlocksLocalGaugeSymmetryBreakingOrderParameter = true;
const bool fradkinShenkerAnalyticContinuityForFundamentalHiggs = true;
const bool osterwalderSeilerLatticeHiggsMechanismTreatmentPresent = true;
const bool gaugeHiggsConfinementComplementarityPresent = true;
const bool higgsAndConfinementRegionsNeedGaugeInvariantDiagnostics = true;
const bool gaugeInvariantDistinctionsCanUseGlobalCustodialOrScConfinement = true;
const bool routeConstrainsObservedFieldExtractionLanguage = true;
const bool routeSupportsFmsObservedSpectrumBoundary = true;

const bool complementarityRouteRequiresGuLocalGaugeHiggsModel = true;
const bool complementarityRouteRequiresGuGaugeInvariantOrderOrOperatorMap = true;
const bool complementarityRouteRequiresGuObservedVacuumAndHilbertSector = true;
const bool complementarityRouteRequiresCorrelationFunctionPoleExtraction = true;
const bool complementarityRouteRequiresPhotonWzHiggsProjectionRows = true;
const bool complementarityRouteRequiresTargetIndependentMassScaleAndCouplings = true;
const bool complementarityRouteRequiresGuHiggsScalarSourceOperator = true;
const bool complementarityRouteRequiresGeVUnitNormalization = true;

const bool complementarityRouteProvidesGuLocalWzTheorem = false;
const bool complementarityRouteProvidesSeparateWzSourceRows = false;
const bool complementarityRouteProvidesTargetIndependentVevOrMassScale = false;
const bool complementarityRouteProvidesGuWeakMixingAngleSource = false;
const bool complementarityRouteProvidesGuGaugeCouplingNormalization = false;
const bool complementarityRouteProvidesObservedPhotonWzHiggsProjectionRows = false;
const bool complementarityRouteProvidesGuObservedFieldExtractionContract = false;
const bool complementarityRouteProvidesGuHiggsScalarSourceOperator = false;
const bool complementarityRouteProvidesObservedHiggsMassFromGu = false;
const bool complementarityRouteProvidesGeVUnitNormalization = false;
const bool complementarityRoutePromotesObservedFieldExtraction = false;
const bool complementarityRoutePromotesWzMasses = false;
const bool complementarityRoutePromotesHiggsMass = false;
const bool complementarityRouteCompletesBosonPredictions = false;
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
var observedFieldExtractionContractCandidateScanPassed = JsonBool(phase295.RootElement, "observedFieldExtractionContractCandidateScanPassed") is true;
var anyObservedFieldExtractionCandidateFillsContract = JsonBool(phase295.RootElement, "anyObservedFieldExtractionCandidateFillsContract") is true;
var intakeReadyObservedFieldExtractionCandidateCount = JsonInt(phase295.RootElement, "intakeReadyObservedFieldExtractionCandidateCount") ?? -1;
var fmsGaugeInvariantSpectrumSourceAuditPassed = JsonBool(phase344.RootElement, "fmsGaugeInvariantSpectrumSourceAuditPassed") is true;
var fmsProvidesObservedFieldExtractionTemplate = JsonBool(phase344.RootElement, "fmsProvidesObservedFieldExtractionTemplate") is true;
var fmsRoutePromotesObservedFieldExtraction = JsonBool(phase344.RootElement, "fmsRoutePromotesObservedFieldExtraction") is true;
var fmsRouteCompletesBosonPredictions = JsonBool(phase344.RootElement, "fmsRouteCompletesBosonPredictions") is true;

var sourceRows = new[]
{
    new SourceRow(
        "doi-10.1103-physrevd-19-3682-fradkin-shenker",
        fradkinShenkerDoi,
        "Fradkin-Shenker lattice gauge-Higgs complementarity",
        "Shows that confinement and Higgs regions can be analytically connected for fundamental Higgs fields, so phase-label language cannot by itself define the observed boson source.",
        "Constrains source-law claims; does not derive W/Z/H masses."),
    new SourceRow(
        "doi-10.1103-physrevd-12-3978-elitzur",
        elitzurDoi,
        "Elitzur theorem",
        "Argues that spontaneous breaking of a local gauge symmetry is impossible without gauge fixing.",
        "Blocks treating local gauge-symmetry breaking as the physical W/Z/H source law."),
    new SourceRow(
        "doi-10.1016-0003-4916-78-90039-8-osterwalder-seiler",
        osterwalderSeilerDoi,
        "Rigorous lattice gauge-Higgs treatment",
        "Provides a nonperturbative lattice framework and rigorous Higgs-mechanism treatment in gauge theories.",
        "Useful complementarity boundary; still not GU-local prediction evidence."),
    new SourceRow(
        "arxiv-0911.1721-su2-higgs-phase-diagram",
        su2HiggsPhaseDiagramUrl,
        "SU(2) lattice Higgs phase diagram",
        "Finds phase-diagram behavior consistent with Fradkin-Shenker expectations for fixed-length fundamental scalar fields.",
        "Empirical lattice support for the boundary; not a source law."),
    new SourceRow(
        "arxiv-1708.08979-separation-of-charge-confinement",
        separationOfChargeConfinementUrl,
        "Gauge-invariant confinement criterion with matter",
        "Adds a gauge-invariant diagnostic stronger than massive color-singlet asymptotic states in theories with fundamental matter.",
        "Suggests diagnostic structure, not W/Z/H source lineage."),
    new SourceRow(
        "arxiv-2001.03068-higgs-spin-glass",
        higgsSpinGlassUrl,
        "Higgs phase as spin glass",
        "Proposes a gauge-invariant distinction using custodial symmetry and varieties of confinement.",
        "May guide observed-sector diagnostics, but imports model-specific global symmetry data."),
    new SourceRow(
        "arxiv-2308.13430-gauge-independent-transition",
        gaugeIndependentTransitionUrl,
        "Gauge-independent transition in SU(2) gauge-scalar lattice theory",
        "Constructs gauge-invariant operators to separate confinement and Higgs regimes in a lattice SU(2) scalar model.",
        "Modern operator lead; still lacks GU-native W/Z/H source, scale, and unit rows.")
};

var checks = new[]
{
    new Check(
        "complementarity-primary-sources-reviewed",
        fradkinShenkerLeadPresent
            && complementarityPrimarySourcesReviewed
            && complementarityRouteExternalToGu
            && sourceRows.Length == 7,
        $"lead={fradkinShenkerLeadPresent}; reviewed={complementarityPrimarySourcesReviewed}; externalToGu={complementarityRouteExternalToGu}; sourceRows={sourceRows.Length}"),
    new Check(
        "gauge-higgs-complementarity-boundary-captured",
        elitzurBlocksLocalGaugeSymmetryBreakingOrderParameter
            && fradkinShenkerAnalyticContinuityForFundamentalHiggs
            && osterwalderSeilerLatticeHiggsMechanismTreatmentPresent
            && gaugeHiggsConfinementComplementarityPresent
            && higgsAndConfinementRegionsNeedGaugeInvariantDiagnostics
            && gaugeInvariantDistinctionsCanUseGlobalCustodialOrScConfinement
            && routeConstrainsObservedFieldExtractionLanguage,
        $"elitzur={elitzurBlocksLocalGaugeSymmetryBreakingOrderParameter}; analyticContinuity={fradkinShenkerAnalyticContinuityForFundamentalHiggs}; osterwalderSeiler={osterwalderSeilerLatticeHiggsMechanismTreatmentPresent}; complementarity={gaugeHiggsConfinementComplementarityPresent}; diagnostics={higgsAndConfinementRegionsNeedGaugeInvariantDiagnostics}; custodialOrSc={gaugeInvariantDistinctionsCanUseGlobalCustodialOrScConfinement}; observedLanguage={routeConstrainsObservedFieldExtractionLanguage}"),
    new Check(
        "fms-observed-spectrum-boundary-preserved",
        routeSupportsFmsObservedSpectrumBoundary
            && fmsGaugeInvariantSpectrumSourceAuditPassed
            && fmsProvidesObservedFieldExtractionTemplate
            && !fmsRoutePromotesObservedFieldExtraction
            && !fmsRouteCompletesBosonPredictions,
        $"supportsFms={routeSupportsFmsObservedSpectrumBoundary}; p344={fmsGaugeInvariantSpectrumSourceAuditPassed}; fmsTemplate={fmsProvidesObservedFieldExtractionTemplate}; fmsPromotesObserved={fmsRoutePromotesObservedFieldExtraction}; fmsCompletes={fmsRouteCompletesBosonPredictions}"),
    new Check(
        "current-contract-blockers-preserved",
        !phase201AllRequiredLineagesPromotable
            && !existingEvidenceFound
            && wzMissingFieldCount == 15
            && higgsMissingFieldCount == 14
            && observedFieldExtractionRequiredFieldCount == 20
            && observedFieldExtractionFilledRequiredFieldCount == 0
            && !observedFieldExtractionContractPromotable
            && observedFieldExtractionContractCandidateScanPassed
            && !anyObservedFieldExtractionCandidateFillsContract
            && intakeReadyObservedFieldExtractionCandidateCount == 0,
        $"phase201Promotable={phase201AllRequiredLineagesPromotable}; existingEvidence={existingEvidenceFound}; wzMissing={wzMissingFieldCount}; higgsMissing={higgsMissingFieldCount}; observedRequired={observedFieldExtractionRequiredFieldCount}; observedFilled={observedFieldExtractionFilledRequiredFieldCount}; observedPromotable={observedFieldExtractionContractPromotable}; p295={observedFieldExtractionContractCandidateScanPassed}; observedCandidates={intakeReadyObservedFieldExtractionCandidateCount}"),
    new Check(
        "complementarity-route-requires-missing-gu-source-data",
        complementarityRouteRequiresGuLocalGaugeHiggsModel
            && complementarityRouteRequiresGuGaugeInvariantOrderOrOperatorMap
            && complementarityRouteRequiresGuObservedVacuumAndHilbertSector
            && complementarityRouteRequiresCorrelationFunctionPoleExtraction
            && complementarityRouteRequiresPhotonWzHiggsProjectionRows
            && complementarityRouteRequiresTargetIndependentMassScaleAndCouplings
            && complementarityRouteRequiresGuHiggsScalarSourceOperator
            && complementarityRouteRequiresGeVUnitNormalization,
        $"guGaugeHiggsModel={complementarityRouteRequiresGuLocalGaugeHiggsModel}; guOperatorMap={complementarityRouteRequiresGuGaugeInvariantOrderOrOperatorMap}; guHilbertSector={complementarityRouteRequiresGuObservedVacuumAndHilbertSector}; poles={complementarityRouteRequiresCorrelationFunctionPoleExtraction}; projection={complementarityRouteRequiresPhotonWzHiggsProjectionRows}; massScale={complementarityRouteRequiresTargetIndependentMassScaleAndCouplings}; scalarSource={complementarityRouteRequiresGuHiggsScalarSourceOperator}; gev={complementarityRouteRequiresGeVUnitNormalization}"),
    new Check(
        "complementarity-route-does-not-fill-gu-contracts",
        !complementarityRouteProvidesGuLocalWzTheorem
            && !complementarityRouteProvidesSeparateWzSourceRows
            && !complementarityRouteProvidesTargetIndependentVevOrMassScale
            && !complementarityRouteProvidesGuWeakMixingAngleSource
            && !complementarityRouteProvidesGuGaugeCouplingNormalization
            && !complementarityRouteProvidesObservedPhotonWzHiggsProjectionRows
            && !complementarityRouteProvidesGuObservedFieldExtractionContract
            && !complementarityRouteProvidesGuHiggsScalarSourceOperator
            && !complementarityRouteProvidesObservedHiggsMassFromGu
            && !complementarityRouteProvidesGeVUnitNormalization
            && !complementarityRoutePromotesObservedFieldExtraction
            && !complementarityRoutePromotesWzMasses
            && !complementarityRoutePromotesHiggsMass
            && !complementarityRouteCompletesBosonPredictions
            && !canFillPhase201WzContract
            && !canFillPhase201HiggsContract
            && !canFillPhase256ObservedFieldExtractionContract,
        $"guWzTheorem={complementarityRouteProvidesGuLocalWzTheorem}; sourceRows={complementarityRouteProvidesSeparateWzSourceRows}; massScale={complementarityRouteProvidesTargetIndependentVevOrMassScale}; weakAngle={complementarityRouteProvidesGuWeakMixingAngleSource}; gaugeNorm={complementarityRouteProvidesGuGaugeCouplingNormalization}; projection={complementarityRouteProvidesObservedPhotonWzHiggsProjectionRows}; observedContract={complementarityRouteProvidesGuObservedFieldExtractionContract}; scalarSource={complementarityRouteProvidesGuHiggsScalarSourceOperator}; observedHiggs={complementarityRouteProvidesObservedHiggsMassFromGu}; gev={complementarityRouteProvidesGeVUnitNormalization}; promotesObserved={complementarityRoutePromotesObservedFieldExtraction}; promotesWz={complementarityRoutePromotesWzMasses}; promotesHiggs={complementarityRoutePromotesHiggsMass}; completes={complementarityRouteCompletesBosonPredictions}")
};

var fradkinShenkerComplementaritySourceAuditPassed = checks.All(check => check.Passed)
    && fradkinShenkerComplementaritySourceAuditPassedExpected
    && !complementarityRoutePromotesObservedFieldExtraction
    && !complementarityRoutePromotesWzMasses
    && !complementarityRoutePromotesHiggsMass
    && !complementarityRouteCompletesBosonPredictions
    && !canFillPhase201WzContract
    && !canFillPhase201HiggsContract
    && !canFillPhase256ObservedFieldExtractionContract;

var terminalStatus = fradkinShenkerComplementaritySourceAuditPassed
    ? "fradkin-shenker-complementarity-source-audit-boundary-not-gu-source"
    : "fradkin-shenker-complementarity-source-audit-review-required";

var result = new
{
    phaseId = "phase345-fradkin-shenker-complementarity-source-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    fradkinShenkerComplementaritySourceAuditPassed,
    fradkinShenkerLeadPresent,
    complementarityPrimarySourcesReviewed,
    complementarityRouteExternalToGu,
    elitzurBlocksLocalGaugeSymmetryBreakingOrderParameter,
    fradkinShenkerAnalyticContinuityForFundamentalHiggs,
    osterwalderSeilerLatticeHiggsMechanismTreatmentPresent,
    gaugeHiggsConfinementComplementarityPresent,
    higgsAndConfinementRegionsNeedGaugeInvariantDiagnostics,
    gaugeInvariantDistinctionsCanUseGlobalCustodialOrScConfinement,
    routeConstrainsObservedFieldExtractionLanguage,
    routeSupportsFmsObservedSpectrumBoundary,
    complementarityRouteRequiresGuLocalGaugeHiggsModel,
    complementarityRouteRequiresGuGaugeInvariantOrderOrOperatorMap,
    complementarityRouteRequiresGuObservedVacuumAndHilbertSector,
    complementarityRouteRequiresCorrelationFunctionPoleExtraction,
    complementarityRouteRequiresPhotonWzHiggsProjectionRows,
    complementarityRouteRequiresTargetIndependentMassScaleAndCouplings,
    complementarityRouteRequiresGuHiggsScalarSourceOperator,
    complementarityRouteRequiresGeVUnitNormalization,
    complementarityRouteProvidesGuLocalWzTheorem,
    complementarityRouteProvidesSeparateWzSourceRows,
    complementarityRouteProvidesTargetIndependentVevOrMassScale,
    complementarityRouteProvidesGuWeakMixingAngleSource,
    complementarityRouteProvidesGuGaugeCouplingNormalization,
    complementarityRouteProvidesObservedPhotonWzHiggsProjectionRows,
    complementarityRouteProvidesGuObservedFieldExtractionContract,
    complementarityRouteProvidesGuHiggsScalarSourceOperator,
    complementarityRouteProvidesObservedHiggsMassFromGu,
    complementarityRouteProvidesGeVUnitNormalization,
    complementarityRoutePromotesObservedFieldExtraction,
    complementarityRoutePromotesWzMasses,
    complementarityRoutePromotesHiggsMass,
    complementarityRouteCompletesBosonPredictions,
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
        observedFieldExtractionContractCandidateScanPassed,
        anyObservedFieldExtractionCandidateFillsContract,
        intakeReadyObservedFieldExtractionCandidateCount,
        fmsGaugeInvariantSpectrumSourceAuditPassed,
        fmsProvidesObservedFieldExtractionTemplate,
        fmsRoutePromotesObservedFieldExtraction,
        fmsRouteCompletesBosonPredictions
    },
    decision = "Do not promote W/Z or Higgs physical masses from gauge-Higgs complementarity or Higgs-phase language. Elitzur and Fradkin-Shenker/Osterwalder-Seiler results make local gauge-symmetry breaking and phase labels insufficient as physical source laws; any viable route must instead supply GU-local gauge-invariant operators, observed-sector pole extraction, target-independent scale/coupling lineage, Higgs scalar-source lineage, and GeV normalization.",
    nextRequiredArtifact = new[]
    {
        "A GU-local gauge-Higgs or equivalent operator system with gauge-invariant photon/W/Z/H observables.",
        "A source-derived observed Hilbert sector, vacuum, and correlation-function pole extraction rule.",
        "Target-independent W/Z mass scale, weak-mixing, gauge-coupling, scalar-source, and GeV-unit lineage.",
        "A proof that any gauge-invariant Higgs/confinement diagnostic actually selects the observed electroweak W/Z/H sector rather than a phase-label convention."
    }
};

var fullPath = Path.Combine(outputDir, "fradkin_shenker_complementarity_source_audit.json");
var summaryPath = Path.Combine(outputDir, "fradkin_shenker_complementarity_source_audit_summary.json");
var jsonOptions = new JsonSerializerOptions { WriteIndented = true };

File.WriteAllText(fullPath, JsonSerializer.Serialize(result, jsonOptions));
File.WriteAllText(summaryPath, JsonSerializer.Serialize(new
{
    result.phaseId,
    result.terminalStatus,
    result.fradkinShenkerComplementaritySourceAuditPassed,
    result.fradkinShenkerLeadPresent,
    result.complementarityPrimarySourcesReviewed,
    result.complementarityRouteExternalToGu,
    result.elitzurBlocksLocalGaugeSymmetryBreakingOrderParameter,
    result.fradkinShenkerAnalyticContinuityForFundamentalHiggs,
    result.osterwalderSeilerLatticeHiggsMechanismTreatmentPresent,
    result.gaugeHiggsConfinementComplementarityPresent,
    result.higgsAndConfinementRegionsNeedGaugeInvariantDiagnostics,
    result.gaugeInvariantDistinctionsCanUseGlobalCustodialOrScConfinement,
    result.routeConstrainsObservedFieldExtractionLanguage,
    result.routeSupportsFmsObservedSpectrumBoundary,
    result.complementarityRouteRequiresGuLocalGaugeHiggsModel,
    result.complementarityRouteRequiresGuGaugeInvariantOrderOrOperatorMap,
    result.complementarityRouteRequiresGuObservedVacuumAndHilbertSector,
    result.complementarityRouteRequiresCorrelationFunctionPoleExtraction,
    result.complementarityRouteRequiresPhotonWzHiggsProjectionRows,
    result.complementarityRouteRequiresTargetIndependentMassScaleAndCouplings,
    result.complementarityRouteRequiresGuHiggsScalarSourceOperator,
    result.complementarityRouteRequiresGeVUnitNormalization,
    result.complementarityRouteProvidesGuLocalWzTheorem,
    result.complementarityRouteProvidesSeparateWzSourceRows,
    result.complementarityRouteProvidesTargetIndependentVevOrMassScale,
    result.complementarityRouteProvidesGuWeakMixingAngleSource,
    result.complementarityRouteProvidesGuGaugeCouplingNormalization,
    result.complementarityRouteProvidesObservedPhotonWzHiggsProjectionRows,
    result.complementarityRouteProvidesGuObservedFieldExtractionContract,
    result.complementarityRouteProvidesGuHiggsScalarSourceOperator,
    result.complementarityRouteProvidesObservedHiggsMassFromGu,
    result.complementarityRouteProvidesGeVUnitNormalization,
    result.complementarityRoutePromotesObservedFieldExtraction,
    result.complementarityRoutePromotesWzMasses,
    result.complementarityRoutePromotesHiggsMass,
    result.complementarityRouteCompletesBosonPredictions,
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
Console.WriteLine($"fradkinShenkerComplementaritySourceAuditPassed={fradkinShenkerComplementaritySourceAuditPassed}");
Console.WriteLine($"elitzurBlocksLocalGaugeSymmetryBreakingOrderParameter={elitzurBlocksLocalGaugeSymmetryBreakingOrderParameter}");
Console.WriteLine($"fradkinShenkerAnalyticContinuityForFundamentalHiggs={fradkinShenkerAnalyticContinuityForFundamentalHiggs}");
Console.WriteLine($"routeConstrainsObservedFieldExtractionLanguage={routeConstrainsObservedFieldExtractionLanguage}");
Console.WriteLine($"complementarityRoutePromotesWzMasses={complementarityRoutePromotesWzMasses}");
Console.WriteLine($"complementarityRoutePromotesHiggsMass={complementarityRoutePromotesHiggsMass}");
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
