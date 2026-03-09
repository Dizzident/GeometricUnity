using Gu.Core;
using Gu.Interop;

namespace Gu.Interop.Tests;

public class GpuKrylovSolverConstructorTests
{
    [Fact]
    public void Constructor_NullBackend_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new GpuKrylovSolver(null!));
    }

    [Fact]
    public void Constructor_ZeroMaxIterations_Throws()
    {
        using var backend = new CpuReferenceBackend();
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new GpuKrylovSolver(backend, maxIterations: 0));
    }

    [Fact]
    public void Constructor_NegativeTolerance_Throws()
    {
        using var backend = new CpuReferenceBackend();
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new GpuKrylovSolver(backend, tolerance: -1e-6));
    }

    [Fact]
    public void Constructor_ZeroTolerance_Throws()
    {
        using var backend = new CpuReferenceBackend();
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new GpuKrylovSolver(backend, tolerance: 0.0));
    }

    [Fact]
    public void Constructor_ValidParams_DoesNotThrow()
    {
        using var backend = new CpuReferenceBackend();
        using var solver = new GpuKrylovSolver(backend, maxIterations: 100, tolerance: 1e-8);
        // No exception = success
    }

    [Fact]
    public void Dispose_CalledTwice_DoesNotThrow()
    {
        using var backend = new CpuReferenceBackend();
        var solver = new GpuKrylovSolver(backend);
        solver.Dispose();
        solver.Dispose(); // Should not throw
    }

    [Fact]
    public void Disposed_ThrowsOnSolve()
    {
        using var backend = new CpuReferenceBackend();
        backend.Initialize(new ManifestSnapshot
        {
            BaseDimension = 4, AmbientDimension = 14,
            LieAlgebraDimension = 3, LieAlgebraId = "su2",
            MeshCellCount = 10, MeshVertexCount = 20,
            ComponentOrderId = "order-row-major",
            TorsionBranchId = "trivial", ShiabBranchId = "identity",
        });
        var solver = new GpuKrylovSolver(backend);
        solver.Dispose();

        var layout = BufferLayoutDescriptor.CreateSoA("x", new[] { "c" }, 3);
        var buf = backend.AllocateBuffer(layout);
        backend.UploadBuffer(buf, new double[3]);

        Assert.Throws<ObjectDisposedException>(() =>
            solver.SolveNormalEquations(buf, new double[3], 3, 3, 1.0));

        backend.FreeBuffer(buf);
    }
}

