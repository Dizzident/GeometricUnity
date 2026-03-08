using Gu.Core;

namespace Gu.Validation.Rules;

/// <summary>
/// A validation rule that checks parity between two sets of values (e.g., CPU vs GPU).
/// </summary>
public sealed class ParityRule : IValidationRule
{
    private readonly Func<double[]> _getValuesA;
    private readonly Func<double[]> _getValuesB;
    private readonly double _tolerance;
    private readonly string _labelA;
    private readonly string _labelB;

    public ParityRule(
        string ruleId,
        string category,
        Func<double[]> getValuesA,
        string labelA,
        Func<double[]> getValuesB,
        string labelB,
        double tolerance)
    {
        RuleId = ruleId;
        Category = category;
        _getValuesA = getValuesA;
        _labelA = labelA;
        _getValuesB = getValuesB;
        _labelB = labelB;
        _tolerance = tolerance;
    }

    public string RuleId { get; }
    public string Category { get; }

    public ValidationRecord Execute()
    {
        var valuesA = _getValuesA();
        var valuesB = _getValuesB();

        if (valuesA.Length != valuesB.Length)
        {
            return new ValidationRecord
            {
                RuleId = RuleId,
                Category = Category,
                Passed = false,
                Detail = $"Length mismatch: {_labelA} has {valuesA.Length}, {_labelB} has {valuesB.Length}",
                Timestamp = DateTimeOffset.UtcNow,
            };
        }

        double maxDiff = 0;
        int maxDiffIndex = 0;
        for (int i = 0; i < valuesA.Length; i++)
        {
            var diff = System.Math.Abs(valuesA[i] - valuesB[i]);
            if (diff > maxDiff)
            {
                maxDiff = diff;
                maxDiffIndex = i;
            }
        }

        var passed = maxDiff <= _tolerance;
        return new ValidationRecord
        {
            RuleId = RuleId,
            Category = Category,
            Passed = passed,
            MeasuredValue = maxDiff,
            Tolerance = _tolerance,
            Detail = passed
                ? $"Max difference between {_labelA} and {_labelB}: {maxDiff:E6} at index {maxDiffIndex} (tolerance: {_tolerance:E3})"
                : $"Parity failure between {_labelA} and {_labelB}: max diff {maxDiff:E6} at index {maxDiffIndex} exceeds tolerance {_tolerance:E3}",
            Timestamp = DateTimeOffset.UtcNow,
        };
    }
}
