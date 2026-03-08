using System.Text;
using Gu.Math;

namespace Gu.ReferenceCpu;

/// <summary>
/// Human-readable debug dumps for tiny test cases (Section 14.3).
/// Only intended for small meshes where full enumeration is feasible.
/// </summary>
public static class DebugPrinter
{
    /// <summary>
    /// Prints a connection field in human-readable form.
    /// </summary>
    public static string PrintConnection(ConnectionField omega)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Connection omega_h on {omega.EdgeCount} edges, dim(g)={omega.AlgebraDimension}");
        sb.AppendLine($"  Algebra: {omega.Algebra.AlgebraId} ({omega.Algebra.Label})");
        sb.AppendLine($"  Mesh: {omega.Mesh.VertexCount} vertices, {omega.Mesh.CellCount} cells");
        sb.AppendLine();

        for (int e = 0; e < omega.EdgeCount; e++)
        {
            var edge = omega.Mesh.Edges[e];
            var val = omega.GetEdgeValueArray(e);
            sb.Append($"  edge[{e}] ({edge[0]}->{edge[1]}): [");
            sb.Append(string.Join(", ", val.Select(v => v.ToString("F6"))));
            sb.AppendLine("]");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Prints a curvature field in human-readable form.
    /// </summary>
    public static string PrintCurvature(CurvatureField curvature)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Curvature F_h on {curvature.FaceCount} faces, dim(g)={curvature.AlgebraDimension}");
        sb.AppendLine($"  ||F||^2 = {curvature.NormSquared():E6}");
        sb.AppendLine();

        for (int f = 0; f < curvature.FaceCount; f++)
        {
            var face = curvature.Mesh.Faces[f];
            var val = curvature.GetFaceValueArray(f);
            sb.Append($"  face[{f}] ({string.Join(",", face)}): [");
            sb.Append(string.Join(", ", val.Select(v => v.ToString("F6"))));
            sb.AppendLine("]");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Prints a summary comparing two curvature fields (e.g., for parity checks).
    /// </summary>
    public static string PrintCurvatureComparison(
        CurvatureField expected, CurvatureField actual, string label = "Parity Check")
    {
        var sb = new StringBuilder();
        sb.AppendLine($"=== {label} ===");

        if (expected.FaceCount != actual.FaceCount)
        {
            sb.AppendLine($"  MISMATCH: face counts differ ({expected.FaceCount} vs {actual.FaceCount})");
            return sb.ToString();
        }

        double maxDiff = 0;
        int maxDiffFace = -1;
        int maxDiffComponent = -1;

        for (int f = 0; f < expected.FaceCount; f++)
        {
            var expVal = expected.GetFaceValueArray(f);
            var actVal = actual.GetFaceValueArray(f);

            for (int a = 0; a < expected.AlgebraDimension; a++)
            {
                double diff = System.Math.Abs(expVal[a] - actVal[a]);
                if (diff > maxDiff)
                {
                    maxDiff = diff;
                    maxDiffFace = f;
                    maxDiffComponent = a;
                }
            }
        }

        sb.AppendLine($"  Faces: {expected.FaceCount}");
        sb.AppendLine($"  Max absolute difference: {maxDiff:E6}");
        if (maxDiffFace >= 0)
            sb.AppendLine($"  At face[{maxDiffFace}], component[{maxDiffComponent}]");
        sb.AppendLine($"  ||F_expected||^2 = {expected.NormSquared():E6}");
        sb.AppendLine($"  ||F_actual||^2 = {actual.NormSquared():E6}");

        return sb.ToString();
    }
}
