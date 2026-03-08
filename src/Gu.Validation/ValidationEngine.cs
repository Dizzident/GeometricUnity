using Gu.Core;
using Gu.Math;

namespace Gu.Validation;

/// <summary>
/// Validation engine that runs all applicable validation rules against a state
/// and produces a ValidationBundle summarizing pass/fail for each rule.
/// </summary>
public sealed class ValidationEngine
{
    /// <summary>Default tolerance for numeric validation checks.</summary>
    public const double DefaultTolerance = 1e-12;

    private readonly double _tolerance;

    /// <summary>
    /// Create a validation engine with the specified tolerance.
    /// </summary>
    public ValidationEngine(double tolerance = DefaultTolerance)
    {
        _tolerance = tolerance;
    }

    /// <summary>
    /// Run all applicable validation rules for a complete state (algebra + derived + manifest).
    /// Returns a ValidationBundle with per-rule results.
    /// </summary>
    public ValidationBundle ValidateAll(
        BranchRef branch,
        LieAlgebra algebra,
        DerivedState derived,
        BranchManifest manifest)
    {
        var registry = ValidationRuleRegistry.CreateFullRegistry(algebra, derived, manifest, _tolerance);
        var runner = new ValidationRunner();
        runner.AddRules(registry.Rules);
        return runner.Run(branch);
    }

    /// <summary>
    /// Run algebraic identity validation rules only (Jacobi, antisymmetry, metric symmetry).
    /// </summary>
    public ValidationBundle ValidateAlgebraicIdentities(BranchRef branch, LieAlgebra algebra)
    {
        var registry = ValidationRuleRegistry.CreateAlgebraicRules(algebra, _tolerance);
        var runner = new ValidationRunner();
        runner.AddRules(registry.Rules);
        return runner.Run(branch);
    }

    /// <summary>
    /// Run signature validation rules only (carrier compatibility, shape matching).
    /// </summary>
    public ValidationBundle ValidateSignatures(BranchRef branch, DerivedState derived)
    {
        var registry = ValidationRuleRegistry.CreateSignatureRules(derived);
        var runner = new ValidationRunner();
        runner.AddRules(registry.Rules);
        return runner.Run(branch);
    }

    /// <summary>
    /// Run manifest completeness validation only.
    /// </summary>
    public ValidationBundle ValidateManifest(BranchRef branch, BranchManifest manifest)
    {
        var registry = ValidationRuleRegistry.CreateManifestRules(manifest);
        var runner = new ValidationRunner();
        runner.AddRules(registry.Rules);
        return runner.Run(branch);
    }

    /// <summary>
    /// Run a custom set of rules from a registry.
    /// </summary>
    public ValidationBundle ValidateWithRegistry(BranchRef branch, ValidationRuleRegistry registry)
    {
        var runner = new ValidationRunner();
        runner.AddRules(registry.Rules);
        return runner.Run(branch);
    }
}
