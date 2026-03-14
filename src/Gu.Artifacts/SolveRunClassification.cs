using System.Text.Json.Serialization;

namespace Gu.Artifacts;

/// <summary>
/// Classification of a solve run for G-002: distinguishes residual-only inspection runs
/// from genuine objective/stationarity solves, and records seed provenance.
/// Written to logs/solve_run_classification.json by every gu run/solve invocation.
/// </summary>
public sealed class SolveRunClassification
{
    /// <summary>
    /// High-level run type. One of:
    ///   "residual-inspection" – Mode A, no genuine minimisation performed.
    ///   "objective-solve"     – Mode B, genuine objective minimisation.
    ///   "stationarity-solve"  – Mode C, stationarity/Newton solve.
    ///   "branch-sensitivity"  – Mode D, branch sweep.
    /// </summary>
    [JsonPropertyName("runType")]
    public required string RunType { get; init; }

    /// <summary>
    /// Source of the initial omega seed. One of:
    ///   "zero-seed"       – fresh zero initialisation (trivial validation path).
    ///   "persisted-state" – loaded from an existing final_state.json in the run folder.
    ///   "explicit-omega"  – supplied via --omega flag.
    ///   "explicit-a0"     – A0 supplied via --a0 flag (omega may still be zero).
    /// </summary>
    [JsonPropertyName("seedSource")]
    public required string SeedSource { get; init; }

    /// <summary>
    /// True when the run is considered a trivial validation path (residual-only + zero seed).
    /// Phase V dossiers must not treat trivial validation paths as genuine evidence.
    /// </summary>
    [JsonPropertyName("isTrivialValidationPath")]
    public required bool IsTrivialValidationPath { get; init; }

    /// <summary>
    /// Human-readable note describing what this run provides as validation evidence.
    /// </summary>
    [JsonPropertyName("validationNote")]
    public string? ValidationNote { get; init; }

    /// <summary>
    /// Solver mode string as passed on the CLI (e.g. "A", "B", "C", "D").
    /// </summary>
    [JsonPropertyName("solverMode")]
    public required string SolverMode { get; init; }

    /// <summary>
    /// Timestamp when the classification was recorded.
    /// </summary>
    [JsonPropertyName("classifiedAt")]
    public required DateTimeOffset ClassifiedAt { get; init; }

    /// <summary>
    /// Classify a run given solver mode and seed information.
    /// </summary>
    public static SolveRunClassification Classify(
        string modeFlag,
        bool hasPersistedOmega,
        bool hasExplicitOmega,
        bool hasExplicitA0)
    {
        var runType = modeFlag.ToUpperInvariant() switch
        {
            "B" => "objective-solve",
            "C" => "stationarity-solve",
            "D" => "branch-sensitivity",
            _   => "residual-inspection",
        };

        string seedSource;
        if (hasExplicitOmega)
            seedSource = "explicit-omega";
        else if (hasPersistedOmega)
            seedSource = "persisted-state";
        else if (hasExplicitA0)
            seedSource = "explicit-a0";
        else
            seedSource = "zero-seed";

        var isTrivial = runType == "residual-inspection" && seedSource == "zero-seed";

        var note = isTrivial
            ? "Residual-only inspection on zero seed. Not a genuine objective/stationarity solve. " +
              "Phase V dossiers must not cite this as evidence of meaningful background validation. " +
              "Recommended: supply --mode B or --mode C with a persisted seed for nontrivial validation."
            : runType == "residual-inspection"
                ? "Residual-only inspection on a non-zero seed. " +
                  "Reports the residual of the loaded state but performs no optimisation. " +
                  "Consider --mode B or --mode C for a genuine solve."
                : $"{runType} on {seedSource}.";

        return new SolveRunClassification
        {
            RunType = runType,
            SeedSource = seedSource,
            IsTrivialValidationPath = isTrivial,
            ValidationNote = note,
            SolverMode = modeFlag,
            ClassifiedAt = DateTimeOffset.UtcNow,
        };
    }
}
