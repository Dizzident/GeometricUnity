using System.Text.Json.Serialization;

namespace Gu.Phase4.Spin;

/// <summary>
/// Specifies the spinor conjugation / dual convention.
/// For Riemannian signature: psi_bar = psi^dagger (Hermitian adjoint).
/// For Lorentzian: psi_bar = psi^dagger * Gamma_0 (Dirac adjoint).
/// </summary>
public sealed class ConjugationConventionSpec
{
    /// <summary>Unique identifier for this convention.</summary>
    [JsonPropertyName("conventionId")]
    public required string ConventionId { get; init; }

    /// <summary>
    /// Type of conjugation.
    /// "hermitian": psi_bar = psi^dagger (Riemannian default).
    /// "dirac-adjoint": psi_bar = psi^dagger * Gamma_0 (Lorentzian).
    /// "majorana": psi = psi^C (Majorana condition, branch variant).
    /// </summary>
    [JsonPropertyName("conjugationType")]
    public required string ConjugationType { get; init; }

    /// <summary>
    /// Whether charge conjugation C * psi^* is available for this signature.
    /// For dimY=14: 14 mod 8 = 6 → charge conjugation exists.
    /// For dimY=5: odd dimension, charge conjugation still exists but properties differ.
    /// </summary>
    [JsonPropertyName("hasChargeConjugation")]
    public required bool HasChargeConjugation { get; init; }

    /// <summary>Optional reference to charge conjugation matrix (branch-defined).</summary>
    [JsonPropertyName("chargeConjugationMatrixId")]
    public string? ChargeConjugationMatrixId { get; init; }
}
