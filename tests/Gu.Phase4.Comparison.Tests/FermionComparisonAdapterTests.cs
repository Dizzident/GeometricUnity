using Gu.Core;
using Gu.Phase4.Comparison;
using Gu.Phase4.FamilyClustering;
using Gu.Phase4.Observation;
using Xunit;

namespace Gu.Phase4.Comparison.Tests;

public sealed class FermionComparisonAdapterTests
{
    private static ProvenanceMeta TestProvenance() => new()
    {
        CreatedAt = DateTimeOffset.UtcNow,
        CodeRevision = "test",
        Branch = new BranchRef { BranchId = "test-branch", SchemaVersion = "1.0.0" },
        Backend = "cpu-reference",
    };

    private static FamilyClusterRecord MakeCluster(
        string id, string chirality, double meanEv = 1.0, double persistence = 1.0,
        bool hasConjugatePair = false, string method = "eigenvalue-proximity")
        => new FamilyClusterRecord
        {
            ClusterId = id,
            ClusterLabel = id,
            MemberFamilyIds = new List<string> { $"family-{id}" },
            DominantChirality = chirality,
            HasConjugatePair = hasConjugatePair,
            EigenvalueMagnitudeEnvelope = new[] { meanEv * 0.9, meanEv, meanEv * 1.1 },
            AmbiguityScore = 0.0,
            MeanBranchPersistence = persistence,
            ClusteringMethod = method,
            Provenance = TestProvenance(),
        };

    private static FermionObservationSummary MakeObservation(
        string clusterId, string chirality, double meanMass = 1.0, bool isTrivial = false)
        => new FermionObservationSummary
        {
            ClusterId = clusterId,
            ObservedChirality = chirality,
            MassLikeEnvelope = new[] { meanMass * 0.9, meanMass, meanMass * 1.1 },
            HasConjugatePair = false,
            BranchPersistenceScore = 1.0,
            AmbiguityScore = 0.0,
            IsTrivial = isTrivial,
            Provenance = TestProvenance(),
        };

    private static FermionCandidateReference MakeReference(
        string id, string chirality, double meanMass = 1.0)
        => new FermionCandidateReference
        {
            ReferenceId = id,
            Label = id,
            ExpectedChirality = chirality,
            ExpectedMassLikeEnvelope = new[] { meanMass * 0.9, meanMass, meanMass * 1.1 },
            ExpectsConjugatePair = false,
        };

    [Fact]
    public void Compare_MatchingChiralityAndMass_IsCompatible()
    {
        var adapter = new FermionComparisonAdapter(massLikeScaleTolerance: 0.5);
        var obs = MakeObservation("c0", "left", meanMass: 1.0);
        var reference = MakeReference("ref-0", "left", meanMass: 1.0);

        var result = adapter.Compare(obs, reference, TestProvenance());

        Assert.Equal("compatible", result.Outcome);
        Assert.Equal("match", result.ChiralityMatch);
        Assert.True(result.MassLikeScaleWithinTolerance);
    }

    [Fact]
    public void Compare_ChiralityMismatch_IsIncompatible()
    {
        var adapter = new FermionComparisonAdapter(massLikeScaleTolerance: 0.5);
        var obs = MakeObservation("c0", "left", meanMass: 1.0);
        var reference = MakeReference("ref-0", "right", meanMass: 1.0);

        var result = adapter.Compare(obs, reference, TestProvenance());

        Assert.Equal("incompatible", result.Outcome);
        Assert.Equal("mismatch", result.ChiralityMatch);
    }

    [Fact]
    public void Compare_MassLikeScaleTooFar_IsIncompatible()
    {
        var adapter = new FermionComparisonAdapter(massLikeScaleTolerance: 0.1);
        var obs = MakeObservation("c0", "left", meanMass: 2.0);
        var reference = MakeReference("ref-0", "left", meanMass: 1.0);

        var result = adapter.Compare(obs, reference, TestProvenance());

        Assert.Equal("incompatible", result.Outcome);
        Assert.False(result.MassLikeScaleWithinTolerance);
    }

