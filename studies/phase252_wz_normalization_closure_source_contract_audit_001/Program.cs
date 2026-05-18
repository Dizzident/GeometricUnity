using System.Text.Json;

const string DefaultOutputDir = "studies/phase252_wz_normalization_closure_source_contract_audit_001/output";
const string Phase29Path = "studies/phase29_wz_ratio_failure_diagnostic_001/wz_ratio_failure_diagnostic.json";
const string Phase31Path = "studies/phase31_wz_normalization_closure_diagnostic_001/wz_normalization_closure_diagnostic.json";
const string Phase44RatioPath = "studies/phase44_selector_eigen_wz_physical_prediction_001/wz_ratio_failure_diagnostic.json";
const string Phase44PromotionPath = "studies/phase44_selector_eigen_wz_physical_prediction_001/promotion_result.json";
const string Phase44PhysicalCalibrationsPath = "studies/phase44_selector_eigen_wz_physical_prediction_001/physical_calibrations.json";
const string Phase44PhysicalMappingsPath = "studies/phase44_selector_eigen_wz_physical_prediction_001/physical_observable_mappings.json";
const string Phase45Path = "studies/phase45_selector_eigen_operator_term_audit_001/selector_eigen_operator_term_audit.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase245Path = "studies/phase245_rank_deficit_minimal_unlock_contract_001/output/rank_deficit_minimal_unlock_contract_summary.json";
const string Phase247Path = "studies/phase247_direct_bridge_repairability_audit_001/output/direct_bridge_repairability_audit_summary.json";
const string Phase250Path = "studies/phase250_phase46_electroweak_feature_source_lineage_audit_001/output/phase46_electroweak_feature_source_lineage_audit_summary.json";
const string Phase251Path = "studies/phase251_upstream_wz_identity_rule_source_chain_audit_001/output/upstream_wz_identity_rule_source_chain_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE252_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase29 = ParseJson(Phase29Path);
using var phase31 = ParseJson(Phase31Path);
using var phase44Ratio = ParseJson(Phase44RatioPath);
using var phase44Promotion = ParseJson(Phase44PromotionPath);
using var phase44Calibrations = ParseJson(Phase44PhysicalCalibrationsPath);
using var phase44Mappings = ParseJson(Phase44PhysicalMappingsPath);
using var phase45 = ParseJson(Phase45Path);
using var phase213 = ParseJson(Phase213Path);
using var phase245 = ParseJson(Phase245Path);
using var phase247 = ParseJson(Phase247Path);
using var phase250 = ParseJson(Phase250Path);
using var phase251 = ParseJson(Phase251Path);

var phase29SelectedPair = phase29.RootElement.GetProperty("selectedPair");
var phase29SelectedPairId = JsonString(phase29SelectedPair, "pairId") ?? "missing";
var phase29SelectedRatio = JsonDouble(phase29SelectedPair, "ratio");
var phase29SelectedPull = JsonDouble(phase29SelectedPair, "pull");
var phase29SelectedPassesSigma5 = JsonBool(phase29SelectedPair, "passesSigma5") is true;
var phase29RequiredScaleToTarget = JsonDouble(phase29SelectedPair, "requiredScaleToTarget");
var phase29PairDiagnostics = phase29.RootElement.GetProperty("pairDiagnostics").EnumerateArray().ToArray();
var phase29Sigma5PassingPairCount = phase29PairDiagnostics.Count(row => JsonBool(row, "passesSigma5") is true);
var phase29TargetObservableId = JsonString(phase29.RootElement, "targetObservableId");

