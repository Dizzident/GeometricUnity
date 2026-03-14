using System.Text.Json;
using System.Text.Json.Serialization;

namespace Gu.Phase5.QuantitativeValidation;

/// <summary>
/// Table of external targets for quantitative comparison (M49).
/// </summary>
public sealed class ExternalTargetTable
{
    /// <summary>Unique table identifier.</summary>
    [JsonPropertyName("tableId")]
    public required string TableId { get; init; }

    /// <summary>External targets.</summary>
    [JsonPropertyName("targets")]
    public required IReadOnlyList<ExternalTarget> Targets { get; init; }

    /// <summary>Deserialize from JSON.</summary>
    public static ExternalTargetTable FromJson(string json)
        => JsonSerializer.Deserialize<ExternalTargetTable>(json,
               new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
           ?? throw new InvalidOperationException("Failed to deserialize ExternalTargetTable.");

    /// <summary>Serialize to JSON.</summary>
    public string ToJson(bool indented = true)
        => JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = indented });
}
