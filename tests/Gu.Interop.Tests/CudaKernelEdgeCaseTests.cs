using Gu.Interop;
using global::Gu.Math;

namespace Gu.Interop.Tests;

/// <summary>
/// Edge case tests T9-T13 from the GPU test plan.
/// Tests CUDA kernel behavior under edge conditions: large buffers,
/// zero inputs, flat connections, algebra packing, and memory stress.
/// </summary>
[Collection("GPU")]
[Trait("Category", "GPU")]
public class CudaKernelEdgeCaseTests
{
    private readonly CudaTestFixture _fixture;
    private const double Tolerance = 1e-14;

    public CudaKernelEdgeCaseTests(CudaTestFixture fixture) => _fixture = fixture;

    // -----------------------------------------------------------------
    // T9: Large buffer scaling -- verify round-trip for various sizes
    // -----------------------------------------------------------------

    [SkipIfNoCudaTheory]
    [InlineData(100)]
    [InlineData(1_000)]
    [InlineData(10_000)]
    [InlineData(100_000)]
    public void T9_LargeBufferRoundTrip(int size)
    {
        var layout = BufferLayoutDescriptor.CreateSoA("test", new[] { "c" }, size);
        var buffer = _fixture.GpuBackend.AllocateBuffer(layout);

        try
        {
            // Generate reproducible random data
            var rng = new Random(size);
            var upload = new double[size];
            for (int i = 0; i < size; i++)
                upload[i] = rng.NextDouble() * 200.0 - 100.0;

            _fixture.GpuBackend.UploadBuffer(buffer, upload);

            var download = new double[size];
            _fixture.GpuBackend.DownloadBuffer(buffer, download);

            // Verify exact bitwise round-trip
            for (int i = 0; i < size; i++)
            {
                Assert.Equal(upload[i], download[i]);
            }
        }
        finally
        {
            _fixture.GpuBackend.FreeBuffer(buffer);
        }
    }

    // -----------------------------------------------------------------
    // T10: Zero omega produces zero everything
    // -----------------------------------------------------------------

    [SkipIfNoCuda]
    public void T10_ZeroOmega_AllOutputsZero()
    {
        int edgeN = _fixture.EdgeCount * _fixture.DimG;
        int faceN = _fixture.FaceCount * _fixture.DimG;

        var omega = new double[edgeN]; // all zeros

        var edgeLayout = _fixture.CreateEdgeLayout();
        var faceLayout = _fixture.CreateFaceLayout();

        var omegaBuf = _fixture.GpuBackend.AllocateBuffer(edgeLayout);
        var curvBuf = _fixture.GpuBackend.AllocateBuffer(faceLayout);
        var torsionBuf = _fixture.GpuBackend.AllocateBuffer(faceLayout);
        var shiabBuf = _fixture.GpuBackend.AllocateBuffer(faceLayout);
        var residualBuf = _fixture.GpuBackend.AllocateBuffer(faceLayout);

        try
        {
            _fixture.GpuBackend.UploadBuffer(omegaBuf, omega);

            // Curvature of zero connection should be zero
            _fixture.GpuBackend.EvaluateCurvature(omegaBuf, curvBuf);
            var curvData = new double[faceN];
            _fixture.GpuBackend.DownloadBuffer(curvBuf, curvData);
            Assert.All(curvData, v => Assert.Equal(0.0, v));

            // Torsion of zero connection should be zero
            _fixture.GpuBackend.EvaluateTorsion(omegaBuf, torsionBuf);
            var torsionData = new double[faceN];
            _fixture.GpuBackend.DownloadBuffer(torsionBuf, torsionData);
            Assert.All(torsionData, v => Assert.Equal(0.0, v));

            // Shiab of zero connection should be zero
            _fixture.GpuBackend.EvaluateShiab(omegaBuf, shiabBuf);
            var shiabData = new double[faceN];
            _fixture.GpuBackend.DownloadBuffer(shiabBuf, shiabData);
            Assert.All(shiabData, v => Assert.Equal(0.0, v));

            // Residual: Upsilon = S - T = 0 - 0 = 0
            _fixture.GpuBackend.EvaluateResidual(shiabBuf, torsionBuf, residualBuf);
            var residualData = new double[faceN];
            _fixture.GpuBackend.DownloadBuffer(residualBuf, residualData);
            Assert.All(residualData, v => Assert.Equal(0.0, v));

            // Objective: (1/2)||0||^2 = 0
            double objective = _fixture.GpuBackend.EvaluateObjective(residualBuf);
            Assert.Equal(0.0, objective);
        }
        finally
        {
            _fixture.GpuBackend.FreeBuffer(omegaBuf);
            _fixture.GpuBackend.FreeBuffer(curvBuf);
            _fixture.GpuBackend.FreeBuffer(torsionBuf);
            _fixture.GpuBackend.FreeBuffer(shiabBuf);
            _fixture.GpuBackend.FreeBuffer(residualBuf);
        }
    }

