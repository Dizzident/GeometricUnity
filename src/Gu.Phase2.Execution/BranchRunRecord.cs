using System.Text.Json.Serialization;
using Gu.Core;
using Gu.Phase2.Semantics;
using Gu.Solvers;

namespace Gu.Phase2.Execution;

/// <summary>
/// Complete record of a single branch variant run within a Phase II branch sweep.
/// Contains solver result, derived state, observed outputs, and artifact bundle.
/// </summary>
public sealed class BranchRunRecord
{
    /// <summary>Branch variant manifest used for this run.</summary>
    [JsonPropertyName("variant")]
    public required BranchVariantManifest Variant { get; init; }

    /// <summary>Phase I manifest generated from the variant.</summary>
    [JsonPropertyName("manifest")]
    public required BranchManifest Manifest { get; init; }

    /// <summary>Whether the solver converged.</summary>
    [JsonPropertyName("converged")]
    public required bool Converged { get; init; }

    /// <summary>Termination reason.</summary>
    [JsonPropertyName("terminationReason")]
    public required string TerminationReason { get; init; }

    /// <summary>Final objective value.</summary>
    [JsonPropertyName("finalObjective")]
    public required double FinalObjective { get; init; }

    /// <summary>Final residual norm.</summary>
    [JsonPropertyName("finalResidualNorm")]
    public required double FinalResidualNorm { get; init; }

    /// <summary>Iteration count.</summary>
    [JsonPropertyName("iterations")]
    public required int Iterations { get; init; }

    /// <summary>Solve mode used.</summary>
    [JsonPropertyName("solveMode")]
    public required SolveMode SolveMode { get; init; }

    /// <summary>Observed state extracted via observation pipeline, if executed.</summary>
    [JsonPropertyName("observedState")]
    public ObservedState? ObservedState { get; init; }

    /// <summary>Artifact bundle for replay.</summary>
    [JsonPropertyName("artifactBundle")]
    public required ArtifactBundle ArtifactBundle { get; init; }
}
