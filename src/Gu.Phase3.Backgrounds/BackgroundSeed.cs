using System.Text.Json.Serialization;
using Gu.Core;

namespace Gu.Phase3.Backgrounds;

/// <summary>
/// Describes the seed (initial guess) for a background solve.
/// </summary>
public sealed class BackgroundSeed
{
    /// <summary>Kind of seed.</summary>
    [JsonPropertyName("kind")]
    public required BackgroundSeedKind Kind { get; init; }

    /// <summary>
    /// Explicit initial state, if Kind is Explicit or SymmetricAnsatz.
    /// Null for Trivial (zero-initialized).
    /// </summary>
    [JsonPropertyName("initialState")]
    public FieldTensor? InitialState { get; init; }

    /// <summary>
    /// Reference background ID for Continuation seeds.
    /// </summary>
    [JsonPropertyName("continuationSourceId")]
    public string? ContinuationSourceId { get; init; }

    /// <summary>Human-readable label for this seed.</summary>
    [JsonPropertyName("label")]
    public string? Label { get; init; }
}
