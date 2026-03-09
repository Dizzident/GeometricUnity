using Gu.Math;

namespace Gu.Math.Tests;

public class LieAlgebraEdgeCaseTests
{
    [Fact]
    public void GetMetricComponent_OutOfRange_Throws()
    {
        var su2 = LieAlgebraFactory.CreateSu2();
        Assert.Throws<ArgumentOutOfRangeException>(() => su2.GetMetricComponent(3, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => su2.GetMetricComponent(0, -1));
    }

    [Fact]
    public void GetStructureConstant_NegativeIndex_Throws()
    {
        var su2 = LieAlgebraFactory.CreateSu2();
        Assert.Throws<ArgumentOutOfRangeException>(() => su2.GetStructureConstant(0, 0, -1));
    }

    [Fact]
    public void Bracket_Antisymmetry_XY_NegatesYX()
    {
        var su2 = LieAlgebraFactory.CreateSu2();
        var x = new double[] { 0.3, 0.7, -0.2 };
        var y = new double[] { -0.1, 0.5, 0.9 };

        var xy = su2.Bracket(x, y);
        var yx = su2.Bracket(y, x);

        for (int i = 0; i < 3; i++)
            Assert.Equal(xy[i], -yx[i], 14);
    }

    [Fact]
    public void Bracket_SelfBracket_IsZero()
    {
        var su2 = LieAlgebraFactory.CreateSu2();
        var x = new double[] { 1.0, 2.0, 3.0 };
        var result = su2.Bracket(x, x);

        Assert.All(result, v => Assert.Equal(0.0, v, 14));
    }

    [Fact]
    public void InnerProduct_Bilinearity()
    {
        var su2 = LieAlgebraFactory.CreateSu2();
        var x = new double[] { 1.0, 0.0, 0.0 };
        var y = new double[] { 0.0, 1.0, 0.0 };
        var z = new double[] { 0.0, 0.0, 1.0 };

        // g(x, y+z) = g(x,y) + g(x,z) (Killing form is diagonal, so off-diag = 0)
        var yz = new double[] { 0.0, 1.0, 1.0 };
        double gxyz = su2.InnerProduct(x, yz);
        double gxy = su2.InnerProduct(x, y);
        double gxz = su2.InnerProduct(x, z);

        Assert.Equal(gxy + gxz, gxyz, 14);
    }

    [Fact]
    public void InnerProduct_Symmetry()
    {
        var su2 = LieAlgebraFactory.CreateSu2();
        var x = new double[] { 0.3, 0.7, -0.2 };
        var y = new double[] { -0.1, 0.5, 0.9 };

        Assert.Equal(su2.InnerProduct(x, y), su2.InnerProduct(y, x), 14);
    }

    [Fact]
    public void Bracket_EmptyVector_WrongSize_Throws()
    {
        var su2 = LieAlgebraFactory.CreateSu2();
        Assert.Throws<ArgumentException>(() => su2.Bracket(Array.Empty<double>(), new double[3]));
        Assert.Throws<ArgumentException>(() => su2.Bracket(new double[3], new double[2]));
    }

    [Fact]
    public void InnerProduct_EmptyVector_WrongSize_Throws()
    {
        var su2 = LieAlgebraFactory.CreateSu2();
        Assert.Throws<ArgumentException>(() => su2.InnerProduct(Array.Empty<double>(), new double[3]));
    }
}

public class LieAlgebraFactoryEdgeCaseTests
{
    [Fact]
    public void CreateSu2_KillingForm_IsNegativeDefinite()
    {
        var su2 = LieAlgebraFactory.CreateSu2();

        // All diagonal elements should be -2
        for (int i = 0; i < 3; i++)
            Assert.Equal(-2.0, su2.GetMetricComponent(i, i));

        // All off-diagonal should be 0
        for (int i = 0; i < 3; i++)
            for (int j = 0; j < 3; j++)
                if (i != j)
                    Assert.Equal(0.0, su2.GetMetricComponent(i, j));
    }

    [Fact]
    public void CreateSu2WithTracePairing_IsPositiveDefinite()
    {
        var su2 = LieAlgebraFactory.CreateSu2WithTracePairing();

        Assert.Equal("trace", su2.PairingId);

        for (int i = 0; i < 3; i++)
            Assert.Equal(1.0, su2.GetMetricComponent(i, i));
    }

