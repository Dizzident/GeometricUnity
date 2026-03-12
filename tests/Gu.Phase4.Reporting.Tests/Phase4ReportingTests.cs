using Gu.Core;
using Gu.Phase4.Couplings;
using Gu.Phase4.Fermions;
using Gu.Phase4.Registry;
using Gu.Phase4.Reporting;
using FermionFamilyAtlas = Gu.Phase4.FamilyClustering.FermionFamilyAtlas;

namespace Gu.Phase4.Reporting.Tests;

// ---------------------------------------------------------------------------
// Shared helpers
// ---------------------------------------------------------------------------

internal static class TestHelpers
{
    internal static ProvenanceMeta MakeProvenance() => new()
    {
        CreatedAt = DateTimeOffset.UtcNow,
        CodeRevision = "test-rev-abc123",
        Branch = new BranchRef { BranchId = "test-branch", SchemaVersion = "1.0.0" },
    };

    internal static FermionModeFamily MakeFamily(
        string id,
        string chirality = "left",
        double branchScore = 0.8,
        string? conjugateId = null)
    {
        return new FermionModeFamily
        {
            FamilyId = id,
            MemberModeIds = new List<string> { $"mode-{id}-a", $"mode-{id}-b" },
            BackgroundIds = new List<string> { "bg-001" },
            BranchVariantIds = new List<string> { "branch-v1" },
            EigenvalueMagnitudeEnvelope = new[] { 0.8, 1.0, 1.2 },
            DominantChiralityProfile = chirality,
            HasConjugationPair = conjugateId != null,
            ConjugateFamilyId = conjugateId,
            BranchPersistenceScore = branchScore,
            RefinementPersistenceScore = 0.9,
            AverageGaugeLeakScore = 0.03,
            Provenance = MakeProvenance(),
        };
    }

    internal static FermionFamilyAtlas MakeAtlas(params FermionModeFamily[] families)
    {
        return new FermionFamilyAtlas
        {
            AtlasId = "atlas-test-001",
            BranchFamilyId = "branch-family-001",
            ContextIds = new List<string> { "ctx-v1" },
            BackgroundIds = new List<string> { "bg-001" },
            Families = families.ToList(),
            Provenance = MakeProvenance(),
        };
    }

    internal static CouplingAtlas MakeCouplingAtlas(params BosonFermionCouplingRecord[] couplings)
    {
        return new CouplingAtlas
        {
            AtlasId = "coupling-atlas-001",
            FermionBackgroundId = "bg-001",
            BosonRegistryVersion = "1.0.0",
            Couplings = couplings.ToList(),
            NormalizationConvention = "unit-mode-norms",
            Provenance = MakeProvenance(),
        };
    }

    internal static BosonFermionCouplingRecord MakeCoupling(
        string bosonId, string fermI, string fermJ, double magnitude)
    {
        return new BosonFermionCouplingRecord
        {
            CouplingId = $"c-{bosonId}-{fermI}-{fermJ}",
            BosonModeId = bosonId,
            FermionModeIdI = fermI,
            FermionModeIdJ = fermJ,
            CouplingProxyReal = magnitude,
            CouplingProxyMagnitude = magnitude,
            NormalizationConvention = "unit-mode-norms",
            Provenance = MakeProvenance(),
        };
    }

    internal static UnifiedParticleRegistry MakeRegistry(params UnifiedParticleRecord[] records)
    {
        var reg = new UnifiedParticleRegistry();
        foreach (var r in records) reg.Register(r);
        return reg;
    }

    internal static UnifiedParticleRecord MakeParticle(
        string id, UnifiedParticleType type, string claimClass = "C1_LocalPersistentMode")
    {
        return new UnifiedParticleRecord
        {
            ParticleId = id,
            ParticleType = type,
            PrimarySourceId = $"src-{id}",
            ContributingSourceIds = new List<string> { $"src-{id}" },
            BranchVariantSet = new List<string> { "v1" },
            BackgroundSet = new List<string> { "bg-001" },
            MassLikeEnvelope = new[] { 0.9, 1.0, 1.1 },
            ClaimClass = claimClass,
            RegistryVersion = "1.0.0",
            Provenance = MakeProvenance(),
        };
    }
}

