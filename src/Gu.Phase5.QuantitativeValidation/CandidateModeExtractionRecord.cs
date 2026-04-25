using System.Text.Json.Serialization;
using Gu.Core;

namespace Gu.Phase5.QuantitativeValidation;

/// <summary>
/// Source observable allowed to feed a physical candidate-mode extraction.
/// External target tables are comparison inputs and must not appear here.
/// </summary>
public sealed class CandidateModeSourceRecord
{
    [JsonPropertyName("sourceId")]
    public required string SourceId { get; init; }

    [JsonPropertyName("sourceOrigin")]
    public required string SourceOrigin { get; init; }

    [JsonPropertyName("sourceArtifactKind")]
    public required string SourceArtifactKind { get; init; }

    [JsonPropertyName("sourceArtifactPath")]
    public required string SourceArtifactPath { get; init; }

    [JsonPropertyName("sourceObservableId")]
    public required string SourceObservableId { get; init; }

    [JsonPropertyName("value")]
    public required double Value { get; init; }

    [JsonPropertyName("uncertainty")]
    public required double Uncertainty { get; init; }

    [JsonPropertyName("unitFamily")]
    public required string UnitFamily { get; init; }

    [JsonPropertyName("unit")]
    public required string Unit { get; init; }

    [JsonPropertyName("environmentId")]
    public required string EnvironmentId { get; init; }

    [JsonPropertyName("branchId")]
    public required string BranchId { get; init; }

    [JsonPropertyName("refinementLevel")]
    public string? RefinementLevel { get; init; }

    [JsonPropertyName("sourceExtractionMethod")]
    public required string SourceExtractionMethod { get; init; }

    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}

public sealed class CandidateModeExtractionRecord
{
    [JsonPropertyName("extractionId")]
    public required string ExtractionId { get; init; }

    [JsonPropertyName("algorithmId")]
    public required string AlgorithmId { get; init; }

    [JsonPropertyName("particleId")]
    public required string ParticleId { get; init; }

    [JsonPropertyName("modeKind")]
    public required string ModeKind { get; init; }

    [JsonPropertyName("sourceObservableIds")]
    public required IReadOnlyList<string> SourceObservableIds { get; init; }

    [JsonPropertyName("sourceArtifactPath")]
    public required string SourceArtifactPath { get; init; }

    [JsonPropertyName("environmentId")]
    public required string EnvironmentId { get; init; }

    [JsonPropertyName("branchId")]
    public required string BranchId { get; init; }

    [JsonPropertyName("refinementLevel")]
    public string? RefinementLevel { get; init; }

    [JsonPropertyName("status")]
    public required string Status { get; init; }

    [JsonPropertyName("candidateMode")]
    public IdentifiedPhysicalModeRecord? CandidateMode { get; init; }

    [JsonPropertyName("closureRequirements")]
    public required IReadOnlyList<string> ClosureRequirements { get; init; }

    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}
