using Gu.Core;
using Gu.Phase3.Registry;
using Gu.Phase4.Couplings;
using Gu.Phase4.FamilyClustering;
using Gu.Phase4.Fermions;
using Gu.Phase4.Registry;

namespace Gu.Phase4.Registry.Tests;

/// <summary>
/// Tests for RegistryMergeEngine (M42) — verifies canonical claim class string format,
/// type-appropriate labels, and demotion rules.
/// </summary>
public sealed class RegistryMergeEngineTests
{
    private static ProvenanceMeta TestProvenance() => new()
    {
        CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
        CodeRevision = "test-m42-merge",
        Branch = new BranchRef { BranchId = "test-branch", SchemaVersion = "1.0.0" },
        Backend = "cpu-reference",
    };

    // -----------------------------------------------------------------------
    // TEST 1: Merge with all-null inputs produces empty registry
    // -----------------------------------------------------------------------

    [Fact]
    public void Build_AllNullInputs_ProducesEmptyRegistry()
    {
        var engine = new RegistryMergeEngine(RegistryMergeConfig.Default);
        var result = engine.Build(null, null, null, null, TestProvenance());

        Assert.Equal(0, result.Count);
    }

    // -----------------------------------------------------------------------
    // TEST 2: Fermionic cluster merge produces "C2_BranchStableCandidate" (not bosonic variant)
    // -----------------------------------------------------------------------

    [Fact]
    public void Build_HighPersistenceFermionCluster_ProducesCanonicalC2FermionicLabel()
    {
        var clusters = new[]
        {
            MakeFamilyCluster("cluster-0", "left", 1.0, meanPersistence: 0.9),
        };

        var engine = new RegistryMergeEngine(RegistryMergeConfig.Default);
        var result = engine.Build(null, clusters, null, null, TestProvenance());

        Assert.Single(result.Candidates);
        var record = result.Candidates[0];
        Assert.Equal(UnifiedParticleType.Fermion, record.ParticleType);
        // Must use canonical type-agnostic C2 label, not bosonic variant
        Assert.Equal("C2_BranchStableCandidate", record.ClaimClass);
    }

    // -----------------------------------------------------------------------
    // TEST 3: Bosonic candidate merge produces correct canonical claim class
    // -----------------------------------------------------------------------

    [Fact]
    public void Build_BosonicCandidate_ProducesCanonicalC2BosonicLabel()
    {
        var bosonReg = new BosonRegistry();
        bosonReg.Register(MakeBoson("b1", BosonClaimClass.C2_BranchStableBosonicCandidate));

        var engine = new RegistryMergeEngine(RegistryMergeConfig.Default);
        var result = engine.Build(bosonReg, null, null, null, TestProvenance());

        Assert.Single(result.Candidates);
        var record = result.Candidates[0];
        Assert.Equal(UnifiedParticleType.Boson, record.ParticleType);
        // Phase III BosonClaimClass.C2_BranchStableBosonicCandidate maps to canonical C2_BranchStableCandidate
        Assert.Equal("C2_BranchStableCandidate", record.ClaimClass);
    }

    // -----------------------------------------------------------------------
    // TEST 4: UnverifiedGpu demotion caps claim class at C1
    // -----------------------------------------------------------------------

    [Fact]
    public void Build_UnverifiedGpuBoson_CappedAtC1()
    {
        var bosonReg = new BosonRegistry();
        bosonReg.Register(MakeBoson("b1", BosonClaimClass.C3_ObservedStableCandidate,
            computedWithUnverifiedGpu: true));

        var engine = new RegistryMergeEngine(RegistryMergeConfig.Default);
        var result = engine.Build(bosonReg, null, null, null, TestProvenance());

        Assert.Single(result.Candidates);
        var record = result.Candidates[0];
        Assert.Equal("C1_LocalPersistentMode", record.ClaimClass);
        Assert.Contains(record.Demotions, d => d.Reason == "UnverifiedGpu");
        Assert.Contains(record.Demotions, d => d.ToClaimClass == "C1_LocalPersistentMode");
    }

