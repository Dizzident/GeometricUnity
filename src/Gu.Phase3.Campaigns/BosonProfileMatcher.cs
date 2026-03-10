using Gu.Phase3.Registry;

namespace Gu.Phase3.Campaigns;

/// <summary>
/// Compares candidate boson records against internal target profiles (BC1)
/// and external analogy descriptors (BC2).
///
/// Per Section 7.10: external comparison may produce compatibility,
/// incompatibility, underdetermination, or not enough evidence.
/// Never forces a unique match.
/// </summary>
public sealed class BosonProfileMatcher
{
    /// <summary>
    /// Compare a candidate against an internal target profile (BC1).
    /// </summary>
    public BosonComparisonResult CompareToProfile(
        CandidateBosonRecord candidate, BosonTargetProfile profile)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        ArgumentNullException.ThrowIfNull(profile);

        var notes = new List<string>();
        double score = 1.0;
        int checks = 0;
        int passes = 0;

        // Check mass range
        if (profile.MassRange != null && profile.MassRange.Length >= 2)
        {
            checks++;
            double meanMass = candidate.MassLikeEnvelope.Length >= 2
                ? candidate.MassLikeEnvelope[1] : candidate.MassLikeEnvelope[0];

            if (meanMass >= profile.MassRange[0] && meanMass <= profile.MassRange[1])
            {
                passes++;
                notes.Add($"Mass {meanMass:F4} within target range [{profile.MassRange[0]:F4}, {profile.MassRange[1]:F4}]");
            }
            else
            {
                score *= 0.2;
                notes.Add($"Mass {meanMass:F4} outside target range [{profile.MassRange[0]:F4}, {profile.MassRange[1]:F4}]");
            }
        }

        // Check multiplicity
        if (profile.ExpectedMultiplicity.HasValue)
        {
            checks++;
            int meanMult = candidate.MultiplicityEnvelope.Length >= 2
                ? candidate.MultiplicityEnvelope[1] : candidate.MultiplicityEnvelope[0];

            if (meanMult == profile.ExpectedMultiplicity.Value)
            {
                passes++;
                notes.Add($"Multiplicity {meanMult} matches expected {profile.ExpectedMultiplicity.Value}");
            }
            else
            {
                score *= 0.3;
                notes.Add($"Multiplicity {meanMult} does not match expected {profile.ExpectedMultiplicity.Value}");
            }
        }

        // Check gauge leak
        if (profile.MaxGaugeLeak.HasValue)
        {
            checks++;
            double meanLeak = candidate.GaugeLeakEnvelope.Length >= 2
                ? candidate.GaugeLeakEnvelope[1] : candidate.GaugeLeakEnvelope[0];

            if (meanLeak <= profile.MaxGaugeLeak.Value)
            {
                passes++;
                notes.Add($"Gauge leak {meanLeak:F4} within threshold {profile.MaxGaugeLeak.Value:F4}");
            }
            else
            {
                score *= 0.1;
                notes.Add($"Gauge leak {meanLeak:F4} exceeds threshold {profile.MaxGaugeLeak.Value:F4}");
            }
        }

        // Check branch stability
        if (profile.MinBranchStability.HasValue)
        {
            checks++;
            if (candidate.BranchStabilityScore >= profile.MinBranchStability.Value)
            {
                passes++;
                notes.Add($"Branch stability {candidate.BranchStabilityScore:F4} meets minimum {profile.MinBranchStability.Value:F4}");
            }
            else
            {
                score *= 0.3;
                notes.Add($"Branch stability {candidate.BranchStabilityScore:F4} below minimum {profile.MinBranchStability.Value:F4}");
            }
        }

        // Check claim class
        if (profile.MinClaimClass.HasValue)
        {
            checks++;
            if (candidate.ClaimClass >= profile.MinClaimClass.Value)
            {
                passes++;
                notes.Add($"Claim class {candidate.ClaimClass} meets minimum {profile.MinClaimClass.Value}");
            }
            else
            {
                score *= 0.2;
                notes.Add($"Claim class {candidate.ClaimClass} below minimum {profile.MinClaimClass.Value}");
            }
        }

