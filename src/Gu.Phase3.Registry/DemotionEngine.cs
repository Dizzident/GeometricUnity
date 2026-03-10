namespace Gu.Phase3.Registry;

/// <summary>
/// Configuration for demotion thresholds.
/// </summary>
public sealed class DemotionConfig
{
    /// <summary>Gauge leak threshold. Modes with leak > threshold get demoted.</summary>
    public double GaugeLeakThreshold { get; init; } = 0.3;

    /// <summary>Refinement stability threshold. Scores below this trigger demotion.</summary>
    public double RefinementStabilityThreshold { get; init; } = 0.5;

    /// <summary>Branch stability threshold.</summary>
    public double BranchStabilityThreshold { get; init; } = 0.5;

    /// <summary>Backend stability threshold.</summary>
    public double BackendStabilityThreshold { get; init; } = 0.8;

    /// <summary>Observation instability threshold. Scores below this trigger demotion by 1.</summary>
    public double ObservationInstabilityThreshold { get; init; } = 0.4;

    /// <summary>Ambiguous matching threshold. AmbiguityCount above this triggers demotion by 1.</summary>
    public int AmbiguousMatchingThreshold { get; init; } = 2;
}

/// <summary>
/// Applies demotion rules to candidate boson records.
///
/// Demotion rules:
/// - High gauge leak -> demote to C0
/// - Refinement fragility -> demote by 1 level (min C0)
/// - Branch fragility -> demote by 1 level (min C0)
/// - Backend fragility -> no high claim class allowed (max C1)
/// - Observation instability -> demote by 1 level
/// - Ambiguous matching -> demote by 1 level
/// </summary>
public sealed class DemotionEngine
{
    private readonly DemotionConfig _config;

    public DemotionEngine(DemotionConfig? config = null)
    {
        _config = config ?? new DemotionConfig();
    }

    /// <summary>
    /// Apply all demotion rules to a candidate and return a (possibly demoted)
    /// version with demotion records.
    /// </summary>
    public CandidateBosonRecord ApplyDemotions(CandidateBosonRecord candidate)
    {
        ArgumentNullException.ThrowIfNull(candidate);

        var demotions = new List<BosonDemotionRecord>(candidate.Demotions);
        var currentClass = candidate.ClaimClass;

        // Rule 1: High gauge leak -> demote to C0
        double meanLeak = candidate.GaugeLeakEnvelope.Length >= 2
            ? candidate.GaugeLeakEnvelope[1]
            : candidate.GaugeLeakEnvelope.Length > 0 ? candidate.GaugeLeakEnvelope[0] : 0;

        if (meanLeak > _config.GaugeLeakThreshold && currentClass > BosonClaimClass.C0_NumericalMode)
        {
            demotions.Add(new BosonDemotionRecord
            {
                CandidateId = candidate.CandidateId,
                Reason = DemotionReason.GaugeLeak,
                PreviousClaimClass = currentClass,
                DemotedClaimClass = BosonClaimClass.C0_NumericalMode,
                Details = $"Mean gauge leak {meanLeak:F4} > threshold {_config.GaugeLeakThreshold:F4}",
                TriggerValue = meanLeak,
                Threshold = _config.GaugeLeakThreshold,
            });
            currentClass = BosonClaimClass.C0_NumericalMode;
        }

        // Rule 2: Refinement fragility -> demote by 1
        if (candidate.RefinementStabilityScore < _config.RefinementStabilityThreshold &&
            currentClass > BosonClaimClass.C0_NumericalMode)
        {
            var newClass = (BosonClaimClass)System.Math.Max(0, (int)currentClass - 1);
            demotions.Add(new BosonDemotionRecord
            {
                CandidateId = candidate.CandidateId,
                Reason = DemotionReason.RefinementFragility,
                PreviousClaimClass = currentClass,
                DemotedClaimClass = newClass,
                Details = $"Refinement stability {candidate.RefinementStabilityScore:F4} < threshold {_config.RefinementStabilityThreshold:F4}",
                TriggerValue = candidate.RefinementStabilityScore,
                Threshold = _config.RefinementStabilityThreshold,
            });
            currentClass = newClass;
        }

        // Rule 3: Branch fragility -> demote by 1
        if (candidate.BranchStabilityScore < _config.BranchStabilityThreshold &&
            currentClass > BosonClaimClass.C0_NumericalMode)
        {
            var newClass = (BosonClaimClass)System.Math.Max(0, (int)currentClass - 1);
            demotions.Add(new BosonDemotionRecord
            {
                CandidateId = candidate.CandidateId,
                Reason = DemotionReason.BranchFragility,
                PreviousClaimClass = currentClass,
                DemotedClaimClass = newClass,
                Details = $"Branch stability {candidate.BranchStabilityScore:F4} < threshold {_config.BranchStabilityThreshold:F4}",
                TriggerValue = candidate.BranchStabilityScore,
                Threshold = _config.BranchStabilityThreshold,
            });
            currentClass = newClass;
        }

        // Rule 4: Backend fragility -> cap at C1
        if (candidate.BackendStabilityScore < _config.BackendStabilityThreshold &&
            currentClass > BosonClaimClass.C1_LocalPersistentMode)
        {
            var newClass = BosonClaimClass.C1_LocalPersistentMode;
            demotions.Add(new BosonDemotionRecord
            {
                CandidateId = candidate.CandidateId,
                Reason = DemotionReason.BackendFragility,
                PreviousClaimClass = currentClass,
                DemotedClaimClass = newClass,
                Details = $"Backend stability {candidate.BackendStabilityScore:F4} < threshold {_config.BackendStabilityThreshold:F4}. High claim class not allowed.",
                TriggerValue = candidate.BackendStabilityScore,
                Threshold = _config.BackendStabilityThreshold,
            });
            currentClass = newClass;
        }

        // Rule 5: Unverified GPU -> cap at C1
        // Enforces IA-5: no high-claim candidate depends on unverified GPU-only logic.
        if (candidate.ComputedWithUnverifiedGpu &&
            currentClass > BosonClaimClass.C1_LocalPersistentMode)
        {
            var newClass = BosonClaimClass.C1_LocalPersistentMode;
            demotions.Add(new BosonDemotionRecord
            {
                CandidateId = candidate.CandidateId,
                Reason = DemotionReason.BackendFragility,
                PreviousClaimClass = currentClass,
                DemotedClaimClass = newClass,
                Details = "Contributing modes computed with unverified GPU backend. Claim class capped at C1 per IA-5.",
            });
            currentClass = newClass;
        }

        // Rule 6: Observation instability -> demote by 1
        if (candidate.ObservationStabilityScore < _config.ObservationInstabilityThreshold &&
            currentClass > BosonClaimClass.C0_NumericalMode)
        {
            var newClass = (BosonClaimClass)System.Math.Max(0, (int)currentClass - 1);
            demotions.Add(new BosonDemotionRecord
            {
                CandidateId = candidate.CandidateId,
                Reason = DemotionReason.ObservationInstability,
                PreviousClaimClass = currentClass,
                DemotedClaimClass = newClass,
                Details = $"Observation stability {candidate.ObservationStabilityScore:F4} < threshold {_config.ObservationInstabilityThreshold:F4}",
                TriggerValue = candidate.ObservationStabilityScore,
                Threshold = _config.ObservationInstabilityThreshold,
            });
            currentClass = newClass;
        }

        // Rule 7: Ambiguous matching -> demote by 1
        if (candidate.AmbiguityCount > _config.AmbiguousMatchingThreshold &&
            currentClass > BosonClaimClass.C0_NumericalMode)
        {
            var newClass = (BosonClaimClass)System.Math.Max(0, (int)currentClass - 1);
            demotions.Add(new BosonDemotionRecord
            {
                CandidateId = candidate.CandidateId,
                Reason = DemotionReason.AmbiguousMatching,
                PreviousClaimClass = currentClass,
                DemotedClaimClass = newClass,
                Details = $"Ambiguity count {candidate.AmbiguityCount} > threshold {_config.AmbiguousMatchingThreshold}",
                TriggerValue = candidate.AmbiguityCount,
                Threshold = _config.AmbiguousMatchingThreshold,
            });
            currentClass = newClass;
        }

        // Return updated record if demoted
        if (currentClass != candidate.ClaimClass)
        {
            return CopyWithDemotion(candidate, currentClass, demotions);
        }

        return candidate;
    }

