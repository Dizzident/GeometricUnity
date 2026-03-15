using Gu.Core;
using Gu.Core.Serialization;
using Gu.Phase3.Backgrounds;
using Gu.Phase5.Convergence;

namespace Gu.Phase5.Reporting;

/// <summary>
/// Exports a direct solver-backed refinement values table from executed background records.
/// Each refinement level is explicitly bound to one persisted background solve.
/// </summary>
public static class DirectRefinementValueExporter
{
    public static RefinementEvidenceManifest Export(
        RefinementStudySpec refinementSpec,
        IReadOnlyDictionary<string, BackgroundRecord> recordsByLevelId,
        IReadOnlyDictionary<string, string> sourceArtifactRefsByLevelId,
        string outDir,
        ProvenanceMeta provenance,
        string? notes = null)
    {
        ArgumentNullException.ThrowIfNull(refinementSpec);
        ArgumentNullException.ThrowIfNull(recordsByLevelId);
        ArgumentNullException.ThrowIfNull(sourceArtifactRefsByLevelId);
        ArgumentException.ThrowIfNullOrWhiteSpace(outDir);
        ArgumentNullException.ThrowIfNull(provenance);

        Directory.CreateDirectory(outDir);

        var levels = new List<RefinementQuantityValueLevel>(refinementSpec.RefinementLevels.Count);
        var sourceRecordIds = new List<string>(refinementSpec.RefinementLevels.Count);
        var sourceArtifactRefs = new List<string>(refinementSpec.RefinementLevels.Count);

        foreach (var level in refinementSpec.RefinementLevels)
        {
            if (!recordsByLevelId.TryGetValue(level.LevelId, out var record))
            {
                throw new InvalidOperationException(
                    $"No direct solver-backed background record was supplied for refinement level '{level.LevelId}'.");
            }

            if (!sourceArtifactRefsByLevelId.TryGetValue(level.LevelId, out var artifactRef) ||
                string.IsNullOrWhiteSpace(artifactRef))
            {
                throw new InvalidOperationException(
                    $"No source artifact ref was supplied for refinement level '{level.LevelId}'.");
            }

            var extracted = BackgroundRecordBranchVariantBridge.ExtractQuantityValues(record);
            var flatQuantities = new Dictionary<string, double>(StringComparer.Ordinal);
            foreach (var quantityId in refinementSpec.TargetQuantities)
            {
                if (extracted.TryGetValue(quantityId, out var arr) && arr.Length > 0)
                    flatQuantities[quantityId] = arr[0];
            }

            levels.Add(new RefinementQuantityValueLevel
            {
                LevelId = level.LevelId,
                SolverConverged = record.Metrics.SolverConverged,
                Quantities = flatQuantities,
            });

            sourceRecordIds.Add(record.BackgroundId);
            sourceArtifactRefs.Add(artifactRef);
        }

        var table = new RefinementQuantityValueTable
        {
            StudyId = refinementSpec.StudyId,
            Levels = levels,
        };

        File.WriteAllText(
            Path.Combine(outDir, "refinement_values.json"),
            GuJsonDefaults.Serialize(table));

        var manifest = new RefinementEvidenceManifest
        {
            ManifestId = $"direct-refinement-{refinementSpec.StudyId}-{DateTimeOffset.UtcNow:yyyyMMddTHHmmssZ}",
            StudyId = refinementSpec.StudyId,
            EvidenceSource = "direct-solver-backed",
            SourceRecordIds = sourceRecordIds,
            SourceArtifactRefs = sourceArtifactRefs,
            Notes = notes,
            Provenance = provenance,
        };

        File.WriteAllText(
            Path.Combine(outDir, "refinement_evidence_manifest.json"),
            GuJsonDefaults.Serialize(manifest));

        return manifest;
    }
}
