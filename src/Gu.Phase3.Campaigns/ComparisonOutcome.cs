namespace Gu.Phase3.Campaigns;

/// <summary>
/// Outcome of comparing a candidate boson to a target profile.
///
/// IMPORTANT: The system never forces a unique match. Multiple candidates
/// may be Compatible with the same target, and a single candidate may
/// be Compatible with multiple targets.
/// </summary>
public enum ComparisonOutcome
{
    /// <summary>Candidate properties are consistent with the target profile within tolerances.</summary>
    Compatible,

    /// <summary>Candidate properties contradict the target profile beyond tolerances.</summary>
    Incompatible,

    /// <summary>Evidence is ambiguous; neither clearly compatible nor incompatible.</summary>
    Underdetermined,

    /// <summary>Not enough data to make a determination.</summary>
    InsufficientEvidence,
}
