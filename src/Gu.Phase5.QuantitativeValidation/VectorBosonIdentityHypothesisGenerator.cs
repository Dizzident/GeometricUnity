using System.Text.Json.Serialization;
using Gu.Core;

namespace Gu.Phase5.QuantitativeValidation;

public sealed class CandidateModeSourceBridgeTable
{
    [JsonPropertyName("tableId")]
    public required string TableId { get; init; }

    [JsonPropertyName("schemaVersion")]
    public required string SchemaVersion { get; init; }

    [JsonPropertyName("terminalStatus")]
    public required string TerminalStatus { get; init; }

    [JsonPropertyName("readySourceCount")]
    public required int ReadySourceCount { get; init; }

    [JsonPropertyName("candidateModeSources")]
    public required IReadOnlyList<CandidateModeSourceRecord> CandidateModeSources { get; init; }
}

public sealed class VectorBosonIdentityHypothesisResult
{
    [JsonPropertyName("resultId")]
    public required string ResultId { get; init; }

    [JsonPropertyName("schemaVersion")]
    public required string SchemaVersion { get; init; }

    [JsonPropertyName("terminalStatus")]
    public required string TerminalStatus { get; init; }

    [JsonPropertyName("candidateModes")]
    public required IReadOnlyList<IdentifiedPhysicalModeRecord> CandidateModes { get; init; }

    [JsonPropertyName("modeIdentificationEvidence")]
    public required ModeIdentificationEvidenceTable ModeIdentificationEvidence { get; init; }

    [JsonPropertyName("candidateObservables")]
    public required IReadOnlyList<QuantitativeObservableRecord> CandidateObservables { get; init; }

    [JsonPropertyName("physicalObservableMappings")]
    public required object PhysicalObservableMappings { get; init; }

    [JsonPropertyName("closureRequirements")]
    public required IReadOnlyList<string> ClosureRequirements { get; init; }

    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}

public static class VectorBosonIdentityHypothesisGenerator
{
    public const string AlgorithmId = "p23-wz-identity-hypothesis-screen:v1";

    public static VectorBosonIdentityHypothesisResult Generate(
        IReadOnlyList<CandidateModeSourceRecord> sources,
        ProvenanceMeta provenance)
    {
        ArgumentNullException.ThrowIfNull(sources);
        var ready = sources
            .Where(s => s.Value > 0 && s.Uncertainty >= 0)
            .OrderBy(s => s.Value)
            .ToList();

        if (ready.Count < 2)
            return Blocked("fewer than two ready identity-neutral source candidates are available.", provenance);

        var lower = ready[0];
        var upper = ready[^1];
        var wMode = CreateMode(lower, "candidate-w-vector-mode-from-p22-lowest-source", "w-boson", provenance);
        var zMode = CreateMode(upper, "candidate-z-vector-mode-from-p22-highest-source", "z-boson", provenance);
        var ratio = SpectrumObservableExtractor.CreatePositiveModeRatioRecord(
            wMode.Value,
            wMode.Uncertainty,
            wMode.ModeId,
            zMode.Value,
            zMode.Uncertainty,
            zMode.ModeId,
            "candidate-w-z-vector-mode-ratio-from-p22-hypothesis",
            lower.EnvironmentId,
            lower.BranchId,
            lower.RefinementLevel,
            provenance);

        return new VectorBosonIdentityHypothesisResult
        {
            ResultId = "phase23-wz-identity-hypotheses-v1",
            SchemaVersion = "1.0.0",
            TerminalStatus = "identity-blocked",
            CandidateModes = [wMode, zMode],
            ModeIdentificationEvidence = new ModeIdentificationEvidenceTable
            {
                TableId = "phase23-wz-mode-identification-evidence-v1",
                Evidence =
                [
                    CreateEvidence(wMode, lower, "lowest-ready-source-ordering-is-not-a-derived-w-identity-rule", provenance),
                    CreateEvidence(zMode, upper, "highest-ready-source-ordering-is-not-a-derived-z-identity-rule", provenance),
                ],
            },
            CandidateObservables = [ratio],
            PhysicalObservableMappings = new
            {
                tableId = "phase23-wz-candidate-mappings-v1",
                mappings = new[]
                {
                    new
                    {
                        mappingId = "blocked-p22-w-z-vector-mode-ratio-to-physical-mass-ratio",
                        particleId = "electroweak-sector",
                        physicalObservableType = "mass-ratio",
                        sourceComputedObservableId = ratio.ObservableId,
                        targetPhysicalObservableId = "physical-w-z-mass-ratio",
                        requiredEnvironmentId = ratio.EnvironmentId,
                        requiredBranchId = ratio.BranchId,
                        unitFamily = "dimensionless",
                        status = "blocked",
                        assumptions = new[]
                        {
                            "The source ratio is computed from P22 identity-neutral source candidates.",
                            "External physical target values were not used to select or calibrate the pair.",
                        },
                        closureRequirements = new[]
                        {
                            "Derive W and Z mode-identification rules independently of physical target values.",
                            "Validate the candidate mode-identification evidence records.",
                            "Promote physical observable mapping only after identity evidence is validated.",
                        },
                    },
                },
            },
            ClosureRequirements =
            [
                "derive W vector-mode identity rule independent of target values",
                "derive Z vector-mode identity rule independent of target values",
                "validate that the selected P22 source pair has electroweak W/Z identity",
                "keep physical target values external until identity and mapping gates pass",
            ],
            Provenance = provenance,
        };
    }

