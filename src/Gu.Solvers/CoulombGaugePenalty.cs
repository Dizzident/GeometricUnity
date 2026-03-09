using Gu.Core;
using Gu.Geometry;

namespace Gu.Solvers;

/// <summary>
/// Proper Coulomb gauge penalty for the discrete setting.
///
/// Objective: (lambda/2) * ||d_{A0}^*(omega - omega_ref)||^2
/// Gradient:  lambda * d(d_{A0}^*(omega - omega_ref))
///
/// where d_{A0}^* is the codifferential (adjoint of covariant exterior derivative)
/// mapping Lie-algebra-valued 1-forms on edges to 0-forms on vertices.
/// d^* is the L2-adjoint of d, satisfying &lt;d^* omega, phi&gt; = &lt;omega, d phi&gt;.
///
/// and d is the exterior derivative mapping 0-forms on vertices to 1-forms on edges:
///
///   (d phi)[e={v0,v1}] = phi(v1) - phi(v0)
///
/// The composition d(d^*) is the graph Laplacian acting on 1-forms.
/// Each Lie algebra component is treated independently.
/// </summary>
public sealed class CoulombGaugePenalty : IGaugePenalty
{
    private readonly SimplicialMesh _mesh;
    private readonly int _dimG;
    private readonly double[]? _omegaRefCoeffs;

    /// <summary>Penalty coefficient lambda.</summary>
    public double Lambda { get; }

    /// <summary>
    /// Create a Coulomb gauge penalty.
    /// </summary>
    /// <param name="mesh">The simplicial mesh providing vertex-edge incidence.</param>
    /// <param name="dimG">Dimension of the Lie algebra (number of generators).</param>
    /// <param name="lambda">Penalty coefficient. Must be non-negative.</param>
    /// <param name="omegaRef">Reference connection (null for zero reference).</param>
    public CoulombGaugePenalty(SimplicialMesh mesh, int dimG, double lambda, FieldTensor? omegaRef = null)
    {
        _mesh = mesh ?? throw new ArgumentNullException(nameof(mesh));

        if (dimG < 1)
            throw new ArgumentOutOfRangeException(nameof(dimG), "Lie algebra dimension must be at least 1.");
        if (lambda < 0)
            throw new ArgumentOutOfRangeException(nameof(lambda), "Gauge penalty lambda must be non-negative.");

        _dimG = dimG;
        Lambda = lambda;

        if (omegaRef != null)
        {
            int expectedLength = mesh.EdgeCount * dimG;
            if (omegaRef.Coefficients.Length != expectedLength)
                throw new ArgumentException(
                    $"omegaRef length {omegaRef.Coefficients.Length} does not match EdgeCount*dimG = {expectedLength}.",
                    nameof(omegaRef));
            _omegaRefCoeffs = (double[])omegaRef.Coefficients.Clone();
        }
    }

