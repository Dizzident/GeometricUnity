using Gu.Core;
using Gu.Phase4.Comparison;
using Gu.Phase4.FamilyClustering;
using Gu.Phase4.Observation;
using Xunit;

namespace Gu.Phase4.Comparison.Tests;

/// <summary>
/// Tests for FermionComparisonCampaignRunner and FermionComparisonCampaign.
///
/// M43 completion criterion: at least one comparison campaign involving
/// fermionic candidates completes.
///
/// These tests verify:
/// - Campaign runner produces correct FermionComparisonCampaign output.
/// - Outcome counts are accurate.
/// - Campaign ID and metadata are preserved.
/// - Integration: observation + comparison in one end-to-end pass.
/// </summary>
public sealed class FermionComparisonCampaignTests
{
    private static ProvenanceMeta TestProvenance() => new()
    {
        CreatedAt = DateTimeOffset.UtcNow,
        CodeRevision = "test",
        Branch = new BranchRef { BranchId = "test-branch", SchemaVersion = "1.0.0" },
        Backend = "cpu-reference",
    };

    private static FamilyClusterRecord MakeCluster(
        string id,
        string chirality,
        double meanEv = 1.0,
        double branchPersistence = 0.8,
        double ambiguityScore = 0.0,
        string method = "eigenvalue-proximity")
        => new FamilyClusterRecord
        {
            ClusterId = id,
            ClusterLabel = id,
            MemberFamilyIds = new List<string> { $"family-{id}" },
            DominantChirality = chirality,
            HasConjugatePair = false,
            EigenvalueMagnitudeEnvelope = new[] { meanEv * 0.9, meanEv, meanEv * 1.1 },
            AmbiguityScore = ambiguityScore,
            MeanBranchPersistence = branchPersistence,
            ClusteringMethod = method,
            Provenance = TestProvenance(),
        };

    private static FermionCandidateReference MakeReference(
        string referenceId,
        string chirality,
        double[]? massEnvelope = null)
        => new FermionCandidateReference
        {
            ReferenceId = referenceId,
            Label = $"ref-{referenceId}",
            ExpectedChirality = chirality,
            ExpectedMassLikeEnvelope = massEnvelope,
        };

    // -------- M43 completion criterion --------

    [Fact]
    public void Run_OneCampaign_CompletesSuccessfully()
    {
        // This is the M43 completion criterion: at least one campaign completes.
        var clusters = new[]
        {
            MakeCluster("c0", "left", meanEv: 1.0),
        };
        var references = new[]
        {
            MakeReference("ref-lepton-like-0", "left", new[] { 0.9, 1.0, 1.1 }),
        };

        var campaign = FermionComparisonCampaignRunner.Run(
            "campaign-m43-01",
            clusters,
            references,
            massLikeScaleTolerance: 0.5,
            TestProvenance());

        Assert.NotNull(campaign);
        Assert.Equal("campaign-m43-01", campaign.CampaignId);
        Assert.Equal(1, campaign.ClusterCount);
        Assert.Equal(1, campaign.ReferenceCount);
        Assert.True(campaign.CompatibleCount + campaign.IncompatibleCount
            + campaign.UnderdeterminedCount + campaign.NotApplicableCount
            == campaign.ComparisonRecords.Count);
    }

    // -------- Outcome counts --------

    [Fact]
    public void Run_CompatibleMatch_CompatibleCountIsOne()
    {
        var clusters = new[]
        {
            MakeCluster("c0", "left", meanEv: 1.0, branchPersistence: 0.9),
        };
        var references = new[]
        {
            MakeReference("ref-0", "left", new[] { 0.9, 1.0, 1.1 }),
        };

        var campaign = FermionComparisonCampaignRunner.Run(
            "camp-1", clusters, references, 0.5, TestProvenance());

        Assert.Equal(1, campaign.CompatibleCount);
        Assert.Equal(0, campaign.IncompatibleCount);
    }

    [Fact]
    public void Run_ChiralityMismatch_IncompatibleCountIsOne()
    {
        var clusters = new[]
        {
            MakeCluster("c0", "right", meanEv: 1.0, branchPersistence: 0.9),
        };
        var references = new[]
        {
            MakeReference("ref-0", "left", new[] { 0.9, 1.0, 1.1 }),
        };

        var campaign = FermionComparisonCampaignRunner.Run(
            "camp-2", clusters, references, 0.5, TestProvenance());

        Assert.Equal(0, campaign.CompatibleCount);
        Assert.Equal(1, campaign.IncompatibleCount);
    }

    [Fact]
    public void Run_NullMassEnvelope_UnderdeterminedCountIsOne()
    {
        var clusters = new[]
        {
            MakeCluster("c0", "left", branchPersistence: 0.9),
        };
        var references = new[]
        {
            MakeReference("ref-0", "left", massEnvelope: null),
        };

        var campaign = FermionComparisonCampaignRunner.Run(
            "camp-3", clusters, references, 0.5, TestProvenance());

        Assert.Equal(0, campaign.CompatibleCount);
        Assert.Equal(1, campaign.UnderdeterminedCount);
    }

