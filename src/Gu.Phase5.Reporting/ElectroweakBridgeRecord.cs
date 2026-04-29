using System.Text.Json.Serialization;

namespace Gu.Phase5.Reporting;

/// <summary>
/// Candidate internal bridge from an electroweak scale input to the repository's
/// internal W/Z mass unit. Records must pass validation before they can support
/// absolute W/Z mass projection.
/// </summary>
public sealed class ElectroweakBridgeRecord
{
    [JsonPropertyName("bridgeObservableId")]
    public required string BridgeObservableId { get; init; }

    [JsonPropertyName("sourceModeIds")]
    public required IReadOnlyList<string> SourceModeIds { get; init; }

    [JsonPropertyName("dimensionlessBridgeValue")]
    public double? DimensionlessBridgeValue { get; init; }

    [JsonPropertyName("dimensionlessBridgeUncertainty")]
    public double? DimensionlessBridgeUncertainty { get; init; }

    [JsonPropertyName("inputKind")]
    public required string InputKind { get; init; }

    [JsonPropertyName("weakCouplingNormalizationConvention")]
    public string? WeakCouplingNormalizationConvention { get; init; }

    [JsonPropertyName("massGenerationRelation")]
    public string? MassGenerationRelation { get; init; }

    [JsonPropertyName("excludedTargetObservableIds")]
    public required IReadOnlyList<string> ExcludedTargetObservableIds { get; init; }

    [JsonPropertyName("status")]
    public required string Status { get; init; }

    [JsonPropertyName("assumptions")]
    public required IReadOnlyList<string> Assumptions { get; init; }

    [JsonPropertyName("closureRequirements")]
    public required IReadOnlyList<string> ClosureRequirements { get; init; }
}

public sealed class ElectroweakBridgeTable
{
    [JsonPropertyName("tableId")]
    public required string TableId { get; init; }

    [JsonPropertyName("bridges")]
    public required IReadOnlyList<ElectroweakBridgeRecord> Bridges { get; init; }
}

public static class ElectroweakBridgeValidator
{
    private static readonly HashSet<string> AcceptedInputKinds = new(StringComparer.Ordinal)
    {
        "normalized-internal-weak-coupling",
        "validated-internal-mass-generation-relation",
    };

    private static readonly HashSet<string> RejectedInputKinds = new(StringComparer.Ordinal)
    {
        "finite-difference-current-profile-hash",
        "coupling-profile-mean-magnitude",
        "coupling-profile-diagonal-mean-magnitude",
        "coupling-profile-off-diagonal-mean-magnitude",
        "target-fitted-W-or-Z-scale",
    };

    public static IReadOnlyList<string> ValidateForAbsoluteWzProjection(ElectroweakBridgeRecord? bridge)
    {
        if (bridge is null)
            return ["electroweak bridge record is missing"];

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(bridge.BridgeObservableId))
            errors.Add("bridgeObservableId is required");

        if (!string.Equals(bridge.Status, "validated", StringComparison.OrdinalIgnoreCase))
            errors.Add($"bridge status is '{bridge.Status}', not 'validated'");

        if (RejectedInputKinds.Contains(bridge.InputKind))
            errors.Add($"bridge input kind '{bridge.InputKind}' is rejected for absolute W/Z projection");
        else if (!AcceptedInputKinds.Contains(bridge.InputKind))
            errors.Add($"bridge input kind '{bridge.InputKind}' is not accepted for absolute W/Z projection");

        if (bridge.SourceModeIds.Count < 2)
            errors.Add("bridge must declare at least two W/Z source modes");

        if (bridge.DimensionlessBridgeValue is not { } value || !double.IsFinite(value) || value <= 0)
            errors.Add("dimensionlessBridgeValue must be finite and positive");

        if (bridge.DimensionlessBridgeUncertainty is not { } uncertainty || !double.IsFinite(uncertainty) || uncertainty < 0)
            errors.Add("dimensionlessBridgeUncertainty must be finite and non-negative");

        if (string.IsNullOrWhiteSpace(bridge.WeakCouplingNormalizationConvention))
            errors.Add("weakCouplingNormalizationConvention is required");

        if (string.IsNullOrWhiteSpace(bridge.MassGenerationRelation))
            errors.Add("massGenerationRelation is required");

        if (!bridge.ExcludedTargetObservableIds.Contains("physical-w-boson-mass-gev", StringComparer.Ordinal))
            errors.Add("bridge must exclude physical-w-boson-mass-gev");

        if (!bridge.ExcludedTargetObservableIds.Contains("physical-z-boson-mass-gev", StringComparer.Ordinal))
            errors.Add("bridge must exclude physical-z-boson-mass-gev");

        if (bridge.ClosureRequirements.Count > 0)
            errors.Add("validated bridge must not have open closure requirements");

        return errors;
    }
}
