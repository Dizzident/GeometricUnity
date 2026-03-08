using System.Text.Json.Serialization;

namespace Gu.Artifacts;

/// <summary>
/// A manifest of file-level SHA-256 hashes for verifying run folder integrity.
/// </summary>
public sealed class FileHashManifest
{
    /// <summary>Root path of the run folder (informational only).</summary>
    [JsonPropertyName("rootPath")]
    public required string RootPath { get; init; }

    /// <summary>Map of relative file path to SHA-256 hash.</summary>
    [JsonPropertyName("fileHashes")]
    public required IReadOnlyDictionary<string, string> FileHashes { get; init; }

    /// <summary>Timestamp when these hashes were computed.</summary>
    [JsonPropertyName("computedAt")]
    public required DateTimeOffset ComputedAt { get; init; }
}
