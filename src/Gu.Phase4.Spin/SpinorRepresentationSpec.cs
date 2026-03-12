using System.Text.Json.Serialization;
using Gu.Core;

namespace Gu.Phase4.Spin;

/// <summary>
/// Complete specification for a spinor representation on Y_h.
/// Aggregates Clifford signature, gamma convention, chirality convention,
/// conjugation convention, and dimensional data.
///
/// PHYSICS: Fermions live in sections of S(Y) ⊗ V_rho, where S(Y) is the
/// spinor bundle associated to the spin structure on Y and V_rho is the gauge
/// representation bundle. SpinorComponents here refers only to the Clifford
/// spinor index (dim = 2^floor(dimY/2)); the gauge index is tracked separately
/// in FermionFieldLayout.
/// </summary>
public sealed class SpinorRepresentationSpec
{
    /// <summary>Unique identifier for this specification.</summary>
    [JsonPropertyName("spinorSpecId")]
    public required string SpinorSpecId { get; init; }

    /// <summary>Schema version.</summary>
    [JsonPropertyName("schemaVersion")]
    public string SchemaVersion { get; init; } = "1.0.0";

    /// <summary>Spacetime dimension of Y (e.g. 5 for toy, 14 for physical).</summary>
    [JsonPropertyName("spacetimeDimension")]
    public required int SpacetimeDimension { get; init; }

    /// <summary>Clifford signature (p,q).</summary>
    [JsonPropertyName("cliffordSignature")]
    public required CliffordSignature CliffordSignature { get; init; }

    /// <summary>Gamma matrix convention specification.</summary>
    [JsonPropertyName("gammaConvention")]
    public required GammaConventionSpec GammaConvention { get; init; }

    /// <summary>Chirality convention (only meaningful for even dimY).</summary>
    [JsonPropertyName("chiralityConvention")]
    public required ChiralityConventionSpec ChiralityConvention { get; init; }

    /// <summary>Conjugation convention.</summary>
    [JsonPropertyName("conjugationConvention")]
    public required ConjugationConventionSpec ConjugationConvention { get; init; }

    /// <summary>
    /// Numeric field: "complex64" (default) = pairs of double-precision floats.
    /// </summary>
    [JsonPropertyName("numericField")]
    public string NumericField { get; init; } = "complex64";

    /// <summary>
    /// Number of Clifford spinor components = 2^floor(dimY/2).
    /// E.g. dimY=5 → 4, dimY=14 → 128.
    /// </summary>
    [JsonPropertyName("spinorComponents")]
    public required int SpinorComponents { get; init; }

    /// <summary>
    /// Chirality split: number of left/right Weyl components each.
    /// = spinorComponents / 2 for even dimY, 0 for odd dimY.
    /// </summary>
    [JsonPropertyName("chiralitySplit")]
    public required int ChiralitySplit { get; init; }

    /// <summary>IDs of inserted assumptions that narrow the physics to this spec.</summary>
    [JsonPropertyName("insertedAssumptionIds")]
    public List<string> InsertedAssumptionIds { get; init; } = new();

    /// <summary>Provenance of this specification.</summary>
    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}
