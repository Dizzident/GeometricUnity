using Gu.Phase2.Canonicity;
using Gu.Phase2.Semantics;

namespace Gu.Phase2.Canonicity.Tests;

public class CanonicityDocketTests
{
    [Fact]
    public void CreateOpen_HasOpenStatus()
    {
        var docket = CanonicityDocketBuilder.CreateOpen("shiab", "identity-shiab", "output-equivalence", "identity-shiab,covariant-shiab");
        Assert.Equal(DocketStatus.Open, docket.Status);
        Assert.Equal("shiab", docket.ObjectClass);
        Assert.Equal("identity-shiab", docket.ActiveRepresentative);
    }

    [Fact]
    public void CreateOpen_HasEmptyCollections()
    {
        var docket = CanonicityDocketBuilder.CreateOpen("shiab", "identity-shiab", "output-equivalence", "identity-shiab,covariant-shiab");
        Assert.Empty(docket.CurrentEvidence);
        Assert.Empty(docket.KnownCounterexamples);
        Assert.Empty(docket.PendingTheorems);
        Assert.Empty(docket.StudyReports);
        Assert.Empty(docket.DownstreamClaimsBlockedUntilClosure);
    }

    [Fact]
    public void CreateMinimumSet_CoversAllRequiredClasses()
    {
        var dockets = CanonicityDocketBuilder.CreateMinimumSet();
        Assert.Equal(6, dockets.Count);

        var classes = dockets.Select(d => d.ObjectClass).ToHashSet();
        Assert.Contains("A0", classes);
        Assert.Contains("torsion", classes);
        Assert.Contains("shiab", classes);
        Assert.Contains("observation-extraction", classes);
        Assert.Contains("regularity", classes);
        Assert.Contains("gauge-fixing", classes);
    }

    [Fact]
    public void CreateMinimumSet_AllOpen()
    {
        var dockets = CanonicityDocketBuilder.CreateMinimumSet();
        Assert.All(dockets, d => Assert.Equal(DocketStatus.Open, d.Status));
    }

    [Fact]
    public void CanConstructDocketWithEvidence()
    {
        var evidence = new CanonicityEvidenceRecord
        {
            EvidenceId = "ev-1",
            StudyId = "study-1",
            Verdict = "consistent",
            MaxObservedDeviation = 1e-8,
            Tolerance = 1e-6,
            Timestamp = DateTimeOffset.UtcNow,
        };

        var docket = new CanonicityDocket
        {
            ObjectClass = "shiab",
            ActiveRepresentative = "identity-shiab",
            EquivalenceRelationId = "output-equivalence",
            AdmissibleComparisonClass = "identity-shiab,covariant-shiab",
            DownstreamClaimsBlockedUntilClosure = new[] { "shiab-independent-observable" },
            CurrentEvidence = new[] { evidence },
            KnownCounterexamples = Array.Empty<string>(),
            PendingTheorems = new[] { "shiab-uniqueness-conjecture" },
            StudyReports = new[] { "study-001" },
            Status = DocketStatus.EvidenceAccumulating,
        };

        Assert.Single(docket.CurrentEvidence);
        Assert.Single(docket.DownstreamClaimsBlockedUntilClosure);
        Assert.Equal(DocketStatus.EvidenceAccumulating, docket.Status);
    }

    [Fact]
    public void RequiredObjectClasses_HasSixEntries()
    {
        Assert.Equal(6, CanonicityDocketBuilder.RequiredObjectClasses.Count);
    }

    [Fact]
    public void DocketStatus_HasFiveValues()
    {
        Assert.Equal(5, Enum.GetValues<DocketStatus>().Length);
    }

    [Fact]
    public void CreateOpen_EmptyObjectClass_Throws()
    {
        Assert.Throws<ArgumentException>(
            () => CanonicityDocketBuilder.CreateOpen("", "rep", "eq", "comp"));
    }

    [Fact]
    public void CreateOpen_EmptyEquivalenceRelationId_Throws()
    {
        Assert.Throws<ArgumentException>(
            () => CanonicityDocketBuilder.CreateOpen("shiab", "rep", "", "comp"));
    }

    [Fact]
    public void CreateOpen_SetsAllExplicitFields()
    {
        var docket = CanonicityDocketBuilder.CreateOpen(
            "shiab", "identity-shiab", "eq-1", "identity-shiab,covariant-shiab");
        Assert.Equal("eq-1", docket.EquivalenceRelationId);
        Assert.Equal("identity-shiab,covariant-shiab", docket.AdmissibleComparisonClass);
    }
}
