using Gu.Artifacts;
using Gu.Core;
using Gu.Core.Factories;
using Gu.Core.Serialization;
using Gu.Geometry;
using Gu.Math;
using Gu.Observation;
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
        Console.Error.WriteLine("Usage: gu run <run-folder> [--backend cpu|cuda] [--mode A|B|C] [--lie-algebra su2|su3] [--max-iter N] [--step-size S]");
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

    if (backend != "cpu")
    {
        Console.Error.WriteLine($"Backend '{backend}' is not yet implemented. Only 'cpu' is currently supported.");
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
        _ => SolveMode.ResidualOnly,
    };

    Console.WriteLine($"Running solver...");
    Console.WriteLine($"  Mode: {solveMode} ({mode})");
    Console.WriteLine($"  Lie algebra: {algebra.Label} (dim={algebra.Dimension})");
    Console.WriteLine($"  Mesh: Y_h {yMesh.VertexCount} vertices, {yMesh.EdgeCount} edges, {yMesh.FaceCount} faces");
    Console.WriteLine($"  Max iterations: {maxIter}");

    // Create branch operators and run pipeline
    var torsion = new TrivialTorsionCpu(yMesh, algebra);
    var shiab = new IdentityShiabCpu(yMesh, algebra);
    var pipeline = new CpuSolverPipeline(yMesh, algebra, torsion, shiab);

    var options = new SolverOptions
    {
        Mode = solveMode,
        MaxIterations = maxIter,
        InitialStepSize = stepSize,
    };

    var result = pipeline.Execute(null, null, manifest, geometry, options);
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
        _ => "A",
    };
    var scriptContent = $$"""
        #!/usr/bin/env bash
        # Reproduce this run. Generated by gu CLI.
        # Original run: {{runPath}}
        # Date: {{DateTimeOffset.UtcNow:O}}
        set -euo pipefail
        REPLAY_DIR="${1:-replay-$(date +%Y%m%d%H%M%S)}"
        dotnet run --project "$(dirname "$0")/../../apps/Gu.Cli" -- \
            solve "$REPLAY_DIR" \
            --backend {{backend}} \
            --mode {{modeFlag}} \
            --lie-algebra {{lieAlgebra}} \
            --max-iter {{maxIter}} \
            --step-size {{stepSize}}
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
        Console.Error.WriteLine("Usage: gu solve <run-folder> [--backend cpu|cuda] [--mode A|B|C] [--lie-algebra su2|su3] [--max-iter N] [--step-size S]");
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
    Console.WriteLine("  gu validate-replay <orig> <replay> [tier]   Validate replay (R0/R1/R2/R3)");
    Console.WriteLine("  gu verify-integrity <run-folder>            Verify or compute integrity hashes");
    Console.WriteLine("  gu validate-schema <file> <schema>          Validate JSON against a schema");
    Console.WriteLine();
    Console.WriteLine("Run/Solve options:");
    Console.WriteLine("  --backend cpu|cuda     Compute backend (default: cpu)");
    Console.WriteLine("  --mode A|B|C           Solver mode (A=residual, B=minimize, C=stationarity)");
    Console.WriteLine("  --lie-algebra su2|su3  Lie algebra (default: su2)");
    Console.WriteLine("  --max-iter N           Max iterations (default: 100)");
    Console.WriteLine("  --step-size S          Initial step size (default: 0.01)");
}
