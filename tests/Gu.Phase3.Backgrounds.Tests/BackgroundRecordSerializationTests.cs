using Gu.Core;
using Gu.Core.Serialization;
using Gu.Phase3.Backgrounds;
using Gu.Solvers;

namespace Gu.Phase3.Backgrounds.Tests;

/// <summary>
/// Comprehensive JSON round-trip serialization tests for all Phase III background types.
/// </summary>
public class BackgroundRecordSerializationTests
{
    [Fact]
    public void BackgroundRecord_AllFields_RoundTrip()
    {
        var record = new BackgroundRecord
        {
            BackgroundId = "bg-full",
            EnvironmentId = "env-test",
            BranchManifestId = "branch-test",
            ContinuationCoordinates = new Dictionary<string, double>
            {
                ["lambda"] = 0.5,
                ["kappa"] = 1.2,
            },
            GeometryFingerprint = "simplicial-X_h-Y_h-centroid-P1",
            GaugeChoice = "coulomb",
            StateArtifactRef = "state-bg-full",
            ResidualNorm = 1.23e-8,
            StationarityNorm = 4.56e-10,
            AdmissibilityLevel = AdmissibilityLevel.B2,
            Metrics = new BackgroundMetrics
            {
                ResidualNorm = 1.23e-8,
                StationarityNorm = 4.56e-10,
                ObjectiveValue = 7.56e-17,
                GaugeViolation = 1e-12,
                SolverIterations = 150,
                SolverConverged = true,
                TerminationReason = "gradient-converged",
                GaussNewtonValid = true,
            },
            SolveTraceRef = "trace-bg-full",
            ReplayTierAchieved = "R2",
            Provenance = TestHelpers.MakeProvenance(),
            RejectionReason = null,
            Notes = "Test background with full fields",
        };

        var json = GuJsonDefaults.Serialize(record);
        var deserialized = GuJsonDefaults.Deserialize<BackgroundRecord>(json);

        Assert.NotNull(deserialized);
        Assert.Equal("bg-full", deserialized.BackgroundId);
        Assert.Equal("env-test", deserialized.EnvironmentId);
        Assert.Equal("branch-test", deserialized.BranchManifestId);
        Assert.NotNull(deserialized.ContinuationCoordinates);
        Assert.Equal(0.5, deserialized.ContinuationCoordinates["lambda"]);
        Assert.Equal(1.2, deserialized.ContinuationCoordinates["kappa"]);
        Assert.Equal("coulomb", deserialized.GaugeChoice);
        Assert.Equal(1.23e-8, deserialized.ResidualNorm);
        Assert.Equal(4.56e-10, deserialized.StationarityNorm);
        Assert.Equal(AdmissibilityLevel.B2, deserialized.AdmissibilityLevel);
        Assert.Equal(150, deserialized.Metrics.SolverIterations);
        Assert.True(deserialized.Metrics.SolverConverged);
        Assert.Equal("trace-bg-full", deserialized.SolveTraceRef);
        Assert.Equal("R2", deserialized.ReplayTierAchieved);
        Assert.Equal("Test background with full fields", deserialized.Notes);
    }

    [Fact]
    public void BackgroundRecord_RejectedRecord_RoundTrip()
    {
        var record = TestHelpers.MakeRecord("bg-rej", AdmissibilityLevel.Rejected);

        var json = GuJsonDefaults.Serialize(record);
        var deserialized = GuJsonDefaults.Deserialize<BackgroundRecord>(json);

        Assert.NotNull(deserialized);
        Assert.Equal(AdmissibilityLevel.Rejected, deserialized.AdmissibilityLevel);
        Assert.Equal("test rejection", deserialized.RejectionReason);
    }

    [Fact]
    public void BackgroundMetrics_RoundTrip()
    {
        var metrics = new BackgroundMetrics
        {
            ResidualNorm = 3.14e-6,
            StationarityNorm = 2.72e-8,
            ObjectiveValue = 4.93e-12,
            GaugeViolation = 0.0,
            SolverIterations = 42,
            SolverConverged = false,
            TerminationReason = "max-iterations",
            GaussNewtonValid = false,
        };

        var json = GuJsonDefaults.Serialize(metrics);
        var deserialized = GuJsonDefaults.Deserialize<BackgroundMetrics>(json);

        Assert.NotNull(deserialized);
        Assert.Equal(3.14e-6, deserialized.ResidualNorm);
        Assert.Equal(2.72e-8, deserialized.StationarityNorm);
        Assert.Equal(42, deserialized.SolverIterations);
        Assert.False(deserialized.SolverConverged);
        Assert.Equal("max-iterations", deserialized.TerminationReason);
    }

