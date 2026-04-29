using Gu.Phase5.Reporting;

namespace Gu.Phase5.Reporting.Tests;

public sealed class WeakCouplingOperatorDerivationAuditorTests
{
    [Fact]
    public void Audit_ReportsBlockedWhenOnlyOperatorUnitAndModesAreAvailable()
    {
        var result = WeakCouplingOperatorDerivationAuditor.Audit(
            hasSharedWzOperatorUnit: true,
            hasSelectedWzSourceModes: true,
            hasCanonicalSu2GeneratorNormalization: false,
            hasNonProxyFermionCurrentMatrixElement: false,
            hasDimensionlessCouplingAmplitudeExtractor: false,
            hasCouplingUncertaintyPropagation: false,
            hasBranchStabilityEvidence: false);

        Assert.Equal("weak-coupling-operator-derivation-blocked", result.TerminalStatus);
        Assert.Contains("canonical SU(2) generator normalization is missing", result.ClosureRequirements);
        Assert.Contains("non-proxy fermion-current matrix element is missing", result.ClosureRequirements);
        Assert.Contains("dimensionless coupling amplitude extractor is missing", result.ClosureRequirements);
        Assert.Contains("coupling uncertainty propagation is missing", result.ClosureRequirements);
        Assert.Contains("branch-stability evidence is missing", result.ClosureRequirements);

        var sharedUnit = result.Records.Single(record => record.InputId == "shared-wz-operator-unit");
        Assert.Equal("available", sharedUnit.Status);
    }

    [Fact]
    public void Audit_ReportsReadyWhenAllDerivationInputsAreAvailable()
    {
        var result = WeakCouplingOperatorDerivationAuditor.Audit(
            hasSharedWzOperatorUnit: true,
            hasSelectedWzSourceModes: true,
            hasCanonicalSu2GeneratorNormalization: true,
            hasNonProxyFermionCurrentMatrixElement: true,
            hasDimensionlessCouplingAmplitudeExtractor: true,
            hasCouplingUncertaintyPropagation: true,
            hasBranchStabilityEvidence: true);

        Assert.Equal("weak-coupling-operator-derivation-ready", result.TerminalStatus);
        Assert.Empty(result.ClosureRequirements);
        Assert.All(result.Records, record => Assert.Equal("available", record.Status));
    }
}
