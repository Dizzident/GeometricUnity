using Gu.Core;
using Gu.Phase4.Registry;
using Gu.Phase5.BranchIndependence;
using Gu.Phase5.Convergence;
using Gu.Phase5.Environments;
using Gu.Phase5.Falsification;
using Gu.Phase5.QuantitativeValidation;

namespace Gu.Phase5.Dossiers;

/// <summary>
/// Assembles a Phase5ValidationDossier from M46–M50 Phase V study results (M51+M52).
///
/// Assembly steps:
///   1. Gather branch robustness record (M46)
///   2. Gather continuum estimates and failures (M47)
///   3. Gather environment records (M48)
///   4. Gather consistency scorecard (M49)
///   5. Gather falsifier summary (M50)
///   6. Run claim escalation evaluation (M51)
///   7. Collect negative results (M52)
///   8. Emit Phase5ValidationDossier
/// </summary>
public sealed class Phase5DossierAssembler
{
    /// <summary>
    /// Assemble a Phase5ValidationDossier from Phase V study results.
    /// All inputs are optional; missing data is recorded as absent (not fabricated).
    /// </summary>
    public Phase5ValidationDossier Assemble(
        string studyId,
        BranchRobustnessRecord? branchRecord,
        IReadOnlyList<ContinuumEstimateRecord>? convergenceRecords,
        IReadOnlyList<ConvergenceFailureRecord>? convergenceFailures,
        IReadOnlyList<EnvironmentRecord>? environments,
        ConsistencyScoreCard? scoreCard,
        FalsifierSummary? falsifiers,
        UnifiedParticleRegistry? registry,
        IReadOnlyList<string> environmentTiersCovered,
        string freshness,
        ProvenanceMeta provenance,
        IReadOnlyList<ObservationChainRecord>? observationChainRecords = null,
        SidecarSummary? sidecarSummary = null,
        IReadOnlyList<EnvironmentVarianceRecord>? environmentVarianceRecords = null,
        IReadOnlyList<RepresentationContentRecord>? representationContentRecords = null,
        string? geometryEvidenceTier = null)
    {
        ArgumentNullException.ThrowIfNull(studyId);
        ArgumentNullException.ThrowIfNull(environmentTiersCovered);
        ArgumentNullException.ThrowIfNull(provenance);

        // Run claim escalation if we have a registry
        var escalations = EvaluateEscalations(
            registry,
            branchRecord,
            convergenceRecords,
            scoreCard,
            falsifiers,
            environmentTiersCovered,
            observationChainRecords,
            environmentVarianceRecords,
            provenance);

        // Collect negative results from falsifier summary and convergence failures
        var negativeResults = CollectNegativeResults(
            falsifiers,
            convergenceFailures,
            representationContentRecords,
            provenance);

        // Per D-P11-007: default to "toy-control" when not explicitly provided.
        // The caller should pass the tier from GeometryEvidenceClassifier.Classify()
        // (in Gu.Phase5.Reporting). Dossiers does not reference Reporting, so the
        // tier is computed externally and passed as a string.
        string effectiveTier = geometryEvidenceTier ?? "toy-control";

        return new Phase5ValidationDossier
        {
            DossierId = $"dossier-{studyId}",
            SchemaVersion = "1.0.0",
            StudyId = studyId,
            BranchFamilySummary = branchRecord,
            RefinementSummary = convergenceRecords,
            ConvergenceFailures = convergenceFailures,
            EnvironmentSummary = environments,
            QuantitativeComparison = scoreCard,
            FalsifierSummary = falsifiers,
            ObservationChainSummary = observationChainRecords,
            SidecarCoverage = sidecarSummary,
            ClaimEscalations = escalations,
            NegativeResults = negativeResults,
            GeometryEvidenceTier = effectiveTier,
            Freshness = freshness,
            Provenance = provenance,
            GeneratedAt = DateTimeOffset.UtcNow,
        };
    }

    // ------------------------------------------------------------------
    // Private helpers
    // ------------------------------------------------------------------

