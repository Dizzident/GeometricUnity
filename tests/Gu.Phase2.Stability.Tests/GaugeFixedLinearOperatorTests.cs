using Gu.Branching;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.Phase2.Stability;
using Gu.ReferenceCpu;
using Gu.Solvers;

namespace Gu.Phase2.Stability.Tests;

public class GaugeFixedLinearOperatorTests
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
    public void OutputDimension_IsStackedJacobianPlusGauge()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var torsion = new TrivialTorsionCpu(mesh, algebra);
        var shiab = new IdentityShiabCpu(mesh, algebra);
        var assembler = new CpuResidualAssembler(mesh, algebra, torsion, shiab);
        var manifest = TestManifest();
        var geometry = DummyGeometry();

        var omega = ConnectionField.Zero(mesh, algebra);
        var a0 = ConnectionField.Zero(mesh, algebra);
        var curvature = CurvatureAssembler.Assemble(omega);
        var jacobian = assembler.BuildJacobian(omega, a0, curvature.ToFieldTensor(), manifest, geometry);

        double lambda = 1.0;
        var gauge = new CoulombGaugePenalty(mesh, algebra.Dimension, lambda);
        var lTilde = new GaugeFixedLinearOperator(jacobian, gauge, mesh, algebra.Dimension);

        // Output = J output (FaceCount * dimG) + gauge output (VertexCount * dimG)
        int expectedOutput = mesh.FaceCount * algebra.Dimension + mesh.VertexCount * algebra.Dimension;
        Assert.Equal(expectedOutput, lTilde.OutputDimension);
        Assert.Equal(jacobian.InputDimension, lTilde.InputDimension);
    }

    [Fact]
    public void LTilde_Transpose_SatisfiesAdjointProperty()
    {
        // Test: <L_tilde*v, w> == <v, L_tilde^T*w>
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var torsion = new TrivialTorsionCpu(mesh, algebra);
        var shiab = new IdentityShiabCpu(mesh, algebra);
        var assembler = new CpuResidualAssembler(mesh, algebra, torsion, shiab);
        var manifest = TestManifest();
        var geometry = DummyGeometry();

        var omega = new ConnectionField(mesh, algebra);
        omega.SetEdgeValue(0, new[] { 0.5, 0.1, 0.0 });
        omega.SetEdgeValue(1, new[] { 0.0, 0.3, 0.2 });
        var a0 = ConnectionField.Zero(mesh, algebra);
        var curvature = CurvatureAssembler.Assemble(omega);
        var jacobian = assembler.BuildJacobian(omega, a0, curvature.ToFieldTensor(), manifest, geometry);

        double lambda = 2.0;
        var gauge = new CoulombGaugePenalty(mesh, algebra.Dimension, lambda);
        var lTilde = new GaugeFixedLinearOperator(jacobian, gauge, mesh, algebra.Dimension);

        // Input vector v
        var vCoeffs = new double[lTilde.InputDimension];
        var rng = new Random(42);
        for (int i = 0; i < vCoeffs.Length; i++)
            vCoeffs[i] = rng.NextDouble() * 2.0 - 1.0;
        var v = new FieldTensor
        {
            Label = "v",
            Signature = lTilde.InputSignature,
            Coefficients = vCoeffs,
            Shape = new[] { vCoeffs.Length },
        };

        // Output vector w
        var wCoeffs = new double[lTilde.OutputDimension];
        for (int i = 0; i < wCoeffs.Length; i++)
            wCoeffs[i] = rng.NextDouble() * 2.0 - 1.0;
        var w = new FieldTensor
        {
            Label = "w",
            Signature = lTilde.OutputSignature,
            Coefficients = wCoeffs,
            Shape = new[] { wCoeffs.Length },
        };

        var ltV = lTilde.Apply(v);
        var ltTw = lTilde.ApplyTranspose(w);

        double lhs = Dot(ltV.Coefficients, w.Coefficients);
        double rhs = Dot(v.Coefficients, ltTw.Coefficients);

        Assert.Equal(lhs, rhs, 10);
    }

    [Fact]
    public void LTilde_ZeroLambda_EqualsJacobian()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var torsion = new TrivialTorsionCpu(mesh, algebra);
        var shiab = new IdentityShiabCpu(mesh, algebra);
        var assembler = new CpuResidualAssembler(mesh, algebra, torsion, shiab);
        var manifest = TestManifest();
        var geometry = DummyGeometry();

        var omega = new ConnectionField(mesh, algebra);
        omega.SetEdgeValue(0, new[] { 0.5, 0.1, 0.0 });
        var a0 = ConnectionField.Zero(mesh, algebra);
        var curvature = CurvatureAssembler.Assemble(omega);
        var jacobian = assembler.BuildJacobian(omega, a0, curvature.ToFieldTensor(), manifest, geometry);

        // lambda = 0: gauge block is zero, L_tilde = (J, 0)
        var gauge = new CoulombGaugePenalty(mesh, algebra.Dimension, 0.0);
        var lTilde = new GaugeFixedLinearOperator(jacobian, gauge, mesh, algebra.Dimension);

        var delta = new ConnectionField(mesh, algebra);
        delta.SetEdgeValue(0, new[] { 0.1, 0.0, 0.0 });
        delta.SetEdgeValue(1, new[] { 0.0, 0.1, 0.0 });
        var deltaTensor = delta.ToFieldTensor();

        var jv = jacobian.Apply(deltaTensor);
        var ltv = lTilde.Apply(deltaTensor);

        // First block of L_tilde output should match J output
        for (int i = 0; i < jv.Coefficients.Length; i++)
            Assert.Equal(jv.Coefficients[i], ltv.Coefficients[i], 12);

        // Gauge block should be zero
        for (int i = jv.Coefficients.Length; i < ltv.Coefficients.Length; i++)
            Assert.Equal(0.0, ltv.Coefficients[i], 12);
    }

    [Fact]
    public void LTilde_FiniteDifference_Passes()
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
        omega.SetEdgeValue(0, new[] { 0.5, 0.1, 0.0 });
        omega.SetEdgeValue(1, new[] { 0.0, 0.3, 0.2 });
        omega.SetEdgeValue(2, new[] { 0.1, 0.0, 0.4 });
        var a0 = ConnectionField.Zero(mesh, algebra);

        var workbench = new LinearizationWorkbench(mesh, algebra, assembler, massMatrix);
        var bg = workbench.CreateBackgroundState(
            "bg-test", omega.ToFieldTensor(), a0.ToFieldTensor(),
            manifest, geometry, solverConverged: true);

        var delta = new ConnectionField(mesh, algebra);
        delta.SetEdgeValue(0, new[] { 0.1, 0.05, 0.0 });
        delta.SetEdgeValue(1, new[] { 0.0, 0.1, 0.03 });
        delta.SetEdgeValue(2, new[] { 0.02, 0.0, 0.1 });

        var record = workbench.ValidateGaugeFixedOperator(
            bg, manifest, geometry, gaugeLambda: 1.0, delta.ToFieldTensor());

        Assert.Equal("verified", record.ValidationStatus);
        Assert.True(record.FdMaxAbsoluteError < 1e-4,
            $"L_tilde FD verification failed: max abs error = {record.FdMaxAbsoluteError:E6}");
    }

    private static double Dot(double[] a, double[] b)
    {
        double sum = 0;
        for (int i = 0; i < a.Length; i++)
            sum += a[i] * b[i];
        return sum;
    }
}
