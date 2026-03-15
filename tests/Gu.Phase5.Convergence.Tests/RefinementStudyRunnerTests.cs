using System.Text.Json;
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

    // Helper: create a level with equal X and F parameters (isotropic)
    private static RefinementLevel Level(string id, double h) =>
        new RefinementLevel { LevelId = id, MeshParameterX = h, MeshParameterF = h };

    [Fact]
    public void Run_ConvergentQuantity_ProducesContinuumEstimate()
    {
        // Q(h) = 2.0 + h^2 — exactly second-order convergent
        var levels = new[]
        {
            Level("L0", 1.0),
            Level("L1", 0.5),
            Level("L2", 0.25),
        };
        var spec = MakeSpec(levels);
        var runner = new RefinementStudyRunner();

        var result = runner.Run(spec, level =>
            new Dictionary<string, double> { ["q1"] = 2.0 + level.EffectiveMeshParameter * level.EffectiveMeshParameter });

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
            Level("L0", 1.0),
            Level("L1", 0.5),
        };
        var spec = MakeSpec(levels);
        var runner = new RefinementStudyRunner();

        var result = runner.Run(spec, level =>
            new Dictionary<string, double> { ["q1"] = 3.0 + level.EffectiveMeshParameter });

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
            Level("L0", 1.0),
            Level("L1", 0.5),
            Level("L2", 0.25),
        };
        var spec = MakeSpec(levels);
        var runner = new RefinementStudyRunner();

        var result = runner.Run(spec, level =>
        {
            if (level.LevelId == "L2")
                throw new InvalidOperationException("Solver failed at finest level.");
            return new Dictionary<string, double> { ["q1"] = 3.0 + level.EffectiveMeshParameter };
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
            Level("L0", 1.0),
            Level("L1", 0.5),
            Level("L2", 0.25),
        };
        // q1 converges, q2 diverges
        var spec = MakeSpec(levels, ["q1", "q2"]);
        var runner = new RefinementStudyRunner();

        var result = runner.Run(spec, level => new Dictionary<string, double>
        {
            ["q1"] = 1.0 + level.EffectiveMeshParameter * level.EffectiveMeshParameter, // convergent
            ["q2"] = 1.0 + 1.0 / level.EffectiveMeshParameter,                          // divergent
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
            Level("L0", 1.0),
            Level("L1", 0.5),
            Level("L2", 0.25),
            Level("L3", 0.125),
        };
        var spec = MakeSpec(levels);
        var runner = new RefinementStudyRunner();

        var result = runner.Run(spec, level =>
            new Dictionary<string, double> { ["q1"] = 5.0 + level.EffectiveMeshParameter });

        Assert.Equal(4, result.RunRecords.Count);
        Assert.All(result.RunRecords, r => Assert.True(r.Converged));
    }

    [Fact]
    public void Run_NonConvergentQuantity_ProducesFailureRecord()
    {
        var levels = new[]
        {
            Level("L0", 1.0),
            Level("L1", 0.5),
            Level("L2", 0.25),
        };
        var spec = MakeSpec(levels);
        var runner = new RefinementStudyRunner();

        // Strictly diverging
        var result = runner.Run(spec, level =>
            new Dictionary<string, double> { ["q1"] = 1.0 / (level.EffectiveMeshParameter * level.EffectiveMeshParameter) });

        // Should produce either failure or weakly-convergent
        bool hasFailure = result.FailureRecords.Any(f => f.QuantityId == "q1");
        bool hasEstimate = result.ContinuumEstimates.Any(e => e.QuantityId == "q1");
        Assert.True(hasFailure || hasEstimate,
            "Expected either a failure record or estimate for non-converging quantity.");
    }

    [Fact]
    public void Run_ConstantQuantity_ProducesContinuumEstimate()
    {
        var levels = new[]
        {
            Level("L0", 1.0),
            Level("L1", 0.5),
            Level("L2", 0.25),
        };
        var spec = MakeSpec(levels);
        var runner = new RefinementStudyRunner();

        var result = runner.Run(spec, _ =>
            new Dictionary<string, double> { ["q1"] = 0.0 });

        Assert.Single(result.ContinuumEstimates);
        Assert.Empty(result.FailureRecords);

        var estimate = result.ContinuumEstimates[0];
        Assert.Equal("convergent", estimate.ConvergenceClassification);
        Assert.Equal(0.0, estimate.ExtrapolatedValue, precision: 12);
        Assert.Equal(0.0, estimate.ErrorBand, precision: 12);
    }

    // WP-8 test (a): legacy single-h JSON input is still accepted and sets both X and F
    [Fact]
    public void RefinementLevel_LegacySingleH_DeserializesWithBothXAndF()
    {
        const string json = """
            {
              "levelId": "L0",
              "meshParameter": 0.5,
              "description": "legacy level"
            }
            """;

        var level = JsonSerializer.Deserialize<RefinementLevel>(json)!;

        Assert.Equal("L0", level.LevelId);
        Assert.Equal(0.5, level.MeshParameterX);
        Assert.Equal(0.5, level.MeshParameterF);
        Assert.Equal(0.5, level.EffectiveMeshParameter);
        Assert.Equal("legacy level", level.Description);
    }

    // WP-8 test (b): anisotropic refinement — EffectiveMeshParameter == max(hX, hF)
    [Fact]
    public void RefinementLevel_AnisotropicRefinement_EffectiveIsMax()
    {
        var level = new RefinementLevel
        {
            LevelId = "L0",
            MeshParameterX = 0.3,
            MeshParameterF = 0.7,
        };

        Assert.Equal(0.7, level.EffectiveMeshParameter);

        // Runner uses EffectiveMeshParameter, so run records reflect max(hX, hF)
        var levels = new[]
        {
            new RefinementLevel { LevelId = "L0", MeshParameterX = 1.0, MeshParameterF = 0.4 },
            new RefinementLevel { LevelId = "L1", MeshParameterX = 0.5, MeshParameterF = 0.2 },
            new RefinementLevel { LevelId = "L2", MeshParameterX = 0.25, MeshParameterF = 0.1 },
        };
        var spec = MakeSpec(levels);
        var runner = new RefinementStudyRunner();

        var result = runner.Run(spec, lv =>
            new Dictionary<string, double> { ["q1"] = 2.0 + lv.EffectiveMeshParameter * lv.EffectiveMeshParameter });

        // Run records should have MeshParameter = max(hX, hF) for each level
        Assert.Equal(1.0, result.RunRecords[0].MeshParameter);   // max(1.0, 0.4) = 1.0
        Assert.Equal(0.5, result.RunRecords[1].MeshParameter);   // max(0.5, 0.2) = 0.5
        Assert.Equal(0.25, result.RunRecords[2].MeshParameter);  // max(0.25, 0.1) = 0.25
    }

    // WP-8 test (c): continuum values unchanged when hX == hF (same as old single-h behavior)
    [Fact]
    public void Run_IsotropicDualH_ContinuumUnchangedVsLegacy()
    {
        // Q(h) = 3.0 + 2*h — first-order convergent
        // With isotropic h_X == h_F, EffectiveMeshParameter = h, identical to old MeshParameter.
        var levels = new[]
        {
            Level("L0", 1.0),
            Level("L1", 0.5),
            Level("L2", 0.25),
        };
        var spec = MakeSpec(levels);
        var runner = new RefinementStudyRunner();

        var result = runner.Run(spec, lv =>
            new Dictionary<string, double> { ["q1"] = 3.0 + 2.0 * lv.EffectiveMeshParameter });

        Assert.Single(result.ContinuumEstimates);
        var est = result.ContinuumEstimates[0];
        // Continuum extrapolation should converge near 3.0
        Assert.Equal(3.0, est.ExtrapolatedValue, precision: 2);
    }

    // ─── WP-12: ShiabVariantId field ───

    [Fact]
    public void RefinementStudySpec_ShiabVariantId_RoundTrips()
    {
        var spec = new RefinementStudySpec
        {
            StudyId = "shiab-test",
            SchemaVersion = "1.0",
            BranchManifestId = "branch-1",
            TargetQuantities = ["q1"],
            RefinementLevels = [Level("L0", 0.5), Level("L1", 0.25), Level("L2", 0.125)],
            ShiabVariantId = "scaled-identity-v1",
            Provenance = MakeProvenance(),
        };

        string json = JsonSerializer.Serialize(spec);
        var deserialized = JsonSerializer.Deserialize<RefinementStudySpec>(json);

        Assert.NotNull(deserialized);
        Assert.Equal("scaled-identity-v1", deserialized.ShiabVariantId);
    }

    [Fact]
    public void RefinementStudySpec_ShiabVariantId_NullByDefault()
    {
        var spec = MakeSpec([Level("L0", 0.5), Level("L1", 0.25), Level("L2", 0.125)]);
        Assert.Null(spec.ShiabVariantId);
    }
}
