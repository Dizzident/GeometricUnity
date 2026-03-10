using Gu.Branching;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.Phase3.GaugeReduction;
using Gu.Phase3.Spectra;
using Gu.ReferenceCpu;

namespace Gu.Phase3.Properties.Tests;

internal static class TestHelpers
{
    public static SimplicialMesh SingleTriangle() =>
        MeshTopologyBuilder.Build(
            embeddingDimension: 2,
            simplicialDimension: 2,
            vertexCoordinates: new double[] { 0, 0, 1, 0, 0, 1 },
            vertexCount: 3,
            cellVertices: new[] { new[] { 0, 1, 2 } });

    public static LieAlgebra TracePairingSu2() =>
        LieAlgebraFactory.CreateSu2WithTracePairing();

    public static BranchManifest TestManifest() => new()
    {
        BranchId = "test-props",
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

    public static GeometryContext DummyGeometry() => new()
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

    public static TensorSignature ConnectionSignature() => new()
    {
        AmbientSpaceId = "Y_h",
        CarrierType = "connection-1form",
        Degree = "1",
        LieAlgebraBasisId = "standard",
        ComponentOrderId = "edge-major",
        NumericPrecision = "float64",
        MemoryLayout = "dense-row-major",
    };

    public static ModeRecord MakeMode(
        string modeId,
        double eigenvalue,
        double[] modeVector,
        string backgroundId = "bg-1",
        double gaugeLeakScore = 0.01,
        SpectralOperatorType opType = SpectralOperatorType.GaussNewton,
        string? clusterId = null)
    {
        return new ModeRecord
        {
            ModeId = modeId,
            BackgroundId = backgroundId,
            OperatorType = opType,
            Eigenvalue = eigenvalue,
            ResidualNorm = 1e-10,
            NormalizationConvention = "unit-M-norm",
            MultiplicityClusterId = clusterId,
            GaugeLeakScore = gaugeLeakScore,
            ModeVector = modeVector,
            ModeIndex = 0,
        };
    }

    public static ModeRecord MakeMode(string modeId, double eigenvalue, int dim, Random rng,
        string backgroundId = "bg-1")
    {
        var v = new double[dim];
        for (int i = 0; i < dim; i++)
            v[i] = rng.NextDouble() * 2 - 1;
        // Normalize
        double norm = 0;
        for (int i = 0; i < dim; i++) norm += v[i] * v[i];
        norm = System.Math.Sqrt(norm);
        for (int i = 0; i < dim; i++) v[i] /= norm;

        return MakeMode(modeId, eigenvalue, v, backgroundId);
    }

    public static GaugeProjector BuildGaugeProjector(SimplicialMesh mesh, LieAlgebra algebra)
    {
        var omega = ConnectionField.Zero(mesh, algebra);
        var a0 = ConnectionField.Zero(mesh, algebra);
        var linearization = new GaugeActionLinearization(
            mesh, algebra, a0.ToFieldTensor(), omega.ToFieldTensor(), "test-bg");
        return new GaugeProjector(GaugeBasis.Build(linearization));
    }
}
