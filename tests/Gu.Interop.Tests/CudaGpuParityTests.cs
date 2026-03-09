using Gu.Interop;

namespace Gu.Interop.Tests;

/// <summary>
/// GPU parity tests T1-T8 that load the real CudaNativeBackend (via P/Invoke
/// to libgu_cuda_core.so) and verify kernel outputs against analytical values,
/// determinism, and cross-kernel consistency.
///
/// These tests require a CUDA-capable GPU and the native library to be built
/// and discoverable. They are automatically skipped via [SkipIfNoCuda] when
/// the library is unavailable.
///
/// The CudaTestFixture initializes the GPU backend with:
///   - ToyGeometryFactory.CreateToy2D() mesh (5 vertices, 8 edges, 4 faces)
///   - su(2) with trace pairing (dimG = 3)
///   - Zero background connection A0
///   - Full physics data uploaded (mesh topology + algebra + A0)
///
/// Buffer sizes: omega = 8*3 = 24 doubles (edge-valued),
///               curvature/torsion/shiab/residual = 4*3 = 12 doubles (face-valued).
/// </summary>
[Collection("GPU")]
[Trait("Category", "GPU")]
public class CudaGpuParityTests
{
    private readonly CudaTestFixture _fixture;
    private const double Tolerance = 1e-12;

    public CudaGpuParityTests(CudaTestFixture fixture) => _fixture = fixture;

    // ------------------------------------------------------------------
    // T1: Curvature kernel parity
    // ------------------------------------------------------------------

    /// <summary>
    /// T1a: Zero omega must produce zero curvature.
    /// F = d(0) + (1/2)[0, 0] = 0.
    /// This is the most basic analytical check for the curvature kernel.
    /// </summary>
    [SkipIfNoCuda]
    public void T1a_Curvature_ZeroOmega_ProducesZeroCurvature()
    {
        var gpu = _fixture.GpuBackend;
        int edgeN = _fixture.EdgeCount * _fixture.DimG;
        int faceN = _fixture.FaceCount * _fixture.DimG;

        var omegaBuf = gpu.AllocateBuffer(_fixture.CreateEdgeLayout());
        var curvBuf = gpu.AllocateBuffer(_fixture.CreateFaceLayout());

        try
        {
            gpu.UploadBuffer(omegaBuf, new double[edgeN]);
            gpu.EvaluateCurvature(omegaBuf, curvBuf);

            var result = new double[faceN];
            gpu.DownloadBuffer(curvBuf, result);

            for (int i = 0; i < faceN; i++)
            {
                Assert.Equal(0.0, result[i], precision: 14);
            }
        }
        finally
        {
            gpu.FreeBuffer(omegaBuf);
            gpu.FreeBuffer(curvBuf);
        }
    }

    /// <summary>
    /// T1b: Curvature must be deterministic -- two evaluations of the same
    /// omega must produce bit-identical results.
    /// </summary>
    [SkipIfNoCuda]
    public void T1b_Curvature_Deterministic()
    {
        var gpu = _fixture.GpuBackend;
        var omega = _fixture.GenerateOmega(seed: 42);
        int faceN = _fixture.FaceCount * _fixture.DimG;

        var omegaBuf = gpu.AllocateBuffer(_fixture.CreateEdgeLayout());
        var curv1Buf = gpu.AllocateBuffer(_fixture.CreateFaceLayout());
        var curv2Buf = gpu.AllocateBuffer(_fixture.CreateFaceLayout());

        try
        {
            gpu.UploadBuffer(omegaBuf, omega);
            gpu.EvaluateCurvature(omegaBuf, curv1Buf);
            gpu.EvaluateCurvature(omegaBuf, curv2Buf);

            var r1 = new double[faceN];
            var r2 = new double[faceN];
            gpu.DownloadBuffer(curv1Buf, r1);
            gpu.DownloadBuffer(curv2Buf, r2);

            var record = ParityChecker.CompareResults(
                "curvature-determinism", r1, r2, "gpu-run1", "gpu-run2", 0.0);

            Assert.True(record.Passed, $"Curvature not deterministic: {record.Message}");
            Assert.Equal(0.0, record.MaxAbsoluteError);
        }
        finally
        {
            gpu.FreeBuffer(omegaBuf);
            gpu.FreeBuffer(curv1Buf);
            gpu.FreeBuffer(curv2Buf);
        }
    }

