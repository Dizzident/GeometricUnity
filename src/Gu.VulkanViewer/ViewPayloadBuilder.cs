using Gu.Core;
using Gu.Geometry;
using Gu.Solvers;

namespace Gu.VulkanViewer;

/// <summary>
/// Converts artifact data (FieldTensor, DerivedState, SolverResult, etc.)
/// into view payloads ready for native Vulkan rendering.
/// All operations are read-only: artifact data is never modified (IX-5).
/// </summary>
public sealed class ViewPayloadBuilder
{
    private readonly ScalarFieldVisualizer _defaultVisualizer;
    private readonly ScalarFieldVisualizer _divergingVisualizer;

    public ViewPayloadBuilder(string colorScheme = "viridis")
    {
        _defaultVisualizer = new ScalarFieldVisualizer(
            new ColorMapper(colorScheme));
        _divergingVisualizer = new ScalarFieldVisualizer(
            new ColorMapper("diverging"), centerAtZero: true);
    }

    /// <summary>
    /// Builds a geometry-only view from a mesh.
    /// Colors are uniform (neutral gray).
    /// </summary>
    public GeometryViewPayload BuildGeometryView(SimplicialMesh mesh)
    {
        ArgumentNullException.ThrowIfNull(mesh);

        // Create a uniform-color field for geometry display
        var uniformField = new FieldTensor
        {
            Label = "uniform",
            Signature = new TensorSignature
            {
                AmbientSpaceId = "display",
                CarrierType = "scalar",
                Degree = "0",
                LieAlgebraBasisId = "N/A",
                ComponentOrderId = "N/A",
                MemoryLayout = "dense-row-major",
                NumericPrecision = "float64",
            },
            Coefficients = new double[mesh.VertexCount],
            Shape = new[] { mesh.VertexCount },
        };

        // Use fixed range so everything maps to midpoint (neutral color)
        var neutralVisualizer = new ScalarFieldVisualizer(
            new ColorMapper("coolwarm"), fixedMin: -1.0, fixedMax: 1.0);
        var visData = neutralVisualizer.PrepareVisualization(uniformField, mesh);

        return new GeometryViewPayload
        {
            MeshData = visData,
            EmbeddingDimension = mesh.EmbeddingDimension,
            CellCount = mesh.CellVertices.Length,
            EdgeCount = mesh.EdgeCount,
        };
    }

    /// <summary>
    /// Builds a field view for a specific field tensor on the mesh.
    /// </summary>
    public FieldViewPayload BuildFieldView(
        FieldTensor field,
        SimplicialMesh mesh,
        string fieldMode)
    {
        ArgumentNullException.ThrowIfNull(field);
        ArgumentNullException.ThrowIfNull(mesh);
        ArgumentException.ThrowIfNullOrWhiteSpace(fieldMode);

        var visData = _defaultVisualizer.PrepareVisualization(field, mesh);
        double norm = ComputeL2Norm(field.Coefficients);

        return new FieldViewPayload
        {
            FieldData = visData,
            FieldMode = fieldMode,
            FieldLabel = field.Label,
            CoefficientCount = field.Coefficients.Length,
            FieldNorm = norm,
        };
    }

    /// <summary>
    /// Builds field views for all derived state fields.
    /// Returns views for curvature, torsion, shiab, and residual.
    /// </summary>
    public IReadOnlyList<FieldViewPayload> BuildDerivedFieldViews(
        DerivedState derived,
        SimplicialMesh mesh)
    {
        ArgumentNullException.ThrowIfNull(derived);
        ArgumentNullException.ThrowIfNull(mesh);

        return new[]
        {
            BuildFieldView(derived.CurvatureF, mesh, "curvature"),
            BuildFieldView(derived.TorsionT, mesh, "torsion"),
            BuildFieldView(derived.ShiabS, mesh, "shiab"),
            BuildFieldView(derived.ResidualUpsilon, mesh, "residual"),
        };
    }

    /// <summary>
    /// Builds a residual view with diverging color map and diagnostics.
    /// </summary>
    public ResidualViewPayload BuildResidualView(
        FieldTensor residualField,
        SimplicialMesh mesh,
        double objectiveValue)
    {
        ArgumentNullException.ThrowIfNull(residualField);
        ArgumentNullException.ThrowIfNull(mesh);

        var visData = _divergingVisualizer.PrepareVisualization(residualField, mesh);
        double norm = ComputeL2Norm(residualField.Coefficients);
        double maxAbs = ComputeMaxAbs(residualField.Coefficients);

        return new ResidualViewPayload
        {
            ResidualData = visData,
            ObjectiveValue = objectiveValue,
            ResidualNorm = norm,
            MaxAbsComponent = maxAbs,
        };
    }

    /// <summary>
    /// Builds a residual view from a ResidualBundle.
    /// </summary>
    public ResidualViewPayload BuildResidualView(
        ResidualBundle bundle,
        FieldTensor residualField,
        SimplicialMesh mesh)
    {
        ArgumentNullException.ThrowIfNull(bundle);
        return BuildResidualView(residualField, mesh, bundle.ObjectiveValue);
    }

    /// <summary>
    /// Builds a convergence view from solver history.
    /// </summary>
    public ConvergenceViewPayload BuildConvergenceView(
        IReadOnlyList<ConvergenceRecord> history,
        bool? converged = null,
        string? terminationReason = null)
    {
        ArgumentNullException.ThrowIfNull(history);
        if (history.Count == 0)
            throw new ArgumentException("History must contain at least one record.", nameof(history));

        var plotData = ConvergencePlotter.ExtractAll(history, converged, terminationReason);
        var last = history[history.Count - 1];

        return new ConvergenceViewPayload
        {
            PlotData = plotData,
            FinalObjective = last.Objective,
            FinalResidualNorm = last.ResidualNorm,
            Converged = converged,
            TerminationReason = terminationReason,
        };
    }

    /// <summary>
    /// Builds a convergence view from a SolverResult.
    /// </summary>
    public ConvergenceViewPayload BuildConvergenceView(SolverResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return BuildConvergenceView(
            result.History,
            result.Converged,
            result.TerminationReason);
    }

    /// <summary>
    /// Builds a spectrum diagnostics view from linearization state.
    /// </summary>
    public SpectrumViewPayload BuildSpectrumView(LinearizationState linearization)
    {
        ArgumentNullException.ThrowIfNull(linearization);

        double gradNorm = ComputeL2Norm(linearization.GradientLikeResidual.Coefficients);

        return new SpectrumViewPayload
        {
            Diagnostics = linearization.SpectralDiagnostics
                ?? new Dictionary<string, double>(),
            GradientResidualNorm = gradNorm,
            JacobianInputDim = linearization.Jacobian.Cols,
            JacobianOutputDim = linearization.Jacobian.Rows,
        };
    }

    private static double ComputeL2Norm(double[] values)
    {
        double sumSq = 0.0;
        for (int i = 0; i < values.Length; i++)
            sumSq += values[i] * values[i];
        return Math.Sqrt(sumSq);
    }

    private static double ComputeMaxAbs(double[] values)
    {
        double max = 0.0;
        for (int i = 0; i < values.Length; i++)
        {
            double abs = Math.Abs(values[i]);
            if (abs > max) max = abs;
        }
        return max;
    }
}
