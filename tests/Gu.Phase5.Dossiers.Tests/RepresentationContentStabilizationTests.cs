using Gu.Core;
using Gu.Phase5.Falsification;

namespace Gu.Phase5.Dossiers.Tests;

/// <summary>
/// Tests for P11-M5: representation-content fatal stabilization.
///
/// Verifies that:
///   1. Representation-content fatals remain active in the dossier negative results (D-P11-004).
///   2. affectedCandidateIds is populated from the falsifier TargetId.
///   3. sourceArtifactRefs are propagated from the matching RepresentationContentRecord.
///   4. p11StabilizationNote is set when missingRequiredCount > 0.
///   5. Non-fatal representation-content records do not generate a stabilization note.
/// </summary>
public sealed class RepresentationContentStabilizationTests
{
    private static ProvenanceMeta MakeProvenance() => new ProvenanceMeta
    {
        CreatedAt = DateTimeOffset.UtcNow,
        CodeRevision = "phase11-rep-content-stabilization-v1",
        Branch = new BranchRef { BranchId = "phase11-m5-stabilization", SchemaVersion = "1.0.0" },
    };

    private static FalsifierSummary MakeRepContentFatalSummary(string candidateId) =>
        new FalsifierSummary
        {
            StudyId = "test-study",
            Falsifiers = new List<FalsifierRecord>
            {
                new FalsifierRecord
                {
                    FalsifierId = "falsifier-0001",
                    FalsifierType = FalsifierTypes.RepresentationContent,
                    Severity = FalsifierSeverity.Fatal,
                    TargetId = candidateId,
                    BranchId = "unknown",
                    TriggerValue = 1,
                    Threshold = 0,
                    Description = $"Candidate '{candidateId}' exposes only 1 persisted family source(s); at least 2 are required.",
                    Evidence = $"RepresentationContentRecord candidateId={candidateId} missingRequiredCount=1",
                    Active = true,
                    Provenance = MakeProvenance(),
                },
            },
            ActiveFatalCount = 1,
            ActiveHighCount = 0,
            TotalActiveCount = 1,
            Provenance = MakeProvenance(),
        };

    private static RepresentationContentRecord MakeSingletonRecord(string candidateId) =>
        new RepresentationContentRecord
        {
            RecordId = $"rep-{candidateId}",
            CandidateId = candidateId,
            ExpectedModeCount = 2,
            ObservedModeCount = 1,
            MissingRequiredCount = 1,
            StructuralMismatchScore = 0.35,
            Consistent = false,
            InconsistencyDescription = $"Candidate '{candidateId}' exposes only 1 persisted family source(s); at least 2 are required.",
            Origin = "upstream-sourced",
            SourceArtifactRefs = new List<string>
            {
                "/studies/phase4_fermion_family_atlas_001/output/fermion_family_atlas.json",
                "/studies/phase4_fermion_family_atlas_001/output/unified_particle_registry.json",
            },
            P11StabilizationNote = "Phase XI P11-M5 examination: singleton cluster; no second family found.",
            Provenance = MakeProvenance(),
        };

    private static RepresentationContentRecord MakeConsistentRecord(string candidateId) =>
        new RepresentationContentRecord
        {
            RecordId = $"rep-{candidateId}",
            CandidateId = candidateId,
            ExpectedModeCount = 2,
            ObservedModeCount = 2,
            MissingRequiredCount = 0,
            StructuralMismatchScore = 0.04,
            Consistent = true,
            InconsistencyDescription = null,
            Origin = "upstream-sourced",
            Provenance = MakeProvenance(),
        };

    private Phase5ValidationDossier AssembleDossier(
        FalsifierSummary? falsifiers,
        IReadOnlyList<RepresentationContentRecord>? repContentRecords)
    {
        var assembler = new Phase5DossierAssembler();
        return assembler.Assemble(
            studyId: "p11-m5-test-study",
            branchRecord: null,
            convergenceRecords: null,
            convergenceFailures: null,
            environments: null,
            scoreCard: null,
            falsifiers: falsifiers,
            registry: null,
            environmentTiersCovered: Array.Empty<string>(),
            freshness: "regenerated-current-code",
            provenance: MakeProvenance(),
            representationContentRecords: repContentRecords);
    }

