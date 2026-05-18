using System.Text.Json;

const string DefaultOutputDir = "studies/phase222_wz_raw_amplitude_source_obstruction_audit_001/output";
const string Phase77Path = "studies/phase77_raw_weak_coupling_matrix_element_evidence_gate_001/raw_matrix_element_evidence_gate.json";
const string Phase78Path = "studies/phase78_replayed_raw_weak_coupling_matrix_element_builder_001/replayed_raw_matrix_element_builder.json";
const string Phase80Path = "studies/phase80_production_analytic_replay_input_materialization_audit_001/production_analytic_replay_input_materialization_audit.json";
const string Phase117Path = "studies/phase117_wz_repaired_pair_sweep_001/output/wz_repaired_pair_sweep.json";
const string Phase221Path = "studies/phase221_su2_casimir_wz_normalization_probe_001/output/su2_casimir_wz_normalization_probe_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE222_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase77 = JsonDocument.Parse(File.ReadAllText(Phase77Path));
using var phase78 = JsonDocument.Parse(File.ReadAllText(Phase78Path));
using var phase80 = JsonDocument.Parse(File.ReadAllText(Phase80Path));
using var phase117 = JsonDocument.Parse(File.ReadAllText(Phase117Path));
using var phase221 = JsonDocument.Parse(File.ReadAllText(Phase221Path));

var phase65RawEvidenceBlocked = string.Equals(JsonString(phase77.RootElement, "terminalStatus"), "raw-weak-coupling-matrix-element-evidence-blocked", StringComparison.Ordinal);
var replayBuilderImplementedButMissingProduction = string.Equals(JsonString(phase78.RootElement, "terminalStatus"), "replay-builder-implemented-production-artifact-missing", StringComparison.Ordinal);
var productionReplayInputsBlocked = string.Equals(JsonString(phase80.RootElement, "terminalStatus"), "production-analytic-replay-inputs-blocked", StringComparison.Ordinal);
var repairedPairSweepNoRepair = string.Equals(JsonString(phase117.RootElement, "terminalStatus"), "wz-repaired-pair-sweep-no-pair-repair", StringComparison.Ordinal);
var casimirNumericalLeadPresent = JsonBool(phase221.RootElement, "numericalTargetComparisonPassed") is true;
var casimirLeadPromotable = JsonBool(phase221.RootElement, "sourceLineagePromotable") is true;

var targetImpliedRaw = JsonDouble(phase117.RootElement, "targetImpliedRawMatrixElementMagnitude");
var strongestByRaw = phase117.RootElement.GetProperty("strongestByRaw");
var bestRawToTargetRatio = JsonDouble(strongestByRaw, "maxRawToTargetRatio");
var bestCommonBridgePassed = JsonBool(strongestByRaw, "commonBridgePassed") is true;
var commonBridgePairCount = JsonInt(phase117.RootElement, "commonBridgePairCount") ?? 0;

var checks = new[]
{
    new Check(
        "phase65-fixture-raw-amplitude-blocked",
        phase65RawEvidenceBlocked,
        $"phase77Status={JsonString(phase77.RootElement, "terminalStatus")}; closureRequirements={string.Join(" | ", JsonStringArray(phase77.RootElement, "closureRequirements"))}"),
    new Check(
        "replay-builder-awaits-production-artifact",
        replayBuilderImplementedButMissingProduction,
        $"phase78Status={JsonString(phase78.RootElement, "terminalStatus")}; closureRequirements={string.Join(" | ", JsonStringArray(phase78.RootElement, "closureRequirements"))}"),
    new Check(
        "production-analytic-replay-inputs-blocked",
        productionReplayInputsBlocked,
        $"phase80Status={JsonString(phase80.RootElement, "terminalStatus")}; closureRequirements={string.Join(" | ", JsonStringArray(phase80.RootElement, "closureRequirements"))}"),
    new Check(
        "repaired-pair-sweep-far-below-required-raw",
        repairedPairSweepNoRepair && bestRawToTargetRatio is < 0.001 && !bestCommonBridgePassed && commonBridgePairCount == 0,
        $"phase117Status={JsonString(phase117.RootElement, "terminalStatus")}; targetImpliedRaw={targetImpliedRaw}; bestRawToTargetRatio={bestRawToTargetRatio}; commonBridgePairCount={commonBridgePairCount}"),
    new Check(
        "casimir-numerical-lead-not-promotable-without-raw-source",
        casimirNumericalLeadPresent && !casimirLeadPromotable,
        $"phase221NumericalTargetComparisonPassed={casimirNumericalLeadPresent}; phase221SourceLineagePromotable={casimirLeadPromotable}"),
};

