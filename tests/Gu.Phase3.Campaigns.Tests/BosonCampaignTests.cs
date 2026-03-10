using Gu.Phase3.Campaigns;
using Gu.Phase3.Registry;

namespace Gu.Phase3.Campaigns.Tests;

public class BosonCampaignTests
{
    private static CandidateBosonRecord MakeCandidate(
        string id,
        BosonClaimClass claimClass = BosonClaimClass.C2_BranchStableBosonicCandidate,
        double mass = 1.0,
        int multiplicity = 1,
        double gaugeLeak = 0.01,
        double branchStability = 0.9)
    {
        return new CandidateBosonRecord
        {
            CandidateId = id,
            PrimaryFamilyId = $"f-{id}",
            ContributingModeIds = new[] { $"m-{id}" },
            BackgroundSet = new[] { "bg-1" },
            MassLikeEnvelope = new[] { mass * 0.9, mass, mass * 1.1 },
            MultiplicityEnvelope = new[] { multiplicity, multiplicity, multiplicity },
            GaugeLeakEnvelope = new[] { gaugeLeak * 0.5, gaugeLeak, gaugeLeak * 1.5 },
            BranchStabilityScore = branchStability,
            RefinementStabilityScore = 0.9,
            BackendStabilityScore = 0.95,
            ObservationStabilityScore = 0.9,
            ClaimClass = claimClass,
            RegistryVersion = "1.0.0",
        };
    }

    [Fact]
    public void ProfileMatcher_Compatible_WhenAllCriteriaMatch()
    {
        var candidate = MakeCandidate("c-1", mass: 1.0, multiplicity: 2, gaugeLeak: 0.01);
        var profile = new BosonTargetProfile
        {
            ProfileId = "p-vector",
            Label = "massless vector-like",
            MassRange = new[] { 0.5, 1.5 },
            ExpectedMultiplicity = 2,
            MaxGaugeLeak = 0.1,
        };

        var matcher = new BosonProfileMatcher();
        var result = matcher.CompareToProfile(candidate, profile);

        Assert.Equal(ComparisonVerdict.Compatible, result.Verdict);
        Assert.True(result.CompatibilityScore > 0.5);
        Assert.Equal("c-1", result.CandidateId);
        Assert.Equal("p-vector", result.TargetId);
    }

    [Fact]
    public void ProfileMatcher_Incompatible_WhenMassOutOfRange()
    {
        var candidate = MakeCandidate("c-2", mass: 10.0);
        var profile = new BosonTargetProfile
        {
            ProfileId = "p-low",
            Label = "low-mass scalar",
            MassRange = new[] { 0.0, 2.0 },
            MaxGaugeLeak = 0.1,
        };

        var matcher = new BosonProfileMatcher();
        var result = matcher.CompareToProfile(candidate, profile);

        Assert.Equal(ComparisonVerdict.Incompatible, result.Verdict);
        Assert.True(result.CompatibilityScore < 0.5);
    }

    [Fact]
    public void ProfileMatcher_Incompatible_WhenGaugeLeakHigh()
    {
        var candidate = MakeCandidate("c-3", gaugeLeak: 0.5);
        var profile = new BosonTargetProfile
        {
            ProfileId = "p-clean",
            Label = "low-leak mode",
            MaxGaugeLeak = 0.05,
        };

        var matcher = new BosonProfileMatcher();
        var result = matcher.CompareToProfile(candidate, profile);

        Assert.Equal(ComparisonVerdict.Incompatible, result.Verdict);
    }

    [Fact]
    public void ProfileMatcher_InsufficientEvidence_WhenNoConstraints()
    {
        var candidate = MakeCandidate("c-4");
        var profile = new BosonTargetProfile
        {
            ProfileId = "p-empty",
            Label = "unconstrained",
        };

        var matcher = new BosonProfileMatcher();
        var result = matcher.CompareToProfile(candidate, profile);

        Assert.Equal(ComparisonVerdict.InsufficientEvidence, result.Verdict);
    }

