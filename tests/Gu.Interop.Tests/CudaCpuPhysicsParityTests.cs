using Gu.Interop;

namespace Gu.Interop.Tests;

/// <summary>
/// True CPU-vs-GPU physics parity tests (GAP-3).
/// These compare the CpuReferenceBackend (with real physics from CurvatureAssembler,
/// AugmentedTorsionCpu, IdentityShiabCpu logic) against the CudaNativeBackend
/// for non-trivial random omega with the same mesh topology and Lie algebra.
///
/// The CudaTestFixture uploads identical physics data (mesh, algebra, A0) to both backends.
/// Buffer sizes: omega = 8*3 = 24 (edge-valued), curvature/torsion/shiab/residual = 4*3 = 12 (face-valued).
/// </summary>
[Collection("GPU")]
[Trait("Category", "GPU")]
public class CudaCpuPhysicsParityTests
{
    private readonly CudaTestFixture _fixture;
    private const double Tolerance = 1e-12;

    public CudaCpuPhysicsParityTests(CudaTestFixture fixture) => _fixture = fixture;

    // ------------------------------------------------------------------
    // Curvature parity: CPU F == GPU F
    // ------------------------------------------------------------------

    [SkipIfNoCuda]
    public void Curvature_CpuVsGpu_Parity()
    {
        var gpu = _fixture.GpuBackend;
        var cpu = _fixture.CpuBackend;
        var omega = _fixture.GenerateOmega(seed: 42);
        int faceN = _fixture.FaceCount * _fixture.DimG;

        var gpuOmegaBuf = gpu.AllocateBuffer(_fixture.CreateEdgeLayout());
        var gpuCurvBuf = gpu.AllocateBuffer(_fixture.CreateFaceLayout());

        var cpuOmegaBuf = cpu.AllocateBuffer(_fixture.CreateEdgeLayout());
        var cpuCurvBuf = cpu.AllocateBuffer(_fixture.CreateFaceLayout());

        try
        {
            gpu.UploadBuffer(gpuOmegaBuf, omega);
            gpu.EvaluateCurvature(gpuOmegaBuf, gpuCurvBuf);

            cpu.UploadBuffer(cpuOmegaBuf, omega);
            cpu.EvaluateCurvature(cpuOmegaBuf, cpuCurvBuf);

            var gpuResult = new double[faceN];
            var cpuResult = new double[faceN];
            gpu.DownloadBuffer(gpuCurvBuf, gpuResult);
            cpu.DownloadBuffer(cpuCurvBuf, cpuResult);

            var record = ParityChecker.CompareResults(
                "curvature-cpu-vs-gpu", cpuResult, gpuResult,
                "cpu-reference", "cuda", Tolerance);

            Assert.True(record.Passed,
                $"Curvature CPU-vs-GPU parity failed: {record.Message}");
        }
        finally
        {
            gpu.FreeBuffer(gpuOmegaBuf);
            gpu.FreeBuffer(gpuCurvBuf);
            cpu.FreeBuffer(cpuOmegaBuf);
            cpu.FreeBuffer(cpuCurvBuf);
        }
    }

    [SkipIfNoCuda]
    public void Curvature_CpuVsGpu_MultipleSeeds()
    {
        var gpu = _fixture.GpuBackend;
        var cpu = _fixture.CpuBackend;
        int faceN = _fixture.FaceCount * _fixture.DimG;

        foreach (int seed in new[] { 1, 17, 42, 99, 256 })
        {
            var omega = _fixture.GenerateOmega(seed);

            var gpuOmegaBuf = gpu.AllocateBuffer(_fixture.CreateEdgeLayout());
            var gpuCurvBuf = gpu.AllocateBuffer(_fixture.CreateFaceLayout());
            var cpuOmegaBuf = cpu.AllocateBuffer(_fixture.CreateEdgeLayout());
            var cpuCurvBuf = cpu.AllocateBuffer(_fixture.CreateFaceLayout());

            try
            {
                gpu.UploadBuffer(gpuOmegaBuf, omega);
                gpu.EvaluateCurvature(gpuOmegaBuf, gpuCurvBuf);

                cpu.UploadBuffer(cpuOmegaBuf, omega);
                cpu.EvaluateCurvature(cpuOmegaBuf, cpuCurvBuf);

                var gpuResult = new double[faceN];
                var cpuResult = new double[faceN];
                gpu.DownloadBuffer(gpuCurvBuf, gpuResult);
                cpu.DownloadBuffer(cpuCurvBuf, cpuResult);

                var record = ParityChecker.CompareResults(
                    $"curvature-parity-seed{seed}", cpuResult, gpuResult,
                    "cpu-reference", "cuda", Tolerance);

                Assert.True(record.Passed,
                    $"Curvature parity failed for seed {seed}: {record.Message}");
            }
            finally
            {
                gpu.FreeBuffer(gpuOmegaBuf);
                gpu.FreeBuffer(gpuCurvBuf);
                cpu.FreeBuffer(cpuOmegaBuf);
                cpu.FreeBuffer(cpuCurvBuf);
            }
        }
    }

