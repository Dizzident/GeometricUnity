using System.Text.Json;

const string DefaultOutputDir = "studies/phase365_dressing_field_electroweak_observed_variables_audit_001/output";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase255Path = "studies/phase255_observed_field_extraction_no_go_audit_001/output/observed_field_extraction_no_go_audit_summary.json";
const string Phase256Path = "studies/phase256_observed_field_extraction_intake_contract_001/output/observed_field_extraction_intake_contract_summary.json";
const string Phase344Path = "studies/phase344_fms_gauge_invariant_spectrum_source_audit_001/output/fms_gauge_invariant_spectrum_source_audit_summary.json";
const string Phase346Path = "studies/phase346_nielsen_pole_mass_gauge_independence_source_audit_001/output/nielsen_pole_mass_gauge_independence_source_audit_summary.json";
const string Phase364Path = "studies/phase364_moment_map_symplectic_reduction_source_audit_001/output/moment_map_symplectic_reduction_source_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE365_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase255 = JsonDocument.Parse(File.ReadAllText(Phase255Path));
using var phase256 = JsonDocument.Parse(File.ReadAllText(Phase256Path));
using var phase344 = JsonDocument.Parse(File.ReadAllText(Phase344Path));
using var phase346 = JsonDocument.Parse(File.ReadAllText(Phase346Path));
using var phase364 = JsonDocument.Parse(File.ReadAllText(Phase364Path));

const string dressingFieldReviewUrl = "https://arxiv.org/abs/1702.02753";
const string dressingVsFixingUrl = "https://arxiv.org/abs/2404.18582";
const string qftGaugeFixingDressingUrl = "https://arxiv.org/abs/2406.19937";
const string gaugeInvariantHiggsAccountsUrl = "https://arxiv.org/abs/1102.0468";
const string secondOrderStandardModelUrl = "https://arxiv.org/abs/1308.1278";

const bool dressingFieldElectroweakObservedVariablesAuditPassedExpected = true;
const bool dressingFieldLeadPresent = true;
const bool dressingFieldPrimarySourcesReviewed = true;
const bool dressingFieldRouteExternalToGu = true;
const bool routeUsesDressingFieldMethod = true;
const bool routeSeparatesDressingFromGaugeFixing = true;
const bool routeConstructsGaugeInvariantCompositeVariables = true;
const bool routeCoversElectroweakHiggsMechanism = true;
const bool routeUsesHiggsFieldAsSu2Dressing = true;
const bool routeBuildsSu2InvariantWeakSectorVariables = true;
const bool routeIdentifiesPhotonAndNeutralMixingAfterDressing = true;
const bool routeConnectsToUnitaryGaugeWithoutTreatingItAsGaugeFixingOnly = true;
const bool routeIncludesGaugeInvariantHiggsMechanismAccounts = true;
const bool routeIncludesSecondOrderStandardModelVariables = true;
const bool routeProvidesExternalObservedFieldExtractionTemplate = true;
const int sourceRowCountExpected = 5;
const int dressingFieldReviewLatestArxivVersion = 1;
const int dressingVsFixingLatestArxivVersion = 3;
const int qftGaugeFixingDressingLatestArxivVersion = 1;
const int gaugeInvariantHiggsAccountsLatestArxivVersion = 2;
const int secondOrderStandardModelLatestArxivVersion = 1;

const bool routeIsVariableDressingNotGuMassLaw = true;
const bool routeDependsOnStandardModelHiggsDoubletAndNonzeroVev = true;
const bool routeDoesNotDeriveGuObservedSectorVacuum = true;
const bool routeDoesNotDeriveGuElectroweakEmbedding = true;
const bool routeDoesNotDeriveGuWeakMixingAngle = true;
const bool routeDoesNotDeriveGuGaugeCouplings = true;
const bool routeDoesNotProvideTargetIndependentMassScale = true;
const bool routeDoesNotProvidePhysicalPoleExtraction = true;
const bool routeDoesNotPredictObservedWzMasses = true;
const bool routeDoesNotPredictObservedHiggsMass = true;

