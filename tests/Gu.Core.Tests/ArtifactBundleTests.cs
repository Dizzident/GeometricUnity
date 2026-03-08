using Gu.Core;
using Gu.Core.Factories;
using Gu.Core.Serialization;

namespace Gu.Core.Tests;

public class ArtifactBundleTests
{
    private static BranchManifest SampleManifest() => BranchManifestFactory.CreateEmpty("test-branch");

    private static ArtifactBundle CreateSample()
    {
        var branch = new BranchRef { BranchId = "test-branch", SchemaVersion = "1.0.0" };
        var now = DateTimeOffset.UtcNow;

        return new ArtifactBundle
        {
            ArtifactId = "artifact-001",
            Branch = branch,
            ReplayContract = new ReplayContract
            {
                BranchManifest = SampleManifest(),
                Deterministic = true,
                RandomSeed = 42,
                BackendId = "cpu-reference",
                ReplayTier = "R2",
            },
            ValidationBundle = new ValidationBundle
            {
                Branch = branch,
                Records = new[]
                {
                    new ValidationRecord
                    {
                        RuleId = "carrier-match",
                        Category = "well-definedness",
                        Passed = true,
                        Timestamp = now
                    }
                },
                AllPassed = true
            },
            Integrity = new IntegrityBundle
            {
                ContentHash = "sha256:abc123def456",
                ComputedAt = now
            },
            Provenance = new ProvenanceMeta
            {
                CreatedAt = now,
                CodeRevision = "abc123",
                Branch = branch,
                Backend = "cpu-reference"
            },
            CreatedAt = now
        };
    }

    [Fact]
    public void RoundTrip_AllFieldsPreserved()
    {
        var original = CreateSample();
        var json = GuJsonDefaults.Serialize(original);
        var deserialized = GuJsonDefaults.Deserialize<ArtifactBundle>(json);

        Assert.NotNull(deserialized);
        Assert.Equal(original.ArtifactId, deserialized.ArtifactId);
        Assert.Equal(original.Branch.BranchId, deserialized.Branch.BranchId);
        Assert.NotNull(deserialized.ReplayContract);
        Assert.True(deserialized.ReplayContract.Deterministic);
        Assert.Equal(42L, deserialized.ReplayContract.RandomSeed);
        Assert.NotNull(deserialized.ValidationBundle);
        Assert.True(deserialized.ValidationBundle.AllPassed);
        Assert.NotNull(deserialized.Integrity);
        Assert.Equal("SHA-256", deserialized.Integrity.HashAlgorithm);
    }

    [Fact]
    public void ReplayContract_ContainsBranchManifest()
    {
        // Per Section 20.1: artifact packages must persist branch manifest
        var artifact = CreateSample();
        Assert.NotNull(artifact.ReplayContract.BranchManifest);
        Assert.Equal("test-branch", artifact.ReplayContract.BranchManifest.BranchId);
    }

    [Fact]
    public void ObservedState_IsOptional()
    {
        var artifact = CreateSample();
        Assert.Null(artifact.ObservedState);
    }

    [Fact]
    public void IntegrityBundle_DefaultsToSHA256()
    {
        var integrity = new IntegrityBundle
        {
            ContentHash = "abc123",
            ComputedAt = DateTimeOffset.UtcNow
        };
        Assert.Equal("SHA-256", integrity.HashAlgorithm);
    }
}
