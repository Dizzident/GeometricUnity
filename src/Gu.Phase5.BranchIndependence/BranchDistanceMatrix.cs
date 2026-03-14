using System.Text.Json.Serialization;

namespace Gu.Phase5.BranchIndependence;

/// <summary>
/// Pairwise distance matrix over branch variants for one target quantity (M46).
///
/// Entry [i,j] stores |Q(branch_i) - Q(branch_j)| normalized by a reference scale.
/// The matrix is symmetric and stored row-major as a flat array.
/// </summary>
public sealed class BranchDistanceMatrix
{
    /// <summary>Schema version.</summary>
    [JsonPropertyName("schemaVersion")]
    public string SchemaVersion { get; init; } = "1.0.0";

    /// <summary>Target quantity ID this matrix measures.</summary>
    [JsonPropertyName("targetQuantityId")]
    public required string TargetQuantityId { get; init; }

    /// <summary>Ordered list of branch variant IDs (row/column labels).</summary>
    [JsonPropertyName("branchVariantIds")]
    public required List<string> BranchVariantIds { get; init; }

    /// <summary>
    /// Raw quantity values per branch variant, in the same order as BranchVariantIds.
    /// </summary>
    [JsonPropertyName("quantityValues")]
    public required double[] QuantityValues { get; init; }

    /// <summary>
    /// Flat row-major distance matrix of size N×N (N = BranchVariantIds.Count).
    /// Entry [i*N + j] = |Q(branch_i) - Q(branch_j)|.
    /// </summary>
    [JsonPropertyName("distances")]
    public required double[] Distances { get; init; }

    /// <summary>
    /// Maximum pairwise distance (the spread of the quantity across the branch family).
    /// </summary>
    [JsonPropertyName("maxDistance")]
    public double MaxDistance { get; init; }

    /// <summary>
    /// Mean pairwise distance (off-diagonal average).
    /// </summary>
    [JsonPropertyName("meanDistance")]
    public double MeanDistance { get; init; }

    /// <summary>
    /// Reference scale used for normalization.
    /// Set to |Q(reference branch)| + 1e-30 to avoid division by zero.
    /// </summary>
    [JsonPropertyName("referenceScale")]
    public double ReferenceScale { get; init; }

    /// <summary>
    /// Number of branch variants (dimension of the matrix).
    /// </summary>
    [JsonIgnore]
    public int Dimension => BranchVariantIds.Count;

    /// <summary>
    /// Get the distance between two branch variants by index.
    /// </summary>
    public double GetDistance(int i, int j) => Distances[i * Dimension + j];

    /// <summary>
    /// Build a BranchDistanceMatrix from quantity values and variant IDs.
    /// </summary>
    public static BranchDistanceMatrix Build(
        string targetQuantityId,
        List<string> branchVariantIds,
        double[] quantityValues)
    {
        int n = branchVariantIds.Count;
        var distances = new double[n * n];

        for (int i = 0; i < n; i++)
        for (int j = 0; j < n; j++)
            distances[i * n + j] = System.Math.Abs(quantityValues[i] - quantityValues[j]);

        double maxDist = 0.0;
        double sumDist = 0.0;
        int offDiagCount = 0;
        for (int i = 0; i < n; i++)
        for (int j = 0; j < n; j++)
        {
            if (i == j) continue;
            var d = distances[i * n + j];
            if (d > maxDist) maxDist = d;
            sumDist += d;
            offDiagCount++;
        }
        double meanDist = offDiagCount > 0 ? sumDist / offDiagCount : 0.0;
        double refScale = (quantityValues.Length > 0 ? System.Math.Abs(quantityValues[0]) : 0.0) + 1e-30;

        return new BranchDistanceMatrix
        {
            TargetQuantityId = targetQuantityId,
            BranchVariantIds = branchVariantIds,
            QuantityValues = quantityValues,
            Distances = distances,
            MaxDistance = maxDist,
            MeanDistance = meanDist,
            ReferenceScale = refScale,
        };
    }
}
