using System.Text.Json.Serialization;

namespace Gu.Phase3.Properties;

/// <summary>
/// Aggregated property vector for a computed mode.
/// Each mode gets one of these, combining all extracted properties.
/// Must serialize and support comparison.
/// (See IMPLEMENTATION_PLAN_P3.md Section 6.5)
/// </summary>
public sealed class BosonPropertyVector
{
    /// <summary>Mode ID this vector describes.</summary>
    [JsonPropertyName("modeId")]
    public required string ModeId { get; init; }

    /// <summary>Background ID for provenance.</summary>
    [JsonPropertyName("backgroundId")]
    public required string BackgroundId { get; init; }

    /// <summary>Mass-like scale record.</summary>
    [JsonPropertyName("massLikeScale")]
    public required MassLikeScaleRecord MassLikeScale { get; init; }

    /// <summary>Polarization descriptor.</summary>
    [JsonPropertyName("polarization")]
    public required PolarizationDescriptor Polarization { get; init; }

    /// <summary>Symmetry descriptor.</summary>
    [JsonPropertyName("symmetry")]
    public required SymmetryDescriptor Symmetry { get; init; }

    /// <summary>Gauge leak score: || P_gauge v || / ||v||.</summary>
    [JsonPropertyName("gaugeLeakScore")]
    public required double GaugeLeakScore { get; init; }

    /// <summary>Multiplicity (cluster size).</summary>
    [JsonPropertyName("multiplicity")]
    public required int Multiplicity { get; init; }

    /// <summary>Stability score card (null if not yet computed).</summary>
    [JsonPropertyName("stability")]
    public StabilityScoreCard? Stability { get; init; }

    /// <summary>Interaction proxy records (empty if not yet computed).</summary>
    [JsonPropertyName("interactionProxies")]
    public IReadOnlyList<InteractionProxyRecord> InteractionProxies { get; init; } = Array.Empty<InteractionProxyRecord>();

    /// <summary>
    /// Compute distance between two property vectors for matching/comparison.
    /// Uses weighted aggregate of normalized differences.
    /// </summary>
    public static double Distance(BosonPropertyVector a, BosonPropertyVector b)
    {
        ArgumentNullException.ThrowIfNull(a);
        ArgumentNullException.ThrowIfNull(b);

        // Mass-like scale distance (relative)
        double massA = a.MassLikeScale.MassLikeScale;
        double massB = b.MassLikeScale.MassLikeScale;
        double massScale = System.Math.Max(System.Math.Abs(massA), System.Math.Max(System.Math.Abs(massB), 1e-30));
        double massDist = System.Math.Abs(massA - massB) / massScale;

        // Gauge leak difference
        double leakDist = System.Math.Abs(a.GaugeLeakScore - b.GaugeLeakScore);

        // Polarization distance (L1 between block energy fractions)
        double polDist = PolarizationDistance(a.Polarization, b.Polarization);

        // Multiplicity distance (normalized)
        double multDist = System.Math.Abs(a.Multiplicity - b.Multiplicity)
                        / System.Math.Max(a.Multiplicity, b.Multiplicity);

        // Weighted aggregate
        return 0.4 * massDist + 0.2 * leakDist + 0.3 * polDist + 0.1 * multDist;
    }

    private static double PolarizationDistance(PolarizationDescriptor a, PolarizationDescriptor b)
    {
        var allKeys = new HashSet<string>(a.BlockEnergyFractions.Keys);
        foreach (var k in b.BlockEnergyFractions.Keys) allKeys.Add(k);

        double sum = 0;
        foreach (var key in allKeys)
        {
            a.BlockEnergyFractions.TryGetValue(key, out double fa);
            b.BlockEnergyFractions.TryGetValue(key, out double fb);
            sum += System.Math.Abs(fa - fb);
        }
        return sum / 2.0; // Normalize to [0,1]
    }
}
