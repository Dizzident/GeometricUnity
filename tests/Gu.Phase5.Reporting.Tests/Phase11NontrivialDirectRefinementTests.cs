using Gu.Core;
using Gu.Core.Serialization;
using Gu.Phase3.Backgrounds;
using Gu.Phase5.Convergence;
using Gu.Phase5.Reporting;

namespace Gu.Phase5.Reporting.Tests;

/// <summary>
/// P11-M3: Tests for the nontrivial direct solver-backed refinement path.
///
/// These tests verify:
/// 1. The nontrivial ladder exports non-constant values across refinement levels.
/// 2. The evidence manifest records evidenceSource = "direct-solver-backed".
/// 3. The nontrivial ladder is explicitly distinguishable from the zero-invariant control ladder.
/// 4. The evidence type distinction (direct-control vs direct-nontrivial vs bridge-derived)
///    is structurally preserved through RefinementEvidenceManifest.
/// </summary>
public sealed class Phase11NontrivialDirectRefinementTests
{
    private static ProvenanceMeta MakeProvenance() => new ProvenanceMeta
    {
        CreatedAt = new DateTimeOffset(2026, 3, 15, 0, 0, 0, TimeSpan.Zero),
        CodeRevision = "phase11-nontrivial-direct-refinement-v1",
        Branch = new BranchRef { BranchId = "phase8-real-atlas-control", SchemaVersion = "1.0" },
        Backend = "cpu",
    };

    /// <summary>
    /// Creates a nontrivial background record with nonzero residual and nontrivial gauge violation.
    /// These represent GaussNewton+SymmetricAnsatz backgrounds admitted at B1 or B2.
    /// </summary>
    private static BackgroundRecord MakeNontrivialRecord(
        string backgroundId, double residual, double gaugeViolation, int iterations) =>
        new BackgroundRecord
        {
            BackgroundId = backgroundId,
            EnvironmentId = "env-refinement",
            BranchManifestId = "phase8-real-atlas-control",
            GeometryFingerprint = "simplicial-X_h-Y_h-centroid-P1",
            StateArtifactRef = $"states/{backgroundId}.json",
            ResidualNorm = residual,
            StationarityNorm = residual * 1.7,
            AdmissibilityLevel = AdmissibilityLevel.B2,
            Metrics = new BackgroundMetrics
            {
                ResidualNorm = residual,
                StationarityNorm = residual * 1.7,
                ObjectiveValue = residual * residual * 0.5,
                GaugeViolation = gaugeViolation,
                SolverIterations = iterations,
                SolverConverged = true,
                TerminationReason = "Objective below tolerance",
                GaussNewtonValid = true,
            },
            ReplayTierAchieved = "R2",
            Provenance = MakeProvenance(),
        };

    /// <summary>
    /// Creates a zero-invariant control record (trivial zero solution).
    /// Residual = 0, gauge violation = 0, solver iterations = 0.
    /// </summary>
    private static BackgroundRecord MakeControlRecord(string backgroundId) =>
        new BackgroundRecord
        {
            BackgroundId = backgroundId,
            EnvironmentId = "env-refinement",
            BranchManifestId = "phase8-real-atlas-control",
            GeometryFingerprint = "simplicial-X_h-Y_h-centroid-P1",
            StateArtifactRef = $"states/{backgroundId}.json",
            ResidualNorm = 0.0,
            StationarityNorm = 0.0,
            AdmissibilityLevel = AdmissibilityLevel.B2,
            Metrics = new BackgroundMetrics
            {
                ResidualNorm = 0.0,
                StationarityNorm = 0.0,
                ObjectiveValue = 0.0,
                GaugeViolation = 0.0,
                SolverIterations = 0,
                SolverConverged = true,
                TerminationReason = "Objective below tolerance",
                GaussNewtonValid = true,
            },
            ReplayTierAchieved = "R2",
            Provenance = MakeProvenance(),
        };

