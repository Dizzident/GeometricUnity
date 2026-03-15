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
            // Must have at least 2 environment records (toy + structured)
            int envCount = spec.EnvironmentRecordPaths?.Count ?? 0;
            if (envCount < 2)
                errors.Add($"requireReferenceSidecars: at least 2 environment records required (toy + structured); found {envCount}.");

            // schemaVersion must be >= 1.1.0
            if (!IsVersionAtLeast(spec.SchemaVersion, MinReferenceSchemaVersion))
                errors.Add($"requireReferenceSidecars: schemaVersion must be >= {MinReferenceSchemaVersion}; found \"{spec.SchemaVersion}\".");

            // All four sidecar paths must be present and exist
            CheckSidecarPath(spec.ObservationChainPath, "observationChainPath", specDir, errors);
            CheckSidecarPath(spec.EnvironmentVariancePath, "environmentVariancePath", specDir, errors);
            CheckSidecarPath(spec.RepresentationContentPath, "representationContentPath", specDir, errors);
            CheckSidecarPath(spec.CouplingConsistencyPath, "couplingConsistencyPath", specDir, errors);
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
