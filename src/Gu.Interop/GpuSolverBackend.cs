using Gu.Branching;
using Gu.Core;
using Gu.Solvers;

namespace Gu.Interop;

/// <summary>
/// GPU-accelerated implementation of ISolverBackend that dispatches computations
/// to a native backend (CUDA or CPU-reference) via the INativeBackend interface.
///
/// This backend bridges the semantic solver world (FieldTensor, DerivedState) with
/// the packed-buffer GPU world (PackedBuffer, SoA layout). It handles:
/// - Packing FieldTensor data to SoA format for GPU upload
/// - Dispatching compute kernels (curvature, torsion, Shiab, residual, objective)
/// - Unpacking results back to FieldTensor format
///
/// The Jacobian and gradient operations are not yet GPU-accelerated (M10+) and
/// will throw NotSupportedException until implemented.
/// </summary>
public sealed class GpuSolverBackend : ISolverBackend, IDisposable
{
    private readonly INativeBackend _nativeBackend;
    private readonly bool _ownsBackend;
    private ManifestSnapshot? _manifest;
    private bool _disposed;

    /// <summary>
    /// Create a GpuSolverBackend wrapping the given native backend.
    /// </summary>
    /// <param name="nativeBackend">The native compute backend (CUDA or CPU-reference).</param>
    /// <param name="ownsBackend">If true, Dispose will also dispose the native backend.</param>
    public GpuSolverBackend(INativeBackend nativeBackend, bool ownsBackend = true)
    {
        _nativeBackend = nativeBackend ?? throw new ArgumentNullException(nameof(nativeBackend));
        _ownsBackend = ownsBackend;
    }

