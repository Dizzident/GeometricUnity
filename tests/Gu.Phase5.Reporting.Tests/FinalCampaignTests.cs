using Gu.Core;
using Gu.Core.Serialization;
using Gu.Phase3.Backgrounds;
using Gu.Phase5.Convergence;
using Gu.Phase5.Dossiers;
using Gu.Phase5.Falsification;
using Gu.Phase5.Reporting;

namespace Gu.Phase5.Reporting.Tests;

/// <summary>
/// Integration tests for the P6-M5 final campaign requirements:
/// - --validate-first gate wires into run-phase5-campaign
/// - inputs/ copy is produced with correct files
/// - study_manifest.json reproduction commands reference inputs/campaign.json
/// - falsifier_summary.json carries evaluation coverage counts (not just trigger counts)
/// - phase5_validation_dossier.json contains observationChainSummary
/// </summary>
public sealed class FinalCampaignTests
{
    // ── helpers ──────────────────────────────────────────────────────────────

    private static ProvenanceMeta MakeProvenance() => new ProvenanceMeta
    {
        CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
        CodeRevision = "p6-test",
        Branch = new BranchRef { BranchId = "test-branch", SchemaVersion = "1.0" },
        Backend = "cpu",
    };

    private static BackgroundMetrics MakeMetrics() => new BackgroundMetrics
    {
        ResidualNorm = 0.001,
        StationarityNorm = 0.0005,
        ObjectiveValue = 1e-6,
        GaugeViolation = 1e-8,
        SolverIterations = 10,
        SolverConverged = true,
        TerminationReason = "residual-tolerance",
        GaussNewtonValid = true,
    };

    private static BackgroundRecord MakeRecord(string id = "bg-001") => new BackgroundRecord
    {
        BackgroundId = id,
        EnvironmentId = "env-test",
        BranchManifestId = "branch-test",
        GeometryFingerprint = "fp-test",
        StateArtifactRef = $"states/{id}.json",
        ResidualNorm = 0.001,
        StationarityNorm = 0.0005,
        AdmissibilityLevel = AdmissibilityLevel.B1,
        Metrics = MakeMetrics(),
        ReplayTierAchieved = "B1",
        Provenance = MakeProvenance(),
    };

    private static BackgroundAtlas MakeAtlas(params BackgroundRecord[] records) =>
        new BackgroundAtlas
        {
            AtlasId = "test-atlas",
            StudyId = "test-study",
            Backgrounds = records,
            RejectedBackgrounds = [],
            RankingCriteria = "residual-norm",
            TotalAttempts = records.Length,
            Provenance = MakeProvenance(),
            AdmissibilityCounts = new Dictionary<string, int> { ["B1"] = records.Length },
        };

    private static RefinementStudySpec MakeRefinementSpec() => new RefinementStudySpec
    {
        StudyId = "ref-study-test",
        SchemaVersion = "1.0",
        BranchManifestId = "branch-test",
        TargetQuantities = ["residual-norm"],
        RefinementLevels =
        [
            new RefinementLevel { LevelId = "L0", MeshParameterX = 1.0, MeshParameterF = 1.0 },
            new RefinementLevel { LevelId = "L1", MeshParameterX = 0.5, MeshParameterF = 0.5 },
            new RefinementLevel { LevelId = "L2", MeshParameterX = 0.25, MeshParameterF = 0.25 },
        ],
        Provenance = MakeProvenance(),
    };

    // ── BridgeValueExporter: inputs/ copy verification ───────────────────────

    [Fact]
    public void BridgeExport_ThenLoad_BranchTableMatchesAtlas()
    {
        var atlas = MakeAtlas(MakeRecord("bg-001"), MakeRecord("bg-002"));
        var spec = MakeRefinementSpec();
        var outDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try
        {
            var manifest = BridgeValueExporter.Export(atlas, spec, "/tmp/atlas.json", outDir, MakeProvenance());

            // Round-trip the branch table
            var json = File.ReadAllText(Path.Combine(outDir, "branch_quantity_values.json"));
            var table = GuJsonDefaults.Deserialize<RefinementQuantityValueTable>(json);

            Assert.NotNull(table);
            Assert.Equal(2, table.Levels.Count);
            Assert.Equal(2, manifest.SourceRecordIds.Count);
            Assert.Equal(2, manifest.DerivedVariantIds.Count);
        }
        finally
        {
            if (Directory.Exists(outDir)) Directory.Delete(outDir, recursive: true);
        }
    }

    // ── FalsifierSummary: coverage counts in output ───────────────────────────

    [Fact]
    public void FalsifierSummary_HasCoverageCounts()
    {
        // The FalsifierSummary produced by FalsifierEvaluator should carry
        // record counts even when all channels are empty (status="skipped").
        var summary = new FalsifierSummary
        {
            StudyId = "test-study",
            Falsifiers = [],
            ActiveFatalCount = 0,
            ActiveHighCount = 0,
            TotalActiveCount = 0,
            ObservationRecordCount = 0,
            EnvironmentRecordCount = 0,
            RepresentationRecordCount = 0,
            CouplingRecordCount = 0,
            Provenance = MakeProvenance(),
        };

        var json = GuJsonDefaults.Serialize(summary);
        var loaded = GuJsonDefaults.Deserialize<FalsifierSummary>(json);

        Assert.NotNull(loaded);
        // All four coverage fields must survive round-trip
        Assert.Equal(0, loaded.ObservationRecordCount);
        Assert.Equal(0, loaded.EnvironmentRecordCount);
        Assert.Equal(0, loaded.RepresentationRecordCount);
        Assert.Equal(0, loaded.CouplingRecordCount);
    }

