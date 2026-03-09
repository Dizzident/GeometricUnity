using Gu.Phase2.Predictions;

namespace Gu.Phase2.Comparison;

/// <summary>
/// Campaign-level adapter for loading and querying external comparison datasets (Section 13.2).
/// Phase II adapters operate at the campaign level, not the single-snapshot level.
/// Implementations may wrap in-memory data, JSON files, or databases.
/// </summary>
public interface IExternalDatasetAdapter
{
    /// <summary>Unique adapter identifier.</summary>
    string AdapterId { get; }

    /// <summary>Whether this adapter can provide data for the given asset ID.</summary>
    bool CanProvide(string assetId);

    /// <summary>Load a comparison asset with all Section 13.2 metadata.</summary>
    ComparisonAsset LoadAsset(string assetId);

    /// <summary>
    /// Get comparison data for the requested variables from a loaded asset.
    /// Returns a dictionary of variable name to data values.
    /// Variables that cannot be provided are omitted from the result.
    /// </summary>
    IReadOnlyDictionary<string, double[]> GetComparisonData(
        string assetId, IReadOnlyList<string> variableNames);
}
