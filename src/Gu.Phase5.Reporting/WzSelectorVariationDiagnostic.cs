using System.Text.Json.Serialization;
using Gu.Core;
using Gu.Core.Serialization;
using Gu.Phase5.QuantitativeValidation;

namespace Gu.Phase5.Reporting;

public sealed class WzSelectorVariationDiagnosticResult
{
    [JsonPropertyName("resultId")]
    public required string ResultId { get; init; }

    [JsonPropertyName("schemaVersion")]
    public required string SchemaVersion { get; init; }

    [JsonPropertyName("terminalStatus")]
    public required string TerminalStatus { get; init; }

    [JsonPropertyName("algorithmId")]
    public required string AlgorithmId { get; init; }

    [JsonPropertyName("wSourceCandidateId")]
    public string? WSourceCandidateId { get; init; }

    [JsonPropertyName("zSourceCandidateId")]
    public string? ZSourceCandidateId { get; init; }

    [JsonPropertyName("targetValue")]
    public required double TargetValue { get; init; }

    [JsonPropertyName("targetUncertainty")]
    public required double TargetUncertainty { get; init; }

    [JsonPropertyName("alignedPointCount")]
    public required int AlignedPointCount { get; init; }

    [JsonPropertyName("ratioMin")]
    public double? RatioMin { get; init; }

    [JsonPropertyName("ratioMax")]
    public double? RatioMax { get; init; }

    [JsonPropertyName("ratioMean")]
    public double? RatioMean { get; init; }

    [JsonPropertyName("ratioStandardDeviation")]
    public double? RatioStandardDeviation { get; init; }

    [JsonPropertyName("targetInsideObservedEnvelope")]
    public required bool TargetInsideObservedEnvelope { get; init; }

    [JsonPropertyName("passingPointCount")]
    public required int PassingPointCount { get; init; }

    [JsonPropertyName("closestPoint")]
    public WzSelectorVariationPointRecord? ClosestPoint { get; init; }

    [JsonPropertyName("points")]
    public required IReadOnlyList<WzSelectorVariationPointRecord> Points { get; init; }

    [JsonPropertyName("diagnosis")]
    public required IReadOnlyList<string> Diagnosis { get; init; }

    [JsonPropertyName("closureRequirements")]
    public required IReadOnlyList<string> ClosureRequirements { get; init; }

    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}

public sealed class WzSelectorVariationPointRecord
{
    [JsonPropertyName("pointId")]
    public required string PointId { get; init; }

    [JsonPropertyName("branchVariantId")]
    public required string BranchVariantId { get; init; }

    [JsonPropertyName("refinementLevel")]
    public required string RefinementLevel { get; init; }

    [JsonPropertyName("environmentId")]
    public required string EnvironmentId { get; init; }

    [JsonPropertyName("wModeRecordId")]
    public required string WModeRecordId { get; init; }

    [JsonPropertyName("zModeRecordId")]
    public required string ZModeRecordId { get; init; }

    [JsonPropertyName("wValue")]
    public required double WValue { get; init; }

    [JsonPropertyName("zValue")]
    public required double ZValue { get; init; }

    [JsonPropertyName("ratio")]
    public required double Ratio { get; init; }

    [JsonPropertyName("extractionUncertainty")]
    public required double ExtractionUncertainty { get; init; }

    [JsonPropertyName("combinedSigma")]
    public required double CombinedSigma { get; init; }

    [JsonPropertyName("pull")]
    public required double Pull { get; init; }

    [JsonPropertyName("passesSigma5")]
    public required bool PassesSigma5 { get; init; }
}

public static class WzSelectorVariationDiagnostic
{
    public const string AlgorithmId = "p30-wz-selector-variation-diagnostic:v1";

