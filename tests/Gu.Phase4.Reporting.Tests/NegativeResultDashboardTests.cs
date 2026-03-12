using Gu.Core;
using Gu.Phase4.FamilyClustering;
using Gu.Phase4.Fermions;
using Gu.Phase4.Registry;
using Gu.Phase4.Reporting;
using Xunit;

namespace Gu.Phase4.Reporting.Tests;

/// <summary>
/// Tests for NegativeResultDashboard and NegativeResultDashboardBuilder.
/// Negative results are first-class outputs: unstable chirality, fragile couplings,
/// broken family clusters.
/// </summary>
public class NegativeResultDashboardTests
{
    private static ProvenanceMeta TestProvenance() => new()
    {
        CreatedAt = DateTimeOffset.UtcNow,
        CodeRevision = "test",
        Branch = new BranchRef { BranchId = "test-branch", SchemaVersion = "1.0.0" },
        Backend = "cpu-reference",
    };

    private static FermionModeFamily MakeFamily(
        string id,
        string chirality = "left",
        double branchPersistence = 0.9,
        double gaugeLeakScore = 0.0,
        List<string>? ambiguityNotes = null)
        => new FermionModeFamily
        {
            FamilyId = id,
            MemberModeIds = new List<string> { $"{id}-m0" },
            BackgroundIds = new List<string> { "bg-1" },
            BranchVariantIds = new List<string> { "v1" },
            EigenvalueMagnitudeEnvelope = new[] { 0.9, 1.0, 1.1 },
            DominantChiralityProfile = chirality,
            BranchPersistenceScore = branchPersistence,
            RefinementPersistenceScore = 1.0,
            AverageGaugeLeakScore = gaugeLeakScore,
            AmbiguityNotes = ambiguityNotes ?? new List<string>(),
            Provenance = TestProvenance(),
        };

    private static FermionFamilyAtlas MakeAtlas(params FermionModeFamily[] families)
        => new FermionFamilyAtlas
        {
            AtlasId = "atlas-test",
            BranchFamilyId = "bf-test",
            ContextIds = new List<string> { "ctx-1" },
            BackgroundIds = new List<string> { "bg-1" },
            Families = families.ToList(),
            Provenance = TestProvenance(),
        };

    private static FamilyClusterRecord MakeCluster(
        string id,
        double persistence,
        double ambiguity = 0.0,
        List<string>? notes = null)
        => new FamilyClusterRecord
        {
            ClusterId = id,
            ClusterLabel = id,
            MemberFamilyIds = new List<string> { $"fam-{id}" },
            DominantChirality = "left",
            EigenvalueMagnitudeEnvelope = new[] { 0.9, 1.0, 1.1 },
            AmbiguityScore = ambiguity,
            MeanBranchPersistence = persistence,
            ClusteringMethod = "eigenvalue-proximity",
            ClusteringNotes = notes ?? new List<string>(),
            Provenance = TestProvenance(),
        };

    private static FermionAtlasSummary MakeFermionSummary(params FermionModeFamily[] families)
    {
        var atlas = MakeAtlas(families);
        var builder = new Phase4ReportBuilder("study-test");
        var reg = new UnifiedParticleRegistry();
        var report = builder.Build(atlas, reg, TestProvenance());
        return report.FermionAtlas;
    }

    // -------------------------------------------------------
    // Unstable chirality entries
    // -------------------------------------------------------

    [Fact]
    public void Dashboard_MixedChirality_ReportsUnstable()
    {
        var summary = MakeFermionSummary(MakeFamily("f0", chirality: "mixed"));
        var dashboard = new NegativeResultDashboardBuilder("study-1").Build(summary);

        Assert.Single(dashboard.UnstableChiralityEntries);
        Assert.Equal("f0", dashboard.UnstableChiralityEntries[0].FamilyId);
        Assert.Equal("mixed", dashboard.UnstableChiralityEntries[0].ChiralityStatus);
    }

