using System.Text.Json.Serialization;

namespace Gu.Phase5.BranchIndependence;

/// <summary>
/// Fragility assessment for a single target quantity across the branch family (M46).
///
/// A quantity is fragile if it varies significantly across admissible branch variants.
/// Formula: FragilityScore = maxDistanceToNeighbor / (meanDistanceToFamily + epsilon)
/// Score &gt; 0.5 → fragile; Score &lt;= 0.5 → invariant/robust.
/// </summary>
public sealed class FragilityRecord
{
    /// <summary>Schema version.</summary>
    [JsonPropertyName("schemaVersion")]
    public string SchemaVersion { get; init; } = "1.0.0";

    /// <summary>Target quantity ID.</summary>
    [JsonPropertyName("targetQuantityId")]
    public required string TargetQuantityId { get; init; }

    /// <summary>
    /// Fragility score = maxDistanceToNeighbor / (meanDistanceToFamily + epsilon).
    /// Score &gt; 0.5 → fragile.
    /// Score &lt;= 0.5 → invariant or robust.
    /// </summary>
    [JsonPropertyName("fragilityScore")]
    public double FragilityScore { get; init; }

    /// <summary>
    /// Maximum pairwise distance across the branch family for this quantity (numerator).
    /// </summary>
    [JsonPropertyName("maxDistanceToNeighbor")]
    public double MaxDistanceToNeighbor { get; init; }

    /// <summary>
    /// Mean of all pairwise distances across the branch family (denominator before epsilon).
    /// </summary>
    [JsonPropertyName("meanDistanceToFamily")]
    public double MeanDistanceToFamily { get; init; }

    /// <summary>
    /// Classification:
    /// "invariant"   — score &lt;= absoluteTolerance / referenceScale,
    /// "robust"      — low fragility but above strict invariance threshold,
    /// "fragile"     — high fragility relative to tolerance,
    /// "indeterminate" — only one variant evaluated.
    /// </summary>
    [JsonPropertyName("classification")]
    public required string Classification { get; init; }

    /// <summary>
    /// Branch variant ID pair with the maximum pairwise distance.
    /// </summary>
    [JsonPropertyName("maxDistancePair")]
    public required string[] MaxDistancePair { get; init; }

    /// <summary>
    /// Number of branch variants evaluated.
    /// </summary>
    [JsonPropertyName("variantCount")]
    public int VariantCount { get; init; }
}
