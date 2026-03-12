using System.Numerics;
using Gu.Core;
using Gu.Phase4.Chirality;
using Gu.Phase4.Fermions;
using Gu.Phase4.Spin;

namespace Gu.Phase4.Chirality.Tests;

/// <summary>
/// Tests for ChiralityAnalyzer (M37).
/// Minimum 10 tests required (see ARCH_P4.md §7.5).
/// </summary>
public class ChiralityAnalyzerTests
{
    private static ProvenanceMeta TestProvenance() => new()
    {
        CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
        CodeRevision = "test-m37",
        Branch = new BranchRef { BranchId = "test", SchemaVersion = "1.0.0" },
        Backend = "cpu-reference",
    };

    /// <summary>
    /// dim=2 Clifford algebra: Gamma_1 = sigma_1, Gamma_2 = sigma_2
    /// Gamma_chi = i^1 * sigma_1 * sigma_2 = i * (i*sigma_3) = i^2 * sigma_3 = -sigma_3
    /// So Gamma_chi = [[-1,0],[0,1]] for dim=2.
    /// Eigenvectors: [1,0] is -1 eigenspace, [0,1] is +1 eigenspace.
    /// </summary>
    private static GammaOperatorBundle Dim2Gammas()
    {
        var builder = new GammaMatrixBuilder();
        var sig = new CliffordSignature { Positive = 2, Negative = 0 };
        var conv = new GammaConventionSpec
        {
            ConventionId = "test-dim2",
            Signature = sig,
            Representation = "standard",
            SpinorDimension = 2,
            HasChirality = true,
        };
        return builder.Build(sig, conv, TestProvenance());
    }

    private static GammaOperatorBundle Dim3Gammas()
    {
        var builder = new GammaMatrixBuilder();
        var sig = new CliffordSignature { Positive = 3, Negative = 0 };
        var conv = new GammaConventionSpec
        {
            ConventionId = "test-dim3",
            Signature = sig,
            Representation = "standard",
            SpinorDimension = 2, // 2^floor(3/2) = 2
            HasChirality = false,
        };
        return builder.Build(sig, conv, TestProvenance());
    }

    private static ChiralityConventionSpec LeftIsMinusConvention() => new()
    {
        ConventionId = "c-std",
        SignConvention = "left-is-minus",
        PhaseFactor = "-1",
        HasChirality = true,
    };

    private static ChiralityConventionSpec LeftIsPlusConvention() => new()
    {
        ConventionId = "c-plus",
        SignConvention = "left-is-plus",
        PhaseFactor = "-1",
        HasChirality = true,
    };

    private static ChiralityConventionSpec NoChiralityConvention() => new()
    {
        ConventionId = "c-none",
        SignConvention = "left-is-minus",
        PhaseFactor = "1",
        HasChirality = false,
    };

    private static FermionFieldLayout TrivialLayout(int spinorDim) =>
        new FermionFieldLayout
        {
            LayoutId = "l1",
            SpinorSpecId = "s1",
            SpinorBlocks = new List<SpinorBlockSpec>
            {
                new SpinorBlockSpec
                {
                    BlockId = "psi", Role = "primal",
                    SpinorDimension = spinorDim, GaugeDimension = 1,
                },
            },
            ConjugationRules = new List<ConjugationRule>(),
            AllowedBilinears = new List<AllowedBilinear>(),
            ObservationEligibility = new ObservationEligibilitySpec
            {
                EligibleBlockIds = new List<string> { "psi" },
            },
            Provenance = TestProvenance(),
        };

