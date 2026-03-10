using System.Text.Json;
using System.Text.Json.Serialization;
using Gu.Core;
using Gu.Phase3.Campaigns;
using Gu.Phase3.Registry;

namespace Gu.Phase3.Campaigns.Tests;

public sealed class TargetProfileTests
{
    [Fact]
    public void InternalTargetProfile_MasslessVector_HasCorrectProperties()
    {
        var profile = InternalTargetProfile.MasslessVector();

        Assert.Equal("internal-massless-vector", profile.ProfileId);
        Assert.Equal("Massless Vector-Like", profile.ProfileName);
        Assert.Equal(0.0, profile.ExpectedMassRange[0]);
        Assert.Equal(1e-6, profile.ExpectedMassRange[1]);
        Assert.Equal(2, profile.ExpectedMultiplicity);
        Assert.Equal("transverse", profile.ExpectedPolarizationType);
        Assert.Equal("u(1)", profile.ExpectedSymmetryGroup);
        Assert.Equal("massless-vector", profile.InternalType);
    }

    [Fact]
    public void InternalTargetProfile_MassiveScalar_HasCorrectProperties()
    {
        var profile = InternalTargetProfile.MassiveScalar();

        Assert.Equal("internal-massive-scalar", profile.ProfileId);
        Assert.Equal(1, profile.ExpectedMultiplicity);
        Assert.Equal("scalar", profile.ExpectedPolarizationType);
        Assert.Equal("massive-scalar", profile.InternalType);
    }

    [Fact]
    public void InternalTargetProfile_BranchStableMixedTensor_HasCorrectProperties()
    {
        var profile = InternalTargetProfile.BranchStableMixedTensor();

        Assert.Equal("internal-branch-stable-mixed-tensor", profile.ProfileId);
        Assert.Equal(3, profile.ExpectedMultiplicity);
        Assert.Equal("su(2)", profile.ExpectedSymmetryGroup);
        Assert.Equal("branch-stable-mixed-tensor", profile.InternalType);
    }

    [Fact]
    public void InternalTargetProfile_TwoPolarizationLowLeak_HasCorrectProperties()
    {
        var profile = InternalTargetProfile.TwoPolarizationLowLeak();

        Assert.Equal("internal-two-polarization-low-leak", profile.ProfileId);
        Assert.Equal(0.05, profile.Tolerances.MaxGaugeLeakForCompatibility);
    }

    [Fact]
    public void ExternalTargetProfile_Construction()
    {
        var profile = new ExternalTargetProfile
        {
            ProfileId = "ext-photon-analogy",
            ProfileName = "Photon Analogy",
            ExpectedMassRange = [0.0, 1e-10],
            ExpectedMultiplicity = 2,
            ExpectedPolarizationType = "transverse",
            ExpectedSymmetryGroup = "u(1)",
            UncertaintyBudget = new UncertaintyBudget
            {
                MassUncertainty = 1e-8,
                MultiplicityUncertainty = 0,
                PolarizationUncertainty = 0.01,
                SymmetryGroupUncertainty = 0.05,
            },
            SourceDescription = "Standard Model photon analogy",
        };

        Assert.Equal("ext-photon-analogy", profile.ProfileId);
        Assert.Equal(1e-8, profile.UncertaintyBudget.MassUncertainty);
        Assert.Equal("Standard Model photon analogy", profile.SourceDescription);
    }
}

public sealed class ComparisonOutcomeTests
{
    [Fact]
    public void ComparisonOutcome_HasAllExpectedValues()
    {
        var values = Enum.GetValues<ComparisonOutcome>();
        Assert.Equal(4, values.Length);
        Assert.Contains(ComparisonOutcome.Compatible, values);
        Assert.Contains(ComparisonOutcome.Incompatible, values);
        Assert.Contains(ComparisonOutcome.Underdetermined, values);
        Assert.Contains(ComparisonOutcome.InsufficientEvidence, values);
    }
}

public sealed class CampaignRunnerTests
{
    private static ProvenanceMeta MakeProvenance() => new()
    {
        CreatedAt = DateTimeOffset.UtcNow,
        CodeRevision = "test-sha",
        Branch = new BranchRef { BranchId = "test-branch", SchemaVersion = "1.0" },
    };

