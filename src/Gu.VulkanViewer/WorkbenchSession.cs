using System.Text;
using System.Text.Json;
using Gu.Artifacts;
using Gu.Core;
using Gu.Core.Serialization;
using Gu.Geometry;
using Gu.Solvers;

namespace Gu.VulkanViewer;

/// <summary>
/// Describes a field available for visualization within a workbench session.
/// </summary>
public sealed class AvailableField
{
    /// <summary>Human-readable name for the field.</summary>
    public required string Name { get; init; }

    /// <summary>Description of what this field represents.</summary>
    public required string Description { get; init; }

    /// <summary>The field tensor data.</summary>
    public required FieldTensor Field { get; init; }

    /// <summary>Whether a diverging color map is recommended (e.g., for residuals).</summary>
    public bool DivergingRecommended { get; init; }
}

/// <summary>
/// Orchestrates a visualization session for Geometric Unity solver output.
///
/// A workbench session can be constructed from:
/// 1. A run folder (using <see cref="RunFolderReader"/>) with an accompanying mesh.
/// 2. An <see cref="ArtifactBundle"/> with an accompanying mesh.
/// 3. A <see cref="SolverResult"/> with an accompanying mesh.
///
/// The session discovers available fields, applies color mapping, produces
/// <see cref="VisualizationData"/> for rendering, and can export to OBJ/PLY/CSV
/// for use in external visualization tools.
/// </summary>
public sealed class WorkbenchSession
{
    private readonly SimplicialMesh _mesh;
    private readonly List<AvailableField> _availableFields = new();
    private readonly List<ConvergenceRecord> _convergenceHistory = new();
    private bool? _converged;
    private string? _terminationReason;

    /// <summary>
    /// Initializes a new workbench session with the given mesh.
    /// </summary>
    /// <param name="mesh">The simplicial mesh for visualization.</param>
    public WorkbenchSession(SimplicialMesh mesh)
    {
        _mesh = mesh ?? throw new ArgumentNullException(nameof(mesh));
    }

    /// <summary>The mesh used for visualization.</summary>
    public SimplicialMesh Mesh => _mesh;

    /// <summary>Fields available for visualization.</summary>
    public IReadOnlyList<AvailableField> AvailableFields => _availableFields;

    /// <summary>Convergence history, if loaded from a solver result.</summary>
    public IReadOnlyList<ConvergenceRecord> ConvergenceHistory => _convergenceHistory;

    /// <summary>
    /// Loads fields from a run folder using <see cref="RunFolderReader"/>.
    /// Discovers initial state, final state, and derived state fields.
    /// </summary>
    /// <param name="runFolderPath">Path to the run folder root.</param>
    /// <returns>The number of fields discovered.</returns>
    public int LoadFromRunFolder(string runFolderPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(runFolderPath);

        var reader = new RunFolderReader(runFolderPath);

        int startCount = _availableFields.Count;

        // Load initial state omega field.
        var initialState = reader.ReadInitialState();
        if (initialState is not null)
        {
            _availableFields.Add(new AvailableField
            {
                Name = "Initial Omega",
                Description = "Initial connection field omega_h",
                Field = initialState.Omega,
                DivergingRecommended = false,
            });
        }

        // Load final state omega field.
        var finalState = reader.ReadFinalState();
        if (finalState is not null)
        {
            _availableFields.Add(new AvailableField
            {
                Name = "Final Omega",
                Description = "Final connection field omega_h after solving",
                Field = finalState.Omega,
                DivergingRecommended = false,
            });
        }

        // Load derived state fields.
        var derivedState = reader.ReadJson<DerivedState>(RunFolderLayout.DerivedStateFile);
        if (derivedState is not null)
        {
            AddDerivedStateFields(derivedState);
        }

        return _availableFields.Count - startCount;
    }

    /// <summary>
    /// Loads fields from an <see cref="ArtifactBundle"/>.
    /// </summary>
    /// <param name="bundle">The artifact bundle.</param>
    /// <returns>The number of fields discovered.</returns>
    public int LoadFromBundle(ArtifactBundle bundle)
    {
        ArgumentNullException.ThrowIfNull(bundle);

        int startCount = _availableFields.Count;

        if (bundle.InitialState is not null)
        {
            _availableFields.Add(new AvailableField
            {
                Name = "Initial Omega",
                Description = "Initial connection field omega_h",
                Field = bundle.InitialState.Omega,
                DivergingRecommended = false,
            });
        }

        if (bundle.FinalState is not null)
        {
            _availableFields.Add(new AvailableField
            {
                Name = "Final Omega",
                Description = "Final connection field omega_h after solving",
                Field = bundle.FinalState.Omega,
                DivergingRecommended = false,
            });
        }

        if (bundle.DerivedState is not null)
        {
            AddDerivedStateFields(bundle.DerivedState);
        }

        return _availableFields.Count - startCount;
    }

