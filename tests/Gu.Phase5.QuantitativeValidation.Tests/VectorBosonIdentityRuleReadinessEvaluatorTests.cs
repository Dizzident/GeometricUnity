using Gu.Core;
using Gu.Phase5.QuantitativeValidation;

namespace Gu.Phase5.QuantitativeValidation.Tests;

public sealed class VectorBosonIdentityRuleReadinessEvaluatorTests
{
    [Fact]
    public void Evaluate_WithSelectorStableFamiliesButNoIdentityFeatures_RemainsBlocked()
    {
        var result = VectorBosonIdentityRuleReadinessEvaluator.Evaluate(
            [MakeSource("phase22-phase12-candidate-11"), MakeSource("phase22-phase12-candidate-2")],
            """
            {
              "families": [
                {
                  "familyId": "phase22-family-phase12-candidate-11",
                  "sourceCandidateId": "phase12-candidate-11",
                  "branchVariantIds": ["b0"],
                  "refinementLevels": ["L0"],
                  "environmentIds": ["env0"],
                  "ambiguityCount": 0,
                  "branchStabilityScore": 1.0,
                  "refinementStabilityScore": 1.0,
                  "environmentStabilityScore": 1.0
                },
                {
                  "familyId": "phase22-family-phase12-candidate-2",
                  "sourceCandidateId": "phase12-candidate-2",
                  "branchVariantIds": ["b0"],
                  "refinementLevels": ["L0"],
                  "environmentIds": ["env0"],
                  "ambiguityCount": 0,
                  "branchStabilityScore": 1.0,
                  "refinementStabilityScore": 1.0,
                  "environmentStabilityScore": 1.0
                }
              ]
            }
            """,
            MakeProvenance());

        Assert.Equal("identity-feature-blocked", result.TerminalStatus);
        Assert.Empty(result.DerivedRules);
        Assert.All(result.Coverage, c => Assert.False(c.IdentityRuleEligible));
        Assert.Contains(result.ClosureRequirements, r => r.Contains("charged/neutral", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Evaluate_WithChargedAndNeutralIdentityFeatures_DerivesRules()
    {
        var result = VectorBosonIdentityRuleReadinessEvaluator.Evaluate(
            [MakeSource("phase22-phase12-candidate-11"), MakeSource("phase22-phase12-candidate-2")],
            """
            {
              "families": [
                {
                  "familyId": "phase22-family-phase12-candidate-11",
                  "sourceCandidateId": "phase12-candidate-11",
                  "branchVariantIds": ["b0"],
                  "refinementLevels": ["L0"],
                  "environmentIds": ["env0"],
                  "ambiguityCount": 0,
                  "branchStabilityScore": 1.0,
                  "refinementStabilityScore": 1.0,
                  "environmentStabilityScore": 1.0,
                  "identityFeatures": {
                    "electroweakMultipletId": "ew-vector-triplet",
                    "chargeSector": "charged",
                    "currentCouplingSignature": "charged-current"
                  }
                },
                {
                  "familyId": "phase22-family-phase12-candidate-2",
                  "sourceCandidateId": "phase12-candidate-2",
                  "branchVariantIds": ["b0"],
                  "refinementLevels": ["L0"],
                  "environmentIds": ["env0"],
                  "ambiguityCount": 0,
                  "branchStabilityScore": 1.0,
                  "refinementStabilityScore": 1.0,
                  "environmentStabilityScore": 1.0,
                  "identityFeatures": {
                    "electroweakMultipletId": "ew-vector-triplet",
                    "chargeSector": "neutral",
                    "currentCouplingSignature": "neutral-current"
                  }
                }
              ]
            }
            """,
            MakeProvenance());

        Assert.Equal("identity-rule-ready", result.TerminalStatus);
        Assert.Equal(2, result.DerivedRules.Count);
        Assert.Contains(result.DerivedRules, r => r.ParticleId == "w-boson");
        Assert.Contains(result.DerivedRules, r => r.ParticleId == "z-boson");
    }

    private static CandidateModeSourceRecord MakeSource(string id) => new()
    {
        SourceId = id,
        SourceOrigin = CandidateModeExtractor.InternalComputedOrigin,
        SourceArtifactKind = CandidateModeExtractor.ComputedObservableArtifactKind,
        SourceArtifactPath = "studies/phase22/source_candidates.json",
        SourceObservableId = id,
        Value = 1.0,
        Uncertainty = 0.01,
        UnitFamily = "mass-energy",
        Unit = "internal-mass-unit",
        EnvironmentId = "env-a",
        BranchId = "branch-a",
        RefinementLevel = "L0",
        SourceExtractionMethod = "test",
        Provenance = MakeProvenance(),
    };

    private static ProvenanceMeta MakeProvenance() => new()
    {
        CreatedAt = DateTimeOffset.Parse("2026-04-26T00:00:00Z"),
        CodeRevision = "test",
        Branch = new BranchRef { BranchId = "phase24-wz-identity-rule-readiness", SchemaVersion = "1.0" },
        Backend = "cpu",
    };
}