    private static CandidateBosonRecord MakeCandidate(
        string id,
        double massMean,
        int multMean,
        double gaugeLeak,
        double branchStab = 0.9,
        double refinStab = 0.9,
        BosonClaimClass claimClass = BosonClaimClass.C3_ObservedStableCandidate)
    {
        return new CandidateBosonRecord
        {
            CandidateId = id,
            PrimaryFamilyId = $"family-{id}",
            ContributingModeIds = [$"mode-{id}-1"],
            BackgroundSet = ["bg-1"],
            MassLikeEnvelope = [massMean * 0.9, massMean, massMean * 1.1],
            MultiplicityEnvelope = [multMean, multMean, multMean],
            GaugeLeakEnvelope = [gaugeLeak * 0.8, gaugeLeak, gaugeLeak * 1.2],
            BranchStabilityScore = branchStab,
            RefinementStabilityScore = refinStab,
            BackendStabilityScore = 0.95,
            ObservationStabilityScore = 0.9,
            ClaimClass = claimClass,
            RegistryVersion = "1.0.0",
        };
    }

    [Fact]
    public void CampaignRunner_CompatibleCandidate_MatchesTarget()
    {
        var registry = new BosonRegistry();
        // Massless vector-like candidate: mass near zero, multiplicity 2, low leak
        registry.Register(MakeCandidate("cand-1", massMean: 0.0, multMean: 2, gaugeLeak: 0.01));

        var campaign = new BosonComparisonCampaign
        {
            CampaignId = "test-campaign",
            TargetProfiles = [InternalTargetProfile.MasslessVector()],
            MinimumClaimClass = BosonClaimClass.C0_NumericalMode,
        };

        var runner = new CampaignRunner();
        var result = runner.Run(registry, campaign, MakeProvenance());

        Assert.Equal(1, result.CompatibleCount);
        Assert.Equal(0, result.IncompatibleCount);
        Assert.Equal("cand-1", result.Comparisons[0].CandidateId);
        Assert.Equal(ComparisonOutcome.Compatible, result.Comparisons[0].Outcome);
    }

    [Fact]
    public void CampaignRunner_IncompatibleCandidate_Fails()
    {
        var registry = new BosonRegistry();
        // Massive candidate with wrong multiplicity compared to massless vector target
        registry.Register(MakeCandidate("cand-2", massMean: 50.0, multMean: 7, gaugeLeak: 0.01));

        var campaign = new BosonComparisonCampaign
        {
            CampaignId = "test-incompat",
            TargetProfiles = [InternalTargetProfile.MasslessVector()],
            MinimumClaimClass = BosonClaimClass.C0_NumericalMode,
        };

        var runner = new CampaignRunner();
        var result = runner.Run(registry, campaign, MakeProvenance());

        Assert.Equal(0, result.CompatibleCount);
        Assert.Equal(ComparisonOutcome.Incompatible, result.Comparisons[0].Outcome);
    }

    [Fact]
    public void CampaignRunner_InsufficientEvidence_LowStability()
    {
        var registry = new BosonRegistry();
        // Candidate with very low stability scores
        registry.Register(MakeCandidate("cand-3", massMean: 0.0, multMean: 2, gaugeLeak: 0.01,
            branchStab: 0.1, refinStab: 0.1));

        var campaign = new BosonComparisonCampaign
        {
            CampaignId = "test-insuff",
            TargetProfiles = [InternalTargetProfile.MasslessVector()],
            MinimumClaimClass = BosonClaimClass.C0_NumericalMode,
        };

        var runner = new CampaignRunner();
        var result = runner.Run(registry, campaign, MakeProvenance());

        Assert.Equal(1, result.InsufficientEvidenceCount);
        Assert.Equal(ComparisonOutcome.InsufficientEvidence, result.Comparisons[0].Outcome);
    }

    [Fact]
    public void CampaignRunner_Underdetermined_HighGaugeLeak()
    {
        var registry = new BosonRegistry();
        // Good mass and multiplicity match but high gauge leak
        registry.Register(MakeCandidate("cand-4", massMean: 0.0, multMean: 2, gaugeLeak: 0.5));

        var campaign = new BosonComparisonCampaign
        {
            CampaignId = "test-undetermined",
            TargetProfiles = [InternalTargetProfile.MasslessVector()],
            MinimumClaimClass = BosonClaimClass.C0_NumericalMode,
        };

        var runner = new CampaignRunner();
        var result = runner.Run(registry, campaign, MakeProvenance());

        Assert.Equal(1, result.UnderdeterminedCount);
        Assert.Equal(ComparisonOutcome.Underdetermined, result.Comparisons[0].Outcome);
    }

