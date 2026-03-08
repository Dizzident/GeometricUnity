using Gu.Branching;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.ReferenceCpu;

namespace Gu.ReferenceCpu.Tests;

public class SimpleBiConnectionCpuTests
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
        BranchId = "test",
        SchemaVersion = "1.0.0",
        SourceEquationRevision = "r1",
        CodeRevision = "test",
        ActiveGeometryBranch = "simplicial",
        ActiveObservationBranch = "sigma-pullback",
        ActiveTorsionBranch = "trivial",
        ActiveShiabBranch = "identity-shiab",
        ActiveGaugeStrategy = "penalty",
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
        InsertedChoiceIds = Array.Empty<string>(),
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
    public void StrategyId_IsCorrect()
    {
        var strategy = new SimpleBiConnectionCpu();
        Assert.Equal("simple-a0-omega", strategy.StrategyId);
    }

    [Fact]
    public void Evaluate_A_EqualsA0_B_EqualsOmega()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var strategy = new SimpleBiConnectionCpu();

        var omega = new ConnectionField(mesh, algebra);
        omega.SetEdgeValue(0, new[] { 1.0, 2.0, 3.0 });
        omega.SetEdgeValue(1, new[] { 4.0, 5.0, 6.0 });
        omega.SetEdgeValue(2, new[] { 7.0, 8.0, 9.0 });

        var a0 = new ConnectionField(mesh, algebra);
        a0.SetEdgeValue(0, new[] { 0.1, 0.2, 0.3 });

        var result = strategy.Evaluate(
            omega.ToFieldTensor(), a0.ToFieldTensor(), TestManifest(), DummyGeometry());

        // A = A0
        for (int i = 0; i < a0.Coefficients.Length; i++)
            Assert.Equal(a0.Coefficients[i], result.ConnectionA.Coefficients[i], 12);

        // B = omega
        for (int i = 0; i < omega.Coefficients.Length; i++)
            Assert.Equal(omega.Coefficients[i], result.ConnectionB.Coefficients[i], 12);
    }

    [Fact]
    public void Linearize_dA_IsZero_dB_IsDeltaOmega()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var strategy = new SimpleBiConnectionCpu();

        var omega = new ConnectionField(mesh, algebra);
        omega.SetEdgeValue(0, new[] { 1.0, 0.0, 0.0 });

        var a0 = ConnectionField.Zero(mesh, algebra);

        var delta = new ConnectionField(mesh, algebra);
        delta.SetEdgeValue(0, new[] { 0.1, 0.2, 0.3 });
        delta.SetEdgeValue(1, new[] { 0.4, 0.5, 0.6 });

        var result = strategy.Linearize(
            omega.ToFieldTensor(), a0.ToFieldTensor(), delta.ToFieldTensor(),
            TestManifest(), DummyGeometry());

        // dA = 0
        Assert.All(result.ConnectionA.Coefficients, c => Assert.Equal(0.0, c, 12));

        // dB = delta_omega
        for (int i = 0; i < delta.Coefficients.Length; i++)
            Assert.Equal(delta.Coefficients[i], result.ConnectionB.Coefficients[i], 12);
    }

    [Fact]
    public void Evaluate_DoesNotMutateInputs()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var strategy = new SimpleBiConnectionCpu();

        var omega = new ConnectionField(mesh, algebra);
        omega.SetEdgeValue(0, new[] { 1.0, 2.0, 3.0 });
        var omegaTensor = omega.ToFieldTensor();
        var originalOmegaCoeffs = (double[])omegaTensor.Coefficients.Clone();

        var a0 = ConnectionField.Zero(mesh, algebra).ToFieldTensor();
        var originalA0Coeffs = (double[])a0.Coefficients.Clone();

        strategy.Evaluate(omegaTensor, a0, TestManifest(), DummyGeometry());

        // Verify inputs are not mutated
        for (int i = 0; i < omegaTensor.Coefficients.Length; i++)
            Assert.Equal(originalOmegaCoeffs[i], omegaTensor.Coefficients[i], 12);
        for (int i = 0; i < a0.Coefficients.Length; i++)
            Assert.Equal(originalA0Coeffs[i], a0.Coefficients[i], 12);
    }
}
