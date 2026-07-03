using Gu.Geometry;

namespace Gu.Geometry.Tests;

/// <summary>
/// Acceptance battery for the 4D platform M1 milestone: the Coxeter–Freudenthal–Kuhn
/// tesseract triangulation, the 3-subsimplex (volume) topology layer, the discrete
/// 2-form → 3-form exterior derivative, and the 4D toy/structured factories.
/// </summary>
public class Mesh4DTests
{
    // ---- CreateUniform4D golden counts -------------------------------------

    [Fact]
    public void CreateUniform4D_1_HasGoldenSubsimplexCounts()
    {
        var mesh = SimplicialMeshGenerator.CreateUniform4D(1);

        Assert.Equal(4, mesh.EmbeddingDimension);
        Assert.Equal(4, mesh.SimplicialDimension);
        Assert.Equal(16, mesh.VertexCount);
        Assert.Equal(65, mesh.EdgeCount);
        Assert.Equal(110, mesh.FaceCount);
        Assert.Equal(84, mesh.VolumeCount);
        Assert.Equal(24, mesh.CellCount);
    }

    [Fact]
    public void CreateUniform4D_1_EulerCharacteristicIsOne()
    {
        var mesh = SimplicialMeshGenerator.CreateUniform4D(1);

        int euler = mesh.VertexCount - mesh.EdgeCount + mesh.FaceCount
                    - mesh.VolumeCount + mesh.CellCount;
        Assert.Equal(1, euler);
    }

    [Fact]
    public void CreateUniform4D_2_GoldenCountsAndInvariants()
    {
        var mesh = SimplicialMeshGenerator.CreateUniform4D(2);

        // Exact, derivable:
        Assert.Equal(81, mesh.VertexCount);   // 3^4
        Assert.Equal(384, mesh.CellCount);    // 16 * 24

        // Euler invariant for the contractible solid block: V - E + F - Vol + C = 1
        // => E - F + Vol = 464.
        int euler = mesh.VertexCount - mesh.EdgeCount + mesh.FaceCount
                    - mesh.VolumeCount + mesh.CellCount;
        Assert.Equal(1, euler);
        Assert.Equal(464, mesh.EdgeCount - mesh.FaceCount + mesh.VolumeCount);

        // Golden E/F/Vol pinned from the first green implementation run.
        Assert.Equal(544, mesh.EdgeCount);
        Assert.Equal(1232, mesh.FaceCount);
        Assert.Equal(1152, mesh.VolumeCount);
    }

    [Fact]
    public void CreateUniform4D_2_VolumeCellSharingIsBoundaryConsistent()
    {
        var mesh = SimplicialMeshGenerator.CreateUniform4D(2);

        var multiplicity = new int[mesh.VolumeCount];
        foreach (var cellVols in mesh.CellVolumes)
            foreach (int v in cellVols)
                multiplicity[v]++;

        int boundary = 0;
        int interior = 0;
        foreach (int m in multiplicity)
        {
            Assert.True(m == 1 || m == 2,
                $"Volume shared by {m} cells; every 3-subsimplex must be shared by 1 (boundary) or 2 (interior) pentachora.");
            if (m == 1) boundary++;
            else interior++;
        }

        // Pinned from the first green run: 384 boundary volumes, 768 interior.
        Assert.Equal(384, boundary);
        Assert.Equal(768, interior);
    }

