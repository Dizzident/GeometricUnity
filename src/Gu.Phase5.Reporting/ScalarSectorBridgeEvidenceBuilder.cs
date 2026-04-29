using System.Text.Json.Serialization;

namespace Gu.Phase5.Reporting;

public sealed class ScalarSectorBridgeEvidenceResult
{
    [JsonPropertyName("algorithmId")]
    public required string AlgorithmId { get; init; }

    [JsonPropertyName("terminalStatus")]
    public required string TerminalStatus { get; init; }

    [JsonPropertyName("evidenceId")]
    public required string EvidenceId { get; init; }

    [JsonPropertyName("scalarOrderParameterRole")]
    public string? ScalarOrderParameterRole { get; init; }

    [JsonPropertyName("sourceRelationId")]
    public string? SourceRelationId { get; init; }

    [JsonPropertyName("externalScaleInputId")]
    public string? ExternalScaleInputId { get; init; }

    [JsonPropertyName("closureRequirements")]
    public required IReadOnlyList<string> ClosureRequirements { get; init; }
}

public static class ScalarSectorBridgeEvidenceBuilder
{
    public const string AlgorithmId = "phase70-scalar-sector-bridge-evidence-builder-v1";
    public const string EvidenceId = "scalar-sector-bridge:electric-weak-vev-order-parameter:v1";

    public static ScalarSectorBridgeEvidenceResult Build(
        ElectroweakMassGenerationRelationResult relation,
        string externalScaleInputId,
        bool scalarOrderParameterDeclared)
    {
        ArgumentNullException.ThrowIfNull(relation);
        ArgumentException.ThrowIfNullOrWhiteSpace(externalScaleInputId);

        var closure = new List<string>();
        if (!string.Equals(relation.TerminalStatus, "electroweak-mass-generation-relation-derived", StringComparison.Ordinal))
            closure.Add("electroweak mass-generation relation has not been derived");
        if (!scalarOrderParameterDeclared)
            closure.Add("scalar-sector order-parameter role is not declared");

        return new ScalarSectorBridgeEvidenceResult
        {
            AlgorithmId = AlgorithmId,
            TerminalStatus = closure.Count == 0
                ? "scalar-sector-bridge-evidence-derived"
                : "scalar-sector-bridge-evidence-blocked",
            EvidenceId = EvidenceId,
            ScalarOrderParameterRole = closure.Count == 0 ? "external electroweak scale acts as scalar-sector order parameter v" : null,
            SourceRelationId = relation.MassGenerationRelationId,
            ExternalScaleInputId = externalScaleInputId,
            ClosureRequirements = closure,
        };
    }
}
