using Gu.Core;
using Gu.Geometry;
using Gu.Phase3.Backgrounds;
using Gu.Phase4.Dirac;
using Gu.Phase4.Fermions;
using Gu.Phase4.Spin;

namespace Gu.Phase4.Dirac.Tests;

/// <summary>
/// CPU/GPU parity tests for Phase IV Dirac kernel (M44).
///
/// GpuDiracKernel falls back to the CPU reference when CUDA is unavailable,
/// so all parity checks must pass trivially (zero error). This validates the
/// DiracParityChecker logic and the CpuDiracKernel reference implementation.
///
/// When actual CUDA kernels are wired in, these tests serve as regression guards.
/// </summary>
[Trait("Category", "CpuGpuParity")]
public class DiracParityCheckerTests
{
    // -------------------------------------------------------
    // Test fixtures — mirrors CpuDiracOperatorAssemblerTests
    // -------------------------------------------------------

    private static ProvenanceMeta TestProvenance() => new()
    {
        CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
        CodeRevision = "test-m44",
        Branch = new BranchRef { BranchId = "test-branch", SchemaVersion = "1.0.0" },
        Backend = "cpu-reference",
    };

    private static SimplicialMesh TwoTriangles() =>
        MeshTopologyBuilder.Build(
            embeddingDimension: 2,
            simplicialDimension: 2,
            vertexCoordinates: new double[] { 0, 0, 1, 0, 0, 1, 1, 1 },
            vertexCount: 4,
            cellVertices: new[] { new[] { 0, 1, 2 }, new[] { 1, 3, 2 } });

    private static SimplicialMesh SingleTriangle() =>
        MeshTopologyBuilder.Build(
            embeddingDimension: 2,
            simplicialDimension: 2,
            vertexCoordinates: new double[] { 0, 0, 1, 0, 0, 1 },
            vertexCount: 3,
            cellVertices: new[] { new[] { 0, 1, 2 } });

