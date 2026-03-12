using System.Text.Json;
using System.Text.Json.Serialization;
using Gu.Core;
using Gu.Phase4.FamilyClustering;
using Gu.Phase4.Fermions;
using Gu.Phase4.Registry;
using Gu.Phase4.Reporting;

namespace Gu.Phase4.Reporting.Tests;

/// <summary>
/// Tests for Gu.Phase4.Reporting: Phase4ReportBuilder, Phase4Report,
/// FermionAtlasSummary, CouplingAtlasSummary, UnifiedRegistrySummary.
/// </summary>
public class Phase4ReportTests
{
    // -------------------------------------------------------
    // Helpers
    // -------------------------------------------------------

    private static ProvenanceMeta TestProvenance() => new()
    {
        CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
        CodeRevision = "test-reporting",
        Branch = new BranchRef { BranchId = "test-branch", SchemaVersion = "1.0.0" },
        Backend = "cpu-reference",
    };

    private static FermionModeFamily MakeFamily(
        string id,
        string chirality = "left",
        double eigenMean = 1.0,
        double branchPersistence = 0.9,
        string? conjugateFamilyId = null,
        List<string>? ambiguityNotes = null)
    {
        return new FermionModeFamily
        {
            FamilyId = id,
            MemberModeIds = new List<string> { $"{id}-mode-0", $"{id}-mode-1" },
            BackgroundIds = new List<string> { "bg-1" },
            BranchVariantIds = new List<string> { "v1", "v2" },
            EigenvalueMagnitudeEnvelope = new[] { eigenMean * 0.9, eigenMean, eigenMean * 1.1 },
            DominantChiralityProfile = chirality,
            BranchPersistenceScore = branchPersistence,
            RefinementPersistenceScore = 1.0,
            ConjugateFamilyId = conjugateFamilyId,
            AmbiguityNotes = ambiguityNotes ?? new List<string>(),
            Provenance = TestProvenance(),
        };
    }

    private static FermionFamilyAtlas MakeAtlas(IReadOnlyList<FermionModeFamily> families)
    {
        return new FermionFamilyAtlas
        {
            AtlasId = "test-atlas",
            BranchFamilyId = "bf-test",
            ContextIds = new List<string> { "ctx-1", "ctx-2" },
            BackgroundIds = new List<string> { "bg-1" },
            Families = families.ToList(),
            Provenance = TestProvenance(),
        };
    }

    private static UnifiedParticleRegistry MakeRegistry(
        int bosons = 0,
        int fermions = 0,
        int interactions = 0)
    {
        var registry = new UnifiedParticleRegistry();
        for (int i = 0; i < bosons; i++)
            registry.Register(MakeRecord($"boson-{i}", UnifiedParticleType.Boson, "C2_BranchStableBosonicCandidate", 1.5));
        for (int i = 0; i < fermions; i++)
            registry.Register(MakeRecord($"fermion-{i}", UnifiedParticleType.Fermion, "C1_LocalPersistentMode", 1.0));
        for (int i = 0; i < interactions; i++)
            registry.Register(MakeRecord($"interaction-{i}", UnifiedParticleType.Interaction, "C0_NumericalMode", 0.5));
        return registry;
    }

    private static UnifiedParticleRecord MakeRecord(
        string id,
        UnifiedParticleType type,
        string claimClass,
        double massLike = 1.0)
    {
        return new UnifiedParticleRecord
        {
            ParticleId = id,
            ParticleType = type,
            PrimarySourceId = $"src-{id}",
            ContributingSourceIds = new List<string> { $"src-{id}" },
            BranchVariantSet = new List<string> { "v1" },
            BackgroundSet = new List<string> { "bg-1" },
            MassLikeEnvelope = new[] { massLike * 0.9, massLike, massLike * 1.1 },
            ClaimClass = claimClass,
            RegistryVersion = "1.0.0",
            Provenance = TestProvenance(),
        };
    }

    // -------------------------------------------------------
    // Phase4ReportBuilder: basic build
    // -------------------------------------------------------

