using System.Text.Json.Serialization;

namespace Gu.Core;

/// <summary>
/// Provenance metadata recording when and how a state or artifact was produced.
/// </summary>
public sealed class ProvenanceMeta
{
    /// <summary>Timestamp of creation (ISO 8601).</summary>
    [JsonPropertyName("createdAt")]
    public required DateTimeOffset CreatedAt { get; init; }

    /// <summary>Code revision (e.g., git SHA) that produced this artifact.</summary>
    [JsonPropertyName("codeRevision")]
    public required string CodeRevision { get; init; }

    /// <summary>Branch reference under which this was produced.</summary>
    [JsonPropertyName("branch")]
    public required BranchRef Branch { get; init; }

    /// <summary>Backend that produced the result (e.g., "cpu-reference", "cuda").</summary>
    [JsonPropertyName("backend")]
    public string? Backend { get; init; }

    /// <summary>Optional human-readable notes.</summary>
    [JsonPropertyName("notes")]
    public string? Notes { get; init; }
}
