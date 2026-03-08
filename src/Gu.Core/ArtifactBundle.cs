using System.Text.Json.Serialization;

namespace Gu.Core;

/// <summary>
/// Complete artifact package for a run, including state, validation, and replay contract.
/// </summary>
public sealed class ArtifactBundle
{
    /// <summary>Unique artifact identifier.</summary>
    [JsonPropertyName("artifactId")]
    public required string ArtifactId { get; init; }

    /// <summary>Branch reference.</summary>
    [JsonPropertyName("branch")]
    public required BranchRef Branch { get; init; }

    /// <summary>Replay contract for reproducibility.</summary>
    [JsonPropertyName("replayContract")]
    public required ReplayContract ReplayContract { get; init; }

    /// <summary>Validation bundle.</summary>
    [JsonPropertyName("validationBundle")]
    public ValidationBundle? ValidationBundle { get; init; }

    /// <summary>Observed state, if observation pipeline was executed.</summary>
    [JsonPropertyName("observedState")]
    public ObservedState? ObservedState { get; init; }

    /// <summary>Integrity information.</summary>
    [JsonPropertyName("integrity")]
    public IntegrityBundle? Integrity { get; init; }

    /// <summary>Geometry context used for the run.</summary>
    [JsonPropertyName("geometry")]
    public GeometryContext? Geometry { get; init; }

    /// <summary>Initial discrete state.</summary>
    [JsonPropertyName("initialState")]
    public DiscreteState? InitialState { get; init; }

    /// <summary>Final discrete state after solving.</summary>
    [JsonPropertyName("finalState")]
    public DiscreteState? FinalState { get; init; }

    /// <summary>Derived state (curvature, torsion, Shiab, residual fields).</summary>
    [JsonPropertyName("derivedState")]
    public DerivedState? DerivedState { get; init; }

    /// <summary>Residual bundle.</summary>
    [JsonPropertyName("residuals")]
    public ResidualBundle? Residuals { get; init; }

    /// <summary>Linearization state, if computed.</summary>
    [JsonPropertyName("linearization")]
    public LinearizationState? Linearization { get; init; }

    /// <summary>Provenance metadata.</summary>
    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }

    /// <summary>Creation timestamp.</summary>
    [JsonPropertyName("createdAt")]
    public required DateTimeOffset CreatedAt { get; init; }
}
