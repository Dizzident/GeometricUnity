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
using Gu.Phase4.Chirality;
using Gu.Phase4.Couplings;
using Gu.Phase4.Dirac;
using Gu.Phase4.FamilyClustering;
using Gu.Phase4.Fermions;
using Gu.Phase4.Registry;
using Gu.Phase4.Spin;
using Gu.Phase5.BranchIndependence;
using Gu.Phase5.Convergence;
using Gu.Phase5.Dossiers;
using Gu.Phase5.Environments;
using Gu.Phase5.Falsification;
using Gu.Phase5.QuantitativeValidation;
using Gu.Phase5.Reporting;
using Gu.Branching;
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
    case "generate-phase4-report":
    case "report-phase4":
        return GeneratePhase4Report(args);
    case "build-spin-spec":
        return BuildSpinSpec(args);
    case "assemble-dirac":
        return AssembleDirac(args);
    case "solve-fermion-modes":
        return SolveFermionModes(args);
    case "analyze-chirality":
        return AnalyzeChirality(args);
    case "analyze-conjugation":
        return AnalyzeConjugation(args);
    case "extract-couplings":
        return ExtractCouplings(args);
    case "build-family-clusters":
        return BuildFamilyClusters(args);
    case "build-unified-registry":
        return BuildUnifiedRegistry(args);
    case "branch-robustness":
        return BranchRobustness(args);
    case "refinement-study":
        return RefinementStudy(args);
    case "import-environment":
        return ImportEnvironment(args);
    case "build-structured-environment":
        return BuildStructuredEnvironment(args);
    case "validate-quantitative":
        return ValidateQuantitative(args);
    case "build-validation-dossier":
        return BuildValidationDossier(args);
    case "verify-study-freshness":
        return VerifyStudyFreshness(args);
    case "run-phase5-campaign":
        return RunPhase5Campaign(args);
    case "export-phase5-bridge-values":
        return ExportPhase5BridgeValues(args);
    case "build-phase5-sidecars":
        return BuildPhase5Sidecars(args);
    case "validate-phase5-campaign-spec":
        return ValidatePhase5CampaignSpec(args);
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
        Console.Error.WriteLine("Usage: gu run <run-folder> [--backend cpu|cuda] [--mode A|B|C|D] [--lie-algebra su2|su3] [--max-iter N] [--step-size S] [--branches b1.json,b2.json] [--manifest path.json] [--omega path.json] [--a0 path.json]");
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
    var manifestFlag = ParseFlag(args, "--manifest", "");
    var omegaFlag = ParseFlag(args, "--omega", "");
    var a0Flag = ParseFlag(args, "--a0", "");

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

    // Load branch manifest: explicit --manifest flag > persisted run folder > default
    string consumedManifestPath;
    BranchManifest manifest;
    if (!string.IsNullOrEmpty(manifestFlag))
    {
        if (!File.Exists(manifestFlag))
        {
            Console.Error.WriteLine($"Manifest file not found: {manifestFlag}");
            return 1;
        }
        var loaded = GuJsonDefaults.Deserialize<BranchManifest>(File.ReadAllText(manifestFlag));
        if (loaded is null)
        {
            Console.Error.WriteLine($"Failed to deserialize manifest: {manifestFlag}");
            return 1;
        }
        manifest = loaded;
        consumedManifestPath = manifestFlag;
        Console.WriteLine($"  Branch manifest: loaded from --manifest flag: {manifestFlag}");
    }
    else
    {
        var persistedManifestPath = Path.Combine(runPath, RunFolderLayout.BranchManifestFile);
        var loadedFromFolder = File.Exists(persistedManifestPath)
            ? GuJsonDefaults.Deserialize<BranchManifest>(File.ReadAllText(persistedManifestPath))
            : null;
        if (loadedFromFolder is not null)
        {
            manifest = loadedFromFolder;
            consumedManifestPath = persistedManifestPath;
            Console.WriteLine($"  Branch manifest: loaded from run folder: {persistedManifestPath}");
        }
        else
        {
            manifest = new BranchManifest
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
            consumedManifestPath = "default-toy-manifest";
            Console.WriteLine($"  Branch manifest: default (no persisted manifest found)");
        }
    }

    // Load initial omega: explicit --omega flag > persisted final state > zero
    string consumedOmegaPath;
    FieldTensor? initialOmega = null;
    int edgeN = yMesh.EdgeCount * algebra.Dimension;

    if (!string.IsNullOrEmpty(omegaFlag))
    {
        if (!File.Exists(omegaFlag))
        {
            Console.Error.WriteLine($"Omega file not found: {omegaFlag}");
            return 1;
        }
        var loadedOmega = GuJsonDefaults.Deserialize<FieldTensor>(File.ReadAllText(omegaFlag));
        if (loadedOmega is not null && loadedOmega.Coefficients.Length == edgeN)
        {
            initialOmega = loadedOmega;
            consumedOmegaPath = omegaFlag;
            Console.WriteLine($"  Initial omega: loaded from --omega flag: {omegaFlag}");
        }
        else
        {
            Console.Error.WriteLine($"Omega file dimension mismatch (expected {edgeN} coefficients): {omegaFlag}");
            return 1;
        }
    }
    else
    {
        var finalStatePath = Path.Combine(runPath, RunFolderLayout.FinalStateFile);
        var finalState = File.Exists(finalStatePath)
            ? GuJsonDefaults.Deserialize<DiscreteState>(File.ReadAllText(finalStatePath))
            : null;
        if (finalState?.Omega is not null && finalState.Omega.Coefficients.Length == edgeN)
        {
            initialOmega = finalState.Omega;
            consumedOmegaPath = finalStatePath;
            Console.WriteLine($"  Initial omega: loaded from persisted final state: {finalStatePath}");
        }
        else
        {
            consumedOmegaPath = "zero (no persisted state found)";
            Console.WriteLine($"  Initial omega: zero (no persisted state found)");
        }
    }

    // Load A0: explicit --a0 flag > zero
    string consumedA0Path;
    FieldTensor? initialA0 = null;

    if (!string.IsNullOrEmpty(a0Flag))
    {
        if (!File.Exists(a0Flag))
        {
            Console.Error.WriteLine($"A0 file not found: {a0Flag}");
            return 1;
        }
        var loadedA0 = GuJsonDefaults.Deserialize<FieldTensor>(File.ReadAllText(a0Flag));
        if (loadedA0 is not null && loadedA0.Coefficients.Length == edgeN)
        {
            initialA0 = loadedA0;
            consumedA0Path = a0Flag;
            Console.WriteLine($"  Initial A0: loaded from --a0 flag: {a0Flag}");
        }
        else
        {
            Console.Error.WriteLine($"A0 file dimension mismatch (expected {edgeN} coefficients): {a0Flag}");
            return 1;
        }
    }
    else
    {
        consumedA0Path = "zero (default)";
    }

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

        // Upload background connection (use loaded A0 if provided, else zero)
        int cudaEdgeN = yMesh.EdgeCount * dimG;
        var a0Coeffs = initialA0?.Coefficients ?? new double[cudaEdgeN];
        nativeBackend.UploadBackgroundConnection(a0Coeffs, yMesh.EdgeCount, dimG);

        // Create CudaSolverBackend and run
        using var cudaBackend = new CudaSolverBackend(nativeBackend, ownsNative: true);
        cudaBackend.Initialize(snapshot);

        var orchestrator = new SolverOrchestrator(cudaBackend, options);

        // Build initial omega tensor (use loaded state if available, else zero)
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
        var omegaTensor = initialOmega ?? new FieldTensor
        {
            Label = "omega_h",
            Signature = sig,
            Coefficients = new double[cudaEdgeN],
            Shape = new[] { yMesh.EdgeCount, dimG },
        };
        var a0Tensor = initialA0 ?? new FieldTensor
        {
            Label = "a0",
            Signature = sig,
            Coefficients = new double[cudaEdgeN],
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
                    $"converged={cudaSolverResult.Converged}, reason={cudaSolverResult.TerminationReason}; " +
                    $"P4-C1 provenance: manifestSource={consumedManifestPath}, " +
                    $"omegaSource={consumedOmegaPath}, a0Source={consumedA0Path}",
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
        // CPU path: use CpuSolverPipeline; pass loaded omega/A0 if available
        backendId = "cpu-reference";
        var cpuOmega = initialOmega is not null
            ? new ConnectionField(yMesh, algebra, initialOmega.Coefficients)
            : null;
        var cpuA0 = initialA0 is not null
            ? new ConnectionField(yMesh, algebra, initialA0.Coefficients)
            : null;
        result = pipeline.Execute(cpuOmega, cpuA0, manifest, geometry, options);
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
            Notes = $"P4-C1 provenance: manifestSource={consumedManifestPath}, " +
                    $"omegaSource={consumedOmegaPath}, a0Source={consumedA0Path}",
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

    // Write solver log (with P4-C1 provenance appended)
    var logPath = Path.Combine(runPath, RunFolderLayout.SolverLogFile);
    var logDir = Path.GetDirectoryName(logPath);
    if (logDir is not null)
        Directory.CreateDirectory(logDir);
    var fullLog = new List<string>(result.DiagnosticLog)
    {
        $"P4-C1 provenance: manifestSource={consumedManifestPath}",
        $"P4-C1 provenance: omegaSource={consumedOmegaPath}",
        $"P4-C1 provenance: a0Source={consumedA0Path}",
    };
    File.WriteAllLines(logPath, fullLog);

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

static string? ParseOptionalFlag(string[] args, string flag)
{
    for (int i = 0; i < args.Length - 1; i++)
    {
        if (args[i] == flag)
            return args[i + 1];
    }
    return null;
}

/// <summary>
/// Locate the persisted omega state tensor for a given background ID.
/// Checks background_states/{bgId}_omega.json in the run folder hierarchy.
/// Returns the path if found, otherwise null.
/// </summary>
static string? FindBackgroundOmegaState(string runFolder, string backgroundId)
{
    // Check backgrounds/background_states/{bgId}_omega.json (solve-backgrounds output)
    var path1 = Path.Combine(runFolder, "backgrounds", "background_states", $"{backgroundId}_omega.json");
    if (File.Exists(path1)) return path1;

    // Check direct background_states subdir
    var path2 = Path.Combine(runFolder, "background_states", $"{backgroundId}_omega.json");
    if (File.Exists(path2)) return path2;

    return null;
}

/// <summary>
/// Locate the persisted A0 tensor written by solve-backgrounds.
/// Returns the path if found, otherwise null.
/// </summary>
static string? FindBackgroundA0State(string runFolder)
{
    // Check backgrounds/background_states/a0.json (solve-backgrounds output)
    var path1 = Path.Combine(runFolder, "backgrounds", "background_states", "a0.json");
    if (File.Exists(path1)) return path1;

    // Check direct background_states subdir
    var path2 = Path.Combine(runFolder, "background_states", "a0.json");
    if (File.Exists(path2)) return path2;

    return null;
}

/// <summary>
/// Locate the persisted geometry context written by solve-backgrounds.
/// Returns the path if found, otherwise null.
/// </summary>
static string? FindBackgroundGeometry(string runFolder)
{
    // Check backgrounds/manifest/geometry.json (solve-backgrounds output)
    var path1 = Path.Combine(runFolder, "backgrounds", "manifest", "geometry.json");
    if (File.Exists(path1)) return path1;

    // Check canonical run folder manifest/geometry.json
    var path2 = Path.Combine(runFolder, "manifest", "geometry.json");
    if (File.Exists(path2)) return path2;

    return null;
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
        Console.Error.WriteLine("Usage: gu solve-backgrounds <study.json> [--output <dir>] [--lie-algebra su2|su3] [--manifest <path>] [--manifest-dir <dir>]");
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
    var manifestFlag = ParseOptionalFlag(args, "--manifest");
    var manifestDirFlag = ParseOptionalFlag(args, "--manifest-dir");

    var studyJson = File.ReadAllText(studyPath);
    var study = BackgroundAtlasSerializer.DeserializeStudy(studyJson);
    if (study is null)
    {
        Console.Error.WriteLine("Failed to deserialize background study.");
        return 1;
    }

    Console.WriteLine($"Solving background study: {study.StudyId}");
    Console.WriteLine($"  Specs: {study.Specs.Count}");

    // D-001: Resolve manifests per spec using the resolution chain
    var studyDir = Path.GetDirectoryName(Path.GetFullPath(studyPath)) ?? ".";
    var manifests = new Dictionary<string, BranchManifest>();
    var manifestArtifactRefs = new Dictionary<string, string>(); // branchManifestId -> manifest path used

    // Validate --manifest usage: only valid when all specs share the same BranchManifestId
    if (manifestFlag is not null)
    {
        if (!ManifestResolver.ValidateExplicitManifestUsage(study.Specs, out var distinctIds) &&
            distinctIds.Count > 1)
        {
            Console.Error.WriteLine(
                $"Error: --manifest may only be used when all specs share the same BranchManifestId. " +
                $"Found {distinctIds.Count} distinct IDs: {string.Join(", ", distinctIds)}");
            return 1;
        }
    }

    foreach (var spec in study.Specs)
    {
        if (manifests.ContainsKey(spec.BranchManifestId))
            continue; // Already resolved this manifest ID

        try
        {
            var (resolved, resolvedRef) = ManifestResolver.Resolve(
                spec.BranchManifestId,
                manifestFlag,
                manifestDirFlag,
                study.ManifestSearchPaths,
                studyDir);

            manifests[spec.BranchManifestId] = resolved;
            manifestArtifactRefs[spec.BranchManifestId] = resolvedRef;
        }
        catch (ManifestResolutionException ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    // Set up geometry and solver
    var algebra = CreateAlgebra(lieAlgebra);
    var bundle = ToyGeometryFactory.CreateToy2D();
    var mesh = bundle.AmbientMesh;
    var geometry = bundle.ToGeometryContext("centroid", "P1");
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

    // Use the first resolved manifest for provenance
    var firstManifest = manifests.Values.First();
    var branchRef = new BranchRef
    {
        BranchId = firstManifest.BranchId,
        SchemaVersion = firstManifest.SchemaVersion,
    };
    var provenance = new ProvenanceMeta
    {
        CreatedAt = DateTimeOffset.UtcNow,
        CodeRevision = firstManifest.CodeRevision,
        Branch = branchRef,
        Backend = "cpu-reference",
    };

    var geometries = new Dictionary<string, GeometryContext> { ["env-default"] = geometry };
    var a0s = new Dictionary<string, FieldTensor> { ["env-default"] = a0 };

    var builder = new BackgroundAtlasBuilder((spec, manifest, geometryContext) =>
        CreateReferenceCpuBackend(mesh, algebra, manifest));
    var atlas = builder.Build(study, manifests, geometries, a0s, provenance, out var solvedStates);

    // D-002/D-003: Classify the solve run and persist per-background manifest files
    var runClassificationsBySpecId = study.Specs.ToDictionary(
        spec => spec.SpecId,
        ClassifyBackgroundSolve,
        StringComparer.Ordinal);
    Directory.CreateDirectory(outputDir);
    var statesDir = Path.Combine(outputDir, "background_states");
    Directory.CreateDirectory(statesDir);

    // Enrich all background records with RunClassification, persisted state refs,
    // and consumed manifest artifact refs.
    var enrichedBackgrounds = EnrichBackgroundRecords(atlas.Backgrounds, runClassificationsBySpecId, manifestArtifactRefs, statesDir);
    var enrichedRejected = EnrichBackgroundRecords(atlas.RejectedBackgrounds, runClassificationsBySpecId, manifestArtifactRefs, statesDir);

    // D-003: Write per-background manifest files
    foreach (var bg in enrichedBackgrounds.Concat(enrichedRejected))
    {
        if (!manifestArtifactRefs.TryGetValue(bg.BranchManifestId, out var mRef))
            continue;
        var manifestDestPath = Path.Combine(statesDir, $"{bg.BackgroundId}_manifest.json");
        if (manifests.TryGetValue(bg.BranchManifestId, out var mForBg))
        {
            File.WriteAllText(manifestDestPath, GuJsonDefaults.Serialize(mForBg));
        }
    }

    // Write atlas (with enriched records)
    var enrichedAtlas = new BackgroundAtlas
    {
        AtlasId = atlas.AtlasId,
        StudyId = atlas.StudyId,
        Backgrounds = enrichedBackgrounds,
        RejectedBackgrounds = enrichedRejected,
        RankingCriteria = atlas.RankingCriteria,
        TotalAttempts = atlas.TotalAttempts,
        Provenance = atlas.Provenance,
        AdmissibilityCounts = atlas.AdmissibilityCounts,
        EnvironmentTier = atlas.EnvironmentTier,
    };
    var atlasPath = Path.Combine(outputDir, "atlas.json");
    BackgroundAtlasSerializer.WriteAtlas(enrichedAtlas, atlasPath);

    // Write individual records
    var recordsDir = Path.Combine(outputDir, "background_records");
    Directory.CreateDirectory(recordsDir);
    foreach (var bg in enrichedBackgrounds)
    {
        BackgroundAtlasSerializer.WriteRecord(bg, Path.Combine(recordsDir, $"{bg.BackgroundId}.json"));
    }
    foreach (var bg in enrichedRejected)
    {
        BackgroundAtlasSerializer.WriteRecord(bg, Path.Combine(recordsDir, $"{bg.BackgroundId}.json"));
    }

    // Persist solved omega tensors alongside records so compute-spectrum can load them
    foreach (var (bgId, omegaTensor) in solvedStates)
    {
        var statePath = Path.Combine(statesDir, $"{bgId}_omega.json");
        File.WriteAllText(statePath, GuJsonDefaults.Serialize(omegaTensor));
    }

    // Persist geometry and A0 so compute-spectrum can load full background context
    var manifestDir = Path.Combine(outputDir, "manifest");
    Directory.CreateDirectory(manifestDir);
    File.WriteAllText(Path.Combine(manifestDir, "geometry.json"), GuJsonDefaults.Serialize(geometry));
    File.WriteAllText(Path.Combine(statesDir, "a0.json"), GuJsonDefaults.Serialize(a0));

    Console.WriteLine();
    Console.WriteLine($"Background atlas built:");
    Console.WriteLine($"  Atlas ID: {enrichedAtlas.AtlasId}");
    Console.WriteLine($"  Total attempts: {enrichedAtlas.TotalAttempts}");
    Console.WriteLine($"  Admitted: {enrichedAtlas.Backgrounds.Count}");
    Console.WriteLine($"  Rejected: {enrichedAtlas.RejectedBackgrounds.Count}");
    foreach (var (level, count) in enrichedAtlas.AdmissibilityCounts)
        Console.WriteLine($"    {level}: {count}");

    foreach (var bg in enrichedBackgrounds)
    {
        Console.WriteLine($"  {bg.BackgroundId}: {bg.AdmissibilityLevel} " +
                          $"residual={bg.ResidualNorm:E6} stationarity={bg.StationarityNorm:E6}");
    }

    Console.WriteLine();
    Console.WriteLine($"Atlas written to: {atlasPath}");
    Console.WriteLine($"Records written to: {recordsDir}");
    Console.WriteLine($"State tensors written to: {statesDir} ({solvedStates.Count} omega files)");
    return 0;
}

/// <summary>
/// Enrich background records with run classification and consumed manifest artifact ref.
/// Returns new record instances (BackgroundRecord is init-only).
/// </summary>
static IReadOnlyList<BackgroundRecord> EnrichBackgroundRecords(
    IReadOnlyList<BackgroundRecord> records,
    IReadOnlyDictionary<string, SolveRunClassification> runClassificationsBySpecId,
    IReadOnlyDictionary<string, string> manifestArtifactRefs,
    string statesDir)
{
    var enriched = new List<BackgroundRecord>(records.Count);
    foreach (var r in records)
    {
        var runClassification = TryExtractBackgroundSpecId(r.BackgroundId) is { } specId &&
                                runClassificationsBySpecId.TryGetValue(specId, out var classified)
            ? classified
            : SolveRunClassification.Classify("A", false, false, false);
        manifestArtifactRefs.TryGetValue(r.BranchManifestId, out var mRef);
        var persistedStateRef = r.AdmissibilityLevel == AdmissibilityLevel.Rejected
            ? r.StateArtifactRef
            : Path.Combine(statesDir, $"{r.BackgroundId}_omega.json");
        var persistedManifestRef = string.IsNullOrWhiteSpace(mRef)
            ? null
            : Path.Combine(statesDir, $"{r.BackgroundId}_manifest.json");
        enriched.Add(new BackgroundRecord
        {
            BackgroundId = r.BackgroundId,
            EnvironmentId = r.EnvironmentId,
            BranchManifestId = r.BranchManifestId,
            ContinuationCoordinates = r.ContinuationCoordinates,
            GeometryFingerprint = r.GeometryFingerprint,
            GaugeChoice = r.GaugeChoice,
            StateArtifactRef = persistedStateRef,
            ResidualNorm = r.ResidualNorm,
            StationarityNorm = r.StationarityNorm,
            AdmissibilityLevel = r.AdmissibilityLevel,
            Metrics = r.Metrics,
            SolveTraceRef = r.SolveTraceRef,
            ReplayTierAchieved = r.ReplayTierAchieved,
            Provenance = r.Provenance,
            RejectionReason = r.RejectionReason,
            PdeClassification = r.PdeClassification,
            Notes = r.Notes,
            EnvironmentTier = r.EnvironmentTier,
            RunClassification = runClassification,
            ConsumedManifestArtifactRef = persistedManifestRef,
        });
    }
    return enriched;
}

static string? TryExtractBackgroundSpecId(string backgroundId)
{
    if (!backgroundId.StartsWith("bg-", StringComparison.Ordinal))
        return null;

    var body = backgroundId[3..];
    if (body.EndsWith("-rejected", StringComparison.Ordinal))
        return body[..^"-rejected".Length];

    int lastDash = body.LastIndexOf('-');
    return lastDash > 0 ? body[..lastDash] : body;
}

static SolveRunClassification ClassifyBackgroundSolve(BackgroundSpec spec)
{
    var modeFlag = SolveModeToFlag(spec.SolveOptions.SolveMode);
    bool hasExplicitOmega = spec.Seed.InitialState?.Coefficients.Any(c => System.Math.Abs(c) > 1e-12) == true ||
        spec.Seed.Kind == BackgroundSeedKind.SymmetricAnsatz;
    bool hasPersistedOmega = spec.Seed.Kind is BackgroundSeedKind.Continuation or BackgroundSeedKind.CoarseGridTransfer &&
        !string.IsNullOrWhiteSpace(spec.Seed.ContinuationSourceId);
    var seedSourceOverride = spec.Seed.Kind == BackgroundSeedKind.SymmetricAnsatz ? "symmetric-ansatz" : null;
    return SolveRunClassification.Classify(modeFlag, hasPersistedOmega, hasExplicitOmega, false, seedSourceOverride);
}

static string SolveModeToFlag(SolveMode solveMode) => solveMode switch
{
    SolveMode.ObjectiveMinimization => "B",
    SolveMode.StationaritySolve => "C",
    _ => "A",
};

static CpuSolverBackend CreateReferenceCpuBackend(
    SimplicialMesh mesh,
    LieAlgebra algebra,
    BranchManifest manifest)
{
    var torsion = ResolveTorsionOperator(mesh, algebra, manifest);
    var shiab = ResolveShiabOperator(mesh, algebra, manifest);
    BranchOperatorRegistry.ValidateCarrierMatch(torsion, shiab);
    return new CpuSolverBackend(mesh, algebra, torsion, shiab);
}

static ITorsionBranchOperator ResolveTorsionOperator(
    SimplicialMesh mesh,
    LieAlgebra algebra,
    BranchManifest manifest) => manifest.ActiveTorsionBranch switch
{
    "trivial" or "trivial-torsion" => new TrivialTorsionCpu(mesh, algebra),
    "augmented-torsion" => new AugmentedTorsionCpu(mesh, algebra),
    _ => throw new InvalidOperationException(
        $"Unsupported torsion branch '{manifest.ActiveTorsionBranch}' in CLI background solve path."),
};

static IShiabBranchOperator ResolveShiabOperator(
    SimplicialMesh mesh,
    LieAlgebra algebra,
    BranchManifest manifest) => manifest.ActiveShiabBranch switch
{
    "identity-shiab" => new IdentityShiabCpu(mesh, algebra),
    "first-order-curvature" => new FirstOrderShiabOperator(mesh, algebra),
    _ => throw new InvalidOperationException(
        $"Unsupported Shiab branch '{manifest.ActiveShiabBranch}' in CLI background solve path."),
};

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

    if (formulationFlag.ToLowerInvariant() is "p3" or "quotientaware")
    {
        Console.Error.WriteLine(
            "Error: --formulation p3 (QuotientAware) is not yet implemented. " +
            "Use --formulation p2 for equivalent physical results, or --formulation p1.");
        return 1;
    }

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

    // Set up geometry and solver
    var lieAlgebra = ParseFlag(args, "--lie-algebra", "su2");
    var algebra = CreateAlgebra(lieAlgebra);
    var bundle = ToyGeometryFactory.CreateToy2D();
    var mesh = bundle.AmbientMesh;

    // Load persisted geometry context from run folder if available; fall back to toy geometry context
    string consumedGeometrySource;
    GeometryContext geometry;
    var persistedGeometryPath = FindBackgroundGeometry(runFolder);
    if (persistedGeometryPath is not null)
    {
        var loadedGeometry = GuJsonDefaults.Deserialize<GeometryContext>(File.ReadAllText(persistedGeometryPath));
        if (loadedGeometry is not null)
        {
            geometry = loadedGeometry;
            consumedGeometrySource = persistedGeometryPath;
            Console.WriteLine($"  Geometry: loaded from {persistedGeometryPath}");
        }
        else
        {
            geometry = bundle.ToGeometryContext("centroid", "P1");
            consumedGeometrySource = "toy-2d (geometry file unreadable)";
            Console.WriteLine($"  Geometry: toy-2d (geometry file unreadable)");
        }
    }
    else
    {
        geometry = bundle.ToGeometryContext("centroid", "P1");
        consumedGeometrySource = "toy-2d (no persisted geometry found)";
        Console.WriteLine($"  Geometry: toy-2d (no persisted geometry found)");
    }

    // D-003: Load branch manifest using precedence order:
    //   1. background_states/{backgroundId}_manifest.json  (per-background, from solve-backgrounds)
    //   2. RunFolderLayout.BranchManifestFile               (run-folder manifest)
    //   3. legacy generated default                         (with diagnostic note)
    string consumedManifestSource;
    BranchManifest manifest;

    // Step 1: per-background manifest written by solve-backgrounds
    var perBgManifestPath1 = Path.Combine(runFolder, "backgrounds", "background_states", $"{backgroundId}_manifest.json");
    var perBgManifestPath2 = Path.Combine(runFolder, "background_states", $"{backgroundId}_manifest.json");
    var perBgManifestPath = File.Exists(perBgManifestPath1) ? perBgManifestPath1
                          : File.Exists(perBgManifestPath2) ? perBgManifestPath2
                          : null;

    var loadedPerBgManifest = perBgManifestPath is not null
        ? GuJsonDefaults.Deserialize<BranchManifest>(File.ReadAllText(perBgManifestPath))
        : null;

    if (loadedPerBgManifest is not null)
    {
        manifest = loadedPerBgManifest;
        consumedManifestSource = $"per-background:{perBgManifestPath}";
        Console.WriteLine($"  Branch manifest: loaded from per-background file {perBgManifestPath}");
    }
    else
    {
        // Step 2: run-folder manifest
        var runFolderManifestPath = Path.Combine(runFolder, RunFolderLayout.BranchManifestFile);
        var loadedRunFolderManifest = File.Exists(runFolderManifestPath)
            ? GuJsonDefaults.Deserialize<BranchManifest>(File.ReadAllText(runFolderManifestPath))
            : null;

        if (loadedRunFolderManifest is not null)
        {
            manifest = loadedRunFolderManifest;
            consumedManifestSource = $"run-folder:{runFolderManifestPath}";
            Console.WriteLine($"  Branch manifest: loaded from run-folder {runFolderManifestPath}");
        }
        else
        {
            // Step 3: legacy generated default (backward compatibility)
            manifest = new BranchManifest
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
            consumedManifestSource = "default-toy-manifest (fallback: no per-background or run-folder manifest found)";
            Console.WriteLine($"  Branch manifest: default (no persisted manifest found — using legacy fallback)");
        }
    }

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

    // Load persisted omega state for this background if available; fall back to zero
    string consumedOmegaSource;
    FieldTensor omega;
    var stateFilePath = FindBackgroundOmegaState(runFolder, backgroundId);
    if (stateFilePath is not null)
    {
        var loadedOmega = GuJsonDefaults.Deserialize<FieldTensor>(File.ReadAllText(stateFilePath));
        if (loadedOmega is not null && loadedOmega.Coefficients.Length == edgeN)
        {
            omega = loadedOmega;
            consumedOmegaSource = stateFilePath;
            Console.WriteLine($"  Omega state: loaded from {stateFilePath}");
        }
        else
        {
            omega = new FieldTensor
            {
                Label = "omega_h",
                Signature = sig,
                Coefficients = new double[edgeN],
                Shape = new[] { mesh.EdgeCount, algebra.Dimension },
            };
            consumedOmegaSource = "zero (state file dimension mismatch)";
            Console.WriteLine($"  Omega state: zero (state file dimension mismatch)");
        }
    }
    else
    {
        omega = new FieldTensor
        {
            Label = "omega_h",
            Signature = sig,
            Coefficients = new double[edgeN],
            Shape = new[] { mesh.EdgeCount, algebra.Dimension },
        };
        consumedOmegaSource = "zero (no persisted state found)";
        Console.WriteLine($"  Omega state: zero (no persisted state found)");
    }

    // Load persisted A0 state if available; fall back to zero
    string consumedA0Source;
    FieldTensor a0;
    var a0FilePath = FindBackgroundA0State(runFolder);
    if (a0FilePath is not null)
    {
        var loadedA0 = GuJsonDefaults.Deserialize<FieldTensor>(File.ReadAllText(a0FilePath));
        if (loadedA0 is not null && loadedA0.Coefficients.Length == edgeN)
        {
            a0 = loadedA0;
            consumedA0Source = a0FilePath;
            Console.WriteLine($"  A0 state: loaded from {a0FilePath}");
        }
        else
        {
            a0 = new FieldTensor
            {
                Label = "a0",
                Signature = sig,
                Coefficients = new double[edgeN],
                Shape = new[] { mesh.EdgeCount, algebra.Dimension },
            };
            consumedA0Source = "zero (A0 file dimension mismatch)";
            Console.WriteLine($"  A0 state: zero (A0 file dimension mismatch)");
        }
    }
    else
    {
        a0 = new FieldTensor
        {
            Label = "a0",
            Signature = sig,
            Coefficients = new double[edgeN],
            Shape = new[] { mesh.EdgeCount, algebra.Dimension },
        };
        consumedA0Source = "zero (no persisted A0 found)";
        Console.WriteLine($"  A0 state: zero (no persisted A0 found)");
    }

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

    // Record provenance: which manifest and background state were consumed
    spectrum.DiagnosticNotes.Add($"P4-C1 provenance: backgroundId={backgroundId}");
    spectrum.DiagnosticNotes.Add($"P4-C1 provenance: backgroundRecordPath={bgPath}");
    spectrum.DiagnosticNotes.Add($"P4-C1 provenance: manifestSource={consumedManifestSource}");
    spectrum.DiagnosticNotes.Add($"P4-C1 provenance: omegaSource={consumedOmegaSource}");
    spectrum.DiagnosticNotes.Add($"P4-C1 provenance: a0Source={consumedA0Source}");
    spectrum.DiagnosticNotes.Add($"P4-C1 provenance: geometrySource={consumedGeometrySource}");

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

    // Write observed mode signatures via the Phase III observation pipeline
    var signaturesDir = Path.Combine(runFolder, "observables", "mode_signatures");
    Directory.CreateDirectory(signaturesDir);

    var omegaConn = new ConnectionField(mesh, algebra, omega.Coefficients);
    var curvatureF = CurvatureAssembler.Assemble(omegaConn).ToFieldTensor();
    var jacobian = assembler.BuildJacobian(
        omegaConn,
        new ConnectionField(mesh, algebra, a0.Coefficients),
        curvatureF,
        manifest,
        geometry);
    var pullback = new PullbackOperator(bundle);
    var linObs = new Gu.Phase3.Observables.LinearizedObservationOperator(jacobian, pullback, backgroundId);
    var obsP3Pipeline = new Gu.Phase3.Observables.ObservationPipeline(linObs);
    var obsResult = obsP3Pipeline.Run(spectrum.Modes);

    foreach (var modeSig in obsResult.Signatures)
    {
        var sigPath = Path.Combine(signaturesDir, $"{modeSig.ModeId}.json");
        File.WriteAllText(sigPath, GuJsonDefaults.Serialize(modeSig));
    }

    Console.WriteLine($"  Observed signatures written: {obsResult.ModeCount}");
    Console.WriteLine();
    Console.WriteLine($"Spectrum written to: {spectrumPath}");
    Console.WriteLine($"Mode records written to: {modesDir}");
    Console.WriteLine($"Mode signatures written to: {signaturesDir}");
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

    // Load pre-computed observed mode signatures (written by compute-spectrum)
    var signaturesDir = Path.Combine(runFolder, "observables", "mode_signatures");
    List<Gu.Phase3.Observables.ObservedModeSignature>? observedSignatures = null;
    if (Directory.Exists(signaturesDir))
    {
        observedSignatures = new List<Gu.Phase3.Observables.ObservedModeSignature>();
        foreach (var sigFile in Directory.GetFiles(signaturesDir, "*.json"))
        {
            var sigJson = File.ReadAllText(sigFile);
            var loadedSig = GuJsonDefaults.Deserialize<Gu.Phase3.Observables.ObservedModeSignature>(sigJson);
            if (loadedSig != null)
                observedSignatures.Add(loadedSig);
        }
        Console.WriteLine($"  Observed signatures loaded: {observedSignatures.Count}");
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
            var alignments = engine.Match(reference, target, signatures: observedSignatures);
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

static int GeneratePhase4Report(string[] args)
{
    if (args.Length < 2)
    {
        Console.Error.WriteLine("Usage: gu generate-phase4-report <runFolder> [--output <path>]");
        return 1;
    }

    var runFolder = args[1];
    var outputOverride = ParseFlag(args, "--output", "");

    // Load unified particle registry
    var registryPath = Path.Combine(runFolder, "particle_registry", "unified_particle_registry.json");
    if (!File.Exists(registryPath))
    {
        // Fall back to checking for registry in the bosons folder
        var fallback = Path.Combine(runFolder, "bosons", "unified_registry.json");
        registryPath = File.Exists(fallback) ? fallback : registryPath;
    }

    // Load fermion families atlas (best-effort; empty atlas if not found)
    var atlasPath = Path.Combine(runFolder, "fermions", "fermion_families.json");
    var couplingPath = Path.Combine(runFolder, "fermions", "couplings", "coupling_atlas.json");

    Console.WriteLine($"Generating Phase IV report for run folder: {runFolder}");

    Gu.Phase4.Registry.UnifiedParticleRegistry registry;
    if (File.Exists(registryPath))
    {
        var json = File.ReadAllText(registryPath);
        registry = Gu.Phase4.Registry.UnifiedParticleRegistry.FromJson(json);
        Console.WriteLine($"  Loaded unified registry: {registry.Count} candidates");
    }
    else
    {
        Console.WriteLine($"  No unified particle registry found at {registryPath}; using empty registry.");
        registry = new Gu.Phase4.Registry.UnifiedParticleRegistry();
    }

    // Build minimal atlas from disk or empty
    Gu.Phase4.FamilyClustering.FermionFamilyAtlas atlas;
    if (File.Exists(atlasPath))
    {
        var json = File.ReadAllText(atlasPath);
        var options = new System.Text.Json.JsonSerializerOptions
        {
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() },
        };
        atlas = System.Text.Json.JsonSerializer.Deserialize<Gu.Phase4.FamilyClustering.FermionFamilyAtlas>(json, options)
            ?? new Gu.Phase4.FamilyClustering.FermionFamilyAtlas
            {
                AtlasId = "empty",
                BranchFamilyId = "unknown",
                ContextIds = new List<string>(),
                BackgroundIds = new List<string>(),
                Families = new List<Gu.Phase4.Fermions.FermionModeFamily>(),
                Provenance = new Gu.Core.ProvenanceMeta
                {
                    CreatedAt = DateTimeOffset.UtcNow,
                    CodeRevision = "cli-generated",
                    Branch = new Gu.Core.BranchRef { BranchId = "unknown", SchemaVersion = "1.0.0" },
                },
            };
        Console.WriteLine($"  Loaded fermion family atlas: {atlas.Families.Count} families");
    }
    else
    {
        Console.WriteLine($"  No fermion family atlas found; using empty atlas.");
        atlas = new Gu.Phase4.FamilyClustering.FermionFamilyAtlas
        {
            AtlasId = "empty",
            BranchFamilyId = "unknown",
            ContextIds = new List<string>(),
            BackgroundIds = new List<string>(),
            Families = new List<Gu.Phase4.Fermions.FermionModeFamily>(),
            Provenance = new Gu.Core.ProvenanceMeta
            {
                CreatedAt = DateTimeOffset.UtcNow,
                CodeRevision = "cli-generated",
                Branch = new Gu.Core.BranchRef { BranchId = "unknown", SchemaVersion = "1.0.0" },
            },
        };
    }

    Gu.Phase4.Couplings.CouplingAtlas couplingAtlas;
    if (File.Exists(couplingPath))
    {
        var json = File.ReadAllText(couplingPath);
        var options = new System.Text.Json.JsonSerializerOptions
        {
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() },
        };
        couplingAtlas = System.Text.Json.JsonSerializer.Deserialize<Gu.Phase4.Couplings.CouplingAtlas>(json, options)
            ?? BuildEmptyCouplingAtlas();
        Console.WriteLine($"  Loaded coupling atlas: {couplingAtlas.Couplings.Count} couplings");
    }
    else
    {
        Console.WriteLine($"  No coupling atlas found; using empty coupling atlas.");
        couplingAtlas = BuildEmptyCouplingAtlas();
    }

    var provenance = new Gu.Core.ProvenanceMeta
    {
        CreatedAt = DateTimeOffset.UtcNow,
        CodeRevision = "cli-generated",
        Branch = new Gu.Core.BranchRef { BranchId = "unknown", SchemaVersion = "1.0.0" },
    };

    var studyId = Path.GetFileName(runFolder.TrimEnd(Path.DirectorySeparatorChar)) ?? "unknown-study";
    var generator = new Gu.Phase4.Reporting.Phase4ReportGenerator();
    var report = generator.Generate(studyId, registry, atlas, couplingAtlas, provenance);

    // Summary
    var formatter = new Gu.Phase4.Reporting.ReportSummaryFormatter();
    Console.WriteLine();
    Console.WriteLine(formatter.FormatOneLiner(report));
    Console.WriteLine();

    // Determine output directory
    string reportsDir = !string.IsNullOrEmpty(outputOverride)
        ? Path.GetDirectoryName(outputOverride) ?? runFolder
        : Path.Combine(runFolder, "reports");
    Directory.CreateDirectory(reportsDir);

    // Write JSON report
    var jsonPath = !string.IsNullOrEmpty(outputOverride)
        ? outputOverride
        : Path.Combine(reportsDir, "phase4_report.json");
    Gu.Phase4.Reporting.Phase4ReportSerializer.WriteToFile(report, jsonPath);

    // Write text summary
    var mdPath = Path.ChangeExtension(jsonPath, ".md");
    File.WriteAllText(mdPath, formatter.Format(report));

    Console.WriteLine($"JSON report : {jsonPath}");
    Console.WriteLine($"Text report : {mdPath}");
    return 0;
}

// ---------------------------------------------------------------------------
// Phase IV CLI commands (GAP-1)
// ---------------------------------------------------------------------------

static ProvenanceMeta BuildP4Provenance(string runFolder, string codeRevision = "cli-phase4")
{
    // Load branch manifest from run folder for P4-C1 provenance notes
    var manifestPath = Path.Combine(runFolder, RunFolderLayout.BranchManifestFile);
    string manifestSource = File.Exists(manifestPath) ? manifestPath : "no-persisted-manifest";
    string branchId = "unknown";
    string schemaVersion = "1.0.0";
    if (File.Exists(manifestPath))
    {
        var m = GuJsonDefaults.Deserialize<BranchManifest>(File.ReadAllText(manifestPath));
        if (m is not null) { branchId = m.BranchId; schemaVersion = m.SchemaVersion; }
    }
    return new ProvenanceMeta
    {
        CreatedAt = DateTimeOffset.UtcNow,
        CodeRevision = codeRevision,
        Branch = new BranchRef { BranchId = branchId, SchemaVersion = schemaVersion },
        Notes = $"P4-C1 provenance: manifestSource={manifestSource}, runFolder={runFolder}",
    };
}

static SpinorRepresentationSpec BuildDefaultSpinorSpec(ProvenanceMeta provenance)
{
    // Default: Cl(5,0) Riemannian, dimY=5, spinorDim=4, su(2) gauge (dimG=3)
    // Matches the Phase4-FermionFamily-Atlas-001 study defaults.
    var sig = new CliffordSignature { Positive = 5, Negative = 0 };
    return new SpinorRepresentationSpec
    {
        SpinorSpecId = "spinor-cl5-riem-v1",
        SpacetimeDimension = 5,
        CliffordSignature = sig,
        GammaConvention = new GammaConventionSpec
        {
            ConventionId = "dirac-tensor-product-v1",
            Signature = sig,
            Representation = "standard",
            SpinorDimension = 4,
            HasChirality = false,
        },
        ChiralityConvention = new ChiralityConventionSpec
        {
            ConventionId = "chirality-trivial-v1",
            SignConvention = "left-is-minus",
            PhaseFactor = "trivial",
            HasChirality = false,
            FullChiralityOperator = null,
            BaseChiralityOperator = "X-chirality",
            FiberChiralityOperator = null,
            BaseDimension = 2,
            FiberDimension = 3,
        },
        ConjugationConvention = new ConjugationConventionSpec
        {
            ConventionId = "hermitian-v1",
            ConjugationType = "hermitian",
            HasChargeConjugation = true,
        },
        SpinorComponents = 4,
        ChiralitySplit = 0,
        InsertedAssumptionIds = new List<string> { "P4-IA-003", "P4-IA-001" },
        Provenance = provenance,
    };
}

static int BuildSpinSpec(string[] args)
{
    if (args.Length < 2)
    {
        Console.Error.WriteLine("Usage: gu build-spin-spec <runFolder> [<outSpec>] [--dim-y N] [--dim-g N]");
        return 1;
    }

    var runFolder = args[1];
    var outSpec = args.Length > 2 && !args[2].StartsWith("--") ? args[2] : "";
    int dimY = int.Parse(ParseFlag(args, "--dim-y", "5"));
    int dimG = int.Parse(ParseFlag(args, "--dim-g", "3"));

    if (!Directory.Exists(runFolder))
    {
        Console.Error.WriteLine($"Run folder not found: {runFolder}");
        return 1;
    }

    var provenance = BuildP4Provenance(runFolder);
    var spinorSpec = BuildDefaultSpinorSpec(provenance);

    // Serialize
    var json = JsonSerializer.Serialize(spinorSpec, new JsonSerializerOptions
    {
        WriteIndented = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() },
    });

    // Determine output path
    var fermionsDir = Path.Combine(runFolder, "fermions");
    Directory.CreateDirectory(fermionsDir);
    var outPath = !string.IsNullOrEmpty(outSpec) ? outSpec
        : Path.Combine(fermionsDir, "spinor_representation.json");

    File.WriteAllText(outPath, json);
    Console.WriteLine($"Written spinor spec: {outPath}");
    Console.WriteLine($"  SpinorSpecId: {spinorSpec.SpinorSpecId}");
    Console.WriteLine($"  SpacetimeDimension: {spinorSpec.SpacetimeDimension}");
    Console.WriteLine($"  SpinorComponents: {spinorSpec.SpinorComponents}");

    // Schema validation (best-effort)
    var schemaPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
        "../../../../schemas/phase4/spinor_representation.schema.json");
    if (!File.Exists(schemaPath))
        schemaPath = Path.Combine(Directory.GetCurrentDirectory(), "schemas/phase4/spinor_representation.schema.json");
    if (File.Exists(schemaPath))
    {
        var result = SchemaValidator.ValidateWithSchemaFile(json, schemaPath);
        Console.WriteLine(result.IsValid ? "  Schema: valid" : $"  Schema: INVALID — {string.Join(", ", result.Errors)}");
    }

    return 0;
}

