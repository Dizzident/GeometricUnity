namespace Gu.Phase5.QuantitativeValidation;

/// <summary>
/// Compares a computed observable to an external target (M49).
///
/// Pull statistic (physicist-confirmed):
///   gaussian:            pull = |computed - target| / sqrt(sigma_computed^2 + sigma_target^2)
///   gaussian-asymmetric: choose lower or upper sigma based on sign of residual
///   student-t:           pull = |computed - target| / sigma_target, normalized by Student-t DoF
///
/// If computed uncertainty is unestimated (-1), uses sigma_target only in denominator
/// (unless policy.RequireFullUncertainty = true, in which case the match fails).
/// </summary>
public static class TargetMatcher
{
    /// <summary>
    /// Compute a TargetMatchRecord for the given computed observable and external target.
    /// Dispatches on target.DistributionModel.
    /// </summary>
    public static TargetMatchRecord Match(
        QuantitativeObservableRecord computed,
        ExternalTarget target,
        CalibrationPolicy policy,
        string? computedEnvironmentTier = null)
    {
        ArgumentNullException.ThrowIfNull(computed);
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(policy);

        return target.DistributionModel switch
        {
            "gaussian-asymmetric" => MatchAsymmetric(computed, target, policy, computedEnvironmentTier),
            "student-t" => MatchStudentT(computed, target, policy, computedEnvironmentTier),
            _ => MatchGaussian(computed, target, policy, computedEnvironmentTier),
        };
    }

    private static TargetMatchRecord MatchGaussian(
        QuantitativeObservableRecord computed,
        ExternalTarget target,
        CalibrationPolicy policy,
        string? computedEnvironmentTier)
    {
        double sigmaComputed = computed.Uncertainty.TotalUncertainty;
        bool hasComputedSigma = sigmaComputed >= 0;
        string? note = null;

        if (!hasComputedSigma && policy.RequireFullUncertainty)
        {
            return new TargetMatchRecord
            {
                ObservableId = computed.ObservableId,
                TargetLabel = target.Label,
                TargetValue = target.Value,
                TargetUncertainty = target.Uncertainty,
                ComputedValue = computed.Value,
                ComputedUncertainty = sigmaComputed,
                Pull = double.PositiveInfinity,
                Passed = false,
                Notes = "Computed uncertainty unestimated; RequireFullUncertainty=true → failed.",
                ComputedEnvironmentId = computed.EnvironmentId,
                ComputedEnvironmentTier = computedEnvironmentTier,
                ComputedBranchId = computed.BranchId,
                ComputedRefinementLevel = computed.RefinementLevel,
                TargetSource = target.Source,
                TargetProvenance = target.TargetProvenance,
                TargetEvidenceTier = target.EvidenceTier,
                TargetEnvironmentId = target.TargetEnvironmentId,
                TargetEnvironmentTier = target.TargetEnvironmentTier,
            };
        }

        if (!hasComputedSigma)
        {
            note = "Computed uncertainty unestimated; using target sigma only in pull denominator.";
            sigmaComputed = 0;
        }

        double denom = System.Math.Sqrt(
            sigmaComputed * sigmaComputed + target.Uncertainty * target.Uncertainty);

        double pull;
        if (denom < 1e-300)
        {
            pull = System.Math.Abs(computed.Value - target.Value) < 1e-12
                ? 0.0
                : double.PositiveInfinity;
        }
        else
        {
            pull = System.Math.Abs(computed.Value - target.Value) / denom;
        }

        bool passed = pull <= policy.SigmaThreshold;

        return new TargetMatchRecord
        {
            ObservableId = computed.ObservableId,
            TargetLabel = target.Label,
            TargetValue = target.Value,
            TargetUncertainty = target.Uncertainty,
            ComputedValue = computed.Value,
            ComputedUncertainty = computed.Uncertainty.TotalUncertainty,
            Pull = pull,
            Passed = passed,
            Notes = note,
            ComputedEnvironmentId = computed.EnvironmentId,
            ComputedEnvironmentTier = computedEnvironmentTier,
            ComputedBranchId = computed.BranchId,
            ComputedRefinementLevel = computed.RefinementLevel,
            TargetSource = target.Source,
            TargetProvenance = target.TargetProvenance,
            TargetEvidenceTier = target.EvidenceTier,
            TargetEnvironmentId = target.TargetEnvironmentId,
            TargetEnvironmentTier = target.TargetEnvironmentTier,
        };
    }

