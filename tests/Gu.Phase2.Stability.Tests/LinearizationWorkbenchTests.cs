using Gu.Branching;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.Phase2.Stability;
using Gu.ReferenceCpu;
using Gu.Solvers;

namespace Gu.Phase2.Stability.Tests;

public class LinearizationWorkbenchTests
{
    private static SimplicialMesh SingleTriangle() =>
        MeshTopologyBuilder.Build(
            embeddingDimension: 2,
            simplicialDimension: 2,
            vertexCoordinates: new double[] { 0, 0, 1, 0, 0, 1 },
            vertexCount: 3,
            cellVertices: new[] { new[] { 0, 1, 2 } });

    private static SimplicialMesh SingleTetrahedron() =>
        MeshTopologyBuilder.Build(
            embeddingDimension: 3,
            simplicialDimension: 3,
            vertexCoordinates: new double[] { 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1 },
            vertexCount: 4,
            cellVertices: new[] { new[] { 0, 1, 2, 3 } });

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
    public void CreateBackgroundState_FlatOmega_ZeroResidual()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var torsion = new TrivialTorsionCpu(mesh, algebra);
        var shiab = new IdentityShiabCpu(mesh, algebra);
        var assembler = new CpuResidualAssembler(mesh, algebra, torsion, shiab);
        var massMatrix = new CpuMassMatrix(mesh, algebra);

        var workbench = new LinearizationWorkbench(mesh, algebra, assembler, massMatrix);

        var omega = ConnectionField.Zero(mesh, algebra);
        var a0 = ConnectionField.Zero(mesh, algebra);
        var manifest = TestManifest();
        var geometry = DummyGeometry();

        var bg = workbench.CreateBackgroundState(
            "bg-flat", omega.ToFieldTensor(), a0.ToFieldTensor(),
            manifest, geometry, solverConverged: true, terminationReason: "flat background");

