using System.Text.Json.Serialization;
using Gu.Core;

namespace Gu.Phase4.Registry;

/// <summary>
/// A unified particle claim: links one or more bosonic/fermionic/interaction candidates
/// under a single claim, with an overall claim class and demotion history.
///
/// Claim classes:
///   C0 = background artifact (no claim)
///   C1 = numerical hint (unverified GPU or low persistence)
///   C2 = reproducible mode (R2 replay, CPU verified)
///   C3 = branch-stable candidate (persists across branch variants)
///   C4 = observation-consistent candidate (matched observable prediction)
///   C5 = comparison-consistent candidate (matches known experimental data)
///
/// Demotions lower the claim class when evidence weakens.
/// </summary>
public sealed class ParticleClaimRecord
{
    /// <summary>Unique claim identifier.</summary>
    [JsonPropertyName("claimId")]
    public required string ClaimId { get; init; }

    /// <summary>Human-readable label for this claim (conservative, not "electron" etc.).</summary>
    [JsonPropertyName("label")]
    public required string Label { get; init; }

    /// <summary>Boson candidate ID referenced by this claim (null if purely fermionic).</summary>
    [JsonPropertyName("bosonCandidateId")]
    public string? BosonCandidateId { get; init; }

    /// <summary>Fermion candidate IDs referenced by this claim.</summary>
    [JsonPropertyName("fermionCandidateIds")]
    public List<string> FermionCandidateIds { get; init; } = new();

    /// <summary>Interaction candidate IDs referenced by this claim.</summary>
    [JsonPropertyName("interactionCandidateIds")]
    public List<string> InteractionCandidateIds { get; init; } = new();

    /// <summary>Current overall claim class (C0–C5).</summary>
    [JsonPropertyName("claimClass")]
    public required string ClaimClass { get; init; }

    /// <summary>Aggregate branch stability score [0, 1] across all contributors.</summary>
    [JsonPropertyName("branchStabilityScore")]
    public double BranchStabilityScore { get; init; }

    /// <summary>Demotion history for this claim.</summary>
    [JsonPropertyName("demotions")]
    public List<ParticleClaimDemotion> Demotions { get; init; } = new();

    /// <summary>Ambiguity notes: multiple identification candidates, uncertain matching.</summary>
    [JsonPropertyName("ambiguityNotes")]
    public List<string> AmbiguityNotes { get; init; } = new();

    /// <summary>Provenance aggregator: all input provenances that contributed to this claim.</summary>
    [JsonPropertyName("provenanceIds")]
    public List<string> ProvenanceIds { get; init; } = new();

    /// <summary>Registry version when this claim was recorded.</summary>
    [JsonPropertyName("registryVersion")]
    public required string RegistryVersion { get; init; }

    /// <summary>Provenance of this claim record.</summary>
    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}
