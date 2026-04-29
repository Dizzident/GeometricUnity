using System.Text.Json.Serialization;

namespace Gu.Phase5.Reporting;

public sealed class DimensionlessWeakCouplingAmplitudeExtractionResult
{
    [JsonPropertyName("algorithmId")]
    public required string AlgorithmId { get; init; }

    [JsonPropertyName("terminalStatus")]
    public required string TerminalStatus { get; init; }

    [JsonPropertyName("matrixElementId")]
    public required string MatrixElementId { get; init; }

    [JsonPropertyName("normalizationConventionId")]
    public string? NormalizationConventionId { get; init; }

    [JsonPropertyName("rawMatrixElementMagnitude")]
    public double? RawMatrixElementMagnitude { get; init; }

    [JsonPropertyName("generatorNormalizationScale")]
    public double? GeneratorNormalizationScale { get; init; }

    [JsonPropertyName("candidate")]
    public NormalizedWeakCouplingCandidateRecord? Candidate { get; init; }

    [JsonPropertyName("closureRequirements")]
    public required IReadOnlyList<string> ClosureRequirements { get; init; }
}

public static class DimensionlessWeakCouplingAmplitudeExtractor
{
    public const string AlgorithmId = "phase65-dimensionless-weak-coupling-amplitude-extractor-v1";
    public const string CandidateId = "phase65-dimensionless-weak-coupling-amplitude-candidate";

    public static DimensionlessWeakCouplingAmplitudeExtractionResult Extract(
        NonProxyFermionCurrentMatrixElementRecord matrixElement,
        Su2GeneratorNormalizationDerivationResult generatorNormalization,
        double rawMatrixElementMagnitude,
        IReadOnlyList<string> excludedTargetObservableIds,
        string? provenanceId = null)
    {
        ArgumentNullException.ThrowIfNull(matrixElement);
        ArgumentNullException.ThrowIfNull(generatorNormalization);
        ArgumentNullException.ThrowIfNull(excludedTargetObservableIds);

        var closure = new List<string>();
        if (!string.Equals(
                matrixElement.TerminalStatus,
                "non-proxy-fermion-current-matrix-element-derived",
                StringComparison.Ordinal))
            closure.Add("non-proxy fermion-current matrix element has not been derived");
        if (!string.Equals(
                generatorNormalization.TerminalStatus,
                "su2-generator-normalization-derived",
                StringComparison.Ordinal))
            closure.Add("canonical SU(2) generator normalization has not been derived");
        if (string.IsNullOrWhiteSpace(generatorNormalization.NormalizationConventionId))
            closure.Add("normalization convention id is missing");
        if (generatorNormalization.InternalToPhysicalGeneratorScale is not { } scale || !double.IsFinite(scale) || scale <= 0.0)
            closure.Add("internal-to-physical generator scale must be finite and positive");
        if (!double.IsFinite(rawMatrixElementMagnitude) || rawMatrixElementMagnitude <= 0.0)
            closure.Add("raw matrix-element magnitude must be finite and positive");
        if (matrixElement.UsesFiniteDifferenceProxy)
            closure.Add("matrix element source must not use finite-difference coupling proxies");
        if (matrixElement.UsesCouplingProxyMagnitude)
            closure.Add("matrix element source must not use coupling proxy magnitudes");

        NormalizedWeakCouplingCandidateRecord? candidate = null;
        if (closure.Count == 0)
        {
            var couplingValue = rawMatrixElementMagnitude * generatorNormalization.InternalToPhysicalGeneratorScale!.Value;
            candidate = new NormalizedWeakCouplingCandidateRecord
            {
                CandidateId = CandidateId,
                SourceKind = NormalizedWeakCouplingInputAuditor.AcceptedSourceKind,
                NormalizationConvention = generatorNormalization.NormalizationConventionId!,
                CouplingValue = couplingValue,
                CouplingUncertainty = null,
                VariationMethod = matrixElement.DerivationMethod,
                BranchStabilityScore = null,
                ExcludedTargetObservableIds = excludedTargetObservableIds,
                ProvenanceId = provenanceId,
            };
        }

        return new DimensionlessWeakCouplingAmplitudeExtractionResult
        {
            AlgorithmId = AlgorithmId,
            TerminalStatus = closure.Count == 0
                ? "dimensionless-weak-coupling-amplitude-extracted"
                : "dimensionless-weak-coupling-amplitude-blocked",
            MatrixElementId = matrixElement.MatrixElementId,
            NormalizationConventionId = generatorNormalization.NormalizationConventionId,
            RawMatrixElementMagnitude = double.IsFinite(rawMatrixElementMagnitude) ? rawMatrixElementMagnitude : null,
            GeneratorNormalizationScale = generatorNormalization.InternalToPhysicalGeneratorScale,
            Candidate = candidate,
            ClosureRequirements = closure,
        };
    }

    public static DimensionlessWeakCouplingAmplitudeExtractionResult ExtractFromEvidence(
        NonProxyFermionCurrentMatrixElementRecord matrixElement,
        Su2GeneratorNormalizationDerivationResult generatorNormalization,
        RawWeakCouplingMatrixElementEvidenceValidationResult rawMatrixElementEvidence,
        IReadOnlyList<string> excludedTargetObservableIds,
        string? provenanceId = null)
    {
        ArgumentNullException.ThrowIfNull(rawMatrixElementEvidence);

        if (!string.Equals(
                rawMatrixElementEvidence.TerminalStatus,
                "raw-weak-coupling-matrix-element-evidence-validated",
                StringComparison.Ordinal) ||
            rawMatrixElementEvidence.AcceptedRawMatrixElementMagnitude is not { } raw ||
            !double.IsFinite(raw) ||
            raw <= 0.0)
        {
            return new DimensionlessWeakCouplingAmplitudeExtractionResult
            {
                AlgorithmId = AlgorithmId,
                TerminalStatus = "dimensionless-weak-coupling-amplitude-blocked",
                MatrixElementId = matrixElement.MatrixElementId,
                NormalizationConventionId = generatorNormalization.NormalizationConventionId,
                RawMatrixElementMagnitude = null,
                GeneratorNormalizationScale = generatorNormalization.InternalToPhysicalGeneratorScale,
                Candidate = null,
                ClosureRequirements = rawMatrixElementEvidence.ClosureRequirements.Count == 0
                    ? ["raw matrix-element evidence has not been validated"]
                    : rawMatrixElementEvidence.ClosureRequirements,
            };
        }

        return Extract(
            matrixElement,
            generatorNormalization,
            raw,
            excludedTargetObservableIds,
            provenanceId ?? rawMatrixElementEvidence.Evidence.ProvenanceId);
    }
}
