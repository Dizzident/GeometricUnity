using System.Text.Json.Serialization;

namespace Gu.Phase3.Campaigns;

/// <summary>
/// External analogy descriptor for BC2 mode comparison.
/// Represents an external data profile (e.g., a known boson's properties)
/// with typed descriptors and uncertainty budgets.
/// </summary>
public sealed class ExternalAnalogyDescriptor
{
    /// <summary>Unique descriptor identifier.</summary>
    [JsonPropertyName("descriptorId")]
    public required string DescriptorId { get; init; }

    /// <summary>Human-readable label (e.g., "photon-like", "W-boson analogy").</summary>
    [JsonPropertyName("label")]
    public required string Label { get; init; }

    /// <summary>Reference mass value (in eigenvalue units).</summary>
    [JsonPropertyName("referenceMass")]
    public double ReferenceMass { get; init; }

    /// <summary>Uncertainty on reference mass.</summary>
    [JsonPropertyName("massUncertainty")]
    public double MassUncertainty { get; init; }

    /// <summary>Expected multiplicity (spin degrees of freedom etc.).</summary>
    [JsonPropertyName("expectedMultiplicity")]
    public int ExpectedMultiplicity { get; init; } = 1;

    /// <summary>Maximum allowed gauge leak for compatibility.</summary>
    [JsonPropertyName("maxGaugeLeak")]
    public double MaxGaugeLeak { get; init; } = 0.1;

    /// <summary>Source of the external data.</summary>
    [JsonPropertyName("source")]
    public required string Source { get; init; }

    /// <summary>Descriptive tags.</summary>
    [JsonPropertyName("tags")]
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();
}