    [Fact]
    public void Builder_Build_EmptyAtlas_EmptyRegistry_Succeeds()
    {
        var atlas = MakeAtlas(Array.Empty<FermionModeFamily>());
        var registry = new UnifiedParticleRegistry();
        var builder = new Phase4ReportBuilder("study-001");

        var report = builder.Build(atlas, registry, TestProvenance());

        Assert.NotNull(report);
        Assert.Equal("study-001", report.StudyId);
        Assert.NotEmpty(report.ReportId);
        Assert.Equal(0, report.FermionAtlas.TotalFamilies);
        Assert.Equal(0, report.RegistrySummary.TotalBosons);
    }

    [Fact]
    public void Builder_Build_WithFamilies_FermionAtlasSummaryPopulated()
    {
        var families = new[]
        {
            MakeFamily("fam-0", "left",  branchPersistence: 0.9),
            MakeFamily("fam-1", "right", branchPersistence: 0.8),
            MakeFamily("fam-2", "mixed", branchPersistence: 0.3),
        };
        var atlas = MakeAtlas(families);
        var registry = new UnifiedParticleRegistry();
        var builder = new Phase4ReportBuilder("study-001");

        var report = builder.Build(atlas, registry, TestProvenance());

        Assert.Equal(3, report.FermionAtlas.TotalFamilies);
        Assert.Equal(2, report.FermionAtlas.StableFamilies);   // persistence > 0.5
        Assert.Equal(0, report.FermionAtlas.AmbiguousFamilies);
        Assert.Equal(3, report.FermionAtlas.FamilySheets.Count);
        Assert.Equal(3, report.FermionAtlas.ChiralitySummaries.Count);
        Assert.Equal(3, report.FermionAtlas.ConjugationSummaries.Count);
    }

    [Fact]
    public void Builder_Build_LeftFamily_ChiralityStatusDefiniteLeft()
    {
        var families = new[] { MakeFamily("fam-0", "left", branchPersistence: 0.9) };
        var atlas = MakeAtlas(families);
        var builder = new Phase4ReportBuilder("s");

        var report = builder.Build(atlas, new UnifiedParticleRegistry(), TestProvenance());

        var chirality = report.FermionAtlas.ChiralitySummaries[0];
        Assert.Equal("definite-left", chirality.ChiralityStatus);
        Assert.Equal(1.0, chirality.LeftProjection, precision: 10);
        Assert.Equal(0.0, chirality.RightProjection, precision: 10);
        Assert.Equal("F", chirality.ChiralityType);
    }

    [Fact]
    public void Builder_Build_RightFamily_ChiralityStatusDefiniteRight()
    {
        var families = new[] { MakeFamily("fam-0", "right", branchPersistence: 0.9) };
        var atlas = MakeAtlas(families);
        var builder = new Phase4ReportBuilder("s");

        var report = builder.Build(atlas, new UnifiedParticleRegistry(), TestProvenance());

        var chirality = report.FermionAtlas.ChiralitySummaries[0];
        Assert.Equal("definite-right", chirality.ChiralityStatus);
    }

    [Fact]
    public void Builder_Build_ConjugatePairFamily_PairingScoreNonzero()
    {
        var families = new[]
        {
            MakeFamily("fam-0", "left",  conjugateFamilyId: "fam-1", branchPersistence: 0.9),
            MakeFamily("fam-1", "right", conjugateFamilyId: "fam-0", branchPersistence: 0.9),
        };
        var atlas = MakeAtlas(families);
        var builder = new Phase4ReportBuilder("s");

        var report = builder.Build(atlas, new UnifiedParticleRegistry(), TestProvenance());

        var conj0 = report.FermionAtlas.ConjugationSummaries[0];
        Assert.True(conj0.HasConjugatePair);
        Assert.Equal("fam-1", conj0.PairedFamilyId);
        Assert.True(conj0.PairingScore > 0.0);
    }

