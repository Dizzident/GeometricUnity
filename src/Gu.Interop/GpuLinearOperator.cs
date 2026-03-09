using Gu.Branching;
using Gu.Core;

namespace Gu.Interop;

/// <summary>
/// Matrix-free GPU linear operator wrapping INativeBackend Jacobian/adjoint actions.
/// Implements ILinearOperator so it can be used by the solver infrastructure.
/// The Jacobian J = dUpsilon/domega is evaluated on the GPU at each Apply call.
/// </summary>
public sealed class GpuLinearOperator : ILinearOperator
{
    private readonly INativeBackend _backend;
    private readonly PackedBuffer _omegaBuf;
    private readonly BufferLayoutDescriptor _edgeLayout;
    private readonly BufferLayoutDescriptor _faceLayout;
    private readonly TensorSignature _inputSig;
    private readonly TensorSignature _outputSig;
    private readonly int _inputDim;
    private readonly int _outputDim;
    private readonly int[] _inputShape;

    public GpuLinearOperator(
        INativeBackend backend,
        PackedBuffer omegaBuf,
        int edgeN,
        int faceN,
        TensorSignature inputSignature,
        int[] inputShape)
    {
        _backend = backend;
        _omegaBuf = omegaBuf;
        _inputDim = edgeN;
        _outputDim = faceN;
        _inputSig = inputSignature;
        _inputShape = inputShape;

        _edgeLayout = BufferLayoutDescriptor.CreateSoA("jac-edge", new[] { "c" }, edgeN);
        _faceLayout = BufferLayoutDescriptor.CreateSoA("jac-face", new[] { "c" }, faceN);

        _outputSig = new TensorSignature
        {
            AmbientSpaceId = inputSignature.AmbientSpaceId,
            CarrierType = "residual-field",
            Degree = "2",
            LieAlgebraBasisId = inputSignature.LieAlgebraBasisId,
            ComponentOrderId = inputSignature.ComponentOrderId,
            MemoryLayout = inputSignature.MemoryLayout,
            NumericPrecision = inputSignature.NumericPrecision,
        };
    }

    public TensorSignature InputSignature => _inputSig;
    public TensorSignature OutputSignature => _outputSig;
    public int InputDimension => _inputDim;
    public int OutputDimension => _outputDim;

    /// <summary>
    /// Forward action: J * v via GPU Jacobian-vector product.
    /// </summary>
    public FieldTensor Apply(FieldTensor v)
    {
        var deltaBuf = _backend.AllocateBuffer(_edgeLayout);
        var jvBuf = _backend.AllocateBuffer(_faceLayout);

        try
        {
            _backend.UploadBuffer(deltaBuf, v.Coefficients);
            _backend.EvaluateJacobianAction(_omegaBuf, deltaBuf, jvBuf);

            var result = new double[_outputDim];
            _backend.DownloadBuffer(jvBuf, result);

            return new FieldTensor
            {
                Label = "J*v",
                Signature = _outputSig,
                Coefficients = result,
                Shape = new[] { _outputDim },
            };
        }
        finally
        {
            _backend.FreeBuffer(deltaBuf);
            _backend.FreeBuffer(jvBuf);
        }
    }

    /// <summary>
    /// Transpose action: J^T * v via GPU adjoint action.
    /// </summary>
    public FieldTensor ApplyTranspose(FieldTensor v)
    {
        var vBuf = _backend.AllocateBuffer(_faceLayout);
        var jtvBuf = _backend.AllocateBuffer(_edgeLayout);

        try
        {
            _backend.UploadBuffer(vBuf, v.Coefficients);
            _backend.EvaluateAdjointAction(_omegaBuf, vBuf, jtvBuf);

            var result = new double[_inputDim];
            _backend.DownloadBuffer(jtvBuf, result);

            return new FieldTensor
            {
                Label = "J^T*v",
                Signature = _inputSig,
                Coefficients = result,
                Shape = _inputShape,
            };
        }
        finally
        {
            _backend.FreeBuffer(vBuf);
            _backend.FreeBuffer(jtvBuf);
        }
    }
}
