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

        // omega is edge-valued: edge_count * dimG
        int nOmega = omega.Coefficients.Length;
        var omegaLayout = BufferLayoutDescriptor.CreateSoA("omega", new[] { "c" }, nOmega);

        // curvature/torsion/Shiab/residual are face-valued: face_count * dimG
        int dimG = _manifest!.LieAlgebraDimension;
        int faceCount = geometry.AmbientSpace.FaceCount
            ?? throw new InvalidOperationException(
                "GeometryContext.AmbientSpace.FaceCount must be set for GPU buffer allocation.");
        int nFace = faceCount * dimG;
        var faceLayout = BufferLayoutDescriptor.CreateSoA("face-field", new[] { "c" }, nFace);

        // Allocate GPU buffers for each stage
        var omegaBuf = _nativeBackend.AllocateBuffer(omegaLayout);
        var curvatureBuf = _nativeBackend.AllocateBuffer(faceLayout);
        var torsionBuf = _nativeBackend.AllocateBuffer(faceLayout);
        var shiabBuf = _nativeBackend.AllocateBuffer(faceLayout);
        var residualBuf = _nativeBackend.AllocateBuffer(faceLayout);

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

            // Download results (face-valued)
            var curvatureData = new double[nFace];
            var torsionData = new double[nFace];
            var shiabData = new double[nFace];
            var residualData = new double[nFace];

            _nativeBackend.DownloadBuffer(curvatureBuf, curvatureData);
            _nativeBackend.DownloadBuffer(torsionBuf, torsionData);
            _nativeBackend.DownloadBuffer(shiabBuf, shiabData);
            _nativeBackend.DownloadBuffer(residualBuf, residualData);

            // Build derived state from GPU results
            var curvatureSig = CreateOutputSignature(omega.Signature, "curvature-2form", "2");
            var residualSig = CreateOutputSignature(omega.Signature, "residual-field", "0");

            var faceShape = new[] { nFace };

            return new DerivedState
            {
                CurvatureF = new FieldTensor
                {
                    Label = "F_h",
                    Signature = curvatureSig,
                    Coefficients = curvatureData,
                    Shape = faceShape,
                },
                TorsionT = new FieldTensor
                {
                    Label = "T_h",
                    Signature = residualSig,
                    Coefficients = torsionData,
                    Shape = faceShape,
                },
                ShiabS = new FieldTensor
                {
                    Label = "S_h",
                    Signature = residualSig,
                    Coefficients = shiabData,
                    Shape = faceShape,
                },
                ResidualUpsilon = new FieldTensor
                {
                    Label = "Upsilon_h",
                    Signature = residualSig,
                    Coefficients = residualData,
                    Shape = faceShape,
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
    /// Build a matrix-free GPU Jacobian operator.
    /// Returns a GpuLinearOperator that dispatches J*v and J^T*v to the native backend.
    /// The omega connection is uploaded once; each Apply call uploads the perturbation vector.
    /// </summary>
    public ILinearOperator BuildJacobian(
        FieldTensor omega,
        FieldTensor a0,
        FieldTensor curvatureF,
        BranchManifest manifest,
        GeometryContext geometry)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        EnsureInitialized();
        ArgumentNullException.ThrowIfNull(omega);

        int edgeN = omega.Coefficients.Length;
        int dimG = _manifest!.LieAlgebraDimension;
        int faceCount = geometry.AmbientSpace.FaceCount
            ?? throw new InvalidOperationException(
                "GeometryContext.AmbientSpace.FaceCount must be set for GPU Jacobian.");
        int faceN = faceCount * dimG;

        // Upload omega to a persistent buffer for the operator's lifetime
        var edgeLayout = BufferLayoutDescriptor.CreateSoA("jac-omega", new[] { "c" }, edgeN);
        var omegaBuf = _nativeBackend.AllocateBuffer(edgeLayout);
        _nativeBackend.UploadBuffer(omegaBuf, omega.Coefficients);

        return new GpuLinearOperator(
            _nativeBackend,
            omegaBuf,
            edgeN,
            faceN,
            omega.Signature,
            omega.Shape.ToArray());
    }

    /// <summary>
    /// Compute gradient G = J^T * Upsilon via GPU adjoint action.
    /// For the simplified case (no mass matrix), this is just J^T * upsilon.
    /// </summary>
    public FieldTensor ComputeGradient(ILinearOperator jacobian, FieldTensor upsilon)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(jacobian);
        ArgumentNullException.ThrowIfNull(upsilon);

        return jacobian.ApplyTranspose(upsilon);
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