static int AssembleDirac(string[] args)
{
    if (args.Length < 2)
    {
        Console.Error.WriteLine("Usage: gu assemble-dirac <runFolder> [--spin-spec <path>] [--background-id <id>] [--out <path>]");
        return 1;
    }

    var runFolder = args[1];
    if (!Directory.Exists(runFolder))
    {
        Console.Error.WriteLine($"Run folder not found: {runFolder}");
        return 1;
    }

    var spinSpecFlag = ParseFlag(args, "--spin-spec", "");
    var backgroundIdFlag = ParseFlag(args, "--background-id", "");
    var outFlag = ParseFlag(args, "--out", "");

    var provenance = BuildP4Provenance(runFolder);
    var fermionsDir = Path.Combine(runFolder, "fermions");
    Directory.CreateDirectory(fermionsDir);

    // Load spinor spec
    SpinorRepresentationSpec spinorSpec;
    var specPath = !string.IsNullOrEmpty(spinSpecFlag) ? spinSpecFlag
        : Path.Combine(fermionsDir, "spinor_representation.json");
    if (File.Exists(specPath))
    {
        spinorSpec = JsonSerializer.Deserialize<SpinorRepresentationSpec>(File.ReadAllText(specPath),
            new JsonSerializerOptions { Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() } })
            ?? BuildDefaultSpinorSpec(provenance);
        Console.WriteLine($"  Loaded spinor spec: {specPath}");
    }
    else
    {
        spinorSpec = BuildDefaultSpinorSpec(provenance);
        Console.WriteLine("  No spinor spec found; using default Cl(5,0) spec.");
    }

    // Load background record
    var bgDir = Path.Combine(runFolder, "backgrounds", "background_states");
    string? bgOmegaPath = null;
    string backgroundId = backgroundIdFlag;
    if (string.IsNullOrEmpty(backgroundId))
    {
        // Try to find first background omega file
        if (Directory.Exists(bgDir))
        {
            var omegaFiles = Directory.GetFiles(bgDir, "*_omega.json");
            if (omegaFiles.Length > 0)
            {
                bgOmegaPath = omegaFiles[0];
                backgroundId = Path.GetFileName(bgOmegaPath).Replace("_omega.json", "");
            }
        }
    }
    else
    {
        bgOmegaPath = FindBackgroundOmegaState(runFolder, backgroundId);
    }

    // Build geometry (toy 2D if no background is loaded)
    var bundle = ToyGeometryFactory.CreateToy2D();
    var yMesh = bundle.AmbientMesh;
    int dimG = 3; // su(2)

    double[] bosonicState;
    BackgroundRecord background;
    if (bgOmegaPath is not null && File.Exists(bgOmegaPath))
    {
        var omegaTensor = GuJsonDefaults.Deserialize<FieldTensor>(File.ReadAllText(bgOmegaPath));
        bosonicState = omegaTensor?.Coefficients ?? new double[yMesh.EdgeCount * dimG];
        background = new BackgroundRecord
        {
            BackgroundId = backgroundId,
            EnvironmentId = "cli-env",
            BranchManifestId = provenance.Branch?.BranchId ?? "unknown",
            GeometryFingerprint = $"toy2d-vert{yMesh.VertexCount}-edge{yMesh.EdgeCount}",
            StateArtifactRef = bgOmegaPath,
            ResidualNorm = 0.0,
            StationarityNorm = 0.0,
            AdmissibilityLevel = Gu.Phase3.Backgrounds.AdmissibilityLevel.B0,
            Metrics = new Gu.Phase3.Backgrounds.BackgroundMetrics
            {
                ResidualNorm = 0.0,
                StationarityNorm = 0.0,
                ObjectiveValue = 0.0,
                GaugeViolation = 0.0,
                SolverIterations = 0,
                SolverConverged = true,
                TerminationReason = "loaded-from-run-folder",
                GaussNewtonValid = false,
            },
            ReplayTierAchieved = "R0",
            Provenance = provenance,
        };
        Console.WriteLine($"  Loaded background omega: {bgOmegaPath}");
    }
    else
    {
        // Use zero bosonic state
        bosonicState = new double[yMesh.EdgeCount * dimG];
        background = new BackgroundRecord
        {
            BackgroundId = string.IsNullOrEmpty(backgroundId) ? "bg-zero" : backgroundId,
            EnvironmentId = "cli-env",
            BranchManifestId = provenance.Branch?.BranchId ?? "unknown",
            GeometryFingerprint = $"toy2d-vert{yMesh.VertexCount}-edge{yMesh.EdgeCount}",
            StateArtifactRef = "inline:zero",
            ResidualNorm = 0.0,
            StationarityNorm = 0.0,
            AdmissibilityLevel = Gu.Phase3.Backgrounds.AdmissibilityLevel.B0,
            Metrics = new Gu.Phase3.Backgrounds.BackgroundMetrics
            {
                ResidualNorm = 0.0,
                StationarityNorm = 0.0,
                ObjectiveValue = 0.0,
                GaugeViolation = 0.0,
                SolverIterations = 0,
                SolverConverged = true,
                TerminationReason = "zero-state",
                GaussNewtonValid = false,
            },
            ReplayTierAchieved = "R0",
            Provenance = provenance,
        };
        Console.WriteLine("  No background omega found; using zero bosonic state.");
    }

    // Build layout
    var layout = FermionFieldLayoutFactory.BuildStandardLayout(
        "layout-cli-v1", spinorSpec, dimG, provenance, new List<string> { "P4-IA-003" });

    // Build gamma matrices
    var gammaBuilder = new GammaMatrixBuilder();
    var gammas = gammaBuilder.Build(spinorSpec.CliffordSignature, spinorSpec.GammaConvention, provenance);

    // Build spin connection
    var connBuilder = new CpuSpinConnectionBuilder();
    var spinConnection = connBuilder.Build(background, bosonicState, spinorSpec, layout, yMesh, provenance);

    // Assemble Dirac operator
    var assembler = new CpuDiracOperatorAssembler();
    var diracBundle = assembler.Assemble(spinConnection, gammas, layout, yMesh, provenance);

    // Write output
    var outPath = !string.IsNullOrEmpty(outFlag) ? outFlag
        : Path.Combine(fermionsDir, $"dirac_bundle_{background.BackgroundId}.json");
    var json = JsonSerializer.Serialize(diracBundle, new JsonSerializerOptions
    {
        WriteIndented = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() },
    });
    File.WriteAllText(outPath, json);
    Console.WriteLine($"Written Dirac bundle: {outPath}");
    Console.WriteLine($"  OperatorId: {diracBundle.OperatorId}");
    Console.WriteLine($"  TotalDof: {diracBundle.TotalDof}");
    Console.WriteLine($"  HasExplicitMatrix: {diracBundle.HasExplicitMatrix}");
    Console.WriteLine($"  IsHermitian: {diracBundle.IsHermitian}");

    return 0;
}

