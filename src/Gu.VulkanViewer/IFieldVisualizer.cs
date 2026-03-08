using Gu.Core;
using Gu.Geometry;

namespace Gu.VulkanViewer;

/// <summary>
/// Interface for preparing field data on a mesh for visualization.
/// Implementations map tensor field values to renderable geometry with colors.
/// </summary>
public interface IFieldVisualizer
{
    /// <summary>
    /// Prepares visualization data from a field tensor and its associated mesh.
    /// </summary>
    /// <param name="field">The field tensor whose values drive the color mapping.</param>
    /// <param name="mesh">The simplicial mesh providing vertex positions and topology.</param>
    /// <returns>A <see cref="VisualizationData"/> instance ready for rendering or export.</returns>
    VisualizationData PrepareVisualization(FieldTensor field, SimplicialMesh mesh);
}
