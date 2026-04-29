using System.Text.Json.Serialization;
using Gu.Phase5.QuantitativeValidation;

namespace Gu.Phase5.Reporting;

public sealed class ElectroweakMassGenerationRelationResult
{
    [JsonPropertyName("algorithmId")]
    public required string AlgorithmId { get; init; }

    [JsonPropertyName("terminalStatus")]
    public required string TerminalStatus { get; init; }

    [JsonPropertyName("massGenerationRelationId")]
    public required string MassGenerationRelationId { get; init; }

    [JsonPropertyName("sourceModeIds")]
    public required IReadOnlyList<string> SourceModeIds { get; init; }

    [JsonPropertyName("weakCouplingNormalizationConvention")]
    public string? WeakCouplingNormalizationConvention { get; init; }

    [JsonPropertyName("wInternalMass")]
    public double? WInternalMass { get; init; }

    [JsonPropertyName("zInternalMass")]
    public double? ZInternalMass { get; init; }

    [JsonPropertyName("internalWzRatio")]
    public double? InternalWzRatio { get; init; }

    [JsonPropertyName("wElectroweakCoefficient")]
    public double? WElectroweakCoefficient { get; init; }

    [JsonPropertyName("zElectroweakCoefficient")]
    public double? ZElectroweakCoefficient { get; init; }

    [JsonPropertyName("dimensionlessBridgeValue")]
    public double? DimensionlessBridgeValue { get; init; }

    [JsonPropertyName("dimensionlessBridgeUncertainty")]
    public double? DimensionlessBridgeUncertainty { get; init; }

    [JsonPropertyName("excludedTargetObservableIds")]
    public required IReadOnlyList<string> ExcludedTargetObservableIds { get; init; }

    [JsonPropertyName("closureRequirements")]
    public required IReadOnlyList<string> ClosureRequirements { get; init; }
}

public static class ElectroweakMassGenerationRelationDeriver
{
    public const string AlgorithmId = "phase69-electroweak-mass-generation-relation-deriver-v1";
    public const string RelationId = "mass-generation:electroweak-vev-times-normalized-weak-coupling:v1";

    public static ElectroweakMassGenerationRelationResult Derive(
        NormalizedWeakCouplingCandidateRecord weakCoupling,
        IdentifiedPhysicalModeRecord wMode,
        IdentifiedPhysicalModeRecord zMode,
        IReadOnlyList<string> excludedTargetObservableIds)
    {
        ArgumentNullException.ThrowIfNull(weakCoupling);
        ArgumentNullException.ThrowIfNull(wMode);
        ArgumentNullException.ThrowIfNull(zMode);
        ArgumentNullException.ThrowIfNull(excludedTargetObservableIds);

        var closure = new List<string>();
        if (!string.Equals(weakCoupling.SourceKind, NormalizedWeakCouplingInputAuditor.AcceptedSourceKind, StringComparison.Ordinal))
            closure.Add("weak-coupling candidate source kind is not normalized-internal-weak-coupling");
        if (weakCoupling.CouplingValue is not { } g || !double.IsFinite(g) || g <= 0.0)
            closure.Add("weak-coupling value must be finite and positive");
        if (weakCoupling.CouplingUncertainty is not { } gUncertainty || !double.IsFinite(gUncertainty) || gUncertainty < 0.0)
            closure.Add("weak-coupling uncertainty must be finite and non-negative");
        if (weakCoupling.BranchStabilityScore is not { } stability || !double.IsFinite(stability) ||
            stability < NormalizedWeakCouplingInputAuditor.DefaultMinimumBranchStabilityScore)
        {
            closure.Add("weak-coupling branch-stability score is below the accepted threshold");
        }

        if (!IsValidatedMode(wMode, "w-boson"))
            closure.Add("validated W internal mass mode is missing");
        if (!IsValidatedMode(zMode, "z-boson"))
            closure.Add("validated Z internal mass mode is missing");
        if (!excludedTargetObservableIds.Contains("physical-w-boson-mass-gev", StringComparer.Ordinal))
            closure.Add("physical-w-boson-mass-gev must be excluded from the relation derivation");
        if (!excludedTargetObservableIds.Contains("physical-z-boson-mass-gev", StringComparer.Ordinal))
            closure.Add("physical-z-boson-mass-gev must be excluded from the relation derivation");

        var internalRatio = closure.Count == 0 ? wMode.Value / zMode.Value : (double?)null;
        var wCoefficient = closure.Count == 0 ? weakCoupling.CouplingValue!.Value / 2.0 : (double?)null;
        var zCoefficient = closure.Count == 0 ? wCoefficient!.Value / internalRatio!.Value : (double?)null;
        var bridge = closure.Count == 0 ? wCoefficient!.Value / wMode.Value : (double?)null;
        var bridgeUncertainty = closure.Count == 0
            ? PropagateBridgeUncertainty(weakCoupling.CouplingValue!.Value, weakCoupling.CouplingUncertainty!.Value, wMode.Value, wMode.Uncertainty)
            : (double?)null;

        return new ElectroweakMassGenerationRelationResult
        {
            AlgorithmId = AlgorithmId,
            TerminalStatus = closure.Count == 0
                ? "electroweak-mass-generation-relation-derived"
                : "electroweak-mass-generation-relation-blocked",
            MassGenerationRelationId = RelationId,
            SourceModeIds = [wMode.ModeId, zMode.ModeId],
            WeakCouplingNormalizationConvention = weakCoupling.NormalizationConvention,
            WInternalMass = closure.Count == 0 ? wMode.Value : null,
            ZInternalMass = closure.Count == 0 ? zMode.Value : null,
            InternalWzRatio = internalRatio,
            WElectroweakCoefficient = wCoefficient,
            ZElectroweakCoefficient = zCoefficient,
            DimensionlessBridgeValue = bridge,
            DimensionlessBridgeUncertainty = bridgeUncertainty,
            ExcludedTargetObservableIds = excludedTargetObservableIds,
            ClosureRequirements = closure,
        };
    }

    private static bool IsValidatedMode(IdentifiedPhysicalModeRecord mode, string particleId)
        => string.Equals(mode.ParticleId, particleId, StringComparison.Ordinal) &&
           string.Equals(mode.Status, "validated", StringComparison.OrdinalIgnoreCase) &&
           string.Equals(mode.UnitFamily, "mass-energy", StringComparison.Ordinal) &&
           string.Equals(mode.Unit, "internal-mass-unit", StringComparison.Ordinal) &&
           double.IsFinite(mode.Value) &&
           mode.Value > 0.0 &&
           double.IsFinite(mode.Uncertainty) &&
           mode.Uncertainty >= 0.0;

    private static double PropagateBridgeUncertainty(
        double coupling,
        double couplingUncertainty,
        double internalMass,
        double internalMassUncertainty)
    {
        var bridge = coupling / (2.0 * internalMass);
        return bridge * System.Math.Sqrt(
            System.Math.Pow(couplingUncertainty / coupling, 2) +
            System.Math.Pow(internalMassUncertainty / internalMass, 2));
    }
}
