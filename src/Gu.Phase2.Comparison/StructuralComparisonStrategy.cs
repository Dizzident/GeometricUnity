using Gu.Phase2.Predictions;
using Gu.Phase2.Semantics;

namespace Gu.Phase2.Comparison;

/// <summary>
/// Structural comparison (Section 13.3): pattern/topology/qualitative structure matching.
/// Returns pass/fail based on whether expected comparison variables exist in the dataset.
/// Score is 1.0 for pass, 0.0 for fail.
/// </summary>
public sealed class StructuralComparisonStrategy : IComparisonStrategy
{
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

        int matched = 0;
        var missing = new List<string>();

        foreach (var variable in variableNames)
        {
            if (data.TryGetValue(variable, out var values) && values.Length > 0)
            {
                matched++;
            }
            else
            {
                missing.Add(variable);
            }
        }

        int total = variableNames.Count;
        bool passed = total > 0 && matched == total;
        double score = total > 0 ? (double)matched / total : 0.0;

        string summary = passed
            ? $"Structural comparison passed: {matched}/{total} variables matched"
            : $"Structural comparison failed: {matched}/{total} variables matched, missing: {string.Join(", ", missing)}";

        return new ComparisonRunRecord
        {
            TestId = prediction.TestId,
            Mode = ComparisonMode.Structural,
            Score = score,
            Passed = passed,
            Uncertainty = asset.UncertaintyModel,
            ResolvedClaimClass = prediction.ClaimClass,
            Summary = summary,
        };
    }
}
