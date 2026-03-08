using System.Text.Json;
using Gu.Core;

namespace Gu.Core.Tests;

public class PatchInfoTests
{
    [Fact]
    public void RoundTrip_AllFieldsPreserved()
    {
        var original = new PatchInfo
        {
            PatchId = "patch-0",
            ElementCount = 256,
            TopologyType = "simplicial",
            Metadata = new Dictionary<string, string>
            {
                ["refinementLevel"] = "2",
                ["region"] = "interior"
            }
        };

        var json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<PatchInfo>(json);

        Assert.NotNull(deserialized);
        Assert.Equal(original.PatchId, deserialized.PatchId);
        Assert.Equal(original.ElementCount, deserialized.ElementCount);
        Assert.Equal(original.TopologyType, deserialized.TopologyType);
        Assert.NotNull(deserialized.Metadata);
        Assert.Equal("2", deserialized.Metadata["refinementLevel"]);
        Assert.Equal("interior", deserialized.Metadata["region"]);
    }

    [Fact]
    public void OptionalFields_CanBeNull()
    {
        var patch = new PatchInfo
        {
            PatchId = "patch-1",
            ElementCount = 64
        };

        Assert.Null(patch.TopologyType);
        Assert.Null(patch.Metadata);
    }

    [Fact]
    public void TopologyTypes_SupportSimplicialAndStructured()
    {
        // Per IX-4: simplicial or structured patch geometry
        var simplicial = new PatchInfo
        {
            PatchId = "simplex-patch",
            ElementCount = 100,
            TopologyType = "simplicial"
        };

        var structured = new PatchInfo
        {
            PatchId = "struct-patch",
            ElementCount = 100,
            TopologyType = "structured"
        };

        Assert.Equal("simplicial", simplicial.TopologyType);
        Assert.Equal("structured", structured.TopologyType);
    }

    [Fact]
    public void JsonPropertyNames_UseCamelCase()
    {
        var patch = new PatchInfo
        {
            PatchId = "test",
            ElementCount = 1,
            TopologyType = "simplicial"
        };

        var json = JsonSerializer.Serialize(patch);

        Assert.Contains("\"patchId\"", json);
        Assert.Contains("\"elementCount\"", json);
        Assert.Contains("\"topologyType\"", json);
    }

    [Fact]
    public void Metadata_EmptyDictionary_RoundTrips()
    {
        var patch = new PatchInfo
        {
            PatchId = "empty-meta",
            ElementCount = 10,
            Metadata = new Dictionary<string, string>()
        };

        var json = JsonSerializer.Serialize(patch);
        var deserialized = JsonSerializer.Deserialize<PatchInfo>(json);

        Assert.NotNull(deserialized);
        Assert.NotNull(deserialized.Metadata);
        Assert.Empty(deserialized.Metadata);
    }
}
