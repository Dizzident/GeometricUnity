using Gu.Geometry;
using Gu.Math;
using Gu.ReferenceCpu;

namespace Gu.ReferenceCpu.Tests;

public class CurvatureAssemblerTests
{
    private static SimplicialMesh SingleTriangle() =>
        MeshTopologyBuilder.Build(
            embeddingDimension: 2,
            simplicialDimension: 2,
            vertexCoordinates: new double[] { 0, 0, 1, 0, 0, 1 },
            vertexCount: 3,
            cellVertices: new[] { new[] { 0, 1, 2 } });

    private static SimplicialMesh SingleTetrahedron() =>
        MeshTopologyBuilder.Build(
            embeddingDimension: 3,
            simplicialDimension: 3,
            vertexCoordinates: new double[] { 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1 },
            vertexCount: 4,
            cellVertices: new[] { new[] { 0, 1, 2, 3 } });

    /// <summary>
    /// A flat (zero) connection should have zero curvature: F = d(0) + (1/2)[0, 0] = 0.
    /// </summary>
    [Fact]
    public void FlatConnection_ZeroCurvature()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var omega = ConnectionField.Zero(mesh, algebra);

        var curvature = CurvatureAssembler.Assemble(omega);

        Assert.Equal(mesh.FaceCount, curvature.FaceCount);
        Assert.All(curvature.Coefficients, c => Assert.Equal(0.0, c, 12));
        Assert.Equal(0.0, curvature.NormSquared(), 12);
    }

    /// <summary>
    /// For an abelian algebra (u(1)), [omega, omega] = 0,
    /// so F = d(omega) exactly.
    /// </summary>
    [Fact]
    public void AbelianAlgebra_CurvatureIsPureExteriorDerivative()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateAbelian(1);
        var omega = new ConnectionField(mesh, algebra);

        // Set edge values: omega on edge 0 = 1.0, edge 1 = 2.0, edge 2 = 3.0
        omega.SetEdgeValue(0, new[] { 1.0 });
        omega.SetEdgeValue(1, new[] { 2.0 });
        omega.SetEdgeValue(2, new[] { 3.0 });

        var curvature = CurvatureAssembler.Assemble(omega);

        // d(omega) on face = sum of signed edge values
        // Boundary orientations: +1, -1, +1
        // d(omega) = (+1)*1.0 + (-1)*2.0 + (+1)*3.0 = 2.0
        Assert.Equal(1, curvature.FaceCount);
        Assert.Equal(1, curvature.AlgebraDimension);
        Assert.Equal(2.0, curvature.Coefficients[0], 12);
    }

    /// <summary>
    /// For non-abelian algebra (su(2)), the bracket term contributes to curvature.
    /// Verify F = d(omega) + (1/2)[omega, omega] with non-zero bracket.
    /// </summary>
    [Fact]
    public void NonAbelianAlgebra_BracketContributes()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var omega = new ConnectionField(mesh, algebra);

        // Set omega on edges: different Lie algebra directions
        // Edge 0: T_1 direction
        omega.SetEdgeValue(0, new[] { 1.0, 0.0, 0.0 });
        // Edge 1: T_2 direction
        omega.SetEdgeValue(1, new[] { 0.0, 1.0, 0.0 });
        // Edge 2: zero
        omega.SetEdgeValue(2, new[] { 0.0, 0.0, 0.0 });

        var curvature = CurvatureAssembler.Assemble(omega);

        // d(omega) = (+1)*(1,0,0) + (-1)*(0,1,0) + (+1)*(0,0,0) = (1,-1,0)
        // [omega,omega] bracket contributions from edge pairs:
        // Pair (0,1): [T_1, T_2] = T_3 -> contributes sign0*sign1*[omega0, omega1]
        //   = (+1)*(-1)*[T_1, T_2] = -T_3 = (0,0,-1)
        // Pair (0,2): [T_1, 0] = 0
        // Pair (1,2): [T_2, 0] = 0
        // wedgeTerm = (0, 0, -1)
        // F = (1,-1,0) + (1/2)*(0,0,-1) = (1, -1, -0.5)

        var faceVal = curvature.GetFaceValueArray(0);
        Assert.Equal(1.0, faceVal[0], 10);
        Assert.Equal(-1.0, faceVal[1], 10);
        Assert.Equal(-0.5, faceVal[2], 10);
    }

    /// <summary>
    /// Flat connection on tetrahedron (3D) also gives zero curvature.
    /// </summary>
    [Fact]
    public void FlatConnection_3D_ZeroCurvature()
    {
        var mesh = SingleTetrahedron();
        var algebra = LieAlgebraFactory.CreateSu2();
        var omega = ConnectionField.Zero(mesh, algebra);

        var curvature = CurvatureAssembler.Assemble(omega);

        // Tet has C(4,3) = 4 faces
        Assert.Equal(4, curvature.FaceCount);
        Assert.All(curvature.Coefficients, c => Assert.Equal(0.0, c, 12));
    }

    /// <summary>
    /// ConnectionField.ToFieldTensor produces correct metadata.
    /// </summary>
    [Fact]
    public void ConnectionField_ToFieldTensor_CorrectMetadata()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var omega = ConnectionField.Zero(mesh, algebra);

        var tensor = omega.ToFieldTensor();

        Assert.Equal("omega_h", tensor.Label);
        Assert.Equal("Y_h", tensor.Signature.AmbientSpaceId);
        Assert.Equal("connection-1form", tensor.Signature.CarrierType);
        Assert.Equal("1", tensor.Signature.Degree);
        Assert.Equal("float64", tensor.Signature.NumericPrecision);
        Assert.Equal(mesh.EdgeCount * algebra.Dimension, tensor.Coefficients.Length);
    }

    /// <summary>
    /// CurvatureField.ToFieldTensor produces correct metadata.
    /// </summary>
    [Fact]
    public void CurvatureField_ToFieldTensor_CorrectMetadata()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var omega = ConnectionField.Zero(mesh, algebra);
        var curvature = CurvatureAssembler.Assemble(omega);

        var tensor = curvature.ToFieldTensor();

        Assert.Equal("F_h", tensor.Label);
        Assert.Equal("Y_h", tensor.Signature.AmbientSpaceId);
        Assert.Equal("curvature-2form", tensor.Signature.CarrierType);
        Assert.Equal("2", tensor.Signature.Degree);
        Assert.Equal(mesh.FaceCount * algebra.Dimension, tensor.Coefficients.Length);
    }

    /// <summary>
    /// Debug printer produces non-empty output.
    /// </summary>
    [Fact]
    public void DebugPrinter_ProducesOutput()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var omega = ConnectionField.Zero(mesh, algebra);
        var curvature = CurvatureAssembler.Assemble(omega);

        var connStr = DebugPrinter.PrintConnection(omega);
        var curvStr = DebugPrinter.PrintCurvature(curvature);

        Assert.Contains("omega_h", connStr);
        Assert.Contains("F_h", curvStr);
        Assert.Contains("edge", connStr);
        Assert.Contains("face", curvStr);
    }
}
