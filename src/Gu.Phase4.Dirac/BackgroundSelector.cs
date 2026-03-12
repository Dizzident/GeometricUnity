using Gu.Phase3.Backgrounds;

namespace Gu.Phase4.Dirac;

/// <summary>
/// Filters a Phase III background atlas to find backgrounds acceptable
/// for fermionic spectral analysis.
///
/// Selection criteria (M38 spec):
///   1. ReplayTierAchieved >= minReplayTier  (lexicographic: R0 < R1 < R2 < R3)
///   2. ResidualNorm <= maxResidualNorm
///   3. StationarityNorm <= maxStationarityNorm
///   4. AdmissibilityLevel is not Rejected
/// </summary>
public sealed class BackgroundSelector
{
    private static readonly string[] TierOrder = { "R0", "R1", "R2", "R3" };

    /// <summary>
    /// Filter backgrounds by replay tier and quality thresholds.
    /// </summary>
    /// <param name="backgrounds">Input atlas.</param>
    /// <param name="minReplayTier">Minimum replay tier string ("R0".."R3").</param>
    /// <param name="maxResidualNorm">Maximum allowed residual norm.</param>
    /// <param name="maxStationarityNorm">Maximum allowed stationarity norm.</param>
    public IReadOnlyList<BackgroundRecord> Select(
        IEnumerable<BackgroundRecord> backgrounds,
        string minReplayTier = "R2",
        double maxResidualNorm = 1e-2,
        double maxStationarityNorm = 1e-2)
    {
        ArgumentNullException.ThrowIfNull(backgrounds);

        int minTierIndex = TierIndex(minReplayTier);

        var result = new List<BackgroundRecord>();
        foreach (var bg in backgrounds)
        {
            if (bg.AdmissibilityLevel == AdmissibilityLevel.Rejected)
                continue;

            int bgTier = TierIndex(bg.ReplayTierAchieved);
            if (bgTier < minTierIndex)
                continue;

            if (bg.ResidualNorm > maxResidualNorm)
                continue;

            if (bg.StationarityNorm > maxStationarityNorm)
                continue;

            result.Add(bg);
        }

        return result;
    }

    private static int TierIndex(string tier)
    {
        int idx = Array.IndexOf(TierOrder, tier);
        return idx < 0 ? 0 : idx;
    }
}