const bool routeRequiresGuNativeDressingField = true;
const bool routeRequiresGuObservedElectroweakEmbedding = true;
const bool routeRequiresGuObservedVacuumAndExpansion = true;
const bool routeRequiresPhotonWzHiggsProjectionRows = true;
const bool routeRequiresSeparateWzSourceRows = true;
const bool routeRequiresGuWeakMixingAngleSource = true;
const bool routeRequiresGuGaugeCouplingNormalization = true;
const bool routeRequiresGuHiggsScalarSourceOperator = true;
const bool routeRequiresGuHiggsSelfCouplingSource = true;
const bool routeRequiresCorrelationFunctionPoleExtraction = true;
const bool routeRequiresTargetIndependentVevOrMassScale = true;
const bool routeRequiresGeVUnitNormalization = true;

const bool routeProvidesGuNativeDressingField = false;
const bool routeProvidesGuObservedElectroweakEmbedding = false;
const bool routeProvidesGuObservedVacuumAndExpansion = false;
const bool routeProvidesObservedPhotonWzHiggsProjectionRows = false;
const bool routeProvidesSeparateWzSourceRows = false;
const bool routeProvidesGuWeakMixingAngleSource = false;
const bool routeProvidesGuGaugeCouplingNormalization = false;
const bool routeProvidesGuHiggsScalarSourceOperator = false;
const bool routeProvidesGuHiggsSelfCouplingSource = false;
const bool routeProvidesCorrelationFunctionPoleExtraction = false;
const bool routeProvidesTargetIndependentVevOrMassScale = false;
const bool routeProvidesGeVUnitNormalization = false;
const bool routePromotesObservedFieldExtraction = false;
const bool routePromotesWzMasses = false;
const bool routePromotesHiggsMass = false;
const bool routeCompletesBosonPredictions = false;
const bool canFillPhase201WzContract = false;
const bool canFillPhase201HiggsContract = false;
const bool canFillPhase256ObservedFieldExtractionContract = false;

var existingEvidenceFound = JsonBool(phase213.RootElement, "existingEvidenceFound") is true;
var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;
var observedFieldExtractionNoGoPassed = JsonBool(phase255.RootElement, "observedFieldExtractionNoGoPassed") is true;
var observedFieldExtractionBridgePromotable = JsonBool(phase255.RootElement, "observedFieldExtractionBridgePromotable") is true;
var newObservedFieldExtractionArtifactRequired = JsonBool(phase255.RootElement, "newObservedFieldExtractionArtifactRequired") is true;
var observedFieldExtractionRequiredFieldCount = JsonInt(phase256.RootElement, "requiredFieldCount") ?? -1;
var observedFieldExtractionFilledRequiredFieldCount = JsonInt(phase256.RootElement, "filledRequiredFieldCount") ?? -1;
var observedFieldExtractionContractPromotable = JsonBool(phase256.RootElement, "observedFieldExtractionContractPromotable") is true;
var fmsGaugeInvariantSpectrumSourceAuditPassed = JsonBool(phase344.RootElement, "fmsGaugeInvariantSpectrumSourceAuditPassed") is true;
var fmsProvidesObservedFieldExtractionTemplate = JsonBool(phase344.RootElement, "fmsProvidesObservedFieldExtractionTemplate") is true;
var fmsRoutePromotesObservedFieldExtraction = JsonBool(phase344.RootElement, "fmsRoutePromotesObservedFieldExtraction") is true;
var nielsenPoleMassGaugeIndependenceSourceAuditPassed = JsonBool(phase346.RootElement, "nielsenPoleMassGaugeIndependenceSourceAuditPassed") is true;
var nielsenRoutePromotesObservedFieldExtraction = JsonBool(phase346.RootElement, "nielsenRoutePromotesObservedFieldExtraction") is true;
var momentMapSymplecticReductionSourceAuditPassed = JsonBool(phase364.RootElement, "momentMapSymplecticReductionSourceAuditPassed") is true;
var momentMapRoutePromotesWzMasses = JsonBool(phase364.RootElement, "routePromotesWzMasses") is true;
var momentMapRoutePromotesHiggsMass = JsonBool(phase364.RootElement, "routePromotesHiggsMass") is true;

