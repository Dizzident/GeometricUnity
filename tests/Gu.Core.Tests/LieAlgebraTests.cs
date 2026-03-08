using Gu.Math;
using Gu.Core.Serialization;

namespace Gu.Core.Tests;

public class LieAlgebraTests
{
    /// <summary>
    /// Creates su(2) with the standard physicist conventions.
    /// Generators: T_1, T_2, T_3
    /// Structure constants: f^c_{ab} = epsilon_{abc} (Levi-Civita)
    /// Metric: g_{ab} = -2 * delta_{ab} (negative-definite Killing form for SU(2))
    /// </summary>
    private static LieAlgebra CreateSu2()
    {
        int dim = 3;
        var structureConstants = new double[dim * dim * dim];
        // f^c_{ab} = epsilon_{abc}
        // [T1, T2] = T3,  [T2, T3] = T1,  [T3, T1] = T2
        structureConstants[0 * 9 + 1 * 3 + 2] = 1.0;  // f^3_{12} = 1
        structureConstants[1 * 9 + 0 * 3 + 2] = -1.0; // f^3_{21} = -1
        structureConstants[1 * 9 + 2 * 3 + 0] = 1.0;  // f^1_{23} = 1
        structureConstants[2 * 9 + 1 * 3 + 0] = -1.0; // f^1_{32} = -1
        structureConstants[2 * 9 + 0 * 3 + 1] = 1.0;  // f^2_{31} = 1
        structureConstants[0 * 9 + 2 * 3 + 1] = -1.0; // f^2_{13} = -1

        // Killing form for SU(2): g_{ab} = -2 delta_{ab}
        var metric = new double[dim * dim];
        metric[0] = -2.0; // g_{11}
        metric[4] = -2.0; // g_{22}
        metric[8] = -2.0; // g_{33}

        return new LieAlgebra
        {
            AlgebraId = "su2",
            Dimension = dim,
            Label = "su(2)",
            BasisLabels = new[] { "T1", "T2", "T3" },
            BasisOrderId = "canonical",
            StructureConstants = structureConstants,
            InvariantMetric = metric,
            PairingId = "killing"
        };
    }

    [Fact]
    public void Su2_HasDimension3()
    {
        var su2 = CreateSu2();
        Assert.Equal(3, su2.Dimension);
        Assert.Equal(3, su2.BasisLabels.Count);
    }

    [Fact]
    public void Su2_StructureConstants_AreAntisymmetric()
    {
        var su2 = CreateSu2();
        double violation = su2.ValidateAntisymmetry();
        Assert.Equal(0.0, violation, precision: 15);
    }

    [Fact]
    public void Su2_JacobiIdentity_Holds()
    {
        var su2 = CreateSu2();
        double violation = su2.ValidateJacobiIdentity();
        Assert.Equal(0.0, violation, precision: 14);
    }

    [Fact]
    public void Su2_MetricIsSymmetric()
    {
        var su2 = CreateSu2();
        double violation = su2.ValidateMetricSymmetry();
        Assert.Equal(0.0, violation, precision: 15);
    }

    [Fact]
    public void Su2_Bracket_T1T2_EqualsT3()
    {
        var su2 = CreateSu2();
        var t1 = new double[] { 1, 0, 0 };
        var t2 = new double[] { 0, 1, 0 };
        var result = su2.Bracket(t1, t2);

        // [T1, T2] = T3
        Assert.Equal(0.0, result[0], precision: 15);
        Assert.Equal(0.0, result[1], precision: 15);
        Assert.Equal(1.0, result[2], precision: 15);
    }

    [Fact]
    public void Su2_Bracket_T2T3_EqualsT1()
    {
        var su2 = CreateSu2();
        var t2 = new double[] { 0, 1, 0 };
        var t3 = new double[] { 0, 0, 1 };
        var result = su2.Bracket(t2, t3);

        // [T2, T3] = T1
        Assert.Equal(1.0, result[0], precision: 15);
        Assert.Equal(0.0, result[1], precision: 15);
        Assert.Equal(0.0, result[2], precision: 15);
    }

    [Fact]
    public void Su2_InnerProduct_WithKillingForm()
    {
        var su2 = CreateSu2();
        var t1 = new double[] { 1, 0, 0 };
        var t2 = new double[] { 0, 1, 0 };

        // g(T1, T1) = -2 for Killing form of SU(2)
        Assert.Equal(-2.0, su2.InnerProduct(t1, t1), precision: 15);
        // g(T1, T2) = 0
        Assert.Equal(0.0, su2.InnerProduct(t1, t2), precision: 15);
    }

    [Fact]
    public void StructureConstantsLength_EqualsDimCubed()
    {
        var su2 = CreateSu2();
        Assert.Equal(su2.Dimension * su2.Dimension * su2.Dimension, su2.StructureConstants.Length);
    }

    [Fact]
    public void MetricLength_EqualsDimSquared()
    {
        var su2 = CreateSu2();
        Assert.Equal(su2.Dimension * su2.Dimension, su2.InvariantMetric.Length);
    }

    [Fact]
    public void Bracket_WrongDimension_Throws()
    {
        var su2 = CreateSu2();
        Assert.Throws<ArgumentException>(() => su2.Bracket(new[] { 1.0 }, new[] { 1.0 }));
    }

    [Fact]
    public void InnerProduct_WrongDimension_Throws()
    {
        var su2 = CreateSu2();
        Assert.Throws<ArgumentException>(() => su2.InnerProduct(new[] { 1.0 }, new[] { 1.0 }));
    }

    [Fact]
    public void GetStructureConstant_OutOfRange_Throws()
    {
        var su2 = CreateSu2();
        Assert.Throws<ArgumentOutOfRangeException>(() => su2.GetStructureConstant(3, 0, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => su2.GetStructureConstant(-1, 0, 0));
    }

    [Fact]
    public void RoundTrip_SerializesAndDeserializes()
    {
        var su2 = CreateSu2();
        var json = GuJsonDefaults.Serialize(su2);
        var deserialized = GuJsonDefaults.Deserialize<LieAlgebra>(json);

        Assert.NotNull(deserialized);
        Assert.Equal(su2.AlgebraId, deserialized.AlgebraId);
        Assert.Equal(su2.Dimension, deserialized.Dimension);
        Assert.Equal(su2.StructureConstants, deserialized.StructureConstants);
        Assert.Equal(su2.InvariantMetric, deserialized.InvariantMetric);
    }
}
