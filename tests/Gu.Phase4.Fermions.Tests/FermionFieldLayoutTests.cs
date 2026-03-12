using Gu.Core;
using Gu.Phase4.Fermions;
using Gu.Phase4.Spin;

namespace Gu.Phase4.Fermions.Tests;

/// <summary>
/// Tests for FermionFieldLayout construction, serialization, and round-trip.
/// </summary>
public class FermionFieldLayoutTests
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

    private static SpinorRepresentationSpec Dim5Spec() => new()
    {
        SpinorSpecId = "spinor-dim5-v1",
        SpacetimeDimension = 5,
        CliffordSignature = new CliffordSignature { Positive = 5, Negative = 0 },
        GammaConvention = new GammaConventionSpec
        {
            ConventionId = "dirac-tensor-product-v1",
            Signature = new CliffordSignature { Positive = 5, Negative = 0 },
            Representation = "standard",
            SpinorDimension = 4, // 2^floor(5/2) = 4
            HasChirality = false,
        },
        ChiralityConvention = new ChiralityConventionSpec
        {
            ConventionId = "chirality-none-v1",
            SignConvention = "left-is-minus",
            PhaseFactor = "1",
            HasChirality = false,
        },
        ConjugationConvention = new ConjugationConventionSpec
        {
            ConventionId = "hermitian-v1",
            ConjugationType = "hermitian",
            HasChargeConjugation = true,
        },
        SpinorComponents = 4,
        ChiralitySplit = 0,
        Provenance = TestProvenance(),
    };

    [Fact]
    public void BuildStandardLayout_Dim4_HasCorrectBlocks()
    {
        var layout = FermionFieldLayoutFactory.BuildStandardLayout(
            "layout-dim4-su2",
            Dim4Spec(),
            gaugeDimension: 3, // su(2) dim
            TestProvenance());

        Assert.Equal("layout-dim4-su2", layout.LayoutId);
        Assert.Equal(2, layout.SpinorBlocks.Count);
        Assert.Contains(layout.SpinorBlocks, b => b.Role == "primal" && b.BlockId == "psi");
        Assert.Contains(layout.SpinorBlocks, b => b.Role == "dual" && b.BlockId == "psi-bar");
    }

    [Fact]
    public void BuildStandardLayout_Dim4_PrimalDofsPerCell_IsSpinorTimesGauge()
    {
        int gaugeDim = 3;
        var layout = FermionFieldLayoutFactory.BuildStandardLayout(
            "layout-dim4-su2",
            Dim4Spec(),
            gaugeDimension: gaugeDim,
            TestProvenance());

        // spinorComponents=4, gaugeDim=3 => 12 primal DOFs per cell
        Assert.Equal(4 * gaugeDim, layout.PrimalDofsPerCell);
    }

    [Fact]
    public void BuildStandardLayout_Dim4_HasConjugationRule()
    {
        var layout = FermionFieldLayoutFactory.BuildStandardLayout(
            "layout-dim4-su2", Dim4Spec(), 3, TestProvenance());

        Assert.Single(layout.ConjugationRules);
        var rule = layout.ConjugationRules[0];
        Assert.Equal("psi", rule.SourceBlockId);
        Assert.Equal("psi-bar", rule.DualBlockId);
        Assert.Equal("hermitian", rule.Convention);
    }

    [Fact]
    public void BuildStandardLayout_Dim4_HasBilinears()
    {
        var layout = FermionFieldLayoutFactory.BuildStandardLayout(
            "layout-dim4-su2", Dim4Spec(), 3, TestProvenance());

        Assert.True(layout.AllowedBilinears.Count >= 4,
            $"Expected >=4 bilinears for even dim, got {layout.AllowedBilinears.Count}");
        Assert.Contains(layout.AllowedBilinears, b => b.BilinearType == "scalar");
        Assert.Contains(layout.AllowedBilinears, b => b.BilinearType == "vector");
        Assert.Contains(layout.AllowedBilinears, b => b.BilinearType == "pseudoscalar");
        Assert.Contains(layout.AllowedBilinears, b => b.BilinearType == "axial-vector");
    }

    [Fact]
    public void BuildStandardLayout_Dim5_NoPseudoscalar()
    {
        // Odd dimension: no chirality, no pseudoscalar or axial-vector bilinears
        var layout = FermionFieldLayoutFactory.BuildStandardLayout(
            "layout-dim5", Dim5Spec(), 1, TestProvenance());

        Assert.DoesNotContain(layout.AllowedBilinears, b => b.BilinearType == "pseudoscalar");
        Assert.DoesNotContain(layout.AllowedBilinears, b => b.BilinearType == "axial-vector");
    }

    [Fact]
    public void BuildChiralSplitLayout_Dim4_HasFourBlocks()
    {
        var layout = FermionFieldLayoutFactory.BuildChiralSplitLayout(
            "layout-dim4-chiral", Dim4Spec(), 3, TestProvenance());

        Assert.Equal(4, layout.SpinorBlocks.Count);
        Assert.Contains(layout.SpinorBlocks, b => b.Role == "chiral-left" && b.BlockId == "psi-L");
        Assert.Contains(layout.SpinorBlocks, b => b.Role == "chiral-right" && b.BlockId == "psi-R");
    }

    [Fact]
    public void BuildChiralSplitLayout_Dim5_ThrowsInvalidOperation()
    {
        Assert.Throws<InvalidOperationException>(() =>
            FermionFieldLayoutFactory.BuildChiralSplitLayout(
                "layout-dim5-chiral", Dim5Spec(), 1, TestProvenance()));
    }

    [Fact]
    public void TotalPrimalDofs_IsCorrect()
    {
        var layout = FermionFieldLayoutFactory.BuildStandardLayout(
            "layout-dim4-su2", Dim4Spec(), 3, TestProvenance());

        int cellCount = 10;
        Assert.Equal(layout.PrimalDofsPerCell * cellCount, layout.TotalPrimalDofs(cellCount));
    }

    [Fact]
    public void Layout_SerializesAndRoundTrips()
    {
        var layout = FermionFieldLayoutFactory.BuildStandardLayout(
            "layout-dim4-su2", Dim4Spec(), 3, TestProvenance());

        var json = FermionStateSerializer.ToJson(layout);
        Assert.Contains("layout-dim4-su2", json);
        Assert.Contains("psi", json);

        var loaded = FermionStateSerializer.LayoutFromJson(json);
        Assert.Equal(layout.LayoutId, loaded.LayoutId);
        Assert.Equal(layout.SpinorBlocks.Count, loaded.SpinorBlocks.Count);
        Assert.Equal(layout.ConjugationRules.Count, loaded.ConjugationRules.Count);
        Assert.Equal(layout.AllowedBilinears.Count, loaded.AllowedBilinears.Count);
    }

    [Fact]
    public void ObservationEligibility_OnlyPrimalBlock()
    {
        var layout = FermionFieldLayoutFactory.BuildStandardLayout(
            "layout-dim4-su2", Dim4Spec(), 3, TestProvenance());

        Assert.Contains("psi", layout.ObservationEligibility.EligibleBlockIds);
        Assert.DoesNotContain("psi-bar", layout.ObservationEligibility.EligibleBlockIds);
    }
}
