using System.Text.Json;

const string DefaultOutputDir = "studies/phase195_electroweak_vev_wz_absolute_closure_audit_001/output";
const string Phase70Path = "studies/phase70_scalar_sector_bridge_evidence_001/scalar_sector_bridge_evidence.json";
const string Phase72Path = "studies/phase72_wz_absolute_scale_calibration_001/wz_absolute_scale_calibration.json";
const string Phase151Path = "studies/phase151_validated_boson_prediction_generator_001/output/validated_boson_predictions.json";
const string Phase185Path = "studies/phase185_wz_operator_unit_scale_materialization_001/output/wz_operator_unit_scale_materialization.json";
const string Phase191Path = "studies/phase191_wz_direct_bridge_prediction_decision_001/output/wz_direct_bridge_prediction_decision_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE195_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase70 = JsonDocument.Parse(File.ReadAllText(Phase70Path));
using var phase72 = JsonDocument.Parse(File.ReadAllText(Phase72Path));
using var phase151 = JsonDocument.Parse(File.ReadAllText(Phase151Path));
using var phase185 = JsonDocument.Parse(File.ReadAllText(Phase185Path));
using var phase191 = JsonDocument.Parse(File.ReadAllText(Phase191Path));

var failedWzRows = phase151.RootElement.GetProperty("failedAttempts")
    .EnumerateArray()
    .Where(row => JsonString(row, "particleId") is "w-boson" or "z-boson")
    .Select(row => new
    {
        particleId = JsonString(row, "particleId"),
        observableId = JsonString(row, "observableId"),
        predictedValue = JsonDouble(row, "predictedValue"),
        predictedUncertainty = JsonDouble(row, "predictedUncertainty"),
        targetValue = JsonDouble(row, "targetValue"),
        targetUncertainty = JsonDouble(row, "targetUncertainty"),
        sigmaResidual = JsonDouble(row, "pullOrSigmaResidual"),
        passed = JsonBool(row, "passed"),
        promotionAllowed = JsonBool(row, "promotionAllowed"),
    })
    .ToArray();

var p166Gates = phase185.RootElement.GetProperty("predictiveReplay").GetProperty("p166Gates");
var p170Gates = phase185.RootElement.GetProperty("predictiveReplay").GetProperty("p170BestAttempt").GetProperty("gates");
var p191Gates = phase191.RootElement.GetProperty("gates");

var scalarOrderParameterPresent = string.Equals(JsonString(phase70.RootElement, "terminalStatus"), "scalar-sector-bridge-evidence-derived", StringComparison.Ordinal);
var externalScaleValidated = string.Equals(JsonString(phase72.RootElement, "status"), "validated", StringComparison.Ordinal);
var p185UnitScaleMaterialized = JsonBool(phase185.RootElement, "unitScaleArtifactMaterialized") is true;
var p185RawGatePassed = JsonBool(p166Gates, "rawGatePassed") is true;
var p185CommonScaleGatePassed = JsonBool(p166Gates, "commonScaleGatePassed") is true;
var p185TargetComparisonPassed = JsonBool(p166Gates, "targetComparisonPassed") is true;
var p170CommonScaleGatePassed = JsonBool(p170Gates, "commonScaleGatePassed") is true;
var p170TargetComparisonPassed = JsonBool(p170Gates, "targetComparisonPassed") is true;
var p191DirectRawGatePassed = JsonBool(p191Gates, "rawGatePassed") is true;
var p191TheoremClaimed = JsonBool(p191Gates, "theoremClaimed") is true;
var p191WzParticleSplitPresent = JsonBool(p191Gates, "wZParticleSplitPresent") is true;

var closureGates = new[]
{
    new Gate("scalar-order-parameter-present", scalarOrderParameterPresent, "P70 supplies the external electroweak VEV/order parameter."),
    new Gate("external-scale-validated", externalScaleValidated, "P72 supplies a target-disjoint GeV-per-internal-unit scale."),
    new Gate("operator-unit-scale-materialized", p185UnitScaleMaterialized, "P185 materializes a target-independent operator/source unit scale."),
    new Gate("operator-unit-scale-raw-gate", p185RawGatePassed, $"P185/P166 raw gate minScaledRawToTargetRatio={JsonDouble(p166Gates, "minScaledRawToTargetRatio")}."),
    new Gate("operator-unit-scale-common-scale", p185CommonScaleGatePassed, $"P185/P166 common-scale spread={JsonDouble(p166Gates, "commonScaleRelativeSpread")}, tolerance={JsonDouble(p166Gates, "commonScaleSpreadTolerance")}."),
    new Gate("operator-unit-scale-target-comparison", p185TargetComparisonPassed, "P185/P166 target comparison must pass before promotion."),
    new Gate("stable-shape-common-scale", p170CommonScaleGatePassed, $"P170 best stable-shape common-scale spread={JsonDouble(p170Gates, "commonScaleRelativeSpread")}."),
    new Gate("stable-shape-target-comparison", p170TargetComparisonPassed, $"P170 best stable-shape max sigma residual={JsonDouble(p170Gates, "maxSigmaResidual")}."),
    new Gate("direct-bridge-raw-gate", p191DirectRawGatePassed, $"P191 direct bridge bestRawToTargetRatio={JsonDouble(p191Gates, "bestRawToTargetRatio")}."),
    new Gate("direct-bridge-theorem", p191TheoremClaimed, "P191 requires a derivation-promoted direct bridge theorem."),
    new Gate("direct-bridge-wz-particle-split", p191WzParticleSplitPresent, "P191 requires separate W and Z source rows."),
};

var canPromoteWzAbsoluteFromVevScale = closureGates.All(gate => gate.Passed);
var terminalStatus = canPromoteWzAbsoluteFromVevScale
    ? "electroweak-vev-wz-absolute-closure-promotable"
    : "electroweak-vev-wz-absolute-closure-blocked-source-shape";

var result = new
{
    phaseId = "phase195-electroweak-vev-wz-absolute-closure-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    canPromoteWzAbsoluteFromVevScale,
    failedWzRows,
    closureGates,
    decision = canPromoteWzAbsoluteFromVevScale
        ? "The electroweak VEV/order-parameter scale and W/Z source gates are sufficient for absolute W/Z promotion."
        : "Do not promote W/Z absolute masses from the VEV/order-parameter scale alone. The scale exists, but the W/Z source-shape/common-scale, target-comparison, direct-bridge theorem, and particle-split gates remain failed.",
    nextRequiredArtifact = "A target-independent W/Z source law that supplies separate W and Z source rows and passes common-scale plus target-comparison gates when combined with the validated electroweak VEV scale.",
    sourceEvidence = new
    {
        phase70Path = Phase70Path,
        phase72Path = Phase72Path,
        phase151Path = Phase151Path,
        phase185Path = Phase185Path,
        phase191Path = Phase191Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "electroweak_vev_wz_absolute_closure_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "electroweak_vev_wz_absolute_closure_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.canPromoteWzAbsoluteFromVevScale,
        result.failedWzRows,
        result.closureGates,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"canPromoteWzAbsoluteFromVevScale={canPromoteWzAbsoluteFromVevScale}");
Console.WriteLine($"failedWzRowCount={failedWzRows.Length}");

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

static double? JsonDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var value) ? value : null;

sealed record Gate(string GateId, bool Passed, string Detail);
