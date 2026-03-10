using System.Text.Json.Serialization;

namespace Gu.Phase3.Properties;

/// <summary>
/// Interaction proxy record from finite-difference cubic response.
/// C(v_i, v_j, v_k) ~ directional multilinear response of the gradient G.
/// (See IMPLEMENTATION_PLAN_P3.md Section 4.10.9)
///
/// This is not a scattering amplitude. It is a first nonlinear coupling descriptor.
/// </summary>
public sealed class InteractionProxyRecord
{
    /// <summary>Mode IDs involved (typically 3 for cubic).</summary>
    [JsonPropertyName("modeIds")]
    public required IReadOnlyList<string> ModeIds { get; init; }

    /// <summary>Cubic response value C(v_i, v_j, v_k).</summary>
    [JsonPropertyName("cubicResponse")]
    public required double CubicResponse { get; init; }

    /// <summary>Finite-difference step size used.</summary>
    [JsonPropertyName("epsilon")]
    public required double Epsilon { get; init; }

    /// <summary>Method: "finite-difference-gradient" or "finite-difference-residual".</summary>
    [JsonPropertyName("method")]
    public required string Method { get; init; }

    /// <summary>Background ID where the cubic response was evaluated.</summary>
    [JsonPropertyName("backgroundId")]
    public required string BackgroundId { get; init; }

    /// <summary>Estimated numerical error (from comparing different step sizes).</summary>
    [JsonPropertyName("estimatedError")]
    public double? EstimatedError { get; init; }
}
