using Gu.Artifacts;

namespace Gu.Phase5.Dossiers.Tests;

public class DossierAssemblerTests
{
    private static StudyManifest MakeStudy(string id, ArtifactEvidenceTier tier)
    {
        ReproducibilityBundle? bundle = tier == ArtifactEvidenceTier.StaleCheckedIn
            ? null
            : ReproducibilityBundle.CreateRegeneratedCpu(
                "abc123",
                new[] { $"dotnet run -- study {id}" });

        // Override tier if needed (e.g. RegeneratedVerifiedCpu or CrossBackendVerified)
        if (bundle is not null && tier != ArtifactEvidenceTier.RegeneratedCpu)
        {
            bundle = new ReproducibilityBundle
            {
                CodeRevision = "abc123",
                ReproductionCommands = new[] { $"dotnet run -- study {id}" },
                RegeneratedAt = DateTimeOffset.UtcNow,
                EvidenceTier = tier,
            };
        }

        return new StudyManifest
        {
            StudyId = id,
            Description = $"Test study {id}",
            RunFolder = $"/runs/{id}",
            Reproducibility = bundle,
        };
    }

    [Fact]
    public void Assemble_WithNoStudies_ProducesEmptyStaleNotAcceptable()
    {
        var dossier = DossierAssembler.Assemble("d-001", "Empty", Array.Empty<StudyManifest>());

        Assert.Equal("d-001", dossier.DossierId);
        Assert.Equal(ArtifactEvidenceTier.StaleCheckedIn, dossier.OverallEvidenceTier);
        Assert.False(dossier.IsAcceptableAsEvidence);
        Assert.Empty(dossier.StaleStudyIds);
    }

    [Fact]
    public void Assemble_AllStale_NotAcceptableAllStaleIdsListed()
    {
        var studies = new[]
        {
            MakeStudy("s-001", ArtifactEvidenceTier.StaleCheckedIn),
            MakeStudy("s-002", ArtifactEvidenceTier.StaleCheckedIn),
        };

        var dossier = DossierAssembler.Assemble("d-002", "All stale", studies);

        Assert.Equal(ArtifactEvidenceTier.StaleCheckedIn, dossier.OverallEvidenceTier);
        Assert.False(dossier.IsAcceptableAsEvidence);
        Assert.Equal(2, dossier.StaleStudyIds.Count);
        Assert.Contains("s-001", dossier.StaleStudyIds);
        Assert.Contains("s-002", dossier.StaleStudyIds);
        Assert.Contains("G-006", dossier.EvidenceVerdict);
        Assert.Contains("NOT acceptable", dossier.EvidenceVerdict);
    }

    [Fact]
    public void Assemble_AllRegenerated_AcceptableNoStaleIds()
    {
        var studies = new[]
        {
            MakeStudy("s-001", ArtifactEvidenceTier.RegeneratedCpu),
            MakeStudy("s-002", ArtifactEvidenceTier.RegeneratedCpu),
        };

        var dossier = DossierAssembler.Assemble("d-003", "All regenerated", studies);

        Assert.Equal(ArtifactEvidenceTier.RegeneratedCpu, dossier.OverallEvidenceTier);
        Assert.True(dossier.IsAcceptableAsEvidence);
        Assert.Empty(dossier.StaleStudyIds);
        Assert.Contains("Acceptable", dossier.EvidenceVerdict);
    }

    [Fact]
    public void Assemble_MixedTiers_OverallIsMinimum()
    {
        var studies = new[]
        {
            MakeStudy("s-001", ArtifactEvidenceTier.CrossBackendVerified),
            MakeStudy("s-002", ArtifactEvidenceTier.RegeneratedCpu),
        };

        var dossier = DossierAssembler.Assemble("d-004", "Mixed tiers", studies);

        // Overall should be the minimum: RegeneratedCpu
        Assert.Equal(ArtifactEvidenceTier.RegeneratedCpu, dossier.OverallEvidenceTier);
        Assert.True(dossier.IsAcceptableAsEvidence);
        Assert.Empty(dossier.StaleStudyIds);
    }

    [Fact]
    public void Assemble_OneStaleOneRegenerated_NotAcceptable_StaleListedOnly()
    {
        var studies = new[]
        {
            MakeStudy("s-good", ArtifactEvidenceTier.RegeneratedCpu),
            MakeStudy("s-stale", ArtifactEvidenceTier.StaleCheckedIn),
        };

        var dossier = DossierAssembler.Assemble("d-005", "One stale", studies);

        // Stale pulls overall down to StaleCheckedIn
        Assert.Equal(ArtifactEvidenceTier.StaleCheckedIn, dossier.OverallEvidenceTier);
        Assert.False(dossier.IsAcceptableAsEvidence);
        Assert.Single(dossier.StaleStudyIds);
        Assert.Equal("s-stale", dossier.StaleStudyIds[0]);
        // Notes contain G-006 warning for stale study
        Assert.Contains(dossier.AssemblyNotes!, n => n.Contains("G-006") && n.Contains("s-stale"));
    }

    [Fact]
    public void Assemble_AssemblyNotesIncludeAllStudies()
    {
        var studies = new[]
        {
            MakeStudy("s-001", ArtifactEvidenceTier.RegeneratedCpu),
            MakeStudy("s-002", ArtifactEvidenceTier.StaleCheckedIn),
        };

        var dossier = DossierAssembler.Assemble("d-006", "Notes check", studies);

        Assert.NotNull(dossier.AssemblyNotes);
        Assert.Equal(2, dossier.AssemblyNotes!.Count);
    }

    [Fact]
    public void BuildIndex_CreatesSummaryEntries()
    {
        var dossier1 = DossierAssembler.Assemble("d-001", "Study 1",
            new[] { MakeStudy("s-001", ArtifactEvidenceTier.RegeneratedCpu) });
        var dossier2 = DossierAssembler.Assemble("d-002", "Study 2",
            new[] { MakeStudy("s-002", ArtifactEvidenceTier.StaleCheckedIn) });

        var index = DossierAssembler.BuildIndex("idx-001", new[]
        {
            ("phase5/dossiers/d-001.json", dossier1),
            ("phase5/dossiers/d-002.json", dossier2),
        });

        Assert.Equal("idx-001", index.IndexId);
        Assert.Equal(2, index.Entries.Count);

        var entry1 = index.Entries.First(e => e.DossierId == "d-001");
        Assert.True(entry1.IsAcceptableAsEvidence);
        Assert.Equal(0, entry1.StaleStudyCount);

        var entry2 = index.Entries.First(e => e.DossierId == "d-002");
        Assert.False(entry2.IsAcceptableAsEvidence);
        Assert.Equal(1, entry2.StaleStudyCount);
    }

    [Fact]
    public void ValidationDossier_AssembledAt_IsRecent()
    {
        var before = DateTimeOffset.UtcNow;
        var dossier = DossierAssembler.Assemble("d-007", "Time check",
            new[] { MakeStudy("s-001", ArtifactEvidenceTier.RegeneratedCpu) });
        var after = DateTimeOffset.UtcNow;

        Assert.True(dossier.AssembledAt >= before);
        Assert.True(dossier.AssembledAt <= after);
    }

    [Fact]
    public void StudyManifest_NullReproducibility_EffectiveTierIsStale()
    {
        var study = new StudyManifest
        {
            StudyId = "s-null",
            Description = "No reproducibility",
            RunFolder = "/runs/s-null",
            Reproducibility = null,
        };

        Assert.Equal(ArtifactEvidenceTier.StaleCheckedIn, study.EffectiveEvidenceTier);
    }
}
