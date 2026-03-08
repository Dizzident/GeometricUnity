using System.Text.Json.Serialization;

namespace Gu.Core;

/// <summary>
/// Linearization state: Jacobian, adjoint, and gradient-like residual (Section 10.5).
/// </summary>
public sealed class LinearizationState
{
    /// <summary>Jacobian J_h = dUpsilon_h / domega_h.</summary>
    [JsonPropertyName("jacobian")]
    public required LinearOperatorModel Jacobian { get; init; }

    /// <summary>Optional adjoint operator.</summary>
    [JsonPropertyName("adjoint")]
    public LinearOperatorModel? Adjoint { get; init; }

    /// <summary>Gradient-like residual J^T M Upsilon.</summary>
    [JsonPropertyName("gradientLikeResidual")]
    public required FieldTensor GradientLikeResidual { get; init; }

    /// <summary>Spectral diagnostics (e.g., condition number estimates).</summary>
    [JsonPropertyName("spectralDiagnostics")]
    public IReadOnlyDictionary<string, double>? SpectralDiagnostics { get; init; }
}