    /// <summary>
    /// Asymmetric Gaussian: choose lower or upper sigma based on sign of (computed - target).
    /// Residual > 0 (computed above target): use upper sigma.
    /// Residual &lt; 0 (computed below target): use lower sigma.
    /// Falls back to symmetric Uncertainty if the directional field is null.
    /// </summary>
    private static TargetMatchRecord MatchAsymmetric(
        QuantitativeObservableRecord computed,
        ExternalTarget target,
        CalibrationPolicy policy,
        string? computedEnvironmentTier)
    {
        double residual = computed.Value - target.Value;
        double sigmaTarget = residual >= 0
            ? (target.UncertaintyUpper ?? target.Uncertainty)
            : (target.UncertaintyLower ?? target.Uncertainty);

        double sigmaComputed = computed.Uncertainty.TotalUncertainty;
        bool hasComputedSigma = sigmaComputed >= 0;
        string? note = $"gaussian-asymmetric; using {(residual >= 0 ? "upper" : "lower")} sigma={sigmaTarget:G6}.";

        if (!hasComputedSigma && policy.RequireFullUncertainty)
        {
            return new TargetMatchRecord
            {
                ObservableId = computed.ObservableId,
                TargetLabel = target.Label,
                TargetValue = target.Value,
                TargetUncertainty = sigmaTarget,
                ComputedValue = computed.Value,
                ComputedUncertainty = sigmaComputed,
                Pull = double.PositiveInfinity,
                Passed = false,
                Notes = "Computed uncertainty unestimated; RequireFullUncertainty=true → failed.",
                ComputedEnvironmentId = computed.EnvironmentId,
                ComputedEnvironmentTier = computedEnvironmentTier,
                ComputedBranchId = computed.BranchId,
                ComputedRefinementLevel = computed.RefinementLevel,
                TargetSource = target.Source,
                TargetProvenance = target.TargetProvenance,
                TargetEvidenceTier = target.EvidenceTier,
                TargetEnvironmentId = target.TargetEnvironmentId,
                TargetEnvironmentTier = target.TargetEnvironmentTier,
            };
        }

        if (!hasComputedSigma)
        {
            note += " Computed uncertainty unestimated; using target sigma only in pull denominator.";
            sigmaComputed = 0;
        }

        double denom = System.Math.Sqrt(
            sigmaComputed * sigmaComputed + sigmaTarget * sigmaTarget);

        double pull;
        if (denom < 1e-300)
        {
            pull = System.Math.Abs(residual) < 1e-12 ? 0.0 : double.PositiveInfinity;
        }
        else
        {
            pull = System.Math.Abs(residual) / denom;
        }

        return new TargetMatchRecord
        {
            ObservableId = computed.ObservableId,
            TargetLabel = target.Label,
            TargetValue = target.Value,
            TargetUncertainty = sigmaTarget,
            ComputedValue = computed.Value,
            ComputedUncertainty = computed.Uncertainty.TotalUncertainty,
            Pull = pull,
            Passed = pull <= policy.SigmaThreshold,
            Notes = note,
            ComputedEnvironmentId = computed.EnvironmentId,
            ComputedEnvironmentTier = computedEnvironmentTier,
            ComputedBranchId = computed.BranchId,
            ComputedRefinementLevel = computed.RefinementLevel,
            TargetSource = target.Source,
            TargetProvenance = target.TargetProvenance,
            TargetEvidenceTier = target.EvidenceTier,
            TargetEnvironmentId = target.TargetEnvironmentId,
            TargetEnvironmentTier = target.TargetEnvironmentTier,
        };
    }

