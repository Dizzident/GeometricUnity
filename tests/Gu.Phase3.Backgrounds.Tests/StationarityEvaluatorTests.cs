using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.Phase2.Stability;
using Gu.Phase3.Backgrounds;
using Gu.ReferenceCpu;
using Gu.Solvers;

namespace Gu.Phase3.Backgrounds.Tests;

public class StationarityEvaluatorTests
{
    private static (SimplicialMesh Mesh, LieAlgebra Algebra, CpuResidualAssembler Assembler, CpuMassMatrix Mass, GeometryContext Geometry, BranchManifest Manifest) SetupToy()
    {
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var bundle = ToyGeometryFactory.CreateToy2D();
        var mesh = bundle.AmbientMesh;
        var geometry = bundle.ToGeometryContext("centroid", "P1");
        var torsion = new TrivialTorsionCpu(mesh, algebra);
        var shiab = new IdentityShiabCpu(mesh, algebra);
        var assembler = new CpuResidualAssembler(mesh, algebra, torsion, shiab);
        var mass = new CpuMassMatrix(mesh, algebra);
        var manifest = TestHelpers.MakeManifest();
        return (mesh, algebra, assembler, mass, geometry, manifest);
    }

    private static FieldTensor ZeroOmega(SimplicialMesh mesh, LieAlgebra algebra)
    {
        int edgeN = mesh.EdgeCount * algebra.Dimension;
        return new FieldTensor
        {
            Label = "omega_zero",
            Signature = new TensorSignature
            {
                AmbientSpaceId = "Y_h",
                CarrierType = "connection-1form",
                Degree = "1",
                LieAlgebraBasisId = "canonical",
                ComponentOrderId = "edge-major",
                MemoryLayout = "dense-row-major",
            },
            Coefficients = new double[edgeN],
            Shape = new[] { mesh.EdgeCount, algebra.Dimension },
        };
    }

    [Fact]
    public void ZeroOmega_StationarityNorm_IsZero()
    {
        var (mesh, algebra, assembler, mass, geometry, manifest) = SetupToy();
        var evaluator = new StationarityEvaluator(mesh, algebra, assembler, mass);

        var omega = ZeroOmega(mesh, algebra);
        var a0 = ZeroOmega(mesh, algebra);

        double norm = evaluator.Evaluate(omega, a0, manifest, geometry);

        // At omega=0 with identity Shiab and trivial torsion,
        // F=0, S=F=0, T=0, Upsilon=0, so G = J^T M * 0 = 0.
        Assert.Equal(0.0, norm, 10);
    }

    [Fact]
    public void RandomOmega_StationarityNorm_IsPositive()
    {
        var (mesh, algebra, assembler, mass, geometry, manifest) = SetupToy();
        var evaluator = new StationarityEvaluator(mesh, algebra, assembler, mass);

        var rng = new Random(42);
        int edgeN = mesh.EdgeCount * algebra.Dimension;
        var coeffs = new double[edgeN];
        for (int i = 0; i < edgeN; i++)
            coeffs[i] = 0.1 * (rng.NextDouble() * 2.0 - 1.0);

        var omega = new FieldTensor
        {
            Label = "omega_rand",
            Signature = new TensorSignature
            {
                AmbientSpaceId = "Y_h",
                CarrierType = "connection-1form",
                Degree = "1",
                LieAlgebraBasisId = "canonical",
                ComponentOrderId = "edge-major",
                MemoryLayout = "dense-row-major",
            },
            Coefficients = coeffs,
            Shape = new[] { mesh.EdgeCount, algebra.Dimension },
        };
        var a0 = ZeroOmega(mesh, algebra);

        double norm = evaluator.Evaluate(omega, a0, manifest, geometry);

        // Non-zero omega should generally give a non-zero stationarity norm
        Assert.True(norm > 0, $"Expected positive stationarity norm, got {norm:E6}");
    }

    [Fact]
    public void Evaluate_WithBackgroundStateRecord_MatchesDirectEvaluation()
    {
        var (mesh, algebra, assembler, mass, geometry, manifest) = SetupToy();
        var evaluator = new StationarityEvaluator(mesh, algebra, assembler, mass);

        var rng = new Random(42);
        int edgeN = mesh.EdgeCount * algebra.Dimension;
        var coeffs = new double[edgeN];
        for (int i = 0; i < edgeN; i++)
            coeffs[i] = 0.05 * (rng.NextDouble() * 2.0 - 1.0);

        var omega = new FieldTensor
        {
            Label = "omega",
            Signature = new TensorSignature
            {
                AmbientSpaceId = "Y_h",
                CarrierType = "connection-1form",
                Degree = "1",
                LieAlgebraBasisId = "canonical",
                ComponentOrderId = "edge-major",
                MemoryLayout = "dense-row-major",
            },
            Coefficients = coeffs,
            Shape = new[] { mesh.EdgeCount, algebra.Dimension },
        };
        var a0 = ZeroOmega(mesh, algebra);

        // Evaluate via raw tensors
        double norm1 = evaluator.Evaluate(omega, a0, manifest, geometry);

        // Evaluate via BackgroundStateRecord
        var workbench = new LinearizationWorkbench(mesh, algebra, assembler, mass);
        var bg = workbench.CreateBackgroundState("test-bg", omega, a0, manifest, geometry, true);
        double norm2 = evaluator.Evaluate(bg, manifest, geometry);

        Assert.Equal(norm1, norm2, 10);
    }

    [Fact]
    public void ComputeGradient_ReturnsCorrectDimension()
    {
        var (mesh, algebra, assembler, mass, geometry, manifest) = SetupToy();
        var evaluator = new StationarityEvaluator(mesh, algebra, assembler, mass);

        var omega = ZeroOmega(mesh, algebra);
        var a0 = ZeroOmega(mesh, algebra);

        var workbench = new LinearizationWorkbench(mesh, algebra, assembler, mass);
        var bg = workbench.CreateBackgroundState("test-bg", omega, a0, manifest, geometry, true);

        var gradient = evaluator.ComputeGradient(bg, manifest, geometry);

        int expectedDim = mesh.EdgeCount * algebra.Dimension;
        Assert.Equal(expectedDim, gradient.Coefficients.Length);
    }

    [Fact]
    public void StationarityNorm_IsNonnegative()
    {
        var (mesh, algebra, assembler, mass, geometry, manifest) = SetupToy();
        var evaluator = new StationarityEvaluator(mesh, algebra, assembler, mass);

        var omega = ZeroOmega(mesh, algebra);
        var a0 = ZeroOmega(mesh, algebra);

        double norm = evaluator.Evaluate(omega, a0, manifest, geometry);
        Assert.True(norm >= 0.0);
    }
}
