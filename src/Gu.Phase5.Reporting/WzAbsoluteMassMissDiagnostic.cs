using System.Text.Json.Serialization;

namespace Gu.Phase5.Reporting;

public sealed class WzAbsoluteMassMissDiagnosticRecord
{
    [JsonPropertyName("observableId")]
    public required string ObservableId { get; init; }

    [JsonPropertyName("requiredScaleFactor")]
    public required double RequiredScaleFactor { get; init; }
}

public sealed class WzAbsoluteMassMissDiagnosticResult
{
    [JsonPropertyName("algorithmId")]
    public required string AlgorithmId { get; init; }

    [JsonPropertyName("terminalStatus")]
    public required string TerminalStatus { get; init; }

    [JsonPropertyName("requiredScaleFactors")]
    public required IReadOnlyList<WzAbsoluteMassMissDiagnosticRecord> RequiredScaleFactors { get; init; }

    [JsonPropertyName("meanRequiredScaleFactor")]
    public double? MeanRequiredScaleFactor { get; init; }

    [JsonPropertyName("relativeRequiredScaleSpread")]
    public double? RelativeRequiredScaleSpread { get; init; }

    [JsonPropertyName("currentWeakCoupling")]
    public double? CurrentWeakCoupling { get; init; }

    [JsonPropertyName("requiredWeakCoupling")]
    public double? RequiredWeakCoupling { get; init; }

    [JsonPropertyName("diagnosis")]
    public required IReadOnlyList<string> Diagnosis { get; init; }

    [JsonPropertyName("closureRequirements")]
    public required IReadOnlyList<string> ClosureRequirements { get; init; }
}

public static class WzAbsoluteMassMissDiagnostic
{
    public const string AlgorithmId = "phase75-wz-absolute-mass-miss-diagnostic-v1";

    public static WzAbsoluteMassMissDiagnosticResult Diagnose(
        WzAbsoluteMassTargetComparisonResult comparison,
        double currentWeakCoupling,
        double coherenceTolerance = 0.01)
    {
        ArgumentNullException.ThrowIfNull(comparison);

        var closure = new List<string>();
        if (!double.IsFinite(currentWeakCoupling) || currentWeakCoupling <= 0.0)
            closure.Add("current weak coupling must be finite and positive");
        if (!double.IsFinite(coherenceTolerance) || coherenceTolerance <= 0.0)
            closure.Add("coherence tolerance must be finite and positive");
        if (comparison.Comparisons.Count == 0)
            closure.Add("target comparison records are missing");

        var factors = new List<WzAbsoluteMassMissDiagnosticRecord>();
        foreach (var record in comparison.Comparisons)
        {
            if (!double.IsFinite(record.PredictedValue) || record.PredictedValue <= 0.0 ||
                !double.IsFinite(record.TargetValue) || record.TargetValue <= 0.0)
            {
                closure.Add($"comparison values must be finite and positive for {record.ObservableId}");
                continue;
            }

            factors.Add(new WzAbsoluteMassMissDiagnosticRecord
            {
                ObservableId = record.ObservableId,
                RequiredScaleFactor = record.TargetValue / record.PredictedValue,
            });
        }

        double? mean = null;
        double? spread = null;
        double? requiredWeakCoupling = null;
        var diagnosis = new List<string>();

        if (closure.Count == 0)
        {
            mean = factors.Average(f => f.RequiredScaleFactor);
            var min = factors.Min(f => f.RequiredScaleFactor);
            var max = factors.Max(f => f.RequiredScaleFactor);
            spread = (max - min) / mean.Value;
            requiredWeakCoupling = currentWeakCoupling * mean.Value;

            diagnosis.Add($"absolute W/Z predictions require a common scale increase of {mean.Value:R}");
            diagnosis.Add($"W/Z required-scale spread is {spread.Value:R}");
            if (spread.Value <= coherenceTolerance)
                diagnosis.Add("miss is coherent across W and Z; prioritize weak-coupling amplitude normalization before W/Z-specific mode changes");
            else
                diagnosis.Add("miss is not coherent across W and Z; investigate selected internal W/Z mode ratio or shared-scale relation");
        }

        if (closure.Count == 0)
            closure.Add("revise weak-coupling amplitude normalization or scalar-sector relation and re-run absolute W/Z projection");

        return new WzAbsoluteMassMissDiagnosticResult
        {
            AlgorithmId = AlgorithmId,
            TerminalStatus = "wz-absolute-mass-miss-diagnosed",
            RequiredScaleFactors = factors,
            MeanRequiredScaleFactor = mean,
            RelativeRequiredScaleSpread = spread,
            CurrentWeakCoupling = double.IsFinite(currentWeakCoupling) ? currentWeakCoupling : null,
            RequiredWeakCoupling = requiredWeakCoupling,
            Diagnosis = diagnosis,
            ClosureRequirements = closure,
        };
    }
}
