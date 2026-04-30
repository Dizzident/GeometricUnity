using System.Text.Json.Serialization;

namespace Gu.Phase5.Reporting;

public sealed class FermionGaugeReductionInputRecord
{
    [JsonPropertyName("artifactId")]
    public required string ArtifactId { get; init; }

    [JsonPropertyName("backgroundId")]
    public required string BackgroundId { get; init; }

    [JsonPropertyName("diracBundleGaugeReductionApplied")]
    public required bool DiracBundleGaugeReductionApplied { get; init; }

    [JsonPropertyName("fermionModesGaugeReductionApplied")]
    public required bool FermionModesGaugeReductionApplied { get; init; }

    [JsonPropertyName("solverConfigRequestedGaugeReduction")]
    public required bool SolverConfigRequestedGaugeReduction { get; init; }

    [JsonPropertyName("hasGaugeProjectorArtifact")]
    public required bool HasGaugeProjectorArtifact { get; init; }

    [JsonPropertyName("hasGaugeReducedDiracOperatorArtifact")]
    public required bool HasGaugeReducedDiracOperatorArtifact { get; init; }

    [JsonPropertyName("hasBranchRefinementStabilityEvidence")]
    public required bool HasBranchRefinementStabilityEvidence { get; init; }
}

public sealed class FermionGaugeReductionReadinessAuditResult
{
    [JsonPropertyName("algorithmId")]
    public required string AlgorithmId { get; init; }

    [JsonPropertyName("terminalStatus")]
    public required string TerminalStatus { get; init; }

    [JsonPropertyName("artifact")]
    public required FermionGaugeReductionInputRecord Artifact { get; init; }

    [JsonPropertyName("closureRequirements")]
    public required IReadOnlyList<string> ClosureRequirements { get; init; }
}

public static class FermionGaugeReductionReadinessAuditor
{
    public const string AlgorithmId = "phase87-fermion-gauge-reduction-readiness-auditor-v1";

    public static FermionGaugeReductionReadinessAuditResult Audit(FermionGaugeReductionInputRecord artifact)
    {
        ArgumentNullException.ThrowIfNull(artifact);

        var closure = new List<string>();
        if (string.IsNullOrWhiteSpace(artifact.ArtifactId))
            closure.Add("artifact id is missing");
        if (string.IsNullOrWhiteSpace(artifact.BackgroundId))
            closure.Add("background id is missing");
        if (artifact.SolverConfigRequestedGaugeReduction && !artifact.DiracBundleGaugeReductionApplied)
            closure.Add("solver requested gauge reduction but the Dirac bundle was assembled with gaugeReductionApplied=false");
        if (!artifact.DiracBundleGaugeReductionApplied)
            closure.Add("gauge-reduced Dirac bundle is missing");
        if (!artifact.FermionModesGaugeReductionApplied)
            closure.Add("fermion modes are not marked gauge-reduced");
        if (!artifact.HasGaugeProjectorArtifact)
            closure.Add("fermion-compatible gauge projector artifact is missing");
        if (!artifact.HasGaugeReducedDiracOperatorArtifact)
            closure.Add("projected or gauge-reduced fermion Dirac operator artifact is missing");
        if (!artifact.HasBranchRefinementStabilityEvidence)
            closure.Add("branch/refinement stability evidence for the fermion modes is missing");

        return new FermionGaugeReductionReadinessAuditResult
        {
            AlgorithmId = AlgorithmId,
            TerminalStatus = closure.Count == 0
                ? "fermion-gauge-reduction-ready"
                : "fermion-gauge-reduction-blocked",
            Artifact = artifact,
            ClosureRequirements = closure,
        };
    }
}
