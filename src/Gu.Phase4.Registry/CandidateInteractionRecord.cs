using System.Text.Json.Serialization;
using Gu.Core;

namespace Gu.Phase4.Registry;

/// <summary>
/// A candidate interaction: evidence of a coupling between a bosonic and fermionic mode.
///
/// Derived from BosonFermionCouplingRecord (Phase IV M40).
/// Does NOT assert physical coupling constant; the proxy magnitude is
/// normalization-convention-dependent.
/// </summary>
public sealed class CandidateInteractionRecord
{
    /// <summary>Unique interaction identifier.</summary>
    [JsonPropertyName("interactionId")]
    public required string InteractionId { get; init; }

    /// <summary>Reference to the bosonic candidate ID.</summary>
    [JsonPropertyName("bosonCandidateId")]
    public required string BosonCandidateId { get; init; }

    /// <summary>Reference to fermion candidate I (psi-bar side).</summary>
    [JsonPropertyName("fermionCandidateIdI")]
    public required string FermionCandidateIdI { get; init; }

    /// <summary>Reference to fermion candidate J (psi side).</summary>
    [JsonPropertyName("fermionCandidateIdJ")]
    public required string FermionCandidateIdJ { get; init; }

    /// <summary>
    /// Coupling proxy magnitude envelope [min, mean, max] across atlas entries
    /// that contribute to this interaction.
    /// </summary>
    [JsonPropertyName("couplingMagnitudeEnvelope")]
    public required double[] CouplingMagnitudeEnvelope { get; init; }

    /// <summary>Normalization convention used for the coupling proxies.</summary>
    [JsonPropertyName("normalizationConvention")]
    public required string NormalizationConvention { get; init; }

    /// <summary>Whether the coupling proxy magnitude is above the zero threshold.</summary>
    [JsonPropertyName("isNonZero")]
    public bool IsNonZero { get; init; }

    /// <summary>Selection rule notes (e.g., "forbidden by chirality mismatch").</summary>
    [JsonPropertyName("selectionRuleNotes")]
    public List<string> SelectionRuleNotes { get; init; } = new();

    /// <summary>Claim class for this interaction record.</summary>
    [JsonPropertyName("claimClass")]
    public required string ClaimClass { get; init; }

    /// <summary>Registry version when this record was last updated.</summary>
    [JsonPropertyName("registryVersion")]
    public required string RegistryVersion { get; init; }

    /// <summary>Provenance of this interaction record.</summary>
    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}
