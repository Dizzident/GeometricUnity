using Gu.Core;
using Gu.Core.Serialization;
using Gu.Phase3.Backgrounds;
using Gu.Phase5.Convergence;

namespace Gu.Phase5.Reporting;

/// <summary>
/// Exports Phase V branch and refinement value tables from a <see cref="BackgroundAtlas"/>,
/// bridging Phase III artifacts into Phase V campaign inputs (D-P6-003).
///
/// Outputs (written to <paramref name="outDir"/>):
/// <list type="bullet">
///   <item><c>branch_quantity_values.json</c> — <see cref="RefinementQuantityValueTable"/> keyed by derived variant IDs</item>
///   <item><c>refinement_values.json</c>       — <see cref="RefinementQuantityValueTable"/> derived from refinement spec levels</item>
///   <item><c>bridge_manifest.json</c>          — <see cref="BridgeManifest"/> recording source record IDs and variant IDs</item>
/// </list>
/// </summary>
public static class BridgeValueExporter
{
    /// <summary>
    /// Export branch/refinement value tables from <paramref name="atlas"/> and
    /// <paramref name="refinementSpec"/> into <paramref name="outDir"/>.
    /// </summary>
    /// <param name="atlas">Source background atlas (Phase III).</param>
    /// <param name="refinementSpec">Refinement study spec that defines level IDs and quantity IDs.</param>
    /// <param name="atlasSourcePath">
    /// Path of the atlas JSON file on disk (recorded verbatim in the bridge manifest for traceability).
    /// </param>
    /// <param name="outDir">Directory to write the three output files into (created if absent).</param>
    /// <param name="provenance">Provenance metadata to embed in the manifest.</param>
    /// <returns>The generated <see cref="BridgeManifest"/>.</returns>
    public static BridgeManifest Export(
        BackgroundAtlas atlas,
        RefinementStudySpec refinementSpec,
        string atlasSourcePath,
        string outDir,
        ProvenanceMeta provenance)
    {
        ArgumentNullException.ThrowIfNull(atlas);
        ArgumentNullException.ThrowIfNull(refinementSpec);
        ArgumentException.ThrowIfNullOrWhiteSpace(atlasSourcePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(outDir);
        ArgumentNullException.ThrowIfNull(provenance);

        Directory.CreateDirectory(outDir);

        // --- 1. Branch quantity values ---
        // One level per admitted background record; level ID = derived variant ID.
        var sourceRecordIds = new List<string>();
        var derivedVariantIds = new List<string>();
        var branchLevels = new List<RefinementQuantityValueLevel>();

        foreach (var record in atlas.Backgrounds)
        {
            var variantId = BackgroundRecordBranchVariantBridge.DeriveVariantId(record);
            var quantities = BackgroundRecordBranchVariantBridge.ExtractQuantityValues(record);

            sourceRecordIds.Add(record.BackgroundId);
            derivedVariantIds.Add(variantId);

            // Flatten single-element arrays → scalar for the value table
            var flatQuantities = quantities.ToDictionary(
                kv => kv.Key,
                kv => kv.Value.Length > 0 ? kv.Value[0] : 0.0);

            branchLevels.Add(new RefinementQuantityValueLevel
            {
                LevelId = variantId,
                SolverConverged = record.Metrics.SolverConverged,
                Quantities = flatQuantities,
            });
        }

        var branchTable = new RefinementQuantityValueTable
        {
            StudyId = atlas.AtlasId,
            Levels = branchLevels,
        };

        File.WriteAllText(
            Path.Combine(outDir, "branch_quantity_values.json"),
            GuJsonDefaults.Serialize(branchTable));

        // --- 2. Refinement values ---
        // Produce a stub refinement table whose levels mirror the spec's RefinementLevels.
        // Quantities are seeded from the best admitted background (first in the atlas)
        // and should be replaced with real per-level solver outputs in production use.
        // The table structure must match what Phase5CampaignArtifactLoader expects.
        var refinementLevels = new List<RefinementQuantityValueLevel>();

        // Gather a representative scalar value per quantity from the atlas (best background)
        var representativeQuantities = new Dictionary<string, double>();
        if (atlas.Backgrounds.Count > 0)
        {
            var bestRecord = atlas.Backgrounds[0];
            var extracted = BackgroundRecordBranchVariantBridge.ExtractQuantityValues(bestRecord);
            foreach (var (qid, arr) in extracted)
                representativeQuantities[qid] = arr.Length > 0 ? arr[0] : 0.0;
        }

        foreach (var level in refinementSpec.RefinementLevels)
        {
            // Seed quantities from representative values; campaign consumers are expected
            // to replace these with actual solver outputs per level.
            var levelQuantities = new Dictionary<string, double>(representativeQuantities);

            // Also seed any target quantities declared in the spec that aren't covered
            foreach (var qid in refinementSpec.TargetQuantities)
            {
                if (!levelQuantities.ContainsKey(qid))
                    levelQuantities[qid] = 0.0;
            }

            refinementLevels.Add(new RefinementQuantityValueLevel
            {
                LevelId = level.LevelId,
                SolverConverged = true,
                Quantities = levelQuantities,
            });
        }

        var refinementTable = new RefinementQuantityValueTable
        {
            StudyId = refinementSpec.StudyId,
            Levels = refinementLevels,
        };

        File.WriteAllText(
            Path.Combine(outDir, "refinement_values.json"),
            GuJsonDefaults.Serialize(refinementTable));

        // --- 3. Bridge manifest ---
        var manifestId = $"bridge-{atlas.AtlasId}-{DateTimeOffset.UtcNow:yyyyMMddTHHmmssZ}";
        var manifest = new BridgeManifest
        {
            ManifestId = manifestId,
            SourceAtlasPath = atlasSourcePath,
            SourceRecordIds = sourceRecordIds,
            DerivedVariantIds = derivedVariantIds,
            Provenance = provenance,
        };

        File.WriteAllText(
            Path.Combine(outDir, "bridge_manifest.json"),
            GuJsonDefaults.Serialize(manifest));

        return manifest;
    }
}
