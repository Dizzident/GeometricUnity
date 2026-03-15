using Gu.Core;
using Gu.Phase5.BranchIndependence;
using Gu.Phase5.Convergence;
using Gu.Phase5.QuantitativeValidation;

namespace Gu.Phase5.Falsification;

/// <summary>
/// Evaluates falsifier conditions against Phase V study results (M50).
///
/// Falsifier sources:
///   BranchFragility        -- from BranchRobustnessRecord (fragility score > threshold)
///   NonConvergence         -- from ConvergenceFailureRecord (any non-convergent quantity)
///   QuantitativeMismatch   -- from ConsistencyScoreCard (failed target matches)
///   EnvironmentInstability -- from EnvironmentVarianceRecord (excessive tier variance)
///   RepresentationContent  -- from RepresentationContentRecord (mode count mismatch)
///   CouplingInconsistency  -- from CouplingConsistencyRecord (coupling spread > threshold)
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
        ProvenanceMeta provenance,
        IReadOnlyList<ObservationChainRecord>? observationRecords = null,
        IReadOnlyList<EnvironmentVarianceRecord>? environmentVarianceRecords = null,
        IReadOnlyList<RepresentationContentRecord>? representationContentRecords = null,
        IReadOnlyList<CouplingConsistencyRecord>? couplingConsistencyRecords = null,
        SidecarSummary? sidecarSummary = null)
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

        // 4. Observation instability falsifiers (WP-7)
        if (observationRecords is not null)
            falsifiers.AddRange(EvaluateObservationInstability(observationRecords, policy, provenance));

        // 5. Environment instability falsifiers
        if (environmentVarianceRecords is not null)
            falsifiers.AddRange(EvaluateEnvironmentInstability(environmentVarianceRecords, policy, provenance));

        // 6. Representation content falsifiers
        if (representationContentRecords is not null)
            falsifiers.AddRange(EvaluateRepresentationContent(representationContentRecords, policy, provenance));

        // 7. Coupling inconsistency falsifiers
        if (couplingConsistencyRecords is not null)
            falsifiers.AddRange(EvaluateCouplingInconsistency(couplingConsistencyRecords, policy, provenance));

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
            ObservationRecordCount = observationRecords?.Count ?? 0,
            EnvironmentRecordCount = environmentVarianceRecords?.Count ?? 0,
            RepresentationRecordCount = representationContentRecords?.Count ?? 0,
            CouplingRecordCount = couplingConsistencyRecords?.Count ?? 0,
            EvaluationCoverage = sidecarSummary?.Channels,
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

    private IEnumerable<FalsifierRecord> EvaluateObservationInstability(
        IReadOnlyList<ObservationChainRecord> records,
        FalsificationPolicy policy,
        ProvenanceMeta provenance)
    {
        // Trigger: SensitivityScore > ObservationInstabilityThreshold; severity: high
        foreach (var record in records.Where(r => r.SensitivityScore > policy.ObservationInstabilityThreshold))
        {
            yield return MakeFalsifier(
                type: FalsifierTypes.ObservationInstability,
                severity: FalsifierSeverity.High,
                targetId: record.CandidateId,
                branchId: "unknown",
                environmentId: null,
                triggerValue: record.SensitivityScore,
                threshold: policy.ObservationInstabilityThreshold,
                description: $"Observation chain for candidate '{record.CandidateId}' / observable " +
                             $"'{record.ObservableId}' has high sensitivity score " +
                             $"{record.SensitivityScore:G4} > threshold {policy.ObservationInstabilityThreshold}.",
                evidence: $"ObservationChainRecord candidateId={record.CandidateId} observableId={record.ObservableId}",
                active: true,
                provenance: provenance);
        }
    }

    private IEnumerable<FalsifierRecord> EvaluateEnvironmentInstability(
        IReadOnlyList<EnvironmentVarianceRecord> records,
        FalsificationPolicy policy,
        ProvenanceMeta provenance)
    {
        // Trigger: RelativeStdDev > EnvironmentInstabilityThreshold; severity: high
        foreach (var record in records.Where(r => r.RelativeStdDev > policy.EnvironmentInstabilityThreshold))
        {
            yield return MakeFalsifier(
                type: FalsifierTypes.EnvironmentInstability,
                severity: policy.EnvironmentInstabilitySeverity,
                targetId: record.QuantityId,
                branchId: "unknown",
                environmentId: record.EnvironmentTierId,
                triggerValue: record.RelativeStdDev,
                threshold: policy.EnvironmentInstabilityThreshold,
                description: $"Quantity '{record.QuantityId}' has excessive relative std-dev " +
                             $"{record.RelativeStdDev:G4} > threshold {policy.EnvironmentInstabilityThreshold} " +
                             $"in environment tier '{record.EnvironmentTierId}'.",
                evidence: $"EnvironmentVarianceRecord recordId={record.RecordId} quantityId={record.QuantityId}",
                active: true,
                provenance: provenance);
        }
    }

    private IEnumerable<FalsifierRecord> EvaluateRepresentationContent(
        IReadOnlyList<RepresentationContentRecord> records,
        FalsificationPolicy policy,
        ProvenanceMeta provenance)
    {
        foreach (var record in records)
        {
            // Trigger 1: MissingRequiredCount > 0 → Fatal
            if (record.MissingRequiredCount > 0)
            {
                yield return MakeFalsifier(
                    type: FalsifierTypes.RepresentationContent,
                    severity: FalsifierSeverity.Fatal,
                    targetId: record.CandidateId,
                    branchId: "unknown",
                    environmentId: null,
                    triggerValue: record.MissingRequiredCount,
                    threshold: 0,
                    description: record.InconsistencyDescription ??
                                 $"Candidate '{record.CandidateId}' is missing {record.MissingRequiredCount} " +
                                 $"required representation content item(s). Expected modes: {record.ExpectedModeCount}, " +
                                 $"observed: {record.ObservedModeCount}.",
                    evidence: $"RepresentationContentRecord recordId={record.RecordId} candidateId={record.CandidateId} " +
                              $"missingRequiredCount={record.MissingRequiredCount}",
                    active: true,
                    provenance: provenance);
            }
            // Trigger 2: StructuralMismatchScore > RepresentationContentThreshold → High
            else if (record.StructuralMismatchScore > policy.RepresentationContentThreshold)
            {
                yield return MakeFalsifier(
                    type: FalsifierTypes.RepresentationContent,
                    severity: FalsifierSeverity.High,
                    targetId: record.CandidateId,
                    branchId: "unknown",
                    environmentId: null,
                    triggerValue: record.StructuralMismatchScore,
                    threshold: policy.RepresentationContentThreshold,
                    description: record.InconsistencyDescription ??
                                 $"Candidate '{record.CandidateId}' structural mismatch score " +
                                 $"{record.StructuralMismatchScore:G4} > threshold {policy.RepresentationContentThreshold}.",
                    evidence: $"RepresentationContentRecord recordId={record.RecordId} candidateId={record.CandidateId} " +
                              $"structuralMismatchScore={record.StructuralMismatchScore:G4}",
                    active: true,
                    provenance: provenance);
            }
        }
    }

    private IEnumerable<FalsifierRecord> EvaluateCouplingInconsistency(
        IReadOnlyList<CouplingConsistencyRecord> records,
        FalsificationPolicy policy,
        ProvenanceMeta provenance)
    {
        foreach (var record in records.Where(r => r.RelativeSpread > policy.CouplingInconsistencyThreshold))
        {
            yield return MakeFalsifier(
                type: FalsifierTypes.CouplingInconsistency,
                severity: policy.CouplingInconsistencySeverity,
                targetId: record.CandidateId,
                branchId: "unknown",
                environmentId: null,
                triggerValue: record.RelativeSpread,
                threshold: policy.CouplingInconsistencyThreshold,
                description: $"Candidate '{record.CandidateId}' coupling '{record.CouplingType}' " +
                             $"has excessive spread {record.RelativeSpread:G4} > threshold {policy.CouplingInconsistencyThreshold} " +
                             $"across branch variants.",
                evidence: $"CouplingConsistencyRecord recordId={record.RecordId} candidateId={record.CandidateId}",
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
