using System.Text.Json.Serialization;

namespace Gu.Phase2.Semantics;

/// <summary>
/// Declares a single branch variant within a branch family.
/// Every run in Phase II must carry a complete BranchVariantManifest.
/// No default branch settings may remain implicit.
/// Each variant field maps to a Phase I BranchManifest field.
/// </summary>
public sealed class BranchVariantManifest
{
    /// <summary>Unique variant identifier.</summary>
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    /// <summary>Parent family identifier.</summary>
    [JsonPropertyName("parentFamilyId")]
    public required string ParentFamilyId { get; init; }

    /// <summary>Distinguished connection A0 variant identifier.</summary>
    [JsonPropertyName("a0Variant")]
    public required string A0Variant { get; init; }

    /// <summary>Bi-connection constructor variant identifier.</summary>
    [JsonPropertyName("biConnectionVariant")]
    public required string BiConnectionVariant { get; init; }

    /// <summary>Torsion extractor variant identifier.</summary>
    [JsonPropertyName("torsionVariant")]
    public required string TorsionVariant { get; init; }

    /// <summary>Shiab operator variant identifier.</summary>
    [JsonPropertyName("shiabVariant")]
    public required string ShiabVariant { get; init; }

    /// <summary>Observation/extraction projector family variant.</summary>
    [JsonPropertyName("observationVariant")]
    public required string ObservationVariant { get; init; }

    /// <summary>Observed-sector extraction map variant.</summary>
    [JsonPropertyName("extractionVariant")]
    public required string ExtractionVariant { get; init; }

    /// <summary>Gauge-fixing/slice realization variant.</summary>
    [JsonPropertyName("gaugeVariant")]
    public required string GaugeVariant { get; init; }

    /// <summary>Analytic regularity branch variant.</summary>
    [JsonPropertyName("regularityVariant")]
    public required string RegularityVariant { get; init; }

    /// <summary>Adjoint/Hessian pairing conventions variant.</summary>
    [JsonPropertyName("pairingVariant")]
    public required string PairingVariant { get; init; }

    /// <summary>
    /// Expected claim ceiling: the highest claim class this variant is expected to support.
    /// </summary>
    [JsonPropertyName("expectedClaimCeiling")]
    public required string ExpectedClaimCeiling { get; init; }

    /// <summary>Human-readable notes.</summary>
    [JsonPropertyName("notes")]
    public string? Notes { get; init; }
}
