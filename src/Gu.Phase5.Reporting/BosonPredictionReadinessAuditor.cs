using System.Text.Json.Serialization;

namespace Gu.Phase5.Reporting;

public sealed class BosonPredictionReadinessInput
{
    [JsonPropertyName("candidateId")]
    public required string CandidateId { get; init; }

    [JsonPropertyName("selectedBosonModeId")]
    public required string SelectedBosonModeId { get; init; }

    [JsonPropertyName("replayTerminalStatus")]
    public required string ReplayTerminalStatus { get; init; }

    [JsonPropertyName("replayClosureRequirements")]
    public required IReadOnlyList<string> ReplayClosureRequirements { get; init; }

    [JsonPropertyName("localGateBlockers")]
    public required IReadOnlyList<string> LocalGateBlockers { get; init; }

    [JsonPropertyName("couplingMagnitude")]
    public double? CouplingMagnitude { get; init; }

    [JsonPropertyName("fullConnectionLiftMaterialized")]
    public required bool FullConnectionLiftMaterialized { get; init; }

    [JsonPropertyName("fullConnectionModeVectorLength")]
    public int FullConnectionModeVectorLength { get; init; }

    [JsonPropertyName("branchStabilityScore")]
    public double? BranchStabilityScore { get; init; }

    [JsonPropertyName("refinementStabilityScore")]
    public double? RefinementStabilityScore { get; init; }

    [JsonPropertyName("claimClass")]
    public required string ClaimClass { get; init; }

    [JsonPropertyName("hasCandidateSpecificPhysicalMapping")]
    public bool HasCandidateSpecificPhysicalMapping { get; init; }

    [JsonPropertyName("hasCandidateSpecificCalibration")]
    public bool HasCandidateSpecificCalibration { get; init; }

    [JsonPropertyName("hasPhysicalTargets")]
    public bool HasPhysicalTargets { get; init; }

    [JsonPropertyName("priorAbsoluteComparisonPassed")]
    public bool? PriorAbsoluteComparisonPassed { get; init; }

    [JsonPropertyName("targetRelevantSevereFalsifierCount")]
    public int? TargetRelevantSevereFalsifierCount { get; init; }

    [JsonPropertyName("globalSidecarSevereFalsifierCount")]
    public int? GlobalSidecarSevereFalsifierCount { get; init; }

    [JsonPropertyName("hasTargetScopedFalsifierPolicy")]
    public bool HasTargetScopedFalsifierPolicy { get; init; }

    [JsonPropertyName("residualLimitations")]
    public IReadOnlyList<string> ResidualLimitations { get; init; } = [];
}

public sealed class BosonPredictionValidationGate
{
    [JsonPropertyName("gateId")]
    public required string GateId { get; init; }

    [JsonPropertyName("passed")]
    public required bool Passed { get; init; }

    [JsonPropertyName("diagnosis")]
    public required string Diagnosis { get; init; }
}

public sealed class BosonPredictionReadinessResult
{
    [JsonPropertyName("algorithmId")]
    public required string AlgorithmId { get; init; }

    [JsonPropertyName("terminalStatus")]
    public required string TerminalStatus { get; init; }

    [JsonPropertyName("candidateId")]
    public required string CandidateId { get; init; }

    [JsonPropertyName("selectedBosonModeId")]
    public required string SelectedBosonModeId { get; init; }

    [JsonPropertyName("predictionLevel")]
    public required string PredictionLevel { get; init; }

    [JsonPropertyName("sourceBackedReplayReady")]
    public required bool SourceBackedReplayReady { get; init; }

    [JsonPropertyName("internalBosonReplayPredictionReady")]
    public required bool InternalBosonReplayPredictionReady { get; init; }

    [JsonPropertyName("externalPhysicalComparisonReady")]
    public required bool ExternalPhysicalComparisonReady { get; init; }

    [JsonPropertyName("couplingMagnitude")]
    public double? CouplingMagnitude { get; init; }

    [JsonPropertyName("validationGates")]
    public required IReadOnlyList<BosonPredictionValidationGate> ValidationGates { get; init; }

    [JsonPropertyName("blockers")]
    public required IReadOnlyList<string> Blockers { get; init; }

    [JsonPropertyName("requiredFixes")]
    public required IReadOnlyList<string> RequiredFixes { get; init; }

    [JsonPropertyName("residualLimitations")]
    public required IReadOnlyList<string> ResidualLimitations { get; init; }
}

public static class BosonPredictionReadinessAuditor
{
    public const string AlgorithmId = "phase100-boson-prediction-readiness-auditor-v1";

