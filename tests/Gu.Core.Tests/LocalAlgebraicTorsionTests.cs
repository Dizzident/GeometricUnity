using Gu.Branching;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.ReferenceCpu;

namespace Gu.Core.Tests;

public class LocalAlgebraicTorsionTests
{
    private static SimplicialMesh CreateMesh()
    {
        return MeshTopologyBuilder.Build(
            embeddingDimension: 2,
            simplicialDimension: 2,
            vertexCoordinates: new double[] { 0, 0, 1, 0, 0, 1 },
            vertexCount: 3,
            cellVertices: new[] { new[] { 0, 1, 2 } });
    }

    private static BranchManifest CreateManifest()
    {
        return new BranchManifest
        {
            BranchId = "test",
            SchemaVersion = "1.0.0",
            SourceEquationRevision = "rev-1",
            CodeRevision = "abc123",
            ActiveGeometryBranch = "simplicial-2d",
            ActiveObservationBranch = "sigma-pullback",
            ActiveTorsionBranch = "local-algebraic",
            ActiveShiabBranch = "first-order-curvature",
            ActiveGaugeStrategy = "penalty",
            BaseDimension = 2,
            AmbientDimension = 5,
            LieAlgebraId = "su2",
            BasisConventionId = "basis-standard",
            ComponentOrderId = "order-row-major",
            AdjointConventionId = "adjoint-explicit",
            PairingConventionId = "pairing-killing",
            NormConventionId = "norm-l2-quadrature",
            DifferentialFormMetricId = "hodge-standard",
            InsertedAssumptionIds = Array.Empty<string>(),
            InsertedChoiceIds = Array.Empty<string>(),
        };
    }

    private static GeometryContext CreateGeometry()
    {
        return new GeometryContext
        {
            BaseSpace = new SpaceRef { SpaceId = "X_h", Dimension = 2 },
            AmbientSpace = new SpaceRef { SpaceId = "Y_h", Dimension = 2 },
            ProjectionBinding = new GeometryBinding
            {
                BindingType = "projection",
                SourceSpace = new SpaceRef { SpaceId = "Y_h", Dimension = 2 },
                TargetSpace = new SpaceRef { SpaceId = "X_h", Dimension = 2 },
            },
            ObservationBinding = new GeometryBinding
            {
                BindingType = "observation",
                SourceSpace = new SpaceRef { SpaceId = "X_h", Dimension = 2 },
                TargetSpace = new SpaceRef { SpaceId = "Y_h", Dimension = 2 },
            },
            DiscretizationType = "simplicial",
            QuadratureRuleId = "centroid",
            BasisFamilyId = "P1",
            Patches = Array.Empty<PatchInfo>(),
        };
    }

    private static FieldTensor MakeEdgeField(SimplicialMesh mesh, LieAlgebra algebra, double[] coeffs)
    {
        return new FieldTensor
        {
            Label = "omega_h",
            Signature = new TensorSignature
            {
                AmbientSpaceId = "Y_h",
                CarrierType = "connection-1form",
                Degree = "1",
                LieAlgebraBasisId = algebra.BasisOrderId,
                ComponentOrderId = "edge-major",
                MemoryLayout = "dense-row-major",
            },
            Coefficients = coeffs,
            Shape = new[] { mesh.EdgeCount, algebra.Dimension },
        };
    }

    [Fact]
    public void ZeroOmega_ZeroA0_GivesZeroTorsion()
    {
        // T = sum [A(e_i), B(e_j)] where A = A0+omega = 0, B = A0-omega = 0
        var mesh = CreateMesh();
        var algebra = LieAlgebraFactory.CreateSu2();
        var op = new LocalAlgebraicTorsionOperator(mesh, algebra);

        int n = mesh.EdgeCount * algebra.Dimension;
        var omega = MakeEdgeField(mesh, algebra, new double[n]);
        var a0 = MakeEdgeField(mesh, algebra, new double[n]);

        var result = op.Evaluate(omega, a0, CreateManifest(), CreateGeometry());

        Assert.Equal("T_h", result.Label);
        Assert.All(result.Coefficients, c => Assert.Equal(0.0, c, 12));
    }

