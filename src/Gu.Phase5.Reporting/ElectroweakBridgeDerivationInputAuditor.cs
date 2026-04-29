using System.Text.Json.Serialization;

namespace Gu.Phase5.Reporting;

public sealed class ElectroweakBridgeDerivationInputAuditRecord
{
    [JsonPropertyName("artifactId")]
    public required string ArtifactId { get; init; }

    [JsonPropertyName("artifactKind")]
    public required string ArtifactKind { get; init; }

    [JsonPropertyName("status")]
    public required string Status { get; init; }

    [JsonPropertyName("relevance")]
    public required string Relevance { get; init; }

    [JsonPropertyName("blockReasons")]
    public required IReadOnlyList<string> BlockReasons { get; init; }
}

public sealed class ElectroweakBridgeDerivationInputAuditResult
{
    [JsonPropertyName("terminalStatus")]
    public required string TerminalStatus { get; init; }

    [JsonPropertyName("records")]
    public required IReadOnlyList<ElectroweakBridgeDerivationInputAuditRecord> Records { get; init; }

    [JsonPropertyName("closureRequirements")]
    public required IReadOnlyList<string> ClosureRequirements { get; init; }
}

public static class ElectroweakBridgeDerivationInputAuditor
{
    public static ElectroweakBridgeDerivationInputAuditResult Audit(
        bool hasValidatedWzModes,
        bool hasExternalElectroweakScale,
        bool hasNormalizedWeakCoupling,
        bool hasMassGenerationRelation,
        bool hasScalarSectorBridge,
        bool hasSharedScaleCheck)
    {
        var records = new List<ElectroweakBridgeDerivationInputAuditRecord>
        {
            MakeRecord(
                "validated-wz-internal-modes",
                "physical-mode-records",
                hasValidatedWzModes,
                "required source modes for absolute W/Z projection",
                "validated W/Z internal mass modes are missing"),
            MakeRecord(
                "external-electroweak-scale",
                "external-scale-input",
                hasExternalElectroweakScale,
                "required GeV electroweak scale input",
                "external electroweak scale input is missing"),
            MakeRecord(
                "normalized-internal-weak-coupling",
                "bridge-input",
                hasNormalizedWeakCoupling,
                "accepted bridge input kind",
                "normalized internal weak coupling is missing"),
            MakeRecord(
                "internal-mass-generation-relation",
                "bridge-input",
                hasMassGenerationRelation,
                "accepted bridge input kind",
                "validated internal mass-generation relation is missing"),
            MakeRecord(
                "scalar-sector-bridge",
                "bridge-supporting-sector",
                hasScalarSectorBridge,
                "possible mass-generation support evidence",
                "scalar/Higgs-sector bridge evidence is missing"),
            MakeRecord(
                "shared-wz-scale-check",
                "bridge-validation-check",
                hasSharedScaleCheck,
                "required consistency check before projection",
                "shared W/Z GeV-per-internal-mass-unit scale check is missing"),
        };

        var closure = records
            .Where(record => !string.Equals(record.Status, "available", StringComparison.Ordinal))
            .SelectMany(record => record.BlockReasons)
            .ToArray();

        return new ElectroweakBridgeDerivationInputAuditResult
        {
            TerminalStatus = closure.Length == 0
                ? "bridge-derivation-inputs-ready"
                : "bridge-derivation-inputs-blocked",
            Records = records,
            ClosureRequirements = closure,
        };
    }

    private static ElectroweakBridgeDerivationInputAuditRecord MakeRecord(
        string artifactId,
        string artifactKind,
        bool available,
        string relevance,
        string blockReason) => new()
    {
        ArtifactId = artifactId,
        ArtifactKind = artifactKind,
        Status = available ? "available" : "missing",
        Relevance = relevance,
        BlockReasons = available ? [] : [blockReason],
    };
}
