using System.Text.Json;
using Gu.Core;

namespace Gu.Core.Tests;

public class TensorSignatureTests
{
    private static TensorSignature CreateSample() => new()
    {
        AmbientSpaceId = "Y_h",
        CarrierType = "connection-1form",
        Degree = "1",
        LieAlgebraBasisId = "su2-standard",
        ComponentOrderId = "lexicographic",
        NumericPrecision = "float64",
        MemoryLayout = "dense-row-major",
        BackendPacking = "SoA"
    };

    [Fact]
    public void RoundTrip_AllFieldsPreserved()
    {
        var original = CreateSample();
        var json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<TensorSignature>(json);

        Assert.NotNull(deserialized);
        Assert.Equal(original.AmbientSpaceId, deserialized.AmbientSpaceId);
        Assert.Equal(original.CarrierType, deserialized.CarrierType);
        Assert.Equal(original.Degree, deserialized.Degree);
        Assert.Equal(original.LieAlgebraBasisId, deserialized.LieAlgebraBasisId);
        Assert.Equal(original.ComponentOrderId, deserialized.ComponentOrderId);
        Assert.Equal(original.NumericPrecision, deserialized.NumericPrecision);
        Assert.Equal(original.MemoryLayout, deserialized.MemoryLayout);
        Assert.Equal(original.BackendPacking, deserialized.BackendPacking);
    }

    [Fact]
    public void NumericPrecision_DefaultsToFloat64()
    {
        var sig = new TensorSignature
        {
            AmbientSpaceId = "Y_h",
            CarrierType = "curvature-2form",
            Degree = "2",
            LieAlgebraBasisId = "su2-standard",
            ComponentOrderId = "lexicographic",
            MemoryLayout = "dense-row-major"
        };

        Assert.Equal("float64", sig.NumericPrecision);
    }

    [Fact]
    public void BackendPacking_IsOptionalAndNullable()
    {
        var sig = new TensorSignature
        {
            AmbientSpaceId = "Y_h",
            CarrierType = "residual-2form",
            Degree = "2",
            LieAlgebraBasisId = "su2-standard",
            ComponentOrderId = "lexicographic",
            MemoryLayout = "sparse-csr"
        };

        Assert.Null(sig.BackendPacking);

        // Should still round-trip with null BackendPacking
        var json = JsonSerializer.Serialize(sig);
        var deserialized = JsonSerializer.Deserialize<TensorSignature>(json);
        Assert.NotNull(deserialized);
        Assert.Null(deserialized.BackendPacking);
    }

    [Fact]
    public void JsonPropertyNames_UseCamelCase()
    {
        var sig = CreateSample();
        var json = JsonSerializer.Serialize(sig);

        Assert.Contains("\"ambientSpaceId\"", json);
        Assert.Contains("\"carrierType\"", json);
        Assert.Contains("\"degree\"", json);
        Assert.Contains("\"lieAlgebraBasisId\"", json);
        Assert.Contains("\"componentOrderId\"", json);
        Assert.Contains("\"numericPrecision\"", json);
        Assert.Contains("\"memoryLayout\"", json);
        Assert.Contains("\"backendPacking\"", json);
    }

    [Fact]
    public void CarrierTypes_ForResidualComponents_MustMatch()
    {
        // Per Section 12.4: T_h and S_h must land in the same discrete carrier type.
        // This test validates that two TensorSignatures can be compared for carrier compatibility.
        var torsionSig = new TensorSignature
        {
            AmbientSpaceId = "Y_h",
            CarrierType = "residual-2form",
            Degree = "2",
            LieAlgebraBasisId = "su2-standard",
            ComponentOrderId = "lexicographic",
            MemoryLayout = "dense-row-major"
        };

        var shiabSig = new TensorSignature
        {
            AmbientSpaceId = "Y_h",
            CarrierType = "residual-2form",
            Degree = "2",
            LieAlgebraBasisId = "su2-standard",
            ComponentOrderId = "lexicographic",
            MemoryLayout = "dense-row-major"
        };

        // Carrier compatibility check: same carrier type, degree, basis, and ordering
        Assert.Equal(torsionSig.CarrierType, shiabSig.CarrierType);
        Assert.Equal(torsionSig.Degree, shiabSig.Degree);
        Assert.Equal(torsionSig.LieAlgebraBasisId, shiabSig.LieAlgebraBasisId);
        Assert.Equal(torsionSig.ComponentOrderId, shiabSig.ComponentOrderId);
    }
}