    [Fact]
    public void CreateUniform4D_InvalidArg_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => SimplicialMeshGenerator.CreateUniform4D(0));
    }

    // ---- Periodic 4-torus variant (M1b) ------------------------------------

    // Derived counts: the Kuhn triangulation is Z^4-translation-invariant, so on the
    // torus (Z_n)^4 each subsimplex type has exactly n^4 translates. Per lattice point
    // the orbit counts (chains ∅ ⊊ B1 ⊊ ... ⊊ Bk of subsets of {1,2,3,4}) are
    // V:1, E:15, F:50, Vol:60, C:24; their alternating sum 1-15+50-60+24 = 0 = χ(T^4).

    [Fact]
    public void CreateUniform4DPeriodic_3_HasDerivedTorusCounts()
    {
        var mesh = SimplicialMeshGenerator.CreateUniform4DPeriodic(3);
        int n4 = 81; // 3^4

        Assert.Equal(4, mesh.EmbeddingDimension);
        Assert.Equal(4, mesh.SimplicialDimension);
        Assert.Equal(1 * n4, mesh.VertexCount);   // 81
        Assert.Equal(15 * n4, mesh.EdgeCount);     // 1215
        Assert.Equal(50 * n4, mesh.FaceCount);     // 4050
        Assert.Equal(60 * n4, mesh.VolumeCount);   // 4860
        Assert.Equal(24 * n4, mesh.CellCount);     // 1944

        // Euler characteristic of the 4-torus is 0.
        int euler = mesh.VertexCount - mesh.EdgeCount + mesh.FaceCount
                    - mesh.VolumeCount + mesh.CellCount;
        Assert.Equal(0, euler);
    }

    [Fact]
    public void CreateUniform4DPeriodic_4_HasDerivedTorusCounts()
    {
        var mesh = SimplicialMeshGenerator.CreateUniform4DPeriodic(4);
        int n4 = 256; // 4^4

        Assert.Equal(1 * n4, mesh.VertexCount);   // 256
        Assert.Equal(15 * n4, mesh.EdgeCount);     // 3840
        Assert.Equal(50 * n4, mesh.FaceCount);     // 12800
        Assert.Equal(60 * n4, mesh.VolumeCount);   // 15360
        Assert.Equal(24 * n4, mesh.CellCount);     // 6144

        int euler = mesh.VertexCount - mesh.EdgeCount + mesh.FaceCount
                    - mesh.VolumeCount + mesh.CellCount;
        Assert.Equal(0, euler);
    }

    [Fact]
    public void CreateUniform4DPeriodic_HasNoBoundary_EveryVolumeSharedByExactlyTwoCells()
    {
        var mesh = SimplicialMeshGenerator.CreateUniform4DPeriodic(3);

        // In 4D the codimension-1 facets are the volumes (3-subsimplices). A closed
        // manifold (no boundary) requires every volume to be shared by exactly 2 cells.
        var volMult = new int[mesh.VolumeCount];
        foreach (var cellVols in mesh.CellVolumes)
            foreach (int v in cellVols)
                volMult[v]++;
        foreach (int m in volMult)
            Assert.Equal(2, m);

        // Faces are codimension-2; their star is a closed cycle of cells, so their
        // multiplicity is a link size (>= 2), never a boundary value of 1. Pinned to
        // the stable {4,6} pattern of the Freudenthal torus.
        var faceMult = new int[mesh.FaceCount];
        foreach (var cf in mesh.CellFaces)
            foreach (int f in cf)
                faceMult[f]++;
        foreach (int m in faceMult)
            Assert.True(m == 4 || m == 6, $"Face shared by {m} cells; expected 4 or 6 on the Freudenthal 4-torus.");
    }

    [Fact]
    public void CreateUniform4DPeriodic_BoundaryOfBoundary_IsExactlyZero()
    {
        var mesh = SimplicialMeshGenerator.CreateUniform4DPeriodic(3);

        int[,] b1 = BuildVertexEdgeBoundary(mesh);
        int[,] b2 = BuildEdgeFaceBoundary(mesh);
        int[,] b3 = BuildFaceVolumeBoundary(mesh);

        AssertAllZero(MultiplyIntMatrices(b1, b2), "periodic B1 * B2");
        AssertAllZero(MultiplyIntMatrices(b2, b3), "periodic B2 * B3");
    }

    [Fact]
    public void CreateUniform4DPeriodic_DiscreteDerivative_DDIsZero()
    {
        var mesh = SimplicialMeshGenerator.CreateUniform4DPeriodic(3);
        const int dimG = 2;

        var rng = new Random(20260702);
        var edgeCoeffs = new double[mesh.EdgeCount * dimG];
        for (int i = 0; i < edgeCoeffs.Length; i++)
            edgeCoeffs[i] = rng.NextDouble() * 2.0 - 1.0;

        double[] faceCoeffs = DiscreteExteriorDerivative.EdgeToFace(mesh, edgeCoeffs, dimG);
        ThreeFormField ddOmega = DiscreteExteriorDerivative.FaceToVolume(mesh, faceCoeffs, dimG);

        foreach (double c in ddOmega.Coefficients)
            Assert.True(System.Math.Abs(c) < 1e-9, $"periodic d(d omega) component {c} is not zero.");
    }

    [Fact]
    public void CreateUniform4DPeriodic_BelowMinimum_Throws()
    {
        // n < 3 collapses distinct subsimplices under the wrap (not a valid complex).
        Assert.Throws<ArgumentOutOfRangeException>(() => SimplicialMeshGenerator.CreateUniform4DPeriodic(2));
        Assert.Throws<ArgumentOutOfRangeException>(() => SimplicialMeshGenerator.CreateUniform4DPeriodic(0));
    }

    // ---- Volume topology structural sanity ---------------------------------

    [Fact]
    public void CreateUniform4D_VolumeLayerIsStructurallyValid()
    {
        var mesh = SimplicialMeshGenerator.CreateUniform4D(1);

        // Each volume is a canonical ascending quadruple.
        foreach (var vol in mesh.Volumes)
        {
            Assert.Equal(4, vol.Length);
            Assert.True(vol[0] < vol[1] && vol[1] < vol[2] && vol[2] < vol[3],
                $"Volume [{string.Join(",", vol)}] is not in canonical ascending order.");
        }

        // Each volume has 4 boundary faces with the (-1)^i sign pattern.
        for (int v = 0; v < mesh.VolumeCount; v++)
        {
            Assert.Equal(4, mesh.VolumeBoundaryFaces[v].Length);
            Assert.Equal(new[] { +1, -1, +1, -1 }, mesh.VolumeBoundaryOrientations[v]);
            foreach (int f in mesh.VolumeBoundaryFaces[v])
                Assert.InRange(f, 0, mesh.FaceCount - 1);
        }

        // Each pentachoron has C(5,4) = 5 volumes.
        foreach (var cellVols in mesh.CellVolumes)
            Assert.Equal(5, cellVols.Length);
    }

    // ---- Boundary-of-boundary batteries (exact integer arithmetic) ---------

    [Fact]
    public void CreateUniform4D_BoundaryOfBoundary_B1B2_IsExactlyZero()
    {
        var mesh = SimplicialMeshGenerator.CreateUniform4D(1);
        int[,] b1 = BuildVertexEdgeBoundary(mesh);   // vertices <- edges
        int[,] b2 = BuildEdgeFaceBoundary(mesh);     // edges    <- faces
        int[,] product = MultiplyIntMatrices(b1, b2);
        AssertAllZero(product, "B1 * B2");
    }

    [Fact]
    public void CreateUniform4D_BoundaryOfBoundary_B2B3_IsExactlyZero()
    {
        var mesh = SimplicialMeshGenerator.CreateUniform4D(1);
        int[,] b2 = BuildEdgeFaceBoundary(mesh);     // edges  <- faces
        int[,] b3 = BuildFaceVolumeBoundary(mesh);   // faces  <- volumes
        int[,] product = MultiplyIntMatrices(b2, b3);
        AssertAllZero(product, "B2 * B3");
    }

    // ---- d o d = 0 on the 1 -> 2 -> 3 form ladder --------------------------

    [Fact]
    public void CreateUniform4D_DiscreteDerivative_DDIsZero()
    {
        var mesh = SimplicialMeshGenerator.CreateUniform4D(1);
        const int dimG = 3; // ad-valued carrier

        var rng = new Random(20260702);
        var edgeCoeffs = new double[mesh.EdgeCount * dimG];
        for (int i = 0; i < edgeCoeffs.Length; i++)
            edgeCoeffs[i] = rng.NextDouble() * 2.0 - 1.0;

        double[] faceCoeffs = DiscreteExteriorDerivative.EdgeToFace(mesh, edgeCoeffs, dimG);
        ThreeFormField ddOmega = DiscreteExteriorDerivative.FaceToVolume(mesh, faceCoeffs, dimG);

        Assert.Equal(mesh.VolumeCount * dimG, ddOmega.Coefficients.Length);
        foreach (double c in ddOmega.Coefficients)
            Assert.True(System.Math.Abs(c) < 1e-9, $"d(d omega) component {c} is not zero.");
    }

    // ---- 3D regression: volume layer now populated & self-consistent -------

    [Fact]
    public void CreateUniform3D_NowCarriesSelfConsistentVolumes()
    {
        var mesh = SimplicialMeshGenerator.CreateUniform3D(2);

        // In 3D each top tetrahedron is its own 3-subsimplex.
        Assert.Equal(mesh.CellCount, mesh.VolumeCount);
        foreach (var cellVols in mesh.CellVolumes)
            Assert.Single(cellVols);

        // Boundary-of-boundary still vanishes exactly for the populated volumes.
        int[,] b2 = BuildEdgeFaceBoundary(mesh);
        int[,] b3 = BuildFaceVolumeBoundary(mesh);
        AssertAllZero(MultiplyIntMatrices(b2, b3), "3D B2 * B3");

        // Face->cell sharing is a valid manifold-with-boundary (each face in 1 or 2 cells).
        var faceMult = new int[mesh.FaceCount];
        foreach (var cf in mesh.CellFaces)
            foreach (int f in cf)
                faceMult[f]++;
        foreach (int m in faceMult)
            Assert.True(m == 1 || m == 2, $"3D face shared by {m} cells.");
    }

    // ---- Backward compatibility: 2D meshes carry no volume layer -----------

    [Fact]
    public void CreateUniform2D_HasEmptyVolumeLayer()
    {
        var mesh = SimplicialMeshGenerator.CreateUniform2D(3, 3);

        Assert.Equal(0, mesh.VolumeCount);
        Assert.Empty(mesh.Volumes);
        Assert.Empty(mesh.VolumeBoundaryFaces);
        Assert.Empty(mesh.VolumeBoundaryOrientations);
        Assert.Empty(mesh.CellVolumes);
    }

    [Fact]
    public void HandWrittenMesh_WithoutVolumeProperties_DefaultsToEmpty()
    {
        // Mirrors the existing hand-written test-site initializers: only the
        // required properties are set; the new volume arrays must default to empty.
        var mesh = new SimplicialMesh
        {
            EmbeddingDimension = 2,
            SimplicialDimension = 2,
            VertexCoordinates = new double[] { 0, 0, 1, 0, 0, 1 },
            VertexCount = 3,
            CellVertices = new[] { new[] { 0, 1, 2 } },
            Edges = new[] { new[] { 0, 1 }, new[] { 0, 2 }, new[] { 1, 2 } },
            Faces = new[] { new[] { 0, 1, 2 } },
            CellEdges = new[] { new[] { 0, 1, 2 } },
            CellFaces = new[] { new[] { 0 } },
            FaceBoundaryEdges = new[] { new[] { 0, 1, 2 } },
            FaceBoundaryOrientations = new[] { new[] { +1, -1, +1 } },
            VertexEdges = new[] { new[] { 0, 1 }, new[] { 0, 2 }, new[] { 1, 2 } },
            VertexEdgeOrientations = new[] { new[] { +1, +1 }, new[] { -1, +1 }, new[] { -1, -1 } },
        };

        Assert.Equal(0, mesh.VolumeCount);
        Assert.Empty(mesh.Volumes);
        Assert.Empty(mesh.VolumeBoundaryFaces);
        Assert.Empty(mesh.VolumeBoundaryOrientations);
        Assert.Empty(mesh.CellVolumes);
    }

    // ---- 4D toy / structured factories -------------------------------------

    [Fact]
    public void CreateToy4D_IsTrivialFiberBundleOverSingleTesseract()
    {
        var bundle = ToyGeometryFactory.CreateToy4D();

        Assert.Same(bundle.BaseMesh, bundle.AmbientMesh);
        Assert.Equal(16, bundle.BaseMesh.VertexCount);
        Assert.Equal(24, bundle.BaseMesh.CellCount);
        Assert.Equal(84, bundle.BaseMesh.VolumeCount);
        Assert.True(bundle.ValidateSection());
        Assert.True(bundle.ValidateFibers());
    }

    [Fact]
    public void CreateStructuredFiberBundle4D_TrivialFiber_IsIdentityBundle()
    {
        var bundle = ToyGeometryFactory.CreateStructuredFiberBundle4D(2);

        Assert.Same(bundle.BaseMesh, bundle.AmbientMesh);
        Assert.Equal(4, bundle.BaseMesh.SimplicialDimension);
        Assert.Equal(81, bundle.BaseMesh.VertexCount);
        Assert.True(bundle.ValidateSection());
        Assert.True(bundle.ValidateFibers());
    }

    [Fact]
    public void CreateStructuredFiberBundle4D_NontrivialFiber_IsValidBundle()
    {
        var bundle = ToyGeometryFactory.CreateStructuredFiberBundle4D(1, fiberSize: 3);

        Assert.NotSame(bundle.BaseMesh, bundle.AmbientMesh);
        Assert.Equal(4, bundle.BaseMesh.SimplicialDimension);
        Assert.Equal(bundle.BaseMesh.VertexCount * 3, bundle.AmbientMesh.VertexCount);
        Assert.Equal(4 + 3, bundle.AmbientMesh.EmbeddingDimension);
        Assert.True(bundle.ValidateSection());
        Assert.True(bundle.ValidateFibers());
    }

    [Fact]
    public void CreateStructuredFiberBundle4D_InvalidFiberSize_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => ToyGeometryFactory.CreateStructuredFiberBundle4D(1, fiberSize: 0));
    }

    // ---- Incidence-matrix helpers ------------------------------------------

    /// <summary>
    /// Boundary map ∂1: edges -> vertices. For edge [va &lt; vb], ∂[va,vb] = [vb] - [va].
    /// Returns a (vertexCount x edgeCount) matrix. This is the convention the stored
    /// face/volume boundary signs are consistent with.
    /// </summary>
    private static int[,] BuildVertexEdgeBoundary(SimplicialMesh mesh)
    {
        var b = new int[mesh.VertexCount, mesh.EdgeCount];
        for (int e = 0; e < mesh.EdgeCount; e++)
        {
            int va = mesh.Edges[e][0];
            int vb = mesh.Edges[e][1];
            b[vb, e] += 1;
            b[va, e] -= 1;
        }
        return b;
    }

    /// <summary>Boundary map ∂2: faces -> edges, from the stored face boundary data. (edgeCount x faceCount)</summary>
    private static int[,] BuildEdgeFaceBoundary(SimplicialMesh mesh)
    {
        var b = new int[mesh.EdgeCount, mesh.FaceCount];
        for (int f = 0; f < mesh.FaceCount; f++)
        {
            int[] edges = mesh.FaceBoundaryEdges[f];
            int[] signs = mesh.FaceBoundaryOrientations[f];
            for (int i = 0; i < edges.Length; i++)
                b[edges[i], f] += signs[i];
        }
        return b;
    }

    /// <summary>Boundary map ∂3: volumes -> faces, from the stored volume boundary data. (faceCount x volumeCount)</summary>
    private static int[,] BuildFaceVolumeBoundary(SimplicialMesh mesh)
    {
        var b = new int[mesh.FaceCount, mesh.VolumeCount];
        for (int v = 0; v < mesh.VolumeCount; v++)
        {
            int[] faces = mesh.VolumeBoundaryFaces[v];
            int[] signs = mesh.VolumeBoundaryOrientations[v];
            for (int i = 0; i < faces.Length; i++)
                b[faces[i], v] += signs[i];
        }
        return b;
    }

    private static int[,] MultiplyIntMatrices(int[,] a, int[,] b)
    {
        int rows = a.GetLength(0);
        int inner = a.GetLength(1);
        int cols = b.GetLength(1);
        Assert.Equal(inner, b.GetLength(0));

        var result = new int[rows, cols];
        for (int i = 0; i < rows; i++)
        {
            for (int k = 0; k < inner; k++)
            {
                int aik = a[i, k];
                if (aik == 0) continue;
                for (int j = 0; j < cols; j++)
                    result[i, j] += aik * b[k, j];
            }
        }
        return result;
    }

    private static void AssertAllZero(int[,] m, string label)
    {
        for (int i = 0; i < m.GetLength(0); i++)
            for (int j = 0; j < m.GetLength(1); j++)
                Assert.True(m[i, j] == 0, $"{label} has nonzero entry [{i},{j}] = {m[i, j]}.");
    }
}
