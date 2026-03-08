using Gu.Artifacts;
using Gu.Core;
using Gu.Geometry;

namespace Gu.VulkanViewer;

/// <summary>
/// Loads artifact run folders and prepares view payloads for visualization.
/// All operations are read-only: artifact data is consumed but never modified (IX-5).
/// </summary>
public sealed class ArtifactViewerService
{
    private readonly ViewPayloadBuilder _payloadBuilder;

    public ArtifactViewerService(string colorScheme = "viridis")
    {
        _payloadBuilder = new ViewPayloadBuilder(colorScheme);
    }

    /// <summary>
    /// Loads a run folder and returns a snapshot with all available view data.
    /// </summary>
    public WorkbenchSnapshot LoadRunFolder(string runFolderPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(runFolderPath);

        var reader = new RunFolderReader(runFolderPath);
        if (!reader.HasValidStructure())
        {
            throw new InvalidOperationException(
                $"Run folder at '{runFolderPath}' does not have a valid structure.");
        }

        var manifest = reader.ReadBranchManifest();
        var geometry = reader.ReadGeometry();
        var initialState = reader.ReadInitialState();
        var finalState = reader.ReadFinalState();
        var residuals = reader.ReadResidualBundle();
        var linearization = reader.ReadLinearizationBundle();
        var observedState = reader.ReadObservedState();
        var validation = reader.ReadValidationBundle();
        var artifactId = reader.ReadArtifactId();

        // Try to read derived state
        var derivedState = reader.ReadJson<DerivedState>(RunFolderLayout.DerivedStateFile);

        // Try to read convergence history from solver result
        var solverResult = reader.ReadJson<Solvers.SolverResult>("state/solver_result.json");

        // Try to read comparison records
        var comparisonRecords = LoadComparisonRecords(reader);

        return new WorkbenchSnapshot
        {
            RunFolderPath = runFolderPath,
            ArtifactId = artifactId,
            BranchManifest = manifest,
            Geometry = geometry,
            InitialState = initialState,
            FinalState = finalState,
            DerivedState = derivedState,
            Residuals = residuals,
            Linearization = linearization,
            ObservedState = observedState,
            Validation = validation,
            SolverResult = solverResult,
            ComparisonRecords = comparisonRecords,
            LoadedAt = DateTimeOffset.UtcNow,
        };
    }

    /// <summary>
    /// Prepares all available view payloads for a loaded snapshot.
    /// Returns only views for which data is available.
    /// </summary>
    public IReadOnlyList<ViewPayload> PrepareViews(
        WorkbenchSnapshot snapshot,
        SimplicialMesh mesh)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(mesh);

        var views = new List<ViewPayload>();

        // Geometry view (always available if mesh is provided)
        views.Add(_payloadBuilder.BuildGeometryView(mesh));

        // Field views from derived state
        if (snapshot.DerivedState is not null)
        {
            views.AddRange(
                _payloadBuilder.BuildDerivedFieldViews(snapshot.DerivedState, mesh));
        }

        // Field view from initial omega
        if (snapshot.InitialState is not null)
        {
            views.Add(_payloadBuilder.BuildFieldView(
                snapshot.InitialState.Omega, mesh, "omega"));
        }

        // Residual view
        if (snapshot.DerivedState is not null && snapshot.Residuals is not null)
        {
            views.Add(_payloadBuilder.BuildResidualView(
                snapshot.Residuals,
                snapshot.DerivedState.ResidualUpsilon,
                mesh));
        }

        // Convergence view
        if (snapshot.SolverResult is not null)
        {
            views.Add(_payloadBuilder.BuildConvergenceView(snapshot.SolverResult));
        }

        // Spectrum view
        if (snapshot.Linearization is not null)
        {
            views.Add(_payloadBuilder.BuildSpectrumView(snapshot.Linearization));
        }

        // Comparison overlay
        if (snapshot.ComparisonRecords.Count > 0)
        {
            views.Add(BuildComparisonOverlay(snapshot.ComparisonRecords));
        }

        return views;
    }

    private static ComparisonOverlayPayload BuildComparisonOverlay(
        IReadOnlyList<ExternalComparison.ComparisonRecord> records)
    {
        var entries = new List<ComparisonSummaryEntry>();
        int passCount = 0, failCount = 0, invalidCount = 0;

        foreach (var rec in records)
        {
            string outcomeName = rec.Outcome.ToString();
            switch (rec.Outcome)
            {
                case ExternalComparison.ComparisonOutcome.Pass:
                    passCount++;
                    break;
                case ExternalComparison.ComparisonOutcome.Fail:
                    failCount++;
                    break;
                default:
                    invalidCount++;
                    break;
            }

            // Pick the primary metric (first one available)
            string metricName = "N/A";
            double metricValue = 0.0;
            foreach (var kvp in rec.Metrics)
            {
                metricName = kvp.Key;
                metricValue = kvp.Value;
                break;
            }

            entries.Add(new ComparisonSummaryEntry
            {
                ObservableId = rec.ObservableId,
                ComparisonRule = rec.ComparisonRule,
                Outcome = outcomeName,
                PrimaryMetric = metricValue,
                PrimaryMetricName = metricName,
                Message = rec.Message,
            });
        }

        return new ComparisonOverlayPayload
        {
            Entries = entries,
            TotalCount = records.Count,
            PassCount = passCount,
            FailCount = failCount,
            InvalidCount = invalidCount,
        };
    }

    private static IReadOnlyList<ExternalComparison.ComparisonRecord> LoadComparisonRecords(
        RunFolderReader reader)
    {
        // Try to load comparison records from a known path
        var records = reader.ReadJson<ExternalComparison.ComparisonRecord[]>(
            "validation/comparison_records.json");
        return records ?? Array.Empty<ExternalComparison.ComparisonRecord>();
    }
}

/// <summary>
/// Immutable snapshot of all artifact data loaded from a run folder.
/// </summary>
public sealed class WorkbenchSnapshot
{
    public required string RunFolderPath { get; init; }
    public string? ArtifactId { get; init; }
    public BranchManifest? BranchManifest { get; init; }
    public GeometryContext? Geometry { get; init; }
    public DiscreteState? InitialState { get; init; }
    public DiscreteState? FinalState { get; init; }
    public DerivedState? DerivedState { get; init; }
    public ResidualBundle? Residuals { get; init; }
    public LinearizationState? Linearization { get; init; }
    public ObservedState? ObservedState { get; init; }
    public ValidationBundle? Validation { get; init; }
    public Solvers.SolverResult? SolverResult { get; init; }
    public IReadOnlyList<ExternalComparison.ComparisonRecord> ComparisonRecords { get; init; }
        = Array.Empty<ExternalComparison.ComparisonRecord>();
    public required DateTimeOffset LoadedAt { get; init; }
}
