using Gu.Core;
using Gu.Geometry;
using Gu.Phase3.Backgrounds;
using Gu.Phase4.Dirac;
using Gu.Phase4.Fermions;
using Gu.Phase4.Spin;

namespace Gu.Phase4.Dirac.Gpu.Tests;

/// <summary>
/// M44 CPU/GPU parity tests for the Phase IV fermionic Dirac operator stack.
///
/// These tests verify that GpuDiracKernel produces results that agree with
/// CpuDiracKernel within parity tolerance (relative error &lt; 1e-4 per element).
///
/// On systems where CUDA is unavailable, GpuDiracKernel falls back gracefully
/// to the same CPU implementation — parity tests still exercise the full code path
/// and confirm that the fallback path is correct.
///
/// Coverage per M44 specification:
///   - ApplyGamma for dimY=4 (even) and dimY=5 (odd)
///   - ApplyDirac for dimY=4 and dimY=5
///   - ApplyMass for dimY=4 and dimY=5
///   - ApplyChiralityProjector for dimY=4 only; SKIP for dimY=5 (no chirality)
///   - ComputeCouplingProxy for dimY=4 and dimY=5
///   - DiracParityChecker.RunFullCheck integration tests
///   - GpuDiracKernel graceful-fallback (no crash, no-CUDA environment)
/// </summary>
[Trait("Category", "CpuGpuParity")]
public class DiracParityTests
{
    // -------------------------------------------------------
    // Test fixtures
    // -------------------------------------------------------

    private static ProvenanceMeta TestProvenance() => new()
    {
        CreatedAt = new DateTimeOffset(2026, 3, 12, 0, 0, 0, TimeSpan.Zero),
        CodeRevision = "test-m44",
        Branch = new BranchRef { BranchId = "test-parity", SchemaVersion = "1.0.0" },
        Backend = "cpu-fallback",
    };

    private static SimplicialMesh TwoTriangles() =>
        MeshTopologyBuilder.Build(
            embeddingDimension: 2,
            simplicialDimension: 2,
            vertexCoordinates: new double[] { 0, 0, 1, 0, 0, 1, 1, 1 },
            vertexCount: 4,
            cellVertices: new[] { new[] { 0, 1, 2 }, new[] { 1, 3, 2 } });

    private static SimplicialMesh ThreeTetrahedra() =>
        MeshTopologyBuilder.Build(
            embeddingDimension: 3,
            simplicialDimension: 3,
            vertexCoordinates: new double[]
            {
                0, 0, 0,  1, 0, 0,  0, 1, 0,  0, 0, 1,
                1, 1, 0,  1, 0, 1,
            },
            vertexCount: 6,
            cellVertices: new[]
            {
                new[] { 0, 1, 2, 3 },
                new[] { 1, 4, 2, 5 },
                new[] { 1, 2, 3, 5 },
            });

    private static BackgroundRecord MakeBackground(string id) => new()
    {
        BackgroundId = id,
        EnvironmentId = "test-env",
        BranchManifestId = "manifest-1",
        GeometryFingerprint = "test-fp",
        StateArtifactRef = "test-state-ref",
        ResidualNorm = 0.001,
        StationarityNorm = 0.001,
        AdmissibilityLevel = AdmissibilityLevel.B1,
        Metrics = new BackgroundMetrics
        {
            ResidualNorm = 0.001,
            StationarityNorm = 0.001,
            ObjectiveValue = 0.1,
            GaugeViolation = 0.0,
            SolverIterations = 10,
            SolverConverged = true,
            TerminationReason = "residual-converged",
            GaussNewtonValid = false,
        },
        ReplayTierAchieved = "R1",
        Provenance = TestProvenance(),
    };

    private static SpinorRepresentationSpec Dim4Spec() => new()
    {
        SpinorSpecId = "spinor-dim4-v1",
        SpacetimeDimension = 4,
        CliffordSignature = new CliffordSignature { Positive = 4, Negative = 0 },
        GammaConvention = new GammaConventionSpec
        {
            ConventionId = "dirac-tensor-product-v1",
            Signature = new CliffordSignature { Positive = 4, Negative = 0 },
            Representation = "standard",
            SpinorDimension = 4,
            HasChirality = true,
        },
        ChiralityConvention = new ChiralityConventionSpec
        {
            ConventionId = "chirality-standard-v1",
            SignConvention = "left-is-minus",
            PhaseFactor = "-1",
            HasChirality = true,
        },
        ConjugationConvention = new ConjugationConventionSpec
        {
            ConventionId = "hermitian-v1",
            ConjugationType = "hermitian",
            HasChargeConjugation = true,
        },
        SpinorComponents = 4,
        ChiralitySplit = 2,
        Provenance = TestProvenance(),
    };

