using Gu.Core;
using Gu.Phase5.BranchIndependence;
using Gu.Phase5.Convergence;
using Gu.Phase5.Reporting;
using Phase3Reporting = Gu.Phase3.Reporting;

namespace Gu.Phase5.Reporting.Tests;

public sealed class Phase5ReportGeneratorTests
{
    private static ProvenanceMeta MakeProvenance() => new ProvenanceMeta
    {
        CreatedAt = DateTimeOffset.UtcNow,
        CodeRevision = "abc123",
        Branch = new BranchRef { BranchId = "test-branch", SchemaVersion = "1.0" },
        Backend = "cpu",
    };

    [Fact]
    public void Generate_NoDossiers_ReturnsReportWithEmptyDossierIds()
    {
        var report = Phase5ReportGenerator.Generate("study-1", [], MakeProvenance());

        Assert.Equal("study-1", report.StudyId);
        Assert.Empty(report.DossierIds);
        Assert.Null(report.BranchIndependenceAtlas);
        Assert.Null(report.ConvergenceAtlas);
        Assert.Null(report.FalsificationDashboard);
    }

    [Fact]
    public void Generate_WithBranchRecord_IncludesBranchAtlas()
    {
        var spec = new BranchRobustnessStudySpec
        {
            StudyId = "branch-study",
            BranchVariantIds = ["V1", "V2"],
            TargetQuantityIds = ["q1"],
        };
        var engine = new BranchRobustnessEngine(spec);
        var branchRecord = engine.Run(
            new Dictionary<string, double[]> { ["q1"] = [1.0, 1.001] },
            MakeProvenance());

        var report = Phase5ReportGenerator.Generate(
            "study-1", [], MakeProvenance(),
            branchRecord: branchRecord);

        Assert.NotNull(report.BranchIndependenceAtlas);
        Assert.Equal(1, report.BranchIndependenceAtlas!.TotalQuantities);
    }

    [Fact]
    public void Generate_WithConvergenceResult_IncludesConvergenceAtlas()
    {
        var spec = new RefinementStudySpec
        {
            StudyId = "refine-study",
            SchemaVersion = "1.0",
            BranchManifestId = "branch-1",
            TargetQuantities = ["q1"],
            RefinementLevels = [
                new RefinementLevel { LevelId = "L0", MeshParameter = 1.0 },
                new RefinementLevel { LevelId = "L1", MeshParameter = 0.5 },
                new RefinementLevel { LevelId = "L2", MeshParameter = 0.25 },
            ],
            Provenance = MakeProvenance(),
        };
        var runner = new RefinementStudyRunner();
        var result = runner.Run(spec, level =>
            new Dictionary<string, double> { ["q1"] = 3.0 + level.MeshParameter * level.MeshParameter });

        var report = Phase5ReportGenerator.Generate(
            "study-1", [], MakeProvenance(),
            refinementResult: result);

        Assert.NotNull(report.ConvergenceAtlas);
        Assert.Equal(1, report.ConvergenceAtlas!.TotalQuantities);
    }

    [Fact]
    public void ToMarkdown_ContainsStudyId()
    {
        var report = new Phase5Report
        {
            ReportId = "r-1",
            SchemaVersion = "1.0.0",
            StudyId = "my-study-xyz",
            DossierIds = [],
            Provenance = MakeProvenance(),
            GeneratedAt = DateTimeOffset.UtcNow,
        };

        var md = Phase5ReportGenerator.ToMarkdown(report);

        Assert.Contains("my-study-xyz", md);
        Assert.Contains("# Phase V Validation Report", md);
    }

    [Fact]
    public void ToMarkdown_ListsDossierIds()
    {
        var report = new Phase5Report
        {
            ReportId = "r-2",
            SchemaVersion = "1.0.0",
            StudyId = "study-2",
            DossierIds = ["dossier-alpha", "dossier-beta"],
            Provenance = MakeProvenance(),
            GeneratedAt = DateTimeOffset.UtcNow,
        };

        var md = Phase5ReportGenerator.ToMarkdown(report);

        Assert.Contains("dossier-alpha", md);
        Assert.Contains("dossier-beta", md);
    }

    [Fact]
    public void ToMarkdown_WithNegativeResults_ListsThem()
    {
        var report = new Phase5Report
        {
            ReportId = "r-3",
            SchemaVersion = "1.0.0",
            StudyId = "study-3",
            DossierIds = [],
            NegativeResultSummary =
            [
                new Phase3Reporting.NegativeResultEntry
                {
                    Description = "q2 diverged",
                    Evidence = "convergence failure",
                    Impact = "low",
                },
            ],
            Provenance = MakeProvenance(),
            GeneratedAt = DateTimeOffset.UtcNow,
        };

        var md = Phase5ReportGenerator.ToMarkdown(report);

        Assert.Contains("Negative Results", md);
        Assert.Contains("q2 diverged", md);
    }

    [Fact]
    public void ToMarkdown_IncludesCodeRevision()
    {
        var report = new Phase5Report
        {
            ReportId = "r-4",
            SchemaVersion = "1.0.0",
            StudyId = "study-4",
            DossierIds = [],
            Provenance = MakeProvenance(),
            GeneratedAt = DateTimeOffset.UtcNow,
        };

        var md = Phase5ReportGenerator.ToMarkdown(report);
        Assert.Contains("abc123", md);
    }
}
