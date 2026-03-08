using Gu.Core;
using Gu.Core.Serialization;

namespace Gu.Core.Tests;

public class FieldTensorTests
{
    private static TensorSignature ConnectionSignature() => new()
    {
        AmbientSpaceId = "Y_h",
        CarrierType = "connection-1form",
        Degree = "1",
        LieAlgebraBasisId = "su2-standard",
        ComponentOrderId = "lexicographic",
        MemoryLayout = "dense-row-major"
    };

    [Fact]
    public void RoundTrip_AllFieldsPreserved()
    {
        var original = new FieldTensor
        {
            Label = "omega_h",
            Signature = ConnectionSignature(),
            Coefficients = new double[] { 1.0, 2.0, 3.0, 4.0, 5.0, 6.0 },
            Shape = new[] { 2, 3 }
        };

        var json = GuJsonDefaults.Serialize(original);
        var deserialized = GuJsonDefaults.Deserialize<FieldTensor>(json);

        Assert.NotNull(deserialized);
        Assert.Equal(original.Label, deserialized.Label);
        Assert.Equal(original.Coefficients, deserialized.Coefficients);
        Assert.Equal(original.Shape, deserialized.Shape);
        Assert.Equal(original.Signature.CarrierType, deserialized.Signature.CarrierType);
    }

    [Fact]
    public void CoefficientsLength_MatchesShapeProduct()
    {
        var ft = new FieldTensor
        {
            Label = "F_h",
            Signature = new TensorSignature
            {
                AmbientSpaceId = "Y_h",
                CarrierType = "curvature-2form",
                Degree = "2",
                LieAlgebraBasisId = "su2-standard",
                ComponentOrderId = "lexicographic",
                MemoryLayout = "dense-row-major"
            },
            Coefficients = new double[24],
            Shape = new[] { 4, 6 }  // 4 patches x 6 curvature components
        };

        int expectedLength = 1;
        foreach (var dim in ft.Shape) expectedLength *= dim;

        Assert.Equal(expectedLength, ft.Coefficients.Length);
    }

    [Fact]
    public void EmptyCoefficients_RoundTrips()
    {
        var ft = new FieldTensor
        {
            Label = "zero_field",
            Signature = ConnectionSignature(),
            Coefficients = Array.Empty<double>(),
            Shape = new[] { 0 }
        };

        var json = GuJsonDefaults.Serialize(ft);
        var deserialized = GuJsonDefaults.Deserialize<FieldTensor>(json);

        Assert.NotNull(deserialized);
        Assert.Empty(deserialized.Coefficients);
    }
}
