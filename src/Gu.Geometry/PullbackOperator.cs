using Gu.Core;

namespace Gu.Geometry;

/// <summary>
/// Implements sigma_h^*: extracts/interpolates a field on Y_h to a field on X_h
/// via the observation section sigma_h.
/// No observable may bypass this layer (IA-6).
/// </summary>
public sealed class PullbackOperator
{
    private readonly FiberBundleMesh _bundle;
    private readonly Dictionary<(int, int), int> _yEdgeLookup;
    private readonly Dictionary<(int, int, int), int> _yFaceLookup;

    public PullbackOperator(FiberBundleMesh bundle)
    {
        _bundle = bundle ?? throw new ArgumentNullException(nameof(bundle));

        // Build O(1) edge lookup for ApplyEdgeField
        _yEdgeLookup = new Dictionary<(int, int), int>(bundle.AmbientMesh.EdgeCount);
        for (int ye = 0; ye < bundle.AmbientMesh.EdgeCount; ye++)
        {
            var edge = bundle.AmbientMesh.Edges[ye];
            _yEdgeLookup[(edge[0], edge[1])] = ye;
        }

        // Build O(1) face lookup for ApplyFaceField
        _yFaceLookup = new Dictionary<(int, int, int), int>(bundle.AmbientMesh.FaceCount);
        for (int yf = 0; yf < bundle.AmbientMesh.FaceCount; yf++)
        {
            var face = bundle.AmbientMesh.Faces[yf];
            _yFaceLookup[(face[0], face[1], face[2])] = yf;
        }
    }

    /// <summary>
    /// Pull back a vertex-based scalar field from Y_h to X_h using sigma_h.
    /// For each X vertex, extracts the field value at sigma(x) in Y_h.
    /// </summary>
    /// <param name="yField">Field on Y_h with one scalar per vertex.</param>
    /// <returns>Field on X_h with one scalar per vertex.</returns>
    public FieldTensor ApplyVertexScalar(FieldTensor yField)
    {
        if (yField.Coefficients.Length != _bundle.AmbientMesh.VertexCount)
            throw new ArgumentException(
                $"Expected {_bundle.AmbientMesh.VertexCount} coefficients for Y_h vertex field, got {yField.Coefficients.Length}.");

        var xCoeffs = new double[_bundle.BaseMesh.VertexCount];

        for (int x = 0; x < _bundle.BaseMesh.VertexCount; x++)
        {
            int yVertex = _bundle.XVertexToYVertex[x];
            xCoeffs[x] = yField.Coefficients[yVertex];
        }

        return new FieldTensor
        {
            Label = $"sigma_h*({yField.Label})",
            Signature = new TensorSignature
            {
                AmbientSpaceId = "X_h",
                CarrierType = yField.Signature.CarrierType,
                Degree = yField.Signature.Degree,
                LieAlgebraBasisId = yField.Signature.LieAlgebraBasisId,
                ComponentOrderId = yField.Signature.ComponentOrderId,
                NumericPrecision = yField.Signature.NumericPrecision,
                MemoryLayout = yField.Signature.MemoryLayout,
                BackendPacking = yField.Signature.BackendPacking,
            },
            Coefficients = xCoeffs,
            Shape = new[] { _bundle.BaseMesh.VertexCount },
        };
    }

    /// <summary>
    /// Pull back a multi-component vertex field from Y_h to X_h.
    /// Each vertex has componentsPerVertex values (e.g., Lie algebra coefficients).
    /// </summary>
    /// <param name="yField">Field on Y_h. Shape: [AmbientMesh.VertexCount, componentsPerVertex].</param>
    /// <param name="componentsPerVertex">Number of components per vertex.</param>
    /// <returns>Field on X_h with same components per vertex.</returns>
    public FieldTensor ApplyVertexMultiComponent(FieldTensor yField, int componentsPerVertex)
    {
        int expectedLength = _bundle.AmbientMesh.VertexCount * componentsPerVertex;
        if (yField.Coefficients.Length != expectedLength)
            throw new ArgumentException(
                $"Expected {expectedLength} coefficients, got {yField.Coefficients.Length}.");

        var xCoeffs = new double[_bundle.BaseMesh.VertexCount * componentsPerVertex];

        for (int x = 0; x < _bundle.BaseMesh.VertexCount; x++)
        {
            int yVertex = _bundle.XVertexToYVertex[x];
            Array.Copy(
                yField.Coefficients, yVertex * componentsPerVertex,
                xCoeffs, x * componentsPerVertex,
                componentsPerVertex);
        }

        return new FieldTensor
        {
            Label = $"sigma_h*({yField.Label})",
            Signature = new TensorSignature
            {
                AmbientSpaceId = "X_h",
                CarrierType = yField.Signature.CarrierType,
                Degree = yField.Signature.Degree,
                LieAlgebraBasisId = yField.Signature.LieAlgebraBasisId,
                ComponentOrderId = yField.Signature.ComponentOrderId,
                NumericPrecision = yField.Signature.NumericPrecision,
                MemoryLayout = yField.Signature.MemoryLayout,
                BackendPacking = yField.Signature.BackendPacking,
            },
            Coefficients = xCoeffs,
            Shape = new[] { _bundle.BaseMesh.VertexCount, componentsPerVertex },
        };
    }

