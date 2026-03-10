using System.Text.Json.Serialization;

namespace Gu.Phase3.Campaigns;

/// <summary>
/// Uncertainty budget for external target profile properties.
///
/// Each value represents one standard deviation of uncertainty
/// in the corresponding target property.
/// </summary>
public sealed class UncertaintyBudget
{
    /// <summary>Mass uncertainty (absolute).</summary>
    [JsonPropertyName("massUncertainty")]
    public double MassUncertainty { get; init; }

    /// <summary>Multiplicity uncertainty.</summary>
    [JsonPropertyName("multiplicityUncertainty")]
    public int MultiplicityUncertainty { get; init; }

    /// <summary>Polarization type uncertainty (as a probability of misclassification).</summary>
    [JsonPropertyName("polarizationUncertainty")]
    public double PolarizationUncertainty { get; init; }

    /// <summary>Symmetry group uncertainty (as a probability of misclassification).</summary>
    [JsonPropertyName("symmetryGroupUncertainty")]
    public double SymmetryGroupUncertainty { get; init; }
}
