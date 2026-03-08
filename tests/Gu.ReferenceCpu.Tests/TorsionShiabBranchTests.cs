using Gu.Branching;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.ReferenceCpu;

namespace Gu.ReferenceCpu.Tests;

public class TorsionShiabBranchTests
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
        BranchId = "test-trivial",
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

    // ===== TrivialTorsionCpu Tests =====

    [Fact]
    public void TrivialTorsion_ReturnsZero()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var torsion = new TrivialTorsionCpu(mesh, algebra);

        var omega = ConnectionField.Zero(mesh, algebra).ToFieldTensor();
        var a0 = ConnectionField.Zero(mesh, algebra).ToFieldTensor();

        var result = torsion.Evaluate(omega, a0, TestManifest(), DummyGeometry());

        Assert.Equal(mesh.FaceCount * algebra.Dimension, result.Coefficients.Length);
        Assert.All(result.Coefficients, c => Assert.Equal(0.0, c, 12));
        Assert.Equal("curvature-2form", result.Signature.CarrierType);
    }

    [Fact]
    public void TrivialTorsion_Linearize_ReturnsZero()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var torsion = new TrivialTorsionCpu(mesh, algebra);

        var omega = ConnectionField.Zero(mesh, algebra).ToFieldTensor();
        var a0 = ConnectionField.Zero(mesh, algebra).ToFieldTensor();
        var delta = ConnectionField.Zero(mesh, algebra).ToFieldTensor();

        var result = torsion.Linearize(omega, a0, delta, TestManifest(), DummyGeometry());

        Assert.All(result.Coefficients, c => Assert.Equal(0.0, c, 12));
    }

    [Fact]
    public void TrivialTorsion_BranchIdAndCarrier()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var torsion = new TrivialTorsionCpu(mesh, algebra);

        Assert.Equal("trivial", torsion.BranchId);
        Assert.Equal("curvature-2form", torsion.OutputCarrierType);
    }

    [Fact]
    public void TrivialTorsion_CorrectShape_3D()
    {
        var mesh = SingleTetrahedron();
        var algebra = LieAlgebraFactory.CreateSu2();
        var torsion = new TrivialTorsionCpu(mesh, algebra);

        var omega = ConnectionField.Zero(mesh, algebra).ToFieldTensor();
        var a0 = ConnectionField.Zero(mesh, algebra).ToFieldTensor();

        var result = torsion.Evaluate(omega, a0, TestManifest(), DummyGeometry());

        // Tet has 4 faces, su(2) has dim 3
        Assert.Equal(4 * 3, result.Coefficients.Length);
        Assert.Equal(2, result.Shape.Count);
        Assert.Equal(4, result.Shape[0]);
        Assert.Equal(3, result.Shape[1]);
    }

    // ===== IdentityShiabCpu Tests =====

    [Fact]
    public void IdentityShiab_ReturnsCurvature()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var shiab = new IdentityShiabCpu(mesh, algebra);

        // Set up non-zero omega to get non-zero curvature
        var omega = new ConnectionField(mesh, algebra);
        omega.SetEdgeValue(0, new[] { 1.0, 0.0, 0.0 });
        omega.SetEdgeValue(1, new[] { 0.0, 1.0, 0.0 });
        omega.SetEdgeValue(2, new[] { 0.0, 0.0, 0.0 });

        var curvature = CurvatureAssembler.Assemble(omega);
        var curvatureTensor = curvature.ToFieldTensor();
        var omegaTensor = omega.ToFieldTensor();

        var result = shiab.Evaluate(curvatureTensor, omegaTensor, TestManifest(), DummyGeometry());

        // S = F, so coefficients should match
        Assert.Equal(curvatureTensor.Coefficients.Length, result.Coefficients.Length);
        for (int i = 0; i < result.Coefficients.Length; i++)
        {
            Assert.Equal(curvatureTensor.Coefficients[i], result.Coefficients[i], 12);
        }
    }

    [Fact]
    public void IdentityShiab_OutputCarrierIsCorrect()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var shiab = new IdentityShiabCpu(mesh, algebra);

        var omega = ConnectionField.Zero(mesh, algebra);
        var curvature = CurvatureAssembler.Assemble(omega);

        var result = shiab.Evaluate(
            curvature.ToFieldTensor(), omega.ToFieldTensor(), TestManifest(), DummyGeometry());

        Assert.Equal("curvature-2form", result.Signature.CarrierType);
        Assert.Equal("S_h", result.Label);
    }

    [Fact]
    public void IdentityShiab_BranchIdAndCarrier()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var shiab = new IdentityShiabCpu(mesh, algebra);

        Assert.Equal("identity-shiab", shiab.BranchId);
        Assert.Equal("curvature-2form", shiab.OutputCarrierType);
    }

    // ===== Carrier Match Validation =====

    [Fact]
    public void CarrierMatch_TrivialTorsionAndIdentityShiab_Pass()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var torsion = new TrivialTorsionCpu(mesh, algebra);
        var shiab = new IdentityShiabCpu(mesh, algebra);

        // Should not throw
        BranchOperatorRegistry.ValidateCarrierMatch(torsion, shiab);
    }

    // ===== Pipeline Test: Upsilon = S - T = F - 0 = F =====

    [Fact]
    public void Pipeline_TrivialTorsionAndIdentityShiab_UpsilonEqualsF()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();

        var torsion = new TrivialTorsionCpu(mesh, algebra);
        var shiab = new IdentityShiabCpu(mesh, algebra);

        // Validate carrier match
        BranchOperatorRegistry.ValidateCarrierMatch(torsion, shiab);

        // Set up non-trivial omega
        var omega = new ConnectionField(mesh, algebra);
        omega.SetEdgeValue(0, new[] { 1.0, 0.0, 0.0 });
        omega.SetEdgeValue(1, new[] { 0.0, 1.0, 0.0 });
        omega.SetEdgeValue(2, new[] { 0.0, 0.0, 1.0 });

        var omegaTensor = omega.ToFieldTensor();
        var a0 = ConnectionField.Zero(mesh, algebra).ToFieldTensor();
        var manifest = TestManifest();
        var geometry = DummyGeometry();

        // Compute curvature F
        var curvature = CurvatureAssembler.Assemble(omega);
        var curvatureTensor = curvature.ToFieldTensor();

        // Compute T_h = 0
        var tH = torsion.Evaluate(omegaTensor, a0, manifest, geometry);

        // Compute S_h = F
        var sH = shiab.Evaluate(curvatureTensor, omegaTensor, manifest, geometry);

        // Upsilon = S - T = F - 0 = F
        var upsilon = FieldTensorOps.Subtract(sH, tH);

        // Verify Upsilon equals F
        for (int i = 0; i < curvatureTensor.Coefficients.Length; i++)
        {
            Assert.Equal(curvatureTensor.Coefficients[i], upsilon.Coefficients[i], 12);
        }

        // Verify carrier type is correct
        Assert.Equal("curvature-2form", upsilon.Signature.CarrierType);
    }

    [Fact]
    public void Pipeline_FlatOmega_UpsilonIsZero()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();

        var torsion = new TrivialTorsionCpu(mesh, algebra);
        var shiab = new IdentityShiabCpu(mesh, algebra);

        var omega = ConnectionField.Zero(mesh, algebra);
        var omegaTensor = omega.ToFieldTensor();
        var a0 = omegaTensor;
        var manifest = TestManifest();
        var geometry = DummyGeometry();

        var curvature = CurvatureAssembler.Assemble(omega);
        var curvatureTensor = curvature.ToFieldTensor();

        var tH = torsion.Evaluate(omegaTensor, a0, manifest, geometry);
        var sH = shiab.Evaluate(curvatureTensor, omegaTensor, manifest, geometry);
        var upsilon = FieldTensorOps.Subtract(sH, tH);

        // Flat connection -> F = 0 -> S = 0, T = 0 -> Upsilon = 0
        Assert.All(upsilon.Coefficients, c => Assert.Equal(0.0, c, 12));
    }

    // ===== Linearization Test: Finite Difference Verification =====

    [Fact]
    public void IdentityShiab_Linearize_MatchesFiniteDifference()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var shiab = new IdentityShiabCpu(mesh, algebra);
        var manifest = TestManifest();
        var geometry = DummyGeometry();

        // Base point omega
        var omega = new ConnectionField(mesh, algebra);
        omega.SetEdgeValue(0, new[] { 0.5, 0.1, 0.0 });
        omega.SetEdgeValue(1, new[] { 0.0, 0.3, 0.2 });
        omega.SetEdgeValue(2, new[] { 0.1, 0.0, 0.4 });
        var omegaTensor = omega.ToFieldTensor();

        // Perturbation
        var delta = new ConnectionField(mesh, algebra);
        delta.SetEdgeValue(0, new[] { 0.1, 0.0, 0.0 });
        delta.SetEdgeValue(1, new[] { 0.0, 0.1, 0.0 });
        delta.SetEdgeValue(2, new[] { 0.0, 0.0, 0.1 });
        var deltaTensor = delta.ToFieldTensor();

        // Curvature at omega
        var curvature = CurvatureAssembler.Assemble(omega);
        var curvatureTensor = curvature.ToFieldTensor();

        // Exact linearization
        var dS = shiab.Linearize(curvatureTensor, omegaTensor, deltaTensor, manifest, geometry);

        // Finite difference: (S(omega + eps*delta) - S(omega)) / eps
        double eps = 1e-7;
        var omegaPerturbed = new ConnectionField(mesh, algebra);
        for (int i = 0; i < omega.Coefficients.Length; i++)
            omegaPerturbed.Coefficients[i] = omega.Coefficients[i] + eps * delta.Coefficients[i];

        var curvaturePerturbed = CurvatureAssembler.Assemble(omegaPerturbed);
        var sBase = shiab.Evaluate(curvatureTensor, omegaTensor, manifest, geometry);
        var sPerturbed = shiab.Evaluate(curvaturePerturbed.ToFieldTensor(), omegaPerturbed.ToFieldTensor(), manifest, geometry);

        // Compare (S_perturbed - S_base) / eps with dS
        for (int i = 0; i < dS.Coefficients.Length; i++)
        {
            double fdApprox = (sPerturbed.Coefficients[i] - sBase.Coefficients[i]) / eps;
            Assert.Equal(fdApprox, dS.Coefficients[i], 4); // 4 decimal places for FD
        }
    }

    [Fact]
    public void TrivialTorsion_Linearize_MatchesFiniteDifference()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var torsion = new TrivialTorsionCpu(mesh, algebra);
        var manifest = TestManifest();
        var geometry = DummyGeometry();

        var omega = new ConnectionField(mesh, algebra);
        omega.SetEdgeValue(0, new[] { 0.5, 0.1, 0.0 });
        var omegaTensor = omega.ToFieldTensor();
        var a0 = ConnectionField.Zero(mesh, algebra).ToFieldTensor();

        var delta = new ConnectionField(mesh, algebra);
        delta.SetEdgeValue(0, new[] { 0.1, 0.0, 0.0 });
        var deltaTensor = delta.ToFieldTensor();

        var dT = torsion.Linearize(omegaTensor, a0, deltaTensor, manifest, geometry);

        // Trivial torsion: T=0 everywhere, so dT=0 everywhere
        Assert.All(dT.Coefficients, c => Assert.Equal(0.0, c, 12));

        // FD: T(omega+eps*delta) - T(omega) = 0 - 0 = 0
        // Consistent with dT = 0
    }
}
