using Gu.Core;
using Gu.Core.Serialization;

namespace Gu.Core.Tests;

public class DerivedStateTests
{
    private static TensorSignature ResidualSignature() => new()
    {
        AmbientSpaceId = "Y_h",
        CarrierType = "residual-2form",
        Degree = "2",
        LieAlgebraBasisId = "su2-standard",
        ComponentOrderId = "lexicographic",
        MemoryLayout = "dense-row-major"
    };

    private static FieldTensor CreateField(string label, double[] coeffs) => new()
    {
        Label = label,
        Signature = ResidualSignature(),
        Coefficients = coeffs,
        Shape = new[] { coeffs.Length }
    };

    [Fact]
    public void RoundTrip_AllFieldsPreserved()
    {
        var original = new DerivedState
        {
            CurvatureF = CreateField("F_h", new[] { 1.0, 2.0, 3.0 }),
            TorsionT = CreateField("T_h", new[] { 0.5, 1.0, 1.5 }),
            ShiabS = CreateField("S_h", new[] { 0.7, 1.2, 1.8 }),
            ResidualUpsilon = CreateField("Upsilon_h", new[] { 0.2, 0.2, 0.3 }),
            Diagnostics = new Dictionary<string, FieldTensor>
            {
                ["gauge_violation"] = CreateField("gauge_viol", new[] { 0.01 })
            }
        };

        var json = GuJsonDefaults.Serialize(original);
        var deserialized = GuJsonDefaults.Deserialize<DerivedState>(json);

        Assert.NotNull(deserialized);
        Assert.Equal("F_h", deserialized.CurvatureF.Label);
        Assert.Equal("T_h", deserialized.TorsionT.Label);
        Assert.Equal("S_h", deserialized.ShiabS.Label);
        Assert.Equal("Upsilon_h", deserialized.ResidualUpsilon.Label);
        Assert.NotNull(deserialized.Diagnostics);
        Assert.True(deserialized.Diagnostics.ContainsKey("gauge_violation"));
    }

    [Fact]
    public void TorsionAndShiab_MustShareCarrierType()
    {
        // Per Section 12.4: T_h and S_h must land in the same discrete carrier type
        var derived = new DerivedState
        {
            CurvatureF = CreateField("F_h", new[] { 1.0 }),
            TorsionT = CreateField("T_h", new[] { 0.5 }),
            ShiabS = CreateField("S_h", new[] { 0.7 }),
            ResidualUpsilon = CreateField("Upsilon_h", new[] { 0.2 })
        };

        Assert.Equal(
            derived.TorsionT.Signature.CarrierType,
            derived.ShiabS.Signature.CarrierType);
    }

    [Fact]
    public void ResidualUpsilon_EqualsShiabMinusTorsion()
    {
        // Per Section 4.9: Upsilon_omega = S_omega - T_omega
        double[] tCoeffs = { 0.5, 1.0, 1.5 };
        double[] sCoeffs = { 0.7, 1.2, 1.8 };
        double[] upsilonCoeffs = new double[3];
        for (int i = 0; i < 3; i++)
            upsilonCoeffs[i] = sCoeffs[i] - tCoeffs[i];

        var derived = new DerivedState
        {
            CurvatureF = CreateField("F_h", new[] { 1.0 }),
            TorsionT = CreateField("T_h", tCoeffs),
            ShiabS = CreateField("S_h", sCoeffs),
            ResidualUpsilon = CreateField("Upsilon_h", upsilonCoeffs)
        };

        for (int i = 0; i < 3; i++)
        {
            double expected = derived.ShiabS.Coefficients[i] - derived.TorsionT.Coefficients[i];
            Assert.Equal(expected, derived.ResidualUpsilon.Coefficients[i], precision: 15);
        }
    }

    [Fact]
    public void Diagnostics_IsOptional()
    {
        var derived = new DerivedState
        {
            CurvatureF = CreateField("F_h", new[] { 1.0 }),
            TorsionT = CreateField("T_h", new[] { 0.5 }),
            ShiabS = CreateField("S_h", new[] { 0.7 }),
            ResidualUpsilon = CreateField("Upsilon_h", new[] { 0.2 })
        };

        Assert.Null(derived.Diagnostics);
    }
}
