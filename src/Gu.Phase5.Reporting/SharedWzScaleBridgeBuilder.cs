using System.Text.Json.Serialization;

namespace Gu.Phase5.Reporting;

public sealed class SharedWzScaleBridgeBuildResult
{
    [JsonPropertyName("algorithmId")]
    public required string AlgorithmId { get; init; }

    [JsonPropertyName("terminalStatus")]
    public required string TerminalStatus { get; init; }

    [JsonPropertyName("wBridgeValue")]
    public double? WBridgeValue { get; init; }

    [JsonPropertyName("zBridgeValue")]
    public double? ZBridgeValue { get; init; }

    [JsonPropertyName("relativeScaleSpread")]
    public double? RelativeScaleSpread { get; init; }

    [JsonPropertyName("relativeTolerance")]
    public required double RelativeTolerance { get; init; }

    [JsonPropertyName("bridge")]
    public ElectroweakBridgeRecord? Bridge { get; init; }

    [JsonPropertyName("closureRequirements")]
    public required IReadOnlyList<string> ClosureRequirements { get; init; }
}

public static class SharedWzScaleBridgeBuilder
{
    public const string AlgorithmId = "phase71-shared-wz-scale-bridge-builder-v1";
    public const string BridgeObservableId = "phase71-validated-electroweak-mass-generation-bridge";

    public static SharedWzScaleBridgeBuildResult Build(
        ElectroweakMassGenerationRelationResult relation,
        ScalarSectorBridgeEvidenceResult scalarEvidence,
        double relativeTolerance)
    {
        ArgumentNullException.ThrowIfNull(relation);
        ArgumentNullException.ThrowIfNull(scalarEvidence);

        var closure = new List<string>();
        if (!string.Equals(relation.TerminalStatus, "electroweak-mass-generation-relation-derived", StringComparison.Ordinal))
            closure.Add("electroweak mass-generation relation has not been derived");
        if (!string.Equals(scalarEvidence.TerminalStatus, "scalar-sector-bridge-evidence-derived", StringComparison.Ordinal))
            closure.Add("scalar-sector bridge evidence has not been derived");
        if (!double.IsFinite(relativeTolerance) || relativeTolerance <= 0.0)
            closure.Add("relative tolerance must be finite and positive");

        var wBridge = relation.DimensionlessBridgeValue;
        double? zBridge = null;
        double? spread = null;
        if (closure.Count == 0)
        {
            if (relation.ZElectroweakCoefficient is not { } zCoefficient ||
                relation.ZInternalMass is not { } zInternal ||
                !double.IsFinite(zCoefficient) ||
                !double.IsFinite(zInternal) ||
                zInternal <= 0.0)
            {
                closure.Add("Z bridge coefficient and internal mass must be finite and positive");
            }
            else
            {
                zBridge = zCoefficient / zInternal;
            }

            if (wBridge is not { } w || !double.IsFinite(w) || w <= 0.0)
                closure.Add("W bridge value must be finite and positive");
        }

        ElectroweakBridgeRecord? bridge = null;
        if (closure.Count == 0 && wBridge is { } wValue && zBridge is { } zValue)
        {
            var mean = (wValue + zValue) / 2.0;
            spread = System.Math.Abs(wValue - zValue) / mean;
            if (spread.Value > relativeTolerance)
            {
                closure.Add($"shared W/Z bridge scale spread {spread.Value:R} exceeds tolerance {relativeTolerance:R}");
            }
            else
            {
                bridge = new ElectroweakBridgeRecord
                {
                    BridgeObservableId = BridgeObservableId,
                    SourceModeIds = relation.SourceModeIds,
                    DimensionlessBridgeValue = mean,
                    DimensionlessBridgeUncertainty = relation.DimensionlessBridgeUncertainty,
                    InputKind = "validated-internal-mass-generation-relation",
                    WeakCouplingNormalizationConvention = relation.WeakCouplingNormalizationConvention,
                    MassGenerationRelation = relation.MassGenerationRelationId,
                    ExcludedTargetObservableIds = relation.ExcludedTargetObservableIds,
                    Status = "validated",
                    Assumptions =
                    [
                        "external electroweak scale is treated as scalar-sector order parameter v",
                        "W relation uses m_W = g v / 2",
                        "Z relation uses the internal W/Z ratio to define the neutral coefficient without W/Z mass targets",
                    ],
                    ClosureRequirements = [],
                };
            }
        }

        return new SharedWzScaleBridgeBuildResult
        {
            AlgorithmId = AlgorithmId,
            TerminalStatus = bridge is not null
                ? "shared-wz-scale-bridge-validated"
                : "shared-wz-scale-bridge-blocked",
            WBridgeValue = wBridge,
            ZBridgeValue = zBridge,
            RelativeScaleSpread = spread,
            RelativeTolerance = relativeTolerance,
            Bridge = bridge,
            ClosureRequirements = closure,
        };
    }
}
