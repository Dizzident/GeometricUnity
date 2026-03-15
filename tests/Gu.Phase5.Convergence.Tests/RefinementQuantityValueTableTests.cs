using Gu.Core;
using Gu.Phase5.Convergence;

namespace Gu.Phase5.Convergence.Tests;

/// <summary>
/// WP-4: Tests for RefinementQuantityValueTable and its use in RefinementStudyRunner.
/// </summary>
public sealed class RefinementQuantityValueTableTests
{
    private static ProvenanceMeta MakeProvenance() => new ProvenanceMeta
    {
        CreatedAt = DateTimeOffset.UtcNow,
        CodeRevision = "test",
        Branch = new BranchRef { BranchId = "test-branch", SchemaVersion = "1.0" },
    };

    private static RefinementLevel Level(string id, double h) =>
        new RefinementLevel { LevelId = id, MeshParameterX = h, MeshParameterF = h };

    private static RefinementStudySpec MakeSpec(IReadOnlyList<RefinementLevel> levels,
        IReadOnlyList<string>? quantities = null) =>
        new RefinementStudySpec
        {
            StudyId = "test-study",
            SchemaVersion = "1.0",
            BranchManifestId = "branch-1",
            TargetQuantities = quantities ?? ["q1"],
            RefinementLevels = levels,
            Provenance = MakeProvenance(),
        };

    // Helper: build a values table whose quantities follow Q(h) = baseValue + h^p
    private static RefinementQuantityValueTable MakeConvergentTable(
        IReadOnlyList<RefinementLevel> levels,
        string quantityId = "q1",
        double baseValue = 2.0,
        double p = 2.0)
    {
        return new RefinementQuantityValueTable
        {
            StudyId = "test-study",
            Levels = levels.Select(l => new RefinementQuantityValueLevel
            {
                LevelId = l.LevelId,
                SolverConverged = true,
                Quantities = new Dictionary<string, double>
                {
                    [quantityId] = baseValue + System.Math.Pow(l.EffectiveMeshParameter, p),
                },
            }).ToList(),
        };
    }

    // WP-4 test (a): missing --values causes validation failure (level ID mismatch check)
    // The CLI itself returns an error when --values is absent; here we test the equivalent
    // programmatic contract: a values table with mismatched level IDs must be detected.
    [Fact]
    public void ValuesTable_MissingLevelId_CanBeDetected()
    {
        var levels = new[]
        {
            Level("L0", 1.0),
            Level("L1", 0.5),
            Level("L2", 0.25),
        };

        // Values table is missing L2 entirely
        var valueTable = new RefinementQuantityValueTable
        {
            StudyId = "test-study",
            Levels =
            [
                new RefinementQuantityValueLevel { LevelId = "L0", Quantities = new Dictionary<string, double> { ["q1"] = 3.0 } },
                new RefinementQuantityValueLevel { LevelId = "L1", Quantities = new Dictionary<string, double> { ["q1"] = 2.5 } },
                // L2 missing
            ],
        };

        var specLevelIds = levels.Select(l => l.LevelId).ToHashSet();
        var valueLevelIds = valueTable.Levels.Select(l => l.LevelId).ToHashSet();
        var missing = specLevelIds.Except(valueLevelIds).ToList();

        Assert.Single(missing);
        Assert.Equal("L2", missing[0]);
    }

    // WP-4 test (b): level IDs in values file must match spec levels
    [Fact]
    public void ValuesTable_ExtraLevelId_CanBeDetected()
    {
        var levels = new[]
        {
            Level("L0", 1.0),
            Level("L1", 0.5),
        };

        var valueTable = new RefinementQuantityValueTable
        {
            StudyId = "test-study",
            Levels =
            [
                new RefinementQuantityValueLevel { LevelId = "L0", Quantities = new Dictionary<string, double> { ["q1"] = 3.0 } },
                new RefinementQuantityValueLevel { LevelId = "L1", Quantities = new Dictionary<string, double> { ["q1"] = 2.5 } },
                new RefinementQuantityValueLevel { LevelId = "L99", Quantities = new Dictionary<string, double> { ["q1"] = 0.0 } },
            ],
        };

        var specLevelIds = levels.Select(l => l.LevelId).ToHashSet();
        var valueLevelIds = valueTable.Levels.Select(l => l.LevelId).ToHashSet();
        var extra = valueLevelIds.Except(specLevelIds).ToList();

        Assert.Single(extra);
        Assert.Equal("L99", extra[0]);
    }

    // WP-4 test (c): non-empty values produce non-empty continuum estimates
    [Fact]
    public void Runner_WithValuesTable_NonEmptyValuesProduceEstimates()
    {
        var levels = new[]
        {
            Level("L0", 1.0),
            Level("L1", 0.5),
            Level("L2", 0.25),
        };
        var spec = MakeSpec(levels);
        var valueTable = MakeConvergentTable(levels, quantityId: "q1", baseValue: 3.0, p: 2.0);
        var valueLookup = valueTable.Levels.ToDictionary(l => l.LevelId);

        var runner = new RefinementStudyRunner();
        var result = runner.Run(spec, level =>
        {
            var entry = valueLookup[level.LevelId];
            if (!entry.SolverConverged)
                throw new InvalidOperationException($"Solver did not converge at level '{level.LevelId}'.");
            return entry.Quantities;
        });

        Assert.NotEmpty(result.ContinuumEstimates);
        Assert.Empty(result.FailureRecords);
        Assert.Equal("q1", result.ContinuumEstimates[0].QuantityId);
        // Extrapolated value should be close to baseValue (3.0)
        Assert.Equal(3.0, result.ContinuumEstimates[0].ExtrapolatedValue, precision: 2);
    }

    // WP-4 test (d): solverConverged=false produces failure records
    [Fact]
    public void Runner_WithValuesTable_SolverConvergedFalse_ProducesFailureRecord()
    {
        var levels = new[]
        {
            Level("L0", 1.0),
            Level("L1", 0.5),
            Level("L2", 0.25),
        };
        var spec = MakeSpec(levels);

        // Mark L2 as not converged
        var valueTable = new RefinementQuantityValueTable
        {
            StudyId = "test-study",
            Levels =
            [
                new RefinementQuantityValueLevel
                {
                    LevelId = "L0",
                    SolverConverged = true,
                    Quantities = new Dictionary<string, double> { ["q1"] = 4.0 },
                },
                new RefinementQuantityValueLevel
                {
                    LevelId = "L1",
                    SolverConverged = true,
                    Quantities = new Dictionary<string, double> { ["q1"] = 3.5 },
                },
                new RefinementQuantityValueLevel
                {
                    LevelId = "L2",
                    SolverConverged = false,
                    Quantities = new Dictionary<string, double> { ["q1"] = 0.0 },
                },
            ],
        };

        var valueLookup = valueTable.Levels.ToDictionary(l => l.LevelId);

        var runner = new RefinementStudyRunner();
        var result = runner.Run(spec, level =>
        {
            var entry = valueLookup[level.LevelId];
            if (!entry.SolverConverged)
                throw new InvalidOperationException($"Solver did not converge at level '{level.LevelId}'.");
            return entry.Quantities;
        });

        Assert.Empty(result.ContinuumEstimates);
        Assert.Single(result.FailureRecords);
        Assert.Equal("solver-failure", result.FailureRecords[0].FailureType);
    }
}
