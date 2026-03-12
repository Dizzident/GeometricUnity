using Gu.Core;
using Gu.Phase4.Couplings;
using Gu.Phase4.Dirac;
using Gu.Phase4.Fermions;
using Xunit;

namespace Gu.Phase4.Couplings.Tests;

public sealed class CouplingProxyEngineTests
{
    private static ProvenanceMeta TestProvenance() => new()
    {
        CreatedAt = DateTimeOffset.UtcNow,
        CodeRevision = "test",
        Branch = new BranchRef { BranchId = "test-branch", SchemaVersion = "1.0.0" },
        Backend = "cpu-reference",
    };

    private static FermionModeRecord MakeMode(string id, double[] eigvec) => new()
    {
        ModeId = id,
        BackgroundId = "bg-001",
        BranchVariantId = "v0",
        LayoutId = "layout-001",
        ModeIndex = 0,
        EigenvalueRe = 0.1,
        EigenvalueIm = 0.0,
        ResidualNorm = 0.001,
        EigenvectorCoefficients = eigvec,
        ChiralityDecomposition = new ChiralityDecompositionRecord
        {
            LeftFraction = 0.5,
            RightFraction = 0.5,
            MixedFraction = 0.0,
            SignConvention = "left-is-minus",
        },
        ConjugationPairing = new ConjugationPairingRecord
        {
            HasPair = false,
            ConjugationType = "charge-conjugation",
        },
        Backend = "cpu-reference",
        Provenance = TestProvenance(),
    };

    [Fact]
    public void ComputeCoupling_IdentityVariation_ReturnsDotProduct()
    {
        // delta_D = I (identity matrix, 2x2 real)
        // phi_i = [1, 0, 0, 0], phi_j = [1, 0, 0, 0]  (complex interleaved, dim=2)
        // g = <phi_i, I phi_j> = 1
        var engine = new CouplingProxyEngine(new CpuDiracOperatorAssembler());

        double[,] identity = { { 1.0, 0.0 }, { 0.0, 1.0 } };
        // phi_i and phi_j: 2 complex components = 4 doubles (Re,Im interleaved)
        double[] phi = { 1.0, 0.0, 0.0, 0.0 };
        var modeI = MakeMode("mode-i", phi);
        var modeJ = MakeMode("mode-j", phi);

        var result = engine.ComputeCoupling(
            modeI, modeJ, "boson-001",
            identity, null,
            "raw",
            TestProvenance());

        Assert.Equal("coupling-boson-001-mode-i-mode-j", result.CouplingId);
        Assert.Equal("boson-001", result.BosonModeId);
        Assert.Equal("mode-i", result.FermionModeIdI);
        Assert.Equal("mode-j", result.FermionModeIdJ);
        Assert.Equal(1.0, result.CouplingProxyReal, precision: 10);
        Assert.Equal(0.0, result.CouplingProxyImaginary, precision: 10);
        Assert.Equal(1.0, result.CouplingProxyMagnitude, precision: 10);
        Assert.Equal("raw", result.NormalizationConvention);
    }

    [Fact]
    public void ComputeCoupling_ZeroVariation_ReturnZeroCoupling()
    {
        var engine = new CouplingProxyEngine(new CpuDiracOperatorAssembler());

        double[,] zero = { { 0.0, 0.0 }, { 0.0, 0.0 } };
        double[] phi = { 1.0, 0.0, 0.0, 0.0 };
        var modeI = MakeMode("mode-i", phi);
        var modeJ = MakeMode("mode-j", phi);

        var result = engine.ComputeCoupling(
            modeI, modeJ, "boson-z",
            zero, null, "raw", TestProvenance());

        Assert.Equal(0.0, result.CouplingProxyMagnitude, precision: 15);
    }

    [Fact]
    public void ComputeCoupling_NullEigenvector_ReturnsZeroWithNote()
    {
        var engine = new CouplingProxyEngine(new CpuDiracOperatorAssembler());

        double[,] identity = { { 1.0, 0.0 }, { 0.0, 1.0 } };
        var modeI = MakeMode("mode-i", null!);
        var modeJ = MakeMode("mode-j", new double[] { 1.0, 0.0, 0.0, 0.0 });

        var result = engine.ComputeCoupling(
            modeI, modeJ, "boson-x",
            identity, null, "raw", TestProvenance());

        Assert.Equal(0.0, result.CouplingProxyMagnitude, precision: 15);
        Assert.NotEmpty(result.SelectionRuleNotes);
    }

