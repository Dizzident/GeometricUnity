using System.Text.Json.Serialization;
using Gu.Phase2.Semantics;

namespace Gu.Phase2.Predictions;

/// <summary>
/// Every comparison-facing record must be typed (Section 5.7).
/// PredictionTestRecord carries the full provenance chain from formal source
/// to falsifier, ensuring no untyped claims reach comparison campaigns.
/// </summary>
public sealed class PredictionTestRecord
{
    /// <summary>Unique test identifier.</summary>
    [JsonPropertyName("testId")]
    public required string TestId { get; init; }

    /// <summary>Claimed class for this prediction.</summary>
    [JsonPropertyName("claimClass")]
    public required ClaimClass ClaimClass { get; init; }

    /// <summary>Formal mathematical source for the prediction.</summary>
    [JsonPropertyName("formalSource")]
    public required string FormalSource { get; init; }

    /// <summary>Branch manifest ID under which this prediction was produced.</summary>
    [JsonPropertyName("branchManifestId")]
    public required string BranchManifestId { get; init; }

    /// <summary>Observable map ID linking to the recovery graph.</summary>
    [JsonPropertyName("observableMapId")]
    public required string ObservableMapId { get; init; }

    /// <summary>Theorem dependency status: "closed", "open", "partial".</summary>
    [JsonPropertyName("theoremDependencyStatus")]
    public required string TheoremDependencyStatus { get; init; }

    /// <summary>Numerical dependency status: "converged", "exploratory", "failed".</summary>
    [JsonPropertyName("numericalDependencyStatus")]
    public required string NumericalDependencyStatus { get; init; }

    /// <summary>Approximation status: "exact", "leading-order", "surrogate".</summary>
    [JsonPropertyName("approximationStatus")]
    public required string ApproximationStatus { get; init; }

    /// <summary>External comparison asset, if applicable.</summary>
    [JsonPropertyName("externalComparisonAsset")]
    public ComparisonAsset? ExternalComparisonAsset { get; init; }

    /// <summary>What would disprove this prediction. Must not be empty.</summary>
    [JsonPropertyName("falsifier")]
    public required string Falsifier { get; init; }

    /// <summary>Links to artifact bundles that produced this prediction.</summary>
    [JsonPropertyName("artifactLinks")]
    public required IReadOnlyList<string> ArtifactLinks { get; init; }

    /// <summary>Human-readable notes.</summary>
    [JsonPropertyName("notes")]
    public string? Notes { get; init; }
}