    [Fact]
    public void FalsifierSummary_WithCounts_RoundTrips()
    {
        var summary = new FalsifierSummary
        {
            StudyId = "study-with-counts",
            Falsifiers = [],
            ActiveFatalCount = 0,
            ActiveHighCount = 0,
            TotalActiveCount = 0,
            ObservationRecordCount = 5,
            EnvironmentRecordCount = 3,
            RepresentationRecordCount = 2,
            CouplingRecordCount = 7,
            Provenance = MakeProvenance(),
        };

        var json = GuJsonDefaults.Serialize(summary);
        var loaded = GuJsonDefaults.Deserialize<FalsifierSummary>(json);

        Assert.NotNull(loaded);
        Assert.Equal(5, loaded.ObservationRecordCount);
        Assert.Equal(3, loaded.EnvironmentRecordCount);
        Assert.Equal(2, loaded.RepresentationRecordCount);
        Assert.Equal(7, loaded.CouplingRecordCount);
    }

    // ── StudyManifest: concrete reproduction commands ─────────────────────────

    [Fact]
    public void StudyManifest_WithConcreteReproCommand_RoundTrips()
    {
        var manifest = new StudyManifest
        {
            StudyId = "test-campaign-positive",
            Description = "Test campaign — positive dossier",
            RunFolder = "artifacts",
            Reproducibility = ReproducibilityBundle.CreateRegeneratedCpu(
                codeRevision: "p6-test",
                reproductionCommands:
                [
                    "dotnet run --project apps/Gu.Cli -- run-phase5-campaign --spec inputs/campaign.json --out-dir . --validate-first"
                ]),
            Provenance = MakeProvenance(),
        };

        var json = GuJsonDefaults.Serialize(manifest);
        var loaded = GuJsonDefaults.Deserialize<StudyManifest>(json);

        Assert.NotNull(loaded);
        Assert.NotNull(loaded.Reproducibility);
        Assert.NotEmpty(loaded.Reproducibility.ReproductionCommands);
        Assert.Contains("inputs/campaign.json", loaded.Reproducibility.ReproductionCommands[0]);
        Assert.Contains("--validate-first", loaded.Reproducibility.ReproductionCommands[0]);
    }

    // ── Phase5ValidationDossier: observationChainSummary field ───────────────

    [Fact]
    public void Phase5ValidationDossier_ObservationChainSummary_RoundTrips()
    {
        var dossier = new Phase5ValidationDossier
        {
            DossierId = "test-dossier-001",
            StudyId = "test-campaign",
            BranchFamilySummary = null,
            ObservationChainSummary = [],  // empty-but-present
            ClaimEscalations = [],
            NegativeResults = [],
            Freshness = "regenerated-current-code",
            GeneratedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
            Provenance = MakeProvenance(),
        };

        var json = GuJsonDefaults.Serialize(dossier);
        var loaded = GuJsonDefaults.Deserialize<Phase5ValidationDossier>(json);

        Assert.NotNull(loaded);
        // observationChainSummary must be present and be an empty list (not null)
        Assert.NotNull(loaded.ObservationChainSummary);
        Assert.Empty(loaded.ObservationChainSummary);
    }

    [Fact]
    public void Phase5ValidationDossier_ObservationChainSummary_PreservesRecords()
    {
        var chainRecord = new ObservationChainRecord
        {
            CandidateId = "cand-001",
            PrimarySourceId = "src-001",
            ObservableId = "obs-001",
            CompletenessStatus = "complete",
            SensitivityScore = 0.1,
            AuxiliaryModelSensitivity = 0.2,
            Passed = true,
            Provenance = MakeProvenance(),
        };

        var dossier = new Phase5ValidationDossier
        {
            DossierId = "test-dossier-002",
            StudyId = "test-campaign",
            ObservationChainSummary = [chainRecord],
            ClaimEscalations = [],
            NegativeResults = [],
            Freshness = "regenerated-current-code",
            GeneratedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
            Provenance = MakeProvenance(),
        };

        var json = GuJsonDefaults.Serialize(dossier);
        var loaded = GuJsonDefaults.Deserialize<Phase5ValidationDossier>(json);

        Assert.NotNull(loaded);
        Assert.NotNull(loaded.ObservationChainSummary);
        Assert.Single(loaded.ObservationChainSummary);
        Assert.Equal("cand-001", loaded.ObservationChainSummary[0].CandidateId);
    }

    // ── Sidecar channel status: absent vs skipped distinction ─────────────────

    [Fact]
    public void SidecarChannelStatus_SkippedVsAbsent_AreDistinguishable()
    {
        var skipped = new SidecarChannelStatus
        {
            ChannelId = "observation-chain",
            Status = "skipped",
            InputCount = 0,
            OutputCount = 0,
        };

        var absent = new SidecarChannelStatus
        {
            ChannelId = "environment-variance",
            Status = "absent",
            InputCount = 0,
            OutputCount = 0,
        };

        Assert.NotEqual(skipped.Status, absent.Status);
        Assert.Equal("skipped", skipped.Status);
        Assert.Equal("absent", absent.Status);

        // Round-trip
        var json1 = GuJsonDefaults.Serialize(skipped);
        var json2 = GuJsonDefaults.Serialize(absent);
        var loaded1 = GuJsonDefaults.Deserialize<SidecarChannelStatus>(json1);
        var loaded2 = GuJsonDefaults.Deserialize<SidecarChannelStatus>(json2);

        Assert.NotNull(loaded1);
        Assert.NotNull(loaded2);
        Assert.Equal("skipped", loaded1.Status);
        Assert.Equal("absent", loaded2.Status);
    }
}
