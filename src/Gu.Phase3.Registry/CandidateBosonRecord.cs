using System.Text.Json.Serialization;

namespace Gu.Phase3.Registry;

/// <summary>
/// Aggregated polarization across contributing modes.
/// </summary>
public sealed class PolarizationEnvelope
{
    /// <summary>Most frequent dominant polarization class across contributing modes.</summary>
    [JsonPropertyName("dominantClass")]
    public required string DominantClass { get; init; }

    /// <summary>Minimum dominance fraction across contributing modes.</summary>
    [JsonPropertyName("minFraction")]
    public required double MinFraction { get; init; }

    /// <summary>Maximum dominance fraction across contributing modes.</summary>
    [JsonPropertyName("maxFraction")]
    public required double MaxFraction { get; init; }
}

/// <summary>
/// Aggregated symmetry across contributing modes.
/// </summary>
public sealed class SymmetryEnvelope
{
    /// <summary>Minimum parity eigenvalue across modes (null if any are mixed/null).</summary>
    [JsonPropertyName("minParity")]
    public int? MinParity { get; init; }

    /// <summary>Maximum parity eigenvalue across modes (null if any are mixed/null).</summary>
    [JsonPropertyName("maxParity")]
    public int? MaxParity { get; init; }

    /// <summary>Union of all symmetry labels across contributing modes.</summary>
    [JsonPropertyName("unionLabels")]
    public required IReadOnlyList<string> UnionLabels { get; init; }
}

/// <summary>
/// Aggregated interaction proxies across contributing modes.
/// </summary>
public sealed class InteractionProxyEnvelope
{
    /// <summary>Minimum |CubicResponse| across all proxy records.</summary>
    [JsonPropertyName("minCubicResponse")]
    public required double MinCubicResponse { get; init; }

    /// <summary>Maximum |CubicResponse| across all proxy records.</summary>
    [JsonPropertyName("maxCubicResponse")]
    public required double MaxCubicResponse { get; init; }

    /// <summary>Total count of interaction proxy records.</summary>
    [JsonPropertyName("proxyCount")]
    public required int ProxyCount { get; init; }
}

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

    /// <summary>Aggregated polarization envelope across contributing modes (null if no property vectors provided).</summary>
    [JsonPropertyName("polarizationEnvelope")]
    public PolarizationEnvelope? PolarizationEnvelope { get; init; }

    /// <summary>Aggregated symmetry envelope across contributing modes (null if no property vectors provided).</summary>
    [JsonPropertyName("symmetryEnvelope")]
    public SymmetryEnvelope? SymmetryEnvelope { get; init; }

    /// <summary>Aggregated interaction proxy envelope (null if no modes have interaction proxies).</summary>
    [JsonPropertyName("interactionProxyEnvelope")]
    public InteractionProxyEnvelope? InteractionProxyEnvelope { get; init; }

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

    /// <summary>
    /// Whether any contributing mode was computed with an unverified GPU backend.
    /// When true, DemotionEngine caps claim class at C1.
    /// </summary>
    [JsonPropertyName("computedWithUnverifiedGpu")]
    public bool ComputedWithUnverifiedGpu { get; init; }

    /// <summary>Number of ambiguous matches from the contributing mode family.</summary>
    [JsonPropertyName("ambiguityCount")]
    public int AmbiguityCount { get; init; }

    /// <summary>Ambiguity notes (matching ambiguities, multiple identifications).</summary>
    [JsonPropertyName("ambiguityNotes")]
    public IReadOnlyList<string> AmbiguityNotes { get; init; } = Array.Empty<string>();

    /// <summary>Registry version when this record was last updated.</summary>
    [JsonPropertyName("registryVersion")]
    public required string RegistryVersion { get; init; }
}
