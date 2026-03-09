using Gu.Branching;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.Phase2.Stability;
using Gu.ReferenceCpu;
using Gu.Solvers;

namespace Gu.Phase2.Stability.Tests;

public class SpectrumProbeTests
{
    private static SimplicialMesh SingleTriangle() =>
        MeshTopologyBuilder.Build(
            embeddingDimension: 2,
            simplicialDimension: 2,
            vertexCoordinates: new double[] { 0, 0, 1, 0, 0, 1 },
            vertexCount: 3,
            cellVertices: new[] { new[] { 0, 1, 2 } });

    private static BranchManifest TestManifest() => new()
    {
        BranchId = "test-stability",
        SchemaVersion = "1.0.0",
        SourceEquationRevision = "r1",
        CodeRevision = "test",
        ActiveGeometryBranch = "simplicial",
        ActiveObservationBranch = "sigma-pullback",
        ActiveTorsionBranch = "trivial",
        ActiveShiabBranch = "identity-shiab",
        ActiveGaugeStrategy = "coulomb",
        BaseDimension = 4,
        AmbientDimension = 14,
        LieAlgebraId = "su2",
        BasisConventionId = "canonical",
        ComponentOrderId = "face-major",
        AdjointConventionId = "adjoint-explicit",
        PairingConventionId = "pairing-trace",
        NormConventionId = "norm-l2-quadrature",
        DifferentialFormMetricId = "hodge-standard",
        InsertedAssumptionIds = Array.Empty<string>(),
        InsertedChoiceIds = new[] { "IX-1", "IX-2" },
    };

    private static GeometryContext DummyGeometry() => new()
    {
        BaseSpace = new SpaceRef { SpaceId = "X_h", Dimension = 4 },
        AmbientSpace = new SpaceRef { SpaceId = "Y_h", Dimension = 14 },
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

    [Fact]
    public void Lanczos_ReturnsNonNegativeEigenvalues_ForHessian()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var torsion = new TrivialTorsionCpu(mesh, algebra);
        var shiab = new IdentityShiabCpu(mesh, algebra);
        var assembler = new CpuResidualAssembler(mesh, algebra, torsion, shiab);
        var massMatrix = new CpuMassMatrix(mesh, algebra);
        var manifest = TestManifest();
        var geometry = DummyGeometry();

        var omega = ConnectionField.Zero(mesh, algebra);
        var a0 = ConnectionField.Zero(mesh, algebra);

        var workbench = new LinearizationWorkbench(mesh, algebra, assembler, massMatrix);
        var bg = workbench.CreateBackgroundState(
            "bg-spectrum", omega.ToFieldTensor(), a0.ToFieldTensor(),
            manifest, geometry, solverConverged: true);

        var probe = new LanczosSpectrumProbe();
        var record = workbench.ProbeHessianSpectrum(
            bg, manifest, geometry, gaugeLambda: 1.0, probe, numEigenvalues: 3);

        Assert.True(record.ObtainedCount > 0, "No eigenvalues computed");
        Assert.Equal("H", record.OperatorId);
        Assert.Equal("lanczos", record.ProbeMethod);
        Assert.Equal("coulomb-slice", record.GaugeHandlingMode);

        // H = L_tilde^T L_tilde is PSD, all eigenvalues >= 0
        foreach (var ev in record.Values)
        {
            Assert.True(ev >= -1e-8,
                $"Hessian eigenvalue should be non-negative, got {ev:E6}");
        }
    }

    [Fact]
    public void Lanczos_SingularValues_AreNonNegative()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var torsion = new TrivialTorsionCpu(mesh, algebra);
        var shiab = new IdentityShiabCpu(mesh, algebra);
        var assembler = new CpuResidualAssembler(mesh, algebra, torsion, shiab);
        var massMatrix = new CpuMassMatrix(mesh, algebra);
        var manifest = TestManifest();
        var geometry = DummyGeometry();

        var omega = new ConnectionField(mesh, algebra);
        omega.SetEdgeValue(0, new[] { 0.3, 0.1, 0.0 });
        omega.SetEdgeValue(1, new[] { 0.0, 0.2, 0.1 });
        var a0 = ConnectionField.Zero(mesh, algebra);