    private static SpinorRepresentationSpec Dim2Spec() => new()
    {
        SpinorSpecId = "spinor-dim2-v1",
        SpacetimeDimension = 2,
        CliffordSignature = new CliffordSignature { Positive = 2, Negative = 0 },
        GammaConvention = new GammaConventionSpec
        {
            ConventionId = "dirac-tensor-product-v1",
            Signature = new CliffordSignature { Positive = 2, Negative = 0 },
            Representation = "standard",
            SpinorDimension = 2,
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
        SpinorComponents = 2,
        ChiralitySplit = 1,
        Provenance = TestProvenance(),
    };

    private static SpinorRepresentationSpec Dim3Spec() => new()
    {
        SpinorSpecId = "spinor-dim3-v1",
        SpacetimeDimension = 3,
        CliffordSignature = new CliffordSignature { Positive = 3, Negative = 0 },
        GammaConvention = new GammaConventionSpec
        {
            ConventionId = "dirac-tensor-product-v1",
            Signature = new CliffordSignature { Positive = 3, Negative = 0 },
            Representation = "standard",
            SpinorDimension = 2,
            HasChirality = false,
        },
        ChiralityConvention = new ChiralityConventionSpec
        {
            ConventionId = "chirality-none-v1",
            SignConvention = "left-is-minus",
            PhaseFactor = "-1",
            HasChirality = false,
        },
        ConjugationConvention = new ConjugationConventionSpec
        {
            ConventionId = "hermitian-v1",
            ConjugationType = "hermitian",
            HasChargeConjugation = false,
        },
        SpinorComponents = 2,
        ChiralitySplit = 0,
        Provenance = TestProvenance(),
    };

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

    /// <summary>
    /// Build a CpuDiracKernel for the given mesh + spec + optional omega.
    /// Returns (cpu, spec) for use in parity tests.
    /// </summary>
    private static (CpuDiracKernel cpu, SpinorRepresentationSpec spec)
        BuildKernel(SimplicialMesh mesh, SpinorRepresentationSpec spec, double[]? omega = null)
    {
        omega ??= new double[mesh.EdgeCount];
        var bg = MakeBackground("bg-parity");

        var gammaBuilder = new GammaMatrixBuilder();
        var gammas = gammaBuilder.Build(
            spec.CliffordSignature, spec.GammaConvention, TestProvenance());

        var connBuilder = new CpuSpinConnectionBuilder();
        var conn = connBuilder.Build(bg, omega, spec, FermionFieldLayoutFactory.BuildStandardLayout(
            "layout-trivial", spec, gaugeDimension: 1, TestProvenance()), mesh, TestProvenance());

        var layout = FermionFieldLayoutFactory.BuildStandardLayout(
            "layout-trivial", spec, gaugeDimension: 1, TestProvenance());
        var assembler = new CpuDiracOperatorAssembler();
        var bundle = assembler.Assemble(conn, gammas, layout, mesh, TestProvenance());

        var cpu = new CpuDiracKernel(bundle, gammas, mesh, layout, conn, assembler);
        return (cpu, spec);
    }

    // -------------------------------------------------------
    // CPU-vs-CPU parity (GpuDiracKernel with null backend falls back to CPU)
    // -------------------------------------------------------

    [Fact]
    public void GpuKernel_IsCudaActive_IsFalseWithoutNativeLibrary()
    {
        var mesh = TwoTriangles();
        var (cpu, _) = BuildKernel(mesh, Dim2Spec());
        var gpu = new GpuDiracKernel(cpu);
        Assert.False(gpu.IsCudaActive);
    }

    [Fact]
    public void ParityReport_AllPassed_ForDim2_CpuVsCpuFallback()
    {
        var mesh = TwoTriangles();
        var spec = Dim2Spec();
        var (cpu, _) = BuildKernel(mesh, spec);
        var gpu = new GpuDiracKernel(cpu);

        int bosonDim = mesh.EdgeCount * 1; // dimG=1
        var report = DiracParityChecker.RunFullCheck(
            cpu, gpu, spec, bosonDim, numTestVectors: 3, seed: 42);

        Assert.True(report.AllPassed,
            $"Parity failed. Worst error={report.WorstError:E3}. " +
            $"First failure: {report.Results.FirstOrDefault(r => !r.Passed)?.OperationName}");
    }

    [Fact]
    public void ParityReport_AllPassed_ForDim3_OddDimY_NoChirality()
    {
        var mesh = TwoTriangles();
        var spec = Dim3Spec();
        var (cpu, _) = BuildKernel(mesh, spec);
        var gpu = new GpuDiracKernel(cpu);

        int bosonDim = mesh.EdgeCount * 1;
        var report = DiracParityChecker.RunFullCheck(
            cpu, gpu, spec, bosonDim, numTestVectors: 3, seed: 7);

        Assert.True(report.AllPassed,
            $"Parity failed for dim3. Worst error={report.WorstError:E3}");
    }

    [Fact]
    public void ParityReport_AllPassed_ForSingleTriangle()
    {
        var mesh = SingleTriangle();
        var spec = Dim2Spec();
        var (cpu, _) = BuildKernel(mesh, spec);
        var gpu = new GpuDiracKernel(cpu);

        int bosonDim = mesh.EdgeCount * 1;
        var report = DiracParityChecker.RunFullCheck(
            cpu, gpu, spec, bosonDim, numTestVectors: 2, seed: 99);

        Assert.True(report.AllPassed,
            $"Parity failed for single triangle. Worst error={report.WorstError:E3}");
    }

    // -------------------------------------------------------
    // DiracParityChecker.Compare: error formula correctness
    // -------------------------------------------------------

    [Fact]
    public void Compare_IdenticalOutputs_ReturnsZeroError()
    {
        double[] a = { 1.0, 2.0, -3.0, 0.0 };
        double[] b = { 1.0, 2.0, -3.0, 0.0 };
        var result = DiracParityChecker.Compare("test-op", a, b, tolerance: 1e-10);

        Assert.Equal(0.0, result.MaxRelativeError);
        Assert.True(result.Passed);
        Assert.Equal(4, result.ComponentsCompared);
    }

    [Fact]
    public void Compare_SmallDifference_BelowTolerance_Passes()
    {
        double[] a = { 1.0, 0.0 };
        double[] b = { 1.0 + 1e-12, 1e-15 };
        var result = DiracParityChecker.Compare("test-op", a, b, tolerance: 1e-10);

        Assert.True(result.Passed);
        Assert.True(result.MaxRelativeError < 1e-10);
    }

    [Fact]
    public void Compare_LargeDifference_AboveTolerance_Fails()
    {
        double[] a = { 1.0, 0.0 };
        double[] b = { 2.0, 0.0 };
        var result = DiracParityChecker.Compare("test-op", a, b, tolerance: 1e-10);

        Assert.False(result.Passed);
        // |a-b| / (1 + |a|) = 1.0 / 2.0 = 0.5
        Assert.True(result.MaxRelativeError > 0.4);
    }

    [Fact]
    public void Compare_ErrorFormula_UsesOnePlusCpuNorm_NotGpuNorm()
    {
        // cpu=10, gpu=9: |10-9|/(1+10) = 1/11 ≈ 0.0909
        // cpu=0, gpu=1:  |0-1|/(1+0)  = 1/1  = 1.0 (much larger)
        double[] cpu = { 10.0 };
        double[] gpu = { 9.0 };
        var result = DiracParityChecker.Compare("test-op", cpu, gpu, tolerance: 1e-8);

        double expected = 1.0 / 11.0;
        Assert.True(System.Math.Abs(result.MaxRelativeError - expected) < 1e-12);
    }

    [Fact]
    public void Compare_ReportsMaxErrorIndex_Correctly()
    {
        double[] cpu = { 1.0, 1.0, 1.0, 100.0 };
        double[] gpu = { 1.0, 1.0, 1.0, 0.0 };
        // Index 3: |100-0|/(1+100) ≈ 0.99 — clearly the max
        var result = DiracParityChecker.Compare("test-op", cpu, gpu, tolerance: 0.5);

        Assert.Equal(3, result.MaxErrorIndex);
        Assert.False(result.Passed); // 0.99 > 0.5
    }

    [Fact]
    public void Compare_LengthMismatch_Throws()
    {
        double[] a = { 1.0, 2.0 };
        double[] b = { 1.0 };
        Assert.Throws<ArgumentException>(() =>
            DiracParityChecker.Compare("op", a, b, 1e-8));
    }

    // -------------------------------------------------------
    // CompareCoupling
    // -------------------------------------------------------

    [Fact]
    public void CompareCoupling_IdenticalValues_BothPassed()
    {
        var cpu = (Real: 1.5, Imag: -0.3);
        var gpu = (Real: 1.5, Imag: -0.3);
        var (re, im) = DiracParityChecker.CompareCoupling("coupling-test", cpu, gpu, 1e-8);

        Assert.True(re.Passed);
        Assert.True(im.Passed);
        Assert.Equal(0.0, re.MaxRelativeError);
        Assert.Equal(0.0, im.MaxRelativeError);
    }

    [Fact]
    public void CompareCoupling_OperationNameHasReSuffix()
    {
        var cpu = (Real: 1.0, Imag: 0.0);
        var gpu = (Real: 1.0, Imag: 0.0);
        var (re, im) = DiracParityChecker.CompareCoupling("proxy-trial=0", cpu, gpu, 1e-8);

        Assert.EndsWith("[Re]", re.OperationName);
        Assert.EndsWith("[Im]", im.OperationName);
    }

    // -------------------------------------------------------
    // RunFullCheck coverage validation
    // -------------------------------------------------------

    [Fact]
    public void RunFullCheck_Dim2_HasCorrectNumberOfResults()
    {
        var mesh = TwoTriangles();
        var spec = Dim2Spec();
        var (cpu, _) = BuildKernel(mesh, spec);
        var gpu = new GpuDiracKernel(cpu);

        int bosonDim = mesh.EdgeCount;
        // dim=2, numTestVectors=3:
        //   ApplyGamma: 2 * 3 = 6
        //   ApplyDirac: 3
        //   ApplyMass: 3
        //   ApplyChiralityProjector (L+R): 2 * 3 = 6 (has chirality for dim2)
        //   ComputeCouplingProxy: 3 * 2 (Re+Im) = 6
        // Total = 24
        var report = DiracParityChecker.RunFullCheck(
            cpu, gpu, spec, bosonDim, numTestVectors: 3, seed: 42);

        Assert.Equal(24, report.Results.Count);
    }

    [Fact]
    public void RunFullCheck_Dim3_SkipsChiralityTests()
    {
        var mesh = TwoTriangles();
        var spec = Dim3Spec();
        var (cpu, _) = BuildKernel(mesh, spec);
        var gpu = new GpuDiracKernel(cpu);

        int bosonDim = mesh.EdgeCount;
        // dim=3, numTestVectors=3:
        //   ApplyGamma: 3 * 3 = 9
        //   ApplyDirac: 3
        //   ApplyMass: 3
        //   ApplyChiralityProjector: 0 (odd dimY, HasChirality=false)
        //   ComputeCouplingProxy: 3 * 2 = 6
        // Total = 21
        var report = DiracParityChecker.RunFullCheck(
            cpu, gpu, spec, bosonDim, numTestVectors: 3, seed: 42);

        Assert.Equal(21, report.Results.Count);
        Assert.DoesNotContain(report.Results, r => r.OperationName.StartsWith("ApplyChirality"));
    }

    [Fact]
    public void RunFullCheck_DimensionMismatch_Throws()
    {
        var mesh = TwoTriangles();
        var (cpu2, _) = BuildKernel(mesh, Dim2Spec());
        var (cpu3, _) = BuildKernel(mesh, Dim3Spec());
        var gpu3 = new GpuDiracKernel(cpu3);

        var spec2 = Dim2Spec();
        // cpu2 has SpacetimeDimension=2, gpu3 has SpacetimeDimension=3
        Assert.Throws<ArgumentException>(() =>
            DiracParityChecker.RunFullCheck(cpu2, gpu3, spec2, 10));
    }

    [Fact]
    public void RunFullCheck_NullCpuKernel_Throws()
    {
        var mesh = TwoTriangles();
        var (cpu, spec) = BuildKernel(mesh, Dim2Spec());
        var gpu = new GpuDiracKernel(cpu);

        Assert.Throws<ArgumentNullException>(() =>
            DiracParityChecker.RunFullCheck(null!, gpu, spec, 10));
    }

    [Fact]
    public void RunFullCheck_NullSpec_Throws()
    {
        var mesh = TwoTriangles();
        var (cpu, _) = BuildKernel(mesh, Dim2Spec());
        var gpu = new GpuDiracKernel(cpu);

        Assert.Throws<ArgumentNullException>(() =>
            DiracParityChecker.RunFullCheck(cpu, gpu, null!, 10));
    }

    [Fact]
    public void RunFullCheck_WorstError_IsMaxOfAllResults()
    {
        var mesh = TwoTriangles();
        var spec = Dim2Spec();
        var (cpu, _) = BuildKernel(mesh, spec);
        var gpu = new GpuDiracKernel(cpu);

        var report = DiracParityChecker.RunFullCheck(
            cpu, gpu, spec, mesh.EdgeCount, numTestVectors: 3, seed: 1);

        double expected = report.Results.Max(r => r.MaxRelativeError);
        Assert.Equal(expected, report.WorstError);
    }

    // -------------------------------------------------------
    // GpuDiracKernel properties
    // -------------------------------------------------------

    [Fact]
    public void GpuKernel_SpinorDimension_MatchesCpu()
    {
        var mesh = TwoTriangles();
        var (cpu, _) = BuildKernel(mesh, Dim2Spec());
        var gpu = new GpuDiracKernel(cpu);

        Assert.Equal(cpu.SpinorDimension, gpu.SpinorDimension);
    }

    [Fact]
    public void GpuKernel_SpacetimeDimension_MatchesCpu()
    {
        var mesh = TwoTriangles();
        var (cpu, _) = BuildKernel(mesh, Dim2Spec());
        var gpu = new GpuDiracKernel(cpu);

        Assert.Equal(cpu.SpacetimeDimension, gpu.SpacetimeDimension);
    }

    [Fact]
    public void GpuKernel_Dispose_SetsNotActive()
    {
        var mesh = TwoTriangles();
        var (cpu, _) = BuildKernel(mesh, Dim2Spec());
        var gpu = new GpuDiracKernel(cpu);

        gpu.Dispose();
        Assert.False(gpu.IsCudaActive);
    }

    [Fact]
    public void GpuKernel_Dispose_ThrowsOnSubsequentApply()
    {
        var mesh = TwoTriangles();
        var (cpu, _) = BuildKernel(mesh, Dim2Spec());
        var gpu = new GpuDiracKernel(cpu);

        gpu.Dispose();

        var spinor = new double[cpu.SpinorDimension];
        var result = new double[cpu.SpinorDimension];
        Assert.Throws<ObjectDisposedException>(() => gpu.ApplyGamma(0, spinor, result));
    }

    // -------------------------------------------------------
    // CpuDiracKernel basic correctness via parity checker
    // -------------------------------------------------------

    [Fact]
    public void CpuDiracKernel_ApplyGamma_OutOfRange_Throws()
    {
        var mesh = TwoTriangles();
        var (cpu, _) = BuildKernel(mesh, Dim2Spec());

        var spinor = new double[cpu.SpinorDimension];
        var result = new double[cpu.SpinorDimension];
        Assert.Throws<ArgumentOutOfRangeException>(() => cpu.ApplyGamma(-1, spinor, result));
        Assert.Throws<ArgumentOutOfRangeException>(() => cpu.ApplyGamma(cpu.SpacetimeDimension, spinor, result));
    }

    [Fact]
    public void CpuDiracKernel_ApplyChirality_OddDim_Throws()
    {
        var mesh = TwoTriangles();
        var (cpu, _) = BuildKernel(mesh, Dim3Spec());

        var spinor = new double[cpu.SpinorDimension];
        var result = new double[cpu.SpinorDimension];
        Assert.Throws<InvalidOperationException>(() =>
            cpu.ApplyChiralityProjector(left: true, spinor, result));
    }

    [Fact]
    public void CpuDiracKernel_ApplyMass_IsIdentity_ForUniformVolume()
    {
        var mesh = TwoTriangles();
        var (cpu, _) = BuildKernel(mesh, Dim2Spec());

        var rng = new Random(17);
        var spinor = new double[cpu.SpinorDimension];
        for (int i = 0; i < spinor.Length; i++) spinor[i] = rng.NextDouble() - 0.5;

        var result = new double[cpu.SpinorDimension];
        cpu.ApplyMass(spinor, result);

        // M_psi = I under uniform volume=1.0
        for (int i = 0; i < spinor.Length; i++)
            Assert.Equal(spinor[i], result[i], precision: 12);
    }

    [Fact]
    public void CpuDiracKernel_ApplyChirality_PLplusPR_IsIdentity()
    {
        var mesh = TwoTriangles();
        var (cpu, _) = BuildKernel(mesh, Dim2Spec());

        var rng = new Random(55);
        var spinor = new double[cpu.SpinorDimension];
        for (int i = 0; i < spinor.Length; i++) spinor[i] = rng.NextDouble() - 0.5;

        var pl = new double[cpu.SpinorDimension];
        var pr = new double[cpu.SpinorDimension];
        cpu.ApplyChiralityProjector(left: true, spinor, pl);
        cpu.ApplyChiralityProjector(left: false, spinor, pr);

        // P_L + P_R = I
        for (int i = 0; i < spinor.Length; i++)
            Assert.True(System.Math.Abs(pl[i] + pr[i] - spinor[i]) < 1e-12,
                $"P_L+P_R != I at index {i}: pl={pl[i]}, pr={pr[i]}, spinor={spinor[i]}");
    }

    [Fact]
    public void CpuDiracKernel_ApplyChirality_PL_IsIdempotent()
    {
        var mesh = TwoTriangles();
        var (cpu, _) = BuildKernel(mesh, Dim2Spec());

        var rng = new Random(66);
        var spinor = new double[cpu.SpinorDimension];
        for (int i = 0; i < spinor.Length; i++) spinor[i] = rng.NextDouble() - 0.5;

        var pl1 = new double[cpu.SpinorDimension];
        var pl2 = new double[cpu.SpinorDimension];
        cpu.ApplyChiralityProjector(left: true, spinor, pl1);
        cpu.ApplyChiralityProjector(left: true, pl1, pl2);

        // P_L * P_L = P_L => P_L^2 spinor should equal P_L spinor
        for (int i = 0; i < spinor.Length; i++)
            Assert.True(System.Math.Abs(pl1[i] - pl2[i]) < 1e-12,
                $"P_L^2 != P_L at index {i}");
    }
}
