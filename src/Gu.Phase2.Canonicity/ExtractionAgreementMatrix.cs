using System.Text.Json.Serialization;

namespace Gu.Phase2.Canonicity;

/// <summary>
/// Boolean matrix indicating whether branch pairs agree on extraction success status.
/// Agrees[i,j] is true when ExtractionStatuses[i] == ExtractionStatuses[j].
/// </summary>
public sealed class ExtractionAgreementMatrix
{
    /// <summary>Branch variant IDs labeling rows and columns.</summary>
    [JsonPropertyName("branchIds")]
    public required IReadOnlyList<string> BranchIds { get; init; }

    /// <summary>Per-branch extraction success status.</summary>
    [JsonPropertyName("extractionStatuses")]
    public required IReadOnlyList<bool> ExtractionStatuses { get; init; }

    /// <summary>Agreement matrix: Agrees[i,j] == (ExtractionStatuses[i] == ExtractionStatuses[j]).</summary>
    [JsonPropertyName("agrees")]
    public required bool[,] Agrees { get; init; }

    /// <summary>Whether all branches agree on extraction status.</summary>
    [JsonIgnore]
    public bool AllAgree
    {
        get
        {
            if (ExtractionStatuses.Count == 0) return true;
            bool first = ExtractionStatuses[0];
            for (int i = 1; i < ExtractionStatuses.Count; i++)
                if (ExtractionStatuses[i] != first)
                    return false;
            return true;
        }
    }
}
