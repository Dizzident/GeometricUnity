using System.Runtime.InteropServices;

namespace Gu.Interop;

/// <summary>
/// CUDA implementation of INativeBackend.
/// Delegates to the gu_cuda_core native library via P/Invoke.
/// This is the production GPU compute backend.
/// </summary>
public sealed class CudaNativeBackend : INativeBackend
{
    private bool _disposed;
    private bool _initialized;

    public InteropVersion Version
    {
        get
        {
            var nv = NativeBindings.GetVersion();
            return new InteropVersion
            {
                Major = nv.Major,
                Minor = nv.Minor,
                Patch = nv.Patch,
                BackendId = "cuda",
            };
        }
    }

    public void Initialize(ManifestSnapshot manifest)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        EnsureNotDisposed();

        var native = new NativeBindings.NativeManifestSnapshot
        {
            BaseDimension = manifest.BaseDimension,
            AmbientDimension = manifest.AmbientDimension,
            LieAlgebraDimension = manifest.LieAlgebraDimension,
            MeshCellCount = manifest.MeshCellCount,
            MeshVertexCount = manifest.MeshVertexCount,
        };

        int result = NativeBindings.Initialize(in native);
        CheckResult(result, "gu_initialize");
        _initialized = true;
    }

    public PackedBuffer AllocateBuffer(BufferLayoutDescriptor layout)
    {
        ArgumentNullException.ThrowIfNull(layout);
        EnsureNotDisposed();
        EnsureInitialized();

        int handle = NativeBindings.AllocateBuffer(layout.TotalElements, layout.BytesPerElement);
        if (handle < 0)
            throw new InvalidOperationException($"Failed to allocate GPU buffer: handle={handle}");

        return new PackedBuffer
        {
            BufferId = handle,
            Layout = layout,
            NativeHandle = handle,
        };
    }

    public unsafe void UploadBuffer(PackedBuffer buffer, ReadOnlySpan<double> data)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        EnsureNotDisposed();

        fixed (double* ptr = data)
        {
            int result = NativeBindings.UploadBuffer(
                buffer.BufferId,
                (nint)ptr,
                (nuint)(data.Length * sizeof(double)));
            CheckResult(result, "gu_upload_buffer");
        }
    }

    public unsafe void DownloadBuffer(PackedBuffer buffer, Span<double> data)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        EnsureNotDisposed();

        fixed (double* ptr = data)
        {
            int result = NativeBindings.DownloadBuffer(
                buffer.BufferId,
                (nint)ptr,
                (nuint)(data.Length * sizeof(double)));
            CheckResult(result, "gu_download_buffer");
        }
    }

    public void FreeBuffer(PackedBuffer buffer)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        int result = NativeBindings.FreeBuffer(buffer.BufferId);
        CheckResult(result, "gu_free_buffer");
        buffer.Dispose();
    }

    public void EvaluateCurvature(PackedBuffer omega, PackedBuffer curvatureOut)
    {
        EnsureNotDisposed();
        EnsureInitialized();
        int result = NativeBindings.EvaluateCurvature(omega.BufferId, curvatureOut.BufferId);
        CheckResult(result, "gu_evaluate_curvature");
    }

    public void EvaluateTorsion(PackedBuffer omega, PackedBuffer torsionOut)
    {
        EnsureNotDisposed();
        EnsureInitialized();
        int result = NativeBindings.EvaluateTorsion(omega.BufferId, torsionOut.BufferId);
        CheckResult(result, "gu_evaluate_torsion");
    }

    public void EvaluateShiab(PackedBuffer omega, PackedBuffer shiabOut)
    {
        EnsureNotDisposed();
        EnsureInitialized();
        int result = NativeBindings.EvaluateShiab(omega.BufferId, shiabOut.BufferId);
        CheckResult(result, "gu_evaluate_shiab");
    }

    public void EvaluateResidual(PackedBuffer shiab, PackedBuffer torsion, PackedBuffer residualOut)
    {
        EnsureNotDisposed();
        EnsureInitialized();
        int result = NativeBindings.EvaluateResidual(shiab.BufferId, torsion.BufferId, residualOut.BufferId);
        CheckResult(result, "gu_evaluate_residual");
    }

    public double EvaluateObjective(PackedBuffer residual)
    {
        EnsureNotDisposed();
        EnsureInitialized();
        int result = NativeBindings.EvaluateObjective(residual.BufferId, out double objective);
        CheckResult(result, "gu_evaluate_objective");
        return objective;
    }

    public ErrorPacket? GetLastError()
    {
        nint ptr = NativeBindings.GetLastError();
        if (ptr == nint.Zero)
            return null;

        var native = Marshal.PtrToStructure<NativeBindings.NativeErrorPacket>(ptr);
        if (native.Code == 0)
            return null;

        return new ErrorPacket
        {
            Code = native.Code,
            Message = native.Message ?? "Unknown error",
            Source = native.Source,
        };
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            if (_initialized)
            {
                NativeBindings.Shutdown();
                _initialized = false;
            }
            _disposed = true;
        }
    }

    private void EnsureNotDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(CudaNativeBackend));
    }

    private void EnsureInitialized()
    {
        if (!_initialized)
            throw new InvalidOperationException("Backend not initialized. Call Initialize() first.");
    }

    private static void CheckResult(int result, string functionName)
    {
        if (result != 0)
        {
            throw new InvalidOperationException(
                $"Native function {functionName} failed with error code {result}");
        }
    }
}
