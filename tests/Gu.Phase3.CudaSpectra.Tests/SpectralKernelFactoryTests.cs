using Gu.Branching;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.Phase3.Backgrounds;
using Gu.Phase3.CudaSpectra;
using Gu.Phase3.Spectra;
using Gu.ReferenceCpu;
using Gu.Solvers;

namespace Gu.Phase3.CudaSpectra.Tests;

public class SpectralKernelFactoryTests
{
    private static LinearizedOperatorBundle BuildToyBundle()
    {
        var algebra = TestHelpers.TracePairingSu2;
        var toyGeo = ToyGeometryFactory.CreateToy2D();
        var mesh = toyGeo.AmbientMesh;
        var geometry = toyGeo.ToGeometryContext("centroid", "P1");
        var torsion = new TrivialTorsionCpu(mesh, algebra);
        var shiab = new IdentityShiabCpu(mesh, algebra);
        var backend = new CpuSolverBackend(mesh, algebra, torsion, shiab);

        int edgeN = mesh.EdgeCount * algebra.Dimension;
        var a0 = TestHelpers.MakeField(edgeN);
        var omega = TestHelpers.MakeField(edgeN);
        var manifest = TestHelpers.TestManifest;

        var assembler = new CpuResidualAssembler(mesh, algebra, torsion, shiab);
        var mass = new CpuMassMatrix(mesh, algebra);

        var builder = new OperatorBundleBuilder(mesh, algebra, assembler, mass, backend);
        var spec = new LinearizedOperatorSpec
        {
            BackgroundId = "bg-factory-test",
            OperatorType = SpectralOperatorType.GaussNewton,
            Formulation = PhysicalModeFormulation.PenaltyFixed,
            BackgroundAdmissibility = AdmissibilityLevel.B2,
        };

        return builder.Build(spec, omega, a0, manifest, geometry);
    }

    [Fact]
    public void CreateKernel_WhenCudaUnavailable_ReturnsCpuKernel()
    {
        var config = new CudaSpectralConfig();
        var factory = new SpectralKernelFactory(config);
        var bundle = BuildToyBundle();

        var kernel = factory.CreateKernel(bundle);

        Assert.IsType<CpuSpectralKernel>(kernel);
    }

    [Fact]
    public void CreateKernel_WhenForceCpu_ReturnsCpuKernel()
    {
        var config = CudaSpectralConfig.CpuOnly();
        var factory = new SpectralKernelFactory(config);
        var bundle = BuildToyBundle();

        var kernel = factory.CreateKernel(bundle);

        Assert.IsType<CpuSpectralKernel>(kernel);
    }

    [Fact]
    public void CreateCpuKernel_ReturnsCpuSpectralKernel()
    {
        var config = new CudaSpectralConfig();
        var factory = new SpectralKernelFactory(config);
        var bundle = BuildToyBundle();

        var kernel = factory.CreateCpuKernel(bundle);

        Assert.IsType<CpuSpectralKernel>(kernel);
        Assert.Equal(bundle.StateDimension, kernel.StateDimension);
    }

    [Fact]
    public void Config_IsExposedByFactory()
    {
        var config = new CudaSpectralConfig { CudaBlockSize = 512 };
        var factory = new SpectralKernelFactory(config);

        Assert.Same(config, factory.Config);
        Assert.Equal(512, factory.Config.CudaBlockSize);
    }

    [Fact]
    public void IsCudaAvailable_ReturnsFalse_WithoutNativeLibrary()
    {
        Assert.False(SpectralKernelFactory.IsCudaAvailable());
    }

    [Fact]
    public void CreateKernel_NullBundle_Throws()
    {
        var factory = new SpectralKernelFactory(new CudaSpectralConfig());
        Assert.Throws<ArgumentNullException>(() => factory.CreateKernel(null!));
    }

    [Fact]
    public void Constructor_NullConfig_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new SpectralKernelFactory(null!));
    }
}
