namespace Gu.Phase5.Dossiers;

/// <summary>
/// String constants for Phase V escalation gate identifiers (M51).
///
/// Escalation gates are the rule-based criteria that must be satisfied
/// before a candidate's claim class can be promoted. Each gate corresponds
/// to a specific type of Phase V evidence.
///
/// Promotion: all required gates passed → promote by 1 level (max C5).
/// Hold: some gates failed, no fatal falsifier → no change.
/// Demotion: any fatal falsifier active → demote to C0.
/// </summary>
public static class EscalationGates
{
    /// <summary>
    /// Gate 1: Candidate survives admissible branch variations.
    /// Requires BranchRobustnessRecord showing candidate quantity in an equivalence
    /// class covering > 50% of variants.
    /// </summary>
    public const string BranchRobust = "branch-robust";

    /// <summary>
    /// Gate 2: Refinement error is bounded.
    /// Requires ContinuumEstimateRecord with errorBand &lt; 10% of extrapolated value.
    /// </summary>
    public const string RefinementBounded = "refinement-bounded";

    /// <summary>
    /// Gate 3: Multi-environment evidence.
    /// Requires quantity computed on at least 2 distinct environment tiers.
    /// </summary>
    public const string MultiEnvironment = "multi-environment";

    /// <summary>
    /// Gate 4: Observation chain validity.
    /// Requires complete observation provenance chain for this candidate.
    /// </summary>
    public const string ObservationChainValid = "observation-chain-valid";

    /// <summary>
    /// Gate 5: Quantitative match.
    /// Requires at least one TargetMatchRecord with Passed = true for this candidate.
    /// </summary>
    public const string QuantitativeMatch = "quantitative-match";

    /// <summary>
    /// Gate 6: No active fatal falsifier.
    /// Requires that no active fatal FalsifierRecord targets this candidate.
    /// </summary>
    public const string NoActiveFatalFalsifier = "no-active-fatal-falsifier";
}
