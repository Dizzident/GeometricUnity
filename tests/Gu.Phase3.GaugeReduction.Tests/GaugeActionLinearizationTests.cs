using Gu.Branching;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.Phase2.Stability;
using Gu.Phase3.GaugeReduction;
using Gu.ReferenceCpu;
using Gu.Solvers;

namespace Gu.Phase3.GaugeReduction.Tests;

public class GaugeActionLinearizationTests
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
        BranchId = "test-gauge-reduction",
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

    internal static BackgroundStateRecord CreateFlatBackground()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var torsion = new TrivialTorsionCpu(mesh, algebra);
        var shiab = new IdentityShiabCpu(mesh, algebra);
        var assembler = new CpuResidualAssembler(mesh, algebra, torsion, shiab);
        var massMatrix = new CpuMassMatrix(mesh, algebra);
        var workbench = new LinearizationWorkbench(mesh, algebra, assembler, massMatrix);
        var omega = ConnectionField.Zero(mesh, algebra).ToFieldTensor();
        var a0 = ConnectionField.Zero(mesh, algebra).ToFieldTensor();

        return workbench.CreateBackgroundState(
            "bg-flat", omega, a0, TestManifest(), DummyGeometry(), solverConverged: true);
    }

    [Fact]
    public void Linearization_Dimensions_AreCorrect()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var bg = CreateFlatBackground();

        var linearization = new GaugeActionLinearization(mesh, algebra, bg);

        // 3 vertices * 3 dimG = 9 gauge params, 3 edges * 3 dimG = 9 connection DOFs
        Assert.Equal(9, linearization.GaugeParameterDimension);
        Assert.Equal(9, linearization.ConnectionDimension);
        Assert.Equal("bg-flat", linearization.BackgroundId);
    }

    [Fact]
    public void Linearization_ExpectedGaugeRank_IsCorrect()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var bg = CreateFlatBackground();

        var linearization = new GaugeActionLinearization(mesh, algebra, bg);

        // For 3 vertices, dimG=3: expected rank = 3*(3-1) = 6
        Assert.Equal(6, linearization.ExpectedGaugeRank);
    }

    [Fact]
    public void Linearization_Apply_MatchesPhase2GaugeMap()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var bg = CreateFlatBackground();

        var linearization = new GaugeActionLinearization(mesh, algebra, bg);
        var phase2GaugeMap = new InfinitesimalGaugeMap(mesh, algebra, bg.A0, bg.Omega);

        int dimG = algebra.Dimension;
        var xiCoeffs = new double[mesh.VertexCount * dimG];
        xiCoeffs[0] = 1.0;
        xiCoeffs[dimG + 1] = 0.5;
        xiCoeffs[2 * dimG + 2] = -0.3;

        var xi = new FieldTensor
        {
            Label = "xi",
            Signature = phase2GaugeMap.InputSignature,
            Coefficients = xiCoeffs,
            Shape = new[] { mesh.VertexCount, dimG },
        };

        var result1 = linearization.Apply(xi);
        var result2 = phase2GaugeMap.Apply(xi);

        for (int i = 0; i < result1.Coefficients.Length; i++)
            Assert.Equal(result2.Coefficients[i], result1.Coefficients[i], 12);
    }

    [Fact]
    public void Linearization_Transpose_SatisfiesAdjointProperty()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var bg = CreateFlatBackground();

        var linearization = new GaugeActionLinearization(mesh, algebra, bg);
        var rng = new Random(42);

        var xiCoeffs = new double[linearization.GaugeParameterDimension];
        for (int i = 0; i < xiCoeffs.Length; i++)
            xiCoeffs[i] = rng.NextDouble() * 2.0 - 1.0;

        var wCoeffs = new double[linearization.ConnectionDimension];
        for (int i = 0; i < wCoeffs.Length; i++)
            wCoeffs[i] = rng.NextDouble() * 2.0 - 1.0;

        var xi = new FieldTensor
        {
            Label = "xi",
            Signature = linearization.Operator.InputSignature,
            Coefficients = xiCoeffs,
            Shape = new[] { xiCoeffs.Length },
        };

        var w = new FieldTensor
        {
            Label = "w",
            Signature = linearization.Operator.OutputSignature,
            Coefficients = wCoeffs,
            Shape = new[] { wCoeffs.Length },
        };

        var rXi = linearization.Apply(xi);
        var rtW = linearization.ApplyTranspose(w);

        double lhs = Dot(rXi.Coefficients, w.Coefficients);
        double rhs = Dot(xi.Coefficients, rtW.Coefficients);

        Assert.Equal(lhs, rhs, 10);
    }

    internal static double Dot(double[] a, double[] b)
    {
        double sum = 0;
        for (int i = 0; i < a.Length; i++)
            sum += a[i] * b[i];
        return sum;
    }
}
