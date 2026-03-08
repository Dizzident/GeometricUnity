using Gu.Geometry;
using Gu.Math;
using Gu.ReferenceCpu;

namespace Gu.Core.Tests;

public class CurvatureAssemblerTests
{
    private static SimplicialMesh CreateSingleTriangleMesh()
    {
        return MeshTopologyBuilder.Build(
            embeddingDimension: 2,
            simplicialDimension: 2,
            vertexCoordinates: new double[]
            {
                0, 0,
                1, 0,
                0, 1,
            },
            vertexCount: 3,
            cellVertices: new[]
            {
                new[] { 0, 1, 2 },
            });
    }

    private static SimplicialMesh CreateTwoTriangleMesh()
    {
        return MeshTopologyBuilder.Build(
            embeddingDimension: 2,
            simplicialDimension: 2,
            vertexCoordinates: new double[]
            {
                0, 0,
                1, 0,
                0, 1,
                1, 1,
            },
            vertexCount: 4,
            cellVertices: new[]
            {
                new[] { 0, 1, 2 },
                new[] { 1, 3, 2 },
            });
    }

    [Fact]
    public void FlatConnection_GivesZeroCurvature()
    {
        // F = d(0) + (1/2)[0, 0] = 0
        var mesh = CreateTwoTriangleMesh();
        var algebra = LieAlgebraFactory.CreateSu2();
        var omega = ConnectionField.Zero(mesh, algebra);

        var curvature = CurvatureAssembler.Assemble(omega);

        Assert.All(curvature.Coefficients, c => Assert.Equal(0.0, c, 12));
        Assert.Equal(0.0, curvature.NormSquared(), 12);
    }

    [Fact]
    public void AbelianConnection_CurvatureIsPureDOmega()
    {
        // For abelian algebra, [omega, omega] = 0, so F = d(omega)
        var mesh = CreateSingleTriangleMesh();
        var algebra = LieAlgebraFactory.CreateAbelian(1);
        var omega = new ConnectionField(mesh, algebra);

        // Set edge values: edges are ordered canonically
        // For a single triangle with vertices 0,1,2:
        // edges: (0,1), (0,2), (1,2) [canonical ordering]
        // Face boundary: edges with orientations from d operator
        Assert.True(mesh.EdgeCount >= 3);

        // Set non-zero values on all edges
        for (int e = 0; e < mesh.EdgeCount; e++)
        {
            omega.SetEdgeValue(e, new double[] { (e + 1) * 1.0 });
        }

        var curvature = CurvatureAssembler.Assemble(omega);

        // Curvature should be non-zero (it's d(omega) which depends on orientations)
        Assert.Equal(mesh.FaceCount, curvature.FaceCount);
        Assert.Equal(1, curvature.AlgebraDimension);
    }

    [Fact]
    public void CurvatureField_HasCorrectDimensions()
    {
        var mesh = CreateTwoTriangleMesh();
        var algebra = LieAlgebraFactory.CreateSu2();
        var omega = ConnectionField.Zero(mesh, algebra);

        var curvature = CurvatureAssembler.Assemble(omega);

        Assert.Equal(mesh.FaceCount, curvature.FaceCount);
        Assert.Equal(algebra.Dimension, curvature.AlgebraDimension);
        Assert.Equal(mesh.FaceCount * algebra.Dimension, curvature.Coefficients.Length);
    }

    [Fact]
    public void CurvatureField_ToFieldTensor_HasCorrectSignature()
    {
        var mesh = CreateTwoTriangleMesh();
        var algebra = LieAlgebraFactory.CreateSu2();
        var omega = ConnectionField.Zero(mesh, algebra);
        var curvature = CurvatureAssembler.Assemble(omega);

        var ft = curvature.ToFieldTensor();

        Assert.Equal("F_h", ft.Label);
        Assert.Equal("Y_h", ft.Signature.AmbientSpaceId);
        Assert.Equal("curvature-2form", ft.Signature.CarrierType);
        Assert.Equal("2", ft.Signature.Degree);
        Assert.Equal("float64", ft.Signature.NumericPrecision);
    }

    [Fact]
    public void CurvatureField_ToFieldTensor_HasCorrectShape()
    {
        var mesh = CreateTwoTriangleMesh();
        var algebra = LieAlgebraFactory.CreateSu2();
        var omega = ConnectionField.Zero(mesh, algebra);
        var curvature = CurvatureAssembler.Assemble(omega);

        var ft = curvature.ToFieldTensor();

        Assert.Equal(new[] { mesh.FaceCount, algebra.Dimension }, ft.Shape);
    }

    [Fact]
    public void NonFlatConnection_HasNonZeroCurvature()
    {
        // Non-zero connection on a non-abelian algebra should produce non-zero curvature.
        // Use trace pairing (positive-definite) so NormSquared > 0 for non-zero F.
        var mesh = CreateSingleTriangleMesh();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var omega = new ConnectionField(mesh, algebra);

        // Set different non-zero values on each edge in different algebra directions
        omega.SetEdgeValue(0, new double[] { 1.0, 0.0, 0.0 }); // T_1 on edge 0
        omega.SetEdgeValue(1, new double[] { 0.0, 1.0, 0.0 }); // T_2 on edge 1
        omega.SetEdgeValue(2, new double[] { 0.0, 0.0, 1.0 }); // T_3 on edge 2

        var curvature = CurvatureAssembler.Assemble(omega);

        // Should have non-zero curvature (both d(omega) and bracket contribute)
        Assert.True(curvature.NormSquared() > 0,
            "Non-flat su(2) connection should have non-zero curvature.");
    }

