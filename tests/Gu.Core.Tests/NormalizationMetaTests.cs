using System.Text.Json;
using Gu.Core;

namespace Gu.Core.Tests;

public class NormalizationMetaTests
{
    [Fact]
    public void RoundTrip_AllFieldsPreserved()
    {
        var original = new NormalizationMeta
        {
            SchemeId = "unit-trace-norm",
            ScaleFactor = 2.5,
            Description = "Trace-norm normalization with scale factor 2.5"
        };

        var json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<NormalizationMeta>(json);

        Assert.NotNull(deserialized);
        Assert.Equal(original.SchemeId, deserialized.SchemeId);
        Assert.Equal(original.ScaleFactor, deserialized.ScaleFactor);
        Assert.Equal(original.Description, deserialized.Description);
    }

    [Fact]
    public void ScaleFactor_DefaultsToOne()
    {
        var norm = new NormalizationMeta
        {
            SchemeId = "identity"
        };

        Assert.Equal(1.0, norm.ScaleFactor);
    }

    [Fact]
    public void Description_IsOptional()
    {
        var norm = new NormalizationMeta
        {
            SchemeId = "custom"
        };

        Assert.Null(norm.Description);
    }

    [Fact]
    public void JsonPropertyNames_UseCamelCase()
    {
        var norm = new NormalizationMeta
        {
            SchemeId = "test",
            ScaleFactor = 1.0,
            Description = "test"
        };

        var json = JsonSerializer.Serialize(norm);

        Assert.Contains("\"schemeId\"", json);
        Assert.Contains("\"scaleFactor\"", json);
        Assert.Contains("\"description\"", json);
    }
}
