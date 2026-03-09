using System.Text.Json.Serialization;

namespace Gu.Phase2.Comparison;

/// <summary>
/// Result of a comparison campaign execution.
/// Contains both successful comparison records and failure records.
/// Failures are never filtered -- they are first-class artifacts.
/// </summary>
public sealed class ComparisonCampaignResult
{
    /// <summary>Campaign ID that produced this result.</summary>
    [JsonPropertyName("campaignId")]
    public required string CampaignId { get; init; }

    /// <summary>Successful comparison run records.</summary>
    [JsonPropertyName("runRecords")]
    public required IReadOnlyList<ComparisonRunRecord> RunRecords { get; init; }

    /// <summary>Failure records -- preserved as first-class artifacts.</summary>
    [JsonPropertyName("failures")]
    public required IReadOnlyList<ComparisonFailureRecord> Failures { get; init; }

    /// <summary>Calibration record used, if calibration was applied.</summary>
    [JsonPropertyName("calibration")]
    public CalibrationRecord? Calibration { get; init; }

    /// <summary>Timestamp when the campaign completed.</summary>
    [JsonPropertyName("completedAt")]
    public required DateTimeOffset CompletedAt { get; init; }
}
