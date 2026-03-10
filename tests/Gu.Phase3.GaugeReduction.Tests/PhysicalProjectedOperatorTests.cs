using Gu.Branching;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.Phase2.Stability;
using Gu.Phase3.GaugeReduction;
using Gu.ReferenceCpu;
using Gu.Solvers;

namespace Gu.Phase3.GaugeReduction.Tests;

/// <summary>
/// Tests for PhysicalProjectedOperator: H_phys = P_phys H P_phys.
/// </summary>
public class PhysicalProjectedOperatorTests
{
    private static SimplicialMesh SingleTriangle() =>
        MeshTopologyBuilder.Build(
            embeddingDimension: 2,
            simplicialDimension: 2,
            vertexCoordinates: new double[] { 0, 0, 1, 0, 0, 1 },
            vertexCount: 3,
            cellVertices: new[] { new[] { 0, 1, 2 } });

    /// <summary>
    /// Simple symmetric diagonal operator for testing.
    /// </summary>
    private sealed class TestDiagonalOperator : ILinearOperator
    {
        private readonly double[] _diag;

        public TestDiagonalOperator(double[] diag) { _diag = diag; }

        public TensorSignature InputSignature => new()
        {
            AmbientSpaceId = "Y_h",
            CarrierType = "connection-1form",
            Degree = "1",
            LieAlgebraBasisId = "standard",
            ComponentOrderId = "edge-major",
            NumericPrecision = "float64",
            MemoryLayout = "dense-row-major",
        };
        public TensorSignature OutputSignature => InputSignature;
        public int InputDimension => _diag.Length;
        public int OutputDimension => _diag.Length;

        public FieldTensor Apply(FieldTensor v)
        {
            var result = new double[_diag.Length];
            for (int i = 0; i < _diag.Length; i++)
                result[i] = _diag[i] * v.Coefficients[i];
            return new FieldTensor
            {
                Label = "H*v",
                Signature = OutputSignature,
                Coefficients = result,
                Shape = new[] { _diag.Length },
            };
        }

        public FieldTensor ApplyTranspose(FieldTensor v) => Apply(v); // symmetric
    }