    // -----------------------------------------------------------------------
    // TEST 5: LowPersistence demotion applied to fermionic cluster
    // -----------------------------------------------------------------------

    [Fact]
    public void Build_LowPersistenceFermionCluster_DemotedToC1WithLowPersistenceReason()
    {
        // meanPersistence=0.3 is below MinBranchPersistenceForC2=0.8 so starts at C1,
        // but also below MinBranchPersistenceThreshold=0.5, so a LowPersistence demotion fires
        var clusters = new[]
        {
            MakeFamilyCluster("cluster-0", "left", 1.0, meanPersistence: 0.3),
        };
        var config = new RegistryMergeConfig
        {
            MinBranchPersistenceForC2 = 0.8,
            MinBranchPersistenceThreshold = 0.5,
        };
        var engine = new RegistryMergeEngine(config);
        var result = engine.Build(null, clusters, null, null, TestProvenance());

        Assert.Single(result.Candidates);
        var record = result.Candidates[0];
        Assert.Equal(1, UnifiedParticleRegistry.ParseClaimClassLevel(record.ClaimClass));
        Assert.Equal("C1_LocalPersistentMode", record.ClaimClass);
    }

    // -----------------------------------------------------------------------
    // TEST 6: LowObservation demotion reduces C3+ boson to C2
    // -----------------------------------------------------------------------

    [Fact]
    public void Build_LowObservationBoson_DemotedFromC3ToC2()
    {
        var bosonReg = new BosonRegistry();
        // ObservationStabilityScore=0.1 is below MinObservationConfidence=0.5 default
        bosonReg.Register(MakeBoson("b1", BosonClaimClass.C3_ObservedStableCandidate,
            observationStabilityScore: 0.1));

        var engine = new RegistryMergeEngine(RegistryMergeConfig.Default);
        var result = engine.Build(bosonReg, null, null, null, TestProvenance());

        Assert.Single(result.Candidates);
        var record = result.Candidates[0];
        // Demoted from C3 to C2 due to low observation stability
        Assert.Equal(2, UnifiedParticleRegistry.ParseClaimClassLevel(record.ClaimClass));
        Assert.Equal("C2_BranchStableCandidate", record.ClaimClass);
        Assert.Contains(record.Demotions, d => d.Reason == "LowObservation");
    }

    // -----------------------------------------------------------------------
    // TEST 7: AmbiguousMatching demotion applied to boson with ambiguity notes
    // -----------------------------------------------------------------------

    [Fact]
    public void Build_AmbiguousBoson_DemotedByOneLevelWithAmbiguousMatchingReason()
    {
        var bosonReg = new BosonRegistry();
        // AmbiguityCount=1 + AmbiguityNotes.Count=1 → total=2 > AmbiguityCountThreshold=0
        bosonReg.Register(MakeBoson("b1", BosonClaimClass.C2_BranchStableBosonicCandidate,
            ambiguityCount: 1, ambiguityNotes: new[] { "Overlapping candidate found" }));

        var engine = new RegistryMergeEngine(RegistryMergeConfig.Default);
        var result = engine.Build(bosonReg, null, null, null, TestProvenance());

        Assert.Single(result.Candidates);
        var record = result.Candidates[0];
        // C2 demoted by one level to C1
        Assert.Equal(1, UnifiedParticleRegistry.ParseClaimClassLevel(record.ClaimClass));
        Assert.Equal("C1_LocalPersistentMode", record.ClaimClass);
        Assert.Contains(record.Demotions, d => d.Reason == "AmbiguousMatching");
    }

    // -----------------------------------------------------------------------
    // TEST 8: All output ClaimClass strings are in canonical full format (not bare "C0" etc.)
    // -----------------------------------------------------------------------

