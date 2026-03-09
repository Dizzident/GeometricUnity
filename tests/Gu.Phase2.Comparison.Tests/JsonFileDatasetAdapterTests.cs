using System.Text.Json;
using Gu.Phase2.Comparison;
using Gu.Phase2.Predictions;

namespace Gu.Phase2.Comparison.Tests;

public sealed class JsonFileDatasetAdapterTests : IDisposable
{
    private readonly string _tempDir;

    public JsonFileDatasetAdapterTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"gu-json-adapter-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    private void WriteAssetFile(string assetId, ComparisonAsset asset, Dictionary<string, double[]> data)
    {
        var doc = new
        {
            asset,
            data,
        };
        var json = JsonSerializer.Serialize(doc, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(Path.Combine(_tempDir, $"{assetId}.json"), json);
    }

    private static ComparisonAsset MakeAsset(string id = "test-asset") => new()
    {
        AssetId = id,
        SourceCitation = "Test citation",
        AcquisitionDate = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
        PreprocessingDescription = "None",
        AdmissibleUseStatement = "Any",
        DomainOfValidity = "All",
        UncertaintyModel = UncertaintyRecord.Unestimated(),
        ComparisonVariables = new Dictionary<string, string>
        {
            ["energy"] = "Energy density",
        },
    };

    [Fact]
    public void AdapterId_DefaultIsJsonFile()
    {
        var adapter = new JsonFileDatasetAdapter(_tempDir);
        Assert.Equal("json-file", adapter.AdapterId);
    }

    [Fact]
    public void AdapterId_CustomValue()
    {
        var adapter = new JsonFileDatasetAdapter(_tempDir, "my-adapter");
        Assert.Equal("my-adapter", adapter.AdapterId);
    }

    [Fact]
    public void CanProvide_FileExists_ReturnsTrue()
    {
        WriteAssetFile("test-asset", MakeAsset(), new() { ["energy"] = [1.0] });
        var adapter = new JsonFileDatasetAdapter(_tempDir);
        Assert.True(adapter.CanProvide("test-asset"));
    }

    [Fact]
    public void CanProvide_FileDoesNotExist_ReturnsFalse()
    {
        var adapter = new JsonFileDatasetAdapter(_tempDir);
        Assert.False(adapter.CanProvide("nonexistent"));
    }

    [Fact]
    public void LoadAsset_ValidFile_ReturnsAsset()
    {
        var asset = MakeAsset();
        WriteAssetFile("test-asset", asset, new() { ["energy"] = [1.0, 2.0] });

        var adapter = new JsonFileDatasetAdapter(_tempDir);
        var loaded = adapter.LoadAsset("test-asset");

        Assert.Equal("test-asset", loaded.AssetId);
        Assert.Equal("Test citation", loaded.SourceCitation);
    }

    [Fact]
    public void LoadAsset_MissingFile_Throws()
    {
        var adapter = new JsonFileDatasetAdapter(_tempDir);
        Assert.Throws<KeyNotFoundException>(() => adapter.LoadAsset("nonexistent"));
    }

    [Fact]
    public void GetComparisonData_ReturnsRequestedVariables()
    {
        WriteAssetFile("test-asset", MakeAsset(), new()
        {
            ["energy"] = [1.0, 2.0, 3.0],
            ["momentum"] = [0.1, 0.2],
        });

        var adapter = new JsonFileDatasetAdapter(_tempDir);
        var data = adapter.GetComparisonData("test-asset", ["energy", "momentum"]);

        Assert.Equal(2, data.Count);
        Assert.Equal(3, data["energy"].Length);
        Assert.Equal(2, data["momentum"].Length);
    }

    [Fact]
    public void GetComparisonData_OmitsMissingVariables()
    {
        WriteAssetFile("test-asset", MakeAsset(), new()
        {
            ["energy"] = [1.0],
        });

        var adapter = new JsonFileDatasetAdapter(_tempDir);
        var data = adapter.GetComparisonData("test-asset", ["energy", "nonexistent"]);

        Assert.Single(data);
        Assert.True(data.ContainsKey("energy"));
    }

    [Fact]
    public void CachesAfterFirstLoad()
    {
        WriteAssetFile("test-asset", MakeAsset(), new() { ["energy"] = [1.0] });

        var adapter = new JsonFileDatasetAdapter(_tempDir);

        // First load
        var asset1 = adapter.LoadAsset("test-asset");
        // Delete the file
        File.Delete(Path.Combine(_tempDir, "test-asset.json"));
        // Second load should still work from cache
        var asset2 = adapter.LoadAsset("test-asset");

        Assert.Equal(asset1.AssetId, asset2.AssetId);
    }

    [Fact]
    public void Constructor_NullDirectory_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new JsonFileDatasetAdapter(null!));
    }
}
