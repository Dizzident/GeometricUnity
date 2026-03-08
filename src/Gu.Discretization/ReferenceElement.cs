namespace Gu.Discretization;

/// <summary>
/// Combines a reference simplex definition, quadrature rule, and basis family
/// into a single element description used for local assembly operations.
/// </summary>
public sealed class ReferenceElement
{
    /// <summary>Dimension of the simplex (e.g., 2 for triangle, 3 for tet).</summary>
    public required int SimplicialDimension { get; init; }

    /// <summary>
    /// Vertices of the reference simplex in Cartesian coordinates.
    /// ReferenceVertices[vertexIdx][coordIdx].
    /// Standard reference simplex: vertex 0 at origin, vertex i at unit vector e_i.
    /// </summary>
    public required double[][] ReferenceVertices { get; init; }

    /// <summary>Quadrature rule on this reference element.</summary>
    public required QuadratureRule Quadrature { get; init; }

    /// <summary>Basis function family on this reference element.</summary>
    public required BasisFamily Basis { get; init; }

    /// <summary>Volume of the reference simplex (= 1/SimplicialDimension!).</summary>
    public double ReferenceVolume => QuadratureRuleFactory.ReferenceSimplexVolume(SimplicialDimension);
}

/// <summary>
/// Factory for standard reference elements.
/// </summary>
public static class ReferenceElementFactory
{
    /// <summary>
    /// Creates a P1 reference element with the specified quadrature order.
    /// Standard reference simplex: vertex 0 at origin, vertex i at e_i.
    /// </summary>
    public static ReferenceElement CreateP1(int simplicialDimension, int quadratureOrder = 1)
    {
        if (simplicialDimension < 1)
            throw new ArgumentOutOfRangeException(nameof(simplicialDimension));

        // Build reference vertices
        int nv = simplicialDimension + 1;
        var refVertices = new double[nv][];

        // Vertex 0 at origin
        refVertices[0] = new double[simplicialDimension];

        // Vertex i at unit vector e_{i-1}
        for (int i = 1; i < nv; i++)
        {
            refVertices[i] = new double[simplicialDimension];
            refVertices[i][i - 1] = 1.0;
        }

        // Choose quadrature rule
        var quadrature = quadratureOrder <= 1
            ? QuadratureRuleFactory.Centroid(simplicialDimension)
            : QuadratureRuleFactory.VertexBased(simplicialDimension);

        var basis = BasisFamilyFactory.P1(simplicialDimension, quadrature);

        return new ReferenceElement
        {
            SimplicialDimension = simplicialDimension,
            ReferenceVertices = refVertices,
            Quadrature = quadrature,
            Basis = basis,
        };
    }
}