    /// <summary>
    /// Loads fields and convergence history from a <see cref="SolverResult"/>.
    /// </summary>
    /// <param name="result">The solver result.</param>
    /// <param name="initialOmega">Optional initial omega for comparison visualization.</param>
    /// <returns>The number of fields discovered.</returns>
    public int LoadFromSolverResult(SolverResult result, FieldTensor? initialOmega = null)
    {
        ArgumentNullException.ThrowIfNull(result);

        int startCount = _availableFields.Count;

        if (initialOmega is not null)
        {
            _availableFields.Add(new AvailableField
            {
                Name = "Initial Omega",
                Description = "Initial connection field omega_h",
                Field = initialOmega,
                DivergingRecommended = false,
            });
        }

        _availableFields.Add(new AvailableField
        {
            Name = "Final Omega",
            Description = "Final connection field omega_h after solving",
            Field = result.FinalOmega,
            DivergingRecommended = false,
        });

        AddDerivedStateFields(result.FinalDerivedState);

        // Load convergence history.
        _convergenceHistory.Clear();
        _convergenceHistory.AddRange(result.History);
        _converged = result.Converged;
        _terminationReason = result.TerminationReason;

        return _availableFields.Count - startCount;
    }

    /// <summary>
    /// Prepares visualization data for a specific field using the configured color mapper.
    /// </summary>
    /// <param name="fieldName">Name of the field from <see cref="AvailableFields"/>.</param>
    /// <param name="colorScheme">Color scheme name (default: auto-selects based on field type).</param>
    /// <returns>Visualization data ready for rendering or export.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="fieldName"/> is not found.</exception>
    public VisualizationData PrepareVisualization(string fieldName, string? colorScheme = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fieldName);

        var available = _availableFields.FirstOrDefault(f =>
            string.Equals(f.Name, fieldName, StringComparison.OrdinalIgnoreCase));

        if (available is null)
        {
            throw new ArgumentException(
                $"Field '{fieldName}' not found. Available: {string.Join(", ", _availableFields.Select(f => f.Name))}",
                nameof(fieldName));
        }

        // Auto-select color scheme if not specified.
        string scheme = colorScheme ?? (available.DivergingRecommended ? "diverging" : "viridis");
        var mapper = new ColorMapper(scheme);
        var visualizer = new ScalarFieldVisualizer(mapper, centerAtZero: available.DivergingRecommended);

