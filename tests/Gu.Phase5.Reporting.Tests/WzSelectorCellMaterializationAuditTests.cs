using Gu.Core;
using Gu.Phase5.Reporting;

namespace Gu.Phase5.Reporting.Tests;

public sealed class WzSelectorCellMaterializationAuditTests : IDisposable
{
    private readonly string _tempDir = Path.Combine(Path.GetTempPath(), $"gu-p36-{Guid.NewGuid():N}");

    public WzSelectorCellMaterializationAuditTests()
    {
        Directory.CreateDirectory(_tempDir);
    }

    [Fact]
    public void Evaluate_MissingSelectorArtifacts_BlocksMaterialization()
    {
        var result = WzSelectorCellMaterializationAudit.Evaluate(
            MakeCampaignSpecJson(),
            MakeSourceCandidatesJson(),
            [_tempDir],
            MakeProvenance());

        Assert.Equal("selector-cell-materialization-blocked", result.TerminalStatus);
        Assert.Equal(1, result.TotalCellCount);
        Assert.Equal(0, result.MaterializedCellCount);
        Assert.Contains("backgroundRecord", result.CellAudits[0].MissingInputs);
    }

    [Fact]
    public void Evaluate_CompleteSelectorArtifacts_MaterializesCell()
    {
        WriteCompleteSelectorArtifacts();

        var result = WzSelectorCellMaterializationAudit.Evaluate(
            MakeCampaignSpecJson(),
            MakeSourceCandidatesJson(),
            [_tempDir],
            MakeProvenance());

        Assert.Equal("selector-cells-materialized", result.TerminalStatus);
        Assert.Equal(1, result.TotalCellCount);
        Assert.Equal(1, result.MaterializedCellCount);
        Assert.Empty(result.CellAudits[0].MissingInputs);
        Assert.NotNull(result.CellAudits[0].BackgroundRecordPath);
        Assert.NotNull(result.CellAudits[0].BranchManifestPath);
        Assert.NotNull(result.CellAudits[0].OmegaStatePath);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    private void WriteCompleteSelectorArtifacts()
    {
        var root = Path.Combine(_tempDir, "cell");
        Directory.CreateDirectory(root);
        File.WriteAllText(Path.Combine(root, "bg-cell-1.json"),
            """
            {
              "backgroundId": "bg-cell-1",
              "environmentId": "env-a",
              "branchVariantId": "branch-a",
              "refinementLevel": "L0",
              "branchManifestId": "branch-a",
              "stateArtifactRef": "bg-cell-1_omega.json"
            }
            """);
        File.WriteAllText(Path.Combine(root, "bg-cell-1_manifest.json"), """{ "branchId": "branch-a" }""");
        File.WriteAllText(Path.Combine(root, "bg-cell-1_omega.json"), """{ "label": "omega" }""");
        File.WriteAllText(Path.Combine(root, "a0.json"), """{ "label": "a0" }""");
        File.WriteAllText(Path.Combine(root, "geometry.json"), """{ "geometry": "test" }""");
        File.WriteAllText(Path.Combine(root, "environment.json"), """{ "environmentId": "env-a" }""");
    }

    private static string MakeCampaignSpecJson()
        => """
           {
             "campaignId": "test",
             "schemaVersion": "1.0.0",
             "sourceCandidateTablePath": "source_candidates.json",
             "readinessSpecPath": "readiness.json",
             "branchVariantIds": ["branch-a"],
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

    private static string MakeSourceCandidatesJson()
        => """
           {
             "tableId": "test",
             "schemaVersion": "1.0.0",
             "terminalStatus": "candidate-source-ready",
             "summaryBlockers": [],
             "candidates": [
               {
                 "sourceCandidateId": "phase12-candidate-0",
                 "sourceOrigin": "internal-computed-artifact",
                 "modeRole": "vector-boson-source-candidate",
                 "sourceArtifactPaths": ["source.json"],
                 "sourceModeIds": ["mode-a"],
                 "sourceFamilyId": "family-a",
                 "massLikeValue": 1.0,
                 "uncertainty": { "extractionError": 0.1 },
                 "branchSelectors": ["branch-a"],
                 "environmentSelectors": ["env-a"],
                 "refinementLevels": ["L0"],
                 "branchStabilityScore": 1,
                 "refinementStabilityScore": 1,
                 "backendStabilityScore": 1,
                 "observationStabilityScore": 1,
                 "ambiguityCount": 0,
                 "gaugeLeakEnvelope": [0, 0, 0],
                 "claimClass": "C2_BranchStableCandidate",
                 "status": "candidate-source-ready",
                 "assumptions": [],
                 "closureRequirements": [],
                 "provenance": {
                   "createdAt": "2026-04-28T00:00:00+00:00",
                   "codeRevision": "test",
                   "branch": { "branchId": "test", "schemaVersion": "1.0" },
                   "backend": "cpu"
                 }
               }
             ],
             "provenance": {
               "createdAt": "2026-04-28T00:00:00+00:00",
               "codeRevision": "test",
               "branch": { "branchId": "test", "schemaVersion": "1.0" },
               "backend": "cpu"
             }
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
