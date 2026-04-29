using System.Text.Json.Serialization;

namespace Gu.Phase5.Reporting;

public sealed class NormalizedWeakCouplingCandidatePromotionResult
{
    [JsonPropertyName("algorithmId")]
    public required string AlgorithmId { get; init; }

    [JsonPropertyName("terminalStatus")]
    public required string TerminalStatus { get; init; }

    [JsonPropertyName("candidate")]
    public NormalizedWeakCouplingCandidateRecord? Candidate { get; init; }

    [JsonPropertyName("phase61Audit")]
    public required NormalizedWeakCouplingInputAuditResult Phase61Audit { get; init; }

    [JsonPropertyName("closureRequirements")]
    public required IReadOnlyList<string> ClosureRequirements { get; init; }
}

public static class NormalizedWeakCouplingCandidatePromoter
{
    public const string AlgorithmId = "phase68-normalized-weak-coupling-candidate-promoter-v1";

    public static NormalizedWeakCouplingCandidatePromotionResult Promote(
        NormalizedWeakCouplingCandidateRecord candidate,
        IReadOnlyList<string> requiredExcludedTargetObservableIds,
        double minimumBranchStabilityScore = NormalizedWeakCouplingInputAuditor.DefaultMinimumBranchStabilityScore)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        ArgumentNullException.ThrowIfNull(requiredExcludedTargetObservableIds);

        var audit = NormalizedWeakCouplingInputAuditor.Audit(
            [candidate],
            requiredExcludedTargetObservableIds,
            minimumBranchStabilityScore);

        var accepted = audit.AcceptedCandidateCount == 1;
        return new NormalizedWeakCouplingCandidatePromotionResult
        {
            AlgorithmId = AlgorithmId,
            TerminalStatus = accepted
                ? "normalized-weak-coupling-candidate-promoted"
                : "normalized-weak-coupling-candidate-blocked",
            Candidate = accepted ? candidate : null,
            Phase61Audit = audit,
            ClosureRequirements = accepted ? [] : audit.ClosureRequirements,
        };
    }
}
