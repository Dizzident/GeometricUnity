using System.Text.Json.Serialization;

namespace Gu.Phase2.Recovery;

/// <summary>
/// Describes the typed projector/extractor used to produce an observed output
/// from an observation-descended field. Every extraction must declare its projector.
/// </summary>
public sealed class ExtractionProjectorRecord
{
    /// <summary>Unique projector identifier.</summary>
    [JsonPropertyName("projectorId")]
    public required string ProjectorId { get; init; }

    /// <summary>Human-readable name of the projector.</summary>
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    /// <summary>Description of what the projector does mathematically.</summary>
    [JsonPropertyName("description")]
    public required string Description { get; init; }

    /// <summary>Input type expected by this projector.</summary>
    [JsonPropertyName("inputType")]
    public required string InputType { get; init; }

    /// <summary>Output class produced by this projector.</summary>
    [JsonPropertyName("outputClass")]
    public required ObservedOutputKind OutputClass { get; init; }

    /// <summary>Whether this projector is branch-dependent.</summary>
    [JsonPropertyName("branchDependent")]
    public bool BranchDependent { get; init; }
}
