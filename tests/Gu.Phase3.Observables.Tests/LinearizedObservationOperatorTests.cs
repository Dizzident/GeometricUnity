using Gu.Branching;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.Phase3.Observables;
using Gu.Phase3.Spectra;
using Gu.ReferenceCpu;

namespace Gu.Phase3.Observables.Tests;

public class LinearizedObservationOperatorTests
{
    private static TensorSignature ConnectionSig() => new()
    {
        AmbientSpaceId = "Y_h",
        CarrierType = "connection-1form",
        Degree = "1",
        LieAlgebraBasisId = "standard",
        ComponentOrderId = "edge-major",
        NumericPrecision = "float64",
        MemoryLayout = "dense-row-major",
    };

    /// <summary>
    /// Mock Jacobian that maps connection-dim to face-dim output.
    /// </summary>
    private sealed class FaceMockJacobian : ILinearOperator
    {
        private readonly int _inputDim;
        private readonly int _outputDim;
        private readonly FiberBundleMesh _bundle;

        public FaceMockJacobian(int inputDim, FiberBundleMesh bundle)
        {
            _inputDim = inputDim;
            _outputDim = bundle.AmbientMesh.FaceCount;
            _bundle = bundle;
        }

        public TensorSignature InputSignature => ConnectionSig();
        public TensorSignature OutputSignature => new()
        {
            AmbientSpaceId = "Y_h",
            CarrierType = "curvature-2form",
            Degree = "2",
            LieAlgebraBasisId = "standard",
            ComponentOrderId = "face-major",
            NumericPrecision = "float64",
            MemoryLayout = "dense-row-major",
        };
        public int InputDimension => _inputDim;
        public int OutputDimension => _outputDim;

        public FieldTensor Apply(FieldTensor v)
        {
            // Simple mock: first _outputDim entries scaled by 2
            var result = new double[_outputDim];
            for (int i = 0; i < System.Math.Min(_inputDim, _outputDim); i++)
                result[i] = v.Coefficients[i] * 2.0;
            return new FieldTensor
            {
                Label = "J*v",
                Signature = OutputSignature,
                Coefficients = result,
                Shape = new[] { _outputDim },
            };
        }

        public FieldTensor ApplyTranspose(FieldTensor v) => Apply(v);
    }

    [Fact]
    public void Apply_ProducesObservedSignature()
    {
        var bundle = ToyGeometryFactory.CreateToy2D();
        var pullback = new PullbackOperator(bundle);
        int connDim = bundle.AmbientMesh.EdgeCount;

        var jacobian = new FaceMockJacobian(connDim, bundle);
        var obsOp = new LinearizedObservationOperator(jacobian, pullback, "test-bg");

        var modeVec = new double[connDim];
        modeVec[0] = 1.0;
        if (connDim > 3) modeVec[3] = 0.5;

        var sig = obsOp.Apply(modeVec, "mode-0");

        Assert.Equal("mode-0", sig.ModeId);
        Assert.Equal("test-bg", sig.BackgroundId);
        Assert.Equal(LinearizationMethod.Analytic, sig.LinearizationMethod);
        Assert.NotNull(sig.ObservedCoefficients);
        Assert.True(sig.ObservedCoefficients.Length > 0);
    }

    [Fact]
    public void Apply_ZeroMode_ZeroSignature()
    {
        var bundle = ToyGeometryFactory.CreateToy2D();
        var pullback = new PullbackOperator(bundle);
        int connDim = bundle.AmbientMesh.EdgeCount;

        var jacobian = new FaceMockJacobian(connDim, bundle);
        var obsOp = new LinearizedObservationOperator(jacobian, pullback, "test-bg");

        var zero = new double[connDim];
        var sig = obsOp.Apply(zero, "mode-zero");
        Assert.Equal(0.0, sig.L2Norm);
    }

