using System.Text.Json.Serialization;

namespace Gu.Core;

/// <summary>
/// Contract guaranteeing that an artifact bundle can be replayed to reproduce results.
/// </summary>
public sealed class ReplayContract
{
    /// <summary>Branch manifest used for the run.</summary>
    [JsonPropertyName("branchManifest")]
    public required BranchManifest BranchManifest { get; init; }

    /// <summary>Whether the run was declared deterministic.</summary>
    [JsonPropertyName("deterministic")]
    public required bool Deterministic { get; init; }

    /// <summary>Random seed used, if applicable.</summary>
    [JsonPropertyName("randomSeed")]
    public long? RandomSeed { get; init; }

    /// <summary>Backend identifier used for the run.</summary>
    [JsonPropertyName("backendId")]
    public required string BackendId { get; init; }

    /// <summary>Non-determinism declaration.</summary>
    [JsonPropertyName("nonDeterminismDeclaration")]
    public string? NonDeterminismDeclaration { get; init; }

    /// <summary>
    /// Replay tier (FIX-M1-1: required, no default):
    /// R0 = archival only, R1 = structural replay,
    /// R2 = numerical replay, R3 = cross-backend replay.
    /// </summary>
    [JsonPropertyName("replayTier")]
    public required string ReplayTier { get; init; }
}