    [Fact]
    public void Dashboard_TrivialChirality_ReportsUnstable()
    {
        var summary = MakeFermionSummary(MakeFamily("f0", chirality: "trivial"));
        var dashboard = new NegativeResultDashboardBuilder("study-1").Build(summary);

        Assert.Single(dashboard.UnstableChiralityEntries);
        Assert.Equal("trivial", dashboard.UnstableChiralityEntries[0].ChiralityStatus);
    }

    [Fact]
    public void Dashboard_DefiniteLeftChirality_NotReportedAsUnstable()
    {
        var summary = MakeFermionSummary(MakeFamily("f0", chirality: "left"));
        var dashboard = new NegativeResultDashboardBuilder("study-1").Build(summary);

        Assert.Empty(dashboard.UnstableChiralityEntries);
    }

    [Fact]
    public void Dashboard_HighGaugeLeakage_ReportsUnstable()
    {
        var summary = MakeFermionSummary(MakeFamily("f0", chirality: "left", gaugeLeakScore: 0.5));
        var dashboard = new NegativeResultDashboardBuilder("study-1").Build(summary);

        Assert.Single(dashboard.UnstableChiralityEntries);
        Assert.Equal(0.5, dashboard.UnstableChiralityEntries[0].LeakageNorm);
    }

    [Fact]
    public void Dashboard_UnstableChiralityEntry_HasNonEmptyReason()
    {
        var summary = MakeFermionSummary(MakeFamily("f0", chirality: "mixed"));
        var dashboard = new NegativeResultDashboardBuilder("study-1").Build(summary);

        Assert.NotEmpty(dashboard.UnstableChiralityEntries[0].Reason);
    }

    // -------------------------------------------------------
    // Fragile coupling entries
    // -------------------------------------------------------

    [Fact]
    public void Dashboard_BelowNoiseFloorCoupling_ReportsFragile()
    {
        var summary = MakeFermionSummary(MakeFamily("f0"));
        var dashboard = new NegativeResultDashboardBuilder("study-1")
            .AddCouplingMatrix(new CouplingMatrixSummary
            {
                BosonModeId = "b0",
                FermionPairCount = 2,
                MaxEntry = 0.0,
                FrobeniusNorm = 0.0,
            })
            .Build(summary);

        Assert.Single(dashboard.FragileCouplingEntries);
        Assert.Equal("b0", dashboard.FragileCouplingEntries[0].BosonModeId);
        Assert.Equal("below-noise-floor", dashboard.FragileCouplingEntries[0].FragilityReason);
    }

    [Fact]
    public void Dashboard_NonzeroCoupling_NotReportedAsFragile()
    {
        var summary = MakeFermionSummary(MakeFamily("f0"));
        var dashboard = new NegativeResultDashboardBuilder("study-1")
            .AddCouplingMatrix(new CouplingMatrixSummary
            {
                BosonModeId = "b0",
                FermionPairCount = 2,
                MaxEntry = 0.5,
                FrobeniusNorm = 0.7,
            })
            .Build(summary);

        Assert.Empty(dashboard.FragileCouplingEntries);
    }

    // -------------------------------------------------------
    // Broken family cluster entries
    // -------------------------------------------------------

    [Fact]
    public void Dashboard_LowPersistenceCluster_ReportsBroken()
    {
        var summary = MakeFermionSummary(MakeFamily("f0"));
        var dashboard = new NegativeResultDashboardBuilder("study-1")
            .AddFamilyCluster(MakeCluster("c0", persistence: 0.1))
            .Build(summary);

        Assert.Single(dashboard.BrokenFamilyClusterEntries);
        Assert.Equal("c0", dashboard.BrokenFamilyClusterEntries[0].ClusterId);
        Assert.Equal("low-persistence", dashboard.BrokenFamilyClusterEntries[0].FailureMode);
    }

    [Fact]
    public void Dashboard_HighAmbiguityCluster_ReportsBroken()
    {
        var summary = MakeFermionSummary(MakeFamily("f0"));
        var dashboard = new NegativeResultDashboardBuilder("study-1")
            .AddFamilyCluster(MakeCluster("c0", persistence: 0.8, ambiguity: 0.9))
            .Build(summary);

        Assert.Single(dashboard.BrokenFamilyClusterEntries);
        Assert.Equal("high-ambiguity", dashboard.BrokenFamilyClusterEntries[0].FailureMode);
    }

