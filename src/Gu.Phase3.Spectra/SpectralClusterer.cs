namespace Gu.Phase3.Spectra;

/// <summary>
/// Public API for eigenvalue clustering.
/// Wraps <see cref="SpectralClustering"/> with configurable tolerances.
/// </summary>
public sealed class SpectralClusterer
{
    private readonly double _relativeTol;
    private readonly double _absoluteTol;

    /// <summary>
    /// Create a spectral clusterer with specified gap tolerances.
    /// </summary>
    /// <param name="relativeTol">Relative gap tolerance (default 0.1 = 10%).</param>
    /// <param name="absoluteTol">Absolute gap tolerance (default 1e-6).</param>
    public SpectralClusterer(double relativeTol = 0.1, double absoluteTol = 1e-6)
    {
        if (relativeTol < 0) throw new ArgumentOutOfRangeException(nameof(relativeTol));
        if (absoluteTol < 0) throw new ArgumentOutOfRangeException(nameof(absoluteTol));
        _relativeTol = relativeTol;
        _absoluteTol = absoluteTol;
    }

    /// <summary>
    /// Cluster eigenvalues into near-degenerate groups.
    /// </summary>
    /// <param name="eigenvalues">Sorted eigenvalues (ascending).</param>
    /// <param name="backgroundId">Background ID for cluster naming.</param>
    public IReadOnlyList<ModeCluster> Cluster(double[] eigenvalues, string backgroundId)
    {
        ArgumentNullException.ThrowIfNull(eigenvalues);
        ArgumentNullException.ThrowIfNull(backgroundId);

        return SpectralClustering.Cluster(eigenvalues, _relativeTol, _absoluteTol, backgroundId);
    }
}
