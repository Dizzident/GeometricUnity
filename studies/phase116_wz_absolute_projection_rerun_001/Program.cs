using System.Text.Json;
using System.Text.Json.Serialization;

const string DefaultOutputDir = "studies/phase116_wz_absolute_projection_rerun_001/output";
const string Phase115EvidencePath = "studies/phase115_wz_route_fermion_quality_replay_001/output/wz_route_fermion_quality_replay.json";
const string Phase110ContractPath = "studies/phase110_wz_absolute_repair_execution_contract_001/output/wz_absolute_repair_execution_contract.json";
const string Phase69RelationPath = "studies/phase69_electroweak_mass_generation_relation_001/electroweak_mass_generation_relation.json";
const string Phase54ExternalScalePath = "studies/phase54_external_electroweak_scale_input_001/external_electroweak_scale_input.json";
const string Phase18TargetsPath = "studies/phase18_experimental_targets_001/physical_targets.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE116_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase115 = JsonDocument.Parse(File.ReadAllText(Phase115EvidencePath));
using var contract = JsonDocument.Parse(File.ReadAllText(Phase110ContractPath));
using var phase69 = JsonDocument.Parse(File.ReadAllText(Phase69RelationPath));
using var phase54 = JsonDocument.Parse(File.ReadAllText(Phase54ExternalScalePath));
using var targets = JsonDocument.Parse(File.ReadAllText(Phase18TargetsPath));

bool phase115Accepted = JsonBool(phase115.RootElement, "repairAccepted") is true;
var wRecord = FindRecord(phase115.RootElement, "w-boson");
var zRecord = FindRecord(phase115.RootElement, "z-boson");
double? wCoupling = wRecord is null ? null : JsonDouble(wRecord.Value, "normalizedWeakCoupling");
double? zCoupling = zRecord is null ? null : JsonDouble(zRecord.Value, "normalizedWeakCoupling");
double? wRaw = wRecord is null ? null : JsonDouble(wRecord.Value, "rawMatrixElementMagnitude");
double? zRaw = zRecord is null ? null : JsonDouble(zRecord.Value, "rawMatrixElementMagnitude");

double? v = FindExternalScale(phase54.RootElement, "phase54-fermi-derived-electroweak-vacuum-scale", "value");
double? vUncertainty = FindExternalScale(phase54.RootElement, "phase54-fermi-derived-electroweak-vacuum-scale", "standardUncertainty");
double? wInternal = JsonDouble(phase69.RootElement, "wInternalMass");
double? zInternal = JsonDouble(phase69.RootElement, "zInternalMass");
double? internalRatio = JsonDouble(phase69.RootElement, "internalWzRatio");
double? targetRaw = JsonDouble(contract.RootElement.GetProperty("repairTarget"), "targetImpliedRawMatrixElementMagnitude");
double? targetWeakCoupling = JsonDouble(contract.RootElement.GetProperty("repairTarget"), "targetImpliedWeakCoupling");

var projectionBlockers = new List<string>();
if (!phase115Accepted)
    projectionBlockers.Add("Phase115 repaired W/Z-route evidence is not accepted");
if (wCoupling is not { } wg || !double.IsFinite(wg) || wg <= 0.0)
    projectionBlockers.Add("repaired W-route normalized weak coupling is missing or non-positive");
if (v is not { } vev || !double.IsFinite(vev) || vev <= 0.0)
    projectionBlockers.Add("external electroweak scale v is missing or non-positive");
if (vUncertainty is not { } vevUncertainty || !double.IsFinite(vevUncertainty) || vevUncertainty < 0.0)
    projectionBlockers.Add("external electroweak scale uncertainty is missing or invalid");
if (wInternal is not { } wi || !double.IsFinite(wi) || wi <= 0.0)
    projectionBlockers.Add("validated W internal mass is missing or non-positive");
if (zInternal is not { } zi || !double.IsFinite(zi) || zi <= 0.0)
    projectionBlockers.Add("validated Z internal mass is missing or non-positive");
if (internalRatio is not { } ratio || !double.IsFinite(ratio) || ratio <= 0.0)
    projectionBlockers.Add("validated internal W/Z ratio is missing or non-positive");

