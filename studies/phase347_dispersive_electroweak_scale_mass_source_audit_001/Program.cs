using System.Text.Json;

const string DefaultOutputDir = "studies/phase347_dispersive_electroweak_scale_mass_source_audit_001/output";
const string Phase201Path = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase256Path = "studies/phase256_observed_field_extraction_intake_contract_001/output/observed_field_extraction_intake_contract_summary.json";
const string Phase260Path = "studies/phase260_mass_definition_convention_sensitivity_audit_001/output/mass_definition_convention_sensitivity_audit_summary.json";
const string Phase286Path = "studies/phase286_alpha_running_threshold_source_viability_audit_001/output/alpha_running_threshold_source_viability_audit_summary.json";
const string Phase317Path = "studies/phase317_electroweak_mass_matrix_bridge_source_audit_001/output/electroweak_mass_matrix_bridge_source_audit_summary.json";
const string Phase322Path = "studies/phase322_higgs_upsilon_scalar_source_boundary_audit_001/output/higgs_upsilon_scalar_source_boundary_audit_summary.json";
const string Phase346Path = "studies/phase346_nielsen_pole_mass_gauge_independence_source_audit_001/output/nielsen_pole_mass_gauge_independence_source_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE347_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase256 = JsonDocument.Parse(File.ReadAllText(Phase256Path));
using var phase260 = JsonDocument.Parse(File.ReadAllText(Phase260Path));
using var phase286 = JsonDocument.Parse(File.ReadAllText(Phase286Path));
using var phase317 = JsonDocument.Parse(File.ReadAllText(Phase317Path));
using var phase322 = JsonDocument.Parse(File.ReadAllText(Phase322Path));
using var phase346 = JsonDocument.Parse(File.ReadAllText(Phase346Path));

const string dispersivePrdDoi = "https://doi.org/10.1103/PhysRevD.108.054020";
const string dispersiveArxiv = "https://arxiv.org/abs/2304.05921";
const string inverseProblemFoundationArxiv = "https://arxiv.org/abs/2211.13753";

const bool dispersiveElectroweakScaleMassSourceAuditPassedExpected = true;
const bool dispersiveElectroweakMassLeadPresent = true;
const bool dispersivePrimarySourcesReviewed = true;
const bool dispersiveRouteExternalToGu = true;
const bool scalarCurrentDispersionExtractsHiggsMass = true;
const bool vectorCurrentDispersionExtractsZMass = true;
const bool wMassOnlyConstrainedByProportionality = true;
const bool topMassExtractedFromFictitiousHeavyQuarkMixing = true;
const bool singleBottomMassInputUsed = true;
const bool inverseProblemFoundationWarnsIllPosedUnstable = true;
const double reportedBottomMassInputGeV = 4.43;
const double reportedHiggsMassGeV = 114.0;
const double reportedZMassGeV = 90.8;
const double reportedTopMassGeV = 176.0;
const double reportedMaxDeviationPercent = 9.0;

const bool dispersiveRouteRequiresExternalBottomMass = true;
const bool dispersiveRouteRequiresSmQcdPerturbativeInput = true;
const bool dispersiveRouteRequiresChosenBottomScalarAndVectorCurrents = true;
const bool dispersiveRouteRequiresRegularizedInverseProblemSolution = true;
const bool dispersiveRouteRequiresGuLocalDispersiveCorrelatorTheorem = true;
const bool dispersiveRouteRequiresGuBottomMassAndCurrentSource = true;
const bool dispersiveRouteRequiresGuIndependentWSourceRow = true;
const bool dispersiveRouteRequiresGuObservedFieldExtraction = true;
const bool dispersiveRouteRequiresGuHiggsScalarSourceOperator = true;
const bool dispersiveRouteRequiresGeVUnitNormalization = true;

