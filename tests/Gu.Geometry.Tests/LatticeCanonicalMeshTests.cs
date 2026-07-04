using System.Security.Cryptography;
using Gu.Geometry;

namespace Gu.Geometry.Tests;

/// <summary>
/// Tests for the lattice-canonical orientation convention (UNLOCK project (i)):
/// <see cref="SimplicialMeshGenerator.CreateUniform4DPeriodic(int, bool)"/> with
/// latticeCanonical = true stores face/volume tuples in the chain order of their
/// minimal-image lattice displacements, so the entire oriented topology — and hence
/// every discrete action assembled from the emitted arrays — commutes with lattice
/// translations on the torus. Phase444 measured the global-index convention failing
/// translation covariance at 2.5e-4 for pure ||F||^2; the canonical convention must
/// reach floating-point roundoff while remaining byte-identical on every default path.
/// </summary>
public class LatticeCanonicalMeshTests
{
    // ---- Hard gate: default paths are byte-identical to the historical builder ----

    // SHA256 of all topology arrays, computed from the pre-change builder (commit
    // 82713d61). Any drift on an UN-GATED path is a regression, not a re-pin.
    private const string Open4DN1Hash = "df7b7cfd3498611f1b3ee587ed7633c11c4d53809d9ec271dd6463f012f540c0";
    private const string Open4DN2Hash = "035e88e2c0011087927752d0d7a9cd19c6ed69d54c55f664caf3495419da4283";
    private const string DefaultTorusN3Hash = "c5de1e32d34c4e7b8b2af61f30d150d8847165658e7d66890772934712dd6722";

    [Fact]
    public void OpenMesh_CreateUniform4D_TopologyByteIdenticalToPreChangeBuilder()
    {
        Assert.Equal(Open4DN1Hash, TopologyHash(SimplicialMeshGenerator.CreateUniform4D(1)));
        Assert.Equal(Open4DN2Hash, TopologyHash(SimplicialMeshGenerator.CreateUniform4D(2)));
    }

    [Fact]
    public void PeriodicMesh_DefaultConvention_TopologyByteIdenticalToPreChangeBuilder()
    {
        Assert.Equal(DefaultTorusN3Hash, TopologyHash(SimplicialMeshGenerator.CreateUniform4DPeriodic(3)));
        Assert.Equal(
            DefaultTorusN3Hash,
            TopologyHash(SimplicialMeshGenerator.CreateUniform4DPeriodic(3, latticeCanonical: false)));
    }

    // ---- Canonical torus: same simplices, same indices, only tuples/signs differ ----

    [Fact]
    public void CanonicalTorus_HasSameSimplicesAndIndicesAsDefault()
    {
        var plain = SimplicialMeshGenerator.CreateUniform4DPeriodic(3);
        var canon = SimplicialMeshGenerator.CreateUniform4DPeriodic(3, latticeCanonical: true);

        Assert.Equal(plain.VertexCount, canon.VertexCount);
        Assert.Equal(plain.EdgeCount, canon.EdgeCount);
        Assert.Equal(plain.FaceCount, canon.FaceCount);
        Assert.Equal(plain.VolumeCount, canon.VolumeCount);
        Assert.Equal(plain.CellCount, canon.CellCount);

        // Edges and vertex-edge incidence are untouched.
        for (int e = 0; e < plain.EdgeCount; e++)
            Assert.Equal(plain.Edges[e], canon.Edges[e]);
        for (int v = 0; v < plain.VertexCount; v++)
        {
            Assert.Equal(plain.VertexEdges[v], canon.VertexEdges[v]);
            Assert.Equal(plain.VertexEdgeOrientations[v], canon.VertexEdgeOrientations[v]);
        }

        // Face/volume index assignment is identical (dedup key is the sorted tuple in
        // both modes); only the stored tuple ORDER may differ.
        for (int f = 0; f < plain.FaceCount; f++)
        {
            var sorted = (int[])canon.Faces[f].Clone();
            Array.Sort(sorted);
            Assert.Equal(plain.Faces[f], sorted);
        }

        for (int w = 0; w < plain.VolumeCount; w++)
        {
            var sorted = (int[])canon.Volumes[w].Clone();
            Array.Sort(sorted);
            Assert.Equal(plain.Volumes[w], sorted);
        }

        // Cell incidence lists (index sets in position-combination order) are identical.
        for (int c = 0; c < plain.CellCount; c++)
        {
            Assert.Equal(plain.CellVertices[c], canon.CellVertices[c]);
            Assert.Equal(plain.CellEdges[c], canon.CellEdges[c]);
            Assert.Equal(plain.CellFaces[c], canon.CellFaces[c]);
            Assert.Equal(plain.CellVolumes[c], canon.CellVolumes[c]);
        }
    }

