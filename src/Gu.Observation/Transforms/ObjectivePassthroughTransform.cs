using Gu.Core;

namespace Gu.Observation.Transforms;

/// <summary>
/// Passes through the full residual Upsilon_h coefficients as an observable.
/// Used when the comparison engine needs raw residual values, not derived norms.
/// Output type: Quantitative.
/// </summary>
public sealed class ObjectivePassthroughTransform : IDerivedObservableTransform
{
    public string ObservableId => "residual-passthrough";
    public OutputType OutputType => OutputType.Quantitative;
    public string TransformId => "residual-passthrough-v1";

    public double[] Compute(PulledBackFields pulledBackFields, ObservableRequest request)
    {
        // Return a copy of the residual coefficients (defensive copy, no mutation)
        var upsilon = pulledBackFields.ResidualUpsilon;
        var result = new double[upsilon.Coefficients.Length];
        Array.Copy(upsilon.Coefficients, result, result.Length);
        return result;
    }
}