    [Fact]
    public void CampaignRunner_NeverForcesUniqueMatch()
    {
        var registry = new BosonRegistry();
        // Two candidates both compatible with the same target
        registry.Register(MakeCandidate("cand-a", massMean: 0.0, multMean: 2, gaugeLeak: 0.01));
        registry.Register(MakeCandidate("cand-b", massMean: 0.0, multMean: 2, gaugeLeak: 0.02));

        var campaign = new BosonComparisonCampaign
        {
            CampaignId = "test-no-unique",
            TargetProfiles = [InternalTargetProfile.MasslessVector()],
            MinimumClaimClass = BosonClaimClass.C0_NumericalMode,
        };

        var runner = new CampaignRunner();
        var result = runner.Run(registry, campaign, MakeProvenance());

        // Both should be compatible -- no forced unique match
        Assert.Equal(2, result.CompatibleCount);
        Assert.All(result.Comparisons, c => Assert.Equal(ComparisonOutcome.Compatible, c.Outcome));
    }

    [Fact]
    public void CampaignRunner_EmptyRegistry_ProducesEmptyResults()
    {
        var registry = new BosonRegistry();

        var campaign = new BosonComparisonCampaign
        {
            CampaignId = "test-empty",
            TargetProfiles = [InternalTargetProfile.MasslessVector()],
            MinimumClaimClass = BosonClaimClass.C0_NumericalMode,
        };

        var runner = new CampaignRunner();
        var result = runner.Run(registry, campaign, MakeProvenance());

        Assert.Equal(0, result.CandidatesEvaluated);
        Assert.Empty(result.Comparisons);
        Assert.Equal(0, result.CompatibleCount);
    }

    [Fact]
    public void Campaign_SerializationRoundtrip()
    {
        var registry = new BosonRegistry();
        registry.Register(MakeCandidate("cand-rt", massMean: 1.0, multMean: 1, gaugeLeak: 0.05));

        var campaign = new BosonComparisonCampaign
        {
            CampaignId = "test-roundtrip",
            TargetProfiles = [InternalTargetProfile.MassiveScalar()],
            MinimumClaimClass = BosonClaimClass.C0_NumericalMode,
        };

        var runner = new CampaignRunner();
        var result = runner.Run(registry, campaign, MakeProvenance());

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() },
        };

        string json = JsonSerializer.Serialize(result, options);
        var deserialized = JsonSerializer.Deserialize<CampaignResult>(json, options);

        Assert.NotNull(deserialized);
        Assert.Equal(result.CampaignId, deserialized.CampaignId);
        Assert.Equal(result.Comparisons.Count, deserialized.Comparisons.Count);
        Assert.Equal(result.CandidatesEvaluated, deserialized.CandidatesEvaluated);
    }

    [Fact]
    public void CampaignRunner_ClaimClassFilter_ExcludesLowCandidates()
    {
        var registry = new BosonRegistry();
        registry.Register(MakeCandidate("cand-low", massMean: 0.0, multMean: 2, gaugeLeak: 0.01,
            claimClass: BosonClaimClass.C0_NumericalMode));
        registry.Register(MakeCandidate("cand-high", massMean: 0.0, multMean: 2, gaugeLeak: 0.01,
            claimClass: BosonClaimClass.C3_ObservedStableCandidate));

        var campaign = new BosonComparisonCampaign
        {
            CampaignId = "test-filter",
            TargetProfiles = [InternalTargetProfile.MasslessVector()],
            MinimumClaimClass = BosonClaimClass.C2_BranchStableBosonicCandidate,
        };

        var runner = new CampaignRunner();
        var result = runner.Run(registry, campaign, MakeProvenance());

        Assert.Equal(1, result.CandidatesEvaluated);
        Assert.Single(result.Comparisons);
        Assert.Equal("cand-high", result.Comparisons[0].CandidateId);
    }
}
