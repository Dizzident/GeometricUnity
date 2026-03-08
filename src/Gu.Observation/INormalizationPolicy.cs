using Gu.Core;

namespace Gu.Observation;

/// <summary>
/// Policy for normalizing observable values.
/// v1: only dimensionless/mesh-relative normalization.
/// </summary>
public interface INormalizationPolicy
{
    /// <summary>
    /// Apply normalization to the raw observable values.
    /// Returns the normalized values and the metadata describing what was applied.
    /// </summary>
    /// <param name="values">Raw observable values.</param>
    /// <param name="request">The request (may specify normalization).</param>
    /// <returns>Normalized values and the applied normalization metadata.</returns>
    (double[] Values, NormalizationMeta? Meta) Normalize(double[] values, ObservableRequest request);
}
