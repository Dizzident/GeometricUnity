using Gu.Core;
using Gu.Phase5.QuantitativeValidation;
using Gu.Phase5.Reporting;

namespace Gu.Phase5.Reporting.Tests;

public sealed class ElectroweakMassGenerationRelationDeriverTests
{
    private static readonly string[] TargetObservableIds =
    [
        "physical-w-boson-mass-gev",
        "physical-z-boson-mass-gev",
    ];

    [Fact]
    public void Derive_WithAcceptedWeakCouplingAndWzModes_ReturnsBridgeValue()
    {
        var result = ElectroweakMassGenerationRelationDeriver.Derive(
            MakeWeakCoupling(),
            MakeMode("phase22-phase12-candidate-0", "w-boson", 1.1158059937692792E-15, 1.3476944083805217E-18),
            MakeMode("phase22-phase12-candidate-2", "z-boson", 1.268406657962647E-15, 1.5805767578887164E-18),
            TargetObservableIds);

        Assert.Equal("electroweak-mass-generation-relation-derived", result.TerminalStatus);
        Assert.Equal(ElectroweakMassGenerationRelationDeriver.RelationId, result.MassGenerationRelationId);
        Assert.True(result.DimensionlessBridgeValue > 0);
        Assert.True(result.DimensionlessBridgeUncertainty > 0);
        Assert.Empty(result.ClosureRequirements);
    }

    [Fact]
    public void Derive_RejectsMissingTargetExclusion()
    {
        var result = ElectroweakMassGenerationRelationDeriver.Derive(
            MakeWeakCoupling(),
            MakeMode("phase22-phase12-candidate-0", "w-boson", 1.0, 0.01),
            MakeMode("phase22-phase12-candidate-2", "z-boson", 1.2, 0.01),
            ["physical-w-boson-mass-gev"]);

        Assert.Equal("electroweak-mass-generation-relation-blocked", result.TerminalStatus);
        Assert.Contains("physical-z-boson-mass-gev must be excluded from the relation derivation", result.ClosureRequirements);
    }

    internal static NormalizedWeakCouplingCandidateRecord MakeWeakCoupling() => new()
    {
        CandidateId = "phase65-dimensionless-weak-coupling-amplitude-candidate",
        SourceKind = NormalizedWeakCouplingInputAuditor.AcceptedSourceKind,
        NormalizationConvention = "physical-weak-coupling-normalization:su2-canonical-trace-half-v1",
        CouplingValue = 0.5656854249492381,
        CouplingUncertainty = 0.0028284271247461905,
        VariationMethod = "analytic-dirac-variation-matrix-element:v1",
        BranchStabilityScore = 0.98,
        ExcludedTargetObservableIds = TargetObservableIds,
        ProvenanceId = "test",
    };

    internal static IdentifiedPhysicalModeRecord MakeMode(string modeId, string particleId, double value, double uncertainty) => new()
    {
        ModeId = modeId,
        ParticleId = particleId,
        ModeKind = "vector-boson-mass-mode",
        ObservableId = modeId,
        Value = value,
        Uncertainty = uncertainty,
        UnitFamily = "mass-energy",
        Unit = "internal-mass-unit",
        Status = "validated",
        EnvironmentId = "env",
        BranchId = "branch",
        RefinementLevel = "L0",
        ExtractionMethod = "test",
        Assumptions = [],
        ClosureRequirements = [],
        Provenance = new ProvenanceMeta
        {
            CreatedAt = DateTimeOffset.Parse("2026-04-29T00:00:00Z"),
            CodeRevision = "test",
            Branch = new BranchRef { BranchId = "test", SchemaVersion = "1.0" },
            Backend = "cpu",
        },
    };
}
