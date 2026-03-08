namespace Gu.Interop;

/// <summary>
/// Describes the memory layout of a packed buffer for GPU dispatch.
/// Supports Structure-of-Arrays (SoA) and Array-of-Structures (AoS) packing.
/// SoA is preferred for GPU compute (coalesced memory access).
/// </summary>
public sealed class BufferLayoutDescriptor
{
    /// <summary>Unique layout identifier.</summary>
    public required string LayoutId { get; init; }

    /// <summary>Packing mode: "SoA" (Structure of Arrays) or "AoS" (Array of Structures).</summary>
    public required string PackingMode { get; init; }

    /// <summary>Numeric type: "float64" (double precision) or "float32".</summary>
    public required string NumericType { get; init; }

    /// <summary>Total number of scalar elements in the buffer.</summary>
    public required int TotalElements { get; init; }

    /// <summary>Component descriptors defining the layout of each field component.</summary>
    public required IReadOnlyList<ComponentDescriptor> Components { get; init; }

    /// <summary>
    /// Total buffer size in bytes.
    /// </summary>
    public int TotalBytes => TotalElements * BytesPerElement;

    /// <summary>
    /// Bytes per scalar element (8 for float64, 4 for float32).
    /// </summary>
    public int BytesPerElement => NumericType switch
    {
        "float64" => 8,
        "float32" => 4,
        _ => 8,
    };

    /// <summary>
    /// Create a SoA layout for a set of named components, each with the same element count.
    /// Components are packed contiguously: [comp0_all, comp1_all, comp2_all, ...].
    /// </summary>
    public static BufferLayoutDescriptor CreateSoA(
        string layoutId,
        IReadOnlyList<string> componentNames,
        int elementsPerComponent,
        string numericType = "float64")
    {
        int bytesPerElement = numericType == "float32" ? 4 : 8;
        int stride = bytesPerElement;
        var components = new List<ComponentDescriptor>(componentNames.Count);

        for (int i = 0; i < componentNames.Count; i++)
        {
            components.Add(new ComponentDescriptor
            {
                ComponentName = componentNames[i],
                Offset = i * elementsPerComponent * bytesPerElement,
                Stride = stride,
                Count = elementsPerComponent,
            });
        }

        return new BufferLayoutDescriptor
        {
            LayoutId = layoutId,
            PackingMode = "SoA",
            NumericType = numericType,
            TotalElements = componentNames.Count * elementsPerComponent,
            Components = components,
        };
    }
}
