using Gu.Phase2.Predictions;
using Gu.Phase2.Semantics;

namespace Gu.Phase2.Comparison;

/// <summary>
/// Semi-quantitative comparison (Section 13.3): scaling/surrogate matching.
/// Computes relative deviation across comparison variables.
/// Passes if average relative deviation is within tolerance.
/// </summary>
public sealed class SemiQuantitativeComparisonStrategy : IComparisonStrategy
{
    /// <summary>Default relative deviation tolerance for semi-quantitative comparison.</summary>
    public const double DefaultTolerance = 0.25;

    private readonly double _tolerance;

    public SemiQuantitativeComparisonStrategy(double tolerance = DefaultTolerance)
    {
        _tolerance = tolerance;
    }

    public ComparisonRunRecord Execute(
        PredictionTestRecord prediction,
        ComparisonAsset asset,
        IExternalDatasetAdapter adapter)
    {
        ArgumentNullException.ThrowIfNull(prediction);
        ArgumentNullException.ThrowIfNull(asset);
        ArgumentNullException.ThrowIfNull(adapter);

        var variableNames = asset.ComparisonVariables.Keys.ToList();
        var data = adapter.GetComparisonData(asset.AssetId, variableNames);

        var deviations = new List<double>();
        var missing = new List<string>();

        foreach (var variable in variableNames)
        {
            if (!data.TryGetValue(variable, out var values) || values.Length == 0)
            {
                missing.Add(variable);
                continue;
            }

            // Semi-quantitative: compute mean of available values as representative
            double mean = 0;
            for (int i = 0; i < values.Length; i++)
                mean += values[i];
            mean /= values.Length;

            // Relative deviation from 1.0 (expected normalized scaling)
            double deviation = System.Math.Abs(mean - 1.0);
            deviations.Add(deviation);
        }

        var propagated = UncertaintyDecomposer.Propagate(asset.UncertaintyModel, prediction);

        if (deviations.Count == 0)
        {
            return new ComparisonRunRecord
            {
                TestId = prediction.TestId,
                Mode = ComparisonMode.SemiQuantitative,
                Score = 0.0,
                Passed = false,
                Uncertainty = propagated,
                ResolvedClaimClass = prediction.ClaimClass,
                Summary = $"Semi-quantitative comparison failed: no data for any variable, missing: {string.Join(", ", missing)}",
            };
        }

        double avgDeviation = 0;
        for (int i = 0; i < deviations.Count; i++)
            avgDeviation += deviations[i];
        avgDeviation /= deviations.Count;

        bool passed = avgDeviation <= _tolerance && missing.Count == 0;
        double score = 1.0 - System.Math.Min(avgDeviation, 1.0);

        // Demote if not passed and claim is higher than PostdictionTarget
        var resolvedClass = prediction.ClaimClass;
        if (!passed && resolvedClass < ClaimClass.PostdictionTarget)
        {
            resolvedClass = ClaimClass.PostdictionTarget;
        }

        string summary = passed
            ? $"Semi-quantitative comparison passed: avg deviation {avgDeviation:F4} within tolerance {_tolerance:F4}"
            : $"Semi-quantitative comparison failed: avg deviation {avgDeviation:F4} exceeds tolerance {_tolerance:F4}";

        if (missing.Count > 0)
        {
            summary += $", missing variables: {string.Join(", ", missing)}";
        }

        return new ComparisonRunRecord
        {
            TestId = prediction.TestId,
            Mode = ComparisonMode.SemiQuantitative,
            Score = score,
            Passed = passed,
            Uncertainty = propagated,
            ResolvedClaimClass = resolvedClass,
            Summary = summary,
        };
    }
}
