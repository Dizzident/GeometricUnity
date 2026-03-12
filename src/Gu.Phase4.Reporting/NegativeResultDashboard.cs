using System.Text.Json.Serialization;

namespace Gu.Phase4.Reporting;

/// <summary>
/// Dashboard of negative results from a Phase IV study.
///
/// Negative results are first-class outputs: they constrain the claim space and
/// distinguish branch-consistent execution (passes structural tests) from
/// physical validation (demonstrates claimed spectrum or symmetry properties).
///
/// Three categories of Phase IV negative results:
///   1. UnstableChiralityEntries — families with broken or ambiguous chirality
///   2. FragileCouplingEntries — boson-fermion couplings that are numerically weak
///      or branch-unstable (low evidence for physical interaction)
///   3. BrokenFamilyClusterEntries — family clusters that failed persistence or
///      split/merge criteria (branch-inconsistent groupings)
/// </summary>
public sealed class NegativeResultDashboard
{
    /// <summary>Dashboard identifier.</summary>
    [JsonPropertyName("dashboardId")]
    public required string DashboardId { get; init; }

    /// <summary>Study identifier that produced these results.</summary>
    [JsonPropertyName("studyId")]
    public required string StudyId { get; init; }

    /// <summary>
    /// Families with unstable, trivial, or ambiguous chirality.
    /// These are families that could not be assigned a definite chirality label,
    /// or whose chirality profile was inconsistent across branches.
    /// </summary>
    [JsonPropertyName("unstableChiralityEntries")]
    public required List<UnstableChiralityEntry> UnstableChiralityEntries { get; init; }

    /// <summary>
    /// Couplings with low magnitude or branch instability.
    /// These are boson-fermion couplings below the nonzero threshold or with
    /// Frobenius norm that varies substantially across branches.
    /// </summary>
    [JsonPropertyName("fragileCouplingEntries")]
    public required List<FragileCouplingEntry> FragileCouplingEntries { get; init; }

    /// <summary>
    /// Family clusters that failed persistence or coherence tests.
    /// These are generation-like clusters that could not be confirmed as
    /// stable multi-branch groupings.
    /// </summary>
    [JsonPropertyName("brokenFamilyClusterEntries")]
    public required List<BrokenFamilyClusterEntry> BrokenFamilyClusterEntries { get; init; }

    /// <summary>Additional freeform negative result descriptions.</summary>
    [JsonPropertyName("additionalNotes")]
    public required List<string> AdditionalNotes { get; init; }

    /// <summary>Total count of all negative result entries.</summary>
    [JsonPropertyName("totalNegativeResults")]
    public int TotalNegativeResults
        => UnstableChiralityEntries.Count + FragileCouplingEntries.Count
           + BrokenFamilyClusterEntries.Count + AdditionalNotes.Count;
}

/// <summary>
/// A family with unstable or ambiguous chirality.
/// </summary>
public sealed class UnstableChiralityEntry
{
    /// <summary>Family identifier.</summary>
    [JsonPropertyName("familyId")]
    public required string FamilyId { get; init; }

    /// <summary>
    /// Chirality status that triggered this negative result:
    /// "mixed", "trivial", "ambiguous", or "branch-inconsistent".
    /// </summary>
    [JsonPropertyName("chiralityStatus")]
    public required string ChiralityStatus { get; init; }

    /// <summary>Left chirality projection weight.</summary>
    [JsonPropertyName("leftProjection")]
    public required double LeftProjection { get; init; }

    /// <summary>Right chirality projection weight.</summary>
    [JsonPropertyName("rightProjection")]
    public required double RightProjection { get; init; }

    /// <summary>Gauge leakage norm (high values indicate gauge contamination).</summary>
    [JsonPropertyName("leakageNorm")]
    public required double LeakageNorm { get; init; }

    /// <summary>
    /// Why this is a negative result.
    /// E.g. "No definite chirality assignment possible — projection weights (L=0.52, R=0.48)"
    /// </summary>
    [JsonPropertyName("reason")]
    public required string Reason { get; init; }
}

/// <summary>
/// A boson-fermion coupling entry that is fragile or below significance threshold.
/// </summary>
public sealed class FragileCouplingEntry
{
    /// <summary>Boson mode identifier.</summary>
    [JsonPropertyName("bosonModeId")]
    public required string BosonModeId { get; init; }

    /// <summary>Maximum coupling magnitude in this boson's coupling matrix.</summary>
    [JsonPropertyName("maxMagnitude")]
    public required double MaxMagnitude { get; init; }

    /// <summary>Frobenius norm of the coupling matrix.</summary>
    [JsonPropertyName("frobeniusNorm")]
    public required double FrobeniusNorm { get; init; }

    /// <summary>Number of fermion pairs with this boson.</summary>
    [JsonPropertyName("fermionPairCount")]
    public required int FermionPairCount { get; init; }

    /// <summary>
    /// Reason for fragility classification:
    /// "below-noise-floor", "branch-unstable", "gauge-leak-contaminated".
    /// </summary>
    [JsonPropertyName("fragilityReason")]
    public required string FragilityReason { get; init; }
}

/// <summary>
/// A family cluster that failed persistence or coherence tests.
/// </summary>
public sealed class BrokenFamilyClusterEntry
{
    /// <summary>Cluster identifier.</summary>
    [JsonPropertyName("clusterId")]
    public required string ClusterId { get; init; }

    /// <summary>Mean branch persistence score of this cluster.</summary>
    [JsonPropertyName("meanPersistence")]
    public required double MeanPersistence { get; init; }

    /// <summary>Ambiguity score for this cluster.</summary>
    [JsonPropertyName("ambiguityScore")]
    public required double AmbiguityScore { get; init; }

    /// <summary>Number of member families in the cluster.</summary>
    [JsonPropertyName("memberCount")]
    public required int MemberCount { get; init; }

    /// <summary>
    /// Failure mode: "low-persistence", "high-ambiguity", "split-detected",
    /// "merge-detected", "avoided-crossing".
    /// </summary>
    [JsonPropertyName("failureMode")]
    public required string FailureMode { get; init; }

    /// <summary>Demotion reason as recorded in the unified registry.</summary>
    [JsonPropertyName("demotionReason")]
    public required string DemotionReason { get; init; }
}
