using System.Text.Json.Serialization;
using Gu.Core;

namespace Gu.Phase5.Environments;

public sealed class EnvironmentImportSpec
{
    [JsonPropertyName("environmentId")]
    public required string EnvironmentId { get; init; }

    [JsonPropertyName("schemaVersion")]
    public required string SchemaVersion { get; init; }

    [JsonPropertyName("sourcePath")]
    public required string SourcePath { get; init; }

    [JsonPropertyName("sourceFormat")]
    public required string SourceFormat { get; init; }

    [JsonPropertyName("geometryTier")]
    public required string GeometryTier { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("datasetId")]
    public string? DatasetId { get; init; }

    [JsonPropertyName("sourceHash")]
    public string? SourceHash { get; init; }

    [JsonPropertyName("conversionVersion")]
    public string? ConversionVersion { get; init; }

    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}
