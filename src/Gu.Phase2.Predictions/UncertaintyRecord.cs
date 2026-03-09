using System.Text.Json.Serialization;

namespace Gu.Phase2.Predictions;

/// <summary>
/// Decomposes uncertainty into its constituent sources.
/// A value of -1 indicates the uncertainty component has not been estimated.
/// </summary>
public sealed class UncertaintyRecord
{
    /// <summary>Discretization uncertainty (mesh/grid effects).</summary>
    [JsonPropertyName("discretization")]
    public required double Discretization { get; init; }

    /// <summary>Solver convergence uncertainty.</summary>
    [JsonPropertyName("solver")]
    public required double Solver { get; init; }

    /// <summary>Branch-dependence uncertainty.</summary>
    [JsonPropertyName("branch")]
    public required double Branch { get; init; }

    /// <summary>Extraction/observation pipeline uncertainty.</summary>
    [JsonPropertyName("extraction")]
    public required double Extraction { get; init; }

    /// <summary>Calibration procedure uncertainty.</summary>
    [JsonPropertyName("calibration")]
    public required double Calibration { get; init; }

    /// <summary>External data asset uncertainty.</summary>
    [JsonPropertyName("dataAsset")]
    public required double DataAsset { get; init; }

    /// <summary>Create an uncertainty record with all components unestimated (-1).</summary>
    public static UncertaintyRecord Unestimated() => new()
    {
        Discretization = -1,
        Solver = -1,
        Branch = -1,
        Extraction = -1,
        Calibration = -1,
        DataAsset = -1,
    };
}
