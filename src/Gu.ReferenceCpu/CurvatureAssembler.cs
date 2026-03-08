using Gu.Core;
using Gu.Geometry;
using Gu.Math;

namespace Gu.ReferenceCpu;

/// <summary>
/// Assembles curvature F_h from a connection omega_h on Y_h.
/// F = d(omega) + (1/2)[omega, omega]
///
/// In discrete form on simplicial mesh:
/// - F_h lives on faces (2-subsimplices) of Y_h
/// - d(omega) on face = sum of signed omega values on boundary edges
/// - [omega, omega] on face = sum over edge pairs using Lie bracket
/// </summary>
public static class CurvatureAssembler
{
    /// <summary>
    /// Compute curvature F_h for a given connection omega_h.
    /// F_h has dim(g) coefficients per face of Y_h.
    /// </summary>
    /// <param name="omega">The discrete connection field.</param>
    /// <returns>Curvature field as a CurvatureField object.</returns>
    public static CurvatureField Assemble(ConnectionField omega)
    {
        var mesh = omega.Mesh;
        var algebra = omega.Algebra;
        int dimG = algebra.Dimension;
        int faceCount = mesh.FaceCount;

        var coefficients = new double[faceCount * dimG];

        for (int fi = 0; fi < faceCount; fi++)
        {
            // 1. Compute d(omega) on this face using the boundary operator
            //    d(omega)[face] = sum_i sign_i * omega[edge_i]
            var dOmega = new double[dimG];
            var boundaryEdges = mesh.FaceBoundaryEdges[fi];
            var boundaryOrientations = mesh.FaceBoundaryOrientations[fi];

            for (int i = 0; i < boundaryEdges.Length; i++)
            {
                int edgeIdx = boundaryEdges[i];
                int sign = boundaryOrientations[i];

                for (int a = 0; a < dimG; a++)
                {
                    dOmega[a] += sign * omega.Coefficients[edgeIdx * dimG + a];
                }
            }

            // 2. Compute (1/2)[omega, omega] on this face
            //    For a 2-face with boundary edges e0, e1, e2:
            //    [omega, omega] = sum over oriented edge pairs contributing to the face
            //    Discrete approximation: use the boundary edges with their orientations
            var wedgeTerm = new double[dimG];

            // For each pair of boundary edges, compute the bracket contribution
            for (int i = 0; i < boundaryEdges.Length; i++)
            {
                for (int j = i + 1; j < boundaryEdges.Length; j++)
                {
                    int ei = boundaryEdges[i];
                    int ej = boundaryEdges[j];
                    int si = boundaryOrientations[i];
                    int sj = boundaryOrientations[j];

                    var omegaI = omega.GetEdgeValueArray(ei);
                    var omegaJ = omega.GetEdgeValueArray(ej);

                    // Scale by orientations
                    for (int a = 0; a < dimG; a++)
                    {
                        omegaI[a] *= si;
                        omegaJ[a] *= sj;
                    }

                    var bracket = algebra.Bracket(omegaI, omegaJ);

                    for (int a = 0; a < dimG; a++)
                    {
                        wedgeTerm[a] += bracket[a];
                    }
                }
            }

            // F = d(omega) + (1/2)[omega, omega]
            for (int a = 0; a < dimG; a++)
            {
                coefficients[fi * dimG + a] = dOmega[a] + 0.5 * wedgeTerm[a];
            }
        }

        return new CurvatureField(mesh, algebra, coefficients);
    }
}

/// <summary>
/// Represents discrete curvature F_h as an ad(P)-valued 2-form on Y_h.
/// Stored as Lie-algebra coefficients per face of the ambient mesh.
/// </summary>
public sealed class CurvatureField
{
    /// <summary>The ambient mesh Y_h.</summary>
    public SimplicialMesh Mesh { get; }

    /// <summary>The Lie algebra.</summary>
    public LieAlgebra Algebra { get; }

    /// <summary>
    /// Coefficient data: flat array of length FaceCount * Algebra.Dimension.
    /// Indexed as [faceIdx * dim(g) + algebraIdx].
    /// </summary>
    public double[] Coefficients { get; }

    /// <summary>Number of faces.</summary>
    public int FaceCount => Mesh.FaceCount;

    /// <summary>Dimension of the Lie algebra.</summary>
    public int AlgebraDimension => Algebra.Dimension;

    public CurvatureField(SimplicialMesh mesh, LieAlgebra algebra, double[] coefficients)
    {
        Mesh = mesh ?? throw new ArgumentNullException(nameof(mesh));
        Algebra = algebra ?? throw new ArgumentNullException(nameof(algebra));
        if (coefficients.Length != mesh.FaceCount * algebra.Dimension)
            throw new ArgumentException(
                $"Expected {mesh.FaceCount * algebra.Dimension} coefficients, got {coefficients.Length}.");
        Coefficients = coefficients;
    }

    /// <summary>
    /// Gets the curvature value on a given face as a new array.
    /// </summary>
    public double[] GetFaceValueArray(int faceIndex)
    {
        var result = new double[AlgebraDimension];
        Array.Copy(Coefficients, faceIndex * AlgebraDimension, result, 0, AlgebraDimension);
        return result;
    }

    /// <summary>
    /// Computes the L2 norm squared of the curvature: sum over faces of |F(face)|^2.
    /// Uses the Lie algebra invariant metric for the inner product.
    /// </summary>
    public double NormSquared()
    {
        double total = 0;
        for (int fi = 0; fi < FaceCount; fi++)
        {
            var faceVal = GetFaceValueArray(fi);
            total += Algebra.InnerProduct(faceVal, faceVal);
        }
        return total;
    }

    /// <summary>
    /// Converts to a FieldTensor with proper TensorSignature.
    /// </summary>
    public FieldTensor ToFieldTensor()
    {
        return new FieldTensor
        {
            Label = "F_h",
            Signature = new TensorSignature
            {
                AmbientSpaceId = "Y_h",
                CarrierType = "curvature-2form",
                Degree = "2",
                LieAlgebraBasisId = Algebra.BasisOrderId,
                ComponentOrderId = "face-major",
                NumericPrecision = "float64",
                MemoryLayout = "dense-row-major",
            },
            Coefficients = (double[])Coefficients.Clone(),
            Shape = new[] { FaceCount, AlgebraDimension },
        };
    }
}
