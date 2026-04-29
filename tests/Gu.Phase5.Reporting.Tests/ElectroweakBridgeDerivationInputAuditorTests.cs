using Gu.Phase5.Reporting;

namespace Gu.Phase5.Reporting.Tests;

public sealed class ElectroweakBridgeDerivationInputAuditorTests
{
    [Fact]
    public void Audit_ReportsBlockedWhenBridgeInputsAreMissing()
    {
        var result = ElectroweakBridgeDerivationInputAuditor.Audit(
            hasValidatedWzModes: true,
            hasExternalElectroweakScale: true,
            hasNormalizedWeakCoupling: false,
            hasMassGenerationRelation: false,
            hasScalarSectorBridge: false,
            hasSharedScaleCheck: false);

        Assert.Equal("bridge-derivation-inputs-blocked", result.TerminalStatus);
        Assert.Contains("normalized internal weak coupling is missing", result.ClosureRequirements);
        Assert.Contains("validated internal mass-generation relation is missing", result.ClosureRequirements);
        Assert.Contains("scalar/Higgs-sector bridge evidence is missing", result.ClosureRequirements);
        Assert.Contains("shared W/Z GeV-per-internal-mass-unit scale check is missing", result.ClosureRequirements);

        var wzModes = result.Records.Single(record => record.ArtifactId == "validated-wz-internal-modes");
        Assert.Equal("available", wzModes.Status);
    }

    [Fact]
    public void Audit_ReportsReadyWhenAllInputsAreAvailable()
    {
        var result = ElectroweakBridgeDerivationInputAuditor.Audit(
            hasValidatedWzModes: true,
            hasExternalElectroweakScale: true,
            hasNormalizedWeakCoupling: true,
            hasMassGenerationRelation: true,
            hasScalarSectorBridge: true,
            hasSharedScaleCheck: true);

        Assert.Equal("bridge-derivation-inputs-ready", result.TerminalStatus);
        Assert.Empty(result.ClosureRequirements);
        Assert.All(result.Records, record => Assert.Equal("available", record.Status));
    }
}
