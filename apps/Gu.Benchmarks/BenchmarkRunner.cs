using System.Diagnostics;
using System.Text.Json;
using Gu.Core;
using Gu.Geometry;
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

    /// <summary>
    /// Run a solve benchmark using a caller-supplied native backend.
    /// The caller is responsible for the lifetime of <paramref name="nativeBackend"/>;
    /// this method will NOT dispose it.
    /// </summary>
    public BenchmarkReport RunSolveBenchmarkWithBackend(
        string benchmarkId,
        int problemSize,
        SolverOptions solverOptions,
        INativeBackend nativeBackend)
    {
        ArgumentNullException.ThrowIfNull(nativeBackend);

        var sig = CreateSignature();
        var manifest = CreateManifest();
        var geometry = CreateGeometry();
        var manifestSnapshot = CreateManifestSnapshot(problemSize);

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

        using var solverBackend = new CudaSolverBackend(nativeBackend, ownsNative: false);
        solverBackend.Initialize(manifestSnapshot);

        var orchestrator = new SolverOrchestrator(solverBackend, solverOptions);

        var sw = Stopwatch.StartNew();
        var result = orchestrator.Solve(omega, a0, manifest, geometry);
        sw.Stop();

        double perIter = result.Iterations > 0
            ? sw.Elapsed.TotalMilliseconds / result.Iterations
            : sw.Elapsed.TotalMilliseconds;

        return new BenchmarkReport
        {
            BenchmarkId = benchmarkId,
            Description = $"Solve benchmark: {problemSize} cells, mode={solverOptions.Mode}, backend={nativeBackend.Version.BackendId}",
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
    /// Run a parity benchmark using a real CUDA backend as the target when available.
    /// Falls back to CpuReferenceBackend if CUDA native library cannot be loaded.
    /// Uploads synthetic mesh topology and algebra data to the GPU backend before running.
    /// </summary>
    public (BenchmarkReport cpuReport, BenchmarkReport gpuReport, IReadOnlyList<ParityRecord> parityRecords)
        RunParityBenchmarkWithGpu(string benchmarkId, int problemSize)
    {
        var manifestSnapshot = CreateManifestSnapshot(problemSize);
        int dimG = manifestSnapshot.LieAlgebraDimension;

        // Try to create CudaNativeBackend; fall back to CpuReferenceBackend if unavailable
        INativeBackend gpu;
        try
        {
            gpu = new CudaNativeBackend();
            _ = gpu.Version;
        }
        catch (Exception ex) when (ex is DllNotFoundException or EntryPointNotFoundException or TypeLoadException)
        {
            gpu = new CpuReferenceBackend();
        }

        try
        {
            string backendId = gpu.Version.BackendId;
            gpu.Initialize(manifestSnapshot);

            // Upload physics data so native kernels run real physics (not stubs)
            var topology = CreateSyntheticTopology(manifestSnapshot);
            var algebra = CreateSyntheticAlgebraData(dimG);
            gpu.UploadMeshTopology(topology);
            gpu.UploadAlgebraData(algebra);

            int edgeCount = topology.EdgeCount;
            int faceCount = topology.FaceCount;
            int edgeN = edgeCount * dimG;
            int faceN = faceCount * dimG;

            // Generate omega (edge-valued)
            var omegaData = new double[edgeN];
            var rng = new Random(42);
            for (int i = 0; i < edgeN; i++)
                omegaData[i] = 0.1 * (rng.NextDouble() - 0.5);

            // Upload zero background connection
            gpu.UploadBackgroundConnection(new double[edgeN], edgeCount, dimG);

            // Allocate properly-sized buffers: edge-valued for omega, face-valued for outputs
            var edgeLayout = BufferLayoutDescriptor.CreateSoA("edge", new[] { "c" }, edgeN);
            var faceLayout = BufferLayoutDescriptor.CreateSoA("face", new[] { "c" }, faceN);

            var omegaBuf = gpu.AllocateBuffer(edgeLayout);
            var curvBuf = gpu.AllocateBuffer(faceLayout);
            var torsionBuf = gpu.AllocateBuffer(faceLayout);
            var shiabBuf = gpu.AllocateBuffer(faceLayout);
            var residualBuf = gpu.AllocateBuffer(faceLayout);

            gpu.UploadBuffer(omegaBuf, omegaData);

            // Run full physics pipeline and self-validate
            var sw = Stopwatch.StartNew();

            gpu.EvaluateCurvature(omegaBuf, curvBuf);
            gpu.EvaluateTorsion(omegaBuf, torsionBuf);
            gpu.EvaluateShiab(omegaBuf, shiabBuf);
            gpu.EvaluateResidual(shiabBuf, torsionBuf, residualBuf);
            double objective = gpu.EvaluateObjective(residualBuf);

            sw.Stop();

            // Download results for self-consistency checks
            var curvData = new double[faceN];
            var torsionData = new double[faceN];
            var shiabData = new double[faceN];
            var residualData = new double[faceN];
            gpu.DownloadBuffer(curvBuf, curvData);
            gpu.DownloadBuffer(torsionBuf, torsionData);
            gpu.DownloadBuffer(shiabBuf, shiabData);
            gpu.DownloadBuffer(residualBuf, residualData);

            // Self-consistency parity checks (validated against physics invariants)
            var parityRecords = new List<ParityRecord>();

            // Identity shiab: S must equal F
            parityRecords.Add(ParityChecker.CompareResults(
                "shiab==curvature", curvData, shiabData, "curvature", "shiab", 1e-14));

            // Residual: Upsilon must equal S - T element-wise
            var expectedResidual = new double[faceN];
            for (int i = 0; i < faceN; i++)
                expectedResidual[i] = shiabData[i] - torsionData[i];
            parityRecords.Add(ParityChecker.CompareResults(
                "residual==S-T", expectedResidual, residualData, "manual", backendId, 1e-14));

            // Objective: I2 must equal 0.5 * sum(r_i^2)
            double expectedObj = 0.0;
            for (int i = 0; i < faceN; i++)
                expectedObj += residualData[i] * residualData[i];
            expectedObj *= 0.5;
            parityRecords.Add(ParityChecker.CompareScalar(
                "objective", expectedObj, objective, "manual", backendId, 1e-14));

            // Determinism: run pipeline again with same omega, compare residual
            var residualBuf2 = gpu.AllocateBuffer(faceLayout);
            var curvBuf2 = gpu.AllocateBuffer(faceLayout);
            var torsionBuf2 = gpu.AllocateBuffer(faceLayout);
            var shiabBuf2 = gpu.AllocateBuffer(faceLayout);
            gpu.EvaluateCurvature(omegaBuf, curvBuf2);
            gpu.EvaluateTorsion(omegaBuf, torsionBuf2);
            gpu.EvaluateShiab(omegaBuf, shiabBuf2);
            gpu.EvaluateResidual(shiabBuf2, torsionBuf2, residualBuf2);
            var residualData2 = new double[faceN];
            gpu.DownloadBuffer(residualBuf2, residualData2);
            parityRecords.Add(ParityChecker.CompareResults(
                "determinism", residualData, residualData2, "run1", "run2", 0.0));

            // Cleanup
            gpu.FreeBuffer(omegaBuf); gpu.FreeBuffer(curvBuf); gpu.FreeBuffer(torsionBuf);
            gpu.FreeBuffer(shiabBuf); gpu.FreeBuffer(residualBuf);
            gpu.FreeBuffer(curvBuf2); gpu.FreeBuffer(torsionBuf2);
            gpu.FreeBuffer(shiabBuf2); gpu.FreeBuffer(residualBuf2);

            bool allPassed = parityRecords.All(r => r.Passed);

            var cpuReport = new BenchmarkReport
            {
                BenchmarkId = $"{benchmarkId}-pipeline",
                Description = $"GPU pipeline benchmark: {problemSize} cells",
                BackendId = backendId,
                ProblemSize = problemSize,
                Iterations = 1,
                TotalTimeMs = sw.Elapsed.TotalMilliseconds,
                PerIterationTimeMs = sw.Elapsed.TotalMilliseconds,
                FinalObjective = objective,
                Converged = true,
                TerminationReason = "Pipeline complete",
            };

            var gpuReport = new BenchmarkReport
            {
                BenchmarkId = $"{benchmarkId}-validation",
                Description = $"GPU self-consistency: {problemSize} cells",
                BackendId = backendId,
                ProblemSize = problemSize,
                Iterations = 1,
                TotalTimeMs = sw.Elapsed.TotalMilliseconds,
                PerIterationTimeMs = sw.Elapsed.TotalMilliseconds,
                FinalObjective = objective,
                Converged = allPassed,
                TerminationReason = allPassed ? "All consistency checks passed" : "Consistency check failed",
            };

            return (cpuReport, gpuReport, parityRecords);
        }
        finally
        {
            gpu.Dispose();
        }
    }

    /// <summary>
    /// Create synthetic mesh topology data for benchmark use.
    /// Builds a simple chain of triangles with the given manifest dimensions.
    /// </summary>
    private static MeshTopologyData CreateSyntheticTopology(ManifestSnapshot manifest)
    {
        int cellCount = manifest.MeshCellCount;
        int vertexCount = manifest.MeshVertexCount;
        int dimG = manifest.LieAlgebraDimension;

        // Build a simple chain: each face is a triangle with 3 boundary edges
        int maxEdgesPerFace = 3;
        int edgeCount = cellCount * 3; // upper bound; real meshes share edges

        var faceBoundaryEdges = new int[cellCount * maxEdgesPerFace];
        var faceBoundaryOrientations = new int[cellCount * maxEdgesPerFace];
        var edgeVertices = new int[edgeCount * 2];

        for (int f = 0; f < cellCount; f++)
        {
            int baseEdge = f * 3;
            faceBoundaryEdges[f * 3 + 0] = baseEdge;
            faceBoundaryEdges[f * 3 + 1] = baseEdge + 1;
            faceBoundaryEdges[f * 3 + 2] = baseEdge + 2;
            faceBoundaryOrientations[f * 3 + 0] = 1;
            faceBoundaryOrientations[f * 3 + 1] = 1;
            faceBoundaryOrientations[f * 3 + 2] = -1;
        }

        for (int e = 0; e < edgeCount; e++)
        {
            int v0 = e % vertexCount;
            int v1 = (e + 1) % vertexCount;
            edgeVertices[e * 2] = System.Math.Min(v0, v1);
            edgeVertices[e * 2 + 1] = System.Math.Max(v0, v1);
        }

        return new MeshTopologyData
        {
            EdgeCount = edgeCount,
            FaceCount = cellCount,
            VertexCount = vertexCount,
            EmbeddingDimension = manifest.AmbientDimension,
            MaxEdgesPerFace = maxEdgesPerFace,
            DimG = dimG,
            FaceBoundaryEdges = faceBoundaryEdges,
            FaceBoundaryOrientations = faceBoundaryOrientations,
            EdgeVertices = edgeVertices,
        };
    }

    /// <summary>
    /// Create synthetic su(2) algebra data for benchmark use.
    /// Uses the standard epsilon structure constants and trace (identity) metric.
    /// </summary>
    private static AlgebraUploadData CreateSyntheticAlgebraData(int dimG)
    {
        var structureConstants = new double[dimG * dimG * dimG];

        // su(2) standard Levi-Civita structure constants: f^c_{ab} = epsilon_{abc}
        if (dimG == 3)
        {
            // f^c_{ab} indexed as [a * dim^2 + b * dim + c]
            SetAntiSymmetric(structureConstants, 3, 0, 1, 2, 1.0);
            SetAntiSymmetric(structureConstants, 3, 1, 2, 0, 1.0);
            SetAntiSymmetric(structureConstants, 3, 2, 0, 1, 1.0);
        }

        // Trace pairing (identity metric) for su(2)
        var metric = new double[dimG * dimG];
        for (int i = 0; i < dimG; i++)
            metric[i * dimG + i] = 1.0;

        return new AlgebraUploadData
        {
            Dimension = dimG,
            StructureConstants = structureConstants,
            InvariantMetric = metric,
        };
    }

    /// <summary>
    /// Set totally anti-symmetric structure constant f^c_{ab} and its permutations.
    /// </summary>
    private static void SetAntiSymmetric(double[] sc, int dim, int a, int b, int c, double value)
    {
        sc[a * dim * dim + b * dim + c] = value;
        sc[b * dim * dim + a * dim + c] = -value;
    }

    /// <summary>
    /// Run scaling benchmarks using real simplicial meshes of increasing size.
    /// Measures CPU residual evaluation (Mode A) at each scale.
    /// </summary>
    public IReadOnlyList<BenchmarkReport> RunMeshScalingBenchmark(
        string benchmarkId,
        int[] targetFaceCounts)
    {
        var reports = new List<BenchmarkReport>();

        foreach (int target in targetFaceCounts)
        {
            var mesh = SimplicialMeshGenerator.CreateUniform2D(target);
            int edgeCount = mesh.EdgeCount;
            int faceCount = mesh.FaceCount;
            int dimG = 3; // su(2)

            var manifestSnapshot = new ManifestSnapshot
            {
                BaseDimension = 2,
                AmbientDimension = 2,
                LieAlgebraDimension = dimG,
                LieAlgebraId = "su2",
                MeshCellCount = faceCount,
                MeshVertexCount = mesh.VertexCount,
                ComponentOrderId = "order-row-major",
                TorsionBranchId = "trivial",
                ShiabBranchId = "identity",
            };

            int edgeN = edgeCount * dimG;
            int faceN = faceCount * dimG;

            var omegaData = new double[edgeN];
            var rng = new Random(42);
            for (int i = 0; i < edgeN; i++)
                omegaData[i] = 0.1 * (rng.NextDouble() - 0.5);

            using var nativeBackend = new CpuReferenceBackend();
            nativeBackend.Initialize(manifestSnapshot);

            // Upload mesh topology and algebra
            var topology = CreateSyntheticTopologyFromMesh(mesh, dimG);
            var algebra = CreateSyntheticAlgebraData(dimG);
            nativeBackend.UploadMeshTopology(topology);
            nativeBackend.UploadAlgebraData(algebra);
            nativeBackend.UploadBackgroundConnection(new double[edgeN], edgeCount, dimG);

            var edgeLayout = BufferLayoutDescriptor.CreateSoA("edge", new[] { "c" }, edgeN);
            var faceLayout = BufferLayoutDescriptor.CreateSoA("face", new[] { "c" }, faceN);

            var omegaBuf = nativeBackend.AllocateBuffer(edgeLayout);
            var curvBuf = nativeBackend.AllocateBuffer(faceLayout);

            nativeBackend.UploadBuffer(omegaBuf, omegaData);

            // Time curvature evaluation
            var sw = Stopwatch.StartNew();
            nativeBackend.EvaluateCurvature(omegaBuf, curvBuf);
            sw.Stop();

            var curvData = new double[faceN];
            nativeBackend.DownloadBuffer(curvBuf, curvData);
            double norm = 0;
            for (int i = 0; i < faceN; i++)
                norm += curvData[i] * curvData[i];

            nativeBackend.FreeBuffer(omegaBuf);
            nativeBackend.FreeBuffer(curvBuf);

            reports.Add(new BenchmarkReport
            {
                BenchmarkId = $"{benchmarkId}-{faceCount}",
                Description = $"Mesh scaling: {faceCount} faces, {edgeCount} edges, {mesh.VertexCount} verts",
                BackendId = "cpu-reference",
                ProblemSize = faceCount,
                Iterations = 1,
                TotalTimeMs = sw.Elapsed.TotalMilliseconds,
                PerIterationTimeMs = sw.Elapsed.TotalMilliseconds,
                FinalObjective = 0.5 * norm,
                Converged = true,
                TerminationReason = "Curvature evaluation complete",
            });
        }

        return reports;
    }

    /// <summary>
    /// Create mesh topology data from a real SimplicialMesh.
    /// </summary>
    private static MeshTopologyData CreateSyntheticTopologyFromMesh(SimplicialMesh mesh, int dimG)
    {
        int faceCount = mesh.FaceCount;
        int edgeCount = mesh.EdgeCount;
        int maxEdgesPerFace = 3;

        var faceBoundaryEdges = new int[faceCount * maxEdgesPerFace];
        var faceBoundaryOrientations = new int[faceCount * maxEdgesPerFace];

        for (int f = 0; f < faceCount; f++)
        {
            for (int e = 0; e < maxEdgesPerFace; e++)
            {
                faceBoundaryEdges[f * maxEdgesPerFace + e] = mesh.FaceBoundaryEdges[f][e];
                faceBoundaryOrientations[f * maxEdgesPerFace + e] = mesh.FaceBoundaryOrientations[f][e];
            }
        }

        var edgeVertices = new int[edgeCount * 2];
        for (int e = 0; e < edgeCount; e++)
        {
            edgeVertices[e * 2] = mesh.Edges[e][0];
            edgeVertices[e * 2 + 1] = mesh.Edges[e][1];
        }

        return new MeshTopologyData
        {
            EdgeCount = edgeCount,
            FaceCount = faceCount,
            VertexCount = mesh.VertexCount,
            EmbeddingDimension = mesh.EmbeddingDimension,
            MaxEdgesPerFace = maxEdgesPerFace,
            DimG = dimG,
            FaceBoundaryEdges = faceBoundaryEdges,
            FaceBoundaryOrientations = faceBoundaryOrientations,
            EdgeVertices = edgeVertices,
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