    [Fact]
    public void Compare_NoReferenceMassEnvelope_IsUnderdetermined()
    {
        var adapter = new FermionComparisonAdapter(massLikeScaleTolerance: 0.5);
        var obs = MakeObservation("c0", "left", meanMass: 1.0);
        var reference = new FermionCandidateReference
        {
            ReferenceId = "ref-unconstrained",
            Label = "unconstrained",
            ExpectedChirality = "left",
            ExpectedMassLikeEnvelope = null,
        };

        var result = adapter.Compare(obs, reference, TestProvenance());

        Assert.Equal("underdetermined", result.Outcome);
    }

    [Fact]
    public void Compare_TrivialObservation_IsNotApplicable()
    {
        var adapter = new FermionComparisonAdapter(massLikeScaleTolerance: 0.5);
        var obs = MakeObservation("c0", "mixed", isTrivial: true);
        var reference = MakeReference("ref-0", "left");

        var result = adapter.Compare(obs, reference, TestProvenance());

        Assert.Equal("not-applicable", result.Outcome);
    }

    [Fact]
    public void Compare_ComparisonIdIsUnique()
    {
        var adapter = new FermionComparisonAdapter();
        var obs = MakeObservation("cluster-123", "left");
        var reference = MakeReference("ref-456", "left");

        var result = adapter.Compare(obs, reference, TestProvenance());

        Assert.Contains("cluster-123", result.ComparisonId);
        Assert.Contains("ref-456", result.ComparisonId);
    }

    [Fact]
    public void Compare_MassRatioIsRecorded()
    {
        var adapter = new FermionComparisonAdapter(massLikeScaleTolerance: 0.5);
        var obs = MakeObservation("c0", "left", meanMass: 2.0);
        var reference = MakeReference("ref-0", "left", meanMass: 1.0);

        var result = adapter.Compare(obs, reference, TestProvenance());

        Assert.NotNull(result.MassLikeScaleRatio);
        Assert.Equal(2.0, result.MassLikeScaleRatio!.Value, precision: 10);
    }

    [Fact]
    public void CompareAll_CrossProduct_ProducesCorrectCount()
    {
        var adapter = new FermionComparisonAdapter();
        var observations = new[]
        {
            MakeObservation("c0", "left"),
            MakeObservation("c1", "right"),
        };
        var references = new[]
        {
            MakeReference("ref-0", "left"),
            MakeReference("ref-1", "right"),
            MakeReference("ref-2", "mixed"),
        };

        var results = adapter.CompareAll(observations, references, TestProvenance());

        Assert.Equal(6, results.Count); // 2 obs * 3 refs
    }

    [Fact]
    public void CampaignRunner_RunsAndProducesResult()
    {
        var clusters = new[]
        {
            MakeCluster("c0", "left", meanEv: 1.0),
            MakeCluster("c1", "right", meanEv: 2.0),
        };
        var references = new[]
        {
            MakeReference("ref-left", "left", meanMass: 1.0),
            MakeReference("ref-right", "right", meanMass: 2.0),
        };

        var campaign = FermionComparisonCampaignRunner.Run(
            "campaign-001", clusters, references,
            massLikeScaleTolerance: 0.5,
            TestProvenance());

        Assert.Equal("campaign-001", campaign.CampaignId);
        Assert.Equal(2, campaign.ClusterCount);
        Assert.Equal(2, campaign.ReferenceCount);
        Assert.Equal(4, campaign.ComparisonRecords.Count); // 2 * 2 = 4
        Assert.Equal(2, campaign.ObservationSummaries.Count);
        // At least 2 compatible (c0 vs ref-left, c1 vs ref-right)
        Assert.True(campaign.CompatibleCount >= 2);
    }

    [Fact]
    public void CampaignRunner_CompletionCriteria_AtLeastOneCampaignCompletes()
    {
        // M43 completion criteria: at least one comparison campaign involving
        // fermionic candidates completes.
        var clusters = new[] { MakeCluster("c0", "left") };
        var references = new[] { MakeReference("ref-0", "left") };

        var campaign = FermionComparisonCampaignRunner.Run(
            "m43-completion-test", clusters, references,
            massLikeScaleTolerance: 0.5,
            TestProvenance());

        // Campaign must complete and have non-null result
        Assert.NotNull(campaign);
        Assert.Equal("m43-completion-test", campaign.CampaignId);
        Assert.NotNull(campaign.ComparisonRecords);
        Assert.NotEmpty(campaign.ObservationSummaries);
    }
}
