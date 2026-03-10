using Gu.Branching;
using Gu.Core;
using Gu.Phase3.Backgrounds;
using Gu.Phase3.GaugeReduction;

namespace Gu.Phase3.Spectra.Tests;

public class LinearizedOperatorBundleTests
{
    private static LinearizedOperatorBundle MakeBundle(
        PhysicalModeFormulation formulation = PhysicalModeFormulation.PenaltyFixed,
        GaugeProjector? gaugeProjector = null)
    {
        int n = 9;
        var diag = new double[n];
        for (int i = 0; i < n; i++) diag[i] = (i + 1.0) * 0.5;
        var spectralOp = new TestHelpers.DiagonalOperator(diag);

        var massDiag = new double[n];
        Array.Fill(massDiag, 1.0);
        var massOp = new TestHelpers.DiagonalOperator(massDiag);

        var jacDiag = new double[n];
        Array.Fill(jacDiag, 0.5);
        var jacobian = new TestHelpers.DiagonalOperator(jacDiag);

        ILinearOperator? physProjector = null;
        int? physDim = null;
        int? gaugeRank = null;

        if (formulation == PhysicalModeFormulation.ProjectedComplement && gaugeProjector is not null)
        {
            physProjector = new PhysicalProjector(gaugeProjector);
            gaugeRank = gaugeProjector.GaugeRank;
            physDim = gaugeProjector.PhysicalDimension;
        }

        return new LinearizedOperatorBundle
        {
            BundleId = "test-bundle",
            BackgroundId = "bg-test",
            BranchManifestId = "branch-test",
            OperatorType = SpectralOperatorType.FullHessian,
            Formulation = formulation,
            BackgroundAdmissibility = AdmissibilityLevel.B1,
            Jacobian = jacobian,
            SpectralOperator = spectralOp,
            MassOperator = massOp,
            PhysicalProjector = physProjector,
            GaugeLambda = 1.0,
            StateDimension = n,
            PhysicalDimension = physDim,
            GaugeRank = gaugeRank,
        };
    }

    [Fact]
    public void ApplySpectral_P1_DelegatesToSpectralOperator()
    {
        var bundle = MakeBundle(PhysicalModeFormulation.PenaltyFixed);

        var v = TestHelpers.MakeField(9, 2.0);
        var result = bundle.ApplySpectral(v);
        var direct = bundle.SpectralOperator.Apply(v);

        for (int i = 0; i < 9; i++)
            Assert.Equal(direct.Coefficients[i], result.Coefficients[i]);
    }

    [Fact]
    public void ApplyMass_P1_DelegatesToMassOperator()
    {
        var bundle = MakeBundle(PhysicalModeFormulation.PenaltyFixed);

        var v = TestHelpers.MakeField(9, 1.0);
        var result = bundle.ApplyMass(v);
        var direct = bundle.MassOperator.Apply(v);

        for (int i = 0; i < 9; i++)
            Assert.Equal(direct.Coefficients[i], result.Coefficients[i]);
    }

    [Fact]
    public void ApplySpectral_P2_WrapsWithProjector()
    {
        var mesh = TestHelpers.SingleTriangle();
        var algebra = TestHelpers.TracePairingSu2();
        var projector = TestHelpers.BuildGaugeProjector(mesh, algebra);
        var bundle = MakeBundle(PhysicalModeFormulation.ProjectedComplement, projector);

        var rng = new Random(42);
        var v = TestHelpers.MakeRandomField(9, rng);
        var result = bundle.ApplySpectral(v);

        // Expected: P^T H P v
        var pv = bundle.PhysicalProjector!.Apply(v);
        var hpv = bundle.SpectralOperator.Apply(pv);
        var expected = bundle.PhysicalProjector.ApplyTranspose(hpv);

        for (int i = 0; i < 9; i++)
            Assert.Equal(expected.Coefficients[i], result.Coefficients[i], 12);
    }

    [Fact]
    public void ApplyMass_P2_WrapsWithProjector()
    {
        var mesh = TestHelpers.SingleTriangle();
        var algebra = TestHelpers.TracePairingSu2();
        var projector = TestHelpers.BuildGaugeProjector(mesh, algebra);
        var bundle = MakeBundle(PhysicalModeFormulation.ProjectedComplement, projector);

        var rng = new Random(42);
        var v = TestHelpers.MakeRandomField(9, rng);
        var result = bundle.ApplyMass(v);

        // Expected: P^T M P v
        var pv = bundle.PhysicalProjector!.Apply(v);
        var mpv = bundle.MassOperator.Apply(pv);
        var expected = bundle.PhysicalProjector.ApplyTranspose(mpv);

        for (int i = 0; i < 9; i++)
            Assert.Equal(expected.Coefficients[i], result.Coefficients[i], 12);
    }

    [Fact]
    public void RequiredProperties_AreSet()
    {
        var bundle = MakeBundle();

        Assert.Equal("test-bundle", bundle.BundleId);
        Assert.Equal("bg-test", bundle.BackgroundId);
        Assert.Equal("branch-test", bundle.BranchManifestId);
        Assert.Equal(SpectralOperatorType.FullHessian, bundle.OperatorType);
        Assert.Equal(PhysicalModeFormulation.PenaltyFixed, bundle.Formulation);
        Assert.Equal(AdmissibilityLevel.B1, bundle.BackgroundAdmissibility);
        Assert.Equal(1.0, bundle.GaugeLambda);
        Assert.Equal(9, bundle.StateDimension);
        Assert.Null(bundle.PhysicalProjector);
        Assert.Null(bundle.PhysicalDimension);
        Assert.Null(bundle.GaugeRank);
    }

    [Fact]
    public void P2Bundle_HasPhysicalDimensionAndGaugeRank()
    {
        var mesh = TestHelpers.SingleTriangle();
        var algebra = TestHelpers.TracePairingSu2();
        var projector = TestHelpers.BuildGaugeProjector(mesh, algebra);
        var bundle = MakeBundle(PhysicalModeFormulation.ProjectedComplement, projector);

        Assert.NotNull(bundle.PhysicalProjector);
        Assert.NotNull(bundle.PhysicalDimension);
        Assert.NotNull(bundle.GaugeRank);
        Assert.Equal(projector.PhysicalDimension, bundle.PhysicalDimension);
        Assert.Equal(projector.GaugeRank, bundle.GaugeRank);
    }
}
