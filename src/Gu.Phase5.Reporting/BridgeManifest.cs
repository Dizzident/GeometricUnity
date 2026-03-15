using System.Text.Json.Serialization;
using Gu.Core;

namespace Gu.Phase5.Reporting;

/// <summary>
/// Records the bridge between a <see cref="Gu.Phase3.Backgrounds.BackgroundAtlas"/> and the
/// Phase V branch/refinement value tables exported by <see cref="BridgeValueExporter"/>.
///
/// Consumers of the exported value tables can use this manifest to trace which atlas
/// records contributed each derived variant ID and to reproduce the export deterministically.
/// </summary>
public sealed class BridgeManifest
{
    /// <summary>Unique manifest identifier.</summary>
    [JsonPropertyName("manifestId")]
    public required string ManifestId { get; init; }

    /// <summary>Absolute or relative path of the source BackgroundAtlas JSON file.</summary>
    [JsonPropertyName("sourceAtlasPath")]
    public required string SourceAtlasPath { get; init; }

    /// <summary>BackgroundRecord IDs from the source atlas that were included in the export.</summary>
    [JsonPropertyName("sourceRecordIds")]
    public required IReadOnlyList<string> SourceRecordIds { get; init; }

    /// <summary>Persisted state artifact refs from the source atlas, in the same order as SourceRecordIds.</summary>
    [JsonPropertyName("sourceStateArtifactRefs")]
    public required IReadOnlyList<string> SourceStateArtifactRefs { get; init; }

    /// <summary>
     /// Derived branch-variant IDs produced by
     /// <see cref="BackgroundRecordBranchVariantBridge.DeriveVariantId"/>, in the same order
     /// as <see cref="SourceRecordIds"/>.
     /// </summary>
    [JsonPropertyName("derivedVariantIds")]
    public required IReadOnlyList<string> DerivedVariantIds { get; init; }

    /// <summary>Provenance metadata for this export run.</summary>
    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}