    [Fact]
    public void ExternalMatcher_Compatible_WhenMassWithinUncertainty()
    {
        var candidate = MakeCandidate("c-5", mass: 1.0, multiplicity: 3, gaugeLeak: 0.01);
        var descriptor = new ExternalAnalogyDescriptor
        {
            DescriptorId = "ext-w",
            Label = "W-boson analogy",
            ReferenceMass = 1.05,
            MassUncertainty = 0.2,
            ExpectedMultiplicity = 3,
            MaxGaugeLeak = 0.1,
            Source = "test-data",
        };

        var matcher = new BosonProfileMatcher();
        var result = matcher.CompareToExternal(candidate, descriptor);

        Assert.Equal(ComparisonVerdict.Compatible, result.Verdict);
        Assert.True(result.CompatibilityScore > 0.5);
    }

    [Fact]
    public void ExternalMatcher_Incompatible_WhenMassFarOff()
    {
        var candidate = MakeCandidate("c-6", mass: 100.0);
        var descriptor = new ExternalAnalogyDescriptor
        {
            DescriptorId = "ext-low",
            Label = "light boson",
            ReferenceMass = 1.0,
            MassUncertainty = 0.5,
            ExpectedMultiplicity = 1,
            MaxGaugeLeak = 0.1,
            Source = "test-data",
        };

        var matcher = new BosonProfileMatcher();
        var result = matcher.CompareToExternal(candidate, descriptor);

        Assert.Equal(ComparisonVerdict.Incompatible, result.Verdict);
    }

    [Fact]
    public void CampaignRunner_BC1_ComparesAllCandidatesToAllProfiles()
    {
        var registry = new BosonRegistry();
        registry.Register(MakeCandidate("c-1", mass: 1.0, multiplicity: 2));
        registry.Register(MakeCandidate("c-2", mass: 5.0, multiplicity: 1));

        var profiles = new Dictionary<string, BosonTargetProfile>
        {
            ["p-1"] = new BosonTargetProfile
            {
                ProfileId = "p-1",
                Label = "vector-like",
                MassRange = new[] { 0.5, 2.0 },
                ExpectedMultiplicity = 2,
                MaxGaugeLeak = 0.1,
            },
        };

        var spec = new BosonCampaignSpec
        {
            CampaignId = "campaign-1",
            Mode = BosonComparisonMode.InternalTargetProfile,
            TargetProfileIds = new[] { "p-1" },
        };

        var runner = new BosonCampaignRunner();
        var result = runner.Run(spec, registry, profiles,
            new Dictionary<string, ExternalAnalogyDescriptor>());

        Assert.Equal("campaign-1", result.CampaignId);
        Assert.Equal(BosonComparisonMode.InternalTargetProfile, result.Mode);
        Assert.Equal(2, result.CandidatesCompared);
        Assert.Equal(1, result.TargetsUsed);
        Assert.Equal(2, result.Results.Count);
    }

    [Fact]
    public void CampaignRunner_BC2_ComparesAgainstExternalDescriptors()
    {
        var registry = new BosonRegistry();
        registry.Register(MakeCandidate("c-1", mass: 1.0, multiplicity: 1, gaugeLeak: 0.01));

        var descriptors = new Dictionary<string, ExternalAnalogyDescriptor>
        {
            ["ext-1"] = new ExternalAnalogyDescriptor
            {
                DescriptorId = "ext-1",
                Label = "test descriptor",
                ReferenceMass = 1.0,
                MassUncertainty = 0.5,
                ExpectedMultiplicity = 1,
                MaxGaugeLeak = 0.1,
                Source = "test",
            },
        };

        var spec = new BosonCampaignSpec
        {
            CampaignId = "campaign-bc2",
            Mode = BosonComparisonMode.ExternalAnalogy,
            ExternalDescriptorIds = new[] { "ext-1" },
        };

        var runner = new BosonCampaignRunner();
        var result = runner.Run(spec, registry,
            new Dictionary<string, BosonTargetProfile>(), descriptors);

        Assert.Equal(1, result.Results.Count);
        Assert.Equal(ComparisonVerdict.Compatible, result.Results[0].Verdict);
    }