    /// <summary>
    /// Initialize the GPU backend with manifest and mesh information.
    /// Must be called before any compute operations.
    /// </summary>
    public void Initialize(ManifestSnapshot manifest)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(manifest);
        _nativeBackend.Initialize(manifest);
        _manifest = manifest;
    }

    /// <summary>The underlying native backend.</summary>
    public INativeBackend NativeBackend => _nativeBackend;

    /// <summary>
    /// Evaluate derived state (F, T, S, Upsilon) from omega via GPU dispatch.
    /// Packs omega to SoA, runs GPU kernels, unpacks results.
    /// </summary>
    public DerivedState EvaluateDerived(
        FieldTensor omega,
        FieldTensor a0,
        BranchManifest manifest,
        GeometryContext geometry)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        EnsureInitialized();
        ArgumentNullException.ThrowIfNull(omega);
        ArgumentNullException.ThrowIfNull(a0);

        int n = omega.Coefficients.Length;
        var layout = BufferLayoutDescriptor.CreateSoA("gpu-field", new[] { "c" }, n);

        // Allocate GPU buffers for each stage
        var omegaBuf = _nativeBackend.AllocateBuffer(layout);
        var curvatureBuf = _nativeBackend.AllocateBuffer(layout);
        var torsionBuf = _nativeBackend.AllocateBuffer(layout);
        var shiabBuf = _nativeBackend.AllocateBuffer(layout);
        var residualBuf = _nativeBackend.AllocateBuffer(layout);

        try
        {
            // Upload omega
            _nativeBackend.UploadBuffer(omegaBuf, omega.Coefficients);

            // Stage 1: Curvature F = d(omega) + (1/2)[omega, omega]
            _nativeBackend.EvaluateCurvature(omegaBuf, curvatureBuf);

            // Stage 2: Torsion T
            _nativeBackend.EvaluateTorsion(omegaBuf, torsionBuf);

            // Stage 3: Shiab S
            _nativeBackend.EvaluateShiab(omegaBuf, shiabBuf);

            // Stage 4: Residual Upsilon = S - T
            _nativeBackend.EvaluateResidual(shiabBuf, torsionBuf, residualBuf);

            // Download results
            var curvatureData = new double[n];
            var torsionData = new double[n];
            var shiabData = new double[n];
            var residualData = new double[n];

            _nativeBackend.DownloadBuffer(curvatureBuf, curvatureData);
            _nativeBackend.DownloadBuffer(torsionBuf, torsionData);
            _nativeBackend.DownloadBuffer(shiabBuf, shiabData);
            _nativeBackend.DownloadBuffer(residualBuf, residualData);

            // Build derived state from GPU results
            var curvatureSig = CreateOutputSignature(omega.Signature, "curvature-2form", "2");
            var residualSig = CreateOutputSignature(omega.Signature, "residual-field", "0");

            return new DerivedState
            {
                CurvatureF = new FieldTensor
                {
                    Label = "F_h",
                    Signature = curvatureSig,
                    Coefficients = curvatureData,
                    Shape = omega.Shape,
                },
                TorsionT = new FieldTensor
                {
                    Label = "T_h",
                    Signature = residualSig,
                    Coefficients = torsionData,
                    Shape = omega.Shape,
                },
                ShiabS = new FieldTensor
                {
                    Label = "S_h",
                    Signature = residualSig,
                    Coefficients = shiabData,
                    Shape = omega.Shape,
                },
                ResidualUpsilon = new FieldTensor
                {
                    Label = "Upsilon_h",
                    Signature = residualSig,
                    Coefficients = residualData,
                    Shape = omega.Shape,
                },
            };
        }
        finally
        {
            _nativeBackend.FreeBuffer(omegaBuf);
            _nativeBackend.FreeBuffer(curvatureBuf);
            _nativeBackend.FreeBuffer(torsionBuf);
            _nativeBackend.FreeBuffer(shiabBuf);
            _nativeBackend.FreeBuffer(residualBuf);
        }
    }

    /// <summary>
    /// Evaluate objective I2_h = (1/2) ||Upsilon||^2 via GPU dispatch.
    /// </summary>
    public double EvaluateObjective(FieldTensor upsilon)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        EnsureInitialized();
        ArgumentNullException.ThrowIfNull(upsilon);

        int n = upsilon.Coefficients.Length;
        var layout = BufferLayoutDescriptor.CreateSoA("residual", new[] { "c" }, n);
        var residualBuf = _nativeBackend.AllocateBuffer(layout);

        try
        {
            _nativeBackend.UploadBuffer(residualBuf, upsilon.Coefficients);
            return _nativeBackend.EvaluateObjective(residualBuf);
        }
        finally
        {
            _nativeBackend.FreeBuffer(residualBuf);
        }
    }

    /// <summary>
    /// Build Jacobian operator.
    /// GPU Jacobian dispatch is not yet implemented (planned for M10).
    /// Throws NotSupportedException.
    /// </summary>
    public ILinearOperator BuildJacobian(
        FieldTensor omega,
        FieldTensor a0,
        FieldTensor curvatureF,
        BranchManifest manifest,
        GeometryContext geometry)
    {
        throw new NotSupportedException(
            "GPU Jacobian is not yet implemented. Use CpuSolverBackend for Jacobian operations. " +
            "GPU Jacobian dispatch is planned for Milestone 10.");
    }

    /// <summary>
    /// Compute gradient G = J^T M Upsilon.
    /// GPU gradient dispatch is not yet implemented (planned for M10).
    /// Throws NotSupportedException.
    /// </summary>
    public FieldTensor ComputeGradient(ILinearOperator jacobian, FieldTensor upsilon)
    {
        throw new NotSupportedException(
            "GPU gradient is not yet implemented. Use CpuSolverBackend for gradient operations. " +
            "GPU gradient dispatch is planned for Milestone 10.");
    }

    /// <summary>
    /// Compute mass-weighted norm of a field via GPU dispatch.
    /// Uses the simplified norm: sqrt(sum(v_i^2)).
    /// </summary>
    public double ComputeNorm(FieldTensor v)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        EnsureInitialized();
        ArgumentNullException.ThrowIfNull(v);

        // Use the objective function (which computes 0.5 * ||v||^2)
        // to derive the norm: ||v|| = sqrt(2 * objective)
        double objective = EvaluateObjective(v);
        return System.Math.Sqrt(2.0 * objective);
    }

    /// <summary>
    /// Get the last error from the native backend, if any.
    /// </summary>
    public ErrorPacket? GetLastError() => _nativeBackend.GetLastError();

    public void Dispose()
    {
        if (!_disposed)
        {
            if (_ownsBackend)
                _nativeBackend.Dispose();
            _disposed = true;
        }
    }

    private void EnsureInitialized()
    {
        if (_manifest is null)
            throw new InvalidOperationException(
                "GpuSolverBackend not initialized. Call Initialize() first.");
    }

    private static TensorSignature CreateOutputSignature(
        TensorSignature inputSig, string carrierType, string degree)
    {
        return new TensorSignature
        {
            AmbientSpaceId = inputSig.AmbientSpaceId,
            CarrierType = carrierType,
            Degree = degree,
            LieAlgebraBasisId = inputSig.LieAlgebraBasisId,
            ComponentOrderId = inputSig.ComponentOrderId,
            MemoryLayout = inputSig.MemoryLayout,
            NumericPrecision = inputSig.NumericPrecision,
        };
    }
}
