using Gu.Core;

namespace Gu.Phase3.Spectra.Tests;

public class FullHessianOperatorTests
{
    [Fact]
    public void WithoutCorrection_DelegatesToHGN()
    {
        int n = 9;
        var diag = new double[n];
        for (int i = 0; i < n; i++) diag[i] = i + 1.0;
        var hgn = new TestHelpers.DiagonalOperator(diag);

        var fullH = new FullHessianOperator(hgn);

        Assert.False(fullH.HasResidualCorrection);

        var rng = new Random(42);
        var v = TestHelpers.MakeRandomField(n, rng);
        var result = fullH.Apply(v);
        var expected = hgn.Apply(v);

        for (int i = 0; i < n; i++)
            Assert.Equal(expected.Coefficients[i], result.Coefficients[i]);
    }

    [Fact]
    public void WithCorrection_AddsToHGN()
    {
        int n = 6;
        var diag = new double[n];
        for (int i = 0; i < n; i++) diag[i] = 1.0;
        var hgn = new TestHelpers.DiagonalOperator(diag);

        // Correction that doubles every component
        Func<FieldTensor, FieldTensor> correction = v =>
        {
            var result = new double[n];
            for (int i = 0; i < n; i++)
                result[i] = v.Coefficients[i]; // same as HGN, so total = 2*v
            return new FieldTensor
            {
                Label = "correction",
                Signature = TestHelpers.ConnectionSignature(),
                Coefficients = result,
                Shape = new[] { n },
            };
        };

        var fullH = new FullHessianOperator(hgn, correction);
        Assert.True(fullH.HasResidualCorrection);

        var v = TestHelpers.MakeField(n, 3.0);
        var result = fullH.Apply(v);

        // HGN(v) = v (identity), correction(v) = v, so H_full(v) = 2*v
        for (int i = 0; i < n; i++)
            Assert.Equal(6.0, result.Coefficients[i]);
    }

    [Fact]
    public void IsSymmetric_WhenBothTermsAre()
    {
        int n = 9;
        var diag = new double[n];
        for (int i = 0; i < n; i++) diag[i] = (i + 1.0) * 0.5;
        var hgn = new TestHelpers.DiagonalOperator(diag);

        // Symmetric correction: another diagonal
        var corrDiag = new double[n];
        for (int i = 0; i < n; i++) corrDiag[i] = 0.1 * (i + 1);
        Func<FieldTensor, FieldTensor> correction = v =>
        {
            var result = new double[n];
            for (int i = 0; i < n; i++)
                result[i] = corrDiag[i] * v.Coefficients[i];
            return new FieldTensor
            {
                Label = "corr",
                Signature = TestHelpers.ConnectionSignature(),
                Coefficients = result,
                Shape = new[] { n },
            };
        };

        var fullH = new FullHessianOperator(hgn, correction);
        var rng = new Random(42);

        for (int t = 0; t < 5; t++)
        {
            var u = TestHelpers.MakeRandomField(n, rng);
            var v = TestHelpers.MakeRandomField(n, rng);

            var hu = fullH.Apply(u);
            var hv = fullH.Apply(v);

            double uHv = TestHelpers.Dot(u.Coefficients, hv.Coefficients);
            double vHu = TestHelpers.Dot(v.Coefficients, hu.Coefficients);

            Assert.Equal(uHv, vHu, 12);
        }
    }

    [Fact]
    public void ApplyTranspose_EqualsApply()
    {
        int n = 6;
        var diag = new double[n];
        for (int i = 0; i < n; i++) diag[i] = i + 1.0;
        var hgn = new TestHelpers.DiagonalOperator(diag);
        var fullH = new FullHessianOperator(hgn);

        var rng = new Random(77);
        var v = TestHelpers.MakeRandomField(n, rng);

        var apply = fullH.Apply(v);
        var applyT = fullH.ApplyTranspose(v);

        for (int i = 0; i < n; i++)
            Assert.Equal(apply.Coefficients[i], applyT.Coefficients[i]);
    }

    [Fact]
    public void Dimensions_MatchHGN()
    {
        int n = 12;
        var diag = new double[n];
        Array.Fill(diag, 1.0);
        var hgn = new TestHelpers.DiagonalOperator(diag);
        var fullH = new FullHessianOperator(hgn);

        Assert.Equal(n, fullH.InputDimension);
        Assert.Equal(n, fullH.OutputDimension);
    }

    [Fact]
    public void Constructor_ThrowsOnNullHGN()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new FullHessianOperator(null!));
    }
}