    private static (GaugeProjector projector, GaugeActionLinearization linearization) BuildProjector()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var bg = GaugeActionLinearizationTests.CreateFlatBackground();
        var linearization = new GaugeActionLinearization(mesh, algebra, bg);
        var basis = GaugeBasis.Build(linearization);
        return (new GaugeProjector(basis), linearization);
    }

    [Fact]
    public void PhysicalProjectedOperator_GaugeDirectionInKernel()
    {
        var (projector, linearization) = BuildProjector();
        int connDim = projector.ConnectionDimension;

        // Create a symmetric test operator
        var diag = new double[connDim];
        for (int i = 0; i < connDim; i++)
            diag[i] = 1.0 + i * 0.5;
        var H = new TestDiagonalOperator(diag);
        var Hphys = new PhysicalProjectedOperator(H, projector);

        // Create a gauge direction
        var eta = new double[linearization.GaugeParameterDimension];
        eta[0] = 1.0;
        eta[3] = 0.7;
        var etaTensor = new FieldTensor
        {
            Label = "eta",
            Signature = linearization.Operator.InputSignature,
            Coefficients = eta,
            Shape = new[] { eta.Length },
        };
        var gaugeDir = linearization.Apply(etaTensor).Coefficients;

        var gaugeTensor = new FieldTensor
        {
            Label = "gauge",
            Signature = Hphys.InputSignature,
            Coefficients = gaugeDir,
            Shape = new[] { connDim },
        };

        var result = Hphys.Apply(gaugeTensor);

        // H_phys on a gauge direction should be ~0
        double norm = 0;
        for (int i = 0; i < connDim; i++)
            norm += result.Coefficients[i] * result.Coefficients[i];
        Assert.True(System.Math.Sqrt(norm) < 1e-10,
            $"H_phys(gauge_dir) should be ~0, got norm {System.Math.Sqrt(norm):E6}");
    }

    [Fact]
    public void PhysicalProjectedOperator_IsSymmetricForSymmetricH()
    {
        var (projector, _) = BuildProjector();
        int connDim = projector.ConnectionDimension;

        var diag = new double[connDim];
        for (int i = 0; i < connDim; i++)
            diag[i] = 1.0 + i * 0.3;
        var H = new TestDiagonalOperator(diag);
        var Hphys = new PhysicalProjectedOperator(H, projector);

        var rng = new Random(42);
        var xCoeffs = new double[connDim];
        var yCoeffs = new double[connDim];
        for (int i = 0; i < connDim; i++)
        {
            xCoeffs[i] = rng.NextDouble() * 2.0 - 1.0;
            yCoeffs[i] = rng.NextDouble() * 2.0 - 1.0;
        }

        var x = new FieldTensor
        {
            Label = "x",
            Signature = Hphys.InputSignature,
            Coefficients = xCoeffs,
            Shape = new[] { connDim },
        };
        var y = new FieldTensor
        {
            Label = "y",
            Signature = Hphys.InputSignature,
            Coefficients = yCoeffs,
            Shape = new[] { connDim },
        };

        var hx = Hphys.Apply(x);
        var hy = Hphys.Apply(y);

        double lhs = GaugeActionLinearizationTests.Dot(hx.Coefficients, yCoeffs);
        double rhs = GaugeActionLinearizationTests.Dot(xCoeffs, hy.Coefficients);

        Assert.Equal(lhs, rhs, 10);
    }

    [Fact]
    public void PhysicalProjectedOperator_ApplyTranspose_EqualsApply_ForSymmetricH()
    {
        var (projector, _) = BuildProjector();
        int connDim = projector.ConnectionDimension;

        var diag = new double[connDim];
        for (int i = 0; i < connDim; i++)
            diag[i] = 2.0 + i;
        var H = new TestDiagonalOperator(diag);
        var Hphys = new PhysicalProjectedOperator(H, projector);

        var rng = new Random(42);
        var vCoeffs = new double[connDim];
        for (int i = 0; i < connDim; i++)
            vCoeffs[i] = rng.NextDouble() * 2.0 - 1.0;

        var v = new FieldTensor
        {
            Label = "v",
            Signature = Hphys.InputSignature,
            Coefficients = vCoeffs,
            Shape = new[] { connDim },
        };

        var forward = Hphys.Apply(v);
        var transpose = Hphys.ApplyTranspose(v);

        for (int i = 0; i < connDim; i++)
            Assert.Equal(forward.Coefficients[i], transpose.Coefficients[i], 12);
    }

    [Fact]
    public void PhysicalProjectedOperator_PhysicalDirectionPreserved()
    {
        var (projector, _) = BuildProjector();
        int connDim = projector.ConnectionDimension;

        // Use identity as H so H_phys = P_phys I P_phys = P_phys
        var ones = new double[connDim];
        for (int i = 0; i < connDim; i++) ones[i] = 1.0;
        var H = new TestDiagonalOperator(ones);
        var Hphys = new PhysicalProjectedOperator(H, projector);

        var rng = new Random(42);
        var vCoeffs = new double[connDim];
        for (int i = 0; i < connDim; i++)
            vCoeffs[i] = rng.NextDouble() * 2.0 - 1.0;

        // Project v to physical first
        var physV = projector.ApplyPhysical(vCoeffs);
        var physTensor = new FieldTensor
        {
            Label = "phys_v",
            Signature = Hphys.InputSignature,
            Coefficients = physV,
            Shape = new[] { connDim },
        };

        // H_phys(P_phys(v)) should equal P_phys(v) when H=I
        var result = Hphys.Apply(physTensor);
        for (int i = 0; i < connDim; i++)
            Assert.Equal(physV[i], result.Coefficients[i], 10);
    }

    [Fact]
    public void WrapOperator_ViaWorkbench()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var bg = GaugeActionLinearizationTests.CreateFlatBackground();
        var workbench = new GaugeReductionWorkbench(mesh, algebra);
        var projector = workbench.BuildGaugeProjector(bg);

        int connDim = projector.ConnectionDimension;
        var diag = new double[connDim];
        for (int i = 0; i < connDim; i++) diag[i] = 1.0;
        var H = new TestDiagonalOperator(diag);

        var wrapped = workbench.WrapOperator(H, projector);
        Assert.NotNull(wrapped);
        Assert.Same(H, wrapped.InnerOperator);
        Assert.Same(projector, wrapped.Projector);
    }
}
