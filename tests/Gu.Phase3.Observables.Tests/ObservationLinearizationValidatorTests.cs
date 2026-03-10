using Gu.Core;
using Gu.Phase3.Observables;

namespace Gu.Phase3.Observables.Tests;

public class ObservationLinearizationValidatorTests
{
    private static ObservedModeSignature MakeSig(string modeId, double[] coeffs, LinearizationMethod method) => new()
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
        LinearizationMethod = method,
    };

    [Fact]
    public void RelativeError_IdenticalSignatures_IsZero()
    {
        var analytic = MakeSig("m0", new double[] { 1, 2, 3 }, LinearizationMethod.Analytic);
        var fd = MakeSig("m0", new double[] { 1, 2, 3 }, LinearizationMethod.FiniteDifference);

        Assert.Equal(0.0, ObservationLinearizationValidator.RelativeError(analytic, fd), 12);
    }

    [Fact]
    public void RelativeError_SmallDifference()
    {
        var analytic = MakeSig("m0", new double[] { 1.0, 0.0 }, LinearizationMethod.Analytic);
        var fd = MakeSig("m0", new double[] { 1.001, 0.0 }, LinearizationMethod.FiniteDifference);

        double error = ObservationLinearizationValidator.RelativeError(analytic, fd);
        Assert.True(error < 0.01, $"Expected small error, got {error:E6}");
    }

    [Fact]
    public void RelativeError_LargeDifference()
    {
        var analytic = MakeSig("m0", new double[] { 1.0, 0.0 }, LinearizationMethod.Analytic);
        var fd = MakeSig("m0", new double[] { 0.0, 1.0 }, LinearizationMethod.FiniteDifference);

        double error = ObservationLinearizationValidator.RelativeError(analytic, fd);
        Assert.True(error > 1.0, $"Expected large error, got {error:E6}");
    }

    [Fact]
    public void Validate_BatchReport()
    {
        var analytic = new[]
        {
            MakeSig("m0", new double[] { 1.0, 0.0 }, LinearizationMethod.Analytic),
            MakeSig("m1", new double[] { 0.0, 1.0 }, LinearizationMethod.Analytic),
        };
        var fd = new[]
        {
            MakeSig("m0", new double[] { 1.001, 0.0 }, LinearizationMethod.FiniteDifference),
            MakeSig("m1", new double[] { 0.0, 0.999 }, LinearizationMethod.FiniteDifference),
        };

        var report = ObservationLinearizationValidator.Validate(analytic, fd);

        Assert.Equal(2, report.Entries.Count);
        Assert.True(report.MaxRelativeError < 0.01);
        Assert.True(report.MeanRelativeError < 0.01);
        Assert.Equal("m0", report.Entries[0].ModeId);
        Assert.Equal("m1", report.Entries[1].ModeId);
    }

    [Fact]
    public void Validate_ReportsMaxCorrectly()
    {
        var analytic = new[]
        {
            MakeSig("m0", new double[] { 1.0 }, LinearizationMethod.Analytic),
            MakeSig("m1", new double[] { 1.0 }, LinearizationMethod.Analytic),
        };
        var fd = new[]
        {
            MakeSig("m0", new double[] { 1.001 }, LinearizationMethod.FiniteDifference),
            MakeSig("m1", new double[] { 2.0 }, LinearizationMethod.FiniteDifference), // big difference
        };

        var report = ObservationLinearizationValidator.Validate(analytic, fd);
        Assert.True(report.MaxRelativeError > 0.4); // mode-1 has large error
        Assert.True(report.MaxRelativeError >= report.MeanRelativeError);
    }
}
