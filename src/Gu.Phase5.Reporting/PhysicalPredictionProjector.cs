using System.Text.Json.Serialization;
using Gu.Phase5.QuantitativeValidation;

namespace Gu.Phase5.Reporting;

/// <summary>
/// Candidate physical prediction produced from a validated mapping and calibration.
/// Blocked records are diagnostic only and must not be reported as predictions.
/// </summary>
public sealed class PhysicalPredictionRecord
{
    [JsonPropertyName("predictionId")]
    public required string PredictionId { get; init; }

    [JsonPropertyName("mappingId")]
    public required string MappingId { get; init; }

    [JsonPropertyName("calibrationId")]
    public string? CalibrationId { get; init; }

    [JsonPropertyName("sourceComputedObservableId")]
    public required string SourceComputedObservableId { get; init; }

    [JsonPropertyName("targetPhysicalObservableId")]
    public string? TargetPhysicalObservableId { get; init; }

    [JsonPropertyName("particleId")]
    public required string ParticleId { get; init; }

    [JsonPropertyName("physicalObservableType")]
    public required string PhysicalObservableType { get; init; }

    [JsonPropertyName("value")]
    public double? Value { get; init; }

    [JsonPropertyName("uncertainty")]
    public double? Uncertainty { get; init; }

    [JsonPropertyName("unit")]
    public string? Unit { get; init; }

    [JsonPropertyName("unitFamily")]
    public required string UnitFamily { get; init; }

    [JsonPropertyName("status")]
    public required string Status { get; init; }

    [JsonPropertyName("blockReasons")]
    public required IReadOnlyList<string> BlockReasons { get; init; }
}

public static class PhysicalPredictionProjector
{
    public static IReadOnlyList<PhysicalPredictionRecord> Project(
        IReadOnlyList<QuantitativeObservableRecord> observables,
        IReadOnlyList<PhysicalObservableMapping>? mappings,
        ObservableClassificationTable? classifications,
        PhysicalCalibrationTable? calibrations,
        IReadOnlyList<IdentifiedPhysicalModeRecord>? physicalModeRecords = null,
        IReadOnlyList<ModeIdentificationEvidenceRecord>? modeIdentificationEvidence = null,
        ElectroweakBridgeTable? electroweakBridges = null)
    {
        ArgumentNullException.ThrowIfNull(observables);

        if (mappings is null || mappings.Count == 0)
            return [];

        var records = new List<PhysicalPredictionRecord>();
        foreach (var mapping in mappings)
        {
            var blockReasons = new List<string>();
            if (!string.Equals(mapping.Status, "validated", StringComparison.OrdinalIgnoreCase))
                blockReasons.Add($"mapping status is '{mapping.Status}', not 'validated'");
            if (string.IsNullOrWhiteSpace(mapping.TargetPhysicalObservableId))
                blockReasons.Add("mapping does not declare targetPhysicalObservableId");

            var observable = FindObservable(observables, mapping);
            if (observable is null)
                blockReasons.Add("no computed observable matches the mapping selectors");
            else
            {
                if (observable.Uncertainty.TotalUncertainty < 0)
                    blockReasons.Add("source observable total uncertainty is unestimated");

                if (IsWzMassRatioMapping(mapping))
                    blockReasons.AddRange(ValidateWzModeEvidence(mapping, observable, physicalModeRecords, modeIdentificationEvidence));
            }

            var classification = classifications?.Classifications.FirstOrDefault(c =>
                string.Equals(c.ObservableId, mapping.SourceComputedObservableId, StringComparison.Ordinal));
            if (classification is null)
            {
                blockReasons.Add("source observable has no classification");
            }
            else if (!classification.PhysicalClaimAllowed ||
                     !string.Equals(classification.Classification, "physical-observable", StringComparison.OrdinalIgnoreCase))
            {
                blockReasons.Add("source observable is not classified for physical claims");
            }

            var calibration = calibrations?.Calibrations.FirstOrDefault(c =>
                string.Equals(c.MappingId, mapping.MappingId, StringComparison.Ordinal) &&
                string.Equals(c.SourceComputedObservableId, mapping.SourceComputedObservableId, StringComparison.Ordinal) &&
                string.Equals(c.Status, "validated", StringComparison.OrdinalIgnoreCase));
            if (calibration is null)
                blockReasons.Add("no validated calibration matches the mapping");

            if (IsAbsoluteWzMassMapping(mapping))
                blockReasons.AddRange(ValidateAbsoluteWzBridge(electroweakBridges));

            records.Add(blockReasons.Count == 0 && observable is not null && calibration is not null
                ? CreatePrediction(mapping, observable, calibration)
                : CreateBlockedRecord(mapping, calibration, blockReasons));
        }

        return records;
    }