var phase31TerminalStatus = JsonString(phase31.RootElement, "terminalStatus") ?? "missing";
var phase31TargetObservableId = JsonString(phase31.RootElement, "targetObservableId");
var phase31ComputedRatio = JsonDouble(phase31.RootElement, "computedRatio");
var phase31RequiredScaleToTarget = JsonDouble(phase31.RootElement, "requiredScaleToTarget");
var phase31DeclaredScaleFactor = JsonDouble(phase31.RootElement, "declaredScaleFactor");
var phase31DeclaredScaleDelta = JsonDouble(phase31.RootElement, "declaredScaleDelta");
var phase31DerivationBackedScaleAvailable = JsonBool(phase31.RootElement, "derivationBackedScaleAvailable") is true;
var phase31NormalizationChangeAllowed = JsonBool(phase31.RootElement, "normalizationChangeAllowed") is true;
var phase31SelectorVariationExplainsMiss = JsonBool(phase31.RootElement, "selectorVariationExplainsMiss") is true;
var phase31DeclaredCalibration = phase31.RootElement.GetProperty("declaredCalibration");
var phase31DeclaredCalibrationHasOperatorDerivation = JsonBool(phase31DeclaredCalibration, "hasOperatorDerivation") is true;
var phase31DeclaredCalibrationIsIdentity = JsonBool(phase31DeclaredCalibration, "isIdentityNormalization") is true;
var phase31ClosureRequirements = JsonStringArray(phase31.RootElement, "closureRequirements");
var phase31RequestsDerivationBackedScale = phase31ClosureRequirements.Any(text => text.Contains("derivation-backed normalization/operator scale", StringComparison.OrdinalIgnoreCase));

var phase44SelectedPair = phase44Ratio.RootElement.GetProperty("selectedPair");
var phase44SelectedPairId = JsonString(phase44SelectedPair, "pairId") ?? "missing";
var phase44SelectedRatio = JsonDouble(phase44SelectedPair, "ratio");
var phase44SelectedPull = JsonDouble(phase44SelectedPair, "pull");
var phase44SelectedPassesSigma5 = JsonBool(phase44SelectedPair, "passesSigma5") is true;
var phase44RequiredScaleToTarget = JsonDouble(phase44SelectedPair, "requiredScaleToTarget");
var phase44PairDiagnostics = phase44Ratio.RootElement.GetProperty("pairDiagnostics").EnumerateArray().ToArray();
var phase44Sigma5PassingPairCount = phase44PairDiagnostics.Count(row => JsonBool(row, "passesSigma5") is true);
var phase44TargetObservableId = JsonString(phase44Ratio.RootElement, "targetObservableId");

var phase44PromotionModeRows = phase44Promotion.RootElement.GetProperty("physicalModeRecords").EnumerateArray().ToArray();
var phase44HasSeparateWzModeRecords = phase44PromotionModeRows.Any(row => JsonString(row, "particleId") == "w-boson")
    && phase44PromotionModeRows.Any(row => JsonString(row, "particleId") == "z-boson");
var phase44ModeUnits = phase44PromotionModeRows
    .Select(row => JsonString(row, "unit"))
    .Where(value => !string.IsNullOrWhiteSpace(value))
    .Distinct(StringComparer.Ordinal)
    .Cast<string>()
    .ToArray();
var phase44UsesInternalMassUnit = phase44ModeUnits.Length == 1 && phase44ModeUnits[0] == "internal-mass-unit";

var phase44MappingRows = phase44Mappings.RootElement.GetProperty("mappings").EnumerateArray().ToArray();
var phase44MappingTypes = phase44MappingRows
    .Select(row => JsonString(row, "physicalObservableType"))
    .Where(value => !string.IsNullOrWhiteSpace(value))
    .Distinct(StringComparer.Ordinal)
    .Cast<string>()
    .ToArray();
var phase44MappingUnits = phase44MappingRows
    .Select(row => JsonString(row, "unitFamily"))
    .Where(value => !string.IsNullOrWhiteSpace(value))
    .Distinct(StringComparer.Ordinal)
    .Cast<string>()
    .ToArray();
var phase44RatioOnlyMapping = phase44MappingTypes.SequenceEqual(new[] { "mass-ratio" })
    && phase44MappingUnits.SequenceEqual(new[] { "dimensionless" });