    [Fact]
    public void AbelianAlgebra_GivesZeroTorsion()
    {
        // For abelian algebra, all brackets are zero -> T = 0
        var mesh = CreateMesh();
        var algebra = LieAlgebraFactory.CreateAbelian(3);
        var op = new LocalAlgebraicTorsionOperator(mesh, algebra);

        int n = mesh.EdgeCount * algebra.Dimension;
        var omegaCoeffs = new double[n];
        for (int i = 0; i < n; i++) omegaCoeffs[i] = (i + 1) * 0.1;
        var omega = MakeEdgeField(mesh, algebra, omegaCoeffs);
        var a0 = MakeEdgeField(mesh, algebra, new double[n]);

        var result = op.Evaluate(omega, a0, CreateManifest(), CreateGeometry());

        Assert.All(result.Coefficients, c => Assert.Equal(0.0, c, 12));
    }

    [Fact]
    public void NonAbelian_NonZeroOmega_ProducesNonZeroTorsion()
    {
        var mesh = CreateMesh();
        var algebra = LieAlgebraFactory.CreateSu2();
        var op = new LocalAlgebraicTorsionOperator(mesh, algebra);

        int n = mesh.EdgeCount * algebra.Dimension;
        var omegaCoeffs = new double[n];
        // Set different algebra directions on different edges
        omegaCoeffs[0] = 1.0; // edge 0, T_1
        omegaCoeffs[4] = 1.0; // edge 1, T_2
        var omega = MakeEdgeField(mesh, algebra, omegaCoeffs);
        var a0 = MakeEdgeField(mesh, algebra, new double[n]);

        var result = op.Evaluate(omega, a0, CreateManifest(), CreateGeometry());

        // With non-zero omega on non-abelian algebra, [A(e_i), B(e_j)] should be non-zero
        bool hasNonZero = result.Coefficients.Any(c => System.Math.Abs(c) > 1e-12);
        Assert.True(hasNonZero, "Non-abelian torsion with non-zero omega should produce non-zero T_h.");
    }

    [Fact]
    public void CarrierType_MatchesFirstOrderShiab()
    {
        var mesh = CreateMesh();
        var algebra = LieAlgebraFactory.CreateSu2();
        var torsion = new LocalAlgebraicTorsionOperator(mesh, algebra);
        var shiab = new FirstOrderShiabOperator(mesh, algebra);

        Assert.Equal(torsion.OutputCarrierType, shiab.OutputCarrierType);
        Assert.Equal("curvature-2form", torsion.OutputCarrierType);
    }

    [Fact]
    public void BranchId_IsLocalAlgebraic()
    {
        var mesh = CreateMesh();
        var algebra = LieAlgebraFactory.CreateSu2();
        var op = new LocalAlgebraicTorsionOperator(mesh, algebra);

        Assert.Equal("local-algebraic", op.BranchId);
    }

    [Fact]
    public void Linearization_AtZero_IsZero()
    {
        // At omega = 0 and A0 = 0: A = 0, B = 0
        // dT/domega * delta = sum [delta_i, B_j] - [A_i, delta_j] = 0 - 0 = 0
        var mesh = CreateMesh();
        var algebra = LieAlgebraFactory.CreateSu2();
        var op = new LocalAlgebraicTorsionOperator(mesh, algebra);

        int n = mesh.EdgeCount * algebra.Dimension;
        var omega = MakeEdgeField(mesh, algebra, new double[n]);
        var a0 = MakeEdgeField(mesh, algebra, new double[n]);
        var deltaCoeffs = new double[n];
        deltaCoeffs[0] = 1.0;
        var delta = MakeEdgeField(mesh, algebra, deltaCoeffs);

        var result = op.Linearize(omega, a0, delta, CreateManifest(), CreateGeometry());

        Assert.Equal("dT_h", result.Label);
        Assert.All(result.Coefficients, c => Assert.Equal(0.0, c, 12));
    }
}
