using System.Text.Json;

const string DefaultOutputDir = "studies/phase319_legacy_selector_spectrum_source_law_audit_001/output";
const string Phase42StudyPath = "studies/phase42_bundle_backed_wz_source_spectra_001/STUDY.md";
const string Phase43StudyPath = "studies/phase43_selector_eigen_wz_source_spectra_001/STUDY.md";
const string Phase73Path = "studies/phase73_wz_absolute_mass_projection_001/wz_absolute_mass_projection.json";
const string Phase74Path = "studies/phase74_wz_absolute_mass_target_comparison_001/wz_absolute_mass_target_comparison.json";
const string Phase75Path = "studies/phase75_wz_absolute_mass_miss_diagnostic_001/wz_absolute_mass_miss_diagnostic.json";
const string Phase76Path = "studies/phase76_weak_coupling_amplitude_normalization_audit_001/weak_coupling_amplitude_normalization_audit.json";
const string Phase80Path = "studies/phase80_production_analytic_replay_input_materialization_audit_001/production_analytic_replay_input_materialization_audit.json";
const string Phase201Path = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase252Path = "studies/phase252_wz_normalization_closure_source_contract_audit_001/output/wz_normalization_closure_source_contract_audit_summary.json";
const string Phase313Path = "studies/phase313_official_draft_electroweak_projection_map_audit_001/output/official_draft_electroweak_projection_map_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE319_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

var phase42Text = File.ReadAllText(Phase42StudyPath);
var phase43Text = File.ReadAllText(Phase43StudyPath);
using var phase73 = JsonDocument.Parse(File.ReadAllText(Phase73Path));
using var phase74 = JsonDocument.Parse(File.ReadAllText(Phase74Path));
using var phase75 = JsonDocument.Parse(File.ReadAllText(Phase75Path));
using var phase76 = JsonDocument.Parse(File.ReadAllText(Phase76Path));
using var phase80 = JsonDocument.Parse(File.ReadAllText(Phase80Path));
using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase252 = JsonDocument.Parse(File.ReadAllText(Phase252Path));
using var phase313 = JsonDocument.Parse(File.ReadAllText(Phase313Path));

var phase42ReadySourceCandidates = phase42Text.Contains("12 ready", StringComparison.OrdinalIgnoreCase)
    && phase42Text.Contains("source candidates", StringComparison.OrdinalIgnoreCase);
var phase42PhysicalPredictionBlocked = phase42Text.Contains("Physical W/Z prediction remains blocked", StringComparison.OrdinalIgnoreCase);
var phase42RatioInvariant = phase42Text.Contains("ratio", StringComparison.OrdinalIgnoreCase)
    && phase42Text.Contains("still", StringComparison.OrdinalIgnoreCase)
    && phase42Text.Contains("invariant", StringComparison.OrdinalIgnoreCase);
var phase42SelectorEigenExtractionRequired = phase42Text.Contains("selector-specific eigenvalue extraction", StringComparison.OrdinalIgnoreCase);

var phase43ReadySourceCandidates = phase43Text.Contains("12 ready", StringComparison.OrdinalIgnoreCase)
    && phase43Text.Contains("source candidates", StringComparison.OrdinalIgnoreCase);
var phase43SelectorEigenSolvesMaterialized = phase43Text.Contains("selector/candidate-specific", StringComparison.OrdinalIgnoreCase)
    && phase43Text.Contains("generalized eigenvalue", StringComparison.OrdinalIgnoreCase)
    && phase43Text.Contains("solves", StringComparison.OrdinalIgnoreCase);
var phase43NonInvariantRatio = phase43Text.Contains("non-invariant selected W/Z ratio", StringComparison.OrdinalIgnoreCase);
var phase43AdvancedOnlyToCalibration = phase43Text.Contains("can now advance to calibration and target comparison", StringComparison.OrdinalIgnoreCase);

var projectedObservables = phase73.RootElement.TryGetProperty("observables", out var phase73Observables)
    && phase73Observables.ValueKind == JsonValueKind.Array
    ? phase73Observables.EnumerateArray().ToArray()
    : [];
var phase73ProjectionMaterialized = string.Equals(JsonString(phase73.RootElement, "status"), "projected", StringComparison.Ordinal)
    && projectedObservables.Length == 2;
var phase73WValue = projectedObservables.FirstOrDefault(row => JsonString(row, "observableId") == "physical-w-boson-mass-gev") is var wProjection
    && wProjection.ValueKind != JsonValueKind.Undefined
    ? JsonDouble(wProjection, "value")
    : null;
