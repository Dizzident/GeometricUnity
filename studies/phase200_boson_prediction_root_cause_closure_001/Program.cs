using System.Text.Json;

const string DefaultOutputDir = "studies/phase200_boson_prediction_root_cause_closure_001/output";
const string Phase192Path = "studies/phase192_boson_scientific_defensibility_ledger_001/output/boson_scientific_defensibility_ledger_summary.json";
const string Phase193Path = "studies/phase193_boson_prediction_completion_audit_001/output/boson_prediction_completion_audit_summary.json";
const string Phase194Path = "studies/phase194_draft_boson_source_evidence_audit_001/output/draft_boson_source_evidence_audit_summary.json";
const string Phase198Path = "studies/phase198_weak_coupling_source_lineage_closure_audit_001/output/weak_coupling_source_lineage_closure_audit_summary.json";
const string Phase199Path = "studies/phase199_higgs_scalar_source_lineage_closure_audit_001/output/higgs_scalar_source_lineage_closure_audit_summary.json";
const string Phase214Path = "studies/phase214_external_electroweak_input_loophole_audit_001/output/external_electroweak_input_loophole_audit_summary.json";
const string Phase215Path = "studies/phase215_higgs_target_implied_self_coupling_loophole_audit_001/output/higgs_target_implied_self_coupling_loophole_audit_summary.json";
const string Phase218Path = "studies/phase218_official_gu_public_source_audit_001/output/official_gu_public_source_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE200_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase192 = JsonDocument.Parse(File.ReadAllText(Phase192Path));
using var phase193 = JsonDocument.Parse(File.ReadAllText(Phase193Path));
using var phase194 = JsonDocument.Parse(File.ReadAllText(Phase194Path));
using var phase198 = JsonDocument.Parse(File.ReadAllText(Phase198Path));
using var phase199 = JsonDocument.Parse(File.ReadAllText(Phase199Path));
using var phase214 = JsonDocument.Parse(File.ReadAllText(Phase214Path));
using var phase215 = JsonDocument.Parse(File.ReadAllText(Phase215Path));
using var phase218 = JsonDocument.Parse(File.ReadAllText(Phase218Path));

var allKnownBosonValuesDefensible = JsonBool(phase192.RootElement, "allKnownBosonValuesDefensible") is true;
var completionAuditPassed = JsonBool(phase193.RootElement, "allSuccessCriteriaMet") is true;
var weakCouplingLineagePromotable = JsonBool(phase198.RootElement, "canPromoteAnyWeakCouplingSourceForWzAbsolute") is true;
var higgsLineagePromotable = JsonBool(phase199.RootElement, "canPromoteAnyHiggsScalarSourceLineage") is true;
var draftProvidesCompletionSource = JsonBool(phase194.RootElement, "draftProvidesDirectWzLaw") is true
    || JsonBool(phase194.RootElement, "draftProvidesSolvedHiggsSource") is true;
var officialDraftProvidesCompletionSource = JsonBool(phase218.RootElement, "officialDraftProvidesCompletionSource") is true;
var externalElectroweakInputLoopholeClosed = JsonBool(phase214.RootElement, "canPromoteExternalElectroweakBridge") is false;
var higgsTargetImpliedSelfCouplingLoopholeClosed = JsonBool(phase215.RootElement, "canPromoteTargetImpliedHiggsSelfCoupling") is false;

var defensibleValues = phase193.RootElement.GetProperty("currentDefensibleValues")
    .EnumerateArray()
    .Select(row => new
    {
        particleId = JsonString(row, "particleId"),
        observableId = JsonString(row, "observableId"),
        predictedValue = row.GetProperty("predictedValue").Clone(),
        predictedUncertainty = row.GetProperty("predictedUncertainty").Clone(),
        unit = JsonString(row, "unit"),
    })
    .ToArray();

var unresolvedIds = phase193.RootElement.GetProperty("unresolvedItems")
    .EnumerateArray()
    .Select(row => JsonString(row, "id") ?? "")
    .Where(id => id.Length > 0)
    .ToArray();

