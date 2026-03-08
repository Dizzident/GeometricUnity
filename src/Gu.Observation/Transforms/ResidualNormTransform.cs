using Gu.Core;

namespace Gu.Observation.Transforms;

/// <summary>
/// Computes ||Upsilon_h||^2 per vertex on X_h: the L2 norm squared of the residual.
/// Output type: Quantitative (numerical values with tolerances).
/// </summary>
public sealed class ResidualNormTransform : IDerivedObservableTransform
{
    public string ObservableId => "residual-norm-squared";
    public OutputType OutputType => OutputType.Quantitative;
    public string TransformId => "residual-norm-squared-v1";

    public double[] Compute(PulledBackFields pulledBackFields, ObservableRequest request)
    {
        var upsilon = pulledBackFields.ResidualUpsilon;
        int totalCoeffs = upsilon.Coefficients.Length;

        // If shape has two dimensions [nVertices, componentsPerVertex], compute per-vertex norm squared
        if (upsilon.Shape.Count == 2)
        {
            int nVertices = upsilon.Shape[0];
            int componentsPerVertex = upsilon.Shape[1];
            var result = new double[nVertices];

            for (int v = 0; v < nVertices; v++)
            {
                double sum = 0.0;
                int offset = v * componentsPerVertex;
                for (int c = 0; c < componentsPerVertex; c++)
                {
                    double val = upsilon.Coefficients[offset + c];
                    sum += val * val;
                }
                result[v] = sum;
            }

            return result;
        }

        // Scalar field: just square each coefficient
        var scalarResult = new double[totalCoeffs];
        for (int i = 0; i < totalCoeffs; i++)
        {
            scalarResult[i] = upsilon.Coefficients[i] * upsilon.Coefficients[i];
        }
        return scalarResult;
    }
}
