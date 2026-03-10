using Gu.Phase3.Backgrounds;
using Gu.Solvers;

namespace Gu.Phase3.Backgrounds.Tests;

public class AdmissibilityGraderTests
{
    private static AdmissibilityGrader MakeGrader(
        double tolDiagnostic = 1e-4,
        double tolStationary = 1e-6,
        double tolStrict = 1e-8)
    {
        return new AdmissibilityGrader(tolDiagnostic, tolStationary, tolStrict);
    }

    [Fact]
    public void B2_WhenStationaryAndStrictResidual()
    {
        var grader = MakeGrader();
        var (level, reason) = grader.Grade(residualNorm: 1e-9, stationarityNorm: 1e-7);
        Assert.Equal(AdmissibilityLevel.B2, level);
        Assert.Null(reason);
    }

    [Fact]
    public void B1_WhenStationaryButResidualAboveStrict()
    {
        var grader = MakeGrader();
        var (level, reason) = grader.Grade(residualNorm: 1e-5, stationarityNorm: 1e-7);
        Assert.Equal(AdmissibilityLevel.B1, level);
        Assert.Null(reason);
    }

    [Fact]
    public void B0_WhenNotStationaryButLowResidual()
    {
        var grader = MakeGrader();
        var (level, reason) = grader.Grade(residualNorm: 1e-5, stationarityNorm: 1e-3);
        Assert.Equal(AdmissibilityLevel.B0, level);
        Assert.Null(reason);
    }

    [Fact]
    public void Rejected_WhenBothNormsHigh()
    {
        var grader = MakeGrader();
        var (level, reason) = grader.Grade(residualNorm: 1.0, stationarityNorm: 1.0);
        Assert.Equal(AdmissibilityLevel.Rejected, level);
        Assert.NotNull(reason);
        Assert.Contains("ResidualNorm", reason);
    }

    [Fact]
    public void B2_AtExactThresholds()
    {
        var grader = MakeGrader();
        var (level, _) = grader.Grade(residualNorm: 1e-8, stationarityNorm: 1e-6);
        Assert.Equal(AdmissibilityLevel.B2, level);
    }

    [Fact]
    public void B1_WhenStationarityExactlyAtThreshold()
    {
        var grader = MakeGrader();
        var (level, _) = grader.Grade(residualNorm: 1e-4, stationarityNorm: 1e-6);
        // residual=1e-4 equals diagnostic threshold, strict residual is 1e-8 so residual > strict
        // stationarity=1e-6 equals stationary threshold => B1
        Assert.Equal(AdmissibilityLevel.B1, level);
    }

    [Fact]
    public void ConstructFromSolveOptions()
    {
        var options = new BackgroundSolveOptions
        {
            SolveMode = SolveMode.StationaritySolve,
            ToleranceResidualDiagnostic = 1e-2,
            ToleranceStationary = 1e-4,
            ToleranceResidualStrict = 1e-6,
        };
        var grader = new AdmissibilityGrader(options);
        var (level, _) = grader.Grade(residualNorm: 1e-7, stationarityNorm: 1e-5);
        Assert.Equal(AdmissibilityLevel.B2, level);
    }

    [Fact]
    public void ZeroNormsGetB2()
    {
        var grader = MakeGrader();
        var (level, _) = grader.Grade(residualNorm: 0.0, stationarityNorm: 0.0);
        Assert.Equal(AdmissibilityLevel.B2, level);
    }
}
