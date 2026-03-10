namespace Gu.Phase3.Registry;

/// <summary>
/// Claim strength classification for candidate bosons.
///
/// Ordered from weakest (C0) to strongest (C5).
/// The system can demote claims automatically when gauge leak,
/// refinement fragility, branch fragility, or comparison mismatch appears.
/// </summary>
public enum BosonClaimClass
{
    /// <summary>
    /// C0: Raw computed mode. No matching, continuation, or branch evidence yet.
    /// </summary>
    C0_NumericalMode = 0,

    /// <summary>
    /// C1: Persistent under local continuation or refinement.
    /// Mode family survives across at least one continuation path or refinement step.
    /// </summary>
    C1_LocalPersistentMode = 1,

    /// <summary>
    /// C2: Persists across branch variant family.
    /// Mode is stable across multiple branch configurations.
    /// </summary>
    C2_BranchStableBosonicCandidate = 2,

    /// <summary>
    /// C3: Also stable in observed-space signatures.
    /// The mode's observed properties persist across contexts.
    /// </summary>
    C3_ObservedStableCandidate = 3,

    /// <summary>
    /// C4: Comparable to an external target but not uniquely identified.
    /// A physical analogy exists but may not be unique.
    /// </summary>
    C4_PhysicalAnalogyCandidate = 4,

    /// <summary>
    /// C5: Strong evidence for identification. Still not proof.
    /// Multiple independent metrics support the identification.
    /// </summary>
    C5_StrongIdentificationCandidate = 5,
}