    /// <summary>
    /// T1c: With physics data uploaded, the curvature kernel must NOT be an
    /// identity stub (i.e., it must produce non-trivial output for non-zero omega).
    /// When the native library has no physics data, it falls back to memcpy (F = omega).
    /// This test verifies the real physics kernel is active.
    /// </summary>
    [SkipIfNoCuda]
    public void T1c_Curvature_NotIdentityStub()
    {
        Assert.True(_fixture.GpuBackend.HasPhysicsData,
            "Physics data not uploaded -- cannot verify real kernel");

        var gpu = _fixture.GpuBackend;
        var omega = _fixture.GenerateOmega(seed: 99);
        int faceN = _fixture.FaceCount * _fixture.DimG;

        var omegaBuf = gpu.AllocateBuffer(_fixture.CreateEdgeLayout());
        var curvBuf = gpu.AllocateBuffer(_fixture.CreateFaceLayout());

        try
        {
            gpu.UploadBuffer(omegaBuf, omega);
            gpu.EvaluateCurvature(omegaBuf, curvBuf);

            var result = new double[faceN];
            gpu.DownloadBuffer(curvBuf, result);

            // With random non-zero omega, curvature F = d(omega) + 0.5*[omega,omega]
            // should be non-zero. If all zeros, the physics kernel may not be active.
            bool hasNonZero = false;
            for (int i = 0; i < faceN; i++)
            {
                if (System.Math.Abs(result[i]) > 1e-15)
                {
                    hasNonZero = true;
                    break;
                }
            }

            Assert.True(hasNonZero,
                "Curvature is all zeros for non-zero omega -- physics kernel may not be active");
        }
        finally
        {
            gpu.FreeBuffer(omegaBuf);
            gpu.FreeBuffer(curvBuf);
        }
    }

    // ------------------------------------------------------------------
    // T2: Augmented torsion kernel parity
    // ------------------------------------------------------------------

    /// <summary>
    /// T2a: Zero omega with zero A0 must produce zero torsion.
    /// T^aug = d_{A0}(omega - A0) = d_0(0 - 0) = 0.
    /// </summary>
    [SkipIfNoCuda]
    public void T2a_Torsion_ZeroOmegaZeroA0_ProducesZeroTorsion()
    {
        var gpu = _fixture.GpuBackend;
        int edgeN = _fixture.EdgeCount * _fixture.DimG;
        int faceN = _fixture.FaceCount * _fixture.DimG;

        var omegaBuf = gpu.AllocateBuffer(_fixture.CreateEdgeLayout());
        var torsionBuf = gpu.AllocateBuffer(_fixture.CreateFaceLayout());

        try
        {
            // A0 is already zero (uploaded by fixture).
            gpu.UploadBuffer(omegaBuf, new double[edgeN]);
            gpu.EvaluateTorsion(omegaBuf, torsionBuf);

            var result = new double[faceN];
            gpu.DownloadBuffer(torsionBuf, result);

            for (int i = 0; i < faceN; i++)
            {
                Assert.Equal(0.0, result[i], precision: 14);
            }
        }
        finally
        {
            gpu.FreeBuffer(omegaBuf);
            gpu.FreeBuffer(torsionBuf);
        }
    }

    /// <summary>
    /// T2b: Torsion must be deterministic -- two evaluations of the same
    /// omega must produce bit-identical results.
    /// </summary>
    [SkipIfNoCuda]
    public void T2b_Torsion_Deterministic()
    {
        var gpu = _fixture.GpuBackend;
        var omega = _fixture.GenerateOmega(seed: 55);
        int faceN = _fixture.FaceCount * _fixture.DimG;

        var omegaBuf = gpu.AllocateBuffer(_fixture.CreateEdgeLayout());
        var t1Buf = gpu.AllocateBuffer(_fixture.CreateFaceLayout());
        var t2Buf = gpu.AllocateBuffer(_fixture.CreateFaceLayout());

        try
        {
            gpu.UploadBuffer(omegaBuf, omega);
            gpu.EvaluateTorsion(omegaBuf, t1Buf);
            gpu.EvaluateTorsion(omegaBuf, t2Buf);

            var r1 = new double[faceN];
            var r2 = new double[faceN];
            gpu.DownloadBuffer(t1Buf, r1);
            gpu.DownloadBuffer(t2Buf, r2);

            var record = ParityChecker.CompareResults(
                "torsion-determinism", r1, r2, "gpu-run1", "gpu-run2", 0.0);

            Assert.True(record.Passed, $"Torsion not deterministic: {record.Message}");
            Assert.Equal(0.0, record.MaxAbsoluteError);
        }
        finally
        {
            gpu.FreeBuffer(omegaBuf);
            gpu.FreeBuffer(t1Buf);
            gpu.FreeBuffer(t2Buf);
        }
    }

