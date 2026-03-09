using System.Text.Json.Serialization;

namespace Gu.Phase2.Canonicity;

/// <summary>
/// Boolean matrix indicating whether branch pairs agree on qualitative classification.
/// Agrees[i,j] is true when Classifications[i] == Classifications[j].
/// </summary>
public sealed class QualitativeClassificationAgreementMatrix
{
    /// <summary>Branch variant IDs labeling rows and columns.</summary>
    [JsonPropertyName("branchIds")]
    public required IReadOnlyList<string> BranchIds { get; init; }

    /// <summary>Per-branch qualitative classification.</summary>
    [JsonPropertyName("classifications")]
    public required IReadOnlyList<QualitativeClass> Classifications { get; init; }

    /// <summary>Agreement matrix: Agrees[i,j] == (Classifications[i] == Classifications[j]).</summary>
    [JsonPropertyName("agrees")]
    public required bool[,] Agrees { get; init; }

    /// <summary>Whether all branches agree on qualitative classification.</summary>
    [JsonIgnore]
    public bool AllAgree
    {
        get
        {
            if (Classifications.Count == 0) return true;
            var first = Classifications[0];
            for (int i = 1; i < Classifications.Count; i++)
                if (Classifications[i] != first)
                    return false;
            return true;
        }
    }
}
