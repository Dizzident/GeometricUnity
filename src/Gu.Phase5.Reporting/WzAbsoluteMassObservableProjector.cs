using System.Text.Json.Serialization;
using Gu.Phase5.QuantitativeValidation;

namespace Gu.Phase5.Reporting;

public sealed class WzAbsoluteMassObservableProjectionResult
{
    [JsonPropertyName("status")]
    public required string Status { get; init; }

    [JsonPropertyName("observables")]
    public required IReadOnlyList<QuantitativeObservableRecord> Observables { get; init; }

    [JsonPropertyName("blockReasons")]
    public required IReadOnlyList<string> BlockReasons { get; init; }
}

public static class WzAbsoluteMassObservableProjector
{
    public static WzAbsoluteMassObservableProjectionResult Project(
        IReadOnlyList<IdentifiedPhysicalModeRecord> physicalModes,
        ElectroweakAbsoluteScaleCalibrationBuildResult calibrationBuild)
    {
        ArgumentNullException.ThrowIfNull(physicalModes);
        ArgumentNullException.ThrowIfNull(calibrationBuild);

        var blockReasons = new List<string>();
        if (!string.Equals(calibrationBuild.Status, "validated", StringComparison.OrdinalIgnoreCase))
            blockReasons.Add("electroweak absolute scale calibration build is not validated");

        if (calibrationBuild.ScaleFactorGeVPerInternalMassUnit is not { } scale || !double.IsFinite(scale) || scale <= 0)
            blockReasons.Add("scaleFactorGeVPerInternalMassUnit must be finite and positive");

        if (calibrationBuild.ScaleUncertaintyGeVPerInternalMassUnit is not { } scaleUncertainty ||
            !double.IsFinite(scaleUncertainty) || scaleUncertainty < 0)
        {
            blockReasons.Add("scaleUncertaintyGeVPerInternalMassUnit must be finite and non-negative");
        }

        var wMode = FindMode(physicalModes, "w-boson");
        var zMode = FindMode(physicalModes, "z-boson");
        if (wMode is null)
            blockReasons.Add("validated W physical mode is missing");
        if (zMode is null)
            blockReasons.Add("validated Z physical mode is missing");

        if (blockReasons.Count > 0 || wMode is null || zMode is null ||
            calibrationBuild.ScaleFactorGeVPerInternalMassUnit is null ||
            calibrationBuild.ScaleUncertaintyGeVPerInternalMassUnit is null)
        {
            return new WzAbsoluteMassObservableProjectionResult
            {
                Status = "blocked",
                Observables = [],
                BlockReasons = blockReasons,
            };
        }

        return new WzAbsoluteMassObservableProjectionResult
        {
            Status = "projected",
            Observables =
            [
                CreateObservable(
                    "physical-w-boson-mass-gev",
                    wMode,
                    calibrationBuild.ScaleFactorGeVPerInternalMassUnit.Value,
                    calibrationBuild.ScaleUncertaintyGeVPerInternalMassUnit.Value),
                CreateObservable(
                    "physical-z-boson-mass-gev",
                    zMode,
                    calibrationBuild.ScaleFactorGeVPerInternalMassUnit.Value,
                    calibrationBuild.ScaleUncertaintyGeVPerInternalMassUnit.Value),
            ],
            BlockReasons = [],
        };
    }

    private static IdentifiedPhysicalModeRecord? FindMode(
        IReadOnlyList<IdentifiedPhysicalModeRecord> modes,
        string particleId)
        => modes.FirstOrDefault(mode =>
            string.Equals(mode.ParticleId, particleId, StringComparison.Ordinal) &&
            string.Equals(mode.Status, "validated", StringComparison.OrdinalIgnoreCase) &&
            string.Equals(mode.UnitFamily, "mass-energy", StringComparison.Ordinal) &&
            string.Equals(mode.Unit, "internal-mass-unit", StringComparison.Ordinal));

    private static QuantitativeObservableRecord CreateObservable(
        string observableId,
        IdentifiedPhysicalModeRecord mode,
        double scale,
        double scaleUncertainty)
    {
        var value = mode.Value * scale;
        var uncertainty = System.Math.Sqrt(
            System.Math.Pow(mode.Uncertainty * scale, 2) +
            System.Math.Pow(mode.Value * scaleUncertainty, 2));

        return new QuantitativeObservableRecord
        {
            ObservableId = observableId,
            Value = value,
            Uncertainty = new QuantitativeUncertainty
            {
                ExtractionError = uncertainty,
                TotalUncertainty = uncertainty,
            },
            BranchId = mode.BranchId,
            EnvironmentId = mode.EnvironmentId,
            RefinementLevel = mode.RefinementLevel,
            ExtractionMethod = $"phase59-absolute-wz-mass-projection:{mode.ObservableId}",
            DistributionModel = "gaussian",
            Provenance = mode.Provenance,
        };
    }
}
