namespace Gu.TheoryConformance.Tests;

/// <summary>
/// Tests that verify scope-boundary conformance checks:
/// - The conformance artifact must declare itself as branch-local, never theory-level.
/// - Branch-local validation is distinguished from theory-level validation in the artifact.
/// - The presence of InsertedAssumptionIds in the manifest is required for scope declaration.
///
/// These tests enforce A-017 from ASSUMPTIONS.md:
/// "Internal consistency does not imply original-theory validation."
/// </summary>
public class ScopeBoundaryConformanceTests
{
    private readonly ConformanceEvaluator _evaluator = new();

    [Fact]
    public void Manifest_WithDeclaredAssumptions_Passes_ScopeCheck()
    {
        var manifest = TestHelpers.CreateConformingManifest();
        Assert.NotNull(manifest.InsertedAssumptionIds);
        Assert.NotEmpty(manifest.InsertedAssumptionIds);

        var artifact = _evaluator.Evaluate(
            manifest,
            TestHelpers.CreateTrivialTorsion(),
            TestHelpers.CreateIdentityShiab(),
            TestHelpers.CreateMatchingProvenance(manifest),
            observedBranchId: manifest.ActiveObservationBranch,
            geometryBranchId: manifest.ActiveGeometryBranch);

        var scopeCheck = artifact.Checks.Single(c => c.CheckId == "branch-scope-declared");
        Assert.True(scopeCheck.Passed);
        Assert.Contains("A-003", scopeCheck.Detail);
    }

    [Fact]
    public void Artifact_ScopeDisclaimer_Mentions_PhysicalCorrectness_Caveat()
    {
        // The artifact must explicitly disclaim that passing conformance does not
        // mean the outputs match physical reality.
        var manifest = TestHelpers.CreateConformingManifest();

        var artifact = _evaluator.Evaluate(
            manifest,
            TestHelpers.CreateTrivialTorsion(),
            TestHelpers.CreateIdentityShiab(),
            TestHelpers.CreateMatchingProvenance(manifest),
            observedBranchId: manifest.ActiveObservationBranch,
            geometryBranchId: manifest.ActiveGeometryBranch);

        // The disclaimer must warn that passing conformance does not equal physical truth
        Assert.Contains("physical reality", artifact.ScopeDisclaimer);
        Assert.Contains("canonical", artifact.ScopeDisclaimer);
    }

    [Fact]
    public void Artifact_BranchId_Matches_Manifest_BranchId()
    {
        var manifest = TestHelpers.CreateConformingManifest(branchId: "specific-test-branch");

        var artifact = _evaluator.Evaluate(
            manifest,
            TestHelpers.CreateTrivialTorsion(),
            TestHelpers.CreateIdentityShiab(),
            TestHelpers.CreateMatchingProvenance(manifest),
            observedBranchId: manifest.ActiveObservationBranch,
            geometryBranchId: manifest.ActiveGeometryBranch);

        Assert.Equal("specific-test-branch", artifact.BranchId);
    }

    [Fact]
    public void Artifact_ConformanceId_Is_Unique()
    {
        var manifest = TestHelpers.CreateConformingManifest();
        var torsion = TestHelpers.CreateTrivialTorsion();
        var shiab = TestHelpers.CreateIdentityShiab();
        var provenance = TestHelpers.CreateMatchingProvenance(manifest);

        var artifact1 = _evaluator.Evaluate(
            manifest, torsion, shiab, provenance,
            observedBranchId: manifest.ActiveObservationBranch,
            geometryBranchId: manifest.ActiveGeometryBranch);

        var artifact2 = _evaluator.Evaluate(
            manifest, torsion, shiab, provenance,
            observedBranchId: manifest.ActiveObservationBranch,
            geometryBranchId: manifest.ActiveGeometryBranch);

        // Two separate conformance runs should produce distinct IDs
        Assert.NotEqual(artifact1.ConformanceId, artifact2.ConformanceId);
    }

    [Fact]
    public void BranchLocal_Checks_Are_Distinct_From_TheoryLevel_Claims()
    {
        // This test verifies the design property:
        // no check in this harness can be misinterpreted as theory-level.
        var manifest = TestHelpers.CreateConformingManifest();

        var artifact = _evaluator.Evaluate(
            manifest,
            TestHelpers.CreateTrivialTorsion(),
            TestHelpers.CreateIdentityShiab(),
            TestHelpers.CreateMatchingProvenance(manifest),
            observedBranchId: manifest.ActiveObservationBranch,
            geometryBranchId: manifest.ActiveGeometryBranch);

        // No check should declare itself as theory-level
        Assert.DoesNotContain(artifact.Checks, c => c.ValidationType == "theory-level");

        // The overall artifact scope must be branch-local
        Assert.Equal("branch-local", artifact.ValidationScope);
    }

