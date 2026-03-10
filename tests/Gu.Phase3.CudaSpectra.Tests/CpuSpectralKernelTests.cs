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

public class CpuSpectralKernelTests
{
    private static LinearizedOperatorBundle BuildToyBundle()
    {
        var algebra = TestHelpers.TracePairingSu2;
        var bundle = ToyGeometryFactory.CreateToy2D();
        var mesh = bundle.AmbientMesh;
        var geometry = bundle.ToGeometryContext("centroid", "P1");
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
            BackgroundId = "bg-test",
            OperatorType = SpectralOperatorType.GaussNewton,
            Formulation = PhysicalModeFormulation.PenaltyFixed,
            BackgroundAdmissibility = AdmissibilityLevel.B2,
        };

        return builder.Build(spec, omega, a0, manifest, geometry);
    }

    [Fact]
    public void StateDimension_MatchesBundle()
    {
        var bundle = BuildToyBundle();
        var kernel = new CpuSpectralKernel(bundle);
        Assert.Equal(bundle.StateDimension, kernel.StateDimension);
    }

    [Fact]
    public void ResidualDimension_MatchesJacobian()
    {
        var bundle = BuildToyBundle();
        var kernel = new CpuSpectralKernel(bundle);
        Assert.Equal(bundle.Jacobian.OutputDimension, kernel.ResidualDimension);
    }

    [Fact]
    public void ApplySpectral_ZeroInput_GivesZeroOutput()
    {
        var bundle = BuildToyBundle();
        var kernel = new CpuSpectralKernel(bundle);
        int n = kernel.StateDimension;
        var v = new double[n];
        var result = new double[n];

        kernel.ApplySpectral(v, result);

        for (int i = 0; i < n; i++)
            Assert.Equal(0.0, result[i], 12);
    }

    [Fact]
    public void ApplyMass_ZeroInput_GivesZeroOutput()
    {
        var bundle = BuildToyBundle();
        var kernel = new CpuSpectralKernel(bundle);
        int n = kernel.StateDimension;
        var v = new double[n];
        var result = new double[n];

        kernel.ApplyMass(v, result);

        for (int i = 0; i < n; i++)
            Assert.Equal(0.0, result[i], 12);
    }

    [Fact]
    public void ApplyJacobian_ZeroInput_GivesZeroOutput()
    {
        var bundle = BuildToyBundle();
        var kernel = new CpuSpectralKernel(bundle);
        int n = kernel.StateDimension;
        int m = kernel.ResidualDimension;
        var v = new double[n];
        var result = new double[m];

        kernel.ApplyJacobian(v, result);

        for (int i = 0; i < m; i++)
            Assert.Equal(0.0, result[i], 12);
    }

    [Fact]
    public void ApplyAdjoint_ZeroInput_GivesZeroOutput()
    {
        var bundle = BuildToyBundle();
        var kernel = new CpuSpectralKernel(bundle);
        int n = kernel.StateDimension;
        int m = kernel.ResidualDimension;
        var w = new double[m];
        var result = new double[n];

        kernel.ApplyAdjoint(w, result);

        for (int i = 0; i < n; i++)
            Assert.Equal(0.0, result[i], 12);
    }

    [Fact]
    public void ApplySpectral_MatchesBundleApplySpectral()
    {
        var bundle = BuildToyBundle();
        var kernel = new CpuSpectralKernel(bundle);
        int n = kernel.StateDimension;

        var rng = new Random(42);
        var v = new double[n];
        for (int i = 0; i < n; i++)
            v[i] = rng.NextDouble() - 0.5;

        // Via kernel
        var kernelResult = new double[n];
        kernel.ApplySpectral(v, kernelResult);

        // Via bundle directly
        var inputTensor = new FieldTensor
        {
            Label = "v",
            Signature = bundle.Jacobian.InputSignature,
            Coefficients = (double[])v.Clone(),
            Shape = new[] { n },
        };
        var bundleResult = bundle.ApplySpectral(inputTensor);

        for (int i = 0; i < n; i++)
            Assert.Equal(bundleResult.Coefficients[i], kernelResult[i], 12);
    }

    [Fact]
    public void ApplyMass_MatchesBundleApplyMass()
    {
        var bundle = BuildToyBundle();
        var kernel = new CpuSpectralKernel(bundle);
        int n = kernel.StateDimension;

        var rng = new Random(43);
        var v = new double[n];
        for (int i = 0; i < n; i++)
            v[i] = rng.NextDouble() - 0.5;

        var kernelResult = new double[n];
        kernel.ApplyMass(v, kernelResult);

        var inputTensor = new FieldTensor
        {
            Label = "v",
            Signature = bundle.Jacobian.InputSignature,
            Coefficients = (double[])v.Clone(),
            Shape = new[] { n },
        };
        var bundleResult = bundle.ApplyMass(inputTensor);

        for (int i = 0; i < n; i++)
            Assert.Equal(bundleResult.Coefficients[i], kernelResult[i], 12);
    }

    [Fact]
    public void ApplySpectral_IsSelfAdjoint()
    {
        var bundle = BuildToyBundle();
        var kernel = new CpuSpectralKernel(bundle);
        int n = kernel.StateDimension;

        var rng = new Random(44);
        var u = new double[n];
        var v = new double[n];
        for (int i = 0; i < n; i++)
        {
            u[i] = rng.NextDouble() - 0.5;
            v[i] = rng.NextDouble() - 0.5;
        }

        var Hu = new double[n];
        var Hv = new double[n];
        kernel.ApplySpectral(u, Hu);
        kernel.ApplySpectral(v, Hv);

        double uHv = TestHelpers.Dot(u, Hv);
        double vHu = TestHelpers.Dot(v, Hu);
        Assert.Equal(uHv, vHu, 8);
    }

    [Fact]
    public void ApplyMass_IsPositiveSemiDefinite()
    {
        var bundle = BuildToyBundle();
        var kernel = new CpuSpectralKernel(bundle);
        int n = kernel.StateDimension;

        var rng = new Random(45);
        for (int trial = 0; trial < 10; trial++)
        {
            var v = new double[n];
            for (int i = 0; i < n; i++)
                v[i] = rng.NextDouble() - 0.5;

            var Mv = new double[n];
            kernel.ApplyMass(v, Mv);

            double vMv = TestHelpers.Dot(v, Mv);
            Assert.True(vMv >= -1e-12, $"Mass form is negative: {vMv}");
        }
    }
}
