using System.Text.Json;

const string DefaultOutputDir = "studies/phase193_boson_prediction_completion_audit_001/output";
const string Phase101Path = "studies/phase101_boson_prediction_package_001/output/boson_prediction_package_summary.json";
const string Phase191Path = "studies/phase191_wz_direct_bridge_prediction_decision_001/output/wz_direct_bridge_prediction_decision_summary.json";
const string Phase192Path = "studies/phase192_boson_scientific_defensibility_ledger_001/output/boson_scientific_defensibility_ledger_summary.json";
const string Phase189Path = "studies/phase189_higgs_scalar_source_operator_census_001/output/higgs_scalar_source_operator_census_summary.json";
const string Phase194Path = "studies/phase194_draft_boson_source_evidence_audit_001/output/draft_boson_source_evidence_audit_summary.json";
const string Phase195Path = "studies/phase195_electroweak_vev_wz_absolute_closure_audit_001/output/electroweak_vev_wz_absolute_closure_audit_summary.json";
const string Phase196Path = "studies/phase196_higgs_potential_self_coupling_closure_audit_001/output/higgs_potential_self_coupling_closure_audit_summary.json";
const string Phase197Path = "studies/phase197_electroweak_weak_coupling_wz_mass_closure_audit_001/output/electroweak_weak_coupling_wz_mass_closure_audit_summary.json";
const string Phase198Path = "studies/phase198_weak_coupling_source_lineage_closure_audit_001/output/weak_coupling_source_lineage_closure_audit_summary.json";
const string Phase199Path = "studies/phase199_higgs_scalar_source_lineage_closure_audit_001/output/higgs_scalar_source_lineage_closure_audit_summary.json";
const string Phase206Path = "studies/phase206_direct_bridge_normalization_closure_001/output/direct_bridge_normalization_closure_summary.json";
const string Phase207Path = "studies/phase207_higgs_quartic_self_coupling_source_scan_001/output/higgs_quartic_self_coupling_source_scan_summary.json";
const string Phase214Path = "studies/phase214_external_electroweak_input_loophole_audit_001/output/external_electroweak_input_loophole_audit_summary.json";
const string Phase215Path = "studies/phase215_higgs_target_implied_self_coupling_loophole_audit_001/output/higgs_target_implied_self_coupling_loophole_audit_summary.json";
const string Phase218Path = "studies/phase218_official_gu_public_source_audit_001/output/official_gu_public_source_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE193_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase101 = JsonDocument.Parse(File.ReadAllText(Phase101Path));
using var phase191 = JsonDocument.Parse(File.ReadAllText(Phase191Path));
using var phase192 = JsonDocument.Parse(File.ReadAllText(Phase192Path));
using var phase189 = JsonDocument.Parse(File.ReadAllText(Phase189Path));
using var phase194 = JsonDocument.Parse(File.ReadAllText(Phase194Path));
using var phase195 = JsonDocument.Parse(File.ReadAllText(Phase195Path));
using var phase196 = JsonDocument.Parse(File.ReadAllText(Phase196Path));
using var phase197 = JsonDocument.Parse(File.ReadAllText(Phase197Path));
using var phase198 = JsonDocument.Parse(File.ReadAllText(Phase198Path));
using var phase199 = JsonDocument.Parse(File.ReadAllText(Phase199Path));
using var phase206 = JsonDocument.Parse(File.ReadAllText(Phase206Path));
using var phase207 = JsonDocument.Parse(File.ReadAllText(Phase207Path));
using var phase214 = JsonDocument.Parse(File.ReadAllText(Phase214Path));
using var phase215 = JsonDocument.Parse(File.ReadAllText(Phase215Path));
using var phase218 = JsonDocument.Parse(File.ReadAllText(Phase218Path));

