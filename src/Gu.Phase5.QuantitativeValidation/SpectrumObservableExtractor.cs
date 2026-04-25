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

    public static QuantitativeObservableRecord CreatePositiveModeRatioRecord(
        QuantitativeObservableRecord numeratorMode,
        QuantitativeObservableRecord denominatorMode,
        string observableId,
        ProvenanceMeta provenance)
    {
        ArgumentNullException.ThrowIfNull(numeratorMode);
        ArgumentNullException.ThrowIfNull(denominatorMode);
        if (string.IsNullOrWhiteSpace(observableId))
            throw new ArgumentException("observableId is required.", nameof(observableId));
        if (!string.Equals(numeratorMode.EnvironmentId, denominatorMode.EnvironmentId, StringComparison.Ordinal))
            throw new ArgumentException("Mode ratio inputs must come from the same environment.");
        if (!string.Equals(numeratorMode.BranchId, denominatorMode.BranchId, StringComparison.Ordinal))
            throw new ArgumentException("Mode ratio inputs must come from the same branch.");
        if (!string.Equals(numeratorMode.RefinementLevel, denominatorMode.RefinementLevel, StringComparison.Ordinal))
            throw new ArgumentException("Mode ratio inputs must come from the same refinement level.");
        if (numeratorMode.Uncertainty.TotalUncertainty < 0)
            throw new ArgumentException("Numerator mode total uncertainty must be estimated.", nameof(numeratorMode));
        if (denominatorMode.Uncertainty.TotalUncertainty < 0)
            throw new ArgumentException("Denominator mode total uncertainty must be estimated.", nameof(denominatorMode));

        return CreatePositiveModeRatioRecord(
            numeratorMode.Value,
            numeratorMode.Uncertainty.TotalUncertainty,
            numeratorMode.ObservableId,
            denominatorMode.Value,
            denominatorMode.Uncertainty.TotalUncertainty,
            denominatorMode.ObservableId,
            observableId,
            numeratorMode.EnvironmentId,
            numeratorMode.BranchId,
            numeratorMode.RefinementLevel,
            provenance);
    }

    public static QuantitativeObservableRecord CreatePositiveModeRatioRecord(
        IdentifiedPhysicalModeRecord numeratorMode,
        IdentifiedPhysicalModeRecord denominatorMode,
        string observableId,
        ProvenanceMeta provenance)
        => CreatePositiveModeRatioRecord(
            numeratorMode,
            denominatorMode,
            observableId,
            provenance,
            evidenceRecords: null,
            requireValidatedEvidence: false);

    public static QuantitativeObservableRecord CreatePositiveModeRatioRecord(
        IdentifiedPhysicalModeRecord numeratorMode,
        IdentifiedPhysicalModeRecord denominatorMode,
        string observableId,
        ProvenanceMeta provenance,
        IReadOnlyList<ModeIdentificationEvidenceRecord>? evidenceRecords,
        bool requireValidatedEvidence)
    {
        ArgumentNullException.ThrowIfNull(numeratorMode);
        ArgumentNullException.ThrowIfNull(denominatorMode);
        if (requireValidatedEvidence)
        {
            PhysicalModeEvidenceValidator.ThrowIfInvalidForPrediction(numeratorMode, evidenceRecords);
            PhysicalModeEvidenceValidator.ThrowIfInvalidForPrediction(denominatorMode, evidenceRecords);
        }

        if (!string.Equals(numeratorMode.Status, "validated", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Numerator physical mode must be validated.", nameof(numeratorMode));
        if (!string.Equals(denominatorMode.Status, "validated", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Denominator physical mode must be validated.", nameof(denominatorMode));
        if (!string.Equals(numeratorMode.UnitFamily, denominatorMode.UnitFamily, StringComparison.Ordinal))
            throw new ArgumentException("Mode ratio inputs must share unitFamily.");
        if (!string.Equals(numeratorMode.Unit, denominatorMode.Unit, StringComparison.Ordinal))
            throw new ArgumentException("Mode ratio inputs must share unit.");

        var numeratorObservable = ToObservableRecord(numeratorMode);
        var denominatorObservable = ToObservableRecord(denominatorMode);
        return CreatePositiveModeRatioRecord(
            numeratorObservable,
            denominatorObservable,
            observableId,
            provenance);
    }

    private static QuantitativeObservableRecord ToObservableRecord(IdentifiedPhysicalModeRecord mode)
    {
        return new QuantitativeObservableRecord
        {
            ObservableId = mode.ObservableId,
            Value = mode.Value,
            Uncertainty = new QuantitativeUncertainty
            {
                BranchVariation = -1,
                RefinementError = -1,
                ExtractionError = mode.Uncertainty,
                EnvironmentSensitivity = -1,
                TotalUncertainty = mode.Uncertainty,
            },
            BranchId = mode.BranchId,
            EnvironmentId = mode.EnvironmentId,
            RefinementLevel = mode.RefinementLevel,
            ExtractionMethod = mode.ExtractionMethod,
            Provenance = mode.Provenance,
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
