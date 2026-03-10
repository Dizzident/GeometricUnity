using Gu.Branching;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.Phase3.Backgrounds;
using Gu.Phase3.GaugeReduction;
using Gu.ReferenceCpu;
using Gu.Solvers;

namespace Gu.Phase3.Spectra.Tests;

public class SpectralOperatorBundleBuilderTests
{
    private static (OperatorBundleBuilder inner, SimplicialMesh mesh, LieAlgebra algebra) SetupBuilder()
    {
        var mesh = TestHelpers.SingleTriangle();
        var algebra = TestHelpers.TracePairingSu2();
        var torsion = new TrivialTorsionCpu(mesh, algebra);
        var shiab = new IdentityShiabCpu(mesh, algebra);
        var assembler = new CpuResidualAssembler(mesh, algebra, torsion, shiab);
        var mass = new CpuMassMatrix(mesh, algebra);
        var backend = new CpuSolverBackend(mesh, algebra, torsion, shiab, mass);
        var inner = new OperatorBundleBuilder(mesh, algebra, assembler, mass, backend);
        return (inner, mesh, algebra);
    }

    [Fact]
    public void BuildWithChecks_GN_AtB2_PassesSelfChecks()
    {
        var (inner, mesh, algebra) = SetupBuilder();
        var builder = new SpectralOperatorBundleBuilder(inner);

        var omega = ConnectionField.Zero(mesh, algebra).ToFieldTensor();
        var a0 = ConnectionField.Zero(mesh, algebra).ToFieldTensor();

        var spec = new LinearizedOperatorSpec
        {
            BackgroundId = "bg-check",
            OperatorType = SpectralOperatorType.GaussNewton,
            BackgroundAdmissibility = AdmissibilityLevel.B2,
            Formulation = PhysicalModeFormulation.PenaltyFixed,
        };

        var (bundle, report) = builder.BuildWithChecks(
            spec, omega, a0, TestHelpers.TestManifest(), TestHelpers.DummyGeometry());

        Assert.NotNull(bundle);
        Assert.True(report.Passed, $"Self-check failed: symErr={report.SpectralSymmetryError:E3}, " +
            $"massSymErr={report.MassSymmetryError:E3}, massMin={report.MassMinQuadratic:E3}");
    }

    [Fact]
    public void BuildWithChecks_FullHessian_AtB1_PassesSelfChecks()
    {
        var (inner, mesh, algebra) = SetupBuilder();
        var builder = new SpectralOperatorBundleBuilder(inner);

        var omega = ConnectionField.Zero(mesh, algebra).ToFieldTensor();
        var a0 = ConnectionField.Zero(mesh, algebra).ToFieldTensor();

        var spec = new LinearizedOperatorSpec
        {
            BackgroundId = "bg-full-check",
            OperatorType = SpectralOperatorType.FullHessian,
            BackgroundAdmissibility = AdmissibilityLevel.B1,
            Formulation = PhysicalModeFormulation.PenaltyFixed,
        };

        var (bundle, report) = builder.BuildWithChecks(
            spec, omega, a0, TestHelpers.TestManifest(), TestHelpers.DummyGeometry());

        Assert.NotNull(bundle);
        Assert.Equal(SpectralOperatorType.FullHessian, bundle.OperatorType);
        Assert.True(report.Passed);
    }

    [Fact]
    public void BuildWithChecks_GN_AtB1_Throws()
    {
        var (inner, mesh, algebra) = SetupBuilder();
        var builder = new SpectralOperatorBundleBuilder(inner);

        var omega = ConnectionField.Zero(mesh, algebra).ToFieldTensor();
        var a0 = ConnectionField.Zero(mesh, algebra).ToFieldTensor();

        var spec = new LinearizedOperatorSpec
        {
            BackgroundId = "bg-bad",
            OperatorType = SpectralOperatorType.GaussNewton,
            BackgroundAdmissibility = AdmissibilityLevel.B1,
        };

        Assert.Throws<InvalidOperationException>(() =>
            builder.BuildWithChecks(spec, omega, a0,
                TestHelpers.TestManifest(), TestHelpers.DummyGeometry()));
    }

