using Gu.Math;
using Gu.Core.Serialization;

namespace Gu.Core.Tests;

public class LieAlgebraFactoryTests
{
    [Fact]
    public void CreateSu2_ValidAlgebra()
    {
        var su2 = LieAlgebraFactory.CreateSu2();
        Assert.Equal(3, su2.Dimension);
        Assert.Equal("su2", su2.AlgebraId);
        Assert.True(su2.ValidateAntisymmetry() < 1e-15);
        Assert.True(su2.ValidateJacobiIdentity() < 1e-14);
        Assert.True(su2.ValidateMetricSymmetry() < 1e-15);
    }

    [Fact]
    public void CreateSu3_ValidAlgebra()
    {
        var su3 = LieAlgebraFactory.CreateSu3();
        Assert.Equal(8, su3.Dimension);
        Assert.Equal("su3", su3.AlgebraId);
        Assert.True(su3.ValidateAntisymmetry() < 1e-15);
        Assert.True(su3.ValidateJacobiIdentity() < 1e-13);
        Assert.True(su3.ValidateMetricSymmetry() < 1e-15);
    }

    [Fact]
    public void CreateSu3_Bracket_T1T2_GivesT3()
    {
        var su3 = LieAlgebraFactory.CreateSu3();
        // [T_1, T_2] = T_3 (same su(2) subalgebra)
        var t1 = new double[8]; t1[0] = 1.0;
        var t2 = new double[8]; t2[1] = 1.0;
        var result = su3.Bracket(t1, t2);

        Assert.Equal(0.0, result[0], 1e-15);
        Assert.Equal(0.0, result[1], 1e-15);
        Assert.Equal(1.0, result[2], 1e-15);
    }

    [Fact]
    public void CreateAbelian_AllBracketsZero()
    {
        var u1 = LieAlgebraFactory.CreateAbelian(5, "u1x5");
        Assert.Equal(5, u1.Dimension);

        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                var ei = new double[5]; ei[i] = 1.0;
                var ej = new double[5]; ej[j] = 1.0;
                var bracket = u1.Bracket(ei, ej);
                Assert.All(bracket, v => Assert.Equal(0.0, v));
            }
        }
    }

    [Fact]
    public void CreateAbelian_MetricIsDiagonal()
    {
        var u1 = LieAlgebraFactory.CreateAbelian(3);
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                double expected = i == j ? 1.0 : 0.0;
                Assert.Equal(expected, u1.GetMetricComponent(i, j));
            }
        }
    }

    [Fact]
    public void AllFactoryAlgebras_RoundTripJson()
    {
        var algebras = new[] { LieAlgebraFactory.CreateSu2(), LieAlgebraFactory.CreateSu3(), LieAlgebraFactory.CreateAbelian(4) };
        foreach (var algebra in algebras)
        {
            var json = GuJsonDefaults.Serialize(algebra);
            var rt = GuJsonDefaults.Deserialize<LieAlgebra>(json);
            Assert.NotNull(rt);
            Assert.Equal(algebra.AlgebraId, rt!.AlgebraId);
            Assert.Equal(algebra.Dimension, rt.Dimension);
        }
    }
}