    [Fact]
    public void CampaignRunner_NegativeResults_AreFirstClass()
    {
        var registry = new BosonRegistry();
        registry.Register(MakeCandidate("c-bad", mass: 100.0, gaugeLeak: 0.5));

        var profiles = new Dictionary<string, BosonTargetProfile>
        {
            ["p-strict"] = new BosonTargetProfile
            {
                ProfileId = "p-strict",
                Label = "strict low-mass",
                MassRange = new[] { 0.0, 2.0 },
                MaxGaugeLeak = 0.05,
            },
        };

        var spec = new BosonCampaignSpec
        {
            CampaignId = "campaign-neg",
            Mode = BosonComparisonMode.InternalTargetProfile,
            TargetProfileIds = new[] { "p-strict" },
        };

        var runner = new BosonCampaignRunner();
        var result = runner.Run(spec, registry, profiles,
            new Dictionary<string, ExternalAnalogyDescriptor>());

        Assert.True(result.NegativeResults.Count > 0, "Negative results must be recorded");
        Assert.Contains(result.NegativeResults, r => r.Verdict == ComparisonVerdict.Incompatible);
    }

    [Fact]
    public void CampaignRunner_MinClaimClass_FiltersCandidate()
    {
        var registry = new BosonRegistry();
        registry.Register(MakeCandidate("c-weak", claimClass: BosonClaimClass.C0_NumericalMode));
        registry.Register(MakeCandidate("c-strong", claimClass: BosonClaimClass.C3_ObservedStableCandidate));

        var profiles = new Dictionary<string, BosonTargetProfile>
        {
            ["p-1"] = new BosonTargetProfile
            {
                ProfileId = "p-1",
                Label = "any",
            },
        };

        var spec = new BosonCampaignSpec
        {
            CampaignId = "campaign-filter",
            Mode = BosonComparisonMode.InternalTargetProfile,
            TargetProfileIds = new[] { "p-1" },
            MinClaimClass = BosonClaimClass.C2_BranchStableBosonicCandidate,
        };

        var runner = new BosonCampaignRunner();
        var result = runner.Run(spec, registry, profiles,
            new Dictionary<string, ExternalAnalogyDescriptor>());

        // Only C3 candidate should be included (C0 is below C2 minimum)
        Assert.Equal(1, result.CandidatesCompared);
    }

    [Fact]
    public void CampaignResult_NeverForcesUniqueMatch()
    {
        // Two candidates, both compatible with same target
        var registry = new BosonRegistry();
        registry.Register(MakeCandidate("c-a", mass: 1.0, multiplicity: 2, gaugeLeak: 0.01));
        registry.Register(MakeCandidate("c-b", mass: 1.1, multiplicity: 2, gaugeLeak: 0.02));

        var profiles = new Dictionary<string, BosonTargetProfile>
        {
            ["p-1"] = new BosonTargetProfile
            {
                ProfileId = "p-1",
                Label = "vector-like",
                MassRange = new[] { 0.5, 2.0 },
                ExpectedMultiplicity = 2,
                MaxGaugeLeak = 0.1,
            },
        };

        var spec = new BosonCampaignSpec
        {
            CampaignId = "campaign-multi",
            Mode = BosonComparisonMode.InternalTargetProfile,
            TargetProfileIds = new[] { "p-1" },
        };

        var runner = new BosonCampaignRunner();
        var result = runner.Run(spec, registry, profiles,
            new Dictionary<string, ExternalAnalogyDescriptor>());

        // Both candidates should have results -- no unique match forced
        var compatible = result.Results.Where(r => r.Verdict == ComparisonVerdict.Compatible).ToList();
        Assert.Equal(2, compatible.Count);
    }
}
