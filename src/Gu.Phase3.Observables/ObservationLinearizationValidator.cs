using System.Text.Json.Serialization;

namespace Gu.Phase3.Observables;

/// <summary>
/// Validates analytic linearization against finite-difference approximation.
///
/// For each mode, compares the analytic observed signature with the FD signature
/// and reports relative error.
/// </summary>
public sealed class ObservationLinearizationValidator
{
    /// <summary>
    /// Compare an analytic signature against a finite-difference signature.
    /// Returns relative error: ||analytic - FD|| / ||FD||.
    /// </summary>
    public static double RelativeError(ObservedModeSignature analytic, ObservedModeSignature fd)
    {
        if (analytic == null) throw new ArgumentNullException(nameof(analytic));
        if (fd == null) throw new ArgumentNullException(nameof(fd));

        int len = System.Math.Min(analytic.ObservedCoefficients.Length, fd.ObservedCoefficients.Length);
        double normDiff = 0, normFd = 0;
        for (int i = 0; i < len; i++)
        {
            double diff = analytic.ObservedCoefficients[i] - fd.ObservedCoefficients[i];
            normDiff += diff * diff;
            normFd += fd.ObservedCoefficients[i] * fd.ObservedCoefficients[i];
        }

        if (normFd < 1e-30) return normDiff < 1e-30 ? 0.0 : double.PositiveInfinity;
        return System.Math.Sqrt(normDiff / normFd);
    }

    /// <summary>
    /// Validate a batch of mode signatures, comparing analytic vs FD.
    /// </summary>
    public static LinearizationValidationReport Validate(
        IReadOnlyList<ObservedModeSignature> analytic,
        IReadOnlyList<ObservedModeSignature> finiteDifference)
    {
        if (analytic == null) throw new ArgumentNullException(nameof(analytic));
        if (finiteDifference == null) throw new ArgumentNullException(nameof(finiteDifference));
        if (analytic.Count != finiteDifference.Count)
            throw new ArgumentException("Analytic and FD lists must have same length.");

        var entries = new List<ValidationEntry>();
        double maxError = 0;
        double sumError = 0;

        for (int i = 0; i < analytic.Count; i++)
        {
            double error = RelativeError(analytic[i], finiteDifference[i]);
            entries.Add(new ValidationEntry
            {
                ModeId = analytic[i].ModeId,
                RelativeError = error,
                AnalyticNorm = analytic[i].L2Norm,
                FiniteDifferenceNorm = finiteDifference[i].L2Norm,
            });
            if (error > maxError) maxError = error;
            sumError += error;
        }

        return new LinearizationValidationReport
        {
            Entries = entries,
            MaxRelativeError = maxError,
            MeanRelativeError = entries.Count > 0 ? sumError / entries.Count : 0.0,
        };
    }
}

/// <summary>
/// Result of comparing analytic vs FD linearization for a batch of modes.
/// </summary>
public sealed class LinearizationValidationReport
{
    [JsonPropertyName("entries")]
    public required IReadOnlyList<ValidationEntry> Entries { get; init; }

    [JsonPropertyName("maxRelativeError")]
    public required double MaxRelativeError { get; init; }

    [JsonPropertyName("meanRelativeError")]
    public required double MeanRelativeError { get; init; }
}

/// <summary>
/// Validation result for a single mode.
/// </summary>
public sealed class ValidationEntry
{
    [JsonPropertyName("modeId")]
    public required string ModeId { get; init; }

    [JsonPropertyName("relativeError")]
    public required double RelativeError { get; init; }

    [JsonPropertyName("analyticNorm")]
    public required double AnalyticNorm { get; init; }

    [JsonPropertyName("finiteDifferenceNorm")]
    public required double FiniteDifferenceNorm { get; init; }
}