        var workbench = new LinearizationWorkbench(mesh, algebra, assembler, massMatrix);
        var bg = workbench.CreateBackgroundState(
            "bg-sv", omega.ToFieldTensor(), a0.ToFieldTensor(),
            manifest, geometry, solverConverged: true);

        var probe = new LanczosSpectrumProbe();
        var record = workbench.ProbeLTildeSingularValues(
            bg, manifest, geometry, gaugeLambda: 1.0, probe, numSingularValues: 3);

        Assert.True(record.ObtainedCount > 0, "No singular values computed");
        Assert.Equal("L_tilde", record.OperatorId);

        foreach (var sv in record.Values)
        {
            Assert.True(sv >= -1e-8,
                $"Singular value should be non-negative, got {sv:E6}");
        }
    }

    [Fact]
    public void Lobpcg_MethodId_IsLobpcg()
    {
        var probe = new LobpcgSpectrumProbe();
        Assert.Equal("lobpcg", probe.MethodId);
    }

    [Fact]
    public void Lobpcg_AgreesWithLanczos_OnHessianEigenvalues()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var torsion = new TrivialTorsionCpu(mesh, algebra);
        var shiab = new IdentityShiabCpu(mesh, algebra);
        var assembler = new CpuResidualAssembler(mesh, algebra, torsion, shiab);
        var massMatrix = new CpuMassMatrix(mesh, algebra);
        var manifest = TestManifest();
        var geometry = DummyGeometry();

        var omega = ConnectionField.Zero(mesh, algebra);
        var a0 = ConnectionField.Zero(mesh, algebra);

        var workbench = new LinearizationWorkbench(mesh, algebra, assembler, massMatrix);
        var bg = workbench.CreateBackgroundState(
            "bg-lobpcg", omega.ToFieldTensor(), a0.ToFieldTensor(),
            manifest, geometry, solverConverged: true);

        var lanczos = new LanczosSpectrumProbe();
        var lobpcg = new LobpcgSpectrumProbe();
        int numEv = 2;

        var lanczosRecord = workbench.ProbeHessianSpectrum(
            bg, manifest, geometry, gaugeLambda: 1.0, lanczos, numEigenvalues: numEv);
        var lobpcgRecord = workbench.ProbeHessianSpectrum(
            bg, manifest, geometry, gaugeLambda: 1.0, lobpcg, numEigenvalues: numEv);

        Assert.True(lobpcgRecord.ObtainedCount > 0, "LOBPCG should compute eigenvalues");
        Assert.Equal("lobpcg", lobpcgRecord.ProbeMethod);

        // Both should produce non-negative eigenvalues for PSD Hessian
        int common = System.Math.Min(lanczosRecord.ObtainedCount, lobpcgRecord.ObtainedCount);
        for (int i = 0; i < common; i++)
        {
            Assert.True(lobpcgRecord.Values[i] >= -1e-8,
                $"LOBPCG eigenvalue {i} should be non-negative, got {lobpcgRecord.Values[i]:E6}");
            // Eigenvalues should agree within tolerance
            double relDiff = System.Math.Abs(lanczosRecord.Values[i] - lobpcgRecord.Values[i]) /
                (1.0 + System.Math.Abs(lanczosRecord.Values[i]));
            Assert.True(relDiff < 1e-4,
                $"Eigenvalue {i}: Lanczos={lanczosRecord.Values[i]:E6}, LOBPCG={lobpcgRecord.Values[i]:E6}, relDiff={relDiff:E6}");
        }
    }

    [Fact]
    public void Lobpcg_HandlesNearDegenerateEigenvalues()
    {
        // Create a simple symmetric operator with known near-degenerate eigenvalues
        var nearDegOp = new DiagonalOperator(new[] { 0.1, 0.1001, 0.5, 1.0, 2.0 });
        var probe = new LobpcgSpectrumProbe();
        var result = probe.ComputeSmallestEigenvalues(nearDegOp, 3);

        Assert.True(result.Values.Length >= 2, "Should compute at least 2 eigenvalues");
        // First two eigenvalues should be near 0.1 and 0.1001
        Assert.True(System.Math.Abs(result.Values[0] - 0.1) < 1e-4,
            $"First eigenvalue should be ~0.1, got {result.Values[0]:E6}");
        Assert.True(System.Math.Abs(result.Values[1] - 0.1001) < 1e-4,
            $"Second eigenvalue should be ~0.1001, got {result.Values[1]:E6}");
    }

    [Fact]
    public void Lobpcg_ReturnsEigenvectors()
    {
        var diagOp = new DiagonalOperator(new[] { 3.0, 1.0, 2.0, 5.0, 4.0 });
        var probe = new LobpcgSpectrumProbe();
        var result = probe.ComputeSmallestEigenvalues(diagOp, 2);

        Assert.NotNull(result.Vectors);
        Assert.True(result.Vectors.Length >= 2, "Should return eigenvectors");
    }

    [Fact]
    public void SpectrumRecord_StabilityInterpretation_IsSet()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var torsion = new TrivialTorsionCpu(mesh, algebra);
        var shiab = new IdentityShiabCpu(mesh, algebra);
        var assembler = new CpuResidualAssembler(mesh, algebra, torsion, shiab);
        var massMatrix = new CpuMassMatrix(mesh, algebra);
        var manifest = TestManifest();
        var geometry = DummyGeometry();

        var omega = ConnectionField.Zero(mesh, algebra);
        var a0 = ConnectionField.Zero(mesh, algebra);

        var workbench = new LinearizationWorkbench(mesh, algebra, assembler, massMatrix);
        var bg = workbench.CreateBackgroundState(
            "bg-interp", omega.ToFieldTensor(), a0.ToFieldTensor(),
            manifest, geometry, solverConverged: true);

        var probe = new LanczosSpectrumProbe();
        var record = workbench.ProbeHessianSpectrum(
            bg, manifest, geometry, gaugeLambda: 1.0, probe, numEigenvalues: 3);

        Assert.NotNull(record.StabilityInterpretation);
        Assert.Contains(record.StabilityInterpretation, new[]
        {
            "strictly-positive-on-slice",
            "near-zero-kernel",
            "soft-modes-present",
            "negative-modes-saddle",
            "no-eigenvalues-computed",
        });
    }

    [Fact]
    public void ClassifySpectrum_CategorizesCorrectly()
    {
        var spec = new StabilityStudySpec
        {
            SoftModeThreshold = 1e-4,
            NearKernelThreshold = 1e-8,
            NegativeModeThreshold = -1e-8,
        };

        // Mix of all categories
        var eigenvalues = new[] { -0.01, 0.0, 1e-6, 0.5, 1.0 };
        var (interp, coercive, soft, nearKernel, negative) =
            LinearizationWorkbench.ClassifySpectrum(eigenvalues, spec);

        Assert.Equal("negative-modes-saddle", interp);
        Assert.Equal(2, coercive);   // 0.5, 1.0
        Assert.Equal(1, soft);       // 1e-6
        Assert.Equal(1, nearKernel); // 0.0
        Assert.Equal(1, negative);   // -0.01
    }

    [Fact]
    public void ClassifySpectrum_AllPositive_StrictlyPositive()
    {
        var spec = StabilityStudySpec.Default;
        var eigenvalues = new[] { 0.1, 0.5, 1.0 };
        var (interp, coercive, soft, nearKernel, negative) =
            LinearizationWorkbench.ClassifySpectrum(eigenvalues, spec);

        Assert.Equal("strictly-positive-on-slice", interp);
        Assert.Equal(3, coercive);
        Assert.Equal(0, soft);
        Assert.Equal(0, nearKernel);
        Assert.Equal(0, negative);
    }

    [Fact]
    public void ClassifySpectrum_Empty_ReturnsNoEigenvalues()
    {
        var spec = StabilityStudySpec.Default;
        var (interp, coercive, soft, nearKernel, negative) =
            LinearizationWorkbench.ClassifySpectrum(Array.Empty<double>(), spec);

        Assert.Equal("no-eigenvalues-computed", interp);
        Assert.Equal(0, coercive);
        Assert.Equal(0, soft);
        Assert.Equal(0, nearKernel);
        Assert.Equal(0, negative);
    }
}