    [Fact]
    public void Dashboard_SplitNoteCluster_ReportsBroken()
    {
        var summary = MakeFermionSummary(MakeFamily("f0"));
        var dashboard = new NegativeResultDashboardBuilder("study-1")
            .AddFamilyCluster(MakeCluster("c0", persistence: 0.8,
                notes: new List<string> { "split detected at step 3" }))
            .Build(summary);

        Assert.Single(dashboard.BrokenFamilyClusterEntries);
        Assert.Equal("split-detected", dashboard.BrokenFamilyClusterEntries[0].FailureMode);
    }

    [Fact]
    public void Dashboard_MergeNoteCluster_ReportsBroken()
    {
        var summary = MakeFermionSummary(MakeFamily("f0"));
        var dashboard = new NegativeResultDashboardBuilder("study-1")
            .AddFamilyCluster(MakeCluster("c0", persistence: 0.8,
                notes: new List<string> { "merge event detected" }))
            .Build(summary);

        Assert.Single(dashboard.BrokenFamilyClusterEntries);
        Assert.Equal("merge-detected", dashboard.BrokenFamilyClusterEntries[0].FailureMode);
    }

    [Fact]
    public void Dashboard_CrossingNoteCluster_ReportsBroken()
    {
        var summary = MakeFermionSummary(MakeFamily("f0"));
        var dashboard = new NegativeResultDashboardBuilder("study-1")
            .AddFamilyCluster(MakeCluster("c0", persistence: 0.8,
                notes: new List<string> { "avoided crossing near eigenvalue 1.5" }))
            .Build(summary);

        Assert.Single(dashboard.BrokenFamilyClusterEntries);
        Assert.Equal("avoided-crossing", dashboard.BrokenFamilyClusterEntries[0].FailureMode);
    }

    [Fact]
    public void Dashboard_StableCluster_NotReportedAsBroken()
    {
        var summary = MakeFermionSummary(MakeFamily("f0"));
        var dashboard = new NegativeResultDashboardBuilder("study-1")
            .AddFamilyCluster(MakeCluster("c0", persistence: 0.9, ambiguity: 0.1))
            .Build(summary);

        Assert.Empty(dashboard.BrokenFamilyClusterEntries);
    }

    // -------------------------------------------------------
    // Additional notes and demotion log
    // -------------------------------------------------------

    [Fact]
    public void Dashboard_RegistryDemotions_AppearInAdditionalNotes()
    {
        var summary = MakeFermionSummary(MakeFamily("f0"));
        var registry = new UnifiedParticleRegistry();
        registry.Register(new UnifiedParticleRecord
        {
            ParticleId = "p0",
            ParticleType = UnifiedParticleType.Fermion,
            PrimarySourceId = "s0",
            ContributingSourceIds = new List<string>(),
            BranchVariantSet = new List<string>(),
            BackgroundSet = new List<string>(),
            MassLikeEnvelope = new[] { 0.9, 1.0, 1.1 },
            ClaimClass = "C1_LocalPersistentMode",
            RegistryVersion = "v1",
            Provenance = TestProvenance(),
            Demotions = new List<ParticleClaimDemotion>
            {
                new ParticleClaimDemotion
                {
                    Reason = "LowPersistence",
                    Details = "Persistence 0.2 below threshold.",
                    FromClaimClass = "C2_BranchStableBosonicCandidate",
                    ToClaimClass = "C1_LocalPersistentMode",
                },
            },
        });

        var dashboard = new NegativeResultDashboardBuilder("study-1").Build(summary, registry);

        Assert.Contains(dashboard.AdditionalNotes,
            n => n.Contains("LowPersistence") && n.Contains("p0"));
    }

