using Gu.Phase3.ModeTracking;
using Gu.Phase3.Properties;
using Gu.Phase3.Registry;
using Gu.Phase3.Spectra;
using System.Text.Json;

namespace Gu.Phase3.Registry.Tests;

public class BosonRegistryTests
{
    private static ModeFamilyRecord MakeFamily(
        string familyId, string[] modeIds, string[] contextIds,
        double meanEig = 1.0, bool stable = true, int ambiguityCount = 0)
    {
        return new ModeFamilyRecord
        {
            FamilyId = familyId,
            MemberModeIds = modeIds,
            ContextIds = contextIds,
            MeanEigenvalue = meanEig,
            EigenvalueSpread = 0.1,
            IsStable = stable,
            AmbiguityCount = ambiguityCount,
            Alignments = Array.Empty<ModeAlignmentRecord>(),
        };
    }

    private static SpectrumBundle MakeSpectrum(string bgId, (string modeId, double eigenvalue, double leak)[] modes)
    {
        var modeRecords = modes.Select((m, i) => new ModeRecord
        {
            ModeId = m.modeId,
            BackgroundId = bgId,
            OperatorType = SpectralOperatorType.GaussNewton,
            Eigenvalue = m.eigenvalue,
            ResidualNorm = 0,
            NormalizationConvention = "unit-M-norm",
            GaugeLeakScore = m.leak,
            ModeVector = new double[] { 1 },
            ModeIndex = i,
        }).ToArray();

        return new SpectrumBundle
        {
            SpectrumId = $"spectrum-{bgId}",
            BackgroundId = bgId,
            OperatorBundleId = $"bundle-{bgId}",
            OperatorType = SpectralOperatorType.GaussNewton,
            Formulation = PhysicalModeFormulation.PenaltyFixed,
            SolverMethod = "explicit-dense",
            StateDimension = 1,
            Modes = modeRecords,
            Clusters = Array.Empty<ModeCluster>(),
            ConvergenceStatus = "converged",
            IterationsUsed = 0,
            MaxOrthogonalityDefect = 0,
        };
    }

    [Fact]
    public void BuildFromFamily_ProducesCandidate()
    {
        var family = MakeFamily("f-1", new[] { "m-0", "m-1" }, new[] { "bg-1", "bg-2" });
        var spectra = new Dictionary<string, SpectrumBundle>
        {
            ["bg-1"] = MakeSpectrum("bg-1", new[] { ("m-0", 1.0, 0.01) }),
            ["bg-2"] = MakeSpectrum("bg-2", new[] { ("m-1", 1.1, 0.02) }),
        };

        var builder = new CandidateBosonBuilder();
        var candidate = builder.BuildFromFamily(family, spectra);

        Assert.NotNull(candidate);
        Assert.Equal("f-1", candidate.PrimaryFamilyId);
        Assert.Equal(2, candidate.ContributingModeIds.Count);
        Assert.Equal(2, candidate.BackgroundSet.Count);
        Assert.True(candidate.ClaimClass >= BosonClaimClass.C1_LocalPersistentMode);
    }

    [Fact]
    public void BuildFromFamily_SingleContext_GetsC0()
    {
        var family = MakeFamily("f-single", new[] { "m-0" }, new[] { "bg-1" });
        var spectra = new Dictionary<string, SpectrumBundle>
        {
            ["bg-1"] = MakeSpectrum("bg-1", new[] { ("m-0", 1.0, 0.01) }),
        };

        var builder = new CandidateBosonBuilder();
        var candidate = builder.BuildFromFamily(family, spectra);

        Assert.Equal(BosonClaimClass.C0_NumericalMode, candidate.ClaimClass);
    }

    [Fact]
    public void DemotionEngine_HighGaugeLeak_DemotesToC0()
    {
        var candidate = new CandidateBosonRecord
        {
            CandidateId = "c-1",
            PrimaryFamilyId = "f-1",
            ContributingModeIds = new[] { "m-0", "m-1" },
            BackgroundSet = new[] { "bg-1" },
            MassLikeEnvelope = new[] { 1.0, 1.0, 1.0 },
            MultiplicityEnvelope = new[] { 1, 1, 1 },
            GaugeLeakEnvelope = new[] { 0.5, 0.8, 0.9 }, // High gauge leak
            ClaimClass = BosonClaimClass.C2_BranchStableBosonicCandidate,
            RegistryVersion = "1.0.0",
        };

        var engine = new DemotionEngine(new DemotionConfig { GaugeLeakThreshold = 0.3 });
        var demoted = engine.ApplyDemotions(candidate);

        Assert.Equal(BosonClaimClass.C0_NumericalMode, demoted.ClaimClass);
        Assert.True(demoted.Demotions.Count > 0);
        Assert.Contains(demoted.Demotions, d => d.Reason == DemotionReason.GaugeLeak);
    }

    [Fact]
    public void DemotionEngine_BranchFragility_DemotesByOne()
    {
        var candidate = new CandidateBosonRecord
        {
            CandidateId = "c-2",
            PrimaryFamilyId = "f-2",
            ContributingModeIds = new[] { "m-0" },
            BackgroundSet = new[] { "bg-1" },
            MassLikeEnvelope = new[] { 1.0, 1.0, 1.0 },
            MultiplicityEnvelope = new[] { 1, 1, 1 },
            GaugeLeakEnvelope = new[] { 0.01, 0.02, 0.03 }, // Low leak
            BranchStabilityScore = 0.2, // Poor stability
            ClaimClass = BosonClaimClass.C2_BranchStableBosonicCandidate,
            RegistryVersion = "1.0.0",
        };

        var engine = new DemotionEngine(new DemotionConfig { BranchStabilityThreshold = 0.5 });
        var demoted = engine.ApplyDemotions(candidate);

        Assert.True(demoted.ClaimClass < BosonClaimClass.C2_BranchStableBosonicCandidate);
        Assert.Contains(demoted.Demotions, d => d.Reason == DemotionReason.BranchFragility);
    }

    [Fact]
    public void DemotionEngine_BackendFragility_CapsAtC1()
    {
        var candidate = new CandidateBosonRecord
        {
            CandidateId = "c-3",
            PrimaryFamilyId = "f-3",
            ContributingModeIds = new[] { "m-0" },
            BackgroundSet = new[] { "bg-1" },
            MassLikeEnvelope = new[] { 1.0, 1.0, 1.0 },
            MultiplicityEnvelope = new[] { 1, 1, 1 },
            GaugeLeakEnvelope = new[] { 0.01, 0.01, 0.01 },
            BranchStabilityScore = 0.9,
            RefinementStabilityScore = 0.9,
            BackendStabilityScore = 0.3, // Poor backend stability
            ClaimClass = BosonClaimClass.C3_ObservedStableCandidate,
            RegistryVersion = "1.0.0",
        };

        var engine = new DemotionEngine(new DemotionConfig { BackendStabilityThreshold = 0.8 });
        var demoted = engine.ApplyDemotions(candidate);

        Assert.True(demoted.ClaimClass <= BosonClaimClass.C1_LocalPersistentMode);
        Assert.Contains(demoted.Demotions, d => d.Reason == DemotionReason.BackendFragility);
    }

