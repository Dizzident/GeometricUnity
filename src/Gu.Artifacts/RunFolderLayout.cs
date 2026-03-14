namespace Gu.Artifacts;

/// <summary>
/// Canonical run folder layout paths (Section 20.2).
/// All paths are relative to the run root directory.
/// </summary>
public static class RunFolderLayout
{
    // Manifest directory
    public const string ManifestDir = "manifest";
    public const string BranchManifestFile = "manifest/branch.json";
    public const string GeometryFile = "manifest/geometry.json";
    public const string RuntimeFile = "manifest/runtime.json";

    // State directory
    public const string StateDir = "state";
    public const string InitialStateFile = "state/initial_state.json";
    public const string FinalStateFile = "state/final_state.json";
    public const string DerivedDir = "state/derived";
    public const string DerivedStateFile = "state/derived/derived_state.json";

    // Residuals directory
    public const string ResidualsDir = "residuals";
    public const string ResidualBundleFile = "residuals/residual_bundle.json";

    // Linearization directory
    public const string LinearizationDir = "linearization";
    public const string LinearizationBundleFile = "linearization/linearization_bundle.json";

    // Observed directory
    public const string ObservedDir = "observed";
    public const string ObservedStateFile = "observed/observed_state.json";

    // Validation directory
    public const string ValidationDir = "validation";
    public const string ValidationBundleFile = "validation/validation_bundle.json";
    public const string ValidationRecordsDir = "validation/records";

    // Integrity directory
    public const string IntegrityDir = "integrity";
    public const string HashesFile = "integrity/hashes.json";
    public const string FileHashManifestFile = "integrity/file_hashes.json";
    public const string PackageRootFile = "integrity/package_root.txt";

    // Replay directory
    public const string ReplayDir = "replay";
    public const string ReplayContractFile = "replay/replay_contract.json";

    // Logs directory
    public const string LogsDir = "logs";
    public const string SolverLogFile = "logs/solver.log";
    public const string EnvironmentFile = "logs/environment.txt";

    /// <summary>G-002: solve run classification record.</summary>
    public const string SolveRunClassificationFile = "logs/solve_run_classification.json";

    /// <summary>
    /// All directories that must be created for a canonical run folder.
    /// </summary>
    public static IReadOnlyList<string> RequiredDirectories { get; } = new[]
    {
        ManifestDir,
        StateDir,
        DerivedDir,
        ResidualsDir,
        LinearizationDir,
        ObservedDir,
        ValidationDir,
        ValidationRecordsDir,
        IntegrityDir,
        ReplayDir,
        LogsDir,
    };
}
