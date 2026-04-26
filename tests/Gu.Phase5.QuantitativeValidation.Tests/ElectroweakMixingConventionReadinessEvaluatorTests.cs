using Gu.Core;
using Gu.Phase5.QuantitativeValidation;

namespace Gu.Phase5.QuantitativeValidation.Tests;

public sealed class ElectroweakMixingConventionReadinessEvaluatorTests
{
    [Fact]
    public void Evaluate_WithoutConvention_RemainsBlocked()
    {
        var result = ElectroweakMixingConventionReadinessEvaluator.Evaluate(
            MakeFeaturesJson(),
            mixingConventionJson: null,
            MakeProvenance());

        Assert.Equal("mixing-convention-blocked", result.TerminalStatus);
        Assert.Empty(result.ChargeSectorAssignments);
        Assert.Contains(result.ClosureRequirements, r => r.Contains("mixing convention", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Evaluate_WithValidatedConvention_AssignsChargedAndNeutralSectors()
    {
        var result = ElectroweakMixingConventionReadinessEvaluator.Evaluate(
            MakeFeaturesJson(),
            """
            {
              "conventionId": "test-convention",
              "schemaVersion": "1.0.0",
              "status": "validated",
              "electroweakMultipletId": "su2-adjoint-triplet:canonical-basis",
              "u1GeneratorId": "test-u1-generator",
              "chargeOperatorDerivationId": "test-charge-operator",
              "neutralBasisAxisIndex": 2,
              "chargedBasisAxisIndices": [0, 1],
              "externalTargetValuesUsed": false,
              "assumptions": ["test-only convention"]
            }
            """,
            MakeProvenance());

        Assert.Equal("mixing-convention-ready", result.TerminalStatus);
        Assert.Equal(2, result.ChargeSectorAssignments.Count);
        Assert.Contains(result.ChargeSectorAssignments, a => a.SourceCandidateId == "phase12-candidate-a" && a.ChargeSector == "charged");
        Assert.Contains(result.ChargeSectorAssignments, a => a.SourceCandidateId == "phase12-candidate-b" && a.ChargeSector == "neutral");
    }

    [Fact]
    public void Evaluate_WithTargetDerivedConvention_RemainsBlocked()
    {
        var result = ElectroweakMixingConventionReadinessEvaluator.Evaluate(
            MakeFeaturesJson(),
            """
            {
              "conventionId": "bad-convention",
              "schemaVersion": "1.0.0",
              "status": "validated",
              "electroweakMultipletId": "su2-adjoint-triplet:canonical-basis",
              "u1GeneratorId": "test-u1-generator",
              "chargeOperatorDerivationId": "test-charge-operator",
              "neutralBasisAxisIndex": 2,
              "chargedBasisAxisIndices": [0, 1],
              "externalTargetValuesUsed": true,
              "assumptions": []
            }
            """,
            MakeProvenance());

        Assert.Equal("mixing-convention-blocked", result.TerminalStatus);
        Assert.Contains(result.ClosureRequirements, r => r.Contains("external physical target", StringComparison.OrdinalIgnoreCase));
    }

    private static string MakeFeaturesJson()
        => """
           {
             "featureRecords": [
               {
                 "sourceCandidateId": "phase12-candidate-a",
                 "electroweakMultipletId": "su2-adjoint-triplet:canonical-basis",
                 "dominantBasisIndex": 0
               },
               {
                 "sourceCandidateId": "phase12-candidate-b",
                 "electroweakMultipletId": "su2-adjoint-triplet:canonical-basis",
                 "dominantBasisIndex": 2
               }
             ]
           }
           """;

    private static ProvenanceMeta MakeProvenance() => new()
    {
        CreatedAt = DateTimeOffset.Parse("2026-04-26T00:00:00Z"),
        CodeRevision = "test",
        Branch = new BranchRef { BranchId = "phase26-electroweak-mixing-convention", SchemaVersion = "1.0" },
        Backend = "cpu",
    };
}
