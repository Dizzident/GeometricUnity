namespace Gu.Interop;

/// <summary>
/// Handle to a GPU-allocated buffer. Managed by INativeBackend.
/// The buffer contents live on the native/GPU side; this is a lightweight
/// C# handle for referencing them.
/// </summary>
public sealed class PackedBuffer : IDisposable
{
    private bool _disposed;

    /// <summary>Unique buffer identifier (assigned by the native backend).</summary>
    public required int BufferId { get; init; }

    /// <summary>Layout descriptor for this buffer.</summary>
    public required BufferLayoutDescriptor Layout { get; init; }

    /// <summary>Native pointer to the buffer (opaque handle from native side).</summary>
    public nint NativeHandle { get; init; }

    /// <summary>Whether this buffer has been freed.</summary>
    public bool IsDisposed => _disposed;

    /// <summary>
    /// Mark the buffer as disposed. The actual native memory must be freed
    /// via INativeBackend.FreeBuffer().
    /// </summary>
    public void Dispose()
    {
        _disposed = true;
    }
}
