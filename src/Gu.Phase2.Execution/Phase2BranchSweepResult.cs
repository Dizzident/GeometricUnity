using System.Text.Json.Serialization;
using Gu.Phase2.Semantics;
using Gu.Solvers;

namespace Gu.Phase2.Execution;

/// <summary>
/// Result of a Phase II branch sweep across a branch family.
/// Contains per-branch run records and sweep metadata.
/// One environment can be run across a branch family, observed outputs produced
/// for each branch, and per-branch artifacts are replayable.
/// </summary>
public sealed class Phase2BranchSweepResult
{
    /// <summary>Family manifest that was swept.</summary>
    [JsonPropertyName("family")]
    public required BranchFamilyManifest Family { get; init; }

    /// <summary>Environment identifier.</summary>
    [JsonPropertyName("environmentId")]
    public required string EnvironmentId { get; init; }

    /// <summary>Per-branch run records.</summary>
    [JsonPropertyName("runRecords")]
    public required IReadOnlyList<BranchRunRecord> RunRecords { get; init; }

    /// <summary>Inner solve mode used for all branches.</summary>
    [JsonPropertyName("innerMode")]
    public required SolveMode InnerMode { get; init; }

    /// <summary>Timestamp when the sweep started.</summary>
    [JsonPropertyName("sweepStarted")]
    public required DateTimeOffset SweepStarted { get; init; }

    /// <summary>Timestamp when the sweep completed.</summary>
    [JsonPropertyName("sweepCompleted")]
    public required DateTimeOffset SweepCompleted { get; init; }

    /// <summary>Number of branches swept.</summary>
    [JsonIgnore]
    public int BranchCount => RunRecords.Count;

    /// <summary>Variant IDs that converged.</summary>
    [JsonIgnore]
    public IReadOnlyList<string> ConvergedVariants =>
        RunRecords.Where(r => r.Converged).Select(r => r.Variant.Id).ToList();

    /// <summary>Variant IDs that did not converge.</summary>
    [JsonIgnore]
    public IReadOnlyList<string> DivergedVariants =>
        RunRecords.Where(r => !r.Converged).Select(r => r.Variant.Id).ToList();

    /// <summary>Whether all branches produced observed outputs.</summary>
    [JsonIgnore]
    public bool AllBranchesHaveObservedOutputs =>
        RunRecords.All(r => r.ObservedState != null);
}
