using Gu.Core;
using Gu.Phase2.Execution;
using Gu.Phase2.Semantics;

namespace Gu.Phase2.Canonicity;

/// <summary>
/// Computes pairwise distance matrices, qualitative classification agreement,
/// canonicity evidence, and docket updates from Phase II branch sweep results.
/// </summary>
public sealed class CanonicityAnalyzer
{
    /// <summary>
    /// Compute D_obs: pairwise observed-output distances.
    /// For each pair (i,j), computes the max L2 norm of difference across all shared observables.
    /// If either branch has no ObservedState, that entry is NaN.
    /// </summary>
    public PairwiseDistanceMatrix ComputeObservedDistances(
        Phase2BranchSweepResult sweepResult,
        EquivalenceSpec equivalence)
    {
        ArgumentNullException.ThrowIfNull(sweepResult);
        ArgumentNullException.ThrowIfNull(equivalence);

        var records = sweepResult.RunRecords;
        int n = records.Count;
        var branchIds = records.Select(r => r.Variant.Id).ToList();
        var distances = new double[n, n];

        for (int i = 0; i < n; i++)
        {
            for (int j = i + 1; j < n; j++)
            {
                double dist = ComputeObservedDistance(records[i], records[j]);
                distances[i, j] = dist;
                distances[j, i] = dist;
            }
        }

        return new PairwiseDistanceMatrix
        {
            MetricId = "D_obs_max",
            BranchIds = branchIds,
            Distances = distances,
        };
    }

    /// <summary>
    /// Compute D_dyn: pairwise dynamic distances.
    /// D_dyn[i,j] = |FinalObjective_i - FinalObjective_j| + |FinalResidualNorm_i - FinalResidualNorm_j|
    /// </summary>
    public PairwiseDistanceMatrix ComputeDynamicDistances(
        Phase2BranchSweepResult sweepResult)
    {
        ArgumentNullException.ThrowIfNull(sweepResult);

        var records = sweepResult.RunRecords;
        int n = records.Count;
        var branchIds = records.Select(r => r.Variant.Id).ToList();
        var distances = new double[n, n];

        for (int i = 0; i < n; i++)
        {
            for (int j = i + 1; j < n; j++)
            {
                double dist = System.Math.Abs(records[i].FinalObjective - records[j].FinalObjective)
                            + System.Math.Abs(records[i].FinalResidualNorm - records[j].FinalResidualNorm);
                distances[i, j] = dist;
                distances[j, i] = dist;
            }
        }

        return new PairwiseDistanceMatrix
        {
            MetricId = "D_dyn",
            BranchIds = branchIds,
            Distances = distances,
        };
    }

    /// <summary>
    /// Compute D_conv: pairwise convergence distances.
    /// D_conv[i,j] = |Iterations_i - Iterations_j| + (classMatch ? 0 : 1)
    /// </summary>
    public PairwiseDistanceMatrix ComputeConvergenceDistances(
        Phase2BranchSweepResult sweepResult)
    {
        ArgumentNullException.ThrowIfNull(sweepResult);

        var records = sweepResult.RunRecords;
        int n = records.Count;
        var branchIds = records.Select(r => r.Variant.Id).ToList();
        var classifications = records.Select(ClassifyBranch).ToList();
        var distances = new double[n, n];

        for (int i = 0; i < n; i++)
        {
            for (int j = i + 1; j < n; j++)
            {
                double dist = System.Math.Abs(records[i].Iterations - records[j].Iterations)
                            + (classifications[i] == classifications[j] ? 0.0 : 1.0);
                distances[i, j] = dist;
                distances[j, i] = dist;
            }
        }

        return new PairwiseDistanceMatrix
        {
            MetricId = "D_conv",
            BranchIds = branchIds,
            Distances = distances,
        };
    }

