using System.Text.Json.Serialization;

namespace Gu.Phase3.Registry;

/// <summary>
/// A candidate boson: an evidence-bearing equivalence class of bosonic
/// linearized modes around one or more admissible backgrounds.
///
/// This is intentionally weaker than "physical particle."
/// All candidate boson records carry background provenance.
/// </summary>
public sealed class CandidateBosonRecord
{
    /// <summary>Unique candidate identifier.</summary>
    [JsonPropertyName("candidateId")]
    public required string CandidateId { get; init; }

    /// <summary>Primary mode family ID from tracking.</summary>
    [JsonPropertyName("primaryFamilyId")]
    public required string PrimaryFamilyId { get; init; }

    /// <summary>All contributing mode IDs.</summary>
    [JsonPropertyName("contributingModeIds")]
    public required IReadOnlyList<string> ContributingModeIds { get; init; }

    /// <summary>Background IDs where this candidate was observed.</summary>
    [JsonPropertyName("backgroundSet")]
    public required IReadOnlyList<string> BackgroundSet { get; init; }

    /// <summary>Branch variant IDs where this candidate persists.</summary>
    [JsonPropertyName("branchVariantSet")]
    public IReadOnlyList<string> BranchVariantSet { get; init; } = Array.Empty<string>();

    /// <summary>Mass-like scale envelope: (min, mean, max) across contexts.</summary>
    [JsonPropertyName("massLikeEnvelope")]
    public required double[] MassLikeEnvelope { get; init; }

    /// <summary>Multiplicity envelope: (min, mean, max) across contexts.</summary>
    [JsonPropertyName("multiplicityEnvelope")]
    public required int[] MultiplicityEnvelope { get; init; }

    /// <summary>Gauge leak score envelope: (min, mean, max).</summary>
    [JsonPropertyName("gaugeLeakEnvelope")]
    public required double[] GaugeLeakEnvelope { get; init; }

    /// <summary>Branch stability score [0, 1]. Higher = more stable.</summary>
    [JsonPropertyName("branchStabilityScore")]
    public double BranchStabilityScore { get; init; }

    /// <summary>Refinement stability score [0, 1].</summary>
    [JsonPropertyName("refinementStabilityScore")]
    public double RefinementStabilityScore { get; init; }

    /// <summary>Backend stability score [0, 1].</summary>
    [JsonPropertyName("backendStabilityScore")]
    public double BackendStabilityScore { get; init; }

    /// <summary>Observation stability score [0, 1].</summary>
    [JsonPropertyName("observationStabilityScore")]
    public double ObservationStabilityScore { get; init; }

    /// <summary>Current claim class.</summary>
    [JsonPropertyName("claimClass")]
    public required BosonClaimClass ClaimClass { get; init; }

    /// <summary>Demotion history.</summary>
    [JsonPropertyName("demotions")]
    public IReadOnlyList<BosonDemotionRecord> Demotions { get; init; } = Array.Empty<BosonDemotionRecord>();

    /// <summary>Ambiguity notes (matching ambiguities, multiple identifications).</summary>
    [JsonPropertyName("ambiguityNotes")]
    public IReadOnlyList<string> AmbiguityNotes { get; init; } = Array.Empty<string>();

    /// <summary>Registry version when this record was last updated.</summary>
    [JsonPropertyName("registryVersion")]
    public required string RegistryVersion { get; init; }
}
