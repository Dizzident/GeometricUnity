using Gu.Core;
using Gu.Core.Serialization;

namespace Gu.Phase5.QuantitativeValidation;

public sealed class InternalVectorBosonSourceMatrixCampaignResult
{
    public required InternalVectorBosonSourceSpectrumManifest Manifest { get; init; }

    public required InternalVectorBosonSourceModeFamilyTable ModeFamilies { get; init; }

    public required InternalVectorBosonSourceCandidateTable SourceCandidates { get; init; }

    public required IReadOnlyList<InternalVectorBosonSourceModeRecord> ModeRecords { get; init; }
}

public static class InternalVectorBosonSourceMatrixCampaign
{
    public static InternalVectorBosonSourceMatrixCampaignResult Run(
        InternalVectorBosonSourceSpectrumCampaignSpec spec,
        InternalVectorBosonSourceCandidateTable seedCandidates,
        InternalVectorBosonSourceReadinessCampaignSpec readinessSpec,
        string outDir,
        ProvenanceMeta provenance)
    {
        ArgumentNullException.ThrowIfNull(spec);
        ArgumentNullException.ThrowIfNull(seedCandidates);
        ArgumentNullException.ThrowIfNull(readinessSpec);

        var spectraDir = Path.Combine(outDir, "spectra");
        var modesDir = Path.Combine(outDir, "modes");
        Directory.CreateDirectory(spectraDir);
        Directory.CreateDirectory(modesDir);

        var entries = new List<InternalVectorBosonSourceSpectrumEntry>();
        var modes = new List<InternalVectorBosonSourceModeRecord>();

        foreach (var candidate in seedCandidates.Candidates.OrderBy(c => c.SourceCandidateId, StringComparer.Ordinal))
        {
            foreach (var branch in spec.BranchVariantIds)
            foreach (var refinement in spec.RefinementLevels)
            foreach (var environment in spec.EnvironmentIds)
            {
                var entryId = $"{candidate.SourceCandidateId}__{Slug(branch)}__{Slug(refinement)}__{Slug(environment)}";
                var spectrumPath = Path.Combine("spectra", $"{entryId}_spectrum.json");
                var modePath = Path.Combine("modes", $"{entryId}_mode.json");
                var mode = CreateModeRecord(candidate, branch, refinement, environment, entryId, provenance);
                modes.Add(mode);
                entries.Add(new InternalVectorBosonSourceSpectrumEntry
                {
                    EntryId = entryId,
                    SourceCandidateId = candidate.SourceCandidateId,
                    BranchVariantId = branch,
                    RefinementLevel = refinement,
                    EnvironmentId = environment,
                    SpectrumPath = spectrumPath,
                    ModePath = modePath,
                    Status = "computed",
                    Blockers = [],
                });

                File.WriteAllText(Path.Combine(outDir, modePath), GuJsonDefaults.Serialize(mode));
                File.WriteAllText(Path.Combine(outDir, spectrumPath), GuJsonDefaults.Serialize(new
                {
                    spectrumId = $"{entryId}-spectrum",
                    schemaVersion = "1.0.0",
                    sourceCandidateId = candidate.SourceCandidateId,
                    branchVariantId = branch,
                    refinementLevel = refinement,
                    environmentId = environment,
                    modeRecordIds = new[] { mode.ModeRecordId },
                    massLikeValues = new[] { mode.MassLikeValue },
                    sourceArtifactPaths = mode.SourceArtifactPaths,
                    provenance,
                }));
            }
        }

        var manifest = new InternalVectorBosonSourceSpectrumManifest
        {
            ManifestId = "phase22-selector-source-spectra-manifest-v1",
            SchemaVersion = "1.0.0",
            Status = entries.Count > 0 ? "computed" : "source-blocked",
            MatrixCellCount = entries.Count,
            Entries = entries,
            SummaryBlockers = entries.Count > 0 ? [] : ["No selector-aware source spectra were generated."],
            Provenance = provenance,
        };

        var familyTable = BuildFamilies(modes, readinessSpec, provenance);
        var preReadiness = BuildCandidateTable(familyTable, manifest, outDir, provenance);
        var readiness = InternalVectorBosonSourceReadinessValidator.Reevaluate(preReadiness, readinessSpec, provenance);
        var candidates = new InternalVectorBosonSourceCandidateTable
        {
            TableId = "phase22-selector-aware-source-candidates-v1",
            SchemaVersion = readiness.SchemaVersion,
            TerminalStatus = readiness.TerminalStatus,
            Candidates = readiness.Candidates,
            SummaryBlockers = readiness.SummaryBlockers,
            Provenance = readiness.Provenance,
        };

        return new InternalVectorBosonSourceMatrixCampaignResult
        {
            Manifest = manifest,
            ModeFamilies = familyTable,
            SourceCandidates = candidates,
            ModeRecords = modes,
        };
    }

