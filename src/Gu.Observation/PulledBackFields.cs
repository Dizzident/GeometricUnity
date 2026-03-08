using Gu.Core;

namespace Gu.Observation;

/// <summary>
/// Container for DerivedState fields after sigma_h^* pullback to X_h.
/// All fields here live on the base space X_h.
/// </summary>
public sealed class PulledBackFields
{
    /// <summary>Curvature F pulled back to X_h.</summary>
    public required FieldTensor CurvatureF { get; init; }

    /// <summary>Torsion T pulled back to X_h.</summary>
    public required FieldTensor TorsionT { get; init; }

    /// <summary>Shiab S pulled back to X_h.</summary>
    public required FieldTensor ShiabS { get; init; }

    /// <summary>Residual Upsilon pulled back to X_h.</summary>
    public required FieldTensor ResidualUpsilon { get; init; }
}
