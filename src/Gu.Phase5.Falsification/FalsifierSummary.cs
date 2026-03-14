using System.Text.Json;
using System.Text.Json.Serialization;
using Gu.Core;

namespace Gu.Phase5.Falsification;

/// <summary>
/// Summary of all falsifiers evaluated for a study (M50).
/// Aggregates FalsifierRecords with count statistics.
/// </summary>
public sealed class FalsifierSummary
{
    /// <summary>Study identifier.</summary>
    [JsonPropertyName("studyId")]
    public required string StudyId { get; init; }

    /// <summary>Schema version.</summary>
    [JsonPropertyName("schemaVersion")]
    public string SchemaVersion { get; init; } = "1.0.0";

    /// <summary>All evaluated falsifier records (active and inactive).</summary>
    [JsonPropertyName("falsifiers")]
    public required IReadOnlyList<FalsifierRecord> Falsifiers { get; init; }

    /// <summary>Count of active fatal falsifiers.</summary>
    [JsonPropertyName("activeFatalCount")]
    public required int ActiveFatalCount { get; init; }

    /// <summary>Count of active high-severity falsifiers.</summary>
    [JsonPropertyName("activeHighCount")]
    public required int ActiveHighCount { get; init; }

    /// <summary>Total count of active falsifiers (all severities).</summary>
    [JsonPropertyName("totalActiveCount")]
    public required int TotalActiveCount { get; init; }

    /// <summary>Provenance metadata.</summary>
    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }

    /// <summary>Serialize to JSON.</summary>
    public string ToJson(bool indented = true)
        => JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = indented });

    /// <summary>Deserialize from JSON.</summary>
    public static FalsifierSummary FromJson(string json)
        => JsonSerializer.Deserialize<FalsifierSummary>(json,
               new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
           ?? throw new InvalidOperationException("Failed to deserialize FalsifierSummary.");
}
