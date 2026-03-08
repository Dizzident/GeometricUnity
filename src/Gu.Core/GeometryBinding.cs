using System.Text.Json.Serialization;

namespace Gu.Core;

/// <summary>
/// Describes a discrete geometric map (e.g., pi_h or sigma_h) between spaces.
/// </summary>
public sealed class GeometryBinding
{
    /// <summary>Binding type identifier (e.g., "projection", "observation").</summary>
    [JsonPropertyName("bindingType")]
    public required string BindingType { get; init; }

    /// <summary>Source space reference.</summary>
    [JsonPropertyName("sourceSpace")]
    public required SpaceRef SourceSpace { get; init; }

    /// <summary>Target space reference.</summary>
    [JsonPropertyName("targetSpace")]
    public required SpaceRef TargetSpace { get; init; }

    /// <summary>Optional description of the mapping strategy.</summary>
    [JsonPropertyName("mappingStrategy")]
    public string? MappingStrategy { get; init; }
}
