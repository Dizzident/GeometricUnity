using System.Text.Json.Serialization;
using Gu.Artifacts;
using Gu.Core;

namespace Gu.Phase5.Dossiers;

/// <summary>
/// A claim escalation or demotion record (M51).
///
/// Phase V engineering rule §12.5: promotion must be rule-based, not narrative-based.
/// Every promotion needs explicit gates and recorded evidence.
///
/// A ClaimEscalationRecord records:
/// - which candidate is being promoted or demoted,
/// - what escalation gates were checked,
/// - which gates passed and which failed,
/// - the resulting claim class change.
///
/// Claim classes mirror Phase III/IV:
/// C0_NumericalMode — mode exists in the spectrum (lowest).
/// C1_UnverifiedGpu — GPU-computed, not CPU-verified.
/// C2_CpuVerified   — CPU-verified mode.
/// C3_ObservationChainValid — observation chain is valid.
/// C4_BranchRobust  — survives admissible branch variation.
/// C5_QuantitativelyComparable — survives quantitative comparison with explicit uncertainty.
/// </summary>
public sealed class ClaimEscalationRecord
{
    /// <summary>Unique record identifier.</summary>
    [JsonPropertyName("recordId")]
    public required string RecordId { get; init; }

    /// <summary>Schema version.</summary>
    [JsonPropertyName("schemaVersion")]
    public string SchemaVersion { get; init; } = "1.0.0";

    /// <summary>
    /// The candidate ID in the boson or fermion registry being evaluated.
    /// </summary>
    [JsonPropertyName("candidateId")]
    public required string CandidateId { get; init; }

    /// <summary>
    /// Registry type: "boson" or "fermion".
    /// </summary>
    [JsonPropertyName("registryType")]
    public required string RegistryType { get; init; }

    /// <summary>
    /// Current claim class before this evaluation.
    /// One of: "C0", "C1", "C2", "C3", "C4", "C5".
    /// </summary>
    [JsonPropertyName("currentClaimClass")]
    public required string CurrentClaimClass { get; init; }

    /// <summary>
    /// Proposed claim class after this evaluation.
    /// Equals CurrentClaimClass if no change occurs.
    /// </summary>
    [JsonPropertyName("proposedClaimClass")]
    public required string ProposedClaimClass { get; init; }

    /// <summary>
    /// Direction of the claim change: "escalation", "demotion", "no-change".
    /// </summary>
    [JsonPropertyName("direction")]
    public required string Direction { get; init; }

    /// <summary>
    /// Gate evaluation results. One entry per declared escalation gate.
    /// </summary>
    [JsonPropertyName("gateResults")]
    public required List<EscalationGateResult> GateResults { get; init; }

    /// <summary>
    /// Whether all required gates passed.
    /// If false, no escalation is applied; the record documents why.
    /// </summary>
    [JsonPropertyName("allGatesPassed")]
    public bool AllGatesPassed { get; init; }

    /// <summary>
    /// Reason for demotion (if Direction == "demotion") or reason why escalation
    /// was blocked (if AllGatesPassed == false). Null if escalation succeeded.
    /// </summary>
    [JsonPropertyName("demotionOrBlockReason")]
    public string? DemotionOrBlockReason { get; init; }

    /// <summary>
    /// The study ID that produced this escalation record.
    /// </summary>
    [JsonPropertyName("sourceStudyId")]
    public required string SourceStudyId { get; init; }

    /// <summary>
    /// Evidence tier at which this evaluation was performed.
    /// Escalation to C4 or C5 requires at least RegeneratedCpu.
    /// </summary>
    [JsonPropertyName("evidenceTier")]
    public ArtifactEvidenceTier EvidenceTier { get; init; } = ArtifactEvidenceTier.RegeneratedCpu;

    /// <summary>When this record was created.</summary>
    [JsonPropertyName("recordedAt")]
    public required DateTimeOffset RecordedAt { get; init; }

    /// <summary>Provenance metadata.</summary>
    [JsonPropertyName("provenance")]
    public ProvenanceMeta? Provenance { get; init; }
}

/// <summary>
/// Result of evaluating a single escalation gate.
///
/// Phase V defines 6 escalation gates (§4.7):
/// 1. survives admissible branch variations,
/// 2. survives refinement with bounded uncertainty,
/// 3. survives more than one environment family,
/// 4. passes observation-chain validity checks,
/// 5. passes quantitative comparison within declared tolerances,
/// 6. has no active high-severity falsifier.
/// </summary>
public sealed class EscalationGateResult
{
    /// <summary>Gate identifier (e.g., "gate-1-branch-robustness").</summary>
    [JsonPropertyName("gateId")]
    public required string GateId { get; init; }

