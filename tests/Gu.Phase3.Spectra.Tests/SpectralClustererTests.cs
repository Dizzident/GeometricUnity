namespace Gu.Phase3.Spectra.Tests;

public class SpectralClustererTests
{
    [Fact]
    public void Cluster_EmptyArray_ReturnsEmpty()
    {
        var clusterer = new SpectralClusterer();
        var result = clusterer.Cluster(Array.Empty<double>(), "bg-1");
        Assert.Empty(result);
    }

    [Fact]
    public void Cluster_SingleValue_ReturnsSingleCluster()
    {
        var clusterer = new SpectralClusterer();
        var result = clusterer.Cluster(new[] { 1.0 }, "bg-1");
        Assert.Single(result);
        Assert.Equal(1, result[0].Multiplicity);
        Assert.Equal(1.0, result[0].MeanEigenvalue);
    }

    [Fact]
    public void Cluster_WellSeparatedValues_ProducesSeparateClusters()
    {
        var clusterer = new SpectralClusterer(relativeTol: 0.01, absoluteTol: 0.1);
        var eigenvalues = new[] { 1.0, 1.001, 5.0, 5.001, 10.0 };
        var result = clusterer.Cluster(eigenvalues, "bg-sep");

        Assert.Equal(3, result.Count);
        Assert.Equal(2, result[0].Multiplicity); // 1.0, 1.001
        Assert.Equal(2, result[1].Multiplicity); // 5.0, 5.001
        Assert.Equal(1, result[2].Multiplicity); // 10.0
    }

    [Fact]
    public void Cluster_NearDegenerateValues_GroupsTogether()
    {
        var clusterer = new SpectralClusterer(relativeTol: 0.1, absoluteTol: 1e-3);
        var eigenvalues = new[] { 1.0, 1.0001, 1.0002 };
        var result = clusterer.Cluster(eigenvalues, "bg-degen");

        Assert.Single(result);
        Assert.Equal(3, result[0].Multiplicity);
    }

    [Fact]
    public void Cluster_SpectralGapToNext_IsSet()
    {
        var clusterer = new SpectralClusterer(relativeTol: 0.01, absoluteTol: 0.1);
        var eigenvalues = new[] { 1.0, 5.0 };
        var result = clusterer.Cluster(eigenvalues, "bg-gap");

        Assert.Equal(2, result.Count);
        Assert.NotNull(result[0].SpectralGapToNext);
        Assert.True(result[0].SpectralGapToNext > 0);
        Assert.Null(result[1].SpectralGapToNext); // last cluster
    }

    [Fact]
    public void Cluster_ClusterIdsContainBackgroundId()
    {
        var clusterer = new SpectralClusterer();
        var result = clusterer.Cluster(new[] { 1.0, 100.0 }, "my-bg");

        foreach (var cluster in result)
            Assert.Contains("my-bg", cluster.ClusterId);
    }

    [Fact]
    public void Constructor_NegativeRelTol_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new SpectralClusterer(relativeTol: -1.0));
    }

    [Fact]
    public void Constructor_NegativeAbsTol_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new SpectralClusterer(absoluteTol: -1.0));
    }

    [Fact]
    public void Cluster_NullEigenvalues_Throws()
    {
        var clusterer = new SpectralClusterer();
        Assert.Throws<ArgumentNullException>(() =>
            clusterer.Cluster(null!, "bg-1"));
    }

    [Fact]
    public void Cluster_ModeIndices_AreCorrect()
    {
        var clusterer = new SpectralClusterer(relativeTol: 0.01, absoluteTol: 0.5);
        var eigenvalues = new[] { 1.0, 1.1, 5.0, 5.1 };
        var result = clusterer.Cluster(eigenvalues, "bg-idx");

        Assert.Equal(2, result.Count);
        Assert.Contains(0, result[0].ModeIndices);
        Assert.Contains(1, result[0].ModeIndices);
        Assert.Contains(2, result[1].ModeIndices);
        Assert.Contains(3, result[1].ModeIndices);
    }
}
