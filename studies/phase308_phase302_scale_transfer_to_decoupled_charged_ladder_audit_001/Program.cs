using System.Text.Json;

const string DefaultOutputDir = "studies/phase308_phase302_scale_transfer_to_decoupled_charged_ladder_audit_001/output";
const string Phase201Path = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase225Path = "studies/phase225_su2_normalization_representation_compatibility_audit_001/output/su2_normalization_representation_compatibility_audit_summary.json";
const string Phase249Path = "studies/phase249_invariant_origin_search_for_near_miss_constants_001/output/invariant_origin_search_for_near_miss_constants_summary.json";
const string Phase302Path = "studies/phase302_identity_split_particle_normalization_audit_001/output/identity_split_particle_normalization_audit_summary.json";
const string Phase306Path = "studies/phase306_decoupled_charged_ladder_wz_row_source_audit_001/output/decoupled_charged_ladder_wz_row_source_audit_summary.json";
const string Phase307Path = "studies/phase307_target_independent_decoupled_wz_row_selection_law_audit_001/output/target_independent_decoupled_wz_row_selection_law_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE308_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase225 = JsonDocument.Parse(File.ReadAllText(Phase225Path));
using var phase249 = JsonDocument.Parse(File.ReadAllText(Phase249Path));
using var phase302 = JsonDocument.Parse(File.ReadAllText(Phase302Path));
using var phase306 = JsonDocument.Parse(File.ReadAllText(Phase306Path));
using var phase307 = JsonDocument.Parse(File.ReadAllText(Phase307Path));

var p302Best = phase302.RootElement.GetProperty("bestSourceInvariantRawCommonCandidate");
double p302CommonScaleValue = RequiredDouble(p302Best, "commonScaleValue");
double p302WParticleMultiplier = RequiredDouble(p302Best, "wParticleMultiplier");
double p302ZParticleMultiplier = RequiredDouble(p302Best, "zParticleMultiplier");
double p302WTotalScale = RequiredDouble(p302Best, "wTotalScale");
double p302ZTotalScale = RequiredDouble(p302Best, "zTotalScale");
string p302CommonScaleId = RequiredString(p302Best, "commonScaleId");
string p302ParticleLawId = RequiredString(p302Best, "particleLawId");
bool p302CommonScaleApplicationTheoremPresent = JsonBool(p302Best, "commonScaleApplicationTheoremPresent") is true;
bool p302ParticleLawApplicationTheoremPresent = JsonBool(p302Best, "particleLawApplicationTheoremPresent") is true;
bool p302PromotionEligible = JsonBool(p302Best, "promotionEligible") is true;
bool p302CanFillContract = JsonBool(phase302.RootElement, "canFillPhase201WzContract") is true;
bool p306CanFillContract = JsonBool(phase306.RootElement, "canFillPhase201WzContract") is true;
bool p307CanFillContract = JsonBool(phase307.RootElement, "canFillPhase201WzContract") is true;
bool p225ObstructionCertified = JsonBool(phase225.RootElement, "representationNormalizationObstructionCertified") is true;
bool p249WzInvariantSourceBacked = JsonBool(phase249.RootElement, "wzInvariantFormulaSourceBacked") is true;
bool phase201AllRequiredLineagesPromotable = JsonBool(phase201.RootElement, "allRequiredLineagesPromotable") is true;
int wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
int higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;

bool targetObservablesUsedForConstruction = false;
bool targetValuesUsedOnlyForPostTransferEvaluation = true;
bool scaleTransferTheoremClaimed = false;
bool scaleTransferAllowed = false;
bool transferredScaleSourceRowsPromotable = false;
bool canFillPhase201WzContract = false;

var transferApplications = new[]
{
    new TransferApplication(
        "phase306-decoupled-charged-ladder-row-near-pass",
        "Apply the Phase302 W/Z particle scales to independently paired charged-ladder W and Z rows.",
        JsonBool(phase306.RootElement, "decoupledChargedLadderWzRowSourceAuditPassed") is true,
        JsonInt(phase306.RootElement, "decoupledRawCommonPassingAssessmentCount") ?? -1,
        JsonInt(phase306.RootElement, "decoupledP302ScaledCommonPassingAssessmentCount") ?? -1,
        JsonBool(phase306.RootElement, "numericalP302ScaledDecoupledNearPassPresent") is true,
        JsonBool(phase306.RootElement, "canFillPhase201WzContract") is true),
    new TransferApplication(
        "phase307-target-independent-selector-near-pass",
        "Apply the Phase302 W/Z particle scales inside target-independent decoupled charged-ladder row selectors.",
        JsonBool(phase307.RootElement, "targetIndependentDecoupledWzRowSelectionLawAuditPassed") is true,
        JsonInt(phase307.RootElement, "rawStableCommonSelectionLawCount") ?? -1,
        JsonInt(phase307.RootElement, "p302ScaledStableCommonSelectionLawCount") ?? -1,
        (JsonInt(phase307.RootElement, "p302ScaledNearPassWithoutRawSelectionLawCount") ?? 0) > 0,
        JsonBool(phase307.RootElement, "canFillPhase201WzContract") is true),
};

