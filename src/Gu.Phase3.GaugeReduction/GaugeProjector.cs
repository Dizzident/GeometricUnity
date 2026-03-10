using Gu.Branching;
using Gu.Core;

namespace Gu.Phase3.GaugeReduction;

/// <summary>
/// Projector onto the gauge subspace and its physical complement.
///
/// Given an M_state-orthonormal gauge basis {q_1, ..., q_r}, the gauge projector is:
///   P_gauge(v) = sum_k q_k (q_k^T M_state v)
///
/// And the physical projector is:
///   P_phys(v) = v - P_gauge(v)
///
/// When StateMassMatrix is null (identity), this reduces to:
///   P_gauge(v) = sum_k q_k (q_k^T v)
///
/// Both are self-adjoint under their respective inner products:
///   (P x)^T M y = x^T M (P y)  (constraint #8)
/// </summary>
public sealed class GaugeProjector : ILinearOperator
{
    private readonly GaugeBasis _basis;

    /// <summary>
    /// The M_state operator, or null for Euclidean (identity) inner product.
    /// </summary>
    public ILinearOperator? StateMassMatrix { get; }

    /// <summary>Background ID.</summary>
    public string BackgroundId => _basis.BackgroundId;

    /// <summary>Gauge rank (number of gauge directions removed).</summary>
    public int GaugeRank => _basis.Rank;

    /// <summary>Physical dimension = ConnectionDimension - GaugeRank.</summary>
    public int PhysicalDimension => _basis.ConnectionDimension - _basis.Rank;

    /// <summary>Connection space dimension.</summary>
    public int ConnectionDimension => _basis.ConnectionDimension;

    /// <summary>Alias for ConnectionDimension (full state space dimension).</summary>
    public int Dimension => _basis.ConnectionDimension;

    /// <summary>The underlying gauge basis.</summary>
    public GaugeBasis Basis => _basis;

    public TensorSignature InputSignature => new()
    {
        AmbientSpaceId = "Y_h",
        CarrierType = "connection-1form",
        Degree = "1",
        LieAlgebraBasisId = "standard",
        ComponentOrderId = "edge-major",
        NumericPrecision = "float64",
        MemoryLayout = "dense-row-major",
    };

    public TensorSignature OutputSignature => InputSignature;

    public int InputDimension => _basis.ConnectionDimension;
    public int OutputDimension => _basis.ConnectionDimension;

    /// <summary>
    /// Create a gauge projector with Euclidean inner product (M_state = I).
    /// The gauge basis must be Euclidean-orthonormal.
    /// </summary>
    public GaugeProjector(GaugeBasis basis)
    {
        _basis = basis ?? throw new ArgumentNullException(nameof(basis));
    }

    /// <summary>
    /// Create a gauge projector with M_state inner product (ILinearOperator).
    ///
    /// The gauge basis must be M_state-orthonormal (built via GaugeBasis.BuildWithMass).
    /// P_gauge(v) = sum_k q_k (q_k^T M v), which is M_state-self-adjoint (constraint #8).
    /// Pass null for Euclidean (identity) inner product.
    /// </summary>
    public GaugeProjector(GaugeBasis basis, ILinearOperator? stateMassMatrix)
    {
        _basis = basis ?? throw new ArgumentNullException(nameof(basis));
        StateMassMatrix = stateMassMatrix;
    }

    /// <summary>
    /// Convenience constructor: create a gauge projector with diagonal M_state.
    /// Wraps the diagonal weights in a DiagonalOperator.
    /// </summary>
    public GaugeProjector(GaugeBasis basis, double[] massWeights)
    {
        _basis = basis ?? throw new ArgumentNullException(nameof(basis));
        if (massWeights == null) throw new ArgumentNullException(nameof(massWeights));
        if (massWeights.Length != basis.ConnectionDimension)
            throw new ArgumentException(
                $"massWeights length {massWeights.Length} != connection dimension {basis.ConnectionDimension}.");
        StateMassMatrix = new DiagonalOperator((double[])massWeights.Clone());
    }

