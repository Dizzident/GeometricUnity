using System.Text.Json.Serialization;

namespace Gu.Phase5.Reporting;

public sealed class ElectroweakAbsoluteScaleCalibrationBuildResult
{
    [JsonPropertyName("status")]
    public required string Status { get; init; }

    [JsonPropertyName("scaleFactorGeVPerInternalMassUnit")]
    public double? ScaleFactorGeVPerInternalMassUnit { get; init; }

    [JsonPropertyName("scaleUncertaintyGeVPerInternalMassUnit")]
    public double? ScaleUncertaintyGeVPerInternalMassUnit { get; init; }

    [JsonPropertyName("calibrations")]
    public required PhysicalCalibrationTable Calibrations { get; init; }

    [JsonPropertyName("blockReasons")]
    public required IReadOnlyList<string> BlockReasons { get; init; }
}

public static class ElectroweakAbsoluteScaleCalibrationBuilder
{
    public static ElectroweakAbsoluteScaleCalibrationBuildResult BuildForWzAbsoluteMasses(
        ElectroweakBridgeRecord? bridge,
        double externalElectroweakScaleGeV,
        double externalElectroweakScaleUncertaintyGeV,
        PhysicalObservableMapping wMapping,
        PhysicalObservableMapping zMapping)
    {
        ArgumentNullException.ThrowIfNull(wMapping);
        ArgumentNullException.ThrowIfNull(zMapping);

        var blockReasons = new List<string>();
        blockReasons.AddRange(ElectroweakBridgeValidator.ValidateForAbsoluteWzProjection(bridge));

        if (!double.IsFinite(externalElectroweakScaleGeV) || externalElectroweakScaleGeV <= 0)
            blockReasons.Add("external electroweak scale must be finite and positive");

        if (!double.IsFinite(externalElectroweakScaleUncertaintyGeV) || externalElectroweakScaleUncertaintyGeV < 0)
            blockReasons.Add("external electroweak scale uncertainty must be finite and non-negative");

        if (!IsExpectedWzMapping(wMapping, "w-boson", "physical-w-boson-mass-gev"))
            blockReasons.Add("W mapping must target physical-w-boson-mass-gev");

        if (!IsExpectedWzMapping(zMapping, "z-boson", "physical-z-boson-mass-gev"))
            blockReasons.Add("Z mapping must target physical-z-boson-mass-gev");

        if (blockReasons.Count > 0 || bridge?.DimensionlessBridgeValue is not { } bridgeValue ||
            bridge.DimensionlessBridgeUncertainty is not { } bridgeUncertainty)
        {
            return new ElectroweakAbsoluteScaleCalibrationBuildResult
            {
                Status = "blocked",
                ScaleFactorGeVPerInternalMassUnit = null,
                ScaleUncertaintyGeVPerInternalMassUnit = null,
                Calibrations = EmptyCalibrationTable(),
                BlockReasons = blockReasons,
            };
        }

        var scale = externalElectroweakScaleGeV * bridgeValue;
        var scaleUncertainty = System.Math.Sqrt(
            System.Math.Pow(externalElectroweakScaleUncertaintyGeV * bridgeValue, 2) +
            System.Math.Pow(externalElectroweakScaleGeV * bridgeUncertainty, 2));

        return new ElectroweakAbsoluteScaleCalibrationBuildResult
        {
            Status = "validated",
            ScaleFactorGeVPerInternalMassUnit = scale,
            ScaleUncertaintyGeVPerInternalMassUnit = scaleUncertainty,
            Calibrations = new PhysicalCalibrationTable
            {
                TableId = "electroweak-absolute-wz-calibrations-v1",
                Calibrations =
                [
                    CreateCalibration(wMapping, bridge, scale, scaleUncertainty),
                    CreateCalibration(zMapping, bridge, scale, scaleUncertainty),
                ],
            },
            BlockReasons = [],
        };
    }

    private static bool IsExpectedWzMapping(PhysicalObservableMapping mapping, string particleId, string targetObservableId)
        => string.Equals(mapping.ParticleId, particleId, StringComparison.Ordinal) &&
           string.Equals(mapping.PhysicalObservableType, "mass", StringComparison.OrdinalIgnoreCase) &&
           string.Equals(mapping.TargetPhysicalObservableId, targetObservableId, StringComparison.Ordinal);

    private static PhysicalCalibrationRecord CreateCalibration(
        PhysicalObservableMapping mapping,
        ElectroweakBridgeRecord bridge,
        double scale,
        double scaleUncertainty) => new()
    {
        CalibrationId = $"calibration-{mapping.TargetPhysicalObservableId}-phase58-electroweak-bridge",
        MappingId = mapping.MappingId,
        SourceComputedObservableId = mapping.SourceComputedObservableId,
        SourceUnitFamily = "mass-energy",
        TargetUnitFamily = "mass-energy",
        TargetUnit = "GeV",
        ScaleFactor = scale,
        ScaleUncertainty = scaleUncertainty,
        Status = "validated",
        Method = $"phase58-external-electroweak-scale-times-validated-bridge:{bridge.BridgeObservableId}",
        Source = "phase54-fermi-derived-electroweak-vacuum-scale + validated ElectroweakBridgeRecord",
        Assumptions =
        [
            "external electroweak scale is disjoint from W/Z mass targets",
            "electroweak bridge validator passed before calibration construction",
            "same GeV-per-internal-mass-unit scale is applied to W and Z absolute mass mappings",
        ],
        ClosureRequirements = [],
    };

    private static PhysicalCalibrationTable EmptyCalibrationTable() => new()
    {
        TableId = "electroweak-absolute-wz-calibrations-v1",
        Calibrations = [],
    };
}
