using Gu.Branching;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;

namespace Gu.ReferenceCpu;

/// <summary>
/// Identity Shiab operator: S = F (just returns curvature).
/// Simplest possible Shiab -- skips all contractions/projections.
/// Combined with TrivialTorsionCpu: Upsilon = F - 0 = F.
/// Linearization: dS/domega = dF/domega = D_omega (covariant exterior derivative).
/// </summary>
public sealed class IdentityShiabCpu : IShiabBranchOperator
{
    private readonly SimplicialMesh _mesh;
    private readonly LieAlgebra _algebra;

    public IdentityShiabCpu(SimplicialMesh mesh, LieAlgebra algebra)
    {
        _mesh = mesh ?? throw new ArgumentNullException(nameof(mesh));
        _algebra = algebra ?? throw new ArgumentNullException(nameof(algebra));
    }

    public string BranchId => "identity-shiab";

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
    /// S = F: returns the curvature with updated label and carrier type.
    /// The curvature's signature is remapped to the common residual carrier type.
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
    /// Linearization: dS/domega = dF/domega = D_omega(delta_omega).
    /// D_omega(delta) = d(delta) + [omega, delta] on each face.
    /// This is the standard curvature linearization.
    /// </summary>
    public FieldTensor Linearize(
        FieldTensor curvatureF,
        FieldTensor omega,
        FieldTensor deltaOmega,
        BranchManifest manifest,
        GeometryContext geometry)
    {
        int faceCount = _mesh.FaceCount;
        int dimG = _algebra.Dimension;
        var result = new double[faceCount * dimG];

        for (int fi = 0; fi < faceCount; fi++)
        {
            // 1. d(delta_omega) on this face: boundary operator applied to perturbation
            var dDelta = new double[dimG];
            var boundaryEdges = _mesh.FaceBoundaryEdges[fi];
            var boundaryOrientations = _mesh.FaceBoundaryOrientations[fi];

            for (int i = 0; i < boundaryEdges.Length; i++)
            {
                int edgeIdx = boundaryEdges[i];
                int sign = boundaryOrientations[i];
                for (int a = 0; a < dimG; a++)
                {
                    dDelta[a] += sign * deltaOmega.Coefficients[edgeIdx * dimG + a];
                }
            }

            // 2. [omega, delta_omega] on this face: sum over edge pairs
            var bracketTerm = new double[dimG];
            for (int i = 0; i < boundaryEdges.Length; i++)
            {
                for (int j = i + 1; j < boundaryEdges.Length; j++)
                {
                    int ei = boundaryEdges[i];
                    int ej = boundaryEdges[j];
                    int si = boundaryOrientations[i];
                    int sj = boundaryOrientations[j];

                    // [omega_i, delta_j] + [delta_i, omega_j]
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