    /// <summary>
    /// Apply the codifferential d^*: maps edge-valued 1-forms to vertex-valued 0-forms.
    /// Input: edge field of length EdgeCount * dimG (indexed as [edgeIdx * dimG + algebraIdx]).
    /// Output: vertex field of length VertexCount * dimG.
    ///
    /// d^* is the L2-adjoint of d (exterior derivative), computed as the transpose of d.
    /// Since d[e={v0,v1}] = phi[v1] - phi[v0], the transpose gives:
    ///   (d^* omega)[v, a] = sum_{e incident to v} -orientation(v,e) * omega[e, a]
    /// where orientation(v,e) = +1 if v is v0 of edge e, -1 if v is v1.
    /// </summary>
    public double[] ApplyCodifferential(double[] edgeField)
    {
        int vertexCount = _mesh.VertexCount;
        var result = new double[vertexCount * _dimG];

        for (int v = 0; v < vertexCount; v++)
        {
            int[] incidentEdges = _mesh.VertexEdges[v];
            int[] orientations = _mesh.VertexEdgeOrientations[v];

            for (int i = 0; i < incidentEdges.Length; i++)
            {
                int edgeIdx = incidentEdges[i];
                int sign = orientations[i];

                // The transpose of d maps: d[e={v0,v1}] = phi[v1] - phi[v0],
                // so d^T[v0,e] = -1 and d^T[v1,e] = +1.
                // VertexEdgeOrientations gives +1 for v0, -1 for v1,
                // so d^T coefficient = -sign.
                for (int a = 0; a < _dimG; a++)
                {
                    result[v * _dimG + a] += -sign * edgeField[edgeIdx * _dimG + a];
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Apply the exterior derivative d: maps vertex-valued 0-forms to edge-valued 1-forms.
    /// Input: vertex field of length VertexCount * dimG.
    /// Output: edge field of length EdgeCount * dimG.
    ///
    /// (d phi)[e={v0,v1}, a] = phi[v1, a] - phi[v0, a]
    /// </summary>
    public double[] ApplyExteriorDerivative(double[] vertexField)
    {
        int edgeCount = _mesh.EdgeCount;
        var result = new double[edgeCount * _dimG];

        for (int e = 0; e < edgeCount; e++)
        {
            int v0 = _mesh.Edges[e][0];
            int v1 = _mesh.Edges[e][1];

            for (int a = 0; a < _dimG; a++)
            {
                result[e * _dimG + a] = vertexField[v1 * _dimG + a] - vertexField[v0 * _dimG + a];
            }
        }

        return result;
    }

    /// <summary>
    /// Compute the difference (omega - omega_ref) as a raw coefficient array.
    /// </summary>
    private double[] GetDifference(FieldTensor omega)
    {
        if (_omegaRefCoeffs == null)
            return omega.Coefficients;

        var diff = new double[omega.Coefficients.Length];
        for (int i = 0; i < diff.Length; i++)
            diff[i] = omega.Coefficients[i] - _omegaRefCoeffs[i];
        return diff;
    }

    /// <summary>
    /// Evaluate the Coulomb gauge penalty objective: (lambda/2) * ||d^*(omega - omega_ref)||^2.
    /// </summary>
    public double EvaluateObjective(FieldTensor omega)
    {
        if (Lambda == 0) return 0;

        var diff = GetDifference(omega);
        var dStarOmega = ApplyCodifferential(diff);

        double normSq = 0;
        for (int i = 0; i < dStarOmega.Length; i++)
            normSq += dStarOmega[i] * dStarOmega[i];

        return 0.5 * Lambda * normSq;
    }

    /// <summary>
    /// Evaluate the Coulomb gauge penalty gradient: lambda * d(d^*(omega - omega_ref)).
    /// </summary>
    public FieldTensor EvaluateGradient(FieldTensor omega)
    {
        var result = new double[omega.Coefficients.Length];

        if (Lambda != 0)
        {
            var diff = GetDifference(omega);
            var dStarOmega = ApplyCodifferential(diff);
            var ddStarOmega = ApplyExteriorDerivative(dStarOmega);

            for (int i = 0; i < result.Length; i++)
                result[i] = Lambda * ddStarOmega[i];
        }

        return new FieldTensor
        {
            Label = "coulomb_gauge_gradient",
            Signature = omega.Signature,
            Coefficients = result,
            Shape = omega.Shape,
        };
    }

    /// <summary>
    /// Compute the Coulomb gauge violation norm: ||d^*(omega - omega_ref)||.
    /// </summary>
    public double ComputeViolationNorm(FieldTensor omega)
    {
        var diff = GetDifference(omega);
        var dStarOmega = ApplyCodifferential(diff);

        double normSq = 0;
        for (int i = 0; i < dStarOmega.Length; i++)
            normSq += dStarOmega[i] * dStarOmega[i];

        return System.Math.Sqrt(normSq);
    }

    /// <summary>
    /// Add the Coulomb gauge penalty objective to the physics objective.
    /// </summary>
    public double AddToObjective(double physicsObjective, FieldTensor omega)
    {
        return physicsObjective + EvaluateObjective(omega);
    }

    /// <summary>
    /// Add the Coulomb gauge penalty gradient to the physics gradient.
    /// Returns a new FieldTensor.
    /// </summary>
    public FieldTensor AddToGradient(FieldTensor physicsGradient, FieldTensor omega)
    {
        if (Lambda == 0) return physicsGradient;

        var gaugeGrad = EvaluateGradient(omega);
        var result = new double[physicsGradient.Coefficients.Length];
        for (int i = 0; i < result.Length; i++)
            result[i] = physicsGradient.Coefficients[i] + gaugeGrad.Coefficients[i];

        return new FieldTensor
        {
            Label = "total_gradient",
            Signature = physicsGradient.Signature,
            Coefficients = result,
            Shape = physicsGradient.Shape,
        };
    }
}