var phase44CalibrationRows = phase44Calibrations.RootElement.GetProperty("calibrations").EnumerateArray().ToArray();
var phase44CalibrationAssumptions = phase44CalibrationRows.SelectMany(row => JsonStringArray(row, "assumptions")).ToArray();
var phase44CalibrationExcludesAbsoluteMass = phase44CalibrationAssumptions.Any(text => text.Contains("not to either absolute boson mass", StringComparison.OrdinalIgnoreCase));
var phase44CalibrationIdentityScale = phase44CalibrationRows
    .Select(row => JsonDouble(row, "scaleFactor"))
    .Where(value => value.HasValue)
    .Select(value => value!.Value)
    .All(value => Math.Abs(value - 1.0) < 1.0e-12);

var phase45TerminalStatus = JsonString(phase45.RootElement, "terminalStatus") ?? "missing";
var phase45RequiredScaleToTarget = JsonDouble(phase45.RootElement, "requiredScaleToTarget");
var phase45RequiredRatioShiftFraction = JsonDouble(phase45.RootElement, "requiredRatioShiftFraction");
var phase45NonTrivialOperatorTermEvidenceCount = JsonInt(phase45.RootElement, "nonTrivialOperatorTermEvidenceCount") ?? 0;
var phase45ObservedOperatorTypes = JsonStringArray(phase45.RootElement, "observedOperatorTypes");
var phase45ObservedModeBlocks = JsonStringArray(phase45.RootElement, "observedModeBlocks");
var phase45ClosureRequirements = JsonStringArray(phase45.RootElement, "closureRequirements");
var phase45OnlyConnectionBlock = phase45ObservedModeBlocks.Length == 1 && phase45ObservedModeBlocks[0] == "connection";
var phase45LacksElectroweakOperatorTerm = phase45NonTrivialOperatorTermEvidenceCount == 0
    && phase45ClosureRequirements.Any(text => text.Contains("no target-independent electroweak", StringComparison.OrdinalIgnoreCase));

var auditedText = File.ReadAllText(Phase29Path)
    + File.ReadAllText(Phase31Path)
    + File.ReadAllText(Phase44RatioPath)
    + File.ReadAllText(Phase44PromotionPath)
    + File.ReadAllText(Phase44PhysicalCalibrationsPath)
    + File.ReadAllText(Phase44PhysicalMappingsPath)
    + File.ReadAllText(Phase45Path);
var contractSourceLineageFieldCount = CountOccurrences(auditedText, "sourceLineage");
var contractSourceRowIdFieldCount = CountOccurrences(auditedText, "sourceRowId");
var contractTheoremOrDerivationIdFieldCount = CountOccurrences(auditedText, "theoremOrDerivationId");
var contractRawAmplitudeGatePassedFieldCount = CountOccurrences(auditedText, "rawAmplitudeGatePassed");
var phase64OccurrenceCount = CountOccurrences(auditedText, "phase64");
var fermionCurrentOccurrenceCount = CountOccurrences(auditedText, "fermion-current");
var traceHalfOccurrenceCount = CountOccurrences(auditedText, "trace-half");

var p213WzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? 0;
var p213HiggsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? 0;
var p245UnlockContractFilled = JsonBool(phase245.RootElement, "unlockContractFilled") is true;
var p245NewSourceEvidenceStillRequired = JsonBool(phase245.RootElement, "newSourceEvidenceStillRequired") is true;
var p247NewDirectBridgeTheoremStillRequired = JsonBool(phase247.RootElement, "newDirectBridgeTheoremStillRequired") is true;
var p250Phase46FillsWzUnlock = JsonBool(phase250.RootElement, "phase46FillsWzAbsoluteScaleUnlock") is true;
var p251UpstreamFillsWzContract = JsonBool(phase251.RootElement, "upstreamFillsWzAbsoluteSourceContract") is true;

var phase31NormalizationClosureAuditPassed = phase31TerminalStatus == "wz-normalization-closure-blocked"
    && phase31TargetObservableId == "physical-w-z-mass-ratio"
    && phase31DeclaredCalibrationIsIdentity
    && !phase31DerivationBackedScaleAvailable
    && !phase31NormalizationChangeAllowed
    && !phase31DeclaredCalibrationHasOperatorDerivation;
