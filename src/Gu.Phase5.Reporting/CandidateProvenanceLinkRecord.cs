using System.Text.Json.Serialization;

namespace Gu.Phase5.Reporting;

/// <summary>
/// Optional candidate-specific provenance links that enrich registry candidates with
/// branch/background identities from a downstream campaign-specific evidence source.
/// </summary>
public sealed class CandidateProvenanceLinkRecord
{
    /// <summary>Candidate ID in the unified particle registry.</summary>
    [JsonPropertyName("candidateId")]
    public required string CandidateId { get; init; }

    /// <summary>Branch variant IDs linked to this candidate in the current campaign context.</summary>
    [JsonPropertyName("branchVariantIds")]
    public IReadOnlyList<string> BranchVariantIds { get; init; } = Array.Empty<string>();

    /// <summary>Background IDs linked to this candidate in the current campaign context.</summary>
    [JsonPropertyName("backgroundIds")]
    public IReadOnlyList<string> BackgroundIds { get; init; } = Array.Empty<string>();

    /// <summary>Optional notes describing how the links were derived.</summary>
    [JsonPropertyName("notes")]
    public string? Notes { get; init; }
}
