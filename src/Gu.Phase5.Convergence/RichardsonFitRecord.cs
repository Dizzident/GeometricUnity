using System.Text.Json.Serialization;

namespace Gu.Phase5.Convergence;

/// <summary>
/// Result of a Richardson extrapolation fit for one target quantity (M47).
/// Fit model: Q(h) = Q_0 + C * h^p
/// </summary>
public sealed class RichardsonFitRecord
{
    /// <summary>Target quantity identifier.</summary>
    [JsonPropertyName("quantityId")]
    public required string QuantityId { get; init; }

    /// <summary>Extrapolated continuum limit Q_0 = Q(h→0).</summary>
    [JsonPropertyName("estimatedLimit")]
    public required double EstimatedLimit { get; init; }

    /// <summary>Estimated convergence order p.</summary>
    [JsonPropertyName("estimatedOrder")]
    public required double EstimatedOrder { get; init; }

    /// <summary>Residual of the fit (L2 norm of fit errors).</summary>
    [JsonPropertyName("residual")]
    public required double Residual { get; init; }

    /// <summary>Mesh parameters used in the fit (coarsest to finest).</summary>
    [JsonPropertyName("meshParameters")]
    public required double[] MeshParameters { get; init; }

    /// <summary>Quantity values at each mesh parameter.</summary>
    [JsonPropertyName("values")]
    public required double[] Values { get; init; }
}
