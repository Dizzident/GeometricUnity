using System.Text.Json.Serialization;

namespace Gu.TheoryConformance.Tests;

/// <summary>
/// Machine-readable conformance summary artifact.
/// Records the result of checking that a runtime execution matches declared
/// branch-local assumptions. Passed artifacts do NOT imply theory-level validity —
/// they only assert branch-local consistency.
/// </summary>
public sealed class ConformanceArtifact
{
    /// <summary>Unique identifier for this conformance run.</summary>
    [JsonPropertyName("conformanceId")]
    public required string ConformanceId { get; init; }

    /// <summary>Branch ID under which conformance was checked.</summary>
    [JsonPropertyName("branchId")]
    public required string BranchId { get; init; }

    /// <summary>ISO 8601 timestamp of conformance evaluation.</summary>
    [JsonPropertyName("evaluatedAt")]
    public required DateTimeOffset EvaluatedAt { get; init; }

    /// <summary>Overall pass/fail. False if any check fails.</summary>
    [JsonPropertyName("overallPass")]
    public required bool OverallPass { get; init; }

    /// <summary>Individual conformance check results.</summary>
    [JsonPropertyName("checks")]
    public required List<ConformanceCheck> Checks { get; init; }

    /// <summary>
    /// Branch-local scope declaration. Always "branch-local" — never "theory-level".
    /// This field is mandatory to prevent callers from misinterpreting passing
    /// conformance as theory-level validation.
    /// </summary>
    [JsonPropertyName("validationScope")]
    public required string ValidationScope { get; init; }

    /// <summary>
    /// Human-readable summary of what was checked and what the results mean.
    /// Must include explicit disclaimer about branch-local vs. theory-level scope.
    /// </summary>
    [JsonPropertyName("scopeDisclaimer")]
    public required string ScopeDisclaimer { get; init; }
}

/// <summary>
/// Result of a single conformance check.
/// </summary>
public sealed class ConformanceCheck
{
    /// <summary>Unique check identifier (kebab-case).</summary>
    [JsonPropertyName("checkId")]
    public required string CheckId { get; init; }

    /// <summary>Category: "branch-identity", "trivial-state", "scope-boundary".</summary>
    [JsonPropertyName("category")]
    public required string Category { get; init; }

    /// <summary>Whether the check passed.</summary>
    [JsonPropertyName("passed")]
    public required bool Passed { get; init; }

    /// <summary>Human-readable detail about the check result.</summary>
    [JsonPropertyName("detail")]
    public required string Detail { get; init; }

    /// <summary>
    /// Validation type: "branch-local" or "theory-level".
    /// All checks in this harness must be "branch-local".
    /// </summary>
    [JsonPropertyName("validationType")]
    public required string ValidationType { get; init; }
}