    [Fact]
    public void RunSelfChecks_ReportContainsBundleId()
    {
        var (inner, mesh, algebra) = SetupBuilder();
        var builder = new SpectralOperatorBundleBuilder(inner, probeCount: 5);

        var omega = ConnectionField.Zero(mesh, algebra).ToFieldTensor();
        var a0 = ConnectionField.Zero(mesh, algebra).ToFieldTensor();

        var spec = new LinearizedOperatorSpec
        {
            BackgroundId = "bg-id-test",
            OperatorType = SpectralOperatorType.GaussNewton,
            BackgroundAdmissibility = AdmissibilityLevel.B2,
            Formulation = PhysicalModeFormulation.PenaltyFixed,
        };

        var bundle = inner.Build(spec, omega, a0,
            TestHelpers.TestManifest(), TestHelpers.DummyGeometry());
        var report = builder.RunSelfChecks(bundle);

        Assert.Contains("bg-id-test", report.BundleId);
        Assert.Equal(5, report.ProbeCount);
    }

    [Fact]
    public void RunSelfChecks_SymmetryError_IsSmall()
    {
        var (inner, mesh, algebra) = SetupBuilder();
        var builder = new SpectralOperatorBundleBuilder(inner, probeCount: 20);

        var omega = ConnectionField.Zero(mesh, algebra).ToFieldTensor();
        var a0 = ConnectionField.Zero(mesh, algebra).ToFieldTensor();

        var spec = new LinearizedOperatorSpec
        {
            BackgroundId = "bg-sym-check",
            OperatorType = SpectralOperatorType.GaussNewton,
            BackgroundAdmissibility = AdmissibilityLevel.B2,
            Formulation = PhysicalModeFormulation.PenaltyFixed,
        };

        var bundle = inner.Build(spec, omega, a0,
            TestHelpers.TestManifest(), TestHelpers.DummyGeometry());
        var report = builder.RunSelfChecks(bundle);

        Assert.True(report.SpectralSymmetryError < 1e-8,
            $"Symmetry error too large: {report.SpectralSymmetryError:E6}");
        Assert.True(report.MassSymmetryError < 1e-8,
            $"Mass symmetry error too large: {report.MassSymmetryError:E6}");
    }

    [Fact]
    public void RunSelfChecks_MassIsPSD()
    {
        var (inner, mesh, algebra) = SetupBuilder();
        var builder = new SpectralOperatorBundleBuilder(inner, probeCount: 20);

        var omega = ConnectionField.Zero(mesh, algebra).ToFieldTensor();
        var a0 = ConnectionField.Zero(mesh, algebra).ToFieldTensor();

        var spec = new LinearizedOperatorSpec
        {
            BackgroundId = "bg-psd-check",
            OperatorType = SpectralOperatorType.GaussNewton,
            BackgroundAdmissibility = AdmissibilityLevel.B2,
            Formulation = PhysicalModeFormulation.PenaltyFixed,
        };

        var bundle = inner.Build(spec, omega, a0,
            TestHelpers.TestManifest(), TestHelpers.DummyGeometry());
        var report = builder.RunSelfChecks(bundle);

        Assert.True(report.MassMinQuadratic >= -1e-12,
            $"Mass not PSD: min quadratic = {report.MassMinQuadratic:E6}");
    }

    [Fact]
    public void Constructor_NullInner_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new SpectralOperatorBundleBuilder(null!));
    }

    [Fact]
    public void BuildWithChecks_P2_WithGaugeProjector()
    {
        var (inner, mesh, algebra) = SetupBuilder();
        var builder = new SpectralOperatorBundleBuilder(inner);
        var gaugeProjector = TestHelpers.BuildGaugeProjector(mesh, algebra);

        var omega = ConnectionField.Zero(mesh, algebra).ToFieldTensor();
        var a0 = ConnectionField.Zero(mesh, algebra).ToFieldTensor();

        var spec = new LinearizedOperatorSpec
        {
            BackgroundId = "bg-p2-check",
            OperatorType = SpectralOperatorType.FullHessian,
            BackgroundAdmissibility = AdmissibilityLevel.B1,
            Formulation = PhysicalModeFormulation.ProjectedComplement,
        };

        var (bundle, report) = builder.BuildWithChecks(
            spec, omega, a0, TestHelpers.TestManifest(), TestHelpers.DummyGeometry(), gaugeProjector);

        Assert.NotNull(bundle.PhysicalProjector);
        Assert.NotNull(report);
    }
}
