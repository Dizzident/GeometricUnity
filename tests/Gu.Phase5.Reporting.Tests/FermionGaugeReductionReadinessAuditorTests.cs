using Gu.Phase5.Reporting;

namespace Gu.Phase5.Reporting.Tests;

public sealed class FermionGaugeReductionReadinessAuditorTests
{
    [Fact]
    public void Audit_WithGaugeReducedInputs_IsReady()
    {
        var result = FermionGaugeReductionReadinessAuditor.Audit(new FermionGaugeReductionInputRecord
        {
            ArtifactId = "phase87-ready",
            BackgroundId = "bg",
            DiracBundleGaugeReductionApplied = true,
            FermionModesGaugeReductionApplied = true,
            SolverConfigRequestedGaugeReduction = true,
            HasGaugeProjectorArtifact = true,
            HasGaugeReducedDiracOperatorArtifact = true,
            HasBranchRefinementStabilityEvidence = true,
        });

        Assert.Equal("fermion-gauge-reduction-ready", result.TerminalStatus);
        Assert.Empty(result.ClosureRequirements);
    }

    [Fact]
    public void Audit_WhenSolverRequestedReductionButBundleIsUnreduced_BlocksWithSpecificRequirement()
    {
        var result = FermionGaugeReductionReadinessAuditor.Audit(new FermionGaugeReductionInputRecord
        {
            ArtifactId = "phase87-blocked",
            BackgroundId = "bg",
            DiracBundleGaugeReductionApplied = false,
            FermionModesGaugeReductionApplied = false,
            SolverConfigRequestedGaugeReduction = true,
            HasGaugeProjectorArtifact = false,
            HasGaugeReducedDiracOperatorArtifact = false,
            HasBranchRefinementStabilityEvidence = false,
        });

        Assert.Equal("fermion-gauge-reduction-blocked", result.TerminalStatus);
        Assert.Contains(
            "solver requested gauge reduction but the Dirac bundle was assembled with gaugeReductionApplied=false",
            result.ClosureRequirements);
        Assert.Contains("gauge-reduced Dirac bundle is missing", result.ClosureRequirements);
        Assert.Contains("fermion-compatible gauge projector artifact is missing", result.ClosureRequirements);
    }
}