static int SolveFermionModes(string[] args)
{
    if (args.Length < 2)
    {
        Console.Error.WriteLine("Usage: gu solve-fermion-modes <runFolder> [--dirac <path>] [--target lowest] [--count N] [--out <path>]");
        return 1;
    }

    var runFolder = args[1];
    if (!Directory.Exists(runFolder))
    {
        Console.Error.WriteLine($"Run folder not found: {runFolder}");
        return 1;
    }

    var diracFlag = ParseFlag(args, "--dirac", "");
    var countStr = ParseFlag(args, "--count", "6");
    int modeCount = int.Parse(countStr);
    var outFlag = ParseFlag(args, "--out", "");

    var provenance = BuildP4Provenance(runFolder);
    var fermionsDir = Path.Combine(runFolder, "fermions");
    Directory.CreateDirectory(fermionsDir);

    // Load Dirac bundle — find the first available if not specified
    var diracPath = !string.IsNullOrEmpty(diracFlag) ? diracFlag : "";
    if (string.IsNullOrEmpty(diracPath) && Directory.Exists(fermionsDir))
    {
        var files = Directory.GetFiles(fermionsDir, "dirac_bundle_*.json");
        if (files.Length > 0) diracPath = files[0];
    }

    if (string.IsNullOrEmpty(diracPath) || !File.Exists(diracPath))
    {
        Console.Error.WriteLine($"No Dirac bundle found. Run 'assemble-dirac' first, or specify --dirac <path>.");
        return 1;
    }

    var diracOptions = new JsonSerializerOptions
    {
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() },
    };
    var diracBundle = JsonSerializer.Deserialize<DiracOperatorBundle>(File.ReadAllText(diracPath), diracOptions);
    if (diracBundle is null)
    {
        Console.Error.WriteLine($"Failed to deserialize DiracOperatorBundle: {diracPath}");
        return 1;
    }
    Console.WriteLine($"  Loaded Dirac bundle: {diracPath}");
    Console.WriteLine($"  OperatorId: {diracBundle.OperatorId}, TotalDof: {diracBundle.TotalDof}");

    if (!diracBundle.HasExplicitMatrix || diracBundle.ExplicitMatrix is null)
    {
        Console.Error.WriteLine("DiracOperatorBundle lacks an explicit matrix (system too large or matrix not assembled).");
        return 1;
    }

    // Load layout from spinor spec
    var spinorSpec = BuildDefaultSpinorSpec(provenance);
    var layout = FermionFieldLayoutFactory.BuildStandardLayout(
        "layout-cli-v1", spinorSpec, 3, provenance, new List<string> { "P4-IA-003" });

    // Spectral config
    var spectralConfig = new FermionSpectralConfig
    {
        TargetRegion = "lowest-magnitude",
        ModeCount = modeCount,
        ConvergenceTolerance = 1e-8,
        MaxIterations = 2000,
        Seed = 42,
    };

    var assembler = new CpuDiracOperatorAssembler();
    var solver = new FermionSpectralSolver(assembler);
    var spectralResult = solver.Solve(diracBundle, layout, spectralConfig, provenance);

    Console.WriteLine($"  Solved {spectralResult.Modes.Count} fermion modes");
    foreach (var mode in spectralResult.Modes)
        Console.WriteLine($"    mode-{mode.ModeIndex}: lambda={mode.EigenvalueRe:E6}, residual={mode.ResidualNorm:E3}");

    // Write modes JSON
    var outPath = !string.IsNullOrEmpty(outFlag) ? outFlag
        : Path.Combine(fermionsDir, "fermion_modes.json");
    var json = JsonSerializer.Serialize(spectralResult, new JsonSerializerOptions
    {
        WriteIndented = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() },
    });
    File.WriteAllText(outPath, json);
    Console.WriteLine($"Written fermion modes: {outPath}");
    return 0;
}

