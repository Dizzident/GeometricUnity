using System.Text.Json.Serialization;

namespace Gu.Phase5.Reporting;

public sealed class ProductionAnalyticReplayInputMaterializationRecord
{
    [JsonPropertyName("artifactId")]
    public required string ArtifactId { get; init; }

    [JsonPropertyName("bosonModeSourceKind")]
    public required string BosonModeSourceKind { get; init; }

    [JsonPropertyName("hasBosonPerturbationVector")]
    public required bool HasBosonPerturbationVector { get; init; }

    [JsonPropertyName("hasAnalyticVariationMatrix")]
    public required bool HasAnalyticVariationMatrix { get; init; }

    [JsonPropertyName("hasFermionModeEigenvectors")]
    public required bool HasFermionModeEigenvectors { get; init; }

    [JsonPropertyName("hasFullCouplingRecord")]
    public required bool HasFullCouplingRecord { get; init; }

    [JsonPropertyName("isTopCouplingSummaryOnly")]
    public required bool IsTopCouplingSummaryOnly { get; init; }

    [JsonPropertyName("variationMethod")]
    public string? VariationMethod { get; init; }

    [JsonPropertyName("normalizationConvention")]
    public string? NormalizationConvention { get; init; }

    [JsonPropertyName("variationEvidenceId")]
    public string? VariationEvidenceId { get; init; }

    [JsonPropertyName("provenanceId")]
    public string? ProvenanceId { get; init; }
}

public sealed class ProductionAnalyticReplayInputMaterializationAuditResult
{
    [JsonPropertyName("algorithmId")]
    public required string AlgorithmId { get; init; }

    [JsonPropertyName("terminalStatus")]
    public required string TerminalStatus { get; init; }

    [JsonPropertyName("artifact")]
    public required ProductionAnalyticReplayInputMaterializationRecord Artifact { get; init; }

    [JsonPropertyName("closureRequirements")]
    public required IReadOnlyList<string> ClosureRequirements { get; init; }
}

public static class ProductionAnalyticReplayInputMaterializationAuditor
{
    public const string AlgorithmId = "phase80-production-analytic-replay-input-materialization-auditor-v1";
    public const string AcceptedBosonModeSourceKind = "selected-physical-wz-boson-mode";

    public static ProductionAnalyticReplayInputMaterializationAuditResult Audit(
        ProductionAnalyticReplayInputMaterializationRecord artifact)
    {
        ArgumentNullException.ThrowIfNull(artifact);

        var closure = new List<string>();
        if (string.IsNullOrWhiteSpace(artifact.ArtifactId))
            closure.Add("artifact id is missing");
        if (!string.Equals(artifact.BosonModeSourceKind, AcceptedBosonModeSourceKind, StringComparison.Ordinal))
            closure.Add($"boson mode source kind must be {AcceptedBosonModeSourceKind}");
        if (!artifact.HasBosonPerturbationVector)
            closure.Add("selected boson perturbation vector is missing");
        if (!artifact.HasAnalyticVariationMatrix)
            closure.Add("analytic variation matrix is missing");
        if (!artifact.HasFermionModeEigenvectors)
            closure.Add("selected fermion mode eigenvectors are missing");
        if (!artifact.HasFullCouplingRecord)
            closure.Add("full replayed coupling record is missing");
        if (artifact.IsTopCouplingSummaryOnly)
            closure.Add("top-coupling summary is insufficient; full real/imaginary coupling record is required");
        if (!string.Equals(
                artifact.VariationMethod,
                RawWeakCouplingMatrixElementEvidenceValidator.AcceptedVariationMethod,
                StringComparison.Ordinal))
            closure.Add($"variation method must be {RawWeakCouplingMatrixElementEvidenceValidator.AcceptedVariationMethod}");
        if (!string.Equals(
                artifact.NormalizationConvention,
                RawWeakCouplingMatrixElementEvidenceValidator.AcceptedNormalizationConvention,
                StringComparison.Ordinal))
            closure.Add($"normalization convention must be {RawWeakCouplingMatrixElementEvidenceValidator.AcceptedNormalizationConvention}");
        if (string.IsNullOrWhiteSpace(artifact.VariationEvidenceId))
            closure.Add("variation evidence id is missing");
        if (string.IsNullOrWhiteSpace(artifact.ProvenanceId))
            closure.Add("provenance id is missing");

        return new ProductionAnalyticReplayInputMaterializationAuditResult
        {
            AlgorithmId = AlgorithmId,
            TerminalStatus = closure.Count == 0
                ? "production-analytic-replay-inputs-materialized"
                : "production-analytic-replay-inputs-blocked",
            Artifact = artifact,
            ClosureRequirements = closure,
        };
    }
}
