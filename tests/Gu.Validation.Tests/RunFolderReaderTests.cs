using Gu.Artifacts;
using Gu.Core;
using Gu.Core.Serialization;

namespace Gu.Validation.Tests;

public class RunFolderReaderTests : IDisposable
{
    private readonly string _tempDir;

    public RunFolderReaderTests()
    {
        _tempDir = TestHelpers.CreateTempRunFolder();
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    private RunFolderWriter CreatePopulatedFolder(string? path = null)
    {
        var dir = path ?? _tempDir;
        var writer = new RunFolderWriter(dir);
        writer.CreateDirectories();
        writer.WriteBranchManifest(TestHelpers.CreateValidManifest());
        writer.WriteReplayContract(TestHelpers.CreateTestReplayContract());
        writer.WriteValidationBundle(TestHelpers.CreateTestValidationBundle());
        writer.WriteIntegrityBundle(TestHelpers.CreateTestIntegrityBundle());
        writer.WritePackageRoot("artifact-001");
        return writer;
    }

    [Fact]
    public void ReadBranchManifest_RoundTrips()
    {
        CreatePopulatedFolder();
        var reader = new RunFolderReader(_tempDir);

        var manifest = reader.ReadBranchManifest();

        Assert.NotNull(manifest);
        Assert.Equal("test-branch", manifest.BranchId);
        Assert.Equal(4, manifest.BaseDimension);
        Assert.Equal(14, manifest.AmbientDimension);
    }

    [Fact]
    public void ReadReplayContract_RoundTrips()
    {
        CreatePopulatedFolder();
        var reader = new RunFolderReader(_tempDir);

        var contract = reader.ReadReplayContract();

        Assert.NotNull(contract);
        Assert.Equal("cpu-reference", contract.BackendId);
        Assert.True(contract.Deterministic);
        Assert.Equal("R2", contract.ReplayTier);
    }

    [Fact]
    public void ReadValidationBundle_RoundTrips()
    {
        CreatePopulatedFolder();
        var reader = new RunFolderReader(_tempDir);

        var bundle = reader.ReadValidationBundle();

        Assert.NotNull(bundle);
        Assert.True(bundle.AllPassed);
        Assert.Equal(2, bundle.Records.Count);
    }

    [Fact]
    public void ReadIntegrityBundle_RoundTrips()
    {
        CreatePopulatedFolder();
        var reader = new RunFolderReader(_tempDir);

        var integrity = reader.ReadIntegrityBundle();

        Assert.NotNull(integrity);
        Assert.Equal("abc123def456", integrity.ContentHash);
        Assert.Equal("SHA-256", integrity.HashAlgorithm);
    }

    [Fact]
    public void ReadArtifactId_RoundTrips()
    {
        CreatePopulatedFolder();
        var reader = new RunFolderReader(_tempDir);

        var artifactId = reader.ReadArtifactId();

        Assert.Equal("artifact-001", artifactId);
    }

    [Fact]
    public void ReadArtifactBundle_RoundTrips()
    {
        CreatePopulatedFolder();
        var reader = new RunFolderReader(_tempDir);

        var bundle = reader.ReadArtifactBundle();

        Assert.NotNull(bundle);
        Assert.Equal("artifact-001", bundle.ArtifactId);
        Assert.Equal("test-branch", bundle.Branch.BranchId);
        Assert.NotNull(bundle.ReplayContract);
        Assert.NotNull(bundle.ValidationBundle);
        Assert.NotNull(bundle.Integrity);
    }

    [Fact]
    public void ReadArtifactBundle_MissingContract_ReturnsNull()
    {
        var writer = new RunFolderWriter(_tempDir);
        writer.CreateDirectories();
        writer.WriteBranchManifest(TestHelpers.CreateValidManifest());
        writer.WritePackageRoot("artifact-001");
        // No replay contract written

        var reader = new RunFolderReader(_tempDir);
        var bundle = reader.ReadArtifactBundle();

        Assert.Null(bundle);
    }

    [Fact]
    public void ReadJson_MissingFile_ReturnsNull()
    {
        var reader = new RunFolderReader(_tempDir);
        var result = reader.ReadJson<BranchManifest>("nonexistent.json");
        Assert.Null(result);
    }

    [Fact]
    public void HasValidStructure_ReturnsFalse_WhenEmpty()
    {
        var reader = new RunFolderReader(_tempDir);
        Assert.False(reader.HasValidStructure());
    }

    [Fact]
    public void HasValidStructure_ReturnsTrue_WhenPopulated()
    {
        CreatePopulatedFolder();
        var reader = new RunFolderReader(_tempDir);
        Assert.True(reader.HasValidStructure());
    }

    [Fact]
    public void WriteAndRead_CompleteArtifactBundle_RoundTrips()
    {
        // Full round-trip: write an ArtifactBundle, read it back
        var original = TestHelpers.CreateTestArtifactBundle();
        var writer = new RunFolderWriter(_tempDir);
        writer.WriteArtifactBundle(original);

        var reader = new RunFolderReader(_tempDir);
        var loaded = reader.ReadArtifactBundle();

        Assert.NotNull(loaded);
        Assert.Equal(original.ArtifactId, loaded.ArtifactId);
        Assert.Equal(original.Branch.BranchId, loaded.Branch.BranchId);
        Assert.Equal(original.ReplayContract.BackendId, loaded.ReplayContract.BackendId);
        Assert.Equal(original.ValidationBundle!.AllPassed, loaded.ValidationBundle!.AllPassed);
    }

    [Fact]
    public void VerifyIntegrity_NoHashFile_ReportsError()
    {
        var writer = new RunFolderWriter(_tempDir);
        writer.CreateDirectories();
        writer.WriteBranchManifest(TestHelpers.CreateValidManifest());
        // No integrity file written

        var reader = new RunFolderReader(_tempDir);
        var mismatches = reader.VerifyIntegrity();

        Assert.NotEmpty(mismatches);
        Assert.Contains(mismatches, m => m.Contains("No integrity hash file"));
    }

    [Fact]
    public void VerifyIntegrity_ValidHashes_Pass()
    {
        var writer = new RunFolderWriter(_tempDir);
        writer.CreateDirectories();
        writer.WriteBranchManifest(TestHelpers.CreateValidManifest());
        writer.WriteReplayContract(TestHelpers.CreateTestReplayContract());

        // Compute and write file-level hashes
        var hashManifest = IntegrityHasher.ComputeRunFolderHashes(_tempDir);
        var hashJson = GuJsonDefaults.Serialize(hashManifest);
        var hashPath = Path.Combine(_tempDir, RunFolderLayout.HashesFile);
        File.WriteAllText(hashPath, hashJson);

        var reader = new RunFolderReader(_tempDir);
        var mismatches = reader.VerifyIntegrity();

        Assert.Empty(mismatches);
    }

    [Fact]
    public void VerifyIntegrity_TamperedFile_DetectsChange()
    {
        var writer = new RunFolderWriter(_tempDir);
        writer.CreateDirectories();
        writer.WriteBranchManifest(TestHelpers.CreateValidManifest());
        writer.WriteReplayContract(TestHelpers.CreateTestReplayContract());

        // Compute and write file-level hashes
        var hashManifest = IntegrityHasher.ComputeRunFolderHashes(_tempDir);
        var hashJson = GuJsonDefaults.Serialize(hashManifest);
        var hashPath = Path.Combine(_tempDir, RunFolderLayout.HashesFile);
        File.WriteAllText(hashPath, hashJson);

        // Tamper with the branch manifest
        var manifestPath = Path.Combine(_tempDir, RunFolderLayout.BranchManifestFile);
        File.WriteAllText(manifestPath, "{ \"tampered\": true }");

        var reader = new RunFolderReader(_tempDir);
        var mismatches = reader.VerifyIntegrity();

        Assert.NotEmpty(mismatches);
        Assert.Contains(mismatches, m => m.Contains("Hash mismatch") && m.Contains("branch.json"));
    }

    [Fact]
    public void ReadObservedState_WhenWritten_RoundTrips()
    {
        var writer = new RunFolderWriter(_tempDir);
        writer.CreateDirectories();

        var observed = TestHelpers.CreateTestObservedState(new[] { 1.0, 2.0, 3.0 });
        writer.WriteObservedState(observed);

        var reader = new RunFolderReader(_tempDir);
        var loaded = reader.ReadObservedState();

        Assert.NotNull(loaded);
        Assert.Equal("sigma-pullback", loaded.ObservationBranchId);
        Assert.Single(loaded.Observables);
        Assert.Equal(new[] { 1.0, 2.0, 3.0 }, loaded.Observables["energy"].Values);
    }

    [Fact]
    public void ReadObservedState_WhenNotWritten_ReturnsNull()
    {
        var writer = new RunFolderWriter(_tempDir);
        writer.CreateDirectories();

        var reader = new RunFolderReader(_tempDir);
        var loaded = reader.ReadObservedState();

        Assert.Null(loaded);
    }
}
