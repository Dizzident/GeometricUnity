using System.Text.Json.Serialization;

namespace Gu.Phase5.Reporting;

public sealed class WeakCouplingOperatorDerivationAuditRecord
{
    [JsonPropertyName("inputId")]
    public required string InputId { get; init; }

    [JsonPropertyName("inputKind")]
    public required string InputKind { get; init; }

    [JsonPropertyName("status")]
    public required string Status { get; init; }

    [JsonPropertyName("relevance")]
    public required string Relevance { get; init; }

    [JsonPropertyName("blockReasons")]
    public required IReadOnlyList<string> BlockReasons { get; init; }
}

public sealed class WeakCouplingOperatorDerivationAuditResult
{
    [JsonPropertyName("algorithmId")]
    public required string AlgorithmId { get; init; }

    [JsonPropertyName("terminalStatus")]
    public required string TerminalStatus { get; init; }

    [JsonPropertyName("records")]
    public required IReadOnlyList<WeakCouplingOperatorDerivationAuditRecord> Records { get; init; }

    [JsonPropertyName("closureRequirements")]
    public required IReadOnlyList<string> ClosureRequirements { get; init; }
}

public static class WeakCouplingOperatorDerivationAuditor
{
    public const string AlgorithmId = "phase62-weak-coupling-operator-derivation-auditor-v1";

    public static WeakCouplingOperatorDerivationAuditResult Audit(
        bool hasSharedWzOperatorUnit,
        bool hasSelectedWzSourceModes,
        bool hasCanonicalSu2GeneratorNormalization,
        bool hasNonProxyFermionCurrentMatrixElement,
        bool hasDimensionlessCouplingAmplitudeExtractor,
        bool hasCouplingUncertaintyPropagation,
        bool hasBranchStabilityEvidence)
    {
        var records = new List<WeakCouplingOperatorDerivationAuditRecord>
        {
            MakeRecord(
                "shared-wz-operator-unit",
                "operator-normalization",
                hasSharedWzOperatorUnit,
                "precondition supplied by Phase33 canonical W/Z operator normalization",
                "shared W/Z operator unit is missing"),
            MakeRecord(
                "selected-wz-source-modes",
                "mode-source-selection",
                hasSelectedWzSourceModes,
                "identifies the internal W/Z modes the coupling must support",
                "selected W/Z source modes are missing"),
            MakeRecord(
                "canonical-su2-generator-normalization",
                "group-normalization",
                hasCanonicalSu2GeneratorNormalization,
                "fixes trace and generator normalization before amplitudes become dimensionless weak couplings",
                "canonical SU(2) generator normalization is missing"),
            MakeRecord(
                "non-proxy-fermion-current-matrix-element",
                "operator-response",
                hasNonProxyFermionCurrentMatrixElement,
                "replaces finite-difference coupling proxies with a replayable operator matrix element",
                "non-proxy fermion-current matrix element is missing"),
            MakeRecord(
                "dimensionless-coupling-amplitude-extractor",
                "coupling-normalization",
                hasDimensionlessCouplingAmplitudeExtractor,
                "maps the internal operator response to a normalized dimensionless weak-coupling value",
                "dimensionless coupling amplitude extractor is missing"),
            MakeRecord(
                "coupling-uncertainty-propagation",
                "uncertainty-model",
                hasCouplingUncertaintyPropagation,
                "propagates source, operator, and numerical uncertainty into the weak-coupling candidate",
                "coupling uncertainty propagation is missing"),
            MakeRecord(
                "branch-stability-evidence",
                "stability-evidence",
                hasBranchStabilityEvidence,
                "proves the normalized weak-coupling candidate is stable across accepted branch variants",
                "branch-stability evidence is missing"),
        };

        var closure = records
            .Where(record => !string.Equals(record.Status, "available", StringComparison.Ordinal))
            .SelectMany(record => record.BlockReasons)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        return new WeakCouplingOperatorDerivationAuditResult
        {
            AlgorithmId = AlgorithmId,
            TerminalStatus = closure.Length == 0
                ? "weak-coupling-operator-derivation-ready"
                : "weak-coupling-operator-derivation-blocked",
            Records = records,
            ClosureRequirements = closure,
        };
    }

    private static WeakCouplingOperatorDerivationAuditRecord MakeRecord(
        string inputId,
        string inputKind,
        bool available,
        string relevance,
        string blockReason) => new()
    {
        InputId = inputId,
        InputKind = inputKind,
        Status = available ? "available" : "missing",
        Relevance = relevance,
        BlockReasons = available ? [] : [blockReason],
    };
}
