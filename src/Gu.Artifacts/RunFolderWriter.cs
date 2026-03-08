using System.Text.Json;
using Gu.Core;
using Gu.Core.Serialization;

namespace Gu.Artifacts;

/// <summary>
/// Writes a canonical run folder (Section 20.2) containing all artifacts from a run.
/// </summary>
public sealed class RunFolderWriter
{
    private readonly string _rootPath;

    public RunFolderWriter(string rootPath)
    {
        _rootPath = rootPath ?? throw new ArgumentNullException(nameof(rootPath));
    }

    /// <summary>
    /// The root path of this run folder.
    /// </summary>
    public string RootPath => _rootPath;

    /// <summary>
    /// Create the canonical directory structure.
    /// </summary>
    public void CreateDirectories()
    {
        foreach (var dir in RunFolderLayout.RequiredDirectories)
        {
            Directory.CreateDirectory(Path.Combine(_rootPath, dir));
        }
    }

    /// <summary>
    /// Write the branch manifest to manifest/branch.json.
    /// </summary>
    public void WriteBranchManifest(BranchManifest manifest)
    {
        WriteJson(RunFolderLayout.BranchManifestFile, manifest);
    }

    /// <summary>
    /// Write the geometry context to manifest/geometry.json.
    /// </summary>
    public void WriteGeometry(GeometryContext geometry)
    {
        WriteJson(RunFolderLayout.GeometryFile, geometry);
    }

    /// <summary>
    /// Write runtime metadata to manifest/runtime.json.
    /// </summary>
    public void WriteRuntime(RuntimeInfo runtime)
    {
        WriteJson(RunFolderLayout.RuntimeFile, runtime);
    }

    /// <summary>
    /// Write the initial discrete state to state/initial_state.json.
    /// </summary>
    public void WriteInitialState(DiscreteState state)
    {
        WriteJson(RunFolderLayout.InitialStateFile, state);
    }

    /// <summary>
    /// Write the final discrete state to state/final_state.json.
    /// </summary>
    public void WriteFinalState(DiscreteState state)
    {
        WriteJson(RunFolderLayout.FinalStateFile, state);
    }

    /// <summary>
    /// Write the derived state to state/derived/derived_state.json.
    /// </summary>
    public void WriteDerivedState(DerivedState state)
    {
        WriteJson(RunFolderLayout.DerivedStateFile, state);
    }

    /// <summary>
    /// Write the residual bundle to residuals/residual_bundle.json.
    /// </summary>
    public void WriteResidualBundle(ResidualBundle bundle)
    {
        WriteJson(RunFolderLayout.ResidualBundleFile, bundle);
    }

    /// <summary>
    /// Write the linearization state to linearization/linearization_bundle.json.
    /// </summary>
    public void WriteLinearizationBundle(LinearizationState state)
    {
        WriteJson(RunFolderLayout.LinearizationBundleFile, state);
    }

    /// <summary>
    /// Write the observed state to observed/observed_state.json.
    /// </summary>
    public void WriteObservedState(ObservedState state)
    {
        WriteJson(RunFolderLayout.ObservedStateFile, state);
    }

    /// <summary>
    /// Write the validation bundle to validation/validation_bundle.json.
    /// </summary>
    public void WriteValidationBundle(ValidationBundle bundle)
    {
        WriteJson(RunFolderLayout.ValidationBundleFile, bundle);
    }

    /// <summary>
    /// Write individual validation records to validation/records/{ruleId}.json.
    /// </summary>
    public void WriteValidationRecords(IEnumerable<ValidationRecord> records)
    {
        foreach (var record in records)
        {
            var fileName = $"{record.RuleId}.json";
            WriteJson(Path.Combine(RunFolderLayout.ValidationRecordsDir, fileName), record);
        }
    }

    /// <summary>
    /// Write the integrity bundle to integrity/hashes.json.
    /// </summary>
    public void WriteIntegrityBundle(IntegrityBundle bundle)
    {
        WriteJson(RunFolderLayout.HashesFile, bundle);
    }

    /// <summary>
    /// Write the replay contract to replay/replay_contract.json.
    /// </summary>
    public void WriteReplayContract(ReplayContract contract)
    {
        WriteJson(RunFolderLayout.ReplayContractFile, contract);
    }

    /// <summary>
    /// Write the package root marker to integrity/package_root.txt.
    /// </summary>
    public void WritePackageRoot(string artifactId)
    {
        var path = Path.Combine(_rootPath, RunFolderLayout.PackageRootFile);
        File.WriteAllText(path, artifactId);
    }

    /// <summary>
    /// Write the environment info to logs/environment.txt.
    /// </summary>
    public void WriteEnvironmentLog(string environmentInfo)
    {
        var path = Path.Combine(_rootPath, RunFolderLayout.EnvironmentFile);
        File.WriteAllText(path, environmentInfo);
    }

    /// <summary>
    /// Write a complete ArtifactBundle, populating the canonical folder structure.
    /// </summary>
    public void WriteArtifactBundle(ArtifactBundle bundle)
    {
        CreateDirectories();

        WriteBranchManifest(bundle.ReplayContract.BranchManifest);
        WriteReplayContract(bundle.ReplayContract);
        WritePackageRoot(bundle.ArtifactId);

        if (bundle.Geometry is not null)
        {
            WriteGeometry(bundle.Geometry);
        }

        if (bundle.InitialState is not null)
        {
            WriteInitialState(bundle.InitialState);
        }

        if (bundle.FinalState is not null)
        {
            WriteFinalState(bundle.FinalState);
        }

        if (bundle.DerivedState is not null)
        {
            WriteDerivedState(bundle.DerivedState);
        }

        if (bundle.Residuals is not null)
        {
            WriteResidualBundle(bundle.Residuals);
        }

        if (bundle.Linearization is not null)
        {
            WriteLinearizationBundle(bundle.Linearization);
        }

        if (bundle.ValidationBundle is not null)
        {
            WriteValidationBundle(bundle.ValidationBundle);
            WriteValidationRecords(bundle.ValidationBundle.Records);
        }

        if (bundle.ObservedState is not null)
        {
            WriteObservedState(bundle.ObservedState);
        }

        if (bundle.Integrity is not null)
        {
            WriteIntegrityBundle(bundle.Integrity);
        }
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

    private void WriteJson<T>(string relativePath, T value)
    {
        var path = Path.Combine(_rootPath, relativePath);
        var dir = Path.GetDirectoryName(path);
        if (dir is not null)
            Directory.CreateDirectory(dir);
        var json = GuJsonDefaults.Serialize(value);
        File.WriteAllText(path, json);
    }
}
