using System.Text.Json.Serialization;

namespace Gu.Phase5.Reporting;

public sealed class WeakCouplingUncertaintyPropagationResult
{
    [JsonPropertyName("algorithmId")]
    public required string AlgorithmId { get; init; }

    [JsonPropertyName("terminalStatus")]
    public required string TerminalStatus { get; init; }

    [JsonPropertyName("rawMatrixElementUncertainty")]
    public double? RawMatrixElementUncertainty { get; init; }

    [JsonPropertyName("generatorNormalizationScaleUncertainty")]
    public double? GeneratorNormalizationScaleUncertainty { get; init; }

    [JsonPropertyName("candidate")]
    public NormalizedWeakCouplingCandidateRecord? Candidate { get; init; }

    [JsonPropertyName("closureRequirements")]
    public required IReadOnlyList<string> ClosureRequirements { get; init; }
}

public static class WeakCouplingUncertaintyPropagator
{
    public const string AlgorithmId = "phase66-weak-coupling-uncertainty-propagator-v1";

    public static WeakCouplingUncertaintyPropagationResult Propagate(
        DimensionlessWeakCouplingAmplitudeExtractionResult extraction,
        double rawMatrixElementUncertainty,
        double generatorNormalizationScaleUncertainty)
    {
        ArgumentNullException.ThrowIfNull(extraction);

        var closure = new List<string>();
        if (!string.Equals(extraction.TerminalStatus, "dimensionless-weak-coupling-amplitude-extracted", StringComparison.Ordinal))
            closure.Add("dimensionless weak-coupling amplitude has not been extracted");
        if (extraction.Candidate is null)
            closure.Add("weak-coupling candidate is missing");
        if (extraction.RawMatrixElementMagnitude is not { } raw || !double.IsFinite(raw) || raw <= 0.0)
            closure.Add("raw matrix-element magnitude must be finite and positive");
        if (extraction.GeneratorNormalizationScale is not { } scale || !double.IsFinite(scale) || scale <= 0.0)
            closure.Add("generator normalization scale must be finite and positive");
        if (!double.IsFinite(rawMatrixElementUncertainty) || rawMatrixElementUncertainty < 0.0)
            closure.Add("raw matrix-element uncertainty must be finite and non-negative");
        if (!double.IsFinite(generatorNormalizationScaleUncertainty) || generatorNormalizationScaleUncertainty < 0.0)
            closure.Add("generator normalization scale uncertainty must be finite and non-negative");

        NormalizedWeakCouplingCandidateRecord? candidate = null;
        if (closure.Count == 0)
        {
            var uncertainty = System.Math.Sqrt(
                System.Math.Pow(extraction.GeneratorNormalizationScale!.Value * rawMatrixElementUncertainty, 2) +
                System.Math.Pow(extraction.RawMatrixElementMagnitude!.Value * generatorNormalizationScaleUncertainty, 2));
            candidate = extraction.Candidate! with
            {
                CouplingUncertainty = uncertainty,
            };
        }

        return new WeakCouplingUncertaintyPropagationResult
        {
            AlgorithmId = AlgorithmId,
            TerminalStatus = closure.Count == 0
                ? "weak-coupling-uncertainty-propagated"
                : "weak-coupling-uncertainty-blocked",
            RawMatrixElementUncertainty = double.IsFinite(rawMatrixElementUncertainty) ? rawMatrixElementUncertainty : null,
            GeneratorNormalizationScaleUncertainty = double.IsFinite(generatorNormalizationScaleUncertainty)
                ? generatorNormalizationScaleUncertainty
                : null,
            Candidate = candidate,
            ClosureRequirements = closure,
        };
    }
}
