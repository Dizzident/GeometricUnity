using System.Text.Json;
using Gu.Artifacts;
using Gu.Core;
using Gu.Core.Factories;
using Gu.Core.Serialization;
using Gu.Geometry;
using Gu.Interop;
using Gu.Math;
using Gu.Observation;
using Gu.Phase3.Backgrounds;
using Gu.Phase3.Campaigns;
using Gu.Phase3.GaugeReduction;
using Gu.Phase3.ModeTracking;
using Gu.Phase3.Properties;
using Gu.Phase3.Registry;
using Gu.Phase3.Reporting;
using Gu.Phase3.Spectra;
using Gu.ReferenceCpu;
using Gu.Solvers;
using Gu.Validation;

if (args.Length == 0)
{
    PrintUsage();
    return 0;
}

switch (args[0])
{
    case "create-branch":
        return CreateBranch(args);
    case "create-environment":
        return CreateEnvironment(args);
    case "validate-replay":
        return ValidateReplay(args);
    case "verify-integrity":
        return VerifyIntegrity(args);
    case "init-run":
        return InitRunFolder(args);
    case "validate-schema":
        return ValidateSchema(args);
    case "run":
        return RunSolver(args);
    case "solve":
        return SolveCommand(args);
    case "reproduce":
        return Reproduce(args);
    case "sweep":
        return SweepBranches(args);
    case "create-background-study":
        return CreateBackgroundStudy(args);
    case "solve-backgrounds":
        return SolveBackgrounds(args);
    case "compute-spectrum":
        return ComputeSpectrum(args);
    case "track-modes":
        return TrackModes(args);
    case "build-boson-registry":
        return BuildBosonRegistry(args);
    case "run-boson-campaign":
        return RunBosonCampaign(args);
    case "export-boson-report":
        return ExportBosonReport(args);
    default:
        Console.Error.WriteLine($"Unknown command: {args[0]}");
        PrintUsage();
        return 1;
}

static int CreateBranch(string[] args)
{
    var branchId = args.Length > 1 ? args[1] : "minimal-gu-v1";
    var manifest = BranchManifestFactory.CreateEmpty(branchId);
    var json = GuJsonDefaults.Serialize(manifest);

    var outputPath = args.Length > 2 ? args[2] : $"{branchId}.branch.json";
    File.WriteAllText(outputPath, json);
    Console.WriteLine($"Created branch manifest: {outputPath}");
    return 0;
}

static int CreateEnvironment(string[] args)
{
    var envId = args.Length > 1 ? args[1] : "default-environment";
    var branchId = args.Length > 2 ? args[2] : "minimal-gu-v1";
    var spec = EnvironmentSpecFactory.CreateEmpty(envId, branchId);
    var json = GuJsonDefaults.Serialize(spec);

    var outputPath = args.Length > 3 ? args[3] : $"{envId}.environment.json";
    File.WriteAllText(outputPath, json);
    Console.WriteLine($"Created environment spec: {outputPath}");
    return 0;
}

static int ValidateReplay(string[] args)
{
    if (args.Length < 3)
    {
        Console.Error.WriteLine("Usage: gu validate-replay <original-run-folder> <replay-run-folder> [tier]");
        return 1;
    }

    var originalPath = args[1];
    var replayPath = args[2];
    var tier = args.Length > 3 ? args[3] : ReplayTiers.R2;

    if (!Directory.Exists(originalPath))
    {
        Console.Error.WriteLine($"Original run folder not found: {originalPath}");
        return 1;
    }
    if (!Directory.Exists(replayPath))
    {
        Console.Error.WriteLine($"Replay run folder not found: {replayPath}");
        return 1;
    }

    var originalFolder = new RunFolderWriter(originalPath);
    var replayFolder = new RunFolderWriter(replayPath);

    var report = ReplayContractValidator.Validate(originalFolder, replayFolder, tier);
    var json = GuJsonDefaults.Serialize(report);
    Console.WriteLine(json);

    Console.WriteLine();
    Console.WriteLine($"Outcome: {report.Outcome}");
    Console.WriteLine($"Tier: {report.ReplayTier}");
    Console.WriteLine($"Checks: {report.Checks.Count} total, {report.Checks.Count(c => c.Passed)} passed, {report.Checks.Count(c => !c.Passed)} failed");

    return report.Outcome == ReplayOutcome.Pass ? 0 : 1;
}

static int VerifyIntegrity(string[] args)
{
    if (args.Length < 2)
    {
        Console.Error.WriteLine("Usage: gu verify-integrity <run-folder>");
        return 1;
    }

    var runPath = args[1];
    if (!Directory.Exists(runPath))
    {
        Console.Error.WriteLine($"Run folder not found: {runPath}");
        return 1;
    }

    var writer = new RunFolderWriter(runPath);
    var hashManifest = writer.ReadJson<FileHashManifest>(RunFolderLayout.FileHashManifestFile);

    if (hashManifest is null)
    {
        // No stored hashes - compute and save them
        Console.WriteLine("No integrity hashes found. Computing...");
        hashManifest = IntegrityHasher.ComputeRunFolderHashes(runPath);
        var hashJson = GuJsonDefaults.Serialize(hashManifest);
        Directory.CreateDirectory(Path.Combine(runPath, RunFolderLayout.IntegrityDir));
        File.WriteAllText(Path.Combine(runPath, RunFolderLayout.FileHashManifestFile), hashJson);
        Console.WriteLine($"Computed hashes for {hashManifest.FileHashes.Count} files.");
        return 0;
    }

    var mismatches = IntegrityHasher.VerifyRunFolderHashes(runPath, hashManifest);
    if (mismatches.Count == 0)
    {
        Console.WriteLine($"Integrity verified: {hashManifest.FileHashes.Count} files match.");
        return 0;
    }

    Console.Error.WriteLine($"Integrity check failed with {mismatches.Count} issue(s):");
    foreach (var m in mismatches)
        Console.Error.WriteLine($"  {m}");
    return 1;
}

static int InitRunFolder(string[] args)
{
    if (args.Length < 2)
    {
        Console.Error.WriteLine("Usage: gu init-run <run-folder> [branchId]");
        return 1;
    }

    var runPath = args[1];
    var branchId = args.Length > 2 ? args[2] : "minimal-gu-v1";

    var writer = new RunFolderWriter(runPath);
    writer.CreateDirectories();

    var manifest = BranchManifestFactory.CreateEmpty(branchId);
    writer.WriteBranchManifest(manifest);

    var runtime = RuntimeInfo.CaptureCurrentEnvironment("cpu-reference");
    writer.WriteRuntime(runtime);

    writer.WriteEnvironmentLog($"Initialized: {DateTimeOffset.UtcNow:O}\nOS: {Environment.OSVersion}\nRuntime: .NET {Environment.Version}");

    Console.WriteLine($"Initialized run folder: {runPath}");
    Console.WriteLine($"  Branch: {branchId}");
    Console.WriteLine($"  Runtime: {runtime.RuntimeVersion}");
    return 0;
}

static int ValidateSchema(string[] args)
{
    if (args.Length < 3)
    {
        Console.Error.WriteLine("Usage: gu validate-schema <file> <schema>");
        Console.Error.WriteLine();
        Console.Error.WriteLine("  <file>   Path to the JSON file to validate");
        Console.Error.WriteLine("  <schema> Schema name (branch, geometry, artifact, observed, validation)");
        Console.Error.WriteLine("           or path to a .schema.json file");
        return 1;
    }

    var filePath = args[1];
    var schemaArg = args[2];

    if (!File.Exists(filePath))
    {
        Console.Error.WriteLine($"File not found: {filePath}");
        return 1;
    }

    var json = File.ReadAllText(filePath);

    SchemaValidationResult result;
    if (File.Exists(schemaArg))
    {
        result = SchemaValidator.ValidateWithSchemaFile(json, schemaArg);
    }
    else
    {
        result = SchemaValidator.Validate(json, schemaArg);
    }

    if (result.IsValid)
    {
        Console.WriteLine($"Valid: {filePath} conforms to schema '{result.SchemaName}'");
        return 0;
    }

    Console.Error.WriteLine($"Invalid: {filePath} does not conform to schema '{result.SchemaName}'");
    foreach (var error in result.Errors)
    {
        Console.Error.WriteLine($"  - {error}");
    }
    return 1;
}

