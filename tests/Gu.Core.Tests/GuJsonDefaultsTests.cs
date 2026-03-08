using Gu.Core;
using Gu.Core.Serialization;

namespace Gu.Core.Tests;

public class GuJsonDefaultsTests
{
    [Fact]
    public void Options_WritesIndentedJson()
    {
        var space = new SpaceRef { SpaceId = "test", Dimension = 4 };
        var json = GuJsonDefaults.Serialize(space);
        Assert.Contains("\n", json);
    }

    [Fact]
    public void Options_OmitsNullValues()
    {
        var space = new SpaceRef { SpaceId = "test", Dimension = 4 };
        var json = GuJsonDefaults.Serialize(space);
        // Label is null but should not appear in output
        Assert.DoesNotContain("\"label\"", json);
    }

    [Fact]
    public void Options_UsesCamelCaseNaming()
    {
        var stamp = new ValidationStamp
        {
            RuleId = "test",
            Passed = true,
            Timestamp = DateTimeOffset.UtcNow
        };
        var json = GuJsonDefaults.Serialize(stamp);
        Assert.Contains("\"ruleId\"", json);
        Assert.Contains("\"passed\"", json);
    }

    [Fact]
    public void Options_AllowsTrailingCommas()
    {
        // GuJsonDefaults allows trailing commas for hand-edited JSON
        var json = """{"spaceId": "test", "dimension": 4,}""";
        var result = GuJsonDefaults.Deserialize<SpaceRef>(json);
        Assert.NotNull(result);
        Assert.Equal("test", result.SpaceId);
    }

    [Fact]
    public void Options_SkipsComments()
    {
        var json = """
        {
            // This is a comment
            "spaceId": "test",
            "dimension": 4
        }
        """;
        var result = GuJsonDefaults.Deserialize<SpaceRef>(json);
        Assert.NotNull(result);
        Assert.Equal("test", result.SpaceId);
    }
}
