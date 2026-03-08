using Gu.Branching;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;

namespace Gu.ReferenceCpu;

/// <summary>
/// First-order curvature-derived Shiab branch operator (Section 4.8, IX-2).
///
/// The simplest Shiab operator: S_h = F_h (identity projection on curvature).
/// This is a projected curvature transform where the projection is the identity,
/// serving as the minimal-compatible first-order branch.
///
/// For the full Shiab operator, this would involve contractions of curvature
/// with the metric and Hodge star. The identity projection is the simplest
/// choice that produces the correct carrier type.
///
/// Linearization uses the standard curvature linearization:
///   dF/domega * delta = d(delta) + [omega, delta]
/// which is the covariant exterior derivative D_omega(delta).
/// </summary>
public sealed class FirstOrderShiabOperator : IShiabBranchOperator
{
    private readonly SimplicialMesh _mesh;
    private readonly LieAlgebra _algebra;

    public FirstOrderShiabOperator(SimplicialMesh mesh, LieAlgebra algebra)
    {
        _mesh = mesh ?? throw new ArgumentNullException(nameof(mesh));
        _algebra = algebra ?? throw new ArgumentNullException(nameof(algebra));
    }

    /// <inheritdoc />
    public string BranchId => "first-order-curvature";

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
    /// Evaluates S_h = F_h (identity projection on curvature).
    /// Returns a copy of the curvature tensor with the correct Shiab label
    /// and carrier type.
    /// </summary>
    public FieldTensor Evaluate(
        FieldTensor curvatureF,
        FieldTensor omega,
        BranchManifest manifest,
        GeometryContext geometry)
    {
        return new FieldTensor
        {
            Label = "S_h",
            Signature = OutputSignature,
            Coefficients = (double[])curvatureF.Coefficients.Clone(),
            Shape = curvatureF.Shape,
        };
    }

    /// <summary>
    /// Linearization of S_h = F_h with respect to omega in direction delta_omega.
    ///
    /// Since S = F and F = d(omega) + (1/2)[omega, omega]:
    ///   dF/domega * delta = d(delta) + [omega, delta]
    ///
    /// This is the covariant exterior derivative D_omega(delta).
    ///
    /// On each face with boundary edges (e_0, e_1, e_2) and orientations (s_0, s_1, s_2):
    ///   d(delta)[face] = sum_i s_i * delta(e_i)
    ///   bracket term = (1/2) * sum_{i &lt; j} (
    ///       [s_i * omega(e_i), s_j * delta(e_j)] + [s_i * delta(e_i), s_j * omega(e_j)]
    ///   )
    ///   dF = d(delta) + bracket term
    /// </summary>
    public FieldTensor Linearize(
        FieldTensor curvatureF,
        FieldTensor omega,
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

            // 1. d(delta) on this face: boundary operator applied to perturbation
            var dDelta = new double[dimG];
            for (int i = 0; i < boundaryEdges.Length; i++)
            {
                int edgeIdx = boundaryEdges[i];
                int sign = boundaryOrientations[i];
                for (int a = 0; a < dimG; a++)
                {
                    dDelta[a] += sign * deltaOmega.Coefficients[edgeIdx * dimG + a];
                }
            }

            // 2. Linearization of (1/2) * sum_{i<j} [omega_i, omega_j] on this face
            //    Product rule produces two cross-terms, each carrying the 1/2 factor.
            var bracketTerm = new double[dimG];
            for (int i = 0; i < boundaryEdges.Length; i++)
            {
                for (int j = i + 1; j < boundaryEdges.Length; j++)
                {
                    int ei = boundaryEdges[i];
                    int ej = boundaryEdges[j];
                    int si = boundaryOrientations[i];
                    int sj = boundaryOrientations[j];

                    var omegaI = new double[dimG];
                    var omegaJ = new double[dimG];
                    var deltaI = new double[dimG];
                    var deltaJ = new double[dimG];

                    for (int a = 0; a < dimG; a++)
                    {
                        omegaI[a] = si * omega.Coefficients[ei * dimG + a];
                        omegaJ[a] = sj * omega.Coefficients[ej * dimG + a];
                        deltaI[a] = si * deltaOmega.Coefficients[ei * dimG + a];
                        deltaJ[a] = sj * deltaOmega.Coefficients[ej * dimG + a];
                    }

                    // Linearization of (1/2) * sum_{i<j} [s_i*omega_i, s_j*omega_j]:
                    // d/deps [(1/2) [s_i*(omega_i+eps*delta_i), s_j*(omega_j+eps*delta_j)]]|_{eps=0}
                    // = (1/2) * ([s_i*omega_i, s_j*delta_j] + [s_i*delta_i, s_j*omega_j])
                    // The 1/2 factor is retained because we sum over ordered pairs (i < j).
                    var bracket1 = _algebra.Bracket(omegaI, deltaJ);
                    var bracket2 = _algebra.Bracket(deltaI, omegaJ);

                    for (int a = 0; a < dimG; a++)
                    {
                        bracketTerm[a] += 0.5 * (bracket1[a] + bracket2[a]);
                    }
                }
            }

            // D_omega(delta) = d(delta) + [omega, delta]
            for (int a = 0; a < dimG; a++)
            {
                result[fi * dimG + a] = dDelta[a] + bracketTerm[a];
            }
        }

        return new FieldTensor
        {
            Label = "dS_h",
            Signature = OutputSignature,
            Coefficients = result,
            Shape = new[] { faceCount, dimG },
        };
    }
}