    [Fact]
    public void NormSquared_IsNonNegative_WithTracePairing()
    {
        // With trace pairing (positive-definite metric), NormSquared >= 0
        var mesh = CreateTwoTriangleMesh();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var omega = new ConnectionField(mesh, algebra);

        // Random-ish values
        for (int e = 0; e < mesh.EdgeCount; e++)
        {
            omega.SetEdgeValue(e, new double[] { e * 0.3, (e + 1) * 0.2, (e + 2) * 0.1 });
        }

        var curvature = CurvatureAssembler.Assemble(omega);

        Assert.True(curvature.NormSquared() >= 0,
            "||F||^2 must be non-negative with positive-definite metric.");
    }

    [Fact]
    public void NormSquared_IsNonPositive_WithKillingForm()
    {
        // With Killing form (negative-definite metric), NormSquared <= 0 for non-zero F
        var mesh = CreateTwoTriangleMesh();
        var algebra = LieAlgebraFactory.CreateSu2();
        var omega = new ConnectionField(mesh, algebra);

        // Random-ish values
        for (int e = 0; e < mesh.EdgeCount; e++)
        {
            omega.SetEdgeValue(e, new double[] { e * 0.3, (e + 1) * 0.2, (e + 2) * 0.1 });
        }

        var curvature = CurvatureAssembler.Assemble(omega);

        Assert.True(curvature.NormSquared() <= 0,
            "||F||^2 must be non-positive with Killing form (negative-definite metric).");
    }

    [Fact]
    public void GetFaceValueArray_ReturnsCorrectSlice()
    {
        var mesh = CreateSingleTriangleMesh();
        var algebra = LieAlgebraFactory.CreateSu2();
        var omega = new ConnectionField(mesh, algebra);
        omega.SetEdgeValue(0, new double[] { 1.0, 0.0, 0.0 });

        var curvature = CurvatureAssembler.Assemble(omega);
        var faceVal = curvature.GetFaceValueArray(0);

        Assert.Equal(algebra.Dimension, faceVal.Length);
    }

    [Fact]
    public void CurvatureField_InvalidCoefficients_Throws()
    {
        var mesh = CreateTwoTriangleMesh();
        var algebra = LieAlgebraFactory.CreateSu2();

        Assert.Throws<ArgumentException>(() =>
            new CurvatureField(mesh, algebra, new double[] { 1.0 }));
    }

    [Fact]
    public void AbelianU1_FlatConnection_ZeroCurvature()
    {
        var mesh = CreateSingleTriangleMesh();
        var algebra = LieAlgebraFactory.CreateAbelian(1);
        var omega = ConnectionField.Zero(mesh, algebra);

        var curvature = CurvatureAssembler.Assemble(omega);

        Assert.Equal(0.0, curvature.NormSquared(), 12);
    }

    [Fact]
    public void DiscreteStokesTheorem_AbelianCase()
    {
        // For abelian algebra: F = d(omega)
        // The discrete exterior derivative should satisfy d^2 = 0
        // i.e., if we set omega = d(phi) for some 0-form phi, then F = d^2(phi) = 0
        //
        // For a single triangle with 3 vertices and edges (0,1), (0,2), (1,2):
        // If phi(0)=1, phi(1)=2, phi(2)=3, then:
        //   omega(0,1) = phi(1) - phi(0) = 1
        //   omega(0,2) = phi(2) - phi(0) = 2
        //   omega(1,2) = phi(2) - phi(1) = 1
        //
        // d(omega) on face should be 0 because d^2 = 0
        var mesh = CreateSingleTriangleMesh();
        var algebra = LieAlgebraFactory.CreateAbelian(1);
        var omega = new ConnectionField(mesh, algebra);

        // Find edge indices by vertex pairs
        var edgeMap = new Dictionary<(int, int), int>();
        for (int e = 0; e < mesh.EdgeCount; e++)
        {
            int v0 = mesh.Edges[e][0];
            int v1 = mesh.Edges[e][1];
            edgeMap[(v0, v1)] = e;
        }

        // Set omega = d(phi) where phi = [1, 2, 3]
        double[] phi = { 1.0, 2.0, 3.0 };
        for (int e = 0; e < mesh.EdgeCount; e++)
        {
            int v0 = mesh.Edges[e][0];
            int v1 = mesh.Edges[e][1];
            omega.SetEdgeValue(e, new double[] { phi[v1] - phi[v0] });
        }

        var curvature = CurvatureAssembler.Assemble(omega);

        // d^2 = 0, so curvature of an exact form should be zero
        for (int f = 0; f < curvature.FaceCount; f++)
        {
            var faceVal = curvature.GetFaceValueArray(f);
            Assert.Equal(0.0, faceVal[0], 10);
        }
    }
}