var targetDerivedRatioScaleOnly = phase29TargetObservableId == "physical-w-z-mass-ratio"
    && phase44TargetObservableId == "physical-w-z-mass-ratio"
    && phase29RequiredScaleToTarget.HasValue
    && phase44RequiredScaleToTarget.HasValue
    && phase31RequiredScaleToTarget.HasValue
    && phase29RequiredScaleToTarget.Value != 1.0
    && phase44RequiredScaleToTarget.Value != 1.0
    && phase31RequiredScaleToTarget.Value != 1.0;
var selectorEigenOperatorTermAuditBlocksNormalization = phase45TerminalStatus == "wz-selector-eigen-operator-term-blocked"
    && phase45LacksElectroweakOperatorTerm
    && phase45OnlyConnectionBlock;
var normalizationArtifactsProvideSourceLineageContractFields = contractSourceLineageFieldCount > 0
    || contractSourceRowIdFieldCount > 0
    || contractTheoremOrDerivationIdFieldCount > 0
    || contractRawAmplitudeGatePassedFieldCount > 0;
var normalizationArtifactsProvidePhase64BridgeTheorem = phase64OccurrenceCount > 0
    && fermionCurrentOccurrenceCount > 0
    && traceHalfOccurrenceCount > 0
    && contractTheoremOrDerivationIdFieldCount > 0;
var normalizationArtifactsFillWzAbsoluteScaleUnlock = phase31DerivationBackedScaleAvailable
    && phase31NormalizationChangeAllowed
    && normalizationArtifactsProvideSourceLineageContractFields
    && normalizationArtifactsProvidePhase64BridgeTheorem
    && !p245NewSourceEvidenceStillRequired;
var normalizationClosurePhysicalMassClaimPromotable = normalizationArtifactsFillWzAbsoluteScaleUnlock
    && !p247NewDirectBridgeTheoremStillRequired
    && !p250Phase46FillsWzUnlock
    && !p251UpstreamFillsWzContract;
var newSourceEvidenceStillRequired = p245NewSourceEvidenceStillRequired
    && p247NewDirectBridgeTheoremStillRequired
    && !normalizationArtifactsFillWzAbsoluteScaleUnlock;

var evidenceRows = new[]
{
    new EvidenceRow(
        "phase31-normalization-closure",
        "Phase31 W/Z normalization closure diagnostic",
        phase31NormalizationClosureAuditPassed,
        "blocked target-ratio normalization diagnostic",
        "Phase31 reports required ratio scale from the target, but no derivation-backed operator normalization scale is available and normalization change is not allowed.",
        false),
    new EvidenceRow(
        "phase29-ratio-failure",
        "Phase29 W/Z ratio failure diagnostic",
        phase29Sigma5PassingPairCount == 0 && !phase29SelectedPassesSigma5,
        "target comparison failure diagnostic",
        "The identity-rule-selected pair fails sigma-5 and every charged/neutral pair in the current internal source set fails the target gate.",
        false),
    new EvidenceRow(
        "phase44-selector-eigen-ratio-failure",
        "Phase44 selector-eigen W/Z ratio failure diagnostic",
        phase44Sigma5PassingPairCount == 0 && !phase44SelectedPassesSigma5,
        "selector-eigen target comparison failure diagnostic",
        "The selector-eigen identity-rule pair also fails sigma-5 and remains a dimensionless W/Z ratio comparison.",
        false),
    new EvidenceRow(
        "phase45-operator-term-audit",
        "Phase45 selector-eigen operator term audit",
        selectorEigenOperatorTermAuditBlocksNormalization,
        "operator-term absence diagnostic",
        "Phase45 finds no target-independent electroweak, mixing, normalization-scale, or nontrivial mass-operator term evidence for the selected spectra.",
        false),
};

