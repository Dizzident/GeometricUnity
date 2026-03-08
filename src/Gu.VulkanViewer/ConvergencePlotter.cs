using Gu.Solvers;

namespace Gu.VulkanViewer;

/// <summary>
/// A single data series for convergence plotting.
/// </summary>
public sealed class PlotSeries
{
    /// <summary>Human-readable label for the series (e.g., "Objective", "Residual Norm").</summary>
    public required string Label { get; init; }

    /// <summary>X-axis values (typically iteration numbers).</summary>
    public required double[] X { get; init; }

    /// <summary>Y-axis values (the quantity being plotted).</summary>
    public required double[] Y { get; init; }

    /// <summary>
    /// Whether a logarithmic Y-axis is recommended for this series.
    /// True for quantities that span many orders of magnitude (norms, objectives).
    /// </summary>
    public bool LogScaleRecommended { get; init; }
}

/// <summary>
/// Structured plot data for convergence diagnostics, extracted from solver history.
/// Not a renderer itself -- produces data suitable for external plotting tools
/// or a future Vulkan-based chart renderer.
/// </summary>
public sealed class ConvergencePlotData
{
    /// <summary>Title for the convergence plot.</summary>
    public required string Title { get; init; }

    /// <summary>X-axis label.</summary>
    public required string XAxisLabel { get; init; }

    /// <summary>Y-axis label.</summary>
    public required string YAxisLabel { get; init; }

    /// <summary>All data series included in the plot.</summary>
    public required IReadOnlyList<PlotSeries> Series { get; init; }

    /// <summary>Total number of iterations.</summary>
    public required int IterationCount { get; init; }

    /// <summary>Whether the solver converged (if known).</summary>
    public bool? Converged { get; init; }

    /// <summary>Termination reason (if known).</summary>
    public string? TerminationReason { get; init; }
}

/// <summary>
/// Extracts structured plot data from solver convergence records.
/// Produces <see cref="ConvergencePlotData"/> for objective, residual norm,
/// gradient norm, gauge violation, step size, and gauge-to-physics ratio.
/// </summary>
public static class ConvergencePlotter
{
    /// <summary>
    /// Extracts all standard convergence series from a solver history.
    /// </summary>
    /// <param name="history">Convergence records from a solver run.</param>
    /// <param name="converged">Whether the solver converged (optional metadata).</param>
    /// <param name="terminationReason">Reason for termination (optional metadata).</param>
    /// <returns>A <see cref="ConvergencePlotData"/> containing all standard diagnostic series.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="history"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="history"/> is empty.</exception>
    public static ConvergencePlotData ExtractAll(
        IReadOnlyList<ConvergenceRecord> history,
        bool? converged = null,
        string? terminationReason = null)
    {
        ArgumentNullException.ThrowIfNull(history);
        if (history.Count == 0)
        {
            throw new ArgumentException("Convergence history must contain at least one record.", nameof(history));
        }

        double[] iterations = new double[history.Count];
        double[] objectives = new double[history.Count];
        double[] residualNorms = new double[history.Count];
        double[] gradientNorms = new double[history.Count];
        double[] gaugeViolations = new double[history.Count];
        double[] stepSizes = new double[history.Count];
        double[] gaugeToPhysics = new double[history.Count];

        for (int i = 0; i < history.Count; i++)
        {
            ConvergenceRecord rec = history[i];
            iterations[i] = rec.Iteration;
            objectives[i] = rec.Objective;
            residualNorms[i] = rec.ResidualNorm;
            gradientNorms[i] = rec.GradientNorm;
            gaugeViolations[i] = rec.GaugeViolation;
            stepSizes[i] = rec.StepSize;
            gaugeToPhysics[i] = rec.GaugeToPhysicsRatio;
        }

        var series = new List<PlotSeries>
        {
            new PlotSeries
            {
                Label = "Objective",
                X = iterations,
                Y = objectives,
                LogScaleRecommended = true,
            },
            new PlotSeries
            {
                Label = "Residual Norm",
                X = iterations,
                Y = residualNorms,
                LogScaleRecommended = true,
            },
            new PlotSeries
            {
                Label = "Gradient Norm",
                X = iterations,
                Y = gradientNorms,
                LogScaleRecommended = true,
            },
            new PlotSeries
            {
                Label = "Gauge Violation",
                X = iterations,
                Y = gaugeViolations,
                LogScaleRecommended = true,
            },
            new PlotSeries
            {
                Label = "Step Size",
                X = iterations,
                Y = stepSizes,
                LogScaleRecommended = true,
            },
            new PlotSeries
            {
                Label = "Gauge/Physics Ratio",
                X = iterations,
                Y = gaugeToPhysics,
                LogScaleRecommended = false,
            },
        };

        return new ConvergencePlotData
        {
            Title = "Solver Convergence Diagnostics",
            XAxisLabel = "Iteration",
            YAxisLabel = "Value",
            Series = series,
            IterationCount = history.Count,
            Converged = converged,
            TerminationReason = terminationReason,
        };
    }

