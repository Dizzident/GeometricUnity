using System.Text.Json;
using Gu.Core;

namespace Gu.Core.Tests;

public class ProvenanceMetaTests
{
    private static ProvenanceMeta CreateSample() => new()
    {
        CreatedAt = new DateTimeOffset(2026, 3, 8, 12, 0, 0, TimeSpan.Zero),
        CodeRevision = "abc123def456",
        Branch = new BranchRef
        {
            BranchId = "minimal-gu-v1",
            SchemaVersion = "1.0.0"
        },
        Backend = "cpu-reference",
        Notes = "Initial test run"
    };

    [Fact]
    public void RoundTrip_AllFieldsPreserved()
    {
        var original = CreateSample();
        var json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<ProvenanceMeta>(json);

        Assert.NotNull(deserialized);
        Assert.Equal(original.CreatedAt, deserialized.CreatedAt);
        Assert.Equal(original.CodeRevision, deserialized.CodeRevision);
        Assert.NotNull(deserialized.Branch);
        Assert.Equal(original.Branch.BranchId, deserialized.Branch.BranchId);
        Assert.Equal(original.Branch.SchemaVersion, deserialized.Branch.SchemaVersion);
        Assert.Equal(original.Backend, deserialized.Backend);
        Assert.Equal(original.Notes, deserialized.Notes);
    }

    [Fact]
    public void OptionalFields_CanBeNull()
    {
        var prov = new ProvenanceMeta
        {
            CreatedAt = DateTimeOffset.UtcNow,
            CodeRevision = "abc123",
            Branch = new BranchRef
            {
                BranchId = "test",
                SchemaVersion = "1.0.0"
            }
        };

        Assert.Null(prov.Backend);
        Assert.Null(prov.Notes);
    }

    [Fact]
    public void BranchRef_IsEmbeddedInJson()
    {
        var prov = CreateSample();
        var json = JsonSerializer.Serialize(prov);

        // Branch should be embedded as nested object
        Assert.Contains("\"branch\"", json);
        Assert.Contains("\"branchId\"", json);
        Assert.Contains("\"schemaVersion\"", json);
    }

    [Fact]
    public void JsonPropertyNames_UseCamelCase()
    {
        var prov = CreateSample();
        var json = JsonSerializer.Serialize(prov);

        Assert.Contains("\"createdAt\"", json);
        Assert.Contains("\"codeRevision\"", json);
        Assert.Contains("\"backend\"", json);
        Assert.Contains("\"notes\"", json);
    }
}
