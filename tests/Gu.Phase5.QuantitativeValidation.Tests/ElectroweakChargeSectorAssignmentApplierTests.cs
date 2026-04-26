using Gu.Core;
using Gu.Phase5.QuantitativeValidation;

namespace Gu.Phase5.QuantitativeValidation.Tests;

public sealed class ElectroweakChargeSectorAssignmentApplierTests
{
    [Fact]
    public void Apply_WithReadyMixingAssignments_UpdatesFeatureAndFamilyChargeSectors()
    {
        var result = ElectroweakChargeSectorAssignmentApplier.Apply(
            MakeIdentityFeaturesJson(),
            MakeModeFamiliesJson(),
            MakeMixingReadinessJson("mixing-convention-ready"),
            MakeProvenance());

        Assert.Equal("charge-sectors-applied", result.TerminalStatus);
        Assert.Equal(2, result.UpdatedFeatureRecordCount);
        Assert.Equal(2, result.UpdatedModeFamilyCount);
        Assert.Equal(1, result.ChargedCount);
        Assert.Equal(1, result.NeutralCount);
        Assert.Contains("\"chargeSector\": \"charged\"", result.UpdatedIdentityFeaturesJson);
        Assert.Contains("\"chargeSector\": \"neutral\"", result.UpdatedModeFamiliesJson);
        Assert.Contains("\"terminalStatus\": \"identity-features-complete\"", result.UpdatedIdentityFeaturesJson);
        Assert.DoesNotContain("charged/neutral sector remains unassigned", result.UpdatedIdentityFeaturesJson);
    }

    [Fact]
    public void Apply_WithBlockedMixingReadiness_RemainsBlocked()
    {
        var result = ElectroweakChargeSectorAssignmentApplier.Apply(
            MakeIdentityFeaturesJson(),
            MakeModeFamiliesJson(),
            MakeMixingReadinessJson("mixing-convention-blocked"),
            MakeProvenance());

        Assert.Equal("charge-sector-application-blocked", result.TerminalStatus);
        Assert.Contains(result.ClosureRequirements, r => r.Contains("mixing-convention-ready", StringComparison.Ordinal));
        Assert.Equal(0, result.UpdatedFeatureRecordCount);
        Assert.Contains("charged/neutral sector remains unassigned", result.UpdatedIdentityFeaturesJson);
    }

    private static string MakeIdentityFeaturesJson()
        => """
           {
             "resultId": "phase25-internal-electroweak-identity-features-v1",
             "schemaVersion": "1.0.0",
             "terminalStatus": "identity-features-partial",
             "algorithmId": "p25-internal-electroweak-feature-extractor:v1",
             "featureRecords": [
               {
                 "familyId": "phase22-family-phase12-candidate-a",
                 "sourceCandidateId": "phase12-candidate-a",
                 "electroweakMultipletId": "su2-adjoint-triplet:canonical-basis",
                 "chargeSector": null,
                 "currentCouplingSignature": "finite-difference-current-profile:a",
                 "featureStatus": "partial",
                 "blockers": [
                   "charged/neutral sector remains unassigned because no electromagnetic or U(1)-mixing convention is present in the internal artifacts."
                 ]
               },
               {
                 "familyId": "phase22-family-phase12-candidate-b",
                 "sourceCandidateId": "phase12-candidate-b",
                 "electroweakMultipletId": "su2-adjoint-triplet:canonical-basis",
                 "chargeSector": null,
                 "currentCouplingSignature": "finite-difference-current-profile:b",
                 "featureStatus": "partial",
                 "blockers": [
                   "charged/neutral sector remains unassigned because no electromagnetic or U(1)-mixing convention is present in the internal artifacts."
                 ]
               }
             ],
             "summaryBlockers": [
               "charged/neutral sector remains unassigned because no electromagnetic or U(1)-mixing convention is present in the internal artifacts."
             ]
           }
           """;

    private static string MakeModeFamiliesJson()
        => """
           {
             "families": [
               {
                 "familyId": "phase22-family-phase12-candidate-a",
                 "sourceCandidateId": "phase12-candidate-a",
                 "identityFeatures": {
                   "featureStatus": "partial",
                   "electroweakMultipletId": "su2-adjoint-triplet:canonical-basis",
                   "chargeSector": null,
                   "currentCouplingSignature": "finite-difference-current-profile:a",
                   "blockers": [
                     "charged/neutral sector remains unassigned because no electromagnetic or U(1)-mixing convention is present in the internal artifacts."
                   ]
                 }
               },
               {
                 "familyId": "phase22-family-phase12-candidate-b",
                 "sourceCandidateId": "phase12-candidate-b",
                 "identityFeatures": {
                   "featureStatus": "partial",
                   "electroweakMultipletId": "su2-adjoint-triplet:canonical-basis",
                   "chargeSector": null,
                   "currentCouplingSignature": "finite-difference-current-profile:b",
                   "blockers": [
                     "charged/neutral sector remains unassigned because no electromagnetic or U(1)-mixing convention is present in the internal artifacts."
                   ]
                 }
               }
             ]
           }
           """;

    private static string MakeMixingReadinessJson(string terminalStatus)
        => $$"""
           {
             "resultId": "phase26-electroweak-mixing-convention-readiness-v1",
             "schemaVersion": "1.0.0",
             "terminalStatus": "{{terminalStatus}}",
             "algorithmId": "p26-electroweak-mixing-convention-readiness:v1",
             "sourceFeatureCount": 2,
             "chargeSectorAssignments": [
               {
                 "sourceCandidateId": "phase12-candidate-a",
                 "electroweakMultipletId": "su2-adjoint-triplet:canonical-basis",
                 "dominantBasisIndex": 0,
                 "chargeSector": "charged",
                 "derivationId": "test"
               },
               {
                 "sourceCandidateId": "phase12-candidate-b",
                 "electroweakMultipletId": "su2-adjoint-triplet:canonical-basis",
                 "dominantBasisIndex": 2,
                 "chargeSector": "neutral",
                 "derivationId": "test"
               }
             ],
             "closureRequirements": [],
             "provenance": {
               "createdAt": "2026-04-26T00:00:00+00:00",
               "codeRevision": "test",
               "branch": { "branchId": "test", "schemaVersion": "1.0" },
               "backend": "cpu"
             }
           }
           """;

    private static ProvenanceMeta MakeProvenance() => new()
    {
        CreatedAt = DateTimeOffset.Parse("2026-04-26T00:00:00Z"),
        CodeRevision = "test",
        Branch = new BranchRef { BranchId = "phase27-charge-sector-convention", SchemaVersion = "1.0" },
        Backend = "cpu",
    };
}
