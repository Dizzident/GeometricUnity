using System.Text.Json;
using System.Text.Json.Serialization;
using Gu.Core;
using Gu.Core.Serialization;

namespace Gu.Phase5.Falsification;

/// <summary>
/// Summary of all falsifiers evaluated for a study (M50).
/// Aggregates FalsifierRecords with count statistics.
/// </summary>
public sealed class FalsifierSummary
{
    /// <summary>Study identifier.</summary>
    [JsonPropertyName("studyId")]
    public required string StudyId { get; init; }

    /// <summary>Schema version.</summary>
    [JsonPropertyName("schemaVersion")]
    public string SchemaVersion { get; init; } = "1.0.0";

    /// <summary>All evaluated falsifier records (active and inactive).</summary>
    [JsonPropertyName("falsifiers")]
    public required IReadOnlyList<FalsifierRecord> Falsifiers { get; init; }

    /// <summary>Count of active fatal falsifiers.</summary>
    [JsonPropertyName("activeFatalCount")]
    public required int ActiveFatalCount { get; init; }

    /// <summary>Count of active high-severity falsifiers.</summary>
    [JsonPropertyName("activeHighCount")]
    public required int ActiveHighCount { get; init; }

    /// <summary>Total count of active falsifiers (all severities).</summary>
    [JsonPropertyName("totalActiveCount")]
    public required int TotalActiveCount { get; init; }

    /// <summary>Number of ObservationChainRecord inputs evaluated (P6-M3/D-P6-002).</summary>
    [JsonPropertyName("observationRecordCount")]
    public int ObservationRecordCount { get; init; }

    /// <summary>Number of EnvironmentVarianceRecord inputs evaluated (P6-M3/D-P6-002).</summary>
    [JsonPropertyName("environmentRecordCount")]
    public int EnvironmentRecordCount { get; init; }

    /// <summary>Number of RepresentationContentRecord inputs evaluated (P6-M3/D-P6-002).</summary>
    [JsonPropertyName("representationRecordCount")]
    public int RepresentationRecordCount { get; init; }

    /// <summary>Number of CouplingConsistencyRecord inputs evaluated (P6-M3/D-P6-002).</summary>
    [JsonPropertyName("couplingRecordCount")]
    public int CouplingRecordCount { get; init; }

    /// <summary>
    /// Per-channel evaluation coverage from sidecar generation (P6-M3/D-P6-002).
    /// Null when no sidecars were generated for this evaluation run.
    /// </summary>
    [JsonPropertyName("evaluationCoverage")]
    public IReadOnlyList<SidecarChannelStatus>? EvaluationCoverage { get; init; }

    /// <summary>Provenance metadata.</summary>
    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }

    /// <summary>Serialize to JSON.</summary>
    public string ToJson(bool indented = true)
        => GuJsonDefaults.Serialize(this);

    /// <summary>Deserialize from JSON.</summary>
    public static FalsifierSummary FromJson(string json)
        => GuJsonDefaults.Deserialize<FalsifierSummary>(json)
           ?? throw new InvalidOperationException("Failed to deserialize FalsifierSummary.");
}