    [Fact]
    public void CanonicalTorus_VolumeBoundaryOrientations_KeepAlternatingPattern()
    {
        // Subchains of chain-ordered quadruples are chain-ordered, so the generic
        // parity is +1 and the (-1)^i contract survives verbatim.
        var canon = SimplicialMeshGenerator.CreateUniform4DPeriodic(3, latticeCanonical: true);
        for (int w = 0; w < canon.VolumeCount; w++)
            Assert.Equal(new[] { +1, -1, +1, -1 }, canon.VolumeBoundaryOrientations[w]);
    }

    // ---- Acceptance 1: the oriented chain complex is exact (∂∘∂ = 0) ----

    [Fact]
    public void CanonicalTorus_BoundaryOfBoundary_IsExactlyZero()
    {
        var mesh = SimplicialMeshGenerator.CreateUniform4DPeriodic(3, latticeCanonical: true);

        for (int w = 0; w < mesh.VolumeCount; w++)
        {
            var edgeChain = new Dictionary<int, int>();
            for (int i = 0; i < 4; i++)
            {
                int f = mesh.VolumeBoundaryFaces[w][i];
                int sf = mesh.VolumeBoundaryOrientations[w][i];
                for (int j = 0; j < 3; j++)
                {
                    int e = mesh.FaceBoundaryEdges[f][j];
                    int se = mesh.FaceBoundaryOrientations[f][j];
                    edgeChain[e] = edgeChain.GetValueOrDefault(e) + sf * se;
                }
            }

            Assert.All(edgeChain.Values, c => Assert.Equal(0, c));
        }
    }

    // ---- Acceptance 3 prerequisite: tuples are translation-equivariant ----

    [Fact]
    public void CanonicalTorus_FaceAndVolumeTuples_AreTranslationEquivariant()
    {
        const int n = 3;
        var mesh = SimplicialMeshGenerator.CreateUniform4DPeriodic(n, latticeCanonical: true);
        var t = new TorusTranslations(mesh, n);

        foreach (var r in TorusTranslations.SampleTranslations)
        {
            for (int f = 0; f < mesh.FaceCount; f++)
            {
                var verts = mesh.Faces[f];
                var image = new[] { t.Translate(verts[0], r), t.Translate(verts[1], r), t.Translate(verts[2], r) };
                int g = t.FaceIndexOf(image);
                Assert.Equal(image, mesh.Faces[g]);
            }

            for (int w = 0; w < mesh.VolumeCount; w++)
            {
                var verts = mesh.Volumes[w];
                var image = new[]
                {
                    t.Translate(verts[0], r), t.Translate(verts[1], r),
                    t.Translate(verts[2], r), t.Translate(verts[3], r),
                };
                int g = t.VolumeIndexOf(image);
                Assert.Equal(image, mesh.Volumes[g]);
            }
        }
    }

    // ---- Acceptance 3: the discrete curvature action is translation-covariant ----
    // Phase444 methodology: S(T_R omega) vs S(omega) under the SIGNED edge permutation
    // (a connection 1-form negates when its stored edge direction reverses), where
    // S = sum_f |F_f|^2 and F = d(omega) + (1/2)[omega, omega] assembled exactly as
    // CurvatureAssembler does (boundary edges + orientations + pairwise bracket).

