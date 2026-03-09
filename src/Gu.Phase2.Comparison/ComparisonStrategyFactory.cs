using Gu.Phase2.Semantics;

namespace Gu.Phase2.Comparison;

/// <summary>
/// Maps ComparisonMode to the appropriate IComparisonStrategy implementation.
/// </summary>
public static class ComparisonStrategyFactory
{
    /// <summary>
    /// Create a comparison strategy for the given mode.
    /// </summary>
    public static IComparisonStrategy Create(ComparisonMode mode) => mode switch
    {
        ComparisonMode.Structural => new StructuralComparisonStrategy(),
        ComparisonMode.SemiQuantitative => new SemiQuantitativeComparisonStrategy(),
        ComparisonMode.Quantitative => new QuantitativeComparisonStrategy(),
        _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unknown comparison mode"),
    };
}