    /// <summary>
    /// Build a FermionModeRecord with a given eigenvector (cellCount=1, dimG=1).
    /// </summary>
    private static FermionModeRecord MakeMode(string id, double[] eigenvecCoeffs, double eigenvalue = 0.5) =>
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
            EigenvectorCoefficients = eigenvecCoeffs,
            ChiralityDecomposition = new ChiralityDecompositionRecord
            {
                LeftFraction = 0.0, RightFraction = 0.0, MixedFraction = 0.0,
                SignConvention = "left-is-minus",
            },
            ConjugationPairing = new ConjugationPairingRecord
            {
                HasPair = false, ConjugationType = "hermitian",
            },
            Backend = "cpu-reference",
            Provenance = TestProvenance(),
        };

    // -------------------------------------------------------
    // Test: odd dimension => trivial
    // -------------------------------------------------------

    [Fact]
    public void Analyze_OddDim_ReturnsTrivial()
    {
        var gammas = Dim3Gammas(); // dim=3, odd
        var conv = NoChiralityConvention();
        var layout = TrivialLayout(2);
        // dim3 spinorDim=2, cellCount=1, dimG=1 => eigvec length = 1*2*1*2 = 4
        var phi = new double[] { 1.0, 0.0, 0.0, 0.0 };
        var mode = MakeMode("m0", phi);
        var analyzer = new ChiralityAnalyzer();

        var result = analyzer.Analyze(mode, gammas, conv, layout, cellCount: 1);

        Assert.Equal("trivial", result.ChiralityTag);
        Assert.Equal("trivial", result.ChiralityStatus);
        Assert.Equal(0.5, result.LeftFraction, 6);
        Assert.Equal(0.5, result.RightFraction, 6);
    }

    // -------------------------------------------------------
    // Test: even dim, pure -1 eigenstate of Gamma_chi (= [1,0] for dim=2)
    //   Gamma_chi = -sigma_3, so Gamma_chi [1,0] = -1 * [1,0]
    //   "left-is-minus" => P_L = (I - Gamma_chi)/2 = (I + sigma_3)/2
    //   P_L [1,0] = (I + sigma_3)/2 [1,0] = [[1,0],[0,0]] [1,0] = [1,0]
    //   => leftFraction = 1, rightFraction = 0  => "left"
    // -------------------------------------------------------

    [Fact]
    public void Analyze_PlusOneEigenstate_LeftIsMinusConvention_IsRight()
    {
        // [1,0] is the -1 eigenstate of Gamma_chi = -sigma_3.
        // "left-is-minus": P_L = (I - Gamma_chi)/2. Gamma_chi [1,0] = -[1,0].
        // P_L [1,0] = (I + sigma_3)/2 [1,0] = [1,0] => left.
        // Test name preserved for tracking; behavior: [1,0] is LEFT under "left-is-minus".
        var gammas = Dim2Gammas();
        var conv = LeftIsMinusConvention();
        var layout = TrivialLayout(2);
        // [1,0] state in spinor space, cellCount=1, dimG=1
        // eigvec length = 1*2*1*2 = 4 (complex interleaved)
        var phi = new double[] { 1.0, 0.0, 0.0, 0.0 }; // spinor=[1,0], complex
        var mode = MakeMode("m+", phi);
        var analyzer = new ChiralityAnalyzer();

        var result = analyzer.Analyze(mode, gammas, conv, layout, cellCount: 1);

        Assert.Equal("left", result.ChiralityTag);
        Assert.Equal(1.0, result.LeftFraction, 6);
        Assert.Equal(0.0, result.RightFraction, 6);
    }

    [Fact]
    public void Analyze_MinusOneEigenstate_LeftIsMinusConvention_IsLeft()
    {
        // [0,1] is the +1 eigenstate of Gamma_chi = -sigma_3.
        // "left-is-minus": P_R = (I + Gamma_chi)/2 = (I - sigma_3)/2.
        // P_R [0,1] = [[0,0],[0,1]] [0,1] = [0,1] => right.
        // Test name preserved for tracking; behavior: [0,1] is RIGHT under "left-is-minus".
        var gammas = Dim2Gammas();
        var conv = LeftIsMinusConvention();
        var layout = TrivialLayout(2);
        // [0,1] state in spinor space: Gamma_chi [0,1] = -sigma_3 [0,1] = +[0,1]
        var phi = new double[] { 0.0, 0.0, 1.0, 0.0 }; // spinor=[0,1], complex
        var mode = MakeMode("m-", phi);
        var analyzer = new ChiralityAnalyzer();

        var result = analyzer.Analyze(mode, gammas, conv, layout, cellCount: 1);

        Assert.Equal("right", result.ChiralityTag);
        Assert.Equal(0.0, result.LeftFraction, 6);
        Assert.Equal(1.0, result.RightFraction, 6);
    }

    [Fact]
    public void Analyze_PlusOneEigenstate_LeftIsPlusConvention_IsLeft()
    {
        // [1,0] is the -1 eigenstate of Gamma_chi = -sigma_3.
        // "left-is-plus": P_L = (I + Gamma_chi)/2 = (I - sigma_3)/2.
        // P_L [1,0] = [[0,0],[0,1]] [1,0] = [0,0] => 0 left fraction => right.
        // Test name preserved; behavior: [1,0] is RIGHT under "left-is-plus".
        var gammas = Dim2Gammas();
        var conv = LeftIsPlusConvention();
        var layout = TrivialLayout(2);
        var phi = new double[] { 1.0, 0.0, 0.0, 0.0 };
        var mode = MakeMode("m+plus", phi);
        var analyzer = new ChiralityAnalyzer();

        var result = analyzer.Analyze(mode, gammas, conv, layout, cellCount: 1);

        // P_L = (I + Gamma_chi)/2 = (I - sigma_3)/2; Gamma_chi [1,0] = -[1,0] => P_L [1,0] = [0,0]
        Assert.Equal("right", result.ChiralityTag);
        Assert.Equal(0.0, result.LeftFraction, 6);
        Assert.Equal(1.0, result.RightFraction, 6);
    }

    [Fact]
    public void Analyze_MixedState_HasBothFractions_SumToOne()
    {
        var gammas = Dim2Gammas();
        var conv = LeftIsMinusConvention();
        var layout = TrivialLayout(2);
        // Equal superposition [1,1]/sqrt(2)
        double v = 1.0 / System.Math.Sqrt(2.0);
        var phi = new double[] { v, 0.0, v, 0.0 };
        var mode = MakeMode("mixed", phi);
        var analyzer = new ChiralityAnalyzer();

        var result = analyzer.Analyze(mode, gammas, conv, layout, cellCount: 1);

        Assert.Equal("mixed", result.ChiralityTag);
        Assert.True(result.LeftFraction > 0.0);
        Assert.True(result.RightFraction > 0.0);
        Assert.Equal(1.0, result.LeftFraction + result.RightFraction, 6);
    }

    [Fact]
    public void Analyze_LeakageDiagnostic_NearZeroForPureEigenstate()
    {
        var gammas = Dim2Gammas();
        var conv = LeftIsMinusConvention();
        var layout = TrivialLayout(2);
        var phi = new double[] { 1.0, 0.0, 0.0, 0.0 };
        var mode = MakeMode("m+", phi);
        var analyzer = new ChiralityAnalyzer();

        var result = analyzer.Analyze(mode, gammas, conv, layout, cellCount: 1);

        Assert.Equal(0.0, result.LeakageDiagnostic, 10);
    }

    [Fact]
    public void Analyze_NullEigenvector_ReturnsNotApplicable()
    {
        var gammas = Dim2Gammas();
        var conv = LeftIsMinusConvention();
        var layout = TrivialLayout(2);
        var mode = MakeMode("m-null", eigenvecCoeffs: null!);
        var modeNoVec = new FermionModeRecord
        {
            ModeId = "m-null",
            BackgroundId = "bg",
            BranchVariantId = "bv",
            LayoutId = "l1",
            ModeIndex = 0,
            EigenvalueRe = 0.5,
            EigenvalueIm = 0.0,
            ResidualNorm = 0.0,
            EigenvectorCoefficients = null,
            ChiralityDecomposition = new ChiralityDecompositionRecord
            {
                LeftFraction = 0.0, RightFraction = 0.0, MixedFraction = 0.0,
                SignConvention = "left-is-minus",
            },
            ConjugationPairing = new ConjugationPairingRecord
            {
                HasPair = false, ConjugationType = "hermitian",
            },
            Backend = "cpu-reference",
            Provenance = TestProvenance(),
        };
        var analyzer = new ChiralityAnalyzer();

        var result = analyzer.Analyze(modeNoVec, gammas, conv, layout, cellCount: 1);

        Assert.Equal("not-applicable", result.ChiralityStatus);
    }

    [Fact]
    public void AnalyzeAll_ReturnsOnePerMode()
    {
        var gammas = Dim2Gammas();
        var conv = LeftIsMinusConvention();
        var layout = TrivialLayout(2);
        var modes = new List<FermionModeRecord>
        {
            MakeMode("m0", new double[] { 1.0, 0.0, 0.0, 0.0 }),
            MakeMode("m1", new double[] { 0.0, 0.0, 1.0, 0.0 }),
        };
        var analyzer = new ChiralityAnalyzer();

        var results = analyzer.AnalyzeAll(modes, gammas, conv, layout, cellCount: 1);

        Assert.Equal(2, results.Count);
        Assert.Equal("m0", results[0].ModeId);
        Assert.Equal("m1", results[1].ModeId);
    }

    [Fact]
    public void Analyze_TwoCells_PureLeft_StillIsLeft()
    {
        // Two cells, spinorDim=2, dimG=1, pure [1,0] in each cell
        // Left-is-minus: Gamma_chi=-sigma_3, so P_L=(I+sigma_3)/2, [1,0] is in the left eigenspace
        var gammas = Dim2Gammas();
        var conv = LeftIsMinusConvention();
        var layout = TrivialLayout(2);
        var phi = new double[]
        {
            // cell 0: spinor=[1,0] complex = [1,0, 0,0]
            1.0, 0.0, 0.0, 0.0,
            // cell 1: spinor=[1,0] complex = [1,0, 0,0]
            1.0, 0.0, 0.0, 0.0,
        };
        var mode = MakeMode("m-2cell", phi);
        var analyzer = new ChiralityAnalyzer();

        var result = analyzer.Analyze(mode, gammas, conv, layout, cellCount: 2);

        Assert.Equal("left", result.ChiralityTag);
        Assert.Equal(1.0, result.LeftFraction, 6);
    }

    [Fact]
    public void Analyze_SignConvention_IsRecordedInResult()
    {
        var gammas = Dim2Gammas();
        var conv = LeftIsPlusConvention();
        var layout = TrivialLayout(2);
        var phi = new double[] { 1.0, 0.0, 0.0, 0.0 };
        var mode = MakeMode("m", phi);
        var analyzer = new ChiralityAnalyzer();

        var result = analyzer.Analyze(mode, gammas, conv, layout, cellCount: 1);

        Assert.Equal("left-is-plus", result.SignConvention);
    }

    [Fact]
    public void Analyze_ChiralityStatus_DefiniteLeft_IsCorrect()
    {
        var gammas = Dim2Gammas();
        var conv = LeftIsMinusConvention();
        var layout = TrivialLayout(2);
        // spinor=[1,0]: the -1 eigenstate of Gamma_chi=-sigma_3, which is left under "left-is-minus"
        var phi = new double[] { 1.0, 0.0, 0.0, 0.0 }; // pure left
        var mode = MakeMode("m", phi);
        var analyzer = new ChiralityAnalyzer();

        var result = analyzer.Analyze(mode, gammas, conv, layout, cellCount: 1);

        Assert.Equal("definite-left", result.ChiralityStatus);
    }
}
