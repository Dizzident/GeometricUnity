namespace Gu.Artifacts;

/// <summary>
/// Reproducibility tiers for replay (Section 20.3).
/// R0 = archival only, R1 = structural replay, R2 = numerical replay, R3 = cross-backend replay.
/// </summary>
public static class ReplayTiers
{
    /// <summary>Archival only: artifacts stored but no replay guarantee.</summary>
    public const string R0 = "R0";

    /// <summary>Structural replay: same branch, geometry, conventions - structure matches.</summary>
    public const string R1 = "R1";

    /// <summary>Numerical replay: same backend reproduces numerical results within tolerance.</summary>
    public const string R2 = "R2";

    /// <summary>Cross-backend replay: different backends produce matching results within tolerance.</summary>
    public const string R3 = "R3";

    /// <summary>
    /// Returns the numeric level for a tier string (R0=0, R1=1, R2=2, R3=3).
    /// </summary>
    public static int TierLevel(string tier) => tier switch
    {
        R0 => 0,
        R1 => 1,
        R2 => 2,
        R3 => 3,
        _ => throw new ArgumentException($"Unknown replay tier: {tier}"),
    };

    /// <summary>
    /// Whether the given tier meets or exceeds the required tier.
    /// </summary>
    public static bool MeetsTier(string actual, string required) =>
        TierLevel(actual) >= TierLevel(required);
}