double? wCoefficient = null;
double? zCoefficient = null;
double? bridge = null;
double? scale = null;
var predictions = new List<ProjectionRecord>();
if (projectionBlockers.Count == 0)
{
    wCoefficient = wCoupling!.Value / 2.0;
    zCoefficient = wCoefficient.Value / internalRatio!.Value;
    bridge = wCoefficient.Value / wInternal!.Value;
    scale = v!.Value * bridge.Value;
    double scaleUncertainty = vUncertainty!.Value * bridge.Value;
    predictions.Add(Project(
        "physical-w-boson-mass-gev",
        "w-boson",
        "phase22-phase12-candidate-0",
        wInternal.Value,
        scale.Value,
        scaleUncertainty));
    predictions.Add(Project(
        "physical-z-boson-mass-gev",
        "z-boson",
        "phase22-phase12-candidate-2",
        zInternal!.Value,
        scale.Value,
        scaleUncertainty));
}

var targetRecords = ReadTargets(targets.RootElement);
var comparisons = projectionBlockers.Count == 0
    ? Compare(predictions, targetRecords, sigmaThreshold: 5.0)
    : [];
bool comparisonPassed = projectionBlockers.Count == 0 && comparisons.Count > 0 && comparisons.All(c => c.Passed);
double? wBridgeFromRepairedW = wCoupling is { } wCouplingValue && wInternal is { } wInternalValue
    ? wCouplingValue / 2.0 / wInternalValue
    : null;
double? zBridgeFromRepairedZ = zCoupling is { } zCouplingValue && zInternal is { } zInternalValue
    ? zCouplingValue / 2.0 / zInternalValue
    : null;
double? repairedBridgeSpread = RelativeSpread(wBridgeFromRepairedW, zBridgeFromRepairedZ);

var closure = new List<string>();
if (projectionBlockers.Count > 0)
{
    closure.AddRange(projectionBlockers);
}
else if (!comparisonPassed)
{
    closure.Add("rerun absolute W/Z projection fails the Phase18 physical target comparison");
    closure.Add("repaired W-route raw matrix element is far below the Phase110 target-implied raw magnitude");
    if (repairedBridgeSpread is { } spread && spread > 1e-12)
        closure.Add("repaired W/Z-route matrix elements do not define a common shared W/Z bridge scale");
}

var terminalStatus = projectionBlockers.Count > 0
    ? "wz-absolute-projection-rerun-blocked"
    : comparisonPassed
        ? "wz-absolute-projection-rerun-target-comparison-passed"
        : "wz-absolute-projection-rerun-target-comparison-failed";

var result = new
{
    phaseId = "phase116-wz-absolute-projection-rerun",
    terminalStatus,
    projectionStatus = projectionBlockers.Count == 0 ? "projected" : "blocked",
    comparisonStatus = projectionBlockers.Count == 0
        ? comparisonPassed ? "wz-absolute-mass-target-comparison-passed" : "wz-absolute-mass-target-comparison-failed"
        : "not-run",
    phase115Accepted,
    sharedWeakCouplingInput = new
    {
        sourceParticleId = "w-boson",
        normalizedWeakCoupling = wCoupling,
        rawMatrixElementMagnitude = wRaw,
        targetImpliedWeakCoupling = targetWeakCoupling,
        targetImpliedRawMatrixElementMagnitude = targetRaw,
        weakCouplingToTargetRatio = Ratio(wCoupling, targetWeakCoupling),
        rawMatrixElementToTargetRatio = Ratio(wRaw, targetRaw),
    },
    zRouteConsistencyDiagnostic = new
    {
        normalizedWeakCoupling = zCoupling,
        rawMatrixElementMagnitude = zRaw,
        weakCouplingToTargetRatio = Ratio(zCoupling, targetWeakCoupling),
        rawMatrixElementToTargetRatio = Ratio(zRaw, targetRaw),
        directZMassIfUsedAsIndependentCoefficientGeV = Product(v, zCoupling, 0.5),
        wBridgeFromRepairedW,
        zBridgeFromRepairedZ,
        repairedBridgeRelativeSpread = repairedBridgeSpread,
        commonBridgeTolerance = 1e-12,
        commonBridgePassed = repairedBridgeSpread is { } s && s <= 1e-12,
    },
    rerunRelation = new
    {
        relationId = "mass-generation:electroweak-vev-times-normalized-weak-coupling:v1",
        wInternalMass = wInternal,
        zInternalMass = zInternal,
        internalWzRatio = internalRatio,
        wElectroweakCoefficient = wCoefficient,
        zElectroweakCoefficient = zCoefficient,
        dimensionlessBridgeValue = bridge,
        scaleFactorGeVPerInternalMassUnit = scale,
        scaleUncertaintyGeVPerInternalMassUnit = projectionBlockers.Count == 0 ? vUncertainty!.Value * bridge!.Value : (double?)null,
        excludedTargetObservableIds = new[] { "physical-w-boson-mass-gev", "physical-z-boson-mass-gev" },
    },
    projection = new
    {
        status = projectionBlockers.Count == 0 ? "projected" : "blocked",
        observables = predictions,
        blockReasons = projectionBlockers,
        uncertaintyPolicy = "external-electroweak-scale-only; repaired raw matrix-element uncertainty is not yet materialized",
    },
    targetComparison = new
    {
        algorithmId = "phase74-wz-absolute-mass-target-comparator-v1",
        sigmaThreshold = 5.0,
        terminalStatus = projectionBlockers.Count == 0
            ? comparisonPassed ? "wz-absolute-mass-target-comparison-passed" : "wz-absolute-mass-target-comparison-failed"
            : "not-run",
        comparisons,
    },
    closureRequirements = closure,
    nextAction = comparisonPassed
        ? "promote rerun W/Z absolute projection"
        : "do not promote; investigate the repaired matrix-element amplitude and common W/Z bridge inconsistency before another projection rerun",
    sourceEvidence = new
    {
        phase115EvidencePath = Phase115EvidencePath,
        phase110ContractPath = Phase110ContractPath,
        phase69RelationPath = Phase69RelationPath,
        phase54ExternalScalePath = Phase54ExternalScalePath,
        phase18TargetsPath = Phase18TargetsPath,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true };
File.WriteAllText(
    Path.Combine(outputDir, "wz_absolute_projection_rerun.json"),
    JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "wz_absolute_projection_rerun_summary.json"),
    JsonSerializer.Serialize(new
    {
        phaseId = "phase116-wz-absolute-projection-rerun",
        terminalStatus,
        result.projectionStatus,
        result.comparisonStatus,
        comparisonPassed,
        closureRequirements = closure,
        result.nextAction,
    }, options));

