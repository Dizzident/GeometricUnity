using Gu.Branching;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;

namespace Gu.ReferenceCpu;

/// <summary>
/// Trivial torsion operator: T = 0.
/// Pipeline test baseline: combined with IdentityShiabCpu, gives Upsilon = F - 0 = F.
/// This validates the entire residual -> objective -> Jacobian -> solver chain
/// before adding real torsion operators.
/// </summary>
public sealed class TrivialTorsionCpu : ITorsionBranchOperator
{
    private readonly SimplicialMesh _mesh;
    private readonly LieAlgebra _algebra;

    public TrivialTorsionCpu(SimplicialMesh mesh, LieAlgebra algebra)
    {
        _mesh = mesh ?? throw new ArgumentNullException(nameof(mesh));
        _algebra = algebra ?? throw new ArgumentNullException(nameof(algebra));
    }

    public string BranchId => "trivial";

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
    /// Returns zero FieldTensor with correct residual TensorSignature.
    /// Shape: [FaceCount, dim(g)] matching curvature/Shiab output.
    /// </summary>
    public FieldTensor Evaluate(
        FieldTensor omega,
        FieldTensor a0,
        BranchManifest manifest,
        GeometryContext geometry)
    {
        return CreateZeroResidual("T_h");
    }

    /// <summary>
    /// Linearization: dT/domega = 0 (torsion is identically zero).
    /// </summary>
    public FieldTensor Linearize(
        FieldTensor omega,
        FieldTensor a0,
        FieldTensor deltaOmega,
        BranchManifest manifest,
        GeometryContext geometry)
    {
        return CreateZeroResidual("dT_h");
    }

    private FieldTensor CreateZeroResidual(string label)
    {
        int faceCount = _mesh.FaceCount;
        int dimG = _algebra.Dimension;

        return new FieldTensor
        {
            Label = label,
            Signature = OutputSignature,
            Coefficients = new double[faceCount * dimG],
            Shape = new[] { faceCount, dimG },
        };
    }
}
