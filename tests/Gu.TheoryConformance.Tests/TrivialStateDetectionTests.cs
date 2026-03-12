namespace Gu.TheoryConformance.Tests;

/// <summary>
/// Tests that verify trivial-state detection conformance checks:
/// - trivial torsion (T=0) branch is flagged explicitly
/// - identity Shiab (S=F) branch is flagged explicitly
/// - zero omega initial condition is flagged explicitly
///
/// These checks are informational: they pass but annotate the artifact with
/// the branch-local scope implications of each trivial choice (A-009, A-011).
/// </summary>
public class TrivialStateDetectionTests
{
    private readonly ConformanceEvaluator _evaluator = new();

    [Fact]
    public void TrivialTorsion_Is_Flagged_In_Detail()
    {
        var manifest = TestHelpers.CreateConformingManifest();
        var torsion = TestHelpers.CreateTrivialTorsion();   // BranchId = "trivial"
        var shiab = TestHelpers.CreateIdentityShiab();
        var provenance = TestHelpers.CreateMatchingProvenance(manifest);

        var artifact = _evaluator.Evaluate(
            manifest, torsion, shiab, provenance,
            observedBranchId: manifest.ActiveObservationBranch,
            geometryBranchId: manifest.ActiveGeometryBranch);

        var check = artifact.Checks.Single(c => c.CheckId == "torsion-trivial-branch-flag");
        Assert.True(check.Passed); // trivial is allowed — just flagged
        Assert.Contains("trivial", check.Detail);
        Assert.Contains("A-009", check.Detail); // references the ASSUMPTIONS.md assumption
    }

    [Fact]
    public void NonTrivialTorsion_Is_Not_Flagged_As_Trivial()
    {
        var manifest = TestHelpers.CreateConformingManifest(torsionBranch: "local-algebraic");
        var torsion = TestHelpers.CreateLocalAlgebraicTorsion(); // BranchId = "local-algebraic"
        var shiab = TestHelpers.CreateIdentityShiab();
        var provenance = TestHelpers.CreateMatchingProvenance(manifest);

        var artifact = _evaluator.Evaluate(
            manifest, torsion, shiab, provenance,
            observedBranchId: manifest.ActiveObservationBranch,
            geometryBranchId: manifest.ActiveGeometryBranch);

        var check = artifact.Checks.Single(c => c.CheckId == "torsion-trivial-branch-flag");
        Assert.True(check.Passed);
        Assert.Contains("non-trivial", check.Detail);
    }

    [Fact]
    public void IdentityShiab_Is_Flagged_In_Detail()
    {
        var manifest = TestHelpers.CreateConformingManifest();
        var torsion = TestHelpers.CreateTrivialTorsion();
        var shiab = TestHelpers.CreateIdentityShiab(); // BranchId = "identity-shiab"
        var provenance = TestHelpers.CreateMatchingProvenance(manifest);

        var artifact = _evaluator.Evaluate(
            manifest, torsion, shiab, provenance,
            observedBranchId: manifest.ActiveObservationBranch,
            geometryBranchId: manifest.ActiveGeometryBranch);

        var check = artifact.Checks.Single(c => c.CheckId == "shiab-identity-branch-flag");
        Assert.True(check.Passed); // identity is allowed — just flagged
        Assert.Contains("identity", check.Detail);
        Assert.Contains("A-009", check.Detail);
    }

    [Fact]
    public void NonIdentityShiab_Is_Not_Flagged_As_Identity()
    {
        var manifest = TestHelpers.CreateConformingManifest(shiabBranch: "first-order-curvature");
        var torsion = TestHelpers.CreateTrivialTorsion();
        var shiab = TestHelpers.CreateFirstOrderShiab(); // BranchId = "first-order-curvature"
        var provenance = TestHelpers.CreateMatchingProvenance(manifest);

        var artifact = _evaluator.Evaluate(
            manifest, torsion, shiab, provenance,
            observedBranchId: manifest.ActiveObservationBranch,
            geometryBranchId: manifest.ActiveGeometryBranch);

        var check = artifact.Checks.Single(c => c.CheckId == "shiab-identity-branch-flag");
        Assert.True(check.Passed);
        Assert.Contains("non-identity", check.Detail);
    }

