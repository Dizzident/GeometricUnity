using Gu.Core;
using Gu.Phase3.Observables;

namespace Gu.Phase3.Observables.Tests;

public class ObservedOverlapComputerTests
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
    public void Compute_OrthogonalModes_ZeroOffDiagonal()
    {
        var sigs = new[]
        {
            MakeSig("m0", new double[] { 1, 0 }),
            MakeSig("m1", new double[] { 0, 1 }),
        };

        var result = ObservedOverlapComputer.Compute(sigs);

        Assert.Equal(2, result.ModeIds.Length);
        Assert.Equal(0.0, result.MaxOffDiagonalOverlap, 12);
    }

    [Fact]
    public void Compute_IdenticalModes_MaxOverlapIsOne()
    {
        var sigs = new[]
        {
            MakeSig("m0", new double[] { 1, 0 }),
            MakeSig("m1", new double[] { 1, 0 }),
        };

        var result = ObservedOverlapComputer.Compute(sigs);

        Assert.Equal(1.0, result.MaxOffDiagonalOverlap, 12);
        Assert.NotNull(result.MaxOverlapPair);
        Assert.Equal("m0", result.MaxOverlapPair!.Value.A);
        Assert.Equal("m1", result.MaxOverlapPair!.Value.B);
    }

    [Fact]
    public void Compute_SingleMode_NoOverlapPair()
    {
        var sigs = new[] { MakeSig("m0", new double[] { 1, 0 }) };

        var result = ObservedOverlapComputer.Compute(sigs);

        Assert.Single(result.ModeIds);
        Assert.Null(result.MaxOverlapPair);
    }

    [Fact]
    public void Compute_ModeIdsPreserved()
    {
        var sigs = new[]
        {
            MakeSig("alpha", new double[] { 1, 0 }),
            MakeSig("beta", new double[] { 0, 1 }),
            MakeSig("gamma", new double[] { 1, 1 }),
        };

        var result = ObservedOverlapComputer.Compute(sigs);

        Assert.Equal(new[] { "alpha", "beta", "gamma" }, result.ModeIds);
    }
}
