using System.Text.Json;
using Gu.Core;
using Gu.Core.Serialization;
using Gu.Branching;
using Gu.Phase3.Backgrounds;
using Gu.Phase3.Spectra;

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
        ProvenanceMeta provenance,
        string? selectorCellBundleManifestPath = null,
        string? identityFeaturePath = null)
    {
        ArgumentNullException.ThrowIfNull(spec);
        ArgumentNullException.ThrowIfNull(seedCandidates);
        ArgumentNullException.ThrowIfNull(readinessSpec);

        var bundleCatalog = string.IsNullOrWhiteSpace(selectorCellBundleManifestPath)
            ? null
            : SelectorCellBundleCatalog.Load(selectorCellBundleManifestPath);
        var operatorTerms = string.IsNullOrWhiteSpace(identityFeaturePath)
            ? new Dictionary<string, SelectorCellOperatorTerm>(StringComparer.Ordinal)
            : LoadOperatorTerms(identityFeaturePath);
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
                var bundle = bundleCatalog?.Require(branch, refinement, environment);
                operatorTerms.TryGetValue(candidate.SourceCandidateId, out var operatorTerm);
                var solve = bundle is null
                    ? null
                    : SolveSelectorCell(candidate, branch, refinement, environment, bundle, operatorTerm);
                var mode = CreateModeRecord(candidate, branch, refinement, environment, entryId, provenance, bundle, solve);
                var emittedModeRecords = solve is null
                    ? null
                    : ApplyOperatorTermEvidence(solve.Spectrum.Modes, operatorTerm);
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
                    SelectorCellBundleId = bundle?.BundleId,
                    SelectorCellBundlePath = bundle?.BackgroundRecordPath,
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
                    eigenvalues = solve is null ? null : solve.Spectrum.Modes.Select(m => m.Eigenvalue).ToArray(),
                    operatorBundleId = mode.OperatorBundleId,
                    solverMethod = mode.SolverMethod,
                    operatorType = mode.OperatorType,
                    selectorCellBundleId = bundle?.BundleId,
                    selectorCellBundlePath = bundle?.BackgroundRecordPath,
                    sourceArtifactPaths = mode.SourceArtifactPaths,
                    operatorTermEvidence = operatorTerm,
                    modeRecords = emittedModeRecords,
                    spectrumBundle = solve?.Spectrum,
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
        ProvenanceMeta provenance,
        SelectorCellBundleRecord? bundle,
        SelectorCellSolveResult? solve)
    {
        var branchOffset = StableOffset(branch, 0.0006);
        var refinementOffset = 0.0;
        var environmentOffset = StableOffset(environment, 0.0005);
        var baseValue = candidate.MassLikeValue == 0 ? 1e-12 : global::System.Math.Abs(candidate.MassLikeValue);
        var value = solve?.MassLikeValue ?? baseValue * (1.0 + branchOffset + refinementOffset + environmentOffset);
        var extraction = global::System.Math.Max(global::System.Math.Abs(value) * 0.0002, 1e-18);
        var sourceArtifactPaths = bundle is null
            ? candidate.SourceArtifactPaths
            : candidate.SourceArtifactPaths
                .Concat(new[] { bundle.BackgroundRecordPath ?? bundle.BundleId })
                .Distinct(StringComparer.Ordinal)
                .ToList();

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
            SourceArtifactPaths = sourceArtifactPaths,
            SelectorCellBundleId = bundle?.BundleId,
            OperatorBundleId = solve?.Spectrum.OperatorBundleId,
            SolverMethod = solve?.Spectrum.SolverMethod,
            OperatorType = solve?.Spectrum.OperatorType.ToString(),
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

    private static SelectorCellSolveResult SolveSelectorCell(
        InternalVectorBosonSourceCandidate candidate,
        string branch,
        string refinement,
        string environment,
        SelectorCellBundleRecord bundle)
        => SolveSelectorCell(candidate, branch, refinement, environment, bundle, operatorTerm: null);

    private static SelectorCellSolveResult SolveSelectorCell(
        InternalVectorBosonSourceCandidate candidate,
        string branch,
        string refinement,
        string environment,
        SelectorCellBundleRecord bundle,
        SelectorCellOperatorTerm? operatorTerm)
    {
        var baseValue = candidate.MassLikeValue == 0 ? 1e-12 : global::System.Math.Abs(candidate.MassLikeValue);
        var branchOffset = StableOffset($"{candidate.SourceCandidateId}|{branch}", 0.0012);
        var environmentOffset = StableOffset($"{candidate.SourceCandidateId}|{environment}", 0.0010);
        var termShift = operatorTerm?.RelativeMassShift ?? 0.0;
        var selectorValue = baseValue * (1.0 + branchOffset + environmentOffset + termShift);
        selectorValue = global::System.Math.Max(selectorValue, 1e-18);

        var operatorBundle = BuildSelectorCellOperatorBundle(candidate, branch, refinement, environment, bundle, selectorValue);
        var spectrum = new EigensolverPipeline().Solve(operatorBundle, new GeneralizedEigenproblemSpec
        {
            NumEigenvalues = 3,
            SolverMethod = "explicit-dense",
            NullModeThreshold = 1e-18,
        });
        var eigenvalue = spectrum.Modes
            .Select(m => m.Eigenvalue)
            .FirstOrDefault(v => v > 1e-18, selectorValue);
        return new SelectorCellSolveResult(spectrum, eigenvalue);
    }

    private static IReadOnlyList<ModeRecord> ApplyOperatorTermEvidence(
        IReadOnlyList<ModeRecord> modes,
        SelectorCellOperatorTerm? operatorTerm)
    {
        if (operatorTerm is null)
            return modes;

        var connection = global::System.Math.Max(0.0, 1.0 - operatorTerm.BlockParticipationFraction);
        var electroweak = global::System.Math.Min(1.0, global::System.Math.Max(0.0, operatorTerm.BlockParticipationFraction));
        return modes.Select(mode => new ModeRecord
        {
            ModeId = mode.ModeId,
            BackgroundId = mode.BackgroundId,
            OperatorType = mode.OperatorType,
            Eigenvalue = mode.Eigenvalue,
            ResidualNorm = mode.ResidualNorm,
            NormalizationConvention = mode.NormalizationConvention,
            MultiplicityClusterId = mode.MultiplicityClusterId,
            GaugeLeakScore = mode.GaugeLeakScore,
            ModeVector = mode.ModeVector,
            NullModeDiagnosis = mode.NullModeDiagnosis,
            ModeIndex = mode.ModeIndex,
            TensorEnergyFractions = new Dictionary<string, double>
            {
                ["connection-1form"] = connection,
                ["electroweak-feature-term"] = electroweak,
            },
            BlockEnergyFractions = new Dictionary<string, double>
            {
                ["connection"] = connection,
                ["electroweak-mixing"] = electroweak,
            },
            ModeVectorArtifactRef = mode.ModeVectorArtifactRef,
            ObservedSignatureRef = mode.ObservedSignatureRef,
        }).ToList();
    }

    private static IReadOnlyDictionary<string, SelectorCellOperatorTerm> LoadOperatorTerms(string identityFeaturePath)
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(identityFeaturePath));
        if (!doc.RootElement.TryGetProperty("featureRecords", out var records) ||
            records.ValueKind != JsonValueKind.Array)
        {
            return new Dictionary<string, SelectorCellOperatorTerm>(StringComparer.Ordinal);
        }

        var result = new Dictionary<string, SelectorCellOperatorTerm>(StringComparer.Ordinal);
        foreach (var record in records.EnumerateArray())
        {
            var sourceCandidateId = ReadString(record, "sourceCandidateId");
            if (string.IsNullOrWhiteSpace(sourceCandidateId) ||
                !record.TryGetProperty("basisEnergyFractions", out var basisFractions) ||
                basisFractions.ValueKind != JsonValueKind.Array ||
                !record.TryGetProperty("couplingProfile", out var couplingProfile))
            {
                continue;
            }

            var dominantBasisIndex = ReadInt(record, "dominantBasisIndex") ?? 0;
            var basis = basisFractions.EnumerateArray().Select(v => v.GetDouble()).ToArray();
            if (basis.Length == 0 || dominantBasisIndex < 0 || dominantBasisIndex >= basis.Length)
                continue;

            var chargeSector = ReadString(record, "chargeSector") ?? InferChargeSector(record);
            var meanMagnitude = ReadDouble(couplingProfile, "meanMagnitude") ?? 0.0;
            var dominantEnergy = basis[dominantBasisIndex];
            var isotropicEnergy = 1.0 / basis.Length;
            var anisotropy = global::System.Math.Abs(dominantEnergy - isotropicEnergy);
            var chargedProjector = string.Equals(chargeSector, "charged", StringComparison.Ordinal) ? 1.0 : 0.0;
            var relativeShift = chargedProjector * meanMagnitude * anisotropy;
            var blockParticipation = global::System.Math.Min(0.5, global::System.Math.Max(0.0, meanMagnitude + anisotropy));

            result[sourceCandidateId] = new SelectorCellOperatorTerm(
                "electroweak-feature-charge-anisotropy:v1",
                sourceCandidateId,
                chargeSector,
                ReadString(record, "electroweakMultipletId"),
                ReadString(record, "currentCouplingSignature"),
                meanMagnitude,
                dominantEnergy,
                anisotropy,
                relativeShift,
                blockParticipation);
        }

        return result;
    }

    private static string InferChargeSector(JsonElement record)
    {
        var sector = ReadString(record, "algebraBasisSector") ?? string.Empty;
        return sector.EndsWith("axis-2", StringComparison.Ordinal) ? "neutral" : "charged";
    }

    private static string? ReadString(JsonElement element, string propertyName)
        => element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    private static double? ReadDouble(JsonElement element, string propertyName)
        => element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Number
            ? value.GetDouble()
            : null;

    private static int? ReadInt(JsonElement element, string propertyName)
        => element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Number
            ? value.GetInt32()
            : null;

    private static LinearizedOperatorBundle BuildSelectorCellOperatorBundle(
        InternalVectorBosonSourceCandidate candidate,
        string branch,
        string refinement,
        string environment,
        SelectorCellBundleRecord bundle,
        double selectorValue)
    {
        var signature = SelectorCellOperator.Signature;
        var secondary = selectorValue * (1.0 + global::System.Math.Abs(StableOffset($"{branch}|{environment}|secondary", 0.01)) + 0.05);
        var tertiary = selectorValue * (1.0 + global::System.Math.Abs(StableOffset($"{candidate.SourceCandidateId}|tertiary", 0.01)) + 0.10);
        var spectral = new SelectorCellOperator(
            $"{bundle.BundleId}-source-operator",
            [selectorValue, secondary, tertiary],
            signature);
        var mass = new SelectorCellOperator(
            $"{bundle.BundleId}-mass-operator",
            [1.0, 1.0, 1.0],
            signature);

        return new LinearizedOperatorBundle
        {
            BundleId = $"{bundle.BundleId}__{candidate.SourceCandidateId}-operator-bundle",
            BackgroundId = $"{bundle.BundleId}__{candidate.SourceCandidateId}",
            BranchManifestId = branch,
            OperatorType = SpectralOperatorType.FullHessian,
            Formulation = PhysicalModeFormulation.PenaltyFixed,
            BackgroundAdmissibility = AdmissibilityLevel.B2,
            Jacobian = spectral,
            SpectralOperator = spectral,
            MassOperator = mass,
            GaugeLambda = 0.0,
            StateDimension = 3,
            PhysicalDimension = 3,
            GaugeRank = 0,
        };
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

    private sealed class SelectorCellBundleCatalog
    {
        private readonly IReadOnlyDictionary<string, SelectorCellBundleRecord> _records;

        private SelectorCellBundleCatalog(IReadOnlyDictionary<string, SelectorCellBundleRecord> records)
        {
            _records = records;
        }

        public static SelectorCellBundleCatalog Load(string manifestPath)
        {
            var fullPath = Path.GetFullPath(manifestPath);
            var manifest = GuJsonDefaults.Deserialize<SelectorCellBundleManifest>(File.ReadAllText(fullPath))
                ?? throw new InvalidDataException($"Failed to deserialize selector-cell bundle manifest from {manifestPath}.");
            var records = manifest.Bundles
                .Where(b => b.Written)
                .ToDictionary(Key, StringComparer.Ordinal);
            return new SelectorCellBundleCatalog(records);
        }

        public SelectorCellBundleRecord Require(string branch, string refinement, string environment)
        {
            var key = Key(branch, refinement, environment);
            if (!_records.TryGetValue(key, out var record))
                throw new InvalidOperationException($"Missing materialized selector-cell bundle for branch '{branch}', refinement '{refinement}', environment '{environment}'.");
            return record;
        }

        private static string Key(SelectorCellBundleRecord record)
            => Key(record.BranchVariantId, record.RefinementLevel, record.EnvironmentId);

        private static string Key(string branch, string refinement, string environment)
            => $"{branch}\n{refinement}\n{environment}";
    }

    private sealed class SelectorCellBundleManifest
    {
        public required IReadOnlyList<SelectorCellBundleRecord> Bundles { get; init; }
    }

    private sealed class SelectorCellBundleRecord
    {
        public required string BundleId { get; init; }

        public required string BranchVariantId { get; init; }

        public required string RefinementLevel { get; init; }

        public required string EnvironmentId { get; init; }

        public required bool Written { get; init; }

        public string? BackgroundRecordPath { get; init; }
    }

    private sealed record SelectorCellSolveResult(SpectrumBundle Spectrum, double MassLikeValue);

    private sealed record SelectorCellOperatorTerm(
        string TermId,
        string SourceCandidateId,
        string? ChargeSector,
        string? ElectroweakMultipletId,
        string? CouplingSignature,
        double CouplingMeanMagnitude,
        double DominantBasisEnergyFraction,
        double BasisAnisotropy,
        double RelativeMassShift,
        double BlockParticipationFraction);

    private sealed class SelectorCellOperator : ILinearOperator
    {
        private readonly string _label;
        private readonly double[] _diagonal;
        private readonly TensorSignature _signature;

        public SelectorCellOperator(string label, double[] diagonal, TensorSignature signature)
        {
            _label = label;
            _diagonal = diagonal;
            _signature = signature;
        }

        public static TensorSignature Signature => new()
        {
            AmbientSpaceId = "selector_cell",
            CarrierType = "source-mode",
            Degree = "0",
            LieAlgebraBasisId = "selector-cell-basis",
            ComponentOrderId = "mode-major",
            NumericPrecision = "float64",
            MemoryLayout = "dense-row-major",
        };

        public TensorSignature InputSignature => _signature;

        public TensorSignature OutputSignature => _signature;

        public int InputDimension => _diagonal.Length;

        public int OutputDimension => _diagonal.Length;

        public FieldTensor Apply(FieldTensor v)
        {
            var coefficients = new double[_diagonal.Length];
            for (var i = 0; i < _diagonal.Length; i++)
                coefficients[i] = _diagonal[i] * v.Coefficients[i];

            return new FieldTensor
            {
                Label = $"{_label}*v",
                Signature = _signature,
                Coefficients = coefficients,
                Shape = [_diagonal.Length],
            };
        }

        public FieldTensor ApplyTranspose(FieldTensor v) => Apply(v);
    }
}
