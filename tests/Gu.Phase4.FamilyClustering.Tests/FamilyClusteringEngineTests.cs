using Gu.Core;
using Gu.Phase4.FamilyClustering;
using Gu.Phase4.Fermions;

namespace Gu.Phase4.FamilyClustering.Tests;

/// <summary>
/// Tests for M41: FamilyClusteringEngine and FamilyClusterReportBuilder.
/// Completion criterion: candidate family-like clusters are emitted with
/// stable IDs and provenance.
/// </summary>
public class FamilyClusteringEngineTests
{
    private static ProvenanceMeta TestProvenance() => new()
    {
        CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
        CodeRevision = "test-m41",
        Branch = new BranchRef { BranchId = "test-branch", SchemaVersion = "1.0.0" },
        Backend = "cpu-reference",
    };

    private static FamilyClusteringConfig DefaultConfig() =>
        new FamilyClusteringConfig { EigenvalueProximityRelTol = 0.3 };

    private static FermionModeFamily MakeFamily(
        string familyId,
        string chirality,
        double meanEigenvalue,
        string? conjugateFamilyId = null,
        double branchPersistence = 1.0,
        bool hasAmbiguity = false)
    {
        double minEv = meanEigenvalue * 0.9;
        double maxEv = meanEigenvalue * 1.1;
        var notes = hasAmbiguity ? new List<string> { "Ambiguous match at step 2." } : new List<string>();
        return new FermionModeFamily
        {
            FamilyId = familyId,
            MemberModeIds = new List<string> { $"{familyId}-m0" },
            BackgroundIds = new List<string> { "bg-1" },
            BranchVariantIds = new List<string> { "v1" },
            EigenvalueMagnitudeEnvelope = new[] { minEv, meanEigenvalue, maxEv },
            DominantChiralityProfile = chirality,
            HasConjugationPair = conjugateFamilyId is not null,
            ConjugateFamilyId = conjugateFamilyId,
            BranchPersistenceScore = branchPersistence,
            RefinementPersistenceScore = 0.0,
            AverageGaugeLeakScore = 0.0,
            AmbiguityNotes = notes,
            Provenance = TestProvenance(),
        };
    }

    // -------------------------------------------------------
    // Basic clustering tests
    // -------------------------------------------------------

    [Fact]
    public void Cluster_Empty_ReturnsEmpty()
    {
        var engine = new FamilyClusteringEngine(DefaultConfig());
        var result = engine.Cluster(Array.Empty<FermionModeFamily>(), TestProvenance());
        Assert.Empty(result);
    }

    [Fact]
    public void Cluster_SingleFamily_ReturnsSingletonCluster()
    {
        var engine = new FamilyClusteringEngine(DefaultConfig());
        var families = new[] { MakeFamily("f0", "left", 1.0) };

        var clusters = engine.Cluster(families, TestProvenance());

        Assert.Single(clusters);
        Assert.Equal("singleton", clusters[0].ClusteringMethod);
        Assert.Single(clusters[0].MemberFamilyIds);
        Assert.Equal("f0", clusters[0].MemberFamilyIds[0]);
    }

    [Fact]
    public void Cluster_ConjugatePair_ProducesConjugationRuleCluster()
    {
        var engine = new FamilyClusteringEngine(DefaultConfig());
        // f0 and f1 are conjugation partners (left/right, same eigenvalue)
        var families = new[]
        {
            MakeFamily("f0", "left",  1.0, conjugateFamilyId: "f1"),
            MakeFamily("f1", "right", 1.0, conjugateFamilyId: "f0"),
        };

        var clusters = engine.Cluster(families, TestProvenance());

        Assert.Single(clusters);
        Assert.Equal("conjugation-rule", clusters[0].ClusteringMethod);
        Assert.Equal(2, clusters[0].MemberFamilyIds.Count);
        Assert.True(clusters[0].HasConjugatePair);
    }

    [Fact]
    public void Cluster_ConjugatePair_DominantChiralityIsConjugatePair()
    {
        var engine = new FamilyClusteringEngine(DefaultConfig());
        var families = new[]
        {
            MakeFamily("fL", "left",  2.0, conjugateFamilyId: "fR"),
            MakeFamily("fR", "right", 2.0, conjugateFamilyId: "fL"),
        };

        var clusters = engine.Cluster(families, TestProvenance());

        Assert.Single(clusters);
        Assert.Equal("conjugate-pair", clusters[0].DominantChirality);
    }

