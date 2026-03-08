namespace Gu.Discretization;

/// <summary>
/// A family of local basis functions on a reference simplex.
/// Pre-tabulates values and gradients at quadrature points for efficiency.
/// </summary>
public sealed class BasisFamily
{
    /// <summary>Family identifier (e.g., "P1-simplex-2d").</summary>
    public required string FamilyId { get; init; }

    /// <summary>Dimension of the reference simplex.</summary>
    public required int SimplicialDimension { get; init; }

    /// <summary>Polynomial order of the basis.</summary>
    public required int Order { get; init; }

    /// <summary>Number of basis functions.</summary>
    public required int FunctionCount { get; init; }

    /// <summary>
    /// Pre-tabulated basis values at quadrature points.
    /// ValuesAtQuadPoints[basisIdx][quadPointIdx].
    /// </summary>
    public required double[][] ValuesAtQuadPoints { get; init; }

    /// <summary>
    /// Pre-tabulated basis gradients at quadrature points (in barycentric coordinates).
    /// GradientsAtQuadPoints[basisIdx][quadPointIdx][barycentricComponentIdx].
    /// </summary>
    public required double[][][] GradientsAtQuadPoints { get; init; }
}

/// <summary>
/// Factory for standard basis families on reference simplices.
/// </summary>
public static class BasisFamilyFactory
{
    /// <summary>
    /// Creates a P1 (linear) Lagrange basis on an n-simplex.
    /// P1 has (n+1) basis functions: one per vertex of the reference simplex.
    /// Each basis function equals the corresponding barycentric coordinate.
    /// </summary>
    public static BasisFamily P1(int simplicialDimension, QuadratureRule quadrature)
    {
        if (simplicialDimension < 1)
            throw new ArgumentOutOfRangeException(nameof(simplicialDimension));
        if (quadrature.SimplicialDimension != simplicialDimension)
            throw new ArgumentException("Quadrature dimension mismatch.");

        int nBasis = simplicialDimension + 1;
        int nQuad = quadrature.PointCount;
        int nBary = simplicialDimension + 1;

        // P1 basis function i = lambda_i (the i-th barycentric coordinate)
        var values = new double[nBasis][];
        var gradients = new double[nBasis][][];

        for (int b = 0; b < nBasis; b++)
        {
            values[b] = new double[nQuad];
            gradients[b] = new double[nQuad][];

            for (int q = 0; q < nQuad; q++)
            {
                // Value: lambda_b at quadrature point q
                values[b][q] = quadrature.Points[q][b];

                // Gradient of lambda_b in barycentric coords:
                // d(lambda_b)/d(lambda_j) = delta_{b,j}
                gradients[b][q] = new double[nBary];
                gradients[b][q][b] = 1.0;
            }
        }

        return new BasisFamily
        {
            FamilyId = $"P1-simplex-{simplicialDimension}d",
            SimplicialDimension = simplicialDimension,
            Order = 1,
            FunctionCount = nBasis,
            ValuesAtQuadPoints = values,
            GradientsAtQuadPoints = gradients,
        };
    }
}
