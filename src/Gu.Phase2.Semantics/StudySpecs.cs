using System.Text.Json.Serialization;

namespace Gu.Phase2.Semantics;

/// <summary>
/// Specification for a branch sweep study (Section 8.5).
/// Defines which variants to run across which environment.
/// </summary>
public sealed class BranchSweepSpec
{
    /// <summary>Unique study identifier.</summary>
    [JsonPropertyName("studyId")]
    public required string StudyId { get; init; }

    /// <summary>Environment to run all variants against.</summary>
    [JsonPropertyName("environmentId")]
    public required string EnvironmentId { get; init; }

    /// <summary>Branch family identifier.</summary>
    [JsonPropertyName("familyId")]
    public required string FamilyId { get; init; }

    /// <summary>Variant IDs to include in the sweep.</summary>
    [JsonPropertyName("variantIds")]
    public required IReadOnlyList<string> VariantIds { get; init; }

    /// <summary>Solver mode: "ResidualOnly", "ObjectiveMinimization", "StationaritySolve".</summary>
    [JsonPropertyName("innerSolveMode")]
    public required string InnerSolveMode { get; init; }

    /// <summary>Equivalence specification for comparing variants.</summary>
    [JsonPropertyName("equivalence")]
    public required EquivalenceSpec Equivalence { get; init; }
}

/// <summary>
/// Specification for a stability study (Section 8.5).
/// </summary>
public sealed class StabilityStudySpec
{
    /// <summary>Unique study identifier.</summary>
    [JsonPropertyName("studyId")]
    public required string StudyId { get; init; }

    /// <summary>Background state to linearize around.</summary>
    [JsonPropertyName("backgroundStateId")]
    public required string BackgroundStateId { get; init; }

    /// <summary>Branch manifest ID for the linearization.</summary>
    [JsonPropertyName("branchManifestId")]
    public required string BranchManifestId { get; init; }

    /// <summary>Gauge handling mode: "gauge-free", "coulomb-slice", etc.</summary>
    [JsonPropertyName("gaugeHandlingMode")]
    public required string GaugeHandlingMode { get; init; }

    /// <summary>Number of eigenvalues/modes to compute.</summary>
    [JsonPropertyName("requestedModeCount")]
    public required int RequestedModeCount { get; init; }

    /// <summary>Spectrum probe method: "lanczos", "lobpcg", "arnoldi".</summary>
    [JsonPropertyName("spectrumProbeMethod")]
    public required string SpectrumProbeMethod { get; init; }

    /// <summary>Eigenvalues below this are "soft" modes.</summary>
    [JsonPropertyName("softModeThreshold")]
    public required double SoftModeThreshold { get; init; }

    /// <summary>Eigenvalues below this are "near-kernel" modes.</summary>
    [JsonPropertyName("nearKernelThreshold")]
    public required double NearKernelThreshold { get; init; }

    /// <summary>Eigenvalues below this are "negative" modes.</summary>
    [JsonPropertyName("negativeModeThreshold")]
    public required double NegativeModeThreshold { get; init; }
}

/// <summary>
/// Specification for a research batch composing multiple studies (Section 8.5).
/// Must preserve per-study branch and artifact boundaries.
/// </summary>
public sealed class ResearchBatchSpec
{
    /// <summary>Unique batch identifier.</summary>
    [JsonPropertyName("batchId")]
    public required string BatchId { get; init; }

    /// <summary>Branch sweep studies in this batch.</summary>
    [JsonPropertyName("sweeps")]
    public required IReadOnlyList<BranchSweepSpec> Sweeps { get; init; }

    /// <summary>Stability studies in this batch.</summary>
    [JsonPropertyName("stabilityStudies")]
    public required IReadOnlyList<StabilityStudySpec> StabilityStudies { get; init; }

    /// <summary>Comparison campaign IDs to run.</summary>
    [JsonPropertyName("comparisonCampaignIds")]
    public required IReadOnlyList<string> ComparisonCampaignIds { get; init; }
}