    [Fact]
    public void Dashboard_AmbiguityNotes_AppearInAdditionalNotes()
    {
        var summary = MakeFermionSummary(MakeFamily("f0"));
        var registry = new UnifiedParticleRegistry();
        registry.Register(new UnifiedParticleRecord
        {
            ParticleId = "p0",
            ParticleType = UnifiedParticleType.Boson,
            PrimarySourceId = "s0",
            ContributingSourceIds = new List<string>(),
            BranchVariantSet = new List<string>(),
            BackgroundSet = new List<string>(),
            MassLikeEnvelope = new[] { 0.9, 1.0, 1.1 },
            ClaimClass = "C0_NumericalMode",
            RegistryVersion = "v1",
            Provenance = TestProvenance(),
            AmbiguityNotes = new List<string> { "multiple candidate matches found" },
        });

        var dashboard = new NegativeResultDashboardBuilder("study-1").Build(summary, registry);

        Assert.Contains(dashboard.AdditionalNotes,
            n => n.Contains("[ambiguity]") && n.Contains("p0"));
    }

    [Fact]
    public void Dashboard_ManualNotes_AppearInAdditionalNotes()
    {
        var summary = MakeFermionSummary(MakeFamily("f0"));
        var dashboard = new NegativeResultDashboardBuilder("study-1")
            .AddNote("Manual note: no spinor zero-mode found")
            .Build(summary);

        Assert.Contains(dashboard.AdditionalNotes, n => n.Contains("Manual note"));
    }

    // -------------------------------------------------------
    // TotalNegativeResults
    // -------------------------------------------------------

    [Fact]
    public void Dashboard_TotalNegativeResults_CountsAllCategories()
    {
        var summary = MakeFermionSummary(MakeFamily("f0", chirality: "mixed"));
        var dashboard = new NegativeResultDashboardBuilder("study-1")
            .AddCouplingMatrix(new CouplingMatrixSummary
            {
                BosonModeId = "b0", FermionPairCount = 1, MaxEntry = 0.0, FrobeniusNorm = 0.0,
            })
            .AddFamilyCluster(MakeCluster("c0", persistence: 0.1))
            .AddNote("extra note")
            .Build(summary);

        Assert.Single(dashboard.UnstableChiralityEntries);
        Assert.Single(dashboard.FragileCouplingEntries);
        Assert.Single(dashboard.BrokenFamilyClusterEntries);
        Assert.Equal(1 + 1 + 1 + dashboard.AdditionalNotes.Count, dashboard.TotalNegativeResults);
    }

    // -------------------------------------------------------
    // Dashboard ID and StudyId
    // -------------------------------------------------------

    [Fact]
    public void Dashboard_DashboardId_ContainsStudyId()
    {
        var summary = MakeFermionSummary(MakeFamily("f0"));
        var dashboard = new NegativeResultDashboardBuilder("my-study").Build(summary);

        Assert.Contains("my-study", dashboard.DashboardId);
        Assert.Equal("my-study", dashboard.StudyId);
    }

    // -------------------------------------------------------
    // JSON serialization
    // -------------------------------------------------------

    [Fact]
    public void Dashboard_JsonRoundTrip_PreservesEntries()
    {
        var summary = MakeFermionSummary(
            MakeFamily("f0", chirality: "mixed"),
            MakeFamily("f1", chirality: "left"));
        var dashboard = new NegativeResultDashboardBuilder("study-1")
            .AddCouplingMatrix(new CouplingMatrixSummary
            {
                BosonModeId = "b0", FermionPairCount = 1, MaxEntry = 0.0, FrobeniusNorm = 0.0,
            })
            .AddFamilyCluster(MakeCluster("c0", persistence: 0.1))
            .Build(summary);

        var json = System.Text.Json.JsonSerializer.Serialize(dashboard);
        var loaded = System.Text.Json.JsonSerializer.Deserialize<NegativeResultDashboard>(json)!;

        Assert.Equal(dashboard.UnstableChiralityEntries.Count, loaded.UnstableChiralityEntries.Count);
        Assert.Equal(dashboard.FragileCouplingEntries.Count, loaded.FragileCouplingEntries.Count);
        Assert.Equal(dashboard.BrokenFamilyClusterEntries.Count, loaded.BrokenFamilyClusterEntries.Count);
        Assert.Equal(dashboard.TotalNegativeResults, loaded.TotalNegativeResults);
    }
}
