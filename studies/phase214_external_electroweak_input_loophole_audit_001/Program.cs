using System.Text.Json;

const string DefaultOutputDir = "studies/phase214_external_electroweak_input_loophole_audit_001/output";
const string Phase54Path = "studies/phase54_external_electroweak_scale_input_001/external_electroweak_scale_input.json";
const string Phase68Path = "studies/phase68_normalized_weak_coupling_candidate_promotion_001/normalized_weak_coupling_candidate_promotion.json";
const string Phase69Path = "studies/phase69_electroweak_mass_generation_relation_001/electroweak_mass_generation_relation.json";
const string Phase75Path = "studies/phase75_wz_absolute_mass_miss_diagnostic_001/wz_absolute_mass_miss_diagnostic.json";
const string Phase197Path = "studies/phase197_electroweak_weak_coupling_wz_mass_closure_audit_001/output/electroweak_weak_coupling_wz_mass_closure_audit_summary.json";
const string Phase201Path = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase210Path = "studies/phase210_boson_source_lineage_evidence_application_gate_001/output/boson_source_lineage_evidence_application_gate_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE214_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase54 = JsonDocument.Parse(File.ReadAllText(Phase54Path));
using var phase68 = JsonDocument.Parse(File.ReadAllText(Phase68Path));
using var phase69 = JsonDocument.Parse(File.ReadAllText(Phase69Path));
using var phase75 = JsonDocument.Parse(File.ReadAllText(Phase75Path));
using var phase197 = JsonDocument.Parse(File.ReadAllText(Phase197Path));
using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201Path));
using var phase210 = JsonDocument.Parse(File.ReadAllText(Phase210Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));

var fermiScale = phase54.RootElement
    .GetProperty("derivedExternalScaleCandidates")
    .EnumerateArray()
    .First(row => JsonString(row, "scaleId") == "phase54-fermi-derived-electroweak-vacuum-scale");
var promotedWeakCoupling = RequiredDouble(phase68.RootElement.GetProperty("candidate"), "couplingValue");
var currentWeakCoupling = RequiredDouble(phase197.RootElement, "currentWeakCoupling");
var targetImpliedWeakCoupling = RequiredDouble(phase197.RootElement, "targetImpliedWeakCoupling");
var requiredWeakCouplingScale = RequiredDouble(phase197.RootElement, "requiredWeakCouplingScale");
var internalWzRatio = RequiredDouble(phase69.RootElement, "internalWzRatio");
var vev = RequiredDouble(fermiScale, "value");
var currentW = promotedWeakCoupling * vev / 2.0;
var currentZ = currentW / internalWzRatio;
var targetImpliedW = targetImpliedWeakCoupling * vev / 2.0;
var targetImpliedZ = targetImpliedW / internalWzRatio;

var allRequiredLineagesPromotable = JsonBool(phase201.RootElement, "allRequiredLineagesPromotable") is true;
var rerunPromotionAllowed = JsonBool(phase210.RootElement, "rerunPromotionAllowed") is true;
var blockerMatrixReady = JsonBool(phase213.RootElement, "blockerMatrixReady") is true;
var existingEvidenceFound = JsonBool(phase213.RootElement, "existingEvidenceFound") is true;
var canPromoteExternalElectroweakBridge = false;

var checks = new[]
{
    new Check("fermi-vev-external-scale-present", true, $"v={vev} GeV from P54; this is an external-disjoint scale input, not a GU source law."),
    new Check("gu-weak-coupling-route-tested", false, $"currentWeakCoupling={currentWeakCoupling}; P197 comparison fails for W/Z absolute masses."),
    new Check("target-implied-weak-coupling-rejected", false, $"targetImpliedWeakCoupling={targetImpliedWeakCoupling}; requiredWeakCouplingScale={requiredWeakCouplingScale}; P75/P197 classify this as diagnostic-only."),
    new Check("source-lineage-contracts-filled", allRequiredLineagesPromotable, $"allRequiredLineagesPromotable={allRequiredLineagesPromotable}"),
    new Check("promotion-rerun-allowed", rerunPromotionAllowed, $"rerunPromotionAllowed={rerunPromotionAllowed}"),
    new Check("blocker-matrix-confirms-no-existing-evidence", blockerMatrixReady && !existingEvidenceFound, $"blockerMatrixReady={blockerMatrixReady}; existingEvidenceFound={existingEvidenceFound}"),
};

var terminalStatus = "external-electroweak-input-loophole-closed-not-gu-prediction";
var result = new
{
    phaseId = "phase214-external-electroweak-input-loophole-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    canPromoteExternalElectroweakBridge,
    currentGuRoute = new
    {
        vev,
        promotedWeakCoupling,
        internalWzRatio,
        predictedWGeV = currentW,
        predictedZGeV = currentZ,
        comparisonStatus = JsonString(phase197.RootElement, "terminalStatus"),
    },
    targetImpliedDiagnosticRoute = new
    {
        targetImpliedWeakCoupling,
        requiredWeakCouplingScale,
        impliedWGeV = targetImpliedW,
        impliedZGeV = targetImpliedZ,
        status = "diagnostic-only-not-promotable",
    },
    checks,
    decision = "Do not complete W/Z absolute prediction by importing or target-implying the weak coupling. The Fermi-derived VEV can be a disjoint scale input, but a scientifically defensible GU W/Z absolute prediction still requires a target-independent source lineage that satisfies P201/P209/P210/P213.",
    nextRequiredArtifact = "A target-independent weak-coupling/source-lineage revision, not an externally chosen or target-implied electroweak coupling, that fills the W/Z Phase201 template and passes P210.",
    sourceEvidence = new
    {
        phase54Path = Phase54Path,
        phase68Path = Phase68Path,
        phase69Path = Phase69Path,
        phase75Path = Phase75Path,
        phase197Path = Phase197Path,
        phase201Path = Phase201Path,
        phase210Path = Phase210Path,
        phase213Path = Phase213Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "external_electroweak_input_loophole_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "external_electroweak_input_loophole_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.canPromoteExternalElectroweakBridge,
        result.currentGuRoute,
        result.targetImpliedDiagnosticRoute,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"canPromoteExternalElectroweakBridge={canPromoteExternalElectroweakBridge}");
Console.WriteLine($"currentWeakCoupling={currentWeakCoupling}");
Console.WriteLine($"targetImpliedWeakCoupling={targetImpliedWeakCoupling}");

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

static double RequiredDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var value)
        ? value
        : throw new InvalidOperationException($"Missing numeric property {propertyName}.");

sealed record Check(string CheckId, bool Passed, string Detail);
