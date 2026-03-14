using System.Text.Json.Serialization;

namespace Gu.Phase5.BranchIndependence;

/// <summary>
/// A target quantity that appears branch-invariant across the evaluated branch family (M46).
///
/// Invariance is a necessary (but not sufficient) condition for physical significance.
/// An invariant quantity may still fail refinement, environment, or quantitative checks.
///
/// Conservative labeling: never assert this quantity is "physical" — only that it survived
/// the branch sweep under declared tolerances.
/// </summary>
public sealed class InvarianceCandidateRecord
{
    /// <summary>Schema version.</summary>
    [JsonPropertyName("schemaVersion")]
    public string SchemaVersion { get; init; } = "1.0.0";

    /// <summary>Target quantity ID that is branch-invariant.</summary>
    [JsonPropertyName("targetQuantityId")]
    public required string TargetQuantityId { get; init; }

    /// <summary>
    /// Branch family size (number of variants in the sweep).
    /// </summary>
    [JsonPropertyName("branchFamilySize")]
    public int BranchFamilySize { get; init; }

    /// <summary>
    /// Mean value across branch variants.
    /// </summary>
    [JsonPropertyName("meanValue")]
    public double MeanValue { get; init; }

    /// <summary>
    /// Maximum absolute deviation from the mean across all branch variants.
    /// </summary>
    [JsonPropertyName("maxAbsDeviation")]
    public double MaxAbsDeviation { get; init; }

    /// <summary>
    /// Fragility score at which this quantity was classified invariant.
    /// </summary>
    [JsonPropertyName("fragilityScore")]
    public double FragilityScore { get; init; }

    /// <summary>
    /// Absolute tolerance used for invariance classification.
    /// </summary>
    [JsonPropertyName("absoluteTolerance")]
    public double AbsoluteTolerance { get; init; }

    /// <summary>
    /// Relative tolerance used for invariance classification.
    /// </summary>
    [JsonPropertyName("relativeTolerance")]
    public double RelativeTolerance { get; init; }

    /// <summary>
    /// Study ID that produced this invariance candidate.
    /// </summary>
    [JsonPropertyName("sourceStudyId")]
    public required string SourceStudyId { get; init; }

    /// <summary>
    /// Conservative note: what this invariance record does and does not claim.
    /// </summary>
    [JsonPropertyName("invarianceNote")]
    public string InvarianceNote { get; init; } =
        "Branch-invariant within declared tolerances. Does not imply physical significance. " +
        "Downstream refinement and quantitative checks required.";
}