    [Fact]
    public void Builder_Build_FamilyWithAmbiguity_AmbiguousFamiliesCount()
    {
        var families = new[]
        {
            MakeFamily("fam-0", ambiguityNotes: new List<string> { "ambiguous-match" }),
        };
        var atlas = MakeAtlas(families);
        var builder = new Phase4ReportBuilder("s");

        var report = builder.Build(atlas, new UnifiedParticleRegistry(), TestProvenance());

        Assert.Equal(1, report.FermionAtlas.AmbiguousFamilies);
        var chirality = report.FermionAtlas.ChiralitySummaries[0];
        Assert.NotNull(chirality.DiagnosticNotes);
        Assert.NotEmpty(chirality.DiagnosticNotes!);
    }

    // -------------------------------------------------------
    // CouplingAtlasSummary
    // -------------------------------------------------------

    [Fact]
    public void Builder_NoCouplings_CouplingAtlasEmpty()
    {
        var atlas = MakeAtlas(Array.Empty<FermionModeFamily>());
        var builder = new Phase4ReportBuilder("s");

        var report = builder.Build(atlas, new UnifiedParticleRegistry(), TestProvenance());

        Assert.Equal(0, report.CouplingAtlas.TotalCouplings);
        Assert.Equal(0, report.CouplingAtlas.NonzeroCouplings);
        Assert.Equal(0.0, report.CouplingAtlas.MaxCouplingMagnitude);
    }

    [Fact]
    public void Builder_WithCouplingMatrices_SummaryAggregatesCorrectly()
    {
        var atlas = MakeAtlas(Array.Empty<FermionModeFamily>());
        var builder = new Phase4ReportBuilder("s");
        builder.AddCouplingMatrix(new CouplingMatrixSummary
        {
            BosonModeId = "b-mode-0",
            FermionPairCount = 3,
            MaxEntry = 0.8,
            FrobeniusNorm = 1.2,
        });
        builder.AddCouplingMatrix(new CouplingMatrixSummary
        {
            BosonModeId = "b-mode-1",
            FermionPairCount = 2,
            MaxEntry = 0.5,
            FrobeniusNorm = 0.9,
        });

        var report = builder.Build(atlas, new UnifiedParticleRegistry(), TestProvenance());

        Assert.Equal(2, report.CouplingAtlas.CouplingMatrices.Count);
        Assert.Equal(0.8, report.CouplingAtlas.MaxCouplingMagnitude, precision: 10);
        Assert.NotEmpty(report.CouplingAtlas.SummaryId);
    }

    // -------------------------------------------------------
    // UnifiedRegistrySummary
    // -------------------------------------------------------

    [Fact]
    public void Builder_RegistrySummary_CountsTypesCorrectly()
    {
        var registry = MakeRegistry(bosons: 2, fermions: 3, interactions: 1);
        var atlas = MakeAtlas(Array.Empty<FermionModeFamily>());
        var builder = new Phase4ReportBuilder("s");

        var report = builder.Build(atlas, registry, TestProvenance());

        Assert.Equal(2, report.RegistrySummary.TotalBosons);
        Assert.Equal(3, report.RegistrySummary.TotalFermions);
        Assert.Equal(1, report.RegistrySummary.TotalInteractions);
    }

    [Fact]
    public void Builder_RegistrySummary_TopCandidatesOrdered()
    {
        var registry = new UnifiedParticleRegistry();
        registry.Register(MakeRecord("low", UnifiedParticleType.Boson, "C0_NumericalMode", 0.5));
        registry.Register(MakeRecord("high", UnifiedParticleType.Boson, "C2_BranchStableBosonicCandidate", 2.0));
        registry.Register(MakeRecord("mid", UnifiedParticleType.Fermion, "C1_LocalPersistentMode", 1.0));

        var atlas = MakeAtlas(Array.Empty<FermionModeFamily>());
        var builder = new Phase4ReportBuilder("s");

        var report = builder.Build(atlas, registry, TestProvenance());

        // Top candidates ordered by claim class desc
        var top = report.RegistrySummary.TopCandidates;
        Assert.NotEmpty(top);
        // First should be C2
        Assert.Equal(2, UnifiedParticleRegistry.ParseClaimClassLevel(top[0].ClaimClass));
    }

