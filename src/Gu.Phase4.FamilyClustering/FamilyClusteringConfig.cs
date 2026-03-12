using System.Text.Json.Serialization;

namespace Gu.Phase4.FamilyClustering;

/// <summary>
/// Configuration for FamilyClusteringEngine.
/// </summary>
public sealed class FamilyClusteringConfig
{
    /// <summary>
    /// Relative tolerance for eigenvalue proximity clustering.
    /// Two families are "proximate" if |mean_i - mean_j| / max(mean_i, mean_j, eps) &lt;= this value.
    /// Default 0.3 (30% relative tolerance).
    /// </summary>
    [JsonPropertyName("eigenvalueProximityRelTol")]
    public double EigenvalueProximityRelTol { get; init; } = 0.3;

    /// <summary>Default configuration.</summary>
    public static FamilyClusteringConfig Default { get; } = new();
}
