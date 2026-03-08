using System.Text.Json.Serialization;
using Gu.Core;

namespace Gu.ExternalComparison;

/// <summary>
/// Self-contained audit record of a single comparison execution.
/// Embeds observation provenance proof so the comparison trail is complete.
/// Failed comparisons are preserved as first-class artifacts (Section 18.4).
/// </summary>
public sealed class ComparisonRecord
{
    /// <summary>Unique comparison execution identifier.</summary>
    [JsonPropertyName("comparisonId")]
    public required string ComparisonId { get; init; }

    /// <summary>Template that defined this comparison.</summary>
    [JsonPropertyName("templateId")]
    public required string TemplateId { get; init; }

    /// <summary>Observable ID that was compared.</summary>
    [JsonPropertyName("observableId")]
    public required string ObservableId { get; init; }

    /// <summary>Reference source identifier.</summary>
    [JsonPropertyName("referenceSourceId")]
    public required string ReferenceSourceId { get; init; }

    /// <summary>Reference version or revision.</summary>
    [JsonPropertyName("referenceVersion")]
    public required string ReferenceVersion { get; init; }

    /// <summary>Branch under which this comparison was run.</summary>
    [JsonPropertyName("branch")]
    public required BranchRef Branch { get; init; }

    /// <summary>Comparison rule used.</summary>
    [JsonPropertyName("comparisonRule")]
    public required string ComparisonRule { get; init; }

    /// <summary>Comparison scope.</summary>
    [JsonPropertyName("comparisonScope")]
    public required string ComparisonScope { get; init; }

    /// <summary>Outcome of the comparison.</summary>
    [JsonPropertyName("outcome")]
    public required ComparisonOutcome Outcome { get; init; }

    /// <summary>Quantitative metrics from the comparison (e.g., relative error, deviation).</summary>
    [JsonPropertyName("metrics")]
    public required IReadOnlyDictionary<string, double> Metrics { get; init; }

    /// <summary>Human-readable message describing the result.</summary>
    [JsonPropertyName("message")]
    public required string Message { get; init; }

    /// <summary>Pullback operator ID from the observation provenance chain.</summary>
    [JsonPropertyName("pullbackOperatorId")]
    public required string PullbackOperatorId { get; init; }

    /// <summary>Observation branch ID from the observation provenance chain.</summary>
    [JsonPropertyName("observationBranchId")]
    public required string ObservationBranchId { get; init; }

    /// <summary>Full provenance metadata of the run that produced this comparison.</summary>
    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }

    /// <summary>Timestamp of comparison execution.</summary>
    [JsonPropertyName("executedAt")]
    public required DateTimeOffset ExecutedAt { get; init; }

    /// <summary>
    /// Creates a record for an invalid comparison (e.g., output type mismatch).
    /// </summary>
    public static ComparisonRecord CreateInvalid(
        ComparisonTemplate template,
        BranchRef branch,
        ProvenanceMeta provenance,
        string reason)
    {
        return new ComparisonRecord
        {
            ComparisonId = Guid.NewGuid().ToString("N"),
            TemplateId = template.TemplateId,
            ObservableId = template.ObservableId,
            ReferenceSourceId = template.ReferenceSourceId,
            ReferenceVersion = "N/A",
            Branch = branch,
            ComparisonRule = template.ComparisonRule,
            ComparisonScope = template.ComparisonScope,
            Outcome = ComparisonOutcome.Invalid,
            Metrics = new Dictionary<string, double>(),
            Message = reason,
            PullbackOperatorId = "N/A",
            ObservationBranchId = "N/A",
            Provenance = provenance,
            ExecutedAt = DateTimeOffset.UtcNow,
        };
    }
}
