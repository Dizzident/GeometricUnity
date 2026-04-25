using System.Text.Json.Serialization;
using Gu.Core;

namespace Gu.Phase5.QuantitativeValidation;

public sealed class InternalVectorBosonSourceCandidate
{
    [JsonPropertyName("sourceCandidateId")]
    public required string SourceCandidateId { get; init; }

    [JsonPropertyName("sourceOrigin")]
    public required string SourceOrigin { get; init; }

    [JsonPropertyName("modeRole")]
    public required string ModeRole { get; init; }

    [JsonPropertyName("sourceArtifactPaths")]
    public required IReadOnlyList<string> SourceArtifactPaths { get; init; }

    [JsonPropertyName("sourceModeIds")]
    public required IReadOnlyList<string> SourceModeIds { get; init; }

    [JsonPropertyName("sourceFamilyId")]
    public string? SourceFamilyId { get; init; }

    [JsonPropertyName("massLikeValue")]
    public required double MassLikeValue { get; init; }

    [JsonPropertyName("uncertainty")]
    public required QuantitativeUncertainty Uncertainty { get; init; }

    [JsonPropertyName("branchSelectors")]
    public required IReadOnlyList<string> BranchSelectors { get; init; }

    [JsonPropertyName("environmentSelectors")]
    public required IReadOnlyList<string> EnvironmentSelectors { get; init; }

    [JsonPropertyName("refinementLevels")]
    public required IReadOnlyList<string> RefinementLevels { get; init; }

    [JsonPropertyName("branchStabilityScore")]
    public double? BranchStabilityScore { get; init; }

    [JsonPropertyName("refinementStabilityScore")]
    public double? RefinementStabilityScore { get; init; }

    [JsonPropertyName("backendStabilityScore")]
    public double? BackendStabilityScore { get; init; }

    [JsonPropertyName("observationStabilityScore")]
    public double? ObservationStabilityScore { get; init; }

    [JsonPropertyName("ambiguityCount")]
    public int? AmbiguityCount { get; init; }

    [JsonPropertyName("gaugeLeakEnvelope")]
    public IReadOnlyList<double>? GaugeLeakEnvelope { get; init; }

    [JsonPropertyName("claimClass")]
    public string? ClaimClass { get; init; }

    [JsonPropertyName("status")]
    public required string Status { get; init; }

    [JsonPropertyName("assumptions")]
    public required IReadOnlyList<string> Assumptions { get; init; }

    [JsonPropertyName("closureRequirements")]
    public required IReadOnlyList<string> ClosureRequirements { get; init; }

    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}

public sealed class InternalVectorBosonSourceCandidateTable
{
    [JsonPropertyName("tableId")]
    public required string TableId { get; init; }

    [JsonPropertyName("schemaVersion")]
    public required string SchemaVersion { get; init; }

    [JsonPropertyName("terminalStatus")]
    public required string TerminalStatus { get; init; }

    [JsonPropertyName("candidates")]
    public required IReadOnlyList<InternalVectorBosonSourceCandidate> Candidates { get; init; }

    [JsonPropertyName("summaryBlockers")]
    public required IReadOnlyList<string> SummaryBlockers { get; init; }

    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}
