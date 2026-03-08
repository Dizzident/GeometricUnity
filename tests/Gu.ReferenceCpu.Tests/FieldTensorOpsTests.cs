using Gu.Core;
using Gu.ReferenceCpu;

namespace Gu.ReferenceCpu.Tests;

public class FieldTensorOpsTests
{
    private static TensorSignature ResidualSignature() => new()
    {
        AmbientSpaceId = "Y_h",
        CarrierType = "curvature-2form",
        Degree = "2",
        LieAlgebraBasisId = "canonical",
        ComponentOrderId = "face-major",
        MemoryLayout = "dense-row-major",
    };

    private static FieldTensor MakeTensor(string label, double[] coeffs) => new()
    {
        Label = label,
        Signature = ResidualSignature(),
        Coefficients = coeffs,
        Shape = new[] { coeffs.Length },
    };

    [Fact]
    public void Add_ElementWise()
    {
        var a = MakeTensor("a", new[] { 1.0, 2.0, 3.0 });
        var b = MakeTensor("b", new[] { 10.0, 20.0, 30.0 });

        var result = FieldTensorOps.Add(a, b);

        Assert.Equal(11.0, result.Coefficients[0], 12);
        Assert.Equal(22.0, result.Coefficients[1], 12);
        Assert.Equal(33.0, result.Coefficients[2], 12);
    }

    [Fact]
    public void Subtract_ElementWise()
    {
        var a = MakeTensor("a", new[] { 10.0, 20.0, 30.0 });
        var b = MakeTensor("b", new[] { 1.0, 2.0, 3.0 });

        var result = FieldTensorOps.Subtract(a, b);

        Assert.Equal(9.0, result.Coefficients[0], 12);
        Assert.Equal(18.0, result.Coefficients[1], 12);
        Assert.Equal(27.0, result.Coefficients[2], 12);
    }

    [Fact]
    public void Scale_ByScalar()
    {
        var a = MakeTensor("a", new[] { 1.0, 2.0, 3.0 });

        var result = FieldTensorOps.Scale(a, 5.0);

        Assert.Equal(5.0, result.Coefficients[0], 12);
        Assert.Equal(10.0, result.Coefficients[1], 12);
        Assert.Equal(15.0, result.Coefficients[2], 12);
    }

    [Fact]
    public void AddScaled_FusedOperation()
    {
        var a = MakeTensor("a", new[] { 1.0, 2.0, 3.0 });
        var b = MakeTensor("b", new[] { 10.0, 20.0, 30.0 });

        var result = FieldTensorOps.AddScaled(a, b, 0.5);

        Assert.Equal(6.0, result.Coefficients[0], 12);
        Assert.Equal(12.0, result.Coefficients[1], 12);
        Assert.Equal(18.0, result.Coefficients[2], 12);
    }

    [Fact]
    public void Dot_Product()
    {
        var a = MakeTensor("a", new[] { 1.0, 2.0, 3.0 });
        var b = MakeTensor("b", new[] { 4.0, 5.0, 6.0 });

        double dot = FieldTensorOps.Dot(a, b);

        Assert.Equal(32.0, dot, 12); // 1*4 + 2*5 + 3*6 = 32
    }

    [Fact]
    public void L2Norm_Correct()
    {
        var a = MakeTensor("a", new[] { 3.0, 4.0 });

        double norm = FieldTensorOps.L2Norm(a);

        Assert.Equal(5.0, norm, 12); // sqrt(9 + 16) = 5
    }

    [Fact]
    public void ZerosLike_CorrectShapeAndSignature()
    {
        var a = MakeTensor("a", new[] { 1.0, 2.0, 3.0 });

        var zero = FieldTensorOps.ZerosLike(a);

        Assert.Equal(3, zero.Coefficients.Length);
        Assert.All(zero.Coefficients, c => Assert.Equal(0.0, c, 12));
        Assert.Equal(a.Signature.CarrierType, zero.Signature.CarrierType);
    }

    [Fact]
    public void Zeros_WithCustomLabel()
    {
        var sig = ResidualSignature();
        var zero = FieldTensorOps.Zeros("T_h", sig, new[] { 4, 3 });

        Assert.Equal("T_h", zero.Label);
        Assert.Equal(12, zero.Coefficients.Length);
        Assert.All(zero.Coefficients, c => Assert.Equal(0.0, c, 12));
    }

    [Fact]
    public void IncompatibleCarrierType_ThrowsOnArithmetic()
    {
        var a = MakeTensor("a", new[] { 1.0 });
        var b = new FieldTensor
        {
            Label = "b",
            Signature = new TensorSignature
            {
                AmbientSpaceId = "Y_h",
                CarrierType = "connection-1form", // different carrier
                Degree = "1",
                LieAlgebraBasisId = "canonical",
                ComponentOrderId = "edge-major",
                MemoryLayout = "dense-row-major",
            },
            Coefficients = new[] { 1.0 },
            Shape = new[] { 1 },
        };

        Assert.Throws<InvalidOperationException>(() => FieldTensorOps.Add(a, b));
        Assert.Throws<InvalidOperationException>(() => FieldTensorOps.Subtract(a, b));
        Assert.Throws<InvalidOperationException>(() => FieldTensorOps.Dot(a, b));
    }

    [Fact]
    public void IncompatibleLength_ThrowsOnArithmetic()
    {
        var a = MakeTensor("a", new[] { 1.0, 2.0 });
        var b = MakeTensor("b", new[] { 1.0, 2.0, 3.0 });

        Assert.Throws<InvalidOperationException>(() => FieldTensorOps.Add(a, b));
    }

    [Fact]
    public void Subtract_PreservesSignature()
    {
        var a = MakeTensor("a", new[] { 5.0 });
        var b = MakeTensor("b", new[] { 3.0 });

        var result = FieldTensorOps.Subtract(a, b);

        Assert.Equal("curvature-2form", result.Signature.CarrierType);
        Assert.Equal("Y_h", result.Signature.AmbientSpaceId);
    }
}
