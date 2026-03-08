using System.Diagnostics;
using System.Text.Json;
using Gu.Core;
using Gu.Interop;
using Gu.Solvers;

namespace Gu.Benchmarks;

/// <summary>
/// Runs scaling and parity benchmarks using different solver backends.
/// Produces BenchmarkReport artifacts for each run.
/// </summary>
public sealed class BenchmarkRunner
{
    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        WriteIndented = true,
    };

    /// <summary>
    /// Run a solve benchmark for a given problem size using a CudaSolverBackend
    /// backed by CpuReferenceBackend (for parity validation).
    /// </summary>
    public BenchmarkReport RunSolveBenchmark(
        string benchmarkId,
        int problemSize,
        SolverOptions solverOptions)
    {
        var sig = CreateSignature();
        var manifest = CreateManifest();
        var geometry = CreateGeometry();
        var manifestSnapshot = CreateManifestSnapshot(problemSize);

        // Create field tensors with given problem size
        int n = problemSize * 3; // nCells * lieAlgDim for su(2)
        var omegaCoeffs = new double[n];
        var rng = new Random(42);
        for (int i = 0; i < n; i++)
            omegaCoeffs[i] = 0.1 * (rng.NextDouble() - 0.5);

        var omega = new FieldTensor
        {
            Label = "omega_h",
            Signature = sig,
            Coefficients = omegaCoeffs,
            Shape = new[] { problemSize, 3 },
        };
        var a0 = new FieldTensor
        {
            Label = "a0",
            Signature = sig,
            Coefficients = new double[n],
            Shape = new[] { problemSize, 3 },
        };

        using var nativeBackend = new CpuReferenceBackend();
        using var solverBackend = new CudaSolverBackend(nativeBackend, ownsNative: false);
        solverBackend.Initialize(manifestSnapshot);

        var orchestrator = new SolverOrchestrator(solverBackend, solverOptions);

        var sw = Stopwatch.StartNew();
        var result = orchestrator.Solve(omega, a0, manifest, geometry);
        sw.Stop();

        double perIter = result.Iterations > 0 ? sw.Elapsed.TotalMilliseconds / result.Iterations : sw.Elapsed.TotalMilliseconds;

        return new BenchmarkReport
        {
            BenchmarkId = benchmarkId,
            Description = $"Solve benchmark: {problemSize} cells, mode={solverOptions.Mode}",
            BackendId = nativeBackend.Version.BackendId,
            ProblemSize = problemSize,
            Iterations = result.Iterations,
            TotalTimeMs = sw.Elapsed.TotalMilliseconds,
            PerIterationTimeMs = perIter,
            FinalObjective = result.FinalObjective,
            Converged = result.Converged,
            TerminationReason = result.TerminationReason,
        };
    }

    /// <summary>
    /// Run a parity benchmark: same problem solved on two backends, results compared.
    /// </summary>
    public (BenchmarkReport cpuReport, BenchmarkReport gpuReport, IReadOnlyList<ParityRecord> parityRecords)
        RunParityBenchmark(string benchmarkId, int problemSize)
    {
        var manifestSnapshot = CreateManifestSnapshot(problemSize);
        int n = problemSize * 3;
        var omegaData = new double[n];
        var rng = new Random(42);
        for (int i = 0; i < n; i++)
            omegaData[i] = 0.1 * (rng.NextDouble() - 0.5);

        using var reference = new CpuReferenceBackend();
        using var target = new CpuReferenceBackend();

        var checker = new ParityChecker(reference, target);

        var sw = Stopwatch.StartNew();
        var parityRecords = checker.RunFullResidualParity(manifestSnapshot, omegaData);
        sw.Stop();

        var cpuReport = new BenchmarkReport
        {
            BenchmarkId = $"{benchmarkId}-cpu",
            Description = $"Parity benchmark CPU: {problemSize} cells",
            BackendId = "cpu-reference",
            ProblemSize = problemSize,
            Iterations = 1,
            TotalTimeMs = sw.Elapsed.TotalMilliseconds,
            PerIterationTimeMs = sw.Elapsed.TotalMilliseconds,
            FinalObjective = 0,
            Converged = true,
            TerminationReason = "Parity test complete",
        };

        var gpuReport = new BenchmarkReport
        {
            BenchmarkId = $"{benchmarkId}-gpu",
            Description = $"Parity benchmark GPU: {problemSize} cells",
            BackendId = "cpu-reference", // Using CPU reference as stand-in
            ProblemSize = problemSize,
            Iterations = 1,
            TotalTimeMs = sw.Elapsed.TotalMilliseconds,
            PerIterationTimeMs = sw.Elapsed.TotalMilliseconds,
            FinalObjective = 0,
            Converged = parityRecords.All(r => r.Passed),
            TerminationReason = parityRecords.All(r => r.Passed) ? "All parity checks passed" : "Parity check failed",
        };

        return (cpuReport, gpuReport, parityRecords);
    }

    /// <summary>
    /// Run scaling benchmarks across multiple problem sizes.
    /// </summary>
    public IReadOnlyList<BenchmarkReport> RunScalingBenchmark(
        string benchmarkId,
        int[] problemSizes,
        SolverOptions solverOptions)
    {
        var reports = new List<BenchmarkReport>();
        foreach (int size in problemSizes)
        {
            var report = RunSolveBenchmark($"{benchmarkId}-{size}", size, solverOptions);
            reports.Add(report);
        }
        return reports;
    }

    /// <summary>
    /// Write benchmark reports to a directory as JSON artifacts.
    /// </summary>
    public static void WriteReports(string outputDir, IReadOnlyList<BenchmarkReport> reports)
    {
        Directory.CreateDirectory(outputDir);
        foreach (var report in reports)
        {
            string path = Path.Combine(outputDir, $"{report.BenchmarkId}.json");
            string json = JsonSerializer.Serialize(report, s_jsonOptions);
            File.WriteAllText(path, json);
        }
    }

    private static TensorSignature CreateSignature() => new()
    {
        AmbientSpaceId = "Y_h",
        CarrierType = "connection-1form",
        Degree = "1",
        LieAlgebraBasisId = "basis-standard",
        ComponentOrderId = "order-row-major",
        MemoryLayout = "dense-row-major",
        NumericPrecision = "float64",
    };

    private static BranchManifest CreateManifest() => new()
    {
        BranchId = "benchmark",
        SchemaVersion = "1.0.0",
        SourceEquationRevision = "r1",
        CodeRevision = "benchmark",
        ActiveGeometryBranch = "simplicial",
        ActiveObservationBranch = "sigma-pullback",
        ActiveTorsionBranch = "trivial",
        ActiveShiabBranch = "identity",
        ActiveGaugeStrategy = "penalty",
        BaseDimension = 4,
        AmbientDimension = 14,
        LieAlgebraId = "su2",
        BasisConventionId = "basis-standard",
        ComponentOrderId = "order-row-major",
        AdjointConventionId = "adjoint-explicit",
        PairingConventionId = "pairing-killing",
        NormConventionId = "norm-l2-quadrature",
        DifferentialFormMetricId = "hodge-standard",
        InsertedAssumptionIds = Array.Empty<string>(),
        InsertedChoiceIds = Array.Empty<string>(),
    };

    private static GeometryContext CreateGeometry()
    {
        var baseSpace = new SpaceRef { SpaceId = "X_h", Dimension = 4 };
        var ambientSpace = new SpaceRef { SpaceId = "Y_h", Dimension = 14 };
        return new GeometryContext
        {
            BaseSpace = baseSpace,
            AmbientSpace = ambientSpace,
            DiscretizationType = "simplicial",
            QuadratureRuleId = "midpoint",
            BasisFamilyId = "whitney-0",
            ProjectionBinding = new GeometryBinding
            {
                BindingType = "projection",
                SourceSpace = ambientSpace,
                TargetSpace = baseSpace,
            },
            ObservationBinding = new GeometryBinding
            {
                BindingType = "observation",
                SourceSpace = baseSpace,
                TargetSpace = ambientSpace,
            },
            Patches = new[]
            {
                new PatchInfo { PatchId = "patch-0", ElementCount = 10 },
            },
        };
    }

    private static ManifestSnapshot CreateManifestSnapshot(int cellCount) => new()
    {
        BaseDimension = 4,
        AmbientDimension = 14,
        LieAlgebraDimension = 3,
        LieAlgebraId = "su2",
        MeshCellCount = cellCount,
        MeshVertexCount = cellCount * 2,
        ComponentOrderId = "order-row-major",
        TorsionBranchId = "trivial",
        ShiabBranchId = "identity",
    };
}
