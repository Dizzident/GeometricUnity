using System.Text.Json.Serialization;

namespace Gu.Phase3.Campaigns;

/// <summary>
/// Target profile for Mode BC2 (external analogy comparison).
///
/// External profiles carry an explicit uncertainty budget for each property,
/// acknowledging that the mapping from GU quantities to physical observables
/// is itself uncertain.
/// </summary>
public sealed class ExternalTargetProfile : TargetProfile
{
    /// <summary>Uncertainty budget for each property.</summary>
    [JsonPropertyName("uncertaintyBudget")]
    public required UncertaintyBudget UncertaintyBudget { get; init; }

    /// <summary>Source description (e.g. "Standard Model photon analogy").</summary>
    [JsonPropertyName("sourceDescription")]
    public required string SourceDescription { get; init; }
}