    private static SpinorRepresentationSpec Dim5Spec() => new()
    {
        SpinorSpecId = "spinor-dim5-v1",
        SpacetimeDimension = 5,
        CliffordSignature = new CliffordSignature { Positive = 5, Negative = 0 },
        GammaConvention = new GammaConventionSpec
        {
            ConventionId = "dirac-tensor-product-v1",
            Signature = new CliffordSignature { Positive = 5, Negative = 0 },
            Representation = "standard",
            SpinorDimension = 4,
            HasChirality = false,
        },
        ChiralityConvention = new ChiralityConventionSpec
        {
            ConventionId = "chirality-none-v1",
            SignConvention = "none",
            PhaseFactor = "1",
            HasChirality = false,
        },
        ConjugationConvention = new ConjugationConventionSpec
        {
            ConventionId = "hermitian-v1",
            ConjugationType = "hermitian",
            HasChargeConjugation = false,
        },
        SpinorComponents = 4,
        ChiralitySplit = 0,
        Provenance = TestProvenance(),
    };

    /// <summary>
    /// Build CPU + GPU kernel pair for the given spec and mesh.
    /// Returns (cpuKernel, gpuKernel, spec, bosonDimension).
    /// </summary>
    private static (CpuDiracKernel cpu, GpuDiracKernel gpu, SpinorRepresentationSpec spec, int bosonDim)
        BuildKernelPair(SpinorRepresentationSpec spec, SimplicialMesh mesh, double[]? omega = null)
    {
        var layout = FermionFieldLayoutFactory.BuildStandardLayout(
            "layout-trivial", spec, gaugeDimension: 1, TestProvenance());
        var bg = MakeBackground("bg-parity-1");

        var gammaBuilder = new GammaMatrixBuilder();
        var gammas = gammaBuilder.Build(spec.CliffordSignature, spec.GammaConvention, TestProvenance());

        omega ??= new double[mesh.EdgeCount];

        var connBuilder = new CpuSpinConnectionBuilder();
        var conn = connBuilder.Build(bg, omega, spec, layout, mesh, TestProvenance());

        var assembler = new CpuDiracOperatorAssembler();
        var bundle = assembler.Assemble(conn, gammas, layout, mesh, TestProvenance());

        var cpu = new CpuDiracKernel(bundle, gammas, mesh, layout, conn, assembler);
        var gpu = new GpuDiracKernel(cpu);

        int bosonDim = mesh.EdgeCount * 1; // dimG = 1
        return (cpu, gpu, spec, bosonDim);
    }

    private static double[] RandomVector(Random rng, int n)
    {
        var v = new double[n];
        for (int i = 0; i < n; i++) v[i] = rng.NextDouble() - 0.5;
        return v;
    }

    // -------------------------------------------------------
    // Basic instantiation tests
    // -------------------------------------------------------

    [Fact]
    public void GpuDiracKernel_Constructs_WithoutCrash()
    {
        var mesh = TwoTriangles();
        var (cpu, gpu, _, _) = BuildKernelPair(Dim4Spec(), mesh);

        Assert.NotNull(gpu);
        Assert.Equal(cpu.SpinorDimension, gpu.SpinorDimension);
        Assert.Equal(cpu.SpacetimeDimension, gpu.SpacetimeDimension);
    }

    [Fact]
    public void GpuDiracKernel_IsCudaActive_ReturnsFalse_WhenNoCuda()
    {
        var mesh = TwoTriangles();
        var (cpu, gpu, _, _) = BuildKernelPair(Dim4Spec(), mesh);
        // On systems without CUDA, IsCudaActive must be false (graceful fallback)
        Assert.False(gpu.IsCudaActive);
    }

