using System.Text.Json.Serialization;
using Gu.Phase5.QuantitativeValidation;

namespace Gu.Phase5.Reporting;

public sealed class WzAbsoluteMassTargetRecord
{
    [JsonPropertyName("observableId")]
    public required string ObservableId { get; init; }

    [JsonPropertyName("targetValue")]
    public required double TargetValue { get; init; }

    [JsonPropertyName("targetUncertainty")]
    public required double TargetUncertainty { get; init; }
}

public sealed class WzAbsoluteMassComparisonRecord
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

public sealed class WzAbsoluteMassTargetComparisonResult
{
    [JsonPropertyName("algorithmId")]
    public required string AlgorithmId { get; init; }

    [JsonPropertyName("terminalStatus")]
    public required string TerminalStatus { get; init; }

    [JsonPropertyName("sigmaThreshold")]
    public required double SigmaThreshold { get; init; }

    [JsonPropertyName("comparisons")]
    public required IReadOnlyList<WzAbsoluteMassComparisonRecord> Comparisons { get; init; }

    [JsonPropertyName("closureRequirements")]
    public required IReadOnlyList<string> ClosureRequirements { get; init; }
}

public static class WzAbsoluteMassTargetComparator
{
    public const string AlgorithmId = "phase74-wz-absolute-mass-target-comparator-v1";

    public static WzAbsoluteMassTargetComparisonResult Compare(
        IReadOnlyList<QuantitativeObservableRecord> predictions,
        IReadOnlyList<WzAbsoluteMassTargetRecord> targets,
        double sigmaThreshold = 5.0)
    {
        ArgumentNullException.ThrowIfNull(predictions);
        ArgumentNullException.ThrowIfNull(targets);

        var closure = new List<string>();
        if (!double.IsFinite(sigmaThreshold) || sigmaThreshold <= 0.0)
            closure.Add("sigma threshold must be finite and positive");

        var comparisons = new List<WzAbsoluteMassComparisonRecord>();
        foreach (var target in targets)
        {
            var prediction = predictions.SingleOrDefault(p => string.Equals(p.ObservableId, target.ObservableId, StringComparison.Ordinal));
            if (prediction is null)
            {
                closure.Add($"prediction is missing for {target.ObservableId}");
                continue;
            }

            if (!double.IsFinite(prediction.Value))
                closure.Add($"prediction value is not finite for {target.ObservableId}");
            if (!double.IsFinite(prediction.Uncertainty.TotalUncertainty) || prediction.Uncertainty.TotalUncertainty < 0.0)
                closure.Add($"prediction uncertainty is not finite and non-negative for {target.ObservableId}");
            if (!double.IsFinite(target.TargetValue))
                closure.Add($"target value is not finite for {target.ObservableId}");
            if (!double.IsFinite(target.TargetUncertainty) || target.TargetUncertainty < 0.0)
                closure.Add($"target uncertainty is not finite and non-negative for {target.ObservableId}");

            if (closure.Count != 0)
                continue;

            var delta = prediction.Value - target.TargetValue;
            var combined = System.Math.Sqrt(
                prediction.Uncertainty.TotalUncertainty * prediction.Uncertainty.TotalUncertainty +
                target.TargetUncertainty * target.TargetUncertainty);
            var sigma = combined > 0.0 ? System.Math.Abs(delta) / combined : double.PositiveInfinity;
            comparisons.Add(new WzAbsoluteMassComparisonRecord
            {
                ObservableId = target.ObservableId,
                PredictedValue = prediction.Value,
                PredictedUncertainty = prediction.Uncertainty.TotalUncertainty,
                TargetValue = target.TargetValue,
                TargetUncertainty = target.TargetUncertainty,
                Delta = delta,
                CombinedUncertainty = combined,
                SigmaResidual = sigma,
                Passed = sigma <= sigmaThreshold,
            });
        }

        if (closure.Count == 0 && comparisons.Any(c => !c.Passed))
            closure.Add("one or more W/Z absolute mass predictions fail the sigma threshold");

        return new WzAbsoluteMassTargetComparisonResult
        {
            AlgorithmId = AlgorithmId,
            TerminalStatus = closure.Count == 0
                ? "wz-absolute-mass-target-comparison-passed"
                : "wz-absolute-mass-target-comparison-failed",
            SigmaThreshold = sigmaThreshold,
            Comparisons = comparisons,
            ClosureRequirements = closure,
        };
    }
}
