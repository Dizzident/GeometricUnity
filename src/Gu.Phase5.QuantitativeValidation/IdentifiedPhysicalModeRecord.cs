using System.Text.Json.Serialization;
using Gu.Core;

namespace Gu.Phase5.QuantitativeValidation;

/// <summary>
/// Candidate or validated physical mode input for physical-observable extraction.
/// A mode can feed physical prediction code only when Status is "validated".
/// </summary>
public sealed class IdentifiedPhysicalModeRecord
{
    [JsonPropertyName("modeId")]
    public required string ModeId { get; init; }

    [JsonPropertyName("particleId")]
    public required string ParticleId { get; init; }

    [JsonPropertyName("modeKind")]
    public required string ModeKind { get; init; }

    [JsonPropertyName("observableId")]
    public required string ObservableId { get; init; }

    [JsonPropertyName("value")]
    public required double Value { get; init; }

    [JsonPropertyName("uncertainty")]
    public required double Uncertainty { get; init; }

    [JsonPropertyName("unitFamily")]
    public required string UnitFamily { get; init; }

    [JsonPropertyName("unit")]
    public required string Unit { get; init; }

    [JsonPropertyName("status")]
    public required string Status { get; init; }

    [JsonPropertyName("environmentId")]
    public required string EnvironmentId { get; init; }

    [JsonPropertyName("branchId")]
    public required string BranchId { get; init; }

    [JsonPropertyName("refinementLevel")]
    public string? RefinementLevel { get; init; }

    [JsonPropertyName("extractionMethod")]
    public required string ExtractionMethod { get; init; }

    [JsonPropertyName("assumptions")]
    public required IReadOnlyList<string> Assumptions { get; init; }

    [JsonPropertyName("closureRequirements")]
    public required IReadOnlyList<string> ClosureRequirements { get; init; }

    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}