    /// <summary>
    /// Pull back an edge-based field from Y_h to X_h using cell-level section mapping.
    /// For each X cell, interpolates the field within the mapped Y cell
    /// using the section coefficients.
    /// </summary>
    /// <param name="yField">Edge-based field on Y_h (e.g., omega_h).</param>
    /// <param name="componentsPerEdge">Number of components per edge (e.g., dim(g)).</param>
    /// <returns>Edge-based field on X_h.</returns>
    public FieldTensor ApplyEdgeField(FieldTensor yField, int componentsPerEdge)
    {
        // For edge-based fields, we need a more sophisticated pullback.
        // For the vertex-aligned section case, we map X edges to Y edges
        // by mapping their endpoint vertices through sigma_h.
        var xEdgeCoeffs = new double[_bundle.BaseMesh.EdgeCount * componentsPerEdge];

        for (int xe = 0; xe < _bundle.BaseMesh.EdgeCount; xe++)
        {
            var xEdge = _bundle.BaseMesh.Edges[xe];
            int xv0 = xEdge[0], xv1 = xEdge[1];

            // Map X edge endpoints to Y via sigma_h
            int yv0 = _bundle.XVertexToYVertex[xv0];
            int yv1 = _bundle.XVertexToYVertex[xv1];

            // Find the corresponding Y edge (canonical ordering)
            int yv0s = System.Math.Min(yv0, yv1);
            int yv1s = System.Math.Max(yv0, yv1);

            if (_yEdgeLookup.TryGetValue((yv0s, yv1s), out int yEdgeIdx))
            {
                // Orientation: +1 if sigma preserves vertex order, -1 if reversed
                int sign = (yv0 == yv0s) ? +1 : -1;

                for (int c = 0; c < componentsPerEdge; c++)
                {
                    xEdgeCoeffs[xe * componentsPerEdge + c] =
                        sign * yField.Coefficients[yEdgeIdx * componentsPerEdge + c];
                }
            }
            // else: edge not found in Y -- leave as zero (boundary or degenerate case)
        }

        return new FieldTensor
        {
            Label = $"sigma_h*({yField.Label})",
            Signature = new TensorSignature
            {
                AmbientSpaceId = "X_h",
                CarrierType = yField.Signature.CarrierType,
                Degree = yField.Signature.Degree,
                LieAlgebraBasisId = yField.Signature.LieAlgebraBasisId,
                ComponentOrderId = yField.Signature.ComponentOrderId,
                NumericPrecision = yField.Signature.NumericPrecision,
                MemoryLayout = yField.Signature.MemoryLayout,
                BackendPacking = yField.Signature.BackendPacking,
            },
            Coefficients = xEdgeCoeffs,
            Shape = new[] { _bundle.BaseMesh.EdgeCount, componentsPerEdge },
        };
    }

