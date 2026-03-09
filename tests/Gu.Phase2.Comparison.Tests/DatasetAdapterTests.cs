using Gu.Phase2.Comparison;
using Gu.Phase2.Predictions;

namespace Gu.Phase2.Comparison.Tests;

public sealed class DatasetAdapterTests
{
    private static ComparisonAsset MakeAsset(string id = "asset-001") => new()
    {
        AssetId = id,
        SourceCitation = "Test citation",
        AcquisitionDate = DateTimeOffset.UtcNow,
        PreprocessingDescription = "None",
        AdmissibleUseStatement = "Any",
        DomainOfValidity = "All",
        UncertaintyModel = UncertaintyRecord.Unestimated(),
        ComparisonVariables = new Dictionary<string, string>
        {
            ["energy"] = "Total energy",
            ["momentum"] = "Total momentum",
        },
    };

    [Fact]
    public void AdapterId_DefaultIsInMemory()
    {
        var adapter = new InMemoryDatasetAdapter();
        Assert.Equal("in-memory", adapter.AdapterId);
    }

    [Fact]
    public void AdapterId_CustomValue()
    {
        var adapter = new InMemoryDatasetAdapter("custom-adapter");
        Assert.Equal("custom-adapter", adapter.AdapterId);
    }

    [Fact]
    public void CanProvide_Registered_ReturnsTrue()
    {
        var adapter = new InMemoryDatasetAdapter();
        adapter.Register(MakeAsset(), new Dictionary<string, double[]>());
        Assert.True(adapter.CanProvide("asset-001"));
    }

    [Fact]
    public void CanProvide_NotRegistered_ReturnsFalse()
    {
        var adapter = new InMemoryDatasetAdapter();
        Assert.False(adapter.CanProvide("nonexistent"));
    }

    [Fact]
    public void LoadAsset_Registered_ReturnsAsset()
    {
        var adapter = new InMemoryDatasetAdapter();
        var asset = MakeAsset();
        adapter.Register(asset, new Dictionary<string, double[]>());

        var loaded = adapter.LoadAsset("asset-001");
        Assert.NotNull(loaded);
        Assert.Equal("asset-001", loaded.AssetId);
    }

    [Fact]
    public void LoadAsset_NotRegistered_Throws()
    {
        var adapter = new InMemoryDatasetAdapter();
        Assert.Throws<KeyNotFoundException>(() => adapter.LoadAsset("nonexistent"));
    }

    [Fact]
    public void GetComparisonData_Registered_ReturnsData()
    {
        var adapter = new InMemoryDatasetAdapter();
        var asset = MakeAsset();
        var data = new Dictionary<string, double[]>
        {
            ["energy"] = [1.0, 1.01, 0.99],
            ["momentum"] = [0.0, 0.001, -0.001],
        };
        adapter.Register(asset, data);

        var result = adapter.GetComparisonData("asset-001", ["energy", "momentum"]);
        Assert.Equal(2, result.Count);
        Assert.Equal(3, result["energy"].Length);
        Assert.Equal(1.0, result["energy"][0]);
    }

    [Fact]
    public void GetComparisonData_UnknownVariable_OmittedFromResult()
    {
        var adapter = new InMemoryDatasetAdapter();
        adapter.Register(MakeAsset(), new Dictionary<string, double[]>());

        var result = adapter.GetComparisonData("asset-001", ["nonexistent"]);
        Assert.Empty(result);
    }

    [Fact]
    public void GetComparisonData_UnknownAsset_ReturnsEmpty()
    {
        var adapter = new InMemoryDatasetAdapter();
        var result = adapter.GetComparisonData("nonexistent", ["energy"]);
        Assert.Empty(result);
    }

    [Fact]
    public void Register_OverwritesPreviousAsset()
    {
        var adapter = new InMemoryDatasetAdapter();
        var asset1 = MakeAsset();
        adapter.Register(asset1, new Dictionary<string, double[]>
        {
            ["energy"] = [1.0],
        });

        var asset2 = new ComparisonAsset
        {
            AssetId = "asset-001",
            SourceCitation = "Updated citation",
            AcquisitionDate = DateTimeOffset.UtcNow,
            PreprocessingDescription = "Updated",
            AdmissibleUseStatement = "Any",
            DomainOfValidity = "All",
            UncertaintyModel = UncertaintyRecord.Unestimated(),
            ComparisonVariables = new Dictionary<string, string> { ["energy"] = "Total energy" },
        };
        adapter.Register(asset2, new Dictionary<string, double[]>
        {
            ["energy"] = [2.0, 3.0],
        });

        var loaded = adapter.LoadAsset("asset-001");
        Assert.Equal("Updated citation", loaded.SourceCitation);

        var data = adapter.GetComparisonData("asset-001", ["energy"]);
        Assert.Equal(2, data["energy"].Length);
        Assert.Equal(2.0, data["energy"][0]);
    }

    [Fact]
    public void Register_NullAsset_Throws()
    {
        var adapter = new InMemoryDatasetAdapter();
        Assert.Throws<ArgumentNullException>(() =>
            adapter.Register(null!, new Dictionary<string, double[]>()));
    }

    [Fact]
    public void Register_NullData_Throws()
    {
        var adapter = new InMemoryDatasetAdapter();
        Assert.Throws<ArgumentNullException>(() =>
            adapter.Register(MakeAsset(), null!));
    }

    [Fact]
    public void MultipleAssets_IndependentData()
    {
        var adapter = new InMemoryDatasetAdapter();
        adapter.Register(MakeAsset("a1"), new Dictionary<string, double[]>
        {
            ["energy"] = [1.0],
        });
        adapter.Register(MakeAsset("a2"), new Dictionary<string, double[]>
        {
            ["energy"] = [2.0],
        });

        var d1 = adapter.GetComparisonData("a1", ["energy"]);
        var d2 = adapter.GetComparisonData("a2", ["energy"]);
        Assert.Equal(1.0, d1["energy"][0]);
        Assert.Equal(2.0, d2["energy"][0]);
    }

    [Fact]
    public void GetComparisonData_PartialVariables_ReturnsOnlyAvailable()
    {
        var adapter = new InMemoryDatasetAdapter();
        adapter.Register(MakeAsset(), new Dictionary<string, double[]>
        {
            ["energy"] = [1.0],
        });

        var result = adapter.GetComparisonData("asset-001", ["energy", "momentum"]);
        Assert.Single(result);
        Assert.True(result.ContainsKey("energy"));
        Assert.False(result.ContainsKey("momentum"));
    }
}
