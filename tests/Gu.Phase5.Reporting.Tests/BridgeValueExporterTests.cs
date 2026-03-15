using Gu.Core;
using Gu.Core.Serialization;
using Gu.Phase3.Backgrounds;
using Gu.Phase5.Convergence;
using Gu.Phase5.Reporting;

namespace Gu.Phase5.Reporting.Tests;

public sealed class BridgeValueExporterTests
{
    // ── helpers ─────────────────────────────────────────────────────────────

    private static ProvenanceMeta MakeProvenance() => new ProvenanceMeta
    {
        CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
        CodeRevision = "abc123",
        Branch = new BranchRef { BranchId = "test-branch", SchemaVersion = "1.0" },
    };

    private static BackgroundMetrics MakeMetrics(bool converged = true) =>
        new BackgroundMetrics
        {
            ResidualNorm = 0.001,
            StationarityNorm = 0.0005,
            ObjectiveValue = 1e-6,
            GaugeViolation = 1e-8,
            SolverIterations = 42,
            SolverConverged = converged,
            TerminationReason = converged ? "residual-tolerance" : "max-iterations",
            GaussNewtonValid = converged,
        };

    private static BackgroundRecord MakeRecord(
        string backgroundId = "bg-001",
        string branchManifestId = "branch-1",
        string geometryFingerprint = "fp-abc123",
        AdmissibilityLevel level = AdmissibilityLevel.B1,
        bool converged = true) =>
        new BackgroundRecord
        {
            BackgroundId = backgroundId,
            EnvironmentId = "env-toy",
            BranchManifestId = branchManifestId,
            GeometryFingerprint = geometryFingerprint,
            StateArtifactRef = $"artifacts/states/{backgroundId}.json",
            ResidualNorm = 0.001,
            StationarityNorm = 0.0005,
            AdmissibilityLevel = level,
            Metrics = MakeMetrics(converged),
            ReplayTierAchieved = "B1",
            Provenance = MakeProvenance(),
        };

    private static BackgroundAtlas MakeAtlas(
        IReadOnlyList<BackgroundRecord> admitted,
        IReadOnlyList<BackgroundRecord>? rejected = null) =>
        new BackgroundAtlas
        {
            AtlasId = "atlas-test-001",
            StudyId = "study-test-001",
            Backgrounds = admitted,
            RejectedBackgrounds = rejected ?? [],
            RankingCriteria = "residual-norm",
            TotalAttempts = admitted.Count + (rejected?.Count ?? 0),
            Provenance = MakeProvenance(),
            AdmissibilityCounts = new Dictionary<string, int> { ["B1"] = admitted.Count },
        };

    private static RefinementStudySpec MakeRefinementSpec(
        string studyId = "refinement-test-001",
        IReadOnlyList<string>? levelIds = null,
        IReadOnlyList<string>? quantities = null) =>
        new RefinementStudySpec
        {
            StudyId = studyId,
            SchemaVersion = "1.0",
            BranchManifestId = "branch-1",
            TargetQuantities = quantities ?? ["residual-norm", "objective-value"],
            RefinementLevels = (levelIds ?? ["level-coarse", "level-medium", "level-fine"])
                .Select((id, i) => new RefinementLevel
                {
                    LevelId = id,
                    MeshParameterX = 0.5 / System.Math.Pow(2, i),
                    MeshParameterF = 0.5 / System.Math.Pow(2, i),
                })
                .ToList(),
            Provenance = MakeProvenance(),
        };

    // ── tests ────────────────────────────────────────────────────────────────

    [Fact]
    public void Export_CreatesBranchQuantityValues_BranchManifest_AndRefinementValues()
    {
        var atlas = MakeAtlas([MakeRecord("bg-001"), MakeRecord("bg-002")]);
        var spec = MakeRefinementSpec();
        var outDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try
        {
            BridgeValueExporter.Export(atlas, spec, "/tmp/atlas.json", outDir, MakeProvenance());

            Assert.True(File.Exists(Path.Combine(outDir, "branch_quantity_values.json")));
            Assert.True(File.Exists(Path.Combine(outDir, "refinement_values.json")));
            Assert.True(File.Exists(Path.Combine(outDir, "bridge_manifest.json")));
        }
        finally
        {
            if (Directory.Exists(outDir)) Directory.Delete(outDir, recursive: true);
        }
    }