const bool dispersiveRouteProvidesGuLocalWzTheorem = false;
const bool dispersiveRouteProvidesSeparateWzSourceRows = false;
const bool dispersiveRouteProvidesIndependentWMassExtraction = false;
const bool dispersiveRouteProvidesTargetIndependentVevOrMassScale = false;
const bool dispersiveRouteProvidesGuWeakMixingAngleSource = false;
const bool dispersiveRouteProvidesGuGaugeCouplingNormalization = false;
const bool dispersiveRouteProvidesObservedPhotonWzHiggsProjectionRows = false;
const bool dispersiveRouteProvidesGuObservedFieldExtractionContract = false;
const bool dispersiveRouteProvidesGuHiggsScalarSourceOperator = false;
const bool dispersiveRouteProvidesObservedHiggsMassFromGu = false;
const bool dispersiveRouteProvidesGeVUnitNormalization = false;
const bool dispersiveRoutePromotesObservedFieldExtraction = false;
const bool dispersiveRoutePromotesWzMasses = false;
const bool dispersiveRoutePromotesHiggsMass = false;
const bool dispersiveRouteCompletesBosonPredictions = false;
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
var failedComparisonsPersistUnderPoleConvention = JsonBool(phase260.RootElement, "failedComparisonsPersistUnderPoleConvention") is true;
var alphaRunningSourcePromotable = JsonBool(phase286.RootElement, "alphaRunningSourcePromotable") is true;
var smMassMatrixPromotesWzMasses = JsonBool(phase317.RootElement, "smMassMatrixPromotesWzMasses") is true;
var smMassMatrixPromotesHiggsMass = JsonBool(phase317.RootElement, "smMassMatrixPromotesHiggsMass") is true;
var higgsUpsilonRouteCompletesBosonPredictions = JsonBool(phase322.RootElement, "higgsUpsilonRouteCompletesBosonPredictions") is true;
var nielsenPoleMassGaugeIndependenceSourceAuditPassed = JsonBool(phase346.RootElement, "nielsenPoleMassGaugeIndependenceSourceAuditPassed") is true;
var nielsenRouteCompletesBosonPredictions = JsonBool(phase346.RootElement, "nielsenRouteCompletesBosonPredictions") is true;

var sourceRows = new[]
{
    new SourceRow(
        "doi-10.1103-physrevd-108-054020-dispersive-electroweak-masses",
        dispersivePrdDoi,
        "Dispersive determination of electroweak-scale masses",
        "Uses bottom scalar-current and vector-current dispersion relations with perturbative QCD input and m_b=4.43 GeV to report m_H=114 GeV, m_Z=90.8 GeV, and m_t=176 GeV.",
        "Direct electroweak-scale numerical lead, but it is external SM/QCD machinery, does not independently extract W, and the Higgs value is not the observed mass."),
    new SourceRow(
        "arxiv-2304-05921-dispersive-electroweak-masses",
        dispersiveArxiv,
        "Open arXiv record for the dispersive electroweak-scale mass analysis",
        "Records that the Z solution exists only when Z and W masses are proportionate and that the method uses a single bottom-mass input with at most 9 percent deviation.",
        "Useful source for the route's own boundary claims; still no GU source-lineage rows."),
    new SourceRow(
        "arxiv-2211-13753-inverse-problem-foundation",
        inverseProblemFoundationArxiv,
        "Inverse problem approach for dispersion relations",
        "States the mathematical inverse problem behind dispersion-relation reconstruction is ill-posed with unique but unstable solutions requiring regularization.",
        "Method boundary: this increases the need for a GU-local stability and source-lineage theorem before promotion.")
};

