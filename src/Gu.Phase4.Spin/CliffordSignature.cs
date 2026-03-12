using System.Text.Json.Serialization;

namespace Gu.Phase4.Spin;

/// <summary>
/// Clifford algebra signature Cl(p,q) where p = positive-definite directions,
/// q = negative-definite directions, and p+q = spacetime dimension.
/// For Riemannian Y: q=0. For Lorentzian: q=1 (branch variant).
/// Default for Phase IV: Cl(dimY, 0) — Riemannian.
/// </summary>
public sealed class CliffordSignature
{
    /// <summary>Number of positive-definite directions.</summary>
    [JsonPropertyName("positive")]
    public required int Positive { get; init; }

    /// <summary>Number of negative-definite directions.</summary>
    [JsonPropertyName("negative")]
    public required int Negative { get; init; }

    /// <summary>Total spacetime dimension = p + q.</summary>
    [JsonIgnore]
    public int Dimension => Positive + Negative;

    /// <summary>True if purely Riemannian (q == 0).</summary>
    [JsonIgnore]
    public bool IsRiemannian => Negative == 0;

    /// <summary>True if standard 4D Lorentzian (p=3, q=1 or p=1, q=3).</summary>
    [JsonIgnore]
    public bool IsLorentzian => (Positive == 3 && Negative == 1) || (Positive == 1 && Negative == 3);
}
