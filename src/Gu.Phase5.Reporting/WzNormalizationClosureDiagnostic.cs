using System.Text.Json.Serialization;
using Gu.Core;
using Gu.Core.Serialization;

namespace Gu.Phase5.Reporting;

public sealed class WzNormalizationClosureDiagnosticResult
{
    [JsonPropertyName("resultId")]
    public required string ResultId { get; init; }

    [JsonPropertyName("schemaVersion")]
    public required string SchemaVersion { get; init; }

    [JsonPropertyName("terminalStatus")]
    public required string TerminalStatus { get; init; }

    [JsonPropertyName("algorithmId")]
    public required string AlgorithmId { get; init; }

    [JsonPropertyName("targetObservableId")]
    public required string TargetObservableId { get; init; }

    [JsonPropertyName("selectedPairId")]
    public string? SelectedPairId { get; init; }

    [JsonPropertyName("computedRatio")]
    public double? ComputedRatio { get; init; }

    [JsonPropertyName("targetValue")]
    public required double TargetValue { get; init; }

    [JsonPropertyName("requiredScaleToTarget")]
    public double? RequiredScaleToTarget { get; init; }

    [JsonPropertyName("declaredScaleFactor")]
    public double? DeclaredScaleFactor { get; init; }

    [JsonPropertyName("declaredScaleUncertainty")]
    public double? DeclaredScaleUncertainty { get; init; }

    [JsonPropertyName("declaredScaleDelta")]
    public double? DeclaredScaleDelta { get; init; }

    [JsonPropertyName("declaredCalibration")]
    public WzNormalizationCalibrationAuditRecord? DeclaredCalibration { get; init; }

    [JsonPropertyName("selectorVariationExplainsMiss")]
    public required bool SelectorVariationExplainsMiss { get; init; }

    [JsonPropertyName("derivationBackedScaleAvailable")]
    public required bool DerivationBackedScaleAvailable { get; init; }

    [JsonPropertyName("normalizationChangeAllowed")]
    public required bool NormalizationChangeAllowed { get; init; }

    [JsonPropertyName("diagnosis")]
    public required IReadOnlyList<string> Diagnosis { get; init; }

    [JsonPropertyName("closureRequirements")]
    public required IReadOnlyList<string> ClosureRequirements { get; init; }

    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}

public sealed class WzNormalizationCalibrationAuditRecord
{
    [JsonPropertyName("calibrationId")]
    public required string CalibrationId { get; init; }

    [JsonPropertyName("mappingId")]
    public required string MappingId { get; init; }

    [JsonPropertyName("scaleFactor")]
    public required double ScaleFactor { get; init; }

    [JsonPropertyName("scaleUncertainty")]
    public required double ScaleUncertainty { get; init; }

    [JsonPropertyName("status")]
    public required string Status { get; init; }

    [JsonPropertyName("method")]
    public required string Method { get; init; }

    [JsonPropertyName("source")]
    public required string Source { get; init; }

    [JsonPropertyName("isIdentityNormalization")]
    public required bool IsIdentityNormalization { get; init; }

    [JsonPropertyName("hasOperatorDerivation")]
    public required bool HasOperatorDerivation { get; init; }

    [JsonPropertyName("hasClosureRequirements")]
    public required bool HasClosureRequirements { get; init; }
}

public static class WzNormalizationClosureDiagnostic
{
    public const string AlgorithmId = "p31-wz-normalization-closure-diagnostic:v1";

