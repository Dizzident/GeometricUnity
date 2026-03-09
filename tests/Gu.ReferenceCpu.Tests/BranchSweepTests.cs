using Gu.Branching;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.ReferenceCpu;
using Gu.Solvers;

namespace Gu.ReferenceCpu.Tests;

/// <summary>
/// Tests for BranchSweepRunner (standalone, backend-agnostic sweep runner)
/// and cross-branch physics difference verification (GAP-9, Section 19.1).
/// </summary>
public class BranchSweepRunnerTests
{
    private static SimplicialMesh SingleTriangle() =>
        MeshTopologyBuilder.Build(
            embeddingDimension: 2,
            simplicialDimension: 2,
            vertexCoordinates: new double[] { 0, 0, 1, 0, 0, 1 },
            vertexCount: 3,
            cellVertices: new[] { new[] { 0, 1, 2 } });

    private static BranchManifest MakeManifest(string branchId, string torsionBranch) => new()
    {
        BranchId = branchId,
        SchemaVersion = "1.0.0",
        SourceEquationRevision = "r1",
        CodeRevision = "test-sweep",
        ActiveGeometryBranch = "simplicial",
        ActiveObservationBranch = "sigma-pullback",
        ActiveTorsionBranch = torsionBranch,
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

    private static GeometryContext TestGeometry() => new()
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

    // ===== BranchSweepRunner structure tests =====

    [Fact]
    public void Sweep_ProducesCorrectEntryCount()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var backend = new CpuSolverBackend(mesh, algebra,
            new TrivialTorsionCpu(mesh, algebra),
            new IdentityShiabCpu(mesh, algebra));

        var runner = new BranchSweepRunner(backend,
            new SolverOptions { Mode = SolveMode.ResidualOnly });

        var manifests = new[]
        {
            MakeManifest("branch-a", "trivial"),
            MakeManifest("branch-b", "augmented-torsion"),
        };

        var omega = ConnectionField.Zero(mesh, algebra).ToFieldTensor();
        var a0 = ConnectionField.Zero(mesh, algebra).ToFieldTensor();

        var result = runner.Sweep("test-env", omega, a0, manifests, TestGeometry());

        Assert.Equal(2, result.BranchCount);
        Assert.Equal(SolveMode.ResidualOnly, result.InnerMode);
    }

    [Fact]
    public void Sweep_EachEntryHasProvenanceTaggedArtifact()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var backend = new CpuSolverBackend(mesh, algebra,
            new TrivialTorsionCpu(mesh, algebra),
            new IdentityShiabCpu(mesh, algebra));

        var runner = new BranchSweepRunner(backend,
            new SolverOptions { Mode = SolveMode.ResidualOnly });

        var manifests = new[]
        {
            MakeManifest("branch-alpha", "trivial"),
            MakeManifest("branch-beta", "augmented-torsion"),
        };

        var omega = ConnectionField.Zero(mesh, algebra).ToFieldTensor();
        var a0 = ConnectionField.Zero(mesh, algebra).ToFieldTensor();

        var result = runner.Sweep("env-prov", omega, a0, manifests, TestGeometry());

        // Each entry should have properly tagged artifact
        Assert.Equal("branch-alpha", result.Entries[0].ArtifactBundle.Branch.BranchId);
        Assert.Equal("branch-beta", result.Entries[1].ArtifactBundle.Branch.BranchId);

        Assert.Equal("branch-alpha",
            result.Entries[0].ArtifactBundle.ReplayContract.BranchManifest.BranchId);
        Assert.Equal("branch-beta",
            result.Entries[1].ArtifactBundle.ReplayContract.BranchManifest.BranchId);

        // Provenance notes should reference the environment ID
        Assert.Contains("env-prov", result.Entries[0].ArtifactBundle.Provenance.Notes);
        Assert.Contains("env-prov", result.Entries[1].ArtifactBundle.Provenance.Notes);

        // Artifact IDs should be distinct
        Assert.NotEqual(
            result.Entries[0].ArtifactBundle.ArtifactId,
            result.Entries[1].ArtifactBundle.ArtifactId);
    }

    [Fact]
    public void Sweep_RequiresAtLeastTwoBranches()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var backend = new CpuSolverBackend(mesh, algebra,
            new TrivialTorsionCpu(mesh, algebra),
            new IdentityShiabCpu(mesh, algebra));

        var runner = new BranchSweepRunner(backend,
            new SolverOptions { Mode = SolveMode.ResidualOnly });

        var omega = ConnectionField.Zero(mesh, algebra).ToFieldTensor();
        var a0 = ConnectionField.Zero(mesh, algebra).ToFieldTensor();

        Assert.Throws<ArgumentException>(() =>
            runner.Sweep("test", omega, a0,
                new[] { MakeManifest("only-one", "trivial") },
                TestGeometry()));
    }

    [Fact]
    public void Constructor_RejectsInnerModeBranchSensitivity()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var backend = new CpuSolverBackend(mesh, algebra,
            new TrivialTorsionCpu(mesh, algebra),
            new IdentityShiabCpu(mesh, algebra));

        Assert.Throws<ArgumentException>(() =>
            new BranchSweepRunner(backend,
                new SolverOptions { Mode = SolveMode.BranchSensitivity }));
    }

    // ===== Cross-branch physics difference (Section 19.1) =====

    [Fact]
    public void DifferentTorsionBranches_ProduceDifferentResiduals()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var geometry = TestGeometry();
        var manifest = MakeManifest("compare-torsion", "trivial");

        // Pipeline 1: Trivial torsion (T = 0, so Upsilon = F)
        var pipeline1 = new CpuSolverPipeline(mesh, algebra,
            new TrivialTorsionCpu(mesh, algebra),
            new IdentityShiabCpu(mesh, algebra));

        // Pipeline 2: Augmented torsion (T = d_{A0}(omega - A0))
        var pipeline2 = new CpuSolverPipeline(mesh, algebra,
            new AugmentedTorsionCpu(mesh, algebra),
            new IdentityShiabCpu(mesh, algebra));

        // Non-zero omega for non-trivial physics
        var rng = new Random(42);
        var coeffs = new double[mesh.EdgeCount * algebra.Dimension];
        for (int i = 0; i < coeffs.Length; i++)
            coeffs[i] = rng.NextDouble() * 2.0 - 1.0;

        var options = new SolverOptions { Mode = SolveMode.ResidualOnly };

        var result1 = pipeline1.ExecuteFromCoefficients(coeffs, manifest, geometry, options);
        var result2 = pipeline2.ExecuteFromCoefficients(coeffs, manifest, geometry, options);

        var r1 = result1.SolverResult.FinalDerivedState.ResidualUpsilon.Coefficients;
        var r2 = result2.SolverResult.FinalDerivedState.ResidualUpsilon.Coefficients;

        Assert.Equal(r1.Length, r2.Length);

        // Residuals must differ for non-zero omega
        bool residualDiffers = false;
        for (int i = 0; i < r1.Length; i++)
        {
            if (System.Math.Abs(r1[i] - r2[i]) > 1e-14)
            {
                residualDiffers = true;
                break;
            }
        }

        Assert.True(residualDiffers,
            "Changing torsion branch (trivial vs augmented) should produce " +
            "different residuals for non-zero omega");

        // Objectives should also differ
        Assert.NotEqual(
            result1.SolverResult.FinalObjective,
            result2.SolverResult.FinalObjective);

        // Both should have proper provenance
        Assert.Equal("cpu-reference", result1.ArtifactBundle.Provenance.Backend);
        Assert.Equal("cpu-reference", result2.ArtifactBundle.Provenance.Backend);
    }
}
