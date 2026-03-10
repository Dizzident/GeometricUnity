using System.Text.Json.Serialization;

namespace Gu.Phase3.Campaigns;

/// <summary>
/// Abstract target descriptor for boson comparison campaigns.
///
/// A target profile describes expected physical properties of a particle-like
/// excitation that a candidate boson might match. This is intentionally weaker
/// than claiming identification with a known particle.
/// </summary>
public abstract class TargetProfile
{
    /// <summary>Unique profile identifier.</summary>
    [JsonPropertyName("profileId")]
    public required string ProfileId { get; init; }

    /// <summary>Human-readable profile name.</summary>
    [JsonPropertyName("profileName")]
    public required string ProfileName { get; init; }

    /// <summary>Expected mass range (min, max). Units are dimensionless mass-like scale.</summary>
    [JsonPropertyName("expectedMassRange")]
    public required double[] ExpectedMassRange { get; init; }

    /// <summary>Expected multiplicity (degeneracy count).</summary>
    [JsonPropertyName("expectedMultiplicity")]
    public required int ExpectedMultiplicity { get; init; }

    /// <summary>Expected polarization type (e.g. "transverse", "longitudinal", "scalar").</summary>
    [JsonPropertyName("expectedPolarizationType")]
    public required string ExpectedPolarizationType { get; init; }

    /// <summary>Expected symmetry group (e.g. "su(2)", "u(1)").</summary>
    [JsonPropertyName("expectedSymmetryGroup")]
    public required string ExpectedSymmetryGroup { get; init; }

    /// <summary>Comparison tolerances.</summary>
    [JsonPropertyName("tolerances")]
    public TargetTolerances Tolerances { get; init; } = new();
}