    // -------------------------------------------------------
    // dimY=4 (even): ApplyGamma parity tests
    // -------------------------------------------------------

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [Trait("Category", "CpuGpuParity")]
    public void ApplyGamma_Dim4_Mu_CpuGpuParity(int mu)
    {
        var mesh = TwoTriangles();
        var (cpu, gpu, _, _) = BuildKernelPair(Dim4Spec(), mesh);
        int n = cpu.SpinorDimension;

        var rng = new Random(42 + mu);
        var spinor = RandomVector(rng, n);
        var cpuOut = new double[n];
        var gpuOut = new double[n];

        cpu.ApplyGamma(mu, spinor, cpuOut);
        gpu.ApplyGamma(mu, spinor, gpuOut);

        var result = DiracParityChecker.Compare(
            $"ApplyGamma[mu={mu}]", cpuOut, gpuOut, DiracParityChecker.GammaTolerance);

        Assert.True(result.Passed,
            $"ApplyGamma[mu={mu}] parity failed: maxRelErr={result.MaxRelativeError:E4}");
    }

    // -------------------------------------------------------
    // dimY=4 (even): ApplyDirac parity test
    // -------------------------------------------------------

    [Fact]
    [Trait("Category", "CpuGpuParity")]
    public void ApplyDirac_Dim4_CpuGpuParity()
    {
        var mesh = TwoTriangles();
        var (cpu, gpu, _, _) = BuildKernelPair(Dim4Spec(), mesh);
        int n = cpu.SpinorDimension;

        var rng = new Random(100);
        var spinor = RandomVector(rng, n);
        var cpuOut = new double[n];
        var gpuOut = new double[n];

        cpu.ApplyDirac(spinor, cpuOut);
        gpu.ApplyDirac(spinor, gpuOut);

        var result = DiracParityChecker.Compare(
            "ApplyDirac[dim4]", cpuOut, gpuOut, DiracParityChecker.DiracTolerance);

        Assert.True(result.Passed,
            $"ApplyDirac dim4 parity failed: maxRelErr={result.MaxRelativeError:E4}");
    }

    // -------------------------------------------------------
    // dimY=5 (odd): ApplyDirac parity test
    // -------------------------------------------------------

    [Fact]
    [Trait("Category", "CpuGpuParity")]
    public void ApplyDirac_Dim5_CpuGpuParity()
    {
        var mesh = ThreeTetrahedra();
        var (cpu, gpu, _, _) = BuildKernelPair(Dim5Spec(), mesh);
        int n = cpu.SpinorDimension;

        var rng = new Random(200);
        var spinor = RandomVector(rng, n);
        var cpuOut = new double[n];
        var gpuOut = new double[n];

        cpu.ApplyDirac(spinor, cpuOut);
        gpu.ApplyDirac(spinor, gpuOut);

        var result = DiracParityChecker.Compare(
            "ApplyDirac[dim5]", cpuOut, gpuOut, DiracParityChecker.DiracTolerance);

        Assert.True(result.Passed,
            $"ApplyDirac dim5 parity failed: maxRelErr={result.MaxRelativeError:E4}");
    }

    // -------------------------------------------------------
    // ApplyMass parity tests (dimY=4 and dimY=5)
    // -------------------------------------------------------

    [Fact]
    [Trait("Category", "CpuGpuParity")]
    public void ApplyMass_Dim4_CpuGpuParity()
    {
        var mesh = TwoTriangles();
        var (cpu, gpu, _, _) = BuildKernelPair(Dim4Spec(), mesh);
        int n = cpu.SpinorDimension;

        var rng = new Random(300);
        var spinor = RandomVector(rng, n);
        var cpuOut = new double[n];
        var gpuOut = new double[n];

        cpu.ApplyMass(spinor, cpuOut);
        gpu.ApplyMass(spinor, gpuOut);

        var result = DiracParityChecker.Compare(
            "ApplyMass[dim4]", cpuOut, gpuOut, DiracParityChecker.MassTolerance);

        Assert.True(result.Passed,
            $"ApplyMass dim4 parity failed: maxRelErr={result.MaxRelativeError:E4}");
    }

    [Fact]
    [Trait("Category", "CpuGpuParity")]
    public void ApplyMass_Dim5_CpuGpuParity()
    {
        var mesh = ThreeTetrahedra();
        var (cpu, gpu, _, _) = BuildKernelPair(Dim5Spec(), mesh);
        int n = cpu.SpinorDimension;

        var rng = new Random(400);
        var spinor = RandomVector(rng, n);
        var cpuOut = new double[n];
        var gpuOut = new double[n];

        cpu.ApplyMass(spinor, cpuOut);
        gpu.ApplyMass(spinor, gpuOut);

        var result = DiracParityChecker.Compare(
            "ApplyMass[dim5]", cpuOut, gpuOut, DiracParityChecker.MassTolerance);

        Assert.True(result.Passed,
            $"ApplyMass dim5 parity failed: maxRelErr={result.MaxRelativeError:E4}");
    }

