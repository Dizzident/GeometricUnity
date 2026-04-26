using Gu.Core;
using Gu.Phase5.Reporting;

namespace Gu.Phase5.Reporting.Tests;

public sealed class WzRatioFailureDiagnosticTests
{
    [Fact]
    public void Evaluate_WithReadyArtifacts_ComputesSelectedAndBestDiagnosticPairs()
    {
        var result = WzRatioFailureDiagnostic.Evaluate(
            MakeIdentityReadinessJson("identity-rule-ready"),
            MakeMixingReadinessJson("mixing-convention-ready"),
            MakeCandidateSourcesJson(),
            MakeModeFamiliesJson(),
            MakeTargetTableJson(),
            MakeProvenance());

        Assert.Equal("wz-ratio-diagnostic-complete", result.TerminalStatus);
        Assert.Equal(4, result.PairDiagnostics.Count);
        Assert.NotNull(result.SelectedPair);
        Assert.Equal("phase22-phase12-candidate-0/phase22-phase12-candidate-2", result.SelectedPair!.PairId);
        Assert.Equal("selected-by-identity-rule", result.SelectedPair.SelectionRole);
        Assert.NotNull(result.BestDiagnosticPair);
        Assert.Contains(result.Diagnosis, d => d.Contains("diagnostic-only", StringComparison.Ordinal));
    }

    [Fact]
    public void Evaluate_WithBlockedIdentityReadiness_IsBlocked()
    {
        var result = WzRatioFailureDiagnostic.Evaluate(
            MakeIdentityReadinessJson("identity-feature-blocked"),
            MakeMixingReadinessJson("mixing-convention-ready"),
            MakeCandidateSourcesJson(),
            MakeModeFamiliesJson(),
            MakeTargetTableJson(),
            MakeProvenance());

        Assert.Equal("wz-ratio-diagnostic-blocked", result.TerminalStatus);
        Assert.Contains(result.ClosureRequirements, r => r.Contains("identity-rule readiness", StringComparison.Ordinal));
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

    private static string MakeMixingReadinessJson(string terminalStatus)
        => $$"""
           {
             "resultId": "phase26-electroweak-mixing-convention-readiness-v1",
             "schemaVersion": "1.0.0",
             "terminalStatus": "{{terminalStatus}}",
             "algorithmId": "p26-electroweak-mixing-convention-readiness:v1",
             "sourceFeatureCount": 4,
             "chargeSectorAssignments": [
               { "sourceCandidateId": "phase12-candidate-0", "electroweakMultipletId": "su2", "dominantBasisIndex": 0, "chargeSector": "charged", "derivationId": "test" },
               { "sourceCandidateId": "phase12-candidate-1", "electroweakMultipletId": "su2", "dominantBasisIndex": 1, "chargeSector": "charged", "derivationId": "test" },
               { "sourceCandidateId": "phase12-candidate-2", "electroweakMultipletId": "su2", "dominantBasisIndex": 2, "chargeSector": "neutral", "derivationId": "test" },
               { "sourceCandidateId": "phase12-candidate-3", "electroweakMultipletId": "su2", "dominantBasisIndex": 2, "chargeSector": "neutral", "derivationId": "test" }
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
             "readySourceCount": 4,
             "candidateModeSources": [
               { "sourceId": "phase22-phase12-candidate-0", "sourceOrigin": "internal", "sourceArtifactKind": "table", "sourceArtifactPath": "test", "sourceObservableId": "phase22-phase12-candidate-0", "value": 8.0, "uncertainty": 0.1, "unitFamily": "mass-energy", "unit": "u", "environmentId": "env", "branchId": "branch", "refinementLevel": "L0", "sourceExtractionMethod": "test", "provenance": { "createdAt": "2026-04-26T00:00:00+00:00", "codeRevision": "test", "branch": { "branchId": "test", "schemaVersion": "1.0" }, "backend": "cpu" } },
               { "sourceId": "phase22-phase12-candidate-1", "sourceOrigin": "internal", "sourceArtifactKind": "table", "sourceArtifactPath": "test", "sourceObservableId": "phase22-phase12-candidate-1", "value": 8.8, "uncertainty": 0.1, "unitFamily": "mass-energy", "unit": "u", "environmentId": "env", "branchId": "branch", "refinementLevel": "L0", "sourceExtractionMethod": "test", "provenance": { "createdAt": "2026-04-26T00:00:00+00:00", "codeRevision": "test", "branch": { "branchId": "test", "schemaVersion": "1.0" }, "backend": "cpu" } },
               { "sourceId": "phase22-phase12-candidate-2", "sourceOrigin": "internal", "sourceArtifactKind": "table", "sourceArtifactPath": "test", "sourceObservableId": "phase22-phase12-candidate-2", "value": 10.0, "uncertainty": 0.1, "unitFamily": "mass-energy", "unit": "u", "environmentId": "env", "branchId": "branch", "refinementLevel": "L0", "sourceExtractionMethod": "test", "provenance": { "createdAt": "2026-04-26T00:00:00+00:00", "codeRevision": "test", "branch": { "branchId": "test", "schemaVersion": "1.0" }, "backend": "cpu" } },
               { "sourceId": "phase22-phase12-candidate-3", "sourceOrigin": "internal", "sourceArtifactKind": "table", "sourceArtifactPath": "test", "sourceObservableId": "phase22-phase12-candidate-3", "value": 9.98, "uncertainty": 0.1, "unitFamily": "mass-energy", "unit": "u", "environmentId": "env", "branchId": "branch", "refinementLevel": "L0", "sourceExtractionMethod": "test", "provenance": { "createdAt": "2026-04-26T00:00:00+00:00", "codeRevision": "test", "branch": { "branchId": "test", "schemaVersion": "1.0" }, "backend": "cpu" } }
             ]
           }
           """;

    private static string MakeModeFamiliesJson()
        => """
           {
             "families": [
               { "sourceCandidateId": "phase12-candidate-0", "uncertainty": { "branchVariation": 0.01, "refinementError": 0.01, "extractionError": 0.01, "environmentSensitivity": 0.01, "totalUncertainty": 0.02 } },
               { "sourceCandidateId": "phase12-candidate-1", "uncertainty": { "branchVariation": 0.01, "refinementError": 0.01, "extractionError": 0.01, "environmentSensitivity": 0.01, "totalUncertainty": 0.02 } },
               { "sourceCandidateId": "phase12-candidate-2", "uncertainty": { "branchVariation": 0.01, "refinementError": 0.01, "extractionError": 0.01, "environmentSensitivity": 0.01, "totalUncertainty": 0.02 } },
               { "sourceCandidateId": "phase12-candidate-3", "uncertainty": { "branchVariation": 0.01, "refinementError": 0.01, "extractionError": 0.01, "environmentSensitivity": 0.01, "totalUncertainty": 0.02 } }
             ]
           }
           """;

    private static string MakeTargetTableJson()
        => """
           {
             "tableId": "targets",
             "targets": [
               {
                 "observableId": "physical-w-z-mass-ratio",
                 "label": "target",
                 "value": 0.88136,
                 "uncertainty": 0.00015,
                 "source": "test"
               }
             ]
           }
           """;

    private static ProvenanceMeta MakeProvenance() => new()
    {
        CreatedAt = DateTimeOffset.Parse("2026-04-26T00:00:00Z"),
        CodeRevision = "test",
        Branch = new BranchRef { BranchId = "phase29-wz-ratio-failure-diagnostic", SchemaVersion = "1.0" },
        Backend = "cpu",
    };
}
