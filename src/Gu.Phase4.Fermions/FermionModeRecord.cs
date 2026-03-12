using System.Text.Json.Serialization;
using Gu.Core;

namespace Gu.Phase4.Fermions;

/// <summary>
/// Chirality decomposition of a fermionic mode.
/// Left-chiral, right-chiral, and mixed fractions must sum to 1.
/// </summary>
public sealed class ChiralityDecompositionRecord
{
    /// <summary>Fraction of mode weight in the left-chiral sector [0, 1].</summary>
    [JsonPropertyName("leftFraction")]
    public required double LeftFraction { get; init; }

    /// <summary>Fraction of mode weight in the right-chiral sector [0, 1].</summary>
    [JsonPropertyName("rightFraction")]
    public required double RightFraction { get; init; }

    /// <summary>Chirality leakage: how much weight is in mixed / undefined chirality sector.</summary>
    [JsonPropertyName("mixedFraction")]
    public required double MixedFraction { get; init; }

    /// <summary>
    /// Net chirality: leftFraction - rightFraction in [-1, 1].
    /// +1 = purely left-chiral, -1 = purely right-chiral.
    /// </summary>
    [JsonIgnore]
    public double NetChirality => LeftFraction - RightFraction;

    /// <summary>Sign convention used for this decomposition (from ChiralityConventionSpec).</summary>
    [JsonPropertyName("signConvention")]
    public required string SignConvention { get; init; }
}

/// <summary>
/// Records whether a fermionic mode has a conjugation partner in the spectrum.
/// </summary>
public sealed class ConjugationPairingRecord
{
    /// <summary>Whether a conjugation partner was found.</summary>
    [JsonPropertyName("hasPair")]
    public required bool HasPair { get; init; }

    /// <summary>Mode ID of the conjugation partner (if found).</summary>
    [JsonPropertyName("partnerModeId")]
    public string? PartnerModeId { get; init; }

    /// <summary>Eigenvalue of the partner (for diagnostics).</summary>
    [JsonPropertyName("partnerEigenvalue")]
    public double? PartnerEigenvalue { get; init; }

    /// <summary>Conjugation convention used for pairing.</summary>
    [JsonPropertyName("conjugationType")]
    public required string ConjugationType { get; init; }

    /// <summary>Notes if pairing is ambiguous or unresolved.</summary>
    [JsonPropertyName("notes")]
    public string? Notes { get; init; }
}

/// <summary>
/// A single fermionic mode: eigenvector of D_h(z_*) phi_i = lambda_i M_psi phi_i.
///
/// Each record carries the mode vector, eigenvalue, chirality decomposition,
/// conjugation info, gauge-leak score, and stability metadata.
///
/// PhysicsNote: lambda_i is not automatically a physical mass. It is a
/// candidate mass-like eigenvalue. Physical interpretation requires going
/// through the full observation chain.
/// </summary>
public sealed class FermionModeRecord
{
    /// <summary>Unique mode identifier.</summary>
    [JsonPropertyName("modeId")]
    public required string ModeId { get; init; }

    /// <summary>Schema version.</summary>
    [JsonPropertyName("schemaVersion")]
    public string SchemaVersion { get; init; } = "1.0.0";

    /// <summary>Fermionic background record ID this mode was computed around.</summary>
    [JsonPropertyName("backgroundId")]
    public required string BackgroundId { get; init; }

    /// <summary>Branch variant ID.</summary>
    [JsonPropertyName("branchVariantId")]
    public required string BranchVariantId { get; init; }

    /// <summary>Layout ID.</summary>
    [JsonPropertyName("layoutId")]
    public required string LayoutId { get; init; }

    /// <summary>0-based index of this mode in the computed spectrum (sorted by |lambda|).</summary>
    [JsonPropertyName("modeIndex")]
    public required int ModeIndex { get; init; }

    /// <summary>
    /// Generalized eigenvalue lambda_i such that D_h phi_i = lambda_i M_psi phi_i.
    /// Real part.
    /// </summary>
    [JsonPropertyName("eigenvalueRe")]
    public required double EigenvalueRe { get; init; }

    /// <summary>Eigenvalue imaginary part (should be near zero for Hermitian D_h).</summary>
    [JsonPropertyName("eigenvalueIm")]
    public required double EigenvalueIm { get; init; }

    /// <summary>|lambda_i| = candidate mass-like invariant.</summary>
    [JsonIgnore]
    public double EigenvalueMagnitude =>
        System.Math.Sqrt(EigenvalueRe * EigenvalueRe + EigenvalueIm * EigenvalueIm);

    /// <summary>Residual ||D_h phi_i - lambda_i M_psi phi_i|| / ||phi_i||.</summary>
    [JsonPropertyName("residualNorm")]
    public required double ResidualNorm { get; init; }

    /// <summary>
    /// Eigenvector as a flat interleaved (Re, Im) array (optional; may be null if only
    /// eigenvalue summaries are stored to reduce artifact size).
    /// </summary>
    [JsonPropertyName("eigenvectorCoefficients")]
    public double[]? EigenvectorCoefficients { get; init; }

    /// <summary>Chirality decomposition of this mode.</summary>
    [JsonPropertyName("chiralityDecomposition")]
    public required ChiralityDecompositionRecord ChiralityDecomposition { get; init; }

    /// <summary>Conjugation pairing information.</summary>
    [JsonPropertyName("conjugationPairing")]
    public required ConjugationPairingRecord ConjugationPairing { get; init; }

    /// <summary>
    /// Gauge-leak score [0, 1]: fraction of mode weight in gauge directions.
    /// A well-resolved physical mode should have GaugeLeakScore near 0.
    /// </summary>
    [JsonPropertyName("gaugeLeakScore")]
    public double GaugeLeakScore { get; init; }

    /// <summary>
    /// Whether gauge/redundancy reduction was applied before this mode was computed.
    /// Physical interpretation requires reduction.
    /// </summary>
    [JsonPropertyName("gaugeReductionApplied")]
    public bool GaugeReductionApplied { get; init; }

    /// <summary>Backend used: "cpu-reference", "cuda", etc.</summary>
    [JsonPropertyName("backend")]
    public required string Backend { get; init; }

    /// <summary>
    /// Whether this mode was computed with an unverified GPU backend.
    /// Caps claim class downstream.
    /// </summary>
    [JsonPropertyName("computedWithUnverifiedGpu")]
    public bool ComputedWithUnverifiedGpu { get; init; }

    /// <summary>Stability score across branch variants [0, 1].</summary>
    [JsonPropertyName("branchStabilityScore")]
    public double BranchStabilityScore { get; init; }

    /// <summary>Stability score across refinement levels [0, 1].</summary>
    [JsonPropertyName("refinementStabilityScore")]
    public double RefinementStabilityScore { get; init; }

    /// <summary>Replay tier of this mode record (R0-R3).</summary>
    [JsonPropertyName("replayTier")]
    public string ReplayTier { get; init; } = "R0";

    /// <summary>Ambiguity notes (e.g. near-degenerate eigenvalue cluster).</summary>
    [JsonPropertyName("ambiguityNotes")]
    public List<string> AmbiguityNotes { get; init; } = new();

    /// <summary>Provenance of this mode record.</summary>
    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}