    [Fact]
    public void DemotionEngine_NoIssues_NoDemotion()
    {
        var candidate = new CandidateBosonRecord
        {
            CandidateId = "c-4",
            PrimaryFamilyId = "f-4",
            ContributingModeIds = new[] { "m-0" },
            BackgroundSet = new[] { "bg-1" },
            MassLikeEnvelope = new[] { 1.0, 1.0, 1.0 },
            MultiplicityEnvelope = new[] { 1, 1, 1 },
            GaugeLeakEnvelope = new[] { 0.01, 0.01, 0.01 },
            BranchStabilityScore = 0.9,
            RefinementStabilityScore = 0.9,
            BackendStabilityScore = 0.95,
            ObservationStabilityScore = 0.9,
            ClaimClass = BosonClaimClass.C2_BranchStableBosonicCandidate,
            RegistryVersion = "1.0.0",
        };

        var engine = new DemotionEngine();
        var result = engine.ApplyDemotions(candidate);

        Assert.Equal(BosonClaimClass.C2_BranchStableBosonicCandidate, result.ClaimClass);
        Assert.Empty(result.Demotions);
    }

    [Fact]
    public void BosonRegistry_RegisterAndQuery()
    {
        var registry = new BosonRegistry();

        var c1 = new CandidateBosonRecord
        {
            CandidateId = "c-1",
            PrimaryFamilyId = "f-1",
            ContributingModeIds = new[] { "m-0" },
            BackgroundSet = new[] { "bg-1" },
            MassLikeEnvelope = new[] { 1.0, 1.0, 1.0 },
            MultiplicityEnvelope = new[] { 1, 1, 1 },
            GaugeLeakEnvelope = new[] { 0.01, 0.01, 0.01 },
            ClaimClass = BosonClaimClass.C2_BranchStableBosonicCandidate,
            RegistryVersion = "1.0.0",
        };

        var c2 = new CandidateBosonRecord
        {
            CandidateId = "c-2",
            PrimaryFamilyId = "f-2",
            ContributingModeIds = new[] { "m-1" },
            BackgroundSet = new[] { "bg-1", "bg-2" },
            MassLikeEnvelope = new[] { 5.0, 5.0, 5.0 },
            MultiplicityEnvelope = new[] { 2, 2, 2 },
            GaugeLeakEnvelope = new[] { 0.01, 0.01, 0.01 },
            ClaimClass = BosonClaimClass.C0_NumericalMode,
            RegistryVersion = "1.0.0",
        };

        registry.Register(c1);
        registry.Register(c2);

        Assert.Equal(2, registry.Count);
        Assert.Equal(1, registry.CountAboveClass(BosonClaimClass.C2_BranchStableBosonicCandidate));

        var byBg = registry.QueryByBackground("bg-2");
        Assert.Single(byBg);
        Assert.Equal("c-2", byBg[0].CandidateId);

        var byFamily = registry.QueryByFamily("f-1");
        Assert.NotNull(byFamily);
        Assert.Equal("c-1", byFamily!.CandidateId);
    }

    [Fact]
    public void BosonRegistry_SerializesCleanly()
    {
        var registry = new BosonRegistry();
        registry.Register(new CandidateBosonRecord
        {
            CandidateId = "c-1",
            PrimaryFamilyId = "f-1",
            ContributingModeIds = new[] { "m-0" },
            BackgroundSet = new[] { "bg-1" },
            MassLikeEnvelope = new[] { 1.0, 1.0, 1.0 },
            MultiplicityEnvelope = new[] { 1, 1, 1 },
            GaugeLeakEnvelope = new[] { 0.01, 0.01, 0.01 },
            ClaimClass = BosonClaimClass.C1_LocalPersistentMode,
            RegistryVersion = "1.0.0",
        });

        var json = registry.ToJson();
        Assert.NotEmpty(json);
        Assert.Contains("candidates", json);
        Assert.Contains("candidateId", json);
        Assert.Contains("claimClass", json);
    }

    [Fact]
    public void AmbiguousCandidates_RepresentedExplicitly()
    {
        var family = MakeFamily("f-ambig",
            new[] { "m-0", "m-1" }, new[] { "bg-1", "bg-2" },
            stable: false, ambiguityCount: 2);

        var spectra = new Dictionary<string, SpectrumBundle>
        {
            ["bg-1"] = MakeSpectrum("bg-1", new[] { ("m-0", 1.0, 0.01) }),
            ["bg-2"] = MakeSpectrum("bg-2", new[] { ("m-1", 1.1, 0.02) }),
        };

        var builder = new CandidateBosonBuilder();
        var candidate = builder.BuildFromFamily(family, spectra);

        Assert.True(candidate.AmbiguityNotes.Count > 0, "Ambiguous candidates must have notes");
        Assert.Contains(candidate.AmbiguityNotes, n => n.Contains("ambiguous"));
    }

    [Theory]
    [InlineData(BosonClaimClass.C0_NumericalMode, 0)]
    [InlineData(BosonClaimClass.C1_LocalPersistentMode, 1)]
    [InlineData(BosonClaimClass.C2_BranchStableBosonicCandidate, 2)]
    [InlineData(BosonClaimClass.C3_ObservedStableCandidate, 3)]
    [InlineData(BosonClaimClass.C4_PhysicalAnalogyCandidate, 4)]
    [InlineData(BosonClaimClass.C5_StrongIdentificationCandidate, 5)]
    public void BosonClaimClass_EnumValues_ExistC0ThroughC5(BosonClaimClass cls, int expected)
    {
        Assert.Equal(expected, (int)cls);
    }

    [Fact]
    public void CandidateBosonRecord_Construction_AllFieldsAccessible()
    {
        var record = new CandidateBosonRecord
        {
            CandidateId = "c-test",
            PrimaryFamilyId = "f-test",
            ContributingModeIds = new[] { "m-0", "m-1" },
            BackgroundSet = new[] { "bg-1" },
            BranchVariantSet = new[] { "bv-1", "bv-2" },
            MassLikeEnvelope = new[] { 0.5, 1.0, 1.5 },
            MultiplicityEnvelope = new[] { 1, 2, 3 },
            GaugeLeakEnvelope = new[] { 0.01, 0.02, 0.03 },
            BranchStabilityScore = 0.95,
            RefinementStabilityScore = 0.9,
            BackendStabilityScore = 0.85,
            ObservationStabilityScore = 0.8,
            ClaimClass = BosonClaimClass.C3_ObservedStableCandidate,
            AmbiguityNotes = new[] { "note-1" },
            RegistryVersion = "2.0.0",
        };

        Assert.Equal("c-test", record.CandidateId);
        Assert.Equal("f-test", record.PrimaryFamilyId);
        Assert.Equal(2, record.ContributingModeIds.Count);
        Assert.Equal(2, record.BranchVariantSet.Count);
        Assert.Equal(new[] { 0.5, 1.0, 1.5 }, record.MassLikeEnvelope);
        Assert.Equal(new[] { 1, 2, 3 }, record.MultiplicityEnvelope);
        Assert.Equal(0.95, record.BranchStabilityScore);
        Assert.Equal(0.8, record.ObservationStabilityScore);
        Assert.Single(record.AmbiguityNotes);
        Assert.Equal("2.0.0", record.RegistryVersion);
    }

