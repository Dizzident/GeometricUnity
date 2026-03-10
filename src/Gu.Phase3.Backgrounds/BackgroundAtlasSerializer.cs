using Gu.Core.Serialization;

namespace Gu.Phase3.Backgrounds;

/// <summary>
/// Serialization utilities for background atlas types.
/// </summary>
public static class BackgroundAtlasSerializer
{
    /// <summary>
    /// Serialize a BackgroundAtlas to JSON.
    /// </summary>
    public static string SerializeAtlas(BackgroundAtlas atlas)
    {
        ArgumentNullException.ThrowIfNull(atlas);
        return GuJsonDefaults.Serialize(atlas);
    }

    /// <summary>
    /// Deserialize a BackgroundAtlas from JSON.
    /// </summary>
    public static BackgroundAtlas? DeserializeAtlas(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        return GuJsonDefaults.Deserialize<BackgroundAtlas>(json);
    }

    /// <summary>
    /// Serialize a BackgroundStudySpec to JSON.
    /// </summary>
    public static string SerializeStudy(BackgroundStudySpec study)
    {
        ArgumentNullException.ThrowIfNull(study);
        return GuJsonDefaults.Serialize(study);
    }

    /// <summary>
    /// Deserialize a BackgroundStudySpec from JSON.
    /// </summary>
    public static BackgroundStudySpec? DeserializeStudy(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        return GuJsonDefaults.Deserialize<BackgroundStudySpec>(json);
    }

    /// <summary>
    /// Write atlas to a file path.
    /// </summary>
    public static void WriteAtlas(BackgroundAtlas atlas, string path)
    {
        ArgumentNullException.ThrowIfNull(atlas);
        ArgumentNullException.ThrowIfNull(path);

        var dir = Path.GetDirectoryName(path);
        if (dir is not null)
            Directory.CreateDirectory(dir);

        File.WriteAllText(path, SerializeAtlas(atlas));
    }

    /// <summary>
    /// Read atlas from a file path.
    /// </summary>
    public static BackgroundAtlas? ReadAtlas(string path)
    {
        ArgumentNullException.ThrowIfNull(path);
        if (!File.Exists(path))
            return null;
        return DeserializeAtlas(File.ReadAllText(path));
    }

    /// <summary>
    /// Write a background record to a file path.
    /// </summary>
    public static void WriteRecord(BackgroundRecord record, string path)
    {
        ArgumentNullException.ThrowIfNull(record);
        ArgumentNullException.ThrowIfNull(path);

        var dir = Path.GetDirectoryName(path);
        if (dir is not null)
            Directory.CreateDirectory(dir);

        File.WriteAllText(path, GuJsonDefaults.Serialize(record));
    }

    /// <summary>
    /// Write a study spec to a file path.
    /// </summary>
    public static void WriteStudy(BackgroundStudySpec study, string path)
    {
        ArgumentNullException.ThrowIfNull(study);
        ArgumentNullException.ThrowIfNull(path);

        var dir = Path.GetDirectoryName(path);
        if (dir is not null)
            Directory.CreateDirectory(dir);

        File.WriteAllText(path, SerializeStudy(study));
    }
}
