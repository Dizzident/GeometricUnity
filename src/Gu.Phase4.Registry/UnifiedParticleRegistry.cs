using System.Text.Json;
using System.Text.Json.Serialization;
using Gu.Core;
using Gu.Phase3.Registry;
using Gu.Phase4.Couplings;
using Gu.Phase4.FamilyClustering;
using Gu.Phase4.Fermions;

namespace Gu.Phase4.Registry;

/// <summary>
/// The unified particle registry: aggregates bosonic candidates (from Phase III),
/// fermionic mode family clusters (from Phase IV M41), and interaction candidates
/// (from Phase IV M40 coupling proxies) into a single queryable, serializable structure.
///
/// PhysicsNote (M42 spec): This registry emits evidence-bearing candidates, NOT confirmed
/// physical particles. All claim classes carry explicit uncertainty and demotion histories.
///
/// Conservative output convention:
/// - Claim classes follow the Phase III C0-C5 scale.
/// - "C0_NumericalMode" means raw computed candidate.
/// - "C5_StrongIdentificationCandidate" is the highest achievable level and still not proof.
/// - UnverifiedGpu flag always caps claim class at C1.
/// </summary>
public sealed class UnifiedParticleRegistry
{
    /// <summary>Registry version string.</summary>
    [JsonPropertyName("registryVersion")]
    public string RegistryVersion { get; init; } = "1.0.0";

    /// <summary>Schema version.</summary>
    [JsonPropertyName("schemaVersion")]
    public string SchemaVersion { get; init; } = "1.0.0";

    /// <summary>All unified candidate particle records.</summary>
    [JsonPropertyName("candidates")]
    public List<UnifiedParticleRecord> Candidates { get; init; } = new();

    /// <summary>Total number of candidates.</summary>
    [JsonIgnore]
    public int Count => Candidates.Count;

    /// <summary>
    /// Register a candidate record.
    /// </summary>
    public UnifiedParticleRecord Register(UnifiedParticleRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);
        Candidates.Add(record);
        return record;
    }

    /// <summary>
    /// Query candidates by type.
    /// </summary>
    public IReadOnlyList<UnifiedParticleRecord> QueryByType(UnifiedParticleType type)
        => Candidates.Where(c => c.ParticleType == type).ToList();

    /// <summary>
    /// Query candidates at or above the given claim class.
    /// Claim class strings are compared by extracting the leading C-number.
    /// </summary>
    public IReadOnlyList<UnifiedParticleRecord> QueryByMinClaimClass(string minClaimClass)
    {
        int minLevel = ParseClaimClassLevel(minClaimClass);
        return Candidates.Where(c => ParseClaimClassLevel(c.ClaimClass) >= minLevel).ToList();
    }

    /// <summary>
    /// Query by background ID.
    /// </summary>
    public IReadOnlyList<UnifiedParticleRecord> QueryByBackground(string backgroundId)
        => Candidates.Where(c => c.BackgroundSet.Contains(backgroundId)).ToList();

    /// <summary>
    /// Count candidates at or above a given claim class level.
    /// </summary>
    public int CountAboveClass(string minClaimClass)
    {
        int minLevel = ParseClaimClassLevel(minClaimClass);
        return Candidates.Count(c => ParseClaimClassLevel(c.ClaimClass) >= minLevel);
    }

    /// <summary>
    /// Serialize the registry to JSON.
    /// </summary>
    public string ToJson(bool indented = true)
        => JsonSerializer.Serialize(this, new JsonSerializerOptions
        {
            WriteIndented = indented,
            Converters = { new JsonStringEnumConverter() },
        });

    /// <summary>
    /// Deserialize a registry from JSON.
    /// </summary>
    public static UnifiedParticleRegistry FromJson(string json)
        => JsonSerializer.Deserialize<UnifiedParticleRegistry>(json,
            new JsonSerializerOptions { Converters = { new JsonStringEnumConverter() } })
            ?? new UnifiedParticleRegistry();

    // ------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------

    /// <summary>
    /// Parse claim class string to numeric level.
    /// Accepts "C0_NumericalMode", "C0", "0", etc.
    /// Returns the integer level (0-5), or 0 for unrecognized.
    /// </summary>
    public static int ParseClaimClassLevel(string claimClass)
    {
        if (string.IsNullOrEmpty(claimClass)) return 0;
        // Try "C{N}" prefix
        if (claimClass.Length >= 2 && claimClass[0] == 'C' && char.IsDigit(claimClass[1]))
        {
            if (int.TryParse(claimClass.AsSpan(1, 1), out int n))
                return n;
        }
        // Try plain integer
        if (int.TryParse(claimClass, out int level))
            return level;
        return 0;
    }
}