var checks = new[]
{
    new Check("phase31-normalization-closure-blocked", phase31NormalizationClosureAuditPassed && phase31RequestsDerivationBackedScale, $"terminalStatus={phase31TerminalStatus}; derivationBackedScaleAvailable={phase31DerivationBackedScaleAvailable}; normalizationChangeAllowed={phase31NormalizationChangeAllowed}; hasOperatorDerivation={phase31DeclaredCalibrationHasOperatorDerivation}"),
    new Check("target-derived-ratio-scales-not-source-laws", targetDerivedRatioScaleOnly && phase29Sigma5PassingPairCount == 0 && phase44Sigma5PassingPairCount == 0, $"phase29RequiredScaleToTarget={phase29RequiredScaleToTarget}; phase31RequiredScaleToTarget={phase31RequiredScaleToTarget}; phase44RequiredScaleToTarget={phase44RequiredScaleToTarget}; phase29Sigma5PassingPairCount={phase29Sigma5PassingPairCount}; phase44Sigma5PassingPairCount={phase44Sigma5PassingPairCount}"),
    new Check("phase44-remains-ratio-only-internal-unit", phase44HasSeparateWzModeRecords && phase44UsesInternalMassUnit && phase44RatioOnlyMapping && phase44CalibrationIdentityScale && phase44CalibrationExcludesAbsoluteMass, $"phase44HasSeparateWzModeRecords={phase44HasSeparateWzModeRecords}; phase44UsesInternalMassUnit={phase44UsesInternalMassUnit}; phase44RatioOnlyMapping={phase44RatioOnlyMapping}; phase44CalibrationExcludesAbsoluteMass={phase44CalibrationExcludesAbsoluteMass}"),
    new Check("phase45-selector-eigen-operator-term-blocked", selectorEigenOperatorTermAuditBlocksNormalization, $"phase45TerminalStatus={phase45TerminalStatus}; nonTrivialOperatorTermEvidenceCount={phase45NonTrivialOperatorTermEvidenceCount}; observedModeBlocks=[{string.Join(",", phase45ObservedModeBlocks)}]; requiredRatioShiftFraction={phase45RequiredRatioShiftFraction}"),
    new Check("normalization-artifacts-have-no-source-contract-fields", !normalizationArtifactsProvideSourceLineageContractFields, $"contractSourceLineageFieldCount={contractSourceLineageFieldCount}; contractSourceRowIdFieldCount={contractSourceRowIdFieldCount}; contractTheoremOrDerivationIdFieldCount={contractTheoremOrDerivationIdFieldCount}; contractRawAmplitudeGatePassedFieldCount={contractRawAmplitudeGatePassedFieldCount}"),
    new Check("normalization-artifacts-have-no-phase64-bridge-theorem", !normalizationArtifactsProvidePhase64BridgeTheorem && phase64OccurrenceCount == 0 && fermionCurrentOccurrenceCount == 0 && traceHalfOccurrenceCount == 0, $"phase64OccurrenceCount={phase64OccurrenceCount}; fermionCurrentOccurrenceCount={fermionCurrentOccurrenceCount}; traceHalfOccurrenceCount={traceHalfOccurrenceCount}"),
    new Check("wz-absolute-unlock-still-unfilled", !normalizationArtifactsFillWzAbsoluteScaleUnlock && !p245UnlockContractFilled && p245NewSourceEvidenceStillRequired && p247NewDirectBridgeTheoremStillRequired && !p250Phase46FillsWzUnlock && !p251UpstreamFillsWzContract, $"normalizationArtifactsFillWzAbsoluteScaleUnlock={normalizationArtifactsFillWzAbsoluteScaleUnlock}; p245UnlockContractFilled={p245UnlockContractFilled}; p250Phase46FillsWzUnlock={p250Phase46FillsWzUnlock}; p251UpstreamFillsWzContract={p251UpstreamFillsWzContract}"),
    new Check("source-blocker-counts-preserved", p213WzMissingFieldCount == 15 && p213HiggsMissingFieldCount == 14 && newSourceEvidenceStillRequired, $"p213WzMissingFieldCount={p213WzMissingFieldCount}; p213HiggsMissingFieldCount={p213HiggsMissingFieldCount}; newSourceEvidenceStillRequired={newSourceEvidenceStillRequired}"),
};