    private static RefinementStudySpec MakeNontrivialSpec() => new RefinementStudySpec
    {
        StudyId = "phase11-nontrivial-direct-refinement-ladder",
        SchemaVersion = "1.0",
        BranchManifestId = "phase8-real-atlas-control",
        TargetQuantities = ["residual-norm", "stationarity-norm", "objective-value", "gauge-violation", "solver-iterations"],
        RefinementLevels =
        [
            new RefinementLevel { LevelId = "L0-2x2", MeshParameterX = 1.0, MeshParameterF = 1.0 },
            new RefinementLevel { LevelId = "L1-4x4", MeshParameterX = 0.5, MeshParameterF = 0.5 },
            new RefinementLevel { LevelId = "L2-8x8", MeshParameterX = 0.25, MeshParameterF = 0.25 },
        ],
        Provenance = MakeProvenance(),
    };

    [Fact]
    public void NontrivialLadder_ExportsNonConstantValuesAcrossLevels()
    {
        // Arrange: nontrivial GaussNewton+SymmetricAnsatz backgrounds with increasing residuals
        // (coarser meshes converge better; finer meshes have larger residuals from nontrivial seeds)
        var spec = MakeNontrivialSpec();
        var records = new Dictionary<string, BackgroundRecord>
        {
            ["L0-2x2"] = MakeNontrivialRecord("bg-nt-l0", residual: 1.24e-9, gaugeViolation: 0.197, iterations: 2),
            ["L1-4x4"] = MakeNontrivialRecord("bg-nt-l1", residual: 4.04e-8, gaugeViolation: 0.422, iterations: 2),
            ["L2-8x8"] = MakeNontrivialRecord("bg-nt-l2", residual: 1.06e-7, gaugeViolation: 0.996, iterations: 2),
        };
        var artifactRefs = new Dictionary<string, string>
        {
            ["L0-2x2"] = "/tmp/bg-nt-l0.json",
            ["L1-4x4"] = "/tmp/bg-nt-l1.json",
            ["L2-8x8"] = "/tmp/bg-nt-l2.json",
        };

        var outDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try
        {
            DirectRefinementValueExporter.Export(spec, records, artifactRefs, outDir, MakeProvenance(),
                notes: "Phase XI nontrivial direct solver-backed refinement ladder.");

            var table = GuJsonDefaults.Deserialize<RefinementQuantityValueTable>(
                File.ReadAllText(Path.Combine(outDir, "refinement_values.json")));

            Assert.NotNull(table);
            Assert.Equal(3, table!.Levels.Count);

            // All levels must have converged
            Assert.All(table.Levels, level => Assert.True(level.SolverConverged));

            // Residual norms must be non-constant (nontrivial across levels)
            var residuals = table.Levels.Select(l => l.Quantities["residual-norm"]).ToArray();
            Assert.True(residuals.Distinct().Count() > 1,
                "Nontrivial direct refinement ladder must have non-constant residual-norm values across levels.");

            // All residuals must be nonzero (not a control ladder)
            Assert.All(residuals, r => Assert.True(r > 0,
                "Nontrivial direct refinement ladder must have nonzero residuals at all levels."));

            // Gauge violations must be non-constant
            var gaugeViolations = table.Levels.Select(l => l.Quantities["gauge-violation"]).ToArray();
            Assert.True(gaugeViolations.Distinct().Count() > 1,
                "Nontrivial direct refinement ladder must have non-constant gauge-violation values across levels.");
        }
        finally
        {
            if (Directory.Exists(outDir)) Directory.Delete(outDir, recursive: true);
        }
    }