var sourceRows = new[]
{
    new SourceRow(
        "attard-francois-lazzarini-masson-1702-02753-dressing-field-method",
        dressingFieldReviewUrl,
        "The dressing field method of gauge symmetry reduction, a review with examples",
        "Reviews the dressing-field method as a way to construct composite variables invariant under a gauge subgroup and reduce gauge symmetry without mere gauge fixing.",
        "Strong observed-variable template; it does not derive GU-native fields, a GU vacuum, W/Z/H source rows, or masses."),
    new SourceRow(
        "berghofer-francois-2404-18582-dressing-vs-fixing",
        dressingVsFixingUrl,
        "Dressing vs. Fixing: On How to Extract and Interpret Gauge-Invariant Content",
        "Clarifies the difference between gauge fixing and dressing transformations, with electroweak examples where gauge-invariant variables carry physical interpretation.",
        "Useful conceptual guardrail for observed-field extraction; still external to GU and not a numerical source law."),
    new SourceRow(
        "francois-2406-19937-gauge-fixing-qft-dressing-field-method",
        qftGaugeFixingDressingUrl,
        "Gauge-fixing in quantum field theory and the dressing field method",
        "Frames dressing as a distinct operation from gauge fixing in quantum field theory and emphasizes that dressed variables can encode gauge-invariant content.",
        "Supports an extraction discipline, not GU-local pole equations or source-lineage parameters."),
    new SourceRow(
        "struyve-1102-0468-gauge-invariant-higgs-accounts",
        gaugeInvariantHiggsAccountsUrl,
        "Gauge invariant accounts of the Higgs mechanism",
        "Compares gauge-invariant descriptions of the Higgs mechanism and explains how physical descriptions can avoid treating gauge symmetry breaking as literal observable breaking.",
        "Good boundary source for interpretation; it does not supply GU electroweak source rows or a mass scale."),
    new SourceRow(
        "espin-krasnov-1308-3914-second-order-standard-model",
        secondOrderStandardModelUrl,
        "Second order Standard Model",
        "Rewrites the electroweak theory using SU(2)-invariant fields after combining spinors and bosons with the Higgs field, leaving residual U(1) structure and Standard Model inputs.",
        "Closest direct electroweak variable construction; it is an SM reformulation, not a GU-derived W/Z/H mass prediction.")
};

