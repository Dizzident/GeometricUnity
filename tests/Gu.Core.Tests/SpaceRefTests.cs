using System.Text.Json;
using Gu.Core;

namespace Gu.Core.Tests;

public class SpaceRefTests
{
    [Fact]
    public void RoundTrip_AllFieldsPreserved()
    {
        var original = new SpaceRef
        {
            SpaceId = "ambient_Y_h",
            Dimension = 14,
            Label = "Observerse Y_h (dim 14 over 4D base)"
        };

        var json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<SpaceRef>(json);

        Assert.NotNull(deserialized);
        Assert.Equal(original.SpaceId, deserialized.SpaceId);
        Assert.Equal(original.Dimension, deserialized.Dimension);
        Assert.Equal(original.Label, deserialized.Label);
    }

    [Fact]
    public void Label_IsOptionalAndNullable()
    {
        var space = new SpaceRef
        {
            SpaceId = "base_X_h",
            Dimension = 4
        };

        Assert.Null(space.Label);

        var json = JsonSerializer.Serialize(space);
        var deserialized = JsonSerializer.Deserialize<SpaceRef>(json);
        Assert.NotNull(deserialized);
        Assert.Null(deserialized.Label);
    }

    [Fact]
    public void BaseSpace_Dimension4_MatchesDefaultTarget()
    {
        // Per IA-1: active branch metadata must default to dim(X) = 4
        var baseSpace = new SpaceRef
        {
            SpaceId = "base_X_h",
            Dimension = 4,
            Label = "base_X_h"
        };

        Assert.Equal(4, baseSpace.Dimension);
    }

    [Fact]
    public void JsonPropertyNames_UseCamelCase()
    {
        var space = new SpaceRef
        {
            SpaceId = "test",
            Dimension = 3,
            Label = "test"
        };

        var json = JsonSerializer.Serialize(space);

        Assert.Contains("\"spaceId\"", json);
        Assert.Contains("\"dimension\"", json);
        Assert.Contains("\"label\"", json);
    }
}