    // -----------------------------------------------------------------
    // T11: Flat connection (omega = A0) => alpha = 0 => torsion = 0
    // -----------------------------------------------------------------

    [SkipIfNoCuda]
    public void T11_FlatConnection_ZeroTorsion()
    {
        int edgeN = _fixture.EdgeCount * _fixture.DimG;
        int faceN = _fixture.FaceCount * _fixture.DimG;

        // Generate a non-zero omega
        var omega = _fixture.GenerateOmega(seed: 777);

        // Upload omega as the background connection A0
        // so that alpha = omega - A0 = 0 => torsion should be zero
        _fixture.GpuBackend.UploadBackgroundConnection(omega, _fixture.EdgeCount, _fixture.DimG);

        var edgeLayout = _fixture.CreateEdgeLayout();
        var faceLayout = _fixture.CreateFaceLayout();

        var omegaBuf = _fixture.GpuBackend.AllocateBuffer(edgeLayout);
        var torsionBuf = _fixture.GpuBackend.AllocateBuffer(faceLayout);
        var curvBuf = _fixture.GpuBackend.AllocateBuffer(faceLayout);

        try
        {
            _fixture.GpuBackend.UploadBuffer(omegaBuf, omega);

            // Torsion should be zero since alpha = omega - A0 = 0
            _fixture.GpuBackend.EvaluateTorsion(omegaBuf, torsionBuf);
            var torsionData = new double[faceN];
            _fixture.GpuBackend.DownloadBuffer(torsionBuf, torsionData);
            Assert.All(torsionData, v => Assert.Equal(0.0, v, Tolerance));

            // Curvature should be non-zero (depends on omega, not on alpha)
            _fixture.GpuBackend.EvaluateCurvature(omegaBuf, curvBuf);
            var curvData = new double[faceN];
            _fixture.GpuBackend.DownloadBuffer(curvBuf, curvData);

            bool anyNonZero = false;
            for (int i = 0; i < curvData.Length; i++)
            {
                if (System.Math.Abs(curvData[i]) > Tolerance)
                {
                    anyNonZero = true;
                    break;
                }
            }
            Assert.True(anyNonZero, "Curvature should be non-zero for non-zero omega.");
        }
        finally
        {
            // Restore A0 to zero so other tests are not affected
            _fixture.GpuBackend.UploadBackgroundConnection(
                new double[edgeN], _fixture.EdgeCount, _fixture.DimG);

            _fixture.GpuBackend.FreeBuffer(omegaBuf);
            _fixture.GpuBackend.FreeBuffer(torsionBuf);
            _fixture.GpuBackend.FreeBuffer(curvBuf);
        }
    }

    // -----------------------------------------------------------------
    // T12: su(3) algebra packing is in a separate class below
    // (Su3AlgebraPackingTests) because it is CPU-only and must not
    // depend on CudaTestFixture which requires the CUDA native library.
    // -----------------------------------------------------------------

    // -----------------------------------------------------------------
    // T13: Memory stress -- allocate/free cycle
    // -----------------------------------------------------------------

    [SkipIfNoCuda]
    public void T13_MemoryStress_AllocateFreeCycle()
    {
        const int bufferCount = 100;
        const int bufferSize = 1000;
        const int cycles = 3;

        var rng = new Random(42);

        for (int cycle = 0; cycle < cycles; cycle++)
        {
            var layout = BufferLayoutDescriptor.CreateSoA(
                $"stress-{cycle}", new[] { "c" }, bufferSize);

            var buffers = new PackedBuffer[bufferCount];
            var uploadData = new double[bufferCount][];

            // Allocate and upload
            for (int i = 0; i < bufferCount; i++)
            {
                buffers[i] = _fixture.GpuBackend.AllocateBuffer(layout);
                uploadData[i] = new double[bufferSize];
                for (int j = 0; j < bufferSize; j++)
                    uploadData[i][j] = rng.NextDouble() * 2.0 - 1.0;

                _fixture.GpuBackend.UploadBuffer(buffers[i], uploadData[i]);
            }

            // Download and verify each buffer
            for (int i = 0; i < bufferCount; i++)
            {
                var download = new double[bufferSize];
                _fixture.GpuBackend.DownloadBuffer(buffers[i], download);

                for (int j = 0; j < bufferSize; j++)
                {
                    Assert.Equal(uploadData[i][j], download[j]);
                }
            }

            // Free all buffers
            for (int i = 0; i < bufferCount; i++)
            {
                _fixture.GpuBackend.FreeBuffer(buffers[i]);
            }
        }
    }

    // -----------------------------------------------------------------
    // Extra: IEEE 754 special values buffer fidelity
    // -----------------------------------------------------------------

