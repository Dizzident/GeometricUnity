using Gu.Branching;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.Phase3.Backgrounds;
using Gu.Phase3.GaugeReduction;
using Gu.ReferenceCpu;
using Gu.Solvers;

namespace Gu.Phase3.Spectra.Tests;

public class OperatorBundleBuilderTests
{
    private static (SimplicialMesh mesh, LieAlgebra algebra, CpuResidualAssembler assembler,
        CpuMassMatrix mass, CpuSolverBackend backend) SetupInfrastructure()
    {
        var mesh = TestHelpers.SingleTriangle();
        var algebra = TestHelpers.TracePairingSu2();
        var torsion = new TrivialTorsionCpu(mesh, algebra);
        var shiab = new IdentityShiabCpu(mesh, algebra);
        var assembler = new CpuResidualAssembler(mesh, algebra, torsion, shiab);
        var mass = new CpuMassMatrix(mesh, algebra);
        var backend = new CpuSolverBackend(mesh, algebra, torsion, shiab, mass);
        return (mesh, algebra, assembler, mass, backend);
    }

    [Fact]
    public void Build_GN_AtB2_Succeeds()
    {
        var (mesh, algebra, assembler, mass, backend) = SetupInfrastructure();
        var builder = new OperatorBundleBuilder(mesh, algebra, assembler, mass, backend);

        var omega = ConnectionField.Zero(mesh, algebra).ToFieldTensor();
        var a0 = ConnectionField.Zero(mesh, algebra).ToFieldTensor();
        var manifest = TestHelpers.TestManifest();
        var geometry = TestHelpers.DummyGeometry();

        var spec = new LinearizedOperatorSpec
        {
            BackgroundId = "bg-gn",
            OperatorType = SpectralOperatorType.GaussNewton,
            BackgroundAdmissibility = AdmissibilityLevel.B2,
            Formulation = PhysicalModeFormulation.PenaltyFixed,
        };

        var bundle = builder.Build(spec, omega, a0, manifest, geometry);

        Assert.Equal(SpectralOperatorType.GaussNewton, bundle.OperatorType);
        Assert.Equal(AdmissibilityLevel.B2, bundle.BackgroundAdmissibility);
        Assert.Equal(mesh.EdgeCount * algebra.Dimension, bundle.StateDimension);
        Assert.NotNull(bundle.SpectralOperator);
        Assert.NotNull(bundle.MassOperator);
        Assert.NotNull(bundle.Jacobian);
    }

    [Fact]
    public void Build_GN_AtNonB2_Throws()
    {
        var (mesh, algebra, assembler, mass, backend) = SetupInfrastructure();
        var builder = new OperatorBundleBuilder(mesh, algebra, assembler, mass, backend);

        var omega = ConnectionField.Zero(mesh, algebra).ToFieldTensor();
        var a0 = ConnectionField.Zero(mesh, algebra).ToFieldTensor();
        var manifest = TestHelpers.TestManifest();
        var geometry = TestHelpers.DummyGeometry();

        // PHYSICS CONSTRAINT #1: GN only valid for B2
        var specB1 = new LinearizedOperatorSpec
        {
            BackgroundId = "bg-gn",
            OperatorType = SpectralOperatorType.GaussNewton,
            BackgroundAdmissibility = AdmissibilityLevel.B1,
        };

        var ex = Assert.Throws<InvalidOperationException>(() =>
            builder.Build(specB1, omega, a0, manifest, geometry));
        Assert.Contains("B2", ex.Message);
        Assert.Contains("Gauss-Newton", ex.Message);
    }

    [Fact]
    public void Build_GN_AtB0_Throws()
    {
        var (mesh, algebra, assembler, mass, backend) = SetupInfrastructure();
        var builder = new OperatorBundleBuilder(mesh, algebra, assembler, mass, backend);

        var omega = ConnectionField.Zero(mesh, algebra).ToFieldTensor();
        var a0 = ConnectionField.Zero(mesh, algebra).ToFieldTensor();

        var spec = new LinearizedOperatorSpec
        {
            BackgroundId = "bg-gn",
            OperatorType = SpectralOperatorType.GaussNewton,
            BackgroundAdmissibility = AdmissibilityLevel.B0,
        };

        Assert.Throws<InvalidOperationException>(() =>
            builder.Build(spec, omega, a0, TestHelpers.TestManifest(), TestHelpers.DummyGeometry()));
    }

    [Fact]
    public void Build_FullHessian_AtB1_Succeeds()
    {
        var (mesh, algebra, assembler, mass, backend) = SetupInfrastructure();
        var builder = new OperatorBundleBuilder(mesh, algebra, assembler, mass, backend);

        var omega = ConnectionField.Zero(mesh, algebra).ToFieldTensor();
        var a0 = ConnectionField.Zero(mesh, algebra).ToFieldTensor();

        var spec = new LinearizedOperatorSpec
        {
            BackgroundId = "bg-full",
            OperatorType = SpectralOperatorType.FullHessian,
            BackgroundAdmissibility = AdmissibilityLevel.B1,
            Formulation = PhysicalModeFormulation.PenaltyFixed,
        };

        var bundle = builder.Build(spec, omega, a0,
            TestHelpers.TestManifest(), TestHelpers.DummyGeometry());

        Assert.Equal(SpectralOperatorType.FullHessian, bundle.OperatorType);
        Assert.Equal(AdmissibilityLevel.B1, bundle.BackgroundAdmissibility);
    }

