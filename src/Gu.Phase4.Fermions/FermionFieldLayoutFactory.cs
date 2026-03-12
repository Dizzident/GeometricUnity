using Gu.Core;
using Gu.Phase4.Spin;

namespace Gu.Phase4.Fermions;

/// <summary>
/// Factory for constructing standard FermionFieldLayout objects.
///
/// All layouts use conservative defaults:
/// - one primal block ("psi") and one dual block ("psi-bar"),
/// - gauge dimension set by the caller,
/// - Hermitian conjugation for Riemannian signature.
/// </summary>
public static class FermionFieldLayoutFactory
{
    /// <summary>
    /// Build a minimal standard layout for a given SpinorRepresentationSpec.
    ///
    /// Creates:
    /// - one primal block with role "primal"
    /// - one dual block with role "dual"
    /// - one Hermitian conjugation rule
    /// - one scalar and one vector bilinear
    /// - observation eligibility for the primal block
    /// </summary>
    public static FermionFieldLayout BuildStandardLayout(
        string layoutId,
        SpinorRepresentationSpec spec,
        int gaugeDimension,
        ProvenanceMeta provenance,
        IReadOnlyList<string>? insertedAssumptionIds = null)
    {
        ArgumentNullException.ThrowIfNull(spec);
        ArgumentNullException.ThrowIfNull(provenance);

        if (gaugeDimension < 1)
            throw new ArgumentOutOfRangeException(nameof(gaugeDimension), "Gauge dimension must be >= 1.");

        var primalBlock = new SpinorBlockSpec
        {
            BlockId = "psi",
            Role = "primal",
            SpinorDimension = spec.SpinorComponents,
            GaugeDimension = gaugeDimension,
            Description = "Primary Dirac spinor field on Y_h",
        };

        var dualBlock = new SpinorBlockSpec
        {
            BlockId = "psi-bar",
            Role = "dual",
            SpinorDimension = spec.SpinorComponents,
            GaugeDimension = gaugeDimension,
            Description = "Dual spinor (psi-bar) for bilinears",
        };

        var conjugationRule = new ConjugationRule
        {
            SourceBlockId = "psi",
            DualBlockId = "psi-bar",
            Convention = spec.ConjugationConvention.ConjugationType,
        };

        var bilinears = new List<AllowedBilinear>
        {
            new() { LeftBlockId = "psi-bar", RightBlockId = "psi",
                    BilinearType = "scalar", ObservationEligible = true },
            new() { LeftBlockId = "psi-bar", RightBlockId = "psi",
                    BilinearType = "vector", ObservationEligible = true },
        };

        if (spec.ChiralityConvention.HasChirality)
        {
            bilinears.Add(new AllowedBilinear
            {
                LeftBlockId = "psi-bar",
                RightBlockId = "psi",
                BilinearType = "pseudoscalar",
                ObservationEligible = true,
            });
            bilinears.Add(new AllowedBilinear
            {
                LeftBlockId = "psi-bar",
                RightBlockId = "psi",
                BilinearType = "axial-vector",
                ObservationEligible = true,
            });
        }

        var observations = new ObservationEligibilitySpec
        {
            EligibleBlockIds = new List<string> { "psi" },
            Notes = "Only primal block eligible for sigma_h pullback to X_h.",
        };

        return new FermionFieldLayout
        {
            LayoutId = layoutId,
            SpinorSpecId = spec.SpinorSpecId,
            SpinorBlocks = new List<SpinorBlockSpec> { primalBlock, dualBlock },
            ConjugationRules = new List<ConjugationRule> { conjugationRule },
            AllowedBilinears = bilinears,
            ObservationEligibility = observations,
            InsertedAssumptionIds = insertedAssumptionIds?.ToList() ?? new(),
            Provenance = provenance,
        };
    }

