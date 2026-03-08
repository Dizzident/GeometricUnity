using Gu.Branching;
using Gu.Core;
using Gu.Solvers;

namespace Gu.Interop;

/// <summary>
/// CUDA implementation of ISolverBackend.
/// Wraps an INativeBackend and handles packing/unpacking between
/// semantic types (FieldTensor, DerivedState) and packed GPU buffers.
///
/// The SolverOrchestrator controls the iteration loop; this backend
/// only does the math per-step on the GPU.
/// </summary>
public sealed class CudaSolverBackend : ISolverBackend, IDisposable
{
    private readonly INativeBackend _native;
    private readonly bool _ownsNative;
    private ManifestSnapshot? _manifest;
    private bool _disposed;

    // Persistent GPU buffers (allocated on first use, reused across iterations)
    private PackedBuffer? _omegaBuffer;
    private PackedBuffer? _a0Buffer;
    private PackedBuffer? _curvatureBuffer;
    private PackedBuffer? _torsionBuffer;
    private PackedBuffer? _shiabBuffer;
    private PackedBuffer? _residualBuffer;
    private BufferLayoutDescriptor? _fieldLayout;

    // Cached signatures for unpacking
    private TensorSignature? _omegaSignature;
    private IReadOnlyList<int>? _omegaShape;

    public CudaSolverBackend(INativeBackend nativeBackend, bool ownsNative = true)
    {
        _native = nativeBackend ?? throw new ArgumentNullException(nameof(nativeBackend));
        _ownsNative = ownsNative;
    }

    /// <summary>
    /// Initialize the backend with manifest information.
    /// Must be called before any compute operations.
    /// </summary>
    public void Initialize(ManifestSnapshot manifest)
    {
        _manifest = manifest ?? throw new ArgumentNullException(nameof(manifest));
        _native.Initialize(manifest);
    }

    public DerivedState EvaluateDerived(
        FieldTensor omega, FieldTensor a0, BranchManifest manifest, GeometryContext geometry)
    {
        EnsureInitialized(omega);

        // Upload omega to GPU
        UploadField(omega, ref _omegaBuffer);

        // Curvature: F = d(omega) + (1/2)[omega, omega]
        EnsureBuffer(ref _curvatureBuffer);
        _native.EvaluateCurvature(_omegaBuffer!, _curvatureBuffer!);
        var curvature = DownloadField(_curvatureBuffer!, "F_h", omega.Signature, omega.Shape);

        // Torsion: T_h
        EnsureBuffer(ref _torsionBuffer);
        _native.EvaluateTorsion(_omegaBuffer!, _torsionBuffer!);
        var torsionT = DownloadField(_torsionBuffer!, "T_h", omega.Signature, omega.Shape);

        // Shiab: S_h
        EnsureBuffer(ref _shiabBuffer);
        _native.EvaluateShiab(_omegaBuffer!, _shiabBuffer!);
        var shiabS = DownloadField(_shiabBuffer!, "S_h", omega.Signature, omega.Shape);

        // Residual: Upsilon = S - T (computed on GPU)
        EnsureBuffer(ref _residualBuffer);
        _native.EvaluateResidual(_shiabBuffer!, _torsionBuffer!, _residualBuffer!);
        var upsilon = DownloadField(_residualBuffer!, "Upsilon_h", omega.Signature, omega.Shape);

        return new DerivedState
        {
            CurvatureF = curvature,
            TorsionT = torsionT,
            ShiabS = shiabS,
            ResidualUpsilon = upsilon,
        };
    }

    public double EvaluateObjective(FieldTensor upsilon)
    {
        EnsureInitialized(upsilon);
        // Upload residual if needed and compute objective on GPU
        UploadField(upsilon, ref _residualBuffer);
        return _native.EvaluateObjective(_residualBuffer!);
    }

    public ILinearOperator BuildJacobian(
        FieldTensor omega, FieldTensor a0, FieldTensor curvatureF,
        BranchManifest manifest, GeometryContext geometry)
    {
        // Return a GPU-backed matrix-free Jacobian operator.
        // For now, this uses a finite-difference approximation through the
        // native backend's EvaluateResidual pipeline.
        return new GpuFiniteDifferenceJacobian(_native, omega, a0, _fieldLayout!, _manifest!);
    }

    public FieldTensor ComputeGradient(ILinearOperator jacobian, FieldTensor upsilon)
    {
        // G = J^T * M * Upsilon
        // For the GPU path, we use the Jacobian's ApplyTranspose.
        // The mass matrix is simplified to identity in the native backend for now.
        return jacobian.ApplyTranspose(upsilon);
    }

    public double ComputeNorm(FieldTensor v)
    {
        // L2 norm computed on CPU (field is already downloaded)
        double sum = 0;
        for (int i = 0; i < v.Coefficients.Length; i++)
            sum += v.Coefficients[i] * v.Coefficients[i];
        return System.Math.Sqrt(sum);
    }

    private void EnsureInitialized(FieldTensor referenceField)
    {
        if (_fieldLayout != null) return;

        _omegaSignature = referenceField.Signature;
        _omegaShape = referenceField.Shape;
        _fieldLayout = FieldPacker.CreateLayout(referenceField, "solver-field");
    }

    private void EnsureBuffer(ref PackedBuffer? buffer)
    {
        if (buffer == null || buffer.IsDisposed)
        {
            buffer = _native.AllocateBuffer(_fieldLayout!);
        }
    }

    private void UploadField(FieldTensor field, ref PackedBuffer? buffer)
    {
        EnsureBuffer(ref buffer);
        var packed = FieldPacker.PackToSoA(field);
        _native.UploadBuffer(buffer!, packed);
    }

