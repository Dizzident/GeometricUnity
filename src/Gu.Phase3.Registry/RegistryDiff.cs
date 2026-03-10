namespace Gu.Phase3.Registry;

/// <summary>
/// Records a change in claim class for a candidate between two registry versions.
/// </summary>
public sealed class ClaimClassChange
{
    /// <summary>Candidate ID that changed claim class.</summary>
    public required string CandidateId { get; init; }

    /// <summary>Claim class in the base (this) registry.</summary>
    public required BosonClaimClass Before { get; init; }

    /// <summary>Claim class in the other registry.</summary>
    public required BosonClaimClass After { get; init; }
}

/// <summary>
/// Records new demotions added to a candidate between two registry versions.
/// </summary>
public sealed class DemotionChange
{
    /// <summary>Candidate ID with new demotions.</summary>
    public required string CandidateId { get; init; }

    /// <summary>Demotion records present in the other registry but not in the base.</summary>
    public required IReadOnlyList<BosonDemotionRecord> Added { get; init; }
}

/// <summary>
/// Diff between two BosonRegistry instances.
/// Computed as: what changed going from this (base) registry to the other registry.
/// </summary>
public sealed class RegistryDiff
{
    /// <summary>Candidate IDs present in other but not in base.</summary>
    public required IReadOnlyList<string> NewCandidateIds { get; init; }

    /// <summary>Candidate IDs present in base but not in other.</summary>
    public required IReadOnlyList<string> RemovedCandidateIds { get; init; }

    /// <summary>Candidates whose claim class changed between base and other.</summary>
    public required IReadOnlyList<ClaimClassChange> ClaimClassChanges { get; init; }

    /// <summary>Candidates that gained new demotion records in the other registry.</summary>
    public required IReadOnlyList<DemotionChange> DemotionChanges { get; init; }
}