    [Fact]
    public void CanonicalTorus_CurvatureAction_IsTranslationCovariantToRoundoff()
    {
        double residual = CurvatureActionCovarianceResidual(latticeCanonical: true);
        Assert.True(residual < 1e-12, $"Curvature action covariance residual {residual:E3} exceeds 1e-12.");
    }

    [Fact]
    public void DefaultTorus_CurvatureAction_IsNotTranslationCovariant_DocumentedLimitation()
    {
        // The phase444 F3 finding (measured 2.5e-4 there): the global-index-sorted
        // convention does NOT commute with lattice translation. Keeps teeth in the
        // covariance test above and documents why the canonical mode exists.
        double residual = CurvatureActionCovarianceResidual(latticeCanonical: false);
        Assert.True(residual > 1e-6, $"Expected the default convention to violate covariance, got {residual:E3}.");
    }

    [Fact]
    public void CanonicalTorus_CoulombDivergence_IsTranslationCovariantToRoundoff()
    {
        const int n = 3;
        var mesh = SimplicialMeshGenerator.CreateUniform4DPeriodic(n, latticeCanonical: true);
        var t = new TorusTranslations(mesh, n);
        var rng = new Random(20260704);
        var omega = RandomEdgeField(mesh, rng);

        double reference = CoulombNormSquared(mesh, omega);
        double worst = 0;
        foreach (var r in TorusTranslations.SampleTranslations)
        {
            double translated = CoulombNormSquared(mesh, t.SignedPermuteEdgeField(omega, r));
            worst = System.Math.Max(worst, System.Math.Abs(translated - reference) / System.Math.Abs(reference));
        }

        Assert.True(worst < 1e-12, $"Coulomb d* covariance residual {worst:E3} exceeds 1e-12.");
    }

    // ---- Builder guards ----