    // ------------------------------------------------------------------
    // Torsion parity: CPU T == GPU T (with zero A0)
    // ------------------------------------------------------------------

    [SkipIfNoCuda]
    public void Torsion_CpuVsGpu_Parity()
    {
        var gpu = _fixture.GpuBackend;
        var cpu = _fixture.CpuBackend;
        var omega = _fixture.GenerateOmega(seed: 55);
        int faceN = _fixture.FaceCount * _fixture.DimG;

        var gpuOmegaBuf = gpu.AllocateBuffer(_fixture.CreateEdgeLayout());
        var gpuTorsionBuf = gpu.AllocateBuffer(_fixture.CreateFaceLayout());

        var cpuOmegaBuf = cpu.AllocateBuffer(_fixture.CreateEdgeLayout());
        var cpuTorsionBuf = cpu.AllocateBuffer(_fixture.CreateFaceLayout());

        try
        {
            gpu.UploadBuffer(gpuOmegaBuf, omega);
            gpu.EvaluateTorsion(gpuOmegaBuf, gpuTorsionBuf);

            cpu.UploadBuffer(cpuOmegaBuf, omega);
            cpu.EvaluateTorsion(cpuOmegaBuf, cpuTorsionBuf);

            var gpuResult = new double[faceN];
            var cpuResult = new double[faceN];
            gpu.DownloadBuffer(gpuTorsionBuf, gpuResult);
            cpu.DownloadBuffer(cpuTorsionBuf, cpuResult);

            var record = ParityChecker.CompareResults(
                "torsion-cpu-vs-gpu", cpuResult, gpuResult,
                "cpu-reference", "cuda", Tolerance);

            Assert.True(record.Passed,
                $"Torsion CPU-vs-GPU parity failed: {record.Message}");
        }
        finally
        {
            gpu.FreeBuffer(gpuOmegaBuf);
            gpu.FreeBuffer(gpuTorsionBuf);
            cpu.FreeBuffer(cpuOmegaBuf);
            cpu.FreeBuffer(cpuTorsionBuf);
        }
    }

    // ------------------------------------------------------------------
    // Shiab parity: CPU S == GPU S (identity Shiab, S = F)
    // ------------------------------------------------------------------

    [SkipIfNoCuda]
    public void Shiab_CpuVsGpu_Parity()
    {
        var gpu = _fixture.GpuBackend;
        var cpu = _fixture.CpuBackend;
        var omega = _fixture.GenerateOmega(seed: 77);
        int faceN = _fixture.FaceCount * _fixture.DimG;

        var gpuOmegaBuf = gpu.AllocateBuffer(_fixture.CreateEdgeLayout());
        var gpuShiabBuf = gpu.AllocateBuffer(_fixture.CreateFaceLayout());

        var cpuOmegaBuf = cpu.AllocateBuffer(_fixture.CreateEdgeLayout());
        var cpuShiabBuf = cpu.AllocateBuffer(_fixture.CreateFaceLayout());

        try
        {
            gpu.UploadBuffer(gpuOmegaBuf, omega);
            gpu.EvaluateShiab(gpuOmegaBuf, gpuShiabBuf);

            cpu.UploadBuffer(cpuOmegaBuf, omega);
            cpu.EvaluateShiab(cpuOmegaBuf, cpuShiabBuf);

            var gpuResult = new double[faceN];
            var cpuResult = new double[faceN];
            gpu.DownloadBuffer(gpuShiabBuf, gpuResult);
            cpu.DownloadBuffer(cpuShiabBuf, cpuResult);

            var record = ParityChecker.CompareResults(
                "shiab-cpu-vs-gpu", cpuResult, gpuResult,
                "cpu-reference", "cuda", Tolerance);

            Assert.True(record.Passed,
                $"Shiab CPU-vs-GPU parity failed: {record.Message}");
        }
        finally
        {
            gpu.FreeBuffer(gpuOmegaBuf);
            gpu.FreeBuffer(gpuShiabBuf);
            cpu.FreeBuffer(cpuOmegaBuf);
            cpu.FreeBuffer(cpuShiabBuf);
        }
    }

