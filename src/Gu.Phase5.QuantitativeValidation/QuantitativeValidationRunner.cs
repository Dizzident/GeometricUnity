using Gu.Core;
using Gu.Phase5.Environments;

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
        ProvenanceMeta provenance,
        IReadOnlyList<EnvironmentRecord>? environmentRecords = null)
    {
        ArgumentNullException.ThrowIfNull(studyId);
        ArgumentNullException.ThrowIfNull(observables);
        ArgumentNullException.ThrowIfNull(targetTable);
        ArgumentNullException.ThrowIfNull(policy);
        ArgumentNullException.ThrowIfNull(provenance);

        var envTierById = (environmentRecords ?? Array.Empty<EnvironmentRecord>())
            .GroupBy(r => r.EnvironmentId, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.First().GeometryTier, StringComparer.Ordinal);

        var matches = new List<TargetMatchRecord>();
        foreach (var target in targetTable.Targets)
        {
            var candidates = observables
                .Where(obs => string.Equals(obs.ObservableId, target.ObservableId, StringComparison.Ordinal))
                .Where(obs => MatchesRequestedEnvironment(obs, target, envTierById))
                .ToList();

            if (candidates.Count == 0)
                continue; // No computed observable for this target — skip

            var obs = SelectObservable(candidates, envTierById);
            envTierById.TryGetValue(obs.EnvironmentId, out var computedEnvironmentTier);

            var match = TargetMatcher.Match(obs, target, policy, computedEnvironmentTier);
            matches.Add(match);
        }

        int passed = matches.Count(m => m.Passed);
        int failed = matches.Count(m => !m.Passed);
        double score = matches.Count > 0 ? (double)passed / matches.Count : double.NaN;
        var benchmarkClassCounts = matches
            .GroupBy(m => string.IsNullOrWhiteSpace(m.TargetBenchmarkClass) ? "unspecified" : m.TargetBenchmarkClass!, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.Count(), StringComparer.Ordinal);
        var failedBenchmarkClassCounts = matches
            .Where(m => !m.Passed)
            .GroupBy(m => string.IsNullOrWhiteSpace(m.TargetBenchmarkClass) ? "unspecified" : m.TargetBenchmarkClass!, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.Count(), StringComparer.Ordinal);

        return new ConsistencyScoreCard
        {
            StudyId = studyId,
            SchemaVersion = "1.0.0",
            Matches = matches,
            TotalPassed = passed,
            TotalFailed = failed,
            OverallScore = score,
            CalibrationPolicyId = policy.PolicyId,
            BenchmarkClassCounts = benchmarkClassCounts,
            FailedBenchmarkClassCounts = failedBenchmarkClassCounts,
            Provenance = provenance,
        };
    }

    private static bool MatchesRequestedEnvironment(
        QuantitativeObservableRecord observable,
        ExternalTarget target,
        IReadOnlyDictionary<string, string> envTierById)
    {
        if (!string.IsNullOrWhiteSpace(target.TargetEnvironmentId) &&
            !string.Equals(observable.EnvironmentId, target.TargetEnvironmentId, StringComparison.Ordinal))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(target.TargetEnvironmentTier))
        {
            envTierById.TryGetValue(observable.EnvironmentId, out var observableTier);
            if (!string.Equals(observableTier, target.TargetEnvironmentTier, StringComparison.Ordinal))
                return false;
        }

        return true;
    }

    private static QuantitativeObservableRecord SelectObservable(
        IReadOnlyList<QuantitativeObservableRecord> candidates,
        IReadOnlyDictionary<string, string> envTierById)
    {
        return candidates
            .OrderBy(obs => obs.Uncertainty.TotalUncertainty < 0 ? 1 : 0)
            .ThenBy(obs => obs.Uncertainty.TotalUncertainty < 0 ? double.PositiveInfinity : obs.Uncertainty.TotalUncertainty)
            .ThenByDescending(obs => RefinementRank(obs.RefinementLevel))
            .ThenByDescending(obs => EnvironmentTierRank(envTierById.TryGetValue(obs.EnvironmentId, out var tier) ? tier : null))
            .ThenBy(obs => obs.EnvironmentId, StringComparer.Ordinal)
            .ThenBy(obs => obs.BranchId, StringComparer.Ordinal)
            .ThenBy(obs => obs.RefinementLevel ?? string.Empty, StringComparer.Ordinal)
            .First();
    }

    private static int EnvironmentTierRank(string? tier) => tier switch
    {
        "imported" => 3,
        "structured" => 2,
        "toy" => 1,
        _ => 0,
    };

    private static int RefinementRank(string? refinementLevel)
    {
        if (string.IsNullOrWhiteSpace(refinementLevel))
            return 0;

        if (refinementLevel.StartsWith('L') && refinementLevel.Length >= 2 && char.IsDigit(refinementLevel[1]))
            return refinementLevel[1] - '0';

        return 0;
    }
}