    /// <summary>
    /// Build a chirally-split layout with left and right Weyl sub-blocks,
    /// in addition to the standard primal/dual blocks.
    /// Only valid for even spacetime dimension (spec.ChiralitySplit > 0).
    /// </summary>
    public static FermionFieldLayout BuildChiralSplitLayout(
        string layoutId,
        SpinorRepresentationSpec spec,
        int gaugeDimension,
        ProvenanceMeta provenance,
        IReadOnlyList<string>? insertedAssumptionIds = null)
    {
        ArgumentNullException.ThrowIfNull(spec);
        ArgumentNullException.ThrowIfNull(provenance);

        if (spec.ChiralitySplit <= 0)
            throw new InvalidOperationException(
                $"Chiral-split layout requires even spacetime dimension (dimY={spec.SpacetimeDimension} has no chirality).");

        if (gaugeDimension < 1)
            throw new ArgumentOutOfRangeException(nameof(gaugeDimension));

        var primalBlock = new SpinorBlockSpec
        {
            BlockId = "psi",
            Role = "primal",
            SpinorDimension = spec.SpinorComponents,
            GaugeDimension = gaugeDimension,
            Description = "Full Dirac spinor psi on Y_h",
        };

        var dualBlock = new SpinorBlockSpec
        {
            BlockId = "psi-bar",
            Role = "dual",
            SpinorDimension = spec.SpinorComponents,
            GaugeDimension = gaugeDimension,
            Description = "Dual spinor psi-bar",
        };

        var leftBlock = new SpinorBlockSpec
        {
            BlockId = "psi-L",
            Role = "chiral-left",
            SpinorDimension = spec.ChiralitySplit,
            GaugeDimension = gaugeDimension,
            Description = "Left-chiral Weyl component P_L psi",
        };

        var rightBlock = new SpinorBlockSpec
        {
            BlockId = "psi-R",
            Role = "chiral-right",
            SpinorDimension = spec.ChiralitySplit,
            GaugeDimension = gaugeDimension,
            Description = "Right-chiral Weyl component P_R psi",
        };

        var conjugationRules = new List<ConjugationRule>
        {
            new() { SourceBlockId = "psi",   DualBlockId = "psi-bar",
                    Convention = spec.ConjugationConvention.ConjugationType },
            new() { SourceBlockId = "psi-L", DualBlockId = "psi-bar",
                    Convention = spec.ConjugationConvention.ConjugationType },
            new() { SourceBlockId = "psi-R", DualBlockId = "psi-bar",
                    Convention = spec.ConjugationConvention.ConjugationType },
        };

        var bilinears = new List<AllowedBilinear>
        {
            new() { LeftBlockId = "psi-bar", RightBlockId = "psi",
                    BilinearType = "scalar", ObservationEligible = true },
            new() { LeftBlockId = "psi-bar", RightBlockId = "psi",
                    BilinearType = "vector", ObservationEligible = true },
            new() { LeftBlockId = "psi-bar", RightBlockId = "psi",
                    BilinearType = "pseudoscalar", ObservationEligible = true },
            new() { LeftBlockId = "psi-bar", RightBlockId = "psi",
                    BilinearType = "axial-vector", ObservationEligible = true },
            new() { LeftBlockId = "psi-bar", RightBlockId = "psi-L",
                    BilinearType = "vector", ObservationEligible = true },
            new() { LeftBlockId = "psi-bar", RightBlockId = "psi-R",
                    BilinearType = "vector", ObservationEligible = true },
        };

        var observations = new ObservationEligibilitySpec
        {
            EligibleBlockIds = new List<string> { "psi", "psi-L", "psi-R" },
            Notes = "Primal and chiral sub-blocks eligible for sigma_h pullback to X_h.",
        };

        return new FermionFieldLayout
        {
            LayoutId = layoutId,
            SpinorSpecId = spec.SpinorSpecId,
            SpinorBlocks = new List<SpinorBlockSpec> { primalBlock, dualBlock, leftBlock, rightBlock },
            ConjugationRules = conjugationRules,
            AllowedBilinears = bilinears,
            ObservationEligibility = observations,
            InsertedAssumptionIds = insertedAssumptionIds?.ToList() ?? new(),
            Provenance = provenance,
        };
    }

    /// <summary>
    /// Create a zero DiscreteFermionState compatible with the given layout and cell count.
    /// </summary>
    public static DiscreteFermionState CreateZeroState(
        string stateId,
        FermionFieldLayout layout,
        int cellCount,
        ProvenanceMeta provenance)
    {
        ArgumentNullException.ThrowIfNull(layout);
        ArgumentNullException.ThrowIfNull(provenance);

        int dofsPerCell = layout.PrimalDofsPerCell;
        return new DiscreteFermionState
        {
            StateId = stateId,
            LayoutId = layout.LayoutId,
            CellCount = cellCount,
            DofsPerCell = dofsPerCell,
            Coefficients = new double[2 * cellCount * dofsPerCell],
            Provenance = provenance,
        };
    }

    /// <summary>
    /// Create the Hermitian dual of a DiscreteFermionState (psi_bar = psi^dagger).
    /// Hermitian conjugation: swap sign of imaginary components.
    /// </summary>
    public static DiscreteDualFermionState CreateHermitianDual(
        DiscreteFermionState primal,
        ProvenanceMeta provenance)
    {
        ArgumentNullException.ThrowIfNull(primal);
        ArgumentNullException.ThrowIfNull(provenance);

        // For Hermitian dual: (Re + i*Im)^† = (Re - i*Im)
        var dualCoeffs = new double[primal.Coefficients.Length];
        for (int i = 0; i < primal.Coefficients.Length; i += 2)
        {
            dualCoeffs[i]     =  primal.Coefficients[i];      // Re unchanged
            dualCoeffs[i + 1] = -primal.Coefficients[i + 1];  // Im negated
        }

        return new DiscreteDualFermionState
        {
            StateId = primal.StateId + "-bar",
            PrimalStateId = primal.StateId,
            LayoutId = primal.LayoutId,
            CellCount = primal.CellCount,
            DofsPerCell = primal.DofsPerCell,
            Coefficients = dualCoeffs,
            ConjugationType = "hermitian",
            Provenance = provenance,
        };
    }
}
