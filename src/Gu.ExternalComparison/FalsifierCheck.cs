using System.Text.Json.Serialization;

namespace Gu.ExternalComparison;

/// <summary>
/// Defines a falsifier condition that can reject a branch.
/// Hard structural falsifiers are fatal and automatic; soft physics falsifiers are warnings.
/// </summary>
public sealed class FalsifierCheck
{
    /// <summary>Unique falsifier identifier (e.g., "F-HARD-01").</summary>
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    /// <summary>Category: "HardStructural", "SoftPhysics", or "NumericalHealth".</summary>
    [JsonPropertyName("category")]
    public required string Category { get; init; }

    /// <summary>Severity: "Fatal", "Warning", or "Informational".</summary>
    [JsonPropertyName("severity")]
    public required string Severity { get; init; }

    /// <summary>Human-readable description of what this falsifier checks.</summary>
    [JsonPropertyName("description")]
    public required string Description { get; init; }

    /// <summary>Tolerance policy for evaluating the condition.</summary>
    [JsonPropertyName("tolerance")]
    public required TolerancePolicy Tolerance { get; init; }

    /// <summary>Whether this falsifier's tolerance depends on the active branch configuration.</summary>
    [JsonPropertyName("branchDependence")]
    public bool BranchDependence { get; init; }
}