static int RunSolver(string[] args)
{
    if (args.Length < 2)
    {
        Console.Error.WriteLine("Usage: gu run <run-folder> [--backend cpu|cuda] [--mode A|B|C|D] [--lie-algebra su2|su3] [--max-iter N] [--step-size S] [--branches b1.json,b2.json]");
        return 1;
    }

    var runPath = args[1];
    if (!Directory.Exists(runPath))
    {
        Console.Error.WriteLine($"Run folder not found: {runPath}");
        return 1;
    }

    // Parse flags
    var backend = ParseFlag(args, "--backend", "cpu");
    var mode = ParseFlag(args, "--mode", "A");
    var lieAlgebra = ParseFlag(args, "--lie-algebra", "su2");
    var maxIter = int.Parse(ParseFlag(args, "--max-iter", "100"));
    var stepSize = double.Parse(ParseFlag(args, "--step-size", "0.01"));

    if (backend != "cpu" && backend != "cuda")
    {
        Console.Error.WriteLine($"Backend '{backend}' is not supported. Supported: cpu, cuda");
        return 1;
    }

    var writer = new RunFolderWriter(runPath);

    // Create geometry (toy 2D fiber bundle)
    var bundle = ToyGeometryFactory.CreateToy2D();
    var yMesh = bundle.AmbientMesh;
    var geometry = bundle.ToGeometryContext("centroid", "P1");

    // Create Lie algebra
    var algebra = CreateAlgebra(lieAlgebra);

    // Build manifest with correct settings for the run
    var manifest = new BranchManifest
    {
        BranchId = "minimal-gu-v1",
        SchemaVersion = "1.0.0",
        SourceEquationRevision = "r1",
        CodeRevision = "cli-run",
        LieAlgebraId = algebra.AlgebraId,
        BaseDimension = bundle.BaseMesh.EmbeddingDimension,
        AmbientDimension = bundle.AmbientMesh.EmbeddingDimension,
        ActiveGeometryBranch = "simplicial",
        ActiveObservationBranch = "sigma-pullback",
        ActiveTorsionBranch = "trivial",
        ActiveShiabBranch = "identity-shiab",
        ActiveGaugeStrategy = "penalty",
        PairingConventionId = algebra.PairingId == "trace" ? "pairing-trace" : "pairing-killing",
        BasisConventionId = "canonical",
        ComponentOrderId = "face-major",
        AdjointConventionId = "adjoint-explicit",
        NormConventionId = "norm-l2-quadrature",
        DifferentialFormMetricId = "hodge-standard",
        InsertedAssumptionIds = Array.Empty<string>(),
        InsertedChoiceIds = new[] { "IX-1", "IX-2" },
    };

    // Parse solve mode
    var solveMode = mode.ToUpperInvariant() switch
    {
        "A" => SolveMode.ResidualOnly,
        "B" => SolveMode.ObjectiveMinimization,
        "C" => SolveMode.StationaritySolve,
        "D" => SolveMode.BranchSensitivity,
        _ => SolveMode.ResidualOnly,
    };

    Console.WriteLine($"Running solver...");
    Console.WriteLine($"  Backend: {backend}");
    Console.WriteLine($"  Mode: {solveMode} ({mode})");
    Console.WriteLine($"  Lie algebra: {algebra.Label} (dim={algebra.Dimension})");
    Console.WriteLine($"  Mesh: Y_h {yMesh.VertexCount} vertices, {yMesh.EdgeCount} edges, {yMesh.FaceCount} faces");
    Console.WriteLine($"  Max iterations: {maxIter}");

    // Create branch operators and run pipeline
    var torsion = new TrivialTorsionCpu(yMesh, algebra);
    var shiab = new IdentityShiabCpu(yMesh, algebra);
    var pipeline = new CpuSolverPipeline(yMesh, algebra, torsion, shiab);

    // Mode D: branch sensitivity sweep
    if (solveMode == SolveMode.BranchSensitivity)
    {
        var branchesFlag = ParseFlag(args, "--branches", "");
        if (string.IsNullOrEmpty(branchesFlag))
        {
            Console.Error.WriteLine("Mode D requires --branches flag with comma-separated branch manifest JSON files.");
            return 1;
        }

        var branchFiles = branchesFlag.Split(',', StringSplitOptions.RemoveEmptyEntries);
        var branchManifests = new List<BranchManifest>();
        foreach (var file in branchFiles)
        {
            if (!File.Exists(file))
            {
                Console.Error.WriteLine($"Branch manifest file not found: {file}");
                return 1;
            }
            var json = File.ReadAllText(file);
            var m = GuJsonDefaults.Deserialize<BranchManifest>(json);
            if (m is null)
            {
                Console.Error.WriteLine($"Failed to deserialize branch manifest: {file}");
                return 1;
            }
            branchManifests.Add(m);
        }

        Console.WriteLine($"  Branches: {branchManifests.Count}");
        foreach (var m in branchManifests)
            Console.WriteLine($"    - {m.BranchId}");

        var innerOptions = new SolverOptions
        {
            Mode = SolveMode.ResidualOnly,
            MaxIterations = maxIter,
            InitialStepSize = stepSize,
        };

        var sweepResult = pipeline.ExecuteBranchSensitivity(
            null, null, branchManifests, geometry, innerOptions);

        Console.WriteLine();
        Console.WriteLine($"Branch sensitivity complete:");
        Console.WriteLine($"  Branches swept: {sweepResult.BranchCount}");
        Console.WriteLine($"  Converged: {string.Join(", ", sweepResult.ConvergedBranches)}");
        Console.WriteLine($"  Diverged: {string.Join(", ", sweepResult.DivergedBranches)}");
        Console.WriteLine($"  Best objective: {sweepResult.BestObjective:E6}");
        Console.WriteLine($"  Worst residual norm: {sweepResult.WorstResidualNorm:E6}");

        foreach (var br in sweepResult.BranchResults)
        {
            Console.WriteLine();
            Console.WriteLine($"  Branch: {br.Manifest.BranchId}");
            Console.WriteLine($"    Converged: {br.SolverResult.Converged}");
            Console.WriteLine($"    Iterations: {br.SolverResult.Iterations}");
            Console.WriteLine($"    Final objective: {br.SolverResult.FinalObjective:E6}");
            Console.WriteLine($"    Final residual norm: {br.SolverResult.FinalResidualNorm:E6}");
            Console.WriteLine($"    Termination: {br.SolverResult.TerminationReason}");
        }

        return 0;
    }

    var options = new SolverOptions
    {
        Mode = solveMode,
        MaxIterations = maxIter,
        InitialStepSize = stepSize,
    };

    PipelineResult result;
    string backendId;

    if (backend == "cuda")
    {
        // CUDA path: use CudaSolverBackend wrapping a native backend
        INativeBackend nativeBackend;
        try
        {
            nativeBackend = new CudaNativeBackend();
            _ = nativeBackend.Version; // Probe to verify native library loads
            Console.WriteLine("  CUDA native backend loaded successfully.");
        }
        catch (Exception ex) when (ex is DllNotFoundException or EntryPointNotFoundException or TypeLoadException)
        {
            Console.WriteLine($"  CUDA native library not available ({ex.GetType().Name}), falling back to CPU reference backend.");
            nativeBackend = new CpuReferenceBackend();
        }

        backendId = nativeBackend.Version.BackendId;

        // Initialize with manifest snapshot
        var snapshot = ManifestSnapshot.FromManifest(
            manifest, algebra.Dimension, yMesh.FaceCount, yMesh.VertexCount);
        nativeBackend.Initialize(snapshot);

        // Upload mesh topology
        int dimG = algebra.Dimension;
        int maxEdgesPerFace = 3;
        var faceBoundaryEdges = new int[yMesh.FaceCount * maxEdgesPerFace];
        var faceBoundaryOrientations = new int[yMesh.FaceCount * maxEdgesPerFace];
        for (int f = 0; f < yMesh.FaceCount; f++)
        {
            for (int e = 0; e < maxEdgesPerFace; e++)
            {
                faceBoundaryEdges[f * maxEdgesPerFace + e] = yMesh.FaceBoundaryEdges[f][e];
                faceBoundaryOrientations[f * maxEdgesPerFace + e] = yMesh.FaceBoundaryOrientations[f][e];
            }
        }
        var edgeVertices = new int[yMesh.EdgeCount * 2];
        for (int e = 0; e < yMesh.EdgeCount; e++)
        {
            edgeVertices[e * 2] = yMesh.Edges[e][0];
            edgeVertices[e * 2 + 1] = yMesh.Edges[e][1];
        }
        nativeBackend.UploadMeshTopology(new MeshTopologyData
        {
            EdgeCount = yMesh.EdgeCount,
            FaceCount = yMesh.FaceCount,
            VertexCount = yMesh.VertexCount,
            EmbeddingDimension = yMesh.EmbeddingDimension,
            MaxEdgesPerFace = maxEdgesPerFace,
            DimG = dimG,
            FaceBoundaryEdges = faceBoundaryEdges,
            FaceBoundaryOrientations = faceBoundaryOrientations,
            EdgeVertices = edgeVertices,
        });

        // Upload algebra data (structure constants and metric are already flat arrays)
        nativeBackend.UploadAlgebraData(new AlgebraUploadData
        {
            Dimension = dimG,
            StructureConstants = (double[])algebra.StructureConstants.Clone(),
            InvariantMetric = (double[])algebra.InvariantMetric.Clone(),
        });

        // Upload zero background connection
        int edgeN = yMesh.EdgeCount * dimG;
        nativeBackend.UploadBackgroundConnection(new double[edgeN], yMesh.EdgeCount, dimG);

        // Create CudaSolverBackend and run
        using var cudaBackend = new CudaSolverBackend(nativeBackend, ownsNative: true);
        cudaBackend.Initialize(snapshot);

        var orchestrator = new SolverOrchestrator(cudaBackend, options);

        // Create initial omega (zero connection)
        var omegaCoeffs = new double[edgeN];
        var sig = new TensorSignature
        {
            AmbientSpaceId = "Y_h",
            CarrierType = "connection-1form",
            Degree = "1",
            LieAlgebraBasisId = "basis-standard",
            ComponentOrderId = "face-major",
            MemoryLayout = "dense-row-major",
            NumericPrecision = "float64",
        };
        var omegaTensor = new FieldTensor
        {
            Label = "omega_h",
            Signature = sig,
            Coefficients = omegaCoeffs,
            Shape = new[] { yMesh.EdgeCount, dimG },
        };
        var a0Tensor = new FieldTensor
        {
            Label = "a0",
            Signature = sig,
            Coefficients = new double[edgeN],
            Shape = new[] { yMesh.EdgeCount, dimG },
        };

        var cudaSolverResult = orchestrator.Solve(omegaTensor, a0Tensor, manifest, geometry);

        // Package into PipelineResult for uniform downstream handling
        var now = DateTimeOffset.UtcNow;
        var cudaBranchRef = new BranchRef
        {
            BranchId = manifest.BranchId,
            SchemaVersion = manifest.SchemaVersion,
        };
        var provenance = new ProvenanceMeta
        {
            CreatedAt = now,
            CodeRevision = manifest.CodeRevision,
            Branch = cudaBranchRef,
            Backend = backendId,
            Notes = $"CUDA backend: {options.Mode}, {cudaSolverResult.Iterations} iterations, " +
                    $"converged={cudaSolverResult.Converged}, reason={cudaSolverResult.TerminationReason}",
        };
        var initialState = new DiscreteState
        {
            Branch = cudaBranchRef,
            Geometry = geometry,
            Omega = omegaTensor,
            Provenance = provenance,
        };
        var finalState = new DiscreteState
        {
            Branch = cudaBranchRef,
            Geometry = geometry,
            Omega = cudaSolverResult.FinalOmega,
            Provenance = provenance,
        };
        var replayContract = new ReplayContract
        {
            BranchManifest = manifest,
            Deterministic = true,
            BackendId = backendId,
            ReplayTier = "R2",
        };
        var artifactBundle = new ArtifactBundle
        {
            ArtifactId = $"cuda-solve-{manifest.BranchId}-{now:yyyyMMddHHmmss}",
            Branch = cudaBranchRef,
            ReplayContract = replayContract,
            Provenance = provenance,
            CreatedAt = now,
            InitialState = initialState,
            FinalState = finalState,
            DerivedState = cudaSolverResult.FinalDerivedState,
            Geometry = geometry,
        };

        result = new PipelineResult
        {
            SolverResult = cudaSolverResult,
            ArtifactBundle = artifactBundle,
            DiagnosticLog = new List<string>
            {
                $"Backend: {backendId}",
                $"Mode: {options.Mode}",
                $"Converged: {cudaSolverResult.Converged}",
                $"Iterations: {cudaSolverResult.Iterations}",
                $"Final objective: {cudaSolverResult.FinalObjective:E6}",
            },
            ConvergenceSummary = new ConvergenceSummary
            {
                TotalIterations = cudaSolverResult.Iterations,
                InitialObjective = cudaSolverResult.History.Count > 0 ? cudaSolverResult.History[0].Objective : 0,
                FinalObjective = cudaSolverResult.FinalObjective,
                ObjectiveReductionRatio = 0,
                FinalGradientNorm = cudaSolverResult.FinalGradientNorm,
                FinalGaugeViolation = cudaSolverResult.History.Count > 0 ? cudaSolverResult.History[^1].GaugeViolation : 0,
                IsStagnated = false,
                StagnationDetectedAtIteration = null,
            },
            FinalConnection = ConnectionField.Zero(yMesh, algebra),
            BiConnectionA = ConnectionField.Zero(yMesh, algebra),
            BiConnectionB = ConnectionField.Zero(yMesh, algebra),
        };
    }
    else
    {
        // CPU path: use CpuSolverPipeline
        backendId = "cpu-reference";
        result = pipeline.Execute(null, null, manifest, geometry, options);
    }

    var solverResult = result.SolverResult;

    Console.WriteLine();
    Console.WriteLine($"Solver complete:");
    Console.WriteLine($"  Converged: {solverResult.Converged}");
    Console.WriteLine($"  Iterations: {solverResult.Iterations}");
    Console.WriteLine($"  Final objective: {solverResult.FinalObjective:E6}");
    Console.WriteLine($"  Final residual norm: {solverResult.FinalResidualNorm:E6}");
    Console.WriteLine($"  Final gradient norm: {solverResult.FinalGradientNorm:E6}");
    Console.WriteLine($"  Termination: {solverResult.TerminationReason}");

    // Run observation pipeline
    var pullback = new PullbackOperator(bundle);
    var obsPipeline = new ObservationPipeline(
        pullback,
        Array.Empty<IDerivedObservableTransform>(),
        new DimensionlessNormalizationPolicy());

    var branchRef = new BranchRef
    {
        BranchId = manifest.BranchId,
        SchemaVersion = manifest.SchemaVersion,
    };
    var discreteState = new DiscreteState
    {
        Branch = branchRef,
        Geometry = geometry,
        Omega = solverResult.FinalOmega,
        Provenance = new ProvenanceMeta
        {
            CreatedAt = DateTimeOffset.UtcNow,
            CodeRevision = manifest.CodeRevision,
            Branch = branchRef,
        },
    };

    var requests = new[]
    {
        new ObservableRequest { ObservableId = "curvature", OutputType = OutputType.Quantitative },
        new ObservableRequest { ObservableId = "residual", OutputType = OutputType.Quantitative },
    };

    var observed = obsPipeline.Extract(
        solverResult.FinalDerivedState, discreteState, geometry, requests, manifest);

    Console.WriteLine($"  Observables: {observed.Observables.Count} extracted");
    foreach (var (id, snap) in observed.Observables)
    {
        var maxVal = snap.Values.Length > 0 ? snap.Values.Max(System.Math.Abs) : 0.0;
        Console.WriteLine($"    {id}: {snap.Values.Length} values, max|v|={maxVal:E6}");
    }

    // Write all artifacts to run folder
    writer.WriteArtifactBundle(result.ArtifactBundle);
    writer.WriteObservedState(observed);
    writer.WriteGeometry(geometry);

    var runtime = RuntimeInfo.CaptureCurrentEnvironment("cpu-reference");
    writer.WriteRuntime(runtime);

    // Write solver log
    var logPath = Path.Combine(runPath, RunFolderLayout.SolverLogFile);
    var logDir = Path.GetDirectoryName(logPath);
    if (logDir is not null)
        Directory.CreateDirectory(logDir);
    File.WriteAllLines(logPath, result.DiagnosticLog);

    // Compute integrity hashes
    var hashes = IntegrityHasher.ComputeRunFolderHashes(runPath);
    var hashJson = GuJsonDefaults.Serialize(hashes);
    File.WriteAllText(Path.Combine(runPath, RunFolderLayout.FileHashManifestFile), hashJson);

    // Generate reproduce.sh script
    var reproduceSh = Path.Combine(runPath, RunFolderLayout.ReplayDir, "reproduce.sh");
    var modeFlag = solveMode switch
    {
        SolveMode.ResidualOnly => "A",
        SolveMode.ObjectiveMinimization => "B",
        SolveMode.StationaritySolve => "C",
        SolveMode.BranchSensitivity => "D",
        _ => "A",
    };
    var scriptContent = $$"""
        #!/usr/bin/env bash
        # Reproduce this run. Generated by gu CLI.
        # Original run: {{runPath}}
        # Date: {{DateTimeOffset.UtcNow:O}}
        set -euo pipefail
        REPLAY_DIR="${1:-replay-$(date +%Y%m%d%H%M%S)}"
        SOLVE_ARGS=( \
            solve "$REPLAY_DIR" \
            --backend {{backend}} \
            --mode {{modeFlag}} \
            --lie-algebra {{lieAlgebra}} \
            --max-iter {{maxIter}} \
            --step-size {{stepSize}} \
        )
        # Try installed CLI first, then dotnet run from repo root
        if command -v gu &>/dev/null; then
            gu "${SOLVE_ARGS[@]}"
        else
            SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
            dotnet run --project "$SCRIPT_DIR/../../apps/Gu.Cli" -- "${SOLVE_ARGS[@]}"
        fi
        echo "Replay written to: $REPLAY_DIR"
        echo "To validate: dotnet run --project apps/Gu.Cli -- validate-replay {{runPath}} $REPLAY_DIR"
        """;
    File.WriteAllText(reproduceSh, scriptContent);

    Console.WriteLine();
    Console.WriteLine($"Artifacts written to: {runPath}");
    Console.WriteLine($"Integrity hashes: {hashes.FileHashes.Count} files");
    Console.WriteLine($"Reproduce script: {reproduceSh}");
    return 0;
}