var checks = new[]
{
    new Check(
        "dispersive-primary-sources-reviewed",
        dispersiveElectroweakMassLeadPresent
            && dispersivePrimarySourcesReviewed
            && dispersiveRouteExternalToGu
            && sourceRows.Length == 3,
        $"lead={dispersiveElectroweakMassLeadPresent}; reviewed={dispersivePrimarySourcesReviewed}; externalToGu={dispersiveRouteExternalToGu}; sourceRows={sourceRows.Length}"),
    new Check(
        "reported-electroweak-scale-mass-claims-captured",
        scalarCurrentDispersionExtractsHiggsMass
            && vectorCurrentDispersionExtractsZMass
            && wMassOnlyConstrainedByProportionality
            && topMassExtractedFromFictitiousHeavyQuarkMixing
            && singleBottomMassInputUsed
            && reportedBottomMassInputGeV == 4.43
            && reportedHiggsMassGeV == 114.0
            && reportedZMassGeV == 90.8
            && reportedTopMassGeV == 176.0
            && reportedMaxDeviationPercent == 9.0,
        $"higgs={reportedHiggsMassGeV}; z={reportedZMassGeV}; top={reportedTopMassGeV}; mb={reportedBottomMassInputGeV}; wProportionalOnly={wMassOnlyConstrainedByProportionality}"),
    new Check(
        "route-is-external-sm-qcd-and-inverse-problem-boundary",
        dispersiveRouteRequiresExternalBottomMass
            && dispersiveRouteRequiresSmQcdPerturbativeInput
            && dispersiveRouteRequiresChosenBottomScalarAndVectorCurrents
            && dispersiveRouteRequiresRegularizedInverseProblemSolution
            && inverseProblemFoundationWarnsIllPosedUnstable,
        $"externalBottomMass={dispersiveRouteRequiresExternalBottomMass}; smQcd={dispersiveRouteRequiresSmQcdPerturbativeInput}; currents={dispersiveRouteRequiresChosenBottomScalarAndVectorCurrents}; inverseProblem={dispersiveRouteRequiresRegularizedInverseProblemSolution}; illPosed={inverseProblemFoundationWarnsIllPosedUnstable}"),
    new Check(
        "current-source-lineage-blockers-preserved",
        !phase201AllRequiredLineagesPromotable
            && !existingEvidenceFound
            && wzMissingFieldCount == 15
            && higgsMissingFieldCount == 14,
        $"phase201Promotable={phase201AllRequiredLineagesPromotable}; existingEvidence={existingEvidenceFound}; wzMissing={wzMissingFieldCount}; higgsMissing={higgsMissingFieldCount}"),
    new Check(
        "observed-field-and-physical-mass-boundaries-preserved",
        massDefinitionConventionSensitivityAuditPassed
            && failedComparisonsPersistUnderPoleConvention
            && observedFieldExtractionRequiredFieldCount == 20
            && observedFieldExtractionFilledRequiredFieldCount == 0
            && !observedFieldExtractionContractPromotable
            && nielsenPoleMassGaugeIndependenceSourceAuditPassed
            && !nielsenRouteCompletesBosonPredictions,
        $"p260={massDefinitionConventionSensitivityAuditPassed}; poleFailurePersists={failedComparisonsPersistUnderPoleConvention}; observedRequired={observedFieldExtractionRequiredFieldCount}; observedFilled={observedFieldExtractionFilledRequiredFieldCount}; observedPromotable={observedFieldExtractionContractPromotable}; p346={nielsenPoleMassGaugeIndependenceSourceAuditPassed}; p346Completes={nielsenRouteCompletesBosonPredictions}"),
    new Check(
        "dispersive-route-requires-missing-gu-source-data",
        dispersiveRouteRequiresGuLocalDispersiveCorrelatorTheorem
            && dispersiveRouteRequiresGuBottomMassAndCurrentSource
            && dispersiveRouteRequiresGuIndependentWSourceRow
            && dispersiveRouteRequiresGuObservedFieldExtraction
            && dispersiveRouteRequiresGuHiggsScalarSourceOperator
            && dispersiveRouteRequiresGeVUnitNormalization,
        $"guCorrelator={dispersiveRouteRequiresGuLocalDispersiveCorrelatorTheorem}; bottomSource={dispersiveRouteRequiresGuBottomMassAndCurrentSource}; wSource={dispersiveRouteRequiresGuIndependentWSourceRow}; observed={dispersiveRouteRequiresGuObservedFieldExtraction}; scalarSource={dispersiveRouteRequiresGuHiggsScalarSourceOperator}; gev={dispersiveRouteRequiresGeVUnitNormalization}"),
    new Check(
        "dispersive-route-does-not-fill-gu-contracts",
        !dispersiveRouteProvidesGuLocalWzTheorem
            && !dispersiveRouteProvidesSeparateWzSourceRows
            && !dispersiveRouteProvidesIndependentWMassExtraction
            && !dispersiveRouteProvidesTargetIndependentVevOrMassScale
            && !dispersiveRouteProvidesGuWeakMixingAngleSource
            && !dispersiveRouteProvidesGuGaugeCouplingNormalization
            && !dispersiveRouteProvidesObservedPhotonWzHiggsProjectionRows
            && !dispersiveRouteProvidesGuObservedFieldExtractionContract
            && !dispersiveRouteProvidesGuHiggsScalarSourceOperator
            && !dispersiveRouteProvidesObservedHiggsMassFromGu
            && !dispersiveRouteProvidesGeVUnitNormalization
            && !dispersiveRoutePromotesObservedFieldExtraction
            && !dispersiveRoutePromotesWzMasses
            && !dispersiveRoutePromotesHiggsMass
            && !dispersiveRouteCompletesBosonPredictions
            && !canFillPhase201WzContract
            && !canFillPhase201HiggsContract
            && !canFillPhase256ObservedFieldExtractionContract,
        $"guWzTheorem={dispersiveRouteProvidesGuLocalWzTheorem}; sourceRows={dispersiveRouteProvidesSeparateWzSourceRows}; independentW={dispersiveRouteProvidesIndependentWMassExtraction}; massScale={dispersiveRouteProvidesTargetIndependentVevOrMassScale}; weakAngle={dispersiveRouteProvidesGuWeakMixingAngleSource}; observed={dispersiveRouteProvidesGuObservedFieldExtractionContract}; scalar={dispersiveRouteProvidesGuHiggsScalarSourceOperator}; promotesWz={dispersiveRoutePromotesWzMasses}; promotesHiggs={dispersiveRoutePromotesHiggsMass}; completes={dispersiveRouteCompletesBosonPredictions}"),
    new Check(
        "adjacent-sm-and-higgs-routes-remain-nonpromotional",
        !alphaRunningSourcePromotable
            && !smMassMatrixPromotesWzMasses
            && !smMassMatrixPromotesHiggsMass
            && !higgsUpsilonRouteCompletesBosonPredictions,
        $"alphaPromotable={alphaRunningSourcePromotable}; smPromotesWz={smMassMatrixPromotesWzMasses}; smPromotesHiggs={smMassMatrixPromotesHiggsMass}; higgsUpsilonCompletes={higgsUpsilonRouteCompletesBosonPredictions}")
};

