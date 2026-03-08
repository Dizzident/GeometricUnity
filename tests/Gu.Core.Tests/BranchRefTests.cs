using System.Text.Json;
using Gu.Core;

namespace Gu.Core.Tests;

public class BranchRefTests
{
    [Fact]
    public void RoundTrip_SerializesAndDeserializesCorrectly()
    {
        var original = new BranchRef
        {
            BranchId = "minimal-gu-v1",
            SchemaVersion = "1.0.0"
        };

        var json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<BranchRef>(json);

        Assert.NotNull(deserialized);
        Assert.Equal(original.BranchId, deserialized.BranchId);
        Assert.Equal(original.SchemaVersion, deserialized.SchemaVersion);
    }

    [Fact]
    public void JsonPropertyNames_AreCorrect()
    {
        var branchRef = new BranchRef
        {
            BranchId = "test-branch",
            SchemaVersion = "0.1.0"
        };

        var json = JsonSerializer.Serialize(branchRef);

        Assert.Contains("\"branchId\"", json);
        Assert.Contains("\"schemaVersion\"", json);
        // Ensure PascalCase is NOT used in JSON output
        Assert.DoesNotContain("\"BranchId\"", json);
        Assert.DoesNotContain("\"SchemaVersion\"", json);
    }

    [Fact]
    public void Deserialize_FromExplicitJson()
    {
        var json = """{"branchId":"from-json","schemaVersion":"2.0.0"}""";
        var result = JsonSerializer.Deserialize<BranchRef>(json);

        Assert.NotNull(result);
        Assert.Equal("from-json", result.BranchId);
        Assert.Equal("2.0.0", result.SchemaVersion);
    }
}