    // ------------------------------------------------------------------
    // Full pipeline residual parity: CPU Upsilon == GPU Upsilon
    // ------------------------------------------------------------------

    [SkipIfNoCuda]
    public void FullPipeline_Residual_CpuVsGpu_Parity()
    {
        var gpu = _fixture.GpuBackend;
        var cpu = _fixture.CpuBackend;
        var omega = _fixture.GenerateOmega(seed: 100);
        int faceN = _fixture.FaceCount * _fixture.DimG;

        // GPU pipeline
        var gpuOmegaBuf = gpu.AllocateBuffer(_fixture.CreateEdgeLayout());
        var gpuCurvBuf = gpu.AllocateBuffer(_fixture.CreateFaceLayout());
        var gpuTorsionBuf = gpu.AllocateBuffer(_fixture.CreateFaceLayout());
        var gpuShiabBuf = gpu.AllocateBuffer(_fixture.CreateFaceLayout());
        var gpuResidualBuf = gpu.AllocateBuffer(_fixture.CreateFaceLayout());

        // CPU pipeline
        var cpuOmegaBuf = cpu.AllocateBuffer(_fixture.CreateEdgeLayout());
        var cpuCurvBuf = cpu.AllocateBuffer(_fixture.CreateFaceLayout());
        var cpuTorsionBuf = cpu.AllocateBuffer(_fixture.CreateFaceLayout());
        var cpuShiabBuf = cpu.AllocateBuffer(_fixture.CreateFaceLayout());
        var cpuResidualBuf = cpu.AllocateBuffer(_fixture.CreateFaceLayout());

        try
        {
            // Run GPU pipeline
            gpu.UploadBuffer(gpuOmegaBuf, omega);
            gpu.EvaluateCurvature(gpuOmegaBuf, gpuCurvBuf);
            gpu.EvaluateTorsion(gpuOmegaBuf, gpuTorsionBuf);
            gpu.EvaluateShiab(gpuOmegaBuf, gpuShiabBuf);
            gpu.EvaluateResidual(gpuShiabBuf, gpuTorsionBuf, gpuResidualBuf);

            // Run CPU pipeline
            cpu.UploadBuffer(cpuOmegaBuf, omega);
            cpu.EvaluateCurvature(cpuOmegaBuf, cpuCurvBuf);
            cpu.EvaluateTorsion(cpuOmegaBuf, cpuTorsionBuf);
            cpu.EvaluateShiab(cpuOmegaBuf, cpuShiabBuf);
            cpu.EvaluateResidual(cpuShiabBuf, cpuTorsionBuf, cpuResidualBuf);

            var gpuResidual = new double[faceN];
            var cpuResidual = new double[faceN];
            gpu.DownloadBuffer(gpuResidualBuf, gpuResidual);
            cpu.DownloadBuffer(cpuResidualBuf, cpuResidual);

            var record = ParityChecker.CompareResults(
                "residual-cpu-vs-gpu", cpuResidual, gpuResidual,
                "cpu-reference", "cuda", Tolerance);

            Assert.True(record.Passed,
                $"Full pipeline residual CPU-vs-GPU parity failed: {record.Message}");
        }
        finally
        {
            gpu.FreeBuffer(gpuOmegaBuf);
            gpu.FreeBuffer(gpuCurvBuf);
            gpu.FreeBuffer(gpuTorsionBuf);
            gpu.FreeBuffer(gpuShiabBuf);
            gpu.FreeBuffer(gpuResidualBuf);
            cpu.FreeBuffer(cpuOmegaBuf);
            cpu.FreeBuffer(cpuCurvBuf);
            cpu.FreeBuffer(cpuTorsionBuf);
            cpu.FreeBuffer(cpuShiabBuf);
            cpu.FreeBuffer(cpuResidualBuf);
        }
    }

