using Gu.Core;
using Gu.Interop;

namespace Gu.Interop.Tests;

public class FieldPackerTests
{
    private static TensorSignature CreateSignature() => new()
    {
        AmbientSpaceId = "Y_h",
        CarrierType = "connection-1form",
        Degree = "1",
        LieAlgebraBasisId = "basis-standard",
        ComponentOrderId = "order-row-major",
        MemoryLayout = "dense-row-major",
        NumericPrecision = "float64",
    };

    [Fact]
    public void PackToSoA_ScalarField_NoCopy()
    {
        var field = new FieldTensor
        {
            Label = "scalar",
            Signature = CreateSignature(),
            Coefficients = new double[] { 1.0, 2.0, 3.0 },
            Shape = new[] { 3 },
        };

        var packed = FieldPacker.PackToSoA(field);

        Assert.Equal(3, packed.Length);
        Assert.Equal(1.0, packed[0]);
        Assert.Equal(2.0, packed[1]);
        Assert.Equal(3.0, packed[2]);
    }

    [Fact]
    public void PackToSoA_MultiComponentField_Transposes()
    {
        // FieldTensor shape [3, 2]: 3 vertices, 2 components
        // Row-major: [v0c0, v0c1, v1c0, v1c1, v2c0, v2c1]
        var field = new FieldTensor
        {
            Label = "omega",
            Signature = CreateSignature(),
            Coefficients = new double[] { 1.0, 2.0, 3.0, 4.0, 5.0, 6.0 },
            Shape = new[] { 3, 2 },
        };

        var packed = FieldPacker.PackToSoA(field);

        // SoA: [c0v0, c0v1, c0v2, c1v0, c1v1, c1v2]
        Assert.Equal(6, packed.Length);
        Assert.Equal(1.0, packed[0]); // c0, v0
        Assert.Equal(3.0, packed[1]); // c0, v1
        Assert.Equal(5.0, packed[2]); // c0, v2
        Assert.Equal(2.0, packed[3]); // c1, v0
        Assert.Equal(4.0, packed[4]); // c1, v1
        Assert.Equal(6.0, packed[5]); // c1, v2
    }

    [Fact]
    public void RoundTrip_PackUnpack_PreservesData()
    {
        var original = new FieldTensor
        {
            Label = "omega",
            Signature = CreateSignature(),
            Coefficients = new double[] { 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0, 10.0, 11.0, 12.0 },
            Shape = new[] { 4, 3 },
        };

        var packed = FieldPacker.PackToSoA(original);
        var restored = FieldPacker.UnpackFromSoA(packed, "omega", CreateSignature(), new[] { 4, 3 });

        Assert.Equal(original.Coefficients.Length, restored.Coefficients.Length);
        for (int i = 0; i < original.Coefficients.Length; i++)
        {
            Assert.Equal(original.Coefficients[i], restored.Coefficients[i], precision: 15);
        }
    }

    [Fact]
    public void CreateLayout_ScalarField_OneSoAComponent()
    {
        var field = new FieldTensor
        {
            Label = "scalar",
            Signature = CreateSignature(),
            Coefficients = new double[] { 1.0, 2.0, 3.0 },
            Shape = new[] { 3 },
        };

        var layout = FieldPacker.CreateLayout(field, "test-layout");

        Assert.Equal("SoA", layout.PackingMode);
        Assert.Equal("float64", layout.NumericType);
        Assert.Equal(3, layout.TotalElements);
        Assert.Single(layout.Components);
        Assert.Equal("scalar_0", layout.Components[0].ComponentName);
    }

    [Fact]
    public void CreateLayout_MultiComponentField_MultipleSoAComponents()
    {
        var field = new FieldTensor
        {
            Label = "omega",
            Signature = CreateSignature(),
            Coefficients = new double[] { 1.0, 2.0, 3.0, 4.0, 5.0, 6.0 },
            Shape = new[] { 3, 2 },
        };

        var layout = FieldPacker.CreateLayout(field, "omega-layout");

        Assert.Equal("SoA", layout.PackingMode);
        Assert.Equal(6, layout.TotalElements);
        Assert.Equal(2, layout.Components.Count);
        Assert.Equal("omega_0", layout.Components[0].ComponentName);
        Assert.Equal("omega_1", layout.Components[1].ComponentName);
    }

    [Fact]
    public void BufferLayoutDescriptor_CreateSoA_CorrectOffsets()
    {
        var layout = BufferLayoutDescriptor.CreateSoA(
            "test",
            new[] { "a", "b", "c" },
            100,
            "float64");

        Assert.Equal(300, layout.TotalElements);
        Assert.Equal(3, layout.Components.Count);
        Assert.Equal(0, layout.Components[0].Offset);
        Assert.Equal(800, layout.Components[1].Offset);   // 100 * 8 bytes
        Assert.Equal(1600, layout.Components[2].Offset);  // 200 * 8 bytes
        Assert.Equal(100, layout.Components[0].Count);
        Assert.Equal(8, layout.Components[0].Stride);
    }
}