var rootCauses = new[]
{
    new RootCause(
        "wz-absolute-source-lineage-gap",
        "W and Z absolute masses are not predictable because no current W/Z source lineage is both target-independent and promotable.",
        "failed",
        new[]
        {
            "direct bridge candidate exists but is not theorem/promoted, does not clear raw-amplitude gate, and has no particle-specific W/Z split",
            "VEV/order-parameter scale exists but does not close source-shape/common-scale/target-comparison gates",
            "promoted weak-coupling mass relation fails W/Z absolute target comparison",
            "weak-coupling source-lineage audit rejects the fixture value, replayed value, and target-implied value",
        },
        new
        {
            weakCouplingLineagePromotable,
            phase198Path = Phase198Path,
            unresolved = unresolvedIds.Where(id => id.StartsWith("wz-", StringComparison.Ordinal)).ToArray(),
        }),
    new RootCause(
        "higgs-scalar-source-lineage-gap",
        "Higgs mass is not predictable because no solved scalar-sector source/operator, identity envelope, massive scalar profile, or potential/self-coupling lineage is promotable.",
        "failed",
        new[]
        {
            "VEV/order-parameter bridge is not a Higgs excitation source",
            "scalar relation repair is diagnostic-only",
            "Higgs scaffold is fail-closed and not identity-validated",
            "source/operator census finds zero scalar feature envelopes, zero branch-stable non-C0 scalar candidates, and zero massive scalar profiles",
            "draft Higgs material is open/conjectural/approximate rather than a solved source",
        },
        new
        {
            higgsLineagePromotable,
            phase199Path = Phase199Path,
            unresolved = unresolvedIds.Where(id => id.StartsWith("higgs-", StringComparison.Ordinal)).ToArray(),
        }),
    new RootCause(
        "draft-does-not-close-source-gaps",
        "The local completion draft cannot be used as a promotion source for the missing boson values.",
        "failed",
        new[]
        {
            "draft provides no W/Z direct bridge theorem",
            "draft provides no solved Higgs source",
            "draft records relevant Higgs/Yukawa/mixed-linearization material as open, approximate, conjectural, or proof-obligation material",
        },
        new
        {
            draftProvidesCompletionSource,
            phase194Path = Phase194Path,
        }),
    new RootCause(
        "official-public-gu-source-does-not-close-source-gaps",
        "The checked official public GU site and April 1, 2021 public draft passages cannot be used as promotion sources for the missing boson values.",
        "failed",
        new[]
        {
            "official public-source audit finds no W/Z direct bridge theorem",
            "official public-source audit finds no solved Higgs scalar source/operator",
            "checked public passages support the architectural research program but not Phase201/P209/P210 source-lineage evidence",
        },
        new
        {
            officialDraftProvidesCompletionSource,
            phase218Path = Phase218Path,
        }),
    new RootCause(
        "wz-external-electroweak-input-loophole-closed",
        "W/Z absolute masses cannot be completed by importing or target-implying the electroweak weak coupling.",
        externalElectroweakInputLoopholeClosed ? "covered" : "failed",
        new[]
        {
            "Fermi-derived VEV is an external-disjoint scale input, not a GU W/Z source law",
            "current promoted GU weak coupling predicts W/Z masses that fail target comparison",
            "target-implied weak coupling is diagnostic-only and cannot fill the source-lineage contract",
        },
        new
        {
            externalElectroweakInputLoopholeClosed,
            phase214Path = Phase214Path,
        }),
    new RootCause(
        "higgs-target-implied-self-coupling-loophole-closed",
        "Higgs mass cannot be completed by deriving a quartic/self-coupling from the observed Higgs target and replaying it as a prediction.",
        higgsTargetImpliedSelfCouplingLoopholeClosed ? "covered" : "failed",
        new[]
        {
            "target-implied Higgs quartic uses the observed Higgs mass by construction",
            "the replayed mass equals the target but supplies no solved scalar source/operator",
            "the Higgs source-lineage contract still lacks identity envelope, massive profile, coupling/excitation source, and stability sidecars",
        },
        new
        {
            higgsTargetImpliedSelfCouplingLoopholeClosed,
            phase215Path = Phase215Path,
        }),
};

var rootCauseClosureComplete = !allKnownBosonValuesDefensible
    && !completionAuditPassed
    && rootCauses.All(cause => cause.Status is "failed" or "covered")
    && rootCauses.Any(cause => cause.Status == "failed")
    && defensibleValues.Length == 3;

var terminalStatus = rootCauseClosureComplete
    ? "boson-prediction-root-cause-closure-complete-source-gaps"
    : allKnownBosonValuesDefensible
        ? "boson-prediction-root-cause-closure-complete-all-values-defensible"
        : "boson-prediction-root-cause-closure-incomplete";

var result = new
{
    phaseId = "phase200-boson-prediction-root-cause-closure",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    rootCauseClosureComplete,
    allKnownBosonValuesDefensible,
    completionAuditPassed,
    defensibleValues,
    unresolvedIds,
    rootCauses,
    answer = rootCauseClosureComplete
        ? "The remaining bosons are not predictable from current repository artifacts or the checked official public GU draft passages because the required source lineages are absent: W/Z absolute masses lack a promotable target-independent bridge/weak-coupling/source-shape lineage, and Higgs lacks a solved scalar-sector source/operator lineage. Numeric shortcuts are also closed: external/target-implied W/Z weak coupling and target-implied Higgs quartic/self-coupling are diagnostic-only non-predictions. The current defensible values are limited to W/Z ratio plus photon/gluon protected masslessness."
        : "Root-cause closure is not complete; inspect unresolved items before making a scientific conclusion.",
    nextRequiredArtifacts = new[]
    {
        "W/Z: derivation-backed target-independent direct bridge/source lineage with separate W and Z rows that clears raw-amplitude, common-bridge, and target-comparison gates.",
        "Higgs: solved scalar-sector source/operator lineage with Higgs identity envelopes, massive scalar profile, potential/self-coupling or excitation relation, and stability sidecars.",
    },
    sourceEvidence = new
    {
        phase192Path = Phase192Path,
        phase193Path = Phase193Path,
        phase194Path = Phase194Path,
        phase198Path = Phase198Path,
        phase199Path = Phase199Path,
        phase214Path = Phase214Path,
        phase215Path = Phase215Path,
        phase218Path = Phase218Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "boson_prediction_root_cause_closure.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "boson_prediction_root_cause_closure_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.rootCauseClosureComplete,
        result.allKnownBosonValuesDefensible,
        result.completionAuditPassed,
        result.defensibleValues,
        result.unresolvedIds,
        result.rootCauses,
        result.answer,
        result.nextRequiredArtifacts,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"rootCauseClosureComplete={rootCauseClosureComplete}");
Console.WriteLine($"defensibleValueCount={defensibleValues.Length}");
Console.WriteLine($"unresolvedItemCount={unresolvedIds.Length}");

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

sealed record RootCause(
    string RootCauseId,
    string Finding,
    string Status,
    IReadOnlyList<string> Evidence,
    object Metrics);
