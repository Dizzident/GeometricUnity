using System.Text.Json;

const string DefaultOutputDir = "studies/phase251_upstream_wz_identity_rule_source_chain_audit_001/output";
const string Phase24Path = "studies/phase24_wz_identity_rule_readiness_001/identity_rule_readiness.json";
const string Phase25ReadinessPath = "studies/phase25_internal_electroweak_features_001/identity_rule_readiness_after_features.json";
const string Phase27IdentityReadinessPath = "studies/phase27_charge_sector_convention_001/identity_rule_readiness_after_charge_sectors.json";
const string Phase27ChargeApplicationPath = "studies/phase27_charge_sector_convention_001/charge_sector_application.json";
const string Phase27MixingConventionPath = "studies/phase27_charge_sector_convention_001/electroweak_mixing_convention.json";
const string Phase27MixingConventionReadinessPath = "studies/phase27_charge_sector_convention_001/mixing_convention_readiness.json";
const string Phase28PromotionResultPath = "studies/phase28_wz_physical_prediction_promotion_001/promotion_result.json";
const string Phase28PhysicalCalibrationsPath = "studies/phase28_wz_physical_prediction_promotion_001/physical_calibrations.json";
const string Phase28PhysicalMappingsPath = "studies/phase28_wz_physical_prediction_promotion_001/physical_observable_mappings.json";
const string Phase28ObservablesPath = "studies/phase28_wz_physical_prediction_promotion_001/observables.json";
const string Phase28PhysicalModeRecordsPath = "studies/phase28_wz_physical_prediction_promotion_001/physical_mode_records.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase245Path = "studies/phase245_rank_deficit_minimal_unlock_contract_001/output/rank_deficit_minimal_unlock_contract_summary.json";
const string Phase247Path = "studies/phase247_direct_bridge_repairability_audit_001/output/direct_bridge_repairability_audit_summary.json";
const string Phase250Path = "studies/phase250_phase46_electroweak_feature_source_lineage_audit_001/output/phase46_electroweak_feature_source_lineage_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE251_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase24 = ParseJson(Phase24Path);
using var phase25 = ParseJson(Phase25ReadinessPath);
using var phase27Identity = ParseJson(Phase27IdentityReadinessPath);
using var phase27Charge = ParseJson(Phase27ChargeApplicationPath);
using var phase27Convention = ParseJson(Phase27MixingConventionPath);
using var phase27ConventionReadiness = ParseJson(Phase27MixingConventionReadinessPath);
using var phase28Promotion = ParseJson(Phase28PromotionResultPath);
using var phase28Calibrations = ParseJson(Phase28PhysicalCalibrationsPath);
using var phase28Mappings = ParseJson(Phase28PhysicalMappingsPath);
using var phase28Observables = ParseJson(Phase28ObservablesPath);
using var phase28ModeRecords = ParseJson(Phase28PhysicalModeRecordsPath);
using var phase213 = ParseJson(Phase213Path);
using var phase245 = ParseJson(Phase245Path);
using var phase247 = ParseJson(Phase247Path);
using var phase250 = ParseJson(Phase250Path);

var phase24DerivedRuleCount = JsonArrayLength(phase24.RootElement, "derivedRules");
var phase24TerminalStatus = JsonString(phase24.RootElement, "terminalStatus") ?? "missing";
var phase25DerivedRuleCount = JsonArrayLength(phase25.RootElement, "derivedRules");
var phase25TerminalStatus = JsonString(phase25.RootElement, "terminalStatus") ?? "missing";
var phase27DerivedRules = phase27Identity.RootElement.GetProperty("derivedRules").EnumerateArray().ToArray();
var phase27DerivedRuleCount = phase27DerivedRules.Length;
var phase27TerminalStatus = JsonString(phase27Identity.RootElement, "terminalStatus") ?? "missing";
var phase27WRulePresent = phase27DerivedRules.Any(row => JsonString(row, "particleId") == "w-boson");
var phase27ZRulePresent = phase27DerivedRules.Any(row => JsonString(row, "particleId") == "z-boson");
var phase27RuleAssumptions = phase27DerivedRules.SelectMany(row => JsonStringArray(row, "assumptions")).Distinct(StringComparer.Ordinal).ToArray();
var phase27IdentityRulesUseExternalTargets = phase27RuleAssumptions.Any(text => text.Contains("external physical target values were used", StringComparison.OrdinalIgnoreCase));
var phase27InternalIdentityRuleReady = phase27TerminalStatus == "identity-rule-ready"
    && phase27DerivedRuleCount == 2
    && phase27WRulePresent
    && phase27ZRulePresent
    && !phase27IdentityRulesUseExternalTargets;

