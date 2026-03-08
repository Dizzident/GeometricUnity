using Gu.Core;
using Gu.Solvers;

namespace Gu.VulkanViewer;

/// <summary>
/// Base type for all view payloads sent to the native Vulkan renderer.
/// All payloads are read-only snapshots of artifact data (IX-5).
/// </summary>
public abstract class ViewPayload
{
    /// <summary>View type identifier for native dispatch.</summary>
    public abstract string ViewType { get; }

    /// <summary>Timestamp when the payload was prepared.</summary>
    public DateTimeOffset PreparedAt { get; init; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// Geometry view: mesh wireframe and surface with optional vertex labels.
/// </summary>
public sealed class GeometryViewPayload : ViewPayload
{
    public override string ViewType => "geometry";

    /// <summary>Visualization data (positions, colors, indices).</summary>
    public required VisualizationData MeshData { get; init; }

    /// <summary>Embedding dimension of the original mesh.</summary>
    public required int EmbeddingDimension { get; init; }

    /// <summary>Number of cells in the mesh.</summary>
    public required int CellCount { get; init; }

    /// <summary>Number of edges in the mesh.</summary>
    public required int EdgeCount { get; init; }

    /// <summary>Whether to show wireframe overlay.</summary>
    public bool ShowWireframe { get; init; } = true;
}

/// <summary>
/// Field view: scalar field mapped onto the mesh surface.
/// Supports four modes: connection (omega), curvature (F), torsion (T), shiab (S).
/// </summary>
public sealed class FieldViewPayload : ViewPayload
{
    public override string ViewType => "field";

    /// <summary>Visualization data with field-driven colors.</summary>
    public required VisualizationData FieldData { get; init; }

    /// <summary>Which field is displayed: "omega", "curvature", "torsion", "shiab", "residual".</summary>
    public required string FieldMode { get; init; }

    /// <summary>Field label from the FieldTensor.</summary>
    public required string FieldLabel { get; init; }

    /// <summary>Number of field coefficients.</summary>
    public required int CoefficientCount { get; init; }

    /// <summary>L2 norm of the field.</summary>
    public required double FieldNorm { get; init; }
}

/// <summary>
/// Residual view: residual field (Upsilon = S - T) mapped onto the mesh,
/// plus scalar objective and norm diagnostics.
/// </summary>
public sealed class ResidualViewPayload : ViewPayload
{
    public override string ViewType => "residual";

    /// <summary>Visualization data with residual-driven colors (diverging map).</summary>
    public required VisualizationData ResidualData { get; init; }

    /// <summary>Objective value I2_h = (1/2)||Upsilon||^2.</summary>
    public required double ObjectiveValue { get; init; }

    /// <summary>Residual L2 norm.</summary>
    public required double ResidualNorm { get; init; }

    /// <summary>Max absolute component of the residual.</summary>
    public required double MaxAbsComponent { get; init; }
}

/// <summary>
/// Convergence view: iteration history plots.
/// </summary>
public sealed class ConvergenceViewPayload : ViewPayload
{
    public override string ViewType => "convergence";

    /// <summary>Structured plot data from solver history.</summary>
    public required ConvergencePlotData PlotData { get; init; }

    /// <summary>Final objective value.</summary>
    public required double FinalObjective { get; init; }

    /// <summary>Final residual norm.</summary>
    public required double FinalResidualNorm { get; init; }

    /// <summary>Whether the solver converged.</summary>
    public bool? Converged { get; init; }

    /// <summary>Termination reason.</summary>
    public string? TerminationReason { get; init; }
}

/// <summary>
/// Comparison overlay: external comparison results displayed alongside fields.
/// </summary>
public sealed class ComparisonOverlayPayload : ViewPayload
{
    public override string ViewType => "comparison_overlay";

    /// <summary>Summary of comparison outcomes.</summary>
    public required IReadOnlyList<ComparisonSummaryEntry> Entries { get; init; }

    /// <summary>Total comparisons executed.</summary>
    public required int TotalCount { get; init; }

    /// <summary>Number of passed comparisons.</summary>
    public required int PassCount { get; init; }

    /// <summary>Number of failed comparisons.</summary>
    public required int FailCount { get; init; }

    /// <summary>Number of invalid comparisons.</summary>
    public required int InvalidCount { get; init; }
}

/// <summary>
/// A single comparison result entry for overlay display.
/// </summary>
public sealed class ComparisonSummaryEntry
{
    /// <summary>Observable ID that was compared.</summary>
    public required string ObservableId { get; init; }

    /// <summary>Comparison rule used.</summary>
    public required string ComparisonRule { get; init; }

    /// <summary>Outcome string: "Pass", "Fail", "Invalid".</summary>
    public required string Outcome { get; init; }

    /// <summary>Primary metric value (e.g., max relative error).</summary>
    public required double PrimaryMetric { get; init; }

    /// <summary>Metric name.</summary>
    public required string PrimaryMetricName { get; init; }

    /// <summary>Human-readable message.</summary>
    public required string Message { get; init; }
}

/// <summary>
/// Spectrum diagnostics view: eigenvalue/condition number information
/// from linearization state.
/// </summary>
public sealed class SpectrumViewPayload : ViewPayload
{
    public override string ViewType => "spectrum";

    /// <summary>Spectral diagnostic key-value pairs.</summary>
    public required IReadOnlyDictionary<string, double> Diagnostics { get; init; }

    /// <summary>Gradient-like residual norm ||J^T M Upsilon||.</summary>
    public required double GradientResidualNorm { get; init; }

    /// <summary>Jacobian input dimension.</summary>
    public required int JacobianInputDim { get; init; }

    /// <summary>Jacobian output dimension.</summary>
    public required int JacobianOutputDim { get; init; }
}