var allKnownBosonValuesDefensible = JsonBool(phase192.RootElement, "allKnownBosonValuesDefensible") is true;
var wzPredictionAllowed = JsonBool(phase101.RootElement, "wzDirectBridgePredictionAllowed") is true;
var p191CanComplete = JsonBool(phase191.RootElement, "canCompleteSuccessfulPrediction") is true;
var p189PredictionAllowed = JsonBool(phase189.RootElement, "predictionAttemptAllowed") is true;
var draftProvidesDirectWzLaw = JsonBool(phase194.RootElement, "draftProvidesDirectWzLaw") is true;
var draftProvidesSolvedHiggsSource = JsonBool(phase194.RootElement, "draftProvidesSolvedHiggsSource") is true;
var draftProvidesCompletionSource = draftProvidesDirectWzLaw || draftProvidesSolvedHiggsSource;
var vevWzAbsoluteClosurePassed = JsonBool(phase195.RootElement, "canPromoteWzAbsoluteFromVevScale") is true;
var higgsPotentialClosurePassed = JsonBool(phase196.RootElement, "canPromoteHiggsFromPotentialOrSelfCoupling") is true;
var weakCouplingWzClosurePassed = JsonBool(phase197.RootElement, "canPromoteWzFromWeakCouplingMassRelation") is true;
var weakCouplingLineageClosurePassed = JsonBool(phase198.RootElement, "canPromoteAnyWeakCouplingSourceForWzAbsolute") is true;
var higgsScalarSourceLineageClosurePassed = JsonBool(phase199.RootElement, "canPromoteAnyHiggsScalarSourceLineage") is true;
var directBridgeNormalizationClosurePassed = JsonBool(phase206.RootElement, "canPromoteDirectBridgeNormalization") is true;
var higgsQuarticSelfCouplingSourceScanPassed = JsonBool(phase207.RootElement, "canPromoteHiggsQuarticSelfCouplingSource") is true;
var externalElectroweakInputLoopholeClosed = JsonBool(phase214.RootElement, "canPromoteExternalElectroweakBridge") is false;
var higgsTargetImpliedSelfCouplingLoopholeClosed = JsonBool(phase215.RootElement, "canPromoteTargetImpliedHiggsSelfCoupling") is false;
var officialDraftProvidesCompletionSource = JsonBool(phase218.RootElement, "officialDraftProvidesCompletionSource") is true;

