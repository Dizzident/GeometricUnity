using Gu.Core;
using Gu.Phase4.Couplings;
using Gu.Phase4.FamilyClustering;
using Gu.Phase4.Observation;
using Xunit;

namespace Gu.Phase4.Observation.Tests;

/// <summary>
/// Tests for FermionObservationPipeline — the class-based pipeline wrapping
/// FermionObservationBuilder logic with batch observation and interaction summaries.
/// </summary>
public sealed class FermionObservationPipelineTests
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

    private static CouplingAtlas MakeAtlas(
        string atlasId,
        IEnumerable<(string bosonId, string fi, string fj, double magnitude)> couplings)
    {
        var prov = TestProvenance();
        return new CouplingAtlas
        {
            AtlasId = atlasId,
            FermionBackgroundId = "bg-001",
            BosonRegistryVersion = "v1",
            NormalizationConvention = "raw",
            Couplings = couplings.Select(c => new BosonFermionCouplingRecord
            {
                CouplingId = $"coup-{c.bosonId}-{c.fi}-{c.fj}",
                BosonModeId = c.bosonId,
                FermionModeIdI = c.fi,
                FermionModeIdJ = c.fj,
                CouplingProxyReal = c.magnitude,
                CouplingProxyMagnitude = c.magnitude,
                NormalizationConvention = "raw",
                Provenance = prov,
            }).ToList(),
            Provenance = prov,
        };
    }

    // -------- ObserveClusters --------

    [Fact]
    public void ObserveClusters_Empty_ReturnsEmptyList()
    {
        var pipeline = new FermionObservationPipeline();
        var results = pipeline.ObserveClusters(Array.Empty<FamilyClusterRecord>(), TestProvenance());
        Assert.Empty(results);
    }

    [Fact]
    public void ObserveClusters_SingleLeftCluster_ProducesOneSummary()
    {
        var pipeline = new FermionObservationPipeline();
        var cluster = MakeCluster("c0", "left");
        var results = pipeline.ObserveClusters(new[] { cluster }, TestProvenance());

        Assert.Single(results);
        Assert.Equal("c0", results[0].ClusterId);
        Assert.Equal("left", results[0].ObservedChirality);
    }

    [Fact]
    public void ObserveClusters_ThreeClusters_ProducesThreeSummaries()
    {
        var pipeline = new FermionObservationPipeline();
        var clusters = new[]
        {
            MakeCluster("c0", "left"),
            MakeCluster("c1", "right"),
            MakeCluster("c2", "mixed"),
        };

        var results = pipeline.ObserveClusters(clusters, TestProvenance());
        Assert.Equal(3, results.Count);
    }

    [Fact]
    public void ObserveClusters_MixedChirality_IsTrivial()
    {
        var pipeline = new FermionObservationPipeline();
        var cluster = MakeCluster("c-mixed", "mixed");
        var results = pipeline.ObserveClusters(new[] { cluster }, TestProvenance());

        Assert.True(results[0].IsTrivial);
    }

    [Fact]
    public void ObserveClusters_LeftChirality_NotTrivial()
    {
        var pipeline = new FermionObservationPipeline();
        var cluster = MakeCluster("c-left", "left", branchPersistence: 0.9);
        var results = pipeline.ObserveClusters(new[] { cluster }, TestProvenance());

        Assert.False(results[0].IsTrivial);
    }

    [Fact]
    public void ObserveClusters_MassLikeEnvelopeHasThreeEntries()
    {
        var pipeline = new FermionObservationPipeline();
        var cluster = MakeCluster("c0", "right", meanEv: 2.5);
        var results = pipeline.ObserveClusters(new[] { cluster }, TestProvenance());

        var envelope = results[0].MassLikeEnvelope;
        Assert.Equal(3, envelope.Length);
        Assert.Equal(2.5, envelope[1], precision: 10);
    }

    [Fact]
    public void ObserveClusters_ConjugatePairCluster_RecordsHasConjugatePair()
    {
        var pipeline = new FermionObservationPipeline();
        var cluster = MakeCluster("c-pair", "conjugate-pair", hasConjugatePair: true);
        var results = pipeline.ObserveClusters(new[] { cluster }, TestProvenance());

        Assert.True(results[0].HasConjugatePair);
    }

    [Fact]
    public void ObserveClusters_HighAmbiguity_HasNote()
    {
        var pipeline = new FermionObservationPipeline();
        var cluster = MakeCluster("c-amb", "left", ambiguityScore: 0.8);
        var results = pipeline.ObserveClusters(new[] { cluster }, TestProvenance());

        Assert.NotEmpty(results[0].Notes);
    }

    // -------- ObserveInteractions --------

    [Fact]
    public void ObserveInteractions_EmptyCouplingAtlas_ReturnsEmpty()
    {
        var pipeline = new FermionObservationPipeline();
        var atlas = MakeAtlas("atlas-empty", Enumerable.Empty<(string, string, string, double)>());

        var results = pipeline.ObserveInteractions(atlas, TestProvenance());
        Assert.Empty(results);
    }

    [Fact]
    public void ObserveInteractions_SingleBosonMode_ProducesOneSummary()
    {
        var pipeline = new FermionObservationPipeline();
        var atlas = MakeAtlas("atlas-1", new[]
        {
            ("b-0", "f-0", "f-1", 0.5),
            ("b-0", "f-1", "f-0", 0.3),
        });

        var results = pipeline.ObserveInteractions(atlas, TestProvenance());
        Assert.Single(results);
        Assert.Equal("b-0", results[0].BosonModeId);
    }

    [Fact]
    public void ObserveInteractions_TwoBosonModes_ProducesTwoSummaries()
    {
        var pipeline = new FermionObservationPipeline();
        var atlas = MakeAtlas("atlas-2", new[]
        {
            ("b-0", "f-0", "f-1", 0.5),
            ("b-1", "f-0", "f-1", 0.2),
        });

        var results = pipeline.ObserveInteractions(atlas, TestProvenance());
        Assert.Equal(2, results.Count);
    }

    [Fact]
    public void ObserveInteractions_MeanMagnitude_ComputedCorrectly()
    {
        var pipeline = new FermionObservationPipeline();
        var atlas = MakeAtlas("atlas-3", new[]
        {
            ("b-0", "f-0", "f-1", 0.4),
            ("b-0", "f-1", "f-0", 0.6),
        });

        var results = pipeline.ObserveInteractions(atlas, TestProvenance());
        Assert.Single(results);
        Assert.Equal(0.5, results[0].MeanCouplingMagnitude, precision: 10);
    }

    [Fact]
    public void ObserveInteractions_MaxMagnitude_ComputedCorrectly()
    {
        var pipeline = new FermionObservationPipeline();
        var atlas = MakeAtlas("atlas-4", new[]
        {
            ("b-0", "f-0", "f-1", 0.1),
            ("b-0", "f-1", "f-0", 0.9),
        });

        var results = pipeline.ObserveInteractions(atlas, TestProvenance());
        Assert.Equal(0.9, results[0].MaxCouplingMagnitude, precision: 10);
    }

    [Fact]
    public void ObserveInteractions_NonZeroCouplingCount_ExcludesZeros()
    {
        var pipeline = new FermionObservationPipeline();
        var atlas = MakeAtlas("atlas-5", new[]
        {
            ("b-0", "f-0", "f-1", 0.5),
            ("b-0", "f-1", "f-0", 0.0),
        });

        var results = pipeline.ObserveInteractions(atlas, TestProvenance());
        Assert.Equal(1, results[0].NonZeroCouplingCount);
    }

    [Fact]
    public void ObserveInteractions_FermionModeIds_ContainContributors()
    {
        var pipeline = new FermionObservationPipeline();
        var atlas = MakeAtlas("atlas-6", new[]
        {
            ("b-0", "f-alice", "f-bob", 0.5),
        });

        var results = pipeline.ObserveInteractions(atlas, TestProvenance());
        var ids = results[0].FermionModeIds;
        Assert.Contains("f-alice", ids);
        Assert.Contains("f-bob", ids);
    }

    [Fact]
    public void ObserveInteractions_NormalizationConventionPreserved()
    {
        var pipeline = new FermionObservationPipeline();
        var atlas = MakeAtlas("atlas-7", new[]
        {
            ("b-0", "f-0", "f-1", 0.5),
        });

        var results = pipeline.ObserveInteractions(atlas, TestProvenance());
        Assert.Equal("raw", results[0].NormalizationConvention);
    }

    [Fact]
    public void ObserveInteractions_AtlasIdPreserved()
    {
        var pipeline = new FermionObservationPipeline();
        var atlas = MakeAtlas("my-atlas", new[]
        {
            ("b-0", "f-0", "f-1", 0.5),
        });

        var results = pipeline.ObserveInteractions(atlas, TestProvenance());
        Assert.Equal("my-atlas", results[0].AtlasId);
    }
}
