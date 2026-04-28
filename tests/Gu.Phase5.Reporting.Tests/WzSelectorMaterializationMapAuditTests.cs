using Gu.Core;
using Gu.Phase5.Reporting;

namespace Gu.Phase5.Reporting.Tests;

public sealed class WzSelectorMaterializationMapAuditTests
{
    [Fact]
    public void Evaluate_BranchAndRefinementMappedButEnvironmentDeclaredOnly_BlocksMap()
    {
        var result = WzSelectorMaterializationMapAudit.Evaluate(
            CampaignSpecJson,
            BridgeManifestJson,
            RefinementManifestJson,
            EnvironmentCampaignJson,
            MakeProvenance());

        Assert.Equal("selector-materialization-map-blocked", result.TerminalStatus);
        Assert.Equal(1, result.MappedBranchVariantCount);
        Assert.Equal(1, result.MappedRefinementLevelCount);
        Assert.Equal(0, result.MappedEnvironmentCount);
        Assert.Contains(result.SummaryBlockers, b => b.Contains("environment", StringComparison.Ordinal));
    }

    private const string CampaignSpecJson = """
        {
          "campaignId": "test",
          "schemaVersion": "1.0.0",
          "sourceCandidateTablePath": "source_candidates.json",
          "readinessSpecPath": "readiness.json",
          "branchVariantIds": ["bg-variant-a"],
          "refinementLevels": ["L0"],
          "environmentIds": ["env-a"],
          "sourceQuantityIds": ["massLikeValue"],
          "missingCellPolicy": "block-missing-cell",
          "identityScope": "identity-neutral-vector-boson-source-candidates",
          "provenance": {
            "createdAt": "2026-04-28T00:00:00+00:00",
            "codeRevision": "test",
            "branch": { "branchId": "test", "schemaVersion": "1.0" },
            "backend": "cpu"
          }
        }
        """;

    private const string BridgeManifestJson = """
        {
          "derivedVariantIds": ["bg-variant-a"],
          "sourceRecordIds": ["bg-a"],
          "sourceStateArtifactRefs": ["bg-a_omega.json"]
        }
        """;

    private const string RefinementManifestJson = """
        {
          "evidenceSource": "direct-solver-backed",
          "sourceRecordIds": ["bg-l0"],
          "sourceArtifactRefs": ["bg-l0.json"]
        }
        """;

    private const string EnvironmentCampaignJson = """
        {
          "environmentIds": ["env-a"]
        }
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
