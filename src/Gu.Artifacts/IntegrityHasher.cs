using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Gu.Core;
using Gu.Core.Serialization;

namespace Gu.Artifacts;

/// <summary>
/// Computes SHA-256 integrity hashes over serialized content.
/// Produces IntegrityBundle for artifact verification.
/// </summary>
public static class IntegrityHasher
{
    /// <summary>
    /// Compute SHA-256 hash of a UTF-8 string.
    /// </summary>
    public static string ComputeHash(string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexStringLower(hash);
    }

    /// <summary>
    /// Compute SHA-256 hash of a byte array.
    /// </summary>
    public static string ComputeHash(byte[] content)
    {
        var hash = SHA256.HashData(content);
        return Convert.ToHexStringLower(hash);
    }

    /// <summary>
    /// Compute SHA-256 hash of a serialized object using GU JSON defaults.
    /// </summary>
    public static string ComputeHashOfObject<T>(T value)
    {
        var json = GuJsonDefaults.Serialize(value);
        return ComputeHash(json);
    }

    /// <summary>
    /// Compute SHA-256 hash of a file.
    /// </summary>
    public static string ComputeFileHash(string filePath)
    {
        var bytes = File.ReadAllBytes(filePath);
        return ComputeHash(bytes);
    }

    /// <summary>
    /// Create an IntegrityBundle for the given content hash.
    /// </summary>
    public static IntegrityBundle CreateBundle(string contentHash) => new()
    {
        ContentHash = contentHash,
        HashAlgorithm = "SHA-256",
        ComputedAt = DateTimeOffset.UtcNow,
    };

    /// <summary>
    /// Compute an IntegrityBundle from a serialized object.
    /// </summary>
    public static IntegrityBundle ComputeBundle<T>(T value) =>
        CreateBundle(ComputeHashOfObject(value));

    /// <summary>
    /// Compute a manifest of file hashes for all JSON files in a run folder.
    /// Returns a dictionary mapping relative path to SHA-256 hash.
    /// </summary>
    public static FileHashManifest ComputeRunFolderHashes(string rootPath)
    {
        var hashes = new Dictionary<string, string>();
        var root = new DirectoryInfo(rootPath);

        if (!root.Exists)
            throw new DirectoryNotFoundException($"Run folder not found: {rootPath}");

        foreach (var file in root.EnumerateFiles("*", SearchOption.AllDirectories))
        {
            // Skip integrity files themselves to avoid circular hashing
            var relativePath = Path.GetRelativePath(rootPath, file.FullName);
            if (relativePath.StartsWith(RunFolderLayout.IntegrityDir, StringComparison.Ordinal))
                continue;

            hashes[relativePath] = ComputeFileHash(file.FullName);
        }

        return new FileHashManifest
        {
            RootPath = rootPath,
            FileHashes = hashes,
            ComputedAt = DateTimeOffset.UtcNow,
        };
    }

    /// <summary>
    /// Verify that all files in a run folder match their recorded hashes.
    /// Returns a list of mismatched file paths, empty if all match.
    /// </summary>
    public static IReadOnlyList<string> VerifyRunFolderHashes(string rootPath, FileHashManifest manifest)
    {
        var mismatches = new List<string>();
        var canonicalRoot = Path.GetFullPath(rootPath);

        foreach (var (relativePath, expectedHash) in manifest.FileHashes)
        {
            var fullPath = Path.GetFullPath(Path.Combine(rootPath, relativePath));

            // Guard against path traversal: resolved path must stay under rootPath
            if (!fullPath.StartsWith(canonicalRoot, StringComparison.Ordinal))
            {
                mismatches.Add($"Path traversal rejected: {relativePath}");
                continue;
            }

            if (!File.Exists(fullPath))
            {
                mismatches.Add($"Missing file: {relativePath}");
                continue;
            }

            var actualHash = ComputeFileHash(fullPath);
            if (!string.Equals(actualHash, expectedHash, StringComparison.OrdinalIgnoreCase))
            {
                mismatches.Add($"Hash mismatch: {relativePath} (expected {expectedHash}, got {actualHash})");
            }
        }

        return mismatches;
    }
}