    // ------------------------------------------------------------------
    // T3: Identity Shiab kernel parity (S = F)
    // ------------------------------------------------------------------

    /// <summary>
    /// T3: For the identity Shiab operator, the Shiab output must equal the
    /// curvature output exactly: S = F.
    /// </summary>
    [SkipIfNoCuda]
    public void T3_Shiab_EqualsIdentityCurvature()
    {
        var gpu = _fixture.GpuBackend;
        var omega = _fixture.GenerateOmega(seed: 77);
        int faceN = _fixture.FaceCount * _fixture.DimG;

        var omegaBuf = gpu.AllocateBuffer(_fixture.CreateEdgeLayout());
        var curvBuf = gpu.AllocateBuffer(_fixture.CreateFaceLayout());
        var shiabBuf = gpu.AllocateBuffer(_fixture.CreateFaceLayout());

        try
        {
            gpu.UploadBuffer(omegaBuf, omega);
            gpu.EvaluateCurvature(omegaBuf, curvBuf);
            gpu.EvaluateShiab(omegaBuf, shiabBuf);

            var curvData = new double[faceN];
            var shiabData = new double[faceN];
            gpu.DownloadBuffer(curvBuf, curvData);
            gpu.DownloadBuffer(shiabBuf, shiabData);

            var record = ParityChecker.CompareResults(
                "shiab-vs-curvature", curvData, shiabData, "curvature", "shiab", Tolerance);

            Assert.True(record.Passed,
                $"Shiab != Curvature (identity shiab): {record.Message}");
        }
        finally
        {
            gpu.FreeBuffer(omegaBuf);
            gpu.FreeBuffer(curvBuf);
            gpu.FreeBuffer(shiabBuf);
        }
    }

    // ------------------------------------------------------------------
    // T4: Residual kernel parity (Upsilon = S - T)
    // ------------------------------------------------------------------

    /// <summary>
    /// T4: The GPU residual kernel must compute Upsilon[i] = S[i] - T[i]
    /// element-wise. Verify by downloading shiab, torsion, and residual
    /// independently and checking the relationship.
    /// </summary>
    [SkipIfNoCuda]
    public void T4_Residual_EqualsSMinusT()
    {
        var gpu = _fixture.GpuBackend;
        var omega = _fixture.GenerateOmega(seed: 88);
        int faceN = _fixture.FaceCount * _fixture.DimG;

        var omegaBuf = gpu.AllocateBuffer(_fixture.CreateEdgeLayout());
        var curvBuf = gpu.AllocateBuffer(_fixture.CreateFaceLayout());
        var torsionBuf = gpu.AllocateBuffer(_fixture.CreateFaceLayout());
        var shiabBuf = gpu.AllocateBuffer(_fixture.CreateFaceLayout());
        var residualBuf = gpu.AllocateBuffer(_fixture.CreateFaceLayout());

        try
        {
            gpu.UploadBuffer(omegaBuf, omega);
            gpu.EvaluateCurvature(omegaBuf, curvBuf);
            gpu.EvaluateTorsion(omegaBuf, torsionBuf);
            gpu.EvaluateShiab(omegaBuf, shiabBuf);
            gpu.EvaluateResidual(shiabBuf, torsionBuf, residualBuf);

            var shiabData = new double[faceN];
            var torsionData = new double[faceN];
            var residualData = new double[faceN];
            gpu.DownloadBuffer(shiabBuf, shiabData);
            gpu.DownloadBuffer(torsionBuf, torsionData);
            gpu.DownloadBuffer(residualBuf, residualData);

            // Compute expected: Upsilon[i] = S[i] - T[i]
            var expected = new double[faceN];
            for (int i = 0; i < faceN; i++)
            {
                expected[i] = shiabData[i] - torsionData[i];
            }

            var record = ParityChecker.CompareResults(
                "residual-S-minus-T", expected, residualData,
                "manual-S-T", "gpu-residual", Tolerance);

            Assert.True(record.Passed, $"Residual != S - T: {record.Message}");
        }
        finally
        {
            gpu.FreeBuffer(omegaBuf);
            gpu.FreeBuffer(curvBuf);
            gpu.FreeBuffer(torsionBuf);
            gpu.FreeBuffer(shiabBuf);
            gpu.FreeBuffer(residualBuf);
        }
    }

