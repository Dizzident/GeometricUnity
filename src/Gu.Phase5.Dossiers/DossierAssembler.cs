using Gu.Artifacts;
using Gu.Core;

namespace Gu.Phase5.Dossiers;

/// <summary>
/// Assembles a ValidationDossier from a list of StudyManifests.
///
/// G-006 enforcement: any study with a missing or StaleCheckedIn
/// reproducibility bundle causes the overall dossier to be marked
/// non-acceptable as Phase V evidence. The assembler records exactly which
/// studies are stale and why.
/// </summary>
public static class DossierAssembler
{
    /// <summary>
    /// Assembles a ValidationDossier from the supplied study manifests.
    /// Enforces G-006: stale artifacts are identified and the overall evidence
    /// tier is set to the minimum tier across all studies.
    /// </summary>
    /// <param name="dossierId">Unique ID for the produced dossier.</param>
    /// <param name="title">Human-readable title.</param>
    /// <param name="studies">Study manifests to include.</param>
    /// <param name="provenance">Optional provenance metadata.</param>
    /// <returns>Assembled ValidationDossier.</returns>
    public static ValidationDossier Assemble(
        string dossierId,
        string title,
        IReadOnlyList<StudyManifest> studies,
        ProvenanceMeta? provenance = null)
    {
        ArgumentNullException.ThrowIfNull(dossierId);
        ArgumentNullException.ThrowIfNull(title);
        ArgumentNullException.ThrowIfNull(studies);

        var notes = new List<string>();
        var staleIds = new List<string>();

        // Determine overall evidence tier: minimum across all studies.
        // An empty dossier gets StaleCheckedIn by default (nothing to show).
        var overallTier = studies.Count == 0
            ? ArtifactEvidenceTier.StaleCheckedIn
            : (ArtifactEvidenceTier)studies.Min(s => (int)s.EffectiveEvidenceTier);

        foreach (var study in studies)
        {
            var tier = study.EffectiveEvidenceTier;
            if (!tier.IsAcceptableAsEvidence())
            {
                staleIds.Add(study.StudyId);
                notes.Add(
                    $"G-006: study '{study.StudyId}' has tier '{tier.Label()}' — " +
                    $"stale or missing reproducibility bundle. " +
                    $"Cannot be cited as Phase V validation evidence. " +
                    $"Regenerate using the documented command sequence and attach a ReproducibilityBundle.");
            }
            else
            {
                var rev = study.Reproducibility?.CodeRevision ?? "unknown";
                notes.Add(
                    $"study '{study.StudyId}' tier='{tier.Label()}' revision='{rev}' — acceptable.");
            }
        }

        bool acceptable = overallTier.IsAcceptableAsEvidence() && staleIds.Count == 0;

        string verdict;
        if (studies.Count == 0)
        {
            verdict = "No studies provided. Dossier is empty and not acceptable as Phase V evidence.";
        }
        else if (staleIds.Count == 0)
        {
            verdict = $"All {studies.Count} studies regenerated from current code. " +
                      $"Overall tier: {overallTier.Label()}. Acceptable as Phase V validation evidence.";
        }
        else
        {
            verdict = $"G-006: {staleIds.Count} of {studies.Count} studies are stale or unverified " +
                      $"(tier StaleCheckedIn). Overall tier: {overallTier.Label()}. " +
                      $"NOT acceptable as Phase V validation evidence until all studies are regenerated. " +
                      $"Stale studies: [{string.Join(", ", staleIds)}].";
        }

        return new ValidationDossier
        {
            DossierId = dossierId,
            Title = title,
            Studies = studies,
            OverallEvidenceTier = overallTier,
            EvidenceVerdict = verdict,
            IsAcceptableAsEvidence = acceptable,
            StaleStudyIds = staleIds,
            AssemblyNotes = notes,
            AssembledAt = DateTimeOffset.UtcNow,
            Provenance = provenance,
        };
    }

    /// <summary>
    /// Builds a DossierIndex from a collection of (path, dossier) pairs.
    /// </summary>
    /// <param name="indexId">Unique ID for the produced index.</param>
    /// <param name="entries">Pairs of (relative-path, dossier).</param>
    /// <returns>DossierIndex.</returns>
    public static DossierIndex BuildIndex(
        string indexId,
        IReadOnlyList<(string RelativePath, ValidationDossier Dossier)> entries)
    {
        ArgumentNullException.ThrowIfNull(indexId);
        ArgumentNullException.ThrowIfNull(entries);

        var indexEntries = entries.Select(e => new DossierIndexEntry
        {
            DossierId = e.Dossier.DossierId,
            Title = e.Dossier.Title,
            Path = e.RelativePath,
            OverallEvidenceTier = e.Dossier.OverallEvidenceTier,
            IsAcceptableAsEvidence = e.Dossier.IsAcceptableAsEvidence,
            StaleStudyCount = e.Dossier.StaleStudyIds.Count,
            AssembledAt = e.Dossier.AssembledAt,
        }).ToList();

        return new DossierIndex
        {
            IndexId = indexId,
            Entries = indexEntries,
            UpdatedAt = DateTimeOffset.UtcNow,
        };
    }
}
