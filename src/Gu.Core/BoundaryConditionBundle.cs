using System.Text.Json.Serialization;

namespace Gu.Core;

/// <summary>
/// Bundle of boundary conditions for an environment.
/// </summary>
public sealed class BoundaryConditionBundle
{
    /// <summary>Boundary condition type (e.g., "Dirichlet", "Neumann", "periodic").</summary>
    [JsonPropertyName("conditionType")]
    public required string ConditionType { get; init; }

    /// <summary>Parameters for the boundary conditions.</summary>
    [JsonPropertyName("parameters")]
    public IReadOnlyDictionary<string, string>? Parameters { get; init; }
}
