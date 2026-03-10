using System.Text.Json.Serialization;
using Gu.Phase3.Registry;

namespace Gu.Phase3.Reporting;

/// <summary>
/// Per-candidate stability assessment for the boson atlas report.
/// </summary>
public sealed class StabilitySummary
{
    /// <summary>Candidate ID.</summary>
    [JsonPropertyName("candidateId")]
    public required string CandidateId { get; init; }

    /// <summary>Branch stability score.</summary>
    [JsonPropertyName("branchStability")]
    public double BranchStability { get; init; }

    /// <summary>Refinement stability score.</summary>
    [JsonPropertyName("refinementStability")]
    public double RefinementStability { get; init; }

    /// <summary>Backend stability score.</summary>
    [JsonPropertyName("backendStability")]
    public double BackendStability { get; init; }

    /// <summary>Observation stability score.</summary>
    [JsonPropertyName("observationStability")]
    public double ObservationStability { get; init; }

    /// <summary>Number of demotions applied.</summary>
    [JsonPropertyName("demotionCount")]
    public int DemotionCount { get; init; }

    /// <summary>Current claim class after demotions.</summary>
    [JsonPropertyName("currentClaimClass")]
    public required BosonClaimClass CurrentClaimClass { get; init; }

    /// <summary>Overall stability assessment: "stable", "fragile", "unstable".</summary>
    [JsonPropertyName("overallAssessment")]
    public required string OverallAssessment { get; init; }
}
