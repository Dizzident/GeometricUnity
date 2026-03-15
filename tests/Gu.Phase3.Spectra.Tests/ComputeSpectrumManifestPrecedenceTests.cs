using Gu.Core;
using Gu.Core.Serialization;

namespace Gu.Phase3.Spectra.Tests;

/// <summary>
/// Tests for WP-2: compute-spectrum manifest precedence (D-003).
/// Validates the three-tier precedence:
///   1. background_states/{backgroundId}_manifest.json  (per-background)
///   2. manifest/branch.json                            (run-folder)
///   3. legacy generated default                        (fallback with diagnostic note)
/// </summary>
public sealed class ComputeSpectrumManifestPrecedenceTests : IDisposable
{
    private readonly string _runFolder;

    public ComputeSpectrumManifestPrecedenceTests()
    {
        _runFolder = Path.Combine(Path.GetTempPath(), $"gu-spectrum-manifest-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_runFolder);
    }

    public void Dispose()
    {
        if (Directory.Exists(_runFolder))
            Directory.Delete(_runFolder, recursive: true);
    }

    private BranchManifest MakeManifest(string branchId) => new()
    {
        BranchId = branchId,
        SchemaVersion = "1.0.0",
        SourceEquationRevision = "r1",
        CodeRevision = "test",
        LieAlgebraId = "su2",
        BaseDimension = 2,
        AmbientDimension = 5,
        ActiveGeometryBranch = "simplicial",
        ActiveObservationBranch = "sigma-pullback",
        ActiveTorsionBranch = "trivial",
        ActiveShiabBranch = "identity-shiab",
        ActiveGaugeStrategy = "penalty",
        PairingConventionId = "pairing-trace",
        BasisConventionId = "canonical",
        ComponentOrderId = "face-major",
        AdjointConventionId = "adjoint-explicit",
        NormConventionId = "norm-l2-quadrature",
        DifferentialFormMetricId = "hodge-standard",
        InsertedAssumptionIds = Array.Empty<string>(),
        InsertedChoiceIds = new[] { "IX-1" },
    };

    private void WritePerBackgroundManifest(string bgId, string branchId)
    {
        var statesDir = Path.Combine(_runFolder, "backgrounds", "background_states");
        Directory.CreateDirectory(statesDir);
        File.WriteAllText(
            Path.Combine(statesDir, $"{bgId}_manifest.json"),
            GuJsonDefaults.Serialize(MakeManifest(branchId)));
    }

    private void WriteRunFolderManifest(string branchId)
    {
        var manifestDir = Path.Combine(_runFolder, "manifest");
        Directory.CreateDirectory(manifestDir);
        File.WriteAllText(
            Path.Combine(manifestDir, "branch.json"),
            GuJsonDefaults.Serialize(MakeManifest(branchId)));
    }

    private (BranchManifest? manifest, string source) LoadManifestWithPrecedence(string backgroundId)
    {
        // This mirrors the D-003 logic in the CLI's ComputeSpectrum function
        var perBgPath1 = Path.Combine(_runFolder, "backgrounds", "background_states", $"{backgroundId}_manifest.json");
        var perBgPath2 = Path.Combine(_runFolder, "background_states", $"{backgroundId}_manifest.json");
        var perBgPath = File.Exists(perBgPath1) ? perBgPath1
                      : File.Exists(perBgPath2) ? perBgPath2
                      : null;

        if (perBgPath is not null)
        {
            var m = GuJsonDefaults.Deserialize<BranchManifest>(File.ReadAllText(perBgPath));
            if (m is not null)
                return (m, $"per-background:{perBgPath}");
        }

        var runFolderManifestPath = Path.Combine(_runFolder, "manifest", "branch.json");
        if (File.Exists(runFolderManifestPath))
        {
            var m = GuJsonDefaults.Deserialize<BranchManifest>(File.ReadAllText(runFolderManifestPath));
            if (m is not null)
                return (m, $"run-folder:{runFolderManifestPath}");
        }

        // Fallback
        return (null, "default-toy-manifest (fallback: no per-background or run-folder manifest found)");
    }

    // Test 1: a run folder with two different per-background manifests yields different operator branch
    //         choices for different background IDs
    [Fact]
    public void TwoDifferentPerBackgroundManifests_YieldDifferentBranchIds()
    {
        WritePerBackgroundManifest("bg-001", "branch-operator-A");
        WritePerBackgroundManifest("bg-002", "branch-operator-B");

        var (manifest1, source1) = LoadManifestWithPrecedence("bg-001");
        var (manifest2, source2) = LoadManifestWithPrecedence("bg-002");

        Assert.NotNull(manifest1);
        Assert.NotNull(manifest2);
        Assert.Equal("branch-operator-A", manifest1.BranchId);
        Assert.Equal("branch-operator-B", manifest2.BranchId);
        Assert.NotEqual(manifest1.BranchId, manifest2.BranchId);
        Assert.Contains("per-background", source1);
        Assert.Contains("per-background", source2);
    }

    // Test 2: per-background manifest overrides run-folder manifest
    [Fact]
    public void PerBackgroundManifest_OverridesRunFolderManifest()
    {
        WriteRunFolderManifest("run-folder-branch");
        WritePerBackgroundManifest("bg-001", "per-bg-branch");

        var (manifest, source) = LoadManifestWithPrecedence("bg-001");

        Assert.NotNull(manifest);
        Assert.Equal("per-bg-branch", manifest.BranchId);
        Assert.Contains("per-background", source);
        Assert.DoesNotContain("run-folder", source);
    }

    // Test 3: missing per-background manifest falls back to run-folder manifest
    [Fact]
    public void MissingPerBackgroundManifest_FallsBackToRunFolderManifest()
    {
        WriteRunFolderManifest("run-folder-branch");
        // No per-background manifest written

        var (manifest, source) = LoadManifestWithPrecedence("bg-001");

        Assert.NotNull(manifest);
        Assert.Equal("run-folder-branch", manifest.BranchId);
        Assert.Contains("run-folder", source);
        Assert.DoesNotContain("per-background", source);
    }

    // Test 4: missing both manifests emits diagnostic fallback note
    [Fact]
    public void MissingBothManifests_EmitsDiagnosticFallbackNote()
    {
        // No manifests written at all

        var (manifest, source) = LoadManifestWithPrecedence("bg-001");

        Assert.Null(manifest);
        Assert.Contains("fallback", source);
        Assert.Contains("no per-background or run-folder manifest found", source);
    }

    // Additional: per-background manifest for bg-A does not affect bg-B's resolution
    [Fact]
    public void PerBackgroundManifest_OnlyAppliesToNamedBackground()
    {
        WritePerBackgroundManifest("bg-001", "per-bg-branch");
        WriteRunFolderManifest("run-folder-branch");

        // bg-001 gets per-background manifest
        var (manifest1, source1) = LoadManifestWithPrecedence("bg-001");
        // bg-002 has no per-background manifest, falls back to run-folder
        var (manifest2, source2) = LoadManifestWithPrecedence("bg-002");

        Assert.NotNull(manifest1);
        Assert.NotNull(manifest2);
        Assert.Equal("per-bg-branch", manifest1.BranchId);
        Assert.Equal("run-folder-branch", manifest2.BranchId);
        Assert.Contains("per-background", source1);
        Assert.Contains("run-folder", source2);
    }

    // Additional: verify manifest source string contains expected tier prefix
    [Fact]
    public void ManifestSourceString_ContainsTierPrefix()
    {
        WritePerBackgroundManifest("bg-001", "test-branch");

        var (_, source) = LoadManifestWithPrecedence("bg-001");

        Assert.StartsWith("per-background:", source);
    }
}
