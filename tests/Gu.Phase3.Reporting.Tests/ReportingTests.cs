using System.Text.Json;
using System.Text.Json.Serialization;
using Gu.Phase3.Campaigns;
using Gu.Phase3.Registry;
using Gu.Phase3.Reporting;

namespace Gu.Phase3.Reporting.Tests;

public sealed class SpectrumSheetTests
{
    [Fact]
    public void SpectrumSheet_Construction()
    {
        var sheet = new SpectrumSheet
        {
            BackgroundId = "bg-1",
            Eigenvalues = [0.0, 0.5, 1.2, 3.4],
            Multiplicities = [2, 1, 1, 3],
            GaugeLeaks = [0.01, 0.02, 0.15, 0.005],
            ModeCount = 4,
            ConvergenceStatus = "converged",
        };

        Assert.Equal("bg-1", sheet.BackgroundId);
        Assert.Equal(4, sheet.ModeCount);
        Assert.Equal(4, sheet.Eigenvalues.Count);
        Assert.Equal("converged", sheet.ConvergenceStatus);
    }
}

public sealed class StabilitySummaryTests
{
    [Fact]
    public void StabilitySummary_Construction()
    {
        var summary = new StabilitySummary
        {
            CandidateId = "cand-1",
            BranchStability = 0.9,
            RefinementStability = 0.7,
            BackendStability = 0.95,
            ObservationStability = 0.85,
            CurrentClaimClass = BosonClaimClass.C3_ObservedStableCandidate,
            OverallAssessment = "fragile",
        };

        Assert.Equal("cand-1", summary.CandidateId);
        Assert.Equal(0.9, summary.BranchStability);
        Assert.Equal("fragile", summary.OverallAssessment);
    }
}

public sealed class AmbiguityMapEntryTests
{
    [Fact]
    public void AmbiguityMapEntry_Construction()
    {
        var entry = new AmbiguityMapEntry
        {
            CandidateId = "cand-1",
            AmbiguityType = "multiple-targets",
            Notes = ["Matches photon profile", "Also matches Z-like profile"],
            AlternativeCount = 2,
        };

        Assert.Equal("cand-1", entry.CandidateId);
        Assert.Equal(2, entry.Notes.Count);
        Assert.Equal("multiple-targets", entry.AmbiguityType);
        Assert.Equal(2, entry.AlternativeCount);
    }
}

public sealed class NegativeResultEntryTests
{
    [Fact]
    public void NegativeResultEntry_Construction()
    {
        var entry = new NegativeResultEntry
        {
            Description = "No massless spin-2 mode found across all backgrounds.",
            Evidence = "All modes with eigenvalue < 1e-6 have gauge leak > 0.3.",
            Impact = "No graviton-like candidate in current background set.",
            Category = "no-match",
        };

        Assert.Equal("no-match", entry.Category);
        Assert.Contains("graviton", entry.Impact);
    }
}

public sealed class BosonAtlasReportConstructionTests
{
    [Fact]
    public void BosonAtlasReport_Construction()
    {
        var report = new BosonAtlasReport
        {
            ReportId = "report-001",
            StudyId = "study-001",
            RegistryVersion = "1.0.0",
            SpectrumSheets = [],
            StabilitySummaries = [],
            AmbiguityEntries = [],
            NegativeResults = [],
            CampaignResults = [],
            TotalCandidates = 5,
            ClaimClassCounts = new Dictionary<string, int>
            {
                ["C0_NumericalMode"] = 2,
                ["C3_ObservedStableCandidate"] = 3,
            },
            GeneratedAt = DateTimeOffset.UtcNow,
        };

        Assert.Equal("report-001", report.ReportId);
        Assert.Equal(5, report.TotalCandidates);
        Assert.Equal(2, report.ClaimClassCounts["C0_NumericalMode"]);
    }
}

public sealed class ReportBuilderTests
{
    private static CandidateBosonRecord MakeCandidate(
        string id,
        double massMean = 1.0,
        int multMean = 1,
        double gaugeLeak = 0.05,
        BosonClaimClass claimClass = BosonClaimClass.C3_ObservedStableCandidate)
    {
        return new CandidateBosonRecord
        {
            CandidateId = id,
            PrimaryFamilyId = $"family-{id}",
            ContributingModeIds = [$"mode-{id}-1"],
            BackgroundSet = ["bg-1"],
            MassLikeEnvelope = [massMean * 0.9, massMean, massMean * 1.1],
            MultiplicityEnvelope = [multMean, multMean, multMean],
            GaugeLeakEnvelope = [gaugeLeak * 0.8, gaugeLeak, gaugeLeak * 1.2],
            BranchStabilityScore = 0.9,
            RefinementStabilityScore = 0.85,
            BackendStabilityScore = 0.95,
            ObservationStabilityScore = 0.9,
            ClaimClass = claimClass,
            RegistryVersion = "1.0.0",
        };
    }

