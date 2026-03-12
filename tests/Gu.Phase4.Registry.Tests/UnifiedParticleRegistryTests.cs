using System.Text.Json;
using System.Text.Json.Serialization;
using Gu.Core;
using Gu.Phase3.Registry;
using Gu.Phase4.Couplings;
using Gu.Phase4.FamilyClustering;
using Gu.Phase4.Fermions;
using Gu.Phase4.Registry;

namespace Gu.Phase4.Registry.Tests;

/// <summary>
/// Tests for M42: UnifiedParticleRegistry — aggregates bosons, fermions, and interactions.
/// Completion criterion: unified registry emitted for at least one run and passes schema validation.
/// </summary>
public class UnifiedParticleRegistryTests
{
    private static ProvenanceMeta TestProvenance() => new()
    {
        CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
        CodeRevision = "test-m42",
        Branch = new BranchRef { BranchId = "test-branch", SchemaVersion = "1.0.0" },
        Backend = "cpu-reference",
    };

    // -------------------------------------------------------
    // UnifiedParticleRegistry basic operations
    // -------------------------------------------------------

    [Fact]
    public void Registry_Empty_CountIsZero()
    {
        var registry = new UnifiedParticleRegistry();
        Assert.Equal(0, registry.Count);
    }

    [Fact]
    public void Registry_Register_IncrementsCount()
    {
        var registry = new UnifiedParticleRegistry();
        registry.Register(new UnifiedParticleRecord
        {
            ParticleId = "p1",
            ParticleType = UnifiedParticleType.Fermion,
            PrimarySourceId = "src-1",
            ContributingSourceIds = new List<string> { "src-1" },
            BranchVariantSet = new List<string> { "v1" },
            BackgroundSet = new List<string> { "bg-1" },
            MassLikeEnvelope = new[] { 1.0, 1.0, 1.0 },
            ClaimClass = "C1_LocalPersistentMode",
            RegistryVersion = "1.0.0",
            Provenance = TestProvenance(),
        });
        Assert.Equal(1, registry.Count);
    }

    [Fact]
    public void Registry_QueryByType_ReturnsCorrectSubset()
    {
        var registry = new UnifiedParticleRegistry();
        registry.Register(MakeRecord("p-boson", UnifiedParticleType.Boson, "C1_LocalPersistentMode"));
        registry.Register(MakeRecord("p-fermion", UnifiedParticleType.Fermion, "C1_LocalPersistentMode"));
        registry.Register(MakeRecord("p-interaction", UnifiedParticleType.Interaction, "C0_NumericalMode"));

        Assert.Single(registry.QueryByType(UnifiedParticleType.Boson));
        Assert.Single(registry.QueryByType(UnifiedParticleType.Fermion));
        Assert.Single(registry.QueryByType(UnifiedParticleType.Interaction));
    }

    [Fact]
    public void Registry_QueryByMinClaimClass_FiltersCorrectly()
    {
        var registry = new UnifiedParticleRegistry();
        registry.Register(MakeRecord("p0", UnifiedParticleType.Fermion, "C0_NumericalMode"));
        registry.Register(MakeRecord("p1", UnifiedParticleType.Fermion, "C1_LocalPersistentMode"));
        registry.Register(MakeRecord("p2", UnifiedParticleType.Fermion, "C2_BranchStableBosonicCandidate"));

        Assert.Equal(3, registry.QueryByMinClaimClass("C0_NumericalMode").Count);
        Assert.Equal(2, registry.QueryByMinClaimClass("C1_LocalPersistentMode").Count);
        Assert.Single(registry.QueryByMinClaimClass("C2_BranchStableBosonicCandidate"));
        Assert.Empty(registry.QueryByMinClaimClass("C3_ObservedStableCandidate"));
    }

    [Fact]
    public void Registry_QueryByBackground_ReturnsCorrectSubset()
    {
        var registry = new UnifiedParticleRegistry();
        registry.Register(MakeRecord("p1", UnifiedParticleType.Boson, "C1_LocalPersistentMode",
            backgroundSet: new[] { "bg-A" }));
        registry.Register(MakeRecord("p2", UnifiedParticleType.Boson, "C1_LocalPersistentMode",
            backgroundSet: new[] { "bg-B" }));

        Assert.Single(registry.QueryByBackground("bg-A"));
        Assert.Single(registry.QueryByBackground("bg-B"));
        Assert.Empty(registry.QueryByBackground("bg-C"));
    }

