using System.Text.Json.Serialization;

namespace Gu.Phase5.QuantitativeValidation;

/// <summary>
/// An external target value for quantitative comparison (M49).
/// Physics note: the reference campaign may mix toy-placeholder controls, derived-synthetic
/// checks, and stronger benchmark targets. None of these labels implies experimental truth.
/// </summary>
public sealed class ExternalTarget
{
    /// <summary>Human-readable label for this target.</summary>
    [JsonPropertyName("label")]
    public required string Label { get; init; }

    /// <summary>Observable ID this target applies to.</summary>
    [JsonPropertyName("observableId")]
    public required string ObservableId { get; init; }

    /// <summary>Target value.</summary>
    [JsonPropertyName("value")]
    public required double Value { get; init; }

    /// <summary>Target uncertainty (1-sigma equivalent).</summary>
    [JsonPropertyName("uncertainty")]
    public required double Uncertainty { get; init; }

    /// <summary>Provenance string for this target (e.g., "synthetic-toy-v1").</summary>
    [JsonPropertyName("source")]
    public required string Source { get; init; }

    /// <summary>Optional provenance string distinct from Source when the target was bridged or normalized.</summary>
    [JsonPropertyName("targetProvenance")]
    public string? TargetProvenance { get; init; }

    /// <summary>Evidence tier (e.g., "toy-placeholder").</summary>
    [JsonPropertyName("evidenceTier")]
    public string? EvidenceTier { get; init; }

    /// <summary>
    /// Benchmark class used for reporting separation.
    /// Examples: "control", "internal-benchmark", "external-measurement".
    /// </summary>
    [JsonPropertyName("benchmarkClass")]
    public string? BenchmarkClass { get; init; }

    /// <summary>Physical particle identifier when this target is a real particle-property target.</summary>
    [JsonPropertyName("particleId")]
    public string? ParticleId { get; init; }

    /// <summary>Physical observable type, for example mass, width, coupling, or branching-ratio.</summary>
    [JsonPropertyName("physicalObservableType")]
    public string? PhysicalObservableType { get; init; }

    /// <summary>Unit family required by the physical target, for example dimensionless or mass-energy.</summary>
    [JsonPropertyName("unitFamily")]
    public string? UnitFamily { get; init; }

    /// <summary>Target unit, for example GeV or dimensionless.</summary>
    [JsonPropertyName("unit")]
    public string? Unit { get; init; }

    /// <summary>Canonical citation for physical target evidence.</summary>
    [JsonPropertyName("citation")]
    public string? Citation { get; init; }

    /// <summary>Source URL for physical target evidence.</summary>
    [JsonPropertyName("sourceUrl")]
    public string? SourceUrl { get; init; }

    /// <summary>Date the physical target source was retrieved, in yyyy-MM-dd format.</summary>
    [JsonPropertyName("retrievedAt")]
    public string? RetrievedAt { get; init; }

    /// <summary>Confidence level for upper/lower-limit physical targets, for example 95% CL.</summary>
    [JsonPropertyName("confidenceLevel")]
    public string? ConfidenceLevel { get; init; }

    /// <summary>Optional environment ID that this target must be matched against.</summary>
    [JsonPropertyName("targetEnvironmentId")]
    public string? TargetEnvironmentId { get; init; }

    /// <summary>Optional environment tier that this target must be matched against.</summary>
    [JsonPropertyName("targetEnvironmentTier")]
    public string? TargetEnvironmentTier { get; init; }

    /// <summary>
    /// Distribution model for this target.
    /// Allowed values: "gaussian" (default), "gaussian-asymmetric", "student-t".
    /// </summary>
    [JsonPropertyName("distributionModel")]
    public string DistributionModel { get; init; } = "gaussian";

    /// <summary>Lower uncertainty (1-sigma equivalent) for asymmetric targets. Null = use Uncertainty.</summary>
    [JsonPropertyName("uncertaintyLower")]
    public double? UncertaintyLower { get; init; }

    /// <summary>Upper uncertainty (1-sigma equivalent) for asymmetric targets. Null = use Uncertainty.</summary>
    [JsonPropertyName("uncertaintyUpper")]
    public double? UncertaintyUpper { get; init; }

    /// <summary>Degrees of freedom for Student-t targets. Required when DistributionModel == "student-t".</summary>
    [JsonPropertyName("studentTDegreesOfFreedom")]
    public double? StudentTDegreesOfFreedom { get; init; }

    /// <summary>Optional diagnostic notes for this target.</summary>
    [JsonPropertyName("notes")]
    public string? Notes { get; init; }
}
