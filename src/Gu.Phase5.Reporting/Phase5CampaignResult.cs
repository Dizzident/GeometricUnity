using System.Text.Json.Serialization;
using Gu.Phase5.Dossiers;

namespace Gu.Phase5.Reporting;

/// <summary>
/// Result of a Phase5CampaignRunner.Run call.
/// Carries both dossier types and the final report (WP-5 / D-006).
///
/// Two dossier types:
///   - <see cref="TypedDossier"/>: Phase5ValidationDossier — technical Phase V evidence bundle
///     (branch/refinement/environment/falsification/escalation content).
///   - <see cref="ProvenanceDossier"/>: ValidationDossier — freshness/evidence-tier gate
///     enforcing G-006.
///   - <see cref="StudyManifests"/>: the two StudyManifest entries (positive + negative).
/// </summary>
public sealed class Phase5CampaignResult
{
    /// <summary>Typed technical dossier (Phase5ValidationDossier).</summary>
    [JsonPropertyName("typedDossier")]
    public required Phase5ValidationDossier TypedDossier { get; init; }

    /// <summary>Provenance/freshness dossier (ValidationDossier).</summary>
    [JsonPropertyName("provenanceDossier")]
    public required ValidationDossier ProvenanceDossier { get; init; }

    /// <summary>
    /// Study manifests — exactly two entries: positive/mixed and negative-result.
    /// </summary>
    [JsonPropertyName("studyManifests")]
    public required IReadOnlyList<StudyManifest> StudyManifests { get; init; }

    /// <summary>Final Phase V validation report.</summary>
    [JsonPropertyName("report")]
    public required Phase5Report Report { get; init; }

    /// <summary>
    /// P11-M5 representation-content stabilization record. Non-null when a fatal
    /// representation-content falsifier was active and examined during Phase XI.
    /// Status "preserved-as-blocker" confirms the fatal is an unresolved scientific
    /// limitation per D-P11-004.
    /// </summary>
    [JsonPropertyName("representationContentStabilization")]
    public RepresentationContentStabilizationRecord? RepresentationContentStabilization { get; init; }
}
