namespace Gu.Phase3.Spectra.Tests;

public class OperatorSelfCheckReportTests
{
    [Fact]
    public void Report_CanConstruct_Passing()
    {
        var report = new OperatorSelfCheckReport
        {
            BundleId = "bundle-1",
            SpectralSymmetryError = 1e-12,
            MassSymmetryError = 1e-13,
            MassMinQuadratic = 0.5,
            Passed = true,
            Tolerance = 1e-8,
            ProbeCount = 10,
        };

        Assert.True(report.Passed);
        Assert.Equal("bundle-1", report.BundleId);
        Assert.Equal(10, report.ProbeCount);
        Assert.True(report.SpectralSymmetryError < report.Tolerance);
        Assert.True(report.MassSymmetryError < report.Tolerance);
        Assert.True(report.MassMinQuadratic >= 0);
    }

    [Fact]
    public void Report_CanConstruct_Failing()
    {
        var report = new OperatorSelfCheckReport
        {
            BundleId = "bundle-bad",
            SpectralSymmetryError = 1.0, // too large
            MassSymmetryError = 1e-13,
            MassMinQuadratic = 0.5,
            Passed = false,
            Tolerance = 1e-8,
            ProbeCount = 10,
        };

        Assert.False(report.Passed);
        Assert.True(report.SpectralSymmetryError > report.Tolerance);
    }
}
