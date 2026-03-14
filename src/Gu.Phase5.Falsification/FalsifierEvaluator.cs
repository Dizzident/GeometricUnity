using Gu.Core;
using Gu.Phase5.BranchIndependence;
using Gu.Phase5.Convergence;
using Gu.Phase5.QuantitativeValidation;

namespace Gu.Phase5.Falsification;

/// <summary>
/// Evaluates falsifier conditions against Phase V study results (M50).
///
/// Falsifier sources:
///   BranchFragility       -- from BranchRobustnessRecord (fragility score > threshold)
///   NonConvergence        -- from ConvergenceFailureRecord (any non-convergent quantity)
///   QuantitativeMismatch  -- from ConsistencyScoreCard (failed target matches)
///   ObservationInstability -- placeholder (reserved for observation chain sensitivity)
///   EnvironmentInstability -- placeholder (reserved for environment variance)
///
/// All triggered conditions produce FalsifierRecords.
/// Non-convergence is a valid scientific result — always record it, never suppress it.
/// </summary>
public sealed class FalsifierEvaluator
{
    private int _idCounter;

    /// <summary>
    /// Evaluate all falsifier conditions and emit a FalsifierSummary.
    /// </summary>
    public FalsifierSummary Evaluate(
        string studyId,
        BranchRobustnessRecord? branchRecord,
        IReadOnlyList<ContinuumEstimateRecord>? convergenceRecords,
        IReadOnlyList<ConvergenceFailureRecord>? convergenceFailures,
        ConsistencyScoreCard? scoreCard,
        FalsificationPolicy policy,
        ProvenanceMeta provenance)
    {
        ArgumentNullException.ThrowIfNull(studyId);
        ArgumentNullException.ThrowIfNull(policy);
        ArgumentNullException.ThrowIfNull(provenance);

        _idCounter = 0;
        var falsifiers = new List<FalsifierRecord>();

        // 1. Branch fragility falsifiers
        if (branchRecord is not null)
            falsifiers.AddRange(EvaluateBranchFragility(branchRecord, policy, provenance));

        // 2. Non-convergence falsifiers
        if (convergenceFailures is not null)
            falsifiers.AddRange(EvaluateNonConvergence(convergenceFailures, policy, provenance));

        // 3. Quantitative mismatch falsifiers
        if (scoreCard is not null)
            falsifiers.AddRange(EvaluateQuantitativeMismatch(scoreCard, policy, provenance));

        // Compute summary counts
        var active = falsifiers.Where(f => f.Active).ToList();
        int fatalCount = active.Count(f => f.Severity == FalsifierSeverity.Fatal);
        int highCount = active.Count(f => f.Severity == FalsifierSeverity.High);

        return new FalsifierSummary
        {
            StudyId = studyId,
            Falsifiers = falsifiers,
            ActiveFatalCount = fatalCount,
            ActiveHighCount = highCount,
            TotalActiveCount = active.Count,
            Provenance = provenance,
        };
    }

    // ------------------------------------------------------------------
    // Private evaluators
    // ------------------------------------------------------------------

    private IEnumerable<FalsifierRecord> EvaluateBranchFragility(
        BranchRobustnessRecord branchRecord,
        FalsificationPolicy policy,
        ProvenanceMeta provenance)
    {
        foreach (var fragility in branchRecord.FragilityRecords.Values)
        {
            if (fragility.Classification != "fragile")
                continue;

            // Branch fragility above threshold is a High-severity falsifier
            // (not Fatal — fragility is an evidence problem, not a physics contradiction)
            yield return MakeFalsifier(
                type: FalsifierTypes.BranchFragility,
                severity: FalsifierSeverity.High,
                targetId: fragility.TargetQuantityId,
                branchId: branchRecord.StudyId,
                environmentId: null,
                triggerValue: fragility.FragilityScore,
                threshold: policy.BranchFragilityThreshold,
                description: $"Quantity '{fragility.TargetQuantityId}' is fragile across branch family " +
                             $"(fragilityScore={fragility.FragilityScore:G4}, threshold={policy.BranchFragilityThreshold}).",
                evidence: $"FragilityRecord targetQuantityId={fragility.TargetQuantityId} studyId={branchRecord.StudyId}",
                active: true,
                provenance: provenance);
        }
    }

    private IEnumerable<FalsifierRecord> EvaluateNonConvergence(
        IReadOnlyList<ConvergenceFailureRecord> convergenceFailures,
        FalsificationPolicy policy,
        ProvenanceMeta provenance)
    {
        foreach (var failure in convergenceFailures)
        {
            // Non-convergence is a valid result — always active, always recorded.
            // Insufficient-data is informational (we can't conclude non-convergence).
            bool isNonConvergent = failure.FailureType is "non-convergent" or "solver-failure" or "fit-failure";
            string severity = isNonConvergent
                ? policy.ConvergenceFailureSeverity
                : FalsifierSeverity.Informational;

            yield return MakeFalsifier(
                type: FalsifierTypes.NonConvergence,
                severity: severity,
                targetId: failure.QuantityId,
                branchId: "unknown",
                environmentId: null,
                triggerValue: null,
                threshold: null,
                description: $"Quantity '{failure.QuantityId}' convergence failure: {failure.FailureType}. {failure.Description}",
                evidence: $"ConvergenceFailureRecord quantityId={failure.QuantityId} failureType={failure.FailureType}",
                active: isNonConvergent,
                provenance: provenance);
        }
    }

    private IEnumerable<FalsifierRecord> EvaluateQuantitativeMismatch(
        ConsistencyScoreCard scoreCard,
        FalsificationPolicy policy,
        ProvenanceMeta provenance)
    {
        foreach (var match in scoreCard.Matches.Where(m => !m.Passed))
        {
            yield return MakeFalsifier(
                type: FalsifierTypes.QuantitativeMismatch,
                severity: policy.QuantitativeMismatchSeverity,
                targetId: match.ObservableId,
                branchId: "unknown",
                environmentId: null,
                triggerValue: match.Pull,
                threshold: null,
                description: $"Observable '{match.ObservableId}' failed target match '{match.TargetLabel}': " +
                             $"pull={match.Pull:G4} (computed={match.ComputedValue:G4}, target={match.TargetValue:G4}).",
                evidence: $"TargetMatchRecord observableId={match.ObservableId} targetLabel={match.TargetLabel}",
                active: true,
                provenance: provenance);
        }
    }

    private FalsifierRecord MakeFalsifier(
        string type, string severity, string targetId, string branchId,
        string? environmentId, double? triggerValue, double? threshold,
        string description, string evidence, bool active, ProvenanceMeta provenance)
    {
        _idCounter++;
        return new FalsifierRecord
        {
            FalsifierId = $"falsifier-{_idCounter:D4}",
            FalsifierType = type,
            Severity = severity,
            TargetId = targetId,
            BranchId = branchId,
            EnvironmentId = environmentId,
            TriggerValue = triggerValue,
            Threshold = threshold,
            Description = description,
            Evidence = evidence,
            Active = active,
            Provenance = provenance,
        };
    }
}
