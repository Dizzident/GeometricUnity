using Gu.Phase2.Predictions;

namespace Gu.Phase2.Comparison;

/// <summary>
/// Strategy interface for comparison modes (Section 13.3).
/// Each mode (Structural, SemiQuantitative, Quantitative) has its own implementation
/// with different requirements and scoring semantics.
/// Returns a ComparisonRunRecord directly.
/// </summary>
public interface IComparisonStrategy
{
    /// <summary>
    /// Execute a comparison between a prediction and external data.
    /// </summary>
    /// <param name="prediction">The validated prediction test record.</param>
    /// <param name="asset">The external comparison asset.</param>
    /// <param name="adapter">Dataset adapter for querying actual values.</param>
    ComparisonRunRecord Execute(
        PredictionTestRecord prediction,
        ComparisonAsset asset,
        IExternalDatasetAdapter adapter);
}