    [Fact]
    public void BackgroundAtlas_WithMixedLevels_RoundTrip()
    {
        var atlas = new BackgroundAtlas
        {
            AtlasId = "atlas-mixed",
            StudyId = "study-mixed",
            Backgrounds = new[]
            {
                TestHelpers.MakeRecord("bg-b2", AdmissibilityLevel.B2, residualNorm: 1e-9),
                TestHelpers.MakeRecord("bg-b1", AdmissibilityLevel.B1, residualNorm: 1e-6),
                TestHelpers.MakeRecord("bg-b0", AdmissibilityLevel.B0, residualNorm: 1e-5),
            },
            RejectedBackgrounds = new[]
            {
                TestHelpers.MakeRecord("bg-rej", AdmissibilityLevel.Rejected),
            },
            RankingCriteria = "residual-then-stationarity",
            TotalAttempts = 4,
            Provenance = TestHelpers.MakeProvenance(),
            AdmissibilityCounts = new Dictionary<string, int>
            {
                ["B0"] = 1, ["B1"] = 1, ["B2"] = 1, ["Rejected"] = 1,
            },
        };

        var json = BackgroundAtlasSerializer.SerializeAtlas(atlas);
        var deserialized = BackgroundAtlasSerializer.DeserializeAtlas(json);

        Assert.NotNull(deserialized);
        Assert.Equal("atlas-mixed", deserialized.AtlasId);
        Assert.Equal(3, deserialized.Backgrounds.Count);
        Assert.Single(deserialized.RejectedBackgrounds);
        Assert.Equal(4, deserialized.TotalAttempts);
        Assert.Equal(4, deserialized.AdmissibilityCounts.Count);
    }

    [Fact]
    public void BackgroundSeed_AllKinds_RoundTrip()
    {
        foreach (var kind in Enum.GetValues<BackgroundSeedKind>())
        {
            var seed = new BackgroundSeed
            {
                Kind = kind,
                Label = $"seed-{kind}",
            };
            var json = GuJsonDefaults.Serialize(seed);
            var deserialized = GuJsonDefaults.Deserialize<BackgroundSeed>(json);
            Assert.NotNull(deserialized);
            Assert.Equal(kind, deserialized.Kind);
        }
    }

    [Fact]
    public void BackgroundSolveOptions_AllFields_RoundTrip()
    {
        var options = new BackgroundSolveOptions
        {
            SolveMode = SolveMode.StationaritySolve,
            SolverMethod = SolverMethod.GaussNewton,
            MaxIterations = 1000,
            InitialStepSize = 0.001,
            GaugePenaltyLambda = 0.5,
            GaugeStrategy = GaugeStrategy.L2Penalty,
            ToleranceResidualDiagnostic = 1e-3,
            ToleranceStationary = 1e-5,
            ToleranceResidualStrict = 1e-7,
        };

        var json = GuJsonDefaults.Serialize(options);
        var deserialized = GuJsonDefaults.Deserialize<BackgroundSolveOptions>(json);

        Assert.NotNull(deserialized);
        Assert.Equal(SolveMode.StationaritySolve, deserialized.SolveMode);
        Assert.Equal(SolverMethod.GaussNewton, deserialized.SolverMethod);
        Assert.Equal(1000, deserialized.MaxIterations);
        Assert.Equal(0.001, deserialized.InitialStepSize);
        Assert.Equal(0.5, deserialized.GaugePenaltyLambda);
        Assert.Equal(1e-3, deserialized.ToleranceResidualDiagnostic);
        Assert.Equal(1e-5, deserialized.ToleranceStationary);
        Assert.Equal(1e-7, deserialized.ToleranceResidualStrict);
    }

    [Fact]
    public void AdmissibilityLevel_AllValues_SerializeAsStrings()
    {
        foreach (var level in Enum.GetValues<AdmissibilityLevel>())
        {
            var record = TestHelpers.MakeRecord($"bg-{level}", level);
            var json = GuJsonDefaults.Serialize(record);
            Assert.Contains($"\"{level}\"", json);
        }
    }
}