    internal static IReadOnlyList<string> ValidateWzModeEvidence(
        PhysicalObservableMapping mapping,
        QuantitativeObservableRecord observable,
        IReadOnlyList<IdentifiedPhysicalModeRecord>? physicalModeRecords,
        IReadOnlyList<ModeIdentificationEvidenceRecord>? modeIdentificationEvidence)
    {
        var errors = new List<string>();
        if (!observable.ExtractionMethod.StartsWith("positive-mode-ratio:", StringComparison.Ordinal))
        {
            errors.Add("W/Z mass-ratio mapping source is not a positive mode-ratio observable");
            return errors;
        }

        var sourceIds = ParsePositiveModeRatioSourceIds(observable.ExtractionMethod);
        if (sourceIds is null)
        {
            errors.Add("W/Z mass-ratio source observable does not declare both ratio mode inputs");
            return errors;
        }

        if (physicalModeRecords is null || physicalModeRecords.Count == 0)
        {
            errors.Add("W/Z mass-ratio mapping requires identified physical mode records");
            return errors;
        }

        var modes = sourceIds
            .Select(id => physicalModeRecords.FirstOrDefault(m =>
                string.Equals(m.ObservableId, id, StringComparison.Ordinal) ||
                string.Equals(m.ModeId, id, StringComparison.Ordinal)))
            .ToList();

        if (modes.Any(m => m is null))
        {
            errors.Add("W/Z mass-ratio mapping source modes are not present in identified physical mode records");
            return errors;
        }

        var nonNullModes = modes.OfType<IdentifiedPhysicalModeRecord>().ToList();
        if (!nonNullModes.Any(m => string.Equals(m.ParticleId, "w-boson", StringComparison.Ordinal)) ||
            !nonNullModes.Any(m => string.Equals(m.ParticleId, "z-boson", StringComparison.Ordinal)))
        {
            errors.Add("W/Z mass-ratio mapping requires one validated W mode and one validated Z mode");
        }

        foreach (var mode in nonNullModes)
        {
            foreach (var error in PhysicalModeEvidenceValidator.ValidateForPrediction(mode, modeIdentificationEvidence))
                errors.Add($"W/Z mass-ratio source {error}");
        }

        return errors;
    }

    internal static bool IsWzMassRatioMapping(PhysicalObservableMapping mapping)
        => string.Equals(mapping.TargetPhysicalObservableId, "physical-w-z-mass-ratio", StringComparison.Ordinal) ||
           (mapping.SourceComputedObservableId.Contains("w-z", StringComparison.OrdinalIgnoreCase) &&
            mapping.PhysicalObservableType.Contains("mass-ratio", StringComparison.OrdinalIgnoreCase));

    internal static bool IsAbsoluteWzMassMapping(PhysicalObservableMapping mapping)
        => string.Equals(mapping.PhysicalObservableType, "mass", StringComparison.OrdinalIgnoreCase) &&
           (string.Equals(mapping.TargetPhysicalObservableId, "physical-w-boson-mass-gev", StringComparison.Ordinal) ||
            string.Equals(mapping.TargetPhysicalObservableId, "physical-z-boson-mass-gev", StringComparison.Ordinal) ||
            string.Equals(mapping.ParticleId, "w-boson", StringComparison.Ordinal) ||
            string.Equals(mapping.ParticleId, "z-boson", StringComparison.Ordinal));

