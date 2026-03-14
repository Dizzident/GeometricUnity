using System.Text.Json.Serialization;
using Gu.Core;

namespace Gu.Phase5.Falsification;

/// <summary>
/// A single falsifier: a triggered condition that challenges the validity
/// of a candidate or study result (M50).
/// </summary>
public sealed class FalsifierRecord
{
    /// <summary>Unique falsifier identifier.</summary>
    [JsonPropertyName("falsifierId")]
    public required string FalsifierId { get; init; }

    /// <summary>Schema version.</summary>
    [JsonPropertyName("schemaVersion")]
    public string SchemaVersion { get; init; } = "1.0.0";

    /// <summary>Falsifier type (see FalsifierTypes constants).</summary>
    [JsonPropertyName("falsifierType")]
    public required string FalsifierType { get; init; }

    /// <summary>Severity (see FalsifierSeverity constants).</summary>
    [JsonPropertyName("severity")]
    public required string Severity { get; init; }

    /// <summary>Identifier of the candidate or quantity this falsifier targets.</summary>
    [JsonPropertyName("targetId")]
    public required string TargetId { get; init; }

    /// <summary>Branch ID where the condition was triggered.</summary>
    [JsonPropertyName("branchId")]
    public required string BranchId { get; init; }

    /// <summary>Environment ID where the condition was triggered (null if environment-agnostic).</summary>
    [JsonPropertyName("environmentId")]
    public string? EnvironmentId { get; init; }

    /// <summary>Trigger value that caused this falsifier to fire (e.g., fragility score).</summary>
    [JsonPropertyName("triggerValue")]
    public double? TriggerValue { get; init; }

    /// <summary>Threshold value that was exceeded.</summary>
    [JsonPropertyName("threshold")]
    public double? Threshold { get; init; }

    /// <summary>Human-readable description of what was triggered.</summary>
    [JsonPropertyName("description")]
    public required string Description { get; init; }

    /// <summary>Evidence source (e.g., record type and ID).</summary>
    [JsonPropertyName("evidence")]
    public required string Evidence { get; init; }

    /// <summary>
    /// Whether this falsifier is currently active.
    /// Inactive falsifiers are preserved for record-keeping but not applied to demotions.
    /// </summary>
    [JsonPropertyName("active")]
    public required bool Active { get; init; }

    /// <summary>Provenance metadata.</summary>
    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}
