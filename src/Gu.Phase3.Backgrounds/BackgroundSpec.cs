using System.Text.Json.Serialization;

namespace Gu.Phase3.Backgrounds;

/// <summary>
/// Specification for a single background solve attempt.
/// Combines a seed with solve options and branch/environment context.
/// </summary>
public sealed class BackgroundSpec
{
    /// <summary>Unique identifier for this background specification.</summary>
    [JsonPropertyName("specId")]
    public required string SpecId { get; init; }

    /// <summary>Environment identifier.</summary>
    [JsonPropertyName("environmentId")]
    public required string EnvironmentId { get; init; }

    /// <summary>Branch manifest identifier.</summary>
    [JsonPropertyName("branchManifestId")]
    public required string BranchManifestId { get; init; }

    /// <summary>Seed for the background solve.</summary>
    [JsonPropertyName("seed")]
    public required BackgroundSeed Seed { get; init; }

    /// <summary>Solve options (tolerances, method, etc.).</summary>
    [JsonPropertyName("solveOptions")]
    public required BackgroundSolveOptions SolveOptions { get; init; }

    /// <summary>
    /// Gauge choice identifier for this background.
    /// </summary>
    [JsonPropertyName("gaugeChoice")]
    public string? GaugeChoice { get; init; }

    /// <summary>
    /// Continuation coordinates (parameter values) if this background
    /// belongs to a continuation family.
    /// </summary>
    [JsonPropertyName("continuationCoordinates")]
    public IReadOnlyDictionary<string, double>? ContinuationCoordinates { get; init; }

    /// <summary>Human-readable notes.</summary>
    [JsonPropertyName("notes")]
    public string? Notes { get; init; }
}
