using Gu.Core;

namespace Gu.Validation.Rules;

/// <summary>
/// A validation rule that checks whether a measured value is within tolerance of an expected value.
/// </summary>
public sealed class ToleranceRule : IValidationRule
{
    private readonly Func<double> _measure;
    private readonly double _expected;
    private readonly double _tolerance;

    public ToleranceRule(string ruleId, string category, Func<double> measure, double expected, double tolerance)
    {
        RuleId = ruleId;
        Category = category;
        _measure = measure;
        _expected = expected;
        _tolerance = tolerance;
    }

    public string RuleId { get; }
    public string Category { get; }

    public ValidationRecord Execute()
    {
        var measured = _measure();
        var diff = System.Math.Abs(measured - _expected);
        var passed = diff <= _tolerance;

        return new ValidationRecord
        {
            RuleId = RuleId,
            Category = Category,
            Passed = passed,
            MeasuredValue = measured,
            Tolerance = _tolerance,
            Detail = passed
                ? $"Value {measured:E6} within tolerance {_tolerance:E3} of expected {_expected:E6}"
                : $"Value {measured:E6} exceeds tolerance {_tolerance:E3} from expected {_expected:E6} (diff={diff:E6})",
            Timestamp = DateTimeOffset.UtcNow,
        };
    }
}
