using System.Text.Json;
using Gu.Core;

namespace Gu.TheoryConformance.Tests;

/// <summary>
/// Tests that verify the branch-identity conformance checks:
/// the runtime torsion, Shiab, pairing, observation, and geometry branch IDs
/// must match the declared branch manifest.
///
/// All checks here are branch-local. Passing means internal consistency,
/// not that the branch is canonical or that outputs are physically correct.
/// </summary>
public class BranchIdentityConformanceTests
{
    private readonly ConformanceEvaluator _evaluator = new();

    [Fact]
    public void Conforming_Run_Passes_All_BranchIdentity_Checks()
    {
        var manifest = TestHelpers.CreateConformingManifest();
        var torsion = TestHelpers.CreateTrivialTorsion();
        var shiab = TestHelpers.CreateIdentityShiab();
        var provenance = TestHelpers.CreateMatchingProvenance(manifest);

        var artifact = _evaluator.Evaluate(
            manifest, torsion, shiab, provenance,
            observedBranchId: manifest.ActiveObservationBranch,
            geometryBranchId: manifest.ActiveGeometryBranch);

        var identityChecks = artifact.Checks
            .Where(c => c.Category == "branch-identity")
            .ToList();

        Assert.All(identityChecks, c => Assert.True(c.Passed, $"[{c.CheckId}] {c.Detail}"));
    }

    [Fact]
    public void Torsion_BranchId_Mismatch_Fails_Check()
    {
        // Manifest declares "trivial" torsion, but we use local-algebraic at runtime.
        var manifest = TestHelpers.CreateConformingManifest(torsionBranch: "trivial");
        var wrongTorsion = TestHelpers.CreateLocalAlgebraicTorsion(); // BranchId = "local-algebraic"
        var shiab = TestHelpers.CreateIdentityShiab();
        var provenance = TestHelpers.CreateMatchingProvenance(manifest);

        var artifact = _evaluator.Evaluate(
            manifest, wrongTorsion, shiab, provenance,
            observedBranchId: manifest.ActiveObservationBranch,
            geometryBranchId: manifest.ActiveGeometryBranch);

        Assert.False(artifact.OverallPass);
        var check = artifact.Checks.Single(c => c.CheckId == "torsion-branch-id-match");
        Assert.False(check.Passed);
        Assert.Contains("local-algebraic", check.Detail);
        Assert.Contains("trivial", check.Detail);
    }

    [Fact]
    public void Shiab_BranchId_Mismatch_Fails_Check()
    {
        // Manifest declares "identity-shiab", but we use first-order at runtime.
        var manifest = TestHelpers.CreateConformingManifest(shiabBranch: "identity-shiab");
        var torsion = TestHelpers.CreateTrivialTorsion();
        var wrongShiab = TestHelpers.CreateFirstOrderShiab(); // BranchId = "first-order-curvature"
        var provenance = TestHelpers.CreateMatchingProvenance(manifest);

        var artifact = _evaluator.Evaluate(
            manifest, torsion, wrongShiab, provenance,
            observedBranchId: manifest.ActiveObservationBranch,
            geometryBranchId: manifest.ActiveGeometryBranch);

        Assert.False(artifact.OverallPass);
        var check = artifact.Checks.Single(c => c.CheckId == "shiab-branch-id-match");
        Assert.False(check.Passed);
        Assert.Contains("first-order-curvature", check.Detail);
        Assert.Contains("identity-shiab", check.Detail);
    }

    [Fact]
    public void Provenance_BranchId_Mismatch_Fails_Check()
    {
        var manifest = TestHelpers.CreateConformingManifest(branchId: "branch-A");
        var torsion = TestHelpers.CreateTrivialTorsion();
        var shiab = TestHelpers.CreateIdentityShiab();
        // Provenance says "different-branch-id" instead of "branch-A"
        var mismatchedProvenance = TestHelpers.CreateMismatchedProvenance(manifest);

        var artifact = _evaluator.Evaluate(
            manifest, torsion, shiab, mismatchedProvenance,
            observedBranchId: manifest.ActiveObservationBranch,
            geometryBranchId: manifest.ActiveGeometryBranch);

        Assert.False(artifact.OverallPass);
        var check = artifact.Checks.Single(c => c.CheckId == "provenance-branch-id-match");
        Assert.False(check.Passed);
        Assert.Contains("different-branch-id", check.Detail);
    }

    [Fact]
    public void Observation_BranchId_Mismatch_Fails_Check()
    {
        var manifest = TestHelpers.CreateConformingManifest(observationBranch: "sigma-pullback");
        var torsion = TestHelpers.CreateTrivialTorsion();
        var shiab = TestHelpers.CreateIdentityShiab();
        var provenance = TestHelpers.CreateMatchingProvenance(manifest);

        var artifact = _evaluator.Evaluate(
            manifest, torsion, shiab, provenance,
            observedBranchId: "wrong-observation-branch", // mismatch
            geometryBranchId: manifest.ActiveGeometryBranch);

        Assert.False(artifact.OverallPass);
        var check = artifact.Checks.Single(c => c.CheckId == "observation-branch-id-match");
        Assert.False(check.Passed);
        Assert.Contains("wrong-observation-branch", check.Detail);
        Assert.Contains("sigma-pullback", check.Detail);
    }

