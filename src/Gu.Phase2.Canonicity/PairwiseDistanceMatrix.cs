using System.Text.Json.Serialization;

namespace Gu.Phase2.Canonicity;

/// <summary>
/// Symmetric matrix of pairwise distances between branch variants for a given metric.
/// Distances[i,j] == Distances[j,i], and Distances[i,i] == 0.
/// NaN entries indicate that the distance could not be computed (e.g., missing ObservedState).
/// </summary>
public sealed class PairwiseDistanceMatrix
{
    /// <summary>Metric identifier: "D_obs_max", "D_obs_l2_sum", "D_dyn", "D_conv", "D_stab".</summary>
    [JsonPropertyName("metricId")]
    public required string MetricId { get; init; }

    /// <summary>Branch variant IDs labeling rows and columns.</summary>
    [JsonPropertyName("branchIds")]
    public required IReadOnlyList<string> BranchIds { get; init; }

    /// <summary>Symmetric distance matrix with zero diagonal.</summary>
    [JsonPropertyName("distances")]
    public required double[,] Distances { get; init; }

    /// <summary>Maximum distance in the matrix (ignoring NaN).</summary>
    [JsonIgnore]
    public double MaxDistance
    {
        get
        {
            int n = BranchIds.Count;
            double max = 0.0;
            for (int i = 0; i < n; i++)
                for (int j = i + 1; j < n; j++)
                    if (!double.IsNaN(Distances[i, j]) && Distances[i, j] > max)
                        max = Distances[i, j];
            return max;
        }
    }
}