    [Fact]
    public void Export_BranchTable_HasOneLevelPerAdmittedBackground()
    {
        var atlas = MakeAtlas([MakeRecord("bg-001"), MakeRecord("bg-002"), MakeRecord("bg-003")]);
        var spec = MakeRefinementSpec();
        var outDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try
        {
            BridgeValueExporter.Export(atlas, spec, "/tmp/atlas.json", outDir, MakeProvenance());

            var json = File.ReadAllText(Path.Combine(outDir, "branch_quantity_values.json"));
            var table = GuJsonDefaults.Deserialize<RefinementQuantityValueTable>(json);

            Assert.NotNull(table);
            Assert.Equal(3, table.Levels.Count);
        }
        finally
        {
            if (Directory.Exists(outDir)) Directory.Delete(outDir, recursive: true);
        }
    }

    [Fact]
    public void Export_BranchTable_LevelIdsAreVariantIds()
    {
        var records = new[]
        {
            MakeRecord("bg-001"),
            MakeRecord("bg-002"),
        };
        var atlas = MakeAtlas(records);
        var spec = MakeRefinementSpec();
        var outDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try
        {
            var manifest = BridgeValueExporter.Export(atlas, spec, "/tmp/atlas.json", outDir, MakeProvenance());

            var json = File.ReadAllText(Path.Combine(outDir, "branch_quantity_values.json"));
            var table = GuJsonDefaults.Deserialize<RefinementQuantityValueTable>(json);

            Assert.NotNull(table);
            for (int i = 0; i < records.Length; i++)
            {
                var expectedId = BackgroundRecordBranchVariantBridge.DeriveVariantId(records[i]);
                Assert.Equal(expectedId, table.Levels[i].LevelId);
                Assert.Equal(expectedId, manifest.DerivedVariantIds[i]);
            }
        }
        finally
        {
            if (Directory.Exists(outDir)) Directory.Delete(outDir, recursive: true);
        }
    }

    [Fact]
    public void Export_IsDeterministic_SameInputsSameOutput()
    {
        var atlas = MakeAtlas([MakeRecord("bg-001")]);
        var spec = MakeRefinementSpec();
        var prov = MakeProvenance();

        var outDir1 = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        var outDir2 = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try
        {
            BridgeValueExporter.Export(atlas, spec, "/fixed/atlas.json", outDir1, prov);
            BridgeValueExporter.Export(atlas, spec, "/fixed/atlas.json", outDir2, prov);

            // branch_quantity_values and refinement_values must be byte-identical
            var branchJson1 = File.ReadAllText(Path.Combine(outDir1, "branch_quantity_values.json"));
            var branchJson2 = File.ReadAllText(Path.Combine(outDir2, "branch_quantity_values.json"));
            Assert.Equal(branchJson1, branchJson2);

            var refJson1 = File.ReadAllText(Path.Combine(outDir1, "refinement_values.json"));
            var refJson2 = File.ReadAllText(Path.Combine(outDir2, "refinement_values.json"));
            Assert.Equal(refJson1, refJson2);
        }
        finally
        {
            if (Directory.Exists(outDir1)) Directory.Delete(outDir1, recursive: true);
            if (Directory.Exists(outDir2)) Directory.Delete(outDir2, recursive: true);
        }
    }

