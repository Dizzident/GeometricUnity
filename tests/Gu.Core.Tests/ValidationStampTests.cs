using System.Text.Json;
using Gu.Core;

namespace Gu.Core.Tests;

public class ValidationStampTests
{
    [Fact]
    public void RoundTrip_AllFieldsPreserved()
    {
        var original = new ValidationStamp
        {
            RuleId = "carrier-match-check",
            Passed = true,
            Timestamp = new DateTimeOffset(2026, 3, 8, 12, 0, 0, TimeSpan.Zero),
            Detail = "T_h and S_h carriers match: residual-2form"
        };

        var json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<ValidationStamp>(json);

        Assert.NotNull(deserialized);
        Assert.Equal(original.RuleId, deserialized.RuleId);
        Assert.Equal(original.Passed, deserialized.Passed);
        Assert.Equal(original.Timestamp, deserialized.Timestamp);
        Assert.Equal(original.Detail, deserialized.Detail);
    }

    [Fact]
    public void FailedValidation_PreservesDetail()
    {
        var stamp = new ValidationStamp
        {
            RuleId = "observation-bypass-check",
            Passed = false,
            Timestamp = DateTimeOffset.UtcNow,
            Detail = "Output 'raw_curvature' bypassed sigma_h* pipeline"
        };

        Assert.False(stamp.Passed);
        Assert.Contains("bypassed", stamp.Detail);
    }

    [Fact]
    public void Detail_IsOptional()
    {
        var stamp = new ValidationStamp
        {
            RuleId = "schema-version-check",
            Passed = true,
            Timestamp = DateTimeOffset.UtcNow
        };

        Assert.Null(stamp.Detail);
    }

    [Fact]
    public void JsonPropertyNames_UseCamelCase()
    {
        var stamp = new ValidationStamp
        {
            RuleId = "test",
            Passed = true,
            Timestamp = DateTimeOffset.UtcNow
        };

        var json = JsonSerializer.Serialize(stamp);

        Assert.Contains("\"ruleId\"", json);
        Assert.Contains("\"passed\"", json);
        Assert.Contains("\"timestamp\"", json);
    }
}
