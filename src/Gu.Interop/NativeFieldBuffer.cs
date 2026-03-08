using System.Runtime.InteropServices;

namespace Gu.Interop;

/// <summary>
/// Managed wrapper for a native field buffer that uses pinned memory to allow
/// safe, zero-copy data exchange between managed C# code and native/CUDA code.
///
/// Pins a managed double[] array so the native side can read/write directly
/// to the memory address without GC interference. The pinned handle is released
/// on Dispose.
///
/// Usage pattern:
///   using var buffer = new NativeFieldBuffer(elementCount);
///   buffer.Span[0] = 1.0; // write from C#
///   SomeNativeFunction(buffer.Address, buffer.ByteCount); // pass to native
///   double v = buffer.Span[0]; // read back from C#
/// </summary>
public sealed class NativeFieldBuffer : IDisposable
{
    private readonly double[] _data;
    private GCHandle _handle;
    private bool _disposed;

    /// <summary>
    /// Create a new pinned field buffer with the specified element count.
    /// All elements are initialized to zero.
    /// </summary>
    /// <param name="elementCount">Number of double-precision elements.</param>
    /// <exception cref="ArgumentOutOfRangeException">If elementCount is not positive.</exception>
    public NativeFieldBuffer(int elementCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(elementCount);

        _data = new double[elementCount];
        _handle = GCHandle.Alloc(_data, GCHandleType.Pinned);
    }

    /// <summary>
    /// Create a new pinned field buffer from existing data.
    /// The data is copied into the pinned buffer.
    /// </summary>
    /// <param name="source">Source data to copy into the buffer.</param>
    /// <exception cref="ArgumentException">If source is empty.</exception>
    public NativeFieldBuffer(ReadOnlySpan<double> source)
    {
        if (source.IsEmpty)
            throw new ArgumentException("Source data cannot be empty.", nameof(source));

        _data = new double[source.Length];
        source.CopyTo(_data);
        _handle = GCHandle.Alloc(_data, GCHandleType.Pinned);
    }

    /// <summary>Number of double-precision elements in the buffer.</summary>
    public int ElementCount => _data.Length;

    /// <summary>Total byte count of the buffer.</summary>
    public int ByteCount => _data.Length * sizeof(double);

    /// <summary>
    /// Pinned native address of the buffer data.
    /// Valid as long as this object has not been disposed.
    /// </summary>
    /// <exception cref="ObjectDisposedException">If the buffer has been disposed.</exception>
    public nint Address
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return _handle.AddrOfPinnedObject();
        }
    }

    /// <summary>
    /// Span over the buffer data for managed read/write access.
    /// </summary>
    /// <exception cref="ObjectDisposedException">If the buffer has been disposed.</exception>
    public Span<double> Span
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return _data.AsSpan();
        }
    }

    /// <summary>
    /// Read-only span over the buffer data.
    /// </summary>
    /// <exception cref="ObjectDisposedException">If the buffer has been disposed.</exception>
    public ReadOnlySpan<double> ReadOnlySpan
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return _data.AsSpan();
        }
    }

    /// <summary>
    /// Copy data from a source span into this buffer.
    /// </summary>
    /// <param name="source">Source data. Must have length less than or equal to ElementCount.</param>
    /// <exception cref="ArgumentException">If source is longer than the buffer.</exception>
    public void CopyFrom(ReadOnlySpan<double> source)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (source.Length > _data.Length)
            throw new ArgumentException(
                $"Source length {source.Length} exceeds buffer capacity {_data.Length}.",
                nameof(source));
        source.CopyTo(_data);
    }

    /// <summary>
    /// Copy data from this buffer to a destination span.
    /// </summary>
    /// <param name="destination">Destination span. Must have length greater than or equal to ElementCount.</param>
    public void CopyTo(Span<double> destination)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _data.AsSpan().CopyTo(destination);
    }

    /// <summary>
    /// Copy the buffer contents to a new managed array.
    /// </summary>
    public double[] ToArray()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return (double[])_data.Clone();
    }

    /// <summary>
    /// Zero-fill the entire buffer.
    /// </summary>
    public void Clear()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        Array.Clear(_data);
    }

    /// <summary>Whether this buffer has been disposed and its memory unpinned.</summary>
    public bool IsDisposed => _disposed;

    public void Dispose()
    {
        if (!_disposed)
        {
            if (_handle.IsAllocated)
                _handle.Free();
            _disposed = true;
        }
    }
}
