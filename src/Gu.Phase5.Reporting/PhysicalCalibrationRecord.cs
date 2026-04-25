using System.Text.Json.Serialization;

namespace Gu.Phase5.Reporting;

/// <summary>
/// Scale-setting or normalization record required before a computed quantity can
/// be compared to a physical target.
/// </summary>
public sealed class PhysicalCalibrationRecord
{
    [JsonPropertyName("calibrationId")]
    public required string CalibrationId { get; init; }

    [JsonPropertyName("mappingId")]
    public required string MappingId { get; init; }

    [JsonPropertyName("sourceComputedObservableId")]
    public required string SourceComputedObservableId { get; init; }

    [JsonPropertyName("sourceUnitFamily")]
    public required string SourceUnitFamily { get; init; }

    [JsonPropertyName("targetUnitFamily")]
    public required string TargetUnitFamily { get; init; }

    [JsonPropertyName("targetUnit")]
    public required string TargetUnit { get; init; }

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

    [JsonPropertyName("assumptions")]
    public required IReadOnlyList<string> Assumptions { get; init; }

    [JsonPropertyName("closureRequirements")]
    public required IReadOnlyList<string> ClosureRequirements { get; init; }
}

public sealed class PhysicalCalibrationTable
{
    [JsonPropertyName("tableId")]
    public required string TableId { get; init; }

    [JsonPropertyName("calibrations")]
    public required IReadOnlyList<PhysicalCalibrationRecord> Calibrations { get; init; }
}