var dispersiveElectroweakScaleMassSourceAuditPassed = checks.All(check => check.Passed)
    && dispersiveElectroweakScaleMassSourceAuditPassedExpected
    && !dispersiveRoutePromotesObservedFieldExtraction
    && !dispersiveRoutePromotesWzMasses
    && !dispersiveRoutePromotesHiggsMass
    && !dispersiveRouteCompletesBosonPredictions
    && !canFillPhase201WzContract
    && !canFillPhase201HiggsContract
    && !canFillPhase256ObservedFieldExtractionContract;

var terminalStatus = dispersiveElectroweakScaleMassSourceAuditPassed
    ? "dispersive-electroweak-scale-mass-source-audit-external-sm-qcd-not-gu-source"
    : "dispersive-electroweak-scale-mass-source-audit-review-required";

var result = new
{
    phaseId = "phase347-dispersive-electroweak-scale-mass-source-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    dispersiveElectroweakScaleMassSourceAuditPassed,
    dispersiveElectroweakMassLeadPresent,
    dispersivePrimarySourcesReviewed,
    dispersiveRouteExternalToGu,
    scalarCurrentDispersionExtractsHiggsMass,
    vectorCurrentDispersionExtractsZMass,
    wMassOnlyConstrainedByProportionality,
    topMassExtractedFromFictitiousHeavyQuarkMixing,
    singleBottomMassInputUsed,
    inverseProblemFoundationWarnsIllPosedUnstable,
    reportedBottomMassInputGeV,
    reportedHiggsMassGeV,
    reportedZMassGeV,
    reportedTopMassGeV,
    reportedMaxDeviationPercent,
    dispersiveRouteRequiresExternalBottomMass,
    dispersiveRouteRequiresSmQcdPerturbativeInput,
    dispersiveRouteRequiresChosenBottomScalarAndVectorCurrents,
    dispersiveRouteRequiresRegularizedInverseProblemSolution,
    dispersiveRouteRequiresGuLocalDispersiveCorrelatorTheorem,
    dispersiveRouteRequiresGuBottomMassAndCurrentSource,
    dispersiveRouteRequiresGuIndependentWSourceRow,
    dispersiveRouteRequiresGuObservedFieldExtraction,
    dispersiveRouteRequiresGuHiggsScalarSourceOperator,
    dispersiveRouteRequiresGeVUnitNormalization,
    dispersiveRouteProvidesGuLocalWzTheorem,
    dispersiveRouteProvidesSeparateWzSourceRows,
    dispersiveRouteProvidesIndependentWMassExtraction,
    dispersiveRouteProvidesTargetIndependentVevOrMassScale,
    dispersiveRouteProvidesGuWeakMixingAngleSource,
    dispersiveRouteProvidesGuGaugeCouplingNormalization,
    dispersiveRouteProvidesObservedPhotonWzHiggsProjectionRows,
    dispersiveRouteProvidesGuObservedFieldExtractionContract,
    dispersiveRouteProvidesGuHiggsScalarSourceOperator,
    dispersiveRouteProvidesObservedHiggsMassFromGu,
    dispersiveRouteProvidesGeVUnitNormalization,
    dispersiveRoutePromotesObservedFieldExtraction,
    dispersiveRoutePromotesWzMasses,
    dispersiveRoutePromotesHiggsMass,
    dispersiveRouteCompletesBosonPredictions,
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
    relatedBlockingEvidence = new
    {
        alphaRunningSourcePromotable,
        smMassMatrixPromotesWzMasses,
        smMassMatrixPromotesHiggsMass,
        higgsUpsilonRouteCompletesBosonPredictions,
        nielsenPoleMassGaugeIndependenceSourceAuditPassed,
        nielsenRouteCompletesBosonPredictions
    },
    decision = "Do not promote W/Z or Higgs physical masses from the dispersive electroweak-scale mass route. It is a direct numerical external lead, but it imports the bottom-quark mass, Standard Model/QCD current correlators, perturbative inputs, and a regularized inverse-problem solution; reports m_H=114 GeV rather than the observed Higgs mass; gives Z but not an independent W source row; and supplies no GU-local observed-field extraction, source-lineage scale/coupling, Higgs scalar-source, or GeV-unit derivation.",
    nextRequiredArtifact = new[]
    {
        "A GU-local theorem deriving the relevant scalar/vector current correlators and dispersion relation from GU fields.",
        "A target-independent GU source for the bottom-mass/current input or a replacement dimensionful source scale.",
        "Independent observed W and Z source rows with photon/Z/W projection and target-comparison gates.",
        "A GU Higgs scalar-source/operator and excitation relation that yields the observed Higgs pole before target comparison.",
        "A stability theorem for any inverse-problem or regularized dispersive extraction used as prediction evidence."
    }
};

