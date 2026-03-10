using System.Text.Json.Serialization;

namespace Gu.Phase3.Observables;

/// <summary>
/// Computes similarity between observed mode signatures using L2 inner product on X_h.
///
/// Wraps ObservedOverlapMetrics to produce a structured OverlapResult
/// containing the overlap matrix and per-pair overlap data.
/// </summary>
public sealed class ObservedOverlapComputer
{
    /// <summary>
    /// Compute overlap between all pairs of observed signatures.
    /// </summary>
    public static OverlapResult Compute(IReadOnlyList<ObservedModeSignature> signatures)
    {
        if (signatures == null) throw new ArgumentNullException(nameof(signatures));

        var matrix = ObservedOverlapMetrics.OverlapMatrix(signatures);
        int n = signatures.Count;

        var modeIds = new string[n];
        for (int i = 0; i < n; i++)
            modeIds[i] = signatures[i].ModeId;

        // Find max off-diagonal overlap
        double maxOffDiag = 0;
        string maxPairA = "", maxPairB = "";
        for (int i = 0; i < n; i++)
        {
            for (int j = i + 1; j < n; j++)
            {
                if (matrix[i, j] > maxOffDiag)
                {
                    maxOffDiag = matrix[i, j];
                    maxPairA = modeIds[i];
                    maxPairB = modeIds[j];
                }
            }
        }

        return new OverlapResult
        {
            ModeIds = modeIds,
            OverlapMatrix = matrix,
            MaxOffDiagonalOverlap = maxOffDiag,
            MaxOverlapPair = n > 1 ? (maxPairA, maxPairB) : null,
        };
    }
}

/// <summary>
/// Result of computing observed-space overlap between modes.
/// </summary>
public sealed class OverlapResult
{
    /// <summary>Mode IDs in order.</summary>
    public required string[] ModeIds { get; init; }

    /// <summary>Overlap matrix (symmetric, diagonal = 1).</summary>
    [JsonIgnore]
    public required double[,] OverlapMatrix { get; init; }

    /// <summary>Maximum off-diagonal overlap value.</summary>
    [JsonPropertyName("maxOffDiagonalOverlap")]
    public required double MaxOffDiagonalOverlap { get; init; }

    /// <summary>Pair of mode IDs with highest off-diagonal overlap (null if fewer than 2 modes).</summary>
    [JsonIgnore]
    public (string A, string B)? MaxOverlapPair { get; init; }
}
