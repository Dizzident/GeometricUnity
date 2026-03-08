using Gu.Solvers;

namespace Gu.VulkanViewer.Tests;

public class ConvergencePlotterTests
{
    [Fact]
    public void ExtractAll_ProducesAllSixSeries()
    {
        var history = TestDataHelper.CreateConvergenceHistory(10);

        var plotData = ConvergencePlotter.ExtractAll(history);

        Assert.Equal(6, plotData.Series.Count);
        Assert.Contains(plotData.Series, s => s.Label == "Objective");
        Assert.Contains(plotData.Series, s => s.Label == "Residual Norm");
        Assert.Contains(plotData.Series, s => s.Label == "Gradient Norm");
        Assert.Contains(plotData.Series, s => s.Label == "Gauge Violation");
        Assert.Contains(plotData.Series, s => s.Label == "Step Size");
        Assert.Contains(plotData.Series, s => s.Label == "Gauge/Physics Ratio");
    }

    [Fact]
    public void ExtractAll_SeriesHaveCorrectLength()
    {
        int count = 15;
        var history = TestDataHelper.CreateConvergenceHistory(count);

        var plotData = ConvergencePlotter.ExtractAll(history);

        foreach (var series in plotData.Series)
        {
            Assert.Equal(count, series.X.Length);
            Assert.Equal(count, series.Y.Length);
        }

        Assert.Equal(count, plotData.IterationCount);
    }

    [Fact]
    public void ExtractAll_XValuesAreIterationNumbers()
    {
        var history = TestDataHelper.CreateConvergenceHistory(5);

        var plotData = ConvergencePlotter.ExtractAll(history);

        var objectiveSeries = plotData.Series.First(s => s.Label == "Objective");
        for (int i = 0; i < 5; i++)
        {
            Assert.Equal(i, objectiveSeries.X[i]);
        }
    }

    [Fact]
    public void ExtractAll_ObjectiveValuesMatchHistory()
    {
        var history = TestDataHelper.CreateConvergenceHistory(5);

        var plotData = ConvergencePlotter.ExtractAll(history);

        var objectiveSeries = plotData.Series.First(s => s.Label == "Objective");
        for (int i = 0; i < 5; i++)
        {
            Assert.Equal(history[i].Objective, objectiveSeries.Y[i]);
        }
    }

    [Fact]
    public void ExtractAll_IncludesConvergenceMetadata()
    {
        var history = TestDataHelper.CreateConvergenceHistory(3);

        var plotData = ConvergencePlotter.ExtractAll(history, converged: true, terminationReason: "Gradient below threshold");

        Assert.True(plotData.Converged);
        Assert.Equal("Gradient below threshold", plotData.TerminationReason);
    }

    [Fact]
    public void ExtractAll_ThrowsOnNull()
    {
        Assert.Throws<ArgumentNullException>(() => ConvergencePlotter.ExtractAll(null!));
    }

    [Fact]
    public void ExtractAll_ThrowsOnEmptyHistory()
    {
        Assert.Throws<ArgumentException>(() => ConvergencePlotter.ExtractAll(new List<ConvergenceRecord>()));
    }

    [Fact]
    public void ExtractAll_LogScaleRecommended_ForMostSeries()
    {
        var history = TestDataHelper.CreateConvergenceHistory(3);

        var plotData = ConvergencePlotter.ExtractAll(history);

        var objectiveSeries = plotData.Series.First(s => s.Label == "Objective");
        Assert.True(objectiveSeries.LogScaleRecommended);

        var ratioSeries = plotData.Series.First(s => s.Label == "Gauge/Physics Ratio");
        Assert.False(ratioSeries.LogScaleRecommended);
    }

    [Theory]
    [InlineData("Objective")]
    [InlineData("ResidualNorm")]
    [InlineData("GradientNorm")]
    [InlineData("GaugeViolation")]
    [InlineData("StepSize")]
    [InlineData("GaugeToPhysicsRatio")]
    public void ExtractSeries_ReturnsCorrectSeries(string seriesName)
    {
        var history = TestDataHelper.CreateConvergenceHistory(5);

        var series = ConvergencePlotter.ExtractSeries(history, seriesName);

        Assert.Equal(seriesName, series.Label);
        Assert.Equal(5, series.X.Length);
        Assert.Equal(5, series.Y.Length);
    }

    [Fact]
    public void ExtractSeries_ThrowsOnUnknownName()
    {
        var history = TestDataHelper.CreateConvergenceHistory(3);

        Assert.Throws<ArgumentException>(() => ConvergencePlotter.ExtractSeries(history, "UnknownSeries"));
    }

    [Fact]
    public void ToCsv_ProducesValidFormat()
    {
        var history = TestDataHelper.CreateConvergenceHistory(3);

        string csv = ConvergencePlotter.ToCsv(history);

        // Check header.
        var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        Assert.True(lines.Length >= 4, "Expected header + 3 data rows.");
        Assert.StartsWith("Iteration,Objective,", lines[0]);

        // Check data rows have correct number of columns.
        for (int i = 1; i < lines.Length; i++)
        {
            string[] cols = lines[i].Split(',');
            Assert.Equal(8, cols.Length); // 8 columns in the CSV
        }
    }

    [Fact]
    public void ToCsv_FirstDataRowMatchesFirstRecord()
    {
        var history = TestDataHelper.CreateConvergenceHistory(3);

        string csv = ConvergencePlotter.ToCsv(history);

        var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        string[] cols = lines[1].Split(',');

        Assert.Equal("0", cols[0]); // Iteration 0
        Assert.Equal(history[0].Objective.ToString("G17"), cols[1]);
    }

    [Fact]
    public void ExtractAll_HasCorrectTitle()
    {
        var history = TestDataHelper.CreateConvergenceHistory(3);

        var plotData = ConvergencePlotter.ExtractAll(history);

        Assert.Equal("Solver Convergence Diagnostics", plotData.Title);
        Assert.Equal("Iteration", plotData.XAxisLabel);
        Assert.Equal("Value", plotData.YAxisLabel);
    }
}
