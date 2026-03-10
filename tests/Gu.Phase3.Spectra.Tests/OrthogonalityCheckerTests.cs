using Gu.Branching;
using Gu.Core;

namespace Gu.Phase3.Spectra.Tests;

public class OrthogonalityCheckerTests
{
    private static ModeRecord MakeMode(int index, double[] vector) => new()
    {
        ModeId = $"mode-{index}",
        BackgroundId = "test-bg",
        OperatorType = SpectralOperatorType.GaussNewton,
        Eigenvalue = 1.0 + index,
        ResidualNorm = 1e-12,
        NormalizationConvention = "unit-L2-norm",
        GaugeLeakScore = 0.0,
        ModeVector = vector,
        ModeIndex = index,
    };

    [Fact]
    public void OrthogonalVectors_ZeroDefect()
    {
        // Standard basis vectors are orthonormal
        var e1 = new double[] { 1, 0, 0 };
        var e2 = new double[] { 0, 1, 0 };
        var e3 = new double[] { 0, 0, 1 };

        var modes = new[] { MakeMode(0, e1), MakeMode(1, e2), MakeMode(2, e3) };
        double defect = OrthogonalityChecker.MaxOrthogonalityDefect(modes);
        Assert.True(defect < 1e-15, $"Expected ~0, got {defect:E6}");
    }

    [Fact]
    public void NonOrthogonalVectors_NonZeroDefect()
    {
        var v1 = new double[] { 1, 0, 0 };
        var v2 = new double[] { 0.5, 0.5, 0 }; // not orthogonal to v1

        var modes = new[] { MakeMode(0, v1), MakeMode(1, v2) };
        double defect = OrthogonalityChecker.MaxOrthogonalityDefect(modes);
        Assert.True(defect > 0.4, $"Expected non-zero defect, got {defect:E6}");
    }

    [Fact]
    public void NormalizationDefect_ForUnitVectors_IsZero()
    {
        var e1 = new double[] { 1, 0, 0 };
        var e2 = new double[] { 0, 1, 0 };

        var modes = new[] { MakeMode(0, e1), MakeMode(1, e2) };
        double defect = OrthogonalityChecker.MaxNormalizationDefect(modes);
        Assert.True(defect < 1e-15, $"Expected ~0, got {defect:E6}");
    }

    [Fact]
    public void NormalizationDefect_ForNonUnitVectors_IsNonZero()
    {
        var v1 = new double[] { 2, 0, 0 }; // norm = 2, not 1

        var modes = new[] { MakeMode(0, v1) };
        double defect = OrthogonalityChecker.MaxNormalizationDefect(modes);
        Assert.True(System.Math.Abs(defect - 3.0) < 1e-10, // |4 - 1| = 3
            $"Expected 3.0, got {defect:E6}");
    }

    [Fact]
    public void WithMassOperator_OrthogonalUnderM()
    {
        // v1 = (1, 0), v2 = (0, 1) with M = diag(2, 3)
        // v1^T M v2 = 0 (orthogonal under M)
        var v1 = new double[] { 1, 0 };
        var v2 = new double[] { 0, 1 };
        var massOp = new TestHelpers.DiagonalOperator(new double[] { 2.0, 3.0 });

        var modes = new[] { MakeMode(0, v1), MakeMode(1, v2) };
        double defect = OrthogonalityChecker.MaxOrthogonalityDefect(modes, massOp);
        Assert.True(defect < 1e-15);
    }

    [Fact]
    public void WithMassOperator_NormalizationDefect()
    {
        // v = (1, 0) with M = diag(2, 3) → v^T M v = 2, defect = |2 - 1| = 1
        var v = new double[] { 1, 0 };
        var massOp = new TestHelpers.DiagonalOperator(new double[] { 2.0, 3.0 });

        var modes = new[] { MakeMode(0, v) };
        double defect = OrthogonalityChecker.MaxNormalizationDefect(modes, massOp);
        Assert.True(System.Math.Abs(defect - 1.0) < 1e-10);
    }

    [Fact]
    public void GramMatrix_OrthonormalBasis()
    {
        var e1 = new double[] { 1, 0, 0 };
        var e2 = new double[] { 0, 1, 0 };

        var modes = new[] { MakeMode(0, e1), MakeMode(1, e2) };
        var gram = OrthogonalityChecker.GramMatrix(modes);

        Assert.Equal(1.0, gram[0, 0], 12);
        Assert.Equal(0.0, gram[0, 1], 12);
        Assert.Equal(0.0, gram[1, 0], 12);
        Assert.Equal(1.0, gram[1, 1], 12);
    }

    [Fact]
    public void SingleMode_ZeroOrthogonalityDefect()
    {
        var v = new double[] { 1, 0, 0 };
        var modes = new[] { MakeMode(0, v) };
        Assert.Equal(0.0, OrthogonalityChecker.MaxOrthogonalityDefect(modes));
    }
}
