using Gu.Artifacts;
using Gu.Core.Serialization;

namespace Gu.Phase5.Dossiers.Tests;

/// <summary>
/// Tests for M52: NegativeResultLedger and NegativeResultEntry.
/// Phase V rule §12.4: negative results are first-class artifacts.
/// </summary>
public sealed class NegativeResultLedgerTests
{
    private static NegativeResultEntry MakeEntry(
        string entryId,
        string category = "branch-fragility",
        bool impliesDemotion = false) => new()
    {
        EntryId = entryId,
        Category = category,
        Description = $"Test negative result: {category}",
        SourceStudyId = "study-001",
        RecommendedAction = "demote-candidate",
        ImpliesDemotion = impliesDemotion,
        RecordedAt = DateTimeOffset.UtcNow,
    };

    [Fact]
    public void Ledger_EmptyEntries_CountIsZero()
    {
        var ledger = new NegativeResultLedger
        {
            LedgerId = "ledger-001",
            Title = "Test Ledger",
            Entries = new List<NegativeResultEntry>(),
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        Assert.Equal(0, ledger.Count);
    }

    [Fact]
    public void Ledger_WithEntries_CountMatchesEntriesCount()
    {
        var ledger = new NegativeResultLedger
        {
            LedgerId = "ledger-001",
            Title = "Test Ledger",
            Entries = new List<NegativeResultEntry>
            {
                MakeEntry("e-001"),
                MakeEntry("e-002"),
                MakeEntry("e-003"),
            },
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        Assert.Equal(3, ledger.Count);
    }

    [Fact]
    public void Entry_AllCategories_AreValidStrings()
    {
        var categories = new[]
        {
            "branch-fragility",
            "non-convergence",
            "observation-instability",
            "environment-instability",
            "quantitative-mismatch",
            "representation-content",
            "coupling-inconsistency",
            "no-mode-found",
            "study-stale",
        };

        foreach (var cat in categories)
        {
            var entry = MakeEntry("e-" + cat, category: cat);
            Assert.Equal(cat, entry.Category);
        }
    }

    [Fact]
    public void Entry_ImpliesDemotion_FlagSetCorrectly()
    {
        var entry = MakeEntry("e-001", impliesDemotion: true);
        Assert.True(entry.ImpliesDemotion);

        var entry2 = MakeEntry("e-002", impliesDemotion: false);
        Assert.False(entry2.ImpliesDemotion);
    }

    [Fact]
    public void Entry_AffectedCandidateIds_DefaultEmpty()
    {
        var entry = MakeEntry("e-001");
        Assert.Empty(entry.AffectedCandidateIds);
    }

    [Fact]
    public void Entry_WithAffectedCandidates_ListPreserved()
    {
        var entry = new NegativeResultEntry
        {
            EntryId = "e-001",
            Category = "branch-fragility",
            Description = "fragile mode",
            SourceStudyId = "study-001",
            AffectedCandidateIds = new List<string> { "boson-001", "boson-002" },
            RecommendedAction = "demote-candidate",
            RecordedAt = DateTimeOffset.UtcNow,
        };

        Assert.Equal(2, entry.AffectedCandidateIds.Count);
        Assert.Contains("boson-001", entry.AffectedCandidateIds);
    }

    [Fact]
    public void Ledger_SerializesAndDeserializes()
    {
        var ledger = new NegativeResultLedger
        {
            LedgerId = "ledger-round-trip",
            Title = "Round-trip Test Ledger",
            Entries = new List<NegativeResultEntry>
            {
                new()
                {
                    EntryId = "e-001",
                    Category = "non-convergence",
                    Description = "Refinement sweep does not converge for quantity Q.",
                    SourceStudyId = "study-refinement-001",
                    AffectedCandidateIds = new List<string> { "boson-mode-0" },
                    QuantitativeEvidence = "convergence rate < 1.0 (rate=0.3)",
                    ImpliesDemotion = true,
                    RecommendedAction = "demote-candidate",
                    EvidenceTier = ArtifactEvidenceTier.RegeneratedCpu,
                    RecordedAt = DateTimeOffset.UtcNow,
                },
            },
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        var json = GuJsonDefaults.Serialize(ledger);
        var roundtrip = GuJsonDefaults.Deserialize<NegativeResultLedger>(json);

        Assert.NotNull(roundtrip);
        Assert.Equal("ledger-round-trip", roundtrip!.LedgerId);
        Assert.Single(roundtrip.Entries);
        Assert.Equal("non-convergence", roundtrip.Entries[0].Category);
        Assert.True(roundtrip.Entries[0].ImpliesDemotion);
    }

    [Fact]
    public void Entry_SchemaVersion_DefaultSet()
    {
        var entry = MakeEntry("e-001");
        Assert.Equal("1.0.0", entry.SchemaVersion);
    }
}
