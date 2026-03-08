using Gu.Core;

namespace Gu.Validation;

/// <summary>
/// Executes a set of validation rules and produces a ValidationBundle.
/// </summary>
public sealed class ValidationRunner
{
    private readonly List<IValidationRule> _rules = new();

    /// <summary>
    /// Register a validation rule.
    /// </summary>
    public ValidationRunner AddRule(IValidationRule rule)
    {
        _rules.Add(rule);
        return this;
    }

    /// <summary>
    /// Register multiple validation rules.
    /// </summary>
    public ValidationRunner AddRules(IEnumerable<IValidationRule> rules)
    {
        _rules.AddRange(rules);
        return this;
    }

    /// <summary>
    /// Execute all registered rules and produce a ValidationBundle.
    /// </summary>
    public ValidationBundle Run(BranchRef branch)
    {
        var records = new List<ValidationRecord>();

        foreach (var rule in _rules)
        {
            records.Add(rule.Execute());
        }

        return new ValidationBundle
        {
            Branch = branch,
            Records = records,
            AllPassed = records.All(r => r.Passed),
        };
    }

    /// <summary>
    /// Execute all registered rules. Throws if any fail.
    /// </summary>
    public ValidationBundle RunOrThrow(BranchRef branch)
    {
        var bundle = Run(branch);
        if (!bundle.AllPassed)
        {
            var failures = bundle.Records
                .Where(r => !r.Passed)
                .Select(r => $"  [{r.RuleId}] {r.Detail ?? "failed"}")
                .ToList();
            throw new InvalidOperationException(
                $"Validation failed with {failures.Count} error(s):\n{string.Join("\n", failures)}");
        }
        return bundle;
    }
}
