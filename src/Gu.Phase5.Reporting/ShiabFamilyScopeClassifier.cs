using Gu.Core;
using Gu.Phase5.BranchIndependence;

namespace Gu.Phase5.Reporting;

/// <summary>
/// Builds a <see cref="ShiabFamilyScopeRecord"/> from executed branch robustness records
/// for the standard and paired Shiab paths.
///
/// Per D-P11-009: neither current Shiab branch is the canonical draft operator. This
/// classifier documents the scope boundary explicitly in the executed artifacts.
///
/// The two current implementations (identity-shiab and first-order-curvature) both
/// compute S=F (identity on curvature). They are mathematically identical despite having
/// different BranchIds. Expansion to a genuinely distinct operator requires physics
/// guidance on what form the richer Shiab should take, which is not yet available in
/// repository context.
/// </summary>
public static class ShiabFamilyScopeClassifier
{
    /// <summary>
    /// Build a ShiabFamilyScopeRecord from standard and paired branch robustness records.
    /// </summary>
    /// <param name="standardRecord">Branch robustness record for the standard (identity-shiab) path.</param>
    /// <param name="pairedRecord">Branch robustness record for the paired (first-order-curvature) path.</param>
    /// <param name="standardShiabIds">Shiab branch IDs used in the standard path.</param>
    /// <param name="pairedShiabIds">Shiab branch IDs used in the paired path.</param>
    /// <param name="supportingArtifactPaths">Paths to the artifact files that support the scope claim.</param>
    /// <param name="provenance">Provenance for the generated record.</param>
    public static ShiabFamilyScopeRecord Classify(
        BranchRobustnessRecord standardRecord,
        BranchRobustnessRecord pairedRecord,
        IReadOnlyList<string> standardShiabIds,
        IReadOnlyList<string> pairedShiabIds,
        IReadOnlyList<string>? supportingArtifactPaths,
        ProvenanceMeta provenance)
    {
        ArgumentNullException.ThrowIfNull(standardRecord);
        ArgumentNullException.ThrowIfNull(pairedRecord);
        ArgumentNullException.ThrowIfNull(standardShiabIds);
        ArgumentNullException.ThrowIfNull(pairedShiabIds);
        ArgumentNullException.ThrowIfNull(provenance);

        bool standardMixed = string.Equals(standardRecord.OverallSummary, "mixed", StringComparison.OrdinalIgnoreCase);
        bool pairedMixed = string.Equals(pairedRecord.OverallSummary, "mixed", StringComparison.OrdinalIgnoreCase);

        // The two current operators are mathematically identical (both S=F).
        // This is a known limitation documented in ASSUMPTIONS.md A-009.
        bool mathematicallyDistinct = false;

        // Neither path changes the project-level conclusion: both remain mixed.
        bool changesConclusion = !string.Equals(
            standardRecord.OverallSummary,
            pairedRecord.OverallSummary,
            StringComparison.OrdinalIgnoreCase);

        string expansionBlockedReason = BuildExpansionBlockedReason(
            standardRecord, pairedRecord, standardMixed, pairedMixed);

        return new ShiabFamilyScopeRecord
        {
            RecordId = $"shiab-scope-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}",
            StandardPathShiabIds = standardShiabIds,
            PairedPathShiabIds = pairedShiabIds,
            OperatorsAreMathematicallyDistinct = mathematicallyDistinct,
            StandardPathBranchResult = standardRecord.OverallSummary,
            PairedPathBranchResult = pairedRecord.OverallSummary,
            PairedPathChangesConclusion = changesConclusion,
            ExpansionBlockedReason = expansionBlockedReason,
            SupportingArtifactPaths = supportingArtifactPaths,
            FamilyOpenStatement =
                "FAMILY-OPEN: The draft (Section 8) leaves Shiab as an underdetermined family. " +
                "Neither identity-shiab nor first-order-curvature is the canonically selected " +
                "draft operator. Both operators implement S=F (identity on curvature) and are " +
                "mathematically identical. Expansion to a genuinely distinct Shiab branch " +
                "requires physics guidance beyond the current repository context.",
            Provenance = provenance,
        };
    }

    /// <summary>
    /// Build a scope record for the case where no paired record is available.
    /// Used when only the standard path has been exercised.
    /// </summary>
    public static ShiabFamilyScopeRecord ClassifyStandardOnly(
        BranchRobustnessRecord standardRecord,
        IReadOnlyList<string> standardShiabIds,
        IReadOnlyList<string>? supportingArtifactPaths,
        ProvenanceMeta provenance)
    {
        ArgumentNullException.ThrowIfNull(standardRecord);
        ArgumentNullException.ThrowIfNull(standardShiabIds);
        ArgumentNullException.ThrowIfNull(provenance);

        return new ShiabFamilyScopeRecord
        {
            RecordId = $"shiab-scope-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}",
            StandardPathShiabIds = standardShiabIds,
            PairedPathShiabIds = Array.Empty<string>(),
            OperatorsAreMathematicallyDistinct = false,
            StandardPathBranchResult = standardRecord.OverallSummary,
            PairedPathBranchResult = "not-evaluated",
            PairedPathChangesConclusion = false,
            ExpansionBlockedReason =
                "Paired path not evaluated in this run. Standard path alone cannot demonstrate " +
                "Shiab family breadth. Expansion beyond a single operator requires additional " +
                "executed comparison evidence.",
            SupportingArtifactPaths = supportingArtifactPaths,
            FamilyOpenStatement =
                "FAMILY-OPEN: The draft (Section 8) leaves Shiab as an underdetermined family. " +
                "The standard path exercises only identity-shiab (S=F). No broader family " +
                "comparison is available in this run.",
            Provenance = provenance,
        };
    }

    private static string BuildExpansionBlockedReason(
        BranchRobustnessRecord standardRecord,
        BranchRobustnessRecord pairedRecord,
        bool standardMixed,
        bool pairedMixed)
    {
        int standardFragile = standardRecord.FragilityRecords.Values
            .Count(f => string.Equals(f.Classification, "fragile", StringComparison.OrdinalIgnoreCase));
        int pairedFragile = pairedRecord.FragilityRecords.Values
            .Count(f => string.Equals(f.Classification, "fragile", StringComparison.OrdinalIgnoreCase));

        var parts = new List<string>
        {
            "Repository context blocks honest Shiab family expansion for the following reasons:",
            $"(1) identity-shiab and first-order-curvature are mathematically identical (both S=F); " +
                "adding a third branch under the same implementation pattern would not expand the family.",
        };

        if (standardMixed)
            parts.Add($"(2) Standard path (identity-shiab) branch result is '{standardRecord.OverallSummary}' " +
                $"with {standardFragile} fragile quantities; no stronger standard-path family exists in repository context.");

        if (pairedMixed)
            parts.Add($"(3) Paired path (first-order-curvature) branch result is '{pairedRecord.OverallSummary}' " +
                $"with {pairedFragile} fragile quantities; broader Shiab scope reproduces the same mixed boundary.");

        parts.Add("(4) Implementing a genuinely distinct Shiab operator (e.g., one involving Hodge star " +
            "contractions or non-identity projections) requires physics guidance on what form the richer " +
            "branch should take, which is not yet available in repository context.");

        return string.Join(" ", parts);
    }
}
