using Gu.Branching;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.ReferenceCpu;

namespace Gu.Core.Tests;

public class TorsionShiabTests
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
            ActiveTorsionBranch = "trivial",
            ActiveShiabBranch = "identity-shiab",
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

    private static GeometryContext CreateGeometry(SimplicialMesh mesh)
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

    [Fact]
    public void TrivialTorsion_ReturnsZero()
    {
        var mesh = CreateMesh();
        var algebra = LieAlgebraFactory.CreateSu2();
        var torsion = new TrivialTorsionCpu(mesh, algebra);

        var omega = CreateConnectionField(mesh, algebra);
        var a0 = CreateConnectionField(mesh, algebra);
        var manifest = CreateManifest();
        var geometry = CreateGeometry(mesh);

        var result = torsion.Evaluate(omega, a0, manifest, geometry);

        Assert.Equal("T_h", result.Label);
        Assert.All(result.Coefficients, c => Assert.Equal(0.0, c, 12));
        Assert.Equal(mesh.FaceCount * algebra.Dimension, result.Coefficients.Length);
    }

    [Fact]
    public void TrivialTorsion_Linearization_IsZero()
    {
        var mesh = CreateMesh();
        var algebra = LieAlgebraFactory.CreateSu2();
        var torsion = new TrivialTorsionCpu(mesh, algebra);

        var omega = CreateConnectionField(mesh, algebra);
        var a0 = CreateConnectionField(mesh, algebra);
        var delta = CreateConnectionField(mesh, algebra);
        var manifest = CreateManifest();
        var geometry = CreateGeometry(mesh);

        var result = torsion.Linearize(omega, a0, delta, manifest, geometry);

        Assert.Equal("dT_h", result.Label);
        Assert.All(result.Coefficients, c => Assert.Equal(0.0, c, 12));
    }

    [Fact]
    public void IdentityShiab_ReturnsCurvature()
    {
        var mesh = CreateMesh();
        var algebra = LieAlgebraFactory.CreateSu2();
        var shiab = new IdentityShiabCpu(mesh, algebra);

        // Build a connection with curvature
        var connField = new ConnectionField(mesh, algebra);
        connField.SetEdgeValue(0, new double[] { 1.0, 0.0, 0.0 });
        connField.SetEdgeValue(1, new double[] { 0.0, 1.0, 0.0 });
        connField.SetEdgeValue(2, new double[] { 0.0, 0.0, 1.0 });

        var curvature = CurvatureAssembler.Assemble(connField);
        var curvatureFt = curvature.ToFieldTensor();
        var omegaFt = connField.ToFieldTensor();
        var manifest = CreateManifest();
        var geometry = CreateGeometry(mesh);

        var result = shiab.Evaluate(curvatureFt, omegaFt, manifest, geometry);

        Assert.Equal("S_h", result.Label);
        // S = F, so coefficients should match
        Assert.Equal(curvatureFt.Coefficients.Length, result.Coefficients.Length);
        for (int i = 0; i < curvatureFt.Coefficients.Length; i++)
        {
            Assert.Equal(curvatureFt.Coefficients[i], result.Coefficients[i], 12);
        }
    }

    [Fact]
    public void TorsionAndShiab_SameCarrierType()
    {
        var mesh = CreateMesh();
        var algebra = LieAlgebraFactory.CreateSu2();
        var torsion = new TrivialTorsionCpu(mesh, algebra);
        var shiab = new IdentityShiabCpu(mesh, algebra);

        Assert.Equal(torsion.OutputCarrierType, shiab.OutputCarrierType);
        Assert.Equal("curvature-2form", torsion.OutputCarrierType);
    }

    [Fact]
    public void Upsilon_EqualsSMinusT_WithTrivialT()
    {
        // With trivial torsion (T=0), Upsilon = S - T = S = F
        var mesh = CreateMesh();
        var algebra = LieAlgebraFactory.CreateSu2();
        var torsion = new TrivialTorsionCpu(mesh, algebra);
        var shiab = new IdentityShiabCpu(mesh, algebra);

        var connField = new ConnectionField(mesh, algebra);
        connField.SetEdgeValue(0, new double[] { 1.0, 0.0, 0.0 });
        connField.SetEdgeValue(1, new double[] { 0.0, 1.0, 0.0 });

        var curvature = CurvatureAssembler.Assemble(connField);
        var curvatureFt = curvature.ToFieldTensor();
        var omegaFt = connField.ToFieldTensor();
        var a0 = ConnectionField.Zero(mesh, algebra).ToFieldTensor();
        var manifest = CreateManifest();
        var geometry = CreateGeometry(mesh);

        var tResult = torsion.Evaluate(omegaFt, a0, manifest, geometry);
        var sResult = shiab.Evaluate(curvatureFt, omegaFt, manifest, geometry);

        // Upsilon = S - T
        var upsilon = FieldTensorOps.Subtract(sResult, tResult);

        // With T=0 and S=F, Upsilon = F
        for (int i = 0; i < curvatureFt.Coefficients.Length; i++)
        {
            Assert.Equal(curvatureFt.Coefficients[i], upsilon.Coefficients[i], 12);
        }
    }

    [Fact]
    public void IdentityShiab_ClonesCoefficients()
    {
        var mesh = CreateMesh();
        var algebra = LieAlgebraFactory.CreateSu2();
        var shiab = new IdentityShiabCpu(mesh, algebra);

        var curvatureFt = new FieldTensor
        {
            Label = "F_h",
            Signature = new TensorSignature
            {
                AmbientSpaceId = "Y_h",
                CarrierType = "curvature-2form",
                Degree = "2",
                LieAlgebraBasisId = "canonical",
                ComponentOrderId = "face-major",
                MemoryLayout = "dense-row-major",
            },
            Coefficients = new double[] { 1.0, 2.0, 3.0 },
            Shape = new[] { 1, 3 },
        };
        var omegaFt = CreateConnectionField(mesh, algebra);
        var manifest = CreateManifest();
        var geometry = CreateGeometry(mesh);

        var result = shiab.Evaluate(curvatureFt, omegaFt, manifest, geometry);

        // Mutating input should not affect output
        curvatureFt.Coefficients[0] = 999.0;
        Assert.Equal(1.0, result.Coefficients[0]);
    }

    private static FieldTensor CreateConnectionField(SimplicialMesh mesh, LieAlgebra algebra)
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
            Coefficients = new double[mesh.EdgeCount * algebra.Dimension],
            Shape = new[] { mesh.EdgeCount, algebra.Dimension },
        };
    }
}
