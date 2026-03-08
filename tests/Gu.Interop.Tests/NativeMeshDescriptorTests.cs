using Gu.Interop;

namespace Gu.Interop.Tests;

/// <summary>
/// Tests for NativeMeshDescriptor: managed wrapper for mesh topology
/// metadata passed to native/CUDA backends.
/// </summary>
public class NativeMeshDescriptorTests
{
    [Fact]
    public void Create_SetsAllFields()
    {
        var desc = NativeMeshDescriptor.Create(
            baseDimension: 4,
            ambientDimension: 14,
            lieAlgebraDimension: 3,
            meshCellCount: 100,
            meshVertexCount: 50,
            meshEdgeCount: 200,
            meshFaceCount: 150);

        Assert.Equal(4, desc.BaseDimension);
        Assert.Equal(14, desc.AmbientDimension);
        Assert.Equal(3, desc.LieAlgebraDimension);
        Assert.Equal(100, desc.MeshCellCount);
        Assert.Equal(50, desc.MeshVertexCount);
        Assert.Equal(200, desc.MeshEdgeCount);
        Assert.Equal(150, desc.MeshFaceCount);
    }

    [Fact]
    public void FiberDimension_Computed()
    {
        var desc = NativeMeshDescriptor.Create(4, 14, 3, 10, 20, 30, 40);
        Assert.Equal(10, desc.FiberDimension);
    }

    [Fact]
    public void ConnectionDofCount_IsEdgesTimesDimG()
    {
        var desc = NativeMeshDescriptor.Create(4, 14, 3, 10, 20, 200, 150);
        Assert.Equal(600, desc.ConnectionDofCount); // 200 * 3
    }

    [Fact]
    public void CurvatureDofCount_IsFacesTimesDimG()
    {
        var desc = NativeMeshDescriptor.Create(4, 14, 3, 10, 20, 200, 150);
        Assert.Equal(450, desc.CurvatureDofCount); // 150 * 3
    }

    [Fact]
    public void FromManifest_CopiesFields()
    {
        var manifest = new ManifestSnapshot
        {
            BaseDimension = 4,
            AmbientDimension = 14,
            LieAlgebraDimension = 3,
            LieAlgebraId = "su2",
            MeshCellCount = 100,
            MeshVertexCount = 50,
            ComponentOrderId = "order-row-major",
            TorsionBranchId = "local-algebraic",
            ShiabBranchId = "first-order-curvature",
        };

        var desc = NativeMeshDescriptor.FromManifest(manifest, edgeCount: 200, faceCount: 150);

        Assert.Equal(4, desc.BaseDimension);
        Assert.Equal(14, desc.AmbientDimension);
        Assert.Equal(3, desc.LieAlgebraDimension);
        Assert.Equal(100, desc.MeshCellCount);
        Assert.Equal(50, desc.MeshVertexCount);
        Assert.Equal(200, desc.MeshEdgeCount);
        Assert.Equal(150, desc.MeshFaceCount);
    }

    [Fact]
    public void ToManifestSnapshot_ProducesValidSnapshot()
    {
        var desc = NativeMeshDescriptor.Create(4, 14, 3, 100, 50, 200, 150);
        var snapshot = desc.ToManifestSnapshot("su2", "order-row-major", "trivial", "identity");

        Assert.Equal(4, snapshot.BaseDimension);
        Assert.Equal(14, snapshot.AmbientDimension);
        Assert.Equal(3, snapshot.LieAlgebraDimension);
        Assert.Equal("su2", snapshot.LieAlgebraId);
        Assert.Equal(100, snapshot.MeshCellCount);
        Assert.Equal(50, snapshot.MeshVertexCount);
        Assert.Equal("order-row-major", snapshot.ComponentOrderId);
        Assert.Equal("trivial", snapshot.TorsionBranchId);
        Assert.Equal("identity", snapshot.ShiabBranchId);
    }

    [Fact]
    public void Validate_ValidDescriptor_NoThrow()
    {
        var desc = NativeMeshDescriptor.Create(4, 14, 3, 100, 50, 200, 150);
        desc.Validate(); // Should not throw
    }

    [Fact]
    public void Validate_BaseDimensionZero_Throws()
    {
        var desc = NativeMeshDescriptor.Create(0, 14, 3, 100, 50, 200, 150);
        Assert.Throws<InvalidOperationException>(() => desc.Validate());
    }

    [Fact]
    public void Validate_AmbientNotGreaterThanBase_Throws()
    {
        var desc = NativeMeshDescriptor.Create(14, 14, 3, 100, 50, 200, 150);
        Assert.Throws<InvalidOperationException>(() => desc.Validate());
    }

    [Fact]
    public void Validate_NegativeLieAlgebra_Throws()
    {
        var desc = NativeMeshDescriptor.Create(4, 14, -1, 100, 50, 200, 150);
        Assert.Throws<InvalidOperationException>(() => desc.Validate());
    }

    [Fact]
    public void Equality_SameValues_AreEqual()
    {
        var a = NativeMeshDescriptor.Create(4, 14, 3, 100, 50, 200, 150);
        var b = NativeMeshDescriptor.Create(4, 14, 3, 100, 50, 200, 150);

        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.False(a != b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        var a = NativeMeshDescriptor.Create(4, 14, 3, 100, 50, 200, 150);
        var b = NativeMeshDescriptor.Create(4, 14, 8, 100, 50, 200, 150);

        Assert.NotEqual(a, b);
        Assert.False(a == b);
        Assert.True(a != b);
    }

    [Fact]
    public void ToString_ContainsDimensions()
    {
        var desc = NativeMeshDescriptor.Create(4, 14, 3, 100, 50, 200, 150);
        var str = desc.ToString();

        Assert.Contains("4", str);
        Assert.Contains("14", str);
        Assert.Contains("3", str);
    }
}