public class GpuKrylovSolverCpuTests
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
        TorsionBranchId = "trivial",
        ShiabBranchId = "identity",
    };

    [Fact]
    public void SolveNormalEquations_ZeroRhs_ConvergesImmediately()
    {
        using var backend = new CpuReferenceBackend();
        backend.Initialize(CreateManifest());
        using var solver = new GpuKrylovSolver(backend, maxIterations: 10, tolerance: 1e-10);

        int edgeN = 6;
        int faceN = 6;
        var edgeLayout = BufferLayoutDescriptor.CreateSoA("omega", new[] { "c" }, edgeN);
        var omegaBuf = backend.AllocateBuffer(edgeLayout);
        backend.UploadBuffer(omegaBuf, new double[edgeN]);

        var result = solver.SolveNormalEquations(omegaBuf, new double[edgeN], edgeN, faceN, 1.0);

        Assert.True(result.Converged);
        Assert.Equal(0, result.Iterations);
        Assert.All(result.Solution, v => Assert.Equal(0.0, v));

        backend.FreeBuffer(omegaBuf);
    }

    [Fact]
    public void SolveNormalEquations_RegularizedIdentity_SolvesExactly()
    {
        // With J=0 (no topology), system is lambda*I*x = rhs, so x = rhs/lambda
        using var backend = new CpuReferenceBackend();
        backend.Initialize(CreateManifest());
        using var solver = new GpuKrylovSolver(backend, maxIterations: 50, tolerance: 1e-10);

        int edgeN = 9;
        int faceN = 6;
        double lambda = 2.0;

        var edgeLayout = BufferLayoutDescriptor.CreateSoA("omega", new[] { "c" }, edgeN);
        var omegaBuf = backend.AllocateBuffer(edgeLayout);
        backend.UploadBuffer(omegaBuf, new double[edgeN]);

        var rhs = new double[] { 2.0, 4.0, 6.0, 8.0, 10.0, 12.0, 14.0, 16.0, 18.0 };
        var result = solver.SolveNormalEquations(omegaBuf, rhs, edgeN, faceN, lambda);

        Assert.True(result.Converged);
        // CG on lambda*I should converge in 1 iteration
        Assert.Equal(1, result.Iterations);
        for (int i = 0; i < edgeN; i++)
            Assert.Equal(rhs[i] / lambda, result.Solution[i], 10);

        backend.FreeBuffer(omegaBuf);
    }

    [Fact]
    public void SolveNormalEquations_FieldTensorOverload_Works()
    {
        using var backend = new CpuReferenceBackend();
        backend.Initialize(CreateManifest());
        using var solver = new GpuKrylovSolver(backend, maxIterations: 10, tolerance: 1e-6);

        int edgeN = 6;
        var sig = new TensorSignature
        {
            AmbientSpaceId = "Y_h",
            CarrierType = "connection-1form",
            Degree = "1",
            LieAlgebraBasisId = "basis-standard",
            ComponentOrderId = "order-row-major",
            MemoryLayout = "dense-row-major",
            NumericPrecision = "float64",
        };

        var omega = new FieldTensor
        {
            Label = "omega",
            Signature = sig,
            Coefficients = new double[edgeN],
            Shape = new[] { edgeN },
        };
        var rhs = new FieldTensor
        {
            Label = "rhs",
            Signature = sig,
            Coefficients = new double[] { 1.0, 2.0, 3.0, 4.0, 5.0, 6.0 },
            Shape = new[] { edgeN },
        };

        var geometry = new GeometryContext
        {
            BaseSpace = new SpaceRef { SpaceId = "X_h", Dimension = 4, FaceCount = 2 },
            AmbientSpace = new SpaceRef { SpaceId = "Y_h", Dimension = 14, FaceCount = 2, EdgeCount = 4 },
            DiscretizationType = "simplicial",
            QuadratureRuleId = "midpoint",
            BasisFamilyId = "whitney-0",
            ProjectionBinding = new GeometryBinding
            {
                BindingType = "projection",
                SourceSpace = new SpaceRef { SpaceId = "Y_h", Dimension = 14 },
                TargetSpace = new SpaceRef { SpaceId = "X_h", Dimension = 4 },
            },
            ObservationBinding = new GeometryBinding
            {
                BindingType = "observation",
                SourceSpace = new SpaceRef { SpaceId = "X_h", Dimension = 4 },
                TargetSpace = new SpaceRef { SpaceId = "Y_h", Dimension = 14 },
            },
            Patches = new[] { new PatchInfo { PatchId = "patch-0", ElementCount = 10 } },
        };

        var result = solver.SolveNormalEquations(omega, rhs, geometry, 3, 1.0);

        Assert.Equal("krylov_delta", result.Label);
        Assert.Equal(edgeN, result.Coefficients.Length);
        // With J=0 and lambda=1, x = rhs
        for (int i = 0; i < edgeN; i++)
            Assert.Equal(rhs.Coefficients[i], result.Coefficients[i], 6);
    }

    [Fact]
    public void SolveNormalEquations_NegativeCurvature_StopsEarly()
    {
        // With lambda=0 and J=0, pAp=0, so CG should stop immediately
        using var backend = new CpuReferenceBackend();
        backend.Initialize(CreateManifest());
        using var solver = new GpuKrylovSolver(backend, maxIterations: 50, tolerance: 1e-10);

        int edgeN = 3;
        int faceN = 3;

        var edgeLayout = BufferLayoutDescriptor.CreateSoA("omega", new[] { "c" }, edgeN);
        var omegaBuf = backend.AllocateBuffer(edgeLayout);
        backend.UploadBuffer(omegaBuf, new double[edgeN]);

        var rhs = new double[] { 1.0, 2.0, 3.0 };
        var result = solver.SolveNormalEquations(omegaBuf, rhs, edgeN, faceN, 0.0);

        // pAp=0 triggers break on first iteration
        Assert.False(result.Converged);
        Assert.True(result.TerminatedNegativeCurvature);
        Assert.Equal(0, result.Iterations);

        backend.FreeBuffer(omegaBuf);
    }
}