    [Fact]
    public void Cluster_ClusterIdsAreStable_AndUseIndex()
    {
        var engine = new FamilyClusteringEngine(DefaultConfig());
        var families = new[]
        {
            MakeFamily("f0", "left",  1.0, conjugateFamilyId: "f1"),
            MakeFamily("f1", "right", 1.0, conjugateFamilyId: "f0"),
            MakeFamily("f2", "mixed", 10.0), // singleton — far from others
        };

        var clusters = engine.Cluster(families, TestProvenance());

        // Clusters sorted by mean eigenvalue, so conjugate-pair cluster (ev≈1) comes first
        Assert.Equal(2, clusters.Count);
        Assert.Equal("cluster-0", clusters[0].ClusterId);
        Assert.Equal("cluster-1", clusters[1].ClusterId);
    }

    [Fact]
    public void Cluster_ProvenancePreserved()
    {
        var engine = new FamilyClusteringEngine(DefaultConfig());
        var prov = TestProvenance();
        var families = new[] { MakeFamily("f0", "left", 1.0) };

        var clusters = engine.Cluster(families, prov);

        Assert.Equal(prov.CodeRevision, clusters[0].Provenance.CodeRevision);
    }

    [Fact]
    public void Cluster_EigenvalueProximity_GroupsCloseFamilies()
    {
        var engine = new FamilyClusteringEngine(DefaultConfig()); // relTol=0.3
        // f0 at 1.0, f1 at 1.2 — relative diff = 0.2 < 0.3 → should cluster
        var families = new[]
        {
            MakeFamily("f0", "mixed", 1.0),
            MakeFamily("f1", "mixed", 1.2),
        };

        var clusters = engine.Cluster(families, TestProvenance());

        // Both are "unclaimed" (no conjugation pairs) but proximate
        Assert.Single(clusters);
        Assert.Equal(2, clusters[0].MemberFamilyIds.Count);
        Assert.Equal("eigenvalue-proximity", clusters[0].ClusteringMethod);
    }

    [Fact]
    public void Cluster_EigenvalueProximity_DoesNotGroupDistantFamilies()
    {
        var engine = new FamilyClusteringEngine(DefaultConfig()); // relTol=0.3
        // f0 at 1.0, f1 at 5.0 — relative diff > 0.3 → two singletons
        var families = new[]
        {
            MakeFamily("f0", "mixed", 1.0),
            MakeFamily("f1", "mixed", 5.0),
        };

        var clusters = engine.Cluster(families, TestProvenance());

        Assert.Equal(2, clusters.Count);
        Assert.All(clusters, c => Assert.Equal("singleton", c.ClusteringMethod));
    }

    [Fact]
    public void Cluster_AmbiguityScore_ReflectsAmbiguousFamilies()
    {
        var engine = new FamilyClusteringEngine(DefaultConfig());
        var families = new[]
        {
            MakeFamily("f0", "left",  1.0, conjugateFamilyId: "f1", hasAmbiguity: true),
            MakeFamily("f1", "right", 1.0, conjugateFamilyId: "f0", hasAmbiguity: false),
        };

        var clusters = engine.Cluster(families, TestProvenance());

        Assert.Single(clusters);
        // 1 of 2 members has ambiguity → ambiguityScore = 0.5
        Assert.Equal(0.5, clusters[0].AmbiguityScore, 6);
    }

    [Fact]
    public void Cluster_MeanBranchPersistence_IsAverageOfMembers()
    {
        var engine = new FamilyClusteringEngine(DefaultConfig());
        var families = new[]
        {
            MakeFamily("f0", "left",  1.0, conjugateFamilyId: "f1", branchPersistence: 0.8),
            MakeFamily("f1", "right", 1.0, conjugateFamilyId: "f0", branchPersistence: 0.6),
        };

        var clusters = engine.Cluster(families, TestProvenance());

        Assert.Single(clusters);
        Assert.Equal(0.7, clusters[0].MeanBranchPersistence, 6);
    }