    [Fact]
    public void Build_FullHessian_AtB2_AlsoSucceeds()
    {
        var (mesh, algebra, assembler, mass, backend) = SetupInfrastructure();
        var builder = new OperatorBundleBuilder(mesh, algebra, assembler, mass, backend);

        var omega = ConnectionField.Zero(mesh, algebra).ToFieldTensor();
        var a0 = ConnectionField.Zero(mesh, algebra).ToFieldTensor();

        var spec = new LinearizedOperatorSpec
        {
            BackgroundId = "bg-full-b2",
            OperatorType = SpectralOperatorType.FullHessian,
            BackgroundAdmissibility = AdmissibilityLevel.B2,
            Formulation = PhysicalModeFormulation.PenaltyFixed,
        };

        var bundle = builder.Build(spec, omega, a0,
            TestHelpers.TestManifest(), TestHelpers.DummyGeometry());

        Assert.Equal(SpectralOperatorType.FullHessian, bundle.OperatorType);
    }

    [Fact]
    public void Build_P2_WithGaugeProjector_SetsPhysicalProjector()
    {
        var (mesh, algebra, assembler, mass, backend) = SetupInfrastructure();
        var builder = new OperatorBundleBuilder(mesh, algebra, assembler, mass, backend);

        var omega = ConnectionField.Zero(mesh, algebra).ToFieldTensor();
        var a0 = ConnectionField.Zero(mesh, algebra).ToFieldTensor();
        var gaugeProjector = TestHelpers.BuildGaugeProjector(mesh, algebra);

        var spec = new LinearizedOperatorSpec
        {
            BackgroundId = "bg-p2",
            OperatorType = SpectralOperatorType.FullHessian,
            BackgroundAdmissibility = AdmissibilityLevel.B1,
            Formulation = PhysicalModeFormulation.ProjectedComplement,
        };

        var bundle = builder.Build(spec, omega, a0,
            TestHelpers.TestManifest(), TestHelpers.DummyGeometry(), gaugeProjector);

        Assert.NotNull(bundle.PhysicalProjector);
        Assert.NotNull(bundle.PhysicalDimension);
        Assert.NotNull(bundle.GaugeRank);
        Assert.Equal(gaugeProjector.GaugeRank, bundle.GaugeRank);
        Assert.Equal(gaugeProjector.PhysicalDimension, bundle.PhysicalDimension);
    }

    [Fact]
    public void Build_P2_WithoutGaugeProjector_NoProjector()
    {
        var (mesh, algebra, assembler, mass, backend) = SetupInfrastructure();
        var builder = new OperatorBundleBuilder(mesh, algebra, assembler, mass, backend);

        var omega = ConnectionField.Zero(mesh, algebra).ToFieldTensor();
        var a0 = ConnectionField.Zero(mesh, algebra).ToFieldTensor();

        var spec = new LinearizedOperatorSpec
        {
            BackgroundId = "bg-p2-no-proj",
            OperatorType = SpectralOperatorType.FullHessian,
            BackgroundAdmissibility = AdmissibilityLevel.B1,
            Formulation = PhysicalModeFormulation.ProjectedComplement,
        };

        var bundle = builder.Build(spec, omega, a0,
            TestHelpers.TestManifest(), TestHelpers.DummyGeometry());

        Assert.Null(bundle.PhysicalProjector);
        Assert.Null(bundle.PhysicalDimension);
    }

    [Fact]
    public void SpectralOperator_IsSquare()
    {
        var (mesh, algebra, assembler, mass, backend) = SetupInfrastructure();
        var builder = new OperatorBundleBuilder(mesh, algebra, assembler, mass, backend);

        var omega = ConnectionField.Zero(mesh, algebra).ToFieldTensor();
        var a0 = ConnectionField.Zero(mesh, algebra).ToFieldTensor();

        var spec = new LinearizedOperatorSpec
        {
            BackgroundId = "bg-sq",
            OperatorType = SpectralOperatorType.GaussNewton,
            BackgroundAdmissibility = AdmissibilityLevel.B2,
            Formulation = PhysicalModeFormulation.PenaltyFixed,
        };

        var bundle = builder.Build(spec, omega, a0,
            TestHelpers.TestManifest(), TestHelpers.DummyGeometry());

        Assert.Equal(bundle.SpectralOperator.InputDimension, bundle.SpectralOperator.OutputDimension);
        Assert.Equal(bundle.MassOperator.InputDimension, bundle.MassOperator.OutputDimension);
        Assert.Equal(bundle.SpectralOperator.InputDimension, bundle.MassOperator.InputDimension);
    }

