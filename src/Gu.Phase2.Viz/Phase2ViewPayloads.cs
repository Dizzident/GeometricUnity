using Gu.VulkanViewer;

namespace Gu.Phase2.Viz;

/// <summary>
/// Branch overlay view: two branches side-by-side with D_obs highlighted on mesh faces.
/// </summary>
public sealed class BranchOverlayPayload : ViewPayload
{
    public override string ViewType => "branch_overlay";

    /// <summary>Left branch variant ID.</summary>
    public required string LeftBranchId { get; init; }

    /// <summary>Right branch variant ID.</summary>
    public required string RightBranchId { get; init; }

    /// <summary>Visualization data for the left branch field.</summary>
    public required VisualizationData LeftMeshData { get; init; }

    /// <summary>Visualization data for the right branch field.</summary>
    public required VisualizationData RightMeshData { get; init; }

    /// <summary>Per-face D_obs difference values for highlighting.</summary>
    public required double[] FaceDifferences { get; init; }

    /// <summary>Max D_obs distance between the two branches.</summary>
    public required double MaxDistance { get; init; }
}

/// <summary>
/// Spectrum dashboard: eigenvalue spectrum bar chart from SpectrumRecord as HUD overlay.
/// </summary>
public sealed class SpectrumDashboardPayload : ViewPayload
{
    public override string ViewType => "spectrum_dashboard";

    /// <summary>Background state ID.</summary>
    public required string BackgroundStateId { get; init; }

    /// <summary>Operator ID ("H" for Hessian, "L_tilde" for gauge-fixed).</summary>
    public required string OperatorId { get; init; }

    /// <summary>Eigenvalues or singular values to display.</summary>
    public required double[] Values { get; init; }

    /// <summary>Stability interpretation string.</summary>
    public string? StabilityInterpretation { get; init; }

    /// <summary>Gauge handling mode.</summary>
    public required string GaugeHandlingMode { get; init; }

    /// <summary>Probe method used.</summary>
    public required string ProbeMethod { get; init; }

    /// <summary>Mode counts for classification.</summary>
    public int NegativeModeCount { get; init; }
    public int SoftModeCount { get; init; }
    public int NearKernelCount { get; init; }
    public int CoerciveModeCount { get; init; }
}

/// <summary>
/// Continuation path view: lambda vs objective/eigenvalue 2D line plot on HUD.
/// </summary>
public sealed class ContinuationPathPayload : ViewPayload
{
    public override string ViewType => "continuation_path";

    /// <summary>Parameter name (typically "lambda").</summary>
    public required string ParameterName { get; init; }

    /// <summary>Branch manifest ID.</summary>
    public required string BranchManifestId { get; init; }

    /// <summary>Plot series for the continuation path.</summary>
    public required IReadOnlyList<PlotSeries> Series { get; init; }

    /// <summary>Total number of continuation steps.</summary>
    public required int TotalSteps { get; init; }

    /// <summary>Termination reason.</summary>
    public required string TerminationReason { get; init; }

    /// <summary>Lambda range covered.</summary>
    public required double LambdaMin { get; init; }
    public required double LambdaMax { get; init; }
}

/// <summary>
/// Canonicity heatmap: PairwiseDistanceMatrix as colored NxN grid.
/// </summary>
public sealed class CanonicityHeatmapPayload : ViewPayload
{
    public override string ViewType => "canonicity_heatmap";

    /// <summary>Metric identifier (e.g., "D_obs_max", "D_stab").</summary>
    public required string MetricId { get; init; }

    /// <summary>Branch variant IDs labeling rows and columns.</summary>
    public required IReadOnlyList<string> BranchIds { get; init; }

    /// <summary>Flattened row-major distance values (N*N).</summary>
    public required double[] FlatDistances { get; init; }

    /// <summary>Number of branches (N).</summary>
    public required int Dimension { get; init; }

    /// <summary>Maximum distance value for color scaling.</summary>
    public required double MaxDistance { get; init; }

    /// <summary>Color map range minimum.</summary>
    public double ColorMin { get; init; } = 0.0;

    /// <summary>Color map range maximum (auto-set from MaxDistance if not specified).</summary>
    public double? ColorMax { get; init; }
}
