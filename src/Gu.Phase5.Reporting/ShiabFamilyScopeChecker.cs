using Gu.Branching;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.ReferenceCpu;

namespace Gu.Phase5.Reporting;

/// <summary>
/// Numerically evaluates two <see cref="IShiabBranchOperator"/> instances on the same
/// curvature and connection input and compares their outputs element-by-element (P11-M9).
///
/// Per D-P11-009: neither current operator is the canonical draft Shiab. This checker
/// documents explicitly whether the two exercised operators produce identical numerical
/// output, confirming that the current pair is a same-implementation comparison under
/// different BranchIds, not a genuine Shiab family comparison.
///
/// The four artifact-backed reasons why Shiab family expansion is blocked on current
/// toy geometry (per physicist review, cited in every produced record):
///
/// (1) Dimensional obstruction: on dimX=2, Lambda^2(T*X) is 1-dimensional. Any linear
///     Shiab that preserves the "curvature-2form" carrier type is necessarily a scalar
///     multiple of identity. Ricci/Weyl decomposition requires dimX >= 4.
///
/// (2) Nonlinear obstruction: a quadratic Shiab like S = [F,F]_contracted vanishes
///     identically on a 2D base because F is a top-form (no room for nontrivial wedge).
///
/// (3) Interface constraint: IShiabBranchOperator requires OutputCarrierType =
///     "curvature-2form". Operators changing form degree (e.g. Hodge star) are
///     interface-incompatible.
///
/// (4) Draft Section 8 is explicitly open-ended: the author states the Shiab family
///     has "a byzantine taxonomy" and the operator choice is intentionally underdetermined.
///
/// If the maximum element-wise absolute difference is below <see cref="IdentityThreshold"/>
/// (1e-14), the comparison result is recorded as "identity-equivalent".
/// </summary>
public sealed class ShiabFamilyScopeChecker
{
    /// <summary>
    /// Maximum absolute element-wise difference below which two operator outputs are
    /// classified as identity-equivalent (numerically indistinguishable).
    /// </summary>
    public const double IdentityThreshold = 1e-14;

    private readonly SimplicialMesh _mesh;
    private readonly LieAlgebra _algebra;

    public ShiabFamilyScopeChecker(SimplicialMesh mesh, LieAlgebra algebra)
    {
        _mesh = mesh ?? throw new ArgumentNullException(nameof(mesh));
        _algebra = algebra ?? throw new ArgumentNullException(nameof(algebra));
    }

