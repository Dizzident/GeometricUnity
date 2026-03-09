using System.Text.Json.Serialization;

namespace Gu.Phase2.Canonicity;

/// <summary>
/// Boolean matrix indicating whether branch pairs agree on comparison admissibility status.
/// Agrees[i,j] is true when AdmissibilityStatuses[i] == AdmissibilityStatuses[j].
/// </summary>
public sealed class AdmissibilityAgreementMatrix
{
    /// <summary>Branch variant IDs labeling rows and columns.</summary>
    [JsonPropertyName("branchIds")]
    public required IReadOnlyList<string> BranchIds { get; init; }

    /// <summary>Per-branch comparison admissibility status.</summary>
    [JsonPropertyName("admissibilityStatuses")]
    public required IReadOnlyList<bool> AdmissibilityStatuses { get; init; }

    /// <summary>Agreement matrix: Agrees[i,j] == (AdmissibilityStatuses[i] == AdmissibilityStatuses[j]).</summary>
    [JsonPropertyName("agrees")]
    public required bool[,] Agrees { get; init; }

    /// <summary>Whether all branches agree on admissibility status.</summary>
    [JsonIgnore]
    public bool AllAgree
    {
        get
        {
            if (AdmissibilityStatuses.Count == 0) return true;
            bool first = AdmissibilityStatuses[0];
            for (int i = 1; i < AdmissibilityStatuses.Count; i++)
                if (AdmissibilityStatuses[i] != first)
                    return false;
            return true;
        }
    }
}
