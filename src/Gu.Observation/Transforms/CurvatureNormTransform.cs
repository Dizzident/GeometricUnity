using Gu.Core;

namespace Gu.Observation.Transforms;

/// <summary>
/// Computes ||F_h||^2 per vertex on X_h: the L2 norm squared of the curvature.
/// Output type: Quantitative.
/// </summary>
public sealed class CurvatureNormTransform : IDerivedObservableTransform
{
    public string ObservableId => "curvature-norm-squared";
    public OutputType OutputType => OutputType.Quantitative;
    public string TransformId => "curvature-norm-squared-v1";

    public double[] Compute(PulledBackFields pulledBackFields, ObservableRequest request)
    {
        var curvature = pulledBackFields.CurvatureF;
        int totalCoeffs = curvature.Coefficients.Length;

        if (curvature.Shape.Count == 2)
        {
            int nVertices = curvature.Shape[0];
            int componentsPerVertex = curvature.Shape[1];
            var result = new double[nVertices];

            for (int v = 0; v < nVertices; v++)
            {
                double sum = 0.0;
                int offset = v * componentsPerVertex;
                for (int c = 0; c < componentsPerVertex; c++)
                {
                    double val = curvature.Coefficients[offset + c];
                    sum += val * val;
                }
                result[v] = sum;
            }

            return result;
        }

        var scalarResult = new double[totalCoeffs];
        for (int i = 0; i < totalCoeffs; i++)
        {
            scalarResult[i] = curvature.Coefficients[i] * curvature.Coefficients[i];
        }
        return scalarResult;
    }
}