    /// <summary>
    /// Student-t: normalized pull = |computed - target| / sigma_target, then evaluate
    /// against a Student-t distribution with the declared degrees of freedom.
    /// The pull reported is the raw normalized residual. Passed = pull &lt;= sigmaThreshold.
    /// If StudentTDegreesOfFreedom is null or &lt;= 0, falls back to Gaussian.
    /// </summary>
    private static TargetMatchRecord MatchStudentT(
        QuantitativeObservableRecord computed,
        ExternalTarget target,
        CalibrationPolicy policy,
        string? computedEnvironmentTier)
    {
        double nu = target.StudentTDegreesOfFreedom ?? 0;
        if (nu <= 0)
        {
            // Degenerate: fall back to Gaussian
            return MatchGaussian(computed, target, policy, computedEnvironmentTier);
        }

        double sigmaComputed = computed.Uncertainty.TotalUncertainty;
        bool hasComputedSigma = sigmaComputed >= 0;
        string? note = $"student-t; nu={nu:G6}.";

        if (!hasComputedSigma && policy.RequireFullUncertainty)
        {
            return new TargetMatchRecord
            {
                ObservableId = computed.ObservableId,
                TargetLabel = target.Label,
                TargetValue = target.Value,
                TargetUncertainty = target.Uncertainty,
                ComputedValue = computed.Value,
                ComputedUncertainty = sigmaComputed,
                Pull = double.PositiveInfinity,
                Passed = false,
                Notes = "Computed uncertainty unestimated; RequireFullUncertainty=true → failed.",
                ComputedEnvironmentId = computed.EnvironmentId,
                ComputedEnvironmentTier = computedEnvironmentTier,
                ComputedBranchId = computed.BranchId,
                ComputedRefinementLevel = computed.RefinementLevel,
                TargetSource = target.Source,
                TargetProvenance = target.TargetProvenance,
                TargetEvidenceTier = target.EvidenceTier,
                TargetEnvironmentId = target.TargetEnvironmentId,
                TargetEnvironmentTier = target.TargetEnvironmentTier,
            };
        }

        if (!hasComputedSigma)
        {
            note += " Computed uncertainty unestimated; using target sigma only.";
            sigmaComputed = 0;
        }

        double denom = System.Math.Sqrt(
            sigmaComputed * sigmaComputed + target.Uncertainty * target.Uncertainty);

        double pull;
        if (denom < 1e-300)
        {
            pull = System.Math.Abs(computed.Value - target.Value) < 1e-12 ? 0.0 : double.PositiveInfinity;
        }
        else
        {
            // Student-t normalized pull: scale by sqrt(nu/(nu-2)) for nu > 2 to match
            // the standard deviation of the t-distribution; for nu <= 2 use raw ratio.
            double rawPull = System.Math.Abs(computed.Value - target.Value) / denom;
            if (nu > 2)
            {
                double tScale = System.Math.Sqrt(nu / (nu - 2.0));
                pull = rawPull / tScale;
            }
            else
            {
                pull = rawPull;
            }
        }

        return new TargetMatchRecord
        {
            ObservableId = computed.ObservableId,
            TargetLabel = target.Label,
            TargetValue = target.Value,
            TargetUncertainty = target.Uncertainty,
            ComputedValue = computed.Value,
            ComputedUncertainty = computed.Uncertainty.TotalUncertainty,
            Pull = pull,
            Passed = pull <= policy.SigmaThreshold,
            Notes = note,
            ComputedEnvironmentId = computed.EnvironmentId,
            ComputedEnvironmentTier = computedEnvironmentTier,
            ComputedBranchId = computed.BranchId,
            ComputedRefinementLevel = computed.RefinementLevel,
            TargetSource = target.Source,
            TargetProvenance = target.TargetProvenance,
            TargetEvidenceTier = target.EvidenceTier,
            TargetEnvironmentId = target.TargetEnvironmentId,
            TargetEnvironmentTier = target.TargetEnvironmentTier,
        };
    }
}