var phase27ChargedCount = JsonInt(phase27Charge.RootElement, "chargedCount") ?? 0;
var phase27NeutralCount = JsonInt(phase27Charge.RootElement, "neutralCount") ?? 0;
var phase27UnassignedCount = JsonInt(phase27Charge.RootElement, "unassignedCount") ?? 0;
var phase27ConventionExternalTargetsUsed = JsonBool(phase27Convention.RootElement, "externalTargetValuesUsed") is true;
var phase27ConventionAssumptions = JsonStringArray(phase27Convention.RootElement, "assumptions");
var phase27ConventionDeclaresInternalBranchConvention = phase27ConventionAssumptions.Any(text => text.Contains("internal branch convention", StringComparison.OrdinalIgnoreCase));
var phase27ConventionIsInternalCartanConvention = JsonString(phase27Convention.RootElement, "status") == "validated"
    && JsonString(phase27Convention.RootElement, "electroweakMultipletId") == "su2-adjoint-triplet:canonical-basis"
    && JsonString(phase27Convention.RootElement, "chargeOperatorDerivationId") == "canonical-su2-cartan-charge-operator-axis-2"
    && !phase27ConventionExternalTargetsUsed
    && phase27ConventionDeclaresInternalBranchConvention;

var phase27AssignmentRows = phase27ConventionReadiness.RootElement.GetProperty("chargeSectorAssignments").EnumerateArray().ToArray();
var phase27ChargeSectorAssignmentCount = phase27AssignmentRows.Length;
var phase27DistinctChargeDerivationIds = phase27AssignmentRows
    .Select(row => JsonString(row, "derivationId"))
    .Where(value => !string.IsNullOrWhiteSpace(value))
    .Distinct(StringComparer.Ordinal)
    .Cast<string>()
    .ToArray();

var phase28ModeRows = phase28ModeRecords.RootElement.ValueKind == JsonValueKind.Array
    ? phase28ModeRecords.RootElement.EnumerateArray().ToArray()
    : Array.Empty<JsonElement>();
var phase28HasSeparateWzModeRecords = phase28ModeRows.Any(row => JsonString(row, "particleId") == "w-boson")
    && phase28ModeRows.Any(row => JsonString(row, "particleId") == "z-boson");
var phase28ModeUnits = phase28ModeRows
    .Select(row => JsonString(row, "unit"))
    .Where(value => !string.IsNullOrWhiteSpace(value))
    .Distinct(StringComparer.Ordinal)
    .Cast<string>()
    .ToArray();
var phase28ModeRecordsUseInternalMassUnit = phase28ModeUnits.Length == 1 && phase28ModeUnits[0] == "internal-mass-unit";

var phase28ObservableIds = phase28Observables.RootElement.ValueKind == JsonValueKind.Array
    ? phase28Observables.RootElement.EnumerateArray()
        .Select(row => JsonString(row, "observableId"))
        .Where(value => !string.IsNullOrWhiteSpace(value))
        .Distinct(StringComparer.Ordinal)
        .Cast<string>()
        .ToArray()
    : Array.Empty<string>();
var phase28AbsoluteMassObservableIds = phase28ObservableIds
    .Where(id => id.Contains("w-boson-mass", StringComparison.Ordinal)
        || id.Contains("z-boson-mass", StringComparison.Ordinal)
        || id == "physical-w-boson-mass-gev"
        || id == "physical-z-boson-mass-gev")
    .ToArray();

var phase28MappingRows = phase28Mappings.RootElement.GetProperty("mappings").EnumerateArray().ToArray();
var phase28MappingTypes = phase28MappingRows
    .Select(row => JsonString(row, "physicalObservableType"))
    .Where(value => !string.IsNullOrWhiteSpace(value))
    .Distinct(StringComparer.Ordinal)
    .Cast<string>()
    .ToArray();
var phase28MappingUnits = phase28MappingRows
    .Select(row => JsonString(row, "unitFamily"))
    .Where(value => !string.IsNullOrWhiteSpace(value))
    .Distinct(StringComparer.Ordinal)
    .Cast<string>()
    .ToArray();