var phase73ZValue = projectedObservables.FirstOrDefault(row => JsonString(row, "observableId") == "physical-z-boson-mass-gev") is var zProjection
    && zProjection.ValueKind != JsonValueKind.Undefined
    ? JsonDouble(zProjection, "value")
    : null;

var targetComparisons = phase74.RootElement.TryGetProperty("comparisons", out var phase74Comparisons)
    && phase74Comparisons.ValueKind == JsonValueKind.Array
    ? phase74Comparisons.EnumerateArray().ToArray()
    : [];
var phase74TargetComparisonFailed = string.Equals(JsonString(phase74.RootElement, "terminalStatus"), "wz-absolute-mass-target-comparison-failed", StringComparison.Ordinal);
var phase74FailedComparisonCount = targetComparisons.Count(row => JsonBool(row, "passed") is false);
var phase74AllResidualsLarge = targetComparisons.Length == 2
    && targetComparisons.All(row => (JsonDouble(row, "sigmaResidual") ?? 0.0) > 5.0);

var phase75MissDiagnosed = string.Equals(JsonString(phase75.RootElement, "terminalStatus"), "wz-absolute-mass-miss-diagnosed", StringComparison.Ordinal);
var phase75CoherentCommonScaleMiss = (JsonDouble(phase75.RootElement, "meanRequiredScaleFactor") ?? 0.0) > 1.0
    && (JsonDouble(phase75.RootElement, "relativeRequiredScaleSpread") ?? double.PositiveInfinity) < 0.01
    && (JsonDouble(phase75.RootElement, "requiredWeakCoupling") ?? 0.0) > (JsonDouble(phase75.RootElement, "currentWeakCoupling") ?? double.PositiveInfinity);

var phase76NormalizationAuditBlocked = string.Equals(JsonString(phase76.RootElement, "terminalStatus"), "weak-coupling-amplitude-normalization-audit-blocked", StringComparison.Ordinal);
var phase76GeneratorNormalizationCannotExplainMiss = JsonBool(phase76.RootElement, "generatorNormalizationCanExplainMiss") is false;
var phase76RequiresNewRawMatrixElementOrScalarRelation = (JsonDouble(phase76.RootElement, "rawMatrixElementRequiredScale") ?? 0.0) > 1.0
    && (JsonDouble(phase76.RootElement, "targetImpliedRawMatrixElementMagnitude") ?? 0.0) > (JsonDouble(phase76.RootElement, "currentRawMatrixElementMagnitude") ?? double.PositiveInfinity);

var phase80ProductionInputsBlocked = string.Equals(JsonString(phase80.RootElement, "terminalStatus"), "production-analytic-replay-inputs-blocked", StringComparison.Ordinal);
var phase80Artifact = phase80.RootElement.TryGetProperty("artifact", out var phase80ArtifactElement)
    ? phase80ArtifactElement
    : default;
var phase80SelectedBosonModeMissing = !string.Equals(JsonString(phase80Artifact, "bosonModeSourceKind"), "selected-physical-wz-boson-mode", StringComparison.Ordinal);
var phase80AnalyticVariationMatrixMissing = JsonBool(phase80Artifact, "hasAnalyticVariationMatrix") is false;
var phase80FermionEigenvectorsMissing = JsonBool(phase80Artifact, "hasFermionModeEigenvectors") is false;

var phase201WzPromotable = phase201.RootElement.TryGetProperty("wzValidation", out var phase201WzValidation)
    && JsonBool(phase201WzValidation, "promotable") is true;
var phase201AllRequiredLineagesPromotable = JsonBool(phase201.RootElement, "allRequiredLineagesPromotable") is true;
var phase201WzCompleteRowCount = phase201.RootElement.TryGetProperty("wzValidation", out phase201WzValidation)
    ? JsonInt(phase201WzValidation, "completeRowCount")
    : null;

var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;
var existingEvidenceFound = JsonBool(phase213.RootElement, "existingEvidenceFound") is true;

var phase252AuditPassed = JsonBool(phase252.RootElement, "wzNormalizationClosureSourceContractAuditPassed") is true;
var phase252NormalizationArtifactsProvideSourceLineageContractFields = JsonBool(phase252.RootElement, "normalizationArtifactsProvideSourceLineageContractFields") is true;
var phase252NormalizationArtifactsProvideBridgeTheorem = JsonBool(phase252.RootElement, "normalizationArtifactsProvidePhase64BridgeTheorem") is true;
var phase252NormalizationArtifactsFillWzAbsoluteScaleUnlock = JsonBool(phase252.RootElement, "normalizationArtifactsFillWzAbsoluteScaleUnlock") is true;
var phase252SelectorEigenOperatorTermAuditBlocksNormalization = JsonBool(phase252.RootElement, "selectorEigenOperatorTermAuditBlocksNormalization") is true;
var phase252TargetDerivedRatioScaleOnly = JsonBool(phase252.RootElement, "targetDerivedRatioScaleOnly") is true;

