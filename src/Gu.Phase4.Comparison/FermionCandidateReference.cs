using System.Text.Json.Serialization;

namespace Gu.Phase4.Comparison;

/// <summary>
/// A reference entry representing a known particle candidate for comparison
/// against fermionic observation summaries.
///
/// These are intentionally minimal: chirality, mass-like scale, conjugation.
/// Physical particle names are NOT used here — labels are conservative and
/// must not be interpreted as identification.
/// </summary>
public sealed class FermionCandidateReference
{
    /// <summary>Reference candidate ID (e.g., "ref-lepton-like-0").</summary>
    [JsonPropertyName("referenceId")]
    public required string ReferenceId { get; init; }

    /// <summary>Conservative label (NOT a physical particle name).</summary>
    [JsonPropertyName("label")]
    public required string Label { get; init; }

    /// <summary>Expected chirality: "left", "right", "mixed", "conjugate-pair".</summary>
    [JsonPropertyName("expectedChirality")]
    public required string ExpectedChirality { get; init; }

    /// <summary>
    /// Expected mass-like scale envelope [min, mean, max] in dimensionless lattice units.
    /// Null means "unconstrained".
    /// </summary>
    [JsonPropertyName("expectedMassLikeEnvelope")]
    public double[]? ExpectedMassLikeEnvelope { get; init; }

    /// <summary>Whether a conjugation partner is expected.</summary>
    [JsonPropertyName("expectsConjugatePair")]
    public bool ExpectsConjugatePair { get; init; }

    /// <summary>Notes on this reference (e.g., calibration caveats).</summary>
    [JsonPropertyName("notes")]
    public List<string> Notes { get; init; } = new();
}
