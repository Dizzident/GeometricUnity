using Gu.Core;

namespace Gu.Validation.Rules;

/// <summary>
/// A validation rule based on an arbitrary boolean predicate.
/// </summary>
public sealed class PredicateRule : IValidationRule
{
    private readonly Func<bool> _predicate;
    private readonly string _description;

    public PredicateRule(string ruleId, string category, Func<bool> predicate, string description)
    {
        RuleId = ruleId;
        Category = category;
        _predicate = predicate;
        _description = description;
    }

    public string RuleId { get; }
    public string Category { get; }

    public ValidationRecord Execute()
    {
        var passed = _predicate();
        return new ValidationRecord
        {
            RuleId = RuleId,
            Category = Category,
            Passed = passed,
            Detail = passed ? $"Passed: {_description}" : $"Failed: {_description}",
            Timestamp = DateTimeOffset.UtcNow,
        };
    }
}
