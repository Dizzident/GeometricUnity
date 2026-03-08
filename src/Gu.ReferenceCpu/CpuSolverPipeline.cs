using Gu.Branching;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.Solvers;

namespace Gu.ReferenceCpu;

/// <summary>
/// End-to-end CPU solver pipeline for Geometric Unity.
/// Wires together the full CPU computational pipeline:
///
/// 1. Create connection field omega on mesh with Lie algebra
/// 2. Build bi-connection (A0 + omega, A0 - omega)
/// 3. Compute curvature F
/// 4. Evaluate T and S branch operators
/// 5. Assemble residual Upsilon = S - T
/// 6. Compute objective I2
/// 7. Compute Jacobian and gradient
/// 8. Run solver iteration (gradient descent + gauge penalty)
/// 9. Return ArtifactBundle with full results
///
/// This is the key integration point binding Gu.ReferenceCpu to Gu.Solvers.
/// </summary>
public sealed class CpuSolverPipeline
{
    private readonly SimplicialMesh _mesh;
    private readonly LieAlgebra _algebra;
    private readonly ITorsionBranchOperator _torsion;
    private readonly IShiabBranchOperator _shiab;

    public CpuSolverPipeline(
        SimplicialMesh mesh,
        LieAlgebra algebra,
        ITorsionBranchOperator torsion,
        IShiabBranchOperator shiab)
    {
        _mesh = mesh ?? throw new ArgumentNullException(nameof(mesh));
        _algebra = algebra ?? throw new ArgumentNullException(nameof(algebra));
        _torsion = torsion ?? throw new ArgumentNullException(nameof(torsion));
        _shiab = shiab ?? throw new ArgumentNullException(nameof(shiab));
    }

    /// <summary>
    /// Execute the full pipeline: evaluate + solve + package results.
    /// </summary>
    /// <param name="initialOmega">Initial connection field omega_h (or null for zero).</param>
    /// <param name="a0">Distinguished connection A0 (or null for flat).</param>
    /// <param name="manifest">Branch manifest controlling the computation.</param>
    /// <param name="geometry">Geometry context.</param>
    /// <param name="options">Solver options (mode, tolerances, gauge penalty).</param>
    /// <returns>Pipeline result containing SolverResult and ArtifactBundle.</returns>
    public PipelineResult Execute(
        ConnectionField? initialOmega,
        ConnectionField? a0,
        BranchManifest manifest,
        GeometryContext geometry,
        SolverOptions options)
    {
        // 1. Default to zero connections if not provided
        var omega = initialOmega ?? ConnectionField.Zero(_mesh, _algebra);
        var a0Field = a0 ?? ConnectionField.Zero(_mesh, _algebra);

        // 2. Build bi-connection for provenance tracking
        var biBuilder = new BiConnectionBuilder(a0Field, manifest.BranchId);
        var (aConn, bConn) = biBuilder.Build(omega);

        // 3. Create solver backend and orchestrator
        var massMatrix = new CpuMassMatrix(_mesh, _algebra);
        var backend = new CpuSolverBackend(_mesh, _algebra, _torsion, _shiab, massMatrix);
        var orchestrator = new SolverOrchestrator(backend, options);

        // 4. Run the solver
        var omegaTensor = omega.ToFieldTensor();
        var a0Tensor = a0Field.ToFieldTensor();
        var solverResult = orchestrator.Solve(omegaTensor, a0Tensor, manifest, geometry);

        // 5. Assemble residual bundle from final state
        var assembler = new CpuResidualAssembler(_mesh, _algebra, _torsion, _shiab);
        var finalOmega = new ConnectionField(_mesh, _algebra, (double[])solverResult.FinalOmega.Coefficients.Clone());
        var residualBundle = assembler.EvaluateResidual(solverResult.FinalDerivedState, massMatrix);

        // 6. Build convergence diagnostics
        var diagnostics = new ConvergenceDiagnostics();
        foreach (var record in solverResult.History)
        {
            diagnostics.Record(record);
        }
        var diagnosticLog = diagnostics.GenerateLog();

        // 7. Package into ArtifactBundle
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
            Notes = $"CpuSolverPipeline: {options.Mode}, {solverResult.Iterations} iterations, " +
                    $"converged={solverResult.Converged}, reason={solverResult.TerminationReason}",
        };

        var initialState = new DiscreteState
        {
            Branch = branchRef,
            Geometry = geometry,
            Omega = omegaTensor,
            Provenance = provenance,
        };

        var finalState = new DiscreteState
        {
            Branch = branchRef,
            Geometry = geometry,
            Omega = solverResult.FinalOmega,
            Provenance = provenance,
        };

        var replayContract = new ReplayContract
        {
            BranchManifest = manifest,
            Deterministic = true,
            BackendId = "cpu-reference",
            ReplayTier = "R2",
        };

        var artifactBundle = new ArtifactBundle
        {
            ArtifactId = $"cpu-solve-{manifest.BranchId}-{now:yyyyMMddHHmmss}",
            Branch = branchRef,
            ReplayContract = replayContract,
            Provenance = provenance,
            CreatedAt = now,
            InitialState = initialState,
            FinalState = finalState,
            DerivedState = solverResult.FinalDerivedState,
            Residuals = residualBundle,
            Geometry = geometry,
        };

        return new PipelineResult
        {
            SolverResult = solverResult,
            ArtifactBundle = artifactBundle,
            DiagnosticLog = diagnosticLog,
            ConvergenceSummary = diagnostics.GetSummary(),
            FinalConnection = finalOmega,
            BiConnectionA = aConn,
            BiConnectionB = bConn,
        };
    }

    /// <summary>
    /// Convenience method: execute with zero initial omega and flat A0.
    /// Useful for testing and baseline establishment.
    /// </summary>
    public PipelineResult ExecuteFromFlat(
        BranchManifest manifest,
        GeometryContext geometry,
        SolverOptions options)
    {
        return Execute(null, null, manifest, geometry, options);
    }

    /// <summary>
    /// Execute with a custom initial omega specified as a flat coefficient array.
    /// </summary>
    public PipelineResult ExecuteFromCoefficients(
        double[] omegaCoefficients,
        BranchManifest manifest,
        GeometryContext geometry,
        SolverOptions options)
    {
        var omega = new ConnectionField(_mesh, _algebra, omegaCoefficients);
        return Execute(omega, null, manifest, geometry, options);
    }
}

/// <summary>
/// Complete result of the CPU solver pipeline.
/// Contains the solver result, artifact bundle, diagnostics, and intermediate fields.
/// </summary>
public sealed class PipelineResult
{
    /// <summary>Solver result with convergence information.</summary>
    public required SolverResult SolverResult { get; init; }

    /// <summary>Complete artifact bundle for archival.</summary>
    public required ArtifactBundle ArtifactBundle { get; init; }

    /// <summary>Diagnostic log lines.</summary>
    public required List<string> DiagnosticLog { get; init; }

    /// <summary>Convergence summary.</summary>
    public required ConvergenceSummary ConvergenceSummary { get; init; }

    /// <summary>Final connection field.</summary>
    public required ConnectionField FinalConnection { get; init; }

    /// <summary>Bi-connection A = A0 + omega (initial).</summary>
    public required ConnectionField BiConnectionA { get; init; }

    /// <summary>Bi-connection B = A0 - omega (initial).</summary>
    public required ConnectionField BiConnectionB { get; init; }
}
