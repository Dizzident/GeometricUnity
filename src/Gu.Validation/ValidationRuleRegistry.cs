using Gu.Core;
using Gu.Math;
using Gu.Validation.Rules;

namespace Gu.Validation;

/// <summary>
/// Registry of validation rules organized by category.
/// Provides factory methods to create rules for common validation scenarios.
/// </summary>
public sealed class ValidationRuleRegistry
{
    /// <summary>Category for algebraic identity rules (Jacobi, antisymmetry, metric symmetry).</summary>
    public const string AlgebraicIdentity = "algebraic-identity";

    /// <summary>Category for conservation law rules.</summary>
    public const string Conservation = "conservation";

    /// <summary>Category for parity rules (CPU vs CUDA comparison).</summary>
    public const string Parity = "parity";

    /// <summary>Category for convergence rules.</summary>
    public const string Convergence = "convergence";

    /// <summary>Category for signature and structural rules.</summary>
    public const string Signature = "signature";

    private readonly List<IValidationRule> _rules = new();

    /// <summary>
    /// All registered rules.
    /// </summary>
    public IReadOnlyList<IValidationRule> Rules => _rules;

    /// <summary>
    /// Register a rule.
    /// </summary>
    public ValidationRuleRegistry Add(IValidationRule rule)
    {
        _rules.Add(rule);
        return this;
    }

    /// <summary>
    /// Get all rules in a given category.
    /// </summary>
    public IReadOnlyList<IValidationRule> GetByCategory(string category) =>
        _rules.Where(r => r.Category == category).ToList();

    /// <summary>
    /// Create a registry populated with algebraic identity rules for a given Lie algebra.
    /// </summary>
    public static ValidationRuleRegistry CreateAlgebraicRules(LieAlgebra algebra, double tolerance = 1e-12)
    {
        var registry = new ValidationRuleRegistry();

        registry.Add(new ThresholdRule(
            "jacobi-identity",
            AlgebraicIdentity,
            () => algebra.ValidateJacobiIdentity(),
            tolerance));

        registry.Add(new ThresholdRule(
            "antisymmetry",
            AlgebraicIdentity,
            () => algebra.ValidateAntisymmetry(),
            tolerance));

        registry.Add(new ThresholdRule(
            "metric-symmetry",
            AlgebraicIdentity,
            () => algebra.ValidateMetricSymmetry(),
            tolerance));

        return registry;
    }

    /// <summary>
    /// Create signature validation rules for a DerivedState (carrier compatibility, shape matching).
    /// </summary>
    public static ValidationRuleRegistry CreateSignatureRules(DerivedState derived)
    {
        var registry = new ValidationRuleRegistry();

        registry.Add(new PredicateRule(
            "carrier-compatibility",
            Signature,
            () => derived.TorsionT.Signature.CarrierType == derived.ShiabS.Signature.CarrierType,
            "T_h and S_h must have the same carrier type"));

        registry.Add(new PredicateRule(
            "shape-coefficient-match",
            Signature,
            () => ShapeMatchesCoefficients(derived.CurvatureF)
                && ShapeMatchesCoefficients(derived.TorsionT)
                && ShapeMatchesCoefficients(derived.ShiabS)
                && ShapeMatchesCoefficients(derived.ResidualUpsilon),
            "FieldTensor shape must match coefficient count for all derived fields"));

        return registry;
    }

    /// <summary>
    /// Create manifest completeness validation rules.
    /// </summary>
    public static ValidationRuleRegistry CreateManifestRules(BranchManifest manifest)
    {
        var registry = new ValidationRuleRegistry();

        registry.Add(new PredicateRule(
            "manifest-complete",
            Signature,
            () => ManifestHasNoUnsetFields(manifest),
            "BranchManifest must have no 'unset' fields"));

        return registry;
    }

    /// <summary>
    /// Create a full validation registry combining algebraic, signature, and manifest rules.
    /// </summary>
    public static ValidationRuleRegistry CreateFullRegistry(
        LieAlgebra algebra,
        DerivedState derived,
        BranchManifest manifest,
        double tolerance = 1e-12)
    {
        var registry = new ValidationRuleRegistry();

        foreach (var rule in CreateAlgebraicRules(algebra, tolerance).Rules)
            registry.Add(rule);

        foreach (var rule in CreateSignatureRules(derived).Rules)
            registry.Add(rule);

        foreach (var rule in CreateManifestRules(manifest).Rules)
            registry.Add(rule);

        return registry;
    }

    /// <summary>
    /// Checks whether a FieldTensor's shape matches its coefficient count.
    /// The product of shape dimensions must equal the number of coefficients.
    /// </summary>
    public static bool ShapeMatchesCoefficients(FieldTensor field)
    {
        if (field.Shape.Count == 0)
            return field.Coefficients.Length == 0;

        int product = 1;
        for (int i = 0; i < field.Shape.Count; i++)
            product *= field.Shape[i];

        return product == field.Coefficients.Length;
    }

    /// <summary>
    /// Checks that no convention fields in the manifest are set to "unset".
    /// </summary>
    public static bool ManifestHasNoUnsetFields(BranchManifest manifest)
    {
        var fieldsToCheck = new[]
        {
            manifest.SourceEquationRevision,
            manifest.CodeRevision,
            manifest.ActiveGeometryBranch,
            manifest.ActiveObservationBranch,
            manifest.ActiveTorsionBranch,
            manifest.ActiveShiabBranch,
            manifest.ActiveGaugeStrategy,
            manifest.LieAlgebraId,
            manifest.BasisConventionId,
            manifest.ComponentOrderId,
            manifest.AdjointConventionId,
            manifest.PairingConventionId,
            manifest.NormConventionId,
            manifest.DifferentialFormMetricId,
        };

        return !fieldsToCheck.Any(f =>
            string.IsNullOrWhiteSpace(f) || string.Equals(f, "unset", StringComparison.OrdinalIgnoreCase));
    }
}