static int SolveCommand(string[] args)
{
    if (args.Length < 2)
    {
        Console.Error.WriteLine("Usage: gu solve <run-folder> [--backend cpu|cuda] [--mode A|B|C|D] [--lie-algebra su2|su3] [--max-iter N] [--step-size S] [--branches b1.json,b2.json]");
        return 1;
    }

    var runPath = args[1];

    // Init the run folder first
    Console.WriteLine($"Initializing run folder: {runPath}");
    var initResult = InitRunFolder(new[] { "init-run", runPath });
    if (initResult != 0)
        return initResult;

    Console.WriteLine();

    // Then run the solver (pass through all original args, replacing "solve" with "run")
    var runArgs = new string[args.Length];
    runArgs[0] = "run";
    Array.Copy(args, 1, runArgs, 1, args.Length - 1);
    return RunSolver(runArgs);
}

static LieAlgebra CreateAlgebra(string id) => id.ToLowerInvariant() switch
{
    "su2" => LieAlgebraFactory.CreateSu2WithTracePairing(),
    "su3" => LieAlgebraFactory.CreateSu3(),
    _ => throw new ArgumentException($"Unknown Lie algebra: {id}. Supported: su2, su3"),
};

static string ParseFlag(string[] args, string flag, string defaultValue)
{
    for (int i = 0; i < args.Length - 1; i++)
    {
        if (args[i] == flag)
            return args[i + 1];
    }
    return defaultValue;
}

