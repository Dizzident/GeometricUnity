using System.Text.Json.Serialization;
using Gu.Core;

namespace Gu.Phase3.Observables;

/// <summary>
/// The observed-space signature of a single mode.
///
/// Produced by applying the linearized observation map D_Obs to a mode vector.
/// Lives on X_h (base space), not Y_h.
/// </summary>
public sealed class ObservedModeSignature
{
    /// <summary>Mode ID this signature corresponds to.</summary>
    [JsonPropertyName("modeId")]
    public required string ModeId { get; init; }

    /// <summary>Background state ID.</summary>
    [JsonPropertyName("backgroundId")]
    public required string BackgroundId { get; init; }

    /// <summary>Observed coefficients on X_h.</summary>
    [JsonPropertyName("observedCoefficients")]
    public required double[] ObservedCoefficients { get; init; }

    /// <summary>Tensor signature of the observed field.</summary>
    [JsonPropertyName("observedSignature")]
    public required TensorSignature ObservedSignature { get; init; }

    /// <summary>Shape of the observed field.</summary>
    [JsonPropertyName("observedShape")]
    public required int[] ObservedShape { get; init; }

    /// <summary>How this signature was computed.</summary>
    [JsonPropertyName("linearizationMethod")]
    public required LinearizationMethod LinearizationMethod { get; init; }

    /// <summary>Low-order moments of the observed signature (optional).</summary>
    [JsonPropertyName("observedMoments")]
    public double[]? ObservedMoments { get; init; }

    /// <summary>
    /// Content hash for reproducibility checking.
    /// Computed from ObservedCoefficients for signature stability verification.
    /// </summary>
    [JsonPropertyName("signatureHash")]
    public string SignatureHash => ComputeHash();

    private string ComputeHash()
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var bytes = new byte[ObservedCoefficients.Length * sizeof(double)];
        System.Buffer.BlockCopy(ObservedCoefficients, 0, bytes, 0, bytes.Length);
        var hash = sha.ComputeHash(bytes);
        return Convert.ToHexStringLower(hash[..8]);
    }

    /// <summary>L2 norm of the observed signature.</summary>
    [JsonIgnore]
    public double L2Norm
    {
        get
        {
            double sum = 0;
            for (int i = 0; i < ObservedCoefficients.Length; i++)
                sum += ObservedCoefficients[i] * ObservedCoefficients[i];
            return System.Math.Sqrt(sum);
        }
    }
}
