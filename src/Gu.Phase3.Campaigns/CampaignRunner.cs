using Gu.Core;
using Gu.Phase3.Registry;

namespace Gu.Phase3.Campaigns;

/// <summary>
/// Executes boson comparison campaigns by comparing registry candidates
/// against target profiles.
///
/// IMPORTANT: The runner never forces unique match. Multiple candidates may
/// be Compatible with the same target, and a single candidate may be
/// Compatible with multiple targets.
/// </summary>
public sealed class CampaignRunner
{
    /// <summary>
    /// Execute a campaign against a boson registry.
    /// </summary>
    public CampaignResult Run(BosonRegistry registry, BosonComparisonCampaign campaign, ProvenanceMeta provenance)
    {
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentNullException.ThrowIfNull(campaign);
        ArgumentNullException.ThrowIfNull(provenance);

        var candidates = registry.QueryByClaimClass(campaign.MinimumClaimClass);
        var comparisons = new List<ComparisonResult>();

        foreach (var candidate in candidates)
        {
            foreach (var target in campaign.TargetProfiles)
            {
                var result = Compare(candidate, target);
                comparisons.Add(result);
            }
        }

        int compatible = comparisons.Count(c => c.Outcome == ComparisonOutcome.Compatible);
        int incompatible = comparisons.Count(c => c.Outcome == ComparisonOutcome.Incompatible);
        int underdetermined = comparisons.Count(c => c.Outcome == ComparisonOutcome.Underdetermined);
        int insufficient = comparisons.Count(c => c.Outcome == ComparisonOutcome.InsufficientEvidence);

        return new CampaignResult
        {
            CampaignId = campaign.CampaignId,
            Comparisons = comparisons,
            CandidatesEvaluated = candidates.Count,
            TargetsCount = campaign.TargetProfiles.Count,
            CompatibleCount = compatible,
            IncompatibleCount = incompatible,
            UnderdeterminedCount = underdetermined,
            InsufficientEvidenceCount = insufficient,
            Summary = $"Campaign '{campaign.CampaignId}': {candidates.Count} candidates x {campaign.TargetProfiles.Count} targets = {comparisons.Count} comparisons. " +
                      $"Compatible={compatible}, Incompatible={incompatible}, Underdetermined={underdetermined}, InsufficientEvidence={insufficient}.",
            Provenance = provenance,
        };
    }

    /// <summary>
    /// Compare a single candidate against a single target profile.
    /// </summary>
    internal static ComparisonResult Compare(CandidateBosonRecord candidate, TargetProfile target)
    {
        var tolerances = target.Tolerances;

        // Check if we have sufficient evidence
        bool hasSufficientStability =
            candidate.BranchStabilityScore >= tolerances.MinBranchStability &&
            candidate.RefinementStabilityScore >= tolerances.MinRefinementStability;

        if (!hasSufficientStability)
        {
            return new ComparisonResult
            {
                CandidateId = candidate.CandidateId,
                ProfileId = target.ProfileId,
                Outcome = ComparisonOutcome.InsufficientEvidence,
                MassScore = 0.0,
                MultiplicityScore = 0.0,
                GaugeLeakScore = MeanGaugeLeak(candidate),
                Details = $"Insufficient stability evidence: branch={candidate.BranchStabilityScore:F3}, refinement={candidate.RefinementStabilityScore:F3}.",
            };
        }

        // Compute mass compatibility
        double massScore = ComputeMassScore(candidate, target);

        // Compute multiplicity compatibility
        double multiplicityScore = ComputeMultiplicityScore(candidate, target);

        // Gauge leak check
        double meanLeak = MeanGaugeLeak(candidate);
        bool leakOk = meanLeak <= tolerances.MaxGaugeLeakForCompatibility;

        // Determine outcome
        bool massOk = massScore >= 0.5;
        bool multOk = multiplicityScore >= 0.5;

        ComparisonOutcome outcome;
        string details;

        if (massOk && multOk && leakOk)
        {
            outcome = ComparisonOutcome.Compatible;
            details = $"Mass score={massScore:F3}, multiplicity score={multiplicityScore:F3}, gauge leak={meanLeak:F4}. All within tolerances.";
        }
        else if (!massOk && !multOk)
        {
            outcome = ComparisonOutcome.Incompatible;
            details = $"Mass score={massScore:F3}, multiplicity score={multiplicityScore:F3}, gauge leak={meanLeak:F4}. Both mass and multiplicity outside tolerances.";
        }
        else if (!leakOk)
        {
            outcome = ComparisonOutcome.Underdetermined;
            details = $"Mass score={massScore:F3}, multiplicity score={multiplicityScore:F3}, gauge leak={meanLeak:F4}. Gauge leak too high for confident determination.";
        }
        else
        {
            outcome = ComparisonOutcome.Underdetermined;
            details = $"Mass score={massScore:F3}, multiplicity score={multiplicityScore:F3}, gauge leak={meanLeak:F4}. Partial match: ambiguous determination.";
        }

        return new ComparisonResult
        {
            CandidateId = candidate.CandidateId,
            ProfileId = target.ProfileId,
            Outcome = outcome,
            MassScore = massScore,
            MultiplicityScore = multiplicityScore,
            GaugeLeakScore = meanLeak,
            Details = details,
        };
    }

    private static double ComputeMassScore(CandidateBosonRecord candidate, TargetProfile target)
    {
        if (candidate.MassLikeEnvelope.Length < 2 || target.ExpectedMassRange.Length < 2)
            return 0.0;

        double candidateMean = candidate.MassLikeEnvelope[1];
        double targetMin = target.ExpectedMassRange[0];
        double targetMax = target.ExpectedMassRange[1];
        double tolerance = target.Tolerances.MassTolerance;

        // Compute overlap score: 1.0 if candidate mean is within range, decreasing outside
        double rangeCenter = (targetMin + targetMax) / 2.0;
        double rangeHalfWidth = (targetMax - targetMin) / 2.0;
        double effectiveHalfWidth = rangeHalfWidth + tolerance * System.Math.Max(1.0, rangeCenter);

        if (effectiveHalfWidth <= 0)
            return candidateMean == rangeCenter ? 1.0 : 0.0;

        double distance = System.Math.Abs(candidateMean - rangeCenter);
        double score = System.Math.Max(0.0, 1.0 - distance / effectiveHalfWidth);
        return score;
    }

    private static double ComputeMultiplicityScore(CandidateBosonRecord candidate, TargetProfile target)
    {
        if (candidate.MultiplicityEnvelope.Length < 2)
            return 0.0;

        int candidateMean = candidate.MultiplicityEnvelope[1];
        int expected = target.ExpectedMultiplicity;
        int tolerance = target.Tolerances.MultiplicityTolerance;

        int diff = System.Math.Abs(candidateMean - expected);
        if (diff <= tolerance)
            return 1.0;

        // Linear decay beyond tolerance
        return System.Math.Max(0.0, 1.0 - (double)(diff - tolerance) / System.Math.Max(1, expected));
    }

    private static double MeanGaugeLeak(CandidateBosonRecord candidate)
    {
        if (candidate.GaugeLeakEnvelope.Length >= 2)
            return candidate.GaugeLeakEnvelope[1];
        if (candidate.GaugeLeakEnvelope.Length > 0)
            return candidate.GaugeLeakEnvelope[0];
        return 0.0;
    }
}