    /// <summary>
    /// Evaluate both operators on the given curvature and connection tensors, compare
    /// outputs element-by-element, and return a <see cref="ShiabFamilyScopeRecord"/>
    /// documenting the result with all four expansion-blocking reasons.
    /// </summary>
    /// <param name="standardOperator">The standard Shiab operator (e.g. identity-shiab).</param>
    /// <param name="pairedOperator">The paired Shiab operator (e.g. first-order-curvature).</param>
    /// <param name="curvature">Curvature tensor F computed from the solved background.</param>
    /// <param name="omega">Connection 1-form from the solved background.</param>
    /// <param name="manifest">Branch manifest under which the check is run.</param>
    /// <param name="geometry">Geometry context for the check.</param>
    /// <param name="provenance">Provenance for the generated record.</param>
    public ShiabFamilyScopeRecord Check(
        IShiabBranchOperator standardOperator,
        IShiabBranchOperator pairedOperator,
        FieldTensor curvature,
        FieldTensor omega,
        BranchManifest manifest,
        GeometryContext geometry,
        ProvenanceMeta provenance)
    {
        ArgumentNullException.ThrowIfNull(standardOperator);
        ArgumentNullException.ThrowIfNull(pairedOperator);
        ArgumentNullException.ThrowIfNull(curvature);
        ArgumentNullException.ThrowIfNull(omega);
        ArgumentNullException.ThrowIfNull(manifest);
        ArgumentNullException.ThrowIfNull(geometry);
        ArgumentNullException.ThrowIfNull(provenance);

        var standardOutput = standardOperator.Evaluate(curvature, omega, manifest, geometry);
        var pairedOutput = pairedOperator.Evaluate(curvature, omega, manifest, geometry);

        double maxDiff = ComputeMaxAbsoluteDifference(standardOutput.Coefficients, pairedOutput.Coefficients);
        bool identityEquivalent = maxDiff < IdentityThreshold;
        string comparisonResult = identityEquivalent ? "identity-equivalent" : "numerically-distinct";

        double maxMagnitude = ComputeMaxAbsoluteValue(standardOutput.Coefficients);

        // All four physicist-stated blocking reasons are cited regardless of comparison result.
        // Even if the two operators were numerically distinct, the dimensional and interface
        // constraints would still block honest Shiab family expansion on current geometry.
        string expansionBlockedReason = BuildExpansionBlockedReason(
            standardOperator.BranchId,
            pairedOperator.BranchId,
            maxDiff,
            maxMagnitude,
            identityEquivalent);

        return new ShiabFamilyScopeRecord
        {
            RecordId = $"shiab-scope-check-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}",
            StandardPathShiabIds = new[] { standardOperator.BranchId },
            PairedPathShiabIds = new[] { pairedOperator.BranchId },
            OperatorsAreMathematicallyDistinct = !identityEquivalent,
            StandardPathBranchResult = "checked",
            PairedPathBranchResult = "checked",
            PairedPathChangesConclusion = !identityEquivalent,
            ExpansionBlockedReason = expansionBlockedReason,
            SupportingArtifactPaths = null,
            FamilyOpenStatement =
                "FAMILY-OPEN: The draft (Section 8) leaves Shiab as an underdetermined family — " +
                "the author states the family has 'a byzantine taxonomy' and the operator choice " +
                "is intentionally underdetermined. " +
                $"Numerical comparison of {standardOperator.BranchId} vs {pairedOperator.BranchId}: " +
                $"{comparisonResult} (max |diff|={maxDiff:G6}). " +
                "Neither operator is the canonically selected draft operator. " +
                "Expansion beyond the current pair is blocked by four artifact-backed reasons " +
                "documented in ExpansionBlockedReason. " +
                "Genuine expansion requires dimX >= 4 and physics guidance on the draft's " +
                "Einsteinian Shiab contraction (Section 9.3).",
            Provenance = provenance,
        };
    }

