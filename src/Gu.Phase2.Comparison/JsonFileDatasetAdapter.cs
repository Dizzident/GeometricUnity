using System.Text.Json;
using Gu.Phase2.Predictions;

namespace Gu.Phase2.Comparison;

/// <summary>
/// Dataset adapter that loads comparison assets and data from JSON files on disk.
/// Each asset is a JSON file containing the ComparisonAsset metadata and variable data.
/// File naming convention: {assetId}.json in the configured directory.
/// </summary>
public sealed class JsonFileDatasetAdapter : IExternalDatasetAdapter
{
    private readonly string _directory;
    private readonly Dictionary<string, (ComparisonAsset Asset, Dictionary<string, double[]> Data)> _cache = new();

    public string AdapterId { get; }

    /// <summary>
    /// Create a JSON file dataset adapter.
    /// </summary>
    /// <param name="directory">Directory containing asset JSON files.</param>
    /// <param name="adapterId">Unique adapter identifier.</param>
    public JsonFileDatasetAdapter(string directory, string adapterId = "json-file")
    {
        ArgumentNullException.ThrowIfNull(directory);
        _directory = directory;
        AdapterId = adapterId;
    }

    public bool CanProvide(string assetId)
    {
        if (_cache.ContainsKey(assetId))
            return true;

        var path = GetFilePath(assetId);
        return File.Exists(path);
    }

    public ComparisonAsset LoadAsset(string assetId)
    {
        EnsureLoaded(assetId);
        return _cache[assetId].Asset;
    }

    public IReadOnlyDictionary<string, double[]> GetComparisonData(
        string assetId, IReadOnlyList<string> variableNames)
    {
        EnsureLoaded(assetId);
        var allData = _cache[assetId].Data;
        var result = new Dictionary<string, double[]>();

        foreach (var name in variableNames)
        {
            if (allData.TryGetValue(name, out var values))
            {
                result[name] = values;
            }
        }

        return result;
    }

    private string GetFilePath(string assetId) =>
        Path.Combine(_directory, $"{assetId}.json");

    private void EnsureLoaded(string assetId)
    {
        if (_cache.ContainsKey(assetId))
            return;

        var path = GetFilePath(assetId);
        if (!File.Exists(path))
            throw new KeyNotFoundException(
                $"Asset file not found: '{path}' in adapter '{AdapterId}'");

        var json = File.ReadAllText(path);
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var asset = JsonSerializer.Deserialize<ComparisonAsset>(
            root.GetProperty("asset").GetRawText())
            ?? throw new InvalidOperationException(
                $"Failed to deserialize ComparisonAsset from '{path}'");

        var data = new Dictionary<string, double[]>();
        if (root.TryGetProperty("data", out var dataElement))
        {
            foreach (var prop in dataElement.EnumerateObject())
            {
                var values = JsonSerializer.Deserialize<double[]>(prop.Value.GetRawText());
                if (values != null)
                {
                    data[prop.Name] = values;
                }
            }
        }

        _cache[assetId] = (asset, data);
    }
}
