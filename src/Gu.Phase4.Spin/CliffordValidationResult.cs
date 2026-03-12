using System.Text.Json.Serialization;

namespace Gu.Phase4.Spin;

/// <summary>
/// Result of validating a set of gamma matrices against the Clifford algebra identities.
/// Records the maximum anticommutation error, chirality check, and conjugation consistency.
/// </summary>
public sealed class CliffordValidationResult
{
    /// <summary>Convention ID that was validated.</summary>
    [JsonPropertyName("conventionId")]
    public required string ConventionId { get; init; }

    /// <summary>
    /// Maximum error in the anticommutation relation:
    /// max_{a,b} || {Gamma_a, Gamma_b} - 2*eta_{ab}*I ||_F
    /// where ||.||_F is the Frobenius norm.
    /// </summary>
    [JsonPropertyName("anticommutationMaxError")]
    public required double AnticommutationMaxError { get; init; }

    /// <summary>
    /// Error in Gamma_chi^2 = I:
    /// || Gamma_chi^2 - I ||_F
    /// Zero if HasChirality == false (odd dimension).
    /// </summary>
    [JsonPropertyName("chiralitySquareError")]
    public required double ChiralitySquareError { get; init; }

    /// <summary>
    /// Error in chirality anticommutation:
    /// max_mu || {Gamma_chi, Gamma_mu} ||_F
    /// Zero if HasChirality == false.
    /// </summary>
    [JsonPropertyName("chiralityAnticommutationMaxError")]
    public required double ChiralityAnticommutationMaxError { get; init; }

    /// <summary>
    /// Error measuring whether each Gamma_mu is Hermitian (Riemannian) or anti-Hermitian:
    /// max_mu || Gamma_mu - Gamma_mu^dagger ||_F  (for Riemannian)
    /// </summary>
    [JsonPropertyName("conjugationConsistencyError")]
    public required double ConjugationConsistencyError { get; init; }

    /// <summary>Whether all checks passed within tolerance.</summary>
    [JsonPropertyName("passed")]
    public required bool Passed { get; init; }

    /// <summary>Tolerance used for pass/fail decision.</summary>
    [JsonPropertyName("tolerance")]
    public required double Tolerance { get; init; }

    /// <summary>Optional diagnostic notes from validation.</summary>
    [JsonPropertyName("diagnosticNotes")]
    public List<string> DiagnosticNotes { get; init; } = new();
}
