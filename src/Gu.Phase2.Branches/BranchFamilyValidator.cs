using Gu.Phase2.Semantics;

namespace Gu.Phase2.Branches;

/// <summary>
/// Validates BranchFamilyManifest and BranchVariantManifest for completeness.
/// Ensures no branch settings remain implicit.
/// </summary>
public static class BranchFamilyValidator
{
    /// <summary>
    /// Validate a branch variant manifest. Returns a list of error messages (empty if valid).
    /// </summary>
    public static IReadOnlyList<string> ValidateVariant(BranchVariantManifest variant)
    {
        ArgumentNullException.ThrowIfNull(variant);
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(variant.Id))
            errors.Add("BranchVariantManifest.Id must not be empty.");
        if (string.IsNullOrWhiteSpace(variant.ParentFamilyId))
            errors.Add("BranchVariantManifest.ParentFamilyId must not be empty.");
        if (string.IsNullOrWhiteSpace(variant.A0Variant))
            errors.Add("BranchVariantManifest.A0Variant must not be empty.");
        if (string.IsNullOrWhiteSpace(variant.BiConnectionVariant))
            errors.Add("BranchVariantManifest.BiConnectionVariant must not be empty.");
        if (string.IsNullOrWhiteSpace(variant.TorsionVariant))
            errors.Add("BranchVariantManifest.TorsionVariant must not be empty.");
        if (string.IsNullOrWhiteSpace(variant.ShiabVariant))
            errors.Add("BranchVariantManifest.ShiabVariant must not be empty.");
        if (string.IsNullOrWhiteSpace(variant.ObservationVariant))
            errors.Add("BranchVariantManifest.ObservationVariant must not be empty.");
        if (string.IsNullOrWhiteSpace(variant.ExtractionVariant))
            errors.Add("BranchVariantManifest.ExtractionVariant must not be empty.");
        if (string.IsNullOrWhiteSpace(variant.GaugeVariant))
            errors.Add("BranchVariantManifest.GaugeVariant must not be empty.");
        if (string.IsNullOrWhiteSpace(variant.RegularityVariant))
            errors.Add("BranchVariantManifest.RegularityVariant must not be empty.");
        if (string.IsNullOrWhiteSpace(variant.PairingVariant))
            errors.Add("BranchVariantManifest.PairingVariant must not be empty.");
        if (string.IsNullOrWhiteSpace(variant.ExpectedClaimCeiling))
            errors.Add("BranchVariantManifest.ExpectedClaimCeiling must not be empty.");

        return errors;
    }

    /// <summary>
    /// Validate a branch family manifest. Returns a list of error messages (empty if valid).
    /// </summary>
    public static IReadOnlyList<string> ValidateFamily(BranchFamilyManifest family)
    {
        ArgumentNullException.ThrowIfNull(family);
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(family.FamilyId))
            errors.Add("BranchFamilyManifest.FamilyId must not be empty.");
        if (string.IsNullOrWhiteSpace(family.Description))
            errors.Add("BranchFamilyManifest.Description must not be empty.");
        if (family.Variants == null || family.Variants.Count == 0)
            errors.Add("BranchFamilyManifest.Variants must contain at least one variant.");
        if (family.DefaultEquivalence == null)
        {
            errors.Add("BranchFamilyManifest.DefaultEquivalence must not be null.");
        }
        else
        {
            var eqErrors = ValidateEquivalenceSpec(family.DefaultEquivalence);
            foreach (var e in eqErrors)
                errors.Add($"DefaultEquivalence: {e}");
        }

        if (family.Variants != null)
        {
            var ids = new HashSet<string>();
            foreach (var variant in family.Variants)
            {
                if (!ids.Add(variant.Id))
                    errors.Add($"Duplicate variant Id '{variant.Id}' in family '{family.FamilyId}'.");

                if (variant.ParentFamilyId != family.FamilyId)
                {
                    errors.Add(
                        $"Variant '{variant.Id}' declares parent '{variant.ParentFamilyId}' " +
                        $"but belongs to family '{family.FamilyId}'.");
                }

                var variantErrors = ValidateVariant(variant);
                foreach (var e in variantErrors)
                    errors.Add($"Variant '{variant.Id}': {e}");
            }
        }

        return errors;
    }

    /// <summary>
    /// Validate and throw if invalid.
    /// </summary>
    public static void ValidateFamilyOrThrow(BranchFamilyManifest family)
    {
        var errors = ValidateFamily(family);
        if (errors.Count > 0)
        {
            throw new InvalidOperationException(
                $"BranchFamilyManifest validation failed with {errors.Count} error(s):\n" +
                string.Join("\n", errors));
        }
    }

    /// <summary>
    /// Validate variant and throw if invalid.
    /// </summary>
    public static void ValidateVariantOrThrow(BranchVariantManifest variant)
    {
        var errors = ValidateVariant(variant);
        if (errors.Count > 0)
        {
            throw new InvalidOperationException(
                $"BranchVariantManifest validation failed with {errors.Count} error(s):\n" +
                string.Join("\n", errors));
        }
    }

    /// <summary>
    /// Validate an equivalence specification. Returns a list of error messages (empty if valid).
    /// </summary>
    public static IReadOnlyList<string> ValidateEquivalenceSpec(EquivalenceSpec spec)
    {
        ArgumentNullException.ThrowIfNull(spec);
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(spec.Id))
            errors.Add("EquivalenceSpec.Id must not be empty.");
        if (string.IsNullOrWhiteSpace(spec.Name))
            errors.Add("EquivalenceSpec.Name must not be empty.");
        if (spec.ComparedObjectClasses == null || spec.ComparedObjectClasses.Count == 0)
            errors.Add("EquivalenceSpec.ComparedObjectClasses must contain at least one entry.");
        if (string.IsNullOrWhiteSpace(spec.NormalizationProcedure))
            errors.Add("EquivalenceSpec.NormalizationProcedure must not be empty.");
        if (spec.Metrics == null || spec.Metrics.Count == 0)
            errors.Add("EquivalenceSpec.Metrics must contain at least one entry.");
        if (spec.Tolerances == null || spec.Tolerances.Count == 0)
            errors.Add("EquivalenceSpec.Tolerances must contain at least one entry.");
        if (string.IsNullOrWhiteSpace(spec.InterpretationRule))
            errors.Add("EquivalenceSpec.InterpretationRule must not be empty.");

        return errors;
    }
}
