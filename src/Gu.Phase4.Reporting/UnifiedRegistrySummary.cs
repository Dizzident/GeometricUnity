using System.Text.Json.Serialization;

namespace Gu.Phase4.Reporting;

/// <summary>
/// Summary of the unified particle registry (bosons + fermions + interactions).
/// </summary>
public sealed class UnifiedRegistrySummary
{
    /// <summary>Unique summary identifier.</summary>
    [JsonPropertyName("summaryId")]
    public required string SummaryId { get; init; }

    /// <summary>Schema version.</summary>
    [JsonPropertyName("schemaVersion")]
    public string SchemaVersion { get; init; } = "1.0.0";

    /// <summary>Total number of bosonic candidates.</summary>
    [JsonPropertyName("totalBosons")]
    public required int TotalBosons { get; init; }

    /// <summary>Total number of fermionic candidates.</summary>
    [JsonPropertyName("totalFermions")]
    public required int TotalFermions { get; init; }

    /// <summary>Total number of interaction candidates.</summary>
    [JsonPropertyName("totalInteractions")]
    public required int TotalInteractions { get; init; }

    /// <summary>
    /// Counts of candidates at each claim class level (C0-C5).
    /// Key: claim class string (e.g. "C0", "C1"), Value: count.
    /// </summary>
    [JsonPropertyName("claimClassCounts")]
    public required Dictionary<string, int> ClaimClassCounts { get; init; }

    /// <summary>Top candidates ordered by claim class descending, then by mass-like value.</summary>
    [JsonPropertyName("topCandidates")]
    public required List<CandidateParticleSummary> TopCandidates { get; init; }
}

/// <summary>
/// Brief summary of a single unified particle candidate.
/// </summary>
public sealed class CandidateParticleSummary
{
    /// <summary>Candidate identifier (from UnifiedParticleRecord.ParticleId).</summary>
    [JsonPropertyName("candidateId")]
    public required string CandidateId { get; init; }

    /// <summary>Particle type string: "Boson", "Fermion", or "Interaction".</summary>
    [JsonPropertyName("particleType")]
    public required string ParticleType { get; init; }

    /// <summary>Current claim class (C0-C5).</summary>
    [JsonPropertyName("claimClass")]
    public required string ClaimClass { get; init; }

    /// <summary>Mean mass-like value from the mass-like envelope.</summary>
    [JsonPropertyName("massLikeValue")]
    public required double MassLikeValue { get; init; }

    /// <summary>Number of claim class demotions this candidate has received.</summary>
    [JsonPropertyName("demotionCount")]
    public required int DemotionCount { get; init; }
}
