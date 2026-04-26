using Gu.Core;
using Gu.Core.Serialization;
using Gu.Phase5.Reporting;

namespace Gu.Phase5.Reporting.Tests;

public sealed class WzCanonicalOperatorNormalizationDeriverTests
{
    [Fact]
    public void Derive_WithSharedOperatorUnit_ReturnsTargetIndependentScaleOne()
    {
        var result = WzCanonicalOperatorNormalizationDeriver.Derive(
            MakeP31Json(),
            MakeCandidateSourcesJson("mass-energy", "internal-mass-unit", "mass-energy", "internal-mass-unit"),
            MakeProvenance());

        Assert.Equal("wz-canonical-operator-normalization-derived", result.TerminalStatus);
        Assert.Equal(1.0, result.DimensionlessWzScale);
        Assert.True(result.TargetIndependent);
        Assert.False(result.ProxyOnly);
        Assert.NotNull(result.DerivedCalibration);
        Assert.Contains("phase12-candidate-0", result.SourceCandidateIds);
        Assert.Contains("phase12-candidate-2", result.SourceCandidateIds);
    }

    [Fact]
    public void Derive_WithMismatchedUnits_IsBlocked()
    {
        var result = WzCanonicalOperatorNormalizationDeriver.Derive(
            MakeP31Json(),
            MakeCandidateSourcesJson("mass-energy", "internal-mass-unit", "mass-energy", "other-unit"),
            MakeProvenance());

        Assert.Equal("wz-canonical-operator-normalization-blocked", result.TerminalStatus);
        Assert.Null(result.DimensionlessWzScale);
        Assert.Contains(result.ClosureRequirements, r => r.Contains("internal operator unit", StringComparison.Ordinal));
    }

    private static string MakeP31Json()
        => GuJsonDefaults.Serialize(new WzNormalizationClosureDiagnosticResult
        {
            ResultId = "phase31-wz-normalization-closure-diagnostic-v1",
            SchemaVersion = "1.0.0",
            TerminalStatus = "wz-normalization-closure-blocked",
            AlgorithmId = WzNormalizationClosureDiagnostic.AlgorithmId,
            TargetObservableId = "physical-w-z-mass-ratio",
            SelectedPairId = "phase22-phase12-candidate-0/phase22-phase12-candidate-2",
            ComputedRatio = 0.864,
            TargetValue = 0.88136,
            RequiredScaleToTarget = 1.02,
            DeclaredScaleFactor = 1.0,
            DeclaredScaleUncertainty = 0.0,
            DeclaredScaleDelta = -0.02,
            DeclaredCalibration = null,
            SelectorVariationExplainsMiss = false,
            DerivationBackedScaleAvailable = false,
            NormalizationChangeAllowed = false,
            Diagnosis = [],
            ClosureRequirements = [],
            Provenance = MakeProvenance(),
        });

    private static string MakeCandidateSourcesJson(string wUnitFamily, string wUnit, string zUnitFamily, string zUnit)
        => $$"""
           {
             "tableId": "test",
             "schemaVersion": "1.0.0",
             "terminalStatus": "candidate-source-ready",
             "readySourceCount": 2,
             "candidateModeSources": [
               { "sourceId": "phase22-phase12-candidate-0", "sourceOrigin": "internal", "sourceArtifactKind": "table", "sourceArtifactPath": "test", "sourceObservableId": "phase22-phase12-candidate-0", "value": 8.0, "uncertainty": 0.1, "unitFamily": "{{wUnitFamily}}", "unit": "{{wUnit}}", "environmentId": "env", "branchId": "branch", "refinementLevel": "L0", "sourceExtractionMethod": "operator-method", "provenance": { "createdAt": "2026-04-26T00:00:00+00:00", "codeRevision": "test", "branch": { "branchId": "test", "schemaVersion": "1.0" }, "backend": "cpu" } },
               { "sourceId": "phase22-phase12-candidate-2", "sourceOrigin": "internal", "sourceArtifactKind": "table", "sourceArtifactPath": "test", "sourceObservableId": "phase22-phase12-candidate-2", "value": 10.0, "uncertainty": 0.1, "unitFamily": "{{zUnitFamily}}", "unit": "{{zUnit}}", "environmentId": "env", "branchId": "branch", "refinementLevel": "L0", "sourceExtractionMethod": "operator-method", "provenance": { "createdAt": "2026-04-26T00:00:00+00:00", "codeRevision": "test", "branch": { "branchId": "test", "schemaVersion": "1.0" }, "backend": "cpu" } }
             ]
           }
           """;

    private static ProvenanceMeta MakeProvenance() => new()
    {
        CreatedAt = DateTimeOffset.Parse("2026-04-26T00:00:00Z"),
        CodeRevision = "test",
        Branch = new BranchRef { BranchId = "phase33-wz-canonical-operator-normalization", SchemaVersion = "1.0" },
        Backend = "cpu",
    };
}
