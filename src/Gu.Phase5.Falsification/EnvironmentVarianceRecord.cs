using System.Text.Json.Serialization;
using Gu.Core;

namespace Gu.Phase5.Falsification;

/// <summary>
/// Records the variance of a computed quantity across environment tiers (M50).
/// Used to trigger EnvironmentInstability falsifiers when variance is excessive.
/// </summary>
public sealed class EnvironmentVarianceRecord : ISidecarEvidenceRecord
{
    /// <summary>Identifier for this variance record.</summary>
    [JsonPropertyName("recordId")]
    public required string RecordId { get; init; }

    /// <summary>Quantity ID whose variance is measured.</summary>
    [JsonPropertyName("quantityId")]
    public required string QuantityId { get; init; }

    /// <summary>Environment tier ID where the variance was measured.</summary>
    [JsonPropertyName("environmentTierId")]
    public required string EnvironmentTierId { get; init; }

    /// <summary>
    /// Relative standard deviation of the quantity across this environment tier
    /// (standard deviation / |mean|, or analogous normalized measure).
    /// Used by EnvironmentInstability trigger rule: RelativeStdDev &gt; threshold.
    /// </summary>
    [JsonPropertyName("relativeStdDev")]
    public required double RelativeStdDev { get; init; }

    /// <summary>Whether this variance was flagged as excessive by upstream analysis.</summary>
    [JsonPropertyName("flagged")]
    public bool Flagged { get; init; }

    /// <summary>Optional description or notes.</summary>
    [JsonPropertyName("notes")]
    public string? Notes { get; init; }

    /// <summary>Origin classification for this sidecar record.</summary>
    [JsonPropertyName("origin")]
    public string Origin { get; init; } = "bridge-derived";

    /// <summary>Artifact references used to support this record.</summary>
    [JsonPropertyName("sourceArtifactRefs")]
    public IReadOnlyList<string>? SourceArtifactRefs { get; init; }

    /// <summary>Provenance of the backend that produced this record.</summary>
    [JsonPropertyName("provenance")]
    public ProvenanceMeta? Provenance { get; init; }
}
