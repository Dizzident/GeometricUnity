using System.Text.Json.Serialization;
using Gu.Core;

namespace Gu.Phase4.Registry;

/// <summary>
/// Particle type enum for unified registry entries.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UnifiedParticleType
{
    /// <summary>Bosonic candidate from Phase III registry.</summary>
    Boson,

    /// <summary>Fermionic candidate from Phase IV family clustering.</summary>
    Fermion,

    /// <summary>Interaction candidate derived from coupling proxy atlas.</summary>
    Interaction,
}

/// <summary>
/// A unified candidate particle record merging bosonic and fermionic evidence.
///
/// Each record aggregates:
/// - provenance from contributing sources
/// - branch stability across contexts
/// - observation confidence
/// - comparison evidence
/// - claim class (C0-C5, mirroring Phase III bosonic claim classes)
/// - demotion history
///
/// PhysicsNote: This is an evidence-bearing candidate, NOT a confirmed physical particle.
/// All claims carry explicit uncertainty and demotion records.
/// </summary>
public sealed class UnifiedParticleRecord
{
    /// <summary>Unique particle record identifier.</summary>
    [JsonPropertyName("particleId")]
    public required string ParticleId { get; init; }

    /// <summary>Schema version.</summary>
    [JsonPropertyName("schemaVersion")]
    public string SchemaVersion { get; init; } = "1.0.0";

    /// <summary>Type of this candidate: Boson, Fermion, or Interaction.</summary>
    [JsonPropertyName("particleType")]
    public required UnifiedParticleType ParticleType { get; init; }

    /// <summary>
    /// Primary source ID (family cluster ID for fermions, candidate boson ID for bosons,
    /// coupling ID for interactions).
    /// </summary>
    [JsonPropertyName("primarySourceId")]
    public required string PrimarySourceId { get; init; }

    /// <summary>All contributing source IDs (cluster IDs, mode IDs, coupling IDs).</summary>
    [JsonPropertyName("contributingSourceIds")]
    public required List<string> ContributingSourceIds { get; init; }

    /// <summary>Branch variant IDs in which this particle was observed.</summary>
    [JsonPropertyName("branchVariantSet")]
    public required List<string> BranchVariantSet { get; init; }

    /// <summary>Background IDs where this particle was observed.</summary>
    [JsonPropertyName("backgroundSet")]
    public required List<string> BackgroundSet { get; init; }

    /// <summary>
    /// Dominant chirality: "left", "right", "mixed", "conjugate-pair", "trivial", or "undetermined".
    /// Fermionic only; null for bosons.
    /// </summary>
    [JsonPropertyName("chirality")]
    public string? Chirality { get; init; }

    /// <summary>
    /// Mass-like scale envelope (min, mean, max) across contributing contexts.
    /// Derived from eigenvalue magnitudes.
    /// </summary>
    [JsonPropertyName("massLikeEnvelope")]
    public required double[] MassLikeEnvelope { get; init; }

    /// <summary>Branch stability score [0, 1] aggregated across contexts.</summary>
    [JsonPropertyName("branchStabilityScore")]
    public double BranchStabilityScore { get; init; }

    /// <summary>Observation confidence [0, 1] (0 = not yet observed through pipeline).</summary>
    [JsonPropertyName("observationConfidence")]
    public double ObservationConfidence { get; init; }

    /// <summary>Comparison evidence score [0, 1] (0 = no comparison performed).</summary>
    [JsonPropertyName("comparisonEvidenceScore")]
    public double ComparisonEvidenceScore { get; init; }

    /// <summary>Current claim class (C0-C5, matching Phase III convention).</summary>
    [JsonPropertyName("claimClass")]
    public required string ClaimClass { get; init; }

    /// <summary>Whether any contributing mode was computed with an unverified GPU backend.</summary>
    [JsonPropertyName("computedWithUnverifiedGpu")]
    public bool ComputedWithUnverifiedGpu { get; init; }

    /// <summary>Demotion history (ordered by demotedAt).</summary>
    [JsonPropertyName("demotions")]
    public List<ParticleClaimDemotion> Demotions { get; init; } = new();

    /// <summary>Ambiguity notes (e.g. from clustering or matching).</summary>
    [JsonPropertyName("ambiguityNotes")]
    public List<string> AmbiguityNotes { get; init; } = new();

    /// <summary>Registry version when this record was last written.</summary>
    [JsonPropertyName("registryVersion")]
    public required string RegistryVersion { get; init; }

    /// <summary>Provenance of this record.</summary>
    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}
