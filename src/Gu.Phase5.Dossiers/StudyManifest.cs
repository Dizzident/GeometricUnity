using System.Text.Json.Serialization;
using Gu.Artifacts;
using Gu.Core;

namespace Gu.Phase5.Dossiers;

/// <summary>
/// Describes a study that contributed evidence to a ValidationDossier.
/// Carries the artifact evidence tier so dossiers can refuse stale artifacts.
/// </summary>
public sealed class StudyManifest
{
    /// <summary>Unique identifier for this study contribution.</summary>
    [JsonPropertyName("studyId")]
    public required string StudyId { get; init; }

    /// <summary>Human-readable description of what this study computes.</summary>
    [JsonPropertyName("description")]
    public required string Description { get; init; }

    /// <summary>
    /// The run folder where artifacts were written (relative to repo root or absolute).
    /// </summary>
    [JsonPropertyName("runFolder")]
    public required string RunFolder { get; init; }

    /// <summary>
    /// Reproducibility metadata — how this study can be regenerated.
    /// If null, the study is treated as StaleCheckedIn.
    /// </summary>
    [JsonPropertyName("reproducibility")]
    public ReproducibilityBundle? Reproducibility { get; init; }

    /// <summary>
    /// Effective evidence tier: derived from Reproducibility if present,
    /// otherwise StaleCheckedIn.
    /// </summary>
    [JsonIgnore]
    public ArtifactEvidenceTier EffectiveEvidenceTier =>
        Reproducibility?.EvidenceTier ?? ArtifactEvidenceTier.StaleCheckedIn;

    /// <summary>Provenance metadata.</summary>
    [JsonPropertyName("provenance")]
    public ProvenanceMeta? Provenance { get; init; }
}