    /// <summary>
    /// Apply comparison mismatch demotion externally (called by campaign runner).
    /// If all comparison results are incompatible, cap claim class at C1.
    /// </summary>
    public CandidateBosonRecord ApplyComparisonMismatch(
        CandidateBosonRecord record,
        bool allResultsIncompatible)
    {
        ArgumentNullException.ThrowIfNull(record);

        if (!allResultsIncompatible || record.ClaimClass <= BosonClaimClass.C1_LocalPersistentMode)
            return record;

        var newClass = BosonClaimClass.C1_LocalPersistentMode;
        var demotions = new List<BosonDemotionRecord>(record.Demotions)
        {
            new BosonDemotionRecord
            {
                CandidateId = record.CandidateId,
                Reason = DemotionReason.ComparisonMismatch,
                PreviousClaimClass = record.ClaimClass,
                DemotedClaimClass = newClass,
                Details = "All comparison campaign results incompatible. Claim class capped at C1.",
            },
        };

        return CopyWithDemotion(record, newClass, demotions);
    }

    private static CandidateBosonRecord CopyWithDemotion(
        CandidateBosonRecord source,
        BosonClaimClass newClass,
        List<BosonDemotionRecord> demotions)
    {
        return new CandidateBosonRecord
        {
            CandidateId = source.CandidateId,
            PrimaryFamilyId = source.PrimaryFamilyId,
            ContributingModeIds = source.ContributingModeIds,
            BackgroundSet = source.BackgroundSet,
            BranchVariantSet = source.BranchVariantSet,
            MassLikeEnvelope = source.MassLikeEnvelope,
            MultiplicityEnvelope = source.MultiplicityEnvelope,
            GaugeLeakEnvelope = source.GaugeLeakEnvelope,
            PolarizationEnvelope = source.PolarizationEnvelope,
            SymmetryEnvelope = source.SymmetryEnvelope,
            InteractionProxyEnvelope = source.InteractionProxyEnvelope,
            BranchStabilityScore = source.BranchStabilityScore,
            RefinementStabilityScore = source.RefinementStabilityScore,
            BackendStabilityScore = source.BackendStabilityScore,
            ObservationStabilityScore = source.ObservationStabilityScore,
            ComputedWithUnverifiedGpu = source.ComputedWithUnverifiedGpu,
            ClaimClass = newClass,
            Demotions = demotions,
            AmbiguityCount = source.AmbiguityCount,
            AmbiguityNotes = source.AmbiguityNotes,
            RegistryVersion = source.RegistryVersion,
        };
    }
}
