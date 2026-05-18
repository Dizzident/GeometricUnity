using System.Text.Json;

const string DefaultOutputDir = "studies/phase196_higgs_potential_self_coupling_closure_audit_001/output";
const string Phase43StudyPath = "studies/phase43_selector_eigen_wz_source_spectra_001/STUDY.md";
const string Phase98SummaryPath = "studies/phase98_selector_eigenmode_boson_bridge_001/output/selector_eigenmode_boson_bridge_summary.json";
const string Phase187Path = "studies/phase187_higgs_scalar_source_identity_scaffold_001/output/higgs_scalar_source_identity_scaffold_summary.json";
const string Phase189Path = "studies/phase189_higgs_scalar_source_operator_census_001/output/higgs_scalar_source_operator_census_summary.json";
const string Phase3OpenIssuesPath = "docs/Phases/OpenIssues/PHASE_3_OPEN_ISSUES.md";

var outputDir = Environment.GetEnvironmentVariable("PHASE196_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase98 = JsonDocument.Parse(File.ReadAllText(Phase98SummaryPath));
using var phase187 = JsonDocument.Parse(File.ReadAllText(Phase187Path));
using var phase189 = JsonDocument.Parse(File.ReadAllText(Phase189Path));

var phase43Text = File.ReadAllText(Phase43StudyPath);
var openIssueText = File.ReadAllText(Phase3OpenIssuesPath);

var phase43IsWzSelectorSource = phase43Text.Contains("Selector-Specific Eigen W/Z Source Spectra", StringComparison.Ordinal)
    && phase43Text.Contains("Physical W/Z prediction", StringComparison.Ordinal);
var phase98ResidualSaysScalarSourceOnly = phase98.RootElement.TryGetProperty("residualLimitations", out var residuals)
    && residuals.EnumerateArray().Any(item => item.ValueKind == JsonValueKind.String
        && (item.GetString() ?? "").Contains("scalar source-spectrum", StringComparison.Ordinal));
var quarticTermsDeferred = openIssueText.Contains("Quartic and higher-order interaction terms", StringComparison.Ordinal)
    && openIssueText.Contains("Higgs-like self-interactions", StringComparison.Ordinal);
var p187SourceIdentityValidated = JsonBool(phase187.RootElement, "higgsSourceIdentityValidated") is true;
var p189CensusPromotable = JsonBool(phase189.RootElement, "censusPromotable") is true;
var p189PredictionAttemptAllowed = JsonBool(phase189.RootElement, "predictionAttemptAllowed") is true;

var checks = new[]
{
    new Check("phase43-is-higgs-potential-source", !phase43IsWzSelectorSource, "Phase43 is a W/Z selector-source campaign, not a Higgs scalar-potential source."),
    new Check("phase98-selector-bridge-is-higgs-source", !phase98ResidualSaysScalarSourceOnly, "Phase98 bridges selector eigenmode replay, but its residual limitation remains scalar source-spectrum lineage rather than Higgs potential evidence."),
    new Check("quartic-self-coupling-available", !quarticTermsDeferred, "Phase3 open issues defer quartic and higher interaction terms needed for Higgs-like self-interactions."),
    new Check("higgs-source-identity-validated", p187SourceIdentityValidated, "P187 requires a validated Higgs source identity before mass prediction."),
    new Check("higgs-source-operator-census-promotable", p189CensusPromotable, "P189 requires a promotable solved scalar source/operator census."),
    new Check("higgs-prediction-attempt-allowed", p189PredictionAttemptAllowed, "P189 must allow a target-independent Higgs prediction attempt before comparison."),
};

var canPromoteHiggsFromPotentialOrSelfCoupling = checks.All(check => check.Passed);
var terminalStatus = canPromoteHiggsFromPotentialOrSelfCoupling
    ? "higgs-potential-self-coupling-closure-promotable"
    : "higgs-potential-self-coupling-closure-blocked-no-source";

var result = new
{
    phaseId = "phase196-higgs-potential-self-coupling-closure-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    canPromoteHiggsFromPotentialOrSelfCoupling,
    checks,
    decision = canPromoteHiggsFromPotentialOrSelfCoupling
        ? "A target-independent Higgs potential/self-coupling source is present and can be used for a Higgs mass prediction attempt."
        : "Do not promote Higgs mass from current scalar or selector artifacts. Phase43 is W/Z-scoped, Phase98 is selector replay lineage, quartic/self-coupling terms are deferred, and P187/P189 still lack a solved Higgs scalar source/operator.",
    nextRequiredArtifact = "A target-independent scalar potential or self-coupling operator with quartic/higher interaction evidence, a Higgs excitation identity, a massive scalar profile, and stability sidecars.",
    sourceEvidence = new
    {
        phase43StudyPath = Phase43StudyPath,
        phase98SummaryPath = Phase98SummaryPath,
        phase187Path = Phase187Path,
        phase189Path = Phase189Path,
        phase3OpenIssuesPath = Phase3OpenIssuesPath,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "higgs_potential_self_coupling_closure_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "higgs_potential_self_coupling_closure_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.canPromoteHiggsFromPotentialOrSelfCoupling,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"canPromoteHiggsFromPotentialOrSelfCoupling={canPromoteHiggsFromPotentialOrSelfCoupling}");
Console.WriteLine($"checkCount={checks.Length}");

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

sealed record Check(string CheckId, bool Passed, string Detail);