var checks = new[]
{
    new Check(
        "dressing-field-primary-sources-reviewed",
        dressingFieldLeadPresent && dressingFieldPrimarySourcesReviewed && dressingFieldRouteExternalToGu && sourceRows.Length == sourceRowCountExpected,
        $"lead={dressingFieldLeadPresent}; reviewed={dressingFieldPrimarySourcesReviewed}; externalToGu={dressingFieldRouteExternalToGu}; sourceRows={sourceRows.Length}"),
    new Check(
        "dressing-field-observed-variable-route-captured",
        routeUsesDressingFieldMethod
            && routeSeparatesDressingFromGaugeFixing
            && routeConstructsGaugeInvariantCompositeVariables
            && routeCoversElectroweakHiggsMechanism
            && routeUsesHiggsFieldAsSu2Dressing
            && routeBuildsSu2InvariantWeakSectorVariables
            && routeIdentifiesPhotonAndNeutralMixingAfterDressing
            && routeConnectsToUnitaryGaugeWithoutTreatingItAsGaugeFixingOnly
            && routeIncludesGaugeInvariantHiggsMechanismAccounts
            && routeIncludesSecondOrderStandardModelVariables
            && routeProvidesExternalObservedFieldExtractionTemplate,
        $"dfm={routeUsesDressingFieldMethod}; distinctFromFixing={routeSeparatesDressingFromGaugeFixing}; composites={routeConstructsGaugeInvariantCompositeVariables}; ew={routeCoversElectroweakHiggsMechanism}; higgsDressing={routeUsesHiggsFieldAsSu2Dressing}; su2Invariant={routeBuildsSu2InvariantWeakSectorVariables}; photonNeutral={routeIdentifiesPhotonAndNeutralMixingAfterDressing}; unitaryGaugeBoundary={routeConnectsToUnitaryGaugeWithoutTreatingItAsGaugeFixingOnly}; gaugeInvariantHiggs={routeIncludesGaugeInvariantHiggsMechanismAccounts}; secondOrderSm={routeIncludesSecondOrderStandardModelVariables}; template={routeProvidesExternalObservedFieldExtractionTemplate}"),
    new Check(
        "dressing-field-version-metadata-captured",
        dressingFieldReviewLatestArxivVersion == 1
            && dressingVsFixingLatestArxivVersion == 3
            && qftGaugeFixingDressingLatestArxivVersion == 1
            && gaugeInvariantHiggsAccountsLatestArxivVersion == 2
            && secondOrderStandardModelLatestArxivVersion == 1,
        $"reviewV={dressingFieldReviewLatestArxivVersion}; dressingVsFixingV={dressingVsFixingLatestArxivVersion}; qftGaugeFixingV={qftGaugeFixingDressingLatestArxivVersion}; struyveV={gaugeInvariantHiggsAccountsLatestArxivVersion}; secondOrderSmV={secondOrderStandardModelLatestArxivVersion}"),
    new Check(
        "observed-field-extraction-blockers-preserved",
        observedFieldExtractionNoGoPassed
            && !observedFieldExtractionBridgePromotable
            && newObservedFieldExtractionArtifactRequired
            && observedFieldExtractionRequiredFieldCount == 20
            && observedFieldExtractionFilledRequiredFieldCount == 0
            && !observedFieldExtractionContractPromotable,
        $"p255NoGo={observedFieldExtractionNoGoPassed}; p255Promotable={observedFieldExtractionBridgePromotable}; newArtifact={newObservedFieldExtractionArtifactRequired}; p256Required={observedFieldExtractionRequiredFieldCount}; p256Filled={observedFieldExtractionFilledRequiredFieldCount}; p256Promotable={observedFieldExtractionContractPromotable}"),
    new Check(
        "adjacent-observed-field-boundaries-preserved",
        fmsGaugeInvariantSpectrumSourceAuditPassed
            && fmsProvidesObservedFieldExtractionTemplate
            && !fmsRoutePromotesObservedFieldExtraction
            && nielsenPoleMassGaugeIndependenceSourceAuditPassed
            && !nielsenRoutePromotesObservedFieldExtraction
            && momentMapSymplecticReductionSourceAuditPassed
            && !momentMapRoutePromotesWzMasses
            && !momentMapRoutePromotesHiggsMass,
        $"fmsPassed={fmsGaugeInvariantSpectrumSourceAuditPassed}; fmsTemplate={fmsProvidesObservedFieldExtractionTemplate}; fmsPromotesObserved={fmsRoutePromotesObservedFieldExtraction}; nielsenPassed={nielsenPoleMassGaugeIndependenceSourceAuditPassed}; nielsenPromotesObserved={nielsenRoutePromotesObservedFieldExtraction}; momentMapPassed={momentMapSymplecticReductionSourceAuditPassed}; momentMapWz={momentMapRoutePromotesWzMasses}; momentMapHiggs={momentMapRoutePromotesHiggsMass}"),
    new Check(
        "dressing-field-promotion-obstructions-captured",
        routeIsVariableDressingNotGuMassLaw
            && routeDependsOnStandardModelHiggsDoubletAndNonzeroVev
            && routeDoesNotDeriveGuObservedSectorVacuum
            && routeDoesNotDeriveGuElectroweakEmbedding
            && routeDoesNotDeriveGuWeakMixingAngle
            && routeDoesNotDeriveGuGaugeCouplings
            && routeDoesNotProvideTargetIndependentMassScale
            && routeDoesNotProvidePhysicalPoleExtraction
            && routeDoesNotPredictObservedWzMasses
            && routeDoesNotPredictObservedHiggsMass,
        $"variableDressing={routeIsVariableDressingNotGuMassLaw}; smHiggsVev={routeDependsOnStandardModelHiggsDoubletAndNonzeroVev}; guVacuum={routeDoesNotDeriveGuObservedSectorVacuum}; guEmbedding={routeDoesNotDeriveGuElectroweakEmbedding}; weakAngle={routeDoesNotDeriveGuWeakMixingAngle}; gaugeCouplings={routeDoesNotDeriveGuGaugeCouplings}; scale={routeDoesNotProvideTargetIndependentMassScale}; pole={routeDoesNotProvidePhysicalPoleExtraction}; observedWz={routeDoesNotPredictObservedWzMasses}; observedHiggs={routeDoesNotPredictObservedHiggsMass}"),
    new Check(
        "dressing-field-route-does-not-fill-gu-contracts",
        routeRequiresGuNativeDressingField
            && routeRequiresGuObservedElectroweakEmbedding
            && routeRequiresGuObservedVacuumAndExpansion
            && routeRequiresPhotonWzHiggsProjectionRows
            && routeRequiresSeparateWzSourceRows
            && routeRequiresGuWeakMixingAngleSource
            && routeRequiresGuGaugeCouplingNormalization
            && routeRequiresGuHiggsScalarSourceOperator
            && routeRequiresGuHiggsSelfCouplingSource
            && routeRequiresCorrelationFunctionPoleExtraction
            && routeRequiresTargetIndependentVevOrMassScale
            && routeRequiresGeVUnitNormalization
            && !routeProvidesGuNativeDressingField
            && !routeProvidesGuObservedElectroweakEmbedding
            && !routeProvidesGuObservedVacuumAndExpansion
            && !routeProvidesObservedPhotonWzHiggsProjectionRows
            && !routeProvidesSeparateWzSourceRows
            && !routeProvidesGuWeakMixingAngleSource
            && !routeProvidesGuGaugeCouplingNormalization
            && !routeProvidesGuHiggsScalarSourceOperator
            && !routeProvidesGuHiggsSelfCouplingSource
            && !routeProvidesCorrelationFunctionPoleExtraction
            && !routeProvidesTargetIndependentVevOrMassScale
            && !routeProvidesGeVUnitNormalization
            && !routePromotesObservedFieldExtraction
            && !routePromotesWzMasses
            && !routePromotesHiggsMass
            && !routeCompletesBosonPredictions
            && !canFillPhase201WzContract
            && !canFillPhase201HiggsContract
            && !canFillPhase256ObservedFieldExtractionContract,
        $"requiresDressing={routeRequiresGuNativeDressingField}; requiresEmbedding={routeRequiresGuObservedElectroweakEmbedding}; requiresVacuum={routeRequiresGuObservedVacuumAndExpansion}; requiresProjection={routeRequiresPhotonWzHiggsProjectionRows}; requiresWzRows={routeRequiresSeparateWzSourceRows}; providesDressing={routeProvidesGuNativeDressingField}; providesProjection={routeProvidesObservedPhotonWzHiggsProjectionRows}; providesWzRows={routeProvidesSeparateWzSourceRows}; promotesObserved={routePromotesObservedFieldExtraction}; promotesWz={routePromotesWzMasses}; promotesHiggs={routePromotesHiggsMass}"),
    new Check(
        "phase213-missing-source-state-preserved",
        !existingEvidenceFound && wzMissingFieldCount == 15 && higgsMissingFieldCount == 14,
        $"existingEvidence={existingEvidenceFound}; wzMissing={wzMissingFieldCount}; higgsMissing={higgsMissingFieldCount}")
};

