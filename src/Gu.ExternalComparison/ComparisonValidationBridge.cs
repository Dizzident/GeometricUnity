using Gu.Core;

namespace Gu.ExternalComparison;

/// <summary>
/// Converts ComparisonRecords into ValidationRecords so comparison results
/// can be included in the standard ValidationBundle.
/// </summary>
public static class ComparisonValidationBridge
{
    /// <summary>
    /// Convert a ComparisonRecord into a ValidationRecord.
    /// </summary>
    public static ValidationRecord ToValidationRecord(ComparisonRecord comparison)
    {
        ArgumentNullException.ThrowIfNull(comparison);

        return new ValidationRecord
        {
            RuleId = $"comparison:{comparison.TemplateId}",
            Category = "external-comparison",
            Passed = comparison.Outcome == ComparisonOutcome.Pass,
            MeasuredValue = GetPrimaryMetric(comparison),
            Tolerance = GetToleranceFromMetrics(comparison),
            Detail = comparison.Message,
            Timestamp = comparison.ExecutedAt,
        };
    }

    /// <summary>
    /// Convert a list of ComparisonRecords into ValidationRecords.
    /// </summary>
    public static IReadOnlyList<ValidationRecord> ToValidationRecords(
        IReadOnlyList<ComparisonRecord> comparisons)
    {
        ArgumentNullException.ThrowIfNull(comparisons);

        var records = new List<ValidationRecord>(comparisons.Count);
        foreach (var comparison in comparisons)
        {
            records.Add(ToValidationRecord(comparison));
        }
        return records;
    }

    private static double? GetPrimaryMetric(ComparisonRecord comparison)
    {
        // Return the most relevant metric as measured value
        if (comparison.Metrics.TryGetValue("maxRelativeError", out var relErr))
            return relErr;
        if (comparison.Metrics.TryGetValue("maxDeviation", out var dev))
            return dev;
        if (comparison.Metrics.TryGetValue("maxFractionalPart", out var frac))
            return frac;
        if (comparison.Metrics.TryGetValue("maxOrderDifference", out var orderDiff))
            return orderDiff;
        if (comparison.Metrics.TryGetValue("deviation", out var countDev))
            return countDev;
        return null;
    }

    private static double? GetToleranceFromMetrics(ComparisonRecord comparison)
    {
        // Tolerance info is in the template, not the record itself.
        // Return null since we don't embed template tolerance in the record.
        return null;
    }
}
