using Gu.Phase5.Environments;

namespace Gu.Phase5.Environments.Tests;

public class EnvironmentAdmissibilityCheckerTests
{
    [Fact]
    public void ValidMesh_PassesAllChecks()
    {
        // Simple 2D triangle: 1 face, 3 edges, embedding dim 2
        double[] volumes = [0.5]; // area of one unit triangle
        var report = EnvironmentAdmissibilityChecker.Check(
            baseDim: 2, ambientDim: 2, edgeCount: 3, faceCount: 1,
            volumes: volumes);

        Assert.True(report.Passed);
        Assert.Equal("admissible", report.Level);
        Assert.All(report.Checks, c => Assert.True(c.Passed));
    }

    [Fact]
    public void DegenerateFace_FailsMeshValidCheck()
    {
        double[] volumes = [0.0]; // degenerate face with zero area
        var report = EnvironmentAdmissibilityChecker.Check(
            baseDim: 2, ambientDim: 2, edgeCount: 3, faceCount: 1,
            volumes: volumes);

        Assert.False(report.Passed);
        Assert.Equal("inadmissible", report.Level);
        var meshCheck = report.Checks.Single(c => c.CheckId == "mesh-valid");
        Assert.False(meshCheck.Passed);
    }

    [Fact]
    public void InvalidDimension_FailsDimensionMatchCheck()
    {
        double[] volumes = [0.5];
        // baseDim=3 but ambientDim=2 is invalid
        var report = EnvironmentAdmissibilityChecker.Check(
            baseDim: 3, ambientDim: 2, edgeCount: 6, faceCount: 1,
            volumes: volumes);

        Assert.False(report.Passed);
        var dimCheck = report.Checks.Single(c => c.CheckId == "dimension-match");
        Assert.False(dimCheck.Passed);
    }

    [Fact]
    public void ZeroFaceCount_FailsOrientationCheck()
    {
        var report = EnvironmentAdmissibilityChecker.Check(
            baseDim: 2, ambientDim: 2, edgeCount: 3, faceCount: 0,
            volumes: []);

        Assert.False(report.Passed);
        var orientCheck = report.Checks.Single(c => c.CheckId == "orientation");
        Assert.False(orientCheck.Passed);
    }

    [Fact]
    public void EmptyVolumes_PassesMeshValidCheck()
    {
        // No faces means mesh-valid vacuously passes
        var report = EnvironmentAdmissibilityChecker.Check(
            baseDim: 2, ambientDim: 2, edgeCount: 3, faceCount: 1,
            volumes: []);

        // mesh-valid passes (no volumes to check), but orientation fails (faceCount != volumes.Length isn't checked)
        var meshCheck = report.Checks.Single(c => c.CheckId == "mesh-valid");
        Assert.True(meshCheck.Passed);
    }

    [Fact]
    public void Notes_ContainFailedCheckIds_WhenFailed()
    {
        double[] volumes = [0.0]; // degenerate
        var report = EnvironmentAdmissibilityChecker.Check(
            baseDim: 2, ambientDim: 2, edgeCount: 3, faceCount: 1,
            volumes: volumes);

        Assert.NotNull(report.Notes);
        Assert.Contains("mesh-valid", report.Notes);
    }
}
