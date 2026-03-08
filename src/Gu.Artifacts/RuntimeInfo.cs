using System.Text.Json.Serialization;

namespace Gu.Artifacts;

/// <summary>
/// Runtime metadata captured at the start of a run, stored in manifest/runtime.json.
/// </summary>
public sealed class RuntimeInfo
{
    /// <summary>Backend identifier (e.g., "cpu-reference", "cuda").</summary>
    [JsonPropertyName("backendId")]
    public required string BackendId { get; init; }

    /// <summary>Machine hostname.</summary>
    [JsonPropertyName("hostname")]
    public string? Hostname { get; init; }

    /// <summary>Operating system description.</summary>
    [JsonPropertyName("os")]
    public string? Os { get; init; }

    /// <summary>.NET runtime version.</summary>
    [JsonPropertyName("runtimeVersion")]
    public string? RuntimeVersion { get; init; }

    /// <summary>Timestamp when the run started.</summary>
    [JsonPropertyName("startedAt")]
    public required DateTimeOffset StartedAt { get; init; }

    /// <summary>Timestamp when the run completed.</summary>
    [JsonPropertyName("completedAt")]
    public DateTimeOffset? CompletedAt { get; init; }

    /// <summary>
    /// Capture current runtime information.
    /// </summary>
    public static RuntimeInfo CaptureCurrentEnvironment(string backendId) => new()
    {
        BackendId = backendId,
        Hostname = Environment.MachineName,
        Os = Environment.OSVersion.ToString(),
        RuntimeVersion = Environment.Version.ToString(),
        StartedAt = DateTimeOffset.UtcNow,
    };
}