var phase28RatioOnlyMapping = phase28MappingTypes.SequenceEqual(new[] { "mass-ratio" })
    && phase28MappingUnits.SequenceEqual(new[] { "dimensionless" });

var phase28CalibrationRows = phase28Calibrations.RootElement.GetProperty("calibrations").EnumerateArray().ToArray();
var phase28CalibrationAssumptions = phase28CalibrationRows.SelectMany(row => JsonStringArray(row, "assumptions")).ToArray();
var phase28CalibrationExcludesAbsoluteMass = phase28CalibrationAssumptions.Any(text => text.Contains("not to either absolute boson mass", StringComparison.OrdinalIgnoreCase));
var phase28CalibrationIdentityScale = phase28CalibrationRows
    .Select(row => JsonDouble(row, "scaleFactor"))
    .Where(value => value.HasValue)
    .Select(value => value!.Value)
    .All(value => Math.Abs(value - 1.0) < 1.0e-12);

var phase28PromotionText = File.ReadAllText(Phase28PromotionResultPath);
var phase27Text = File.ReadAllText(Phase27IdentityReadinessPath)
    + File.ReadAllText(Phase27ChargeApplicationPath)
    + File.ReadAllText(Phase27MixingConventionPath)
    + File.ReadAllText(Phase27MixingConventionReadinessPath);
var upstreamIdentityText = phase27Text + phase28PromotionText;
var upstreamDerivationIdCount = CountOccurrences(upstreamIdentityText, "\"derivationId\"");
var upstreamSourceLineageFieldCount = CountOccurrences(upstreamIdentityText, "sourceLineage");
var upstreamSourceRowIdFieldCount = CountOccurrences(upstreamIdentityText, "sourceRowId");
var upstreamTheoremOrDerivationIdFieldCount = CountOccurrences(upstreamIdentityText, "theoremOrDerivationId");
var upstreamRawAmplitudeGatePassedFieldCount = CountOccurrences(upstreamIdentityText, "rawAmplitudeGatePassed");
var upstreamPhase64OccurrenceCount = CountOccurrences(upstreamIdentityText, "phase64");
var upstreamFermionCurrentOccurrenceCount = CountOccurrences(upstreamIdentityText, "fermion-current");
var upstreamTraceHalfOccurrenceCount = CountOccurrences(upstreamIdentityText, "trace-half");

var p213WzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? 0;
var p213HiggsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? 0;
var p245UnlockContractFilled = JsonBool(phase245.RootElement, "unlockContractFilled") is true;
var p245NewSourceEvidenceStillRequired = JsonBool(phase245.RootElement, "newSourceEvidenceStillRequired") is true;
var p247CurrentDirectBridgeCandidatePromotable = JsonBool(phase247.RootElement, "currentDirectBridgeCandidatePromotable") is true;
var p247SourceRowRepairPossibleFromCurrentRegistry = JsonBool(phase247.RootElement, "sourceRowRepairPossibleFromCurrentRegistry") is true;
var p247NewDirectBridgeTheoremStillRequired = JsonBool(phase247.RootElement, "newDirectBridgeTheoremStillRequired") is true;
var p250Phase46FillsWzUnlock = JsonBool(phase250.RootElement, "phase46FillsWzAbsoluteScaleUnlock") is true;
var p250Phase46ProvidesTheorem = JsonBool(phase250.RootElement, "phase46ProvidesAdjointRmsApplicationTheorem") is true;

var upstreamIdentityChainHasDerivationIds = upstreamDerivationIdCount > 0;
var upstreamDerivationIdsAreInternalIdentityOnly = phase27InternalIdentityRuleReady
    && phase27ConventionIsInternalCartanConvention
    && phase28RatioOnlyMapping
    && phase28CalibrationExcludesAbsoluteMass;
var upstreamProvidesSourceLineageContractFields = upstreamSourceLineageFieldCount > 0
    || upstreamSourceRowIdFieldCount > 0
    || upstreamTheoremOrDerivationIdFieldCount > 0
    || upstreamRawAmplitudeGatePassedFieldCount > 0;
var upstreamProvidesPhase64BridgeTheorem = upstreamPhase64OccurrenceCount > 0
    && upstreamFermionCurrentOccurrenceCount > 0
    && upstreamTraceHalfOccurrenceCount > 0
    && upstreamTheoremOrDerivationIdFieldCount > 0;