    [Fact]
    public void NontrivialLadder_EvidenceManifestRecordsDirectSolverBacked()
    {
        var spec = MakeNontrivialSpec();
        var records = new Dictionary<string, BackgroundRecord>
        {
            ["L0-2x2"] = MakeNontrivialRecord("bg-nt-l0", 1.24e-9, 0.197, 2),
            ["L1-4x4"] = MakeNontrivialRecord("bg-nt-l1", 4.04e-8, 0.422, 2),
            ["L2-8x8"] = MakeNontrivialRecord("bg-nt-l2", 1.06e-7, 0.996, 2),
        };
        var artifactRefs = records.ToDictionary(kv => kv.Key, kv => $"/tmp/{kv.Value.BackgroundId}.json");

        var outDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try
        {
            var manifest = DirectRefinementValueExporter.Export(spec, records, artifactRefs, outDir, MakeProvenance(),
                notes: "Phase XI nontrivial direct solver-backed refinement ladder.");

            Assert.Equal("direct-solver-backed", manifest.EvidenceSource);
            Assert.Equal(3, manifest.SourceRecordIds.Count);
            Assert.Contains("bg-nt-l0", manifest.SourceRecordIds);
            Assert.Contains("bg-nt-l1", manifest.SourceRecordIds);
            Assert.Contains("bg-nt-l2", manifest.SourceRecordIds);
        }
        finally
        {
            if (Directory.Exists(outDir)) Directory.Delete(outDir, recursive: true);
        }
    }

    [Fact]
    public void ControlLadder_HasConstantZeroValues_DistinctFromNontrivialLadder()
    {
        // Confirm that the zero-invariant control ladder (trivial seed) is structurally
        // distinguishable from the nontrivial ladder by all values being constant zero.
        var spec = MakeNontrivialSpec();
        var controlRecords = new Dictionary<string, BackgroundRecord>
        {
            ["L0-2x2"] = MakeControlRecord("bg-ctrl-l0"),
            ["L1-4x4"] = MakeControlRecord("bg-ctrl-l1"),
            ["L2-8x8"] = MakeControlRecord("bg-ctrl-l2"),
        };
        var artifactRefs = controlRecords.ToDictionary(kv => kv.Key, kv => $"/tmp/{kv.Value.BackgroundId}.json");

        var outDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try
        {
            DirectRefinementValueExporter.Export(spec, controlRecords, artifactRefs, outDir, MakeProvenance(),
                notes: "Phase X zero-invariant direct control ladder.");

            var table = GuJsonDefaults.Deserialize<RefinementQuantityValueTable>(
                File.ReadAllText(Path.Combine(outDir, "refinement_values.json")));

            Assert.NotNull(table);

            // Control ladder: all residuals are exactly zero
            var residuals = table!.Levels.Select(l => l.Quantities["residual-norm"]).ToArray();
            Assert.All(residuals, r => Assert.Equal(0.0, r));

            // Control ladder: all gauge violations are exactly zero
            var gaugeViolations = table.Levels.Select(l => l.Quantities["gauge-violation"]).ToArray();
            Assert.All(gaugeViolations, g => Assert.Equal(0.0, g));

            // Control ladder: constant values across levels (zero-invariant)
            Assert.True(residuals.Distinct().Count() == 1, "Control ladder has constant (zero) residuals.");
        }
        finally
        {
            if (Directory.Exists(outDir)) Directory.Delete(outDir, recursive: true);
        }
    }

    [Fact]
    public void DirectEvidenceManifest_StudyId_MatchesRefinementSpec()
    {
        var spec = MakeNontrivialSpec();
        var records = new Dictionary<string, BackgroundRecord>
        {
            ["L0-2x2"] = MakeNontrivialRecord("bg-nt-l0", 1.24e-9, 0.197, 2),
            ["L1-4x4"] = MakeNontrivialRecord("bg-nt-l1", 4.04e-8, 0.422, 2),
            ["L2-8x8"] = MakeNontrivialRecord("bg-nt-l2", 1.06e-7, 0.996, 2),
        };
        var artifactRefs = records.ToDictionary(kv => kv.Key, kv => $"/tmp/{kv.Value.BackgroundId}.json");

        var outDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try
        {
            var manifest = DirectRefinementValueExporter.Export(spec, records, artifactRefs, outDir, MakeProvenance());
            Assert.Equal(spec.StudyId, manifest.StudyId);
        }
        finally
        {
            if (Directory.Exists(outDir)) Directory.Delete(outDir, recursive: true);
        }
    }
}