// ---------------------------------------------------------------------------
// Data type construction tests
// ---------------------------------------------------------------------------

public sealed class Phase4ReportConstructionTests
{
    [Fact]
    public void Phase4Report_Construction_SetsAllFields()
    {
        var report = new Phase4Report
        {
            ReportId = "report-001",
            StudyId = "study-001",
            FermionAtlas = new FermionAtlasSummary
            {
                SummaryId = "atlas-sum-001",
                FamilySheets = new List<FermionFamilySheet>(),
                ChiralitySummaries = new List<ChiralitySummaryEntry>(),
                ConjugationSummaries = new List<ConjugationSummaryEntry>(),
                TotalFamilies = 0,
                StableFamilies = 0,
                AmbiguousFamilies = 0,
            },
            CouplingAtlas = new CouplingAtlasSummary
            {
                SummaryId = "coupling-sum-001",
                CouplingMatrices = new List<CouplingMatrixSummary>(),
                TotalCouplings = 0,
                NonzeroCouplings = 0,
                MaxCouplingMagnitude = 0.0,
            },
            RegistrySummary = new UnifiedRegistrySummary
            {
                SummaryId = "reg-sum-001",
                TotalBosons = 3,
                TotalFermions = 5,
                TotalInteractions = 2,
                ClaimClassCounts = new Dictionary<string, int> { ["C0"] = 4, ["C1"] = 6 },
                TopCandidates = new List<CandidateParticleSummary>(),
            },
            NegativeResults = new List<string>(),
            Provenance = TestHelpers.MakeProvenance(),
            GeneratedAt = DateTimeOffset.UtcNow,
        };

        Assert.Equal("report-001", report.ReportId);
        Assert.Equal("study-001", report.StudyId);
        Assert.Equal(3, report.RegistrySummary.TotalBosons);
        Assert.Equal(5, report.RegistrySummary.TotalFermions);
        Assert.Equal(2, report.RegistrySummary.TotalInteractions);
    }

    [Fact]
    public void FermionFamilySheet_Construction()
    {
        var sheet = new FermionFamilySheet
        {
            FamilyId = "family-001",
            MeanEigenvalue = 1.5,
            EigenvalueSpread = 0.2,
            MemberCount = 3,
            IsStable = true,
            ClaimClass = "C2_BranchStableCandidate",
            MemberModeIds = new List<string> { "mode-001", "mode-002", "mode-003" },
        };

        Assert.Equal("family-001", sheet.FamilyId);
        Assert.Equal(3, sheet.MemberCount);
        Assert.True(sheet.IsStable);
    }

    [Fact]
    public void ChiralitySummaryEntry_Construction()
    {
        var entry = new ChiralitySummaryEntry
        {
            FamilyId = "family-001",
            LeftProjection = 0.9,
            RightProjection = 0.1,
            LeakageNorm = 0.02,
            ChiralityType = "Y",
            ChiralityStatus = "definite-left",
        };

        Assert.Equal("definite-left", entry.ChiralityStatus);
        Assert.Equal("Y", entry.ChiralityType);
        Assert.Equal(0.9, entry.LeftProjection);
    }

    [Fact]
    public void ConjugationSummaryEntry_Construction()
    {
        var entry = new ConjugationSummaryEntry
        {
            FamilyId = "family-001",
            HasConjugatePair = true,
            PairedFamilyId = "family-002",
            PairingScore = 0.95,
        };

        Assert.True(entry.HasConjugatePair);
        Assert.Equal("family-002", entry.PairedFamilyId);
    }

