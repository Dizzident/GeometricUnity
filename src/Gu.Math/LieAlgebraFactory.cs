namespace Gu.Math;

/// <summary>
/// Factory for creating standard Lie algebras with correct structure constants
/// and invariant metrics.
/// </summary>
public static class LieAlgebraFactory
{
    /// <summary>
    /// Creates the su(2) Lie algebra with standard basis {T_1, T_2, T_3}
    /// and structure constants f^c_{ab} = epsilon_{abc}.
    /// Uses the Killing form g_{ab} = -2 * delta_{ab} as the invariant metric.
    /// The Killing form is negative-definite for compact semisimple algebras.
    /// </summary>
    public static LieAlgebra CreateSu2()
    {
        const int dim = 3;
        var structureConstants = new double[dim * dim * dim];

        // f^c_{ab} = epsilon_{abc} (Levi-Civita symbol)
        // [T_1, T_2] = T_3, [T_2, T_3] = T_1, [T_3, T_1] = T_2
        SetStructureConstant(structureConstants, dim, 0, 1, 2, 1.0);  // f^3_{12} = 1
        SetStructureConstant(structureConstants, dim, 1, 0, 2, -1.0); // f^3_{21} = -1
        SetStructureConstant(structureConstants, dim, 1, 2, 0, 1.0);  // f^1_{23} = 1
        SetStructureConstant(structureConstants, dim, 2, 1, 0, -1.0); // f^1_{32} = -1
        SetStructureConstant(structureConstants, dim, 2, 0, 1, 1.0);  // f^2_{31} = 1
        SetStructureConstant(structureConstants, dim, 0, 2, 1, -1.0); // f^2_{13} = -1

        // Killing form for su(2): g_{ab} = -2 * delta_{ab}
        var metric = new double[dim * dim];
        metric[0] = -2.0; // g_{11}
        metric[4] = -2.0; // g_{22}
        metric[8] = -2.0; // g_{33}

        return new LieAlgebra
        {
            AlgebraId = "su2",
            Dimension = dim,
            Label = "su(2)",
            BasisLabels = new[] { "T_1", "T_2", "T_3" },
            BasisOrderId = "canonical",
            StructureConstants = structureConstants,
            InvariantMetric = metric,
            PairingId = "killing",
        };
    }

    /// <summary>
    /// Creates the su(2) Lie algebra with the positive-definite trace pairing
    /// g_{ab} = delta_{ab} instead of the Killing form. Useful for tests and
    /// branches that require a positive-definite metric on the algebra.
    /// </summary>
    public static LieAlgebra CreateSu2WithTracePairing()
    {
        const int dim = 3;
        var structureConstants = new double[dim * dim * dim];

        // f^c_{ab} = epsilon_{abc} (Levi-Civita symbol)
        SetStructureConstant(structureConstants, dim, 0, 1, 2, 1.0);  // f^3_{12} = 1
        SetStructureConstant(structureConstants, dim, 1, 0, 2, -1.0); // f^3_{21} = -1
        SetStructureConstant(structureConstants, dim, 1, 2, 0, 1.0);  // f^1_{23} = 1
        SetStructureConstant(structureConstants, dim, 2, 1, 0, -1.0); // f^1_{32} = -1
        SetStructureConstant(structureConstants, dim, 2, 0, 1, 1.0);  // f^2_{31} = 1
        SetStructureConstant(structureConstants, dim, 0, 2, 1, -1.0); // f^2_{13} = -1

        // Trace pairing: g_{ab} = delta_{ab} (positive-definite)
        var metric = new double[dim * dim];
        metric[0] = 1.0; // g_{11}
        metric[4] = 1.0; // g_{22}
        metric[8] = 1.0; // g_{33}

        return new LieAlgebra
        {
            AlgebraId = "su2",
            Dimension = dim,
            Label = "su(2)",
            BasisLabels = new[] { "T_1", "T_2", "T_3" },
            BasisOrderId = "canonical",
            StructureConstants = structureConstants,
            InvariantMetric = metric,
            PairingId = "trace",
        };
    }

