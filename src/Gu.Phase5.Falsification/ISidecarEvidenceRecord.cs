namespace Gu.Phase5.Falsification;

/// <summary>
/// Common provenance/origin contract for sidecar evidence records.
/// </summary>
public interface ISidecarEvidenceRecord
{
    /// <summary>Evidence origin classification: upstream-sourced, bridge-derived, or heuristic.</summary>
    string Origin { get; }

    /// <summary>Artifact references that support this record.</summary>
    IReadOnlyList<string>? SourceArtifactRefs { get; }
}