    [Fact]
    public void ZeroOmega_Is_Flagged_With_A011_Reference()
    {
        var manifest = TestHelpers.CreateConformingManifest();
        var torsion = TestHelpers.CreateTrivialTorsion();
        var shiab = TestHelpers.CreateIdentityShiab();
        var provenance = TestHelpers.CreateMatchingProvenance(manifest);
        var (mesh, algebra) = TestHelpers.CreateMinimalSetup();
        var zeroOmega = TestHelpers.CreateZeroOmega(mesh.EdgeCount, algebra.Dimension);

        var artifact = _evaluator.Evaluate(
            manifest, torsion, shiab, provenance,
            observedBranchId: manifest.ActiveObservationBranch,
            geometryBranchId: manifest.ActiveGeometryBranch,
            omegaCoefficients: zeroOmega);

        var check = artifact.Checks.Single(c => c.CheckId == "omega-zero-state-flag");
        Assert.True(check.Passed); // zero is allowed — just flagged
        Assert.Contains("zero", check.Detail);
        Assert.Contains("A-011", check.Detail);
        Assert.Contains("trivial", check.Detail);
    }

    [Fact]
    public void NonZeroOmega_Is_Identified_As_Nontrivial()
    {
        var manifest = TestHelpers.CreateConformingManifest();
        var torsion = TestHelpers.CreateTrivialTorsion();
        var shiab = TestHelpers.CreateIdentityShiab();
        var provenance = TestHelpers.CreateMatchingProvenance(manifest);
        var (mesh, algebra) = TestHelpers.CreateMinimalSetup();
        var nonZeroOmega = TestHelpers.CreateNonZeroOmega(mesh.EdgeCount, algebra.Dimension);

        var artifact = _evaluator.Evaluate(
            manifest, torsion, shiab, provenance,
            observedBranchId: manifest.ActiveObservationBranch,
            geometryBranchId: manifest.ActiveGeometryBranch,
            omegaCoefficients: nonZeroOmega);

        var check = artifact.Checks.Single(c => c.CheckId == "omega-zero-state-flag");
        Assert.True(check.Passed);
        Assert.Contains("non-zero", check.Detail);
    }

    [Fact]
    public void Without_Omega_NoZeroStateCheck_Is_Present()
    {
        var manifest = TestHelpers.CreateConformingManifest();
        var torsion = TestHelpers.CreateTrivialTorsion();
        var shiab = TestHelpers.CreateIdentityShiab();
        var provenance = TestHelpers.CreateMatchingProvenance(manifest);

        var artifact = _evaluator.Evaluate(
            manifest, torsion, shiab, provenance,
            observedBranchId: manifest.ActiveObservationBranch,
            geometryBranchId: manifest.ActiveGeometryBranch,
            omegaCoefficients: null); // no omega provided

        // omega-zero-state-flag check must not be present when no omega is passed
        Assert.DoesNotContain(artifact.Checks, c => c.CheckId == "omega-zero-state-flag");
    }

    [Fact]
    public void TrivialBranch_Entire_Run_StillPasses_Overall()
    {
        // A fully trivial run (zero omega, trivial torsion, identity Shiab) passes
        // overall — trivial-state checks are informational only.
        var manifest = TestHelpers.CreateConformingManifest();
        var torsion = TestHelpers.CreateTrivialTorsion();
        var shiab = TestHelpers.CreateIdentityShiab();
        var provenance = TestHelpers.CreateMatchingProvenance(manifest);
        var (mesh, algebra) = TestHelpers.CreateMinimalSetup();
        var zeroOmega = TestHelpers.CreateZeroOmega(mesh.EdgeCount, algebra.Dimension);

        var artifact = _evaluator.Evaluate(
            manifest, torsion, shiab, provenance,
            observedBranchId: manifest.ActiveObservationBranch,
            geometryBranchId: manifest.ActiveGeometryBranch,
            omegaCoefficients: zeroOmega);

        Assert.True(artifact.OverallPass,
            "A trivial (but conforming) run must pass overall conformance. " +
            "Trivial-state checks are informational, not failures.");
    }
}
