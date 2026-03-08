using System.Text.Json.Serialization;

namespace Gu.Core;

/// <summary>
/// Classification of observed output type (Section 17.4).
/// The system must never force a quantitative comparison on an output that is only structural.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OutputType
{
    /// <summary>Exact structural output (e.g., topology, symmetry group).</summary>
    ExactStructural,

    /// <summary>Semi-quantitative output (e.g., order-of-magnitude, scaling behavior).</summary>
    SemiQuantitative,

    /// <summary>Fully quantitative output (e.g., numerical values with tolerances).</summary>
    Quantitative,
}