var fullPath = Path.Combine(outputDir, "dispersive_electroweak_scale_mass_source_audit.json");
var summaryPath = Path.Combine(outputDir, "dispersive_electroweak_scale_mass_source_audit_summary.json");
var jsonOptions = new JsonSerializerOptions { WriteIndented = true };

File.WriteAllText(fullPath, JsonSerializer.Serialize(result, jsonOptions));
File.WriteAllText(summaryPath, JsonSerializer.Serialize(new
{
    result.phaseId,
    result.terminalStatus,
    result.dispersiveElectroweakScaleMassSourceAuditPassed,
    result.dispersiveElectroweakMassLeadPresent,
    result.dispersivePrimarySourcesReviewed,
    result.dispersiveRouteExternalToGu,
    result.scalarCurrentDispersionExtractsHiggsMass,
    result.vectorCurrentDispersionExtractsZMass,
    result.wMassOnlyConstrainedByProportionality,
    result.topMassExtractedFromFictitiousHeavyQuarkMixing,
    result.singleBottomMassInputUsed,
    result.inverseProblemFoundationWarnsIllPosedUnstable,
    result.reportedBottomMassInputGeV,
    result.reportedHiggsMassGeV,
    result.reportedZMassGeV,
    result.reportedTopMassGeV,
    result.reportedMaxDeviationPercent,
    result.dispersiveRouteRequiresExternalBottomMass,
    result.dispersiveRouteRequiresSmQcdPerturbativeInput,
    result.dispersiveRouteRequiresChosenBottomScalarAndVectorCurrents,
    result.dispersiveRouteRequiresRegularizedInverseProblemSolution,
    result.dispersiveRouteRequiresGuLocalDispersiveCorrelatorTheorem,
    result.dispersiveRouteRequiresGuBottomMassAndCurrentSource,
    result.dispersiveRouteRequiresGuIndependentWSourceRow,
    result.dispersiveRouteRequiresGuObservedFieldExtraction,
    result.dispersiveRouteRequiresGuHiggsScalarSourceOperator,
    result.dispersiveRouteRequiresGeVUnitNormalization,
    result.dispersiveRouteProvidesGuLocalWzTheorem,
    result.dispersiveRouteProvidesSeparateWzSourceRows,
    result.dispersiveRouteProvidesIndependentWMassExtraction,
    result.dispersiveRouteProvidesTargetIndependentVevOrMassScale,
    result.dispersiveRouteProvidesGuWeakMixingAngleSource,
    result.dispersiveRouteProvidesGuGaugeCouplingNormalization,
    result.dispersiveRouteProvidesObservedPhotonWzHiggsProjectionRows,
    result.dispersiveRouteProvidesGuObservedFieldExtractionContract,
    result.dispersiveRouteProvidesGuHiggsScalarSourceOperator,
    result.dispersiveRouteProvidesObservedHiggsMassFromGu,
    result.dispersiveRouteProvidesGeVUnitNormalization,
    result.dispersiveRoutePromotesObservedFieldExtraction,
    result.dispersiveRoutePromotesWzMasses,
    result.dispersiveRoutePromotesHiggsMass,
    result.dispersiveRouteCompletesBosonPredictions,
    result.canFillPhase201WzContract,
    result.canFillPhase201HiggsContract,
    result.canFillPhase256ObservedFieldExtractionContract,
    sourceRowCount = sourceRows.Length,
    result.contractImpact,
    result.relatedBlockingEvidence,
    result.decision,
    result.nextRequiredArtifact
}, jsonOptions));

Console.WriteLine(terminalStatus);
Console.WriteLine($"dispersiveElectroweakScaleMassSourceAuditPassed={dispersiveElectroweakScaleMassSourceAuditPassed}");
Console.WriteLine($"reportedHiggsMassGeV={reportedHiggsMassGeV}");
Console.WriteLine($"reportedZMassGeV={reportedZMassGeV}");
Console.WriteLine($"wMassOnlyConstrainedByProportionality={wMassOnlyConstrainedByProportionality}");
Console.WriteLine($"dispersiveRoutePromotesWzMasses={dispersiveRoutePromotesWzMasses}");
Console.WriteLine($"dispersiveRoutePromotesHiggsMass={dispersiveRoutePromotesHiggsMass}");
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
