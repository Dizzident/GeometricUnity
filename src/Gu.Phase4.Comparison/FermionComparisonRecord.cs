using System.Text.Json.Serialization;
using Gu.Core;
using Gu.Phase4.Observation;

namespace Gu.Phase4.Comparison;

/// <summary>
/// Comparison outcome for a single fermionic observation summary against
/// a reference candidate (from experimental data or prior theory results).
///
/// Conservative output convention:
/// - "compatible": observation does not contradict the reference.
/// - "incompatible": observation contradicts the reference within tolerance.
/// - "underdetermined": insufficient evidence to distinguish.
/// - "not-applicable": the observation is trivial or the reference is absent.
///
/// PhysicsNote (M43): This is not a mass identification. It is a structural
/// comparison of spectral invariants (chirality, mass-like scale, conjugation).
/// </summary>
public sealed class FermionComparisonRecord
{
    /// <summary>Unique comparison record ID.</summary>
    [JsonPropertyName("comparisonId")]
    public required string ComparisonId { get; init; }

    /// <summary>Source observation summary (from FermionObservationBuilder).</summary>
    [JsonPropertyName("clusterId")]
    public required string ClusterId { get; init; }

    /// <summary>Reference candidate ID being compared against.</summary>
    [JsonPropertyName("referenceCandidateId")]
    public string? ReferenceCandidateId { get; init; }

    /// <summary>
    /// Comparison outcome: "compatible", "incompatible", "underdetermined", "not-applicable".
    /// </summary>
    [JsonPropertyName("outcome")]
    public required string Outcome { get; init; }

    /// <summary>
    /// Chirality match status: "match", "mismatch", "not-applicable".
    /// </summary>
    [JsonPropertyName("chiralityMatch")]
    public required string ChiralityMatch { get; init; }

    /// <summary>
    /// Mass-like scale ratio: observed mean / reference mean.
    /// 1.0 = exact match, null if reference is absent.
    /// </summary>
    [JsonPropertyName("massLikeScaleRatio")]
    public double? MassLikeScaleRatio { get; init; }

    /// <summary>
    /// Whether the mass-like scale ratio is within tolerance.
    /// </summary>
    [JsonPropertyName("massLikeScaleWithinTolerance")]
    public bool MassLikeScaleWithinTolerance { get; init; }

    /// <summary>
    /// Tolerance used for mass-like scale comparison (relative, e.g. 0.5 = 50%).
    /// </summary>
    [JsonPropertyName("massLikeScaleTolerance")]
    public double MassLikeScaleTolerance { get; init; }

    /// <summary>Comparison notes and caveats.</summary>
    [JsonPropertyName("notes")]
    public List<string> Notes { get; init; } = new();

    /// <summary>Provenance of this comparison record.</summary>
    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}
