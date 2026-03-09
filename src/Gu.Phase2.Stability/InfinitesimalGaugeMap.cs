using Gu.Branching;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.ReferenceCpu;
using Gu.Solvers;

namespace Gu.Phase2.Stability;

/// <summary>
/// Infinitesimal gauge map R_{z_*} from gauge parameters to connection perturbations.
///
/// R_{z_*}(xi) = -d_{A0}(xi) + [A_* - A0, xi]
///
/// where:
/// - xi is a Lie-algebra-valued 0-form on vertices (gauge parameter)
/// - d_{A0}(xi) = d(xi) + [A0 wedge xi] is the covariant exterior derivative
/// - A_* is the background connection
/// - A0 is the distinguished connection
/// - [A_* - A0, xi] is the bracket of the difference 1-form with xi
///
/// For the simplest case A_* = A0, this reduces to R(xi) = -d(xi) (pure exterior derivative).
///
/// The image of R spans the gauge directions in the connection space.
/// The null space of J restricted to image(R) reveals gauge redundancy.
/// </summary>
public sealed class InfinitesimalGaugeMap : ILinearOperator
{
    private readonly SimplicialMesh _mesh;
    private readonly LieAlgebra _algebra;
    private readonly double[] _a0Coeffs;
    private readonly double[] _omegaStarCoeffs;
    private readonly CoulombGaugePenalty _dOperator;

    /// <summary>
    /// Create an infinitesimal gauge map.
    /// </summary>
    /// <param name="mesh">Simplicial mesh.</param>
    /// <param name="algebra">Lie algebra (provides bracket).</param>
    /// <param name="a0">Distinguished connection A0 (edge-valued, length EdgeCount*dimG).</param>
    /// <param name="omegaStar">Background connection omega_* (edge-valued, length EdgeCount*dimG).</param>
    public InfinitesimalGaugeMap(
        SimplicialMesh mesh,
        LieAlgebra algebra,
        FieldTensor a0,
        FieldTensor omegaStar)
    {
        _mesh = mesh ?? throw new ArgumentNullException(nameof(mesh));
        _algebra = algebra ?? throw new ArgumentNullException(nameof(algebra));

        int expectedEdgeLen = mesh.EdgeCount * algebra.Dimension;
        if (a0.Coefficients.Length != expectedEdgeLen)
            throw new ArgumentException(
                $"A0 length {a0.Coefficients.Length} != EdgeCount*dimG = {expectedEdgeLen}.");
        if (omegaStar.Coefficients.Length != expectedEdgeLen)
            throw new ArgumentException(
                $"omegaStar length {omegaStar.Coefficients.Length} != EdgeCount*dimG = {expectedEdgeLen}.");

        _a0Coeffs = (double[])a0.Coefficients.Clone();
        _omegaStarCoeffs = (double[])omegaStar.Coefficients.Clone();

        // We reuse CoulombGaugePenalty's d operator for the exterior derivative
        _dOperator = new CoulombGaugePenalty(mesh, algebra.Dimension, 0.0);
    }

    /// <summary>Input: vertex-valued gauge parameters (VertexCount * dimG).</summary>
    public TensorSignature InputSignature => new()
    {
        AmbientSpaceId = "Y_h",
        CarrierType = "gauge-parameter-0form",
        Degree = "0",
        LieAlgebraBasisId = _algebra.BasisOrderId,
        ComponentOrderId = "vertex-major",
        NumericPrecision = "float64",
        MemoryLayout = "dense-row-major",
    };

    /// <summary>Output: edge-valued connection perturbations (EdgeCount * dimG).</summary>
    public TensorSignature OutputSignature => new()
    {
        AmbientSpaceId = "Y_h",
        CarrierType = "connection-1form",
        Degree = "1",
        LieAlgebraBasisId = _algebra.BasisOrderId,
        ComponentOrderId = "edge-major",
        NumericPrecision = "float64",
        MemoryLayout = "dense-row-major",
    };

    /// <summary>Number of gauge parameter DOFs (VertexCount * dimG).</summary>
    public int InputDimension => _mesh.VertexCount * _algebra.Dimension;

    /// <summary>Number of connection DOFs (EdgeCount * dimG).</summary>
    public int OutputDimension => _mesh.EdgeCount * _algebra.Dimension;

