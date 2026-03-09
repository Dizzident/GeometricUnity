using Gu.Core;
using Gu.Geometry;
using Gu.Phase2.Execution;
using Gu.VulkanViewer;

namespace Gu.Phase2.Viz;

/// <summary>
/// Renders two branches' observed outputs side-by-side with computed D_obs
/// highlighted on mesh faces. Produces a <see cref="BranchOverlayPayload"/>
/// for the Vulkan HUD renderer.
/// </summary>
public sealed class BranchOverlayView : IVulkanDiagnosticPanel
{
    private readonly IFieldVisualizer _visualizer;

    public string PanelId => "branch-overlay";
    public string Title => "Branch Overlay (D_obs)";

    public BranchOverlayView(IFieldVisualizer? visualizer = null)
    {
        _visualizer = visualizer ?? new ScalarFieldVisualizer(centerAtZero: true);
    }

    /// <summary>
    /// Prepare an overlay payload comparing two branch run records on a mesh.
    /// </summary>
    public BranchOverlayPayload Prepare(
        BranchRunRecord left,
        BranchRunRecord right,
        SimplicialMesh mesh)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);
        ArgumentNullException.ThrowIfNull(mesh);

        // Compute per-face differences from observed state
        double[] faceDiffs = ComputeFaceDifferences(left, right, mesh.FaceCount);
        double maxDist = 0.0;
        foreach (double d in faceDiffs)
            if (!double.IsNaN(d) && d > maxDist) maxDist = d;

        // Create difference field tensor for visualization
        var diffField = new FieldTensor
        {
            Label = $"D_obs({left.Variant.Id},{right.Variant.Id})",
            Signature = new TensorSignature
            {
                AmbientSpaceId = "Y_h",
                CarrierType = "difference-2form",
                Degree = "2",
                LieAlgebraBasisId = "canonical",
                ComponentOrderId = "face-major",
                MemoryLayout = "dense-row-major",
            },
            Coefficients = faceDiffs,
            Shape = new[] { faceDiffs.Length },
        };

        var leftViz = _visualizer.PrepareVisualization(diffField, mesh);
        var rightViz = _visualizer.PrepareVisualization(diffField, mesh);

        return new BranchOverlayPayload
        {
            LeftBranchId = left.Variant.Id,
            RightBranchId = right.Variant.Id,
            LeftMeshData = leftViz,
            RightMeshData = rightViz,
            FaceDifferences = faceDiffs,
            MaxDistance = maxDist,
        };
    }

    private static double[] ComputeFaceDifferences(
        BranchRunRecord left, BranchRunRecord right, int faceCount)
    {
        var diffs = new double[faceCount];

        if (left.ObservedState == null || right.ObservedState == null)
        {
            Array.Fill(diffs, double.NaN);
            return diffs;
        }

        // Use the first shared observable to compute per-component differences
        foreach (var (obsId, leftSnap) in left.ObservedState.Observables)
        {
            if (!right.ObservedState.Observables.TryGetValue(obsId, out var rightSnap))
                continue;

            int len = System.Math.Min(leftSnap.Values.Length,
                System.Math.Min(rightSnap.Values.Length, faceCount));
            for (int i = 0; i < len; i++)
            {
                diffs[i] = System.Math.Abs(leftSnap.Values[i] - rightSnap.Values[i]);
            }
            break; // Use first shared observable
        }

        return diffs;
    }
}
