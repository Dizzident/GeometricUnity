using System.Text.Json.Serialization;

namespace Gu.Phase5.Reporting;

/// <summary>
/// Checked-in contract connecting a computed observable to a possible physical boson quantity.
/// A mapping is usable for physical target comparison only when Status is "validated".
/// </summary>
public sealed class PhysicalObservableMapping
{
    [JsonPropertyName("mappingId")]
    public required string MappingId { get; init; }

    [JsonPropertyName("particleId")]
    public required string ParticleId { get; init; }

    [JsonPropertyName("physicalObservableType")]
    public required string PhysicalObservableType { get; init; }

    [JsonPropertyName("sourceComputedObservableId")]
    public required string SourceComputedObservableId { get; init; }

    [JsonPropertyName("targetPhysicalObservableId")]
    public string? TargetPhysicalObservableId { get; init; }

    [JsonPropertyName("requiredEnvironmentId")]
    public string? RequiredEnvironmentId { get; init; }

    [JsonPropertyName("requiredEnvironmentTier")]
    public string? RequiredEnvironmentTier { get; init; }

    [JsonPropertyName("requiredBranchId")]
    public string? RequiredBranchId { get; init; }

    [JsonPropertyName("unitFamily")]
    public required string UnitFamily { get; init; }

    [JsonPropertyName("status")]
    public required string Status { get; init; }

    [JsonPropertyName("assumptions")]
    public required IReadOnlyList<string> Assumptions { get; init; }

    [JsonPropertyName("closureRequirements")]
    public required IReadOnlyList<string> ClosureRequirements { get; init; }
}

public sealed class PhysicalObservableMappingTable
{
    [JsonPropertyName("tableId")]
    public required string TableId { get; init; }

    [JsonPropertyName("mappings")]
    public required IReadOnlyList<PhysicalObservableMapping> Mappings { get; init; }
}