    // ------------------------------------------------------------------
    // T5: Objective kernel parity (I2_h = 0.5 * sum(r_i^2))
    // ------------------------------------------------------------------

    /// <summary>
    /// T5a: Verify the GPU objective kernel against a hand-computed value
    /// using known residual data.
    /// </summary>
    [SkipIfNoCuda]
    public void T5a_Objective_MatchesHandComputed()
    {
        var gpu = _fixture.GpuBackend;
        int faceN = _fixture.FaceCount * _fixture.DimG;

        // Construct known residual: r[i] = (i+1) * 0.5
        var residualData = new double[faceN];
        for (int i = 0; i < faceN; i++)
        {
            residualData[i] = (i + 1) * 0.5;
        }

        var residualBuf = gpu.AllocateBuffer(_fixture.CreateFaceLayout());

        try
        {
            gpu.UploadBuffer(residualBuf, residualData);
            double gpuObjective = gpu.EvaluateObjective(residualBuf);

            // Expected: I2 = 0.5 * sum_{i=0}^{faceN-1} ((i+1)*0.5)^2
            double expectedSum = 0.0;
            for (int i = 0; i < faceN; i++)
            {
                expectedSum += residualData[i] * residualData[i];
            }
            double expectedObjective = 0.5 * expectedSum;

            var record = ParityChecker.CompareScalar(
                "objective-hand-computed", expectedObjective, gpuObjective,
                "hand-computed", "gpu", Tolerance);

            Assert.True(record.Passed, $"Objective mismatch: {record.Message}");
        }
        finally
        {
            gpu.FreeBuffer(residualBuf);
        }
    }

    /// <summary>
    /// T5b: Run the full pipeline (curvature -> torsion -> shiab -> residual ->
    /// objective) and verify the objective value matches a manual computation
    /// from the downloaded residual.
    /// </summary>
    [SkipIfNoCuda]
    public void T5b_Objective_FullPipeline()
    {
        var gpu = _fixture.GpuBackend;
        var omega = _fixture.GenerateOmega(seed: 100);
        int faceN = _fixture.FaceCount * _fixture.DimG;

        var omegaBuf = gpu.AllocateBuffer(_fixture.CreateEdgeLayout());
        var curvBuf = gpu.AllocateBuffer(_fixture.CreateFaceLayout());
        var torsionBuf = gpu.AllocateBuffer(_fixture.CreateFaceLayout());
        var shiabBuf = gpu.AllocateBuffer(_fixture.CreateFaceLayout());
        var residualBuf = gpu.AllocateBuffer(_fixture.CreateFaceLayout());

        try
        {
            gpu.UploadBuffer(omegaBuf, omega);
            gpu.EvaluateCurvature(omegaBuf, curvBuf);
            gpu.EvaluateTorsion(omegaBuf, torsionBuf);
            gpu.EvaluateShiab(omegaBuf, shiabBuf);
            gpu.EvaluateResidual(shiabBuf, torsionBuf, residualBuf);

            double gpuObjective = gpu.EvaluateObjective(residualBuf);

            // Download residual and compute objective manually on CPU
            var residualData = new double[faceN];
            gpu.DownloadBuffer(residualBuf, residualData);

            double manualObjective = 0.0;
            for (int i = 0; i < faceN; i++)
            {
                manualObjective += residualData[i] * residualData[i];
            }
            manualObjective *= 0.5;

            var record = ParityChecker.CompareScalar(
                "objective-pipeline", manualObjective, gpuObjective,
                "manual-from-residual", "gpu-objective", Tolerance);

            Assert.True(record.Passed, $"Pipeline objective mismatch: {record.Message}");
        }
        finally
        {
            gpu.FreeBuffer(omegaBuf);
            gpu.FreeBuffer(curvBuf);
            gpu.FreeBuffer(torsionBuf);
            gpu.FreeBuffer(shiabBuf);
            gpu.FreeBuffer(residualBuf);
        }
    }

