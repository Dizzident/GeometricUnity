using System.Text.Json.Serialization;

namespace Gu.Phase5.BranchIndependence;

/// <summary>
/// Specification for a branch-robustness study (M46).
///
/// Declares the branch family, target quantities, and comparison policy
/// for a branch-independence sweep. The study measures how sensitive
/// each declared target quantity is to variation across the admissible
/// branch family.
/// </summary>
public sealed class BranchRobustnessStudySpec
{
    /// <summary>Unique study identifier.</summary>
    [JsonPropertyName("studyId")]
    public required string StudyId { get; init; }

    /// <summary>Schema version.</summary>
    [JsonPropertyName("schemaVersion")]
    public string SchemaVersion { get; init; } = "1.0.0";

    /// <summary>
    /// Human-readable description of the study goal.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }

    /// <summary>
    /// List of branch variant IDs to include in the sweep.
    /// Each variant is an admissible branch configuration differing in
    /// torsion, Shiab, gauge strategy, or other declared branch choices.
    /// </summary>
    [JsonPropertyName("branchVariantIds")]
    public required List<string> BranchVariantIds { get; init; }

    /// <summary>
    /// Target quantity IDs to track across branch variants.
    /// Examples: "bosonic-eigenvalue-0", "fermionic-eigenvalue-0", "coupling-proxy-00".
    /// </summary>
    [JsonPropertyName("targetQuantityIds")]
    public required List<string> TargetQuantityIds { get; init; }

    /// <summary>
    /// Absolute tolerance used for distance computation and equivalence classification.
    /// Two quantity values are considered equivalent if |q_i - q_j| &lt;= AbsoluteTolerance.
    /// </summary>
    [JsonPropertyName("absoluteTolerance")]
    public double AbsoluteTolerance { get; init; } = 1e-6;

    /// <summary>
    /// Relative tolerance used alongside absolute tolerance.
    /// Two values are equivalent if |q_i - q_j| / (|q_ref| + 1e-30) &lt;= RelativeTolerance.
    /// </summary>
    [JsonPropertyName("relativeTolerance")]
    public double RelativeTolerance { get; init; } = 1e-4;

    /// <summary>
    /// Environment ID to use for all branch evaluations.
    /// If null, the study uses the default toy environment.
    /// </summary>
    [JsonPropertyName("environmentId")]
    public string? EnvironmentId { get; init; }

    /// <summary>
    /// Optional reference branch variant ID for distance normalization.
    /// If null, the first variant in BranchVariantIds is used as reference.
    /// </summary>
    [JsonPropertyName("referenceBranchVariantId")]
    public string? ReferenceBranchVariantId { get; init; }
}
