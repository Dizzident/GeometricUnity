using Gu.Core.Serialization;
using Gu.Phase4.Registry;
using Gu.Phase5.Convergence;
using Gu.Phase5.Dossiers;
using Gu.Phase5.Environments;
using Gu.Phase5.Falsification;
using Gu.Phase5.QuantitativeValidation;

namespace Gu.Phase5.Reporting;

/// <summary>
/// Loads all campaign input artifacts from the paths declared in a <see cref="Phase5CampaignSpec"/>.
/// Implements D-004: the CLI is analysis-first — artifacts are loaded from persisted files.
/// Optional artifact paths (observationChainPath, environmentVariancePath, etc.) yield null when absent.
/// </summary>
public sealed class Phase5CampaignArtifacts
{
    /// <summary>Branch quantity values table (required).</summary>
    public required RefinementQuantityValueTable BranchQuantityValues { get; init; }

    /// <summary>Refinement quantity values table (required).</summary>
    public required RefinementQuantityValueTable RefinementValues { get; init; }

    /// <summary>Quantitative observable records (required).</summary>
    public required IReadOnlyList<QuantitativeObservableRecord> Observables { get; init; }

    /// <summary>Environment records (required).</summary>
    public required IReadOnlyList<EnvironmentRecord> EnvironmentRecords { get; init; }

    /// <summary>External target table (required).</summary>
    public required ExternalTargetTable TargetTable { get; init; }

    /// <summary>Unified particle registry (required).</summary>
    public required UnifiedParticleRegistry Registry { get; init; }

    /// <summary>Observation chain records (optional, typed by WP-6).</summary>
    public IReadOnlyList<ObservationChainRecord>? ObservationChainRecords { get; init; }

    /// <summary>Environment variance records (optional, typed by WP-7).</summary>
    public IReadOnlyList<EnvironmentVarianceRecord>? EnvironmentVarianceRecords { get; init; }

    /// <summary>Representation content records (optional, typed by WP-7).</summary>
    public IReadOnlyList<RepresentationContentRecord>? RepresentationContentRecords { get; init; }

    /// <summary>Coupling consistency records (optional, typed by WP-7).</summary>
    public IReadOnlyList<CouplingConsistencyRecord>? CouplingConsistencyRecords { get; init; }

    /// <summary>Sidecar generation summary inferred from sidecar_summary.json when present.</summary>
    public SidecarSummary? SidecarSummary { get; init; }

    /// <summary>
    /// Optional refinement evidence manifest inferred from the refinement values directory.
    /// When present, this captures whether convergence evidence is bridge-derived or
    /// direct solver-backed and how many source records seeded the ladder.
    /// </summary>
    public RefinementEvidenceManifest? RefinementEvidenceManifest { get; init; }

    /// <summary>
    /// Optional candidate-specific provenance links inferred from candidate_provenance_links.json
    /// in the campaign config directory.
    /// </summary>
    public IReadOnlyList<CandidateProvenanceLinkRecord>? CandidateProvenanceLinks { get; init; }

    /// <summary>Optional Phase XVI observable classifications.</summary>
    public ObservableClassificationTable? ObservableClassifications { get; init; }

    /// <summary>Optional Phase XVI physical observable mappings.</summary>
    public PhysicalObservableMappingTable? PhysicalObservableMappings { get; init; }

    /// <summary>Optional Phase XVII physical scale-setting/calibration records.</summary>
    public PhysicalCalibrationTable? PhysicalCalibrations { get; init; }
}

