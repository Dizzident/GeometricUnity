using System.Text.Json.Serialization;
using Gu.Core;

namespace Gu.Phase5.Environments;

public sealed class StructuredEnvironmentSpec
{
    [JsonPropertyName("environmentId")]
    public required string EnvironmentId { get; init; }

    [JsonPropertyName("schemaVersion")]
    public required string SchemaVersion { get; init; }

    [JsonPropertyName("generatorId")]
    public required string GeneratorId { get; init; }

    [JsonPropertyName("parameters")]
    public required IReadOnlyDictionary<string, double> Parameters { get; init; }

    [JsonPropertyName("baseDimension")]
    public required int BaseDimension { get; init; }

    [JsonPropertyName("ambientDimension")]
    public required int AmbientDimension { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}