static int Reproduce(string[] args)
{
    if (args.Length < 2)
    {
        Console.Error.WriteLine("Usage: gu reproduce <original-run-folder> [replay-run-folder] [--validate]");
        return 1;
    }

    var originalPath = args[1];
    if (!Directory.Exists(originalPath))
    {
        Console.Error.WriteLine($"Original run folder not found: {originalPath}");
        return 1;
    }

    var replayPath = args.Length > 2 && !args[2].StartsWith("--")
        ? args[2]
        : $"{originalPath}-replay-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}";
    var shouldValidate = args.Contains("--validate");

    // Read the replay contract from the original run
    var originalWriter = new RunFolderWriter(originalPath);
    var contract = originalWriter.ReadJson<ReplayContract>(RunFolderLayout.ReplayContractFile);
    if (contract is null)
    {
        Console.Error.WriteLine("No replay contract found in original run folder.");
        return 1;
    }

    var manifest = contract.BranchManifest;
    Console.WriteLine($"Reproducing run from: {originalPath}");
    Console.WriteLine($"  Branch: {manifest.BranchId}");
    Console.WriteLine($"  Backend: {contract.BackendId}");
    Console.WriteLine($"  Replay tier: {contract.ReplayTier}");

    // Determine mode from the original manifest
    // Default to Mode A; a real implementation would store mode in contract
    var modeFlag = "A";
    var lieAlgebra = manifest.LieAlgebraId;

    // Map internal backend ID to CLI flag
    var backendFlag = contract.BackendId switch
    {
        "cpu-reference" => "cpu",
        _ => contract.BackendId,
    };

    // Re-run as a solve command with the same parameters
    var solveArgs = new List<string>
    {
        "solve", replayPath,
        "--backend", backendFlag,
        "--lie-algebra", lieAlgebra,
        "--mode", modeFlag,
    };

    var solveResult = SolveCommand(solveArgs.ToArray());
    if (solveResult != 0)
        return solveResult;

    // Optionally validate the replay
    if (shouldValidate)
    {
        Console.WriteLine();
        Console.WriteLine("Validating replay...");
        var validateArgs = new[] { "validate-replay", originalPath, replayPath, contract.ReplayTier };
        return ValidateReplay(validateArgs);
    }

    Console.WriteLine();
    Console.WriteLine($"Replay written to: {replayPath}");
    Console.WriteLine($"To validate: gu validate-replay {originalPath} {replayPath} {contract.ReplayTier}");
    return 0;
}

