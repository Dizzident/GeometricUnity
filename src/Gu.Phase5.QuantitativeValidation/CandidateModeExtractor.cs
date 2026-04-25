using Gu.Core;

namespace Gu.Phase5.QuantitativeValidation;

public static class CandidateModeExtractor
{
    public const string InternalComputedOrigin = "internal-computed-artifact";
    public const string ComputedObservableArtifactKind = "computed-observable-table";
    public const string AlgorithmId = "p19-candidate-mode-extraction:v1";

    public static CandidateModeExtractionRecord ExtractCandidateMode(
        CandidateModeSourceRecord source,
        string extractionId,
        string modeId,
        string particleId,
        string modeKind,
        ProvenanceMeta provenance)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (string.IsNullOrWhiteSpace(extractionId))
            throw new ArgumentException("extractionId is required.", nameof(extractionId));
        if (string.IsNullOrWhiteSpace(modeId))
            throw new ArgumentException("modeId is required.", nameof(modeId));
        if (string.IsNullOrWhiteSpace(particleId))
            throw new ArgumentException("particleId is required.", nameof(particleId));
        if (string.IsNullOrWhiteSpace(modeKind))
            throw new ArgumentException("modeKind is required.", nameof(modeKind));

        var blockers = ValidateSource(source);
        if (blockers.Count > 0)
        {
            return CreateBlockedRecord(source, extractionId, particleId, modeKind, blockers, provenance);
        }

        var candidateMode = new IdentifiedPhysicalModeRecord
        {
            ModeId = modeId,
            ParticleId = particleId,
            ModeKind = modeKind,
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
                "candidate identity requires independent mode-identification evidence",
                "external target values were not used as source observables",
            ],
            ClosureRequirements =
            [
                "validate mode-identification evidence",
                "close physical-mode uncertainty budget",
            ],
            Provenance = provenance,
        };

        return new CandidateModeExtractionRecord
        {
            ExtractionId = extractionId,
            AlgorithmId = AlgorithmId,
            ParticleId = particleId,
            ModeKind = modeKind,
            SourceObservableIds = [source.SourceObservableId],
            SourceArtifactPath = source.SourceArtifactPath,
            EnvironmentId = source.EnvironmentId,
            BranchId = source.BranchId,
            RefinementLevel = source.RefinementLevel,
            Status = "provisional",
            CandidateMode = candidateMode,
            ClosureRequirements = candidateMode.ClosureRequirements,
            Provenance = provenance,
        };
    }

    private static List<string> ValidateSource(CandidateModeSourceRecord source)
    {
        var blockers = new List<string>();
        Require(source.SourceId, "sourceId", blockers);
        Require(source.SourceOrigin, "sourceOrigin", blockers);
        Require(source.SourceArtifactKind, "sourceArtifactKind", blockers);
        Require(source.SourceArtifactPath, "sourceArtifactPath", blockers);
        Require(source.SourceObservableId, "sourceObservableId", blockers);
        Require(source.UnitFamily, "unitFamily", blockers);
        Require(source.Unit, "unit", blockers);
        Require(source.EnvironmentId, "environmentId", blockers);
        Require(source.BranchId, "branchId", blockers);
        Require(source.SourceExtractionMethod, "sourceExtractionMethod", blockers);

        if (!string.Equals(source.SourceOrigin, InternalComputedOrigin, StringComparison.Ordinal))
            blockers.Add("sourceOrigin must be internal-computed-artifact; external targets are comparison data only.");
        if (!string.Equals(source.SourceArtifactKind, ComputedObservableArtifactKind, StringComparison.Ordinal))
            blockers.Add("sourceArtifactKind must be computed-observable-table, not an external target table.");
        if (Contains(source.SourceArtifactPath, "external_target") ||
            Contains(source.SourceArtifactPath, "target_table"))
        {
            blockers.Add("sourceArtifactPath appears to reference an external target table.");
        }
        if (source.Value <= 0)
            blockers.Add("source value must be positive for a mass-mode candidate.");
        if (source.Uncertainty < 0)
            blockers.Add("source uncertainty must be estimated before candidate extraction.");

        return blockers;
    }

    private static void Require(string? value, string name, List<string> blockers)
    {
        if (string.IsNullOrWhiteSpace(value))
            blockers.Add($"{name} is required.");
    }

    private static bool Contains(string? value, string fragment)
        => value?.Contains(fragment, StringComparison.OrdinalIgnoreCase) == true;

    private static CandidateModeExtractionRecord CreateBlockedRecord(
        CandidateModeSourceRecord source,
        string extractionId,
        string particleId,
        string modeKind,
        IReadOnlyList<string> blockers,
        ProvenanceMeta provenance)
        => new()
        {
            ExtractionId = extractionId,
            AlgorithmId = AlgorithmId,
            ParticleId = particleId,
            ModeKind = modeKind,
            SourceObservableIds = string.IsNullOrWhiteSpace(source.SourceObservableId) ? [] : [source.SourceObservableId],
            SourceArtifactPath = source.SourceArtifactPath,
            EnvironmentId = source.EnvironmentId,
            BranchId = source.BranchId,
            RefinementLevel = source.RefinementLevel,
            Status = "blocked",
            CandidateMode = null,
            ClosureRequirements = blockers,
            Provenance = provenance,
        };
}