var upstreamFillsWzAbsoluteSourceContract = upstreamProvidesSourceLineageContractFields
    && upstreamProvidesPhase64BridgeTheorem
    && phase28AbsoluteMassObservableIds.Length >= 2
    && !p245NewSourceEvidenceStillRequired;
var upstreamIdentityRulePhysicalMassClaimPromotable = upstreamFillsWzAbsoluteSourceContract
    && !p247NewDirectBridgeTheoremStillRequired
    && !p250Phase46FillsWzUnlock;
var newSourceEvidenceStillRequired = p245NewSourceEvidenceStillRequired
    && p247NewDirectBridgeTheoremStillRequired
    && !upstreamFillsWzAbsoluteSourceContract;

var evidenceRows = new[]
{
    new EvidenceRow(
        "phase24-initial-readiness",
        "Phase24 initial W/Z identity readiness",
        phase24TerminalStatus == "identity-feature-blocked" && phase24DerivedRuleCount == 0,
        "pre-feature blocker",
        "Initial source families lacked electroweak multiplet, charge-sector, and current-coupling identity features.",
        false),
    new EvidenceRow(
        "phase25-feature-readiness",
        "Phase25 identity features",
        phase25TerminalStatus == "identity-feature-blocked" && phase25DerivedRuleCount == 0,
        "feature-only blocker",
        "Electroweak multiplet/current signatures are present after Phase25, but charge-sector signatures are still missing.",
        false),
    new EvidenceRow(
        "phase27-internal-identity-rules",
        "Phase27 charge-sector convention and identity rules",
        phase27InternalIdentityRuleReady,
        "internal W/Z identity labels",
        "Phase27 derives W/Z identity rules from a declared internal SU(2) Cartan convention without external targets.",
        false),
    new EvidenceRow(
        "phase28-ratio-promotion",
        "Phase28 ratio promotion",
        phase28RatioOnlyMapping && phase28CalibrationExcludesAbsoluteMass,
        "dimensionless W/Z ratio",
        "Phase28 maps identity-ready internal modes to a dimensionless W/Z mass ratio only, with calibration explicitly excluding absolute W and Z masses.",
        false),
};

var checks = new[]
{
    new Check("phase27-internal-identity-rule-chain-present", phase27InternalIdentityRuleReady && phase27ChargedCount == 9 && phase27NeutralCount == 3 && phase27UnassignedCount == 0, $"phase27TerminalStatus={phase27TerminalStatus}; phase27DerivedRuleCount={phase27DerivedRuleCount}; charged={phase27ChargedCount}; neutral={phase27NeutralCount}; unassigned={phase27UnassignedCount}"),
    new Check("phase27-convention-is-internal-not-physical-scale", phase27ConventionIsInternalCartanConvention && phase27DistinctChargeDerivationIds.Length == 1, $"phase27ConventionIsInternalCartanConvention={phase27ConventionIsInternalCartanConvention}; distinctChargeDerivationIdCount={phase27DistinctChargeDerivationIds.Length}; externalTargetsUsed={phase27ConventionExternalTargetsUsed}"),
    new Check("phase28-promotes-ratio-only", phase28HasSeparateWzModeRecords && phase28ModeRecordsUseInternalMassUnit && phase28RatioOnlyMapping && phase28CalibrationExcludesAbsoluteMass && phase28AbsoluteMassObservableIds.Length == 0, $"phase28HasSeparateWzModeRecords={phase28HasSeparateWzModeRecords}; phase28ModeRecordsUseInternalMassUnit={phase28ModeRecordsUseInternalMassUnit}; phase28RatioOnlyMapping={phase28RatioOnlyMapping}; absoluteMassObservableCount={phase28AbsoluteMassObservableIds.Length}"),
    new Check("upstream-derivation-ids-not-source-contract-fields", upstreamIdentityChainHasDerivationIds && upstreamDerivationIdsAreInternalIdentityOnly && !upstreamProvidesSourceLineageContractFields, $"upstreamDerivationIdCount={upstreamDerivationIdCount}; upstreamSourceLineageFieldCount={upstreamSourceLineageFieldCount}; upstreamSourceRowIdFieldCount={upstreamSourceRowIdFieldCount}; upstreamTheoremOrDerivationIdFieldCount={upstreamTheoremOrDerivationIdFieldCount}; upstreamRawAmplitudeGatePassedFieldCount={upstreamRawAmplitudeGatePassedFieldCount}"),
    new Check("upstream-phase64-bridge-theorem-absent", !upstreamProvidesPhase64BridgeTheorem && upstreamPhase64OccurrenceCount == 0 && upstreamFermionCurrentOccurrenceCount == 0 && upstreamTraceHalfOccurrenceCount == 0 && !p250Phase46ProvidesTheorem, $"upstreamPhase64OccurrenceCount={upstreamPhase64OccurrenceCount}; upstreamFermionCurrentOccurrenceCount={upstreamFermionCurrentOccurrenceCount}; upstreamTraceHalfOccurrenceCount={upstreamTraceHalfOccurrenceCount}; p250Phase46ProvidesTheorem={p250Phase46ProvidesTheorem}"),
    new Check("wz-absolute-contract-still-unfilled", !upstreamFillsWzAbsoluteSourceContract && !p245UnlockContractFilled && !p247CurrentDirectBridgeCandidatePromotable && !p247SourceRowRepairPossibleFromCurrentRegistry && p247NewDirectBridgeTheoremStillRequired && !p250Phase46FillsWzUnlock, $"upstreamFillsWzAbsoluteSourceContract={upstreamFillsWzAbsoluteSourceContract}; p245UnlockContractFilled={p245UnlockContractFilled}; p247NewDirectBridgeTheoremStillRequired={p247NewDirectBridgeTheoremStillRequired}; p250Phase46FillsWzUnlock={p250Phase46FillsWzUnlock}"),
    new Check("source-blocker-counts-preserved", p213WzMissingFieldCount == 15 && p213HiggsMissingFieldCount == 14 && newSourceEvidenceStillRequired, $"p213WzMissingFieldCount={p213WzMissingFieldCount}; p213HiggsMissingFieldCount={p213HiggsMissingFieldCount}; newSourceEvidenceStillRequired={newSourceEvidenceStillRequired}"),
};

