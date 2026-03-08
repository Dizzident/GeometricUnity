using System.Text.Json.Serialization;

namespace Gu.Core;

/// <summary>
/// A discrete tensor field carrying coefficient data with explicit signature metadata.
/// </summary>
public sealed class FieldTensor
{
    /// <summary>Human-readable label (e.g., "omega_h", "F_h", "Upsilon_h").</summary>
    [JsonPropertyName("label")]
    public required string Label { get; init; }

    /// <summary>Tensor signature declaring basis, ordering, precision, and layout.</summary>
    [JsonPropertyName("signature")]
    public required TensorSignature Signature { get; init; }

    /// <summary>
    /// Coefficient data as a flat array. Layout defined by signature.
    /// This array must not be mutated after construction; treat as immutable.
    /// </summary>
    [JsonPropertyName("coefficients")]
    public required double[] Coefficients { get; init; }

    /// <summary>Logical shape of the coefficient array.</summary>
    [JsonPropertyName("shape")]
    public required IReadOnlyList<int> Shape { get; init; }
}