var phase313ProjectionMapPassed = JsonBool(phase313.RootElement, "officialDraftElectroweakProjectionMapAuditPassed") is true;
var phase313ObservedElectroweakGaugeEmbedding = JsonBool(phase313.RootElement, "officialDraftProvidesObservedElectroweakGaugeEmbedding") is true;
var phase313CanFillPhase201WzContract = JsonBool(phase313.RootElement, "canFillPhase201WzContract") is true;
var phase313CanFillPhase256ObservedFieldExtractionContract = JsonBool(phase313.RootElement, "canFillPhase256ObservedFieldExtractionContract") is true;

const bool legacySelectorSpectrumSourceLawFound = false;
const bool legacySelectorRoutePromotableForBosonMasses = false;
const bool legacySelectorRouteCanFillPhase201WzContract = false;
const bool legacySelectorRouteCanFillPhase201HiggsContract = false;
const bool legacySelectorRouteCanFillPhase256ObservedFieldExtractionContract = false;
const bool legacySelectorRouteCompletesBosonPredictions = false;

var checks = new[]
{
    new Check(
        "legacy-selector-spectrum-history-materialized",
        phase42ReadySourceCandidates
            && phase42PhysicalPredictionBlocked
            && phase42RatioInvariant
            && phase42SelectorEigenExtractionRequired
            && phase43ReadySourceCandidates
            && phase43SelectorEigenSolvesMaterialized
            && phase43NonInvariantRatio
            && phase43AdvancedOnlyToCalibration,
        $"phase42Ready={phase42ReadySourceCandidates}; phase42Blocked={phase42PhysicalPredictionBlocked}; phase42RatioInvariant={phase42RatioInvariant}; phase43Ready={phase43ReadySourceCandidates}; phase43SelectorEigenSolves={phase43SelectorEigenSolvesMaterialized}; phase43NonInvariantRatio={phase43NonInvariantRatio}; phase43AdvancedOnlyToCalibration={phase43AdvancedOnlyToCalibration}"),
    new Check(
        "legacy-absolute-projection-fails-physical-targets",
        phase73ProjectionMaterialized
            && phase73WValue is > 0.0
            && phase73ZValue is > 0.0
            && phase74TargetComparisonFailed
            && phase74FailedComparisonCount == 2
            && phase74AllResidualsLarge,
        $"phase73ProjectionMaterialized={phase73ProjectionMaterialized}; wValue={phase73WValue:R}; zValue={phase73ZValue:R}; phase74TargetComparisonFailed={phase74TargetComparisonFailed}; failedComparisonCount={phase74FailedComparisonCount}; allResidualsLarge={phase74AllResidualsLarge}"),
    new Check(
        "legacy-miss-requires-new-source-normalization",
        phase75MissDiagnosed
            && phase75CoherentCommonScaleMiss
            && phase76NormalizationAuditBlocked
            && phase76GeneratorNormalizationCannotExplainMiss
            && phase76RequiresNewRawMatrixElementOrScalarRelation,
        $"phase75MissDiagnosed={phase75MissDiagnosed}; coherentCommonScaleMiss={phase75CoherentCommonScaleMiss}; phase76Blocked={phase76NormalizationAuditBlocked}; generatorNormalizationCannotExplainMiss={phase76GeneratorNormalizationCannotExplainMiss}; requiresNewRawOrScalarRelation={phase76RequiresNewRawMatrixElementOrScalarRelation}"),
    new Check(
        "production-analytic-inputs-remain-blocked",
        phase80ProductionInputsBlocked
            && phase80SelectedBosonModeMissing
            && phase80AnalyticVariationMatrixMissing
            && phase80FermionEigenvectorsMissing,
        $"phase80ProductionInputsBlocked={phase80ProductionInputsBlocked}; selectedPhysicalWzBosonModeMissing={phase80SelectedBosonModeMissing}; analyticVariationMatrixMissing={phase80AnalyticVariationMatrixMissing}; fermionEigenvectorsMissing={phase80FermionEigenvectorsMissing}"),
    new Check(
        "selector-normalization-artifacts-do-not-fill-source-contract",
        phase252AuditPassed
            && phase252TargetDerivedRatioScaleOnly
            && phase252SelectorEigenOperatorTermAuditBlocksNormalization
            && !phase252NormalizationArtifactsProvideSourceLineageContractFields
            && !phase252NormalizationArtifactsProvideBridgeTheorem
            && !phase252NormalizationArtifactsFillWzAbsoluteScaleUnlock,
        $"phase252AuditPassed={phase252AuditPassed}; targetDerivedRatioScaleOnly={phase252TargetDerivedRatioScaleOnly}; selectorEigenBlocks={phase252SelectorEigenOperatorTermAuditBlocksNormalization}; providesSourceLineageFields={phase252NormalizationArtifactsProvideSourceLineageContractFields}; providesBridgeTheorem={phase252NormalizationArtifactsProvideBridgeTheorem}; fillsWzAbsoluteScaleUnlock={phase252NormalizationArtifactsFillWzAbsoluteScaleUnlock}"),
    new Check(
        "phase201-source-contract-still-unfilled",
        !phase201WzPromotable
            && !phase201AllRequiredLineagesPromotable
            && phase201WzCompleteRowCount == 0
            && !existingEvidenceFound
            && wzMissingFieldCount == 15
            && higgsMissingFieldCount == 14,
        $"phase201WzPromotable={phase201WzPromotable}; allRequiredLineagesPromotable={phase201AllRequiredLineagesPromotable}; wzCompleteRowCount={phase201WzCompleteRowCount}; existingEvidenceFound={existingEvidenceFound}; wzMissing={wzMissingFieldCount}; higgsMissing={higgsMissingFieldCount}"),
    new Check(
        "observed-electroweak-projection-map-still-missing",
        phase313ProjectionMapPassed
            && !phase313ObservedElectroweakGaugeEmbedding
            && !phase313CanFillPhase201WzContract
            && !phase313CanFillPhase256ObservedFieldExtractionContract,
        $"phase313ProjectionMapPassed={phase313ProjectionMapPassed}; observedElectroweakGaugeEmbedding={phase313ObservedElectroweakGaugeEmbedding}; canFillPhase201WzContract={phase313CanFillPhase201WzContract}; canFillPhase256ObservedFieldExtraction={phase313CanFillPhase256ObservedFieldExtractionContract}"),
};

