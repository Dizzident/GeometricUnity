using Gu.Core;
using Gu.Core.Serialization;

namespace Gu.Phase3.Backgrounds;

/// <summary>
/// Resolves branch manifests for background study specs using the D-001 priority order:
/// 1. explicit manifest path (single-manifest studies only)
/// 2. manifest directory
/// 3. ManifestSearchPaths from the study spec (relative to studyDir)
/// 4. hard error
/// </summary>
public static class ManifestResolver
{
    /// <summary>
    /// Resolve a manifest for the given <paramref name="branchManifestId"/> using D-001 priority.
    /// </summary>
    /// <param name="branchManifestId">The manifest ID to resolve.</param>
    /// <param name="explicitManifestPath">
    /// Path from --manifest flag. Only valid when all specs share the same BranchManifestId.
    /// Pass null if not provided.
    /// </param>
    /// <param name="manifestDir">Path from --manifest-dir flag. Pass null if not provided.</param>
    /// <param name="manifestSearchPaths">
    /// Search paths from <see cref="BackgroundStudySpec.ManifestSearchPaths"/>.
    /// Each path is resolved relative to <paramref name="studyDir"/> if relative.
    /// Pass null or empty if not present.
    /// </param>
    /// <param name="studyDir">Absolute directory of the study JSON file. Used to resolve relative search paths.</param>
    /// <returns>
    /// A tuple of the resolved <see cref="BranchManifest"/> and the absolute path of the file consumed.
    /// </returns>
    /// <exception cref="ManifestResolutionException">
    /// Thrown when no manifest can be resolved (hard error per D-001).
    /// </exception>
    public static (BranchManifest Manifest, string ConsumedPath) Resolve(
        string branchManifestId,
        string? explicitManifestPath,
        string? manifestDir,
        IReadOnlyList<string>? manifestSearchPaths,
        string studyDir)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(branchManifestId);
        ArgumentException.ThrowIfNullOrWhiteSpace(studyDir);

        // Step 1: explicit --manifest path
        if (explicitManifestPath is not null)
        {
            if (!File.Exists(explicitManifestPath))
                throw new ManifestResolutionException(
                    $"--manifest file not found: {explicitManifestPath}");

            var manifest = LoadManifest(explicitManifestPath);
            return (manifest, Path.GetFullPath(explicitManifestPath));
        }

        // Step 2: --manifest-dir
        if (manifestDir is not null)
        {
            var branchJsonPath = Path.Combine(manifestDir, $"{branchManifestId}.branch.json");
            if (File.Exists(branchJsonPath))
            {
                var manifest = LoadManifest(branchJsonPath);
                return (manifest, Path.GetFullPath(branchJsonPath));
            }

            var jsonPath = Path.Combine(manifestDir, $"{branchManifestId}.json");
            if (File.Exists(jsonPath))
            {
                var manifest = LoadManifest(jsonPath);
                return (manifest, Path.GetFullPath(jsonPath));
            }
        }

        // Step 3: ManifestSearchPaths (relative to studyDir)
        if (manifestSearchPaths is not null)
        {
            foreach (var searchPath in manifestSearchPaths)
            {
                var absSearchDir = Path.IsPathRooted(searchPath)
                    ? searchPath
                    : Path.Combine(studyDir, searchPath);

                var branchJsonPath = Path.Combine(absSearchDir, $"{branchManifestId}.branch.json");
                if (File.Exists(branchJsonPath))
                {
                    var manifest = LoadManifest(branchJsonPath);
                    return (manifest, Path.GetFullPath(branchJsonPath));
                }

                var jsonPath = Path.Combine(absSearchDir, $"{branchManifestId}.json");
                if (File.Exists(jsonPath))
                {
                    var manifest = LoadManifest(jsonPath);
                    return (manifest, Path.GetFullPath(jsonPath));
                }
            }
        }

        // Step 4: hard error
        throw new ManifestResolutionException(
            $"Cannot resolve manifest for BranchManifestId '{branchManifestId}'. " +
            $"Provide --manifest, --manifest-dir, or set ManifestSearchPaths in the study spec.");
    }

    /// <summary>
    /// Validate that --manifest is only used when all specs share the same BranchManifestId.
    /// </summary>
    /// <returns>True if validation passes; false if mixed manifest IDs detected.</returns>
    public static bool ValidateExplicitManifestUsage(
        IReadOnlyList<BackgroundSpec> specs,
        out IReadOnlyList<string> distinctIds)
    {
        var ids = specs.Select(s => s.BranchManifestId).Distinct().ToList();
        distinctIds = ids;
        return ids.Count <= 1;
    }

    private static BranchManifest LoadManifest(string path)
    {
        var json = File.ReadAllText(path);
        var manifest = GuJsonDefaults.Deserialize<BranchManifest>(json);
        return manifest ?? throw new ManifestResolutionException(
            $"Failed to deserialize manifest at: {path}");
    }
}

/// <summary>
/// Exception thrown when manifest resolution fails (D-001 hard error).
/// </summary>
public sealed class ManifestResolutionException : Exception
{
    public ManifestResolutionException(string message) : base(message) { }
}
