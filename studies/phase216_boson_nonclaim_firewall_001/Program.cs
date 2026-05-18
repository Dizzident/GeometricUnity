using System.Text.Json;

const string DefaultOutputDir = "studies/phase216_boson_nonclaim_firewall_001/output";
const string Phase101Path = "studies/phase101_boson_prediction_package_001/output/boson_prediction_package_summary.json";
const string Phase203Path = "studies/phase203_defensible_boson_value_manifest_001/output/defensible_boson_value_manifest_summary.json";
const string Phase212Path = "studies/phase212_boson_scientific_claim_boundary_certificate_001/output/boson_scientific_claim_boundary_certificate_summary.json";
const string Phase214Path = "studies/phase214_external_electroweak_input_loophole_audit_001/output/external_electroweak_input_loophole_audit_summary.json";
const string Phase215Path = "studies/phase215_higgs_target_implied_self_coupling_loophole_audit_001/output/higgs_target_implied_self_coupling_loophole_audit_summary.json";
const string Phase218Path = "studies/phase218_official_gu_public_source_audit_001/output/official_gu_public_source_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE216_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase101 = JsonDocument.Parse(File.ReadAllText(Phase101Path));
using var phase203 = JsonDocument.Parse(File.ReadAllText(Phase203Path));
using var phase212 = JsonDocument.Parse(File.ReadAllText(Phase212Path));
using var phase214 = JsonDocument.Parse(File.ReadAllText(Phase214Path));
using var phase215 = JsonDocument.Parse(File.ReadAllText(Phase215Path));
using var phase218 = JsonDocument.Parse(File.ReadAllText(Phase218Path));

var allowedClaimIds = new HashSet<string>
{
    "physical-w-z-mass-ratio",
    "physical-photon-masslessness",
    "physical-gluon-masslessness",
};
var prohibitedClaimIds = new HashSet<string>
{
    "physical-w-boson-mass-gev",
    "physical-z-boson-mass-gev",
    "physical-higgs-mass-gev",
};
var prohibitedShortcutIds = new HashSet<string>
{
    "external-or-target-implied-electroweak-coupling-as-gu-wz-prediction",
    "target-implied-higgs-quartic-or-self-coupling-as-gu-higgs-prediction",
};
var prohibitedSourceIds = new HashSet<string>
{
    "official-public-gu-draft-as-wz-or-higgs-source-lineage-promotion",
};

var defensibleRows = phase203.RootElement.GetProperty("defensibleValues").EnumerateArray().Select(row => row.Clone()).ToArray();
var nonDefensibleRows = phase203.RootElement.GetProperty("nonDefensibleRows").EnumerateArray().Select(row => row.Clone()).ToArray();
var defensibleObservableIds = defensibleRows.Select(row => JsonString(row, "observableId") ?? "").Where(id => id.Length > 0).ToHashSet();
var nonDefensibleObservableIds = nonDefensibleRows.Select(row => JsonString(row, "observableId") ?? "").Where(id => id.Length > 0).ToHashSet();
var p212AllowedClaims = JsonStringArray(phase212.RootElement, "allowedClaims").ToHashSet();
var p212ProhibitedClaims = JsonStringArray(phase212.RootElement, "prohibitedClaimsUntilNewEvidence").ToHashSet();
var p212ProhibitedShortcuts = JsonStringArray(phase212.RootElement, "prohibitedShortcutClaims").ToHashSet();
var p212ProhibitedSources = JsonStringArray(phase212.RootElement, "prohibitedSourceClaims").ToHashSet();

var noProhibitedPhysicalPromoted = !defensibleObservableIds.Overlaps(prohibitedClaimIds);
var allAllowedClaimsPromoted = allowedClaimIds.SetEquals(defensibleObservableIds);
var allPhysicalNonClaimsPresent = prohibitedClaimIds.IsSubsetOf(nonDefensibleObservableIds)
    && prohibitedClaimIds.IsSubsetOf(p212ProhibitedClaims);
var shortcutNonClaimsPresent = prohibitedShortcutIds.IsSubsetOf(p212ProhibitedShortcuts);
var sourceNonClaimsPresent = prohibitedSourceIds.IsSubsetOf(p212ProhibitedSources);
var packageStillIncomplete = JsonBool(phase101.RootElement, "allKnownBosonValuesDefensible") is false
    && JsonBool(phase101.RootElement, "predictionSetComplete") is false
    && JsonBool(phase101.RootElement, "objectiveAchieved") is false;
var shortcutPromotionBlocked = JsonBool(phase214.RootElement, "canPromoteExternalElectroweakBridge") is false
    && JsonBool(phase215.RootElement, "canPromoteTargetImpliedHiggsSelfCoupling") is false;
var officialPublicDraftPromotionBlocked = JsonBool(phase218.RootElement, "officialPublicSourceAuditMaterialized") is true
    && JsonBool(phase218.RootElement, "officialDraftProvidesCompletionSource") is false
    && JsonBool(phase212.RootElement, "officialPublicSourceNonclaimClosed") is true;