    [Fact]
    public void CouplingMatrixSummary_Construction()
    {
        var summary = new CouplingMatrixSummary
        {
            BosonModeId = "boson-mode-001",
            FermionPairCount = 4,
            MaxEntry = 0.75,
            FrobeniusNorm = 1.2,
        };

        Assert.Equal("boson-mode-001", summary.BosonModeId);
        Assert.Equal(4, summary.FermionPairCount);
        Assert.Equal(0.75, summary.MaxEntry);
    }
}

// ---------------------------------------------------------------------------
// Phase4ReportGenerator tests
// ---------------------------------------------------------------------------

public sealed class Phase4ReportGeneratorTests
{
    [Fact]
    public void Generator_ProducesReport_WithCorrectIds()
    {
        var atlas = TestHelpers.MakeAtlas(TestHelpers.MakeFamily("f1"), TestHelpers.MakeFamily("f2"));
        var couplingAtlas = TestHelpers.MakeCouplingAtlas();
        var registry = TestHelpers.MakeRegistry(
            TestHelpers.MakeParticle("p1", UnifiedParticleType.Boson),
            TestHelpers.MakeParticle("p2", UnifiedParticleType.Fermion));

        var generator = new Phase4ReportGenerator();
        var report = generator.Generate("study-test", registry, atlas, couplingAtlas, TestHelpers.MakeProvenance());

        Assert.Equal("study-test", report.StudyId);
        Assert.StartsWith("phase4-report-study-test-", report.ReportId);
        Assert.NotNull(report.FermionAtlas);
        Assert.NotNull(report.CouplingAtlas);
        Assert.NotNull(report.RegistrySummary);
    }

    [Fact]
    public void Generator_FermionAtlasSummary_CorrectCounts()
    {
        var atlas = TestHelpers.MakeAtlas(
            TestHelpers.MakeFamily("f1", branchScore: 0.9),
            TestHelpers.MakeFamily("f2", branchScore: 0.3),
            TestHelpers.MakeFamily("f3", chirality: "right", branchScore: 0.8));
        var report = new Phase4ReportGenerator().Generate(
            "study-counts",
            TestHelpers.MakeRegistry(),
            atlas,
            TestHelpers.MakeCouplingAtlas(),
            TestHelpers.MakeProvenance());

        Assert.Equal(3, report.FermionAtlas.TotalFamilies);
        Assert.Equal(2, report.FermionAtlas.StableFamilies); // f1 and f3 >= 0.5
        Assert.Equal(3, report.FermionAtlas.FamilySheets.Count);
        Assert.Equal(3, report.FermionAtlas.ChiralitySummaries.Count);
        Assert.Equal(3, report.FermionAtlas.ConjugationSummaries.Count);
    }

    [Fact]
    public void Generator_ChiralitySummary_LeftDominant()
    {
        var atlas = TestHelpers.MakeAtlas(TestHelpers.MakeFamily("f1", chirality: "left"));
        var report = new Phase4ReportGenerator().Generate(
            "study-chiral",
            TestHelpers.MakeRegistry(),
            atlas,
            TestHelpers.MakeCouplingAtlas(),
            TestHelpers.MakeProvenance());

        var entry = Assert.Single(report.FermionAtlas.ChiralitySummaries);
        Assert.Equal("definite-left", entry.ChiralityStatus);
        Assert.Equal(0.9, entry.LeftProjection);
        Assert.Equal(0.1, entry.RightProjection);
    }

    [Fact]
    public void Generator_ChiralitySummary_TrivialForTrivialDimY()
    {
        var atlas = TestHelpers.MakeAtlas(TestHelpers.MakeFamily("f1", chirality: "trivial"));
        var report = new Phase4ReportGenerator().Generate(
            "study-trivial",
            TestHelpers.MakeRegistry(),
            atlas,
            TestHelpers.MakeCouplingAtlas(),
            TestHelpers.MakeProvenance());

        var entry = Assert.Single(report.FermionAtlas.ChiralitySummaries);
        Assert.Equal("trivial", entry.ChiralityStatus);
        Assert.Equal(0.5, entry.LeftProjection);
        Assert.Equal(0.5, entry.RightProjection);
    }

