using System.Text.Json.Serialization;

namespace Gu.Phase5.Reporting;

public sealed class WeakCouplingAmplitudeNormalizationAuditResult
{
    [JsonPropertyName("algorithmId")]
    public required string AlgorithmId { get; init; }

    [JsonPropertyName("terminalStatus")]
    public required string TerminalStatus { get; init; }

    [JsonPropertyName("currentRawMatrixElementMagnitude")]
    public double? CurrentRawMatrixElementMagnitude { get; init; }

    [JsonPropertyName("currentGeneratorNormalizationScale")]
    public double? CurrentGeneratorNormalizationScale { get; init; }

    [JsonPropertyName("currentWeakCoupling")]
    public double? CurrentWeakCoupling { get; init; }

    [JsonPropertyName("targetImpliedWeakCoupling")]
    public double? TargetImpliedWeakCoupling { get; init; }

    [JsonPropertyName("targetImpliedRawMatrixElementMagnitude")]
    public double? TargetImpliedRawMatrixElementMagnitude { get; init; }

    [JsonPropertyName("rawMatrixElementRequiredScale")]
    public double? RawMatrixElementRequiredScale { get; init; }

    [JsonPropertyName("targetImpliedGeneratorScaleIfRawHeldFixed")]
    public double? TargetImpliedGeneratorScaleIfRawHeldFixed { get; init; }

    [JsonPropertyName("canonicalTraceHalfGeneratorScale")]
    public double? CanonicalTraceHalfGeneratorScale { get; init; }

    [JsonPropertyName("generatorNormalizationCanExplainMiss")]
    public required bool GeneratorNormalizationCanExplainMiss { get; init; }

    [JsonPropertyName("diagnosis")]
    public required IReadOnlyList<string> Diagnosis { get; init; }

    [JsonPropertyName("closureRequirements")]
    public required IReadOnlyList<string> ClosureRequirements { get; init; }
}

public static class WeakCouplingAmplitudeNormalizationAudit
{
    public const string AlgorithmId = "phase76-weak-coupling-amplitude-normalization-audit-v1";

    public static WeakCouplingAmplitudeNormalizationAuditResult Audit(
        DimensionlessWeakCouplingAmplitudeExtractionResult extraction,
        WzAbsoluteMassMissDiagnosticResult missDiagnostic,
        double generatorScaleTolerance = 1e-12)
    {
        ArgumentNullException.ThrowIfNull(extraction);
        ArgumentNullException.ThrowIfNull(missDiagnostic);

        var closure = new List<string>();
        if (!string.Equals(
                extraction.TerminalStatus,
                "dimensionless-weak-coupling-amplitude-extracted",
                StringComparison.Ordinal))
            closure.Add("dimensionless weak-coupling amplitude has not been extracted");
        var raw = extraction.RawMatrixElementMagnitude;
        var generatorScale = extraction.GeneratorNormalizationScale;
        var currentCoupling = extraction.Candidate?.CouplingValue;
        var requiredCoupling = missDiagnostic.RequiredWeakCoupling;

        if (raw is not { } || !double.IsFinite(raw.Value) || raw.Value <= 0.0)
            closure.Add("raw matrix-element magnitude must be finite and positive");
        if (generatorScale is not { } || !double.IsFinite(generatorScale.Value) || generatorScale.Value <= 0.0)
            closure.Add("generator normalization scale must be finite and positive");
        if (currentCoupling is not { } || !double.IsFinite(currentCoupling.Value) || currentCoupling.Value <= 0.0)
            closure.Add("current weak-coupling candidate value must be finite and positive");
        if (requiredCoupling is not { } || !double.IsFinite(requiredCoupling.Value) || requiredCoupling.Value <= 0.0)
            closure.Add("target-implied weak coupling from miss diagnostic must be finite and positive");
        if (!double.IsFinite(generatorScaleTolerance) || generatorScaleTolerance <= 0.0)
            closure.Add("generator scale tolerance must be finite and positive");

        double? targetImpliedRaw = null;
        double? rawScale = null;
        double? targetImpliedGeneratorScale = null;
        var generatorCanExplain = false;
        var diagnosis = new List<string>();

        if (closure.Count == 0)
        {
            targetImpliedRaw = requiredCoupling!.Value / generatorScale!.Value;
            rawScale = targetImpliedRaw.Value / raw!.Value;
            targetImpliedGeneratorScale = requiredCoupling.Value / raw.Value;
            generatorCanExplain = System.Math.Abs(targetImpliedGeneratorScale.Value - generatorScale.Value) <= generatorScaleTolerance;

            diagnosis.Add($"Phase65 coupling is raw-matrix-element magnitude {raw.Value:R} times generator scale {generatorScale.Value:R}");
            diagnosis.Add($"Phase75 target-implied weak coupling would require raw matrix-element magnitude {targetImpliedRaw.Value:R} if the canonical generator scale is held fixed");
            diagnosis.Add($"equivalently, the raw matrix element would need scale {rawScale.Value:R}");

            if (generatorCanExplain)
            {
                diagnosis.Add("canonical SU(2) generator normalization can explain the miss within tolerance");
            }
            else
            {
                diagnosis.Add("canonical SU(2) trace-half generator normalization cannot explain the coherent W/Z miss by itself");
                closure.Add("replace the Phase65 scalar raw matrix-element input with a replayed analytic matrix-element evaluation or revise the scalar-sector relation before promoting new W/Z absolute masses");
            }
        }

        return new WeakCouplingAmplitudeNormalizationAuditResult
        {
            AlgorithmId = AlgorithmId,
            TerminalStatus = closure.Count == 0
                ? "weak-coupling-amplitude-normalization-audit-passed"
                : "weak-coupling-amplitude-normalization-audit-blocked",
            CurrentRawMatrixElementMagnitude = double.IsFinite(extraction.RawMatrixElementMagnitude.GetValueOrDefault(double.NaN))
                ? extraction.RawMatrixElementMagnitude
                : null,
            CurrentGeneratorNormalizationScale = double.IsFinite(extraction.GeneratorNormalizationScale.GetValueOrDefault(double.NaN))
                ? extraction.GeneratorNormalizationScale
                : null,
            CurrentWeakCoupling = extraction.Candidate?.CouplingValue is { } g && double.IsFinite(g) ? g : null,
            TargetImpliedWeakCoupling = missDiagnostic.RequiredWeakCoupling,
            TargetImpliedRawMatrixElementMagnitude = targetImpliedRaw,
            RawMatrixElementRequiredScale = rawScale,
            TargetImpliedGeneratorScaleIfRawHeldFixed = targetImpliedGeneratorScale,
            CanonicalTraceHalfGeneratorScale = System.Math.Sqrt(0.5),
            GeneratorNormalizationCanExplainMiss = generatorCanExplain,
            Diagnosis = diagnosis,
            ClosureRequirements = closure,
        };
    }
}
