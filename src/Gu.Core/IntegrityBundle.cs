using System.Text.Json.Serialization;

namespace Gu.Core;

/// <summary>
/// Integrity information for an artifact bundle, including checksums.
/// </summary>
public sealed class IntegrityBundle
{
    /// <summary>SHA-256 hash of the serialized artifact content.</summary>
    [JsonPropertyName("contentHash")]
    public required string ContentHash { get; init; }

    /// <summary>Hash algorithm used (e.g., "SHA-256").</summary>
    [JsonPropertyName("hashAlgorithm")]
    public string HashAlgorithm { get; init; } = "SHA-256";

    /// <summary>Timestamp when the integrity check was computed.</summary>
    [JsonPropertyName("computedAt")]
    public required DateTimeOffset ComputedAt { get; init; }

    /// <summary>
    /// Per-file SHA-256 hashes mapping relative file paths to hex strings.
    /// ContentHash becomes the root hash (hash of all file hashes).
    /// </summary>
    [JsonPropertyName("fileHashes")]
    public IReadOnlyDictionary<string, string>? FileHashes { get; init; }
}
