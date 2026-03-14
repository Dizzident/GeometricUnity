using System.Text.Json;
using Gu.Core;
using Gu.Phase3.Reporting;
using Gu.Phase5.Reporting;

namespace Gu.Phase5.Reporting.Tests;

public sealed class Phase5ReportTests
{
    private static ProvenanceMeta MakeProvenance() => new ProvenanceMeta
    {
        CreatedAt = DateTimeOffset.UtcNow,
        CodeRevision = "test-sha",
        Branch = new BranchRef { BranchId = "test-branch", SchemaVersion = "1.0" },
        Backend = "cpu",
    };

    [Fact]
    public void Phase5Report_JsonRoundTrip_PreservesAllFields()
    {
        var report = new Phase5Report
        {
            ReportId = "report-001",
            SchemaVersion = "1.0.0",
            StudyId = "study-001",
            DossierIds = ["dossier-A", "dossier-B"],
            BranchIndependenceAtlas = new BranchIndependenceAtlas
            {
                TotalQuantities = 3,
                InvariantCount = 2,
                FragileCount = 1,
                EquivalenceClassCount = 2,
                SummaryLines = ["- 2 invariant, 1 fragile"],
            },
            ConvergenceAtlas = new ConvergenceAtlas
            {
                TotalQuantities = 2,
                ConvergentCount = 1,
                NonConvergentCount = 1,
                InsufficientDataCount = 0,
                SummaryLines = ["- 1 convergent"],
            },
            FalsificationDashboard = new FalsificationDashboard
            {
                TotalFalsifiers = 5,
                ActiveFatalCount = 0,
                ActiveHighCount = 1,
                PromotionCount = 0,
                DemotionCount = 1,
                SummaryLines = ["- 1 active high falsifier"],
            },
            NegativeResultSummary =
            [
                new NegativeResultEntry { Description = "q1 did not converge", Evidence = "ConvergenceFailureRecord", Impact = "medium" },
            ],
            Provenance = MakeProvenance(),
            GeneratedAt = DateTimeOffset.UtcNow,
        };

        var json = report.ToJson();
        var roundTripped = Phase5Report.FromJson(json);

        Assert.Equal(report.ReportId, roundTripped.ReportId);
        Assert.Equal(report.StudyId, roundTripped.StudyId);
        Assert.Equal(2, roundTripped.DossierIds.Count);
        Assert.NotNull(roundTripped.BranchIndependenceAtlas);
        Assert.Equal(3, roundTripped.BranchIndependenceAtlas!.TotalQuantities);
        Assert.NotNull(roundTripped.ConvergenceAtlas);
        Assert.Equal(2, roundTripped.ConvergenceAtlas!.TotalQuantities);
        Assert.NotNull(roundTripped.FalsificationDashboard);
        Assert.Equal(5, roundTripped.FalsificationDashboard!.TotalFalsifiers);
        Assert.Single(roundTripped.NegativeResultSummary!);
    }

    [Fact]
    public void Phase5Report_NullOptionalFields_Serializes()
    {
        var report = new Phase5Report
        {
            ReportId = "report-minimal",
            SchemaVersion = "1.0.0",
            StudyId = "study-minimal",
            DossierIds = [],
            Provenance = MakeProvenance(),
            GeneratedAt = DateTimeOffset.UtcNow,
        };

        var json = report.ToJson();
        Assert.NotEmpty(json);

        var doc = JsonDocument.Parse(json);
        Assert.True(doc.RootElement.TryGetProperty("reportId", out _));
    }

    [Fact]
    public void Phase5Report_SchemaVersionIsPreserved()
    {
        var report = new Phase5Report
        {
            ReportId = "r1",
            SchemaVersion = "2.0.0",
            StudyId = "s1",
            DossierIds = [],
            Provenance = MakeProvenance(),
            GeneratedAt = DateTimeOffset.UtcNow,
        };

        var json = report.ToJson();
        var rt = Phase5Report.FromJson(json);
        Assert.Equal("2.0.0", rt.SchemaVersion);
    }
}
