namespace Gu.Interop;

/// <summary>
/// Error information from the native backend.
/// Returned by INativeBackend.GetLastError() for diagnostic purposes.
/// </summary>
public sealed class ErrorPacket
{
    /// <summary>Error code from the native side.</summary>
    public required int Code { get; init; }

    /// <summary>Human-readable error message.</summary>
    public required string Message { get; init; }

    /// <summary>Native function where the error occurred.</summary>
    public string? Source { get; init; }

    /// <summary>Additional diagnostic details.</summary>
    public string? Details { get; init; }
}
