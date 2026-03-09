using System.Text.Json.Serialization;

namespace Gu.Phase2.Canonicity;

/// <summary>
/// Reports whether a branch variant is fragile (sensitive to small variant changes).
/// A fragile branch should not be silently dropped -- it is preserved as a counterexample or outlier.
/// </summary>
public sealed class FragilityReport
{
    /// <summary>Branch variant ID.</summary>
    [JsonPropertyName("branchId")]
    public required string BranchId { get; init; }

    /// <summary>Whether this branch is classified as fragile.</summary>
    [JsonPropertyName("isFragile")]
    public required bool IsFragile { get; init; }

    /// <summary>Reasons for fragility classification.</summary>
    [JsonPropertyName("fragilityReasons")]
    public required IReadOnlyList<string> FragilityReasons { get; init; }
}
