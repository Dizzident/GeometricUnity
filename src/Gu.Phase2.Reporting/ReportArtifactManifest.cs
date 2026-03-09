using System.Text.Json.Serialization;
using Gu.Phase2.Reporting.Artifacts;

namespace Gu.Phase2.Reporting;

/// <summary>
/// Manifest of all artifacts produced during a research batch.
/// Provides a single point of access for all artifact types,
/// ensuring nothing is lost or orphaned.
/// </summary>
public sealed class ReportArtifactManifest
{
    /// <summary>Branch sweep artifacts.</summary>
    [JsonPropertyName("sweepArtifacts")]
    public required IReadOnlyList<BranchSweepArtifact> SweepArtifacts { get; init; }

    /// <summary>Canonicity docket artifacts.</summary>
    [JsonPropertyName("canonicityArtifacts")]
    public required IReadOnlyList<CanonicityDocketArtifact> CanonicityArtifacts { get; init; }

    /// <summary>Stability atlas artifacts.</summary>
    [JsonPropertyName("stabilityArtifacts")]
    public required IReadOnlyList<StabilityAtlasArtifact> StabilityArtifacts { get; init; }

    /// <summary>Continuation artifacts.</summary>
    [JsonPropertyName("continuationArtifacts")]
    public required IReadOnlyList<ContinuationArtifact> ContinuationArtifacts { get; init; }

    /// <summary>Recovery graph artifacts.</summary>
    [JsonPropertyName("recoveryArtifacts")]
    public required IReadOnlyList<RecoveryGraphArtifact> RecoveryArtifacts { get; init; }

    /// <summary>Prediction matrix artifacts.</summary>
    [JsonPropertyName("predictionArtifacts")]
    public required IReadOnlyList<PredictionMatrixArtifact> PredictionArtifacts { get; init; }

    /// <summary>Comparison campaign artifacts.</summary>
    [JsonPropertyName("comparisonArtifacts")]
    public required IReadOnlyList<ComparisonCampaignArtifact> ComparisonArtifacts { get; init; }

    /// <summary>Total number of artifacts across all categories.</summary>
    [JsonIgnore]
    public int TotalCount =>
        SweepArtifacts.Count +
        CanonicityArtifacts.Count +
        StabilityArtifacts.Count +
        ContinuationArtifacts.Count +
        RecoveryArtifacts.Count +
        PredictionArtifacts.Count +
        ComparisonArtifacts.Count;
}
