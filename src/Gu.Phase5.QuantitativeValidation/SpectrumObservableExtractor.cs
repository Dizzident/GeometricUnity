using Gu.Core;

namespace Gu.Phase5.QuantitativeValidation;

/// <summary>
/// Extracts simple spectrum-derived observables from externally generated eigenvalues.
/// </summary>
public static class SpectrumObservableExtractor
{
    public static double ComputePositiveModeRatio(double numeratorModeValue, double denominatorModeValue)
    {
        if (numeratorModeValue <= 0)
            throw new ArgumentOutOfRangeException(nameof(numeratorModeValue), "Numerator mode value must be positive.");
        if (denominatorModeValue <= 0)
            throw new ArgumentOutOfRangeException(nameof(denominatorModeValue), "Denominator mode value must be positive.");

        return numeratorModeValue / denominatorModeValue;
    }

    public static double PropagatePositiveModeRatioUncertainty(
        double numeratorModeValue,
        double numeratorModeUncertainty,
        double denominatorModeValue,
        double denominatorModeUncertainty)
    {
        if (numeratorModeValue <= 0)
            throw new ArgumentOutOfRangeException(nameof(numeratorModeValue), "Numerator mode value must be positive.");
        if (denominatorModeValue <= 0)
            throw new ArgumentOutOfRangeException(nameof(denominatorModeValue), "Denominator mode value must be positive.");
        if (numeratorModeUncertainty < 0)
            throw new ArgumentOutOfRangeException(nameof(numeratorModeUncertainty), "Numerator mode uncertainty must be nonnegative.");
        if (denominatorModeUncertainty < 0)
            throw new ArgumentOutOfRangeException(nameof(denominatorModeUncertainty), "Denominator mode uncertainty must be nonnegative.");

        var ratio = ComputePositiveModeRatio(numeratorModeValue, denominatorModeValue);
        var relativeNumerator = numeratorModeUncertainty / numeratorModeValue;
        var relativeDenominator = denominatorModeUncertainty / denominatorModeValue;
        return ratio * System.Math.Sqrt(
            System.Math.Pow(relativeNumerator, 2) +
            System.Math.Pow(relativeDenominator, 2));
    }

    public static QuantitativeObservableRecord CreatePositiveModeRatioRecord(
        double numeratorModeValue,
        double numeratorModeUncertainty,
        string numeratorModeId,
        double denominatorModeValue,
        double denominatorModeUncertainty,
        string denominatorModeId,
        string observableId,
        string environmentId,
        string branchId,
        string? refinementLevel,
        ProvenanceMeta provenance)
    {
        if (string.IsNullOrWhiteSpace(numeratorModeId))
            throw new ArgumentException("numeratorModeId is required.", nameof(numeratorModeId));
        if (string.IsNullOrWhiteSpace(denominatorModeId))
            throw new ArgumentException("denominatorModeId is required.", nameof(denominatorModeId));
        if (string.IsNullOrWhiteSpace(observableId))
            throw new ArgumentException("observableId is required.", nameof(observableId));
        if (string.IsNullOrWhiteSpace(environmentId))
            throw new ArgumentException("environmentId is required.", nameof(environmentId));
        if (string.IsNullOrWhiteSpace(branchId))
            throw new ArgumentException("branchId is required.", nameof(branchId));

        var value = ComputePositiveModeRatio(numeratorModeValue, denominatorModeValue);
        var uncertainty = PropagatePositiveModeRatioUncertainty(
            numeratorModeValue,
            numeratorModeUncertainty,
            denominatorModeValue,
            denominatorModeUncertainty);

        return new QuantitativeObservableRecord
        {
            ObservableId = observableId,
            Value = value,
            Uncertainty = new QuantitativeUncertainty
            {
                BranchVariation = -1,
                RefinementError = -1,
                ExtractionError = uncertainty,
                EnvironmentSensitivity = -1,
                TotalUncertainty = uncertainty,
            },
            BranchId = branchId,
            EnvironmentId = environmentId,
            RefinementLevel = refinementLevel,
            ExtractionMethod = $"positive-mode-ratio:{numeratorModeId}/{denominatorModeId}",
            Provenance = provenance,
        };
    }

    public static double ComputeAdjacentGapRatio(IReadOnlyList<double> eigenvalues, int gapIndex)
    {
        ArgumentNullException.ThrowIfNull(eigenvalues);

        if (eigenvalues.Count < 3)
            throw new ArgumentException("At least three eigenvalues are required to compute an adjacent gap ratio.", nameof(eigenvalues));
        if (gapIndex < 0 || gapIndex + 2 >= eigenvalues.Count)
            throw new ArgumentOutOfRangeException(nameof(gapIndex), "gapIndex must select two adjacent gaps.");

        for (int i = 1; i < eigenvalues.Count; i++)
        {
            if (eigenvalues[i] < eigenvalues[i - 1])
                throw new ArgumentException("Eigenvalues must be sorted in nondecreasing order.", nameof(eigenvalues));
        }

        var firstGap = eigenvalues[gapIndex + 1] - eigenvalues[gapIndex];
        var secondGap = eigenvalues[gapIndex + 2] - eigenvalues[gapIndex + 1];
        if (firstGap <= 0 || secondGap <= 0)
            throw new ArgumentException("Selected eigenvalue gaps must be strictly positive.", nameof(eigenvalues));

        return System.Math.Min(firstGap, secondGap) / System.Math.Max(firstGap, secondGap);
    }

    public static QuantitativeObservableRecord CreateAdjacentGapRatioRecord(
        IReadOnlyList<double> eigenvalues,
        int gapIndex,
        string observableId,
        string environmentId,
        string branchId,
        string? refinementLevel,
        double extractionUncertainty,
        ProvenanceMeta provenance)
    {
        if (string.IsNullOrWhiteSpace(observableId))
            throw new ArgumentException("observableId is required.", nameof(observableId));
        if (string.IsNullOrWhiteSpace(environmentId))
            throw new ArgumentException("environmentId is required.", nameof(environmentId));
        if (string.IsNullOrWhiteSpace(branchId))
            throw new ArgumentException("branchId is required.", nameof(branchId));
        if (extractionUncertainty < 0)
            throw new ArgumentOutOfRangeException(nameof(extractionUncertainty), "extractionUncertainty must be nonnegative.");

        var value = ComputeAdjacentGapRatio(eigenvalues, gapIndex);
        return new QuantitativeObservableRecord
        {
            ObservableId = observableId,
            Value = value,
            Uncertainty = new QuantitativeUncertainty
            {
                BranchVariation = -1,
                RefinementError = -1,
                ExtractionError = extractionUncertainty,
                EnvironmentSensitivity = -1,
                TotalUncertainty = extractionUncertainty,
            },
            BranchId = branchId,
            EnvironmentId = environmentId,
            RefinementLevel = refinementLevel,
            ExtractionMethod = $"adjacent-gap-ratio:index-{gapIndex}",
            Provenance = provenance,
        };
    }
}
