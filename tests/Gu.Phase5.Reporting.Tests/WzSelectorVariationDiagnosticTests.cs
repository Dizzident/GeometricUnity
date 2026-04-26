using Gu.Core;
using Gu.Core.Serialization;
using Gu.Phase5.QuantitativeValidation;
using Gu.Phase5.Reporting;

namespace Gu.Phase5.Reporting.Tests;

public sealed class WzSelectorVariationDiagnosticTests : IDisposable
{
    private readonly string _tempDir = Path.Combine(Path.GetTempPath(), $"gu-p30-{Guid.NewGuid():N}");

    public WzSelectorVariationDiagnosticTests()
    {
        Directory.CreateDirectory(_tempDir);
    }

    [Fact]
    public void Evaluate_WithAlignedModeRecords_ComputesVariationEnvelope()
    {
        WriteMode("phase12-candidate-0", "b0", "L0", "env-a", 8.0);
        WriteMode("phase12-candidate-2", "b0", "L0", "env-a", 10.0);
        WriteMode("phase12-candidate-0", "b0", "L0", "env-b", 8.8);
        WriteMode("phase12-candidate-2", "b0", "L0", "env-b", 10.0);

        var result = WzSelectorVariationDiagnostic.Evaluate(
            MakeIdentityReadinessJson("identity-rule-ready"),
            _tempDir,
            MakeTargetTableJson(),
            MakeProvenance());

        Assert.Equal("selector-variation-diagnostic-complete", result.TerminalStatus);
        Assert.Equal(2, result.AlignedPointCount);
        Assert.Equal(0.8, result.RatioMin!.Value, precision: 12);
        Assert.Equal(0.88, result.RatioMax!.Value, precision: 12);
        Assert.True(result.TargetInsideObservedEnvelope);
        Assert.NotNull(result.ClosestPoint);
    }

    [Fact]
    public void Evaluate_WithBlockedIdentityReadiness_IsBlocked()
    {
        var result = WzSelectorVariationDiagnostic.Evaluate(
            MakeIdentityReadinessJson("identity-feature-blocked"),
            _tempDir,
            MakeTargetTableJson(),
            MakeProvenance());

        Assert.Equal("selector-variation-diagnostic-blocked", result.TerminalStatus);
        Assert.Contains(result.ClosureRequirements, r => r.Contains("identity-rule readiness", StringComparison.Ordinal));
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    private void WriteMode(string sourceCandidateId, string branch, string refinement, string environment, double value)
    {
        var record = new InternalVectorBosonSourceModeRecord
        {
            ModeRecordId = $"{sourceCandidateId}__{branch}__{refinement}__{environment}-mode",
            SourceCandidateId = sourceCandidateId,
            SourceFamilyId = null,
            BranchVariantId = branch,
            RefinementLevel = refinement,
            EnvironmentId = environment,
            MassLikeValue = value,
            ExtractionError = 0.01,
            GaugeLeakEnvelope = [0, 0, 0],
            SourceArtifactPaths = ["test"],
            Status = "computed",
            Blockers = [],
            Provenance = MakeProvenance(),
        };
        File.WriteAllText(
            Path.Combine(_tempDir, $"{sourceCandidateId}__{branch}__{refinement}__{environment}_mode.json"),
            GuJsonDefaults.Serialize(record));
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

    private static string MakeTargetTableJson()
        => """
           {
             "tableId": "targets",
             "targets": [
               {
                 "observableId": "physical-w-z-mass-ratio",
                 "label": "target",
                 "value": 0.85,
                 "uncertainty": 0.01,
                 "source": "test"
               }
             ]
           }
           """;

    private static ProvenanceMeta MakeProvenance() => new()
    {
        CreatedAt = DateTimeOffset.Parse("2026-04-26T00:00:00Z"),
        CodeRevision = "test",
        Branch = new BranchRef { BranchId = "phase30-wz-selector-variation-diagnostic", SchemaVersion = "1.0" },
        Backend = "cpu",
    };
}
