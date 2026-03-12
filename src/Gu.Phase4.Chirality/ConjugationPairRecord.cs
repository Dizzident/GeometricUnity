using System.Text.Json.Serialization;

namespace Gu.Phase4.Chirality;

/// <summary>
/// Records a detected conjugation pair between two fermionic modes.
///
/// Under charge conjugation C, a mode phi with eigenvalue lambda maps to
/// a mode C*phi with eigenvalue -lambda* (or lambda for Majorana).
/// A conjugation pair is identified by high overlap between C*phi_A and phi_B.
///
/// For Hermitian D_h (Riemannian case):
///   - Eigenvalues are real
///   - Pairs: (lambda, -lambda) or (lambda, lambda) for zero modes
///   - Overlap: check ||phi_B - C*phi_A|| / ||phi_A||
/// </summary>
public sealed class ConjugationPairRecord
{
    /// <summary>Unique pair identifier.</summary>
    [JsonPropertyName("pairId")]
    public required string PairId { get; init; }

    /// <summary>Mode ID of the first mode in the pair.</summary>
    [JsonPropertyName("modeIdA")]
    public required string ModeIdA { get; init; }

    /// <summary>Mode ID of the second mode in the pair.</summary>
    [JsonPropertyName("modeIdB")]
    public required string ModeIdB { get; init; }

    /// <summary>Overlap score between C*phi_A and phi_B [0, 1].</summary>
    [JsonPropertyName("overlapScore")]
    public required double OverlapScore { get; init; }

    /// <summary>
    /// Conjugation type used: "hermitian" (phi -> phi*), "charge" (C-matrix), "majorana".
    /// </summary>
    [JsonPropertyName("conjugationType")]
    public required string ConjugationType { get; init; }

    /// <summary>Whether this pair is confidently identified (overlapScore >= threshold).</summary>
    [JsonPropertyName("isConfident")]
    public required bool IsConfident { get; init; }

    /// <summary>Eigenvalue of mode A.</summary>
    [JsonPropertyName("eigenvalueA")]
    public required double EigenvalueA { get; init; }

    /// <summary>Eigenvalue of mode B.</summary>
    [JsonPropertyName("eigenvalueB")]
    public required double EigenvalueB { get; init; }

    /// <summary>Expected eigenvalue of conjugate (e.g., -eigenvalueA for charge conjugation).</summary>
    [JsonPropertyName("expectedConjugateEigenvalue")]
    public required double ExpectedConjugateEigenvalue { get; init; }

    /// <summary>Notes on the pairing (ambiguity, near-degenerate, etc.).</summary>
    [JsonPropertyName("notes")]
    public List<string>? Notes { get; init; }
}