/// <summary>
/// Loads <see cref="Phase5CampaignArtifacts"/> from a campaign spec.
/// Paths in the spec that are relative are resolved relative to the campaign spec file's directory.
/// </summary>
public static class Phase5CampaignArtifactLoader
{
    /// <summary>
    /// Load all campaign artifacts described by <paramref name="spec"/>.
    /// </summary>
    /// <param name="spec">The campaign spec containing artifact paths.</param>
    /// <param name="specDir">
    /// Absolute directory of the campaign spec JSON file.
    /// Used to resolve relative paths.
    /// </param>
    public static Phase5CampaignArtifacts Load(Phase5CampaignSpec spec, string specDir)
    {
        ArgumentNullException.ThrowIfNull(spec);
        ArgumentException.ThrowIfNullOrWhiteSpace(specDir);

        // 1. Branch quantity values
        var branchValuesPath = ResolvePath(spec.BranchQuantityValuesPath, specDir);
        var branchValues = LoadRequired<RefinementQuantityValueTable>(branchValuesPath, "branchQuantityValuesPath");

        // 2. Refinement values
        var refinementValuesPath = ResolvePath(spec.RefinementValuesPath, specDir);
        var refinementValues = LoadRequired<RefinementQuantityValueTable>(refinementValuesPath, "refinementValuesPath");

        // 3. Observables
        var observablesPath = ResolvePath(spec.ObservablesPath, specDir);
        var observables = LoadRequiredList<QuantitativeObservableRecord>(observablesPath, "observablesPath");

        // 4. Environment records
        var envRecords = new List<EnvironmentRecord>();
        foreach (var envPath in spec.EnvironmentRecordPaths)
        {
            var absEnvPath = ResolvePath(envPath, specDir);
            var record = LoadRequired<EnvironmentRecord>(absEnvPath, $"environmentRecordPaths[{envPath}]");
            envRecords.Add(record);
        }

        // 5. External targets (from ExternalTargetTablePath)
        var targetsPath = ResolvePath(spec.ExternalTargetTablePath, specDir);
        var targetTable = LoadRequired<ExternalTargetTable>(targetsPath, "externalTargetTablePath");

        // 6. Registry
        var registryPath = ResolvePath(spec.RegistryPath, specDir);
        var registry = LoadRequired<UnifiedParticleRegistry>(registryPath, "registryPath");

        // 7. Observation chain records (optional, now typed — WP-6)
        IReadOnlyList<ObservationChainRecord>? observationChainRecords = null;
        if (spec.ObservationChainPath is not null)
        {
            var absPath = ResolvePath(spec.ObservationChainPath, specDir);
            if (File.Exists(absPath))
                observationChainRecords = GuJsonDefaults.Deserialize<List<ObservationChainRecord>>(
                    File.ReadAllText(absPath));
        }

        // 8. Environment variance records (optional)
        IReadOnlyList<EnvironmentVarianceRecord>? envVarianceRecords = null;
        if (spec.EnvironmentVariancePath is not null)
        {
            var absPath = ResolvePath(spec.EnvironmentVariancePath, specDir);
            if (File.Exists(absPath))
                envVarianceRecords = GuJsonDefaults.Deserialize<List<EnvironmentVarianceRecord>>(
                    File.ReadAllText(absPath));
        }

        // 9. Representation content records (optional)
        IReadOnlyList<RepresentationContentRecord>? repContentRecords = null;
        if (spec.RepresentationContentPath is not null)
        {
            var absPath = ResolvePath(spec.RepresentationContentPath, specDir);
            if (File.Exists(absPath))
                repContentRecords = GuJsonDefaults.Deserialize<List<RepresentationContentRecord>>(
                    File.ReadAllText(absPath));
        }

        // 10. Coupling consistency records (optional)
        IReadOnlyList<CouplingConsistencyRecord>? couplingRecords = null;
        if (spec.CouplingConsistencyPath is not null)
        {
            var absPath = ResolvePath(spec.CouplingConsistencyPath, specDir);
            if (File.Exists(absPath))
                couplingRecords = GuJsonDefaults.Deserialize<List<CouplingConsistencyRecord>>(
                    File.ReadAllText(absPath));
        }

        // 11. Sidecar summary (optional, inferred from the campaign config directory)
        SidecarSummary? sidecarSummary = null;
        var inferredSummaryPath = Path.Combine(specDir, "sidecar_summary.json");
        if (File.Exists(inferredSummaryPath))
        {
            sidecarSummary = GuJsonDefaults.Deserialize<SidecarSummary>(
                File.ReadAllText(inferredSummaryPath));
        }

        // 12. Bridge manifest (optional, inferred from the refinement values directory)
        RefinementEvidenceManifest? refinementEvidenceManifest = null;
        var inferredRefinementEvidenceManifestPath = Path.Combine(
            Path.GetDirectoryName(refinementValuesPath) ?? specDir,
            "refinement_evidence_manifest.json");
        if (File.Exists(inferredRefinementEvidenceManifestPath))
        {
            refinementEvidenceManifest = GuJsonDefaults.Deserialize<RefinementEvidenceManifest>(
                File.ReadAllText(inferredRefinementEvidenceManifestPath));
        }

        var inferredBridgeManifestPath = Path.Combine(
            Path.GetDirectoryName(refinementValuesPath) ?? specDir,
            "bridge_manifest.json");
        if (refinementEvidenceManifest is null && File.Exists(inferredBridgeManifestPath))
        {
            var bridgeManifest = GuJsonDefaults.Deserialize<BridgeManifest>(
                File.ReadAllText(inferredBridgeManifestPath));
            if (bridgeManifest is not null)
            {
                refinementEvidenceManifest = new RefinementEvidenceManifest
                {
                    ManifestId = $"legacy-bridge-{bridgeManifest.ManifestId}",
                    StudyId = spec.RefinementSpec.StudyId,
                    EvidenceSource = "bridge-derived",
                    SourceRecordIds = bridgeManifest.SourceRecordIds,
                    SourceArtifactRefs = bridgeManifest.SourceStateArtifactRefs,
                    Notes = $"Legacy bridge manifest inferred from {Path.GetFileName(inferredBridgeManifestPath)}.",
                    Provenance = bridgeManifest.Provenance,
                };
            }
        }

        // 13. Candidate-specific provenance links (optional)
        IReadOnlyList<CandidateProvenanceLinkRecord>? candidateProvenanceLinks = null;
        var inferredCandidateLinksPath = Path.Combine(specDir, "candidate_provenance_links.json");
        if (File.Exists(inferredCandidateLinksPath))
        {
            candidateProvenanceLinks = GuJsonDefaults.Deserialize<List<CandidateProvenanceLinkRecord>>(
                File.ReadAllText(inferredCandidateLinksPath));
        }

        // 14. Phase XVI observable classifications and physical mappings (optional)
        ObservableClassificationTable? observableClassifications = null;
        if (spec.ObservableClassificationsPath is not null)
        {
            var absPath = ResolvePath(spec.ObservableClassificationsPath, specDir);
            if (File.Exists(absPath))
                observableClassifications = GuJsonDefaults.Deserialize<ObservableClassificationTable>(
                    File.ReadAllText(absPath));
        }

        PhysicalObservableMappingTable? physicalObservableMappings = null;
        if (spec.PhysicalObservableMappingsPath is not null)
        {
            var absPath = ResolvePath(spec.PhysicalObservableMappingsPath, specDir);
            if (File.Exists(absPath))
                physicalObservableMappings = GuJsonDefaults.Deserialize<PhysicalObservableMappingTable>(
                    File.ReadAllText(absPath));
        }

        PhysicalCalibrationTable? physicalCalibrations = null;
        if (spec.PhysicalCalibrationPath is not null)
        {
            var absPath = ResolvePath(spec.PhysicalCalibrationPath, specDir);
            if (File.Exists(absPath))
                physicalCalibrations = GuJsonDefaults.Deserialize<PhysicalCalibrationTable>(
                    File.ReadAllText(absPath));
        }

        return new Phase5CampaignArtifacts
        {
            BranchQuantityValues = branchValues,
            RefinementValues = refinementValues,
            Observables = observables,
            EnvironmentRecords = envRecords,
            TargetTable = targetTable,
            Registry = registry,
            ObservationChainRecords = observationChainRecords,
            EnvironmentVarianceRecords = envVarianceRecords,
            RepresentationContentRecords = repContentRecords,
            CouplingConsistencyRecords = couplingRecords,
            SidecarSummary = sidecarSummary,
            RefinementEvidenceManifest = refinementEvidenceManifest,
            CandidateProvenanceLinks = candidateProvenanceLinks,
            ObservableClassifications = observableClassifications,
            PhysicalObservableMappings = physicalObservableMappings,
            PhysicalCalibrations = physicalCalibrations,
        };
    }

