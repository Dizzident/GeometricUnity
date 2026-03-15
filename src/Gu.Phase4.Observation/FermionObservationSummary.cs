using System.Text.Json.Serialization;
using Gu.Core;
using Gu.Phase4.Chirality;
using Gu.Phase4.Fermions;

namespace Gu.Phase4.Observation;

/// <summary>
/// Observed-space summary of a fermionic mode family cluster.
///
/// Produced by pulling the fermionic mode family's spectral properties through
/// the sigma_h map to X_h (base space). This is a coarse summary; full
/// eigenvector pullback is out of scope at this phase.
///
/// Physical interpretation: the observed chirality, mass-like scale, and
/// conjugation status are the key fermionic observables accessible via sigma_h.
///
/// PhysicsNote: This summary is NOT a mass. It is a candidate mass-like invariant.
/// Physical mass identification requires M43 comparison.
/// </summary>
public sealed class FermionObservationSummary
{
    /// <summary>Source cluster ID (from FamilyClusterRecord).</summary>
    [JsonPropertyName("clusterId")]
    public required string ClusterId { get; init; }

    /// <summary>Schema version.</summary>
    [JsonPropertyName("schemaVersion")]
    public string SchemaVersion { get; init; } = "1.0.0";

    /// <summary>Observed chirality classification (string tag, canonical X-space value).</summary>
    [JsonPropertyName("observedChirality")]
    public required string ObservedChirality { get; init; }

    /// <summary>
    /// X-chirality decomposition (base, physically observed 4D chirality).
    /// This is the canonical chirality for comparison to external targets.
    /// Always present when dimX is even (dimX=4 in our case).
    /// </summary>
    [JsonPropertyName("xChirality")]
    public required ChiralityDecomposition XChirality { get; init; }

    /// <summary>
    /// Y-chirality decomposition (full ambient space). Diagnostic only — not directly
    /// observable but needed for consistency checks and decomposition validation.
    /// Null when dimY is odd (Y-chirality undefined).
    /// </summary>
    [JsonPropertyName("yChirality")]
    public ChiralityDecomposition? YChirality { get; init; }

    /// <summary>
    /// Observed mass-like scale envelope [min, mean, max] from eigenvalue magnitudes.
    /// Units are dimensionless (lattice units) unless calibration is applied.
    /// </summary>
    [JsonPropertyName("massLikeEnvelope")]
    public required double[] MassLikeEnvelope { get; init; }

    /// <summary>Whether a conjugation partner was identified for this cluster.</summary>
    [JsonPropertyName("hasConjugatePair")]
    public bool HasConjugatePair { get; init; }

    /// <summary>ID of the conjugate cluster, if known.</summary>
    [JsonPropertyName("conjugateClusterId")]
    public string? ConjugateClusterId { get; init; }

    /// <summary>Branch persistence score of the cluster [0, 1].</summary>
    [JsonPropertyName("branchPersistenceScore")]
    public double BranchPersistenceScore { get; init; }

    /// <summary>Ambiguity score of the cluster [0, 1].</summary>
    [JsonPropertyName("ambiguityScore")]
    public double AmbiguityScore { get; init; }

    /// <summary>
    /// Whether the observation is trivial (mixed/undetermined chirality,
    /// or singleton with insufficient evidence).
    /// Trivial observations should not be directly compared to physical particles.
    /// </summary>
    [JsonPropertyName("isTrivial")]
    public bool IsTrivial { get; init; }

    /// <summary>
    /// Observation path label classifying how this observation was produced.
    ///
    /// "proxy-observation": produced from cluster-level spectral summaries (chirality
    ///   tag, eigenvalue envelope) without a full sigma_h pullback from Y to X. This is
    ///   the status of all current (Phase X and earlier) fermion observations. Per
    ///   D-P11-010, proxy observations must not be described as full draft-aligned
    ///   pullback evidence.
    ///
    /// "full-pullback": produced via a full sigma_h pullback path from Y_h to X_h.
    ///   Not yet implemented in this repository.
    /// </summary>
    [JsonPropertyName("observationPathLabel")]
    public required string ObservationPathLabel { get; init; }

    /// <summary>Observation notes (warnings, physics caveats).</summary>
    [JsonPropertyName("notes")]
    public List<string> Notes { get; init; } = new();

    /// <summary>Provenance of this observation.</summary>
    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}

/// <summary>
/// Well-known observation path label constants for FermionObservationSummary.
/// Per D-P11-010: proxy observations must not be described as full pullback evidence.
/// </summary>
public static class ObservationPathLabels
{
    /// <summary>
    /// The observation is produced from cluster-level spectral summaries without
    /// performing a full sigma_h pullback. This is the status of all current
    /// fermion observations (Phase X and earlier).
    /// </summary>
    public const string ProxyObservation = "proxy-observation";

    /// <summary>
    /// The observation is produced via a full sigma_h pullback path from Y_h to X_h.
    /// Not yet implemented in this repository.
    /// </summary>
    public const string FullPullback = "full-pullback";
}

/// <summary>
/// Observed-space summary of a boson-fermion interaction (coupling proxy).
/// </summary>
public sealed class InteractionObservationSummary
{
    /// <summary>Coupling atlas ID.</summary>
    [JsonPropertyName("atlasId")]
    public required string AtlasId { get; init; }

    /// <summary>Boson mode ID involved in this interaction.</summary>
    [JsonPropertyName("bosonModeId")]
    public required string BosonModeId { get; init; }

    /// <summary>Fermion mode IDs involved.</summary>
    [JsonPropertyName("fermionModeIds")]
    public required List<string> FermionModeIds { get; init; }

    /// <summary>Mean coupling proxy magnitude across all (i,j) pairs for this boson mode.</summary>
    [JsonPropertyName("meanCouplingMagnitude")]
    public double MeanCouplingMagnitude { get; init; }

    /// <summary>Max coupling proxy magnitude.</summary>
    [JsonPropertyName("maxCouplingMagnitude")]
    public double MaxCouplingMagnitude { get; init; }

    /// <summary>Number of non-zero couplings observed.</summary>
    [JsonPropertyName("nonZeroCouplingCount")]
    public int NonZeroCouplingCount { get; init; }

    /// <summary>Normalization convention used.</summary>
    [JsonPropertyName("normalizationConvention")]
    public required string NormalizationConvention { get; init; }

    /// <summary>Provenance of this observation.</summary>
    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}
