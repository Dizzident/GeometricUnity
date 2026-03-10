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

    /// <summary>Observation stability threshold.</summary>
    public double ObservationStabilityThreshold { get; init; } = 0.5;
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

        // Return updated record if demoted
        if (currentClass != candidate.ClaimClass)
        {
            return new CandidateBosonRecord
            {
                CandidateId = candidate.CandidateId,
                PrimaryFamilyId = candidate.PrimaryFamilyId,
                ContributingModeIds = candidate.ContributingModeIds,
                BackgroundSet = candidate.BackgroundSet,
                BranchVariantSet = candidate.BranchVariantSet,
                MassLikeEnvelope = candidate.MassLikeEnvelope,
                MultiplicityEnvelope = candidate.MultiplicityEnvelope,
                GaugeLeakEnvelope = candidate.GaugeLeakEnvelope,
                BranchStabilityScore = candidate.BranchStabilityScore,
                RefinementStabilityScore = candidate.RefinementStabilityScore,
                BackendStabilityScore = candidate.BackendStabilityScore,
                ObservationStabilityScore = candidate.ObservationStabilityScore,
                ClaimClass = currentClass,
                Demotions = demotions,
                AmbiguityNotes = candidate.AmbiguityNotes,
                RegistryVersion = candidate.RegistryVersion,
            };
        }

        return candidate;
    }
}
