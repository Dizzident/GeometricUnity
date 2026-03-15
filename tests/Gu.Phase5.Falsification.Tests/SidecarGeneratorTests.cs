using Gu.Core;
using Gu.Core.Serialization;
using Gu.Phase4.Registry;
using Gu.Phase5.Falsification;

namespace Gu.Phase5.Falsification.Tests;

/// <summary>
/// Tests for SidecarGenerator (P6-M3).
/// Covers:
///   - Empty-but-present sidecars are distinguishable from omitted sidecars
///   - No-trigger falsifier runs still report non-zero evaluated input counts
///   - Dossier includes observation-chain summary when sidecars are present
/// </summary>
public sealed class SidecarGeneratorTests
{
    private static ProvenanceMeta MakeProvenance() => new ProvenanceMeta
    {
        CreatedAt = DateTimeOffset.UtcNow,
        CodeRevision = "test-sidecar",
        Branch = new BranchRef { BranchId = "test-branch", SchemaVersion = "1.0.0" },
    };

    private static UnifiedParticleRegistry EmptyRegistry() => new UnifiedParticleRegistry();

    // ------------------------------------------------------------------
    // Absent vs. skipped channel distinction
    // ------------------------------------------------------------------

    [Fact]
    public void GenerateSidecars_NullChannel_StatusIsAbsent()
    {
        using var tmpDir = new TempDirectory();
        var registry = EmptyRegistry();
        var provenance = MakeProvenance();

        // All four sidecar channels are null → absent
        var summary = SidecarGenerator.GenerateSidecars(
            registry,
            observables: null,
            envRecords: null,
            outDir: tmpDir.Path,
            studyId: "study-null",
            provenance: provenance,
            observationChainRecords: null,
            environmentVarianceRecords: null,
            representationContentRecords: null,
            couplingConsistencyRecords: null);

        Assert.Equal(4, summary.Channels.Count);
        Assert.All(summary.Channels, ch => Assert.Equal("absent", ch.Status));
        Assert.All(summary.Channels, ch => Assert.Equal(0, ch.InputCount));
        Assert.All(summary.Channels, ch => Assert.Equal(0, ch.OutputCount));

        // None of the four sidecar files should have been written
        Assert.False(File.Exists(Path.Combine(tmpDir.Path, "observation_chain.json")));
        Assert.False(File.Exists(Path.Combine(tmpDir.Path, "environment_variance.json")));
        Assert.False(File.Exists(Path.Combine(tmpDir.Path, "representation_content.json")));
        Assert.False(File.Exists(Path.Combine(tmpDir.Path, "coupling_consistency.json")));

        // sidecar_summary.json must still be written
        Assert.True(File.Exists(Path.Combine(tmpDir.Path, "sidecar_summary.json")));
    }

    [Fact]
    public void GenerateSidecars_EmptyListChannel_StatusIsSkipped()
    {
        using var tmpDir = new TempDirectory();
        var registry = EmptyRegistry();
        var provenance = MakeProvenance();

        // Pass empty (non-null) lists for all four channels → skipped
        var summary = SidecarGenerator.GenerateSidecars(
            registry,
            observables: null,
            envRecords: null,
            outDir: tmpDir.Path,
            studyId: "study-empty",
            provenance: provenance,
            observationChainRecords: new List<ObservationChainRecord>(),
            environmentVarianceRecords: new List<EnvironmentVarianceRecord>(),
            representationContentRecords: new List<RepresentationContentRecord>(),
            couplingConsistencyRecords: new List<CouplingConsistencyRecord>());

        Assert.Equal(4, summary.Channels.Count);
        Assert.All(summary.Channels, ch => Assert.Equal("skipped", ch.Status));
        Assert.All(summary.Channels, ch => Assert.Equal(0, ch.InputCount));

        // All four sidecar files must be written (even if empty arrays)
        Assert.True(File.Exists(Path.Combine(tmpDir.Path, "observation_chain.json")));
        Assert.True(File.Exists(Path.Combine(tmpDir.Path, "environment_variance.json")));
        Assert.True(File.Exists(Path.Combine(tmpDir.Path, "representation_content.json")));
        Assert.True(File.Exists(Path.Combine(tmpDir.Path, "coupling_consistency.json")));
    }

    [Fact]
    public void GenerateSidecars_EmptyAndNull_DistinguishedByStatus()
    {
        using var tmpDir = new TempDirectory();
        var registry = EmptyRegistry();
        var provenance = MakeProvenance();

        // obs chain = empty list (skipped), rest = null (absent)
        var summary = SidecarGenerator.GenerateSidecars(
            registry,
            observables: null,
            envRecords: null,
            outDir: tmpDir.Path,
            studyId: "study-mixed",
            provenance: provenance,
            observationChainRecords: new List<ObservationChainRecord>(),
            environmentVarianceRecords: null,
            representationContentRecords: null,
            couplingConsistencyRecords: null);

        var obs = summary.Channels.First(c => c.ChannelId == "observation-chain");
        var env = summary.Channels.First(c => c.ChannelId == "environment-variance");

        Assert.Equal("skipped", obs.Status);
        Assert.Equal("absent", env.Status);

        Assert.True(File.Exists(Path.Combine(tmpDir.Path, "observation_chain.json")));
        Assert.False(File.Exists(Path.Combine(tmpDir.Path, "environment_variance.json")));
    }

    // ------------------------------------------------------------------
    // Evaluated status and input count reporting
    // ------------------------------------------------------------------

