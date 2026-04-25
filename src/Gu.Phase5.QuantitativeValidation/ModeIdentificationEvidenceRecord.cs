using System.Text.Json.Serialization;
using Gu.Core;

namespace Gu.Phase5.QuantitativeValidation;

/// <summary>
/// Evidence connecting a computed mode to a physical particle interpretation.
/// Validated physical-mode extraction requires a matching validated record.
/// </summary>
public sealed class ModeIdentificationEvidenceRecord
{
    [JsonPropertyName("evidenceId")]
    public required string EvidenceId { get; init; }

    [JsonPropertyName("modeId")]
    public required string ModeId { get; init; }

    [JsonPropertyName("particleId")]
    public required string ParticleId { get; init; }

    [JsonPropertyName("modeKind")]
    public required string ModeKind { get; init; }

    [JsonPropertyName("sourceObservableIds")]
    public required IReadOnlyList<string> SourceObservableIds { get; init; }

    [JsonPropertyName("environmentId")]
    public required string EnvironmentId { get; init; }

    [JsonPropertyName("branchId")]
    public required string BranchId { get; init; }

    [JsonPropertyName("refinementLevel")]
    public string? RefinementLevel { get; init; }

    [JsonPropertyName("derivationId")]
    public required string DerivationId { get; init; }

    [JsonPropertyName("status")]
    public required string Status { get; init; }

    [JsonPropertyName("assumptions")]
    public required IReadOnlyList<string> Assumptions { get; init; }

    [JsonPropertyName("closureRequirements")]
    public required IReadOnlyList<string> ClosureRequirements { get; init; }

    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}

public sealed class ModeIdentificationEvidenceTable
{
    [JsonPropertyName("tableId")]
    public required string TableId { get; init; }

    [JsonPropertyName("evidence")]
    public required IReadOnlyList<ModeIdentificationEvidenceRecord> Evidence { get; init; }
}
