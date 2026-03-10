using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.Phase2.Stability;
using Gu.Phase3.GaugeReduction;
using Gu.ReferenceCpu;
using Gu.Solvers;

namespace Gu.Phase3.GaugeReduction.Tests;

public class GaugeProjectorTests
{
    private static (GaugeProjector projector, GaugeActionLinearization linearization) BuildProjector()
    {
        var mesh = MeshTopologyBuilder.Build(
            embeddingDimension: 2,
            simplicialDimension: 2,
            vertexCoordinates: new double[] { 0, 0, 1, 0, 0, 1 },
            vertexCount: 3,
            cellVertices: new[] { new[] { 0, 1, 2 } });
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var bg = GaugeActionLinearizationTests.CreateFlatBackground();

        var linearization = new GaugeActionLinearization(mesh, algebra, bg);
        var basis = GaugeBasis.Build(linearization);
        return (new GaugeProjector(basis), linearization);
    }

    [Fact]
    public void GaugeProjection_IsIdempotent()
    {
        var (projector, _) = BuildProjector();
        var rng = new Random(42);

        var v = new double[projector.ConnectionDimension];
        for (int i = 0; i < v.Length; i++)
            v[i] = rng.NextDouble() * 2.0 - 1.0;

        var pv = projector.ApplyGauge(v);
        var ppv = projector.ApplyGauge(pv);

        // P^2 = P (idempotent)
        for (int i = 0; i < v.Length; i++)
            Assert.Equal(pv[i], ppv[i], 10);
    }

    [Fact]
    public void PhysicalProjection_IsIdempotent()
    {
        var (projector, _) = BuildProjector();
        var rng = new Random(42);

        var v = new double[projector.ConnectionDimension];
        for (int i = 0; i < v.Length; i++)
            v[i] = rng.NextDouble() * 2.0 - 1.0;

        var pv = projector.ApplyPhysical(v);
        var ppv = projector.ApplyPhysical(pv);

        // (I-P)^2 = I-P
        for (int i = 0; i < v.Length; i++)
            Assert.Equal(pv[i], ppv[i], 10);
    }

    [Fact]
    public void GaugeAndPhysical_SumToIdentity()
    {
        var (projector, _) = BuildProjector();
        var rng = new Random(42);

        var v = new double[projector.ConnectionDimension];
        for (int i = 0; i < v.Length; i++)
            v[i] = rng.NextDouble() * 2.0 - 1.0;

        var gaugeComp = projector.ApplyGauge(v);
        var physComp = projector.ApplyPhysical(v);

        // P_gauge(v) + P_phys(v) = v
        for (int i = 0; i < v.Length; i++)
            Assert.Equal(v[i], gaugeComp[i] + physComp[i], 10);
    }

    [Fact]
    public void GaugeProjection_OrthogonalToPhysical()
    {
        var (projector, _) = BuildProjector();
        var rng = new Random(42);

        var v = new double[projector.ConnectionDimension];
        for (int i = 0; i < v.Length; i++)
            v[i] = rng.NextDouble() * 2.0 - 1.0;

        var gaugeComp = projector.ApplyGauge(v);
        var physComp = projector.ApplyPhysical(v);

        double dot = GaugeActionLinearizationTests.Dot(gaugeComp, physComp);
        Assert.True(System.Math.Abs(dot) < 1e-10,
            $"Gauge and physical components not orthogonal: dot = {dot:E6}");
    }

    [Fact]
    public void GaugeLeakScore_ForGaugeDirection_IsOne()
    {
        var (projector, linearization) = BuildProjector();

        // Create a gauge direction by applying Gamma_* to some eta
        int dimG = 3; // su(2)
        var etaCoeffs = new double[linearization.GaugeParameterDimension];
        etaCoeffs[0] = 1.0;
        etaCoeffs[dimG + 1] = 0.5;

        var eta = new FieldTensor
        {
            Label = "eta",
            Signature = linearization.Operator.InputSignature,
            Coefficients = etaCoeffs,
            Shape = new[] { etaCoeffs.Length },
        };
        var gaugeDir = linearization.Apply(eta);

        double leak = projector.GaugeLeakScore(gaugeDir.Coefficients);
        Assert.True(leak > 0.99,
            $"Gauge direction should have leak ~1.0, got {leak:F6}");
    }

    [Fact]
    public void GaugeLeakScore_ForPhysicalDirection_IsSmall()
    {
        var (projector, _) = BuildProjector();
        var rng = new Random(42);

        // Create a random vector and project to physical space
        var v = new double[projector.ConnectionDimension];
        for (int i = 0; i < v.Length; i++)
            v[i] = rng.NextDouble() * 2.0 - 1.0;
        var physV = projector.ApplyPhysical(v);

        double leak = projector.GaugeLeakScore(physV);
        Assert.True(leak < 1e-10,
            $"Physical direction should have leak ~0, got {leak:E6}");
    }

    [Fact]
    public void GaugeLeakScore_ZeroVector_ReturnsZero()
    {
        var (projector, _) = BuildProjector();
        var zero = new double[projector.ConnectionDimension];
        Assert.Equal(0.0, projector.GaugeLeakScore(zero));
    }

    [Fact]
    public void ILinearOperator_Apply_EqualsApplyPhysical()
    {
        var (projector, _) = BuildProjector();
        var rng = new Random(42);

        var vCoeffs = new double[projector.ConnectionDimension];
        for (int i = 0; i < vCoeffs.Length; i++)
            vCoeffs[i] = rng.NextDouble() * 2.0 - 1.0;

        var v = new FieldTensor
        {
            Label = "v",
            Signature = projector.InputSignature,
            Coefficients = vCoeffs,
            Shape = new[] { vCoeffs.Length },
        };

        // ILinearOperator.Apply should be the physical projector
        var result = projector.Apply(v);
        var physResult = projector.ApplyPhysical(vCoeffs);

        for (int i = 0; i < vCoeffs.Length; i++)
            Assert.Equal(physResult[i], result.Coefficients[i], 12);
    }

    [Fact]
    public void ILinearOperator_IsSelfAdjoint()
    {
        var (projector, _) = BuildProjector();
        var rng = new Random(42);

        var xCoeffs = new double[projector.ConnectionDimension];
        var yCoeffs = new double[projector.ConnectionDimension];
        for (int i = 0; i < xCoeffs.Length; i++)
        {
            xCoeffs[i] = rng.NextDouble() * 2.0 - 1.0;
            yCoeffs[i] = rng.NextDouble() * 2.0 - 1.0;
        }

        var x = new FieldTensor
        {
            Label = "x",
            Signature = projector.InputSignature,
            Coefficients = xCoeffs,
            Shape = new[] { xCoeffs.Length },
        };
        var y = new FieldTensor
        {
            Label = "y",
            Signature = projector.InputSignature,
            Coefficients = yCoeffs,
            Shape = new[] { yCoeffs.Length },
        };

        var px = projector.Apply(x);
        var py = projector.Apply(y);

        double lhs = GaugeActionLinearizationTests.Dot(px.Coefficients, yCoeffs);
        double rhs = GaugeActionLinearizationTests.Dot(xCoeffs, py.Coefficients);

        Assert.Equal(lhs, rhs, 10);
    }
}
