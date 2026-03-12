namespace Gu.Phase3.Properties.Tests;

public class DispersionFitMassExtractorTests
{
    // --- LinearInterpolationMassExtractor tests (renamed from DispersionFitMassExtractor.Compute) ---

    [Fact]
    public void LinearInterp_ReturnsLinearInterpolationExtractionMethod()
    {
        var samples = new List<(double BackgroundParameter, double MassLikeScale)>
        {
            (0.0, 1.0),
            (1.0, 1.5),
            (2.0, 2.0),
        };

        var record = LinearInterpolationMassExtractor.Compute(
            "mode-1", "bg-1", "GaussNewton", samples);

        Assert.Equal("linear-interpolation", record.ExtractionMethod);
        Assert.Equal("mode-1", record.ModeId);
        Assert.Equal("bg-1", record.BackgroundId);
        Assert.Equal("GaussNewton", record.OperatorType);
    }

    [Fact]
    public void LinearInterp_SingleSample_ReturnsSampleValue()
    {
        var samples = new List<(double BackgroundParameter, double MassLikeScale)>
        {
            (0.5, 3.0),
        };

        var record = LinearInterpolationMassExtractor.Compute(
            "mode-s", "bg-1", "GaussNewton", samples);

        Assert.Equal(3.0, record.MassLikeScale, 12);
        Assert.Equal("linear-interpolation", record.ExtractionMethod);
    }

    [Fact]
    public void LinearInterp_LinearData_FitsIntercept()
    {
        // y = 2.0 + 0.5 * x => intercept = 2.0
        var samples = new List<(double BackgroundParameter, double MassLikeScale)>
        {
            (0.0, 2.0),
            (2.0, 3.0),
            (4.0, 4.0),
        };

        var record = LinearInterpolationMassExtractor.Compute(
            "mode-lin", "bg-1", "GaussNewton", samples);

        Assert.Equal(2.0, record.MassLikeScale, 10);
    }

    [Fact]
    public void LinearInterp_EmptySamples_Throws()
    {
        var samples = new List<(double BackgroundParameter, double MassLikeScale)>();

        Assert.Throws<ArgumentException>(() =>
            LinearInterpolationMassExtractor.Compute("m", "bg", "GN", samples));
    }

    [Fact]
    public void LinearInterp_NullModeId_Throws()
    {
        var samples = new List<(double BackgroundParameter, double MassLikeScale)>
        {
            (0.0, 1.0),
        };

        Assert.Throws<ArgumentNullException>(() =>
            LinearInterpolationMassExtractor.Compute(null!, "bg", "GN", samples));
    }

    [Fact]
    public void LinearInterp_NullSamples_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            LinearInterpolationMassExtractor.Compute("m", "bg", "GN", null!));
    }

    [Fact]
    public void LinearInterp_ConstantMassLikeScale_ReturnsConstant()
    {
        var samples = new List<(double BackgroundParameter, double MassLikeScale)>
        {
            (0.0, 5.0),
            (1.0, 5.0),
            (2.0, 5.0),
        };

        var record = LinearInterpolationMassExtractor.Compute(
            "mode-c", "bg-1", "GaussNewton", samples);

        Assert.Equal(5.0, record.MassLikeScale, 10);
    }

    // --- DispersionFitMassExtractor.ComputeFromDispersion tests ---

    [Fact]
    public void ComputeFromDispersion_ToyRelativistic_RecoversMass()
    {
        // omega^2(k) = 4 + k^2  =>  m^2 = 4  =>  m = 2.0
        var kValues = new double[] { 0.0, 1.0, 2.0, 3.0 };
        var omegaSquareds = new double[] { 4.0, 5.0, 8.0, 13.0 };

        var record = DispersionFitMassExtractor.ComputeFromDispersion(
            "mode-rel", "bg-1", "FullHessian", kValues, omegaSquareds);

        Assert.Equal("dispersion-fit", record.ExtractionMethod);
        Assert.Equal(2.0, record.MassLikeScale, 1e-10);
        Assert.Equal(4.0, record.Eigenvalue, 1e-10);
        Assert.Null(record.Notes); // not massless, not tachyonic
    }

    [Fact]
    public void ComputeFromDispersion_Massless_NearZero()
    {
        // omega^2(k) = k^2  =>  m^2 = 0
        var kValues = new double[] { 1.0, 2.0, 3.0 };
        var omegaSquareds = new double[] { 1.0, 4.0, 9.0 };

        var record = DispersionFitMassExtractor.ComputeFromDispersion(
            "mode-massless", "bg-1", "FullHessian", kValues, omegaSquareds);

        Assert.True(System.Math.Abs(record.MassLikeScale) < 1e-6,
            $"Expected near-zero mass, got {record.MassLikeScale}");
        Assert.Equal("massless", record.Notes);
    }

    [Fact]
    public void ComputeFromDispersion_Tachyonic_NegativeMass()
    {
        // omega^2(k) = -1 + k^2  =>  m^2 = -1  =>  m < 0, notes "tachyonic"
        var kValues = new double[] { 2.0, 3.0, 4.0 };
        var omegaSquareds = new double[] { 3.0, 8.0, 15.0 };

        var record = DispersionFitMassExtractor.ComputeFromDispersion(
            "mode-tach", "bg-1", "FullHessian", kValues, omegaSquareds);

        Assert.True(record.MassLikeScale < 0,
            $"Expected negative mass-like scale for tachyonic mode, got {record.MassLikeScale}");
        Assert.Equal("tachyonic", record.Notes);
    }

    [Fact]
    public void ComputeFromDispersion_SinglePoint_NotesSinglePointFallback()
    {
        var kValues = new double[] { 1.0 };
        var omegaSquareds = new double[] { 5.0 };

        var record = DispersionFitMassExtractor.ComputeFromDispersion(
            "mode-single", "bg-1", "FullHessian", kValues, omegaSquareds);

        Assert.Equal("single-point-fallback", record.Notes);
        Assert.Equal("dispersion-fit", record.ExtractionMethod);
    }

    [Fact]
    public void ComputeFromDispersion_MismatchedLengths_Throws()
    {
        var kValues = new double[] { 1.0, 2.0 };
        var omegaSquareds = new double[] { 5.0 };

        Assert.Throws<ArgumentException>(() =>
            DispersionFitMassExtractor.ComputeFromDispersion(
                "m", "bg", "op", kValues, omegaSquareds));
    }

    [Fact]
    public void ComputeFromDispersion_EmptyInputs_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            DispersionFitMassExtractor.ComputeFromDispersion(
                "m", "bg", "op", Array.Empty<double>(), Array.Empty<double>()));
    }

    [Fact]
    public void ComputeFromDispersion_NullKValues_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            DispersionFitMassExtractor.ComputeFromDispersion(
                "m", "bg", "op", null!, new double[] { 1.0 }));
    }
}