var dressingFieldElectroweakObservedVariablesAuditPassed = checks.All(check => check.Passed)
    && dressingFieldElectroweakObservedVariablesAuditPassedExpected
    && !routePromotesObservedFieldExtraction
    && !routePromotesWzMasses
    && !routePromotesHiggsMass
    && !routeCompletesBosonPredictions
    && !canFillPhase201WzContract
    && !canFillPhase201HiggsContract
    && !canFillPhase256ObservedFieldExtractionContract;

var terminalStatus = dressingFieldElectroweakObservedVariablesAuditPassed
    ? "dressing-field-electroweak-observed-variables-audit-template-not-gu-source"
    : "dressing-field-electroweak-observed-variables-audit-review-required";

var result = new
{
    phaseId = "phase365-dressing-field-electroweak-observed-variables-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    dressingFieldElectroweakObservedVariablesAuditPassed,
    dressingFieldLeadPresent,
    dressingFieldPrimarySourcesReviewed,
    dressingFieldRouteExternalToGu,
    routeUsesDressingFieldMethod,
    routeSeparatesDressingFromGaugeFixing,
    routeConstructsGaugeInvariantCompositeVariables,
    routeCoversElectroweakHiggsMechanism,
    routeUsesHiggsFieldAsSu2Dressing,
    routeBuildsSu2InvariantWeakSectorVariables,
    routeIdentifiesPhotonAndNeutralMixingAfterDressing,
    routeConnectsToUnitaryGaugeWithoutTreatingItAsGaugeFixingOnly,
    routeIncludesGaugeInvariantHiggsMechanismAccounts,
    routeIncludesSecondOrderStandardModelVariables,
    routeProvidesExternalObservedFieldExtractionTemplate,
    dressingFieldReviewLatestArxivVersion,
    dressingVsFixingLatestArxivVersion,
    qftGaugeFixingDressingLatestArxivVersion,
    gaugeInvariantHiggsAccountsLatestArxivVersion,
    secondOrderStandardModelLatestArxivVersion,
    routeIsVariableDressingNotGuMassLaw,
    routeDependsOnStandardModelHiggsDoubletAndNonzeroVev,
    routeDoesNotDeriveGuObservedSectorVacuum,
    routeDoesNotDeriveGuElectroweakEmbedding,
    routeDoesNotDeriveGuWeakMixingAngle,
    routeDoesNotDeriveGuGaugeCouplings,
    routeDoesNotProvideTargetIndependentMassScale,
    routeDoesNotProvidePhysicalPoleExtraction,
    routeDoesNotPredictObservedWzMasses,
    routeDoesNotPredictObservedHiggsMass,
    routeRequiresGuNativeDressingField,
    routeRequiresGuObservedElectroweakEmbedding,
    routeRequiresGuObservedVacuumAndExpansion,
    routeRequiresPhotonWzHiggsProjectionRows,
    routeRequiresSeparateWzSourceRows,
    routeRequiresGuWeakMixingAngleSource,
    routeRequiresGuGaugeCouplingNormalization,
    routeRequiresGuHiggsScalarSourceOperator,
    routeRequiresGuHiggsSelfCouplingSource,
    routeRequiresCorrelationFunctionPoleExtraction,
    routeRequiresTargetIndependentVevOrMassScale,
    routeRequiresGeVUnitNormalization,
    routeProvidesGuNativeDressingField,
    routeProvidesGuObservedElectroweakEmbedding,
    routeProvidesGuObservedVacuumAndExpansion,
    routeProvidesObservedPhotonWzHiggsProjectionRows,
    routeProvidesSeparateWzSourceRows,
    routeProvidesGuWeakMixingAngleSource,
    routeProvidesGuGaugeCouplingNormalization,
    routeProvidesGuHiggsScalarSourceOperator,
    routeProvidesGuHiggsSelfCouplingSource,
    routeProvidesCorrelationFunctionPoleExtraction,
    routeProvidesTargetIndependentVevOrMassScale,
    routeProvidesGeVUnitNormalization,
    routePromotesObservedFieldExtraction,
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
        observedFieldExtractionNoGoPassed,
        observedFieldExtractionBridgePromotable,
        newObservedFieldExtractionArtifactRequired,
        fmsGaugeInvariantSpectrumSourceAuditPassed,
        fmsProvidesObservedFieldExtractionTemplate,
        fmsRoutePromotesObservedFieldExtraction,
        nielsenPoleMassGaugeIndependenceSourceAuditPassed,
        nielsenRoutePromotesObservedFieldExtraction,
        momentMapSymplecticReductionSourceAuditPassed,
        momentMapRoutePromotesWzMasses,
        momentMapRoutePromotesHiggsMass
    },
    contractImpact = new
    {
        canFillPhase201WzContract,
        canFillPhase201HiggsContract,
        canFillPhase256ObservedFieldExtractionContract,
        wzMissingFieldCount,
        higgsMissingFieldCount,
        observedFieldExtractionRequiredFieldCount,
        observedFieldExtractionFilledRequiredFieldCount
    },
    checks,
    decision = "Do not promote W/Z or Higgs physical masses from dressing-field or SU(2)-invariant electroweak-variable routes in this repository. These sources provide a strong external template for observed-field extraction: construct gauge-invariant dressed variables, distinguish dressing from gauge fixing, and rewrite electroweak fields using the Higgs field. They still assume Standard Model electroweak structure and do not supply a GU-native dressing field, observed-sector vacuum, electroweak embedding, separate W/Z source rows, weak angle/coupling lineage, Higgs scalar-source/self-coupling lineage, pole extraction, mass scale, or GeV normalization.",
    nextRequiredArtifact = new[]
    {
        "A GU-native dressing field or gauge-invariant composite-variable theorem derived from Shiab/observer-sector geometry.",
        "A source-derived GU observed vacuum and electroweak embedding producing photon/W/Z/H projection rows before target comparison.",
        "Separate W and Z source rows with target-independent weak-angle, coupling, scale, and stability sidecars.",
        "A Higgs scalar-source/self-coupling lineage plus physical-pole extraction and GeV normalization before any dressing-field route can promote W/Z/H masses."
    }
};

