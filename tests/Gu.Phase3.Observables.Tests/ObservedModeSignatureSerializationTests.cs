using System.Text.Json;
using Gu.Core;
using Gu.Phase3.Observables;

namespace Gu.Phase3.Observables.Tests;

public class ObservedModeSignatureSerializationTests
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
    public void RoundTrip_PreservesAllFields()
    {
        var original = MakeSig("m0", new double[] { 1.0, 2.0, 3.0 });

        var json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<ObservedModeSignature>(json);

        Assert.NotNull(deserialized);
        Assert.Equal(original.ModeId, deserialized!.ModeId);
        Assert.Equal(original.BackgroundId, deserialized.BackgroundId);
        Assert.Equal(original.ObservedCoefficients, deserialized.ObservedCoefficients);
        Assert.Equal(original.ObservedShape, deserialized.ObservedShape);
        Assert.Equal(original.LinearizationMethod, deserialized.LinearizationMethod);
        Assert.Equal(original.SignatureHash, deserialized.SignatureHash);
    }

    [Fact]
    public void RoundTrip_WithObservedMoments()
    {
        var original = new ObservedModeSignature
        {
            ModeId = "m1",
            BackgroundId = "bg-1",
            ObservedCoefficients = new double[] { 1.0, -1.0 },
            ObservedSignature = new TensorSignature
            {
                AmbientSpaceId = "X_h",
                CarrierType = "curvature-2form",
                Degree = "2",
                LieAlgebraBasisId = "standard",
                ComponentOrderId = "face-major",
                MemoryLayout = "dense-row-major",
            },
            ObservedShape = new[] { 2 },
            LinearizationMethod = LinearizationMethod.FiniteDifference,
            ObservedMoments = new double[] { 0.0, 1.0, 0.5 },
        };

        var json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<ObservedModeSignature>(json);

        Assert.NotNull(deserialized);
        Assert.Equal(original.ObservedMoments, deserialized!.ObservedMoments);
    }

    [Fact]
    public void SignatureHash_IsDeterministic()
    {
        var a = MakeSig("m0", new double[] { 1.0, 2.0, 3.0 });
        var b = MakeSig("m0", new double[] { 1.0, 2.0, 3.0 });

        Assert.Equal(a.SignatureHash, b.SignatureHash);
    }

    [Fact]
    public void SignatureHash_DiffersForDifferentCoefficients()
    {
        var a = MakeSig("m0", new double[] { 1.0, 2.0, 3.0 });
        var b = MakeSig("m0", new double[] { 1.0, 2.0, 3.001 });

        Assert.NotEqual(a.SignatureHash, b.SignatureHash);
    }

    [Fact]
    public void L2Norm_ComputedCorrectly()
    {
        var sig = MakeSig("m0", new double[] { 3.0, 4.0 });
        Assert.Equal(5.0, sig.L2Norm, 12);
    }
}