var legacySelectorSpectrumSourceLawAuditPassed = checks.All(check => check.Passed)
    && !legacySelectorSpectrumSourceLawFound
    && !legacySelectorRoutePromotableForBosonMasses
    && !legacySelectorRouteCanFillPhase201WzContract
    && !legacySelectorRouteCanFillPhase201HiggsContract
    && !legacySelectorRouteCanFillPhase256ObservedFieldExtractionContract
    && !legacySelectorRouteCompletesBosonPredictions;
var terminalStatus = legacySelectorSpectrumSourceLawAuditPassed
    ? "legacy-selector-spectrum-source-law-audit-no-promotable-source-law"
    : "legacy-selector-spectrum-source-law-audit-review-required";

var result = new
{
    phaseId = "phase319-legacy-selector-spectrum-source-law-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    legacySelectorSpectrumSourceLawAuditPassed,
    legacySelectorSpectrumSourceLawFound,
    legacySelectorRoutePromotableForBosonMasses,
    legacySelectorRouteCanFillPhase201WzContract,
    legacySelectorRouteCanFillPhase201HiggsContract,
    legacySelectorRouteCanFillPhase256ObservedFieldExtractionContract,
    legacySelectorRouteCompletesBosonPredictions,
    legacySelectorHistory = new
    {
        phase42ReadySourceCandidates,
        phase42PhysicalPredictionBlocked,
        phase42RatioInvariant,
        phase42SelectorEigenExtractionRequired,
        phase43ReadySourceCandidates,
        phase43SelectorEigenSolvesMaterialized,
        phase43NonInvariantRatio,
        phase43AdvancedOnlyToCalibration,
    },
    absoluteProjectionFailure = new
    {
        phase73ProjectionMaterialized,
        phase73WValue,
        phase73ZValue,
        phase74TargetComparisonFailed,
        phase74FailedComparisonCount,
        phase74AllResidualsLarge,
        phase75MissDiagnosed,
        phase75CoherentCommonScaleMiss,
        phase75MeanRequiredScaleFactor = JsonDouble(phase75.RootElement, "meanRequiredScaleFactor"),
        phase75RelativeRequiredScaleSpread = JsonDouble(phase75.RootElement, "relativeRequiredScaleSpread"),
        phase75RequiredWeakCoupling = JsonDouble(phase75.RootElement, "requiredWeakCoupling"),
        phase76NormalizationAuditBlocked,
        phase76GeneratorNormalizationCannotExplainMiss,
        phase76RequiresNewRawMatrixElementOrScalarRelation,
        phase76TargetImpliedRawMatrixElementMagnitude = JsonDouble(phase76.RootElement, "targetImpliedRawMatrixElementMagnitude"),
        phase76RawMatrixElementRequiredScale = JsonDouble(phase76.RootElement, "rawMatrixElementRequiredScale"),
    },
    replayInputBoundary = new
    {
        phase80ProductionInputsBlocked,
        phase80BosonModeSourceKind = JsonString(phase80Artifact, "bosonModeSourceKind"),
        phase80SelectedBosonModeMissing,
        phase80AnalyticVariationMatrixMissing,
        phase80FermionEigenvectorsMissing,
    },
    contractImpact = new
    {
        canFillPhase201WzContract = legacySelectorRouteCanFillPhase201WzContract,
        canFillPhase201HiggsContract = legacySelectorRouteCanFillPhase201HiggsContract,
        canFillPhase256ObservedFieldExtractionContract = legacySelectorRouteCanFillPhase256ObservedFieldExtractionContract,
        legacySelectorRouteCompletesBosonPredictions,
        phase201WzPromotable,
        phase201AllRequiredLineagesPromotable,
        phase201WzCompleteRowCount,
        existingEvidenceFound,
        wzMissingFieldCount,
        higgsMissingFieldCount,
        phase252NormalizationArtifactsProvideSourceLineageContractFields,
        phase252NormalizationArtifactsProvideBridgeTheorem,
        phase252NormalizationArtifactsFillWzAbsoluteScaleUnlock,
        phase313ObservedElectroweakGaugeEmbedding,
        phase313CanFillPhase201WzContract,
        phase313CanFillPhase256ObservedFieldExtractionContract,
    },
    checks,
    decision = "Do not promote the legacy Phase42/43/73 selector-spectrum route as the missing W/Z direct target-independent bridge-source law. It records useful selector-spectrum progress and an absolute projection, but the projection fails W/Z physical target comparison, the coherent miss requires a new source normalization or scalar relation, production analytic replay inputs remain blocked, and the current Phase201/P252/P313 gates still lack source-lineage rows and observed electroweak projection.",
    nextRequiredArtifact = new[]
    {
        "A theorem-backed, target-independent W/Z source law with separate W and Z rows, raw/common/target/stability gates, and derivation IDs filled before target comparison.",
        "A source-derived observed electroweak projection map including photon/Z rotation, W charged rows, and low-energy coupling or VEV/source closure.",
        "A solved Higgs scalar-source/operator and self-coupling or excitation lineage remains separately required.",
    },
    sourceEvidence = new
    {
        phase42StudyPath = Phase42StudyPath,
        phase43StudyPath = Phase43StudyPath,
        phase73Path = Phase73Path,
        phase74Path = Phase74Path,
        phase75Path = Phase75Path,
        phase76Path = Phase76Path,
        phase80Path = Phase80Path,
        phase201Path = Phase201Path,
        phase213Path = Phase213Path,
        phase252Path = Phase252Path,
        phase313Path = Phase313Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "legacy_selector_spectrum_source_law_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "legacy_selector_spectrum_source_law_audit_summary.json"),
    JsonSerializer.Serialize(result, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"legacySelectorSpectrumSourceLawAuditPassed={legacySelectorSpectrumSourceLawAuditPassed}");
Console.WriteLine($"legacySelectorRoutePromotableForBosonMasses={legacySelectorRoutePromotableForBosonMasses}");
Console.WriteLine($"legacySelectorRouteCanFillPhase201WzContract={legacySelectorRouteCanFillPhase201WzContract}");
Console.WriteLine($"legacySelectorRouteCompletesBosonPredictions={legacySelectorRouteCompletesBosonPredictions}");

static string? JsonString(JsonElement element, string propertyName) =>
    element.ValueKind == JsonValueKind.Object
        && element.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.String
        ? property.GetString()
        : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.ValueKind == JsonValueKind.Object
        && element.TryGetProperty(propertyName, out var property)
        ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null }
        : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.ValueKind == JsonValueKind.Object
        && element.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.Number
        && property.TryGetInt32(out var value)
        ? value
        : null;

static double? JsonDouble(JsonElement element, string propertyName) =>
    element.ValueKind == JsonValueKind.Object
        && element.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.Number
        && property.TryGetDouble(out var value)
        ? value
        : null;

public sealed record Check(
    string CheckId,
    bool Passed,
    string Detail);
