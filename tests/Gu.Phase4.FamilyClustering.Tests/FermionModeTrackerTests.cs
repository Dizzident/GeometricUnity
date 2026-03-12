using Gu.Core;
using Gu.Phase4.FamilyClustering;
using Gu.Phase4.Fermions;

namespace Gu.Phase4.FamilyClustering.Tests;

/// <summary>
/// Tests for M39: FermionModeTracker — mode matching across spectral contexts.
/// </summary>
public class FermionModeTrackerTests
{
    private static ProvenanceMeta TestProvenance() => new()
    {
        CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
        CodeRevision = "test-m39",
        Branch = new BranchRef { BranchId = "test-branch", SchemaVersion = "1.0.0" },
        Backend = "cpu-reference",
    };

    private static FermionModeRecord MakeMode(
        string id,
        double eigenvalue,
        double netChirality = 0.0,
        double[]? eigvec = null)
    {
        double left = 0.5 + netChirality * 0.5;
        double right = 0.5 - netChirality * 0.5;
        return new FermionModeRecord
        {
            ModeId = id,
            BackgroundId = "bg-1",
            BranchVariantId = "v1",
            LayoutId = "layout-1",
            ModeIndex = 0,
            EigenvalueRe = eigenvalue,
            EigenvalueIm = 0.0,
            ResidualNorm = 1e-8,
            EigenvectorCoefficients = eigvec,
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

    private static FermionTrackingConfig DefaultConfig() => new()
    {
        MatchThreshold = 0.5,
        EigenvalueRelTol = 0.3,
        EigenvalueWeight = 0.5,
        ChiralityWeight = 0.5,
        EigenspaceWeight = 0.0,
    };

    // -------------------------------------------------------
    // Tests
    // -------------------------------------------------------

    [Fact]
    public void Match_EmptySource_ReturnsEmpty()
    {
        var tracker = new FermionModeTracker(DefaultConfig());
        var result = tracker.Match(Array.Empty<FermionModeRecord>(),
                                   new[] { MakeMode("t1", 1.0) });
        Assert.Empty(result);
    }

    [Fact]
    public void Match_EmptyTarget_ReturnsUnmatched()
    {
        var tracker = new FermionModeTracker(DefaultConfig());
        var result = tracker.Match(new[] { MakeMode("s1", 1.0) },
                                   Array.Empty<FermionModeRecord>());
        Assert.Single(result);
        Assert.False(result[0].IsMatch);
    }

    [Fact]
    public void Match_IdenticalEigenvalue_ScoresHigh()
    {
        var tracker = new FermionModeTracker(DefaultConfig());
        var src = new[] { MakeMode("s1", 1.0) };
        var tgt = new[] { MakeMode("t1", 1.0) };
        var result = tracker.Match(src, tgt);

        Assert.Single(result);
        Assert.True(result[0].IsMatch);
        Assert.True(result[0].AggregateScore > 0.9);
    }

    [Fact]
    public void Match_VeryDifferentEigenvalue_LowEigenvalueSimilarity()
    {
        var tracker = new FermionModeTracker(DefaultConfig());
        // eigenvalue 1.0 vs 1000.0 — relative difference ≈ 1.0, much larger than EigenvalueRelTol=0.3
        var src = new[] { MakeMode("s1", 1.0) };
        var tgt = new[] { MakeMode("t1", 1000.0) };
        var result = tracker.Match(src, tgt);

        Assert.Single(result);
        // Eigenvalue similarity should be very low
        Assert.True(result[0].EigenvalueSimilarity < 0.05,
            $"Expected low eigenvalue similarity, got {result[0].EigenvalueSimilarity}");
    }

    [Fact]
    public void Match_OppositeChirality_LowChiralityScore()
    {
        var tracker = new FermionModeTracker(DefaultConfig());
        // Same eigenvalue but opposite chirality
        var src = new[] { MakeMode("s1", 1.0, netChirality: 1.0) };
        var tgt = new[] { MakeMode("t1", 1.0, netChirality: -1.0) };
        var result = tracker.Match(src, tgt);

        // chirality similarity = 1 - |1 - (-1)| / 2 = 0
        // eigenvalue similarity ≈ 1 (same)
        // aggregate = 0.5 * 1.0 + 0.5 * 0.0 = 0.5
        // IsMatch depends on threshold 0.5 — borderline
        Assert.Single(result);
        Assert.True(result[0].ChiralitySimilarity < 0.1);
    }

    [Fact]
    public void Match_SameChirality_HighChiralityScore()
    {
        var tracker = new FermionModeTracker(DefaultConfig());
        var src = new[] { MakeMode("s1", 1.0, netChirality: 0.8) };
        var tgt = new[] { MakeMode("t1", 1.05, netChirality: 0.75) };
        var result = tracker.Match(src, tgt);

        Assert.Single(result);
        Assert.True(result[0].ChiralitySimilarity > 0.9);
        Assert.True(result[0].IsMatch);
    }

    [Fact]
    public void Match_EigenspaceOverlap_UsedWhenVectorsAvailable()
    {
        var config = new FermionTrackingConfig
        {
            MatchThreshold = 0.3,
            EigenvalueRelTol = 0.3,
            EigenvalueWeight = 1.0 / 3,
            ChiralityWeight = 1.0 / 3,
            EigenspaceWeight = 1.0 / 3,
        };
        var tracker = new FermionModeTracker(config);

        // Identical eigenvectors
        var v = new double[] { 1.0, 0.0, 0.0, 0.0 };
        var src = new[] { MakeMode("s1", 1.0, eigvec: v) };
        var tgt = new[] { MakeMode("t1", 1.0, eigvec: v) };
        var result = tracker.Match(src, tgt);

        Assert.Single(result);
        Assert.True(result[0].EigenspaceOverlap.HasValue);
        Assert.True(result[0].EigenspaceOverlap!.Value > 0.99);
        Assert.True(result[0].IsMatch);
    }

    [Fact]
    public void Match_TwoModes_OneToOneAssignment()
    {
        var tracker = new FermionModeTracker(DefaultConfig());
        var src = new[]
        {
            MakeMode("s1", 1.0),
            MakeMode("s2", 5.0),
        };
        var tgt = new[]
        {
            MakeMode("t1", 5.1),
            MakeMode("t2", 1.05),
        };
        var result = tracker.Match(src, tgt);

        Assert.Equal(2, result.Count);
        // s1 (ev=1.0) should match t2 (ev=1.05)
        // s2 (ev=5.0) should match t1 (ev=5.1)
        Assert.Equal("t2", result[0].TargetModeId);
        Assert.Equal("t1", result[1].TargetModeId);
    }

    [Fact]
    public void Match_AmbiguousCase_FlagsAmbiguity()
    {
        var config = new FermionTrackingConfig
        {
            MatchThreshold = 0.1,
            EigenvalueRelTol = 10.0, // very lenient → nearly equal scores
            EigenvalueWeight = 1.0,
            ChiralityWeight = 0.0,
            EigenspaceWeight = 0.0,
            AmbiguityMargin = 0.5,
        };
        var tracker = new FermionModeTracker(config);
        var src = new[] { MakeMode("s1", 1.0) };
        // Two near-identical targets
        var tgt = new[]
        {
            MakeMode("t1", 1.0),
            MakeMode("t2", 1.01),
        };
        var result = tracker.Match(src, tgt);

        Assert.Single(result);
        Assert.True(result[0].IsAmbiguous);
        Assert.NotEmpty(result[0].AmbiguityNotes);
    }

    [Fact]
    public void Match_EigenspaceOverlap_ZeroForOrthogonalVectors()
    {
        var config = new FermionTrackingConfig
        {
            MatchThreshold = 0.1,
            EigenvalueRelTol = 10.0,
            EigenvalueWeight = 0.5,
            ChiralityWeight = 0.5,
            EigenspaceWeight = 0.0,
        };
        var tracker = new FermionModeTracker(config);
        // Orthogonal eigenvectors
        var v1 = new double[] { 1.0, 0.0, 0.0, 0.0 };
        var v2 = new double[] { 0.0, 0.0, 1.0, 0.0 };
        var src = new[] { MakeMode("s1", 1.0, eigvec: v1) };
        var tgt = new[] { MakeMode("t1", 1.0, eigvec: v2) };
        var result = tracker.Match(src, tgt);

        // eigenspace overlap should be ~0 but we have eigenspaceWeight=0 so
        // only verify the computed overlap is near zero
        // We need to use a config with nonzero eigenspace weight to trigger the computation
        var config2 = new FermionTrackingConfig
        {
            MatchThreshold = 0.1,
            EigenvalueRelTol = 10.0,
            EigenvalueWeight = 1.0 / 3,
            ChiralityWeight = 1.0 / 3,
            EigenspaceWeight = 1.0 / 3,
        };
        var tracker2 = new FermionModeTracker(config2);
        var result2 = tracker2.Match(src, tgt);
        Assert.True(result2[0].EigenspaceOverlap.HasValue);
        Assert.True(result2[0].EigenspaceOverlap!.Value < 1e-10);
    }
}
