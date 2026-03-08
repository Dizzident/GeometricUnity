using Gu.Interop;

namespace Gu.Interop.Tests;

/// <summary>
/// Tests for NativeFieldBuffer: pinned memory wrapper for safe
/// CPU/GPU data exchange.
/// </summary>
public class NativeFieldBufferTests
{
    [Fact]
    public void Constructor_AllocatesByCount()
    {
        using var buffer = new NativeFieldBuffer(10);

        Assert.Equal(10, buffer.ElementCount);
        Assert.Equal(80, buffer.ByteCount); // 10 * 8 bytes
        Assert.False(buffer.IsDisposed);
    }

    [Fact]
    public void Constructor_FromSpan_CopiesData()
    {
        var source = new double[] { 1.0, 2.0, 3.0, 4.0 };
        using var buffer = new NativeFieldBuffer(source);

        Assert.Equal(4, buffer.ElementCount);
        Assert.Equal(1.0, buffer.Span[0]);
        Assert.Equal(4.0, buffer.Span[3]);
    }

    [Fact]
    public void Constructor_ZeroCount_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new NativeFieldBuffer(0));
    }

    [Fact]
    public void Constructor_NegativeCount_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new NativeFieldBuffer(-1));
    }

    [Fact]
    public void Constructor_EmptySpan_Throws()
    {
        Assert.Throws<ArgumentException>(() => new NativeFieldBuffer(ReadOnlySpan<double>.Empty));
    }

    [Fact]
    public void Address_ReturnsNonZero()
    {
        using var buffer = new NativeFieldBuffer(5);
        Assert.NotEqual(nint.Zero, buffer.Address);
    }

    [Fact]
    public void Address_AfterDispose_Throws()
    {
        var buffer = new NativeFieldBuffer(5);
        buffer.Dispose();
        Assert.Throws<ObjectDisposedException>(() => _ = buffer.Address);
    }

    [Fact]
    public void Span_ReadWrite()
    {
        using var buffer = new NativeFieldBuffer(3);

        buffer.Span[0] = 10.0;
        buffer.Span[1] = 20.0;
        buffer.Span[2] = 30.0;

        Assert.Equal(10.0, buffer.ReadOnlySpan[0]);
        Assert.Equal(20.0, buffer.ReadOnlySpan[1]);
        Assert.Equal(30.0, buffer.ReadOnlySpan[2]);
    }

    [Fact]
    public void CopyFrom_CopiesData()
    {
        using var buffer = new NativeFieldBuffer(4);
        buffer.CopyFrom(new double[] { 5.0, 6.0, 7.0, 8.0 });

        Assert.Equal(5.0, buffer.Span[0]);
        Assert.Equal(8.0, buffer.Span[3]);
    }

    [Fact]
    public void CopyFrom_TooLarge_Throws()
    {
        using var buffer = new NativeFieldBuffer(2);
        Assert.Throws<ArgumentException>(() =>
            buffer.CopyFrom(new double[] { 1.0, 2.0, 3.0 }));
    }

    [Fact]
    public void CopyTo_CopiesData()
    {
        using var buffer = new NativeFieldBuffer(new double[] { 1.1, 2.2, 3.3 });

        var dest = new double[3];
        buffer.CopyTo(dest);

        Assert.Equal(1.1, dest[0]);
        Assert.Equal(2.2, dest[1]);
        Assert.Equal(3.3, dest[2]);
    }

    [Fact]
    public void ToArray_ReturnsIndependentCopy()
    {
        using var buffer = new NativeFieldBuffer(new double[] { 1.0, 2.0 });
        var arr = buffer.ToArray();

        Assert.Equal(1.0, arr[0]);
        Assert.Equal(2.0, arr[1]);

        // Mutating the returned array should not affect the buffer
        arr[0] = 99.0;
        Assert.Equal(1.0, buffer.Span[0]);
    }

    [Fact]
    public void Clear_ZerosBuffer()
    {
        using var buffer = new NativeFieldBuffer(new double[] { 1.0, 2.0, 3.0 });
        buffer.Clear();

        Assert.Equal(0.0, buffer.Span[0]);
        Assert.Equal(0.0, buffer.Span[1]);
        Assert.Equal(0.0, buffer.Span[2]);
    }

    [Fact]
    public void Dispose_IsIdempotent()
    {
        var buffer = new NativeFieldBuffer(3);
        buffer.Dispose();
        buffer.Dispose(); // Should not throw
        Assert.True(buffer.IsDisposed);
    }

    [Fact]
    public void Span_AfterDispose_Throws()
    {
        var buffer = new NativeFieldBuffer(3);
        buffer.Dispose();
        Assert.Throws<ObjectDisposedException>(() => _ = buffer.Span);
    }

    [Fact]
    public void ReadOnlySpan_AfterDispose_Throws()
    {
        var buffer = new NativeFieldBuffer(3);
        buffer.Dispose();
        Assert.Throws<ObjectDisposedException>(() => _ = buffer.ReadOnlySpan);
    }

    [Fact]
    public void CopyFrom_AfterDispose_Throws()
    {
        var buffer = new NativeFieldBuffer(3);
        buffer.Dispose();
        Assert.Throws<ObjectDisposedException>(() =>
            buffer.CopyFrom(new double[] { 1.0 }));
    }

    [Fact]
    public void PinnedAddress_StableAcrossGC()
    {
        using var buffer = new NativeFieldBuffer(new double[] { 42.0 });
        nint addr1 = buffer.Address;

        // Force GC to verify pinning holds
        GC.Collect(2, GCCollectionMode.Forced, true);
        GC.WaitForPendingFinalizers();

        nint addr2 = buffer.Address;
        Assert.Equal(addr1, addr2);

        // Data should still be intact after GC
        Assert.Equal(42.0, buffer.Span[0]);
    }
}
