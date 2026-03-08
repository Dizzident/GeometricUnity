using Gu.Branching;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;

namespace Gu.ReferenceCpu;

/// <summary>
/// Augmented torsion operator: T^aug = d_{A0}(omega - A0).
///
/// This is the covariant exterior derivative of the connection difference
/// alpha = omega - A0, where A0 is the background connection and omega is
/// the dynamical connection.
///
/// T^aug_h[face f] = d(alpha)[f] + [A0 wedge alpha][f]
///
/// where:
///   d(alpha)[f] = sum over boundary edges e of (sign_e * alpha_h[e])
///   [A0 wedge alpha][f] = sum over boundary edge pairs (i&lt;j) of
///       [s_i*A0_i, s_j*alpha_j] + [s_i*alpha_i, s_j*A0_j]
///
/// Key properties:
/// - Linear in omega (A0 is fixed), so the Jacobian dT/domega is constant
/// - Vanishes when omega = A0 (no displacement = no torsion)
/// - Lands in ad(P)-valued 2-forms (same carrier as F_h)
/// - Gauge-equivariant by construction (d_{A0} is a covariant operator)
/// </summary>
public sealed class AugmentedTorsionCpu : ITorsionBranchOperator
{
    private readonly SimplicialMesh _mesh;
    private readonly LieAlgebra _algebra;

    public AugmentedTorsionCpu(SimplicialMesh mesh, LieAlgebra algebra)
    {
        _mesh = mesh ?? throw new ArgumentNullException(nameof(mesh));
        _algebra = algebra ?? throw new ArgumentNullException(nameof(algebra));
    }

    public string BranchId => "augmented-torsion";

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
    /// Evaluate T^aug = d_{A0}(omega - A0).
    /// </summary>
    public FieldTensor Evaluate(
        FieldTensor omega,
        FieldTensor a0,
        BranchManifest manifest,
        GeometryContext geometry)
    {
        int faceCount = _mesh.FaceCount;
        int edgeCount = _mesh.EdgeCount;
        int dimG = _algebra.Dimension;

        // Compute alpha = omega - A0 (edge-by-edge)
        var alpha = new double[edgeCount * dimG];
        for (int i = 0; i < alpha.Length; i++)
            alpha[i] = omega.Coefficients[i] - a0.Coefficients[i];

        // Compute d_{A0}(alpha) on each face
        var result = ComputeCovariantExteriorDerivative(a0.Coefficients, alpha, faceCount, dimG);

        return new FieldTensor
        {
            Label = "T_h",
            Signature = OutputSignature,
            Coefficients = result,
            Shape = new[] { faceCount, dimG },
        };
    }

    /// <summary>
    /// Linearization: dT/domega(delta) = d_{A0}(delta).
    /// Since T^aug is linear in omega (A0 is fixed), the linearization
    /// is the same covariant exterior derivative applied to the perturbation.
    /// The Jacobian dT/domega is constant (independent of omega).
    /// </summary>
    public FieldTensor Linearize(
        FieldTensor omega,
        FieldTensor a0,
        FieldTensor deltaOmega,
        BranchManifest manifest,
        GeometryContext geometry)
    {
        int faceCount = _mesh.FaceCount;
        int dimG = _algebra.Dimension;

        // dT/domega(delta) = d_{A0}(delta)
        var result = ComputeCovariantExteriorDerivative(
            a0.Coefficients, deltaOmega.Coefficients, faceCount, dimG);

        return new FieldTensor
        {
            Label = "dT_h",
            Signature = OutputSignature,
            Coefficients = result,
            Shape = new[] { faceCount, dimG },
        };
    }

    /// <summary>
    /// Compute d_{A0}(beta) = d(beta) + [A0 wedge beta] on each face.
    /// </summary>
    private double[] ComputeCovariantExteriorDerivative(
        double[] a0Coefficients, double[] betaCoefficients,
        int faceCount, int dimG)
    {
        var result = new double[faceCount * dimG];

        for (int fi = 0; fi < faceCount; fi++)
        {
            // 1. d(beta) on this face: sum over boundary edges
            var dBeta = new double[dimG];
            var boundaryEdges = _mesh.FaceBoundaryEdges[fi];
            var boundaryOrientations = _mesh.FaceBoundaryOrientations[fi];

            for (int i = 0; i < boundaryEdges.Length; i++)
            {
                int edgeIdx = boundaryEdges[i];
                int sign = boundaryOrientations[i];
                for (int a = 0; a < dimG; a++)
                {
                    dBeta[a] += sign * betaCoefficients[edgeIdx * dimG + a];
                }
            }

            // 2. [A0 wedge beta] on this face: sum over boundary edge pairs
            var bracketTerm = new double[dimG];
            for (int i = 0; i < boundaryEdges.Length; i++)
            {
                for (int j = i + 1; j < boundaryEdges.Length; j++)
                {
                    int ei = boundaryEdges[i];
                    int ej = boundaryEdges[j];
                    int si = boundaryOrientations[i];
                    int sj = boundaryOrientations[j];

                    // Extract oriented edge values
                    var a0I = new double[dimG];
                    var a0J = new double[dimG];
                    var betaI = new double[dimG];
                    var betaJ = new double[dimG];

                    for (int a = 0; a < dimG; a++)
                    {
                        a0I[a] = si * a0Coefficients[ei * dimG + a];
                        a0J[a] = sj * a0Coefficients[ej * dimG + a];
                        betaI[a] = si * betaCoefficients[ei * dimG + a];
                        betaJ[a] = sj * betaCoefficients[ej * dimG + a];
                    }

                    // [A0 wedge beta]: cross terms
                    // [A0_i, beta_j] + [beta_i, A0_j]
                    var bracket1 = _algebra.Bracket(a0I, betaJ);
                    var bracket2 = _algebra.Bracket(betaI, a0J);

                    for (int a = 0; a < dimG; a++)
                    {
                        bracketTerm[a] += bracket1[a] + bracket2[a];
                    }
                }
            }

            // d_{A0}(beta) = d(beta) + [A0 wedge beta]
            for (int a = 0; a < dimG; a++)
            {
                result[fi * dimG + a] = dBeta[a] + bracketTerm[a];
            }
        }

        return result;
    }

}