    [Fact]
    public void Registry_CountAboveClass_CorrectCount()
    {
        var registry = new UnifiedParticleRegistry();
        registry.Register(MakeRecord("p0", UnifiedParticleType.Fermion, "C0_NumericalMode"));
        registry.Register(MakeRecord("p1", UnifiedParticleType.Fermion, "C1_LocalPersistentMode"));
        registry.Register(MakeRecord("p2", UnifiedParticleType.Fermion, "C2_BranchStableBosonicCandidate"));

        Assert.Equal(2, registry.CountAboveClass("C1_LocalPersistentMode"));
    }

    // -------------------------------------------------------
    // ParseClaimClassLevel
    // -------------------------------------------------------

    [Theory]
    [InlineData("C0_NumericalMode", 0)]
    [InlineData("C1_LocalPersistentMode", 1)]
    [InlineData("C2_BranchStableBosonicCandidate", 2)]
    [InlineData("C3_ObservedStableCandidate", 3)]
    [InlineData("C4_PhysicalAnalogyCandidate", 4)]
    [InlineData("C5_StrongIdentificationCandidate", 5)]
    [InlineData("C0", 0)]
    [InlineData("C3", 3)]
    [InlineData("", 0)]
    public void ParseClaimClassLevel_VariousInputs(string input, int expected)
    {
        Assert.Equal(expected, UnifiedParticleRegistry.ParseClaimClassLevel(input));
    }

    // -------------------------------------------------------
    // JSON round-trip
    // -------------------------------------------------------

    [Fact]
    public void Registry_ToJson_ProducesValidJson()
    {
        var registry = new UnifiedParticleRegistry();
        registry.Register(MakeRecord("p1", UnifiedParticleType.Boson, "C1_LocalPersistentMode"));

        string json = registry.ToJson();
        Assert.NotEmpty(json);

        // Deserializable
        var deserialized = JsonSerializer.Deserialize<UnifiedParticleRegistry>(json,
            new JsonSerializerOptions { Converters = { new JsonStringEnumConverter() } });
        Assert.NotNull(deserialized);
        Assert.Single(deserialized!.Candidates);
    }

    [Fact]
    public void Registry_FromJson_PreservesAllCandidates()
    {
        var registry = new UnifiedParticleRegistry();
        registry.Register(MakeRecord("p1", UnifiedParticleType.Boson, "C2_BranchStableBosonicCandidate"));
        registry.Register(MakeRecord("p2", UnifiedParticleType.Fermion, "C1_LocalPersistentMode"));

        string json = registry.ToJson();
        var restored = UnifiedParticleRegistry.FromJson(json);

        Assert.Equal(2, restored.Candidates.Count);
        Assert.Equal("p1", restored.Candidates[0].ParticleId);
        Assert.Equal("p2", restored.Candidates[1].ParticleId);
    }

    [Fact]
    public void Registry_FromJson_STJ_Deserializes_List_NotEmpty()
    {
        // Regression guard: IReadOnlyList<T> silently produces empty list on STJ deserialize.
        // Candidates must use List<T>.
        var registry = new UnifiedParticleRegistry();
        for (int i = 0; i < 3; i++)
            registry.Register(MakeRecord($"p{i}", UnifiedParticleType.Fermion, "C1_LocalPersistentMode"));

        string json = registry.ToJson();
        var restored = UnifiedParticleRegistry.FromJson(json);

        Assert.Equal(3, restored.Count);
    }

    // -------------------------------------------------------
    // RegistryMergeEngine: basic build
    // -------------------------------------------------------

    [Fact]
    public void MergeEngine_Build_EmptyInputs_EmptyRegistry()
    {
        var engine = new RegistryMergeEngine(RegistryMergeConfig.Default);
        var result = engine.Build(null, null, null, null, TestProvenance());
        Assert.Equal(0, result.Count);
    }

