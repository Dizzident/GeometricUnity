using System.Text.Json.Serialization;
using Gu.Core;

namespace Gu.ExternalComparison;

/// <summary>
/// Defines a single comparison to perform: what observable, against what reference,
/// using what rule, and with what tolerance. Links to a falsifier condition.
/// The MinimumOutputType guard enforces Section 17.4: never force a quantitative
/// comparison on an output that is only structural.
/// </summary>
public sealed class ComparisonTemplate
{
    /// <summary>Unique template identifier, referenced by EnvironmentSpec.ComparisonTemplateIds.</summary>
    [JsonPropertyName("templateId")]
    public required string TemplateId { get; init; }

    /// <summary>Adapter type to dispatch to: "curated_table", "simulated_benchmark", "structural_fact", etc.</summary>
    [JsonPropertyName("adapterType")]
    public required string AdapterType { get; init; }

    /// <summary>Observable ID to compare (must match ObservableSnapshot.ObservableId).</summary>
    [JsonPropertyName("observableId")]
    public required string ObservableId { get; init; }

    /// <summary>Reference source identifier (e.g., "PDG-2024", "benchmark-su2-flat").</summary>
    [JsonPropertyName("referenceSourceId")]
    public required string ReferenceSourceId { get; init; }

    /// <summary>Comparison rule: "relative_error", "structural_match", "order_of_magnitude", etc.</summary>
    [JsonPropertyName("comparisonRule")]
    public required string ComparisonRule { get; init; }

    /// <summary>Scope classification (Section 18.3).</summary>
    [JsonPropertyName("comparisonScope")]
    public required string ComparisonScope { get; init; }

    /// <summary>Tolerance policy for this comparison.</summary>
    [JsonPropertyName("tolerance")]
    public required TolerancePolicy Tolerance { get; init; }

    /// <summary>Falsifier condition this comparison is linked to.</summary>
    [JsonPropertyName("falsifierCondition")]
    public required string FalsifierCondition { get; init; }

    /// <summary>
    /// Minimum output type required for this comparison (Section 17.4 guard).
    /// If the observable's OutputType is below this, the comparison returns Invalid.
    /// </summary>
    [JsonPropertyName("minimumOutputType")]
    public required OutputType MinimumOutputType { get; init; }
}
