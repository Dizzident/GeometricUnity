namespace Gu.Interop;

/// <summary>
/// Version information for the native backend.
/// Used to verify C#/native compatibility at initialization.
/// </summary>
public sealed class InteropVersion
{
    /// <summary>Major version (breaking changes).</summary>
    public required int Major { get; init; }

    /// <summary>Minor version (additive changes).</summary>
    public required int Minor { get; init; }

    /// <summary>Patch version (bug fixes).</summary>
    public required int Patch { get; init; }

    /// <summary>Backend identifier (e.g., "cuda", "cpu-reference").</summary>
    public required string BackendId { get; init; }

    /// <summary>Current interop version for the C# side.</summary>
    public static InteropVersion Current { get; } = new()
    {
        Major = 1,
        Minor = 0,
        Patch = 0,
        BackendId = "gu-interop-csharp",
    };

    /// <summary>
    /// Check compatibility with a native backend version.
    /// Major versions must match; minor can differ.
    /// </summary>
    public bool IsCompatibleWith(InteropVersion other)
        => Major == other.Major;

    public override string ToString()
        => $"{BackendId} v{Major}.{Minor}.{Patch}";
}
