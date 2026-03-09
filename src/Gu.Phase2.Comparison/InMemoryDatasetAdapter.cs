using Gu.Phase2.Predictions;

namespace Gu.Phase2.Comparison;

/// <summary>
/// In-memory dataset adapter for testing and synthetic campaigns.
/// Assets and their data values are registered explicitly.
/// </summary>
public sealed class InMemoryDatasetAdapter : IExternalDatasetAdapter
{
    private readonly Dictionary<string, ComparisonAsset> _assets = new();
    private readonly Dictionary<string, Dictionary<string, double[]>> _data = new();

    public string AdapterId { get; }

    public InMemoryDatasetAdapter(string adapterId = "in-memory")
    {
        AdapterId = adapterId;
    }

    /// <summary>Register an asset with its comparison data.</summary>
    public void Register(ComparisonAsset asset, IReadOnlyDictionary<string, double[]> variableData)
    {
        ArgumentNullException.ThrowIfNull(asset);
        ArgumentNullException.ThrowIfNull(variableData);

        _assets[asset.AssetId] = asset;
        _data[asset.AssetId] = new Dictionary<string, double[]>(variableData);
    }

    public bool CanProvide(string assetId)
    {
        return _assets.ContainsKey(assetId);
    }

    public ComparisonAsset LoadAsset(string assetId)
    {
        return _assets.TryGetValue(assetId, out var asset)
            ? asset
            : throw new KeyNotFoundException($"Asset '{assetId}' not registered in adapter '{AdapterId}'");
    }

    public IReadOnlyDictionary<string, double[]> GetComparisonData(
        string assetId, IReadOnlyList<string> variableNames)
    {
        var result = new Dictionary<string, double[]>();

        if (!_data.TryGetValue(assetId, out var variables))
            return result;

        foreach (var name in variableNames)
        {
            if (variables.TryGetValue(name, out var values))
            {
                result[name] = values;
            }
        }

        return result;
    }
}
