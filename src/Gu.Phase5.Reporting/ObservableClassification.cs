using System.Text.Json.Serialization;

namespace Gu.Phase5.Reporting;

/// <summary>
/// Explicit classification for computed observables. This prevents benchmark
/// quantities from being silently reused as physical particle observables.
/// </summary>
public sealed class ObservableClassification
{
    [JsonPropertyName("observableId")]
    public required string ObservableId { get; init; }

    [JsonPropertyName("classification")]
    public required string Classification { get; init; }

    [JsonPropertyName("rationale")]
    public required string Rationale { get; init; }

    [JsonPropertyName("physicalClaimAllowed")]
    public required bool PhysicalClaimAllowed { get; init; }
}

public sealed class ObservableClassificationTable
{
    [JsonPropertyName("tableId")]
    public required string TableId { get; init; }

    [JsonPropertyName("classifications")]
    public required IReadOnlyList<ObservableClassification> Classifications { get; init; }
}
