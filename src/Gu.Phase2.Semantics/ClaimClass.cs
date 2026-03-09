namespace Gu.Phase2.Semantics;

/// <summary>
/// Claim class hierarchy for Phase II results (Section 5.7).
/// Determines what level of physical claim a result can support.
/// </summary>
public enum ClaimClass
{
    ExactStructuralConsequence,
    ApproximateStructuralSurrogate,
    PostdictionTarget,
    SpeculativeInterpretation,
    Inadmissible,
}
