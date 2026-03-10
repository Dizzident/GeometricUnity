using System.Text.Json.Serialization;

namespace Gu.Phase3.Properties;

/// <summary>
/// Branch / refinement / backend stability scores for a mode family.
/// (See IMPLEMENTATION_PLAN_P3.md Sections 4.10.6, 4.10.7, 4.10.8)
/// </summary>
public sealed class StabilityScoreCard
{
    /// <summary>Family ID or mode ID this score card applies to.</summary>
    [JsonPropertyName("entityId")]
    public required string EntityId { get; init; }

    /// <summary>
    /// Branch stability score: persistence under branch variant changes.
    /// 1.0 = perfectly stable, 0.0 = vanishes in some variants.
    /// </summary>
    [JsonPropertyName("branchStability")]
    public required double BranchStability { get; init; }

    /// <summary>
    /// Refinement stability score: persistence under mesh/discretization refinement.
    /// 1.0 = eigenvalue converges, 0.0 = unstable under refinement.
    /// </summary>
    [JsonPropertyName("refinementStability")]
    public required double RefinementStability { get; init; }

    /// <summary>
    /// Backend stability score: agreement between CPU and CUDA pipelines.
    /// 1.0 = identical results, 0.0 = significant discrepancy.
    /// </summary>
    [JsonPropertyName("backendStability")]
    public required double BackendStability { get; init; }

    /// <summary>Number of branch variants compared.</summary>
    [JsonPropertyName("branchVariantCount")]
    public required int BranchVariantCount { get; init; }

    /// <summary>Number of refinement levels compared.</summary>
    [JsonPropertyName("refinementLevelCount")]
    public required int RefinementLevelCount { get; init; }

    /// <summary>Number of backends compared.</summary>
    [JsonPropertyName("backendCount")]
    public required int BackendCount { get; init; }

    /// <summary>Maximum eigenvalue drift across branch variants.</summary>
    [JsonPropertyName("maxEigenvalueDrift")]
    public double? MaxEigenvalueDrift { get; init; }

    /// <summary>Maximum overlap loss across branch variants.</summary>
    [JsonPropertyName("maxOverlapLoss")]
    public double? MaxOverlapLoss { get; init; }
}
