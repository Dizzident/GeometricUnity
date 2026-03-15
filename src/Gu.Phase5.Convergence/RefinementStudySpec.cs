using System.Text.Json.Serialization;
using Gu.Core;

namespace Gu.Phase5.Convergence;

/// <summary>
/// Specification for a refinement convergence study (M47).
/// Drives Richardson extrapolation over a sequence of refinement levels.
/// </summary>
public sealed class RefinementStudySpec
{
    /// <summary>Unique study identifier.</summary>
    [JsonPropertyName("studyId")]
    public required string StudyId { get; init; }

    /// <summary>Schema version.</summary>
    [JsonPropertyName("schemaVersion")]
    public required string SchemaVersion { get; init; }

    /// <summary>Branch manifest ID to use for all refinement runs.</summary>
    [JsonPropertyName("branchManifestId")]
    public required string BranchManifestId { get; init; }

    /// <summary>Target quantity IDs to track across refinement levels.</summary>
    [JsonPropertyName("targetQuantities")]
    public required IReadOnlyList<string> TargetQuantities { get; init; }

    /// <summary>
    /// Refinement levels in order of decreasing h (coarsest to finest).
    /// At least 3 levels are required for Richardson extrapolation.
    /// </summary>
    [JsonPropertyName("refinementLevels")]
    public required IReadOnlyList<RefinementLevel> RefinementLevels { get; init; }

    /// <summary>
    /// Extrapolation method: "richardson" (default) or "linear".
    /// </summary>
    [JsonPropertyName("extrapolationMethod")]
    public string ExtrapolationMethod { get; init; } = "richardson";

    /// <summary>
    /// Optional Shiab operator variant ID to use in refinement runs.
    /// When null, the identity Shiab (S=F) is used.
    /// Note: spelled ShiabVariantId, not ShibaVariantId.
    /// </summary>
    [JsonPropertyName("shiabVariantId")]
    public string? ShiabVariantId { get; init; }

    /// <summary>Provenance metadata.</summary>
    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}