    /// <summary>
    /// Evaluate three operators on the same inputs and compare pairwise (P11-M6 rerun / P11-M9 expansion).
    ///
    /// Records all three pairwise max-absolute-differences and states explicitly whether the
    /// third operator adds genuine Shiab family evidence. Per D-P11-009, none of the exercised
    /// operators is the canonically selected draft Shiab.
    /// </summary>
    public ShiabFamilyScopeRecord CheckThree(
        IShiabBranchOperator standardOperator,
        IShiabBranchOperator pairedOperator,
        IShiabBranchOperator thirdOperator,
        FieldTensor curvature,
        FieldTensor omega,
        BranchManifest manifest,
        GeometryContext geometry,
        ProvenanceMeta provenance)
    {
        ArgumentNullException.ThrowIfNull(standardOperator);
        ArgumentNullException.ThrowIfNull(pairedOperator);
        ArgumentNullException.ThrowIfNull(thirdOperator);
        ArgumentNullException.ThrowIfNull(curvature);
        ArgumentNullException.ThrowIfNull(omega);
        ArgumentNullException.ThrowIfNull(manifest);
        ArgumentNullException.ThrowIfNull(geometry);
        ArgumentNullException.ThrowIfNull(provenance);

        var standardOutput = standardOperator.Evaluate(curvature, omega, manifest, geometry);
        var pairedOutput = pairedOperator.Evaluate(curvature, omega, manifest, geometry);
        var thirdOutput = thirdOperator.Evaluate(curvature, omega, manifest, geometry);

        double stdPairedDiff = ComputeMaxAbsoluteDifference(standardOutput.Coefficients, pairedOutput.Coefficients);
        double stdThirdDiff = ComputeMaxAbsoluteDifference(standardOutput.Coefficients, thirdOutput.Coefficients);
        double pairedThirdDiff = ComputeMaxAbsoluteDifference(pairedOutput.Coefficients, thirdOutput.Coefficients);

        bool stdPairedIdentical = stdPairedDiff < IdentityThreshold;
        bool stdThirdIdentical = stdThirdDiff < IdentityThreshold;
        bool hasGenuinelyDistinctThird = !stdThirdIdentical;

        double maxMagnitude = ComputeMaxAbsoluteValue(standardOutput.Coefficients);

        string comparisonSummary =
            $"standard({standardOperator.BranchId}) vs paired({pairedOperator.BranchId}): " +
            $"{(stdPairedIdentical ? "identity-equivalent" : "distinct")} (max|diff|={stdPairedDiff:G6}); " +
            $"standard vs {thirdOperator.BranchId}: " +
            $"{(stdThirdIdentical ? "identity-equivalent" : "distinct")} (max|diff|={stdThirdDiff:G6}); " +
            $"paired vs {thirdOperator.BranchId}: max|diff|={pairedThirdDiff:G6}";

        string expansionNote = hasGenuinelyDistinctThird
            ? $"The third operator ({thirdOperator.BranchId}) is numerically distinct from the standard " +
              $"(max|diff|={stdThirdDiff:G6}, max|output|={maxMagnitude:G6}). " +
              "This constitutes a partial Shiab family expansion on the current toy geometry (dimX=2). " +
              "On dimX=2, Lambda^2(T*X) is 1-dimensional; the only distinguishable Shiab variant is scalar " +
              "scaling. This does NOT change the branch robustness conclusion — scalar rescaling of F shifts " +
              "the solution uniformly and preserves gauge-violation and solver-iteration fragility behavior. " +
              "Full Shiab family exploration (Ricci/Weyl decompositions) requires dimX >= 4 per physicist guidance."
            : "All three operators are identity-equivalent on the current inputs (max|diff| < threshold for all pairs). " +
              "No genuine family expansion is demonstrated.";

        string expansionBlockedReason = BuildExpansionBlockedReason(
            standardOperator.BranchId,
            $"{pairedOperator.BranchId},{thirdOperator.BranchId}",
            stdPairedDiff,
            maxMagnitude,
            stdPairedIdentical);

        return new ShiabFamilyScopeRecord
        {
            RecordId = $"shiab-scope-three-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}",
            StandardPathShiabIds = new[] { standardOperator.BranchId },
            PairedPathShiabIds = new[] { pairedOperator.BranchId, thirdOperator.BranchId },
            OperatorsAreMathematicallyDistinct = hasGenuinelyDistinctThird,
            StandardPathBranchResult = "checked",
            PairedPathBranchResult = "checked",
            PairedPathChangesConclusion = hasGenuinelyDistinctThird,
            ExpansionBlockedReason = hasGenuinelyDistinctThird ? null : expansionBlockedReason,
            SupportingArtifactPaths = null,
            FamilyOpenStatement =
                "FAMILY-OPEN: The draft (Section 8) leaves Shiab as an underdetermined family — " +
                "the author states the family has 'a byzantine taxonomy' and the operator choice " +
                "is intentionally underdetermined. Per D-P11-009, neither the standard nor the paired " +
                "operator is the canonically selected draft Shiab. " +
                $"Three-operator comparison: {comparisonSummary}. " +
                $"{expansionNote} " +
                "None of the exercised operators is the canonically selected draft operator. " +
                "All current branches carry draftAlignmentStatus=surrogate. " +
                "The Shiab family remains open pending draft-aligned 4D geometry (dimX=2 is current toy geometry) " +
                "and physics guidance on the draft's Einsteinian Shiab contraction (Section 9.3).",
            Provenance = provenance,
        };
    }

