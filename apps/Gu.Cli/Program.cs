using Gu.Artifacts;
using Gu.Core.Factories;
using Gu.Core.Serialization;

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

static void PrintUsage()
{
    Console.WriteLine("Geometric Unity CLI");
    Console.WriteLine();
    Console.WriteLine("Usage:");
    Console.WriteLine("  gu create-branch [branchId] [outputPath]");
    Console.WriteLine("  gu create-environment [environmentId] [branchId] [outputPath]");
    Console.WriteLine("  gu init-run <run-folder> [branchId]         Initialize a canonical run folder");
    Console.WriteLine("  gu validate-replay <orig> <replay> [tier]   Validate replay (R0/R1/R2/R3)");
    Console.WriteLine("  gu verify-integrity <run-folder>            Verify or compute integrity hashes");
}
