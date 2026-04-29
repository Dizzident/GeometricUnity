using Gu.Phase5.Reporting;

namespace Gu.Phase5.Reporting.Tests;

public sealed class SharedWzScaleBridgeBuilderTests
{
    [Fact]
    public void Build_WithRelationAndScalarEvidence_ReturnsValidatedBridge()
    {
        var relation = ScalarSectorBridgeEvidenceBuilderTests.MakeRelation();
        var scalar = ScalarSectorBridgeEvidenceBuilder.Build(
            relation,
            "phase54-fermi-derived-electroweak-vacuum-scale",
            scalarOrderParameterDeclared: true);

        var result = SharedWzScaleBridgeBuilder.Build(relation, scalar, relativeTolerance: 1e-12);

        Assert.Equal("shared-wz-scale-bridge-validated", result.TerminalStatus);
        Assert.NotNull(result.Bridge);
        Assert.Empty(ElectroweakBridgeValidator.ValidateForAbsoluteWzProjection(result.Bridge));
        Assert.Equal("validated-internal-mass-generation-relation", result.Bridge!.InputKind);
    }

    [Fact]
    public void Build_RejectsMissingScalarEvidence()
    {
        var relation = ScalarSectorBridgeEvidenceBuilderTests.MakeRelation();
        var scalar = ScalarSectorBridgeEvidenceBuilder.Build(
            relation,
            "phase54-fermi-derived-electroweak-vacuum-scale",
            scalarOrderParameterDeclared: false);

        var result = SharedWzScaleBridgeBuilder.Build(relation, scalar, relativeTolerance: 1e-12);

        Assert.Equal("shared-wz-scale-bridge-blocked", result.TerminalStatus);
        Assert.Contains("scalar-sector bridge evidence has not been derived", result.ClosureRequirements);
    }
}