    /// <summary>
    /// Apply the physical projector: P_phys(v) = v - P_gauge(v).
    /// This is the default Apply (physical projection).
    /// </summary>
    public FieldTensor Apply(FieldTensor v)
    {
        var result = ApplyPhysical(v.Coefficients);
        return new FieldTensor
        {
            Label = "P_phys*v",
            Signature = OutputSignature,
            Coefficients = result,
            Shape = new[] { _basis.ConnectionDimension },
        };
    }

    /// <summary>
    /// Apply transpose. P_phys is M_state-self-adjoint, so P_phys^T = P_phys
    /// under the M_state inner product. Under Euclidean inner product,
    /// P_phys^T = M P_phys M^{-1}, but for the ILinearOperator interface
    /// we treat Apply and ApplyTranspose as the same action (the projection).
    /// </summary>
    public FieldTensor ApplyTranspose(FieldTensor w) => Apply(w);

    /// <summary>
    /// Apply the gauge projector.
    ///
    /// With Euclidean inner product: P_gauge(v) = sum_k q_k (q_k^T v).
    /// With M_state inner product:   P_gauge(v) = sum_k q_k (q_k^T M v).
    ///
    /// Both are self-adjoint under their respective inner products.
    /// </summary>
    public double[] ApplyGauge(double[] v)
    {
        int dim = _basis.ConnectionDimension;
        var result = new double[dim];

        // Compute M*v once if M_state is set
        double[]? mv = null;
        if (StateMassMatrix != null)
        {
            var vTensor = new FieldTensor
            {
                Label = "v",
                Signature = InputSignature,
                Coefficients = v,
                Shape = new[] { dim },
            };
            mv = StateMassMatrix.Apply(vTensor).Coefficients;
        }

        for (int k = 0; k < _basis.Rank; k++)
        {
            var qk = _basis.Vectors[k];
            double dot = 0;
            var target = mv ?? v;
            for (int i = 0; i < dim; i++)
                dot += qk[i] * target[i];
            for (int i = 0; i < dim; i++)
                result[i] += dot * qk[i];
        }

        return result;
    }

    /// <summary>
    /// Apply the physical projector: P_phys(v) = v - P_gauge(v).
    /// </summary>
    public double[] ApplyPhysical(double[] v)
    {
        int dim = _basis.ConnectionDimension;
        var gauge = ApplyGauge(v);
        var result = new double[dim];
        for (int i = 0; i < dim; i++)
            result[i] = v[i] - gauge[i];
        return result;
    }

    /// <summary>
    /// Apply the gauge projector (alias for ApplyGauge).
    /// </summary>
    public double[] ApplyGaugeProjection(double[] v) => ApplyGauge(v);

    /// <summary>
    /// Apply the physical projector (alias for ApplyPhysical).
    /// </summary>
    public double[] ApplyPhysicalProjection(double[] v) => ApplyPhysical(v);

    /// <summary>
    /// Compute the gauge leak score for a given vector.
    /// Returns the fraction of the vector's norm in the gauge subspace:
    ///   leak = ||P_gauge(v)|| / ||v||
    /// </summary>
    /// <exception cref="ArgumentException">If v has wrong dimension.</exception>
    public double GaugeLeakScore(double[] v)
    {
        if (v.Length != _basis.ConnectionDimension)
            throw new ArgumentException(
                $"Vector length {v.Length} does not match connection dimension {_basis.ConnectionDimension}.",
                nameof(v));

        double normV = 0;
        for (int i = 0; i < v.Length; i++)
            normV += v[i] * v[i];
        if (normV < 1e-30) return 0.0;

        var pGauge = ApplyGauge(v);
        double normGauge = 0;
        for (int i = 0; i < pGauge.Length; i++)
            normGauge += pGauge[i] * pGauge[i];

        return System.Math.Sqrt(normGauge / normV);
    }
}
