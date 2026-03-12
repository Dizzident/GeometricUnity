using Gu.Core;
using Gu.Phase4.Fermions;
using Gu.Phase4.Spin;

namespace Gu.Phase4.Fermions.Tests;

/// <summary>
/// Tests for DiscreteFermionState, DiscreteDualFermionState, and their factory methods.
/// </summary>
public class DiscreteFermionStateTests
{
    private static ProvenanceMeta TestProvenance() => new()
    {
        CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
        CodeRevision = "test-rev",
        Branch = new BranchRef { BranchId = "test-branch", SchemaVersion = "1.0.0" },
        Backend = "cpu-reference",
    };

    private static SpinorRepresentationSpec Dim4Spec() => new()
    {
        SpinorSpecId = "spinor-dim4-v1",
        SpacetimeDimension = 4,
        CliffordSignature = new CliffordSignature { Positive = 4, Negative = 0 },
        GammaConvention = new GammaConventionSpec
        {
            ConventionId = "dirac-tensor-product-v1",
            Signature = new CliffordSignature { Positive = 4, Negative = 0 },
            Representation = "standard",
            SpinorDimension = 4,
            HasChirality = true,
        },
        ChiralityConvention = new ChiralityConventionSpec
        {
            ConventionId = "chirality-standard-v1",
            SignConvention = "left-is-minus",
            PhaseFactor = "-1",
            HasChirality = true,
        },
        ConjugationConvention = new ConjugationConventionSpec
        {
            ConventionId = "hermitian-v1",
            ConjugationType = "hermitian",
            HasChargeConjugation = true,
        },
        SpinorComponents = 4,
        ChiralitySplit = 2,
        Provenance = TestProvenance(),
    };

    private static FermionFieldLayout MakeLayout() =>
        FermionFieldLayoutFactory.BuildStandardLayout(
            "layout-dim4-test",
            Dim4Spec(),
            gaugeDimension: 1,
            TestProvenance());

    [Fact]
    public void CreateZeroState_HasCorrectShape()
    {
        var layout = MakeLayout();
        int cellCount = 6;
        var state = FermionFieldLayoutFactory.CreateZeroState("s-zero", layout, cellCount, TestProvenance());

        Assert.Equal(cellCount, state.CellCount);
        Assert.Equal(layout.PrimalDofsPerCell, state.DofsPerCell);
        Assert.Equal(2 * cellCount * layout.PrimalDofsPerCell, state.Coefficients.Length);
    }

    [Fact]
    public void CreateZeroState_AllCoefficientsAreZero()
    {
        var layout = MakeLayout();
        var state = FermionFieldLayoutFactory.CreateZeroState("s-zero", layout, 5, TestProvenance());

        Assert.All(state.Coefficients, c => Assert.Equal(0.0, c));
    }

    [Fact]
    public void GetRe_GetIm_ReturnsCorrectComponents()
    {
        var layout = MakeLayout();
        int cellCount = 2;
        int dofs = layout.PrimalDofsPerCell;
        var coeffs = new double[2 * cellCount * dofs];

        // Set cell=1, dofIndex=2 to (3.0 + 4.0i)
        int idx = 2 * (1 * dofs + 2);
        coeffs[idx]     = 3.0;
        coeffs[idx + 1] = 4.0;

        var state = new DiscreteFermionState
        {
            StateId = "s-test",
            LayoutId = layout.LayoutId,
            CellCount = cellCount,
            DofsPerCell = dofs,
            Coefficients = coeffs,
            Provenance = TestProvenance(),
        };

        Assert.Equal(3.0, state.GetRe(1, 2));
        Assert.Equal(4.0, state.GetIm(1, 2));
        Assert.Equal(0.0, state.GetRe(0, 0));
    }

    [Fact]
    public void L2Norm_IsCorrect()
    {
        var layout = MakeLayout();
        int cellCount = 1;
        int dofs = layout.PrimalDofsPerCell;
        var coeffs = new double[2 * cellCount * dofs];
        // Set first component to (3 + 4i), norm contribution = 5
        coeffs[0] = 3.0;
        coeffs[1] = 4.0;

        var state = new DiscreteFermionState
        {
            StateId = "s-norm",
            LayoutId = layout.LayoutId,
            CellCount = cellCount,
            DofsPerCell = dofs,
            Coefficients = coeffs,
            Provenance = TestProvenance(),
        };

        // L2 norm = sqrt(3^2 + 4^2) = 5
        Assert.Equal(5.0, state.L2Norm(), 10);
    }

