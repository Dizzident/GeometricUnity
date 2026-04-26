using Gu.Core;
using Gu.Phase5.Reporting;

namespace Gu.Phase5.Reporting.Tests;

public sealed class WzNormalizationClosureDiagnosticTests
{
    [Fact]
    public void Evaluate_WithIdentityScaleAndInvariantSelector_IsBlocked()
    {
        var result = WzNormalizationClosureDiagnostic.Evaluate(
            MakeRatioDiagnosticJson(),
            MakeSelectorDiagnosticJson(targetInsideEnvelope: false, passingPointCount: 0),
            MakePhysicalCalibrationsJson(scaleFactor: 1.0, method: "dimensionless-identity-normalization-from-validated-wz-ratio"),
            MakeProvenance());

        Assert.Equal("wz-normalization-closure-blocked", result.TerminalStatus);
        Assert.False(result.SelectorVariationExplainsMiss);
        Assert.False(result.DerivationBackedScaleAvailable);
        Assert.False(result.NormalizationChangeAllowed);
        Assert.Equal(1.02, result.RequiredScaleToTarget);
        Assert.Equal(1.0, result.DeclaredScaleFactor);
        Assert.Contains(result.ClosureRequirements, r => r.Contains("derivation-backed", StringComparison.Ordinal));
    }

    [Fact]
    public void Evaluate_WithMatchingOperatorDerivedScale_IsReady()
    {
        var result = WzNormalizationClosureDiagnostic.Evaluate(
            MakeRatioDiagnosticJson(),
            MakeSelectorDiagnosticJson(targetInsideEnvelope: false, passingPointCount: 0),
            MakePhysicalCalibrationsJson(scaleFactor: 1.02, method: "operator-derivation-normalization-closure"),
            MakeProvenance());

        Assert.Equal("wz-normalization-closure-blocked", result.TerminalStatus);
        Assert.True(result.DerivationBackedScaleAvailable);
        Assert.True(result.NormalizationChangeAllowed);
        Assert.Contains(result.ClosureRequirements, r => r.Contains("selector variation", StringComparison.Ordinal));
    }

    private static string MakeRatioDiagnosticJson()
        => """
           {
             "resultId": "phase29-wz-ratio-failure-diagnostic-v1",
             "schemaVersion": "1.0.0",
             "terminalStatus": "wz-ratio-diagnostic-complete",
             "algorithmId": "p29-wz-ratio-failure-diagnostic:v1",
             "targetObservableId": "physical-w-z-mass-ratio",
             "targetValue": 0.88136,
             "targetUncertainty": 0.00015,
             "selectedPair": {
               "pairId": "w/z",
               "wSourceId": "w",
               "zSourceId": "z",
               "wChargeSector": "charged",
               "zChargeSector": "neutral",
               "ratio": 0.864,
               "uncertainty": {
                 "branchVariation": 0.001,
                 "refinementError": 0.0,
                 "extractionError": 0.001,
                 "environmentSensitivity": 0.001,
                 "totalUncertainty": 0.002
               },
               "targetDelta": -0.01736,
               "combinedSigma": 0.002,
               "pull": -8.68,
               "passesSigma5": false,
               "requiredScaleToTarget": 1.02,
               "requiredTotalUncertaintyForSigma5": 0.004,
               "uncertaintyInflationFactorForSigma5": 2.0,
               "selectionRole": "selected-by-identity-rule"
             },
             "bestDiagnosticPair": null,
             "pairDiagnostics": [],
             "diagnosis": [],
             "closureRequirements": [],
             "provenance": {
               "createdAt": "2026-04-26T00:00:00+00:00",
               "codeRevision": "test",
               "branch": { "branchId": "test", "schemaVersion": "1.0" },
               "backend": "cpu"
             }
           }
           """;

    private static string MakeSelectorDiagnosticJson(bool targetInsideEnvelope, int passingPointCount)
        => $$"""
           {
             "resultId": "phase30-wz-selector-variation-diagnostic-v1",
             "schemaVersion": "1.0.0",
             "terminalStatus": "selector-variation-diagnostic-complete",
             "algorithmId": "p30-wz-selector-variation-diagnostic:v1",
             "wSourceCandidateId": "w",
             "zSourceCandidateId": "z",
             "targetValue": 0.88136,
             "targetUncertainty": 0.00015,
             "alignedPointCount": 1,
             "ratioMin": 0.864,
             "ratioMax": 0.864,
             "ratioMean": 0.864,
             "ratioStandardDeviation": 0,
             "targetInsideObservedEnvelope": {{targetInsideEnvelope.ToString().ToLowerInvariant()}},
             "passingPointCount": {{passingPointCount}},
             "closestPoint": null,
             "points": [],
             "diagnosis": [],
             "closureRequirements": [],
             "provenance": {
               "createdAt": "2026-04-26T00:00:00+00:00",
               "codeRevision": "test",
               "branch": { "branchId": "test", "schemaVersion": "1.0" },
               "backend": "cpu"
             }
           }
           """;

    private static string MakePhysicalCalibrationsJson(double scaleFactor, string method)
        => $$"""
           {
             "tableId": "calibrations",
             "calibrations": [
               {
                 "calibrationId": "test-calibration",
                 "mappingId": "test-mapping",
                 "sourceComputedObservableId": "physical-w-z-mass-ratio",
                 "sourceUnitFamily": "dimensionless",
                 "targetUnitFamily": "dimensionless",
                 "targetUnit": "dimensionless",
                 "scaleFactor": {{scaleFactor}},
                 "scaleUncertainty": 0,
                 "status": "validated",
                 "method": "{{method}}",
                 "source": "test",
                 "assumptions": [],
                 "closureRequirements": []
               }
             ]
           }
           """;

    private static ProvenanceMeta MakeProvenance() => new()
    {
        CreatedAt = DateTimeOffset.Parse("2026-04-26T00:00:00Z"),
        CodeRevision = "test",
        Branch = new BranchRef { BranchId = "phase31-wz-normalization-closure-diagnostic", SchemaVersion = "1.0" },
        Backend = "cpu",
    };
}
