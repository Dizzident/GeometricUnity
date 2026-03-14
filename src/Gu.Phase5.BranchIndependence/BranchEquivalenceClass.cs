using System.Text.Json.Serialization;

namespace Gu.Phase5.BranchIndependence;

/// <summary>
/// A group of branch variants that are equivalent within declared tolerances
/// for a single target quantity (M46).
///
/// Two branches are equivalent if |Q(branch_i) - Q(branch_j)| &lt;= max(AbsoluteTolerance,
/// RelativeTolerance * |Q(reference)|).
/// </summary>
public sealed class BranchEquivalenceClass
{
    /// <summary>Schema version.</summary>
    [JsonPropertyName("schemaVersion")]
    public string SchemaVersion { get; init; } = "1.0.0";

    /// <summary>Target quantity ID for which this class was computed.</summary>
    [JsonPropertyName("targetQuantityId")]
    public required string TargetQuantityId { get; init; }

    /// <summary>Branch variant IDs that belong to this equivalence class.</summary>
    [JsonPropertyName("memberBranchVariantIds")]
    public required List<string> MemberBranchVariantIds { get; init; }

    /// <summary>Number of members in this class.</summary>
    [JsonIgnore]
    public int Size => MemberBranchVariantIds.Count;

    /// <summary>
    /// Mean quantity value across members.
    /// </summary>
    [JsonPropertyName("meanValue")]
    public double MeanValue { get; init; }

    /// <summary>
    /// Maximum absolute deviation from the mean within the class.
    /// </summary>
    [JsonPropertyName("maxDeviationFromMean")]
    public double MaxDeviationFromMean { get; init; }

    /// <summary>
    /// Absolute tolerance used when forming this class.
    /// </summary>
    [JsonPropertyName("absoluteTolerance")]
    public double AbsoluteTolerance { get; init; }

    /// <summary>
    /// Relative tolerance used when forming this class.
    /// </summary>
    [JsonPropertyName("relativeTolerance")]
    public double RelativeTolerance { get; init; }
}
