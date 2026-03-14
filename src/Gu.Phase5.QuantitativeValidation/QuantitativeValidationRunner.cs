using Gu.Core;

namespace Gu.Phase5.QuantitativeValidation;

/// <summary>
/// Orchestrator for quantitative validation studies (M49).
///
/// Given a list of computed observables, an external target table, and a calibration policy,
/// runs all target matches and assembles a ConsistencyScoreCard.
/// </summary>
public sealed class QuantitativeValidationRunner
{
    /// <summary>
    /// Run quantitative validation: match all computed observables against their targets.
    /// Only observables with matching ObservableId in the target table are compared.
    /// </summary>
    /// <param name="studyId">Study identifier for the resulting scorecard.</param>
    /// <param name="observables">Computed observables (may include extras without targets).</param>
    /// <param name="targetTable">External target table.</param>
    /// <param name="policy">Calibration policy (sigma threshold, full-uncertainty requirement).</param>
    /// <param name="provenance">Provenance for the scorecard.</param>
    /// <returns>ConsistencyScoreCard aggregating all matched comparisons.</returns>
    public ConsistencyScoreCard Run(
        string studyId,
        IReadOnlyList<QuantitativeObservableRecord> observables,
        ExternalTargetTable targetTable,
        CalibrationPolicy policy,
        ProvenanceMeta provenance)
    {
        ArgumentNullException.ThrowIfNull(studyId);
        ArgumentNullException.ThrowIfNull(observables);
        ArgumentNullException.ThrowIfNull(targetTable);
        ArgumentNullException.ThrowIfNull(policy);
        ArgumentNullException.ThrowIfNull(provenance);

        // Index observables by observableId (take first if duplicates)
        var obsIndex = new Dictionary<string, QuantitativeObservableRecord>(StringComparer.Ordinal);
        foreach (var obs in observables)
        {
            if (!obsIndex.ContainsKey(obs.ObservableId))
                obsIndex[obs.ObservableId] = obs;
        }

        var matches = new List<TargetMatchRecord>();
        foreach (var target in targetTable.Targets)
        {
            if (!obsIndex.TryGetValue(target.ObservableId, out var obs))
                continue; // No computed observable for this target — skip

            var match = TargetMatcher.Match(obs, target, policy);
            matches.Add(match);
        }

        int passed = matches.Count(m => m.Passed);
        int failed = matches.Count(m => !m.Passed);
        double score = matches.Count > 0 ? (double)passed / matches.Count : double.NaN;

        return new ConsistencyScoreCard
        {
            StudyId = studyId,
            SchemaVersion = "1.0.0",
            Matches = matches,
            TotalPassed = passed,
            TotalFailed = failed,
            OverallScore = score,
            CalibrationPolicyId = policy.PolicyId,
            Provenance = provenance,
        };
    }
}
