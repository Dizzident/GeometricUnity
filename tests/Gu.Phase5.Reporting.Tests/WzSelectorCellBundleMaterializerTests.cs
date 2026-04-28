using Gu.Core;
using Gu.Phase5.Reporting;

namespace Gu.Phase5.Reporting.Tests;

public sealed class WzSelectorCellBundleMaterializerTests : IDisposable
{
    private readonly string _tempDir = Path.Combine(Path.GetTempPath(), $"gu-p40-{Guid.NewGuid():N}");

    public WzSelectorCellBundleMaterializerTests()
    {
        Directory.CreateDirectory(_tempDir);
    }

    [Fact]
    public void Materialize_WritesBackedCellAndSkipsUnbackedEnvironment()
    {
        var result = WzSelectorCellBundleMaterializer.Materialize(
            CampaignSpecJson,
            SelectorMapJson,
            EnvironmentClosureJson,
            _tempDir,
            Provenance);

        Assert.Equal("selector-cell-bundles-partial", result.TerminalStatus);
        Assert.Equal(2, result.ExpectedSelectorCellCount);
        Assert.Equal(1, result.WrittenBundleCount);
        Assert.Equal(1, result.SkippedBundleCount);
        Assert.Contains(result.SummaryBlockers, b => b.Contains("env-b", StringComparison.Ordinal));
        var written = Assert.Single(result.Bundles, b => b.Written);
        Assert.True(File.Exists(written.BackgroundRecordPath));
        Assert.True(File.Exists(Path.Combine(_tempDir, "selector_cell_bundle_manifest.json")));
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    private const string CampaignSpecJson = """
        {
          "campaignId": "campaign-a",
          "schemaVersion": "1.0.0",
          "sourceCandidateTablePath": "source_candidates.json",
          "readinessSpecPath": "readiness.json",
          "branchVariantIds": ["branch-a"],
          "refinementLevels": ["L0"],
          "environmentIds": ["env-a", "env-b"],
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

    private const string SelectorMapJson = """
        {
          "branchMappings": [
            {
              "selectorId": "branch-a",
              "mapped": true,
              "sourceRecordId": "bg-branch-a",
              "sourceArtifactRef": "branch-a_omega.json"
            }
          ],
          "refinementMappings": [
            {
              "selectorId": "L0",
              "mapped": true,
              "sourceRecordId": "bg-ref-l0",
              "sourceArtifactRef": "ref-l0.json"
            }
          ]
        }
        """;

    private const string EnvironmentClosureJson = """
        {
          "environmentMappings": [
            {
              "environmentId": "env-a",
              "environmentRecordPath": "env-a.json",
              "backgroundRecordFound": true,
              "backgroundRecordIds": ["bg-env-a"],
              "backgroundRecordPaths": ["background_records/bg-env-a.json"]
            },
            {
              "environmentId": "env-b",
              "environmentRecordPath": "env-b.json",
              "backgroundRecordFound": false,
              "backgroundRecordIds": [],
              "backgroundRecordPaths": []
            }
          ]
        }
        """;

    private static readonly ProvenanceMeta Provenance = new()
    {
        CreatedAt = DateTimeOffset.Parse("2026-04-28T00:00:00+00:00"),
        CodeRevision = "test",
        Branch = new BranchRef { BranchId = "test", SchemaVersion = "1.0" },
        Backend = "cpu",
    };
}