    public static WzNormalizationClosureDiagnosticResult Evaluate(
        string ratioFailureDiagnosticJson,
        string selectorVariationDiagnosticJson,
        string physicalCalibrationsJson,
        ProvenanceMeta provenance)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ratioFailureDiagnosticJson);
        ArgumentException.ThrowIfNullOrWhiteSpace(selectorVariationDiagnosticJson);
        ArgumentException.ThrowIfNullOrWhiteSpace(physicalCalibrationsJson);

        var ratioDiagnostic = GuJsonDefaults.Deserialize<WzRatioFailureDiagnosticResult>(ratioFailureDiagnosticJson)
            ?? throw new InvalidDataException("Failed to deserialize W/Z ratio failure diagnostic.");
        var selectorDiagnostic = GuJsonDefaults.Deserialize<WzSelectorVariationDiagnosticResult>(selectorVariationDiagnosticJson)
            ?? throw new InvalidDataException("Failed to deserialize W/Z selector variation diagnostic.");
        var calibrationTable = GuJsonDefaults.Deserialize<PhysicalCalibrationTable>(physicalCalibrationsJson)
            ?? throw new InvalidDataException("Failed to deserialize physical calibration table.");

        var closure = new List<string>();
        if (!string.Equals(ratioDiagnostic.TerminalStatus, "wz-ratio-diagnostic-complete", StringComparison.Ordinal))
            closure.Add("W/Z ratio failure diagnostic must be complete");
        if (!string.Equals(selectorDiagnostic.TerminalStatus, "selector-variation-diagnostic-complete", StringComparison.Ordinal))
            closure.Add("W/Z selector variation diagnostic must be complete");

        var selectedPair = ratioDiagnostic.SelectedPair;
        if (selectedPair is null)
            closure.Add("W/Z ratio failure diagnostic must provide a selected pair");

        var calibration = calibrationTable.Calibrations.FirstOrDefault(c =>
            string.Equals(c.SourceComputedObservableId, ratioDiagnostic.TargetObservableId, StringComparison.Ordinal) ||
            string.Equals(c.SourceComputedObservableId, "physical-w-z-mass-ratio", StringComparison.Ordinal));
        if (calibration is null)
            closure.Add("provide a physical calibration record for physical-w-z-mass-ratio");

        var audit = calibration is null ? null : AuditCalibration(calibration);
        var selectorExplainsMiss = selectorDiagnostic.TargetInsideObservedEnvelope || selectorDiagnostic.PassingPointCount > 0;
        var derivationBackedScale = audit is not null &&
            string.Equals(audit.Status, "validated", StringComparison.Ordinal) &&
            audit.HasOperatorDerivation &&
            !audit.HasClosureRequirements;
        var requiredScale = selectedPair?.RequiredScaleToTarget;
        var declaredScale = calibration?.ScaleFactor;
        var declaredDelta = requiredScale.HasValue && declaredScale.HasValue
            ? declaredScale.Value - requiredScale.Value
            : (double?)null;

        var normalizationChangeAllowed = selectorExplainsMiss ||
            (derivationBackedScale && declaredScale.HasValue && requiredScale.HasValue &&
             NearlyEqual(declaredScale.Value, requiredScale.Value, calibration?.ScaleUncertainty ?? 0));

        if (!selectorExplainsMiss)
            closure.Add("selector variation does not cover the physical W/Z target");
        if (!derivationBackedScale)
            closure.Add("provide a derivation-backed normalization/operator scale independent of the physical W/Z target");
        if (audit is { IsIdentityNormalization: true } && requiredScale is not null && !NearlyEqual(audit.ScaleFactor, requiredScale.Value, audit.ScaleUncertainty))
            closure.Add("current identity normalization scale is not the scale required by the physical target");

        return new WzNormalizationClosureDiagnosticResult
        {
            ResultId = "phase31-wz-normalization-closure-diagnostic-v1",
            SchemaVersion = "1.0.0",
            TerminalStatus = normalizationChangeAllowed && closure.Count == 0
                ? "wz-normalization-closure-ready"
                : "wz-normalization-closure-blocked",
            AlgorithmId = AlgorithmId,
            TargetObservableId = ratioDiagnostic.TargetObservableId,
            SelectedPairId = selectedPair?.PairId,
            ComputedRatio = selectedPair?.Ratio,
            TargetValue = ratioDiagnostic.TargetValue,
            RequiredScaleToTarget = requiredScale,
            DeclaredScaleFactor = declaredScale,
            DeclaredScaleUncertainty = calibration?.ScaleUncertainty,
            DeclaredScaleDelta = declaredDelta,
            DeclaredCalibration = audit,
            SelectorVariationExplainsMiss = selectorExplainsMiss,
            DerivationBackedScaleAvailable = derivationBackedScale,
            NormalizationChangeAllowed = normalizationChangeAllowed,
            Diagnosis = BuildDiagnosis(selectedPair, selectorDiagnostic, audit, requiredScale, declaredDelta, derivationBackedScale),
            ClosureRequirements = closure.Distinct(StringComparer.Ordinal).ToList(),
            Provenance = provenance,
        };
    }

    private static WzNormalizationCalibrationAuditRecord AuditCalibration(PhysicalCalibrationRecord calibration)
    {
        var text = $"{calibration.CalibrationId} {calibration.Method} {calibration.Source}";
        var hasOperatorDerivation =
            text.Contains("operator", StringComparison.OrdinalIgnoreCase) ||
            text.Contains("derived", StringComparison.OrdinalIgnoreCase) ||
            text.Contains("derivation", StringComparison.OrdinalIgnoreCase);
        var isIdentity =
            calibration.ScaleFactor == 1.0 ||
            text.Contains("identity-normalization", StringComparison.OrdinalIgnoreCase);

        return new WzNormalizationCalibrationAuditRecord
        {
            CalibrationId = calibration.CalibrationId,
            MappingId = calibration.MappingId,
            ScaleFactor = calibration.ScaleFactor,
            ScaleUncertainty = calibration.ScaleUncertainty,
            Status = calibration.Status,
            Method = calibration.Method,
            Source = calibration.Source,
            IsIdentityNormalization = isIdentity,
            HasOperatorDerivation = hasOperatorDerivation,
            HasClosureRequirements = calibration.ClosureRequirements.Count > 0,
        };
    }

    private static IReadOnlyList<string> BuildDiagnosis(
        WzRatioPairDiagnosticRecord? selectedPair,
        WzSelectorVariationDiagnosticResult selectorDiagnostic,
        WzNormalizationCalibrationAuditRecord? calibration,
        double? requiredScale,
        double? declaredDelta,
        bool derivationBackedScale)
    {
        var diagnosis = new List<string>();
        if (selectedPair is not null)
        {
            diagnosis.Add($"selected W/Z pair '{selectedPair.PairId}' has ratio {selectedPair.Ratio:R} and requires scale {selectedPair.RequiredScaleToTarget:R} to hit the target");
            diagnosis.Add($"selected pair remains outside the sigma-5 gate with pull {selectedPair.Pull:R}");
        }

        if (!selectorDiagnostic.TargetInsideObservedEnvelope && selectorDiagnostic.PassingPointCount == 0)
            diagnosis.Add("Phase30 selector variation did not move the ratio envelope over the physical target");

        if (calibration is null)
        {
            diagnosis.Add("no declared physical calibration was available for the W/Z ratio");
        }
        else
        {
            diagnosis.Add($"declared calibration '{calibration.CalibrationId}' uses scale {calibration.ScaleFactor:R} with method '{calibration.Method}'");
            if (requiredScale is not null && declaredDelta is not null)
                diagnosis.Add($"declared scale differs from the required scale by {declaredDelta.Value:R}");
            if (!derivationBackedScale)
                diagnosis.Add("declared calibration is not an operator-derived normalization closure for the required scale");
        }

        return diagnosis;
    }

    private static bool NearlyEqual(double a, double b, double tolerance)
    {
        var effectiveTolerance = System.Math.Max(tolerance, 1e-12);
        return System.Math.Abs(a - b) <= effectiveTolerance;
    }
}