    [Fact]
    public void CreateSu2_AndTracePairing_SameStructureConstants()
    {
        var killing = LieAlgebraFactory.CreateSu2();
        var trace = LieAlgebraFactory.CreateSu2WithTracePairing();

        Assert.Equal(killing.StructureConstants, trace.StructureConstants);
    }

    [Fact]
    public void CreateSu3_KillingForm_DiagonalIsNeg3()
    {
        var su3 = LieAlgebraFactory.CreateSu3();
        Assert.Equal("killing", su3.PairingId);

        for (int i = 0; i < 8; i++)
            Assert.Equal(-3.0, su3.GetMetricComponent(i, i));
    }

    [Fact]
    public void CreateSu3_TotallyAntisymmetric_AllPermutationsCorrect()
    {
        var su3 = LieAlgebraFactory.CreateSu3();

        // f_{123} = 1 (indices 0,1,2 in 0-based)
        // Check all 6 permutations
        Assert.Equal(1.0, su3.GetStructureConstant(0, 1, 2), 15);  // (a,b,c)
        Assert.Equal(1.0, su3.GetStructureConstant(1, 2, 0), 15);  // (b,c,a)
        Assert.Equal(1.0, su3.GetStructureConstant(2, 0, 1), 15);  // (c,a,b)
        Assert.Equal(-1.0, su3.GetStructureConstant(1, 0, 2), 15); // (b,a,c)
        Assert.Equal(-1.0, su3.GetStructureConstant(0, 2, 1), 15); // (a,c,b)
        Assert.Equal(-1.0, su3.GetStructureConstant(2, 1, 0), 15); // (c,b,a)
    }

    [Fact]
    public void CreateAbelian_Dimension1_LabelIsU1()
    {
        var u1 = LieAlgebraFactory.CreateAbelian(1);
        Assert.Equal("u(1)", u1.Label);
        Assert.Equal(1, u1.Dimension);
        Assert.Equal("trace", u1.PairingId);
    }

    [Fact]
    public void CreateAbelian_DimensionN_LabelHasExponent()
    {
        var u1n = LieAlgebraFactory.CreateAbelian(3, "u1x3");
        Assert.Equal("u(1)^3", u1n.Label);
        Assert.Equal("u1x3", u1n.AlgebraId);
    }

    [Fact]
    public void CreateAbelian_JacobiIsZero()
    {
        var u1 = LieAlgebraFactory.CreateAbelian(4);
        Assert.Equal(0.0, u1.ValidateJacobiIdentity());
    }

    [Fact]
    public void CreateAbelian_AntisymmetryIsZero()
    {
        var u1 = LieAlgebraFactory.CreateAbelian(4);
        Assert.Equal(0.0, u1.ValidateAntisymmetry());
    }

    [Fact]
    public void CreateAbelian_MetricSymmetryIsZero()
    {
        var u1 = LieAlgebraFactory.CreateAbelian(4);
        Assert.Equal(0.0, u1.ValidateMetricSymmetry());
    }

    [Fact]
    public void Su2_Bracket_CyclicPermutations()
    {
        var su2 = LieAlgebraFactory.CreateSu2();
        var e1 = new double[] { 1, 0, 0 };
        var e2 = new double[] { 0, 1, 0 };
        var e3 = new double[] { 0, 0, 1 };

        // [T3, T1] = T2
        var r = su2.Bracket(e3, e1);
        Assert.Equal(0.0, r[0], 15);
        Assert.Equal(1.0, r[1], 15);
        Assert.Equal(0.0, r[2], 15);
    }

    [Fact]
    public void Su3_Su2Subalgebra_T1T2BracketIsT3()
    {
        // The first three generators of su(3) form an su(2) subalgebra
        var su3 = LieAlgebraFactory.CreateSu3();
        var t1 = new double[8]; t1[0] = 1.0;
        var t2 = new double[8]; t2[1] = 1.0;

        var result = su3.Bracket(t1, t2);

        // [T1,T2] = f^c_{12} T_c = T3
        Assert.Equal(1.0, result[2], 14);
        // All other components should be zero
        for (int i = 0; i < 8; i++)
            if (i != 2)
                Assert.Equal(0.0, result[i], 14);
    }
}
