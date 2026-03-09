using System.Text.Json.Serialization;
using Gu.Core;

namespace Gu.Solvers;

/// <summary>
/// Per-branch result within a branch sweep, including full artifact bundle.
/// </summary>
public sealed class BranchSweepEntry
{
    /// <summary>Branch manifest used.</summary>
    [JsonPropertyName("manifest")]
    public required BranchManifest Manifest { get; init; }

    /// <summary>Whether this branch's solver converged.</summary>
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

    /// <summary>Artifact bundle for this branch run.</summary>
    [JsonPropertyName("artifactBundle")]
    public required ArtifactBundle ArtifactBundle { get; init; }
}

/// <summary>
/// Result of a branch sensitivity sweep across multiple branch manifests (GAP-9).
/// </summary>
public sealed class BranchSweepResult
{
    /// <summary>Per-branch entries with full results and artifact bundles.</summary>
    [JsonPropertyName("entries")]
    public required IReadOnlyList<BranchSweepEntry> Entries { get; init; }

    /// <summary>Inner solve mode used.</summary>
    [JsonPropertyName("innerMode")]
    public required SolveMode InnerMode { get; init; }

    /// <summary>Number of branches.</summary>
    [JsonIgnore]
    public int BranchCount => Entries.Count;

    /// <summary>Branch IDs that converged.</summary>
    [JsonIgnore]
    public IReadOnlyList<string> ConvergedBranches =>
        Entries.Where(e => e.Converged).Select(e => e.Manifest.BranchId).ToList();

    /// <summary>Branch IDs that did not converge.</summary>
    [JsonIgnore]
    public IReadOnlyList<string> DivergedBranches =>
        Entries.Where(e => !e.Converged).Select(e => e.Manifest.BranchId).ToList();
}