    /// <summary>
    /// Extracts a single named series from the convergence history.
    /// </summary>
    /// <param name="history">Convergence records.</param>
    /// <param name="seriesName">
    /// Name of the series: "Objective", "ResidualNorm", "GradientNorm",
    /// "GaugeViolation", "StepSize", or "GaugeToPhysicsRatio".
    /// </param>
    /// <returns>A single <see cref="PlotSeries"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="seriesName"/> is not recognized.</exception>
    public static PlotSeries ExtractSeries(
        IReadOnlyList<ConvergenceRecord> history,
        string seriesName)
    {
        ArgumentNullException.ThrowIfNull(history);
        ArgumentException.ThrowIfNullOrWhiteSpace(seriesName);

        double[] x = new double[history.Count];
        double[] y = new double[history.Count];

        Func<ConvergenceRecord, double> selector = seriesName switch
        {
            "Objective" => r => r.Objective,
            "ResidualNorm" => r => r.ResidualNorm,
            "GradientNorm" => r => r.GradientNorm,
            "GaugeViolation" => r => r.GaugeViolation,
            "StepSize" => r => r.StepSize,
            "GaugeToPhysicsRatio" => r => r.GaugeToPhysicsRatio,
            _ => throw new ArgumentException(
                $"Unknown series name '{seriesName}'. Supported: Objective, ResidualNorm, GradientNorm, GaugeViolation, StepSize, GaugeToPhysicsRatio.",
                nameof(seriesName)),
        };

        for (int i = 0; i < history.Count; i++)
        {
            x[i] = history[i].Iteration;
            y[i] = selector(history[i]);
        }

        return new PlotSeries
        {
            Label = seriesName,
            X = x,
            Y = y,
            LogScaleRecommended = seriesName != "GaugeToPhysicsRatio",
        };
    }

    /// <summary>
    /// Exports convergence data to CSV format.
    /// </summary>
    /// <param name="history">Convergence records.</param>
    /// <returns>CSV string with header row and one row per iteration.</returns>
    public static string ToCsv(IReadOnlyList<ConvergenceRecord> history)
    {
        ArgumentNullException.ThrowIfNull(history);

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Iteration,Objective,ResidualNorm,GradientNorm,GaugeViolation,StepNorm,StepSize,GaugeToPhysicsRatio");

        for (int i = 0; i < history.Count; i++)
        {
            ConvergenceRecord rec = history[i];
            sb.AppendLine(FormattableString.Invariant(
                $"{rec.Iteration},{rec.Objective:G17},{rec.ResidualNorm:G17},{rec.GradientNorm:G17},{rec.GaugeViolation:G17},{rec.StepNorm:G17},{rec.StepSize:G17},{rec.GaugeToPhysicsRatio:G17}"));
        }

        return sb.ToString();
    }
}