    public static BosonPredictionReadinessResult Evaluate(BosonPredictionReadinessInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var gates = new List<BosonPredictionValidationGate>();
        var blockers = new List<string>();
        var fixes = new List<string>();

        bool sourceBackedReplayReady =
            string.Equals(input.ReplayTerminalStatus, "source-backed-analytic-replay-package-built", StringComparison.Ordinal) &&
            input.ReplayClosureRequirements.Count == 0 &&
            input.LocalGateBlockers.Count == 0 &&
            input.CouplingMagnitude is { } coupling &&
            double.IsFinite(coupling);

        AddGate(
            gates,
            blockers,
            fixes,
            "replay-integrity",
            sourceBackedReplayReady,
            sourceBackedReplayReady
                ? "source-backed replay built with finite coupling and no local closure blockers"
                : "source-backed replay is missing, non-finite, or still has local closure blockers",
            "repair source-backed replay inputs, vector materialization, or local fermion-mode gate blockers");

        bool fullLiftReady = input.FullConnectionLiftMaterialized && input.FullConnectionModeVectorLength > 0;
        AddGate(
            gates,
            blockers,
            fixes,
            "full-connection-lift",
            fullLiftReady,
            fullLiftReady
                ? $"full connection lift materialized with vector length {input.FullConnectionModeVectorLength}"
                : "selected boson mode does not have a materialized full connection-space vector",
            "materialize a replay-compatible full connection-space boson vector");

        bool branchReady = input.BranchStabilityScore is { } branch && double.IsFinite(branch) && branch >= 0.5;
        AddGate(
            gates,
            blockers,
            fixes,
            "target-blind-branch-stability",
            branchReady,
            branchReady
                ? $"target-blind branch stability score {input.BranchStabilityScore:R} passes"
                : "target-blind branch stability score is missing or below threshold",
            "rerun branch-stability scan and promote evidence-backed branch scores");

        bool refinementReady = input.RefinementStabilityScore is { } refinement && double.IsFinite(refinement) && refinement >= 0.5;
        AddGate(
            gates,
            blockers,
            fixes,
            "target-blind-refinement-stability",
            refinementReady,
            refinementReady
                ? $"target-blind refinement stability score {input.RefinementStabilityScore:R} passes"
                : "target-blind refinement stability score is missing or below threshold",
            "rerun refinement scan and promote evidence-backed refinement scores");

        bool internalReady = sourceBackedReplayReady && fullLiftReady && branchReady && refinementReady;

        bool claimPromoted = !string.Equals(input.ClaimClass, "C0_NumericalMode", StringComparison.Ordinal);
        AddGate(
            gates,
            blockers,
            fixes,
            "claim-class-promotion",
            claimPromoted,
            claimPromoted
                ? $"claim class {input.ClaimClass} permits physical-language review"
                : "candidate remains capped at C0_NumericalMode",
            "promote the candidate through replay, branch, refinement, and observation evidence or keep output internal-only");

        AddGate(
            gates,
            blockers,
            fixes,
            "candidate-specific-physical-mapping",
            input.HasCandidateSpecificPhysicalMapping,
            input.HasCandidateSpecificPhysicalMapping
                ? "candidate-specific physical observable mapping is present"
                : "candidate-specific physical observable mapping is missing or blocked",
            "derive or declare a candidate-specific mapping before external comparison");

        AddGate(
            gates,
            blockers,
            fixes,
            "candidate-specific-calibration",
            input.HasCandidateSpecificCalibration,
            input.HasCandidateSpecificCalibration
                ? "candidate-specific calibration is present"
                : "candidate-specific calibration is missing or blocked",
            "build a candidate-specific unit or scale calibration");

        AddGate(
            gates,
            blockers,
            fixes,
            "physical-target-table",
            input.HasPhysicalTargets,
            input.HasPhysicalTargets
                ? "physical target table is present"
                : "physical target table is missing",
            "attach authoritative physical target values and uncertainties");

        bool comparisonReady = input.PriorAbsoluteComparisonPassed is true;
        AddGate(
            gates,
            blockers,
            fixes,
            "absolute-comparison-status",
            comparisonReady,
            comparisonReady
                ? "prior absolute comparison passed"
                : "prior absolute comparison is missing or failed",
            "rerun calibrated target comparison; if it misses coherently, diagnose weak-coupling normalization or scalar-sector relation");

        bool falsifierReady =
            input.TargetRelevantSevereFalsifierCount is 0 &&
            (input.GlobalSidecarSevereFalsifierCount.GetValueOrDefault() == 0 || input.HasTargetScopedFalsifierPolicy);
        AddGate(
            gates,
            blockers,
            fixes,
            "falsifier-policy",
            falsifierReady,
            falsifierReady
                ? "no target-relevant severe falsifiers block the claim under the supplied policy"
                : "target-relevant or unresolved global severe falsifiers still block unrestricted physical language",
            "resolve severe falsifiers or adopt an explicit target-scoped physical-claim policy");

        bool externalReady =
            internalReady &&
            claimPromoted &&
            input.HasCandidateSpecificPhysicalMapping &&
            input.HasCandidateSpecificCalibration &&
            input.HasPhysicalTargets &&
            comparisonReady &&
            falsifierReady;

        string level = externalReady
            ? "external-physical-boson-prediction"
            : internalReady
                ? "internal-boson-replay-prediction"
                : "blocked";

        return new BosonPredictionReadinessResult
        {
            AlgorithmId = AlgorithmId,
            TerminalStatus = externalReady
                ? "external-physical-boson-prediction-ready"
                : internalReady
                    ? "internal-boson-replay-prediction-ready-physical-comparison-blocked"
                    : "boson-prediction-blocked",
            CandidateId = input.CandidateId,
            SelectedBosonModeId = input.SelectedBosonModeId,
            PredictionLevel = level,
            SourceBackedReplayReady = sourceBackedReplayReady,
            InternalBosonReplayPredictionReady = internalReady,
            ExternalPhysicalComparisonReady = externalReady,
            CouplingMagnitude = input.CouplingMagnitude,
            ValidationGates = gates,
            Blockers = blockers.Distinct(StringComparer.Ordinal).ToList(),
            RequiredFixes = fixes.Distinct(StringComparer.Ordinal).ToList(),
            ResidualLimitations = input.ResidualLimitations,
        };
    }

    private static void AddGate(
        List<BosonPredictionValidationGate> gates,
        List<string> blockers,
        List<string> fixes,
        string gateId,
        bool passed,
        string diagnosis,
        string requiredFix)
    {
        gates.Add(new BosonPredictionValidationGate
        {
            GateId = gateId,
            Passed = passed,
            Diagnosis = diagnosis,
        });

        if (!passed)
        {
            blockers.Add(diagnosis);
            fixes.Add(requiredFix);
        }
    }
}