    [Fact]
    public void Generator_ConjugationSummary_PairedFamily()
    {
        var atlas = TestHelpers.MakeAtlas(
            TestHelpers.MakeFamily("f1", conjugateId: "f2"),
            TestHelpers.MakeFamily("f2", conjugateId: "f1"));
        var report = new Phase4ReportGenerator().Generate(
            "study-conj",
            TestHelpers.MakeRegistry(),
            atlas,
            TestHelpers.MakeCouplingAtlas(),
            TestHelpers.MakeProvenance());

        var f1Summary = report.FermionAtlas.ConjugationSummaries.First(e => e.FamilyId == "f1");
        Assert.True(f1Summary.HasConjugatePair);
        Assert.Equal("f2", f1Summary.PairedFamilyId);
        Assert.Equal(1.0, f1Summary.PairingScore);
    }

    [Fact]
    public void Generator_CouplingAtlasSummary_CorrectCounts()
    {
        var couplingAtlas = TestHelpers.MakeCouplingAtlas(
            TestHelpers.MakeCoupling("boson-001", "ferm-i", "ferm-j", 0.5),
            TestHelpers.MakeCoupling("boson-001", "ferm-i", "ferm-k", 1e-12), // below threshold
            TestHelpers.MakeCoupling("boson-002", "ferm-j", "ferm-k", 0.3));

        var report = new Phase4ReportGenerator().Generate(
            "study-coupling",
            TestHelpers.MakeRegistry(),
            TestHelpers.MakeAtlas(),
            couplingAtlas,
            TestHelpers.MakeProvenance());

        Assert.Equal(3, report.CouplingAtlas.TotalCouplings);
        Assert.Equal(2, report.CouplingAtlas.NonzeroCouplings);
        Assert.Equal(0.5, report.CouplingAtlas.MaxCouplingMagnitude);
        Assert.Equal(2, report.CouplingAtlas.CouplingMatrices.Count);
    }

    [Fact]
    public void Generator_UnifiedRegistrySummary_TypeCounts()
    {
        var registry = TestHelpers.MakeRegistry(
            TestHelpers.MakeParticle("b1", UnifiedParticleType.Boson),
            TestHelpers.MakeParticle("b2", UnifiedParticleType.Boson),
            TestHelpers.MakeParticle("f1", UnifiedParticleType.Fermion),
            TestHelpers.MakeParticle("i1", UnifiedParticleType.Interaction));

        var report = new Phase4ReportGenerator().Generate(
            "study-reg",
            registry,
            TestHelpers.MakeAtlas(),
            TestHelpers.MakeCouplingAtlas(),
            TestHelpers.MakeProvenance());

        Assert.Equal(2, report.RegistrySummary.TotalBosons);
        Assert.Equal(1, report.RegistrySummary.TotalFermions);
        Assert.Equal(1, report.RegistrySummary.TotalInteractions);
    }

    [Fact]
    public void Generator_TopCandidates_OrderedByClaimClass()
    {
        var registry = TestHelpers.MakeRegistry(
            TestHelpers.MakeParticle("p-low", UnifiedParticleType.Fermion, "C0_NumericalMode"),
            TestHelpers.MakeParticle("p-high", UnifiedParticleType.Boson, "C3_ObservedStableCandidate"),
            TestHelpers.MakeParticle("p-mid", UnifiedParticleType.Fermion, "C1_LocalPersistentMode"));

        var report = new Phase4ReportGenerator().Generate(
            "study-top",
            registry,
            TestHelpers.MakeAtlas(),
            TestHelpers.MakeCouplingAtlas(),
            TestHelpers.MakeProvenance());

        Assert.Equal("p-high", report.RegistrySummary.TopCandidates[0].CandidateId);
    }