    [Fact]
    public void ReportBuilder_ProducesReport_FromRegistry()
    {
        var registry = new BosonRegistry();
        registry.Register(MakeCandidate("cand-1"));
        registry.Register(MakeCandidate("cand-2", claimClass: BosonClaimClass.C0_NumericalMode));

        var builder = new ReportBuilder("study-001");
        var report = builder.Build(registry);

        Assert.Equal("study-001", report.StudyId);
        Assert.Equal(2, report.TotalCandidates);
        Assert.Equal(2, report.StabilitySummaries.Count);
    }

    [Fact]
    public void ReportBuilder_IncludesNegativeResults()
    {
        var registry = new BosonRegistry();
        var builder = new ReportBuilder("study-neg");
        builder.AddNegativeResult(new NegativeResultSummary
        {
            CandidateId = "N/A",
            ResultType = "no-match",
            Description = "No spin-2 candidate found.",
        });
        builder.AddNegativeResult(new NegativeResultSummary
        {
            CandidateId = "N/A",
            ResultType = "insufficient-evidence",
            Description = "Gauge leak pervasive in low modes.",
        });

        var report = builder.Build(registry);

        Assert.Equal(2, report.NegativeResults.Count);
        Assert.Contains(report.NegativeResults, n => n.Description.Contains("spin-2"));
    }

    [Fact]
    public void ReportBuilder_IncludesAmbiguityEntries()
    {
        var candidateWithAmbiguity = new CandidateBosonRecord
        {
            CandidateId = "cand-amb",
            PrimaryFamilyId = "family-amb",
            ContributingModeIds = ["mode-amb-1"],
            BackgroundSet = ["bg-1"],
            MassLikeEnvelope = [0.0, 0.0, 0.0],
            MultiplicityEnvelope = [2, 2, 2],
            GaugeLeakEnvelope = [0.008, 0.01, 0.012],
            BranchStabilityScore = 0.9,
            RefinementStabilityScore = 0.85,
            BackendStabilityScore = 0.95,
            ObservationStabilityScore = 0.9,
            ClaimClass = BosonClaimClass.C3_ObservedStableCandidate,
            RegistryVersion = "1.0.0",
            AmbiguityNotes = ["Matches photon profile", "Also matches Z-like profile"],
        };

        var registry = new BosonRegistry();
        registry.Register(candidateWithAmbiguity);

        var builder = new ReportBuilder("study-amb");
        var report = builder.Build(registry);

        Assert.Single(report.AmbiguityEntries);
        Assert.Equal("cand-amb", report.AmbiguityEntries[0].CandidateId);
        Assert.Equal("degenerate-candidates", report.AmbiguityEntries[0].AmbiguityType);
    }

    [Fact]
    public void Report_SerializationRoundtrip()
    {
        var registry = new BosonRegistry();
        registry.Register(MakeCandidate("cand-ser"));

        var builder = new ReportBuilder("study-ser");
        var report = builder.Build(registry);

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() },
        };

        string json = JsonSerializer.Serialize(report, options);
        var deserialized = JsonSerializer.Deserialize<BosonAtlasReport>(json, options);

        Assert.NotNull(deserialized);
        Assert.Equal(report.ReportId, deserialized.ReportId);
        Assert.Equal(report.StudyId, deserialized.StudyId);
        Assert.Equal(report.TotalCandidates, deserialized.TotalCandidates);
    }

    [Fact]
    public void ReportBuilder_EmptyInput_ProducesValidReport()
    {
        var registry = new BosonRegistry();
        var builder = new ReportBuilder("study-empty");
        var report = builder.Build(registry);

        Assert.Equal("study-empty", report.StudyId);
        Assert.Equal(0, report.TotalCandidates);
        Assert.Empty(report.StabilitySummaries);
        Assert.Empty(report.AmbiguityEntries);
        Assert.Empty(report.NegativeResults);
        Assert.Empty(report.CampaignResults);
    }
}