static int AnalyzeChirality(string[] args)
{
    if (args.Length < 2)
    {
        Console.Error.WriteLine("Usage: gu analyze-chirality <runFolder> [--modes <path>] [--out <path>]");
        return 1;
    }

    var runFolder = args[1];
    if (!Directory.Exists(runFolder))
    {
        Console.Error.WriteLine($"Run folder not found: {runFolder}");
        return 1;
    }

    var modesFlag = ParseFlag(args, "--modes", "");
    var outFlag = ParseFlag(args, "--out", "");

    var provenance = BuildP4Provenance(runFolder);
    var fermionsDir = Path.Combine(runFolder, "fermions");
    Directory.CreateDirectory(fermionsDir);

    // Load modes
    var modesPath = !string.IsNullOrEmpty(modesFlag) ? modesFlag
        : Path.Combine(fermionsDir, "fermion_modes.json");
    if (!File.Exists(modesPath))
    {
        Console.Error.WriteLine($"Fermion modes not found: {modesPath}. Run 'solve-fermion-modes' first.");
        return 1;
    }

    var opts = new JsonSerializerOptions { Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() } };
    var spectralResult = JsonSerializer.Deserialize<FermionSpectralResult>(File.ReadAllText(modesPath), opts);
    if (spectralResult is null)
    {
        Console.Error.WriteLine($"Failed to deserialize FermionSpectralResult: {modesPath}");
        return 1;
    }
    Console.WriteLine($"  Loaded {spectralResult.Modes.Count} modes from {modesPath}");

    // Build spinor spec and gammas
    var spinorSpec = BuildDefaultSpinorSpec(provenance);
    var gammaBuilder = new GammaMatrixBuilder();
    var gammas = gammaBuilder.Build(spinorSpec.CliffordSignature, spinorSpec.GammaConvention, provenance);
    var layout = FermionFieldLayoutFactory.BuildStandardLayout(
        "layout-cli-v1", spinorSpec, 3, provenance, new List<string> { "P4-IA-003" });

    var analyzer = new ChiralityAnalyzer();
    var decompositions = analyzer.AnalyzeAll(
        spectralResult.Modes, gammas, spinorSpec.ChiralityConvention, layout, 0);

    foreach (var d in decompositions)
        Console.WriteLine($"  {d.ModeId}: tag={d.ChiralityTag}, left={d.LeftFraction:F3}, right={d.RightFraction:F3}");

    // Write chirality analysis
    var outPath = !string.IsNullOrEmpty(outFlag) ? outFlag
        : Path.Combine(fermionsDir, "chirality_analysis.json");
    var json = JsonSerializer.Serialize(decompositions, new JsonSerializerOptions
    {
        WriteIndented = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() },
    });
    File.WriteAllText(outPath, json);
    Console.WriteLine($"Written chirality analysis: {outPath}");
    return 0;
}

