using Gu.Phase3.Backgrounds;

namespace Gu.Phase4.Chirality;

/// <summary>
/// Filters BackgroundRecord collections by replay tier for Phase IV fermionic spectral solves.
///
/// Per ARCH_P4.md §8.1, Phase IV should only compute fermionic modes on backgrounds
/// that have achieved replay tier >= R2 (sufficient physical reproducibility).
///
/// Tier ordering: R0 < R1 < R2 < R3.
/// </summary>
public static class BackgroundReplayFilter
{
    private static readonly Dictionary<string, int> TierOrder = new()
    {
        ["R0"] = 0,
        ["R1"] = 1,
        ["R2"] = 2,
        ["R3"] = 3,
    };

    /// <summary>
    /// Filter backgrounds to those with ReplayTierAchieved >= minimumTier.
    /// Returns an empty list if none qualify.
    /// </summary>
    public static IReadOnlyList<BackgroundRecord> FilterByMinimumTier(
        IEnumerable<BackgroundRecord> backgrounds,
        string minimumTier = "R2")
    {
        ArgumentNullException.ThrowIfNull(backgrounds);
        if (!TierOrder.TryGetValue(minimumTier, out int minOrder))
            throw new ArgumentException($"Unknown replay tier: '{minimumTier}'. Valid values: R0, R1, R2, R3.", nameof(minimumTier));

        return backgrounds
            .Where(b => TierOrder.TryGetValue(b.ReplayTierAchieved, out int order) && order >= minOrder)
            .ToList();
    }

    /// <summary>
    /// Returns true if the background meets the minimum replay tier.
    /// </summary>
    public static bool MeetsTier(BackgroundRecord background, string minimumTier = "R2")
    {
        ArgumentNullException.ThrowIfNull(background);
        if (!TierOrder.TryGetValue(minimumTier, out int minOrder))
            return false;
        return TierOrder.TryGetValue(background.ReplayTierAchieved, out int order) && order >= minOrder;
    }
}