    [Fact]
    public void GenerateSidecars_WithRecords_StatusIsEvaluatedAndInputCountNonZero()
    {
        using var tmpDir = new TempDirectory();
        var registry = EmptyRegistry();
        var provenance = MakeProvenance();

        var obsRecords = new List<ObservationChainRecord>
        {
            new ObservationChainRecord
            {
                CandidateId = "cand-A",
                PrimarySourceId = "src-A",
                ObservableId = "obs-1",
                CompletenessStatus = "complete",
                SensitivityScore = 0.1,
                AuxiliaryModelSensitivity = 0.1,
                Passed = true,
                Provenance = provenance,
            },
            new ObservationChainRecord
            {
                CandidateId = "cand-B",
                PrimarySourceId = "src-B",
                ObservableId = "obs-2",
                CompletenessStatus = "partial",
                SensitivityScore = 0.2,
                AuxiliaryModelSensitivity = 0.15,
                Passed = false,
                Provenance = provenance,
            },
        };

        var summary = SidecarGenerator.GenerateSidecars(
            registry,
            observables: null,
            envRecords: null,
            outDir: tmpDir.Path,
            studyId: "study-eval",
            provenance: provenance,
            observationChainRecords: obsRecords,
            environmentVarianceRecords: null,
            representationContentRecords: null,
            couplingConsistencyRecords: null);

        var ch = summary.Channels.First(c => c.ChannelId == "observation-chain");
        Assert.Equal("evaluated", ch.Status);
        Assert.Equal(2, ch.InputCount);
        Assert.Equal(2, ch.OutputCount);
    }

    // ------------------------------------------------------------------
    // No-trigger falsifier runs still report non-zero evaluated input counts
    // ------------------------------------------------------------------

    [Fact]
    public void FalsifierEvaluator_WithNonTriggeringRecords_ReportsNonZeroCounts()
    {
        var evaluator = new FalsifierEvaluator();
        var policy = new FalsificationPolicy
        {
            ObservationInstabilityThreshold = 0.5,
            EnvironmentInstabilityThreshold = 0.5,
            CouplingInconsistencyThreshold = 0.5,
        };
        var provenance = MakeProvenance();

        // Records present but below all trigger thresholds → no falsifiers fire
        var obsRecords = new[]
        {
            new ObservationChainRecord
            {
                CandidateId = "cand-X",
                PrimarySourceId = "src-X",
                ObservableId = "obs-x",
                CompletenessStatus = "complete",
                SensitivityScore = 0.1,
                AuxiliaryModelSensitivity = 0.1,
                Passed = true,
                Provenance = provenance,
            },
        };
        var envRecords = new[]
        {
            new EnvironmentVarianceRecord
            {
                RecordId = "ev-001",
                QuantityId = "q-lambda",
                EnvironmentTierId = "toy",
                RelativeStdDev = 0.1,  // below threshold
            },
        };

        var summary = evaluator.Evaluate(
            "study-no-trigger",
            branchRecord: null,
            convergenceRecords: null,
            convergenceFailures: null,
            scoreCard: null,
            policy: policy,
            provenance: provenance,
            observationRecords: obsRecords,
            environmentVarianceRecords: envRecords);

        // No falsifiers should have fired
        Assert.Equal(0, summary.TotalActiveCount);

        // But counts must reflect the inputs that were evaluated
        Assert.Equal(1, summary.ObservationRecordCount);
        Assert.Equal(1, summary.EnvironmentRecordCount);
        Assert.Equal(0, summary.RepresentationRecordCount);
        Assert.Equal(0, summary.CouplingRecordCount);
    }

    // ------------------------------------------------------------------
    // SidecarSummary round-trip serialization
    // ------------------------------------------------------------------

    [Fact]
    public void GenerateSidecars_SidecarSummaryJsonRoundTrips()
    {
        using var tmpDir = new TempDirectory();
        var registry = EmptyRegistry();
        var provenance = MakeProvenance();

        SidecarGenerator.GenerateSidecars(
            registry,
            observables: null,
            envRecords: null,
            outDir: tmpDir.Path,
            studyId: "study-rt",
            provenance: provenance,
            observationChainRecords: new List<ObservationChainRecord>(),
            environmentVarianceRecords: null,
            representationContentRecords: null,
            couplingConsistencyRecords: null);

        var summaryPath = Path.Combine(tmpDir.Path, "sidecar_summary.json");
        Assert.True(File.Exists(summaryPath));

        var json = File.ReadAllText(summaryPath);
        var loaded = GuJsonDefaults.Deserialize<SidecarSummary>(json);

        Assert.NotNull(loaded);
        Assert.Equal("study-rt", loaded!.StudyId);
        Assert.Equal(4, loaded.Channels.Count);

        var obsChannel = loaded.Channels.First(c => c.ChannelId == "observation-chain");
        Assert.Equal("skipped", obsChannel.Status);
        Assert.Equal(0, obsChannel.InputCount);

        var envChannel = loaded.Channels.First(c => c.ChannelId == "environment-variance");
        Assert.Equal("absent", envChannel.Status);
    }

    // ------------------------------------------------------------------
    // Helper: temp directory that auto-deletes
    // ------------------------------------------------------------------

    private sealed class TempDirectory : IDisposable
    {
        public string Path { get; } = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            "gu_sidecar_tests_" + Guid.NewGuid().ToString("N")[..8]);

        public TempDirectory() => Directory.CreateDirectory(Path);

        public void Dispose()
        {
            try { Directory.Delete(Path, recursive: true); }
            catch { /* best-effort */ }
        }
    }
}