    [Fact]
    public void Builder_RegistrySummary_ClaimClassCounts()
    {
        var registry = MakeRegistry(bosons: 1, fermions: 2);
        var atlas = MakeAtlas(Array.Empty<FermionModeFamily>());
        var builder = new Phase4ReportBuilder("s");

        var report = builder.Build(atlas, registry, TestProvenance());

        // C1 → 2 fermions, C2 → 1 boson
        Assert.True(report.RegistrySummary.ClaimClassCounts.ContainsKey("C2") ||
                    report.RegistrySummary.ClaimClassCounts.ContainsKey("C1"));
    }

    // -------------------------------------------------------
    // Negative results
    // -------------------------------------------------------

    [Fact]
    public void Builder_NegativeResults_AreIncluded()
    {
        var atlas = MakeAtlas(Array.Empty<FermionModeFamily>());
        var builder = new Phase4ReportBuilder("s");
        builder.AddNegativeResult("Background bg-fail had insufficient modes.");
        builder.AddNegativeResult("Coupling matrix degenerate for boson-mode-3.");

        var report = builder.Build(atlas, new UnifiedParticleRegistry(), TestProvenance());

        Assert.Equal(2, report.NegativeResults.Count);
        Assert.Contains("bg-fail", report.NegativeResults[0]);
    }

    // -------------------------------------------------------
    // Provenance
    // -------------------------------------------------------

    [Fact]
    public void Builder_Provenance_PreservedInReport()
    {
        var prov = TestProvenance();
        var atlas = MakeAtlas(Array.Empty<FermionModeFamily>());
        var builder = new Phase4ReportBuilder("study-xyz");

        var report = builder.Build(atlas, new UnifiedParticleRegistry(), prov);

        Assert.Equal(prov.CodeRevision, report.Provenance.CodeRevision);
        Assert.Equal(prov.Backend, report.Provenance.Backend);
    }

    // -------------------------------------------------------
    // JSON round-trip
    // -------------------------------------------------------

    [Fact]
    public void Phase4Report_ToJson_RoundTrip_PreservesStudyId()
    {
        var families = new[] { MakeFamily("fam-0", "left", branchPersistence: 0.9) };
        var atlas = MakeAtlas(families);
        var registry = MakeRegistry(bosons: 1, fermions: 1);
        var builder = new Phase4ReportBuilder("round-trip-study");
        builder.AddNegativeResult("nothing negative");

        var report = builder.Build(atlas, registry, TestProvenance());
        string json = report.ToJson();

        Assert.NotEmpty(json);
        var restored = Phase4Report.FromJson(json);
        Assert.Equal("round-trip-study", restored.StudyId);
        Assert.Equal(report.ReportId, restored.ReportId);
    }

    [Fact]
    public void Phase4Report_ToJson_RoundTrip_PreservesFermionAtlas()
    {
        var families = new[]
        {
            MakeFamily("fam-0", "left",  branchPersistence: 0.9),
            MakeFamily("fam-1", "right", branchPersistence: 0.6),
        };
        var atlas = MakeAtlas(families);
        var builder = new Phase4ReportBuilder("s");

        var report = builder.Build(atlas, new UnifiedParticleRegistry(), TestProvenance());
        string json = report.ToJson();
        var restored = Phase4Report.FromJson(json);

        Assert.Equal(2, restored.FermionAtlas.TotalFamilies);
        Assert.Equal(2, restored.FermionAtlas.StableFamilies);
        Assert.Equal(2, restored.FermionAtlas.FamilySheets.Count);
        Assert.Equal(2, restored.FermionAtlas.ChiralitySummaries.Count);
        Assert.Equal(2, restored.FermionAtlas.ConjugationSummaries.Count);
    }

