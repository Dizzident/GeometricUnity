using System.Text.Json.Serialization;
using Gu.Phase2.Continuation;

namespace Gu.Phase2.Reporting.Artifacts;

/// <summary>
/// Artifact wrapping a continuation/bifurcation study result for reporting.
/// </summary>
public sealed class ContinuationArtifact
{
    /// <summary>Unique artifact identifier.</summary>
    [JsonPropertyName("artifactId")]
    public required string ArtifactId { get; init; }

    /// <summary>Study ID.</summary>
    [JsonPropertyName("studyId")]
    public required string StudyId { get; init; }

    /// <summary>The continuation result data.</summary>
    [JsonPropertyName("result")]
    public required ContinuationResult Result { get; init; }

    /// <summary>Number of continuation steps completed.</summary>
    [JsonPropertyName("stepCount")]
    public required int StepCount { get; init; }

    /// <summary>Whether any bifurcation events were detected.</summary>
    [JsonPropertyName("bifurcationDetected")]
    public required bool BifurcationDetected { get; init; }

    /// <summary>Schema version for this artifact type.</summary>
    [JsonPropertyName("schemaVersion")]
    public required string SchemaVersion { get; init; } = "2.0.0";

    /// <summary>Timestamp when this artifact was produced.</summary>
    [JsonPropertyName("producedAt")]
    public required DateTimeOffset ProducedAt { get; init; }
}
