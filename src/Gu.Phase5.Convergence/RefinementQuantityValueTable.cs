using System.Text.Json.Serialization;

namespace Gu.Phase5.Convergence;

/// <summary>
/// A table of pre-computed quantity values for each refinement level.
/// Used as the values file input for the `refinement-study` CLI command (WP-4/D-009).
/// Each level must match a <see cref="RefinementStudySpec.RefinementLevels"/> entry by LevelId.
/// </summary>
public sealed class RefinementQuantityValueTable
{
    /// <summary>Matches the StudyId of the corresponding RefinementStudySpec.</summary>
    [JsonPropertyName("studyId")]
    public required string StudyId { get; init; }

    /// <summary>One level entry per refinement level in the study.</summary>
    [JsonPropertyName("levels")]
    public required IReadOnlyList<RefinementQuantityValueLevel> Levels { get; init; }
}

/// <summary>
/// Quantity values for a single refinement level.
/// </summary>
public sealed class RefinementQuantityValueLevel
{
    /// <summary>Must match a LevelId in the corresponding RefinementStudySpec.</summary>
    [JsonPropertyName("levelId")]
    public required string LevelId { get; init; }

    /// <summary>True if the solver converged for this level.</summary>
    [JsonPropertyName("solverConverged")]
    public bool SolverConverged { get; init; } = true;

    /// <summary>Quantity values keyed by quantity ID.</summary>
    [JsonPropertyName("quantities")]
    public required IReadOnlyDictionary<string, double> Quantities { get; init; }
}