    private static VectorBosonIdentityHypothesisResult Blocked(string reason, ProvenanceMeta provenance)
        => new()
        {
            ResultId = "phase23-wz-identity-hypotheses-v1",
            SchemaVersion = "1.0.0",
            TerminalStatus = "source-blocked",
            CandidateModes = [],
            ModeIdentificationEvidence = new ModeIdentificationEvidenceTable
            {
                TableId = "phase23-wz-mode-identification-evidence-v1",
                Evidence = [],
            },
            CandidateObservables = [],
            PhysicalObservableMappings = new { tableId = "phase23-wz-candidate-mappings-v1", mappings = Array.Empty<object>() },
            ClosureRequirements = [reason],
            Provenance = provenance,
        };

    private static IdentifiedPhysicalModeRecord CreateMode(
        CandidateModeSourceRecord source,
        string modeId,
        string particleId,
        ProvenanceMeta provenance)
        => new()
        {
            ModeId = modeId,
            ParticleId = particleId,
            ModeKind = "vector-boson-mass-mode",
            ObservableId = source.SourceObservableId,
            Value = source.Value,
            Uncertainty = source.Uncertainty,
            UnitFamily = source.UnitFamily,
            Unit = source.Unit,
            Status = "provisional",
            EnvironmentId = source.EnvironmentId,
            BranchId = source.BranchId,
            RefinementLevel = source.RefinementLevel,
            ExtractionMethod = $"{AlgorithmId}:{source.SourceId}",
            Assumptions =
            [
                "mode is a W/Z identity hypothesis, not a validated physical identification",
                "external target values were not used as source observables or selection criteria",
            ],
            ClosureRequirements =
            [
                "validate independent mode-identification evidence",
                "validate physical observable mapping before comparison to real values",
            ],
            Provenance = provenance,
        };

    private static ModeIdentificationEvidenceRecord CreateEvidence(
        IdentifiedPhysicalModeRecord mode,
        CandidateModeSourceRecord source,
        string derivationId,
        ProvenanceMeta provenance)
        => new()
        {
            EvidenceId = $"provisional-{mode.ModeId}-evidence",
            ModeId = mode.ModeId,
            ParticleId = mode.ParticleId,
            ModeKind = mode.ModeKind,
            SourceObservableIds = [source.SourceObservableId],
            EnvironmentId = source.EnvironmentId,
            BranchId = source.BranchId,
            RefinementLevel = source.RefinementLevel,
            DerivationId = derivationId,
            Status = "provisional",
            Assumptions =
            [
                "ordering among ready P22 sources is a screening heuristic, not a physical identity derivation",
                "no external physical target value was used to create this evidence record",
            ],
            ClosureRequirements =
            [
                "replace ordering heuristic with derived W/Z identity rule",
                "test the identity rule across branch, refinement, and environment selectors",
            ],
            Provenance = provenance,
        };
}