var checklist = new[]
{
    new AuditItem(
        "defensible-values-ledger",
        "Produce a ledger of scientifically defensible boson values.",
        "covered",
        JsonString(phase192.RootElement, "terminalStatus") ?? "missing",
        Phase192Path),
    new AuditItem(
        "all-known-bosons-complete",
        "Every known boson row must be promoted, passed, and gate-defensible.",
        allKnownBosonValuesDefensible ? "passed" : "failed",
        $"allKnownBosonValuesDefensible={allKnownBosonValuesDefensible}; defensibleValueCount={JsonInt(phase192.RootElement, "defensibleValueCount")}; failedAttemptCount={JsonInt(phase192.RootElement, "failedAttemptCount")}; blockedCount={JsonInt(phase192.RootElement, "blockedCount")}",
        Phase192Path),
    new AuditItem(
        "wz-direct-bridge-law",
        "W/Z absolute masses require a derivation-promoted direct bridge source and particle-specific W/Z rows.",
        p191CanComplete && wzPredictionAllowed ? "passed" : "failed",
        $"canCompleteSuccessfulPrediction={p191CanComplete}; wzDirectBridgePredictionAllowed={wzPredictionAllowed}; decision={JsonString(phase191.RootElement, "decision")}",
        Phase191Path),
    new AuditItem(
        "wz-vev-scale-closure",
        "The validated electroweak VEV/order-parameter scale must close W/Z absolute masses without source-shape or target-comparison failure.",
        vevWzAbsoluteClosurePassed ? "passed" : "failed",
        $"canPromoteWzAbsoluteFromVevScale={vevWzAbsoluteClosurePassed}; decision={JsonString(phase195.RootElement, "decision")}",
        Phase195Path),
    new AuditItem(
        "wz-weak-coupling-mass-relation",
        "The promoted weak coupling and electroweak mass-generation relation must pass W/Z absolute target comparison.",
        weakCouplingWzClosurePassed ? "passed" : "failed",
        $"canPromoteWzFromWeakCouplingMassRelation={weakCouplingWzClosurePassed}; decision={JsonString(phase197.RootElement, "decision")}",
        Phase197Path),
    new AuditItem(
        "wz-weak-coupling-source-lineage",
        "At least one weak-coupling source lineage must be promotable after fixture, replay, and target-implied routes are reconciled.",
        weakCouplingLineageClosurePassed ? "passed" : "failed",
        $"canPromoteAnyWeakCouplingSourceForWzAbsolute={weakCouplingLineageClosurePassed}; decision={JsonString(phase198.RootElement, "decision")}",
        Phase198Path),
    new AuditItem(
        "wz-external-electroweak-input-loophole-closed",
        "External or target-implied electroweak coupling inputs must not be treated as GU W/Z source-lineage predictions.",
        externalElectroweakInputLoopholeClosed ? "covered" : "failed",
        $"canPromoteExternalElectroweakBridge={JsonBool(phase214.RootElement, "canPromoteExternalElectroweakBridge")}; decision={JsonString(phase214.RootElement, "decision")}",
        Phase214Path),
    new AuditItem(
        "wz-direct-bridge-normalization",
        "The P190 direct bridge candidate combined with target-independent normalization must be theorem-promoted, split into W/Z rows, and pass downstream gates.",
        directBridgeNormalizationClosurePassed ? "passed" : "failed",
        $"canPromoteDirectBridgeNormalization={directBridgeNormalizationClosurePassed}; decision={JsonString(phase206.RootElement, "decision")}",
        Phase206Path),
    new AuditItem(
        "higgs-scalar-source",
        "Higgs mass requires a solved scalar-sector source/operator with target-independent identity and stability evidence.",
        p189PredictionAllowed ? "passed" : "failed",
        $"predictionAttemptAllowed={p189PredictionAllowed}; terminalStatus={JsonString(phase189.RootElement, "terminalStatus")}; decision={JsonString(phase189.RootElement, "decision")}",
        Phase189Path),
    new AuditItem(
        "higgs-potential-self-coupling-source",
        "Higgs mass requires a target-independent potential/self-coupling or scalar excitation source before physical mass comparison.",
        higgsPotentialClosurePassed ? "passed" : "failed",
        $"canPromoteHiggsFromPotentialOrSelfCoupling={higgsPotentialClosurePassed}; decision={JsonString(phase196.RootElement, "decision")}",
        Phase196Path),
    new AuditItem(
        "higgs-quartic-self-coupling-source-scan",
        "The repository must contain an intake-ready Higgs quartic/self-coupling source before Higgs mass can be promoted.",
        higgsQuarticSelfCouplingSourceScanPassed ? "passed" : "failed",
        $"canPromoteHiggsQuarticSelfCouplingSource={higgsQuarticSelfCouplingSourceScanPassed}; decision={JsonString(phase207.RootElement, "decision")}",
        Phase207Path),
    new AuditItem(
        "higgs-target-implied-self-coupling-loophole-closed",
        "Target-implied Higgs quartic/self-coupling replay must not be treated as a GU Higgs source-lineage prediction.",
        higgsTargetImpliedSelfCouplingLoopholeClosed ? "covered" : "failed",
        $"canPromoteTargetImpliedHiggsSelfCoupling={JsonBool(phase215.RootElement, "canPromoteTargetImpliedHiggsSelfCoupling")}; decision={JsonString(phase215.RootElement, "decision")}",
        Phase215Path),
    new AuditItem(
        "higgs-scalar-source-lineage",
        "At least one Higgs scalar-source lineage must be promotable after VEV, scalar-relation, scaffold, census, potential, and draft routes are reconciled.",
        higgsScalarSourceLineageClosurePassed ? "passed" : "failed",
        $"canPromoteAnyHiggsScalarSourceLineage={higgsScalarSourceLineageClosurePassed}; decision={JsonString(phase199.RootElement, "decision")}",
        Phase199Path),
    new AuditItem(
        "draft-source-evidence",
        "The local completion draft must provide the missing W/Z bridge law or solved Higgs scalar source before remaining masses can be promoted.",
        draftProvidesCompletionSource ? "passed" : "failed",
        $"draftProvidesDirectWzLaw={draftProvidesDirectWzLaw}; draftProvidesSolvedHiggsSource={draftProvidesSolvedHiggsSource}; conclusion={JsonString(phase194.RootElement, "conclusion")}",
        Phase194Path),
    new AuditItem(
        "official-public-source-evidence",
        "The official public GU site/draft audit must provide the missing W/Z bridge law or solved Higgs scalar source before remaining masses can be promoted from public source material.",
        officialDraftProvidesCompletionSource ? "passed" : "failed",
        $"officialDraftProvidesCompletionSource={officialDraftProvidesCompletionSource}; conclusion={JsonString(phase218.RootElement, "conclusion")}",
        Phase218Path),
    new AuditItem(
        "package-summary",
        "The top-level boson package must expose whether the prediction set is physically complete.",
        JsonBool(phase101.RootElement, "allKnownBosonValuesDefensible") is true ? "passed" : "failed",
        $"terminalStatus={JsonString(phase101.RootElement, "terminalStatus")}; externalPhysicalComparisonReady={JsonBool(phase101.RootElement, "externalPhysicalComparisonReady")}",
        Phase101Path),
};