    [SkipIfNoCuda]
    public void BufferFidelity_SpecialValues()
    {
        // Upload IEEE 754 edge-case values and verify bitwise round-trip
        var specialValues = new double[]
        {
            0.0,
            -0.0,
            double.Epsilon,                     // smallest positive subnormal
            -double.Epsilon,
            double.MinValue,                    // most negative finite
            double.MaxValue,                    // most positive finite
            1.0,
            -1.0,
            System.Math.PI,
            System.Math.E,
            5e-324,                             // smallest positive subnormal (same as double.Epsilon)
            2.2250738585072014e-308,            // smallest positive normal
            -2.2250738585072014e-308,
            1.7976931348623157e+308,            // largest finite
            -1.7976931348623157e+308,
            1e-300,                             // very small
            1e+300,                             // very large
            -1e-300,
            -1e+300,
            1.0000000000000002,                 // 1 + epsilon(1)
        };

        int n = specialValues.Length;
        var layout = BufferLayoutDescriptor.CreateSoA("ieee754", new[] { "c" }, n);
        var buffer = _fixture.GpuBackend.AllocateBuffer(layout);

        try
        {
            _fixture.GpuBackend.UploadBuffer(buffer, specialValues);

            var download = new double[n];
            _fixture.GpuBackend.DownloadBuffer(buffer, download);

            for (int i = 0; i < n; i++)
            {
                // Use BitConverter to verify exact bitwise equality
                long expectedBits = BitConverter.DoubleToInt64Bits(specialValues[i]);
                long actualBits = BitConverter.DoubleToInt64Bits(download[i]);

                Assert.True(expectedBits == actualBits,
                    $"Bitwise mismatch at index {i}: " +
                    $"expected {specialValues[i]:R} (0x{expectedBits:X16}), " +
                    $"got {download[i]:R} (0x{actualBits:X16})");
            }
        }
        finally
        {
            _fixture.GpuBackend.FreeBuffer(buffer);
        }
    }
}

/// <summary>
/// T12: su(3) algebra packing verification (CPU-only, no GPU needed).
///
/// This test is in a separate class because it must not depend on
/// CudaTestFixture. The fixture instantiates CudaNativeBackend in its
/// constructor, which will throw DllNotFoundException on machines without
/// the CUDA native library, causing all tests in the fixture-dependent
/// class to fail -- including CPU-only tests.
///
/// Creating a second CudaNativeBackend while the fixture's is active
/// would also conflict because the native library has global state
/// (gu_initialize/gu_shutdown). By keeping this test standalone, it
/// runs reliably on any machine.
/// </summary>
[Trait("Category", "GPU")]
public class Su3AlgebraPackingTests
{
    [Fact]
    public void T12_Su3AlgebraData_PacksCorrectly()
    {
        var algebra = LieAlgebraFactory.CreateSu3();

        Assert.Equal(8, algebra.Dimension);
        Assert.Equal("su3", algebra.AlgebraId);

        // Verify Jacobi identity holds
        double jacobiViolation = algebra.ValidateJacobiIdentity();
        Assert.True(jacobiViolation < 1e-14,
            $"Jacobi identity violation: {jacobiViolation}");

        // Verify antisymmetry
        double antisymViolation = algebra.ValidateAntisymmetry();
        Assert.True(antisymViolation < 1e-14,
            $"Antisymmetry violation: {antisymViolation}");

        // Verify metric symmetry
        double metricViolation = algebra.ValidateMetricSymmetry();
        Assert.True(metricViolation < 1e-14,
            $"Metric symmetry violation: {metricViolation}");

        // Pack into upload format
        var uploadData = AlgebraUploadData.FromLieAlgebra(algebra);

        Assert.Equal(8, uploadData.Dimension);
        Assert.Equal(8 * 8 * 8, uploadData.StructureConstants.Length);
        Assert.Equal(8 * 8, uploadData.InvariantMetric.Length);

        // Verify structure constants were cloned (not aliased)
        Assert.NotSame(algebra.StructureConstants, uploadData.StructureConstants);
        Assert.Equal(algebra.StructureConstants, uploadData.StructureConstants);

        // Verify metric was cloned
        Assert.NotSame(algebra.InvariantMetric, uploadData.InvariantMetric);
        Assert.Equal(algebra.InvariantMetric, uploadData.InvariantMetric);

        // Verify specific known structure constants:
        // f_{123} = 1.0 => f^3_{12} stored at [0*64 + 1*8 + 2] = [10]
        Assert.Equal(1.0, uploadData.StructureConstants[0 * 64 + 1 * 8 + 2]);
        // f_{147} = 0.5 => f^7_{14} stored at [0*64 + 3*8 + 6] = [30]
        Assert.Equal(0.5, uploadData.StructureConstants[0 * 64 + 3 * 8 + 6]);

        // Verify Killing form diagonal: g_{ii} = -3 for su(3)
        for (int i = 0; i < 8; i++)
        {
            Assert.Equal(-3.0, uploadData.InvariantMetric[i * 8 + i]);
        }

        // Verify off-diagonal is zero
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (i != j)
                {
                    Assert.Equal(0.0, uploadData.InvariantMetric[i * 8 + j]);
                }
            }
        }
    }
}
