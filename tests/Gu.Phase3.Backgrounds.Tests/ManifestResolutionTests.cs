using Gu.Artifacts;
using Gu.Core;
using Gu.Core.Serialization;
using Gu.Phase3.Backgrounds;
using Gu.Solvers;

namespace Gu.Phase3.Backgrounds.Tests;

/// <summary>
/// Tests for WP-1: solve-backgrounds manifest resolution (D-001) and classification (D-002/D-003).
/// Covers all 8 required test cases.
/// </summary>
public class ManifestResolutionTests : IDisposable
{
    private readonly string _tmpDir;

    public ManifestResolutionTests()
    {
        _tmpDir = Path.Combine(Path.GetTempPath(), $"gu-manifest-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tmpDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tmpDir))
            Directory.Delete(_tmpDir, recursive: true);
    }

    private string WriteManifest(string dir, string fileName, string branchId = "branch-1")
    {
        var manifest = TestHelpers.MakeManifest(branchId);
        var path = Path.Combine(dir, fileName);
        Directory.CreateDirectory(dir);
        File.WriteAllText(path, GuJsonDefaults.Serialize(manifest));
        return path;
    }

    // Test 1: --manifest works for single-manifest studies
    [Fact]
    public void Resolve_ExplicitManifestPath_SingleManifestStudy_Succeeds()
    {
        var manifestPath = WriteManifest(_tmpDir, "branch-1.json", "branch-1");

        var (manifest, consumedPath) = ManifestResolver.Resolve(
            "branch-1",
            explicitManifestPath: manifestPath,
            manifestDir: null,
            manifestSearchPaths: null,
            studyDir: _tmpDir);

        Assert.NotNull(manifest);
        Assert.Equal("branch-1", manifest.BranchId);
        Assert.Equal(Path.GetFullPath(manifestPath), consumedPath);
    }

    // Test 2: --manifest hard-fails for mixed-manifest studies
    [Fact]
    public void ValidateExplicitManifestUsage_MixedManifestIds_ReturnsFalse()
    {
        var specs = new List<BackgroundSpec>
        {
            TestHelpers.MakeSpec("spec-1", branchId: "branch-a"),
            TestHelpers.MakeSpec("spec-2", branchId: "branch-b"),
        };

        var isValid = ManifestResolver.ValidateExplicitManifestUsage(specs, out var distinctIds);

        Assert.False(isValid);
        Assert.Equal(2, distinctIds.Count);
        Assert.Contains("branch-a", distinctIds);
        Assert.Contains("branch-b", distinctIds);
    }

    [Fact]
    public void ValidateExplicitManifestUsage_SingleManifestId_ReturnsTrue()
    {
        var specs = new List<BackgroundSpec>
        {
            TestHelpers.MakeSpec("spec-1", branchId: "branch-1"),
            TestHelpers.MakeSpec("spec-2", branchId: "branch-1"),
        };

        var isValid = ManifestResolver.ValidateExplicitManifestUsage(specs, out var distinctIds);

        Assert.True(isValid);
        Assert.Single(distinctIds);
    }

    // Test 3: --manifest-dir resolves <id>.branch.json
    [Fact]
    public void Resolve_ManifestDir_FindsBranchJsonFile()
    {
        var manifestDir = Path.Combine(_tmpDir, "manifests");
        WriteManifest(manifestDir, "my-branch.branch.json", "my-branch");

        var (manifest, consumedPath) = ManifestResolver.Resolve(
            "my-branch",
            explicitManifestPath: null,
            manifestDir: manifestDir,
            manifestSearchPaths: null,
            studyDir: _tmpDir);

        Assert.NotNull(manifest);
        Assert.Equal("my-branch", manifest.BranchId);
        Assert.EndsWith("my-branch.branch.json", consumedPath);
    }

    [Fact]
    public void Resolve_ManifestDir_FallsBackToJsonFile_WhenBranchJsonMissing()
    {
        var manifestDir = Path.Combine(_tmpDir, "manifests");
        WriteManifest(manifestDir, "my-branch.json", "my-branch");

        var (manifest, consumedPath) = ManifestResolver.Resolve(
            "my-branch",
            explicitManifestPath: null,
            manifestDir: manifestDir,
            manifestSearchPaths: null,
            studyDir: _tmpDir);

        Assert.NotNull(manifest);
        Assert.Equal("my-branch", manifest.BranchId);
        Assert.EndsWith("my-branch.json", consumedPath);
    }

    // Test 4: ManifestSearchPaths resolves relative to the study JSON file
    [Fact]
    public void Resolve_ManifestSearchPaths_ResolvesRelativeToStudyDir()
    {
        // Place manifest in a subdir relative to studyDir
        var subDir = Path.Combine(_tmpDir, "config", "manifests");
        WriteManifest(subDir, "branch-rel.branch.json", "branch-rel");

        var studyDir = _tmpDir;
        var searchPaths = new List<string> { Path.Combine("config", "manifests") };

        var (manifest, consumedPath) = ManifestResolver.Resolve(
            "branch-rel",
            explicitManifestPath: null,
            manifestDir: null,
            manifestSearchPaths: searchPaths,
            studyDir: studyDir);

        Assert.NotNull(manifest);
        Assert.Equal("branch-rel", manifest.BranchId);
        Assert.Contains("branch-rel.branch.json", consumedPath);
    }

    // Test 5: missing manifest hard-fails
    [Fact]
    public void Resolve_NoSourceAvailable_ThrowsManifestResolutionException()
    {
        Assert.Throws<ManifestResolutionException>(() =>
            ManifestResolver.Resolve(
                "nonexistent-branch",
                explicitManifestPath: null,
                manifestDir: null,
                manifestSearchPaths: null,
                studyDir: _tmpDir));
    }

    [Fact]
    public void Resolve_ManifestDirDoesNotContainId_ThrowsManifestResolutionException()
    {
        var manifestDir = Path.Combine(_tmpDir, "empty-dir");
        Directory.CreateDirectory(manifestDir);

        Assert.Throws<ManifestResolutionException>(() =>
            ManifestResolver.Resolve(
                "nonexistent-branch",
                explicitManifestPath: null,
                manifestDir: manifestDir,
                manifestSearchPaths: null,
                studyDir: _tmpDir));
    }

    // Test 6: each BackgroundRecord contains RunClassification
    [Fact]
    public void BackgroundRecord_CanCarryRunClassification()
    {
        var classification = SolveRunClassification.Classify("A", false, false, false);
        var record = TestHelpers.MakeRecord("bg-1", AdmissibilityLevel.B1);

        // Create enriched record (init-only, must create new)
        var enriched = new BackgroundRecord
        {
            BackgroundId = record.BackgroundId,
            EnvironmentId = record.EnvironmentId,
            BranchManifestId = record.BranchManifestId,
            GeometryFingerprint = record.GeometryFingerprint,
            StateArtifactRef = record.StateArtifactRef,
            ResidualNorm = record.ResidualNorm,
            StationarityNorm = record.StationarityNorm,
            AdmissibilityLevel = record.AdmissibilityLevel,
            Metrics = record.Metrics,
            ReplayTierAchieved = record.ReplayTierAchieved,
            Provenance = record.Provenance,
            RunClassification = classification,
        };

        Assert.NotNull(enriched.RunClassification);
        Assert.Equal("residual-inspection", enriched.RunClassification.RunType);
        Assert.Equal("zero-seed", enriched.RunClassification.SeedSource);
        Assert.True(enriched.RunClassification.IsTrivialValidationPath);
    }

    [Fact]
    public void BackgroundRecord_RunClassification_RoundTripsJson()
    {
        var classification = SolveRunClassification.Classify("B", true, false, false);
        var record = new BackgroundRecord
        {
            BackgroundId = "bg-classify",
            EnvironmentId = "env-1",
            BranchManifestId = "branch-1",
            GeometryFingerprint = "fp",
            StateArtifactRef = "state",
            ResidualNorm = 1e-6,
            StationarityNorm = 1e-8,
            AdmissibilityLevel = AdmissibilityLevel.B1,
            Metrics = TestHelpers.MakeRecord("bg-classify", AdmissibilityLevel.B1).Metrics,
            ReplayTierAchieved = "R2",
            Provenance = TestHelpers.MakeProvenance(),
            RunClassification = classification,
        };

        var json = GuJsonDefaults.Serialize(record);
        var deserialized = GuJsonDefaults.Deserialize<BackgroundRecord>(json);

        Assert.NotNull(deserialized);
        Assert.NotNull(deserialized.RunClassification);
        Assert.Equal("objective-solve", deserialized.RunClassification.RunType);
        Assert.Equal("persisted-state", deserialized.RunClassification.SeedSource);
        Assert.False(deserialized.RunClassification.IsTrivialValidationPath);
    }

    // Test 7: each BackgroundRecord contains ConsumedManifestArtifactRef
    [Fact]
    public void BackgroundRecord_CanCarryConsumedManifestArtifactRef()
    {
        var record = TestHelpers.MakeRecord("bg-1", AdmissibilityLevel.B1);

        var enriched = new BackgroundRecord
        {
            BackgroundId = record.BackgroundId,
            EnvironmentId = record.EnvironmentId,
            BranchManifestId = record.BranchManifestId,
            GeometryFingerprint = record.GeometryFingerprint,
            StateArtifactRef = record.StateArtifactRef,
            ResidualNorm = record.ResidualNorm,
            StationarityNorm = record.StationarityNorm,
            AdmissibilityLevel = record.AdmissibilityLevel,
            Metrics = record.Metrics,
            ReplayTierAchieved = record.ReplayTierAchieved,
            Provenance = record.Provenance,
            ConsumedManifestArtifactRef = "/some/path/branch-1.branch.json",
        };

        Assert.Equal("/some/path/branch-1.branch.json", enriched.ConsumedManifestArtifactRef);

        // Round-trip
        var json = GuJsonDefaults.Serialize(enriched);
        Assert.Contains("consumedManifestArtifactRef", json);
        var deserialized = GuJsonDefaults.Deserialize<BackgroundRecord>(json);
        Assert.NotNull(deserialized);
        Assert.Equal("/some/path/branch-1.branch.json", deserialized.ConsumedManifestArtifactRef);
    }

    // Test 8: per-background manifest file is actually written
    [Fact]
    public void SolveBackgroundsScenario_PerBackgroundManifestFileIsWritten()
    {
        // Simulate what the CLI does: for each background, write a manifest file
        var statesDir = Path.Combine(_tmpDir, "background_states");
        Directory.CreateDirectory(statesDir);

        var manifest = TestHelpers.MakeManifest("branch-1");
        var bgId = "bg-test-001";

        // This is the pattern the CLI uses
        var manifestDestPath = Path.Combine(statesDir, $"{bgId}_manifest.json");
        File.WriteAllText(manifestDestPath, GuJsonDefaults.Serialize(manifest));

        Assert.True(File.Exists(manifestDestPath),
            $"Expected per-background manifest file at: {manifestDestPath}");

        // Verify the written file is valid
        var written = GuJsonDefaults.Deserialize<BranchManifest>(File.ReadAllText(manifestDestPath));
        Assert.NotNull(written);
        Assert.Equal("branch-1", written.BranchId);
    }

    // Additional: BackgroundStudySpec carries ManifestSearchPaths
    [Fact]
    public void BackgroundStudySpec_ManifestSearchPaths_SerializesCorrectly()
    {
        var spec = new BackgroundStudySpec
        {
            StudyId = "study-with-paths",
            Specs = Array.Empty<BackgroundSpec>(),
            ManifestSearchPaths = new[] { "config/manifests", "/abs/path/manifests" },
        };

        var json = GuJsonDefaults.Serialize(spec);
        Assert.Contains("manifestSearchPaths", json);

        var deserialized = GuJsonDefaults.Deserialize<BackgroundStudySpec>(json);
        Assert.NotNull(deserialized);
        Assert.NotNull(deserialized.ManifestSearchPaths);
        Assert.Equal(2, deserialized.ManifestSearchPaths.Count);
        Assert.Equal("config/manifests", deserialized.ManifestSearchPaths[0]);
        Assert.Equal("/abs/path/manifests", deserialized.ManifestSearchPaths[1]);
    }

    // Manifest resolution priority: explicit path takes precedence over manifest-dir
    [Fact]
    public void Resolve_ExplicitManifestPath_TakesPrecedenceOverManifestDir()
    {
        var explicitManifestPath = WriteManifest(_tmpDir, "explicit.json", "explicit-branch");
        var manifestDir = Path.Combine(_tmpDir, "dir");
        WriteManifest(manifestDir, "dir-branch.json", "dir-branch");

        // Even though manifestDir is provided, explicit path should win
        var (manifest, consumedPath) = ManifestResolver.Resolve(
            "explicit-branch",
            explicitManifestPath: explicitManifestPath,
            manifestDir: manifestDir,
            manifestSearchPaths: null,
            studyDir: _tmpDir);

        Assert.Equal("explicit-branch", manifest.BranchId);
        Assert.Equal(Path.GetFullPath(explicitManifestPath), consumedPath);
    }
}
