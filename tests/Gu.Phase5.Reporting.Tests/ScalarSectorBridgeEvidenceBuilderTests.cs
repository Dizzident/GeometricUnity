using Gu.Phase5.Reporting;

namespace Gu.Phase5.Reporting.Tests;

public sealed class ScalarSectorBridgeEvidenceBuilderTests
{
    [Fact]
    public void Build_WithMassGenerationRelationAndDeclaredOrderParameter_ReturnsEvidence()
    {
        var relation = MakeRelation();

        var result = ScalarSectorBridgeEvidenceBuilder.Build(
            relation,
            externalScaleInputId: "phase54-fermi-derived-electroweak-vacuum-scale",
            scalarOrderParameterDeclared: true);

        Assert.Equal("scalar-sector-bridge-evidence-derived", result.TerminalStatus);
        Assert.Equal(ScalarSectorBridgeEvidenceBuilder.EvidenceId, result.EvidenceId);
        Assert.Empty(result.ClosureRequirements);
    }

    [Fact]
    public void Build_RejectsMissingOrderParameter()
    {
        var result = ScalarSectorBridgeEvidenceBuilder.Build(
            MakeRelation(),
            externalScaleInputId: "phase54-fermi-derived-electroweak-vacuum-scale",
            scalarOrderParameterDeclared: false);

        Assert.Equal("scalar-sector-bridge-evidence-blocked", result.TerminalStatus);
        Assert.Contains("scalar-sector order-parameter role is not declared", result.ClosureRequirements);
    }

    internal static ElectroweakMassGenerationRelationResult MakeRelation()
        => ElectroweakMassGenerationRelationDeriver.Derive(
            ElectroweakMassGenerationRelationDeriverTests.MakeWeakCoupling(),
            ElectroweakMassGenerationRelationDeriverTests.MakeMode("phase22-phase12-candidate-0", "w-boson", 1.1158059937692792E-15, 1.3476944083805217E-18),
            ElectroweakMassGenerationRelationDeriverTests.MakeMode("phase22-phase12-candidate-2", "z-boson", 1.268406657962647E-15, 1.5805767578887164E-18),
            ["physical-w-boson-mass-gev", "physical-z-boson-mass-gev"]);
}
