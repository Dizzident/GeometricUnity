using System.Text.Json.Serialization;
using Gu.Phase3.Backgrounds;

namespace Gu.Phase3.Spectra;

/// <summary>
/// Specification for building a linearized operator bundle at a background.
/// </summary>
public sealed class LinearizedOperatorSpec
{
    /// <summary>Background record ID.</summary>
    [JsonPropertyName("backgroundId")]
    public required string BackgroundId { get; init; }

    /// <summary>
    /// Which operator type to use for the spectral problem.
    /// PHYSICS: GaussNewton is ONLY valid for B2 backgrounds.
    /// </summary>
    [JsonPropertyName("operatorType")]
    public required SpectralOperatorType OperatorType { get; init; }

    /// <summary>Physical mode formulation (P1/P2/P3).</summary>
    [JsonPropertyName("formulation")]
    public PhysicalModeFormulation Formulation { get; init; } = PhysicalModeFormulation.ProjectedComplement;

    /// <summary>Gauge penalty lambda (used in P1 formulation and Hessian construction).</summary>
    [JsonPropertyName("gaugeLambda")]
    public double GaugeLambda { get; init; } = 0.1;

    /// <summary>
    /// Admissibility level of the background. Used to enforce the GN-only-for-B2 rule.
    /// </summary>
    [JsonPropertyName("backgroundAdmissibility")]
    public required AdmissibilityLevel BackgroundAdmissibility { get; init; }
}