var coveredItemIds = new[]
{
    "defensible-values-ledger",
    "wz-external-electroweak-input-loophole-closed",
    "higgs-target-implied-self-coupling-loophole-closed",
};
var missingOrFailed = checklist.Where(item => item.Status != "passed" && !coveredItemIds.Contains(item.Id)).ToArray();
var fixableAsImplementationDefect = false;
var terminalStatus = allKnownBosonValuesDefensible
    ? "boson-prediction-completion-audit-complete"
    : "boson-prediction-completion-audit-incomplete-scientific-source-gaps";

var result = new
{
    phaseId = "phase193-boson-prediction-completion-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    objective = "Figure out why the remaining known-boson values are not predictable and determine whether scientifically defensible values can be completed from current repository artifacts.",
    successCriteria = new[]
    {
        "All known boson observables are predicted, passed, and gate-promoted.",
        "W/Z absolute masses clear direct bridge source, raw-amplitude, particle-split, and target-comparison gates.",
        "The electroweak VEV/order-parameter scale closes W/Z absolute masses without target-fitted source-shape repair.",
        "The promoted weak coupling and mass-generation relation pass W/Z absolute target comparison.",
        "A weak-coupling source lineage remains promotable after fixture, replay, and target-implied routes are reconciled.",
        "External or target-implied electroweak coupling routes are explicitly blocked from promotion.",
        "The direct W/Z bridge candidate remains promotable after target-independent normalization closure.",
        "Higgs mass has a solved scalar source/operator and target-independent identity/stability evidence.",
        "Higgs mass has a target-independent potential/self-coupling or scalar excitation source.",
        "Repository scan finds an intake-ready Higgs quartic/self-coupling source.",
        "Target-implied Higgs quartic/self-coupling replay is explicitly blocked from promotion.",
        "A Higgs scalar-source lineage remains promotable after VEV, scalar-relation, scaffold, census, potential, and draft routes are reconciled.",
        "Draft/manuscript research supplies a missing completion source for W/Z or Higgs.",
        "Official public GU source research supplies a missing completion source for W/Z or Higgs.",
        "The top-level package reports allKnownBosonValuesDefensible=true.",
    },
    checklist,
    allSuccessCriteriaMet = missingOrFailed.Length == 0,
    fixableAsImplementationDefect,
    currentDefensibleValues = JsonArraySummary(phase192.RootElement, "defensibleValues"),
    unresolvedItems = missingOrFailed,
    conclusion = allKnownBosonValuesDefensible
        ? "The current repository artifacts complete the requested boson prediction set."
        : "This is not currently fixable as an implementation/reporting defect. The remaining W/Z absolute-mass and Higgs blockers are missing scientific source/derivation artifacts, not ignored promoted evidence.",
    nextRequiredWork = new[]
    {
        "W/Z: supply a derivation-backed direct bridge-source theorem or branch-local proof with separate W and Z source rows that clear raw-amplitude and target-comparison gates.",
        "W/Z: supply a normalization/source-shape law that remains compatible with common-scale and target-comparison gates after the direct bridge source is fixed.",
        "Higgs: supply a solved scalar-sector source/operator with identity envelopes, massive scalar profile, and stability sidecars before attempting a physical mass comparison.",
        "Higgs: supply explicit quartic/self-coupling or scalar-potential evidence rather than open issue, approximate draft, or blocked audit text.",
        "Source research: supply a public or local derivation-backed source artifact that satisfies the W/Z or Higgs Phase201/P209 gates.",
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "boson_prediction_completion_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "boson_prediction_completion_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.allSuccessCriteriaMet,
        result.fixableAsImplementationDefect,
        result.currentDefensibleValues,
        unresolvedItemCount = result.unresolvedItems.Length,
        result.unresolvedItems,
        result.conclusion,
        result.nextRequiredWork,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"allSuccessCriteriaMet={result.allSuccessCriteriaMet}");
Console.WriteLine($"fixableAsImplementationDefect={fixableAsImplementationDefect}");
Console.WriteLine($"unresolvedItemCount={missingOrFailed.Length}");

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

static IReadOnlyList<object> JsonArraySummary(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Array
        ? property.EnumerateArray()
            .Select(item => new
            {
                particleId = JsonString(item, "particleId"),
                observableId = JsonString(item, "observableId"),
                predictedValue = item.TryGetProperty("predictedValue", out var predictedValue) ? predictedValue.Clone() : default,
                predictedUncertainty = item.TryGetProperty("predictedUncertainty", out var predictedUncertainty) ? predictedUncertainty.Clone() : default,
                unit = JsonString(item, "unit"),
            })
            .Cast<object>()
            .ToArray()
        : Array.Empty<object>();

sealed record AuditItem(string Id, string Requirement, string Status, string Evidence, string EvidencePath);
