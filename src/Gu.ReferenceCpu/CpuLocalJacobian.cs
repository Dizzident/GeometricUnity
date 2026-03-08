using Gu.Branching;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;

namespace Gu.ReferenceCpu;

/// <summary>
/// Matrix-free Jacobian J_h = dUpsilon_h/domega_h.
/// Uses the branch operators' Linearize methods to compute J*v.
///
/// J_h = dS/domega - dT/domega
///
/// For trivial torsion + identity Shiab: J = D_omega (curvature linearization).
/// </summary>
public sealed class CpuLocalJacobian : ILinearOperator
{
    private readonly IShiabBranchOperator _shiab;
    private readonly ITorsionBranchOperator _torsion;
    private readonly FieldTensor _omega;
    private readonly FieldTensor _a0;
    private readonly FieldTensor _curvatureF;
    private readonly BranchManifest _manifest;
    private readonly GeometryContext _geometry;
    private readonly SimplicialMesh _mesh;
    private readonly LieAlgebra _algebra;
    private readonly string _outputCarrierType;

    public CpuLocalJacobian(
        IShiabBranchOperator shiab,
        ITorsionBranchOperator torsion,
        FieldTensor omega,
        FieldTensor a0,
        FieldTensor curvatureF,
        BranchManifest manifest,
        GeometryContext geometry,
        SimplicialMesh mesh,
        LieAlgebra algebra)
    {
        _shiab = shiab;
        _torsion = torsion;
        _omega = omega;
        _a0 = a0;
        _curvatureF = curvatureF;
        _manifest = manifest;
        _geometry = geometry;
        _mesh = mesh;
        _algebra = algebra;
        _outputCarrierType = shiab.OutputCarrierType;
    }

    public TensorSignature InputSignature => _omega.Signature;

    public TensorSignature OutputSignature => new()
    {
        AmbientSpaceId = "Y_h",
        CarrierType = _outputCarrierType,
        Degree = "2",
        LieAlgebraBasisId = _algebra.BasisOrderId,
        ComponentOrderId = "face-major",
        NumericPrecision = "float64",
        MemoryLayout = "dense-row-major",
    };

    public int InputDimension => _omega.Coefficients.Length;

    public int OutputDimension => _mesh.FaceCount * _algebra.Dimension;

    /// <summary>
    /// Forward action: J * v = dS/domega(v) - dT/domega(v).
    /// Delegates to branch operator Linearize methods.
    /// </summary>
    public FieldTensor Apply(FieldTensor v)
    {
        var dS = _shiab.Linearize(_curvatureF, _omega, v, _manifest, _geometry);
        var dT = _torsion.Linearize(_omega, _a0, v, _manifest, _geometry);
        return FieldTensorOps.Subtract(dS, dT);
    }

    /// <summary>
    /// Transpose action: J^T * v.
    /// Computed by assembling J^T column by column using basis vectors.
    /// This is O(n_input * n_output) -- acceptable for small problems.
    /// </summary>
    public FieldTensor ApplyTranspose(FieldTensor v)
    {
        int nIn = InputDimension;
        int nOut = OutputDimension;
        var result = new double[nIn];

        // J^T * v = sum_i v_i * (row i of J) = sum_i v_i * J(e_i)
        // Actually J^T * v has components (J^T v)_j = sum_i J_{ij} v_i
        // We compute this by: for each input DOF j, compute J * e_j (column j of J),
        // then dot with v.
        var ej = new double[nIn];
        for (int j = 0; j < nIn; j++)
        {
            ej[j] = 1.0;
            var jEj = Apply(new FieldTensor
            {
                Label = "e_j",
                Signature = _omega.Signature,
                Coefficients = (double[])ej.Clone(),
                Shape = _omega.Shape,
            });

            // (J^T v)_j = dot(J*e_j, v)
            double dot = 0;
            for (int i = 0; i < nOut; i++)
                dot += jEj.Coefficients[i] * v.Coefficients[i];
            result[j] = dot;

            ej[j] = 0.0;
        }

        return new FieldTensor
        {
            Label = "J^T*v",
            Signature = _omega.Signature,
            Coefficients = result,
            Shape = _omega.Shape,
        };
    }

    /// <summary>
    /// Compute gradient: G = J^T * M * Upsilon.
    /// </summary>
    public FieldTensor ComputeGradient(FieldTensor upsilon, CpuMassMatrix massMatrix)
    {
        var mUpsilon = massMatrix.Apply(upsilon);
        return ApplyTranspose(mUpsilon);
    }
}
