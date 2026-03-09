using Gu.Branching;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.Solvers;

namespace Gu.Phase2.Stability;

/// <summary>
/// Coulomb slice operator C_* as an ILinearOperator.
///
/// C_*(delta_u) = d_{A0}^*(delta_u)  (covariant codifferential: edges -> vertices)
/// C_*^T(phi)   = d_{A0}(phi)        (covariant exterior derivative: vertices -> edges)
///
/// The composition C_*^T C_* = d_{A0} d_{A0}^* is the gauge Laplacian on 1-forms.
///
/// The covariant codifferential includes the bracket correction:
///   d_{A0}^*(beta)_a^v = d^*(beta)_a^v - sum_{e~v} orient(v,e) * [A0_e, beta_e]_a
///
/// When no background connection is provided (A0 = null or zero), this reduces
/// to the flat codifferential d^*.
///
/// Key identity: C_* R_{z_*} = d_{A0}^*(-d_{A0}) = -Delta_gauge (on 0-forms).
/// This should be invertible on the complement of constant gauge transformations.
/// </summary>
public sealed class CoulombSliceOperator : IGaugeConstraintOperator
{
    private readonly CoulombGaugePenalty _gauge;
    private readonly SimplicialMesh _mesh;
    private readonly int _dimG;
    private readonly double[]? _a0;
    private readonly double[]? _structureConstants;
    private readonly int _edgeDim;
    private readonly int _vertexDim;
    private readonly TensorSignature _inputSig;
    private readonly TensorSignature _outputSig;

    /// <inheritdoc />
    public string GaugeHandlingMode => "coulomb";

    /// <summary>
    /// Create a Coulomb slice operator.
    /// </summary>
    /// <param name="gauge">Coulomb gauge penalty (provides d^* and d operators).</param>
    /// <param name="mesh">Simplicial mesh.</param>
    /// <param name="dimG">Lie algebra dimension.</param>
    /// <param name="basisId">Lie algebra basis ID for tensor signatures.</param>
    /// <param name="algebra">Lie algebra (required for covariant codifferential with non-zero A0).</param>
    /// <param name="backgroundConnection">Background connection A0, edge-valued with length EdgeCount * dimG. Null for flat codifferential.</param>
    public CoulombSliceOperator(
        CoulombGaugePenalty gauge,
        SimplicialMesh mesh,
        int dimG,
        string basisId = "standard",
        LieAlgebra? algebra = null,
        double[]? backgroundConnection = null)
    {
        _gauge = gauge ?? throw new ArgumentNullException(nameof(gauge));
        _mesh = mesh;
        _dimG = dimG;
        _edgeDim = mesh.EdgeCount * dimG;
        _vertexDim = mesh.VertexCount * dimG;

        if (backgroundConnection != null)
        {
            if (algebra == null)
                throw new ArgumentException(
                    "LieAlgebra is required when a non-null background connection is provided.",
                    nameof(algebra));
            if (backgroundConnection.Length != _edgeDim)
                throw new ArgumentException(
                    $"backgroundConnection length {backgroundConnection.Length} does not match EdgeCount*dimG = {_edgeDim}.",
                    nameof(backgroundConnection));
            _a0 = (double[])backgroundConnection.Clone();
            _structureConstants = algebra.StructureConstants;
        }

        _inputSig = new TensorSignature
        {
            AmbientSpaceId = "Y_h",
            CarrierType = "connection-1form",
            Degree = "1",
            LieAlgebraBasisId = basisId,
            ComponentOrderId = "edge-major",
            NumericPrecision = "float64",
            MemoryLayout = "dense-row-major",
        };

        _outputSig = new TensorSignature
        {
            AmbientSpaceId = "Y_h",
            CarrierType = "gauge-violation-0form",
            Degree = "0",
            LieAlgebraBasisId = basisId,
            ComponentOrderId = "vertex-major",
            NumericPrecision = "float64",
            MemoryLayout = "dense-row-major",
        };
    }

    public TensorSignature InputSignature => _inputSig;
    public TensorSignature OutputSignature => _outputSig;
    public int InputDimension => _edgeDim;
    public int OutputDimension => _vertexDim;

