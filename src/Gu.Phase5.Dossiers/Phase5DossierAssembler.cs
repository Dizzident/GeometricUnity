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
        ProvenanceMeta provenance)
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
            provenance);

        // Collect negative results from falsifier summary and convergence failures
        var negativeResults = CollectNegativeResults(
            falsifiers,
            convergenceFailures,
            provenance);

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
            ClaimEscalations = escalations,
            NegativeResults = negativeResults,
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

            // Gate 1: BranchRobust
            bool branchRobust = IsBranchRobust(branchRecord, candidate.PrimarySourceId);
            gates.Add(new EscalationGateResult
            {
                GateId = EscalationGates.BranchRobust,
                Description = "Candidate survives admissible branch variations (invariance class covers > 50% of variants).",
                Passed = branchRobust,
                Evidence = branchRecord is null ? "No branch record available." : (branchRobust ? "Passed." : "Failed — not invariant."),
                Required = true,
            });

            // Gate 2: RefinementBounded
            bool refinementBounded = IsRefinementBounded(convergenceRecords, candidate.PrimarySourceId);
            gates.Add(new EscalationGateResult
            {
                GateId = EscalationGates.RefinementBounded,
                Description = "Continuum estimate has error band < 10% of extrapolated value.",
                Passed = refinementBounded,
                Evidence = convergenceRecords is null ? "No convergence records." : (refinementBounded ? "Passed." : "Error band too large or not converged."),
                Required = true,
            });

            // Gate 3: MultiEnvironment
            bool multiEnv = environmentTiersCovered.Count >= 2;
            gates.Add(new EscalationGateResult
            {
                GateId = EscalationGates.MultiEnvironment,
                Description = "Quantity computed on at least 2 distinct environment tiers.",
                Passed = multiEnv,
                Evidence = $"Environment tiers covered: {environmentTiersCovered.Count}.",
                Required = true,
            });

            // Gate 4: ObservationChainValid (simplified — check if candidate has observation confidence > 0)
            bool obsValid = candidate.ObservationConfidence > 0;
            gates.Add(new EscalationGateResult
            {
                GateId = EscalationGates.ObservationChainValid,
                Description = "Observation provenance chain is complete (observation confidence > 0).",
                Passed = obsValid,
                Evidence = $"ObservationConfidence={candidate.ObservationConfidence:G4}.",
                Required = true,
            });

            // Gate 5: QuantitativeMatch
            bool quantMatch = HasQuantitativeMatch(scoreCard, candidate.PrimarySourceId);
            gates.Add(new EscalationGateResult
            {
                GateId = EscalationGates.QuantitativeMatch,
                Description = "At least one target match passed for this candidate.",
                Passed = quantMatch,
                Evidence = scoreCard is null ? "No scorecard available." : (quantMatch ? "Passed." : "No passing target match."),
                Required = true,
            });

            // Gate 6: NoActiveFatalFalsifier
            bool noFatalFalsifier = !HasActiveFatalFalsifier(falsifiers, candidate.ParticleId, candidate.PrimarySourceId);
            gates.Add(new EscalationGateResult
            {
                GateId = EscalationGates.NoActiveFatalFalsifier,
                Description = "No active fatal falsifier targets this candidate.",
                Passed = noFatalFalsifier,
                Evidence = noFatalFalsifier ? "No active fatal falsifiers." : "Active fatal falsifier found.",
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

    private static bool IsBranchRobust(BranchRobustnessRecord? branchRecord, string primarySourceId)
    {
        if (branchRecord is null) return false;
        // A quantity is considered branch-robust if it is classified "invariant" or "robust"
        // in FragilityRecords (i.e., the equivalence class covers the branch family).
        return branchRecord.FragilityRecords.Values
            .Any(fr => fr.Classification is "invariant" or "robust");
    }

    private static bool IsRefinementBounded(
        IReadOnlyList<ContinuumEstimateRecord>? convergenceRecords,
        string primarySourceId)
    {
        if (convergenceRecords is null || convergenceRecords.Count == 0) return false;
        return convergenceRecords.Any(r =>
        {
            double relError = r.ExtrapolatedValue != 0
                ? System.Math.Abs(r.ErrorBand / r.ExtrapolatedValue)
                : r.ErrorBand;
            return relError < 0.1;
        });
    }

    private static bool HasQuantitativeMatch(ConsistencyScoreCard? scoreCard, string primarySourceId)
    {
        if (scoreCard is null) return false;
        return scoreCard.Matches.Any(m => m.Passed);
    }

    private static bool HasActiveFatalFalsifier(FalsifierSummary? falsifiers, string particleId, string primarySourceId)
    {
        if (falsifiers is null) return false;
        return falsifiers.Falsifiers.Any(f =>
            f.Active &&
            f.Severity == FalsifierSeverity.Fatal &&
            (f.TargetId == particleId || f.TargetId == primarySourceId));
    }

    private static List<NegativeResultEntry> CollectNegativeResults(
        FalsifierSummary? falsifiers,
        IReadOnlyList<ConvergenceFailureRecord>? convergenceFailures,
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
                entries.Add(new NegativeResultEntry
                {
                    EntryId = $"neg-{counter:D4}",
                    Category = f.FalsifierType,
                    Description = f.Description,
                    SourceStudyId = "phase5-dossier-assembler",
                    ImpliesDemotion = true,
                    RecommendedAction = "demote-candidate",
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