    [Fact]
    public void Apply_DifferentModes_DifferentSignatures()
    {
        var bundle = ToyGeometryFactory.CreateToy2D();
        var pullback = new PullbackOperator(bundle);
        int connDim = bundle.AmbientMesh.EdgeCount;

        var jacobian = new FaceMockJacobian(connDim, bundle);
        var obsOp = new LinearizedObservationOperator(jacobian, pullback, "test-bg");

        var mode1 = new double[connDim];
        mode1[0] = 1.0;
        var mode2 = new double[connDim];
        mode2[1] = 1.0;

        var sig1 = obsOp.Apply(mode1, "mode-1");
        var sig2 = obsOp.Apply(mode2, "mode-2");

        // Different modes should give different signatures
        bool allSame = true;
        for (int i = 0; i < sig1.ObservedCoefficients.Length; i++)
        {
            if (System.Math.Abs(sig1.ObservedCoefficients[i] - sig2.ObservedCoefficients[i]) > 1e-12)
            {
                allSame = false;
                break;
            }
        }
        Assert.False(allSame, "Different mode inputs should produce different observed signatures");
    }

    [Fact]
    public void Apply_Linearity()
    {
        var bundle = ToyGeometryFactory.CreateToy2D();
        var pullback = new PullbackOperator(bundle);
        int connDim = bundle.AmbientMesh.EdgeCount;

        var jacobian = new FaceMockJacobian(connDim, bundle);
        var obsOp = new LinearizedObservationOperator(jacobian, pullback, "test-bg");

        var v1 = new double[connDim];
        var v2 = new double[connDim];
        var vSum = new double[connDim];
        v1[0] = 1.0;
        v2[0] = 0.5;
        vSum[0] = 1.5;

        var sig1 = obsOp.Apply(v1, "m1");
        var sig2 = obsOp.Apply(v2, "m2");
        var sigSum = obsOp.Apply(vSum, "msum");

        // D_Obs(v1 + v2) = D_Obs(v1) + D_Obs(v2)
        for (int i = 0; i < sigSum.ObservedCoefficients.Length; i++)
        {
            double expected = sig1.ObservedCoefficients[i] + sig2.ObservedCoefficients[i];
            Assert.Equal(expected, sigSum.ObservedCoefficients[i], 10);
        }
    }

    [Fact]
    public void NormalizationStability_L2Unit_vs_MaxBlockNorm_HighOverlap()
    {
        // Build observation operator
        var bundle = ToyGeometryFactory.CreateToy2D();
        var pullback = new PullbackOperator(bundle);
        int connDim = bundle.AmbientMesh.EdgeCount;

        var jacobian = new FaceMockJacobian(connDim, bundle);
        var obsOp = new LinearizedObservationOperator(jacobian, pullback, "test-bg");

        // Create a non-trivial mode vector
        var rawMode = new double[connDim];
        for (int i = 0; i < connDim; i++)
            rawMode[i] = (i + 1) * 0.3 - 0.5;

        // Normalize with L2Unit convention
        var modeL2 = ModeNormalizer.NormalizeL2(rawMode);

        // Normalize with MaxBlockNorm convention (dimG=3 for su(2))
        var modeMaxBlock = ModeNormalizer.NormalizeMaxBlockNorm(rawMode, dimG: 3);

        // Compute observed signatures for both normalizations
        var sigA = obsOp.Apply(modeL2, "mode-l2");
        var sigB = obsOp.Apply(modeMaxBlock, "mode-maxblock");

        // The observation operator is linear, so D_Obs(alpha * v) = alpha * D_Obs(v).
        // Since both normalizations are just different positive scalings of the same
        // direction, the observed signatures should be proportional (same direction).
        // L2Overlap measures cosine similarity, which is 1.0 for proportional vectors.
        double overlap = ObservedOverlapMetrics.L2Overlap(sigA, sigB);
        Assert.True(overlap > 0.99,
            $"L2 overlap between L2Unit and MaxBlockNorm observed signatures should be > 0.99, got {overlap}");
    }
}
