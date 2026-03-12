using Gu.Core;
using Gu.Phase4.Fermions;
using Gu.Phase4.Spin;

namespace Gu.Phase4.Fermions.Tests;

/// <summary>
/// Tests for FermionStateBinaryStorage round-trip correctness.
/// </summary>
public class FermionBinaryStorageTests
{
    private static ProvenanceMeta TestProvenance() => new()
    {
        CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
        CodeRevision = "test-rev",
        Branch = new BranchRef { BranchId = "test-branch", SchemaVersion = "1.0.0" },
        Backend = "cpu-reference",
    };

    private static DiscreteFermionState MakeRandomState(int cellCount, int dofsPerCell, int seed = 42)
    {
        var rng = new Random(seed);
        var coeffs = new double[2 * cellCount * dofsPerCell];
        for (int i = 0; i < coeffs.Length; i++)
            coeffs[i] = rng.NextDouble() * 2 - 1;

        return new DiscreteFermionState
        {
            StateId = $"state-{seed}",
            LayoutId = "layout-test",
            CellCount = cellCount,
            DofsPerCell = dofsPerCell,
            Coefficients = coeffs,
            Provenance = TestProvenance(),
        };
    }

    [Fact]
    public void WriteRead_RoundTrip_PreservesCoefficients()
    {
        var state = MakeRandomState(cellCount: 8, dofsPerCell: 4);

        using var ms = new MemoryStream();
        FermionStateBinaryStorage.WriteCoefficients(ms, state);

        ms.Seek(0, SeekOrigin.Begin);
        var loaded = FermionStateBinaryStorage.ReadCoefficients(ms, out int cellCount, out int dofsPerCell);

        Assert.Equal(state.CellCount, cellCount);
        Assert.Equal(state.DofsPerCell, dofsPerCell);
        Assert.Equal(state.Coefficients.Length, loaded.Length);

        for (int i = 0; i < loaded.Length; i++)
            Assert.Equal(state.Coefficients[i], loaded[i], 15);
    }

    [Fact]
    public void ReadCoefficients_BadMagic_Throws()
    {
        using var ms = new MemoryStream(new byte[] { 0x00, 0x01, 0x02, 0x03 });
        Assert.Throws<InvalidDataException>(() =>
        {
            FermionStateBinaryStorage.ReadCoefficients(ms, out _, out _);
        });
    }

    [Fact]
    public void WriteRead_ZeroState_AllZero()
    {
        var state = new DiscreteFermionState
        {
            StateId = "zero",
            LayoutId = "layout-test",
            CellCount = 4,
            DofsPerCell = 2,
            Coefficients = new double[2 * 4 * 2],
            Provenance = TestProvenance(),
        };

        using var ms = new MemoryStream();
        FermionStateBinaryStorage.WriteCoefficients(ms, state);
        ms.Seek(0, SeekOrigin.Begin);
        var loaded = FermionStateBinaryStorage.ReadCoefficients(ms, out _, out _);

        Assert.All(loaded, v => Assert.Equal(0.0, v));
    }

    [Fact]
    public void WriteRead_Eigenvector_RoundTrip()
    {
        var rng = new Random(7);
        var v = new double[32];
        for (int i = 0; i < v.Length; i++)
            v[i] = rng.NextDouble() * 2 - 1;

        var mode = new FermionModeRecord
        {
            ModeId = "mode-ev",
            BackgroundId = "bg-001",
            BranchVariantId = "bv-001",
            LayoutId = "layout-test",
            ModeIndex = 0,
            EigenvalueRe = 1.23,
            EigenvalueIm = 0.0,
            ResidualNorm = 1e-10,
            EigenvectorCoefficients = v,
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

        using var ms = new MemoryStream();
        FermionStateBinaryStorage.WriteEigenvector(ms, mode);
        ms.Seek(0, SeekOrigin.Begin);
        var loaded = FermionStateBinaryStorage.ReadEigenvector(ms);

        Assert.NotNull(loaded);
        Assert.Equal(v.Length, loaded!.Length);
        for (int i = 0; i < v.Length; i++)
            Assert.Equal(v[i], loaded[i], 15);
    }

    [Fact]
    public void WriteRead_NullEigenvector_ReturnsNull()
    {
        var mode = new FermionModeRecord
        {
            ModeId = "mode-no-ev",
            BackgroundId = "bg-001",
            BranchVariantId = "bv-001",
            LayoutId = "layout-test",
            ModeIndex = 0,
            EigenvalueRe = 0.5,
            EigenvalueIm = 0.0,
            ResidualNorm = 1e-8,
            EigenvectorCoefficients = null,
            ChiralityDecomposition = new ChiralityDecompositionRecord
            {
                LeftFraction = 1.0, RightFraction = 0.0, MixedFraction = 0.0,
                SignConvention = "left-is-minus",
            },
            ConjugationPairing = new ConjugationPairingRecord
            {
                HasPair = false, ConjugationType = "hermitian",
            },
            Backend = "cpu-reference",
            Provenance = TestProvenance(),
        };

        using var ms = new MemoryStream();
        FermionStateBinaryStorage.WriteEigenvector(ms, mode);
        ms.Seek(0, SeekOrigin.Begin);
        var loaded = FermionStateBinaryStorage.ReadEigenvector(ms);

        Assert.Null(loaded);
    }

    [Fact]
    public void WriteRead_LargeState_IsAccurate()
    {
        // Test with a state large enough to catch alignment issues
        var state = MakeRandomState(cellCount: 128, dofsPerCell: 16);

        using var ms = new MemoryStream();
        FermionStateBinaryStorage.WriteCoefficients(ms, state);
        ms.Seek(0, SeekOrigin.Begin);
        var loaded = FermionStateBinaryStorage.ReadCoefficients(ms, out int cellCount, out int dofsPerCell);

        Assert.Equal(128, cellCount);
        Assert.Equal(16, dofsPerCell);
        for (int i = 0; i < loaded.Length; i++)
            Assert.Equal(state.Coefficients[i], loaded[i], 15);
    }
}