    [Fact]
    public void BranchIdentity_Checks_Belong_To_BranchIdentity_Category()
    {
        var manifest = TestHelpers.CreateConformingManifest();

        var artifact = _evaluator.Evaluate(
            manifest,
            TestHelpers.CreateTrivialTorsion(),
            TestHelpers.CreateIdentityShiab(),
            TestHelpers.CreateMatchingProvenance(manifest),
            observedBranchId: manifest.ActiveObservationBranch,
            geometryBranchId: manifest.ActiveGeometryBranch);

        var branchIdentityChecks = artifact.Checks
            .Where(c => c.Category == "branch-identity")
            .ToList();

        Assert.NotEmpty(branchIdentityChecks);
        // Branch-identity checks: torsion, shiab, provenance, observation, geometry, pairing
        Assert.True(branchIdentityChecks.Count >= 5,
            $"Expected at least 5 branch-identity checks, got {branchIdentityChecks.Count}");
    }

    [Fact]
    public void TrivialState_Checks_Belong_To_TrivialState_Category()
    {
        var manifest = TestHelpers.CreateConformingManifest();

        var artifact = _evaluator.Evaluate(
            manifest,
            TestHelpers.CreateTrivialTorsion(),
            TestHelpers.CreateIdentityShiab(),
            TestHelpers.CreateMatchingProvenance(manifest),
            observedBranchId: manifest.ActiveObservationBranch,
            geometryBranchId: manifest.ActiveGeometryBranch);

        var trivialChecks = artifact.Checks
            .Where(c => c.Category == "trivial-state")
            .ToList();

        Assert.NotEmpty(trivialChecks);
        Assert.True(trivialChecks.Count >= 2,
            $"Expected at least 2 trivial-state checks, got {trivialChecks.Count}");
    }

    [Fact]
    public void ScopeBoundary_Check_Belongs_To_ScopeBoundary_Category()
    {
        var manifest = TestHelpers.CreateConformingManifest();

        var artifact = _evaluator.Evaluate(
            manifest,
            TestHelpers.CreateTrivialTorsion(),
            TestHelpers.CreateIdentityShiab(),
            TestHelpers.CreateMatchingProvenance(manifest),
            observedBranchId: manifest.ActiveObservationBranch,
            geometryBranchId: manifest.ActiveGeometryBranch);

        var scopeChecks = artifact.Checks
            .Where(c => c.Category == "scope-boundary")
            .ToList();

        Assert.NotEmpty(scopeChecks);
        Assert.Contains(scopeChecks, c => c.CheckId == "branch-scope-declared");
    }

    [Fact]
    public void MultipleFailures_AllPresentInArtifact()
    {
        // Deliberately break multiple checks at once.
        var manifest = TestHelpers.CreateConformingManifest(
            torsionBranch: "trivial",
            shiabBranch: "identity-shiab",
            observationBranch: "sigma-pullback");

        var wrongTorsion = TestHelpers.CreateLocalAlgebraicTorsion(); // will mismatch torsion
        var wrongShiab = TestHelpers.CreateFirstOrderShiab();         // will mismatch shiab
        var mismatchedProvenance = TestHelpers.CreateMismatchedProvenance(manifest);

        var artifact = _evaluator.Evaluate(
            manifest, wrongTorsion, wrongShiab, mismatchedProvenance,
            observedBranchId: "wrong-observation",  // will mismatch observation
            geometryBranchId: "wrong-geometry");    // will mismatch geometry

        Assert.False(artifact.OverallPass);

        var failures = artifact.Checks.Where(c => !c.Passed).ToList();
        Assert.True(failures.Count >= 4,
            $"Expected at least 4 failures (torsion, shiab, provenance, observation), got {failures.Count}");

        Assert.Contains(failures, c => c.CheckId == "torsion-branch-id-match");
        Assert.Contains(failures, c => c.CheckId == "shiab-branch-id-match");
        Assert.Contains(failures, c => c.CheckId == "provenance-branch-id-match");
        Assert.Contains(failures, c => c.CheckId == "observation-branch-id-match");
    }
}