    private FieldTensor DownloadField(PackedBuffer buffer, string label, TensorSignature sig, IReadOnlyList<int> shape)
    {
        return FieldPacker.DownloadField(_native, buffer, label, sig, shape);
    }

    public void Dispose()
    {
        if (_disposed) return;

        FreeBuffer(ref _omegaBuffer);
        FreeBuffer(ref _a0Buffer);
        FreeBuffer(ref _curvatureBuffer);
        FreeBuffer(ref _torsionBuffer);
        FreeBuffer(ref _shiabBuffer);
        FreeBuffer(ref _residualBuffer);

        if (_ownsNative)
            _native.Dispose();

        _disposed = true;
    }

    private void FreeBuffer(ref PackedBuffer? buffer)
    {
        if (buffer != null && !buffer.IsDisposed)
        {
            _native.FreeBuffer(buffer);
            buffer = null;
        }
    }
}

/// <summary>
/// GPU-backed finite-difference Jacobian approximation.
/// Computes J*v by: J*v ≈ (Residual(omega + eps*v) - Residual(omega)) / eps.
/// Used when analytic Jacobian kernels are not yet available.
/// </summary>
internal sealed class GpuFiniteDifferenceJacobian : ILinearOperator
{
    private readonly INativeBackend _native;
    private readonly FieldTensor _omega;
    private readonly FieldTensor _a0;
    private readonly BufferLayoutDescriptor _layout;
    private readonly ManifestSnapshot _manifest;
    private const double Epsilon = 1e-7;

    public GpuFiniteDifferenceJacobian(
        INativeBackend native, FieldTensor omega, FieldTensor a0,
        BufferLayoutDescriptor layout, ManifestSnapshot manifest)
    {
        _native = native;
        _omega = omega;
        _a0 = a0;
        _layout = layout;
        _manifest = manifest;
    }

    public TensorSignature InputSignature => _omega.Signature;
    public TensorSignature OutputSignature => _omega.Signature;
    public int InputDimension => _omega.Coefficients.Length;
    public int OutputDimension => _omega.Coefficients.Length;

    /// <summary>
    /// J*v via finite difference on the GPU residual pipeline.
    /// </summary>
    public FieldTensor Apply(FieldTensor v)
    {
        int n = _omega.Coefficients.Length;

        // Compute Residual(omega)
        var residual0 = ComputeResidualOnGpu(_omega);

        // Compute Residual(omega + eps * v)
        var perturbedCoeffs = new double[n];
        for (int i = 0; i < n; i++)
            perturbedCoeffs[i] = _omega.Coefficients[i] + Epsilon * v.Coefficients[i];
        var perturbedOmega = new FieldTensor
        {
            Label = "omega_perturbed",
            Signature = _omega.Signature,
            Coefficients = perturbedCoeffs,
            Shape = _omega.Shape,
        };
        var residual1 = ComputeResidualOnGpu(perturbedOmega);

        // J*v = (r1 - r0) / eps
        var result = new double[n];
        for (int i = 0; i < n; i++)
            result[i] = (residual1[i] - residual0[i]) / Epsilon;

        return new FieldTensor
        {
            Label = "J*v",
            Signature = _omega.Signature,
            Coefficients = result,
            Shape = _omega.Shape,
        };
    }

    /// <summary>
    /// J^T * v via column-by-column finite difference.
    /// O(n) GPU kernel launches. Acceptable for small problems.
    /// </summary>
    public FieldTensor ApplyTranspose(FieldTensor v)
    {
        int n = InputDimension;
        var result = new double[n];

        // J^T * v: (J^T v)_j = dot(J*e_j, v)
        var ej = new double[n];
        for (int j = 0; j < n; j++)
        {
            ej[j] = 1.0;
            var jEj = Apply(new FieldTensor
            {
                Label = "e_j",
                Signature = _omega.Signature,
                Coefficients = (double[])ej.Clone(),
                Shape = _omega.Shape,
            });

            double dot = 0;
            for (int i = 0; i < v.Coefficients.Length; i++)
                dot += jEj.Coefficients[i] * v.Coefficients[i];
            result[j] = dot;

            ej[j] = 0.0;
        }

        return new FieldTensor
        {
            Label = "J^T*v",
            Signature = _omega.Signature,
            Coefficients = result,
            Shape = _omega.Shape,
        };
    }

    private double[] ComputeResidualOnGpu(FieldTensor omega)
    {
        int n = omega.Coefficients.Length;
        var omegaPacked = FieldPacker.PackToSoA(omega);

        var omegaBuf = _native.AllocateBuffer(_layout);
        var curvBuf = _native.AllocateBuffer(_layout);
        var torsionBuf = _native.AllocateBuffer(_layout);
        var shiabBuf = _native.AllocateBuffer(_layout);
        var residualBuf = _native.AllocateBuffer(_layout);

        _native.UploadBuffer(omegaBuf, omegaPacked);
        _native.EvaluateCurvature(omegaBuf, curvBuf);
        _native.EvaluateTorsion(omegaBuf, torsionBuf);
        _native.EvaluateShiab(omegaBuf, shiabBuf);
        _native.EvaluateResidual(shiabBuf, torsionBuf, residualBuf);

        var resultPacked = new double[n];
        _native.DownloadBuffer(residualBuf, resultPacked);

        _native.FreeBuffer(omegaBuf);
        _native.FreeBuffer(curvBuf);
        _native.FreeBuffer(torsionBuf);
        _native.FreeBuffer(shiabBuf);
        _native.FreeBuffer(residualBuf);

        // Unpack SoA -> row-major
        var resultField = FieldPacker.UnpackFromSoA(
            resultPacked, "residual", omega.Signature, omega.Shape);
        return resultField.Coefficients;
    }
}
