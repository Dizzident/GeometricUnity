using Gu.Phase3.Backgrounds;

namespace Gu.Phase3.Spectra.Tests;

public class SpectralTypesTests
{
    [Fact]
    public void SpectralOperatorType_HasExpectedValues()
    {
        Assert.Equal(0, (int)SpectralOperatorType.GaussNewton);
        Assert.Equal(1, (int)SpectralOperatorType.FullHessian);
    }

    [Fact]
    public void PhysicalModeFormulation_HasExpectedValues()
    {
        Assert.Equal(0, (int)PhysicalModeFormulation.PenaltyFixed);
        Assert.Equal(1, (int)PhysicalModeFormulation.ProjectedComplement);
        Assert.Equal(2, (int)PhysicalModeFormulation.QuotientAware);
    }

    [Fact]
    public void LinearizedOperatorSpec_RequiredProperties()
    {
        var spec = new LinearizedOperatorSpec
        {
            BackgroundId = "bg-1",
            OperatorType = SpectralOperatorType.FullHessian,
            BackgroundAdmissibility = AdmissibilityLevel.B1,
        };

        Assert.Equal("bg-1", spec.BackgroundId);
        Assert.Equal(SpectralOperatorType.FullHessian, spec.OperatorType);
        Assert.Equal(AdmissibilityLevel.B1, spec.BackgroundAdmissibility);
        Assert.Equal(PhysicalModeFormulation.ProjectedComplement, spec.Formulation); // default
        Assert.Equal(0.1, spec.GaugeLambda); // default
    }

    [Fact]
    public void LinearizedOperatorSpec_CustomValues()
    {
        var spec = new LinearizedOperatorSpec
        {
            BackgroundId = "bg-custom",
            OperatorType = SpectralOperatorType.GaussNewton,
            BackgroundAdmissibility = AdmissibilityLevel.B2,
            Formulation = PhysicalModeFormulation.PenaltyFixed,
            GaugeLambda = 5.0,
        };

        Assert.Equal(PhysicalModeFormulation.PenaltyFixed, spec.Formulation);
        Assert.Equal(5.0, spec.GaugeLambda);
    }
}
