using Gu.Phase2.Comparison;
using Gu.Phase2.Semantics;

namespace Gu.Phase2.Comparison.Tests;

public sealed class NegativeResultArtifactTests
{
    [Fact]
    public void Construction_AllFieldsSet()
    {
        var failure = new ComparisonFailureRecord
        {
            TestId = "test-001",
            FailureReason = "Deviation too large",
            FailureLevel = "empirical",
            FalsifiesRecord = true,
            BlocksCampaign = false,
            DemotedClaimClass = ClaimClass.PostdictionTarget,
        };

        var artifact = new NegativeResultArtifact
        {
            ArtifactId = "neg-campaign-001-test-001",
            CampaignId = "campaign-001",
            Failure = failure,
            OriginalTestId = "test-001",
            AttemptedMode = ComparisonMode.Quantitative,
            BranchManifestId = "manifest-001",
            IsFalsification = true,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        Assert.Equal("neg-campaign-001-test-001", artifact.ArtifactId);
        Assert.Equal("campaign-001", artifact.CampaignId);
        Assert.Equal("test-001", artifact.OriginalTestId);
        Assert.Equal(ComparisonMode.Quantitative, artifact.AttemptedMode);
        Assert.Equal("manifest-001", artifact.BranchManifestId);
        Assert.True(artifact.IsFalsification);
        Assert.NotNull(artifact.Failure);
        Assert.Equal("Deviation too large", artifact.Failure.FailureReason);
    }

    [Fact]
    public void NonFalsification_Construction()
    {
        var failure = new ComparisonFailureRecord
        {
            TestId = "test-002",
            FailureReason = "Missing data",
            FailureLevel = "extraction",
            FalsifiesRecord = false,
            BlocksCampaign = false,
            DemotedClaimClass = ClaimClass.ApproximateStructuralSurrogate,
        };

        var artifact = new NegativeResultArtifact
        {
            ArtifactId = "neg-campaign-001-test-002",
            CampaignId = "campaign-001",
            Failure = failure,
            OriginalTestId = "test-002",
            AttemptedMode = ComparisonMode.Structural,
            BranchManifestId = "manifest-002",
            IsFalsification = false,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        Assert.False(artifact.IsFalsification);
        Assert.False(artifact.Failure.FalsifiesRecord);
        Assert.Equal(ComparisonMode.Structural, artifact.AttemptedMode);
    }

    [Fact]
    public void FailureLevels_AllValid()
    {
        foreach (var level in new[] { "numerical", "branch-local", "extraction", "empirical" })
        {
            var failure = new ComparisonFailureRecord
            {
                TestId = "test-001",
                FailureReason = $"Failure at {level} level",
                FailureLevel = level,
                FalsifiesRecord = false,
                BlocksCampaign = false,
                DemotedClaimClass = ClaimClass.Inadmissible,
            };

            Assert.Equal(level, failure.FailureLevel);
        }
    }
}