    [Fact]
    public void ComputeCoupling_ComplexVariation_CorrectImaginaryPart()
    {
        // delta_D = [[0, -i], [i, 0]] (imaginary off-diagonal)
        // phi_i = [1, 0, 0, 0], phi_j = [0, 0, 1, 0]  (e_0 and e_1 as complex vecs)
        // delta_D * phi_j = column 1 of delta_D = [Im part = -i * 1 ...]
        var engine = new CouplingProxyEngine(new CpuDiracOperatorAssembler());

        double[,] reMatrix = { { 0.0, 0.0 }, { 0.0, 0.0 } };
        double[,] imMatrix = { { 0.0, -1.0 }, { 1.0, 0.0 } };
        // phi_i = (1+0i, 0+0i), phi_j = (0+0i, 1+0i)
        double[] phiI = { 1.0, 0.0, 0.0, 0.0 };
        double[] phiJ = { 0.0, 0.0, 1.0, 0.0 };
        var modeI = MakeMode("mode-i", phiI);
        var modeJ = MakeMode("mode-j", phiJ);

        var result = engine.ComputeCoupling(
            modeI, modeJ, "boson-c",
            reMatrix, imMatrix, "raw", TestProvenance());

        // delta_D * phi_j: component 0 = (0 + i*(-1))*1 = -i, component 1 = (0 + i*1)*1 = i
        // <phi_i, delta_D phi_j> = conj(1,0)*(-i) + conj(0,0)*(i) = -i
        Assert.Equal(0.0, result.CouplingProxyReal, precision: 10);
        Assert.Equal(-1.0, result.CouplingProxyImaginary, precision: 10);
        Assert.Equal(1.0, result.CouplingProxyMagnitude, precision: 10);
    }

    [Fact]
    public void ComputeCoupling_UnitModesNormalization_NormalizesFirst()
    {
        var engine = new CouplingProxyEngine(new CpuDiracOperatorAssembler());

        double[,] identity = { { 1.0, 0.0 }, { 0.0, 1.0 } };
        // phi with norm = 2
        double[] phi = { 2.0, 0.0, 0.0, 0.0 };
        var modeI = MakeMode("mode-i", phi);
        var modeJ = MakeMode("mode-j", phi);

        var rawResult = engine.ComputeCoupling(
            modeI, modeJ, "boson-001", identity, null, "raw", TestProvenance());
        var normResult = engine.ComputeCoupling(
            modeI, modeJ, "boson-001", identity, null, "unit-modes", TestProvenance());

        // raw: g = 4 (dot product of [2,0,0,0] with itself through identity)
        Assert.Equal(4.0, rawResult.CouplingProxyReal, precision: 10);
        // unit-modes: g = 1 (each vector normalized to length 1 first)
        Assert.Equal(1.0, normResult.CouplingProxyReal, precision: 10);
    }

    [Fact]
    public void ComputeVariationMatrix_FiniteDifference_CorrectResult()
    {
        // D_base = [[1, 0],[0, 1]], D_perturbed = [[1+eps, 0],[0, 1]]
        // delta_D = [[1, 0],[0, 0]]
        double eps = 1e-5;
        double[,] baseRe = { { 1.0, 0.0 }, { 0.0, 1.0 } };
        double[,] basIm = { { 0.0, 0.0 }, { 0.0, 0.0 } };
        double[,] pertRe = { { 1.0 + eps, 0.0 }, { 0.0, 1.0 } };
        double[,] pertIm = { { 0.0, 0.0 }, { 0.0, 0.0 } };

        var (re, im) = CouplingProxyEngine.ComputeVariationMatrix(baseRe, basIm, pertRe, pertIm, eps);

        Assert.Equal(1.0, re[0, 0], precision: 8);
        Assert.Equal(0.0, re[0, 1], precision: 8);
        Assert.Equal(0.0, re[1, 0], precision: 8);
        Assert.Equal(0.0, re[1, 1], precision: 8);
        Assert.Equal(0.0, im[0, 0], precision: 8);
    }

    [Fact]
    public void ComputeVariationMatrix_ZeroEpsilonThrows()
    {
        double[,] m = { { 1.0 } };
        double[,] mIm = { { 0.0 } };
        Assert.Throws<ArgumentException>(() =>
            CouplingProxyEngine.ComputeVariationMatrix(m, mIm, m, mIm, 0.0));
    }