    [Fact]
    public void Export_BridgeManifest_RoundTrips()
    {
        var atlas = MakeAtlas([MakeRecord("bg-001"), MakeRecord("bg-002")]);
        var spec = MakeRefinementSpec();
        var outDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try
        {
            var exported = BridgeValueExporter.Export(atlas, spec, "/tmp/atlas.json", outDir, MakeProvenance());

            var json = File.ReadAllText(Path.Combine(outDir, "bridge_manifest.json"));
            var loaded = GuJsonDefaults.Deserialize<BridgeManifest>(json);

            Assert.NotNull(loaded);
            Assert.Equal(exported.ManifestId, loaded.ManifestId);
            Assert.Equal(exported.SourceAtlasPath, loaded.SourceAtlasPath);
            Assert.Equal(exported.SourceRecordIds, loaded.SourceRecordIds);
            Assert.Equal(exported.SourceStateArtifactRefs, loaded.SourceStateArtifactRefs);
            Assert.Equal(exported.DerivedVariantIds, loaded.DerivedVariantIds);
        }
        finally
        {
            if (Directory.Exists(outDir)) Directory.Delete(outDir, recursive: true);
        }
    }

    [Fact]
    public void Export_BridgeManifest_RecordsSourceAtlasPath()
    {
        var atlas = MakeAtlas([MakeRecord("bg-001")]);
        var spec = MakeRefinementSpec();
        var outDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        const string atlasPath = "/studies/my_study/atlas.json";
        try
        {
            var manifest = BridgeValueExporter.Export(atlas, spec, atlasPath, outDir, MakeProvenance());
            Assert.Equal(atlasPath, manifest.SourceAtlasPath);
        }
        finally
        {
            if (Directory.Exists(outDir)) Directory.Delete(outDir, recursive: true);
        }
    }

    [Fact]
    public void Export_BridgeManifest_SourceRecordIds_MatchAtlasBackgroundIds()
    {
        var records = new[] { MakeRecord("bg-A"), MakeRecord("bg-B") };
        var atlas = MakeAtlas(records);
        var spec = MakeRefinementSpec();
        var outDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try
        {
            var manifest = BridgeValueExporter.Export(atlas, spec, "/tmp/atlas.json", outDir, MakeProvenance());

            Assert.Equal(2, manifest.SourceRecordIds.Count);
            Assert.Contains("bg-A", manifest.SourceRecordIds);
            Assert.Contains("bg-B", manifest.SourceRecordIds);
            Assert.Contains("artifacts/states/bg-A.json", manifest.SourceStateArtifactRefs);
            Assert.Contains("artifacts/states/bg-B.json", manifest.SourceStateArtifactRefs);
        }
        finally
        {
            if (Directory.Exists(outDir)) Directory.Delete(outDir, recursive: true);
        }
    }

    [Fact]
    public void Export_BridgeManifest_SourceStateArtifactRefs_MatchAtlasStateRefs()
    {
        var records = new[] { MakeRecord("bg-A"), MakeRecord("bg-B") };
        var atlas = MakeAtlas(records);
        var spec = MakeRefinementSpec();
        var outDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try
        {
            var manifest = BridgeValueExporter.Export(atlas, spec, "/tmp/atlas.json", outDir, MakeProvenance());
            Assert.Equal(records.Select(r => r.StateArtifactRef).ToArray(), manifest.SourceStateArtifactRefs);
        }
        finally
        {
            if (Directory.Exists(outDir)) Directory.Delete(outDir, recursive: true);
        }
    }

    [Fact]
    public void Export_RefinementTable_HasOneLevelPerSpecLevel()
    {
        var atlas = MakeAtlas([MakeRecord("bg-001")]);
        var spec = MakeRefinementSpec(levelIds: ["coarse", "medium", "fine"]);
        var outDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try
        {
            BridgeValueExporter.Export(atlas, spec, "/tmp/atlas.json", outDir, MakeProvenance());

            var json = File.ReadAllText(Path.Combine(outDir, "refinement_values.json"));
            var table = GuJsonDefaults.Deserialize<RefinementQuantityValueTable>(json);

            Assert.NotNull(table);
            Assert.Equal(3, table.Levels.Count);
            Assert.Equal("coarse", table.Levels[0].LevelId);
            Assert.Equal("medium", table.Levels[1].LevelId);
            Assert.Equal("fine", table.Levels[2].LevelId);
        }
        finally
        {
            if (Directory.Exists(outDir)) Directory.Delete(outDir, recursive: true);
        }
    }