    private static string ResolvePath(string path, string baseDir)
    {
        return Path.IsPathRooted(path)
            ? path
            : Path.GetFullPath(Path.Combine(baseDir, path));
    }

    private static T LoadRequired<T>(string path, string fieldName)
    {
        if (!File.Exists(path))
            throw new ArtifactLoadException($"Required artifact not found for field '{fieldName}': {path}");

        var json = File.ReadAllText(path);
        var value = GuJsonDefaults.Deserialize<T>(json);
        if (value is null)
            throw new ArtifactLoadException(
                $"Failed to deserialize artifact for field '{fieldName}' at path: {path}");
        return value;
    }

    private static IReadOnlyList<T> LoadRequiredList<T>(string path, string fieldName)
    {
        if (!File.Exists(path))
            throw new ArtifactLoadException($"Required artifact not found for field '{fieldName}': {path}");

        var json = File.ReadAllText(path);
        var value = GuJsonDefaults.Deserialize<List<T>>(json);
        if (value is null)
            throw new ArtifactLoadException(
                $"Failed to deserialize list artifact for field '{fieldName}' at path: {path}");
        return value;
    }

}

/// <summary>Exception thrown when a required campaign artifact cannot be loaded.</summary>
public sealed class ArtifactLoadException : Exception
{
    public ArtifactLoadException(string message) : base(message) { }
}
