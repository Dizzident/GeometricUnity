using Gu.Core;
using Gu.Phase5.QuantitativeValidation;

namespace Gu.Phase5.QuantitativeValidation.Tests;

public sealed class InternalVectorBosonSourceReadinessValidatorTests
{
    private static ProvenanceMeta MakeProvenance() => new()
    {
        CreatedAt = DateTimeOffset.Parse("2026-04-25T00:00:00Z"),
        CodeRevision = "test",
        Branch = new BranchRef { BranchId = "phase21-source-readiness", SchemaVersion = "1.0" },
        Backend = "cpu",
    };

    private static InternalVectorBosonSourceReadinessPolicy StandardPolicy => new()
    {
        PolicyId = "phase21-standard-source-readiness",
        MinimumClaimClass = "C2_BranchStableCandidate",
        MinimumBranchStabilityScore = 0.5,
        MinimumRefinementStabilityScore = 1.0,
        MaximumAmbiguityCount = 0,
        RequireBranchSelectors = true,
        RequireRefinementCoverage = true,
        RequireEnvironmentSelectors = true,
        RequireCompleteUncertainty = true,
        AllowedClaimClasses = ["C2_BranchStableCandidate"],
    };

    [Fact]
    public void Validate_ReadyIdentityNeutralCandidate_ReturnsNoErrors()
    {
        var candidate = MakeReadyCandidate();

        var errors = InternalVectorBosonSourceReadinessValidator.Validate(candidate, StandardPolicy);

        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_MissingBranchSelectors_ReturnsBlocker()
    {
        var candidate = MakeReadyCandidate(branchSelectors: []);

        var errors = InternalVectorBosonSourceReadinessValidator.Validate(candidate, StandardPolicy);

        Assert.Contains(errors, e => e.Contains("branch selectors", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_AmbiguousFamily_ReturnsBlocker()
    {
        var candidate = MakeReadyCandidate(ambiguityCount: 1);

        var errors = InternalVectorBosonSourceReadinessValidator.Validate(candidate, StandardPolicy);

        Assert.Contains(errors, e => e.Contains("ambiguity count", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_WeakClaimClass_ReturnsBlocker()
    {
        var candidate = MakeReadyCandidate(claimClass: "C0_NumericalMode");

        var errors = InternalVectorBosonSourceReadinessValidator.Validate(candidate, StandardPolicy);

        Assert.Contains(errors, e => e.Contains("claim class", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_IncompleteUncertainty_ReturnsBlocker()
    {
        var candidate = MakeReadyCandidate(uncertainty: new QuantitativeUncertainty
        {
            ExtractionError = 0.01,
            EnvironmentSensitivity = 0.02,
            BranchVariation = -1,
            RefinementError = 0.03,
            TotalUncertainty = -1,
        });

        var errors = InternalVectorBosonSourceReadinessValidator.Validate(candidate, StandardPolicy);

        Assert.Contains(errors, e => e.Contains("uncertainty components", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(errors, e => e.Contains("total uncertainty", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_ExternalTargetPath_ReturnsBlocker()
    {
        var candidate = MakeReadyCandidate(paths: ["studies/phase19_dimensionless_wz_candidate_001/physical_targets.json"]);

        var errors = InternalVectorBosonSourceReadinessValidator.Validate(candidate, StandardPolicy);

        Assert.Contains(errors, e => e.Contains("external target", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Reevaluate_RequiresAtLeastTwoReadySourcesForTerminalReady()
    {
        var table = new InternalVectorBosonSourceCandidateTable
        {
            TableId = "sources",
            SchemaVersion = "1.0.0",
            TerminalStatus = "source-blocked",
            Candidates = [MakeReadyCandidate("source-1"), MakeReadyCandidate("source-2")],
            SummaryBlockers = [],
            Provenance = MakeProvenance(),
        };
        var spec = MakeSpec();

        var result = InternalVectorBosonSourceReadinessValidator.Reevaluate(table, spec, MakeProvenance());

        Assert.Equal("candidate-source-ready", result.TerminalStatus);
        Assert.All(result.Candidates, c => Assert.Equal("candidate-source-ready", c.Status));
    }

    [Fact]
    public void Reevaluate_OneReadySource_RemainsTerminalBlocked()
    {
        var table = new InternalVectorBosonSourceCandidateTable
        {
            TableId = "sources",
            SchemaVersion = "1.0.0",
            TerminalStatus = "source-blocked",
            Candidates = [MakeReadyCandidate("source-1")],
            SummaryBlockers = [],
            Provenance = MakeProvenance(),
        };

        var result = InternalVectorBosonSourceReadinessValidator.Reevaluate(table, MakeSpec(), MakeProvenance());

        Assert.Equal("source-blocked", result.TerminalStatus);
        Assert.Contains(result.SummaryBlockers, b => b.Contains("at least 2", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Reevaluate_Phase20BlockedCandidate_RemainsBlockedWithSpecificReasons()
    {
        var candidate = MakeReadyCandidate(
            sourceCandidateId: "phase12-candidate-0",
            branchSelectors: [],
            refinementLevels: [],
            ambiguityCount: 1,
            claimClass: "C0_NumericalMode",
            uncertainty: new QuantitativeUncertainty { ExtractionError = 0.01 });
        var table = new InternalVectorBosonSourceCandidateTable
        {
            TableId = "sources",
            SchemaVersion = "1.0.0",
            TerminalStatus = "source-blocked",
            Candidates = [candidate],
            SummaryBlockers = [],
            Provenance = MakeProvenance(),
        };

        var result = InternalVectorBosonSourceReadinessValidator.Reevaluate(table, MakeSpec(), MakeProvenance());

        Assert.Equal("source-blocked", result.TerminalStatus);
        var blocked = Assert.Single(result.Candidates);
        Assert.Contains(blocked.ClosureRequirements, e => e.Contains("branch selectors", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(blocked.ClosureRequirements, e => e.Contains("refinement coverage", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(blocked.ClosureRequirements, e => e.Contains("ambiguity", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(blocked.ClosureRequirements, e => e.Contains("claim class", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(blocked.ClosureRequirements, e => e.Contains("uncertainty", StringComparison.OrdinalIgnoreCase));
    }

    private static InternalVectorBosonSourceReadinessCampaignSpec MakeSpec() => new()
    {
        CampaignId = "phase21-source-readiness-test",
        SchemaVersion = "1.0.0",
        BranchVariantIds = ["branch-a", "branch-b"],
        RefinementLevels = ["L0", "L1"],
        EnvironmentIds = ["env-a", "env-b"],
        SourceQuantityIds = ["massLikeValue"],
        ReadinessPolicy = StandardPolicy,
        IdentityScope = "identity-neutral-vector-boson-source-candidates",
        Provenance = MakeProvenance(),
    };

    private static InternalVectorBosonSourceCandidate MakeReadyCandidate(
        string sourceCandidateId = "source-1",
        IReadOnlyList<string>? paths = null,
        IReadOnlyList<string>? branchSelectors = null,
        IReadOnlyList<string>? refinementLevels = null,
        int? ambiguityCount = 0,
        string claimClass = "C2_BranchStableCandidate",
        QuantitativeUncertainty? uncertainty = null)
        => new()
        {
            SourceCandidateId = sourceCandidateId,
            SourceOrigin = InternalVectorBosonSourceCandidateAdapter.SourceOrigin,
            ModeRole = InternalVectorBosonSourceCandidateAdapter.ModeRole,
            SourceArtifactPaths = paths ?? ["studies/phase21/source.json"],
            SourceModeIds = ["mode-a", "mode-b"],
            SourceFamilyId = "family-a",
            MassLikeValue = 1.0,
            Uncertainty = uncertainty ?? new QuantitativeUncertainty
            {
                BranchVariation = 0.01,
                RefinementError = 0.02,
                ExtractionError = 0.03,
                EnvironmentSensitivity = 0.04,
                TotalUncertainty = 0.055,
            },
            BranchSelectors = branchSelectors ?? ["branch-a", "branch-b"],
            EnvironmentSelectors = ["env-a", "env-b"],
            RefinementLevels = refinementLevels ?? ["L0", "L1"],
            BranchStabilityScore = 0.8,
            RefinementStabilityScore = 1.0,
            BackendStabilityScore = 1.0,
            ObservationStabilityScore = 1.0,
            AmbiguityCount = ambiguityCount,
            GaugeLeakEnvelope = [0.0, 0.0, 0.0],
            ClaimClass = claimClass,
            Status = "source-blocked",
            Assumptions = ["identity-neutral source candidate"],
            ClosureRequirements = [],
            Provenance = MakeProvenance(),
        };
}
