using System.Text.Json.Serialization;

namespace Gu.Phase2.Continuation;

/// <summary>
/// Configuration for a pseudo-arclength continuation run.
/// Per IMPLEMENTATION_PLAN_P2.md Section 10.2.
///
/// Tracks a solution family G(u, lambda) = 0 using predictor-corrector steps:
/// 1. tangent predictor from previous converged states,
/// 2. augmented corrector solve,
/// 3. spectrum and conditioning probe,
/// 4. branch event detection,
/// 5. artifact emission.
/// </summary>
public sealed class ContinuationSpec
{
    /// <summary>Name of the continuation parameter (e.g. "gauge-lambda", "mesh-scale").</summary>
    [JsonPropertyName("parameterName")]
    public required string ParameterName { get; init; }

    /// <summary>Starting value of the continuation parameter.</summary>
    [JsonPropertyName("lambdaStart")]
    public required double LambdaStart { get; init; }

    /// <summary>Target ending value of the continuation parameter.</summary>
    [JsonPropertyName("lambdaEnd")]
    public required double LambdaEnd { get; init; }

    /// <summary>Initial step size in arclength.</summary>
    [JsonPropertyName("initialStepSize")]
    public required double InitialStepSize { get; init; }

    /// <summary>Maximum number of continuation steps.</summary>
    [JsonPropertyName("maxSteps")]
    public required int MaxSteps { get; init; }

    /// <summary>Minimum allowed step size (below this, continuation terminates).</summary>
    [JsonPropertyName("minStepSize")]
    public required double MinStepSize { get; init; }

    /// <summary>Maximum allowed step size.</summary>
    [JsonPropertyName("maxStepSize")]
    public required double MaxStepSize { get; init; }

    /// <summary>Convergence tolerance for the corrector solve.</summary>
    [JsonPropertyName("correctorTolerance")]
    public required double CorrectorTolerance { get; init; }

    /// <summary>Maximum corrector iterations per step.</summary>
    [JsonPropertyName("maxCorrectorIterations")]
    public required int MaxCorrectorIterations { get; init; }

    /// <summary>Branch manifest ID for this continuation.</summary>
    [JsonPropertyName("branchManifestId")]
    public required string BranchManifestId { get; init; }

    /// <summary>Whether to probe the spectrum at each step.</summary>
    [JsonPropertyName("probeSpectrum")]
    public bool ProbeSpectrum { get; init; }

    /// <summary>Number of eigenvalues to probe (if ProbeSpectrum is true).</summary>
    [JsonPropertyName("spectrumProbeCount")]
    public int SpectrumProbeCount { get; init; } = 6;
}