    /// <summary>
    /// Build qualitative classification agreement matrix.
    /// </summary>
    public QualitativeClassificationAgreementMatrix ComputeAgreementMatrix(
        Phase2BranchSweepResult sweepResult)
    {
        ArgumentNullException.ThrowIfNull(sweepResult);

        var records = sweepResult.RunRecords;
        int n = records.Count;
        var branchIds = records.Select(r => r.Variant.Id).ToList();
        var classifications = records.Select(ClassifyBranch).ToList();
        var agrees = new bool[n, n];

        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
                agrees[i, j] = classifications[i] == classifications[j];

        return new QualitativeClassificationAgreementMatrix
        {
            BranchIds = branchIds,
            Classifications = classifications,
            Agrees = agrees,
        };
    }

    /// <summary>
    /// Evaluate canonicity evidence from a branch sweep.
    /// Checks if all pairwise observed distances are within tolerance.
    /// Verdict: "consistent" if all within tolerance, "inconsistent" if any exceed, "inconclusive" if data is missing.
    /// </summary>
    public CanonicityEvidenceRecord Evaluate(
        Phase2BranchSweepResult sweepResult,
        EquivalenceSpec equivalence,
        string objectClass)
    {
        ArgumentNullException.ThrowIfNull(sweepResult);
        ArgumentNullException.ThrowIfNull(equivalence);
        ArgumentException.ThrowIfNullOrEmpty(objectClass);

        var obsMatrix = ComputeObservedDistances(sweepResult, equivalence);
        var dynMatrix = ComputeDynamicDistances(sweepResult);
        var agreement = ComputeAgreementMatrix(sweepResult);

        double tolerance = equivalence.Tolerances.TryGetValue("D_obs", out var t) ? t : 1e-6;
        double maxDeviation = obsMatrix.MaxDistance;
        bool hasNaN = HasNaN(obsMatrix);

        string verdict;
        if (hasNaN && sweepResult.RunRecords.All(r => r.ObservedState == null))
        {
            // No observed outputs at all -- inconclusive
            verdict = "inconclusive";
            maxDeviation = double.NaN;
        }
        else if (maxDeviation <= tolerance && agreement.AllAgree)
        {
            verdict = "consistent";
        }
        else
        {
            verdict = "inconsistent";
        }

        return new CanonicityEvidenceRecord
        {
            EvidenceId = $"ev-{objectClass}-{sweepResult.EnvironmentId}-{sweepResult.SweepCompleted:yyyyMMddHHmmss}",
            StudyId = $"sweep-{sweepResult.EnvironmentId}",
            Verdict = verdict,
            MaxObservedDeviation = double.IsNaN(maxDeviation) ? -1.0 : maxDeviation,
            Tolerance = tolerance,
            Timestamp = sweepResult.SweepCompleted,
        };
    }

    /// <summary>
    /// Update a canonicity docket with new evidence. Creates a new immutable docket instance.
    /// Never removes old evidence -- append only.
    /// If verdict is "inconsistent", adds to KnownCounterexamples.
    /// Transitions Open -> EvidenceAccumulating on first evidence.
    /// </summary>
    public CanonicityDocket UpdateDocket(
        CanonicityDocket existing,
        CanonicityEvidenceRecord newEvidence)
    {
        ArgumentNullException.ThrowIfNull(existing);
        ArgumentNullException.ThrowIfNull(newEvidence);

        var updatedEvidence = existing.CurrentEvidence.Append(newEvidence).ToList();
        var updatedStudyReports = existing.StudyReports.Contains(newEvidence.StudyId)
            ? existing.StudyReports
            : existing.StudyReports.Append(newEvidence.StudyId).ToList();

        var updatedCounterexamples = existing.KnownCounterexamples;
        if (newEvidence.Verdict == "inconsistent")
        {
            updatedCounterexamples = existing.KnownCounterexamples
                .Append(newEvidence.EvidenceId).ToList();
        }

        var newStatus = existing.Status;
        if (existing.Status == DocketStatus.Open)
        {
            newStatus = DocketStatus.EvidenceAccumulating;
        }
        // Do NOT auto-falsify or auto-close. That requires explicit decision.

        return new CanonicityDocket
        {
            ObjectClass = existing.ObjectClass,
            ActiveRepresentative = existing.ActiveRepresentative,
            EquivalenceRelationId = existing.EquivalenceRelationId,
            AdmissibleComparisonClass = existing.AdmissibleComparisonClass,
            DownstreamClaimsBlockedUntilClosure = existing.DownstreamClaimsBlockedUntilClosure,
            CurrentEvidence = updatedEvidence,
            KnownCounterexamples = updatedCounterexamples,
            PendingTheorems = existing.PendingTheorems,
            StudyReports = updatedStudyReports,
            Status = newStatus,
        };
    }

