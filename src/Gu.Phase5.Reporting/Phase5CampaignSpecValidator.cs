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

        ValidateEnvironmentEvidence(spec, specDir, requireReferenceSidecars, errors);
        ValidateTargetTable(spec, specDir, errors);
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
        List<string> errors)
    {
        var resolved = ResolvePath(spec.ExternalTargetTablePath, specDir);
        if (!File.Exists(resolved))
            return;

        try
        {
            var table = GuJsonDefaults.Deserialize<ExternalTargetTable>(File.ReadAllText(resolved));
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
        }
        catch (Exception ex)
        {
            errors.Add($"externalTargetTablePath: failed to validate target table ({ex.Message}).");
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
