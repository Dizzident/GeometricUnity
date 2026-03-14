using System.Text.Json.Serialization;

namespace Gu.Phase5.Reporting;

/// <summary>
/// Summary atlas of branch-independence results from a Phase V campaign (M53).
/// </summary>
public sealed class BranchIndependenceAtlas
{
    /// <summary>Total number of target quantities analyzed.</summary>
    [JsonPropertyName("totalQuantities")]
    public required int TotalQuantities { get; init; }

    /// <summary>Number of quantities that are branch-invariant.</summary>
    [JsonPropertyName("invariantCount")]
    public required int InvariantCount { get; init; }

    /// <summary>Number of quantities that are branch-fragile.</summary>
    [JsonPropertyName("fragileCount")]
    public required int FragileCount { get; init; }

    /// <summary>Total number of equivalence classes identified.</summary>
    [JsonPropertyName("equivalenceClassCount")]
    public required int EquivalenceClassCount { get; init; }

    /// <summary>Human-readable summary lines for reporting.</summary>
    [JsonPropertyName("summaryLines")]
    public required IReadOnlyList<string> SummaryLines { get; init; }
}
