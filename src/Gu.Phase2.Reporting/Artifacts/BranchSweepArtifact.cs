using System.Text.Json.Serialization;
using Gu.Phase2.Execution;

namespace Gu.Phase2.Reporting.Artifacts;

/// <summary>
/// Artifact wrapping a branch sweep result for report generation.
/// Preserves per-branch boundaries and branch provenance.
/// </summary>
public sealed class BranchSweepArtifact
{
    /// <summary>Unique artifact identifier.</summary>
    [JsonPropertyName("artifactId")]
    public required string ArtifactId { get; init; }

    /// <summary>Study ID from the BranchSweepSpec.</summary>
    [JsonPropertyName("studyId")]
    public required string StudyId { get; init; }

    /// <summary>Branch family ID.</summary>
    [JsonPropertyName("familyId")]
    public required string FamilyId { get; init; }

    /// <summary>The sweep result data.</summary>
    [JsonPropertyName("result")]
    public required Phase2BranchSweepResult Result { get; init; }

    /// <summary>Number of branches in the sweep.</summary>
    [JsonPropertyName("branchCount")]
    public required int BranchCount { get; init; }

    /// <summary>Schema version for this artifact type.</summary>
    [JsonPropertyName("schemaVersion")]
    public required string SchemaVersion { get; init; } = "2.0.0";

    /// <summary>Timestamp when this artifact was produced.</summary>
    [JsonPropertyName("producedAt")]
    public required DateTimeOffset ProducedAt { get; init; }
}
