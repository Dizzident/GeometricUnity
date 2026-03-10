using System.Text.Json.Serialization;
using Gu.Core;

namespace Gu.Phase3.Campaigns;

/// <summary>
/// Result of executing a boson comparison campaign.
/// </summary>
public sealed class CampaignResult
{
    /// <summary>Campaign identifier that produced this result.</summary>
    [JsonPropertyName("campaignId")]
    public required string CampaignId { get; init; }

    /// <summary>All individual comparison results.</summary>
    [JsonPropertyName("comparisons")]
    public required IReadOnlyList<ComparisonResult> Comparisons { get; init; }

    /// <summary>Number of candidates evaluated.</summary>
    [JsonPropertyName("candidatesEvaluated")]
    public required int CandidatesEvaluated { get; init; }

    /// <summary>Number of targets in the campaign.</summary>
    [JsonPropertyName("targetsCount")]
    public required int TargetsCount { get; init; }

    /// <summary>Count of Compatible outcomes.</summary>
    [JsonPropertyName("compatibleCount")]
    public required int CompatibleCount { get; init; }

    /// <summary>Count of Incompatible outcomes.</summary>
    [JsonPropertyName("incompatibleCount")]
    public required int IncompatibleCount { get; init; }

    /// <summary>Count of Underdetermined outcomes.</summary>
    [JsonPropertyName("underdeterminedCount")]
    public required int UnderdeterminedCount { get; init; }

    /// <summary>Count of InsufficientEvidence outcomes.</summary>
    [JsonPropertyName("insufficientEvidenceCount")]
    public required int InsufficientEvidenceCount { get; init; }

    /// <summary>Human-readable summary of results.</summary>
    [JsonPropertyName("summary")]
    public required string Summary { get; init; }

    /// <summary>Provenance metadata.</summary>
    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}
