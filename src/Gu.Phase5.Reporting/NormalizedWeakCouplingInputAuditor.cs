using System.Text.Json.Serialization;

namespace Gu.Phase5.Reporting;

public sealed record NormalizedWeakCouplingCandidateRecord
{
    [JsonPropertyName("candidateId")]
    public required string CandidateId { get; init; }

    [JsonPropertyName("sourceKind")]
    public required string SourceKind { get; init; }

    [JsonPropertyName("normalizationConvention")]
    public required string NormalizationConvention { get; init; }

    [JsonPropertyName("couplingValue")]
    public double? CouplingValue { get; init; }

    [JsonPropertyName("couplingUncertainty")]
    public double? CouplingUncertainty { get; init; }

    [JsonPropertyName("variationMethod")]
    public string? VariationMethod { get; init; }

    [JsonPropertyName("branchStabilityScore")]
    public double? BranchStabilityScore { get; init; }

    [JsonPropertyName("excludedTargetObservableIds")]
    public required IReadOnlyList<string> ExcludedTargetObservableIds { get; init; }

    [JsonPropertyName("provenanceId")]
    public string? ProvenanceId { get; init; }
}

public sealed class NormalizedWeakCouplingInputAuditRecord
{
    [JsonPropertyName("candidateId")]
    public required string CandidateId { get; init; }

    [JsonPropertyName("sourceKind")]
    public required string SourceKind { get; init; }

    [JsonPropertyName("status")]
    public required string Status { get; init; }

    [JsonPropertyName("blockReasons")]
    public required IReadOnlyList<string> BlockReasons { get; init; }
}

public sealed class NormalizedWeakCouplingInputAuditResult
{
    [JsonPropertyName("algorithmId")]
    public required string AlgorithmId { get; init; }

    [JsonPropertyName("terminalStatus")]
    public required string TerminalStatus { get; init; }

    [JsonPropertyName("acceptedCandidateCount")]
    public required int AcceptedCandidateCount { get; init; }

    [JsonPropertyName("rejectedCandidateCount")]
    public required int RejectedCandidateCount { get; init; }

    [JsonPropertyName("requiredExcludedTargetObservableIds")]
    public required IReadOnlyList<string> RequiredExcludedTargetObservableIds { get; init; }

    [JsonPropertyName("minimumBranchStabilityScore")]
    public required double MinimumBranchStabilityScore { get; init; }

    [JsonPropertyName("records")]
    public required IReadOnlyList<NormalizedWeakCouplingInputAuditRecord> Records { get; init; }

    [JsonPropertyName("closureRequirements")]
    public required IReadOnlyList<string> ClosureRequirements { get; init; }
}

public static class NormalizedWeakCouplingInputAuditor
{
    public const string AlgorithmId = "phase61-normalized-weak-coupling-input-auditor-v1";
    public const string AcceptedSourceKind = "normalized-internal-weak-coupling";
    public const string AcceptedNormalizationPrefix = "physical-weak-coupling-normalization:";
    public const double DefaultMinimumBranchStabilityScore = 0.95;

    public static NormalizedWeakCouplingInputAuditResult Audit(
        IReadOnlyList<NormalizedWeakCouplingCandidateRecord> candidates,
        IReadOnlyList<string> requiredExcludedTargetObservableIds,
        double minimumBranchStabilityScore = DefaultMinimumBranchStabilityScore)
    {
        ArgumentNullException.ThrowIfNull(candidates);
        ArgumentNullException.ThrowIfNull(requiredExcludedTargetObservableIds);

        var records = candidates
            .Select(candidate => AuditCandidate(
                candidate,
                requiredExcludedTargetObservableIds,
                minimumBranchStabilityScore))
            .ToArray();

        var accepted = records.Count(record => string.Equals(record.Status, "accepted", StringComparison.Ordinal));
        var rejected = records.Length - accepted;
        var closure = BuildClosureRequirements(records, accepted);

        return new NormalizedWeakCouplingInputAuditResult
        {
            AlgorithmId = AlgorithmId,
            TerminalStatus = accepted > 0
                ? "normalized-weak-coupling-inputs-ready"
                : "normalized-weak-coupling-inputs-blocked",
            AcceptedCandidateCount = accepted,
            RejectedCandidateCount = rejected,
            RequiredExcludedTargetObservableIds = requiredExcludedTargetObservableIds,
            MinimumBranchStabilityScore = minimumBranchStabilityScore,
            Records = records,
            ClosureRequirements = closure,
        };
    }

    private static NormalizedWeakCouplingInputAuditRecord AuditCandidate(
        NormalizedWeakCouplingCandidateRecord candidate,
        IReadOnlyList<string> requiredExcludedTargetObservableIds,
        double minimumBranchStabilityScore)
    {
        var reasons = new List<string>();

        if (!string.Equals(candidate.SourceKind, AcceptedSourceKind, StringComparison.Ordinal))
            reasons.Add($"source kind must be {AcceptedSourceKind}");

        if (!candidate.NormalizationConvention.StartsWith(AcceptedNormalizationPrefix, StringComparison.Ordinal))
            reasons.Add($"normalization convention must start with {AcceptedNormalizationPrefix}");

        if (candidate.CouplingValue is not { } value || !double.IsFinite(value) || value <= 0.0)
            reasons.Add("coupling value must be finite and positive");

        if (candidate.CouplingUncertainty is not { } uncertainty || !double.IsFinite(uncertainty) || uncertainty < 0.0)
            reasons.Add("coupling uncertainty must be finite and non-negative");

        if (string.Equals(candidate.VariationMethod, "finite-difference", StringComparison.OrdinalIgnoreCase))
            reasons.Add("finite-difference coupling proxies are not normalized weak-coupling inputs");

        if (candidate.BranchStabilityScore is not { } stability || !double.IsFinite(stability) || stability < minimumBranchStabilityScore)
            reasons.Add($"branch stability score must be finite and at least {minimumBranchStabilityScore:R}");

        var missingExclusions = requiredExcludedTargetObservableIds
            .Where(required => !candidate.ExcludedTargetObservableIds.Contains(required, StringComparer.Ordinal))
            .ToArray();
        if (missingExclusions.Length > 0)
            reasons.Add($"candidate must exclude target observable ids: {string.Join(", ", missingExclusions)}");

        return new NormalizedWeakCouplingInputAuditRecord
        {
            CandidateId = candidate.CandidateId,
            SourceKind = candidate.SourceKind,
            Status = reasons.Count == 0 ? "accepted" : "rejected",
            BlockReasons = reasons,
        };
    }

    private static IReadOnlyList<string> BuildClosureRequirements(
        IReadOnlyList<NormalizedWeakCouplingInputAuditRecord> records,
        int acceptedCount)
    {
        if (acceptedCount > 0)
            return [];

        var requirements = records
            .SelectMany(record => record.BlockReasons)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(requirement => requirement, StringComparer.Ordinal)
            .ToList();

        requirements.Insert(0, "no accepted normalized weak-coupling candidate is available");
        return requirements;
    }
}
