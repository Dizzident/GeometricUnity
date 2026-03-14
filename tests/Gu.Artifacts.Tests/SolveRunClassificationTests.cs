using Gu.Artifacts;
using Gu.Core.Serialization;

namespace Gu.Artifacts.Tests;

/// <summary>
/// Tests for G-002: SolveRunClassification correctly distinguishes trivial from nontrivial
/// validation paths and records seed provenance.
/// </summary>
public class SolveRunClassificationTests : IDisposable
{
    private readonly string _tempDir;

    public SolveRunClassificationTests()
    {
        _tempDir = TestHelpers.CreateTempRunFolder();
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    // ── Classify() ──────────────────────────────────────────────────────────────

    [Fact]
    public void Classify_ModeA_ZeroSeed_IsTrivialValidationPath()
    {
        var c = SolveRunClassification.Classify(
            modeFlag: "A",
            hasPersistedOmega: false,
            hasExplicitOmega: false,
            hasExplicitA0: false);

        Assert.Equal("residual-inspection", c.RunType);
        Assert.Equal("zero-seed", c.SeedSource);
        Assert.True(c.IsTrivialValidationPath);
        Assert.NotNull(c.ValidationNote);
        Assert.Contains("trivial", c.ValidationNote, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Classify_ModeB_ZeroSeed_IsObjectiveSolve_NotTrivial()
    {
        var c = SolveRunClassification.Classify(
            modeFlag: "B",
            hasPersistedOmega: false,
            hasExplicitOmega: false,
            hasExplicitA0: false);

        Assert.Equal("objective-solve", c.RunType);
        Assert.Equal("zero-seed", c.SeedSource);
        Assert.False(c.IsTrivialValidationPath);
    }

    [Fact]
    public void Classify_ModeC_ZeroSeed_IsStationaritySolve_NotTrivial()
    {
        var c = SolveRunClassification.Classify(
            modeFlag: "C",
            hasPersistedOmega: false,
            hasExplicitOmega: false,
            hasExplicitA0: false);

        Assert.Equal("stationarity-solve", c.RunType);
        Assert.Equal("zero-seed", c.SeedSource);
        Assert.False(c.IsTrivialValidationPath);
    }

    [Fact]
    public void Classify_ModeD_IsBranchSensitivity_NotTrivial()
    {
        var c = SolveRunClassification.Classify(
            modeFlag: "D",
            hasPersistedOmega: false,
            hasExplicitOmega: false,
            hasExplicitA0: false);

        Assert.Equal("branch-sensitivity", c.RunType);
        Assert.False(c.IsTrivialValidationPath);
    }

    [Fact]
    public void Classify_ModeA_PersistedOmega_SeedSourceIsPersistedState_NotTrivial()
    {
        var c = SolveRunClassification.Classify(
            modeFlag: "A",
            hasPersistedOmega: true,
            hasExplicitOmega: false,
            hasExplicitA0: false);

        Assert.Equal("residual-inspection", c.RunType);
        Assert.Equal("persisted-state", c.SeedSource);
        Assert.False(c.IsTrivialValidationPath);
    }

    [Fact]
    public void Classify_ExplicitOmegaTakesPrecedenceOverPersistedState()
    {
        var c = SolveRunClassification.Classify(
            modeFlag: "B",
            hasPersistedOmega: true,
            hasExplicitOmega: true,
            hasExplicitA0: false);

        Assert.Equal("explicit-omega", c.SeedSource);
    }

    [Fact]
    public void Classify_ExplicitA0WithNoOmega_SeedSourceIsExplicitA0()
    {
        var c = SolveRunClassification.Classify(
            modeFlag: "A",
            hasPersistedOmega: false,
            hasExplicitOmega: false,
            hasExplicitA0: true);

        Assert.Equal("explicit-a0", c.SeedSource);
        // Still residual-only mode — not trivial (A0 provided)
        Assert.False(c.IsTrivialValidationPath);
    }

    [Fact]
    public void Classify_LowerCaseModeFlag_StillClassifiedCorrectly()
    {
        var c = SolveRunClassification.Classify(
            modeFlag: "b",
            hasPersistedOmega: false,
            hasExplicitOmega: false,
            hasExplicitA0: false);

        Assert.Equal("objective-solve", c.RunType);
    }

    [Fact]
    public void Classify_UnknownMode_FallsBackToResidualInspection()
    {
        var c = SolveRunClassification.Classify(
            modeFlag: "Z",
            hasPersistedOmega: false,
            hasExplicitOmega: false,
            hasExplicitA0: false);

        Assert.Equal("residual-inspection", c.RunType);
    }

    // ── Serialization roundtrip ─────────────────────────────────────────────────

    [Fact]
    public void Classify_SerializesAndDeserializesCorrectly()
    {
        var c = SolveRunClassification.Classify(
            modeFlag: "B",
            hasPersistedOmega: true,
            hasExplicitOmega: false,
            hasExplicitA0: false);

        var json = GuJsonDefaults.Serialize(c);
        var round = GuJsonDefaults.Deserialize<SolveRunClassification>(json);

        Assert.NotNull(round);
        Assert.Equal("objective-solve", round.RunType);
        Assert.Equal("persisted-state", round.SeedSource);
        Assert.False(round.IsTrivialValidationPath);
        Assert.Equal("B", round.SolverMode);
    }

    // ── RunFolderWriter integration ─────────────────────────────────────────────

    [Fact]
    public void WriteSolveRunClassification_WritesFileAtExpectedPath()
    {
        var writer = new RunFolderWriter(_tempDir);
        var c = SolveRunClassification.Classify(
            modeFlag: "A",
            hasPersistedOmega: false,
            hasExplicitOmega: false,
            hasExplicitA0: false);

        writer.WriteSolveRunClassification(c);

        var expectedPath = Path.Combine(_tempDir, RunFolderLayout.SolveRunClassificationFile);
        Assert.True(File.Exists(expectedPath),
            $"Classification file not written at {RunFolderLayout.SolveRunClassificationFile}");
    }

    [Fact]
    public void WriteSolveRunClassification_ContentsAreReadableAndCorrect()
    {
        var writer = new RunFolderWriter(_tempDir);
        var c = SolveRunClassification.Classify(
            modeFlag: "C",
            hasPersistedOmega: false,
            hasExplicitOmega: true,
            hasExplicitA0: false);

        writer.WriteSolveRunClassification(c);

        var read = writer.ReadJson<SolveRunClassification>(
            RunFolderLayout.SolveRunClassificationFile);

        Assert.NotNull(read);
        Assert.Equal("stationarity-solve", read.RunType);
        Assert.Equal("explicit-omega", read.SeedSource);
        Assert.False(read.IsTrivialValidationPath);
    }

    // ── G-002 policy: trivial path detection ────────────────────────────────────

    [Fact]
    public void G002_TrivialPathRequiresBothResidualModeAndZeroSeed()
    {
        // Any non-zero seed prevents trivial flag
        Assert.False(SolveRunClassification.Classify("A", hasPersistedOmega: true, false, false).IsTrivialValidationPath);
        Assert.False(SolveRunClassification.Classify("A", false, hasExplicitOmega: true, false).IsTrivialValidationPath);
        Assert.False(SolveRunClassification.Classify("A", false, false, hasExplicitA0: true).IsTrivialValidationPath);
        // Any non-A mode prevents trivial flag
        Assert.False(SolveRunClassification.Classify("B", false, false, false).IsTrivialValidationPath);
        Assert.False(SolveRunClassification.Classify("C", false, false, false).IsTrivialValidationPath);
        Assert.False(SolveRunClassification.Classify("D", false, false, false).IsTrivialValidationPath);
        // Only Mode A + zero seed → trivial
        Assert.True(SolveRunClassification.Classify("A", false, false, false).IsTrivialValidationPath);
    }
}