var checks = new[]
{
    new Check(
        "phase302-scale-lead-materialized",
        p302CommonScaleId == "source-mode-vector-length"
            && p302ParticleLawId == "adjoint-casimir-over-fundamental-casimir"
            && Math.Abs(p302CommonScaleValue - 156.0) < 1.0e-12
            && Math.Abs(p302WParticleMultiplier - (8.0 / 3.0)) < 1.0e-12
            && Math.Abs(p302ZParticleMultiplier - 1.0) < 1.0e-12
            && Math.Abs(p302WTotalScale - 416.0) < 1.0e-12
            && Math.Abs(p302ZTotalScale - 156.0) < 1.0e-12,
        $"commonScaleId={p302CommonScaleId}; particleLawId={p302ParticleLawId}; commonScaleValue={p302CommonScaleValue:R}; wMultiplier={p302WParticleMultiplier:R}; zMultiplier={p302ZParticleMultiplier:R}; wTotalScale={p302WTotalScale:R}; zTotalScale={p302ZTotalScale:R}"),
    new Check(
        "phase302-scale-lead-is-not-promotable-at-source",
        !p302CommonScaleApplicationTheoremPresent
            && !p302ParticleLawApplicationTheoremPresent
            && !p302PromotionEligible
            && !p302CanFillContract
            && p225ObstructionCertified
            && !p249WzInvariantSourceBacked,
        $"commonScaleApplicationTheoremPresent={p302CommonScaleApplicationTheoremPresent}; particleLawApplicationTheoremPresent={p302ParticleLawApplicationTheoremPresent}; p302PromotionEligible={p302PromotionEligible}; p302CanFillContract={p302CanFillContract}; p225ObstructionCertified={p225ObstructionCertified}; p249WzInvariantSourceBacked={p249WzInvariantSourceBacked}"),
    new Check(
        "charged-ladder-transfer-is-only-numerical",
        transferApplications.All(application => application.AuditPassed)
            && transferApplications.Any(application => application.P302ScaledPassingCount > 0)
            && transferApplications.All(application => application.UnscaledRawPassingCount == 0)
            && transferApplications.All(application => !application.CanFillPhase201WzContract),
        string.Join("; ", transferApplications.Select(application => $"{application.ApplicationId}:auditPassed={application.AuditPassed},unscaledRawPassingCount={application.UnscaledRawPassingCount},p302ScaledPassingCount={application.P302ScaledPassingCount},scaledNearPassPresent={application.ScaledNearPassPresent},canFillPhase201WzContract={application.CanFillPhase201WzContract}"))),
    new Check(
        "no-scale-transfer-theorem-or-contract-fields",
        !scaleTransferTheoremClaimed
            && !scaleTransferAllowed
            && !transferredScaleSourceRowsPromotable
            && !canFillPhase201WzContract
            && !phase201AllRequiredLineagesPromotable
            && wzMissingFieldCount > 0,
        $"scaleTransferTheoremClaimed={scaleTransferTheoremClaimed}; scaleTransferAllowed={scaleTransferAllowed}; transferredScaleSourceRowsPromotable={transferredScaleSourceRowsPromotable}; canFillPhase201WzContract={canFillPhase201WzContract}; phase201AllRequiredLineagesPromotable={phase201AllRequiredLineagesPromotable}; wzMissingFieldCount={wzMissingFieldCount}"),
};

bool phase302ScaleTransferToDecoupledChargedLadderAuditPassed =
    checks.All(check => check.Passed)
    && !scaleTransferAllowed
    && !transferredScaleSourceRowsPromotable
    && !canFillPhase201WzContract;

var terminalStatus = phase302ScaleTransferToDecoupledChargedLadderAuditPassed
    ? "phase302-scale-transfer-to-decoupled-charged-ladder-audit-transfer-not-promotable"
    : "phase302-scale-transfer-to-decoupled-charged-ladder-audit-review-required";

