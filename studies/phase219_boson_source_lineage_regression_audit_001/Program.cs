using System.Text.Json;

const string DefaultOutputDir = "studies/phase219_boson_source_lineage_regression_audit_001/output";
const string Phase101Path = "studies/phase101_boson_prediction_package_001/output/boson_prediction_package_summary.json";
const string Phase201Path = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase210Path = "studies/phase210_boson_source_lineage_evidence_application_gate_001/output/boson_source_lineage_evidence_application_gate_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase216Path = "studies/phase216_boson_nonclaim_firewall_001/output/boson_nonclaim_firewall_summary.json";
const string Phase217Path = "studies/phase217_boson_independent_source_review_001/output/boson_independent_source_review_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE219_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase101 = JsonDocument.Parse(File.ReadAllText(Phase101Path));
using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201Path));
using var phase210 = JsonDocument.Parse(File.ReadAllText(Phase210Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase216 = JsonDocument.Parse(File.ReadAllText(Phase216Path));
using var phase217 = JsonDocument.Parse(File.ReadAllText(Phase217Path));

var wzTemplatePath = RequiredString(phase201.RootElement, "wzTemplatePath");
var higgsTemplatePath = RequiredString(phase201.RootElement, "higgsTemplatePath");
using var wzTemplate = JsonDocument.Parse(File.ReadAllText(wzTemplatePath));
using var higgsTemplate = JsonDocument.Parse(File.ReadAllText(higgsTemplatePath));

var wzNullPlaceholderCount = CountNulls(wzTemplate.RootElement);
var higgsNullPlaceholderCount = CountNulls(higgsTemplate.RootElement);
var wzMissingFields = JsonStringArray(phase213.RootElement, "wzMissingFields");
var higgsMissingFields = JsonStringArray(phase213.RootElement, "higgsMissingFields");
string[] physicalMassClaims =
[
    "physical-w-boson-mass-gev",
    "physical-z-boson-mass-gev",
    "physical-higgs-mass-gev",
];
string[] requiredWzBlockers =
[
    "theoremOrDerivationId",
    "w-boson.rawAmplitudeGatePassed=true",
    "z-boson.rawAmplitudeGatePassed=true",
];
string[] requiredHiggsBlockers =
[
    "scalarSourceOperatorId",
    "predictionRow.targetComparisonGatePassed=true",
];

var checks = new[]
{
    new Check(
        "phase201-wz-null-placeholders-not-promotable",
        wzNullPlaceholderCount > 0
            && JsonString(wzTemplate.RootElement, "status") == "template-unfilled"
            && JsonNestedBool(phase201.RootElement, "wzValidation", "promotable") is false,
        $"wzNullPlaceholderCount={wzNullPlaceholderCount}; wzStatus={JsonString(wzTemplate.RootElement, "status")}; wzPromotable={JsonNestedBool(phase201.RootElement, "wzValidation", "promotable")}"),
    new Check(
        "phase201-higgs-null-placeholders-not-promotable",
        higgsNullPlaceholderCount > 0
            && JsonString(higgsTemplate.RootElement, "status") == "template-unfilled"
            && JsonNestedBool(phase201.RootElement, "higgsValidation", "promotable") is false,
        $"higgsNullPlaceholderCount={higgsNullPlaceholderCount}; higgsStatus={JsonString(higgsTemplate.RootElement, "status")}; higgsPromotable={JsonNestedBool(phase201.RootElement, "higgsValidation", "promotable")}"),
    new Check(
        "phase213-exact-wz-missing-fields-present",
        JsonInt(phase213.RootElement, "wzMissingFieldCount") == wzMissingFields.Length
            && requiredWzBlockers.All(wzMissingFields.Contains),
        $"wzMissingFieldCount={JsonInt(phase213.RootElement, "wzMissingFieldCount")}; wzMissingFields={string.Join(",", wzMissingFields)}"),
    new Check(
        "phase213-exact-higgs-missing-fields-present",
        JsonInt(phase213.RootElement, "higgsMissingFieldCount") == higgsMissingFields.Length
            && requiredHiggsBlockers.All(higgsMissingFields.Contains),
        $"higgsMissingFieldCount={JsonInt(phase213.RootElement, "higgsMissingFieldCount")}; higgsMissingFields={string.Join(",", higgsMissingFields)}"),
    new Check(
        "phase210-promotion-rerun-blocked",
        JsonBool(phase210.RootElement, "rerunPromotionAllowed") is false
            && JsonNestedBool(phase210.RootElement, "wzApplication", "readyForApplication") is false
            && JsonNestedBool(phase210.RootElement, "higgsApplication", "readyForApplication") is false,
        $"rerunPromotionAllowed={JsonBool(phase210.RootElement, "rerunPromotionAllowed")}; wzReady={JsonNestedBool(phase210.RootElement, "wzApplication", "readyForApplication")}; higgsReady={JsonNestedBool(phase210.RootElement, "higgsApplication", "readyForApplication")}"),
    new Check(
        "phase216-nonclaim-firewall-ready",
        JsonBool(phase216.RootElement, "nonclaimFirewallReady") is true
            && JsonStringArray(phase216.RootElement, "prohibitedClaimIds").Intersect(physicalMassClaims).Count() == physicalMassClaims.Length,
        $"nonclaimFirewallReady={JsonBool(phase216.RootElement, "nonclaimFirewallReady")}; prohibitedClaimIds={string.Join(",", JsonStringArray(phase216.RootElement, "prohibitedClaimIds"))}"),
    new Check(
        "phase217-no-fixable-route-regression",
        JsonBool(phase217.RootElement, "noFixableImplementationRoute") is true
            && JsonBool(phase217.RootElement, "promotionRerunAllowed") is false,
        $"noFixableImplementationRoute={JsonBool(phase217.RootElement, "noFixableImplementationRoute")}; promotionRerunAllowed={JsonBool(phase217.RootElement, "promotionRerunAllowed")}"),
    new Check(
        "phase101-completion-claims-still-blocked",
        JsonBool(phase101.RootElement, "objectiveAchieved") is false
            && JsonBool(phase101.RootElement, "allKnownBosonValuesDefensible") is false
            && JsonBool(phase101.RootElement, "predictionSetComplete") is false,
        $"objectiveAchieved={JsonBool(phase101.RootElement, "objectiveAchieved")}; allKnownBosonValuesDefensible={JsonBool(phase101.RootElement, "allKnownBosonValuesDefensible")}; predictionSetComplete={JsonBool(phase101.RootElement, "predictionSetComplete")}"),
};

var regressionAuditPassed = checks.All(check => check.Passed);
var terminalStatus = regressionAuditPassed
    ? "boson-source-lineage-regression-audit-passed-current-blockers-preserved"
    : "boson-source-lineage-regression-audit-failed-review-required";

var result = new
{
    phaseId = "phase219-boson-source-lineage-regression-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    regressionAuditPassed,
    wzNullPlaceholderCount,
    higgsNullPlaceholderCount,
    wzMissingFieldCount = wzMissingFields.Length,
    higgsMissingFieldCount = higgsMissingFields.Length,
    checks,
    decision = regressionAuditPassed
        ? "Current artifacts preserve the source-lineage blockers: null Phase201 templates remain non-promotable, Phase210 blocks promotion rerun, Phase216 preserves nonclaims, Phase217 reports no fixable implementation route, and Phase101 remains incomplete."
        : "Review source-lineage artifacts before publishing or rerunning promotion; at least one regression guard failed.",
    sourceEvidence = new
    {
        phase101Path = Phase101Path,
        phase201Path = Phase201Path,
        phase210Path = Phase210Path,
        phase213Path = Phase213Path,
        phase216Path = Phase216Path,
        phase217Path = Phase217Path,
        wzTemplatePath,
        higgsTemplatePath,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "boson_source_lineage_regression_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "boson_source_lineage_regression_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.regressionAuditPassed,
        result.wzNullPlaceholderCount,
        result.higgsNullPlaceholderCount,
        result.wzMissingFieldCount,
        result.higgsMissingFieldCount,
        result.checks,
        result.decision,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"regressionAuditPassed={regressionAuditPassed}");
Console.WriteLine($"wzNullPlaceholderCount={wzNullPlaceholderCount}");
Console.WriteLine($"higgsNullPlaceholderCount={higgsNullPlaceholderCount}");

static string RequiredString(JsonElement element, string propertyName) =>
    JsonString(element, propertyName) ?? throw new InvalidOperationException($"Missing string property {propertyName}.");

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

static bool? JsonNestedBool(JsonElement element, string objectName, string propertyName) =>
    element.TryGetProperty(objectName, out var obj) && obj.ValueKind == JsonValueKind.Object
        ? JsonBool(obj, propertyName)
        : null;

static string[] JsonStringArray(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Array
        ? property.EnumerateArray()
            .Where(item => item.ValueKind == JsonValueKind.String)
            .Select(item => item.GetString()!)
            .ToArray()
        : Array.Empty<string>();

static int CountNulls(JsonElement element) => element.ValueKind switch
{
    JsonValueKind.Null => 1,
    JsonValueKind.Array => element.EnumerateArray().Sum(CountNulls),
    JsonValueKind.Object => element.EnumerateObject().Sum(property => CountNulls(property.Value)),
    _ => 0,
};

sealed record Check(string CheckId, bool Passed, string Detail);