var rawAmplitudeSourceObstructionCertified = checks.All(check => check.Passed);
var terminalStatus = rawAmplitudeSourceObstructionCertified
    ? "wz-raw-amplitude-source-obstruction-certified-production-replay-required"
    : "wz-raw-amplitude-source-obstruction-indeterminate-review-required";

var result = new
{
    phaseId = "phase222-wz-raw-amplitude-source-obstruction-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    rawAmplitudeSourceObstructionCertified,
    targetImpliedRawMatrixElementMagnitude = targetImpliedRaw,
    bestProductionReplay = new
    {
        pairId = JsonString(strongestByRaw, "pairId"),
        modeI = JsonInt(strongestByRaw, "modeI"),
        modeJ = JsonInt(strongestByRaw, "modeJ"),
        wRawMatrixElementMagnitude = JsonDouble(strongestByRaw, "wRawMatrixElementMagnitude"),
        zRawMatrixElementMagnitude = JsonDouble(strongestByRaw, "zRawMatrixElementMagnitude"),
        bestRawToTargetRatio,
        commonBridgePassed = bestCommonBridgePassed,
        commonBridgeRelativeSpread = JsonDouble(strongestByRaw, "commonBridgeRelativeSpread"),
        rawEvidenceValidated = JsonBool(strongestByRaw, "rawEvidenceValidated"),
        fermionQualityPassed = JsonBool(strongestByRaw, "fermionQualityPassed"),
    },
    casimirNumericalLead = new
    {
        casimirNumericalLeadPresent,
        casimirLeadPromotable,
        casimirWeakCoupling = JsonDouble(phase221.RootElement, "casimirWeakCoupling"),
        decision = JsonString(phase221.RootElement, "decision"),
    },
    checks,
    decision = rawAmplitudeSourceObstructionCertified
        ? "The W/Z numerical normalization lead cannot be promoted from current artifacts because the only near-target raw amplitude is the Phase65 fixture blocked by Phase77, while production replayed raw amplitudes are far too small and fail common-bridge repair."
        : "Review W/Z raw-amplitude artifacts before relying on this obstruction; at least one guard is inconsistent.",
    nextRequiredArtifact = new[]
    {
        "A replayed analytic Dirac-variation raw matrix-element source with production boson perturbation, selected exact fermion eigenvectors, variation evidence id, and accepted normalization.",
        "The replayed raw source must be near the required raw amplitude without using W/Z targets and must pass common W/Z bridge consistency.",
        "Only after that should the SU(2) Casimir/RMS normalization lead be reconsidered for Phase201/P209/P210/P213 promotion.",
    },
    sourceEvidence = new
    {
        phase77Path = Phase77Path,
        phase78Path = Phase78Path,
        phase80Path = Phase80Path,
        phase117Path = Phase117Path,
        phase221Path = Phase221Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "wz_raw_amplitude_source_obstruction_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "wz_raw_amplitude_source_obstruction_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.rawAmplitudeSourceObstructionCertified,
        result.targetImpliedRawMatrixElementMagnitude,
        result.bestProductionReplay,
        result.casimirNumericalLead,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"rawAmplitudeSourceObstructionCertified={rawAmplitudeSourceObstructionCertified}");
Console.WriteLine($"targetImpliedRawMatrixElementMagnitude={targetImpliedRaw}");
Console.WriteLine($"bestRawToTargetRatio={bestRawToTargetRatio}");

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

static double? JsonDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var value) ? value : null;

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
