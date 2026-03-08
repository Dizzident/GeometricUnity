using System.Text.Json.Serialization;

namespace Gu.Math;

/// <summary>
/// Finite-dimensional real Lie algebra backend (IX-3).
/// Provides explicit structure constants, invariant metric/pairing, and basis order metadata.
/// Does not hardcode physical Standard Model assumptions into the core engine.
/// </summary>
public sealed class LieAlgebra
{
    /// <summary>Unique identifier for this Lie algebra (e.g., "su2", "su3", "so14").</summary>
    [JsonPropertyName("algebraId")]
    public required string AlgebraId { get; init; }

    /// <summary>Dimension of the Lie algebra (number of generators).</summary>
    [JsonPropertyName("dimension")]
    public required int Dimension { get; init; }

    /// <summary>Human-readable label.</summary>
    [JsonPropertyName("label")]
    public string? Label { get; init; }

    /// <summary>
    /// Basis labels, one per generator. Length must equal <see cref="Dimension"/>.
    /// </summary>
    [JsonPropertyName("basisLabels")]
    public required IReadOnlyList<string> BasisLabels { get; init; }

    /// <summary>
    /// Basis ordering convention identifier (e.g., "canonical", "physicist", "mathematician").
    /// </summary>
    [JsonPropertyName("basisOrderId")]
    public required string BasisOrderId { get; init; }

    /// <summary>
    /// Structure constants f^c_{ab} stored as a flat array of size dim^3.
    /// Indexed as [a * dim * dim + b * dim + c], so f[a,b] = sum_c f^c_{ab} * T_c.
    /// Antisymmetric: f^c_{ab} = -f^c_{ba}.
    /// </summary>
    [JsonPropertyName("structureConstants")]
    public required double[] StructureConstants { get; init; }

    /// <summary>
    /// Invariant metric (Killing form or chosen pairing) as a dim x dim symmetric matrix.
    /// Stored row-major as flat array of size dim^2.
    /// </summary>
    [JsonPropertyName("invariantMetric")]
    public required double[] InvariantMetric { get; init; }

    /// <summary>
    /// Pairing convention identifier (e.g., "killing", "trace", "custom").
    /// </summary>
    [JsonPropertyName("pairingId")]
    public required string PairingId { get; init; }

    /// <summary>
    /// Get the structure constant f^c_{ab}.
    /// </summary>
    public double GetStructureConstant(int a, int b, int c)
    {
        ValidateIndex(a);
        ValidateIndex(b);
        ValidateIndex(c);
        return StructureConstants[a * Dimension * Dimension + b * Dimension + c];
    }

    /// <summary>
    /// Get the invariant metric component g_{ab}.
    /// </summary>
    public double GetMetricComponent(int a, int b)
    {
        ValidateIndex(a);
        ValidateIndex(b);
        return InvariantMetric[a * Dimension + b];
    }

    /// <summary>
    /// Compute the Lie bracket [T_a, T_b] = sum_c f^c_{ab} * T_c,
    /// returning the result as a vector of coefficients in the basis.
    /// </summary>
    public double[] Bracket(double[] x, double[] y)
    {
        if (x.Length != Dimension || y.Length != Dimension)
            throw new ArgumentException($"Input vectors must have length {Dimension}.");

        var result = new double[Dimension];
        for (int c = 0; c < Dimension; c++)
        {
            double sum = 0;
            for (int a = 0; a < Dimension; a++)
            {
                for (int b = 0; b < Dimension; b++)
                {
                    sum += StructureConstants[a * Dimension * Dimension + b * Dimension + c] * x[a] * y[b];
                }
            }
            result[c] = sum;
        }
        return result;
    }

    /// <summary>
    /// Compute the inner product of two algebra elements using the invariant metric.
    /// </summary>
    public double InnerProduct(double[] x, double[] y)
    {
        if (x.Length != Dimension || y.Length != Dimension)
            throw new ArgumentException($"Input vectors must have length {Dimension}.");

        double sum = 0;
        for (int a = 0; a < Dimension; a++)
        {
            for (int b = 0; b < Dimension; b++)
            {
                sum += InvariantMetric[a * Dimension + b] * x[a] * y[b];
            }
        }
        return sum;
    }

    /// <summary>
    /// Validate the Jacobi identity: [X, [Y, Z]] + [Y, [Z, X]] + [Z, [X, Y]] = 0
    /// for all basis elements. Returns max violation norm.
    /// </summary>
    public double ValidateJacobiIdentity()
    {
        double maxViolation = 0;

        for (int a = 0; a < Dimension; a++)
        {
            var ea = BasisVector(a);
            for (int b = 0; b < Dimension; b++)
            {
                var eb = BasisVector(b);
                for (int c = 0; c < Dimension; c++)
                {
                    var ec = BasisVector(c);

                    var bc = Bracket(eb, ec);
                    var ca = Bracket(ec, ea);
                    var ab = Bracket(ea, eb);

                    var term1 = Bracket(ea, bc);
                    var term2 = Bracket(eb, ca);
                    var term3 = Bracket(ec, ab);

                    for (int d = 0; d < Dimension; d++)
                    {
                        double violation = System.Math.Abs(term1[d] + term2[d] + term3[d]);
                        if (violation > maxViolation)
                            maxViolation = violation;
                    }
                }
            }
        }

        return maxViolation;
    }

    /// <summary>
    /// Validate antisymmetry of structure constants: f^c_{ab} = -f^c_{ba}.
    /// Returns max violation.
    /// </summary>
    public double ValidateAntisymmetry()
    {
        double maxViolation = 0;
        for (int a = 0; a < Dimension; a++)
        {
            for (int b = 0; b < Dimension; b++)
            {
                for (int c = 0; c < Dimension; c++)
                {
                    double fab = StructureConstants[a * Dimension * Dimension + b * Dimension + c];
                    double fba = StructureConstants[b * Dimension * Dimension + a * Dimension + c];
                    double violation = System.Math.Abs(fab + fba);
                    if (violation > maxViolation)
                        maxViolation = violation;
                }
            }
        }
        return maxViolation;
    }

    /// <summary>
    /// Validate metric symmetry: g_{ab} = g_{ba}. Returns max violation.
    /// </summary>
    public double ValidateMetricSymmetry()
    {
        double maxViolation = 0;
        for (int a = 0; a < Dimension; a++)
        {
            for (int b = 0; b < Dimension; b++)
            {
                double violation = System.Math.Abs(
                    InvariantMetric[a * Dimension + b] - InvariantMetric[b * Dimension + a]);
                if (violation > maxViolation)
                    maxViolation = violation;
            }
        }
        return maxViolation;
    }

    private double[] BasisVector(int index)
    {
        var v = new double[Dimension];
        v[index] = 1.0;
        return v;
    }

    private void ValidateIndex(int index)
    {
        if (index < 0 || index >= Dimension)
            throw new ArgumentOutOfRangeException(nameof(index),
                $"Index {index} is out of range [0, {Dimension}).");
    }
}
