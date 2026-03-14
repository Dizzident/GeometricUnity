using System.Text.Json.Serialization;

namespace Gu.Phase5.Environments;

public sealed class EnvironmentAdmissibilityReport
{
    [JsonPropertyName("level")]
    public required string Level { get; init; }

    [JsonPropertyName("checks")]
    public required IReadOnlyList<AdmissibilityCheck> Checks { get; init; }

    [JsonPropertyName("passed")]
    public required bool Passed { get; init; }

    [JsonPropertyName("notes")]
    public string? Notes { get; init; }
}
