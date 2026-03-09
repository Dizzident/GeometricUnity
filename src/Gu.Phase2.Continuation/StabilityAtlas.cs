using System.Text.Json.Serialization;
using Gu.Phase2.Stability;

namespace Gu.Phase2.Continuation;

/// <summary>
/// A stability atlas: collection of continuation paths, Hessian records,
/// and principal-symbol samples for a parameter family.
/// Per IMPLEMENTATION_PLAN_P2.md Section 14.3.
///
/// Must include:
/// - background family definition
/// - linearization records
/// - Hessian records
/// - spectrum probes
/// - continuation path data
/// - singularity/bifurcation indicators
/// - discretization sensitivity notes
/// - theorem-status notes
/// </summary>
public sealed class StabilityAtlas
{
    /// <summary>Unique atlas identifier.</summary>
    [JsonPropertyName("atlasId")]
    public required string AtlasId { get; init; }

    /// <summary>Branch manifest ID for this atlas.</summary>
    [JsonPropertyName("branchManifestId")]
    public required string BranchManifestId { get; init; }

    /// <summary>Description of the parameter family studied.</summary>
    [JsonPropertyName("familyDescription")]
    public required string FamilyDescription { get; init; }

    /// <summary>Continuation paths in this atlas.</summary>
    [JsonPropertyName("paths")]
    public required IReadOnlyList<ContinuationResult> Paths { get; init; }

    /// <summary>Hessian records at sampled background states.</summary>
    [JsonPropertyName("hessianRecords")]
    public required IReadOnlyList<HessianRecord> HessianRecords { get; init; }

    /// <summary>Principal-symbol samples from PDE studies.</summary>
    [JsonPropertyName("symbolSamples")]
    public required IReadOnlyList<PrincipalSymbolRecord> SymbolSamples { get; init; }

    /// <summary>Linearization records at sampled background states.</summary>
    [JsonPropertyName("linearizationRecords")]
    public required IReadOnlyList<LinearizationRecord> LinearizationRecords { get; init; }

    /// <summary>Detected singularity/bifurcation indicators across all paths.</summary>
    [JsonPropertyName("bifurcationIndicators")]
    public required IReadOnlyList<ContinuationEvent> BifurcationIndicators { get; init; }

    /// <summary>Notes on discretization sensitivity.</summary>
    [JsonPropertyName("discretizationNotes")]
    public string? DiscretizationNotes { get; init; }

    /// <summary>Theorem status notes (what has/hasn't been proven).</summary>
    [JsonPropertyName("theoremStatusNotes")]
    public string? TheoremStatusNotes { get; init; }

    /// <summary>Timestamp when this atlas was generated.</summary>
    [JsonPropertyName("timestamp")]
    public required DateTimeOffset Timestamp { get; init; }
}
