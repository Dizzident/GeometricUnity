using System.Text.Json;

const string DefaultOutputDir = "studies/phase245_rank_deficit_minimal_unlock_contract_001/output";
const string Phase209Path = "studies/phase209_boson_source_lineage_evidence_request_package_001/output/boson_source_lineage_evidence_request_package_summary.json";
const string Phase210Path = "studies/phase210_boson_source_lineage_evidence_application_gate_001/output/boson_source_lineage_evidence_application_gate_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase224Path = "studies/phase224_electroweak_parameter_dependency_audit_001/output/electroweak_parameter_dependency_audit_summary.json";
const string Phase244Path = "studies/phase244_electroweak_identifiability_rank_audit_001/output/electroweak_identifiability_rank_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE245_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase209 = JsonDocument.Parse(File.ReadAllText(Phase209Path));
using var phase210 = JsonDocument.Parse(File.ReadAllText(Phase210Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase224 = JsonDocument.Parse(File.ReadAllText(Phase224Path));
using var phase244 = JsonDocument.Parse(File.ReadAllText(Phase244Path));

var remainingNullity = JsonInt(phase244.RootElement, "remainingNullity") ?? -1;
var minimumAdditionalIndependentSourceConstraints = JsonInt(phase244.RootElement, "minimumAdditionalIndependentSourceConstraints") ?? -1;
var currentPromotedConstraintRank = JsonInt(phase244.RootElement, "currentPromotedConstraintRank") ?? -1;
var rankAuditPassed = JsonBool(phase244.RootElement, "electroweakIdentifiabilityRankAuditPassed") is true;
var rankAuditPromotableForBosonMasses = JsonBool(phase244.RootElement, "rankAuditPromotableForBosonMasses") is true;
var rerunPromotionAllowed = JsonBool(phase210.RootElement, "rerunPromotionAllowed") is true;
var wzReadyForApplication = phase210.RootElement.TryGetProperty("wzApplication", out var wzApplication)
    && JsonBool(wzApplication, "readyForApplication") is true;
var higgsReadyForApplication = phase210.RootElement.TryGetProperty("higgsApplication", out var higgsApplication)
    && JsonBool(higgsApplication, "readyForApplication") is true;
var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? 0;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? 0;

var p224Closure = phase224.RootElement.GetProperty("closure");
var wAbsoluteMassParameterClosure = JsonBool(p224Closure, "wAbsoluteMassParameterClosure") is true;
var zAbsoluteMassParameterClosure = JsonBool(p224Closure, "zAbsoluteMassParameterClosure") is true;
var higgsMassParameterClosure = JsonBool(p224Closure, "higgsMassParameterClosure") is true;

var unlockRows = new[]
{
    new UnlockRow(
        "wz-absolute-scale-unlock",
        "common-wz-scale-direction",
        "log(v g), or equivalent independent GU rows for v and g",
        Filled: wAbsoluteMassParameterClosure && zAbsoluteMassParameterClosure,
        RequiredEvidence: new[]
        {
            "externalTargetValuesUsed=false",
            "theoremOrDerivationId",
            "sourceLineageId",
            "separate W and Z sourceRowId values",
            "W and Z rawAmplitudeGatePassed=true",
            "W and Z commonBridgeGatePassed=true",
            "W and Z stabilitySidecarsPresent=true",
            "post-construction W and Z targetComparisonGatePassed=true",
        },
        PhaseRequestPath: JsonString(phase209.RootElement, "wzRequestPath") ?? "studies/phase209_boson_source_lineage_evidence_request_package_001/output/wz_absolute_source_lineage_evidence_request.json",
        Detail: "This unlock must fix the common W/Z absolute mass scale independently of target W/Z masses. The existing W/Z ratio only fixes weak mixing."),
    new UnlockRow(
        "higgs-scalar-scale-unlock",
        "higgs-scale-direction",
        "log(v sqrt(lambda)), or equivalent independent GU rows for v and scalar self-coupling lambda",
        Filled: higgsMassParameterClosure,
        RequiredEvidence: new[]
        {
            "externalTargetValuesUsed=false",
            "sourceLineageId",
            "scalarSourceOperatorId",
            "higgsIdentityEnvelopeId",
            "massiveScalarProfileId",
            "potentialOrSelfCouplingSourceId-or-excitationRelationId",
            "branch/refinement/environment/representation/coupling stability sidecars",
            "predictionRow.sourceRowId",
            "post-construction predictionRow.targetComparisonGatePassed=true",
        },
        PhaseRequestPath: JsonString(phase209.RootElement, "higgsRequestPath") ?? "studies/phase209_boson_source_lineage_evidence_request_package_001/output/higgs_scalar_source_lineage_evidence_request.json",
        Detail: "This unlock must fix the Higgs scalar scale from a solved GU scalar source, not from the observed Higgs target mass or a numerical factor alone."),
};

var rejectedSubstitutes = new[]
{
    new RejectedSubstitute("w-z-ratio-only", "rank-deficient", "The ratio fixes weak mixing but leaves the common W/Z absolute scale free."),
    new RejectedSubstitute("external-fermi-vev", "external-input", "The Fermi-derived electroweak scale is comparison context unless a GU vacuum/VEV source lineage derives it."),
    new RejectedSubstitute("target-w-or-z-mass-inversion", "target-leakage", "Solving for v or g from observed W/Z masses is not a target-independent prediction."),
    new RejectedSubstitute("su2-casimir-numerical-coupling", "nonpromotable-diagnostic", "The current SU(2) Casimir coupling is numerically close but has sourceLineagePromotable=false."),
    new RejectedSubstitute("higgs-target-implied-quartic", "target-leakage", "Solving lambda from observed Higgs mass is not a scalar-source derivation."),
    new RejectedSubstitute("higgs-three-tenths-numerical-factor", "nonpromotable-diagnostic", "The 3/10 lead is numerical and lacks scalarSourceOperatorId, potential/source lineage, and stability sidecars."),
    new RejectedSubstitute("quartic-gauge-sign-falsifier", "category-error", "An anomalous quartic gauge-coupling sign constraint is not the Higgs scalar quartic lambda."),
    new RejectedSubstitute("symbolic-cox-ii-formula-with-free-parameters", "parameterized-not-prediction", "Symbolic formula structure does not fix the independent source parameters required by the rank audit."),
};

var unlockContractFilled = unlockRows.All(row => row.Filled);
var allRejectedSubstitutesRejected = rejectedSubstitutes.All(row => row.RejectionClass.Length > 0);
var minimalUnlockContractMaterialized = rankAuditPassed
    && currentPromotedConstraintRank == 1
    && remainingNullity == 2
    && minimumAdditionalIndependentSourceConstraints == unlockRows.Length
    && allRejectedSubstitutesRejected;
var newSourceEvidenceStillRequired = minimalUnlockContractMaterialized
    && !unlockContractFilled
    && !rerunPromotionAllowed
    && wzMissingFieldCount > 0
    && higgsMissingFieldCount > 0;

var checks = new[]
{
    new Check("rank-deficit-input-valid", rankAuditPassed && !rankAuditPromotableForBosonMasses && currentPromotedConstraintRank == 1 && remainingNullity == 2, $"rankAuditPassed={rankAuditPassed}; rankAuditPromotableForBosonMasses={rankAuditPromotableForBosonMasses}; currentPromotedConstraintRank={currentPromotedConstraintRank}; remainingNullity={remainingNullity}"),
    new Check("two-minimal-unlock-rows-present", unlockRows.Length == minimumAdditionalIndependentSourceConstraints && unlockRows.Any(row => row.UnlockId == "wz-absolute-scale-unlock") && unlockRows.Any(row => row.UnlockId == "higgs-scalar-scale-unlock"), $"unlockRowCount={unlockRows.Length}; minimumAdditionalIndependentSourceConstraints={minimumAdditionalIndependentSourceConstraints}"),
    new Check("current-unlocks-unfilled", unlockRows.All(row => !row.Filled) && !wAbsoluteMassParameterClosure && !zAbsoluteMassParameterClosure && !higgsMassParameterClosure, $"wClosure={wAbsoluteMassParameterClosure}; zClosure={zAbsoluteMassParameterClosure}; higgsClosure={higgsMassParameterClosure}"),
    new Check("rejected-substitutes-materialized", allRejectedSubstitutesRejected && rejectedSubstitutes.Length >= 8, $"rejectedSubstituteCount={rejectedSubstitutes.Length}"),
    new Check("phase209-requests-linked", !string.IsNullOrWhiteSpace(JsonString(phase209.RootElement, "wzRequestPath")) && !string.IsNullOrWhiteSpace(JsonString(phase209.RootElement, "higgsRequestPath")), $"wzRequestPath={JsonString(phase209.RootElement, "wzRequestPath")}; higgsRequestPath={JsonString(phase209.RootElement, "higgsRequestPath")}"),
    new Check("promotion-rerun-still-blocked", !rerunPromotionAllowed && !wzReadyForApplication && !higgsReadyForApplication, $"rerunPromotionAllowed={rerunPromotionAllowed}; wzReadyForApplication={wzReadyForApplication}; higgsReadyForApplication={higgsReadyForApplication}"),
    new Check("source-lineage-blockers-preserved", wzMissingFieldCount == 15 && higgsMissingFieldCount == 14, $"wzMissingFieldCount={wzMissingFieldCount}; higgsMissingFieldCount={higgsMissingFieldCount}"),
    new Check("new-source-evidence-still-required", newSourceEvidenceStillRequired, $"newSourceEvidenceStillRequired={newSourceEvidenceStillRequired}"),
};

var rankDeficitMinimalUnlockContractPassed = checks.All(check => check.Passed)
    && minimalUnlockContractMaterialized
    && !unlockContractFilled;
var terminalStatus = rankDeficitMinimalUnlockContractPassed
    ? "rank-deficit-minimal-unlock-contract-ready-source-evidence-required"
    : "rank-deficit-minimal-unlock-contract-review-required";

var result = new
{
    phaseId = "phase245-rank-deficit-minimal-unlock-contract",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    rankDeficitMinimalUnlockContractPassed,
    minimalUnlockContractMaterialized,
    unlockContractFilled,
    newSourceEvidenceStillRequired,
    currentPromotedConstraintRank,
    remainingNullity,
    minimumAdditionalIndependentSourceConstraints,
    objective = "Convert the Phase244 electroweak rank deficit into the minimal source-lineage evidence that would actually unlock W/Z/H absolute predictions.",
    unlockRows,
    rejectedSubstitutes,
    checks,
    decision = rankDeficitMinimalUnlockContractPassed
        ? "Do not rerun promotion from current evidence. The minimal unlock contract requires two new independent target-independent source artifacts: a W/Z absolute-scale source and a Higgs scalar-scale source. Current substitutes are either target leakage, diagnostic-only, category errors, or parameterized formulas."
        : "Review minimal unlock contract before relying on it.",
    nextRequiredArtifact = new[]
    {
        "Fill the W/Z absolute-scale unlock with a derivation-backed GU source for log(vg), or independent GU rows for v and g, satisfying Phase209 W/Z gates.",
        "Fill the Higgs scalar-scale unlock with a solved GU scalar source for log(v sqrt(lambda)), or independent GU rows for v and lambda, satisfying Phase209 Higgs gates.",
        "Only after both rank-deficit unlock rows and the Phase201/Phase209/Phase210/Phase213 gates pass should P101/P202 be allowed to claim W/Z/H absolute mass completion.",
    },
    sourceEvidence = new
    {
        phase209Path = Phase209Path,
        phase210Path = Phase210Path,
        phase213Path = Phase213Path,
        phase224Path = Phase224Path,
        phase244Path = Phase244Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "rank_deficit_minimal_unlock_contract.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "rank_deficit_minimal_unlock_contract_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.rankDeficitMinimalUnlockContractPassed,
        result.minimalUnlockContractMaterialized,
        result.unlockContractFilled,
        result.newSourceEvidenceStillRequired,
        result.currentPromotedConstraintRank,
        result.remainingNullity,
        result.minimumAdditionalIndependentSourceConstraints,
        result.unlockRows,
        result.rejectedSubstitutes,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"rankDeficitMinimalUnlockContractPassed={rankDeficitMinimalUnlockContractPassed}");
Console.WriteLine($"unlockContractFilled={unlockContractFilled}");
Console.WriteLine($"newSourceEvidenceStillRequired={newSourceEvidenceStillRequired}");

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

sealed record UnlockRow(string UnlockId, string NullDirectionId, string RequiredCoordinate, bool Filled, string[] RequiredEvidence, string PhaseRequestPath, string Detail);
sealed record RejectedSubstitute(string SubstituteId, string RejectionClass, string Reason);
sealed record Check(string CheckId, bool Passed, string Detail);