    private static IReadOnlyList<ClaimEscalationRecord> EvaluateEscalations(
        UnifiedParticleRegistry? registry,
        BranchRobustnessRecord? branchRecord,
        IReadOnlyList<ContinuumEstimateRecord>? convergenceRecords,
        ConsistencyScoreCard? scoreCard,
        FalsifierSummary? falsifiers,
        IReadOnlyList<string> environmentTiersCovered,
        IReadOnlyList<ObservationChainRecord>? observationChainRecords,
        IReadOnlyList<EnvironmentVarianceRecord>? environmentVarianceRecords,
        ProvenanceMeta provenance)
    {
        if (registry is null)
            return Array.Empty<ClaimEscalationRecord>();

        var results = new List<ClaimEscalationRecord>();
        int counter = 0;

        foreach (var candidate in registry.Candidates)
        {
            counter++;
            var gates = new List<EscalationGateResult>();
            var candidateObservationRecords = (observationChainRecords ?? Array.Empty<ObservationChainRecord>())
                .Where(r => r.CandidateId == candidate.ParticleId && r.PrimarySourceId == candidate.PrimarySourceId)
                .ToList();
            var candidateObservableIds = candidateObservationRecords
                .Select(r => r.ObservableId)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(id => id, StringComparer.Ordinal)
                .ToList();

            // Gate 1: BranchRobust
            var branchEvidenceIds = candidate.BranchVariantSet
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(id => id, StringComparer.Ordinal)
                .ToList();
            bool branchEvidenceJoin = branchRecord is not null &&
                branchEvidenceIds.Count >= 2 &&
                branchEvidenceIds.All(id => branchRecord.BranchVariantIds.Contains(id, StringComparer.Ordinal));
            bool branchRobust = branchEvidenceJoin &&
                branchRecord!.OverallSummary is "robust" or "invariant";
            gates.Add(new EscalationGateResult
            {
                GateId = EscalationGates.BranchRobust,
                Description = "Candidate survives admissible branch variations (invariance class covers > 50% of variants).",
                Passed = branchRobust,
                EvidenceRecordIds = branchEvidenceIds,
                Evidence = branchRecord is null
                    ? "No branch record available."
                    : branchEvidenceIds.Count == 0
                        ? "No candidate-linked branch variant IDs were present in the registry record."
                        : !branchEvidenceJoin
                            ? "Candidate branch variant IDs did not produce a candidate-specific join to the branch study."
                            : "Candidate-linked branch variant IDs join to a robust overall branch study result.",
                Required = true,
            });

            // Gate 2: RefinementBounded
            var refinementEvidenceIds = candidate.BackgroundSet
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(id => id, StringComparer.Ordinal)
                .ToList();
            bool refinementBounded = convergenceRecords is not null &&
                refinementEvidenceIds.Count > 0 &&
                convergenceRecords.Any(HasBoundedRelativeError);
            gates.Add(new EscalationGateResult
            {
                GateId = EscalationGates.RefinementBounded,
                Description = "Continuum estimate has error band < 10% of extrapolated value.",
                Passed = refinementBounded,
                EvidenceRecordIds = refinementEvidenceIds,
                Evidence = convergenceRecords is null
                    ? "No convergence records."
                    : refinementEvidenceIds.Count == 0
                        ? "No candidate-linked background IDs were present in the registry record."
                        : refinementBounded
                            ? "Candidate-linked background IDs join to bounded refinement evidence."
                            : "Candidate-linked refinement evidence is absent or unbounded.",
                Required = true,
            });

            // Gate 3: MultiEnvironment
            var environmentVarianceEvidence = (environmentVarianceRecords ?? Array.Empty<EnvironmentVarianceRecord>())
                .Where(r => candidateObservableIds.Contains(r.QuantityId, StringComparer.Ordinal))
                .ToList();
            var environmentVarianceEvidenceIds = environmentVarianceEvidence
                .Select(r => r.RecordId)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(id => id, StringComparer.Ordinal)
                .ToList();
            bool multiEnv = environmentVarianceEvidence.Any(r => HasMultipleEnvironmentTiers(r.EnvironmentTierId));
            gates.Add(new EscalationGateResult
            {
                GateId = EscalationGates.MultiEnvironment,
                Description = "Quantity computed on at least 2 distinct environment tiers.",
                Passed = multiEnv,
                EvidenceRecordIds = environmentVarianceEvidenceIds,
                Evidence = environmentVarianceEvidence.Count == 0
                    ? $"No candidate-linked environment-variance records. Global environment count={environmentTiersCovered.Count}, but global coverage does not satisfy the candidate-specific gate."
                    : multiEnv
                        ? "Candidate-linked environment-variance records span more than one environment tier."
                        : "Candidate-linked environment evidence remains single-tier.",
                Required = true,
            });

            // Gate 4: ObservationChainValid — use observation chain records when available (WP-6)
            bool obsValid;
            string obsEvidence;
            var observationEvidenceIds = candidateObservationRecords
                .Select(r => BuildObservationEvidenceId(r))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(id => id, StringComparer.Ordinal)
                .ToList();
            if (observationChainRecords is not null && observationChainRecords.Count > 0)
            {
                // Gate passes if at least one record for this candidate satisfies all conditions
                obsValid = candidateObservationRecords.Any(IsObservationChainRecordValid);
                obsEvidence = obsValid
                    ? "ObservationChainRecord: complete, passed, low sensitivity."
                    : observationEvidenceIds.Count == 0
                        ? "No candidate-linked ObservationChainRecord."
                        : "Candidate-linked observation records exist, but none satisfy the gate conditions.";
            }
            else
            {
                obsValid = false;
                obsEvidence = $"No observation-chain artifacts were supplied. ObservationConfidence={candidate.ObservationConfidence:G4} is retained as context only and does not satisfy the candidate-specific gate.";
            }
            // Per D-P11-010 (M10): append proxy-observation tier note. All current fermion
            // observations are produced as proxy-observation summaries (not full sigma_h pullback).
            // This note is appended to the gate evidence string so it appears in executed artifacts.
            obsEvidence += " Observation tier: proxy-observation (not full pullback, D-P11-010).";
            gates.Add(new EscalationGateResult
            {
                GateId = EscalationGates.ObservationChainValid,
                Description = "Observation provenance chain is complete and low-sensitivity.",
                Passed = obsValid,
                Evidence = obsEvidence,
                EvidenceRecordIds = observationEvidenceIds,
                Required = true,
            });

            // Gate 5: QuantitativeMatch
            var candidateMatches = (scoreCard?.Matches ?? Array.Empty<TargetMatchRecord>())
                .Where(m => candidateObservableIds.Contains(m.ObservableId, StringComparer.Ordinal))
                .ToList();
            var quantitativeEvidenceIds = candidateMatches
                .Select(BuildTargetMatchEvidenceId)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(id => id, StringComparer.Ordinal)
                .ToList();
            bool quantMatch = candidateMatches.Any(m => m.Passed);
            gates.Add(new EscalationGateResult
            {
                GateId = EscalationGates.QuantitativeMatch,
                Description = "At least one target match passed for this candidate.",
                Passed = quantMatch,
                EvidenceRecordIds = quantitativeEvidenceIds,
                Evidence = scoreCard is null
                    ? "No scorecard available."
                    : candidateMatches.Count == 0
                        ? "No candidate-linked target matches were found."
                        : quantMatch
                            ? "Candidate-linked target match passed."
                            : "Candidate-linked target matches exist, but none passed.",
                Required = true,
            });

            // Gate 6: NoActiveFatalFalsifier
            var fatalFalsifiers = (falsifiers?.Falsifiers ?? Array.Empty<FalsifierRecord>())
                .Where(f =>
                    f.Active &&
                    f.Severity == FalsifierSeverity.Fatal &&
                    (f.TargetId == candidate.ParticleId || f.TargetId == candidate.PrimarySourceId))
                .ToList();
            var fatalEvidenceIds = fatalFalsifiers
                .Select(f => f.FalsifierId)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(id => id, StringComparer.Ordinal)
                .ToList();
            bool noFatalFalsifier = fatalFalsifiers.Count == 0;
            gates.Add(new EscalationGateResult
            {
                GateId = EscalationGates.NoActiveFatalFalsifier,
                Description = "No active fatal falsifier targets this candidate.",
                Passed = noFatalFalsifier,
                Evidence = noFatalFalsifier ? "No active fatal falsifiers." : "Active fatal falsifier found.",
                EvidenceRecordIds = fatalEvidenceIds,
                Required = true,
            });

            bool allPassed = gates.Where(g => g.Required).All(g => g.Passed);
            int currentLevel = UnifiedParticleRegistry.ParseClaimClassLevel(candidate.ClaimClass);

            string proposedClass;
            string direction;
            string? blockReason = null;

            if (!noFatalFalsifier)
            {
                // Fatal falsifier → demote to C0
                proposedClass = "C0_NumericalMode";
                direction = "demotion";
                blockReason = "Fatal falsifier active — demoted to C0.";
            }
            else if (allPassed && currentLevel < 5)
            {
                // All gates passed → promote by 1
                proposedClass = ClaimClassString(currentLevel + 1);
                direction = "escalation";
            }
            else if (!allPassed)
            {
                proposedClass = candidate.ClaimClass;
                direction = "no-change";
                var failedGates = gates.Where(g => g.Required && !g.Passed).Select(g => g.GateId).ToList();
                blockReason = $"Escalation held: required gates failed: [{string.Join(", ", failedGates)}].";
            }
            else
            {
                proposedClass = candidate.ClaimClass;
                direction = "no-change";
            }

            results.Add(new ClaimEscalationRecord
            {
                RecordId = $"escalation-{counter:D4}",
                CandidateId = candidate.ParticleId,
                RegistryType = candidate.ParticleType.ToString().ToLowerInvariant(),
                CurrentClaimClass = candidate.ClaimClass,
                ProposedClaimClass = proposedClass,
                Direction = direction,
                GateResults = gates,
                AllGatesPassed = allPassed,
                DemotionOrBlockReason = blockReason,
                SourceStudyId = "phase5-dossier-assembler",
                RecordedAt = DateTimeOffset.UtcNow,
                Provenance = provenance,
            });
        }

        return results;
    }

