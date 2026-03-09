using Gu.Branching;
using Gu.Core;
using Gu.Geometry;
using Gu.Observation;
using Gu.Phase2.Branches;
using Gu.Phase2.Semantics;
using Gu.Phase2.Stability;
using Gu.Solvers;

namespace Gu.Phase2.Execution;

/// <summary>
/// Phase II branch sweep runner: executes one environment across a branch family.
/// For each variant: resolves operators, runs solver, extracts observed outputs,
/// emits per-branch artifact bundles with full provenance.
///
/// Pseudocode (Section 10.1):
///   for each branch b in BranchSet:
///     state0      = InitializeState(environment, b)
///     solveResult = SolveBranch(environment, b, state0)
///     nativeOut   = ComputeNativeOutputs(solveResult, b)
///     obsOut      = RecoverObservedOutputs(nativeOut, environment, b)
///     store RunRecord(b, solveResult, nativeOut, obsOut)
/// </summary>
public sealed class Phase2BranchSweepRunner
{
    private readonly ISolverBackend _backend;
    private readonly SolverOptions _innerOptions;
    private readonly BranchVariantOperatorDispatch _operatorDispatch;
    private readonly ObservationVariantDispatch _observationDispatch;
    private readonly PullbackOperator? _pullback;

    /// <param name="backend">Solver backend for each branch run.</param>
    /// <param name="innerOptions">Solver options (must not be BranchSensitivity).</param>
    /// <param name="operatorDispatch">Resolves torsion/shiab/biConnection operators per variant.</param>
    /// <param name="observationDispatch">Resolves observation/extraction pipeline per variant.</param>
    /// <param name="pullback">Pullback operator for observation pipeline. If null, observation extraction is skipped.</param>
    public Phase2BranchSweepRunner(
        ISolverBackend backend,
        SolverOptions innerOptions,
        BranchVariantOperatorDispatch operatorDispatch,
        ObservationVariantDispatch observationDispatch,
        PullbackOperator? pullback = null)
    {
        _backend = backend ?? throw new ArgumentNullException(nameof(backend));
        _innerOptions = innerOptions ?? throw new ArgumentNullException(nameof(innerOptions));
        _operatorDispatch = operatorDispatch ?? throw new ArgumentNullException(nameof(operatorDispatch));
        _observationDispatch = observationDispatch ?? throw new ArgumentNullException(nameof(observationDispatch));
        _pullback = pullback;

        if (_innerOptions.Mode == SolveMode.BranchSensitivity)
            throw new ArgumentException(
                "Inner solve mode cannot be BranchSensitivity.", nameof(innerOptions));
    }

    /// <summary>
    /// Execute the branch sweep across all variants in a family.
    /// </summary>
    /// <param name="environmentId">Environment identifier for provenance tagging.</param>
    /// <param name="initialOmega">Initial connection field (same for all branches).</param>
    /// <param name="a0">Background connection (same for all branches).</param>
    /// <param name="family">Branch family manifest.</param>
    /// <param name="baseManifest">Base Phase I manifest providing shared configuration.</param>
    /// <param name="geometry">Geometry context (same for all branches).</param>
    /// <param name="stabilityProbe">Optional callback to compute per-branch stability diagnostics
    /// from the solver result. When null, StabilityDiagnostics is not populated.</param>
    public Phase2BranchSweepResult Sweep(
        string environmentId,
        FieldTensor initialOmega,
        FieldTensor a0,
        BranchFamilyManifest family,
        BranchManifest baseManifest,
        GeometryContext geometry,
        Func<SolverResult, BranchManifest, GeometryContext, HessianSummary>? stabilityProbe = null)
    {
        ArgumentNullException.ThrowIfNull(environmentId);
        ArgumentNullException.ThrowIfNull(initialOmega);
        ArgumentNullException.ThrowIfNull(a0);
        ArgumentNullException.ThrowIfNull(family);
        ArgumentNullException.ThrowIfNull(baseManifest);
        ArgumentNullException.ThrowIfNull(geometry);

        BranchFamilyValidator.ValidateFamilyOrThrow(family);

        var sweepStarted = DateTimeOffset.UtcNow;
        var records = new List<BranchRunRecord>(family.Variants.Count);

        foreach (var variant in family.Variants)
        {
            var record = RunSingleBranch(
                environmentId, initialOmega, a0, variant, baseManifest, geometry, stabilityProbe);
            records.Add(record);
        }

        return new Phase2BranchSweepResult
        {
            Family = family,
            EnvironmentId = environmentId,
            RunRecords = records,
            InnerMode = _innerOptions.Mode,
            SweepStarted = sweepStarted,
            SweepCompleted = DateTimeOffset.UtcNow,
        };
    }

