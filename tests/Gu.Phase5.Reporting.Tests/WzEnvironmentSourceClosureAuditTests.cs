using Gu.Core;
using Gu.Phase5.Reporting;

namespace Gu.Phase5.Reporting.Tests;

public sealed class WzEnvironmentSourceClosureAuditTests
{
    [Fact]
    public void Evaluate_ObservableBackedEnvironmentWithoutBackgroundRecord_BlocksClosure()
    {
        using var temp = new TempDirectory();
        var envPath = Path.Combine(temp.Path, "env-a.json");
        File.WriteAllText(envPath, """{ "environmentId": "env-a" }""");
        var bgPath = Path.Combine(temp.Path, "bg-other.json");
        File.WriteAllText(bgPath, """{ "recordId": "bg-other", "environmentId": "env-other" }""");

        var result = WzEnvironmentSourceClosureAudit.Evaluate(
            CampaignSpecJson,
            [envPath],
            ObservableJson,
            [bgPath],
            Provenance);

        Assert.Equal("environment-source-closure-blocked", result.TerminalStatus);
        Assert.Equal(1, result.EnvironmentCount);
        Assert.Equal(1, result.EnvironmentRecordCount);
        Assert.Equal(1, result.ObservableBackedCount);
        Assert.Equal(0, result.BackgroundBackedCount);
        Assert.Contains(result.SummaryBlockers, b => b.Contains("persisted solver-backed background records", StringComparison.Ordinal));
        var mapping = Assert.Single(result.EnvironmentMappings);
        Assert.True(mapping.EnvironmentRecordFound);
        Assert.True(mapping.ObservableBacked);
        Assert.False(mapping.BackgroundRecordFound);
    }

    [Fact]
    public void Evaluate_BackgroundRecordForEnvironment_CompletesClosure()
    {
        using var temp = new TempDirectory();
        var envPath = Path.Combine(temp.Path, "env-a.json");
        File.WriteAllText(envPath, """{ "environmentId": "env-a" }""");
        var bgPath = Path.Combine(temp.Path, "bg-a.json");
        File.WriteAllText(bgPath, """{ "recordId": "bg-a", "environmentId": "env-a" }""");

        var result = WzEnvironmentSourceClosureAudit.Evaluate(
            CampaignSpecJson,
            [envPath],
            ObservableJson,
            [bgPath],
            Provenance);

        Assert.Equal("environment-source-closure-complete", result.TerminalStatus);
        Assert.Equal(1, result.BackgroundBackedCount);
        Assert.Empty(result.SummaryBlockers);
    }

    private const string CampaignSpecJson = """
        {
          "campaignId": "campaign-a",
          "schemaVersion": "1.0",
          "sourceCandidateTablePath": "source_candidates.json",
          "readinessSpecPath": "source_readiness_campaign.json",
          "branchVariantIds": ["branch-a"],
          "refinementLevels": ["L0"],
          "environmentIds": ["env-a"],
          "sourceQuantityIds": ["massLikeValue", "environmentId"],
          "missingCellPolicy": "block-missing-cell",
          "identityScope": "test-scope",
          "provenance": {
            "createdAt": "2026-04-28T00:00:00+00:00",
            "codeRevision": "test",
            "branch": { "branchId": "test", "schemaVersion": "1.0" },
            "backend": "cpu"
          }
        }
        """;

    private const string ObservableJson = """
        [
          {
            "recordId": "obs-a",
            "observableId": "bosonic-eigenvalue-ratio-1",
            "environmentId": "env-a"
          }
        ]
        """;

    private static readonly ProvenanceMeta Provenance = new()
    {
        CreatedAt = DateTimeOffset.Parse("2026-04-28T00:00:00+00:00"),
        CodeRevision = "test",
        Branch = new BranchRef { BranchId = "test", SchemaVersion = "1.0" },
        Backend = "cpu",
    };

    private sealed class TempDirectory : IDisposable
    {
        public string Path { get; } = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            "gu-p38-" + Guid.NewGuid().ToString("N"));

        public TempDirectory() => Directory.CreateDirectory(Path);

        public void Dispose()
        {
            if (Directory.Exists(Path))
                Directory.Delete(Path, recursive: true);
        }
    }
}