    public static WzSelectorVariationDiagnosticResult Evaluate(
        string identityRuleReadinessJson,
        string modesRoot,
        string targetTableJson,
        ProvenanceMeta provenance)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(identityRuleReadinessJson);
        ArgumentException.ThrowIfNullOrWhiteSpace(modesRoot);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetTableJson);

        var readiness = GuJsonDefaults.Deserialize<VectorBosonIdentityRuleReadinessResult>(identityRuleReadinessJson)
            ?? throw new InvalidDataException("Failed to deserialize W/Z identity-rule readiness.");
        var targetTable = GuJsonDefaults.Deserialize<ExternalTargetTable>(targetTableJson)
            ?? throw new InvalidDataException("Failed to deserialize external target table.");
        var target = targetTable.Targets.FirstOrDefault(t => t.ObservableId == "physical-w-z-mass-ratio")
            ?? throw new InvalidDataException("External target table must contain physical-w-z-mass-ratio.");

        var closure = new List<string>();
        if (!string.Equals(readiness.TerminalStatus, "identity-rule-ready", StringComparison.Ordinal))
            closure.Add("identity-rule readiness is not ready");
        if (!Directory.Exists(modesRoot))
            closure.Add($"modes root does not exist: {modesRoot}");

        var wSourceCandidateId = SourceCandidateId(readiness.DerivedRules.FirstOrDefault(r => r.ParticleId == "w-boson")?.SourceId);
        var zSourceCandidateId = SourceCandidateId(readiness.DerivedRules.FirstOrDefault(r => r.ParticleId == "z-boson")?.SourceId);
        if (string.IsNullOrWhiteSpace(wSourceCandidateId))
            closure.Add("identity readiness must provide a W-boson source");
        if (string.IsNullOrWhiteSpace(zSourceCandidateId))
            closure.Add("identity readiness must provide a Z-boson source");

        var points = new List<WzSelectorVariationPointRecord>();
        if (closure.Count == 0 && wSourceCandidateId is not null && zSourceCandidateId is not null)
        {
            var wModes = LoadModes(modesRoot, wSourceCandidateId);
            var zModes = LoadModes(modesRoot, zSourceCandidateId);
            var zByPoint = zModes.ToDictionary(PointKey, mode => mode, StringComparer.Ordinal);

            foreach (var wMode in wModes.OrderBy(PointKey, StringComparer.Ordinal))
            {
                if (!zByPoint.TryGetValue(PointKey(wMode), out var zMode))
                    continue;
                points.Add(CreatePoint(wMode, zMode, target));
            }

            if (points.Count == 0)
                closure.Add("no aligned W/Z branch-refinement-environment mode records are available");
        }

        var ratioMin = points.Count > 0 ? points.Min(p => p.Ratio) : (double?)null;
        var ratioMax = points.Count > 0 ? points.Max(p => p.Ratio) : (double?)null;
        var ratioMean = points.Count > 0 ? points.Average(p => p.Ratio) : (double?)null;
        var ratioStd = points.Count > 1 && ratioMean is not null
            ? System.Math.Sqrt(points.Sum(p => System.Math.Pow(p.Ratio - ratioMean.Value, 2)) / (points.Count - 1))
            : points.Count == 1 ? 0 : (double?)null;
        var closest = points.OrderBy(p => System.Math.Abs(p.Pull)).FirstOrDefault();
        var targetInside = ratioMin is not null && ratioMax is not null &&
            target.Value >= ratioMin.Value && target.Value <= ratioMax.Value;
        var passingCount = points.Count(p => p.PassesSigma5);

        return new WzSelectorVariationDiagnosticResult
        {
            ResultId = "phase30-wz-selector-variation-diagnostic-v1",
            SchemaVersion = "1.0.0",
            TerminalStatus = closure.Count == 0 ? "selector-variation-diagnostic-complete" : "selector-variation-diagnostic-blocked",
            AlgorithmId = AlgorithmId,
            WSourceCandidateId = wSourceCandidateId,
            ZSourceCandidateId = zSourceCandidateId,
            TargetValue = target.Value,
            TargetUncertainty = target.Uncertainty,
            AlignedPointCount = points.Count,
            RatioMin = ratioMin,
            RatioMax = ratioMax,
            RatioMean = ratioMean,
            RatioStandardDeviation = ratioStd,
            TargetInsideObservedEnvelope = targetInside,
            PassingPointCount = passingCount,
            ClosestPoint = closest,
            Points = points,
            Diagnosis = BuildDiagnosis(points, closest, targetInside, passingCount),
            ClosureRequirements = closure.Distinct(StringComparer.Ordinal).ToList(),
            Provenance = provenance,
        };
    }

    private static WzSelectorVariationPointRecord CreatePoint(
        InternalVectorBosonSourceModeRecord wMode,
        InternalVectorBosonSourceModeRecord zMode,
        ExternalTarget target)
    {
        var ratio = wMode.MassLikeValue / zMode.MassLikeValue;
        var uncertainty = ratio * System.Math.Sqrt(
            System.Math.Pow(wMode.ExtractionError / wMode.MassLikeValue, 2) +
            System.Math.Pow(zMode.ExtractionError / zMode.MassLikeValue, 2));
        var combined = System.Math.Sqrt(uncertainty * uncertainty + target.Uncertainty * target.Uncertainty);
        var pull = (ratio - target.Value) / combined;
        var pointId = PointKey(wMode);
        return new WzSelectorVariationPointRecord
        {
            PointId = pointId,
            BranchVariantId = wMode.BranchVariantId,
            RefinementLevel = wMode.RefinementLevel,
            EnvironmentId = wMode.EnvironmentId,
            WModeRecordId = wMode.ModeRecordId,
            ZModeRecordId = zMode.ModeRecordId,
            WValue = wMode.MassLikeValue,
            ZValue = zMode.MassLikeValue,
            Ratio = ratio,
            ExtractionUncertainty = uncertainty,
            CombinedSigma = combined,
            Pull = pull,
            PassesSigma5 = System.Math.Abs(pull) <= 5.0,
        };
    }

    private static IReadOnlyList<string> BuildDiagnosis(
        IReadOnlyList<WzSelectorVariationPointRecord> points,
        WzSelectorVariationPointRecord? closest,
        bool targetInside,
        int passingCount)
    {
        var diagnosis = new List<string>();
        if (points.Count == 0)
        {
            diagnosis.Add("no aligned selector-variation points were available");
            return diagnosis;
        }

        diagnosis.Add(targetInside
            ? "target lies inside the observed branch-refinement-environment ratio envelope"
            : "target lies outside the observed branch-refinement-environment ratio envelope");
        diagnosis.Add(passingCount > 0
            ? $"{passingCount} aligned selector-variation point(s) pass sigma-5"
            : "no aligned selector-variation point passes sigma-5");
        if (closest is not null)
            diagnosis.Add($"closest aligned point is {closest.PointId} with ratio {closest.Ratio} and pull {closest.Pull}");
        return diagnosis;
    }

    private static IReadOnlyList<InternalVectorBosonSourceModeRecord> LoadModes(string modesRoot, string sourceCandidateId)
        => Directory.EnumerateFiles(modesRoot, $"{sourceCandidateId}__*_mode.json")
            .Select(path => GuJsonDefaults.Deserialize<InternalVectorBosonSourceModeRecord>(File.ReadAllText(path))
                            ?? throw new InvalidDataException($"Failed to deserialize mode record {path}."))
            .ToList();

    private static string PointKey(InternalVectorBosonSourceModeRecord mode)
        => $"{mode.BranchVariantId}::{mode.RefinementLevel}::{mode.EnvironmentId}";

    private static string? SourceCandidateId(string? sourceId)
    {
        if (string.IsNullOrWhiteSpace(sourceId))
            return null;
        return sourceId.StartsWith("phase22-", StringComparison.Ordinal)
            ? sourceId["phase22-".Length..]
            : sourceId;
    }
}
