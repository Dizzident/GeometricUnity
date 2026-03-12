using Gu.Branching;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.ReferenceCpu;

namespace Gu.TheoryConformance.Tests;

/// <summary>
/// Shared test helpers for theory-conformance tests.
/// </summary>
internal static class TestHelpers
{
    // Known branch IDs as declared in ASSUMPTIONS.md and typical manifests
    internal const string DefaultTorsionBranch = "trivial";
    internal const string DefaultShiabBranch = "identity-shiab";
    internal const string DefaultObservationBranch = "sigma-pullback";
    internal const string DefaultGeometryBranch = "simplicial-toy-2d";
    internal const string DefaultLieAlgebraId = "su2";
    internal const string DefaultPairingConvention = "trace";

    internal static BranchManifest CreateConformingManifest(
        string branchId = "test-branch",
        string torsionBranch = DefaultTorsionBranch,
        string shiabBranch = DefaultShiabBranch,
        string observationBranch = DefaultObservationBranch,
        string geometryBranch = DefaultGeometryBranch,
        string pairingConvention = DefaultPairingConvention) => new()
    {
        BranchId = branchId,
        SchemaVersion = "1.0",
        SourceEquationRevision = "rev-1",
        CodeRevision = "test-abc123",
        ActiveGeometryBranch = geometryBranch,
        ActiveObservationBranch = observationBranch,
        ActiveTorsionBranch = torsionBranch,
        ActiveShiabBranch = shiabBranch,
        ActiveGaugeStrategy = "penalty",
        BaseDimension = 2,
        AmbientDimension = 5,
        LieAlgebraId = DefaultLieAlgebraId,
        BasisConventionId = "standard",
        ComponentOrderId = "lexicographic",
        AdjointConventionId = "physicist",
        PairingConventionId = pairingConvention,
        NormConventionId = "l2",
        DifferentialFormMetricId = "flat",
        InsertedAssumptionIds = new[] { "A-003", "A-004", "A-009", "A-011" },
        InsertedChoiceIds = new[] { "IX-1", "IX-2" },
    };

    internal static ProvenanceMeta CreateMatchingProvenance(BranchManifest manifest) => new()
    {
        CreatedAt = DateTimeOffset.UtcNow,
        CodeRevision = manifest.CodeRevision,
        Branch = new BranchRef { BranchId = manifest.BranchId, SchemaVersion = manifest.SchemaVersion },
        Backend = "cpu-reference",
    };

    internal static ProvenanceMeta CreateMismatchedProvenance(BranchManifest manifest) => new()
    {
        CreatedAt = DateTimeOffset.UtcNow,
        CodeRevision = manifest.CodeRevision,
        Branch = new BranchRef { BranchId = "different-branch-id", SchemaVersion = manifest.SchemaVersion },
        Backend = "cpu-reference",
    };

    internal static (SimplicialMesh mesh, LieAlgebra algebra) CreateMinimalSetup()
    {
        var mesh = MeshTopologyBuilder.Build(
            embeddingDimension: 2,
            simplicialDimension: 2,
            vertexCoordinates: new double[] { 0, 0, 1, 0, 0, 1 },
            vertexCount: 3,
            cellVertices: new[] { new[] { 0, 1, 2 } });

        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        return (mesh, algebra);
    }

    internal static ITorsionBranchOperator CreateTrivialTorsion()
    {
        var (mesh, algebra) = CreateMinimalSetup();
        return new TrivialTorsionCpu(mesh, algebra);
    }

    internal static ITorsionBranchOperator CreateLocalAlgebraicTorsion()
    {
        var (mesh, algebra) = CreateMinimalSetup();
        return new LocalAlgebraicTorsionOperator(mesh, algebra);
    }

    internal static IShiabBranchOperator CreateIdentityShiab()
    {
        var (mesh, algebra) = CreateMinimalSetup();
        return new IdentityShiabCpu(mesh, algebra);
    }

    internal static IShiabBranchOperator CreateFirstOrderShiab()
    {
        var (mesh, algebra) = CreateMinimalSetup();
        return new FirstOrderShiabOperator(mesh, algebra);
    }

    internal static double[] CreateZeroOmega(int edgeCount, int dimG)
        => new double[edgeCount * dimG];

    internal static double[] CreateNonZeroOmega(int edgeCount, int dimG)
    {
        var omega = new double[edgeCount * dimG];
        var rng = new Random(42);
        for (int i = 0; i < omega.Length; i++)
            omega[i] = rng.NextDouble() * 0.1;
        return omega;
    }
}
