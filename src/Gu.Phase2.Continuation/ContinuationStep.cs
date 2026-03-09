using System.Text.Json.Serialization;
using Gu.Core;
using Gu.Phase2.Stability;

namespace Gu.Phase2.Continuation;

/// <summary>
/// A single converged step along a continuation path.
/// Records the state, parameter value, solver diagnostics, and any detected events.
/// </summary>
public sealed class ContinuationStep
{
    /// <summary>Step index (0-based).</summary>
    [JsonPropertyName("stepIndex")]
    public required int StepIndex { get; init; }

    /// <summary>Parameter value lambda at this step.</summary>
    [JsonPropertyName("lambda")]
    public required double Lambda { get; init; }

    /// <summary>Arclength s along the continuation path.</summary>
    [JsonPropertyName("arclength")]
    public required double Arclength { get; init; }

    /// <summary>Step size used to reach this step.</summary>
    [JsonPropertyName("stepSize")]
    public required double StepSize { get; init; }

    /// <summary>Residual norm ||G(u, lambda)|| at this step.</summary>
    [JsonPropertyName("residualNorm")]
    public required double ResidualNorm { get; init; }

    /// <summary>Number of corrector iterations used.</summary>
    [JsonPropertyName("correctorIterations")]
    public required int CorrectorIterations { get; init; }

    /// <summary>Whether the corrector converged for this step.</summary>
    [JsonPropertyName("correctorConverged")]
    public required bool CorrectorConverged { get; init; }

    /// <summary>Spectrum probe result at this step (null if not probed).</summary>
    [JsonPropertyName("spectrumResult")]
    public SpectrumRecord? SpectrumResult { get; init; }

    /// <summary>Events detected at this step.</summary>
    [JsonPropertyName("events")]
    public required IReadOnlyList<ContinuationEvent> Events { get; init; }
}
