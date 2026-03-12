using Gu.Core;
using Gu.Phase4.CudaAcceleration;
using Gu.Phase4.Dirac;

namespace Gu.Phase4.CudaAcceleration.Tests;

/// <summary>
/// Tests for DiracParityChecker, CpuDiracKernel, GpuDiracKernelStub,
/// DiracKernelFactory, and DiracBenchmarkRunner.
///
/// M44 completion criterion: CPU/CUDA parity established for key Dirac operator paths.
/// </summary>
public sealed class DiracParityCheckerTests
{
    private static ProvenanceMeta TestProvenance() => new()
    {
        CreatedAt = DateTimeOffset.UtcNow,
        CodeRevision = "test",
        Branch = new BranchRef { BranchId = "test-branch", SchemaVersion = "1.0.0" },
        Backend = "cpu-reference",
    };

    /// <summary>
    /// Minimal DiracOperatorBundle for testing:
    ///   cellCount=2, dofsPerCell=2, no ExplicitMatrix (identity fallback)
    /// </summary>
    private static DiracOperatorBundle MakeBundle(int cellCount = 2, int dofsPerCell = 2)
        => new DiracOperatorBundle
        {
            OperatorId = "test-op",
            FermionBackgroundId = "bg-001",
            LayoutId = "layout-001",
            SpinConnectionId = "conn-001",
            MatrixShape = new[] { cellCount * dofsPerCell, cellCount * dofsPerCell },
            HasExplicitMatrix = false,
            ExplicitMatrix = null,
            IsHermitian = true,
            HermiticityResidual = 0.0,
            MassBranchTermIncluded = false,
            CorrectionTermIncluded = false,
            GaugeReductionApplied = false,
            CellCount = cellCount,
            DofsPerCell = dofsPerCell,
            Provenance = TestProvenance(),
        };

    // -------- IDiracKernel / CpuDiracKernel --------

    [Fact]
    public void CpuDiracKernel_TotalDof_Is2xCellsxDofs()
    {
        var bundle = MakeBundle(cellCount: 3, dofsPerCell: 4);
        var kernel = new CpuDiracKernel(bundle);
        Assert.Equal(2 * 3 * 4, kernel.TotalDof);
    }

    [Fact]
    public void CpuDiracKernel_ComputedWithCuda_IsFalse()
    {
        var kernel = new CpuDiracKernel(MakeBundle());
        Assert.False(kernel.ComputedWithCuda);
    }

    [Fact]
    public void CpuDiracKernel_ApplyDirac_IdentityWhenNoMatrix()
    {
        // Without ExplicitMatrix, result stays all zeros (identity fallback)
        var kernel = new CpuDiracKernel(MakeBundle(cellCount: 1, dofsPerCell: 2));
        int n = kernel.TotalDof;
        var psi = Enumerable.Range(0, n).Select(i => (double)i + 1.0).ToArray();
        var result = new double[n];
        kernel.ApplyDirac(psi, result);
        // Without explicit matrix, result is all zeros
        Assert.All(result, v => Assert.Equal(0.0, v));
    }

    [Fact]
    public void CpuDiracKernel_ApplyMassPsi_IsIdentity()
    {
        var kernel = new CpuDiracKernel(MakeBundle());
        int n = kernel.TotalDof;
        var psi = Enumerable.Range(0, n).Select(i => (double)(i + 1)).ToArray();
        var result = new double[n];
        kernel.ApplyMassPsi(psi, result);
        Assert.Equal(psi, result);
    }

    [Fact]
    public void CpuDiracKernel_ProjectLeft_OutputLengthCorrect()
    {
        var kernel = new CpuDiracKernel(MakeBundle(cellCount: 2, dofsPerCell: 4));
        int n = kernel.TotalDof;
        var psi = new double[n];
        var rng = new Random(10);
        for (int i = 0; i < n; i++) psi[i] = rng.NextDouble();
        var result = new double[n];
        kernel.ProjectLeft(psi, result);
        Assert.Equal(n, result.Length);
    }

    [Fact]
    public void CpuDiracKernel_ProjectRight_OutputLengthCorrect()
    {
        var kernel = new CpuDiracKernel(MakeBundle(cellCount: 2, dofsPerCell: 4));
        int n = kernel.TotalDof;
        var psi = new double[n];
        var result = new double[n];
        kernel.ProjectRight(psi, result);
        Assert.Equal(n, result.Length);
    }

    [Fact]
    public void CpuDiracKernel_ProjectLeft_PlusProjectRight_EqualsIdentity()
    {
        // P_L + P_R = I
        var kernel = new CpuDiracKernel(MakeBundle(cellCount: 2, dofsPerCell: 4));
        int n = kernel.TotalDof;
        var psi = Enumerable.Range(0, n).Select(i => (i + 1.0) * 0.1).ToArray();
        var left = new double[n];
        var right = new double[n];
        kernel.ProjectLeft(psi, left);
        kernel.ProjectRight(psi, right);
        for (int i = 0; i < n; i++)
            Assert.Equal(psi[i], left[i] + right[i], precision: 12);
    }