    [Fact]
    public void Generator_NegativeResults_IncludesDemotions()
    {
        var demotedParticle = new UnifiedParticleRecord
        {
            ParticleId = "p-demoted",
            ParticleType = UnifiedParticleType.Fermion,
            PrimarySourceId = "src-demoted",
            ContributingSourceIds = new List<string> { "src-demoted" },
            BranchVariantSet = new List<string> { "v1" },
            BackgroundSet = new List<string> { "bg-001" },
            MassLikeEnvelope = new[] { 0.9, 1.0, 1.1 },
            ClaimClass = "C0_NumericalMode",
            RegistryVersion = "1.0.0",
            Provenance = TestHelpers.MakeProvenance(),
            Demotions = new List<ParticleClaimDemotion>
            {
                new()
                {
                    Reason = "GaugeLeak",
                    FromClaimClass = "C1_LocalPersistentMode",
                    ToClaimClass = "C0_NumericalMode",
                    Details = "Gauge leak score 0.25 exceeded threshold 0.1",
                },
            },
        };

        var registry = TestHelpers.MakeRegistry(demotedParticle);
        var report = new Phase4ReportGenerator().Generate(
            "study-neg",
            registry,
            TestHelpers.MakeAtlas(),
            TestHelpers.MakeCouplingAtlas(),
            TestHelpers.MakeProvenance());

        Assert.Single(report.NegativeResults);
        Assert.Contains("GaugeLeak", report.NegativeResults[0]);
        Assert.Contains("p-demoted", report.NegativeResults[0]);
    }

    [Fact]
    public void Generator_NegativeResults_IncludesAmbiguityNotes()
    {
        var ambiguousParticle = new UnifiedParticleRecord
        {
            ParticleId = "p-ambiguous",
            ParticleType = UnifiedParticleType.Fermion,
            PrimarySourceId = "src-amb",
            ContributingSourceIds = new List<string> { "src-amb" },
            BranchVariantSet = new List<string> { "v1" },
            BackgroundSet = new List<string> { "bg-001" },
            MassLikeEnvelope = new[] { 0.9, 1.0, 1.1 },
            ClaimClass = "C1_LocalPersistentMode",
            RegistryVersion = "1.0.0",
            Provenance = TestHelpers.MakeProvenance(),
            AmbiguityNotes = new List<string>
            {
                "Possible match with cluster-2",
                "Eigenvalue near-degenerate",
            },
        };

        var registry = TestHelpers.MakeRegistry(ambiguousParticle);
        var report = new Phase4ReportGenerator().Generate(
            "study-amb",
            registry,
            TestHelpers.MakeAtlas(),
            TestHelpers.MakeCouplingAtlas(),
            TestHelpers.MakeProvenance());

        Assert.Equal(2, report.NegativeResults.Count);
        Assert.All(report.NegativeResults, r => Assert.Contains("p-ambiguous", r));
    }

    [Fact]
    public void Generator_EmptyInputs_ProducesValidReport()
    {
        var report = new Phase4ReportGenerator().Generate(
            "study-empty",
            TestHelpers.MakeRegistry(),
            TestHelpers.MakeAtlas(),
            TestHelpers.MakeCouplingAtlas(),
            TestHelpers.MakeProvenance());

        Assert.Equal("study-empty", report.StudyId);
        Assert.Equal(0, report.FermionAtlas.TotalFamilies);
        Assert.Equal(0, report.CouplingAtlas.TotalCouplings);
        Assert.Equal(0, report.RegistrySummary.TotalBosons);
        Assert.Empty(report.NegativeResults);
    }
}

// ---------------------------------------------------------------------------
// JSON serialization round-trip tests
// ---------------------------------------------------------------------------

