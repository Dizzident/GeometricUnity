using Gu.Geometry;

namespace Gu.Phase4.Dirac;

/// <summary>
/// Builds the diagonal M_psi weight array for the fermionic generalized eigenproblem
///   D_h phi = lambda M_psi phi.
///
/// M_psi is block-diagonal: one vol(c) * I_{dofsPerCell} block per cell.
/// In the real representation used by FermionSpectralSolver (complex DOFs stored as
/// interleaved (Re, Im) pairs), each complex DOF contributes two real entries both
/// equal to vol(c).
///
/// The resulting array has length = totalDof * 2 = cellCount * dofsPerCell * 2.
/// </summary>
public static class MassPsiWeightsBuilder
{
    /// <summary>
    /// Build M_psi diagonal weights from per-cell volumes.
    ///
    /// cellVolumes[c] is the Riemannian volume of cell c (must be positive).
    /// dofsPerCell = spinorDim * gaugeDim.
    ///
    /// Returns a real array of length cellCount * dofsPerCell * 2 where
    /// entry [c * dofsPerCell * 2 + k] = cellVolumes[c] for all k in [0, dofsPerCell*2).
    /// </summary>
    public static double[] BuildFromCellVolumes(double[] cellVolumes, int dofsPerCell)
    {
        ArgumentNullException.ThrowIfNull(cellVolumes);
        if (dofsPerCell <= 0)
            throw new ArgumentOutOfRangeException(nameof(dofsPerCell), "dofsPerCell must be positive.");

        int cellCount = cellVolumes.Length;
        int realDim = cellCount * dofsPerCell * 2;
        var weights = new double[realDim];

        for (int c = 0; c < cellCount; c++)
        {
            double vol = cellVolumes[c];
            if (vol <= 0)
                throw new ArgumentException(
                    $"Cell volume at index {c} is {vol}; must be positive for M_psi.", nameof(cellVolumes));
            int cellBase = c * dofsPerCell * 2;
            for (int k = 0; k < dofsPerCell * 2; k++)
                weights[cellBase + k] = vol;
        }

        return weights;
    }

    /// <summary>
    /// Build uniform M_psi = I weights (all 1.0).
    /// Equivalent to the default when MassPsiWeights is null.
    /// </summary>
    public static double[] BuildIdentity(int cellCount, int dofsPerCell)
    {
        if (cellCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(cellCount));
        if (dofsPerCell <= 0)
            throw new ArgumentOutOfRangeException(nameof(dofsPerCell));

        int realDim = cellCount * dofsPerCell * 2;
        var weights = new double[realDim];
        Array.Fill(weights, 1.0);
        return weights;
    }

    /// <summary>
    /// Build M_psi weights from a SimplicialMesh using a vertex-based dual-cell volume.
    ///
    /// Since the Dirac operator is vertex-centered (totalDof = vertexCount * dofsPerCell),
    /// each vertex weight is set to the average volume of its adjacent cells.
    ///
    /// Returns a real array of length vertexCount * dofsPerCell * 2.
    /// </summary>
    public static double[] BuildFromMesh(SimplicialMesh mesh, int dofsPerCell)
    {
        ArgumentNullException.ThrowIfNull(mesh);
        if (dofsPerCell <= 0)
            throw new ArgumentOutOfRangeException(nameof(dofsPerCell));

        int vertexCount = mesh.VertexCount;
        var vertexVolumes = new double[vertexCount];
        var adjacentCellCounts = new int[vertexCount];

        for (int c = 0; c < mesh.CellCount; c++)
        {
            double cellVol = ComputeCellVolume(mesh, c);
            foreach (int v in mesh.CellVertices[c])
            {
                vertexVolumes[v] += cellVol;
                adjacentCellCounts[v]++;
            }
        }

        // Average over adjacent cells; fallback to 1 for isolated vertices
        for (int v = 0; v < vertexCount; v++)
            vertexVolumes[v] = adjacentCellCounts[v] > 0
                ? vertexVolumes[v] / adjacentCellCounts[v]
                : 1.0;

        return BuildFromCellVolumes(vertexVolumes, dofsPerCell);
    }

    private static double ComputeCellVolume(SimplicialMesh mesh, int cellIndex)
    {
        // For a 2-simplex (triangle) in 2D: area = 0.5 * |cross(v1-v0, v2-v0)|
        // For a d-simplex in d-dimensional embedding: vol = |det(edge matrix)| / d!
        // We handle the cases the mesh reports.
        int dim = mesh.EmbeddingDimension;
        int sDim = mesh.SimplicialDimension;

        // Retrieve vertex indices for this cell
        var cellVertices = mesh.CellVertices[cellIndex];
        int nVerts = cellVertices.Length; // = sDim + 1

        if (nVerts <= 1)
            return 1.0; // degenerate, return 1

        // Build edge vectors from v_0: columns are (v_i - v_0) for i=1..nVerts-1
        // Matrix is dim x sDim
        int rows = dim;
        int cols = sDim;
        var mat = new double[rows, cols];

        var coords = mesh.VertexCoordinates;
        for (int j = 0; j < cols; j++)
        {
            int vIdx = cellVertices[j + 1];
            int v0Idx = cellVertices[0];
            for (int i = 0; i < rows && i < dim; i++)
                mat[i, j] = coords[vIdx * dim + i] - coords[v0Idx * dim + i];
        }

        // For square matrices (sDim == dim), compute |det| / sDim!
        if (rows == cols)
        {
            double det = ComputeDet(mat, rows);
            double factorial = Factorial(sDim);
            return System.Math.Abs(det) / factorial;
        }

        // Non-square: use Gram determinant sqrt(det(A^T A)) / sDim!
        var gramDet = GramDeterminant(mat, rows, cols);
        return System.Math.Sqrt(System.Math.Max(gramDet, 0.0)) / Factorial(sDim);
    }

    private static double ComputeDet(double[,] m, int n)
    {
        // LU decomposition for determinant
        var a = new double[n, n];
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
                a[i, j] = m[i, j];

        double det = 1.0;
        for (int col = 0; col < n; col++)
        {
            // Partial pivot
            int pivot = col;
            for (int row = col + 1; row < n; row++)
                if (System.Math.Abs(a[row, col]) > System.Math.Abs(a[pivot, col]))
                    pivot = row;

            if (pivot != col)
            {
                for (int k = 0; k < n; k++) (a[col, k], a[pivot, k]) = (a[pivot, k], a[col, k]);
                det = -det;
            }

            if (System.Math.Abs(a[col, col]) < 1e-15)
                return 0.0;

            det *= a[col, col];
            for (int row = col + 1; row < n; row++)
            {
                double factor = a[row, col] / a[col, col];
                for (int k = col; k < n; k++)
                    a[row, k] -= factor * a[col, k];
            }
        }
        return det;
    }

    private static double GramDeterminant(double[,] a, int rows, int cols)
    {
        // det(A^T A), where A is rows x cols
        // A^T A is cols x cols
        var gram = new double[cols, cols];
        for (int i = 0; i < cols; i++)
            for (int j = 0; j < cols; j++)
            {
                double s = 0;
                for (int k = 0; k < rows; k++) s += a[k, i] * a[k, j];
                gram[i, j] = s;
            }
        return ComputeDet(gram, cols);
    }

    private static double Factorial(int n)
    {
        double f = 1.0;
        for (int i = 2; i <= n; i++) f *= i;
        return f;
    }
}
