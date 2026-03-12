using Gu.Core;
using Gu.Phase4.FamilyClustering;
using Gu.Phase4.Fermions;

namespace Gu.Phase4.FamilyClustering.Tests;

/// <summary>
/// Tests for M39: FermionFamilyAtlasBuilder — atlas building from nontrivial branch family.
/// Completion criterion: at least one atlas exists for a nontrivial branch family.
/// </summary>
public class FermionFamilyAtlasTests
{
    private static ProvenanceMeta TestProvenance() => new()
    {
        CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
        CodeRevision = "test-m39-atlas",
        Branch = new BranchRef { BranchId = "test-branch", SchemaVersion = "1.0.0" },
        Backend = "cpu-reference",
    };

    private static FermionTrackingConfig DefaultConfig() => new()
    {
        MatchThreshold = 0.5,
        EigenvalueRelTol = 0.3,
        EigenvalueWeight = 0.5,
        ChiralityWeight = 0.5,
        EigenspaceWeight = 0.0,
    };

    private static FermionModeRecord MakeMode(
        string id, string bgId, string branchId,
        double eigenvalue, double netChirality = 0.0)
    {
        double left = 0.5 + netChirality * 0.5;
        double right = 0.5 - netChirality * 0.5;
        return new FermionModeRecord
        {
            ModeId = id,
            BackgroundId = bgId,
            BranchVariantId = branchId,
            LayoutId = "layout-1",
            ModeIndex = 0,
            EigenvalueRe = eigenvalue,
            EigenvalueIm = 0.0,
            ResidualNorm = 1e-8,
            ChiralityDecomposition = new ChiralityDecompositionRecord
            {
                LeftFraction = left,
                RightFraction = right,
                MixedFraction = 0.0,
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
    }

    /// <summary>
    /// Build a nontrivial branch family: 3 branch variants, each with 2 modes.
    /// The two modes have SIMILAR eigenvalue magnitudes (both near 1.0) but opposite chirality,
    /// so they will be detected as a conjugate pair.
    /// </summary>
    private static List<NamedSpectralResult> NontrivialBranchFamily()
    {
        return new List<NamedSpectralResult>
        {
            new NamedSpectralResult
            {
                ContextId = "branch-v1",
                BackgroundId = "bg-1",
                Modes = new[]
                {
                    MakeMode("m-v1-0", "bg-1", "branch-v1", 1.0, netChirality: 0.8),
                    MakeMode("m-v1-1", "bg-1", "branch-v1", 1.1, netChirality: -0.8),
                },
            },
            new NamedSpectralResult
            {
                ContextId = "branch-v2",
                BackgroundId = "bg-2",
                Modes = new[]
                {
                    MakeMode("m-v2-0", "bg-2", "branch-v2", 1.02, netChirality: 0.75),
                    MakeMode("m-v2-1", "bg-2", "branch-v2", 1.08, netChirality: -0.78),
                },
            },
            new NamedSpectralResult
            {
                ContextId = "branch-v3",
                BackgroundId = "bg-3",
                Modes = new[]
                {
                    MakeMode("m-v3-0", "bg-3", "branch-v3", 0.98, netChirality: 0.82),
                    MakeMode("m-v3-1", "bg-3", "branch-v3", 1.05, netChirality: -0.77),
                },
            },
        };
    }

    // -------------------------------------------------------
    // Tests
    // -------------------------------------------------------

    [Fact]
    public void Build_EmptyResults_ReturnsEmptyAtlas()
    {
        var builder = new FermionFamilyAtlasBuilder(DefaultConfig());
        var atlas = builder.Build("atlas-0", "branch-family-0",
            Array.Empty<NamedSpectralResult>(), TestProvenance());

        Assert.NotNull(atlas);
        Assert.Empty(atlas.Families);
    }

    [Fact]
    public void Build_SingleContext_FamilyCountEqualsModeCount()
    {
        var builder = new FermionFamilyAtlasBuilder(DefaultConfig());
        var results = new[]
        {
            new NamedSpectralResult
            {
                ContextId = "ctx-1",
                BackgroundId = "bg-1",
                Modes = new[] { MakeMode("m1", "bg-1", "v1", 1.0), MakeMode("m2", "bg-1", "v1", 2.0) },
            },
        };

        var atlas = builder.Build("atlas-1", "bf-1", results, TestProvenance());

        Assert.Equal(2, atlas.Families.Count);
    }

    [Fact]
    public void Build_NontrivialFamily_AtlasHasFamilies()
    {
        var builder = new FermionFamilyAtlasBuilder(DefaultConfig());
        var atlas = builder.Build("atlas-nontrivial", "branch-family-nontrivial",
            NontrivialBranchFamily(), TestProvenance());

        Assert.NotNull(atlas);
        Assert.NotEmpty(atlas.Families);
    }

    [Fact]
    public void Build_NontrivialFamily_PersistentFamiliesExist()
    {
        var builder = new FermionFamilyAtlasBuilder(DefaultConfig());
        var atlas = builder.Build("atlas-nontrivial", "branch-family-nontrivial",
            NontrivialBranchFamily(), TestProvenance());

        // Two modes track across all 3 branches → should have at least 1 persistent family
        Assert.True(atlas.PersistentFamilyCount >= 1,
            $"Expected at least 1 persistent family, got {atlas.PersistentFamilyCount}");
    }

    [Fact]
    public void Build_NontrivialFamily_ContextIdsPreserved()
    {
        var builder = new FermionFamilyAtlasBuilder(DefaultConfig());
        var results = NontrivialBranchFamily();
        var atlas = builder.Build("atlas-ctx", "bf-ctx", results, TestProvenance());

        Assert.Equal(3, atlas.ContextIds.Count);
        Assert.Contains("branch-v1", atlas.ContextIds);
        Assert.Contains("branch-v2", atlas.ContextIds);
        Assert.Contains("branch-v3", atlas.ContextIds);
    }

    [Fact]
    public void Build_NontrivialFamily_BackgroundIdsPreserved()
    {
        var builder = new FermionFamilyAtlasBuilder(DefaultConfig());
        var atlas = builder.Build("atlas-bg", "bf-bg",
            NontrivialBranchFamily(), TestProvenance());

        Assert.Contains("bg-1", atlas.BackgroundIds);
        Assert.Contains("bg-2", atlas.BackgroundIds);
        Assert.Contains("bg-3", atlas.BackgroundIds);
    }

    [Fact]
    public void Build_NontrivialFamily_SummaryIsConsistent()
    {
        var builder = new FermionFamilyAtlasBuilder(DefaultConfig());
        var atlas = builder.Build("atlas-sum", "bf-sum",
            NontrivialBranchFamily(), TestProvenance());

        Assert.Equal(atlas.Families.Count, atlas.Summary.TotalFamilies);
        Assert.True(atlas.Summary.MeanBranchPersistence > 0.0);
        Assert.True(atlas.Summary.MeanBranchPersistence <= 1.0);
    }

    [Fact]
    public void Build_NontrivialFamily_LeftAndRightChiralFamiliesDetected()
    {
        var builder = new FermionFamilyAtlasBuilder(DefaultConfig());
        var atlas = builder.Build("atlas-chiral", "bf-chiral",
            NontrivialBranchFamily(), TestProvenance());

        // One family should be "left"-dominant (netChirality ~ +0.8)
        // One family should be "right"-dominant (netChirality ~ -0.8)
        Assert.True(atlas.Summary.LeftChiralFamilies >= 1,
            $"Expected at least 1 left-chiral family, got {atlas.Summary.LeftChiralFamilies}");
        Assert.True(atlas.Summary.RightChiralFamilies >= 1,
            $"Expected at least 1 right-chiral family, got {atlas.Summary.RightChiralFamilies}");
    }

    [Fact]
    public void Build_NontrivialFamily_ConjugatePairsResolved()
    {
        var builder = new FermionFamilyAtlasBuilder(DefaultConfig());
        var atlas = builder.Build("atlas-conj", "bf-conj",
            NontrivialBranchFamily(), TestProvenance());

        // Left and right chiral families with similar eigenvalues → conjugate pair
        Assert.True(atlas.Summary.FamiliesWithConjugatePair >= 1,
            $"Expected conjugate pair, got {atlas.Summary.FamiliesWithConjugatePair}");
    }

    [Fact]
    public void Build_ProvenancePreserved()
    {
        var prov = TestProvenance();
        var builder = new FermionFamilyAtlasBuilder(DefaultConfig());
        var atlas = builder.Build("atlas-prov", "bf-prov",
            NontrivialBranchFamily(), prov);

        Assert.Equal(prov.CodeRevision, atlas.Provenance.CodeRevision);
        Assert.Equal(prov.Backend, atlas.Provenance.Backend);
    }

    [Fact]
    public void Build_NontrivialFamily_AtlasIdAndBranchFamilyIdSet()
    {
        var builder = new FermionFamilyAtlasBuilder(DefaultConfig());
        var atlas = builder.Build("my-atlas-id", "my-branch-family",
            NontrivialBranchFamily(), TestProvenance());

        Assert.Equal("my-atlas-id", atlas.AtlasId);
        Assert.Equal("my-branch-family", atlas.BranchFamilyId);
    }

    [Fact]
    public void FermionFamilyBuilder_FamiliesHaveEigenvalueEnvelope()
    {
        var builder = new FermionFamilyAtlasBuilder(DefaultConfig());
        var atlas = builder.Build("atlas-env", "bf-env",
            NontrivialBranchFamily(), TestProvenance());

        foreach (var family in atlas.Families)
        {
            Assert.Equal(3, family.EigenvalueMagnitudeEnvelope.Length);
            double min = family.EigenvalueMagnitudeEnvelope[0];
            double mean = family.EigenvalueMagnitudeEnvelope[1];
            double max = family.EigenvalueMagnitudeEnvelope[2];
            Assert.True(min <= mean);
            Assert.True(mean <= max);
        }
    }
}
