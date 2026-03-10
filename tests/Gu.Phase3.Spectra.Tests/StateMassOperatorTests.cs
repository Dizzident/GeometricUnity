using Gu.Math;

namespace Gu.Phase3.Spectra.Tests;

public class StateMassOperatorTests
{
    [Fact]
    public void Dimensions_AreCorrect()
    {
        var mesh = TestHelpers.SingleTriangle();
        var algebra = TestHelpers.TracePairingSu2();
        var mass = new StateMassOperator(mesh, algebra);

        int expected = mesh.EdgeCount * algebra.Dimension; // 3 edges * 3 = 9
        Assert.Equal(expected, mass.InputDimension);
        Assert.Equal(expected, mass.OutputDimension);
    }

    [Fact]
    public void Apply_ZeroVector_ReturnsZero()
    {
        var mesh = TestHelpers.SingleTriangle();
        var algebra = TestHelpers.TracePairingSu2();
        var mass = new StateMassOperator(mesh, algebra);

        var zero = TestHelpers.MakeField(mass.InputDimension);
        var result = mass.Apply(zero);

        for (int i = 0; i < result.Coefficients.Length; i++)
            Assert.Equal(0.0, result.Coefficients[i]);
    }

    [Fact]
    public void Apply_IsSymmetric()
    {
        var mesh = TestHelpers.SingleTriangle();
        var algebra = TestHelpers.TracePairingSu2();
        var mass = new StateMassOperator(mesh, algebra);
        var rng = new Random(42);

        for (int t = 0; t < 5; t++)
        {
            var u = TestHelpers.MakeRandomField(mass.InputDimension, rng);
            var v = TestHelpers.MakeRandomField(mass.InputDimension, rng);

            var mu = mass.Apply(u);
            var mv = mass.Apply(v);

            double uMv = TestHelpers.Dot(u.Coefficients, mv.Coefficients);
            double vMu = TestHelpers.Dot(v.Coefficients, mu.Coefficients);

            Assert.Equal(uMv, vMu, 12);
        }
    }

    [Fact]
    public void Apply_IsPositiveSemiDefinite_WithTracePairing()
    {
        var mesh = TestHelpers.SingleTriangle();
        var algebra = TestHelpers.TracePairingSu2();
        var mass = new StateMassOperator(mesh, algebra);
        var rng = new Random(123);

        for (int t = 0; t < 20; t++)
        {
            var v = TestHelpers.MakeRandomField(mass.InputDimension, rng);
            var mv = mass.Apply(v);
            double vMv = TestHelpers.Dot(v.Coefficients, mv.Coefficients);
            Assert.True(vMv >= -1e-14,
                $"M_state not PSD: <v, M*v> = {vMv:E6}");
        }
    }

    [Fact]
    public void InnerProduct_MatchesManualComputation()
    {
        var mesh = TestHelpers.SingleTriangle();
        var algebra = TestHelpers.TracePairingSu2();
        var mass = new StateMassOperator(mesh, algebra);
        var rng = new Random(99);

        var u = TestHelpers.MakeRandomField(mass.InputDimension, rng);
        var v = TestHelpers.MakeRandomField(mass.InputDimension, rng);

        double ipMethod = mass.InnerProduct(u, v);
        var mv = mass.Apply(v);
        double ipManual = TestHelpers.Dot(u.Coefficients, mv.Coefficients);

        Assert.Equal(ipManual, ipMethod, 12);
    }

    [Fact]
    public void Norm_IsConsistentWithInnerProduct()
    {
        var mesh = TestHelpers.SingleTriangle();
        var algebra = TestHelpers.TracePairingSu2();
        var mass = new StateMassOperator(mesh, algebra);
        var rng = new Random(77);

        var v = TestHelpers.MakeRandomField(mass.InputDimension, rng);

        double norm = mass.Norm(v);
        double ip = mass.InnerProduct(v, v);

        Assert.Equal(System.Math.Sqrt(ip), norm, 12);
    }

    [Fact]
    public void ApplyTranspose_EqualsApply()
    {
        var mesh = TestHelpers.SingleTriangle();
        var algebra = TestHelpers.TracePairingSu2();
        var mass = new StateMassOperator(mesh, algebra);
        var rng = new Random(55);

        var v = TestHelpers.MakeRandomField(mass.InputDimension, rng);

        var apply = mass.Apply(v);
        var applyT = mass.ApplyTranspose(v);

        for (int i = 0; i < apply.Coefficients.Length; i++)
            Assert.Equal(apply.Coefficients[i], applyT.Coefficients[i]);
    }

    [Fact]
    public void CustomWeights_AreRespected()
    {
        var mesh = TestHelpers.SingleTriangle();
        var algebra = TestHelpers.TracePairingSu2();
        var weights = new double[] { 2.0, 3.0, 5.0 };
        var mass = new StateMassOperator(mesh, algebra, weights);

        // With trace pairing (identity metric) and custom weights,
        // M*v[e*dimG + a] = w_e * v[e*dimG + a]
        var v = TestHelpers.MakeField(mass.InputDimension, 1.0);
        var result = mass.Apply(v);

        // edge 0: weight=2, each component * 2
        for (int a = 0; a < 3; a++)
            Assert.Equal(2.0, result.Coefficients[0 * 3 + a]);
        // edge 1: weight=3
        for (int a = 0; a < 3; a++)
            Assert.Equal(3.0, result.Coefficients[1 * 3 + a]);
        // edge 2: weight=5
        for (int a = 0; a < 3; a++)
            Assert.Equal(5.0, result.Coefficients[2 * 3 + a]);
    }

    [Fact]
    public void Constructor_ThrowsOnWrongWeightLength()
    {
        var mesh = TestHelpers.SingleTriangle();
        var algebra = TestHelpers.TracePairingSu2();

        Assert.Throws<ArgumentException>(() =>
            new StateMassOperator(mesh, algebra, new double[] { 1.0, 2.0 })); // need 3
    }
}