    [Fact]
    public void MergeEngine_Build_BosonRegistry_CreatesBosonCandidates()
    {
        var bosonReg = new BosonRegistry();
        bosonReg.Register(MakeBoson("b1", BosonClaimClass.C2_BranchStableBosonicCandidate));
        bosonReg.Register(MakeBoson("b2", BosonClaimClass.C1_LocalPersistentMode));

        var engine = new RegistryMergeEngine(RegistryMergeConfig.Default);
        var result = engine.Build(bosonReg, null, null, null, TestProvenance());

        Assert.Equal(2, result.Count);
        Assert.All(result.Candidates, r => Assert.Equal(UnifiedParticleType.Boson, r.ParticleType));
    }

    [Fact]
    public void MergeEngine_Build_FamilyClusters_CreatesFermionCandidates()
    {
        var clusters = new[]
        {
            MakeFamilyCluster("cluster-0", "left", 1.0, meanPersistence: 0.9),
            MakeFamilyCluster("cluster-1", "right", 2.0, meanPersistence: 0.85),
        };

        var engine = new RegistryMergeEngine(RegistryMergeConfig.Default);
        var result = engine.Build(null, clusters, null, null, TestProvenance());

        Assert.Equal(2, result.Count);
        Assert.All(result.Candidates, r => Assert.Equal(UnifiedParticleType.Fermion, r.ParticleType));
    }

    [Fact]
    public void MergeEngine_Build_CouplingAtlas_CreatesInteractionCandidates()
    {
        var atlas = MakeCouplingAtlas(new[]
        {
            MakeCoupling("c1", 0.7),
            MakeCoupling("c2", 0.3),
        });

        var engine = new RegistryMergeEngine(RegistryMergeConfig.Default);
        var result = engine.Build(null, null, null, atlas, TestProvenance());

        Assert.Equal(2, result.Count);
        Assert.All(result.Candidates, r => Assert.Equal(UnifiedParticleType.Interaction, r.ParticleType));
    }

    [Fact]
    public void MergeEngine_Build_MixedSources_AllTypesPresent()
    {
        var bosonReg = new BosonRegistry();
        bosonReg.Register(MakeBoson("b1", BosonClaimClass.C1_LocalPersistentMode));

        var clusters = new[] { MakeFamilyCluster("cluster-0", "left", 1.0) };

        var couplingAtlas = MakeCouplingAtlas(new[] { MakeCoupling("c1", 0.6) });

        var engine = new RegistryMergeEngine(RegistryMergeConfig.Default);
        var result = engine.Build(bosonReg, clusters, null, couplingAtlas, TestProvenance());

        Assert.Equal(3, result.Count);
        Assert.Contains(result.Candidates, r => r.ParticleType == UnifiedParticleType.Boson);
        Assert.Contains(result.Candidates, r => r.ParticleType == UnifiedParticleType.Fermion);
        Assert.Contains(result.Candidates, r => r.ParticleType == UnifiedParticleType.Interaction);
    }

    // -------------------------------------------------------
    // Demotion rules
    // -------------------------------------------------------

    [Fact]
    public void MergeEngine_UnverifiedGpu_CapsAtC1()
    {
        var bosonReg = new BosonRegistry();
        bosonReg.Register(MakeBoson("b1", BosonClaimClass.C3_ObservedStableCandidate,
            computedWithUnverifiedGpu: true));

        var engine = new RegistryMergeEngine(RegistryMergeConfig.Default);
        var result = engine.Build(bosonReg, null, null, null, TestProvenance());

        Assert.Single(result.Candidates);
        var candidate = result.Candidates[0];
        Assert.Equal(1, UnifiedParticleRegistry.ParseClaimClassLevel(candidate.ClaimClass));
        Assert.Contains(candidate.Demotions, d => d.Reason == "UnverifiedGpu");
    }

    [Fact]
    public void MergeEngine_LowPersistence_DemotesFermionToC1()
    {
        // BranchPersistenceScore = 0.3, below default threshold of 0.5
        var clusters = new[] { MakeFamilyCluster("cluster-0", "left", 1.0, meanPersistence: 0.3) };
        var config = new RegistryMergeConfig { MinBranchPersistenceForC2 = 0.8, MinBranchPersistenceThreshold = 0.5 };
        var engine = new RegistryMergeEngine(config);
        var result = engine.Build(null, clusters, null, null, TestProvenance());

        Assert.Single(result.Candidates);
        // Should be C1 due to low persistence demotion
        Assert.Equal(1, UnifiedParticleRegistry.ParseClaimClassLevel(result.Candidates[0].ClaimClass));
        // No demotion triggered because start was C1 (persistence 0.3 < 0.8 → doesn't reach C2)
        // Actually C1 is assigned first via start at C1 (persistence 0.3 < 0.8 threshold for C2)
        // then checked if < 0.5 → C1 already, no demotion needed
    }