public sealed class Phase4ReportSerializerTests
{
    [Fact]
    public void Serializer_RoundTrip_PreservesReportId()
    {
        var atlas = TestHelpers.MakeAtlas(TestHelpers.MakeFamily("f1"));
        var report = new Phase4ReportGenerator().Generate(
            "study-ser",
            TestHelpers.MakeRegistry(TestHelpers.MakeParticle("p1", UnifiedParticleType.Boson)),
            atlas,
            TestHelpers.MakeCouplingAtlas(TestHelpers.MakeCoupling("b1", "fi", "fj", 0.4)),
            TestHelpers.MakeProvenance());

        string json = Phase4ReportSerializer.Serialize(report);
        var deserialized = Phase4ReportSerializer.Deserialize(json);

        Assert.Equal(report.ReportId, deserialized.ReportId);
        Assert.Equal(report.StudyId, deserialized.StudyId);
    }

    [Fact]
    public void Serializer_RoundTrip_PreservesFermionAtlasCounts()
    {
        var atlas = TestHelpers.MakeAtlas(TestHelpers.MakeFamily("f1"), TestHelpers.MakeFamily("f2"));
        var report = new Phase4ReportGenerator().Generate(
            "study-ser2",
            TestHelpers.MakeRegistry(),
            atlas,
            TestHelpers.MakeCouplingAtlas(),
            TestHelpers.MakeProvenance());

        string json = Phase4ReportSerializer.Serialize(report);
        var deserialized = Phase4ReportSerializer.Deserialize(json);

        Assert.Equal(report.FermionAtlas.TotalFamilies, deserialized.FermionAtlas.TotalFamilies);
        Assert.Equal(report.FermionAtlas.StableFamilies, deserialized.FermionAtlas.StableFamilies);
        Assert.Equal(report.FermionAtlas.FamilySheets.Count, deserialized.FermionAtlas.FamilySheets.Count);
    }

    [Fact]
    public void Serializer_RoundTrip_PreservesCouplingAtlas()
    {
        var couplingAtlas = TestHelpers.MakeCouplingAtlas(
            TestHelpers.MakeCoupling("b1", "fi", "fj", 0.7));
        var report = new Phase4ReportGenerator().Generate(
            "study-coupling-ser",
            TestHelpers.MakeRegistry(),
            TestHelpers.MakeAtlas(),
            couplingAtlas,
            TestHelpers.MakeProvenance());

        string json = Phase4ReportSerializer.Serialize(report);
        var deserialized = Phase4ReportSerializer.Deserialize(json);

        Assert.Equal(report.CouplingAtlas.TotalCouplings, deserialized.CouplingAtlas.TotalCouplings);
        Assert.Equal(report.CouplingAtlas.MaxCouplingMagnitude, deserialized.CouplingAtlas.MaxCouplingMagnitude);
    }

    [Fact]
    public void Serializer_RoundTrip_PreservesRegistrySummary()
    {
        var registry = TestHelpers.MakeRegistry(
            TestHelpers.MakeParticle("b1", UnifiedParticleType.Boson),
            TestHelpers.MakeParticle("f1", UnifiedParticleType.Fermion));
        var report = new Phase4ReportGenerator().Generate(
            "study-reg-ser",
            registry,
            TestHelpers.MakeAtlas(),
            TestHelpers.MakeCouplingAtlas(),
            TestHelpers.MakeProvenance());

        string json = Phase4ReportSerializer.Serialize(report);
        var deserialized = Phase4ReportSerializer.Deserialize(json);

        Assert.Equal(report.RegistrySummary.TotalBosons, deserialized.RegistrySummary.TotalBosons);
        Assert.Equal(report.RegistrySummary.TotalFermions, deserialized.RegistrySummary.TotalFermions);
    }