    /// <summary>
    /// Detect fragile branches: branches where small variant changes produce large D_obs
    /// or where qualitative classification disagrees with the majority.
    /// </summary>
    public IReadOnlyList<FragilityReport> DetectFragility(
        Phase2BranchSweepResult sweepResult,
        EquivalenceSpec equivalence)
    {
        ArgumentNullException.ThrowIfNull(sweepResult);
        ArgumentNullException.ThrowIfNull(equivalence);

        var obsMatrix = ComputeObservedDistances(sweepResult, equivalence);
        var agreement = ComputeAgreementMatrix(sweepResult);
        double tolerance = equivalence.Tolerances.TryGetValue("D_obs", out var t) ? t : 1e-6;

        var records = sweepResult.RunRecords;
        int n = records.Count;
        var reports = new List<FragilityReport>(n);

        // Find majority qualitative class
        var majorityClass = agreement.Classifications
            .GroupBy(c => c)
            .OrderByDescending(g => g.Count())
            .First().Key;

        for (int i = 0; i < n; i++)
        {
            var reasons = new List<string>();

            // Check if max D_obs to any other branch exceeds tolerance
            double maxDist = 0.0;
            for (int j = 0; j < n; j++)
            {
                if (i != j && !double.IsNaN(obsMatrix.Distances[i, j]))
                    maxDist = System.Math.Max(maxDist, obsMatrix.Distances[i, j]);
            }
            if (maxDist > tolerance)
            {
                reasons.Add($"Max D_obs ({maxDist:G4}) exceeds tolerance ({tolerance:G4})");
            }

            // Check if qualitative class disagrees with majority
            if (agreement.Classifications[i] != majorityClass)
            {
                reasons.Add($"Qualitative class '{agreement.Classifications[i]}' disagrees with majority '{majorityClass}'");
            }

            reports.Add(new FragilityReport
            {
                BranchId = records[i].Variant.Id,
                IsFragile = reasons.Count > 0,
                FragilityReasons = reasons,
            });
        }

        return reports;
    }

    /// <summary>
    /// Classify a branch run into a qualitative convergence class.
    /// </summary>
    public static QualitativeClass ClassifyBranch(BranchRunRecord record)
    {
        if (record.Converged)
            return QualitativeClass.Converged;

        if (record.TerminationReason.Contains("Stagnation", StringComparison.OrdinalIgnoreCase)
            || record.TerminationReason.Contains("stalled", StringComparison.OrdinalIgnoreCase))
            return QualitativeClass.Stalled;

        return QualitativeClass.Failed;
    }

    private static double ComputeObservedDistance(BranchRunRecord a, BranchRunRecord b)
    {
        if (a.ObservedState == null || b.ObservedState == null)
            return double.NaN;

        double maxDist = 0.0;
        var aObs = a.ObservedState.Observables;
        var bObs = b.ObservedState.Observables;

        // Compare all shared observables, take max distance
        foreach (var (obsId, aSnap) in aObs)
        {
            if (!bObs.TryGetValue(obsId, out var bSnap))
                continue;

            double dist = L2NormDifference(aSnap.Values, bSnap.Values);
            maxDist = System.Math.Max(maxDist, dist);
        }

        return maxDist;
    }

    private static double L2NormDifference(double[] a, double[] b)
    {
        int len = System.Math.Min(a.Length, b.Length);
        double sumSq = 0.0;
        for (int i = 0; i < len; i++)
        {
            double d = a[i] - b[i];
            sumSq += d * d;
        }
        return System.Math.Sqrt(sumSq);
    }

    private static bool HasNaN(PairwiseDistanceMatrix matrix)
    {
        int n = matrix.BranchIds.Count;
        for (int i = 0; i < n; i++)
            for (int j = i + 1; j < n; j++)
                if (double.IsNaN(matrix.Distances[i, j]))
                    return true;
        return false;
    }
}
