using System.Text.Json;

const string DefaultOutputDir = "studies/phase103_target_scoped_falsifier_policy_001/output";
const string Phase47AuditPath = "studies/phase47_wz_physical_claim_falsifier_relevance_001/physical_claim_falsifier_relevance_audit.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE103_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var audit = JsonDocument.Parse(File.ReadAllText(Phase47AuditPath));
int targetRelevant = JsonInt(audit.RootElement, "targetRelevantSevereFalsifierCount") ?? int.MaxValue;
int globalSidecar = JsonInt(audit.RootElement, "globalSidecarSevereFalsifierCount") ?? 0;

bool targetScopedPolicyAdopted = targetRelevant == 0;
var policy = new
{
    phaseId = "phase103-target-scoped-falsifier-policy",
    terminalStatus = targetScopedPolicyAdopted
        ? "target-scoped-falsifier-policy-adopted"
        : "target-scoped-falsifier-policy-blocked",
    policyId = "phase103-target-scoped-physical-claim-policy-v1",
    targetObservableId = JsonString(audit.RootElement, "targetObservableId"),
    targetScopedPolicyAdopted,
    targetRelevantSevereFalsifierCount = targetRelevant,
    globalSidecarSevereFalsifierCount = globalSidecar,
    targetScopedPhysicalComparisonAllowedByFalsifierPolicy = targetScopedPolicyAdopted,
    unrestrictedPhysicalBosonLanguageAllowed = targetScopedPolicyAdopted && globalSidecar == 0,
    policyRules = new[]
    {
        "Any target-relevant fatal/high falsifier blocks the target-scoped physical comparison.",
        "Global sidecar fatal/high falsifiers must be disclosed in every physical-comparison package.",
        "Global sidecar falsifiers continue to block unrestricted global physical-boson prediction language.",
        "This policy does not satisfy mapping, calibration, claim-class, or quantitative-comparison gates.",
    },
    sourceAuditPath = Phase47AuditPath,
};

var options = new JsonSerializerOptions { WriteIndented = true };
File.WriteAllText(
    Path.Combine(outputDir, "target_scoped_falsifier_policy.json"),
    JsonSerializer.Serialize(policy, options));
File.WriteAllText(
    Path.Combine(outputDir, "target_scoped_falsifier_policy_summary.json"),
    JsonSerializer.Serialize(new
    {
        phaseId = "phase103-target-scoped-falsifier-policy",
        policy.terminalStatus,
        policy.targetScopedPolicyAdopted,
        policy.targetRelevantSevereFalsifierCount,
        policy.globalSidecarSevereFalsifierCount,
        policy.unrestrictedPhysicalBosonLanguageAllowed,
    }, options));

Console.WriteLine(policy.terminalStatus);

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
        ? value.GetString()
        : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var i)
        ? i
        : null;