Console.WriteLine(terminalStatus);
foreach (var comparison in comparisons)
    Console.WriteLine($"{comparison.ObservableId}: predicted={comparison.PredictedValue:R}, target={comparison.TargetValue:R}, sigma={comparison.SigmaResidual:R}, passed={comparison.Passed}");

static JsonElement? FindRecord(JsonElement root, string particleId)
{
    if (!root.TryGetProperty("records", out var records) || records.ValueKind != JsonValueKind.Array)
        return null;

    foreach (var record in records.EnumerateArray())
    {
        if (string.Equals(JsonString(record, "particleId"), particleId, StringComparison.Ordinal))
            return record.Clone();
    }

    return null;
}

static double? FindExternalScale(JsonElement root, string scaleId, string propertyName)
{
    if (!root.TryGetProperty("derivedExternalScaleCandidates", out var scales) || scales.ValueKind != JsonValueKind.Array)
        return null;

    foreach (var scale in scales.EnumerateArray())
    {
        if (string.Equals(JsonString(scale, "scaleId"), scaleId, StringComparison.Ordinal))
            return JsonDouble(scale, propertyName);
    }

    return null;
}

static IReadOnlyList<TargetRecord> ReadTargets(JsonElement root)
{
    if (!root.TryGetProperty("targets", out var targets) || targets.ValueKind != JsonValueKind.Array)
        return [];

    return targets.EnumerateArray()
        .Where(target =>
            string.Equals(JsonString(target, "observableId"), "physical-w-boson-mass-gev", StringComparison.Ordinal) ||
            string.Equals(JsonString(target, "observableId"), "physical-z-boson-mass-gev", StringComparison.Ordinal))
        .Select(target => new TargetRecord
        {
            ObservableId = JsonString(target, "observableId") ?? "",
            TargetValue = JsonDouble(target, "value") ?? double.NaN,
            TargetUncertainty = JsonDouble(target, "uncertainty") ?? double.NaN,
        })
        .ToList();
}

static ProjectionRecord Project(
    string observableId,
    string particleId,
    string sourceObservableId,
    double internalMass,
    double scale,
    double scaleUncertainty)
{
    double value = internalMass * scale;
    double uncertainty = internalMass * scaleUncertainty;
    return new ProjectionRecord
    {
        ObservableId = observableId,
        ParticleId = particleId,
        SourceObservableId = sourceObservableId,
        Value = value,
        TotalUncertainty = uncertainty,
        BranchId = "bg-variant-53b598740d9569b4",
        EnvironmentId = "env-imported-repo-benchmark",
        RefinementLevel = "L0-2x2",
        ExtractionMethod = $"phase116-repaired-wz-absolute-projection-rerun:{sourceObservableId}",
        DistributionModel = "gaussian",
    };
}