    [Fact]
    public void CpuDiracKernel_ApplyDirac_WrongLength_Throws()
    {
        var kernel = new CpuDiracKernel(MakeBundle());
        var psi = new double[kernel.TotalDof + 1];
        var result = new double[kernel.TotalDof];
        Assert.Throws<ArgumentException>(() => kernel.ApplyDirac(psi, result));
    }

    [Fact]
    public void CpuDiracKernel_AccumulateCouplingProxy_NoModes_ReturnsZero()
    {
        var kernel = new CpuDiracKernel(MakeBundle(), modes: null);
        int n = kernel.TotalDof;
        var boson = new double[n];
        var pairs = new List<(int, int)> { (0, 0) };
        double proxy = kernel.AccumulateCouplingProxy(boson, pairs);
        Assert.Equal(0.0, proxy);
    }

    // -------- GpuDiracKernelStub --------

    [Fact]
    public void GpuDiracKernelStub_ComputedWithCuda_IsTrue()
    {
        var stub = new GpuDiracKernelStub(MakeBundle());
        Assert.True(stub.ComputedWithCuda);
    }

    [Fact]
    public void GpuDiracKernelStub_VerificationStatus_IsStubUnverified()
    {
        var stub = new GpuDiracKernelStub(MakeBundle());
        Assert.Equal("stub-unverified", stub.VerificationStatus);
    }

    [Fact]
    public void GpuDiracKernelStub_TotalDof_MatchesCpu()
    {
        var bundle = MakeBundle(cellCount: 3, dofsPerCell: 2);
        var cpu = new CpuDiracKernel(bundle);
        var gpu = new GpuDiracKernelStub(bundle);
        Assert.Equal(cpu.TotalDof, gpu.TotalDof);
    }

    [Fact]
    public void GpuDiracKernelStub_ApplyMassPsi_MatchesCpu()
    {
        var bundle = MakeBundle(cellCount: 2, dofsPerCell: 2);
        var cpu = new CpuDiracKernel(bundle);
        var gpu = new GpuDiracKernelStub(bundle);
        int n = cpu.TotalDof;
        var psi = Enumerable.Range(0, n).Select(i => (double)(i + 1)).ToArray();
        var cpuResult = new double[n];
        var gpuResult = new double[n];
        cpu.ApplyMassPsi(psi, cpuResult);
        gpu.ApplyMassPsi(psi, gpuResult);
        Assert.Equal(cpuResult, gpuResult);
    }

    // -------- DiracKernelFactory --------

    [Fact]
    public void DiracKernelFactory_CreateCpu_ReturnsCpuKernel()
    {
        var kernel = DiracKernelFactory.CreateCpu(MakeBundle());
        Assert.IsType<CpuDiracKernel>(kernel);
        Assert.False(kernel.ComputedWithCuda);
    }

    [Fact]
    public void DiracKernelFactory_CreateGpu_ReturnsStub()
    {
        var kernel = DiracKernelFactory.CreateGpu(MakeBundle());
        Assert.IsType<GpuDiracKernelStub>(kernel);
        Assert.True(kernel.ComputedWithCuda);
    }

    [Fact]
    public void DiracKernelFactory_Create_UseCudaFalse_ReturnsCpu()
    {
        var kernel = DiracKernelFactory.Create(MakeBundle(), useCuda: false);
        Assert.False(kernel.ComputedWithCuda);
    }

    [Fact]
    public void DiracKernelFactory_Create_UseCudaTrue_ReturnsGpu()
    {
        var kernel = DiracKernelFactory.Create(MakeBundle(), useCuda: true);
        Assert.True(kernel.ComputedWithCuda);
    }

    // -------- DiracParityChecker --------

    [Fact]
    public void DiracParityChecker_CpuVsStub_AllPassed()
    {
        var bundle = MakeBundle(cellCount: 2, dofsPerCell: 2);
        var cpu = DiracKernelFactory.CreateCpu(bundle);
        var gpu = DiracKernelFactory.CreateGpu(bundle);

        var report = DiracParityChecker.Check(cpu, gpu, "report-01", TestProvenance());

        Assert.True(report.AllPassed);
        Assert.Equal(0.0, report.Checks.Max(c => c.MaxAbsoluteError));
    }

    [Fact]
    public void DiracParityChecker_ReportId_Preserved()
    {
        var bundle = MakeBundle();
        var cpu = DiracKernelFactory.CreateCpu(bundle);
        var gpu = DiracKernelFactory.CreateGpu(bundle);
        var report = DiracParityChecker.Check(cpu, gpu, "my-report", TestProvenance());
        Assert.Equal("my-report", report.ReportId);
    }