static int SweepBranches(string[] args)
{
    if (args.Length < 2)
    {
        Console.Error.WriteLine("Usage: gu sweep --branches b1.json,b2.json [--output <dir>] [--lie-algebra su2|su3] [--max-iter N] [--step-size S] [--mode A|B|C]");
        return 1;
    }

    var branchesFlag = ParseFlag(args, "--branches", "");
    if (string.IsNullOrEmpty(branchesFlag))
    {
        Console.Error.WriteLine("--branches flag is required with comma-separated branch manifest JSON files.");
        return 1;
    }

    var outputDir = ParseFlag(args, "--output", "sweep-" + DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss"));
    var lieAlgebra = ParseFlag(args, "--lie-algebra", "su2");
    var maxIter = int.Parse(ParseFlag(args, "--max-iter", "100"));
    var stepSize = double.Parse(ParseFlag(args, "--step-size", "0.01"));
    var innerMode = ParseFlag(args, "--mode", "A").ToUpperInvariant() switch
    {
        "B" => SolveMode.ObjectiveMinimization,
        "C" => SolveMode.StationaritySolve,
        _ => SolveMode.ResidualOnly,
    };

    var branchFiles = branchesFlag.Split(',', StringSplitOptions.RemoveEmptyEntries);
    var manifests = new List<BranchManifest>();
    foreach (var file in branchFiles)
    {
        if (!File.Exists(file))
        {
            Console.Error.WriteLine($"Branch manifest file not found: {file}");
            return 1;
        }
        var json = File.ReadAllText(file);
        var m = GuJsonDefaults.Deserialize<BranchManifest>(json);
        if (m is null)
        {
            Console.Error.WriteLine($"Failed to deserialize branch manifest: {file}");
            return 1;
        }
        manifests.Add(m);
    }

    if (manifests.Count < 2)
    {
        Console.Error.WriteLine("Branch sweep requires at least 2 branch manifests.");
        return 1;
    }

    var algebra = CreateAlgebra(lieAlgebra);
    var bundle = ToyGeometryFactory.CreateToy2D();
    var geometry = bundle.ToGeometryContext("centroid", "P1");

    Console.WriteLine("Branch sensitivity sweep");
    Console.WriteLine($"  Inner mode: {innerMode}");
    Console.WriteLine($"  Lie algebra: {algebra.Label} (dim={algebra.Dimension})");
    Console.WriteLine($"  Branches: {manifests.Count}");
    foreach (var m in manifests)
        Console.WriteLine($"    - {m.BranchId}");

    var torsion = new TrivialTorsionCpu(bundle.AmbientMesh, algebra);
    var shiab = new IdentityShiabCpu(bundle.AmbientMesh, algebra);
    var pipeline = new CpuSolverPipeline(bundle.AmbientMesh, algebra, torsion, shiab);

    var innerOptions = new SolverOptions
    {
        Mode = innerMode,
        MaxIterations = maxIter,
        InitialStepSize = stepSize,
    };

    var sweepResult = pipeline.ExecuteBranchSweep(null, null, manifests, geometry, innerOptions);

    // Write results to output directory
    Directory.CreateDirectory(outputDir);
    var sweepJson = GuJsonDefaults.Serialize(sweepResult);
    File.WriteAllText(Path.Combine(outputDir, "sweep_result.json"), sweepJson);

    // Write per-branch artifact bundles
    foreach (var entry in sweepResult.Entries)
    {
        var branchDir = Path.Combine(outputDir, entry.Manifest.BranchId);
        var writer = new RunFolderWriter(branchDir);
        writer.WriteArtifactBundle(entry.ArtifactBundle);
    }

    Console.WriteLine();
    Console.WriteLine($"Sweep complete:");
    Console.WriteLine($"  Branches: {sweepResult.BranchCount}");
    Console.WriteLine($"  Converged: {string.Join(", ", sweepResult.ConvergedBranches)}");
    Console.WriteLine($"  Diverged: {string.Join(", ", sweepResult.DivergedBranches)}");
    Console.WriteLine($"  Output: {outputDir}");

    foreach (var entry in sweepResult.Entries)
    {
        Console.WriteLine();
        Console.WriteLine($"  Branch: {entry.Manifest.BranchId}");
        Console.WriteLine($"    Converged: {entry.Converged}");
        Console.WriteLine($"    Iterations: {entry.Iterations}");
        Console.WriteLine($"    Final objective: {entry.FinalObjective:E6}");
        Console.WriteLine($"    Final residual: {entry.FinalResidualNorm:E6}");
        Console.WriteLine($"    Termination: {entry.TerminationReason}");
    }

    return 0;
}

static int CreateBackgroundStudy(string[] args)
{
    var outputPath = args.Length > 1 ? args[1] : "background-study.json";
    var lieAlgebra = ParseFlag(args, "--lie-algebra", "su2");
    var mode = ParseFlag(args, "--mode", "B");
    var seedCount = int.Parse(ParseFlag(args, "--seeds", "3"));

    var solveMode = mode.ToUpperInvariant() switch
    {
        "A" => SolveMode.ResidualOnly,
        "C" => SolveMode.StationaritySolve,
        _ => SolveMode.ObjectiveMinimization,
    };

    var specs = new List<BackgroundSpec>();
    for (int i = 0; i < seedCount; i++)
    {
        specs.Add(new BackgroundSpec
        {
            SpecId = $"bg-spec-{i}",
            EnvironmentId = "env-default",
            BranchManifestId = "minimal-gu-v1",
            Seed = new BackgroundSeed
            {
                Kind = i == 0 ? BackgroundSeedKind.Trivial : BackgroundSeedKind.SymmetricAnsatz,
                Label = $"seed-{i}",
            },
            SolveOptions = new BackgroundSolveOptions
            {
                SolveMode = solveMode,
                MaxIterations = 200,
                ToleranceResidualDiagnostic = 1e-4,
                ToleranceStationary = 1e-6,
                ToleranceResidualStrict = 1e-8,
            },
        });
    }

    var study = new BackgroundStudySpec
    {
        StudyId = $"bg-study-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}",
        Specs = specs,
    };

    BackgroundAtlasSerializer.WriteStudy(study, outputPath);
    Console.WriteLine($"Created background study: {outputPath}");
    Console.WriteLine($"  Specs: {specs.Count}");
    Console.WriteLine($"  Solve mode: {solveMode}");
    return 0;
}

static int SolveBackgrounds(string[] args)
{
    if (args.Length < 2)
    {
        Console.Error.WriteLine("Usage: gu solve-backgrounds <study.json> [--output <dir>] [--lie-algebra su2|su3]");
        return 1;
    }

    var studyPath = args[1];
    if (!File.Exists(studyPath))
    {
        Console.Error.WriteLine($"Study file not found: {studyPath}");
        return 1;
    }

    var outputDir = ParseFlag(args, "--output", Path.Combine(Path.GetDirectoryName(studyPath) ?? ".", "backgrounds"));
    var lieAlgebra = ParseFlag(args, "--lie-algebra", "su2");

    var studyJson = File.ReadAllText(studyPath);
    var study = BackgroundAtlasSerializer.DeserializeStudy(studyJson);
    if (study is null)
    {
        Console.Error.WriteLine("Failed to deserialize background study.");
        return 1;
    }

    Console.WriteLine($"Solving background study: {study.StudyId}");
    Console.WriteLine($"  Specs: {study.Specs.Count}");

    // Set up geometry and solver
    var algebra = CreateAlgebra(lieAlgebra);
    var bundle = ToyGeometryFactory.CreateToy2D();
    var mesh = bundle.AmbientMesh;
    var geometry = bundle.ToGeometryContext("centroid", "P1");
    var torsion = new TrivialTorsionCpu(mesh, algebra);
    var shiab = new IdentityShiabCpu(mesh, algebra);
    var backend = new CpuSolverBackend(mesh, algebra, torsion, shiab);

    int edgeN = mesh.EdgeCount * algebra.Dimension;
    var a0 = new FieldTensor
    {
        Label = "a0",
        Signature = new TensorSignature
        {
            AmbientSpaceId = "Y_h",
            CarrierType = "connection-1form",
            Degree = "1",
            LieAlgebraBasisId = "canonical",
            ComponentOrderId = "edge-major",
            MemoryLayout = "dense-row-major",
        },
        Coefficients = new double[edgeN],
        Shape = new[] { mesh.EdgeCount, algebra.Dimension },
    };

    var manifest = new BranchManifest
    {
        BranchId = "minimal-gu-v1",
        SchemaVersion = "1.0.0",
        SourceEquationRevision = "r1",
        CodeRevision = "cli-bg-study",
        LieAlgebraId = algebra.AlgebraId,
        BaseDimension = bundle.BaseMesh.EmbeddingDimension,
        AmbientDimension = bundle.AmbientMesh.EmbeddingDimension,
        ActiveGeometryBranch = "simplicial",
        ActiveObservationBranch = "sigma-pullback",
        ActiveTorsionBranch = "trivial",
        ActiveShiabBranch = "identity-shiab",
        ActiveGaugeStrategy = "penalty",
        PairingConventionId = algebra.PairingId == "trace" ? "pairing-trace" : "pairing-killing",
        BasisConventionId = "canonical",
        ComponentOrderId = "face-major",
        AdjointConventionId = "adjoint-explicit",
        NormConventionId = "norm-l2-quadrature",
        DifferentialFormMetricId = "hodge-standard",
        InsertedAssumptionIds = Array.Empty<string>(),
        InsertedChoiceIds = new[] { "IX-1", "IX-2" },
    };

    var branchRef = new BranchRef
    {
        BranchId = manifest.BranchId,
        SchemaVersion = manifest.SchemaVersion,
    };
    var provenance = new ProvenanceMeta
    {
        CreatedAt = DateTimeOffset.UtcNow,
        CodeRevision = manifest.CodeRevision,
        Branch = branchRef,
        Backend = "cpu-reference",
    };

    var manifests = new Dictionary<string, BranchManifest> { [manifest.BranchId] = manifest };
    var geometries = new Dictionary<string, GeometryContext> { ["env-default"] = geometry };
    var a0s = new Dictionary<string, FieldTensor> { ["env-default"] = a0 };

    var builder = new BackgroundAtlasBuilder(backend);
    var atlas = builder.Build(study, manifests, geometries, a0s, provenance);

    // Write atlas
    Directory.CreateDirectory(outputDir);
    var atlasPath = Path.Combine(outputDir, "atlas.json");
    BackgroundAtlasSerializer.WriteAtlas(atlas, atlasPath);

    // Write individual records
    var recordsDir = Path.Combine(outputDir, "background_records");
    Directory.CreateDirectory(recordsDir);
    foreach (var bg in atlas.Backgrounds)
    {
        BackgroundAtlasSerializer.WriteRecord(bg, Path.Combine(recordsDir, $"{bg.BackgroundId}.json"));
    }
    foreach (var bg in atlas.RejectedBackgrounds)
    {
        BackgroundAtlasSerializer.WriteRecord(bg, Path.Combine(recordsDir, $"{bg.BackgroundId}.json"));
    }

    Console.WriteLine();
    Console.WriteLine($"Background atlas built:");
    Console.WriteLine($"  Atlas ID: {atlas.AtlasId}");
    Console.WriteLine($"  Total attempts: {atlas.TotalAttempts}");
    Console.WriteLine($"  Admitted: {atlas.Backgrounds.Count}");
    Console.WriteLine($"  Rejected: {atlas.RejectedBackgrounds.Count}");
    foreach (var (level, count) in atlas.AdmissibilityCounts)
        Console.WriteLine($"    {level}: {count}");

    foreach (var bg in atlas.Backgrounds)
    {
        Console.WriteLine($"  {bg.BackgroundId}: {bg.AdmissibilityLevel} " +
                          $"residual={bg.ResidualNorm:E6} stationarity={bg.StationarityNorm:E6}");
    }

    Console.WriteLine();
    Console.WriteLine($"Atlas written to: {atlasPath}");
    Console.WriteLine($"Records written to: {recordsDir}");
    return 0;
}

static int ComputeSpectrum(string[] args)
{
    if (args.Length < 3)
    {
        Console.Error.WriteLine("Usage: gu compute-spectrum <runFolder> <backgroundId> [--num-modes N] [--formulation p1|p2]");
        return 1;
    }

    var runFolder = args[1];
    var backgroundId = args[2];
    var numModes = int.Parse(ParseFlag(args, "--num-modes", "10"));
    var formulationFlag = ParseFlag(args, "--formulation", "p1");

    var formulation = formulationFlag.ToLowerInvariant() switch
    {
        "p2" => PhysicalModeFormulation.ProjectedComplement,
        _ => PhysicalModeFormulation.PenaltyFixed,
    };

    // Load background record
    var bgPath = Path.Combine(runFolder, "backgrounds", $"{backgroundId}.json");
    if (!File.Exists(bgPath))
    {
        // Try background_records subfolder
        bgPath = Path.Combine(runFolder, "backgrounds", "background_records", $"{backgroundId}.json");
    }
    if (!File.Exists(bgPath))
    {
        Console.Error.WriteLine($"Background not found: {bgPath}");
        return 1;
    }

    var bgJson = File.ReadAllText(bgPath);
    var bgRecord = GuJsonDefaults.Deserialize<BackgroundRecord>(bgJson);
    if (bgRecord is null)
    {
        Console.Error.WriteLine($"Failed to deserialize background: {bgPath}");
        return 1;
    }

    Console.WriteLine($"Computing spectrum for background: {backgroundId}");
    Console.WriteLine($"  Admissibility: {bgRecord.AdmissibilityLevel}");
    Console.WriteLine($"  Formulation: {formulation}");
    Console.WriteLine($"  Num modes: {numModes}");

    // Set up geometry and solver (same as solve-backgrounds)
    var lieAlgebra = ParseFlag(args, "--lie-algebra", "su2");
    var algebra = CreateAlgebra(lieAlgebra);
    var bundle = ToyGeometryFactory.CreateToy2D();
    var mesh = bundle.AmbientMesh;
    var geometry = bundle.ToGeometryContext("centroid", "P1");

    var manifest = new BranchManifest
    {
        BranchId = bgRecord.BranchManifestId,
        SchemaVersion = "1.0.0",
        SourceEquationRevision = "r1",
        CodeRevision = "cli-spectrum",
        LieAlgebraId = algebra.AlgebraId,
        BaseDimension = bundle.BaseMesh.EmbeddingDimension,
        AmbientDimension = bundle.AmbientMesh.EmbeddingDimension,
        ActiveGeometryBranch = "simplicial",
        ActiveObservationBranch = "sigma-pullback",
        ActiveTorsionBranch = "trivial",
        ActiveShiabBranch = "identity-shiab",
        ActiveGaugeStrategy = "penalty",
        PairingConventionId = algebra.PairingId == "trace" ? "pairing-trace" : "pairing-killing",
        BasisConventionId = "canonical",
        ComponentOrderId = "face-major",
        AdjointConventionId = "adjoint-explicit",
        NormConventionId = "norm-l2-quadrature",
        DifferentialFormMetricId = "hodge-standard",
        InsertedAssumptionIds = Array.Empty<string>(),
        InsertedChoiceIds = new[] { "IX-1", "IX-2" },
    };

    int edgeN = mesh.EdgeCount * algebra.Dimension;
    var sig = new TensorSignature
    {
        AmbientSpaceId = "Y_h",
        CarrierType = "connection-1form",
        Degree = "1",
        LieAlgebraBasisId = "canonical",
        ComponentOrderId = "edge-major",
        MemoryLayout = "dense-row-major",
    };
    var omega = new FieldTensor
    {
        Label = "omega_h",
        Signature = sig,
        Coefficients = new double[edgeN],
        Shape = new[] { mesh.EdgeCount, algebra.Dimension },
    };
    var a0 = new FieldTensor
    {
        Label = "a0",
        Signature = sig,
        Coefficients = new double[edgeN],
        Shape = new[] { mesh.EdgeCount, algebra.Dimension },
    };

    // Build operator bundle
    var torsion = new TrivialTorsionCpu(mesh, algebra);
    var shiab = new IdentityShiabCpu(mesh, algebra);
    var backend = new CpuSolverBackend(mesh, algebra, torsion, shiab);
    var assembler = new CpuResidualAssembler(mesh, algebra, torsion, shiab);
    var residualMass = new CpuMassMatrix(mesh, algebra);

    var operatorType = bgRecord.AdmissibilityLevel == AdmissibilityLevel.B2
        ? SpectralOperatorType.GaussNewton
        : SpectralOperatorType.FullHessian;

    var opSpec = new LinearizedOperatorSpec
    {
        BackgroundId = backgroundId,
        OperatorType = operatorType,
        Formulation = formulation,
        BackgroundAdmissibility = bgRecord.AdmissibilityLevel,
    };

    var bundleBuilder = new OperatorBundleBuilder(mesh, algebra, assembler, residualMass, backend);
    var opBundle = bundleBuilder.Build(opSpec, omega, a0, manifest, geometry);

    Console.WriteLine($"  Operator bundle: {opBundle.BundleId}");
    Console.WriteLine($"  Operator type: {opBundle.OperatorType}");
    Console.WriteLine($"  State dimension: {opBundle.StateDimension}");

    // Solve eigenproblem
    var eigSpec = new GeneralizedEigenproblemSpec
    {
        NumEigenvalues = numModes,
    };

    var pipeline = new EigensolverPipeline();
    var spectrum = pipeline.Solve(opBundle, eigSpec);

    Console.WriteLine($"  Convergence: {spectrum.ConvergenceStatus}");
    Console.WriteLine($"  Modes found: {spectrum.Modes.Count}");

    foreach (var mode in spectrum.Modes)
    {
        Console.WriteLine($"    mode {mode.ModeIndex}: eigenvalue={mode.Eigenvalue:E6} " +
                          $"residual={mode.ResidualNorm:E6} leak={mode.GaugeLeakScore:F4}");
    }

    // Write spectrum bundle
    var spectraDir = Path.Combine(runFolder, "spectra");
    Directory.CreateDirectory(spectraDir);
    var spectrumPath = Path.Combine(spectraDir, $"{backgroundId}_spectrum.json");
    File.WriteAllText(spectrumPath, GuJsonDefaults.Serialize(spectrum));

    // Write individual mode records
    var modesDir = Path.Combine(spectraDir, "modes");
    Directory.CreateDirectory(modesDir);
    foreach (var mode in spectrum.Modes)
    {
        var modePath = Path.Combine(modesDir, $"{mode.ModeId}.json");
        File.WriteAllText(modePath, GuJsonDefaults.Serialize(mode));
    }

    Console.WriteLine();
    Console.WriteLine($"Spectrum written to: {spectrumPath}");
    Console.WriteLine($"Mode records written to: {modesDir}");
    return 0;
}

static int TrackModes(string[] args)
{
    if (args.Length < 2)
    {
        Console.Error.WriteLine("Usage: gu track-modes <runFolder> [--context continuation|branch|refinement]");
        return 1;
    }

    var runFolder = args[1];
    var contextFlag = ParseFlag(args, "--context", "continuation");

    var contextType = contextFlag.ToLowerInvariant() switch
    {
        "branch" => TrackingContextType.BranchSweep,
        "refinement" => TrackingContextType.Refinement,
        _ => TrackingContextType.Continuation,
    };

    // Load all spectrum bundles
    var spectraDir = Path.Combine(runFolder, "spectra");
    if (!Directory.Exists(spectraDir))
    {
        Console.Error.WriteLine($"Spectra directory not found: {spectraDir}");
        return 1;
    }

    var spectrumFiles = Directory.GetFiles(spectraDir, "*_spectrum.json");
    if (spectrumFiles.Length == 0)
    {
        Console.Error.WriteLine($"No spectrum bundles found in: {spectraDir}");
        return 1;
    }

    var spectra = new List<SpectrumBundle>();
    foreach (var file in spectrumFiles.OrderBy(f => f))
    {
        var json = File.ReadAllText(file);
        var sb = GuJsonDefaults.Deserialize<SpectrumBundle>(json);
        if (sb != null)
            spectra.Add(sb);
    }

    Console.WriteLine($"Mode tracking: {contextType}");
    Console.WriteLine($"  Spectra loaded: {spectra.Count}");

    if (spectra.Count < 2)
    {
        Console.Error.WriteLine("Need at least 2 spectra for mode tracking.");
        return 1;
    }

    var config = new TrackingConfig { ContextType = contextType };
    var engine = new ModeMatchingEngine(config);

    IReadOnlyList<ModeFamilyRecord> families;

    if (contextType == TrackingContextType.BranchSweep)
    {
        // Cross-branch matching: match each pair against the first spectrum
        var reference = spectra[0];
        var allAlignments = new List<ModeAlignmentRecord>();
        foreach (var target in spectra.Skip(1))
        {
            var alignments = engine.Match(reference, target);
            allAlignments.AddRange(alignments);
        }
        var crossMap = new CrossBranchModeMap
        {
            SourceBranchId = spectra[0].BackgroundId,
            TargetBranchId = spectra[^1].BackgroundId,
            Alignments = allAlignments,
        };

        Console.WriteLine($"  Matched: {crossMap.MatchedCount}");
        Console.WriteLine($"  Births: {crossMap.BirthCount}");
        Console.WriteLine($"  Deaths: {crossMap.DeathCount}");
        Console.WriteLine($"  Splits: {crossMap.SplitCount}");
        Console.WriteLine($"  Merges: {crossMap.MergeCount}");

        // Build families from the continuation path for output consistency
        families = engine.BuildFamilies(spectra);
    }
    else
    {
        // Continuation / refinement: build families along path
        families = engine.BuildFamilies(spectra);
    }

    Console.WriteLine($"  Families built: {families.Count}");
    Console.WriteLine($"  Stable families: {families.Count(f => f.IsStable)}");

    // Write mode families
    var modesDir = Path.Combine(runFolder, "modes");
    Directory.CreateDirectory(modesDir);
    var familiesPath = Path.Combine(modesDir, "mode_families.json");
    File.WriteAllText(familiesPath, GuJsonDefaults.Serialize(families));

    foreach (var fam in families)
    {
        Console.WriteLine($"    {fam.FamilyId}: {fam.MemberModeIds.Count} members, " +
                          $"mean_eig={fam.MeanEigenvalue:E6}, stable={fam.IsStable}, " +
                          $"ambiguity={fam.AmbiguityCount}");
    }

    Console.WriteLine();
    Console.WriteLine($"Families written to: {familiesPath}");
    return 0;
}

static int BuildBosonRegistry(string[] args)
{
    if (args.Length < 2)
    {
        Console.Error.WriteLine("Usage: gu build-boson-registry <runFolder>");
        return 1;
    }

    var runFolder = args[1];

    // Load mode families
    var familiesPath = Path.Combine(runFolder, "modes", "mode_families.json");
    if (!File.Exists(familiesPath))
    {
        Console.Error.WriteLine($"Mode families not found: {familiesPath}");
        return 1;
    }

    var familiesJson = File.ReadAllText(familiesPath);
    var families = GuJsonDefaults.Deserialize<List<ModeFamilyRecord>>(familiesJson);
    if (families is null || families.Count == 0)
    {
        Console.Error.WriteLine("Failed to deserialize mode families or empty list.");
        return 1;
    }

    // Load spectrum bundles for property lookup
    var spectraDir = Path.Combine(runFolder, "spectra");
    var spectraByBackground = new Dictionary<string, SpectrumBundle>();
    if (Directory.Exists(spectraDir))
    {
        foreach (var file in Directory.GetFiles(spectraDir, "*_spectrum.json"))
        {
            var json = File.ReadAllText(file);
            var sb = GuJsonDefaults.Deserialize<SpectrumBundle>(json);
            if (sb != null)
                spectraByBackground[sb.BackgroundId] = sb;
        }
    }

    // Load property vectors if present
    Dictionary<string, Gu.Phase3.Properties.BosonPropertyVector>? propertyVectors = null;
    var propsDir = Path.Combine(runFolder, "properties");
    if (Directory.Exists(propsDir))
    {
        propertyVectors = new Dictionary<string, Gu.Phase3.Properties.BosonPropertyVector>();
        foreach (var file in Directory.GetFiles(propsDir, "*.json"))
        {
            var json = File.ReadAllText(file);
            var pv = GuJsonDefaults.Deserialize<Gu.Phase3.Properties.BosonPropertyVector>(json);
            if (pv != null)
                propertyVectors[pv.ModeId] = pv;
        }
    }

    Console.WriteLine($"Building boson registry...");
    Console.WriteLine($"  Families: {families.Count}");
    Console.WriteLine($"  Spectra: {spectraByBackground.Count}");
    Console.WriteLine($"  Property vectors: {propertyVectors?.Count ?? 0}");

    var builder = new CandidateBosonBuilder();
    var candidates = builder.BuildFromFamilies(families, spectraByBackground, propertyVectors);

    var registry = new BosonRegistry();
    foreach (var candidate in candidates)
        registry.Register(candidate);

    Console.WriteLine($"  Candidates: {registry.Count}");
    foreach (var cls in Enum.GetValues<BosonClaimClass>())
    {
        int count = registry.CountAboveClass(cls);
        if (count > 0)
            Console.WriteLine($"    >= {cls}: {count}");
    }

    // Write registry
    var bosonsDir = Path.Combine(runFolder, "bosons");
    Directory.CreateDirectory(bosonsDir);
    var registryPath = Path.Combine(bosonsDir, "registry.json");
    File.WriteAllText(registryPath, registry.ToJson());

    Console.WriteLine();
    Console.WriteLine($"Registry written to: {registryPath}");
    return 0;
}

static int RunBosonCampaign(string[] args)
{
    if (args.Length < 2)
    {
        Console.Error.WriteLine("Usage: gu run-boson-campaign <runFolder> [--campaign <campaignSpec.json>]");
        return 1;
    }

    var runFolder = args[1];
    var campaignSpecPath = ParseFlag(args, "--campaign", "");

    // Load registry
    var registryPath = Path.Combine(runFolder, "bosons", "registry.json");
    if (!File.Exists(registryPath))
    {
        Console.Error.WriteLine($"Registry not found: {registryPath}");
        return 1;
    }

    var registryJson = File.ReadAllText(registryPath);
    var registry = BosonRegistry.FromJson(registryJson);

    // Load or create campaign spec
    BosonCampaignSpec campaignSpec;
    if (!string.IsNullOrEmpty(campaignSpecPath) && File.Exists(campaignSpecPath))
    {
        var specJson = File.ReadAllText(campaignSpecPath);
        var spec = GuJsonDefaults.Deserialize<BosonCampaignSpec>(specJson);
        if (spec is null)
        {
            Console.Error.WriteLine($"Failed to deserialize campaign spec: {campaignSpecPath}");
            return 1;
        }
        campaignSpec = spec;
    }
    else
    {
        // Generate default campaign spec (BC1 with standard target profiles)
        campaignSpec = new BosonCampaignSpec
        {
            CampaignId = $"campaign-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}",
            Mode = BosonComparisonMode.InternalTargetProfile,
            TargetProfileIds = new[] { "massless-vector", "massive-scalar" },
            MinClaimClass = BosonClaimClass.C0_NumericalMode,
            IncludeDemoted = true,
        };
    }

    Console.WriteLine($"Running boson campaign: {campaignSpec.CampaignId}");
    Console.WriteLine($"  Mode: {campaignSpec.Mode}");
    Console.WriteLine($"  Registry candidates: {registry.Count}");

    // Create default target profiles for BC1
    var profiles = new Dictionary<string, BosonTargetProfile>
    {
        ["massless-vector"] = new BosonTargetProfile
        {
            ProfileId = "massless-vector",
            Label = "Massless vector-like",
            MassRange = new[] { 0.0, 0.01 },
            MaxGaugeLeak = 0.1,
        },
        ["massive-scalar"] = new BosonTargetProfile
        {
            ProfileId = "massive-scalar",
            Label = "Massive scalar-like",
            MassRange = new[] { 0.1, 100.0 },
            MaxGaugeLeak = 0.3,
        },
    };

    var descriptors = new Dictionary<string, ExternalAnalogyDescriptor>();

    var runner = new BosonCampaignRunner();
    var result = runner.Run(campaignSpec, registry, profiles, descriptors);

    Console.WriteLine($"  Candidates compared: {result.CandidatesCompared}");
    Console.WriteLine($"  Targets used: {result.TargetsUsed}");
    Console.WriteLine($"  Total results: {result.Results.Count}");
    Console.WriteLine($"  Negative results: {result.NegativeResults.Count}");

    // Write results
    var campaignsDir = Path.Combine(runFolder, "campaigns", "boson_campaigns");
    Directory.CreateDirectory(campaignsDir);
    var resultPath = Path.Combine(campaignsDir, $"{result.CampaignId}.json");
    File.WriteAllText(resultPath, GuJsonDefaults.Serialize(result));

    Console.WriteLine();
    Console.WriteLine($"Campaign result written to: {resultPath}");
    return 0;
}

static int ExportBosonReport(string[] args)
{
    if (args.Length < 2)
    {
        Console.Error.WriteLine("Usage: gu export-boson-report <runFolder> [--output <path>]");
        return 1;
    }

    var runFolder = args[1];
    var outputOverride = ParseFlag(args, "--output", "");

    // Load registry
    var registryPath = Path.Combine(runFolder, "bosons", "registry.json");
    if (!File.Exists(registryPath))
    {
        Console.Error.WriteLine($"Registry not found: {registryPath}");
        return 1;
    }

    var registryJson = File.ReadAllText(registryPath);
    var registry = BosonRegistry.FromJson(registryJson);

    // Load campaign results
    var campaignResults = new List<BosonCampaignResult>();
    var campaignsDir = Path.Combine(runFolder, "campaigns", "boson_campaigns");
    if (Directory.Exists(campaignsDir))
    {
        foreach (var file in Directory.GetFiles(campaignsDir, "*.json"))
        {
            var json = File.ReadAllText(file);
            var cr = GuJsonDefaults.Deserialize<BosonCampaignResult>(json);
            if (cr != null)
                campaignResults.Add(cr);
        }
    }

    // Load spectrum bundles to build spectrum sheets
    var spectrumSheets = new List<SpectrumSheet>();
    var spectraDir = Path.Combine(runFolder, "spectra");
    if (Directory.Exists(spectraDir))
    {
        foreach (var file in Directory.GetFiles(spectraDir, "*_spectrum.json"))
        {
            var json = File.ReadAllText(file);
            var sb = GuJsonDefaults.Deserialize<SpectrumBundle>(json);
            if (sb != null)
            {
                spectrumSheets.Add(new SpectrumSheet
                {
                    BackgroundId = sb.BackgroundId,
                    Eigenvalues = sb.Modes.Select(m => m.Eigenvalue).ToList(),
                    Multiplicities = sb.Modes.Select(m => 1).ToList(),
                    GaugeLeaks = sb.Modes.Select(m => m.GaugeLeakScore).ToList(),
                    ModeCount = sb.Modes.Count,
                    ConvergenceStatus = sb.ConvergenceStatus,
                });
            }
        }
    }

    Console.WriteLine($"Exporting boson atlas report...");
    Console.WriteLine($"  Registry candidates: {registry.Count}");
    Console.WriteLine($"  Campaign results: {campaignResults.Count}");
    Console.WriteLine($"  Spectrum sheets: {spectrumSheets.Count}");

    var generator = new BosonAtlasReportGenerator();
    var studyId = Path.GetFileName(runFolder) ?? "unknown-study";
    var report = generator.Generate(studyId, registry, campaignResults, spectrumSheets);

    // Determine output paths
    string reportsDir;
    if (!string.IsNullOrEmpty(outputOverride))
    {
        reportsDir = Path.GetDirectoryName(outputOverride) ?? runFolder;
    }
    else
    {
        reportsDir = Path.Combine(runFolder, "reports");
    }
    Directory.CreateDirectory(reportsDir);

    // Write JSON report
    var jsonPath = !string.IsNullOrEmpty(outputOverride)
        ? outputOverride
        : Path.Combine(reportsDir, "boson_atlas.json");
    File.WriteAllText(jsonPath, BosonAtlasReportGenerator.ToJson(report));

    // Write Markdown summary
    var mdPath = Path.ChangeExtension(jsonPath, ".md");
    var md = GenerateMarkdownReport(report);
    File.WriteAllText(mdPath, md);

    Console.WriteLine($"  Total candidates: {report.TotalCandidates}");
    Console.WriteLine($"  Negative results: {report.NegativeResults.Count}");
    Console.WriteLine($"  Ambiguity entries: {report.AmbiguityEntries.Count}");
    Console.WriteLine();
    Console.WriteLine($"JSON report: {jsonPath}");
    Console.WriteLine($"Markdown report: {mdPath}");
    return 0;
}

static string GenerateMarkdownReport(BosonAtlasReport report)
{
    var sb = new System.Text.StringBuilder();
    sb.AppendLine("# Boson Atlas Report");
    sb.AppendLine();
    sb.AppendLine($"- **Report ID:** {report.ReportId}");
    sb.AppendLine($"- **Study ID:** {report.StudyId}");
    sb.AppendLine($"- **Registry Version:** {report.RegistryVersion}");
    sb.AppendLine($"- **Generated:** {report.GeneratedAt:O}");
    sb.AppendLine($"- **Total Candidates:** {report.TotalCandidates}");
    sb.AppendLine();

    sb.AppendLine("## Claim Class Distribution");
    sb.AppendLine();
    foreach (var (cls, count) in report.ClaimClassCounts)
        sb.AppendLine($"- {cls}: {count}");
    sb.AppendLine();

    if (report.SpectrumSheets.Count > 0)
    {
        sb.AppendLine("## Spectrum Sheets");
        sb.AppendLine();
        foreach (var sheet in report.SpectrumSheets)
        {
            sb.AppendLine($"### Background: {sheet.BackgroundId}");
            sb.AppendLine($"- Modes: {sheet.ModeCount}");
            sb.AppendLine($"- Convergence: {sheet.ConvergenceStatus}");
            if (sheet.Eigenvalues.Count > 0)
                sb.AppendLine($"- Eigenvalue range: [{sheet.Eigenvalues.Min():E4}, {sheet.Eigenvalues.Max():E4}]");
            sb.AppendLine();
        }
    }

    if (report.StabilitySummaries.Count > 0)
    {
        sb.AppendLine("## Stability Summaries");
        sb.AppendLine();
        sb.AppendLine("| Candidate | Branch | Refinement | Backend | Observation | Assessment |");
        sb.AppendLine("|-----------|--------|------------|---------|-------------|------------|");
        foreach (var s in report.StabilitySummaries)
        {
            sb.AppendLine($"| {s.CandidateId} | {s.BranchStability:F2} | {s.RefinementStability:F2} | " +
                          $"{s.BackendStability:F2} | {s.ObservationStability:F2} | {s.OverallAssessment} |");
        }
        sb.AppendLine();
    }

    if (report.NegativeResults.Count > 0)
    {
        sb.AppendLine("## Negative Results");
        sb.AppendLine();
        foreach (var neg in report.NegativeResults)
            sb.AppendLine($"- **{neg.CandidateId}** ({neg.ResultType}): {neg.Description}");
        sb.AppendLine();
    }

    if (report.CampaignResults.Count > 0)
    {
        sb.AppendLine("## Campaign Results");
        sb.AppendLine();
        foreach (var cr in report.CampaignResults)
        {
            sb.AppendLine($"### Campaign: {cr.CampaignId}");
            sb.AppendLine($"- Candidates compared: {cr.CandidatesCompared}");
            sb.AppendLine($"- Targets used: {cr.TargetsUsed}");
            sb.AppendLine($"- Results: {cr.Results.Count} total, {cr.NegativeResults.Count} negative");
            sb.AppendLine();
        }
    }

    return sb.ToString();
}

static void PrintUsage()
{
    Console.WriteLine("Geometric Unity CLI");
    Console.WriteLine();
    Console.WriteLine("Usage:");
    Console.WriteLine("  gu create-branch [branchId] [outputPath]");
    Console.WriteLine("  gu create-environment [environmentId] [branchId] [outputPath]");
    Console.WriteLine("  gu init-run <run-folder> [branchId]         Initialize a canonical run folder");
    Console.WriteLine("  gu run <run-folder> [options]               Run solver on existing run folder");
    Console.WriteLine("  gu solve <run-folder> [options]             Init + run in one step");
    Console.WriteLine("  gu reproduce <run-folder> [replay] [--validate]  Reproduce a run from its replay contract");
    Console.WriteLine("  gu sweep --branches b1,b2 [--output dir]   Sweep branches and compare results");
    Console.WriteLine("  gu create-background-study [output.json]   Create a background study spec");
    Console.WriteLine("  gu solve-backgrounds <study.json> [opts]   Solve backgrounds and build atlas");
    Console.WriteLine("  gu compute-spectrum <run> <bgId> [opts]    Compute spectrum for a background");
    Console.WriteLine("  gu track-modes <runFolder> [--context ...]  Track modes across spectra");
    Console.WriteLine("  gu build-boson-registry <runFolder>         Build candidate boson registry");
    Console.WriteLine("  gu run-boson-campaign <runFolder> [opts]    Run boson comparison campaign");
    Console.WriteLine("  gu export-boson-report <runFolder> [opts]   Export boson atlas report");
    Console.WriteLine("  gu validate-replay <orig> <replay> [tier]   Validate replay (R0/R1/R2/R3)");
    Console.WriteLine("  gu verify-integrity <run-folder>            Verify or compute integrity hashes");
    Console.WriteLine("  gu validate-schema <file> <schema>          Validate JSON against a schema");
    Console.WriteLine();
    Console.WriteLine("Run/Solve options:");
    Console.WriteLine("  --backend cpu|cuda     Compute backend (default: cpu)");
    Console.WriteLine("  --mode A|B|C|D         Solver mode (A=residual, B=minimize, C=stationarity, D=branch-sensitivity)");
    Console.WriteLine("  --branches f1,f2,...   Branch manifest JSON files (Mode D / sweep)");
    Console.WriteLine("  --lie-algebra su2|su3  Lie algebra (default: su2)");
    Console.WriteLine("  --max-iter N           Max iterations (default: 100)");
    Console.WriteLine("  --step-size S          Initial step size (default: 0.01)");
    Console.WriteLine("  --output <dir>         Output directory for sweep results");
    Console.WriteLine();
    Console.WriteLine("Phase III Spectral/Boson options:");
    Console.WriteLine("  --num-modes N          Number of modes to compute (default: 10)");
    Console.WriteLine("  --formulation p1|p2    Physical mode formulation (default: p1)");
    Console.WriteLine("  --context continuation|branch|refinement  Tracking context type");
    Console.WriteLine("  --campaign <file>      Campaign spec JSON file");
}
