using System.Text.Json.Serialization;

namespace Gu.Phase3.Reporting;

/// <summary>
/// Summary of the boson registry state for a report.
/// </summary>
public sealed class RegistrySummary
{
    /// <summary>Total number of candidates in the registry.</summary>
    [JsonPropertyName("totalCandidates")]
    public required int TotalCandidates { get; init; }

    /// <summary>Claim class distribution: count per claim class.</summary>
    [JsonPropertyName("claimClassDistribution")]
    public required IReadOnlyDictionary<string, int> ClaimClassDistribution { get; init; }

    /// <summary>Registry version string.</summary>
    [JsonPropertyName("registryVersion")]
    public required string RegistryVersion { get; init; }
}