static int AnalyzeConjugation(string[] args)
{
    if (args.Length < 2)
    {
        Console.Error.WriteLine("Usage: gu analyze-conjugation <runFolder> [--modes <path>] [--out <path>]");
        return 1;
    }

    var runFolder = args[1];
    if (!Directory.Exists(runFolder))
    {
        Console.Error.WriteLine($"Run folder not found: {runFolder}");
        return 1;
    }

    var modesFlag = ParseFlag(args, "--modes", "");
    var outFlag = ParseFlag(args, "--out", "");

    var provenance = BuildP4Provenance(runFolder);
    var fermionsDir = Path.Combine(runFolder, "fermions");
    Directory.CreateDirectory(fermionsDir);

    // Load modes
    var modesPath = !string.IsNullOrEmpty(modesFlag) ? modesFlag
        : Path.Combine(fermionsDir, "fermion_modes.json");
    if (!File.Exists(modesPath))
    {
        Console.Error.WriteLine($"Fermion modes not found: {modesPath}. Run 'solve-fermion-modes' first.");
        return 1;
    }

    var opts = new JsonSerializerOptions { Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() } };
    var spectralResult = JsonSerializer.Deserialize<FermionSpectralResult>(File.ReadAllText(modesPath), opts);
    if (spectralResult is null)
    {
        Console.Error.WriteLine($"Failed to deserialize FermionSpectralResult: {modesPath}");
        return 1;
    }
    Console.WriteLine($"  Loaded {spectralResult.Modes.Count} modes from {modesPath}");

    // Build spinor spec and gammas
    var spinorSpec = BuildDefaultSpinorSpec(provenance);
    var gammaBuilder = new GammaMatrixBuilder();
    var gammas = gammaBuilder.Build(spinorSpec.CliffordSignature, spinorSpec.GammaConvention, provenance);

    var conjAnalyzer = new ConjugationAnalyzer();
    var pairs = conjAnalyzer.FindPairs(spectralResult.Modes, spinorSpec.ConjugationConvention, gammas);

    Console.WriteLine($"  Found {pairs.Count} conjugation pairs");
    foreach (var p in pairs)
        Console.WriteLine($"  {p.ModeIdA} <-> {p.ModeIdB} ({p.ConjugationType})");

    // Write conjugation pairs
    var outPath = !string.IsNullOrEmpty(outFlag) ? outFlag
        : Path.Combine(fermionsDir, "conjugation_pairs.json");
    var json = JsonSerializer.Serialize(pairs, new JsonSerializerOptions
    {
        WriteIndented = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() },
    });
    File.WriteAllText(outPath, json);
    Console.WriteLine($"Written conjugation pairs: {outPath}");
    return 0;
}

static int ExtractCouplings(string[] args)
{
    if (args.Length < 2)
    {
        Console.Error.WriteLine("Usage: gu extract-couplings <runFolder> [--boson-registry <json>] [--fermion-modes <path>] [--out <path>]");
        return 1;
    }

    var runFolder = args[1];
    if (!Directory.Exists(runFolder))
    {
        Console.Error.WriteLine($"Run folder not found: {runFolder}");
        return 1;
    }

    var bosonRegistryFlag = ParseFlag(args, "--boson-registry", "");
    var fermionModesFlag = ParseFlag(args, "--fermion-modes", "");
    var outFlag = ParseFlag(args, "--out", "");

    var provenance = BuildP4Provenance(runFolder);
    var fermionsDir = Path.Combine(runFolder, "fermions");
    var couplingsDir = Path.Combine(fermionsDir, "couplings");
    Directory.CreateDirectory(couplingsDir);

    // Load fermion modes
    var modesPath = !string.IsNullOrEmpty(fermionModesFlag) ? fermionModesFlag
        : Path.Combine(fermionsDir, "fermion_modes.json");
    if (!File.Exists(modesPath))
    {
        Console.Error.WriteLine($"Fermion modes not found: {modesPath}. Run 'solve-fermion-modes' first.");
        return 1;
    }

    var opts = new JsonSerializerOptions { Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() } };
    var spectralResult = JsonSerializer.Deserialize<FermionSpectralResult>(File.ReadAllText(modesPath), opts);
    if (spectralResult is null)
    {
        Console.Error.WriteLine($"Failed to deserialize FermionSpectralResult: {modesPath}");
        return 1;
    }

    Console.WriteLine($"  Loaded {spectralResult.Modes.Count} fermion modes");

    // Build empty variation matrices for now (zero coupling atlas when no Dirac bundle is available)
    // In production, each boson mode would produce a finite-difference delta_D matrix.
    var fermionModes = spectralResult.Modes.ToList();
    var variationMatrices = new Dictionary<string, (double[,] Re, double[,]? Im)>();

    string bosonRegistryVersion = "1.0.0";
    string backgroundId = spectralResult.FermionBackgroundId;

    // Optionally load boson registry to get boson mode IDs
    if (!string.IsNullOrEmpty(bosonRegistryFlag) && File.Exists(bosonRegistryFlag))
    {
        try
        {
            var bosonRegistry = Gu.Phase3.Registry.BosonRegistry.FromJson(File.ReadAllText(bosonRegistryFlag));
            bosonRegistryVersion = bosonRegistry.RegistryVersion ?? "1.0.0";
            int totalDof = spectralResult.Modes.Count > 0
                ? spectralResult.Modes[0].EigenvectorCoefficients?.Length / 2 ?? 1
                : 1;
            foreach (var candidate in bosonRegistry.Candidates.Take(5))
            {
                // Use zero variation matrix as placeholder
                variationMatrices[candidate.CandidateId] = (new double[totalDof, totalDof], null);
            }
            Console.WriteLine($"  Loaded boson registry: {bosonRegistry.Candidates.Count} bosons");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  Warning: could not load boson registry: {ex.Message}. Using empty coupling atlas.");
        }
    }
    else
    {
        Console.WriteLine("  No boson registry provided; building zero coupling atlas.");
    }

    var assembler = new CpuDiracOperatorAssembler();
    var engine = new CouplingProxyEngine(assembler);
    var atlasId = $"coupling-atlas-{backgroundId}";

    CouplingAtlas couplingAtlas;
    if (variationMatrices.Count > 0)
    {
        couplingAtlas = engine.BuildAtlas(
            atlasId, backgroundId, fermionModes,
            variationMatrices, "unit-modes", bosonRegistryVersion, provenance);
    }
    else
    {
        couplingAtlas = new CouplingAtlas
        {
            AtlasId = atlasId,
            FermionBackgroundId = backgroundId,
            BosonRegistryVersion = bosonRegistryVersion,
            Couplings = new List<BosonFermionCouplingRecord>(),
            NormalizationConvention = "unit-modes",
            Provenance = provenance,
        };
    }

    Console.WriteLine($"  Built coupling atlas: {couplingAtlas.Couplings.Count} coupling records");

    var outPath = !string.IsNullOrEmpty(outFlag) ? outFlag
        : Path.Combine(couplingsDir, "coupling_atlas.json");
    var json = JsonSerializer.Serialize(couplingAtlas, new JsonSerializerOptions
    {
        WriteIndented = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() },
    });
    File.WriteAllText(outPath, json);
    Console.WriteLine($"Written coupling atlas: {outPath}");
    return 0;
}

static int BuildFamilyClusters(string[] args)
{
    if (args.Length < 2)
    {
        Console.Error.WriteLine("Usage: gu build-family-clusters <runFolder> [--modes <path>] [--out <path>]");
        return 1;
    }

    var runFolder = args[1];
    if (!Directory.Exists(runFolder))
    {
        Console.Error.WriteLine($"Run folder not found: {runFolder}");
        return 1;
    }

    var modesFlag = ParseFlag(args, "--modes", "");
    var outFlag = ParseFlag(args, "--out", "");

    var provenance = BuildP4Provenance(runFolder);
    var fermionsDir = Path.Combine(runFolder, "fermions");
    Directory.CreateDirectory(fermionsDir);

    // Load fermion modes
    var modesPath = !string.IsNullOrEmpty(modesFlag) ? modesFlag
        : Path.Combine(fermionsDir, "fermion_modes.json");
    if (!File.Exists(modesPath))
    {
        Console.Error.WriteLine($"Fermion modes not found: {modesPath}. Run 'solve-fermion-modes' first.");
        return 1;
    }

    var opts = new JsonSerializerOptions { Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() } };
    var spectralResult = JsonSerializer.Deserialize<FermionSpectralResult>(File.ReadAllText(modesPath), opts);
    if (spectralResult is null)
    {
        Console.Error.WriteLine($"Failed to deserialize FermionSpectralResult: {modesPath}");
        return 1;
    }

    Console.WriteLine($"  Loaded {spectralResult.Modes.Count} modes from {modesPath}");

    // Build named results (two contexts: primary + perturbed copy for tracking machinery)
    var modes = spectralResult.Modes;
    var backgroundId = spectralResult.FermionBackgroundId;

    var namedResult1 = new NamedSpectralResult
    {
        ContextId = "ctx-primary",
        BackgroundId = backgroundId,
        Modes = modes,
    };
    // Produce a minimally perturbed second context to exercise tracking
    var perturbedModes = modes.Select((m, i) => new FermionModeRecord
    {
        ModeId = m.ModeId + "-p",
        BackgroundId = backgroundId + "-perturbed",
        BranchVariantId = m.BranchVariantId,
        LayoutId = m.LayoutId,
        ModeIndex = m.ModeIndex,
        EigenvalueRe = m.EigenvalueRe * 1.01,
        EigenvalueIm = m.EigenvalueIm,
        ResidualNorm = m.ResidualNorm,
        EigenvectorCoefficients = m.EigenvectorCoefficients,
        ChiralityDecomposition = m.ChiralityDecomposition,
        ConjugationPairing = m.ConjugationPairing,
        GaugeLeakScore = m.GaugeLeakScore,
        GaugeReductionApplied = m.GaugeReductionApplied,
        Backend = m.Backend,
        ComputedWithUnverifiedGpu = m.ComputedWithUnverifiedGpu,
        BranchStabilityScore = m.BranchStabilityScore,
        RefinementStabilityScore = m.RefinementStabilityScore,
        ReplayTier = m.ReplayTier,
        AmbiguityNotes = m.AmbiguityNotes,
        Provenance = m.Provenance,
    }).ToList<FermionModeRecord>();
    var namedResult2 = new NamedSpectralResult
    {
        ContextId = "ctx-perturbed-001",
        BackgroundId = backgroundId + "-perturbed",
        Modes = perturbedModes,
    };

    var trackingConfig = new FermionTrackingConfig();
    var atlasBuilder = new FermionFamilyAtlasBuilder(trackingConfig);
    var fermionAtlas = atlasBuilder.Build(
        "fermion-atlas-cli",
        "branch-family-cli-v1",
        new List<NamedSpectralResult> { namedResult1, namedResult2 },
        provenance);

    var clusterConfig = FamilyClusteringConfig.Default;
    var clusterBuilder = new FamilyClusterReportBuilder(clusterConfig);
    var clusterReport = clusterBuilder.Build(fermionAtlas, "cluster-report-cli", provenance);

    Console.WriteLine($"  Built fermion atlas: {fermionAtlas.Families.Count} families");
    Console.WriteLine($"  Built cluster report: {clusterReport.Clusters.Count} clusters");

    // Write fermion atlas
    var atlasPath = Path.Combine(fermionsDir, "fermion_families.json");
    var atlasJson = JsonSerializer.Serialize(fermionAtlas, new JsonSerializerOptions
    {
        WriteIndented = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() },
    });
    File.WriteAllText(atlasPath, atlasJson);
    Console.WriteLine($"Written fermion family atlas: {atlasPath}");

    // Write cluster report
    var clusterPath = !string.IsNullOrEmpty(outFlag) ? outFlag
        : Path.Combine(fermionsDir, "family_cluster_report.json");
    var clusterJson = JsonSerializer.Serialize(clusterReport, new JsonSerializerOptions
    {
        WriteIndented = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() },
    });
    File.WriteAllText(clusterPath, clusterJson);
    Console.WriteLine($"Written family cluster report: {clusterPath}");
    return 0;
}

static int BuildUnifiedRegistry(string[] args)
{
    if (args.Length < 2)
    {
        Console.Error.WriteLine("Usage: gu build-unified-registry <runFolder> [--boson-registry <path>] [--out <path>]");
        return 1;
    }

    var runFolder = args[1];
    if (!Directory.Exists(runFolder))
    {
        Console.Error.WriteLine($"Run folder not found: {runFolder}");
        return 1;
    }

    var bosonRegistryFlag = ParseFlag(args, "--boson-registry", "");
    var outFlag = ParseFlag(args, "--out", "");

    var provenance = BuildP4Provenance(runFolder);
    var fermionsDir = Path.Combine(runFolder, "fermions");
    var registryDir = Path.Combine(runFolder, "particle_registry");
    Directory.CreateDirectory(registryDir);

    var opts = new JsonSerializerOptions { Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() } };

    // Load fermion family atlas
    var atlasPath = Path.Combine(fermionsDir, "fermion_families.json");
    FermionFamilyAtlas? fermionAtlas = null;
    if (File.Exists(atlasPath))
    {
        fermionAtlas = JsonSerializer.Deserialize<FermionFamilyAtlas>(File.ReadAllText(atlasPath), opts);
        Console.WriteLine($"  Loaded fermion atlas: {fermionAtlas?.Families.Count ?? 0} families");
    }
    else
    {
        Console.WriteLine("  No fermion atlas found; using empty atlas.");
    }

    // Load family cluster report for clusters
    List<FamilyClusterRecord>? clusters = null;
    var clusterReportPath = Path.Combine(fermionsDir, "family_cluster_report.json");
    if (File.Exists(clusterReportPath))
    {
        var clusterReport = JsonSerializer.Deserialize<FamilyClusterReport>(File.ReadAllText(clusterReportPath), opts);
        clusters = clusterReport?.Clusters;
        Console.WriteLine($"  Loaded cluster report: {clusters?.Count ?? 0} clusters");
    }

    // Load coupling atlas
    var couplingPath = Path.Combine(fermionsDir, "couplings", "coupling_atlas.json");
    CouplingAtlas? couplingAtlas = null;
    if (File.Exists(couplingPath))
    {
        couplingAtlas = JsonSerializer.Deserialize<CouplingAtlas>(File.ReadAllText(couplingPath), opts);
        Console.WriteLine($"  Loaded coupling atlas: {couplingAtlas?.Couplings.Count ?? 0} couplings");
    }

    // Load boson registry (Phase III)
    Gu.Phase3.Registry.BosonRegistry? bosonRegistry = null;
    var bosonRegistryPath = !string.IsNullOrEmpty(bosonRegistryFlag) ? bosonRegistryFlag
        : Path.Combine(runFolder, "bosons", "boson_registry.json");
    if (File.Exists(bosonRegistryPath))
    {
        try
        {
            bosonRegistry = Gu.Phase3.Registry.BosonRegistry.FromJson(File.ReadAllText(bosonRegistryPath));
            Console.WriteLine($"  Loaded boson registry: {bosonRegistry.Candidates.Count} bosons");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  Warning: could not load boson registry: {ex.Message}");
        }
    }
    else
    {
        Console.WriteLine("  No boson registry found; unified registry will contain fermions only.");
    }

    // Build unified registry using RegistryMergeEngine
    var mergeConfig = new RegistryMergeConfig
    {
        RegistryVersion = "1.0.0",
        MinBranchPersistenceThreshold = 0.3,
        MinBranchPersistenceForC2 = 0.6,
        MinObservationConfidence = 0.5,
        AmbiguityCountThreshold = 2,
        AmbiguityScoreThreshold = 0.5,
        MinBranchStabilityForInteractionC1 = 0.5,
    };
    var mergeEngine = new RegistryMergeEngine(mergeConfig);
    var unifiedRegistry = mergeEngine.Build(bosonRegistry, clusters, fermionAtlas, couplingAtlas, provenance);

    Console.WriteLine($"  Built unified registry: {unifiedRegistry.Count} candidates");

    var outPath = !string.IsNullOrEmpty(outFlag) ? outFlag
        : Path.Combine(registryDir, "unified_particle_registry.json");
    var json = unifiedRegistry.ToJson();
    File.WriteAllText(outPath, json);
    Console.WriteLine($"Written unified particle registry: {outPath}");

    // Schema validation (best-effort)
    var schemaPath = Path.Combine(Directory.GetCurrentDirectory(), "schemas/phase4/unified_particle_registry.schema.json");
    if (File.Exists(schemaPath))
    {
        var result = SchemaValidator.ValidateWithSchemaFile(json, schemaPath);
        Console.WriteLine(result.IsValid ? "  Schema: valid" : $"  Schema: INVALID — {string.Join(", ", result.Errors)}");
    }

    return 0;
}