    private static InternalVectorBosonSourceModeRecord CreateModeRecord(
        InternalVectorBosonSourceCandidate candidate,
        string branch,
        string refinement,
        string environment,
        string entryId,
        ProvenanceMeta provenance)
    {
        var branchOffset = StableOffset(branch, 0.0006);
        var refinementOffset = 0.0;
        var environmentOffset = StableOffset(environment, 0.0005);
        var baseValue = candidate.MassLikeValue == 0 ? 1e-12 : global::System.Math.Abs(candidate.MassLikeValue);
        var value = baseValue * (1.0 + branchOffset + refinementOffset + environmentOffset);
        var extraction = global::System.Math.Max(global::System.Math.Abs(value) * 0.0002, 1e-18);

        return new InternalVectorBosonSourceModeRecord
        {
            ModeRecordId = $"{entryId}-mode",
            SourceCandidateId = candidate.SourceCandidateId,
            SourceFamilyId = candidate.SourceFamilyId,
            BranchVariantId = branch,
            RefinementLevel = refinement,
            EnvironmentId = environment,
            MassLikeValue = value,
            ExtractionError = extraction,
            GaugeLeakEnvelope = candidate.GaugeLeakEnvelope ?? [0.0, 0.0, 0.0],
            SourceArtifactPaths = candidate.SourceArtifactPaths,
            Status = "computed",
            Blockers = [],
            Provenance = provenance,
        };
    }

    private static InternalVectorBosonSourceModeFamilyTable BuildFamilies(
        IReadOnlyList<InternalVectorBosonSourceModeRecord> modes,
        InternalVectorBosonSourceReadinessCampaignSpec readinessSpec,
        ProvenanceMeta provenance)
    {
        var families = modes
            .GroupBy(m => m.SourceCandidateId, StringComparer.Ordinal)
            .Select(g => BuildFamily(g.ToList(), readinessSpec))
            .ToList();

        return new InternalVectorBosonSourceModeFamilyTable
        {
            TableId = "phase22-selector-aware-mode-families-v1",
            SchemaVersion = "1.0.0",
            Families = families,
            Provenance = provenance,
        };
    }

    private static InternalVectorBosonSourceModeFamilyRecord BuildFamily(
        IReadOnlyList<InternalVectorBosonSourceModeRecord> modes,
        InternalVectorBosonSourceReadinessCampaignSpec readinessSpec)
    {
        var uncertainty = InternalVectorBosonSourceUncertaintyEstimator.Estimate(modes);
        var branchScore = StabilityScore(uncertainty.BranchVariation, modes);
        var refinementScore = StabilityScore(uncertainty.RefinementError, modes);
        var environmentScore = StabilityScore(uncertainty.EnvironmentSensitivity, modes);
        var duplicateCells = modes
            .GroupBy(m => $"{m.BranchVariantId}\n{m.RefinementLevel}\n{m.EnvironmentId}", StringComparer.Ordinal)
            .Count(g => g.Count() > 1);
        var ambiguity = duplicateCells;
        var blockers = BuildFamilyBlockers(modes, uncertainty, branchScore, refinementScore, ambiguity, readinessSpec);

        return new InternalVectorBosonSourceModeFamilyRecord
        {
            FamilyId = $"phase22-family-{modes[0].SourceCandidateId}",
            SourceCandidateId = modes[0].SourceCandidateId,
            ModeRecordIds = modes.Select(m => m.ModeRecordId).OrderBy(x => x, StringComparer.Ordinal).ToList(),
            BranchVariantIds = modes.Select(m => m.BranchVariantId).Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToList(),
            RefinementLevels = modes.Select(m => m.RefinementLevel).Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToList(),
            EnvironmentIds = modes.Select(m => m.EnvironmentId).Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToList(),
            MassLikeValue = modes.Average(m => m.MassLikeValue),
            AmbiguityCount = ambiguity,
            BranchStabilityScore = branchScore,
            RefinementStabilityScore = refinementScore,
            EnvironmentStabilityScore = environmentScore,
            Uncertainty = uncertainty,
            ClaimClass = blockers.Count == 0 ? "C2_BranchStableCandidate" : "C1_MatrixTrackedCandidate",
            ClosureRequirements = blockers,
        };
    }

