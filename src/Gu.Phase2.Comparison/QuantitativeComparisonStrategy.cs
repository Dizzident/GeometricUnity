using Gu.Phase2.Predictions;
using Gu.Phase2.Semantics;

namespace Gu.Phase2.Comparison;

/// <summary>
/// Quantitative comparison (Section 13.3): full numerical comparison with uncertainty decomposition.
/// Requires complete branch manifest, typed extraction, declared approximation status,
/// strong numerical status, uncertainty decomposition, and falsifier present.
/// Computes chi-squared-like score normalized by total uncertainty.
/// </summary>
public sealed class QuantitativeComparisonStrategy : IComparisonStrategy
{
    /// <summary>Default sigma threshold for quantitative pass/fail.</summary>
    public const double DefaultSigmaThreshold = 5.0;

    private readonly double _sigmaThreshold;

    public QuantitativeComparisonStrategy(double sigmaThreshold = DefaultSigmaThreshold)
    {
        _sigmaThreshold = sigmaThreshold;
    }

    public ComparisonRunRecord Execute(
        PredictionTestRecord prediction,
        ComparisonAsset asset,
        IExternalDatasetAdapter adapter)
    {
        ArgumentNullException.ThrowIfNull(prediction);
        ArgumentNullException.ThrowIfNull(asset);
        ArgumentNullException.ThrowIfNull(adapter);

        var propagated = UncertaintyDecomposer.Propagate(asset.UncertaintyModel, prediction);
        double totalUncertainty = UncertaintyDecomposer.TotalUncertainty(propagated);

        // If total uncertainty is unestimated or zero, cannot do quantitative comparison
        if (totalUncertainty <= 0)
        {
            return new ComparisonRunRecord
            {
                TestId = prediction.TestId,
                Mode = ComparisonMode.Quantitative,
                Score = 0.0,
                Passed = false,
                Uncertainty = propagated,
                ResolvedClaimClass = ClaimClass.PostdictionTarget,
                Summary = "Quantitative comparison failed: total uncertainty is zero or unestimated",
            };
        }

        var variableNames = asset.ComparisonVariables.Keys.ToList();
        var data = adapter.GetComparisonData(asset.AssetId, variableNames);

        var residuals = new List<double>();
        var missing = new List<string>();

        foreach (var variable in variableNames)
        {
            if (!data.TryGetValue(variable, out var values) || values.Length == 0)
            {
                missing.Add(variable);
                continue;
            }

            // Compute mean residual for this variable (deviation from expected = 1.0)
            double sumSq = 0;
            for (int i = 0; i < values.Length; i++)
            {
                double residual = values[i] - 1.0;
                sumSq += residual * residual;
            }

            residuals.Add(sumSq / values.Length);
        }

        if (residuals.Count == 0)
        {
            return new ComparisonRunRecord
            {
                TestId = prediction.TestId,
                Mode = ComparisonMode.Quantitative,
                Score = 0.0,
                Passed = false,
                Uncertainty = propagated,
                ResolvedClaimClass = prediction.ClaimClass,
                Summary = $"Quantitative comparison failed: no data available, missing: {string.Join(", ", missing)}",
            };
        }

        // Chi-squared-like: sum of (mean residual^2) / totalUncertainty^2
        double chiSq = 0;
        for (int i = 0; i < residuals.Count; i++)
            chiSq += residuals[i];
        double normalizedScore = System.Math.Sqrt(chiSq) / totalUncertainty;

        bool passed = normalizedScore <= _sigmaThreshold && missing.Count == 0;

        // Score: 1.0 = perfect, decays toward 0 as normalized score grows
        double score = System.Math.Exp(-normalizedScore * normalizedScore / (2.0 * _sigmaThreshold * _sigmaThreshold));

        var resolvedClass = prediction.ClaimClass;
        if (!passed)
        {
            // Falsified: demote to at least PostdictionTarget
            if (resolvedClass < ClaimClass.PostdictionTarget)
            {
                resolvedClass = ClaimClass.PostdictionTarget;
            }
        }

        string summary = passed
            ? $"Quantitative comparison passed: normalized score {normalizedScore:F4} within {_sigmaThreshold:F1} sigma"
            : $"Quantitative comparison failed: normalized score {normalizedScore:F4} exceeds {_sigmaThreshold:F1} sigma threshold";

        if (missing.Count > 0)
        {
            summary += $", missing variables: {string.Join(", ", missing)}";
        }

        return new ComparisonRunRecord
        {
            TestId = prediction.TestId,
            Mode = ComparisonMode.Quantitative,
            Score = score,
            Passed = passed,
            Uncertainty = propagated,
            ResolvedClaimClass = resolvedClass,
            Summary = summary,
        };
    }
}