    [Fact]
    public void Build_MixedSources_AllClaimClassStringsAreCanonicalFullFormat()
    {
        var bosonReg = new BosonRegistry();
        bosonReg.Register(MakeBoson("b1", BosonClaimClass.C0_NumericalMode));
        bosonReg.Register(MakeBoson("b2", BosonClaimClass.C1_LocalPersistentMode));
        bosonReg.Register(MakeBoson("b3", BosonClaimClass.C2_BranchStableBosonicCandidate));
        bosonReg.Register(MakeBoson("b4", BosonClaimClass.C3_ObservedStableCandidate));

        var clusters = new[]
        {
            MakeFamilyCluster("cluster-0", "left", 1.0, meanPersistence: 0.9),
            MakeFamilyCluster("cluster-1", "right", 2.0, meanPersistence: 0.3),
        };

        var atlas = MakeCouplingAtlas(new[]
        {
            MakeCoupling("c1", branchStability: 0.7),
        });

        var engine = new RegistryMergeEngine(RegistryMergeConfig.Default);
        var result = engine.Build(bosonReg, clusters, null, atlas, TestProvenance());

        Assert.True(result.Count > 0);
        foreach (var record in result.Candidates)
        {
            // Must not be bare "C0", "C1", "C2", "C3", "C4", "C5"
            Assert.False(record.ClaimClass.Length == 2,
                $"Bare claim class '{record.ClaimClass}' found on particle '{record.ParticleId}'. Expected canonical full format like 'C2_BranchStableCandidate'.");
            // Must start with C followed by a digit
            Assert.True(record.ClaimClass.Length >= 2 && record.ClaimClass[0] == 'C' && char.IsDigit(record.ClaimClass[1]),
                $"Claim class '{record.ClaimClass}' does not start with C-digit prefix.");
            // Must include underscore separator (full canonical name)
            Assert.Contains("_", record.ClaimClass);
        }
    }

    // -----------------------------------------------------------------------
    // TEST 9: Fermionic family atlas fallback also uses canonical claim class
    // -----------------------------------------------------------------------

    [Fact]
    public void Build_FermionAtlasFallback_HighPersistence_CanonicalC2Label()
    {
        var atlas = MakeFermionAtlas(new[]
        {
            MakeFermionModeFamily("fam-0", "left", 1.0, branchPersistence: 0.9),
        });

        var engine = new RegistryMergeEngine(RegistryMergeConfig.Default);
        var result = engine.Build(null, null, atlas, null, TestProvenance());

        Assert.Single(result.Candidates);
        var record = result.Candidates[0];
        Assert.Equal(UnifiedParticleType.Fermion, record.ParticleType);
        Assert.Equal("C2_BranchStableCandidate", record.ClaimClass);
    }

    // -----------------------------------------------------------------------
    // TEST 10: Demotion ToClaimClass fields are also in canonical full format
    // -----------------------------------------------------------------------

    [Fact]
    public void Build_DemotionRecords_UseCanonicalFullFormatStrings()
    {
        var bosonReg = new BosonRegistry();
        bosonReg.Register(MakeBoson("b1", BosonClaimClass.C3_ObservedStableCandidate,
            computedWithUnverifiedGpu: true));

        var engine = new RegistryMergeEngine(RegistryMergeConfig.Default);
        var result = engine.Build(bosonReg, null, null, null, TestProvenance());

        Assert.Single(result.Candidates);
        var record = result.Candidates[0];
        Assert.NotEmpty(record.Demotions);

        foreach (var demotion in record.Demotions)
        {
            // FromClaimClass must be canonical
            Assert.Contains("_", demotion.FromClaimClass);
            // ToClaimClass must be canonical
            Assert.Contains("_", demotion.ToClaimClass);
        }
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private static CandidateBosonRecord MakeBoson(
        string id,
        BosonClaimClass claimClass,
        bool computedWithUnverifiedGpu = false,
        int ambiguityCount = 0,
        string[]? ambiguityNotes = null,
        double observationStabilityScore = 0.8)
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
            ObservationStabilityScore = observationStabilityScore,
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
            Provenance = TestProvenance(),
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
            Provenance = TestProvenance(),
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
            Provenance = TestProvenance(),
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
            Provenance = TestProvenance(),
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
            Provenance = TestProvenance(),
        };
    }
}
