using System.Text.Json.Serialization;

namespace Gu.Phase3.Spectra;

/// <summary>
/// A cluster of near-degenerate eigenvalues identified by spectral gap analysis.
///
/// Multiplicity must never be inferred from one run alone when
/// continuation/refinement data is available.
/// </summary>
public sealed class ModeCluster
{
    /// <summary>Unique cluster identifier.</summary>
    [JsonPropertyName("clusterId")]
    public required string ClusterId { get; init; }

    /// <summary>Mode indices (0-based) in this cluster.</summary>
    [JsonPropertyName("modeIndices")]
    public required int[] ModeIndices { get; init; }

    /// <summary>Number of modes in the cluster (apparent multiplicity).</summary>
    [JsonPropertyName("multiplicity")]
    public int Multiplicity => ModeIndices.Length;

    /// <summary>Mean eigenvalue of the cluster.</summary>
    [JsonPropertyName("meanEigenvalue")]
    public required double MeanEigenvalue { get; init; }

    /// <summary>Spread of eigenvalues within the cluster: max - min.</summary>
    [JsonPropertyName("eigenvalueSpread")]
    public required double EigenvalueSpread { get; init; }

    /// <summary>
    /// Spectral gap to the next cluster (ratio of inter-cluster to intra-cluster gap).
    /// Null if this is the last cluster.
    /// </summary>
    [JsonPropertyName("spectralGapToNext")]
    public double? SpectralGapToNext { get; init; }
}
