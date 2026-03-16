using System.Text.Json.Serialization;
using Gu.Core;

namespace Gu.Phase4.Couplings;

/// <summary>
/// Coupling proxy between one bosonic mode and two fermionic modes:
///   g_ijk = &lt;phi_i^dagger, delta_D[b_k] phi_j&gt;
///
/// This is a proxy (not a derived coupling constant) because:
/// - M_branch = 0 (P4-IA-004)
/// - The normalization is configurable
/// - Sign/phase conventions affect interpretation
/// </summary>
public sealed class BosonFermionCouplingRecord
{
    [JsonPropertyName("couplingId")]
    public required string CouplingId { get; init; }

    [JsonPropertyName("bosonModeId")]
    public required string BosonModeId { get; init; }

    [JsonPropertyName("fermionModeIdI")]
    public required string FermionModeIdI { get; init; }

    [JsonPropertyName("fermionModeIdJ")]
    public required string FermionModeIdJ { get; init; }

    /// <summary>Real part of g_ijk.</summary>
    [JsonPropertyName("couplingProxyReal")]
    public required double CouplingProxyReal { get; init; }

    /// <summary>Imaginary part of g_ijk.</summary>
    [JsonPropertyName("couplingProxyImaginary")]
    public double CouplingProxyImaginary { get; init; }

    /// <summary>|g_ijk| = sqrt(Re^2 + Im^2).</summary>
    [JsonPropertyName("couplingProxyMagnitude")]
    public required double CouplingProxyMagnitude { get; init; }

    [JsonPropertyName("normalizationConvention")]
    public required string NormalizationConvention { get; init; }

    [JsonPropertyName("selectionRuleNotes")]
    public List<string> SelectionRuleNotes { get; init; } = new();

    /// <summary>
    /// Branch stability score in [0, 1]: how stable this coupling is across branch variants.
    /// 0 = unstable, 1 = perfectly stable. Filled by downstream atlas comparison.
    /// </summary>
    [JsonPropertyName("branchStabilityScore")]
    public double BranchStabilityScore { get; init; }

    [JsonPropertyName("variationMethod")]
    public string VariationMethod { get; init; } = "unknown";

    [JsonPropertyName("variationEvidenceId")]
    public string? VariationEvidenceId { get; init; }

    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}