    // -------------------------------------------------------
    // ApplyChiralityProjector parity tests (dimY=4 only)
    // -------------------------------------------------------

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    [Trait("Category", "CpuGpuParity")]
    public void ApplyChiralityProjector_Dim4_CpuGpuParity(bool left)
    {
        var mesh = TwoTriangles();
        var (cpu, gpu, _, _) = BuildKernelPair(Dim4Spec(), mesh);
        int n = cpu.SpinorDimension;

        var rng = new Random(500);
        var spinor = RandomVector(rng, n);
        var cpuOut = new double[n];
        var gpuOut = new double[n];

        cpu.ApplyChiralityProjector(left, spinor, cpuOut);
        gpu.ApplyChiralityProjector(left, spinor, gpuOut);

        string side = left ? "P_L" : "P_R";
        var result = DiracParityChecker.Compare(
            $"ApplyChiralityProjector[{side}]", cpuOut, gpuOut, DiracParityChecker.ChiralityTolerance);

        Assert.True(result.Passed,
            $"ApplyChiralityProjector {side} parity failed: maxRelErr={result.MaxRelativeError:E4}");
    }

    [Fact]
    public void ApplyChiralityProjector_Dim5_Throws_InvalidOperation()
    {
        // Per M44 spec: ApplyChiralityProjector with odd dimY must throw InvalidOperationException
        var mesh = ThreeTetrahedra();
        var (cpu, gpu, _, _) = BuildKernelPair(Dim5Spec(), mesh);
        int n = cpu.SpinorDimension;

        var spinor = new double[n];
        var result = new double[n];

        // CPU must throw for odd dimY
        Assert.Throws<InvalidOperationException>(() =>
            cpu.ApplyChiralityProjector(true, spinor, result));

        // GPU must also throw (delegates to CPU)
        Assert.Throws<InvalidOperationException>(() =>
            gpu.ApplyChiralityProjector(true, spinor, result));
    }

    // -------------------------------------------------------
    // ComputeCouplingProxy parity tests (dimY=4 and dimY=5)
    // -------------------------------------------------------

    [Fact]
    [Trait("Category", "CpuGpuParity")]
    public void ComputeCouplingProxy_Dim4_CpuGpuParity()
    {
        var mesh = TwoTriangles();
        var (cpu, gpu, _, bosonDim) = BuildKernelPair(Dim4Spec(), mesh);
        int n = cpu.SpinorDimension;

        var rng = new Random(600);
        var spinorI = RandomVector(rng, n);
        var spinorJ = RandomVector(rng, n);
        var bosonK  = RandomVector(rng, bosonDim);

        var cpuCoupling = cpu.ComputeCouplingProxy(spinorI, spinorJ, bosonK);
        var gpuCoupling = gpu.ComputeCouplingProxy(spinorI, spinorJ, bosonK);

        var (reResult, imResult) = DiracParityChecker.CompareCoupling(
            "ComputeCouplingProxy[dim4]", cpuCoupling, gpuCoupling, DiracParityChecker.CouplingTolerance);

        Assert.True(reResult.Passed,
            $"CouplingProxy[Re] dim4 failed: maxRelErr={reResult.MaxRelativeError:E4}");
        Assert.True(imResult.Passed,
            $"CouplingProxy[Im] dim4 failed: maxRelErr={imResult.MaxRelativeError:E4}");
    }

    [Fact]
    [Trait("Category", "CpuGpuParity")]
    public void ComputeCouplingProxy_Dim5_CpuGpuParity()
    {
        var mesh = ThreeTetrahedra();
        var (cpu, gpu, _, bosonDim) = BuildKernelPair(Dim5Spec(), mesh);
        int n = cpu.SpinorDimension;

        var rng = new Random(700);
        var spinorI = RandomVector(rng, n);
        var spinorJ = RandomVector(rng, n);
        var bosonK  = RandomVector(rng, bosonDim);

        var cpuCoupling = cpu.ComputeCouplingProxy(spinorI, spinorJ, bosonK);
        var gpuCoupling = gpu.ComputeCouplingProxy(spinorI, spinorJ, bosonK);

        var (reResult, imResult) = DiracParityChecker.CompareCoupling(
            "ComputeCouplingProxy[dim5]", cpuCoupling, gpuCoupling, DiracParityChecker.CouplingTolerance);

        Assert.True(reResult.Passed,
            $"CouplingProxy[Re] dim5 failed: maxRelErr={reResult.MaxRelativeError:E4}");
        Assert.True(imResult.Passed,
            $"CouplingProxy[Im] dim5 failed: maxRelErr={imResult.MaxRelativeError:E4}");
    }