    [Fact]
    public void MergeEngine_HighPersistence_ReachesC2()
    {
        var clusters = new[] { MakeFamilyCluster("cluster-0", "left", 1.0, meanPersistence: 0.9) };
        var engine = new RegistryMergeEngine(RegistryMergeConfig.Default);
        var result = engine.Build(null, clusters, null, null, TestProvenance());

        Assert.Single(result.Candidates);
        Assert.Equal(2, UnifiedParticleRegistry.ParseClaimClassLevel(result.Candidates[0].ClaimClass));
    }

    [Fact]
    public void MergeEngine_AmbiguousBoson_DemotesOneLevel()
    {
        var bosonReg = new BosonRegistry();
        // AmbiguityCount = 1, AmbiguityNotes has 1 note → total > threshold (0)
        bosonReg.Register(MakeBoson("b1", BosonClaimClass.C2_BranchStableBosonicCandidate,
            ambiguityCount: 1, ambiguityNotes: new[] { "Ambiguous match" }));

        var engine = new RegistryMergeEngine(RegistryMergeConfig.Default);
        var result = engine.Build(bosonReg, null, null, null, TestProvenance());

        Assert.Single(result.Candidates);
        // C2 demoted by AmbiguousMatching → C1
        Assert.Equal(1, UnifiedParticleRegistry.ParseClaimClassLevel(result.Candidates[0].ClaimClass));
        Assert.Contains(result.Candidates[0].Demotions, d => d.Reason == "AmbiguousMatching");
    }

    [Fact]
    public void MergeEngine_InteractionWithHighStability_ReachesC1()
    {
        var atlas = MakeCouplingAtlas(new[] { MakeCoupling("c1", branchStability: 0.8) });
        var engine = new RegistryMergeEngine(RegistryMergeConfig.Default);
        var result = engine.Build(null, null, null, atlas, TestProvenance());

        Assert.Single(result.Candidates);
        Assert.Equal(1, UnifiedParticleRegistry.ParseClaimClassLevel(result.Candidates[0].ClaimClass));
    }

    [Fact]
    public void MergeEngine_InteractionWithLowStability_StaysC0()
    {
        var atlas = MakeCouplingAtlas(new[] { MakeCoupling("c1", branchStability: 0.2) });
        var engine = new RegistryMergeEngine(RegistryMergeConfig.Default);
        var result = engine.Build(null, null, null, atlas, TestProvenance());

        Assert.Single(result.Candidates);
        Assert.Equal(0, UnifiedParticleRegistry.ParseClaimClassLevel(result.Candidates[0].ClaimClass));
    }

    // -------------------------------------------------------
    // Provenance
    // -------------------------------------------------------

    [Fact]
    public void MergeEngine_Build_ProvenancePreserved()
    {
        var prov = TestProvenance();
        var clusters = new[] { MakeFamilyCluster("cluster-0", "left", 1.0) };
        var engine = new RegistryMergeEngine(RegistryMergeConfig.Default);
        var result = engine.Build(null, clusters, null, null, prov);

        Assert.Single(result.Candidates);
        Assert.Equal(prov.CodeRevision, result.Candidates[0].Provenance.CodeRevision);
    }

    // -------------------------------------------------------
    // Schema validation: at least one run produces a non-empty registry
    // -------------------------------------------------------