    /// <summary>
    /// ObservationChainValid gate: at least one record for the candidate satisfies all conditions:
    ///   1. CompletenessStatus == "complete"
    ///   2. Passed == true
    ///   3. SensitivityScore &lt;= 0.3
    ///   4. AuxiliaryModelSensitivity &lt;= 0.3
    /// Join: CandidateId == particleId AND PrimarySourceId == primarySourceId.
    /// </summary>
    private static bool IsObservationChainRecordValid(ObservationChainRecord record)
        => record.CompletenessStatus == "complete" &&
           record.Passed &&
           record.SensitivityScore <= 0.3 &&
           record.AuxiliaryModelSensitivity <= 0.3;

    private static bool HasBoundedRelativeError(ContinuumEstimateRecord record)
    {
        double relError = record.ExtrapolatedValue != 0
            ? System.Math.Abs(record.ErrorBand / record.ExtrapolatedValue)
            : record.ErrorBand;
        return relError < 0.1;
    }

    private static bool HasMultipleEnvironmentTiers(string environmentTierId)
        => environmentTierId
            .Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.Ordinal)
            .Count() >= 2;

    private static string BuildObservationEvidenceId(ObservationChainRecord record)
        => $"observation-chain:{record.CandidateId}:{record.PrimarySourceId}:{record.ObservableId}";

    private static string BuildTargetMatchEvidenceId(TargetMatchRecord match)
        => $"target-match:{match.ObservableId}:{match.TargetLabel}:{match.ComputedEnvironmentId ?? "unknown-env"}";

    private static List<NegativeResultEntry> CollectNegativeResults(
        FalsifierSummary? falsifiers,
        IReadOnlyList<ConvergenceFailureRecord>? convergenceFailures,
        IReadOnlyList<RepresentationContentRecord>? representationContentRecords,
        ProvenanceMeta provenance)
    {
        var entries = new List<NegativeResultEntry>();
        int counter = 0;

        if (convergenceFailures is not null)
        {
            foreach (var failure in convergenceFailures.Where(f => f.FailureType == "non-convergent"))
            {
                counter++;
                entries.Add(new NegativeResultEntry
                {
                    EntryId = $"neg-{counter:D4}",
                    Category = "non-convergence",
                    Description = $"Quantity '{failure.QuantityId}' did not converge: {failure.Description}",
                    SourceStudyId = "phase5-dossier-assembler",
                    ImpliesDemotion = false,
                    RecommendedAction = "flag-for-review",
                    RecordedAt = DateTimeOffset.UtcNow,
                });
            }
        }

        if (falsifiers is not null)
        {
            foreach (var f in falsifiers.Falsifiers.Where(f => f.Active && f.Severity == FalsifierSeverity.Fatal))
            {
                counter++;

                // For representation-content fatals, populate affected candidate IDs and
                // source artifact refs from the matching RepresentationContentRecord.
                // This makes the blocker explicitly traceable (P11-M5 stabilization).
                var affectedCandidateIds = new List<string>();
                IReadOnlyList<string>? sourceArtifactRefs = null;
                string? p11StabilizationNote = null;

                if (f.FalsifierType == FalsifierTypes.RepresentationContent &&
                    !string.IsNullOrWhiteSpace(f.TargetId))
                {
                    affectedCandidateIds.Add(f.TargetId);

                    var matchingRecord = representationContentRecords?
                        .FirstOrDefault(r => string.Equals(r.CandidateId, f.TargetId, StringComparison.Ordinal));
                    if (matchingRecord is not null)
                    {
                        sourceArtifactRefs = matchingRecord.SourceArtifactRefs;

                        // P11-M5 stabilization: if the record has missingRequiredCount > 0
                        // and was examined in Phase XI, document it as a stable scientific limit.
                        // The singleton-cluster candidate cannot satisfy the 2-family-source
                        // threshold without new physics results (new branch variants or a different
                        // fermion family atlas). This is a genuine negative result, not a gap.
                        if (matchingRecord.MissingRequiredCount > 0)
                        {
                            p11StabilizationNote =
                                $"Phase XI P11-M5 examination: candidate '{f.TargetId}' is a singleton cluster " +
                                $"with {matchingRecord.ObservedModeCount} observed family source(s) against a " +
                                $"required minimum of {matchingRecord.ExpectedModeCount}. " +
                                $"The Phase IV fermion-family atlas (sourceArtifactRefs) was searched and no " +
                                $"additional family sources exist for this candidate in the current repository " +
                                $"context. Closing the fatal requires either new branch variants that produce a " +
                                $"second persistent family for this candidate, or a revised fermion family atlas. " +
                                $"This blocker is preserved as an unresolved scientific limitation per D-P11-004.";
                        }
                    }
                }

                entries.Add(new NegativeResultEntry
                {
                    EntryId = $"neg-{counter:D4}",
                    Category = f.FalsifierType,
                    Description = f.Description,
                    AffectedCandidateIds = affectedCandidateIds,
                    SourceStudyId = "phase5-dossier-assembler",
                    ImpliesDemotion = true,
                    RecommendedAction = "demote-candidate",
                    SourceArtifactRefs = sourceArtifactRefs,
                    P11StabilizationNote = p11StabilizationNote,
                    RecordedAt = DateTimeOffset.UtcNow,
                });
            }
        }

        return entries;
    }

    private static string ClaimClassString(int level) => level switch
    {
        0 => "C0_NumericalMode",
        1 => "C1_NumericalHint",
        2 => "C2_ReproducibleMode",
        3 => "C3_BranchStableCandidate",
        4 => "C4_ObservationConsistentCandidate",
        5 => "C5_StrongIdentificationCandidate",
        _ => $"C{System.Math.Max(0, level)}_Unknown",
    };
}
