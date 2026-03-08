using Gu.Core;

namespace Gu.Validation.Rules;

/// <summary>
/// A validation rule that checks whether a measured value is below a threshold.
/// </summary>
public sealed class ThresholdRule : IValidationRule
{
    private readonly Func<double> _measure;
    private readonly double _threshold;

    public ThresholdRule(string ruleId, string category, Func<double> measure, double threshold)
    {
        RuleId = ruleId;
        Category = category;
        _measure = measure;
        _threshold = threshold;
    }

    public string RuleId { get; }
    public string Category { get; }

    public ValidationRecord Execute()
    {
        var measured = _measure();
        var passed = measured <= _threshold;

        return new ValidationRecord
        {
            RuleId = RuleId,
            Category = Category,
            Passed = passed,
            MeasuredValue = measured,
            Tolerance = _threshold,
            Detail = passed
                ? $"Value {measured:E6} is below threshold {_threshold:E3}"
                : $"Value {measured:E6} exceeds threshold {_threshold:E3}",
            Timestamp = DateTimeOffset.UtcNow,
        };
    }
}
