using Gu.Core.Serialization;

namespace Gu.Phase3.Spectra;

/// <summary>
/// Serialization utilities for operator bundle artifacts.
/// </summary>
public static class OperatorBundleSerializer
{
    public static string Serialize(OperatorBundleArtifact artifact)
    {
        ArgumentNullException.ThrowIfNull(artifact);
        return GuJsonDefaults.Serialize(artifact);
    }

    public static OperatorBundleArtifact? Deserialize(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        return GuJsonDefaults.Deserialize<OperatorBundleArtifact>(json);
    }

    public static string Serialize(LinearizedOperatorSpec spec)
    {
        ArgumentNullException.ThrowIfNull(spec);
        return GuJsonDefaults.Serialize(spec);
    }

    public static LinearizedOperatorSpec? DeserializeSpec(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        return GuJsonDefaults.Deserialize<LinearizedOperatorSpec>(json);
    }

    public static async Task WriteArtifactAsync(string path, OperatorBundleArtifact artifact)
    {
        ArgumentNullException.ThrowIfNull(path);
        var json = Serialize(artifact);
        await File.WriteAllTextAsync(path, json);
    }

    public static async Task<OperatorBundleArtifact?> ReadArtifactAsync(string path)
    {
        ArgumentNullException.ThrowIfNull(path);
        var json = await File.ReadAllTextAsync(path);
        return Deserialize(json);
    }
}
