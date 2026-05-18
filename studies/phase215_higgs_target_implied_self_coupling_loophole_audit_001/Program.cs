using System.Text.Json;

const string DefaultOutputDir = "studies/phase215_higgs_target_implied_self_coupling_loophole_audit_001/output";
const string Phase54Path = "studies/phase54_external_electroweak_scale_input_001/external_electroweak_scale_input.json";
const string Phase196Path = "studies/phase196_higgs_potential_self_coupling_closure_audit_001/output/higgs_potential_self_coupling_closure_audit_summary.json";
const string Phase203Path = "studies/phase203_defensible_boson_value_manifest_001/output/defensible_boson_value_manifest_summary.json";
const string Phase207Path = "studies/phase207_higgs_quartic_self_coupling_source_scan_001/output/higgs_quartic_self_coupling_source_scan_summary.json";
const string Phase210Path = "studies/phase210_boson_source_lineage_evidence_application_gate_001/output/boson_source_lineage_evidence_application_gate_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE215_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase54 = JsonDocument.Parse(File.ReadAllText(Phase54Path));
using var phase196 = JsonDocument.Parse(File.ReadAllText(Phase196Path));
using var phase203 = JsonDocument.Parse(File.ReadAllText(Phase203Path));
using var phase207 = JsonDocument.Parse(File.ReadAllText(Phase207Path));
using var phase210 = JsonDocument.Parse(File.ReadAllText(Phase210Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));

var fermiScale = phase54.RootElement
    .GetProperty("derivedExternalScaleCandidates")
    .EnumerateArray()
    .First(row => JsonString(row, "scaleId") == "phase54-fermi-derived-electroweak-vacuum-scale");
var higgsRow = phase203.RootElement.GetProperty("nonDefensibleRows")
    .EnumerateArray()
    .First(row => JsonString(row, "observableId") == "physical-higgs-mass-gev");

var vev = RequiredDouble(fermiScale, "value");
var targetHiggsMass = RequiredDouble(higgsRow, "targetValue");
var targetImpliedQuartic = targetHiggsMass * targetHiggsMass / (2.0 * vev * vev);
var targetImpliedMassReplay = Math.Sqrt(2.0 * targetImpliedQuartic) * vev;

var canPromoteHiggsFromPotentialOrSelfCoupling = JsonBool(phase196.RootElement, "canPromoteHiggsFromPotentialOrSelfCoupling") is true;
var canPromoteHiggsQuarticSelfCouplingSource = JsonBool(phase207.RootElement, "canPromoteHiggsQuarticSelfCouplingSource") is true;
var rerunPromotionAllowed = JsonBool(phase210.RootElement, "rerunPromotionAllowed") is true;
var blockerMatrixReady = JsonBool(phase213.RootElement, "blockerMatrixReady") is true;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? 0;
var canPromoteTargetImpliedHiggsSelfCoupling = false;

var checks = new[]
{
    new Check("target-higgs-row-is-non-defensible", JsonString(higgsRow, "status") == "not-defensible-blocked", $"higgsStatus={JsonString(higgsRow, "status")}"),
    new Check("target-implied-quartic-is-diagnostic", false, $"lambda_target=m_h^2/(2 v^2)={targetImpliedQuartic}; replayMass={targetImpliedMassReplay} GeV uses the Higgs target by construction."),
    new Check("higgs-potential-self-coupling-source-promotable", canPromoteHiggsFromPotentialOrSelfCoupling, $"canPromoteHiggsFromPotentialOrSelfCoupling={canPromoteHiggsFromPotentialOrSelfCoupling}"),
    new Check("higgs-quartic-source-scan-promotable", canPromoteHiggsQuarticSelfCouplingSource, $"canPromoteHiggsQuarticSelfCouplingSource={canPromoteHiggsQuarticSelfCouplingSource}"),
    new Check("higgs-source-lineage-ready-for-rerun", rerunPromotionAllowed, $"rerunPromotionAllowed={rerunPromotionAllowed}"),
    new Check("blocker-matrix-still-missing-higgs-fields", blockerMatrixReady && higgsMissingFieldCount > 0, $"blockerMatrixReady={blockerMatrixReady}; higgsMissingFieldCount={higgsMissingFieldCount}"),
};

var terminalStatus = "higgs-target-implied-self-coupling-loophole-closed-not-gu-prediction";
var result = new
{
    phaseId = "phase215-higgs-target-implied-self-coupling-loophole-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    canPromoteTargetImpliedHiggsSelfCoupling,
    targetImpliedDiagnostic = new
    {
        vev,
        targetHiggsMass,
        targetImpliedQuartic,
        targetImpliedMassReplay,
        status = "diagnostic-only-not-promotable",
    },
    checks,
    decision = "Do not complete the Higgs prediction by deriving the quartic/self-coupling from the observed Higgs mass and Fermi-derived VEV. That reproduces the target by construction and does not supply the solved target-independent scalar source/operator, identity envelope, massive profile, and stability sidecars required by P201/P209/P210/P213.",
    nextRequiredArtifact = "A target-independent Higgs scalar potential or self-coupling source/operator that fills the Higgs Phase201 template and passes P210.",
    sourceEvidence = new
    {
        phase54Path = Phase54Path,
        phase196Path = Phase196Path,
        phase203Path = Phase203Path,
        phase207Path = Phase207Path,
        phase210Path = Phase210Path,
        phase213Path = Phase213Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "higgs_target_implied_self_coupling_loophole_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "higgs_target_implied_self_coupling_loophole_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.canPromoteTargetImpliedHiggsSelfCoupling,
        result.targetImpliedDiagnostic,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"canPromoteTargetImpliedHiggsSelfCoupling={canPromoteTargetImpliedHiggsSelfCoupling}");
Console.WriteLine($"targetImpliedQuartic={targetImpliedQuartic}");

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

static double RequiredDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var value)
        ? value
        : throw new InvalidOperationException($"Missing numeric property {propertyName}.");

sealed record Check(string CheckId, bool Passed, string Detail);
