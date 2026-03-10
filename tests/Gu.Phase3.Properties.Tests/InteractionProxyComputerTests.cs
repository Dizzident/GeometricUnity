using Gu.Math;
using Gu.Phase3.Spectra;
using Gu.ReferenceCpu;

namespace Gu.Phase3.Properties.Tests;

public class InteractionProxyComputerTests
{
    [Fact]
    public void Compute_AtFlatBackground_ReturnsFiniteValue()
    {
        var mesh = TestHelpers.SingleTriangle();
        var algebra = TestHelpers.TracePairingSu2();
        var torsion = new TrivialTorsionCpu(mesh, algebra);
        var shiab = new IdentityShiabCpu(mesh, algebra);
        var mass = new CpuMassMatrix(mesh, algebra);
        var backend = new CpuSolverBackend(mesh, algebra, torsion, shiab, mass);

        var computer = new InteractionProxyComputer(backend, epsilon: 1e-2);

        int dim = mesh.EdgeCount * algebra.Dimension;
        var omega = new Gu.Core.FieldTensor
        {
            Label = "omega",
            Signature = TestHelpers.ConnectionSignature(),
            Coefficients = new double[dim],
            Shape = new[] { dim },
        };
        var a0 = new Gu.Core.FieldTensor
        {
            Label = "a0",
            Signature = TestHelpers.ConnectionSignature(),
            Coefficients = new double[dim],
            Shape = new[] { dim },
        };

        var rng = new Random(42);
        var vi = TestHelpers.MakeMode("m-i", 1.0, dim, rng);
        var vj = TestHelpers.MakeMode("m-j", 2.0, dim, rng);
        var vk = TestHelpers.MakeMode("m-k", 3.0, dim, rng);

        var result = computer.Compute(
            vi, vj, vk, omega, a0,
            TestHelpers.TestManifest(), TestHelpers.DummyGeometry());

        Assert.False(double.IsNaN(result.CubicResponse), "Cubic response is NaN");
        Assert.False(double.IsInfinity(result.CubicResponse), "Cubic response is Inf");
        Assert.Equal(3, result.ModeIds.Count);
        Assert.Equal("finite-difference-gradient", result.Method);
        Assert.Equal(1e-2, result.Epsilon);
    }

    [Fact]
    public void Compute_ResultContainsCorrectModeIds()
    {
        var mesh = TestHelpers.SingleTriangle();
        var algebra = TestHelpers.TracePairingSu2();
        var torsion = new TrivialTorsionCpu(mesh, algebra);
        var shiab = new IdentityShiabCpu(mesh, algebra);
        var mass = new CpuMassMatrix(mesh, algebra);
        var backend = new CpuSolverBackend(mesh, algebra, torsion, shiab, mass);

        var computer = new InteractionProxyComputer(backend, epsilon: 1e-2);

        int dim = mesh.EdgeCount * algebra.Dimension;
        var omega = new Gu.Core.FieldTensor
        {
            Label = "omega",
            Signature = TestHelpers.ConnectionSignature(),
            Coefficients = new double[dim],
            Shape = new[] { dim },
        };
        var a0 = omega;

        var vi = TestHelpers.MakeMode("mode-A", 1.0, new double[dim]);
        var vj = TestHelpers.MakeMode("mode-B", 1.0, new double[dim]);
        var vk = TestHelpers.MakeMode("mode-C", 1.0, new double[dim]);

        var result = computer.Compute(
            vi, vj, vk, omega, a0,
            TestHelpers.TestManifest(), TestHelpers.DummyGeometry());

        Assert.Contains("mode-A", result.ModeIds);
        Assert.Contains("mode-B", result.ModeIds);
        Assert.Contains("mode-C", result.ModeIds);
    }

    [Fact]
    public void Constructor_NullBackend_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new InteractionProxyComputer(null!));
    }
}
