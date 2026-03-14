namespace Gu.Phase5.QuantitativeValidation;

/// <summary>
/// Compares a computed observable to an external target (M49).
///
/// Pull statistic (physicist-confirmed):
///   pull = |computed - target| / sqrt(sigma_computed^2 + sigma_target^2)
///   passed = pull &lt;= sigmaThreshold
///
/// If computed uncertainty is unestimated (-1), uses sigma_target only in denominator
/// (unless policy.RequireFullUncertainty = true, in which case the match fails).
/// </summary>
public static class TargetMatcher
{
    /// <summary>
    /// Compute a TargetMatchRecord for the given computed observable and external target.
    /// </summary>
    public static TargetMatchRecord Match(
        QuantitativeObservableRecord computed,
        ExternalTarget target,
        CalibrationPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(computed);
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(policy);

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
            // Both uncertainties are zero or near-zero: exact comparison
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
        };
    }
}
