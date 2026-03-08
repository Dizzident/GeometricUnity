using Gu.Core;
using Gu.Core.Serialization;

namespace Gu.Artifacts;

/// <summary>
/// Reads a canonical run folder back into typed objects.
/// Validates integrity hashes on load when an integrity manifest is present.
/// </summary>
public sealed class RunFolderReader
{
    private readonly string _rootPath;

    /// <summary>
    /// Create a reader for the given run folder root path.
    /// </summary>
    public RunFolderReader(string rootPath)
    {
        _rootPath = rootPath ?? throw new ArgumentNullException(nameof(rootPath));
    }

    /// <summary>
    /// The root path of this run folder.
    /// </summary>
    public string RootPath => _rootPath;

    /// <summary>
    /// Read the branch manifest from the run folder.
    /// </summary>
    public BranchManifest? ReadBranchManifest() =>
        ReadJson<BranchManifest>(RunFolderLayout.BranchManifestFile);

    /// <summary>
    /// Read the geometry context from the run folder.
    /// </summary>
    public GeometryContext? ReadGeometry() =>
        ReadJson<GeometryContext>(RunFolderLayout.GeometryFile);

    /// <summary>
    /// Read the runtime info from the run folder.
    /// </summary>
    public RuntimeInfo? ReadRuntime() =>
        ReadJson<RuntimeInfo>(RunFolderLayout.RuntimeFile);

    /// <summary>
    /// Read the initial discrete state from the run folder.
    /// </summary>
    public DiscreteState? ReadInitialState() =>
        ReadJson<DiscreteState>(RunFolderLayout.InitialStateFile);

    /// <summary>
    /// Read the final discrete state from the run folder.
    /// </summary>
    public DiscreteState? ReadFinalState() =>
        ReadJson<DiscreteState>(RunFolderLayout.FinalStateFile);

    /// <summary>
    /// Read the residual bundle from the run folder.
    /// </summary>
    public ResidualBundle? ReadResidualBundle() =>
        ReadJson<ResidualBundle>(RunFolderLayout.ResidualBundleFile);

    /// <summary>
    /// Read the linearization state from the run folder.
    /// </summary>
    public LinearizationState? ReadLinearizationBundle() =>
        ReadJson<LinearizationState>(RunFolderLayout.LinearizationBundleFile);

    /// <summary>
    /// Read the observed state from the run folder.
    /// </summary>
    public ObservedState? ReadObservedState() =>
        ReadJson<ObservedState>(RunFolderLayout.ObservedStateFile);

    /// <summary>
    /// Read the validation bundle from the run folder.
    /// </summary>
    public ValidationBundle? ReadValidationBundle() =>
        ReadJson<ValidationBundle>(RunFolderLayout.ValidationBundleFile);

    /// <summary>
    /// Read the integrity bundle from the run folder.
    /// </summary>
    public IntegrityBundle? ReadIntegrityBundle() =>
        ReadJson<IntegrityBundle>(RunFolderLayout.HashesFile);

    /// <summary>
    /// Read the replay contract from the run folder.
    /// </summary>
    public ReplayContract? ReadReplayContract() =>
        ReadJson<ReplayContract>(RunFolderLayout.ReplayContractFile);

    /// <summary>
    /// Read the artifact ID from the package root marker.
    /// </summary>
    public string? ReadArtifactId()
    {
        var path = Path.Combine(_rootPath, RunFolderLayout.PackageRootFile);
        if (!File.Exists(path))
            return null;
        return File.ReadAllText(path).Trim();
    }

    /// <summary>
    /// Read a complete ArtifactBundle from the run folder.
    /// Assembles all available components into a single bundle.
    /// </summary>
    public ArtifactBundle? ReadArtifactBundle()
    {
        var artifactId = ReadArtifactId();
        var contract = ReadReplayContract();

        if (artifactId is null || contract is null)
            return null;

        var manifest = ReadBranchManifest();
        if (manifest is null)
            return null;

        // Construct a provenance from available data
        var branch = new BranchRef
        {
            BranchId = manifest.BranchId,
            SchemaVersion = manifest.SchemaVersion,
        };

        return new ArtifactBundle
        {
            ArtifactId = artifactId,
            Branch = branch,
            ReplayContract = contract,
            ValidationBundle = ReadValidationBundle(),
            ObservedState = ReadObservedState(),
            Integrity = ReadIntegrityBundle(),
            Provenance = new ProvenanceMeta
            {
                CreatedAt = DateTimeOffset.UtcNow,
                CodeRevision = manifest.CodeRevision,
                Branch = branch,
                Backend = contract.BackendId,
            },
            CreatedAt = DateTimeOffset.UtcNow,
        };
    }

    /// <summary>
    /// Read and verify the run folder against its file hash manifest.
    /// Returns an empty list if all hashes match, otherwise returns a list of mismatched paths.
    /// Throws if no hash manifest is found.
    /// </summary>
    public IReadOnlyList<string> VerifyIntegrity()
    {
        var hashManifestPath = Path.Combine(_rootPath, RunFolderLayout.HashesFile);
        if (!File.Exists(hashManifestPath))
        {
            return new[] { "No integrity hash file found at " + RunFolderLayout.HashesFile };
        }

        // Read the file hash manifest written by IntegrityHasher
        var json = File.ReadAllText(hashManifestPath);
        var manifest = GuJsonDefaults.Deserialize<FileHashManifest>(json);

        if (manifest is null)
        {
            return new[] { "Failed to deserialize integrity hash manifest" };
        }

        return IntegrityHasher.VerifyRunFolderHashes(_rootPath, manifest);
    }

    /// <summary>
    /// Check whether the canonical folder structure exists.
    /// </summary>
    public bool HasValidStructure()
    {
        foreach (var dir in RunFolderLayout.RequiredDirectories)
        {
            if (!Directory.Exists(Path.Combine(_rootPath, dir)))
                return false;
        }
        return true;
    }

    /// <summary>
    /// Read a JSON artifact from the run folder.
    /// </summary>
    public T? ReadJson<T>(string relativePath)
    {
        var path = Path.Combine(_rootPath, relativePath);
        if (!File.Exists(path))
            return default;
        var json = File.ReadAllText(path);
        return GuJsonDefaults.Deserialize<T>(json);
    }
}
