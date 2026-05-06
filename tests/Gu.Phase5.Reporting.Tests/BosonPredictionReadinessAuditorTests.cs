using Gu.Phase5.Reporting;

namespace Gu.Phase5.Reporting.Tests;

public sealed class BosonPredictionReadinessAuditorTests
{
    [Fact]
    public void Evaluate_ReturnsInternalPredictionWhenReplayAndStabilityPassButPhysicalGatesFail()
    {
        var result = BosonPredictionReadinessAuditor.Evaluate(MakeInput());

        Assert.Equal("internal-boson-replay-prediction-ready-physical-comparison-blocked", result.TerminalStatus);
        Assert.True(result.SourceBackedReplayReady);
        Assert.True(result.InternalBosonReplayPredictionReady);
        Assert.False(result.ExternalPhysicalComparisonReady);
        Assert.Equal("internal-boson-replay-prediction", result.PredictionLevel);
        Assert.Contains(result.ValidationGates, g => g.GateId == "claim-class-promotion" && !g.Passed);
        Assert.Contains(result.ValidationGates, g => g.GateId == "candidate-specific-physical-mapping" && !g.Passed);
        Assert.Contains(result.RequiredFixes, f => f.Contains("candidate-specific mapping", StringComparison.Ordinal));
    }

    [Fact]
    public void Evaluate_BlocksInternalPredictionWhenLocalReplayGateFails()
    {
        var result = BosonPredictionReadinessAuditor.Evaluate(MakeInput(localGateBlockers: ["fermion mode I branch stability 0 is below 0.5"]));

        Assert.Equal("boson-prediction-blocked", result.TerminalStatus);
        Assert.False(result.SourceBackedReplayReady);
        Assert.False(result.InternalBosonReplayPredictionReady);
        Assert.False(result.ExternalPhysicalComparisonReady);
        Assert.Contains(result.ValidationGates, g => g.GateId == "replay-integrity" && !g.Passed);
    }

    [Fact]
    public void Evaluate_ReturnsExternalReadyWhenEveryGatePasses()
    {
        var result = BosonPredictionReadinessAuditor.Evaluate(MakeInput(
            claimClass: "C1_LocalPersistentMode",
            hasMapping: true,
            hasCalibration: true,
            hasTargets: true,
            priorComparisonPassed: true,
            globalSidecarFalsifiers: 0));

        Assert.Equal("external-physical-boson-prediction-ready", result.TerminalStatus);
        Assert.True(result.SourceBackedReplayReady);
        Assert.True(result.InternalBosonReplayPredictionReady);
        Assert.True(result.ExternalPhysicalComparisonReady);
        Assert.Empty(result.Blockers);
        Assert.All(result.ValidationGates, gate => Assert.True(gate.Passed));
    }

    private static BosonPredictionReadinessInput MakeInput(
        IReadOnlyList<string>? localGateBlockers = null,
        string claimClass = "C0_NumericalMode",
        bool hasMapping = false,
        bool hasCalibration = false,
        bool hasTargets = true,
        bool? priorComparisonPassed = false,
        int globalSidecarFalsifiers = 3) => new()
    {
        CandidateId = "candidate-3",
        SelectedBosonModeId = "phase99-selector-eigenvector-full-lift-candidate-3-mode-0-4x4",
        ReplayTerminalStatus = "source-backed-analytic-replay-package-built",
        ReplayClosureRequirements = [],
        LocalGateBlockers = localGateBlockers ?? [],
        CouplingMagnitude = 0.00010677731386910604,
        FullConnectionLiftMaterialized = true,
        FullConnectionModeVectorLength = 576,
        BranchStabilityScore = 0.9735835329372028,
        RefinementStabilityScore = 0.8250944968993068,
        ClaimClass = claimClass,
        HasCandidateSpecificPhysicalMapping = hasMapping,
        HasCandidateSpecificCalibration = hasCalibration,
        HasPhysicalTargets = hasTargets,
        PriorAbsoluteComparisonPassed = priorComparisonPassed,
        TargetRelevantSevereFalsifierCount = 0,
        GlobalSidecarSevereFalsifierCount = globalSidecarFalsifiers,
        HasTargetScopedFalsifierPolicy = false,
        ResidualLimitations = ["secondary selector axes remain scalar"],
    };
}