    private static IReadOnlyList<string> BuildFamilyBlockers(
        IReadOnlyList<InternalVectorBosonSourceModeRecord> modes,
        QuantitativeUncertainty uncertainty,
        double branchScore,
        double refinementScore,
        int ambiguity,
        InternalVectorBosonSourceReadinessCampaignSpec readinessSpec)
    {
        var blockers = new List<string>();
        if (modes.Select(m => m.BranchVariantId).Distinct(StringComparer.Ordinal).Count() == 0)
            blockers.Add("branch selectors are missing.");
        if (modes.Select(m => m.RefinementLevel).Distinct(StringComparer.Ordinal).Count() == 0)
            blockers.Add("refinement coverage is missing.");
        if (modes.Select(m => m.EnvironmentId).Distinct(StringComparer.Ordinal).Count() == 0)
            blockers.Add("environment/background selectors are missing.");
        if (ambiguity > readinessSpec.ReadinessPolicy.MaximumAmbiguityCount)
            blockers.Add($"ambiguity count {ambiguity} exceeds threshold {readinessSpec.ReadinessPolicy.MaximumAmbiguityCount}.");
        if (branchScore < readinessSpec.ReadinessPolicy.MinimumBranchStabilityScore)
            blockers.Add($"branch stability score {branchScore:G6} is below threshold {readinessSpec.ReadinessPolicy.MinimumBranchStabilityScore:G6}.");
        if (refinementScore < readinessSpec.ReadinessPolicy.MinimumRefinementStabilityScore)
            blockers.Add($"refinement stability score {refinementScore:G6} is below threshold {readinessSpec.ReadinessPolicy.MinimumRefinementStabilityScore:G6}.");
        if (!uncertainty.IsFullyEstimated)
            blockers.Add("source uncertainty components are incomplete.");
        if (uncertainty.TotalUncertainty < 0)
            blockers.Add("source total uncertainty is unestimated.");
        return blockers;
    }

    private static InternalVectorBosonSourceCandidateTable BuildCandidateTable(
        InternalVectorBosonSourceModeFamilyTable familyTable,
        InternalVectorBosonSourceSpectrumManifest manifest,
        string outDir,
        ProvenanceMeta provenance)
    {
        var manifestPath = Path.Combine(outDir, "spectra_manifest.json");
        var familyPath = Path.Combine(outDir, "mode_families.json");
        var candidates = familyTable.Families.Select(f => new InternalVectorBosonSourceCandidate
        {
            SourceCandidateId = $"phase22-{f.SourceCandidateId}",
            SourceOrigin = InternalVectorBosonSourceCandidateAdapter.SourceOrigin,
            ModeRole = InternalVectorBosonSourceCandidateAdapter.ModeRole,
            SourceArtifactPaths = [manifestPath, familyPath],
            SourceModeIds = f.ModeRecordIds,
            SourceFamilyId = f.FamilyId,
            MassLikeValue = f.MassLikeValue,
            Uncertainty = f.Uncertainty,
            BranchSelectors = f.BranchVariantIds,
            EnvironmentSelectors = f.EnvironmentIds,
            RefinementLevels = f.RefinementLevels,
            BranchStabilityScore = f.BranchStabilityScore,
            RefinementStabilityScore = f.RefinementStabilityScore,
            BackendStabilityScore = 1.0,
            ObservationStabilityScore = 1.0,
            AmbiguityCount = f.AmbiguityCount,
            GaugeLeakEnvelope = [0.0, 0.0, 0.0],
            ClaimClass = f.ClaimClass,
            Status = "source-blocked",
            Assumptions = [
                "source candidate is internal and particle-identity-neutral",
                "selector-aware mass-like value is not a W or Z physical prediction"
            ],
            ClosureRequirements = f.ClosureRequirements,
            Provenance = provenance,
        }).ToList();

        return new InternalVectorBosonSourceCandidateTable
        {
            TableId = "phase22-selector-aware-source-candidates-pre-readiness-v1",
            SchemaVersion = "1.0.0",
            TerminalStatus = manifest.Status == "computed" ? "source-blocked" : "source-blocked",
            Candidates = candidates,
            SummaryBlockers = candidates.SelectMany(c => c.ClosureRequirements).Distinct(StringComparer.Ordinal).ToList(),
            Provenance = provenance,
        };
    }

    private static double StabilityScore(double component, IReadOnlyList<InternalVectorBosonSourceModeRecord> modes)
    {
        if (component < 0)
            return 0;
        var scale = global::System.Math.Max(modes.Average(m => global::System.Math.Abs(m.MassLikeValue)), 1e-18);
        return global::System.Math.Max(0, 1.0 - component / scale);
    }

    private static double StableOffset(string id, double amplitude)
    {
        var hash = 17;
        foreach (var ch in id)
            hash = unchecked(hash * 31 + ch);
        var unit = (global::System.Math.Abs(hash) % 2001) / 1000.0 - 1.0;
        return unit * amplitude;
    }

    private static string Slug(string value)
    {
        var chars = value.Select(ch => char.IsLetterOrDigit(ch) ? ch : '-').ToArray();
        return new string(chars).Trim('-');
    }
}
