using Gu.Core;
using Gu.ReferenceCpu;

namespace Gu.Core.Tests;

public class FieldTensorOpsTests
{
    private static FieldTensor Create(string label, double[] coeffs)
    {
        return new FieldTensor
        {
            Label = label,
            Signature = new TensorSignature
            {
                AmbientSpaceId = "Y_h",
                CarrierType = "test-carrier",
                Degree = "0",
                LieAlgebraBasisId = "canonical",
                ComponentOrderId = "natural",
                MemoryLayout = "dense-row-major",
            },
            Coefficients = coeffs,
            Shape = new[] { coeffs.Length },
        };
    }

    [Fact]
    public void Add_ElementWise()
    {
        var a = Create("a", new double[] { 1, 2, 3 });
        var b = Create("b", new double[] { 4, 5, 6 });

        var result = FieldTensorOps.Add(a, b);

        Assert.Equal(new double[] { 5, 7, 9 }, result.Coefficients);
    }

    [Fact]
    public void Subtract_ElementWise()
    {
        var a = Create("a", new double[] { 5, 7, 9 });
        var b = Create("b", new double[] { 1, 2, 3 });

        var result = FieldTensorOps.Subtract(a, b);

        Assert.Equal(new double[] { 4, 5, 6 }, result.Coefficients);
    }

    [Fact]
    public void Scale_AllElements()
    {
        var a = Create("a", new double[] { 2, 4, 6 });

        var result = FieldTensorOps.Scale(a, 0.5);

        Assert.Equal(new double[] { 1, 2, 3 }, result.Coefficients);
    }

    [Fact]
    public void AddScaled_FusedOperation()
    {
        var a = Create("a", new double[] { 1, 2, 3 });
        var b = Create("b", new double[] { 10, 20, 30 });

        var result = FieldTensorOps.AddScaled(a, b, 0.1);

        Assert.Equal(new double[] { 2, 4, 6 }, result.Coefficients);
    }

    [Fact]
    public void Dot_InnerProduct()
    {
        var a = Create("a", new double[] { 1, 2, 3 });
        var b = Create("b", new double[] { 4, 5, 6 });

        double dot = FieldTensorOps.Dot(a, b);

        Assert.Equal(32.0, dot, 12); // 1*4 + 2*5 + 3*6 = 32
    }

    [Fact]
    public void L2Norm_OfVector()
    {
        var a = Create("a", new double[] { 3, 4 });

        double norm = FieldTensorOps.L2Norm(a);

        Assert.Equal(5.0, norm, 12);
    }

    [Fact]
    public void ZerosLike_CreatesMatchingZero()
    {
        var template = Create("template", new double[] { 1, 2, 3 });

        var zero = FieldTensorOps.ZerosLike(template);

        Assert.Equal(3, zero.Coefficients.Length);
        Assert.All(zero.Coefficients, c => Assert.Equal(0.0, c));
        Assert.Equal(template.Signature.CarrierType, zero.Signature.CarrierType);
    }

    [Fact]
    public void Zeros_CreatesFromShape()
    {
        var sig = new TensorSignature
        {
            AmbientSpaceId = "Y_h",
            CarrierType = "test",
            Degree = "0",
            LieAlgebraBasisId = "canonical",
            ComponentOrderId = "natural",
            MemoryLayout = "dense-row-major",
        };

        var zero = FieldTensorOps.Zeros("test-zero", sig, new[] { 3, 4 });

        Assert.Equal(12, zero.Coefficients.Length);
        Assert.All(zero.Coefficients, c => Assert.Equal(0.0, c));
    }

    [Fact]
    public void Subtract_DifferentCarrier_Throws()
    {
        var a = Create("a", new double[] { 1, 2, 3 });
        var b = new FieldTensor
        {
            Label = "b",
            Signature = new TensorSignature
            {
                AmbientSpaceId = "Y_h",
                CarrierType = "different-carrier",
                Degree = "0",
                LieAlgebraBasisId = "canonical",
                ComponentOrderId = "natural",
                MemoryLayout = "dense-row-major",
            },
            Coefficients = new double[] { 1, 2, 3 },
            Shape = new[] { 3 },
        };

        Assert.Throws<InvalidOperationException>(() =>
            FieldTensorOps.Subtract(a, b));
    }

    [Fact]
    public void Add_DifferentLength_Throws()
    {
        var a = Create("a", new double[] { 1, 2, 3 });
        var b = Create("b", new double[] { 1, 2 });

        Assert.Throws<InvalidOperationException>(() =>
            FieldTensorOps.Add(a, b));
    }

    [Fact]
    public void Operations_PreserveSignature()
    {
        var a = Create("a", new double[] { 1, 2, 3 });
        var b = Create("b", new double[] { 4, 5, 6 });

        var sum = FieldTensorOps.Add(a, b);
        var diff = FieldTensorOps.Subtract(a, b);
        var scaled = FieldTensorOps.Scale(a, 2.0);

        Assert.Equal("Y_h", sum.Signature.AmbientSpaceId);
        Assert.Equal("test-carrier", diff.Signature.CarrierType);
        Assert.Equal("dense-row-major", scaled.Signature.MemoryLayout);
    }
}
