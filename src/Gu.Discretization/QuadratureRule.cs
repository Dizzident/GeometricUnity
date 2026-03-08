namespace Gu.Discretization;

/// <summary>
/// A quadrature rule on a reference simplex.
/// Provides points (in barycentric coordinates) and weights for numerical integration.
/// Physical integration requires scaling by |det(J)| of the reference-to-physical map.
/// </summary>
public sealed class QuadratureRule
{
    /// <summary>Unique rule identifier (e.g., "simplex-2d-order2").</summary>
    public required string RuleId { get; init; }

    /// <summary>Dimension of the reference simplex.</summary>
    public required int SimplicialDimension { get; init; }

    /// <summary>Polynomial exactness order.</summary>
    public required int Order { get; init; }

    /// <summary>Number of quadrature points.</summary>
    public int PointCount => Weights.Length;

    /// <summary>
    /// Barycentric coordinates of quadrature points.
    /// Points[i] has (SimplicialDimension + 1) components that sum to 1.
    /// </summary>
    public required double[][] Points { get; init; }

    /// <summary>
    /// Quadrature weights. Sum equals the reference simplex volume (1/SimplicialDimension!).
    /// </summary>
    public required double[] Weights { get; init; }
}

/// <summary>
/// Factory for standard quadrature rules on simplices.
/// </summary>
public static class QuadratureRuleFactory
{
    /// <summary>
    /// Creates a centroid (order-1) quadrature rule for an n-simplex.
    /// Single point at the centroid with weight = reference volume.
    /// </summary>
    public static QuadratureRule Centroid(int simplicialDimension)
    {
        if (simplicialDimension < 1)
            throw new ArgumentOutOfRangeException(nameof(simplicialDimension));

        int nv = simplicialDimension + 1;
        double coord = 1.0 / nv;
        var point = new double[nv];
        for (int i = 0; i < nv; i++)
            point[i] = coord;

        double volume = ReferenceSimplexVolume(simplicialDimension);

        return new QuadratureRule
        {
            RuleId = $"simplex-{simplicialDimension}d-centroid",
            SimplicialDimension = simplicialDimension,
            Order = 1,
            Points = new[] { point },
            Weights = new[] { volume },
        };
    }

    /// <summary>
    /// Creates a vertex-based (order-2) quadrature rule for an n-simplex.
    /// Uses (n+1) points located at positions biased toward each vertex.
    /// </summary>
    public static QuadratureRule VertexBased(int simplicialDimension)
    {
        if (simplicialDimension < 1)
            throw new ArgumentOutOfRangeException(nameof(simplicialDimension));

        int nv = simplicialDimension + 1;
        double volume = ReferenceSimplexVolume(simplicialDimension);

        // For n-simplex order 2: points at alpha*e_i + (1-alpha)/(n)*sum(e_j, j!=i)
        // where alpha depends on dimension
        double alpha, beta;
        if (simplicialDimension == 1)
        {
            // Gauss 2-point rule on edge
            double s = Math.Sqrt(3.0) / 6.0;
            return new QuadratureRule
            {
                RuleId = "simplex-1d-gauss2",
                SimplicialDimension = 1,
                Order = 2,
                Points = new[]
                {
                    new[] { 0.5 + s, 0.5 - s },
                    new[] { 0.5 - s, 0.5 + s },
                },
                Weights = new[] { volume / 2.0, volume / 2.0 },
            };
        }
        else if (simplicialDimension == 2)
        {
            // 3-point rule on triangle
            alpha = 2.0 / 3.0;
            beta = 1.0 / 6.0;
        }
        else if (simplicialDimension == 3)
        {
            // 4-point rule on tetrahedron
            double sqrt5 = Math.Sqrt(5.0);
            alpha = (5.0 + 3.0 * sqrt5) / 20.0;
            beta = (5.0 - sqrt5) / 20.0;
        }
        else
        {
            // General formula: place points near vertices
            // This is a simple scheme, not necessarily optimal for higher dimensions
            alpha = 1.0 - simplicialDimension * (1.0 / (simplicialDimension + 2.0));
            beta = 1.0 / (simplicialDimension + 2.0);
        }

        var points = new double[nv][];
        var weights = new double[nv];
        double weightEach = volume / nv;

        for (int i = 0; i < nv; i++)
        {
            points[i] = new double[nv];
            for (int j = 0; j < nv; j++)
                points[i][j] = (i == j) ? alpha : beta;
            weights[i] = weightEach;
        }

        return new QuadratureRule
        {
            RuleId = $"simplex-{simplicialDimension}d-order2",
            SimplicialDimension = simplicialDimension,
            Order = 2,
            Points = points,
            Weights = weights,
        };
    }

    /// <summary>
    /// Returns the volume of the reference n-simplex: 1/n!
    /// </summary>
    public static double ReferenceSimplexVolume(int n)
    {
        double vol = 1.0;
        for (int i = 2; i <= n; i++)
            vol /= i;
        return vol;
    }
}
