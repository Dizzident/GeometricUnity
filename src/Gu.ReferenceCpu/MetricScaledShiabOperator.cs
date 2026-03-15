using Gu.Branching;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;

namespace Gu.ReferenceCpu;

/// <summary>
/// Metric-scaled Shiab branch operator: S_h = lambda * F_h.
///
/// A scalar-scaled variant of the identity Shiab, numerically distinct from S=F
/// (identity-shiab, first-order-curvature) whenever lambda != 1. Used for
/// Shiab family exploration in P11-M9 three-operator scope checks.
///
/// On the current toy geometry (dimX=2), Lambda^2(T*X) is 1-dimensional, so scalar
/// scaling is the only available distinguishing Shiab variant. Richer contractions
/// (Ricci/Weyl decompositions) require dimX >= 4 per physicist guidance.
///
/// Linearization: dS/domega = lambda * dF/domega = lambda * D_omega.
/// </summary>
public sealed class MetricScaledShiabOperator : IShiabBranchOperator
{
    private readonly SimplicialMesh _mesh;
    private readonly LieAlgebra _algebra;
    private readonly double _lambda;

    public MetricScaledShiabOperator(SimplicialMesh mesh, LieAlgebra algebra, double lambda)
    {
        _mesh = mesh ?? throw new ArgumentNullException(nameof(mesh));
        _algebra = algebra ?? throw new ArgumentNullException(nameof(algebra));
        _lambda = lambda;
    }

    /// <summary>
    /// Construct from a BranchManifest, reading lambda from Parameters["metricScale"].
    /// Defaults to lambda=1.0 if the key is absent.
    /// </summary>
    public MetricScaledShiabOperator(SimplicialMesh mesh, LieAlgebra algebra, BranchManifest manifest)
    {
        _mesh = mesh ?? throw new ArgumentNullException(nameof(mesh));
        _algebra = algebra ?? throw new ArgumentNullException(nameof(algebra));
        ArgumentNullException.ThrowIfNull(manifest);
        _lambda = manifest.Parameters != null &&
                  manifest.Parameters.TryGetValue("metricScale", out var val) &&
                  double.TryParse(val, System.Globalization.NumberStyles.Float,
                      System.Globalization.CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : 1.0;
    }

    public double Lambda => _lambda;

    public string BranchId => $"metric-scaled-shiab";

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
    /// S_h = lambda * F_h: returns lambda times the curvature coefficients.
    /// </summary>
    public FieldTensor Evaluate(
        FieldTensor curvatureF,
        FieldTensor omega,
        BranchManifest manifest,
        GeometryContext geometry)
    {
        var coeffs = (double[])curvatureF.Coefficients.Clone();
        for (int i = 0; i < coeffs.Length; i++)
            coeffs[i] *= _lambda;

        return new FieldTensor
        {
            Label = "S_h",
            Signature = OutputSignature,
            Coefficients = coeffs,
            Shape = curvatureF.Shape,
        };
    }

    /// <summary>
    /// Linearization: dS/domega = lambda * dF/domega = lambda * D_omega(delta).
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

            var dDelta = new double[dimG];
            for (int i = 0; i < boundaryEdges.Length; i++)
            {
                int edgeIdx = boundaryEdges[i];
                int sign = boundaryOrientations[i];
                for (int a = 0; a < dimG; a++)
                    dDelta[a] += sign * deltaOmega.Coefficients[edgeIdx * dimG + a];
            }

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

                    var bracket1 = _algebra.Bracket(omegaI, deltaJ);
                    var bracket2 = _algebra.Bracket(deltaI, omegaJ);

                    for (int a = 0; a < dimG; a++)
                        bracketTerm[a] += 0.5 * (bracket1[a] + bracket2[a]);
                }
            }

            for (int a = 0; a < dimG; a++)
                result[fi * dimG + a] = _lambda * (dDelta[a] + bracketTerm[a]);
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