    [Fact]
    public void BuildAtlas_SingleBosonPair_PopulatesAtlas()
    {
        var engine = new CouplingProxyEngine(new CpuDiracOperatorAssembler());

        double[,] identity = { { 1.0, 0.0 }, { 0.0, 1.0 } };
        double[] phi = { 1.0, 0.0, 0.0, 0.0 };
        var modes = new List<FermionModeRecord>
        {
            MakeMode("f-0", phi),
            MakeMode("f-1", new double[] { 0.0, 0.0, 1.0, 0.0 }),
        };

        var varMatrices = new Dictionary<string, (double[,] Re, double[,]? Im)>
        {
            ["boson-1"] = (identity, null),
        };

        var atlas = engine.BuildAtlas(
            "atlas-001", "bg-001",
            modes, varMatrices,
            "raw", "registry-v1", TestProvenance());

        Assert.Equal("atlas-001", atlas.AtlasId);
        Assert.Equal("bg-001", atlas.FermionBackgroundId);
        Assert.Equal("registry-v1", atlas.BosonRegistryVersion);
        // 1 boson * 2 * 2 = 4 couplings
        Assert.Equal(4, atlas.Couplings.Count);
        Assert.All(atlas.Couplings, c => Assert.Equal("raw", c.NormalizationConvention));
    }

    [Fact]
    public void BuildAtlas_MultipleBosonModes_AllRecorded()
    {
        var engine = new CouplingProxyEngine(new CpuDiracOperatorAssembler());

        double[,] identity = { { 1.0, 0.0 }, { 0.0, 1.0 } };
        double[] phi = { 1.0, 0.0, 0.0, 0.0 };
        var modes = new List<FermionModeRecord> { MakeMode("f-0", phi) };

        var varMatrices = new Dictionary<string, (double[,] Re, double[,]? Im)>
        {
            ["b-1"] = (identity, null),
            ["b-2"] = (identity, null),
            ["b-3"] = (identity, null),
        };

        var atlas = engine.BuildAtlas(
            "atlas-002", "bg-001",
            modes, varMatrices,
            "unit-modes", "registry-v1", TestProvenance());

        // 3 bosons * 1 * 1 = 3 couplings
        Assert.Equal(3, atlas.Couplings.Count);
        Assert.Contains(atlas.Couplings, c => c.BosonModeId == "b-1");
        Assert.Contains(atlas.Couplings, c => c.BosonModeId == "b-2");
        Assert.Contains(atlas.Couplings, c => c.BosonModeId == "b-3");
    }

    [Fact]
    public void BuildAtlas_EmptyModes_EmptyCouplings()
    {
        var engine = new CouplingProxyEngine(new CpuDiracOperatorAssembler());

        double[,] identity = { { 1.0, 0.0 }, { 0.0, 1.0 } };
        var modes = new List<FermionModeRecord>();
        var varMatrices = new Dictionary<string, (double[,] Re, double[,]? Im)>
        {
            ["b-1"] = (identity, null),
        };

        var atlas = engine.BuildAtlas(
            "atlas-003", "bg-001",
            modes, varMatrices,
            "raw", "registry-v1", TestProvenance());

        Assert.Empty(atlas.Couplings);
    }

    [Fact]
    public void ComputeCoupling_MagnitudeIsNonNegative()
    {
        var engine = new CouplingProxyEngine(new CpuDiracOperatorAssembler());

        double[,] m = { { 0.5, 0.3 }, { -0.3, 0.5 } };
        double[] phi = { 1.0, 0.0, 0.0, 0.0 };
        var modeI = MakeMode("mode-i", phi);
        var modeJ = MakeMode("mode-j", phi);

        var result = engine.ComputeCoupling(
            modeI, modeJ, "b-1", m, null, "raw", TestProvenance());

        Assert.True(result.CouplingProxyMagnitude >= 0.0);
        Assert.Equal(
            System.Math.Sqrt(result.CouplingProxyReal * result.CouplingProxyReal
                + result.CouplingProxyImaginary * result.CouplingProxyImaginary),
            result.CouplingProxyMagnitude, precision: 10);
    }

    [Fact]
    public void CouplingRecord_HasCorrectIds()
    {
        var engine = new CouplingProxyEngine(new CpuDiracOperatorAssembler());
        double[,] m = { { 1.0 } };
        double[] phi = { 1.0, 0.0 };
        var modeI = MakeMode("fermion-0", phi);
        var modeJ = MakeMode("fermion-1", phi);

        var result = engine.ComputeCoupling(
            modeI, modeJ, "bos-xyz", m, null, "raw", TestProvenance());

        Assert.Equal("fermion-0", result.FermionModeIdI);
        Assert.Equal("fermion-1", result.FermionModeIdJ);
        Assert.Equal("bos-xyz", result.BosonModeId);
    }
}
