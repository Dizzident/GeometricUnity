using Gu.Core;
using Gu.Phase4.Chirality;
using Gu.Phase4.Fermions;
using Gu.Phase4.Spin;

namespace Gu.Phase4.Chirality.Tests;

/// <summary>
/// Tests for ConjugationAnalyzer (M37).
/// </summary>
public class ConjugationAnalyzerTests
{
    private static ProvenanceMeta TestProvenance() => new()
    {
        CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
        CodeRevision = "test-m37-conj",
        Branch = new BranchRef { BranchId = "test", SchemaVersion = "1.0.0" },
        Backend = "cpu-reference",
    };

    private static ConjugationConventionSpec HermitianConvention() => new()
    {
        ConventionId = "h1",
        ConjugationType = "hermitian",
        HasChargeConjugation = true,
    };

    private static FermionModeRecord MakeMode(string id, double[] phi, double eigenvalue) =>
        new FermionModeRecord
        {
            ModeId = id,
            BackgroundId = "bg",
            BranchVariantId = "bv",
            LayoutId = "l1",
            ModeIndex = 0,
            EigenvalueRe = eigenvalue,
            EigenvalueIm = 0.0,
            ResidualNorm = 1e-8,
            EigenvectorCoefficients = phi,
            ChiralityDecomposition = new ChiralityDecompositionRecord
            {
                LeftFraction = 0.5, RightFraction = 0.5, MixedFraction = 0.0,
                SignConvention = "left-is-minus",
            },
            ConjugationPairing = new ConjugationPairingRecord
            {
                HasPair = false, ConjugationType = "hermitian",
            },
            Backend = "cpu-reference",
            Provenance = TestProvenance(),
        };

    [Fact]
    public void FindPairs_ConjugatePair_Detected()
    {
        // Two real modes: phi_A = [1,0,0,0] with eigenvalue +0.5
        //                 phi_B = [1,0,0,0] with eigenvalue -0.5
        // C*phi_A = phi_A* = [1,0,0,0] (real vector, same)
        // overlap = 1.0
        var phiA = new double[] { 1.0, 0.0, 0.0, 0.0 };
        var phiB = new double[] { 1.0, 0.0, 0.0, 0.0 };
        var modeA = MakeMode("mA", phiA, +0.5);
        var modeB = MakeMode("mB", phiB, -0.5);
        var conv = HermitianConvention();
        var analyzer = new ConjugationAnalyzer();

        var pairs = analyzer.FindPairs(new[] { modeA, modeB }, conv,
            gammas: null!, overlapThreshold: 0.8, eigenvalueTolerance: 0.1);

        Assert.Single(pairs);
        Assert.Equal("mA", pairs[0].ModeIdA);
        Assert.Equal("mB", pairs[0].ModeIdB);
        Assert.True(pairs[0].IsConfident);
    }

    [Fact]
    public void FindPairs_NoConjugate_EmptyResult()
    {
        // All modes have same eigenvalue sign — no conjugate pairs
        var phi = new double[] { 1.0, 0.0, 0.0, 0.0 };
        var modes = new[]
        {
            MakeMode("m1", phi, +0.5),
            MakeMode("m2", phi, +0.7),
            MakeMode("m3", phi, +1.0),
        };
        var analyzer = new ConjugationAnalyzer();

        var pairs = analyzer.FindPairs(modes, HermitianConvention(),
            gammas: null!, overlapThreshold: 0.8, eigenvalueTolerance: 0.05);

        Assert.Empty(pairs);
    }

    [Fact]
    public void FindPairs_PairScore_IsHighForIdenticalRealVectors()
    {
        var phi = new double[] { 0.6, 0.0, 0.8, 0.0 }; // real, normalized
        var modeA = MakeMode("mA", phi, +1.0);
        var modeB = MakeMode("mB", phi, -1.0);
        var analyzer = new ConjugationAnalyzer();

        var pairs = analyzer.FindPairs(new[] { modeA, modeB }, HermitianConvention(),
            gammas: null!, overlapThreshold: 0.8, eigenvalueTolerance: 0.5);

        Assert.Single(pairs);
        Assert.True(pairs[0].OverlapScore > 0.99, $"Expected overlap > 0.99, got {pairs[0].OverlapScore}");
    }

    [Fact]
    public void FindPairs_EigenvaluePreserved()
    {
        var phi = new double[] { 1.0, 0.0, 0.0, 0.0 };
        var modeA = MakeMode("mA", phi, +2.5);
        var modeB = MakeMode("mB", phi, -2.5);
        var analyzer = new ConjugationAnalyzer();

        var pairs = analyzer.FindPairs(new[] { modeA, modeB }, HermitianConvention(),
            gammas: null!, overlapThreshold: 0.8, eigenvalueTolerance: 0.1);

        Assert.Single(pairs);
        Assert.Equal(+2.5, pairs[0].EigenvalueA, 8);
        Assert.Equal(-2.5, pairs[0].EigenvalueB, 8);
        Assert.Equal(-2.5, pairs[0].ExpectedConjugateEigenvalue, 8); // hermitian: expected = -lambda_A
    }

    [Fact]
    public void FindPairs_ModesWithoutEigenvectors_Skipped()
    {
        var phi = new double[] { 1.0, 0.0, 0.0, 0.0 };
        var modeWithVec = MakeMode("mA", phi, +0.5);
        var modeNoVec = new FermionModeRecord
        {
            ModeId = "mB",
            BackgroundId = "bg", BranchVariantId = "bv", LayoutId = "l1",
            ModeIndex = 1,
            EigenvalueRe = -0.5, EigenvalueIm = 0.0, ResidualNorm = 0.0,
            EigenvectorCoefficients = null, // no vector
            ChiralityDecomposition = new ChiralityDecompositionRecord
            {
                LeftFraction = 0.5, RightFraction = 0.5, MixedFraction = 0.0,
                SignConvention = "left-is-minus",
            },
            ConjugationPairing = new ConjugationPairingRecord
            {
                HasPair = false, ConjugationType = "hermitian",
            },
            Backend = "cpu-reference",
            Provenance = TestProvenance(),
        };
        var analyzer = new ConjugationAnalyzer();

        var pairs = analyzer.FindPairs(new[] { modeWithVec, modeNoVec }, HermitianConvention(),
            gammas: null!, overlapThreshold: 0.8);

        Assert.Empty(pairs);
    }
}
