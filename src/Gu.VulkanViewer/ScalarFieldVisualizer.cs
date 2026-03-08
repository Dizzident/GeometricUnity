using Gu.Core;
using Gu.Geometry;

namespace Gu.VulkanViewer;

/// <summary>
/// Visualizes scalar fields on mesh vertices by mapping field values to
/// per-vertex RGBA colors on the mesh surface triangulation.
/// </summary>
/// <remarks>
/// The visualizer handles meshes of any embedding dimension by projecting
/// vertex coordinates to 3D: dimensions beyond the third are dropped,
/// and missing dimensions (e.g., 2D meshes) are padded with zero.
///
/// For scalar fields (degree "0"), coefficients are expected to be one-per-vertex.
/// For higher-degree forms (edge-based or face-based), per-simplex values are
/// averaged to vertices for visualization.
/// </remarks>
public sealed class ScalarFieldVisualizer : IFieldVisualizer
{
    private readonly ColorMapper _colorMapper;
    private readonly double? _fixedMin;
    private readonly double? _fixedMax;
    private readonly bool _centerAtZero;

    /// <summary>
    /// Initializes a new <see cref="ScalarFieldVisualizer"/>.
    /// </summary>
    /// <param name="colorMapper">The color mapper to use for value-to-color conversion.</param>
    /// <param name="fixedMin">Optional fixed minimum for the color range. If null, computed from data.</param>
    /// <param name="fixedMax">Optional fixed maximum for the color range. If null, computed from data.</param>
    /// <param name="centerAtZero">
    /// If true and fixedMin/fixedMax are not set, the range is centered at zero.
    /// Useful for diverging quantities like residuals.
    /// </param>
    public ScalarFieldVisualizer(
        ColorMapper? colorMapper = null,
        double? fixedMin = null,
        double? fixedMax = null,
        bool centerAtZero = false)
    {
        _colorMapper = colorMapper ?? new ColorMapper();
        _fixedMin = fixedMin;
        _fixedMax = fixedMax;
        _centerAtZero = centerAtZero;
    }

    /// <inheritdoc/>
    public VisualizationData PrepareVisualization(FieldTensor field, SimplicialMesh mesh)
    {
        ArgumentNullException.ThrowIfNull(field);
        ArgumentNullException.ThrowIfNull(mesh);

        // Step 1: Extract per-vertex scalar values from the field tensor.
        double[] vertexValues = ExtractVertexValues(field, mesh);

        // Step 2: Compute color range.
        double minVal, maxVal;
        if (_fixedMin.HasValue && _fixedMax.HasValue)
        {
            minVal = _fixedMin.Value;
            maxVal = _fixedMax.Value;
        }
        else
        {
            (minVal, maxVal) = ColorMapper.ComputeRange(vertexValues, _centerAtZero);
            if (_fixedMin.HasValue) minVal = _fixedMin.Value;
            if (_fixedMax.HasValue) maxVal = _fixedMax.Value;
        }

        // Step 3: Build vertex positions (always 3-component).
        float[] positions = BuildPositions(mesh);

        // Step 4: Map field values to RGBA colors.
        float[] colors = _colorMapper.MapArray(vertexValues, minVal, maxVal);

        // Step 5: Build triangle index buffer from mesh faces.
        uint[] indices = BuildTriangleIndices(mesh);

        return new VisualizationData
        {
            Positions = positions,
            Colors = colors,
            Indices = indices,
            VertexCount = mesh.VertexCount,
            ColorMap = new ColorMapMetadata
            {
                MinValue = minVal,
                MaxValue = maxVal,
                ColorSchemeName = _colorMapper.SchemeName,
            },
        };
    }

    /// <summary>
    /// Extracts per-vertex scalar values from a field tensor.
    /// For vertex-based fields (degree 0), coefficients are used directly.
    /// For edge-based fields (degree 1), values are averaged to adjacent vertices.
    /// For face-based fields (degree 2), values are averaged to face vertices.
    /// For multi-component fields, the L2 norm of components at each simplex is used.
    /// </summary>
    internal static double[] ExtractVertexValues(FieldTensor field, SimplicialMesh mesh)
    {
        int coeffCount = field.Coefficients.Length;

        // Determine if field is vertex-based, edge-based, or face-based.
        if (coeffCount == mesh.VertexCount)
        {
            // Direct vertex-based scalar field.
            return (double[])field.Coefficients.Clone();
        }

        if (coeffCount == mesh.EdgeCount || IsMultiComponentEdge(field, mesh))
        {
            // Edge-based field: average to vertices.
            return AverageEdgesToVertices(field, mesh);
        }

        if (coeffCount == mesh.FaceCount || IsMultiComponentFace(field, mesh))
        {
            // Face-based field: average to vertices.
            return AverageFacesToVertices(field, mesh);
        }

        // Multi-component vertex field: compute L2 norm per vertex.
        if (coeffCount > mesh.VertexCount && coeffCount % mesh.VertexCount == 0)
        {
            int components = coeffCount / mesh.VertexCount;
            return ComputeVertexNorms(field.Coefficients, mesh.VertexCount, components);
        }

        // Fallback: use as many values as we have vertices, padding with zero.
        double[] result = new double[mesh.VertexCount];
        int copyCount = Math.Min(coeffCount, mesh.VertexCount);
        Array.Copy(field.Coefficients, result, copyCount);
        return result;
    }

