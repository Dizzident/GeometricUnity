using Gu.Core;

namespace Gu.Solvers;

/// <summary>
/// Result of a single branch run within a branch sensitivity sweep.
/// </summary>
public sealed class BranchRunResult
{
    /// <summary>Branch manifest used for this run.</summary>
    public required BranchManifest Manifest { get; init; }

    /// <summary>Solver result from this branch run.</summary>
    public required SolverResult SolverResult { get; init; }
}

/// <summary>
/// Aggregated result of a branch sensitivity analysis (Mode D).
/// Holds per-branch results and comparison summary.
/// </summary>
public sealed class BranchComparisonResult
{
    /// <summary>Per-branch results in order of input manifests.</summary>
    public required IReadOnlyList<BranchRunResult> BranchResults { get; init; }

    /// <summary>Inner solve mode used for each branch run (A, B, or C).</summary>
    public required SolveMode InnerMode { get; init; }

    /// <summary>Number of branches swept.</summary>
    public int BranchCount => BranchResults.Count;

    /// <summary>Branch IDs that converged.</summary>
    public IReadOnlyList<string> ConvergedBranches =>
        BranchResults
            .Where(r => r.SolverResult.Converged)
            .Select(r => r.Manifest.BranchId)
            .ToList();

    /// <summary>Branch IDs that did not converge.</summary>
    public IReadOnlyList<string> DivergedBranches =>
        BranchResults
            .Where(r => !r.SolverResult.Converged)
            .Select(r => r.Manifest.BranchId)
            .ToList();

    /// <summary>Best (lowest) final objective across branches.</summary>
    public double BestObjective =>
        BranchResults.Count > 0
            ? BranchResults.Min(r => r.SolverResult.FinalObjective)
            : 0.0;

    /// <summary>Worst (highest) final residual norm across branches.</summary>
    public double WorstResidualNorm =>
        BranchResults.Count > 0
            ? BranchResults.Max(r => r.SolverResult.FinalResidualNorm)
            : 0.0;
}

/// <summary>
/// Runs branch sensitivity analysis (Mode D, Section 13.3).
/// Sweeps the same environment under multiple branch manifests and compares results.
/// </summary>
public sealed class BranchSensitivityRunner
{
    private readonly ISolverBackend _backend;
    private readonly SolverOptions _innerOptions;

    /// <param name="backend">Solver backend to use for each branch run.</param>
    /// <param name="innerOptions">
    /// Solver options for the inner solve. The Mode field specifies which inner mode
    /// (A, B, or C) to use for each branch. Must not be BranchSensitivity.
    /// </param>
    public BranchSensitivityRunner(ISolverBackend backend, SolverOptions innerOptions)
    {
        _backend = backend ?? throw new ArgumentNullException(nameof(backend));
        _innerOptions = innerOptions ?? throw new ArgumentNullException(nameof(innerOptions));

        if (_innerOptions.Mode == SolveMode.BranchSensitivity)
            throw new ArgumentException("Inner solve mode cannot be BranchSensitivity.", nameof(innerOptions));
    }

    /// <summary>
    /// Run the sensitivity sweep across all provided branch manifests.
    /// </summary>
    /// <param name="initialOmega">Initial connection field (same for all branches).</param>
    /// <param name="a0">Background connection (same for all branches).</param>
    /// <param name="manifests">Branch manifests to sweep. Must contain at least 2.</param>
    /// <param name="geometry">Geometry context (same for all branches).</param>
    public BranchComparisonResult Sweep(
        FieldTensor initialOmega,
        FieldTensor a0,
        IReadOnlyList<BranchManifest> manifests,
        GeometryContext geometry)
    {
        if (manifests is null) throw new ArgumentNullException(nameof(manifests));
        if (manifests.Count < 2)
            throw new ArgumentException("Branch sensitivity requires at least 2 branch manifests.", nameof(manifests));

        var results = new List<BranchRunResult>(manifests.Count);

        foreach (var manifest in manifests)
        {
            var orchestrator = new SolverOrchestrator(_backend, _innerOptions);
            var solverResult = orchestrator.Solve(
                CloneField(initialOmega), a0, manifest, geometry);

            results.Add(new BranchRunResult
            {
                Manifest = manifest,
                SolverResult = solverResult,
            });
        }

        return new BranchComparisonResult
        {
            BranchResults = results,
            InnerMode = _innerOptions.Mode,
        };
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