    [Fact]
    public void SpectralOperator_IsSymmetric_AtFlatBackground()
    {
        var (mesh, algebra, assembler, mass, backend) = SetupInfrastructure();
        var builder = new OperatorBundleBuilder(mesh, algebra, assembler, mass, backend);

        var omega = ConnectionField.Zero(mesh, algebra).ToFieldTensor();
        var a0 = ConnectionField.Zero(mesh, algebra).ToFieldTensor();

        var spec = new LinearizedOperatorSpec
        {
            BackgroundId = "bg-sym",
            OperatorType = SpectralOperatorType.GaussNewton,
            BackgroundAdmissibility = AdmissibilityLevel.B2,
            Formulation = PhysicalModeFormulation.PenaltyFixed,
        };

        var bundle = builder.Build(spec, omega, a0,
            TestHelpers.TestManifest(), TestHelpers.DummyGeometry());

        var rng = new Random(42);
        int n = bundle.SpectralOperator.InputDimension;
        for (int t = 0; t < 5; t++)
        {
            var u = TestHelpers.MakeRandomField(n, rng);
            var v = TestHelpers.MakeRandomField(n, rng);

            var hu = bundle.ApplySpectral(u);
            var hv = bundle.ApplySpectral(v);

            double uHv = TestHelpers.Dot(u.Coefficients, hv.Coefficients);
            double vHu = TestHelpers.Dot(v.Coefficients, hu.Coefficients);

            Assert.True(System.Math.Abs(uHv - vHu) < 1e-10,
                $"Spectral operator not symmetric: |<u,Hv> - <v,Hu>| = {System.Math.Abs(uHv - vHu):E6}");
        }
    }

    [Fact]
    public void SpectralOperator_IsSemiDefinite_AtFlatBackground()
    {
        var (mesh, algebra, assembler, mass, backend) = SetupInfrastructure();
        var builder = new OperatorBundleBuilder(mesh, algebra, assembler, mass, backend);

        var omega = ConnectionField.Zero(mesh, algebra).ToFieldTensor();
        var a0 = ConnectionField.Zero(mesh, algebra).ToFieldTensor();

        var spec = new LinearizedOperatorSpec
        {
            BackgroundId = "bg-psd",
            OperatorType = SpectralOperatorType.GaussNewton,
            BackgroundAdmissibility = AdmissibilityLevel.B2,
            Formulation = PhysicalModeFormulation.PenaltyFixed,
        };

        var bundle = builder.Build(spec, omega, a0,
            TestHelpers.TestManifest(), TestHelpers.DummyGeometry());

        var rng = new Random(123);
        int n = bundle.SpectralOperator.InputDimension;
        for (int t = 0; t < 10; t++)
        {
            var v = TestHelpers.MakeRandomField(n, rng);
            var hv = bundle.ApplySpectral(v);
            double vHv = TestHelpers.Dot(v.Coefficients, hv.Coefficients);

            Assert.True(vHv >= -1e-12,
                $"Spectral operator not PSD: <v, Hv> = {vHv:E6}");
        }
    }

    [Fact]
    public void BundleId_ContainsBackgroundIdAndOperatorType()
    {
        var (mesh, algebra, assembler, mass, backend) = SetupInfrastructure();
        var builder = new OperatorBundleBuilder(mesh, algebra, assembler, mass, backend);

        var omega = ConnectionField.Zero(mesh, algebra).ToFieldTensor();
        var a0 = ConnectionField.Zero(mesh, algebra).ToFieldTensor();

        var spec = new LinearizedOperatorSpec
        {
            BackgroundId = "my-bg",
            OperatorType = SpectralOperatorType.GaussNewton,
            BackgroundAdmissibility = AdmissibilityLevel.B2,
            Formulation = PhysicalModeFormulation.PenaltyFixed,
        };

        var bundle = builder.Build(spec, omega, a0,
            TestHelpers.TestManifest(), TestHelpers.DummyGeometry());

        Assert.Contains("my-bg", bundle.BundleId);
        Assert.Contains("GaussNewton", bundle.BundleId);
    }

    [Fact]
    public void Constructor_ThrowsOnNullArgs()
    {
        var (mesh, algebra, assembler, mass, backend) = SetupInfrastructure();

        Assert.Throws<ArgumentNullException>(() =>
            new OperatorBundleBuilder(null!, algebra, assembler, mass, backend));
        Assert.Throws<ArgumentNullException>(() =>
            new OperatorBundleBuilder(mesh, null!, assembler, mass, backend));
        Assert.Throws<ArgumentNullException>(() =>
            new OperatorBundleBuilder(mesh, algebra, null!, mass, backend));
        Assert.Throws<ArgumentNullException>(() =>
            new OperatorBundleBuilder(mesh, algebra, assembler, null!, backend));
        Assert.Throws<ArgumentNullException>(() =>
            new OperatorBundleBuilder(mesh, algebra, assembler, mass, null!));
    }
}
