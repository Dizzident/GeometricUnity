using System.Text.Json.Serialization;
using Gu.Core;

namespace Gu.Phase5.Environments;

public sealed class EnvironmentRecord
{
    [JsonPropertyName("environmentId")]
    public required string EnvironmentId { get; init; }

    [JsonPropertyName("geometryTier")]
    public required string GeometryTier { get; init; }

    [JsonPropertyName("geometryFingerprint")]
    public required string GeometryFingerprint { get; init; }

    [JsonPropertyName("baseDimension")]
    public required int BaseDimension { get; init; }

    [JsonPropertyName("ambientDimension")]
    public required int AmbientDimension { get; init; }

    [JsonPropertyName("edgeCount")]
    public required int EdgeCount { get; init; }

    [JsonPropertyName("faceCount")]
    public required int FaceCount { get; init; }

    [JsonPropertyName("admissibility")]
    public required EnvironmentAdmissibilityReport Admissibility { get; init; }

    [JsonPropertyName("sourceSpec")]
    public string? SourceSpec { get; init; }

    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}
