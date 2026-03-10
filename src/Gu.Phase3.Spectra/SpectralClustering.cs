namespace Gu.Phase3.Spectra;

/// <summary>
/// Clusters near-degenerate eigenvalues using relative and absolute gap tolerances.
///
/// Algorithm:
/// 1. Sort eigenvalues ascending.
/// 2. Walk sorted values; start a new cluster when the gap to the previous
///    eigenvalue exceeds both relative and absolute thresholds.
/// 3. Assign a ModeClusterId to each group.
/// </summary>
internal static class SpectralClustering
{
    /// <summary>
    /// Cluster sorted eigenvalues into groups of near-degenerate values.
    /// </summary>
    /// <param name="eigenvalues">Sorted eigenvalues (ascending).</param>
    /// <param name="relativeTol">Relative gap tolerance.</param>
    /// <param name="absoluteTol">Absolute gap tolerance.</param>
    /// <param name="backgroundId">Background ID for cluster naming.</param>
    public static IReadOnlyList<ModeCluster> Cluster(
        double[] eigenvalues,
        double relativeTol,
        double absoluteTol,
        string backgroundId)
    {
        if (eigenvalues.Length == 0) return Array.Empty<ModeCluster>();

        var clusters = new List<ModeCluster>();
        var currentIndices = new List<int> { 0 };

        for (int i = 1; i < eigenvalues.Length; i++)
        {
            double gap = eigenvalues[i] - eigenvalues[i - 1];
            double scale = System.Math.Max(System.Math.Abs(eigenvalues[i - 1]), 1e-30);
            double relGap = gap / scale;

            bool newCluster = gap > absoluteTol && relGap > relativeTol;

            if (newCluster)
            {
                clusters.Add(BuildCluster(eigenvalues, currentIndices, clusters.Count, backgroundId));
                currentIndices = new List<int>();
            }
            currentIndices.Add(i);
        }

        // Final cluster
        clusters.Add(BuildCluster(eigenvalues, currentIndices, clusters.Count, backgroundId));

        // Compute spectral gaps between clusters
        for (int c = 0; c < clusters.Count - 1; c++)
        {
            double intraSpread = clusters[c].EigenvalueSpread;
            double interGap = clusters[c + 1].MeanEigenvalue - clusters[c].MeanEigenvalue;
            double gapRatio = intraSpread > 1e-30 ? interGap / intraSpread : interGap / 1e-30;

            // Rebuild with spectral gap (records are immutable)
            var prev = clusters[c];
            clusters[c] = new ModeCluster
            {
                ClusterId = prev.ClusterId,
                ModeIndices = prev.ModeIndices,
                MeanEigenvalue = prev.MeanEigenvalue,
                EigenvalueSpread = prev.EigenvalueSpread,
                SpectralGapToNext = gapRatio,
            };
        }

        return clusters;
    }

    private static ModeCluster BuildCluster(
        double[] eigenvalues, List<int> indices, int clusterIndex, string backgroundId)
    {
        double mean = 0;
        double min = double.MaxValue;
        double max = double.MinValue;

        foreach (int idx in indices)
        {
            mean += eigenvalues[idx];
            if (eigenvalues[idx] < min) min = eigenvalues[idx];
            if (eigenvalues[idx] > max) max = eigenvalues[idx];
        }
        mean /= indices.Count;

        return new ModeCluster
        {
            ClusterId = $"{backgroundId}-cluster-{clusterIndex}",
            ModeIndices = indices.ToArray(),
            MeanEigenvalue = mean,
            EigenvalueSpread = max - min,
        };
    }
}