    [Fact]
    public void CandidateBosonRecord_JsonRoundtrip()
    {
        var original = new CandidateBosonRecord
        {
            CandidateId = "c-rt",
            PrimaryFamilyId = "f-rt",
            ContributingModeIds = new[] { "m-0" },
            BackgroundSet = new[] { "bg-1" },
            MassLikeEnvelope = new[] { 1.0, 1.0, 1.0 },
            MultiplicityEnvelope = new[] { 1, 1, 1 },
            GaugeLeakEnvelope = new[] { 0.01, 0.01, 0.01 },
            BranchStabilityScore = 0.9,
            ClaimClass = BosonClaimClass.C2_BranchStableBosonicCandidate,
            RegistryVersion = "1.0.0",
        };

        var options = new JsonSerializerOptions
        {
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() },
        };
        var json = JsonSerializer.Serialize(original, options);
        var deserialized = JsonSerializer.Deserialize<CandidateBosonRecord>(json, options);

        Assert.NotNull(deserialized);
        Assert.Equal(original.CandidateId, deserialized!.CandidateId);
        Assert.Equal(original.PrimaryFamilyId, deserialized.PrimaryFamilyId);
        Assert.Equal(original.ClaimClass, deserialized.ClaimClass);
        Assert.Equal(original.BranchStabilityScore, deserialized.BranchStabilityScore);
        Assert.Equal(original.MassLikeEnvelope, deserialized.MassLikeEnvelope);
    }

    [Fact]
    public void BosonDemotionRecord_Construction()
    {
        var record = new BosonDemotionRecord
        {
            CandidateId = "c-dem",
            Reason = DemotionReason.RefinementFragility,
            PreviousClaimClass = BosonClaimClass.C3_ObservedStableCandidate,
            DemotedClaimClass = BosonClaimClass.C2_BranchStableBosonicCandidate,
            Details = "Refinement stability 0.3 < threshold 0.5",
            TriggerValue = 0.3,
            Threshold = 0.5,
        };

        Assert.Equal("c-dem", record.CandidateId);
        Assert.Equal(DemotionReason.RefinementFragility, record.Reason);
        Assert.Equal(BosonClaimClass.C3_ObservedStableCandidate, record.PreviousClaimClass);
        Assert.Equal(BosonClaimClass.C2_BranchStableBosonicCandidate, record.DemotedClaimClass);
        Assert.Equal(0.3, record.TriggerValue);
        Assert.Equal(0.5, record.Threshold);
    }

    [Fact]
    public void DemotionEngine_RefinementFragility_DemotesByOne()
    {
        var candidate = new CandidateBosonRecord
        {
            CandidateId = "c-ref",
            PrimaryFamilyId = "f-ref",
            ContributingModeIds = new[] { "m-0" },
            BackgroundSet = new[] { "bg-1" },
            MassLikeEnvelope = new[] { 1.0, 1.0, 1.0 },
            MultiplicityEnvelope = new[] { 1, 1, 1 },
            GaugeLeakEnvelope = new[] { 0.01, 0.01, 0.01 },
            BranchStabilityScore = 0.9,
            RefinementStabilityScore = 0.2, // Poor refinement stability
            BackendStabilityScore = 0.95,
            ObservationStabilityScore = 0.9,
            ClaimClass = BosonClaimClass.C2_BranchStableBosonicCandidate,
            RegistryVersion = "1.0.0",
        };

        var engine = new DemotionEngine(new DemotionConfig { RefinementStabilityThreshold = 0.5 });
        var demoted = engine.ApplyDemotions(candidate);

        Assert.Equal(BosonClaimClass.C1_LocalPersistentMode, demoted.ClaimClass);
        Assert.Contains(demoted.Demotions, d => d.Reason == DemotionReason.RefinementFragility);
    }

    [Fact]
    public void DemotionEngine_MultipleDemotions_Accumulate()
    {
        // Candidate with both high gauge leak AND poor branch stability
        var candidate = new CandidateBosonRecord
        {
            CandidateId = "c-multi",
            PrimaryFamilyId = "f-multi",
            ContributingModeIds = new[] { "m-0" },
            BackgroundSet = new[] { "bg-1" },
            MassLikeEnvelope = new[] { 1.0, 1.0, 1.0 },
            MultiplicityEnvelope = new[] { 1, 1, 1 },
            GaugeLeakEnvelope = new[] { 0.5, 0.8, 0.9 }, // High leak
            BranchStabilityScore = 0.2, // Poor branch stability
            RefinementStabilityScore = 0.2, // Poor refinement stability
            BackendStabilityScore = 0.3, // Poor backend stability
            ClaimClass = BosonClaimClass.C5_StrongIdentificationCandidate,
            RegistryVersion = "1.0.0",
        };

        var engine = new DemotionEngine(new DemotionConfig
        {
            GaugeLeakThreshold = 0.3,
            BranchStabilityThreshold = 0.5,
            RefinementStabilityThreshold = 0.5,
            BackendStabilityThreshold = 0.8,
        });
        var demoted = engine.ApplyDemotions(candidate);

        // Gauge leak alone demotes to C0, so subsequent rules won't trigger further
        // (they check currentClass > C0)
        Assert.Equal(BosonClaimClass.C0_NumericalMode, demoted.ClaimClass);
        Assert.True(demoted.Demotions.Count >= 1);
        Assert.Contains(demoted.Demotions, d => d.Reason == DemotionReason.GaugeLeak);
    }

    [Fact]
    public void CandidateBosonBuilder_EmptyInput_ReturnsEmptyList()
    {
        var builder = new CandidateBosonBuilder();
        var spectra = new Dictionary<string, SpectrumBundle>();
        var candidates = builder.BuildFromFamilies(Array.Empty<ModeFamilyRecord>(), spectra);

        Assert.Empty(candidates);
    }

    [Fact]
    public void BosonRegistry_FilterByClaimClass()
    {
        var registry = new BosonRegistry();

        registry.Register(MakeCandidate("c-0", BosonClaimClass.C0_NumericalMode));
        registry.Register(MakeCandidate("c-1", BosonClaimClass.C1_LocalPersistentMode));
        registry.Register(MakeCandidate("c-2", BosonClaimClass.C2_BranchStableBosonicCandidate));
        registry.Register(MakeCandidate("c-3", BosonClaimClass.C3_ObservedStableCandidate));

        var atLeastC2 = registry.QueryByClaimClass(BosonClaimClass.C2_BranchStableBosonicCandidate);
        Assert.Equal(2, atLeastC2.Count);

        var atLeastC0 = registry.QueryByClaimClass(BosonClaimClass.C0_NumericalMode);
        Assert.Equal(4, atLeastC0.Count);

        var atLeastC5 = registry.QueryByClaimClass(BosonClaimClass.C5_StrongIdentificationCandidate);
        Assert.Empty(atLeastC5);
    }

    private static CandidateBosonRecord MakeCandidate(string id, BosonClaimClass claimClass)
    {
        return new CandidateBosonRecord
        {
            CandidateId = id,
            PrimaryFamilyId = $"f-{id}",
            ContributingModeIds = new[] { $"m-{id}" },
            BackgroundSet = new[] { "bg-1" },
            MassLikeEnvelope = new[] { 1.0, 1.0, 1.0 },
            MultiplicityEnvelope = new[] { 1, 1, 1 },
            GaugeLeakEnvelope = new[] { 0.01, 0.01, 0.01 },
            ClaimClass = claimClass,
            RegistryVersion = "1.0.0",
        };
    }

    [Fact]
    public void DemotionEngine_UnverifiedGpu_CapsAtC1()
    {
        var candidate = new CandidateBosonRecord
        {
            CandidateId = "c-gpu",
            PrimaryFamilyId = "f-gpu",
            ContributingModeIds = new[] { "m-0" },
            BackgroundSet = new[] { "bg-1" },
            MassLikeEnvelope = new[] { 1.0, 1.0, 1.0 },
            MultiplicityEnvelope = new[] { 1, 1, 1 },
            GaugeLeakEnvelope = new[] { 0.01, 0.01, 0.01 },
            BranchStabilityScore = 0.9,
            RefinementStabilityScore = 0.9,
            BackendStabilityScore = 0.95,
            ObservationStabilityScore = 0.9,
            ComputedWithUnverifiedGpu = true, // Unverified GPU
            ClaimClass = BosonClaimClass.C3_ObservedStableCandidate,
            RegistryVersion = "1.0.0",
        };

        var engine = new DemotionEngine();
        var demoted = engine.ApplyDemotions(candidate);

        Assert.True(demoted.ClaimClass <= BosonClaimClass.C1_LocalPersistentMode);
        Assert.Contains(demoted.Demotions, d =>
            d.Reason == DemotionReason.BackendFragility &&
            d.Details.Contains("unverified GPU"));
    }

    [Fact]
    public void DemotionEngine_VerifiedGpu_NoDemotion()
    {
        // When ComputedWithUnverifiedGpu is false (default), no GPU demotion
        var candidate = new CandidateBosonRecord
        {
            CandidateId = "c-verified",
            PrimaryFamilyId = "f-verified",
            ContributingModeIds = new[] { "m-0" },
            BackgroundSet = new[] { "bg-1" },
            MassLikeEnvelope = new[] { 1.0, 1.0, 1.0 },
            MultiplicityEnvelope = new[] { 1, 1, 1 },
            GaugeLeakEnvelope = new[] { 0.01, 0.01, 0.01 },
            BranchStabilityScore = 0.9,
            RefinementStabilityScore = 0.9,
            BackendStabilityScore = 0.95,
            ObservationStabilityScore = 0.9,
            ComputedWithUnverifiedGpu = false,
            ClaimClass = BosonClaimClass.C3_ObservedStableCandidate,
            RegistryVersion = "1.0.0",
        };

        var engine = new DemotionEngine();
        var result = engine.ApplyDemotions(candidate);

        Assert.Equal(BosonClaimClass.C3_ObservedStableCandidate, result.ClaimClass);
        Assert.Empty(result.Demotions);
    }

    [Fact]
    public void EndToEnd_FamilyToRegistryWithDemotion()
    {
        // Build families
        var family1 = MakeFamily("f-good",
            new[] { "m-0", "m-1" }, new[] { "bg-1", "bg-2" }, stable: true);
        var family2 = MakeFamily("f-leaky",
            new[] { "m-2", "m-3" }, new[] { "bg-1", "bg-2" }, stable: true);

        var spectra = new Dictionary<string, SpectrumBundle>
        {
            ["bg-1"] = MakeSpectrum("bg-1", new[]
            {
                ("m-0", 1.0, 0.01),  // Good mode
                ("m-2", 2.0, 0.9),   // Leaky mode
            }),
            ["bg-2"] = MakeSpectrum("bg-2", new[]
            {
                ("m-1", 1.1, 0.02),
                ("m-3", 2.1, 0.85),
            }),
        };

        var builder = new CandidateBosonBuilder(
            new DemotionConfig { GaugeLeakThreshold = 0.3 });

        var candidates = builder.BuildFromFamilies(
            new[] { family1, family2 }, spectra);

        var registry = new BosonRegistry();
        foreach (var c in candidates)
            registry.Register(c);

        Assert.Equal(2, registry.Count);

        // The leaky candidate should be demoted
        var leaky = registry.QueryByFamily("f-leaky");
        Assert.NotNull(leaky);
        Assert.Equal(BosonClaimClass.C0_NumericalMode, leaky!.ClaimClass);
        Assert.True(leaky.Demotions.Count > 0);

        // The good candidate should keep its claim
        var good = registry.QueryByFamily("f-good");
        Assert.NotNull(good);
        Assert.True(good!.ClaimClass >= BosonClaimClass.C1_LocalPersistentMode);
    }

    // --- GAP-4 Envelope Tests ---

    private static BosonPropertyVector MakePropertyVector(
        string modeId, string bgId,
        string dominantClass, double dominanceFraction,
        double? parityEigenvalue, string[] symmetryLabels,
        InteractionProxyRecord[]? proxies = null)
    {
        return new BosonPropertyVector
        {
            ModeId = modeId,
            BackgroundId = bgId,
            MassLikeScale = new MassLikeScaleRecord
            {
                ModeId = modeId,
                BackgroundId = bgId,
                Eigenvalue = 1.0,
                MassLikeScale = 1.0,
                ExtractionMethod = "eigenvalue",
                OperatorType = "GaussNewton",
            },
            Polarization = new PolarizationDescriptor
            {
                ModeId = modeId,
                BackgroundId = bgId,
                DominantClass = dominantClass,
                DominanceFraction = dominanceFraction,
                BlockEnergyFractions = new Dictionary<string, double>
                {
                    [dominantClass] = dominanceFraction,
                    ["other"] = 1.0 - dominanceFraction,
                },
            },
            Symmetry = new SymmetryDescriptor
            {
                ModeId = modeId,
                BackgroundId = bgId,
                ParityEigenvalue = parityEigenvalue,
                SymmetryLabels = symmetryLabels,
            },
            GaugeLeakScore = 0.01,
            Multiplicity = 1,
            InteractionProxies = proxies ?? Array.Empty<InteractionProxyRecord>(),
        };
    }

    [Fact]
    public void BuildFromFamily_WithPropertyVectors_PopulatesPolarizationEnvelope()
    {
        var family = MakeFamily("f-pol", new[] { "m-0", "m-1", "m-2" }, new[] { "bg-1", "bg-2" });
        var spectra = new Dictionary<string, SpectrumBundle>
        {
            ["bg-1"] = MakeSpectrum("bg-1", new[]
            {
                ("m-0", 1.0, 0.01),
                ("m-2", 1.5, 0.01),
            }),
            ["bg-2"] = MakeSpectrum("bg-2", new[] { ("m-1", 1.1, 0.02) }),
        };

        // Two modes are "vector-like", one is "scalar-like" -> majority is "vector-like"
        var props = new Dictionary<string, BosonPropertyVector>
        {
            ["m-0"] = MakePropertyVector("m-0", "bg-1", "vector-like", 0.7, 1, new[] { "A1" }),
            ["m-1"] = MakePropertyVector("m-1", "bg-2", "vector-like", 0.9, 1, new[] { "A1" }),
            ["m-2"] = MakePropertyVector("m-2", "bg-1", "scalar-like", 0.6, -1, new[] { "B1" }),
        };

        var builder = new CandidateBosonBuilder();
        var candidate = builder.BuildFromFamily(family, spectra, props);

        Assert.NotNull(candidate.PolarizationEnvelope);
        Assert.Equal("vector-like", candidate.PolarizationEnvelope!.DominantClass);
        Assert.Equal(0.6, candidate.PolarizationEnvelope.MinFraction, 6);
        Assert.Equal(0.9, candidate.PolarizationEnvelope.MaxFraction, 6);
    }

    [Fact]
    public void BuildFromFamily_WithPropertyVectors_PopulatesSymmetryEnvelope()
    {
        var family = MakeFamily("f-sym", new[] { "m-0", "m-1" }, new[] { "bg-1" });
        var spectra = new Dictionary<string, SpectrumBundle>
        {
            ["bg-1"] = MakeSpectrum("bg-1", new[]
            {
                ("m-0", 1.0, 0.01),
                ("m-1", 1.1, 0.02),
            }),
        };

        var props = new Dictionary<string, BosonPropertyVector>
        {
            ["m-0"] = MakePropertyVector("m-0", "bg-1", "vector-like", 0.8, 1, new[] { "A1", "E" }),
            ["m-1"] = MakePropertyVector("m-1", "bg-1", "vector-like", 0.9, -1, new[] { "B1", "E" }),
        };

        var builder = new CandidateBosonBuilder();
        var candidate = builder.BuildFromFamily(family, spectra, props);

        Assert.NotNull(candidate.SymmetryEnvelope);
        Assert.Equal(-1, candidate.SymmetryEnvelope!.MinParity);
        Assert.Equal(1, candidate.SymmetryEnvelope.MaxParity);
        // Union of labels: A1, B1, E (sorted)
        Assert.Equal(3, candidate.SymmetryEnvelope.UnionLabels.Count);
        Assert.Contains("A1", candidate.SymmetryEnvelope.UnionLabels);
        Assert.Contains("B1", candidate.SymmetryEnvelope.UnionLabels);
        Assert.Contains("E", candidate.SymmetryEnvelope.UnionLabels);
    }

    [Fact]
    public void BuildFromFamily_WithMixedParity_ParityIsNull()
    {
        var family = MakeFamily("f-mixed", new[] { "m-0", "m-1" }, new[] { "bg-1" });
        var spectra = new Dictionary<string, SpectrumBundle>
        {
            ["bg-1"] = MakeSpectrum("bg-1", new[]
            {
                ("m-0", 1.0, 0.01),
                ("m-1", 1.1, 0.02),
            }),
        };

        var props = new Dictionary<string, BosonPropertyVector>
        {
            ["m-0"] = MakePropertyVector("m-0", "bg-1", "vector-like", 0.8, 1, Array.Empty<string>()),
            ["m-1"] = MakePropertyVector("m-1", "bg-1", "vector-like", 0.9, null, Array.Empty<string>()), // null parity
        };

        var builder = new CandidateBosonBuilder();
        var candidate = builder.BuildFromFamily(family, spectra, props);

        Assert.NotNull(candidate.SymmetryEnvelope);
        Assert.Null(candidate.SymmetryEnvelope!.MinParity);
        Assert.Null(candidate.SymmetryEnvelope.MaxParity);
    }

    [Fact]
    public void BuildFromFamily_WithInteractionProxies_PopulatesInteractionProxyEnvelope()
    {
        var family = MakeFamily("f-proxy", new[] { "m-0", "m-1" }, new[] { "bg-1" });
        var spectra = new Dictionary<string, SpectrumBundle>
        {
            ["bg-1"] = MakeSpectrum("bg-1", new[]
            {
                ("m-0", 1.0, 0.01),
                ("m-1", 1.1, 0.02),
            }),
        };

        var proxies0 = new[]
        {
            new InteractionProxyRecord
            {
                ModeIds = new[] { "m-0", "m-0", "m-0" },
                CubicResponse = -0.5,
                Epsilon = 1e-4,
                Method = "finite-difference-gradient",
                BackgroundId = "bg-1",
            },
        };
        var proxies1 = new[]
        {
            new InteractionProxyRecord
            {
                ModeIds = new[] { "m-0", "m-0", "m-1" },
                CubicResponse = 1.2,
                Epsilon = 1e-4,
                Method = "finite-difference-gradient",
                BackgroundId = "bg-1",
            },
        };

        var props = new Dictionary<string, BosonPropertyVector>
        {
            ["m-0"] = MakePropertyVector("m-0", "bg-1", "vector-like", 0.8, 1, new[] { "A1" }, proxies0),
            ["m-1"] = MakePropertyVector("m-1", "bg-1", "vector-like", 0.9, 1, new[] { "A1" }, proxies1),
        };

        var builder = new CandidateBosonBuilder();
        var candidate = builder.BuildFromFamily(family, spectra, props);

        Assert.NotNull(candidate.InteractionProxyEnvelope);
        Assert.Equal(0.5, candidate.InteractionProxyEnvelope!.MinCubicResponse, 6); // |CubicResponse|
        Assert.Equal(1.2, candidate.InteractionProxyEnvelope.MaxCubicResponse, 6);
        Assert.Equal(2, candidate.InteractionProxyEnvelope.ProxyCount);
    }

    [Fact]
    public void BuildFromFamily_NoProxies_InteractionProxyEnvelopeIsNull()
    {
        var family = MakeFamily("f-noproxy", new[] { "m-0" }, new[] { "bg-1" });
        var spectra = new Dictionary<string, SpectrumBundle>
        {
            ["bg-1"] = MakeSpectrum("bg-1", new[] { ("m-0", 1.0, 0.01) }),
        };

        var props = new Dictionary<string, BosonPropertyVector>
        {
            ["m-0"] = MakePropertyVector("m-0", "bg-1", "scalar-like", 0.95, 1, new[] { "A1" }),
        };

        var builder = new CandidateBosonBuilder();
        var candidate = builder.BuildFromFamily(family, spectra, props);

        Assert.Null(candidate.InteractionProxyEnvelope);
    }

    [Fact]
    public void BuildFromFamily_NoPropertyVectors_EnvelopesAreNull()
    {
        var family = MakeFamily("f-nopv", new[] { "m-0" }, new[] { "bg-1" });
        var spectra = new Dictionary<string, SpectrumBundle>
        {
            ["bg-1"] = MakeSpectrum("bg-1", new[] { ("m-0", 1.0, 0.01) }),
        };

        var builder = new CandidateBosonBuilder();
        // Call without property vectors
        var candidate = builder.BuildFromFamily(family, spectra);

        Assert.Null(candidate.PolarizationEnvelope);
        Assert.Null(candidate.SymmetryEnvelope);
        Assert.Null(candidate.InteractionProxyEnvelope);
    }

    [Fact]
    public void CandidateBosonRecord_JsonRoundtrip_WithEnvelopes()
    {
        var original = new CandidateBosonRecord
        {
            CandidateId = "c-env",
            PrimaryFamilyId = "f-env",
            ContributingModeIds = new[] { "m-0", "m-1" },
            BackgroundSet = new[] { "bg-1" },
            MassLikeEnvelope = new[] { 1.0, 1.5, 2.0 },
            MultiplicityEnvelope = new[] { 1, 1, 1 },
            GaugeLeakEnvelope = new[] { 0.01, 0.02, 0.03 },
            PolarizationEnvelope = new PolarizationEnvelope
            {
                DominantClass = "vector-like",
                MinFraction = 0.6,
                MaxFraction = 0.9,
            },
            SymmetryEnvelope = new SymmetryEnvelope
            {
                MinParity = -1,
                MaxParity = 1,
                UnionLabels = new[] { "A1", "B1", "E" },
            },
            InteractionProxyEnvelope = new InteractionProxyEnvelope
            {
                MinCubicResponse = 0.3,
                MaxCubicResponse = 1.5,
                ProxyCount = 4,
            },
            ClaimClass = BosonClaimClass.C2_BranchStableBosonicCandidate,
            RegistryVersion = "1.0.0",
        };

        var options = new JsonSerializerOptions
        {
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() },
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        };

        var json = JsonSerializer.Serialize(original, options);
        var deserialized = JsonSerializer.Deserialize<CandidateBosonRecord>(json, options);

        Assert.NotNull(deserialized);

        // PolarizationEnvelope round-trip
        Assert.NotNull(deserialized!.PolarizationEnvelope);
        Assert.Equal("vector-like", deserialized.PolarizationEnvelope!.DominantClass);
        Assert.Equal(0.6, deserialized.PolarizationEnvelope.MinFraction, 6);
        Assert.Equal(0.9, deserialized.PolarizationEnvelope.MaxFraction, 6);

        // SymmetryEnvelope round-trip
        Assert.NotNull(deserialized.SymmetryEnvelope);
        Assert.Equal(-1, deserialized.SymmetryEnvelope!.MinParity);
        Assert.Equal(1, deserialized.SymmetryEnvelope.MaxParity);
        Assert.Equal(3, deserialized.SymmetryEnvelope.UnionLabels.Count);
        Assert.Contains("A1", deserialized.SymmetryEnvelope.UnionLabels);

        // InteractionProxyEnvelope round-trip
        Assert.NotNull(deserialized.InteractionProxyEnvelope);
        Assert.Equal(0.3, deserialized.InteractionProxyEnvelope!.MinCubicResponse, 6);
        Assert.Equal(1.5, deserialized.InteractionProxyEnvelope.MaxCubicResponse, 6);
        Assert.Equal(4, deserialized.InteractionProxyEnvelope.ProxyCount);
    }

    [Fact]
    public void CandidateBosonRecord_JsonRoundtrip_NullEnvelopesOmitted()
    {
        var original = new CandidateBosonRecord
        {
            CandidateId = "c-null-env",
            PrimaryFamilyId = "f-null-env",
            ContributingModeIds = new[] { "m-0" },
            BackgroundSet = new[] { "bg-1" },
            MassLikeEnvelope = new[] { 1.0, 1.0, 1.0 },
            MultiplicityEnvelope = new[] { 1, 1, 1 },
            GaugeLeakEnvelope = new[] { 0.01, 0.01, 0.01 },
            // All three envelopes left null
            ClaimClass = BosonClaimClass.C0_NumericalMode,
            RegistryVersion = "1.0.0",
        };

        var options = new JsonSerializerOptions
        {
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() },
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        };

        var json = JsonSerializer.Serialize(original, options);

        // Null envelopes should be omitted from JSON
        Assert.DoesNotContain("polarizationEnvelope", json);
        Assert.DoesNotContain("interactionProxyEnvelope", json);
        Assert.DoesNotContain("symmetryEnvelope", json);

        // Round-trip preserves null
        var deserialized = JsonSerializer.Deserialize<CandidateBosonRecord>(json, options);
        Assert.NotNull(deserialized);
        Assert.Null(deserialized!.PolarizationEnvelope);
        Assert.Null(deserialized.SymmetryEnvelope);
        Assert.Null(deserialized.InteractionProxyEnvelope);
    }

    // --- GAP-5: ObservationInstability demotion rule ---

    [Fact]
    public void DemotionEngine_ObservationInstability_DemotesByOne()
    {
        var candidate = new CandidateBosonRecord
        {
            CandidateId = "c-obs",
            PrimaryFamilyId = "f-obs",
            ContributingModeIds = new[] { "m-0" },
            BackgroundSet = new[] { "bg-1" },
            MassLikeEnvelope = new[] { 1.0, 1.0, 1.0 },
            MultiplicityEnvelope = new[] { 1, 1, 1 },
            GaugeLeakEnvelope = new[] { 0.01, 0.01, 0.01 },
            BranchStabilityScore = 0.9,
            RefinementStabilityScore = 0.9,
            BackendStabilityScore = 0.95,
            ObservationStabilityScore = 0.2, // Below threshold 0.4
            ClaimClass = BosonClaimClass.C3_ObservedStableCandidate,
            RegistryVersion = "1.0.0",
        };

        var engine = new DemotionEngine(new DemotionConfig { ObservationInstabilityThreshold = 0.4 });
        var demoted = engine.ApplyDemotions(candidate);

        Assert.Equal(BosonClaimClass.C2_BranchStableBosonicCandidate, demoted.ClaimClass);
        Assert.Contains(demoted.Demotions, d => d.Reason == DemotionReason.ObservationInstability);
    }

    // --- GAP-5: ComparisonMismatch demotion rule ---

    [Fact]
    public void DemotionEngine_ComparisonMismatch_CapsAtC1()
    {
        var candidate = new CandidateBosonRecord
        {
            CandidateId = "c-cmp",
            PrimaryFamilyId = "f-cmp",
            ContributingModeIds = new[] { "m-0" },
            BackgroundSet = new[] { "bg-1" },
            MassLikeEnvelope = new[] { 1.0, 1.0, 1.0 },
            MultiplicityEnvelope = new[] { 1, 1, 1 },
            GaugeLeakEnvelope = new[] { 0.01, 0.01, 0.01 },
            BranchStabilityScore = 0.9,
            RefinementStabilityScore = 0.9,
            BackendStabilityScore = 0.95,
            ObservationStabilityScore = 0.9,
            ClaimClass = BosonClaimClass.C4_PhysicalAnalogyCandidate,
            RegistryVersion = "1.0.0",
        };

        var engine = new DemotionEngine();
        var demoted = engine.ApplyComparisonMismatch(candidate, allResultsIncompatible: true);

        Assert.Equal(BosonClaimClass.C1_LocalPersistentMode, demoted.ClaimClass);
        Assert.Contains(demoted.Demotions, d => d.Reason == DemotionReason.ComparisonMismatch);
    }

    [Fact]
    public void DemotionEngine_ComparisonMismatch_NotIncompatible_NoDemotion()
    {
        var candidate = MakeCandidate("c-cmp-ok", BosonClaimClass.C3_ObservedStableCandidate);

        var engine = new DemotionEngine();
        var result = engine.ApplyComparisonMismatch(candidate, allResultsIncompatible: false);

        Assert.Equal(BosonClaimClass.C3_ObservedStableCandidate, result.ClaimClass);
    }

    [Fact]
    public void DemotionEngine_ComparisonMismatch_AlreadyAtC1_NoDemotion()
    {
        var candidate = MakeCandidate("c-cmp-c1", BosonClaimClass.C1_LocalPersistentMode);

        var engine = new DemotionEngine();
        var result = engine.ApplyComparisonMismatch(candidate, allResultsIncompatible: true);

        Assert.Equal(BosonClaimClass.C1_LocalPersistentMode, result.ClaimClass);
        Assert.Empty(result.Demotions);
    }

    // --- GAP-5: AmbiguousMatching demotion rule ---

    [Fact]
    public void DemotionEngine_AmbiguousMatching_DemotesByOne()
    {
        var candidate = new CandidateBosonRecord
        {
            CandidateId = "c-amb",
            PrimaryFamilyId = "f-amb",
            ContributingModeIds = new[] { "m-0" },
            BackgroundSet = new[] { "bg-1" },
            MassLikeEnvelope = new[] { 1.0, 1.0, 1.0 },
            MultiplicityEnvelope = new[] { 1, 1, 1 },
            GaugeLeakEnvelope = new[] { 0.01, 0.01, 0.01 },
            BranchStabilityScore = 0.9,
            RefinementStabilityScore = 0.9,
            BackendStabilityScore = 0.95,
            ObservationStabilityScore = 0.9,
            AmbiguityCount = 5, // Above threshold of 2
            ClaimClass = BosonClaimClass.C3_ObservedStableCandidate,
            RegistryVersion = "1.0.0",
        };

        var engine = new DemotionEngine(new DemotionConfig { AmbiguousMatchingThreshold = 2 });
        var demoted = engine.ApplyDemotions(candidate);

        Assert.Equal(BosonClaimClass.C2_BranchStableBosonicCandidate, demoted.ClaimClass);
        Assert.Contains(demoted.Demotions, d => d.Reason == DemotionReason.AmbiguousMatching);
    }

    [Fact]
    public void DemotionEngine_AmbiguousMatching_BelowThreshold_NoDemotion()
    {
        var candidate = new CandidateBosonRecord
        {
            CandidateId = "c-amb-ok",
            PrimaryFamilyId = "f-amb-ok",
            ContributingModeIds = new[] { "m-0" },
            BackgroundSet = new[] { "bg-1" },
            MassLikeEnvelope = new[] { 1.0, 1.0, 1.0 },
            MultiplicityEnvelope = new[] { 1, 1, 1 },
            GaugeLeakEnvelope = new[] { 0.01, 0.01, 0.01 },
            BranchStabilityScore = 0.9,
            RefinementStabilityScore = 0.9,
            BackendStabilityScore = 0.95,
            ObservationStabilityScore = 0.9,
            AmbiguityCount = 1, // At or below threshold of 2
            ClaimClass = BosonClaimClass.C3_ObservedStableCandidate,
            RegistryVersion = "1.0.0",
        };

        var engine = new DemotionEngine(new DemotionConfig { AmbiguousMatchingThreshold = 2 });
        var result = engine.ApplyDemotions(candidate);

        Assert.Equal(BosonClaimClass.C3_ObservedStableCandidate, result.ClaimClass);
        Assert.Empty(result.Demotions);
    }

    // --- GAP-5: Registry diffing tests ---

    [Fact]
    public void RegistryDiff_SameRegistry_EmptyDiff()
    {
        var registry = new BosonRegistry();
        registry.Register(MakeCandidate("c-1", BosonClaimClass.C2_BranchStableBosonicCandidate));
        registry.Register(MakeCandidate("c-2", BosonClaimClass.C1_LocalPersistentMode));

        var diff = registry.Diff(registry);

        Assert.Empty(diff.NewCandidateIds);
        Assert.Empty(diff.RemovedCandidateIds);
        Assert.Empty(diff.ClaimClassChanges);
        Assert.Empty(diff.DemotionChanges);
    }

    [Fact]
    public void RegistryDiff_NewAndRemovedCandidates()
    {
        var oldRegistry = new BosonRegistry();
        oldRegistry.Register(MakeCandidate("c-1", BosonClaimClass.C2_BranchStableBosonicCandidate));
        oldRegistry.Register(MakeCandidate("c-2", BosonClaimClass.C1_LocalPersistentMode));

        var newRegistry = new BosonRegistry();
        newRegistry.Register(MakeCandidate("c-2", BosonClaimClass.C1_LocalPersistentMode));
        newRegistry.Register(MakeCandidate("c-3", BosonClaimClass.C0_NumericalMode));

        var diff = oldRegistry.Diff(newRegistry);

        Assert.Single(diff.NewCandidateIds);
        Assert.Contains("c-3", diff.NewCandidateIds);
        Assert.Single(diff.RemovedCandidateIds);
        Assert.Contains("c-1", diff.RemovedCandidateIds);
    }

    [Fact]
    public void RegistryDiff_ClaimClassChanges()
    {
        var oldRegistry = new BosonRegistry();
        oldRegistry.Register(MakeCandidate("c-1", BosonClaimClass.C3_ObservedStableCandidate));

        var newRegistry = new BosonRegistry();
        newRegistry.Register(MakeCandidate("c-1", BosonClaimClass.C1_LocalPersistentMode));

        var diff = oldRegistry.Diff(newRegistry);

        Assert.Single(diff.ClaimClassChanges);
        Assert.Equal("c-1", diff.ClaimClassChanges[0].CandidateId);
        Assert.Equal(BosonClaimClass.C3_ObservedStableCandidate, diff.ClaimClassChanges[0].Before);
        Assert.Equal(BosonClaimClass.C1_LocalPersistentMode, diff.ClaimClassChanges[0].After);
    }

    [Fact]
    public void RegistryDiff_DemotionChanges()
    {
        var baseCand = MakeCandidate("c-1", BosonClaimClass.C2_BranchStableBosonicCandidate);
        var oldRegistry = new BosonRegistry();
        oldRegistry.Register(baseCand);

        var demotedCand = new CandidateBosonRecord
        {
            CandidateId = "c-1",
            PrimaryFamilyId = "f-c-1",
            ContributingModeIds = new[] { "m-c-1" },
            BackgroundSet = new[] { "bg-1" },
            MassLikeEnvelope = new[] { 1.0, 1.0, 1.0 },
            MultiplicityEnvelope = new[] { 1, 1, 1 },
            GaugeLeakEnvelope = new[] { 0.01, 0.01, 0.01 },
            ClaimClass = BosonClaimClass.C1_LocalPersistentMode,
            Demotions = new[]
            {
                new BosonDemotionRecord
                {
                    CandidateId = "c-1",
                    Reason = DemotionReason.ObservationInstability,
                    PreviousClaimClass = BosonClaimClass.C2_BranchStableBosonicCandidate,
                    DemotedClaimClass = BosonClaimClass.C1_LocalPersistentMode,
                    Details = "Observation instability triggered",
                },
            },
            RegistryVersion = "1.0.0",
        };
        var newRegistry = new BosonRegistry();
        newRegistry.Register(demotedCand);

        var diff = oldRegistry.Diff(newRegistry);

        Assert.Single(diff.DemotionChanges);
        Assert.Equal("c-1", diff.DemotionChanges[0].CandidateId);
        Assert.Single(diff.DemotionChanges[0].Added);
        Assert.Equal(DemotionReason.ObservationInstability, diff.DemotionChanges[0].Added[0].Reason);
    }

    [Fact]
    public void FromJson_RoundTrip_PreservesCandidates()
    {
        var registry = new BosonRegistry();
        registry.Register(MakeCandidate("c-1", BosonClaimClass.C0_NumericalMode));
        registry.Register(MakeCandidate("c-2", BosonClaimClass.C2_BranchStableBosonicCandidate));
        registry.Register(MakeCandidate("c-3", BosonClaimClass.C4_PhysicalAnalogyCandidate));

        var json = registry.ToJson();
        var restored = BosonRegistry.FromJson(json);

        Assert.Equal(3, restored.Count);
        Assert.Equal(BosonClaimClass.C0_NumericalMode,
            restored.Candidates.Single(c => c.CandidateId == "c-1").ClaimClass);
        Assert.Equal(BosonClaimClass.C2_BranchStableBosonicCandidate,
            restored.Candidates.Single(c => c.CandidateId == "c-2").ClaimClass);
        Assert.Equal(BosonClaimClass.C4_PhysicalAnalogyCandidate,
            restored.Candidates.Single(c => c.CandidateId == "c-3").ClaimClass);
    }

    [Fact]
    public void RegistryDiff_DuplicateDemotionReason_BothDetected()
    {
        // Base: candidate with one ComparisonMismatch demotion
        var baseDemotion = new BosonDemotionRecord
        {
            CandidateId = "c-1",
            Reason = DemotionReason.ComparisonMismatch,
            PreviousClaimClass = BosonClaimClass.C3_ObservedStableCandidate,
            DemotedClaimClass = BosonClaimClass.C2_BranchStableBosonicCandidate,
            Details = "mismatch-1",
        };
        var baseCandidate = new CandidateBosonRecord
        {
            CandidateId = "c-1",
            PrimaryFamilyId = "f-c-1",
            ContributingModeIds = new[] { "m-c-1" },
            BackgroundSet = new[] { "bg-1" },
            MassLikeEnvelope = new[] { 1.0, 1.0, 1.0 },
            MultiplicityEnvelope = new[] { 1, 1, 1 },
            GaugeLeakEnvelope = new[] { 0.01, 0.01, 0.01 },
            ClaimClass = BosonClaimClass.C2_BranchStableBosonicCandidate,
            Demotions = new[] { baseDemotion },
            RegistryVersion = "1.0.0",
        };
        var baseRegistry = new BosonRegistry();
        baseRegistry.Register(baseCandidate);

        // Other: same candidate now with a second ComparisonMismatch demotion (different details)
        var secondDemotion = new BosonDemotionRecord
        {
            CandidateId = "c-1",
            Reason = DemotionReason.ComparisonMismatch,
            PreviousClaimClass = BosonClaimClass.C2_BranchStableBosonicCandidate,
            DemotedClaimClass = BosonClaimClass.C1_LocalPersistentMode,
            Details = "mismatch-2",
        };
        var otherCandidate = new CandidateBosonRecord
        {
            CandidateId = "c-1",
            PrimaryFamilyId = "f-c-1",
            ContributingModeIds = new[] { "m-c-1" },
            BackgroundSet = new[] { "bg-1" },
            MassLikeEnvelope = new[] { 1.0, 1.0, 1.0 },
            MultiplicityEnvelope = new[] { 1, 1, 1 },
            GaugeLeakEnvelope = new[] { 0.01, 0.01, 0.01 },
            ClaimClass = BosonClaimClass.C1_LocalPersistentMode,
            Demotions = new[] { baseDemotion, secondDemotion },
            RegistryVersion = "1.0.0",
        };
        var otherRegistry = new BosonRegistry();
        otherRegistry.Register(otherCandidate);

        var diff = baseRegistry.Diff(otherRegistry);

        Assert.Single(diff.DemotionChanges);
        Assert.Equal("c-1", diff.DemotionChanges[0].CandidateId);
        Assert.Single(diff.DemotionChanges[0].Added);
        Assert.Equal(DemotionReason.ComparisonMismatch, diff.DemotionChanges[0].Added[0].Reason);
        Assert.Equal("mismatch-2", diff.DemotionChanges[0].Added[0].Details);
    }
}