        return visualizer.PrepareVisualization(available.Field, _mesh);
    }

    /// <summary>
    /// Gets convergence plot data, if convergence history is available.
    /// </summary>
    /// <returns>The convergence plot data, or null if no history is loaded.</returns>
    public ConvergencePlotData? GetConvergencePlot()
    {
        if (_convergenceHistory.Count == 0) return null;

        return ConvergencePlotter.ExtractAll(_convergenceHistory, _converged, _terminationReason);
    }

    /// <summary>
    /// Exports a field visualization to OBJ + CSV files.
    /// </summary>
    /// <param name="fieldName">Name of the field to export.</param>
    /// <param name="outputDirectory">Directory to write export files to.</param>
    /// <param name="colorScheme">Optional color scheme override.</param>
    /// <returns>Paths of the exported files.</returns>
    public (string ObjPath, string ColorCsvPath) ExportToObj(
        string fieldName,
        string outputDirectory,
        string? colorScheme = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(outputDirectory);
        Directory.CreateDirectory(outputDirectory);

        var data = PrepareVisualization(fieldName, colorScheme);
        string safeName = SanitizeFileName(fieldName);
        string objPath = Path.Combine(outputDirectory, $"{safeName}.obj");
        string csvPath = Path.Combine(outputDirectory, $"{safeName}_colors.csv");

        MeshExporter.WriteFiles(data, objPath, csvPath);

        return (objPath, csvPath);
    }

    /// <summary>
    /// Exports a field visualization to PLY format (single file with embedded colors).
    /// </summary>
    /// <param name="fieldName">Name of the field to export.</param>
    /// <param name="outputDirectory">Directory to write export files to.</param>
    /// <param name="colorScheme">Optional color scheme override.</param>
    /// <returns>Path of the exported PLY file.</returns>
    public string ExportToPly(
        string fieldName,
        string outputDirectory,
        string? colorScheme = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(outputDirectory);
        Directory.CreateDirectory(outputDirectory);

        var data = PrepareVisualization(fieldName, colorScheme);
        string safeName = SanitizeFileName(fieldName);
        string plyPath = Path.Combine(outputDirectory, $"{safeName}.ply");

        File.WriteAllText(plyPath, MeshExporter.ToPly(data), Encoding.UTF8);

        return plyPath;
    }

    /// <summary>
    /// Exports convergence diagnostics to CSV.
    /// </summary>
    /// <param name="outputPath">Path for the output CSV file.</param>
    /// <returns>True if convergence data was available and exported.</returns>
    public bool ExportConvergenceCsv(string outputPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);

        if (_convergenceHistory.Count == 0) return false;

        string? dir = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }

        string csv = ConvergencePlotter.ToCsv(_convergenceHistory);
        File.WriteAllText(outputPath, csv, Encoding.UTF8);
        return true;
    }

    /// <summary>
    /// Exports a summary of the session state as JSON, suitable for external tools
    /// or dashboards that need to know what fields are available and their ranges.
    /// </summary>
    /// <returns>JSON string summarizing the session.</returns>
    public string ExportSessionSummaryJson()
    {
        var summary = new
        {
            meshInfo = new
            {
                vertexCount = _mesh.VertexCount,
                edgeCount = _mesh.EdgeCount,
                faceCount = _mesh.FaceCount,
                cellCount = _mesh.CellCount,
                embeddingDimension = _mesh.EmbeddingDimension,
                simplicialDimension = _mesh.SimplicialDimension,
            },
            availableFields = _availableFields.Select(f => new
            {
                name = f.Name,
                description = f.Description,
                coefficientCount = f.Field.Coefficients.Length,
                label = f.Field.Label,
                divergingRecommended = f.DivergingRecommended,
            }).ToArray(),
            convergenceInfo = _convergenceHistory.Count > 0
                ? new
                {
                    iterationCount = _convergenceHistory.Count,
                    converged = _converged,
                    terminationReason = _terminationReason,
                    finalObjective = _convergenceHistory[^1].Objective,
                    finalResidualNorm = _convergenceHistory[^1].ResidualNorm,
                }
                : null,
        };

        return JsonSerializer.Serialize(summary, GuJsonDefaults.Options);
    }

    private void AddDerivedStateFields(DerivedState derived)
    {
        _availableFields.Add(new AvailableField
        {
            Name = "Curvature F",
            Description = "Curvature 2-form F_h",
            Field = derived.CurvatureF,
            DivergingRecommended = false,
        });

        _availableFields.Add(new AvailableField
        {
            Name = "Torsion T",
            Description = "Torsion field T_h",
            Field = derived.TorsionT,
            DivergingRecommended = false,
        });

        _availableFields.Add(new AvailableField
        {
            Name = "Shiab S",
            Description = "Shiab operator field S_h",
            Field = derived.ShiabS,
            DivergingRecommended = false,
        });

        _availableFields.Add(new AvailableField
        {
            Name = "Residual Upsilon",
            Description = "Residual field Upsilon_h = S_h - T_h",
            Field = derived.ResidualUpsilon,
            DivergingRecommended = true,
        });

        if (derived.Diagnostics is not null)
        {
            foreach (var (key, value) in derived.Diagnostics)
            {
                _availableFields.Add(new AvailableField
                {
                    Name = $"Diagnostic: {key}",
                    Description = $"Diagnostic field '{key}'",
                    Field = value,
                    DivergingRecommended = false,
                });
            }
        }
    }

    private static string SanitizeFileName(string name)
    {
        char[] invalid = Path.GetInvalidFileNameChars();
        var sb = new StringBuilder(name.Length);
        foreach (char c in name)
        {
            sb.Append(invalid.Contains(c) || c == ' ' ? '_' : c);
        }

        return sb.ToString();
    }
}
