using Gu.Branching;
using Gu.Core;
using Gu.Geometry;
using Gu.Phase3.Observables;
using Gu.Phase3.Spectra;

namespace Gu.Phase3.Observables.Tests;

public class ObservationPipelineTests
{
    // Minimal mock Jacobian: outputs a face-based field by copying input coefficients.
    private sealed class FaceMockJacobian : ILinearOperator
    {
        private readonly int _inputDim;
        private readonly int _outputDim;

        public FaceMockJacobian(int inputDim, FiberBundleMesh bundle)
        {
            _inputDim = inputDim;
            _outputDim = bundle.AmbientMesh.FaceCount;
        }

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

    private static (ObservationPipeline pipeline, int connDim, FiberBundleMesh bundle) BuildPipeline(string bgId = "bg-0")
    {
        var bundle = ToyGeometryFactory.CreateToy2D();
        var pullback = new PullbackOperator(bundle);
        int connDim = bundle.AmbientMesh.EdgeCount;
        var jacobian = new FaceMockJacobian(connDim, bundle);
        var linObs = new LinearizedObservationOperator(jacobian, pullback, bgId);
        var pipeline = new ObservationPipeline(linObs);
        return (pipeline, connDim, bundle);
    }

    private static ModeRecord MakeMode(string modeId, double[] vector, int index = 0) => new()
    {
        ModeId = modeId,
        BackgroundId = "test-bg",
        OperatorType = SpectralOperatorType.GaussNewton,
        Eigenvalue = 1.0,
        ResidualNorm = 0.001,
        NormalizationConvention = "unit-L2-norm",
        GaugeLeakScore = 0.0,
        ModeVector = vector,
        ModeIndex = index,
    };

    [Fact]
    public void Run_MultipleModesProducesSignatures_CorrectCount()
    {
        var (pipeline, connDim, _) = BuildPipeline();

        var v0 = new double[connDim]; v0[0] = 1.0;
        var v1 = new double[connDim]; v1[1] = 1.0;
        var v2 = new double[connDim]; v2[2] = 1.0;
        var modes = new[] { MakeMode("m0", v0, 0), MakeMode("m1", v1, 1), MakeMode("m2", v2, 2) };

        var result = pipeline.Run(modes);

        Assert.Equal(3, result.ModeCount);
        Assert.Equal(3, result.Signatures.Count);
    }

    [Fact]
    public void Run_ReturnsNonNullOverlapResult()
    {
        var (pipeline, connDim, _) = BuildPipeline();

        var v0 = new double[connDim]; v0[0] = 1.0;
        var v1 = new double[connDim]; v1[1] = 1.0;
        var modes = new[] { MakeMode("m0", v0, 0), MakeMode("m1", v1, 1) };

        var result = pipeline.Run(modes);

        Assert.NotNull(result.Overlap);
        Assert.Equal(2, result.Overlap.ModeIds.Length);

        // Overlap matrix must be symmetric: O[i,j] == O[j,i]
        Assert.Equal(result.Overlap.OverlapMatrix[0, 1], result.Overlap.OverlapMatrix[1, 0]);
    }

    [Fact]
    public void Run_SingleMode_OverlapPairIsNull()
    {
        var (pipeline, connDim, _) = BuildPipeline();

        var v0 = new double[connDim]; v0[0] = 1.0;
        var modes = new[] { MakeMode("m0", v0, 0) };

        var result = pipeline.Run(modes);

        Assert.Equal(1, result.ModeCount);
        Assert.Null(result.Overlap.MaxOverlapPair);
    }

    [Fact]
    public void Run_DifferentModes_OverlapIsLessThanOne()
    {
        var (pipeline, connDim, _) = BuildPipeline();

        // Two orthogonal mode vectors → observed overlap < 1
        var v0 = new double[connDim]; v0[0] = 1.0;
        var v1 = new double[connDim]; v1[1] = 1.0;
        var modes = new[] { MakeMode("m0", v0, 0), MakeMode("m1", v1, 1) };

        var result = pipeline.Run(modes);

        Assert.True(result.Overlap.MaxOffDiagonalOverlap < 1.0,
            $"Expected off-diagonal overlap < 1, got {result.Overlap.MaxOffDiagonalOverlap}");
    }

    [Fact]
    public void Run_ResultModeCount_MatchesInput()
    {
        var (pipeline, connDim, _) = BuildPipeline();

        var v0 = new double[connDim]; v0[0] = 1.0;
        var v1 = new double[connDim]; v1[1] = 1.0;
        var modes = new[] { MakeMode("m0", v0, 0), MakeMode("m1", v1, 1) };

        var result = pipeline.Run(modes);

        Assert.Equal(modes.Length, result.ModeCount);
    }

    [Fact]
    public void NoYSpaceBypass_DirectNormVsObservationPipeline_Differ()
    {
        // §14.3: no direct Y-space bypass in comparison paths.
        // Compute L2 inner product directly in Y-space and compare with
        // the observation-pipeline overlap. They must differ (pullback != identity).
        var (pipeline, connDim, bundle) = BuildPipeline();

        var v0 = new double[connDim]; v0[0] = 1.0;
        var v1 = new double[connDim]; v1[0] = 0.6; v1[1] = 0.8;  // non-orthogonal in Y-space

        // Direct Y-space overlap
        double dot = 0, normA = 0, normB = 0;
        for (int i = 0; i < connDim; i++)
        {
            dot += v0[i] * v1[i];
            normA += v0[i] * v0[i];
            normB += v1[i] * v1[i];
        }
        double directYOverlap = System.Math.Abs(dot) / (System.Math.Sqrt(normA) * System.Math.Sqrt(normB));

        // Observation pipeline overlap (through Jacobian + pullback)
        var modes = new[] { MakeMode("m0", v0, 0), MakeMode("m1", v1, 1) };
        var result = pipeline.Run(modes);
        double pipelineOverlap = result.Overlap.MaxOffDiagonalOverlap;

        // The pullback (projection from Y_h to X_h) changes the metric,
        // so pipeline overlap != direct Y-space overlap.
        Assert.NotEqual(directYOverlap, pipelineOverlap, 6);
    }
}