        Assert.Equal("bg-flat", bg.Id);
        Assert.Equal(0.0, bg.ResidualNorm, 12);
        Assert.Equal(0.0, bg.ObjectiveValue, 12);
        Assert.True(bg.SolverConverged);
    }

    [Fact]
    public void ValidateJacobian_2D_FdPasses()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var torsion = new TrivialTorsionCpu(mesh, algebra);
        var shiab = new IdentityShiabCpu(mesh, algebra);
        var assembler = new CpuResidualAssembler(mesh, algebra, torsion, shiab);
        var massMatrix = new CpuMassMatrix(mesh, algebra);

        var workbench = new LinearizationWorkbench(mesh, algebra, assembler, massMatrix);

        var omega = new ConnectionField(mesh, algebra);
        omega.SetEdgeValue(0, new[] { 0.5, 0.1, 0.0 });
        omega.SetEdgeValue(1, new[] { 0.0, 0.3, 0.2 });
        omega.SetEdgeValue(2, new[] { 0.1, 0.0, 0.4 });
        var a0 = ConnectionField.Zero(mesh, algebra);
        var manifest = TestManifest();
        var geometry = DummyGeometry();

        var bg = workbench.CreateBackgroundState(
            "bg-2d", omega.ToFieldTensor(), a0.ToFieldTensor(),
            manifest, geometry, solverConverged: true);

        var delta = new ConnectionField(mesh, algebra);
        delta.SetEdgeValue(0, new[] { 0.1, 0.05, 0.0 });
        delta.SetEdgeValue(1, new[] { 0.0, 0.1, 0.03 });
        delta.SetEdgeValue(2, new[] { 0.02, 0.0, 0.1 });

        var record = workbench.ValidateJacobian(bg, manifest, geometry, delta.ToFieldTensor());

        Assert.Equal("verified", record.ValidationStatus);
        Assert.Equal("analytic", record.DerivativeMode);
        Assert.Equal("matrix-free", record.AssemblyMode);
        Assert.Equal("raw", record.GaugeHandlingMode);
    }

    [Fact]
    public void ValidateJacobian_3D_FdPasses()
    {
        var mesh = SingleTetrahedron();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var torsion = new TrivialTorsionCpu(mesh, algebra);
        var shiab = new IdentityShiabCpu(mesh, algebra);
        var assembler = new CpuResidualAssembler(mesh, algebra, torsion, shiab);
        var massMatrix = new CpuMassMatrix(mesh, algebra);

        var workbench = new LinearizationWorkbench(mesh, algebra, assembler, massMatrix);

        var omega = new ConnectionField(mesh, algebra);
        for (int e = 0; e < mesh.EdgeCount; e++)
            omega.SetEdgeValue(e, new[] { 0.1 * (e + 1), 0.05 * (e + 2), 0.02 * (e + 3) });
        var a0 = ConnectionField.Zero(mesh, algebra);
        var manifest = TestManifest();
        var geometry = DummyGeometry();

        var bg = workbench.CreateBackgroundState(
            "bg-3d", omega.ToFieldTensor(), a0.ToFieldTensor(),
            manifest, geometry, solverConverged: true);

        var delta = new ConnectionField(mesh, algebra);
        for (int e = 0; e < mesh.EdgeCount; e++)
            delta.SetEdgeValue(e, new[] { 0.01 * (e + 1), 0.02 * (e + 2), 0.03 * (e + 3) });

        var record = workbench.ValidateJacobian(bg, manifest, geometry, delta.ToFieldTensor());

        Assert.Equal("verified", record.ValidationStatus);
    }

    [Fact]
    public void FullWorkflow_BackgroundToSpectrum()
    {
        // End-to-end test: create background -> validate J -> validate L_tilde
        // -> validate H symmetry -> probe spectrum
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var torsion = new TrivialTorsionCpu(mesh, algebra);
        var shiab = new IdentityShiabCpu(mesh, algebra);
        var assembler = new CpuResidualAssembler(mesh, algebra, torsion, shiab);
        var massMatrix = new CpuMassMatrix(mesh, algebra);
        var manifest = TestManifest();
        var geometry = DummyGeometry();

        var workbench = new LinearizationWorkbench(mesh, algebra, assembler, massMatrix);

        var omega = new ConnectionField(mesh, algebra);
        omega.SetEdgeValue(0, new[] { 0.3, 0.1, 0.0 });
        omega.SetEdgeValue(1, new[] { 0.0, 0.2, 0.1 });
        var a0 = ConnectionField.Zero(mesh, algebra);

        // 1. Background state
        var bg = workbench.CreateBackgroundState(
            "bg-full", omega.ToFieldTensor(), a0.ToFieldTensor(),
            manifest, geometry, solverConverged: true);
        Assert.True(bg.ObjectiveValue >= 0);

        // 2. Validate Jacobian
        var delta = new ConnectionField(mesh, algebra);
        delta.SetEdgeValue(0, new[] { 0.1, 0.0, 0.0 });
        delta.SetEdgeValue(1, new[] { 0.0, 0.1, 0.0 });
        var jRecord = workbench.ValidateJacobian(bg, manifest, geometry, delta.ToFieldTensor());
        Assert.Equal("verified", jRecord.ValidationStatus);

        // 3. Validate L_tilde
        var ltRecord = workbench.ValidateGaugeFixedOperator(
            bg, manifest, geometry, gaugeLambda: 1.0, delta.ToFieldTensor());
        Assert.Equal("verified", ltRecord.ValidationStatus);
        Assert.Equal("coulomb-slice", ltRecord.GaugeHandlingMode);

        // 4. Validate H symmetry
        var hRecord = workbench.ValidateHessianSymmetry(bg, manifest, geometry, gaugeLambda: 1.0);
        Assert.True(hRecord.SymmetryVerified);

        // 5. Probe spectrum
        var probe = new LanczosSpectrumProbe();
        var sRecord = workbench.ProbeHessianSpectrum(
            bg, manifest, geometry, gaugeLambda: 1.0, probe, numEigenvalues: 3);
        Assert.True(sRecord.ObtainedCount > 0);
        Assert.NotNull(sRecord.StabilityInterpretation);
    }
}