        var verdict = DetermineVerdict(checks, passes, score);

        return new BosonComparisonResult
        {
            CandidateId = candidate.CandidateId,
            TargetId = profile.ProfileId,
            Verdict = verdict,
            CompatibilityScore = score,
            Notes = notes,
            CandidateClaimClass = candidate.ClaimClass,
        };
    }

    /// <summary>
    /// Compare a candidate against an external analogy descriptor (BC2).
    /// </summary>
    public BosonComparisonResult CompareToExternal(
        CandidateBosonRecord candidate, ExternalAnalogyDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        ArgumentNullException.ThrowIfNull(descriptor);

        var notes = new List<string>();
        double score = 1.0;
        int checks = 0;
        int passes = 0;

        // Check mass compatibility with uncertainty
        checks++;
        double meanMass = candidate.MassLikeEnvelope.Length >= 2
            ? candidate.MassLikeEnvelope[1] : candidate.MassLikeEnvelope[0];
        double massDiff = System.Math.Abs(meanMass - descriptor.ReferenceMass);
        double tolerance = descriptor.MassUncertainty > 0 ? descriptor.MassUncertainty : 0.1;

        if (massDiff <= 2.0 * tolerance)
        {
            passes++;
            double massScore = 1.0 - (massDiff / (2.0 * tolerance));
            score *= System.Math.Max(0.1, massScore);
            notes.Add($"Mass {meanMass:F4} within 2-sigma of reference {descriptor.ReferenceMass:F4} +/- {tolerance:F4}");
        }
        else
        {
            score *= 0.05;
            notes.Add($"Mass {meanMass:F4} outside 2-sigma of reference {descriptor.ReferenceMass:F4} +/- {tolerance:F4}");
        }

        // Check multiplicity
        checks++;
        int meanMult = candidate.MultiplicityEnvelope.Length >= 2
            ? candidate.MultiplicityEnvelope[1] : candidate.MultiplicityEnvelope[0];
        if (meanMult == descriptor.ExpectedMultiplicity)
        {
            passes++;
            notes.Add($"Multiplicity {meanMult} matches expected {descriptor.ExpectedMultiplicity}");
        }
        else
        {
            score *= 0.3;
            notes.Add($"Multiplicity {meanMult} does not match expected {descriptor.ExpectedMultiplicity}");
        }

        // Check gauge leak
        checks++;
        double meanLeak = candidate.GaugeLeakEnvelope.Length >= 2
            ? candidate.GaugeLeakEnvelope[1] : candidate.GaugeLeakEnvelope[0];
        if (meanLeak <= descriptor.MaxGaugeLeak)
        {
            passes++;
            notes.Add($"Gauge leak {meanLeak:F4} within threshold {descriptor.MaxGaugeLeak:F4}");
        }
        else
        {
            score *= 0.1;
            notes.Add($"Gauge leak {meanLeak:F4} exceeds threshold {descriptor.MaxGaugeLeak:F4}");
        }

        var verdict = DetermineVerdict(checks, passes, score);

        return new BosonComparisonResult
        {
            CandidateId = candidate.CandidateId,
            TargetId = descriptor.DescriptorId,
            Verdict = verdict,
            CompatibilityScore = score,
            Notes = notes,
            CandidateClaimClass = candidate.ClaimClass,
        };
    }

    private static ComparisonVerdict DetermineVerdict(int checks, int passes, double score)
    {
        if (checks == 0)
            return ComparisonVerdict.InsufficientEvidence;

        if (passes == checks && score > 0.5)
            return ComparisonVerdict.Compatible;

        if (score <= 0.2)
            return ComparisonVerdict.Incompatible;

        double passRatio = (double)passes / checks;
        if (passRatio < 0.5)
            return ComparisonVerdict.Incompatible;

        return ComparisonVerdict.Underdetermined;
    }
}