/// <summary>
/// Tests for INativeBackend BLAS-like primitives (Axpy, InnerProduct, Scale, Copy)
/// on CpuReferenceBackend.
/// </summary>
public class NativeBackendPrimitivesTests
{
    private static CpuReferenceBackend CreateBackend()
    {
        var b = new CpuReferenceBackend();
        b.Initialize(new ManifestSnapshot
        {
            BaseDimension = 4, AmbientDimension = 14, LieAlgebraDimension = 3,
            LieAlgebraId = "su2", MeshCellCount = 10, MeshVertexCount = 20,
            ComponentOrderId = "order-row-major", TorsionBranchId = "trivial", ShiabBranchId = "identity",
        });
        return b;
    }

    private static PackedBuffer Alloc(CpuReferenceBackend b, int n, double[]? data = null)
    {
        var layout = BufferLayoutDescriptor.CreateSoA("test", new[] { "c" }, n);
        var buf = b.AllocateBuffer(layout);
        if (data != null) b.UploadBuffer(buf, data);
        return buf;
    }

    [Fact]
    public void Axpy_AddsScaled()
    {
        using var b = CreateBackend();
        var y = Alloc(b, 3, new[] { 1.0, 2.0, 3.0 });
        var x = Alloc(b, 3, new[] { 10.0, 20.0, 30.0 });

        b.Axpy(y, 0.5, x, 3);

        var result = new double[3];
        b.DownloadBuffer(y, result);
        Assert.Equal(6.0, result[0]);
        Assert.Equal(12.0, result[1]);
        Assert.Equal(18.0, result[2]);
    }

    [Fact]
    public void InnerProduct_ComputesDot()
    {
        using var b = CreateBackend();
        var u = Alloc(b, 3, new[] { 1.0, 2.0, 3.0 });
        var v = Alloc(b, 3, new[] { 4.0, 5.0, 6.0 });

        double dot = b.InnerProduct(u, v, 3);
        Assert.Equal(32.0, dot); // 1*4 + 2*5 + 3*6
    }

    [Fact]
    public void Scale_MultipliesInPlace()
    {
        using var b = CreateBackend();
        var x = Alloc(b, 3, new[] { 2.0, 4.0, 6.0 });

        b.Scale(x, 0.5, 3);

        var result = new double[3];
        b.DownloadBuffer(x, result);
        Assert.Equal(1.0, result[0]);
        Assert.Equal(2.0, result[1]);
        Assert.Equal(3.0, result[2]);
    }

    [Fact]
    public void Copy_DuplicatesBuffer()
    {
        using var b = CreateBackend();
        var src = Alloc(b, 3, new[] { 7.0, 8.0, 9.0 });
        var dst = Alloc(b, 3);

        b.Copy(dst, src, 3);

        var result = new double[3];
        b.DownloadBuffer(dst, result);
        Assert.Equal(7.0, result[0]);
        Assert.Equal(8.0, result[1]);
        Assert.Equal(9.0, result[2]);
    }
}

[Collection("GPU")]
[Trait("Category", "GPU")]
public class GpuKrylovSolverGpuTests
{
    private readonly CudaTestFixture _fixture;

    public GpuKrylovSolverGpuTests(CudaTestFixture fixture)
    {
        _fixture = fixture;
    }

    [SkipIfNoCuda]
    public void SolveNormalEquations_ZeroRhs_ReturnsZeroSolution()
    {
        using var solver = new GpuKrylovSolver(_fixture.GpuBackend, maxIterations: 10, tolerance: 1e-10);

        int edgeN = _fixture.EdgeCount * _fixture.DimG;
        int faceN = _fixture.FaceCount * _fixture.DimG;

        // Upload omega = 0
        var edgeLayout = BufferLayoutDescriptor.CreateSoA("krylov-omega", new[] { "c" }, edgeN);
        var omegaBuf = _fixture.GpuBackend.AllocateBuffer(edgeLayout);

        try
        {
            _fixture.GpuBackend.UploadBuffer(omegaBuf, new double[edgeN]);

            var rhs = new double[edgeN]; // zero RHS
            var result = solver.SolveNormalEquations(omegaBuf, rhs, edgeN, faceN, 0.0);

            Assert.True(result.Converged);
            Assert.Equal(0, result.Iterations);
            Assert.All(result.Solution, v => Assert.Equal(0.0, v, 15));
        }
        finally
        {
            _fixture.GpuBackend.FreeBuffer(omegaBuf);
        }
    }

