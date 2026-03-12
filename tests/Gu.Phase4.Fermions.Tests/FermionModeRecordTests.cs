using Gu.Core;
using Gu.Phase4.Fermions;

namespace Gu.Phase4.Fermions.Tests;

/// <summary>
/// Tests for FermionModeRecord and FermionModeFamily serialization and properties.
/// </summary>
public class FermionModeRecordTests
{
    private static ProvenanceMeta TestProvenance() => new()
    {
        CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
        CodeRevision = "test-rev",
        Branch = new BranchRef { BranchId = "test-branch", SchemaVersion = "1.0.0" },
        Backend = "cpu-reference",
    };

    private static FermionModeRecord MakeMode(string id, double eigenRe, double eigenIm,
        double leftFrac, double rightFrac) => new()
    {
        ModeId = id,
        BackgroundId = "bg-001",
        BranchVariantId = "bv-001",
        LayoutId = "layout-test",
        ModeIndex = 0,
        EigenvalueRe = eigenRe,
        EigenvalueIm = eigenIm,
        ResidualNorm = 1e-10,
        ChiralityDecomposition = new ChiralityDecompositionRecord
        {
            LeftFraction = leftFrac,
            RightFraction = rightFrac,
            MixedFraction = 1.0 - leftFrac - rightFrac,
            SignConvention = "left-is-minus",
        },
        ConjugationPairing = new ConjugationPairingRecord
        {
            HasPair = false,
            ConjugationType = "hermitian",
        },
        Backend = "cpu-reference",
        Provenance = TestProvenance(),
    };

    [Fact]
    public void EigenvalueMagnitude_IsCorrect()
    {
        var mode = MakeMode("m1", eigenRe: 3.0, eigenIm: 4.0, leftFrac: 0.5, rightFrac: 0.5);
        Assert.Equal(5.0, mode.EigenvalueMagnitude, 12);
    }

    [Fact]
    public void EigenvalueMagnitude_RealOnly()
    {
        var mode = MakeMode("m2", eigenRe: -2.5, eigenIm: 0.0, leftFrac: 1.0, rightFrac: 0.0);
        Assert.Equal(2.5, mode.EigenvalueMagnitude, 12);
    }

    [Fact]
    public void ChiralityDecomposition_NetChirality_LeftDominant()
    {
        var mode = MakeMode("m3", 1.0, 0.0, leftFrac: 0.9, rightFrac: 0.1);
        Assert.Equal(0.8, mode.ChiralityDecomposition.NetChirality, 12);
    }

    [Fact]
    public void ChiralityDecomposition_NetChirality_RightDominant()
    {
        var mode = MakeMode("m4", 1.0, 0.0, leftFrac: 0.1, rightFrac: 0.9);
        Assert.Equal(-0.8, mode.ChiralityDecomposition.NetChirality, 12);
    }

    [Fact]
    public void FermionModeRecord_SerializesAndRoundTrips()
    {
        var mode = MakeMode("mode-rt", 1.5, 0.0, 0.7, 0.3);
        var json = FermionStateSerializer.ToJson(mode);

        Assert.Contains("mode-rt", json);
        Assert.Contains("eigenvalueRe", json);

        var loaded = FermionStateSerializer.ModeRecordFromJson(json);
        Assert.Equal(mode.ModeId, loaded.ModeId);
        Assert.Equal(mode.EigenvalueRe, loaded.EigenvalueRe, 14);
        Assert.Equal(mode.EigenvalueIm, loaded.EigenvalueIm, 14);
        Assert.Equal(mode.ChiralityDecomposition.LeftFraction,
                     loaded.ChiralityDecomposition.LeftFraction, 14);
    }

    [Fact]
    public void FermionModeFamily_SerializesAndRoundTrips()
    {
        var family = new FermionModeFamily
        {
            FamilyId = "family-001",
            MemberModeIds = new List<string> { "m1", "m2", "m3" },
            BackgroundIds = new List<string> { "bg-001" },
            BranchVariantIds = new List<string> { "bv-001", "bv-002" },
            EigenvalueMagnitudeEnvelope = new[] { 0.5, 1.0, 1.5 },
            DominantChiralityProfile = "left",
            HasConjugationPair = true,
            ConjugateFamilyId = "family-002",
            BranchPersistenceScore = 0.85,
            RefinementPersistenceScore = 0.9,
            AverageGaugeLeakScore = 0.02,
            Provenance = TestProvenance(),
        };

        var json = FermionStateSerializer.ToJson(family);
        Assert.Contains("family-001", json);

        var loaded = FermionStateSerializer.ModeFamilyFromJson(json);
        Assert.Equal(family.FamilyId, loaded.FamilyId);
        Assert.Equal(family.MemberModeIds.Count, loaded.MemberModeIds.Count);
        Assert.Equal(family.DominantChiralityProfile, loaded.DominantChiralityProfile);
        Assert.Equal(family.BranchPersistenceScore, loaded.BranchPersistenceScore, 14);
    }

    [Fact]
    public void FermionBackgroundRecord_ZeroBackground_SerializesAndRoundTrips()
    {
        var bg = new FermionBackgroundRecord
        {
            BackgroundId = "fbg-001",
            BosonicBackgroundId = "bg-001",
            BranchVariantId = "bv-001",
            LayoutId = "layout-test",
            IsZeroBackground = true,
            SelectedFromPhase3 = true,
            BosonicReplayTier = "R2",
            Provenance = TestProvenance(),
        };

        var json = FermionStateSerializer.ToJson(bg);
        Assert.Contains("fbg-001", json);
        Assert.Contains("isZeroBackground", json);

        var loaded = FermionStateSerializer.BackgroundRecordFromJson(json);
        Assert.Equal(bg.BackgroundId, loaded.BackgroundId);
        Assert.True(loaded.IsZeroBackground);
        Assert.Equal("R2", loaded.BosonicReplayTier);
        Assert.Null(loaded.PrimalState);
    }

    [Fact]
    public void FermionModeRecord_ComputedWithUnverifiedGpu_DefaultsFalse()
    {
        var mode = MakeMode("m-default", 1.0, 0.0, 0.5, 0.5);
        Assert.False(mode.ComputedWithUnverifiedGpu);
    }

    [Fact]
    public void FermionModeRecord_GaugeReductionApplied_DefaultsFalse()
    {
        var mode = MakeMode("m-default2", 1.0, 0.0, 0.5, 0.5);
        Assert.False(mode.GaugeReductionApplied);
    }

    [Fact]
    public void FermionModeFamily_NegativeResultPreservation()
    {
        // A family with ambiguity notes (negative result is first-class)
        var family = new FermionModeFamily
        {
            FamilyId = "family-ambiguous",
            MemberModeIds = new List<string>(),
            BackgroundIds = new List<string> { "bg-001" },
            BranchVariantIds = new List<string> { "bv-001" },
            EigenvalueMagnitudeEnvelope = new[] { 0.0, 0.0, 0.0 },
            DominantChiralityProfile = "undetermined",
            BranchPersistenceScore = 0.0,
            RefinementPersistenceScore = 0.0,
            AverageGaugeLeakScore = 1.0,
            AmbiguityNotes = new List<string> { "No modes found in this branch variant", "Unstable under refinement" },
            Provenance = TestProvenance(),
        };

        var json = FermionStateSerializer.ToJson(family);
        var loaded = FermionStateSerializer.ModeFamilyFromJson(json);

        Assert.Equal(2, loaded.AmbiguityNotes.Count);
        Assert.Equal("undetermined", loaded.DominantChiralityProfile);
    }
}
