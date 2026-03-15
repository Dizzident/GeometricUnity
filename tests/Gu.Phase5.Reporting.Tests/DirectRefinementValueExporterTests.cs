using Gu.Core;
using Gu.Core.Serialization;
using Gu.Phase3.Backgrounds;
using Gu.Phase5.Convergence;
using Gu.Phase5.Reporting;

namespace Gu.Phase5.Reporting.Tests;

public sealed class DirectRefinementValueExporterTests
{
    private static ProvenanceMeta MakeProvenance() => new ProvenanceMeta
    {
        CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
        CodeRevision = "phase10-test",
        Branch = new BranchRef { BranchId = "phase10-branch", SchemaVersion = "1.0" },
        Backend = "cpu",
    };

    private static BackgroundRecord MakeRecord(string backgroundId, double residual, int iterations) => new BackgroundRecord
    {
        BackgroundId = backgroundId,
        EnvironmentId = "env-refinement",
        BranchManifestId = "phase10-branch",
        GeometryFingerprint = "structured-fiber-bundle",
        StateArtifactRef = $"states/{backgroundId}.json",
        ResidualNorm = residual,
        StationarityNorm = residual * 0.5,
        AdmissibilityLevel = AdmissibilityLevel.B2,
        Metrics = new BackgroundMetrics
        {
            ResidualNorm = residual,
            StationarityNorm = residual * 0.5,
            ObjectiveValue = residual * residual,
            GaugeViolation = residual * 10.0,
            SolverIterations = iterations,
            SolverConverged = true,
            TerminationReason = "Objective below tolerance",
            GaussNewtonValid = true,
        },
        ReplayTierAchieved = "R2",
        Provenance = MakeProvenance(),
    };

    [Fact]
    public void Export_WritesRefinementValuesAndDirectEvidenceManifest()
    {
        var spec = new RefinementStudySpec
        {
            StudyId = "direct-refinement-test",
            SchemaVersion = "1.0",
            BranchManifestId = "phase10-branch",
            TargetQuantities = ["residual-norm", "stationarity-norm", "objective-value", "gauge-violation", "solver-iterations"],
            RefinementLevels =
            [
                new RefinementLevel { LevelId = "L0-2x2", MeshParameterX = 1.0, MeshParameterF = 1.0 },
                new RefinementLevel { LevelId = "L1-4x4", MeshParameterX = 0.5, MeshParameterF = 0.5 },
                new RefinementLevel { LevelId = "L2-8x8", MeshParameterX = 0.25, MeshParameterF = 0.25 },
            ],
            Provenance = MakeProvenance(),
        };

        var outDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try
        {
            DirectRefinementValueExporter.Export(
                spec,
                new Dictionary<string, BackgroundRecord>
                {
                    ["L0-2x2"] = MakeRecord("bg-L0", 1e-3, 32),
                    ["L1-4x4"] = MakeRecord("bg-L1", 4e-4, 18),
                    ["L2-8x8"] = MakeRecord("bg-L2", 1e-4, 11),
                },
                new Dictionary<string, string>
                {
                    ["L0-2x2"] = "/tmp/bg-L0.json",
                    ["L1-4x4"] = "/tmp/bg-L1.json",
                    ["L2-8x8"] = "/tmp/bg-L2.json",
                },
                outDir,
                MakeProvenance());

            var table = GuJsonDefaults.Deserialize<RefinementQuantityValueTable>(
                File.ReadAllText(Path.Combine(outDir, "refinement_values.json")));
            Assert.NotNull(table);
            Assert.Equal(3, table!.Levels.Count);
            Assert.Equal(1e-4, table.Levels[2].Quantities["residual-norm"]);

            var manifest = GuJsonDefaults.Deserialize<RefinementEvidenceManifest>(
                File.ReadAllText(Path.Combine(outDir, "refinement_evidence_manifest.json")));
            Assert.NotNull(manifest);
            Assert.Equal("direct-solver-backed", manifest!.EvidenceSource);
            Assert.Equal(new[] { "bg-L0", "bg-L1", "bg-L2" }, manifest.SourceRecordIds);
        }
        finally
        {
            if (Directory.Exists(outDir))
                Directory.Delete(outDir, recursive: true);
        }
    }
}