    [Fact]
    public void Serializer_RoundTrip_PreservesNegativeResults()
    {
        var demotedParticle = new UnifiedParticleRecord
        {
            ParticleId = "p-demoted",
            ParticleType = UnifiedParticleType.Fermion,
            PrimarySourceId = "src-demoted",
            ContributingSourceIds = new List<string> { "src-demoted" },
            BranchVariantSet = new List<string> { "v1" },
            BackgroundSet = new List<string> { "bg-001" },
            MassLikeEnvelope = new[] { 0.9, 1.0, 1.1 },
            ClaimClass = "C0",
            RegistryVersion = "1.0.0",
            Provenance = TestHelpers.MakeProvenance(),
            Demotions = new List<ParticleClaimDemotion>
            {
                new() { Reason = "Test", FromClaimClass = "C1", ToClaimClass = "C0", Details = "test demotion" },
            },
        };

        var registry = TestHelpers.MakeRegistry(demotedParticle);
        var report = new Phase4ReportGenerator().Generate(
            "study-neg-ser",
            registry,
            TestHelpers.MakeAtlas(),
            TestHelpers.MakeCouplingAtlas(),
            TestHelpers.MakeProvenance());

        string json = Phase4ReportSerializer.Serialize(report);
        var deserialized = Phase4ReportSerializer.Deserialize(json);

        Assert.Equal(report.NegativeResults.Count, deserialized.NegativeResults.Count);
    }

    [Fact]
    public void Serializer_ToJson_ProducesValidJson()
    {
        var report = new Phase4ReportGenerator().Generate(
            "study-json",
            TestHelpers.MakeRegistry(),
            TestHelpers.MakeAtlas(),
            TestHelpers.MakeCouplingAtlas(),
            TestHelpers.MakeProvenance());

        string json = Phase4ReportSerializer.Serialize(report);

        Assert.NotEmpty(json);
        Assert.Contains("reportId", json);
        Assert.Contains("studyId", json);
        Assert.Contains("fermionAtlas", json);
        Assert.Contains("couplingAtlas", json);
        Assert.Contains("registrySummary", json);
    }

    [Fact]
    public void Serializer_TryDeserialize_ReturnsNullForInvalidJson()
    {
        var result = Phase4ReportSerializer.TryDeserialize("not-valid-json");
        Assert.Null(result);
    }

    [Fact]
    public void Serializer_TryDeserialize_ReturnsNullForEmptyString()
    {
        var result = Phase4ReportSerializer.TryDeserialize("");
        Assert.Null(result);
    }

    [Fact]
    public void Report_ToJson_FromJson_RoundTrip()
    {
        // Also test the convenience methods on Phase4Report itself
        var report = new Phase4ReportGenerator().Generate(
            "study-direct",
            TestHelpers.MakeRegistry(TestHelpers.MakeParticle("p1", UnifiedParticleType.Boson)),
            TestHelpers.MakeAtlas(TestHelpers.MakeFamily("f1")),
            TestHelpers.MakeCouplingAtlas(),
            TestHelpers.MakeProvenance());

        string json = report.ToJson();
        var deserialized = Phase4Report.FromJson(json);

        Assert.Equal(report.ReportId, deserialized.ReportId);
        Assert.Equal(report.FermionAtlas.TotalFamilies, deserialized.FermionAtlas.TotalFamilies);
    }
}

// ---------------------------------------------------------------------------
// ReportSummaryFormatter tests
// ---------------------------------------------------------------------------

public sealed class ReportSummaryFormatterTests
{
    private static Phase4Report MakeReport(string studyId = "study-format")
    {
        var atlas = TestHelpers.MakeAtlas(
            TestHelpers.MakeFamily("f1", branchScore: 0.9),
            TestHelpers.MakeFamily("f2", chirality: "right", branchScore: 0.4));
        var couplingAtlas = TestHelpers.MakeCouplingAtlas(
            TestHelpers.MakeCoupling("b1", "fi", "fj", 0.5));
        var registry = TestHelpers.MakeRegistry(
            TestHelpers.MakeParticle("p1", UnifiedParticleType.Boson),
            TestHelpers.MakeParticle("f1", UnifiedParticleType.Fermion));

        return new Phase4ReportGenerator().Generate(
            studyId, registry, atlas, couplingAtlas, TestHelpers.MakeProvenance());
    }

