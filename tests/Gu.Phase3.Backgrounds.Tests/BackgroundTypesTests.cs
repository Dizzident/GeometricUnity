using Gu.Core;
using Gu.Core.Serialization;
using Gu.Phase3.Backgrounds;
using Gu.Solvers;

namespace Gu.Phase3.Backgrounds.Tests;

public class BackgroundTypesTests
{
    [Fact]
    public void BackgroundSpec_CanConstruct()
    {
        var spec = TestHelpers.MakeSpec("spec-1");
        Assert.Equal("spec-1", spec.SpecId);
        Assert.Equal("env-1", spec.EnvironmentId);
        Assert.Equal("branch-1", spec.BranchManifestId);
    }

    [Fact]
    public void BackgroundRecord_CanConstruct()
    {
        var record = TestHelpers.MakeRecord("bg-1", AdmissibilityLevel.B1);
        Assert.Equal("bg-1", record.BackgroundId);
        Assert.Equal(AdmissibilityLevel.B1, record.AdmissibilityLevel);
    }

    [Fact]
    public void BackgroundMetrics_CanConstruct()
    {
        var metrics = new BackgroundMetrics
        {
            ResidualNorm = 1e-5,
            StationarityNorm = 1e-7,
            ObjectiveValue = 5e-11,
            GaugeViolation = 1e-12,
            SolverIterations = 50,
            SolverConverged = true,
            TerminationReason = "gradient-converged",
            GaussNewtonValid = true,
        };
        Assert.Equal(1e-5, metrics.ResidualNorm);
        Assert.True(metrics.SolverConverged);
        Assert.True(metrics.GaussNewtonValid);
    }

    [Fact]
    public void BackgroundSolveOptions_ToSolverOptions()
    {
        var options = new BackgroundSolveOptions
        {
            SolveMode = SolveMode.StationaritySolve,
            SolverMethod = SolverMethod.GaussNewton,
            MaxIterations = 500,
            GaugePenaltyLambda = 0.1,
        };
        var solverOpts = options.ToSolverOptions();
        Assert.Equal(SolveMode.StationaritySolve, solverOpts.Mode);
        Assert.Equal(SolverMethod.GaussNewton, solverOpts.Method);
        Assert.Equal(500, solverOpts.MaxIterations);
        Assert.Equal(0.1, solverOpts.GaugePenaltyLambda);
    }

    [Fact]
    public void BackgroundStudySpec_CanConstruct()
    {
        var study = new BackgroundStudySpec
        {
            StudyId = "study-1",
            Specs = new[] { TestHelpers.MakeSpec("s1"), TestHelpers.MakeSpec("s2") },
        };
        Assert.Equal("study-1", study.StudyId);
        Assert.Equal(2, study.Specs.Count);
        Assert.Equal("admissibility-then-residual", study.RankingCriteria);
    }

    [Fact]
    public void BackgroundAtlas_CanConstruct()
    {
        var atlas = new BackgroundAtlas
        {
            AtlasId = "atlas-1",
            StudyId = "study-1",
            Backgrounds = new[] { TestHelpers.MakeRecord("bg-1", AdmissibilityLevel.B2) },
            RejectedBackgrounds = new[] { TestHelpers.MakeRecord("bg-rej", AdmissibilityLevel.Rejected) },
            RankingCriteria = "admissibility-then-residual",
            TotalAttempts = 2,
            Provenance = TestHelpers.MakeProvenance(),
            AdmissibilityCounts = new Dictionary<string, int>
            {
                ["B2"] = 1,
                ["Rejected"] = 1,
            },
        };
        Assert.Single(atlas.Backgrounds);
        Assert.Single(atlas.RejectedBackgrounds);
        Assert.Equal(2, atlas.TotalAttempts);
    }

    [Fact]
    public void BackgroundSeed_TrivialKind()
    {
        var seed = new BackgroundSeed { Kind = BackgroundSeedKind.Trivial };
        Assert.Equal(BackgroundSeedKind.Trivial, seed.Kind);
        Assert.Null(seed.InitialState);
    }

    [Fact]
    public void BackgroundSeed_ExplicitKindWithState()
    {
        var state = TestHelpers.MakeFieldTensor(10);
        var seed = new BackgroundSeed
        {
            Kind = BackgroundSeedKind.Explicit,
            InitialState = state,
            Label = "test-explicit",
        };
        Assert.Equal(BackgroundSeedKind.Explicit, seed.Kind);
        Assert.NotNull(seed.InitialState);
    }

    [Fact]
    public void BackgroundSeed_ContinuationKind()
    {
        var seed = new BackgroundSeed
        {
            Kind = BackgroundSeedKind.Continuation,
            ContinuationSourceId = "bg-prev-1",
        };
        Assert.Equal("bg-prev-1", seed.ContinuationSourceId);
    }
}