    // -------------------------------------------------------
    // RunFullCheck integration test (dimY=4, even — exercises all 5 operations)
    // -------------------------------------------------------

    [Fact]
    [Trait("Category", "CpuGpuParity")]
    public void RunFullCheck_Dim4_AllOperations_AllPass()
    {
        var mesh = TwoTriangles();
        var (cpu, gpu, spec, bosonDim) = BuildKernelPair(Dim4Spec(), mesh);

        var report = DiracParityChecker.RunFullCheck(
            cpu, gpu, spec, bosonDim, numTestVectors: 3, seed: 42);

        Assert.True(report.AllPassed,
            $"RunFullCheck dim4 failed. WorstError={report.WorstError:E4}. " +
            $"First failure: {report.Results.FirstOrDefault(r => !r.Passed)?.OperationName}");
    }

    // -------------------------------------------------------
    // RunFullCheck integration test (dimY=5, odd — chirality skipped)
    // -------------------------------------------------------

    [Fact]
    [Trait("Category", "CpuGpuParity")]
    public void RunFullCheck_Dim5_OddDimension_AllPass()
    {
        var mesh = ThreeTetrahedra();
        var (cpu, gpu, spec, bosonDim) = BuildKernelPair(Dim5Spec(), mesh);

        var report = DiracParityChecker.RunFullCheck(
            cpu, gpu, spec, bosonDim, numTestVectors: 3, seed: 42);

        // For odd dimY, chirality projectors are skipped — all remaining ops must pass.
        Assert.True(report.AllPassed,
            $"RunFullCheck dim5 failed. WorstError={report.WorstError:E4}. " +
            $"First failure: {report.Results.FirstOrDefault(r => !r.Passed)?.OperationName}");
    }

    // -------------------------------------------------------
    // GpuDiracKernel dispose safety test
    // -------------------------------------------------------

    [Fact]
    public void GpuDiracKernel_AfterDispose_ThrowsObjectDisposed()
    {
        var mesh = TwoTriangles();
        var (cpu, gpu, _, _) = BuildKernelPair(Dim4Spec(), mesh);
        int n = cpu.SpinorDimension;
        var spinor = new double[n];
        var result = new double[n];

        gpu.Dispose();

        Assert.Throws<ObjectDisposedException>(() => gpu.ApplyDirac(spinor, result));
    }

    // -------------------------------------------------------
    // DiracParityChecker.Compare unit tests
    // -------------------------------------------------------

    [Fact]
    public void DiracParityChecker_Compare_IdenticalOutputs_Passes()
    {
        var a = new double[] { 1.0, 2.0, 3.0 };
        var b = new double[] { 1.0, 2.0, 3.0 };

        var result = DiracParityChecker.Compare("test", a, b, 1e-9);
        Assert.True(result.Passed);
        Assert.Equal(0.0, result.MaxRelativeError);
    }

    [Fact]
    public void DiracParityChecker_Compare_LargeDifference_Fails()
    {
        var a = new double[] { 1.0, 2.0, 3.0 };
        var b = new double[] { 1.0, 2.0, 4.0 };

        var result = DiracParityChecker.Compare("test", a, b, 1e-9);
        Assert.False(result.Passed);
        Assert.True(result.MaxRelativeError > 0.1);
    }

    [Fact]
    public void DiracParityChecker_ParityReport_AllPassed_CorrectlyAggregated()
    {
        var mesh = TwoTriangles();
        var (cpu, gpu, spec, bosonDim) = BuildKernelPair(Dim4Spec(), mesh);

        var report = DiracParityChecker.RunFullCheck(cpu, gpu, spec, bosonDim);

        Assert.NotEmpty(report.Results);
        // AllPassed is derived from individual results
        bool expectedAllPassed = report.Results.All(r => r.Passed);
        Assert.Equal(expectedAllPassed, report.AllPassed);
    }
}
