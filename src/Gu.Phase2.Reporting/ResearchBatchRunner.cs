using Gu.Phase2.Continuation;
using Gu.Phase2.Execution;
using Gu.Phase2.Recovery;
using Gu.Phase2.Semantics;

namespace Gu.Phase2.Reporting;

/// <summary>
/// Callback for executing a single branch sweep study.
/// Returns the sweep result for the given spec.
/// </summary>
public delegate Phase2BranchSweepResult SweepExecutor(BranchSweepSpec spec);

/// <summary>
/// Callback for executing a single stability study.
/// Returns the continuation result for the given spec.
/// </summary>
public delegate ContinuationResult StabilityExecutor(StabilityStudySpec spec);

/// <summary>
/// Callback for executing a single recovery study.
/// Returns the identification records produced by the recovery graph pipeline.
/// </summary>
public delegate IReadOnlyList<PhysicalIdentificationRecord> RecoveryExecutor(RecoveryStudySpec spec);

/// <summary>
/// Orchestrates reproducible research batch execution across environments and branch sets.
/// Per IMPLEMENTATION_PLAN_P2.md Section 14, Milestone 22.
///
/// The runner is backend-agnostic: callers provide executor delegates for sweeps,
/// stability studies, and recovery studies, keeping the orchestration logic decoupled
/// from solver and physics implementations.
///
/// Execution order:
/// 1. Branch sweeps (all, in order)
/// 2. Stability studies (all, in order)
/// 3. Recovery studies (all, in order)
/// 4. Comparison campaigns (recorded as executed, actual execution delegated)
///
/// Per-study boundaries are preserved: each study produces an independent result.
/// </summary>
public sealed class ResearchBatchRunner
{
    private readonly SweepExecutor _sweepExecutor;
    private readonly StabilityExecutor _stabilityExecutor;
    private readonly RecoveryExecutor? _recoveryExecutor;

    public ResearchBatchRunner(
        SweepExecutor sweepExecutor,
        StabilityExecutor stabilityExecutor,
        RecoveryExecutor? recoveryExecutor = null)
    {
        _sweepExecutor = sweepExecutor ?? throw new ArgumentNullException(nameof(sweepExecutor));
        _stabilityExecutor = stabilityExecutor ?? throw new ArgumentNullException(nameof(stabilityExecutor));
        _recoveryExecutor = recoveryExecutor;
    }

    /// <summary>
    /// Execute a research batch.
    /// </summary>
    public ResearchBatchResult Run(ResearchBatchSpec spec)
    {
        ArgumentNullException.ThrowIfNull(spec);

        var batchStarted = DateTimeOffset.UtcNow;
        var sweepResults = new Dictionary<string, Phase2BranchSweepResult>();
        var stabilityResults = new Dictionary<string, ContinuationResult>();
        var recoveryResults = new Dictionary<string, IReadOnlyList<PhysicalIdentificationRecord>>();

        // 1. Execute branch sweeps
        foreach (var sweep in spec.Sweeps)
        {
            var result = _sweepExecutor(sweep);
            sweepResults[sweep.StudyId] = result;
        }

        // 2. Execute stability studies
        foreach (var study in spec.StabilityStudies)
        {
            var result = _stabilityExecutor(study);
            stabilityResults[study.StudyId] = result;
        }

        // 3. Execute recovery studies
        if (_recoveryExecutor is not null)
        {
            foreach (var recovery in spec.RecoveryStudies)
            {
                var result = _recoveryExecutor(recovery);
                recoveryResults[recovery.StudyId] = result;
            }
        }

        // 4. Record comparison campaigns as executed
        // (actual comparison execution is delegated to the comparison layer)
        var executedCampaigns = spec.ComparisonCampaignIds.ToList();

        return new ResearchBatchResult
        {
            Spec = spec,
            SweepResults = sweepResults,
            StabilityResults = stabilityResults,
            RecoveryResults = recoveryResults,
            ExecutedCampaignIds = executedCampaigns,
            BatchStarted = batchStarted,
            BatchCompleted = DateTimeOffset.UtcNow,
        };
    }
}
