using System.Text.Json.Serialization;

namespace Gu.Phase4.Chirality;

/// <summary>
/// Chirality decomposition of a single fermionic mode.
///
/// For a mode phi with eigenvector coefficients in spinor-gauge space:
///   P_L phi = left-chiral projection
///   P_R phi = right-chiral projection
///   leftFraction  = ||P_L phi||^2 / ||phi||^2
///   rightFraction = ||P_R phi||^2 / ||phi||^2
///   mixedFraction = 1 - leftFraction - rightFraction  (should be near 0 for Hermitian D_h)
///
/// Sign convention (from ChiralityConventionSpec.SignConvention):
///   "left-is-minus": P_L = (I - Gamma_chi)/2,  P_R = (I + Gamma_chi)/2
///   "left-is-plus":  P_L = (I + Gamma_chi)/2,  P_R = (I - Gamma_chi)/2
/// </summary>
public sealed class ChiralityDecomposition
{
    /// <summary>Mode ID this decomposition applies to.</summary>
    [JsonPropertyName("modeId")]
    public required string ModeId { get; init; }

    /// <summary>Fraction of mode weight in left-chiral sector [0, 1].</summary>
    [JsonPropertyName("leftFraction")]
    public required double LeftFraction { get; init; }

    /// <summary>Fraction of mode weight in right-chiral sector [0, 1].</summary>
    [JsonPropertyName("rightFraction")]
    public required double RightFraction { get; init; }

    /// <summary>
    /// Mixed fraction: weight not captured by left or right projectors.
    /// For a Hermitian Gamma_chi with eigenvalues +1/-1 this should be near 0.
    /// </summary>
    [JsonPropertyName("mixedFraction")]
    public required double MixedFraction { get; init; }

    /// <summary>
    /// Chirality classification:
    /// "left" if leftFraction > 1 - threshold,
    /// "right" if rightFraction > 1 - threshold,
    /// "mixed" otherwise.
    /// "trivial" for odd-dimensional Y where chirality is undefined.
    /// </summary>
    [JsonPropertyName("chiralityTag")]
    public required string ChiralityTag { get; init; }

    /// <summary>
    /// Chirality status (following architect convention from ARCH_P4.md §3.10):
    /// "definite-left", "definite-right", "mixed", "trivial", "not-applicable".
    /// </summary>
    [JsonPropertyName("chiralityStatus")]
    public required string ChiralityStatus { get; init; }

    /// <summary>
    /// Leakage diagnostic: |leftFraction + rightFraction - 1.0|.
    /// Near 0 means the projectors are complete (Gamma_chi^2 = I).
    /// Large values indicate a convention mismatch or assembly error.
    /// </summary>
    [JsonPropertyName("leakageDiagnostic")]
    public required double LeakageDiagnostic { get; init; }

    /// <summary>Sign convention used: "left-is-minus" or "left-is-plus".</summary>
    [JsonPropertyName("signConvention")]
    public required string SignConvention { get; init; }

    /// <summary>Optional diagnostic notes (e.g. odd-dim triviality note).</summary>
    [JsonPropertyName("diagnosticNotes")]
    public List<string>? DiagnosticNotes { get; init; }
}
