using System.Text.Json.Serialization;
using Gu.Phase2.Semantics;

namespace Gu.Phase2.Recovery;

/// <summary>
/// A typed physical-identification attempt (Section 9.8).
/// All 6 fields are required. The IdentificationGate evaluates whether
/// the combination supports the claimed interpretation class.
/// </summary>
public sealed class PhysicalIdentificationRecord
{
    /// <summary>Unique identification record identifier.</summary>
    [JsonPropertyName("identificationId")]
    public required string IdentificationId { get; init; }

    /// <summary>1. Formal source: identifies the native Y-space source object and its extraction chain.</summary>
    [JsonPropertyName("formalSource")]
    public required string FormalSource { get; init; }

    /// <summary>2. Observation/extraction map: identifies sigma_h, pullback, and projector used.</summary>
    [JsonPropertyName("observationExtractionMap")]
    public required string ObservationExtractionMap { get; init; }

    /// <summary>3. Support status: "theorem-supported", "numerical-only", or "conjectural".</summary>
    [JsonPropertyName("supportStatus")]
    public required string SupportStatus { get; init; }

    /// <summary>4. Approximation status: what approximations were made and their validity regime.</summary>
    [JsonPropertyName("approximationStatus")]
    public required string ApproximationStatus { get; init; }

    /// <summary>5. Comparison target: the external physical quantity this is being compared to.</summary>
    [JsonPropertyName("comparisonTarget")]
    public required string ComparisonTarget { get; init; }

    /// <summary>6. Falsifier: what observable outcome would falsify this identification.</summary>
    [JsonPropertyName("falsifier")]
    public required string Falsifier { get; init; }

    /// <summary>The resolved claim class after gate evaluation.</summary>
    [JsonPropertyName("resolvedClaimClass")]
    public required ClaimClass ResolvedClaimClass { get; init; }
}