var upstreamWzIdentityRuleSourceChainAuditPassed = checks.All(check => check.Passed);
var terminalStatus = upstreamWzIdentityRuleSourceChainAuditPassed
    ? "upstream-wz-identity-rule-source-chain-audit-complete-internal-identity-not-absolute-source"
    : "upstream-wz-identity-rule-source-chain-audit-review-required";
var bestSupportedScope = "internal-wz-identity-labels-and-dimensionless-ratio";

var result = new
{
    phaseId = "phase251-upstream-wz-identity-rule-source-chain-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    upstreamWzIdentityRuleSourceChainAuditPassed,
    phase27InternalIdentityRuleReady,
    phase27ConventionIsInternalCartanConvention,
    phase28RatioOnlyMapping,
    phase28CalibrationExcludesAbsoluteMass,
    upstreamIdentityChainHasDerivationIds,
    upstreamDerivationIdsAreInternalIdentityOnly,
    upstreamProvidesSourceLineageContractFields,
    upstreamProvidesPhase64BridgeTheorem,
    upstreamFillsWzAbsoluteSourceContract,
    upstreamIdentityRulePhysicalMassClaimPromotable,
    newSourceEvidenceStillRequired,
    bestSupportedScope,
    identityRuleChain = new
    {
        phase24TerminalStatus,
        phase24DerivedRuleCount,
        phase25TerminalStatus,
        phase25DerivedRuleCount,
        phase27TerminalStatus,
        phase27DerivedRuleCount,
        phase27WRulePresent,
        phase27ZRulePresent,
        phase27ChargedCount,
        phase27NeutralCount,
        phase27UnassignedCount,
        phase27RuleAssumptions,
        phase27ChargeSectorAssignmentCount,
        phase27DistinctChargeDerivationIds,
        phase27ConventionAssumptions,
    },
    phase28RatioPromotion = new
    {
        phase28HasSeparateWzModeRecords,
        phase28ModeRecordsUseInternalMassUnit,
        phase28ModeUnits,
        phase28ObservableIds,
        phase28AbsoluteMassObservableIds,
        phase28MappingTypes,
        phase28MappingUnits,
        phase28CalibrationIdentityScale,
        phase28CalibrationAssumptions,
    },
    contractFieldAudit = new
    {
        upstreamDerivationIdCount,
        upstreamSourceLineageFieldCount,
        upstreamSourceRowIdFieldCount,
        upstreamTheoremOrDerivationIdFieldCount,
        upstreamRawAmplitudeGatePassedFieldCount,
        upstreamPhase64OccurrenceCount,
        upstreamFermionCurrentOccurrenceCount,
        upstreamTraceHalfOccurrenceCount,
    },
    currentBlockerState = new
    {
        p213WzMissingFieldCount,
        p213HiggsMissingFieldCount,
        p245UnlockContractFilled,
        p245NewSourceEvidenceStillRequired,
        p247CurrentDirectBridgeCandidatePromotable,
        p247SourceRowRepairPossibleFromCurrentRegistry,
        p247NewDirectBridgeTheoremStillRequired,
        p250Phase46FillsWzUnlock,
        p250Phase46ProvidesTheorem,
    },
    evidenceRows,
    checks,
    decision = upstreamWzIdentityRuleSourceChainAuditPassed
        ? "The upstream Phase24/27/28 identity chain supplies target-independent internal W/Z labels and a dimensionless W/Z ratio mapping, but its derivationId fields are not Phase201 theoremOrDerivationId/source-lineage contract fields. The chain does not provide a Phase64 bridge theorem, absolute W/Z mass observables, raw-amplitude gates, or source rows."
        : "Review the upstream W/Z identity-rule source-chain classification before relying on it.",
    nextRequiredArtifact = new[]
    {
        "A W/Z theoremOrDerivationId that derives the absolute source law, not just an internal charge-sector identity label.",
        "A sourceLineageId with W and Z sourceRowId entries and rawAmplitudeGatePassed=true sidecars.",
        "A bridge theorem tying the internal identity-rule modes and any SU(2) normalization to the Phase64 fermion-current source and an absolute physical scale.",
    },
    sourceEvidence = new
    {
        phase24Path = Phase24Path,
        phase25ReadinessPath = Phase25ReadinessPath,
        phase27IdentityReadinessPath = Phase27IdentityReadinessPath,
        phase27ChargeApplicationPath = Phase27ChargeApplicationPath,
        phase27MixingConventionPath = Phase27MixingConventionPath,
        phase27MixingConventionReadinessPath = Phase27MixingConventionReadinessPath,
        phase28PromotionResultPath = Phase28PromotionResultPath,
        phase28PhysicalCalibrationsPath = Phase28PhysicalCalibrationsPath,
        phase28PhysicalMappingsPath = Phase28PhysicalMappingsPath,
        phase28ObservablesPath = Phase28ObservablesPath,
        phase28PhysicalModeRecordsPath = Phase28PhysicalModeRecordsPath,
        phase213Path = Phase213Path,
        phase245Path = Phase245Path,
        phase247Path = Phase247Path,
        phase250Path = Phase250Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "upstream_wz_identity_rule_source_chain_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "upstream_wz_identity_rule_source_chain_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.upstreamWzIdentityRuleSourceChainAuditPassed,
        result.phase27InternalIdentityRuleReady,
        result.phase27ConventionIsInternalCartanConvention,
        result.phase28RatioOnlyMapping,
        result.phase28CalibrationExcludesAbsoluteMass,
        result.upstreamIdentityChainHasDerivationIds,
        result.upstreamDerivationIdsAreInternalIdentityOnly,
        result.upstreamProvidesSourceLineageContractFields,
        result.upstreamProvidesPhase64BridgeTheorem,
        result.upstreamFillsWzAbsoluteSourceContract,
        result.upstreamIdentityRulePhysicalMassClaimPromotable,
        result.newSourceEvidenceStillRequired,
        result.bestSupportedScope,
        result.identityRuleChain,
        result.phase28RatioPromotion,
        result.contractFieldAudit,
        result.currentBlockerState,
        result.evidenceRows,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"upstreamWzIdentityRuleSourceChainAuditPassed={upstreamWzIdentityRuleSourceChainAuditPassed}");
Console.WriteLine($"phase27InternalIdentityRuleReady={phase27InternalIdentityRuleReady}");
Console.WriteLine($"phase28RatioOnlyMapping={phase28RatioOnlyMapping}");
Console.WriteLine($"upstreamProvidesSourceLineageContractFields={upstreamProvidesSourceLineageContractFields}");
Console.WriteLine($"upstreamProvidesPhase64BridgeTheorem={upstreamProvidesPhase64BridgeTheorem}");
Console.WriteLine($"upstreamFillsWzAbsoluteSourceContract={upstreamFillsWzAbsoluteSourceContract}");
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

static int JsonArrayLength(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Array ? property.GetArrayLength() : 0;

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
