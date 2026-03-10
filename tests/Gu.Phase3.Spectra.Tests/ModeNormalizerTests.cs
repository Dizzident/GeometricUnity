using Gu.Branching;
using Gu.Core;

namespace Gu.Phase3.Spectra.Tests;

public class ModeNormalizerTests
{
    [Fact]
    public void NormalizeL2_UnitVector()
    {
        var v = new double[] { 3.0, 4.0 }; // norm = 5
        var normalized = ModeNormalizer.NormalizeL2(v);

        double norm = System.Math.Sqrt(normalized[0] * normalized[0] + normalized[1] * normalized[1]);
        Assert.Equal(1.0, norm, 12);
        Assert.Equal(3.0 / 5.0, normalized[0], 12);
        Assert.Equal(4.0 / 5.0, normalized[1], 12);
    }

    [Fact]
    public void NormalizeL2_AlreadyNormalized()
    {
        var v = new double[] { 1.0, 0.0, 0.0 };
        var normalized = ModeNormalizer.NormalizeL2(v);
        Assert.Equal(1.0, normalized[0], 12);
        Assert.Equal(0.0, normalized[1], 12);
    }

    [Fact]
    public void NormalizeL2_ZeroVector_ReturnsZero()
    {
        var v = new double[] { 0.0, 0.0, 0.0 };
        var normalized = ModeNormalizer.NormalizeL2(v);
        Assert.Equal(0.0, normalized[0]);
        Assert.Equal(0.0, normalized[1]);
    }

    [Fact]
    public void NormalizeMState_DiagonalMass()
    {
        var v = new double[] { 1.0, 1.0 };
        var massOp = new TestHelpers.DiagonalOperator(new double[] { 4.0, 9.0 });
        // v^T M v = 1*4*1 + 1*9*1 = 13, norm = sqrt(13)
        var normalized = ModeNormalizer.NormalizeMState(v, massOp);

        double mNorm = 0;
        var mv = massOp.Apply(new FieldTensor
        {
            Label = "v",
            Signature = massOp.InputSignature,
            Coefficients = normalized,
            Shape = new[] { 2 },
        }).Coefficients;
        for (int i = 0; i < normalized.Length; i++)
            mNorm += normalized[i] * mv[i];

        Assert.Equal(1.0, mNorm, 10);
    }

    [Fact]
    public void NormalizeMState_IdentityMass_EqualsL2()
    {
        var v = new double[] { 3.0, 4.0 };
        var identityOp = new TestHelpers.DiagonalOperator(new double[] { 1.0, 1.0 });

        var mNorm = ModeNormalizer.NormalizeMState(v, identityOp);
        var l2Norm = ModeNormalizer.NormalizeL2(v);

        for (int i = 0; i < v.Length; i++)
            Assert.Equal(l2Norm[i], mNorm[i], 12);
    }

    [Fact]
    public void NormalizeMaxBlockNorm_SingleBlock()
    {
        var v = new double[] { 3.0, 4.0, 0.0 }; // one block, dimG=3, norm=5
        var normalized = ModeNormalizer.NormalizeMaxBlockNorm(v, dimG: 3);

        double blockNorm = 0;
        for (int i = 0; i < 3; i++)
            blockNorm += normalized[i] * normalized[i];
        Assert.Equal(1.0, System.Math.Sqrt(blockNorm), 12);
    }

    [Fact]
    public void NormalizeMaxBlockNorm_MultipleBlocks()
    {
        // Two blocks of dimG=3: block0 = (1,0,0) norm=1, block1 = (3,4,0) norm=5
        var v = new double[] { 1, 0, 0, 3, 4, 0 };
        var normalized = ModeNormalizer.NormalizeMaxBlockNorm(v, dimG: 3);

        // Max block norm = 5, so all divided by 5
        Assert.Equal(1.0 / 5.0, normalized[0], 12);
        Assert.Equal(3.0 / 5.0, normalized[3], 12);

        // Largest block should now have unit norm
        double block1Norm = 0;
        for (int k = 3; k < 6; k++)
            block1Norm += normalized[k] * normalized[k];
        Assert.Equal(1.0, System.Math.Sqrt(block1Norm), 12);
    }

    [Fact]
    public void NormalizeMaxBlockNorm_ZeroVector()
    {
        var v = new double[] { 0, 0, 0 };
        var normalized = ModeNormalizer.NormalizeMaxBlockNorm(v, dimG: 3);
        Assert.Equal(0.0, normalized[0]);
    }

    [Fact]
    public void Normalize_Dispatch_L2()
    {
        var v = new double[] { 3.0, 4.0 };
        var result = ModeNormalizer.Normalize(v, NormalizationConvention.L2Unit);
        var expected = ModeNormalizer.NormalizeL2(v);
        for (int i = 0; i < v.Length; i++)
            Assert.Equal(expected[i], result[i], 12);
    }

    [Fact]
    public void Normalize_Dispatch_MState()
    {
        var v = new double[] { 3.0, 4.0 };
        var massOp = new TestHelpers.DiagonalOperator(new double[] { 1.0, 1.0 });
        var result = ModeNormalizer.Normalize(v, NormalizationConvention.MStateUnit, massOp);
        var expected = ModeNormalizer.NormalizeMState(v, massOp);
        for (int i = 0; i < v.Length; i++)
            Assert.Equal(expected[i], result[i], 12);
    }

    [Fact]
    public void Normalize_Dispatch_MaxBlockNorm()
    {
        var v = new double[] { 3.0, 4.0, 0.0 };
        var result = ModeNormalizer.Normalize(v, NormalizationConvention.MaxBlockNorm, dimG: 3);
        var expected = ModeNormalizer.NormalizeMaxBlockNorm(v, 3);
        for (int i = 0; i < v.Length; i++)
            Assert.Equal(expected[i], result[i], 12);
    }

    [Fact]
    public void ConventionToString_AllValues()
    {
        Assert.Equal("unit-L2-norm", ModeNormalizer.ConventionToString(NormalizationConvention.L2Unit));
        Assert.Equal("unit-M-norm", ModeNormalizer.ConventionToString(NormalizationConvention.MStateUnit));
        Assert.Equal("max-block-norm", ModeNormalizer.ConventionToString(NormalizationConvention.MaxBlockNorm));
    }

    [Fact]
    public void NormalizeL2_DoesNotMutateInput()
    {
        var v = new double[] { 3.0, 4.0 };
        ModeNormalizer.NormalizeL2(v);
        Assert.Equal(3.0, v[0]);
        Assert.Equal(4.0, v[1]);
    }
}
