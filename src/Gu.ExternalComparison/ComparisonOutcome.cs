using System.Text.Json.Serialization;

namespace Gu.ExternalComparison;

/// <summary>
/// Outcome of a single comparison check (Section 18.4).
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ComparisonOutcome
{
    /// <summary>Comparison passed within tolerance.</summary>
    Pass,

    /// <summary>Comparison failed -- a falsifier condition triggered.</summary>
    Fail,

    /// <summary>Comparison could not be evaluated (e.g., output type mismatch).</summary>
    Invalid,

    /// <summary>Comparison ran but result is ambiguous (e.g., near tolerance boundary).</summary>
    Inconclusive,
}