    [Fact]
    public void Formatter_Format_ProducesNonEmptyOutput()
    {
        var formatter = new ReportSummaryFormatter();
        var report = MakeReport();
        string output = formatter.Format(report);
        Assert.NotEmpty(output);
    }

    [Fact]
    public void Formatter_Format_ContainsScopeDisclaimer()
    {
        var formatter = new ReportSummaryFormatter();
        var report = MakeReport();
        string output = formatter.Format(report);
        Assert.Contains(ReportSummaryFormatter.ScopeDisclaimer, output);
    }

    [Fact]
    public void Formatter_ScopeDisclaimer_ContainsBranchLocalNote()
    {
        Assert.Contains("branch-local computation", ReportSummaryFormatter.ScopeDisclaimer);
        Assert.Contains("additional validation steps", ReportSummaryFormatter.ScopeDisclaimer);
    }

    [Fact]
    public void Formatter_Format_ContainsStudyId()
    {
        var formatter = new ReportSummaryFormatter();
        var report = MakeReport("study-format-id-test");
        string output = formatter.Format(report);
        Assert.Contains("study-format-id-test", output);
    }

    [Fact]
    public void Formatter_Format_ContainsFamilyCounts()
    {
        var formatter = new ReportSummaryFormatter();
        var report = MakeReport();
        string output = formatter.Format(report);
        // Should mention total families (2) and section header
        Assert.Contains("Fermion", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("2", output);
    }

    [Fact]
    public void Formatter_Format_ContainsCouplingInfo()
    {
        var formatter = new ReportSummaryFormatter();
        var report = MakeReport();
        string output = formatter.Format(report);
        Assert.Contains("Coupling", output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Formatter_FormatOneLiner_ProducesNonEmptyOutput()
    {
        var formatter = new ReportSummaryFormatter();
        var report = MakeReport("study-one-liner");
        string oneLiner = formatter.FormatOneLiner(report);
        Assert.NotEmpty(oneLiner);
        Assert.Contains("study-one-liner", oneLiner);
    }

    [Fact]
    public void Formatter_FormatOneLiner_ContainsCounts()
    {
        var formatter = new ReportSummaryFormatter();
        var report = MakeReport();
        string oneLiner = formatter.FormatOneLiner(report);
        Assert.Contains("families=", oneLiner);
        Assert.Contains("couplings=", oneLiner);
        Assert.Contains("bosons=", oneLiner);
        Assert.Contains("fermions=", oneLiner);
    }

    [Fact]
    public void Formatter_Format_DiagnosticNotes_PropagateThroughNegativeResults()
    {
        // Build a report with a demotion, which becomes a negative result / diagnostic note
        var demotedParticle = new UnifiedParticleRecord
        {
            ParticleId = "p-diag",
            ParticleType = UnifiedParticleType.Fermion,
            PrimarySourceId = "src-diag",
            ContributingSourceIds = new List<string> { "src-diag" },
            BranchVariantSet = new List<string> { "v1" },
            BackgroundSet = new List<string> { "bg-001" },
            MassLikeEnvelope = new[] { 0.9, 1.0, 1.1 },
            ClaimClass = "C0",
            RegistryVersion = "1.0.0",
            Provenance = TestHelpers.MakeProvenance(),
            Demotions = new List<ParticleClaimDemotion>
            {
                new() { Reason = "DiagnosticTest", FromClaimClass = "C2", ToClaimClass = "C0", Details = "test" },
            },
        };

        var registry = TestHelpers.MakeRegistry(demotedParticle);
        var report = new Phase4ReportGenerator().Generate(
            "study-diag",
            registry,
            TestHelpers.MakeAtlas(),
            TestHelpers.MakeCouplingAtlas(),
            TestHelpers.MakeProvenance());

        var formatter = new ReportSummaryFormatter();
        string output = formatter.Format(report);

        Assert.Contains("DiagnosticTest", output);
        Assert.Contains("p-diag", output);
    }
}
