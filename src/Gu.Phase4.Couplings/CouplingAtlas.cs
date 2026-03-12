using System.Text.Json.Serialization;
using Gu.Core;

namespace Gu.Phase4.Couplings;

/// <summary>
/// A complete table of boson-fermion coupling proxies for one fermion background.
/// </summary>
public sealed class CouplingAtlas
{
    [JsonPropertyName("atlasId")]
    public required string AtlasId { get; init; }

    [JsonPropertyName("fermionBackgroundId")]
    public required string FermionBackgroundId { get; init; }

    [JsonPropertyName("bosonRegistryVersion")]
    public required string BosonRegistryVersion { get; init; }

    [JsonPropertyName("couplings")]
    public required List<BosonFermionCouplingRecord> Couplings { get; init; }

    [JsonPropertyName("normalizationConvention")]
    public required string NormalizationConvention { get; init; }

    [JsonPropertyName("diagnosticNotes")]
    public List<string>? DiagnosticNotes { get; init; }

    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}
