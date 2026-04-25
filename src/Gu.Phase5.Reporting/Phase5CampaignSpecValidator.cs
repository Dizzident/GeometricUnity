using Gu.Core.Serialization;
using Gu.Phase5.Environments;
using Gu.Phase5.QuantitativeValidation;
using Gu.Validation;

namespace Gu.Phase5.Reporting;

/// <summary>
/// Result of validating a Phase5CampaignSpec.
/// </summary>
public sealed class CampaignValidationResult
{
    /// <summary>Whether the spec is valid.</summary>
    public required bool IsValid { get; init; }

    /// <summary>List of validation error messages (empty when IsValid is true).</summary>
    public required IReadOnlyList<string> Errors { get; init; }
}

/// <summary>
/// Validates a Phase5CampaignSpec before running the campaign.
/// </summary>
public static class Phase5CampaignSpecValidator
{
    private const string MinReferenceSchemaVersion = "1.1.0";

    /// <summary>
    /// Validate a Phase5CampaignSpec.
    /// </summary>
    /// <param name="spec">The campaign spec to validate.</param>
    /// <param name="specDir">Directory used to resolve relative paths in the spec.</param>
    /// <param name="requireReferenceSidecars">
    /// When true, optional sidecar paths (ObservationChainPath, EnvironmentVariancePath,
    /// RepresentationContentPath, CouplingConsistencyPath) must be non-null and the files
    /// must exist. Also requires schemaVersion >= 1.1.0 and at least 2 environment records.
    /// </param>
    /// <returns>A CampaignValidationResult describing any errors found.</returns>
    public static CampaignValidationResult Validate(
        Phase5CampaignSpec spec,
        string specDir,
        bool requireReferenceSidecars)
    {
        ArgumentNullException.ThrowIfNull(spec);
        ArgumentNullException.ThrowIfNull(specDir);

        var errors = new List<string>();
        var repoRoot = FindRepoRoot(specDir);
        ValidateSchema(
            GuJsonDefaults.Serialize(spec),
            Path.Combine(repoRoot, "schemas", "phase5_campaign.schema.json"),
            "phase5_campaign.schema.json",
            errors);

        // --- Required file paths ---
        CheckFilePath(spec.ExternalTargetTablePath, "externalTargetTablePath", specDir, errors);
        CheckFilePath(spec.BranchQuantityValuesPath, "branchQuantityValuesPath", specDir, errors);
        CheckFilePath(spec.RefinementValuesPath, "refinementValuesPath", specDir, errors);
        CheckFilePath(spec.ObservablesPath, "observablesPath", specDir, errors);
        CheckFilePath(spec.RegistryPath, "registryPath", specDir, errors);

        // --- Environment record paths ---
        if (spec.EnvironmentRecordPaths is null || spec.EnvironmentRecordPaths.Count == 0)
        {
            errors.Add("environmentRecordPaths: no environment records declared.");
        }
        else
        {
            for (int i = 0; i < spec.EnvironmentRecordPaths.Count; i++)
            {
                var p = spec.EnvironmentRecordPaths[i];
                if (string.IsNullOrWhiteSpace(p))
                    errors.Add($"environmentRecordPaths[{i}]: path is null or empty.");
                else
                    CheckFilePath(p, $"environmentRecordPaths[{i}]", specDir, errors);
            }
        }

        // --- Reference campaign requirements ---
        if (requireReferenceSidecars)
        {
            // schemaVersion must be >= 1.1.0
            if (!IsVersionAtLeast(spec.SchemaVersion, MinReferenceSchemaVersion))
                errors.Add($"requireReferenceSidecars: schemaVersion must be >= {MinReferenceSchemaVersion}; found \"{spec.SchemaVersion}\".");

            // All four sidecar paths must be present and exist
            CheckSidecarPath(spec.ObservationChainPath, "observationChainPath", specDir, errors);
            CheckSidecarPath(spec.EnvironmentVariancePath, "environmentVariancePath", specDir, errors);
            CheckSidecarPath(spec.RepresentationContentPath, "representationContentPath", specDir, errors);
            CheckSidecarPath(spec.CouplingConsistencyPath, "couplingConsistencyPath", specDir, errors);

            var inferredSidecarSummaryPath = Path.Combine(specDir, "sidecar_summary.json");
            if (!File.Exists(inferredSidecarSummaryPath))
            {
                errors.Add($"requireReferenceSidecars: sidecar_summary.json not found at \"{inferredSidecarSummaryPath}\".");
            }
            else
            {
                ValidateSchema(
                    File.ReadAllText(inferredSidecarSummaryPath),
                    Path.Combine(repoRoot, "schemas", "sidecar_summary.schema.json"),
                    "sidecar_summary.schema.json",
                    errors);
            }
        }
        else
        {
            // Optional sidecar paths: if provided, files must exist
            if (spec.ObservationChainPath is not null)
                CheckFilePath(spec.ObservationChainPath, "observationChainPath", specDir, errors);
            if (spec.EnvironmentVariancePath is not null)
                CheckFilePath(spec.EnvironmentVariancePath, "environmentVariancePath", specDir, errors);
            if (spec.RepresentationContentPath is not null)
                CheckFilePath(spec.RepresentationContentPath, "representationContentPath", specDir, errors);
            if (spec.CouplingConsistencyPath is not null)
                CheckFilePath(spec.CouplingConsistencyPath, "couplingConsistencyPath", specDir, errors);
        }

        if (spec.TargetCoverageBlockersPath is not null)
            CheckFilePath(spec.TargetCoverageBlockersPath, "targetCoverageBlockersPath", specDir, errors);
        if (spec.PhysicalObservableMappingsPath is not null)
            CheckFilePath(spec.PhysicalObservableMappingsPath, "physicalObservableMappingsPath", specDir, errors);
        if (spec.ObservableClassificationsPath is not null)
            CheckFilePath(spec.ObservableClassificationsPath, "observableClassificationsPath", specDir, errors);
        if (spec.PhysicalCalibrationPath is not null)
            CheckFilePath(spec.PhysicalCalibrationPath, "physicalCalibrationPath", specDir, errors);

        ValidateEnvironmentEvidence(spec, specDir, requireReferenceSidecars, errors);
        ValidateTargetTable(spec, specDir, repoRoot, errors);
        ValidateObservableClassifications(spec, specDir, repoRoot, errors);
        ValidateSidecarSchemas(spec, specDir, repoRoot, errors);

        return new CampaignValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors,
        };
    }

    private static void CheckFilePath(string path, string fieldName, string specDir, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            errors.Add($"{fieldName}: path is null or empty.");
            return;
        }

        var resolved = Path.IsPathRooted(path) ? path : Path.Combine(specDir, path);
        if (!File.Exists(resolved))
            errors.Add($"{fieldName}: file not found at \"{resolved}\".");
    }

    private static void CheckSidecarPath(string? path, string fieldName, string specDir, List<string> errors)
    {
        if (path is null)
        {
            errors.Add($"{fieldName}: required in reference mode (requireReferenceSidecars=true) but is null.");
            return;
        }
        CheckFilePath(path, fieldName, specDir, errors);
    }

    private static void ValidateEnvironmentEvidence(
        Phase5CampaignSpec spec,
        string specDir,
        bool requireReferenceSidecars,
        List<string> errors)
    {
        var tiers = new HashSet<string>(StringComparer.Ordinal);
        foreach (var path in spec.EnvironmentRecordPaths)
        {
            var resolved = ResolvePath(path, specDir);
            if (!File.Exists(resolved))
                continue;

            try
            {
                var record = GuJsonDefaults.Deserialize<EnvironmentRecord>(File.ReadAllText(resolved));
                if (record is null)
                {
                    errors.Add($"environmentRecordPaths[{path}]: failed to deserialize EnvironmentRecord.");
                    continue;
                }

                tiers.Add(record.GeometryTier);
                if (record.GeometryTier == "imported" &&
                    (string.IsNullOrWhiteSpace(record.DatasetId) ||
                     string.IsNullOrWhiteSpace(record.SourceHash) ||
                     string.IsNullOrWhiteSpace(record.ConversionVersion)))
                {
                    errors.Add($"environmentRecordPaths[{path}]: imported environment is missing datasetId/sourceHash/conversionVersion.");
                }
            }
            catch (Exception ex)
            {
                errors.Add($"environmentRecordPaths[{path}]: failed to load EnvironmentRecord ({ex.Message}).");
            }
        }

        if (requireReferenceSidecars)
        {
            if (!tiers.Contains("toy"))
                errors.Add("requireReferenceSidecars: reference campaign must include a toy environment record.");
            if (!tiers.Contains("structured"))
                errors.Add("requireReferenceSidecars: reference campaign must include a structured environment record.");
        }
    }

    private static void ValidateTargetTable(
        Phase5CampaignSpec spec,
        string specDir,
        string repoRoot,
        List<string> errors)
    {
        var resolved = ResolvePath(spec.ExternalTargetTablePath, specDir);
        if (!File.Exists(resolved))
            return;

        try
        {
            var table = GuJsonDefaults.Deserialize<ExternalTargetTable>(File.ReadAllText(resolved));
            var observablesPath = ResolvePath(spec.ObservablesPath, specDir);
            IReadOnlyList<QuantitativeObservableRecord> observables = Array.Empty<QuantitativeObservableRecord>();
            if (File.Exists(observablesPath))
            {
                var loadedObservables = GuJsonDefaults.Deserialize<List<QuantitativeObservableRecord>>(File.ReadAllText(observablesPath));
                if (loadedObservables is null)
                    errors.Add("observablesPath: failed to deserialize quantitative observables.");
                else
                    observables = loadedObservables;
            }

            var environmentTiers = LoadEnvironmentTierMap(spec, specDir);
            var targetCoverageBlockers = LoadTargetCoverageBlockers(spec, specDir, errors);
            var physicalMappings = LoadPhysicalMappings(spec, specDir, repoRoot, errors);
            var physicalCalibrations = LoadPhysicalCalibrations(spec, specDir, repoRoot, errors);
            if (table is null)
            {
                errors.Add("externalTargetTablePath: failed to deserialize ExternalTargetTable.");
                return;
            }

            if (table.Targets.Any(t => string.IsNullOrWhiteSpace(t.DistributionModel)))
                errors.Add("externalTargetTablePath: every target must declare distributionModel explicitly.");

            if (!table.Targets.Any(t => string.Equals(t.EvidenceTier, "derived-synthetic", StringComparison.OrdinalIgnoreCase)
                                     || string.Equals(t.EvidenceTier, "evidence-grade", StringComparison.OrdinalIgnoreCase)))
            {
                errors.Add("externalTargetTablePath: reference campaign must distinguish toy-placeholder targets from stronger target tiers.");
            }

            var duplicatedObservableIds = observables
                .GroupBy(o => o.ObservableId, StringComparer.Ordinal)
                .Where(g => g.Select(o => o.EnvironmentId).Distinct(StringComparer.Ordinal).Count() > 1)
                .Select(g => g.Key)
                .ToHashSet(StringComparer.Ordinal);

            foreach (var target in table.Targets.Where(t => duplicatedObservableIds.Contains(t.ObservableId)))
            {
                if (string.IsNullOrWhiteSpace(target.TargetEnvironmentId) &&
                    string.IsNullOrWhiteSpace(target.TargetEnvironmentTier))
                {
                    errors.Add(
                        $"externalTargetTablePath: target '{target.Label}' for observableId '{target.ObservableId}' " +
                        "must declare targetEnvironmentId or targetEnvironmentTier because multiple environment-specific observables exist.");
                }
            }

            foreach (var target in table.Targets)
            {
                ValidatePhysicalTargetMapping(target, physicalMappings, physicalCalibrations, errors);

                if (observables.Any(obs => TargetSelectorsMatch(obs, target, environmentTiers)))
                    continue;

                if (HasTargetCoverageBlocker(target, targetCoverageBlockers))
                    continue;

                errors.Add(
                    $"externalTargetTablePath: target '{target.Label}' for observableId '{target.ObservableId}' " +
                    "has no matching computed observable after applying targetEnvironmentId/targetEnvironmentTier selectors.");
            }
        }
        catch (Exception ex)
        {
            errors.Add($"externalTargetTablePath: failed to validate target table ({ex.Message}).");
        }
    }

    private static IReadOnlyList<PhysicalObservableMapping> LoadPhysicalMappings(
        Phase5CampaignSpec spec,
        string specDir,
        string repoRoot,
        List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(spec.PhysicalObservableMappingsPath))
            return Array.Empty<PhysicalObservableMapping>();

        var resolved = ResolvePath(spec.PhysicalObservableMappingsPath, specDir);
        if (!File.Exists(resolved))
            return Array.Empty<PhysicalObservableMapping>();

        ValidateSchema(
            File.ReadAllText(resolved),
            Path.Combine(repoRoot, "schemas", "physical_observable_mapping.schema.json"),
            "physicalObservableMappingsPath:physical_observable_mapping.schema.json",
            errors);

        try
        {
            var table = GuJsonDefaults.Deserialize<PhysicalObservableMappingTable>(File.ReadAllText(resolved));
            if (table is null)
            {
                errors.Add("physicalObservableMappingsPath: failed to deserialize PhysicalObservableMappingTable.");
                return Array.Empty<PhysicalObservableMapping>();
            }

            foreach (var mapping in table.Mappings)
            {
                if (string.IsNullOrWhiteSpace(mapping.MappingId))
                    errors.Add("physicalObservableMappingsPath: every mapping must declare mappingId.");
                if (string.IsNullOrWhiteSpace(mapping.ParticleId))
                    errors.Add($"physicalObservableMappingsPath: mapping '{mapping.MappingId}' must declare particleId.");
                if (string.IsNullOrWhiteSpace(mapping.PhysicalObservableType))
                    errors.Add($"physicalObservableMappingsPath: mapping '{mapping.MappingId}' must declare physicalObservableType.");
                if (string.IsNullOrWhiteSpace(mapping.SourceComputedObservableId))
                    errors.Add($"physicalObservableMappingsPath: mapping '{mapping.MappingId}' must declare sourceComputedObservableId.");
                if (string.Equals(mapping.Status, "validated", StringComparison.OrdinalIgnoreCase) &&
                    string.IsNullOrWhiteSpace(mapping.TargetPhysicalObservableId))
                {
                    errors.Add($"physicalObservableMappingsPath: validated mapping '{mapping.MappingId}' must declare targetPhysicalObservableId.");
                }
                if (string.IsNullOrWhiteSpace(mapping.UnitFamily))
                    errors.Add($"physicalObservableMappingsPath: mapping '{mapping.MappingId}' must declare unitFamily.");
                if (!IsKnownMappingStatus(mapping.Status))
                    errors.Add($"physicalObservableMappingsPath: mapping '{mapping.MappingId}' has unknown status '{mapping.Status}'.");
                if (mapping.Assumptions.Count == 0)
                    errors.Add($"physicalObservableMappingsPath: mapping '{mapping.MappingId}' must declare at least one assumption.");
                if (string.Equals(mapping.Status, "blocked", StringComparison.OrdinalIgnoreCase) &&
                    mapping.ClosureRequirements.Count == 0)
                {
                    errors.Add($"physicalObservableMappingsPath: blocked mapping '{mapping.MappingId}' must declare closureRequirements.");
                }
            }

            return table.Mappings;
        }
        catch (Exception ex)
        {
            errors.Add($"physicalObservableMappingsPath: failed to validate mappings ({ex.Message}).");
            return Array.Empty<PhysicalObservableMapping>();
        }
    }

    private static void ValidatePhysicalTargetMapping(
        ExternalTarget target,
        IReadOnlyList<PhysicalObservableMapping> mappings,
        IReadOnlyList<PhysicalCalibrationRecord> calibrations,
        List<string> errors)
    {
        if (!IsPhysicalTarget(target))
            return;

        if (string.IsNullOrWhiteSpace(target.ParticleId))
            errors.Add($"externalTargetTablePath: physical target '{target.Label}' must declare particleId.");
        if (string.IsNullOrWhiteSpace(target.PhysicalObservableType))
            errors.Add($"externalTargetTablePath: physical target '{target.Label}' must declare physicalObservableType.");
        if (string.IsNullOrWhiteSpace(target.UnitFamily))
            errors.Add($"externalTargetTablePath: physical target '{target.Label}' must declare unitFamily.");
        ValidatePhysicalTargetEvidence(target, errors);

        if (mappings.Count == 0)
        {
            errors.Add(
                $"externalTargetTablePath: physical target '{target.Label}' cannot be compared because no physical observable mapping table is configured.");
            return;
        }

        var candidates = mappings.Where(m =>
            string.Equals(m.ParticleId, target.ParticleId, StringComparison.Ordinal) &&
            string.Equals(m.PhysicalObservableType, target.PhysicalObservableType, StringComparison.Ordinal) &&
            string.Equals(m.TargetPhysicalObservableId, target.ObservableId, StringComparison.Ordinal) &&
            (string.IsNullOrWhiteSpace(target.UnitFamily) ||
             string.Equals(m.UnitFamily, target.UnitFamily, StringComparison.Ordinal)) &&
            (string.IsNullOrWhiteSpace(target.TargetEnvironmentId) ||
             string.IsNullOrWhiteSpace(m.RequiredEnvironmentId) ||
             string.Equals(m.RequiredEnvironmentId, target.TargetEnvironmentId, StringComparison.Ordinal)) &&
            (string.IsNullOrWhiteSpace(target.TargetEnvironmentTier) ||
             string.IsNullOrWhiteSpace(m.RequiredEnvironmentTier) ||
             string.Equals(m.RequiredEnvironmentTier, target.TargetEnvironmentTier, StringComparison.Ordinal)))
            .ToList();

        if (candidates.Count == 0)
        {
            errors.Add(
                $"externalTargetTablePath: physical target '{target.Label}' has no mapping for particle '{target.ParticleId}', observable type '{target.PhysicalObservableType}', and computed observable '{target.ObservableId}'.");
            return;
        }

        var validated = candidates.FirstOrDefault(m => string.Equals(m.Status, "validated", StringComparison.OrdinalIgnoreCase));
        if (validated is not null)
        {
            ValidatePhysicalTargetCalibration(target, validated, calibrations, errors);
            return;
        }

        var blocked = candidates.FirstOrDefault(m => string.Equals(m.Status, "blocked", StringComparison.OrdinalIgnoreCase));
        if (blocked is not null)
        {
            var closure = blocked.ClosureRequirements.Count == 0
                ? "No closure requirement was provided."
                : string.Join(" ", blocked.ClosureRequirements);
            errors.Add(
                $"externalTargetTablePath: physical target '{target.Label}' is blocked by mapping '{blocked.MappingId}'. Closure requirement: {closure}");
            return;
        }

        errors.Add(
            $"externalTargetTablePath: physical target '{target.Label}' has only provisional mappings; comparison requires a validated mapping.");
    }

    private static bool IsPhysicalTarget(ExternalTarget target)
    {
        return !string.IsNullOrWhiteSpace(target.ParticleId) ||
               !string.IsNullOrWhiteSpace(target.PhysicalObservableType) ||
               !string.IsNullOrWhiteSpace(target.UnitFamily) ||
               string.Equals(target.BenchmarkClass, "physical-observable", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(target.EvidenceTier, "physical-prediction", StringComparison.OrdinalIgnoreCase);
    }

    private static void ValidatePhysicalTargetEvidence(ExternalTarget target, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(target.Citation))
            errors.Add($"externalTargetTablePath: physical target '{target.Label}' must declare citation.");
        if (string.IsNullOrWhiteSpace(target.SourceUrl))
            errors.Add($"externalTargetTablePath: physical target '{target.Label}' must declare sourceUrl.");
        if (string.IsNullOrWhiteSpace(target.RetrievedAt))
        {
            errors.Add($"externalTargetTablePath: physical target '{target.Label}' must declare retrievedAt.");
        }
        else if (!DateOnly.TryParse(target.RetrievedAt, out _))
        {
            errors.Add($"externalTargetTablePath: physical target '{target.Label}' has invalid retrievedAt '{target.RetrievedAt}'; expected yyyy-MM-dd.");
        }

        if (string.IsNullOrWhiteSpace(target.Unit))
            errors.Add($"externalTargetTablePath: physical target '{target.Label}' must declare unit.");
    }

    private static bool IsKnownMappingStatus(string status)
        => string.Equals(status, "validated", StringComparison.OrdinalIgnoreCase) ||
           string.Equals(status, "provisional", StringComparison.OrdinalIgnoreCase) ||
           string.Equals(status, "blocked", StringComparison.OrdinalIgnoreCase);

    private static IReadOnlyList<PhysicalCalibrationRecord> LoadPhysicalCalibrations(
        Phase5CampaignSpec spec,
        string specDir,
        string repoRoot,
        List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(spec.PhysicalCalibrationPath))
            return Array.Empty<PhysicalCalibrationRecord>();

        var resolved = ResolvePath(spec.PhysicalCalibrationPath, specDir);
        if (!File.Exists(resolved))
            return Array.Empty<PhysicalCalibrationRecord>();

        ValidateSchema(
            File.ReadAllText(resolved),
            Path.Combine(repoRoot, "schemas", "physical_calibration.schema.json"),
            "physicalCalibrationPath:physical_calibration.schema.json",
            errors);

        try
        {
            var table = GuJsonDefaults.Deserialize<PhysicalCalibrationTable>(File.ReadAllText(resolved));
            if (table is null)
            {
                errors.Add("physicalCalibrationPath: failed to deserialize PhysicalCalibrationTable.");
                return Array.Empty<PhysicalCalibrationRecord>();
            }

            foreach (var calibration in table.Calibrations)
            {
                if (string.IsNullOrWhiteSpace(calibration.CalibrationId))
                    errors.Add("physicalCalibrationPath: every calibration must declare calibrationId.");
                if (string.IsNullOrWhiteSpace(calibration.MappingId))
                    errors.Add($"physicalCalibrationPath: calibration '{calibration.CalibrationId}' must declare mappingId.");
                if (string.IsNullOrWhiteSpace(calibration.SourceComputedObservableId))
                    errors.Add($"physicalCalibrationPath: calibration '{calibration.CalibrationId}' must declare sourceComputedObservableId.");
                if (string.IsNullOrWhiteSpace(calibration.TargetUnitFamily))
                    errors.Add($"physicalCalibrationPath: calibration '{calibration.CalibrationId}' must declare targetUnitFamily.");
                if (string.IsNullOrWhiteSpace(calibration.TargetUnit))
                    errors.Add($"physicalCalibrationPath: calibration '{calibration.CalibrationId}' must declare targetUnit.");
                if (!IsKnownMappingStatus(calibration.Status))
                    errors.Add($"physicalCalibrationPath: calibration '{calibration.CalibrationId}' has unknown status '{calibration.Status}'.");
                if (calibration.ScaleFactor <= 0)
                    errors.Add($"physicalCalibrationPath: calibration '{calibration.CalibrationId}' must have scaleFactor > 0.");
                if (calibration.ScaleUncertainty < 0)
                    errors.Add($"physicalCalibrationPath: calibration '{calibration.CalibrationId}' must have scaleUncertainty >= 0.");
                if (calibration.Assumptions.Count == 0)
                    errors.Add($"physicalCalibrationPath: calibration '{calibration.CalibrationId}' must declare at least one assumption.");
                if (string.Equals(calibration.Status, "blocked", StringComparison.OrdinalIgnoreCase) &&
                    calibration.ClosureRequirements.Count == 0)
                {
                    errors.Add($"physicalCalibrationPath: blocked calibration '{calibration.CalibrationId}' must declare closureRequirements.");
                }
            }

            return table.Calibrations;
        }
        catch (Exception ex)
        {
            errors.Add($"physicalCalibrationPath: failed to validate calibrations ({ex.Message}).");
            return Array.Empty<PhysicalCalibrationRecord>();
        }
    }

    private static void ValidatePhysicalTargetCalibration(
        ExternalTarget target,
        PhysicalObservableMapping mapping,
        IReadOnlyList<PhysicalCalibrationRecord> calibrations,
        List<string> errors)
    {
        if (calibrations.Count == 0)
        {
            errors.Add(
                $"externalTargetTablePath: physical target '{target.Label}' cannot be compared because no physical calibration table is configured for validated mapping '{mapping.MappingId}'.");
            return;
        }

        var candidates = calibrations.Where(c =>
            string.Equals(c.MappingId, mapping.MappingId, StringComparison.Ordinal) &&
            string.Equals(c.SourceComputedObservableId, mapping.SourceComputedObservableId, StringComparison.Ordinal) &&
            (string.IsNullOrWhiteSpace(target.UnitFamily) ||
             string.Equals(c.TargetUnitFamily, target.UnitFamily, StringComparison.Ordinal)))
            .ToList();

        if (candidates.Count == 0)
        {
            errors.Add(
                $"externalTargetTablePath: physical target '{target.Label}' has no calibration for mapping '{mapping.MappingId}' and unit family '{target.UnitFamily}'.");
            return;
        }

        if (candidates.Any(c => string.Equals(c.Status, "validated", StringComparison.OrdinalIgnoreCase)))
            return;

        var blocked = candidates.FirstOrDefault(c => string.Equals(c.Status, "blocked", StringComparison.OrdinalIgnoreCase));
        if (blocked is not null)
        {
            var closure = blocked.ClosureRequirements.Count == 0
                ? "No closure requirement was provided."
                : string.Join(" ", blocked.ClosureRequirements);
            errors.Add(
                $"externalTargetTablePath: physical target '{target.Label}' is blocked by calibration '{blocked.CalibrationId}'. Closure requirement: {closure}");
            return;
        }

        errors.Add(
            $"externalTargetTablePath: physical target '{target.Label}' has only provisional calibrations; comparison requires a validated calibration.");
    }

    private static IReadOnlyList<TargetCoverageBlockerRecord> LoadTargetCoverageBlockers(
        Phase5CampaignSpec spec,
        string specDir,
        List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(spec.TargetCoverageBlockersPath))
            return Array.Empty<TargetCoverageBlockerRecord>();

        var resolved = ResolvePath(spec.TargetCoverageBlockersPath, specDir);
        if (!File.Exists(resolved))
            return Array.Empty<TargetCoverageBlockerRecord>();

        try
        {
            var table = GuJsonDefaults.Deserialize<TargetCoverageBlockerTable>(File.ReadAllText(resolved));
            if (table is null)
            {
                errors.Add("targetCoverageBlockersPath: failed to deserialize TargetCoverageBlockerTable.");
                return Array.Empty<TargetCoverageBlockerRecord>();
            }

            foreach (var blocker in table.Blockers)
            {
                if (string.IsNullOrWhiteSpace(blocker.BlockerId))
                    errors.Add("targetCoverageBlockersPath: every blocker must declare blockerId.");
                if (string.IsNullOrWhiteSpace(blocker.ObservableId))
                    errors.Add($"targetCoverageBlockersPath: blocker '{blocker.BlockerId}' must declare observableId.");
                if (string.IsNullOrWhiteSpace(blocker.BlockerReason))
                    errors.Add($"targetCoverageBlockersPath: blocker '{blocker.BlockerId}' must declare blockerReason.");
                if (string.IsNullOrWhiteSpace(blocker.ClosureRequirement))
                    errors.Add($"targetCoverageBlockersPath: blocker '{blocker.BlockerId}' must declare closureRequirement.");
            }

            return table.Blockers;
        }
        catch (Exception ex)
        {
            errors.Add($"targetCoverageBlockersPath: failed to validate blockers ({ex.Message}).");
            return Array.Empty<TargetCoverageBlockerRecord>();
        }
    }

    private static bool HasTargetCoverageBlocker(
        ExternalTarget target,
        IReadOnlyList<TargetCoverageBlockerRecord> blockers)
    {
        return blockers.Any(blocker =>
            string.Equals(blocker.ObservableId, target.ObservableId, StringComparison.Ordinal) &&
            (string.IsNullOrWhiteSpace(blocker.TargetLabel) ||
             string.Equals(blocker.TargetLabel, target.Label, StringComparison.Ordinal)) &&
            (string.IsNullOrWhiteSpace(blocker.TargetEnvironmentId) ||
             string.Equals(blocker.TargetEnvironmentId, target.TargetEnvironmentId, StringComparison.Ordinal)) &&
            (string.IsNullOrWhiteSpace(blocker.TargetEnvironmentTier) ||
             string.Equals(blocker.TargetEnvironmentTier, target.TargetEnvironmentTier, StringComparison.Ordinal)));
    }

    private static IReadOnlyDictionary<string, string> LoadEnvironmentTierMap(
        Phase5CampaignSpec spec,
        string specDir)
    {
        var map = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var path in spec.EnvironmentRecordPaths)
        {
            var resolved = ResolvePath(path, specDir);
            if (!File.Exists(resolved))
                continue;

            try
            {
                var record = GuJsonDefaults.Deserialize<EnvironmentRecord>(File.ReadAllText(resolved));
                if (record is not null)
                    map[record.EnvironmentId] = record.GeometryTier;
            }
            catch
            {
                // Detailed environment parse errors are reported by ValidateEnvironmentEvidence.
            }
        }

        return map;
    }

    private static bool TargetSelectorsMatch(
        QuantitativeObservableRecord observable,
        ExternalTarget target,
        IReadOnlyDictionary<string, string> environmentTiers)
    {
        if (!string.Equals(observable.ObservableId, target.ObservableId, StringComparison.Ordinal))
            return false;

        if (!string.IsNullOrWhiteSpace(target.TargetEnvironmentId) &&
            !string.Equals(observable.EnvironmentId, target.TargetEnvironmentId, StringComparison.Ordinal))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(target.TargetEnvironmentTier))
        {
            environmentTiers.TryGetValue(observable.EnvironmentId, out var tier);
            if (!string.Equals(tier, target.TargetEnvironmentTier, StringComparison.Ordinal))
                return false;
        }

        return true;
    }

    private static void ValidateObservableClassifications(
        Phase5CampaignSpec spec,
        string specDir,
        string repoRoot,
        List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(spec.ObservableClassificationsPath))
            return;

        var resolved = ResolvePath(spec.ObservableClassificationsPath, specDir);
        if (!File.Exists(resolved))
            return;

        ValidateSchema(
            File.ReadAllText(resolved),
            Path.Combine(repoRoot, "schemas", "observable_classification.schema.json"),
            "observableClassificationsPath:observable_classification.schema.json",
            errors);

        try
        {
            var table = GuJsonDefaults.Deserialize<ObservableClassificationTable>(File.ReadAllText(resolved));
            if (table is null)
            {
                errors.Add("observableClassificationsPath: failed to deserialize ObservableClassificationTable.");
                return;
            }

            var knownClasses = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "internal-control",
                "internal-benchmark",
                "external-lattice-benchmark",
                "physical-candidate",
                "physical-observable",
            };

            foreach (var classification in table.Classifications)
            {
                if (string.IsNullOrWhiteSpace(classification.ObservableId))
                    errors.Add("observableClassificationsPath: every classification must declare observableId.");
                if (!knownClasses.Contains(classification.Classification))
                    errors.Add($"observableClassificationsPath: observable '{classification.ObservableId}' has unknown classification '{classification.Classification}'.");
                if (string.IsNullOrWhiteSpace(classification.Rationale))
                    errors.Add($"observableClassificationsPath: observable '{classification.ObservableId}' must declare rationale.");
                if (classification.PhysicalClaimAllowed &&
                    !string.Equals(classification.Classification, "physical-observable", StringComparison.OrdinalIgnoreCase))
                {
                    errors.Add($"observableClassificationsPath: observable '{classification.ObservableId}' cannot allow physical claims unless classified as physical-observable.");
                }
            }

            var observablesPath = ResolvePath(spec.ObservablesPath, specDir);
            if (!File.Exists(observablesPath))
                return;

            var observables = GuJsonDefaults.Deserialize<List<QuantitativeObservableRecord>>(File.ReadAllText(observablesPath));
            if (observables is null)
                return;

            var classifiedIds = table.Classifications
                .Select(c => c.ObservableId)
                .ToHashSet(StringComparer.Ordinal);
            foreach (var observableId in observables.Select(o => o.ObservableId).Distinct(StringComparer.Ordinal))
            {
                if (!classifiedIds.Contains(observableId))
                    errors.Add($"observableClassificationsPath: observable '{observableId}' has no explicit classification.");
            }
        }
        catch (Exception ex)
        {
            errors.Add($"observableClassificationsPath: failed to validate classifications ({ex.Message}).");
        }
    }

    private static void ValidateSidecarSchemas(
        Phase5CampaignSpec spec,
        string specDir,
        string repoRoot,
        List<string> errors)
    {
        ValidateSidecarSchema(spec.ObservationChainPath, "observationChainPath", "observation_chain.schema.json", specDir, repoRoot, errors);
        ValidateSidecarSchema(spec.EnvironmentVariancePath, "environmentVariancePath", "environment_variance.schema.json", specDir, repoRoot, errors);
        ValidateSidecarSchema(spec.RepresentationContentPath, "representationContentPath", "representation_content.schema.json", specDir, repoRoot, errors);
        ValidateSidecarSchema(spec.CouplingConsistencyPath, "couplingConsistencyPath", "coupling_consistency.schema.json", specDir, repoRoot, errors);
    }

    private static void ValidateSidecarSchema(
        string? relativePath,
        string fieldName,
        string schemaFileName,
        string specDir,
        string repoRoot,
        List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return;

        var resolved = ResolvePath(relativePath, specDir);
        if (!File.Exists(resolved))
            return;

        ValidateSchema(
            File.ReadAllText(resolved),
            Path.Combine(repoRoot, "schemas", schemaFileName),
            $"{fieldName}:{schemaFileName}",
            errors);
    }

    private static void ValidateSchema(
        string json,
        string schemaPath,
        string label,
        List<string> errors)
    {
        if (!File.Exists(schemaPath))
        {
            errors.Add($"{label}: schema file not found at \"{schemaPath}\".");
            return;
        }

        var result = SchemaValidator.ValidateWithSchemaFile(json, schemaPath);
        if (!result.IsValid)
        {
            foreach (var error in result.Errors)
                errors.Add($"{label}: {error}");
        }
    }

    private static string ResolvePath(string path, string specDir)
        => Path.IsPathRooted(path) ? path : Path.GetFullPath(Path.Combine(specDir, path));

    private static string FindRepoRoot(string startDir)
    {
        foreach (var candidate in new[]
                 {
                     Path.GetFullPath(startDir),
                     Directory.GetCurrentDirectory(),
                     AppContext.BaseDirectory,
                 })
        {
            var current = new DirectoryInfo(Path.GetFullPath(candidate));
            while (current is not null)
            {
                if (Directory.Exists(Path.Combine(current.FullName, "schemas")) &&
                    Directory.Exists(Path.Combine(current.FullName, "src")))
                {
                    return current.FullName;
                }

                current = current.Parent;
            }
        }

        throw new DirectoryNotFoundException($"Could not locate repository root from \"{startDir}\".");
    }

    /// <summary>
    /// Returns true if <paramref name="version"/> is semantically >= <paramref name="minimum"/>.
    /// Falls back to string comparison if parsing fails.
    /// </summary>
    private static bool IsVersionAtLeast(string version, string minimum)
    {
        if (TryParseVersion(version, out var v) && TryParseVersion(minimum, out var m))
            return v >= m;
        // Fallback: lexicographic comparison
        return string.Compare(version, minimum, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static bool TryParseVersion(string s, out Version version)
    {
        // Version.Parse requires at least Major.Minor — pad if needed
        if (!s.Contains('.'))
            s = s + ".0";
        return Version.TryParse(s, out version!);
    }
}