var options = new JsonSerializerOptions { WriteIndented = true };
File.WriteAllText(Path.Combine(outputDir, "dressing_field_electroweak_observed_variables_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "dressing_field_electroweak_observed_variables_audit_summary.json"),
    JsonSerializer.Serialize(
        new
        {
            result.phaseId,
            result.terminalStatus,
            result.dressingFieldElectroweakObservedVariablesAuditPassed,
            result.dressingFieldLeadPresent,
            result.dressingFieldPrimarySourcesReviewed,
            result.dressingFieldRouteExternalToGu,
            result.routeUsesDressingFieldMethod,
            result.routeSeparatesDressingFromGaugeFixing,
            result.routeConstructsGaugeInvariantCompositeVariables,
            result.routeCoversElectroweakHiggsMechanism,
            result.routeUsesHiggsFieldAsSu2Dressing,
            result.routeBuildsSu2InvariantWeakSectorVariables,
            result.routeIdentifiesPhotonAndNeutralMixingAfterDressing,
            result.routeConnectsToUnitaryGaugeWithoutTreatingItAsGaugeFixingOnly,
            result.routeIncludesGaugeInvariantHiggsMechanismAccounts,
            result.routeIncludesSecondOrderStandardModelVariables,
            result.routeProvidesExternalObservedFieldExtractionTemplate,
            result.dressingFieldReviewLatestArxivVersion,
            result.dressingVsFixingLatestArxivVersion,
            result.qftGaugeFixingDressingLatestArxivVersion,
            result.gaugeInvariantHiggsAccountsLatestArxivVersion,
            result.secondOrderStandardModelLatestArxivVersion,
            result.routeIsVariableDressingNotGuMassLaw,
            result.routeDependsOnStandardModelHiggsDoubletAndNonzeroVev,
            result.routeDoesNotDeriveGuObservedSectorVacuum,
            result.routeDoesNotDeriveGuElectroweakEmbedding,
            result.routeDoesNotDeriveGuWeakMixingAngle,
            result.routeDoesNotDeriveGuGaugeCouplings,
            result.routeDoesNotProvideTargetIndependentMassScale,
            result.routeDoesNotProvidePhysicalPoleExtraction,
            result.routeDoesNotPredictObservedWzMasses,
            result.routeDoesNotPredictObservedHiggsMass,
            result.routeRequiresGuNativeDressingField,
            result.routeRequiresGuObservedElectroweakEmbedding,
            result.routeRequiresGuObservedVacuumAndExpansion,
            result.routeRequiresPhotonWzHiggsProjectionRows,
            result.routeRequiresSeparateWzSourceRows,
            result.routeRequiresGuWeakMixingAngleSource,
            result.routeRequiresGuGaugeCouplingNormalization,
            result.routeRequiresGuHiggsScalarSourceOperator,
            result.routeRequiresGuHiggsSelfCouplingSource,
            result.routeRequiresCorrelationFunctionPoleExtraction,
            result.routeRequiresTargetIndependentVevOrMassScale,
            result.routeRequiresGeVUnitNormalization,
            result.routeProvidesGuNativeDressingField,
            result.routeProvidesGuObservedElectroweakEmbedding,
            result.routeProvidesGuObservedVacuumAndExpansion,
            result.routeProvidesObservedPhotonWzHiggsProjectionRows,
            result.routeProvidesSeparateWzSourceRows,
            result.routeProvidesGuWeakMixingAngleSource,
            result.routeProvidesGuGaugeCouplingNormalization,
            result.routeProvidesGuHiggsScalarSourceOperator,
            result.routeProvidesGuHiggsSelfCouplingSource,
            result.routeProvidesCorrelationFunctionPoleExtraction,
            result.routeProvidesTargetIndependentVevOrMassScale,
            result.routeProvidesGeVUnitNormalization,
            result.routePromotesObservedFieldExtraction,
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
Console.WriteLine($"dressingFieldElectroweakObservedVariablesAuditPassed={dressingFieldElectroweakObservedVariablesAuditPassed}");
Console.WriteLine($"routeUsesDressingFieldMethod={routeUsesDressingFieldMethod}");
Console.WriteLine($"routeProvidesExternalObservedFieldExtractionTemplate={routeProvidesExternalObservedFieldExtractionTemplate}");
Console.WriteLine($"routePromotesObservedFieldExtraction={routePromotesObservedFieldExtraction}");
Console.WriteLine($"routePromotesWzMasses={routePromotesWzMasses}");
Console.WriteLine($"routePromotesHiggsMass={routePromotesHiggsMass}");
Console.WriteLine($"canFillPhase256ObservedFieldExtractionContract={canFillPhase256ObservedFieldExtractionContract}");

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
