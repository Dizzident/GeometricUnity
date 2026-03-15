using System.Text.Json.Serialization;
using Gu.Artifacts;
using Gu.Core;

namespace Gu.Phase3.Backgrounds;

/// <summary>
/// Complete record of a solved background state (Section 6.1).
/// Includes provenance, metrics, admissibility level, and artifact references.
/// </summary>
public sealed class BackgroundRecord
{
    /// <summary>Unique background identifier.</summary>
    [JsonPropertyName("backgroundId")]
    public required string BackgroundId { get; init; }

    /// <summary>Environment identifier.</summary>
    [JsonPropertyName("environmentId")]
    public required string EnvironmentId { get; init; }

    /// <summary>Branch manifest identifier.</summary>
    [JsonPropertyName("branchManifestId")]
    public required string BranchManifestId { get; init; }

    /// <summary>
    /// Continuation coordinates if this background was produced via continuation.
    /// </summary>
    [JsonPropertyName("continuationCoordinates")]
    public IReadOnlyDictionary<string, double>? ContinuationCoordinates { get; init; }

    /// <summary>
    /// Geometry fingerprint for deduplication and tracking.
    /// </summary>
    [JsonPropertyName("geometryFingerprint")]
    public required string GeometryFingerprint { get; init; }

    /// <summary>Gauge choice identifier.</summary>
    [JsonPropertyName("gaugeChoice")]
    public string? GaugeChoice { get; init; }

    /// <summary>
    /// Reference to the persisted state artifact (FieldTensor).
    /// </summary>
    [JsonPropertyName("stateArtifactRef")]
    public required string StateArtifactRef { get; init; }

    /// <summary>Residual norm ||Upsilon_h(z_*)||.</summary>
    [JsonPropertyName("residualNorm")]
    public required double ResidualNorm { get; init; }

    /// <summary>Stationarity norm ||G_h(z_*)||.</summary>
    [JsonPropertyName("stationarityNorm")]
    public required double StationarityNorm { get; init; }

    /// <summary>Admissibility level (B0/B1/B2/Rejected).</summary>
    [JsonPropertyName("admissibilityLevel")]
    public required AdmissibilityLevel AdmissibilityLevel { get; init; }

    /// <summary>Background metrics (full solve diagnostics).</summary>
    [JsonPropertyName("metrics")]
    public required BackgroundMetrics Metrics { get; init; }

    /// <summary>Reference to the solve trace artifact.</summary>
    [JsonPropertyName("solveTraceRef")]
    public string? SolveTraceRef { get; init; }

    /// <summary>Replay tier achieved for this background.</summary>
    [JsonPropertyName("replayTierAchieved")]
    public required string ReplayTierAchieved { get; init; }

    /// <summary>Provenance metadata.</summary>
    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }

    /// <summary>
    /// Rejection reason if AdmissibilityLevel is Rejected.
    /// Preserved for negative-result tracking.
    /// </summary>
    [JsonPropertyName("rejectionReason")]
    public string? RejectionReason { get; init; }

    /// <summary>PDE classification from Phase II classifier (forward-compatibility).</summary>
    [JsonPropertyName("pdeClassification")]
    public string? PdeClassification { get; init; }

    /// <summary>Human-readable notes.</summary>
    [JsonPropertyName("notes")]
    public string? Notes { get; init; }

    /// <summary>G-003: geometry tier of the environment used (toy/structured/imported).</summary>
    [JsonPropertyName("environmentTier")]
    public string? EnvironmentTier { get; init; }

    /// <summary>
    /// D-002: Classification of the solve run that produced this background record.
    /// Embedded for single-source-of-truth persistence (not a separate classifications.json).
    /// </summary>
    [JsonPropertyName("runClassification")]
    public SolveRunClassification? RunClassification { get; init; }

    /// <summary>
    /// D-002: Reference to the manifest artifact consumed during this background solve.
    /// Set to the path of the persisted per-background manifest JSON file.
    /// </summary>
    [JsonPropertyName("consumedManifestArtifactRef")]
    public string? ConsumedManifestArtifactRef { get; init; }
}