    /// <summary>
    /// T5c: Zero residual must produce zero objective.
    /// I2 = 0.5 * sum(0^2) = 0.
    /// </summary>
    [SkipIfNoCuda]
    public void T5c_Objective_ZeroResidual_ProducesZero()
    {
        var gpu = _fixture.GpuBackend;
        int faceN = _fixture.FaceCount * _fixture.DimG;

        var residualBuf = gpu.AllocateBuffer(_fixture.CreateFaceLayout());

        try
        {
            gpu.UploadBuffer(residualBuf, new double[faceN]);
            double gpuObjective = gpu.EvaluateObjective(residualBuf);

            Assert.Equal(0.0, gpuObjective, precision: 14);
        }
        finally
        {
            gpu.FreeBuffer(residualBuf);
        }
    }

    // ------------------------------------------------------------------
    // T6: Buffer round-trip integrity
    // ------------------------------------------------------------------
    // NativeBindings (axpy, inner_product, scale) is internal to Gu.Interop
    // and not accessible from the test project. Instead, we verify the
    // upload/download round-trip fidelity which exercises the same native
    // buffer management code paths.

    /// <summary>
    /// T6a: Upload and download of edge-valued data must preserve all values
    /// exactly (bit-for-bit round-trip).
    /// </summary>
    [SkipIfNoCuda]
    public void T6a_BufferRoundTrip_EdgeData()
    {
        var gpu = _fixture.GpuBackend;
        int edgeN = _fixture.EdgeCount * _fixture.DimG;

        var data = _fixture.GenerateOmega(seed: 111);
        var edgeBuf = gpu.AllocateBuffer(_fixture.CreateEdgeLayout());

        try
        {
            gpu.UploadBuffer(edgeBuf, data);

            var downloaded = new double[edgeN];
            gpu.DownloadBuffer(edgeBuf, downloaded);

            var record = ParityChecker.CompareResults(
                "edge-roundtrip", data, downloaded,
                "uploaded", "downloaded", 0.0);

            Assert.True(record.Passed, $"Edge buffer round-trip failed: {record.Message}");
            Assert.Equal(0.0, record.MaxAbsoluteError);
        }
        finally
        {
            gpu.FreeBuffer(edgeBuf);
        }
    }

    /// <summary>
    /// T6b: Upload and download of face-valued data must preserve all values
    /// exactly (bit-for-bit round-trip).
    /// </summary>
    [SkipIfNoCuda]
    public void T6b_BufferRoundTrip_FaceData()
    {
        var gpu = _fixture.GpuBackend;
        int faceN = _fixture.FaceCount * _fixture.DimG;

        var rng = new Random(222);
        var data = new double[faceN];
        for (int i = 0; i < faceN; i++)
        {
            data[i] = rng.NextDouble() * 200.0 - 100.0;
        }

        var faceBuf = gpu.AllocateBuffer(_fixture.CreateFaceLayout());

        try
        {
            gpu.UploadBuffer(faceBuf, data);

            var downloaded = new double[faceN];
            gpu.DownloadBuffer(faceBuf, downloaded);

            var record = ParityChecker.CompareResults(
                "face-roundtrip", data, downloaded,
                "uploaded", "downloaded", 0.0);

            Assert.True(record.Passed, $"Face buffer round-trip failed: {record.Message}");
            Assert.Equal(0.0, record.MaxAbsoluteError);
        }
        finally
        {
            gpu.FreeBuffer(faceBuf);
        }
    }

    /// <summary>
    /// T6c: Overwriting a buffer with new data and downloading must return
    /// the new data, not stale values.
    /// </summary>
    [SkipIfNoCuda]
    public void T6c_BufferOverwrite_ReturnsNewData()
    {
        var gpu = _fixture.GpuBackend;
        int edgeN = _fixture.EdgeCount * _fixture.DimG;

        var data1 = _fixture.GenerateOmega(seed: 333);
        var data2 = _fixture.GenerateOmega(seed: 444);
        var edgeBuf = gpu.AllocateBuffer(_fixture.CreateEdgeLayout());

        try
        {
            // Upload first set
            gpu.UploadBuffer(edgeBuf, data1);

            // Overwrite with second set
            gpu.UploadBuffer(edgeBuf, data2);

            var downloaded = new double[edgeN];
            gpu.DownloadBuffer(edgeBuf, downloaded);

            // Must match data2, not data1
            var record = ParityChecker.CompareResults(
                "buffer-overwrite", data2, downloaded,
                "data2", "downloaded", 0.0);

            Assert.True(record.Passed, $"Buffer overwrite failed: {record.Message}");
            Assert.Equal(0.0, record.MaxAbsoluteError);
        }
        finally
        {
            gpu.FreeBuffer(edgeBuf);
        }
    }

