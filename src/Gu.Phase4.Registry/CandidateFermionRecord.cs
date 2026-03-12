using System.Text.Json.Serialization;
using Gu.Core;

namespace Gu.Phase4.Registry;

/// <summary>
/// A candidate fermion: an evidence-bearing equivalence class of fermionic mode families,
/// built from one or more FermionModeFamilyRecords across backgrounds.
///
/// Conservative conventions:
/// - Does NOT assert physical particle identity.
/// - All scores in [0, 1]; higher = stronger evidence.
/// - AmbiguityNotes preserves all matching ambiguities.
/// </summary>
public sealed class CandidateFermionRecord
{
    /// <summary>Unique candidate identifier (e.g., "cf-bg001-cluster0-family0").</summary>
    [JsonPropertyName("candidateId")]
    public required string CandidateId { get; init; }

    /// <summary>Primary family ID (from FermionModeFamilyRecord).</summary>
    [JsonPropertyName("primaryFamilyId")]
    public required string PrimaryFamilyId { get; init; }

    /// <summary>All contributing family IDs (across backgrounds).</summary>
    [JsonPropertyName("contributingFamilyIds")]
    public required List<string> ContributingFamilyIds { get; init; }

    /// <summary>Cluster ID from FamilyClusterRecord (if this fermion is part of a cluster).</summary>
    [JsonPropertyName("clusterIdRef")]
    public string? ClusterIdRef { get; init; }

    /// <summary>Background IDs where this candidate was observed.</summary>
    [JsonPropertyName("backgroundSet")]
    public required List<string> BackgroundSet { get; init; }

    /// <summary>
    /// Dominant chirality: "left", "right", "mixed", "trivial", "undetermined".
    /// Determined by majority vote across contributing families.
    /// </summary>
    [JsonPropertyName("dominantChirality")]
    public required string DominantChirality { get; init; }

    /// <summary>Mean |eigenvalue| envelope [min, mean, max] across contributing families.</summary>
    [JsonPropertyName("eigenvalueEnvelope")]
    public required double[] EigenvalueEnvelope { get; init; }

    /// <summary>
    /// Family ID of the conjugation-partner candidate, if known.
    /// Null if no partner has been identified.
    /// </summary>
    [JsonPropertyName("conjugationPartnerCandidateId")]
    public string? ConjugationPartnerCandidateId { get; init; }

    /// <summary>Branch persistence score [0, 1]. Mean across contributing families.</summary>
    [JsonPropertyName("branchPersistenceScore")]
    public double BranchPersistenceScore { get; init; }

    /// <summary>Refinement persistence score [0, 1]. Mean across contributing families.</summary>
    [JsonPropertyName("refinementPersistenceScore")]
    public double RefinementPersistenceScore { get; init; }

    /// <summary>Current particle claim class (C0–C5).</summary>
    [JsonPropertyName("claimClass")]
    public required string ClaimClass { get; init; }

    /// <summary>Demotion history for this candidate.</summary>
    [JsonPropertyName("demotions")]
    public List<ParticleClaimDemotion> Demotions { get; init; } = new();

    /// <summary>Number of ambiguous match contributions across all families.</summary>
    [JsonPropertyName("ambiguityCount")]
    public int AmbiguityCount { get; init; }

    /// <summary>Ambiguity notes from family matching.</summary>
    [JsonPropertyName("ambiguityNotes")]
    public List<string> AmbiguityNotes { get; init; } = new();

    /// <summary>Whether any contributing mode was computed with an unverified GPU backend.</summary>
    [JsonPropertyName("computedWithUnverifiedGpu")]
    public bool ComputedWithUnverifiedGpu { get; init; }

    /// <summary>Registry version when this record was last updated.</summary>
    [JsonPropertyName("registryVersion")]
    public required string RegistryVersion { get; init; }

    /// <summary>Provenance of this candidate record.</summary>
    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}
