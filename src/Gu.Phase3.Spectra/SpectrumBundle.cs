using System.Text.Json.Serialization;

namespace Gu.Phase3.Spectra;

/// <summary>
/// Complete result of a spectral solve: eigenvalues, modes, clusters, and diagnostics.
/// </summary>
public sealed class SpectrumBundle
{
    /// <summary>Unique bundle identifier.</summary>
    [JsonPropertyName("spectrumId")]
    public required string SpectrumId { get; init; }

    /// <summary>Background state ID.</summary>
    [JsonPropertyName("backgroundId")]
    public required string BackgroundId { get; init; }

    /// <summary>Operator bundle ID that produced this spectrum.</summary>
    [JsonPropertyName("operatorBundleId")]
    public required string OperatorBundleId { get; init; }

    /// <summary>Operator type used (GN or Full Hessian).</summary>
    [JsonPropertyName("operatorType")]
    public required SpectralOperatorType OperatorType { get; init; }

    /// <summary>Physical mode formulation (P1/P2).</summary>
    [JsonPropertyName("formulation")]
    public required PhysicalModeFormulation Formulation { get; init; }

    /// <summary>Solver method used.</summary>
    [JsonPropertyName("solverMethod")]
    public required string SolverMethod { get; init; }

    /// <summary>State-space dimension.</summary>
    [JsonPropertyName("stateDimension")]
    public required int StateDimension { get; init; }

    /// <summary>Computed modes, sorted by eigenvalue ascending.</summary>
    [JsonPropertyName("modes")]
    public required IReadOnlyList<ModeRecord> Modes { get; init; }

    /// <summary>Spectral clusters (groups of near-degenerate eigenvalues).</summary>
    [JsonPropertyName("clusters")]
    public required IReadOnlyList<ModeCluster> Clusters { get; init; }

    /// <summary>Null mode diagnosis.</summary>
    [JsonPropertyName("nullModeDiagnosis")]
    public NullModeDiagnosis? NullModeDiagnosis { get; init; }

    /// <summary>
    /// Convergence status: "converged", "partially-converged", "failed".
    /// </summary>
    [JsonPropertyName("convergenceStatus")]
    public required string ConvergenceStatus { get; init; }

    /// <summary>Number of iterations used.</summary>
    [JsonPropertyName("iterationsUsed")]
    public required int IterationsUsed { get; init; }

    /// <summary>Maximum orthogonality defect among computed modes: max |v_i^T M v_j| for i != j.</summary>
    [JsonPropertyName("maxOrthogonalityDefect")]
    public required double MaxOrthogonalityDefect { get; init; }

    /// <summary>Backend used to compute this spectrum: "cpu" or "cuda".</summary>
    [JsonPropertyName("computedWithBackend")]
    public string ComputedWithBackend { get; init; } = "cpu";

    /// <summary>
    /// Diagnostic notes emitted during the solve (e.g., convergence warnings).
    /// </summary>
    [JsonPropertyName("diagnosticNotes")]
    public List<string> DiagnosticNotes { get; init; } = new();
}