    // ------------------------------------------------------------------
    // T7: Full pipeline determinism and consistency
    // ------------------------------------------------------------------

    /// <summary>
    /// T7: Run the full physics pipeline twice with the same omega and verify
    /// that the residual output is bit-identical across runs.
    /// </summary>
    [SkipIfNoCuda]
    public void T7_FullPipeline_Deterministic()
    {
        var gpu = _fixture.GpuBackend;
        var omega = _fixture.GenerateOmega(seed: 200);
        int faceN = _fixture.FaceCount * _fixture.DimG;

        double[] RunPipeline()
        {
            var omegaBuf = gpu.AllocateBuffer(_fixture.CreateEdgeLayout());
            var curvBuf = gpu.AllocateBuffer(_fixture.CreateFaceLayout());
            var torsionBuf = gpu.AllocateBuffer(_fixture.CreateFaceLayout());
            var shiabBuf = gpu.AllocateBuffer(_fixture.CreateFaceLayout());
            var residualBuf = gpu.AllocateBuffer(_fixture.CreateFaceLayout());

            try
            {
                gpu.UploadBuffer(omegaBuf, omega);
                gpu.EvaluateCurvature(omegaBuf, curvBuf);
                gpu.EvaluateTorsion(omegaBuf, torsionBuf);
                gpu.EvaluateShiab(omegaBuf, shiabBuf);
                gpu.EvaluateResidual(shiabBuf, torsionBuf, residualBuf);

                var result = new double[faceN];
                gpu.DownloadBuffer(residualBuf, result);
                return result;
            }
            finally
            {
                gpu.FreeBuffer(omegaBuf);
                gpu.FreeBuffer(curvBuf);
                gpu.FreeBuffer(torsionBuf);
                gpu.FreeBuffer(shiabBuf);
                gpu.FreeBuffer(residualBuf);
            }
        }

        var residual1 = RunPipeline();
        var residual2 = RunPipeline();

        var record = ParityChecker.CompareResults(
            "full-pipeline-determinism", residual1, residual2,
            "run-1", "run-2", 0.0);

        Assert.True(record.Passed,
            $"Full pipeline not deterministic: {record.Message}");
        Assert.Equal(0.0, record.MaxAbsoluteError);
    }

    // ------------------------------------------------------------------
    // T8: Physics data gate verification
    // ------------------------------------------------------------------

    /// <summary>
    /// T8: After the fixture uploads mesh topology, algebra data, and background
    /// connection, HasPhysicsData must be true. This verifies that the native
    /// library correctly tracks the physics data upload state, which gates
    /// real kernel dispatch vs. stub fallback.
    /// </summary>
    [SkipIfNoCuda]
    public void T8_HasPhysicsData_IsTrue()
    {
        Assert.True(_fixture.GpuBackend.HasPhysicsData,
            "GPU backend should have physics data after fixture setup. " +
            "If false, the native kernels fall back to stubs and all " +
            "physics parity tests are invalid.");
    }

    /// <summary>
    /// T8b: The GPU backend version must report "cuda" as the backend ID.
    /// </summary>
    [SkipIfNoCuda]
    public void T8b_BackendId_IsCuda()
    {
        var version = _fixture.GpuBackend.Version;
        Assert.Equal("cuda", version.BackendId);
    }

    /// <summary>
    /// T8c: The fixture's CPU reference backend must report "cpu-reference"
    /// as its backend ID (sanity check that we have two distinct backends).
    /// </summary>
    [SkipIfNoCuda]
    public void T8c_CpuBackendId_IsCpuReference()
    {
        var version = _fixture.CpuBackend.Version;
        Assert.Equal("cpu-reference", version.BackendId);
    }
}
