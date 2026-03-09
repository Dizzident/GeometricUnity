using Gu.Branching;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.Phase2.Stability;
using Gu.ReferenceCpu;
using Gu.Solvers;

namespace Gu.Phase2.Stability.Tests;

public class InfinitesimalGaugeMapTests
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
    public void GaugeMap_Dimensions_AreCorrect()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var a0 = ConnectionField.Zero(mesh, algebra).ToFieldTensor();
        var omega = ConnectionField.Zero(mesh, algebra).ToFieldTensor();

        var gaugeMap = new InfinitesimalGaugeMap(mesh, algebra, a0, omega);

        Assert.Equal(mesh.VertexCount * algebra.Dimension, gaugeMap.InputDimension);
        Assert.Equal(mesh.EdgeCount * algebra.Dimension, gaugeMap.OutputDimension);
    }

    [Fact]
    public void GaugeMap_AtFlatBackground_EqualsNegativeExteriorDerivative()
    {
        // When A_* = A0 = 0: R(xi) = -d(xi) (pure exterior derivative)
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var a0 = ConnectionField.Zero(mesh, algebra).ToFieldTensor();
        var omega = ConnectionField.Zero(mesh, algebra).ToFieldTensor();

        var gaugeMap = new InfinitesimalGaugeMap(mesh, algebra, a0, omega);
        var dOp = new CoulombGaugePenalty(mesh, algebra.Dimension, 0.0);

        // Test with a non-trivial xi
        int dimG = algebra.Dimension;
        var xiCoeffs = new double[mesh.VertexCount * dimG];
        xiCoeffs[0 * dimG + 0] = 1.0; // vertex 0, component 0
        xiCoeffs[1 * dimG + 1] = 0.5; // vertex 1, component 1
        xiCoeffs[2 * dimG + 2] = 0.3; // vertex 2, component 2

        var xi = new FieldTensor
        {
            Label = "xi",
            Signature = gaugeMap.InputSignature,
            Coefficients = xiCoeffs,
            Shape = new[] { mesh.VertexCount, dimG },
        };

        var rXi = gaugeMap.Apply(xi);
        var dXi = dOp.ApplyExteriorDerivative(xiCoeffs);

        // R(xi) should equal -d(xi) at flat background
        for (int i = 0; i < rXi.Coefficients.Length; i++)
        {
            Assert.Equal(-dXi[i], rXi.Coefficients[i], 10);
        }
    }

    [Fact]
    public void GaugeMap_Transpose_SatisfiesAdjointProperty()
    {
        // Test: <R*xi, w> == <xi, R^T*w>
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var a0 = ConnectionField.Zero(mesh, algebra).ToFieldTensor();

        // Non-zero background to test bracket term
        var omega = new ConnectionField(mesh, algebra);
        omega.SetEdgeValue(0, new[] { 0.3, 0.1, 0.0 });
        omega.SetEdgeValue(1, new[] { 0.0, 0.2, 0.1 });
        var omegaTensor = omega.ToFieldTensor();

        var gaugeMap = new InfinitesimalGaugeMap(mesh, algebra, a0, omegaTensor);

        var rng = new Random(42);
        int dimG = algebra.Dimension;

        // Random xi (vertex-valued)
        var xiCoeffs = new double[gaugeMap.InputDimension];
        for (int i = 0; i < xiCoeffs.Length; i++)
            xiCoeffs[i] = rng.NextDouble() * 2.0 - 1.0;
        var xi = new FieldTensor
        {
            Label = "xi",
            Signature = gaugeMap.InputSignature,
            Coefficients = xiCoeffs,
            Shape = new[] { mesh.VertexCount, dimG },
        };

        // Random w (edge-valued)
        var wCoeffs = new double[gaugeMap.OutputDimension];
        for (int i = 0; i < wCoeffs.Length; i++)
            wCoeffs[i] = rng.NextDouble() * 2.0 - 1.0;
        var w = new FieldTensor
        {
            Label = "w",
            Signature = gaugeMap.OutputSignature,
            Coefficients = wCoeffs,
            Shape = new[] { mesh.EdgeCount, dimG },
        };

        var rXi = gaugeMap.Apply(xi);
        var rtW = gaugeMap.ApplyTranspose(w);

        double lhs = Dot(rXi.Coefficients, w.Coefficients);
        double rhs = Dot(xi.Coefficients, rtW.Coefficients);

        Assert.Equal(lhs, rhs, 10);
    }

    [Fact]
    public void GaugeMap_ImageIsInJacobianNullSpace_AtFlatSolution()
    {
        // At omega=0 (a solution with Upsilon=0), gauge directions R(xi)
        // should be in the null space of J (since gauge transformations
        // preserve the solution). For flat connections: J*R(xi) = d(d(xi)) = 0
        // because d^2 = 0. This is a key structural property.
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
        var curvature = CurvatureAssembler.Assemble(omega);
        var jacobian = assembler.BuildJacobian(omega, a0, curvature.ToFieldTensor(), manifest, geometry);

        var workbench = new LinearizationWorkbench(mesh, algebra, assembler, massMatrix);
        var bg = workbench.CreateBackgroundState(
            "bg-null", omega.ToFieldTensor(), a0.ToFieldTensor(),
            manifest, geometry, solverConverged: true);
        var gaugeMap = workbench.BuildGaugeMap(bg);

        int dimG = algebra.Dimension;
        var xiCoeffs = new double[mesh.VertexCount * dimG];
        xiCoeffs[0] = 1.0;
        xiCoeffs[dimG + 1] = 0.5;

        var xi = new FieldTensor
        {
            Label = "xi",
            Signature = gaugeMap.InputSignature,
            Coefficients = xiCoeffs,
            Shape = new[] { mesh.VertexCount, dimG },
        };

        // R(xi) = -d(xi) at flat background
        var rXi = gaugeMap.Apply(xi);

        // J * R(xi) should be approximately zero (d^2 = 0 for exterior derivative)
        var jRxi = jacobian.Apply(rXi);
        double norm = 0;
        for (int i = 0; i < jRxi.Coefficients.Length; i++)
            norm += jRxi.Coefficients[i] * jRxi.Coefficients[i];
        norm = System.Math.Sqrt(norm);

        Assert.True(norm < 1e-10,
            $"J*R(xi) should be ~0 at flat background (d^2=0), got norm = {norm:E6}");
    }

    private static double Dot(double[] a, double[] b)
    {
        double sum = 0;
        for (int i = 0; i < a.Length; i++)
            sum += a[i] * b[i];
        return sum;
    }
}
