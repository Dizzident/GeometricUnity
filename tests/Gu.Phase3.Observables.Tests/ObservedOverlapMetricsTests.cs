using Gu.Core;
using Gu.Phase3.Observables;

namespace Gu.Phase3.Observables.Tests;

public class ObservedOverlapMetricsTests
{
    private static ObservedModeSignature MakeSig(string modeId, double[] coeffs) => new()
    {
        ModeId = modeId,
        BackgroundId = "test-bg",
        ObservedCoefficients = coeffs,
        ObservedSignature = new TensorSignature
        {
            AmbientSpaceId = "X_h",
            CarrierType = "curvature-2form",
            Degree = "2",
            LieAlgebraBasisId = "standard",
            ComponentOrderId = "face-major",
            MemoryLayout = "dense-row-major",
        },
        ObservedShape = new[] { coeffs.Length },
        LinearizationMethod = LinearizationMethod.Analytic,
    };

    [Fact]
    public void L2Overlap_IdenticalDirections_IsOne()
    {
        var a = MakeSig("m0", new double[] { 1, 0, 0 });
        var b = MakeSig("m1", new double[] { 2, 0, 0 }); // same direction, different magnitude

        double overlap = ObservedOverlapMetrics.L2Overlap(a, b);
        Assert.Equal(1.0, overlap, 12);
    }

    [Fact]
    public void L2Overlap_OrthogonalDirections_IsZero()
    {
        var a = MakeSig("m0", new double[] { 1, 0, 0 });
        var b = MakeSig("m1", new double[] { 0, 1, 0 });

        double overlap = ObservedOverlapMetrics.L2Overlap(a, b);
        Assert.Equal(0.0, overlap, 12);
    }

    [Fact]
    public void L2Overlap_PartialOverlap()
    {
        var a = MakeSig("m0", new double[] { 1, 0 });
        var b = MakeSig("m1", new double[] { 1, 1 });

        double overlap = ObservedOverlapMetrics.L2Overlap(a, b);
        // |1*1 + 0*1| / (1 * sqrt(2)) = 1/sqrt(2)
        Assert.Equal(1.0 / System.Math.Sqrt(2.0), overlap, 10);
    }

    [Fact]
    public void L2Overlap_ZeroVector_IsZero()
    {
        var a = MakeSig("m0", new double[] { 0, 0, 0 });
        var b = MakeSig("m1", new double[] { 1, 0, 0 });

        Assert.Equal(0.0, ObservedOverlapMetrics.L2Overlap(a, b));
    }

    [Fact]
    public void RelativeL2Distance_IdenticalVectors_IsZero()
    {
        var a = MakeSig("m0", new double[] { 1, 2, 3 });
        var b = MakeSig("m1", new double[] { 1, 2, 3 });

        Assert.Equal(0.0, ObservedOverlapMetrics.RelativeL2Distance(a, b), 12);
    }

    [Fact]
    public void RelativeL2Distance_DifferentVectors_IsPositive()
    {
        var a = MakeSig("m0", new double[] { 1, 0 });
        var b = MakeSig("m1", new double[] { 0, 1 });

        double dist = ObservedOverlapMetrics.RelativeL2Distance(a, b);
        // ||[1,-1]|| / max(1, 1) = sqrt(2)
        Assert.Equal(System.Math.Sqrt(2.0), dist, 10);
    }

    [Fact]
    public void OverlapMatrix_DiagonalIsOne()
    {
        var sigs = new[]
        {
            MakeSig("m0", new double[] { 1, 0 }),
            MakeSig("m1", new double[] { 0, 1 }),
        };

        var matrix = ObservedOverlapMetrics.OverlapMatrix(sigs);
        Assert.Equal(1.0, matrix[0, 0], 12);
        Assert.Equal(1.0, matrix[1, 1], 12);
        Assert.Equal(0.0, matrix[0, 1], 12);
    }

    [Fact]
    public void OverlapMatrix_IsSymmetric()
    {
        var sigs = new[]
        {
            MakeSig("m0", new double[] { 1, 1 }),
            MakeSig("m1", new double[] { 1, 0 }),
            MakeSig("m2", new double[] { 0, 1 }),
        };

        var matrix = ObservedOverlapMetrics.OverlapMatrix(sigs);
        for (int i = 0; i < 3; i++)
            for (int j = 0; j < 3; j++)
                Assert.Equal(matrix[i, j], matrix[j, i], 12);
    }
}