    /// <summary>
    /// Pull back a face-based field (2-form) from Y_h to X_h.
    /// For each X_h face, maps vertices through sigma_h to find the
    /// corresponding Y_h face, with orientation correction for antisymmetry.
    /// Used for curvature F_h, torsion, Shiab, and residual fields.
    /// </summary>
    /// <param name="yField">Face-based field on Y_h (e.g., F_h).</param>
    /// <param name="componentsPerFace">Number of components per face (e.g., dim(g)).</param>
    /// <returns>Face-based field on X_h.</returns>
    public FieldTensor ApplyFaceField(FieldTensor yField, int componentsPerFace)
    {
        var xFaceCoeffs = new double[_bundle.BaseMesh.FaceCount * componentsPerFace];

        for (int xf = 0; xf < _bundle.BaseMesh.FaceCount; xf++)
        {
            var xFace = _bundle.BaseMesh.Faces[xf];
            int xv0 = xFace[0], xv1 = xFace[1], xv2 = xFace[2];

            // Map X face vertices to Y via sigma_h
            int yv0 = _bundle.XVertexToYVertex[xv0];
            int yv1 = _bundle.XVertexToYVertex[xv1];
            int yv2 = _bundle.XVertexToYVertex[xv2];

            // Sort to canonical ordering and track permutation parity
            int sign = CanonicalFaceSign(yv0, yv1, yv2, out int ys0, out int ys1, out int ys2);

            if (_yFaceLookup.TryGetValue((ys0, ys1, ys2), out int yFaceIdx))
            {
                for (int c = 0; c < componentsPerFace; c++)
                {
                    xFaceCoeffs[xf * componentsPerFace + c] =
                        sign * yField.Coefficients[yFaceIdx * componentsPerFace + c];
                }
            }
            // else: face not found in Y -- leave as zero
        }

        return new FieldTensor
        {
            Label = $"sigma_h*({yField.Label})",
            Signature = new TensorSignature
            {
                AmbientSpaceId = "X_h",
                CarrierType = yField.Signature.CarrierType,
                Degree = yField.Signature.Degree,
                LieAlgebraBasisId = yField.Signature.LieAlgebraBasisId,
                ComponentOrderId = yField.Signature.ComponentOrderId,
                NumericPrecision = yField.Signature.NumericPrecision,
                MemoryLayout = yField.Signature.MemoryLayout,
                BackendPacking = yField.Signature.BackendPacking,
            },
            Coefficients = xFaceCoeffs,
            Shape = new[] { _bundle.BaseMesh.FaceCount, componentsPerFace },
        };
    }

    /// <summary>
    /// Pull back a multi-component cell field from Y_h to X_h using sigma_h.
    /// Each cell has componentsPerCell values (e.g., spinor DoFs: 2*dofsPerCell for Re/Im interleaving).
    /// No orientation correction is needed — cells are volumes, not antisymmetric forms.
    /// </summary>
    /// <param name="yField">Field on Y_h. Shape: [AmbientMesh.CellCount, componentsPerCell].</param>
    /// <param name="componentsPerCell">Number of components per cell. For fermion spinors with
    /// Re/Im interleaving, pass 2*dofsPerCell.</param>
    /// <returns>Field on X_h with same components per cell.</returns>
    public FieldTensor ApplyCellField(FieldTensor yField, int componentsPerCell)
    {
        int expectedLength = _bundle.AmbientMesh.CellCount * componentsPerCell;
        if (yField.Coefficients.Length != expectedLength)
            throw new ArgumentException(
                $"Expected {expectedLength} coefficients ({_bundle.AmbientMesh.CellCount} Y cells * {componentsPerCell} components), got {yField.Coefficients.Length}.");

        var xCoeffs = new double[_bundle.BaseMesh.CellCount * componentsPerCell];

        for (int xc = 0; xc < _bundle.BaseMesh.CellCount; xc++)
        {
            int yCell = _bundle.XCellToYCell[xc];
            Array.Copy(
                yField.Coefficients, yCell * componentsPerCell,
                xCoeffs, xc * componentsPerCell,
                componentsPerCell);
        }

        return new FieldTensor
        {
            Label = $"sigma_h*({yField.Label})",
            Signature = new TensorSignature
            {
                AmbientSpaceId = "X_h",
                CarrierType = yField.Signature.CarrierType,
                Degree = yField.Signature.Degree,
                LieAlgebraBasisId = yField.Signature.LieAlgebraBasisId,
                ComponentOrderId = yField.Signature.ComponentOrderId,
                NumericPrecision = yField.Signature.NumericPrecision,
                MemoryLayout = yField.Signature.MemoryLayout,
                BackendPacking = yField.Signature.BackendPacking,
            },
            Coefficients = xCoeffs,
            Shape = new[] { _bundle.BaseMesh.CellCount, componentsPerCell },
        };
    }

    /// <summary>
    /// Sorts three vertex indices into canonical order (ascending) and returns
    /// the permutation sign (+1 for even, -1 for odd permutation).
    /// </summary>
    private static int CanonicalFaceSign(int v0, int v1, int v2, out int s0, out int s1, out int s2)
    {
        // Bubble sort with swap counting for 3 elements
        int swaps = 0;
        s0 = v0; s1 = v1; s2 = v2;

        if (s0 > s1) { (s0, s1) = (s1, s0); swaps++; }
        if (s1 > s2) { (s1, s2) = (s2, s1); swaps++; }
        if (s0 > s1) { (s0, s1) = (s1, s0); swaps++; }

        return (swaps % 2 == 0) ? +1 : -1;
    }
}