var wzNormalizationClosureSourceContractAuditPassed = checks.All(check => check.Passed);
var terminalStatus = wzNormalizationClosureSourceContractAuditPassed
    ? "wz-normalization-closure-source-contract-audit-complete-target-ratio-scale-not-source-law"
    : "wz-normalization-closure-source-contract-audit-review-required";
var bestSupportedScope = "blocked-wz-ratio-normalization-diagnostic";

var result = new
{
    phaseId = "phase252-wz-normalization-closure-source-contract-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    wzNormalizationClosureSourceContractAuditPassed,
    phase31NormalizationClosureAuditPassed,
    targetDerivedRatioScaleOnly,
    selectorEigenOperatorTermAuditBlocksNormalization,
    normalizationArtifactsProvideSourceLineageContractFields,
    normalizationArtifactsProvidePhase64BridgeTheorem,
    normalizationArtifactsFillWzAbsoluteScaleUnlock,
    normalizationClosurePhysicalMassClaimPromotable,
    newSourceEvidenceStillRequired,
    bestSupportedScope,
    phase31NormalizationClosure = new
    {
        phase31TerminalStatus,
        phase31TargetObservableId,
        phase31ComputedRatio,
        phase31RequiredScaleToTarget,
        phase31DeclaredScaleFactor,
        phase31DeclaredScaleDelta,
        phase31DerivationBackedScaleAvailable,
        phase31NormalizationChangeAllowed,
        phase31SelectorVariationExplainsMiss,
        phase31DeclaredCalibrationIsIdentity,
        phase31DeclaredCalibrationHasOperatorDerivation,
        phase31ClosureRequirements,
    },
    ratioFailureDiagnostics = new
    {
        phase29SelectedPairId,
        phase29SelectedRatio,
        phase29SelectedPull,
        phase29SelectedPassesSigma5,
        phase29RequiredScaleToTarget,
        phase29Sigma5PassingPairCount,
        phase44SelectedPairId,
        phase44SelectedRatio,
        phase44SelectedPull,
        phase44SelectedPassesSigma5,
        phase44RequiredScaleToTarget,
        phase44Sigma5PassingPairCount,
    },
    phase44RatioPromotion = new
    {
        phase44HasSeparateWzModeRecords,
        phase44UsesInternalMassUnit,
        phase44ModeUnits,
        phase44RatioOnlyMapping,
        phase44MappingTypes,
        phase44MappingUnits,
        phase44CalibrationIdentityScale,
        phase44CalibrationExcludesAbsoluteMass,
    },
    phase45OperatorTermAudit = new
    {
        phase45TerminalStatus,
        phase45RequiredScaleToTarget,
        phase45RequiredRatioShiftFraction,
        phase45NonTrivialOperatorTermEvidenceCount,
        phase45ObservedOperatorTypes,
        phase45ObservedModeBlocks,
        phase45ClosureRequirements,
    },
    contractFieldAudit = new
    {
        contractSourceLineageFieldCount,
        contractSourceRowIdFieldCount,
        contractTheoremOrDerivationIdFieldCount,
        contractRawAmplitudeGatePassedFieldCount,
        phase64OccurrenceCount,
        fermionCurrentOccurrenceCount,
        traceHalfOccurrenceCount,
    },
    currentBlockerState = new
    {
        p213WzMissingFieldCount,
        p213HiggsMissingFieldCount,
        p245UnlockContractFilled,
        p245NewSourceEvidenceStillRequired,
        p247NewDirectBridgeTheoremStillRequired,
        p250Phase46FillsWzUnlock,
        p251UpstreamFillsWzContract,
    },
    evidenceRows,
    checks,
    decision = wzNormalizationClosureSourceContractAuditPassed
        ? "Phase31/29/44/45 identify a missing W/Z ratio normalization scale, but the only required scale is target-implied and the selector-eigen operator audit finds no target-independent electroweak or normalization-scale term. These artifacts do not provide Phase201 source-lineage contract fields, a Phase64 bridge theorem, source rows, raw-amplitude gates, or W/Z absolute masses."
        : "Review the W/Z normalization-closure source-contract classification before relying on this audit.",
    nextRequiredArtifact = new[]
    {
        "A derivation-backed normalization/operator scale independent of the physical W/Z target.",
        "Phase201 W/Z sourceLineageId and theoremOrDerivationId fields, with separate W/Z source rows and rawAmplitudeGatePassed=true.",
        "A bridge from the normalization/operator scale to the Phase64 fermion-current source and absolute W/Z physical mass scale.",
    },
    sourceEvidence = new
    {
        phase29Path = Phase29Path,
        phase31Path = Phase31Path,
        phase44RatioPath = Phase44RatioPath,
        phase44PromotionPath = Phase44PromotionPath,
        phase44PhysicalCalibrationsPath = Phase44PhysicalCalibrationsPath,
        phase44PhysicalMappingsPath = Phase44PhysicalMappingsPath,
        phase45Path = Phase45Path,
        phase213Path = Phase213Path,
        phase245Path = Phase245Path,
        phase247Path = Phase247Path,
        phase250Path = Phase250Path,
        phase251Path = Phase251Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "wz_normalization_closure_source_contract_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "wz_normalization_closure_source_contract_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.wzNormalizationClosureSourceContractAuditPassed,
        result.phase31NormalizationClosureAuditPassed,
        result.targetDerivedRatioScaleOnly,
        result.selectorEigenOperatorTermAuditBlocksNormalization,
        result.normalizationArtifactsProvideSourceLineageContractFields,
        result.normalizationArtifactsProvidePhase64BridgeTheorem,
        result.normalizationArtifactsFillWzAbsoluteScaleUnlock,
        result.normalizationClosurePhysicalMassClaimPromotable,
        result.newSourceEvidenceStillRequired,
        result.bestSupportedScope,
        result.phase31NormalizationClosure,
        result.ratioFailureDiagnostics,
        result.phase44RatioPromotion,
        result.phase45OperatorTermAudit,
        result.contractFieldAudit,
        result.currentBlockerState,
        result.evidenceRows,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"wzNormalizationClosureSourceContractAuditPassed={wzNormalizationClosureSourceContractAuditPassed}");
Console.WriteLine($"phase31NormalizationClosureAuditPassed={phase31NormalizationClosureAuditPassed}");
Console.WriteLine($"targetDerivedRatioScaleOnly={targetDerivedRatioScaleOnly}");
Console.WriteLine($"normalizationArtifactsProvideSourceLineageContractFields={normalizationArtifactsProvideSourceLineageContractFields}");
Console.WriteLine($"normalizationArtifactsProvidePhase64BridgeTheorem={normalizationArtifactsProvidePhase64BridgeTheorem}");
Console.WriteLine($"normalizationArtifactsFillWzAbsoluteScaleUnlock={normalizationArtifactsFillWzAbsoluteScaleUnlock}");
Console.WriteLine($"newSourceEvidenceStillRequired={newSourceEvidenceStillRequired}");

static JsonDocument ParseJson(string path)
{
    if (!File.Exists(path))
    {
        throw new FileNotFoundException($"Required artifact missing: {path}", path);
    }

    return JsonDocument.Parse(File.ReadAllText(path));
}

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static string[] JsonStringArray(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Array
        ? property.EnumerateArray()
            .Where(item => item.ValueKind == JsonValueKind.String)
            .Select(item => item.GetString()!)
            .ToArray()
        : Array.Empty<string>();

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

static double? JsonDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var value) ? value : null;

static int CountOccurrences(string text, string pattern)
{
    var count = 0;
    var index = 0;
    while ((index = text.IndexOf(pattern, index, StringComparison.Ordinal)) >= 0)
    {
        count++;
        index += pattern.Length;
    }

    return count;
}

sealed record EvidenceRow(
    string EvidenceId,
    string Label,
    bool Present,
    string BestSupportedScope,
    string Detail,
    bool FillsWzAbsoluteScaleUnlock);

sealed record Check(string CheckId, bool Passed, string Detail);