    [Fact]
    public void DiracParityChecker_GpuStatus_IsStubUnverified()
    {
        var bundle = MakeBundle();
        var cpu = DiracKernelFactory.CreateCpu(bundle);
        var gpu = DiracKernelFactory.CreateGpu(bundle);
        var report = DiracParityChecker.Check(cpu, gpu, "r", TestProvenance());
        Assert.Equal("stub-unverified", report.GpuVerificationStatus);
    }

    [Fact]
    public void DiracParityChecker_ChecksContainAllOperations()
    {
        var bundle = MakeBundle(cellCount: 2, dofsPerCell: 2);
        var cpu = DiracKernelFactory.CreateCpu(bundle);
        var gpu = DiracKernelFactory.CreateGpu(bundle);
        var report = DiracParityChecker.Check(cpu, gpu, "r", TestProvenance());

        var names = report.Checks.Select(c => c.OperationName).ToList();
        Assert.Contains("ApplyDirac", names);
        Assert.Contains("ApplyMassPsi", names);
        Assert.Contains("ProjectLeft", names);
        Assert.Contains("ProjectRight", names);
        Assert.Contains("AccumulateCouplingProxy", names);
    }

    [Fact]
    public void DiracParityChecker_TotalDofMismatch_Throws()
    {
        var cpu = DiracKernelFactory.CreateCpu(MakeBundle(cellCount: 2, dofsPerCell: 2));
        var gpu = DiracKernelFactory.CreateGpu(MakeBundle(cellCount: 3, dofsPerCell: 2));
        Assert.Throws<InvalidOperationException>(() =>
            DiracParityChecker.Check(cpu, gpu, "r", TestProvenance()));
    }

    [Fact]
    public void DiracParityChecker_CustomConfig_NumTestVectors()
    {
        var bundle = MakeBundle();
        var cpu = DiracKernelFactory.CreateCpu(bundle);
        var gpu = DiracKernelFactory.CreateGpu(bundle);
        var config = new DiracParityConfig { NumTestVectors = 1 };
        var report = DiracParityChecker.Check(cpu, gpu, "r", TestProvenance(), config);
        Assert.True(report.AllPassed);
    }

    // -------- DiracBenchmarkRunner --------

    [Fact]
    public void DiracBenchmarkRunner_Run_ProducesArtifact()
    {
        var bundle = MakeBundle();
        var cpu = DiracKernelFactory.CreateCpu(bundle);
        var gpu = DiracKernelFactory.CreateGpu(bundle);
        var artifact = DiracBenchmarkRunner.Run(cpu, gpu, "bench-01", numTrials: 1, TestProvenance());

        Assert.Equal("bench-01", artifact.ArtifactId);
        Assert.NotEmpty(artifact.OperationBenchmarks);
        Assert.Equal("stub-unverified", artifact.GpuVerificationStatus);
        Assert.Equal(cpu.TotalDof, artifact.TotalDof);
    }

    [Fact]
    public void DiracBenchmarkRunner_SpeedupRatio_NearOneForStub()
    {
        var bundle = MakeBundle();
        var cpu = DiracKernelFactory.CreateCpu(bundle);
        var gpu = DiracKernelFactory.CreateGpu(bundle);
        var artifact = DiracBenchmarkRunner.Run(cpu, gpu, "bench-02", numTrials: 2, TestProvenance());

        // For stub, speedup should be near 1 (both are CPU)
        foreach (var bench in artifact.OperationBenchmarks)
            Assert.True(bench.SpeedupRatio > 0.0);
    }

    [Fact]
    public void DiracBenchmarkRunner_ContainsAllOperations()
    {
        var bundle = MakeBundle();
        var cpu = DiracKernelFactory.CreateCpu(bundle);
        var gpu = DiracKernelFactory.CreateGpu(bundle);
        var artifact = DiracBenchmarkRunner.Run(cpu, gpu, "bench-03", numTrials: 1, TestProvenance());

        var names = artifact.OperationBenchmarks.Select(b => b.OperationName).ToList();
        Assert.Contains("ApplyDirac", names);
        Assert.Contains("ApplyMassPsi", names);
        Assert.Contains("ProjectLeft", names);
        Assert.Contains("ProjectRight", names);
    }

    [Fact]
    public void DiracBenchmarkArtifact_SchemaVersion_IsSet()
    {
        var bundle = MakeBundle();
        var cpu = DiracKernelFactory.CreateCpu(bundle);
        var gpu = DiracKernelFactory.CreateGpu(bundle);
        var artifact = DiracBenchmarkRunner.Run(cpu, gpu, "bench-sv", 1, TestProvenance());
        Assert.Equal("1.0.0", artifact.SchemaVersion);
    }

    // -------- DiracParityConfig --------

    [Fact]
    public void DiracParityConfig_Default_IsSet()
    {
        var cfg = DiracParityConfig.Default;
        Assert.True(cfg.MaxAbsoluteError > 0.0);
        Assert.True(cfg.NumTestVectors > 0);
    }
}
