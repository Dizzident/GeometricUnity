using System.Text.Json.Serialization;
using Gu.Core;

namespace Gu.Phase4.Couplings;

/// <summary>
/// Records metadata about a variation of the Dirac operator D
/// induced by a bosonic mode perturbation.
///
/// The actual matrix entries are stored externally; this record
/// captures provenance and conventions only.
/// </summary>
public sealed class DiracVariationBundle
{
    [JsonPropertyName("variationId")]
    public required string VariationId { get; init; }

    [JsonPropertyName("bosonModeId")]
    public required string BosonModeId { get; init; }

    [JsonPropertyName("fermionBackgroundId")]
    public required string FermionBackgroundId { get; init; }

    /// <summary>
    /// Normalization convention for the variation matrix.
    /// Typical values: "unit-boson", "l2-normalized", "raw".
    /// </summary>
    [JsonPropertyName("normalizationConvention")]
    public required string NormalizationConvention { get; init; }

    [JsonPropertyName("symmetryNotes")]
    public List<string> SymmetryNotes { get; init; } = new();

    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}
