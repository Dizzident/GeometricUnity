using Gu.Core;
using Gu.Phase5.Reporting;

namespace Gu.Phase5.Reporting.Tests;

public sealed class WzPhysicalPredictionArtifactPromoterTests
{
    [Fact]
    public void Promote_WithIdentityRuleReadyInputs_EmitsValidatedPhysicalCampaignArtifacts()
    {
        var result = WzPhysicalPredictionArtifactPromoter.Promote(
            MakeIdentityReadinessJson("identity-rule-ready"),
            MakeCandidateSourcesJson(),
            MakeModeFamiliesJson(),
            MakeProvenance());

        Assert.Equal("physical-prediction-artifacts-ready", result.TerminalStatus);
        Assert.Equal(2, result.PhysicalModeRecords.Count);
        Assert.All(result.PhysicalModeRecords, mode => Assert.Equal("validated", mode.Status));
        Assert.All(result.ModeIdentificationEvidence.Evidence, evidence => Assert.Equal("validated", evidence.Status));
        var observable = Assert.Single(result.Observables);
        Assert.Equal("physical-w-z-mass-ratio", observable.ObservableId);
        Assert.True(observable.Value > 0);
        Assert.True(observable.Uncertainty.TotalUncertainty > 0);
        Assert.True(observable.Uncertainty.BranchVariation >= 0);
        Assert.Equal("validated", Assert.Single(result.PhysicalObservableMappings.Mappings).Status);
        Assert.True(Assert.Single(result.ObservableClassifications.Classifications).PhysicalClaimAllowed);
        Assert.Equal("validated", Assert.Single(result.PhysicalCalibrations.Calibrations).Status);
    }

    [Fact]
    public void Promote_WithBlockedIdentityReadiness_RemainsBlocked()
    {
        var result = WzPhysicalPredictionArtifactPromoter.Promote(
            MakeIdentityReadinessJson("identity-feature-blocked"),
            MakeCandidateSourcesJson(),
            MakeModeFamiliesJson(),
            MakeProvenance());

        Assert.Equal("physical-prediction-artifact-promotion-blocked", result.TerminalStatus);
        Assert.Contains(result.ClosureRequirements, r => r.Contains("identity-rule-ready", StringComparison.Ordinal));
        Assert.Empty(result.PhysicalModeRecords);
    }

    private static string MakeIdentityReadinessJson(string terminalStatus)
        => $$"""
           {
             "resultId": "phase24-wz-identity-rule-readiness-v1",
             "schemaVersion": "1.0.0",
             "terminalStatus": "{{terminalStatus}}",
             "algorithmId": "p24-wz-identity-feature-readiness:v1",
             "coverage": [],
             "derivedRules": [
               {
                 "ruleId": "validated-w-boson-identity-rule-from-internal-features",
                 "particleId": "w-boson",
                 "sourceId": "phase22-phase12-candidate-0",
                 "sourceObservableId": "phase22-phase12-candidate-0",
                 "derivationId": "test:w",
                 "status": "validated",
                 "assumptions": ["test"]
               },
               {
                 "ruleId": "validated-z-boson-identity-rule-from-internal-features",
                 "particleId": "z-boson",
                 "sourceId": "phase22-phase12-candidate-2",
                 "sourceObservableId": "phase22-phase12-candidate-2",
                 "derivationId": "test:z",
                 "status": "validated",
                 "assumptions": ["test"]
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

    private static string MakeCandidateSourcesJson()
        => """
           {
             "tableId": "test",
             "schemaVersion": "1.0.0",
             "terminalStatus": "ready",
             "readySourceCount": 2,
             "candidateModeSources": [
               {
                 "sourceId": "phase22-phase12-candidate-0",
                 "sourceOrigin": "internal-computed-artifact",
                 "sourceArtifactKind": "computed-observable-table",
                 "sourceArtifactPath": "test",
                 "sourceObservableId": "phase22-phase12-candidate-0",
                 "value": 8.0,
                 "uncertainty": 0.4,
                 "unitFamily": "mass-energy",
                 "unit": "internal-mass-unit",
                 "environmentId": "env",
                 "branchId": "branch",
                 "refinementLevel": "L0",
                 "sourceExtractionMethod": "test",
                 "provenance": {
                   "createdAt": "2026-04-26T00:00:00+00:00",
                   "codeRevision": "test",
                   "branch": { "branchId": "test", "schemaVersion": "1.0" },
                   "backend": "cpu"
                 }
               },
               {
                 "sourceId": "phase22-phase12-candidate-2",
                 "sourceOrigin": "internal-computed-artifact",
                 "sourceArtifactKind": "computed-observable-table",
                 "sourceArtifactPath": "test",
                 "sourceObservableId": "phase22-phase12-candidate-2",
                 "value": 10.0,
                 "uncertainty": 0.5,
                 "unitFamily": "mass-energy",
                 "unit": "internal-mass-unit",
                 "environmentId": "env",
                 "branchId": "branch",
                 "refinementLevel": "L0",
                 "sourceExtractionMethod": "test",
                 "provenance": {
                   "createdAt": "2026-04-26T00:00:00+00:00",
                   "codeRevision": "test",
                   "branch": { "branchId": "test", "schemaVersion": "1.0" },
                   "backend": "cpu"
                 }
               }
             ]
           }
           """;

    private static string MakeModeFamiliesJson()
        => """
           {
             "families": [
               {
                 "sourceCandidateId": "phase12-candidate-0",
                 "uncertainty": {
                   "branchVariation": 0.1,
                   "refinementError": 0.2,
                   "extractionError": 0.3,
                   "environmentSensitivity": 0.1,
                   "totalUncertainty": 0.4
                 }
               },
               {
                 "sourceCandidateId": "phase12-candidate-2",
                 "uncertainty": {
                   "branchVariation": 0.2,
                   "refinementError": 0.2,
                   "extractionError": 0.4,
                   "environmentSensitivity": 0.1,
                   "totalUncertainty": 0.5
                 }
               }
             ]
           }
           """;

    private static ProvenanceMeta MakeProvenance() => new()
    {
        CreatedAt = DateTimeOffset.Parse("2026-04-26T00:00:00Z"),
        CodeRevision = "test",
        Branch = new BranchRef { BranchId = "phase28-wz-physical-prediction-artifact-promotion", SchemaVersion = "1.0" },
        Backend = "cpu",
    };
}
