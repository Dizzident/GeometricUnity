using System.Text.Json.Serialization;

namespace Gu.Phase5.Reporting;

public sealed class RawWeakCouplingMatrixElementEvidenceRecord
{
    [JsonPropertyName("evidenceId")]
    public required string EvidenceId { get; init; }

    [JsonPropertyName("sourceKind")]
    public required string SourceKind { get; init; }

    [JsonPropertyName("variationMethod")]
    public required string VariationMethod { get; init; }

    [JsonPropertyName("normalizationConvention")]
    public required string NormalizationConvention { get; init; }

    [JsonPropertyName("rawMatrixElementMagnitude")]
    public required double RawMatrixElementMagnitude { get; init; }

    [JsonPropertyName("usesFiniteDifferenceProxy")]
    public required bool UsesFiniteDifferenceProxy { get; init; }

    [JsonPropertyName("usesCouplingProxyMagnitude")]
    public required bool UsesCouplingProxyMagnitude { get; init; }

    [JsonPropertyName("replayedFromCouplingRecordId")]
    public string? ReplayedFromCouplingRecordId { get; init; }

    [JsonPropertyName("variationEvidenceId")]
    public string? VariationEvidenceId { get; init; }

    [JsonPropertyName("provenanceId")]
    public string? ProvenanceId { get; init; }
}

public sealed class RawWeakCouplingMatrixElementEvidenceValidationResult
{
    [JsonPropertyName("algorithmId")]
    public required string AlgorithmId { get; init; }

    [JsonPropertyName("terminalStatus")]
    public required string TerminalStatus { get; init; }

    [JsonPropertyName("evidence")]
    public required RawWeakCouplingMatrixElementEvidenceRecord Evidence { get; init; }

    [JsonPropertyName("acceptedRawMatrixElementMagnitude")]
    public double? AcceptedRawMatrixElementMagnitude { get; init; }

    [JsonPropertyName("closureRequirements")]
    public required IReadOnlyList<string> ClosureRequirements { get; init; }
}

public static class RawWeakCouplingMatrixElementEvidenceValidator
{
    public const string AlgorithmId = "phase77-raw-weak-coupling-matrix-element-evidence-validator-v1";
    public const string AcceptedSourceKind = "replayed-analytic-dirac-variation-matrix-element";
    public const string AcceptedVariationMethod = "analytic-dirac-variation-matrix-element:v1";
    public const string AcceptedNormalizationConvention = "unit-modes";

    public static RawWeakCouplingMatrixElementEvidenceValidationResult Validate(
        RawWeakCouplingMatrixElementEvidenceRecord evidence)
    {
        ArgumentNullException.ThrowIfNull(evidence);

        var closure = new List<string>();
        if (string.IsNullOrWhiteSpace(evidence.EvidenceId))
            closure.Add("matrix-element evidence id is missing");
        if (!string.Equals(evidence.SourceKind, AcceptedSourceKind, StringComparison.Ordinal))
            closure.Add($"matrix-element source kind must be {AcceptedSourceKind}");
        if (!string.Equals(evidence.VariationMethod, AcceptedVariationMethod, StringComparison.Ordinal))
            closure.Add($"variation method must be {AcceptedVariationMethod}");
        if (!string.Equals(evidence.NormalizationConvention, AcceptedNormalizationConvention, StringComparison.Ordinal))
            closure.Add($"normalization convention must be {AcceptedNormalizationConvention}");
        if (!double.IsFinite(evidence.RawMatrixElementMagnitude) || evidence.RawMatrixElementMagnitude <= 0.0)
            closure.Add("raw matrix-element magnitude must be finite and positive");
        if (evidence.UsesFiniteDifferenceProxy)
            closure.Add("raw matrix-element evidence must not use finite-difference coupling proxies");
        if (evidence.UsesCouplingProxyMagnitude)
            closure.Add("raw matrix-element evidence must not use coupling proxy magnitudes");
        if (string.IsNullOrWhiteSpace(evidence.ReplayedFromCouplingRecordId))
            closure.Add("replayed coupling record id is missing");
        if (string.IsNullOrWhiteSpace(evidence.VariationEvidenceId))
            closure.Add("variation evidence id is missing");
        if (string.IsNullOrWhiteSpace(evidence.ProvenanceId))
            closure.Add("provenance id is missing");

        return new RawWeakCouplingMatrixElementEvidenceValidationResult
        {
            AlgorithmId = AlgorithmId,
            TerminalStatus = closure.Count == 0
                ? "raw-weak-coupling-matrix-element-evidence-validated"
                : "raw-weak-coupling-matrix-element-evidence-blocked",
            Evidence = evidence,
            AcceptedRawMatrixElementMagnitude = closure.Count == 0 ? evidence.RawMatrixElementMagnitude : null,
            ClosureRequirements = closure,
        };
    }
}
