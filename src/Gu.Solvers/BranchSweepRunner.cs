using Gu.Core;

namespace Gu.Solvers;

/// <summary>
/// Runs a branch sensitivity sweep from an environment context and list of branch manifests.
/// Higher-level than BranchSensitivityRunner: handles artifact packaging and provenance
/// tagging for each branch result. Produces a BranchSweepResult (defined in BranchSweepTypes.cs).
/// </summary>
public sealed class BranchSweepRunner
{
    private readonly ISolverBackend _backend;
    private readonly SolverOptions _innerOptions;

    /// <param name="backend">Solver backend to use for each branch run.</param>
    /// <param name="innerOptions">
    /// Solver options for the inner solve (A, B, or C). Must not be BranchSensitivity.
    /// </param>
    public BranchSweepRunner(ISolverBackend backend, SolverOptions innerOptions)
    {
        _backend = backend ?? throw new ArgumentNullException(nameof(backend));
        _innerOptions = innerOptions ?? throw new ArgumentNullException(nameof(innerOptions));

        if (_innerOptions.Mode == SolveMode.BranchSensitivity)
            throw new ArgumentException(
                "Inner solve mode cannot be BranchSensitivity.", nameof(innerOptions));
    }

    /// <summary>
    /// Run the branch sensitivity sweep and produce artifact-tagged results.
    /// </summary>
    /// <param name="environmentId">Environment ID for provenance tagging.</param>
    /// <param name="initialOmega">Initial connection field (same for all branches).</param>
    /// <param name="a0">Background connection (same for all branches).</param>
    /// <param name="manifests">Branch manifests to sweep (at least 2).</param>
    /// <param name="geometry">Geometry context (same for all branches).</param>
    public BranchSweepResult Sweep(
        string environmentId,
        FieldTensor initialOmega,
        FieldTensor a0,
        IReadOnlyList<BranchManifest> manifests,
        GeometryContext geometry)
    {
        ArgumentNullException.ThrowIfNull(manifests);
        if (manifests.Count < 2)
            throw new ArgumentException(
                "Branch sensitivity requires at least 2 branch manifests.", nameof(manifests));

        // Run inner sweep
        var sensitivityRunner = new BranchSensitivityRunner(_backend, _innerOptions);
        var comparisonResult = sensitivityRunner.Sweep(initialOmega, a0, manifests, geometry);

        // Package each branch result into a BranchSweepEntry with ArtifactBundle
        var now = DateTimeOffset.UtcNow;
        var entries = new List<BranchSweepEntry>(manifests.Count);

        foreach (var branchResult in comparisonResult.BranchResults)
        {
            var manifest = branchResult.Manifest;
            var solverResult = branchResult.SolverResult;

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
                Notes = $"BranchSweep[{environmentId}]: {_innerOptions.Mode}, " +
                        $"{solverResult.Iterations} iters, " +
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
                ArtifactId = $"sweep-{environmentId}-{manifest.BranchId}-{now:yyyyMMddHHmmss}",
                Branch = branchRef,
                ReplayContract = replayContract,
                Provenance = provenance,
                CreatedAt = now,
                FinalState = finalState,
                DerivedState = solverResult.FinalDerivedState,
                Geometry = geometry,
            };

            entries.Add(new BranchSweepEntry
            {
                Manifest = manifest,
                Converged = solverResult.Converged,
                TerminationReason = solverResult.TerminationReason,
                FinalObjective = solverResult.FinalObjective,
                FinalResidualNorm = solverResult.FinalResidualNorm,
                Iterations = solverResult.Iterations,
                ArtifactBundle = artifactBundle,
            });
        }

        return new BranchSweepResult
        {
            Entries = entries,
            InnerMode = _innerOptions.Mode,
        };
    }
}
