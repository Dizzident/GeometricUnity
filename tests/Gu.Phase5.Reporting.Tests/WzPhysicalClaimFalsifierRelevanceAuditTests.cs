using Gu.Core;
using Gu.Phase5.Reporting;

namespace Gu.Phase5.Reporting.Tests;

public sealed class WzPhysicalClaimFalsifierRelevanceAuditTests
{
    [Fact]
    public void Evaluate_Phase46GlobalSidecars_TargetClearButGlobalBlocked()
    {
        var result = WzPhysicalClaimFalsifierRelevanceAudit.Evaluate(
            MakeFalsifierSummaryJson(),
            MakeScorecardJson(passed: true),
            MakeSelectorVariationJson(passed: true),
            MakePhysicalModeRecordsJson(),
            MakeProvenance());

        Assert.Equal("wz-physical-claim-target-clear-global-sidecars-blocked", result.TerminalStatus);
        Assert.Equal("physical-w-z-mass-ratio", result.TargetObservableId);
        Assert.True(result.TargetComparisonPassed);
        Assert.True(result.SelectorVariationPassed);
        Assert.Equal(3, result.ActiveSevereFalsifierCount);
        Assert.Equal(0, result.TargetRelevantSevereFalsifierCount);
        Assert.Equal(3, result.GlobalSidecarSevereFalsifierCount);
        Assert.All(result.FalsifierAudits, r => Assert.Equal("global-sidecar", r.Relevance));
        Assert.Contains("phase12-candidate-0", result.SelectedSourceCandidateIds);
        Assert.Contains(result.ClosureRequirements, r => r.Contains("target-scoped physical-claim policy", StringComparison.Ordinal));
    }

    [Fact]
    public void Evaluate_TargetMatchingSevereFalsifier_BlocksTarget()
    {
        var result = WzPhysicalClaimFalsifierRelevanceAudit.Evaluate(
            MakeFalsifierSummaryJson(includeTargetFalsifier: true),
            MakeScorecardJson(passed: true),
            MakeSelectorVariationJson(passed: true),
            MakePhysicalModeRecordsJson(),
            MakeProvenance());

        Assert.Equal("wz-physical-claim-target-blocked", result.TerminalStatus);
        Assert.Equal(1, result.TargetRelevantSevereFalsifierCount);
        Assert.Contains(result.FalsifierAudits, r =>
            r.Relevance == "target-relevant" &&
            r.TargetId == "physical-w-z-mass-ratio");
        Assert.Contains(result.ClosureRequirements, r => r.Contains("directly target the W/Z observable", StringComparison.Ordinal));
    }

    [Fact]
    public void Evaluate_FailingSelectorVariation_KeepsBranchFragilityTargetRelevant()
    {
        var result = WzPhysicalClaimFalsifierRelevanceAudit.Evaluate(
            MakeFalsifierSummaryJson(),
            MakeScorecardJson(passed: true),
            MakeSelectorVariationJson(passed: false),
            MakePhysicalModeRecordsJson(),
            MakeProvenance());

        Assert.Equal("wz-physical-claim-target-blocked", result.TerminalStatus);
        Assert.Equal(2, result.TargetRelevantSevereFalsifierCount);
        Assert.Equal(1, result.GlobalSidecarSevereFalsifierCount);
    }

