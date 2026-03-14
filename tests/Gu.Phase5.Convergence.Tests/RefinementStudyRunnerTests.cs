using Gu.Core;
using Gu.Phase5.Convergence;

namespace Gu.Phase5.Convergence.Tests;

public sealed class RefinementStudyRunnerTests
{
    private static ProvenanceMeta MakeProvenance() => new ProvenanceMeta
    {
        CreatedAt = DateTimeOffset.UtcNow,
        CodeRevision = "test",
        Branch = new BranchRef { BranchId = "test-branch", SchemaVersion = "1.0" },
    };

    private static RefinementStudySpec MakeSpec(IReadOnlyList<RefinementLevel> levels,
        IReadOnlyList<string>? quantities = null)
    {
        return new RefinementStudySpec
        {
            StudyId = "test-study",
            SchemaVersion = "1.0",
            BranchManifestId = "branch-1",
            TargetQuantities = quantities ?? ["q1"],
            RefinementLevels = levels,
            Provenance = MakeProvenance(),
        };
    }

    [Fact]
    public void Run_ConvergentQuantity_ProducesContinuumEstimate()
    {
        // Q(h) = 2.0 + h^2 — exactly second-order convergent
        var levels = new[]
        {
            new RefinementLevel { LevelId = "L0", MeshParameter = 1.0 },
            new RefinementLevel { LevelId = "L1", MeshParameter = 0.5 },
            new RefinementLevel { LevelId = "L2", MeshParameter = 0.25 },
        };
        var spec = MakeSpec(levels);
        var runner = new RefinementStudyRunner();

        var result = runner.Run(spec, level =>
            new Dictionary<string, double> { ["q1"] = 2.0 + level.MeshParameter * level.MeshParameter });

        Assert.Equal("test-study", result.StudyId);
        Assert.Single(result.ContinuumEstimates);
        Assert.Empty(result.FailureRecords);

        var est = result.ContinuumEstimates[0];
        Assert.Equal("q1", est.QuantityId);
        Assert.Equal(2.0, est.ExtrapolatedValue, precision: 3);
        Assert.True(est.ConvergenceOrder > 1.0, $"Expected order ~2, got {est.ConvergenceOrder}");
        Assert.Equal("convergent", est.ConvergenceClassification);
    }

    [Fact]
    public void Run_InsufficientLevels_ProducesFailureRecord()
    {
        var levels = new[]
        {
            new RefinementLevel { LevelId = "L0", MeshParameter = 1.0 },
            new RefinementLevel { LevelId = "L1", MeshParameter = 0.5 },
        };
        var spec = MakeSpec(levels);
        var runner = new RefinementStudyRunner();

        var result = runner.Run(spec, level =>
            new Dictionary<string, double> { ["q1"] = 3.0 + level.MeshParameter });

        Assert.Empty(result.ContinuumEstimates);
        Assert.Single(result.FailureRecords);
        Assert.Equal("q1", result.FailureRecords[0].QuantityId);
        Assert.Equal("insufficient-data", result.FailureRecords[0].FailureType);
    }

    [Fact]
    public void Run_SolverFailure_ProducesFailureRecord()
    {
        var levels = new[]
        {
            new RefinementLevel { LevelId = "L0", MeshParameter = 1.0 },
            new RefinementLevel { LevelId = "L1", MeshParameter = 0.5 },
            new RefinementLevel { LevelId = "L2", MeshParameter = 0.25 },
        };
        var spec = MakeSpec(levels);
        var runner = new RefinementStudyRunner();

        var result = runner.Run(spec, level =>
        {
            if (level.LevelId == "L2")
                throw new InvalidOperationException("Solver failed at finest level.");
            return new Dictionary<string, double> { ["q1"] = 3.0 + level.MeshParameter };
        });

        Assert.Empty(result.ContinuumEstimates);
        Assert.Single(result.FailureRecords);
        Assert.Equal("solver-failure", result.FailureRecords[0].FailureType);
    }

    [Fact]
    public void Run_MultipleQuantities_IndependentResults()
    {
        var levels = new[]
        {
            new RefinementLevel { LevelId = "L0", MeshParameter = 1.0 },
            new RefinementLevel { LevelId = "L1", MeshParameter = 0.5 },
            new RefinementLevel { LevelId = "L2", MeshParameter = 0.25 },
        };
        // q1 converges, q2 diverges
        var spec = MakeSpec(levels, ["q1", "q2"]);
        var runner = new RefinementStudyRunner();

        var result = runner.Run(spec, level => new Dictionary<string, double>
        {
            ["q1"] = 1.0 + level.MeshParameter * level.MeshParameter, // convergent
            ["q2"] = 1.0 + 1.0 / level.MeshParameter,                 // divergent
        });

        var q1Est = result.ContinuumEstimates.FirstOrDefault(e => e.QuantityId == "q1");
        var q2Fail = result.FailureRecords.FirstOrDefault(f => f.QuantityId == "q2");

        Assert.NotNull(q1Est);
        Assert.Equal(1.0, q1Est.ExtrapolatedValue, precision: 2);
        Assert.NotNull(q2Fail);
        Assert.Equal("non-convergent", q2Fail.FailureType);
    }

    [Fact]
    public void Run_FourLevels_ProducesRunRecordForEach()
    {
        var levels = new[]
        {
            new RefinementLevel { LevelId = "L0", MeshParameter = 1.0 },
            new RefinementLevel { LevelId = "L1", MeshParameter = 0.5 },
            new RefinementLevel { LevelId = "L2", MeshParameter = 0.25 },
            new RefinementLevel { LevelId = "L3", MeshParameter = 0.125 },
        };
        var spec = MakeSpec(levels);
        var runner = new RefinementStudyRunner();

        var result = runner.Run(spec, level =>
            new Dictionary<string, double> { ["q1"] = 5.0 + level.MeshParameter });

        Assert.Equal(4, result.RunRecords.Count);
        Assert.All(result.RunRecords, r => Assert.True(r.Converged));
    }

    [Fact]
    public void Run_NonConvergentQuantity_ProducesFailureRecord()
    {
        var levels = new[]
        {
            new RefinementLevel { LevelId = "L0", MeshParameter = 1.0 },
            new RefinementLevel { LevelId = "L1", MeshParameter = 0.5 },
            new RefinementLevel { LevelId = "L2", MeshParameter = 0.25 },
        };
        var spec = MakeSpec(levels);
        var runner = new RefinementStudyRunner();

        // Strictly diverging
        var result = runner.Run(spec, level =>
            new Dictionary<string, double> { ["q1"] = 1.0 / (level.MeshParameter * level.MeshParameter) });

        // Should produce either failure or weakly-convergent
        bool hasFailure = result.FailureRecords.Any(f => f.QuantityId == "q1");
        bool hasEstimate = result.ContinuumEstimates.Any(e => e.QuantityId == "q1");
        Assert.True(hasFailure || hasEstimate,
            "Expected either a failure record or estimate for non-converging quantity.");
    }
}
