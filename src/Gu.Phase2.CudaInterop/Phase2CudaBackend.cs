using System.Runtime.InteropServices;
using Gu.Phase2.Semantics;

namespace Gu.Phase2.CudaInterop;

/// <summary>
/// CUDA implementation of Phase II Jacobian, Hessian, and batch kernels.
/// Delegates to the gu_phase2_cuda native library via P/Invoke.
///
/// This backend requires the native library to be built and available
/// in the runtime library path.
/// </summary>
public sealed class Phase2CudaBackend : IPhase2JacobianKernel, IPhase2HessianKernel, IPhase2BatchKernel, IDisposable
{
    private bool _disposed;
    private bool _initialized;
    private int _edgeCount;
    private int _faceCount;
    private int _dimG;

    /// <summary>
    /// Initialize the Phase II CUDA backend with geometry configuration.
    /// </summary>
    public void Initialize(int edgeCount, int faceCount, int dimG, int maxEdgesPerFace)
    {
        EnsureNotDisposed();
        var config = new Phase2NativeBindings.Phase2KernelConfig
        {
            EdgeCount = edgeCount,
            FaceCount = faceCount,
            DimG = dimG,
            MaxEdgesPerFace = maxEdgesPerFace,
        };

        int result = Phase2NativeBindings.Initialize(in config);
        if (result != 0)
            throw new InvalidOperationException($"Phase II CUDA init failed: error {result}");
        _edgeCount = edgeCount;
        _faceCount = faceCount;
        _dimG = dimG;
        _initialized = true;
    }

    public unsafe void ApplyJv(ReadOnlySpan<double> u, ReadOnlySpan<double> v, Span<double> result,
        BranchVariantManifest variant)
    {
        EnsureReady();

        fixed (double* uPtr = u, vPtr = v, rPtr = result)
        {
            int rc = Phase2NativeBindings.JacobianAction(
                (nint)uPtr, (nint)vPtr, (nint)rPtr,
                _edgeCount, _faceCount, _dimG,
                GetBranchFlags(variant));
            CheckResult(rc, "gu_phase2_jacobian_action");
        }
    }

    public unsafe void ApplyJtw(ReadOnlySpan<double> u, ReadOnlySpan<double> w, Span<double> result,
        BranchVariantManifest variant)
    {
        EnsureReady();

        fixed (double* uPtr = u, wPtr = w, rPtr = result)
        {
            int rc = Phase2NativeBindings.AdjointAction(
                (nint)uPtr, (nint)wPtr, (nint)rPtr,
                _edgeCount, _faceCount, _dimG,
                GetBranchFlags(variant));
            CheckResult(rc, "gu_phase2_adjoint_action");
        }
    }

    public unsafe void ApplyHv(ReadOnlySpan<double> u, ReadOnlySpan<double> v, Span<double> result,
        BranchVariantManifest variant, double lambda)
    {
        EnsureReady();

        fixed (double* uPtr = u, vPtr = v, rPtr = result)
        {
            int rc = Phase2NativeBindings.HessianAction(
                (nint)uPtr, (nint)vPtr, (nint)rPtr,
                _edgeCount, _faceCount, _dimG,
                lambda, GetBranchFlags(variant));
            CheckResult(rc, "gu_phase2_hessian_action");
        }
    }

    public unsafe void EvaluateBatch(
        IReadOnlyList<BranchVariantManifest> variants,
        ReadOnlySpan<double> connectionStates,
        Span<double> residualsOut,
        int fieldDof,
        int residualDof)
    {
        EnsureReady();
        int batchSize = variants.Count;
        var branchFlags = new int[batchSize];
        for (int i = 0; i < batchSize; i++)
            branchFlags[i] = GetBranchFlags(variants[i]);

        fixed (double* csPtr = connectionStates, rPtr = residualsOut)
        fixed (int* bfPtr = branchFlags)
        {
            int rc = Phase2NativeBindings.BatchResidual(
                (nint)csPtr, (nint)rPtr,
                batchSize, fieldDof, residualDof,
                (nint)bfPtr);
            CheckResult(rc, "gu_phase2_batch_residual");
        }
    }

    /// <summary>
    /// Encode branch variant parameters as an integer flag word for kernel dispatch.
    /// Bit layout:
    ///   bits 0-1: torsion type (0=trivial, 1=augmented)
    ///   bits 2-3: shiab type (0=identity, 1=trace-free)
    ///   bits 4-5: bi-connection type (0=A0+omega, 1=A0-omega)
    /// </summary>
    private static int GetBranchFlags(BranchVariantManifest variant)
    {
        int flags = 0;
        if (variant.TorsionVariant.Contains("augmented", StringComparison.OrdinalIgnoreCase))
            flags |= 1;
        if (!variant.ShiabVariant.Contains("identity", StringComparison.OrdinalIgnoreCase))
            flags |= (1 << 2);
        if (variant.BiConnectionVariant.Contains("minus", StringComparison.OrdinalIgnoreCase))
            flags |= (1 << 4);
        return flags;
    }

    private void EnsureReady()
    {
        EnsureNotDisposed();
        if (!_initialized)
            throw new InvalidOperationException("Phase II CUDA backend not initialized.");
    }

    private void EnsureNotDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    private static void CheckResult(int result, string functionName)
    {
        if (result != 0)
            throw new InvalidOperationException(
                $"Native function {functionName} failed with error code {result}");
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            if (_initialized)
            {
                Phase2NativeBindings.Shutdown();
                _initialized = false;
            }
            _disposed = true;
        }
    }
}