var claimBoundaryReady = JsonBool(phase212.RootElement, "claimBoundaryReady") is true;

var checks = new[]
{
    new Check("allowed-claims-exactly-promoted", allAllowedClaimsPromoted, $"defensibleObservableIds={string.Join(",", defensibleObservableIds.OrderBy(id => id))}"),
    new Check("no-prohibited-physical-claim-promoted", noProhibitedPhysicalPromoted, $"prohibitedPromoted={string.Join(",", defensibleObservableIds.Intersect(prohibitedClaimIds).OrderBy(id => id))}"),
    new Check("physical-nonclaims-present", allPhysicalNonClaimsPresent, $"nonDefensible={string.Join(",", nonDefensibleObservableIds.Intersect(prohibitedClaimIds).OrderBy(id => id))}; p212Prohibited={string.Join(",", p212ProhibitedClaims.Intersect(prohibitedClaimIds).OrderBy(id => id))}"),
    new Check("shortcut-nonclaims-present", shortcutNonClaimsPresent, $"p212ProhibitedShortcuts={string.Join(",", p212ProhibitedShortcuts.OrderBy(id => id))}"),
    new Check("source-nonclaims-present", sourceNonClaimsPresent, $"p212ProhibitedSources={string.Join(",", p212ProhibitedSources.OrderBy(id => id))}"),
    new Check("shortcut-promotion-blocked", shortcutPromotionBlocked, $"canPromoteExternalElectroweakBridge={JsonBool(phase214.RootElement, "canPromoteExternalElectroweakBridge")}; canPromoteTargetImpliedHiggsSelfCoupling={JsonBool(phase215.RootElement, "canPromoteTargetImpliedHiggsSelfCoupling")}"),
    new Check("official-public-draft-promotion-blocked", officialPublicDraftPromotionBlocked, $"officialPublicSourceAuditMaterialized={JsonBool(phase218.RootElement, "officialPublicSourceAuditMaterialized")}; officialDraftProvidesCompletionSource={JsonBool(phase218.RootElement, "officialDraftProvidesCompletionSource")}; p212OfficialPublicSourceNonclaimClosed={JsonBool(phase212.RootElement, "officialPublicSourceNonclaimClosed")}"),
    new Check("package-still-incomplete", packageStillIncomplete, $"allKnownBosonValuesDefensible={JsonBool(phase101.RootElement, "allKnownBosonValuesDefensible")}; predictionSetComplete={JsonBool(phase101.RootElement, "predictionSetComplete")}; objectiveAchieved={JsonBool(phase101.RootElement, "objectiveAchieved")}"),
    new Check("claim-boundary-ready", claimBoundaryReady, $"claimBoundaryReady={claimBoundaryReady}"),
};

var nonclaimFirewallReady = checks.All(check => check.Passed);
var terminalStatus = nonclaimFirewallReady
    ? "boson-nonclaim-firewall-ready"
    : "boson-nonclaim-firewall-failed-review-required";

var result = new
{
    phaseId = "phase216-boson-nonclaim-firewall",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    nonclaimFirewallReady,
    allowedClaimIds = allowedClaimIds.OrderBy(id => id).ToArray(),
    prohibitedClaimIds = prohibitedClaimIds.OrderBy(id => id).ToArray(),
    prohibitedShortcutIds = prohibitedShortcutIds.OrderBy(id => id).ToArray(),
    prohibitedSourceIds = prohibitedSourceIds.OrderBy(id => id).ToArray(),
    checks,
    decision = nonclaimFirewallReady
        ? "Current artifacts consistently publish only defensible promoted rows and keep W, Z, Higgs, official public draft-only source promotion, and shortcut routes as explicit non-claims."
        : "Review current boson manifests before publishing; at least one prohibited claim firewall check failed.",
    sourceEvidence = new
    {
        phase101Path = Phase101Path,
        phase203Path = Phase203Path,
        phase212Path = Phase212Path,
        phase214Path = Phase214Path,
        phase215Path = Phase215Path,
        phase218Path = Phase218Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "boson_nonclaim_firewall.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "boson_nonclaim_firewall_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.nonclaimFirewallReady,
        result.allowedClaimIds,
        result.prohibitedClaimIds,
        result.prohibitedShortcutIds,
        result.prohibitedSourceIds,
        result.checks,
        result.decision,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"nonclaimFirewallReady={nonclaimFirewallReady}");
Console.WriteLine($"allowedClaimCount={allowedClaimIds.Count}");
Console.WriteLine($"prohibitedClaimCount={prohibitedClaimIds.Count}");

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

static string[] JsonStringArray(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Array
        ? property.EnumerateArray()
            .Where(item => item.ValueKind == JsonValueKind.String)
            .Select(item => item.GetString()!)
            .ToArray()
        : Array.Empty<string>();

sealed record Check(string CheckId, bool Passed, string Detail);