    // ------------------------------------------------------------------
    // Objective parity: CPU I2 == GPU I2
    // ------------------------------------------------------------------

    [SkipIfNoCuda]
    public void Objective_CpuVsGpu_Parity()
    {
        var gpu = _fixture.GpuBackend;
        var cpu = _fixture.CpuBackend;
        var omega = _fixture.GenerateOmega(seed: 200);
        int faceN = _fixture.FaceCount * _fixture.DimG;

        // GPU pipeline
        var gpuOmegaBuf = gpu.AllocateBuffer(_fixture.CreateEdgeLayout());
        var gpuCurvBuf = gpu.AllocateBuffer(_fixture.CreateFaceLayout());
        var gpuTorsionBuf = gpu.AllocateBuffer(_fixture.CreateFaceLayout());
        var gpuShiabBuf = gpu.AllocateBuffer(_fixture.CreateFaceLayout());
        var gpuResidualBuf = gpu.AllocateBuffer(_fixture.CreateFaceLayout());

        // CPU pipeline
        var cpuOmegaBuf = cpu.AllocateBuffer(_fixture.CreateEdgeLayout());
        var cpuCurvBuf = cpu.AllocateBuffer(_fixture.CreateFaceLayout());
        var cpuTorsionBuf = cpu.AllocateBuffer(_fixture.CreateFaceLayout());
        var cpuShiabBuf = cpu.AllocateBuffer(_fixture.CreateFaceLayout());
        var cpuResidualBuf = cpu.AllocateBuffer(_fixture.CreateFaceLayout());

        try
        {
            // Run GPU pipeline
            gpu.UploadBuffer(gpuOmegaBuf, omega);
            gpu.EvaluateCurvature(gpuOmegaBuf, gpuCurvBuf);
            gpu.EvaluateTorsion(gpuOmegaBuf, gpuTorsionBuf);
            gpu.EvaluateShiab(gpuOmegaBuf, gpuShiabBuf);
            gpu.EvaluateResidual(gpuShiabBuf, gpuTorsionBuf, gpuResidualBuf);
            double gpuObjective = gpu.EvaluateObjective(gpuResidualBuf);

            // Run CPU pipeline
            cpu.UploadBuffer(cpuOmegaBuf, omega);
            cpu.EvaluateCurvature(cpuOmegaBuf, cpuCurvBuf);
            cpu.EvaluateTorsion(cpuOmegaBuf, cpuTorsionBuf);
            cpu.EvaluateShiab(cpuOmegaBuf, cpuShiabBuf);
            cpu.EvaluateResidual(cpuShiabBuf, cpuTorsionBuf, cpuResidualBuf);
            double cpuObjective = cpu.EvaluateObjective(cpuResidualBuf);

            var record = ParityChecker.CompareScalar(
                "objective-cpu-vs-gpu", cpuObjective, gpuObjective,
                "cpu-reference", "cuda", Tolerance);

            Assert.True(record.Passed,
                $"Objective CPU-vs-GPU parity failed: {record.Message}");
        }
        finally
        {
            gpu.FreeBuffer(gpuOmegaBuf);
            gpu.FreeBuffer(gpuCurvBuf);
            gpu.FreeBuffer(gpuTorsionBuf);
            gpu.FreeBuffer(gpuShiabBuf);
            gpu.FreeBuffer(gpuResidualBuf);
            cpu.FreeBuffer(cpuOmegaBuf);
            cpu.FreeBuffer(cpuCurvBuf);
            cpu.FreeBuffer(cpuTorsionBuf);
            cpu.FreeBuffer(cpuShiabBuf);
            cpu.FreeBuffer(cpuResidualBuf);
        }
    }
}