var result = new
{
    phaseId = "phase308-phase302-scale-transfer-to-decoupled-charged-ladder-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    phase302ScaleTransferToDecoupledChargedLadderAuditPassed,
    targetObservablesUsedForConstruction,
    targetValuesUsedOnlyForPostTransferEvaluation,
    p302CommonScaleId,
    p302ParticleLawId,
    p302CommonScaleValue,
    p302WParticleMultiplier,
    p302ZParticleMultiplier,
    p302WTotalScale,
    p302ZTotalScale,
    p302CommonScaleApplicationTheoremPresent,
    p302ParticleLawApplicationTheoremPresent,
    p302PromotionEligible,
    p302CanFillPhase201WzContract = p302CanFillContract,
    p306CanFillPhase201WzContract = p306CanFillContract,
    p307CanFillPhase201WzContract = p307CanFillContract,
    p225ObstructionCertified,
    p249WzInvariantSourceBacked,
    transferApplications,
    scaleTransferTheoremClaimed,
    scaleTransferAllowed,
    transferredScaleSourceRowsPromotable,
    canFillPhase201WzContract,
    phase201AllRequiredLineagesPromotable,
    wzMissingFieldCount,
    higgsMissingFieldCount,
    checks,
    decision = phase302ScaleTransferToDecoupledChargedLadderAuditPassed
        ? "Do not transfer the Phase302 source-mode-vector-length and adjoint/fundamental Casimir scale into the decoupled charged-ladder W/Z selector as a prediction law. The transfer produces useful numerical near-passes downstream, but the original scale lead has no application theorem, Phase225/Phase249 still block the invariant as source-backed W/Z lineage, and the charged-ladder transfer does not clear the unscaled raw/common gate or fill Phase201/P209 contract fields."
        : "Review the Phase302 scale transfer audit before relying on Phase302-scaled charged-ladder results.",
    nextRequiredArtifact = new[]
    {
        "A theorem deriving the source-mode-vector-length normalization and W-specific 8/3 Casimir application for the physical W/Z source rows.",
        "A transfer theorem showing why that scale applies to the charged-ladder decoupled row family before target comparison.",
        "Branch-stable W and Z source rows that clear raw/common gates without target-fitted scaling.",
        "Phase201/P209 W/Z source-lineage rows with derivation, raw sidecars, common-bridge sidecars, target-comparison sidecars, and stability sidecars filled.",
    },
    sourceEvidence = new
    {
        phase201Path = Phase201Path,
        phase213Path = Phase213Path,
        phase225Path = Phase225Path,
        phase249Path = Phase249Path,
        phase302Path = Phase302Path,
        phase306Path = Phase306Path,
        phase307Path = Phase307Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "phase302_scale_transfer_to_decoupled_charged_ladder_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "phase302_scale_transfer_to_decoupled_charged_ladder_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.phase302ScaleTransferToDecoupledChargedLadderAuditPassed,
        result.targetObservablesUsedForConstruction,
        result.targetValuesUsedOnlyForPostTransferEvaluation,
        result.p302CommonScaleId,
        result.p302ParticleLawId,
        result.p302CommonScaleValue,
        result.p302WParticleMultiplier,
        result.p302ZParticleMultiplier,
        result.p302WTotalScale,
        result.p302ZTotalScale,
        result.p302CommonScaleApplicationTheoremPresent,
        result.p302ParticleLawApplicationTheoremPresent,
        result.p302PromotionEligible,
        result.p302CanFillPhase201WzContract,
        result.p306CanFillPhase201WzContract,
        result.p307CanFillPhase201WzContract,
        result.p225ObstructionCertified,
        result.p249WzInvariantSourceBacked,
        result.transferApplications,
        result.scaleTransferTheoremClaimed,
        result.scaleTransferAllowed,
        result.transferredScaleSourceRowsPromotable,
        result.canFillPhase201WzContract,
        result.phase201AllRequiredLineagesPromotable,
        result.wzMissingFieldCount,
        result.higgsMissingFieldCount,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"phase302ScaleTransferToDecoupledChargedLadderAuditPassed={phase302ScaleTransferToDecoupledChargedLadderAuditPassed}");
Console.WriteLine($"p302CommonScaleId={p302CommonScaleId}");
Console.WriteLine($"p302ParticleLawId={p302ParticleLawId}");
Console.WriteLine($"p302WTotalScale={p302WTotalScale:R}");
Console.WriteLine($"p302ZTotalScale={p302ZTotalScale:R}");
Console.WriteLine($"p302CommonScaleApplicationTheoremPresent={p302CommonScaleApplicationTheoremPresent}");
Console.WriteLine($"p302ParticleLawApplicationTheoremPresent={p302ParticleLawApplicationTheoremPresent}");
Console.WriteLine($"p306CanFillPhase201WzContract={p306CanFillContract}");
Console.WriteLine($"p307CanFillPhase201WzContract={p307CanFillContract}");
Console.WriteLine($"scaleTransferAllowed={scaleTransferAllowed}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");

static string RequiredString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
        ? property.GetString() ?? ""
        : throw new InvalidDataException($"Missing string property '{propertyName}'.");

static double RequiredDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var value)
        ? value
        : throw new InvalidDataException($"Missing numeric property '{propertyName}'.");

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

sealed record Check(string CheckId, bool Passed, string Detail);

sealed record TransferApplication(
    string ApplicationId,
    string Description,
    bool AuditPassed,
    int UnscaledRawPassingCount,
    int P302ScaledPassingCount,
    bool ScaledNearPassPresent,
    bool CanFillPhase201WzContract);