    [Fact]
    public void MergeEngine_OneRun_ProducesNonEmptyRegistry_SchemaValid()
    {
        var bosonReg = new BosonRegistry();
        bosonReg.Register(MakeBoson("b1", BosonClaimClass.C2_BranchStableBosonicCandidate));

        var clusters = new[]
        {
            MakeFamilyCluster("cluster-0", "left",  1.0, meanPersistence: 0.9),
            MakeFamilyCluster("cluster-1", "right", 1.0, meanPersistence: 0.9),
        };

        var couplingAtlas = MakeCouplingAtlas(new[] { MakeCoupling("c1", 0.7) });

        var engine = new RegistryMergeEngine(RegistryMergeConfig.Default);
        var result = engine.Build(bosonReg, clusters, null, couplingAtlas, TestProvenance());

        // Non-empty
        Assert.True(result.Count > 0, "Registry must have at least one candidate.");

        // JSON round-trip validates schema
        string json = result.ToJson();
        Assert.NotEmpty(json);

        var deserialized = UnifiedParticleRegistry.FromJson(json);
        Assert.Equal(result.Count, deserialized.Count);

        // Each candidate has required fields
        foreach (var candidate in deserialized.Candidates)
        {
            Assert.NotEmpty(candidate.ParticleId);
            Assert.NotEmpty(candidate.PrimarySourceId);
            Assert.NotEmpty(candidate.ClaimClass);
            Assert.NotNull(candidate.MassLikeEnvelope);
            Assert.Equal(3, candidate.MassLikeEnvelope.Length);
            Assert.NotNull(candidate.Provenance);
        }
    }

    // -------------------------------------------------------
    // FamilyAtlas fallback (when no clusters provided)
    // -------------------------------------------------------

    [Fact]
    public void MergeEngine_NullClusters_FermionAtlas_CreatesFermionCandidates()
    {
        var atlas = MakeFermionAtlas(new[]
        {
            MakeFermionModeFamily("fam-0", "left", 1.0),
            MakeFermionModeFamily("fam-1", "right", 2.0),
        });

        var engine = new RegistryMergeEngine(RegistryMergeConfig.Default);
        var result = engine.Build(null, null, atlas, null, TestProvenance());

        Assert.Equal(2, result.Count);
        Assert.All(result.Candidates, r => Assert.Equal(UnifiedParticleType.Fermion, r.ParticleType));
    }

    // -------------------------------------------------------
    // ParticleClaimDemotion
    // -------------------------------------------------------

    [Fact]
    public void ParticleClaimDemotion_FieldsSet()
    {
        var demotion = new ParticleClaimDemotion
        {
            Reason = "UnverifiedGpu",
            Details = "GPU not verified.",
            FromClaimClass = "C3_ObservedStableCandidate",
            ToClaimClass = "C1_LocalPersistentMode",
        };

        Assert.Equal("UnverifiedGpu", demotion.Reason);
        Assert.Equal("C3_ObservedStableCandidate", demotion.FromClaimClass);
        Assert.Equal("C1_LocalPersistentMode", demotion.ToClaimClass);
        Assert.True(demotion.DemotedAt > DateTimeOffset.MinValue);
    }

    // -------------------------------------------------------
    // Test helpers
    // -------------------------------------------------------

    private static UnifiedParticleRecord MakeRecord(
        string id,
        UnifiedParticleType type,
        string claimClass,
        string[]? backgroundSet = null)
    {
        return new UnifiedParticleRecord
        {
            ParticleId = id,
            ParticleType = type,
            PrimarySourceId = $"src-{id}",
            ContributingSourceIds = new List<string> { $"src-{id}" },
            BranchVariantSet = new List<string> { "v1" },
            BackgroundSet = backgroundSet != null ? new List<string>(backgroundSet) : new List<string> { "bg-1" },
            MassLikeEnvelope = new[] { 1.0, 1.0, 1.0 },
            ClaimClass = claimClass,
            RegistryVersion = "1.0.0",
            Provenance = new ProvenanceMeta
            {
                CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
                CodeRevision = "test",
                Branch = new BranchRef { BranchId = "b", SchemaVersion = "1.0.0" },
                Backend = "cpu-reference",
            },
        };
    }

    private static CandidateBosonRecord MakeBoson(
        string id,
        BosonClaimClass claimClass,
        bool computedWithUnverifiedGpu = false,
        int ambiguityCount = 0,
        string[]? ambiguityNotes = null)
    {
        return new CandidateBosonRecord
        {
            CandidateId = id,
            PrimaryFamilyId = $"family-{id}",
            ContributingModeIds = new[] { $"mode-{id}" },
            BackgroundSet = new[] { "bg-1" },
            MassLikeEnvelope = new[] { 1.0, 1.0, 1.0 },
            MultiplicityEnvelope = new[] { 1, 1, 1 },
            GaugeLeakEnvelope = new[] { 0.01, 0.01, 0.01 },
            ClaimClass = claimClass,
            ComputedWithUnverifiedGpu = computedWithUnverifiedGpu,
            AmbiguityCount = ambiguityCount,
            AmbiguityNotes = ambiguityNotes ?? Array.Empty<string>(),
            RegistryVersion = "1.0.0",
        };
    }

