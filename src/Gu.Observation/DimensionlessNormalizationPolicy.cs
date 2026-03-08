using Gu.Core;

namespace Gu.Observation;

/// <summary>
/// v1 normalization: dimensionless/mesh-relative only.
/// If the request specifies normalization, applies the scale factor.
/// Otherwise returns values unchanged.
/// </summary>
public sealed class DimensionlessNormalizationPolicy : INormalizationPolicy
{
    public (double[] Values, NormalizationMeta? Meta) Normalize(double[] values, ObservableRequest request)
    {
        if (request.Normalization is null)
            return (values, null);

        var meta = request.Normalization;
        if (meta.ScaleFactor == 1.0)
            return (values, meta);

        var normalized = new double[values.Length];
        for (int i = 0; i < values.Length; i++)
        {
            normalized[i] = values[i] * meta.ScaleFactor;
        }
        return (normalized, meta);
    }
}
