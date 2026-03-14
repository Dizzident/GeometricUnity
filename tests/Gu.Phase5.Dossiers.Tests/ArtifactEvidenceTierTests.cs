using Gu.Artifacts;

namespace Gu.Phase5.Dossiers.Tests;

public class ArtifactEvidenceTierTests
{
    [Fact]
    public void StaleCheckedIn_IsNotAcceptableAsEvidence()
    {
        Assert.False(ArtifactEvidenceTier.StaleCheckedIn.IsAcceptableAsEvidence());
        Assert.False(ArtifactEvidenceTier.StaleCheckedIn.IsRegeneratedFromCurrentCode());
    }

    [Theory]
    [InlineData(ArtifactEvidenceTier.RegeneratedCpu)]
    [InlineData(ArtifactEvidenceTier.RegeneratedVerifiedCpu)]
    [InlineData(ArtifactEvidenceTier.CrossBackendVerified)]
    public void RegeneratedTiers_AreAcceptableAsEvidence(ArtifactEvidenceTier tier)
    {
        Assert.True(tier.IsAcceptableAsEvidence());
        Assert.True(tier.IsRegeneratedFromCurrentCode());
    }

    [Fact]
    public void TierLabels_AreCorrect()
    {
        Assert.Equal("stale-checked-in",         ArtifactEvidenceTier.StaleCheckedIn.Label());
        Assert.Equal("regenerated-cpu",           ArtifactEvidenceTier.RegeneratedCpu.Label());
        Assert.Equal("regenerated-verified-cpu",  ArtifactEvidenceTier.RegeneratedVerifiedCpu.Label());
        Assert.Equal("cross-backend-verified",    ArtifactEvidenceTier.CrossBackendVerified.Label());
    }

    [Fact]
    public void ReproducibilityBundle_CreateStale_HasStaleCheckedInTier()
    {
        var bundle = ReproducibilityBundle.CreateStale();
        Assert.Equal(ArtifactEvidenceTier.StaleCheckedIn, bundle.EvidenceTier);
        Assert.Equal("unknown", bundle.CodeRevision);
        Assert.Empty(bundle.ReproductionCommands);
        Assert.Equal(DateTimeOffset.MinValue, bundle.RegeneratedAt);
    }

    [Fact]
    public void ReproducibilityBundle_CreateRegeneratedCpu_HasRegeneratedTier()
    {
        var commands = new[] { "dotnet run -- solve-backgrounds study.json" };
        var bundle = ReproducibilityBundle.CreateRegeneratedCpu("abc123", commands);
        Assert.Equal(ArtifactEvidenceTier.RegeneratedCpu, bundle.EvidenceTier);
        Assert.Equal("abc123", bundle.CodeRevision);
        Assert.Single(bundle.ReproductionCommands);
        Assert.True(bundle.RegeneratedAt > DateTimeOffset.MinValue);
    }
}