    private static bool IsMultiComponentEdge(FieldTensor field, SimplicialMesh mesh)
    {
        return mesh.EdgeCount > 0
            && field.Coefficients.Length > mesh.EdgeCount
            && field.Coefficients.Length % mesh.EdgeCount == 0;
    }

    private static bool IsMultiComponentFace(FieldTensor field, SimplicialMesh mesh)
    {
        return mesh.FaceCount > 0
            && field.Coefficients.Length > mesh.FaceCount
            && field.Coefficients.Length % mesh.FaceCount == 0;
    }

    private static double[] AverageEdgesToVertices(FieldTensor field, SimplicialMesh mesh)
    {
        double[] vertexValues = new double[mesh.VertexCount];
        int[] vertexCounts = new int[mesh.VertexCount];

        int componentsPerEdge = mesh.EdgeCount > 0
            ? field.Coefficients.Length / mesh.EdgeCount
            : 1;

        for (int e = 0; e < mesh.EdgeCount; e++)
        {
            // Compute the magnitude of this edge's field value.
            double magnitude = 0.0;
            for (int c = 0; c < componentsPerEdge; c++)
            {
                double val = field.Coefficients[e * componentsPerEdge + c];
                magnitude += val * val;
            }

            magnitude = Math.Sqrt(magnitude);

            int v0 = mesh.Edges[e][0];
            int v1 = mesh.Edges[e][1];

            vertexValues[v0] += magnitude;
            vertexValues[v1] += magnitude;
            vertexCounts[v0]++;
            vertexCounts[v1]++;
        }

        for (int v = 0; v < mesh.VertexCount; v++)
        {
            if (vertexCounts[v] > 0)
            {
                vertexValues[v] /= vertexCounts[v];
            }
        }

        return vertexValues;
    }

    private static double[] AverageFacesToVertices(FieldTensor field, SimplicialMesh mesh)
    {
        double[] vertexValues = new double[mesh.VertexCount];
        int[] vertexCounts = new int[mesh.VertexCount];

        int componentsPerFace = mesh.FaceCount > 0
            ? field.Coefficients.Length / mesh.FaceCount
            : 1;

        for (int f = 0; f < mesh.FaceCount; f++)
        {
            double magnitude = 0.0;
            for (int c = 0; c < componentsPerFace; c++)
            {
                double val = field.Coefficients[f * componentsPerFace + c];
                magnitude += val * val;
            }

            magnitude = Math.Sqrt(magnitude);

            for (int vi = 0; vi < mesh.Faces[f].Length; vi++)
            {
                int v = mesh.Faces[f][vi];
                vertexValues[v] += magnitude;
                vertexCounts[v]++;
            }
        }

        for (int v = 0; v < mesh.VertexCount; v++)
        {
            if (vertexCounts[v] > 0)
            {
                vertexValues[v] /= vertexCounts[v];
            }
        }

        return vertexValues;
    }

    private static double[] ComputeVertexNorms(double[] coefficients, int vertexCount, int components)
    {
        double[] norms = new double[vertexCount];
        for (int v = 0; v < vertexCount; v++)
        {
            double sumSq = 0.0;
            for (int c = 0; c < components; c++)
            {
                double val = coefficients[v * components + c];
                sumSq += val * val;
            }

            norms[v] = Math.Sqrt(sumSq);
        }

        return norms;
    }

    /// <summary>
    /// Builds a flat float[] of 3D vertex positions from the mesh.
    /// Coordinates beyond the third dimension are dropped;
    /// meshes with fewer than 3 dimensions are padded with 0.
    /// </summary>
    internal static float[] BuildPositions(SimplicialMesh mesh)
    {
        float[] positions = new float[mesh.VertexCount * 3];
        int dim = mesh.EmbeddingDimension;

        for (int v = 0; v < mesh.VertexCount; v++)
        {
            ReadOnlySpan<double> coords = mesh.GetVertexCoordinates(v);
            int outOffset = v * 3;

            positions[outOffset] = dim > 0 ? (float)coords[0] : 0f;
            positions[outOffset + 1] = dim > 1 ? (float)coords[1] : 0f;
            positions[outOffset + 2] = dim > 2 ? (float)coords[2] : 0f;
        }

        return positions;
    }

    /// <summary>
    /// Builds triangle indices from the mesh faces.
    /// Only 2-simplicial faces (triangles) are included directly.
    /// For higher-dimensional cells, the face array is used.
    /// </summary>
    internal static uint[] BuildTriangleIndices(SimplicialMesh mesh)
    {
        // Each face in the mesh is a triangle (3 vertices).
        uint[] indices = new uint[mesh.FaceCount * 3];

        for (int f = 0; f < mesh.FaceCount; f++)
        {
            int[] face = mesh.Faces[f];
            int offset = f * 3;
            indices[offset] = (uint)face[0];
            indices[offset + 1] = (uint)face[1];
            indices[offset + 2] = (uint)face[2];
        }

        return indices;
    }
}
