using Gu.Branching;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.Phase2.Stability;
using Gu.ReferenceCpu;
using Gu.Solvers;

namespace Gu.Phase2.Stability.Tests;

public class HessianOperatorTests
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
    public void Hessian_IsSquare()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var torsion = new TrivialTorsionCpu(mesh, algebra);
        var shiab = new IdentityShiabCpu(mesh, algebra);
        var assembler = new CpuResidualAssembler(mesh, algebra, torsion, shiab);
        var massMatrix = new CpuMassMatrix(mesh, algebra);

        var omega = ConnectionField.Zero(mesh, algebra);
        var a0 = ConnectionField.Zero(mesh, algebra);
        var curvature = CurvatureAssembler.Assemble(omega);
        var jacobian = assembler.BuildJacobian(omega, a0, curvature.ToFieldTensor(), TestManifest(), DummyGeometry());
        double lambda = 1.0;
        var gauge = new CoulombGaugePenalty(mesh, algebra.Dimension, lambda);
        var hessian = new HessianOperator(
            jacobian, gauge, massMatrix, null, lambda, algebra.Dimension, mesh.VertexCount);

        Assert.Equal(hessian.InputDimension, hessian.OutputDimension);
        Assert.Equal(jacobian.InputDimension, hessian.InputDimension);
    }

    [Fact]
    public void Hessian_IsSymmetric()
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
        var a0 = ConnectionField.Zero(mesh, algebra);

        var workbench = new LinearizationWorkbench(mesh, algebra, assembler, massMatrix);
        var bg = workbench.CreateBackgroundState(
            "bg-sym", omega.ToFieldTensor(), a0.ToFieldTensor(),
            manifest, geometry, solverConverged: true);

        var record = workbench.ValidateHessianSymmetry(bg, manifest, geometry, gaugeLambda: 1.0);

        Assert.True(record.SymmetryVerified,
            $"Hessian symmetry check failed: error = {record.SymmetryError:E6}");
    }

    [Fact]
    public void Hessian_IsSemiDefinite_AtFlatBackground()
    {
        // At omega=0 with identity Shiab/trivial torsion, H = d^T d + lambda d_star^T d_star
        // This should be positive semi-definite
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
            "bg-flat", omega.ToFieldTensor(), a0.ToFieldTensor(),
            manifest, geometry, solverConverged: true);

        var hessian = workbench.BuildHessian(bg, manifest, geometry, gaugeLambda: 1.0);

        // Check: <v, H*v> >= 0 for random vectors
        var rng = new Random(42);
        for (int t = 0; t < 10; t++)
        {
            var v = MakeRandomField(hessian.InputDimension, rng, hessian.InputSignature);
            var hv = hessian.Apply(v);
            double vHv = Dot(v.Coefficients, hv.Coefficients);
            Assert.True(vHv >= -1e-12,
                $"H is not positive semi-definite: <v,Hv> = {vHv:E6}");
        }
    }

    [Fact]
    public void Hessian_EqualsJtMrJ_PlusLambdaCtM0C()
    {
        // Verify H = J^T M_R J + lambda * C^T M_0 C by direct computation
        // With trace pairing and uniform weights: M_R = I, M_0 = I
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
        var a0 = ConnectionField.Zero(mesh, algebra);
        var curvature = CurvatureAssembler.Assemble(omega);
        var jacobian = assembler.BuildJacobian(omega, a0, curvature.ToFieldTensor(), manifest, geometry);

        double lambda = 2.5;
        var gauge = new CoulombGaugePenalty(mesh, algebra.Dimension, lambda);
        var hessian = new HessianOperator(
            jacobian, gauge, massMatrix, null, lambda, algebra.Dimension, mesh.VertexCount);

        var rng = new Random(123);
        var v = MakeRandomField(hessian.InputDimension, rng, hessian.InputSignature);

        // H*v via Hessian operator
        var hv = hessian.Apply(v);

        // J^T M_R J * v (with trace pairing + uniform weights, M_R = I)
        var jv = jacobian.Apply(v);
        var mJv = massMatrix.Apply(jv);
        var jtMjv = jacobian.ApplyTranspose(mJv);

        // C^T M_0 C * v = d(M_0 * d^*(v)) (with uniform M_0 = I)
        var dStarV = gauge.ApplyCodifferential(v.Coefficients);
        var ddStarV = gauge.ApplyExteriorDerivative(dStarV);

        // Expected: J^T M_R J v + lambda * C^T M_0 C v
        for (int i = 0; i < hv.Coefficients.Length; i++)
        {
            double expected = jtMjv.Coefficients[i] + lambda * ddStarV[i];
            Assert.Equal(expected, hv.Coefficients[i], 10);
        }
    }

    private static FieldTensor MakeRandomField(int n, Random rng, TensorSignature sig)
    {
        var coeffs = new double[n];
        for (int i = 0; i < n; i++)
            coeffs[i] = rng.NextDouble() * 2.0 - 1.0;
        return new FieldTensor
        {
            Label = "random",
            Signature = sig,
            Coefficients = coeffs,
            Shape = new[] { n },
        };
    }

    private static double Dot(double[] a, double[] b)
    {
        double sum = 0;
        for (int i = 0; i < a.Length; i++)
            sum += a[i] * b[i];
        return sum;
    }
}
