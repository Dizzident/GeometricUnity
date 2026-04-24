using System.Text.Json;
using System.Text.Json.Serialization;
using Gu.Core;

namespace Gu.Phase5.QuantitativeValidation;

/// <summary>
/// Summary of quantitative validation results against external targets (M49).
/// Aggregates all target match records for a study.
/// </summary>
public sealed class ConsistencyScoreCard
{
    /// <summary>Study identifier.</summary>
    [JsonPropertyName("studyId")]
    public required string StudyId { get; init; }

    /// <summary>Schema version.</summary>
    [JsonPropertyName("schemaVersion")]
    public required string SchemaVersion { get; init; }

    /// <summary>All target match records.</summary>
    [JsonPropertyName("matches")]
    public required IReadOnlyList<TargetMatchRecord> Matches { get; init; }

    /// <summary>Total external targets declared in the target table.</summary>
    [JsonPropertyName("totalTargets")]
    public int? TotalTargets { get; init; }

    /// <summary>Number of external targets with at least one matching computed observable.</summary>
    [JsonPropertyName("matchedTargetCount")]
    public int? MatchedTargetCount { get; init; }

    /// <summary>Number of external targets without a matching computed observable.</summary>
    [JsonPropertyName("missingTargetCount")]
    public int? MissingTargetCount { get; init; }

    /// <summary>Coverage status for every target declared in the target table.</summary>
    [JsonPropertyName("targetCoverage")]
    public IReadOnlyList<TargetCoverageRecord>? TargetCoverage { get; init; }

    /// <summary>Count of matches that passed.</summary>
    [JsonPropertyName("totalPassed")]
    public required int TotalPassed { get; init; }

    /// <summary>Count of matches that failed.</summary>
    [JsonPropertyName("totalFailed")]
    public required int TotalFailed { get; init; }

    /// <summary>Overall score: totalPassed / (totalPassed + totalFailed). NaN if no matches.</summary>
    [JsonPropertyName("overallScore")]
    public required double OverallScore { get; init; }

    /// <summary>Calibration policy ID used.</summary>
    [JsonPropertyName("calibrationPolicyId")]
    public required string CalibrationPolicyId { get; init; }

    /// <summary>Counts of matches by benchmark class.</summary>
    [JsonPropertyName("benchmarkClassCounts")]
    public IReadOnlyDictionary<string, int>? BenchmarkClassCounts { get; init; }

    /// <summary>Counts of failed matches by benchmark class.</summary>
    [JsonPropertyName("failedBenchmarkClassCounts")]
    public IReadOnlyDictionary<string, int>? FailedBenchmarkClassCounts { get; init; }

    /// <summary>Provenance metadata.</summary>
    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals,
        PropertyNameCaseInsensitive = true,
    };

    private static readonly JsonSerializerOptions CompactOptions = new()
    {
        WriteIndented = false,
        NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals,
        PropertyNameCaseInsensitive = true,
    };

    /// <summary>Serialize to JSON.</summary>
    public string ToJson(bool indented = true)
        => JsonSerializer.Serialize(this, indented ? SerializerOptions : CompactOptions);

    /// <summary>Deserialize from JSON.</summary>
    public static ConsistencyScoreCard FromJson(string json)
        => JsonSerializer.Deserialize<ConsistencyScoreCard>(json, SerializerOptions)
           ?? throw new InvalidOperationException("Failed to deserialize ConsistencyScoreCard.");
}
