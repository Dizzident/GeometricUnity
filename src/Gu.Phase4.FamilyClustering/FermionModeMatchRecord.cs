using System.Text.Json.Serialization;

namespace Gu.Phase4.FamilyClustering;

/// <summary>
/// Records a match between two fermionic modes from different spectral contexts
/// (different branch variants or refinement levels).
///
/// The aggregate score combines:
///   - eigenvalue band similarity (weight ~0.4)
///   - chirality profile similarity (weight ~0.3)
///   - eigenspace overlap if vectors available (weight ~0.3)
/// </summary>
public sealed class FermionModeMatchRecord
{
    /// <summary>Mode ID in the source spectral result.</summary>
    [JsonPropertyName("sourceModeId")]
    public required string SourceModeId { get; init; }

    /// <summary>Mode ID in the target spectral result.</summary>
    [JsonPropertyName("targetModeId")]
    public required string TargetModeId { get; init; }

    /// <summary>Eigenvalue similarity score [0, 1]. 1 = identical eigenvalue.</summary>
    [JsonPropertyName("eigenvalueSimilarity")]
    public required double EigenvalueSimilarity { get; init; }

    /// <summary>
    /// Chirality profile similarity [0, 1].
    /// Based on |netChirality_src - netChirality_tgt| mapped to [0,1].
    /// </summary>
    [JsonPropertyName("chiralitySimilarity")]
    public required double ChiralitySimilarity { get; init; }

    /// <summary>
    /// Eigenspace overlap |&lt;phi_src, phi_tgt&gt;|^2 in [0, 1].
    /// Null if eigenvectors are not available for one or both modes.
    /// </summary>
    [JsonPropertyName("eigenspaceOverlap")]
    public double? EigenspaceOverlap { get; init; }

    /// <summary>
    /// Weighted aggregate match score [0, 1]. Higher = better match.
    /// Computed from eigenvalue, chirality, and eigenspace components.
    /// </summary>
    [JsonPropertyName("aggregateScore")]
    public required double AggregateScore { get; init; }

    /// <summary>Whether the aggregate score exceeds the matching threshold.</summary>
    [JsonPropertyName("isMatch")]
    public required bool IsMatch { get; init; }

    /// <summary>
    /// Whether the match is flagged as ambiguous (i.e., another candidate
    /// had a very similar aggregate score).
    /// </summary>
    [JsonPropertyName("isAmbiguous")]
    public bool IsAmbiguous { get; init; }

    /// <summary>Ambiguity notes if IsAmbiguous is true.</summary>
    [JsonPropertyName("ambiguityNotes")]
    public List<string> AmbiguityNotes { get; init; } = new();
}
