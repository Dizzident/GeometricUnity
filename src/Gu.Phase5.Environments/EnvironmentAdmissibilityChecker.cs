namespace Gu.Phase5.Environments;

/// <summary>
/// Checks environment admissibility for use in Phase V studies.
///
/// Checks performed:
///   "mesh-valid"       -- no degenerate faces (all volumes strictly positive)
///   "dimension-match"  -- baseDim + fiberDim = ambientDim (fiberDim = ambientDim - baseDim)
///   "connectivity"     -- edgeCount >= baseDim (mesh is minimally connected)
///   "orientation"      -- faceCount > 0 (mesh has faces for differential forms)
/// </summary>
public static class EnvironmentAdmissibilityChecker
{
    private const double DegeneracyThreshold = 1e-14;

    public static EnvironmentAdmissibilityReport Check(
        int baseDim, int ambientDim, int edgeCount, int faceCount,
        double[] volumes)
    {
        ArgumentNullException.ThrowIfNull(volumes);

        var checks = new List<AdmissibilityCheck>();

        // mesh-valid: all volumes strictly positive
        double minVolume = volumes.Length > 0 ? volumes.Min() : 0.0;
        bool meshValid = volumes.Length == 0 || minVolume > DegeneracyThreshold;
        checks.Add(new AdmissibilityCheck
        {
            CheckId = "mesh-valid",
            Description = "All face volumes are strictly positive (no degenerate faces)",
            Passed = meshValid,
            Value = minVolume,
            Threshold = DegeneracyThreshold,
        });

        // dimension-match: baseDim must be <= ambientDim
        bool dimMatch = baseDim >= 1 && ambientDim >= baseDim;
        checks.Add(new AdmissibilityCheck
        {
            CheckId = "dimension-match",
            Description = "Base dimension is at least 1 and does not exceed ambient dimension",
            Passed = dimMatch,
            Value = baseDim,
            Threshold = ambientDim,
        });

        // connectivity: edgeCount >= baseDim (minimally connected)
        bool connected = edgeCount >= baseDim;
        checks.Add(new AdmissibilityCheck
        {
            CheckId = "connectivity",
            Description = "Edge count is at least the base dimension (minimally connected)",
            Passed = connected,
            Value = edgeCount,
            Threshold = baseDim,
        });

        // orientation: faceCount > 0 for differential forms to be well-defined
        bool oriented = faceCount > 0;
        checks.Add(new AdmissibilityCheck
        {
            CheckId = "orientation",
            Description = "Face count is positive (required for differential forms)",
            Passed = oriented,
            Value = faceCount,
            Threshold = 1,
        });

        bool allPassed = checks.All(c => c.Passed);
        string level = allPassed ? "admissible" : "inadmissible";

        return new EnvironmentAdmissibilityReport
        {
            Level = level,
            Checks = checks,
            Passed = allPassed,
            Notes = allPassed ? null : BuildFailureNotes(checks),
        };
    }

    private static string BuildFailureNotes(IEnumerable<AdmissibilityCheck> checks)
    {
        var failed = checks.Where(c => !c.Passed).Select(c => c.CheckId).ToList();
        return $"Failed checks: {string.Join(", ", failed)}";
    }
}