    [Fact]
    public void Cluster_EigenvalueEnvelope_CoversAllMembersInCluster()
    {
        var engine = new FamilyClusteringEngine(DefaultConfig());
        // f0 at 1.0 (range [0.9, 1.1]), f1 at 1.2 (range [1.08, 1.32])
        var families = new[]
        {
            MakeFamily("f0", "mixed", 1.0),
            MakeFamily("f1", "mixed", 1.2),
        };

        var clusters = engine.Cluster(families, TestProvenance());

        Assert.Single(clusters);
        var env = clusters[0].EigenvalueMagnitudeEnvelope;
        Assert.Equal(3, env.Length);
        Assert.True(env[0] <= env[1], "min <= mean");
        Assert.True(env[1] <= env[2], "mean <= max");
        Assert.Equal(0.9, env[0], 6); // min of f0's min
        Assert.Equal(1.32, env[2], 6); // max of f1's max
    }

    [Fact]
    public void Cluster_ThreeFamilies_ConjugatePairPlusSingleton()
    {
        var engine = new FamilyClusteringEngine(DefaultConfig());
        var families = new[]
        {
            MakeFamily("fL", "left",  1.0, conjugateFamilyId: "fR"),
            MakeFamily("fR", "right", 1.0, conjugateFamilyId: "fL"),
            MakeFamily("fX", "mixed", 50.0), // distant singleton
        };

        var clusters = engine.Cluster(families, TestProvenance());

        Assert.Equal(2, clusters.Count);
        var conjugateCluster = clusters.Single(c => c.ClusteringMethod == "conjugation-rule");
        var singletonCluster = clusters.Single(c => c.ClusteringMethod == "singleton");
        Assert.Equal(2, conjugateCluster.MemberFamilyIds.Count);
        Assert.Single(singletonCluster.MemberFamilyIds);
        Assert.Equal("fX", singletonCluster.MemberFamilyIds[0]);
    }

    // -------------------------------------------------------
    // FamilyClusterReportBuilder tests
    // -------------------------------------------------------

    [Fact]
    public void ReportBuilder_Build_ProducesNonNullReport()
    {
        var atlas = BuildMinimalAtlas();
        var builder = new FamilyClusterReportBuilder(DefaultConfig());

        var report = builder.Build(atlas, "report-1", TestProvenance());

        Assert.NotNull(report);
        Assert.Equal("report-1", report.ReportId);
        Assert.Equal(atlas.AtlasId, report.SourceAtlasId);
    }

    [Fact]
    public void ReportBuilder_Build_SummaryIsConsistent()
    {
        var atlas = BuildMinimalAtlas();
        var builder = new FamilyClusterReportBuilder(DefaultConfig());

        var report = builder.Build(atlas, "report-2", TestProvenance());

        Assert.Equal(report.Clusters.Count, report.Summary.TotalClusters);
        Assert.Equal(report.ConjugationRuleClusters, report.Summary.ConjugationRuleClusters);
        Assert.Equal(report.SingletonClusters, report.Summary.SingletonClusters);
    }

    [Fact]
    public void ReportBuilder_Build_EmptyAtlas_ProducesEmptyReport()
    {
        var atlas = new FermionFamilyAtlas
        {
            AtlasId = "empty-atlas",
            BranchFamilyId = "bf-0",
            ContextIds = new List<string>(),
            BackgroundIds = new List<string>(),
            Families = new List<FermionModeFamily>(),
            Provenance = TestProvenance(),
        };
        var builder = new FamilyClusterReportBuilder(DefaultConfig());

        var report = builder.Build(atlas, "report-empty", TestProvenance());

        Assert.NotNull(report);
        Assert.Empty(report.Clusters);
        Assert.Equal(0, report.TotalClusters);
    }

    // -------------------------------------------------------
    // Helper
    // -------------------------------------------------------

    private static FermionFamilyAtlas BuildMinimalAtlas()
    {
        var prov = TestProvenance();
        var families = new List<FermionModeFamily>
        {
            MakeFamily("fL", "left",  1.0, conjugateFamilyId: "fR"),
            MakeFamily("fR", "right", 1.0, conjugateFamilyId: "fL"),
            MakeFamily("fX", "mixed", 5.0),
        };

        return new FermionFamilyAtlas
        {
            AtlasId = "test-atlas",
            BranchFamilyId = "bf-1",
            ContextIds = new List<string> { "ctx-1" },
            BackgroundIds = new List<string> { "bg-1" },
            Families = families,
            Provenance = prov,
        };
    }
}