    private static string MakeFalsifierSummaryJson(bool includeTargetFalsifier = false)
    {
        var extra = includeTargetFalsifier
            ? """
              ,
                  {
                    "falsifierId": "falsifier-target",
                    "schemaVersion": "1.0.0",
                    "falsifierType": "quantitative-mismatch",
                    "severity": "high",
                    "targetId": "physical-w-z-mass-ratio",
                    "branchId": "b0",
                    "triggerValue": 6.0,
                    "threshold": 5.0,
                    "description": "target mismatch",
                    "evidence": "scorecard",
                    "active": true,
                    "provenance": {
                      "createdAt": "2026-04-28T00:00:00+00:00",
                      "codeRevision": "test",
                      "branch": {"branchId": "test", "schemaVersion": "1.0"},
                      "backend": "cpu"
                    }
                  }
              """
            : "";

        return $$"""
            {
              "studyId": "test",
              "schemaVersion": "1.0.0",
              "activeFatalCount": 1,
              "activeHighCount": {{(includeTargetFalsifier ? 3 : 2)}},
              "totalActiveCount": {{(includeTargetFalsifier ? 4 : 3)}},
              "falsifiers": [
                {
                  "falsifierId": "falsifier-0001",
                  "schemaVersion": "1.0.0",
                  "falsifierType": "branch-fragility",
                  "severity": "high",
                  "targetId": "gauge-violation",
                  "branchId": "phase5-branch-family",
                  "triggerValue": 1.83,
                  "threshold": 0.5,
                  "description": "branch fragility",
                  "evidence": "branch",
                  "active": true,
                  "provenance": {
                    "createdAt": "2026-04-28T00:00:00+00:00",
                    "codeRevision": "test",
                    "branch": {"branchId": "test", "schemaVersion": "1.0"},
                    "backend": "cpu"
                  }
                },
                {
                  "falsifierId": "falsifier-0002",
                  "schemaVersion": "1.0.0",
                  "falsifierType": "branch-fragility",
                  "severity": "high",
                  "targetId": "solver-iterations",
                  "branchId": "phase5-branch-family",
                  "triggerValue": 2.0,
                  "threshold": 0.5,
                  "description": "branch fragility",
                  "evidence": "branch",
                  "active": true,
                  "provenance": {
                    "createdAt": "2026-04-28T00:00:00+00:00",
                    "codeRevision": "test",
                    "branch": {"branchId": "test", "schemaVersion": "1.0"},
                    "backend": "cpu"
                  }
                },
                {
                  "falsifierId": "falsifier-0003",
                  "schemaVersion": "1.0.0",
                  "falsifierType": "representation-content",
                  "severity": "fatal",
                  "targetId": "fermion-registry-phase4-toy-v1-0000",
                  "branchId": "phase5-branch-family",
                  "triggerValue": 1.0,
                  "threshold": 0.0,
                  "description": "missing representation content",
                  "evidence": "representation-content",
                  "active": true,
                  "provenance": {
                    "createdAt": "2026-04-28T00:00:00+00:00",
                    "codeRevision": "test",
                    "branch": {"branchId": "test", "schemaVersion": "1.0"},
                    "backend": "cpu"
                  }
                }{{extra}}
              ],
              "provenance": {
                "createdAt": "2026-04-28T00:00:00+00:00",
                "codeRevision": "test",
                "branch": {"branchId": "test", "schemaVersion": "1.0"},
                "backend": "cpu"
              }
            }
            """;
    }

    private static string MakeScorecardJson(bool passed)
        => $$"""
            {
              "matches": [
                {
                  "observableId": "physical-w-z-mass-ratio",
                  "passed": {{passed.ToString().ToLowerInvariant()}}
                }
              ]
            }
            """;

    private static string MakeSelectorVariationJson(bool passed)
        => $$"""
            {
              "terminalStatus": "selector-variation-diagnostic-complete",
              "alignedPointCount": 36,
              "passingPointCount": {{(passed ? 36 : 35)}},
              "targetInsideObservedEnvelope": {{passed.ToString().ToLowerInvariant()}}
            }
            """;

    private static string MakePhysicalModeRecordsJson()
        => """
            [
              {
                "modeId": "phase22-phase12-candidate-0",
                "particleId": "w-boson",
                "status": "validated"
              },
              {
                "modeId": "phase22-phase12-candidate-2",
                "particleId": "z-boson",
                "status": "validated"
              }
            ]
            """;

    private static ProvenanceMeta MakeProvenance()
        => new()
        {
            CreatedAt = DateTimeOffset.Parse("2026-04-28T00:00:00+00:00"),
            CodeRevision = "test",
            Branch = new BranchRef { BranchId = "test", SchemaVersion = "1.0" },
            Backend = "cpu",
        };
}
