using System.Text.Json.Serialization;

namespace Gu.Artifacts;

/// <summary>
/// Records all information needed to reproduce a study artifact from the
/// current code tree.
/// G-006: every Phase V artifact cited as validation evidence must carry a
/// ReproducibilityBundle. Artifacts without one are treated as StaleCheckedIn.
/// </summary>
public sealed class ReproducibilityBundle
{
    /// <summary>
    /// Git commit hash (or "unknown") of the code that produced this artifact.
    /// </summary>
    [JsonPropertyName("codeRevision")]
    public required string CodeRevision { get; init; }

    /// <summary>
    /// Ordered list of CLI commands that, when run sequentially from the
    /// repository root, reproduce this artifact.
    /// Example: ["dotnet run --project apps/Gu.Cli -- solve-backgrounds study.json"]
    /// </summary>
    [JsonPropertyName("reproductionCommands")]
    public required IReadOnlyList<string> ReproductionCommands { get; init; }

    /// <summary>
    /// UTC timestamp when this artifact was last regenerated.
    /// </summary>
    [JsonPropertyName("regeneratedAt")]
    public required DateTimeOffset RegeneratedAt { get; init; }

    /// <summary>
    /// Evidence tier of this artifact.
    /// </summary>
    [JsonPropertyName("evidenceTier")]
    public required ArtifactEvidenceTier EvidenceTier { get; init; }

    /// <summary>
    /// Optional: path to a script file (relative to repo root) that can
    /// reproduce this artifact (e.g., studies/my_study/run_study.sh).
    /// </summary>
    [JsonPropertyName("reproduceScript")]
    public string? ReproduceScript { get; init; }

    /// <summary>
    /// Optional: human-readable notes about what this artifact provides as
    /// validation evidence.
    /// </summary>
    [JsonPropertyName("evidenceNotes")]
    public string? EvidenceNotes { get; init; }

    /// <summary>
    /// Creates a StaleCheckedIn bundle — used when an artifact has no
    /// recorded regeneration provenance.
    /// </summary>
    public static ReproducibilityBundle CreateStale(string? notes = null) =>
        new ReproducibilityBundle
        {
            CodeRevision = "unknown",
            ReproductionCommands = Array.Empty<string>(),
            RegeneratedAt = DateTimeOffset.MinValue,
            EvidenceTier = ArtifactEvidenceTier.StaleCheckedIn,
            EvidenceNotes = notes ?? "No regeneration provenance recorded. Artifact may be stale.",
        };

    /// <summary>
    /// Creates a RegeneratedCpu bundle with the given code revision and commands.
    /// </summary>
    public static ReproducibilityBundle CreateRegeneratedCpu(
        string codeRevision,
        IReadOnlyList<string> reproductionCommands,
        string? reproduceScript = null,
        string? notes = null) =>
        new ReproducibilityBundle
        {
            CodeRevision = codeRevision,
            ReproductionCommands = reproductionCommands,
            RegeneratedAt = DateTimeOffset.UtcNow,
            EvidenceTier = ArtifactEvidenceTier.RegeneratedCpu,
            ReproduceScript = reproduceScript,
            EvidenceNotes = notes,
        };
}
