using Gu.Artifacts;
using Gu.Core;
using Gu.Core.Serialization;

namespace Gu.Artifacts.Tests;

public class RunFolderWriterTests : IDisposable
{
    private readonly string _tempDir;

    public RunFolderWriterTests()
    {
        _tempDir = TestHelpers.CreateTempRunFolder();
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    [Fact]
    public void CreateDirectories_CreatesCanonicalStructure()
    {
        var writer = new RunFolderWriter(_tempDir);
        writer.CreateDirectories();

        foreach (var dir in RunFolderLayout.RequiredDirectories)
        {
            Assert.True(Directory.Exists(Path.Combine(_tempDir, dir)), $"Missing directory: {dir}");
        }
    }

    [Fact]
    public void HasValidStructure_ReturnsFalse_WhenEmpty()
    {
        var writer = new RunFolderWriter(_tempDir);
        Assert.False(writer.HasValidStructure());
    }

    [Fact]
    public void HasValidStructure_ReturnsTrue_AfterCreateDirectories()
    {
        var writer = new RunFolderWriter(_tempDir);
        writer.CreateDirectories();
        Assert.True(writer.HasValidStructure());
    }

    [Fact]
    public void WriteBranchManifest_CreatesFile()
    {
        var writer = new RunFolderWriter(_tempDir);
        writer.CreateDirectories();

        var manifest = TestHelpers.CreateTestManifest();
        writer.WriteBranchManifest(manifest);

        var filePath = Path.Combine(_tempDir, RunFolderLayout.BranchManifestFile);
        Assert.True(File.Exists(filePath));

        var loaded = writer.ReadJson<BranchManifest>(RunFolderLayout.BranchManifestFile);
        Assert.NotNull(loaded);
        Assert.Equal("test-branch", loaded.BranchId);
        Assert.Equal(4, loaded.BaseDimension);
        Assert.Equal(14, loaded.AmbientDimension);
    }

    [Fact]
    public void WriteReplayContract_RoundTrips()
    {
        var writer = new RunFolderWriter(_tempDir);
        writer.CreateDirectories();

        var contract = TestHelpers.CreateTestReplayContract();
        writer.WriteReplayContract(contract);

        var loaded = writer.ReadJson<ReplayContract>(RunFolderLayout.ReplayContractFile);
        Assert.NotNull(loaded);
        Assert.Equal("cpu-reference", loaded.BackendId);
        Assert.True(loaded.Deterministic);
        Assert.Equal("R2", loaded.ReplayTier);
        Assert.Equal(42, loaded.RandomSeed);
    }

    [Fact]
    public void WriteValidationBundle_RoundTrips()
    {
        var writer = new RunFolderWriter(_tempDir);
        writer.CreateDirectories();

        var bundle = TestHelpers.CreateTestValidationBundle();
        writer.WriteValidationBundle(bundle);

        var loaded = writer.ReadJson<ValidationBundle>(RunFolderLayout.ValidationBundleFile);
        Assert.NotNull(loaded);
        Assert.True(loaded.AllPassed);
        Assert.Equal(2, loaded.Records.Count);
    }

    [Fact]
    public void WriteValidationRecords_CreatesIndividualFiles()
    {
        var writer = new RunFolderWriter(_tempDir);
        writer.CreateDirectories();

        var records = new[]
        {
            TestHelpers.CreateTestRecord("gauge-norm", true),
            TestHelpers.CreateTestRecord("jacobi-check", true),
        };
        writer.WriteValidationRecords(records);

        Assert.True(File.Exists(Path.Combine(_tempDir, "validation/records/gauge-norm.json")));
        Assert.True(File.Exists(Path.Combine(_tempDir, "validation/records/jacobi-check.json")));
    }

    [Fact]
    public void WriteIntegrityBundle_RoundTrips()
    {
        var writer = new RunFolderWriter(_tempDir);
        writer.CreateDirectories();

        var bundle = TestHelpers.CreateTestIntegrityBundle();
        writer.WriteIntegrityBundle(bundle);

        var loaded = writer.ReadJson<IntegrityBundle>(RunFolderLayout.HashesFile);
        Assert.NotNull(loaded);
        Assert.Equal("abc123def456", loaded.ContentHash);
        Assert.Equal("SHA-256", loaded.HashAlgorithm);
    }

    [Fact]
    public void WritePackageRoot_WritesArtifactId()
    {
        var writer = new RunFolderWriter(_tempDir);
        writer.CreateDirectories();

        writer.WritePackageRoot("artifact-001");

        var content = File.ReadAllText(Path.Combine(_tempDir, RunFolderLayout.PackageRootFile));
        Assert.Equal("artifact-001", content);
    }

    [Fact]
    public void WriteArtifactBundle_PopulatesEntireFolder()
    {
        var writer = new RunFolderWriter(_tempDir);
        var bundle = TestHelpers.CreateTestArtifactBundle();

        writer.WriteArtifactBundle(bundle);

        Assert.True(writer.HasValidStructure());
        Assert.True(File.Exists(Path.Combine(_tempDir, RunFolderLayout.BranchManifestFile)));
        Assert.True(File.Exists(Path.Combine(_tempDir, RunFolderLayout.ReplayContractFile)));
        Assert.True(File.Exists(Path.Combine(_tempDir, RunFolderLayout.ValidationBundleFile)));
        Assert.True(File.Exists(Path.Combine(_tempDir, RunFolderLayout.HashesFile)));
        Assert.True(File.Exists(Path.Combine(_tempDir, RunFolderLayout.PackageRootFile)));

        var rootContent = File.ReadAllText(Path.Combine(_tempDir, RunFolderLayout.PackageRootFile));
        Assert.Equal("artifact-001", rootContent);
    }

    [Fact]
    public void WriteRuntime_CreatesFile()
    {
        var writer = new RunFolderWriter(_tempDir);
        writer.CreateDirectories();

        var runtime = RuntimeInfo.CaptureCurrentEnvironment("cpu-reference");
        writer.WriteRuntime(runtime);

        var loaded = writer.ReadJson<RuntimeInfo>(RunFolderLayout.RuntimeFile);
        Assert.NotNull(loaded);
        Assert.Equal("cpu-reference", loaded.BackendId);
        Assert.NotNull(loaded.Hostname);
        Assert.NotNull(loaded.RuntimeVersion);
    }

    [Fact]
    public void WriteResidualBundle_RoundTrips()
    {
        var writer = new RunFolderWriter(_tempDir);
        writer.CreateDirectories();

        var bundle = TestHelpers.CreateTestResidualBundle(1.5e-6);
        writer.WriteResidualBundle(bundle);

        var loaded = writer.ReadJson<ResidualBundle>(RunFolderLayout.ResidualBundleFile);
        Assert.NotNull(loaded);
        Assert.Equal(1.5e-6, loaded.ObjectiveValue, precision: 12);
    }

    [Fact]
    public void ReadJson_ReturnsDefault_WhenFileMissing()
    {
        var writer = new RunFolderWriter(_tempDir);
        var result = writer.ReadJson<BranchManifest>("nonexistent.json");
        Assert.Null(result);
    }

    [Fact]
    public void WriteEnvironmentLog_WritesContent()
    {
        var writer = new RunFolderWriter(_tempDir);
        writer.CreateDirectories();

        writer.WriteEnvironmentLog("OS: Linux\nRuntime: .NET 10.0");

        var content = File.ReadAllText(Path.Combine(_tempDir, RunFolderLayout.EnvironmentFile));
        Assert.Contains("Linux", content);
    }
}