    [Fact]
    public void Geometry_BranchId_Mismatch_Fails_Check()
    {
        var manifest = TestHelpers.CreateConformingManifest(geometryBranch: "simplicial-toy-2d");
        var torsion = TestHelpers.CreateTrivialTorsion();
        var shiab = TestHelpers.CreateIdentityShiab();
        var provenance = TestHelpers.CreateMatchingProvenance(manifest);

        var artifact = _evaluator.Evaluate(
            manifest, torsion, shiab, provenance,
            observedBranchId: manifest.ActiveObservationBranch,
            geometryBranchId: "4d-production-mesh"); // mismatch

        Assert.False(artifact.OverallPass);
        var check = artifact.Checks.Single(c => c.CheckId == "geometry-branch-id-match");
        Assert.False(check.Passed);
        Assert.Contains("4d-production-mesh", check.Detail);
        Assert.Contains("simplicial-toy-2d", check.Detail);
    }

    [Fact]
    public void Missing_Pairing_Convention_Fails_Check()
    {
        // A manifest with an empty/unset pairing convention fails the pairing check.
        var manifest = TestHelpers.CreateConformingManifest(pairingConvention: "unset");
        var torsion = TestHelpers.CreateTrivialTorsion();
        var shiab = TestHelpers.CreateIdentityShiab();
        var provenance = TestHelpers.CreateMatchingProvenance(manifest);

        var artifact = _evaluator.Evaluate(
            manifest, torsion, shiab, provenance,
            observedBranchId: manifest.ActiveObservationBranch,
            geometryBranchId: manifest.ActiveGeometryBranch);

        Assert.False(artifact.OverallPass);
        var check = artifact.Checks.Single(c => c.CheckId == "pairing-convention-declared");
        Assert.False(check.Passed);
    }

    [Fact]
    public void All_Checks_Are_BranchLocal_ValidationType()
    {
        var manifest = TestHelpers.CreateConformingManifest();
        var torsion = TestHelpers.CreateTrivialTorsion();
        var shiab = TestHelpers.CreateIdentityShiab();
        var provenance = TestHelpers.CreateMatchingProvenance(manifest);

        var artifact = _evaluator.Evaluate(
            manifest, torsion, shiab, provenance,
            observedBranchId: manifest.ActiveObservationBranch,
            geometryBranchId: manifest.ActiveGeometryBranch);

        // Every check must declare itself as branch-local, never theory-level
        Assert.All(artifact.Checks, c =>
            Assert.Equal("branch-local", c.ValidationType));
    }

    [Fact]
    public void Conformance_Artifact_ValidationScope_Is_BranchLocal()
    {
        var manifest = TestHelpers.CreateConformingManifest();
        var torsion = TestHelpers.CreateTrivialTorsion();
        var shiab = TestHelpers.CreateIdentityShiab();
        var provenance = TestHelpers.CreateMatchingProvenance(manifest);

        var artifact = _evaluator.Evaluate(
            manifest, torsion, shiab, provenance,
            observedBranchId: manifest.ActiveObservationBranch,
            geometryBranchId: manifest.ActiveGeometryBranch);

        Assert.Equal("branch-local", artifact.ValidationScope);
        Assert.Contains("branch-local", artifact.ScopeDisclaimer);
        Assert.Contains("ASSUMPTIONS.md", artifact.ScopeDisclaimer);
    }

    [Fact]
    public void Conformance_Artifact_Is_JsonSerializable()
    {
        var manifest = TestHelpers.CreateConformingManifest();
        var torsion = TestHelpers.CreateTrivialTorsion();
        var shiab = TestHelpers.CreateIdentityShiab();
        var provenance = TestHelpers.CreateMatchingProvenance(manifest);

        var artifact = _evaluator.Evaluate(
            manifest, torsion, shiab, provenance,
            observedBranchId: manifest.ActiveObservationBranch,
            geometryBranchId: manifest.ActiveGeometryBranch);

        var json = JsonSerializer.Serialize(artifact);
        Assert.NotEmpty(json);

        var deserialized = JsonSerializer.Deserialize<ConformanceArtifact>(json);
        Assert.NotNull(deserialized);
        Assert.Equal(artifact.BranchId, deserialized!.BranchId);
        Assert.Equal(artifact.OverallPass, deserialized.OverallPass);
        Assert.Equal(artifact.Checks.Count, deserialized.Checks.Count);
        Assert.Equal("branch-local", deserialized.ValidationScope);
    }
}