    internal static IReadOnlyList<string> ValidateAbsoluteWzBridge(ElectroweakBridgeTable? bridges)
    {
        if (bridges is null || bridges.Bridges.Count == 0)
            return ["absolute W/Z mass mapping requires a validated electroweak bridge"];

        var validatedBridge = bridges.Bridges.FirstOrDefault(bridge =>
            ElectroweakBridgeValidator.ValidateForAbsoluteWzProjection(bridge).Count == 0);
        if (validatedBridge is not null)
            return [];

        var errors = ElectroweakBridgeValidator.ValidateForAbsoluteWzProjection(bridges.Bridges[0]);
        return errors.Count == 0
            ? []
            : errors.Select(error => $"absolute W/Z electroweak bridge {error}").ToArray();
    }

    private static string[]? ParsePositiveModeRatioSourceIds(string extractionMethod)
    {
        const string Prefix = "positive-mode-ratio:";
        if (!extractionMethod.StartsWith(Prefix, StringComparison.Ordinal))
            return null;

        var body = extractionMethod[Prefix.Length..];
        var suffixStart = body.IndexOf(':', StringComparison.Ordinal);
        if (suffixStart >= 0)
            body = body[..suffixStart];

        var parts = body.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return parts.Length == 2 ? parts : null;
    }

    private static QuantitativeObservableRecord? FindObservable(
        IReadOnlyList<QuantitativeObservableRecord> observables,
        PhysicalObservableMapping mapping)
    {
        return observables.FirstOrDefault(o =>
            string.Equals(o.ObservableId, mapping.SourceComputedObservableId, StringComparison.Ordinal) &&
            (string.IsNullOrWhiteSpace(mapping.RequiredEnvironmentId) ||
             string.Equals(o.EnvironmentId, mapping.RequiredEnvironmentId, StringComparison.Ordinal)) &&
            (string.IsNullOrWhiteSpace(mapping.RequiredBranchId) ||
             string.Equals(o.BranchId, mapping.RequiredBranchId, StringComparison.Ordinal)));
    }

    private static PhysicalPredictionRecord CreatePrediction(
        PhysicalObservableMapping mapping,
        QuantitativeObservableRecord observable,
        PhysicalCalibrationRecord calibration)
    {
        var value = observable.Value * calibration.ScaleFactor;
        var sourceUncertainty = observable.Uncertainty.TotalUncertainty >= 0
            ? observable.Uncertainty.TotalUncertainty
            : 0.0;
        var uncertainty = System.Math.Sqrt(
            System.Math.Pow(sourceUncertainty * calibration.ScaleFactor, 2) +
            System.Math.Pow(observable.Value * calibration.ScaleUncertainty, 2));

        return new PhysicalPredictionRecord
        {
            PredictionId = $"prediction-{mapping.TargetPhysicalObservableId}",
            MappingId = mapping.MappingId,
            CalibrationId = calibration.CalibrationId,
            SourceComputedObservableId = mapping.SourceComputedObservableId,
            TargetPhysicalObservableId = mapping.TargetPhysicalObservableId,
            ParticleId = mapping.ParticleId,
            PhysicalObservableType = mapping.PhysicalObservableType,
            Value = value,
            Uncertainty = uncertainty,
            Unit = calibration.TargetUnit,
            UnitFamily = calibration.TargetUnitFamily,
            Status = "predicted",
            BlockReasons = [],
        };
    }

    private static PhysicalPredictionRecord CreateBlockedRecord(
        PhysicalObservableMapping mapping,
        PhysicalCalibrationRecord? calibration,
        IReadOnlyList<string> blockReasons)
    {
        return new PhysicalPredictionRecord
        {
            PredictionId = $"blocked-{mapping.MappingId}",
            MappingId = mapping.MappingId,
            CalibrationId = calibration?.CalibrationId,
            SourceComputedObservableId = mapping.SourceComputedObservableId,
            TargetPhysicalObservableId = mapping.TargetPhysicalObservableId,
            ParticleId = mapping.ParticleId,
            PhysicalObservableType = mapping.PhysicalObservableType,
            Value = null,
            Uncertainty = null,
            Unit = calibration?.TargetUnit,
            UnitFamily = mapping.UnitFamily,
            Status = "blocked",
            BlockReasons = blockReasons,
        };
    }
}
