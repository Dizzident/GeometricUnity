using System.Text.Json.Serialization;
using Gu.Phase4.Couplings;

namespace Gu.Phase5.Reporting;

public sealed class ReplayedRawWeakCouplingMatrixElementEvidenceBuildResult
{
    [JsonPropertyName("algorithmId")]
    public required string AlgorithmId { get; init; }

    [JsonPropertyName("terminalStatus")]
    public required string TerminalStatus { get; init; }

    [JsonPropertyName("sourceCouplingId")]
    public required string SourceCouplingId { get; init; }

    [JsonPropertyName("evidenceValidation")]
    public required RawWeakCouplingMatrixElementEvidenceValidationResult EvidenceValidation { get; init; }

    [JsonPropertyName("closureRequirements")]
    public required IReadOnlyList<string> ClosureRequirements { get; init; }
}

public static class ReplayedRawWeakCouplingMatrixElementEvidenceBuilder
{
    public const string AlgorithmId = "phase78-replayed-raw-weak-coupling-matrix-element-evidence-builder-v1";

    public static ReplayedRawWeakCouplingMatrixElementEvidenceBuildResult Build(
        BosonFermionCouplingRecord coupling)
    {
        ArgumentNullException.ThrowIfNull(coupling);

        var closure = new List<string>();
        if (string.IsNullOrWhiteSpace(coupling.CouplingId))
            closure.Add("source coupling id is missing");
        if (!double.IsFinite(coupling.CouplingProxyReal))
            closure.Add("source coupling real component must be finite");
        if (!double.IsFinite(coupling.CouplingProxyImaginary))
            closure.Add("source coupling imaginary component must be finite");
        if (!double.IsFinite(coupling.CouplingProxyMagnitude) || coupling.CouplingProxyMagnitude <= 0.0)
            closure.Add("source coupling magnitude must be finite and positive");

        var rawMagnitude = double.IsFinite(coupling.CouplingProxyReal) &&
            double.IsFinite(coupling.CouplingProxyImaginary)
                ? System.Math.Sqrt(
                    coupling.CouplingProxyReal * coupling.CouplingProxyReal +
                    coupling.CouplingProxyImaginary * coupling.CouplingProxyImaginary)
                : double.NaN;

        if (double.IsFinite(coupling.CouplingProxyMagnitude) &&
            double.IsFinite(rawMagnitude) &&
            System.Math.Abs(rawMagnitude - coupling.CouplingProxyMagnitude) > 1e-12)
        {
            closure.Add("source coupling magnitude does not match real/imaginary matrix-element components");
        }

        var evidence = new RawWeakCouplingMatrixElementEvidenceRecord
        {
            EvidenceId = $"raw-weak-coupling-matrix-element:{coupling.CouplingId}",
            SourceKind = RawWeakCouplingMatrixElementEvidenceValidator.AcceptedSourceKind,
            VariationMethod = coupling.VariationMethod,
            NormalizationConvention = coupling.NormalizationConvention,
            RawMatrixElementMagnitude = rawMagnitude,
            UsesFiniteDifferenceProxy = !string.Equals(
                coupling.VariationMethod,
                RawWeakCouplingMatrixElementEvidenceValidator.AcceptedVariationMethod,
                StringComparison.Ordinal),
            UsesCouplingProxyMagnitude = false,
            ReplayedFromCouplingRecordId = coupling.CouplingId,
            VariationEvidenceId = coupling.VariationEvidenceId,
            ProvenanceId = BuildProvenanceId(coupling),
        };

        var validation = RawWeakCouplingMatrixElementEvidenceValidator.Validate(evidence);
        closure.AddRange(validation.ClosureRequirements);

        return new ReplayedRawWeakCouplingMatrixElementEvidenceBuildResult
        {
            AlgorithmId = AlgorithmId,
            TerminalStatus = closure.Count == 0
                ? "replayed-raw-weak-coupling-matrix-element-evidence-built"
                : "replayed-raw-weak-coupling-matrix-element-evidence-blocked",
            SourceCouplingId = coupling.CouplingId,
            EvidenceValidation = validation,
            ClosureRequirements = closure,
        };
    }

    private static string? BuildProvenanceId(BosonFermionCouplingRecord coupling)
    {
        var revision = coupling.Provenance.CodeRevision;
        var branch = coupling.Provenance.Branch?.BranchId;
        if (string.IsNullOrWhiteSpace(revision) || string.IsNullOrWhiteSpace(branch))
            return null;

        return $"{branch}:{revision}:{coupling.Provenance.Backend}";
    }
}
