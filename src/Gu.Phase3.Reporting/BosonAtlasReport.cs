using System.Text.Json.Serialization;
using Gu.Phase3.Campaigns;
using Gu.Phase3.Registry;

namespace Gu.Phase3.Reporting;

/// <summary>
/// A complete boson atlas report: the top-level output of a Phase III study.
///
/// Per the implementation plan, the report must explicitly state:
/// - which candidates are strong,
/// - which are weak or ambiguous,
/// - which are negative (demoted, incompatible, insufficient evidence),
/// - what remains underdetermined.
///
/// The reporting layer must make overclaiming difficult.
/// </summary>
public sealed class BosonAtlasReport
{
    /// <summary>Unique report identifier.</summary>
    [JsonPropertyName("reportId")]
    public required string ReportId { get; init; }

    /// <summary>Study identifier that produced this report.</summary>
    [JsonPropertyName("studyId")]
    public required string StudyId { get; init; }

    /// <summary>Registry version used for this report.</summary>
    [JsonPropertyName("registryVersion")]
    public required string RegistryVersion { get; init; }

    /// <summary>Spectrum sheets: per-candidate spectral summaries.</summary>
    [JsonPropertyName("spectrumSheets")]
    public required IReadOnlyList<SpectrumSheet> SpectrumSheets { get; init; }

    /// <summary>Stability summaries: per-candidate stability assessments.</summary>
    [JsonPropertyName("stabilitySummaries")]
    public required IReadOnlyList<StabilitySummary> StabilitySummaries { get; init; }

    /// <summary>Ambiguity map entries.</summary>
    [JsonPropertyName("ambiguityEntries")]
    public required IReadOnlyList<AmbiguityMapEntry> AmbiguityEntries { get; init; }

    /// <summary>Negative result summaries.</summary>
    [JsonPropertyName("negativeResults")]
    public required IReadOnlyList<NegativeResultSummary> NegativeResults { get; init; }

    /// <summary>Campaign results included in this report.</summary>
    [JsonPropertyName("campaignResults")]
    public required IReadOnlyList<BosonCampaignResult> CampaignResults { get; init; }

    /// <summary>Total candidates in the registry.</summary>
    [JsonPropertyName("totalCandidates")]
    public int TotalCandidates { get; init; }

    /// <summary>Count of candidates at or above each claim class.</summary>
    [JsonPropertyName("claimClassCounts")]
    public required IReadOnlyDictionary<string, int> ClaimClassCounts { get; init; }

    /// <summary>Timestamp when this report was generated.</summary>
    [JsonPropertyName("generatedAt")]
    public DateTimeOffset GeneratedAt { get; init; }
}
