namespace Gu.Interop;

/// <summary>
/// CPU reference implementation of INativeBackend.
/// Used for parity testing: CUDA results must match this within tolerance.
/// Operates on packed buffers in the same format as the GPU backend.
/// </summary>
public sealed class CpuReferenceBackend : INativeBackend
{
    private int _nextBufferId;
    private ManifestSnapshot? _manifest;
    private readonly Dictionary<int, double[]> _buffers = new();
    private ErrorPacket? _lastError = null;
    private bool _disposed;

    public InteropVersion Version { get; } = new()
    {
        Major = 1,
        Minor = 0,
        Patch = 0,
        BackendId = "cpu-reference",
    };

    public void Initialize(ManifestSnapshot manifest)
    {
        _manifest = manifest ?? throw new ArgumentNullException(nameof(manifest));
    }

    public PackedBuffer AllocateBuffer(BufferLayoutDescriptor layout)
    {
        ArgumentNullException.ThrowIfNull(layout);
        EnsureNotDisposed();

        int id = _nextBufferId++;
        _buffers[id] = new double[layout.TotalElements];

        return new PackedBuffer
        {
            BufferId = id,
            Layout = layout,
        };
    }

    public void UploadBuffer(PackedBuffer buffer, ReadOnlySpan<double> data)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        EnsureNotDisposed();

        if (!_buffers.TryGetValue(buffer.BufferId, out var storage))
            throw new InvalidOperationException($"Buffer {buffer.BufferId} not found.");

        if (data.Length > storage.Length)
            throw new ArgumentException($"Data length {data.Length} exceeds buffer size {storage.Length}.");

        data.CopyTo(storage);
    }

    public void DownloadBuffer(PackedBuffer buffer, Span<double> data)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        EnsureNotDisposed();

        if (!_buffers.TryGetValue(buffer.BufferId, out var storage))
            throw new InvalidOperationException($"Buffer {buffer.BufferId} not found.");

        storage.AsSpan(0, System.Math.Min(storage.Length, data.Length)).CopyTo(data);
    }

    public void FreeBuffer(PackedBuffer buffer)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        _buffers.Remove(buffer.BufferId);
        buffer.Dispose();
    }

    public void EvaluateCurvature(PackedBuffer omega, PackedBuffer curvatureOut)
    {
        // CPU reference: curvature evaluation stub
        // In a full implementation, this would compute F = d(omega) + (1/2)[omega, omega]
        // using the mesh and Lie algebra from the manifest.
        // For now, just copy omega -> curvatureOut as a placeholder.
        EnsureNotDisposed();
        EnsureInitialized();

        if (!_buffers.TryGetValue(omega.BufferId, out var omegaData))
            throw new InvalidOperationException("Omega buffer not found.");
        if (!_buffers.TryGetValue(curvatureOut.BufferId, out var curvatureData))
            throw new InvalidOperationException("Curvature output buffer not found.");

        int count = System.Math.Min(omegaData.Length, curvatureData.Length);
        Array.Copy(omegaData, curvatureData, count);
    }

    public void EvaluateTorsion(PackedBuffer omega, PackedBuffer torsionOut)
    {
        EnsureNotDisposed();
        EnsureInitialized();

        if (!_buffers.TryGetValue(omega.BufferId, out var omegaData))
            throw new InvalidOperationException("Omega buffer not found.");
        if (!_buffers.TryGetValue(torsionOut.BufferId, out var torsionData))
            throw new InvalidOperationException("Torsion output buffer not found.");

        // CPU reference torsion stub -- trivial torsion (T = 0) for simplest branch
        Array.Clear(torsionData);
    }

    public void EvaluateShiab(PackedBuffer omega, PackedBuffer shiabOut)
    {
        EnsureNotDisposed();
        EnsureInitialized();

        if (!_buffers.TryGetValue(omega.BufferId, out var omegaData))
            throw new InvalidOperationException("Omega buffer not found.");
        if (!_buffers.TryGetValue(shiabOut.BufferId, out var shiabData))
            throw new InvalidOperationException("Shiab output buffer not found.");

        // CPU reference Shiab stub -- identity Shiab (S = F) for simplest branch
        // In real implementation, this would apply the Shiab operator to the curvature
        int count = System.Math.Min(omegaData.Length, shiabData.Length);
        Array.Copy(omegaData, shiabData, count);
    }

    public void EvaluateResidual(PackedBuffer shiab, PackedBuffer torsion, PackedBuffer residualOut)
    {
        EnsureNotDisposed();
        EnsureInitialized();

        if (!_buffers.TryGetValue(shiab.BufferId, out var shiabData))
            throw new InvalidOperationException("Shiab buffer not found.");
        if (!_buffers.TryGetValue(torsion.BufferId, out var torsionData))
            throw new InvalidOperationException("Torsion buffer not found.");
        if (!_buffers.TryGetValue(residualOut.BufferId, out var residualData))
            throw new InvalidOperationException("Residual output buffer not found.");

        // Upsilon = S - T
        int count = System.Math.Min(System.Math.Min(shiabData.Length, torsionData.Length), residualData.Length);
        for (int i = 0; i < count; i++)
            residualData[i] = shiabData[i] - torsionData[i];
    }

    public double EvaluateObjective(PackedBuffer residual)
    {
        EnsureNotDisposed();
        EnsureInitialized();

        if (!_buffers.TryGetValue(residual.BufferId, out var residualData))
            throw new InvalidOperationException("Residual buffer not found.");

        // I2_h = (1/2) * sum(Upsilon_i^2) -- simplified, no metric/quadrature
        double sum = 0.0;
        foreach (var v in residualData)
            sum += v * v;
        return 0.5 * sum;
    }

    public ErrorPacket? GetLastError() => _lastError;

    public void Dispose()
    {
        if (!_disposed)
        {
            _buffers.Clear();
            _disposed = true;
        }
    }

    private void EnsureNotDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(CpuReferenceBackend));
    }

    private void EnsureInitialized()
    {
        if (_manifest is null)
            throw new InvalidOperationException("Backend not initialized. Call Initialize() first.");
    }
}
