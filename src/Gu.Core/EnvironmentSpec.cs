using System.Text.Json.Serialization;

namespace Gu.Core;

/// <summary>
/// First-class environment specification for simulation runs (Section 17.1).
/// </summary>
public sealed class EnvironmentSpec
{
    /// <summary>Unique environment identifier.</summary>
    [JsonPropertyName("environmentId")]
    public required string EnvironmentId { get; init; }

    /// <summary>Branch reference for this environment.</summary>
    [JsonPropertyName("branch")]
    public required BranchRef Branch { get; init; }

    /// <summary>Scenario type (e.g., "toy-consistency", "branch-sensitivity", "observation-pipeline").</summary>
    [JsonPropertyName("scenarioType")]
    public required string ScenarioType { get; init; }

    /// <summary>Geometry context for this environment.</summary>
    [JsonPropertyName("geometry")]
    public required GeometryContext Geometry { get; init; }

    /// <summary>Boundary conditions.</summary>
    [JsonPropertyName("boundaryConditions")]
    public required BoundaryConditionBundle BoundaryConditions { get; init; }

    /// <summary>Initial conditions.</summary>
    [JsonPropertyName("initialConditions")]
    public required InitialConditionBundle InitialConditions { get; init; }

    /// <summary>Gauge conditions.</summary>
    [JsonPropertyName("gaugeConditions")]
    public required GaugeConditionBundle GaugeConditions { get; init; }

    /// <summary>Requested observables to extract.</summary>
    [JsonPropertyName("observableRequests")]
    public required IReadOnlyList<ObservableRequest> ObservableRequests { get; init; }

    /// <summary>Comparison template identifiers.</summary>
    [JsonPropertyName("comparisonTemplateIds")]
    public required IReadOnlyList<string> ComparisonTemplateIds { get; init; }

    /// <summary>Provenance metadata.</summary>
    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}
