using System.Security.Cryptography;
using System.Text;
using Gu.Core;
using Gu.Phase3.Backgrounds;

namespace Gu.Phase5.Reporting;

/// <summary>
/// Bridges persisted Phase III <see cref="BackgroundRecord"/> artifacts into
/// Phase V branch-study quantity values.
///
/// Responsibility:
///   1. Loads a <see cref="BackgroundRecord"/> (or receives one directly).
///   2. Derives a stable branch-variant identity from the artifact set
///      (backgroundId + branchManifestId + geometryFingerprint).
///   3. Exports branch-study quantity values keyed by that variant ID,
///      suitable for use as the <c>branchPipelineExecutor</c> input in
///      <see cref="Phase5CampaignRunner"/>.
///
/// The bridge is the approved path for supplying Phase V branch-independence
/// studies from persisted Phase III background solves. Once available, it
/// replaces the analytic A0 family as the default evidence path.
/// </summary>
public static class BackgroundRecordBranchVariantBridge
{
    /// <summary>
    /// Derive a stable branch-variant ID from a <see cref="BackgroundRecord"/>.
    ///
    /// The variant ID is a deterministic short hash of:
    ///   backgroundId + "|" + branchManifestId + "|" + geometryFingerprint
    ///
    /// This ensures that re-loading the same artifacts always yields the same ID,
    /// and that IDs are unique across distinct artifact combinations.
    /// </summary>
    public static string DeriveVariantId(BackgroundRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);

        var raw = $"{record.BackgroundId}|{record.BranchManifestId}|{record.GeometryFingerprint}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        // Use first 8 bytes (16 hex chars) for a compact but collision-resistant ID
        var shortHash = Convert.ToHexString(hash[..8]).ToLowerInvariant();
        return $"bg-variant-{shortHash}";
    }

    /// <summary>
    /// Extract branch-study quantity values from a single <see cref="BackgroundRecord"/>.
    ///
    /// Extracted quantities:
    ///   - "residual-norm":       solver residual ||Upsilon_h(z*)||
    ///   - "stationarity-norm":   stationarity ||G_h(z*)||
    ///   - "objective-value":     objective I2_h(z*)
    ///   - "gauge-violation":     final gauge violation norm
    ///   - "solver-converged":    1.0 if converged, 0.0 otherwise
    ///   - "solver-iterations":   iteration count (as double)
    ///
    /// Each quantity is returned as a single-element array, matching the
    /// contract expected by <see cref="Phase5CampaignRunner"/>.
    /// </summary>
    public static IReadOnlyDictionary<string, double[]> ExtractQuantityValues(BackgroundRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);

        var m = record.Metrics;
        return new Dictionary<string, double[]>
        {
            ["residual-norm"]      = [record.ResidualNorm],
            ["stationarity-norm"]  = [record.StationarityNorm],
            ["objective-value"]    = [m.ObjectiveValue],
            ["gauge-violation"]    = [m.GaugeViolation],
            ["solver-converged"]   = [m.SolverConverged ? 1.0 : 0.0],
            ["solver-iterations"]  = [(double)m.SolverIterations],
        };
    }

    /// <summary>
    /// Build a branch-variant map from a collection of <see cref="BackgroundRecord"/> objects.
    ///
    /// Returns a dictionary from derived variant ID -> quantity value dictionary,
    /// ready to be consumed by a branch-independence study or campaign runner.
    ///
    /// Only admitted (non-rejected) records are included by default.
    /// Pass <paramref name="includeRejected"/> = true to include rejected records as well.
    /// </summary>
    public static IReadOnlyDictionary<string, IReadOnlyDictionary<string, double[]>> BuildVariantMap(
        IEnumerable<BackgroundRecord> records,
        bool includeRejected = false)
    {
        ArgumentNullException.ThrowIfNull(records);

        var map = new Dictionary<string, IReadOnlyDictionary<string, double[]>>();

        foreach (var record in records)
        {
            if (!includeRejected && record.AdmissibilityLevel == AdmissibilityLevel.Rejected)
                continue;

            var variantId = DeriveVariantId(record);
            map[variantId] = ExtractQuantityValues(record);
        }

        return map;
    }

    /// <summary>
    /// Build a branch-variant map from a <see cref="BackgroundAtlas"/>.
    ///
    /// Convenience overload that reads the admitted backgrounds list from the atlas.
    /// </summary>
    public static IReadOnlyDictionary<string, IReadOnlyDictionary<string, double[]>> BuildVariantMap(
        BackgroundAtlas atlas,
        bool includeRejected = false)
    {
        ArgumentNullException.ThrowIfNull(atlas);

        var records = includeRejected
            ? (IEnumerable<BackgroundRecord>)atlas.Backgrounds.Concat(atlas.RejectedBackgrounds)
            : atlas.Backgrounds;

        return BuildVariantMap(records, includeRejected: false);
    }

    /// <summary>
    /// Create a branch-pipeline executor function from a <see cref="BackgroundAtlas"/>.
    ///
    /// The returned delegate is compatible with the <c>branchPipelineExecutor</c>
    /// parameter of <see cref="Phase5CampaignRunner.Run"/>.
    ///
    /// <param name="atlas">The background atlas to source records from.</param>
    /// <param name="extraQuantities">
    ///   Optional additional quantities (keyed by variant ID then quantity ID).
    ///   Values are merged on top of the standard extracted quantities.
    /// </param>
    /// </summary>
    public static Func<string, IReadOnlyDictionary<string, double[]>> CreateBranchExecutor(
        BackgroundAtlas atlas,
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, double[]>>? extraQuantities = null)
    {
        ArgumentNullException.ThrowIfNull(atlas);

        var variantMap = BuildVariantMap(atlas);

        return variantId =>
        {
            if (!variantMap.TryGetValue(variantId, out var quantities))
                return new Dictionary<string, double[]>();

            if (extraQuantities is null || !extraQuantities.TryGetValue(variantId, out var extras))
                return quantities;

            // Merge: extra quantities override standard ones
            var merged = new Dictionary<string, double[]>(quantities);
            foreach (var (key, val) in extras)
                merged[key] = val;
            return merged;
        };
    }

    /// <summary>
    /// List all derived variant IDs for the admitted backgrounds in an atlas.
    /// Useful for populating <see cref="Gu.Phase5.BranchIndependence.BranchRobustnessStudySpec.BranchVariantIds"/>.
    /// </summary>
    public static IReadOnlyList<string> GetVariantIds(BackgroundAtlas atlas)
    {
        ArgumentNullException.ThrowIfNull(atlas);
        return atlas.Backgrounds
            .Select(DeriveVariantId)
            .ToList();
    }
}