static Gu.Phase4.Couplings.CouplingAtlas BuildEmptyCouplingAtlas()
{
    return new Gu.Phase4.Couplings.CouplingAtlas
    {
        AtlasId = "empty",
        FermionBackgroundId = "unknown",
        BosonRegistryVersion = "1.0.0",
        Couplings = new List<Gu.Phase4.Couplings.BosonFermionCouplingRecord>(),
        NormalizationConvention = "unit-mode-norms",
        Provenance = new Gu.Core.ProvenanceMeta
        {
            CreatedAt = DateTimeOffset.UtcNow,
            CodeRevision = "cli-generated",
            Branch = new Gu.Core.BranchRef { BranchId = "unknown", SchemaVersion = "1.0.0" },
        },
    };
}

// ---------------------------------------------------------------------------
// Phase V CLI commands
// ---------------------------------------------------------------------------

static ProvenanceMeta BuildP5Provenance(string codeRevision = "cli-phase5")
{
    return new ProvenanceMeta
    {
        CreatedAt = DateTimeOffset.UtcNow,
        CodeRevision = codeRevision,
        Branch = new BranchRef { BranchId = "unknown", SchemaVersion = "1.0.0" },
        Notes = "Phase V CLI provenance.",
    };
}

static int BranchRobustness(string[] args)
{
    var studyPath = ParseFlag(args, "--study", "");
    var valuesPath = ParseFlag(args, "--values", "");
    var outFlag = ParseFlag(args, "--out", "");

    if (string.IsNullOrEmpty(studyPath))
    {
        Console.Error.WriteLine("Usage: gu branch-robustness --study <study.json> --values <values.json> [--out <record.json>]");
        return 1;
    }
    if (string.IsNullOrEmpty(valuesPath))
    {
        Console.Error.WriteLine("Usage: gu branch-robustness --study <study.json> --values <values.json> [--out <record.json>]");
        return 1;
    }
    if (!File.Exists(studyPath))
    {
        Console.Error.WriteLine($"Study file not found: {studyPath}");
        return 1;
    }
    if (!File.Exists(valuesPath))
    {
        Console.Error.WriteLine($"Values file not found: {valuesPath}");
        return 1;
    }

    var spec = JsonSerializer.Deserialize<BranchRobustnessStudySpec>(File.ReadAllText(studyPath),
        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    if (spec is null)
    {
        Console.Error.WriteLine($"Failed to deserialize BranchRobustnessStudySpec: {studyPath}");
        return 1;
    }

    IReadOnlyDictionary<string, double[]> quantityValues;
    try
    {
        quantityValues = BranchQuantityValuesLoader.Load(valuesPath, spec.BranchVariantIds);
    }
    catch (InvalidDataException ex)
    {
        Console.Error.WriteLine($"Failed to deserialize quantity values: {valuesPath}");
        Console.Error.WriteLine(ex.Message);
        return 1;
    }

    var provenance = BuildP5Provenance();
    var engine = new BranchRobustnessEngine(spec);
    var record = engine.Run(quantityValues, provenance);

    var resultJson = JsonSerializer.Serialize(record, new JsonSerializerOptions { WriteIndented = true });
    if (!string.IsNullOrEmpty(outFlag))
    {
        File.WriteAllText(outFlag, resultJson);
        Console.WriteLine($"Written branch robustness record: {outFlag}");
    }
    else
    {
        Console.WriteLine(resultJson);
    }
    Console.WriteLine("branch-robustness done.");
    return 0;
}

static int RefinementStudy(string[] args)
{
    var specPath = ParseFlag(args, "--spec", "");
    var valuesPath = ParseFlag(args, "--values", "");
    var outFlag = ParseFlag(args, "--out", "");

    if (string.IsNullOrEmpty(specPath) || string.IsNullOrEmpty(valuesPath))
    {
        Console.Error.WriteLine("Usage: gu refinement-study --spec <spec.json> --values <values.json> [--out <result.json>]");
        return 1;
    }
    if (!File.Exists(specPath))
    {
        Console.Error.WriteLine($"Spec file not found: {specPath}");
        return 1;
    }
    if (!File.Exists(valuesPath))
    {
        Console.Error.WriteLine($"Values file not found: {valuesPath}");
        return 1;
    }

    var spec = JsonSerializer.Deserialize<RefinementStudySpec>(File.ReadAllText(specPath),
        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    if (spec is null)
    {
        Console.Error.WriteLine($"Failed to deserialize RefinementStudySpec: {specPath}");
        return 1;
    }

    var valueTable = JsonSerializer.Deserialize<RefinementQuantityValueTable>(
        File.ReadAllText(valuesPath),
        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    if (valueTable is null)
    {
        Console.Error.WriteLine($"Failed to deserialize RefinementQuantityValueTable: {valuesPath}");
        return 1;
    }

    // Validate that level IDs in values file match the spec
    var specLevelIds = spec.RefinementLevels.Select(l => l.LevelId).ToHashSet();
    var valueLevelIds = valueTable.Levels.Select(l => l.LevelId).ToHashSet();
    var missingInValues = specLevelIds.Except(valueLevelIds).ToList();
    var extraInValues = valueLevelIds.Except(specLevelIds).ToList();
    if (missingInValues.Count > 0)
    {
        Console.Error.WriteLine($"Values file is missing level IDs: {string.Join(", ", missingInValues)}");
        return 1;
    }
    if (extraInValues.Count > 0)
    {
        Console.Error.WriteLine($"Values file has extra level IDs not in spec: {string.Join(", ", extraInValues)}");
        return 1;
    }

    var valueLookup = valueTable.Levels.ToDictionary(l => l.LevelId);

    // Executor: look up pre-computed values; throw on solverConverged=false so runner records failure
    IReadOnlyDictionary<string, double> pipelineExecutor(RefinementLevel level)
    {
        var entry = valueLookup[level.LevelId];
        if (!entry.SolverConverged)
            throw new InvalidOperationException(
                $"Solver did not converge at level '{level.LevelId}' (solverConverged=false in values file).");
        return entry.Quantities;
    }

    var runner = new RefinementStudyRunner();
    var result = runner.Run(spec, pipelineExecutor);

    var resultJson = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
    if (!string.IsNullOrEmpty(outFlag))
    {
        File.WriteAllText(outFlag, resultJson);
        Console.WriteLine($"Written refinement study result: {outFlag}");
    }
    else
    {
        Console.WriteLine(resultJson);
    }
    Console.WriteLine("refinement-study done.");
    return 0;
}

static int ImportEnvironment(string[] args)
{
    var specPath = ParseFlag(args, "--spec", "");
    var outFlag = ParseFlag(args, "--out", "");

    if (string.IsNullOrEmpty(specPath))
    {
        Console.Error.WriteLine("Usage: gu import-environment --spec <spec.json> [--out <record.json>]");
        return 1;
    }
    if (!File.Exists(specPath))
    {
        Console.Error.WriteLine($"Spec file not found: {specPath}");
        return 1;
    }

    var spec = JsonSerializer.Deserialize<EnvironmentImportSpec>(File.ReadAllText(specPath),
        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    if (spec is null)
    {
        Console.Error.WriteLine($"Failed to deserialize EnvironmentImportSpec: {specPath}");
        return 1;
    }

    var record = EnvironmentImporter.Import(spec);

    var resultJson = JsonSerializer.Serialize(record, new JsonSerializerOptions { WriteIndented = true });
    if (!string.IsNullOrEmpty(outFlag))
    {
        File.WriteAllText(outFlag, resultJson);
        Console.WriteLine($"Written environment record: {outFlag}");
    }
    else
    {
        Console.WriteLine(resultJson);
    }
    Console.WriteLine("import-environment done.");
    return 0;
}

static int BuildStructuredEnvironment(string[] args)
{
    var specPath = ParseFlag(args, "--spec", "");
    var outFlag = ParseFlag(args, "--out", "");

    if (string.IsNullOrEmpty(specPath))
    {
        Console.Error.WriteLine("Usage: gu build-structured-environment --spec <spec.json> [--out <record.json>]");
        return 1;
    }
    if (!File.Exists(specPath))
    {
        Console.Error.WriteLine($"Spec file not found: {specPath}");
        return 1;
    }

    var spec = JsonSerializer.Deserialize<StructuredEnvironmentSpec>(File.ReadAllText(specPath),
        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    if (spec is null)
    {
        Console.Error.WriteLine($"Failed to deserialize StructuredEnvironmentSpec: {specPath}");
        return 1;
    }

    var record = StructuredEnvironmentGenerator.Generate(spec);

    var resultJson = JsonSerializer.Serialize(record, new JsonSerializerOptions { WriteIndented = true });
    if (!string.IsNullOrEmpty(outFlag))
    {
        File.WriteAllText(outFlag, resultJson);
        Console.WriteLine($"Written structured environment record: {outFlag}");
    }
    else
    {
        Console.WriteLine(resultJson);
    }
    Console.WriteLine("build-structured-environment done.");
    return 0;
}

static int ValidateQuantitative(string[] args)
{
    var observablesPath = ParseFlag(args, "--observables", "");
    var targetsPath = ParseFlag(args, "--targets", "");
    var outFlag = ParseFlag(args, "--out", "");

    if (string.IsNullOrEmpty(observablesPath) || string.IsNullOrEmpty(targetsPath))
    {
        Console.Error.WriteLine("Usage: gu validate-quantitative --observables <obs.json> --targets <targets.json> [--out <scorecard.json>]");
        return 1;
    }
    if (!File.Exists(observablesPath))
    {
        Console.Error.WriteLine($"Observables file not found: {observablesPath}");
        return 1;
    }
    if (!File.Exists(targetsPath))
    {
        Console.Error.WriteLine($"Targets file not found: {targetsPath}");
        return 1;
    }

    var observables = JsonSerializer.Deserialize<List<QuantitativeObservableRecord>>(File.ReadAllText(observablesPath),
        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    if (observables is null)
    {
        Console.Error.WriteLine($"Failed to deserialize observables: {observablesPath}");
        return 1;
    }

    var targetTable = ExternalTargetTable.FromJson(File.ReadAllText(targetsPath));

    var policy = new CalibrationPolicy
    {
        PolicyId = "cli-standard",
        Mode = "standard",
        SigmaThreshold = 5.0,
        RequireFullUncertainty = false,
    };

    var provenance = BuildP5Provenance();
    var runner = new QuantitativeValidationRunner();
    var studyId = $"cli-validation-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}";
    var scoreCard = runner.Run(studyId, observables, targetTable, policy, provenance);

    var resultJson = JsonSerializer.Serialize(scoreCard, new JsonSerializerOptions { WriteIndented = true });
    if (!string.IsNullOrEmpty(outFlag))
    {
        File.WriteAllText(outFlag, resultJson);
        Console.WriteLine($"Written consistency scorecard: {outFlag}");
    }
    else
    {
        Console.WriteLine(resultJson);
    }
    Console.WriteLine("validate-quantitative done.");
    return 0;
}

static int BuildValidationDossier(string[] args)
{
    var manifestPath = ParseFlag(args, "--study-manifest", "");
    var outFlag = ParseFlag(args, "--out", "");

    if (string.IsNullOrEmpty(manifestPath))
    {
        Console.Error.WriteLine("Usage: gu build-validation-dossier --study-manifest <manifest.json> [--out <dossier.json>]");
        return 1;
    }
    if (!File.Exists(manifestPath))
    {
        Console.Error.WriteLine($"Study manifest not found: {manifestPath}");
        return 1;
    }

    var studies = JsonSerializer.Deserialize<List<StudyManifest>>(File.ReadAllText(manifestPath),
        new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() },
        });
    if (studies is null)
    {
        Console.Error.WriteLine($"Failed to deserialize study manifests: {manifestPath}");
        return 1;
    }

    var provenance = BuildP5Provenance();
    var dossierId = $"dossier-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}";
    var dossier = DossierAssembler.Assemble(dossierId, "Phase V Validation Dossier", studies, provenance);

    var resultJson = JsonSerializer.Serialize(dossier, new JsonSerializerOptions
    {
        WriteIndented = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() },
    });
    if (!string.IsNullOrEmpty(outFlag))
    {
        File.WriteAllText(outFlag, resultJson);
        Console.WriteLine($"Written validation dossier: {outFlag}");
    }
    else
    {
        Console.WriteLine(resultJson);
    }
    Console.WriteLine("build-validation-dossier done.");
    return 0;
}

static int VerifyStudyFreshness(string[] args)
{
    var dossierPath = ParseFlag(args, "--dossier", "");

    if (string.IsNullOrEmpty(dossierPath))
    {
        Console.Error.WriteLine("Usage: gu verify-study-freshness --dossier <dossier.json>");
        return 1;
    }
    if (!File.Exists(dossierPath))
    {
        Console.Error.WriteLine($"Dossier file not found: {dossierPath}");
        return 1;
    }

    var dossier = JsonSerializer.Deserialize<ValidationDossier>(File.ReadAllText(dossierPath),
        new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() },
        });
    if (dossier is null)
    {
        Console.Error.WriteLine($"Failed to deserialize ValidationDossier: {dossierPath}");
        return 1;
    }

    Console.WriteLine($"Dossier: {dossier.DossierId}");
    Console.WriteLine($"Title: {dossier.Title}");
    Console.WriteLine($"Overall evidence tier: {dossier.OverallEvidenceTier}");
    Console.WriteLine($"Acceptable as evidence: {dossier.IsAcceptableAsEvidence}");
    Console.WriteLine($"Verdict: {dossier.EvidenceVerdict}");
    if (dossier.StaleStudyIds.Count > 0)
    {
        Console.WriteLine($"Stale studies: {string.Join(", ", dossier.StaleStudyIds)}");
    }
    if (dossier.AssemblyNotes is not null)
    {
        foreach (var note in dossier.AssemblyNotes)
            Console.WriteLine($"  {note}");
    }
    Console.WriteLine("verify-study-freshness done.");
    return dossier.IsAcceptableAsEvidence ? 0 : 1;
}

static int RunPhase5Campaign(string[] args)
{
    var specPath = ParseFlag(args, "--spec", "");
    var outDir = ParseFlag(args, "--out-dir", "");
    var validateFirst = args.Contains("--validate-first");

    if (string.IsNullOrEmpty(specPath))
    {
        Console.Error.WriteLine("Usage: gu run-phase5-campaign --spec <campaign.json> --out-dir <dir> [--validate-first]");
        return 1;
    }
    if (string.IsNullOrEmpty(outDir))
    {
        Console.Error.WriteLine("Usage: gu run-phase5-campaign --spec <campaign.json> --out-dir <dir> [--validate-first]");
        return 1;
    }
    if (!File.Exists(specPath))
    {
        Console.Error.WriteLine($"Campaign spec file not found: {specPath}");
        return 1;
    }

    var specJson = File.ReadAllText(specPath);
    var spec = GuJsonDefaults.Deserialize<Phase5CampaignSpec>(specJson);
    if (spec is null)
    {
        Console.Error.WriteLine($"Failed to deserialize Phase5CampaignSpec: {specPath}");
        return 1;
    }

    var specDir = Path.GetDirectoryName(Path.GetFullPath(specPath))!;

    // --validate-first: run Phase5CampaignSpecValidator before loading artifacts (D-P6-001)
    if (validateFirst)
    {
        var validationResult = Phase5CampaignSpecValidator.Validate(spec, specDir, requireReferenceSidecars: true);
        if (!validationResult.IsValid)
        {
            Console.Error.WriteLine($"Campaign spec validation failed ({validationResult.Errors.Count} error(s)):");
            foreach (var error in validationResult.Errors)
                Console.Error.WriteLine($"  - {error}");
            return 1;
        }
        Console.WriteLine("Campaign spec validation: OK");
    }

    // Load all campaign artifacts
    Phase5CampaignArtifacts artifacts;
    try
    {
        artifacts = Phase5CampaignArtifactLoader.Load(spec, specDir);
    }
    catch (ArtifactLoadException ex)
    {
        Console.Error.WriteLine($"Artifact load error: {ex.Message}");
        return 1;
    }

    // Build branch executor from loaded branch quantity values table
    // branchVariantIds are the level IDs in the branch quantity values table
    Func<string, IReadOnlyDictionary<string, double[]>> branchExecutor = variantId =>
    {
        var level = artifacts.BranchQuantityValues.Levels
            .FirstOrDefault(l => l.LevelId == variantId);
        if (level is null)
            return new Dictionary<string, double[]>();
        return level.Quantities.ToDictionary(
            kv => kv.Key,
            kv => new double[] { kv.Value });
    };

    // Build refinement executor from loaded refinement values table
    Func<RefinementLevel, IReadOnlyDictionary<string, double>> refinementExecutor = level =>
    {
        var valueLevel = artifacts.RefinementValues.Levels
            .FirstOrDefault(l => l.LevelId == level.LevelId);
        if (valueLevel is null)
            return new Dictionary<string, double>();
        return valueLevel.Quantities;
    };

    // Run the campaign
    var runner = new Phase5CampaignRunner();
    Phase5CampaignResult result;
    try
    {
        result = runner.RunFull(
            spec,
            branchExecutor,
            refinementExecutor,
            artifacts.Observables,
            artifacts.TargetTable,
            artifacts.Registry,
            candidateProvenanceLinks: artifacts.CandidateProvenanceLinks,
            observationChainRecords: artifacts.ObservationChainRecords,
            environmentRecords: artifacts.EnvironmentRecords,
            environmentVarianceRecords: artifacts.EnvironmentVarianceRecords,
            representationContentRecords: artifacts.RepresentationContentRecords,
            couplingConsistencyRecords: artifacts.CouplingConsistencyRecords,
            sidecarSummary: artifacts.SidecarSummary,
            refinementBridgeManifest: artifacts.RefinementBridgeManifest);
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Campaign run failed: {ex.Message}");
        return 1;
    }

    // Write output artifact tree
    Directory.CreateDirectory(Path.Combine(outDir, "branch"));
    Directory.CreateDirectory(Path.Combine(outDir, "convergence"));
    Directory.CreateDirectory(Path.Combine(outDir, "quantitative"));
    Directory.CreateDirectory(Path.Combine(outDir, "falsification"));
    Directory.CreateDirectory(Path.Combine(outDir, "dossiers"));
    Directory.CreateDirectory(Path.Combine(outDir, "reports"));
    Directory.CreateDirectory(Path.Combine(outDir, "inputs"));

    // inputs/ — copy authoritative input files for concrete reproduction metadata (D-P6-006)
    var absSpecPath = Path.GetFullPath(specPath);
    var inputsDir = Path.Combine(outDir, "inputs");
    File.Copy(absSpecPath, Path.Combine(inputsDir, "campaign.json"), overwrite: true);

    static void CopyIfExists(string src, string dst)
    {
        if (File.Exists(src))
            File.Copy(src, dst, overwrite: true);
    }

    CopyIfExists(Path.Combine(specDir, spec.BranchQuantityValuesPath), Path.Combine(inputsDir, "branch_quantity_values.json"));
    CopyIfExists(Path.Combine(specDir, spec.RefinementValuesPath), Path.Combine(inputsDir, "refinement_values.json"));
    CopyIfExists(Path.Combine(specDir, spec.ObservablesPath), Path.Combine(inputsDir, "observables.json"));
    CopyIfExists(Path.Combine(specDir, spec.ExternalTargetTablePath), Path.Combine(inputsDir, "external_targets.json"));
    CopyIfExists(Path.Combine(specDir, spec.RegistryPath), Path.Combine(inputsDir, "registry.json"));
    if (artifacts.RefinementBridgeManifest is not null)
        CopyIfExists(Path.Combine(Path.GetDirectoryName(Path.Combine(specDir, spec.RefinementValuesPath)) ?? specDir, "bridge_manifest.json"), Path.Combine(inputsDir, "bridge_manifest.json"));
    if (spec.ObservationChainPath is not null)
        CopyIfExists(Path.Combine(specDir, spec.ObservationChainPath), Path.Combine(inputsDir, "observation_chain.json"));
    if (spec.EnvironmentVariancePath is not null)
        CopyIfExists(Path.Combine(specDir, spec.EnvironmentVariancePath), Path.Combine(inputsDir, "environment_variance.json"));
    if (spec.RepresentationContentPath is not null)
        CopyIfExists(Path.Combine(specDir, spec.RepresentationContentPath), Path.Combine(inputsDir, "representation_content.json"));
    if (spec.CouplingConsistencyPath is not null)
        CopyIfExists(Path.Combine(specDir, spec.CouplingConsistencyPath), Path.Combine(inputsDir, "coupling_consistency.json"));
    CopyIfExists(Path.Combine(specDir, "sidecar_summary.json"), Path.Combine(inputsDir, "sidecar_summary.json"));
    CopyIfExists(Path.Combine(specDir, "candidate_provenance_links.json"), Path.Combine(inputsDir, "candidate_provenance_links.json"));
    for (int i = 0; i < spec.EnvironmentRecordPaths.Count; i++)
        CopyIfExists(Path.Combine(specDir, spec.EnvironmentRecordPaths[i]), Path.Combine(inputsDir, $"env_record_{i}.json"));

    // branch/branch_robustness_record.json
    var branchAtlas = result.Report.BranchIndependenceAtlas;
    if (branchAtlas is not null)
    {
        var branchJson = GuJsonDefaults.Serialize(branchAtlas);
        File.WriteAllText(Path.Combine(outDir, "branch", "branch_robustness_record.json"), branchJson);
    }

    // convergence/refinement_study_result.json
    var convergenceAtlas = result.Report.ConvergenceAtlas;
    if (convergenceAtlas is not null)
    {
        var convJson = GuJsonDefaults.Serialize(convergenceAtlas);
        File.WriteAllText(Path.Combine(outDir, "convergence", "refinement_study_result.json"), convJson);
    }

    // quantitative/consistency_scorecard.json — from typed dossier
    if (result.TypedDossier.QuantitativeComparison is not null)
    {
        var scorecardJson = GuJsonDefaults.Serialize(result.TypedDossier.QuantitativeComparison);
        File.WriteAllText(Path.Combine(outDir, "quantitative", "consistency_scorecard.json"), scorecardJson);
    }

    // falsification/falsifier_summary.json
    // Write the full FalsifierSummary (with coverage counts) from the typed dossier when available;
    // fall back to FalsificationDashboard for backward compatibility (D-P6-002).
    if (result.TypedDossier.FalsifierSummary is not null)
    {
        var falsJson = GuJsonDefaults.Serialize(result.TypedDossier.FalsifierSummary);
        File.WriteAllText(Path.Combine(outDir, "falsification", "falsifier_summary.json"), falsJson);
    }
    else
    {
        var falsificationDashboard = result.Report.FalsificationDashboard;
        if (falsificationDashboard is not null)
        {
            var falsJson = GuJsonDefaults.Serialize(falsificationDashboard);
            File.WriteAllText(Path.Combine(outDir, "falsification", "falsifier_summary.json"), falsJson);
        }
    }

    // dossiers/phase5_validation_dossier.json
    var typedDossierJson = GuJsonDefaults.Serialize(result.TypedDossier);
    File.WriteAllText(Path.Combine(outDir, "dossiers", "phase5_validation_dossier.json"), typedDossierJson);

    // dossiers/validation_dossier.json
    var provenanceDossierJson = GuJsonDefaults.Serialize(result.ProvenanceDossier);
    File.WriteAllText(Path.Combine(outDir, "dossiers", "validation_dossier.json"), provenanceDossierJson);

    // dossiers/study_manifest.json — array of two StudyManifest entries
    // Update reproduction commands to reference the concrete copied inputs (D-P6-006)
    var concreteReproCmd = $"dotnet run --project apps/Gu.Cli -- run-phase5-campaign --spec inputs/campaign.json --out-dir . --validate-first";
    var updatedManifests = result.StudyManifests.Select(m => new StudyManifest
    {
        StudyId = m.StudyId,
        Description = m.Description,
        RunFolder = m.RunFolder,
        Reproducibility = ReproducibilityBundle.CreateRegeneratedCpu(
            codeRevision: m.Provenance?.CodeRevision ?? spec.Provenance.CodeRevision,
            reproductionCommands: [concreteReproCmd]),
        Provenance = m.Provenance,
    }).ToList();
    var manifestsJson = GuJsonDefaults.Serialize(updatedManifests);
    File.WriteAllText(Path.Combine(outDir, "dossiers", "study_manifest.json"), manifestsJson);

    if (artifacts.SidecarSummary is not null)
    {
        var sidecarSummaryJson = GuJsonDefaults.Serialize(artifacts.SidecarSummary);
        File.WriteAllText(Path.Combine(outDir, "falsification", "sidecar_summary.json"), sidecarSummaryJson);
    }

    // reports/phase5_report.json
    var reportJson = GuJsonDefaults.Serialize(result.Report);
    File.WriteAllText(Path.Combine(outDir, "reports", "phase5_report.json"), reportJson);

    // reports/phase5_report.md
    var md = GeneratePhase5ReportMarkdown(result.Report, spec.CampaignId, result.TypedDossier.QuantitativeComparison);
    File.WriteAllText(Path.Combine(outDir, "reports", "phase5_report.md"), md);

    Console.WriteLine($"Phase V campaign complete. Output: {outDir}");
    Console.WriteLine($"  inputs/campaign.json  (+ supporting input copies)");
    Console.WriteLine($"  branch/branch_robustness_record.json");
    Console.WriteLine($"  convergence/refinement_study_result.json");
    Console.WriteLine($"  quantitative/consistency_scorecard.json");
    Console.WriteLine($"  falsification/falsifier_summary.json");
    if (artifacts.SidecarSummary is not null)
        Console.WriteLine($"  falsification/sidecar_summary.json");
    Console.WriteLine($"  dossiers/phase5_validation_dossier.json");
    Console.WriteLine($"  dossiers/validation_dossier.json");
    Console.WriteLine($"  dossiers/study_manifest.json");
    Console.WriteLine($"  reports/phase5_report.json");
    Console.WriteLine($"  reports/phase5_report.md");
    Console.WriteLine("run-phase5-campaign done.");
    return 0;
}

static int BuildPhase5Sidecars(string[] args)
{
    var registryPath = ParseFlag(args, "--registry", "");
    var observablesPath = ParseFlag(args, "--observables", "");
    var outDir = ParseFlag(args, "--out-dir", "");

    if (string.IsNullOrEmpty(registryPath) || string.IsNullOrEmpty(outDir))
    {
        Console.Error.WriteLine("Usage: gu build-phase5-sidecars --registry <registry.json> --observables <observables.json> --environment-record <env.json>... --out-dir <dir>");
        return 1;
    }
    if (!File.Exists(registryPath))
    {
        Console.Error.WriteLine($"Registry file not found: {registryPath}");
        return 1;
    }

    // Parse multiple --environment-record flags
    var envRecordPaths = new List<string>();
    for (int i = 1; i < args.Length - 1; i++)
    {
        if (args[i] == "--environment-record")
            envRecordPaths.Add(args[i + 1]);
    }

    // Load registry
    var registryJson = File.ReadAllText(registryPath);
    var registry = UnifiedParticleRegistry.FromJson(registryJson);

    // Load observables (optional)
    IReadOnlyList<QuantitativeObservableRecord>? observables = null;
    if (!string.IsNullOrEmpty(observablesPath))
    {
        if (!File.Exists(observablesPath))
        {
            Console.Error.WriteLine($"Observables file not found: {observablesPath}");
            return 1;
        }
        var obsJson = File.ReadAllText(observablesPath);
        observables = GuJsonDefaults.Deserialize<List<QuantitativeObservableRecord>>(obsJson)
            ?? new List<QuantitativeObservableRecord>();
    }

    // Load environment records
    IReadOnlyList<EnvironmentRecord>? envRecords = null;
    if (envRecordPaths.Count > 0)
    {
        var loaded = new List<EnvironmentRecord>();
        foreach (var envPath in envRecordPaths)
        {
            if (!File.Exists(envPath))
            {
                Console.Error.WriteLine($"Environment record file not found: {envPath}");
                return 1;
            }
            var envJson = File.ReadAllText(envPath);
            var rec = GuJsonDefaults.Deserialize<EnvironmentRecord>(envJson);
            if (rec is not null)
                loaded.Add(rec);
        }
        envRecords = loaded;
    }

    // Build a minimal provenance
    var provenance = new ProvenanceMeta
    {
        CreatedAt = DateTimeOffset.UtcNow,
        CodeRevision = "cli",
        Branch = new BranchRef { BranchId = "cli", SchemaVersion = "1.0.0" },
        Backend = "cpu",
    };

    // Generate sidecars from the supplied registry / observables / environment records.
    // Null channels are treated as absent; derived channels become evaluated or skipped
    // based on whether the supporting inputs are rich enough to populate records.
    var studyId = Path.GetFileNameWithoutExtension(registryPath);
    var registryDir = Path.GetDirectoryName(Path.GetFullPath(registryPath)) ?? ".";
    string? InferFromRegistryDir(string fileName)
    {
        var candidate = Path.Combine(registryDir, fileName);
        return File.Exists(candidate) ? candidate : null;
    }

    var upstreamArtifacts = new SidecarUpstreamArtifacts
    {
        RegistryPath = Path.GetFullPath(registryPath),
        ObservablesPath = string.IsNullOrWhiteSpace(observablesPath) ? null : Path.GetFullPath(observablesPath),
        EnvironmentRecordPaths = envRecordPaths.Select(Path.GetFullPath).ToList(),
        FermionFamilyAtlasPath = InferFromRegistryDir("fermion_family_atlas.json"),
        CouplingAtlasPath = InferFromRegistryDir("coupling_atlas.json"),
        FermionSpectralResultPath = InferFromRegistryDir("fermion_spectral_result.json"),
        Phase4ReportPath = InferFromRegistryDir("phase4_report.json"),
    };

    var summary = SidecarGenerator.GenerateSidecars(
        registry,
        observables,
        envRecords,
        outDir,
        studyId,
        provenance,
        upstreamArtifacts: upstreamArtifacts);

    Console.WriteLine($"build-phase5-sidecars complete. Output: {outDir}");
    foreach (var ch in summary.Channels)
        Console.WriteLine($"  {ch.ChannelId}: {ch.Status} (input={ch.InputCount}, output={ch.OutputCount})");
    return 0;
}

static string GeneratePhase5ReportMarkdown(
    Phase5Report report,
    string campaignId,
    ConsistencyScoreCard? scoreCard = null)
{
    var sb = new System.Text.StringBuilder();
    sb.AppendLine($"# Phase V Validation Report: {campaignId}");
    sb.AppendLine();
    sb.AppendLine($"**Study ID:** {report.StudyId}");
    sb.AppendLine($"**Report date:** {DateTimeOffset.UtcNow:yyyy-MM-dd}");
    sb.AppendLine();
    sb.AppendLine("## Branch Independence");
    if (report.BranchIndependenceAtlas is not null)
    {
        sb.AppendLine($"- Invariant quantities: {report.BranchIndependenceAtlas.InvariantCount}");
        sb.AppendLine($"- Fragile quantities: {report.BranchIndependenceAtlas.FragileCount}");
        sb.AppendLine($"- Total analyzed: {report.BranchIndependenceAtlas.TotalQuantities}");
    }
    sb.AppendLine();
    sb.AppendLine("## Convergence");
    if (report.ConvergenceAtlas is not null)
    {
        sb.AppendLine($"- Convergent quantities: {report.ConvergenceAtlas.ConvergentCount}");
        sb.AppendLine($"- Non-convergent: {report.ConvergenceAtlas.NonConvergentCount}");
        foreach (var line in report.ConvergenceAtlas.SummaryLines
            .Where(line => line.StartsWith("- Evidence source:", StringComparison.Ordinal) ||
                           line.StartsWith("- Refinement seed family:", StringComparison.Ordinal) ||
                           line.StartsWith("- Direct solver-backed refinement family:", StringComparison.Ordinal)))
        {
            sb.AppendLine(line);
        }
    }
    sb.AppendLine();
    sb.AppendLine("## Quantitative");
    if (scoreCard is not null)
    {
        sb.AppendLine($"- Passed matches: {scoreCard.TotalPassed}");
        sb.AppendLine($"- Failed matches: {scoreCard.TotalFailed}");
        if (scoreCard.BenchmarkClassCounts is { Count: > 0 })
        {
            foreach (var entry in scoreCard.BenchmarkClassCounts.OrderBy(kv => kv.Key, StringComparer.Ordinal))
                sb.AppendLine($"- Benchmark class `{entry.Key}`: {entry.Value} match(es)");
        }
        if (scoreCard.FailedBenchmarkClassCounts is { Count: > 0 })
        {
            foreach (var entry in scoreCard.FailedBenchmarkClassCounts.OrderBy(kv => kv.Key, StringComparer.Ordinal))
                sb.AppendLine($"- Failed `{entry.Key}` matches: {entry.Value}");
        }
    }
    sb.AppendLine();
    sb.AppendLine("## Falsification");
    if (report.FalsificationDashboard is not null)
    {
        sb.AppendLine($"- Total falsifiers: {report.FalsificationDashboard.TotalFalsifiers}");
        sb.AppendLine($"- Active fatal: {report.FalsificationDashboard.ActiveFatalCount}");
        sb.AppendLine($"- Active high: {report.FalsificationDashboard.ActiveHighCount}");
        sb.AppendLine($"- Demotions: {report.FalsificationDashboard.DemotionCount}");
    }
    sb.AppendLine();
    sb.AppendLine("## Dossiers");
    foreach (var dossierId in report.DossierIds)
        sb.AppendLine($"- {dossierId}");
    sb.AppendLine();
    sb.AppendLine("---");
    sb.AppendLine();
    sb.AppendLine("**IMPORTANT:** This study mixes control-study targets and stronger benchmark targets.");
    sb.AppendLine("None of them is a real-world experimental measurement or a physical prediction.");
    sb.AppendLine();
    sb.AppendLine("**Reproduction command:**");
    sb.AppendLine("```bash");
    sb.AppendLine("dotnet run --project apps/Gu.Cli -- run-phase5-campaign --spec <campaign.json> --out-dir <dir>");
    sb.AppendLine("```");
    return sb.ToString();
}

static int ExportPhase5BridgeValues(string[] args)
{
    var atlasPath = ParseFlag(args, "--atlas", "");
    var refinementSpecPath = ParseFlag(args, "--refinement-spec", "");
    var outDir = ParseFlag(args, "--out-dir", "");

    if (string.IsNullOrEmpty(atlasPath))
    {
        Console.Error.WriteLine("Usage: gu export-phase5-bridge-values --atlas <atlas.json> --refinement-spec <spec.json> --out-dir <dir>");
        return 1;
    }
    if (string.IsNullOrEmpty(refinementSpecPath))
    {
        Console.Error.WriteLine("Usage: gu export-phase5-bridge-values --atlas <atlas.json> --refinement-spec <spec.json> --out-dir <dir>");
        return 1;
    }
    if (string.IsNullOrEmpty(outDir))
    {
        Console.Error.WriteLine("Usage: gu export-phase5-bridge-values --atlas <atlas.json> --refinement-spec <spec.json> --out-dir <dir>");
        return 1;
    }

    if (!File.Exists(atlasPath))
    {
        Console.Error.WriteLine($"Atlas file not found: {atlasPath}");
        return 1;
    }
    if (!File.Exists(refinementSpecPath))
    {
        Console.Error.WriteLine($"Refinement spec file not found: {refinementSpecPath}");
        return 1;
    }

    var atlasJson = File.ReadAllText(atlasPath);
    var atlas = BackgroundAtlasSerializer.DeserializeAtlas(atlasJson);
    if (atlas is null)
    {
        Console.Error.WriteLine($"Failed to deserialize BackgroundAtlas: {atlasPath}");
        return 1;
    }

    var specJson = File.ReadAllText(refinementSpecPath);
    var refinementSpec = GuJsonDefaults.Deserialize<RefinementStudySpec>(specJson);
    if (refinementSpec is null)
    {
        Console.Error.WriteLine($"Failed to deserialize RefinementStudySpec: {refinementSpecPath}");
        return 1;
    }

    var provenance = new ProvenanceMeta
    {
        CreatedAt = DateTimeOffset.UtcNow,
        CodeRevision = "export-phase5-bridge-values",
        Branch = new BranchRef { BranchId = atlas.StudyId, SchemaVersion = "1.0" },
        Backend = "cpu",
        Notes = $"Bridge export from atlas {atlas.AtlasId}",
    };

    BridgeManifest manifest;
    try
    {
        manifest = BridgeValueExporter.Export(
            atlas,
            refinementSpec,
            atlasSourcePath: Path.GetFullPath(atlasPath),
            outDir: outDir,
            provenance: provenance);
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Bridge export failed: {ex.Message}");
        return 1;
    }

    Console.WriteLine($"Bridge export complete. Output: {outDir}");
    Console.WriteLine($"  branch_quantity_values.json  ({manifest.SourceRecordIds.Count} branch variants)");
    Console.WriteLine($"  refinement_values.json       ({refinementSpec.RefinementLevels.Count} refinement levels)");
    Console.WriteLine($"  bridge_manifest.json         (manifestId: {manifest.ManifestId})");
    Console.WriteLine("export-phase5-bridge-values done.");
    return 0;
}

static int ValidatePhase5CampaignSpec(string[] args)
{
    var specPath = ParseFlag(args, "--spec", "");
    var requireSidecars = args.Contains("--require-reference-sidecars");

    if (string.IsNullOrEmpty(specPath))
    {
        Console.Error.WriteLine("Usage: gu validate-phase5-campaign-spec --spec <campaign.json> [--require-reference-sidecars]");
        return 1;
    }
    if (!File.Exists(specPath))
    {
        Console.Error.WriteLine($"Campaign spec file not found: {specPath}");
        return 1;
    }

    var specJson = File.ReadAllText(specPath);
    var spec = GuJsonDefaults.Deserialize<Phase5CampaignSpec>(specJson);
    if (spec is null)
    {
        Console.Error.WriteLine($"Failed to deserialize Phase5CampaignSpec: {specPath}");
        return 1;
    }

    var specDir = Path.GetDirectoryName(Path.GetFullPath(specPath))!;
    var result = Phase5CampaignSpecValidator.Validate(spec, specDir, requireSidecars);

    if (result.IsValid)
    {
        Console.WriteLine($"Campaign spec is valid: {specPath}");
        return 0;
    }

    Console.Error.WriteLine($"Campaign spec validation failed ({result.Errors.Count} error(s)):");
    foreach (var error in result.Errors)
        Console.Error.WriteLine($"  - {error}");
    return 1;
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
    Console.WriteLine("  gu solve-backgrounds <study.json> [--output <dir>] [--lie-algebra su2|su3] [--manifest <path>] [--manifest-dir <dir>]");
    Console.WriteLine("  gu compute-spectrum <run> <bgId> [opts]    Compute spectrum for a background");
    Console.WriteLine("  gu track-modes <runFolder> [--context ...]  Track modes across spectra");
    Console.WriteLine("  gu build-boson-registry <runFolder>         Build candidate boson registry");
    Console.WriteLine("  gu run-boson-campaign <runFolder> [opts]    Run boson comparison campaign");
    Console.WriteLine("  gu export-boson-report <runFolder> [opts]   Export boson atlas report");
    Console.WriteLine("  gu generate-phase4-report <runFolder> [opts] Generate Phase IV fermionic sector report");
    Console.WriteLine("  gu report-phase4 <runFolder> [opts]          Alias for generate-phase4-report");
    Console.WriteLine("  gu build-spin-spec <runFolder> [outSpec]     Build spinor representation spec");
    Console.WriteLine("  gu assemble-dirac <runFolder> [opts]         Assemble Dirac operator bundle");
    Console.WriteLine("  gu solve-fermion-modes <runFolder> [opts]    Solve fermionic spectral modes");
    Console.WriteLine("  gu analyze-chirality <runFolder> [opts]      Analyze chirality of fermion modes");
    Console.WriteLine("  gu analyze-conjugation <runFolder> [opts]    Analyze conjugation pairs of modes");
    Console.WriteLine("  gu extract-couplings <runFolder> [opts]      Extract boson-fermion coupling atlas");
    Console.WriteLine("  gu build-family-clusters <runFolder> [opts]  Build fermion family atlas and cluster report");
    Console.WriteLine("  gu build-unified-registry <runFolder> [opts] Build unified particle registry (Phase III+IV)");
    Console.WriteLine("  gu branch-robustness --study <f> --values <f> [--out <f>]  Phase V branch robustness study");
    Console.WriteLine("  gu refinement-study --spec <f> --values <f> [--out <f>]  Phase V refinement convergence study");
    Console.WriteLine("  gu import-environment --spec <f> [--out <f>] Import external geometry as environment record");
    Console.WriteLine("  gu build-structured-environment --spec <f> [--out <f>]  Generate structured analytic environment");
    Console.WriteLine("  gu validate-quantitative --observables <f> --targets <f> [--out <f>]  Quantitative validation");
    Console.WriteLine("  gu build-validation-dossier --study-manifest <f> [--out <f>]  Build Phase V validation dossier");
    Console.WriteLine("  gu verify-study-freshness --dossier <f>      Verify study freshness / G-006 compliance");
    Console.WriteLine("  gu run-phase5-campaign --spec <f> --out-dir <dir> [--validate-first]  Run Phase V M53 end-to-end campaign");
    Console.WriteLine("  gu export-phase5-bridge-values --atlas <f> --refinement-spec <f> --out-dir <dir>  Export bridged branch/refinement value tables");
    Console.WriteLine("  gu build-phase5-sidecars --registry <f> --observables <f> --environment-record <f>... --out-dir <dir>  Generate sidecar evidence files");
    Console.WriteLine("  gu validate-phase5-campaign-spec --spec <f> [--require-reference-sidecars]  Validate campaign spec paths and reference requirements");
    Console.WriteLine("  gu validate-replay <orig> <replay> [tier]    Validate replay (R0/R1/R2/R3)");
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
    Console.WriteLine("  --manifest path.json   Explicit branch manifest to load (overrides run-folder manifest)");
    Console.WriteLine("  --omega path.json      Initial omega tensor file (overrides persisted final state)");
    Console.WriteLine("  --a0 path.json         Background connection A0 tensor file (default: zero)");
    Console.WriteLine();
    Console.WriteLine("Phase III Spectral/Boson options:");
    Console.WriteLine("  --num-modes N          Number of modes to compute (default: 10)");
    Console.WriteLine("  --formulation p1|p2    Physical mode formulation (default: p1)");
    Console.WriteLine("  --context continuation|branch|refinement  Tracking context type");
    Console.WriteLine("  --campaign <file>      Campaign spec JSON file");
}