    [Fact]
    public void Build_LatticePeriodBelowThree_Throws()
    {
        var coords = new double[] { 0, 0, 1, 0, 0, 1 };
        var cells = new[] { new[] { 0, 1, 2 } };

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            MeshTopologyBuilder.Build(2, 2, coords, 3, cells, latticePeriod: -1));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            MeshTopologyBuilder.Build(2, 2, coords, 3, cells, latticePeriod: 1));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            MeshTopologyBuilder.Build(2, 2, coords, 3, cells, latticePeriod: 2));
    }

    [Fact]
    public void Build_LatticePeriodOnNonLatticeMesh_Throws()
    {
        // Non-integer vertex displacement: the lattice-canonical convention is
        // undefined, so the builder must fail loudly instead of guessing.
        var coords = new double[] { 0, 0, 1, 0, 0.5, 0.5 };
        var cells = new[] { new[] { 0, 1, 2 } };

        Assert.Throws<InvalidOperationException>(() =>
            MeshTopologyBuilder.Build(2, 2, coords, 3, cells, latticePeriod: 3));
    }

    // =====================================================================
    // Test machinery (phase444 methodology)
    // =====================================================================

    private static double CurvatureActionCovarianceResidual(bool latticeCanonical)
    {
        const int n = 3;
        var mesh = SimplicialMeshGenerator.CreateUniform4DPeriodic(n, latticeCanonical);
        var t = new TorusTranslations(mesh, n);
        var rng = new Random(20260703);

        double worst = 0;
        for (int trial = 0; trial < 2; trial++)
        {
            var omega = RandomEdgeField(mesh, rng);
            double reference = CurvatureActionNormSquared(mesh, omega);
            foreach (var r in TorusTranslations.SampleTranslations)
            {
                double translated = CurvatureActionNormSquared(mesh, t.SignedPermuteEdgeField(omega, r));
                worst = System.Math.Max(
                    worst, System.Math.Abs(translated - reference) / System.Math.Abs(reference));
            }
        }

        return worst;
    }

    /// <summary>su(2) edge field: 3 coefficients per edge, seeded random.</summary>
    private static double[] RandomEdgeField(SimplicialMesh mesh, Random rng)
    {
        var omega = new double[mesh.EdgeCount * 3];
        for (int i = 0; i < omega.Length; i++)
            omega[i] = 0.4 * (rng.NextDouble() - 0.5);
        return omega;
    }

    /// <summary>
    /// S = sum_f |F_f|^2 with F = d(omega) + (1/2)[omega, omega], assembled per face
    /// from FaceBoundaryEdges/FaceBoundaryOrientations exactly as CurvatureAssembler
    /// does, with the su(2) bracket [x,y] = x × y.
    /// </summary>
    private static double CurvatureActionNormSquared(SimplicialMesh mesh, double[] omega)
    {
        double total = 0;
        Span<double> f = stackalloc double[3];
        Span<double> oi = stackalloc double[3];
        Span<double> oj = stackalloc double[3];

        for (int fi = 0; fi < mesh.FaceCount; fi++)
        {
            var edges = mesh.FaceBoundaryEdges[fi];
            var signs = mesh.FaceBoundaryOrientations[fi];

            f.Clear();
            for (int i = 0; i < edges.Length; i++)
            {
                for (int a = 0; a < 3; a++)
                    f[a] += signs[i] * omega[edges[i] * 3 + a];
            }

            for (int i = 0; i < edges.Length; i++)
            {
                for (int j = i + 1; j < edges.Length; j++)
                {
                    for (int a = 0; a < 3; a++)
                    {
                        oi[a] = signs[i] * omega[edges[i] * 3 + a];
                        oj[a] = signs[j] * omega[edges[j] * 3 + a];
                    }

                    // (1/2) [oi, oj] with the su(2) cross-product bracket.
                    f[0] += 0.5 * (oi[1] * oj[2] - oi[2] * oj[1]);
                    f[1] += 0.5 * (oi[2] * oj[0] - oi[0] * oj[2]);
                    f[2] += 0.5 * (oi[0] * oj[1] - oi[1] * oj[0]);
                }
            }

            total += f[0] * f[0] + f[1] * f[1] + f[2] * f[2];
        }

        return total;
    }

    /// <summary>sum_v |d* omega(v)|^2 from VertexEdges/VertexEdgeOrientations.</summary>
    private static double CoulombNormSquared(SimplicialMesh mesh, double[] omega)
    {
        double total = 0;
        Span<double> acc = stackalloc double[3];

        for (int v = 0; v < mesh.VertexCount; v++)
        {
            acc.Clear();
            var edges = mesh.VertexEdges[v];
            var signs = mesh.VertexEdgeOrientations[v];
            for (int i = 0; i < edges.Length; i++)
            {
                for (int a = 0; a < 3; a++)
                    acc[a] += signs[i] * omega[edges[i] * 3 + a];
            }

            total += acc[0] * acc[0] + acc[1] * acc[1] + acc[2] * acc[2];
        }

        return total;
    }

    /// <summary>
    /// Lattice translation machinery on the (Z_n)^4 torus: vertex/edge/face/volume
    /// index maps and the signed edge-field permutation (phase444 methodology — a
    /// connection 1-form negates when its stored edge direction reverses).
    /// </summary>
    private sealed class TorusTranslations
    {
        public static readonly int[][] SampleTranslations =
        [
            [1, 0, 0, 0], [0, 1, 0, 0], [0, 0, 1, 0], [0, 0, 0, 1],
            [1, 1, 0, 0], [2, 1, 2, 0], [1, 2, 1, 2],
        ];

        private readonly SimplicialMesh _mesh;
        private readonly int _n;
        private readonly Dictionary<(int, int, int, int), int> _coordToVertex = new();
        private readonly Dictionary<(int, int), int> _edgeLookup = new();
        private readonly Dictionary<(int, int, int), int> _faceLookup = new();
        private readonly Dictionary<(int, int, int, int), int> _volumeLookup = new();

        public TorusTranslations(SimplicialMesh mesh, int n)
        {
            _mesh = mesh;
            _n = n;

            for (int v = 0; v < mesh.VertexCount; v++)
            {
                var c = Coord(v);
                _coordToVertex[(c[0], c[1], c[2], c[3])] = v;
            }

            for (int e = 0; e < mesh.EdgeCount; e++)
                _edgeLookup[(mesh.Edges[e][0], mesh.Edges[e][1])] = e;

            for (int f = 0; f < mesh.FaceCount; f++)
            {
                var s = (int[])mesh.Faces[f].Clone();
                Array.Sort(s);
                _faceLookup[(s[0], s[1], s[2])] = f;
            }

            for (int w = 0; w < mesh.VolumeCount; w++)
            {
                var s = (int[])mesh.Volumes[w].Clone();
                Array.Sort(s);
                _volumeLookup[(s[0], s[1], s[2], s[3])] = w;
            }
        }

        public int Translate(int vertex, int[] r)
        {
            var c = Coord(vertex);
            return _coordToVertex[(
                Wrap(c[0] + r[0]), Wrap(c[1] + r[1]), Wrap(c[2] + r[2]), Wrap(c[3] + r[3]))];
        }

        public int FaceIndexOf(int[] verts)
        {
            var s = (int[])verts.Clone();
            Array.Sort(s);
            return _faceLookup[(s[0], s[1], s[2])];
        }

        public int VolumeIndexOf(int[] verts)
        {
            var s = (int[])verts.Clone();
            Array.Sort(s);
            return _volumeLookup[(s[0], s[1], s[2], s[3])];
        }

        public double[] SignedPermuteEdgeField(double[] omega, int[] r)
        {
            var result = new double[omega.Length];
            for (int e = 0; e < _mesh.EdgeCount; e++)
            {
                int a = Translate(_mesh.Edges[e][0], r);
                int b = Translate(_mesh.Edges[e][1], r);
                int target = _edgeLookup[(System.Math.Min(a, b), System.Math.Max(a, b))];
                int sign = a < b ? +1 : -1; // stored edges are (min, max)
                for (int c = 0; c < 3; c++)
                    result[target * 3 + c] = sign * omega[e * 3 + c];
            }

            return result;
        }

        private int Wrap(int x) => ((x % _n) + _n) % _n;

        private int[] Coord(int vertex)
        {
            var c = _mesh.GetVertexCoordinates(vertex);
            return
            [
                (int)System.Math.Round(c[0]), (int)System.Math.Round(c[1]),
                (int)System.Math.Round(c[2]), (int)System.Math.Round(c[3]),
            ];
        }
    }

    // =====================================================================
    // Topology hashing (the byte-identity gate)
    // =====================================================================

    private static string TopologyHash(SimplicialMesh mesh)
    {
        using var ms = new MemoryStream();
        using (var w = new BinaryWriter(ms, System.Text.Encoding.UTF8, leaveOpen: true))
        {
            w.Write(mesh.EmbeddingDimension);
            w.Write(mesh.SimplicialDimension);
            w.Write(mesh.VertexCount);
            w.Write(mesh.VertexCoordinates.Length);
            foreach (double c in mesh.VertexCoordinates) w.Write(c);
            Append(w, mesh.CellVertices);
            Append(w, mesh.Edges);
            Append(w, mesh.Faces);
            Append(w, mesh.CellEdges);
            Append(w, mesh.CellFaces);
            Append(w, mesh.FaceBoundaryEdges);
            Append(w, mesh.FaceBoundaryOrientations);
            Append(w, mesh.VertexEdges);
            Append(w, mesh.VertexEdgeOrientations);
            Append(w, mesh.Volumes);
            Append(w, mesh.VolumeBoundaryFaces);
            Append(w, mesh.VolumeBoundaryOrientations);
            Append(w, mesh.CellVolumes);
        }

        return Convert.ToHexString(SHA256.HashData(ms.ToArray())).ToLowerInvariant();
    }

    private static void Append(BinaryWriter w, int[][] arrays)
    {
        w.Write(arrays.Length);
        foreach (var a in arrays)
        {
            w.Write(a.Length);
            foreach (int x in a) w.Write(x);
        }
    }
}
