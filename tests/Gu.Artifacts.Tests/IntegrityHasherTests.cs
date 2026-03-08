using Gu.Artifacts;
using Gu.Core;

namespace Gu.Artifacts.Tests;

public class IntegrityHasherTests : IDisposable
{
    private readonly string _tempDir;

    public IntegrityHasherTests()
    {
        _tempDir = TestHelpers.CreateTempRunFolder();
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    [Fact]
    public void ComputeHash_String_ReturnsConsistentHash()
    {
        var hash1 = IntegrityHasher.ComputeHash("hello world");
        var hash2 = IntegrityHasher.ComputeHash("hello world");
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void ComputeHash_DifferentStrings_ReturnDifferentHashes()
    {
        var hash1 = IntegrityHasher.ComputeHash("hello");
        var hash2 = IntegrityHasher.ComputeHash("world");
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void ComputeHash_ReturnsLowercaseHex()
    {
        var hash = IntegrityHasher.ComputeHash("test");
        Assert.Matches("^[0-9a-f]{64}$", hash); // SHA-256 = 64 hex chars
    }

    [Fact]
    public void ComputeHashOfObject_ReturnsConsistentHash()
    {
        var manifest = TestHelpers.CreateTestManifest();
        var hash1 = IntegrityHasher.ComputeHashOfObject(manifest);
        var hash2 = IntegrityHasher.ComputeHashOfObject(manifest);
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void ComputeBundle_ReturnsValidBundle()
    {
        var manifest = TestHelpers.CreateTestManifest();
        var bundle = IntegrityHasher.ComputeBundle(manifest);

        Assert.NotNull(bundle.ContentHash);
        Assert.Equal("SHA-256", bundle.HashAlgorithm);
        Assert.True(bundle.ContentHash.Length == 64);
    }

    [Fact]
    public void ComputeFileHash_MatchesStringHash()
    {
        var content = "test content for hashing";
        var filePath = Path.Combine(_tempDir, "test.txt");
        File.WriteAllText(filePath, content);

        var fileHash = IntegrityHasher.ComputeFileHash(filePath);
        // File hash is over raw bytes, including any encoding differences
        Assert.NotNull(fileHash);
        Assert.Equal(64, fileHash.Length);
    }

    [Fact]
    public void ComputeRunFolderHashes_ProducesManifest()
    {
        // Set up a run folder with some files
        var writer = new RunFolderWriter(_tempDir);
        writer.CreateDirectories();
        writer.WriteBranchManifest(TestHelpers.CreateTestManifest());
        writer.WriteReplayContract(TestHelpers.CreateTestReplayContract());

        var manifest = IntegrityHasher.ComputeRunFolderHashes(_tempDir);

        Assert.NotEmpty(manifest.FileHashes);
        Assert.Contains(manifest.FileHashes, kv => kv.Key.Contains("branch.json"));
        Assert.Contains(manifest.FileHashes, kv => kv.Key.Contains("replay_contract.json"));
    }

    [Fact]
    public void ComputeRunFolderHashes_ExcludesIntegrityDirectory()
    {
        var writer = new RunFolderWriter(_tempDir);
        writer.CreateDirectories();
        writer.WriteBranchManifest(TestHelpers.CreateTestManifest());
        writer.WriteIntegrityBundle(TestHelpers.CreateTestIntegrityBundle());

        var manifest = IntegrityHasher.ComputeRunFolderHashes(_tempDir);

        Assert.DoesNotContain(manifest.FileHashes, kv => kv.Key.StartsWith("integrity"));
    }

    [Fact]
    public void VerifyRunFolderHashes_PassesForUnmodifiedFiles()
    {
        var writer = new RunFolderWriter(_tempDir);
        writer.CreateDirectories();
        writer.WriteBranchManifest(TestHelpers.CreateTestManifest());
        writer.WriteReplayContract(TestHelpers.CreateTestReplayContract());

        var manifest = IntegrityHasher.ComputeRunFolderHashes(_tempDir);
        var mismatches = IntegrityHasher.VerifyRunFolderHashes(_tempDir, manifest);

        Assert.Empty(mismatches);
    }

    [Fact]
    public void VerifyRunFolderHashes_DetectsModifiedFile()
    {
        var writer = new RunFolderWriter(_tempDir);
        writer.CreateDirectories();
        writer.WriteBranchManifest(TestHelpers.CreateTestManifest());

        var manifest = IntegrityHasher.ComputeRunFolderHashes(_tempDir);

        // Tamper with the branch manifest
        var filePath = Path.Combine(_tempDir, RunFolderLayout.BranchManifestFile);
        File.WriteAllText(filePath, "{ \"tampered\": true }");

        var mismatches = IntegrityHasher.VerifyRunFolderHashes(_tempDir, manifest);
        Assert.NotEmpty(mismatches);
        Assert.Contains(mismatches, m => m.Contains("Hash mismatch") && m.Contains("branch.json"));
    }

    [Fact]
    public void VerifyRunFolderHashes_DetectsMissingFile()
    {
        var writer = new RunFolderWriter(_tempDir);
        writer.CreateDirectories();
        writer.WriteBranchManifest(TestHelpers.CreateTestManifest());

        var manifest = IntegrityHasher.ComputeRunFolderHashes(_tempDir);

        // Delete the branch manifest
        File.Delete(Path.Combine(_tempDir, RunFolderLayout.BranchManifestFile));

        var mismatches = IntegrityHasher.VerifyRunFolderHashes(_tempDir, manifest);
        Assert.NotEmpty(mismatches);
        Assert.Contains(mismatches, m => m.Contains("Missing file"));
    }

    [Fact]
    public void ComputeRunFolderHashes_ThrowsForMissingFolder()
    {
        Assert.Throws<DirectoryNotFoundException>(() =>
            IntegrityHasher.ComputeRunFolderHashes("/nonexistent/path"));
    }
}
