using Gu.Core;

namespace Gu.Geometry;

/// <summary>
/// Represents the fiber bundle structure Y_h -> X_h with explicit
/// projection pi_h (many-to-one) and observation section sigma_h (X -> Y).
/// </summary>
public sealed class FiberBundleMesh
{
    /// <summary>The base space mesh X_h.</summary>
    public required SimplicialMesh BaseMesh { get; init; }

    /// <summary>The ambient/total space mesh Y_h where the connection lives.</summary>
    public required SimplicialMesh AmbientMesh { get; init; }

    /// <summary>
    /// Projection map pi_h: Y_h -> X_h at vertex level.
    /// YVertexToXVertex[yVertIdx] = xVertIdx.
    /// This is many-to-one: multiple Y vertices map to the same X vertex.
    /// </summary>
    public required int[] YVertexToXVertex { get; init; }

    /// <summary>
    /// Fiber decomposition: for each X vertex, which Y vertices project to it.
    /// FiberVerticesPerXVertex[xVertIdx] = array of Y vertex indices in the fiber over x.
    /// </summary>
    public required int[][] FiberVerticesPerXVertex { get; init; }

    /// <summary>
    /// Observation section sigma_h: X_h -> Y_h at vertex level.
    /// XVertexToYVertex[xVertIdx] = yVertIdx.
    /// Selects one Y vertex per X vertex (section of the bundle).
    /// Must satisfy: YVertexToXVertex[XVertexToYVertex[x]] == x for all x.
    /// </summary>
    public required int[] XVertexToYVertex { get; init; }

    /// <summary>
    /// Observation section at cell level: which Y cell contains sigma(x) for each X cell.
    /// XCellToYCell[xCellIdx] = yCellIdx.
    /// </summary>
    public required int[] XCellToYCell { get; init; }

    /// <summary>
    /// Barycentric coordinates for sigma_h interpolation within the target Y cell.
    /// SectionCoefficients[xCellIdx] = barycentric coordinates within Y cell.
    /// For vertex-aligned sections, these are unit vectors (1 at the selected vertex, 0 elsewhere).
    /// </summary>
    public required double[][] SectionCoefficients { get; init; }

    /// <summary>
    /// Validates that the section property holds: pi(sigma(x)) == x for all X vertices.
    /// </summary>
    public bool ValidateSection()
    {
        for (int x = 0; x < BaseMesh.VertexCount; x++)
        {
            int yVertex = XVertexToYVertex[x];
            if (yVertex < 0 || yVertex >= AmbientMesh.VertexCount)
                return false;
            if (YVertexToXVertex[yVertex] != x)
                return false;
        }
        return true;
    }

    /// <summary>
    /// Validates that the fiber decomposition is consistent with the projection.
    /// </summary>
    public bool ValidateFibers()
    {
        for (int x = 0; x < BaseMesh.VertexCount; x++)
        {
            foreach (int y in FiberVerticesPerXVertex[x])
            {
                if (YVertexToXVertex[y] != x)
                    return false;
            }
        }

        // Check all Y vertices appear in exactly one fiber
        var covered = new bool[AmbientMesh.VertexCount];
        for (int x = 0; x < BaseMesh.VertexCount; x++)
        {
            foreach (int y in FiberVerticesPerXVertex[x])
            {
                if (covered[y])
                    return false; // duplicate
                covered[y] = true;
            }
        }

        return covered.All(c => c);
    }

    /// <summary>
    /// Builds a GeometryContext (Gu.Core metadata type) from this fiber bundle mesh.
    /// </summary>
    public GeometryContext ToGeometryContext(string quadratureRuleId, string basisFamilyId)
    {
        var baseSpace = new SpaceRef
        {
            SpaceId = "X_h",
            Dimension = BaseMesh.EmbeddingDimension,
            EdgeCount = BaseMesh.EdgeCount,
            FaceCount = BaseMesh.FaceCount,
            Label = "base_X_h",
        };

        var ambientSpace = new SpaceRef
        {
            SpaceId = "Y_h",
            Dimension = AmbientMesh.EmbeddingDimension,
            EdgeCount = AmbientMesh.EdgeCount,
            FaceCount = AmbientMesh.FaceCount,
            Label = "ambient_Y_h",
        };

        var projectionBinding = new GeometryBinding
        {
            BindingType = "projection",
            SourceSpace = ambientSpace,
            TargetSpace = baseSpace,
            MappingStrategy = "many-to-one-fiber-projection",
        };

        var observationBinding = new GeometryBinding
        {
            BindingType = "observation",
            SourceSpace = baseSpace,
            TargetSpace = ambientSpace,
            MappingStrategy = "section-vertex-aligned",
        };

        var patches = new List<PatchInfo>
        {
            new PatchInfo
            {
                PatchId = "Y_h_patch_0",
                ElementCount = AmbientMesh.CellCount,
                TopologyType = "simplicial",
                Metadata = new Dictionary<string, string>
                {
                    ["simplicialDimension"] = AmbientMesh.SimplicialDimension.ToString(),
                    ["embeddingDimension"] = AmbientMesh.EmbeddingDimension.ToString(),
                    ["vertexCount"] = AmbientMesh.VertexCount.ToString(),
                    ["edgeCount"] = AmbientMesh.EdgeCount.ToString(),
                    ["faceCount"] = AmbientMesh.FaceCount.ToString(),
                },
            },
        };

        return new GeometryContext
        {
            BaseSpace = baseSpace,
            AmbientSpace = ambientSpace,
            DiscretizationType = "simplicial",
            QuadratureRuleId = quadratureRuleId,
            BasisFamilyId = basisFamilyId,
            ProjectionBinding = projectionBinding,
            ObservationBinding = observationBinding,
            Patches = patches,
        };
    }
}
