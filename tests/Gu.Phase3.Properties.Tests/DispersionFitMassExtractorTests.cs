namespace Gu.Phase3.Properties.Tests;

public class DispersionFitMassExtractorTests
{
    [Fact]
    public void Compute_ReturnsDispersionFitExtractionMethod()
    {
        var samples = new List<(double BackgroundParameter, double MassLikeScale)>
        {
            (0.0, 1.0),
            (1.0, 1.5),
            (2.0, 2.0),
        };

        var record = DispersionFitMassExtractor.Compute(
            "mode-1", "bg-1", "GaussNewton", samples);

        Assert.Equal("dispersion-fit", record.ExtractionMethod);
        Assert.Equal("mode-1", record.ModeId);
        Assert.Equal("bg-1", record.BackgroundId);
        Assert.Equal("GaussNewton", record.OperatorType);
    }

    [Fact]
    public void Compute_SingleSample_ReturnsSampleValue()
    {
        var samples = new List<(double BackgroundParameter, double MassLikeScale)>
        {
            (0.5, 3.0),
        };

        var record = DispersionFitMassExtractor.Compute(
            "mode-s", "bg-1", "GaussNewton", samples);

        Assert.Equal(3.0, record.MassLikeScale, 12);
        Assert.Equal("dispersion-fit", record.ExtractionMethod);
    }

    [Fact]
    public void Compute_LinearData_FitsIntercept()
    {
        // y = 2.0 + 0.5 * x => intercept = 2.0
        var samples = new List<(double BackgroundParameter, double MassLikeScale)>
        {
            (0.0, 2.0),
            (2.0, 3.0),
            (4.0, 4.0),
        };

        var record = DispersionFitMassExtractor.Compute(
            "mode-lin", "bg-1", "GaussNewton", samples);

        Assert.Equal(2.0, record.MassLikeScale, 10);
    }

    [Fact]
    public void Compute_EmptySamples_Throws()
    {
        var samples = new List<(double BackgroundParameter, double MassLikeScale)>();

        Assert.Throws<ArgumentException>(() =>
            DispersionFitMassExtractor.Compute("m", "bg", "GN", samples));
    }

    [Fact]
    public void Compute_NullModeId_Throws()
    {
        var samples = new List<(double BackgroundParameter, double MassLikeScale)>
        {
            (0.0, 1.0),
        };

        Assert.Throws<ArgumentNullException>(() =>
            DispersionFitMassExtractor.Compute(null!, "bg", "GN", samples));
    }

    [Fact]
    public void Compute_NullSamples_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            DispersionFitMassExtractor.Compute("m", "bg", "GN", null!));
    }

    [Fact]
    public void Compute_ConstantMassLikeScale_ReturnsConstant()
    {
        var samples = new List<(double BackgroundParameter, double MassLikeScale)>
        {
            (0.0, 5.0),
            (1.0, 5.0),
            (2.0, 5.0),
        };

        var record = DispersionFitMassExtractor.Compute(
            "mode-c", "bg-1", "GaussNewton", samples);

        Assert.Equal(5.0, record.MassLikeScale, 10);
    }
}