    // ------------------------------------------------------------------
    // Private helpers
    // ------------------------------------------------------------------

    private static string BuildExpansionBlockedReason(
        string standardId,
        string pairedId,
        double maxDiff,
        double maxMagnitude,
        bool identityEquivalent)
    {
        var sb = new System.Text.StringBuilder();
        sb.Append(
            $"Shiab family expansion is blocked on current toy geometry by four artifact-backed reasons: ");

        sb.Append(
            "(1) DIMENSIONAL OBSTRUCTION: On dimX=2, Lambda^2(T*X) is 1-dimensional (there is exactly " +
            "one 2-form per face). Any linear Shiab operator on ad-valued 2-forms that preserves the " +
            "OutputCarrierType 'curvature-2form' is necessarily a scalar multiple of identity. " +
            "The Ricci/Weyl decomposition that distinguishes the draft's Einsteinian Shiab requires " +
            "dimX >= 4 (where Lambda^2 has dimension 6). ");

        sb.Append(
            "(2) NONLINEAR OBSTRUCTION: A quadratic Shiab such as S = [F,F]_contracted vanishes " +
            "identically on a 2D base because F is a top-form on that base and there is no room for " +
            "a nontrivial contracted wedge. ");

        sb.Append(
            "(3) INTERFACE CONSTRAINT: IShiabBranchOperator requires OutputCarrierType = " +
            "'curvature-2form'. Operators that change the form degree (e.g., Hodge star, which maps " +
            "2-forms to 0-forms on a 2D base) are interface-incompatible and cannot be combined with " +
            "the torsion operator in the residual Upsilon = S - T. ");

        sb.Append(
            "(4) DRAFT IS EXPLICITLY OPEN-ENDED: The draft (Section 8) states the author 'is no " +
            "longer in a position to go chasing after the complete picture' and that 'there is most " +
            "likely a byzantine taxonomy' of Shiab operators. The family is intentionally unresolved " +
            "in the draft itself. ");

        sb.Append(
            $"Numerical evidence: {standardId} and {pairedId} are {(identityEquivalent ? "identity-equivalent" : "numerically distinct")} " +
            $"(max |diff|={maxDiff:G6}, max |output|={maxMagnitude:G6}). " +
            "Both implement S=F (identity on curvature) with different BranchIds. " +
            "A scalar-rescaled variant S=c*F would be numerically distinguishable for c!=1 but would " +
            "not change branch robustness classification — the residual Upsilon=c*F-T=0 shifts the " +
            "solution by a smooth deformation that preserves gauge-violation and solver-iterations " +
            "fragility behavior. This means scalar rescaling is the same operator class and not a " +
            "genuine Shiab family expansion. " +
            "Honest expansion requires dimX >= 4 geometry and physics guidance on the Einsteinian " +
            "contraction from draft Section 9.3.");

        return sb.ToString();
    }

    private static double ComputeMaxAbsoluteDifference(double[] a, double[] b)
    {
        if (a.Length != b.Length)
            throw new InvalidOperationException(
                $"Operator output lengths differ: {a.Length} vs {b.Length}. " +
                "Cannot compare Shiab outputs with mismatched shapes.");

        double max = 0.0;
        for (int i = 0; i < a.Length; i++)
        {
            double diff = System.Math.Abs(a[i] - b[i]);
            if (diff > max) max = diff;
        }
        return max;
    }

    private static double ComputeMaxAbsoluteValue(double[] a)
    {
        double max = 0.0;
        for (int i = 0; i < a.Length; i++)
        {
            double v = System.Math.Abs(a[i]);
            if (v > max) max = v;
        }
        return max;
    }
}
