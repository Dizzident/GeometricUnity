using Gu.Core;
using Gu.Phase2.Semantics;

namespace Gu.Phase2.Branches;

/// <summary>
/// Converts a BranchVariantManifest + base BranchManifest into a concrete Phase I BranchManifest
/// by overriding variant-specific fields. This is THE critical bridge to Phase I.
///
/// Field provenance (no hidden defaults):
///   FROM VARIANT (branch-sensitive choices):
///     BranchId, ActiveTorsionBranch, ActiveShiabBranch, ActiveGaugeStrategy,
///     ActiveObservationBranch, PairingConventionId,
///     Parameters[biConnectionStrategy, a0Variant, regularityVariant, extractionVariant]
///   FROM BASE MANIFEST (environment/infrastructure constants shared across all variants):
///     SchemaVersion, SourceEquationRevision, CodeRevision, ActiveGeometryBranch,
///     BaseDimension, AmbientDimension, LieAlgebraId, BasisConventionId,
///     ComponentOrderId, AdjointConventionId, NormConventionId, DifferentialFormMetricId,
///     InsertedAssumptionIds, InsertedChoiceIds
/// </summary>
public static class BranchVariantResolver
{
    /// <summary>
    /// Keys in base manifest Parameters that are branch-sensitive and must NOT
    /// silently pass through. These are overridden by the variant.
    /// </summary>
    private static readonly HashSet<string> BranchSensitiveParameterKeys = new(StringComparer.Ordinal)
    {
        "biConnectionStrategy",
        "a0Variant",
        "regularityVariant",
        "extractionVariant",
    };

    /// <summary>
    /// Produce a Phase I BranchManifest by applying variant overrides to a base manifest.
    /// All branch-sensitive choices come from the variant; infrastructure constants come from the base.
    /// Throws if the base manifest contains branch-sensitive parameter keys that would be silently inherited.
    /// </summary>
    public static BranchManifest Resolve(
        BranchVariantManifest variant,
        BranchManifest baseManifest)
    {
        ArgumentNullException.ThrowIfNull(variant);
        ArgumentNullException.ThrowIfNull(baseManifest);

        ValidateBaseManifest(baseManifest);

        return new BranchManifest
        {
            BranchId = variant.Id,
            SchemaVersion = baseManifest.SchemaVersion,
            SourceEquationRevision = baseManifest.SourceEquationRevision,
            CodeRevision = baseManifest.CodeRevision,
            ActiveGeometryBranch = baseManifest.ActiveGeometryBranch,
            ActiveObservationBranch = variant.ObservationVariant,
            ActiveTorsionBranch = variant.TorsionVariant,
            ActiveShiabBranch = variant.ShiabVariant,
            ActiveGaugeStrategy = variant.GaugeVariant,
            BaseDimension = baseManifest.BaseDimension,
            AmbientDimension = baseManifest.AmbientDimension,
            LieAlgebraId = baseManifest.LieAlgebraId,
            BasisConventionId = baseManifest.BasisConventionId,
            ComponentOrderId = baseManifest.ComponentOrderId,
            AdjointConventionId = baseManifest.AdjointConventionId,
            PairingConventionId = variant.PairingVariant,
            NormConventionId = baseManifest.NormConventionId,
            DifferentialFormMetricId = baseManifest.DifferentialFormMetricId,
            InsertedAssumptionIds = baseManifest.InsertedAssumptionIds,
            InsertedChoiceIds = baseManifest.InsertedChoiceIds,
            Parameters = BuildParameters(variant, baseManifest),
        };
    }

    /// <summary>
    /// Validates the base manifest has required infrastructure fields and does not
    /// contain branch-sensitive parameter keys that would silently pass through.
    /// </summary>
    private static void ValidateBaseManifest(BranchManifest baseManifest)
    {
        if (string.IsNullOrWhiteSpace(baseManifest.ActiveGeometryBranch))
            throw new ArgumentException("Base manifest ActiveGeometryBranch must not be empty.", nameof(baseManifest));
        if (string.IsNullOrWhiteSpace(baseManifest.LieAlgebraId))
            throw new ArgumentException("Base manifest LieAlgebraId must not be empty.", nameof(baseManifest));
        if (string.IsNullOrWhiteSpace(baseManifest.DifferentialFormMetricId))
            throw new ArgumentException("Base manifest DifferentialFormMetricId must not be empty.", nameof(baseManifest));

        if (baseManifest.Parameters != null)
        {
            foreach (var key in baseManifest.Parameters.Keys)
            {
                if (BranchSensitiveParameterKeys.Contains(key))
                {
                    throw new ArgumentException(
                        $"Base manifest contains branch-sensitive parameter '{key}' that would be " +
                        "silently overridden. Remove it from the base manifest to avoid hidden defaults.",
                        nameof(baseManifest));
                }
            }
        }
    }

    private static IReadOnlyDictionary<string, string> BuildParameters(
        BranchVariantManifest variant,
        BranchManifest baseManifest)
    {
        var parameters = new Dictionary<string, string>();

        if (baseManifest.Parameters != null)
        {
            foreach (var kvp in baseManifest.Parameters)
                parameters[kvp.Key] = kvp.Value;
        }

        // Variant-specific branch-sensitive parameters (always override base)
        parameters["biConnectionStrategy"] = variant.BiConnectionVariant;
        parameters["a0Variant"] = variant.A0Variant;
        parameters["regularityVariant"] = variant.RegularityVariant;
        parameters["extractionVariant"] = variant.ExtractionVariant;

        return parameters;
    }
}
