using System.Globalization;
using System.Text;

namespace Gu.VulkanViewer;

/// <summary>
/// Exports visualization mesh data to interchange formats for use in external
/// tools such as ParaView, MeshLab, or Blender.
/// </summary>
public static class MeshExporter
{
    /// <summary>
    /// Exports mesh geometry to Wavefront OBJ format.
    /// Vertex positions are written as "v x y z" lines.
    /// Triangle faces are written as "f v1 v2 v3" lines (1-indexed).
    /// </summary>
    /// <param name="data">The visualization data to export.</param>
    /// <returns>OBJ file content as a string.</returns>
    public static string ToObj(VisualizationData data)
    {
        ArgumentNullException.ThrowIfNull(data);

        var sb = new StringBuilder();
        sb.AppendLine("# Geometric Unity - Vulkan Workbench OBJ Export");
        sb.AppendLine($"# Vertices: {data.VertexCount}");
        sb.AppendLine($"# Triangles: {data.TriangleCount}");
        sb.AppendLine($"# Color scheme: {data.ColorMap.ColorSchemeName}");
        sb.AppendLine($"# Field range: [{data.ColorMap.MinValue:G6}, {data.ColorMap.MaxValue:G6}]");
        sb.AppendLine();

        // Write vertex positions.
        for (int v = 0; v < data.VertexCount; v++)
        {
            int offset = v * 3;
            sb.Append("v ");
            sb.Append(data.Positions[offset].ToString("G9", CultureInfo.InvariantCulture));
            sb.Append(' ');
            sb.Append(data.Positions[offset + 1].ToString("G9", CultureInfo.InvariantCulture));
            sb.Append(' ');
            sb.AppendLine(data.Positions[offset + 2].ToString("G9", CultureInfo.InvariantCulture));
        }

        sb.AppendLine();

        // Write triangle faces (OBJ uses 1-based indexing).
        for (int t = 0; t < data.TriangleCount; t++)
        {
            int offset = t * 3;
            sb.Append("f ");
            sb.Append(data.Indices[offset] + 1);
            sb.Append(' ');
            sb.Append(data.Indices[offset + 1] + 1);
            sb.Append(' ');
            sb.AppendLine((data.Indices[offset + 2] + 1).ToString());
        }

        return sb.ToString();
    }

    /// <summary>
    /// Exports per-vertex colors to CSV format.
    /// Each row contains: vertex_index, R, G, B, A (values in [0, 1]).
    /// </summary>
    /// <param name="data">The visualization data to export.</param>
    /// <returns>CSV string with header row and one row per vertex.</returns>
    public static string ColorsToCs(VisualizationData data)
    {
        ArgumentNullException.ThrowIfNull(data);

        var sb = new StringBuilder();
        sb.AppendLine("vertex_index,R,G,B,A");

        for (int v = 0; v < data.VertexCount; v++)
        {
            int offset = v * 4;
            sb.Append(v);
            sb.Append(',');
            sb.Append(data.Colors[offset].ToString("F6", CultureInfo.InvariantCulture));
            sb.Append(',');
            sb.Append(data.Colors[offset + 1].ToString("F6", CultureInfo.InvariantCulture));
            sb.Append(',');
            sb.Append(data.Colors[offset + 2].ToString("F6", CultureInfo.InvariantCulture));
            sb.Append(',');
            sb.AppendLine(data.Colors[offset + 3].ToString("F6", CultureInfo.InvariantCulture));
        }

        return sb.ToString();
    }

    /// <summary>
    /// Writes the OBJ and color CSV files to disk.
    /// </summary>
    /// <param name="data">The visualization data to export.</param>
    /// <param name="objPath">File path for the OBJ file.</param>
    /// <param name="colorCsvPath">File path for the vertex colors CSV file.</param>
    public static void WriteFiles(VisualizationData data, string objPath, string colorCsvPath)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentException.ThrowIfNullOrWhiteSpace(objPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(colorCsvPath);

        File.WriteAllText(objPath, ToObj(data), Encoding.UTF8);
        File.WriteAllText(colorCsvPath, ColorsToCs(data), Encoding.UTF8);
    }

    /// <summary>
    /// Exports to PLY format with embedded vertex colors.
    /// PLY supports per-vertex color natively, making it a single-file export
    /// that tools like ParaView, MeshLab, and CloudCompare can load directly.
    /// </summary>
    /// <param name="data">The visualization data to export.</param>
    /// <returns>PLY file content as a string (ASCII format).</returns>
    public static string ToPly(VisualizationData data)
    {
        ArgumentNullException.ThrowIfNull(data);

        var sb = new StringBuilder();

        // PLY header.
        sb.AppendLine("ply");
        sb.AppendLine("format ascii 1.0");
        sb.AppendLine($"comment Geometric Unity - Vulkan Workbench Export");
        sb.AppendLine($"comment Color scheme: {data.ColorMap.ColorSchemeName}");
        sb.AppendLine($"comment Field range: [{data.ColorMap.MinValue:G6}, {data.ColorMap.MaxValue:G6}]");
        sb.AppendLine($"element vertex {data.VertexCount}");
        sb.AppendLine("property float x");
        sb.AppendLine("property float y");
        sb.AppendLine("property float z");
        sb.AppendLine("property uchar red");
        sb.AppendLine("property uchar green");
        sb.AppendLine("property uchar blue");
        sb.AppendLine("property uchar alpha");
        sb.AppendLine($"element face {data.TriangleCount}");
        sb.AppendLine("property list uchar int vertex_indices");
        sb.AppendLine("end_header");

        // Vertex data with colors.
        for (int v = 0; v < data.VertexCount; v++)
        {
            int posOffset = v * 3;
            int colorOffset = v * 4;

            sb.Append(data.Positions[posOffset].ToString("G9", CultureInfo.InvariantCulture));
            sb.Append(' ');
            sb.Append(data.Positions[posOffset + 1].ToString("G9", CultureInfo.InvariantCulture));
            sb.Append(' ');
            sb.Append(data.Positions[posOffset + 2].ToString("G9", CultureInfo.InvariantCulture));
            sb.Append(' ');
            sb.Append((int)(data.Colors[colorOffset] * 255));
            sb.Append(' ');
            sb.Append((int)(data.Colors[colorOffset + 1] * 255));
            sb.Append(' ');
            sb.Append((int)(data.Colors[colorOffset + 2] * 255));
            sb.Append(' ');
            sb.AppendLine(((int)(data.Colors[colorOffset + 3] * 255)).ToString());
        }

        // Face data.
        for (int t = 0; t < data.TriangleCount; t++)
        {
            int offset = t * 3;
            sb.Append("3 ");
            sb.Append(data.Indices[offset]);
            sb.Append(' ');
            sb.Append(data.Indices[offset + 1]);
            sb.Append(' ');
            sb.AppendLine(data.Indices[offset + 2].ToString());
        }

        return sb.ToString();
    }
}
