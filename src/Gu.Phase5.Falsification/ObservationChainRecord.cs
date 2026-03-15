using System.Text.Json.Serialization;
using Gu.Core;

namespace Gu.Phase5.Falsification;

/// <summary>
/// Summarizes the observation pipeline chain for a single candidate/observable pair (WP-6/D-007).
///
/// Moved to Gu.Phase5.Falsification so FalsifierEvaluator can accept it as input (WP-7).
///
/// Join rule (D-007):
///   - CandidateId must equal UnifiedParticleRecord.ParticleId
///   - PrimarySourceId must equal UnifiedParticleRecord.PrimarySourceId
///
/// ObservationChainValid gate passes for a candidate if at least one record satisfies all of:
///   1. CompletenessStatus == "complete"
///   2. Passed == true
///   3. SensitivityScore &lt;= 0.3
///   4. AuxiliaryModelSensitivity &lt;= 0.3
/// </summary>
public sealed class ObservationChainRecord
{
    /// <summary>Joins to UnifiedParticleRecord.ParticleId.</summary>
    [JsonPropertyName("candidateId")]
    public required string CandidateId { get; init; }

    /// <summary>Joins to UnifiedParticleRecord.PrimarySourceId.</summary>
    [JsonPropertyName("primarySourceId")]
    public required string PrimarySourceId { get; init; }

    /// <summary>Observable identifier.</summary>
    [JsonPropertyName("observableId")]
    public required string ObservableId { get; init; }

    /// <summary>Artifact reference for the native (pre-observation) artifact.</summary>
    [JsonPropertyName("nativeArtifactRef")]
    public string? NativeArtifactRef { get; init; }

    /// <summary>Artifact reference for the observed artifact.</summary>
    [JsonPropertyName("observedArtifactRef")]
    public string? ObservedArtifactRef { get; init; }

    /// <summary>Artifact reference for the extracted observable.</summary>
    [JsonPropertyName("extractionArtifactRef")]
    public string? ExtractionArtifactRef { get; init; }

    /// <summary>ID of the auxiliary model used for extraction.</summary>
    [JsonPropertyName("auxiliaryModelId")]
    public string? AuxiliaryModelId { get; init; }

    /// <summary>Completeness status: "complete", "partial", or "missing".</summary>
    [JsonPropertyName("completenessStatus")]
    public required string CompletenessStatus { get; init; }

    /// <summary>Sensitivity score of the observable to extractor perturbations (0–1). Lower is better.</summary>
    [JsonPropertyName("sensitivityScore")]
    public required double SensitivityScore { get; init; }

    /// <summary>Sensitivity score of the auxiliary model (0–1). Lower is better.</summary>
    [JsonPropertyName("auxiliaryModelSensitivity")]
    public required double AuxiliaryModelSensitivity { get; init; }

    /// <summary>True if the observation chain passed all checks.</summary>
    [JsonPropertyName("passed")]
    public required bool Passed { get; init; }

    /// <summary>Optional notes about the observation chain result.</summary>
    [JsonPropertyName("notes")]
    public string? Notes { get; init; }

    /// <summary>Provenance metadata.</summary>
    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}
