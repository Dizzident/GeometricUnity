using Gu.Branching;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;

namespace Gu.ReferenceCpu;

/// <summary>
/// Local algebraic torsion branch operator (Section 4.7, IX-1).
///
/// Computes a torsion-like term from the bi-connection pair (A, B) = (A0 + omega, A0 - omega).
/// For each face, the torsion is assembled as a bracket of the oriented bi-connection values
/// on the boundary edges:
///
///   T_h[face] = sum_{i &lt; j} sign_i * sign_j * [A(edge_i), B(edge_j)]
///
/// This produces an ad(P)-valued 2-form (same carrier as curvature F_h).
///
/// The bracket [A_edge, B_edge] captures the algebraic interaction between the two
/// connections of the bi-connection pair. When A0 = 0, A = omega and B = -omega, so
/// this reduces to -[omega_i, omega_j] (up to orientations), producing a curvature-like
/// correction term.
/// </summary>
public sealed class LocalAlgebraicTorsionOperator : ITorsionBranchOperator
{
    private readonly SimplicialMesh _mesh;
    private readonly LieAlgebra _algebra;

    public LocalAlgebraicTorsionOperator(SimplicialMesh mesh, LieAlgebra algebra)
    {
        _mesh = mesh ?? throw new ArgumentNullException(nameof(mesh));
        _algebra = algebra ?? throw new ArgumentNullException(nameof(algebra));
    }

    /// <inheritdoc />
    public string BranchId => "local-algebraic";

    /// <inheritdoc />
    public string OutputCarrierType => "curvature-2form";

    public TensorSignature OutputSignature => new()
    {
        AmbientSpaceId = "Y_h",
        CarrierType = OutputCarrierType,
        Degree = "2",
        LieAlgebraBasisId = _algebra.BasisOrderId,
        ComponentOrderId = "face-major",
        NumericPrecision = "float64",
        MemoryLayout = "dense-row-major",
    };

    /// <summary>
    /// Evaluates the torsion branch T_h.
    ///
    /// For each face with boundary edges (e_0, e_1, e_2) and orientations (s_0, s_1, s_2):
    ///   T_h[face] = sum_{i &lt; j} s_i * s_j * [A(e_i), B(e_j)]
    ///
    /// where A = A0 + omega and B = A0 - omega are the bi-connection pair.
    /// </summary>
    public FieldTensor Evaluate(
        FieldTensor omega,
        FieldTensor a0,
        BranchManifest manifest,
        GeometryContext geometry)
    {
        int dimG = _algebra.Dimension;
        int faceCount = _mesh.FaceCount;
        var result = new double[faceCount * dimG];

        // Build bi-connection values: A = A0 + omega, B = A0 - omega (per edge)
        // omega and a0 are edge-major FieldTensors with shape [edgeCount, dimG]
        for (int fi = 0; fi < faceCount; fi++)
        {
            var boundaryEdges = _mesh.FaceBoundaryEdges[fi];
            var boundaryOrientations = _mesh.FaceBoundaryOrientations[fi];

            var torsionFace = new double[dimG];

            for (int i = 0; i < boundaryEdges.Length; i++)
            {
                for (int j = i + 1; j < boundaryEdges.Length; j++)
                {
                    int ei = boundaryEdges[i];
                    int ej = boundaryEdges[j];
                    int si = boundaryOrientations[i];
                    int sj = boundaryOrientations[j];

                    // A(e_i) = A0(e_i) + omega(e_i), oriented by si
                    var aI = new double[dimG];
                    // B(e_j) = A0(e_j) - omega(e_j), oriented by sj
                    var bJ = new double[dimG];

                    for (int k = 0; k < dimG; k++)
                    {
                        double omegaIk = omega.Coefficients[ei * dimG + k];
                        double a0Ik = a0.Coefficients[ei * dimG + k];
                        aI[k] = si * (a0Ik + omegaIk);

                        double omegaJk = omega.Coefficients[ej * dimG + k];
                        double a0Jk = a0.Coefficients[ej * dimG + k];
                        bJ[k] = sj * (a0Jk - omegaJk);
                    }

                    var bracket = _algebra.Bracket(aI, bJ);

                    for (int k = 0; k < dimG; k++)
                    {
                        torsionFace[k] += bracket[k];
                    }
                }
            }

            for (int k = 0; k < dimG; k++)
            {
                result[fi * dimG + k] = torsionFace[k];
            }
        }

        return new FieldTensor
        {
            Label = "T_h",
            Signature = OutputSignature,
            Coefficients = result,
            Shape = new[] { faceCount, dimG },
        };
    }

    /// <summary>
    /// Linearization of T_h with respect to omega in direction delta_omega.
    ///
    /// Since A = A0 + omega, B = A0 - omega:
    ///   dA/domega = +Id, dB/domega = -Id
    ///
    /// For each face:
    ///   dT/domega * delta = sum_{i &lt; j} s_i * s_j * (
    ///       [delta(e_i), B(e_j)] + [A(e_i), -delta(e_j)]
    ///   )
    ///   = sum_{i &lt; j} s_i * s_j * ([delta(e_i), B(e_j)] - [A(e_i), delta(e_j)])
    /// </summary>
    public FieldTensor Linearize(
        FieldTensor omega,
        FieldTensor a0,
        FieldTensor deltaOmega,
        BranchManifest manifest,
        GeometryContext geometry)
    {
        int dimG = _algebra.Dimension;
        int faceCount = _mesh.FaceCount;
        var result = new double[faceCount * dimG];

        for (int fi = 0; fi < faceCount; fi++)
        {
            var boundaryEdges = _mesh.FaceBoundaryEdges[fi];
            var boundaryOrientations = _mesh.FaceBoundaryOrientations[fi];

            var linearizedFace = new double[dimG];

            for (int i = 0; i < boundaryEdges.Length; i++)
            {
                for (int j = i + 1; j < boundaryEdges.Length; j++)
                {
                    int ei = boundaryEdges[i];
                    int ej = boundaryEdges[j];
                    int si = boundaryOrientations[i];
                    int sj = boundaryOrientations[j];

                    // Build oriented connection values
                    var aI = new double[dimG];
                    var bJ = new double[dimG];
                    var deltaI = new double[dimG];
                    var deltaJ = new double[dimG];

                    for (int k = 0; k < dimG; k++)
                    {
                        aI[k] = si * (a0.Coefficients[ei * dimG + k] + omega.Coefficients[ei * dimG + k]);
                        bJ[k] = sj * (a0.Coefficients[ej * dimG + k] - omega.Coefficients[ej * dimG + k]);
                        deltaI[k] = si * deltaOmega.Coefficients[ei * dimG + k];
                        deltaJ[k] = sj * deltaOmega.Coefficients[ej * dimG + k];
                    }

                    // dT contribution: [delta_i, B_j] + [A_i, -delta_j]
                    // = [delta_i, B_j] - [A_i, delta_j]
                    var bracket1 = _algebra.Bracket(deltaI, bJ);
                    var bracket2 = _algebra.Bracket(aI, deltaJ);

                    for (int k = 0; k < dimG; k++)
                    {
                        linearizedFace[k] += bracket1[k] - bracket2[k];
                    }
                }
            }

            for (int k = 0; k < dimG; k++)
            {
                result[fi * dimG + k] = linearizedFace[k];
            }
        }

        return new FieldTensor
        {
            Label = "dT_h",
            Signature = OutputSignature,
            Coefficients = result,
            Shape = new[] { faceCount, dimG },
        };
    }
}