    [Fact]
    public void ComplexDofCount_IsCorrect()
    {
        var layout = MakeLayout();
        int cellCount = 7;
        var state = FermionFieldLayoutFactory.CreateZeroState("s", layout, cellCount, TestProvenance());
        Assert.Equal(cellCount * layout.PrimalDofsPerCell, state.ComplexDofCount);
    }

    [Fact]
    public void CreateHermitianDual_NegatesImaginaryPart()
    {
        var layout = MakeLayout();
        int dofs = layout.PrimalDofsPerCell;
        var coeffs = new double[2 * dofs];
        coeffs[0] = 1.0;  coeffs[1] = 2.0;  // (1 + 2i)
        coeffs[2] = -3.0; coeffs[3] = 4.0;  // (-3 + 4i)

        var primal = new DiscreteFermionState
        {
            StateId = "p",
            LayoutId = layout.LayoutId,
            CellCount = 1,
            DofsPerCell = dofs,
            Coefficients = coeffs,
            Provenance = TestProvenance(),
        };

        var dual = FermionFieldLayoutFactory.CreateHermitianDual(primal, TestProvenance());

        Assert.Equal("hermitian", dual.ConjugationType);
        Assert.Equal(primal.StateId, dual.PrimalStateId);

        // Re unchanged, Im negated
        Assert.Equal( 1.0, dual.Coefficients[0], 14);
        Assert.Equal(-2.0, dual.Coefficients[1], 14);
        Assert.Equal(-3.0, dual.Coefficients[2], 14);
        Assert.Equal(-4.0, dual.Coefficients[3], 14);
    }

    [Fact]
    public void BilinearPairing_HermitianSelfPairing_IsRealAndNonNegative()
    {
        // For psi_bar = psi^dagger: <psi^dagger, psi> = ||psi||^2 (real, non-negative)
        var layout = MakeLayout();
        int dofs = layout.PrimalDofsPerCell;
        var coeffs = new double[2 * dofs];
        var rng = new Random(42);
        for (int i = 0; i < coeffs.Length; i++)
            coeffs[i] = rng.NextDouble() * 2 - 1;

        var primal = new DiscreteFermionState
        {
            StateId = "p",
            LayoutId = layout.LayoutId,
            CellCount = 1,
            DofsPerCell = dofs,
            Coefficients = coeffs,
            Provenance = TestProvenance(),
        };
        var dual = FermionFieldLayoutFactory.CreateHermitianDual(primal, TestProvenance());
        var (re, im) = dual.BilinearPairing(primal);

        Assert.True(re >= 0, $"<psi^dagger, psi> must be non-negative, got {re}");
        Assert.Equal(0.0, im, 12); // imaginary part must be zero
    }

    [Fact]
    public void BilinearPairing_DimensionMismatch_Throws()
    {
        var layout = MakeLayout();
        var primal1 = FermionFieldLayoutFactory.CreateZeroState("p1", layout, 2, TestProvenance());
        var primal2 = FermionFieldLayoutFactory.CreateZeroState("p2", layout, 3, TestProvenance());
        var dual = FermionFieldLayoutFactory.CreateHermitianDual(primal1, TestProvenance());

        Assert.Throws<ArgumentException>(() => dual.BilinearPairing(primal2));
    }

    [Fact]
    public void FermionState_SerializesAndRoundTrips()
    {
        var layout = MakeLayout();
        var state = FermionFieldLayoutFactory.CreateZeroState("s-rt", layout, 3, TestProvenance());

        var json = FermionStateSerializer.ToJson(state);
        Assert.Contains("s-rt", json);

        var loaded = FermionStateSerializer.FermionStateFromJson(json);
        Assert.Equal(state.StateId, loaded.StateId);
        Assert.Equal(state.CellCount, loaded.CellCount);
        Assert.Equal(state.DofsPerCell, loaded.DofsPerCell);
        Assert.Equal(state.Coefficients.Length, loaded.Coefficients.Length);
    }
}
