using System.Text.Json;

const string DefaultOutputDir = "studies/phase284_predicted_ratio_alpha_gf_external_closure_diagnostic_001/output";
const string Phase54Path = "studies/phase54_external_electroweak_scale_input_001/external_electroweak_scale_input.json";
const string Phase148Path = "studies/phase148_all_known_boson_prediction_comparison_001/output/all_known_boson_prediction_comparison.json";
const string Phase203Path = "studies/phase203_defensible_boson_value_manifest_001/output/defensible_boson_value_manifest_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase214Path = "studies/phase214_external_electroweak_input_loophole_audit_001/output/external_electroweak_input_loophole_audit_summary.json";
const string Phase224Path = "studies/phase224_electroweak_parameter_dependency_audit_001/output/electroweak_parameter_dependency_audit_summary.json";
const string Phase236Path = "studies/phase236_low_energy_rg_transport_source_audit_001/output/low_energy_rg_transport_source_audit_summary.json";
const string Phase245Path = "studies/phase245_rank_deficit_minimal_unlock_contract_001/output/rank_deficit_minimal_unlock_contract_summary.json";
const string Phase261Path = "studies/phase261_electroweak_scheme_radiative_source_audit_001/output/electroweak_scheme_radiative_source_audit_summary.json";
const string Phase283Path = "studies/phase283_legacy_electroweak_bridge_source_survivability_audit_001/output/legacy_electroweak_bridge_source_survivability_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE284_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase54 = JsonDocument.Parse(File.ReadAllText(Phase54Path));
using var phase148 = JsonDocument.Parse(File.ReadAllText(Phase148Path));
using var phase203 = JsonDocument.Parse(File.ReadAllText(Phase203Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase214 = JsonDocument.Parse(File.ReadAllText(Phase214Path));
using var phase224 = JsonDocument.Parse(File.ReadAllText(Phase224Path));
using var phase236 = JsonDocument.Parse(File.ReadAllText(Phase236Path));
using var phase245 = JsonDocument.Parse(File.ReadAllText(Phase245Path));
using var phase261 = JsonDocument.Parse(File.ReadAllText(Phase261Path));
using var phase283 = JsonDocument.Parse(File.ReadAllText(Phase283Path));

var ratioRow = phase203.RootElement.GetProperty("defensibleValues")
    .EnumerateArray()
    .First(row => string.Equals(JsonString(row, "particleId"), "electroweak-sector", StringComparison.Ordinal)
        && string.Equals(JsonString(row, "observableId"), "physical-w-z-mass-ratio", StringComparison.Ordinal));
var guWzRatio = RequiredDouble(ratioRow, "predictedValue");
var guWzRatioUncertainty = RequiredDouble(ratioRow, "predictedUncertainty");
var guWzRatioSourcePromoted = JsonBool(ratioRow, "gatePromoted") is true;
var guSin2Theta = 1.0 - guWzRatio * guWzRatio;
var guSin2ThetaUncertainty = 2.0 * Math.Abs(guWzRatio) * guWzRatioUncertainty;
if (guSin2Theta <= 0.0)
{
    throw new InvalidOperationException($"Invalid GU W/Z ratio for weak-mixing diagnostic: {guWzRatio:R}.");
}

var vevRow = phase54.RootElement.GetProperty("derivedExternalScaleCandidates")
    .EnumerateArray()
    .First(row => string.Equals(JsonString(row, "scaleId"), "phase54-fermi-derived-electroweak-vacuum-scale", StringComparison.Ordinal));
var externalVevGeV = RequiredDouble(vevRow, "value");
var externalVevUncertaintyGeV = RequiredDouble(vevRow, "standardUncertainty");
var externalVevIngested = string.Equals(JsonString(vevRow, "status"), "ingested", StringComparison.Ordinal);
var phase54InternalBridgeBlocked = phase54.RootElement.TryGetProperty("internalBridgeStatus", out var p54InternalBridge)
    && string.Equals(JsonString(p54InternalBridge, "status"), "blocked", StringComparison.Ordinal);

var comparisonRows = phase148.RootElement.GetProperty("comparisonRows").EnumerateArray().ToArray();
var wTargetRow = FindRow(comparisonRows, "w-boson");
var zTargetRow = FindRow(comparisonRows, "z-boson");
var targetWGeV = RequiredDouble(wTargetRow, "targetValue");
var targetWUncertaintyGeV = RequiredDouble(wTargetRow, "targetUncertainty");
var targetZGeV = RequiredDouble(zTargetRow, "targetValue");
var targetZUncertaintyGeV = RequiredDouble(zTargetRow, "targetUncertainty");

var alphaInputs = phase261.RootElement.GetProperty("schemeInputs");
var alphaZeroInverse = RequiredDouble(alphaInputs, "alphaZeroInverse");
var alphaMzInverse = RequiredDouble(alphaInputs, "alphaMzInverse");
const double TargetComparisonSigmaGate = 3.0;

var diagnosticRows = new[]
{
    BuildRow("alpha0-gu-ratio-gf-vev", "alpha(0) inverse from Phase261 external electroweak scheme audit", alphaZeroInverse),
    BuildRow("alphaMz-gu-ratio-gf-vev", "alpha(MZ) inverse from Phase261 external electroweak scheme audit", alphaMzInverse),
};
var bestRowByMaxSigmaResidual = diagnosticRows
    .OrderBy(row => Math.Max(row.WPullOrSigmaResidual, row.ZPullOrSigmaResidual))
    .First();
var anyRowPassesWzTargetComparison = diagnosticRows.Any(row => row.TargetComparisonPassed);
var alphaZeroRowFailsWzTargetComparison = diagnosticRows.Any(row => string.Equals(row.RowId, "alpha0-gu-ratio-gf-vev", StringComparison.Ordinal)
    && !row.TargetComparisonPassed);
var alphaMzRowPassesWzTargetComparison = diagnosticRows.Any(row => string.Equals(row.RowId, "alphaMz-gu-ratio-gf-vev", StringComparison.Ordinal)
    && row.TargetComparisonPassed);

var externalInputsUsed = true;
var targetMassesUsedForConstruction = false;
var completeGuSourceLineagePresent = false;
var alphaSourceLineagePresent = false;
var vevSourceLineagePresent = false;
var lowEnergyRgTransportSourcePresent = false;
var promotesBosonMasses = false;
var wZAbsoluteScaleSourceLawFound = false;
var higgsScalarScaleSourceLawFound = false;
var sourceContractsFilled = false;
var newSourceEvidenceStillRequired = true;

var canPromoteExternalElectroweakBridge = JsonBool(phase214.RootElement, "canPromoteExternalElectroweakBridge") is true;
var electroweakParameterAuditPassed = JsonBool(phase224.RootElement, "electroweakParameterAuditPassed") is true;
var wAbsoluteMassParameterClosure = phase224.RootElement.TryGetProperty("closure", out var p224Closure)
    && JsonBool(p224Closure, "wAbsoluteMassParameterClosure") is true;
var zAbsoluteMassParameterClosure = phase224.RootElement.TryGetProperty("closure", out p224Closure)
    && JsonBool(p224Closure, "zAbsoluteMassParameterClosure") is true;
var lowEnergyRgTransportSourceAuditPassed = JsonBool(phase236.RootElement, "lowEnergyRgTransportSourceAuditPassed") is true;
var lowEnergyRgTransportSourcePromotable = JsonBool(phase236.RootElement, "lowEnergyRgTransportSourcePromotable") is true;
var unlockContractFilled = JsonBool(phase245.RootElement, "unlockContractFilled") is true;
var phase245NewSourceEvidenceStillRequired = JsonBool(phase245.RootElement, "newSourceEvidenceStillRequired") is true;
var legacyBridgeSourceSurvivabilityAuditPassed = JsonBool(phase283.RootElement, "legacyBridgeSourceSurvivabilityAuditPassed") is true;
var legacyBridgeRoutePromotableForBosonMasses = JsonBool(phase283.RootElement, "legacyBridgeRoutePromotableForBosonMasses") is true;
var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;

var checks = new[]
{
    new Check(
        "promoted-gu-ratio-and-external-inputs-materialized",
        guWzRatioSourcePromoted && externalVevIngested && phase54InternalBridgeBlocked && alphaZeroInverse > 0.0 && alphaMzInverse > 0.0,
        $"guWzRatioSourcePromoted={guWzRatioSourcePromoted}; externalVevIngested={externalVevIngested}; phase54InternalBridgeBlocked={phase54InternalBridgeBlocked}; alphaZeroInverse={alphaZeroInverse:R}; alphaMzInverse={alphaMzInverse:R}"),
    new Check(
        "alpha-mz-external-row-numerically-closes-wz-under-existing-sigma-gate",
        alphaMzRowPassesWzTargetComparison && anyRowPassesWzTargetComparison,
        $"alphaMzRowPassesWzTargetComparison={alphaMzRowPassesWzTargetComparison}; anyRowPassesWzTargetComparison={anyRowPassesWzTargetComparison}; bestRow={bestRowByMaxSigmaResidual.RowId}; bestMaxSigmaResidual={Math.Max(bestRowByMaxSigmaResidual.WPullOrSigmaResidual, bestRowByMaxSigmaResidual.ZPullOrSigmaResidual):R}"),
    new Check(
        "alpha-zero-row-does-not-close-wz",
        alphaZeroRowFailsWzTargetComparison,
        $"alphaZeroRowFailsWzTargetComparison={alphaZeroRowFailsWzTargetComparison}"),
    new Check(
        "construction-is-target-independent-with-respect-to-wz-masses",
        !targetMassesUsedForConstruction,
        $"targetMassesUsedForConstruction={targetMassesUsedForConstruction}; targetMassesUsedOnlyForAposterioriComparison=true"),
    new Check(
        "external-input-closure-is-not-gu-source-law",
        externalInputsUsed
            && !completeGuSourceLineagePresent
            && !alphaSourceLineagePresent
            && !vevSourceLineagePresent
            && !lowEnergyRgTransportSourcePresent
            && !promotesBosonMasses
            && !sourceContractsFilled,
        $"externalInputsUsed={externalInputsUsed}; completeGuSourceLineagePresent={completeGuSourceLineagePresent}; alphaSourceLineagePresent={alphaSourceLineagePresent}; vevSourceLineagePresent={vevSourceLineagePresent}; lowEnergyRgTransportSourcePresent={lowEnergyRgTransportSourcePresent}; promotesBosonMasses={promotesBosonMasses}; sourceContractsFilled={sourceContractsFilled}"),
    new Check(
        "existing-source-lineage-blockers-remain-binding",
        !canPromoteExternalElectroweakBridge
            && electroweakParameterAuditPassed
            && !wAbsoluteMassParameterClosure
            && !zAbsoluteMassParameterClosure
            && lowEnergyRgTransportSourceAuditPassed
            && !lowEnergyRgTransportSourcePromotable
            && !unlockContractFilled
            && phase245NewSourceEvidenceStillRequired
            && legacyBridgeSourceSurvivabilityAuditPassed
            && !legacyBridgeRoutePromotableForBosonMasses
            && wzMissingFieldCount > 0
            && higgsMissingFieldCount > 0,
        $"canPromoteExternalElectroweakBridge={canPromoteExternalElectroweakBridge}; wAbsoluteMassParameterClosure={wAbsoluteMassParameterClosure}; zAbsoluteMassParameterClosure={zAbsoluteMassParameterClosure}; lowEnergyRgTransportSourcePromotable={lowEnergyRgTransportSourcePromotable}; unlockContractFilled={unlockContractFilled}; legacyBridgeRoutePromotableForBosonMasses={legacyBridgeRoutePromotableForBosonMasses}; wzMissingFieldCount={wzMissingFieldCount}; higgsMissingFieldCount={higgsMissingFieldCount}"),
};

var predictedRatioAlphaGfExternalClosureDiagnosticPassed = checks.All(check => check.Passed)
    && anyRowPassesWzTargetComparison
    && !promotesBosonMasses
    && !wZAbsoluteScaleSourceLawFound
    && !higgsScalarScaleSourceLawFound
    && !sourceContractsFilled
    && newSourceEvidenceStillRequired;
var terminalStatus = predictedRatioAlphaGfExternalClosureDiagnosticPassed
    ? "predicted-ratio-alpha-gf-external-closure-diagnostic-target-pass-not-promotable"
    : "predicted-ratio-alpha-gf-external-closure-diagnostic-review-required";

var result = new
{
    phaseId = "phase284-predicted-ratio-alpha-gf-external-closure-diagnostic",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    predictedRatioAlphaGfExternalClosureDiagnosticPassed,
    anyRowPassesWzTargetComparison,
    alphaMzRowPassesWzTargetComparison,
    alphaZeroRowFailsWzTargetComparison,
    externalInputsUsed,
    targetMassesUsedForConstruction,
    guWzRatioSourcePromoted,
    completeGuSourceLineagePresent,
    alphaSourceLineagePresent,
    vevSourceLineagePresent,
    lowEnergyRgTransportSourcePresent,
    promotesBosonMasses,
    wZAbsoluteScaleSourceLawFound,
    higgsScalarScaleSourceLawFound,
    sourceContractsFilled,
    newSourceEvidenceStillRequired,
    inputScalars = new
    {
        guWzRatio,
        guWzRatioUncertainty,
        guSin2Theta,
        guSin2ThetaUncertainty,
        externalVevGeV,
        externalVevUncertaintyGeV,
        alphaZeroInverse,
        alphaMzInverse,
        targetComparisonSigmaGate = TargetComparisonSigmaGate,
    },
    targetComparison = new
    {
        targetWGeV,
        targetWUncertaintyGeV,
        targetZGeV,
        targetZUncertaintyGeV,
        targetMassesUsedForConstruction,
        targetMassesUsedForAposterioriComparison = true,
    },
    diagnosticRows,
    bestRowByMaxSigmaResidual,
    sourceLineageBoundary = new
    {
        phase54InternalBridgeBlocked,
        canPromoteExternalElectroweakBridge,
        electroweakParameterAuditPassed,
        wAbsoluteMassParameterClosure,
        zAbsoluteMassParameterClosure,
        lowEnergyRgTransportSourceAuditPassed,
        lowEnergyRgTransportSourcePromotable,
        unlockContractFilled,
        phase245NewSourceEvidenceStillRequired,
        legacyBridgeSourceSurvivabilityAuditPassed,
        legacyBridgeRoutePromotableForBosonMasses,
        wzMissingFieldCount,
        higgsMissingFieldCount,
    },
    checks,
    decision = predictedRatioAlphaGfExternalClosureDiagnosticPassed
        ? "The promoted GU W/Z ratio plus external alpha(MZ) and the external Fermi-derived VEV numerically closes W and Z inside the existing broad sigma comparison gate. This is a diagnostic closure only: alpha, VEV, and low-energy transport are imported electroweak inputs, no complete GU source lineage or observed-field extraction is supplied, and the Phase213/245 source contracts remain unfilled."
        : "Review the external alpha/GF closure diagnostic before using it as numerical evidence.",
    nextRequiredArtifact = new[]
    {
        "Promote no W/Z physical masses from this diagnostic. Replace the external alpha/GF inputs with GU-derived source rows for the low-energy electroweak coupling and VEV, or a direct GU source for log(v g).",
        "Materialize observed W/Z field extraction and source-lineage fields before any physical W/Z mass claim.",
        "Independently solve the Higgs scalar-source/self-coupling lineage; this W/Z diagnostic does not address the Higgs mass blocker.",
    },
    sourceEvidence = new
    {
        phase54Path = Phase54Path,
        phase148Path = Phase148Path,
        phase203Path = Phase203Path,
        phase213Path = Phase213Path,
        phase214Path = Phase214Path,
        phase224Path = Phase224Path,
        phase236Path = Phase236Path,
        phase245Path = Phase245Path,
        phase261Path = Phase261Path,
        phase283Path = Phase283Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "predicted_ratio_alpha_gf_external_closure_diagnostic.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "predicted_ratio_alpha_gf_external_closure_diagnostic_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.predictedRatioAlphaGfExternalClosureDiagnosticPassed,
        result.anyRowPassesWzTargetComparison,
        result.alphaMzRowPassesWzTargetComparison,
        result.alphaZeroRowFailsWzTargetComparison,
        result.externalInputsUsed,
        result.targetMassesUsedForConstruction,
        result.guWzRatioSourcePromoted,
        result.completeGuSourceLineagePresent,
        result.alphaSourceLineagePresent,
        result.vevSourceLineagePresent,
        result.lowEnergyRgTransportSourcePresent,
        result.promotesBosonMasses,
        result.wZAbsoluteScaleSourceLawFound,
        result.higgsScalarScaleSourceLawFound,
        result.sourceContractsFilled,
        result.newSourceEvidenceStillRequired,
        result.inputScalars,
        result.targetComparison,
        result.diagnosticRows,
        result.bestRowByMaxSigmaResidual,
        result.sourceLineageBoundary,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"predictedRatioAlphaGfExternalClosureDiagnosticPassed={predictedRatioAlphaGfExternalClosureDiagnosticPassed}");
Console.WriteLine($"anyRowPassesWzTargetComparison={anyRowPassesWzTargetComparison}");
Console.WriteLine($"promotesBosonMasses={promotesBosonMasses}");
Console.WriteLine($"bestRow={bestRowByMaxSigmaResidual.RowId}");

DiagnosticRow BuildRow(string rowId, string alphaInputId, double alphaInverse)
{
    var alpha = 1.0 / alphaInverse;
    var electricCharge = Math.Sqrt(4.0 * Math.PI * alpha);
    var weakCoupling = electricCharge / Math.Sqrt(guSin2Theta);
    var predictedWGeV = weakCoupling * externalVevGeV / 2.0;
    var predictedZGeV = predictedWGeV / guWzRatio;
    var wRelativeUncertainty = Math.Sqrt(
        Math.Pow(guWzRatio * guWzRatioUncertainty / guSin2Theta, 2.0)
        + Math.Pow(externalVevUncertaintyGeV / externalVevGeV, 2.0));
    var zRatioDerivative = -1.0 / guWzRatio + guWzRatio / guSin2Theta;
    var zRelativeUncertainty = Math.Sqrt(
        Math.Pow(Math.Abs(zRatioDerivative) * guWzRatioUncertainty, 2.0)
        + Math.Pow(externalVevUncertaintyGeV / externalVevGeV, 2.0));
    var predictedWUncertaintyGeV = predictedWGeV * wRelativeUncertainty;
    var predictedZUncertaintyGeV = predictedZGeV * zRelativeUncertainty;
    var wResidualGeV = predictedWGeV - targetWGeV;
    var zResidualGeV = predictedZGeV - targetZGeV;
    var wPullOrSigmaResidual = Math.Abs(wResidualGeV) / Math.Sqrt(
        predictedWUncertaintyGeV * predictedWUncertaintyGeV
        + targetWUncertaintyGeV * targetWUncertaintyGeV);
    var zPullOrSigmaResidual = Math.Abs(zResidualGeV) / Math.Sqrt(
        predictedZUncertaintyGeV * predictedZUncertaintyGeV
        + targetZUncertaintyGeV * targetZUncertaintyGeV);
    var targetComparisonPassed = wPullOrSigmaResidual <= TargetComparisonSigmaGate
        && zPullOrSigmaResidual <= TargetComparisonSigmaGate;

    return new DiagnosticRow(
        rowId,
        alphaInputId,
        alphaInverse,
        alpha,
        electricCharge,
        guSin2Theta,
        weakCoupling,
        predictedWGeV,
        predictedWUncertaintyGeV,
        predictedZGeV,
        predictedZUncertaintyGeV,
        targetWGeV,
        targetZGeV,
        wResidualGeV,
        zResidualGeV,
        wPullOrSigmaResidual,
        zPullOrSigmaResidual,
        TargetComparisonSigmaGate,
        targetComparisonPassed,
        ExternalInputsUsed: true,
        TargetMassesUsedForConstruction: false,
        PromotesBosonMasses: false);
}

static JsonElement FindRow(IEnumerable<JsonElement> rows, string particleId) =>
    rows.First(row => string.Equals(JsonString(row, "particleId"), particleId, StringComparison.Ordinal));

static double RequiredDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number
        ? property.GetDouble()
        : throw new InvalidOperationException($"Missing numeric property '{propertyName}'.");

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

sealed record DiagnosticRow(
    string RowId,
    string AlphaInputId,
    double AlphaInverse,
    double Alpha,
    double ElectricCharge,
    double GuSin2Theta,
    double WeakCoupling,
    double PredictedWGeV,
    double PredictedWUncertaintyGeV,
    double PredictedZGeV,
    double PredictedZUncertaintyGeV,
    double TargetWGeV,
    double TargetZGeV,
    double WResidualGeV,
    double ZResidualGeV,
    double WPullOrSigmaResidual,
    double ZPullOrSigmaResidual,
    double TargetComparisonSigmaGate,
    bool TargetComparisonPassed,
    bool ExternalInputsUsed,
    bool TargetMassesUsedForConstruction,
    bool PromotesBosonMasses);

sealed record Check(string CheckId, bool Passed, string Detail);
