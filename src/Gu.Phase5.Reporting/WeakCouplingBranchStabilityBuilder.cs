using System.Text.Json.Serialization;

namespace Gu.Phase5.Reporting;

public sealed class WeakCouplingBranchVariantValue
{
    [JsonPropertyName("variantId")]
    public required string VariantId { get; init; }

    [JsonPropertyName("couplingValue")]
    public required double CouplingValue { get; init; }
}

public sealed class WeakCouplingBranchStabilityResult
{
    [JsonPropertyName("algorithmId")]
    public required string AlgorithmId { get; init; }

    [JsonPropertyName("terminalStatus")]
    public required string TerminalStatus { get; init; }

    [JsonPropertyName("relativeTolerance")]
    public required double RelativeTolerance { get; init; }

    [JsonPropertyName("maxRelativeDeviation")]
    public double? MaxRelativeDeviation { get; init; }

    [JsonPropertyName("branchStabilityScore")]
    public double? BranchStabilityScore { get; init; }

    [JsonPropertyName("variantValues")]
    public required IReadOnlyList<WeakCouplingBranchVariantValue> VariantValues { get; init; }

    [JsonPropertyName("candidate")]
    public NormalizedWeakCouplingCandidateRecord? Candidate { get; init; }

    [JsonPropertyName("closureRequirements")]
    public required IReadOnlyList<string> ClosureRequirements { get; init; }
}

public static class WeakCouplingBranchStabilityBuilder
{
    public const string AlgorithmId = "phase67-weak-coupling-branch-stability-builder-v1";

    public static WeakCouplingBranchStabilityResult Build(
        NormalizedWeakCouplingCandidateRecord candidate,
        IReadOnlyList<WeakCouplingBranchVariantValue> variantValues,
        double relativeTolerance)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        ArgumentNullException.ThrowIfNull(variantValues);

        var closure = new List<string>();
        if (candidate.CouplingValue is not { } value || !double.IsFinite(value) || value <= 0.0)
            closure.Add("candidate coupling value must be finite and positive");
        if (candidate.CouplingUncertainty is not { } uncertainty || !double.IsFinite(uncertainty) || uncertainty < 0.0)
            closure.Add("candidate coupling uncertainty must be finite and non-negative");
        if (variantValues.Count < 3)
            closure.Add("at least three branch-variant coupling values are required");
        if (!double.IsFinite(relativeTolerance) || relativeTolerance <= 0.0)
            closure.Add("relative tolerance must be finite and positive");
        if (variantValues.Any(v => !double.IsFinite(v.CouplingValue) || v.CouplingValue <= 0.0))
            closure.Add("all branch-variant coupling values must be finite and positive");

        double? maxRelativeDeviation = null;
        double? score = null;
        NormalizedWeakCouplingCandidateRecord? stableCandidate = null;
        if (closure.Count == 0)
        {
            maxRelativeDeviation = variantValues
                .Select(v => System.Math.Abs(v.CouplingValue - candidate.CouplingValue!.Value) / candidate.CouplingValue.Value)
                .Max();
            score = System.Math.Clamp(1.0 - (maxRelativeDeviation.Value / relativeTolerance), 0.0, 1.0);
            stableCandidate = candidate with
            {
                BranchStabilityScore = score,
            };
        }

        return new WeakCouplingBranchStabilityResult
        {
            AlgorithmId = AlgorithmId,
            TerminalStatus = closure.Count == 0
                ? "weak-coupling-branch-stability-derived"
                : "weak-coupling-branch-stability-blocked",
            RelativeTolerance = relativeTolerance,
            MaxRelativeDeviation = maxRelativeDeviation,
            BranchStabilityScore = score,
            VariantValues = variantValues,
            Candidate = stableCandidate,
            ClosureRequirements = closure,
        };
    }
}
