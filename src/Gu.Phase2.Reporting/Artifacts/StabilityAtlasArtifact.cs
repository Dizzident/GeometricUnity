using System.Text.Json.Serialization;
using Gu.Phase2.Stability;

namespace Gu.Phase2.Reporting.Artifacts;

/// <summary>
/// Artifact wrapping stability analysis results (Hessian spectrum, mode classification)
/// for reporting. Drawn from the linearization workbench output.
/// </summary>
public sealed class StabilityAtlasArtifact
{
    /// <summary>Unique artifact identifier.</summary>
    [JsonPropertyName("artifactId")]
    public required string ArtifactId { get; init; }

    /// <summary>Study ID from the StabilityStudySpec.</summary>
    [JsonPropertyName("studyId")]
    public required string StudyId { get; init; }

    /// <summary>Background state ID.</summary>
    [JsonPropertyName("backgroundStateId")]
    public required string BackgroundStateId { get; init; }

    /// <summary>Spectrum records from the Hessian probe.</summary>
    [JsonPropertyName("spectrumRecords")]
    public required IReadOnlyList<SpectrumRecord> SpectrumRecords { get; init; }

    /// <summary>Hessian records with mode classification.</summary>
    [JsonPropertyName("hessianRecords")]
    public required IReadOnlyList<HessianRecord> HessianRecords { get; init; }

    /// <summary>Overall stability interpretation.</summary>
    [JsonPropertyName("stabilityInterpretation")]
    public required string StabilityInterpretation { get; init; }

    /// <summary>Schema version for this artifact type.</summary>
    [JsonPropertyName("schemaVersion")]
    public required string SchemaVersion { get; init; } = "2.0.0";

    /// <summary>Timestamp when this artifact was produced.</summary>
    [JsonPropertyName("producedAt")]
    public required DateTimeOffset ProducedAt { get; init; }
}
