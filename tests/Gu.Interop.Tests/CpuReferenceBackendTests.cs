using Gu.Interop;

namespace Gu.Interop.Tests;

public class CpuReferenceBackendTests
{
    private static ManifestSnapshot CreateManifest() => new()
    {
        BaseDimension = 4,
        AmbientDimension = 14,
        LieAlgebraDimension = 3,
        LieAlgebraId = "su2",
        MeshCellCount = 10,
        MeshVertexCount = 20,
        ComponentOrderId = "order-row-major",
        TorsionBranchId = "local-algebraic",
        ShiabBranchId = "first-order-curvature",
    };

    [Fact]
    public void Initialize_SetsUpBackend()
    {
        using var backend = new CpuReferenceBackend();
        backend.Initialize(CreateManifest());
        // No exception means success
    }

    [Fact]
    public void AllocateBuffer_ReturnsValidHandle()
    {
        using var backend = new CpuReferenceBackend();
        backend.Initialize(CreateManifest());

        var layout = BufferLayoutDescriptor.CreateSoA("test", new[] { "a", "b" }, 10);
        var buffer = backend.AllocateBuffer(layout);

        Assert.NotNull(buffer);
        Assert.Equal(layout, buffer.Layout);
        Assert.False(buffer.IsDisposed);
    }

    [Fact]
    public void UploadDownload_RoundTrip()
    {
        using var backend = new CpuReferenceBackend();
        backend.Initialize(CreateManifest());

        var layout = BufferLayoutDescriptor.CreateSoA("test", new[] { "a" }, 5);
        var buffer = backend.AllocateBuffer(layout);

        var data = new double[] { 1.0, 2.0, 3.0, 4.0, 5.0 };
        backend.UploadBuffer(buffer, data);

        var result = new double[5];
        backend.DownloadBuffer(buffer, result);

        for (int i = 0; i < 5; i++)
            Assert.Equal(data[i], result[i]);
    }

    [Fact]
    public void FreeBuffer_DisposesHandle()
    {
        using var backend = new CpuReferenceBackend();
        backend.Initialize(CreateManifest());

        var layout = BufferLayoutDescriptor.CreateSoA("test", new[] { "a" }, 5);
        var buffer = backend.AllocateBuffer(layout);

        backend.FreeBuffer(buffer);

        Assert.True(buffer.IsDisposed);
    }

    [Fact]
    public void EvaluateResidual_ComputesDifference()
    {
        using var backend = new CpuReferenceBackend();
        backend.Initialize(CreateManifest());

        var layout = BufferLayoutDescriptor.CreateSoA("field", new[] { "c" }, 4);
        var shiab = backend.AllocateBuffer(layout);
        var torsion = backend.AllocateBuffer(layout);
        var residual = backend.AllocateBuffer(layout);

        backend.UploadBuffer(shiab, new double[] { 10.0, 20.0, 30.0, 40.0 });
        backend.UploadBuffer(torsion, new double[] { 1.0, 2.0, 3.0, 4.0 });

        backend.EvaluateResidual(shiab, torsion, residual);

        var result = new double[4];
        backend.DownloadBuffer(residual, result);

        Assert.Equal(9.0, result[0]);
        Assert.Equal(18.0, result[1]);
        Assert.Equal(27.0, result[2]);
        Assert.Equal(36.0, result[3]);
    }

    [Fact]
    public void EvaluateObjective_ComputesHalfSquaredNorm()
    {
        using var backend = new CpuReferenceBackend();
        backend.Initialize(CreateManifest());

        var layout = BufferLayoutDescriptor.CreateSoA("residual", new[] { "c" }, 3);
        var residual = backend.AllocateBuffer(layout);

        backend.UploadBuffer(residual, new double[] { 1.0, 2.0, 3.0 });

        double objective = backend.EvaluateObjective(residual);

        // (1/2) * (1 + 4 + 9) = 7.0
        Assert.Equal(7.0, objective, precision: 10);
    }

    [Fact]
    public void Version_IsCpuReference()
    {
        using var backend = new CpuReferenceBackend();
        Assert.Equal("cpu-reference", backend.Version.BackendId);
    }

    [Fact]
    public void GetLastError_InitiallyNull()
    {
        using var backend = new CpuReferenceBackend();
        Assert.Null(backend.GetLastError());
    }

    [Fact]
    public void OperationBeforeInitialize_Throws()
    {
        using var backend = new CpuReferenceBackend();
        var layout = BufferLayoutDescriptor.CreateSoA("test", new[] { "a" }, 5);
        var buffer = backend.AllocateBuffer(layout);
        backend.UploadBuffer(buffer, new double[] { 1.0, 2.0, 3.0, 4.0, 5.0 });

        Assert.Throws<InvalidOperationException>(() =>
            backend.EvaluateObjective(buffer));
    }

    [Fact]
    public void Disposed_ThrowsOnAllocate()
    {
        var backend = new CpuReferenceBackend();
        backend.Dispose();

        Assert.Throws<ObjectDisposedException>(() =>
            backend.AllocateBuffer(BufferLayoutDescriptor.CreateSoA("x", new[] { "a" }, 1)));
    }

    [Fact]
    public void InteropVersion_Compatibility()
    {
        var v1 = new InteropVersion { Major = 1, Minor = 0, Patch = 0, BackendId = "a" };
        var v1Minor = new InteropVersion { Major = 1, Minor = 1, Patch = 0, BackendId = "b" };
        var v2 = new InteropVersion { Major = 2, Minor = 0, Patch = 0, BackendId = "c" };

        Assert.True(v1.IsCompatibleWith(v1Minor));
        Assert.False(v1.IsCompatibleWith(v2));
    }
}