    /// <summary>
    /// Creates the su(3) Lie algebra with Gell-Mann basis {T_1, ..., T_8}
    /// and standard structure constants.
    /// </summary>
    public static LieAlgebra CreateSu3()
    {
        const int dim = 8;
        var structureConstants = new double[dim * dim * dim];

        // SU(3) structure constants f_{abc} (totally antisymmetric)
        // Using Gell-Mann basis convention (1-indexed in comments, 0-indexed in code)
        // f_{123} = 1
        SetTotallyAntiSymmetric(structureConstants, dim, 0, 1, 2, 1.0);
        // f_{147} = 1/2
        SetTotallyAntiSymmetric(structureConstants, dim, 0, 3, 6, 0.5);
        // f_{156} = -1/2
        SetTotallyAntiSymmetric(structureConstants, dim, 0, 4, 5, -0.5);
        // f_{246} = 1/2
        SetTotallyAntiSymmetric(structureConstants, dim, 1, 3, 5, 0.5);
        // f_{257} = 1/2
        SetTotallyAntiSymmetric(structureConstants, dim, 1, 4, 6, 0.5);
        // f_{345} = 1/2
        SetTotallyAntiSymmetric(structureConstants, dim, 2, 3, 4, 0.5);
        // f_{367} = -1/2
        SetTotallyAntiSymmetric(structureConstants, dim, 2, 5, 6, -0.5);
        // f_{458} = sqrt(3)/2
        SetTotallyAntiSymmetric(structureConstants, dim, 3, 4, 7, System.Math.Sqrt(3.0) / 2.0);
        // f_{678} = sqrt(3)/2
        SetTotallyAntiSymmetric(structureConstants, dim, 5, 6, 7, System.Math.Sqrt(3.0) / 2.0);

        // Killing form for su(3) in Gell-Mann normalization: g_{ab} = -3 * delta_{ab}
        var metric = new double[dim * dim];
        for (int i = 0; i < dim; i++)
            metric[i * dim + i] = -3.0;

        return new LieAlgebra
        {
            AlgebraId = "su3",
            Dimension = dim,
            Label = "su(3)",
            BasisLabels = new[] { "T_1", "T_2", "T_3", "T_4", "T_5", "T_6", "T_7", "T_8" },
            BasisOrderId = "gell-mann",
            StructureConstants = structureConstants,
            InvariantMetric = metric,
            PairingId = "killing",
        };
    }

    /// <summary>
    /// Creates an abelian Lie algebra u(1)^n with trivial structure constants.
    /// </summary>
    public static LieAlgebra CreateAbelian(int dimension, string algebraId = "u1")
    {
        var structureConstants = new double[dimension * dimension * dimension]; // all zero
        var metric = new double[dimension * dimension];
        var labels = new string[dimension];

        for (int i = 0; i < dimension; i++)
        {
            metric[i * dimension + i] = 1.0;
            labels[i] = $"T_{i + 1}";
        }

        return new LieAlgebra
        {
            AlgebraId = algebraId,
            Dimension = dimension,
            Label = dimension == 1 ? "u(1)" : $"u(1)^{dimension}",
            BasisLabels = labels,
            BasisOrderId = "canonical",
            StructureConstants = structureConstants,
            InvariantMetric = metric,
            PairingId = "trace",
        };
    }

    private static void SetStructureConstant(double[] f, int dim, int a, int b, int c, double value)
    {
        f[a * dim * dim + b * dim + c] = value;
    }

    /// <summary>
    /// Set a totally antisymmetric structure constant f_{abc} = value,
    /// filling all 6 permutations with appropriate signs.
    /// The indexing convention is f^c_{ab}, so f_{abc} maps to f^c_{ab}.
    /// </summary>
    private static void SetTotallyAntiSymmetric(double[] f, int dim, int a, int b, int c, double value)
    {
        // Even permutations: (a,b,c), (b,c,a), (c,a,b) -> +value
        // Odd permutations:  (a,c,b), (c,b,a), (b,a,c) -> -value
        // Stored as f^c_{ab}: f[a, b, c] = f^c_{ab}
        SetStructureConstant(f, dim, a, b, c, value);   // f^c_{ab}
        SetStructureConstant(f, dim, b, c, a, value);   // f^a_{bc}
        SetStructureConstant(f, dim, c, a, b, value);   // f^b_{ca}
        SetStructureConstant(f, dim, b, a, c, -value);  // f^c_{ba}
        SetStructureConstant(f, dim, a, c, b, -value);  // f^b_{ac}
        SetStructureConstant(f, dim, c, b, a, -value);  // f^a_{cb}
    }
}
