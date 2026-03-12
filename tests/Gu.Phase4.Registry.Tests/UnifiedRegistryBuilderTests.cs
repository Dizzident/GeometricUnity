using Gu.Core;
using Gu.Phase3.Registry;
using Gu.Phase4.Couplings;
using Gu.Phase4.FamilyClustering;
using Gu.Phase4.Registry;
using Xunit;

namespace Gu.Phase4.Registry.Tests;

public sealed class UnifiedRegistryBuilderTests
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
        double meanEv,
        string chirality,
        double branchPersistence = 1.0,
        double ambiguityScore = 0.0,
        string method = "eigenvalue-proximity")
        => new FamilyClusterRecord
        {
            ClusterId = id,
            ClusterLabel = id,
            MemberFamilyIds = new List<string> { $"family-{id}" },
            DominantChirality = chirality,
            HasConjugatePair = chirality == "conjugate-pair",
            EigenvalueMagnitudeEnvelope = new[] { meanEv * 0.9, meanEv, meanEv * 1.1 },
            AmbiguityScore = ambiguityScore,
            MeanBranchPersistence = branchPersistence,
            ClusteringMethod = method,
            Provenance = TestProvenance(),
        };

    private static CandidateBosonRecord MakeBoson(
        string id,
        double massLike,
        BosonClaimClass claimClass,
        bool unverifiedGpu = false)
        => new CandidateBosonRecord
        {
            CandidateId = id,
            PrimaryFamilyId = $"fam-{id}",
            ContributingModeIds = new[] { $"mode-{id}" },
            BackgroundSet = new[] { "bg-001" },
            MassLikeEnvelope = new[] { massLike * 0.9, massLike, massLike * 1.1 },
            MultiplicityEnvelope = new[] { 1, 1, 1 },
            GaugeLeakEnvelope = new[] { 0.0, 0.0, 0.0 },
            ClaimClass = claimClass,
            ComputedWithUnverifiedGpu = unverifiedGpu,
            RegistryVersion = "1.0",
        };

    [Fact]
    public void Build_EmptyInputs_ReturnsEmptyRegistry()
    {
        var builder = new UnifiedRegistryBuilder(UnifiedRegistryConfig.Default);
        var registry = builder.Build(
            "reg-001", "v1",
            Array.Empty<FamilyClusterRecord>(),
            null, null, TestProvenance());

        Assert.Equal(0, registry.Count);
    }

    [Fact]
    public void Build_FermionClusters_CreatesCorrectFermionRecords()
    {
        var builder = new UnifiedRegistryBuilder(UnifiedRegistryConfig.Default);
        var clusters = new[]
        {
            MakeCluster("c0", 1.0, "left"),
            MakeCluster("c1", 2.0, "right"),
        };

        var registry = builder.Build(
            "reg-002", "v1",
            clusters, null, null, TestProvenance());

        var fermions = registry.QueryByType(UnifiedParticleType.Fermion);
        Assert.Equal(2, fermions.Count);
        Assert.All(fermions, r => Assert.Equal(UnifiedParticleType.Fermion, r.ParticleType));
    }

    [Fact]
    public void Build_FermionRecord_HasCorrectFields()
    {
        var builder = new UnifiedRegistryBuilder(UnifiedRegistryConfig.Default);
        var cluster = MakeCluster("c0", 1.5, "left", branchPersistence: 0.9);

        var registry = builder.Build(
            "reg-003", "v1",
            new[] { cluster }, null, null, TestProvenance());

        var record = registry.Candidates[0];
        Assert.Equal(UnifiedParticleType.Fermion, record.ParticleType);
        Assert.Equal("c0", record.PrimarySourceId);
        Assert.Equal("left", record.Chirality);
        Assert.Equal(3, record.MassLikeEnvelope.Length);
        Assert.Equal(0.9, record.BranchStabilityScore, precision: 10);
        Assert.Equal("v1", record.RegistryVersion);
    }

    [Fact]
    public void Build_LowPersistenceCluster_DemotedToC2()
    {
        var config = new UnifiedRegistryConfig { StabilityThreshold = 0.5 };
        var builder = new UnifiedRegistryBuilder(config);
        var cluster = MakeCluster("c0", 1.0, "left", branchPersistence: 0.3);

        var registry = builder.Build(
            "reg-004", "v1",
            new[] { cluster }, null, null, TestProvenance());

        var record = registry.Candidates[0];
        Assert.Equal("C2_BranchStableCandidate", record.ClaimClass);
        Assert.NotEmpty(record.Demotions);
        Assert.Contains(record.Demotions, d => d.Reason == "LowPersistence");
    }

    [Fact]
    public void Build_HighPersistenceCluster_GetsC3()
    {
        var builder = new UnifiedRegistryBuilder(UnifiedRegistryConfig.Default);
        var cluster = MakeCluster("c0", 1.0, "left", branchPersistence: 0.9);

        var registry = builder.Build(
            "reg-005", "v1",
            new[] { cluster }, null, null, TestProvenance());

        var record = registry.Candidates[0];
        Assert.Equal("C3_ObservedStableCandidate", record.ClaimClass);
    }

    [Fact]
    public void Build_BosonCandidates_CreatesBosonRecords()
    {
        var builder = new UnifiedRegistryBuilder(UnifiedRegistryConfig.Default);
        var bosons = new[]
        {
            MakeBoson("b0", 1.0, BosonClaimClass.C2_BranchStableBosonicCandidate),
            MakeBoson("b1", 2.0, BosonClaimClass.C3_ObservedStableCandidate),
        };

        var registry = builder.Build(
            "reg-006", "v1",
            Array.Empty<FamilyClusterRecord>(),
            bosons, null, TestProvenance());

        var bosonRecords = registry.QueryByType(UnifiedParticleType.Boson);
        Assert.Equal(2, bosonRecords.Count);
        Assert.All(bosonRecords, r => Assert.Null(r.Chirality));
    }

    [Fact]
    public void Build_BosonWithUnverifiedGpu_DemotedToC1()
    {
        var builder = new UnifiedRegistryBuilder(UnifiedRegistryConfig.Default);
        var boson = MakeBoson("b0", 1.0, BosonClaimClass.C3_ObservedStableCandidate, unverifiedGpu: true);

        var registry = builder.Build(
            "reg-007", "v1",
            Array.Empty<FamilyClusterRecord>(),
            new[] { boson }, null, TestProvenance());

        var record = registry.Candidates[0];
        Assert.Equal("C1_LocalPersistentMode", record.ClaimClass);
        Assert.Contains(record.Demotions, d => d.Reason == "UnverifiedGpu");
    }

    [Fact]
    public void Build_BosonWithC0_NotDemoted()
    {
        var builder = new UnifiedRegistryBuilder(UnifiedRegistryConfig.Default);
        var boson = MakeBoson("b0", 1.0, BosonClaimClass.C0_NumericalMode, unverifiedGpu: true);

        var registry = builder.Build(
            "reg-008", "v1",
            Array.Empty<FamilyClusterRecord>(),
            new[] { boson }, null, TestProvenance());

        var record = registry.Candidates[0];
        Assert.Equal("C0_NumericalMode", record.ClaimClass);
        Assert.Empty(record.Demotions);
    }

    [Fact]
    public void Build_CouplingAtlas_CreatesInteractionRecords()
    {
        var builder = new UnifiedRegistryBuilder(UnifiedRegistryConfig.Default);
        var atlas = new CouplingAtlas
        {
            AtlasId = "atlas-1",
            FermionBackgroundId = "bg-001",
            BosonRegistryVersion = "v1",
            NormalizationConvention = "raw",
            Couplings = new List<BosonFermionCouplingRecord>
            {
                new BosonFermionCouplingRecord
                {
                    CouplingId = "coup-1",
                    BosonModeId = "b-0",
                    FermionModeIdI = "f-0",
                    FermionModeIdJ = "f-1",
                    CouplingProxyReal = 0.5,
                    CouplingProxyMagnitude = 0.5,
                    NormalizationConvention = "raw",
                    Provenance = TestProvenance(),
                },
            },
            Provenance = TestProvenance(),
        };

        var registry = builder.Build(
            "reg-009", "v1",
            Array.Empty<FamilyClusterRecord>(),
            null, new[] { atlas }, TestProvenance());

        var interactions = registry.QueryByType(UnifiedParticleType.Interaction);
        Assert.Single(interactions);
        Assert.Equal("C0_NumericalMode", interactions[0].ClaimClass);
    }

    [Fact]
    public void Build_CouplingBelowThreshold_Excluded()
    {
        var config = new UnifiedRegistryConfig { MinCouplingMagnitude = 1.0 };
        var builder = new UnifiedRegistryBuilder(config);
        var atlas = new CouplingAtlas
        {
            AtlasId = "atlas-1",
            FermionBackgroundId = "bg-001",
            BosonRegistryVersion = "v1",
            NormalizationConvention = "raw",
            Couplings = new List<BosonFermionCouplingRecord>
            {
                new BosonFermionCouplingRecord
                {
                    CouplingId = "coup-small",
                    BosonModeId = "b-0",
                    FermionModeIdI = "f-0",
                    FermionModeIdJ = "f-0",
                    CouplingProxyReal = 0.01,
                    CouplingProxyMagnitude = 0.01,
                    NormalizationConvention = "raw",
                    Provenance = TestProvenance(),
                },
            },
            Provenance = TestProvenance(),
        };

        var registry = builder.Build(
            "reg-010", "v1",
            Array.Empty<FamilyClusterRecord>(),
            null, new[] { atlas }, TestProvenance());

        Assert.Empty(registry.QueryByType(UnifiedParticleType.Interaction));
    }

    [Fact]
    public void Build_MixedInput_CountsCorrect()
    {
        var builder = new UnifiedRegistryBuilder(UnifiedRegistryConfig.Default);
        var clusters = new[] { MakeCluster("c0", 1.0, "left") };
        var bosons = new[] { MakeBoson("b0", 2.0, BosonClaimClass.C2_BranchStableBosonicCandidate) };

        var registry = builder.Build(
            "reg-011", "v1",
            clusters, bosons, null, TestProvenance());

        Assert.Single(registry.QueryByType(UnifiedParticleType.Fermion));
        Assert.Single(registry.QueryByType(UnifiedParticleType.Boson));
        Assert.Equal(2, registry.Count);
    }

    [Fact]
    public void Registry_JsonRoundTrip()
    {
        var builder = new UnifiedRegistryBuilder(UnifiedRegistryConfig.Default);
        var cluster = MakeCluster("c0", 1.0, "left");

        var registry = builder.Build(
            "reg-012", "v1",
            new[] { cluster }, null, null, TestProvenance());

        var json = registry.ToJson();
        Assert.Contains("Fermion", json);

        var loaded = UnifiedParticleRegistry.FromJson(json);
        Assert.Equal(registry.Count, loaded.Count);
    }

    [Fact]
    public void Registry_SchemaVersion_IsSet()
    {
        var registry = new UnifiedParticleRegistry();
        Assert.Equal("1.0.0", registry.SchemaVersion);
    }

    [Fact]
    public void Registry_QueryByMinClaimClass_FiltersCorrectly()
    {
        var builder = new UnifiedRegistryBuilder(UnifiedRegistryConfig.Default);
        var clusters = new[]
        {
            MakeCluster("c0", 1.0, "left", branchPersistence: 0.9),  // -> C3
            MakeCluster("c1", 2.0, "right", branchPersistence: 0.3), // -> C2 (low persistence)
        };

        var registry = builder.Build(
            "reg-013", "v1",
            clusters, null, null, TestProvenance());

        var aboveC3 = registry.QueryByMinClaimClass("C3");
        var aboveC2 = registry.QueryByMinClaimClass("C2");

        Assert.Single(aboveC3);  // only the high-persistence one
        Assert.Equal(2, aboveC2.Count);  // both
    }

    [Fact]
    public void Registry_CountAboveClass_Correct()
    {
        var registry = new UnifiedParticleRegistry();
        registry.Register(new UnifiedParticleRecord
        {
            ParticleId = "p0",
            ParticleType = UnifiedParticleType.Fermion,
            PrimarySourceId = "s0",
            ContributingSourceIds = new List<string>(),
            BranchVariantSet = new List<string>(),
            BackgroundSet = new List<string>(),
            MassLikeEnvelope = new[] { 0.0, 1.0, 2.0 },
            ClaimClass = "C3",
            RegistryVersion = "v1",
            Provenance = TestProvenance(),
        });

        Assert.Equal(1, registry.CountAboveClass("C3"));
        Assert.Equal(1, registry.CountAboveClass("C2"));
        Assert.Equal(0, registry.CountAboveClass("C4"));
    }
}