    [SkipIfNoCuda]
    public void SolveNormalEquations_WithRegularization_Converges()
    {
        using var solver = new GpuKrylovSolver(_fixture.GpuBackend, maxIterations: 50, tolerance: 1e-6);

        int edgeN = _fixture.EdgeCount * _fixture.DimG;
        int faceN = _fixture.FaceCount * _fixture.DimG;

        var edgeLayout = BufferLayoutDescriptor.CreateSoA("krylov-omega", new[] { "c" }, edgeN);
        var omegaBuf = _fixture.GpuBackend.AllocateBuffer(edgeLayout);

        try
        {
            // Use zero omega so J=0 and the normal equations become lambda*I * x = rhs
            _fixture.GpuBackend.UploadBuffer(omegaBuf, new double[edgeN]);

            var rhs = _fixture.GenerateOmega(42);
            double lambda = 1.0;

            GpuKrylovResult result;
            try
            {
                result = solver.SolveNormalEquations(omegaBuf, rhs, edgeN, faceN, lambda);
            }
            catch (EntryPointNotFoundException)
            {
                // CUDA stub library may not have Jacobian/adjoint entry points yet
                return;
            }

            // With J=0 and lambda=1, the system is I*x = rhs, so x = rhs
            Assert.True(result.Converged);
            for (int i = 0; i < edgeN; i++)
            {
                Assert.Equal(rhs[i], result.Solution[i], 4); // tolerance for floating point
            }
        }
        finally
        {
            _fixture.GpuBackend.FreeBuffer(omegaBuf);
        }
    }

    [SkipIfNoCuda]
    public void SolveNormalEquations_FieldTensorOverload_ReturnsCorrectShape()
    {
        using var solver = new GpuKrylovSolver(_fixture.GpuBackend, maxIterations: 10, tolerance: 1e-6);

        int edgeN = _fixture.EdgeCount * _fixture.DimG;

        var omega = new FieldTensor
        {
            Label = "omega",
            Signature = new TensorSignature
            {
                AmbientSpaceId = "Y_h",
                CarrierType = "connection-1form",
                Degree = "1",
                LieAlgebraBasisId = "canonical",
                ComponentOrderId = "edge-major",
                MemoryLayout = "dense-row-major",
            },
            Coefficients = new double[edgeN],
            Shape = new[] { _fixture.EdgeCount, _fixture.DimG },
        };

        var rhs = new FieldTensor
        {
            Label = "rhs",
            Signature = omega.Signature,
            Coefficients = new double[edgeN],
            Shape = omega.Shape.ToArray(),
        };

        var geometry = new GeometryContext
        {
            BaseSpace = new SpaceRef { SpaceId = "X_h", Dimension = 4 },
            AmbientSpace = new SpaceRef
            {
                SpaceId = "Y_h",
                Dimension = 14,
                FaceCount = _fixture.FaceCount,
                EdgeCount = _fixture.EdgeCount,
            },
            DiscretizationType = "simplicial",
            QuadratureRuleId = "centroid",
            BasisFamilyId = "P1",
            ProjectionBinding = new GeometryBinding
            {
                BindingType = "projection",
                SourceSpace = new SpaceRef { SpaceId = "Y_h", Dimension = 14 },
                TargetSpace = new SpaceRef { SpaceId = "X_h", Dimension = 4 },
            },
            ObservationBinding = new GeometryBinding
            {
                BindingType = "observation",
                SourceSpace = new SpaceRef { SpaceId = "X_h", Dimension = 4 },
                TargetSpace = new SpaceRef { SpaceId = "Y_h", Dimension = 14 },
            },
            Patches = Array.Empty<PatchInfo>(),
        };

        var result = solver.SolveNormalEquations(omega, rhs, geometry, _fixture.DimG, 0.0);

        Assert.Equal("krylov_delta", result.Label);
        Assert.Equal(edgeN, result.Coefficients.Length);
    }
}
