using Gu.Branching;
using Gu.Core;
using Gu.Geometry;
using Gu.Phase3.Observables;
using Gu.Phase3.Spectra;

namespace Gu.Phase3.Observables.Tests;

public class ObservedModeMapperTests
{
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
    public void Map_SingleMode_ProducesSignature()
    {
        var bundle = ToyGeometryFactory.CreateToy2D();
        var pullback = new PullbackOperator(bundle);
        int connDim = bundle.AmbientMesh.EdgeCount;

        var jacobian = new FaceMockJacobian(connDim, bundle);
        var linObs = new LinearizedObservationOperator(jacobian, pullback, "bg-0");
        var mapper = new ObservedModeMapper(linObs);

        var modeVec = new double[connDim];
        modeVec[0] = 1.0;
        var mode = MakeMode("m0", modeVec);
        var sig = mapper.Map(mode);

        Assert.Equal("m0", sig.ModeId);
        Assert.Equal("bg-0", sig.BackgroundId);
        Assert.Equal(LinearizationMethod.Analytic, sig.LinearizationMethod);
    }

    [Fact]
    public void MapAll_MultipleModes_ProducesCorrectCount()
    {
        var bundle = ToyGeometryFactory.CreateToy2D();
        var pullback = new PullbackOperator(bundle);
        int connDim = bundle.AmbientMesh.EdgeCount;

        var jacobian = new FaceMockJacobian(connDim, bundle);
        var linObs = new LinearizedObservationOperator(jacobian, pullback, "bg-0");
        var mapper = new ObservedModeMapper(linObs);

        var v0 = new double[connDim]; v0[0] = 1.0;
        var v1 = new double[connDim]; v1[1] = 1.0;
        var modes = new[]
        {
            MakeMode("m0", v0, 0),
            MakeMode("m1", v1, 1),
        };

        var sigs = mapper.MapAll(modes);
        Assert.Equal(2, sigs.Count);
        Assert.Equal("m0", sigs[0].ModeId);
        Assert.Equal("m1", sigs[1].ModeId);
    }
}
