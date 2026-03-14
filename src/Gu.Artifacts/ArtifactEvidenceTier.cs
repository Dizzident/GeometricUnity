using System.Text.Json.Serialization;

namespace Gu.Artifacts;

/// <summary>
/// Describes how trustworthy a study artifact is as validation evidence.
/// G-006: stale checked-in artifacts must not be treated as proof of current
/// pipeline correctness.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ArtifactEvidenceTier
{
    /// <summary>
    /// Artifact was checked into the repository without a recorded regeneration
    /// command. Its contents may lag the current code. Must not be cited as
    /// Phase V validation evidence.
    /// </summary>
    StaleCheckedIn = 0,

    /// <summary>
    /// Artifact was regenerated from the current code tree by running a
    /// documented command sequence, but no cross-backend verification was
    /// performed.
    /// </summary>
    RegeneratedCpu = 1,

    /// <summary>
    /// Artifact was regenerated from the current code tree and verified against
    /// a second CPU reference run (numerical match within declared tolerance).
    /// </summary>
    RegeneratedVerifiedCpu = 2,

    /// <summary>
    /// Artifact was regenerated and verified across CPU and GPU backends within
    /// declared tolerance. Highest-confidence tier.
    /// </summary>
    CrossBackendVerified = 3,
}

/// <summary>
/// Extension helpers for ArtifactEvidenceTier.
/// </summary>
public static class ArtifactEvidenceTierExtensions
{
    /// <summary>
    /// Returns true if the tier represents evidence that was regenerated from
    /// the current code (i.e. not stale).
    /// </summary>
    public static bool IsRegeneratedFromCurrentCode(this ArtifactEvidenceTier tier) =>
        tier >= ArtifactEvidenceTier.RegeneratedCpu;

    /// <summary>
    /// Returns true if the tier is acceptable as Phase V validation evidence.
    /// StaleCheckedIn is not acceptable.
    /// </summary>
    public static bool IsAcceptableAsEvidence(this ArtifactEvidenceTier tier) =>
        tier >= ArtifactEvidenceTier.RegeneratedCpu;

    /// <summary>
    /// Returns a short human-readable label for the tier.
    /// </summary>
    public static string Label(this ArtifactEvidenceTier tier) => tier switch
    {
        ArtifactEvidenceTier.StaleCheckedIn        => "stale-checked-in",
        ArtifactEvidenceTier.RegeneratedCpu        => "regenerated-cpu",
        ArtifactEvidenceTier.RegeneratedVerifiedCpu => "regenerated-verified-cpu",
        ArtifactEvidenceTier.CrossBackendVerified   => "cross-backend-verified",
        _ => "unknown",
    };
}