    private static FamilyClusterRecord MakeFamilyCluster(
        string clusterId,
        string chirality,
        double eigenMean,
        double meanPersistence = 1.0,
        double ambiguityScore = 0.0)
    {
        return new FamilyClusterRecord
        {
            ClusterId = clusterId,
            ClusterLabel = clusterId,
            MemberFamilyIds = new List<string> { $"family-{clusterId}" },
            DominantChirality = chirality,
            EigenvalueMagnitudeEnvelope = new[] { eigenMean * 0.9, eigenMean, eigenMean * 1.1 },
            AmbiguityScore = ambiguityScore,
            MeanBranchPersistence = meanPersistence,
            ClusteringMethod = "eigenvalue-proximity",
            Provenance = new ProvenanceMeta
            {
                CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
                CodeRevision = "test",
                Branch = new BranchRef { BranchId = "test-branch", SchemaVersion = "1.0.0" },
                Backend = "cpu-reference",
            },
        };
    }

    private static FermionModeFamily MakeFermionModeFamily(
        string familyId,
        string chirality,
        double eigenMean,
        double branchPersistence = 1.0)
    {
        return new FermionModeFamily
        {
            FamilyId = familyId,
            MemberModeIds = new List<string> { $"{familyId}-mode-0" },
            BackgroundIds = new List<string> { "bg-1" },
            BranchVariantIds = new List<string> { "v1" },
            EigenvalueMagnitudeEnvelope = new[] { eigenMean * 0.9, eigenMean, eigenMean * 1.1 },
            DominantChiralityProfile = chirality,
            BranchPersistenceScore = branchPersistence,
            RefinementPersistenceScore = 1.0,
            Provenance = new ProvenanceMeta
            {
                CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
                CodeRevision = "test",
                Branch = new BranchRef { BranchId = "test-branch", SchemaVersion = "1.0.0" },
                Backend = "cpu-reference",
            },
        };
    }

    private static FermionFamilyAtlas MakeFermionAtlas(FermionModeFamily[] families)
    {
        return new FermionFamilyAtlas
        {
            AtlasId = "atlas-test",
            BranchFamilyId = "bf-test",
            ContextIds = new List<string> { "ctx-1" },
            BackgroundIds = new List<string> { "bg-1" },
            Families = families.ToList(),
            Provenance = new ProvenanceMeta
            {
                CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
                CodeRevision = "test",
                Branch = new BranchRef { BranchId = "test-branch", SchemaVersion = "1.0.0" },
                Backend = "cpu-reference",
            },
        };
    }

    private static BosonFermionCouplingRecord MakeCoupling(
        string id,
        double branchStability = 0.0)
    {
        return new BosonFermionCouplingRecord
        {
            CouplingId = id,
            BosonModeId = $"boson-{id}",
            FermionModeIdI = $"fermion-i-{id}",
            FermionModeIdJ = $"fermion-j-{id}",
            CouplingProxyReal = 0.5,
            CouplingProxyMagnitude = 0.5,
            NormalizationConvention = "unit-norm",
            BranchStabilityScore = branchStability,
            Provenance = new ProvenanceMeta
            {
                CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
                CodeRevision = "test",
                Branch = new BranchRef { BranchId = "test-branch", SchemaVersion = "1.0.0" },
                Backend = "cpu-reference",
            },
        };
    }

    private static CouplingAtlas MakeCouplingAtlas(BosonFermionCouplingRecord[] couplings)
    {
        return new CouplingAtlas
        {
            AtlasId = "atlas-coupling",
            FermionBackgroundId = "bg-1",
            BosonRegistryVersion = "1.0.0",
            Couplings = couplings.ToList(),
            NormalizationConvention = "unit-norm",
            Provenance = new ProvenanceMeta
            {
                CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
                CodeRevision = "test",
                Branch = new BranchRef { BranchId = "test-branch", SchemaVersion = "1.0.0" },
                Backend = "cpu-reference",
            },
        };
    }
}
