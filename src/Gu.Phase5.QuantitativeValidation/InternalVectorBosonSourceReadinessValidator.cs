using Gu.Core;

namespace Gu.Phase5.QuantitativeValidation;

public static class InternalVectorBosonSourceReadinessValidator
{
    private const int MinimumReadySourceCount = 2;

    public static IReadOnlyList<string> Validate(
        InternalVectorBosonSourceCandidate candidate,
        InternalVectorBosonSourceReadinessPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        ArgumentNullException.ThrowIfNull(policy);

        var errors = new List<string>();
        if (!string.Equals(candidate.SourceOrigin, InternalVectorBosonSourceCandidateAdapter.SourceOrigin, StringComparison.Ordinal))
            errors.Add("source origin is not internal-computed-artifact.");
        if (!string.Equals(candidate.ModeRole, InternalVectorBosonSourceCandidateAdapter.ModeRole, StringComparison.Ordinal))
            errors.Add("mode role is not vector-boson-source-candidate.");
        if (ContainsExternalTargetPath(candidate.SourceArtifactPaths))
            errors.Add("source artifact paths include an external target table.");
        if (policy.RequireBranchSelectors && candidate.BranchSelectors.Count == 0)
            errors.Add("branch selectors are missing.");
        if (policy.RequireRefinementCoverage && candidate.RefinementLevels.Count == 0)
            errors.Add("refinement coverage is missing.");
        if (policy.RequireEnvironmentSelectors && candidate.EnvironmentSelectors.Count == 0)
            errors.Add("environment/background selectors are missing.");
        if (candidate.AmbiguityCount is null)
            errors.Add("ambiguity count is missing.");
        else if (candidate.AmbiguityCount > policy.MaximumAmbiguityCount)
            errors.Add($"ambiguity count {candidate.AmbiguityCount} exceeds threshold {policy.MaximumAmbiguityCount}.");
        if (candidate.BranchStabilityScore is null)
            errors.Add("branch stability score is missing.");
        else if (candidate.BranchStabilityScore < policy.MinimumBranchStabilityScore)
            errors.Add($"branch stability score {candidate.BranchStabilityScore:G6} is below threshold {policy.MinimumBranchStabilityScore:G6}.");
        if (candidate.RefinementStabilityScore is null)
            errors.Add("refinement stability score is missing.");
        else if (candidate.RefinementStabilityScore < policy.MinimumRefinementStabilityScore)
            errors.Add($"refinement stability score {candidate.RefinementStabilityScore:G6} is below threshold {policy.MinimumRefinementStabilityScore:G6}.");
        if (string.IsNullOrWhiteSpace(candidate.ClaimClass))
            errors.Add("claim class is missing.");
        else if (!IsClaimClassAllowed(candidate.ClaimClass, policy))
            errors.Add($"claim class '{candidate.ClaimClass}' is below readiness threshold '{policy.MinimumClaimClass}'.");
        if (policy.RequireCompleteUncertainty)
        {
            if (!candidate.Uncertainty.IsFullyEstimated)
                errors.Add("source uncertainty components are incomplete.");
            if (candidate.Uncertainty.TotalUncertainty < 0)
                errors.Add("source total uncertainty is unestimated.");
        }

        return errors;
    }

    public static InternalVectorBosonSourceCandidateTable Reevaluate(
        InternalVectorBosonSourceCandidateTable sourceTable,
        InternalVectorBosonSourceReadinessCampaignSpec spec,
        ProvenanceMeta provenance)
    {
        ArgumentNullException.ThrowIfNull(sourceTable);
        ArgumentNullException.ThrowIfNull(spec);

        var candidates = sourceTable.Candidates
            .Select(c => ReevaluateCandidate(c, spec.ReadinessPolicy, provenance))
            .ToList();
        var readyCount = candidates.Count(c =>
            string.Equals(c.Status, "candidate-source-ready", StringComparison.Ordinal));
        var summary = readyCount >= MinimumReadySourceCount
            ? [$"{readyCount} source candidate(s) satisfy Phase XXI readiness policy."]
            : readyCount > 0
                ? [$"Only {readyCount} source candidate(s) satisfy Phase XXI readiness policy; at least {MinimumReadySourceCount} are required."]
            : BuildSummaryBlockers(candidates);

        return new InternalVectorBosonSourceCandidateTable
        {
            TableId = "phase21-source-readiness-candidates-v1",
            SchemaVersion = "1.0.0",
            TerminalStatus = readyCount >= MinimumReadySourceCount ? "candidate-source-ready" : "source-blocked",
            Candidates = candidates,
            SummaryBlockers = summary,
            Provenance = provenance,
        };
    }

    private static InternalVectorBosonSourceCandidate ReevaluateCandidate(
        InternalVectorBosonSourceCandidate candidate,
        InternalVectorBosonSourceReadinessPolicy policy,
        ProvenanceMeta provenance)
    {
        var blockers = Validate(candidate, policy);
        var status = blockers.Count == 0 ? "candidate-source-ready" : "source-blocked";
        return new InternalVectorBosonSourceCandidate
        {
            SourceCandidateId = candidate.SourceCandidateId,
            SourceOrigin = candidate.SourceOrigin,
            ModeRole = candidate.ModeRole,
            SourceArtifactPaths = candidate.SourceArtifactPaths,
            SourceModeIds = candidate.SourceModeIds,
            SourceFamilyId = candidate.SourceFamilyId,
            MassLikeValue = candidate.MassLikeValue,
            Uncertainty = candidate.Uncertainty,
            BranchSelectors = candidate.BranchSelectors,
            EnvironmentSelectors = candidate.EnvironmentSelectors,
            RefinementLevels = candidate.RefinementLevels,
            BranchStabilityScore = candidate.BranchStabilityScore,
            RefinementStabilityScore = candidate.RefinementStabilityScore,
            BackendStabilityScore = candidate.BackendStabilityScore,
            ObservationStabilityScore = candidate.ObservationStabilityScore,
            AmbiguityCount = candidate.AmbiguityCount,
            GaugeLeakEnvelope = candidate.GaugeLeakEnvelope,
            ClaimClass = candidate.ClaimClass,
            Status = status,
            Assumptions = candidate.Assumptions,
            ClosureRequirements = blockers,
            Provenance = provenance,
        };
    }

    private static IReadOnlyList<string> BuildSummaryBlockers(IReadOnlyList<InternalVectorBosonSourceCandidate> candidates)
        => candidates
            .SelectMany(c => c.ClosureRequirements)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(x => x, StringComparer.Ordinal)
            .ToList();

    private static bool ContainsExternalTargetPath(IReadOnlyList<string> paths)
        => paths.Any(path =>
            path.Contains("external_target", StringComparison.OrdinalIgnoreCase) ||
            path.Contains("target_table", StringComparison.OrdinalIgnoreCase) ||
            path.Contains("physical_targets", StringComparison.OrdinalIgnoreCase));

    private static bool IsClaimClassAllowed(string claimClass, InternalVectorBosonSourceReadinessPolicy policy)
    {
        if (policy.AllowedClaimClasses.Count > 0)
            return policy.AllowedClaimClasses.Contains(claimClass, StringComparer.Ordinal);

        return string.Equals(claimClass, policy.MinimumClaimClass, StringComparison.Ordinal);
    }
}
