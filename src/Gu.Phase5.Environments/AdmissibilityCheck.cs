using System.Text.Json.Serialization;

namespace Gu.Phase5.Environments;

public sealed class AdmissibilityCheck
{
    [JsonPropertyName("checkId")]
    public required string CheckId { get; init; }

    [JsonPropertyName("description")]
    public required string Description { get; init; }

    [JsonPropertyName("passed")]
    public required bool Passed { get; init; }

    [JsonPropertyName("value")]
    public double? Value { get; init; }

    [JsonPropertyName("threshold")]
    public double? Threshold { get; init; }
}