    [Fact]
    public void Run_TrivialCluster_NotApplicableCount()
    {
        // mixed chirality -> trivial observation -> not-applicable
        var clusters = new[]
        {
            MakeCluster("c-mixed", "mixed"),
        };
        var references = new[]
        {
            MakeReference("ref-0", "left"),
        };

        var campaign = FermionComparisonCampaignRunner.Run(
            "camp-4", clusters, references, 0.5, TestProvenance());

        Assert.Equal(1, campaign.NotApplicableCount);
    }

    [Fact]
    public void Run_EmptyClusters_ZeroCounts()
    {
        var campaign = FermionComparisonCampaignRunner.Run(
            "camp-empty",
            Array.Empty<FamilyClusterRecord>(),
            new[] { MakeReference("ref-0", "left") },
            0.5,
            TestProvenance());

        Assert.Equal(0, campaign.ClusterCount);
        Assert.Equal(0, campaign.CompatibleCount + campaign.IncompatibleCount
            + campaign.UnderdeterminedCount + campaign.NotApplicableCount);
        Assert.Empty(campaign.ComparisonRecords);
    }

    [Fact]
    public void Run_EmptyReferences_NoCampaign()
    {
        var clusters = new[]
        {
            MakeCluster("c0", "left"),
        };

        var campaign = FermionComparisonCampaignRunner.Run(
            "camp-no-refs",
            clusters,
            Array.Empty<FermionCandidateReference>(),
            0.5,
            TestProvenance());

        Assert.Equal(0, campaign.ReferenceCount);
        Assert.Empty(campaign.ComparisonRecords);
    }

    // -------- Observation summaries in campaign --------

    [Fact]
    public void Run_CampaignContainsObservationSummaries()
    {
        var clusters = new[]
        {
            MakeCluster("c0", "left"),
            MakeCluster("c1", "right"),
        };
        var references = new[] { MakeReference("ref-0", "left") };

        var campaign = FermionComparisonCampaignRunner.Run(
            "camp-5", clusters, references, 0.5, TestProvenance());

        Assert.Equal(2, campaign.ObservationSummaries.Count);
    }

    [Fact]
    public void Run_ObservationSummary_ChiralityMatchesClusters()
    {
        var clusters = new[]
        {
            MakeCluster("c0", "right"),
        };
        var references = new[] { MakeReference("ref-0", "right") };

        var campaign = FermionComparisonCampaignRunner.Run(
            "camp-6", clusters, references, 0.5, TestProvenance());

        Assert.Single(campaign.ObservationSummaries);
        Assert.Equal("right", campaign.ObservationSummaries[0].ObservedChirality);
    }

    // -------- Cross-product of obs x refs --------

    [Fact]
    public void Run_TwoClustersThreeRefs_SixComparisonRecords()
    {
        var clusters = new[]
        {
            MakeCluster("c0", "left", branchPersistence: 0.9),
            MakeCluster("c1", "right", branchPersistence: 0.9),
        };
        var references = new[]
        {
            MakeReference("ref-0", "left"),
            MakeReference("ref-1", "right"),
            MakeReference("ref-2", "mixed"),
        };

        var campaign = FermionComparisonCampaignRunner.Run(
            "camp-7", clusters, references, 0.5, TestProvenance());

        Assert.Equal(6, campaign.ComparisonRecords.Count);
    }

    // -------- Schema version --------

    [Fact]
    public void Campaign_SchemaVersion_IsSet()
    {
        var campaign = FermionComparisonCampaignRunner.Run(
            "camp-sv",
            Array.Empty<FamilyClusterRecord>(),
            Array.Empty<FermionCandidateReference>(),
            0.5,
            TestProvenance());

        Assert.Equal("1.0.0", campaign.SchemaVersion);
    }

    // -------- Count invariant --------

    [Fact]
    public void Run_TotalOutcomeCountEqualsRecordCount()
    {
        var clusters = new[]
        {
            MakeCluster("c0", "left", meanEv: 1.0, branchPersistence: 0.9),
            MakeCluster("c1", "right", meanEv: 2.0, branchPersistence: 0.3),
            MakeCluster("c2", "mixed", branchPersistence: 0.5),
        };
        var references = new[]
        {
            MakeReference("ref-0", "left", new[] { 0.9, 1.0, 1.1 }),
            MakeReference("ref-1", "right"),
        };

        var campaign = FermionComparisonCampaignRunner.Run(
            "camp-inv", clusters, references, 0.5, TestProvenance());

        int total = campaign.CompatibleCount + campaign.IncompatibleCount
            + campaign.UnderdeterminedCount + campaign.NotApplicableCount;
        Assert.Equal(campaign.ComparisonRecords.Count, total);
    }
}