static IReadOnlyList<ComparisonRecord> Compare(
    IReadOnlyList<ProjectionRecord> predictions,
    IReadOnlyList<TargetRecord> targets,
    double sigmaThreshold)
{
    var comparisons = new List<ComparisonRecord>();
    foreach (var target in targets)
    {
        var prediction = predictions.SingleOrDefault(p => string.Equals(p.ObservableId, target.ObservableId, StringComparison.Ordinal));
        if (prediction is null)
            continue;

        double delta = prediction.Value - target.TargetValue;
        double combined = Math.Sqrt(
            prediction.TotalUncertainty * prediction.TotalUncertainty +
            target.TargetUncertainty * target.TargetUncertainty);
        double sigma = combined > 0.0 ? Math.Abs(delta) / combined : double.PositiveInfinity;
        comparisons.Add(new ComparisonRecord
        {
            ObservableId = target.ObservableId,
            PredictedValue = prediction.Value,
            PredictedUncertainty = prediction.TotalUncertainty,
            TargetValue = target.TargetValue,
            TargetUncertainty = target.TargetUncertainty,
            Delta = delta,
            CombinedUncertainty = combined,
            SigmaResidual = sigma,
            Passed = sigma <= sigmaThreshold,
        });
    }

    return comparisons;
}

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
        ? value.GetString()
        : null;

static double? JsonDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetDouble(out var d)
        ? d
        : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind is JsonValueKind.True or JsonValueKind.False
        ? value.GetBoolean()
        : null;

static double? Ratio(double? numerator, double? denominator) =>
    numerator is { } n && denominator is { } d && double.IsFinite(n) && double.IsFinite(d) && Math.Abs(d) > 0.0
        ? n / d
        : null;

static double? Product(double? a, double? b, double c) =>
    a is { } x && b is { } y && double.IsFinite(x) && double.IsFinite(y) && double.IsFinite(c)
        ? x * y * c
        : null;

static double? RelativeSpread(double? a, double? b)
{
    if (a is not { } x || b is not { } y || !double.IsFinite(x) || !double.IsFinite(y))
        return null;

    double mean = (x + y) / 2.0;
    return Math.Abs(mean) > 0.0 ? Math.Abs(x - y) / Math.Abs(mean) : null;
}

public sealed class ProjectionRecord
{
    [JsonPropertyName("observableId")]
    public required string ObservableId { get; init; }

    [JsonPropertyName("particleId")]
    public required string ParticleId { get; init; }

    [JsonPropertyName("sourceObservableId")]
    public required string SourceObservableId { get; init; }

    [JsonPropertyName("value")]
    public required double Value { get; init; }

    [JsonPropertyName("totalUncertainty")]
    public required double TotalUncertainty { get; init; }

    [JsonPropertyName("branchId")]
    public required string BranchId { get; init; }

    [JsonPropertyName("environmentId")]
    public required string EnvironmentId { get; init; }

    [JsonPropertyName("refinementLevel")]
    public required string RefinementLevel { get; init; }

    [JsonPropertyName("extractionMethod")]
    public required string ExtractionMethod { get; init; }

    [JsonPropertyName("distributionModel")]
    public required string DistributionModel { get; init; }
}

public sealed class TargetRecord
{
    [JsonPropertyName("observableId")]
    public required string ObservableId { get; init; }

    [JsonPropertyName("targetValue")]
    public required double TargetValue { get; init; }

    [JsonPropertyName("targetUncertainty")]
    public required double TargetUncertainty { get; init; }
}

public sealed class ComparisonRecord
{
    [JsonPropertyName("observableId")]
    public required string ObservableId { get; init; }

    [JsonPropertyName("predictedValue")]
    public required double PredictedValue { get; init; }

    [JsonPropertyName("predictedUncertainty")]
    public required double PredictedUncertainty { get; init; }

    [JsonPropertyName("targetValue")]
    public required double TargetValue { get; init; }

    [JsonPropertyName("targetUncertainty")]
    public required double TargetUncertainty { get; init; }

    [JsonPropertyName("delta")]
    public required double Delta { get; init; }

    [JsonPropertyName("combinedUncertainty")]
    public required double CombinedUncertainty { get; init; }

    [JsonPropertyName("sigmaResidual")]
    public required double SigmaResidual { get; init; }

    [JsonPropertyName("passed")]
    public required bool Passed { get; init; }
}
