using System.Text.Json.Serialization;

namespace Gu.Phase2.Canonicity;

/// <summary>
/// Qualitative classification of a branch run's convergence behavior.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum QualitativeClass
{
    /// <summary>Solver converged within tolerance.</summary>
    Converged,

    /// <summary>Solver terminated but did not converge (hit max iterations, diverged, etc.).</summary>
    Failed,

    /// <summary>Solver stagnated (residual stopped decreasing meaningfully).</summary>
    Stalled,
}
