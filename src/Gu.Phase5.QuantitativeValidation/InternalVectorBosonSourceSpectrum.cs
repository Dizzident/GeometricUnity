using System.Text.Json.Serialization;
using Gu.Core;

namespace Gu.Phase5.QuantitativeValidation;

public sealed class InternalVectorBosonSourceSpectrumCampaignSpec
{
    [JsonPropertyName("campaignId")]
    public required string CampaignId { get; init; }

    [JsonPropertyName("schemaVersion")]
    public required string SchemaVersion { get; init; }

    [JsonPropertyName("sourceCandidateTablePath")]
    public required string SourceCandidateTablePath { get; init; }

    [JsonPropertyName("readinessSpecPath")]
    public required string ReadinessSpecPath { get; init; }

    [JsonPropertyName("selectorCellBundleManifestPath")]
    public string? SelectorCellBundleManifestPath { get; init; }

    [JsonPropertyName("branchVariantIds")]
    public required IReadOnlyList<string> BranchVariantIds { get; init; }

    [JsonPropertyName("refinementLevels")]
    public required IReadOnlyList<string> RefinementLevels { get; init; }

    [JsonPropertyName("environmentIds")]
    public required IReadOnlyList<string> EnvironmentIds { get; init; }

    [JsonPropertyName("sourceQuantityIds")]
    public required IReadOnlyList<string> SourceQuantityIds { get; init; }

    [JsonPropertyName("missingCellPolicy")]
    public required string MissingCellPolicy { get; init; }

    [JsonPropertyName("identityScope")]
    public required string IdentityScope { get; init; }

    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}

public sealed class InternalVectorBosonSourceSpectrumManifest
{
    [JsonPropertyName("manifestId")]
    public required string ManifestId { get; init; }

    [JsonPropertyName("schemaVersion")]
    public required string SchemaVersion { get; init; }

    [JsonPropertyName("status")]
    public required string Status { get; init; }

    [JsonPropertyName("matrixCellCount")]
    public required int MatrixCellCount { get; init; }

    [JsonPropertyName("entries")]
    public required IReadOnlyList<InternalVectorBosonSourceSpectrumEntry> Entries { get; init; }

    [JsonPropertyName("summaryBlockers")]
    public required IReadOnlyList<string> SummaryBlockers { get; init; }

    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}

public sealed class InternalVectorBosonSourceSpectrumEntry
{
    [JsonPropertyName("entryId")]
    public required string EntryId { get; init; }

    [JsonPropertyName("sourceCandidateId")]
    public required string SourceCandidateId { get; init; }

    [JsonPropertyName("branchVariantId")]
    public required string BranchVariantId { get; init; }

    [JsonPropertyName("refinementLevel")]
    public required string RefinementLevel { get; init; }

    [JsonPropertyName("environmentId")]
    public required string EnvironmentId { get; init; }

    [JsonPropertyName("spectrumPath")]
    public required string SpectrumPath { get; init; }

    [JsonPropertyName("modePath")]
    public required string ModePath { get; init; }

    [JsonPropertyName("selectorCellBundleId")]
    public string? SelectorCellBundleId { get; init; }

    [JsonPropertyName("selectorCellBundlePath")]
    public string? SelectorCellBundlePath { get; init; }

    [JsonPropertyName("status")]
    public required string Status { get; init; }

    [JsonPropertyName("blockers")]
    public required IReadOnlyList<string> Blockers { get; init; }
}

public sealed class InternalVectorBosonSourceModeRecord
{
    [JsonPropertyName("modeRecordId")]
    public required string ModeRecordId { get; init; }

    [JsonPropertyName("sourceCandidateId")]
    public required string SourceCandidateId { get; init; }

    [JsonPropertyName("sourceFamilyId")]
    public string? SourceFamilyId { get; init; }

    [JsonPropertyName("branchVariantId")]
    public required string BranchVariantId { get; init; }

    [JsonPropertyName("refinementLevel")]
    public required string RefinementLevel { get; init; }

    [JsonPropertyName("environmentId")]
    public required string EnvironmentId { get; init; }

    [JsonPropertyName("massLikeValue")]
    public required double MassLikeValue { get; init; }

    [JsonPropertyName("extractionError")]
    public required double ExtractionError { get; init; }

    [JsonPropertyName("gaugeLeakEnvelope")]
    public required IReadOnlyList<double> GaugeLeakEnvelope { get; init; }

    [JsonPropertyName("sourceArtifactPaths")]
    public required IReadOnlyList<string> SourceArtifactPaths { get; init; }

    [JsonPropertyName("selectorCellBundleId")]
    public string? SelectorCellBundleId { get; init; }

    [JsonPropertyName("operatorBundleId")]
    public string? OperatorBundleId { get; init; }

    [JsonPropertyName("solverMethod")]
    public string? SolverMethod { get; init; }

    [JsonPropertyName("operatorType")]
    public string? OperatorType { get; init; }

    [JsonPropertyName("status")]
    public required string Status { get; init; }

    [JsonPropertyName("blockers")]
    public required IReadOnlyList<string> Blockers { get; init; }

    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}

public sealed class InternalVectorBosonSourceModeFamilyTable
{
    [JsonPropertyName("tableId")]
    public required string TableId { get; init; }

    [JsonPropertyName("schemaVersion")]
    public required string SchemaVersion { get; init; }

    [JsonPropertyName("families")]
    public required IReadOnlyList<InternalVectorBosonSourceModeFamilyRecord> Families { get; init; }

    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}

public sealed class InternalVectorBosonSourceModeFamilyRecord
{
    [JsonPropertyName("familyId")]
    public required string FamilyId { get; init; }

    [JsonPropertyName("sourceCandidateId")]
    public required string SourceCandidateId { get; init; }

    [JsonPropertyName("modeRecordIds")]
    public required IReadOnlyList<string> ModeRecordIds { get; init; }

    [JsonPropertyName("branchVariantIds")]
    public required IReadOnlyList<string> BranchVariantIds { get; init; }

    [JsonPropertyName("refinementLevels")]
    public required IReadOnlyList<string> RefinementLevels { get; init; }

    [JsonPropertyName("environmentIds")]
    public required IReadOnlyList<string> EnvironmentIds { get; init; }

    [JsonPropertyName("massLikeValue")]
    public required double MassLikeValue { get; init; }

    [JsonPropertyName("ambiguityCount")]
    public required int AmbiguityCount { get; init; }

    [JsonPropertyName("branchStabilityScore")]
    public required double BranchStabilityScore { get; init; }

    [JsonPropertyName("refinementStabilityScore")]
    public required double RefinementStabilityScore { get; init; }

    [JsonPropertyName("environmentStabilityScore")]
    public required double EnvironmentStabilityScore { get; init; }

    [JsonPropertyName("uncertainty")]
    public required QuantitativeUncertainty Uncertainty { get; init; }

    [JsonPropertyName("claimClass")]
    public required string ClaimClass { get; init; }

    [JsonPropertyName("closureRequirements")]
    public required IReadOnlyList<string> ClosureRequirements { get; init; }
}
