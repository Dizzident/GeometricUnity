using System.Text.Json.Serialization;

namespace Gu.Phase2.Semantics;

/// <summary>
/// Declares a branch-equivalence relation for a study.
/// Two branches b_i ~ b_j are equivalent under a chosen study relation.
/// The relation is study-dependent, not universal.
/// </summary>
public sealed class EquivalenceSpec
{
    /// <summary>Unique identifier for this equivalence specification.</summary>
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    /// <summary>Human-readable name.</summary>
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    /// <summary>Object classes being compared (e.g. "observed-output", "residual-norm").</summary>
    [JsonPropertyName("comparedObjectClasses")]
    public required IReadOnlyList<string> ComparedObjectClasses { get; init; }

    /// <summary>Normalization procedure applied before comparison.</summary>
    [JsonPropertyName("normalizationProcedure")]
    public required string NormalizationProcedure { get; init; }

    /// <summary>Transformations allowed when declaring equivalence.</summary>
    [JsonPropertyName("allowedTransformations")]
    public required IReadOnlyList<string> AllowedTransformations { get; init; }

    /// <summary>Metric identifiers used for distance computation.</summary>
    [JsonPropertyName("metrics")]
    public required IReadOnlyList<string> Metrics { get; init; }

    /// <summary>Tolerance thresholds keyed by metric identifier.</summary>
    [JsonPropertyName("tolerances")]
    public required IReadOnlyDictionary<string, double> Tolerances { get; init; }

    /// <summary>Rule for interpreting the comparison result.</summary>
    [JsonPropertyName("interpretationRule")]
    public required string InterpretationRule { get; init; }
}
