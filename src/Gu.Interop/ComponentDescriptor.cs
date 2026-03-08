namespace Gu.Interop;

/// <summary>
/// Describes a single component within a packed buffer layout.
/// Offset and stride define the memory layout for SoA packing.
/// </summary>
public sealed class ComponentDescriptor
{
    /// <summary>Semantic name of this component (e.g., "omega_0", "curvature_12").</summary>
    public required string ComponentName { get; init; }

    /// <summary>Byte offset from buffer start to the first element of this component.</summary>
    public required int Offset { get; init; }

    /// <summary>Byte stride between consecutive elements of this component.</summary>
    public required int Stride { get; init; }

    /// <summary>Number of elements in this component.</summary>
    public required int Count { get; init; }
}
