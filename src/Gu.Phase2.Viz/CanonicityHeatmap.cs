using Gu.Phase2.Canonicity;

namespace Gu.Phase2.Viz;

/// <summary>
/// Renders a <see cref="PairwiseDistanceMatrix"/> as a colored NxN grid
/// for the Vulkan HUD. Produces a <see cref="CanonicityHeatmapPayload"/>.
/// </summary>
public sealed class CanonicityHeatmap : IVulkanDiagnosticPanel
{
    public string PanelId => "canonicity-heatmap";
    public string Title => "Canonicity Distance Heatmap";

    /// <summary>
    /// Prepare a heatmap payload from a PairwiseDistanceMatrix.
    /// Flattens the 2D distance array into row-major format for rendering.
    /// </summary>
    public CanonicityHeatmapPayload Prepare(PairwiseDistanceMatrix matrix)
    {
        ArgumentNullException.ThrowIfNull(matrix);

        int n = matrix.BranchIds.Count;
        double[] flat = new double[n * n];

        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
                flat[i * n + j] = matrix.Distances[i, j];

        return new CanonicityHeatmapPayload
        {
            MetricId = matrix.MetricId,
            BranchIds = matrix.BranchIds,
            FlatDistances = flat,
            Dimension = n,
            MaxDistance = matrix.MaxDistance,
        };
    }
}
