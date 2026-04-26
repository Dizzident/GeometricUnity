using Gu.Core;
using Gu.Core.Serialization;
using Gu.Phase5.Reporting;

namespace Gu.Phase5.Reporting.Tests;

public sealed class WzOperatorNormalizationSourceAuditTests : IDisposable
{
    private readonly string _tempDir = Path.Combine(Path.GetTempPath(), $"gu-p32-{Guid.NewGuid():N}");

    public WzOperatorNormalizationSourceAuditTests()
    {
        Directory.CreateDirectory(_tempDir);
    }

    [Fact]
    public void Evaluate_WithProxyOnlyCouplingArtifact_DoesNotPromote()
    {
        File.WriteAllText(Path.Combine(_tempDir, "coupling_atlas.json"), """
            {
              "atlasId": "test-coupling-atlas",
              "couplings": [
                {
                  "bosonModeId": "candidate-0",
                  "couplingProxyMagnitude": 0.1,
                  "normalizationConvention": "unit-modes"
                },
                {
                  "bosonModeId": "candidate-2",
                  "couplingProxyMagnitude": 0.2,
                  "normalizationConvention": "unit-modes"
                }
              ]
            }
            """);

        var result = WzOperatorNormalizationSourceAudit.Evaluate(MakeP31Json(), [_tempDir], MakeProvenance());

        Assert.Equal("wz-operator-normalization-source-blocked", result.TerminalStatus);
        Assert.Equal(1, result.SourceCount);
        Assert.Equal(0, result.PromotableSourceCount);
        Assert.True(result.Sources[0].ProxyOnly);
        Assert.Contains(result.Sources[0].PromotionBlockers, b => b.Contains("proxy-only", StringComparison.Ordinal));
    }

    [Fact]
    public void Evaluate_WithOperatorDerivedScale_Promotes()
    {
        File.WriteAllText(Path.Combine(_tempDir, "operator_scale.json"), """
            {
              "sourceId": "synthetic-wz-scale",
              "sourceCandidateIds": ["phase12-candidate-0", "phase12-candidate-2"],
              "dimensionlessWzScale": 1.02,
              "operatorNormalizationDerivationId": "operator-normalization:test",
              "normalizationConvention": "unit-operator-wz-ratio"
            }
            """);

        var result = WzOperatorNormalizationSourceAudit.Evaluate(MakeP31Json(), [_tempDir], MakeProvenance());

        Assert.Equal("wz-operator-normalization-source-ready", result.TerminalStatus);
        Assert.Equal(1, result.PromotableSourceCount);
        Assert.NotNull(result.BestPromotableSource);
        Assert.Equal("promotable", result.BestPromotableSource!.PromotionStatus);
    }

    [Fact]
    public void Evaluate_WithNoArtifacts_IsBlocked()
    {
        var result = WzOperatorNormalizationSourceAudit.Evaluate(MakeP31Json(), [_tempDir], MakeProvenance());

        Assert.Equal("wz-operator-normalization-source-blocked", result.TerminalStatus);
        Assert.Contains(result.ClosureRequirements, r => r.Contains("no JSON", StringComparison.Ordinal));
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
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

    private static ProvenanceMeta MakeProvenance() => new()
    {
        CreatedAt = DateTimeOffset.Parse("2026-04-26T00:00:00Z"),
        CodeRevision = "test",
        Branch = new BranchRef { BranchId = "phase32-wz-operator-normalization-source-audit", SchemaVersion = "1.0" },
        Backend = "cpu",
    };
}