    /// <summary>
    /// Run a single branch variant: solve + observe + package artifact.
    /// </summary>
    private BranchRunRecord RunSingleBranch(
        string environmentId,
        FieldTensor initialOmega,
        FieldTensor a0,
        BranchVariantManifest variant,
        BranchManifest baseManifest,
        GeometryContext geometry,
        Func<SolverResult, BranchManifest, GeometryContext, HessianSummary>? stabilityProbe = null)
    {
        // 1. Resolve operators via dispatch
        var resolvedOps = _operatorDispatch.Resolve(variant, baseManifest);
        var manifest = resolvedOps.Manifest;

        // 2. Run solver with cloned initial state
        var omegaClone = CloneField(initialOmega);
        var orchestrator = new SolverOrchestrator(_backend, _innerOptions);
        var solverResult = orchestrator.Solve(omegaClone, a0, manifest, geometry);

        // 3. Extract observed outputs if pullback is available
        ObservedState? observedState = null;
        if (_pullback != null && solverResult.FinalDerivedState != null)
        {
            observedState = ExtractObservations(
                variant, baseManifest, solverResult, geometry);
        }

        // 3b. Compute stability diagnostics if probe is provided
        HessianSummary? stabilityDiagnostics = null;
        if (stabilityProbe != null)
        {
            stabilityDiagnostics = stabilityProbe(solverResult, manifest, geometry);
        }

        // 4. Build artifact bundle
        var now = DateTimeOffset.UtcNow;
        var branchRef = new BranchRef
        {
            BranchId = manifest.BranchId,
            SchemaVersion = manifest.SchemaVersion,
        };
        var provenance = new ProvenanceMeta
        {
            CreatedAt = now,
            CodeRevision = manifest.CodeRevision,
            Branch = branchRef,
            Backend = "cpu-reference",
            Notes = $"Phase2Sweep[{environmentId}]: variant={variant.Id}, " +
                    $"mode={_innerOptions.Mode}, {solverResult.Iterations} iters, " +
                    $"converged={solverResult.Converged}",
        };
        var replayContract = new ReplayContract
        {
            BranchManifest = manifest,
            Deterministic = true,
            BackendId = "cpu-reference",
            ReplayTier = "R2",
        };
        var finalState = new DiscreteState
        {
            Branch = branchRef,
            Geometry = geometry,
            Omega = solverResult.FinalOmega,
            Provenance = provenance,
        };
        var artifactBundle = new ArtifactBundle
        {
            ArtifactId = $"p2-sweep-{environmentId}-{variant.Id}-{now:yyyyMMddHHmmss}",
            Branch = branchRef,
            ReplayContract = replayContract,
            Provenance = provenance,
            CreatedAt = now,
            FinalState = finalState,
            DerivedState = solverResult.FinalDerivedState,
            ObservedState = observedState,
            Geometry = geometry,
        };

        // Extraction succeeds if we attempted observation and got a non-null result
        bool extractionSucceeded = observedState != null;

        // Comparison admissible if extraction succeeded and observed state has at least one observable
        bool comparisonAdmissible = extractionSucceeded
            && observedState!.Observables.Count > 0;

        return new BranchRunRecord
        {
            Variant = variant,
            Manifest = manifest,
            Converged = solverResult.Converged,
            TerminationReason = solverResult.TerminationReason,
            FinalObjective = solverResult.FinalObjective,
            FinalResidualNorm = solverResult.FinalResidualNorm,
            Iterations = solverResult.Iterations,
            SolveMode = _innerOptions.Mode,
            ObservedState = observedState,
            ExtractionSucceeded = extractionSucceeded,
            ComparisonAdmissible = comparisonAdmissible,
            StabilityDiagnostics = stabilityDiagnostics,
            ArtifactBundle = artifactBundle,
        };
    }

    /// <summary>
    /// Extract observed outputs for a branch variant using the observation pipeline.
    /// </summary>
    private ObservedState? ExtractObservations(
        BranchVariantManifest variant,
        BranchManifest baseManifest,
        SolverResult solverResult,
        GeometryContext geometry)
    {
        if (_pullback == null || solverResult.FinalDerivedState == null)
            return null;

        var obsConfig = _observationDispatch.Resolve(variant, baseManifest);
        var pipeline = new ObservationPipeline(
            _pullback, obsConfig.Transforms, obsConfig.Normalization);

        var branchRef = new BranchRef
        {
            BranchId = obsConfig.Manifest.BranchId,
            SchemaVersion = obsConfig.Manifest.SchemaVersion,
        };
        var finalState = new DiscreteState
        {
            Branch = branchRef,
            Geometry = geometry,
            Omega = solverResult.FinalOmega,
            Provenance = new ProvenanceMeta
            {
                CreatedAt = DateTimeOffset.UtcNow,
                CodeRevision = obsConfig.Manifest.CodeRevision,
                Branch = branchRef,
            },
        };

        return pipeline.Extract(
            solverResult.FinalDerivedState,
            finalState,
            geometry,
            obsConfig.Requests,
            obsConfig.Manifest);
    }

    private static FieldTensor CloneField(FieldTensor f)
    {
        return new FieldTensor
        {
            Label = f.Label,
            Signature = f.Signature,
            Coefficients = (double[])f.Coefficients.Clone(),
            Shape = f.Shape,
        };
    }
}
