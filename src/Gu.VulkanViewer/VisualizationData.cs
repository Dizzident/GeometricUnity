namespace Gu.VulkanViewer;

/// <summary>
/// Metadata describing the color mapping applied to a visualization.
/// </summary>
public sealed class ColorMapMetadata
{
    /// <summary>Minimum scalar value in the field data.</summary>
    public required double MinValue { get; init; }

    /// <summary>Maximum scalar value in the field data.</summary>
    public required double MaxValue { get; init; }

    /// <summary>Name of the color scheme used (e.g., "viridis", "plasma").</summary>
    public required string ColorSchemeName { get; init; }
}

/// <summary>
/// Prepared visualization data ready for rendering or export.
/// Contains vertex positions, per-vertex RGBA colors mapped from field values,
/// and triangle indices for the mesh surface.
/// </summary>
public sealed class VisualizationData
{
    /// <summary>
    /// Vertex positions as a flat array: [x0, y0, z0, x1, y1, z1, ...].
    /// Always 3-component (z = 0 for 2D meshes). Length = VertexCount * 3.
    /// </summary>
    public required float[] Positions { get; init; }

    /// <summary>
    /// Per-vertex RGBA colors as a flat array: [r0, g0, b0, a0, r1, g1, b1, a1, ...].
    /// Each component in [0, 1]. Length = VertexCount * 4.
    /// </summary>
    public required float[] Colors { get; init; }

    /// <summary>
    /// Triangle index buffer: every 3 consecutive entries define one triangle.
    /// Length = TriangleCount * 3.
    /// </summary>
    public required uint[] Indices { get; init; }

    /// <summary>Number of vertices.</summary>
    public required int VertexCount { get; init; }

    /// <summary>Number of triangles.</summary>
    public int TriangleCount => Indices.Length / 3;

    /// <summary>Color map metadata describing the mapping that produced the colors.</summary>
    public required ColorMapMetadata ColorMap { get; init; }
}