    [Fact]
    public void RepresentationContentFatal_IsPreservedInNegativeResults()
    {
        // D-P11-004: the fatal must be preserved, not suppressed.
        var candidateId = "fermion-registry-phase4-toy-v1-0000";
        var falsifiers = MakeRepContentFatalSummary(candidateId);
        var records = new[] { MakeSingletonRecord(candidateId) };

        var dossier = AssembleDossier(falsifiers, records);

        Assert.Single(dossier.NegativeResults);
        var entry = dossier.NegativeResults[0];
        Assert.Equal("representation-content", entry.Category);
        Assert.True(entry.ImpliesDemotion);
        Assert.Equal("demote-candidate", entry.RecommendedAction);
    }

    [Fact]
    public void RepresentationContentFatal_PopulatesAffectedCandidateIds()
    {
        var candidateId = "fermion-registry-phase4-toy-v1-0000";
        var falsifiers = MakeRepContentFatalSummary(candidateId);
        var records = new[] { MakeSingletonRecord(candidateId) };

        var dossier = AssembleDossier(falsifiers, records);

        var entry = Assert.Single(dossier.NegativeResults);
        Assert.Contains(candidateId, entry.AffectedCandidateIds);
    }

    [Fact]
    public void RepresentationContentFatal_PopulatesSourceArtifactRefs()
    {
        var candidateId = "fermion-registry-phase4-toy-v1-0000";
        var falsifiers = MakeRepContentFatalSummary(candidateId);
        var records = new[] { MakeSingletonRecord(candidateId) };

        var dossier = AssembleDossier(falsifiers, records);

        var entry = Assert.Single(dossier.NegativeResults);
        Assert.NotNull(entry.SourceArtifactRefs);
        Assert.NotEmpty(entry.SourceArtifactRefs);
        Assert.Contains(entry.SourceArtifactRefs, r => r.Contains("fermion_family_atlas.json"));
    }

    [Fact]
    public void RepresentationContentFatal_Singleton_SetsP11StabilizationNote()
    {
        // P11-M5: singleton-cluster fatal must carry an explicit stabilization note.
        var candidateId = "fermion-registry-phase4-toy-v1-0000";
        var falsifiers = MakeRepContentFatalSummary(candidateId);
        var records = new[] { MakeSingletonRecord(candidateId) };

        var dossier = AssembleDossier(falsifiers, records);

        var entry = Assert.Single(dossier.NegativeResults);
        Assert.NotNull(entry.P11StabilizationNote);
        Assert.Contains("singleton", entry.P11StabilizationNote, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("D-P11-004", entry.P11StabilizationNote);
    }

    [Fact]
    public void RepresentationContentFatal_WithoutMatchingRecord_StillPreservesBlocker()
    {
        // Even without a matching RepresentationContentRecord, the fatal is preserved.
        var candidateId = "fermion-registry-phase4-toy-v1-0000";
        var falsifiers = MakeRepContentFatalSummary(candidateId);

        var dossier = AssembleDossier(falsifiers, repContentRecords: null);

        var entry = Assert.Single(dossier.NegativeResults);
        Assert.Equal("representation-content", entry.Category);
        Assert.Contains(candidateId, entry.AffectedCandidateIds);
        Assert.Null(entry.SourceArtifactRefs);
        Assert.Null(entry.P11StabilizationNote);
    }

    [Fact]
    public void RepresentationContentConsistent_ProducesNoNegativeResult()
    {
        // Candidates that satisfy the check should not generate negative results.
        var candidateId = "fermion-registry-phase4-toy-v1-0001";
        var records = new[] { MakeConsistentRecord(candidateId) };

        // No fatal falsifier — only a consistent record.
        var dossier = AssembleDossier(falsifiers: null, repContentRecords: records);

        Assert.Empty(dossier.NegativeResults);
    }

    [Fact]
    public void FalsifierSummary_RepContentFatal_RemainsActive_PerDp11004()
    {
        // The falsifier itself must remain active — do not suppress it.
        var candidateId = "fermion-registry-phase4-toy-v1-0000";
        var falsifiers = MakeRepContentFatalSummary(candidateId);
        var records = new[] { MakeSingletonRecord(candidateId) };

        var dossier = AssembleDossier(falsifiers, records);

        // The dossier exposes the FalsifierSummary unchanged.
        Assert.NotNull(dossier.FalsifierSummary);
        Assert.Equal(1, dossier.FalsifierSummary!.ActiveFatalCount);
        Assert.True(dossier.HasFatalFalsifier);
    }
}
