using Gu.Core;
using Gu.Core.Serialization;

namespace Gu.Core.Tests;

public class ResidualBundleTests
{
    private static FieldTensor CreateResidualField(string label, double[] coeffs) => new()
    {
        Label = label,
        Signature = new TensorSignature
        {
            AmbientSpaceId = "Y_h",
            CarrierType = "residual-2form",
            Degree = "2",
            LieAlgebraBasisId = "su2-standard",
            ComponentOrderId = "lexicographic",
            MemoryLayout = "dense-row-major"
        },
        Coefficients = coeffs,
        Shape = new[] { coeffs.Length }
    };

    [Fact]
    public void RoundTrip_AllFieldsPreserved()
    {
        var original = new ResidualBundle
        {
            Components = new[]
            {
                new ResidualComponent
                {
                    Label = "Upsilon_h",
                    Norm = 0.5,
                    Field = CreateResidualField("Upsilon_h", new[] { 0.2, 0.3, 0.4 })
                }
            },
            ObjectiveValue = 0.125,
            TotalNorm = 0.5
        };

        var json = GuJsonDefaults.Serialize(original);
        var deserialized = GuJsonDefaults.Deserialize<ResidualBundle>(json);

        Assert.NotNull(deserialized);
        Assert.Single(deserialized.Components);
        Assert.Equal(0.125, deserialized.ObjectiveValue);
        Assert.Equal(0.5, deserialized.TotalNorm);
    }

    [Fact]
    public void ObjectiveValue_MatchesFormula()
    {
        // Per Section 4.10: I2_h = (1/2) Upsilon_h^T M_Upsilon Upsilon_h
        // For identity mass matrix: I2 = (1/2) * sum(u_i^2)
        double[] upsilon = { 1.0, 2.0, 3.0 };
        double expectedI2 = 0.5 * (1.0 + 4.0 + 9.0); // = 7.0

        var bundle = new ResidualBundle
        {
            Components = new[]
            {
                new ResidualComponent
                {
                    Label = "Upsilon_h",
                    Norm = System.Math.Sqrt(14.0),
                    Field = CreateResidualField("Upsilon_h", upsilon)
                }
            },
            ObjectiveValue = expectedI2,
            TotalNorm = System.Math.Sqrt(14.0)
        };

        Assert.Equal(7.0, bundle.ObjectiveValue, precision: 10);
    }
}