    /// <summary>
    /// Forward action: C_*(beta) = d_{A0}^*(beta) (covariant codifferential, edges -> vertices).
    ///
    /// d_{A0}^*(beta)_a^v = d^*(beta)_a^v - sum_{e~v} orient(v,e) * [A0_e, beta_e]_a
    ///
    /// The bracket term uses structure constants f^a_{bc}:
    ///   [A0_e, beta_e]_a = sum_{b,c} f^a_{bc} * A0_b^e * beta_c^e
    /// </summary>
    public FieldTensor Apply(FieldTensor v)
    {
        var result = _gauge.ApplyCodifferential(v.Coefficients);

        if (_a0 != null && _structureConstants != null)
        {
            ApplyBracketCorrectionForward(v.Coefficients, result);
        }

        return new FieldTensor
        {
            Label = "C*v",
            Signature = _outputSig,
            Coefficients = result,
            Shape = new[] { _vertexDim },
        };
    }

    /// <summary>
    /// Transpose action: C_*^T(phi) = d_{A0}(phi) (covariant exterior derivative, vertices -> edges).
    ///
    /// The transpose of the bracket correction contributes:
    ///   B^T(phi)_c^e = -sum_{v~e} orient(v,e) * sum_{a,b} f^a_{bc} * A0_b^e * phi_a^v
    /// </summary>
    public FieldTensor ApplyTranspose(FieldTensor w)
    {
        var result = _gauge.ApplyExteriorDerivative(w.Coefficients);

        if (_a0 != null && _structureConstants != null)
        {
            ApplyBracketCorrectionTranspose(w.Coefficients, result);
        }

        return new FieldTensor
        {
            Label = "C^T*w",
            Signature = _inputSig,
            Coefficients = result,
            Shape = new[] { _edgeDim },
        };
    }

    /// <summary>
    /// Apply the bracket correction to the codifferential result (in-place addition).
    ///
    /// For each vertex v and algebra index a:
    ///   correction_a^v = -sum_{e~v} orient(v,e) * sum_{b,c} f^a_{bc} * A0_b^e * beta_c^e
    /// </summary>
    private void ApplyBracketCorrectionForward(double[] beta, double[] result)
    {
        int vertexCount = _mesh.VertexCount;
        int dim = _dimG;
        int dim2 = dim * dim;

        for (int v = 0; v < vertexCount; v++)
        {
            int[] incidentEdges = _mesh.VertexEdges[v];
            int[] orientations = _mesh.VertexEdgeOrientations[v];

            for (int i = 0; i < incidentEdges.Length; i++)
            {
                int edgeIdx = incidentEdges[i];
                int sign = orientations[i];

                for (int a = 0; a < dim; a++)
                {
                    double bracketA = 0;
                    for (int b = 0; b < dim; b++)
                    {
                        double a0b = _a0![edgeIdx * dim + b];
                        if (a0b == 0) continue;
                        for (int c = 0; c < dim; c++)
                        {
                            // f^a_{bc} at index [b * dim^2 + c * dim + a]
                            double fabc = _structureConstants![b * dim2 + c * dim + a];
                            if (fabc == 0) continue;
                            bracketA += fabc * a0b * beta[edgeIdx * dim + c];
                        }
                    }
                    result[v * dim + a] -= sign * bracketA;
                }
            }
        }
    }

    /// <summary>
    /// Apply the transpose of the bracket correction (in-place addition).
    ///
    /// For each edge e and algebra index c:
    ///   correction_c^e = -sum_{v~e} orient(v,e) * sum_{a,b} f^a_{bc} * A0_b^e * phi_a^v
    /// </summary>
    private void ApplyBracketCorrectionTranspose(double[] phi, double[] result)
    {
        int vertexCount = _mesh.VertexCount;
        int dim = _dimG;
        int dim2 = dim * dim;

        for (int v = 0; v < vertexCount; v++)
        {
            int[] incidentEdges = _mesh.VertexEdges[v];
            int[] orientations = _mesh.VertexEdgeOrientations[v];

            for (int i = 0; i < incidentEdges.Length; i++)
            {
                int edgeIdx = incidentEdges[i];
                int sign = orientations[i];

                for (int c = 0; c < dim; c++)
                {
                    double correction = 0;
                    for (int b = 0; b < dim; b++)
                    {
                        double a0b = _a0![edgeIdx * dim + b];
                        if (a0b == 0) continue;
                        for (int a = 0; a < dim; a++)
                        {
                            // f^a_{bc} at index [b * dim^2 + c * dim + a]
                            double fabc = _structureConstants![b * dim2 + c * dim + a];
                            if (fabc == 0) continue;
                            correction += fabc * a0b * phi[v * dim + a];
                        }
                    }
                    result[edgeIdx * dim + c] -= sign * correction;
                }
            }
        }
    }
}