    /// <summary>
    /// Forward action: R(xi) = -d_{A0}(xi) + [(omega_* - A0), xi].
    ///
    /// d_{A0}(xi) = d(xi) + [A0 wedge xi]
    ///
    /// So: R(xi) = -d(xi) - [A0, xi]_edge + [(omega_* - A0), xi]_edge
    ///           = -d(xi) + [(omega_* - 2*A0), xi]_edge  ... NO, let's be careful.
    ///
    /// Actually: R(xi) = -d(xi) - [A0 wedge xi] + [(omega_* - A0) wedge xi]
    ///                  = -d(xi) + [(omega_* - 2*A0) wedge xi]  ... still wrong.
    ///
    /// Let's expand carefully:
    ///   R(xi) = -d_{A0}(xi) + [A_* - A0, xi]
    /// where A_* = omega_* (the background connection).
    ///   d_{A0}(xi)[e] = d(xi)[e] + [A0[e], xi_avg[e]]
    ///   [A_* - A0, xi][e] = [(omega_*[e] - A0[e]), xi_avg[e]]
    ///
    /// So: R(xi)[e] = -d(xi)[e] - [A0[e], xi_avg[e]] + [(omega_*[e] - A0[e]), xi_avg[e]]
    ///             = -d(xi)[e] + [omega_*[e] - 2*A0[e], xi_avg[e]]
    ///
    /// Wait, that's not right either. The bracket terms:
    ///   -[A0[e], xi_avg[e]] + [(omega_*[e] - A0[e]), xi_avg[e]]
    ///   = [-A0[e] + omega_*[e] - A0[e], xi_avg[e]]
    ///   = [omega_*[e] - 2*A0[e], xi_avg[e]]
    ///
    /// This is correct. For A_* = A0: R(xi) = -d(xi) + [A0 - 2*A0, xi] = -d(xi) - [A0, xi] = -d_{A0}(xi).
    /// For A_* = A0: R(xi) = -d_{A0}(xi), as expected.
    /// </summary>
    public FieldTensor Apply(FieldTensor xi)
    {
        int edgeCount = _mesh.EdgeCount;
        int dimG = _algebra.Dimension;

        // Step 1: -d(xi) (exterior derivative of vertex 0-form to edge 1-form)
        var dXi = _dOperator.ApplyExteriorDerivative(xi.Coefficients);

        var result = new double[edgeCount * dimG];

        for (int e = 0; e < edgeCount; e++)
        {
            int v0 = _mesh.Edges[e][0];
            int v1 = _mesh.Edges[e][1];

            // Average xi on the edge: xi_avg = (xi[v0] + xi[v1]) / 2
            var xiAvg = new double[dimG];
            for (int a = 0; a < dimG; a++)
                xiAvg[a] = 0.5 * (xi.Coefficients[v0 * dimG + a] + xi.Coefficients[v1 * dimG + a]);

            // Bracket coefficient: omega_*[e] - 2*A0[e]
            var bracketCoeff = new double[dimG];
            for (int a = 0; a < dimG; a++)
                bracketCoeff[a] = _omegaStarCoeffs[e * dimG + a] - 2.0 * _a0Coeffs[e * dimG + a];

            // Compute [bracketCoeff, xiAvg] using Lie bracket
            var bracket = _algebra.Bracket(bracketCoeff, xiAvg);

            for (int a = 0; a < dimG; a++)
                result[e * dimG + a] = -dXi[e * dimG + a] + bracket[a];
        }

        return new FieldTensor
        {
            Label = "R*xi",
            Signature = OutputSignature,
            Coefficients = result,
            Shape = new[] { edgeCount, dimG },
        };
    }

    /// <summary>
    /// Transpose action: R^T * w.
    /// Computed column-by-column (acceptable for small problems).
    /// </summary>
    public FieldTensor ApplyTranspose(FieldTensor w)
    {
        int nIn = InputDimension;
        int nOut = OutputDimension;
        var result = new double[nIn];

        var ej = new double[nIn];
        for (int j = 0; j < nIn; j++)
        {
            ej[j] = 1.0;
            var rEj = Apply(new FieldTensor
            {
                Label = "e_j",
                Signature = InputSignature,
                Coefficients = (double[])ej.Clone(),
                Shape = new[] { _mesh.VertexCount, _algebra.Dimension },
            });

            double dot = 0;
            for (int i = 0; i < nOut; i++)
                dot += rEj.Coefficients[i] * w.Coefficients[i];
            result[j] = dot;

            ej[j] = 0.0;
        }

        return new FieldTensor
        {
            Label = "R^T*w",
            Signature = InputSignature,
            Coefficients = result,
            Shape = new[] { _mesh.VertexCount, _algebra.Dimension },
        };
    }
}
