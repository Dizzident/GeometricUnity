using System.Text.Json.Serialization;

namespace Gu.Phase3.ModeTracking;

/// <summary>
/// A mode family: a set of modes across different contexts that are identified
/// as "the same physical mode" by the matching engine.
///
/// Families persist across continuation paths, branch sweeps, refinement levels,
/// and backend changes.
/// </summary>
public sealed class ModeFamilyRecord
{
    /// <summary>Unique family identifier.</summary>
    [JsonPropertyName("familyId")]
    public required string FamilyId { get; init; }

    /// <summary>Mode IDs that belong to this family, across all contexts.</summary>
    [JsonPropertyName("memberModeIds")]
    public required IReadOnlyList<string> MemberModeIds { get; init; }

    /// <summary>
    /// Context IDs (background IDs, branch IDs, etc.) in which this family has members.
    /// </summary>
    [JsonPropertyName("contextIds")]
    public required IReadOnlyList<string> ContextIds { get; init; }

    /// <summary>Mean eigenvalue across all members.</summary>
    [JsonPropertyName("meanEigenvalue")]
    public required double MeanEigenvalue { get; init; }

    /// <summary>Eigenvalue spread across all members.</summary>
    [JsonPropertyName("eigenvalueSpread")]
    public required double EigenvalueSpread { get; init; }

    /// <summary>
    /// Whether the family is stable (all members matched with high confidence).
    /// </summary>
    [JsonPropertyName("isStable")]
    public required bool IsStable { get; init; }

    /// <summary>Number of ambiguous matches in this family's history.</summary>
    [JsonPropertyName("ambiguityCount")]
    public int AmbiguityCount { get; init; }

    /// <summary>Alignment records tracing how modes were matched across contexts.</summary>
    [JsonPropertyName("alignments")]
    public required IReadOnlyList<ModeAlignmentRecord> Alignments { get; init; }
}