    [Fact]
    public void Export_BranchTable_ContainsExpectedQuantityKeys()
    {
        var atlas = MakeAtlas([MakeRecord("bg-001")]);
        var spec = MakeRefinementSpec();
        var outDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try
        {
            BridgeValueExporter.Export(atlas, spec, "/tmp/atlas.json", outDir, MakeProvenance());

            var json = File.ReadAllText(Path.Combine(outDir, "branch_quantity_values.json"));
            var table = GuJsonDefaults.Deserialize<RefinementQuantityValueTable>(json);

            Assert.NotNull(table);
            Assert.Single(table.Levels);
            var quantities = table.Levels[0].Quantities;
            Assert.True(quantities.ContainsKey("residual-norm"));
            Assert.True(quantities.ContainsKey("stationarity-norm"));
            Assert.True(quantities.ContainsKey("objective-value"));
            Assert.True(quantities.ContainsKey("gauge-violation"));
            Assert.True(quantities.ContainsKey("solver-converged"));
            Assert.True(quantities.ContainsKey("solver-iterations"));
        }
        finally
        {
            if (Directory.Exists(outDir)) Directory.Delete(outDir, recursive: true);
        }
    }

    [Fact]
    public void Export_CampaignArtifactLoader_AcceptsGeneratedFiles()
    {
        // Verify that the generated branch_quantity_values.json and refinement_values.json
        // can be deserialized as RefinementQuantityValueTable (the type that
        // Phase5CampaignArtifactLoader expects for these files).
        var atlas = MakeAtlas([MakeRecord("bg-001"), MakeRecord("bg-002")]);
        var spec = MakeRefinementSpec();
        var outDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try
        {
            BridgeValueExporter.Export(atlas, spec, "/tmp/atlas.json", outDir, MakeProvenance());

            // Simulate what Phase5CampaignArtifactLoader.Load does for BranchQuantityValuesPath
            var branchJson = File.ReadAllText(Path.Combine(outDir, "branch_quantity_values.json"));
            var branchTable = GuJsonDefaults.Deserialize<RefinementQuantityValueTable>(branchJson);
            Assert.NotNull(branchTable);
            Assert.Equal(atlas.AtlasId, branchTable.StudyId);
            Assert.Equal(2, branchTable.Levels.Count);

            // Simulate what loader does for RefinementValuesPath
            var refinementJson = File.ReadAllText(Path.Combine(outDir, "refinement_values.json"));
            var refinementTable = GuJsonDefaults.Deserialize<RefinementQuantityValueTable>(refinementJson);
            Assert.NotNull(refinementTable);
            Assert.Equal(spec.StudyId, refinementTable.StudyId);
            Assert.Equal(spec.RefinementLevels.Count, refinementTable.Levels.Count);
        }
        finally
        {
            if (Directory.Exists(outDir)) Directory.Delete(outDir, recursive: true);
        }
    }

    [Fact]
    public void Export_EmptyAtlas_ProducesEmptyBranchTable()
    {
        var atlas = MakeAtlas([]);
        var spec = MakeRefinementSpec();
        var outDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try
        {
            var manifest = BridgeValueExporter.Export(atlas, spec, "/tmp/atlas.json", outDir, MakeProvenance());

            Assert.Empty(manifest.SourceRecordIds);
            Assert.Empty(manifest.DerivedVariantIds);

            var json = File.ReadAllText(Path.Combine(outDir, "branch_quantity_values.json"));
            var table = GuJsonDefaults.Deserialize<RefinementQuantityValueTable>(json);
            Assert.NotNull(table);
            Assert.Empty(table.Levels);
        }
        finally
        {
            if (Directory.Exists(outDir)) Directory.Delete(outDir, recursive: true);
        }
    }
}
