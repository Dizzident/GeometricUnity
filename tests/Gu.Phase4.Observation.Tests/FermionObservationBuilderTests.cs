using Gu.Core;
using Gu.Phase4.FamilyClustering;
using Gu.Phase4.Observation;
using Xunit;

namespace Gu.Phase4.Observation.Tests;

public sealed class FermionObservationBuilderTests
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
        double branchPersistence = 1.0,
        double ambiguityScore = 0.0,
        bool hasConjugatePair = false,
        string method = "eigenvalue-proximity")
        => new FamilyClusterRecord
        {
            ClusterId = id,
            ClusterLabel = id,
            MemberFamilyIds = new List<string> { $"family-{id}" },
            DominantChirality = chirality,
            HasConjugatePair = hasConjugatePair,
            EigenvalueMagnitudeEnvelope = new[] { meanEv * 0.9, meanEv, meanEv * 1.1 },
            AmbiguityScore = ambiguityScore,
            MeanBranchPersistence = branchPersistence,
            ClusteringMethod = method,
            Provenance = TestProvenance(),
        };

    [Fact]
    public void Build_LeftChiralCluster_ProducesCorrectSummary()
    {
        var cluster = MakeCluster("c0", "left", meanEv: 1.5, branchPersistence: 0.9);
        var summary = FermionObservationBuilder.Build(cluster, TestProvenance());

        Assert.Equal("c0", summary.ClusterId);
        Assert.Equal("left", summary.ObservedChirality);
        Assert.Equal(3, summary.MassLikeEnvelope.Length);
        Assert.Equal(1.5, summary.MassLikeEnvelope[1], precision: 10);
        Assert.Equal(0.9, summary.BranchPersistenceScore, precision: 10);
        Assert.False(summary.IsTrivial);
        // Per D-P11-010: proxy-observation note is always included.
        Assert.NotEmpty(summary.Notes);
    }

    [Fact]
    public void Build_MixedChiralCluster_IsTrivial()
    {
        var cluster = MakeCluster("c-mixed", "mixed");
        var summary = FermionObservationBuilder.Build(cluster, TestProvenance());

        Assert.True(summary.IsTrivial);
        Assert.NotEmpty(summary.Notes);
    }

    [Fact]
    public void Build_UndeterminedChiralCluster_IsTrivial()
    {
        var cluster = MakeCluster("c-undet", "undetermined");
        var summary = FermionObservationBuilder.Build(cluster, TestProvenance());

        Assert.True(summary.IsTrivial);
    }

    [Fact]
    public void Build_SingletonLowPersistence_IsTrivial()
    {
        var cluster = MakeCluster("c-singleton", "left", branchPersistence: 0.3, method: "singleton");
        var summary = FermionObservationBuilder.Build(cluster, TestProvenance());

        Assert.True(summary.IsTrivial);
    }

    [Fact]
    public void Build_SingletonHighPersistence_IsNotTrivial()
    {
        var cluster = MakeCluster("c-single-ok", "right", branchPersistence: 0.9, method: "singleton");
        var summary = FermionObservationBuilder.Build(cluster, TestProvenance());

        Assert.False(summary.IsTrivial);
    }

    [Fact]
    public void Build_WithConjugatePair_RecordsHasConjugatePair()
    {
        var cluster = MakeCluster("c-conj", "left", hasConjugatePair: true);
        var summary = FermionObservationBuilder.Build(cluster, TestProvenance());

        Assert.True(summary.HasConjugatePair);
    }

    [Fact]
    public void Build_WithAmbiguity_HasNote()
    {
        var cluster = MakeCluster("c-amb", "left", ambiguityScore: 0.3);
        var summary = FermionObservationBuilder.Build(cluster, TestProvenance());

        Assert.Contains(summary.Notes, n => n.Contains("Ambiguity"));
    }

    [Fact]
    public void BuildAll_ProducesOnePerCluster()
    {
        var clusters = new[]
        {
            MakeCluster("c0", "left"),
            MakeCluster("c1", "right"),
            MakeCluster("c2", "mixed"),
        };

        var summaries = FermionObservationBuilder.BuildAll(clusters, TestProvenance());

        Assert.Equal(3, summaries.Count);
        Assert.Equal("c0", summaries[0].ClusterId);
        Assert.Equal("c1", summaries[1].ClusterId);
        Assert.Equal("c2", summaries[2].ClusterId);
    }

    [Fact]
    public void Build_ProviderVersion_IsSet()
    {
        var cluster = MakeCluster("c0", "left");
        var summary = FermionObservationBuilder.Build(cluster, TestProvenance());

        Assert.Equal("1.0.0", summary.SchemaVersion);
    }

    // -------- ObservationPathLabel contract tests (P11-M10) --------

    [Fact]
    public void Build_ObservationPathLabel_IsProxyObservation()
    {
        // Per D-P11-010: all current observations must be labeled proxy-observation.
        var cluster = MakeCluster("c0", "left");
        var summary = FermionObservationBuilder.Build(cluster, TestProvenance());

        Assert.Equal(ObservationPathLabels.ProxyObservation, summary.ObservationPathLabel);
    }

    [Fact]
    public void Build_ObservationPathLabel_IsNeverFullPullback()
    {
        // Enforce D-P11-010: current builder must not produce full-pullback label.
        var cluster = MakeCluster("c0", "right");
        var summary = FermionObservationBuilder.Build(cluster, TestProvenance());

        Assert.NotEqual(ObservationPathLabels.FullPullback, summary.ObservationPathLabel);
    }

    [Fact]
    public void Build_Notes_ContainProxyObservationWarning()
    {
        // Reports that read the notes field will surface the proxy-observation caveat.
        var cluster = MakeCluster("c0", "left");
        var summary = FermionObservationBuilder.Build(cluster, TestProvenance());

        Assert.Contains(summary.Notes,
            n => n.Contains("proxy-observation", StringComparison.Ordinal));
    }

    [Fact]
    public void BuildAll_AllSummaries_HaveProxyObservationLabel()
    {
        var clusters = new[]
        {
            MakeCluster("c0", "left"),
            MakeCluster("c1", "right"),
            MakeCluster("c2", "mixed"),
        };

        var summaries = FermionObservationBuilder.BuildAll(clusters, TestProvenance());

        Assert.All(summaries, s =>
            Assert.Equal(ObservationPathLabels.ProxyObservation, s.ObservationPathLabel));
    }
}
