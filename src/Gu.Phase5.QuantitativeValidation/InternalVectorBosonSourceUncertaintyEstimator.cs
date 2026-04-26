namespace Gu.Phase5.QuantitativeValidation;

public static class InternalVectorBosonSourceUncertaintyEstimator
{
    public static QuantitativeUncertainty Estimate(IReadOnlyList<InternalVectorBosonSourceModeRecord> modes)
    {
        ArgumentNullException.ThrowIfNull(modes);
        if (modes.Count == 0)
            return new QuantitativeUncertainty();

        var extractionError = modes.All(m => m.ExtractionError >= 0)
            ? RootMeanSquare(modes.Select(m => m.ExtractionError))
            : -1;
        var branchVariation = AxisVariation(modes, m => m.BranchVariantId);
        var refinementError = RefinementError(modes);
        var environmentSensitivity = AxisVariation(modes, m => m.EnvironmentId);

        var total = extractionError >= 0 &&
                    branchVariation >= 0 &&
                    refinementError >= 0 &&
                    environmentSensitivity >= 0
            ? global::System.Math.Sqrt(
                extractionError * extractionError +
                branchVariation * branchVariation +
                refinementError * refinementError +
                environmentSensitivity * environmentSensitivity)
            : -1;

        return new QuantitativeUncertainty
        {
            ExtractionError = extractionError,
            BranchVariation = branchVariation,
            RefinementError = refinementError,
            EnvironmentSensitivity = environmentSensitivity,
            TotalUncertainty = total,
        };
    }

    private static double AxisVariation(
        IReadOnlyList<InternalVectorBosonSourceModeRecord> modes,
        Func<InternalVectorBosonSourceModeRecord, string> keySelector)
    {
        var groups = modes
            .GroupBy(keySelector, StringComparer.Ordinal)
            .Select(g => g.Average(m => m.MassLikeValue))
            .ToList();
        return groups.Count >= 2 ? StandardDeviation(groups) : -1;
    }

    private static double RefinementError(IReadOnlyList<InternalVectorBosonSourceModeRecord> modes)
    {
        var levels = modes
            .GroupBy(m => m.RefinementLevel, StringComparer.Ordinal)
            .OrderBy(g => g.Key, StringComparer.Ordinal)
            .Select(g => g.Average(m => m.MassLikeValue))
            .ToList();
        return levels.Count >= 2 ? global::System.Math.Abs(levels[^1] - levels[^2]) : -1;
    }

    private static double RootMeanSquare(IEnumerable<double> values)
    {
        var list = values.ToList();
        return list.Count == 0 ? -1 : global::System.Math.Sqrt(list.Average(v => v * v));
    }

    private static double StandardDeviation(IReadOnlyList<double> values)
    {
        if (values.Count < 2)
            return -1;
        var mean = values.Average();
        return global::System.Math.Sqrt(values.Sum(v => (v - mean) * (v - mean)) / (values.Count - 1));
    }
}
