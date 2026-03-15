using System.Text.Json.Serialization;
using Gu.Core;

namespace Gu.Phase5.Falsification;

/// <summary>
/// Status of one evidence channel in a sidecar generation run (P6-M3/D-P6-002).
/// Status values:
///   "evaluated" — records were present and written,
///   "skipped"   — channel was requested but no inputs were supplied,
///   "absent"    — channel was not requested (inputs were null/empty and no file written).
/// </summary>
public sealed class SidecarChannelStatus
{
    /// <summary>Channel identifier: "observation-chain", "environment-variance",
    /// "representation-content", or "coupling-consistency".</summary>
    [JsonPropertyName("channelId")]
    public required string ChannelId { get; init; }

    /// <summary>Status: "evaluated", "skipped", or "absent".</summary>
    [JsonPropertyName("status")]
    public required string Status { get; init; }

    /// <summary>Number of input records supplied to this channel.</summary>
    [JsonPropertyName("inputCount")]
    public required int InputCount { get; init; }

    /// <summary>Number of records written to the output sidecar file.</summary>
    [JsonPropertyName("outputCount")]
    public required int OutputCount { get; init; }

    /// <summary>Breakdown of record origins for this channel.</summary>
    [JsonPropertyName("originCounts")]
    public IReadOnlyDictionary<string, int>? OriginCounts { get; init; }
}

/// <summary>
/// Summary of a sidecar generation run, reporting per-channel coverage (P6-M3/D-P6-002).
/// Written as sidecar_summary.json in the output directory.
/// </summary>
public sealed class SidecarSummary
{
    /// <summary>Study identifier.</summary>
    [JsonPropertyName("studyId")]
    public required string StudyId { get; init; }

    /// <summary>Schema version.</summary>
    [JsonPropertyName("schemaVersion")]
    public string SchemaVersion { get; init; } = "1.0.0";

    /// <summary>Per-channel status records.</summary>
    [JsonPropertyName("channels")]
    public required IReadOnlyList<SidecarChannelStatus> Channels { get; init; }

    /// <summary>Provenance metadata.</summary>
    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}