    /// <summary>Human-readable gate description.</summary>
    [JsonPropertyName("description")]
    public required string Description { get; init; }

    /// <summary>Whether this gate passed.</summary>
    [JsonPropertyName("passed")]
    public required bool Passed { get; init; }

    /// <summary>
    /// Evidence that supports the pass/fail determination.
    /// </summary>
    [JsonPropertyName("evidence")]
    public string? Evidence { get; init; }

    /// <summary>
    /// Whether this gate is required for the proposed escalation.
    /// If a required gate fails, the escalation is blocked.
    /// </summary>
    [JsonPropertyName("required")]
    public bool Required { get; init; } = true;
}

/// <summary>
/// Evaluates escalation gates and produces ClaimEscalationRecords (M51).
///
/// The evaluator enforces rule-based (not narrative-based) promotion.
/// </summary>
public static class ClaimEscalationEvaluator
{
    /// <summary>
    /// Evaluate whether a candidate can be escalated from its current class to
    /// the proposed class, given the supplied gate results.
    /// </summary>
    /// <param name="recordId">Unique record identifier.</param>
    /// <param name="candidateId">Candidate registry ID.</param>
    /// <param name="registryType">"boson" or "fermion".</param>
    /// <param name="currentClass">Current claim class.</param>
    /// <param name="proposedClass">Proposed claim class.</param>
    /// <param name="gateResults">Evaluation results for all applicable gates.</param>
    /// <param name="sourceStudyId">Study that triggered this evaluation.</param>
    /// <param name="provenance">Provenance metadata.</param>
    /// <returns>A ClaimEscalationRecord.</returns>
    public static ClaimEscalationRecord Evaluate(
        string recordId,
        string candidateId,
        string registryType,
        string currentClass,
        string proposedClass,
        IReadOnlyList<EscalationGateResult> gateResults,
        string sourceStudyId,
        ProvenanceMeta? provenance = null)
    {
        bool allRequired = gateResults.Where(g => g.Required).All(g => g.Passed);

        string direction;
        int currentRank = ClaimClassRank(currentClass);
        int proposedRank = ClaimClassRank(proposedClass);
        if (proposedRank > currentRank)
            direction = "escalation";
        else if (proposedRank < currentRank)
            direction = "demotion";
        else
            direction = "no-change";

        string? blockReason = null;
        if (direction == "escalation" && !allRequired)
        {
            var failedGates = gateResults.Where(g => g.Required && !g.Passed)
                .Select(g => g.GateId)
                .ToList();
            blockReason = $"Escalation blocked: required gates failed: [{string.Join(", ", failedGates)}]. " +
                          "All required gates must pass for claim promotion. This is a rule-based requirement.";
        }
        else if (direction == "demotion")
        {
            var failedGates = gateResults.Where(g => !g.Passed)
                .Select(g => g.GateId)
                .ToList();
            blockReason = failedGates.Count > 0
                ? $"Demotion triggered by failed gates: [{string.Join(", ", failedGates)}]."
                : "Demotion triggered by explicit demotion rule.";
        }

        return new ClaimEscalationRecord
        {
            RecordId = recordId,
            CandidateId = candidateId,
            RegistryType = registryType,
            CurrentClaimClass = currentClass,
            ProposedClaimClass = direction == "escalation" && !allRequired ? currentClass : proposedClass,
            Direction = direction == "escalation" && !allRequired ? "no-change" : direction,
            GateResults = gateResults.ToList(),
            AllGatesPassed = allRequired,
            DemotionOrBlockReason = blockReason,
            SourceStudyId = sourceStudyId,
            RecordedAt = DateTimeOffset.UtcNow,
            Provenance = provenance,
        };
    }

    private static int ClaimClassRank(string claimClass) => claimClass.ToUpperInvariant() switch
    {
        "C0" or "C0_NUMERICALMODE" => 0,
        "C1" or "C1_UNVERIFIEDGPU" => 1,
        "C2" or "C2_CPUVERIFIED"   => 2,
        "C3" or "C3_OBSERVATIONCHAINVALID" => 3,
        "C4" or "C4_BRANCHROBUST"  => 4,
        "C5" or "C5_QUANTITATIVELYCOMPARABLE" => 5,
        _ => -1,
    };
}