    [Fact]
    public void Phase4Report_ToJson_RoundTrip_PreservesCouplingAtlas()
    {
        var atlas = MakeAtlas(Array.Empty<FermionModeFamily>());
        var builder = new Phase4ReportBuilder("s");
        builder.AddCouplingMatrix(new CouplingMatrixSummary
        {
            BosonModeId = "bm-0",
            FermionPairCount = 4,
            MaxEntry = 1.2,
            FrobeniusNorm = 2.5,
        });

        var report = builder.Build(atlas, new UnifiedParticleRegistry(), TestProvenance());
        string json = report.ToJson();
        var restored = Phase4Report.FromJson(json);

        Assert.Single(restored.CouplingAtlas.CouplingMatrices);
        Assert.Equal("bm-0", restored.CouplingAtlas.CouplingMatrices[0].BosonModeId);
        Assert.Equal(1.2, restored.CouplingAtlas.CouplingMatrices[0].MaxEntry, precision: 10);
    }

    [Fact]
    public void Phase4Report_ToJson_RoundTrip_PreservesRegistrySummary()
    {
        var registry = MakeRegistry(bosons: 2, fermions: 1, interactions: 1);
        var atlas = MakeAtlas(Array.Empty<FermionModeFamily>());
        var builder = new Phase4ReportBuilder("s");

        var report = builder.Build(atlas, registry, TestProvenance());
        string json = report.ToJson();
        var restored = Phase4Report.FromJson(json);

        Assert.Equal(2, restored.RegistrySummary.TotalBosons);
        Assert.Equal(1, restored.RegistrySummary.TotalFermions);
        Assert.Equal(1, restored.RegistrySummary.TotalInteractions);
    }

    // -------------------------------------------------------
    // FamilySheet claim class assignment
    // -------------------------------------------------------

    [Fact]
    public void FamilySheet_StableFamily_HasC1ClaimClass()
    {
        var families = new[] { MakeFamily("fam-0", branchPersistence: 0.8) };
        var atlas = MakeAtlas(families);
        var builder = new Phase4ReportBuilder("s");

        var report = builder.Build(atlas, new UnifiedParticleRegistry(), TestProvenance());

        Assert.Equal("C1_ObservedMode", report.FermionAtlas.FamilySheets[0].ClaimClass);
        Assert.True(report.FermionAtlas.FamilySheets[0].IsStable);
    }

    [Fact]
    public void FamilySheet_UnstableFamily_HasC0ClaimClass()
    {
        var families = new[] { MakeFamily("fam-0", branchPersistence: 0.2) };
        var atlas = MakeAtlas(families);
        var builder = new Phase4ReportBuilder("s");

        var report = builder.Build(atlas, new UnifiedParticleRegistry(), TestProvenance());

        Assert.Equal("C0_NumericalMode", report.FermionAtlas.FamilySheets[0].ClaimClass);
        Assert.False(report.FermionAtlas.FamilySheets[0].IsStable);
    }

    // -------------------------------------------------------
    // Null argument guards
    // -------------------------------------------------------

    [Fact]
    public void Builder_Build_NullAtlas_Throws()
    {
        var builder = new Phase4ReportBuilder("s");
        Assert.Throws<ArgumentNullException>(
            () => builder.Build(null!, new UnifiedParticleRegistry(), TestProvenance()));
    }

    [Fact]
    public void Builder_Build_NullRegistry_Throws()
    {
        var atlas = MakeAtlas(Array.Empty<FermionModeFamily>());
        var builder = new Phase4ReportBuilder("s");
        Assert.Throws<ArgumentNullException>(
            () => builder.Build(atlas, null!, TestProvenance()));
    }

    [Fact]
    public void Builder_Build_NullProvenance_Throws()
    {
        var atlas = MakeAtlas(Array.Empty<FermionModeFamily>());
        var builder = new Phase4ReportBuilder("s");
        Assert.Throws<ArgumentNullException>(
            () => builder.Build(atlas, new UnifiedParticleRegistry(), null!));
    }
}
