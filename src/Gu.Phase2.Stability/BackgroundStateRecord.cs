using System.Text.Json.Serialization;
using Gu.Core;

namespace Gu.Phase2.Stability;

/// <summary>
/// Records a background state z_* around which linearization is performed.
/// Includes the solved connection, derived quantities, and solver diagnostics.
/// </summary>
public sealed class BackgroundStateRecord
{
    /// <summary>Unique identifier for this background state.</summary>
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    /// <summary>Branch manifest ID that produced this state.</summary>
    [JsonPropertyName("branchManifestId")]
    public required string BranchManifestId { get; init; }

    /// <summary>Branch variant ID (Phase II) if applicable.</summary>
    [JsonPropertyName("branchVariantId")]
    public string? BranchVariantId { get; init; }

    /// <summary>The background connection omega_*.</summary>
    [JsonPropertyName("omega")]
    public required FieldTensor Omega { get; init; }

    /// <summary>The distinguished connection A0.</summary>
    [JsonPropertyName("a0")]
    public required FieldTensor A0 { get; init; }

    /// <summary>Derived state at the background (F, T, S, Upsilon).</summary>
    [JsonPropertyName("derivedState")]
    public required DerivedState DerivedState { get; init; }

    /// <summary>Residual norm at background: ||Upsilon_*||.</summary>
    [JsonPropertyName("residualNorm")]
    public required double ResidualNorm { get; init; }

    /// <summary>Objective value at background: I2(omega_*).</summary>
    [JsonPropertyName("objectiveValue")]
    public required double ObjectiveValue { get; init; }

    /// <summary>Whether the background was obtained from a converged solver run.</summary>
    [JsonPropertyName("solverConverged")]
    public required bool SolverConverged { get; init; }

    /// <summary>Solver termination reason.</summary>
    [JsonPropertyName("solverTerminationReason")]
    public string? SolverTerminationReason { get; init; }

    /// <summary>Environment identifier (mesh/geometry description).</summary>
    [JsonPropertyName("environmentId")]
    public string? EnvironmentId { get; init; }

    /// <summary>Optional parameter label for parameterized background families z_*(lambda).</summary>
    [JsonPropertyName("parameterLabel")]
    public string? ParameterLabel { get; init; }

    /// <summary>Optional parameter value for parameterized background families.</summary>
    [JsonPropertyName("parameterValue")]
    public double? ParameterValue { get; init; }
}
